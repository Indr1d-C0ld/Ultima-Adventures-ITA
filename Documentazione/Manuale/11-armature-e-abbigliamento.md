# 11. Armature, scudi, gioielli e vestiario

Questo capitolo descrive tutto ciò che indossi per proteggerti: armature, elmi, scudi, gioielli e abiti. Spiega come funzionano le resistenze, perché alcune armature ti impediscono di meditare o di muoverti in furtività e quali materiali conviene usare. Tutti i valori vengono dal codice del server.

---

## 11.1 Come funziona la protezione

### Le cinque resistenze

Ogni pezzo di armatura ha cinque **resistenze**: **fisica**, **fuoco**, **freddo**, **veleno**, **energia**. Le resistenze dei pezzi indossati si sommano e riducono della stessa percentuale il danno di quel tipo che ricevi, da armi, incantesimi, soffi e trappole.

- Il **tetto** per un giocatore è **70** per ogni resistenza.
- La Maledizione (*Curse*) abbassa il tetto delle resistenze non fisiche a 60 finché dura.
- La **Pelle di Pietra** della Ricerca arcana alza il tetto fisico a 90.
- Gli elfi hanno +5 al tetto dell'energia.
- L'**Armatura Reattiva** e la **Protezione** della Magia, alcune pozioni e molte proprietà magiche aggiungono resistenza.

La resistenza di un pezzo è data da:

```
resistenza = valore base del pezzo
           + bonus del materiale (11.3)
           + bonus di "protezione" delle vecchie armature magiche (da +0 a +4)
           + resistenze aggiunte (artigianato eccezionale, attrezzi runici, magia)
```

> **Consiglio:** all'inizio punta soprattutto sulla **resistenza fisica**: quasi tutti i mostri dei primi dungeon fanno danno fisico. Poi bilancia fuoco (draghi, demoni, elementali del fuoco), veleno (ragni, serpenti, melme) ed energia (maghi, elementali dell'aria).

### Forza richiesta e durabilità

- Per indossare un pezzo ti serve la **Forza** indicata (colonna **For**). La proprietà *Lower Requirements* e alcuni materiali (oro, acciaio, ottone, mithril, xormite, legni pregiati) la riducono.
- La **durabilità** (colonna **Durab.**, punti iniziali minimo–massimo) scende quando vieni colpito: a ogni colpo il gioco sceglie a caso un pezzo che hai addosso (armi, abiti esterni, armature, abiti interni, gioielli). A zero il pezzo si distrugge. Fai riparare dal fabbro (metallo), dal sarto o dal conciatore (cuoio, stoffa), dal carpentiere (legno), dal gioielliere o dal meccanico (gioielli).

### Meditazione e armature

La colonna **Med.** indica se il pezzo permette di meditare:

| Valore | Significato |
|---|---|
| **sì** | Non ostacola la meditazione (cuoio, pellicce, occhiali, armature di pelle) |
| **metà** | Penalità dimezzata (borchiato) |
| **no** | Blocca la meditazione (osso, anelli, maglia, piastre, drago, legno...) |

In questo shard la regola è severa: **basta un solo pezzo che non permette di meditare per azzerare completamente il bonus di Meditazione** alla rigenerazione del mana, e l'abilità attiva Meditazione fallisce con il messaggio "le forze rigeneratrici non attraversano la tua armatura". Fanno eccezione i pezzi che hanno la proprietà **Mage Armor** ("armatura del mago") o **Spell Channeling**: per la meditazione contano come se non ci fossero. Gli **scudi** non contano. Restano invece la rigenerazione da Concentrazione e la proprietà *Mana Regeneration* degli oggetti.

> **Consiglio:** un mago che vuole un po' di protezione deve vestirsi di **cuoio** o cercare pezzi con **Mage Armor**. Anche un solo elmo di piastre senza Mage Armor gli toglie tutto il bonus di Meditazione e rallenta molto il recupero del mana.

### Furtività e armature

Per usare **Furtività** (*Stealth*) il gioco somma un "peso" per ogni pezzo di armatura indossato (colonna **Furt.**). Se la somma arriva a **42** o più, non puoi muoverti in furtività ("non puoi sperare di muoverti in silenzio con tutta questa armatura"). Sotto quella soglia, il peso rende anche più difficile la prova di abilità (ogni punto aggiunge 2 alla difficoltà).

| Materiale | Gorgiera | Guanti | Elmo | Maniche | Gambe | Piedi | Torso |
|---|---|---|---|---|---|---|---|
| Stoffa | 0 | 0 | 0 | 1 | 1 | 0 | 2 |
| Cuoio | 1 | 1 | 0 | 2 | 3 | 1 | 5 |
| Cuoio *spined* / *horned* | 1 | 1 | 0 | 2 | 3–4 | 1 | 5–6 |
| Cuoio *barbed*, *necrotic*, *volcanic*, *frozen* | 2 | 2 | 0 | 3 | 5 | 2 | 7 |
| Borchiato e cuoi *goliath*, *draconic*, *hellish*, *dinosaur*, *alien* | 2 | 2 | 0 | 4 | 6 | 2 | 10 |
| Osso | 0 | 5 | 10 | 10 | 18 | 5 | 30 |
| Anelli | 0 | 5 | 0 | 10 | 15 | 5 | 25 |
| Maglia | 0 | 0 | 10 | 0 | 15 | 0 | 25 |
| Piastre (anche legno) | 5 | 5 | 10 | 14 | 18 | 5 | 30 |
| Drago | 0 | 7 | 13 | 15 | 22 | 7 | 35 |

Con un set completo: cuoio 12, borchiato 24 (entrambi ammessi), maglia 50, anelli 55, osso 73, piastre 82, drago 92 (tutti vietati). Gli scudi non pesano.

> **Consiglio:** un ladro o un assassino può portare un set completo di **borchiato** più un elmo leggero e restare comunque sotto la soglia di 42. Una tunica di piastre da sola (30) più gambali di maglia (15) la supera già.

---

## 11.2 Tabelle delle armature

Legenda: **Fis/Fuo/Fre/Vel/Ene** = resistenze base fisica, fuoco, freddo, veleno, energia; **Tot** = somma; **For** = Forza richiesta; **Durab.** = durabilità iniziale; **Med.** = meditazione; **Furt.** = peso per la Furtività. I valori sono quelli **base**, prima del bonus del materiale (11.3) e delle proprietà magiche. La riga "Set completo" somma i pezzi base del set (le varianti come gonne o corpetti sostituiscono il pezzo corrispondente).

