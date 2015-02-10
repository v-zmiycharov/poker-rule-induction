using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PokerRuleInduction.API.Controllers
{
    public class ValuesController : ApiController
    {
        public object Get(string hand)
        {
            var pokerHand = new PokerHand(hand);
            Helpers.DetermineHand(pokerHand);

            return new { value = pokerHand.Hand.Value };
        }
    }
}
