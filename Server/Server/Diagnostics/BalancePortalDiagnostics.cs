using System;
using System.IO;

namespace Server.Diagnostics
{
	public static class BalancePortalDiagnostics
	{
		private static readonly object m_Lock = new object();

		private static Sample m_Latest;
		private static Sample m_Worst;
		private static Totals m_Totals = new Totals();
		private static ValidationSample m_LastValidation;

		public sealed class Sample
		{
			public long RunId;
			public bool Startup;
			public DateTime StartedUtc;
			public long TotalPhaseMs;
			public long CandidateBuildMs;
			public long WorldItemsScanned;
			public long PortalCandidates;
			public long PortalGoodCandidates;
			public long PortalEvilCandidates;
			public long PlayersChecked;
			public long ApplicablePlayers;
			public long CandidatePortalIterations;
			public long PortalsMatched;
			public long PortalGoodMatched;
			public long PortalEvilMatched;
			public long BalanceEffectCalls;
			public long SleepTransitions;
			public long OriginalEquivalentChecks;
			public long OptimizedActualChecks;
			public long AvoidedChecks;
			public long CandidateBuildErrors;
			public string WorstPlayerByIterations;
			public long WorstPlayerCandidateIterations;
			public long WorstPlayerIterationMatches;
			public string WorstPlayerByMatchedPortals;
			public long WorstPlayerMatchedPortals;
			public long WorstPlayerMatchedIterations;
			public string ValidationStatus;

			public Sample Clone()
			{
				Sample copy = new Sample();
				copy.RunId = RunId;
				copy.Startup = Startup;
				copy.StartedUtc = StartedUtc;
				copy.TotalPhaseMs = TotalPhaseMs;
				copy.CandidateBuildMs = CandidateBuildMs;
				copy.WorldItemsScanned = WorldItemsScanned;
				copy.PortalCandidates = PortalCandidates;
				copy.PortalGoodCandidates = PortalGoodCandidates;
				copy.PortalEvilCandidates = PortalEvilCandidates;
				copy.PlayersChecked = PlayersChecked;
				copy.ApplicablePlayers = ApplicablePlayers;
				copy.CandidatePortalIterations = CandidatePortalIterations;
				copy.PortalsMatched = PortalsMatched;
				copy.PortalGoodMatched = PortalGoodMatched;
				copy.PortalEvilMatched = PortalEvilMatched;
				copy.BalanceEffectCalls = BalanceEffectCalls;
				copy.SleepTransitions = SleepTransitions;
				copy.OriginalEquivalentChecks = OriginalEquivalentChecks;
				copy.OptimizedActualChecks = OptimizedActualChecks;
				copy.AvoidedChecks = AvoidedChecks;
				copy.CandidateBuildErrors = CandidateBuildErrors;
				copy.WorstPlayerByIterations = WorstPlayerByIterations;
				copy.WorstPlayerCandidateIterations = WorstPlayerCandidateIterations;
				copy.WorstPlayerIterationMatches = WorstPlayerIterationMatches;
				copy.WorstPlayerByMatchedPortals = WorstPlayerByMatchedPortals;
				copy.WorstPlayerMatchedPortals = WorstPlayerMatchedPortals;
				copy.WorstPlayerMatchedIterations = WorstPlayerMatchedIterations;
				copy.ValidationStatus = ValidationStatus;
				return copy;
			}
		}

		private sealed class Totals
		{
			public long Runs;
			public long StartupRuns;
			public long TotalPhaseMs;
			public long MaxPhaseMs;
			public long CandidateBuildMs;
			public long WorldItemsScanned;
			public long PortalCandidates;
			public long PlayersChecked;
			public long ApplicablePlayers;
			public long CandidatePortalIterations;
			public long PortalsMatched;
			public long BalanceEffectCalls;
			public long SleepTransitions;
			public long OriginalEquivalentChecks;
			public long OptimizedActualChecks;
			public long AvoidedChecks;

			public Totals Clone()
			{
				Totals copy = new Totals();
				copy.Runs = Runs;
				copy.StartupRuns = StartupRuns;
				copy.TotalPhaseMs = TotalPhaseMs;
				copy.MaxPhaseMs = MaxPhaseMs;
				copy.CandidateBuildMs = CandidateBuildMs;
				copy.WorldItemsScanned = WorldItemsScanned;
				copy.PortalCandidates = PortalCandidates;
				copy.PlayersChecked = PlayersChecked;
				copy.ApplicablePlayers = ApplicablePlayers;
				copy.CandidatePortalIterations = CandidatePortalIterations;
				copy.PortalsMatched = PortalsMatched;
				copy.BalanceEffectCalls = BalanceEffectCalls;
				copy.SleepTransitions = SleepTransitions;
				copy.OriginalEquivalentChecks = OriginalEquivalentChecks;
				copy.OptimizedActualChecks = OptimizedActualChecks;
				copy.AvoidedChecks = AvoidedChecks;
				return copy;
			}
		}

