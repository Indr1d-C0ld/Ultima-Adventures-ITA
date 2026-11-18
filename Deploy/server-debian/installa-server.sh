#!/bin/bash
# Installa Ultima Adventures (ITA) come servizio su Debian .
# Uso (da root o con sudo):  sudo ./installa-server.sh  [archivio-server.tar.gz] [indirizzo-pubblico]
set -euo pipefail
ARCHIVIO="${1:-$(dirname "$(readlink -f "$0")")/ultima-adventures-server.tar.gz}"
DEST=/opt/ultima-adventures
INDIRIZZO="${2:-}"
UTENTE=ultima
PORTA=2593

[ "$(id -u)" -eq 0 ] || { echo "Esegui con sudo."; exit 1; }
[ -f "$ARCHIVIO" ] || { echo "Archivio non trovato: $ARCHIVIO"; exit 1; }

echo "== Utente di servizio '$UTENTE'"
id "$UTENTE" >/dev/null 2>&1 || useradd --system --create-home --home-dir /var/lib/$UTENTE --shell /usr/sbin/nologin "$UTENTE"

echo "== Pacchetti di sistema necessari"
apt-get update -qq
apt-get install -y -qq curl bzip2 ca-certificates zlib1g >/dev/null

echo "== Copia del server in $DEST"
mkdir -p "$DEST"
if [ -d "$DEST/Server/Saves" ]; then
  echo "   Trovati salvataggi esistenti: li conservo (copia in $DEST/Saves-prima-aggiornamento)"
  rm -rf "$DEST/Saves-prima-aggiornamento"; cp -a "$DEST/Server/Saves" "$DEST/Saves-prima-aggiornamento"
  tar -xzf "$ARCHIVIO" -C "$DEST" --exclude='Server/Saves'
else
  tar -xzf "$ARCHIVIO" -C "$DEST"
fi

if [ -n "$INDIRIZZO" ]; then echo "$INDIRIZZO" > "$DEST/Server/Data/indirizzo-pubblico.txt"; echo "== Indirizzo pubblico: $INDIRIZZO"; fi

echo "== Mono 6.12 (runtime .NET) in $DEST/mono"
if [ ! -x "$DEST/mono/bin/mono" ]; then
  mkdir -p "$DEST/micromamba"
  curl -fsSL https://micro.mamba.pm/api/micromamba/linux-64/latest | tar -xj -C "$DEST/micromamba" bin/micromamba
  MAMBA_ROOT_PREFIX="$DEST/micromamba" "$DEST/micromamba/bin/micromamba" create -y -q -p "$DEST/mono" -c conda-forge mono=6.12
fi
"$DEST/mono/bin/mono" --version | head -1

echo "== Script di avvio e servizio"
install -m 755 "$(dirname "$(readlink -f "$0")")/avvia-server.sh" "$DEST/avvia-server.sh"
install -m 755 "$(dirname "$(readlink -f "$0")")/console-server.sh" /usr/local/bin/ultima-console
install -m 755 "$(dirname "$(readlink -f "$0")")/backup-salvataggi.sh" "$DEST/backup-salvataggi.sh"
install -m 644 "$(dirname "$(readlink -f "$0")")/ultima-adventures.service" /etc/systemd/system/ultima-adventures.service
install -m 644 "$(dirname "$(readlink -f "$0")")/ultima-backup.service" /etc/systemd/system/ultima-backup.service
install -m 644 "$(dirname "$(readlink -f "$0")")/ultima-backup.timer" /etc/systemd/system/ultima-backup.timer
mkdir -p /var/backups/ultima-adventures
chown -R "$UTENTE:$UTENTE" "$DEST" /var/backups/ultima-adventures

if command -v ufw >/dev/null 2>&1 && ufw status | grep -q active; then
  echo "== Firewall: apro la porta $PORTA/tcp"; ufw allow $PORTA/tcp
fi

systemctl daemon-reload
systemctl enable --now ultima-backup.timer
systemctl enable ultima-adventures.service
systemctl restart ultima-adventures.service
echo
echo "Fatto. Il server si sta avviando (il primo avvio compila gli script: 1-2 minuti)."
echo "  Stato:         systemctl status ultima-adventures"
echo "  Log:           journalctl -u ultima-adventures -f"
echo "  Comandi:       ultima-console save | ultima-console \"bc Messaggio a tutti\" | ultima-console online"
echo "  Arresto:       systemctl stop ultima-adventures   (salva il mondo prima di chiudere)"
echo "  Backup:        ogni notte in /var/backups/ultima-adventures (ultimi 14)"
