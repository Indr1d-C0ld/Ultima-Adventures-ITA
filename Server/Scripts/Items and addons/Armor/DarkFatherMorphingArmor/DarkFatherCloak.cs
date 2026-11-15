using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{  
        public class DarkFatherCloak : BaseCloak
        {

		//public static DateTime speciallastused;

		[Constructable]
		public DarkFatherCloak() : this( 0 )
		{
		}

		[Constructable]
		public DarkFatherCloak( int hue ) : base( 11012 )
		{
			Weight = 4.0;
			Name = "Morphing cloak of the Dark Father";
			//speciallastused = DateTime.UtcNow;
			
		}
                /*
                public static void UseMinotaurAbility( Mobile from )
                {

                        if (from != null)
                        {
                                        if (DateTime.UtcNow > ( speciallastused + TimeSpan.FromHours( 24.0 )) );
                                        {
                                                speciallastused = DateTime.UtcNow;

                                                double seconds = 8;

                                                Map map = from.Map;

                                                if ( map != null )
                                                {

                                                        foreach ( Mobile m in from.GetMobilesInRange( 10 ) )
                                                        {
                                                                if ((m is BaseCreature && ((BaseCreature)m).Uncalmable) || (m is BaseCreature && ((BaseCreature)m).AreaPeaceImmune) || m == from || !from.CanBeHarmful ( m, false ))
                                                                        continue;

                                                                m.SendLocalizedMessage(1, "You emit a great roar and stun those around you!" );
                                                                m.Combatant = null;
                                                                m.Warmode = false;

                                                                if ( m is BaseCreature && !((BaseCreature)m).BardPacified )
                                                                        ((BaseCreature)m).Pacify( m, DateTime.UtcNow + TimeSpan.FromSeconds( seconds ) );
                                                        }
                                                }
                                        }

                        }
                }
*/

                public override bool OnEquip( Mobile from )
                {
                        Item shirt = from.FindItemOnLayer( Layer.InnerTorso );
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

                        if ( shirt != null && shirt.GetType() == typeof( DarkFatherMorphChest )
                                && glove != null && glove.GetType() == typeof( DarkFatherMorphGloves )
                                && pants != null && pants.GetType() == typeof( DarkFatherMorphLegs )
                                && neck != null && neck.GetType() == typeof( DarkFatherMorphGorget )
                                && helm != null && helm.GetType() == typeof( DarkFatherMorphHelm )
                                && arms != null && arms.GetType() == typeof( DarkFatherMorphArms ) )
                        {
                                Effects.PlaySound( from.Location, from.Map, 503 );
                                from.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );

                                if ( from is PlayerMobile )
                                {
                                        from.SendMessage( "You Morph!" );
                                        from.BodyMod = 318;
                                        from.HueMod = 1931;
                                        from.NameHue = 39;
                                }

                                Hue = 0x47E;

                                DarkFatherMorphChest chest = shirt as DarkFatherMorphChest;
                                DarkFatherMorphGloves gloves = glove as DarkFatherMorphGloves;
                                DarkFatherMorphLegs legs = pants as DarkFatherMorphLegs;
                                DarkFatherMorphGorget gorget = neck as DarkFatherMorphGorget;
                                DarkFatherMorphHelm helmet = helm as DarkFatherMorphHelm;
                                DarkFatherMorphArms arm = arms as DarkFatherMorphArms;

                                chest.Hue = 0x47E;
                                chest.Attributes.NightSight = 1;
                                chest.Attributes.DefendChance = 20;
                                chest.ArmorAttributes.SelfRepair = 3;
                                chest.SkillBonuses.SetValues( 0, SkillName.Necromancy, 50.0 );
                                chest.PhysicalBonus = 10;
                                chest.FireBonus = 17;
                                chest.ColdBonus = 17;
                                chest.PoisonBonus = 10;
                                chest.EnergyBonus = 17;

                                gloves.Hue = 0x47E;
                                gloves.Attributes.NightSight = 1;
                                gloves.ArmorAttributes.SelfRepair = 3;
                                gloves.Attributes.BonusStr = -30;
                                gloves.PhysicalBonus = 10;
                                gloves.FireBonus = 17;
                                gloves.ColdBonus = 17;
                                gloves.PoisonBonus = 10;
                                gloves.EnergyBonus = 17;

                                legs.Hue = 0x47E;
                                legs.Attributes.NightSight = 1;
                                legs.ArmorAttributes.SelfRepair = 3;
                                legs.Attributes.BonusDex = 15;
                                legs.Attributes.BonusMana = 150;
                                legs.Attributes.BonusInt = 50;
                                legs.PhysicalBonus = 10;
                                legs.FireBonus = 17;
                                legs.ColdBonus = 17;
                                legs.PoisonBonus = 10;
                                legs.EnergyBonus = 17;

                                gorget.Hue = 0x47E;
                                gorget.Attributes.NightSight = 1;
                                gorget.ArmorAttributes.SelfRepair = 3;
                                gorget.Attributes.RegenMana = 35;
                                gorget.PhysicalBonus = 10;
                                gorget.FireBonus = 17;
                                gorget.ColdBonus = 17;
                                gorget.PoisonBonus = 10;
                                gorget.EnergyBonus = 17;

                                helmet.Hue = 0x47E;
                                helmet.Attributes.NightSight = 1;
                                helmet.ArmorAttributes.SelfRepair = 3;
                                helmet.Attributes.BonusHits = 40;
                                helmet.Attributes.CastRecovery = 4;
                                helmet.Attributes.CastSpeed = 4;
                                helmet.PhysicalBonus = 10;
                                helmet.FireBonus = 17;
                                helmet.ColdBonus = 17;
                                helmet.PoisonBonus = 10;
                                helmet.EnergyBonus = 17;

                                arm.Hue = 0x47E;
                                arm.ArmorAttributes.SelfRepair = 3;
                                arm.Attributes.RegenStam = 10;
                                arm.Attributes.NightSight = 1;
                                arm.Attributes.SpellDamage = 300;
                                arm.Attributes.LowerRegCost = 100;
                                arm.Attributes.LowerManaCost = 50;
                                arm.PhysicalBonus = 10;
                                arm.FireBonus = 17;
                                arm.ColdBonus = 17;
                                arm.PoisonBonus = 10;
                                arm.EnergyBonus = 17;
                                arm.Attributes.Luck = 4000;

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
				if ( m.FindItemOnLayer( Layer.InnerTorso ) is DarkFatherMorphChest 
				&& m.FindItemOnLayer( Layer.Gloves ) is DarkFatherMorphGloves 
				&& m.FindItemOnLayer( Layer.Arms ) is DarkFatherMorphArms 
				&& m.FindItemOnLayer( Layer.Neck ) is DarkFatherMorphGorget 
				&& m.FindItemOnLayer( Layer.Pants ) is DarkFatherMorphLegs 
				&& m.FindItemOnLayer( Layer.Helm ) is DarkFatherMorphHelm )
				{

				DarkFatherMorphLegs legs = m.FindItemOnLayer( Layer.Pants ) as DarkFatherMorphLegs;
				if (m is PlayerMobile)
				{
					
					m.SendMessage( "You remove the armor." );
					m.BodyMod = 0;
					m.NameHue = -1;
					m.HueMod = -1;
				}
				
					DarkFatherMorphChest chest = m.FindItemOnLayer( Layer.InnerTorso ) as DarkFatherMorphChest;
					chest.Hue = 0x0;
					chest.Attributes.NightSight = 0;
					chest.Attributes.DefendChance = 0;
					chest.ArmorAttributes.SelfRepair = 0;
					chest.SkillBonuses.SetValues( 0, SkillName.Necromancy, 0.0 );
					chest.PhysicalBonus = 0;
					chest.FireBonus = 0;
					chest.ColdBonus = 0;
					chest.PoisonBonus = 0;
					chest.EnergyBonus = 0;

					DarkFatherMorphGloves gloves = m.FindItemOnLayer( Layer.Gloves ) as DarkFatherMorphGloves;
					gloves.Hue = 0x0;
					gloves.Attributes.NightSight = 0;
					gloves.ArmorAttributes.SelfRepair = 0;
					gloves.Attributes.BonusStr = 0;
					gloves.PhysicalBonus = 0;
					gloves.FireBonus = 0;
					gloves.ColdBonus = 0;
					gloves.PoisonBonus = 0;
					gloves.EnergyBonus = 0;				
					
				

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


					DarkFatherMorphGorget gorget = m.FindItemOnLayer( Layer.Neck ) as DarkFatherMorphGorget;
					gorget.Hue = 0x0;
					gorget.Attributes.NightSight = 0;
					gorget.ArmorAttributes.SelfRepair = 0;
					gorget.Attributes.RegenMana = 0;
					gorget.PhysicalBonus = 0;
					gorget.FireBonus = 0;
					gorget.ColdBonus = 0;
					gorget.PoisonBonus = 0;
					gorget.EnergyBonus = 0;

					DarkFatherMorphHelm helmet = m.FindItemOnLayer( Layer.Helm ) as DarkFatherMorphHelm;
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

					DarkFatherMorphArms arm = m.FindItemOnLayer( Layer.Arms ) as DarkFatherMorphArms;
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


		public DarkFatherCloak( Serial serial ) : base( serial )
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
