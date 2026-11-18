#!/bin/bash
# Applica un aggiornamento degli script al server sul server.
# La cartella Scripts/ accanto a questo file ha la stessa struttura di /opt/ultima-adventures/Server/Scripts.
# Uso:  sudo ./applica-aggiornamento.sh
set -euo pipefail
SRV=/opt/ultima-adventures/Server; QUI="$(dirname "$(readlink -f "$0")")"
[ "$(id -u)" -eq 0 ] || { echo "Esegui con sudo."; exit 1; }
echo "== File aggiornati:"; (cd "$QUI/Scripts" && find . -type f | sed 's|^\./|   |')
systemctl stop ultima-adventures
cp -r "$QUI/Scripts/." "$SRV/Scripts/"
chown -R ultima:ultima "$SRV/Scripts"
systemctl start ultima-adventures
echo "== Riavvio in corso: il server ricompila gli script (1-2 minuti)."
for i in $(seq 1 60); do
  journalctl -u ultima-adventures --since "-5min" --no-pager | grep -q "Listening: " && break; sleep 5
done
journalctl -u ultima-adventures --since "-5min" --no-pager | grep -E "Compiling C# scripts|error|Listening" | grep -v "warning" | tail -4
