# 10. Le armi e il combattimento

Questo capitolo spiega come funziona il combattimento corpo a corpo e a distanza in Ultima Adventures. Le regole sono quelle di Ultima Online (epoca *Age of Shadows* e successive), ma lo shard le ha modificate in molti punti: cinque mosse speciali per arma invece di due, bonus di danno da abilità insolite, un tetto ai danni del singolo colpo, la schivata nelle Midlands. Tutti i numeri vengono dal codice del server.

Nella seconda parte trovi le **tabelle complete delle armi**, divise per categoria.

> **Nota sui nomi.** Le abilità usano i nomi ufficiali del glossario (Scherma con Spada, Combattere con Mazza, Lame Leggere, Tiro con l'Arco, Lotta, Tattica, Anatomia, Parata...). Le armi sono indicate con il nome italiano e, tra parentesi, il nome inglese che vedi nel gioco.

---

## 10.1 Come si svolge uno scambio di colpi

1. Entri in **modalità guerra** (tasto Tab o doppio clic sul pulsante della barra di stato) e scegli un bersaglio.
2. Quando il tempo di attacco dell'arma è trascorso, il server tira per **colpire** (10.2).
3. Se il colpo va a segno, il difensore può **parare** con lo scudo o con l'arma (10.3) e, nelle Midlands, **schivare**.
4. Se il colpo passa, si calcola il **danno** (10.5): tiro base dell'arma, bonus delle abilità e degli oggetti, moltiplicatori (mosse speciali, ammazza-creature), poi le **resistenze** del bersaglio.
5. Infine si applicano gli effetti "al colpo" dell'arma (incantesimi automatici, risucchi) e quelli della **mossa speciale** eventualmente armata (10.8).

Ogni colpo può allenare l'abilità d'arma, **Tattica** e **Anatomia**. Non si guadagnano abilità di combattimento colpendo altri giocatori, animali addomesticati o creature evocate.

---

## 10.2 Colpire e mancare

La probabilità di colpire confronta l'abilità d'arma dell'attaccante con quella del difensore (il difensore usa l'abilità dell'arma che *lui* impugna):

```
probabilità = (abilità attaccante + 20) × (100 + BonusColpire)
              ─────────────────────────────────────────────────
              2 × (abilità difensore + 20) × (100 + BonusDifesa)
```

- **BonusColpire** (*Hit Chance Increase*, "Aumento probabilità di colpire") somma le proprietà degli oggetti e alcuni effetti: Furia Divina (+10), forma del lupo o della volpe mistica (+25), il bonus di precisione della mossa speciale. Il **tetto è 45%**.
- **BonusDifesa** (*Defense Chance Increase*) somma le proprietà degli oggetti, la mossa **Blocco** e altri effetti. Furia Divina lo riduce di 20, Discordia del bardo fino a −28. Anche qui il **tetto è 45%**.
- La probabilità minima è sempre **2%**.

Esempio: tu hai 80 in Scherma con Spada, l'orco ha 60 in Lotta. Senza bonus: (100 × 100) / (2 × 80 × 100) = 62,5%. Con +20% BonusColpire sali al 75%.

> **Consiglio:** contro mostri molto abili il primo modo per colpire di più non è alzare il danno ma il **BonusColpire** (anelli, bracciali, armi magiche). Il **BonusDifesa** è invece la prima difesa di un guerriero: ogni punto conta come un punto di armatura in più.

---

## 10.3 Parata e schivata

### Parata con lo scudo

Con uno **scudo** equipaggiato la probabilità di parare è:

- (Parata − Bushido senza bonus razziali) / 400, cioè **25% con Parata 100** e nessun Bushido;
- **+5%** se Parata o Bushido arrivano a 100;
- se hai meno di **80 Destrezza**, la probabilità viene moltiplicata per (20 + Destrezza) / 100.

Il colpo parato fa **0 danni** e annulla la mossa speciale dell'attaccante (che non paga mana). Lo scudo perde un po' di durabilità.

### Parata con l'arma (senza scudo)

Senza scudo si para con l'arma, ma conta soprattutto il **Bushido**: probabilità = Parata × Bushido / 48000 (arma a una mano) o / 41140 (a due mani), con +5% a 100 di Parata o Bushido. Con **125 di Parata** e nessun Bushido vale comunque almeno Parata/800 + 5% (circa 20%). Si para anche a mani nude o con un'arma a distanza, ma solo con almeno **125 di Parata**.

> **Consiglio:** la guida originale lo dice chiaramente: all'inizio non comprare Parata se non hai anche Bushido. Bushido rende inutile lo scudo e permette di parare con un'arma a due mani, che fa molti più danni.

### Contrattacco (riposta)

- Se impugni un'arma di **Lame Leggere** e hai la Lame Leggere **base a 125**, ogni parata riuscita ha il **20%** di probabilità di generare un contrattacco immediato.
- Nelle **Midlands** qualunque parata riuscita può dare un contrattacco se superi anche una prova di schivata.

### Schivata (solo nelle Midlands)

Nelle **Midlands** (e per i personaggi in modalità "puritana") esiste un tiro di **schivata** basato sull'**Agilità**, un valore da 0 a 1 calcolato così:

- parte da 1 e scende con il **peso dell'armatura** (indice di Furtività ×1,5) e le **resistenze** totali, rapportati a Forza/3 + Destrezza;
- scende fino a 0,5 se sei **stanco** (Vigore basso);
- scende se porti molto peso nello zaino;
- −0,15 se stai **correndo**.

La probabilità di schivare è Agilità/1,25 (massimo 80%), più un quarto della differenza di Agilità tra te e l'attaccante, **dimezzata se sei a cavallo** e ridotta se hai meno di 50 di Destrezza. Nelle Midlands l'Agilità aumenta anche la probabilità di colpire (+15% × Agilità) e la velocità (fino a +50%), mentre **gli oggetti magici non danno alcun bonus**.

---

## 10.4 Velocità d'attacco

Ogni arma ha una velocità in secondi (colonna **Vel.** delle tabelle). Il tempo reale fra due colpi è:

```
tick = arrotonda_giù( (Vel. × 4 − Vigore/30) × 100 / (100 + BonusVelocità) )
tempo = tick × 0,25 secondi        (minimo 1,25 s)
```

- Il **Vigore** (Stamina) attuale conta: ogni 30 punti tolgono un quarto di secondo. Un guerriero stanco colpisce più lentamente.
- **BonusVelocità** (*Swing Speed Increase*): proprietà degli oggetti (massimo 100 dagli oggetti), Furia Divina +10, Esecuzione Onorevole, Doppia Impugnatura; Discordia lo riduce. Il **totale è limitato a +60%**.
- Il tempo minimo è **1,25 secondi** (5 tick).

| Vel. arma | Vigore 100, nessun bonus | Vigore 150, nessun bonus | Vigore 100, +30% |
|---|---|---|---|
| 2,00 | 1,25 s | 1,25 s | 1,25 s |
| 2,50 | 1,75 s | 1,25 s | 1,25 s |
| 3,00 | 2,25 s | 1,75 s | 1,50 s |
| 3,50 | 2,75 s | 2,25 s | 2,00 s |
| 4,00 | 3,25 s | 2,75 s | 2,50 s |
| 4,50 | 3,75 s | 3,25 s | 2,75 s |
| 5,00 | 4,25 s | 3,75 s | 3,25 s |

> **Consiglio:** un'arma lenta e pesante (alabarda, martello da guerra) fa molto danno per colpo ma ti lascia scoperto fra un colpo e l'altro. La guida originale suggerisce, con le armi a due mani, di **colpire e allontanarti** mentre l'arma si ricarica, invece di restare fermo a prendere colpi.

---

## 10.5 Il danno

### Danno base

Ogni colpo tira un numero casuale fra il **danno minimo e massimo** dell'arma (colonna **Danno**), a cui si aggiunge il **bonus del materiale** con cui l'arma è fatta (stesso valore al minimo e al massimo):

| Materiale | Bonus | Materiale | Bonus | Materiale | Bonus |
|---|---|---|---|---|---|
| Ferro (iron) | 0 | Rame opaco (dull copper) | +1 | Ferro ombra (shadow iron) | +2 |
| Rame (copper) | +3 | Bronzo (bronze) | +4 | Oro (gold) | +4 |
| Agapite | +5 | Verite | +5 | Valorite | +6 |
| Nepturite | +6 | Ossidiana (obsidian) | +6 | Acciaio (steel) | +7 |
| Ottone (brass) | +8 | Mithril | +9 | Xormite | +9 |
| **Nanico (dwarven)** | **+18** | | | | |
| Cuoio *spined* (deep sea) | +1 | *horned* (lizard) | +2 | *barbed* (serpent) | +3 |
| *necrotic* / *volcanic* | +4 | *frozen* | +5 | *goliath* | +6 |
| *draconic* | +8 | *hellish* | +9 | *dinosaur* | +10 |
| **Alieno (alien)** | **+18** | | | | |
| Frassino / Ciliegio (ash / cherry) | +1 | Ebano / Quercia dorata | +2 | Noce americano / Mogano | +3 |
| Quercia / Pino (oak / pine) | +4 | Palissandro / Noce (rosewood / walnut) | +5 | Legno di deriva (driftwood) | +7 |
| Legno spettrale (ghostwood) | +8 | Legno pietrificato (petrified) | +9 | **Legno elfico (elven)** | **+18** |

I materiali speciali aggiungono anche **danno elementale** (per esempio il bronzo converte il 40% del danno in fuoco, la verite il 40% in veleno, il cuoio *barbed* il 70% in veleno): vedi il capitolo 11, tabella dei materiali.

### Bonus percentuali delle abilità e degli oggetti

Al danno base si somma una percentuale data da:

| Fonte | Bonus | Vale per |
|---|---|---|
| **Forza** | 0,3% per punto, +5% extra a 100 | tutte le armi |
| **Anatomia** | 0,5% per punto, +5% extra a 100 (55% a 100) | tutte le armi |
| **Tattica** | 0,625% per punto, +6,25% extra a 100 (68,75% a 100) | tutte le armi |
| **Conoscenza Armi** | 0,625% per punto, +6,25% a 100 | tutte le armi (regola di questo shard) |
| **Ninjitsu** | 0,625% per punto, +6,25% a 100 | tutte le armi |
| **Bushido** | 0,625% per punto, +6,25% a 100 | solo armi da taglio, asce e armi d'asta |
| **Taglialegna** | 0,2% per punto, +10% a 100 | solo asce |
| **Estrazione Mineraria** | 0,2% per punto, +10% a 100 | solo armi contundenti (mazze, martelli) |
| **Pesca** | 0,2% per punto, +10% a 100 | solo arpioni |
| **Magia** e **Negromanzia** | 0,625% per punto ciascuna, +6,25% a 100 | solo bacchetta del mago, bastoni e scettri del mago, scettro |
| **Costruzione Archi** | 0,625% per punto, +6,25% a 100 | solo armi a distanza di legno |
| **Aumento danno** degli oggetti (*Damage Increase*) | valore della proprietà | tetto **300%** (incluse Forma Bestiale +25, Furia Divina +10, Colpo Letale +20) |
| Qualità | eccezionale +20%, scadente −20% | |
| Vecchie armi magiche "di rovina/potenza/forza/vigore/vanquishing" | +15 / +20 / +25 / +30 / +35% | |

Maestria Difensiva e Discordia riducono il danno.

### Il tetto del colpo singolo

Dopo i bonus, il danno di un colpo viene "schiacciato" da una curva a rendimenti decrescenti. Per i giocatori la curva ha tetto 150 (200 se hai lo stato di **Avatar**), per i mostri 225:

| Danno prima della curva | 20 | 30 | 50 | 80 | 100 | 150 | 200 |
|---|---|---|---|---|---|---|---|
| Giocatore | 19 | 27 | 41 | 55 | 63 | 76 | 80 |
| Giocatore Avatar | 20 | 28 | 44 | 64 | 71 | 87 | 102 |
| Mostro | 20 | 29 | 46 | 67 | 74 | 94 | 111 |

In pratica, oltre circa 50 punti, ogni bonus di danno rende sempre meno. Conviene spesso investire in **velocità** e **probabilità di colpire**.

### Moltiplicatori

Dopo la curva si applicano i moltiplicatori, sommati fra loro con **tetto +300%**:

| Effetto | Bonus |
|---|---|
| Mossa speciale armata | il moltiplicatore della mossa (es. Colpo Devastante +100%, Colpo Schiacciante +50%) |
| **Ammazza-creature** (*slayer*) contro la famiglia giusta | **+75%** (non c'è differenza fra slayer normale e "super slayer") |
| Nemico Unico (Cavalleria) contro il tipo scelto | +50% |
| Istinto del branco (animali) | variabile |
| Onore attivo a distanza 1 | +25% |
| Ammazza-creature d'argento contro un negromante trasformato | +25% |
| A cavallo, solo nelle Midlands | +15% |
| Secondo colpo di Doppio Colpo | −10% |

L'**attacco furtivo**: se colpisci uscendo da Nascondersi, il danno aumenta di circa 1% ogni 1,5 punti di Nascondersi (più Furtività se ti stavi muovendo in furtività), fino a **+125%** (dimezzato con le armi a distanza). Il gioco ti dice la percentuale ottenuta.

### Resistenze

Il danno finale si divide fra i cinque tipi (fisico, fuoco, freddo, veleno, energia) secondo le percentuali dell'arma, e ogni parte viene ridotta della resistenza corrispondente del bersaglio. Le resistenze dei giocatori hanno **tetto 70** (60 sotto Maledizione per le resistenze non fisiche; 90 fisica con la Pelle di Pietra della Ricerca arcana). La mossa **Ignora Armatura** e alcune mosse elementali infliggono danni che ignorano le resistenze.

> **Consiglio:** contro un mostro con resistenza fisica alta (golem, elementali di terra) un'arma che converte il danno in un altro elemento (bronzo, verite, cuoi speciali) rende molto più di un'arma più forte ma solo fisica.

---

## 10.6 Requisiti, mani e proprietà speciali delle armi

- **Forza richiesta** (colonna **For**): se non ce l'hai non puoi impugnare l'arma. La proprietà *Lower Requirements* e alcuni materiali (oro, acciaio, ottone, mithril, xormite, ecc.) la riducono.
- **Mani**: un'arma a **2 mani** non lascia spazio allo scudo. Con un'arma a 1 mano puoi portare uno scudo o una fonte di luce magica.
- **Spell Channeling** ("canalizzazione"): puoi lanciare incantesimi senza toglierti l'arma. I **guanti da pugile** e i **guanti da lancio** lo hanno sempre, insieme alla **bacchetta del mago** e al **bastone glaciale**.
- **Mage Weapon** ("arma del mago"): se la tua Magia è più alta dell'abilità d'arma, colpisci usando la Magia.
- **Use Best Weapon Skill**: usi la più alta fra Scherma con Spada, Lame Leggere e Combattere con Mazza.
- **Durabilità** (colonna **Durab.**: punti iniziali minimo–massimo): ogni colpo dato o ricevuto può consumarla. Quando arriva a zero l'arma si rompe. Fai riparare dal fabbro (metallo), dal carpentiere (legno) o dal sarto/conciatore (cuoio).
- **Avvelenare l'arma**: con Avvelenamento puoi applicare veleno alle lame (spade, pugnali, lance). Le cariche si consumano a ogni colpo.

---

## 10.7 Abilità d'arma e famiglie

| Famiglia | Abilità | Tipo di colpo | Note |
|---|---|---|---|
| Spade, lame da taglio, asce, armi d'asta | Scherma con Spada | taglio | Le asce ricevono il bonus di Taglialegna, spade/asce/armi d'asta quello di Bushido |
| Kryss, lance, forconi, pugnali da punta | Lame Leggere | punta | Con 125 di Lame Leggere: 20% di contrattacco dopo una parata |
| Mazze, martelli, bastoni | Combattere con Mazza | contundente | Le mazze ricevono il bonus di Estrazione Mineraria |
| Archi, balestre, guanti da lancio, bastoni del mago, arpioni | Tiro con l'Arco | distanza | Serve stare fermi un attimo per tirare (salvo Tiro in Movimento); servono munizioni |
| Mani nude, guanti da pugile | Lotta | contundente | I guanti da pugile hanno Spell Channeling |

---

## 10.8 Le mosse speciali

In Ultima Adventures ogni arma ha fino a **cinque mosse speciali** invece delle due classiche. Si sbloccano così:

| Mossa | Abilità d'arma e Tattica richieste |
|---|---|
| 1ª (primaria) | 70 |
| 2ª (secondaria) | 80 |
| 3ª | 90 |
| 4ª | 100 |
| 5ª | 110 |

Servono **entrambe**: l'abilità dell'arma che impugni *e* Tattica (fanno eccezione Colpo dell'Ombra e Colpo Infettivo, che non chiedono Tattica). Con la proprietà *Use Best Weapon Skill* vale la migliore fra Scherma con Spada, Lame Leggere e Combattere con Mazza.

### Come attivarle

- Quando impugni un'arma e hai le abilità sufficienti, compare automaticamente una **barra delle mosse speciali** (una finestra con le icone). Clicca l'icona: la striscia a sinistra diventa **rossa** e la mossa è **armata**. Il prossimo colpo che va a segno la esegue. Clicca di nuovo per disarmarla.
- Comandi da tastiera (funzionano solo con la barra aperta): `[Set1` … `[Set5` (oppure `[SetPrimaryAbility`, `[SetSecondaryAbility`... `[SetFifthAbility`).
- `[SAD` (o `[SpecialAttacksDisplay`) riapre la barra se l'hai chiusa.
- `[abilitynames` mostra o nasconde i nomi accanto alle icone.
- Il **Libro delle abilità d'arma** (*Weapon Abilities*), in vendita dai mercanti, descrive ogni mossa e mostra il costo in mana calcolato per il tuo personaggio.

> **Attenzione:** i pulsanti "abilità primaria/secondaria" del client (le icone del libro di combattimento di Ultima Online e le macro corrispondenti) **non funzionano** su questo server: il server non gestisce quel comando. Usa la barra o i comandi `[Set1`–`[Set5`.

### Costo in mana

Ogni mossa ha un costo base (tabella sotto), ridotto così:

- **−5** se la somma di Scherma con Spada, Combattere con Mazza, Lame Leggere, Tiro con l'Arco, Parata, Taglialegna, Furtività, Lotta, Avvelenamento, Bushido e Ninjitsu arriva a 200; **−10** se arriva a 300;
- poi la percentuale di **Riduzione costo mana** (*Lower Mana Cost*) degli oggetti;
- il costo **raddoppia** se usi un'altra mossa entro **3 secondi** dalla precedente.

Il mana si paga in genere solo se il colpo va a segno (Tiro in Movimento e Doppio Tiro lo consumano anche se manchi). Un colpo parato annulla la mossa senza costo.

### Effetti delle mosse

Nella tabella "abilità/20" indica l'abilità d'arma divisa per 20 (da +0 a +6). "Ignora resistenze" significa che quel danno non è ridotto dall'armatura.

| Mossa (nome inglese) | Mana | Effetto |
|---|---|---|
| **Ignora Armatura** (*Armor Ignore*) | 20 | Il colpo (a −10% di danno) ignora tutte le resistenze. |
| **Perforazione** (*Armor Pierce*) | 20 | Danno ×2. Nonostante il nome non ignora l'armatura. |
| **Danza delle Lame** (*Bladeweave*) | 10 | Esegue a caso una fra Ignora Armatura, Dissanguamento, Colpo Concussivo, Colpo Schiacciante, Doppio Colpo, Colpo Mortale, Colpo Paralizzante. Paga anche il mana della mossa scelta (raddoppiato, vedi sopra). |
| **Dissanguamento** (*Bleed Attack*) | 30 | Il bersaglio sanguina per 14 secondi (7 colpi crescenti): totale pari al danno del colpo × (0,6 + Anatomia/200), doppio contro i mostri. Non funziona su chi è in forma di Lich o Spettro. |
| **Blocco** (*Block*) | 20 | Per 6 secondi +50/60% BonusDifesa (dipende da Tattica o Anatomia), sempre entro il tetto del 45%. |
| **Colpo Concussivo** (*Concussion Blow*) | 20 | 10–35 danni diretti, maggiori quanto più differiscono le percentuali di PF e Mana del bersaglio; oltre 50 punti di differenza lo paralizza per 1,5 s. |
| **Colpo Schiacciante** (*Crushing Blow*) | 25 | Danno +50% e −15 alla resistenza fisica del bersaglio per 8 s. |
| **Maestria Difensiva** (*Defense Mastery*) | 20 | Per 3 s: resistenza fisica +50/+80, ma il tuo danno scende del 50–80%. |
| **Disarmare** (*Disarm*) | 20 | Mette l'arma del bersaglio nel suo zaino e gli impedisce di riprenderla per 5 s. Il mana si paga solo se riesce. |
| **Disarcionare** (*Dismount*) | 20 | Devi essere a piedi (con la lancia da cavaliere anche a cavallo) e il bersaglio a cavallo: cade e non può risalire per 10 s. Gli scagnozzi (*henchmen*) sono immuni. |
| **Svestire** (*Disrobe*) | 15 | Toglie al bersaglio la veste interna (o quella esterna) per 5 s. |
| **Doppio Tiro** (*Double Shot*) | 30 | Spara una seconda freccia, a +30% di danno se la prima ha colpito. |
| **Doppio Colpo** (*Double Strike*) | 20 | Un secondo colpo immediato (a −10%), danno della mossa +20%. |
| **Doppia Impugnatura** (*Dual Wield*) | 30 | Per 6 s velocità d'attacco +20/50% (dipende da Tattica). |
| **Finta** (*Feint*) | 25 | Per 4 s il danno che quel nemico ti infligge cala del 20–50%. |
| **Freccia di Forza** (*Force Arrow*) | 20 | Danno +30%, riduce la difesa del bersaglio e può respingerlo di 2 caselle. |
| **Forza della Natura** (*Force of Nature*) | 30 | 4 s di danno da veleno al secondo, basato su Parlare con gli Spiriti; prosciuga anche Vigore. |
| **Turbine Frenetico** (*Frenzied Whirlwind*) | 30 | Colpisce il bersaglio e chi gli sta entro 2 caselle con 50–60 danni diretti in 3 s; si accumula fino a +45%. |
| **Colpo Infettivo** (*Infectious Strike*) | 15 | Serve un'arma avvelenata: applica il veleno (livello massimo Avvelenamento/20), con possibilità di alzarlo di un livello. Costa 20 di Karma. |
| **Freccia Fulminante** (*Lightning Arrow*) | 20 | Chi sta entro 2 caselle dal bersaglio subisce un colpo completo ×1,5. |
| **Colpo Mortale** (*Mortal Strike*) | 25 | Il bersaglio non può essere curato per 6 s (12 s se è un mostro). |
| **Tiro in Movimento** (*Moving Shot*) | 15 | Puoi tirare correndo; precisione −25 a 0 di abilità fino a 0 a 125; danno +50% e più se ti muovi veloce. |
| **Colpo ai Nervi** (*Nerve Strike*) | 20 | Danno che ignora le resistenze (fino a 15 + 10 casuali) e probabilità di paralizzare 2 s. |
| **Colpo Paralizzante** (*Paralyzing Blow*) | 20 | Paralizza 3 s i giocatori, 6 s i mostri; poi il bersaglio è immune per 8 s. |
| **Attacco Psichico** (*Psychic Attack*) | 30 | Svuota il mana del bersaglio per 5 s; poi per altri 5 s ogni incantesimo che inizia gli infligge 25 danni e viene interrotto. |
| **Fendente in Corsa** (*Riding Swipe*) | 30 | 15–30 danni fisici; se hai percorso 5 caselle dopo averla armata fa +50% e colpisce anche in un cono dietro il bersaglio. |
| **Freccia del Serpente** (*Serpent Arrow*) | 30 | Avvelena: Mortale con Avvelenamento 100, Superiore oltre 85, Normale oltre 65, altrimenti Minore. Oltre 110 di Avvelenamento riduce anche del 30% la resistenza al veleno. |
| **Colpo dell'Ombra** (*Shadow Strike*) | 20 | Danno +25% e ti nasconde. Richiede 80 in Furtività. |
| **Colpo d'Artiglio** (*Talon Strike*) | 30 | Danno +40% più un danno continuo extra che ignora le resistenze. |
| **Attacco a Turbine** (*Whirlwind Attack*) | 15 | Colpisce tutti i nemici adiacenti (bonus di danno fino a ×2 in base al numero e a Tattica). |
| **Colpo d'Achille** (*Achilles Strike*) | 20 | 5–25 (+abilità/20) ×1,3 danni fisici che ignorano le resistenze; immobilizza il bersaglio 4+ s e poi lo rallenta per 7 s. Mentre è immobilizzato hai +15% a colpirlo. |
| **Colpo Consacrato** (*Consecrated Strike*) | 20 | Danno +50%; ogni tre usi sullo stesso nemico −20 alla sua resistenza più bassa per 10 s. |
| **Colpo Stordente** (*Stunning Strike*) | 20 | Congela il bersaglio per circa 3–5 s. |
| **Colpo di Terra** (*Earth Strike*) | 20 | 15–35 (+abilità/20) danni fisici che ignorano le resistenze; −10 res. fisica (fino a 3 volte, 10 s). Ogni 3 usi un'onda d'urto a cono (7 caselle) che respinge e indebolisce. |
| **Colpo di Fuoco** (*Fire Strike*) | 20 | 15–35 (+abilità/20) danni da fuoco che ignorano le resistenze; −10 res. fuoco (fino a 3 volte, 20 s); in più un'ustione pari a 1,5 volte il colpo in 10 s. È la più forte delle mosse elementali. |
| **Colpo di Gelo** (*Freeze Strike*) | 20 | 15–35 (+abilità/20) danni da freddo; ogni carica (max 3): −10 res. freddo, Destrezza ridotta, rigenerazione rallentata; a 3 cariche il bersaglio cammina. |
| **Colpo Tossico** (*Toxic Strike*) | 20 | 15–35 (+abilità/20) danni da veleno; −10 res. veleno (max 3 volte); ogni 3 usi un'esplosione velenosa in 8 direzioni. |
| **Colpo di Fulmine** (*Lightning Strike*) | 20 | 15–35 (+abilità/20) danni da energia; 25% di paralizzare 3 s; tre usi in 6 s causano un sovraccarico che colpisce tutti entro 5 caselle. |
| **Colpo Elementale** (*Elemental Strike*) | 20 | (15–35 + abilità/20) ×1,5 danni di un elemento casuale, ognuno con un effetto extra (ustione, −70% Destrezza, veleno, paralisi 1 s). |
| **Prosciuga Forza / Destrezza / Intelligenza** (*Zap Str/Dex/Int*) | 25 | −40/70 (+2×abilità/20) alla caratteristica per 10–16 s. |
| **Prosciuga Vigore** (*Zap Stamina*) | 25 | Toglie fino a metà del Vigore (40–70 + abilità/20). |
| **Prosciuga Mana** (*Zap Mana*) | 25 | Toglie fino a metà del Mana (40–70 + abilità/20). |
| **Protezione Magica / II** (*Magic Protection*) | 25 / 30 | +6 / +8 alla riserva di assorbimento dei danni magici (max 50). |
| **Protezione da Mischia / II** (*Melee Protection*) | 25 / 30 | Dovrebbe assorbire danni fisici (+15 / +30), ma nel codice attuale **non ha effetto** (vedi riquadro). |
| **Carica a Cavallo** (*Riding Attack*) | 20 | Solo a cavallo. Danno basato su Cavalleria, aumentato da ogni passo di corsa fatto dopo averla armata (fino a 10). Un bersaglio a cavallo viene disarcionato, uno a piedi paralizzato 4 s. |
| **Colpo Infettivo dell'Ombra** (*Shadow Infectious Strike*) | 25 | Come Colpo Infettivo e, con 80 in Furtività, ti nasconde. |
| **Doppio Turbine** (*Double Whirlwind*) | 25 | Richiede 50 di Vigore. Due colpi a ogni nemico adiacente più uno extra al bersaglio (bonus basato su Bushido). |
| **Pugni di Furia** (*Fists of Fury*) | 20 | 1 o 2 colpi extra (in base a Lotta), ciascuno a −10%. |
| **Attacco Rotante** (*Spin Attack*) | 20 | 1 o 2 colpi extra (in base all'abilità d'arma), ciascuno a −10%. |
| **Colpo Devastante** (*Devastating Blow*) | 30 | Danno ×2. |
| **Colpo Letale** (*Death Blow*) | 50 | Aggiunge un grosso danno extra che cresce con il materiale e il danno massimo dell'arma; per 10 s il tuo danno sale del 20%. |

> **Attenzione, differenze fra libro e codice:** il libro delle abilità riporta alcuni costi diversi (Doppio Colpo 30 invece di 20, Forza della Natura 40 invece di 30, Turbine Frenetico 20 invece di 30). Inoltre Protezione da Mischia non funziona, Fendente in Corsa non disarciona e Doppio Tiro non richiede di essere a cavallo, contrariamente a quanto scritto nel libro. La tabella sopra segue il codice.

> **Consiglio:** per un principiante le mosse migliori sono quelle semplici e sempre utili: **Colpo Paralizzante** (blocca il nemico mentre ti curi o scappi), **Colpo Mortale** (impedisce ai mostri di curarsi), **Ignora Armatura** (contro corazze pesanti) e **Dissanguamento**. Le mosse elementali di 4° e 5° livello diventano devastanti a 100–110 di abilità.

---

## 10.9 Tabelle delle armi

Legenda delle colonne:

- **Abilità**: abilità d'arma usata per colpire.
- **Danno**: danno base minimo–massimo, prima dei bonus (si aggiunge il bonus del materiale).
- **Vel. (s)**: velocità base in secondi (vedi 10.4: più è bassa, più l'arma è veloce).
- **For**: Forza richiesta.
- **Mani**: 1 o 2 mani. I guanti da pugile e da lancio, pur avendo l'aspetto di guanti, occupano lo slot dell'arma a una mano.
- **Durab.**: punti di durabilità iniziali (minimo–massimo).
- **Gittata**: distanza massima in caselle (solo armi a distanza).
- **Mosse speciali**: nell'ordine in cui si sbloccano a 70, 80, 90, 100 e 110 di abilità d'arma e Tattica.

### Spade e lame da taglio

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Spada lunga (longsword) | Scherma con Spada | 15–16 | 3,50 | 35 | 1 | 31–110 | Ignora Armatura / Colpo Concussivo / Colpo Devastante / Colpo Tossico / Doppio Turbine |
| Spada lunga sottile (long sword) | Scherma con Spada | 15–16 | 3,50 | 35 | 1 | 31–110 | Dissanguamento / Disarmare / Attacco a Turbine / Colpo Infettivo / Colpo Letale |
| Spadone (broadsword) | Scherma con Spada | 14–15 | 3,25 | 30 | 1 | 31–100 | Colpo Schiacciante / Ignora Armatura / Forza della Natura / Colpo Infettivo dell'Ombra / Colpo Letale |
| Spada barbarica (barbarian sword) | Scherma con Spada | 15–17 | 3,75 | 40 | 1 | 31–100 | Colpo Schiacciante / Colpo Paralizzante / Fendente in Corsa / Colpo Letale / Protezione da Mischia II |
| Spada reale (royal sword) | Scherma con Spada | 15–17 | 3,75 | 40 | 1 | 31–100 | Colpo Schiacciante / Colpo Paralizzante / Fendente in Corsa / Colpo Letale / Protezione da Mischia II |
| Katana (katana) | Scherma con Spada | 11–13 | 2,50 | 25 | 1 | 31–90 | Doppio Turbine / Ignora Armatura / Doppio Colpo / Colpo di Fuoco / Colpo Letale |
| Scimitarra (scimitar) | Scherma con Spada | 13–15 | 3,00 | 25 | 1 | 31–90 | Doppio Colpo / Colpo Paralizzante / Protezione Magica II / Colpo Infettivo dell'Ombra / Prosciuga Vigore |
| Sciabola (cutlass) | Scherma con Spada | 11–13 | 2,50 | 25 | 1 | 31–70 | Dissanguamento / Colpo dell'Ombra / Colpo d'Artiglio / Fendente in Corsa / Colpo Tossico |
| Falcione (falchion) | Scherma con Spada | 12–14 | 2,50 | 20 | 1 | 30–60 | Attacco a Turbine / Danza delle Lame / Maestria Difensiva / Colpo Consacrato / Colpo di Fuoco |
| Machete (machete) | Scherma con Spada | 13–15 | 2,75 | 20 | 1 | 30–60 | Maestria Difensiva / Danza delle Lame / Colpo Elementale / Turbine Frenetico / Colpo Infettivo dell'Ombra |
| Spada dell'assassino (assassin sword) | Scherma con Spada | 12–14 | 2,50 | 35 | 1 | 30–60 | Attacco Psichico / Dissanguamento / Protezione Magica II / Colpo di Fuoco / Protezione da Mischia |
| Lama a mezzaluna (crescent blade) | Scherma con Spada | 11–14 | 2,50 | 55 | 2 | 51–80 | Doppio Colpo / Colpo Mortale / Colpo di Fulmine / Colpo di Gelo / Protezione Magica |
| Lame da guerra (war blades) | Scherma con Spada | 15–17 | 3,00 | 30 | 2 | 30–60 | Disarmare / Danza delle Lame / Colpo d'Artiglio / Forza della Natura / Colpo Tossico |
| Falcetto (sickle) | Scherma con Spada | 13–15 | 3,00 | 25 | 1 | 31–70 | Colpo Paralizzante / Colpo Mortale / Svestire / Prosciuga Mana / Blocco |

### Asce

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Ascia (axe) | Scherma con Spada | 14–16 | 3,00 | 35 | 2 | 31–110 | Colpo Schiacciante / Disarcionare / Colpo d'Artiglio / Colpo Tossico / Turbine Frenetico |
| Ascia da battaglia (battle axe) | Scherma con Spada | 15–17 | 3,50 | 35 | 2 | 31–70 | Dissanguamento / Colpo Concussivo / Carica a Cavallo / Attacco a Turbine / Colpo Elementale |
| Ascia doppia (double axe) | Scherma con Spada | 15–17 | 3,25 | 45 | 2 | 31–110 | Doppio Colpo / Attacco a Turbine / Carica a Cavallo / Colpo Consacrato / Colpo Stordente |
| Ascia del boia (executioner's axe) | Scherma con Spada | 15–17 | 3,25 | 40 | 2 | 31–70 | Dissanguamento / Colpo Mortale / Forza della Natura / Colpo di Terra / Colpo Letale |
| Grande ascia da battaglia (large battle axe) | Scherma con Spada | 16–17 | 3,75 | 80 | 2 | 31–70 | Attacco a Turbine / Dissanguamento / Carica a Cavallo / Colpo di Gelo / Fendente in Corsa |
| Ascia a due mani (two handed axe) | Scherma con Spada | 16–17 | 3,50 | 40 | 2 | 31–90 | Doppio Colpo / Colpo dell'Ombra / Colpo Tossico / Prosciuga Intelligenza / Maestria Difensiva |
| Ascia barbarica (barbarian axe) | Scherma con Spada | 16–17 | 3,75 | 80 | 1 | 31–70 | Disarmare / Colpo Schiacciante / Colpo Infettivo dell'Ombra / Colpo d'Artiglio / Colpo di Fulmine |
| Ascia da guerra (war axe) | Combattere con Mazza | 14–15 | 3,25 | 35 | 1 | 31–80 | Ignora Armatura / Dissanguamento / Maestria Difensiva / Colpo Stordente / Protezione da Mischia II |
| Accetta (hatchet) | Scherma con Spada | 13–15 | 2,75 | 20 | 2 | 31–80 | Ignora Armatura / Disarmare / Protezione Magica II / Turbine Frenetico / Protezione da Mischia II |
| Piccone (pickaxe) | Scherma con Spada | 13–15 | 3,00 | 50 | 1 | 31–60 | Doppio Colpo / Disarmare / Protezione Magica II / Prosciuga Destrezza / Colpo dell'Ombra |

### Pugnali e coltelli

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Pugnale (dagger) | Lame Leggere | 10–11 | 2,00 | 10 | 1 | 31–40 | Colpo Infettivo / Colpo dell'Ombra / Disarmare / Doppio Turbine / Colpo Tossico |
| Pugnale dell'assassino (assassin dagger) | Lame Leggere | 10–12 | 2,00 | 15 | 1 | 30–60 | Colpo Infettivo / Colpo dell'Ombra / Colpo di Fulmine / Protezione da Mischia / Colpo di Fuoco |
| Pugnale da guerra (war dagger) | Scherma con Spada | 13–15 | 2,75 | 20 | 1 | 30–60 | Finta / Ignora Armatura / Prosciuga Intelligenza / Attacco Psichico / Prosciuga Vigore |
| Mannaia da guerra (war cleaver) | Lame Leggere | 9–11 | 2,25 | 15 | 1 | 30–60 | Disarmare / Danza delle Lame / Prosciuga Forza / Prosciuga Destrezza / Blocco |
| Mannaia (cleaver) | Scherma con Spada | 11–13 | 2,50 | 10 | 1 | 31–50 | Dissanguamento / Colpo Infettivo / Protezione da Mischia II / Prosciuga Destrezza / Prosciuga Vigore |
| Coltello da macellaio (butcher knife) | Scherma con Spada | 9–11 | 2,25 | 5 | 1 | 31–40 | Colpo Infettivo / Disarmare / Colpo Consacrato / Doppio Turbine / Protezione Magica II |
| Coltello da scuoiatore (skinning knife) | Scherma con Spada | 9–11 | 2,25 | 5 | 1 | 31–40 | Colpo dell'Ombra / Disarmare / Svestire / Prosciuga Forza / Forza della Natura |

### Lame leggere: lance, forconi, kryss

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Kryss (kryss) | Lame Leggere | 10–12 | 2,00 | 10 | 1 | 31–90 | Ignora Armatura / Colpo Infettivo / Prosciuga Destrezza / Colpo di Fulmine / Colpo Tossico |
| Lancia (spear) | Lame Leggere | 13–15 | 2,75 | 50 | 2 | 31–80 | Ignora Armatura / Colpo Paralizzante / Colpo d'Achille / Colpo Infettivo dell'Ombra / Doppio Turbine |
| Lancia corta (short spear) | Lame Leggere | 10–13 | 2,00 | 40 | 2 | 31–70 | Colpo dell'Ombra / Colpo Mortale / Colpo d'Achille / Colpo di Terra / Turbine Frenetico |
| Lancia tribale (tribal spear) | Lame Leggere | 13–15 | 2,75 | 50 | 2 | 31–80 | Ignora Armatura / Colpo Paralizzante / Prosciuga Vigore / Colpo d'Achille / Colpo ai Nervi |
| Picca (pike) | Lame Leggere | 14–16 | 3,00 | 50 | 2 | 31–110 | Colpo Paralizzante / Colpo Infettivo / Protezione Magica / Colpo Stordente / Forza della Natura |
| Tridente (trident) | Lame Leggere | 13–14 | 2,50 | 55 | 2 | 31–60 | Dissanguamento / Disarcionare / Colpo dell'Ombra / Colpo d'Achille / Colpo Stordente |
| Forcone da guerra (war fork) | Lame Leggere | 12–13 | 2,50 | 45 | 1 | 31–110 | Dissanguamento / Disarmare / Forza della Natura / Protezione Magica II / Colpo Devastante |
| Bastone a doppia lama (double bladed staff) | Lame Leggere | 12–13 | 2,25 | 50 | 2 | 31–80 | Doppio Colpo / Colpo Infettivo / Prosciuga Forza / Colpo di Gelo / Fendente in Corsa |
| Bastone con lama (bladed staff) | Scherma con Spada | 14–16 | 3,00 | 40 | 2 | 21–110 | Ignora Armatura / Disarcionare / Colpo di Terra / Protezione Magica / Disarmare |
| Lancia da cavaliere (lance) | Lame Leggere | 17–18 | 4,50 | 95 | 1 | 31–110 | Disarcionare / Colpo Concussivo / Carica a Cavallo / Colpo Devastante / Colpo Letale |

### Armi d'asta

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Bardiche (bardiche) | Scherma con Spada | 17–18 | 3,75 | 45 | 2 | 31–100 | Colpo Paralizzante / Disarcionare / Attacco Psichico / Colpo Devastante / Doppio Colpo |
| Alabarda (halberd) | Scherma con Spada | 18–19 | 4,25 | 95 | 2 | 31–80 | Attacco a Turbine / Colpo Concussivo / Prosciuga Forza / Colpo di Gelo / Fendente in Corsa |
| Falce (scythe) | Scherma con Spada | 15–18 | 3,50 | 45 | 2 | 31–100 | Dissanguamento / Colpo Paralizzante / Colpo ai Nervi / Colpo Elementale / Colpo di Fulmine |

### Mazze e martelli

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Clava (club) | Combattere con Mazza | 11–13 | 2,50 | 40 | 1 | 31–40 | Colpo dell'Ombra / Disarcionare / Colpo Stordente / Attacco Rotante / Colpo Devastante |
| Mazza (mace) | Combattere con Mazza | 12–14 | 2,75 | 45 | 1 | 31–70 | Colpo Concussivo / Disarmare / Protezione da Mischia / Colpo Schiacciante / Colpo ai Nervi |
| Maglio (maul) | Combattere con Mazza | 14–16 | 3,50 | 45 | 1 | 31–70 | Colpo Schiacciante / Colpo Concussivo / Colpo Letale / Prosciuga Forza / Prosciuga Mana |
| Mazza da guerra (war mace) | Combattere con Mazza | 16–17 | 4,00 | 80 | 1 | 31–110 | Colpo Schiacciante / Dissanguamento / Colpo Letale / Colpo Elementale / Attacco Rotante |
| Martello da guerra (war hammer) | Combattere con Mazza | 17–18 | 3,75 | 95 | 2 | 31–110 | Attacco a Turbine / Colpo Schiacciante / Prosciuga Intelligenza / Prosciuga Mana / Carica a Cavallo |
| Martello-piccone (hammer pick) | Combattere con Mazza | 15–17 | 3,75 | 45 | 1 | 31–70 | Ignora Armatura / Colpo Mortale / Attacco Rotante / Colpo Elementale / Colpo di Terra |
| Scettro (scepter) | Combattere con Mazza | 14–17 | 3,50 | 40 | 1 | 31–110 | Colpo Schiacciante / Colpo Mortale / Carica a Cavallo / Attacco Rotante / Colpo di Gelo |
| Mazza da battaglia (battle mace) | Combattere con Mazza | 14–17 | 3,00 | 35 | 1 | 30–60 | Colpo Concussivo / Colpo Schiacciante / Attacco Psichico / Blocco / Doppio Turbine |
| Bacchetta (wand) | Combattere con Mazza | 9–11 | 2,75 | 5 | 1 | 31–110 | Disarcionare / Disarmare / – / – / – |
| Bacchetta del mago (wizard wand) | Combattere con Mazza | 10–12 | 2,50 | 20 | 1 | 31–40 | Attacco Psichico / Prosciuga Mana / Colpo Elementale / Protezione Magica / Protezione Magica II |

### Bastoni

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Bastone ferrato (quarter staff) | Combattere con Mazza | 11–14 | 2,25 | 30 | 2 | 31–60 | Doppio Colpo / Colpo Concussivo / Protezione da Mischia / Protezione Magica / Colpo Letale |
| Bastone nodoso (gnarled staff) | Combattere con Mazza | 15–17 | 3,25 | 20 | 2 | 31–50 | Colpo Concussivo / Colpo Paralizzante / Colpo Consacrato / Colpo ai Nervi / Finta |
| Bastone nero (black staff) | Combattere con Mazza | 13–16 | 2,75 | 35 | 2 | 31–70 | Attacco a Turbine / Colpo Paralizzante / Fendente in Corsa / Prosciuga Forza / Attacco Rotante |
| Bastone da pastore (shepherd's crook) | Combattere con Mazza | 13–15 | 2,75 | 20 | 2 | 31–50 | Colpo Schiacciante / Disarmare / Colpo Devastante / Attacco Psichico / Attacco Rotante |
| Bastone druidico (druid staff) | Combattere con Mazza | 10–12 | 2,25 | 15 | 1 | 30–60 | Blocco / Forza della Natura / Protezione Magica / Colpo di Gelo / Attacco Psichico |

### Armi orientali

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Bokuto (bokuto) | Scherma con Spada | 9–11 | 2,00 | 20 | 1 | 25–50 | Finta / Colpo ai Nervi / Danza delle Lame / Maestria Difensiva / Colpo di Gelo |
| Daisho (daisho) | Scherma con Spada | 13–15 | 2,75 | 40 | 2 | 45–65 | Finta / Doppio Colpo / Prosciuga Forza / Colpo ai Nervi / Protezione da Mischia II |
| No-dachi (no-dachi) | Scherma con Spada | 16–18 | 3,50 | 40 | 2 | 31–90 | Colpo Schiacciante / Fendente in Corsa / Colpo di Fuoco / Danza delle Lame / Colpo Consacrato |
| Wakizashi (wakizashi) | Scherma con Spada | 11–13 | 2,50 | 20 | 1 | 45–50 | Turbine Frenetico / Doppio Colpo / Colpo di Terra / Colpo di Fuoco / Prosciuga Vigore |
| Kama (kama) | Lame Leggere | 9–11 | 2,00 | 15 | 2 | 35–60 | Attacco a Turbine / Maestria Difensiva / Prosciuga Intelligenza / Carica a Cavallo / Colpo d'Artiglio |
| Lajatang (lajatang) | Lame Leggere | 16–18 | 3,50 | 65 | 2 | 90–95 | Maestria Difensiva / Turbine Frenetico / Colpo Devastante / Forza della Natura / Prosciuga Mana |
| Sai (sai) | Lame Leggere | 9–11 | 2,00 | 15 | 2 | 55–60 | Blocco / Perforazione / Doppio Turbine / Colpo di Terra / Protezione da Mischia II |
| Tekagi (tekagi) | Lame Leggere | 10–12 | 2,00 | 10 | 2 | 35–60 | Doppia Impugnatura / Colpo d'Artiglio / Colpo di Gelo / Colpo Devastante / Colpo Consacrato |
| Tessen (tessen) | Combattere con Mazza | 10–12 | 2,00 | 10 | 2 | 55–60 | Finta / Blocco / Protezione Magica II / Protezione da Mischia / Colpo di Fulmine |
| Tetsubo (tetsubo) | Combattere con Mazza | 12–14 | 2,50 | 35 | 2 | 60–65 | Turbine Frenetico / Colpo Schiacciante / Prosciuga Mana / Carica a Cavallo / Colpo Elementale |
| Nunchaku (nunchaku) | Combattere con Mazza | 11–13 | 2,50 | 15 | 2 | 40–55 | Colpo Schiacciante / Fendente in Corsa / Colpo di Fuoco / Danza delle Lame / Colpo Consacrato |

### Lotta

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Guanti da pugile (pugilist gloves) | Lotta | 8–10 | 2,00 | 20 | 1 | 31–50 | Disarmare / Colpo Paralizzante / Attacco a Turbine / Pugni di Furia / Colpo Letale |

### Attrezzi da lavoro usabili come armi

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Piccone dei gargoyle (gargoyle's pickaxe) | Scherma con Spada | 13–15 | 3,00 | 50 | 1 | 31–60 | Doppio Colpo / Disarmare / Protezione Magica / Colpo Stordente / Protezione da Mischia |
| Piccone robusto (sturdy pickaxe) | Scherma con Spada | 13–15 | 3,00 | 50 | 1 | – | Doppio Colpo / Disarmare / Protezione Magica / Prosciuga Destrezza / Colpo di Fuoco |
| Piccone di adamantio (adamantium pickaxe) | Scherma con Spada | 13–15 | 3,00 | 50 | 1 | 100–100 | Doppio Colpo / Disarmare / Protezione Magica II / Prosciuga Destrezza / Colpo dell'Ombra |
| Attrezzo del cercatore (prospector's tool) | Combattere con Mazza | 13–15 | – | 40 | 1 | – | Colpo Schiacciante / Colpo dell'Ombra / Prosciuga Intelligenza / Prosciuga Forza / Turbine Frenetico |
| Ascia da taglialegna (lumber axe) | Scherma con Spada | 13–15 | 2,75 | 20 | 2 | 31–80 | Ignora Armatura / Disarmare / Protezione Magica II / Turbine Frenetico / Protezione da Mischia II |

### Armi esotiche (Underworld, alieni)

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|
| Spada laser (laser sword) | Scherma con Spada | 20–24 | 3,00 | 35 | 1 | 50–110 | Ignora Armatura / Doppio Colpo / Maestria Difensiva / Colpo Elementale / Colpo Letale |
| Spada laser doppia (double laser sword) | Scherma con Spada | 24–28 | 3,25 | 50 | 2 | 50–110 | Ignora Armatura / Doppio Colpo / Maestria Difensiva / Colpo Elementale / Colpo Letale |
| Balestra kilrathi (Kilrathi bowcaster) | Tiro con l'Arco | 18–22 | 4,50 | 35 | 1 | 31–80 | Doppio Colpo / Tiro in Movimento / Colpo Stordente / Prosciuga Mana / Prosciuga Destrezza |
| Balestra kilrathi pesante (Kilrathi heavy bowcaster) | Tiro con l'Arco | 20–24 | 5,00 | 80 | 2 | 31–100 | Colpo Paralizzante / Colpo Mortale / Doppio Tiro / Tiro in Movimento / Prosciuga Intelligenza |

### Archi e balestre

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Gittata | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|---|
| Arco (bow) | Tiro con l'Arco | 15–19 | 4,25 | 30 | 2 | 10 | 31–60 | Colpo Paralizzante / Colpo Mortale / Doppio Tiro / Tiro in Movimento / Prosciuga Intelligenza |
| Arco composito (composite bow) | Tiro con l'Arco | 13–17 | 4,00 | 45 | 2 | 10 | 31–70 | Ignora Armatura / Tiro in Movimento / Doppio Tiro / Prosciuga Destrezza / Prosciuga Mana |
| Arco corto silvano (woodland shortbow) | Tiro con l'Arco | 9–13 | 3,00 | 45 | 2 | 10 | 41–90 | Freccia Fulminante / Attacco Psichico / Tiro in Movimento / Doppio Tiro / Colpo Infettivo |
| Arco lungo silvano (woodland longbow) | Tiro con l'Arco | 12–16 | 4,00 | 45 | 2 | 10 | 41–90 | Freccia di Forza / Freccia del Serpente / Prosciuga Intelligenza / Doppio Tiro / Prosciuga Vigore |
| Yumi (yumi) | Tiro con l'Arco | 16–20 | 4,50 | 35 | 2 | 10 | 55–60 | Perforazione / Doppio Tiro / Prosciuga Forza / Tiro in Movimento / Doppio Turbine |
| Balestra (crossbow) | Tiro con l'Arco | 18–22 | 4,50 | 35 | 2 | 8 | 31–80 | Colpo Concussivo / Colpo Mortale / Tiro in Movimento / Doppio Tiro / Colpo Infettivo |
| Balestra pesante (heavy crossbow) | Tiro con l'Arco | 20–24 | 5,00 | 80 | 2 | 8 | 31–100 | Tiro in Movimento / Disarcionare / Prosciuga Vigore / Doppio Tiro / Blocco |
| Balestra a ripetizione (repeating crossbow) | Tiro con l'Arco | 8–12 | 2,75 | 30 | 2 | 7 | 31–80 | Doppio Colpo / Tiro in Movimento / Colpo Stordente / Prosciuga Mana / Prosciuga Destrezza |

### Armi da lancio, bastoni magici e arpione

| Arma | Abilità | Danno | Vel. (s) | For | Mani | Gittata | Durab. | Mosse speciali (70 / 80 / 90 / 100 / 110) |
|---|---|---|---|---|---|---|---|---|
| Guanti da lancio (throwing gloves) | Tiro con l'Arco | 10–14 | 4,00 | 20 | 1 | 8 | 31–60 | Colpo Concussivo / Colpo Stordente / Colpo Schiacciante / Colpo Letale / Colpo ai Nervi |
| Bastone del mago (stave) | Tiro con l'Arco | 15–19 | 5,00 | 20 | 2 | 10 | 50–90 | Prosciuga (variabile, vedi note) / Protezione Magica / Colpo Elementale / Ignora Armatura / Protezione Magica II |
| Scettro del mago (sceptre) | Tiro con l'Arco | 11–15 | 5,00 | 15 | 1 | 8 | 50–90 | Prosciuga (variabile, vedi note) / Protezione Magica / Colpo Elementale / Ignora Armatura / Protezione Magica II |
| Arpione (harpoon) | Tiro con l'Arco | 15–19 | 5,00 | 20 | 2 | 10 | 50–90 | Colpo Paralizzante / Colpo Mortale / Tiro in Movimento / Doppio Tiro / Colpo Infettivo |
### Note sulle armi particolari

- **Guanti da lancio** (*throwing gloves*): arma a distanza (Tiro con l'Arco, gittata 8) che ha l'aspetto di un paio di guanti ma occupa la mano dell'arma, con **Spell Channeling** e solo 20 di Forza richiesta. Consumano **armi da lancio** (*throwing weapon*), che trovi da meccanici, fornitori e monaci. Le mosse cambiano secondo il tipo di guanto:

  | Tipo | Mosse (70 / 80 / 90 / 100 / 110) |
  |---|---|
  | Pietre (*Stones*, predefinito) | Colpo Concussivo / Colpo Stordente / Colpo Schiacciante / Colpo Letale / Colpo ai Nervi |
  | Asce (*Axes*) | Ignora Armatura / Colpo d'Artiglio / Dissanguamento / Colpo Consacrato / Doppio Colpo |
  | Pugnali (*Daggers*) | Ignora Armatura / Colpo d'Artiglio / Colpo Infettivo / Colpo Devastante / Colpo Letale |
  | Dardi (*Darts*) | Colpo Paralizzante / Ignora Armatura / Colpo Infettivo / Colpo Tossico / Colpo di Fulmine |
  | Carte (*Cards*) | Ignora Armatura / Colpo d'Artiglio / Colpo Infettivo / Colpo Devastante / Colpo Letale |
  | Pomodori (*Tomatoes*) | come le pietre |
  | Altro | Colpo dell'Ombra / Colpo Paralizzante / Colpo Infettivo / Colpo Infettivo dell'Ombra / Colpo Devastante |

- **Bastone e scettro del mago** (*stave*, *sceptre*): armi a distanza che sparano dardi magici e usano Tiro con l'Arco. Per impugnarli ti servono almeno **30 in Magia o Negromanzia**. Le munizioni sono gli **occhi del mago** (*mage eye*): fai doppio clic sul bastone e scegli delle gemme nello zaino per trasformarle (ogni gemma rende: diamante 25 occhi, zaffiro stellato 15, smeraldo, zaffiro o ametista 12, tormalina 11, rubino 9, citrino o ambra 6). Il bonus di danno viene da Magia e Negromanzia invece che dalla Tattica soltanto. La prima mossa dipende dall'elemento prevalente del danno: Prosciuga Vigore (fuoco), Prosciuga Destrezza (freddo), Prosciuga Intelligenza (energia), Prosciuga Forza (veleno), altrimenti Prosciuga Mana.
- **Arpione** (*harpoon*): arma a distanza da pescatore, usa le **corde da arpione** (*harpoon rope*) come munizioni e riceve il bonus della Pesca.
- **Archi** usano **frecce** (*arrow*), **balestre** usano **dardi** (*bolt*). Una faretra (*quiver*) con *Lower Ammo Cost* fa risparmiare munizioni.
- **Lancia da cavaliere** (*lance*): usa Lame Leggere ed è l'unica arma che permette Disarcionare restando a cavallo.
- **Lilarcor** è una spada senziente legata a un evento (ha solo due mosse, Attacco a Turbine e Colpo Infettivo).
- **Armi "Gift" e "Level"**: quasi ogni arma esiste anche nelle versioni *dono* (ricompense delle missioni di tributo, personalizzabili) e *leggendaria/livellabile* (salgono di livello uccidendo mostri). Hanno le stesse statistiche base della versione normale; vedi il capitolo 12.
- Nel codice esistono anche armi gargoyle dell'espansione *Stygian Abyss* (Dread Sword, Soul Glaive, Glass Staff...) ma **non compaiono nel gioco**: non sono nel bottino né nell'artigianato.

---

## 10.10 Bacchette magiche (wands)

Le **bacchette** (*wand of ...*) sono armi contundenti leggere (7–9 danni, velocità 2,00) che contengono **cariche** di un incantesimo. Si usano così: equipaggiale, fai **doppio clic** e scegli il bersaglio. Fra un uso e l'altro devi aspettare **4 secondi**. Danno anche un bonus alla Magia mentre le impugni.

| Cerchio | Cariche (casuali, da 1 a) | Intelligenza richiesta | Bonus a Magia |
|---|---|---|---|
| 1° | 25 | 10 | +10 |
| 2° | 20 | 15 | +20 |
| 3° | 15 | 20 | +30 |
| 4° | 11 | 25 | +40 |
| 5° | 9 | 30 | +50 |
| 6° | 7 | 35 | +60 |
| 7° | 5 | 40 | +70 |
| 8° | 3 | 45 | +80 |

Esistono bacchette per tutti i 64 incantesimi di Magia (per esempio *wand of healing* = Cura, *wand of fireballs* = Palla di Fuoco, *wand of recalling* = Richiamo, *wand of resurrecting* = Resurrezione). Le bacchette trovate nei dungeon sono spesso **non identificate** (*unknown wand*): usarle senza sapere cosa sono può colpire te stesso. Non puoi usare una bacchetta mentre impugni un'altra arma o i guanti da pugile.

> **Consiglio:** una *wand of healing* o *greater healing* è un ottimo aiuto per un guerriero senza Magia; una *wand of recalling* o *gate travels* ti riporta in città se hai dimenticato le pergamene.

---

## 10.11 Consigli per scegliere l'arma

| Stile | Arma consigliata all'inizio | Perché |
|---|---|---|
| Guerriero a due mani | Ascia doppia, ascia del boia, bardiche, bastone nero | Molto danno, mosse forti già a 70–80; la guida originale suggerisce un'ascia doppia eccezionale |
| Guerriero con scudo | Spada lunga, scimitarra, mazza da battaglia | Lasciano libera la mano dello scudo; buone mosse iniziali (Ignora Armatura, Doppio Colpo) |
| Schermidore veloce | Kryss, pugnale, sai, tekagi | Velocità 2,00 (colpo ogni 1,25 s con Vigore alto); ottimi per veleni e mosse frequenti |
| Arciere | Arco composito, balestra, yumi | Tieni i mostri a distanza; ricorda le munizioni |
| Lottatore o mago guerriero | Guanti da pugile o guanti da lancio | Spell Channeling: puoi lanciare incantesimi senza toglierli |
| Taglialegna o minatore | Asce (Taglialegna +10%), mazze (Estrazione +10%) | Il mestiere aumenta il danno |

> **Consiglio:** cerca presto un'arma con **Spell Channeling**. La guida originale insiste: anche se è di un tipo diverso dal tuo, conviene cambiare arma e imparare la nuova abilità, perché poter lanciare incantesimi (cure, Palla di Fuoco, Richiamo) senza disarmarti cambia completamente il modo di combattere.

> **Consiglio:** le armi con la proprietà **Aumento danno** (*Damage Increase*) si vendono molto meglio ai mercanti di qualunque altra arma: se non ti servono, raccoglile per venderle.
