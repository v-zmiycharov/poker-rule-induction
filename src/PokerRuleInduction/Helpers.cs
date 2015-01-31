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
        public static Dictionary<int, List<OrderedCardsRule>> HandOrderedCardsDict = new Dictionary<int,List<OrderedCardsRule>>();
        public static Dictionary<int, List<SameRankRule>> HandSameRankDict = new Dictionary<int,List<SameRankRule>>();
        public static Dictionary<int, List<SameSuitRule>> HandSameSuitDict = new Dictionary<int, List<SameSuitRule>>();
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
                
                if(!HandOrderedCardsDict.ContainsKey(hand.Hand.Value))
                    HandOrderedCardsDict.Add(hand.Hand.Value, new List<OrderedCardsRule>());
                HandOrderedCardsDict[hand.Hand.Value].AddRange(hand.GetOrderedCardsRules());
                
                if(!HandSameRankDict.ContainsKey(hand.Hand.Value))
                    HandSameRankDict.Add(hand.Hand.Value, new List<SameRankRule>());
                HandSameRankDict[hand.Hand.Value].AddRange(hand.GetSameRankRules());
                
                if(!HandSameSuitDict.ContainsKey(hand.Hand.Value))
                    HandSameSuitDict.Add(hand.Hand.Value, new List<SameSuitRule>());
                HandSameSuitDict[hand.Hand.Value].AddRange(hand.GetSameSuitRules());

                if (!HandCountDict.ContainsKey(hand.Hand.Value))
                    HandCountDict.Add(hand.Hand.Value, 0);
                HandCountDict[hand.Hand.Value]++;
            }
        }
    }
}
