using System;
using System.Collections.Generic;
using Server;

namespace Server.Mobiles
{
    public sealed class StableCustomPetList
    {
        private readonly List<Serial> m_PetSerials;
        private string m_Name;

        public StableCustomPetList( string name )
        {
            m_Name = StableMasterListState.NormalizeListName( name );
            m_PetSerials = new List<Serial>();
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = StableMasterListState.NormalizeListName( value ); }
        }

        public List<Serial> PetSerials
        {
            get { return m_PetSerials; }
        }

        public bool ContainsPet( Serial petSerial )
        {
            return StableMasterListState.ContainsSerial( m_PetSerials, petSerial );
        }

        public bool AddPet( Serial petSerial )
        {
            if ( !StableMasterListState.IsUsableSerial( petSerial ) )
                return false;

            if ( ContainsPet( petSerial ) )
                return false;

            if ( m_PetSerials.Count >= StableMasterListState.MaxPetsPerCustomList )
                return false;

            m_PetSerials.Add( petSerial );
            return true;
        }

        public bool RemovePet( Serial petSerial )
        {
            for ( int i = 0; i < m_PetSerials.Count; ++i )
            {
                if ( m_PetSerials[i] == petSerial )
                {
                    m_PetSerials.RemoveAt( i );
                    return true;
                }
            }

            return false;
        }

        public int PruneInvalidPets( IList<Serial> validPetSerials )
        {
            int removed = 0;

            for ( int i = m_PetSerials.Count - 1; i >= 0; --i )
            {
                if ( !StableMasterListState.ContainsSerial( validPetSerials, m_PetSerials[i] ) )
                {
                    m_PetSerials.RemoveAt( i );
                    removed++;
                }
            }

            return removed;
        }

        public void Serialize( GenericWriter writer )
        {
            writer.Write( (int)0 );
            writer.Write( m_Name );
            writer.Write( m_PetSerials.Count );

            for ( int i = 0; i < m_PetSerials.Count; ++i )
                writer.Write( m_PetSerials[i] );
        }

