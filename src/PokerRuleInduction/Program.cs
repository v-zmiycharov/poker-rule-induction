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
            

            Console.WriteLine("Press any key to exit the program");
            Console.ReadKey();
        }
    }
}
