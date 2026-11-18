# 9. Alchimia, pozioni e altri consumabili magici

Le pozioni sono la "magia di chi non è mago": curano, rinforzano, esplodono e avvelenano senza bisogno di Magia. In Ultima Adventures l'Alchimia produce anche **elisir** che aumentano le abilità, **misture** che evocano melme, **liquidi** che allagano il terreno e **trasmutazioni** di metalli, cuoio e legno. Esistono poi due alchimie "parallele": l'**alchimia necrotica** dei negromanti e l'**erboristeria druidica**.

---

## 9.1 Come si fabbricano le pozioni

| Cosa serve | Note |
|---|---|
| Abilità **Alchimia** | Determina la riuscita; ogni ricetta ha un intervallo da 0% a 100% |
| **Mortaio e pestello** (*mortar and pestle*) | Lo strumento dell'alchimista; si consuma con l'uso |
| **Bottiglie vuote** | Una per pozione; per misture e liquidi servono anche **barattoli** (*jar*) |
| Reagenti | Vedi tabelle |

Usa il mortaio, scegli la categoria (**Potions**, **Elixirs**, **Mixtures**, **Transmutations**, **Hair**) e la ricetta. Se fallisci perdi i reagenti ma **riavi la bottiglia** (e il barattolo).

Le pozioni dello stesso tipo si possono versare in un **barile per pozioni** (*potion keg*) per conservarne molte con poco peso.

### Potenziamento delle pozioni

Gli effetti delle pozioni (cura, danno, durata, statistiche) aumentano con il **Potenziamento Pozioni** (*Enhance Potions*, EP):

- dagli **oggetti**: tetto 50% (100% per la classe **Alchimista**);
- dall'**abilità Alchimia**: +10%, +20% o +30%. Nelle intenzioni dovrebbe servire Alchimia 33/66/99, ma il codice confronta il valore interno (decimi di punto): in pratica bastano **3,3 / 6,6 / 9,9 di Alchimia** per ottenere il +30%.

La classe **Alchimista** ha anche: bonus extra sulle pozioni, possibilità di non consumare la pozione bevuta (con il Costo Reagenti Ridotto, massimo 50%), pozioni esplosive che detonano all'impatto senza timer. In cambio **non può lanciare incantesimi**.

> **Consiglio:** bere una pozione richiede **una mano libera**. Fanno eccezione armi bilanciate, guanti da pugile e guanti da lancio. Le pozioni da lancio (esplosione, conflagrazione, confusione, gelo) non richiedono la mano libera.

---

## 9.2 Pozioni standard

Abbreviazioni dei reagenti: vedi glossario; tutte richiedono anche una **bottiglia vuota**. "Abilità" = intervallo di Alchimia da 0% a 100% di riuscita.

### Cura, ristoro e statistiche

| Pozione | Reagente | Abilità | Effetto |
|---|---|---|---|
| Cura minore (Lesser Heal) | 1 Ginseng | -25 → 25 | Cura 6–8 PF (×1,25 per i giocatori, +EP). Attesa 3 s prima di un'altra pozione di cura |
| Cura (Heal) | 3 Ginseng | 15 → 65 | Cura 13–16 PF. Attesa 8 s |
| Cura superiore (Greater Heal) | 7 Ginseng | 55 → 105 | Cura 20–25 PF. Attesa 10 s |
| Purificazione minore (Lesser Cure) | 1 Aglio | -10 → 40 | Cura il veleno: minore 75%, normale 50%, superiore 25% |
| Purificazione (Cure) | 3 Aglio | 25 → 75 | Minore 100%, normale 95%, superiore 75%, mortale 50%, letale 25% |
| Purificazione superiore (Greater Cure) | 6 Aglio | 65 → 115 | Minore, normale e superiore 100%, mortale 95%, letale 75% |
| Ristoro (Refresh) | 1 Perla Nera | -25 → 25 | Ripristina il 25% del vigore massimo |
| Ristoro totale (Total Refresh) | 5 Perla Nera | 25 → 75 | Ripristina tutto il vigore |
| Agilità (Agility) | 1 Muschio Sanguigno | 15 → 65 | +10 Destrezza per 2 minuti (+EP) |
| Agilità superiore (Greater Agility) | 3 Muschio Sanguigno | 35 → 85 | +20 Destrezza per 2 minuti |
| Forza (Strength) | 2 Radice di Mandragora | 25 → 75 | +10 Forza per 2 minuti |
| Forza superiore (Greater Strength) | 5 Radice di Mandragora | 45 → 95 | +20 Forza per 2 minuti |
| Visione notturna (Night Sight) | 1 Seta di Ragno | -25 → 25 | Vedi al buio |

