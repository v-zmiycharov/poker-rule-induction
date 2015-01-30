using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    class Program
    {
        static void Main(string[] args)
        {
            Helpers.ReadTrainData();

            Console.WriteLine("Press any key to exit the program");
            Console.ReadKey();
        }
    }
}
