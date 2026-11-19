# 2. Creazione del personaggio e primo giorno

Questo capitolo ti accompagna dalla creazione del personaggio alla fine del tuo primo giorno di gioco. Se è la tua prima volta, leggilo tutto.

## 2.1 Creare il personaggio

Nella schermata di creazione del client scegli **aspetto**, **caratteristiche** e **abilità iniziali**. Tutti i personaggi sono umani.

### Caratteristiche (stats)

Hai **80 punti** (90 con client dalla versione 7.0.16) da distribuire tra **Forza** (Strength, For), **Destrezza** (Dexterity, Des) e **Intelligenza** (Intelligence, Int). Ogni caratteristica deve stare tra **10** e **60**. Se il client invia valori non validi, il server li azzera tutti a 10/10/10.

| Caratteristica | A cosa serve |
|---|---|
| Forza | Punti Ferita (PF = For × 1,25), peso trasportabile (100 + 4 × For), danni in mischia |
| Destrezza | Vigore (Vig = Des × 1,25), velocità d'attacco |
| Intelligenza | Mana (Int × 1,25), potenza degli incantesimi, **numero di seguaci** (1 ogni 20 punti di Int, da un minimo di 2 a un massimo di 6) |

Non preoccuparti troppo della scelta iniziale: le caratteristiche crescono usando le abilità (vedi [4.1](04-meccaniche.md#41-caratteristiche)). Ricorda solo che un personaggio troppo debole fisicamente muore facilmente e porta poco peso.

### Abilità iniziali

Puoi usare un modello già pronto oppure scegliere da te le abilità:

| Modello | Abilità |
|---|---|
| Guerriero (Warrior) | Guarigione 50, Scherma con Spada 50 |
| Mago (Magician) | Magia 50, Meditazione 50 |
| Fabbro (Blacksmith) | Estrazione Mineraria 50, Fabbro 50 |
| Negromante (Necromancer) | Negromanzia 50, Parlare con gli Spiriti 50 |
| Paladino (Paladin) | Cavalleria 51, Scherma con Spada 49 |
| Samurai | Bushido 50, Scherma con Spada 50 |
| Ninja | Ninjitsu 50, Nascondersi 50 |

Nella scelta personalizzata ("Advanced") puoi mettere **al massimo 50 punti per abilità**, con un totale di 100 o 120 punti a seconda del client, cioè 2 o 3 abilità. Alcune abilità non si possono scegliere all'inizio: Furtività (Stealth, tranne per il Ninja), Disinnescare Trappole (Remove Trap) e Tessitura Arcana (Spellweaving).

> **Consiglio:** la scelta iniziale **non ti vincola**. In questo gioco puoi imparare quasi tutte le abilità, e i primi punti di molte abilità si comprano dai PNG con poche monete. Conviene partire con abilità che costano fatica da alzare, o che ti servono subito per sopravvivere. Ecco alcune combinazioni collaudate:
> - **combattente**: un'arma + Guarigione (Healing);
> - **mago**: Magia (Magery) + Alchimia (Alchemy) oppure Tiro con l'Arco (Archery);
> - **domatore**: Addestrare Animali (Animal Taming) + Veterinaria (Veterinary), con molta Intelligenza per avere più seguaci;
> - **artigiano**: Fabbro (Blacksmithy) oppure Sartoria (Tailoring) + Alchimia.

### Cosa trovi nello zaino

Ogni nuovo personaggio parte con:

- **5.000 monete d'oro**;
- un libro, *Lore Guide to Adventure*, da leggere;
- un **pugnale** (dagger), un paio di **forbici** (scissors) e una **torcia**;
- 2 pagnotte e un otre d'acqua (waterskin);
- **2 libri di studio** (study book) casuali, che ti fanno allenare un'abilità mentre sei disconnesso (vedi [2.4](#24-allenare-le-prime-abilità));
- vestiti e scarpe.

Nella tua **cassetta di sicurezza** (bank box) trovi anche il **Character Database**: è l'oggetto in cui il gioco registra i tuoi progressi. Non buttarlo. Se sparisce, il gioco te ne dà uno nuovo al successivo accesso.

## 2.2 L'introduzione: la cella e le tre domande

Appena creato, il personaggio si trova in una stanza su Serpent Island. Cammina sulla **luce** per entrare nella cella dove si svolge la scena d'apertura. Se c'è già un altro nuovo giocatore, compare il messaggio "Something is blocking the light": aspetta il tuo turno.

Nella cella assisti a una breve scenetta. Lo stregone Peter Grimm sta tentando l'ennesimo "exploit" per avere oro infinito, ma viene interrotto da **Final**, che poi si rivolge a te e ti pone **tre domande**.

Per rispondere puoi:

- cliccare i **pulsanti** della finestra che compare: **Sì / No / Ripeti / Salta**;
- **scrivere** la risposta in chat, in italiano o in inglese: "sì", "certo", "va bene", "giuro", "yes", "aye"… oppure "no", "mai", "rifiuto", "nay"…;
- dire **"ripeti"** (repeat) per riascoltare la domanda;
- dire **"salta"** (skip) per accelerare i dialoghi.

> **Attenzione:** se non rispondi per troppo tempo (circa 10 solleciti), vieni rimandato nella stanza iniziale. Poi puoi rientrare sulla luce e ricominciare.

### Domanda 1 – Il giuramento (Community Conduct)

*"Prometti di mantenere pulito questo mondo e di rispettare gli altri avventurieri?"*

| Risposta | Conseguenza |
|---|---|
| **Sì** | L'introduzione continua. |
| **No** | L'introduzione finisce e torni nella stanza iniziale. Non puoi entrare nel mondo finché non accetti. |

### Domanda 2 – Avatar della Bilancia

*"Vuoi diventare un Avatar della Bilancia?"*

Questa è la scelta più importante. Determina la "modalità" del personaggio:

| | **Avatar** (rispondi Sì) | **Non Avatar** (rispondi No) |
|---|---|---|
| Tetto delle caratteristiche | **250** | **225** |
| Velocità di apprendimento delle abilità | normale (×1,75) | uguale all'Avatar |
| Influenza sulla Bilancia | sì: uccisioni, missioni e morti spostano l'equilibrio | no, o quasi nulla |
| Fama e karma | guadagno pieno | guadagno ×0,75; perdite normali |
| Danni | tetti di danno più alti (armi 200 contro 150, incantesimi 250 contro 200) | tetti più bassi |
| Fortuna e raccolta | Fortuna ×1,15, raccolta di risorse ×1,15 | Fortuna ×0,75 |
| Morte | perdita di fama/karma, più una **piccola perdita casuale di abilità e caratteristiche** | stessa perdita di fama/karma, **ma mai** perdita di abilità o caratteristiche |
| Maledizione di viaggio | perdita di fama/karma, più un possibile calo delle abilità | solo perdita di fama/karma |
| Tributo per la resurrezione dal guaritore | normale | **doppio** |
| Possibilità di giurare per l'Ordine o il Caos | sì | no |

> **Consiglio:** per un primo personaggio vanno bene entrambe le scelte. Il **non Avatar** è più rilassato: alla morte non perdi mai abilità né caratteristiche, e impari alla stessa velocità. L'**Avatar** è più potente a lungo termine (250 punti di caratteristiche, più danni, più fortuna) e partecipa alla lotta tra Bene e Male, ma paga di più ogni errore. La perdita per singola morte è comunque piccola: al massimo intorno al 2% di alcune abilità.

### Domanda 3 – Anima legata (SoulBound)

*"Vuoi legare la tua anima al mondo?"* (Final stesso avverte: *non consigliato ai nuovi avventurieri*).

| Risposta | Conseguenza |
|---|---|
| **No** (consigliato) | Personaggio normale. L'introduzione termina e vieni trasportato a **Britain**, in Sosaria. |
| **Sì** | Diventi **SoulBound**, la modalità "hardcore". La morte è **permanente**: a ogni morte il personaggio viene azzerato e riparte come una persona nuova. Ricevi un **Soul Tome**, con cui puoi assorbire nel tuo corpo le proprietà degli oggetti, e sei automaticamente Avatar (tetto 250). Non hai la protezione del principiante. |

Al termine Final pronuncia *"An Vam Trav"* e ti teletrasporta a **Britain**, nel cuore di Sosaria.

## 2.3 Il primo giorno, passo per passo

Sei appena arrivato a Britain con 5.000 monete d'oro. Ecco un percorso semplice e sicuro.

### Passo 1 – Orientati

1. Premi **Ctrl+Shift** (con il client ClassicUO/TazUO): compaiono i nomi di tutti gli oggetti a terra. È utilissimo per capire con cosa puoi interagire.
2. Apri la **mappa** del client e il pulsante **Help** del paperdoll, cioè la guida di gioco con molte funzioni utili (vedi [2.5](#25-linterfaccia-del-client)).
3. Digita `[principiante` per vedere quanto ti resta di **protezione del principiante** (vedi [2.6](#protezione-del-principiante)).

### Passo 2 – Metti al sicuro l'oro

Cerca la **banca** e di' **"banca"** (bank) vicino al banchiere. Si apre la cassetta di sicurezza: trascinaci dentro l'oro che non ti serve subito. Puoi anche lasciare l'oro direttamente sul banchiere per depositarlo.

> **Consiglio:** tieni nello zaino solo quello che stai per spendere. Se muori, ciò che porti addosso resta sul tuo cadavere, da cui i mostri e gli altri possono prenderlo. Finché sei protetto come principiante, gli oggetti restano invece nel tuo zaino. Il tributo per la resurrezione si paga **dalla banca**.

### Passo 3 – Attiva i libri di studio

Fai doppio clic su uno dei due **libri di studio** dello zaino. Al prossimo logout il personaggio studierà quell'abilità mentre sei disconnesso:

- fa un tentativo di guadagno ogni **10 minuti** offline, fino a un massimo di **20 punti** per sessione;
- funziona solo fino a **70** per i libri normali (100 per gli avanzati, 120 per i leggendari);
- devi riattivarlo a **ogni** accesso, perché si legge un libro alla volta;
- dopo ogni sessione c'è un 8% di probabilità che il libro si sbricioli;
- se hai studiato almeno 5 ore, al rientro impari quell'abilità più in fretta per 30 minuti.

### Passo 4 – Compra le prime abilità

Quasi tutti i mercanti (vendor) insegnano le loro abilità. Hai due modi per farlo:

- **Menu contestuale.** Fai clic destro sul mercante e scegli "Teach…" (o la voce equivalente) sull'abilità che vuoi.
- **A voce.** Di' "**addestra**" (o "insegna", "train") per sentire l'elenco delle abilità che insegna. Poi di' il nome dell'abilità per conoscere il prezzo.

Poi **lascia l'oro sul mercante**. Le regole sono queste:

- il prezzo è **10 monete d'oro per punto** di abilità (1 moneta per 0,1);
- il mercante ti insegna fino a metà della sua abilità, mai oltre **50**. In pratica, con mercanti normali, arrivi intorno a **45–50**;
- se paghi meno, impari meno. Se paghi troppo, ti restituisce il resto;
- non puoi comprare abilità se sei criminale, se l'abilità è bloccata in discesa, o per Misticismo, Tessitura Arcana, Lancio e Infusione. Per Furtività ti serve Nascondersi ad almeno 50.

Con circa **300 monete d'oro** porti un'abilità da 0 a 30. Abilità utili a tutti:

| Abilità | Perché |
|---|---|
| **Guarigione** (Healing) + **Anatomia** (Anatomy) | Curarsi con le bende senza usare mana |
| **Magia** (Magery) | Cura, Purificazione, Protezione, Disinnesco Magico. Utile come riserva anche a chi combatte |
| **Alchimia** (Alchemy) | Pozioni di cura, di guarigione dal veleno ed esplosive |
| **Addestrare Animali** (Animal Taming) + **Conoscenza Animali** (Animal Lore) | Una cavalcatura gratis e alleati in combattimento |
| **Accampamento** (Camping) | La migliore abilità per far crescere le caratteristiche. Riduce fame e sete e dà accesso alle tende. **Non comprarla**: sale molto in fretta da sola |
| **Rivelare Nascosti** (Detecting Hidden) | Trovare trappole e forzieri nascosti nei dungeon |

> **Attenzione:** se compri un'abilità da un PNG la cui abilità è bassa (sotto 60), lui non te la insegna. Prova con un altro mercante dello stesso mestiere.

### Passo 5 – Fai la spesa

Con il resto dell'oro compra:

- **bende** (bandages): oppure compra un rotolo di stoffa (bolt of cloth) e tagliala con le forbici che hai già;
- **pozioni** di Cura (Heal) e di Purificazione (Cure) dal veleno, almeno 5–10 per tipo;
- un'**armatura leggera** adatta alla tua Forza: il cuoio (leather) va bene per tutti;
- un'**arma** adatta alla tua abilità di combattimento;
- cibo e acqua di riserva;
- se fai magia: un **libro degli incantesimi** (spellbook), le pergamene dei primi cerchi e i **reagenti**.

### Passo 6 – Fai crescere le caratteristiche fin da subito

Ogni caratteristica può salire **al massimo di un punto ogni 10 minuti**, contati separatamente per For, Des e Int. Per questo conviene iniziare subito, anche mentre fai altro:

1. Impugna il **pugnale**, fai doppio clic su di esso e seleziona un **albero**: ottieni legnetti (kindling).
2. Fai doppio clic sui legnetti per **accendere un fuoco**. Così usi Accampamento, che fa salire tutte e tre le caratteristiche (vedi la tabella in [4.1](04-meccaniche.md#41-caratteristiche)).
3. Quando hai guadagnato un punto per caratteristica, riprova dopo 10 minuti.

### Passo 7 – Guadagna i primi soldi

Metodi tranquilli per un principiante:

- **Bacheca delle missioni** (quest board) nelle taverne: incarichi "uccidi" o "trova" un oggetto in un dungeon, pagati in oro. Vedi [4.20](04-meccaniche.md#420-missioni-e-taglie).
- **Raccolta e artigianato**: estrai minerale grezzo, fondi i lingotti e crea armature da vendere ai fabbri. Oppure scuoia animali e vendi il cuoio lavorato.
- **Vendere il bottino**: gioielli, gemme, armi e armature con proprietà magiche si vendono bene. Di' "**mercante vendi**" (vendor sell) vicino al mercante giusto.
- **Monete antiche**: rame, argento, gemme e pepite si convertono in oro se le metti nella cassetta di sicurezza e ci fai doppio clic sopra.
- **Domare e vendere animali** al mediatore di animali (animal broker).

### Passo 8 – La prima uscita

1. Esci dalla città lungo le strade. Sono abbastanza sicure, ma possono capitare **incontri casuali**.
2. Allenati su animali e mostri deboli: conigli, cervi, lupi, scheletri vicino ai cimiteri.
3. Quando ti senti pronto, entra in un dungeon di livello **Facile** ("Easy") di Sosaria. Il messaggio di ingresso in un'area pericolosa ne indica la difficoltà: Easy, Normal, Difficult, Challenging, Hard, Deadly. L'elenco completo è nel [capitolo 13](13-mondo-e-dungeon.md).
4. Torna in città per curarti, rifornirti e mettere in banca il bottino.

### Errori da evitare

| Errore | Perché è un problema |
|---|---|
| Girare con tutto l'oro addosso | Se muori resta sul cadavere |
| Attaccare PNG blu o innocenti, o saccheggiare cadaveri altrui | Diventi **criminale** (se qualcuno ti vede) e le guardie ti attaccano. Vedi [4.6](04-meccaniche.md#46-notorietà-karma-fama-criminali-assassini) |
| Dimenticare cibo e acqua | Con fame o sete a zero perdi PF, Vigore e Mana, e impari più lentamente |
| Usare Negromanzia, Accattonaggio o Furto a cuor leggero | Fanno scendere il karma: sotto −5.000 le guardie ti attaccano |
| Viaggiare continuamente con Richiamo/Portale tra mappe diverse | Rischi la **maledizione di viaggio** (dopo la protezione del principiante) |
| Pagare troppo un PNG per un'abilità | Non è grave: ti restituisce il resto. Ma non insegna oltre il suo limite |
| Bere pozioni sconosciute o leggere pergamene non identificate a PF bassi | Possono avvelenarti o esplodere |
| Ignorare le trappole nei dungeon | Possono farti perdere oggetti, oro e caratteristiche, o teletrasportarti altrove. Usa Rivelare Nascosti, l'**orbe** di Disinnesco Magico o il **palo da 10 piedi** |

## 2.4 Allenare le prime abilità

- **Le abilità salgono usandole.** Più l'abilità è bassa, più è facile guadagnare: fino a 25 punti ogni successo dà 0,3–0,5 punti. Sopra 80 ogni guadagno vale 0,1 e diventa via via più raro.
- **Sotto 10 punti** ogni tentativo fa guadagnare qualcosa.
- **I dungeon danno ispirazione.** Nei dungeon le abilità di combattimento, di magia, da bardo e da ladro salgono più in fretta: fino a +80% nei dungeon più pericolosi. Le abilità di conoscenza e di utilità salgono fino a +27%. Compare il messaggio *"You gain inspiration from training here"*.
- **La fame conta.** Se sei ben nutrito (fame tra 21 e 40) impari fino al doppio più in fretta. Se stai morendo di fame (fame ≤ 3) impari molto più lentamente.
- **Le gilde dei PNG aiutano.** Le abilità della tua gilda salgono più in fretta.
- **I libri di studio** allenano mentre sei disconnesso (vedi il passo 3).
- **Tetto delle abilità.** Il totale massimo è di 2.500 punti e ogni abilità arriva a 100, o di più con le pergamene del potere (power scroll). Quando sei al tetto, per salire in un'abilità devi metterne un'altra in discesa con le frecce della finestra delle abilità.

> **Consiglio:** se curi le ferite con le **bende** appena perdi qualche PF, alleni Guarigione e Anatomia gratis. Allo stesso modo, prova a usare Rivelare Nascosti, Identificare Oggetti e Valutare Intelligenza su tutto quello che incontri.

## 2.5 L'interfaccia del client

Ultima Adventures si gioca con **ClassicUO** o **TazUO**. La cartella `Client/Documentation` contiene le immagini di configurazione dei launcher.

### Le finestre principali

| Finestra | Come aprirla | A cosa serve |
|---|---|---|
| **Paperdoll** | Doppio clic su te stesso, o Alt+P | Equipaggiamento. Contiene i pulsanti Help, Options, Skills, Guild, Peace/War, Status |
| **Barra di stato** (status) | Pulsante Status, o il comando `[status` | PF, Mana, Vigore, caratteristiche (con le frecce di blocco), oro, peso, resistenze. `[status` apre una scheda dettagliata con la Bilancia e il costo della tua resurrezione |
| **Abilità** (skills) | Pulsante Skills | Elenco delle abilità con le frecce **su / giù / bloccata** |
| **Zaino** (backpack) | Doppio clic sullo zaino nel paperdoll | Inventario |
| **Guida di gioco** | Pulsante **Help** del paperdoll | Menu personalizzato: Quick Bar, ricerca del portale lunare (Moongate Search), ricerca del cadavere (Corpse Search), opzioni del bottino (Loot Options), elenco delle abilità, stili di gioco, musica, barre degli incantesimi, missioni (Quests), MOTD, chat, emote, segnalazione bug |
| **Quick Bar** | `[quickbar` | Barra verticale con le funzioni più comuni |

### Opzioni consigliate del client (Options)

- **Generale:** corsa automatica e apertura automatica delle porte.
- **Mobiles:** mostra i PF sopra i personaggi.
- **Gumps:** barre della salute personalizzate; salva le barre al logout.
- **Combattimento/Incantesimi:** lancia gli incantesimi con un clic.
- **Contenitori:** contenitori grandi e bottino a griglia (grid loot).
- Abbassa volume e musica: di default sono molto forti.

> **Attenzione:** l'opzione "apri automaticamente i cadaveri" ti fa aprire anche i cadaveri altrui. Se prendi qualcosa dal cadavere di un innocente davanti a testimoni, diventi **criminale**.

### Macro utili

Nelle opzioni del client crea delle macro di tipo **Say** (parla) con queste frasi. Puoi scriverle in italiano o in inglese:

| Macro | Effetto |
|---|---|
| "mercante compra" / "vendor buy" | Apre la lista d'acquisto del mercante vicino |
| "mercante vendi" / "vendor sell" | Apre la lista di vendita |
| "banca" / "bank" | Apre la cassetta di sicurezza |
| "tutti seguitemi" / "all follow me" | Tutti i tuoi animali ti seguono |
| "tutti proteggetemi" / "all guard me" | Tutti i tuoi animali ti proteggono |
| "tutti attaccate" / "all kill" | Tutti i tuoi animali attaccano il bersaglio che scegli |
| "tutti restate" / "all stay" | Tutti i tuoi animali restano fermi dove sono |
| `[bandself` | Ti bendi da solo |
| `[bandother` | Bendi qualcun altro |

Molto utili anche le macro del client per *Usa abilità* (Rivelare Nascosti, Nascondersi), *Lancia incantesimo*, *Ultimo bersaglio* e *Bersaglio se stesso*.

## 2.6 Consigli per principianti

- **Leggi tutto.** Libri, pergamene e cartelli contengono indizi su missioni, terre nascoste e sistemi di gioco. In Sarth Abbey c'è una biblioteca.
- **Parla con i PNG.** Molti rispondono a parole chiave e raccontano voci utili, alcune false!
- **Usa la Quick Bar e la guida Help.** Raccolgono quasi tutti i comandi in pulsanti.
- **Tieni una "scorta di emergenza" in banca**: un'arma, un'armatura, pozioni, bende, un piccone, un'ascia. Ti servono se perdi tutto.
- **Unisciti a una gilda di PNG.** Costa 2.000 monete d'oro, ti dà un anello che potenzia alcune abilità e prezzi migliori dai mercanti del tuo mestiere. Vedi [4.16](04-meccaniche.md#416-gilde).
- **Una cavalcatura cambia la vita**, ma nei dungeon e nelle caverne la cavalcatura viene messa al sicuro automaticamente. Ti aspetta all'uscita.
- **Non avere fretta.** Il gioco è pensato per essere giocato senza macro ossessive: offre libri di studio, guadagni più rapidi nei dungeon e botteghe artigiane.

<a id="protezione-del-principiante"></a>
## Protezione del principiante

Ogni nuovo personaggio parte sotto la **protezione del principiante**. È un periodo di tranquillità per imparare il gioco senza perdere i progressi per un errore.

### Quanto dura

La protezione finisce da sola alla **prima** di queste due soglie:

- **20 ore di gioco** del personaggio. Contano solo le ore in cui sei connesso;
- **300 punti abilità** in totale, sommando tutte le abilità. Contano anche i 100–120 punti iniziali.

Quando finisce, ricevi un messaggio. Da quel momento morte, trappole e viaggi magici comportano i rischi normali.

> **Attenzione:** i personaggi **SoulBound** (anima legata) non hanno mai questa protezione.

### Cosa ti protegge

| Situazione | Con la protezione | Senza la protezione |
|---|---|---|
| **Morte – oggetti** | Restano **nel tuo zaino**: al ritorno in vita hai ancora tutto | Restano sul cadavere |
| **Morte – penalità** | **Nessuna** perdita di abilità, caratteristiche, fama o karma | Perdita di fama/karma ed eventuale perdita di abilità (vedi [4.9](04-meccaniche.md#49-morte-spiriti-e-resurrezione)) |
| **Resurrezione immediata** ("pregare gli dei") | Di nuovo disponibile dopo **10 minuti** | Di nuovo disponibile dopo 60 minuti |
| **Trappole del pavimento** | Quelle che rubano, distruggono o trasformano i tuoi oggetti sono **innocue** | Effetto pieno |
| **Trappole dei forzieri** | **Disinnescate** senza danni quando apri il forziere | Esplosioni, dardi, veleno, magia |
| **Viaggio magico** (Richiamo, Portale, ecc.) | **Nessuna** maledizione di viaggio | Possibile perdita di fama/karma (e di abilità per l'Avatar) |
| **Incontri casuali** (agguati lungo le strade e nelle terre selvagge) | **Non** ti capitano | Possono capitare ovunque fuori dalle zone sicure |
| **Fame e sete** | Calano a **metà velocità** | Velocità normale |

La protezione **non** ti rende invulnerabile: puoi comunque morire in combattimento, essere avvelenato, subire danni dalle trappole che feriscono e così via. Ti evita "solo" le conseguenze pesanti.

### Il comando `[principiante`

Scrivi `[principiante` (oppure gli alias `[novizio` o `[beginner`) per aprire una finestra che mostra:

- se la protezione è attiva;
- quante **ore** e quanti **punti abilità** ti mancano alla scadenza;
- l'elenco delle protezioni.

Dalla stessa finestra puoi **rinunciare** alla protezione se ti senti pronto. Il gioco ti chiede conferma ("Sì, rinuncio"). **La scelta è definitiva**: non si può riattivare.

> **Consiglio:** sfrutta queste prime ore per esplorare, provare i dungeon facili, imparare a viaggiare con i portali lunari e capire quali abilità ti piacciono. Nel frattempo metti da parte una scorta di oro in banca: dopo la protezione ti servirà per i tributi di resurrezione.
