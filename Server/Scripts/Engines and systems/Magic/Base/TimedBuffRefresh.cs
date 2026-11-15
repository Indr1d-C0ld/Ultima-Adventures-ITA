using System;

namespace Server.Spells
{
	public enum TimedBuffRefreshDecision
	{
		Apply,
		RefreshExisting,
		ReplaceExisting,
		Reject
	}

	public delegate void TimedBuffExpireCallback( TimedBuffState state );

	public static class TimedBuffRefresh
	{
		public static TimedBuffRefreshDecision Decide(
			bool active,
			double currentStrength,
			double candidateStrength,
			bool locked )
		{
			if ( locked )
				return TimedBuffRefreshDecision.Reject;
			if ( !active )
				return TimedBuffRefreshDecision.Apply;
			if ( candidateStrength < currentStrength )
				return TimedBuffRefreshDecision.RefreshExisting;
			return TimedBuffRefreshDecision.ReplaceExisting;
		}
	}

	public sealed class TimedBuffState
	{
		private object m_Owner;
		private Mobile m_Source;
		private double m_Strength;
		private TimeSpan m_FullDuration;
		private DateTime m_Expires;
		private int m_Generation;
		private TimedBuffExpireCallback m_Expire;
		private Timer m_Timer;

		public object Owner { get { return m_Owner; } }
		public Mobile Source { get { return m_Source; } }
		public double Strength { get { return m_Strength; } }
		public TimeSpan FullDuration { get { return m_FullDuration; } }
		public DateTime Expires { get { return m_Expires; } }
		public int Generation { get { return m_Generation; } }

		public TimedBuffState( object owner, Mobile source, double strength, TimeSpan duration, TimedBuffExpireCallback expire )
		{
			m_Owner = owner;
			m_Source = source;
			m_Strength = strength;
			m_FullDuration = duration;
			m_Expire = expire;

			Restart();
		}

		public void Refresh()
		{
			Restart();
		}

		public void Replace( Mobile source, double strength, TimeSpan duration, TimedBuffExpireCallback expire )
		{
			m_Source = source;
			m_Strength = strength;
			m_FullDuration = duration;
			m_Expire = expire;

			Restart();
		}

		public void Stop()
		{
			if ( m_Timer != null )
			{
				m_Timer.Stop();
				m_Timer = null;
			}

			++m_Generation;
		}

		private void Restart()
		{
			if ( m_Timer != null )
				m_Timer.Stop();

			++m_Generation;
			m_Expires = DateTime.UtcNow.Add( m_FullDuration );
			m_Timer = new ExpireTimer( this, m_Generation, m_FullDuration );
			m_Timer.Start();
		}

		private sealed class ExpireTimer : Timer
		{
			private TimedBuffState m_State;
			private int m_Generation;

			public ExpireTimer( TimedBuffState state, int generation, TimeSpan duration )
				: base( duration )
			{
				m_State = state;
				m_Generation = generation;
			}

			protected override void OnTick()
			{
				if ( m_State.Generation == m_Generation )
				{
					m_State.m_Timer = null;

					if ( m_State.m_Expire != null )
						m_State.m_Expire( m_State );
				}
			}
		}
	}
}
