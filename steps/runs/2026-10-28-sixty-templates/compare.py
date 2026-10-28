# Compares KPI template workbooks cell by cell, every sheet, value and formula, straight out
# of the zip. The first path is the base and every other path is compared with it. It prints
# client cell text, so its output goes under reports/, which is never committed.
import sys, zipfile, re
import xml.etree.ElementTree as ET

REL = '{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id'


def local(tag):
    return tag.split('}')[-1]


def read(path):
    z = zipfile.ZipFile(path)
    shared = []
    if 'xl/sharedStrings.xml' in z.namelist():
        root = ET.fromstring(z.read('xl/sharedStrings.xml'))
        for si in root:
            shared.append(''.join(t.text or '' for t in si.iter() if local(t.tag) == 't'))
    wb = ET.fromstring(z.read('xl/workbook.xml'))
    rels = ET.fromstring(z.read('xl/_rels/workbook.xml.rels'))
    target = {r.get('Id'): r.get('Target') for r in rels}
    sheets = []
    for s in wb.iter():
        if local(s.tag) == 'sheet':
            t = target[s.get(REL)].lstrip('/')
            if not t.startswith('xl/'):
                t = 'xl/' + t
            sheets.append((s.get('name'), t))
    out = {}
    for name, part in sheets:
        root = ET.fromstring(z.read(part))
        cells = {}
        masters = {}
        for c in root.iter():
            if local(c.tag) != 'c':
                continue
            typ = c.get('t') or 'n'
            f = v = None
            for ch in c:
                if local(ch.tag) == 'f':
                    if ch.get('t') == 'shared' and ch.text:
                        masters[ch.get('si')] = (c.get('r'), ch.text)
                    f = ch.text or ('shared:' + (ch.get('si') or ''))
                elif local(ch.tag) == 'v':
                    v = ch.text
                elif local(ch.tag) == 'is':
                    v = ''.join(t.text or '' for t in ch.iter() if local(t.tag) == 't')
            if typ == 's' and v is not None:
                v = shared[int(v)]
            if f is None and (v is None or v == ''):
                continue
            cells[c.get('r')] = (typ, f, v)
        for ref, (typ, f, v) in list(cells.items()):
            if f and f.startswith('shared:'):
                master = masters.get(f[7:])
                if master:
                    cells[ref] = (typ, shifted(master[1], master[0], ref), v)
        out[name] = cells
    return [n for n, _ in sheets], out


def split(ref):
    m = re.match(r'([A-Z]+)(\d+)', ref)
    col = 0
    for ch in m.group(1):
        col = col * 26 + ord(ch) - 64
    return col, int(m.group(2))


def letters(col):
    out = ''
    while col:
        col, r = divmod(col - 1, 26)
        out = chr(65 + r) + out
    return out


# A shared formula is stored once on its master cell and every other cell of the group
# carries only its index, so the text a cell really computes is the master's moved by the
# offset, the unanchored parts only.
def shifted(formula, master, ref):
    mc, mr = split(master)
    rc, rr = split(ref)
    dc, dr = rc - mc, rr - mr

    def move(m):
        c = m.group(2) if m.group(1) else letters(split(m.group(2) + '1')[0] + dc)
        r = m.group(4) if m.group(3) else str(int(m.group(4)) + dr)
        return m.group(1) + c + m.group(3) + r

    return re.sub(r'(?<![A-Za-z0-9_])(\$?)([A-Z]{1,3})(\$?)(\d+)(?![0-9(])', move, formula)


def order(ref):
    return (int(re.sub(r'\D', '', ref)), len(ref), ref)


def compare(a, b):
    na, ca = read(a)
    nb, cb = read(b)
    print('BASE ', a.split('DD_')[-1])
    print('OTHER', b.split('DD_')[-1])
    print('  sheets', na)
    print('  sheet names the same and in the same order:', na == nb)
    total = 0
    for name in na:
        if name not in cb:
            print('  sheet missing from OTHER:', name)
            continue
        x, y = ca[name], cb[name]
        for ref in sorted(set(x) | set(y), key=order):
            if x.get(ref) != y.get(ref):
                total += 1
                print('  DIFF', name, ref, 'BASE', x.get(ref), 'OTHER', y.get(ref))
    print('  cells differing over every sheet:', total)
    print()


if __name__ == '__main__':
    for other in sys.argv[2:]:
        compare(sys.argv[1], other)
