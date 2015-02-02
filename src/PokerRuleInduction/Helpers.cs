using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    public static class Helpers
    {
        #region Train

        // From train data
        public static Dictionary<int, List<List<OrderedCardsRule>>> HandOrderedCardsDict = new Dictionary<int, List<List<OrderedCardsRule>>>();
        public static Dictionary<int, List<List<SameRankRule>>> HandSameRankDict = new Dictionary<int, List<List<SameRankRule>>>();
        public static Dictionary<int, List<List<SameSuitRule>>> HandSameSuitDict = new Dictionary<int, List<List<SameSuitRule>>>();
        public static Dictionary<int, int> HandCountDict = new Dictionary<int, int>();

        public static void ReadTrainData()
        {
            string projectDir = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            StreamReader file = new StreamReader(projectDir + "\\Samples\\train.csv");

            // Skip first line
            string line = file.ReadLine();

            while ((line = file.ReadLine()) != null)
            {
                var hand = new PokerHand(line);

                if (!HandOrderedCardsDict.ContainsKey(hand.Hand.Value))
                    HandOrderedCardsDict.Add(hand.Hand.Value, new List<List<OrderedCardsRule>>());
                HandOrderedCardsDict[hand.Hand.Value].Add(hand.GetOrderedCardsRules());

                if (!HandSameRankDict.ContainsKey(hand.Hand.Value))
                    HandSameRankDict.Add(hand.Hand.Value, new List<List<SameRankRule>>());
                HandSameRankDict[hand.Hand.Value].Add(hand.GetSameRankRules());

                if (!HandSameSuitDict.ContainsKey(hand.Hand.Value))
                    HandSameSuitDict.Add(hand.Hand.Value, new List<List<SameSuitRule>>());
                HandSameSuitDict[hand.Hand.Value].Add(hand.GetSameSuitRules());

                if (!HandCountDict.ContainsKey(hand.Hand.Value))
                    HandCountDict.Add(hand.Hand.Value, 0);
                HandCountDict[hand.Hand.Value]++;
            }
        }

        #endregion

        #region Analyze level 1

        private static double GetRulePercentageByHand(int hand, RuleType ruleType, int ruleOccurs)
        {
            var totalCount = HandCountDict[hand];

            switch (ruleType)
            {
                case RuleType.OrderedCards: return (double)(HandOrderedCardsDict[hand].Where(e => e.Count == ruleOccurs).Count() * 100) / totalCount;
                case RuleType.SameRank: return (double)(HandSameRankDict[hand].Where(e => e.Count == ruleOccurs).Count() * 100) / totalCount;
                case RuleType.SameSuit: return (double)(HandSameSuitDict[hand].Where(e => e.Count == ruleOccurs).Count() * 100) / totalCount;
            }

            throw new Exception("Unknown rule type");
        }

        private static List<int> GetRuleOccurs(RuleType ruleType, int hand)
        {
            switch (ruleType)
            {
                case RuleType.OrderedCards: return HandOrderedCardsDict[hand].Select(e => e.Count).Distinct().OrderBy(e => e).ToList();
                case RuleType.SameRank: return HandSameRankDict[hand].Select(e => e.Count).Distinct().OrderBy(e => e).ToList();
                case RuleType.SameSuit: return HandSameSuitDict[hand].Select(e => e.Count).Distinct().OrderBy(e => e).ToList();
            }

            throw new Exception("Unknown rule type");
        }

        public static Dictionary<int, List<ConclusiveRule>> HandConclusiveRulesDict = new Dictionary<int, List<ConclusiveRule>>();
        
        private static void FillHandConclusiveRulesDict(int hand, RuleType type)
        {
            var occurs = GetRuleOccurs(type, hand);
            foreach (var occur in occurs)
            {
                var perc = GetRulePercentageByHand(hand, type, occur);

                Debug.WriteLine(String.Format("Hand {0}: {1}% {2} occurs rule {3}", hand
                    , perc, occur, type.ToString()));
                
                if (perc == 100)
                {
                    if (!HandConclusiveRulesDict.ContainsKey(hand))
                        HandConclusiveRulesDict.Add(hand, new List<ConclusiveRule>());
                    HandConclusiveRulesDict[hand].Add(new ConclusiveRule(type, occur));
                }
            }
        }

        public static void SaveConclusiveRules()
        {
            var hands = HandCountDict.Keys.OrderBy(e => e);

            foreach (var hand in hands)
            {
                FillHandConclusiveRulesDict(hand, RuleType.OrderedCards);
                FillHandConclusiveRulesDict(hand, RuleType.SameRank);
                FillHandConclusiveRulesDict(hand, RuleType.SameSuit);

                Debug.WriteLine("");
                Debug.WriteLine("");
            }
        }

        #endregion

    }
}
