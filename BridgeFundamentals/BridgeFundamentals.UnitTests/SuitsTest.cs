using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodes.Base;
using Sodes.Bridge.Base;
using System.Diagnostics;

namespace TestBaseClasses
{
    [TestClass]
    public class SuitsTest
    {
        [TestMethod, TestCategory("CI"), TestCategory("Other")]
        public void SuitRankCollection_Clones()
        {
            var target1 = new SuitRankCollectionInt();
            for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
            {
                for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                {
                    target1[s, r] = 13 * (int)s + (int)r;
                }
            }

            var target2 = target1.Clone();
            for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
            {
                for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                {
                    Assert.AreEqual<int>(target1[s, r], target2[s, r]);
                }
            }

            target1[Suits.Hearts, Ranks.Jack] = 1;
            Assert.AreEqual<int>(1, target1[Suits.Hearts, Ranks.Jack]);
            Assert.AreEqual<int>(35, target2[Suits.Hearts, Ranks.Jack]);
        }

        [TestMethod, TestCategory("CI"), TestCategory("Other")]
        public void SuitRankCollection_Performs()
        {
            int loopSize = 1000000;
            int newValue = 13;

            var target3 = new SuitRankCollection<byte>();
            byte newValue3 = 13;
            // warm-up
            for (int i = 0; i < 100; i++)
            {
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        target3[s, r] = newValue3;
                    }
                }
            }

            var t7 = ElapsedTime.Do(() =>
            {
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        target3[s, r] = newValue3;
                    }
                }
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        var y = target3[s, r];
                    }
                }
            }, loopSize);
            Trace2("SuitRankCollection<byte>: read/write[suit, rank] int: {0}", t7);
            Assert.IsTrue(t7 < 0.00001);

            var t8 = ElapsedTime.Do(() =>
            {
                for (int s = 0; s <= 3; s++)
                {
                    for (int r = 0; r <= 12; r++)
                    {
                        target3[s, r] = newValue3;
                    }
                }
                for (int s = 0; s <= 3; s++)
                {
                    for (int r = 0; r <= 12; r++)
                    {
                        var y = target3[s, r];
                    }
                }
            }, loopSize);
            Trace2("SuitRankCollection<byte>: read/write[int, int] int: {0}", t8);

            var t9 = ElapsedTime.Do(() =>
            {
                var x = target3.Clone();
            }, loopSize);
            Trace2("SuitRankCollection<byte>: Clone: {0}", t9);
            Assert.IsTrue(t9 < 0.000001);

            var target = new SuitRankCollectionInt();
            //Assert.AreEqual<int>(104, Marshal.SizeOf(target), "size");

            // warm-up
            for (int i = 0; i < 100; i++)
            {
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        target[s, r] = newValue;
                    }
                }
            }

            var t1 = ElapsedTime.Do(() =>
            {
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        target[s, r] = newValue;
                    }
                }
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        var y = target[s, r];
                    }
                }
            }, loopSize);
            Trace2("SuitRankCollectionInt: read/write[suit, rank] int: {0}", t1);
            Assert.IsTrue(t1 < 0.00001);

            var t2 = ElapsedTime.Do(() =>
            {
                for (int s = 0; s <= 3; s++)
                {
                    for (int r = 0; r <= 12; r++)
                    {
                        target[s, r] = newValue;
                    }
                }
                for (int s = 0; s <= 3; s++)
                {
                    for (int r = 0; r <= 12; r++)
                    {
                        var y = target[s, r];
                    }
                }
            }, loopSize);
            Trace2("SuitRankCollectionInt: read/write[int, int] int: {0}", t2);
            //Assert.IsTrue(t2 < t1);

            var t3 = ElapsedTime.Do(() =>
            {
                var x = target.Clone();
            }, loopSize);
            Trace2("SuitRankCollectionInt: Clone: {0}", t3);
            Assert.IsTrue(t3 < 0.000001);

            var target2 = new SuitRankCollection<int>();
            // warm-up
            for (int i = 0; i < 100; i++)
            {
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        target2[s, r] = newValue;
                    }
                }
            }

            var t4 = ElapsedTime.Do(() =>
            {
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        target2[s, r] = newValue;
                    }
                }
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        var y = target2[s, r];
                    }
                }
            }, loopSize);
            Trace2("SuitRankCollection<int>: read/write[suit, rank] int: {0}", t4);
            Assert.IsTrue(t4 < 0.00001);

            var t5 = ElapsedTime.Do(() =>
            {
                for (int s = 0; s <= 3; s++)
                {
                    for (int r = 0; r <= 12; r++)
                    {
                        target2[s, r] = newValue;
                    }
                }
                for (int s = 0; s <= 3; s++)
                {
                    for (int r = 0; r <= 12; r++)
                    {
                        var y = target2[s, r];
                    }
                }
            }, loopSize);
            Trace2("SuitRankCollection<int>: read/write[int, int] int: {0}", t5);
            //Assert.IsTrue(t5 < t4);

            var t6 = ElapsedTime.Do(() =>
            {
                var x = target2.Clone();
            }, loopSize);
            Trace2("SuitRankCollection<int>: Clone: {0}", t6);
            Assert.IsTrue(t6 < 0.000001);
        }

        [TestMethod, TestCategory("CI"), TestCategory("Other")]
        public void IntCalculations_Performs()
        {
            int loopSize = 10000000;

            var t1 = ElapsedTime.Do(() =>
            {
                int x = 34;
                int y = 5;
                int z = 13;
                int a = x * y + z;
            }, loopSize);
            Trace.WriteLine(string.Format("int: {0}", t1));

            var t2 = ElapsedTime.Do(() =>
            {
                byte x = 34;
                byte y = 5;
                byte z = 13;
                byte a = (byte)(x * y + z);
            }, loopSize);
            Trace.WriteLine(string.Format("byte: {0}", t2));
        }

        private void Trace2(string format, params object[] args)
        {
            Trace.WriteLine(string.Format(format, args));
        }
    }
}
