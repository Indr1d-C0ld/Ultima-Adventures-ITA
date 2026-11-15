using System;
using Server;
using System.Data;
using System.Threading;
using System.Collections;
using Server.Mobiles;
using Server.Misc;
using Server.Network;
using Server.Commands;
using Server.Targeting;

// Delegate to the method to be called Asynchronously
public delegate SpeechResponse GetResponseDelegate(string Said, Mobile Speaker, Townsperson tp);

namespace Server.Mobiles.Data
{
    public class SpeechData
    {
        public static System.Data.DataSet dsSpeechRules = new DataSet();
        public static bool blockRequests;

        public static void Initialize()
        {
            Console.Write("Initializing SpeechRules Dataset...");

            CommandSystem.Register("dbReload", AccessLevel.GameMaster, new CommandEventHandler(dbReload_OnCommand));
            SpeechData.LoadData();

            Console.WriteLine("done ({0})", SpeechData.dsSpeechRules.DataSetName);
        }

        public static void LoadData()
        {
            SpeechData.blockRequests = true;

            SpeechData.dsSpeechRules.Reset();
            SpeechData.dsSpeechRules.ReadXml("Data\\SpeechRules.xml", XmlReadMode.ReadSchema);

            SpeechData.blockRequests = false;
        }

        [Usage("dbReload")]
        [Description("Reloads the Townsperson Speech Data from XML.")]
        public static void dbReload_OnCommand(CommandEventArgs e)
        {
            SpeechData.blockRequests = true;

            // 2 second pause for pending requests to clear
            ReloadTimer t = new ReloadTimer(e.Mobile);
            t.Start();
        }

        private class ReloadTimer : Timer
        {
            Mobile m_from;

            public ReloadTimer(Mobile From)
                : base(TimeSpan.FromSeconds(2))
            {
                m_from = From;
                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                SpeechData.LoadData();
                m_from.SendMessage("Townsperson Database Reloaded");

                if (Townsperson.Logging == Townsperson.LogLevel.Basic || Townsperson.Logging == Townsperson.LogLevel.Debug)
                    TownspersonLogging.WriteLine(m_from, "Reloaded the Townsperson Speech Data from XML");
            }
        }
    }

    public class Response
    {
        // Asyncronous call will take at least this long; determines npc response delay.
        private static TimeSpan duration = TimeSpan.FromMilliseconds(1100);
        private const double FuzzyThreshold = 0.4;
        private class TopicHelpDefinition
        {
            public readonly string[] Keywords;
            public readonly string Response;
            public readonly bool SkipWhenSpeechContainsWhere;

            public TopicHelpDefinition(string[] keywords, string response, bool skipWhenSpeechContainsWhere)
            {
                Keywords = keywords;
                Response = response;
                SkipWhenSpeechContainsWhere = skipWhenSpeechContainsWhere;
            }

            public bool Matches(string speech)
            {
                if (string.IsNullOrEmpty(speech) || Keywords == null)
                    return false;

                for (int i = 0; i < Keywords.Length; i++)
                {
                    string keyword = Keywords[i];

                    if (string.IsNullOrEmpty(keyword))
                        continue;

                    if (speech.IndexOf(keyword) >= 0)
                        return true;
                }

                return false;
            }
        }

