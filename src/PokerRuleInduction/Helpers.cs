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

        #region Conclusive rules sizes

        public static void FindConclusiveRulesSizes()
        {
            foreach (var key in HandConclusiveRulesDict.Keys)
            {
                var rules = new List<ConclusiveRule>();
                rules.AddRange(HandConclusiveRulesDict[key]);

                foreach (var rule in rules)
                {
                    switch (rule.Type)
                    {
                        case RuleType.OrderedCards:
                            {
                                List<int> ruleSizes = new List<int>();
                                var foundRules = HandOrderedCardsDict[key];
                                foreach (var list in foundRules)
                                {
                                    ruleSizes.AddRange(list.Select(e => e.Size).Distinct());
                                }
                                ruleSizes = ruleSizes.Distinct().ToList();

                                if (ruleSizes.Count > rule.Occurs)
                                    HandConclusiveRulesDict[key].Remove(rule);
                                else if (rule.Occurs == 1)
                                {
                                    rule.Sizes = ruleSizes;
                                }
                                else if (rule.Occurs >= 2)
                                {
                                    rule.Sizes = ruleSizes;

                                    var firstList = foundRules[0];

                                    Dictionary<int, int> SizesCountsDict = firstList.GroupBy(e => e.Size)
                                        .ToDictionary(e => e.Key, e => e.Count());

                                    foreach (var list in foundRules)
                                    {
                                        foreach (var size in SizesCountsDict.Keys)
                                        {
                                            if (list.Count(e => e.Size == size) != SizesCountsDict[size])
                                            {
                                                HandConclusiveRulesDict[key].Remove(rule);
                                            }
                                        }
                                    }
                                }
                            } break;

                        case RuleType.SameRank:
                            {
                                List<int> ruleSizes = new List<int>();
                                var foundRules = HandSameRankDict[key];
                                foreach (var list in foundRules)
                                {
                                    ruleSizes.AddRange(list.Select(e => e.Size).Distinct());
                                }
                                ruleSizes = ruleSizes.Distinct().ToList();

                                if (ruleSizes.Count > rule.Occurs)
                                    HandConclusiveRulesDict[key].Remove(rule);
                                else if (rule.Occurs == 1)
                                {
                                    rule.Sizes = ruleSizes;
                                }
                                else if (rule.Occurs >= 2)
                                {
                                    rule.Sizes = ruleSizes;

                                    var firstList = foundRules[0];

                                    Dictionary<int, int> SizesCountsDict = firstList.GroupBy(e => e.Size)
                                        .ToDictionary(e => e.Key, e => e.Count());

                                    foreach (var list in foundRules)
                                    {
                                        foreach (var size in SizesCountsDict.Keys)
                                        {
                                            if (list.Count(e => e.Size == size) != SizesCountsDict[size])
                                            {
                                                HandConclusiveRulesDict[key].Remove(rule);
                                            }
                                        }
                                    }
                                }
                            } break;

                        case RuleType.SameSuit:
                            {
                                List<int> ruleSizes = new List<int>();
                                var foundRules = HandSameSuitDict[key];
                                foreach (var list in foundRules)
                                {
                                    ruleSizes.AddRange(list.Select(e => e.Size).Distinct());
                                }
                                ruleSizes = ruleSizes.Distinct().ToList();

                                if (ruleSizes.Count > rule.Occurs)
                                    HandConclusiveRulesDict[key].Remove(rule);
                                else if (rule.Occurs == 1)
                                {
                                    rule.Sizes = ruleSizes;
                                }
                                else if (rule.Occurs >= 2)
                                {
                                    rule.Sizes = ruleSizes;

                                    var firstList = foundRules[0];

                                    Dictionary<int, int> SizesCountsDict = firstList.GroupBy(e => e.Size)
                                        .ToDictionary(e => e.Key, e => e.Count());

                                    foreach (var list in foundRules)
                                    {
                                        foreach (var size in SizesCountsDict.Keys)
                                        {
                                            if (list.Count(e => e.Size == size) != SizesCountsDict[size])
                                            {
                                                HandConclusiveRulesDict[key].Remove(rule);
                                            }
                                        }
                                    }
                                }
                            } break;
                        default: throw new Exception("Invalid rule type");
                    }
                }
            }
        }

        #endregion

        #region Define final rules

        public static List<int> FinalHands = new List<int>();

        public static void DetermineFinalRules()
        {
            int keysCount = HandConclusiveRulesDict.Keys.Count;
            var keys = new List<int>();
            keys.AddRange(HandConclusiveRulesDict.Keys);

            foreach (var hand in keys)
            {
                List<int> keysOposingRules = new List<int>();

                foreach (var rule in HandConclusiveRulesDict[hand])
                {
                    keysOposingRules.AddRange(
                        HandConclusiveRulesDict.Keys
                        .Where(e => HandConclusiveRulesDict[e]
                        .Any(r => r.Type == rule.Type
                        && (r.Occurs != rule.Occurs || !AreListsSame(r.Sizes, rule.Sizes)))
                        &&
                        !HandConclusiveRulesDict[e]
                        .Any(r => r.Type == rule.Type
                        && r.Occurs == rule.Occurs && AreListsSame(r.Sizes, rule.Sizes))
                        )
                    );
                }

                keysOposingRules = keysOposingRules.Distinct().ToList();

                if (keysOposingRules.Count == keysCount - 1)
                    FinalHands.Add(hand);
            }
        }

        private static bool AreListsSame(List<int> list1, List<int> list2)
        {
            if (list1 == null && list2 == null)
                return true;

            if ((list1 == null && list2 != null)
                || (list1 != null && list2 == null)
                || list1.Count != list2.Count)
                return false;

            list1 = list1.OrderBy(e => e).ToList();
            list2 = list2.OrderBy(e => e).ToList();

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }

            return true;
        }

        #endregion

        #region Fill final rules

        public static Dictionary<int, List<ConclusiveRule>> FinalRulesDict = new Dictionary<int, List<ConclusiveRule>>();

        public static void FillFinalRulesDict()
        {
            foreach (var hand in FinalHands)
            {
                if (HandConclusiveRulesDict.ContainsKey(hand))
                {
                    FinalRulesDict.Add(hand, HandConclusiveRulesDict[hand]);

                    HandConclusiveRulesDict.Remove(hand);
                }
            }
        }

        #endregion

        #region Remove same rules

        public static void RemoveSameRules()
        {
            List<ConclusiveRule> sameRules = new List<ConclusiveRule>();

            var keys = new List<int>();
            keys.AddRange(HandConclusiveRulesDict.Keys);
            int keysCount = keys.Count;

            foreach (var hand in keys)
            {
                foreach (var rule in HandConclusiveRulesDict[hand])
                {
                    int occurances = HandConclusiveRulesDict.Values
                        .Count(e => e.Any(r => r.Type == rule.Type && r.Occurs == rule.Occurs && AreListsSame(r.Sizes, rule.Sizes)));

                    if (occurances == keysCount && !sameRules.Any(r => r.Type == rule.Type && r.Occurs == rule.Occurs && AreListsSame(r.Sizes, rule.Sizes)))
                        sameRules.Add(rule);
                }
            }

            foreach (var rule in sameRules)
            {
                foreach (var hand in keys)
                {
                    var foundRule = HandConclusiveRulesDict[hand].First(r => r.Type == rule.Type && r.Occurs == rule.Occurs && AreListsSame(r.Sizes, rule.Sizes));
                    HandConclusiveRulesDict[hand].Remove(foundRule);
                }
            }

            var emptyHands = HandConclusiveRulesDict.Keys.Where(e => HandConclusiveRulesDict[e].Count == 0);

            FinalHands.AddRange(emptyHands);

            FillFinalRulesDict();
        }

        #endregion
    }
}
