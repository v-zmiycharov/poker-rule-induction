using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    public class SameSuitRule
    {
        public int Suit { get; set; }
        public List<int> Ranks { get; set; }

        public int Size
        {
            get
            {
                return this.Ranks.Count;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("Same suit: {0}: {1}", this.Suit, String.Join(Constants.COMMA_SEPARATOR, this.Ranks)));
            return sb.ToString();
        }
    }
}
