# 7. L'artigianato

Creare oggetti (crafting) è uno dei pilastri di Ultima Adventures. Con l'artigianato puoi:

- equipaggiarti da solo;
- guadagnare oro;
- ottenere ricompense uniche con gli **ordini di massa**;
- avere un **Negozio** in casa che lavora per te.

Questo capitolo spiega come funziona il sistema in generale (7.1), dove trovare le risorse (7.2), cosa offrono i diversi materiali (7.3), la qualità eccezionale e gli attrezzi runici (7.4). Poi descrive ogni mestiere, uno per uno, con le categorie di oggetti e le soglie di abilità (7.5–7.17). Infine tratta la riparazione, i miglioramenti, gli ordini di massa e i Negozi (7.18–7.21).

Le ricette sono estratte dai file di definizione del server (`Def*.cs`). Le pozioni sono descritte in dettaglio nel [capitolo 9](09-alchimia-e-pozioni.md).

---

## 7.1 Come si crea un oggetto

### Passo per passo

1. Procurati l'**attrezzo** del mestiere (martello da fabbro, kit da cucito, sega, ecc.) e mettilo nello zaino.
2. Procurati i **materiali** (lingotti, cuoio, assi, reagenti...) e tienili nello zaino.
3. Se serve, avvicinati alla **postazione** richiesta: incudine e forgia per il fabbro, una fonte di calore o un forno per il cuoco.
4. Fai **doppio clic** sull'attrezzo: si apre il menu del mestiere.
5. A sinistra scegli la **categoria**, a destra l'**oggetto**. Il pulsante con la lente mostra i materiali necessari e la tua probabilità di riuscita.
6. Se il mestiere usa più materiali (metalli, legni, cuoi), scegli quello da usare con il pulsante **"Materiale"** in basso.
7. Clicca il pulsante accanto all'oggetto per crearlo. **"Crea ultimo"** (Make Last) ripete l'ultima creazione.

Ogni attrezzo ha un numero di **utilizzi** e alla fine si consuma: tienine sempre uno di scorta. I meccanici fabbricano quasi tutti gli attrezzi (7.11).

### Riuscita

Ogni oggetto ha un'**abilità minima** e un'**abilità massima**:

- sotto il minimo non puoi crearlo;
- al massimo riesci sempre;
- in mezzo la probabilità sale in modo lineare.

Il punto di partenza al minimo dipende dal mestiere:

| Probabilità di riuscita al minimo | Mestieri |
|---|---|
| **0%** | Fabbro, Alchimia, Cucina, Scrittura Arcana, Cartografia, Meccanica (per la maggior parte degli oggetti) |
| **50%** | Sartoria, Carpenteria, Costruzione Archi, Lavorazione del legno (scaffali), Bacchette magiche |

Se fallisci puoi perdere una parte dei materiali.

### Guadagnare abilità

**Ogni tentativo**, riuscito o no, fa un tiro di guadagno, purché tu sia tra il minimo e il massimo dell'oggetto. Se l'oggetto richiede due abilità (per esempio Carpenteria + Sartoria per un manichino), le alleni entrambe.

> **Consiglio:** per allenarti scegli oggetti con riuscita tra il 50% e il 75%: imparano bene e sprecano pochi materiali. Gli oggetti troppo facili non insegnano più niente.

---

## 7.2 Le risorse e dove trovarle

### Minerali (Estrazione Mineraria)

**Come si fa:**

1. Prendi un **piccone** o una **pala** (anche la **pala da minerale**).
2. Doppio clic sull'attrezzo, poi bersaglia una parete di montagna o il pavimento di una grotta. Non si può fare a cavallo.
3. Porta il minerale grezzo a una **forgia**: doppio clic sul minerale e poi sulla forgia per **fonderlo** in lingotti.

In alternativa di' *"I wish to start mining"* per l'estrazione automatica.

**Fusione**: la riuscita è una prova di Estrazione tra difficoltà − 25 e difficoltà + 25. I metalli con difficoltà oltre 50 richiedono almeno quella Estrazione, altrimenti appare *"You have no idea how to smelt this strange ore!"*. Se fallisci perdi metà del minerale.

| Minerale | Estrazione per scavarlo | Fusione (difficoltà) | Frequenza nelle vene |
|---|---|---|---|
| Ferro (Iron) | 0 | 50 | 45% |
| Rame opaco (Dull Copper) | 65 | 65 | 15% |
| Ferro ombra (Shadow Iron) | 70 | 70 | 11% |
| Rame (Copper) | 75 | 75 | 8% |
| Bronzo (Bronze) | 80 | 80 | 6% |
| Oro (Gold) | 85 | 85 | 5% |
| Agapite | 90 | 90 | 4% |
| Verite | 95 | 95 | 3% |
| Valorite | 99 | 99 | 2% |
| Nanico (Dwarven) | 100,1 | 101 | 1% |

**Metalli rari**: scavando agapite, verite o valorite hai il 50% di probabilità che il minerale si trasformi in un metallo raro, se sei nel posto giusto:

| Metallo raro | Dove |
|---|---|
| **Mithril** | the Underworld |
| **Xormite** | the Underworld, sulla mappa TerMur |
| **Ossidiana** (Obsidian) | Serpent Island |
| **Nepturite** | aree marine, grotte e moli dei pirati (Shipwreck Grotto, Barnacled Cavern, Savage Sea Docks, ecc.) |
| **Acciaio** (Steel) | bottino nel Savaged Empire (1 lingotto su 10 trovati) |
| **Ottone** (Brass) | bottino a Umber Veil (1 lingotto su 8 trovati) |
| Mithril (bottino) | 1 lingotto su 8 trovati nel bottino dell'Underworld |

Nelle **Mines of Morinia** due volte su tre il minerale raccolto è doppio.

I **cristalli metallici** (crystalline ...) trovati come bottino si trasformano direttamente in lingotti con un doppio clic, se hai Fabbro ≥ 50. Esistono anche cristalli di pietre preziose (rubino, zaffiro, ametista...) che danno i **blocchi** usati dall'artigianato divino (7.16).

Con i libri appositi e Estrazione 100 puoi scavare anche:

| Risorsa | Libro | Prezzo del libro | Usata da |
|---|---|---|---|
| **Granito** | *Mining For Quality Stone*, dallo scalpellino | 25.625 monete | Lavorazione della pietra |
| **Sabbia** | *Finding Glass-Quality Sand*, dal vetraio | 20.637 monete | Soffiatura del vetro |

> **Consiglio:** un **cavallo da soma** o un lama da soma porta i minerali al posto tuo. Uno **scarabeo di fuoco** domato fonde il minerale direttamente.

### Legname (Taglialegna)

**Come si fa:**

1. Doppio clic su un'**ascia** (o accetta), poi su un albero.
2. I tronchi si trasformano in **assi** (boards) usando l'ascia o la sega sui tronchi.

L'automazione si avvia con *"I wish to start lumberjacking"* e *"I wish to start cutting logs"*.

| Legno | Taglialegna minimo |
|---|---|
| Normale (Log) | 0 |
| Frassino (Ash) | 55 |
| Ciliegio (Cherry) | 60 |
| Ebano (Ebony) | 65 |
| Quercia dorata (Golden Oak) | 70 |
| Hickory | 75 |
| Mogano (Mahogany) | 80 |
| Quercia (Oak) | 85 |
| Pino (Pine) | 90 |
| Palissandro (Rosewood) | 95 |
| Noce (Walnut) | 100 |
| Elfico (Elven) | 100,1 |

**Legni speciali** (solo in certi luoghi):

| Legno | Dove |
|---|---|
| **Legno fantasma** (Ghost) | nelle zone dei negromanti, al posto di noce, palissandro, pino o quercia |
| **Ebano** | nelle zone dei negromanti, al posto dei legni da frassino a mogano |
| **Legno pietrificato** (Petrified) | nell'Underworld |
| **Legno di deriva** (Driftwood) | nelle aree marine, nella Shipwreck Grotto e nella Barnacled Cavern |

