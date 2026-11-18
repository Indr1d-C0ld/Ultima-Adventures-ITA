# 5. Le abilità

In Ultima Adventures il personaggio **non ha una classe fissa**: è la somma delle sue abilità (skill). Ogni abilità va da 0 a 100 (il "Gran Maestro") e, con oggetti speciali, fino a 125. A differenza dell'Ultima Online classico qui il tetto totale è altissimo: puoi imparare **quasi tutto**, con calma.

Questo capitolo spiega:

- le regole generali: tetti, velocità di apprendimento, come si sale (5.1–5.4);
- come comprare, studiare e alzare i limiti (5.5–5.7);
- i titoli professionali (5.8);
- tutte le 58 abilità, una per una, raggruppate per tipo (5.9–5.16);
- una tabella riassuntiva finale (5.17).

> **Nota sui nomi.** Uso i nomi italiani ufficiali delle abilità con il nome inglese tra parentesi alla prima occorrenza, perché in alcune finestre (o nelle guide in rete) li troverai in inglese.

---

> **Novità di questa versione:** riposo e ispirazione, bonus alla varietà e fascia 80–100 ammorbidita rendono la crescita più rapida: vedi «Progressione» nel capitolo 4.

## 5.1 I tetti: totale, singola abilità, caratteristiche

| Limite | Valore | Note |
|---|---|---|
| **Tetto totale delle abilità** (nuovo personaggio) | **2500 punti** | Valore del server (`skillcap() = 25000` in decimi). La vecchia guida inglese parla di "2.4k": è superato. |
| Tetto totale dopo il Portale di Mangar | 10000 punti | Si ottiene attraversando il portale di Mangar al termine della relativa missione (vedi 5.7). |
| Personaggi "Barbaro", "Ricercato", "Alieno" | 10000 punti | Modalità speciali di gioco (vedi capitolo 6). |
| **Tetto di una singola abilità** | **100** | Si alza a 105/110/115/120/125 con le *pergamene del potere* (5.7). |
| **Tetto delle caratteristiche** (For + Des + Int) | **250** se Avatar, **225** se non Avatar | Si sceglie con la seconda domanda dell'introduzione. |
| Massimo di una singola caratteristica | 150 | 175 se il tuo tetto totale supera 275. |

Con 2500 punti puoi avere **25 abilità a Gran Maestro**: in pratica puoi combinare un combattente, un mago, un artigiano e un domatore nello stesso personaggio. L'unico vero limite è il tempo.

### Cosa succede quando arrivi al tetto totale

Quando la somma delle tue abilità raggiunge il tetto, per salire in un'abilità devi **perdere punti in un'altra**. Si usano le frecce accanto a ogni abilità nella finestra delle abilità:

- **freccia su** (Up): l'abilità può salire;
- **freccia giù** (Down): l'abilità può scendere per far posto alle altre;
- **lucchetto** (Locked): l'abilità resta ferma.

Se sei al tetto e **nessuna** abilità è impostata su "giù", non guadagni più niente. Lo stesso vale per le caratteristiche (frecce accanto a For, Des e Int nel paperdoll).

---

## 5.2 Come si impara: il "tiro di guadagno"

Ogni volta che usi un'abilità in modo utile, il server controlla se riesci (in base alla difficoltà) e poi tenta un **guadagno**. Le regole (file `SkillCheck.cs`):

1. **Sotto 10,0 punti guadagni sempre** a ogni utilizzo valido.
2. La probabilità di guadagno è più alta quando l'abilità è bassa e quando l'azione è **difficile per te ma ci riesci**. Le azioni troppo facili (abilità già oltre il massimo della difficoltà) **non danno mai** guadagni; quelle troppo difficili (abilità sotto il minimo) non riescono e non insegnano niente.
3. Fallire insegna meno che riuscire: il bonus legato alla difficoltà si applica solo ai successi.
4. Il risultato è moltiplicato per il **fattore di apprendimento** dell'abilità (1,0 per la maggior parte; 0,8, 0,7 o 0,5 per alcune, vedi tabella finale).
5. Tutti i personaggi, Avatar e non Avatar, hanno lo stesso moltiplicatore **×1,75**.

**Esempio pratico.** Abilità a 50, azione con il 50% di riuscita, appetito normale:

- se riesci: circa **48%** di probabilità di guadagnare;
- se fallisci: circa **22%**.

A 95, con lo stesso 50%, la probabilità di "tiro" resta intorno al 24%, ma il guadagno scatta davvero solo 1 volta su 5 (vedi la tabella seguente). Per questo gli ultimi punti sono lenti.

### Quanto si guadagna ogni volta

| Valore attuale | Guadagno per riuscita |
|---|---|
| fino a 25 | da 0,3 a 0,5 |
| 25,1 – 55 | da 0,2 a 0,3 |
| 55,1 – 70 | da 0,1 a 0,2 |
| 70,1 – 80 | 0,1 |
| 80,1 – 85 | 0,1 con probabilità 1/2 |
| 85,1 – 90 | 0,1 con probabilità 1/3 |
| 90,1 – 95 | 0,1 con probabilità 1/4 |
| 95,1 – 100 | 0,1 con probabilità 1/5 |
| 100,1 – 105 | 0,1 con probabilità 1/6 |
| 105,1 – 110 | 0,1 con probabilità 1/7 |
| oltre 110 | 0,1 con probabilità 1/8 |

**Concentrazione** (Focus) e **Meditazione** (Meditation) hanno un'ulteriore riduzione: solo 1 guadagno su 3 va a buon fine.

---

## 5.3 Cosa accelera (o rallenta) l'apprendimento

| Fattore | Effetto |
|---|---|
| **Fame e sete** (valore "Hunger") | ≤ 3: guadagni ÷1,5 · ≤ 10: ÷1,25 · ≤ 20: ×1,10 · ≤ 25: ×1,25 · ≤ 30: ×1,5 · ≤ 40: ×2 |
| **Gilda PNG** (abilità della tua gilda) | circa ×1,4 più veloce (il divisore passa da 2 a 1,55–1,80) |
| **Dungeon** di difficoltà Difficile o superiore | da ×1,07 a ×1,8 a seconda dell'abilità e del dungeon (vedi sotto) |
| **Pergamena della Rapidità** (Scroll of Alacrity) | per 15 minuti, ogni guadagno in quell'abilità vale ×2, ×3 o ×4 |
| **Libri di studio** con 5 ore di studio o più | 30 minuti di effetto Rapidità al rientro |
| **Filatterio** (personaggi Anima Legata) | modificatore extra; ×4 nel "Basement" |
| In **prigione** | nessun guadagno |

### Il cibo è un acceleratore

Il valore di sazietà normale arriva a 20. Il **cibo cucinato** dai giocatori ha un "beneficio" che permette di mangiare oltre la sazietà, fino a 20 + beneficio. Sopra 20 i guadagni salgono fino a **×2** (sazietà 31–40). Tenere lo stomaco pieno di buon cibo è il modo più semplice per imparare più in fretta.

> **Consiglio:** porta sempre con te cibo cucinato da un cuoco esperto (torte, pizze, razioni). Mangialo prima di una sessione di allenamento: raddoppiare i guadagni gratis non ha prezzo.

### Il bonus dei dungeon

Nei dungeon di difficoltà **Difficile (1)** o superiore, alcune abilità imparano più in fretta. Il bonus base è *difficoltà ÷ 5* (fino a 0,8 nei dungeon Mortali):

| Gruppo | Abilità | Moltiplicatore |
|---|---|---|
| **A** (bonus pieno) | Scherma con Spada, Lame Leggere, Combattere con Mazza, Tiro con l'Arco, Magia, Negromanzia, Cavalleria, Bushido, Ninjitsu, Provocazione, Discordia, Pacificazione, Furto, Furtività | da ×1,2 (Difficile) a **×1,8** (Mortale) |
| **B** (un terzo del bonus) | Rivelare Nascosti, Meditazione, Assaggiare, Identificare Oggetti, Anatomia, Scassinare, Disinnescare Trappole, Tattica, Musica, Conoscenza Animali, Estrazione Mineraria, Seguire Tracce, Parata, Valutare Intelligenza, Nascondersi | da ×1,07 a ×1,27 |
| **C** | tutte le altre (artigianato, ecc.) | nessun bonus |

Nei dungeon Facili e Normali non c'è bonus. Quando il bonus è attivo e guadagni, compare la scritta *"You gain inspiration from training here"*.

> **Nota.** La guida inglese originale parla di "fino al 200%": nel codice il massimo è ×1,8.

### Niente blocco "anti-macro"

Sul server il controllo anti-macro è **disattivato**: puoi ripetere la stessa azione sullo stesso bersaglio o nello stesso posto senza che i guadagni si blocchino. Le uniche eccezioni sono i limiti propri di ogni abilità (per esempio la Pesca, vedi 5.13).

---

## 5.4 Le caratteristiche crescono con le abilità

Ogni guadagno di abilità può far salire anche una caratteristica. L'ordine del controllo è Forza, poi Destrezza, poi Intelligenza; la probabilità dipende dall'abilità (colonne "For/Des/Int" della tabella finale; per esempio Accampamento dà 6% Forza, 4,5% Destrezza, 4,5% Intelligenza).

- Ogni caratteristica può salire **al massimo una volta ogni 10 minuti** (5 minuti con lo stato FastGain). Il ritardo è calcolato per giocatore.
- Le caratteristiche salgono solo se la loro freccia è su "su".
- Quando sei al tetto delle caratteristiche, una sale e un'altra con la freccia "giù" scende.

Le abilità migliori per le caratteristiche:

