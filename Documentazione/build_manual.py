#!/usr/bin/env python3
"""Assembla Documentazione/Manuale/*.md in un'unica pagina HTML offline (Documentazione/Manuale.html).
Convertitore Markdown minimale: titoli, paragrafi, elenchi (anche annidati), tabelle, citazioni,
blocchi di codice, grassetto/corsivo/codice in linea, link. Nessuna dipendenza esterna."""
import re, glob, os, html, sys

ROOT = '/percorso/progetto/src/Ultima-Adventures/Documentazione'
SRC = os.path.join(ROOT, 'Manuale')
OUT = os.path.join(ROOT, 'Manuale.html')

def slug(t):
    t = re.sub(r'<[^>]+>', '', t).lower()
    t = re.sub(r'[^\w\s-]', '', t, flags=re.U)
    return re.sub(r'\s+', '-', t.strip())

def inline(s, links):
    s = html.escape(s, quote=False)
    codes = []
    def keep(m):
        codes.append('<code>' + m.group(1) + '</code>'); return '\x00%d\x00' % (len(codes) - 1)
    s = re.sub(r'`([^`]+)`', keep, s)
    s = re.sub(r'\*\*(.+?)\*\*', r'<strong>\1</strong>', s)
    s = re.sub(r'(?<![\w*])\*(?!\s)(.+?)(?<!\s)\*(?![\w*])', r'<em>\1</em>', s)
    def link(m):
        text, href = m.group(1), m.group(2)
        if href.endswith('.md') or '.md#' in href:
            anchor = href.split('#', 1)[1] if '#' in href else 'cap-' + os.path.basename(href.split('#')[0])[:-3]
            href = '#' + anchor
        return '<a href="%s">%s</a>' % (html.escape(href, quote=True), text)
    s = re.sub(r'\[([^\]]+)\]\(([^)\s]+)\)', link, s)
    s = re.sub(r'\x00(\d+)\x00', lambda m: codes[int(m.group(1))], s)
    return s

def convert(md, chapter_id, toc):
    lines = md.replace('\r\n', '\n').split('\n')
    out = []; i = 0; first_h = True
    def flush_par(buf):
        if buf: out.append('<p>' + inline(' '.join(x.strip() for x in buf), None) + '</p>')
    par = []
    while i < len(lines):
        l = lines[i]
        if l.strip().startswith('```'):
            flush_par(par); par = []
            j = i + 1; code = []
            while j < len(lines) and not lines[j].strip().startswith('```'):
                code.append(lines[j]); j += 1
            out.append('<pre><code>' + html.escape('\n'.join(code)) + '</code></pre>')
            i = j + 1; continue
        m = re.match(r'^(#{1,6})\s+(.*)$', l)
        if m:
            flush_par(par); par = []
            lvl = len(m.group(1)); txt = m.group(2).strip()
            hid = chapter_id if (first_h and lvl == 1) else slug(txt)
            first_h = False
            out.append('<h%d id="%s">%s</h%d>' % (lvl, hid, inline(txt, None), lvl))
            if lvl <= 2: toc.append((lvl, hid, re.sub(r'[*`]', '', txt)))
            i += 1; continue
        if re.match(r'^\s*(---|\*\*\*)\s*$', l):
            flush_par(par); par = []; out.append('<hr>'); i += 1; continue
        if l.lstrip().startswith('|') and i + 1 < len(lines) and re.match(r'^\s*\|?\s*:?-{2,}', lines[i + 1]):
            flush_par(par); par = []
            def cells(r):
                r = r.strip()
                if r.startswith('|'): r = r[1:]
                if r.endswith('|'): r = r[:-1]
                return [c.strip() for c in re.split(r'(?<!\\)\|', r)]
            head = cells(l); i += 2; rows = []
            while i < len(lines) and lines[i].lstrip().startswith('|'):
                rows.append(cells(lines[i])); i += 1
            t = ['<div class="tw"><table><thead><tr>'] + ['<th>%s</th>' % inline(c, None) for c in head] + ['</tr></thead><tbody>']
            for r in rows:
                t.append('<tr>' + ''.join('<td>%s</td>' % inline(c.replace('\\|', '|'), None) for c in r) + '</tr>')
            t.append('</tbody></table></div>')
            out.append(''.join(t)); continue
        if l.lstrip().startswith('>'):
            flush_par(par); par = []
            q = []
            while i < len(lines) and lines[i].lstrip().startswith('>'):
                q.append(re.sub(r'^\s*>\s?', '', lines[i])); i += 1
            inner = []
            convert_block = convert('\n'.join(q), None, [])
            out.append('<blockquote>' + convert_block + '</blockquote>'); continue
        if re.match(r'^\s*([-*+]|\d+[.)])\s+', l):
            flush_par(par); par = []
            stack = []  # (indent, tag)
            while i < len(lines) and (re.match(r'^\s*([-*+]|\d+[.)])\s+', lines[i]) or (lines[i].startswith('   ') and lines[i].strip() and stack)):
                cur = lines[i]
                m2 = re.match(r'^(\s*)([-*+]|\d+[.)])\s+(.*)$', cur)
                if not m2:
                    out[-1] = out[-1][:-5] + ' ' + inline(cur.strip(), None) + '</li>'
                    i += 1; continue
                ind = len(m2.group(1)); tag = 'ol' if m2.group(2)[0].isdigit() else 'ul'
                if not stack or ind > stack[-1][0]:
                    out.append('<%s>' % tag); stack.append((ind, tag))
                else:
                    while stack and ind < stack[-1][0]:
                        out.append('</%s>' % stack.pop()[1])
                out.append('<li>' + inline(m2.group(3), None) + '</li>')
                i += 1
            while stack: out.append('</%s>' % stack.pop()[1])
            continue
        if not l.strip():
            flush_par(par); par = []; i += 1; continue
        if re.match(r'^\s*<!--.*-->\s*$', l):
            i += 1; continue
        par.append(l); i += 1
    flush_par(par)
    return '\n'.join(out)

