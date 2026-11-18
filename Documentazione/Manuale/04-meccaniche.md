# 4. Meccaniche generali

Questo capitolo è il **riferimento delle regole**. Tutti i numeri (costi, tempi, soglie) sono presi dal codice del server. Se una guida trovata in rete dice qualcosa di diverso, considera valido questo capitolo.

---

## 4.1 Caratteristiche

Le tre caratteristiche sono **Forza** (Strength, For), **Destrezza** (Dexterity, Des) e **Intelligenza** (Intelligence, Int).

| Caratteristica | Effetti principali |
|---|---|
| Forza | **Punti Ferita** massimi = For × 1,25 (più i bonus degli oggetti). **Peso massimo** = 100 + 4 × For. Danni in mischia |
| Destrezza | **Vigore** massimo = Des × 1,25 (più i bonus). Velocità d'attacco |
| Intelligenza | **Mana** massimo = Int × 1,25 (più i bonus). Numero di **seguaci** (vedi [4.14](#414-animali-e-seguaci)) |

I bonus a PF, Vigore e Mana dati dagli oggetti hanno **rendimenti decrescenti** e un tetto di **115**.

### Tetti

| | Totale delle caratteristiche | Singola caratteristica |
|---|---|---|
| Avatar | **250** | 150 (175 con tetto totale oltre 275) |
| Non Avatar | **225** | 150 |

Il tetto totale si può alzare con le **pergamene delle caratteristiche** (stat cap scroll, fino a 250, +5 per chi ha il premio veterano), con il premio veterano (+5) e con la missione dell'Ossidiana (+50).

### Come crescono

Le caratteristiche crescono **usando le abilità**:

- ogni volta che un'abilità impostata **in salita** tenta un avanzamento, il gioco prova ad alzare la Forza, poi la Destrezza, poi l'Intelligenza (al massimo una per volta);
- la probabilità dipende dall'abilità usata (vedi la tabella sotto). La caratteristica deve avere la freccia **in salita**;
- **ogni caratteristica può salire al massimo di 1 punto ogni 10 minuti**, con un contatore separato per For, Des e Int. Per gli animali l'attesa è di 5 minuti;
- quando sei al tetto totale, ogni aumento abbassa di un punto una caratteristica che hai messo **in discesa**. Se nessuna è in discesa, non sali più;
- una caratteristica in discesa scende solo finché resta sopra 10.

**Probabilità di aumento per ogni tentativo di avanzamento** (valori più alti = abilità migliori per allenare quella caratteristica):

| Abilità | For | Des | Int |
|---|---|---|---|
| **Accampamento** (Camping) | **6,0%** | **4,5%** | **4,5%** |
| Estrazione Mineraria (Mining), Taglialegna (Lumberjacking), Carpenteria (Carpentry) | 6,0% | – | – (Carpenteria: Des 1,5%) |
| Pastorizia (Herding) | 4,9% | 1,9% | 0,8% |
| Addestrare Animali (Animal Taming) | 4,2% | 0,6% | 1,2% |
| Fabbro (Blacksmithy) | 3,0% | – | – |
| Lotta (Wrestling), Combattere con Mazza (Mace Fighting) | 2,7% | 0,3% | – |
| Curiosare (Snooping) | – | **7,5%** | – |
| Scassinare (Lockpicking) | – | 6,0% | – |
| Cucina (Cooking) | – | 6,0% | **9,0%** |
| Sartoria (Tailoring) | 1,1% | 4,9% | 1,5% |
| Costruzione Archi (Bowcraft/Fletching) | 1,8% | 4,8% | – |
| Seguire Tracce (Tracking) | – | 3,8% | 3,8% |
| Furto (Stealing) | – | 3,0% | – |
| Avvelenamento (Poisoning) | – | 1,2% | 4,8% |
| Magia (Magery) | – | – | 4,5% |
| Scrittura Arcana (Inscription) | – | 0,6% | 2,4% |
| Guarigione (Healing) | 1,8% | 1,8% | 2,4% |
| Veterinaria (Veterinary) | 2,4% | 1,2% | 2,4% |

Abilità come Meditazione, Tattica, Furtività, Negromanzia, Cavalleria, Bushido e Ninjitsu **non** fanno crescere nessuna caratteristica.

> **Consiglio:** ogni 10 minuti fai qualche tentativo di **Accampamento**. Basta accendere dei legnetti, che ottieni usando un pugnale su un albero, e fa crescere tutte e tre le caratteristiche. La **Cucina** è la migliore per l'Intelligenza. Metti **in discesa** (freccia verso il basso) le caratteristiche che non vuoi far crescere oltre un certo valore.

---

## 4.2 Abilità

### Tetti

- **Tetto totale**: **2.500 punti** per un nuovo personaggio.
- **Tetto illimitato**: chi sconfigge lo stregone Mangar e fugge da **Skara Brae** (l'avventura del Bard's Tale) può attraversare il *Mangar's Gate*, che porta il tetto a **10.000 punti**. Lo stesso tetto lo hanno fin dall'inizio i personaggi con partenza speciale "Barbarian", "Alien" o "Wanted".
- **Tetti volontariamente più bassi**: l'Uomo della Luna (Man of the Moon) offre percorsi religiosi che **riducono** il tetto a 1.000 o a 1.500 punti.
- **Tetto della singola abilità**: **100**. Si porta a 105, 110, 115 e 120 con le **pergamene del potere** (power scroll), da usare una dopo l'altra in un santuario. Gli oggetti possono dare bonus anche oltre.

Quando il totale ha raggiunto il tetto, un'abilità sale solo se ce n'è un'altra impostata **in discesa** da cui togliere punti.

### Quanto si guadagna

Ogni avanzamento riuscito dà:

| Valore attuale dell'abilità | Punti guadagnati |
|---|---|
| fino a 25 | 0,3 – 0,5 |
| 25 – 55 | 0,2 – 0,3 |
| 55 – 70 | 0,1 – 0,2 |
| 70 – 80 | 0,1 |
| oltre 80 | 0,1, ma solo con probabilità 1/2 (fino a 85), 1/3 (90), 1/4 (95), 1/5 (100), 1/6 (105), 1/7 (110), 1/8 (oltre) |

Concentrazione (Focus) e Meditazione hanno un ulteriore filtro di 1 su 3. Una **pergamena di Alacrità** (Scroll of Alacrity) moltiplica il guadagno da 2 a 4 volte per la sua durata.

### Probabilità di avanzare

Più l'abilità è bassa, più è facile avanzare. Pesano anche:

- **la modalità**: Avatar e non Avatar imparano alla **stessa velocità**;
- **la fame**: vedi la tabella in [4.4](#44-fame-e-sete). Ben nutrito si impara fino al doppio più in fretta, affamato fino al 50% più lentamente;
- **la gilda dei PNG**: le abilità del tuo mestiere salgono più facilmente;
- **sotto 10 punti** ogni tentativo fa guadagnare qualcosa;
- **la Pesca** (Fishing) sopra 60 sale solo se peschi da una barca.

### Bonus nei dungeon ("ispirazione")

Nei dungeon compare il messaggio *"You gain inspiration from training here"* e alcune abilità salgono più in fretta, in base alla difficoltà del luogo:

| Gruppo di abilità | Bonus massimo (dungeon "Deadly") |
|---|---|
| Combattimento, magia, bardo, furto: Scherma con Spada, Lame Leggere, Combattere con Mazza, Tiro con l'Arco, Magia, Negromanzia, Cavalleria, Bushido, Ninjitsu, Provocazione, Discordia, Pacificazione, Furto, Furtività | **+80%** (+20% per livello di difficoltà) |
| Conoscenze e utilità: Rivelare Nascosti, Meditazione, Assaggiare, Identificare Oggetti, Anatomia, Scassinare, Disinnescare Trappole, Tattica, Musica, Conoscenza Animali, Estrazione Mineraria, Seguire Tracce, Parata, Valutare Intelligenza, Nascondersi | **+27%** |
| Artigianato e altre | nessun bonus |

### Comprare abilità dai PNG

Quasi tutti i mercanti insegnano le proprie abilità:

- costano **10 monete d'oro per punto**;
- il PNG insegna fino a **metà della propria abilità**, mai oltre **50**. Con i mercanti normali significa circa **45–50** punti;
- per insegnare, il PNG deve avere almeno 60 nell'abilità.

La procedura passo per passo è nel [capitolo 2](02-primi-passi.md#passo-4--compra-le-prime-abilità).

I **cloni dei giocatori**, cioè le copie che restano nel mondo quando un giocatore esperto si disconnette, insegnano fino a **80** a **3.000 monete d'oro per punto**. Metà della somma va al giocatore originale.

### Libri di studio (allenamento offline)

| Libro | Allena fino a | Dove si trova |
|---|---|---|
| Standard | 70 | 2 nello zaino iniziale; dal rilegatore (Bookbinder) a 7.500 monete d'oro; come bottino |
| Avanzato | 100 | Bottino, artigianato |
| Leggendario ("tome of lost … techniques") | 120 | Molto raro |

Come funzionano:

- fai **doppio clic** sul libro con l'abilità impostata in salita, fuori dal combattimento;
- al logout inizi a studiare: fai **1 tentativo da 0,1 punti ogni 10 minuti** offline, fino a **200 tentativi (20 punti)** per sessione;
- su abilità difficili (Addestrare Animali, Scassinare, Provocazione, Magia, Bushido, Ninjitsu, Cavalleria, Negromanzia) la probabilità di riuscita è dimezzata;
- dopo ogni sessione il libro ha l'**8%** di probabilità di sbriciolarsi;
- se hai studiato almeno **5 ore**, al rientro hai **Alacrità** su quell'abilità per **30 minuti**;
- il rilegatore ricompra i libri a 500 (standard), 2.500 (avanzati) e 10.000 (leggendari) monete d'oro.

---

## 4.3 Punti Ferita, Vigore e Mana

- **PF, Vigore e Mana** si rigenerano col tempo. La rigenerazione è **dimezzata** se fame o sete sono a 3 o meno, ed è divisa per 1,5 se sono a 10 o meno.
- Ogni passo consuma un po' di Vigore: 1 punto ogni 16 passi a piedi, ogni 48 a cavallo. Di più se sei sovraccarico (vedi [4.5](#45-peso)).

---

## 4.4 Fame e sete

Fame (Hunger) e sete (Thirst) vanno da 0 a circa 20 (il cibo ti riempie fino a 20). Un nuovo personaggio parte a 20/20.

### Quanto calano

Ogni **10 minuti** (il primo controllo avviene dopo 7), se sei connesso, vivo e non in AFK:

| | Fermo | Camminando | Correndo |
|---|---|---|---|
| Fame | −1 | −1 | −1/−3 |
| Sete | −1 | −1/−2 | −2/−3 |

- **Accampamento** riduce il consumo: ogni punto di Accampamento dà circa lo 0,25% di probabilità di saltare il calo, quindi circa il 25% a 100 e il 31% a 125.
- La **protezione del principiante** dimezza il consumo.
- Anche gli animali hanno fame e sete (−1 per controllo).

### Effetti

| Livello | Effetto |
|---|---|
| Fame **0** | "You are starving to death!": −5 PF e −2 Mana a ogni controllo |
| Sete **0** | "You are exhausted from thirst": −5 Vigore e −2 Mana a ogni controllo |
| ≤ 3 | Rigenerazione dimezzata; apprendimento delle abilità ÷1,5 |
| ≤ 10 | Rigenerazione ÷1,5; apprendimento ÷1,25 |
| 11 – 20 | Normale (sopra 15: apprendimento ×1,1) |
| 21 – 40 (cibi "deliziosi" che riempiono oltre 20) | Apprendimento ×1,25 – ×2 |

Il comando `[myhunger` (o `[mhgr`) ti dice come stai: da "starving" a "quite full" per la fame, da "exhausted from thirst" a "not thirsty" per la sete.

> **Consiglio:** porta sempre pane e un otre d'acqua. I cibi elaborati con Cucina ("so delicious") riempiono oltre il normale e ti fanno imparare più in fretta.

---

## 4.5 Peso

- **Peso massimo** = 100 + 4 × Forza (per i personaggi "Puritain": 80 + 3 × Forza). Si conta anche il peso del corpo.
- Puoi superare il limite di **4 pietre** senza conseguenze.
- Oltre quel margine, **ogni passo** costa 5 + (eccesso ÷ 25) punti di Vigore: ÷3 se cavalchi, ×2 se corri. A Vigore zero sei *"too fatigued to move"*.
- Se sei sovraccarico non puoi lanciare **Richiamo** ("Thou art too encumbered to move").
- La **cassetta di sicurezza** non ha limite di peso (vedi [4.10](#410-banca-e-denaro)).

---

## 4.6 Notorietà: karma, fama, criminali, assassini

### Fama e karma

| | Minimo | Massimo |
|---|---|---|
| Fama (Fame) | 0 | 15.000 |
| Karma | −15.000 | 15.000 |

- Più fama hai, meno ne guadagni: il guadagno è ridotto di Fama/100.
- I **non Avatar** guadagnano fama e karma ×0,75; le perdite sono le stesse dell'Avatar.
- Il karma sale uccidendo mostri malvagi e scende con azioni malvagie: Negromanzia, Furto, Accattonaggio verso i mercanti (−40 a vendita), carneficina di cadaveri umani (−50), uccisione di PNG buoni, ecc.
- **Bloccare il karma**: dal menu contestuale di un **ankh** (santuario), entro 4 caselle, puoi bloccare il karma. Da bloccato il karma **non sale più**, ma può ancora scendere. Serve a chi vuole restare "neutro" o malvagio.
- Il tuo **titolo** dipende da fama e karma: per esempio "The Glorious", oppure "The Dread Lord" con karma molto negativo. Sopra 10.000 di fama compare anche "Lord/Lady".

### Criminali (grigi)

Diventi **criminale** per **2 minuti** se commetti un atto criminale **e qualcuno ti vede**, cioè un umano o un PNG entro 12 caselle. Altrimenti ricevi il messaggio *"Your criminal act hasn't been seen"*.

Sono atti criminali:

- saccheggiare il cadavere di un innocente;
- fare a pezzi un cadavere umano (anche −50 karma);
- attaccare o danneggiare un innocente;
- uccidere un cittadino o un bambino (conta anche come omicidio);
- uccidere un mercante (+1 omicidio e −300 karma; nessuna penalità in The Pit, e uccidere mercanti malvagi in DarkMoor dà +300 karma).

### Assassini (rossi)

- Diventi **assassino** (murderer, nome rosso) con **2 omicidi** (kills). Non 5 come nell'Ultima Online classico.
- **Gli omicidi diminuiscono col tempo di gioco**, cioè solo mentre sei connesso:
  - quelli **a breve termine** calano di 1 ogni **4 ore** (il primo dopo 8 ore);
  - quelli **a lungo termine**, quelli che ti rendono rosso, calano di 1 ogni **12 ore** (il primo dopo 40 ore).
- **Xardok** cancella tutti gli omicidi per **10.000 monete d'oro** l'uno: di' "murders" (o "bribe"/"kills"), poi "accept".
- Purificare le **Rune della Virtù** azzera omicidi, criminalità e stato di fuggitivo, e porta fama e karma a 15.000.
- Di' "i must consider my sins" per vedere quanti omicidi hai.

### Chi ti attacca

| Chi | Ti attacca se… |
|---|---|
| **Guardie blu** (Blue Guard) | sei criminale, **oppure** hai karma ≤ **−5.000**, **oppure** hai almeno **2 omicidi**. Lo stesso vale per i tuoi animali |
| Creature "buone" (per esempio unicorni) | karma ≤ **−2.500**, criminale o almeno 1 omicidio |
| Mercanti di alcune città | Rifiutano di servirti con omicidi o karma ≤ −5.000 |
| PNG buoni in generale | Ti ignorano finché il karma è ≥ −6.500 e non sei criminale |

Le guardie blu non resuscitano chi ha almeno 2 omicidi o karma sotto −5.000. Sotto −6.500 di karma nessun PNG buono ti resuscita.

> **Attenzione:** la vecchia guida indica −4.000 come soglia: è sbagliata. Le soglie reali sono −2.500 (creature buone) e −5.000 (guardie). Dire "guardie" **non** evoca guardie su questo server.

<a id="la-bilancia-ordine-e-caos"></a>
### La Bilancia: Ordine e Caos

Il mondo ha un indicatore globale, la **Bilancia** (Balance), che va da 0 a 100.000. Il valore 50.000 è l'equilibrio. Sopra, vince il **Caos** (il Male); sotto, l'**Ordine** (il Bene). Puoi controllarla dal globo colorato nella banca: facendo doppio clic vedi anche il tuo punteggio e i campioni.

| Valore | Messaggio |
|---|---|
| > 90.000 | "Chaos has taken over" |
| > 55.000 | "Chaos is winning" |
| > 45.000 | equilibrio |
| > 20.000 | "Order is winning" |
| altrimenti | "Order is everpresent" |

- **Chi la muove:** solo gli **Avatar**, con le missioni, le uccisioni e le proprie morti.
- **Giurare fedeltà:** nel dungeon *Time Awaits* trovi il **Lord of Time**. Di' "balance" o "pledge" per aprire il menu.
  - "platu verrata nectu" ti lega all'**Ordine**: il karma negativo viene azzerato e da allora il karma non può più scendere sotto zero.
  - "platu verrata narghbrl" ti lega al **Caos**: il karma positivo viene azzerato e da allora il karma non può più salire sopra zero.
  - La scelta è **permanente**. Di' "buy" per spendere i punti della Bilancia.
- **Cosa influenza:** prezzi e scorte dei mercanti, oro trovato, punti ferita dei mostri, penalità di morte e maledizione di viaggio.
- I **campioni** dell'Ordine e del Caos non subiscono la maledizione di viaggio e, alla morte, non perdono abilità né caratteristiche: perdono invece una parte (dal 5% al 15%) della loro influenza sulla Bilancia.

---

## 4.7 Modalità del personaggio

| Modalità | Come si ottiene | Caratteristiche principali |
|---|---|---|
| **Avatar** | Rispondi "Sì" alla 2ª domanda dell'introduzione | Tetto delle caratteristiche 250; partecipa alla Bilancia; tetti di danno più alti, Fortuna ×1,15; alla morte rischia di perdere un po' di abilità e caratteristiche |
| **Non Avatar** | Rispondi "No" alla 2ª domanda | Tetto delle caratteristiche 225; stessa velocità di apprendimento; **non perde mai abilità né caratteristiche** alla morte; il tributo di resurrezione costa il doppio |
| **SoulBound** (anima legata) | Rispondi "Sì" alla 3ª domanda | Morte permanente con azzeramento del personaggio; Soul Tome; sempre Avatar; nessuna protezione del principiante |
| **Fuggitivo** (Wanted) | Partenza speciale | Parte criminale e con 1 omicidio; tetto delle abilità 10.000; nessuna gilda dei PNG; resurrezione ×1,25 |
| **Puritain** | Personaggi delle Midlands | Regole proprie: meno peso, 1–2 seguaci, metà dell'oro, nessuna penalità di morte |

---

## 4.8 Accampamento, tende e riposo

L'**Accampamento** (Camping) è una delle abilità più utili del gioco:

- fa crescere tutte e tre le caratteristiche (vedi [4.1](#41-caratteristiche));
- riduce il consumo di cibo e acqua (vedi [4.4](#44-fame-e-sete));
- dà accesso alle **tende**.

| Oggetto | Requisito | Cosa fa |
|---|---|---|
| **Legnetti** (kindling) | – | Accendi un fuoco. Dopo **30 secondi** entro 7 caselle dal fuoco l'accampamento è "sicuro", e con un **giaciglio** (bedroll) puoi scollegarti in sicurezza |
| **Tenda piccola** (small tent) | Accampamento ≥ 10 | Monta una tenda nelle terre selvagge per 5 minuti. 10 cariche |
| **Tenda da campeggio** (camping tent) | Accampamento ≥ 40 | Ti porta in un **rifugio sicuro** con mercante e banca. Con Accampamento > 66 il rifugio è più bello. **Con Accampamento ≥ 90 funziona anche nei dungeon** (ti porta nella "Dungeon Room"); sotto 90 solo all'aperto. 10 cariche; un fallimento consuma una carica |

Nei rifugi delle tende ti scolleghi all'istante e nessuno può attaccarti. Non puoi lanciare Richiamo o Portale per uscirne o per entrarvi: quando esci, torni da dove eri venuto.

Altre soglie di Accampamento:

- con Accampamento (o Seguire Tracce) ≥ 90 puoi costruire una casa nel *Ranger Outpost*;
- con Accampamento **100** puoi usare il **palo per cavalli** (stable stone) di casa, al costo di 30 monete d'oro per animale a settimana.

---

## 4.9 Morte, spiriti e resurrezione

### Cosa succede quando muori

1. Il tuo corpo diventa un **cadavere** con tutto ciò che portavi. Sotto la protezione del principiante gli oggetti restano invece nel tuo zaino.
2. Diventi uno **spirito** (fantasma, *ghost*). Puoi muoverti, ma non interagire con gli oggetti.
3. Compare automaticamente una **freccia** verso il guaritore o il santuario più vicino disposto a resuscitarti, nella stessa terra.
4. Dopo **30 secondi** compare la finestra di **resurrezione immediata** (vedi sotto).

### I modi per tornare in vita

| Modo | Costo | Penalità |
|---|---|---|
| **Guaritore** (healer), pagando il **tributo** dalla banca | Il tributo (vedi sotto) | Lieve |
| **Guaritore senza oro** sufficiente in banca ("implorare") | 0 | Pesante |
| **Santuario / ankh**: clic destro, "Resurrect", entro pochi passi | 0 | Media |
| **Altari speciali** (di Azrael, del Mietitore, di Anfitrite) | Tributo, oppure nulla se non hai oro | Come il guaritore |
| **Altare di un giocatore** (AltarPlayer) | 0 | Nessuna |
| **Resurrezione immediata** ("pregare gli dei", la finestra dopo 30 s) | 0 | Pesante; si può usare una volta ogni **60 minuti** (10 per i principianti) |
| **Un altro giocatore** (incantesimo Resurrezione, bende, Sacrificio Nobile, pozioni…) | 0 | Solo −10% della fama. Per resuscitare un **Avatar** chi lancia deve sacrificare **10 punti di influenza sulla Bilancia** |

**Chi ti resuscita:**

- i guaritori **di città** rifiutano chi è criminale, ha omicidi o ha karma negativo;
- i **guaritori erranti** (Wandering Healer) e i **guaritori malvagi** (Evil Healer) resuscitano tutti;
- nelle zone malvagie (il castello di Xardok, Umbra, le regioni dei negromanti) i guaritori accettano anche i malvagi.

Dopo la resurrezione hai PF, Vigore e Mana al massimo, sei **nascosto** e la condizione di criminale viene cancellata.

> **Consiglio:** la resurrezione è **gratuita e senza penalità** finché il totale delle tue abilità è ≤ **500** punti, oppure finché la somma delle caratteristiche è ≤ **100**. Un nuovo personaggio (90 punti di caratteristiche) parte quindi già in questa condizione.

### Il tributo per la resurrezione

Il tributo cresce con fama, karma, abilità e caratteristiche:

```
punti = min(Fama/3, 5000) + min(|Karma|/3, 5000) + (totale abilità/3) + min(For+Des+Int, 250) × 50
livello = (punti/1000 − 10) × 1,12        (minimo 1, massimo 150)
tributo = livello × 100 monete d'oro
```

| Esempio | Tributo (Avatar) | Non Avatar |
|---|---|---|
| Caratteristiche 150, abilità 600, fama 1.000, karma 1.000 | **100** | 200 |
| Caratteristiche 250, abilità 2.000, fama 10.000, karma 10.000 | **1.600** | 3.200 |
| Tutto al massimo (caratteristiche 250, abilità 2.500, fama e karma 15.000) | **≈ 4.100** | ≈ 8.200 |

I fuggitivi (Wanted) pagano ×1,25. Il tributo si paga **dall'oro in banca**, non dallo zaino.

### Le penalità

La penalità si compone di due parti.

**1. Perdita di fama e karma** (tutti, tranne SoulBound, Puritain e principianti protetti):

| Come torni in vita | Perdita di fama e karma |
|---|---|
| Guaritore, pagando il tributo | **5%** |
| Santuario/ankh | **10%** |
| Guaritore senza tributo, o resurrezione immediata | **10%** |
| Ankh del Sacrificio | 20% |
| Resurrezione gratuita (abilità ≤ 500 o caratteristiche ≤ 100) | **nessuna** |
| Da un altro giocatore | 10% della sola fama |

Se la somma delle caratteristiche è sotto 125 le perdite maggiori sono dimezzate, ma mai sotto il 5%.

**2. Perdita di abilità e caratteristiche** (**solo Avatar**, solo con caratteristiche ≥ 125, non campioni, non principianti protetti):

- La percentuale di perdita dipende dal tuo karma e dalla Bilancia: chi ha molto karma buono perde di più quando vince il Caos, e viceversa. Il massimo è circa **0,67%** con il tributo e circa **2%** senza.
- **Con il tributo**: ogni caratteristica ha il 33% di probabilità di essere ridotta di quella percentuale. Ogni abilità ha una piccola probabilità (al massimo intorno al 35%) di esserlo.
- **Senza tributo / resurrezione immediata**: tutte le caratteristiche vengono ridotte, e ogni abilità ha il 65% di probabilità di esserlo.
- Le abilità non scendono mai sotto 35 per effetto di questa penalità, e le caratteristiche non scendono sotto 10.
- Messaggi: *"Your body revives... but a little weaker"* (lieve) o *"...but much weaker"* (pesante).

I **non Avatar** ricevono invece *"You avoid any serious penalty for your death…"*: non perdono mai abilità né caratteristiche.

> **Attenzione:** alcune finestre di resurrezione mostrano ancora percentuali vecchie per i non Avatar (10%, 20% o 40% di fama/karma). I valori applicati davvero sono quelli della tabella sopra.

### La resurrezione immediata

Dopo 30 secondi da spirito compare la finestra *"YOU HAVE DIED!"*, che mostra il tributo e l'oro che hai in banca:

- **Resurrect Me**: torni in vita sul posto con la penalità pesante. Puoi farlo una volta ogni **60 minuti** (10 per i principianti protetti);
- **Maybe Later**: la finestra si ripresenta più tardi;
- **Stop Asking**: non te la ripropone più (fino al prossimo accesso).

### Recuperare il cadavere

- `[corpse` (da vivo) ti indica la strada verso il cadavere.
- `[gravedigger` porta i tuoi cadaveri da te per 40.000 monete d'oro (vedi il [capitolo 3](03-comandi.md#32-morte-e-cadaveri)).
- Gli oggetti nel cadavere possono essere presi da altri. Prenderli dal cadavere di un innocente è però un atto **criminale**.

---

## 4.10 Banca e denaro

- Ogni personaggio ha una **cassetta di sicurezza** senza limite di peso, con un massimo di **201 oggetti** (usa sacche e borselli per contenerne di più). È accessibile da qualunque banchiere.
- Parole chiave: "banca", "saldo", "preleva <N>" (max 60.000 per volta), "assegno <N>" (da 5.000 a 10.000.000). Per depositare, lascia l'oro sul banchiere. Vedi il [capitolo 3](03-comandi.md#banchiere).
- **Non ci sono commissioni**, tranne presso i banchieri delle Midlands (1% sui depositi).
- `[bigbank` ingrandisce la finestra della cassetta. `[wealth` mostra il totale della tua ricchezza.

### Convertire monete e gemme

Metti queste valute **nella cassetta di sicurezza** e fai **doppio clic** su di esse:

| Valuta | Cambio in oro |
|---|---|
| Monete di rame | 10 rame = 1 oro |
| Monete d'argento | 5 argento = 1 oro |
| Pepite d'oro | 1 = 1 |
| Gioielli, gemme | ×2 |
| Xormite | ×3 |

L'oro viene consegnato in pile da 60.000 al massimo.

---

## 4.11 Case

1. Compra un **progetto di costruzione** (house construction blueprint) per **500 monete d'oro** da un architetto, da un mediatore immobiliare o dal maestro della gilda dei mercanti.
2. Piazza il progetto: compare un **cantiere**.
3. Metti nel cantiere **assi** o **pietra**, in quantità pari al costo ÷ 20 (i chiodi non servono). Il prezzo in oro viene prelevato **dalla banca**.

| Casa | Prezzo (monete d'oro) |
|---|---|
| Tenda (blu o verde) | 20.736 |
| Casa piccola | 39.168 |
| Negozietto (small shop) | 70.656 |
| Capanna di tronchi / negozio in pietra | 119.040 |
| Torretta | 138.240 |
| Casa della gilda / casa grande con patio | 238.080 |
| Casa a due piani / grande casa di marmo | 403.200 |
| Torre del castello | 576.000 |
| Torre | 921.600 |
| Mastio | 1.536.000 |
| Piramide | 1.612.800 |
| Castello | 2.304.000 |
| Fortezza | 2.995.200 |

- Le **case personalizzate** richiedono Carpenteria **110**.
- **Numero di case per account: illimitato.**
- Le case **non decadono**.
- Ci sono anche **case cittadine** (town houses) da comprare o affittare tramite un cartello: l'affitto si paga dalla banca, ogni 1, 7 o 30 giorni.
- `[myhouses` elenca le tue case. Le parole chiave per bloccare, proteggere e bandire sono nel [capitolo 3](03-comandi.md#311-parole-chiave-per-la-casa).

---

## 4.12 Mercanti ed economia

- **Rifornimento**: ogni mercante si rifornisce ogni **2 ore**, e ogni volta **cambia** quello che vende e quello che compra. Se un fabbro oggi non vuole i tuoi elmi, prova più tardi o da un altro fabbro.
- **Probabilità che un oggetto sia disponibile**: comuni 80% in vendita e 90% in acquisto; normali 50% e 70%; rari 25% e 40%; rarissimi 5% e 5%.
- **Prezzo di vendita** (quanto ti pagano):
  - parte dalla metà del prezzo base, moltiplicata per (1 + 0,03 × **Identificare Oggetti**). Con 100 punti significa fino a ×4;
  - i membri di una gilda vendono ai mercanti del proprio mestiere come se avessero 100 punti;
  - con **Accattonaggio** (Begging) si usa quell'abilità al posto di Identificare Oggetti, ma ogni vendita costa −40 karma;
  - armi e armature: qualità bassa ×0,6, eccezionale ×1,25, più il valore del materiale e delle proprietà magiche.
- **Limite di acquisto**: un mercante compra al massimo 100 oggetti non impilabili, o 300 impilabili, dello stesso tipo.
- **Bilancia**: prezzi e scorte cambiano in base alla Bilancia e al tuo allineamento. Ai buoni conviene un mondo in cui vince l'Ordine, ai malvagi il Caos.
- **Oro trovato**: la quantità di oro di mostri, forzieri e ricompense dipende dalla Bilancia ed è molto più bassa nelle zone malvagie (DarkMoor, Umbra, regioni dei negromanti, The Pit).
- **Mercanti dei giocatori**: nelle zone sicure e pubbliche (banche, rifugi) vendono anche a "clienti simulati", a intervalli di 15–75 minuti. Gli oggetti dentro una sacca non in vendita, a sua volta dentro un'altra sacca non in vendita, sono visibili solo ai giocatori veri.

---

## 4.13 Viaggio

### Portali lunari

- Ci cammini sopra e scegli la destinazione. Puoi andare solo nelle **terre già scoperte**, e ci arrivi insieme ai tuoi animali.
- Trovi il portale lunare più vicino con `[magicgate`. Una **pietra lunare** (moonstone) crea un portale temporaneo di 120 secondi.
- Le destinazioni sono 31, tra cui: Sosaria (Central, Clues, Devil Guard, East, Frozen Isles, Lost Glade, Montor, Moon, WestGate, Yew), Lodoria (Greensky, Islegem, Portshine, South, Springvale, Whisper, Winterlands), Serpent Island e Savaged Empire (Nord/Sud/Ovest), Ambrosia, Umber Veil, Isles of Dread, Kuldar, DarkMoor, l'Isola del Fuoco, l'Isola Perduta e la Volta del Cavaliere Nero.

### Richiamo, Portale e Marchio

**Richiamo** (Recall), **Portale** (Gate Travel) e **Marchio** (Mark) seguono regole speciali:

- **non puoi** usare Richiamo né Portale per entrare in un **dungeon**;
- **non puoi** marcare una runa dentro un dungeon, né nelle acque dei pirati;
- **non puoi** raggiungere una terra che non hai ancora scoperto ("You don't know this location…");
- **non puoi** usare la magia per **uscire** da: Skara Brae, Moonlight Cavern, Kuldar (dopo averla scoperta), Ambrosia, Ravendark, il Gauntlet di Doom, le Midlands, il rifugio della tenda e la stiva delle navi;
- da un dungeon, il Portale crea solo il passaggio di andata, senza ritorno;
- nell'**Underworld** gli incantesimi di Magia e di Ricerca arcana falliscono 2 volte su 3, a meno di indossare l'Orbe dell'Abisso o il Codice della Saggezza;
- i **libri delle rune** (runebook) contengono fino a 16 rune e 12 cariche (fino a 20 se creati da un esperto di Scrittura Arcana). Si ricaricano anche con pergamene di Portale e altri incantesimi di viaggio. Usare una carica non richiede abilità, ma valgono tutte le regole sopra.

### La maledizione di viaggio (Aether's curse)

Ogni volta che viaggi con la magia (Richiamo, Portale, incantesimi di viaggio di altre scuole come Viaggio Sacro o il Passaggio della Natura, oppure entrando in un portale magico) rischi la **maledizione del vuoto**.

- **Chi ne è colpito:** solo i personaggi con **karma positivo**. Non ne sono colpiti i principianti protetti, i campioni della Bilancia e i personaggi con karma negativo.
- **Probabilità:** circa (Bilancia/100.000) × (Karma/15.000) ÷ 1,5, moltiplicata per un fattore casuale tra 1 e 2. Ci sono poi altri moltiplicatori:
  - **×5** se cambi mappa (per esempio da Sosaria a Lodoria);
  - ×0,9 per Richiamo, ÷2 per Portale, ×0,75 per gli altri incantesimi di viaggio;
  - massimo **91%**.
- **Effetto:** perdi una parte di fama e karma, pari a (Bilancia/200.000) × (Karma/15.000), cioè **fino al 50%**. Gli **Avatar** possono inoltre perdere fino a circa l'**1,8%** di alcune abilità (mai sotto 35).
- **Messaggi:** *"You reappear, but shaken"* (non Avatar), *"Crossing the voids of the worlds has weakened you..."* o *"The Aether's curse corrupts your being..."* (Avatar).

> **Consiglio:** per i lunghi spostamenti, specialmente **tra mappe diverse**, usa i **portali lunari**: non danno la maledizione. Tieni Richiamo e Portale per le emergenze.

### Altri mezzi

| Mezzo | Costo | Note |
|---|---|---|
| Traghetto | 500 monete d'oro | Ogni 15 minuti; resta sulla stessa mappa; devi essere entro 6 caselle alla partenza; gli animali non vengono con te |
| Carovana | 300 monete d'oro | Come il traghetto |
| Nave | Atto della nave | Si pilota dando ordini al timoniere |

---

## 4.14 Animali e seguaci

### Numero di seguaci

| Fonte | Seguaci |
|---|---|
| Intelligenza naturale | Int ÷ 20, da un minimo di **2** a un massimo di **6** (Int 60 = 3, 80 = 4, 100 = 5, 120 = 6) |
| Pastorizia (Herding) | +1 a 80, +1 a 100, +2 a 120 (totale +4) |
| Veterinaria + Conoscenza Animali + Addestrare Animali **tutte** ≥ 80 | +1 (e +2 se tutte ≥ 120) |
| Atti "slot per animali" (dal mediatore di animali) | +1 ciascuno, fino a 5 |

Il massimo teorico è **18**. Gli animali più forti occupano più posti: fino a 7 per un singolo animale potentissimo.

### Domare e tenere gli animali

- **Domare**: fai doppio clic su Addestrare Animali e seleziona l'animale. Molti animali grigi si arrabbiano e chiamano i compagni!
- **Lealtà** (massimo 100): ogni 5 minuti c'è il 10% di probabilità di perdere 1–2 punti. Ogni cibo dato all'animale fa guadagnare da 3 a 10 punti. Sotto 10 l'animale "si guarda intorno disperato"; a 0, se non è legato, torna selvatico. Mettere l'animale nella stalla e riprenderlo riporta la lealtà al massimo.
- Gli attacchi ad area dei tuoi animali possono ferire anche te e gli altri tuoi animali, tanto più quanto la lealtà è bassa.
- **Legame** (bonding): dai da mangiare all'animale. Dopo **7 giorni** diventa legato: se muore puoi resuscitarlo. Serve un'abilità di domare sufficiente per quell'animale. Un animale legato morto e abbandonato sparisce dopo 1 giorno.
- **Stalla**: costa **30 monete d'oro** ad animale (dallo zaino o dalla banca). Hai **30 posti**, e puoi aggiungerne 10 per 20.000 monete d'oro, fino a 2.000. Al logout gli animali vengono messi nella stalla automaticamente.
- **Mediatore di animali** (Animal Broker): compra gli animali domati. Il prezzo dipende dalla difficoltà dell'animale e dal suo livello. Le vendite di animali costosi danno a volte premi come atti per slot aggiuntivi o pozioni di domatura e legame istantanei.

---

## 4.15 Cavalcature

- Le cavalcature (cavalli, lama, ostard, draghi delle paludi…) si domano o si comprano.
- **Nei dungeon, nelle caverne, nei rifugi pubblici e in alcuni luoghi speciali non si cavalca.** Entrandoci vieni fatto smontare: la cavalcatura viva viene messa al sicuro ("Your mount is safely waiting for you elsewhere"), quella eterea finisce nello zaino. Quando esci, la cavalcatura viva torna da sola, se hai posti liberi tra i seguaci.
- Negli stessi luoghi si spengono anche gli effetti di velocità: stivali di Hermes, Corsa del Vento (WindRunner), Celerità, ecc.
- **Cavalcature eteree** speciali richiedono:
  - destriero del paladino: Cavalleria 100 e karma ≥ 0;
  - destriero da guerra: 100 in un'abilità di combattimento o in Tattica;
  - destriero del Cavaliere della Morte: Cavalleria 100 e karma ≤ 0;
  - cavallo negromantico: Negromanzia 100;
  - cavalcatura demoniaca: Negromanzia o Magia 100.

---

## 4.16 Gilde

### Gilde dei PNG (di mestiere)

Ci sono **21 gilde**. Ti iscrivi dal **menu contestuale** del maestro di gilda (*Join*) e lasci l'oro richiesto.

- **Costo**: **2.000** monete d'oro la prima volta. Se lasci la gilda, la quota successiva diventa **4.000** e raddoppia a ogni nuova uscita (8.000, 16.000…).
- Puoi far parte di una sola gilda alla volta. Uscendo perdi l'anello.
- **Anello di gilda**: lo può indossare solo il proprietario (non i SoulBound). Se lo perdi, ne compri un altro per 400 monete d'oro.
- **Vantaggi**: le abilità del mestiere salgono più in fretta; ottieni prezzi di vendita migliori dai mercanti del tuo mestiere; solo i membri possono comprare dal maestro di gilda. Le gilde artigiane danno anche strumenti speciali per migliorare gli oggetti.
- I fuggitivi (Wanted) non possono iscriversi. La **gilda dei ladri** richiede karma ≤ −500.

| Gilda | Bonus dell'anello |
|---|---|
| Maghi | Valutare Intelligenza, Magia, Meditazione +10 |
| Guerrieri | Lame Leggere, Combattere con Mazza, Parata, Scherma con Spada, Tattica +10 |
| Ladri | Nascondersi, Scassinare, Curiosare, Furto, Furtività +10 |
| Ranger | Accampamento, Seguire Tracce +25 |
| Guaritori | Anatomia, Guarigione, Veterinaria +15 |
| Minatori | Estrazione Mineraria +30 |
| Mercanti | Identificare Oggetti, Conoscenza Armi, Assaggiare +15 |
| Meccanici | Meccanica +30 |
| Sarti | Sartoria +30 |
| Marinai (pescatori) | Pesca +30 |
| Bardi | Discordia, Musica, Pacificazione, Provocazione +10 |
| Fabbri | Fabbro, Conoscenza Armi +20 |
| Negromanti (Magia Nera) | Medicina Legale, Negromanzia, Parlare con gli Spiriti +10 |
| Alchimisti | Alchimia, Cucina, Assaggiare +15 |
| Druidi | Conoscenza Animali, Addestrare Animali, Pastorizia, Veterinaria +10; Cucina +5 |
| Arcieri | Tiro con l'Arco +10, Costruzione Archi +20, Tattica +10 |
| Carpentieri | Carpenteria, Taglialegna +20 |
| Cartografi | Cartografia +30 |
| Bibliotecari | Identificare Oggetti, Scrittura Arcana +20 |
| Cuochi | Cucina, Assaggiare +20 |
| Assassini | Lame Leggere, Nascondersi, Furtività +10; Avvelenamento +15 |

### Gilde dei giocatori

Sono le classiche gilde di Ultima Online, fondate con un atto di gilda e gestite dalla pietra della gilda. Per uscirne di' "i resign from my guild" (o "abbandono la gilda").

---

## 4.17 Gregari e scudieri

- **Gregari** (henchmen): si comprano da locandieri e osti, anche se non tutti li hanno in vendita.
  - Costano 5.000 (guerriero), 6.000 (arciere) o 7.000 (mago) monete d'oro.
  - Puoi averne al massimo **2**, e ognuno occupa 1 posto tra i seguaci.
  - Li chiami solo in casa, in una locanda o in una taverna.
  - Restano con te per un tempo pari a fama ÷ 5 minuti (60 minuti all'inizio). Puoi prolungarlo dandogli oro, monete o gemme, fino a 6 ore.
  - Quando muoiono tornano un oggetto nel tuo zaino.
- **Scudieri** (squires): si ottengono con un contratto da **5.000** monete d'oro e occupano **3** posti tra i seguaci.
  - Crescono di livello e si possono equipaggiare.
  - Imparano abilità tramite contratti di addestramento da 500 monete d'oro.
  - Si possono far alloggiare dal locandiere degli scudieri per 300 monete d'oro.

---

## 4.18 Trappole

### Trappole del pavimento (nei dungeon)

Nei dungeon ci sono trappole nascoste, che si vedono solo dopo averle rivelate (scintille o luci a terra).

Quando ci passi sopra:

1. c'è il **35%** di probabilità di evitarla automaticamente;
2. altrimenti hai ancora queste possibilità di evitarla, nell'ordine:
   - un tiro di **Disinnescare Trappole** (Remove Trap): l'abilità ti protegge anche senza usarla;
   - un **orbe anti-trappole**, che crei lanciando **Disinnesco Magico** (Magic Untrap) **su te stesso**. La sua potenza è Magia ÷ 2 + 25, fino al 100%;
   - un tiro di **Fortuna**;
   - un **palo da 10 piedi** (ten-foot pole): 50% di riuscita a uso, 20 cariche. Lo vende il fornitore (provisioner) per 500–1.000 monete d'oro.

Esistono **25 tipi** di trappole:

- rivelazione e fil di ferro;
- monete trasformate in piombo;
- perdita di un oggetto, di una caratteristica o di fama;
- veleno (da debole a letale), prosciugamento;
- gemme o reagenti distrutti, libro vincolato;
- **teletrasporto**;
- oggetto maledetto;
- punte, seghe, fiamme, esplosioni, scariche elettriche;
- frecce o pozioni spezzate, nube tossica, gioielli fusi;
- **fossa**;
- cambio di allineamento.

Contro molte trappole hai un **tiro salvezza** (al massimo 60%), basato sulla caratteristica e sulla resistenza adatte. Per esempio: Destrezza per le trappole d'agilità, Forza più resistenza fisica per quelle fisiche, Intelligenza più resistenza all'energia per quelle magiche.

### Trappole dei forzieri

I forzieri possono avere trappole magiche, esplosive, a dardi o velenose. Quando apri il forziere la trappola viene disinnescata automaticamente, se va a segno, nell'ordine:

1. un tiro di **Disinnescare Trappole**;
2. un tiro di **Fortuna**;
3. l'**orbe** anti-trappole;
4. il **palo**;
5. la protezione del principiante.

Altrimenti subisci **50–200 danni**, ridotti dalle resistenze, oppure un veleno da debole a letale.

> **Consiglio:** nei dungeon usa spesso **Rivelare Nascosti**: fa apparire trappole e forzieri nascosti. Prima di aprire forzieri sospetti lancia **Disinnesco Magico** sul forziere: funziona se la tua Magia supera il livello della trappola × 12.

---

## 4.19 Incontri casuali

Fuori dalle zone sicure possono comparire **incontri casuali**: gruppi di mostri, briganti, bande di "assassini" PNG, ma anche guaritori erranti o mercanti.

- Il gioco controlla ogni 300 secondi nei dungeon, ogni 400 nelle terre selvagge e ogni 500 nelle zone sorvegliate.
- Essere **nascosti non protegge** dagli incontri.
- I **principianti protetti** non subiscono agguati.

---

## 4.20 Missioni e taglie

### Bacheca "Seeking Brave Adventurers" (taverne)

- Prendi un incarico con il pulsante *Hire Me* o dal menu contestuale.
- Metà degli incarichi chiede di **uccidere** una creatura in un dungeon (ricompensa ≈ fama della creatura ÷ 5), l'altra metà di **trovare** un oggetto (≈ fama ÷ 3). La ricompensa è adattata in base alla Bilancia.
- Completata la missione, c'è il 5% di probabilità di ricevere anche un oggetto bonus.
- **Per rinunciare** a un incarico, lascia sulla bacheca tanto oro quanto la ricompensa.
- Tra una missione e l'altra deve passare almeno **1 minuto**.

### Altre missioni

| Missione | Dove | Note |
|---|---|---|
| Missioni di pesca ("Seeking Brave Sailors") | Bacheca dei marinai | Come quella degli avventurieri |
| Lavori da ladro | Maestro della gilda dei ladri (menu contestuale) | La ricompensa cresce con Furto e con i lavori consecutivi |
| Contratti da assassino | Xardok | Richiedono la gilda degli Assassini e karma ≤ −1.250 |
| Ricerca di manufatti (artifact) | **Saggi** (sage) nelle città | Lascia 10.000–20.000 monete d'oro (più paghi, migliore è la voce). Il manufatto è in un forziere su un piedistallo in un dungeon. Tra una ricerca e l'altra devono passare **2 giorni** |
| Missioni tributo | PNG con il nome verde | Fama + un oggetto dal boss di un dungeon + oro, in cambio di un oggetto personalizzabile |
| Avventure principali | Skara Brae, Rune della Virtù, terre nascoste… | Si scoprono giocando e leggendo |

`[quests` apre il diario con gli eventi delle missioni.

### Taglie

- **Taglie sui pirati** (Pirate Bounty): sono biglietti che lasciano le navi pirata affondate. Valgono 1.000–3.000 monete d'oro (adattate alla Bilancia) e si consegnano a una **guardia cittadina**.
- **Teste di fuorilegge**: le guardie cittadine pagano anche le teste dei criminali che uccidi.

| Testa | Oro | Karma |
|---|---|---|
| Ladro | 80–120 | +40/60 |
| Bandito | 30–40 | +20/30 |
| Brigante | 50–80 | +30/40 |
| Pirata | 120–160 | +90/110 |
| Assassino | 100–140 | +60/80 |

Non esiste un sistema di taglie sui giocatori.

---

## 4.21 Riepilogo dei numeri più importanti

| Cosa | Valore |
|---|---|
| Oro iniziale | 5.000 |
| Caratteristiche iniziali | 90 punti, ognuna tra 10 e 60 |
| Tetto delle caratteristiche | 250 (Avatar) / 225 (non Avatar) |
| Attesa tra due aumenti della stessa caratteristica | 10 minuti |
| Tetto totale delle abilità | 2.500 (10.000 dopo Skara Brae) |
| Abilità comprate dai PNG | 10 monete d'oro per punto, fino a circa 30–33 (max 42) |
| Protezione del principiante | 20 ore di gioco o 300 punti abilità |
| Resurrezione gratuita e senza penalità | Abilità ≤ 500 o caratteristiche ≤ 100 |
| Resurrezione immediata | Dopo 30 s da spirito; una volta ogni 60 min (10 per i principianti) |
| Diventi assassino | 2 omicidi |
| Le guardie ti attaccano | Criminale, karma ≤ −5.000 o ≥ 2 omicidi |
| Durata della condizione di criminale | 2 minuti |
| Calo degli omicidi | 1 ogni 12 ore di gioco (il primo dopo 40) |
| Cancellare gli omicidi (Xardok) | 10.000 monete d'oro per omicidio |
| Iscrizione a una gilda dei PNG | 2.000, poi 4.000, 8.000… |
| Stalla | 30 monete d'oro ad animale |
| Rifornimento dei mercanti | Ogni 2 ore |
| Fame e sete | −1 ogni 10 minuti (circa) |
| Becchino | 40.000 monete d'oro |
| Traghetto / carovana | 500 / 300 monete d'oro |

---

## Progressione: imparare senza ripetere all'infinito

Ultima Adventures resta un gioco in cui il personaggio cresce *facendo* le cose, ma alcune regole rendono la crescita meno ripetitiva e premiano chi gioca in modo vario e "di ruolo".

### Riposo e ispirazione
Quando esci dal gioco in un **luogo sicuro** — una locanda, una taverna, la tua casa o accanto a un giaciglio da campo (ovunque l'uscita sia immediata) — il tuo personaggio riposa e accumula **ispirazione**:

| Regola | Valore |
|---|---|
| Ispirazione guadagnata | 1 minuto ogni 4 minuti di riposo |
| Massimo accumulabile | 120 minuti (circa 8 ore di riposo) |
| Effetto | la probabilità di migliorare le abilità è **raddoppiata** |
| Durata | scorre mentre giochi; se esci di nuovo, i minuti non usati restano |

Al rientro un messaggio ti dice quanti minuti di ispirazione hai; il comando `[ispirazione` lo mostra in qualsiasi momento.

> **Consiglio:** chiudi sempre la sessione in una locanda o a casa: chi gioca poche ore a settimana recupera così gran parte del distacco da chi gioca ogni giorno.

### Varietà
Se negli ultimi **30 minuti** hai usato almeno **4 abilità diverse** (per esempio combattere, curarti con le bende, nasconderti, scuoiare), impari tutte le abilità il **25% più in fretta**. È il modo in cui il gioco premia l'avventura vera rispetto alla ripetizione meccanica della stessa azione.

### La fascia 80–100
Sopra 80 punti ogni progresso è più difficile. In questa versione la fascia è stata ammorbidita: quando un tentativo di miglioramento riesce, conta davvero con probabilità **2/3** (80–85), **1/2** (85–90), **1/3** (90–95) e **1/4** (95–100), invece di 1/2, 1/3, 1/4 e 1/5. Oltre 100 nulla è cambiato: il gran maestro leggendario resta un traguardo raro.

### Caratteristiche e maestri
- Ogni caratteristica (Forza, Destrezza, Intelligenza) può aumentare una volta ogni **10 minuti** (prima 15).
- I **maestri PNG** insegnano a pagamento un'abilità fino a **50 punti** (al massimo metà della loro abilità; prima il limite era 42): di' «addestra» o «insegna» a un PNG del mestiere giusto.

Tutti questi valori si regolano in `MyServerSettings.cs` (sezione «PROGRESSIONE»).

---

## L'Equilibrio del mondo: oro e mostri

Le azioni buone e malvagie degli Avatar spostano l'**Equilibrio** del mondo (lo mostra il colore dei globi dell'Etere). Oltre agli effetti già descritti, l'Equilibrio modifica:

| Effetto | Mondo molto buono | Neutro | Mondo molto malvagio |
|---|---|---|---|
| Oro ricevuto nelle zone normali (mostri, forzieri, carichi, botteghe, taglie) | 65% | 55% | 45% |
| Oro ricevuto nelle zone malvagie (Umbra, terre dei negromanti, la Fossa, Darkmoor) | 10% | 20% | 30% |
| Punti ferita dei mostri malvagi | un po' meno | normali | un po' di più (circa +20% di robustezza) |
| Punti ferita dei mostri buoni | un po' di più | normali | un po' di meno |

In breve: quando il bene prevale, le terre civili sono più ricche e i mostri malvagi più fragili; quando prevale il male accade il contrario. Le percentuali si regolano in `MyServerSettings.cs` (`BalanceGoldSwing`, `BalanceMonsterSwing`).
