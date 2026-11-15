// Cards defined here require the client update (Gumpart.mul/Gumpart.idx) with relevant graphics

using System;

namespace Server.Items
{
    public class CollectableCardAncientWyrm : CollectableCard
    {
        [Constructable]
        public CollectableCardAncientWyrm() : base(
                "Ancient Wyrm",
                "Boss",
                CardRarity.Epic,
                CardCategory.Boss,
                400,
                "The Deadliest of the Wyrms"
                ) {}

        public CollectableCardAncientWyrm(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSoulbound : CollectableCard
    {
        [Constructable]
        public CollectableCardSoulbound() : base(
                "Soulbound",
                "Legends",
                CardRarity.Rare,
                CardCategory.Legends,
                401,
                "Eternally Reborn to Become a Roguelike Legend"
                ) {}

        public CollectableCardSoulbound(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardFoundingFathers : CollectableCard
    {
        [Constructable]
        public CollectableCardFoundingFathers() : base(
                "Founding Fathers",
                "May 22, 2020",
                CardRarity.Legendary,
                CardCategory.History,
                402,
                "Forever Cast in Stone and Metal"
                ) {}

        public CollectableCardFoundingFathers(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardLordBritish : CollectableCard
    {
        [Constructable]
        public CollectableCardLordBritish() : base(
                "Lord British",
                "Legends",
                CardRarity.Common,
                CardCategory.Legends,
                403,
                "Sovereign of Sosaria. Rumored to be Invincible"
                ) {}

        public CollectableCardLordBritish(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSarthAbbey : CollectableCard
    {
        [Constructable]
        public CollectableCardSarthAbbey() : base(
                "Sarth Abbey",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                404,
                "Home to the Largest Public Library in Sosaria"
                ) {}

        public CollectableCardSarthAbbey(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardBritain : CollectableCard
    {
        [Constructable]
        public CollectableCardBritain() : base(
                "Britain",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                405,
                "Where Thy Adventures Started"
                ) {}

        public CollectableCardBritain(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardPeterGrimm : CollectableCard
    {
        [Constructable]
        public CollectableCardPeterGrimm() : base(
                "Peter Grimm",
                "Legends",
                CardRarity.Rare,
                CardCategory.Legends,
                406,
                "The Infamous Dread Lord"
                ) {}

        public CollectableCardPeterGrimm(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDarkFather : CollectableCard
    {
        [Constructable]
        public CollectableCardDarkFather() : base(
                "Dark Father",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                407,
                "Thy Doom Materialized"
                ) {}

        public CollectableCardDarkFather(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMorinia : CollectableCard
    {
        [Constructable]
        public CollectableCardMorinia() : base(
                "Mines of Morinia",
                "Dungeon",
                CardRarity.Uncommon,
                CardCategory.Dungeon,
                408,
                "Burial Place for More than One Miner"
                ) {}

        public CollectableCardMorinia(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardFiresOfHell : CollectableCard
    {
        [Constructable]
        public CollectableCardFiresOfHell() : base(
                "Fires of Hell",
                "Dungeon",
                CardRarity.Uncommon,
                CardCategory.Dungeon,
                409,
                "Fiery and Deadly"
                ) {}

        public CollectableCardFiresOfHell(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardColdhall : CollectableCard
    {
        [Constructable]
        public CollectableCardColdhall() : base(
                "Coldhall Depths",
                "Dungeon",
                CardRarity.Uncommon,
                CardCategory.Dungeon,
                410,
                "Ice Cold and Unwelcoming"
                ) {}

        public CollectableCardColdhall(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAncientPyramid : CollectableCard
    {
        [Constructable]
        public CollectableCardAncientPyramid() : base(
                "Ancient Pyramid",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                411,
                "Full of Ancient Mysteries"
                ) {}

        public CollectableCardAncientPyramid(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCard6thAnniversary : CollectableCard
    {
        [Constructable]
        public CollectableCard6thAnniversary() : base(
                "6th Anniversary",
                "May 2026",
                CardRarity.Epic,
                CardCategory.ServerEvent,
                412,
                "Six Years of Epic Adventures"
                ) {}

        public CollectableCard6thAnniversary(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardShardLaunch : CollectableCard
    {
        [Constructable]
        public CollectableCardShardLaunch() : base(
                "Ultima Adventures Launch",
                "May 2020",
                CardRarity.Legendary,
                CardCategory.ServerEvent,
                413,
                "From Hence, The Chronicles of Ultima Adventures Began"
                ) {}

        public CollectableCardShardLaunch(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardPublicBank : CollectableCard
    {
        [Constructable]
        public CollectableCardPublicBank() : base(
                "The Bank",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                414,
                "Where Adventurers' Paths Inevitably Cross"
                ) {}

        public CollectableCardPublicBank(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMangarsTower : CollectableCard
    {
        [Constructable]
        public CollectableCardMangarsTower() : base(
                "Mangar's Tower",
                "Town",
                CardRarity.Epic,
                CardCategory.Town,
                415,
                "One-Way Ticket to Skara Brae"
                ) {}

        public CollectableCardMangarsTower(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }


    public class CollectableCardDawn : CollectableCard
    {
        [Constructable]
        public CollectableCardDawn() : base(
                "Dawn",
                "Town",
                CardRarity.Epic,
                CardCategory.Town,
                416,
                "Mystical city bathed in perpetual moonlight"
                ) {}

        public CollectableCardDawn(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }


    public class CollectableCardDeathGulch : CollectableCard
    {
        [Constructable]
        public CollectableCardDeathGulch() : base(
                "Death Gulch",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                417,
                "" // TODO: description
                ) {}

        public CollectableCardDeathGulch(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }


    public class CollectableCardDevilGuard : CollectableCard
    {
        [Constructable]
        public CollectableCardDevilGuard() : base(
                "Devil Guard",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                418,
                "A Secluded Town in the Mountains"
                ) {}

        public CollectableCardDevilGuard(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardFawn : CollectableCard
    {
        [Constructable]
        public CollectableCardFawn() : base(
                "Fawn",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                419,
                "A Delicate Elven Enclave Among the Flowering Meadows"
                ) {}

        public CollectableCardFawn(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }


    public class CollectableCardGlacialCoast : CollectableCard
    {
        [Constructable]
        public CollectableCardGlacialCoast() : base(
                "Glacial Coast",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                420,
                "A Small Icy Coastal Town"
                ) {}

        public CollectableCardGlacialCoast(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }


    public class CollectableCardGray : CollectableCard
    {
        [Constructable]
        public CollectableCardGray() : base(
                "Gray",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                421,
                "A Dreary Mining Town Where the Sun Rarely Shines"
                ) {}

        public CollectableCardGray(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardLamutCountry : CollectableCard
    {
        [Constructable]
        public CollectableCardLamutCountry() : base(
                "Lamut Country",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                422,
		"Rolling Farmlands Ruled by the House of Lamut"
                ) {}

        public CollectableCardLamutCountry(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMontor : CollectableCard
    {
        [Constructable]
        public CollectableCardMontor() : base(
                "Montor",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                423,
                "The Biggest City in the South of Sosaria"
                ) {}

        public CollectableCardMontor(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMountainCrest : CollectableCard
    {
        [Constructable]
        public CollectableCardMountainCrest() : base(
                "Mountain Crest",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                424,
		"A Settlement Clinging to the High Mountain Peaks"
                ) {}

        public CollectableCardMountainCrest(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardUmbra : CollectableCard
    {
        [Constructable]
        public CollectableCardUmbra() : base(
                "Undercity of Umbra",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                425,
		"A Sunless City Where Necromancers Practice Their Dark Arts"
                ) {}

        public CollectableCardUmbra(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }


    public class CollectableCardYew : CollectableCard
    {
        [Constructable]
        public CollectableCardYew() : base(
                "Yew",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                426,
                "Forest Town Among Ancient Yew Trees"
                ) {}

        public CollectableCardYew(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAccursedMaze : CollectableCard
    {
        [Constructable]
        public CollectableCardAccursedMaze() : base(
                "Accursed Maze",
                "Dungeon",
                CardRarity.Common,
                CardCategory.Dungeon,
                427,
                "Endless Labyrinth that Confuses All Who Enter"
                ) {}

        public CollectableCardAccursedMaze(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardChamberOfBane : CollectableCard
    {
        [Constructable]
        public CollectableCardChamberOfBane() : base(
                "Chamber of Bane",
                "Dungeon",
                CardRarity.Common,
                CardCategory.Dungeon,
                428,
                "Toxic Dungeon Where Even the Air is Deadly"
                ) {}

        public CollectableCardChamberOfBane(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardClues : CollectableCard
    {
        [Constructable]
        public CollectableCardClues() : base(
                "Clues",
                "Dungeon",
                CardRarity.Uncommon,
                CardCategory.Dungeon,
                429,
                "Secluded Cardle of the Adventurers' Secrets"
                ) {}

        public CollectableCardClues(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDarkSanctum : CollectableCard
    {
        [Constructable]
        public CollectableCardDarkSanctum() : base(
                "Dark Sanctum",
                "Dungeon",
                CardRarity.Common,
                CardCategory.Dungeon,
                430,
                "Temple Where Darkness Physically Manifests"
                ) {}

        public CollectableCardDarkSanctum(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardForgottenTombs : CollectableCard
    {
        [Constructable]
        public CollectableCardForgottenTombs() : base(
                "Forgotten Tombs",
                "Dungeon",
                CardRarity.Common,
                CardCategory.Dungeon,
                431,
                "Ancient Burial Grounds of a Forgotten Dynasty"
                ) {}

        public CollectableCardForgottenTombs(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMagmaVaults : CollectableCard
    {
        [Constructable]
        public CollectableCardMagmaVaults() : base(
                "Magma Vaults",
                "Dungeon",
                CardRarity.Common,
                CardCategory.Dungeon,
                432,
                "Volcanic Caverns Filled with Fiery Creatures"
                ) {}

        public CollectableCardMagmaVaults(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardShroudedGrave : CollectableCard
    {
        [Constructable]
        public CollectableCardShroudedGrave() : base(
                "Shrouded Grave",
                "Dungeon",
                CardRarity.Common,
                CardCategory.Dungeon,
                433,
                "Excavated Necropolis of Restless Dead"
                ) {}

        public CollectableCardShroudedGrave(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }


    public class CollectableCardTimeAwaits : CollectableCard
    {
        [Constructable]
        public CollectableCardTimeAwaits() : base(
                "Time Awaits",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                434,
                "Where Time Flows Differently and Paradoxes Abound"
                ) {}

        public CollectableCardTimeAwaits(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAbandon : CollectableCard
    {
        [Constructable]
        public CollectableCardAbandon() : base(
                "Abandon",
                "Dungeon",
                CardRarity.Uncommon,
                CardCategory.Dungeon,
                435,
                "" // TODO: Description
                ) {}

        public CollectableCardAbandon(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAmbrosia : CollectableCard
    {
        [Constructable]
        public CollectableCardAmbrosia() : base(
                "Ambrosia",
                "Legends",
                CardRarity.Rare,
                CardCategory.Legends,
                436,
                "The Land of the Champions"
                ) {}

        public CollectableCardAmbrosia(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardCavernsOfPoseidon : CollectableCard
    {
        [Constructable]
        public CollectableCardCavernsOfPoseidon() : base(
                "Caverns of Poseidon",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                437,
                "Underwater Realm of Krarens and Sea Serpents"
                ) {}

        public CollectableCardCavernsOfPoseidon(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDragonsMaw : CollectableCard
    {
        [Constructable]
        public CollectableCardDragonsMaw() : base(
                "Dragon's Maw",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                438,
                "Seething Caldera"
                ) {}

        public CollectableCardDragonsMaw(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardGlacialHills : CollectableCard
    {
        [Constructable]
        public CollectableCardGlacialHills() : base(
                "Glacial Hills",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                439,
                "Hardy Frontier Town Known for its Fierce Warriors"
                ) {}

        public CollectableCardGlacialHills(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardKrakenReefDocks : CollectableCard
    {
        [Constructable]
        public CollectableCardKrakenReefDocks() : base(
                "Kraken Reef Docks",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                440,
		"The Last Port Before the Open Sea"
                ) {}

        public CollectableCardKrakenReefDocks(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardLodoria : CollectableCard
    {
        [Constructable]
        public CollectableCardLodoria() : base(
                "Lodoria",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                441,
		"A Bustling Elven Capital Known for the Castle of Knowledge"
                ) {}

        public CollectableCardLodoria(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardRavendark : CollectableCard
    {
        [Constructable]
        public CollectableCardRavendark() : base(
                "Ravendark",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                442,
                "Cursed Village Perpetually Shrouded in Darkness"
                ) {}

        public CollectableCardRavendark(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSpringvale : CollectableCard
    {
        [Constructable]
        public CollectableCardSpringvale() : base(
                "Springvale",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                443,
                "Peaceful Settlement Built Around Natural Thermal Waters"
                ) {}

        public CollectableCardSpringvale(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardUnderworld : CollectableCard
    {
        [Constructable]
        public CollectableCardUnderworld() : base(
                "The Underworld",
                "Dungeons",
                CardRarity.Legendary,
                CardCategory.Dungeon,
                444,
                "A Massive, Sprawling Dungeon of Dark Mysteries"
                ) {}

        public CollectableCardUnderworld(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWhisper : CollectableCard
    {
        [Constructable]
        public CollectableCardWhisper() : base(
                "Whisper",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                445,
                "Quiet and Serene Village"
                ) {}

        public CollectableCardWhisper(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardZealanTombs : CollectableCard
    {
        [Constructable]
        public CollectableCardZealanTombs() : base(
                "Zealan Tombs",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                446,
                "Burial Complex of a Fallen Empire"
                ) {}

        public CollectableCardZealanTombs(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardCovetous : CollectableCard
    {
        [Constructable]
        public CollectableCardCovetous() : base(
                "Covetous",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                447,
                "Where the Avaricious are Tortured Forever"
                ) {}

        public CollectableCardCovetous(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDeceit : CollectableCard
    {
        [Constructable]
        public CollectableCardDeceit() : base(
                "Deceit",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                448,
                "A Deceptive Maze for the Deceitful"
                ) {}

        public CollectableCardDeceit(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDespise : CollectableCard
    {
        [Constructable]
        public CollectableCardDespise() : base(
                "Despise",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                449,
                "Home to the Spiteful and Vengeful"
                ) {}

        public CollectableCardDespise(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDoom : CollectableCard
    {
        [Constructable]
        public CollectableCardDoom() : base(
                "Doom",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                450,
                "Demonic Lair of the Dark Father Himself"
                ) {}

        public CollectableCardDoom(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardFrozenHells : CollectableCard
    {
        [Constructable]
        public CollectableCardFrozenHells() : base(
                "Frozen Hells",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                451,
                "Blizzard-Wracked Dungeon of Eternal Winter"
                ) {}

        public CollectableCardFrozenHells(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardHythloth : CollectableCard
    {
        [Constructable]
        public CollectableCardHythloth() : base(
                "Hythloth",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                452,
                "Abyssal Gateway to Hell"
                ) {}

        public CollectableCardHythloth(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardShame : CollectableCard
    {
        [Constructable]
        public CollectableCardShame() : base(
                "Shame",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                453,
                "Where Pride is Shattered by Savage Beasts"
                ) {}

        public CollectableCardShame(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardStonegateCastle : CollectableCard
    {
        [Constructable]
        public CollectableCardStonegateCastle() : base(
                "Stonegate Castle",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                454,
		"Once a Mighty Fortress, Now a Crumbling Haunt of Fallen Knights"
                ) {}

        public CollectableCardStonegateCastle(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardUndermountain : CollectableCard
    {
        [Constructable]
        public CollectableCardUndermountain() : base(
                "Undermountain",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                455,
                "Vast Network of Abandoned Tunnels"
                ) {}

        public CollectableCardUndermountain(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardVolcanicCaves : CollectableCard
    {
        [Constructable]
        public CollectableCardVolcanicCaves() : base(
                "Volcanic Caves",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                456,
                "Molten-Core Tunnels of Fire Daemons"
                ) {}

        public CollectableCardVolcanicCaves(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardEtherealVoid : CollectableCard
    {
        [Constructable]
        public CollectableCardEtherealVoid() : base(
                "Ethereal Void",
                "Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                457,
                "Celestial Framework that Binds Together the Cosmos Itself"
                ) {}

        public CollectableCardEtherealVoid(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWrong : CollectableCard
    {
        [Constructable]
        public CollectableCardWrong() : base(
                "Wrong",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                458,
                "Justice Twisted into Cruel Punishment"
                ) {}

        public CollectableCardWrong(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAnkh : CollectableCard
    {
        [Constructable]
        public CollectableCardAnkh() : base(
                "Ankh",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                459,
		"A Crypt Holding the Power of the Symbol of Resurrection"
                ) {}

        public CollectableCardAnkh(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardBane : CollectableCard
    {
        [Constructable]
        public CollectableCardBane() : base(
                "Bane",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                460,
		"A Venom-Filled Chasm, the Doom of Many Adventurers"
                ) {}

        public CollectableCardBane(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardChampionSpawn : CollectableCard
    {
        [Constructable]
        public CollectableCardChampionSpawn() : base(
                "Champion Spawn",
                "Server Event",
                CardRarity.Epic,
                CardCategory.ServerEvent,
                461,
                "Challenge the Entire Army of Powerful Foes"
                ) {}

        public CollectableCardChampionSpawn(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardFloodedTemple : CollectableCard
    {
        [Constructable]
        public CollectableCardFloodedTemple() : base(
                "Flooded Temple",
                "Dungeon",
                CardRarity.Rare,
                CardCategory.Dungeon,
                462,
		"Ancient Shrine Submerged for Millennia"
                ) {}

        public CollectableCardFloodedTemple(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardForgeOfFire : CollectableCard
    {
        [Constructable]
        public CollectableCardForgeOfFire() : base(
                "The Great Forge of Fire",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                463,
		"Primordial Smithy Where the Titans Forged Weapons of Immortal Might"
                ) {}

        public CollectableCardForgeOfFire(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardGauntlet : CollectableCard
    {
        [Constructable]
        public CollectableCardGauntlet() : base(
                "Gauntlet",
                "Server Event",
                CardRarity.Rare,
                CardCategory.ServerEvent,
                464,
                "A Low Risk, High Reward Arena"
                ) {}

        public CollectableCardGauntlet(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardHate : CollectableCard
    {
        [Constructable]
        public CollectableCardHate() : base(
                "Hate",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                465,
		"A Vile Prison Where Demons Feed on the Anger of the Damned"
                ) {}

        public CollectableCardHate(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardLichTower : CollectableCard
    {
        [Constructable]
        public CollectableCardLichTower() : base(
                "Lich Tower",
                "Legends",
                CardRarity.Rare,
                CardCategory.Legends,
                466,
                "Desecrated Tower of Black Magic"
                ) {}

        public CollectableCardLichTower(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMangarTheDark : CollectableCard
    {
        [Constructable]
        public CollectableCardMangarTheDark() : base(
                "Mangar the Dark",
                "Boss",
                CardRarity.Epic,
                CardCategory.Boss,
                467,
                "The Most Wanted Villain of Skara Brae"
                ) {}

        public CollectableCardMangarTheDark(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardScorn : CollectableCard
    {
        [Constructable]
        public CollectableCardScorn() : base(
                "Scorn",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                468,
		"A Labyrinth That Show Visitors Their Deepest Failures"
                ) {}

        public CollectableCardScorn(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSerpentSanctum : CollectableCard
    {
        [Constructable]
        public CollectableCardSerpentSanctum() : base(
                "Serpent Sanctum",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                469,
		"Hidden Temple of the Serpentile Worship"
                ) {}

        public CollectableCardSerpentSanctum(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardChroniclesOfSkara: CollectableCard
    {
        [Constructable]
        public CollectableCardChroniclesOfSkara() : base(
                "Chronicles of Skara Brae",
                "History",
                CardRarity.Rare,
                CardCategory.History,
                470,
                "Signed By Those Who Were on the Quest Before"
                ) {}

        public CollectableCardChroniclesOfSkara(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardStygianAbyss : CollectableCard
    {
        [Constructable]
        public CollectableCardStygianAbyss() : base(
                "Stygian Abyss",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                471,
                "A Dungeon That Is As Dark As It Is Immense"
                ) {}

        public CollectableCardStygianAbyss(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTorment : CollectableCard
    {
        [Constructable]
        public CollectableCardTorment() : base(
                "Torment",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                472,
                "A Place of Eternal Torture and Torment"
                ) {}

        public CollectableCardTorment(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardVile : CollectableCard
    {
        [Constructable]
        public CollectableCardVile() : base(
                "Vile",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                473,
		"A Plague-Ridden Catacomb Where Corruption Seeps Through Every Stone"
                ) {}

        public CollectableCardVile(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWicked : CollectableCard
    {
        [Constructable]
        public CollectableCardWicked() : base(
                "Wicked",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                474,
		"A Twisted Dungeon of Cruel Traps"
                ) {}

        public CollectableCardWicked(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWrath : CollectableCard
    {
        [Constructable]
        public CollectableCardWrath() : base(
                "Wrath",
                "Dungeon",
                CardRarity.Epic,
                CardCategory.Dungeon,
                475,
                "An Underground Rift of Cruelty and Despair"
                ) {}

        public CollectableCardWrath(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTavern : CollectableCard
    {
        [Constructable]
        public CollectableCardTavern() : base(
                "The Tavern",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                476,
                "Bottoms Up!"
                ) {}

        public CollectableCardTavern(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardCasino : CollectableCard
    {
        [Constructable]
        public CollectableCardCasino() : base(
                "The Casino",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                477,
                "Come Blow Through Your Fortunes!"
                ) {}

        public CollectableCardCasino(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDracula : CollectableCard
    {
        [Constructable]
        public CollectableCardDracula() : base(
                "Dracula",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                478,
                "Master Vampire, Ruler of the Night"
                ) {}

        public CollectableCardDracula(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardExodus : CollectableCard
    {
        [Constructable]
        public CollectableCardExodus() : base(
                "Exodus",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                479,
                "Mechanical Spawn of Mondain and Minax"
                ) {}

        public CollectableCardExodus(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTitanHydros : CollectableCard
    {
        [Constructable]
        public CollectableCardTitanHydros() : base(
                "Hydros the Titan of Water",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                480,
                "Ruler of the Deep"
                ) {}

        public CollectableCardTitanHydros(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTitanLithos : CollectableCard
    {
        [Constructable]
        public CollectableCardTitanLithos() : base(
                "Lithos the Titan of Earth",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                481,
                "The World-Shaker"
                ) {}

        public CollectableCardTitanLithos(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTitanPyros : CollectableCard
    {
        [Constructable]
        public CollectableCardTitanPyros() : base(
                "Pyros the Titan of Fire",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                482,
                "The Inferno Incarnate"
                ) {}

        public CollectableCardTitanPyros(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTitanStratos : CollectableCard
    {
        [Constructable]
        public CollectableCardTitanStratos() : base(
                "Stratos the Titan of Air",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                483,
                "Ruler of Storms, Bringer of Hurricanes"
                ) {}

        public CollectableCardTitanStratos(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWidowsKeep : CollectableCard
    {
        [Constructable]
        public CollectableCardWidowsKeep() : base(
                "Widow's Keep",
                "Dungeon",
                CardRarity.Legendary,
                CardCategory.Dungeon,
                484,
                "The Endless Source of the Infected"
                ) {}

        public CollectableCardWidowsKeep(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWidow : CollectableCard
    {
        [Constructable]
        public CollectableCardWidow() : base(
                "The Infected Widow",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                485,
                "Incarnation of the Zombie Infection"
                ) {}

        public CollectableCardWidow(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSlasherOfVoid : CollectableCard
    {
        [Constructable]
        public CollectableCardSlasherOfVoid() : base(
                "Slasher of Void",
                "Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                486,
                "A Wyrm that Rends the Fabric of Reality"
                ) {}

        public CollectableCardSlasherOfVoid(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSerpentPillars : CollectableCard
    {
        [Constructable]
        public CollectableCardSerpentPillars() : base(
                "Serpent Pillars",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                487,
                "Towering Mysterious Portal"
                ) {}

        public CollectableCardSerpentPillars(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardElidor : CollectableCard
    {
        [Constructable]
        public CollectableCardElidor() : base(
                "Elidor",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                488,
                "Secluded Elven City Untouched by Time and War"
                ) {}

        public CollectableCardElidor(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardIslegem : CollectableCard
    {
        [Constructable]
        public CollectableCardIslegem() : base(
                "Islegem",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                489,
                "Settlement Famous for its Precious Stones"
                ) {}

        public CollectableCardIslegem(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardPortshine : CollectableCard
    {
        [Constructable]
        public CollectableCardPortshine() : base(
                "Portshine",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                490,
                "Vibrant Port Town with Markets from Across the World"
                ) {}

        public CollectableCardPortshine(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardStarguide : CollectableCard
    {
        [Constructable]
        public CollectableCardStarguide() : base(
                "The Port of Starguide",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                491,
                "Home to the Realm's Most Skilled Ship Captains"
                ) {}

        public CollectableCardStarguide(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardGuardiansChamber : CollectableCard
    {
        [Constructable]
        public CollectableCardGuardiansChamber() : base(
                "Guardian's Chamber",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                492,
                "The Lair of the Guardian Himself"
                ) {}

        public CollectableCardGuardiansChamber(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardHallOfLegends : CollectableCard
    {
        [Constructable]
        public CollectableCardHallOfLegends() : base(
                "Hall of Legends",
                "Legends",
                CardRarity.Rare,
                CardCategory.History,
                493,
                "Temple Honoring Sosaria's Greatest Champions"
                ) {}

        public CollectableCardHallOfLegends(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAnchorRockDocks : CollectableCard
    {
        [Constructable]
        public CollectableCardAnchorRockDocks() : base(
                "Anchor Rock Docks",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                494,
                "Meeting Place of Sailors and Pirates"
                ) {}

        public CollectableCardAnchorRockDocks(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardVirtualTrade : CollectableCard
    {
        [Constructable]
        public CollectableCardVirtualTrade() : base(
                "Player Vendor Expansion",
                "History",
                CardRarity.Uncommon,
                CardCategory.History,
                495,
                "Vendors Now Trade to Virtual Adventurers"
                ) {}

        public CollectableCardVirtualTrade(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardFinalTwistsHouse : CollectableCard
    {
        [Constructable]
        public CollectableCardFinalTwistsHouse() : base(
                "FinalTwist's House Museum",
                "History",
                CardRarity.Legendary,
                CardCategory.History,
                496,
                "Decayed Once, Yet Forever Standing, Timeless"
                ) {}

        public CollectableCardFinalTwistsHouse(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSigilOfTheBalance : CollectableCard
    {
        [Constructable]
        public CollectableCardSigilOfTheBalance() : base(
                "Sigil of the Balance",
                "History",
                CardRarity.Epic,
                CardCategory.History,
                497,
                "Reflection of the Ever-Shifting Balance"
                ) {}

        public CollectableCardSigilOfTheBalance(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardLostGlade : CollectableCard
    {
        [Constructable]
        public CollectableCardLostGlade() : base(
                "Lost Glade",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                498,
                "Who Says Tamers are Overpowered?"
                ) {}

        public CollectableCardLostGlade(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWizardsGuild : CollectableCard
    {
        [Constructable]
        public CollectableCardWizardsGuild() : base(
                "Wizards' Guild",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                499,
                "School of Wizardry and Spellcasting"
                ) {}

        public CollectableCardWizardsGuild(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardThievesGuild : CollectableCard
    {
        [Constructable]
        public CollectableCardThievesGuild() : base(
                "Thieves Guild",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                399,
                "Underground Headquarters of Rogues"
                ) {}

        public CollectableCardThievesGuild(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardPlayerHousing : CollectableCard
    {
        [Constructable]
        public CollectableCardPlayerHousing() : base(
                "Housing Expansion",
                "History",
                CardRarity.Rare,
                CardCategory.History,
                398,
                "Players Now Use Resources to Build Their Houses"
                ) {}

        public CollectableCardPlayerHousing(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardLadyIllundai : CollectableCard
    {
        [Constructable]
        public CollectableCardLadyIllundai() : base(
                "Illundai, Elder Beastmaster",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                392,
                "The Illustrious Lady Illundai"
                ) {}

        public CollectableCardLadyIllundai(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardBeejeemon : CollectableCard
    {
        [Constructable]
        public CollectableCardBeejeemon() : base(
                "Beejeemon, Renowned Naturalist",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                393,
                "The Legendary Beejeemon"
                ) {}

        public CollectableCardBeejeemon(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSilkySnow : CollectableCard
    {
        [Constructable]
        public CollectableCardSilkySnow() : base(
                "SilkySnow, Grandmaster Skulker",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                394,
                "The Admirable SilkySnow"
                ) {}

        public CollectableCardSilkySnow(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAgetian : CollectableCard
    {
        [Constructable]
        public CollectableCardAgetian() : base(
                "Agetian, Legendary Mage",
                "Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                395,
                "The Illustrious Agetian"
                ) {}

        public CollectableCardAgetian(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardFinalTwist : CollectableCard
    {
        [Constructable]
        public CollectableCardFinalTwist() : base(
                "Lord FinalTwist the Admin",
                "Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                396,
                "The Mastermind Who Started Ultima Adventures"
                ) {}

        public CollectableCardFinalTwist(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardGarthavan : CollectableCard
    {
        [Constructable]
        public CollectableCardGarthavan() : base(
                "Garthavan, Grandmaster Scholar",
                "Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                397,
                "The Renowned Garthavan"
                ) {}

        public CollectableCardGarthavan(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSygun : CollectableCard
    {
        [Constructable]
        public CollectableCardSygun() : base(
                "Sygun, Grandmaster Archer",
                "Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                391,
                "Dread Lord Sygun"
                ) {}

        public CollectableCardSygun(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardHalloweenAdventures : CollectableCard
    {
        [Constructable]
        public CollectableCardHalloweenAdventures() : base(
                "Halloween Adventures",
                "Server Event",
                CardRarity.Uncommon,
                CardCategory.ServerEvent,
                390,
                "Yearly Event with Thematic Decorations"
                ) {}

        public CollectableCardHalloweenAdventures(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardZombieInvasion : CollectableCard
    {
        [Constructable]
        public CollectableCardZombieInvasion() : base(
                "Zombie Invasion",
                "Server Event",
                CardRarity.Rare,
                CardCategory.ServerEvent,
                389,
                "When Players Stand United Against the Zombie Apocalypse"
                ) {}

        public CollectableCardZombieInvasion(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMassacreBonyHand : CollectableCard
    {
        [Constructable]
        public CollectableCardMassacreBonyHand() : base(
                "Massacre of the Bony Hand",
                "June 18, 2020",
                CardRarity.Epic,
                CardCategory.ServerEvent,
                388,
                "Revenge of the Red Fraternity"
                ) {}

        public CollectableCardMassacreBonyHand(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTsai : CollectableCard
    {
        [Constructable]
        public CollectableCardTsai() : base(
                "Admin Tsai, Legendary Brawler",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                387,
                "The Evil Lord Admin Tsai"
                ) {}

        public CollectableCardTsai(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSarthMarket : CollectableCard
    {
        [Constructable]
        public CollectableCardSarthMarket() : base(
                "Sarth Abbey Market",
                "Town",
                CardRarity.Common,
                CardCategory.Town,
                380,
                "The Center of Sosarian Trade"
                ) {}

        public CollectableCardSarthMarket(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardRoadConstruction : CollectableCard
    {
        [Constructable]
        public CollectableCardRoadConstruction() : base(
                "Sosarian Roads Expansion",
                "History",
                CardRarity.Rare,
                CardCategory.History,
                381,
                "Roads are Built for Safer Travel in Sosaria"
                ) {}

        public CollectableCardRoadConstruction(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardCodexOfWisdom : CollectableCard
    {
        [Constructable]
        public CollectableCardCodexOfWisdom() : base(
                "Codex of Ultimate Wisdom",
                "Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                382,
                "Magical Tome of All Knowledge in Existence"
                ) {}

        public CollectableCardCodexOfWisdom(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardThePit : CollectableCard
    {
        [Constructable]
        public CollectableCardThePit() : base(
                "The Pit",
                "Town",
                CardRarity.Rare,
                CardCategory.Town,
                383,
                "The Reds Need a Place to Call Home Too"
                ) {}

        public CollectableCardThePit(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardXardoksCastle : CollectableCard
    {
        [Constructable]
        public CollectableCardXardoksCastle() : base(
                "Xardok's Castle",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                386,
		"Impenetrable Fortress of Mysteries Untold"
                ) {}

        public CollectableCardXardoksCastle(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardCrashSite : CollectableCard
    {
        [Constructable]
        public CollectableCardCrashSite() : base(
                "The Crash Site",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                385,
                "Because in Tough Times, Every Land Needs Aliens"
                ) {}

        public CollectableCardCrashSite(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardBasement : CollectableCard
    {
        [Constructable]
        public CollectableCardBasement() : base(
                "The Hidden Basement",
                "Town",
                CardRarity.Epic,
                CardCategory.Town,
                384,
                "Only For Those Who Know the Secrets of the Soulbound"
                ) {}

        public CollectableCardBasement(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSythHolocron : CollectableCard
    {
        [Constructable]
        public CollectableCardSythHolocron() : base(
                "The Syth Holocron",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                379,
                "Feel the Power of the Dark Side of the Force?"
                ) {}

        public CollectableCardSythHolocron(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardBritainMarket : CollectableCard
    {
        [Constructable]
        public CollectableCardBritainMarket() : base(
                "Britain Market",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                378,
                "Ran By Both NPCs and Player Vendors"
                ) {}

        public CollectableCardBritainMarket(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardCoffee : CollectableCard
    {
        [Constructable]
        public CollectableCardCoffee() : base(
                "The Dread Lady Coffee",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                377,
                "You May Have Seen The Corpse of Coffee Once or Fifty Times... No Relation, Or...?"
                ) {}

        public CollectableCardCoffee(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardHeltsDirtyDishes : CollectableCard
    {
        [Constructable]
        public CollectableCardHeltsDirtyDishes() : base(
                "Helt's Dirty Dishes",
                "History",
                CardRarity.Common,
                CardCategory.History,
                376,
                "Items Don't Decay Here, I Tell You, So Don't Litter!"
                ) {}

        public CollectableCardHeltsDirtyDishes(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardIllundaisGarden : CollectableCard
    {
        [Constructable]
        public CollectableCardIllundaisGarden() : base(
                "Illundai's Garden",
                "Town",
                CardRarity.Epic,
                CardCategory.Town,
                375,
                "A Serene Place in West Gate City"
                ) {}

        public CollectableCardIllundaisGarden(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardPublicTransport : CollectableCard
    {
        [Constructable]
        public CollectableCardPublicTransport() : base(
                "Public Transport Expansion",
                "History",
                CardRarity.Uncommon,
                CardCategory.History,
                374,
                "Players Placed Gates First, Now There Are Wagons and Ferries"
                ) {}

        public CollectableCardPublicTransport(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWestGateCity : CollectableCard
    {
        [Constructable]
        public CollectableCardWestGateCity() : base(
                "West Gate City",
                "Town",
                CardRarity.Uncommon,
                CardCategory.Town,
                373,
                "A Town Founded and Built Entirely By Adventurers"
                ) {}

        public CollectableCardWestGateCity(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMoonsCore : CollectableCard
    {
        [Constructable]
        public CollectableCardMoonsCore() : base(
                "Moon's Core",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                372,
                "The Core of the Moon"
                ) {}

        public CollectableCardMoonsCore(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSacrifice : CollectableCard
    {
        [Constructable]
        public CollectableCardSacrifice() : base(
                "Sacrifice",
                "Legends",
                CardRarity.Uncommon,
                CardCategory.Legends,
                371,
                "This Could Have Been You"
                ) {}

        public CollectableCardSacrifice(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSeaGiantsTreasure : CollectableCard
    {
        [Constructable]
        public CollectableCardSeaGiantsTreasure() : base(
                "Sea Giant's Treasure",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                370,
                "Sparkling Riches in the Underwater Ruins"
                ) {}

        public CollectableCardSeaGiantsTreasure(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardWarpGate : CollectableCard
    {
        [Constructable]
        public CollectableCardWarpGate() : base(
                "Warp Gate",
                "Legends",
                CardRarity.Rare,
                CardCategory.Legends,
                369,
                "A Gate to a Far Off Land"
                ) {}

        public CollectableCardWarpGate(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardMomonos : CollectableCard
    {
        [Constructable]
        public CollectableCardMomonos() : base(
                "Momonos, Elder Veterinarian",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                368,
                "A Tamer Known to Command an Army of Paragon Pets"
                ) {}

        public CollectableCardMomonos(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAsarco : CollectableCard
    {
        [Constructable]
        public CollectableCardAsarco() : base(
                "Asarco, Legendary Tailor",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                367,
                "The Glorious Lord Asarco"
                ) {}

        public CollectableCardAsarco(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardHighSeasPiracy : CollectableCard
    {
        [Constructable]
        public CollectableCardHighSeasPiracy() : base(
                "High Seas Piracy",
                "Server Event",
                CardRarity.Rare,
                CardCategory.ServerEvent,
                366,
                "Fifteen Men on a Dead Man's Chest"
                ) {}

        public CollectableCardHighSeasPiracy(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardNox : CollectableCard
    {
        [Constructable]
        public CollectableCardNox() : base(
                "Nox, Grandmaster Archmage",
                "Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                365,
                "The Evil Nox"
                ) {}

        public CollectableCardNox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTreasureHunting : CollectableCard
    {
        [Constructable]
        public CollectableCardTreasureHunting() : base(
                "Treasure Hunting",
                "Server Event",
                CardRarity.Rare,
                CardCategory.Legends,
                364,
                "\"X\" Marks the Spot!"
                ) {}

        public CollectableCardTreasureHunting(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardChristmasAdventures : CollectableCard
    {
        [Constructable]
        public CollectableCardChristmasAdventures() : base(
                "Christmas Adventures",
                "Server Event",
                CardRarity.Uncommon,
                CardCategory.ServerEvent,
                363,
                "'Tis the Season to Be Jolly"
                ) {}

        public CollectableCardChristmasAdventures(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardJormungandr : CollectableCard
    {
        [Constructable]
        public CollectableCardJormungandr() : base(
                "Jormungandr",
		"Boss",
                CardRarity.Legendary,
                CardCategory.Boss,
                362,
                "Jormungandr the Midgard Serpent, Terror of the High Seas"
                ) {}

        public CollectableCardJormungandr(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardPacem : CollectableCard
    {
        [Constructable]
        public CollectableCardPacem() : base(
                "Pacem, Legendary Blacksmith",
		"Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                361,
                "The Dread Lord Pacem"
                ) {}

        public CollectableCardPacem(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDeides : CollectableCard
    {
        [Constructable]
        public CollectableCardDeides() : base(
                "Deides, Master Beastmaster",
		"Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                360,
                "The Illustrious Deides"
                ) {}

        public CollectableCardDeides(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardDow : CollectableCard
    {
        [Constructable]
        public CollectableCardDow() : base(
                "Dow, Elder Blacksmith",
		"Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                359,
                "The Illustrious Dow"
                ) {}

        public CollectableCardDow(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardPug : CollectableCard
    {
        [Constructable]
        public CollectableCardPug() : base(
                "Pug, Legendary Skulker",
		"Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                358,
                "The Dread Lord Pug"
                ) {}

        public CollectableCardPug(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardBram : CollectableCard
    {
        [Constructable]
        public CollectableCardBram() : base(
                "Bram of the Sapphire Helm",
		"Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                357,
                "The Estimable Bram, Soulbound Eternal"
                ) {}

        public CollectableCardBram(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardVexain : CollectableCard
    {
        [Constructable]
        public CollectableCardVexain() : base(
                "Vexain, Elder Bard",
		"Legends",
                CardRarity.Legendary,
                CardCategory.Legends,
                356,
                "The Glorious Lord Vexain"
                ) {}

        public CollectableCardVexain(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardSurtaz : CollectableCard
    {
        [Constructable]
        public CollectableCardSurtaz() : base(
                "Surtaz the Fallen",
                "Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                355,
                "The Keeper of Unholy Secrets"
                ) {}

        public CollectableCardSurtaz(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardVulcrum : CollectableCard
    {
        [Constructable]
        public CollectableCardVulcrum() : base(
                "Vulcrum of the Flame",
                "Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                354,
                "Living Flame Incarnate"
                ) {}

        public CollectableCardVulcrum(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardArachnar : CollectableCard
    {
        [Constructable]
        public CollectableCardArachnar() : base(
                "Arachnar the Deep Crawler",
                "Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                353,
                "The Menacing Arachnid"
                ) {}

        public CollectableCardArachnar(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardKhumashGor : CollectableCard
    {
        [Constructable]
        public CollectableCardKhumashGor() : base(
                "Khumash-Gor",
                "Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                352,
                "The Zealan Warrior"
                ) {}

        public CollectableCardKhumashGor(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardXurtzar : CollectableCard
    {
        [Constructable]
        public CollectableCardXurtzar() : base(
                "Xurtzar of Demonic Energy",
                "Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                351,
                "Demonic Power Unbound"
                ) {}

        public CollectableCardXurtzar(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardTanaka : CollectableCard
    {
        [Constructable]
        public CollectableCardTanaka() : base(
                "Lady Tanaka the Battler",
		"Legends",
                CardRarity.Epic,
                CardCategory.Legends,
                350,
                "The Dread Lady Tanaka"
                ) {}

        public CollectableCardTanaka(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardShadowWyrm : CollectableCard
    {
        [Constructable]
        public CollectableCardShadowWyrm() : base(
                "Shadow Wyrm",
		"Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                349,
                "Desolation Lurking in the Shadows"
                ) {}

        public CollectableCardShadowWyrm(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAncientLich : CollectableCard
    {
        [Constructable]
        public CollectableCardAncientLich() : base(
                "Ancient Lich",
		"Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                348,
                "Paragon of Undeath"
                ) {}

        public CollectableCardAncientLich(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardAncientWyvern : CollectableCard
    {
        [Constructable]
        public CollectableCardAncientWyvern() : base(
                "Ancient Wyvern",
		"Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                347,
                "Ageless Lurker of the Deepest Caverns"
                ) {}

        public CollectableCardAncientWyvern(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardLichKing : CollectableCard
    {
        [Constructable]
        public CollectableCardLichKing() : base(
                "Lich King",
		"Boss",
                CardRarity.Rare,
                CardCategory.Boss,
                345,
                "The King of the Dead"
                ) {}

        public CollectableCardLichKing(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardStygianDragon : CollectableCard
    {
        [Constructable]
        public CollectableCardStygianDragon() : base(
                "Stygian Dragon",
		"Boss",
                CardRarity.Epic,
                CardCategory.Boss,
                344,
                "Guardian of the Untold Treasures of the Underworld"
                ) {}

        public CollectableCardStygianDragon(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class CollectableCardThoroar : CollectableCard
    {
        [Constructable]
        public CollectableCardThoroar() : base(
                "Thoroar",
		"Boss",
                CardRarity.Epic,
                CardCategory.Boss,
                343,
                "A Translucent Demonic Abomination"
                ) {}

        public CollectableCardThoroar(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}
