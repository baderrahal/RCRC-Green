"""Works out what a git commit is actually going to contain.

Both commit hooks used to read the index and nothing else, which is wrong twice over.
`git commit -a` stages the working tree after a PreToolUse hook has already returned, so a
hook reading the index sees the wrong set. `git commit <path>` ignores the index for
everything except the paths named on the command line. In the first case the hooks passed
content they never looked at, in the second they refused a commit that was fine.

Called as:

    python3 commit-scope.py paths     <the whole bash command>
    python3 commit-scope.py all-paths <the whole bash command>
    python3 commit-scope.py message   <the whole bash command>

`paths` prints NUL separated fields. The first is the mode, index or worktree, which says
where the content of those files is going to be read from. The rest are the paths.
`all-paths` is the same list with deletions kept in, for the territory hook.
`message` prints the commit message text, empty when the command carries none.
"""

import os
import re
import shlex
import subprocess
import sys

# Where one command in the line ends and the next begins. Without this the words of a
# following command read as pathspecs, and a commit chained after a git add looks like a
# commit that names paths.
SHELL_OPERATOR = re.compile(r"\A[&|;()]+\Z")
REDIRECTION = re.compile(r"\A\d*(>>|>|<<|<)(&\d*)?\Z")


def ends_the_command(tokens, at):
    word = tokens[at]
    if SHELL_OPERATOR.match(word) or REDIRECTION.match(word):
        return True

    # shlex with punctuation_chars splits 2>&1 into 2, >&, 1, so the file descriptor arrives
    # as a word of its own and read as a path named 2. Only a redirection claims it. A digit
    # in front of a pipe really is a path.
    return (word.isdigit()
            and at + 1 < len(tokens)
            and REDIRECTION.match(tokens[at + 1]) is not None)

# Short options that swallow a value, either the rest of their own cluster or the next word.
VALUE_TAKING_SHORT = set("mFCct")

VALUE_TAKING_LONG = {
    "--message",
    "--file",
    "--author",
    "--date",
    "--template",
    "--cleanup",
    "--reuse-message",
    "--reedit-message",
    "--fixup",
    "--squash",
    "--pathspec-from-file",
    "--trailer",
}

GIT_VALUE_TAKING_LONG = {"--git-dir", "--work-tree", "--namespace", "--exec-path", "--super-prefix"}


class CommitCall(object):
    def __init__(self):
        self.messages = []
        self.message_files = []
        self.all_flag = False
        self.pathspecs = []
        self.found = False


def find_commit_arguments(tokens):
    """Every git commit on the line, each cut off at the end of its own command."""
    found = []
    at = 0
    while at < len(tokens):
        word = tokens[at]
        if word == "git" or word.endswith("/git"):
            after = at + 1
            while after < len(tokens):
                option = tokens[after]
                if ends_the_command(tokens, after):
                    break
                if option in ("-C", "-c") or option in GIT_VALUE_TAKING_LONG:
                    after += 2
                    continue
                if option.startswith("-"):
                    after += 1
                    continue
                break
            if after < len(tokens) and tokens[after] == "commit":
                end = after + 1
                while end < len(tokens) and not ends_the_command(tokens, end):
                    end += 1
                found.append(tokens[after + 1:end])
                at = end
                continue
        at += 1
    return found


def read_commit_call(command):
    call = CommitCall()
    try:
        # punctuation_chars keeps the shell operators as tokens of their own, so a semicolon
        # glued to the word in front of it still ends the command rather than becoming part
        # of a message or a path.
        lexer = shlex.shlex(command, posix=True, punctuation_chars=True)
        lexer.whitespace_split = True
        tokens = list(lexer)
    except ValueError:
        # An unbalanced quote means the command will not run as written. Say nothing was
        # found rather than guessing, and let the caller fail closed.
        return call

    every = find_commit_arguments(tokens)
    if not every:
        return call
    call.found = True

    for arguments in every:
        read_one_commit(call, arguments)
    return call


