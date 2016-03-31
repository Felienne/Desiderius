
namespace Sodes.Bridge.Base
{
	/// <summary>Functional different versions of the program</summary>
	public enum ProgramType
	{
		/// <summary>Regular contract bridge</summary>
		RegularBridge,

		/// <summary>Kids version of bridge (without bidding)</summary>
		MiniBridge
	}

	public static class ProgramTypes
	{
		/// <summary>What type of bridge program is running?</summary>
		public static ProgramType Running { get; set; }
	}
}
