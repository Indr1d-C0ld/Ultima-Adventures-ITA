#!/usr/bin/env python3
"""Genera Documentazione/Manuale.pdf (A4, stampabile) dai capitoli Markdown di Documentazione/Manuale.
Impaginazione: copertina, indice con numeri di pagina (due passate), ogni capitolo su pagina nuova,
titoli mai isolati a fondo pagina, tabelle con intestazione ripetuta e righe mai spezzate,
tabelle brevi, riquadri e voci di elenco mai divisi fra due pagine.
Richiede: chromium, node (>= 22), pdftotext (poppler)."""
import os, re, glob, html, subprocess, sys, tempfile, unicodedata
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import build_manual as bm

ROOT = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(ROOT, 'Manuale')
OUT = os.path.join(ROOT, 'Manuale.pdf')
COPERTINA = os.path.join(ROOT, '..', 'docs', 'immagini', '09-introduzione.png')

CSS = r'''
@page { size: A4; margin: 16mm 15mm 18mm 15mm; }
* { box-sizing: border-box; }
html { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
body { margin: 0; font-family: 'DejaVu Serif', 'Liberation Serif', serif; font-size: 9.6pt; line-height: 1.42; color: #1d1a17; }
h1, h2, h3, h4 { font-family: 'DejaVu Serif', serif; color: #6b2a0c; break-after: avoid; page-break-after: avoid; break-inside: avoid; }
h1 { font-size: 20pt; margin: 0 0 6mm; padding-bottom: 2mm; border-bottom: 1.5pt solid #b98a5a; break-before: page; page-break-before: always; }
h2 { font-size: 13.5pt; margin: 6mm 0 2mm; }
h3 { font-size: 11pt; margin: 4mm 0 1.5mm; color: #3d2a1a; }
h4 { font-size: 10pt; margin: 3mm 0 1mm; }
/* un titolo non resta mai da solo a fondo pagina: lo si lega al blocco che segue */
h2 + *, h3 + *, h4 + * { break-before: avoid; page-break-before: avoid; }
p { margin: 0 0 2.2mm; orphans: 3; widows: 3; text-align: justify; hyphens: auto; }
ul, ol { margin: 0 0 2.5mm; padding-left: 6mm; }
li { margin: 0 0 1mm; break-inside: avoid; page-break-inside: avoid; }
blockquote { margin: 2.5mm 0; padding: 2mm 4mm; border-left: 2.5pt solid #b98a5a; background: #f6efe4; break-inside: avoid; page-break-inside: avoid; }
blockquote p { margin: 0.8mm 0; text-align: left; }
code { font-family: 'DejaVu Sans Mono', monospace; font-size: 8.4pt; background: #efe7d9; padding: 0 1mm; border-radius: 1mm; }
pre { font-size: 8pt; background: #efe7d9; padding: 2mm; white-space: pre-wrap; break-inside: avoid; }
a { color: inherit; text-decoration: none; }
hr { border: 0; border-top: 0.6pt solid #d9cfbf; margin: 4mm 0; }
.tw { margin: 2mm 0 3.5mm; }
.tw.breve { break-inside: avoid; page-break-inside: avoid; }
table { width: 100%; border-collapse: collapse; font-size: 8.2pt; line-height: 1.3; }
thead { display: table-header-group; }
tr { break-inside: avoid; page-break-inside: avoid; }
th { background: #e9dcc6; color: #3d2a1a; text-align: left; font-weight: bold; }
th, td { border: 0.5pt solid #c9b89f; padding: 1mm 1.6mm; vertical-align: top; overflow-wrap: break-word; hyphens: manual; }
tbody tr:nth-child(even) td { background: #faf6ef; }
section { }
/* copertina */
.copertina { height: 296mm; width: 210mm; position: relative; background: #17120e; color: #f3e6cf; break-after: page; page-break-after: always; overflow: hidden; }
.copertina img { position: absolute; top: 0; left: 0; width: 210mm; height: 175mm; object-fit: cover; opacity: .92; }
.copertina .velo { position: absolute; top: 120mm; left: 0; width: 210mm; height: 60mm; background: linear-gradient(to bottom, rgba(23,18,14,0), #17120e); }
.copertina .testo { position: absolute; top: 185mm; left: 22mm; right: 22mm; }
.copertina h1 { all: unset; display: block; font-size: 34pt; color: #f0c27a; font-family: 'DejaVu Serif', serif; line-height: 1.1; }
.copertina .sotto { font-size: 15pt; margin-top: 5mm; color: #e9d9bd; }
.copertina .nota { position: absolute; bottom: 18mm; left: 22mm; right: 22mm; font-size: 9pt; color: #b9a78c; border-top: .5pt solid #5a4a38; padding-top: 3mm; }
/* indice */
.indice { break-after: page; page-break-after: always; }
.indice h1 { break-before: auto; page-break-before: auto; }
.indice ol { list-style: none; padding: 0; font-size: 11pt; }
.indice li { display: flex; align-items: baseline; margin: 0 0 2.4mm; }
.indice li .t { flex: 0 1 auto; }
.indice li .d { flex: 1 1 auto; border-bottom: 1pt dotted #9c8a72; margin: 0 2mm; transform: translateY(-1mm); }
.indice li .p { flex: 0 0 auto; font-weight: bold; color: #6b2a0c; }
.indice li.parte { margin-top: 4mm; font-weight: bold; }
'''

