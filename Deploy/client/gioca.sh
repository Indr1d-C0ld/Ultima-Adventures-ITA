#!/bin/bash
# Ultima Adventures (ITA) - avvio del client su Linux.
# Uso: ./gioca.sh [locale|lan|internet]   (senza argomento: chiede quale collegamento usare)
BASE="$(cd "$(dirname "$(readlink -f "$0")")" && pwd)"
PROF="$1"
ipdi() { grep -o '"ip": *"[^"]*"' "$BASE/Profili/$1.json" | sed 's/.*"\([^"]*\)"$/\1/'; }
if [ -z "$PROF" ]; then
  if command -v zenity >/dev/null 2>&1; then
    PROF=$(zenity --list --title="Ultima Adventures" --text="Dove si trova il server?" --column=Profilo --column=Descrizione \
      internet "Da Internet ($(ipdi internet))" lan "Rete di casa ($(ipdi lan))" locale "Questo stesso PC" --height=260 --width=420) || exit 0
  else
    echo "Collegamento: 1) Internet  2) Rete di casa (LAN)  3) Questo PC"; read -r -p "Scelta [1]: " c
    case "$c" in 2) PROF=lan;; 3) PROF=locale;; *) PROF=internet;; esac
  fi
fi
case "$PROF" in locale|lan|internet) ;; *) echo "Profilo sconosciuto: $PROF (usa locale, lan o internet)"; exit 1;; esac
chmod +x "$BASE/Client-Linux/TazUO" 2>/dev/null
cd "$BASE/Client-Linux" || exit 1
exec ./TazUO -settings "$BASE/Profili/$PROF.json" -uopath "$BASE/Dati" -profilespath "$BASE/Profili/Personaggi"