| Caratteristica | Abilità consigliate |
|---|---|
| Forza | Accampamento, Estrazione Mineraria, Taglialegna, Carpenteria, Pastorizia, Addomesticare Animali, Fabbro |
| Destrezza | Curiosare, Scassinare, Cucina, Costruzione Archi, Sartoria, Accampamento, Seguire Tracce, Furto |
| Intelligenza | Cucina, Avvelenamento, Magia, Accampamento, Seguire Tracce, Conoscenza Animali, Valutare Intelligenza |

> **Consiglio:** **Accampamento** (Camping) è la migliore abilità per alzare tutte e tre le caratteristiche. Dal primo giorno, ogni 10 minuti accendi un fuoco con la legna da ardere: con pochissima fatica le caratteristiche salgono in parallelo alle abilità.

---

## 5.5 Comprare le abilità dagli insegnanti

Quasi tutti i PNG possono insegnarti le loro abilità:

1. Clicca con il tasto sinistro sul PNG (o usa il menu contestuale) e scegli **"Train ..."** con il nome dell'abilità.
2. Il PNG ti dice il prezzo.
3. Trascina sul PNG **esattamente** quella quantità di monete d'oro. Se dai meno, impari meno.

Regole (file `BaseCreature.cs`):

- Il PNG deve avere almeno 60 nell'abilità.
- Insegna fino a **metà** del suo valore, **massimo 50,0**.
- Prezzo: **1 moneta d'oro per ogni 0,1 punti** (da 0 a 50 costano 500 monete).
- I **cloni** dei giocatori disconnessi (CharacterClone) insegnano fino a **80,0**, ma solo se hanno almeno 100 nell'abilità, e costano 300 monete per 0,1 punti.
- Non si insegnano mai: Tessitura Arcana, Misticismo, Infusione, Lancio. **Furtività** si insegna solo se hai almeno 50 in Nascondersi.
- I guaritori insegnano solo Medicina Legale, Guarigione, Parlare con gli Spiriti e Scherma con Spada.

> **Consiglio:** comprare i primi 50 punti costa poco ed evita la parte più noiosa. Fallo per tutte le abilità che ti interessano, **tranne** quelle che vuoi usare per alzare le caratteristiche (come Accampamento): sotto 10 si guadagna sempre, e ogni guadagno è un'occasione di statistica.

---

## 5.6 I libri di studio (allenamento offline)

I **libri di studio** (study books, "manuali di addestramento") ti fanno guadagnare punti **mentre sei disconnesso**. Ogni nuovo personaggio ne riceve **due casuali** di livello base.

### Come si usano

1. Metti il libro nello **zaino**.
2. Fai **doppio clic** sul libro: il personaggio si prepara a studiarlo.
3. **Esci dal gioco** (log out). Il doppio clic va ripetuto **a ogni accesso**.
4. Al rientro ricevi i punti guadagnati.

### Le regole (file `StudyBook.cs`)

| Voce | Valore |
|---|---|
| Tentativi | 1 ogni 10 minuti offline |
| Probabilità di ogni tentativo | ((massimo del libro − abilità) ÷ massimo del libro) × ((tetto − abilità) ÷ tetto) ÷ 1,5 (÷ 2 per Addomesticare Animali, Scassinare, Provocazione, Magia, Bushido, Ninjitsu, Cavalleria, Negromanzia); minimo 1% |
| Guadagno massimo per sessione | 20 punti (sopra 100 i punti valgono metà) |
| Usura | 8% di probabilità che il libro si sbricioli dopo l'uso |
| Bonus | 5 ore o più di studio: 30 minuti di Rapidità su quell'abilità |

| Livello del libro | Porta fino a | Dove si trova |
|---|---|---|
| Base (Standard) | **70** | Iniziale, bottino, scaffali dei bibliotecari; un rilegatore li vende a 7500 monete |
| Avanzato (Advanced) | **100** | Bottino raro, ricompense degli ordini di massa |
| Leggendario ("tome of lost ... techniques") | **120** | Molto raro |

I libri si possono anche **scrivere** con Scrittura Arcana (vedi capitolo 7): i libri "Expert" portano a 70, quelli "Grandmaster" a 100.

> **Consiglio:** i libri di studio sono perfetti per le abilità noiose da allenare (Meditazione, Nascondersi, Resistenza Magica) o che costano karma (Furto, Curiosare, Accattonaggio).

---

## 5.7 Pergamene e oggetti che alzano i limiti

### Pergamene del potere (Power Scroll)

Alzano il **tetto di una singola abilità** a 105, 110, 115, 120 o 125.

- Vanno usate **in ordine**: per usare la +110 il tuo tetto attuale deve già essere 105, e così via.
- Vanno lette in un **santuario** preciso. Altrimenti appare *"This magic can only be unleashed at the Shrine of ..."*:

| Santuario | Abilità |
|---|---|
| **Santuario della Forza** (Shrine of Strength) | Lotta, Bushido, Scherma con Spada, Taglialegna, Estrazione Mineraria, Fabbro, Carpenteria, Combattere con Mazza, Tattica, Parata, Lame Leggere, Accampamento |
| **Santuario dell'Intelligenza** (Shrine of Intelligence) | Magia, Resistenza Magica, Meditazione, Negromanzia, Conoscenza Armi, Cartografia, Cucina, Valutare Intelligenza, Anatomia, Alchimia, Sartoria, Meccanica, Scrittura Arcana, Medicina Legale, Pastorizia, Identificare Oggetti, Assaggiare |
| **Santuario della Destrezza** (Shrine of Dexterity) | Discordia, Provocazione, Musica, Tiro con l'Arco, Furto, Furtività, Nascondersi, Disinnescare Trappole, Curiosare, Rivelare Nascosti, Ninjitsu, Costruzione Archi, Scassinare, Accattonaggio |
| **Santuario della Saggezza** (Shrine of Wisdom) | Parlare con gli Spiriti, Cavalleria, Pacificazione, Seguire Tracce, Veterinaria, Conoscenza Animali, Addomesticare Animali, Avvelenamento, Concentrazione, Pesca, Guarigione |

Dove si trovano: campioni (champion spawn), mercanti speciali (fino a 120, costosi), e ricompense degli **ordini di massa** per le abilità di artigianato (fino a 125, vedi capitolo 7).

### Altre pergamene e pozioni

| Oggetto | Effetto |
|---|---|
| **Pergamena del tetto delle caratteristiche** (Stat Cap Scroll) | Porta il tetto delle caratteristiche al valore indicato (+5 se hai la ricompensa veterani). |
| **Pergamena della Trascendenza** (Scroll of Transcendence) | Aggiunge subito punti a un'abilità, senza superarne il tetto. Se sei al tetto totale toglie punti da un'abilità con la freccia "giù". |
| **Pergamena della Rapidità** (Scroll of Alacrity) | 15 minuti di guadagni ×2/×4 in un'abilità. |
| **Pozioni di Potenza, Agilità e Saggezza** e le **Pozze magiche** | Alzano le caratteristiche fino al tetto. |
| **Portale di Mangar** (Mangar's Gate) | Porta il tetto totale a **10000 punti**. Richiede di aver completato la missione di Mangar (Bard's Tale). |
| **Sorso Santo dell'Umiltà** (Holy Draught of Humility) | **Abbassa** il tetto totale a 1000 punti, solo se ne hai al massimo 1000. Ti rende un discepolo della "via della luna": è richiesto per passare oltre l'Obelisco della Luna. |
| **Missione dell'Ossidiana** (Titano dell'Etere) | +50 al tetto delle caratteristiche. |

### Bonus degli oggetti

Gli oggetti con "+X a un'abilità" (anelli di gilda, guanti, ecc.) alzano il valore **effettivo** oltre il valore base, anche oltre 100. Non alzano però il valore "base" né fanno guadagnare oltre il tetto. Molti effetti "a 125" (vedi le singole abilità) controllano il valore **base**, quindi richiedono le pergamene del potere.

---

## 5.8 Titoli professionali

Il tuo titolo (es. *"Gran Maestro Fabbro"*) mostra l'abilità più alta, oppure quella che scegli tu. Il prefisso dipende dal valore:

| Valore | Titolo | Valore | Titolo |
|---|---|---|---|
| fino a 29,9 | Aspirante (Aspiring) | 80 | Adepto (Adept) |
| 30 | Neofita (Neophyte) | 90 | Maestro (Master) |
| 40 | Novizio (Novice) | 100 | **Gran Maestro** (Grandmaster) |
| 50 | Apprendista (Apprentice) | 110 | Anziano (Elder) |
| 60 | Garzone (Journeyman) | 120 o più | Leggendario (Legendary) |
| 70 | Esperto (Expert) | | |

Bushido e Ninjitsu hanno nomi propri a 110/120 (Tatsujin/Kengo e Shinobi/Ka-ge).

Il nome della professione dipende dall'abilità (vedi la colonna "Titolo" della tabella finale). Alcuni titoli cambiano in base al karma o ad altre abilità:

| Titolo base | Diventa | Condizione |
|---|---|---|
| Mago / Negromante | **Arcimago** (Archmage) | Magia **e** Negromanzia entrambe ad almeno 100 base |
| Paladino (Cavalleria) | **Cavaliere della Morte** | karma negativo |
| Guaritore / Medium | **Sacerdote** | karma ≥ 2500, Guarigione e Parlare con gli Spiriti ad almeno 50 |
| Samurai / Ninja | **Ronin** / **Yakuza** | karma negativo |
| Mendicante / Studioso | **Giullare** / **Burlone** | se sei un Giullare (vedi capitolo 6) |