		private sealed class ValidationSample
		{
			public DateTime Utc;
			public long ElapsedMs;
			public long PlayersChecked;
			public long ApplicablePlayers;
			public long WorldItemsScanned;
			public long PortalCandidates;
			public long MatchingPortals;
			public long MismatchCount;
			public string FirstMismatch;
			public long OriginalFingerprint;
			public long CandidateFingerprint;

			public ValidationSample Clone()
			{
				ValidationSample copy = new ValidationSample();
				copy.Utc = Utc;
				copy.ElapsedMs = ElapsedMs;
				copy.PlayersChecked = PlayersChecked;
				copy.ApplicablePlayers = ApplicablePlayers;
				copy.WorldItemsScanned = WorldItemsScanned;
				copy.PortalCandidates = PortalCandidates;
				copy.MatchingPortals = MatchingPortals;
				copy.MismatchCount = MismatchCount;
				copy.FirstMismatch = FirstMismatch;
				copy.OriginalFingerprint = OriginalFingerprint;
				copy.CandidateFingerprint = CandidateFingerprint;
				return copy;
			}
		}

		public static void RecordRun(Sample sample)
		{
			if (sample == null)
				return;

			lock (m_Lock)
			{
				Sample copy = sample.Clone();
				m_Latest = copy;

				if (m_Worst == null || copy.TotalPhaseMs > m_Worst.TotalPhaseMs)
					m_Worst = copy.Clone();

				++m_Totals.Runs;

				if (copy.Startup)
					++m_Totals.StartupRuns;

				m_Totals.TotalPhaseMs += copy.TotalPhaseMs;
				m_Totals.CandidateBuildMs += copy.CandidateBuildMs;
				m_Totals.WorldItemsScanned += copy.WorldItemsScanned;
				m_Totals.PortalCandidates += copy.PortalCandidates;
				m_Totals.PlayersChecked += copy.PlayersChecked;
				m_Totals.ApplicablePlayers += copy.ApplicablePlayers;
				m_Totals.CandidatePortalIterations += copy.CandidatePortalIterations;
				m_Totals.PortalsMatched += copy.PortalsMatched;
				m_Totals.BalanceEffectCalls += copy.BalanceEffectCalls;
				m_Totals.SleepTransitions += copy.SleepTransitions;
				m_Totals.OriginalEquivalentChecks += copy.OriginalEquivalentChecks;
				m_Totals.OptimizedActualChecks += copy.OptimizedActualChecks;
				m_Totals.AvoidedChecks += copy.AvoidedChecks;

				if (copy.TotalPhaseMs > m_Totals.MaxPhaseMs)
					m_Totals.MaxPhaseMs = copy.TotalPhaseMs;
			}
		}

		public static void RecordValidation(
			long elapsedMs,
			long playersChecked,
			long applicablePlayers,
			long worldItemsScanned,
			long portalCandidates,
			long matchingPortals,
			long mismatchCount,
			string firstMismatch,
			long originalFingerprint,
			long candidateFingerprint)
		{
			ValidationSample sample = new ValidationSample();
			sample.Utc = DateTime.UtcNow;
			sample.ElapsedMs = elapsedMs;
			sample.PlayersChecked = playersChecked;
			sample.ApplicablePlayers = applicablePlayers;
			sample.WorldItemsScanned = worldItemsScanned;
			sample.PortalCandidates = portalCandidates;
			sample.MatchingPortals = matchingPortals;
			sample.MismatchCount = mismatchCount;
			sample.FirstMismatch = !String.IsNullOrEmpty(firstMismatch) ? firstMismatch : "none";
			sample.OriginalFingerprint = originalFingerprint;
			sample.CandidateFingerprint = candidateFingerprint;

			lock (m_Lock)
			{
				m_LastValidation = sample;
			}
		}

		public static bool TryGetLatestSample(out Sample sample)
		{
			lock (m_Lock)
			{
				sample = m_Latest != null ? m_Latest.Clone() : null;
				return sample != null;
			}
		}

		public static void WriteStats(TextWriter writer)
		{
			if (writer == null)
				return;

			Sample latest;
			Sample worst;
			Totals totals;
			ValidationSample validation;

			lock (m_Lock)
			{
				latest = m_Latest != null ? m_Latest.Clone() : null;
				worst = m_Worst != null ? m_Worst.Clone() : null;
				totals = m_Totals.Clone();
				validation = m_LastValidation != null ? m_LastValidation.Clone() : null;
			}

			WriteSample(writer, "latest", latest);
			WriteSample(writer, "worst", worst);
			WriteTotals(writer, totals);
			WriteValidation(writer, validation);
		}

