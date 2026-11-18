#!/bin/bash
# Pulizia del mondo (via tutti i giocatori originali) e primo account Owner.
# Uso:  sudo ./applica-pulizia.sh  [nome-account-owner]     (predefinito: admin)
set -euo pipefail
OWNER="${1:-admin}"
DEST=/opt/ultima-adventures; SRV=$DEST/Server; QUI="$(dirname "$(readlink -f "$0")")"
[ "$(id -u)" -eq 0 ] || { echo "Esegui con sudo."; exit 1; }

echo "== Arresto del server (salva il mondo)"
systemctl stop ultima-adventures

echo "== Copia di sicurezza dei salvataggi attuali"
mkdir -p /var/backups/ultima-adventures
tar -czf /var/backups/ultima-adventures/salvataggi-prima-della-pulizia-$(date +%Y%m%d-%H%M).tar.gz -C "$SRV" Saves

echo "== Installazione della pulizia e della promozione dell'Owner"
install -o ultima -g ultima -m 644 "$QUI/PuliziaMondo.cs" "$SRV/Scripts/Server Functions/Admin Commands/PuliziaMondo.cs"
install -m 755 "$QUI/console-server.sh" /usr/local/bin/ultima-console
echo "pulizia richiesta $(date)" > "$SRV/Data/pulizia-mondo.txt"
echo "$OWNER" > "$SRV/Data/primo-owner.txt"
chown ultima:ultima "$SRV/Data/pulizia-mondo.txt" "$SRV/Data/primo-owner.txt"

echo "== Avvio del server: compila gli script e pulisce il mondo (alcuni minuti)"
systemctl start ultima-adventures
for i in $(seq 1 120); do
  if journalctl -u ultima-adventures --since "-15min" --no-pager | grep -q "PuliziaMondo: mondo salvato"; then break; fi
  sleep 5
done
journalctl -u ultima-adventures --since "-15min" --no-pager | grep -E "PuliziaMondo: eliminati|PuliziaMondo: mondo salvato|rror" | grep -v "calling squire" | tail -5
echo
echo "Fatto. Ora entra con il client usando l'account '$OWNER' e la tua password:"
echo "l'account viene creato al primo accesso e promosso subito a Owner."
