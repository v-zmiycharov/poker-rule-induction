using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    public class PokerHand
    {
        public int? Id { get; set; }
        public List<Card> Cards { get; set; }
        public int? Hand { get; set; }

        public PokerHand() { }

        public PokerHand(string input, bool hasId = false)
        {
            this.Cards = new List<Card>();

            var items = input.Split(',');
            if (items.Length != Constants.ITEMS_COUNT_IN_LINE)
                throw new Exception("Invalid input line");

            int index = 0;
            if (hasId)
            {
                this.Id = Int32.Parse(items[0]);
                index++;
            }
            else
            {
                this.Hand = Int32.Parse(items[Constants.ITEMS_COUNT_IN_LINE - 1]);
            }

            for (int i = 0; i < Constants.CARDS_COUNT_IN_HAND; i++)
            {
                int suit = Int32.Parse(items[index]);
                int rank = Int32.Parse(items[index+1]);
                index += 2;
                this.Cards.Add(new Card(suit, rank));
            }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.Id.HasValue)
                sb.Append(this.Id.Value + Constants.COMMA_SEPARATOR);

            if(this.Cards != null)
                foreach(var card in Cards)
                {
                    sb.Append(card.Suit + Constants.COMMA_SEPARATOR);
                    sb.Append(card.Rank + Constants.COMMA_SEPARATOR);
                }

            if (this.Hand.HasValue)
                sb.Append(this.Hand.Value);

            string result = sb.ToString();

            if (result.EndsWith(Constants.COMMA_SEPARATOR))
                result = result.Substring(0, result.Length - 1);

            return result;
        }
    }
}
