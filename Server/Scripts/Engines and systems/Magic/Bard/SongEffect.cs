using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Spells.Song;

namespace Server.Spells.Song
{

	public class SongEffect 
	{
		private Serial m_MobileSerial;
		private Song m_Song;
		private int m_Strength;
		private int m_FullDuration;
		private int m_RemainingDuration;
		private StatMod m_StatMod;
		private ResistanceMod m_ResistanceMod;

		public Serial MobileSerial
		{ 
			get { return m_MobileSerial; } 
			set { m_MobileSerial = value; } 
		}

		public Song Song
		{ 
			get { return m_Song; } 
			set { m_Song = value; } 
		}

		public int Strength
		{
			get { return m_Strength; }
		}

		public int FullDuration
		{
			get { return m_FullDuration; }
		}

		public int RemainingDuration
		{
			get { return m_RemainingDuration; }
		}

		public StatMod StatMod
		{
			get { return m_StatMod; }
			set { m_StatMod = value; }
		}

		public ResistanceMod ResistanceMod
		{
			get { return m_ResistanceMod; }
			set { m_ResistanceMod = value; }
		}

		public SongEffect(Serial mobileSerial, Song song) : this(mobileSerial, song, 0, GetSongDuration(song))
		{
		}

		public SongEffect(Serial mobileSerial, Song song, int strength, int duration) {
			Song = song;
			MobileSerial = mobileSerial;
			m_Strength = strength;
			m_FullDuration = duration;
			m_RemainingDuration = duration;
		}

		public void ResetDuration()
		{
			m_RemainingDuration = m_FullDuration;
		}

		public bool TickDuration()
		{
			if (m_RemainingDuration <= 0)
				return false;

			m_RemainingDuration--;
			return m_RemainingDuration > 0;
		}

		private static int GetSongDuration(Song song)
		{
			ModifierSong modifierSong = song as ModifierSong;
			return modifierSong == null ? 0 : modifierSong.Duration;
		}
	}
}