Le probabilità di Purificazione aumentano con il Potenziamento Pozioni.

### Pozioni da lancio

Fai doppio clic e scegli dove lanciarle.

| Pozione | Reagente | Abilità | Effetto |
|---|---|---|---|
| Esplosione minore (Lesser Explosion) | 3 Cenere Sulfurea | 5 → 55 | Esplode dopo circa 3,5 s: 30–40 danni da fuoco nel raggio di 2 caselle |
| Esplosione (Explosion) | 5 Cenere Sulfurea | 35 → 85 | 50–60 danni |
| Esplosione superiore (Greater Explosion) | 10 Cenere Sulfurea | 65 → 115 | 70–80 danni |
| Conflagrazione (Conflagration) | 5 Polvere di Tomba | 55 → 105 | Crea a terra un'area di fuoco: 10–20 danni a chi la attraversa. Attesa circa 30 s fra un lancio e l'altro |
| Conflagrazione superiore (Greater Conflagration) | 10 Polvere di Tomba | 65 → 115 | 20–30 danni |
| Gelo (Frostbite) | 5 Cristallo Lunare | 55 → 105 | Area di gelo: 10–20 danni da freddo. Attesa circa 30 s |
| Gelo superiore (Greater Frostbite) | 10 Cristallo Lunare | 65 → 115 | 20–30 danni |
| Esplosione confusa (Confusion Blast) | 5 Ghisa | 55 → 105 | Nube che "calma" i mostri entro 5 caselle per 5 s. Attesa 20 s |
| Esplosione confusa superiore (Greater Confusion Blast) | 10 Ghisa | 65 → 115 | Raggio 7 caselle |

Il danno e il raggio delle esplosioni crescono con il Potenziamento Pozioni (raggio +1 ogni 50%).

> **Consiglio:** se hai una pila di pozioni esplosive, il doppio clic separa automaticamente una sola pozione: non rischi di far esplodere tutta la pila. Lanciale subito: il timer parte appena le "attivi".

### Bomba fumogena

| Oggetto | Ingredienti | Abilità | Effetto |
|---|---|---|---|
| Bomba fumogena (Smoke Bomb) | 1 Uovo, 3 Ginseng | 90 → 120 | Ti nasconde istantaneamente in una nube di fumo (strumento del ninja) |

---

## 9.3 Veleni

| Pozione | Belladonna | Abilità | Livello di veleno | Avvelenamento per usarla come "pozza" |
|---|---|---|---|---|
| Veleno minore (Lesser Poison) | 1 | -5 → 45 | Minore | 50 |
| Veleno (Poison) | 2 | 15 → 65 | Normale | 60 |
| Veleno superiore (Greater Poison) | 4 | 35 → 85 | Superiore | 70 |
| Veleno mortale (Deadly Poison) | 8 | 55 → 105 | Mortale | 80 |
| Veleno letale (Lethal Poison) | 12 | 75 → 125 | Letale | 90 |

Come si usano:

- **Applicarle alle armi:** usa l'abilità **Avvelenamento** e scegli la pozione e poi l'arma (lame e frecce). I colpi successivi possono avvelenare il bersaglio. Il livello applicabile dipende dalla tua abilità (tra 0 e 40 per il veleno minore, fino a 80–120 per quello letale).
- **Versarle a terra:** se la tua abilità di Avvelenamento raggiunge il valore indicato in tabella, il doppio clic ti permette di **gettare il veleno a terra** (fino a 8 caselle): si forma una pozza velenosa per chi la calpesta. Costa 40 di karma.
- **Berle:** se non hai abbastanza Avvelenamento, il doppio clic ti fa **bere** il veleno. Ti avveleni, ma hai una piccola probabilità di migliorare l'abilità Avvelenamento.

Fabbricare veleni dà una probabilità di allenare anche **Avvelenamento** (fino a 40, 75, 95, 115 o 125 secondo il livello).