---

## 5.9 Come leggere le schede

Ogni abilità ha una scheda con:

- **Tipo**: *attiva* (si usa dal pulsante nella finestra delle abilità, o con una macro "Usa abilità") oppure *passiva* (funziona da sola, con oggetti o attrezzi).
- **Uso**: cosa fa e come si usa, passo per passo.
- **Caratteristiche**: probabilità di far salire For/Des/Int a ogni guadagno.
- **Allenamento**: come salire da 0 a 100 e oltre.
- **A 125**: l'effetto speciale che si sblocca con valore base 125 (dove esiste).

Le abilità con **pulsante** (attive) sono 22: Anatomia, Conoscenza Animali, Identificare Oggetti, Conoscenza Armi, Accattonaggio, Pacificazione, Rivelare Nascosti, Discordia, Valutare Intelligenza, Medicina Legale, Nascondersi, Provocazione, Scrittura Arcana, Avvelenamento, Parlare con gli Spiriti, Furto, Addomesticare Animali, Assaggiare, Seguire Tracce, Meditazione, Furtività, Disinnescare Trappole.

> **Automazione.** Puoi dire *"I wish to start fishing"* (oppure mining, lumberjacking, milling, skinning, smelting ore, cutting logs; *"I wish to start list"* mostra le opzioni) per far ripetere automaticamente al personaggio la raccolta. È legale e utile per le abilità di raccolta.

---

## 5.10 Abilità di combattimento

### Scherma con Spada (Swordsmanship), Combattere con Mazza (Mace Fighting), Lame Leggere (Fencing)

- **Tipo**: passive. Si usano equipaggiando un'arma del tipo giusto e attaccando (modalità guerra, doppio clic sul nemico).
  - Scherma con Spada: spade, asce, alabarde e armi taglienti.
  - Combattere con Mazza: mazze, martelli, bastoni.
  - Lame Leggere: pugnali, lance, kryss, forche.
- **Effetto**: probabilità di colpire e di non essere colpito. Le armi da taglio/asce ricevono bonus da Taglialegna; le contundenti da Estrazione Mineraria (vedi le rispettive abilità).
- **Caratteristiche**: Spada For 2,3% / Des 0,8%; Mazza For 2,7% / Des 0,3%; Lame Leggere For 1,4% / Des 1,7%.
- **Allenamento**:
  - 0–50: compra dall'insegnante (guerriero, guardia, maestro d'armi).
  - 50–70: combatti animali e mostri facili. Il guadagno si ha a **ogni colpo**, ma **non** contro giocatori, manichini, animali domestici o creature evocate.
  - 70–100: dungeon di difficoltà crescente. Il bonus dungeon (gruppo A) arriva a ×1,8.
  - Più il nemico è "alla pari", più impari: un nemico troppo debole non insegna niente.
- **A 125 (Lame Leggere)**: 20% di probabilità di contrattaccare (riposta) dopo una parata riuscita.

### Lotta (Wrestling)

- **Tipo**: passiva. Si combatte a mani nude, oppure con **guanti da pugile** o **guanti da lancio**.
- **Effetto**:
  - La difesa a mani nude usa il valore più alto tra Lotta e (Anatomia + Valutare Intelligenza + 20) ÷ 2.
  - **Stordimento**: richiede Anatomia e Lotta ad almeno 80. Costa 15 Vigore e blocca il nemico 4 secondi.
  - **Disarmo**: richiede Conoscenza Armi e Lotta ad almeno 80.
  - Lotta è l'abilità di lancio e di danno del **Monaco** (capitolo 8C) e aumenta le cure di Parlare con gli Spiriti.
- **Caratteristiche**: For 2,7%, Des 0,3%.
- **Allenamento**: come le altre armi. I guanti da pugile fatti dai sarti a 32,9 permettono di combattere e di lanciare incantesimi (incanalamento).

### Tiro con l'Arco (Archery)

- **Tipo**: passiva. Equipaggia un arco o una balestra, tieni **frecce o dardi nello zaino** e attacca.
- **Effetto**: precisione con archi e balestre. Il danno degli archi di legno sale anche con **Costruzione Archi** (vedi 5.15).
- **Caratteristiche**: For 0,8%, Des 2,3%.
- **Allenamento**: come le altre armi. Combattere tenendo la distanza (*kiting*) è il modo più sicuro. Il bonus dungeon è pieno (gruppo A).

### Tattica (Tactics)

- **Tipo**: passiva. Si allena a ogni colpo andato a segno con qualsiasi arma.
- **Effetto**: +0,625% di danno per punto (+62,5% a 100), +6,25% extra a 100.
- **Caratteristiche**: nessuna.
- **Allenamento**: insieme alla tua abilità d'arma.
- **A 125**: anche un colpo mancato infligge il 25% del danno.

### Anatomia (Anatomy)

- **Tipo**: attiva. Pulsante → bersaglio: una creatura entro 8 caselle. Attesa 1 secondo. A partire da 65 mostra anche la percentuale di vigore.
- **Effetto**:
  - +0,5% di danno in mischia per punto (+50% a 100), +5% extra a 100.
  - Seconda abilità della **Guarigione** con le bende.
  - Con Lotta 80: stordimento.
  - Quando scuoi un cadavere: +Anatomia ÷ 25 pelli, carne, piume, ecc. (insieme a Medicina Legale).
- **Caratteristiche**: For 0,5%, Des 0,5%, Int 2,1%. Fattore di apprendimento 0,7.
- **Allenamento**: il pulsante su qualunque creatura (anche PNG in città) da 0 a 100, più un tiro passivo a ogni colpo d'arma. È un'abilità del gruppo B (bonus ridotto nei dungeon).
- **A 125**: i tuoi colpi non magici trattano le resistenze del bersaglio come più basse del 30%.

### Parata (Parrying)

- **Tipo**: passiva.
- **Effetto** (probabilità di bloccare un colpo):
  - Con uno **scudo**: (Parata − Bushido) ÷ 400, +5% a 100; ridotta se la Destrezza è sotto 80.
  - Con un'**arma** (senza scudo): (Parata × Bushido) ÷ 41140–48000, oppure Parata ÷ 800; +5% a 100.

  In pratica: **il Bushido peggiora la parata con lo scudo** ma permette di parare con le armi a due mani.
- **Caratteristiche**: For 2,3%, Des 0,8%.
- **Allenamento**: farsi attaccare in combattimento (stesse limitazioni delle armi: niente giocatori, animali domestici o evocazioni).
- **A 125**: puoi parare anche a mani nude o con armi a distanza (Parata ÷ 800).

> **Consiglio:** all'inizio gli scudi sono scarsi. Se non vuoi il Bushido, rimanda la Parata a quando avrai uno scudo decente.

### Conoscenza Armi (Arms Lore)

- **Tipo**: attiva. Pulsante → un'arma o un'armatura entro 2 caselle.
- **Effetto**:
  - **Identifica** armi e armature trovate (le rende "identificate"), e gli oggetti sconosciuti marcati "Use Arms Lore".
  - +0,625% di danno per punto con **qualsiasi** arma, +6,25% a 100.
  - Riduce la probabilità che il tuo equipaggiamento perda durabilità.
  - Per i fabbri: le armi eccezionali ricevono +Conoscenza Armi ÷ 20 % di danno; le armature eccezionali ricevono un punto di resistenza casuale in più ogni 15 punti (6 a 90).
  - Con Lotta 80: disarmo. Per la Furtività: stima del "rumore" dell'armatura.
- **Caratteristiche**: For 2,3%, Des 0,5%, Int 0,3%. Fattore 0,8.
- **Allenamento**: usa il pulsante su ogni arma o armatura che trovi; anche il crafting di armi eccezionali dà tiri.

### Resistenza Magica (Resisting Spells)

- **Tipo**: passiva. Si allena quando un incantesimo ti colpisce.
- **Effetto**:
  - Riduce durata ed effetto di incantesimi come Paralisi e Veleno.
  - Dà una **resistenza minima** a tutti gli elementi: (Resistenza − 40) ÷ 1,5 % a partire da 40; 40% a 100; +1 ogni 5 punti oltre.
- **Caratteristiche**: For 0,8%, Des 0,8%, Int 1,5%. Fattore 0,7.
- **Allenamento**: fatti lanciare incantesimi addosso (un amico, oppure tu stesso con Goffaggine, Indebolimento, Mente Debole, Maledizione). Altrimenti con i libri di studio.
- **A 125**: 20% di probabilità di **riflettere** un incantesimo.

### Concentrazione (Focus)

- **Tipo**: passiva.
- **Effetto**: rigenerazione Vigore +0,1 per punto e Mana +0,05 per punto. È l'abilità di lancio e di danno degli incantesimi degli **oggetti magici** (Evoca Drago, Evoca Serpi, Fulmine di Thor).
- **Caratteristiche**: nessuna. Fattore 0,5, e solo 1 guadagno su 3 va a segno.
- **Allenamento**: sale da sola mentre il Vigore o il Mana si rigenerano. Corri finché sei stanco, poi riposa. È lentissima: usa i libri di studio.
- **A 125**: i tuoi incantesimi non vengono più interrotti dai danni.

---

## 5.11 Abilità magiche

I dettagli di ogni scuola di magia sono nel **capitolo 8**. Qui trovi solo come allenarle.

### Magia (Magery)

- **Tipo**: passiva. Si lanciano incantesimi dal libro degli incantesimi, con i reagenti.
- **Allenamento**: ogni incantesimo dà guadagni in un intervallo legato al cerchio, circa (cerchio − 1) × 14,3 ± 20 punti. Le pergamene contano come 2 cerchi in meno.

