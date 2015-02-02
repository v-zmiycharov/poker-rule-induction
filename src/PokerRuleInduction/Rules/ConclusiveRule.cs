using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    public class ConclusiveRule
    {
        public RuleType Type { get; set; }
        public int Occurs { get; set; }

        public ConclusiveRule() { }
        
        public ConclusiveRule(RuleType type, int occurs)
        {
            this.Type = type;
            this.Occurs = occurs;
        }
    }
}