> **Consiglio:** la sacca di veleno dei ragni e dei serpenti (*venom sack*) è un'altra fonte di veleno. E ricorda che la Magia combinata con l'Avvelenamento rende più forte anche l'incantesimo **Veleno**.

---

## 9.4 Pozioni rare (trovate o acquistate)

Queste pozioni non si fabbricano con il mortaio: si trovano nei dungeon, nei forzieri, presso mercanti particolari o come ricompense.

| Pozione | Effetto |
|---|---|
| Mana minore / Mana / Mana superiore (Lesser / Mana / Greater Mana) | Ripristina il **maggiore** fra 6–8 / 13–16 / 20–25 punti e il 15% / 22% / 30% del mana massimo. Attesa 3 / 8 / 10 s |
| Ringiovanimento minore / Ringiovanimento / superiore (Lesser / Rejuvenate / Greater Rejuvenate) | Ripristina 6–8 / 13–16 / 20–25 punti di PF, vigore **e** mana. Attesa 3 / 8 / 10 s |
| Invisibilità minore / Invisibilità / superiore | Invisibile per 1 / 2 / 3 minuti |
| Invulnerabilità (Invulnerability) | Sei immune a ogni danno per 1 minuto |
| Pozione superiore (Superior Potion) | +10 a Forza, Destrezza e Intelligenza per 5 minuti |
| Pozione della forza / destrezza / saggezza (Potion of Might / Dexterity / Wisdom) | **Permanente:** +1–5 a Forza / Destrezza / Intelligenza (più probabile un valore alto con il Potenziamento Pozioni), se non hai già raggiunto il tetto delle caratteristiche |
| Pozione della rinascita (Potion of Rebirth) | Se muori entro 30 s dal berla, risorgi |
| Pozione di resurrezione (Resurrection Potion) | 🎯 Resuscita un altro giocatore o un tuo seguace |
| Pozione di durabilità (Durability Potion) | Versata su un oggetto nello zaino, ne aumenta la durabilità |
| Pozione di riparazione (Repairing Potion) | Ripara completamente un oggetto, ma ne riduce di 1 la durabilità massima |
| Acqua santa (Holy Water) | Oggetto decorativo fisso (non si raccoglie né si beve) |
| Pozione del cambio di sesso (Gender Change) | Cambia il sesso del personaggio |
| Pozioni per capelli (Hair Dye, Hair Styling) | Cambiano colore o taglio dei capelli |
| Barattolo di polvere di teschio (Jar of Skull Dust) | Rende la pelle di un bianco spettrale (o la riporta normale) |
| Teschio malvagio (Evil Skull) | Si sbriciola e ripristina il mana |
| Calderone di pozione (Potion Cauldron) | Nei dungeon: calderoni da cui riempire bottiglie con una pozione casuale, per alcuni usi |
| Vaso canopo (Canopic Jar) | Reperto antico; con un po' di Alchimia si può esaminarne il contenuto |
| Grande Pozione Sacra dell'Umiltà (Holy Draught of Humility) | Purifica i peccati e ti avvia sul "sentiero della luna" |

---

## 9.5 Elisir delle abilità

Gli **elisir** aumentano temporaneamente un'abilità. Tutti richiedono **Alchimia 60 → 120**, una bottiglia vuota, 3 unità di un reagente principale, 1 di un reagente secondario e **una gemma**.