#### Cuoio (leather)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Berretto di cuoio (leather cap) | 2 | 4 | 3 | 3 | 3 | 15 | 20 | 30–40 | sì | 0 |
| Gorgiera di cuoio (leather gorget) | 3 | 4 | 3 | 3 | 3 | 16 | 20 | 30–40 | sì | 1 |
| Guanti di cuoio (leather gloves) | 4 | 6 | 6 | 6 | 6 | 28 | 20 | 30–40 | sì | 1 |
| Maniche di cuoio (leather sleeves) | 4 | 6 | 7 | 7 | 7 | 31 | 20 | 30–40 | sì | 2 |
| Tunica di cuoio (leather tunic) | 7 | 6 | 5 | 7 | 7 | 32 | 25 | 30–40 | sì | 5 |
| Gambali di cuoio (leather leggings) | 6 | 7 | 6 | 6 | 6 | 31 | 20 | 30–40 | sì | 3 |
| Corpetto di cuoio (female leather armor) | 6 | 7 | 6 | 6 | 6 | 31 | 25 | 30–40 | sì | 5 |
| Bustino di cuoio (leather bustier) | 4 | 7 | 6 | 6 | 6 | 29 | 20 | 30–40 | sì | 5 |
| Tunica di pelle grezza (hide tunic) | 6 | 7 | 6 | 6 | 6 | 31 | 25 | 30–40 | sì | 5 |
| Calzoncini di cuoio (leather shorts) | 3 | 6 | 5 | 5 | 5 | 24 | 20 | 30–40 | sì | 3 |
| Gonna di cuoio (leather skirt) | 3 | 6 | 5 | 5 | 5 | 24 | 20 | 30–40 | sì | 3 |
| Stivali di cuoio (leather boots) | 3 | 3 | 4 | 3 | 3 | 16 | 10 | 30–40 | sì | 1 |
| Stivali morbidi (soft leather boots) | 3 | 3 | 4 | 3 | 3 | 16 | 10 | 30–40 | sì | 1 |
| Stivali alti di cuoio (leather thigh boots) | 4 | 4 | 5 | 4 | 4 | 21 | 10 | 30–40 | sì | 1 |
| Scarpe di cuoio (leather shoes) | 2 | 2 | 3 | 2 | 2 | 11 | 10 | 30–40 | sì | 1 |
| Sandali di cuoio (leather sandals) | 2 | 3 | 4 | 3 | 6 | 18 | 10 | 30–40 | sì | 1 |
| Mantello di cuoio (leather cloak) | 2 | 4 | 3 | 3 | 3 | 15 | 10 | 30–40 | sì | 1 |
| Tunica lunga di cuoio (leather robe) | 3 | 4 | 4 | 4 | 4 | 19 | 15 | 30–40 | sì | 5 |
| **Set completo** (6 pezzi base) | 26 | 33 | 30 | 32 | 32 | 153 | | | | **12** |

#### Borchiato (studded)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Gorgiera borchiata (studded gorget) | 4 | 5 | 5 | 5 | 4 | 23 | 25 | 35–45 | metà | 2 |
| Guanti borchiati (studded gloves) | 5 | 7 | 7 | 7 | 7 | 33 | 25 | 35–45 | metà | 2 |
| Maniche borchiate (studded sleeves) | 6 | 8 | 9 | 9 | 9 | 41 | 25 | 35–45 | metà | 4 |
| Tunica borchiata (studded tunic) | 9 | 8 | 7 | 9 | 9 | 42 | 35 | 35–45 | metà | 10 |
| Gambali borchiati (studded leggings) | 8 | 9 | 8 | 8 | 8 | 41 | 30 | 35–45 | metà | 6 |
| Corpetto borchiato (studded armor) | 8 | 9 | 8 | 8 | 8 | 41 | 35 | 35–45 | metà | 10 |
| Bustino borchiato (studded bustier) | 6 | 8 | 9 | 9 | 9 | 41 | 35 | 35–45 | metà | 10 |
| Tunica di pelle borchiata (studded hide tunic) | 9 | 8 | 7 | 9 | 9 | 42 | 35 | 35–45 | metà | 10 |
| Gonna borchiata (studded skirt) | 5 | 8 | 7 | 7 | 6 | 33 | 30 | 35–45 | metà | 6 |
| **Set completo** (5 pezzi base) | 32 | 37 | 36 | 38 | 37 | 180 | | | | **24** |

#### Osso (bone)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo di osso (bone helmet) | 3 | 3 | 4 | 2 | 4 | 16 | 20 | 25–30 | no | 10 |
| Guanti di osso (bone gloves) | 7 | 6 | 3 | 7 | 6 | 29 | 55 | 25–30 | no | 5 |
| Maniche di osso (bone arms) | 11 | 6 | 5 | 7 | 6 | 35 | 55 | 25–30 | no | 10 |
| Corazza di osso (bone armor) | 14 | 9 | 8 | 8 | 6 | 45 | 60 | 25–30 | no | 30 |
| Gambali di osso (bone leggings) | 10 | 7 | 3 | 7 | 7 | 34 | 55 | 25–30 | no | 18 |
| Gonna di osso (bone skirt) | 10 | 7 | 3 | 7 | 7 | 34 | 55 | 25–30 | no | 18 |
| **Set completo** (5 pezzi base) | 45 | 31 | 23 | 31 | 29 | 159 | | | | **73** |

#### Anelli (ringmail)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Guanti ad anelli (ringmail gloves) | 6 | 3 | 2 | 3 | 2 | 16 | 40 | 40–50 | no | 5 |
| Maniche ad anelli (ringmail sleeves) | 9 | 5 | 4 | 5 | 4 | 27 | 40 | 40–50 | no | 10 |
| Tunica ad anelli (ringmail tunic) | 10 | 5 | 4 | 5 | 4 | 28 | 40 | 40–50 | no | 25 |
| Gambali ad anelli (ringmail leggings) | 9 | 3 | 2 | 3 | 2 | 19 | 40 | 40–50 | no | 15 |
| Gonna a bande (banded mail skirt) | 9 | 3 | 2 | 3 | 2 | 19 | 40 | 40–50 | no | 15 |
| **Set completo** (4 pezzi base) | 34 | 16 | 12 | 16 | 12 | 90 | | | | **55** |

#### Maglia (chainmail)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Cuffia di maglia (chainmail coif) | 6 | 6 | 6 | 1 | 2 | 21 | 60 | 35–60 | no | 10 |
| Tunica di maglia (chainmail tunic) | 10 | 7 | 4 | 5 | 4 | 30 | 60 | 45–60 | no | 25 |
| Gambali di maglia (chainmail leggings) | 9 | 5 | 2 | 3 | 2 | 21 | 60 | 45–60 | no | 15 |
| Gonna di metallo (metal skirt) | 8 | 4 | 4 | 1 | 2 | 19 | 60 | 45–60 | no | 15 |
| **Set completo** (3 pezzi base) | 25 | 18 | 12 | 9 | 8 | 72 | | | | **50** |