def main():
    files = sorted(glob.glob(os.path.join(SRC, '*.md')), key=lambda f: (os.path.basename(f)[:2], os.path.basename(f)[2:3] != '-', os.path.basename(f)))
    toc = []; body = []
    for f in files:
        cid = 'cap-' + os.path.basename(f)[:-3]
        body.append('<section>' + convert(open(f, encoding='utf-8').read(), cid, toc) + '</section>')
    nav = ['<ul>']
    for lvl, hid, txt in toc:
        if lvl == 1: nav.append('<li class="c"><a href="#%s">%s</a></li>' % (hid, html.escape(txt)))
    nav.append('</ul>')
    page = TEMPLATE.replace('%NAV%', '\n'.join(nav)).replace('%BODY%', '\n'.join(body))
    open(OUT, 'w', encoding='utf-8').write(page)
    print('scritto', OUT, len(page) // 1024, 'KB,', len(files), 'capitoli')

TEMPLATE = '''<!doctype html>
<html lang="it"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Manuale di Ultima Adventures</title>
<style>
:root{--bg:#f7f3ea;--fg:#2a2420;--muted:#6b6058;--accent:#8a3b12;--line:#d9cfbf;--card:#fffdf8;--code:#efe7d9}
@media (prefers-color-scheme:dark){:root{--bg:#1b1815;--fg:#e8e0d4;--muted:#a89c8e;--accent:#e0915a;--line:#3a332c;--card:#231f1b;--code:#2e2822}}
*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--fg);font:16px/1.6 Georgia,"Times New Roman",serif}
nav{position:fixed;top:0;left:0;bottom:0;width:270px;overflow:auto;padding:20px 14px;border-right:1px solid var(--line);background:var(--card)}
nav h2{font-size:18px;margin:0 0 10px;color:var(--accent)}nav ul{list-style:none;padding:0;margin:0}nav li{margin:2px 0}
nav a{color:var(--fg);text-decoration:none;font-size:14px}nav a:hover{color:var(--accent)}
#q{width:100%;padding:6px 8px;margin:0 0 12px;border:1px solid var(--line);border-radius:6px;background:var(--bg);color:var(--fg)}
main{margin-left:270px;padding:24px 40px;max-width:1100px}
h1{color:var(--accent);border-bottom:2px solid var(--line);padding-bottom:6px;margin-top:48px}h2{margin-top:32px;color:var(--accent)}
h3{margin-top:24px}a{color:var(--accent)}
.tw{overflow-x:auto;margin:12px 0}table{border-collapse:collapse;font-size:14px;background:var(--card)}
th,td{border:1px solid var(--line);padding:4px 8px;text-align:left;vertical-align:top}th{background:var(--code)}
code{background:var(--code);padding:1px 4px;border-radius:3px;font-size:90%}pre{background:var(--code);padding:10px;overflow:auto}
blockquote{margin:12px 0;padding:8px 14px;border-left:4px solid var(--accent);background:var(--card)}blockquote p{margin:4px 0}
hr{border:0;border-top:1px solid var(--line);margin:24px 0}
@media (max-width:800px){nav{position:static;width:auto;border-right:0;border-bottom:1px solid var(--line)}main{margin:0;padding:16px}}
@media print{nav{display:none}main{margin:0}}
</style></head><body>
<nav><h2>Manuale di Ultima Adventures</h2><input id="q" placeholder="Cerca nel manuale..." oninput="cerca(this.value)">%NAV%</nav>
<main>%BODY%</main>
<script>
function cerca(v){v=v.trim().toLowerCase();document.querySelectorAll('main section > *').forEach(function(e){e.style.display=(!v||e.textContent.toLowerCase().indexOf(v)>=0||/^H[12]$/.test(e.tagName))?'':'none'})}
</script>
</body></html>'''

if __name__ == '__main__':
    main()