| Elisir | Reagenti | Gemma |
|---|---|---|
| Alchimia | 3 Cristallo Lunare, 1 Aglio | Zaffiro |
| Anatomia | 3 Bacche di Palude, 1 Ala di Pipistrello | Rubino |
| Conoscenza Animali | 3 Loto Rosso, 1 Ginseng | Zaffiro |
| Addomesticare Animali | 3 Guscio di Scarabeo, 1 Ghisa | Tormalina |
| Tiro con l'Arco | 3 Uovo di Fata, 1 Seta di Ragno | Ametista |
| Conoscenza Armi | 3 Ali di Farfalla, 1 Aglio | Smeraldo |
| Accattonaggio | 3 Uovo di Fata, 1 Ghisa | Tormalina |
| Fabbro | 3 Orecchio di Gargoyle, 1 Polvere di Tomba | Diamante |
| Accampamento | 3 Loto Rosso, 1 Sangue di Demone | Zaffiro |
| Carpenteria | 3 Loto Rosso, 1 Perla Nera | Ametista |
| Cartografia | 3 Sale Marino, 1 Sangue di Demone | Rubino |
| Cucina | 3 Sale Marino, 1 Ala di Pipistrello | Ambra |
| Rivelare Nascosti | 3 Teschio di Folletto, 1 Cristallo di Nox | Smeraldo |
| Discordia | 3 Orecchio di Gargoyle, 1 Perla Nera | Ambra |
| Valutare Intelligenza | 3 Ali di Farfalla, 1 Muschio Sanguigno | Zaffiro stellato |
| Lame Leggere | 3 Bacche di Palude, 1 Muschio Sanguigno | Citrino |
| Pesca | 3 Sale Marino, 1 Polvere di Tomba | Ambra |
| Costruzione Archi | 3 Occhio di Rospo, 1 Ginseng | Ambra |
| Concentrazione | 3 Bacche di Palude, 1 Aglio | Tormalina |
| Medicina Legale | 3 Uovo di Fata, 1 Perla Nera | Zaffiro |
| Guarigione ("of the healer") | 3 Vedova d'Argento, 1 Cenere Sulfurea | Ambra |
| Pastorizia | 3 Teschio di Folletto, 1 Cenere Sulfurea | Ambra |
| Nascondersi | 3 Cristallo Lunare, 1 Ala di Pipistrello | Ametista |
| Scrittura Arcana | 3 Occhio di Rospo, 1 Ghisa | Zaffiro |
| Identificare Oggetti | 3 Occhio di Rospo, 1 Cenere Sulfurea | Zaffiro |
| Scassinare | 3 Ali di Farfalla, 1 Radice di Mandragora | Tormalina |
| Taglialegna | 3 Orecchio di Gargoyle, 1 Aglio | Diamante |
| Combattere con Mazza | 3 Vedova d'Argento, 1 Ghisa | Diamante |
| Resistenza Magica | 3 Guscio di Scarabeo, 1 Cenere Sulfurea | Ambra |
| Meditazione | 3 Guscio di Scarabeo, 1 Ginseng | Ambra |
| Estrazione Mineraria | 3 Zolfo, 1 Ala di Pipistrello | Diamante |
| Musica | 3 Ali di Farfalla, 1 Belladonna | Zaffiro stellato |
| Parata | 3 Zolfo, 1 Cristallo di Nox | Ambra |
| Pacificazione | 3 Teschio di Folletto, 1 Cenere Sulfurea | Ambra |
| Avvelenamento | 3 Sale Marino, 1 Perla Nera | Zaffiro stellato |
| Provocazione | 3 Occhio di Rospo, 1 Ginseng | Smeraldo |
| Disinnescare Trappole | 3 Vedova d'Argento, 1 Ala di Pipistrello | Smeraldo |
| Curiosare | 3 Ali di Farfalla, 1 Belladonna | Ambra |
| Parlare con gli Spiriti | 3 Bacche di Palude, 1 Polvere di Tomba | Smeraldo |
| Furto | 3 Cristallo Lunare, 1 Cenere Sulfurea | Zaffiro |
| Furtività | 3 Teschio di Folletto, 1 Cenere Sulfurea | Citrino |
| Scherma con Spada | 3 Uovo di Fata, 1 Cristallo di Nox | Citrino |
| Tattica | 3 Orecchio di Gargoyle, 1 Seta di Ragno | Zaffiro stellato |
| Sartoria | 3 Zolfo, 1 Aglio | Smeraldo |
| Assaggiare | 3 Guscio di Scarabeo, 1 Aglio | Diamante |
| Meccanica | 3 Ali di Farfalla, 1 Aglio | Tormalina |
| Seguire Tracce | 3 Vedova d'Argento, 1 Radice di Mandragora | Tormalina |
| Veterinaria | 3 Loto Rosso, 1 Sangue di Demone | Zaffiro stellato |
| Lotta | 3 Cristallo Lunare, 1 Seta di Ragno | Zaffiro stellato |

### Effetto degli elisir

- **Bonus all'abilità:** 10 + EP/8 + Cucina/5 + Assaggiare/5 punti (fino a circa +50), ma mai oltre 125 di abilità totale.
- **Durata:** (120 + 2×EP + 2×Cucina + 2×Assaggiare)/120 minuti: da 1 minuto a circa 6 minuti e mezzo.
- Puoi avere **al massimo 2 elisir** attivi contemporaneamente, e non puoi bere due volte lo stesso elisir finché il primo non finisce.

