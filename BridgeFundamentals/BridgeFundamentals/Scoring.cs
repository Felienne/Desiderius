
namespace Sodes.Bridge.Base
{
	/// <summary>Enumeration of all possible scoring methodologies</summary>
	public enum Scorings
	{
		/// <summary>Only for looping purposes</summary>
		scFirst,

		/// <summary>Cavendish scoring</summary>
		scCavendish,

		/// <summary>Chicago scoring</summary>
		scChicago,

		/// <summary>Rubber scoring</summary>
		scRubber,

		/// <summary>European Match Points</summary>
		scEMP,

		/// <summary>IMP scoring used between 1948 and 1960</summary>
		scIMP_1948,

		/// <summary>IMP scoring revised in 1961</summary>
		scIMP_1961,

		/// <summary>current IMP scoring since 1962</summary>
		scIMP,

		/// <summary>Board-A-Match</summary>
		scBAM,

		/// <summary>MatchPoint scoring</summary>
		scMP,

		/// <summary>apply InstantScoreTable</summary>
		scInstant,

		/// <summary>the trick point score is IMPed against the average value of all scores</summary>
		scButler,

		/// <summary>as "Butler", but the 2 extreme scores are not used in computing the average value</summary>
		scButler2,

		/// <summary>the trick point score is IMPed against a datum score determined by experts</summary>
		scExperts,

		/// <summary>the trick point score is IMPed against every other trick point score, and summed</summary>
		scCross,

		/// <summary>value of "Cross" , divided by number of scores</summary>
		scCross1,

		/// <summary>value of "Cross" , divided by number of comparisons</summary>
		scCross2,

		/// <summary>MatchPoints are computed as:  the sum of points, constructed by earning 2 points for each lower score, 1 point for each equal score, and 0 points for each higher score.</summary>
		scMP1,

		/// <summary>MatchPoints are computed as:  the sum of points, constructed by earning 1 point for each lower score, 0.5 points for each equal score, and 0 points for each higher score.</summary>
		scMP2,

		/// <summary>NO bonus of 100 (Doubled) or 200 (Redoubled) for the fourth and each subsequent undertrick, when not vulnerable</summary>
		scOldMP,

		/// <summary>see http://www.gallery.uunet.be/hermandw/bridge/hermtd.html</summary>
		scMitchell2,

		/// <summary>idem</summary>
		scMitchell3,

		/// <summary>idem</summary>
		scMitchell4,

		/// <summary>idem</summary>
		scAscherman,

		/// <summary>idem</summary>
		scBastille,

		/// <summary>?</summary>
		scPairs,

		/// <summary>?</summary>
		scMiniBridge,

		/// <summary>Only for looping purpose</summary>
		scLast
	}

	public static class Scoring
	{
		public static int ToImp(int matchPoints)
		{
			int sign = matchPoints < 0 ? -1 : 1;
			if (matchPoints < 0) matchPoints *= -1;
			
			if (matchPoints < 0020) return 00 * sign;
			if (matchPoints < 0050) return 01 * sign;
			if (matchPoints < 0090) return 02 * sign;
			if (matchPoints < 0130) return 03 * sign;
			if (matchPoints < 0170) return 04 * sign;
			if (matchPoints < 0220) return 05 * sign;
			if (matchPoints < 0270) return 06 * sign;
			if (matchPoints < 0320) return 07 * sign;
			if (matchPoints < 0370) return 08* sign;
			if (matchPoints < 0430) return 09 * sign;
			if (matchPoints < 0500) return 10 * sign;
			if (matchPoints < 0600) return 11 * sign;
			if (matchPoints < 0750) return 12 * sign;
			if (matchPoints < 0900) return 13 * sign;
			if (matchPoints < 1100) return 14 * sign;
			if (matchPoints < 1300) return 15 * sign;
			if (matchPoints < 1500) return 16 * sign;
			if (matchPoints < 1750) return 17 * sign;
			if (matchPoints < 2000) return 18 * sign;
			if (matchPoints < 2250) return 19 * sign;
			if (matchPoints < 2500) return 20 * sign;
			if (matchPoints < 3000) return 21 * sign;
			if (matchPoints < 3500) return 22 * sign;
			if (matchPoints < 4000) return 23 * sign;
			return 24 * sign;
		}

		public static Scorings FromXml(string scoring)
		{
			switch (scoring)
			{
				case "Pairs":
					return Scorings.scPairs;

				default:
					return Scorings.scIMP;
			}
		}
	}
}
