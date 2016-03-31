using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Sodes.Base.Test.Helpers
{
    public class TestDeployment : Deployment
    {
        public static void Init(TestContext testContext)
        {
            Instance = new TestDeployment();
            Instance.ConventionCards = testContext.DeploymentDirectory;
            Instance.OfflineTournaments = "c:\\";		// should not be used in tests
            //if (!System.IO.Directory.Exists(Instance.OfflineTournaments)) System.IO.Directory.CreateDirectory(Instance.OfflineTournaments);
        }

        public static void Cleanup()
        {
            Instance = null;
        }

        public override string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }

        public override void EnsureSufficientExecutionStack2()
        {
        }
    }
}
