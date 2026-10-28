"""The records split of 2026-10-28, kept so it can be run again and checked.

Reads every original off the blob at 4b244b3, the main this round was cut from, never off the
working copy, so a Windows checkout's line endings cannot reach a part and a rerun after the
merge splits the originals again rather than the parts. It writes the parts, the index and the
proof, and refuses with an assertion when any join differs from its original or any heading
count moves.

It overwrites what it writes, so it runs only in a throwaway worktree at 4b244b3, never in a
working copy holding later work, and it refuses anywhere else:

    git worktree add --detach <somewhere> 4b244b3
    copy this script there and run it from that folder
"""
import hashlib, math, os, re, subprocess, sys

DAY = "2026-10-28"
PROOF = "steps/runs/2026-10-28-records-split/split_proof.txt"
# CHOSEN. A rules part a helper reads is at most 30 KB and a history part at most 100 KB, the
# version 4 caps. What stays of a log is at most 30 KB, so a session that greps it stays cheap.
ORIGIN = "4b244b3"
RULE_PART_CAP = 30000
LOG_KEEP_CAP = 30000
HISTORY_PART_CAP = 100000


HEAD = subprocess.run(["git", "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
if not HEAD.startswith(ORIGIN):
    sys.exit("Refused. HEAD is " + (HEAD or "unreadable") + ", not " + ORIGIN + ". Run this in a "
             "throwaway worktree at " + ORIGIN + ", because it overwrites every file it writes.")


def blob(path):
    return subprocess.run(["git", "show", ORIGIN + ":" + path], capture_output=True, check=True).stdout


def sha(b):
    return hashlib.sha256(b).hexdigest()


def starts_of(text, pattern):
    return [m.start() for m in re.finditer(pattern, text, re.M)]


def write(path, data):
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "wb") as f:
        f.write(data)


def read(path):
    with open(path, "rb") as f:
        return f.read()


def heading_count(b):
    return len(re.findall(rb"^#", b, re.M))


index_lines = []
proof = []

RULES_POINTER = (
    "The rest of these rules moved, byte for byte, to `docs/rules/` on " + DAY + ". "
    "`docs/rules/index.md` holds one line per rule id and the file it lives in. Read a rule "
    "there by its id, and only its part. A new rule goes into the part for its topic, with its "
    "line in the index.\n").encode()


def rules_units(src, letter, sub_split):
    t = blob(src)
    starts = starts_of(t, rb"^## ")
    bounds = starts + [len(t)]
    units = [(letter + "00", "the opening lines of the file", t[0:starts[0]])]
    for i, s in enumerate(starts):
        body = t[s:bounds[i + 1]]
        title = body[3:body.index(b"\n")].decode()
        uid = "%s%02d" % (letter, i + 1)
        if i + 1 == sub_split:
            # One rule is 66 KB on its own, so it splits at its own ### headings.
            subs = starts_of(body, rb"^### ")
            sb = subs + [len(body)]
            units.append((uid, title, body[0:subs[0]]))
            for j, ss in enumerate(subs):
                part = body[ss:sb[j + 1]]
                units.append(("%s.%02d" % (uid, j + 1), part[4:part.index(b"\n")].decode(), part))
        else:
            units.append((uid, title, body))
    assert b"".join(u[2] for u in units) == t
    return t, units


def place(src, letter, topics, keep, prefix, sub_split=None, sub_topic=None, note=b""):
    """topics maps a topic to its 1-based section numbers. keep is what stays in .claude/rules."""
    t, units = rules_units(src, letter, sub_split)
    where = {}
    for uid, _, _ in units:
        top = int(uid[1:3])
        if top == 0 or top in keep:
            where[uid] = None
        elif top == sub_split:
            where[uid] = sub_topic
        else:
            hits = [n for n, nums in topics.items() if top in nums]
            assert len(hits) == 1, (src, uid, hits)
            where[uid] = hits[0]

    files = {}
    kept = b"".join(u[2] for u in units if where[u[0]] is None)
    sep = b"" if kept.endswith(b"\n\n") else b"\n"
    files[src] = kept + sep + RULES_POINTER + note
    file_of = {u[0]: src for u in units if where[u[0]] is None}

    for name in sorted(set(v for v in where.values() if v)):
        mine = [u for u in units if where[u[0]] == name]
        total = sum(len(u[2]) for u in mine)
        n = math.ceil(total / RULE_PART_CAP)
        target = total / n
        parts, cur = [], []
        for u in mine:
            size = sum(len(x[2]) for x in cur)
            if cur and (size + len(u[2]) > RULE_PART_CAP or (size >= target and len(parts) < n - 1)):
                parts.append(cur)
                cur = []
            cur.append(u)
        parts.append(cur)
        for k, p in enumerate(parts):
            path = "docs/rules/%s-%s%s.md" % (prefix, name, "" if len(parts) == 1 else "-%d" % (k + 1))
            data = b"".join(u[2] for u in p)
            assert len(data) <= RULE_PART_CAP, (path, len(data))
            files[path] = data
            for u in p:
                file_of[u[0]] = path

    for uid, title, _ in units:
        index_lines.append("- %s `%s` %s" % (uid, file_of[uid], title))
    for path, data in files.items():
        write(path, data)

    # The proof reads every file back off disk. Each must be exactly its own units in index
    # order, the kept file followed by its pointer, so nothing is lost, doubled or reordered.
    for path in files:
        mine = b"".join(body for uid, _, body in units if file_of[uid] == path)
        if path == src:
            mine += sep + RULES_POINTER + note
        assert read(path) == mine, path
    rebuilt = b"".join(body for _, _, body in units)
    after = sum(heading_count(read(p)) for p in files)
    lines = ["%s on main: %d bytes, SHA-256 %s" % (src, len(t), sha(t))]
    for path in files:
        lines.append("  %-44s %7d bytes" % (path, len(read(path))))
    lines.append("  %d units, %d index lines" % (len(units), len(units)))
    lines.append("  every file read back is exactly its units in index order")
    lines.append("  units joined in index order: %d bytes, SHA-256 %s, %s" % (
        len(rebuilt), sha(rebuilt), "MATCH" if rebuilt == t else "DIFFERS"))
    lines.append("  headings before %d, after %d, %s" % (
        heading_count(t), after, "EQUAL" if heading_count(t) == after else "DIFFER"))
    assert rebuilt == t and heading_count(t) == after
    proof.append("\n".join(lines))


KPI_TOPICS = {
    "plots": [2, 3, 4, 10, 13, 21, 28, 30, 42, 43],
    "workbook": [5, 6, 7, 8, 9, 14, 15, 20, 54, 55, 56, 66],
    "report": [11, 12, 16, 18, 57],
    "reading": [17, 26, 27, 31, 32, 33, 34, 59, 65],
    "species": [36, 37, 38],
    "scan": [39, 41, 44, 45, 67],
    "measured": [58, 60, 61, 62, 63, 64],
    "pane": [22, 23, 29, 48, 49, 50, 51, 52, 68, 69, 70, 71],
}
KPI_KEEP = [1, 19, 24, 25, 40, 46, 47, 53]

CORE_TOPICS = {
    "views-and-grid": [2, 4, 5, 12, 14, 17, 22, 24, 25, 26],
    "sheets": [8, 9, 10, 15, 18, 19, 20, 21, 23],
}
CORE_KEEP = [1, 3, 6, 7, 11, 13, 16, 27]

REVIT_TOPICS = {
    "panel": [5, 6, 7, 8, 13, 22, 23, 25],
    "run": [14, 15, 17, 18, 19, 21, 24],
}
REVIT_KEEP = [1, 2, 3, 4, 9, 10, 11, 12, 16, 20, 26]

place(".claude/rules/kpi-rules.md", "K", KPI_TOPICS, KPI_KEEP, "kpi", sub_split=35, sub_topic="pdf",
      note=b"K25 above says under the section above. That section is K17, The client's note holds, "
           b"and the index names its part.\n")
place(".claude/rules/core-rules.md", "C", CORE_TOPICS, CORE_KEEP, "core")
place(".claude/rules/revit-commands.md", "R", REVIT_TOPICS, REVIT_KEEP, "revit")

# CLAUDE.md: three whole sections out, one line in the place of each.
t = blob("CLAUDE.md")
starts = starts_of(t, rb"^## ")
bounds = starts + [len(t)]
sections = {t[s:t.index(b"\n", s)].decode()[3:]: (s, bounds[i + 1]) for i, s in enumerate(starts)}
# Without a heading of their own the three pointers read as part of the rule above them. It is
# the one heading the split adds, and the count below takes it off by name.
MOVED_HEADING = "## Three sections that live in docs\n\n"
MOVES = [
    ("Project facts", "docs/rules/project-facts.md", "M07",
     MOVED_HEADING + "**Project facts**, from the team and from real models, are in `docs/rules/project-facts.md`.\n\n"),
    ("Hooks", "docs/hooks.md", "M08",
     "**Hooks**: four walls wired in `.claude/settings.json`, described in `docs/hooks.md`. Run "
     "`bash .claude/hooks/hook-tests.sh` after any change to one.\n\n"),
    ("Things that have gone wrong before", "docs/gone-wrong.md", "M09",
     "**Things that have gone wrong before** are in `docs/gone-wrong.md`. Add to it whenever "
     "something breaks.\n\n"),
]
# Two lines left in CLAUDE.md named kpi-rules.md for measurements that moved, so they are
# repointed. They are the only words of the kept text that change, and the proof undoes them.
REPOINTS = [
    (b"measured fact is in `.claude/rules/kpi-rules.md`.",
     b"measured fact is in the KPI rules, by id in `docs/rules/index.md`."),
    (b"The KPI rules file holds the measurement.",
     b"The KPI rules hold the measurement, K35.09 in `docs/rules/index.md`."),
]
new = t
for title, path, uid, line in reversed(MOVES):
    s, e = sections[title]
    write(path, t[s:e])
    new = new[:s] + line.encode() + new[e:]
for old, repointed in REPOINTS:
    assert new.count(old) == 1, old
    new = new.replace(old, repointed)
write("CLAUDE.md", new)
rebuilt = read("CLAUDE.md")
for old, repointed in REPOINTS:
    assert rebuilt.count(repointed) == 1, repointed
    rebuilt = rebuilt.replace(repointed, old)
for title, path, uid, line in MOVES:
    assert rebuilt.count(line.encode()) == 1
    rebuilt = rebuilt.replace(line.encode(), read(path), 1)
after = heading_count(read("CLAUDE.md")) - 1 + sum(heading_count(read(m[1])) for m in MOVES)
lines = ["CLAUDE.md on main: %d bytes, SHA-256 %s" % (len(t), sha(t)),
         "  %-44s %7d bytes" % ("CLAUDE.md", len(read("CLAUDE.md")))]
for m in MOVES:
    lines.append("  %-44s %7d bytes" % (m[1], len(read(m[1]))))
for old, repointed in REPOINTS:
    lines.append("  repointed: %s" % repointed.decode())
lines.append("  each repoint undone and each pointer line put back by its file: %d bytes, SHA-256 %s, %s" % (
    len(rebuilt), sha(rebuilt), "MATCH" if rebuilt == t else "DIFFERS"))
lines.append("  headings before %d, after %d without the one added over the pointers, %s" % (
    heading_count(t), after, "EQUAL" if heading_count(t) == after else "DIFFER"))
assert rebuilt == t and heading_count(t) == after
proof.append("\n".join(lines))
for title, path, uid, line in MOVES:
    index_lines.append("- %s `%s` %s, a whole section of `CLAUDE.md`" % (uid, path, title))

INDEX_HEAD = (
    "# Rules index\n\n"
    "One line per rule id and the file it lives in, in the order the rules first stood. K is\n"
    "`.claude/rules/kpi-rules.md` as it was, C is `core-rules.md`, R is `revit-commands.md` and M is\n"
    "`CLAUDE.md`. An id with a dot is a `###` part of one long rule, and 00 is a file's opening\n"
    "lines. Every part was moved byte for byte on " + DAY + ", and the proof is\n"
    "`" + PROOF + "`. Look a rule up here, then read only its part.\n\n")
write("docs/rules/index.md", (INDEX_HEAD + "\n".join(index_lines) + "\n").encode())


def split_log(src, pattern, name):
    """The newest entries stay. The older ones go whole to steps/history/, newest first."""
    t = blob(src)
    starts = starts_of(t, pattern)
    header = t[:starts[0]]
    bounds = starts + [len(t)]
    entries = [t[s:bounds[i + 1]] for i, s in enumerate(starts)]
    assert max(len(e) for e in entries) <= HISTORY_PART_CAP
    keep, size = [], len(header)
    for e in entries:
        if keep and size + len(e) > LOG_KEEP_CAP:
            break
        keep.append(e)
        size += len(e)
    parts, cur = [], []
    for e in entries[len(keep):]:
        if cur and sum(map(len, cur)) + len(e) > HISTORY_PART_CAP:
            parts.append(cur)
            cur = []
        cur.append(e)
    parts.append(cur)
    paths = ["steps/history/%s-%d.md" % (name, k + 1) for k in range(len(parts))]
    kept = header + b"".join(keep)
    tail = (b"" if kept.endswith(b"\n\n") else b"\n") + (
        "Older entries were moved whole, byte for byte, on %s: `%s` to `%s`, newest first.\n"
        % (DAY, paths[0], paths[-1])).encode()
    write(src, kept + tail)
    for p, part in zip(paths, parts):
        write(p, b"".join(part))

    back = read(src)
    assert back.endswith(tail)
    joined = back[:len(back) - len(tail)] + b"".join(read(p) for p in paths)
    count = lambda b: len(re.findall(pattern, b, re.M))
    after = count(back) + sum(count(read(p)) for p in paths)
    lines = ["%s on main: %d bytes, SHA-256 %s" % (src, len(t), sha(t)),
             "  %-44s %7d bytes, %d entries kept, its pointer line left out of the join" % (
                 src, len(back), len(keep))]
    for p, part in zip(paths, parts):
        lines.append("  %-44s %7d bytes, %d entries" % (p, len(read(p)), len(part)))
    lines.append("  joined in order: %d bytes, SHA-256 %s, %s" % (
        len(joined), sha(joined), "MATCH" if joined == t else "DIFFERS"))
    lines.append("  entry headings before %d, after %d, %s" % (
        count(t), after, "EQUAL" if count(t) == after else "DIFFER"))
    assert joined == t and count(t) == after
    proof.append("\n".join(lines))


split_log("steps/log-kpi.md", rb"^## ", "log-kpi")
split_log("steps/log-drawing.md", rb"^## ", "log-drawing")
split_log("steps/ai-max-state-kpi.md", rb"^Phase:", "ai-max-state-kpi")

write(PROOF, ("Split proof, " + DAY + ". Every original is the blob on main at 4b244b3, read with git\n"
              "show, and every part is read back off disk for the join.\n\n"
              + "\n\n".join(proof) + "\n").encode())
print("\n\n".join(proof))
