# Ultima Adventures — edizione italiana

Versione personalizzata di Ultima Adventures, shard di Ultima Online basato su RunUO 2.7, tradotta in italiano e resa più accessibile ai principianti.

## Cosa cambia rispetto all'originale

- **Traduzione completa in italiano**, applicata al volo sui pacchetti in uscita: il codice e i salvataggi restano in inglese.
  - Testi del server, dialoghi degli NPC, libri e Cliloc del client (`Server/Data/Localization/it/`).
  - Parole chiave italiane per i dialoghi e i comandi vocali ("banca", "mercante compra", "tutti seguitemi"…).
  - Nomi delle abilità e professioni nel client.
- **Protezione del principiante** (prime 20 ore o 300 punti abilità, comando `[principiante`). Finché è attiva:
  - alla morte non si perdono oggetti né abilità;
  - le trappole distruttive non hanno effetto;
  - niente maledizioni di viaggio e niente agguati casuali;
  - fame e sete calano più lentamente.
- **Progressione meno ripetitiva**:
  - il riposo in locanda, in taverna o a casa accumula ispirazione (comando `[ispirazione`);
  - bonus a chi usa abilità diverse;
  - fascia di abilità 80–100 ammorbidita;
  - caratteristiche che salgono ogni 10 minuti;
  - maestri PNG che insegnano fino a 50.
- **Modalità non-Avatar riequilibrata** e **Equilibrio del mondo funzionante**, centrato sui valori originali.
- **Guida in gioco** (`[guida`) e **manuale completo** in `Documentazione/Manuale.html`, con 18 capitoli e le tabelle di incantesimi, armi e armature.
- **Correzioni di bug.** Tra le altre:
  - funzionamento del server su Linux con Mono (zlib, console senza terminale);
  - mappa UltimaLive inviata già all'ingresso nel gioco;
  - ritardo di guadagno delle caratteristiche condiviso fra tutti i giocatori;
  - risposte dell'introduzione;
  - mosse speciali (Protezione da Mischia, Danza delle Lame, Colpo Tossico);
  - icone delle mosse del client;
  - incantesimo Scaccia Demoni;
  - bonus di Alchimia;
  - gioielli nanici;
  - strumenti ammazza-creature.
- **Strumenti per un server personale**:
  - pulizia del mondo dagli account del server pubblico (`Data/pulizia-mondo.txt`);
  - promozione automatica del primo amministratore (`Data/primo-owner.txt`).

Le soglie e i valori nuovi si regolano in `Server/Scripts/MyServerSettings.cs`.

## Installazione

- **Server su Debian:** vedi `Deploy/server-debian/LEGGIMI.txt`. Il server si installa come servizio systemd, con backup notturni e una console comandi (`ultima-console`).
- **Client:** TazUO, con build native per Windows e Linux, più i profili e gli script in `Deploy/client/`. I dati del client e i salvataggi del mondo non sono inclusi nel repository.

L'indirizzo pubblico del server si imposta in `Server/Data/indirizzo-pubblico.txt`, un file locale che non viene incluso nel repository.

## Licenza

Il progetto originale è distribuito gratuitamente dalla comunità di Ultima Adventures e va usato senza scopo di lucro.
