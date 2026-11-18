# 13. Il mondo, le terre e i dungeon

Ultima Adventures non usa la Britannia classica di Ultima Online: le sei mappe del client (Trammel, Felucca, Ilshenar, Malas, Tokuno, Ter Mur) più una settima mappa personalizzata (Midland) sono state ridisegnate e suddivise in **terre** distinte, ognuna con città, mari, dungeon e mostri propri. Questo capitolo spiega come è organizzato il mondo, dove andare a seconda del tuo livello, come spostarti tra le terre, quali missioni puoi svolgere e quali creature incontrerai.

I nomi propri (città, dungeon, terre) restano in inglese, come nel gioco: così li riconosci sulle mappe e nei messaggi. Quando una traduzione aiuta a capire, la trovi accanto tra parentesi.

---

## 13.1 Le terre e le mappe

Il gioco decide in quale terra ti trovi in base alla mappa e alle coordinate (funzione `Worlds.GetMyWorld`). Il nome della terra compare nei messaggi, nei contratti delle missioni e nel diario delle scoperte.

| Terra (nome nel gioco) | Mappa del client | Dove si trova | Carattere |
|---|---|---|---|
| **the Land of Sosaria** | Trammel | Continente principale | Terra di partenza: città sicure, dungeon facili e normali |
| **the Island of Umber Veil** | Trammel (sud-ovest) | Isola separata | Città di Renika, Mausoleum, Tower of Brass |
| **the Land of Ambrosia** | Trammel (sud-est) | Terra vulcanica | Dragon's Maw, Cave of the Zuluu, City of the Dead; niente Richiamo |
| **the Moon of Luna** | Trammel (est) | Sulla luna | Lunar City of Dawn, Moonlight Cavern, nucleo della luna |
| **the Bottle World of Kuldar** | Trammel (est estremo) | Un mondo dentro una bottiglia | Città di Kuldara, Vordo's Castle; da qui non si fugge facilmente |
| **the Land of Lodoria** | Felucca | Secondo continente, "il mondo degli elfi" | Dungeon classici (Covetous, Deceit, Despise, Destard, Shame...) |
| **the Town of Skara Brae** | Felucca | Città-isola distrutta | Grande missione ispirata a *The Bard's Tale* |
| **the Serpent Island** | Malas | Isola dei gargoyle | Città di Furnace; dungeon difficili e mortali |
| **the Isles of Dread** | Tokuno | Arcipelago selvaggio | Cimmeran Hold; tutte le creature possono essere "maledette" |
| **the Savaged Empire** | Ter Mur | Valle barbarica | Villaggi di Barako e Kurak; terra di partenza per i personaggi barbari |
| **the Underworld** | Ilshenar (e parti di Felucca/Trammel) | Mondo sotterraneo | Stygian Abyss, Zealan Tombs, Morgaelin's Inferno; solo per esperti |
| **DarkMoor** | Ilshenar (nord-ovest) | Terra del male | Tempio di Praetoria; accesso solo a personaggi malvagi |
| **the Midlands** | Midland | Terra a sé con regole proprie | Midkemia, Freeport, Calypso, Ha'ars Crag; niente Richiamo e niente bonus magici |

Nota: molti dungeon "appartengono" a una terra ma il loro interno sta fisicamente su un'altra mappa. Per esempio l'ingresso del Temple of Osirus è nelle Isles of Dread (Tokuno) ma le sale sono su Felucca; l'ingresso del Vault of the Black Knight è a Serpent Island (Malas). Non stupirti se il nome della mappa cambia quando scendi le scale.

### Scoprire una terra

Il gioco tiene un elenco delle terre che il tuo personaggio ha **scoperto**. Una terra si scopre semplicemente entrandoci (a piedi, attraverso un passaggio, un portale o una missione). L'elenco è visibile nel menu **Aiuto → Missioni** ("Discovered the World of Sosaria", "Discovered Umber Veil"...).

La scoperta conta per tre cose:

1. **Portali lunari**: puoi scegliere come destinazione solo le terre già scoperte.
2. **Contratti delle bacheche**: le missioni possono mandarti solo in terre che conosci.
3. **Diario**: segna i tuoi progressi di esploratore.

> **Consiglio:** appena raggiungi una terra nuova, cerca subito il suo portale lunare e prendi nota della posizione. Da quel momento potrai tornarci da qualunque altro portale.

### Livello di difficoltà del luogo

Ogni dungeon ha un livello di difficoltà che il gioco ti comunica appena entri, ad esempio *"You have entered Dungeon Despise (Hard)."*:

| Valore | Etichetta in gioco | Traduzione | Effetto |
|---|---|---|---|
| -1 | Easy | Facile | Mostri più deboli, nessuna trappola né forziere nascosto di valore |
| 0 | Normal | Normale | Valori standard; compaiono trappole e forzieri |
| 1 | Difficult | Difficile | Mostri potenziati di un grado |
| 2 | Challenging | Impegnativo | Mostri potenziati di due gradi |
| 3 | Hard | Arduo | Mostri potenziati di tre gradi |
| 4 | Deadly | Mortale | Mostri potenziati al massimo |

Che cosa cambia in pratica (da `BaseCreature.BeefUp` e `MyServerSettings.AdditionalHitPoints`):

