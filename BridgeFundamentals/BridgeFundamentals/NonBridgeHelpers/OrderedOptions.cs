
namespace Sodes.Base
{
	public delegate bool ReadyDelegate();
	public delegate void Option();

	public static class OrderedOptions
	{
		public static void ConsiderOrderedOptions(Option[] options, ReadyDelegate ready)
		{
			for (int option = 0; option < options.Length; option++)
			{
				options[option]();
				if (ready()) return;
			}
		}
	}
}
