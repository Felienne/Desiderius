using System;
using System.Collections.ObjectModel;

namespace Sodes.Bridge.Base
{
	public class ConventionCard
	{
		public ConventionCard(string cardName, string baseCard)
		{
			this.CardName = cardName;
			this.BaseCard = baseCard;
			this.Conventions = new Collection<Conventies>();
		}

		public ConventionCard()
		{
		}

		public string CardName { get; private set; }

		public string BaseCard { get; private set; }

		public Collection<Conventies> Conventions { get; private set; }
	}

	public class ChampionshipCard
	{
		public string Name { get; set; }
		public DateTime Released { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] Card { get; set; }
	}
}
