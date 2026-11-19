// Stampa un file HTML in PDF con Chromium headless (protocollo DevTools), con numeri di pagina a piè di pagina.
// Uso: node stampa-pdf.mjs <input.html> <output.pdf>
import { spawn } from 'node:child_process';
import { writeFileSync, mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';

const [, , input, output, flag] = process.argv;
const piede = flag !== '--senza-piede';
const port = 9300 + Math.floor(Math.random() * 500);
const profile = mkdtempSync(join(tmpdir(), 'ua-pdf-'));
const chrome = spawn('chromium', ['--headless=new', `--remote-debugging-port=${port}`, `--user-data-dir=${profile}`,
  '--no-first-run', '--disable-gpu', '--allow-file-access-from-files', 'about:blank'], { stdio: 'ignore' });

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
let target;
for (let i = 0; i < 100 && !target; i++) {
  try { target = (await (await fetch(`http://127.0.0.1:${port}/json`)).json()).find((t) => t.type === 'page'); }
  catch { await sleep(200); }
}
if (!target) { chrome.kill(); throw new Error('Chromium non risponde'); }

const ws = new WebSocket(target.webSocketDebuggerUrl);
await new Promise((r) => ws.addEventListener('open', r, { once: true }));
let id = 0; const pending = new Map(); const events = [];
ws.addEventListener('message', (m) => {
  const d = JSON.parse(m.data);
  if (d.id && pending.has(d.id)) { pending.get(d.id)(d); pending.delete(d.id); } else events.push(d);
});
const send = (method, params = {}) => new Promise((r) => { const i = ++id; pending.set(i, r); ws.send(JSON.stringify({ id: i, method, params })); });

await send('Page.enable');
await send('Page.navigate', { url: 'file://' + resolve(input) });
for (let i = 0; i < 600 && !events.some((e) => e.method === 'Page.loadEventFired'); i++) await sleep(100);
await send('Runtime.evaluate', { expression: 'document.fonts.ready', awaitPromise: true });

const footer = `<div style="width:100%;font-size:8px;font-family:'DejaVu Serif',serif;color:#555;padding:0 14mm;display:flex;justify-content:space-between">
  <span>Ultima Adventures — Manuale del giocatore</span><span><span class="pageNumber"></span> / <span class="totalPages"></span></span></div>`;
const res = await send('Page.printToPDF', {
  printBackground: true, preferCSSPageSize: true, displayHeaderFooter: piede,
  headerTemplate: '<div></div>', footerTemplate: footer,
});
if (!res.result) { console.error(JSON.stringify(res.error)); process.exit(1); }
writeFileSync(output, Buffer.from(res.result.data, 'base64'));
ws.close(); chrome.kill();
console.log('PDF scritto:', output);
