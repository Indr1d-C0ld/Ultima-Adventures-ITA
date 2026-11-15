using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Misc;
using Server.Spells;
using Server.Targeting;

namespace Server.Spells.Research
{
	public class ResearchEnchant : ResearchSpell
	{
		public override int spellIndex { get { return 35; } }
		public int CirclePower = 5;
		public static int spellID = 35;
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3.0 ); } }
		public override double RequiredSkill { get { return (double)Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 8 ) ); } }
		public override int RequiredMana { get { return Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 7 ) ); } }

		private static SpellInfo m_Info = new SpellInfo( Server.Misc.Research.SpellInformation( spellID, 2 ), Server.Misc.Research.CapsCast( Server.Misc.Research.SpellInformation( spellID, 4 ) ), 266, 9040 );
		private static Hashtable m_Table = new Hashtable();


		public ResearchEnchant( Mobile caster, Item scroll ) : base( caster, scroll, m_Info ) { }

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this, spellID );
		}

		public void Target( object o, int spellID )
		{
			Item item = o as Item;

			if ( item == null || !CanEnchant( item ) )
				Caster.SendMessage( "You can only enchant instruments, weapons, and spellbooks with this spell." );
			else if ( !Caster.CanSee( item ) )
				Caster.SendLocalizedMessage( 500237 );
			else if ( !item.IsChildOf( Caster.Backpack ) )
				Caster.SendMessage( "The item must be in your pack to enchant." );
			else if ( CheckSequence() )
			{
				TimeSpan duration = GetDuration();
				ResearchEnchantStone.ResolvePendingRestore( item );
				EnchantState state = (EnchantState)m_Table[item];
				int candidateStrength = GetAppliedEnchantValue( item );

				if ( state != null )
				{
					TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( true, state.Strength, candidateStrength, false );

					if ( decision == TimedBuffRefreshDecision.RefreshExisting )
					{
						state.Refresh();
						CompleteEnchant( spellID );
						FinishSequence();
						return;
					}

					RemoveState( item, state, false );
				}

				string originalName = item.Name;
				string displayName = originalName;
				if ( String.IsNullOrEmpty( displayName ) )
					displayName = MorphingItem.AddSpacesToSentence( item.GetType().Name );

				int originalHue = item.Hue;
				int originalStrength = GetCurrentEnchantValue( item );
				item.Name = displayName + " [enchanted]";
				item.Hue = 0x489;
				int appliedStrength = ApplyEnchantBonus( item );
				ResearchEnchantStone token = new ResearchEnchantStone();
				token.EnchantOwner = Caster; token.EnchantSerial = item.Serial; token.EnchantName = originalName; token.EnchantHue = originalHue; token.EnchantDmg = originalStrength; token.EnchantType = GetEnchantType( item ); Caster.AddToBackpack( token );
				state = new EnchantState( item, token, Caster, originalName, originalHue, originalStrength, item.Name, item.Hue, appliedStrength, GetEnchantType( item ), duration );
				m_Table[item] = state;
				CompleteEnchant( spellID );
			}

			FinishSequence();
		}

		private TimeSpan GetDuration()
		{
			int minutes = (int)( DamagingSkill( Caster ) / 2 );
			if ( minutes > 120 ) minutes = 120;
			return TimeSpan.FromMinutes( minutes );
		}

		private void CompleteEnchant( int spellID )
		{
			Caster.PlaySound( 0x1F7 );
			Caster.FixedParticles( 0x3039, 9, 32, 5008, Server.Items.CharacterDatabase.GetMySpellHue( Caster, 0x85E ), 0, EffectLayer.Waist );
			Server.Misc.Research.ConsumeScroll( Caster, true, spellID, false );
		}

		private static bool CanEnchant( Item item ) { return item is BaseWeapon || item is BaseInstrument || item is Spellbook; }
		private static int GetEnchantType( Item item ) { if ( item is BaseWeapon ) return 1; if ( item is BaseInstrument ) return 2; if ( item is Spellbook ) return 3; return 0; }
		private static int GetCurrentEnchantValue( Item item ) { if ( item is BaseWeapon ) return ( (BaseWeapon)item ).Attributes.WeaponDamage; if ( item is BaseInstrument ) return ( (BaseInstrument)item ).Attributes.SpellDamage; if ( item is Spellbook ) return ( (Spellbook)item ).Attributes.SpellDamage; return 0; }
		private static int GetAppliedEnchantValue( Item item ) { return item is BaseWeapon ? 100 : 50; }

		private static int ApplyEnchantBonus( Item item )
		{
			int original = GetCurrentEnchantValue( item );
			if ( item is BaseWeapon ) ( (BaseWeapon)item ).Attributes.WeaponDamage += 100;
			else if ( item is BaseInstrument ) ( (BaseInstrument)item ).Attributes.SpellDamage += 50;
			else if ( item is Spellbook ) ( (Spellbook)item ).Attributes.SpellDamage += 50;
			return GetCurrentEnchantValue( item ) - original;
		}

		private static void RemoveState( Item item, EnchantState state, bool playSound )
		{
			if ( item == null || state == null || m_Table[item] != state ) return;
			m_Table.Remove( item );
			state.Stop();
			if ( !item.Deleted ) state.Restore( item );
			state.DeleteToken();
			if ( playSound && state.Source != null && !state.Source.Deleted ) state.Source.PlaySound( 0x1F8 );
		}

		private sealed class EnchantState
		{
			private Item m_Item;
			private ResearchEnchantStone m_Token;
			private string m_OriginalName, m_AppliedName;
			private int m_OriginalHue, m_AppliedHue, m_OriginalStrength, m_AppliedStrength, m_Type;
			private TimedBuffState m_State;

			public int Strength { get { return m_AppliedStrength; } }
			public Mobile Source { get { return m_State.Source; } }

			public EnchantState( Item item, ResearchEnchantStone token, Mobile source, string originalName, int originalHue, int originalStrength, string appliedName, int appliedHue, int appliedStrength, int type, TimeSpan duration )
			{
				m_Item = item;
				m_Token = token;
				m_OriginalName = originalName;
				m_OriginalHue = originalHue;
				m_OriginalStrength = originalStrength;
				m_AppliedName = appliedName;
				m_AppliedHue = appliedHue;
				m_AppliedStrength = appliedStrength;
				m_Type = type;
				m_State = new TimedBuffState( this, source, appliedStrength, duration, Expire );
			}

			public void Refresh() { m_State.Refresh(); }
			public void Stop() { m_State.Stop(); }
			public void DeleteToken() { if ( m_Token != null && !m_Token.Deleted ) m_Token.Delete(); }

			public void Restore( Item item )
			{
				if ( item.Name == m_AppliedName ) item.Name = m_OriginalName;
				if ( item.Hue == m_AppliedHue ) item.Hue = m_OriginalHue;
				SetCurrentEnchantValue( item, GetCurrentEnchantValue( item ) - m_AppliedStrength, m_Type );
			}

			public void RemoveForSave()
			{
				if ( m_Item != null && !m_Item.Deleted && m_Table[m_Item] == this ) RemoveState( m_Item, this, false );
			}

			private void Expire( TimedBuffState ignored )
			{
				if ( m_Item == null || m_Item.Deleted )
				{
					if ( m_Item != null && m_Table[m_Item] == this ) m_Table.Remove( m_Item );
					Stop();
					DeleteToken();
					return;
				}

				RemoveState( m_Item, this, true );
			}
		}

		private static void SetCurrentEnchantValue( Item item, int value, int type )
		{
			if ( type == 1 && item is BaseWeapon ) ( (BaseWeapon)item ).Attributes.WeaponDamage = value;
			else if ( type == 2 && item is BaseInstrument ) ( (BaseInstrument)item ).Attributes.SpellDamage = value;
			else if ( type == 3 && item is Spellbook ) ( (Spellbook)item ).Attributes.SpellDamage = value;
		}

		private class InternalTarget : Target
		{
			private ResearchEnchant m_Owner;
			private int m_SpellID;
			public InternalTarget( ResearchEnchant owner, int spellID ) : base( 12, false, TargetFlags.None ) { m_Owner = owner; m_SpellID = spellID; }
			protected override void OnTarget( Mobile from, object o ) { m_Owner.Target( o, m_SpellID ); }
			protected override void OnTargetFinish( Mobile from ) { m_Owner.FinishSequence(); }
		}
	}
}

