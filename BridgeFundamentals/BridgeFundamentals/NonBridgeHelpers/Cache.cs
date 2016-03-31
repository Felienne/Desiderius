
namespace Sodes.Base
{
	public abstract class Cache
	{
		public abstract void Add(string key, object x);

		public abstract object Get(string key);

		public static Cache Instance { get; protected set; }
	}
}
