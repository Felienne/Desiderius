using System.Collections;

namespace Sodes.Base
{
	public static class BitArrayExtension
	{
		public static BitArray Clone(this BitArray b)
		{
			return new BitArray(b);
		}
	}
}
