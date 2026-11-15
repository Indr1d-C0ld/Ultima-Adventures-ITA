using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Server.Diagnostics
{
	public enum CitizenConstructorPhase
	{
		BaseConstructor,
		NameBodySetup,
		DressUp,
		SetupCitizenTotal,
		BackpackReplace,
		OneHandLayerLookup,
		TwoHandLayerLookup,
		MaterialColorOrMorph,
		RandomDungeonCitySetup,
		RandomQuestItemSetup,
		PhraseRumorGeneration,
		TitleSkinHairHue,
		StatResistanceSkillSetup,
		FinalFlags,
		SubclassConstructorExtra,
		OtherUnattributed
	}

	public enum CitizenPhraseHelper
	{
		SomePlace,
		MadeUpDungeon,
		GetRandomCity,
		MadeUpCity,
		Adventurer,
		GetSpecialItem,
		QuestItems,
		ContainerOwner,
		RandomWords,
		GetRandomJob,
		GetRareLocation,
		CommonTalk,
		TopicTemplateFormatting,
		PrefaceSelection,
		ServiceSelection,
		PhraseGeneration,
		BackpackFormatting,
		OtherUnattributed
	}

	public struct CitizenConstructorSample
	{
		public string TypeName;
		public string Source;
		public string DressUpKind;
		public string Error;
		public long TotalMs;
		public long BaseConstructorMs;
		public long NameBodySetupMs;
		public long DressUpMs;
		public long SetupCitizenTotalMs;
		public long BackpackReplaceMs;
		public long OneHandLayerLookupMs;
		public long TwoHandLayerLookupMs;
		public long MaterialColorOrMorphMs;
		public long RandomDungeonCitySetupMs;
		public long RandomQuestItemSetupMs;
		public long PhraseRumorGenerationMs;
		public long TitleSkinHairHueMs;
		public long StatResistanceSkillSetupMs;
		public long FinalFlagsMs;
		public long SubclassConstructorExtraMs;
		public long OtherUnattributedMs;
		public int CitizenType;
		public int Topic;
		public int CitizenService;
		public bool CommonTalkUsed;
		public string PhraseMaxHelper;
		public string PhraseTopTotalHelper;
		public long PhraseTopHelperTotalMs;
		public long PhraseTopHelperMaxMs;
		public string PhraseTopCountHelper;
		public long PhraseTopCount;
		public long PhraseNestedTotalMs;
		public long PhraseRumorUnattributedMs;
		public long CommonTalkCalls;
		public long CommonTalkTotalMs;
		public long CommonTalkMaxMs;
		public long QuestItemsCalls;
		public long QuestItemsTotalMs;
		public long QuestItemsMaxMs;
		public long GetRareLocationCalls;
		public long GetRareLocationTotalMs;
		public long GetRareLocationMaxMs;
		public long RandomWordsCalls;
		public long RandomWordsTotalMs;
		public long RandomWordsMaxMs;

		public long GetPhaseMilliseconds(CitizenConstructorPhase phase)
		{
			switch (phase)
			{
				case CitizenConstructorPhase.BaseConstructor: return BaseConstructorMs;
				case CitizenConstructorPhase.NameBodySetup: return NameBodySetupMs;
				case CitizenConstructorPhase.DressUp: return DressUpMs;
				case CitizenConstructorPhase.SetupCitizenTotal: return SetupCitizenTotalMs;
				case CitizenConstructorPhase.BackpackReplace: return BackpackReplaceMs;
				case CitizenConstructorPhase.OneHandLayerLookup: return OneHandLayerLookupMs;
				case CitizenConstructorPhase.TwoHandLayerLookup: return TwoHandLayerLookupMs;
				case CitizenConstructorPhase.MaterialColorOrMorph: return MaterialColorOrMorphMs;
				case CitizenConstructorPhase.RandomDungeonCitySetup: return RandomDungeonCitySetupMs;
				case CitizenConstructorPhase.RandomQuestItemSetup: return RandomQuestItemSetupMs;
				case CitizenConstructorPhase.PhraseRumorGeneration: return PhraseRumorGenerationMs;
				case CitizenConstructorPhase.TitleSkinHairHue: return TitleSkinHairHueMs;
				case CitizenConstructorPhase.StatResistanceSkillSetup: return StatResistanceSkillSetupMs;
				case CitizenConstructorPhase.FinalFlags: return FinalFlagsMs;
				case CitizenConstructorPhase.SubclassConstructorExtra: return SubclassConstructorExtraMs;
				case CitizenConstructorPhase.OtherUnattributed: return OtherUnattributedMs;
				default: return 0;
			}
		}
	}

	public static class CitizenConstructorDiagnostics
	{
		private const int MaxTrackedTypes = 64;
		private const int TopCount = 10;
		private const long WindowMilliseconds = 60000;
		private const int MaxHelperDepth = 16;
		private const long SlowHelperMilliseconds = 25;

		private static readonly object m_Lock = new object();
		private static Dictionary<string, TypeStat> m_Current = new Dictionary<string, TypeStat>(StringComparer.Ordinal);
		private static Dictionary<string, TypeStat> m_Last = new Dictionary<string, TypeStat>(StringComparer.Ordinal);
		private static Dictionary<string, TypeStat> m_AllTime = new Dictionary<string, TypeStat>(StringComparer.Ordinal);
		private static long m_WindowStarted = Core.TickCount;
		private static CitizenConstructorSample m_WorstSample;
		private static bool m_HasWorstSample;
		private static CitizenConstructorSample m_WorstPhraseSample;
		private static long m_WorstPhraseHelperTicks;
		private static bool m_HasWorstPhraseSample;
		private static readonly long[] m_HelperTotalTicks = new long[HelperCount];
		private static readonly long[] m_HelperMaxTicks = new long[HelperCount];
		private static readonly long[] m_HelperExclusiveMaxTicks = new long[HelperCount];
		private static readonly long[] m_HelperLastTicks = new long[HelperCount];
		private static readonly long[] m_HelperCalls = new long[HelperCount];
		private static readonly long[] m_HelperSlowCalls = new long[HelperCount];
		private static readonly long[] m_HelperErrors = new long[HelperCount];
		private static readonly string[] m_HelperMaxTypes = new string[HelperCount];
		private static readonly string[] m_HelperMaxSources = new string[HelperCount];
		private static readonly int[] m_HelperMaxTopics = new int[HelperCount];
		private static readonly int[] m_HelperMaxServices = new int[HelperCount];
		private static readonly bool[] m_HelperMaxCommonTalk = new bool[HelperCount];

		[ThreadStatic]
		private static ActiveTrace m_Active;

		[ThreadStatic]
		private static ActiveTrace m_Cached;

		private sealed class ActiveTrace
		{
			public string TypeName;
			public string Source;
			public string DressUpKind;
			public long Started;
			public long CitizenBodyCompleted;
			public long[] PhaseTicks = new long[PhaseCount];
			public long[] HelperTicks = new long[HelperCount];
			public long[] HelperExclusiveTicks = new long[HelperCount];
			public long[] HelperMaxTicks = new long[HelperCount];
			public long[] HelperExclusiveMaxTicks = new long[HelperCount];
			public long[] HelperLastTicks = new long[HelperCount];
			public long[] HelperCalls = new long[HelperCount];
			public long[] HelperSlowCalls = new long[HelperCount];
			public long[] HelperErrors = new long[HelperCount];
			public CitizenPhraseHelper[] HelperStack = new CitizenPhraseHelper[MaxHelperDepth];
			public long[] HelperStarted = new long[MaxHelperDepth];
			public long[] HelperChildTicks = new long[MaxHelperDepth];
			public bool[] HelperInsidePhrase = new bool[MaxHelperDepth];
			public int HelperDepth;
			public bool PhraseScopeActive;
			public long PhraseRootHelperTicks;
			public int CitizenType;
			public int Topic;
			public int CitizenService;
			public bool CommonTalkUsed;

			public void Reset(string typeName, string source)
			{
				TypeName = typeName;
				Source = source;
				DressUpKind = null;
				Started = Stopwatch.GetTimestamp();
				CitizenBodyCompleted = 0;
				Array.Clear(PhaseTicks, 0, PhaseTicks.Length);
				Array.Clear(HelperTicks, 0, HelperTicks.Length);
				Array.Clear(HelperExclusiveTicks, 0, HelperExclusiveTicks.Length);
				Array.Clear(HelperMaxTicks, 0, HelperMaxTicks.Length);
				Array.Clear(HelperExclusiveMaxTicks, 0, HelperExclusiveMaxTicks.Length);
				Array.Clear(HelperLastTicks, 0, HelperLastTicks.Length);
				Array.Clear(HelperCalls, 0, HelperCalls.Length);
				Array.Clear(HelperSlowCalls, 0, HelperSlowCalls.Length);
				Array.Clear(HelperErrors, 0, HelperErrors.Length);
				Array.Clear(HelperStarted, 0, HelperStarted.Length);
				Array.Clear(HelperChildTicks, 0, HelperChildTicks.Length);
				Array.Clear(HelperInsidePhrase, 0, HelperInsidePhrase.Length);
				HelperDepth = 0;
				PhraseScopeActive = false;
				PhraseRootHelperTicks = 0;
				CitizenType = 0;
				Topic = -1;
				CitizenService = 0;
				CommonTalkUsed = false;
			}
		}

		private sealed class TypeStat
		{
			public string TypeName;
			public string LastSource;
			public string LastDressUpKind;
			public long Count;
			public long TotalMs;
			public long MaxMs;
			public long HeavyCount;
			public long ErrorCount;
			public long[] PhaseTotals = new long[PhaseCount];
			public long[] PhaseMax = new long[PhaseCount];
			public long[] HelperTotalTicks = new long[HelperCount];
			public long[] HelperMaxTicks = new long[HelperCount];
			public long[] HelperLastTicks = new long[HelperCount];
			public long[] HelperCalls = new long[HelperCount];
			public long[] HelperSlowCalls = new long[HelperCount];
			public long[] HelperErrors = new long[HelperCount];
			public string WorstSample;

			public void CopyFrom(TypeStat source)
			{
				if (source == null)
					return;

				TypeName = source.TypeName;
				LastSource = source.LastSource;
				LastDressUpKind = source.LastDressUpKind;
				Count = source.Count;
				TotalMs = source.TotalMs;
				MaxMs = source.MaxMs;
				HeavyCount = source.HeavyCount;
				ErrorCount = source.ErrorCount;
				WorstSample = source.WorstSample;
				Array.Copy(source.PhaseTotals, PhaseTotals, PhaseCount);
				Array.Copy(source.PhaseMax, PhaseMax, PhaseCount);
				Array.Copy(source.HelperTotalTicks, HelperTotalTicks, HelperCount);
				Array.Copy(source.HelperMaxTicks, HelperMaxTicks, HelperCount);
				Array.Copy(source.HelperLastTicks, HelperLastTicks, HelperCount);
				Array.Copy(source.HelperCalls, HelperCalls, HelperCount);
				Array.Copy(source.HelperSlowCalls, HelperSlowCalls, HelperCount);
				Array.Copy(source.HelperErrors, HelperErrors, HelperCount);
			}
		}

		public static int PhaseCount
		{
			get { return 16; }
		}

		public static int HelperCount
		{
			get { return 18; }
		}

		public static bool Begin(string typeName, string source)
		{
			if (m_Active != null)
				return false;

			ActiveTrace trace = m_Cached;

			if (trace == null)
				m_Cached = trace = new ActiveTrace();

			trace.Reset(
				String.IsNullOrEmpty(typeName) ? "unknown" : typeName,
				String.IsNullOrEmpty(source) ? "unknown" : source);
			m_Active = trace;
			return true;
		}

		public static bool IsActive
		{
			get { return m_Active != null; }
		}

		public static void MarkBaseConstructorComplete()
		{
			ActiveTrace trace = m_Active;

			if (trace != null)
				trace.PhaseTicks[(int)CitizenConstructorPhase.BaseConstructor] = Stopwatch.GetTimestamp() - trace.Started;
		}

		public static long BeginPhase()
		{
			return m_Active != null ? Stopwatch.GetTimestamp() : 0;
		}

		public static void EndPhase(CitizenConstructorPhase phase, long started)
		{
			ActiveTrace trace = m_Active;

			if (trace == null || started == 0)
				return;

			trace.PhaseTicks[(int)phase] += Stopwatch.GetTimestamp() - started;
		}

		public static long BeginHelper(CitizenPhraseHelper helper)
		{
			ActiveTrace trace = m_Active;

			if (trace == null || trace.HelperDepth >= MaxHelperDepth)
				return 0;

			long started = Stopwatch.GetTimestamp();
			int depth = trace.HelperDepth++;
			trace.HelperStack[depth] = helper;
			trace.HelperStarted[depth] = started;
			trace.HelperChildTicks[depth] = 0;
			trace.HelperInsidePhrase[depth] = trace.PhraseScopeActive;
			return started;
		}

		public static void EndHelper(CitizenPhraseHelper helper, long started)
		{
			ActiveTrace trace = m_Active;

			if (trace == null || started == 0 || trace.HelperDepth <= 0)
				return;

			int depth = trace.HelperDepth - 1;

			if (trace.HelperStack[depth] != helper || trace.HelperStarted[depth] != started)
			{
				trace.HelperDepth = 0;
				return;
			}

			long elapsed = Stopwatch.GetTimestamp() - started;
			long exclusive = elapsed - trace.HelperChildTicks[depth];
			int index = (int)helper;

			if (exclusive < 0)
				exclusive = 0;

			trace.HelperTicks[index] += elapsed;
			trace.HelperExclusiveTicks[index] += exclusive;
			trace.HelperLastTicks[index] = elapsed;
			++trace.HelperCalls[index];

			if (IsSlowHelper(elapsed))
				++trace.HelperSlowCalls[index];

			if (elapsed > trace.HelperMaxTicks[index])
				trace.HelperMaxTicks[index] = elapsed;

			if (exclusive > trace.HelperExclusiveMaxTicks[index])
				trace.HelperExclusiveMaxTicks[index] = exclusive;

			trace.HelperDepth = depth;

			if (depth > 0)
				trace.HelperChildTicks[depth - 1] += elapsed;
			else if (trace.HelperInsidePhrase[depth])
				trace.PhraseRootHelperTicks += elapsed;
		}

		public static void MarkHelperError(CitizenPhraseHelper helper)
		{
			ActiveTrace trace = m_Active;

			if (trace != null)
				++trace.HelperErrors[(int)helper];
		}

		public static void BeginPhraseRumorScope()
		{
			ActiveTrace trace = m_Active;

			if (trace != null)
			{
				trace.PhraseScopeActive = true;
				trace.PhraseRootHelperTicks = 0;
			}
		}

		public static void EndPhraseRumorScope()
		{
			ActiveTrace trace = m_Active;

			if (trace != null)
				trace.PhraseScopeActive = false;
		}

		public static void SetPhraseContext(int citizenType, int topic, int citizenService, bool commonTalkUsed)
		{
			ActiveTrace trace = m_Active;

			if (trace == null)
				return;

			trace.CitizenType = citizenType;
			trace.Topic = topic;
			trace.CitizenService = citizenService;
			trace.CommonTalkUsed = commonTalkUsed;
		}

		public static void MarkCommonTalkUsed()
		{
			ActiveTrace trace = m_Active;

			if (trace != null)
				trace.CommonTalkUsed = true;
		}

		public static void SetDressUpKind(string value)
		{
			ActiveTrace trace = m_Active;

			if (trace != null)
				trace.DressUpKind = value;
		}

		public static void MarkCitizenBodyComplete()
		{
			ActiveTrace trace = m_Active;

			if (trace != null)
				trace.CitizenBodyCompleted = Stopwatch.GetTimestamp();
		}

		public static CitizenConstructorSample End(long measuredTotalMs, string error)
		{
			ActiveTrace trace = m_Active;

			if (trace == null)
				return new CitizenConstructorSample();

			try
			{
				long ended = Stopwatch.GetTimestamp();
				long totalTicks = ended - trace.Started;

				if (trace.HelperDepth > 0)
					CloseOpenHelpersOnEnd(trace, ended);

				trace.PhraseScopeActive = false;

				if (trace.CitizenBodyCompleted > 0 && ended > trace.CitizenBodyCompleted)
					trace.PhaseTicks[(int)CitizenConstructorPhase.SubclassConstructorExtra] += ended - trace.CitizenBodyCompleted;

				long majorTicks =
					trace.PhaseTicks[(int)CitizenConstructorPhase.BaseConstructor]
					+ trace.PhaseTicks[(int)CitizenConstructorPhase.NameBodySetup]
					+ trace.PhaseTicks[(int)CitizenConstructorPhase.DressUp]
					+ trace.PhaseTicks[(int)CitizenConstructorPhase.SetupCitizenTotal]
					+ trace.PhaseTicks[(int)CitizenConstructorPhase.TitleSkinHairHue]
					+ trace.PhaseTicks[(int)CitizenConstructorPhase.StatResistanceSkillSetup]
					+ trace.PhaseTicks[(int)CitizenConstructorPhase.FinalFlags]
					+ trace.PhaseTicks[(int)CitizenConstructorPhase.SubclassConstructorExtra];

				if (totalTicks > majorTicks)
					trace.PhaseTicks[(int)CitizenConstructorPhase.OtherUnattributed] = totalTicks - majorTicks;

				CitizenConstructorSample sample = CreateSample(trace, measuredTotalMs, error);
				Record(sample, trace);
				return sample;
			}
			finally
			{
				m_Active = null;
			}
		}

		public static void Abandon()
		{
			m_Active = null;
		}

		public static string GetPhaseName(CitizenConstructorPhase phase)
		{
			switch (phase)
			{
				case CitizenConstructorPhase.BaseConstructor: return "baseConstructor";
				case CitizenConstructorPhase.NameBodySetup: return "nameBodySetup";
				case CitizenConstructorPhase.DressUp: return "dressUp";
				case CitizenConstructorPhase.SetupCitizenTotal: return "setupCitizenTotal";
				case CitizenConstructorPhase.BackpackReplace: return "backpackReplace";
				case CitizenConstructorPhase.OneHandLayerLookup: return "oneHandLayerLookup";
				case CitizenConstructorPhase.TwoHandLayerLookup: return "twoHandLayerLookup";
				case CitizenConstructorPhase.MaterialColorOrMorph: return "materialColorOrMorph";
				case CitizenConstructorPhase.RandomDungeonCitySetup: return "randomDungeonCitySetup";
				case CitizenConstructorPhase.RandomQuestItemSetup: return "randomQuestItemSetup";
				case CitizenConstructorPhase.PhraseRumorGeneration: return "phraseRumorGeneration";
				case CitizenConstructorPhase.TitleSkinHairHue: return "titleSkinHairHue";
				case CitizenConstructorPhase.StatResistanceSkillSetup: return "statResistanceSkillSetup";
				case CitizenConstructorPhase.FinalFlags: return "finalFlags";
				case CitizenConstructorPhase.SubclassConstructorExtra: return "subclassConstructorExtra";
				case CitizenConstructorPhase.OtherUnattributed: return "otherUnattributed";
				default: return "unknown";
			}
		}

		public static string GetHelperName(CitizenPhraseHelper helper)
		{
			switch (helper)
			{
				case CitizenPhraseHelper.SomePlace: return "somePlace";
				case CitizenPhraseHelper.MadeUpDungeon: return "madeUpDungeon";
				case CitizenPhraseHelper.GetRandomCity: return "getRandomCity";
				case CitizenPhraseHelper.MadeUpCity: return "madeUpCity";
				case CitizenPhraseHelper.Adventurer: return "adventurer";
				case CitizenPhraseHelper.GetSpecialItem: return "getSpecialItem";
				case CitizenPhraseHelper.QuestItems: return "questItems";
				case CitizenPhraseHelper.ContainerOwner: return "containerOwner";
				case CitizenPhraseHelper.RandomWords: return "randomWords";
				case CitizenPhraseHelper.GetRandomJob: return "getRandomJob";
				case CitizenPhraseHelper.GetRareLocation: return "getRareLocation";
				case CitizenPhraseHelper.CommonTalk: return "commonTalk";
				case CitizenPhraseHelper.TopicTemplateFormatting: return "topicTemplateFormatting";
				case CitizenPhraseHelper.PrefaceSelection: return "prefaceSelection";
				case CitizenPhraseHelper.ServiceSelection: return "serviceSelection";
				case CitizenPhraseHelper.PhraseGeneration: return "phraseGeneration";
				case CitizenPhraseHelper.BackpackFormatting: return "backpackFormatting";
				case CitizenPhraseHelper.OtherUnattributed: return "otherUnattributed";
				default: return "unknown";
			}
		}

		public static string FormatCompact(CitizenConstructorSample sample)
		{
			if (sample.TotalMs <= 0 && String.IsNullOrEmpty(sample.TypeName))
				return "Not sampled";

			CitizenConstructorPhase topMajor;
			long topMajorMs;
			GetTopMajorPhase(sample, out topMajor, out topMajorMs);

			CitizenConstructorPhase topSetup;
			long topSetupMs;
			GetTopSetupPhase(sample, out topSetup, out topSetupMs);

			return String.Format(
				"citizenCtorType={0},citizenCtorSource={1},citizenCtorTotal={2}ms,citizenCtorTopSubphase={3}:{4}ms,citizenCtorWorstSubphase={5}:{6}ms,citizenCtorDressUp={7}:{8}ms,citizenCtorSetupCitizen={9}ms,citizenCtorRumor={10}ms,citizenCtorBackpack={11}ms,citizenCtorMaterialMorph={12}ms,citizenCtorOther={13}ms,citizenPhraseMaxHelper={14}:{15}ms,citizenPhraseTopTotal={16}:{17}ms,citizenPhraseTopCount={18}:{19},citizenPhraseNestedTotal={20}ms,citizenPhraseRumorUnattributed={21}ms,citizenPhraseContext=type:{22}/topic:{23}/service:{24}/commonTalk:{25},commonTalk={26}/{27}ms/{28}ms,questItems={29}/{30}ms/{31}ms,getRareLocation={32}/{33}ms/{34}ms,randomWords={35}/{36}ms/{37}ms,citizenCtorError={38}",
				sample.TypeName,
				sample.Source,
				sample.TotalMs,
				GetPhaseName(topMajor),
				topMajorMs,
				GetPhaseName(topSetup),
				topSetupMs,
				String.IsNullOrEmpty(sample.DressUpKind) ? "unknown" : sample.DressUpKind,
				sample.DressUpMs,
				sample.SetupCitizenTotalMs,
				sample.PhraseRumorGenerationMs,
				sample.BackpackReplaceMs,
				sample.MaterialColorOrMorphMs,
				sample.OtherUnattributedMs,
				String.IsNullOrEmpty(sample.PhraseMaxHelper) ? "none" : sample.PhraseMaxHelper,
				sample.PhraseTopHelperMaxMs,
				String.IsNullOrEmpty(sample.PhraseTopTotalHelper) ? "none" : sample.PhraseTopTotalHelper,
				sample.PhraseTopHelperTotalMs,
				String.IsNullOrEmpty(sample.PhraseTopCountHelper) ? "none" : sample.PhraseTopCountHelper,
				sample.PhraseTopCount,
				sample.PhraseNestedTotalMs,
				sample.PhraseRumorUnattributedMs,
				sample.CitizenType,
				sample.Topic,
				sample.CitizenService,
				sample.CommonTalkUsed,
				sample.CommonTalkCalls,
				sample.CommonTalkTotalMs,
				sample.CommonTalkMaxMs,
				sample.QuestItemsCalls,
				sample.QuestItemsTotalMs,
				sample.QuestItemsMaxMs,
				sample.GetRareLocationCalls,
				sample.GetRareLocationTotalMs,
				sample.GetRareLocationMaxMs,
				sample.RandomWordsCalls,
				sample.RandomWordsTotalMs,
				sample.RandomWordsMaxMs,
				String.IsNullOrEmpty(sample.Error) ? "none" : sample.Error);
		}

		public static void GetTopMajorPhase(CitizenConstructorSample sample, out CitizenConstructorPhase phase, out long milliseconds)
		{
			CitizenConstructorPhase[] phases = new CitizenConstructorPhase[]
			{
				CitizenConstructorPhase.BaseConstructor,
				CitizenConstructorPhase.NameBodySetup,
				CitizenConstructorPhase.DressUp,
				CitizenConstructorPhase.SetupCitizenTotal,
				CitizenConstructorPhase.TitleSkinHairHue,
				CitizenConstructorPhase.StatResistanceSkillSetup,
				CitizenConstructorPhase.FinalFlags,
				CitizenConstructorPhase.SubclassConstructorExtra,
				CitizenConstructorPhase.OtherUnattributed
			};

			GetTopPhase(sample, phases, out phase, out milliseconds);
		}

		public static void GetTopSetupPhase(CitizenConstructorSample sample, out CitizenConstructorPhase phase, out long milliseconds)
		{
			CitizenConstructorPhase[] phases = new CitizenConstructorPhase[]
			{
				CitizenConstructorPhase.BackpackReplace,
				CitizenConstructorPhase.OneHandLayerLookup,
				CitizenConstructorPhase.TwoHandLayerLookup,
				CitizenConstructorPhase.MaterialColorOrMorph,
				CitizenConstructorPhase.RandomDungeonCitySetup,
				CitizenConstructorPhase.RandomQuestItemSetup,
				CitizenConstructorPhase.PhraseRumorGeneration
			};

			GetTopPhase(sample, phases, out phase, out milliseconds);
		}

		public static void RollWindow()
		{
			lock (m_Lock)
			{
				CopyStats(m_Current, m_Last);
				m_Current.Clear();
				m_WindowStarted = Core.TickCount;
			}
		}

		public static void WriteStats(TextWriter writer)
		{
			if (writer == null)
				return;

			Dictionary<string, TypeStat> current = new Dictionary<string, TypeStat>(StringComparer.Ordinal);
			Dictionary<string, TypeStat> last = new Dictionary<string, TypeStat>(StringComparer.Ordinal);
			Dictionary<string, TypeStat> allTime = new Dictionary<string, TypeStat>(StringComparer.Ordinal);
			CitizenConstructorSample worst;
			bool hasWorst;
			CitizenConstructorSample worstPhrase;
			bool hasWorstPhrase;
			long[] helperTotals = new long[HelperCount];
			long[] helperMaxima = new long[HelperCount];
			long[] helperLast = new long[HelperCount];
			long[] helperCalls = new long[HelperCount];
			long[] helperSlow = new long[HelperCount];
			long[] helperErrors = new long[HelperCount];
			string[] helperTypes = new string[HelperCount];
			string[] helperSources = new string[HelperCount];
			int[] helperTopics = new int[HelperCount];
			int[] helperServices = new int[HelperCount];
			bool[] helperCommonTalk = new bool[HelperCount];

			lock (m_Lock)
			{
				RollWindowIfNeededNoLock();
				CopyStats(m_Current, current);
				CopyStats(m_Last, last);
				CopyStats(m_AllTime, allTime);
				worst = m_WorstSample;
				hasWorst = m_HasWorstSample;
				worstPhrase = m_WorstPhraseSample;
				hasWorstPhrase = m_HasWorstPhraseSample;
				Array.Copy(m_HelperTotalTicks, helperTotals, HelperCount);
				Array.Copy(m_HelperMaxTicks, helperMaxima, HelperCount);
				Array.Copy(m_HelperLastTicks, helperLast, HelperCount);
				Array.Copy(m_HelperCalls, helperCalls, HelperCount);
				Array.Copy(m_HelperSlowCalls, helperSlow, HelperCount);
				Array.Copy(m_HelperErrors, helperErrors, HelperCount);
				Array.Copy(m_HelperMaxTypes, helperTypes, HelperCount);
				Array.Copy(m_HelperMaxSources, helperSources, HelperCount);
				Array.Copy(m_HelperMaxTopics, helperTopics, HelperCount);
				Array.Copy(m_HelperMaxServices, helperServices, HelperCount);
				Array.Copy(m_HelperMaxCommonTalk, helperCommonTalk, HelperCount);
			}

			writer.WriteLine("CitizenConstructorStats");
			WriteWindow(writer, "current", current);
			WriteWindow(writer, "last", last);
			WriteWindow(writer, "allTime", allTime);
			writer.WriteLine("CitizenConstructor exactWorst: {0}", hasWorst ? FormatCompact(worst) : "Not sampled");
			writer.WriteLine("CitizenConstructor exactWorstPhraseHelper: {0}", hasWorstPhrase ? FormatCompact(worstPhrase) : "Not sampled");
			WritePhraseHelperStats(writer, helperTotals, helperMaxima, helperLast, helperCalls, helperSlow, helperErrors, helperTypes, helperSources, helperTopics, helperServices, helperCommonTalk);
			writer.WriteLine("CitizenConstructor note: baseConstructor includes reflection invocation overhead before the Citizens constructor body begins.");
		}

		public static void GetPerformanceSummary(out string topType, out long topTypeMaxMs, out string topPhase, out long topPhaseMs, out string topSetupPhase, out long topSetupPhaseMs)
		{
			lock (m_Lock)
			{
				TypeStat top = GetTopType(m_AllTime, true);

				if (top == null)
				{
					topType = "Not sampled";
					topTypeMaxMs = 0;
					topPhase = "Not sampled";
					topPhaseMs = 0;
					topSetupPhase = "Not sampled";
					topSetupPhaseMs = 0;
					return;
				}

				topType = top.TypeName;
				topTypeMaxMs = top.MaxMs;

				int phaseIndex = GetTopPhaseIndex(top.PhaseTotals, false);
				topPhase = phaseIndex >= 0 ? GetPhaseName((CitizenConstructorPhase)phaseIndex) : "Not sampled";
				topPhaseMs = phaseIndex >= 0 ? top.PhaseTotals[phaseIndex] : 0;

				int setupIndex = GetTopPhaseIndex(top.PhaseTotals, true);
				topSetupPhase = setupIndex >= 0 ? GetPhaseName((CitizenConstructorPhase)setupIndex) : "Not sampled";
				topSetupPhaseMs = setupIndex >= 0 ? top.PhaseTotals[setupIndex] : 0;
			}
		}

		public static void GetPhrasePerformanceSummary(
			out string maxHelper,
			out long maxHelperMs,
			out string totalHelper,
			out long totalHelperMs,
			out string countHelper,
			out long helperCalls,
			out string worstSample)
		{
			lock (m_Lock)
			{
				int maxIndex = GetTopHelperIndex(m_HelperExclusiveMaxTicks);
				int totalIndex = GetTopHelperIndex(m_HelperTotalTicks);
				int countIndex = GetTopHelperIndex(m_HelperCalls);

				maxHelper = maxIndex >= 0 ? GetHelperName((CitizenPhraseHelper)maxIndex) : "Not sampled";
				maxHelperMs = maxIndex >= 0 ? ToMilliseconds(m_HelperExclusiveMaxTicks[maxIndex]) : 0;
				totalHelper = totalIndex >= 0 ? GetHelperName((CitizenPhraseHelper)totalIndex) : "Not sampled";
				totalHelperMs = totalIndex >= 0 ? ToMilliseconds(m_HelperTotalTicks[totalIndex]) : 0;
				countHelper = countIndex >= 0 ? GetHelperName((CitizenPhraseHelper)countIndex) : "Not sampled";
				helperCalls = countIndex >= 0 ? m_HelperCalls[countIndex] : 0;
				worstSample = m_HasWorstPhraseSample ? FormatCompact(m_WorstPhraseSample) : "Not sampled";
			}
		}

		private static CitizenConstructorSample CreateSample(ActiveTrace trace, long measuredTotalMs, string error)
		{
			CitizenConstructorSample sample = new CitizenConstructorSample();
			sample.TypeName = trace.TypeName;
			sample.Source = trace.Source;
			sample.DressUpKind = trace.DressUpKind;
			sample.Error = String.IsNullOrEmpty(error) ? "none" : error;
			sample.TotalMs = measuredTotalMs;
			sample.BaseConstructorMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.BaseConstructor]);
			sample.NameBodySetupMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.NameBodySetup]);
			sample.DressUpMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.DressUp]);
			sample.SetupCitizenTotalMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.SetupCitizenTotal]);
			sample.BackpackReplaceMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.BackpackReplace]);
			sample.OneHandLayerLookupMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.OneHandLayerLookup]);
			sample.TwoHandLayerLookupMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.TwoHandLayerLookup]);
			sample.MaterialColorOrMorphMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.MaterialColorOrMorph]);
			sample.RandomDungeonCitySetupMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.RandomDungeonCitySetup]);
			sample.RandomQuestItemSetupMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.RandomQuestItemSetup]);
			sample.PhraseRumorGenerationMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.PhraseRumorGeneration]);
			sample.TitleSkinHairHueMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.TitleSkinHairHue]);
			sample.StatResistanceSkillSetupMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.StatResistanceSkillSetup]);
			sample.FinalFlagsMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.FinalFlags]);
			sample.SubclassConstructorExtraMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.SubclassConstructorExtra]);
			sample.OtherUnattributedMs = ToMilliseconds(trace.PhaseTicks[(int)CitizenConstructorPhase.OtherUnattributed]);
			sample.CitizenType = trace.CitizenType;
			sample.Topic = trace.Topic;
			sample.CitizenService = trace.CitizenService;
			sample.CommonTalkUsed = trace.CommonTalkUsed;

			int maxHelper = GetTopHelperIndex(trace.HelperExclusiveMaxTicks);
			int totalHelper = GetTopHelperIndex(trace.HelperExclusiveTicks);
			int countHelper = GetTopHelperIndex(trace.HelperCalls);
			sample.PhraseMaxHelper = maxHelper >= 0 ? GetHelperName((CitizenPhraseHelper)maxHelper) : "none";
			sample.PhraseTopHelperMaxMs = maxHelper >= 0 ? ToMilliseconds(trace.HelperExclusiveMaxTicks[maxHelper]) : 0;
			sample.PhraseTopTotalHelper = totalHelper >= 0 ? GetHelperName((CitizenPhraseHelper)totalHelper) : "none";
			sample.PhraseTopHelperTotalMs = totalHelper >= 0 ? ToMilliseconds(trace.HelperExclusiveTicks[totalHelper]) : 0;
			sample.PhraseTopCountHelper = countHelper >= 0 ? GetHelperName((CitizenPhraseHelper)countHelper) : "none";
			sample.PhraseTopCount = countHelper >= 0 ? trace.HelperCalls[countHelper] : 0;
			sample.PhraseNestedTotalMs = ToMilliseconds(trace.PhraseRootHelperTicks);

			long rumorTicks = trace.PhaseTicks[(int)CitizenConstructorPhase.PhraseRumorGeneration];
			sample.PhraseRumorUnattributedMs = ToMilliseconds(rumorTicks > trace.PhraseRootHelperTicks ? rumorTicks - trace.PhraseRootHelperTicks : 0);
			SetKeyHelperSample(trace, CitizenPhraseHelper.CommonTalk, out sample.CommonTalkCalls, out sample.CommonTalkTotalMs, out sample.CommonTalkMaxMs);
			SetKeyHelperSample(trace, CitizenPhraseHelper.QuestItems, out sample.QuestItemsCalls, out sample.QuestItemsTotalMs, out sample.QuestItemsMaxMs);
			SetKeyHelperSample(trace, CitizenPhraseHelper.GetRareLocation, out sample.GetRareLocationCalls, out sample.GetRareLocationTotalMs, out sample.GetRareLocationMaxMs);
			SetKeyHelperSample(trace, CitizenPhraseHelper.RandomWords, out sample.RandomWordsCalls, out sample.RandomWordsTotalMs, out sample.RandomWordsMaxMs);
			return sample;
		}

		private static void Record(CitizenConstructorSample sample, ActiveTrace trace)
		{
			lock (m_Lock)
			{
				RollWindowIfNeededNoLock();
				RecordNoLock(m_Current, sample, trace);
				RecordNoLock(m_AllTime, sample, trace);
				RecordPhraseHelpersNoLock(sample, trace);

				if (!m_HasWorstSample || sample.TotalMs > m_WorstSample.TotalMs)
				{
					m_WorstSample = sample;
					m_HasWorstSample = true;
				}
			}
		}

		private static void RecordPhraseHelpersNoLock(CitizenConstructorSample sample, ActiveTrace trace)
		{
			for (int i = 0; i < HelperCount; ++i)
			{
				if (trace.HelperCalls[i] <= 0)
					continue;

				m_HelperTotalTicks[i] += trace.HelperExclusiveTicks[i];
				m_HelperLastTicks[i] = trace.HelperLastTicks[i];
				m_HelperCalls[i] += trace.HelperCalls[i];
				m_HelperSlowCalls[i] += trace.HelperSlowCalls[i];
				m_HelperErrors[i] += trace.HelperErrors[i];

				if (trace.HelperMaxTicks[i] > m_HelperMaxTicks[i])
				{
					m_HelperMaxTicks[i] = trace.HelperMaxTicks[i];
					m_HelperMaxTypes[i] = sample.TypeName;
					m_HelperMaxSources[i] = sample.Source;
					m_HelperMaxTopics[i] = sample.Topic;
					m_HelperMaxServices[i] = sample.CitizenService;
					m_HelperMaxCommonTalk[i] = sample.CommonTalkUsed;
				}

				if (trace.HelperExclusiveMaxTicks[i] > m_HelperExclusiveMaxTicks[i])
					m_HelperExclusiveMaxTicks[i] = trace.HelperExclusiveMaxTicks[i];
			}

			int phraseMaxIndex = GetTopHelperIndex(trace.HelperExclusiveMaxTicks);

			if (phraseMaxIndex >= 0 && (!m_HasWorstPhraseSample || trace.HelperExclusiveMaxTicks[phraseMaxIndex] > m_WorstPhraseHelperTicks))
			{
				m_WorstPhraseSample = sample;
				m_WorstPhraseHelperTicks = trace.HelperExclusiveMaxTicks[phraseMaxIndex];
				m_HasWorstPhraseSample = true;
			}
		}

		private static void RecordNoLock(Dictionary<string, TypeStat> stats, CitizenConstructorSample sample, ActiveTrace trace)
		{
			TypeStat stat = GetTypeStatNoLock(stats, sample.TypeName);

			if (stat == null)
				return;

			++stat.Count;
			stat.TotalMs += sample.TotalMs;
			stat.LastSource = sample.Source;
			stat.LastDressUpKind = sample.DressUpKind;

			if (sample.TotalMs >= 75)
				++stat.HeavyCount;

			if (!String.IsNullOrEmpty(sample.Error) && sample.Error != "none")
				++stat.ErrorCount;

			for (int i = 0; i < PhaseCount; ++i)
			{
				long phaseMs = sample.GetPhaseMilliseconds((CitizenConstructorPhase)i);
				stat.PhaseTotals[i] += phaseMs;

				if (phaseMs > stat.PhaseMax[i])
					stat.PhaseMax[i] = phaseMs;
			}

			for (int i = 0; i < HelperCount; ++i)
			{
				stat.HelperTotalTicks[i] += trace.HelperExclusiveTicks[i];
				stat.HelperLastTicks[i] = trace.HelperLastTicks[i];
				stat.HelperCalls[i] += trace.HelperCalls[i];
				stat.HelperSlowCalls[i] += trace.HelperSlowCalls[i];
				stat.HelperErrors[i] += trace.HelperErrors[i];

				if (trace.HelperMaxTicks[i] > stat.HelperMaxTicks[i])
					stat.HelperMaxTicks[i] = trace.HelperMaxTicks[i];
			}

			if (sample.TotalMs > stat.MaxMs)
			{
				stat.MaxMs = sample.TotalMs;
				stat.WorstSample = FormatCompact(sample);
			}
		}

		private static TypeStat GetTypeStatNoLock(Dictionary<string, TypeStat> stats, string typeName)
		{
			TypeStat stat;
			string key = String.IsNullOrEmpty(typeName) ? "unknown" : typeName;

			if (stats.TryGetValue(key, out stat))
				return stat;

			if (stats.Count >= MaxTrackedTypes)
				key = "otherCitizenTypes";

			if (!stats.TryGetValue(key, out stat))
			{
				stat = new TypeStat();
				stat.TypeName = key;
				stats[key] = stat;
			}

			return stat;
		}

		private static void RollWindowIfNeededNoLock()
		{
			long now = Core.TickCount;

			if (now - m_WindowStarted < WindowMilliseconds)
				return;

			CopyStats(m_Current, m_Last);
			m_Current.Clear();
			m_WindowStarted = now;
		}

		private static void CopyStats(Dictionary<string, TypeStat> source, Dictionary<string, TypeStat> destination)
		{
			destination.Clear();

			foreach (TypeStat stat in source.Values)
			{
				TypeStat copy = new TypeStat();
				copy.CopyFrom(stat);
				destination[copy.TypeName] = copy;
			}
		}

		private static void WriteWindow(TextWriter writer, string label, Dictionary<string, TypeStat> stats)
		{
			writer.WriteLine("CitizenConstructor {0}: types={1}", label, stats.Count);

			if (stats.Count == 0)
			{
				writer.WriteLine("CitizenConstructor {0}: Not sampled", label);
				return;
			}

			WriteTop(writer, label, "topMax", GetSorted(stats, true));
			WriteTop(writer, label, "topTotal", GetSorted(stats, false));
		}

		private static void WritePhraseHelperStats(
			TextWriter writer,
			long[] totals,
			long[] maxima,
			long[] last,
			long[] calls,
			long[] slow,
			long[] errors,
			string[] types,
			string[] sources,
			int[] topics,
			int[] services,
			bool[] commonTalk)
		{
			bool sampled = false;

			for (int i = 0; i < HelperCount; ++i)
			{
				if (calls[i] <= 0)
					continue;

				sampled = true;
				long totalMs = ToMilliseconds(totals[i]);
				writer.WriteLine(
					"CitizenPhraseHelper allTime: helper={0} calls={1} total={2}ms max={3}ms avg={4}ms slow={5} last={6}ms errors={7} worstType={8} worstTopic={9} worstService={10} worstCommonTalk={11} worstSource={12}",
					GetHelperName((CitizenPhraseHelper)i),
					calls[i],
					totalMs,
					ToMilliseconds(maxima[i]),
					calls[i] > 0 ? totalMs / calls[i] : 0,
					slow[i],
					ToMilliseconds(last[i]),
					errors[i],
					String.IsNullOrEmpty(types[i]) ? "unknown" : types[i],
					topics[i],
					services[i],
					commonTalk[i],
					String.IsNullOrEmpty(sources[i]) ? "unknown" : sources[i]);
			}

			if (!sampled)
				writer.WriteLine("CitizenPhraseHelper allTime: Not sampled");
		}

		private static void WriteTop(TextWriter writer, string label, string mode, List<TypeStat> stats)
		{
			int count = Math.Min(TopCount, stats.Count);

			for (int i = 0; i < count; ++i)
			{
				TypeStat stat = stats[i];
				int topPhase = GetTopPhaseIndex(stat.PhaseTotals, false);
				int topPhaseMax = GetTopPhaseIndex(stat.PhaseMax, false);
				int topSetup = GetTopPhaseIndex(stat.PhaseTotals, true);
				int topSetupMax = GetTopPhaseIndex(stat.PhaseMax, true);

				writer.WriteLine(
					"CitizenConstructor {0} {1} #{2}: type={3} count={4} total={5}ms max={6}ms avg={7}ms heavy={8} errors={9} source={10} dressUp={11} topPhaseTotal={12}:{13}ms topPhaseMax={14}:{15}ms topSetupTotal={16}:{17}ms topSetupMax={18}:{19}ms worst={20}",
					label,
					mode,
					i + 1,
					stat.TypeName,
					stat.Count,
					stat.TotalMs,
					stat.MaxMs,
					stat.Count > 0 ? stat.TotalMs / stat.Count : 0,
					stat.HeavyCount,
					stat.ErrorCount,
					String.IsNullOrEmpty(stat.LastSource) ? "unknown" : stat.LastSource,
					String.IsNullOrEmpty(stat.LastDressUpKind) ? "unknown" : stat.LastDressUpKind,
					topPhase >= 0 ? GetPhaseName((CitizenConstructorPhase)topPhase) : "none",
					topPhase >= 0 ? stat.PhaseTotals[topPhase] : 0,
					topPhaseMax >= 0 ? GetPhaseName((CitizenConstructorPhase)topPhaseMax) : "none",
					topPhaseMax >= 0 ? stat.PhaseMax[topPhaseMax] : 0,
					topSetup >= 0 ? GetPhaseName((CitizenConstructorPhase)topSetup) : "none",
					topSetup >= 0 ? stat.PhaseTotals[topSetup] : 0,
					topSetupMax >= 0 ? GetPhaseName((CitizenConstructorPhase)topSetupMax) : "none",
					topSetupMax >= 0 ? stat.PhaseMax[topSetupMax] : 0,
					String.IsNullOrEmpty(stat.WorstSample) ? "none" : stat.WorstSample);

				int topHelperTotal = GetTopHelperIndex(stat.HelperTotalTicks);
				int topHelperMax = GetTopHelperIndex(stat.HelperMaxTicks);
				int topHelperCount = GetTopHelperIndex(stat.HelperCalls);
				writer.WriteLine(
					"CitizenConstructor {0} {1} #{2} phraseHelpers: max={3}:{4}ms topTotal={5}:{6}ms topCount={7}:{8} avg={9}ms slow={10} last={11}ms errors={12} commonTalk={13}/{14}ms/{15}ms questItems={16}/{17}ms/{18}ms getRareLocation={19}/{20}ms/{21}ms randomWords={22}/{23}ms/{24}ms",
					label,
					mode,
					i + 1,
					topHelperMax >= 0 ? GetHelperName((CitizenPhraseHelper)topHelperMax) : "none",
					topHelperMax >= 0 ? ToMilliseconds(stat.HelperMaxTicks[topHelperMax]) : 0,
					topHelperTotal >= 0 ? GetHelperName((CitizenPhraseHelper)topHelperTotal) : "none",
					topHelperTotal >= 0 ? ToMilliseconds(stat.HelperTotalTicks[topHelperTotal]) : 0,
					topHelperCount >= 0 ? GetHelperName((CitizenPhraseHelper)topHelperCount) : "none",
					topHelperCount >= 0 ? stat.HelperCalls[topHelperCount] : 0,
					topHelperTotal >= 0 && stat.HelperCalls[topHelperTotal] > 0 ? ToMilliseconds(stat.HelperTotalTicks[topHelperTotal]) / stat.HelperCalls[topHelperTotal] : 0,
					topHelperTotal >= 0 ? stat.HelperSlowCalls[topHelperTotal] : 0,
					topHelperTotal >= 0 ? ToMilliseconds(stat.HelperLastTicks[topHelperTotal]) : 0,
					SumValues(stat.HelperErrors),
					stat.HelperCalls[(int)CitizenPhraseHelper.CommonTalk],
					ToMilliseconds(stat.HelperTotalTicks[(int)CitizenPhraseHelper.CommonTalk]),
					ToMilliseconds(stat.HelperMaxTicks[(int)CitizenPhraseHelper.CommonTalk]),
					stat.HelperCalls[(int)CitizenPhraseHelper.QuestItems],
					ToMilliseconds(stat.HelperTotalTicks[(int)CitizenPhraseHelper.QuestItems]),
					ToMilliseconds(stat.HelperMaxTicks[(int)CitizenPhraseHelper.QuestItems]),
					stat.HelperCalls[(int)CitizenPhraseHelper.GetRareLocation],
					ToMilliseconds(stat.HelperTotalTicks[(int)CitizenPhraseHelper.GetRareLocation]),
					ToMilliseconds(stat.HelperMaxTicks[(int)CitizenPhraseHelper.GetRareLocation]),
					stat.HelperCalls[(int)CitizenPhraseHelper.RandomWords],
					ToMilliseconds(stat.HelperTotalTicks[(int)CitizenPhraseHelper.RandomWords]),
					ToMilliseconds(stat.HelperMaxTicks[(int)CitizenPhraseHelper.RandomWords]));
			}
		}

		private static List<TypeStat> GetSorted(Dictionary<string, TypeStat> stats, bool byMax)
		{
			List<TypeStat> list = new List<TypeStat>(stats.Values);
			list.Sort(delegate(TypeStat left, TypeStat right)
			{
				long leftValue = byMax ? left.MaxMs : left.TotalMs;
				long rightValue = byMax ? right.MaxMs : right.TotalMs;
				int compare = rightValue.CompareTo(leftValue);
				return compare != 0 ? compare : String.Compare(left.TypeName, right.TypeName, StringComparison.Ordinal);
			});
			return list;
		}

		private static TypeStat GetTopType(Dictionary<string, TypeStat> stats, bool byMax)
		{
			TypeStat best = null;

			foreach (TypeStat stat in stats.Values)
			{
				if (best == null || (byMax ? stat.MaxMs > best.MaxMs : stat.TotalMs > best.TotalMs))
					best = stat;
			}

			return best;
		}

		private static int GetTopPhaseIndex(long[] values, bool setupOnly)
		{
			int best = -1;
			long bestValue = -1;

			for (int i = 0; i < PhaseCount; ++i)
			{
				CitizenConstructorPhase phase = (CitizenConstructorPhase)i;
				bool isSetupDetail = phase >= CitizenConstructorPhase.BackpackReplace && phase <= CitizenConstructorPhase.PhraseRumorGeneration;

				if (setupOnly != isSetupDetail)
					continue;

				if (values[i] > bestValue)
				{
					best = i;
					bestValue = values[i];
				}
			}

			return bestValue > 0 ? best : -1;
		}

		private static void GetTopPhase(CitizenConstructorSample sample, CitizenConstructorPhase[] phases, out CitizenConstructorPhase phase, out long milliseconds)
		{
			phase = phases[0];
			milliseconds = sample.GetPhaseMilliseconds(phase);

			for (int i = 1; i < phases.Length; ++i)
			{
				long value = sample.GetPhaseMilliseconds(phases[i]);

				if (value > milliseconds)
				{
					phase = phases[i];
					milliseconds = value;
				}
			}
		}

		private static int GetTopHelperIndex(long[] values)
		{
			int best = -1;
			long bestValue = 0;

			for (int i = 0; i < HelperCount; ++i)
			{
				if (values[i] > bestValue)
				{
					best = i;
					bestValue = values[i];
				}
			}

			return best;
		}

		private static void SetKeyHelperSample(ActiveTrace trace, CitizenPhraseHelper helper, out long calls, out long totalMs, out long maxMs)
		{
			int index = (int)helper;
			calls = trace.HelperCalls[index];
			totalMs = ToMilliseconds(trace.HelperExclusiveTicks[index]);
			maxMs = ToMilliseconds(trace.HelperMaxTicks[index]);
		}

		private static void CloseOpenHelpersOnEnd(ActiveTrace trace, long ended)
		{
			while (trace.HelperDepth > 0)
			{
				int depth = trace.HelperDepth - 1;
				CitizenPhraseHelper helper = trace.HelperStack[depth];
				long elapsed = ended - trace.HelperStarted[depth];
				long exclusive = elapsed - trace.HelperChildTicks[depth];
				int index = (int)helper;

				if (exclusive < 0)
					exclusive = 0;

				trace.HelperTicks[index] += elapsed;
				trace.HelperExclusiveTicks[index] += exclusive;
				trace.HelperLastTicks[index] = elapsed;
				++trace.HelperCalls[index];
				++trace.HelperErrors[index];

				if (IsSlowHelper(elapsed))
					++trace.HelperSlowCalls[index];

				if (elapsed > trace.HelperMaxTicks[index])
					trace.HelperMaxTicks[index] = elapsed;

				if (exclusive > trace.HelperExclusiveMaxTicks[index])
					trace.HelperExclusiveMaxTicks[index] = exclusive;

				trace.HelperDepth = depth;

				if (depth > 0)
					trace.HelperChildTicks[depth - 1] += elapsed;
				else if (trace.HelperInsidePhrase[depth])
					trace.PhraseRootHelperTicks += elapsed;
			}
		}

		private static long SumValues(long[] values)
		{
			long total = 0;

			for (int i = 0; i < values.Length; ++i)
				total += values[i];

			return total;
		}

		private static long ToMilliseconds(long ticks)
		{
			if (ticks <= 0)
				return 0;

			return (ticks * 1000L) / Stopwatch.Frequency;
		}

		private static bool IsSlowHelper(long ticks)
		{
			return ticks > 0 && ticks * 1000L > SlowHelperMilliseconds * Stopwatch.Frequency;
		}
	}
}