def norm(s):
    s = unicodedata.normalize('NFKD', s)
    s = ''.join(c for c in s if not unicodedata.combining(c))
    return re.sub(r'[^a-z0-9]', '', s.lower())

def capitoli():
    files = sorted(glob.glob(os.path.join(SRC, '*.md')),
                   key=lambda f: (os.path.basename(f)[:2], os.path.basename(f)[2:3] != '-', os.path.basename(f)))
    out = []
    for f in files:
        md = open(f, encoding='utf-8').read()
        title = re.search(r'^#\s+(.+)$', md, re.M).group(1).strip()
        out.append((f, title, md))
    return out

def segna_tabelle_brevi(h):
    def fix(m):
        rows = m.group(0).count('<tr>')
        return m.group(0).replace('<div class="tw">', '<div class="tw breve">', 1) if rows <= 25 else m.group(0)
    return re.sub(r'<div class="tw">.*?</table></div>', fix, h, flags=re.S)

def pagina(chap, pagine):
    toc = []
    for i, (f, title, _) in enumerate(chap):
        p = pagine.get(i, '')
        cls = ' class="parte"' if i == 0 else ''
        toc.append(f'<li{cls}><span class="t">{html.escape(re.sub(r"[*`]", "", title))}</span><span class="d"></span><span class="p">{p}</span></li>')
    body = []
    for f, title, md in chap:
        cid = 'cap-' + os.path.basename(f)[:-3]
        body.append('<section>' + segna_tabelle_brevi(bm.convert(md, cid, [])) + '</section>')
    img = os.path.abspath(COPERTINA)
    cover = f'''<div class="copertina"><img src="file://{img}"><div class="velo"></div>
<div class="testo"><h1>Ultima Adventures</h1><div class="sotto">Manuale del giocatore<br>Edizione italiana</div></div>
<div class="nota">Guida completa per principianti ed esperti: primi passi, abilità, mestieri, magia, equipaggiamento e il mondo di Sosaria.</div></div>'''
    testa = f'<!doctype html><html lang="it"><head><meta charset="utf-8"><title>Ultima Adventures — Manuale del giocatore</title><style>{CSS}</style></head><body>'
    return (testa + '<style>@page { margin: 0; }</style>' + cover + '</body></html>',
            testa + f'<div class="indice"><h1>Indice</h1><ol>{"".join(toc)}</ol></div>' + ''.join(body) + '</body></html>')

def stampa(html_text, pdf, piede=True):
    with tempfile.NamedTemporaryFile('w', suffix='.html', dir=ROOT, delete=False, encoding='utf-8') as t:
        t.write(html_text); tmp = t.name
    try:
        subprocess.run(['node', os.path.join(ROOT, 'stampa-pdf.mjs'), tmp, pdf] + ([] if piede else ['--senza-piede']), check=True, capture_output=True)
    finally:
        os.unlink(tmp)

def trova_pagine(pdf, chap):
    n = int(re.search(r'Pages:\s+(\d+)', subprocess.run(['pdfinfo', pdf], capture_output=True, text=True).stdout).group(1))
    testi = [norm(subprocess.run(['pdftotext', '-f', str(p), '-l', str(p), '-layout', pdf, '-'], capture_output=True, text=True).stdout) for p in range(1, n + 1)]
    pagine, start = {}, 2  # dopo l'indice (la copertina è un file a parte)
    for i, (_, title, _) in enumerate(chap):
        key = norm(re.sub(r'[*`]', '', title))[:28]
        for p in range(start, n + 1):
            if key and key in testi[p - 1][:400]:
                pagine[i] = p; start = p; break
    return pagine, n

def main():
    chap = capitoli()
    tmpdir = tempfile.mkdtemp()
    cop, corpo = os.path.join(tmpdir, 'copertina.pdf'), os.path.join(tmpdir, 'corpo.pdf')
    h_cop, h_corpo = pagina(chap, {})
    stampa(h_cop, cop, piede=False)
    stampa(h_corpo, corpo)                              # prima passata: numeri di pagina provvisori
    pagine, n = trova_pagine(corpo, chap)
    stampa(pagina(chap, pagine)[1], corpo)              # seconda passata: indice completo
    pagine2, n2 = trova_pagine(corpo, chap)
    subprocess.run(['pdfunite', cop, corpo, OUT], check=True)
    mancanti = [chap[i][1] for i in range(len(chap)) if i not in pagine2]
    print(f'PDF: {OUT}  pagine: {n2}  capitoli trovati: {len(pagine2)}/{len(chap)}' + (f'  mancanti: {mancanti}' if mancanti else ''))
    if pagine2 != pagine:
        print('attenzione: i numeri di pagina sono cambiati fra le due passate', pagine, pagine2)

if __name__ == '__main__':
    main()
