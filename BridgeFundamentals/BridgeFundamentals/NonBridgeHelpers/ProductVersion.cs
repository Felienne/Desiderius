
namespace Sodes.Base
{
	public abstract class ProductVersion
	{
		public abstract string Version { get; }

		public static ProductVersion Instance { get; protected set; }
	}
}
