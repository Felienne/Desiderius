
namespace Sodes.Base
{
	/// <summary>
	/// Data that will be stored on the device
	/// </summary>
	public abstract class LocalData
	{
		public abstract void Save<T>(string name, T value);

		public abstract T Load<T>(string name);

		public static LocalData Instance { get; protected set; }
	}
}
