
namespace Sodes.Bridge.Networking
{
	public enum TableManagerProtocolState
	{
		Initial
		, WaitForSeated
		, WaitForTeams
		, WaitForStartOfBoard
		, WaitForBoardInfo
		, WaitForMyCards
		, WaitForOwnBid
		, WaitForCardPlay
		, WaitForOtherBid
		, WaitForOtherCardPlay
		, WaitForOwnCardPlay
		, WaitForDummiesCardPlay
		, WaitForDummiesCards
        , GiveDummiesCards
		, WaitForDisconnect
		, WaitForLead
		, Finished
	}
}