        private static readonly TopicHelpDefinition[] TopicHelps = new TopicHelpDefinition[]
        {
            new TopicHelpDefinition(
                new [] { "tribute" },
                "Tribute items are rare relics forged for heroes who earn the League of Jetsons' favor. Each one rests with a powerful guardian, and our guides keep track of where they lie. Ask me 'where can I find the Bone of the Frost Giant?' or 'where is the Cowl of Shadegloom?' when you want directions to a specific tribute.",
                true),
            new TopicHelpDefinition(
                new [] { "dungeon entrance", "dungeon" },
                "Dungeon entrances are the gateways into the realm's greatest challenges. When you need the doorway to a particular lair, ask me 'where is dungeon Exodus?' or 'where is dungeon Destard?' and I'll point the way.",
                true),
            new TopicHelpDefinition(
                new [] { "reagent", "reagents" },
                "Reagents are the raw components that power spells and rituals. Buy them from alchemists, herbalists, or mages in town, or gather them from the monsters you defeat. Ask me 'where can I get reagents?' or 'where can I buy reagents?' whenever you need a refresher.",
                false),
            new TopicHelpDefinition(
                new [] { "guild" },
                "Guilds are professional orders that train craftsmen and adventurers for a fee. Each craft keeps a guildmaster in a specific hall. Ask me 'what is a guild?' for the basics, or say 'where is the tailor guildmaster?' or 'where is the blacksmith guildmaster?' when you want directions.",
                true),
            new TopicHelpDefinition(
                new [] { "powerscroll", "power scroll" },
                "Powerscrolls are magical deeds that raise a skill's cap beyond Grandmaster. Hunt powerful bosses or check vendors to obtain them, then use each scroll in order (105, 110, 115, 120, 125) at the shrines in Ambrosia. Ask me 'what are powerscrolls?', 'where do I find powerscrolls?', or 'how do I use powerscrolls?' if you need the details repeated.",
                false),
            new TopicHelpDefinition(
                new [] { "lost art", "lost arts" },
                "Lost Arts are ancient bardic songs encoded on rare sheet music hidden in perilous dungeons. Learning one unlocks a new performance in your song book. When you chase a specific tune, ask me 'where can I find the Lost Art of Army's Paeon?' or 'where can I find the Lost Art of Foe Requiem?' and I'll guide you toward it.",
                true)
        };

        // Used if database is blocked during reload from xml.
        private static string[] blockedReplies = { "*cough*", "hmm", "*yawn*", "Sorry, what was that?", "Burp", "huh?" };
        private static readonly char[] WordSeparators = new char[] { ' ', '\t', '\r', '\n', ',', '.', '!', '?', ';', ':', '-', '\'', '"' };

        private DateTime started;
        private SpeechResponse sr;

