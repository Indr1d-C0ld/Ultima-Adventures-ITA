#!/bin/bash
# Crea le voci "Ultima Adventures" nel menu delle applicazioni (Debian/GNOME/KDE/XFCE).
BASE="$(cd "$(dirname "$(readlink -f "$0")")" && pwd)"
APPS="$HOME/.local/share/applications"; mkdir -p "$APPS"
ICON="$BASE/Client-Linux/ultima.png"; [ -f "$ICON" ] || ICON=applications-games
for p in "internet:Internet" "lan:Rete di casa" "locale:Questo PC"; do
  id=${p%%:*}; label=${p#*:}
  cat > "$APPS/ultima-adventures-$id.desktop" <<DESK
[Desktop Entry]
Type=Application
Name=Ultima Adventures ($label)
Comment=Gioca a Ultima Adventures in italiano
Exec="$BASE/gioca.sh" $id
Path=$BASE
Icon=$ICON
Terminal=false
Categories=Game;RolePlaying;
DESK
done
chmod +x "$BASE/gioca.sh" "$BASE/Client-Linux/TazUO"
update-desktop-database "$APPS" >/dev/null 2>&1
echo "Fatto: cerca \"Ultima Adventures\" nel menu delle applicazioni."