		private static void WriteSample(TextWriter writer, string label, Sample sample)
		{
			if (sample == null)
			{
				writer.WriteLine("BalancePortalStats {0}: Unavailable", label);
				return;
			}

			writer.WriteLine(
				"BalancePortalStats {0}: runId={1} startup={2} total={3}ms candidateBuild={4}ms worldItemsScanned={5} portalCandidates={6} goodCandidates={7} evilCandidates={8} playersChecked={9} applicablePlayers={10} candidatePortalIterations={11} portalsMatched={12} goodMatched={13} evilMatched={14} balanceEffectCalls={15} sleepTransitions={16} originalEquivalentChecks={17} optimizedActualChecks={18} avoidedChecks={19} estimatedReduction={20}% candidateBuildErrors={21} validation={22} startedUtc={23}",
				label,
				sample.RunId,
				sample.Startup,
				sample.TotalPhaseMs,
				sample.CandidateBuildMs,
				sample.WorldItemsScanned,
				sample.PortalCandidates,
				sample.PortalGoodCandidates,
				sample.PortalEvilCandidates,
				sample.PlayersChecked,
				sample.ApplicablePlayers,
				sample.CandidatePortalIterations,
				sample.PortalsMatched,
				sample.PortalGoodMatched,
				sample.PortalEvilMatched,
				sample.BalanceEffectCalls,
				sample.SleepTransitions,
				sample.OriginalEquivalentChecks,
				sample.OptimizedActualChecks,
				sample.AvoidedChecks,
				FormatReduction(sample.AvoidedChecks, sample.OriginalEquivalentChecks),
				sample.CandidateBuildErrors,
				!String.IsNullOrEmpty(sample.ValidationStatus) ? sample.ValidationStatus : "Not run",
				sample.StartedUtc);

			writer.WriteLine(
				"  worstPlayerByIterations={0} iterations={1} matches={2}; worstPlayerByMatchedPortals={3} matches={4} iterations={5}",
				!String.IsNullOrEmpty(sample.WorstPlayerByIterations) ? sample.WorstPlayerByIterations : "none",
				sample.WorstPlayerCandidateIterations,
				sample.WorstPlayerIterationMatches,
				!String.IsNullOrEmpty(sample.WorstPlayerByMatchedPortals) ? sample.WorstPlayerByMatchedPortals : "none",
				sample.WorstPlayerMatchedPortals,
				sample.WorstPlayerMatchedIterations);
		}

		private static void WriteTotals(TextWriter writer, Totals totals)
		{
			long avg = totals.Runs > 0 ? totals.TotalPhaseMs / totals.Runs : 0;
			long avgBuild = totals.Runs > 0 ? totals.CandidateBuildMs / totals.Runs : 0;

			writer.WriteLine(
				"BalancePortalStats sinceBoot: runs={0} startupRuns={1} total={2}ms max={3}ms avg={4}ms avgCandidateBuild={5}ms worldItemsScanned={6} portalCandidates={7} playersChecked={8} applicablePlayers={9} candidatePortalIterations={10} portalsMatched={11} balanceEffectCalls={12} sleepTransitions={13} originalEquivalentChecks={14} optimizedActualChecks={15} avoidedChecks={16} estimatedReduction={17}%",
				totals.Runs,
				totals.StartupRuns,
				totals.TotalPhaseMs,
				totals.MaxPhaseMs,
				avg,
				avgBuild,
				totals.WorldItemsScanned,
				totals.PortalCandidates,
				totals.PlayersChecked,
				totals.ApplicablePlayers,
				totals.CandidatePortalIterations,
				totals.PortalsMatched,
				totals.BalanceEffectCalls,
				totals.SleepTransitions,
				totals.OriginalEquivalentChecks,
				totals.OptimizedActualChecks,
				totals.AvoidedChecks,
				FormatReduction(totals.AvoidedChecks, totals.OriginalEquivalentChecks));
		}

		private static void WriteValidation(TextWriter writer, ValidationSample validation)
		{
			if (validation == null)
			{
				writer.WriteLine("BalancePortalValidate: Not run");
				return;
			}

			writer.WriteLine(
				"BalancePortalValidate: utc={0} elapsed={1}ms playersChecked={2} applicablePlayers={3} worldItemsScanned={4} portalCandidates={5} matchingPortals={6} mismatches={7} originalFingerprint={8} candidateFingerprint={9} firstMismatch={10}",
				validation.Utc,
				validation.ElapsedMs,
				validation.PlayersChecked,
				validation.ApplicablePlayers,
				validation.WorldItemsScanned,
				validation.PortalCandidates,
				validation.MatchingPortals,
				validation.MismatchCount,
				validation.OriginalFingerprint,
				validation.CandidateFingerprint,
				!String.IsNullOrEmpty(validation.FirstMismatch) ? validation.FirstMismatch : "none");
		}

		private static string FormatReduction(long avoided, long original)
		{
			if (original <= 0)
				return "0";

			return ((avoided * 10000) / original / 100.0).ToString("F2");
		}
	}
}