        public SpeechResponse GetResponse(string Said, Mobile Speaker, Townsperson tp)
        {
            started = DateTime.UtcNow;
            DataView dvReplies;
            DataRowView[] drvReplies;
            object key;
            ArrayList alReplies = new ArrayList();
            DataRow drFound = null;
            String test = null;
            bool isWord = false;

            if (Townsperson.Logging == Townsperson.LogLevel.Debug)
                TownspersonLogging.WriteLine(Speaker, "Asynchronous call begun.");

            if (SpeechData.blockRequests == true)
            {
                sr = new SpeechResponse(blockedReplies[Utility.Random(blockedReplies.Length)], Speaker, 0, 0, null, null);

                if (Townsperson.Logging == Townsperson.LogLevel.Debug)
                    TownspersonLogging.WriteLine(Speaker, "Access to Database blocked.");
            }
            else
            {
                string normalizedSpeech = Said.ToLower().Trim();
                if (isEmpty(normalizedSpeech))
                    return new SpeechResponse(blockedReplies[Utility.Random(blockedReplies.Length)], Speaker, 0, 0, null, null);

                SpeechResponse topicResponse;
                if (TryGetTopicHelpResponse(normalizedSpeech, Speaker, out topicResponse))
                    return topicResponse;

                string bestMatch = null;
                bool bestMatchIsWord = false;
                string paddedSpeech = ' ' + normalizedSpeech + ' ';
                string[] speechWords = normalizedSpeech.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries);
                DataRow bestFuzzyRow = null;
                bool bestFuzzyIsWord = false;
                string bestFuzzyTrigger = null;
                double bestFuzzyScore = double.MaxValue;

                foreach (DataRow dr in SpeechData.dsSpeechRules.Tables["dtTriggers"].Rows)
                {
                    test = dr["trigger"].ToString();
                    if (!isEmpty(test))
                        test = test.ToLower();

                    isWord = (bool)dr["word"];
                    if (isWord)
                        test = ' ' + test + ' ';

                    // Search for a substring
                    if (!isEmpty(test) && paddedSpeech.IndexOf(test) >= 0)
                    {
                        if (
                            bestMatch == null // First match found
                            || (bestMatchIsWord && !isWord) // Replace - Phrase > Single word
                            || bestMatch.Length < test.Length // Replace - Longer > Shorter
                        )
                        {
                            bestMatch = test;
                            bestMatchIsWord = isWord;
                            drFound = dr;
                        }

                        if (Townsperson.Logging == Townsperson.LogLevel.Debug)
                            TownspersonLogging.WriteLine(Speaker, "Trigger matched: \"{0}\" : \"{1}\"", test, normalizedSpeech);
                    }
                    else
                    {
                        string triggerForDistance = isWord ? test.Trim() : test;
                        double score;

                        if (isWord)
                            score = GetBestWordScore(speechWords, triggerForDistance);
                        else
                            score = CalculateNormalizedDistance(normalizedSpeech, triggerForDistance);

                        if (score < bestFuzzyScore)
                        {
                            bestFuzzyScore = score;
                            bestFuzzyRow = dr;
                            bestFuzzyIsWord = isWord;
                            bestFuzzyTrigger = triggerForDistance;
                        }
                    }
                }

                if (drFound == null && bestFuzzyRow != null && bestFuzzyScore <= FuzzyThreshold)
                {
                    drFound = bestFuzzyRow;
                    bestMatch = bestFuzzyTrigger;
                    bestMatchIsWord = bestFuzzyIsWord;

                    if (Townsperson.Logging == Townsperson.LogLevel.Debug)
                        TownspersonLogging.WriteLine(Speaker, "Fuzzy trigger matched: \"{0}\" (score {1:F2})", bestMatch, bestFuzzyScore);
                }

                dvReplies = new DataView(SpeechData.dsSpeechRules.Tables["dtResponses"]);
                dvReplies.RowStateFilter = DataViewRowState.CurrentRows;
                dvReplies.Sort = "index";

                if (drFound == null)
                {
                    key = (object)0;

                    if (Townsperson.Logging == Townsperson.LogLevel.Basic || Townsperson.Logging == Townsperson.LogLevel.Debug)
                        TownspersonLogging.WriteLine(Speaker, "Default Rule: \"{0}\"", normalizedSpeech.ToUpper());
                }
                else
                    key = (object)drFound[0].ToString();

                drvReplies = dvReplies.FindRows(key);

                foreach (DataRowView drv in drvReplies)
                {
                    if ((int)drv["npcAttitude"] != 0 && (int)drv["npcAttitude"] != (int)tp.attitude)
                        continue;
                    if ((int)drv["playerGender"] != 0 && (int)drv["playerGender"] != (Speaker.Female ? 2 : 1))
                        continue;
                    if ((int)drv["npcGender"] != 0 && (int)drv["npcGender"] != (tp.Female ? 2 : 1))
                        continue;
                    if ((int)drv["timeOfDay"] != 0 && !(Townsperson.CheckTOD(tp, (int)drv["timeOfDay"])))
                        continue;
                    if (!isEmpty(drv["npcRegion"].ToString()) && drv["npcRegion"].ToString() != tp.Region.ToString())
                        continue;
                    if (!isEmpty(drv["npcTag"].ToString()) && drv["npcTag"].ToString() != tp.Tag)
                        continue;
                    if (!isEmpty(drv["npcName"].ToString()) && drv["npcName"].ToString() != tp.Name)
                        continue;
                    if (!isEmpty(drv["npcTitle"].ToString()) && drv["npcTitle"].ToString() != tp.Title)
                        continue;
                    if ((int)drv["objStatus"] != 0)
                    {
                        Item item = Townsperson.CheckInventory(Speaker, drv["questObject"].ToString());
                        if (item == null && (int)drv["objStatus"] == 1)
                            continue;
                        if (item != null && (int)drv["objStatus"] == 2)
                            continue;
                    }

                    alReplies.Add(drv);
                }

                int cnt = alReplies.Count;
				
				if (cnt <= 0)
				{
					sr = new SpeechResponse(blockedReplies[Utility.Random(blockedReplies.Length)], Speaker, 0, 0, null, null);
				} 
				else
				{
					DataRowView reply = (DataRowView)alReplies[Utility.Random(cnt)];

					if (Townsperson.Logging == Townsperson.LogLevel.Debug)
					TownspersonLogging.WriteLine(Speaker, "Matched {0} Responses.", cnt);

					string toSay = reply["response"].ToString();
					if (toSay == "{blank}")
					toSay = "";
					int anim = (int)reply["npcAnimation"];
					int react = (int)reply["npcReaction"];
					string reward = reply["packObject"].ToString(); // is it better to pass empty string or null?
					string remove = null;
					if (!isEmpty(reply["questObject"].ToString()) && (bool)reply["questObjDelete"])
					remove = reply["questObject"].ToString();

					sr = new SpeechResponse(toSay, Speaker, anim, react, reward, remove);
				}

            }

