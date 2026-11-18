#!/bin/bash
# Copia compressa dei salvataggi; conserva le ultime 14.
DEST=/opt/ultima-adventures; OUT=/var/backups/ultima-adventures
[ -p "$DEST/console.fifo" ] && { echo save > "$DEST/console.fifo"; sleep 30; }
tar -czf "$OUT/salvataggi-$(date +%Y%m%d-%H%M).tar.gz" -C "$DEST/Server" Saves
ls -1t "$OUT"/salvataggi-*.tar.gz | tail -n +15 | xargs -r rm -f