> **Consiglio:** per l'alchimista "da elisir", Cucina e Assaggiare contano quanto l'Alchimia: raddoppiano bonus e durata.

---

## 9.6 Misture e liquidi

Si preparano in un **barattolo vuoto** (che torna indietro insieme a una bottiglia) partendo da una pozione normale già pronta. Alchimia **50 → 120**.

### Liquidi (pozze dannose)

Si versano a terra (doppio clic → bersaglio) e creano una pozza che danneggia i nemici che la calpestano. Il danno cresce con il Potenziamento Pozioni.

| Liquido | Ingredienti | Danno |
|---|---|---|
| Fuoco liquido (Liquid Fire) | 2 Zolfo, 1 pozione di Esplosione, 1 Perla Nera | Fuoco |
| Melma liquida (Liquid Goo) | 2 Bacche di Palude, 1 pozione di Cura, 1 Sangue di Demone | Energia |
| Ghiaccio liquido (Liquid Ice) | 2 Cristallo Lunare, 1 pozione di Purificazione, 1 Polvere di Tomba | Freddo |
| Dolore liquido (Liquid Pain) | 2 Orecchio di Gargoyle, 1 pozione di Visione notturna, 1 Aglio | Dolore |
| Putrefazione liquida (Liquid Rot) | 2 Teschio di Folletto, 1 pozione di Veleno, 1 Cristallo di Nox | Veleno |

### Misture di melma (alleati evocati)

Versate a terra, generano **1–4 melme** che combattono per te. Serve almeno uno slot seguace libero. La forza delle melme cresce con EP, Cucina e Assaggiare; il loro veleno con l'Avvelenamento (livello Avvelenamento/25 + 1).

| Mistura | Ingredienti | Melma | Danno |
|---|---|---|---|
| Mistura viscida (Slime) | 2 Guscio di Scarabeo, 1 pozione di Forza, 1 Ginseng | Melma | Fisico |
| Melma infetta (Diseased Slime) | 2 Occhio di Rospo, 1 pozione di Esplosione confusa, 1 Belladonna | Melma infetta | Veleno |
| Melma ardente (Fire Slime) | 2 Loto Rosso, 1 pozione di Conflagrazione, 1 Cenere Sulfurea | Melma ardente | Fuoco |
| Melma gelida (Ice Slime) | 2 Sale Marino, 1 pozione di Agilità, 1 Muschio Sanguigno | Melma gelida | Freddo |
| Melma irradiata (Irradiated Slime) | 2 Uovo di Fata, 1 pozione di Ristoro, 1 Ghisa | Melma irradiata | Energia |

### Altre misture

| Oggetto | Ingredienti | Abilità | Uso |
|---|---|---|---|
| Bottiglia d'acido (Bottle of Acid) | 2 Zolfo, 2 Cenere Sulfurea, 2 Sale Marino | 95 → 120 | Dissolve serrature e trappole della maggior parte dei forzieri (non quelli maledetti) |
| Olio del tecnomante (Technomancer Oil) | 20 Ghisa, 10 Melma liquida, 2 Mistura viscida | 95 → 120 | Ingrediente per i cristalli di potere |

---

## 9.7 Trasmutazioni

L'alchimista può trasformare 10 unità di un materiale nel grado successivo.

| Risultato | Materiale (10 unità) | Abilità |
|---|---|---|
| Lingotto di rame opaco (Dull Copper) | Ferro | 40 → 60 |
| Ferro ombra (Shadow Iron) | Rame opaco | 50 → 70 |
| Rame (Copper) | Ferro ombra | 60 → 80 |
| Bronzo (Bronze) | Rame | 70 → 90 |
| Oro (Gold) | Bronzo | 90 → 100 |
| Agapite | Oro | 100 → 120 |
| Verite | Agapite | 110 → 120 |
| Cuoio spinato (Spined) | Cuoio | 40 → 80 |
| Cuoio cornuto (Horned) | Spinato | 60 → 100 |
| Cuoio uncinato (Barbed) | Cornuto | 80 → 120 |
| Assi di frassino (Ash) | Assi qualsiasi | 30 → 50 |
| Ciliegio (Cherry) | Frassino | 40 → 60 |
| Ebano (Ebony) | Ciliegio | 50 → 70 |
| Quercia dorata (Golden Oak) | Ebano | 60 → 80 |
| Noce americano (Hickory) | Quercia dorata | 70 → 90 |
| Mogano (Mahogany) | Noce americano | 80 → 100 |
| Legno di deriva (Driftwood) | Mogano | 90 → 110 |
| Quercia (Oak) | Legno di deriva | 100 → 120 |
| Cristallo di potere (Power Crystal) | 10 Gemme arcane, 10 Olio del tecnomante, 50 Cristalli di Nox | 95 → 120 |

