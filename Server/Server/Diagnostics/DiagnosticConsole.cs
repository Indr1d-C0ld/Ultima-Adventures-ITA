using System;
using System.Globalization;

namespace Server.Diagnostics
{
	public enum DiagnosticSeverity
	{
		Trace,
		Info,
		Good,
		Warn,
		Error,
		Critical
	}

	public static class DiagnosticConsole
	{
		private static readonly object m_Sync = new object();
		private static volatile bool m_LagOutputEnabled = true;

		public static bool LagOutputEnabled
		{
			get { return m_LagOutputEnabled; }
			set { m_LagOutputEnabled = value; }
		}

		public static void WriteLine( string tag, DiagnosticSeverity severity, string format, params object[] args )
		{
			if ( !m_LagOutputEnabled && ( IsLagLine( tag ) || ( String.IsNullOrWhiteSpace( tag ) && IsLagLine( format ) ) ) )
				return;

			string message;

			try
			{
				message = args != null && args.Length > 0 ? String.Format( format, args ) : format;
			}
			catch
			{
				message = format;
			}

			WriteParsedLine( String.IsNullOrWhiteSpace( tag ) ? message : tag + " " + message, severity );
		}

		public static void WriteParsedLine( string line )
		{
			WriteParsedLine( line, InferLineSeverity( line ) );
		}

		public static void WriteParsedLine( string format, params object[] args )
		{
			if ( !m_LagOutputEnabled && IsLagLine( format ) )
				return;

			string line;

			try
			{
				line = args != null && args.Length > 0 ? String.Format( format, args ) : format;
			}
			catch
			{
				line = format;
			}

			WriteParsedLine( line );
		}

		public static void WriteParsedLine( string line, DiagnosticSeverity severity )
		{
			if ( line == null )
				line = String.Empty;

			if ( !m_LagOutputEnabled && IsLagLine( line ) )
				return;

			lock ( m_Sync )
			{
				ConsoleColor original;

				try
				{
					original = Console.ForegroundColor;
				}
				catch
				{
					Console.WriteLine( line );
					return;
				}

				try
				{
					WriteColoredLine( line, severity, original );
				}
				catch
				{
					try
					{
						Console.ForegroundColor = original;
					}
					catch
					{
					}

					Console.WriteLine( line );
				}
				finally
				{
					try
					{
						Console.ForegroundColor = original;
					}
					catch
					{
					}
				}
			}
		}

		private static void WriteColoredLine( string line, DiagnosticSeverity severity, ConsoleColor original )
		{
			int index = 0;

			if ( line.Length > 0 && line[0] == '[' )
			{
				int tagEnd = line.IndexOf( ']' );

				if ( tagEnd >= 0 )
				{
					Console.ForegroundColor = GetSeverityColor( severity, original );
					Console.Write( line.Substring( 0, tagEnd + 1 ) );
					index = tagEnd + 1;
				}
			}

			while ( index < line.Length )
			{
				if ( Char.IsWhiteSpace( line[index] ) )
				{
					Console.ForegroundColor = original;
					Console.Write( line[index++] );
					continue;
				}

				int end = index;

				while ( end < line.Length && !Char.IsWhiteSpace( line[end] ) )
					++end;

				WriteToken( line.Substring( index, end - index ), original );
				index = end;
			}

			Console.ForegroundColor = original;
			Console.WriteLine();
		}

