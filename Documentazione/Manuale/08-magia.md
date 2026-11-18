# 8. La magia

Ultima Adventures contiene molti più sistemi magici del Ultima Online classico. Oltre alla **Magia** (Magery) dei maghi, alla **Negromanzia** e alla **Cavalleria**, trovi le tecniche di **Bushido** e **Ninjitsu**, le abilità del **Monaco**, le canzoni del **Bardo**, i trucchi del **Giullare**, le preghiere del **Sant'Uomo**, la magia del **Cavaliere della Morte**, i poteri **Syth** e **Jedi** e i 64 **incantesimi antichi** della **Ricerca arcana**.

Questo capitolo è diviso in più file:

| File | Contenuto |
|---|---|
| **08-magia.md** (questo) | Come funziona la magia in generale: mana, reagenti, libri, pergamene, tempi di lancio, interruzioni, abilità che influenzano danno e successo, come allenarsi |
| [08a-magia-arcana.md](08a-magia-arcana.md) | La Magia (Magery): i 64 incantesimi in 8 cerchi |
| [08b-negromanzia-e-cavalleria.md](08b-negromanzia-e-cavalleria.md) | Negromanzia e Cavalleria |
| [08c-bushido-ninjitsu-monaco.md](08c-bushido-ninjitsu-monaco.md) | Bushido, Ninjitsu e abilità del Monaco (Mystic) |
| [08d-sistemi-speciali.md](08d-sistemi-speciali.md) | Cavaliere della Morte, Sant'Uomo, Giullare, Syth, Jedi, canzoni del Bardo, oggetti magici |
| [08e-ricerca-arcana.md](08e-ricerca-arcana.md) | Ricerca arcana: i 64 incantesimi antichi |

Le pozioni, i veleni e le misture alchemiche (comprese le "pozioni" lanciabili dei negromanti e dei druidi) sono nel [capitolo 9](09-alchimia-e-pozioni.md).

> **Nota sui nomi.** I nomi degli incantesimi di Magia e dei reagenti sono quelli ufficiali della traduzione italiana. Per tutti gli altri sistemi trovi il nome italiano seguito dal nome inglese tra parentesi, perché in alcune finestre del gioco potresti vederlo ancora in inglese. Le **parole di potere** (per esempio *Kal Vas Flam*) non si traducono mai: è ciò che il tuo personaggio pronuncia.

---

## 8.1 Panoramica dei sistemi magici

