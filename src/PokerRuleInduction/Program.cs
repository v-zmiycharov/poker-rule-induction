using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    class Program
    {
        static void Main(string[] args)
        {
            string projectDir = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            Helpers.ReadTrainData(projectDir + "\\Samples\\train.csv");
            Helpers.SaveConclusiveRules();
            Helpers.FindConclusiveRulesSizes();
            Helpers.DetermineFinalRules();
            Helpers.RemoveSameRules();
            Helpers.FillAdditionalFields();
            Helpers.CompleteFinalRules();
            Helpers.DetermineUndefinedHands();
            Helpers.OrderHands();

            Helpers.ReadTestData();
            Helpers.WriteResultDict();
            
            Console.WriteLine("Press any key to exit the program");
            Console.ReadKey();
        }
    }
}