Le pozioni per capelli si fanno con Alchimia 80 → 110: **pozione per acconciature** (2 Teschio di Folletto) e **tintura per capelli** (3 Uovo di Fata).

> **Consiglio:** le trasmutazioni dei legni sono un ottimo modo per allenare l'Alchimia oltre 90 producendo qualcosa che si vende bene ai falegnami.

---

## 9.8 Alchimia necrotica (Necrotic Alchemy)

I negromanti preparano in un **calderone di miscelazione** (*mixing cauldron*) intrugli che funzionano come incantesimi in bottiglia: si "lanciano" bevendoli o usandoli, consumano il barattolo (che ti viene restituito vuoto) e richiedono un minimo di **Negromanzia** (la potenza dipende da Parlare con gli Spiriti). Non costano mana.

### Come si preparano

1. Procurati un **libro di alchimia necrotica** da un saggio o da un negromante e il calderone.
2. Servono **Medicina Legale ≥ 40** e una **fonte di calore** vicina.
3. Metti nel calderone, nell'ordine indicato dal calderone stesso: due ingredienti diversi ricavati con il **coltello da chirurgo** (*surgeon's knife*) dai cadaveri, un **reagente magico**, **ossa o una parte del corpo**, l'acqua di una **borraccia**; poi mescola con un **mestolo** (*cauldron mixer*, si consuma).
4. Le ricette **non sono note in partenza**: si scoprono sperimentando le combinazioni. Ogni scoperta viene annotata nel libro. Le ultime ricette richiedono reagenti magici rari.
5. Riempi i barattoli dal calderone.

Gli intrugli si trovano anche, raramente, presso streghe, becchini e la gilda dei negromanti (10–100 monete).

