using System;
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Desi_tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void getDealerfromDealString()
        {
            string deal = "[Deal \"N:.63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";

            //get dealer

            var expected = Desi.Dealer.North;
            var result = Desi.getDealer(deal);

            Assert.AreEqual(expected,result);

        }

         [TestMethod]
        public void getHandsStringfromDealString()
        {
            string deal = "[Deal \"N:.63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";

            //get hands

            var expected = ".63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85";
            var result = Desi.getHandsString(deal);

            Assert.AreEqual(expected, result);

        }

         [TestMethod]
         public void getHandsListfromDealString()
         {
             string deal = "[Deal \"N:.63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";

             var handsList = new List<string>() { ".63.AKQ987.A9732","A8654.KQ5.T.QJT6","J973.J98742.3.K4","KQT2.AT.J6542.85" };

             FSharpList<string> expected = ListModule.OfSeq(handsList);

             var result = Desi.getHandList(deal);

             Assert.AreEqual(expected, result);

         }

    }
}
