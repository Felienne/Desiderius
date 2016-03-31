using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Sodes.Bridge.Base.Test
{
    [TestClass]
    public class ContractTest
    {
        [TestMethod, TestCategory("CI"), TestCategory("Other")]
        public void Contract_Score()
        {
            var target = new Contract("1C", Seats.North, Vulnerable.Neither);
            target.tricksForDeclarer = 7;
            Assert.AreEqual<int>(70, target.Score, "Score for 1C C");
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(90, target.Score, "Score for 1C +1");

            target.Doubled = true;
            target.tricksForDeclarer = 7;
            Assert.AreEqual<int>(140, target.Score, "Score for 1Cx C");
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(240, target.Score, "Score for 1Cx +1");

            target.Redoubled = true;
            target.tricksForDeclarer = 7;
            Assert.AreEqual<int>(230, target.Score, "Score for 1Cxx C");
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(430, target.Score, "Score for 1Cxx +1");

            target.Vulnerability = Vulnerable.Both;
            target.Doubled = false;
            target.Redoubled = false;
            target.tricksForDeclarer = 7;
            Assert.AreEqual<int>(70, target.Score, "Score for 1C C");
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(90, target.Score, "Score for 1C +1");
            
            target.Doubled = true;
            target.tricksForDeclarer = 7;
            Assert.AreEqual<int>(140, target.Score, "Score for 1Cx C");
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(340, target.Score, "Score for 1Cx +1");
            
            target.Redoubled = true;
            target.tricksForDeclarer = 7;
            Assert.AreEqual<int>(230, target.Score, "Score for 1Cxx C");
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(630, target.Score, "Score for 1Cxx +1");

            target = new Contract("2H", Seats.North, Vulnerable.Neither);
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(110, target.Score, "Score for 2H C");
            target.tricksForDeclarer = 9;
            Assert.AreEqual<int>(140, target.Score, "Score for 2H +1");

            target.Doubled = true;
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(470, target.Score, "Score for 2Hx C");
            target.tricksForDeclarer = 9;
            Assert.AreEqual<int>(570, target.Score, "Score for 2Hx +1");

            target.Redoubled = true;
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(640, target.Score, "Score for 2Hxx C");
            target.tricksForDeclarer = 9;
            Assert.AreEqual<int>(840, target.Score, "Score for 2Hxx +1");

            target.Vulnerability = Vulnerable.Both;
            target.Doubled = false;
            target.Redoubled = false;
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(110, target.Score, "Score for 2H C");
            target.tricksForDeclarer = 9;
            Assert.AreEqual<int>(140, target.Score, "Score for 2H +1");

            target.Doubled = true;
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(670, target.Score, "Score for 2Hx C");
            target.tricksForDeclarer = 9;
            Assert.AreEqual<int>(870, target.Score, "Score for 2Hx +1");

            target.Redoubled = true;
            target.tricksForDeclarer = 8;
            Assert.AreEqual<int>(840, target.Score, "Score for 2Hxx C");
            target.tricksForDeclarer = 9;
            Assert.AreEqual<int>(1240, target.Score, "Score for 2Hxx +1");
        }
    }
}