| Fascia | Cosa lanciare |
|---|---|
| 0–50 | compra dall'insegnante (mago) |
| 50–55 | 3°–4° cerchio: Benedizione, Purificazione Superiore, Cura Superiore |
| 55–70 | 5° cerchio: Paralisi, Riflesso Magico, Incognito |
| 70–85 | 6° cerchio: Invisibilità, Marchio, Dissolvi |
| 85–100 | 7° cerchio: Dissolvi di Massa, Portale, Metamorfosi |
| 100–120 | 8° cerchio: Terremoto, evocazioni di elementali |

- **Caratteristiche**: Int 4,5%. Bonus dungeon pieno (gruppo A).
- **Bonus**: bastoni e bacchette da mago guadagnano +0,625% di danno per punto, +6,25% a 100.

> **Consiglio:** Meditazione e Concentrazione si allenano da sole mentre consumi mana per la Magia. Alzale insieme.

### Valutare Intelligenza (Evaluating Intelligence)

- **Tipo**: attiva. Pulsante → una creatura entro 8 caselle. Da 76 mostra anche il mana.
- **Effetto**:
  - Aumenta il danno degli incantesimi di Magia (scala AOS: 30% + 0,9% per punto).
  - È l'abilità di lancio dei poteri **Syth** e **Jedi** e l'abilità di danno dei trucchi del **Giullare**.
- **Caratteristiche**: Int 3%. Fattore 0,8.
- **Allenamento**: il pulsante su chiunque, da 0 a 120. In più ottieni un tiro a ogni incantesimo di danno della Magia.

### Meditazione (Meditation)

- **Tipo**: attiva (10 secondi) e passiva.
- **Effetto**:
  - **Attiva**: riuscita (50 + 2 × abilità)%. Raddoppia la rigenerazione del mana finché resti fermo. L'armatura che blocca la meditazione (piastre, ecc.) lo impedisce.
  - **Passiva**: rigenerazione del mana (Int + 3 × Meditazione) × 0,025, 0,0275 a 100.
- **Caratteristiche**: nessuna. Fattore 0,5, e solo 1 guadagno su 3 va a segno.
- **Allenamento**: consuma mana e meditaci sopra. Sale anche passivamente durante la rigenerazione. Molto lenta: usa i libri di studio.
- **A 125**: +5 rigenerazione di Punti Ferita e Vigore mentre mediti.

### Scrittura Arcana (Inscription)

- **Tipo**: attiva (pulsante → un libro da copiare) e soprattutto **artigianato** (penna da scriba; capitolo 7).
- **Effetto**:
  - Bonus al danno degli incantesimi: abilità ÷ 20 % sotto 100, con un salto a **+10% a Gran Maestro** (11% a 120–125).
  - Bonus al danno magico delle armi: +Scrittura ÷ 20 %, +5 a Gran Maestro.
  - Armatura Reattiva e Protezione più forti.
  - Cariche dei libri delle rune: +Scrittura ÷ 30.
  - Più pergamene vuote quando le raccogli.
  - Aiuta a identificare le pergamene sconosciute.
  - È l'abilità di lancio e di danno degli **incantesimi antichi** (Ricerca arcana).
- **Caratteristiche**: Des 0,6%, Int 2,4%.
- **Allenamento**: scrivi pergamene di Magia, poi libri e libri di studio (capitolo 7). Usa il **monocolo** sulle librerie dei dungeon per trovare libri e pergamene gratuiti.
- **A 125**: i libri degli incantesimi che crei escono **già completi**.

### Negromanzia (Necromancy)

- **Tipo**: passiva. Si lancia con il libro della negromanzia e i reagenti negromantici. Il karma deve essere ≤ 0.
- **Allenamento**: ogni incantesimo dà guadagni da "abilità richiesta" a "richiesta + 40". Una progressione tipica: Arma Maledetta e Fitta di Dolore fino a 50, Colpo Velenoso fino a 70, Avvizzimento e Strangolamento fino a 90, poi Forma di Lich e Spirito Vendicativo (vedi capitolo 8B).
- **Caratteristiche**: nessuna.
- **Nota**: abbassa il **karma** molto in fretta. A −4000 karma le guardie ti attaccano.

### Parlare con gli Spiriti (Spirit Speak)

- **Tipo**: attiva. Pulsante: è un "incantesimo" di 1 secondo che fallisce se vieni colpito. Attesa 5 secondi.
- **Effetto**:
  - **Cura** Punti Ferita e Vigore: minimo 1 + 0,25 × Parlare con gli Spiriti + 0,15 × Lotta, fino a +4 oltre il minimo.
  - 33% di probabilità di recuperare da 1 a 10 punti di mana.
  - Costa 20 mana, oppure 0 se c'è un cadavere entro 3 caselle (solo con karma negativo) o se bruci delle ossa.
  - Non funziona se sei avvelenato, affamato o assetato.
  - È l'abilità di danno della Negromanzia e l'abilità di lancio delle preghiere del **Sant'Uomo**.
- **Caratteristiche**: Int 3%. Fattore 0,7.
- **Allenamento**: usala ogni volta che hai perso un po' di vita (riuscita = abilità ÷ 100, guadagni da 0 a 120).

### Cavalleria (Chivalry)

- **Tipo**: passiva. Incantesimi del paladino dal libro della Cavalleria; consumano mana e **punti di decima** (si ottengono donando oro in un santuario).
- **Allenamento**: ogni incantesimo dà guadagni da "richiesta" a "richiesta + 50". Purificazione col Fuoco (Cleanse by Fire) e Chiudi Ferite (Close Wounds) all'inizio, poi Consacra Arma (Consecrate Weapon), Furia Divina (Divine Fury), Nemico Giurato (Enemy of One), Luce Sacra (Holy Light) e Nobile Sacrificio (Noble Sacrifice). Con karma negativo gli incantesimi non hanno effetto ma fai comunque pratica. Vedi capitolo 8B.
- **Caratteristiche**: nessuna. È anche l'abilità del **Cavaliere della Morte**.

### Bushido e Ninjitsu

- **Tipo**: passive. Mosse e "incantesimi" dai libri del Bushido e del Ninjitsu (750 monete ciascuno dal mercante di arti marziali).
- **Effetto**:
  - **Bushido**: +0,625% di danno per punto con asce, armi taglienti e armi in asta; influenza la Parata (vedi sopra).
  - **Ninjitsu**: +0,625% di danno per punto; si combina con Seguire Tracce per gli attacchi furtivi.
- **Allenamento**:
  - Bushido: guadagni da "richiesta − 12,5" a "richiesta + 37,5".
  - Ninjitsu: le Forme Animali sono ottime da ripetere.

  Vedi capitolo 8C.
- **Caratteristiche**: nessuna.

### Tessitura Arcana, Misticismo, Infusione, Lancio (Spellweaving, Mysticism, Imbuing, Throwing)

Compaiono nella finestra delle abilità ma **non hanno alcuna funzione** in questo server:

- non hanno incantesimi, pulsante né ricette;
- gli insegnanti non le insegnano;
- per il Lancio esistono libri di studio, ma nessuna arma da lancio di quel tipo è ottenibile.

Il "Mistico" del gioco è il **Monaco** e usa la Lotta (capitolo 8C).

> **Consiglio:** imposta queste quattro abilità sul **lucchetto** o sulla freccia "giù": non sprecare punti del tetto.

---

## 5.12 Abilità del ladro e dell'esploratore

### Nascondersi (Hiding)

- **Tipo**: attiva. Pulsante; attesa 4 secondi (2,5 se occupato o in combattimento). Dentro una casa di cui sei amico riesce sempre.
- **Effetto**:
  - Diventi invisibile.
  - Non puoi nasconderti se un nemico entro min((100 − abilità) ÷ 2 + 8, 18) caselle ti sta combattendo. Da 75 puoi tentare comunque (prova da 75 a 250).
  - Nasconde anche i tuoi animali entro 14 caselle, se li vedi.
- **Caratteristiche**: Des 2,4%, Int 0,6%. Fattore 0,7.
- **Allenamento**: premi il pulsante di continuo (macro), da 0 a 100.
- **A 125**: puoi **sempre** nasconderti, anche in combattimento.

> **Attenzione:** essere nascosti **non ti protegge in modo affidabile** dagli incontri casuali nelle terre selvagge.

### Furtività (Stealth)

- **Tipo**: attiva. Pulsante; attesa 2 secondi.
- **Requisiti**:
  - essere già nascosto;
  - Nascondersi ≥ 30;
  - armatura "leggera" (punteggio sotto 42: niente piastre complete);
  - non essere seguito da qualcuno con Seguire Tracce 125.
- **Effetto**:
  - Puoi camminare restando nascosto per Furtività ÷ 5 passi (20 passi a 100); correre consuma 2 passi.
  - Sotto 25, o a cavallo, ti scopri appena ti muovi.
  - Finiti i passi la Furtività si riprova da sola.
  - Ogni osservatore in più entro 8 caselle rende più difficile riuscire.
- **Caratteristiche**: nessuna.
- **Allenamento**: nasconditi, poi usa Furtività e cammina; ripeti. Bonus dungeon pieno (gruppo A).

### Rivelare Nascosti (Detecting Hidden)

