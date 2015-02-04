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
        public List<int> Sizes { get; set; }

        #region Additional fields

        public bool? AreSameSuitCardsOrdered { get; set; }
        
        public bool? AreOrderedCardsSameSuit { get; set; }
        public List<int> OrderedCardsStart { get; set; }

        #endregion

        public ConclusiveRule() { }
        
        public ConclusiveRule(RuleType type, int occurs)
        {
            this.Type = type;
            this.Occurs = occurs;
        }
    }
}
