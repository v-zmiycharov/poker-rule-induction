﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerRuleInduction
{
    public class Card
    {
        /// <summary>
        /// Боя: 1/2/3/4 Купа/Пика/Каро/Спатия
        /// </summary>
        public int Suit { get; set; }

        /// <summary>
        /// Стойност: 2-14
        /// </summary>
        public int Rank { get; set; }

        public Card() { }

        public Card(int suit, int rank)
        {
            this.Suit = suit;

            // Ace case
            // if (rank == 1)
            //     rank = 14;

            this.Rank = rank;
        }
    }
}
