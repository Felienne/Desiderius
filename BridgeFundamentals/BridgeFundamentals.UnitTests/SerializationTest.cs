using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sodes.Bridge.Base.Test
{
  [TestClass]
  public class SerializationTest
  {
    [TestMethod, TestCategory("CI"), TestCategory("Other")]
    public void Board_SerializeTest()
    {
      Board2 board = new Board2(@"South, All
         S T87432
         H 53
         D 72
         C KT6
S QJ965           S AK
H JT96            H AQ8
D 9               D KJ865
C 973             C AQ4
         S 
         H K742
         D AQT43
         C J852
");
      var newResult = new BoardResult("", board, new Participant("N", "E", "S", "W"));
      newResult.Auction = new Sodes.Bridge.Base.Auction(newResult);
      newResult.Auction.Record(Bid.C("3NT"));
      newResult.Auction.Record(Bid.C("p"));
      newResult.Auction.Record(Bid.C("p"));
      newResult.Auction.Record(Bid.C("p"));
      newResult.Play = new Sodes.Bridge.Base.PlaySequence(new Sodes.Bridge.Base.Contract("3NT", Seats.South, board.Vulnerable), 13);
      newResult.Play.Record(Suits.Clubs, Ranks.Seven);
      newResult.Play.Record(Suits.Clubs, Ranks.Eight);
      newResult.Play.Record(Suits.Clubs, Ranks.Queen);
      newResult.Play.Record(Suits.Clubs, Ranks.Ace);
      newResult.Play.Record(Suits.Diamonds, Ranks.Ace);
      newResult.Play.Record(Suits.Diamonds, Ranks.Seven);
      newResult.Play.Record(Suits.Diamonds, Ranks.Two);
      newResult.Play.Record(Suits.Diamonds, Ranks.Four);
      newResult.Play.Record(Suits.Diamonds, Ranks.King);
      newResult.Play.Record(Suits.Diamonds, Ranks.Queen);
      newResult.Play.Record(Suits.Diamonds, Ranks.Three);
      newResult.Play.Record(Suits.Diamonds, Ranks.Five);
      board.Results.Add(newResult);

      var serializer = new DataContractSerializer(board.GetType());
      string serializedBoard = "";
      using (var sw = new StringWriter())
      {
        using (var xw = new XmlTextWriter(sw))
        {
          serializer.WriteObject(xw, board);
        }
        serializedBoard = sw.ToString();
      }

      Debug.WriteLine(serializedBoard);
      Assert.IsNotNull(serializedBoard);

      Board2 copy = null;
      using (var sr = new StringReader(serializedBoard))
      {
        using (var xr = new XmlTextReader(sr))
        {
          copy = (Board2)serializer.ReadObject(xr);
        }
      }

      Assert.AreEqual(board, copy, "board");

      var serializer2 = new DataContractSerializer(newResult.GetType());
      string serializedResult = "";
      using (var sw = new StringWriter())
      {
        using (var xw = new XmlTextWriter(sw))
        {
          serializer2.WriteObject(xw, newResult);
        }
        serializedResult = sw.ToString();
      }

      BoardResult resultCopy = null;
      using (var sr = new StringReader(serializedResult))
      {
        using (var xr = new XmlTextReader(sr))
        {
          resultCopy = (BoardResult)serializer2.ReadObject(xr);
        }
      }

      resultCopy.Board = board;
      Assert.AreEqual<BoardResult>(newResult, resultCopy);
    }
  }
}
