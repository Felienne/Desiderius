using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodes.Base.Test.Helpers;

namespace Sodes.Bridge.Base.Test.Helpers
{
    public class BridgeTestBase : TestBase
    {
        public static void Initialize(TestContext testContext)
        {
            TestDeployment.Init(testContext);
            //WinFormConventionCards.Init();
            //WinFormCache.Init();
            //WinFormMemory.Init();
        }

        public static void Cleanup()
        {
            TestDeployment.Cleanup();
            //WinFormConventionCards.Cleanup();
            //WinFormCache.Cleanup();
        }
    }
}