| Intruglio | Nome nel gioco | Negromanzia | Effetto |
|---|---|---|---|
| Miscela degli Occhi dei Morti | eyes of the dead mixture | 10 | 🎯 Visione dei morti: vedi al buio |
| Intruglio del Predatore di Tombe | tomb raiding concoction | 15 | 🎯 Gli spiriti aprono un contenitore chiuso a chiave |
| Pozione del Morbo | disease draught | 15 | 🎯 Fa ammalare (avvelena) il bersaglio |
| Elisir del Fantasma | phantasm elixir | 15 | 🎯 Uno spirito disinnesca una trappola |
| Elisir dell'Aria Nauseabonda | retched air elixir | 20 | 🎯 Getto di gas nocivo: 1d5+19 danni (calcolati come da fuoco) |
| Miscela Sanguisuga del Lich | lich leech mixture | 20 | 🎯 Assorbe il mana del bersaglio |
| Pozione del Muro di Spuntoni | wall of spikes draught | 24 | Crea un muro protettivo di spuntoni per 10 s |
| Intruglio Guarisci-Morbo | disease curing concoction | 25 | 🎯 Cura le malattie (veleno) |
| Elisir del Patto di Sangue | blood pact elixir | 25 | 🎯 Doni parte della tua vita a un altro (non a te stesso) |
| Elisir dell'Ombra Spettrale | spectre shadow elixir | 28 | 🎯 Rende invisibile chiunque, per 6 × Parlare con gli Spiriti/5 + 1 s |
| Intruglio della Fase Spettrale | ghost phase concoction | 35 | Il tuo corpo diventa materia spettrale e riappare altrove (costa un po' di karma) |
| Melma di Fuoco Demoniaco | demonic fire ooze | 40 | 🎯 Accende una runa marchiata e ti trasporta là (come Richiamo) |
| Pozione delle Immagini Spettrali | ghostly images draught | 45 | Crea un'immagine illusoria di te per (Negromanzia + Spiriti)/2 secondi |
| Melma del Marchio Infernale | hellish branding ooze | 55 | 🎯 Marchia una runa con simboli malvagi (come Marchio) |
| Pozione del Portale Nero | black gate draught | 80 | 🎯 Apre un orribile portale nero verso un luogo segnato (come Portale) |
| Pozione di Sangue di Vampiro | vampire blood draught | 80 | 🎯 Resuscita qualcuno; su te stesso crea un globo che ti farà risorgere |

---

## 9.9 Erboristeria druidica (Druidic Herbalism)

I druidi preparano in un **calderone dell'erborista** pozioni naturali che funzionano come incantesimi. Richiedono un minimo di **Conoscenza Animali**; la potenza dipende da **Addomesticare Animali**. Non costano mana.

### Come si preparano

1. Procurati un **libro di erboristeria druidica** da un saggio, un erborista o un druido, e il calderone.
2. Servono **Cucina ≥ 40** e una **fonte di calore**.
3. Nel calderone: due ingredienti diversi raccolti con l'**erboristeria** (attrezzo da giardino, *garden tool*, sulle piante), un **reagente magico**, l'acqua di una **borraccia**; poi mescola con il mestolo.
4. Come per l'alchimia necrotica, le ricette si **scoprono sperimentando** e vengono annotate nel libro.

| Pozione | Nome nel gioco | Conoscenza Animali | Effetto |
|---|---|---|---|
| Pietra Richiamo in Bottiglia | stone in a bottle (liquid lure stone) | 10 | Versa una pietra magica che attira tutti gli animali vicini |
| Miscela del Passaggio Naturale | nature passage mixture | 15 | 🎯 Diventi petali di fiore portati dal vento fino al luogo di una runa (come Richiamo) |
| Liquido dello Scudo di Terra | shield of earth liquid | 20 | Fa crescere un muro di vegetazione che blocca il passaggio |
| Olio di Protezione Silvestre | woodland protection oil | 25 | 🎯 La pelle diventa come corteccia: più protezione |
| Intruglio delle Pietre Sorgenti | stone rising concoction | 30 | Pietre emergono dal terreno e intrappolano i nemici |
| Miscela delle Radici Avvinghianti | grasping roots mixture | 35 | 🎯 Radici avvolgono e immobilizzano un bersaglio |
| Olio del Marchio Druidico | druidic marking oil | 40 | 🎯 Segna una runa con la tua posizione (come Marchio) |
| Elisir di Guarigione Erboristica | herbal healing elixir | 45 | 🎯 Cura il bersaglio da ogni malessere |
| Olio del Mimetismo Silvestre | forest blending oil | 50 | Ti confondi perfettamente con la foresta |
| Bottiglia di Lucciole | bottle of fireflies (firefly attraction potion) | 55 | Libera lucciole che distraggono un nemico dal combattimento |
| Portale di Funghi | mushroom gateway growth | 60 | 🎯 Funghi magici crescono formando un portale verso un altro luogo (come Portale) |
| Bottiglia di Insetti | bottle of insects (insect trapping mixture) | 65 | Sciame di insetti che morde e punge i nemici |
| Fata in Bottiglia | fairy in a bottle (fairy pheromones) | 70 | Libera una fata che aiuta l'avventuriero |
| Fertilizzante per Treant | treant fertilizer | 75 | Fa crescere un albero vivente che ti assiste |
| Fluido Vulcanico | volcanic fluid | 80 | 🌐 Lava erompe dal terreno colpendo tutti i nemici vicini |
| Fango Magico | bottle of magical mud | 85 | Versato a terra, riporta in vita i morti che lo calpestano |

> **Consiglio:** la gilda dei druidi di Montor dà un anello che aumenta Addomesticare e Conoscenza Animali: per un domatore-druido è il compagno ideale dell'erboristeria.

---

## 9.10 Oli e altri consumabili

- **Oli di potenziamento** (gemme, metallo, legno, cuoio, ghiaccio...): si fabbricano con l'artigianato divino (*God Crafting*) a partire da 90 di abilità e servono a cambiare il materiale di armi e armature.
- **Pergamene degli incantesimi**, **bacchette** e **frammenti d'anima**: vedi il [capitolo 8](08-magia.md).
- **Teschio malvagio** e **gemma arcana**: ripristinano mana o sostituiscono i reagenti (capitolo 8).
