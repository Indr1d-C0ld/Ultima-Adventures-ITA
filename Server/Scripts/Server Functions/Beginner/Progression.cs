using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Mobiles;

namespace Server.Misc
{
	/// <summary>
	/// Progressione meno ripetitiva, senza togliere peso al gioco di ruolo:
	///  - RIPOSO: disconnettersi in un luogo sicuro (locanda, taverna, casa propria, giaciglio da campo:
	///    ovunque l'uscita dal gioco sia immediata) accumula ISPIRAZIONE. Al ritorno, finché dura,
	///    la probabilità di migliorare le abilità è raddoppiata.
	///  - VARIETÀ: chi usa molte abilità diverse (avventura vera, non la stessa azione ripetuta)
	///    impara un po' più in fretta.
	/// Tutti i valori sono in MyServerSettings.
	/// </summary>
	public static class Progression
	{
		public static void Initialize()
		{
			EventSink.Logout += new LogoutEventHandler( OnLogout );
			EventSink.Login += new LoginEventHandler( OnLogin );
			CommandSystem.Register( "ispirazione", AccessLevel.Player, new CommandEventHandler( Inspiration_OnCommand ) );
			CommandSystem.Register( "inspiration", AccessLevel.Player, new CommandEventHandler( Inspiration_OnCommand ) );
		}

		private static void OnLogout( LogoutEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if ( pm == null )
				return;

			pm.InspirationLeft = pm.InspirationMinutes;

			// luogo sicuro = uscita dal gioco immediata (locande, taverne, case, giaciglio)
			bool safe = pm.Alive && pm.GetLogoutDelay() == TimeSpan.Zero;
			pm.RestStart = safe ? DateTime.UtcNow : DateTime.MinValue;
		}

		private static void OnLogin( LoginEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if ( pm == null || !MyServerSettings.RestedInspirationEnabled() )
				return;

			double left = Math.Max( 0.0, pm.InspirationLeft );
			double earned = 0.0;

			if ( pm.RestStart > DateTime.MinValue && pm.RestStart < DateTime.UtcNow )
				earned = ( DateTime.UtcNow - pm.RestStart ).TotalMinutes / MyServerSettings.RestMinutesPerInspirationMinute();

			double total = Math.Min( left + earned, MyServerSettings.MaxInspirationMinutes() );

			pm.RestStart = DateTime.MinValue;
			pm.InspirationLeft = 0.0;
			pm.InspirationUntil = DateTime.UtcNow + TimeSpan.FromMinutes( total );

			if ( total >= 1.0 )
				Timer.DelayCall( TimeSpan.FromSeconds( 10.0 ), new TimerStateCallback( LoginNotice ), pm );
		}

		private static void LoginNotice( object state )
		{
			PlayerMobile pm = state as PlayerMobile;

			if ( pm == null || pm.NetState == null )
				return;

			int min = (int)pm.InspirationMinutes;
			if ( min > 0 )
				pm.SendMessage( 0x59, "Ti senti riposato e ispirato: per i prossimi {0} minuti imparerai le abilità più in fretta. ([ispirazione)", min );
		}

		/// <summary>Moltiplicatore della probabilità di guadagno di abilità dovuto a ispirazione e varietà.</summary>
		public static double GainMultiplier( PlayerMobile pm, SkillName skill )
		{
			double mult = 1.0;

			if ( MyServerSettings.RestedInspirationEnabled() && pm.InspirationUntil > DateTime.UtcNow )
				mult *= MyServerSettings.InspirationGainMultiplier();

			if ( MyServerSettings.VarietyBonusEnabled() )
			{
				DateTime now = DateTime.UtcNow;
				pm.RecentSkillUse[skill] = now;

				int distinct = 0;
				List<SkillName> old = null;
				TimeSpan window = TimeSpan.FromMinutes( MyServerSettings.VarietyWindowMinutes() );

				foreach ( KeyValuePair<SkillName, DateTime> kv in pm.RecentSkillUse )
				{
					if ( now - kv.Value <= window )
						distinct++;
					else
					{
						if ( old == null ) old = new List<SkillName>();
						old.Add( kv.Key );
					}
				}

				if ( old != null )
					foreach ( SkillName s in old )
						pm.RecentSkillUse.Remove( s );

				if ( distinct >= MyServerSettings.VarietySkillsNeeded() )
					mult *= MyServerSettings.VarietyGainMultiplier();
			}

			return mult;
		}

		[Usage( "ispirazione" )]
		[Description( "Mostra l'ispirazione accumulata riposando e il bonus di varietà." )]
		private static void Inspiration_OnCommand( CommandEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if ( pm == null )
				return;

			int min = (int)pm.InspirationMinutes;

			if ( min > 0 )
				pm.SendMessage( 0x59, "Ispirazione: ancora {0} minuti di apprendimento accelerato (probabilità di miglioramento x{1}).", min, MyServerSettings.InspirationGainMultiplier() );
			else
				pm.SendMessage( "Non sei ispirato. Esci dal gioco in un luogo sicuro (locanda, taverna, la tua casa o un giaciglio da campo) per accumulare ispirazione: ogni {0} minuti di riposo valgono 1 minuto di ispirazione, fino a {1} minuti.", MyServerSettings.RestMinutesPerInspirationMinute(), MyServerSettings.MaxInspirationMinutes() );

			int distinct = 0;
			TimeSpan window = TimeSpan.FromMinutes( MyServerSettings.VarietyWindowMinutes() );
			foreach ( KeyValuePair<SkillName, DateTime> kv in pm.RecentSkillUse )
				if ( DateTime.UtcNow - kv.Value <= window )
					distinct++;

			if ( distinct >= MyServerSettings.VarietySkillsNeeded() )
				pm.SendMessage( 0x59, "Varietà: hai usato {0} abilità diverse di recente, impari più in fretta.", distinct );
			else
				pm.SendMessage( "Varietà: hai usato {0} abilità diverse negli ultimi {1} minuti; con {2} o più impari più in fretta.", distinct, MyServerSettings.VarietyWindowMinutes(), MyServerSettings.VarietySkillsNeeded() );
		}
	}
}
