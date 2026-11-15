using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
        public static class PatchVersionInfo
        {
                private static bool m_OverrideSet;
                private static string m_Override;

                public static void Override( string version )
                {
                        m_Override = String.IsNullOrWhiteSpace( version ) ? null : version.Trim();
                        m_OverrideSet = true;
                }

                public static string Get()
                {
                        if ( m_OverrideSet )
                                return m_Override;

                        string envValue = Environment.GetEnvironmentVariable( "RUNUO_PATCH_VERSION" );

                        if ( !String.IsNullOrWhiteSpace( envValue ) )
                                return envValue.Trim();

                        foreach ( string path in GetCandidatePaths() )
                        {
                                try
                                {
                                        if ( File.Exists( path ) )
                                        {
                                                foreach ( string line in File.ReadAllLines( path ) )
                                                {
                                                        string contents = line.Trim();

                                                        if ( contents.Length == 0 || contents.StartsWith( "#" ) || contents.StartsWith( "//" ) || contents.StartsWith( ";" ) )
                                                                continue;

                                                        return contents;
                                                }
                                        }
                                }
                                catch
                                {
                                }
                        }

                        return null;
                }

                private static IEnumerable<string> GetCandidatePaths()
                {
                        string baseDirectory = Core.BaseDirectory;

                        yield return Path.Combine( baseDirectory, "PatchVersion.txt" );
                        yield return Path.Combine( baseDirectory, "patchversion.txt" );
                        yield return Path.Combine( baseDirectory, "Info", "PatchVersion.txt" );
                        yield return Path.Combine( baseDirectory, "Info", "patchversion.txt" );
                }
        }
}
