#!/bin/bash
# Invia un comando alla console del server.  Esempi: ultima-console save   ultima-console "bc Riavvio tra 5 minuti"
FIFO=/opt/ultima-adventures/console.fifo
[ -p "$FIFO" ] || { echo "Il server non sembra avviato ($FIFO assente)."; exit 1; }
[ -n "${1:-}" ] || { echo "Uso: ultima-console <comando>   (help per l'elenco)"; exit 1; }
if [ ! -w "$FIFO" ]; then exec sudo -u ultima "$0" "$@"; fi
echo "$*" > "$FIFO"
echo "Inviato: $*   (vedi l'esito con: journalctl -u ultima-adventures -n 20)"