#### Piastre (platemail)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo di piastre (plate helm) | 12 | 5 | 5 | 5 | 5 | 32 | 80 | 50–65 | no | 10 |
| Gorgiera di piastre (platemail gorget) | 6 | 4 | 3 | 4 | 3 | 20 | 45 | 50–65 | no | 5 |
| Guanti di piastre (platemail gloves) | 8 | 5 | 4 | 5 | 4 | 26 | 70 | 50–65 | no | 5 |
| Maniche di piastre (platemail arms) | 12 | 7 | 6 | 7 | 6 | 38 | 80 | 50–65 | no | 14 |
| Tunica di piastre (platemail) | 15 | 8 | 7 | 8 | 7 | 45 | 95 | 50–65 | no | 30 |
| Gambali di piastre (platemail legs) | 12 | 6 | 5 | 6 | 5 | 34 | 90 | 50–65 | no | 18 |
| Corazza femminile di piastre (plate armor) | 15 | 8 | 7 | 8 | 7 | 45 | 95 | 50–65 | no | 30 |
| Gonna di piastre (platemail skirt) | 12 | 6 | 5 | 6 | 5 | 34 | 90 | 50–65 | no | 18 |
| **Set completo** (6 pezzi base) | 65 | 35 | 30 | 35 | 30 | 195 | | | | **82** |

#### Scaglie di drago (dragon scale)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo di scaglie (scalemail helm) | 3 | 3 | 3 | 3 | 3 | 15 | 75 | 55–75 | no | 13 |
| Guanti di scaglie (scalemail gloves) | 3 | 3 | 3 | 3 | 3 | 15 | 75 | 55–75 | no | 7 |
| Maniche di scaglie (scalemail arms) | 3 | 3 | 3 | 3 | 3 | 15 | 75 | 55–75 | no | 15 |
| Tunica di scaglie (scalemail tunic) | 3 | 3 | 3 | 3 | 3 | 15 | 75 | 55–75 | no | 35 |
| Gambali di scaglie (scalemail leggings) | 3 | 3 | 3 | 3 | 3 | 15 | 75 | 55–75 | no | 22 |
| **Set completo** (5 pezzi base) | 15 | 15 | 15 | 15 | 15 | 75 | | | | **92** |

#### Ossa demoniache (daemon bone)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo di osso demoniaco (daemon bone helmet) | 6 | 6 | 7 | 5 | 7 | 31 | 20 | 40–70 | no | 10 |
| Guanti di osso demoniaco (daemon bone gloves) | 6 | 6 | 7 | 5 | 7 | 31 | 55 | 40–70 | no | 5 |
| Maniche di osso demoniaco (daemon bone arms) | 6 | 6 | 7 | 5 | 7 | 31 | 55 | 40–70 | no | 10 |
| Corazza di osso demoniaco (daemon bone armor) | 6 | 6 | 7 | 5 | 7 | 31 | 60 | 40–70 | no | 30 |
| Gambali di osso demoniaco (daemon bone leggings) | 6 | 6 | 7 | 5 | 7 | 31 | 55 | 40–70 | no | 18 |
| **Set completo** (5 pezzi base) | 30 | 30 | 35 | 25 | 35 | 155 | | | | **73** |

#### Selvaggia (savage)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo selvaggio (savage helm) | 3 | 3 | 4 | 2 | 4 | 16 | 20 | 25–30 | no | 10 |
| Guanti selvaggi (savage guantlets) | 5 | 5 | 6 | 4 | 6 | 26 | 55 | 25–30 | no | 5 |
| Bracciali selvaggi (savage bracers) | 6 | 6 | 8 | 4 | 8 | 32 | 55 | 25–30 | no | 10 |
| Tunica selvaggia (savage tunic) | 8 | 7 | 8 | 6 | 8 | 37 | 60 | 25–30 | no | 30 |
| Schinieri selvaggi (savage greaves) | 6 | 6 | 7 | 2 | 8 | 29 | 55 | 25–30 | no | 18 |
| **Set completo** (5 pezzi base) | 28 | 27 | 33 | 18 | 34 | 140 | | | | **73** |

#### Reale (royal)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo reale (royal helm) | 12 | 5 | 5 | 5 | 5 | 32 | 80 | 50–65 | no | 10 |
| Gorgiera reale (royal gorget) | 6 | 4 | 3 | 4 | 3 | 20 | 45 | 50–65 | no | 5 |
| Bracciali reali (royal bracers) | 8 | 5 | 4 | 5 | 4 | 26 | 70 | 50–65 | no | 5 |
| Mantellina reale (royal mantle) | 12 | 7 | 6 | 7 | 6 | 38 | 80 | 50–65 | no | 14 |
| Tunica reale (royal tunic) | 15 | 8 | 7 | 8 | 7 | 45 | 95 | 50–65 | no | 30 |
| Gambali reali (royal leggings) | 12 | 6 | 5 | 6 | 5 | 34 | 90 | 50–65 | no | 18 |
| Stivali reali (royal boots) | 6 | 4 | 3 | 4 | 3 | 20 | 45 | 50–65 | no | 5 |
| **Set completo** (6 pezzi base) | 65 | 35 | 30 | 35 | 30 | 195 | | | | **82** |

#### Legno (wooden plate)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo di legno (wooden helm) | 11 | 1 | 5 | 6 | 6 | 29 | 60 | 50–65 | no | 10 |
| Gorgiera di legno (wooden gorget) | 5 | 1 | 3 | 5 | 4 | 18 | 25 | 50–65 | no | 5 |
| Guanti di legno (wooden gauntlets) | 7 | 1 | 4 | 7 | 6 | 25 | 50 | 50–65 | no | 5 |
| Maniche di legno (wooden arms) | 11 | 1 | 6 | 8 | 7 | 33 | 60 | 50–65 | no | 14 |
| Tunica di legno (wooden tunic) | 14 | 3 | 7 | 9 | 8 | 41 | 75 | 50–65 | no | 30 |
| Gambali di legno (wooden leggings) | 10 | 1 | 6 | 9 | 8 | 34 | 70 | 50–65 | no | 18 |
| **Set completo** (6 pezzi base) | 58 | 8 | 31 | 44 | 39 | 180 | | | | **82** |

