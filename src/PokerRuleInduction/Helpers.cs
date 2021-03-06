﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    public static class Helpers
    {
        private static bool writeToFile = true;

        #region Train

        // From train data
        public static Dictionary<int, List<List<OrderedCardsRule>>> HandOrderedCardsDict = new Dictionary<int, List<List<OrderedCardsRule>>>();
        public static Dictionary<int, List<List<SameRankRule>>> HandSameRankDict = new Dictionary<int, List<List<SameRankRule>>>();
        public static Dictionary<int, List<List<SameSuitRule>>> HandSameSuitDict = new Dictionary<int, List<List<SameSuitRule>>>();
        public static Dictionary<int, int> HandCountDict = new Dictionary<int, int>();

        public static void ReadTrainData(string path)
        {
            StreamReader file = new StreamReader(path);

            // Skip first line
            string line = file.ReadLine();

            while ((line = file.ReadLine()) != null)
            {
                var hand = new PokerHand(line, false, true);

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

            FillFinalRulesDict();
        }

        public static bool AreListsSame(List<int> list1, List<int> list2)
        {
            if ((list1 == null && list2 == null)
                || (list1 == null && list2 != null && list2.Count == 0)
                || (list2 == null && list1 != null && list1.Count == 0))
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

            if (HandConclusiveRulesDict.Keys.Count == 1)
            {
                var key = HandConclusiveRulesDict.Keys.First();

                FinalRulesDict.Add(key, HandConclusiveRulesDict[key]);

                HandConclusiveRulesDict.Remove(key);
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

        #region Fill additional fields

        public static void FillAdditionalFields()
        {
            var keys = new List<int>();
            keys.AddRange(HandConclusiveRulesDict.Keys);

            foreach (var hand in keys)
            {
                var rules = new List<ConclusiveRule>();
                rules.AddRange(HandConclusiveRulesDict[hand]);

                foreach (var rule in rules)
                {
                    if (rule.Type == RuleType.SameSuit)
                    {
                        var foundRule = HandConclusiveRulesDict[hand]
                            .First(e => e.Type == rule.Type && e.Occurs == rule.Occurs && AreListsSame(e.Sizes, rule.Sizes));

                        if (!HandSameSuitDict[hand].Any(e => e.Any(r => !IsOrderedList(r.Ranks))))
                        {
                            foundRule.AreSameSuitCardsOrdered = true;
                        }

                        if (!HandSameSuitDict[hand].Any(e => e.Any(r => IsOrderedList(r.Ranks))))
                        {
                            foundRule.AreSameSuitCardsOrdered = false;
                        }
                    }

                    if (rule.Type == RuleType.OrderedCards)
                    {
                        var foundRule = HandConclusiveRulesDict[hand]
                            .First(e => e.Type == rule.Type && e.Occurs == rule.Occurs && AreListsSame(e.Sizes, rule.Sizes));

                        if (!HandOrderedCardsDict[hand].Any(e => e.Any(r => !(r.Suits.Distinct().Count() == 1))))
                        {
                            foundRule.AreOrderedCardsSameSuit = true;
                        }

                        if (!HandOrderedCardsDict[hand].Any(e => e.Any(r => (r.Suits.Distinct().Count() == 1))))
                        {
                            foundRule.AreOrderedCardsSameSuit = false;
                        }

                        List<int> startRanks = new List<int>();

                        var listOfRules = HandOrderedCardsDict[hand];
                        foreach (var list in listOfRules)
                        {
                            startRanks.AddRange(list.Select(e => e.StartRank));
                        }
                        startRanks = startRanks.Distinct().ToList();

                        foundRule.OrderedCardsStart = startRanks;
                    }
                }
            }
        }

        public static bool IsOrderedList(List<int> list)
        {
            if (list == null || list.Count <= 1)
                return true;

            var orderedList = list.OrderBy(e => e).ToList();

            for (int i = 1; i < list.Count - 1; i++)
            {
                if (orderedList[i + 1] - orderedList[i] != 1)
                    return false;
            }

            if (orderedList[1] - orderedList[0] != 1 && orderedList[list.Count - 1] - orderedList[0] != 12)
                return false;

            return true;
        }

        #endregion

        #region Complete final rules

        public static void CompleteFinalRules()
        {
            int keysCount = HandConclusiveRulesDict.Keys.Count;
            var keys = new List<int>();
            keys.AddRange(HandConclusiveRulesDict.Keys);

            foreach (var hand in keys)
            {
                List<int> keysOposingRules = new List<int>();

                foreach (var rule in HandConclusiveRulesDict[hand])
                {
                    if (rule.Type == RuleType.SameSuit && rule.AreSameSuitCardsOrdered.HasValue)
                    {
                        keysOposingRules.AddRange(
                           HandConclusiveRulesDict.Keys
                            .Where(e =>
                                HandConclusiveRulesDict[e].Any(r => r.Type == rule.Type
                                && (
                                r.AreSameSuitCardsOrdered.HasValue && r.AreSameSuitCardsOrdered.Value == !rule.AreSameSuitCardsOrdered.Value
                                ))
                                ||
                                (!rule.AreSameSuitCardsOrdered.Value
                                && HandConclusiveRulesDict[e].Any(r => r.Type == RuleType.OrderedCards && AreListsSame(r.Sizes, rule.Sizes))
                                )
                            )
                          );
                    }

                    if (rule.Type == RuleType.OrderedCards && rule.AreOrderedCardsSameSuit.HasValue)
                    {
                        keysOposingRules.AddRange(
                           HandConclusiveRulesDict.Keys
                            .Where(e =>
                                HandConclusiveRulesDict[e].Any(r => r.Type == rule.Type
                                && (
                                r.AreOrderedCardsSameSuit.HasValue && r.AreOrderedCardsSameSuit.Value == !rule.AreOrderedCardsSameSuit.Value
                                ))
                                ||
                                (!rule.AreOrderedCardsSameSuit.Value
                                && HandConclusiveRulesDict[e].Any(r => r.Type == RuleType.SameSuit && AreListsSame(r.Sizes, rule.Sizes))
                                )
                            )
                          );
                    }

                    if (rule.Type == RuleType.OrderedCards)
                    {
                        keysOposingRules.AddRange(
                           HandConclusiveRulesDict.Keys
                            .Where(e =>
                                HandConclusiveRulesDict[e].Any(r => r.Type == rule.Type
                                && !HaveSameMember(r.OrderedCardsStart, rule.OrderedCardsStart)
                            )));
                    }
                }

                keysOposingRules = keysOposingRules.Distinct().ToList();

                if (keysOposingRules.Count == keysCount - 1)
                    FinalHands.Add(hand);
            }

            FillFinalRulesDict();
        }

        private static bool HaveSameMember(List<int> list1, List<int> list2)
        {
            if (list1 == null || list1.Count == 0 || list2 == null || list2.Count == 0)
                return false;

            foreach (var item in list1)
            {
                if (list2.Any(e => e == item))
                    return true;
            }

            return false;
        }

        #endregion

        #region Undefined hands

        public static List<int> UndefinedHands = new List<int>();

        public static void DetermineUndefinedHands()
        {
            UndefinedHands.AddRange(FinalRulesDict.Keys.Where(e => FinalRulesDict[e].Count == 0));
        }

        public static int GetRandomUndefinedHand()
        {
            Random rnd = new Random();
            var index = rnd.Next(UndefinedHands.Count);

            return UndefinedHands[index];
        }

        #endregion

        #region Order hands

        public static List<int> OrderedHands = new List<int>();

        public static void OrderHands()
        {
            OrderedHands = FinalRulesDict.Keys
                .Where(e => !UndefinedHands.Contains(e))
                .OrderByDescending(e => HandCountDict[e]).ToList();
        }

        #endregion

        #region Read test data

        public static void ReadTestData()
        {
            string projectDir = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            StreamReader file = new StreamReader(projectDir + "\\Samples\\test.csv");

            // Skip first line
            string line = file.ReadLine();

            using (StreamWriter writer = new StreamWriter(projectDir + "\\result.txt"))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var hand = new PokerHand(line, true, false);

                    DetermineHand(hand);

                    if (writeToFile)
                        writer.WriteLine(hand.ToString());

                    FillResultDict(hand);
                }
            }
        }

        #endregion

        #region Determine hand

        public static void DetermineHand(PokerHand hand)
        {
            foreach (var currentHand in OrderedHands)
            {
                bool isCurrentHand = true;
                foreach (var rule in FinalRulesDict[currentHand])
                {
                    if (!hand.ContainsRule(rule))
                    {
                        isCurrentHand = false;
                        break;
                    }
                }

                if (isCurrentHand)
                {
                    hand.Hand = currentHand;
                }
            }

            if (!hand.Hand.HasValue)
                hand.Hand = GetRandomUndefinedHand();
        }

        #endregion

        #region Result dictionary

        public static Dictionary<int, List<int>> ResultDict = new Dictionary<int, List<int>>();

        private static void FillResultDict(PokerHand hand)
        {
            if (hand.Hand.HasValue && hand.Id.HasValue)
            {
                int value = hand.Hand.Value;
                if (!ResultDict.ContainsKey(value))
                    ResultDict.Add(value, new List<int>());

                ResultDict[value].Add(hand.Id.Value);
            }
        }

        public static void WriteResultDict()
        {
            string projectDir = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            using (StreamWriter writer = new StreamWriter(projectDir + "\\result-dict.txt"))
            {
                var keys = ResultDict.Keys.OrderBy(e => ResultDict[e].Count);

                foreach (var key in keys)
                {
                    string line = String.Format("{0}: {1}", key, String.Join(Constants.COMMA_SEPARATOR, ResultDict[key].Take(500)));

                    if (writeToFile)
                        writer.WriteLine(line);
                }
            }
        }

        #endregion
    }
}