- **Tipo**: attiva. Pulsante → un punto entro 12 caselle; attesa 6 secondi.
- **Effetto**:
  - Raggio di Rivelare ÷ 10 caselle, dimezzato se il tiro fallisce.
  - Rivela giocatori e mostri nascosti, **trappole**, porte segrete e **forzieri nascosti**. I forzieri contengono 100–200 × livello monete, a volte argento, gemme o pepite, e scadono dopo 2 giorni.
  - **Passivo**: passandoci sopra puoi scoprire forzieri nascosti, e le trappole a terra entro 2 caselle diventano visibili con probabilità 20% + abilità ÷ 200.
- **Caratteristiche**: Des 1,2%, Int 1,8%. Fattore 0,7.
- **Allenamento**: usalo in continuazione, soprattutto nei dungeon (gruppo B).

> **Consiglio:** nei dungeon di difficoltà Normale o superiore metti Rivelare Nascosti su un tasto e premilo spesso: trovi forzieri gratuiti ed eviti trappole mortali.

### Disinnescare Trappole (Remove Trap)

- **Tipo**: attiva. Pulsante → una trappola o un contenitore con trappola entro 2 caselle; attesa 5 secondi.
- **Effetto**:
  - **Contenitori con trappola**: richiede abilità ≥ livello della trappola × 10.
  - **Trappole a terra nascoste**: prova da 0 a 100.
  - **Passivo, importantissimo**: quando calpesti una trappola hai una probabilità di evitarla o disattivarla. Quando apri un contenitore con trappola tenti un disinnesco automatico. Il danno delle trappole magiche si riduce di (Disinnescare + Resistenza Energia) ÷ 3 %, fino al 90%.
  - Serve (> 0) per usare i **kit da trappola**.
- **Caratteristiche**: nessuna. Fattore 0,8.
- **Allenamento**: disinnesca tutto quello che trovi nei dungeon (gruppo B). Non si sceglie alla creazione del personaggio.

### Scassinare (Lockpicking)

- **Tipo**: passiva. Doppio clic sui **grimaldelli** → bersaglio la serratura. 3 secondi; 25% di probabilità di rompere il grimaldello se fallisci.
- **Effetto**:
  - Apre forzieri chiusi (prova dal livello della serratura al suo massimo).
  - Apre le porte chiuse dei dungeon (abilità ≥ valore della porta).
  - Le chiavi scheletro richiedono 95.
- **Caratteristiche**: Des 6%.
- **Allenamento**: i meccanici creano **scatole da scasso** di difficoltà crescente (da "dilapidated" a 75 fino a "harder" a 97,5). Aprile e richiudile. Poi passa ai forzieri dei dungeon (gruppo B).

### Curiosare (Snooping)

- **Tipo**: passiva. Doppio clic sullo zaino di qualcun altro entro 1 casella.
- **Effetto**: guardi nello zaino altrui; −4 karma a tentativo. Sui forzieri delle città (coffer) mostra quanto oro contengono (prova 10–125). Serve (o Nascondersi, o Furtività, ≥ 50) per usare il kit da travestimento.
- **Caratteristiche**: Des 7,5%: ottima per la Destrezza.
- **Allenamento**: curiosa negli zaini dei PNG. Costa karma: valuta i libri di studio.

### Furto (Stealing)

- **Tipo**: attiva. Pulsante → un oggetto o un mostro; attesa 5 secondi. Devi avere le mani libere. Qui **non ti rivela** se sei nascosto.
- **Effetto**:
  - **Oggetti**: il peso dell'oggetto determina la difficoltà.
  - **Mostri**: probabilità = Furto ÷ 150 × (1 − |Fama| ÷ 30000) × (1 + Des grezza ÷ 150). Se riesci, prendi un oggetto di bottino casuale, migliore contro mostri famosi.
  - **Forzieri e piedistalli nei dungeon**: puoi rubare forzieri e decorazioni (prova 0–125 o 85–125).
  - Per rubare ai **giocatori** devi essere nella Gilda dei Ladri (karma ≤ −500).
  - Abbassa il karma.
- **Caratteristiche**: Des 3%.
- **Allenamento**: ruba ai mostri durante le esplorazioni (gruppo A).

### Seguire Tracce (Tracking)

- **Tipo**: attiva. Pulsante; attesa 5 secondi. Si apre una finestra con le categorie (animali, mostri, umani, giocatori).
- **Effetto**:
  - Raggio di 25 + abilità ÷ 2 caselle (75 a 100).
  - Trova anche giocatori nascosti, contro il loro Nascondersi e la loro Furtività.
  - Insieme al Ninjitsu dà il bonus "pedinamento".
  - Con Seguire Tracce o Accampamento ≥ 90 puoi avere una casa all'Avamposto dei Ranger.
- **Caratteristiche**: Des 3,8%, Int 3,8%. Fattore 0,7.
- **Allenamento**: usalo continuamente durante i viaggi (gruppo B nei dungeon).
- **A 125**: chi segui è costretto a camminare e non può nascondersi.

### Accattonaggio (Begging)

- **Tipo**: attiva. Pulsante → bersaglio.
- **Effetto**:
  - **Su te stesso**: attiva o disattiva l'"aria da mendicante". Mentre è attiva, i prezzi di vendita ai mercanti usano Accattonaggio invece di Identificare Oggetti (−40 karma per vendita), i guaritori costano meno e il costo di un Negozio scende di Accattonaggio × 25 monete (massimo −3000).
  - **Su un mostro ostile**: lo **pacifica** per 10–120 secondi, al costo di −40 karma e −40 fama.
  - **Su un umano entro 2 caselle**: ricevi fino a metà del suo oro (massimo 1000 monete); −40 karma.
  - Non si usa sui giocatori in zone sicure o nelle case.
  - È l'abilità di lancio dei trucchi del **Giullare**.
- **Caratteristiche**: nessuna.
- **Allenamento**: elemosina dai PNG. Costa karma: considera i libri di studio.

### Avvelenamento (Poisoning)

- **Tipo**: attiva. Pulsante → una pozione di veleno → l'oggetto da avvelenare (arma, cibo, bevanda, shuriken, dardi). Attesa 5 secondi.
- **Effetto**:
  - Si possono avvelenare solo le armi con "Colpo Infettivo" (Infectious Strike).
  - Intervalli di prova: Minore 0–40, Normale 20–60, Superiore 40–80, Mortale 60–100, Letale 80–120.
  - Cariche: 18 − 2 × livello del veleno.
  - −20 karma per uso. Sotto 80, se fallisci, hai il 5% di probabilità di avvelenarti.
  - Le misture lanciate avvelenano al livello Avvelenamento ÷ 25 + 1.
  - Rende più forti gli incantesimi di veleno.
- **Caratteristiche**: Des 1,2%, Int 4,8%: ottima per l'Intelligenza.
- **Allenamento**: avvelena un pugnale. Il veleno si compra dall'alchimista o lo crei con Alchimia.

---

## 5.13 Abilità degli animali, del bardo e della natura

### Addomesticare Animali (Animal Taming)

- **Tipo**: attiva. Pulsante → un animale entro 3 caselle.
- **Effetto**: rende l'animale tuo seguace. Il tentativo dura 4–9 "tiri" di 3 secondi; resta vicino e non farti colpire. Più l'animale è difficile rispetto alla tua abilità, più è probabile che si arrabbi e attacchi. Gli animali che hanno già avuto padroni sono più difficili (+6 di difficoltà per ogni padrone). Alla prima doma le abilità dell'animale scendono all'80% (65% se era paralizzato).
- **Caratteristiche**: For 4,2%, Des 0,6%, Int 1,2%.
- **Allenamento**:

| Fascia | Su cosa |
|---|---|
| 0–50 | compra dall'allevatore |
| 50–55 | orsi bruni, giaguari, cervi (vicino alle città iniziali) |
| 55–70 | orsi grizzly (boschi lontani), lupi, gorilla |
| 70–85 | orsi polari, leopardi delle nevi (isole ghiacciate: le zone innevate sono le migliori per domare), elefanti |
| 85–100 | draghi giovani, viverne, draghi di palude, mammut |
| 100+ | creature delle altre terre |

  Dopo circa 45, portare gli animali in avventura li fa crescere e dà guadagni. Ogni tiro di doma controlla anche Conoscenza Animali.
- **A 125**: esperienza dei tuoi animali ×2 e doppi tiri di abilità per loro.

> **Consiglio:** rilascia l'animale appena domato e ridomalo: ogni tentativo riuscito è un guadagno. Attenzione: molti animali grigi ora diventano aggressivi quando provi a domarli e chiamano i compagni.

### Conoscenza Animali (Animal Lore)

- **Tipo**: attiva. Pulsante → una creatura entro 8 caselle.
- **Effetto**:
  - Mostra statistiche e abilità di una creatura. Sui tuoi animali è sempre gratuito (ma non insegna).
  - Su animali di altri richiede 100; su creature non domabili, 110.
  - Seconda abilità della **Veterinaria**.
  - Conta per i posti seguace (vedi Pastorizia).
- **Caratteristiche**: Int 3%. Fattore 0,7.
- **Allenamento**: il pulsante su qualunque animale (0–120) e i tiri durante la doma.
- **A 125**: la perdita di lealtà dei tuoi animali è dimezzata; alla resurrezione mantengono almeno metà della lealtà.

### Veterinaria (Veterinary)

- **Tipo**: passiva. Doppio clic sulle **bende** → un animale o una creatura (non giocatore).
- **Effetto**: come Guarigione, ma per gli animali (seconda abilità Conoscenza Animali). Tempo di bendaggio "secondi da Destrezza + 1" (minimo 3). Conta per i posti seguace.
- **Caratteristiche**: For 2,4%, Des 1,2%, Int 2,4%.
- **Allenamento**: benda i tuoi animali mentre combattono. Se il tuo animale combatte una creatura con più del doppio dei suoi Punti Ferita, ricevi tiri di guadagno extra.
- **A 125**: +1 lealtà a ogni cura riuscita sul tuo animale.

