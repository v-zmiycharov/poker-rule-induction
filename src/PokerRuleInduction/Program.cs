using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    class Program
    {
        static void Main(string[] args)
        {
            Helpers.ReadTrainData();
            Helpers.SaveConclusiveRules();
            Helpers.FindConclusiveRulesSizes();
            Helpers.DetermineFinalRules();
            Helpers.RemoveSameRules();
            Helpers.FillAdditionalFields();
            Helpers.CompleteFinalRules();
            Helpers.DetermineUndefinedHands();
            
            Console.WriteLine("Press any key to exit the program");
            Console.ReadKey();
        }
    }
}