### Pelli e cuoio (scuoiare)

**Come si fa:**

1. Uccidi un animale o un mostro.
2. Doppio clic su un **pugnale o coltello**, poi sul cadavere: ottieni carne, **pelli grezze** (hides), piume, lana, scaglie.
3. Taglia le pelli con le **forbici** per avere il **cuoio**.

**Medicina Legale** e **Anatomia** aumentano la quantità: +1 pezzo ogni 25 punti di ciascuna. L'automazione si avvia con *"I wish to start skinning"*.

| Cuoio | Sartoria minima | Da quali creature (esempi) |
|---|---|---|
| Normale | 0 | animali comuni (capre, cervi, orsi...) |
| Cornuto (Horned) | 60 | uomini lucertola, slith, rettiliani |
| Spinato (Barbed) | 65 | serpenti, naga, ofidi, viverne, draghi |
| Necrotico (Necrotic) | 70 | non-morti: mummie, vampiri, zombi marini |
| Vulcanico (Volcanic) | 75 | creature del fuoco: destrieri di fuoco, gatti infernali, draghi vulcanici |
| Ghiacciato (Frozen) | 80 | yeti, lupi invernali, leoni delle nevi, mammut, leopardi delle nevi, orsi polari |
| Coriaceo marino (Spined) | 85 | creature marine (aqueath, locathah) |
| Golia (Goliath) | 90 | giganti, giganti zombi e mummia, cose della palude |
| Draconico (Draconic) | 95 | draghi, drachi |
| Infernale (Hellish) | 99 | bestie infernali, incubi, chimere, cerberi |
| Dinosauro (Dinosaur) | 105 | dinosauri, rettili antichi |
| Alieno (Alien) | 120 | xenomorfi, osservatori |

**Pellicce** (furs) e pelli speciali (di demone, drago, incubo, serpente, troll, unicorno) servono per oggetti particolari e per l'artigianato divino.

### Scaglie

Si ottengono scuoiando i draghi e i rettili più grandi. Esistono rosse, gialle, nere, verdi, bianche, blu e di dinosauro. Servono al fabbro per le **armature a scaglie** (Scalemail).

### Stoffa

- Cotone, lino e lana si raccolgono nei campi o tosando le pecore.
- Si filano all'**arcolaio** e si tessono al **telaio**: ottieni un **rotolo di stoffa**.
- Le **forbici** trasformano i rotoli in stoffa, e la stoffa in **bende**.
- Si comprano dal sarto e dal tessitore.

### Reagenti, gemme e altro

- **Reagenti**: dai maghi, dagli alchimisti e da altri mercanti, o come bottino.
- **Gemme**: si trovano nel bottino e nei forzieri; servono ai gioielli e alle bacchette.
- **Cera d'api**: dagli alveari (apicoltura).
- **Pergamene vuote**: dalla Cartografia o dalla Scrittura Arcana (frammenti di corteccia), oppure comprate.

---

## 7.3 I materiali e i loro bonus

Il materiale cambia le **resistenze** delle armature e il **danno elementale** delle armi. Le armature di metallo prendono i bonus del metallo, quelle di cuoio i bonus del cuoio, gli oggetti di legno quelli del legno.

**Legenda:**

- **Fis/Fuo/Fre/Vel/Ene**: resistenza fisica, al fuoco, al freddo, al veleno, all'energia (armature).
- **Dur**: durabilità +%.
- **Req−**: requisiti di Forza ridotti del %.
- **Danno arma**: percentuale di danno convertita in quell'elemento.

### Metalli (Fabbro e Meccanica)

| Metallo | Fabbro min. | Fis | Fuo | Fre | Vel | Ene | Extra armatura | Arma |
|---|---|---|---|---|---|---|---|---|
| Ferro | 0 | – | – | – | – | – | – | – |
| Rame opaco | 65 | 2 | – | – | – | 5 | Dur +30, Req− 20 | Dur +30, Req− 50 |
| Ferro ombra | 70 | 2 | – | – | 2 | 5 | Dur +10 | 20% freddo, Dur +50 |
| Rame | 75 | 3 | 2 | – | 4 | 2 | – | 10% veleno, 20% energia |
| Bronzo | 80 | 3 | – | 5 | 2 | 3 | – | 40% fuoco |
| Oro | 85 | 4 | 5 | 2 | – | 4 | Fortuna +40, Req− 30 | Fortuna +40, Req− 50 |
| Agapite | 90 | 5 | 3 | 3 | 3 | 3 | – | 30% freddo, 20% energia |
| Verite | 95 | 5 | 4 | 4 | 5 | 1 | – | 40% veleno, 20% energia |
| Valorite | 99 | 6 | – | 5 | 5 | 5 | Dur +15 | 10% fuoco, 20% freddo, 10% veleno, 20% energia |
| Nepturite | 99 | 6 | – | 8 | 10 | – | Dur +20 | 25% freddo, 25% veleno |
| Ossidiana | 99 | 7 | 12 | 4 | 4 | 9 | Dur +25 | 20% fuoco, 10% energia |
| Acciaio | 99 | 5 | – | 3 | 3 | 2 | Dur +10, Req− 25 | Dur +10, Req− 25 |
| Ottone | 105 | 6 | – | 4 | 4 | 3 | Dur +10, Req− 45 | 20% fuoco, 20% energia, Req− 45 |
| Mithril | 110 | 15 | – | 15 | 9 | 9 | Fortuna +100, Dur +30, Req− 75 | 30% energia, Fortuna +100, Req− 75 |
| Xormite | 115 | 15 | 8 | 8 | 8 | 15 | Dur +20, Req− 75 | 30% danno diretto, Req− 75 |
| Nanico | 120 | 16 | 10 | 7 | – | – | Dur +35 | Dur +35 |

> **Attenzione:** il metallo nanico richiede **Fabbro 120**: senza le pergamene del potere fino a 120 non potrai mai lavorarlo. Ottone, Mithril e Xormite richiedono rispettivamente 105, 110 e 115.

### Legni (Carpenteria e Costruzione Archi)

| Legno | Abilità min. | Fis | Fuo | Fre | Vel | Ene | Extra |
|---|---|---|---|---|---|---|---|
| Normale | 0 | – | – | – | – | – | – |
| Frassino | 65 | 1 | 1 | 1 | 1 | 1 | arma: 5% di ogni elemento |
| Ciliegio | 70 | 1 | 2 | 1 | 2 | 1 | arma: 10% veleno, 20% energia |
| Ebano | 75 | 3 | 3 | – | – | 2 | Dur +20; arma: 20% freddo |
| Quercia dorata | 80 | 3 | 2 | 3 | – | 2 | Fortuna +40, Req− 30 (armi 50) |
| Hickory | 85 | 5 | – | 7 | – | – | Dur +20, Req− 20 (armi 50) |
| Mogano | 90 | 7 | 3 | – | 3 | 2 | arma: 10% veleno, 20% energia |
| Quercia | 95 | 3 | – | 5 | 5 | 3 | arma: 40% fuoco |
| Pino | 100 | 3 | 3 | 3 | 5 | 5 | arma: 30% freddo, 20% energia |
| Palissandro | 100 | 5 | 5 | 3 | 8 | 3 | arma: 40% veleno, 20% energia |
| Noce | 100 | 10 | – | 5 | 4 | 4 | Dur +10; arma: 4 elementi |
| Legno di deriva | 105 | 9 | – | 8 | 10 | 5 | Dur +5; arma: 4 elementi |
| Legno fantasma | 110 | 9 | 5 | 9 | 9 | 6 | arma: 25% freddo, 25% energia |
| Legno pietrificato | 115 | 10 | – | 5 | 5 | 10 | Dur +30; arma: 25% freddo |
| Elfico | 120 | 10 | 3 | – | 11 | 15 | Dur +25, Fortuna +100 |

### Cuoi (Sartoria)