#### Ranger

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Gorgiera del ranger (ranger gorget) | 4 | 8 | 6 | 6 | 8 | 32 | 25 | 35–45 | no | 2 |
| Guanti del ranger (ranger gloves) | 4 | 8 | 6 | 6 | 8 | 32 | 25 | 35–45 | no | 2 |
| Maniche del ranger (ranger sleeves) | 2 | 4 | 3 | 3 | 4 | 16 | 25 | 35–45 | no | 4 |
| Tunica del ranger (ranger tunic) | 8 | 7 | 8 | 7 | 8 | 38 | 35 | 35–45 | no | 10 |
| Gambali del ranger (ranger leggings) | 5 | 8 | 6 | 6 | 8 | 33 | 30 | 35–45 | no | 6 |
| **Set completo** (5 pezzi base) | 23 | 35 | 29 | 28 | 36 | 151 | | | | **24** |

#### Pellicce (fur)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Maniche di pelliccia (fur arms) | 2 | 4 | 8 | 3 | 3 | 20 | 20 | 30–40 | sì | 2 |
| Tunica di pelliccia (fur tunic) | 2 | 4 | 13 | 3 | 3 | 25 | 25 | 30–40 | sì | 5 |
| Gambali di pelliccia (fur leggings) | 2 | 4 | 13 | 3 | 3 | 25 | 20 | 30–40 | sì | 3 |
| **Set completo** (3 pezzi base) | 6 | 12 | 34 | 9 | 9 | 70 | | | | **10** |

#### Fenice (phoenix)

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo della fenice (Phoenix Helm) | 11 | 14 | 12 | 11 | 12 | 60 | 40 | – | no | 0 |
| Gorgiera della fenice (Phoenix Gorget) | 13 | 12 | 11 | 12 | 11 | 59 | 40 | – | no | 0 |
| Guanti della fenice (Phoenix Gloves) | 11 | 13 | 11 | 9 | 15 | 59 | 40 | – | no | 5 |
| Maniche della fenice (Phoenix Sleeves) | 14 | 12 | 17 | 13 | 11 | 67 | 40 | – | no | 10 |
| Tunica della fenice (Phoenix Tunic) | 17 | 13 | 14 | 12 | 13 | 69 | 40 | – | no | 25 |
| Gambali della fenice (Phoenix Leggings) | 13 | 11 | 12 | 14 | 13 | 63 | 40 | – | no | 15 |
| **Set completo** (6 pezzi base) | 79 | 75 | 77 | 71 | 75 | 377 | | | | **55** |


Note sui set:

