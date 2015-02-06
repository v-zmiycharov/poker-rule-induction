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
                int rank = Int32.Parse(items[index + 1]);
                index += 2;
                this.Cards.Add(new Card(suit, rank));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.Id.HasValue)
                sb.Append(this.Id.Value + Constants.COMMA_SEPARATOR);

            if (this.Cards != null)
                foreach (var card in Cards)
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

        public string GetRulesInfo()
        {
            StringBuilder sb = new StringBuilder();

            var orderedCards = this.GetOrderedCardsRules();
            if (orderedCards != null && orderedCards.Count > 0)
            {
                foreach (var rule in orderedCards)
                {
                    sb.Append(rule.ToString() + Environment.NewLine);
                }
            }

            var sameSuitCards = this.GetSameSuitRules();
            if (sameSuitCards != null && sameSuitCards.Count > 0)
            {
                foreach (var rule in sameSuitCards)
                {
                    sb.Append(rule.ToString() + Environment.NewLine);
                }
            }

            var sameRankCards = this.GetSameRankRules();
            if (sameRankCards != null && sameRankCards.Count > 0)
            {
                foreach (var rule in sameRankCards)
                {
                    sb.Append(rule.ToString() + Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        #region Get rules

        public List<OrderedCardsRule> GetOrderedCardsRules()
        {
            List<OrderedCardsRule> result = new List<OrderedCardsRule>();

            var orderedCards = this.Cards.OrderBy(e => e.Rank).ToList();

            for (int i = 0; i < orderedCards.Count; i++)
            {
                var currentCollection = new List<Card>();
                currentCollection.Add(orderedCards[i]);

                while (i + 1 != orderedCards.Count && orderedCards[i + 1].Rank - orderedCards[i].Rank == 1)
                {
                    i++;
                    currentCollection.Add(orderedCards[i]);
                }

                if (i + 1 == orderedCards.Count && orderedCards[i].Rank - orderedCards[0].Rank == 12)
                    currentCollection.Add(orderedCards[0]);

                if (currentCollection.Count > 1)
                    result.Add(new OrderedCardsRule()
                        {
                            StartRank = currentCollection[0].Rank,
                            Suits = currentCollection.Select(e => e.Suit).ToList()
                        });
            }

            return result;
        }

        public List<SameSuitRule> GetSameSuitRules()
        {
            List<SameSuitRule> result = new List<SameSuitRule>();

            Dictionary<int, List<Card>> suitGroups = this.Cards.GroupBy(e => e.Suit)
                .ToDictionary(e => e.Key, e => e.ToList());

            result.AddRange(
                suitGroups.Where(e => e.Value.Count > 2)
                .Select(e => new SameSuitRule()
                {
                    Suit = e.Key,
                    Ranks = e.Value.Select(val => val.Rank).ToList()
                }).ToList()
                );

            return result;
        }

        public List<SameRankRule> GetSameRankRules()
        {
            List<SameRankRule> result = new List<SameRankRule>();

            Dictionary<int, List<Card>> rankGroups = this.Cards.GroupBy(e => e.Rank)
                .ToDictionary(e => e.Key, e => e.ToList());

            result.AddRange(
                rankGroups.Where(e => e.Value.Count > 1)
                .Select(e => new SameRankRule()
                {
                    Rank = e.Key,
                    Suits = e.Value.Select(val => val.Suit).ToList()
                }).ToList()
                );

            return result;
        }

        #endregion

        public bool ContainsRule(ConclusiveRule rule)
        {
            switch (rule.Type)
            {
                case RuleType.OrderedCards:
                    {
                        var rules = this.GetOrderedCardsRules();
                        if (rules.Count != rule.Occurs)
                            return false;

                        if (!Helpers.AreListsSame(rule.Sizes, rules.Select(e => e.Size).ToList()))
                            return false;

                        if(rule.AreOrderedCardsSameSuit.HasValue)
                        {
                            var distinctSuitsCounts = rules.Select(e=>e.Suits.Distinct().Count());

                            if(rule.AreOrderedCardsSameSuit.Value
                                && distinctSuitsCounts.Any(e=>e != 1))
                                return false;

                            if (!rule.AreOrderedCardsSameSuit.Value
                                && distinctSuitsCounts.Any(e => e == 1))
                                return false;
                        }

                        if(rule.OrderedCardsStart != null && rule.OrderedCardsStart.Count == 1)
                        {
                            int start = rule.OrderedCardsStart[0];
                            if (rules.Any(e => e.StartRank != start))
                                return false;
                        }
                    } break;

                case RuleType.SameSuit:
                    {
                        var rules = this.GetSameSuitRules();
                        if (rules.Count != rule.Occurs)
                            return false;

                        if (!Helpers.AreListsSame(rule.Sizes, rules.Select(e => e.Size).ToList()))
                            return false;

                        if (rule.AreSameSuitCardsOrdered.HasValue)
                        {
                            if (rule.AreSameSuitCardsOrdered.Value
                                && rules.Any(e => !Helpers.IsOrderedList(e.Ranks)))
                                return false;

                            if (!rule.AreSameSuitCardsOrdered.Value
                                && rules.Any(e => Helpers.IsOrderedList(e.Ranks)))
                                return false;
                        }
                    } break;

                case RuleType.SameRank:
                    {
                        var rules = this.GetSameRankRules();
                        if (rules.Count != rule.Occurs)
                            return false;

                        if (!Helpers.AreListsSame(rule.Sizes, rules.Select(e => e.Size).ToList()))
                            return false;
                    } break;
            }

            return true;
        }
    }
}