| Cuoio | Sartoria min. | Fis | Fuo | Fre | Vel | Ene | Extra |
|---|---|---|---|---|---|---|---|
| Normale | 0 | – | – | – | – | – | – |
| Cornuto | 60 | 1 | – | 3 | 3 | – | Dur +10 |
| Spinato | 65 | 1 | 3 | 3 | 2 | – | Dur +20; arma: 70% veleno |
| Necrotico | 70 | 1 | – | 4 | 3 | 2 | Dur +5; arma: 50% fuoco |
| Vulcanico | 75 | 2 | 6 | – | 3 | – | Dur +15; arma: 50% fuoco |
| Ghiacciato | 80 | 2 | – | 6 | – | 4 | Dur +10; arma: 50% freddo |
| Coriaceo marino | 85 | – | – | 2 | 2 | 1 | Fortuna +40, Dur +5; arma: 20% veleno |
| Golia | 90 | 4 | 3 | 3 | – | 3 | Dur +25; arma: 25% energia |
| Draconico | 95 | 2 | 6 | – | 3 | 3 | Dur +25; arma: 25% fuoco |
| Infernale | 99 | 3 | – | 4 | 2 | 6 | Dur +10; arma: 50% fuoco |
| Dinosauro | 105 | 3 | 4 | 4 | 5 | – | Dur +40 |
| Alieno | 120 | 5 | 7 | 7 | 6 | 7 | Dur +30 |

> **Nota:** il cuoio "coriaceo marino" (Spined) richiede 85 di Sartoria pur essendo, nel codice, il cuoio colorato più debole. Le "armi" di cuoio sono i guanti da pugile e da lancio.

### Scaglie (Fabbro, armature a scaglie)

Tutte le scaglie si lavorano da 0. La soglia vera è quella dell'oggetto: da 68,9 a 85.

| Scaglie | Fis | Fuo | Fre | Vel | Ene | Extra |
|---|---|---|---|---|---|---|
| Rosse | 5 | 8 | 8 | 2 | 2 | – |
| Gialle | 5 | 4 | 4 | 4 | 4 | Oro trovato +4 |
| Nere | 6 | 3 | 3 | 9 | 9 | – |
| Verdi | 6 | 3 | 9 | 9 | 3 | – |
| Bianche | 6 | 9 | 3 | 3 | 9 | – |
| Blu | 9 | 9 | 3 | 3 | 6 | – |
| Dinosauro | 11 | 6 | 6 | 6 | 6 | – |

---

## 7.4 Qualità eccezionale, marchio e attrezzi runici

### Probabilità di oggetto eccezionale

Dalla probabilità di riuscita si sottrae una quota fissa:

| Mestieri | Probabilità di eccezionale |
|---|---|
| Fabbro, Sartoria, Cucina, Bacchette, Artigianato divino | riuscita − 60% (che scende fino a −45% man mano che l'abilità supera 95; −45% da 100 in su) |
| Costruzione Archi | riuscita × 0,5 − 10% |
| Altri | riuscita − 60% |

Quindi, **a Gran Maestro, su un oggetto che riesci sempre** hai circa il 55% di eccezionali come fabbro o sarto, e il 40% come arcaio.

> **Nota.** La vecchia guida dice che entrare nella Gilda dei Fabbri "aumenta la probabilità di eccezionale": nel codice non c'è un bonus diretto. Conta solo l'anello di gilda (+20 Fabbro), che alza l'abilità e quindi la probabilità.

### Cosa dà la qualità eccezionale

- **Armature eccezionali**:
  - +5 punti di resistenza distribuiti a caso (+8 con un attrezzo runico);
  - **+1 resistenza casuale ogni 15 punti di Conoscenza Armi** (6 a 90);
  - durabilità +20%.
- **Armi eccezionali**: più danno, più un bonus di danno pari a Conoscenza Armi ÷ 20 %.
- **Marchio dell'artefice**: a partire da 100 di abilità, un oggetto eccezionale può portare il tuo nome. Il gioco te lo chiede, a seconda dell'opzione "Marchio" nel menu.

### Attrezzi runici

I **martelli, kit da cucito, seghe e attrezzi runici** si ottengono soprattutto con gli **ordini di massa**. Sono legati a un materiale. Ogni oggetto creato con un attrezzo runico riceve **proprietà magiche casuali**:

| Materiale dell'attrezzo runico | Proprietà | Intensità |
|---|---|---|
| Rame opaco / Coriaceo marino / Frassino | 1 | 60–70% |
| Ferro ombra / Cornuto / Ciliegio | 1–2 | 70–80% |
| Rame / Spinato / Ebano | 2–3 | 70–80% |
| Bronzo / Necrotico / Quercia dorata | 2–4 | 70–80% |
| Oro / Vulcanico / Hickory | 3–4 | 70–80% |
| Agapite / Ghiacciato / Mogano | 3–5 | 80–90% |
| Verite / Golia / Quercia | 4–6 | 80–100% |
| Valorite / Draconico / Pino | 5–9 | 90–100% |
| Nepturite / Legno fantasma | 5–6 | 80–100% |
| Ossidiana / Palissandro | 5–6 | 85–100% |
| Acciaio / Infernale / Noce | 5–7 | 85–100% |
| Ottone / Legno pietrificato | 5–7 | 90–100% |
| Mithril / Dinosauro / Legno di deriva | 6–7 | 90–100% |
| Xormite | 6–7 | 100–120% |
| Nanico / Alieno / Elfico | 6–8 | 100–150% |

> **Consiglio:** l'obiettivo classico del fabbro è un martello runico per creare armi finché ne esce una con **Spell Channeling** (incanalamento magico).

---

## 7.5 Fabbro (Blacksmithy)

| Voce | Dettaglio |
|---|---|
| **Attrezzo** | Martello da fabbro (smith hammer), mazza da forgia (sledge hammer) o pinze (tongs) |
| **Postazione** | Entro 2 caselle da un'**incudine** e da una **forgia** |
| **Materiali** | Lingotti di metallo; scaglie per le armature a scaglie |
| **Riuscita al minimo** | 0% |
| **Opzioni** | Riparare, Fondere (resmelt: recuperi lingotti dagli oggetti), Marchio, Migliorare (enhance) |

### Categorie e soglie

| Categoria | Soglie (minimo) | Esempi |
|---|---|---|
| **Lingotti di valuta** (Currency Bars) | 0 | Lingotto d'oro (60.000 monete d'oro), d'argento (60.000 d'argento), di rame (60.000 di rame); pile da 3 e da 5. Utili per conservare grandi somme |
| **Componenti** | 0 | Chiodi |
| **Maglia, anelli, bande** | 12 – 39,1 | Guanti ad anelli 12; cuffia di maglia 14,5; maniche ad anelli 16,9; gambali ad anelli 19,4; tunica ad anelli 21,9; gambali di maglia 36,7; **tunica di maglia 39,1** |
| **Piastre** (Platemail) | 44,1 – 80 | Pettorale femminile 44,1; gorgiera 56,4; guanti 58,9; maniche 66,3; gambali 68,8; bardatura da cavallo 72,5 (650 lingotti); **pettorale 75**; armature samurai a piastre 65–80 |
| **Armatura reale** (Royal Armor) | 86,4 – 98 | Gorgiera 86,4; stivali e bracciali 88,9; elmo 92,6; mantello 96,3; gambali 96,8; **tunica 98** |
| **A scaglie** (Scalemail) | 68,9 – 85 | Guanti 68,9; elmo 72,6; maniche 76,3; gambali 78,8; tunica 85 (con scaglie) |
| **Elmi** | 8,3 – 90 | Bacinetto 8,3; elmo chiuso, elmo, elmo normanno 37,9; jingasa 45; elmo a piastre ed elmo del terrore 62,6; kabuto 90 |
| **Scudi** | −25 – 85 | Brocchiere −25; scudo rotondo −15,2; scudo di metallo −10,2; aquilone di metallo 4,6; scudo a goccia 24,3; scudi del campione, crestato, oscuro, elfico, della guardia, ingemmato e reale 54,3; scudi dell'Ordine e del Caos 85 |
| **Lame** (Bladed) | −0,4 – 80 | Pugnale −0,4; pugnale dell'assassino 10; coltellaccio da guerra 10; sciabola 24,3; spada lunga 28; scimitarra 31,7; spadone 35,4; kryss 36,7; katana 44,1; sai e wakizashi 50; spada reale 54,3; daisho 60; no-dachi 75; lajatang 80 |
| **Asce** | 24,2 – 39,1 | Accetta e ascia da boscaiolo 24,2; ascia doppia 29,3; ascia da battaglia 30,5; ascia del boia 34,2; ascia da guerra 39,1 |
| **Armi in asta** (Polearms) | 30 – 49 | Arpione 30; bardica 31,7; falce 39; alabarda 39,1; lancia corta 45,3; picca 47; lancia da cavaliere 48; lancia 49 |
| **Contundenti** (Bashing) | 14,5 – 85 | Mazza 14,5; maglio 19,4; scettro 21,4; mazza da guerra 28; martello da guerra 34,2; tessen 85 |

