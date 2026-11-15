using System;
using Server.Items;
using Server.Mobiles;

namespace Server.SkillHandlers
{
	public class Stealth
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Stealth].Callback = new SkillUseCallback( OnUse );
		}

		public static int[,] ArmorTable{ get { return m_ArmorTable; } }
		private static int[,] m_ArmorTable = new int[,]
			{
							//	Gorget	Gloves	Helmet	Arms	Legs	Feet	Chest	Shield
				/* Cloth	*/	{ 0,	0,		0,		1,		1,		0,		2,		0 },
				/* Leather	*/	{ 1,	1,		0,		2,		3,		1,		5,		0 },
				/* Studded	*/	{ 2,	2,		0,		4,		6,		2,		10,		0 },
				/* Bone		*/ 	{ 0,	5,		10,		10,		18,		5,		30,		0 },
				/* Spined	*/	{ 1,	1,		0,		2,		3,		1,		5,		0 },
				/* Horned	*/	{ 1,	1,		0,		2,		4,		1,		6,		0 },
				/* Barbed	*/	{ 2,	2,		0,		3,		5,		2,		7,		0 },
				/* Necrotic	*/	{ 2,	2,		0,		3,		5,		2,		7,		0 },
				/* Volcanic	*/	{ 2,	2,		0,		3,		5,		2,		7,		0 },
				/* Frozen	*/	{ 2,	2,		0,		3,		5,		2,		7,		0 },
				/* Goliath	*/	{ 2,	2,		0,		4,		6,		2,		10,		0 },
				/* Draconic	*/	{ 2,	2,		0,		4,		6,		2,		10,		0 },
				/* Hellish	*/	{ 2,	2,		0,		4,		6,		2,		10,		0 },
				/* Dinosaur	*/	{ 2,	2,		0,		4,		6,		2,		10,		0 },
				/* Alien	*/	{ 2,	2,		0,		4,		6,		2,		10,		0 },
				/* Ring		*/	{ 0,	5,		0,		10,		15,		5,		25,		0 },
				/* Chain	*/	{ 0,	0,		10,		0,		15,		0,		25,		0 },
				/* Plate	*/	{ 5,	5,		10,		14,		18,		5,		30,		0 },
				/* Dragon	*/	{ 0,	7,		13,		15,		22,		7,		35,		0 }
			};

		public static int GetArmorRating( Mobile m )
		{
			if( !Core.AOS )
				return (int)m.ArmorRating;
			
			int ar = 0;

			for( int i = 0; i < m.Items.Count; i++ )
			{
				BaseArmor armor = m.Items[i] as BaseArmor;

				if( armor == null )
					continue;

				int materialType = (int)armor.MaterialType;
				int bodyPosition = (int)armor.BodyPosition;

				if( materialType >= m_ArmorTable.GetLength( 0 ) || bodyPosition >= m_ArmorTable.GetLength( 1 ) )
					continue;

				ar += m_ArmorTable[materialType, bodyPosition];
			}

			return ar;
		}

		public static TimeSpan OnUse( Mobile m )
		{
			if ( !m.Hidden )
			{
				m.SendLocalizedMessage( 502725 ); // You must hide first
			}
			else if ( Tracking.IsBeingTracked( m ) )
			{
				m.SendMessage( "You are being tracked and cannot enter stealth." );
				m.RevealingAction();
			}
			else if ( m.Skills[SkillName.Hiding].Value < ((Core.ML) ? 30.0 : (Core.SE) ? 50.0 : 80.0) )
			{
				m.SendLocalizedMessage( 502726 ); // You are not hidden well enough.  Become better at hiding.
				m.RevealingAction();
			}
			else if( !m.CanBeginAction( typeof( Stealth ) ) )
			{
				m.SendLocalizedMessage( 1063086 ); // You cannot use this skill right now.
				m.RevealingAction();
			}
			else
			{
				int armorRating = GetArmorRating( m );
				int nearbyCheckers = 0;

				foreach ( Mobile check in m.GetMobilesInRange( 8 ) )
				{
					if ( check != m && check.Alive && !check.Hidden && check.CanSee( m ) )
						++nearbyCheckers;
				}

				double mobPenalty = Math.Max( 0, nearbyCheckers - 1 ) * 2.0;

				if ( m.Skills[SkillName.Stealth].Base >= 125.0 )
					mobPenalty *= 0.5;

				if( armorRating >= (Core.AOS ? 42 : 26) )	//I have a hunch '42' was chosen cause someone's a fan of DNA
				{
					m.SendLocalizedMessage( 502727 ); // You could not hope to move quietly wearing this much armor.
					m.RevealingAction();
				}
				else if( m.CheckSkill( SkillName.Stealth, -20.0 + (armorRating * 2) + mobPenalty, (Core.AOS ? 60.0 : 80.0) + (armorRating * 2) + mobPenalty ) )
				{
					int steps = (int)(m.Skills[SkillName.Stealth].Value / (Core.AOS ? 5.0 : 10.0));

					if( steps < 1 )
						steps = 1;

					m.AllowedStealthSteps = steps;

					PlayerMobile pm = m as PlayerMobile; // IsStealthing should be moved to Server.Mobiles

					if( pm != null )
    						pm.IsStealthing = true;

					m.SendLocalizedMessage( 502730 ); // You begin to move quietly.

					return TimeSpan.FromSeconds( 2.0 );
				}
				else
				{
					m.SendLocalizedMessage( 502731 ); // You fail in your attempt to move unnoticed.
					m.RevealingAction();
				}
			}

			return TimeSpan.FromSeconds( 2.0 );
		}
	}
}