def read_one_commit(call, arguments):
    at = 0
    only_paths = False
    while at < len(arguments):
        word = arguments[at]

        if only_paths:
            call.pathspecs.append(word)
            at += 1
            continue

        if word == "--":
            only_paths = True
            at += 1
            continue

        if word.startswith("--"):
            name, sep, inline = word.partition("=")
            if name in VALUE_TAKING_LONG:
                value = inline if sep else None
                if value is None:
                    at += 1
                    value = arguments[at] if at < len(arguments) else ""
                if name == "--message":
                    call.messages.append(value)
                elif name == "--file":
                    call.message_files.append(value)
                at += 1
                continue
            if name == "--all":
                call.all_flag = True
            at += 1
            continue

        if word.startswith("-") and len(word) > 1:
            cluster = word[1:]
            position = 0
            while position < len(cluster):
                letter = cluster[position]
                if letter == "a":
                    call.all_flag = True
                    position += 1
                    continue
                if letter in VALUE_TAKING_SHORT:
                    rest = cluster[position + 1:]
                    if rest:
                        value = rest
                    else:
                        at += 1
                        value = arguments[at] if at < len(arguments) else ""
                    if letter == "m":
                        call.messages.append(value)
                    elif letter == "F":
                        call.message_files.append(value)
                    position = len(cluster)
                    continue
                position += 1
            at += 1
            continue

        call.pathspecs.append(word)
        at += 1


def git(arguments):
    done = subprocess.run(["git"] + arguments, capture_output=True)
    if done.returncode != 0:
        return None
    return done.stdout


def split_paths(raw):
    if raw is None:
        return None
    return [part for part in raw.decode("utf-8", "surrogateescape").split("\0") if part]


def has_head():
    return subprocess.run(
        ["git", "rev-parse", "--verify", "--quiet", "HEAD"], capture_output=True).returncode == 0


def committed_paths(call, diff_filter="ACMR"):
    staged = ["diff", "--cached", "--name-only", "-z", "--diff-filter=" + diff_filter]

    if call.pathspecs:
        if not has_head():
            return "worktree", split_paths(git(staged + ["--"] + call.pathspecs)) or []
        found = split_paths(
            git(["diff", "--name-only", "-z", "HEAD",
                 "--diff-filter=" + diff_filter, "--"] + call.pathspecs))
        return "worktree", found or []

    if call.all_flag:
        if not has_head():
            return "worktree", split_paths(git(staged)) or []
        found = split_paths(
            git(["diff", "--name-only", "-z", "HEAD", "--diff-filter=" + diff_filter]))
        return "worktree", found or []

    return "index", split_paths(git(staged)) or []


def commit_message(call):
    parts = list(call.messages)
    for path in call.message_files:
        if path == "-":
            continue
        try:
            with open(path, encoding="utf-8", errors="replace") as handle:
                parts.append(handle.read())
        except IOError:
            # A message file that cannot be read is reported as such rather than skipped,
            # because skipping it would pass a message nothing looked at.
            parts.append("RCRC_UNREADABLE_MESSAGE_FILE " + path)
    return "\n\n".join(parts)


def main():
    if len(sys.argv) < 3:
        sys.stderr.write("Usage: commit-scope.py paths|message <command>\n")
        return 2

    action = sys.argv[1]
    call = read_commit_call(sys.argv[2])

    if action == "message":
        sys.stdout.write(commit_message(call))
        return 0

    if action == "paths":
        mode, paths = committed_paths(call)
        sys.stdout.write("\0".join([mode] + paths))
        return 0

    if action == "all-paths":
        # Deletions included. The territory hook needs them, because deleting another
        # task's file is as much an edit as changing it. The plain paths action keeps
        # its ACMR filter: its two callers read file content, which a deletion has
        # none of, and one of them counts a deleted file as present, the wrong way.
        mode, paths = committed_paths(call, "ACMRD")
        sys.stdout.write("\0".join([mode] + paths))
        return 0

    sys.stderr.write("Unknown action " + action + "\n")
    return 2


if __name__ == "__main__":
    sys.exit(main())
