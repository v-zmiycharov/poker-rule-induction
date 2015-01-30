using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    public class OrderedCardsRule
    {
        public int StartRank { get; set; }
        public List<int> Suits { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("Ordered cards: {0}: {1}", this.StartRank, String.Join(Constants.COMMA_SEPARATOR, this.Suits)));
            return sb.ToString();
        }
    }
}