- Per ogni grado oltre lo 0, i mostri guadagnano circa **+10% PF e Forza**, **+30% Destrezza, Intelligenza e abilità**, **+1 al danno**, +10% oro, Fama e Karma.
- In più ricevono PF aggiuntivi: +30–50% al livello 1, +55–85% al livello 2, +90–110% al livello 3, +115–175% al livello 4.
- Le creature molto famose sono limitate: Fama ≥ 20.000 non vengono potenziate, Fama ≥ 18.000 al massimo di 1 grado, ≥ 15.000 di 2, ≥ 10.000 di 3.
- **Apprendimento più rapido**: nei dungeon le abilità di combattimento (Scherma con Spada, Lame Leggere, Combattere con Mazza, Tiro con l'Arco, Magia, Negromanzia, Cavalleria, Bushido, Ninjitsu, Furto, Furtività, abilità bardiche) crescono con un moltiplicatore 1 + livello/5 (fino a ×1,8 nei dungeon Mortali). Le abilità di supporto (Tattica, Parata, Anatomia, Rivelare Nascosti, Meditazione, Scassinare, Disinnescare Trappole, Valutare Intelligenza, Nascondersi, Seguire Tracce, Estrazione Mineraria...) hanno 1 + livello/15. Nei dungeon Facili il moltiplicatore scende sotto 1 (×0,8 per il combattimento).
- **L'equilibrio del mondo** dovrebbe modificare questi PF aggiuntivi: più PF ai mostri malvagi quando il mondo pende verso il caos, più PF ai mostri buoni quando pende verso l'ordine. A causa di un errore nel codice, però, il calcolo dà sempre lo stesso risultato: i mostri malvagi ricevono il 25% in meno di PF aggiuntivi e quelli buoni il 35% in più, qualunque sia l'equilibrio. Lo stato dell'equilibrio si legge sul globo nella banca (vedi 13.5).

Per i contratti delle bacheche il gioco calcola anche il **tuo** livello (da Fama, Karma, totale delle abilità e caratteristiche): fino a livello 24 ricevi incarichi solo in luoghi di difficoltà ≤ 0, da 25 fino a 1, da 50 fino a 2, da 75 fino a 3, da 95 fino a 4.

---

## 13.2 Le città principali e i loro servizi

### Città di partenza

Un nuovo personaggio passa prima per un'area introduttiva e poi sceglie una delle **otto città di partenza** di Sosaria: **Britain, Fawn, Moon, Yew, Devil Guard, Death Gulch, Grey, Montor**.

### Città per terra

| Terra | Città e insediamenti |
|---|---|
| Sosaria | Britain (con il castello), Yew, Moon, Fawn, Grey, Montor, Devil Guard (e le sue fattorie), Death Gulch, Mountain Crest, Glacial Coast Village, Iceclad Fisherman's Village, Lamut County, Sarth Abbey, Anchor Rock Docks (porto), Sosarian Keep (zona sicura); the Undercity of Umbra (città sotterranea che accoglie anche i malvagi) |
| Umber Veil | the Town of Renika |
| Moon of Luna | the Lunar City of Dawn, the Celestial Moon of Luna |
| Kuldar | the City of Kuldara |
| Lodoria | the City of Lodoria (con il castello), Dusk (porto), Elidor, Glacial Hills, Greensky, Islegem, Portshine, Springvale, Starguide (porto), Whisper, Kraken Reef Docks, the Castle of Knowledge; Ravendark (villaggio dei negromanti) |
| Serpent Island | the City of Furnace (città dei gargoyle), Serpent Sail Docks |
| Isles of Dread | the Cimmeran Hold, the Forgotten Lighthouse (zona sicura) |
| Savaged Empire | Village of Barako, Village of Kurak, Savage Sea Docks, miniere di Barako e dei barbari |
| DarkMoor | the Temple of Praetoria |
| Midlands | Midkemia, Freeport, the port of Calypso, Ha'ars Crag |

### Servizi che trovi nelle città

| Servizio | Dove | A che serve |
|---|---|---|
| **Banca** (banchiere) | In ogni città principale | Cassetta di sicurezza; deposita oro, monete d'argento e di rame, gemme e cristalli e fai doppio clic per convertirli in oro. Dentro la banca c'è il **globo dell'equilibrio** e, lungo la parete ovest, i **forzieri per i novizi** dove lasciare oggetti utili ad altri |
| **Taverna / locanda** | Quasi ovunque | Bacheche delle missioni ("Seeking Brave Adventurers"), mercenari e scudieri da assumere, stallaggio |
| **Guaritore** | Città e strade | Resurrezione e cure |
| **Mercanti** (fabbro, armaiolo, sarto, conciatore, falegname, arciere, alchimista, mago, ecc.) | Città | Comprano e vendono. Cosa vogliono comprare cambia a intervalli: un fabbro che oggi rifiuta i tuoi gambali potrebbe accettarli fra qualche ora |
| **Maestri di gilda** | Città | Iscrizione alle gilde NPC: prezzi migliori con i mercanti del tuo mestiere e missioni dedicate |
| **Insegnanti di abilità** | Clic sinistro su un PNG → scegli l'abilità | Ti portano rapidamente le abilità di base a un valore utile |
| **Saggi** (sage) | Città | Missioni per trovare manufatti (vedi 13.5) |
| **Bibliotecari e biblioteche** | Sarth Abbey (a sud dell'edificio principale), Westshire | Libri di indizi, segreti e manuali di addestramento |
| **Guardie** | Città "blu" | Proteggono gli innocenti, attaccano chi ha Karma ≤ -4000 o 3 omicidi e accettano taglie e teste di fuorilegge |
| **Moli e cantieri navali** | Porti | Navi e missioni di pesca |

Luoghi speciali da conoscere (regioni "pubbliche" di Sosaria): the Hall of Legends (oggetti leggendari), Xardok's Castle (gilda degli Assassini), the Thieves Guild (gilda dei Ladri), the Wizards Guild (gilda dei Maghi), the Black Magic Guild (gilda della Magia Nera), the Church of the Divine, the Lost Glade.

> **Consiglio:** prima di uscire dalla città di partenza deposita in banca l'oro che non ti serve. Se muori lontano, con l'oro in banca puoi ricomprare l'equipaggiamento di base.

> **Consiglio:** non buttare a terra ciò che non vuoi. In Ultima Adventures **gli oggetti a terra non spariscono mai** (cadaveri compresi). Ciò che può servire ad altri mettilo nei forzieri per i novizi in banca.

---

## 13.3 Viaggiare tra le terre

### I portali lunari

Il mezzo principale è il **portale lunare** (moongate). Ci entri sopra, si apre la finestra **MOONGATE DESTINATIONS** e scegli la destinazione. Le regole (da `GateMoon.cs`):

- Vedi solo le destinazioni delle **terre che hai già scoperto**.
- Devi trovarti **entro 2 caselle** da un portale lunare (o da un portale di ossidiana) quando confermi.
- I tuoi animali vengono portati con te.
- Il portale non funziona dove non è permesso il Richiamo, salvo eccezioni per Ambrosia e Kuldar.
- **Kuldar**: dall'interno del mondo nella bottiglia il portale "non fa nulla" finché non possiedi la **chiave di Vordo** (VordoKey).

Destinazioni disponibili (31 in totale):

| Gruppo | Destinazioni |
|---|---|
| Sosaria | Central, Clues, Devil Guard, East, Frozen Isles, Lost Glade, Montor, Moon, WestGate, Yew |
| Ambrosia | Isle of Fire, Land of Ambrosia |
| Umber Veil | Lost Isle, Island of Umber Veil |
| Isles of Dread | Isles of Dread |
| Lodoria | Greensky, Islegem, Portshine, South, Springvale, Whisper, Winterlands |
| Serpent Island | North, South, West |
| Savaged Empire | North, South, West |
| Kuldar | Bottle World of Kuldar, Black Knight's Vault |
| DarkMoor | The Lands of DarkMoor |

**Come trovare il portale più vicino**: menu **Aiuto → Moongate Search**. Una freccia ti guida; segui i portali che indica finché arrivi a un portale lunare o a una grande roccia nera.

La **roccia nera** (portale di ossidiana, *Blackrock gate*) apre la stessa lista con il titolo "BLACKROCK GATE DESTINATIONS": doppio clic per usarla.

### Altri passaggi tra le terre

| Passaggio | Collega | Requisiti |
|---|---|---|
| **Serpent Pillars** (Pilastri del Serpente) | Sosaria ↔ Lodoria | Devi conoscerne il segreto; altrimenti "subisci l'ira di Poseidone" e prendi danni. Coordinate dopo la scoperta: Sosaria 86° 41'S, 124° 39'E; Lodoria 35° 36'S, 65° 2'E |
| **Lodoria Forest** | Ingresso a Lodoria | Lodoria viene segnata come scoperta quando esci da questa piccola foresta protetta |
| **Skull Gate** (Porta del Teschio) | Undercity of Umbra (Sosaria) ↔ Ravendark Woods (Lodoria) | Devi aver scoperto il segreto della porta |
| **Undersea Pass** | Grotta sottomarina di Lodoria | — |
| **"a city in a bottle"** | Ingresso a Kuldar | Scopre la terra; per uscire serve la chiave di Vordo |
| **Underworld Gate** e porte di pietra runica | Sosaria → Underworld | Porte coperte di rune: richiedono le missioni dell'Underworld |
| **Moorstone / lo Straniero** | Verso DarkMoor | Karma ≤ -5000. Uno straniero a Lamut County conosce la via segreta per DarkMoor e Praetoria. Entrando a DarkMoor lo stato di criminale viene cancellato |
| **MidlandExitMoongate** | Midlands | Il punto di uscita nelle Midlands cambia ogni 250 minuti tra cinque luoghi (Central Town, Broken Fortress Town, Central Bridge, South Eastern Road, Western Oasis) |
| Personaggio **barbaro** | Savaged Empire | I barbari nascono qui con la terra già scoperta |

### Richiamo, Portale e Teletrasporto

Il gioco limita i viaggi magici (`Worlds.RegionAllowedRecall` e `RegionAllowedTeleport`):

| Dove | Richiamo / Portale | Marchio e teletrasporto |
|---|---|---|
| Town of Skara Brae | No | No |
| Bottle World of Kuldar | No | No |
| Land of Ambrosia | No | No |
| Midlands | No | No |
| Village of Ravendark, Moonlight Cavern, Doom Gauntlet | No | No |
| Underworld | Sì | No |
| Qualunque dungeon | Sì (uscita) | Non puoi marcare runa né teletrasportarti dentro |
| Città dei gargoyle, labirinti, regioni pubbliche (banca, taverna, gilde), castello del Cavaliere Nero | Sì | No |

### Navi

Puoi navigare, pescare, cacciare pirati e raggiungere isole altrimenti irraggiungibili. Tieni presente che **ogni terra ha il suo mare chiuso**: arrivato al bordo, la nave ricompare dal lato opposto della stessa terra. **Non puoi passare da una terra all'altra in nave**: per cambiare terra servono portali e passaggi.

- Porti principali: Anchor Rock Docks (Sosaria), Kraken Reef Docks (Lodoria), Serpent Sail Docks (Serpent Island), Savage Sea Docks (Savaged Empire), the Forgotten Lighthouse (Isles of Dread).
- Le **acque dei pirati** (regioni come "the Waters of the Mad Wolf", "the Waters of the Bloody Wave"...) sono pattugliate da navi pirata. Sulle loro navi trovi le **taglie sui pirati** (vedi 13.5).
- Una nave inutilizzata decade dopo 30 giorni; usarla azzera il conteggio.

> **Consiglio:** girare le isole di Sosaria in barca è uno dei modi migliori per scoprire nuovi portali lunari, dungeon e luoghi segreti. La pesca d'altura dà oggetti potenti, ma su una nave non hai spazio per scappare: preparati prima di salpare.

### Incontri casuali

Fuori dalle città e dalle zone sicure puoi imbatterti in **incontri casuali**: guaritori e banchieri ambulanti, carri abbandonati e avventurieri morti con il loro bottino, ma anche bande di orchi, demoni che escono da un portale o gruppi di PK. Sono più frequenti e pericolosi nelle terre avanzate. Restare nascosti **non** basta a evitarli. Diffida anche dei portali che compaiono nelle terre selvagge: di solito portano in posti peggiori.

---

## 13.4 I dungeon in ordine di difficoltà

Le tabelle seguono il livello assegnato da `MyServerSettings.GetDifficultyLevel`. Le coordinate degli ingressi principali si leggono in gioco nell'elenco dei dungeon (sestante). Nei dungeon di livello Normale e superiore compaiono **trappole** e **forzieri nascosti** generati a caso.

### Livello -1 – Facile (dungeon per principianti di Sosaria)

| Dungeon | Note |
|---|---|
| the Accursed Maze | Labirinto per i primi passi |
| the Chamber of Bane | — |
| Coldhall Depths | — |
| the Dark Sanctum | — |
| the Forgotten Tombs | Non morti deboli |
| the Magma Vaults | — |
| the Shrouded Grave | Non morti deboli |

Sono i "Starter Dungeons": piccoli, senza trappole serie, ideali per addestrare le armi e accumulare il primo oro. Anche i **cimiteri** (the Graveyard, Sarth Cemetery) vanno bene per iniziare.

### Livello 0 – Normale

| Terra | Dungeon |
|---|---|
| Sosaria | Dungeon Clues, Dardin's Pit, Dungeon Abandon, Dungeon Exodus, Castle Exodus, the Ancient Pyramid, the Cave of Banished Mages, Frostwall Caverns, the Frozen Dungeon, the Fires of Hell, the Mines of Morinia, the Perinian Depths, the Ratmen Lair, the Montor Sewers |
| Umber Veil | the Mausoleum |
| Ambrosia | the Dragon's Maw, the Cave of the Zuluu (vedi avvertenza sotto) |
| Skara Brae | the Sewers, the Cellar |

> **Consiglio:** Dragon's Maw e Cave of the Zuluu risultano "Normali", ma si trovano in Ambrosia, una terra dove non funziona il Richiamo. Nelle sale interne di Dragon's Maw vivono i grandi draghi (del Vuoto, delle Radiazioni, di Cristallo, Anziano) con oltre 550 PF. Non sono dungeon per principianti.

### Livello 1 – Difficile

| Terra | Dungeon |
|---|---|
| Sosaria | the Dungeon of Time Awaits (qui si trova il Signore del Tempo), the Caverns of Poseidon, the Lost Graveyard |
| Umber Veil | the Tower of Brass |
| Ambrosia | the City of the Dead |
| Lodoria | the Lodoria Sewers, the Crypt, the Volcanic Cave, Dungeon Hythloth, the City of Embers |
| Skara Brae | Harkyn's Castle, the Catacombs, the Lower Catacombs, the Mines |
| Savaged Empire | the Pixie Cave |
| DarkMoor | Tutta la terra |

### Livello 2 – Impegnativo

| Terra | Dungeon |
|---|---|
| Sosaria | the Ruins of the Black Blade |
| Lodoria | the Lizardman Cave, the Ratmen Cave, Dungeon Wrong, the Ice Fiend Lair, the Frozen Hells, the Lodoria Catacombs, the Castle of Dracula |
| Serpent Island | the Ancient Prison, the Gargoyle Crypts |
| Isles of Dread | the Temple of Osirus, the Glacial Scar, the Altar of the Blood God |
| Savaged Empire | the Ratmen Mines, the Sakkhra Tunnel, the Spider Cave, the Orc Fort |
| Skara Brae | Mangar's Tower, Kylearan's Tower |

### Livello 3 – Arduo

| Terra | Dungeon |
|---|---|
| Lodoria | Terathan Keep, Dungeon Shame, Dungeon Destard, Dungeon Despise, Dungeon Deceit, Dungeon Covetous, the Mind Flayer City, the Halls of Undermountain, the Crypts of Dracula |
| Kuldar | Steamfire Cave, the Kuldara Sewers, the Crypts of Kuldar, Vordo's Castle, Vordo's Dungeon, Vordo's Castle Grounds |
| Serpent Island | the Cave of Fire, the Cave of Souls, Dungeon Ankh, Dungeon Bane, Dungeon Hate, Dungeon Vile, Dungeon Wicked, the Serpent Sanctum, the Tomb of the Fallen Wizard |
| Isles of Dread | the Sanctum of Saltmarsh, the Blood Temple, the Ice Queen Fortress, the Scurvy Reef |
| Savaged Empire | the Corrupt Pass, the Crypt, the Great Pyramid, the Altar of the Dragon King, Dungeon Rock, the Isle of the Lich, the Castle of the Mad Archmage, the Island of the Storm Giant, the Hedge Maze, the Forgotten Halls, the Tomb of Kazibal, the Azure Castle |
| Underworld | Le gallerie generiche dell'Underworld |
| Skara Brae | Mangar's Chamber |

### Livello 4 – Mortale

| Terra | Dungeon |
|---|---|
| Lodoria | Stonegate Castle, the Ancient Elven Mine |
| Serpent Island | Dungeon Scorn, Dungeon Torment, Dungeon Wrath, the Flooded Temple, the Castle of the Black Knight, the Vault of the Black Knight |
| Savaged Empire | the Tombs, the Dungeon of the Lich King, the Dungeon of the Mad Archmage, the Halls of Ogrimar, the Storm Giant Lair, the Cave of the Ancient Wyrm, the Mage Mansion, the Undersea Castle, the Catacombs of Azerok, the Ancient Crash Site |
| Underworld | the Stygian Abyss, the Zealan Tombs, Morgaelin's Inferno, Argentrock Castle, the Daemon's Crag, the Hall of the Mountain King, the Depths of Carthax Lake, the Ancient Sky Ship |
| Ilshenar | Doom, Doom Gauntlet, Widow's Lament |

### Percorso consigliato per un principiante

1. **Primi giorni**: dungeon Facili di Sosaria e cimiteri, finché le abilità d'arma arrivano a 50–60.
2. **Prima crescita**: dungeon Normali di Sosaria (Clues, Dardin's Pit, Abandon, Exodus, Ratmen Lair, Mines of Morinia, Perinian Depths) e il Mausoleum di Umber Veil.
3. **Verso i 70–80 di abilità**: Time Awaits, Caverns of Poseidon, Tower of Brass; a Lodoria Hythloth, City of Embers e Volcanic Cave.
4. **Personaggio formato (GM in un'arma, buone resistenze)**: dungeon Impegnativi di Lodoria (Wrong, Lizardman Cave, Frozen Hells, Lodoria Catacombs) e Isles of Dread.
5. **Esperto**: i dungeon classici di Lodoria (Covetous, Deceit, Despise, Destard, Shame), Serpent Island, Kuldar.
6. **Fine gioco**: dungeon Mortali, Underworld, Savaged Empire profondo.

> **Consiglio:** nei dungeon usa **Rivelare Nascosti** continuamente (mettilo su un tasto rapido). Le trappole rivelate brillano a terra. Possono distruggere l'equipaggiamento, rovinare il contenuto dello zaino, ucciderti o teletrasportarti in posti peggiori. Ti proteggono Disinnescare Trappole (che dà anche una resistenza passiva), un'asta da dieci piedi, l'incantesimo di disinnesco, resistenze alte, Fortuna e Resistenza Magica.

> **Consiglio:** i forzieri casuali dei dungeon si spostano e svaniscono da soli. Se però raccogli un forziere e lo sposti, resta per sempre dove lo lasci.

> **Consiglio:** ogni 250 minuti il server cambia a caso lo stato delle porte dei dungeon (aperte, chiuse a chiave, più o meno difficili da scassinare) e lo annuncia con il messaggio "Dungeon doors have been randomized!". Una porta chiusa oggi può essere aperta più tardi.

---

## 13.5 Missioni, taglie e sistemi di avventura

### Il diario delle missioni

Menu **Aiuto → Missioni** elenca le tue missioni attive (contratti di avventura, assassinio e navigazione), le chiavi trovate (chiave nanica di Undermountain, chiave del Cavaliere Nero), i segreti scoperti (Skull Gate, Serpent Pillars, Ranger Outpost), le tappe della missione di Skara Brae e le terre scoperte. Non compaiono i messaggi in bottiglia, le mappe del tesoro e gli appunti trovati.

### Bacheca delle missioni (contratti da avventuriero)

Nelle locande trovi la bacheca **"Seeking Brave Adventurers"**. Scegli **Hire Me** per ricevere un incarico:

- **Uccidere** una creatura malvagia (ricompensa = Fama della creatura / 5), oppure
- **Trovare un oggetto** in un dungeon (ricompensa = Fama / 3, arrotondata alle centinaia). Mentre esplori la regione indicata, ogni forziere aperto o creatura uccisa ha il **10%** di probabilità di fartelo trovare.

Il bersaglio è scelto tra le creature di Karma negativo dentro un dungeon, in una terra che conosci e di difficoltà adatta al tuo livello. Torna alla bacheca e scegli **Done** per incassare. Ottieni anche Fama e Karma pari a 1/100 della ricompensa e hai il **5%** di probabilità di ricevere un regalo (manufatto, arma, armatura o gioiello potenziato, strumento, faretra, bacchetta).

Per **rinunciare** deposita sulla bacheca esattamente la somma della ricompensa: l'incarico viene annullato. Fra un contratto e l'altro devi aspettare 1 minuto.

### Gilda degli Assassini (Xardok)

Xardok, nel suo castello, affida contratti di assassinio. Requisiti: essere membro della **gilda degli Assassini** e avere **Karma ≤ -1250**. Paga in oro (Fama del bersaglio / 5); si rinuncia come per la bacheca.

### Missioni di navigazione e pesca

Nei porti la **bacheca dei marinai** offre incarichi "in alto mare" (the high seas) in una terra che conosci: uccidere un mostro marino o un capitano pirata, oppure ripescare un oggetto. Il compenso dipende dalla Fama del bersaglio.

### Taglie

| Tipo | Come funziona | Ricompensa |
|---|---|---|
| **Taglia sui pirati** (Pirate Bounty) | Contratto trovato sulle navi pirata; consegnalo a una guardia | 1000–3000 oro (variano con il tasso dell'oro del server), più Fama pari a 1/5 del valore. Attenzione: consegnarla toglie anche Karma per lo stesso importo, quindi non conviene a chi vuole restare buono |
| **Teste di fuorilegge** | Trascina la testa sulla guardia | Ladro 80–120 oro, brigante 50–80, bandito 30–40, pirata 120–160, assassino 100–140, più Karma. Le guardie "blu" pagano anche le teste di molti altri PNG ostili: assassini di giocatori (200–500 oro, 750–1500 se a cavallo), maghi, bardi, monaci, boia, cavalieri, cultisti |

Le guardie rifiutano le taglie se ti considerano un nemico.

### Gilda dei Ladri

Con **Karma ≤ -500** puoi iscriverti alla gilda dei Ladri. Ti dà missioni di furto (appunti che indicano forzieri, casse o ceppi cavi da svuotare nei dungeon, con ricompense che crescono con la difficoltà) e ti permette di rubare ad altri giocatori. Chi ti cattura con un doppio clic ti immobilizza per un po' e può prendere ciò che hai nello zaino.

### Saggi e manufatti

I **saggi** delle città vendono voci su dove trovare un manufatto. Dai loro tra 5.000 e 20.000 oro: più paghi, più la voce è affidabile. Scegli il manufatto dal libro e ricevi un foglietto con un dungeon. Lì troverai alcuni forzieri su piedistalli di pietra: uno solo contiene l'oggetto (cambia ogni volta). Per cambiare missione paga di nuovo il saggio. I piedistalli vengono riorganizzati ogni giorno.

### Missioni di tributo

PNG dal nome verde (nelle città o, per i malvagi, nascosti nei dungeon) chiedono Fama, oro e un oggetto lasciato da un boss di dungeon. In cambio ti danno un oggetto personalizzabile con bonus alle abilità del loro mestiere: scegli tu lo slot e le proprietà. Tutti chiedono lo stesso oggetto, che cambia appena qualcuno completa il tributo.

### Altre missioni e sistemi

| Sistema | In breve |
|---|---|
| **Corriere** (Courier) | Consegne e ricerche verso luoghi adatti al tuo livello |
| **Prigionieri** | Trovi prigionieri in gabbia nei dungeon: liberali e diventano seguaci (guerrieri, maghi...) |
| **Pozze magiche** | Pozze nei dungeon con effetti casuali, buoni o cattivi |
| **Tesori ammassati** (Hoard) | Mucchi di tesoro sorvegliati |
| **Museo** | Raccolte di oggetti da completare |
| **Missioni maggiori** | Serpents (Bane), Shadowlords (Fiamme), Pagan (obelisco e portale di ossidiana), Rune, Codex (Camera del Codex nell'Underworld): catene lunghe che portano in più terre e sbloccano passaggi e oggetti unici |
| **Skara Brae** (*The Bard's Tale*) | Missione lunga e a enigmi nella città distrutta: chiavi, triangolo, quadrato e cerchio d'argento, golem di cristallo, il mago Mangar. Vincendola il tuo **limite di abilità diventa illimitato**. Da lì non si esce con il Richiamo |
| **Frankenstein, Golem, Robot** | Missioni di costruzione: raccogli i pezzi e assembli un servitore |
| **Ricerca arcana** | 500 oro a un bibliotecario per la borsa di ricerca: lunghissima catena per otto scuole di incantesimi |
| **Oggetti leggendari** | Fama e Karma (positivo o negativo) al massimo e 20.000 oro, poi la Hall of Legends |

### Equilibrio tra bene e male

Al **Signore del Tempo**, nelle profondità di the Dungeon of Time Awaits, un personaggio normale può **giurare fedeltà al Bene o al Male**. La scelta è **permanente** e blocca il tuo Karma sopra o sotto lo zero. Da quel momento accumuli punti equilibrio che spendi presso il Signore del Tempo e che spostano l'**equilibrio globale**.

Il globo al centro della banca mostra se il mondo pende verso il caos (rosso) o l'ordine (verde). Fai doppio clic per vedere il tuo punteggio e i campioni attuali. Il **campione del Bene** e il **campione del Male** possono usare il Richiamo liberamente e non subiscono penalità alla morte. L'equilibrio influisce anche sui prezzi dei mercanti (un personaggio buono vende peggio quando prevale il male) e sui danni degli Avatar: quando prevale l'ordine, gli Avatar buoni infliggono fino al 17% di danni in più.

### Essere malvagi

Con **Karma ≤ -4000** o **3 omicidi** sei considerato malvagio: le guardie blu ti attaccano a vista (e hanno le bolas). Un omicidio decade dopo 40 ore di gioco; un PNG specifico può cancellarli a pagamento. Ai malvagi sono riservati DarkMoor, Praetoria, la città segnata "CHAOS" sulla mappa (dal portale "Sosaria - Central" vai verso ovest) e una caverna a est di Sarth Abbey, lungo il fiume vicino a Umbra, che accoglie chiunque.

### Compiti automatici del server (Task Manager)

Il mondo cambia da solo a intervalli regolari. Conoscere questi cicli aiuta a pianificare:

| Ogni | Cosa succede |
|---|---|
| 60 minuti | Mercanti e guardie tornano al loro posto; gli specchi magici cambiano; si azzerano le liste di furto e di curiosare |
| 150 minuti | Trappole, altari e forzieri dei dungeon vengono cambiati; si ripiantano i giardini |
| 250 minuti | Nuovi cittadini negli insediamenti; porte dei dungeon riassegnate; l'uscita delle Midlands si sposta; controllo delle infezioni (Infected Keep) |
| 23–24 ore | Casse delle missioni di ricerca e piedistalli dei furti riorganizzati; pagamento degli investimenti; aggiornamento dell'equilibrio, dei campioni e delle invasioni; pulizia dei mostri sparsi; ripopolamento di villaggi e città |

---

## 13.6 Bestiario sintetico

### Regole generali sui mostri

- **Statistiche semi-casuali**: molte creature hanno un intervallo di PF molto ampio. Il prefisso "greater" (un giaguaro maggiore ha 64–188 PF) indica un esemplare più forte. Due orsi uno accanto all'altro possono differire di 3–5 volte.
- **Varianti maledette** (Paragon): a Serpent Island e nelle Isles of Dread le creature malvagie possono comparire in versione colorata con **PF ×3**, +20% alle caratteristiche e alle abilità, +5 al danno, più veloci, con Fama e Karma ×1,4. Probabilità tra 1/20 e 1/10, maggiore per i mostri più famosi. Possono lasciare manufatti.
- **Draghi, demoni e balron elementali**: draghi (Dragons), demoni (Daemon) e balron hanno decine di varianti per colore e materiale ("a sapphire dragon", "a brass dragon"...). La variante decide l'elemento del soffio, il tipo di danno e le resistenze: zaffiro e azzurrite = freddo; ottone, mithril, acciaio, titanio, perla, viola = energia; drago velenoso (poison) e di ruggine (rust) = veleno; drago di sangue (blood) = vuoto; rubino, topazio, bronzo, verite, valorite e gran parte delle altre = fuoco.
- **Chi frugano i cadaveri**: orchi, uomini ratto, uomini lucertola, troll e simili raccolgono il bottino dai cadaveri. Uccidili prima che svuotino il tuo.
- **Le creature vedono i nascosti**: in questo server i mostri hanno Rivelare Nascosti, quindi nascondersi non sempre ti salva.

Legenda: PF = punti ferita base (prima dei potenziamenti del dungeon); Danno = danno base per colpo; Res = resistenze principali (Fi = fisico, Fu = fuoco, Fr = freddo, Ve = veleno, En = energia).

### Livello 1 – Innocui o quasi (Fama fino a 1000)

| Creatura | PF | Danno | Note |
|---|---|---|---|
| ratto, ratto di fogna, coniglio, mongbat | 4–6 | 1–2 | Bersagli d'allenamento |
| ratto gigante | 26–39 | 4–8 | Veleno minore |
| melma (slime) | 15–19 | 1–5 | Veleno minore; danneggia le armi |
| goblin | 25–38 | 1–6 | Res Fi 25–30 |
| coboldo / sciamano coboldo | 26–48 | 2–8 | Lo sciamano lancia incantesimi |
| scheletro | 34–48 | 3–7 | Immune al veleno minore; Res Fr 25–40 |
| zombi | 28–42 | 3–7 | Veleno minore; non risente della musica dei bardi |
| serpe (snake) | 20–25 | 3–9 | Veleno minore |
| ragno gigante | 46–60 | 5–13 | Veleno normale; addestra Guarigione e Purificazione |
| lupo grigio | 34–48 | 3–7 | Addomesticabile (53) |
| shambler (bogling) | 58–72 | 5–7 | Lento ma resistente |
| servo dell'orda | 10–24 | 5–10 | Arriva in gruppo |
| bandito | — | 2–6 | Umano, porta oro |

> **Consiglio:** contro scheletri e zombi le armi contundenti (mazze) sono le più efficaci. Gli scheletri sono immuni al veleno: non sprecare pozioni o armi avvelenate.

### Livello 2 – Primi pericoli (Fama 1000–3000)

| Creatura | PF | Danno | Note |
|---|---|---|---|
| orco | 58–72 | 5–7 | Fruga i cadaveri |
| capitano orco | 67–87 | 5–15 | — |
| uomo ratto | 58–72 | 4–5 | Fruga i cadaveri |
| uomo lucertola | 70–120 | 5–12 | — |
| arpia | 58–72 | 5–7 | Volante |
| gnomo | 67–87 | 5–15 | — |
| **mago gnomo** (gnome magician) | 70–90 | 4–14 | Magia 60: **molto pericoloso** per i principianti |
| mago orco | 70–90 | 4–14 | Magia 60 |
| ghoul | 46–60 | 7–9 | Usa mosse speciali |
| scorpione | 50–63 | 5–10 | Veleno superiore |
| corpser | 94–108 | 10–23 | Pianta immobile: colpisce forte da vicino |
| banshee urlante | 76–90 | 10–14 | Res Fr 70–80 |
| ettin | 82–99 | 7–17 | Res Fr 40–50 |
| ogre | 100–117 | 9–11 | — |
| diavoletto (imp) | 55–70 | 10–14 | Magia 90 |
| fantasma (ghost) | — | 10–23 | Immune al veleno |
| orso nero / grizzly | 60–133 | 8–19 | Addomesticabili (55–69) |
| lupo feroce | 58–112 | 11–20 | Addomesticabile (89) |
| giaguaro | 64–188 | 8–25 | Ottimo primo animale da combattimento (71) |
| leopardo delle nevi / orso polare | 34–154 | 3–18 | Nelle Frozen Isles; danno pelle ghiacciata |
| gatto infernale | 48–167 | 6–18 | Soffio di fuoco; Res Fu 80–90 |

> **Consiglio:** la guida originale lo ripete per ogni classe: **attento ai maghi gnomi nei dungeon facili**. Colpiscono da lontano con incantesimi che un personaggio nuovo non regge. Avvicinati di corsa o tieni una pozione di cura in mano.

### Livello 3 – Avversari seri (Fama 3000–6000)

| Creatura | PF | Danno | Note |
|---|---|---|---|
| troll / troll marino | 106–123 | 8–14 | Fruga i cadaveri |
| troll del gelo | 140–156 | 14–20 | Res Fr 40–50 |
| troll di palude | 206–223 | 12–20 | Veleno |
| gargoyle | 88–105 | 7–14 | Magia 70 |
| gargoyle di pietra | 148–165 | 11–17 | Res Fi 45–55 |
| gargoyle di fuoco | 211–240 | 7–14 | Soffio di fuoco |
| elementale della terra | 76–93 | 9–16 | Res Fi 30–35 |
| elementali dell'aria / del fuoco / dell'acqua | 76–93 | 7–10 | Magia 60; resistenti al proprio elemento (fuoco 60–80, veleno 60–70...) |
| spettro (wraith, spectre, shade) | 46–60 | 7–11 | Magia 55; immuni al veleno |
| mummia | 208–222 | 13–23 | Res Fr 50–60; usa mosse speciali |
| cavaliere d'ossa / scheletrico | 118–150 | 8–18 | Res Fr 50–60 |
| mago scheletrico | 46–60 | 3–7 | Magia 60 |
| minotauro | 100–117 | 9–11 | Mosse speciali |
| ciclope | 202–231 | 7–23 | Lancia massi |
| fuoco fatuo (wisp) | 118–135 | 17–18 | Res En 50–70 |
| guerriero terathan | 100–129 | 7–17 | — |
| guerriero ophidian | 128–155 | 5–11 | — |
| lucertola di lava | 76–190 | 6–24 | Soffio di fuoco |
| serpente gigante | 112–129 | 7–17 | Veleno; Res Ve 70–90 |
| **ragno del terrore** | 118–132 | 5–17 | Veleno **letale**, Magia 65 |
| giovane vampiro | 90–150 | 5–10 | Risucchia la vita |
| segugio infernale | 66–225 | 11–25 | Soffio di fuoco |
| basilisco | 106–223 | 8–19 | — |
| draco / draco marino | 241–358 | 11–25 | Soffio; addomesticabile (84–88) |
| viverna | 125–241 | 8–25 | Veleno **mortale**; Res Ve 90–100 |
| serpente marino | 110–255 | 6–14 | Soffio; si incontra in mare |
| orsogufo / orsotroll | 236–373 | 16–31 | Picchiano duro |
| draghetto (cucciolo) | 198–312 | 8–20 | Soffio |

> **Consiglio:** da questo livello serve un antidoto affidabile (Purificazione, pozioni di cura del veleno o Guarigione alta). Viverne e ragni del terrore avvelenano a livelli che uccidono un personaggio senza cure in pochi secondi.

### Livello 4 – Pericolosi (Fama 6000–12.500)

| Creatura | PF | Danno | Note |
|---|---|---|---|
| mago uomo ratto / arciere uomo ratto | 88–108 | 4–14 | Magia 70 / attacchi a distanza |
| centauro | 130–172 | 13–24 | Res alte (Ve 45–55) |
| lich | 180–250 | 24–26 | Magia 70; immune al veleno |
| demone (demon) | 186–203 | 10–15 | Magia 70 |
| efreet | 196–213 | 11–13 | Res Fu 60–70 |
| mind flayer | 81–105 | 5–10 | Magia **95**: devasta i maghi |
| signore degli ogre / ogre artico | 202–231 | 7–23 | Immune al veleno normale |
| giganti (collina, pietra, fuoco) | 322–351 | 16–23 | Lanciano massi; res al loro elemento |
| gigante del gelo | 622–851 | 16–53 | Res Fr 70–80 |
| titano / gigante delle tempeste | 322–351 | 13–16 | Magia 85 |
| elementale del veleno | 256–309 | 12–18 | Veleno **letale**; immune al veleno |
| elementale del sangue | 316–369 | 17–27 | Magia 85 |
| beholder / gazer anziano | 178–195 | 8–19 | Magia 90; Res Fu 60–70 |
| kraken | 454–468 | 19–33 | In mare |
| signore vampiro | 300–750 | 10–25 | Magia 95; risucchio vitale |
| viverna mutata (wyvra) | 378–395 | 12–18 | Veleno mortale |

### Livello 5 – Mortali e boss (Fama oltre 12.500)

| Creatura | PF | Danno | Note |
|---|---|---|---|
| idra | 478–495 | 16–22 | Soffio; dissolve le evocazioni |
| drago (tutte le varianti) | 478–495 | 16–22 | Soffio elementale; veleno; dissolve le evocazioni |
| draco anziano | 641–858 | 22–45 | Soffio; mosse speciali |
| demone maggiore (daemon) / fiend | 286–303 | 7–14 | Varianti elementali; Magia 70 |
| incubo maggiore (nightmare) | 298–415 | 16–29 | Magia 99; addomesticabile (alto) |
| unicorno | 298–315 | 16–22 | Magia 95; immune al veleno |
| medusa | 212–253 | 10–20 | Res Fi 80–90 |
| fenice | 340–383 | 25 | Magia 95 |
| signore lich | 450–503 | 11–33 | Magia 90 |
| lich antico | 560–595 | 15–50 | Magia **120**; non provocabile |
| succube | 312–353 | 18–28 | Magia 99; risucchio vitale; Res Fi 80–90 |
| wyrm d'ombra / wyrm antico | 558–1011 | 29–45 | Soffio, veleno mortale, dissolve le evocazioni |
| drago scheletrico / dracolich | 558–599 | 29–35 | Immune al veleno; Res Fi 75–80 |
| grandi draghi (Vuoto, Radiazioni, Cristallo, Anziano, Vulcanico) | 558–711 | 23–35 | Dragon's Maw e dungeon finali |
| diavolo | 592–711 | 22–29 | Immune al veleno; Magia 95 |
| **balron** | 750–1250 | 35–55 | Aura di danno; varianti elementali; Magia 95 |
| Re dei Draghi, Caddellite Dragon | 1500–1900 | 22–35 | Boss unici (prove del Signore del Tempo) |

> **Consiglio:** per i mostri di questo livello controlla il **tipo di danno** e adegua le resistenze. Un drago di zaffiro colpisce col freddo, uno d'ottone con l'energia: la stessa armatura che ti salva da uno può farti uccidere dall'altro. Leggi il nome del drago prima di attaccare.

> **Consiglio:** le creature che "dissolvono le evocazioni" (draghi, idre, wyrm, dracolich) eliminano le tue creature evocate. Contro di loro conviene un animale domestico vero o un gruppo.

### Consigli di sopravvivenza generali

1. **Banca prima di tutto**: tieni in banca un "kit di emergenza" (armi, armatura, pozioni, bende, piccone, accetta) per ricominciare se perdi tutto.
2. **Prima di entrare in un dungeon**: bende, pozioni di cura e di purificazione, un modo per uscire (rune di richiamo, pergamene) e Rivelare Nascosti a portata di tasto.
3. **Il messaggio d'ingresso** ("Easy", "Normal", "Difficult", "Challenging", "Hard", "Deadly") è il tuo semaforo: se leggi "Hard" o "Deadly" e non sei preparato, torna indietro.
4. **I mercanti cambiano idea**: se nessuno compra il tuo bottino, riprova più tardi o in un'altra città.
5. **Non tutto ciò che leggi è vero**: libri, appunti e PNG possono mentire di proposito. Incrocia gli indizi prima di partire per una spedizione lunga.