		private static void WriteToken( string token, ConsoleColor original )
		{
			string prefix;
			string label;
			string value;
			string suffix;

			if ( TrySplitMetricToken( token, out prefix, out label, out value, out suffix ) )
			{
				Console.ForegroundColor = original;
				Console.Write( prefix );
				Console.Write( label );
				Console.Write( '=' );
				WriteMetricValue( label, value, original );
				Console.ForegroundColor = original;
				Console.Write( suffix );
				return;
			}

			string normalized = token.Trim( ',', ';', ':', '(', ')', '[', ']', '.' );
			long milliseconds;

			if ( String.Equals( normalized, "PASS", StringComparison.OrdinalIgnoreCase ) )
				Console.ForegroundColor = ConsoleColor.Green;
			else if ( String.Equals( normalized, "WARN", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( normalized, "WAITING", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( normalized, "SETTLING", StringComparison.OrdinalIgnoreCase ) )
				Console.ForegroundColor = ConsoleColor.DarkYellow;
			else if ( String.Equals( normalized, "FAIL", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( normalized, "ERROR", StringComparison.OrdinalIgnoreCase ) )
				Console.ForegroundColor = ConsoleColor.Red;
			else if ( String.Equals( normalized, "CRITICAL", StringComparison.OrdinalIgnoreCase ) )
				Console.ForegroundColor = ConsoleColor.Red;
			else if ( TryGetMilliseconds( normalized, out milliseconds ) )
				Console.ForegroundColor = GetDurationColor( milliseconds );
			else
				Console.ForegroundColor = original;

			int durationStart;
			int durationLength;

			if ( TryGetTrailingDuration( token, out durationStart, out durationLength, out milliseconds ) )
			{
				Console.ForegroundColor = original;
				Console.Write( token.Substring( 0, durationStart ) );
				Console.ForegroundColor = GetDurationColor( milliseconds );
				Console.Write( token.Substring( durationStart, durationLength ) );
				Console.ForegroundColor = original;
				Console.Write( token.Substring( durationStart + durationLength ) );
			}
			else
			{
				Console.Write( token );
			}
		}

		private static ConsoleColor GetMetricColor( string label, string value, ConsoleColor original )
		{
			string normalizedLabel = NormalizeMetricName( label );
			string normalizedValue = value.Trim( ',', ';', ':', '(', ')', '[', ']', '.' );
			long milliseconds;
			long numericValue;
			bool boolValue;

			if ( TryGetBoolean( normalizedValue, out boolValue ) )
			{
				if ( IsNeutralBooleanMetric( normalizedLabel ) )
					return original;

				if ( IsBadWhenTrueMetric( normalizedLabel ) )
					return boolValue ? ConsoleColor.Red : ConsoleColor.Green;

				if ( IsGoodWhenTrueMetric( normalizedLabel ) )
					return boolValue ? ConsoleColor.Green : ConsoleColor.Red;

				return original;
			}

			if ( TryGetMilliseconds( normalizedValue, out milliseconds ) )
			{
				if ( IsConfigurationDurationMetric( normalizedLabel ) )
					return original;

				if ( IsOldestReadyMetric( normalizedLabel ) )
					return GetOldestReadyColor( milliseconds );

				return GetDurationColor( milliseconds );
			}

			if ( !TryGetLong( normalizedValue, out numericValue ) )
			{
				if ( String.Equals( normalizedValue, "pass", StringComparison.OrdinalIgnoreCase ) )
					return ConsoleColor.Green;
				if ( String.Equals( normalizedValue, "warn", StringComparison.OrdinalIgnoreCase )
					|| String.Equals( normalizedValue, "waiting", StringComparison.OrdinalIgnoreCase )
					|| String.Equals( normalizedValue, "settling", StringComparison.OrdinalIgnoreCase ) )
					return ConsoleColor.DarkYellow;
				if ( String.Equals( normalizedValue, "fail", StringComparison.OrdinalIgnoreCase )
					|| String.Equals( normalizedValue, "error", StringComparison.OrdinalIgnoreCase )
					|| String.Equals( normalizedValue, "critical", StringComparison.OrdinalIgnoreCase ) )
					return ConsoleColor.Red;

				return original;
			}

			if ( IsNeutralNumericMetric( normalizedLabel ) )
				return original;

			if ( IsCoreDurationMetric( normalizedLabel ) )
				return GetDurationColor( numericValue );

			if ( IsZeroIsGoodMetric( normalizedLabel ) )
				return numericValue == 0 ? ConsoleColor.Green : ConsoleColor.Red;

			if ( IsDeferredPressureMetric( normalizedLabel ) )
			{
				if ( numericValue == 0 )
					return ConsoleColor.Green;
				if ( numericValue <= 100 )
					return ConsoleColor.DarkYellow;
				return ConsoleColor.Red;
			}

			if ( IsSlowCountMetric( normalizedLabel ) )
			{
				if ( numericValue == 0 )
					return ConsoleColor.Green;
				if ( numericValue <= 10 )
					return ConsoleColor.DarkYellow;
				return ConsoleColor.Red;
			}

			if ( IsBacklogPressureMetric( normalizedLabel ) )
			{
				if ( numericValue <= 1000 )
					return ConsoleColor.Green;
				if ( numericValue <= 5000 )
					return ConsoleColor.DarkYellow;
				return ConsoleColor.Red;
			}

			if ( IsProcessedDueMetric( normalizedLabel ) )
				return ConsoleColor.Green;

			if ( normalizedLabel.IndexOf( "duplicateprevented" ) >= 0 && numericValue == 0 )
				return ConsoleColor.Green;

			return original;
		}

		private static void WriteMetricValue( string label, string value, ConsoleColor original )
		{
			ConsoleColor color = GetMetricColor( label, value, original );

			if ( color != original )
			{
				Console.ForegroundColor = color;
				Console.Write( value );
				return;
			}

			long milliseconds;
			int durationStart;
			int durationLength;

			if ( TryGetTrailingDuration( value, out durationStart, out durationLength, out milliseconds ) )
			{
				Console.ForegroundColor = original;
				Console.Write( value.Substring( 0, durationStart ) );
				Console.ForegroundColor = GetDurationColor( milliseconds );
				Console.Write( value.Substring( durationStart, durationLength ) );
				Console.ForegroundColor = original;
				Console.Write( value.Substring( durationStart + durationLength ) );
				return;
			}

			Console.ForegroundColor = original;
			Console.Write( value );
		}

		private static DiagnosticSeverity InferLineSeverity( string line )
		{
			if ( String.IsNullOrWhiteSpace( line ) )
				return DiagnosticSeverity.Info;

			if ( line.IndexOf( "CRITICAL", StringComparison.OrdinalIgnoreCase ) >= 0 )
				return DiagnosticSeverity.Critical;
			if ( line.IndexOf( "ERROR", StringComparison.OrdinalIgnoreCase ) >= 0
				|| line.IndexOf( "FAIL", StringComparison.OrdinalIgnoreCase ) >= 0 )
				return DiagnosticSeverity.Error;
			if ( line.IndexOf( "WARN", StringComparison.OrdinalIgnoreCase ) >= 0 )
				return DiagnosticSeverity.Warn;

			DiagnosticSeverity premiumSpawnerSeverity;

			if ( TryGetPremiumSpawnerBacklogSeverity( line, out premiumSpawnerSeverity ) )
				return premiumSpawnerSeverity;

			long maximumMilliseconds = GetSeverityMilliseconds( line );

			if ( maximumMilliseconds > 1000 )
				return DiagnosticSeverity.Critical;
			if ( maximumMilliseconds > 250 )
				return DiagnosticSeverity.Error;
			if ( maximumMilliseconds >= 100 )
				return DiagnosticSeverity.Warn;
			if ( maximumMilliseconds > 0 )
				return DiagnosticSeverity.Good;
			if ( line.IndexOf( "PASS", StringComparison.OrdinalIgnoreCase ) >= 0 )
				return DiagnosticSeverity.Good;

			return DiagnosticSeverity.Info;
		}

		private static bool TryGetPremiumSpawnerBacklogSeverity( string line, out DiagnosticSeverity severity )
		{
			severity = DiagnosticSeverity.Info;

			if ( line.IndexOf( "PremiumSpawner scheduler backlog", StringComparison.OrdinalIgnoreCase ) < 0 )
				return false;

			long oldestReady;
			long oldestUnpromoted;
			long readyCount;
			long futureCount;
			long unpromotedDue;
			long newlyBudgetDeferred;

			TryGetNamedLong( line, "oldestReady", true, out oldestReady );
			TryGetNamedLong( line, "oldestUnpromoted", true, out oldestUnpromoted );
			TryGetNamedLong( line, "readyCount", false, out readyCount );
			TryGetNamedLong( line, "futureCount", false, out futureCount );
			TryGetNamedLong( line, "unpromotedDue", false, out unpromotedDue );
			TryGetNamedLong( line, "newlyBudgetDeferred", false, out newlyBudgetDeferred );

			long oldest = Math.Max( oldestReady, oldestUnpromoted );
			long backlog = Math.Max( readyCount, futureCount );
			long deferred = Math.Max( unpromotedDue, newlyBudgetDeferred );

			if ( oldest > 30000 || backlog > 5000 || deferred > 100 )
				severity = DiagnosticSeverity.Error;
			else if ( oldest > 15000 || backlog > 1000 || deferred > 0 )
				severity = DiagnosticSeverity.Warn;
			else
				severity = DiagnosticSeverity.Good;

			return true;
		}

		private static long GetSeverityMilliseconds( string line )
		{
			long milliseconds;

			if ( TryGetMillisecondsAfter( line, " took ", out milliseconds ) )
				return milliseconds;

			if ( line.IndexOf( "TimerStats", StringComparison.OrdinalIgnoreCase ) >= 0
				|| line.IndexOf( "Stats 60s", StringComparison.OrdinalIgnoreCase ) >= 0 )
			{
				milliseconds = GetNamedMaximumMilliseconds( line, "max" );

				if ( milliseconds > 0 )
					return milliseconds;
			}

			if ( line.IndexOf( "TimerSliceBreakdown", StringComparison.OrdinalIgnoreCase ) >= 0
				&& TryGetNamedMilliseconds( line, "slice", out milliseconds ) )
				return milliseconds;

			return GetMaximumMilliseconds( line );
		}

		private static long GetMaximumMilliseconds( string line )
		{
			long maximum = 0;
			int index = 0;

			while ( index < line.Length )
			{
				if ( !Char.IsDigit( line[index] ) )
				{
					++index;
					continue;
				}

				int start = index;

				while ( index < line.Length && Char.IsDigit( line[index] ) )
					++index;

				if ( index + 1 < line.Length
					&& ( line[index] == 'm' || line[index] == 'M' )
					&& ( line[index + 1] == 's' || line[index + 1] == 'S' ) )
				{
					long value;

					if ( TryGetLong( line.Substring( start, index - start ), out value ) && value > maximum )
						maximum = value;
				}
			}

			return maximum;
		}

		private static bool TryGetMilliseconds( string value, out long milliseconds )
		{
			milliseconds = 0;

			if ( String.IsNullOrWhiteSpace( value ) || !value.EndsWith( "ms", StringComparison.OrdinalIgnoreCase ) )
				return false;

			return TryGetLong( value.Substring( 0, value.Length - 2 ), out milliseconds );
		}

		private static bool TryGetMillisecondsAfter( string line, string marker, out long milliseconds )
		{
			milliseconds = 0;
			int markerIndex = line.IndexOf( marker, StringComparison.OrdinalIgnoreCase );

			if ( markerIndex < 0 )
				return false;

			int start = markerIndex + marker.Length;
			int end = start;

			while ( end < line.Length && Char.IsDigit( line[end] ) )
				++end;

			return end > start
				&& end + 1 < line.Length
				&& ( line[end] == 'm' || line[end] == 'M' )
				&& ( line[end + 1] == 's' || line[end + 1] == 'S' )
				&& TryGetLong( line.Substring( start, end - start ), out milliseconds );
		}

		private static bool TryGetNamedMilliseconds( string line, string name, out long milliseconds )
		{
			milliseconds = 0;
			string marker = name + "=";
			int index = 0;

			while ( index < line.Length )
			{
				index = line.IndexOf( marker, index, StringComparison.OrdinalIgnoreCase );

				if ( index < 0 )
					return false;

				if ( index == 0 || !Char.IsLetterOrDigit( line[index - 1] ) )
				{
					int start = index + marker.Length;
					int end = start;

					while ( end < line.Length && Char.IsDigit( line[end] ) )
						++end;

					if ( end > start
						&& end + 1 < line.Length
						&& ( line[end] == 'm' || line[end] == 'M' )
						&& ( line[end + 1] == 's' || line[end + 1] == 'S' )
						&& TryGetLong( line.Substring( start, end - start ), out milliseconds ) )
						return true;
				}

				index += marker.Length;
			}

			return false;
		}

		private static bool TryGetNamedLong( string line, string name, bool allowMilliseconds, out long value )
		{
			value = 0;
			string marker = name + "=";
			int index = line.IndexOf( marker, StringComparison.OrdinalIgnoreCase );

			if ( index < 0 )
				return false;

			int start = index + marker.Length;
			int end = start;

			while ( end < line.Length && Char.IsDigit( line[end] ) )
				++end;

			if ( end == start || !TryGetLong( line.Substring( start, end - start ), out value ) )
				return false;

			if ( allowMilliseconds
				&& end + 1 < line.Length
				&& ( line[end] == 'm' || line[end] == 'M' )
				&& ( line[end + 1] == 's' || line[end + 1] == 'S' ) )
				return true;

			return !allowMilliseconds;
		}

		private static long GetNamedMaximumMilliseconds( string line, string name )
		{
			long maximum = 0;
			string marker = name + "=";
			int index = 0;

			while ( index < line.Length )
			{
				index = line.IndexOf( marker, index, StringComparison.OrdinalIgnoreCase );

				if ( index < 0 )
					break;

				if ( index == 0 || !Char.IsLetterOrDigit( line[index - 1] ) )
				{
					int start = index + marker.Length;
					int end = start;

					while ( end < line.Length && Char.IsDigit( line[end] ) )
						++end;

					long value;

					if ( end > start
						&& end + 1 < line.Length
						&& ( line[end] == 'm' || line[end] == 'M' )
						&& ( line[end + 1] == 's' || line[end + 1] == 'S' )
						&& TryGetLong( line.Substring( start, end - start ), out value )
						&& value > maximum )
						maximum = value;
				}

				index += marker.Length;
			}

			return maximum;
		}

		private static bool TryGetLong( string value, out long result )
		{
			return Int64.TryParse( value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result );
		}

		private static bool TryGetBoolean( string value, out bool result )
		{
			if ( String.Equals( value, "true", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( value, "on", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( value, "available", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( value, "pass", StringComparison.OrdinalIgnoreCase ) )
			{
				result = true;
				return true;
			}

			if ( String.Equals( value, "false", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( value, "off", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( value, "unavailable", StringComparison.OrdinalIgnoreCase )
				|| String.Equals( value, "fail", StringComparison.OrdinalIgnoreCase ) )
			{
				result = false;
				return true;
			}

			result = false;
			return false;
		}

		private static bool TrySplitMetricToken( string token, out string prefix, out string label, out string value, out string suffix )
		{
			prefix = String.Empty;
			label = String.Empty;
			value = String.Empty;
			suffix = String.Empty;

			int equals = token.IndexOf( '=' );

			if ( equals <= 0 )
				return false;

			int labelStart = 0;

			while ( labelStart < equals && IsMetricPrefix( token[labelStart] ) )
				++labelStart;

			if ( labelStart >= equals )
				return false;

			int valueEnd = token.Length;

			while ( valueEnd > equals + 1 && IsMetricSuffix( token[valueEnd - 1] ) )
				--valueEnd;

			prefix = token.Substring( 0, labelStart );
			label = token.Substring( labelStart, equals - labelStart );
			value = token.Substring( equals + 1, valueEnd - equals - 1 );
			suffix = token.Substring( valueEnd );
			return true;
		}

		private static bool TryGetTrailingDuration( string value, out int durationStart, out int durationLength, out long milliseconds )
		{
			durationStart = 0;
			durationLength = 0;
			milliseconds = 0;

			int end = value.Length;

			while ( end > 0 && IsMetricSuffix( value[end - 1] ) )
				--end;

			if ( end < 3
				|| ( value[end - 2] != 'm' && value[end - 2] != 'M' )
				|| ( value[end - 1] != 's' && value[end - 1] != 'S' ) )
				return false;

			int start = end - 2;

			while ( start > 0 && Char.IsDigit( value[start - 1] ) )
				--start;

			if ( start == end - 2 || !TryGetLong( value.Substring( start, end - start - 2 ), out milliseconds ) )
				return false;

			durationStart = start;
			durationLength = end - start;
			return true;
		}

		private static bool IsMetricPrefix( char value )
		{
			return value == '(' || value == '[' || value == '{' || value == ',' || value == ';';
		}

		private static bool IsMetricSuffix( char value )
		{
			return value == ',' || value == ';' || value == ':' || value == ')' || value == ']' || value == '}' || value == '.';
		}

		private static string NormalizeMetricName( string label )
		{
			if ( String.IsNullOrEmpty( label ) )
				return String.Empty;

			int start = 0;
			int end = label.Length;

			while ( start < end && !Char.IsLetterOrDigit( label[start] ) )
				++start;

			while ( end > start && !Char.IsLetterOrDigit( label[end - 1] ) )
				--end;

			return label.Substring( start, end - start ).ToLowerInvariant();
		}

		private static bool IsNeutralBooleanMetric( string label )
		{
			return label == "repeating"
				|| label == "startup"
				|| label == "manual"
				|| label == "forced"
				|| label == "duenow"
				|| label == "due"
				|| label == "fromcache"
				|| label == "regioncheck"
				|| label == "direct"
				|| label == "commandinitiated"
				|| label.IndexOf( "oldschedule" ) >= 0;
		}

		private static bool IsBadWhenTrueMetric( string label )
		{
			return label.IndexOf( "error" ) >= 0
				|| label.IndexOf( "failed" ) >= 0
				|| label.IndexOf( "timeout" ) >= 0
				|| label.IndexOf( "inflight" ) >= 0
				|| label.IndexOf( "saving" ) >= 0
				|| label.IndexOf( "settling" ) >= 0
				|| label.IndexOf( "refused" ) >= 0
				|| label.IndexOf( "cancelled" ) >= 0
				|| label.IndexOf( "overtarget" ) >= 0
				|| label.IndexOf( "overbudget" ) >= 0
				|| label.IndexOf( "exceeded" ) >= 0
				|| label.IndexOf( "critical" ) >= 0;
		}

		private static bool IsGoodWhenTrueMetric( string label )
		{
			return label.IndexOf( "enabled" ) >= 0
				|| label.IndexOf( "registryready" ) >= 0
				|| label.IndexOf( "success" ) >= 0
				|| label.IndexOf( "complete" ) >= 0
				|| label.IndexOf( "pass" ) >= 0
				|| label.IndexOf( "healthy" ) >= 0
				|| label.IndexOf( "connected" ) >= 0;
		}

		private static bool IsConfigurationDurationMetric( string label )
		{
			return label == "delay"
				|| label == "interval"
				|| label.IndexOf( "processlimit" ) >= 0
				|| label.IndexOf( "budgetlimit" ) >= 0
				|| label.IndexOf( "target" ) >= 0
				|| label.IndexOf( "warning" ) >= 0;
		}

		private static bool IsCoreDurationMetric( string label )
		{
			return label == "timer"
				|| label == "messagepump"
				|| label == "flushall"
				|| label == "disposed"
				|| label == "sliceevent"
				|| label == "slice"
				|| label == "named"
				|| label == "other"
				|| label == "callbackms"
				|| label == "maxcallback";
		}

		private static bool IsNeutralNumericMetric( string label )
		{
			return label == "online"
				|| label == "mobiledelta"
				|| label == "itemdelta"
				|| label == "processed"
				|| label == "checked"
				|| label == "created"
				|| label == "deleted"
				|| label == "changed"
				|| label == "priority"
				|| label == "delay"
				|| label == "interval"
				|| label == "nextdelay"
				|| label.IndexOf( "processlimit" ) >= 0
				|| label.IndexOf( "budgetlimit" ) >= 0
				|| label.IndexOf( "adaptivelevel" ) >= 0;
		}

		private static bool IsZeroIsGoodMetric( string label )
		{
			return label.IndexOf( "error" ) >= 0
				|| label.IndexOf( "failed" ) >= 0
				|| label == "fail"
				|| label.IndexOf( "timeout" ) >= 0
				|| label.IndexOf( "partialsend" ) >= 0
				|| label.IndexOf( "unknownorigin" ) >= 0
				|| label.IndexOf( "oldinternaltimer" ) >= 0
				|| label.IndexOf( "diagnosticerror" ) >= 0;
		}

		private static bool IsDeferredPressureMetric( string label )
		{
			return label.IndexOf( "budgetdeferreddue" ) >= 0
				|| label.IndexOf( "newlybudgetdeferred" ) >= 0
				|| label.IndexOf( "unpromoteddue" ) >= 0;
		}

		private static bool IsSlowCountMetric( string label )
		{
			return label == "slow" || label == "slowcalls" || label == "slowcount";
		}

		private static bool IsBacklogPressureMetric( string label )
		{
			if ( label.IndexOf( "processed" ) >= 0
				|| label.IndexOf( "processlimit" ) >= 0
				|| label.IndexOf( "budgetlimit" ) >= 0 )
				return false;

			return label.IndexOf( "readycount" ) >= 0
				|| label.IndexOf( "readyqueue" ) >= 0
				|| label.IndexOf( "futurecount" ) >= 0
				|| label.IndexOf( "backlog" ) >= 0
				|| label == "queue"
				|| label == "queuemax"
				|| label.IndexOf( "promoteddue" ) >= 0
				|| label.IndexOf( "stagedcallbacks" ) >= 0;
		}

		private static bool IsProcessedDueMetric( string label )
		{
			return label.IndexOf( "processeddue" ) >= 0;
		}

		private static bool IsOldestReadyMetric( string label )
		{
			return label.IndexOf( "oldestready" ) >= 0
				|| label.IndexOf( "oldestunpromoted" ) >= 0;
		}

		private static ConsoleColor GetOldestReadyColor( long milliseconds )
		{
			if ( milliseconds <= 15000 )
				return ConsoleColor.Green;
			if ( milliseconds <= 30000 )
				return ConsoleColor.DarkYellow;
			return ConsoleColor.Red;
		}

		private static ConsoleColor GetDurationColor( long milliseconds )
		{
			if ( milliseconds < 100 )
				return ConsoleColor.Green;
			if ( milliseconds <= 250 )
				return ConsoleColor.DarkYellow;
			return ConsoleColor.Red;
		}

		private static ConsoleColor GetSeverityColor( DiagnosticSeverity severity, ConsoleColor original )
		{
			switch ( severity )
			{
				case DiagnosticSeverity.Good: return ConsoleColor.Green;
				case DiagnosticSeverity.Warn: return ConsoleColor.DarkYellow;
				case DiagnosticSeverity.Error: return ConsoleColor.Red;
				case DiagnosticSeverity.Critical: return ConsoleColor.Red;
				default: return original;
			}
		}

		private static bool IsLagLine( string line )
		{
			return !String.IsNullOrEmpty( line ) && line.StartsWith( "[Lag]", StringComparison.OrdinalIgnoreCase );
		}
	}
}