- Le **scaglie di drago** hanno solo 3 punti per resistenza di base, ma prendono quasi tutta la protezione dal tipo di **scaglia** usata (11.3): una tunica di scaglie blu arriva a 12 fisico e 12 fuoco.
- I set **gargoyle** di pietre e metalli preziosi (ametista, caddellite, smeraldo, granato, ghiaccio, giada, marmo, onice, quarzo, rubino, zaffiro, argento, spinello, rubino stellato, topazio) hanno **gli stessi valori delle piastre**; cambia l'aspetto. Lo stesso vale per i rispettivi scudi (come lo scudo triangolare).
- Le armature di **pelle** (pelle di morto, ghiacciata, lavica, di demone, di drago, d'incubo, di serpente, di troll, di unicorno, di alghe) hanno **gli stessi valori del cuoio** e permettono di meditare.
- Le armature **mutaforma** (del Padre Oscuro, del Minotauro, della Vedova) danno 8/5/5/7/5 per pezzo e trasformano chi indossa il set completo.
- L'**armatura d'onore** (*Plate of Honor*) ha 8/5/5/7/5 per pezzo con i requisiti delle piastre.

### Elmi

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Elmo (helmet) | 2 | 4 | 4 | 3 | 2 | 15 | 45 | 45–60 | no | 10 |
| Elmo chiuso (close helmet) | 9 | 2 | 2 | 2 | 2 | 17 | 55 | 45–60 | no | 10 |
| Bacinetto (bascinet) | 4 | 1 | 4 | 4 | 2 | 15 | 55 | 45–60 | no | 10 |
| Elmo norreno (norse helm) | 7 | 2 | 2 | 2 | 2 | 15 | 40 | 40–50 | no | 10 |
| Elmo del terrore (dread helm) | 5 | 3 | 2 | 3 | 2 | 15 | 80 | 50–65 | no | 10 |
| Elmo cornuto (horned helm) | 3 | 1 | 3 | 3 | 5 | 15 | 30 | 30–50 | no | 0 |

Altri elmi compaiono nelle tabelle dei set: berretto di cuoio, elmo di osso, cuffia di maglia, elmo di piastre, elmo di scaglie, elmo di osso demoniaco, elmo selvaggio, elmo reale, elmo di legno.

### Armature orientali (samurai e ninja)

I pezzi orientali di cuoio permettono di meditare; quelli borchiati e di piastre no (attenzione: a differenza del borchiato occidentale, il borchiato orientale **blocca** del tutto la meditazione).

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Jingasa di cuoio (leather jingasa) | 4 | 3 | 3 | 2 | 3 | 15 | 25 | 20–30 | sì | 0 |
| Mempo di cuoio (leather mempo) | 2 | 4 | 3 | 3 | 3 | 15 | 30 | 35–40 | sì | 1 |
| Hiro-sode di cuoio (leather hiro sode) | 2 | 4 | 3 | 3 | 3 | 15 | 25 | 40–50 | sì | 2 |
| Do di cuoio (leather do) | 4 | 6 | 5 | 5 | 5 | 25 | 40 | 35–45 | sì | 5 |
| Haidate di cuoio (leather haidate) | 4 | 6 | 5 | 5 | 5 | 25 | 20 | 30–40 | sì | 3 |
| Suneate di cuoio (leather suneate) | 4 | 8 | 6 | 6 | 5 | 29 | 20 | 25–40 | sì | 3 |
| Cappuccio ninja di cuoio (leather ninja hood) | 2 | 3 | 3 | 3 | 4 | 15 | 10 | 25–45 | sì | 0 |
| Muffole ninja di cuoio (leather ninja mitts) | 2 | 4 | 3 | 3 | 3 | 15 | 10 | 25–25 | sì | 1 |
| Giacca ninja di cuoio (leather ninja jacket) | 2 | 4 | 3 | 3 | 3 | 15 | 10 | 55–65 | sì | 5 |
| Pantaloni ninja di cuoio (leather ninja pants) | 2 | 4 | 3 | 3 | 3 | 15 | 10 | 40–50 | sì | 3 |
| Mempo borchiato (studded mempo) | 2 | 4 | 3 | 3 | 3 | 15 | 30 | 30–40 | no | 2 |
| Hiro-sode borchiato (studded hiro sode) | 2 | 4 | 3 | 3 | 4 | 16 | 30 | 45–55 | no | 4 |
| Do borchiato (studded do) | 6 | 8 | 7 | 7 | 7 | 35 | 55 | 40–50 | no | 10 |
| Haidate borchiato (studded haidate) | 6 | 8 | 7 | 7 | 6 | 34 | 30 | 35–45 | no | 6 |
| Suneate borchiato (studded suneate) | 6 | 9 | 8 | 8 | 7 | 38 | 30 | 35–50 | no | 6 |
| Hatsuburi di maglia (mail hatsuburi) | 8 | 2 | 2 | 2 | 4 | 18 | 50 | 55–75 | no | 10 |
| Hatsuburi di piastre (plate hatsuburi) | 12 | 5 | 5 | 5 | 5 | 32 | 65 | 55–75 | no | 10 |
| Jingasa leggero di piastre (light plate jingasa) | 10 | 4 | 4 | 4 | 4 | 26 | 55 | 55–60 | no | 10 |
| Jingasa piccolo di piastre (small plate jingasa) | 10 | 4 | 4 | 4 | 4 | 26 | 55 | 55–60 | no | 10 |
| Jingasa pesante di piastre (heavy plate jingasa) | 12 | 5 | 5 | 5 | 5 | 32 | 55 | 50–70 | no | 10 |
| Kabuto di piastre (standard plate kabuto) | 11 | 4 | 4 | 4 | 4 | 27 | 70 | 60–65 | no | 10 |
| Kabuto da battaglia (plate battle kabuto) | 12 | 5 | 5 | 5 | 5 | 32 | 70 | 60–65 | no | 10 |
| Kabuto decorativo (decorative plate kabuto) | 6 | 2 | 2 | 2 | 3 | 15 | 70 | 55–75 | no | 10 |
| Mempo di piastre (plate mempo) | 6 | 4 | 3 | 4 | 3 | 20 | 50 | 60–70 | no | 5 |
| Hiro-sode di piastre (plate hiro sode) | 12 | 7 | 6 | 7 | 6 | 38 | 75 | 55–75 | no | 14 |
| Do di piastre (plate do) | 15 | 8 | 7 | 8 | 7 | 45 | 85 | 60–70 | no | 30 |
| Haidate di piastre (plate haidate) | 12 | 6 | 5 | 6 | 5 | 34 | 80 | 55–65 | no | 18 |
| Suneate di piastre (plate suneate) | 12 | 6 | 5 | 6 | 5 | 34 | 80 | 55–65 | no | 18 |

### Scudi

Gli scudi quasi non danno resistenze: servono a **parare** (capitolo 10, 10.3). Più alta è la Parata, più parerai. Gli scudi non contano né per la meditazione né per la furtività, ma occupano la seconda mano.

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Scudo di legno (wooden shield) | 0 | 0 | 0 | 0 | 1 | 1 | 20 | 20–25 | – | 0 |
| Brocchiere (buckler) | 0 | 0 | 0 | 1 | 0 | 1 | 20 | 40–50 | – | 0 |
| Scudo rotondo (round shield) | 0 | 0 | 1 | 0 | 0 | 1 | 35 | 25–30 | – | 0 |
| Scudo del campione (champion shield) | 0 | 0 | 1 | 0 | 0 | 1 | 35 | 25–30 | – | 0 |
| Scudo crestato (crested shield) | 0 | 0 | 1 | 0 | 0 | 1 | 35 | 25–30 | – | 0 |
| Scudo di metallo (metal shield) | 0 | 1 | 0 | 0 | 0 | 1 | 45 | 50–65 | – | 0 |
| Scudo a mandorla di legno (wooden kite shield) | 0 | 0 | 0 | 0 | 1 | 1 | 20 | 50–65 | – | 0 |
| Scudo a mandorla (kite shield) | 0 | 0 | 0 | 0 | 1 | 1 | 45 | 45–60 | – | 0 |
| Scudo elfico (elven shield) | 0 | 0 | 0 | 0 | 1 | 1 | 45 | 45–60 | – | 0 |
| Scudo della guardia (guardsman shield) | 0 | 0 | 0 | 0 | 1 | 1 | 45 | 45–60 | – | 0 |
| Scudo ingioiellato (jeweled shield) | 0 | 0 | 0 | 0 | 1 | 1 | 45 | 45–60 | – | 0 |
| Scudo triangolare (heater shield) | 0 | 1 | 0 | 0 | 0 | 1 | 90 | 50–65 | – | 0 |
| Scudo oscuro (dark shield) | 0 | 1 | 0 | 0 | 0 | 1 | 90 | 50–65 | – | 0 |
| Scudo reale (royal shield) | 0 | 1 | 0 | 0 | 0 | 1 | 90 | 50–65 | – | 0 |
| Scudo dell'Ordine (order shield) | 1 | 0 | 0 | 0 | 0 | 1 | 95 | 100–125 | – | 0 |
| Scudo del Caos (chaos shield) | 1 | 0 | 0 | 0 | 0 | 1 | 95 | 100–125 | – | 0 |

Lo **scudo di Dupre** (*Dupre's shield*, ricompensa di anniversario) è un'eccezione: 16 fisico più 2/3/4/5.

> **Consiglio:** uno scudo leggero (brocchiere, scudo di legno) basta per allenare Parata. Quando la Parata supera 100 ottieni anche un +5% fisso. Se hai anche Bushido, lascia perdere lo scudo e prendi un'arma a due mani.

### Occhiali (glasses)

Gli **occhiali** sono elmi speciali (cuoio, permettono di meditare, Forza 45, durabilità 36–48) con resistenze alte e proprietà fisse, per esempio:

| Pezzo | Fis | Fuo | Fre | Vel | Ene | Tot | For | Durab. | Med. | Furt. |
|---|---|---|---|---|---|---|---|---|---|---|
| Occhiali elfici (elven glasses) | 2 | 4 | 4 | 3 | 2 | 15 | 45 | 36–48 | sì | 0 |
| Occhiali del mago (wizard's glasses) | 5 | 5 | 5 | 5 | 5 | 25 | 45 | 36–48 | sì | 0 |
| Occhiali marittimi (maritime glasses) | 3 | 4 | 30 | 5 | 3 | 45 | 45 | 36–48 | sì | 0 |
| Occhiali dell'antropomorfista (anthropomorphist glasses) | 5 | 5 | 10 | 20 | 20 | 60 | 45 | 36–48 | sì | 0 |
| Occhiali delle arti (arts glasses) | 10 | 8 | 8 | 4 | 10 | 40 | 45 | 36–48 | sì | 0 |
| Occhiali della via luminosa (light of way glasses) | 10 | 10 | 10 | 10 | 10 | 50 | 45 | 36–48 | sì | 0 |
| Occhiali lirici (lyrical glasses) | 10 | 10 | 10 | 10 | 10 | 50 | 45 | 36–48 | sì | 0 |
| Occhiali del mercante (trade glasses) | 10 | 10 | 10 | 10 | 10 | 50 | 45 | 36–48 | sì | 0 |
| Occhiali del cercatore di tesori (treasure trinket glasses) | 10 | 10 | 10 | 10 | 10 | 50 | 45 | 36–48 | sì | 0 |
| Occhiali avvelenati (poisoned glasses) | 10 | 10 | 10 | 30 | 10 | 70 | 45 | 36–48 | sì | 0 |
| Occhiali d'acciaio ripiegato (folded steel glasses) | 20 | 10 | 10 | 10 | 10 | 60 | 45 | 36–48 | sì | 0 |
| Occhiali mazza e scudo (mace and shield glasses) | 25 | 10 | 10 | 10 | 10 | 65 | 45 | 36–48 | sì | 0 |
| Occhiali negromantici (necromantic glasses) | 0 | 0 | 0 | 0 | 0 | 0 | 45 | 36–48 | sì | 0 |

Sono oggetti rari, trovati come bottino o ricompensa.

---

## 11.3 I materiali

Il materiale cambia le resistenze, la durabilità, i requisiti, la Fortuna e, per le armi, il danno e il tipo di danno. La tabella riporta i bonus **per pezzo di armatura** (fisico/fuoco/freddo/veleno/energia), la durabilità extra e la riduzione dei requisiti, più l'effetto sulle **armi** dello stesso materiale.

### Metalli (fabbro)

| Metallo | Armatura F/Fu/Fr/V/E | Durab. | Req. | Fortuna | Armi: danno extra e conversione elementale | Attrezzo runico: proprietà / intensità |
|---|---|---|---|---|---|---|
| Ferro (iron) | 0/0/0/0/0 | – | – | – | +0 | – |
| Rame opaco (dull copper) | 2/0/0/0/5 | +30% | −20% | – | +1; durab. +30%, req. −50% | 1 / 60–70% |
| Ferro ombra (shadow iron) | 2/0/0/2/5 | +10% | – | – | +2; 20% freddo, durab. +50% | 1–2 / 70–80% |
| Rame (copper) | 3/2/0/4/2 | – | – | – | +3; 10% veleno, 20% energia | 2–3 / 70–80% |
| Bronzo (bronze) | 3/0/5/2/3 | – | – | – | +4; 40% fuoco | 2–4 / 70–80% |
| Oro (gold) | 4/5/2/0/4 | – | −30% | +40 | +4; Fortuna +40, req. −50% | 3–4 / 70–80% |
| Agapite | 5/3/3/3/3 | – | – | – | +5; 30% freddo, 20% energia | 3–5 / 80–90% |
| Verite | 5/4/4/5/1 | – | – | – | +5; 40% veleno, 20% energia | 4–6 / 80–100% |
| Valorite | 6/0/5/5/5 | +15% | – | – | +6; 10% fuoco, 20% freddo, 10% veleno, 20% energia | 5–9 / 90–100% |
| Nepturite | 6/0/8/10/0 | +20% | – | – | +6; 25% freddo, 25% veleno | 5–6 / 80–100% |
| Ossidiana (obsidian) | 7/12/4/4/9 | +25% | – | – | +6; 20% fuoco, 10% energia | 5–6 / 85–100% |
| Acciaio (steel) | 5/0/3/3/2 | +10% | −25% | – | +7; durab. +10%, req. −25% | 5–7 / 85–100% |
| Ottone (brass) | 6/0/4/4/3 | +10% | −45% | – | +8; 20% fuoco, 20% energia | 5–7 / 90–100% |
| Mithril | 15/0/15/9/9 | +30% | −75% | +100 | +9; 30% energia, Fortuna +100 | 6–7 / 90–100% |
| Xormite | 15/8/8/8/15 | +20% | −75% | – | +9; 30% danno diretto | 6–7 / 100–120% |
| **Nanico (dwarven)** | 16/10/7/0/0 | +35% | – | – | **+18**; durab. +35% | 6–8 / 100–150% |

### Cuoi (sarto e conciatore)

| Cuoio | Armatura F/Fu/Fr/V/E | Durab. | Altro | Armi (guanti, fionde...) | Attrezzo runico |
|---|---|---|---|---|---|
| Normale | 0/0/0/0/0 | – | – | – | – |
| *Spined* (deep sea, del mare profondo) | 0/0/2/2/1 | +5% | Fortuna +40 | +1; 20% veleno | 1 / 60–70% |
| *Horned* (lizard, di lucertola) | 1/0/3/3/0 | +10% | – | +2 | 1–2 / 70–80% |
| *Barbed* (serpent, di serpente) | 1/3/3/2/0 | +20% | – | +3; 70% veleno | 2–3 / 70–80% |
| *Necrotic* (necrotico) | 1/0/4/3/2 | +5% | – | +4; 50% fuoco | 2–4 / 70–80% |
| *Volcanic* (vulcanico) | 2/6/0/3/0 | +15% | – | +4; 50% fuoco | 3–4 / 70–80% |
| *Frozen* (gelato) | 2/0/6/0/4 | +10% | – | +5; 50% freddo | 3–5 / 80–90% |
| *Goliath* | 4/3/3/0/3 | +25% | – | +6; 25% energia | 4–6 / 80–100% |
| *Draconic* (draconico) | 2/6/0/3/3 | +25% | – | +8; 25% fuoco | 5–9 / 90–100% |
| *Hellish* (infernale) | 3/0/4/2/6 | +10% | – | +9; 50% fuoco | 5–7 / 85–100% |
| *Dinosaur* (di dinosauro) | 3/4/4/5/0 | +40% | – | +10 | 6–7 / 90–100% |
| **Alien** (alieno) | 5/7/7/6/7 | +30% | – | **+18** | 6–8 / 100–150% |

### Scaglie (fabbro, armature di drago)

| Scaglie | Armatura F/Fu/Fr/V/E | Note |
|---|---|---|
| Rosse (red) | 5/8/8/2/2 | |
| Gialle (yellow) | 5/4/4/4/4 | +4 a *Gold Increase* |
| Nere (black) | 6/3/3/9/9 | |
| Verdi (green) | 6/3/9/9/3 | |
| Bianche (white) | 6/9/3/3/9 | |
| Blu (blue) | 9/9/3/3/6 | |
| Di dinosauro (dinosaur) | 11/6/6/6/6 | |

### Legni (carpentiere e costruttore di archi)

| Legno | Armatura F/Fu/Fr/V/E | Durab. | Altro | Armi | Attrezzo runico |
|---|---|---|---|---|---|
| Normale | 0/0/0/0/0 | – | – | – | – |
| Frassino (ash) | 1/1/1/1/1 | – | – | +1; 5% per ogni elemento | 1 / 60–70% |
| Ciliegio (cherry) | 1/2/1/2/1 | – | – | +1; 10% veleno, 20% energia | 1–2 / 70–80% |
| Ebano (ebony) | 3/3/0/0/2 | +20% | – | +2; 20% freddo | 2–3 / 70–80% |
| Quercia dorata (golden oak) | 3/2/3/0/2 | – | Fortuna +40, req. −30% | +2 | 2–4 / 70–80% |
| Noce americano (hickory) | 5/0/7/0/0 | +20% | req. −20% | +3 | 3–4 / 70–80% |
| Mogano (mahogany) | 7/3/0/3/2 | – | – | +3; 10% veleno, 20% energia | 3–5 / 80–90% |
| Quercia (oak) | 3/0/5/5/3 | – | – | +4; 40% fuoco | 4–6 / 80–100% |
| Pino (pine) | 3/3/3/5/5 | – | – | +4; 30% freddo, 20% energia | 5–9 / 90–100% |
| Legno spettrale (ghostwood) | 9/5/9/9/6 | – | – | +8; 25% freddo, 25% energia | 5–6 / 80–100% |
| Palissandro (rosewood) | 5/5/3/8/3 | – | – | +5; 40% veleno, 20% energia | 5–6 / 85–100% |
| Noce (walnut) | 10/0/5/4/4 | +10% | – | +5; 10% fuoco, 20% freddo, 10% veleno, 20% energia | 5–7 / 85–100% |
| Legno pietrificato (petrified) | 10/0/5/5/10 | +30% | – | +9; 25% freddo | 5–7 / 90–100% |
| Legno di deriva (driftwood) | 9/0/8/10/5 | +5% | – | +7; 10% fuoco, 10% freddo, 20% veleno, 10% energia | 6–7 / 90–100% |
| **Legno elfico (elven)** | 10/3/0/11/15 | +25% | Fortuna +100 | **+18** | 6–8 / 100–150% |

> **Consiglio:** la guida originale consiglia di forgiare l'**agapite** e la **valorite** per le armi quando arrivi a 90 di Fabbro, e il **bronzo** come buon metallo intermedio per le armature. I materiali alti (mithril, xormite, nanico, legno elfico, cuoio alieno) danno vantaggi enormi ma sono rarissimi.

### Artigianato eccezionale e attrezzi runici

- Un pezzo **eccezionale** riceve **5 punti di resistenza** distribuiti a caso (8 se fatto con un attrezzo runico), più **1 punto ogni 15 di Conoscenza Armi** dell'artigiano (6 punti a 90, 8 a 120), e +20% di durabilità. Gli scudi eccezionali non ricevono i 5 punti casuali, ma solo quelli dovuti a Conoscenza Armi.
- Un **attrezzo runico** (martello, kit da cucito, attrezzi da falegname di un materiale speciale) aggiunge al pezzo da 1 a 9 proprietà magiche casuali con l'intensità indicata nelle tabelle sopra (vedi il capitolo 12 per i valori). Gli attrezzi dei materiali "alti" (nepturite, ossidiana, acciaio, ottone, mithril, xormite, nanico) sono segnalati nel codice come "non ancora bilanciati": la valorite, per esempio, dà più proprietà (5–9) della nepturite (5–6).

### Vecchie armature magiche

Alcune armature del bottino portano ancora i vecchi nomi magici:

| Nome | Effetto |
|---|---|
| *of defense / guarding / hardening / fortification / invulnerability* (protezione) | +0 / +1 / +2 / +3 / +4 a ogni resistenza |
| *durable / substantial / massive / fortified / indestructible* (durabilità) | durabilità aumentata (+20%, +50%, +70%...) |

---

## 11.4 Gioielli

I gioielli non proteggono di per sé (resistenze 0) ma sono il posto migliore per le **proprietà magiche**: possono avere quasi tutte quelle elencate nel capitolo 12, comprese probabilità di colpire e di difesa, bonus alle caratteristiche, aumento del danno degli incantesimi e bonus alle abilità.

| Slot | Oggetti |
|---|---|
| Anello (*ring*) | anello d'oro, d'argento, e anelli di metalli e gemme |
| Bracciale (*bracelet*) | bracciale d'oro, d'argento, e varianti di metalli e gemme |
| Orecchini (*earrings*) | orecchini d'oro, d'argento, e varianti |
| Collana o amuleto (*necklace*, *amulet*) | collane, collane di perline, amuleti |

I **gioielli fatti dal meccanico** con un metallo o una gemma possono nascere già magici, con una proprietà casuale. La probabilità e l'intensità dipendono dal materiale:

| Materiale | Probabilità di 1 proprietà | Intensità |
|---|---|---|
| Ambra, ametista, agapite, rubino, zaffiro, zaffiro stellato, tormalina | 25% | 1–3% |
| Smeraldo | 25% | 2–4% |
| Diamante | 25% | 3–6% |
| Rame, rame opaco, ferro ombra, bronzo | 35% | 2–6% |
| Oro, rubino stellato | 35% | 3–7% |
| Verite, valorite | 45% | 3–8% |
| Argento, granato, giada, onice, perla, quarzo, spinello, topazio, ossidiana, ottone, acciaio | 50% | 3–9% |
| Caddellite, mithril, nepturite | 60% | 4–9% |
| **Nanico (dwarven)** | 90% | 8–18% |

L'intensità è la percentuale del valore massimo della proprietà: per esempio un anello d'argento con "intensità 9%" su *Aumento danno* (massimo 25) dà circa +2%. Sono quindi piccoli bonus, utili soprattutto per vendere.

Ci sono poi oggetti magici che occupano altri slot ma funzionano **come gioielli** (stesse proprietà casuali): **cinture** (*belt*, *loin cloth*, *apron*), **fasce** (*sash*), **vesti** (*robe*), **cappelli**, **mantelli**, **stivali** magici, **talismani**, **torce**, **candele** e **lanterne** magiche. Si riconoscono perché nel bottino compaiono già con un nome "magico".

> **Consiglio:** la guida originale suggerisce di raccogliere **tutti** i gioielli e le gemme che trovi: si vendono bene ai mercanti, anche se non identificati (non conviene pagare per identificarli solo per venderli).

Gli **anelli di gilda** (*guild ring*) si ricevono entrando in una gilda e danno bonus fissi alle abilità della gilda:

| Gilda | Bonus dell'anello |
|---|---|
| Maghi (*Wizards*) | +10 Valutare Intelligenza, Magia, Meditazione |
| Guerrieri (*Warriors*) | +10 Lame Leggere, Combattere con Mazza, Parata, Scherma con Spada, Tattica |
| Ladri (*Thieves*) | +10 Nascondersi, Scassinare, Curiosare, Furto, Furtività |
| Ranger | +25 Accampamento, Seguire Tracce |
| Guaritori (*Healers*) | +15 Anatomia, Guarigione, Veterinaria |
| Minatori (*Miners*) | +30 Estrazione Mineraria |
| Mercanti (*Merchants*) | +15 Identificare Oggetti, Conoscenza Armi, Assaggiare |
| Meccanici (*Tinkers*) | +30 Meccanica |
| Sarti (*Tailors*) | +30 Sartoria |
| Marinai (*Sailors*) | +30 Pesca |
| Bardi (*Bards*) | +10 Discordia, Musica, Pacificazione, Provocazione |
| Fabbri (*Blacksmiths*) | +20 Fabbro, Conoscenza Armi |
| Negromanti (*Necromancers*) | +10 Alchimia, Medicina Legale, Negromanzia, Parlare con gli Spiriti |
| Alchimisti (*Alchemists*) | +15 Alchimia, Cucina, Assaggiare |
| Druidi (*Druids*) | +5 Alchimia, +10 Conoscenza Animali, Addomesticare Animali, Pastorizia, Veterinaria, +5 Cucina |
| Arcieri (*Archers*) | +10 Tiro con l'Arco, +20 Costruzione Archi, +10 Tattica |
| Carpentieri (*Carpenters*) | +20 Carpenteria, Taglialegna |
| Cartografi (*Cartographers*) | +30 Cartografia |
| Bibliotecari (*Librarians*) | +20 Identificare Oggetti, Scrittura Arcana |
| Cuochi (*Culinarians*) | +20 Cucina, Assaggiare |
| Assassini (*Assassins*) | +10 Lame Leggere, Nascondersi, Furtività, +15 Avvelenamento |

> **Consiglio:** la guida originale consiglia ai guerrieri la **gilda dei guerrieri**: i +10 a Tattica e all'abilità d'arma dell'anello fanno sbloccare prima le mosse speciali (servono 70 in entrambe).

---

## 11.5 Vestiario

Gli abiti normali (camicie, pantaloni, vesti, mantelli, stivali di stoffa...) non hanno resistenze: si portano per l'aspetto, per le proprietà magiche e perché occupano slot che le armature non usano (camicia, veste esterna, mantello, cintura). Possono avere le stesse proprietà magiche dei cappelli (11.6) e si riparano dal sarto.

Alcune eccezioni utili:

- La maggior parte degli **stivali, scarpe e sandali di cuoio** (*leather boots*, *shoes*, *sandals*, *thigh boots*) e il **mantello e la veste di cuoio** sono **armature di cuoio** (vedi la tabella del cuoio): danno resistenze e permettono di meditare.
- Le **pellicce** (*fur*: tunica, maniche, gambali) sono armature di cuoio con **13 di resistenza al freddo** sulla tunica e sui gambali: utili nelle terre gelate. Esistono berretto, mantello, stivali, sarong e veste di pelliccia come semplici abiti.
- Le **vesti del vagabondo, del negromante, del mago**, ecc. sono solo estetiche, salvo proprietà magiche.

### 11.6 Cappelli, cappucci e maschere

A differenza degli altri abiti, i **cappelli** hanno resistenze base e possono avere proprietà magiche "da armatura" (resistenze, rigenerazione, Fortuna, durabilità...). Non ostacolano né la meditazione né la furtività.

| Copricapo | Fis | Fuo | Fre | Vel | Ene |
|---|---|---|---|---|---|
| Cappelli di stoffa (cap, floppy hat, wide-brim hat, straw hat, tall straw hat, feathered hat, bonnet, tricorne, pirate hat, jester hat, wizard's hat, witch hat, kasa), cappucci (cloth hood, cowl, fancy hood, reaper hood) | 0 | 5 | 9 | 5 | 5 |
| Bandana, berretto (skullcap) | 0 | 3 | 5 | 8 | 8 |
| Ghirlanda di fiori (flower garland), cappuccio ninja (ninja hood) | 3 | 3 | 6 | 9 | 9 |
| Maschera dell'orso (bear mask), maschera del lupo (wolf mask) | 5 | 3 | 8 | 4 | 4 |
| Maschera del cervo / del daino (stag / deer mask) | 2 | 6 | 8 | 1 | 7 |
| Maschera tribale (tribal mask) | 3 | 0 | 6 | 10 | 5 |
| Maschera tribale cornuta (horned tribal mask) | 6 | 9 | 0 | 4 | 5 |
| Maschera orchesca (orc mask) | 1 | 1 | 7 | 7 | 8 |
| Marchio dell'orco (Orc Mark) | 15 | 5 | 5 | 0 | 0 |
| Marchio del gargoyle (Gargoyle Mark) | 0 | 10 | 5 | 0 | 10 |
| Marchio della lucertola (Lizard Mark) | 0 | 10 | 0 | 10 | 5 |
| Marchio di Calypso (Calypso Mark) | 5 | 0 | 10 | 10 | 0 |
| Berretto di pelliccia (fur cap) | 0 | 0 | 5 | 0 | 0 |

> **Consiglio:** un mago che non vuole perdere la meditazione può proteggere la testa con un cappello di stoffa (0/5/9/5/5) invece che con un elmo: un berretto di cuoio dà solo 2/4/3/3/3.

---

## 11.7 Come costruire una protezione equilibrata

| Personaggio | Testa | Corpo | Note |
|---|---|---|---|
| Guerriero principiante | elmo chiuso o elmo di piastre | piastre o anelli | Tutta resistenza fisica; compra l'armatura dal fabbro o dal monaco (spesso più economico, secondo la guida originale) |
| Guerriero con magia di supporto | elmo con Mage Armor o cappello | piastre con Mage Armor, oppure cuoio | Senza Mage Armor perdi la Meditazione |
| Mago | cappello di stoffa | cuoio o pelli, vesti magiche | Cerca gioielli con *Spell Damage*, *Lower Mana Cost*, *Faster Casting* |
| Ladro, assassino | bandana o cappuccio | borchiato | Resta sotto 42 di peso per la Furtività |
| Arciere, ranger | elmo leggero | borchiato o set del ranger | Buon compromesso fra resistenze e libertà |

> **Consiglio:** i pezzi con la proprietà **Durabilità %** (*Durability*) si vendono ai mercanti meglio delle altre armature: se non ti servono, raccoglili.
