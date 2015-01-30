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
        public static void ReadTrainData()
        {
            string projectDir = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            StreamReader file = new StreamReader(projectDir + "\\Samples\\train.csv");

            // Skip first line
            string line = file.ReadLine();

            while ((line = file.ReadLine()) != null)
            {
                var hand = new PokerHand(line);
                Debug.WriteLine(hand.ToString());
            }
        }
    }
}
