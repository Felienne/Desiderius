
namespace Sodes.Base
{
	public abstract class Memory
	{
		public abstract void AssertAvailable(int megaBytes);

        public static Memory Instance { get; protected set; }
	}
}
