using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Gumps
{
	public class PhylacteryGump : Gump
	{
		private const int CycleOwlNightSightButton = 2000;
		private const int ToggleSpellChannelingButton = 2001;
		private const int ToggleSpeedBoostButton = 2002;

		private Phylactery m_Phylactery;

		private BloodLich m_BloodLich;
		private bool m_ReadOnly;

		public PhylacteryGump( Mobile from, Phylactery phylactery, BloodLich bloodLich, int page )
			: this( from, phylactery, bloodLich, page, false )
		{
		}

		public PhylacteryGump( Mobile from, Phylactery phylactery, int page )
			: this( from, phylactery, null, page, true )
		{
		}

		private PhylacteryGump( Mobile from, Phylactery phylactery, BloodLich bloodLich, int page, bool readOnly ) : base( 0, 0 )
		{
			if ( from == null )
				return;

			from.CloseGump( typeof( PhylacteryGump ) );
			from.SendSound( 0x55 );
			m_BloodLich = bloodLich;
			m_ReadOnly = readOnly;

			if ( phylactery != null )
			{	
				if ( !phylactery.Temporary )
				{
					m_Phylactery = phylactery;

					this.Closable=true;
					this.Disposable=true;
					this.Dragable=true;
					this.Resizable=false;	

					this.AddPage(0);
					this.AddImage(40,36,1100);

					if (page == 3)
					{

						this.AddButton(108, 50, 1057, 1057, 1003, GumpButtonType.Reply, 0);
						this.AddLabel(360, 60, 0, "Total essence power: " + phylactery.TotalPower().ToString() + "/" + phylactery.GetRawPoints(from).ToString());
						this.AddLabel(360, 90, 0, "Soul Items:");
						this.AddLabel(400, 125, 0, "White Shard: 7500 SF");
						AddActionButton(360, 120, 11320, 11320, 27);
						this.AddLabel(400, 155, 0, "Prismatic Shard: 7500 SF");
						AddActionButton(360, 150, 11320, 11320, 28);
						PlayerMobile runeOwner = from as PlayerMobile;
						BloodRune playerBloodRune = BloodRune.FindOwnedBy( runeOwner );
			string bloodRuneLabel = playerBloodRune == null
				? "Blood Rune: 2500 SF"
							: playerBloodRune.CanUpgrade
								? "Blood Rune Upgrade: 2500 SF"
								: "Blood Rune: MAX";
						this.AddLabel(400, 185, 0, bloodRuneLabel);
						AddActionButton(360, 180, 11320, 11320, 29);
						this.AddLabel(400, 215, 0, "Royal Blood Urn: 35000 SF");
						AddActionButton(360, 210, 11320, 11320, 30);
						this.AddLabel(360, 245, 0, "Cosmetic Items:");
						this.AddLabel(400, 275, 0, "Orc Soul: 25000 SF");
						AddActionButton(360, 270, 11320, 11320, 31);

					}
					else if (page == 2) 
					{
						this.AddButton(108, 50, 1057, 1057, 1000, GumpButtonType.Reply, 0);
						this.AddButton(525, 50, 1058, 1058, 1002, GumpButtonType.Reply, 0);
						this.AddLabel(165, 60, 0, "Owl Essence: " + phylactery.OwlEssence.ToString() + "/" + phylactery.OwlEssenceMax.ToString());
						if (phylactery.OwlEssence < phylactery.OwlEssenceMax) {
							AddActionButton(290, 55, 2151, 2153, 15);
						}
						this.AddLabel(140, 90, 0, "Pixie Essence: " + phylactery.PixieEssence.ToString() + "/" + phylactery.PixieEssenceMax.ToString());
						if (phylactery.PixieEssence < phylactery.PixieEssenceMax) {
							AddActionButton(290, 85, 2151, 2153, 16);
						}
						this.AddLabel(140, 120, 0, "Planar Essence: " + phylactery.PlanarEssence.ToString() + "/" + phylactery.PlanarEssenceMax.ToString());
						if (phylactery.PlanarEssence < phylactery.PlanarEssenceMax) {
							AddActionButton(290, 115, 2151, 2153, 17);
						}
						this.AddLabel(140, 150, 0, "Plant Essence: " + phylactery.PlantEssence.ToString() + "/" + phylactery.PlantEssenceMax.ToString());
						if (phylactery.PlantEssence < phylactery.PlantEssenceMax) {
							AddActionButton(290, 145, 2151, 2153, 18);
						}
						this.AddLabel(140, 180, 0, "Sage Essence: " + phylactery.SageEssence.ToString() + "/" + phylactery.SageEssenceMax.ToString());
						if (phylactery.SageEssence < phylactery.SageEssenceMax) {
							AddActionButton(290, 175, 2151, 2153, 19);
						}
						this.AddLabel(140, 210, 0, "Snake Essence: " + phylactery.SnakeEssence.ToString() + "/" + phylactery.SnakeEssenceMax.ToString());
						if (phylactery.SnakeEssence < phylactery.SnakeEssenceMax) {
							AddActionButton(290, 205, 2151, 2153, 20);
						}
						this.AddLabel(140, 240, 0, "Thorn Essence: " + phylactery.ThornEssence.ToString() + "/" + phylactery.ThornEssenceMax.ToString());
						if (phylactery.ThornEssence < phylactery.ThornEssenceMax) {
							AddActionButton(290, 235, 2151, 2153, 21);
						}
						this.AddLabel(140, 270, 0, "Titan Essence: " + phylactery.TitanEssence.ToString() + "/" + phylactery.TitanEssenceMax.ToString());
						if (phylactery.TitanEssence < phylactery.TitanEssenceMax) {
							AddActionButton(290, 265, 2151, 2153, 22);
						}
						this.AddLabel(140, 300, 0, "Water Essence: " + phylactery.WaterEssence.ToString() + "/" + phylactery.WaterEssenceMax.ToString());
						if (phylactery.WaterEssence < phylactery.WaterEssenceMax) {
							AddActionButton(290, 295, 2151, 2153, 23);
						}

						this.AddLabel(360, 60, 0, "Vampire Essence: " + phylactery.VampireEssence.ToString() + "/" + phylactery.VampireEssenceMax.ToString());
						if (phylactery.VampireEssence < phylactery.VampireEssenceMax) {
							AddActionButton(495, 55, 2151, 2153, 24);
						}

						this.AddLabel(360, 90, 0, "Spring Essence: " + phylactery.SpringEssence.ToString() + "/" + phylactery.SpringEssenceMax.ToString());
						if (phylactery.SpringEssence < phylactery.SpringEssenceMax) {
							AddActionButton(530, 85, 2151, 2153, 25);
						}

						this.AddLabel(360, 120, 0, "Sacred Essence: " + phylactery.SacredEssence.ToString() + "/" + phylactery.SacredEssenceMax.ToString());
						if (phylactery.SacredEssence < phylactery.SacredEssenceMax) {
							AddActionButton(530, 115, 2151, 2153, 26);
						}
						if (m_ReadOnly) {
							int optionY = 150;
							PlayerMobile player = from as PlayerMobile;

							if (phylactery.HasOwlNightSight()) {
								this.AddLabel(360, optionY, 0, "Owl Light: " + phylactery.GetOwlNightSightLabel());
								this.AddButton(530, optionY - 5, 2151, 2153, CycleOwlNightSightButton, GumpButtonType.Reply, 0);
								optionY += 30;
							}

							if (phylactery.HasSpellChanneling()) {
								this.AddLabel(360, optionY, 0, "Spell Channeling: " + phylactery.GetSpellChannelingLabel());
								this.AddButton(530, optionY - 5, 2151, 2153, ToggleSpellChannelingButton, GumpButtonType.Reply, 0);
								optionY += 30;
							}

							if (player != null && player.HasSoulBoundSpeedBoost()) {
								this.AddLabel(360, optionY, 0, "Speed Boost: " + player.GetSoulBoundSpeedBoostLabel());
								this.AddButton(530, optionY - 5, 2151, 2153, ToggleSpeedBoostButton, GumpButtonType.Reply, 0);
							}
						}
					} 
					else 
					{
						this.AddButton(525, 50, 1058, 1058, 1001, GumpButtonType.Reply, 0);
						this.AddLabel(140, 60, 0, "Available Soul Force: " + ((PlayerMobile)from).SoulForce.ToString() );
						this.AddLabel(140, 80, 0, "Soul Force Costs:"); 
						this.AddLabel(140, 100, 0, "Gain Level:  " + phylactery.SoulForceRequiredForGain(from, "BaseSoulForcePowerCost").ToString() );
						this.AddLabel(140, 120, 0, "Gain Essence: " + phylactery.SoulForceRequiredForGain(from, "BaseSoulForceEssenceCost").ToString() );
						this.AddLabel(140, 140, 0, "Gain Luck:  " + phylactery.SoulForceRequiredForGain(from, "BaseSoulForceLuckCost").ToString() );
						this.AddLabel(140, 170, 0, "Vault Level: " + phylactery.PowerLevel.ToString() + "/" + phylactery.PowerLevelMax.ToString());
						if (phylactery.PowerLevel < phylactery.PowerLevelMax) {
							AddActionButton(290, 165, 2151, 2153, 1);
							// this.AddLabel(300, 161, 0, "+");
						}
						this.AddLabel(140, 200, 0, "Cold Essence: " + phylactery.ColdEssence.ToString() + "/" + phylactery.ColdEssenceMax.ToString());
						if (phylactery.ColdEssence < phylactery.ColdEssenceMax) {
							AddActionButton(290, 195, 2151, 2153, 2);
							// this.AddLabel(165, 218, 0, "Add cold");
						}
						this.AddLabel(140, 230, 0, "Earth Essence: " + phylactery.EarthEssence.ToString() + "/" + phylactery.EarthEssenceMax.ToString());
						if (phylactery.EarthEssence < phylactery.EarthEssenceMax) {
							AddActionButton(290, 225, 2151, 2153, 3);
							// this.AddLabel(165, 258, 0, "Add earth");
						}
						this.AddLabel(140, 260, 0, "Energy Essence: " + phylactery.EnergyEssence.ToString() + "/" + phylactery.EnergyEssenceMax.ToString());
						if (phylactery.EnergyEssence < phylactery.EnergyEssenceMax) {
							AddActionButton(290, 255, 2151, 2153, 4);
							// this.AddLabel(104, 228, 0, "Add point");
						}			
						this.AddLabel(140, 290, 0, "Fire Essence: " + phylactery.FireEssence.ToString() + "/" + phylactery.FireEssenceMax.ToString());
						if (phylactery.FireEssence < phylactery.FireEssenceMax) {
							AddActionButton(290, 285, 2151, 2153, 5);
						}

						this.AddLabel(360, 60, 0, "Bear Essence: " + phylactery.BearEssence.ToString() + "/" + phylactery.BearEssenceMax.ToString());
						if (phylactery.BearEssence < phylactery.BearEssenceMax) {
							AddActionButton(495, 55, 2151, 2153, 6);
						}

						this.AddLabel(360, 90, 0, "Scorpion Essence: " + phylactery.ScorpionEssence.ToString() + "/" + phylactery.ScorpionEssenceMax.ToString());
						if (phylactery.ScorpionEssence < phylactery.ScorpionEssenceMax) {
							AddActionButton(530, 85, 2151, 2153, 7);
						}

						this.AddLabel(360, 120, 0, "Celestial Essence: " + phylactery.CelestialEssence.ToString() + "/" + phylactery.CelestialEssenceMax.ToString());
						if (phylactery.CelestialEssence < phylactery.CelestialEssenceMax) {
							AddActionButton(530, 115, 2151, 2153, 8);
						}

						this.AddLabel(360, 150, 0, "Demonic Essence: " + phylactery.DemonicEssence.ToString() + "/" + phylactery.DemonicEssenceMax.ToString());
						if (phylactery.DemonicEssence < phylactery.DemonicEssenceMax) {
							AddActionButton(530, 145, 2151, 2153, 9);
						}
						
						this.AddLabel(360, 180, 0, "Eagle Essence: " + phylactery.EagleEssence.ToString() + "/" + phylactery.EagleEssenceMax.ToString());
						if (phylactery.EagleEssence < phylactery.EagleEssenceMax) {
							AddActionButton(530, 175, 2151, 2153, 10);
						}
						this.AddLabel(360, 210, 0, "Gazer Essence: " + phylactery.GazerEssence.ToString() + "/" + phylactery.GazerEssenceMax.ToString());
						if (phylactery.GazerEssence < phylactery.GazerEssenceMax) {
							AddActionButton(530, 205, 2151, 2153, 11);
						}

						this.AddLabel(360, 240, 0, "Horse Essence: " + phylactery.HorseEssence.ToString() + "/" + phylactery.HorseEssenceMax.ToString());
						if (phylactery.HorseEssence < phylactery.HorseEssenceMax) {
							AddActionButton(530, 235, 2151, 2153, 12);
						}

						this.AddLabel(360, 270, 0, "Imp Essence: " + phylactery.ImpEssence.ToString() + "/" + phylactery.ImpEssenceMax.ToString());
						if (phylactery.ImpEssence < phylactery.ImpEssenceMax) {
							AddActionButton(530, 265, 2151, 2153, 13);
						}

						this.AddLabel(360, 300, 0, "Lucky Essence: " + phylactery.LuckyEssence.ToString() + "/" + phylactery.LuckyEssenceMax.ToString());
						if (phylactery.LuckyEssence < phylactery.LuckyEssenceMax) {
							AddActionButton(530, 295, 2151, 2153, 14);
						}
					}
				} 
				else if ( m_BloodLich != null )
				{
					m_BloodLich.SayTo(from, "This phylactery is too weak to peform a soul crafting ritual");
				}
				
			}
		}

		private void AddActionButton( int x, int y, int normalID, int pressedID, int buttonID )
		{
			if ( !m_ReadOnly )
				AddButton( x, y, normalID, pressedID, buttonID, GumpButtonType.Reply, 0 );
		}

		private static bool TryGetNavigationPage( int buttonID, out int page )
		{
			switch ( buttonID )
			{
				case 1000:
					page = 1;
					return true;
				case 1001:
					page = 2;
					return true;
				case 1002:
					page = 3;
					return true;
				case 1003:
					page = 2;
					return true;
				default:
					page = 0;
					return false;
			}
		}

		private static bool IsValidActionButton( int buttonID )
		{
			return buttonID >= 1 && buttonID <= 31;
		}

		private bool CanUseBloodLich( PlayerMobile player )
		{
			if ( player == null || !player.Alive )
				return false;

			if ( m_Phylactery == null || m_Phylactery.Deleted || m_Phylactery.Temporary )
				return false;

			if ( player.Backpack == null || !m_Phylactery.IsChildOf( player.Backpack ) )
				return false;

			if ( m_BloodLich == null || m_BloodLich.Deleted || !m_BloodLich.Alive )
				return false;

			if ( player.Map == null || player.Map != m_BloodLich.Map )
				return false;

			if ( !player.InRange( m_BloodLich, 4 ) || !player.InLOS( m_BloodLich ) )
				return false;

			return true;
		}

		private void HandleReadOnlyResponse( PlayerMobile player, RelayInfo info )
		{
			if ( player == null || info == null || m_Phylactery == null || m_Phylactery.Deleted )
				return;

			if ( player.Backpack == null || !m_Phylactery.IsChildOf( player.Backpack ) )
				return;

			if ( info.ButtonID == CycleOwlNightSightButton )
			{
				m_Phylactery.CycleOwlNightSight( player );
				player.SendGump( new PhylacteryGump( player, m_Phylactery, 2 ) );
				return;
			}

			if ( info.ButtonID == ToggleSpellChannelingButton )
			{
				m_Phylactery.ToggleSpellChanneling( player );
				player.SendGump( new PhylacteryGump( player, m_Phylactery, 2 ) );
				return;
			}

			if ( info.ButtonID == ToggleSpeedBoostButton )
			{
				player.ToggleSoulBoundSpeedBoost();
				player.SendGump( new PhylacteryGump( player, m_Phylactery, 2 ) );
				return;
			}

			int page;

			if ( !TryGetNavigationPage( info.ButtonID, out page ) )
				return;

			player.SendGump( new PhylacteryGump( player, m_Phylactery, page ) );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			PlayerMobile player = state == null ? null : state.Mobile as PlayerMobile;

			if ( m_ReadOnly )
			{
				HandleReadOnlyResponse( player, info );
				return;
			}

			if ( player == null )
				return;

			if ( info == null || info.ButtonID == 0 )
				return;

			if ( !CanUseBloodLich( player ) )
			{
				player.SendMessage( "You must remain near the Blood Lich to use your soul vault." );
				return;
			}

			int page;

			if ( TryGetNavigationPage( info.ButtonID, out page ) )
			{
				player.SendGump( new PhylacteryGump( player, m_Phylactery, m_BloodLich, page ) );
				return;
			}

			if ( !IsValidActionButton( info.ButtonID ) )
				return;

			page = 1;

			if ( info.ButtonID >= 27 )
			{
				page = 3;
				ProcessPurchase( info.ButtonID, player );
			}
			else
			{
				if ( info.ButtonID >= 15 )
					page = 2;

				ProcessImbue( info.ButtonID, player );
			}

			player.SendGump( new PhylacteryGump( player, m_Phylactery, m_BloodLich, page ) );
			player.SendSound( 0x19D );
		}

		public void ProcessImbue(int buttonID, PlayerMobile player) {
			string soulForcePropertyName = "BaseSoulForceEssenceCost";
			if (buttonID == 14) { // luck
				soulForcePropertyName = "BaseSoulForceLuckCost";
			} else if (buttonID == 1)  {// power
				soulForcePropertyName = "BaseSoulForcePowerCost";
			}
			int requiredSoulForce = m_Phylactery.SoulForceRequiredForGain(player, soulForcePropertyName);
			// soul items overwrite the above
			if (player.SoulForce >= requiredSoulForce) { 
				bool speak = (m_BloodLich.NextMockery < DateTime.UtcNow);
				// remove old stats first!
				// m_Phylactery.ResetOwnerStats(player);
				string message = "Here, have some of my blood";
				switch (buttonID) {
					//Power
					case 1:
						if (m_Phylactery.PowerLevel >= m_Phylactery.PowerLevelMax)
							return;
						++m_Phylactery.PowerLevel;
						if (m_Phylactery.PowerLevel == 10)
							player.sbmaster = true;
					
					break;
					//ColdEssence
					case 2:
						if (m_Phylactery.ColdEssence >= m_Phylactery.ColdEssenceMax)
							return;
						++m_Phylactery.ColdEssence;
						message = "Frosty cold.";
					break;
					case 3:
						if (m_Phylactery.EarthEssence >= m_Phylactery.EarthEssenceMax)
							return;
						++m_Phylactery.EarthEssence;
						message = "Do you have any idea how many earth elementals I had to obliterate to find this essence.";
					break;
					//EnergyEssence
					case 4:
						if (m_Phylactery.EnergyEssence >= m_Phylactery.EnergyEssenceMax)
							return;
						++m_Phylactery.EnergyEssence;
						message = "I love the blood of a mage, don't you?";
					break;
					//FireEssence
					case 5:
						if (m_Phylactery.FireEssence >= m_Phylactery.FireEssenceMax)
							return;
						++m_Phylactery.FireEssence;
						message = "Toasty warm.";
					break;
					//BearEssence
					case 6:
						if (m_Phylactery.BearEssence >= m_Phylactery.BearEssenceMax)
							return;
						++m_Phylactery.BearEssence;
						message = "I went on a bear hunt";
					break;
					//ScorpionEssence
					case 7:
						if (m_Phylactery.ScorpionEssence >= m_Phylactery.ScorpionEssenceMax)
							return;
						++m_Phylactery.ScorpionEssence;
						message = "I hate scorpions.";
					break;
					//CelestialEssence
					case 8:
						if (m_Phylactery.CelestialEssence >= m_Phylactery.CelestialEssenceMax)
							return;
						++m_Phylactery.CelestialEssence;
						message = "Ack! Its too holy, away with it!";
					break;
					//DemonicEssence
					case 9:
						if (m_Phylactery.DemonicEssence >= m_Phylactery.DemonicEssenceMax)
							return;
						++m_Phylactery.DemonicEssence;	
						message = "The blood of my most recent summon, it didn't end well for " + NameList.RandomName( "demonic" ) + ".";
					break;
					//EagleEssence
					case 10:
						if (m_Phylactery.EagleEssence >= m_Phylactery.EagleEssenceMax)
							return;
						++m_Phylactery.EagleEssence;
						message = "It definitely saw you coming!";
					break;
					//GazerEssence
					case 11:
						if (m_Phylactery.GazerEssence >= m_Phylactery.GazerEssenceMax)
							return;
						++m_Phylactery.GazerEssence;
						message = "Careful, you don't want Mad Gazer Disease";
					break;
					//HorseEssence
					case 12:
						if (m_Phylactery.HorseEssence >= m_Phylactery.HorseEssenceMax)
							return;
						++m_Phylactery.HorseEssence;
						message = "Good with a side of skull worms";
					break;
					//ImpEssence
					case 13:
						if (m_Phylactery.ImpEssence >= m_Phylactery.ImpEssenceMax)
							return;
						++m_Phylactery.ImpEssence;
						message = "Poor " + NameList.RandomName( "imp" ) + ", it didn't listen to me.";
					break;						
					//LuckyEssence
					case 14:
						if (m_Phylactery.LuckyEssence >= m_Phylactery.LuckyEssenceMax)
							return;
						++m_Phylactery.LuckyEssence;
						message = "You take it, I don't need luck.";
					break;
					//OwlEssence
					case 15:
						if (m_Phylactery.OwlEssence >= m_Phylactery.OwlEssenceMax)
							return;
						++m_Phylactery.OwlEssence;
						message = "Don't start hooting, people will think you're crazy.";
					break;
					//PixieEssence
					case 16:
						if (m_Phylactery.PixieEssence >= m_Phylactery.PixieEssenceMax)
							return;
						++m_Phylactery.PixieEssence;
						message = "I love a gloomy Tinkerbell.";
					break;
					//PlanarEssence
					case 17:
						if (m_Phylactery.PlanarEssence >= m_Phylactery.PlanarEssenceMax)
							return;
						++m_Phylactery.PlanarEssence;
						message = "Falling into our plane.... now.";
					break;
					//PlantEssence
					case 18:
						if (m_Phylactery.PlantEssence >= m_Phylactery.PlantEssenceMax)
							return;
						++m_Phylactery.PlantEssence;
						message = "Cold off the back of a hairy Ent named " + NameList.RandomName( "trees" ) + ".";
					break;
					//SageEssence
					case 19:
						if (m_Phylactery.SageEssence >= m_Phylactery.SageEssenceMax)
							return;
						++m_Phylactery.SageEssence;
						message = "Sage blood, better than the herb.";
					break;
					//SnakeEssence
					case 20:
						if (m_Phylactery.SnakeEssence >= m_Phylactery.SnakeEssenceMax)
							return;
						++m_Phylactery.SnakeEssence;
						message = "I loot this snake oil from the white witches, the black ones are ok.";
					break;
					//ThornEssence
					case 21:
						if (m_Phylactery.ThornEssence >= m_Phylactery.ThornEssenceMax)
							return;
						++m_Phylactery.ThornEssence;
						message = "Ouch, you spiked me!";
					break;
					//TitanEssence
					case 22:
						if (m_Phylactery.TitanEssence >= m_Phylactery.TitanEssenceMax)
							return;
						++m_Phylactery.TitanEssence;
						message = "Titan flesh, a bit tough, but edible.";
					break;
					//WaterEssence
					case 23:
						if (m_Phylactery.WaterEssence >= m_Phylactery.WaterEssenceMax)
							return;
						++m_Phylactery.WaterEssence;
						message = "The tears of a monk. So salty. So delicious.";
					break;
					case 24:
						if (m_Phylactery.VampireEssence >= m_Phylactery.VampireEssenceMax)
							return;
						++m_Phylactery.VampireEssence;
						message = "Ah yes, the grinded tooth of a vampire.  Crunchy.";
					break;
					case 25:
						if (m_Phylactery.SpringEssence >= m_Phylactery.SpringEssenceMax)
							return;
						++m_Phylactery.SpringEssence;
						message = "Reguvenating water from the purest spring.";
					break;
					case 26:
						if (m_Phylactery.SacredEssence >= m_Phylactery.SacredEssenceMax)
							return;
						++m_Phylactery.SacredEssence;
						message = "A little piece of a sacred altar.  Swallow it whole.";
					break;
				}
				if (speak) {
					m_BloodLich.NextMockery = DateTime.UtcNow.AddSeconds(7);
					m_BloodLich.SayTo( player, message );  
				}
				player.SoulForce -= requiredSoulForce;
				m_Phylactery.UpdateOwnerSoul(player);
				m_Phylactery.SoulForceSpent += requiredSoulForce;
				m_Phylactery.UpdateSoulForceCost(soulForcePropertyName, m_Phylactery.SoulForceFraction(player, soulForcePropertyName), true);
				// Server.Items.CharacterDatabase.GetMySpellHue( player, 0 )
				player.FixedParticles( 0x374A, 10, 30, 5013, 18, 2, EffectLayer.Waist );
				player.PlaySound( 0x0FC );
			} else {
				m_BloodLich.SayTo( player, BloodLich.BadForceReply );
			}
		}

		public void ProcessPurchase(int buttonID, PlayerMobile player) {
			string message = "";
			int requiredSoulForce = 0;
			BloodRune playerBloodRune = null;
			if (buttonID == 27 || buttonID == 28) {
				// white shard or prismatic shard
				requiredSoulForce = 7500;
			} else if (buttonID == 29) {
				// blood rune
				requiredSoulForce = 2500;
			} else if (buttonID == 30) {
				// royal blood urn
				requiredSoulForce = 35000;
			} else if (buttonID == 31 ) {
				// orc soul
				requiredSoulForce = 25000;
			}

			if ( buttonID == 29 && player != null && player.Backpack != null ) {
				playerBloodRune = BloodRune.FindOwnedBy( player );

				if ( playerBloodRune != null && !playerBloodRune.CanUpgrade ) {
					m_BloodLich.SayTo( player, "Your blood rune has already reached its minimum 10 minute cooldown." );
					return;
				}
			}

			if (player.SoulForce >= requiredSoulForce && player.Backpack != null) { 
				bool speak = (m_BloodLich.NextMockery < DateTime.UtcNow);
				switch (buttonID) {
					case 27:
						// white shard
					 	WhiteShard playerWhiteShard = (WhiteShard)player.Backpack.FindItemByType(typeof(WhiteShard)); 
					 	if (playerWhiteShard != null) {
					 		m_BloodLich.SayTo(player, BloodLich.AlreadyHaveOne);
					 		return;
					 	}
						WhiteShard whiteShard = new WhiteShard();
						player.Backpack.DropItem(whiteShard);
						message = "And you thought healing wands were good!";
					break;
					case 28:
						// prismatic shard
						PrismaticShard playerPrismaticShard = (PrismaticShard)player.Backpack.FindItemByType(typeof(PrismaticShard)); 
					 	if (playerPrismaticShard != null) {
					 		m_BloodLich.SayTo(player, BloodLich.AlreadyHaveOne);
					 		return;
					 	}
						PrismaticShard prismaticShard = new PrismaticShard();
						player.Backpack.DropItem(prismaticShard);
						message = "Now you know why us liches never run out of mana!";
					break;
					case 29:
						// blood rune
						if ( playerBloodRune == null ) {
							BloodRune bloodRune = new BloodRune();
							bloodRune.Owner = player;
							player.Backpack.DropItem( bloodRune );
							message = "I guess I will be seeing you more quickly next time?";
						} else if ( playerBloodRune.UpgradeCooldown() ) {
							message = "Your blood rune now has a " + playerBloodRune.CooldownMinutes.ToString() + " minute cooldown.";
							player.SendMessage( "Your blood rune cooldown has been reduced to " + playerBloodRune.CooldownMinutes.ToString() + " minutes." );
							player.PublicOverheadMessage( MessageType.Regular, 0x3B2, false, "you feel the blood rune in your inventory getting stronger" );
						}
					break;
					case 30:
						// royal blood urn; 
						RoyalBloodUrn urn = new RoyalBloodUrn();
						player.Backpack.DropItem(urn);
						message = "You are brave, mortal, make sure to party and get prepared before using this deadly item.";
					break;
					case 31:
						// orc soul 
						OrcSoul playerOrcSoul = (OrcSoul)player.Backpack.FindItemByType(typeof(OrcSoul)); 
					 	if (playerOrcSoul != null) {
					 		m_BloodLich.SayTo(player, BloodLich.AlreadyHaveOne);
					 		return;
					 	}
						OrcSoul soul = new OrcSoul();
						player.Backpack.DropItem(soul);
						message = "Enjoy dressing up as an orc, not sure why you would want to, but there here you are.";
					break;
				}
				if (speak) {
					m_BloodLich.NextMockery = DateTime.UtcNow.AddSeconds(7);
					m_BloodLich.SayTo( player, message );  
				}
				player.SoulForce -= requiredSoulForce;
			} else {
				m_BloodLich.SayTo( player, BloodLich.BadForceReply );
			}
		}
		

	}
}
