using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    public class SameRankRule
    {
        public int Rank { get; set; }
        public List<int> Suits { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("Same rank: {0}: {1}", this.Rank, String.Join(Constants.COMMA_SEPARATOR, this.Suits)));
            return sb.ToString();
        }
    }
}
