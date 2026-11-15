using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{  
	public class WidowCloak : BaseCloak
	{

		//public static DateTime speciallastused;

		[Constructable]
		public WidowCloak() : this( 0 )
		{
		}

		[Constructable]
        public WidowCloak( int hue ) : base( 11012 )
        {
                Weight = 4.0;
                Name = "Morphing cloak of the Widow";
                //speciallastused = DateTime.UtcNow;

        }

               public override bool OnEquip( Mobile from )
               {

                       Item chest = from.FindItemOnLayer( Layer.InnerTorso );
                       Item glove = from.FindItemOnLayer( Layer.Gloves );
                       Item pants = from.FindItemOnLayer( Layer.Pants );
                       Item neck = from.FindItemOnLayer( Layer.Neck );
                       Item helm = from.FindItemOnLayer( Layer.Helm );
                       Item arms = from.FindItemOnLayer( Layer.Arms );
                       Item twohanded = from.FindItemOnLayer( Layer.TwoHanded );
                       Item firstvalid = from.FindItemOnLayer( Layer.FirstValid );

                       if ( from.Mounted == true )
                       {
                               from.SendMessage( "You cannot be mounted while wearing this armor." );
                               return false;
                       }
                       if ( from.BodyMod != 0 )
                       {
                               from.SendMessage( "You cannot this armor while your body is transformed!" );
                               return false;
                       }
                       if ( firstvalid != null || twohanded != null )
                       {
                               from.SendMessage( "You cannot this armor while your hands are holding an item." );
                               return false;
                       }

                       if ( chest != null && chest.GetType() == typeof( WidowMorphChest ) &&
                               glove != null && glove.GetType() == typeof( WidowMorphGloves ) &&
                               pants != null && pants.GetType() == typeof( WidowMorphLegs ) &&
                               helm != null && helm.GetType() == typeof( WidowMorphHelm ) &&
                               neck != null && neck.GetType() == typeof( WidowMorphGorget ) &&
                               arms != null && arms.GetType() == typeof( WidowMorphArms ) )
                       {
                               Effects.PlaySound( from.Location, from.Map, 503 );
                               from.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );

                               if (from is PlayerMobile)
                               {
                                       from.SendMessage( "You Morph!" );
                                       from.BodyMod = 84;
                                       from.HueMod = 1931;
                                       from.NameHue = 39;

                               }

                               WidowMorphChest chestPiece = chest as WidowMorphChest;
                               WidowMorphGloves glovePiece = glove as WidowMorphGloves;
                               WidowMorphLegs legPiece = pants as WidowMorphLegs;
                               WidowMorphGorget gorgetPiece = neck as WidowMorphGorget;
                               WidowMorphHelm helmPiece = helm as WidowMorphHelm;
                               WidowMorphArms armPiece = arms as WidowMorphArms;

                               chestPiece.Hue = 0x47E;
                               chestPiece.Attributes.NightSight = 1;
                               chestPiece.Attributes.DefendChance = 20;
                               chestPiece.ArmorAttributes.SelfRepair = 3;
                               chestPiece.SkillBonuses.SetValues( 0, SkillName.Magery, 50.0 );
                               chestPiece.Attributes.EnhancePotions = 50;
                               chestPiece.PhysicalBonus = 17;
                               chestPiece.FireBonus = 7;
                               chestPiece.ColdBonus = 17;
                               chestPiece.PoisonBonus = 17;
                               chestPiece.EnergyBonus = 10;

                               glovePiece.Hue = 0x47E;
                               glovePiece.Attributes.NightSight = 1;
                               glovePiece.ArmorAttributes.SelfRepair = 3;
                               glovePiece.Attributes.BonusStr = -30;
                               glovePiece.PhysicalBonus = 17;
                               glovePiece.FireBonus = 7;
                               glovePiece.ColdBonus = 17;
                               glovePiece.PoisonBonus = 17;
                               glovePiece.EnergyBonus = 10;

                               legPiece.Hue = 0x47E;
                               legPiece.Attributes.NightSight = 1;
                               legPiece.ArmorAttributes.SelfRepair = 3;
                               legPiece.Attributes.BonusDex = 15;
                               legPiece.Attributes.BonusMana = 50;
                               legPiece.Attributes.BonusInt = 50;
                               legPiece.PhysicalBonus = 17;
                               legPiece.FireBonus = 7;
                               legPiece.ColdBonus = 17;
                               legPiece.PoisonBonus = 17;
                               legPiece.EnergyBonus = 10;

                               helmPiece.Hue = 0x47E;
                               helmPiece.Attributes.NightSight = 1;
                               helmPiece.ArmorAttributes.SelfRepair = 3;
                               helmPiece.Attributes.BonusHits = 40;
                               helmPiece.Attributes.CastRecovery = 4;
                               helmPiece.Attributes.CastSpeed = 4;
                               helmPiece.PhysicalBonus = 17;
                               helmPiece.FireBonus = 7;
                               helmPiece.ColdBonus = 17;
                               helmPiece.PoisonBonus = 17;
                               helmPiece.EnergyBonus = 10;

                               gorgetPiece.Hue = 0x47E;
                               gorgetPiece.Attributes.NightSight = 1;
                               gorgetPiece.ArmorAttributes.SelfRepair = 3;
                               gorgetPiece.Attributes.RegenMana = 35;
                               gorgetPiece.PhysicalBonus = 17;
                               gorgetPiece.FireBonus = 7;
                               gorgetPiece.ColdBonus = 17;
                               gorgetPiece.PoisonBonus = 17;
                               gorgetPiece.EnergyBonus = 10;

                               armPiece.Hue = 0x47E;
                               armPiece.ArmorAttributes.SelfRepair = 3;
                               armPiece.Attributes.RegenStam = 10;
                               armPiece.Attributes.NightSight = 1;
                               armPiece.Attributes.SpellDamage = 500;
                               armPiece.Attributes.LowerRegCost = 100;
                               armPiece.Attributes.LowerManaCost = 50;
                               armPiece.PhysicalBonus = 17;
                               armPiece.FireBonus = 7;
                               armPiece.ColdBonus = 17;
                               armPiece.PoisonBonus = 17;
                               armPiece.EnergyBonus = 10;
                               armPiece.Attributes.Luck = 500;

                               from.SendLocalizedMessage( 1072391 );
                       }
                       this.InvalidateProperties();
                       return base.OnEquip( from );
               }

		public override void OnRemoved(IEntity parent )
		{
			if ( parent is Mobile )
			{
				
				Mobile m = (Mobile)parent;
				if ( m.FindItemOnLayer( Layer.InnerTorso ) is WidowMorphChest 
				&& m.FindItemOnLayer( Layer.Gloves ) is WidowMorphGloves 
				&& m.FindItemOnLayer( Layer.Arms ) is WidowMorphArms 
				&& m.FindItemOnLayer( Layer.Neck ) is WidowMorphGorget 
				&& m.FindItemOnLayer( Layer.Pants ) is WidowMorphLegs 
				&& m.FindItemOnLayer( Layer.Helm ) is WidowMorphHelm )
				{


				if (m is PlayerMobile)
				{
					m.SendMessage( "You remove the armor." );
					m.BodyMod = 0;
					m.NameHue = -1;
					m.HueMod = -1;
				}
				
					WidowMorphChest chest = m.FindItemOnLayer( Layer.InnerTorso ) as WidowMorphChest;
					chest.Hue = 0x0;
					chest.Attributes.NightSight = 0;
					chest.Attributes.DefendChance = 0;
					chest.ArmorAttributes.SelfRepair = 0;
					chest.SkillBonuses.SetValues( 0, SkillName.Magery, 0.0 );
					chest.Attributes.EnhancePotions = 0;
					chest.PhysicalBonus = 0;
					chest.FireBonus = 0;
					chest.ColdBonus = 0;
					chest.PoisonBonus = 0;
					chest.EnergyBonus = 0;

					WidowMorphGloves gloves = m.FindItemOnLayer( Layer.Gloves ) as WidowMorphGloves;
					gloves.Hue = 0x0;
					gloves.Attributes.NightSight = 0;
					gloves.ArmorAttributes.SelfRepair = 0;
					gloves.Attributes.BonusStr = 0;
					gloves.PhysicalBonus = 0;
					gloves.FireBonus = 0;
					gloves.ColdBonus = 0;
					gloves.PoisonBonus = 0;
					gloves.EnergyBonus = 0;				
					
				
					WidowMorphLegs legs = m.FindItemOnLayer( Layer.Pants ) as WidowMorphLegs;
					legs.Hue = 0x0;
					legs.Attributes.NightSight = 0;
					legs.ArmorAttributes.SelfRepair = 0;
					legs.Attributes.BonusDex = 0;
					legs.Attributes.BonusMana = 0;
					legs.Attributes.BonusInt = 0;
					legs.PhysicalBonus = 0;
					legs.FireBonus = 0;
					legs.ColdBonus = 0;
					legs.PoisonBonus = 0;
					legs.EnergyBonus = 0;


					WidowMorphGorget gorget = m.FindItemOnLayer( Layer.Neck ) as WidowMorphGorget;
					gorget.Hue = 0x0;
					gorget.Attributes.NightSight = 0;
					gorget.ArmorAttributes.SelfRepair = 0;
					gorget.Attributes.RegenMana = 0;
					gorget.PhysicalBonus = 0;
					gorget.FireBonus = 0;
					gorget.ColdBonus = 0;
					gorget.PoisonBonus = 0;
					gorget.EnergyBonus = 0;

					WidowMorphHelm helmet = m.FindItemOnLayer( Layer.Helm ) as WidowMorphHelm;
					helmet.Hue = 0x0;
					helmet.Attributes.NightSight = 0;
					helmet.ArmorAttributes.SelfRepair = 0;
					helmet.Attributes.CastRecovery = 0;
					helmet.Attributes.CastSpeed = 0;
					helmet.PhysicalBonus = 0;
					helmet.FireBonus = 0;
					helmet.ColdBonus = 0;
					helmet.PoisonBonus = 0;
					helmet.EnergyBonus = 0;

					WidowMorphArms arm = m.FindItemOnLayer( Layer.Arms ) as WidowMorphArms;
					arm.Hue = 0x0;
					arm.Attributes.NightSight = 0;
					arm.ArmorAttributes.SelfRepair = 0;
					arm.Attributes.RegenStam = 0;
					arm.Attributes.SpellDamage = 0;
					arm.PhysicalBonus = 0;
					arm.FireBonus = 0;
					arm.ColdBonus = 0;
					arm.PoisonBonus = 0;
					arm.EnergyBonus = 0;
					arm.Attributes.Luck = 0;
					
				this.InvalidateProperties();
				}
				base.OnRemoved( parent );
			}
		}


		public WidowCloak( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
			//writer.Write( (DateTime)speciallastused );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			//speciallastused = reader.ReadDateTime();
		}
	}
}