            // Delay results for more realistic conversation
            TimeSpan timeused = DateTime.UtcNow - started;
            TimeSpan timeleft = duration - timeused;

            if (Townsperson.Logging == Townsperson.LogLevel.Debug)
                TownspersonLogging.WriteLine(Speaker, "Asynchronous call took {0} ms.", timeused.Milliseconds.ToString());

            if (timeleft > TimeSpan.Zero && !Townsperson.Synchronous)
                Thread.Sleep(timeleft);

            return sr;
        }

        private static bool TryGetTopicHelpResponse(string normalizedSpeech, Mobile speaker, out SpeechResponse sr)
        {
            sr = default(SpeechResponse);

            if (string.IsNullOrEmpty(normalizedSpeech))
                return false;

            for (int i = 0; i < TopicHelps.Length; i++)
            {
                TopicHelpDefinition help = TopicHelps[i];

                if (!help.Matches(normalizedSpeech))
                    continue;

                if (help.SkipWhenSpeechContainsWhere && normalizedSpeech.IndexOf("where") >= 0)
                    continue;

                sr = new SpeechResponse(help.Response, speaker, 0, 0, null, null);
                return true;
            }

            return false;
        }

        private static double GetBestWordScore(string[] words, string trigger)
        {
            if (words == null || words.Length == 0 || isEmpty(trigger))
                return double.MaxValue;

            double best = double.MaxValue;

            foreach (string word in words)
            {
                if (isEmpty(word))
                    continue;

                double score = CalculateNormalizedDistance(word, trigger);

                if (score < best)
                    best = score;
            }

            return best;
        }

        private static double CalculateNormalizedDistance(string source, string target)
        {
            if (isEmpty(source) && isEmpty(target))
                return 0.0;

            if (isEmpty(source) || isEmpty(target))
                return 1.0;

            int distance = ComputeLevenshteinDistance(source, target);
            int maxLength = Math.Max(source.Length, target.Length);

            if (maxLength == 0)
                return 0.0;

            return (double)distance / maxLength;
        }

        private static int ComputeLevenshteinDistance(string source, string target)
        {
            int sourceLength = source.Length;
            int targetLength = target.Length;

            if (sourceLength == 0)
                return targetLength;

            if (targetLength == 0)
                return sourceLength;

            int[,] distances = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; i++)
                distances[i, 0] = i;

            for (int j = 0; j <= targetLength; j++)
                distances[0, j] = j;

            for (int i = 1; i <= sourceLength; i++)
            {
                char sourceChar = source[i - 1];

                for (int j = 1; j <= targetLength; j++)
                {
                    char targetChar = target[j - 1];
                    int cost = (sourceChar == targetChar) ? 0 : 1;

                    int deletion = distances[i - 1, j] + 1;
                    int insertion = distances[i, j - 1] + 1;
                    int substitution = distances[i - 1, j - 1] + cost;

                    int value = Math.Min(Math.Min(deletion, insertion), substitution);

                    distances[i, j] = value;
                }
            }

            return distances[sourceLength, targetLength];
        }

        private static bool isEmpty(string str)
        {
            return (str == null || str == "");
        }
    }
}
