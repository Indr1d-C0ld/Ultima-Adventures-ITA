# 3. Comandi e parole chiave

In Ultima Adventures puoi agire in tre modi:

1. con il **mouse**: doppio clic, trascinamento, menu contestuale con il clic destro;
2. con i **comandi**, cioè testo scritto nella chat che inizia con la parentesi quadra `[`;
3. con le **parole chiave**, cioè frasi normali dette vicino a un PNG o ai tuoi animali.

Questo capitolo elenca tutti i comandi disponibili ai giocatori e le principali parole chiave.

> **Consiglio:** il comando `[help` mostra l'elenco di tutti i comandi a cui hai accesso. `[helpinfo <comando>` spiega un comando specifico, e `[helpinfo search <parola>` cerca tra i comandi. Molti comandi sono anche pulsanti nella guida **Help** del paperdoll e nella `[quickbar`.

**Come leggere le tabelle:** `<testo>` va sostituito con un valore (senza parentesi angolari), `[testo]` è facoltativo. Maiuscole e minuscole non contano: `[QuickBar` e `[quickbar` sono equivalenti.

---

## 3.1 Comandi essenziali

| Comando | Cosa fa | Esempio |
|---|---|---|
| `[principiante` (alias `[novizio`, `[beginner`) | Mostra lo stato della **protezione del principiante**: ore e punti abilità mancanti. Permette di rinunciarvi, con una conferma; la scelta è definitiva. Vedi il [capitolo 2](02-primi-passi.md#protezione-del-principiante) | `[principiante` |
| `[guida` | Apre la guida in gioco, con gli argomenti essenziali per chi inizia (alias `[guide`, `[manuale`) | `[guida` |
| `[ispirazione` | Mostra l'ispirazione accumulata riposando e il bonus di varietà (vedi «Progressione» nel capitolo 4) | `[ispirazione` |
| `[help` | Elenca tutti i comandi che puoi usare | `[help` |
| `[helpinfo [<comando>\|search <termine>]` | Spiega un comando o cerca tra i comandi | `[helpinfo loot` |
| `[status` | Scheda dettagliata del personaggio: caratteristiche, livello, Bilancia, **costo della resurrezione** | `[status` |
| `[quickbar` | Barra verticale con le funzioni più usate | `[quickbar` |
| `[skill` | Descrive a cosa serve ogni abilità | `[skill` |
| `[skilllist` (alias `[skillslist`) | Finestra compatta con le abilità impostate "su" (e quelle bloccate) | `[skilllist` |
| `[myhunger` (alias `[mhgr`) | Ti dice quanto sei affamato e assetato | `[myhunger` |
| `[quests` | Apre il diario delle missioni | `[quests` |
| `[motd` | Messaggio del giorno | `[motd` |
| `[corpse` | Mostra una freccia verso il tuo **cadavere** più vicino (entro 1.000 caselle, nella stessa terra). Funziona **solo da vivo** | `[corpse` |
| `[magicgate` | Mostra una freccia verso il **portale lunare** o la pietra nera più vicini, entro 1.000 caselle | `[magicgate` |
| `[bandself` | Ti bendi da solo, usando le bende dello zaino. Ti avvisa se ne restano 5 o meno | `[bandself` |
| `[bandother` | Apre il cursore per bendare un'altra persona o un animale | `[bandother` |
| `[wealth` | Barra orizzontale con l'oro nello zaino e in banca, e il valore delle monete antiche (rame, argento, xormite, gioielli, cristalli). Clic destro per aggiornarla | `[wealth` |
| `[dailytimer` | Tempo mancante ai prossimi azzeramenti orari e giornalieri | `[dailytimer` |
| `[bug` | Invia una segnalazione di bug | `[bug` |
| `[password` | Cambia la password del tuo account | `[password` |
| `[changecharacter` | Torna alla scelta del personaggio senza scollegarti. Funziona solo se hai più personaggi e sei in un luogo dove ti puoi scollegare subito, fuori dal combattimento | `[changecharacter` |

## 3.2 Morte e cadaveri

| Comando | Cosa fa |
|---|---|
| `[corpse` | Freccia verso il tuo cadavere (solo da vivo) |
| `[gravedigger` | Evoca **Squeiyk il Becchino** per 5 minuti. Se gli dici "**fetch**" entro 4 caselle, porta ai tuoi piedi tutti i tuoi cadaveri non vuoti che si trovano nella stessa terra e non dentro una casa. Costa **40.000 monete d'oro**, prese dallo zaino o dalla banca. Solo da vivo |
| `[corpseclear` | **Cancella** i tuoi cadaveri che si trovano **su una nave**, insieme a tutto quello che contengono |
| `[deathreport` (alias `[dr`) | Attiva o disattiva la finestra di 5 secondi che, alla morte, mostra chi ti ha ucciso |

> **Attenzione:** `[corpseclear` distrugge anche gli oggetti nel cadavere. Usalo solo se hai già recuperato tutto.

## 3.3 Combattimento e abilità speciali delle armi

| Comando | Cosa fa |
|---|---|
| `[sad` (alias `[specialattacksdisplay`) | Apre la barra delle **abilità speciali** dell'arma impugnata |
| `[set1` … `[set5` (alias `[setprimaryability`, `[setsecondaryability`, `[setthirdability`, `[setfourthability`, `[setfifthability`) | Attiva la 1ª…5ª abilità speciale dell'arma. Ottimi da mettere in una macro |
| `[abilitynames` | Mostra o nasconde i nomi delle abilità accanto alle icone |
| `[combatbar` | Apre la barra di combattimento |
| `[autodefend` | Attiva o disattiva la difesa automatica quando vieni attaccato |
| `[sheathe` | Attiva o disattiva il rinfoderare automatico dell'arma quando non combatti |
| `[poisons` | Cambia il funzionamento delle armi avvelenate. Di default il veleno si applica con i colpi speciali "infettanti"; in alternativa con qualsiasi colpo di un'arma a una mano da taglio o da punta |
| `[agility` | Mostra il tuo valore di agilità. Conta solo per i personaggi "Puritain" delle Midlands; per gli altri risponde "That won't work here" |
| `[itemexp` (alias `[ixp`) | Mostra esperienza e punti di un **oggetto livellabile** (zaino o equipaggiamento) |

## 3.4 Animali e seguaci

| Comando | Cosa fa | Esempio |
|---|---|---|
| `[petrange <0-10>` (alias `[petstandby`) | Quanto i tuoi animali si sparpagliano quando ti seguono o restano fermi. 0 = non si sparpagliano | `[petrange 3` |

Per gli ordini agli animali vedi [3.12](#312-ordini-agli-animali).

## 3.5 Magia e barre degli incantesimi

### Contatori dei reagenti

| Comando | Cosa fa |
|---|---|
| `[mreagents` | Barra dei reagenti di Magia |
| `[nreagents` | Barra dei reagenti di Negromanzia |
| `[areagents` | Barra dei reagenti alchemici |
| `[creagents` | Chiude tutte le barre dei reagenti |
| `[reagents`, `[reagentbar1` … `[reagentbar4` | Varianti delle barre dei reagenti |

Le barre si aggiornano quando lanci un incantesimo o prepari una pozione. Clic destro per aggiornarle a mano.

### Barre degli incantesimi

Ogni scuola ha due tipi di comandi: `…spell<N>` (oppure `…song<N>`) apre l'**editor** per scegliere quali incantesimi mettere nella barra, `…tool<N>` **apre** la barra e `…close<N>` la **chiude**.

| Scuola | Editor | Apri barra | Chiudi barra |
|---|---|---|---|
| Magia | `[magespell1`…`4` | `[magetool1`…`4` | `[mageclose1`…`4` |
| Negromanzia | `[necrospell1`, `2` | `[necrotool1`, `2` | `[necroclose1`, `2` |
| Cavalleria | `[chivalryspell1`, `2` | `[chivalrytool1`, `2` | `[chivalryclose1`, `2` |
| Canti del Bardo | `[bardsong1`, `2` | `[bardtool1`, `2` | `[bardclose1`, `2` |
| Cavaliere della Morte | `[deathspell1`, `2` | `[deathtool1`, `2` | `[deathclose1`, `2` |
| Sant'Uomo (preghiere) | `[holyspell1`, `2` | `[holytool1`, `2` | `[holyclose1`, `2` |
| Monaco | `[monkspell1`, `2` | `[monktool1`, `2` | `[monkclose1`, `2` |
| Ricerca arcana | `[researchspell1`…`4` | `[researchtool1`…`4` | `[researchclose1`…`4` |
| Jedi | `[jedipowerrow`, `[jedipowercolumn` (barra orizzontale o verticale) | | |
| Syth | `[sythpowerrow`, `[sythpowercolumn` | | |

| Comando | Cosa fa | Esempio |
|---|---|---|
| `[spellhue <numero>` | Cambia il colore degli effetti dei tuoi incantesimi di Magia. Evita il valore 1, che viene reso come 0 | `[spellhue 1153` |

### Lanciare incantesimi speciali per nome

Le scuole di magia personalizzate si possono lanciare anche con un comando, molto comodo nelle macro:

| Scuola | Comandi |
|---|---|
| **Canti del Bardo** | `[ArmysPaeon` `[EnchantingEtude` `[EnergyCarol` `[EnergyThrenody` `[FireCarol` `[FireThrenody` `[FoeRequiem` `[IceCarol` `[IceThrenody` `[KnightsMinne` `[MagesBallad` `[MagicFinale` `[PoisonCarol` `[PoisonThrenody` `[ShephardsDance` `[SinewyEtude` |
| **Cavaliere della Morte** | `[Banish` `[DemonicTouch` `[DevilPact` `[GrimReaper` `[HagHand` `[Hellfire` `[LucifersBolt` `[OrbofOrcus` `[ShieldofHate` `[SoulReaper` `[StrengthofSteel` `[Strike` `[SuccubusSkin` `[Wrath` |
| **Sant'Uomo** | `[BanishEvil` `[DampenSpirit` `[Enchant` `[HammerOfFaith` `[HeavenlyLight` `[Nourish` `[Purge` `[Rebirth` `[SacredBoon` `[Sanctify` `[Seance` `[Smite` `[TouchOfLife` `[TrialByFire` |
| **Giullare** | `[CanOfSnakes` `[Clowns` `[FlowerPower` `[Hilarity` `[Insult` `[JumpAround` `[PoppingBalloon` `[RabbitInAHat` `[SeltzerBottle` `[SurpriseGift` |
| **Monaco** (Misticismo) | `[AstralProjection` `[AstralTravel` `[CreateRobe` `[GentleTouch` `[Leap` `[PsionicBlast` `[PsychicWall` `[PurityOfBody` `[QuiveringPalm` `[WindRunner` |
| **Jedi** | `[ForceGrip` `[MindsEye` `[Mirage` `[ThrowSabre` `[Celerity` `[PsychicAura` `[Deflection` `[SoothingTouch` `[StasisField` `[Replicate` |
| **Syth** | `[Psychokinesis` `[DeathGrip` `[Projection` `[ThrowSword` `[SythSpeed` `[SythLightning` `[Absorption` `[PsychicBlast` `[DrainLife` `[CloneBody` |
| **Ricerca arcana** | `[Cast<Nome>`, per esempio `[CastConjure`, `[CastDeathSpeak`, `[CastSneak`, `[CastCreateFire`, `[CastIcicle`, `[CastHealingTouch`, `[CastSnowBall`, `[CastSleep`, `[CastEtherealTravel`, `[CastWizardEye`, `[CastCreateGold`, `[CastAnimateBones`, `[CastFlameBolt`, `[CastMagicSteed`, `[CastMeteorShower`, `[CastAirWalk`, `[CastWithstandDeath`, `[CastRestoration`, `[CastMassDeath`… 64 incantesimi in tutto. Per l'elenco completo vedi il capitolo sulla magia |

Il comando funziona solo se conosci l'incantesimo e hai i requisiti (abilità, mana, reagenti o oggetti richiesti).

## 3.6 Bottino, ricchezza e mercato

| Comando | Cosa fa |
|---|---|
| `[loot` | Imposta il **bottino automatico**: quali categorie di oggetti prendere da forzieri comuni e cadaveri (oro, gemme, oggetti non identificati, reagenti di Magia, Negromanzia, alchimia o erboristeria…) |
| `[wealth` | Barra della ricchezza (vedi [3.1](#31-comandi-essenziali)) |
| `[bigbank` | Alterna la finestra della cassetta di sicurezza tra formato piccolo e grande |
| `[vendorsearch` | Cerca oggetti in vendita presso i **mercanti dei giocatori**. Se l'indice è ancora in costruzione, riprova poco dopo |
| `[myhouses` | Elenca tutte le case del tuo account, con le opzioni "vai alla casa" e "demolisci" |
| `[lottery` | Informazioni sulla prossima estrazione della lotteria di Lady Luck |

## 3.7 Stile di gioco, aspetto e socialità

| Comando | Cosa fa | Esempio |
|---|---|---|
| `[evil` | Stile di gioco **malvagio**: tesori e titoli a tema oscuro. Si può tenere attivo un solo stile alla volta | `[evil` |
| `[oriental` | Stile **orientale**: tesori e titoli giapponesi o cinesi | `[oriental` |
| `[barbaric` | Stile **barbarico**: compare nello zaino una sacca che trasforma l'equipaggiamento in versioni "spada e stregoneria", con titoli da Barbaro (o Amazzone) | `[barbaric` |
| `[skillname "<abilità>"` | Imposta quale abilità dà il titolo al personaggio. Il nome va in inglese, minuscolo e tra virgolette. Usa `"clear"` per tornare al titolo automatico | `[skillname "animal taming"` |
| `[look` | Guarda il profilo di qualcuno; selezionando te stesso puoi modificare il tuo | `[look` |
| `[afk` | Segnala agli altri che sei lontano dalla tastiera | `[afk` |
| `[emote` / `[e` | Apre la finestra (o la mini-finestra) delle **emote**, con suoni e animazioni. Puoi anche scrivere direttamente `[emote <nome>`, con questi nomi: ah, ahha, applaud, blownose, bow, bscough, burp, clearthroat, cough, cry, faint, fart, gasp, giggle, groan, growl, hey, hiccup, huh, kiss, laugh, no, oh, oooh, oops, puke, punch, scream, shush, sigh, slap, sneeze, sniff, snore, spit, stickouttongue, tapfoot, wistle, woohoo, yawn, yea, yell | `[emote bow` |
| `[roll [<min>-<max>]` | Tira un dado | `[roll 1-20` |
| `[mex` | Apre il gioco di dadi del Mexicali (Liar's Dice) | `[mex` |
| `[createstory` | Crea un oggetto con una storia da piazzare nel mondo (Marked Items) | `[createstory` |
| `[ratings` | Classifica della Bilancia (Ordine o Caos). Funziona solo se hai giurato per uno dei due schieramenti | `[ratings` |
| `[gscores` | Classifica del Gauntlet PvM | `[gscores` |
| `[statistics` | Statistiche del server | `[statistics` |
| `[publicchar` / `[privatechar` | Rende pubblici o privati i dettagli del personaggio sulle statistiche web (se il server le usa) | |

### Chat tra giocatori

| Comando | Cosa fa |
|---|---|
| `[pm <nome> <messaggio>` (alias `[msg`) | Messaggio privato |
| `[friends` (alias `[fri`) | Elenco degli amici |
| `[mail` (alias `[ma`) | Posta della chat |
| `[viewall` (alias `[va`) | Mostra tutti gli utenti della chat |
| `[helpcontents` (alias `[hc`) | Aiuto del sistema di chat |

La chat completa si apre anche dal pulsante *Chat* della guida Help.

## 3.8 Musica

| Comando | Cosa fa |
|---|---|
| `[music` | Apre il lettore musicale per scegliere il brano |
| `[musical` | Nei dungeon suona la musica "da viaggio" al posto di quella dei dungeon (e viceversa) |
| `[stopmusic` | Interrompe una melodia in corso |
| `[filtermusic` | Attiva o disattiva l'ascolto delle melodie suonate dagli altri |

## 3.9 Comandi da ignorare

Alcuni comandi sono registrati come comandi da giocatore ma servono solo agli sviluppatori o non fanno nulla di utile:

- `[search octant|line|circle|spiral` mostra solo effetti grafici di prova;
- `[fonts` e `[dumpfonts` sono strumenti tecnici.

La vecchia guida Help cita anche `[c` e `[private`, che **non esistono più**. `[save` è riservato allo staff su questo server.

---

## 3.10 Parole chiave per i PNG

Le parole chiave si **dicono** nella chat normale, senza `[`, stando vicino al PNG (di solito entro 3–4 caselle; il banchiere sente fino a 12). Il server capisce **sia l'inglese sia l'italiano**. Basta che la frase **contenga** la parola: per esempio "vorrei aprire la mia banca" funziona come "banca".

### Mercanti

| Italiano | Inglese | Effetto |
|---|---|---|
| "mercante compra" / "compra" / "comprare" | "vendor buy" / "buy" | Apre la lista di ciò che il mercante vende |
| "mercante vendi" / "vendi" / "vendere" | "vendor sell" / "sell" | Apre la lista di ciò che il mercante compra da te |
| — | "restock" | Ti dice quanto manca al prossimo rifornimento (avviene ogni 2 ore) |
| — | "vendor explain" o "<nome> explain" | Spiega il mestiere del mercante |
| "addestra" / "insegna" / "addestrami" | "train" | Elenca le abilità che il PNG può insegnarti. Poi di' il nome dell'abilità per il prezzo e lascia l'oro sul PNG |
| "spostati" | "move" | Chiede al PNG di spostarsi |
| "che ore" / "che ora" | "time" | Ti dice l'ora |

Su questo server puoi anche **fare clic destro** sui mercanti e scegliere *Compra* o *Vendi* dal menu contestuale.

I mercanti di Fabbro, Sartoria, Carpenteria e Costruzione Archi gestiscono anche gli **ordini di massa**. Di' "credits" o "total" per vedere i tuoi crediti, e "claim help", "claim <N>", "cash in" o "redeem" per spenderli.

### Banchiere

| Italiano | Inglese | Effetto |
|---|---|---|
| "banca" | "bank" | Apre la cassetta di sicurezza |
| "saldo" / "estratto conto" | "balance" | Ti dice quanto oro hai in banca |
| "preleva <importo>" | "withdraw <importo>" | Preleva oro dalla banca, al massimo **60.000** per volta. Esempio: "preleva 2000" |
| "assegno <importo>" | "check <importo>" | Crea un assegno bancario, da **5.000** a **10.000.000**. Esempio: "assegno 50000" |
| (lascia l'oro sul banchiere) | "deposit" | Per depositare, trascina l'oro sul banchiere |

> **Attenzione:** criminali e assassini (chi ha almeno un omicidio) non possono usare il menu contestuale della banca.

### Maestro di stalla e veterinario

| Italiano | Inglese | Effetto |
|---|---|---|
| "stalla" | "stable" | Metti un animale nella stalla: **30 monete d'oro** ad animale |
| "riprendi" / "riscatta" | "claim" / "claim <nome>" | Riprendi tutti i tuoi animali, o quello con quel nome |
| — | "find" / "fetch" | Il maestro di stalla (o il veterinario) richiama tutti i tuoi seguaci sperduti, per **1.000 monete d'oro** |

### Gilde dei PNG

Per entrare in una gilda di mestiere usa il **menu contestuale** del maestro di gilda (*Join*). Per uscirne usa *Resign*. Vedi [4.16](04-meccaniche.md#416-gilde). Di' "gilda" o "gilde" (guild) per avere informazioni.

### Altri PNG

| PNG | Parole | Effetto |
|---|---|---|
| Capitano del traghetto | "ferry", "passage", "travel" | Viaggio sulla stessa mappa: 500 monete d'oro, partenza ogni 15 minuti |
| Cocchiere | "wagon", "coach", "caravan", "travel" | Viaggio in carovana: 300 monete d'oro, partenza ogni 15 minuti |
| Mediatore di animali | "sell", "appraise" (in inglese, come frase esatta) | Valuta o compra il tuo animale |
| Mediatore immobiliare | "valuta" / "appraise" | Valuta un atto di proprietà |
| Araldo cittadino | "notizie" (news), "infected" | Notizie, epidemie |
| Xardok | "murders" / "bribe" / "kills", poi "accept" | Cancella i tuoi omicidi per **10.000 monete d'oro** ciascuno |
| Lord of Time (Time Lord) | "balance" / "pledge" / "buy" | Giuramento per Ordine o Caos e negozio della Bilancia (vedi [4.6](04-meccaniche.md#la-bilancia-ordine-e-caos)) |
| Squeiyk il Becchino | "fetch" | Recupera i tuoi cadaveri (vedi [3.2](#32-morte-e-cadaveri)) |
| Locandiere degli scudieri | "room", "help" (preceduti da "innkeeper", "keeper", "vendor" o dal suo nome) | Alloggio per lo scudiero (300 monete d'oro) |

### Guardie

Dire "guards" (o "guardie") **non evoca nessuna guardia** su questo server. La sicurezza delle città è affidata alle **guardie blu** (Blue Guard), che pattugliano e attaccano da sole criminali, assassini e personaggi con karma molto basso (vedi [4.6](04-meccaniche.md#46-notorietà-karma-fama-criminali-assassini)).

### Frasi generali

| Frase | Effetto |
|---|---|
| "i must consider my sins" | Mostra i tuoi omicidi |
| "abbandono la gilda" / "i resign from my guild" | Lasci la gilda **dei giocatori** |
| "aiuto" / "help" | Richiesta d'aiuto |

### Mercante di un giocatore (player vendor)

| Italiano | Inglese | Effetto |
|---|---|---|
| "mercante guarda" | "vendor browse" | Mostra la merce |
| "mercante incassa" | "vendor collect" | Ritira i guadagni (proprietario) |
| "mercante stato" | "vendor status" | Stato del mercante |
| "mercante congeda" | "vendor dismiss" | Congeda il mercante |
| "<nome> report [N]" | | Elenco delle ultime vendite simulate |

## 3.11 Parole chiave per la casa

Si dicono **dentro la propria casa**:

| Italiano | Inglese | Effetto |
|---|---|---|
| "voglio bloccare questo" | "I wish to lock this down" | Fissa un oggetto alla casa |
| "voglio liberare questo" | "I wish to release this" | Sblocca un oggetto fissato |
| "voglio proteggere questo" | "I wish to secure this" | Rende sicuro un contenitore |
| "voglio togliere la protezione" | "I wish to unsecure this" | Toglie la protezione |
| "voglio mettere una cassaforte" | "I wish to place a strongbox" | Piazza una cassaforte (comproprietari) |
| "voglio mettere un cestino" | "I wish to place a trash barrel" | Piazza un cestino dei rifiuti |
| "vattene" | "remove thyself" | Caccia qualcuno dalla casa |
| "ti bandisco" | "I ban thee" | Bandisce qualcuno dalla casa |
| — | "I wish to resize my house" | Ridimensiona la casa |
| — | "check house rent" | Affitto (solo case cittadine in affitto) |

## 3.12 Ordini agli animali

Puoi dare ordini a **un animale** dicendo il suo nome seguito dal comando, per esempio "Fido segui". Per darli a **tutti** gli animali usa la forma "tutti …" (o "all …" in inglese). Gli ordini funzionano per il padrone e per gli amici dell'animale.

### Ordini a un singolo animale ("<nome> …")

| Italiano | Inglese | Effetto |
|---|---|---|
| "vieni" | "come" | Viene da te |
| "segui" | "follow" | Segue il bersaglio che indichi |
| "seguimi" | "follow me" | Ti segue |
| "proteggi" / "difendi" | "guard" | Protegge te (o il bersaglio) |
| "uccidi" | "kill" | Attacca il bersaglio che indichi |
| "attacca" | "attack" | Come "uccidi" |
| "fermo" / "fermati" | "stop" | Smette quello che sta facendo |
| "resta" / "resta qui" | "stay" | Resta fermo dove si trova |
| "pattuglia" | "patrol" | Pattuglia la zona |
| "lascia" | "drop" | Lascia a terra ciò che trasporta |
| "riporta" / "prendi" / "porta qui" | "fetch" / "get" / "bring" | Riporta un oggetto |
| "amico" | "friend" | Aggiunge un amico dell'animale, che potrà dargli ordini |
| "trasferisci" | "transfer" | Cede l'animale a un altro giocatore |
| "libera" | "release" | **Libera** l'animale, che torna selvatico |
| "rapporto" | "report" | Rapporto (soprattutto per gli scudieri) |
| "ti assumo" | "hire" | Assume un mercenario |

### Ordini a tutti gli animali

| Italiano | Inglese | Effetto |
|---|---|---|
| "tutti vieni" / "tutti venite" | "all come" | Vengono tutti da te |
| "tutti segui" / "tutti seguite" | "all follow" | Seguono il bersaglio indicato |
| "tutti seguitemi" | "all follow me" | Ti seguono tutti |
| "tutti proteggete" | "all guard" | Proteggono il bersaglio |
| "tutti proteggetemi" | "all guard me" | Proteggono te |
| "tutti uccidete" / "tutti attaccate" / "tutti attacca" | "all kill" / "all attack" | Attaccano tutti il bersaglio indicato |
| "tutti fermi" | "all stop" | Si fermano tutti |
| "tutti restate" / "tutti fermi qui" | "all stay" | Restano tutti dove sono |
| "tutti rapporto" | "all report" | Rapporto |

> **Attenzione:** non esiste "tutti liberi" / "all release": gli animali si liberano uno alla volta, per sicurezza. Gli animali con poca lealtà possono ignorare gli ordini, e quelli inselvatichiti ne ignorano il 75%.

> **Consiglio:** dare troppi ordini in fretta fa calare la lealtà degli animali. Crea delle macro per "tutti seguitemi", "tutti proteggetemi" e "tutti attaccate" e usale con calma.

### Scudieri (squire)

Gli scudieri accettano ordini preceduti da "all", dal loro nome, dal soprannome o dal nome della squadra. Tra gli ordini ci sono "stop", "stay", "follow me", "come", "attack", "guard me", "dress", "undress", "mount", "dismount", "loot all", "grab all", "heal", "drink <pozione>", "stats", "skills", "equip set one/two/three" e altri. I dettagli sono nel capitolo dedicato ai seguaci.

## 3.13 Mini-giochi e varie

- **Navi**: al timoniere si danno i classici ordini inglesi: "forward", "back", "left", "right", "stop", "turn left", "raise anchor", "drop anchor", "one" (un passo)…
- **Automazione del personaggio**: frasi come "i wish to make…", "i wish to start…", "i wish to stop" attivano alcune azioni ripetute (per esempio artigianato in serie) dove previste.