### Pastorizia (Herding)

- **Tipo**: passiva. Doppio clic sul **bastone da pastore** → un animale domabile → un punto dove spostarlo.
- **Effetto: i posti seguace** (file `FollowersIntelBased.cs`):
  - Base = Intelligenza grezza ÷ 20, tra 2 e 6.
  - Pastorizia ≥ 80: +1; ≥ 100: +1; ≥ 120: +2 (totale +4).
  - Veterinaria, Conoscenza Animali **e** Addomesticare tutte ≥ 80: +1; tutte ≥ 120: +2 ulteriori.
  - Più gli eventuali atti "posto extra" (dagli intermediari di animali).
  - Esperienza degli animali ×(1 + Pastorizia ÷ 500).
- **Caratteristiche**: For 4,9%, Des 1,9%, Int 0,8%. Fattore 0,5.
- **Allenamento**: usa il bastone sugli animali domabili (difficoltà vicino a quella di doma); 15% di probabilità di un tiro a ogni comando ben eseguito dai tuoi animali.
- **A 125** ("fardello condiviso"): metà del danno subito da un animale con istinto di branco si divide tra i tuoi animali dello stesso tipo entro 8 caselle.

### Musica (Musicianship)

- **Tipo**: passiva. Doppio clic su uno **strumento** (tamburo, liuto, arpa, flauto, tamburello).
- **Effetto**:
  - Ogni azione da bardo fa una prova di Musica (riesci a suonare bene con probabilità abilità ÷ 100).
  - Raggio del bardo = 8 + abilità ÷ 15 caselle.
  - Sopra 100, le difficoltà dei bersagli scendono di (Musica − 100) × 0,5.
  - È l'abilità di lancio delle **canzoni del bardo**.
- **Caratteristiche**: Des 2,4%, Int 0,6%. Fattore 0,7.
- **Allenamento**: suona lo strumento (macro). Sale anche usando Pacificazione, Provocazione e Discordia (gruppo B).

### Pacificazione (Peacemaking)

- **Tipo**: attiva. Pulsante → scegli lo strumento → bersaglio.
- **Effetto**:
  - **Su te stesso**: calma tutti entro 8 + abilità ÷ 15 caselle, per Musica ÷ 10 secondi.
  - **Su una creatura**: la calma per 10–120 secondi.
- **Attesa**: 5 secondi dopo un successo, 10 dopo un tentativo.
- **Caratteristiche**: nessuna.
- **Allenamento**: usala su te stesso in città (0–120), poi sui mostri (gruppo A nei dungeon).

### Provocazione (Provocation)

- **Tipo**: attiva. Pulsante → strumento → creatura A → creatura B.
- **Effetto**: A attacca B. La difficoltà è la media delle due. Attesa 10 secondi. Non funziona sui paragoni troppo forti.
- **Caratteristiche**: Des 1,4%. Gruppo A nei dungeon.
- **Allenamento**: fai combattere tra loro i mostri.
- **A 125**: le creature provocate infliggono +20% di danno.

### Discordia (Discordance)

- **Tipo**: attiva. Pulsante → strumento → creatura entro il raggio del bardo.
- **Effetto**:
  - Riduce abilità e caratteristiche del bersaglio di Discordia ÷ 5 %, dimezzato contro creature molto forti.
  - Un nemico in discordia infligge −2% di danno per punto di effetto.
  - Dura finché resti vivo, visibile e nel raggio, più 15 secondi.
- **Attesa**: 12 secondi.
- **Caratteristiche**: Des 0,8%, Int 0,8%.
- **A 125**: almeno il 30% di riduzione, e abbassa anche le resistenze.

> **Consiglio:** la Discordia su un singolo nemico difficile e la Pacificazione sugli altri sono un'ottima tattica per qualsiasi personaggio. Un tamburo costa pochissimo.

### Accampamento (Camping)

- **Tipo**: passiva, senza pulsante. Si usa con oggetti:
  - **Legna da ardere** (kindling, si ottiene usando un pugnale sugli alberi o con Costruzione Archi): doppio clic per accendere un fuoco. Dà 3 tiri extra di guadagno. Non funziona dentro i dungeon. Restare 30 secondi entro 7 caselle dal fuoco rende **sicuro uscire dal gioco** lì.
  - **Tenda da campeggio** (camper tent): richiede 40, e 90 per usarla **dentro i dungeon**; 10 cariche. Ti teletrasporta con i tuoi animali in un'**area tenda sicura** con mercanti e banca condivisa; poi puoi tornare dove eri.
  - **Tenda piccola**: richiede 10, solo nelle terre selvagge; si monta in 5 secondi.
- **Effetto**: ogni 7–10 minuti hai una probabilità di Accampamento ÷ 400 di non consumare fame e sete (fino al 31% a 125). Con 100 puoi tenere animali nella **stalla di casa**.
- **Caratteristiche**: For 6%, Des 4,5%, Int 4,5%: la **migliore abilità per le caratteristiche**.
- **Allenamento**: accendi un fuoco ogni 10 minuti, poi usa le tende.

> **Consiglio:** non pagare mai l'insegnante per l'Accampamento: sotto 10 guadagni a ogni fuoco, ed è proprio quello che ti serve per le caratteristiche.

### Pesca (Fishing)

- **Tipo**: passiva. Doppio clic sulla **canna da pesca** → acqua.
- **Effetto**:
  - In acque profonde e dalla barca escono reti, mappe del tesoro, bottiglie con messaggi e altro.
  - Le reti richiedono 30/50/60/90/100.
  - Gli arpioni ricevono +0,2% di danno per punto (+10% a 100).
- **Regola speciale**: **sopra 60 si migliora solo pescando da una barca** (messaggio *"You will only get better at fishing if you do it from a boat"*).
- **Caratteristiche**: For 1,5%, Des 1,5%. Fattore 0,8.
- **Allenamento**: 0–60 da riva (anche con "I wish to start fishing"), poi in barca.

---

## 5.14 Abilità di conoscenza e utilità

### Guarigione (Healing)

- **Tipo**: passiva. Doppio clic sulle **bende** → te stesso o un altro giocatore entro 5 caselle.
- **Effetto**:
  - **Cura**: riuscita (Guarigione + 10)%; quantità tra Anatomia ÷ 2 + Guarigione ÷ 2 + 25 e quel valore + 100. Ogni colpo subito durante il bendaggio ("scivolata") riduce la cura.
  - **Rimuovere il veleno**: Guarigione e Anatomia ≥ 60.
  - **Resuscitare** un altro giocatore: entrambe ≥ 80.
  - **Tempo**: su te stesso 3 + 4 × (1 − Des ÷ 300) secondi; su altri circa 3,2 − (effetto Des) secondi. Qui la Destrezza conta **molto meno** del solito.
  - È l'abilità di danno delle preghiere del **Sant'Uomo**.
- **Caratteristiche**: For 1,8%, Des 1,8%, Int 2,4%.
- **Allenamento**: bendati ogni volta che perdi anche un solo Punto Ferita (prova 0–120). Le bende si fanno tagliando stoffa con le forbici.
- **A 125**: le bende su te stesso tentano di curare il veleno a metà del bendaggio.

> **Consiglio:** inizia a bendarti appena perdi un Punto Ferita, anche se è uno spreco: le bende costano pochissimo e i guadagni sono tanti.

### Identificare Oggetti (Item Identification)

- **Tipo**: attiva. Pulsante → un oggetto.
- **Effetto**:
  - Identifica pergamene, bacchette, manufatti e oggetti sconosciuti marcati "Use Item Identification". Dopo 5 fallimenti solo un mercante può identificarli.
  - Fallire su pergamene o bacchette può avvelenarti o farti 5–40 danni da fuoco.
  - **Vendere ai mercanti**: il prezzo base viene dimezzato e poi moltiplicato per (1 + 0,03 × Identificare, massimo 100). Con 100 vendi al **doppio** del prezzo normale; i membri della gilda del mercante contano sempre come 100.
  - Reliquie, bottiglie, carichi e oggetti d'antiquariato rendono +Identificare % oro.
- **Caratteristiche**: Int 3%. Fattore 0,8.
- **Allenamento**: il pulsante su ogni oggetto sconosciuto (gruppo B).

### Assaggiare (Taste Identification)

- **Tipo**: attiva. Pulsante → cibo entro 2 caselle (rivela se è avvelenato).
- **Effetto**:
  - Si usa soprattutto con **doppio clic sulle bottiglie, i reagenti e i barili sconosciuti** (prova −5..125). Se fallisci vomiti, singhiozzi e li rovesci.
  - Il cibo che mangi cura Assaggiare Punti Ferita, con Assaggiare % di probabilità di curare il veleno.
  - Rende più forti elisir, misture e pesci magici. È richiesta per la specializzazione **Alchimista** (capitolo 6).