Le armature a scaglie richiedono anche Sartoria 50 per alcuni pezzi.

### Allenamento consigliato

| Fabbro | Cosa creare |
|---|---|
| 0–42 | compra dall'insegnante (fabbro o armaiolo) |
| 42–55 | tunica ad anelli (21,9), sciabole, spade lunghe |
| 55–65 | gambali e tunica di maglia (36,7–39,1) |
| 65–80 | gorgiera e guanti di piastre (56,4–58,9) |
| 80–95 | gambali di piastre (68,8), pettorale (75) |
| 95–100 | armatura reale (86,4–98) |
| 100+ | armi e armature con metalli colorati, ordini di massa |

> **Consiglio:** vendi le armature di metallo ai fabbri e agli armaioli: in proporzione ai lingotti usati rendono più delle armi. Se un mercante non le vuole, riprova qualche ora dopo: le richieste cambiano nel tempo.

---

## 7.6 Sartoria (Tailoring)

| Voce | Dettaglio |
|---|---|
| **Attrezzo** | Kit da cucito (sewing kit) |
| **Materiali** | Stoffa (vestiti), cuoio (armature di cuoio, borchiate, d'osso, scarpe, borse), pellicce, ossa |
| **Riuscita al minimo** | 50% |
| **Opzioni** | Riparare, Marchio, Migliorare |

### Categorie e soglie

| Categoria | Soglie | Esempi |
|---|---|---|
| **Pellicce** | 0 – 70,5 | Berretto di pelliccia 0; stivali 33,1; mantello e sarong 35; maniche 53,9; veste 55; gambali 66,3; tunica 70,5 (anche in pelliccia bianca) |
| **Cappelli** | 0 – 80 | Bandana e berretto 0; cappelli vari 6,2; cappello da giullare, da mago, cappucci, maschere 7,2; ghirlanda 10; kasa 60; cappuccio da ninja 80 |
| **Camicie e vesti** | 0 – 91,4 | Farsetto e tunica 0; abito da giullare 8,2; camicia 20,7; camicia elegante 24,8; mantello 41,4; kimono 50; **veste 53,9**; vesti speciali (da arcimago, assassino, negromante, sacerdote, vampiro, reale...) 70; giacca da ninja 75; mantello reale 91,4 |
| **Pantaloni** | 20,7 – 50 | Kilt 20,7; pantaloni lunghi e corti 24,8; gonna 29; sarong di pelliccia 35; hakama 50 |
| **Varie** | 0 – 74,6 | Otre 0; fascia 4,1; obi 20; grembiule 20,7 (intero 29); **guanti da pugile e da lancio 32,9**; borse da soma 40; stuoie 55; esca da pesca 60; tela cerata 74,6 |
| **Calzature** | 12,4 – 81,4 | Sandali 12,4; scarpe 16,5; stivali 33,1; stivali alti 41,4; stivali di pelliccia e barbarici 50; stivali di cuoio 63,1; tabi da ninja 70; stivali morbidi 81,4 |
| **Armatura di cuoio** | 6,2 – 90 | Berretto 6,2; jingasa 45; guanti 51,8; maniche e gorgiera 53,9; gonna 58; pettorale femminile 62,2; gambali e mantello 66,3; **tunica 70,5**; veste di cuoio 76,3; mempo 80; armature da ninja 80–90 |
| **Armatura borchiata** | 78,8 – 95 | Gorgiera 78,8; guanti 82,9; maniche 87,1; gambali 91,2; **tunica 94**; do 95 |
| **Armatura d'osso** | 85 – 96 | Elmo 85; guanti 89; elmo cornuto 90; maniche 92; gambali 95; tunica 96 |
| **Borse** | 0 – 90 | Borsello e sacca 0; zaino 8,2; zaino robusto 10,7; sacca grande 16,5; zaino da viaggio 20,7; sacca gigante 26; zaini da alchimista, minatore, boscaiolo, scuoiatore e borsello per monete 90 |

### Allenamento consigliato

| Sartoria | Cosa creare |
|---|---|
| 0–30 | bandane, berretti, farsetti, tuniche |
| 30–45 | camicie (20,7), camicie eleganti, pantaloni |
| 45–55 | mantelli (41,4), stivali alti |
| 55–70 | vesti (53,9), guanti e maniche di cuoio (51,8–53,9) |
| 70–80 | vesti speciali (70), tunica di cuoio (70,5) |
| 80–95 | armatura borchiata (78,8–94) |
| 95–100 | armatura d'osso (85–96), stivali morbidi, mantello reale |

> **Consiglio:** con l'anello della Gilda dei Sarti (+30) la Sartoria sale molto in fretta. Fai i **guanti da pugile o da lancio** il prima possibile: hanno sempre l'incanalamento magico.

---

## 7.7 Carpenteria (Carpentry)

| Voce | Dettaglio |
|---|---|
| **Attrezzi** | Sega, sega a coda di rondine, pialle, scalpello curvo, raschietto, coltello a due manici, martello, spaccalegna (froe) |
| **Materiali** | Assi (boards) di ogni legno, a volte stoffa, lingotti o reagenti |
| **Riuscita al minimo** | 50% |
| **Opzioni** | Riparare, Marchio, Migliorare |

### Categorie e soglie

| Categoria | Soglie | Esempi |
|---|---|---|
| **Altro** | 0 – 86,8 | Doghe di botte 0; coperchio 11; mestolo da calderone 30; catrame 35; frammenti di corteccia 40; pece 55; lanterne appese 65; **pertica di 10 piedi** 66,3 (sonda le trappole); canna da pesca 68,4; leggii 78,9; paraventi 80; cavalletto 86,8 |
| **Mobili** | 11 – 84,2 | Sgabello 11; sedia 21; sedia imbottita e comodino 42,1; panca e trono di legno 52,6; scrittoio 63,1; trono 73,6; tavoli bassi e sedia elfica 80; tavolo grande 84,2 |
| **Contenitori** | 10 – 90 | Cassetta piccola 10; scatola di legno 21; cassa media 31; libreria vuota 31,5; cassa grande 47,3; forziere di legno 73,6; armadi 84,2; forzieri rifiniti e mobiletti 90 |
| **Barili** | 57,8 – 87,8 | Barilotto 57,8; tinozza 70; tinozza alchemica 87,8 |
| **Armi e armature** | 52,6 – 80 | Scudo di legno 52,6; armatura di legno (gorgiera 56,4, guanti 58,9, elmo 62,6, maniche 66,3, gambali 68,8, tunica 75); bokuto 70; **bastone ferrato** 73,6; bastone nodoso, bastone da pastore e bastone druidico 78,9; tetsubo 80 |
| **Strumenti musicali** | 57,8 – 80 | Tamburo e tamburello 57,8; arpa da grembo 63,1; liuto 68,4; arpa 78,9; flauto di bambù 80 (con Musica 45–50) |
| **Accessori per la casa** | 15,7 – 100 | Bersaglio per freccette 15,7; letti 30–90; urna elettorale 47,3; bacheca 85; mattatoio e pentacolo 100 |
| **Fabbro** (accessori) | 73,6 – 78,9 | Incudine e forgia piccola 73,6; forgia grande 78,9 (con Fabbro) |
| **Addestramento** | 68,4 – 73,6 | Manichino d'addestramento 68,4; manichino da borseggio 73,6 |
| **Sartoria** (accessori) | 63,1 – 84,2 | Manichino da sarto 63,1; arcolaio 73,6; telaio 84,2 (con Sartoria) |
| **Cucina** (accessori) | 68,4 – 94,7 | Forno di pietra 68,4; mulino e abbeveratoio 94,7 |

### Lavorazione del legno: scaffali e mobili decorativi

Con gli **attrezzi da falegname** (woodworking tools) si apre un secondo menu con centinaia di mobili decorativi:

| Categoria | Soglie |
|---|---|
| Casse decorate per ogni mestiere | 10 – 47,3 |
| Cassettiere | 31,5 |
| Scaffali (bambù, sequoia, rustici, tinti, di legno), banconi | 41,5 |
| Armadi e mobiletti | 51,5 – 90 |
| Forzieri, bara, cassa da morto | 21 – 90 |

Alcuni pezzi richiedono anche Medicina Legale 75.

### Allenamento consigliato

| Carpenteria | Cosa creare |
|---|---|
| 0–42 | compra dall'insegnante, oppure doghe e sgabelli |
| 42–55 | casse medie (31), sedie imbottite (42,1) |
| 55–70 | casse grandi (47,3), **scudi di legno** (52,6, si vendono bene) |
| 70–85 | scrittoi (63,1), bastoni ferrati (73,6), forzieri (73,6) |
| 85–100 | armadi (84,2), tavoli grandi, mobiletti (90) |

---

## 7.8 Costruzione Archi (Bowcraft/Fletching)

| Voce | Dettaglio |
|---|---|
| **Attrezzo** | Attrezzi da arciere (fletcher tools) |
| **Materiali** | Tronchi (aste), piume (frecce), assi (archi) |
| **Riuscita al minimo** | 50% |
| **Eccezionale** | riuscita × 0,5 − 10% |

| Categoria | Soglie | Esempi |
|---|---|---|
| **Materiali** | 0 | Legna da ardere, **aste** (shaft) dai tronchi |
| **Munizioni** | 0 (massimo 40) | Frecce, dardi, dardi da fukiya, armi da lancio |
| **Armi** | 30 – 90 | Arco 30; archi lungo e corto dei boschi 50; balestra 60; arco composito 70; balestra pesante 80; balestra a ripetizione e yumi 90 |

### Allenamento consigliato

| Costruzione Archi | Cosa creare |
|---|---|
| 0–40 | aste e frecce (economiche e sempre utili) |
| 40–60 | archi (30–70) |
| 60–80 | balestre (60–100) |
| 80–100 | archi compositi (70–110), balestre pesanti (80–120) |
| 100+ | balestre a ripetizione e yumi (90–130) |

Costruzione Archi a 100 dà anche **+6,25% di danno** (più 0,625% per punto) con gli archi di legno.

---

## 7.9 Alchimia (Alchemy)

| Voce | Dettaglio |
|---|---|
| **Attrezzo** | Mortaio e pestello |
| **Materiali** | Reagenti e **bottiglie vuote** |
| **Riuscita al minimo** | 0% |

| Categoria | Soglie | Esempi |
|---|---|---|
| **Pozioni** | −25 – 90 | Cura minore, visione notturna, ristoro −25; purificazione minore −10; veleno minore −5; esplosione minore 5; agilità e cura 15; purificazione 25; esplosione 35; cura superiore e veleno mortale 55; purificazione ed esplosione superiori 65; veleno letale 75; bomba fumogena 90 |
| **Elisir delle abilità** | 60 | Uno per ogni abilità (Elisir di Alchimia, di Anatomia, ...): aumentano temporaneamente l'abilità |
| **Misture** | 50 – 95 | Liquidi dannosi (fuoco, ghiaccio, dolore, putrefazione) e melme alleate 50; olio del tecnomante 95 |
| **Trasmutazioni** | 30 – 110 | Trasformano un materiale nel successivo: legno → frassino 30; frassino → ciliegio 40; ferro → rame opaco 40; cuoio → spinato 40; ... ; oro → agapite 100; agapite → verite 110 |
| **Trasmutazione speciale** | 95 | Cristallo del potere |
| **Capelli** | 80 | Tinture e acconciature |

Tutti i dettagli (effetti, reagenti, durata, il bonus "Migliora Pozioni") sono nel [capitolo 9](09-alchimia-e-pozioni.md).

### Soffiatura del vetro (Glassblowing)

- **Requisiti**: Alchimia **100** e il libro *Crafting Glass With Glassblowing* (20.637 monete dal vetraio).
- **Attrezzo**: cannello (blowpipe), vicino a una forgia.
- **Materiale**: **sabbia** (Estrazione 100 con il suo libro).

| Oggetto | Soglia |
|---|---|
| Monocolo (per cercare libri e pergamene sulle librerie dei dungeon) | 5 |
| Bottiglie, fiale e barattoli | 52,5 |
| Fiasche (curva, lunga, grande, decorate) | 55 – 60 |
| Brocca di vetro | 60 |
| Rastrelliere di fiale | 65 |
| Clessidra | 75 |

---

## 7.10 Cucina (Cooking)

| Voce | Dettaglio |
|---|---|
| **Attrezzo** | Padella (skillet), mattarello (rolling pin), setaccio per farina (flour sifter) |
| **Postazione** | Fuoco o forno vicino, per le cotture |
| **Riuscita al minimo** | 0% |
| **Eccezionale** | Sì (come il fabbro) |

| Categoria | Soglie | Esempi |
|---|---|---|
| **Ingredienti** | 0 – 70 | Farina e impasto 0; olio da lampada 10; impasto dolce 50; preparati per torte e biscotti 70 |
| **Preparazioni** | 30 – 90 | Pizze crude e torte crude 30–40; sushi 50; wasabi 70; tè verde 80; bomba d'uova 90 |
| **Forno** | 30 – 80 | Pagnotta 30; zuppe di miso e pane al formaggio 40; torte salate, muffin, pane di mais 60; torta di mele, pizza 70; torta alla frutta, biscotti, torta 80 |
| **Barbecue** | 0 | Cosce di pollo, uccelli, bistecche di pesce, uova, cosce d'agnello, costolette |
| **Razioni** | 70 – 95 | Razioni piccole 70; grandi 90–95 |
| **Pesce e frutti di mare** | 45 – 70 | Filetto, bisque, granchio e aragosta al vapore 45; tortini e ravioli di granchio 70 |
| **Crocchette per animali** (Kibbles) | 10 – 50 | Dalla crocchetta di carne (10) a quella lunare (50) |
| **Halloween** | 80 – 99 | Zucche intagliate |
| **Cesti** | 40 | Cesto di frutta |

Il cibo cucinato ha un **"beneficio"**: ti permette di mangiare oltre la sazietà normale, e con lo stomaco più pieno **impari tutte le abilità più in fretta**, fino al doppio (capitolo 5).

### Allenamento consigliato

| Cucina | Cosa creare |
|---|---|
| 0–40 | barbecue (0–70), farina, impasto |
| 40–60 | pagnotte (30–80), pizze e torte crude |
| 60–80 | zuppe, torte salate, muffin (60–110) |
| 80–100 | biscotti, torte, razioni grandi |

### Cera e candele (apicoltura)

- **Strumento**: pentola per la cera (waxing pot), con la Cucina; la cera d'api viene dagli alveari.
- **Candele**: piccola 5, grande 15, colorabili 10–20, applique e candela-teschio 50, candela religiosa 80.
- **Lucidi** per strumenti, cuoio e metallo: 60.

### Erboristeria druidica

Usa la Cucina (≥ 40) con il calderone dell'erborista. Vedi capitoli 6 e 9.

---

## 7.11 Meccanica (Tinkering)

| Voce | Dettaglio |
|---|---|
| **Attrezzo** | Attrezzi da meccanico (tinker tools) |
| **Materiali** | Lingotti (ogni metallo, come il fabbro), tronchi o assi, gemme |
| **Riuscita al minimo** | 0% per la maggior parte (alcune ricette 50%) |
| **Opzioni** | Riparare, Marchio, Migliorare |

| Categoria | Soglie | Esempi |
|---|---|---|
| **Oggetti di legno** | −25 – 70 | Asse −25; telaio di orologio, pialle, mattarello 0; trappola da pesca 45; segheria 60; nunchaku 70 |
| **Attrezzi** | 5 – 75 | Forbici e cesoie 5; attrezzi da meccanico e kit da cucito 10; coltello da scuoiatore e da chirurgo 15; mortaio 20; penne da scriba e da cartografo 25; seghe, martelli, padella, accetta 30; attrezzi da arciere, pinze, pala da becchino, pala da minerale 35; **piccone, pala, martello da fabbro 40**; grimaldelli 45; setaccio 50; cannello e mazzuolo da scalpellino 55; attrezzi da trappola 75 |
| **Componenti** | −15 – 45 | Cerchi da botte −15; ingranaggi, cardini, molle 5; parti di orologio 25; parti di sestante 30; spina da botte 35; palle per bolas 45 |
| **Utensili** | 0 – 25 | Posate e piatti 0; calice e boccale 10; mannaia 20; coltello da macellaio 25 |
| **Varie** | 10 – 85 | Portachiavi 10; chiave 20; lanterna 30; candela grande 45; globo e candelabro 55; bilancia e telescopio 60; lanterne di carta 65; scacciapensieri 80 |
| **Gioielli** | 40 (nanici 99) | Anelli, bracciali, orecchini, amuleti di ogni metallo o pietra (ferro + gemma, oppure 2 lingotti del materiale) |
| **Assemblaggi** | 0 – 97,5 | Orologi, sestante 0; bolas 60; barilotto per pozioni 75; **scatole da scasso** da 75 ("dilapidated") a 97,5 ("harder") |
| **Ospitalità** | 60 – 95 | Barili di birra, sidro, latte e pressa per formaggio 60; botte di vino 80; botte di liquore 95 |

> **Consiglio:** il meccanico è l'artigiano "di supporto". Picconi, martelli, kit da cucito e penne servono a tutti gli altri mestieri. Le **scatole da scasso** sono l'allenamento ideale per Scassinare.

> **Attenzione:** i gioielli **nanici** (99) usano in realtà lingotti di **mithril**, anche se il menu li chiama "dwarven ingot".

### Allenamento consigliato

| Meccanica | Cosa creare |
|---|---|
| 0–30 | ingranaggi, cardini, molle (5), posate |
| 30–50 | attrezzi (forbici 5–55, attrezzi da meccanico 10–60), parti di orologio (25) |
| 50–70 | seghe, martelli (30–80), picconi (40–90) |
| 70–90 | grimaldelli (45–95), gioielli (40–90) |
| 90–100 | lanterne di carta, telescopi (60–110), scacciapensieri (80–130), scatole da scasso |

---

## 7.12 Scrittura Arcana (Inscription)

| Voce | Dettaglio |
|---|---|
| **Attrezzo** | Penna da scriba (scribe's pen) |
| **Materiali** | Pergamene vuote, reagenti, cera d'api, a volte cuoio, gemme ed elisir |
| **Riuscita al minimo** | 0% |
| **Nota** | Puoi scrivere una pergamena solo se l'incantesimo è nel tuo libro |

### Pergamene di Magia

| Cerchio | Soglia (min → max) |
|---|---|
| 1° | −25 → 25 |
| 2° | −10,8 → 39,2 |
| 3° | 3,5 → 53,5 |
| 4° | 17,8 → 67,8 |
| 5° | 32,1 → 82,1 |
| 6° | 46,4 → 96,4 |
| 7° | 60,7 → 110,7 |
| 8° | 75 → 125 |

### Pergamene di Negromanzia

| Pergamena | Soglia |
|---|---|
| Patto di Sangue, Pelle di Cadavere, Arma Maledetta, Cattivo Presagio, Fitta di Dolore | 19,6 |
| Marcescenza Mentale, Evoca Famiglio | 29,6 |
| Animare Morti, Forma Bestiale | 39,6 |
| Colpo Velenoso | 49,6 |
| Avvizzimento | 59,6 |
| Strangolamento | 64,6 |
| Forma di Lich | 69,6 |
| Forma Spettrale, Spirito Vendicativo, Esorcismo | 79,6 |
| Abbraccio del Vampiro | 98,6 |

### Altro

| Oggetto | Soglia | Note |
|---|---|---|
| Pergamena vuota (da frammenti di corteccia) | 40 | |
| Libro delle rune | 45 | 8 pergamene vuote, Richiamo, Portale, 5 cera |
| Libro degli incantesimi, libro di negromanzia, libro delle canzoni | 50 | 10 pergamene, 5 cera. A 125 escono **già completi** |
| Contratto scrivibile | 55 | |
| Libro degli ordini di massa | 65 | |
| **Pergamene delle canzoni del bardo** | 75 | Richiedono anche **Musica 95** e un liuto |
| **Libri di studio "Expert"** | 65 | Portano a 70. Richiedono anche **70 nell'abilità** del libro |
| **Libri di studio "Grandmaster"** | 65 | Portano a 100. Richiedono anche **100 nell'abilità**, gemme arcane e l'elisir di quell'abilità |

### Bacchette magiche (Tome of Wands)

- **Libro**: il *Tomo delle Bacchette* (Tome of Wands). Si trova sugli incantatori uccisi da chi ha Fortuna, con Scrittura Arcana e Magia ≥ 20. I maghi lo ricomprano a 100–400 monete.
- **Requisiti**: martello da fabbro nello zaino, vicino a **incudine e forgia**.
- **Abilità**: Scrittura Arcana più Magia.

| Cerchio | Scrittura Arcana / Magia minime | Lingotti (3 per bacchetta) |
|---|---|---|
| 1° – 4° | 20 / 30 / 40 / 50 | metalli colorati (da rame opaco a valorite) |
| 5° – 8° | 60 / 70 / 80 / 90 | blocchi di pietre preziose o metalli rari (rubino stellato, onice, granato, nepturite, ametista, ossidiana, smeraldo, zaffiro, mithril, argento, spinello, rubino, ottone, acciaio, giada, topazio, quarzo) |

Oltre ai lingotti, ogni bacchetta richiede **una gemma, una pergamena dell'incantesimo e una gemma arcana per ogni cerchio**: 1 di ciascuna al 1° cerchio, 5 al 5°, 8 all'8°.

### Allenamento consigliato

| Scrittura Arcana | Cosa creare |
|---|---|
| 0–40 | pergamene di 1°–3° cerchio |
| 40–60 | 4°–5° cerchio, pergamene vuote, libri delle rune (45) |
| 60–80 | 6° cerchio, libri degli incantesimi (50) |
| 80–100 | 7°–8° cerchio |
| 100+ | libri di studio, pergamene del bardo |

> **Consiglio:** il **monocolo** sulle librerie dei dungeon trova libri e pergamene gratis. A Gran Maestro la Scrittura Arcana dà **+10% di danno** a tutti gli incantesimi.

---

## 7.13 Cartografia (Cartography)

| Voce | Dettaglio |
|---|---|
| **Attrezzo** | Penna da cartografo (mapmaker's pen) |
| **Materiali** | Pergamene vuote, frammenti di corteccia |

| Oggetto | Soglia |
|---|---|
| Mappa vuota | 0 |
| Mappa piccola | 30 |
| Pergamena vuota (da corteccia) | 40 |
| Mappa grande | 60 |
| Carta nautica | 65 |
| Mappa enorme | 80 |
| Mappa del mondo | 89,5 |

**Mappe del tesoro**: per decifrarle servono 21 / 41 / 61 / 81 / 91 / 100 / 100 di Cartografia (livelli da 1 a 7). Poi scavi con piccone o pala: il raggio dipende da Estrazione Mineraria.

---

## 7.14 Lavorazione della pietra (Masonry)

- **Requisiti**: Carpenteria **100** e il libro *Making Valuables With Stonecrafting* (50.625 monete dallo scalpellino).
- **Attrezzo**: mazzuolo e scalpello.
- **Materiale**: **granito** (Estrazione 100 con il libro *Mining For Quality Stone*).

| Categoria | Soglie | Esempi |
|---|---|---|
| Decorazioni | 42,5 – 62,5 | Anfore, urne, vasi, sculture |
| Pietre tombali | 45 | 20 modelli |
| Mobili | 55 – 95 | Panche, gradini, tavoli, colonne, piedistalli, sarcofagi, camini (75), tavolo del mago (95) |
| Statue piccole / medie / grandi / enormi / giganti | 55 / 65 / 75 / 85 / 95 | Dagli angeli e grifoni fino al guardiano del portale |
| Contenitori | 80 – 95 | Urne, vasi, sarcofagi |

---

## 7.15 Altri mestieri e attività

| Attività | Abilità | Note |
|---|---|---|
| **Tassidermia** | Carpenteria ≥ 90 | Kit per impagliare i trofei di caccia |
| **Navi** | Carpenteria, Sartoria (vele), Meccanica (timone e fissaggi) | Sistema di costruzione navale |
| **Profanazione di tombe** | Medicina Legale | Pala da becchino (Meccanica 35) |
| **Alchimia necrotica** | Medicina Legale ≥ 40 | Capitolo 9 |
| **Oli** | Fabbro ≥ 90 (metalli), Sartoria ≥ 90 (cuoio) | Sostituiscono il materiale di un oggetto (7.19) |

---

## 7.16 Artigianato divino (Magical Smithing, Sewing, Alchemy)

Il livello più alto dell'artigianato. Richiede **attrezzi dorati** e postazioni leggendarie nella terra di TerMur:

| Attrezzo | Prezzo | Abilità | Dove si usa |
|---|---|---|---|
| Martello dorato magico (Magical Golden Smithing Hammer) | 1000 monete (fabbro dell'Enchanted Pass) | Fabbro | **Forgia della Testa di Drago** (Dragon Head Forge) |
| Forbici dorate magiche (Magical Golden Scissors) | 1000 monete (sarto) | Sartoria | **Arcolaio Incantato** (Enchanted Spinning Wheel) |
| Bottiglia dorata magica (Magical Golden Alchemy Bottle) | 1000 monete (alchimista) | Alchimia | **L'Alchimista Dorato** (Golden Alchemist) |

Ogni attrezzo ha solo **10 utilizzi**. Le soglie partono da **90** (alcune pozioni da 70) con massimo 125.

| Categoria | Cosa si crea |
|---|---|
| **Armature di pietre preziose** (fabbro) | Set completi (maniche, pettorale femminile, guanti, gorgiera, elmo, gambali, scudo, tunica) e **oli** in ametista, smeraldo, granato, ghiaccio, giada, marmo, onice, quarzo, rubino, zaffiro, argento, spinello, rubino stellato, topazio, caddellite. Servono **blocchi** della pietra (18–30 per pezzo) |
| **Armature di pelle speciale** (sarto) | Pelle di demone, drago, incubo, serpente, troll, unicorno: 6 pezzi ciascuna |
| **Pozioni leggendarie** (alchimista) | Invisibilità, Mana e Ringiovanimento (70/80/90), **Invulnerabilità** (90), Resurrezione di sé e degli altri (90), pozione di Riparazione (90) |

---

## 7.17 Strumenti e libri dei mestieri: riepilogo

| Mestiere | Attrezzo | Chi lo vende / chi lo fa | Libro speciale |
|---|---|---|---|
| Fabbro | Martello da fabbro, pinze | Fabbro, meccanico (Meccanica 35–40) | – |
| Sartoria | Kit da cucito | Sarto, meccanico (10) | – |
| Carpenteria | Seghe, pialle | Carpentiere, meccanico (0–30) | – |
| Lavorazione del legno | Attrezzi da falegname | Meccanico (30) | – |
| Costruzione Archi | Attrezzi da arciere | Arcaio, meccanico (35) | – |
| Meccanica | Attrezzi da meccanico | Meccanico (10) | – |
| Alchimia | Mortaio e pestello | Alchimista, meccanico (20) | – |
| Vetro | Cannello | Vetraio, meccanico (55) | 20.637 monete |
| Pietra | Mazzuolo e scalpello | Scalpellino, meccanico (55) | 50.625 monete |
| Cucina | Padella, mattarello, setaccio | Cuoco, meccanico | – |
| Scrittura Arcana | Penna da scriba | Scriba, meccanico (25) | – |
| Cartografia | Penna da cartografo | Cartografo, meccanico (25) | – |
| Bacchette | Tomo delle Bacchette | Bottino | – |

---

## 7.18 Riparare gli oggetti

### Riparare da soli

1. Apri il menu del mestiere e scegli **"Ripara"** (Repair).
2. Bersaglia l'oggetto nello zaino.

| Mestiere | Cosa ripara |
|---|---|
| Fabbro | Armi e armature di metallo |
| Sartoria | Cuoio e vestiti |
| Carpenteria | Oggetti di legno |
| Costruzione Archi | Archi |
| Meccanica | Gioielli e oggetti meccanici |

Regole (file `Repair.cs`):

- **Indebolimento**: a ogni riparazione l'oggetto perde punti di durabilità **massima**: 1 con abilità ≥ 90, 2 con ≥ 70, 3 sotto 70. La probabilità che succeda è 40% + 1% per ogni punto di durabilità mancante − 1% ogni 10 punti di abilità.
- **Riuscita**: dipende da quanto è danneggiato l'oggetto. Un oggetto molto rovinato è più difficile da riparare.
- Quando la durabilità massima è troppo bassa, l'oggetto *"si romperà se lo ripari ancora"*.

### Altri modi

- **Contratti di riparazione** (repair deed): ripari usando l'abilità scritta sul contratto. Se fallisci, il contratto si distrugge.
- **Atti di riparazione magica** (Magic Repair Deed): dagli ordini di massa.
- **Pozione di Riparazione**: artigianato divino.
- **PNG artigiani**: molti fabbri riparano per oro.

> **Consiglio:** ripara presto. Un oggetto quasi rotto è difficile da riparare e perde più durabilità massima.

---

## 7.19 Migliorare gli oggetti esistenti

### Attrezzi di gilda (enhancement)

Ogni gilda artigiana vende un **attrezzo straordinario** (500 monete, solo ai membri): martello del fabbro, forbici del sarto, sega del carpentiere, attrezzi dell'arcaio, del meccanico, dello scriba.

**Requisiti** (esempio del martello):

- essere nella **Gilda dei Fabbri** (oppure avere Fabbro ≥ 110);
- avere Fabbro ≥ 90;
- stare vicino a un maestro di gilda dei fabbri o a un tuo Negozio da fabbro.

**Uso**: scegli l'oggetto e **paghi in oro** per aggiungere proprietà (Costo Mana Ridotto, Costo Reagenti Ridotto, resistenze, ecc.) fino al loro massimo. I manufatti leggendari non si possono migliorare.

### Oli e lingotti

- **Oli** (gemme, metalli): un Gran Maestro (Fabbro 90+ o Sartoria 90+ per il cuoio) può applicarli per **sostituire il materiale** di un oggetto con quello dell'olio. Si trovano nelle bottiglie sconosciute del bottino, dagli ordini di massa e dall'artigianato divino.
- Con **"Migliora"** (Enhance) nel menu del mestiere usi lingotti, assi o cuoio di un materiale migliore su un oggetto esistente. Può riuscire o rovinare l'oggetto.

### Atti

Atti rari aggiungono proprietà direttamente. Gli **atti di benedizione** (bless deed) dagli ordini di massa rendono un oggetto benedetto.

---

## 7.20 Ordini di massa (Bulk Order Deeds, BOD)

Gli ordini di massa sono **richieste di produzione**: "fammi 15 spade lunghe eccezionali di bronzo". Esistono per **Fabbro, Sartoria, Carpenteria e Costruzione Archi**.

### Come ottenerli

1. Devi avere almeno 0,1 nell'abilità.
2. Clic sul mercante del mestiere (fabbro/armaiolo, sarto/tessitore, carpentiere, arcaio) e scegli **"Bulk Order Info"** dal menu. Puoi riceverne anche a caso quando vendi.
3. **Ordini piccoli**: 1 tipo di oggetto. **Ordini grandi**: un set di più oggetti. Dal valore **70,1** hai una probabilità pari a (abilità − 40) ÷ 300 di ricevere un ordine grande.
4. **Attesa tra un ordine e l'altro** (fabbro): 30 secondi sotto 50,1; 6 secondi fino a 70; quasi nulla sopra 70. Il tempo si azzera quando consegni un ordine.

### Come completarli

1. Crea gli oggetti richiesti, con la qualità e il materiale indicati.
2. Doppio clic sull'ordine → bersaglia gli oggetti (o un contenitore) per inserirli.
3. Quando è completo, **trascina l'ordine sul mercante** giusto.

### Crediti e ricompense

Consegnando un ordine non ricevi subito un premio. Ricevi **crediti con la gilda** del mestiere; gli ordini di materiale normale danno anche un po' di oro.

| Cosa dire al mercante | Effetto |
|---|---|
| *"credits"* o *"total"* | ti dice quanti crediti hai |
| *"claim"* | spendi tutti i crediti per il premio più alto possibile |
| *"claim 1000"* | spendi solo 1000 crediti |
| *"claim help"* | spiegazione |

Ogni premio dà anche **fama** e un piccolo bonus in oro.

**Dove riscuotere**:

| Mestiere | Mercanti |
|---|---|
| Fabbro | fabbro o maestro della gilda dei fabbri |
| Sartoria | sarto o maestro della gilda dei sarti |
| Carpenteria | carpentiere o maestro della gilda |
| Costruzione Archi | arcaio, maestro degli arcieri o dei ranger |

| Livello | Crediti | Livello | Crediti | Livello | Crediti |
|---|---|---|---|---|---|
| 1 | 96 | 6 | 2.778 | 11 | 6.166 |
| 2 | 237 | 7 | 3.484 | 12 | 7.249 |
| 3 | 472 | 8 | 4.049 | 13 | 8.425 |
| 4 | 802 | 9 | 4.708 | 14 | 9.696 |
| 5 | 1.649 | 10 | 5.413 | 15 | 11.061 |

A ogni riscossione c'è il 50% di probabilità di ricevere il premio del mestiere, altrimenti un premio comune (decorazioni, gemme, tappeti d'orso, artefatti...).

### Premi del fabbro (esempio)

| Livello | Premio (uno dei due) |
|---|---|
| 1 | Piccone robusto / guanti +3 Estrazione |
| 2 | Atto di riparazione magica / guanti +5 Estrazione |
| 3 | Attrezzo del cercatore / stendardo di gilda |
| 4 | Piccone del gargoyle / incudine colorata |
| 5 | Tavolozza per metalli / **martello runico di rame opaco** |
| 6 | Martello magico / martello runico di ferro ombra |
| 7 | Olio metallico / pietra per affilare |
| 8 | Forgia nanica / martello runico di rame |
| 9 | Olio di gemma / martello runico di bronzo |
| 10 | **Libro di studio di Fabbro (fino a 120)** / martello runico d'oro |
| 11 | Atto di riparazione magica (10) / guanti antichi da artigiano +10 |
| 12 | Forgia elfica / martello runico di agapite |
| 13 | **Pergamena del potere 105** / martello runico di verite |
| 14 | **Pergamena del potere 110** / martello runico di valorite |
| 15 | Pergamena del potere casuale: 105 (35%), 110 (30%), 115 (15%), 120 (7%), **125 (3%)**; oppure guanti antichi +25 (10%) |

Gli altri mestieri hanno tabelle analoghe con i loro attrezzi runici e le loro pergamene del potere.

> **Consiglio:** gli ordini di massa sono **l'unico modo** per avere le pergamene del potere degli artigiani fino a 125 e gli attrezzi runici. Conserva i materiali colorati per gli ordini che li richiedono; un **libro degli ordini di massa** (Scrittura Arcana 65) ti aiuta a tenerli in ordine.

---

## 7.21 Il Negozio (Shoppe)

Un **Negozio** è un mobile da mettere in casa che "lavora" per te.

### Come aprirlo

1. Devi avere una **casa** e almeno **50** nell'abilità del mestiere.
2. Dal menu contestuale del mercante giusto scegli **"Setup Shoppe"** (per l'erborista servono Alchimia, Cucina e Conoscenza Animali a 40).
3. Paga **10.000 monete d'oro**. Con l'aria da mendicante (Accattonaggio) il prezzo scende di Accattonaggio × 25, fino a −3000.
4. **Fissa** il Negozio in casa (lock down).

Puoi avere un solo Negozio per tipo, ma più Negozi di mestieri diversi.

| Negozio | Mercante | Abilità ≥ 50 |
|---|---|---|
| Fabbro | Fabbro | Fabbro |
| Arcaio | Arcaio | Costruzione Archi |
| Carpentiere | Carpentiere | Carpenteria |
| Cartografo | Cartografo | Cartografia |
| Panettiere | Cuoco, fornaio | Cucina |
| Bibliotecario | Scriba, saggio | Scrittura Arcana |
| Sarto | Sarto, tessitore, conciatore | Sartoria |
| Meccanico | Meccanico | Meccanica |
| Alchimista | Alchimista | Alchimia |
| Mortuario | Negromante, strega | Medicina Legale |
| Erborista | Erborista, druido | Alchimia, Cucina, Conoscenza Animali 40 |

### Come funziona

1. **Rifornisci** il Negozio trascinandoci sopra **attrezzi** (contano gli utilizzi, massimo 1000) e **materiali** (massimo 5000).
2. Ogni 2–4 ore (a volte 20–40 minuti) arrivano **richieste dei clienti**. Ognuna mostra attrezzi e materiali necessari, la tua probabilità di riuscita (in base all'abilità) e la **reputazione** che guadagni.
3. Accetti o rifiuti. Se fallisci o rifiuti, perdi quella reputazione. Più reputazione hai (massimo 10.000), più i lavori sono redditizi. I membri della gilda del mestiere hanno un bonus di reputazione.
4. L'oro guadagnato resta nel Negozio: clic sul Negozio e **"Transfer"** per ricevere un **assegno**. Ritirare dà anche fama.

> **Attenzione:** la guida nel gioco dice che il Negozio può contenere fino a 500.000 monete, ma il codice accetta nuovi lavori solo finché il totale resta **sotto 100.001**. Svuotalo spesso.

> **Consiglio:** i Negozi fanno anche **guadagnare abilità** senza ripetere macro, e i lavori di gilda pagano meglio. Un'ottima rendita passiva per ogni artigiano.

---

## 7.22 Consigli finali per l'artigiano

- **Unisciti alla gilda del tuo mestiere**:
  - anelli da +20 a +30;
  - apprendimento circa 1,4 volte più veloce;
  - prezzi migliori;
  - attrezzo per migliorare gli oggetti.
- **Mangia cibo cucinato** prima di allenarti: fino al doppio dei guadagni.
- **Raccogli da solo**:
  - un minatore-fabbro o un cacciatore-sarto non dipende dal mercato;
  - i cuoi migliori vengono da creature specifiche, quindi scegli le prede in base al cuoio che ti serve.
- **Vendi con giudizio**:
  - Identificare Oggetti a 100 raddoppia i prezzi di vendita;
  - i mercanti cambiano richieste nel tempo;
  - i **mercanti dei giocatori** in casa vendono spesso meglio ai "giocatori simulati".
- **Punta agli ordini di massa** quando arrivi a Gran Maestro: attrezzi runici, pergamene del potere fino a 125, libri di studio fino a 120.