        public static StableCustomPetList Deserialize( GenericReader reader )
        {
            int version = reader.ReadInt();

            string name = reader.ReadString();
            StableCustomPetList list = new StableCustomPetList( name );

            int count = reader.ReadInt();

            for ( int i = 0; i < count; ++i )
            {
                Serial petSerial = (Serial)reader.ReadInt();
                list.AddPet( petSerial );
            }

            return list;
        }
    }

    public sealed class StableMasterListState
    {
        public const int MaxSearchLength = 64;
        public const int MaxListNameLength = 32;
        public const int MaxFavoritePets = 200;
        public const int MaxCustomLists = 12;
        public const int MaxPetsPerCustomList = 200;
        public const string SearchPlaceholder = "Type here...";

        private readonly List<Serial> m_FavoritePetSerials;
        private readonly List<StableCustomPetList> m_CustomLists;

        public StableMasterListState()
        {
            m_FavoritePetSerials = new List<Serial>();
            m_CustomLists = new List<StableCustomPetList>();
        }

        public List<Serial> FavoritePetSerials
        {
            get { return m_FavoritePetSerials; }
        }

        public List<StableCustomPetList> CustomLists
        {
            get { return m_CustomLists; }
        }

        public static bool IsUsableSerial( Serial serial )
        {
            return serial != Serial.Zero && serial != Serial.MinusOne;
        }

        public static bool ContainsSerial( IList<Serial> serials, Serial serial )
        {
            if ( serials == null )
                return false;

            for ( int i = 0; i < serials.Count; ++i )
            {
                if ( serials[i] == serial )
                    return true;
            }

            return false;
        }

        public static string NormalizeSearchTerm( string term )
        {
            if ( String.IsNullOrEmpty( term ) )
                return String.Empty;

            term = term.Trim();

            if ( String.Equals( term, SearchPlaceholder, StringComparison.OrdinalIgnoreCase ) )
                return String.Empty;

            if ( term.Length > MaxSearchLength )
                term = term.Substring( 0, MaxSearchLength );

            return term;
        }

        public static string NormalizeListName( string name )
        {
            if ( String.IsNullOrEmpty( name ) )
                return String.Empty;

            name = name.Trim();

            if ( name.Length > MaxListNameLength )
                name = name.Substring( 0, MaxListNameLength );

            return name;
        }

        public bool IsFavorite( Serial petSerial )
        {
            return ContainsSerial( m_FavoritePetSerials, petSerial );
        }

        public bool AddFavorite( Serial petSerial )
        {
            if ( !IsUsableSerial( petSerial ) )
                return false;

            if ( IsFavorite( petSerial ) )
                return false;

            if ( m_FavoritePetSerials.Count >= MaxFavoritePets )
                return false;

            m_FavoritePetSerials.Add( petSerial );
            return true;
        }

        public bool RemoveFavorite( Serial petSerial )
        {
            for ( int i = 0; i < m_FavoritePetSerials.Count; ++i )
            {
                if ( m_FavoritePetSerials[i] == petSerial )
                {
                    m_FavoritePetSerials.RemoveAt( i );
                    return true;
                }
            }

            return false;
        }

        public bool ToggleFavorite( Serial petSerial )
        {
            if ( IsFavorite( petSerial ) )
            {
                RemoveFavorite( petSerial );
                return false;
            }

            AddFavorite( petSerial );
            return IsFavorite( petSerial );
        }

        public StableCustomPetList FindList( string name )
        {
            string normalized = NormalizeListName( name );

            if ( normalized.Length == 0 )
                return null;

            for ( int i = 0; i < m_CustomLists.Count; ++i )
            {
                StableCustomPetList list = m_CustomLists[i];

                if ( list != null && String.Equals( list.Name, normalized, StringComparison.OrdinalIgnoreCase ) )
                    return list;
            }

            return null;
        }

        public StableCustomPetList GetOrCreateList( string name )
        {
            string normalized = NormalizeListName( name );

            if ( normalized.Length == 0 )
                return null;

            StableCustomPetList existing = FindList( normalized );

            if ( existing != null )
                return existing;

            if ( m_CustomLists.Count >= MaxCustomLists )
                return null;

            StableCustomPetList list = new StableCustomPetList( normalized );
            m_CustomLists.Add( list );
            return list;
        }

        public bool RemoveListAt( int index )
        {
            if ( index < 0 || index >= m_CustomLists.Count )
                return false;

            m_CustomLists.RemoveAt( index );
            return true;
        }

        public int PruneInvalidPets( IList<Serial> validPetSerials )
        {
            int removed = 0;

            for ( int i = m_FavoritePetSerials.Count - 1; i >= 0; --i )
            {
                if ( !ContainsSerial( validPetSerials, m_FavoritePetSerials[i] ) )
                {
                    m_FavoritePetSerials.RemoveAt( i );
                    removed++;
                }
            }

            for ( int i = 0; i < m_CustomLists.Count; ++i )
            {
                StableCustomPetList list = m_CustomLists[i];

                if ( list != null )
                    removed += list.PruneInvalidPets( validPetSerials );
            }

            return removed;
        }

        public void Serialize( GenericWriter writer )
        {
            writer.Write( (int)0 );
            writer.Write( m_FavoritePetSerials.Count );

            for ( int i = 0; i < m_FavoritePetSerials.Count; ++i )
                writer.Write( m_FavoritePetSerials[i] );

            writer.Write( m_CustomLists.Count );

            for ( int i = 0; i < m_CustomLists.Count; ++i )
                m_CustomLists[i].Serialize( writer );
        }

        public static StableMasterListState Deserialize( GenericReader reader )
        {
            StableMasterListState state = new StableMasterListState();
            int version = reader.ReadInt();

            int favorites = reader.ReadInt();

            for ( int i = 0; i < favorites; ++i )
            {
                Serial petSerial = (Serial)reader.ReadInt();
                state.AddFavorite( petSerial );
            }

            int lists = reader.ReadInt();

            for ( int i = 0; i < lists; ++i )
            {
                StableCustomPetList list = StableCustomPetList.Deserialize( reader );

                if ( list != null && list.Name.Length > 0 && state.FindList( list.Name ) == null && state.CustomLists.Count < MaxCustomLists )
                    state.CustomLists.Add( list );
            }

            return state;
        }
    }
}