namespace Server.Items
{
	public class ResearchEnchantStone : Item
	{
		private static Hashtable m_PendingRestores = new Hashtable();

		[Constructable]
		public ResearchEnchantStone() : base( 0x3199 )
		{
			Weight = 1.0;
			Movable = false;
			Hue = 0x489;
			Name = "Enchantment Power Orb";
			LootType = LootType.Blessed;
		}

		public override bool DisplayLootType { get { return false; } }

		public ResearchEnchantStone( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)2 );
			writer.Write( (Mobile)EnchantOwner );
			writer.Write( EnchantSerial );
			writer.Write( EnchantName );
			writer.Write( EnchantDmg );
			writer.Write( EnchantHue );
			writer.Write( EnchantType );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			EnchantOwner = reader.ReadMobile();
			EnchantSerial = reader.ReadInt();
			EnchantName = reader.ReadString();
			EnchantDmg = reader.ReadInt();
			EnchantHue = reader.ReadInt();
			EnchantType = version >= 2 ? reader.ReadInt() : 1;

			RegisterPendingRestore();
			Timer.DelayCall( TimeSpan.FromSeconds( 10.0 ), new TimerCallback( RestorePending ) );
		}

		private void RegisterPendingRestore()
		{
			m_PendingRestores[EnchantSerial] = this;
		}

		public static void ResolvePendingRestore( Item item )
		{
			if ( item == null || item.Deleted )
				return;

			ResearchEnchantStone token = m_PendingRestores[item.Serial.Value] as ResearchEnchantStone;

			if ( token != null )
				token.RestorePending( item );
		}

		private void RestorePending()
		{
			Item item = World.FindItem( EnchantSerial );
			RestorePending( item );
		}

		private void RestorePending( Item item )
		{
			if ( m_PendingRestores[EnchantSerial] != this )
			{
				Delete();
				return;
			}

			m_PendingRestores.Remove( EnchantSerial );

			if ( item != null && !item.Deleted && item.Serial.Value == EnchantSerial )
			{
				item.Name = EnchantName;
				item.Hue = EnchantHue;

				if ( EnchantType == 1 && item is BaseWeapon )
					((BaseWeapon)item).Attributes.WeaponDamage = EnchantDmg;
				else if ( EnchantType == 2 && item is BaseInstrument )
					((BaseInstrument)item).Attributes.SpellDamage = EnchantDmg;
				else if ( EnchantType == 3 && item is Spellbook )
					((Spellbook)item).Attributes.SpellDamage = EnchantDmg;
			}

			Delete();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( m_PendingRestores[EnchantSerial] == this )
				m_PendingRestores.Remove( EnchantSerial );
		}

		public Mobile EnchantOwner;
		public int EnchantSerial;
		public string EnchantName;
		public int EnchantDmg;
		public int EnchantHue;
		public int EnchantType;
	}
}
