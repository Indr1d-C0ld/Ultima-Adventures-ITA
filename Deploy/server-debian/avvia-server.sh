#!/bin/bash
# Avvio del server con la console collegata a una named pipe (per i comandi e l'arresto pulito).
DEST=/opt/ultima-adventures
FIFO="$DEST/console.fifo"
cd "$DEST/Server" || exit 1
[ -p "$FIFO" ] || { rm -f "$FIFO"; mkfifo -m 600 "$FIFO"; }
exec 3<>"$FIFO"          # lettura+scrittura: la pipe non va mai in EOF
export PATH="$DEST/mono/bin:$PATH"
exec "$DEST/mono/bin/mono" --server AdventuresServer.exe -service <&3   # -service: nessuna attesa di tasti in caso di errore