| Sistema | Abilità di lancio | Abilità che ne aumenta l'effetto | Risorsa consumata | Libro / strumento | Requisito di karma |
|---|---|---|---|---|---|
| Magia (Magery) | Magia | Valutare Intelligenza, Scrittura Arcana | Mana + reagenti | Libro degli incantesimi | Nessuno |
| Negromanzia (Necromancy) | Negromanzia | Parlare con gli Spiriti | Mana + reagenti negromantici | Libro di negromanzia | Karma ≤ 0 (con karma positivo l'incantesimo non ha effetto) |
| Cavalleria (Chivalry) | Cavalleria | Cavalleria + karma | Mana + punti di decima | Libro della Cavalleria | Karma ≥ 0 |
| Bushido | Bushido | Bushido | Mana | Libro del Bushido | Nessuno |
| Ninjitsu | Ninjitsu | Ninjitsu | Mana | Libro del Ninjitsu | Nessuno |
| Monaco (Mystic) | Lotta | Lotta | Mana + punti di decima | Tomo del monaco | Nessuno; regole di equipaggiamento severe |
| Canzoni del Bardo (Bard Songs) | Musica | Musica, Provocazione, Discordia, Pacificazione | Mana + usura dello strumento | Libro delle canzoni ("Bardic Songs") | Nessuno |
| Giullare (Jester) | Accattonaggio | Valutare Intelligenza, Accattonaggio | Mana + punti scherzo | Borsa dei trucchi | Nessuno; serve l'abito da giullare |
| Sant'Uomo (Holy Man) | Parlare con gli Spiriti | Guarigione | Mana + pietà | Libro delle preghiere + simbolo sacro | Karma ≥ 2500 |
| Cavaliere della Morte (Death Knight) | Cavalleria | Cavalleria + karma negativo | Mana + anime | Libro della magia della morte + lanterna delle anime | Karma ≤ 0 |
| Syth | Valutare Intelligenza / Scherma con Spada | Scherma con Spada, Tattica, karma negativo | Mana + frammenti infernali | Datacron Syth | Karma ≤ 0 |
| Jedi | Valutare Intelligenza / Scherma con Spada | Scherma con Spada, Tattica, karma positivo | Mana + cristalli karan | Datacron Jedi | Karma ≥ 0 (libro: karma ≥ 2500) |
| Ricerca arcana (Research) | La più alta fra Magia e Negromanzia (o Cavalleria con karma positivo) | Magia, Negromanzia, Parlare con gli Spiriti, Valutare Intelligenza | Mana + pergamene scritte in anticipo | Borsa della ricerca | Nessuno |

> **Consiglio:** i personaggi con la specializzazione **Alchimista** (classe personalizzata) non possono lanciare *nessun* incantesimo: il gioco risponde "You can't seem to grasp magic forces!". Se vuoi fare il mago, non scegliere quella classe.

---

## 8.2 Come si lancia un incantesimo

1. Apri il libro (doppio clic) e clicca sull'icona dell'incantesimo, oppure usa una macro o una barra rapida.
2. Il personaggio pronuncia le parole di potere e muove le mani per il **tempo di lancio**. In questo intervallo resta fermo: non puoi camminare.
3. Alla fine compare il cursore di mira (se l'incantesimo ha un bersaglio). Selezioni il bersaglio.
4. Solo ora il gioco controlla mana e reagenti, tira il **controllo di abilità** e, se riesce, consuma mana e reagenti e applica l'effetto. Se fallisce, l'incantesimo "svanisce" (*The spell fizzles*) e i reagenti non vengono consumati.

Condizioni che impediscono di iniziare un lancio:

- sei paralizzato, congelato o "calmato" da un bardo;
- non ti sei ancora ripreso dal lancio precedente (*You have not yet recovered from casting a spell*);
- non hai abbastanza mana;
- sei trasformato in **Bestia Orrenda** (Horrific Beast) o, per alcuni incantesimi, in forma animale;
- sei morto o in stato "benedetto" (invulnerabile), salvo eccezioni come *Seduta Spiritica* e *Proiezione Astrale*.

### Effetti curiosi

- **Ubriachezza.** Se il tuo tasso alcolico (BAC) è alto, c'è una probabilità pari a BAC/200 che tu biascichi le parole di potere: l'incantesimo non parte.
- **Aglio e vampiri.** Se sei sotto **Abbraccio Vampirico** e lanci un incantesimo che usa l'Aglio, subisci 17–23 danni fisici.
- **Città.** Alcuni incantesimi dannosi o di evocazione (campi, terremoti, evocazioni) non si possono lanciare nelle zone protette delle città.

---

## 8.3 Mana

Il mana è la riserva di energia magica. Il massimo dipende dall'Intelligenza (più eventuali bonus degli oggetti).

### Costo in mana

| Cerchio di Magia | 1° | 2° | 3° | 4° | 5° | 6° | 7° | 8° |
|---|---|---|---|---|---|---|---|---|
| Mana | 4 | 6 | 9 | 11 | 14 | 20 | 40 | 50 |

Gli altri sistemi hanno un costo fisso per ogni incantesimo (vedi le tabelle dei singoli capitoli).

Il costo reale viene modificato così:

- **Costo Mana Ridotto** (*Lower Mana Cost*, LMC) degli oggetti: riduce il costo della percentuale indicata, con rendimenti decrescenti e un tetto del 90% (100% per la classe **Stregone**/Sorcerer).
- **Costo Reagenti Ridotto** (*Lower Reagent Cost*, LRC): ti fa risparmiare i reagenti, **ma in questo shard aumenta il costo in mana** di metà della sua percentuale (LRC 100% = +50% di mana).
- **Marcescenza Mentale** (Mind Rot, negromanzia) aumenta il costo del bersaglio colpito.
- Il costo non scende mai sotto il 10% del valore base.

> **Consiglio:** LRC al 100% significa niente reagenti, ma ogni Colonna di Fuoco ti costa 60 mana invece di 40. Bilancia LRC e LMC: un buon equipaggiamento da mago ha entrambi.

### Recupero del mana

Il mana si rigenera da solo. In base al codice (sistema AOS) il gioco calcola dei "punti di rigenerazione":

- **Meditazione:** (Intelligenza + 3 × Meditazione) × 0,025 (× 0,0275 da 100 di Meditazione in su).
- **Concentrazione:** Concentrazione × 0,05.
- **Mentre mediti attivamente** (usando l'abilità Meditazione): aggiungi di nuovo i punti di Meditazione, fino a un massimo di 13 (17 per lo Stregone).
- **Oggetti con Rigenerazione Mana:** tetto di 18 punti dall'equipaggiamento. Forma di Lich +18, Abbraccio Vampirico +5, cavalcare un Kirin +10.
- **Fame e sete:** con fame o sete sotto 10 la rigenerazione cala (÷1,5; sotto 3 ÷2). Con la sete sopra 20 ottieni fino al +25%; un po' di alcol dà fino al +50%.

Il tempo per recuperare 1 punto di mana è circa `1 / (0,1 × (2 + punti))` secondi.

*Esempio:* Intelligenza 100, Meditazione 100, Concentrazione 100. Punti = 11 + 5 = 16: recuperi circa 1,8 mana al secondo. Se mediti attivamente salgono a 27: circa 2,9 mana al secondo.

> **Consiglio:** qualsiasi armatura che **blocca la meditazione** (quasi tutto il metallo, molte armature di cuoio borchiato) **azzera** il bonus di Meditazione, non lo riduce soltanto. Un mago in armatura a piastre recupera il mana solo grazie a Concentrazione e oggetti.

### Altri modi per recuperare mana

- Pozioni di mana e di ringiovanimento (capitolo 9).
- **Prosciuga Mana** e **Vampiro di Mana** (Magia), **Spegni Spirito** (Sant'Uomo), la miscela **Sanguisuga del Lich** (alchimia necrotica).
- **Parola dei Morti** (Ricerca arcana) sui cadaveri di creature sovrannaturali.
- **Teschio malvagio** (*evil skull*): si sbriciola e ripristina il mana.
- La canzone **Ballata del Mago** del bardo rigenera il mana del gruppo.

---

## 8.4 Reagenti

Ogni incantesimo di Magia e Negromanzia consuma **un'unità di ciascun reagente** indicato.

| Reagente | Sistemi che lo usano | Dove trovarlo |
|---|---|---|
| Perla Nera (Black Pearl) | Magia, alchimia | Maghi, alchimisti, erboristi; bottino |
| Muschio Sanguigno (Bloodmoss) | Magia, alchimia | Come sopra |
| Aglio (Garlic) | Magia, alchimia | Come sopra |
| Ginseng | Magia, alchimia | Come sopra |
| Radice di Mandragora (Mandrake Root) | Magia, alchimia | Come sopra |
| Belladonna (Nightshade) | Magia, alchimia | Come sopra |
| Seta di Ragno (Spider's Silk) | Magia, alchimia | Come sopra |
| Cenere Sulfurea (Sulfurous Ash) | Magia, alchimia | Come sopra |
| Ala di Pipistrello (Bat Wing) | Negromanzia | Maghi, negromanti, streghe |
| Polvere di Tomba (Grave Dust) | Negromanzia | Come sopra |
| Sangue di Demone (Daemon Blood) | Negromanzia | Come sopra |
| Cristallo di Nox (Nox Crystal) | Negromanzia | Come sopra |
| Ghisa (Pig Iron) | Negromanzia | Come sopra |

I reagenti "esotici" (Cristallo Lunare, Uovo di Fata, Teschio di Folletto, Orecchio di Gargoyle, Occhio di Rospo, Loto Rosso, ecc.) servono per scrivere gli incantesimi antichi della Ricerca arcana, per l'alchimia avanzata (elisir, misture) e per le pozioni dei negromanti e dei druidi.

Alternative ai reagenti:

- **Pergamene:** lanciare da pergamena non consuma reagenti (vedi 8.6).
- **Gemma arcana** (Arcane Gem): se ti mancano i reagenti, l'incantesimo consuma 1 carica della gemma (Magia e Negromanzia).
- **Costo Reagenti Ridotto:** probabilità pari alla percentuale di non consumare reagenti.
- Le bacchette e i bastoni magici non usano reagenti né abilità (vedi 8.11).

> **Consiglio:** il mago vende una **borsa di reagenti** (*bag of reagents*) a 500 monete: contiene 50 unità di ciascuno degli 8 reagenti base. All'inizio è il modo più economico per partire.

---

## 8.5 Libri degli incantesimi

Un incantesimo si può lanciare dal libro solo se il libro lo contiene. Per aggiungerne uno, trascina la pergamena sul libro.

| Libro | Dove si compra | Prezzo indicativo |
|---|---|---|
| Libro degli incantesimi (Magia) | Maghi, gilda dei maghi, saggi | 18 monete (vuoto) |
| Libro di negromanzia | Negromanti, gilda dei negromanti, maghi | 115 monete |
| Libro della Cavalleria | Custode della Cavalleria (Keeper of Chivalry) | 140 monete (completo) |
| Libro del Bushido, Libro del Ninjitsu | Maestro di arti marziali (Kung Fu) | 750 monete ciascuno |
| Tomo del monaco (Mystic) | Maestro di arti marziali (Kung Fu) | 750 monete |
| Libro delle canzoni (Bardic Songs) | Bardi, gilda dei bardi | 24–240 monete |
| Borsa dei trucchi del giullare | Giullari nelle taverne e sulle navi | 200 monete |
| Libro delle preghiere (Holy Man) | Dato dal sacerdote; talvolta in vendita | vedi 08d |
| Libro della magia della morte (Death Knight) | Tomba di Kas; talvolta in vendita | vedi 08d |
| Datacron Syth / Jedi | Tombe di Malak e di Zoda | vedi 08d |
| Borsa della ricerca | Bibliotecari/scribi | 500 monete |

Alcuni libri (Cavaliere della Morte, Sant'Uomo, Syth, Jedi, Monaco, borsa della ricerca) sono **legati al proprietario**: solo chi li ha ottenuti può usarli, e ottenerne uno nuovo fa svanire il precedente.

Un libro degli incantesimi equipaggiato può avere proprietà magiche (Aumento Danno Incantesimi, Lancio Rapido, ecc.) e perfino un'**uccisione** (*slayer*): contro il tipo di creatura giusto i tuoi incantesimi fanno **danno doppio**.

---

## 8.6 Pergamene

- Le **pergamene** (*scrolls*) si trovano nel bottino, si comprano o si scrivono con **Scrittura Arcana**.
- Si possono **leggere direttamente** (doppio clic): l'incantesimo parte senza reagenti e la pergamena si consuma.
- Lanciare una pergamena di Magia è **più facile di due cerchi**: una pergamena di 7° cerchio ha la difficoltà di un incantesimo di 5°.
- Il mago vende solo le pergamene dei **primi 3 cerchi**, la gilda dei maghi quelle dei **primi 4**. Gli altri cerchi vanno trovati nei dungeon (forzieri, librerie esaminate con il monocolo, bottino), comprati da altri giocatori o scritti.
- I negromanti vendono solo **Giuramento di Sangue**, **Pelle di Cadavere** e **Arma Maledetta**.

### Scrivere pergamene (Scrittura Arcana)

Con l'abilità Scrittura Arcana, una penna (*scribe's pen*) e una pergamena bianca puoi copiare gli incantesimi **che hai già nel libro**. Servono i reagenti dell'incantesimo e il suo costo in mana.

| Cerchio | Scrittura Arcana: minima / successo garantito | Mana |
|---|---|---|
| 1° | -25 / 25 | 4 |
| 2° | -10,8 / 39,2 | 6 |
| 3° | 3,5 / 53,5 | 9 |
| 4° | 17,8 / 67,8 | 11 |
| 5° | 32,1 / 82,1 | 14 |
| 6° | 46,4 / 96,4 | 20 |
| 7° | 60,7 / 110,7 | 40 |
| 8° | 75 / 125 | 50 |

Le pergamene di negromanzia richiedono l'abilità minima dell'incantesimo meno 0,4 (per esempio 69,6 per Forma di Lich).

> **Consiglio:** se trovi una pergamena di alto cerchio che non sai ancora lanciare dal libro, puoi comunque leggerla (è due cerchi più facile) oppure ricopiarla per venderla. All'inizio è un ottimo modo per guadagnare.

---

## 8.7 Probabilità di successo e allenamento

### Magia

Per gli incantesimi di Magia il gioco usa un intervallo di abilità per ogni cerchio. Sotto il minimo fallisci sempre, sopra il massimo riesci sempre; nel mezzo la probabilità cresce in modo lineare. **Guadagni abilità solo lanciando incantesimi all'interno di questo intervallo.**

| Cerchio | Abilità minima (0%) | Successo garantito (100%) | Con pergamena |
|---|---|---|---|
| 1° | -20 | 20 | sempre riuscito |
| 2° | -5,7 | 34,3 | sempre riuscito |
| 3° | 8,6 | 48,6 | come 1° |
| 4° | 22,9 | 62,9 | come 2° |
| 5° | 37,1 | 77,1 | come 3° |
| 6° | 51,4 | 91,4 | come 4° |
| 7° | 65,7 | 105,7 | come 5° |
| 8° | 80 | 120 | come 6° |

### Altri sistemi

| Sistema | Intervallo di successo (min → max) |
|---|---|
| Negromanzia | abilità richiesta → +40 (con pergamena: sempre all'abilità richiesta) |
| Cavalleria, Monaco, Giullare | abilità richiesta → +50 |
| Canzoni del Bardo | abilità richiesta → +30 |
| Bushido, Ninjitsu | abilità richiesta −12,5 → +37,5 |
| Cavaliere della Morte, Sant'Uomo, Syth, Jedi, Ricerca arcana | **nessun tiro**: se hai i requisiti (abilità, mana, risorsa) l'incantesimo riesce sempre |

> **Attenzione:** proprio perché non fanno un tiro di abilità, le magie del Cavaliere della Morte, del Sant'Uomo, dei Syth e dei Jedi **non fanno salire l'abilità** quando le usi. Devi allenare Cavalleria, Parlare con gli Spiriti, Valutare Intelligenza e Scherma con Spada in altri modi. Gli incantesimi antichi della Ricerca arcana invece allenano Magia, Negromanzia, Parlare con gli Spiriti e Valutare Intelligenza quando consumi la pergamena.

### Percorso di allenamento consigliato per la Magia

| Abilità | Cosa lanciare |
|---|---|
| 0–30 | Comprala dal maestro fino a 30, poi Goffaggine, Indebolimento, Freccia Magica, Cura |
| 30–45 | Agilità, Astuzia, Forza, Protezione (2°) poi Benedizione (3°) |
| 45–55 | Purificazione Superiore, Cura Superiore, Fulmine (4°) |
| 55–65 | Riflesso Magico, Paralisi, Incognito (5°) |
| 65–75 | Invisibilità, Rivela, Dardo di Energia (6°) |
| 75–90 | Colonna di Fuoco, Vampiro di Mana, Campo di Energia (7°) |
| 90–120 | Terremoto, Vortice di Energia, Elementali, Resurrezione (8°) |

> **Consiglio:** per allenarti senza rischi, lancia gli incantesimi su te stesso (Cura, Benedizione, Riflesso Magico, Invisibilità) o su un manichino in casa. Vampiro di Mana su un animale innocuo fa salire anche Resistenza Magica... a lui.

---

## 8.8 Tempo di lancio, recupero e interruzioni

### Tempo di lancio

| Cerchio | 1° | 2° | 3° | 4° | 5° | 6° | 7° | 8° |
|---|---|---|---|---|---|---|---|---|
| Tempo base (s) | 0,75 | 1,0 | 1,25 | 1,5 | 1,75 | 2,0 | 2,25 | 2,5 |

- **Lancio Rapido** (*Faster Casting*, FC): −0,25 s per punto. Tetto: 2 (4 per chi ha Cavalleria ≥ 70 senza Magia o Negromanzia ≥ 70, solo per gli incantesimi di Cavalleria). Tempo minimo 0,25 s.
- **Protezione** e **Protezione Superiore** tolgono 2 punti di FC finché sono attive.
- Evoca Creatura e Spiriti delle Lame hanno un tempo di lancio molto più lungo (fino a 5 volte).
- Ogni sistema ha tempi propri: per esempio Cavaliere della Morte 1 s, Sant'Uomo 3 s, canzoni del bardo 2–6 s.

### Recupero tra un lancio e l'altro

`(6 − Recupero Lancio Rapido) / 4` secondi. Il **Recupero Lancio Rapido** (FCR) ha un tetto di 6 (= nessuna attesa). Monaco e Giullare hanno un recupero base molto breve.

### Interruzioni

Se subisci danno mentre stai lanciando, l'incantesimo viene interrotto (*Your concentration is disturbed*) e mana e reagenti **non** vengono consumati. Vale anche se equipaggi o usi un oggetto durante il lancio.

Come evitarlo:

- **Protezione / Protezione Superiore:** finché sono attive **non vieni mai interrotto** dal danno. In cambio perdi 2 punti di Lancio Rapido, resistenza fisica (−15 + Scrittura Arcana/20) e Resistenza Magica (−35 + Scrittura Arcana/20). Si attivano e disattivano rilanciandole.
- **Concentrazione a 125 (valore base):** il danno non interrompe più alcun incantesimo.
- I mostri e gli animali non vengono mai interrotti.
- Gli incantesimi lanciati da un **frammento d'anima** (Soul Shard) del filatterio non possono essere interrotti e non costano mana.

> **Consiglio:** con Scrittura Arcana a 100 le penalità di Protezione scendono quasi a zero (resistenza fisica −10, Resistenza Magica −30). Per un mago da combattimento è l'incantesimo più importante del 2° cerchio.

---

## 8.9 Abilità che influenzano danno ed effetti

### Formula del danno (Magia e incantesimi simili)

Il danno dei principali incantesimi offensivi segue questi passi:

1. **Danno base** = dadi dell'incantesimo (per esempio Colonna di Fuoco 1d6 + 55).
2. **Bonus percentuale** = Scrittura Arcana + Intelligenza/10 + Aumento Danno Incantesimi (SDI).
   - Scrittura Arcana: fino al 5% sotto 100, **10% a 100 e oltre**.
   - SDI: tetto 300% contro i mostri, 100% contro i giocatori.
3. **Valutare Intelligenza**: moltiplica il risultato per `30% + 0,9 × Valutare Intelligenza` (120% a 100, 138% a 120).
4. **Modificatori del bersaglio:** uccisione del libro (×2), Evasione del Bushido (annulla il danno), regione.
5. **Bonus di Magia di Ultima Adventures:** molti incantesimi aggiungono un danno fisso in base alla Magia del lanciatore (per esempio Colonna di Fuoco +Magia/5, Dardo di Energia +Magia/7). Questo bonus si somma **dopo** i passi precedenti ed è uno dei motivi per cui qui la Magia alta conta molto più che nell'UO classico.

Infine il danno viene ridotto dalla **resistenza elementale** del bersaglio (fuoco, freddo, veleno, energia, fisico).

### Resistenza Magica

- Riduce la probabilità di subire per intero maledizioni, paralisi, veleno e simili: alcuni effetti vengono dimezzati o annullati (*You feel yourself resisting magical energy*).
- La probabilità di resistere a un incantesimo di Magia è circa il massimo tra `Resistenza/5` e `Resistenza − ((Magia del lanciatore − 20)/5 + 5 × cerchio)`, il tutto dimezzato.
- Riduce la durata di Paralisi e di molti effetti negativi della Negromanzia.
- Si allena subendo incantesimi: i cerchi alti la fanno salire fino a valori più alti.

### Altre abilità

| Abilità | Effetto sulla magia |
|---|---|
| Valutare Intelligenza | Danno della Magia; durata ed entità di benedizioni e maledizioni (Agilità, Forza, Maledizione...); durata di Paralisi |
| Scrittura Arcana | Bonus al danno; riduce le penalità di Protezione; serve per scrivere pergamene, bacchette e incantesimi antichi |
| Meditazione | Rigenerazione del mana |
| Concentrazione | Rigenerazione di mana e vigore; a 125 annulla le interruzioni |
| Parlare con gli Spiriti | "Valutare Intelligenza" dei negromanti; abilità di lancio del Sant'Uomo |
| Avvelenamento | Con Magia: livello del veleno dell'incantesimo Veleno |
| Resistenza Magica | Difesa contro gli incantesimi |

### Il bonus "Puritano" (Midland)

Nella terra di **Midland**, e per i personaggi "puritani", le proprietà magiche degli oggetti (Aumento Danno Incantesimi, Lancio Rapido, Recupero Lancio Rapido, Rigenerazione Mana) **non funzionano**. Al loro posto conta la **lucidità**: quanto sei libero da armatura pesante e carico. Con lucidità alta ottieni fino a +3 di recupero e +2 di lancio rapido e un bonus al danno basato sull'Intelligenza.

### Classe Stregone (Sorcerer)

La specializzazione Stregone ha: costo mana ridotto fino al 100%, Meditazione più efficace, Cura Superiore +25%, Benedizione più forte e più lunga (+2 minuti).

---

## 8.10 Karma e magia

- La **Negromanzia** abbassa il tuo karma a ogni lancio riuscito. Se il tuo karma è positivo, l'incantesimo "riesce" (fai pratica) ma **non ha alcun effetto** (*your call to the dead remains unanswered*).
- La **Cavalleria** con karma negativo ha lo stesso destino: niente effetto (*no one answers*).
- Il **Cavaliere della Morte** e i **Syth** richiedono karma ≤ 0 e diventano più forti quanto più il karma è basso.
- Il **Sant'Uomo** richiede karma ≥ 2500; i **Jedi** karma ≥ 0.
- Con karma molto negativo (circa −4000) le guardie cittadine ti attaccano.

> **Consiglio:** con karma esattamente 0 puoi usare sia Negromanzia sia Cavalleria. Ma ogni incantesimo negromantico ti fa scendere sotto lo zero: tienilo presente se vuoi essere anche paladino.

---

## 8.11 Oggetti magici che lanciano incantesimi

- **Bacchette e bastoni magici** (*magic staff/wand*): contengono cariche di un incantesimo di Magia. Si usano **senza abilità, senza reagenti** e senza togliere l'arma di mano. Si fabbricano con **Scrittura Arcana** (sezione bacchette) usando lingotti speciali, oppure si trovano nel bottino.
- **Veste del teletrasporto** e altri oggetti magici speciali: lanciano il loro effetto senza abilità.
- Alcuni manufatti hanno incantesimi propri, non lanciabili dal libro:

| Nome italiano (inglese) | Effetto |
|---|---|
| Evoca Serpenti (Summon Snakes) | Evoca serpenti alleati |
| Evoca Drago (Summon Dragon) | Evoca un drago alleato |
| Fulmine di Thor (Thor Lightning) | Colpisce con un fulmine |

- **Filatterio e frammenti d'anima** (Phylactery, Soul Shard): i frammenti lanciano incantesimi senza costo in mana e senza interruzioni; la potenza dipende dal livello del filatterio invece che dalla tua Magia.
- **Fiasca di ferro** (*iron flask*, 500 monete dal mago): serve all'incantesimo **Serratura Magica** per intrappolare una creatura (vedi 08a).

---

## 8.12 Barre rapide e comandi

Quasi tutti i sistemi speciali hanno comandi da digitare (con la parentesi quadra iniziale) che lanciano direttamente l'incantesimo o aprono barre rapide. Esempi:

- Cavaliere della Morte: `[deathspell1`, `[deathspell2` (editor barre), `[deathtool1`, `[deathtool2` (apri barra), `[deathclose1`, `[deathclose2`; ogni incantesimo ha un comando, per esempio `[Banish`, `[Wrath`.
- Sant'Uomo: `[holyspell1`, `[holytool1`, `[holyclose1` (e varianti 2); `[Smite`, `[Rebirth`...
- Giullare: `[CanOfSnakes`, `[Clowns`, `[Hilarity`...
- Bardo: `[ArmysPaeon`, `[FoeRequiem`, `[ShephardsDance`...
- Ricerca arcana: `[CastFlameBolt`, `[CastHailStorm`... (un comando per ogni incantesimo antico).

I comandi esatti sono indicati nelle tabelle dei capitoli successivi.
