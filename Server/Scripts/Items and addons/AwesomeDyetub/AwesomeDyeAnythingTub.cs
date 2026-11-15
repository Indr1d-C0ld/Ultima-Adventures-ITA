using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;
using System;

namespace Server.Items
{
    public class AwesomeDyeNearlyAnythingTub : AwesomeDyeTub
    {
        [Constructable]
        public AwesomeDyeNearlyAnythingTub()
        {
            Name = "a bubbling tub";

            m_HueDelay = 0.3;
            m_HueStart = 0;
            m_HueStop = 900;
            m_IsCycling = false;
            CostRunebookDye = 50000;
            CostFurnitureDye = 50000;
            CostStatuettesDye = 50000;
            CostLeatherDye = 50000;
            CostMetalDye = 50000;
            CostBackpackDye = 50000;
            CostContainerDye = 50000;
            CostDyablesDye = 50000;
            CostMiscDye = 50000;
            AllowLeather = true;
            AllowMetal = true;
            AllowContainer = true;
        }

        public AwesomeDyeNearlyAnythingTub(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(this.GetWorldLocation(), 2) && from.AccessLevel < AccessLevel.GameMaster)
            {
                from.SendMessage("The dye tub is too far away...");
            }
            else
            {
                from.CloseGump(typeof(AwesomeDyeTubGump));
                from.SendGump(new AwesomeDyeTubGump(from, this));
                from.SendMessage(240, from.Name + ", this dye tub can paint most cloth, leather and metal objects, but it needs a material component and thus requires 50000 gold per use!");
            }
        }
    }
}
