using System;
using System.Collections.Generic;
using System.Text;

namespace Sodes.Base
{
  public abstract class Combinatorial
  {
    private static long[,] combResult;

    private static void CalcCombinations()
    {
      combResult = new long[53, 53];
      // After this call:
      //   Combinatorial[n,m] = n! / (m! * (n-m)!), for each m <= n <= 52
      combResult[0,0] = 1;
      for (int n = 1; n <= 52; n++)
      {
        combResult[n, 0] = 1;
        combResult[n, n] = 1;
        for (int m = 1; m < n; m++)
        {
          combResult[n, m] = combResult[n - 1, m - 1] + combResult[n - 1, m];
        }
      }
    }

    public static long Combinations(int n, int k)
    {
      if (n < 0 || n > 52 || k < 0 || k > 52)
      {
#if DEBUG
				if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#endif
        throw new ArgumentOutOfRangeException("n");
      }

      if (combResult == null) CalcCombinations();
      return combResult[n, k];
    }
  }
}
/*
http://www.durangobill.com/Bridge.html explains combinatorics for bridge

 
unit CountDealsUnit;
// By Jeroen Warmerdam, Sept 2003

interface
uses
  BigNumbers;

type
  THCP = 0..40;
  THighCard = (A, K, Q, J);
var
  NbDealsCardEW,
  NbDealsCardEast: array[THCP, THCP, THighCard] of BigNumber;
  // NbDealsCardEW[15, 10, J] is the number of ways 26 cards of 52
  //   can be given to West and East such that West has 15 HCP, East has 10 HCP
  //   and one of them has spade jack.
  // NbDealsCardEast[15, 10, J] is the number of ways 26 cards of 52
  //   can be given to West and East such that West has 15 HCP, East has 10 HCP
  //   and East has Spade jack.

  // > Perhaps one of the mathematicians out there can help with
  // > this problem in bridge probability theory.  Suppose you
  // > select a random bridge deal with these conditions:
  // >
  // >   West has X HCP
  // >   East has Y HCP
  // >   East-West have card Z (some specific high card)
  // >
  // > What is the probability that East holds card Z?

  // NbDealsCardEast[X, Y, Z] / NbDealsCardEW[X, Y, Z]


procedure Count;

implementation

var
  HCWest, HCEast: array[THighCard] of byte;
  Combinatorial: array[0..52, 0..52] of BigNumber;

procedure MakeCombinatorial;
// After this call:
//   Combinatorial[n,m] = n! / (m! * (n-m)!), for each m <= n <= 52
var
  n, m: byte;
begin
  Combinatorial[0,0]:= IntToBigNumber(1);;
  for n:= 1 to 52 do
  begin
    Combinatorial[n,0]:= IntToBigNumber(1);
    Combinatorial[n,n]:= IntToBigNumber(1);
    for m:= 1 to n-1 do
      Combinatorial[n, m]:=
        Sum(Combinatorial[n-1, m-1], Combinatorial[n-1, m]);
  end;
end{MakeCombinatorial};

procedure Init;
var
  W, E: THCP;
  C: THighCard;
begin
  for W:= 0 to 40 do
    for E:= 0 to 40 do
      for C:= A to J do
      begin
        NbDealsCardEW[W, E, C]:= IntToBignumber(0);
        NbDealsCardEast[W, E, C]:= IntToBignumber(0);
      end;

  for C:= A to J do
  begin
    HCWest[C]:= 0;
    HCEast[C]:= 0;
  end;
end{Init};

procedure Deal(const Card: THighCard; const Factor: BigNumber);
var
  w, e: byte;
  NewFactor: BigNumber;
  NbWest, NbEast: byte;
  HCPWest, HCPEast: byte;
  NbDealsNow: BigNumber;
  C: THighCard;
begin
  for w:= 0 to 4 do
    for e:= 0 to 4-w do
    begin
      HCWest[Card]:= w;
      HCEast[Card]:= e;
      NewFactor:= Product(Product(Factor, Combinatorial[4,w]), Combinatorial[4-w,e]);
      if Card < J then
        Deal(Succ(Card), NewFactor)
      else
      begin
        NbWest:= HCWest[A] + HCWest[K] + HCWest[Q] + HCWest[J];
        NbEast:= HCEast[A] + HCEast[K] + HCEast[Q] + HCEast[J];

        if (NbWest <= 13) and (NbEast <= 13)then
        begin
          NbDealsNow:= Product(Product(NewFactor,
                                       Combinatorial[36, 13 - NbEast]),
                                       Combinatorial[36-13+NbEast, 13 - NbWest]);

          HCPWest:= 4*HCWest[A] + 3*HCWest[K] + 2*HCWest[Q] + HCWest[J];
          HCPEast:= 4*HCEast[A] + 3*HCEast[K] + 2*HCEast[Q] + HCEast[J];

          for C:= A to J do
          begin
            Add(NbDealsCardEW[HCPWest, HCPEast, C],
                Product(Divide(NbDealsNow, 4), HCWest[C]+HCEast[C]));
            Add(NbDealsCardEast[HCPWest, HCPEast, C],
                Product(Divide(NbDealsNow, 4), HCEast[C]));
          end;
        end;
      end;
    end;
end{Deal};

procedure Count;
begin
  Init;
  Deal(A, IntToBigNumber(1));
end{CountNbDeals};

begin
  MakeCombinatorial;
end.


*/