- **Caratteristiche**: For 0,6%, Int 2,4%. Fattore 0,8.
- **Allenamento**: identifica bottiglie sconosciute (sono anche l'unica fonte di alcuni materiali rari).

### Medicina Legale (Forensic Evaluation)

- **Tipo**: attiva. Pulsante → bersaglio entro 10 caselle.
- **Effetto**:
  - **Su un cadavere**: mostra chi l'ha ucciso e chi l'ha saccheggiato; da 60 l'ora della morte, da 90 i segni criminali.
  - Su un forziere o su una serratura: chi li ha derubati o forzati.
  - **Scuoiare**: se riesce una prova (0–100) e hai almeno 5, ottieni +Medicina Legale ÷ 25 + Anatomia ÷ 25 pelli, carne, piume, lana, scaglie e pellicce.
  - Con il **coltello da chirurgo** e dei barattoli si prelevano parti dei corpi.
  - Profanare le tombe ("grave robbing") con la pala da becchino.
  - Serve (≥ 40) per l'**alchimia necrotica** (capitolo 6).
- **Caratteristiche**: Des 0,6%, Int 2,4%.
- **Allenamento**: il pulsante su ogni cadavere.

### Rivelare Nascosti, Disinnescare Trappole, Seguire Tracce

Vedi 5.12.

---

## 5.15 Abilità di raccolta e di artigianato

Le ricette, i materiali e le soglie sono nel **capitolo 7**. Qui trovi gli effetti fuori dall'artigianato e come allenarle.

Regola comune a tutte le abilità di artigianato: **ogni tentativo** di creare un oggetto fa un tiro di guadagno (anche se fallisce), purché la tua abilità sia tra il minimo e il massimo dell'oggetto. La riuscita sale in modo lineare dal minimo (dove hai circa il 50% o meno) al massimo (100%). Conviene sempre creare oggetti con riuscita intorno al 50–70%.

Dal valore **50** in un'abilità di artigianato puoi aprire un **Negozio** (Shoppe) in casa (capitolo 7).

### Estrazione Mineraria (Mining)

- **Tipo**: passiva. Doppio clic su **piccone** o **pala** → il terreno di una montagna o di una grotta. Non si può fare a cavallo.
- **Effetto**:
  - Raccogli minerale grezzo, poi lo fondi in lingotti alla forgia.
  - Le armi contundenti ricevono +0,2% di danno per punto (+10% a 100).
  - Il raggio di scavo dei tesori dipende da Estrazione: 1–4 caselle, con soglie a 51, 81 e 100.
  - Con il libro apposito: **sabbia** (100) e **pietra** (100).
- **Caratteristiche**: For 6%.
- **Allenamento**: il ferro va bene da 0 a 100; i metalli colorati da 65 in su (tabella nel capitolo 7). Gruppo B nei dungeon.

### Taglialegna (Lumberjacking)

- **Tipo**: passiva. Doppio clic su un'**ascia** → un albero.
- **Effetto**: tronchi (normali da 0 a 85, legni speciali da 55 in su: capitolo 7). Le asce ricevono +0,2% di danno per punto (+10% a 100).
- **Caratteristiche**: For 6%.
- **Allenamento**: taglia qualsiasi albero, poi i legni speciali.

### Fabbro (Blacksmithy)

- **Tipo**: passiva (craft). Martello da fabbro o pinze, vicino a un'**incudine e una forgia**.
- **Effetto**: armi e armature di metallo; riparazioni. Da 90 applica gli **oli** ai metalli; da 90 (in gilda, o 110 senza gilda) usa il martello di gilda per migliorare gli oggetti.
- **Caratteristiche**: For 3%.

### Costruzione Archi (Bowcraft/Fletching)

- **Tipo**: passiva (craft). Attrezzi da arciere.
- **Effetto**: archi, balestre, frecce. Gli archi di legno ricevono +0,625% di danno per punto (+6,25% a 100). Corde d'arco speciali a 60/80/100.
- **Caratteristiche**: For 1,8%, Des 4,8%.

### Carpenteria (Carpentry)

- **Tipo**: passiva (craft). Sega, pialle e altri attrezzi da falegname.
- **Effetto**: mobili, contenitori, armi e armature di legno, strumenti musicali, decorazioni per la casa. A 100, con il libro, la **lavorazione della pietra**; da 90 il kit da tassidermia. Aiuta a recuperare i relitti, e serve per costruire le navi.
- **Caratteristiche**: For 6%, Des 1,5%.

### Sartoria (Tailoring)

- **Tipo**: passiva (craft). Kit da cucito.
- **Effetto**: vestiti, armature di cuoio, borchiate e d'osso, borse. Da 90 applica l'olio per il cuoio.
- **Caratteristiche**: For 1,1%, Des 4,9%, Int 1,5%.

### Meccanica (Tinkering)

- **Tipo**: passiva (craft). Attrezzi da meccanico.
- **Effetto**: attrezzi, gioielli, trappole, parti meccaniche; copia chiavi; riparazioni; timone e componenti per le navi.
- **Caratteristiche**: For 1,5%, Des 0,6%, Int 0,9%.

### Alchimia (Alchemy)

- **Tipo**: passiva (craft). Mortaio e pestello, bottiglie vuote, reagenti.
- **Effetto**: pozioni, elisir, misture, trasmutazioni. A 100, con il libro, la **soffiatura del vetro**. Il bonus "Migliora Pozioni" (+10/+20/+30%) è legato all'Alchimia: vedi la nota sotto.
- **Caratteristiche**: Des 1,5%, Int 1,5%.

> **Nota tecnica.** Il bonus alle pozioni controlla il valore interno dell'Alchimia in decimi (soglie 33/66/99), quindi scatta già a 3,3/6,6/9,9 punti invece che a 33/66/99. È quasi certamente un errore: per ora, chiunque abbia almeno 10 in Alchimia ha il +30% pieno.

### Cucina (Cooking)

- **Tipo**: passiva (craft). Padella, mattarello o setaccio, vicino a un fuoco o a un forno.
- **Effetto**:
  - Il cibo cucinato ha un "beneficio" che ti fa mangiare oltre la sazietà, e quindi **accelera i guadagni** (5.3).
  - Con Assaggiare rafforza elisir e misture.
  - Si usa per l'**erboristeria** dei druidi (raccolta con le cesoie da giardino) e per la **cera** delle api.
- **Caratteristiche**: Des 6%, Int 9%: la migliore per l'Intelligenza.
- **Allenamento**: cucina; guadagni anche quando altri mangiano il tuo cibo, e macinando la farina.

### Cartografia (Cartography)

- **Tipo**: passiva (craft). Penna da cartografo.
- **Effetto**:
  - Disegna mappe.
  - **Decifra le mappe del tesoro**: minimo per livello 1–7: 21 / 41 / 61 / 81 / 91 / 100 / 100. Aprire il forziere del tesoro dà altri guadagni.
  - Le mappe dei ranger richiedono Cartografia o Seguire Tracce ≥ 80.
- **Caratteristiche**: Des 2,3%, Int 2,3%.

---

## 5.16 Le gilde PNG e le abilità

Iscriversi a una **gilda PNG** costa 2000 monete; dopo un abbandono il costo diventa 4000 e poi raddoppia a ogni nuovo abbandono. In cambio:

- le abilità della gilda imparano circa ×1,4 più in fretta;
- ricevi un **anello di gilda** con bonus alle abilità (sostituzione: 400 monete);
- i mercanti della gilda pagano meglio.

Dettagli su come iscriversi nel capitolo 6.

| Gilda | Abilità che imparano più in fretta | Anello |
|---|---|---|
| Maghi | Valutare Intelligenza, Magia, Meditazione, Scrittura Arcana, Alchimia | +10 Valutare Int., Magia, Meditazione |
| Guerrieri | Lame Leggere, Mazza, Parata, Spada, Tattica, Guarigione, Anatomia, Cavalleria | +10 Lame Leggere, Mazza, Parata, Spada, Tattica |
| Ladri | Nascondersi, Scassinare, Curiosare, Furto, Furtività, Rivelare Nascosti, Ninjitsu | +10 Nascondersi, Scassinare, Curiosare, Furto, Furtività |
| Bardi | Discordia, Musica, Pacificazione, Provocazione | +10 a tutte e quattro |
| Fabbri | Fabbro, Conoscenza Armi, Estrazione, Meccanica | +20 Fabbro, Conoscenza Armi |
| Minatori | Estrazione, Conoscenza Armi, Fabbro, Meccanica | +30 Estrazione |
| Negromanti | Medicina Legale, Negromanzia, Parlare con gli Spiriti, Alchimia, Scrittura Arcana | +10 Medicina Legale, Negromanzia, Parlare con gli Spiriti |
| Druidi | Conoscenza Animali, Addomesticare, Pastorizia, Veterinaria, Cucina, Accampamento, Seguire Tracce | +10 Conoscenza Animali, Addomesticare, Pastorizia; +5 Cucina |
| Cartografi | Cartografia, Disinnescare Trappole, Scassinare, Pesca | +30 Cartografia |
| Assassini | Lame Leggere, Nascondersi, Avvelenamento, Furtività, Tiro con l'Arco, Alchimia, Bushido | +10 Lame Leggere, Nascondersi, Furtività; +15 Avvelenamento |
| Mercanti | Identificare Oggetti, Conoscenza Armi, Assaggiare | +15 a tutte e tre |
| Sarti | Sartoria, Meccanica | +30 Sartoria |
| Carpentieri | Carpenteria, Taglialegna, Costruzione Archi, Meccanica | +20 Carpenteria, Taglialegna |
| Cucinieri | Cucina, Assaggiare, Meccanica | +20 Cucina, Assaggiare |
| Meccanici | Meccanica, Costruzione Archi, Carpenteria, Sartoria | +30 Meccanica |
| Arcieri | Tiro con l'Arco, Costruzione Archi, Tattica, Cavalleria | +10 Tiro con l'Arco, +20 Costruzione Archi, +10 Tattica |
| Alchimisti | Alchimia, Cucina, Assaggiare | +15 Alchimia, Cucina, Assaggiare |
| Ranger | Accampamento, Seguire Tracce | +25 Accampamento, Seguire Tracce |
| Bibliotecari | Identificare Oggetti, Scrittura Arcana | +20 a entrambe |
| Pescatori | Pesca | +30 Pesca |
| Guaritori | Anatomia, Guarigione, Veterinaria | +15 a tutte e tre |

---

## 5.17 Tabella riassuntiva delle 58 abilità

**Legenda:**

- **Tipo**: A = attiva (pulsante), P = passiva/oggetto, C = artigianato o raccolta, — = non funzionante.
- **For/Des/Int**: probabilità (%) di far salire la caratteristica a ogni guadagno.
- **Fatt.**: fattore di apprendimento (più basso = più lenta).
- **Dungeon**: gruppo del bonus dungeon (A pieno, B ridotto, — nessuno).

| # | Abilità (inglese) | Tipo | Uso principale | For | Des | Int | Fatt. | Dungeon | Titolo |
|---|---|---|---|---|---|---|---|---|---|
| 0 | Alchimia (Alchemy) | C | pozioni | – | 1,5 | 1,5 | 1,0 | — | Alchimista |
| 1 | Anatomia (Anatomy) | A | danno, bende | 0,5 | 0,5 | 2,1 | 0,7 | B | Biologo |
| 2 | Conoscenza Animali (Animal Lore) | A | studiare animali | – | – | 3,0 | 0,7 | B | Naturalista |
| 3 | Identificare Oggetti (Item Identification) | A | identificare, vendere | – | – | 3,0 | 0,8 | B | Mercante |
| 4 | Conoscenza Armi (Arms Lore) | A | identificare armi, danno | 2,3 | 0,5 | 0,3 | 0,8 | — | Maestro d'armi |
| 5 | Parata (Parrying) | P | bloccare colpi | 2,3 | 0,8 | – | 0,8 | B | Duellante |
| 6 | Accattonaggio (Begging) | A | sconti, pacificare | – | – | – | 1,0 | — | Mendicante |
| 7 | Fabbro (Blacksmithy) | C | armi e armature di metallo | 3,0 | – | – | 1,0 | — | Fabbro |
| 8 | Costruzione Archi (Bowcraft/Fletching) | C | archi, frecce | 1,8 | 4,8 | – | 1,0 | — | Arcaio |
| 9 | Pacificazione (Peacemaking) | A | calmare | – | – | – | 1,0 | A | Pacificatore |
| 10 | Accampamento (Camping) | P | fuoco, tende, fame | 6,0 | 4,5 | 4,5 | 0,7 | — | Esploratore |
| 11 | Carpenteria (Carpentry) | C | mobili, legno | 6,0 | 1,5 | – | 1,0 | — | Carpentiere |
| 12 | Cartografia (Cartography) | C | mappe, tesori | – | 2,3 | 2,3 | 1,0 | — | Cartografo |
| 13 | Cucina (Cooking) | C | cibo | – | 6,0 | 9,0 | 1,0 | — | Cuoco |
| 14 | Rivelare Nascosti (Detecting Hidden) | A | trappole, forzieri | – | 1,2 | 1,8 | 0,7 | B | Esploratore |
| 15 | Discordia (Discordance) | A | indebolire | – | 0,8 | 0,8 | 1,0 | A | Demoralizzatore |
| 16 | Valutare Intelligenza (Evaluating Intelligence) | A | danno magico | – | – | 3,0 | 0,8 | B | Studioso |
| 17 | Guarigione (Healing) | P | bende | 1,8 | 1,8 | 2,4 | 1,0 | — | Guaritore |
| 18 | Pesca (Fishing) | C | pesci, tesori marini | 1,5 | 1,5 | – | 0,8 | — | Pescatore |
| 19 | Medicina Legale (Forensic Evaluation) | A | cadaveri, scuoiare | – | 0,6 | 2,4 | 1,0 | — | Detective |
| 20 | Pastorizia (Herding) | P | posti seguace | 4,9 | 1,9 | 0,8 | 0,5 | — | Pastore |
| 21 | Nascondersi (Hiding) | A | invisibilità | – | 2,4 | 0,6 | 0,7 | B | Ombra |
| 22 | Provocazione (Provocation) | A | far combattere mostri | – | 1,4 | 0,2 | 1,0 | A | Agitatore |
| 23 | Scrittura Arcana (Inscription) | A/C | pergamene, libri | – | 0,6 | 2,4 | 1,0 | — | Scriba |
| 24 | Scassinare (Lockpicking) | P | serrature | – | 6,0 | – | 1,0 | B | Infiltratore |
| 25 | Magia (Magery) | P | incantesimi | – | – | 4,5 | 1,0 | A | Mago |
| 26 | Resistenza Magica (Resisting Spells) | P | difesa magica | 0,8 | 0,8 | 1,5 | 0,7 | — | Custode |
| 27 | Tattica (Tactics) | P | danno | – | – | – | 0,8 | B | Tattico |
| 28 | Curiosare (Snooping) | P | guardare negli zaini | – | 7,5 | – | 1,0 | — | Spia |
| 29 | Musica (Musicianship) | P | strumenti, canzoni | – | 2,4 | 0,6 | 0,7 | B | Bardo |
| 30 | Avvelenamento (Poisoning) | A | armi avvelenate | – | 1,2 | 4,8 | 1,0 | — | Assassino |
| 31 | Tiro con l'Arco (Archery) | P | archi, balestre | 0,8 | 2,3 | – | 1,0 | A | Arciere |
| 32 | Parlare con gli Spiriti (Spirit Speak) | A | curarsi, Sant'Uomo | – | – | 3,0 | 0,7 | — | Medium |
| 33 | Furto (Stealing) | A | rubare | – | 3,0 | – | 1,0 | A | Borseggiatore |
| 34 | Sartoria (Tailoring) | C | vestiti, cuoio | 1,1 | 4,9 | 1,5 | 1,0 | — | Sarto |
| 35 | Addomesticare Animali (Animal Taming) | A | domare | 4,2 | 0,6 | 1,2 | 1,0 | — | Domatore |
| 36 | Assaggiare (Taste Identification) | A | bottiglie, cibo | 0,6 | – | 2,4 | 0,8 | B | Assaggiatore |
| 37 | Meccanica (Tinkering) | C | attrezzi, gioielli | 1,5 | 0,6 | 0,9 | 1,0 | — | Meccanico |
| 38 | Seguire Tracce (Tracking) | A | trovare creature | – | 3,8 | 3,8 | 0,7 | B | Ranger |
| 39 | Veterinaria (Veterinary) | P | curare animali | 2,4 | 1,2 | 2,4 | 1,0 | — | Veterinario |
| 40 | Scherma con Spada (Swordsmanship) | P | spade, asce | 2,3 | 0,8 | – | 1,0 | A | Spadaccino |
| 41 | Combattere con Mazza (Mace Fighting) | P | mazze | 2,7 | 0,3 | – | 1,0 | A | Armigero |
| 42 | Lame Leggere (Fencing) | P | pugnali, lance | 1,4 | 1,7 | – | 1,0 | A | Schermidore |
| 43 | Lotta (Wrestling) | P | mani nude, Monaco | 2,7 | 0,3 | – | 1,0 | — | Lottatore |
| 44 | Taglialegna (Lumberjacking) | C | tronchi | 6,0 | – | – | 1,0 | — | Boscaiolo |
| 45 | Estrazione Mineraria (Mining) | C | minerali | 6,0 | – | – | 1,0 | B | Minatore |
| 46 | Meditazione (Meditation) | A | mana | – | – | – | 0,5 | B | Stoico |
| 47 | Furtività (Stealth) | A | muoversi nascosti | – | – | – | 1,0 | A | Furfante |
| 48 | Disinnescare Trappole (Remove Trap) | A | trappole | – | – | – | 0,8 | B | Specialista di trappole |
| 49 | Negromanzia (Necromancy) | P | incantesimi oscuri | – | – | – | 1,0 | A | Negromante |
| 50 | Concentrazione (Focus) | P | rigenerazione | – | – | – | 0,5 | — | Risoluto |
| 51 | Cavalleria (Chivalry) | P | paladino, Cav. della Morte | – | – | – | 1,0 | A | Paladino |
| 52 | Bushido | P | samurai | – | – | – | 1,0 | A | Samurai |
| 53 | Ninjitsu | P | ninja | – | – | – | 1,0 | A | Ninja |
| 54 | Tessitura Arcana (Spellweaving) | — | nessuno | – | – | – | 1,0 | — | Arcanista |
| 55 | Misticismo (Mysticism) | — | nessuno | – | – | – | 1,0 | — | Mistico |
| 56 | Infusione (Imbuing) | — | nessuno | – | – | – | 1,0 | — | Artefice |
| 57 | Lancio (Throwing) | — | nessuno | – | – | – | 1,0 | — | Lanciatore |

---

## 5.18 Consigli finali

- **Allena insieme abilità che si usano insieme**: Guarigione e Anatomia; Magia, Meditazione, Valutare Intelligenza e Concentrazione; Addomesticare Animali, Conoscenza Animali e Veterinaria; un'arma e Tattica.
- **Esci a combattere invece di ripetere macro**: nei dungeon Difficili o più duri le abilità di combattimento e di magia imparano fino all'80% più in fretta.
- **Compra i primi 42 punti** di quasi tutto, ma non delle abilità che vuoi usare per le caratteristiche.
- **Usa i libri di studio** per tutte le abilità lente o che costano karma.
- **Tieni d'occhio il tetto**: con 2500 punti ci stanno tante abilità, ma arrivato al limite imposta su "giù" quelle che non usi più (e subito le quattro abilità non funzionanti).
