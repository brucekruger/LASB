using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blobs_01_ConnectTo
    {
        private static TestContext _Context = null;
        private static string saName = null;
        private static string saKey = null;

        [ClassInitialize]
        public static void Class_Init(TestContext ctx)
        {
            _Context = ctx;

            saName = ConfigurationProvider.saName;
            saKey = ConfigurationProvider.saKey;
        }

        [TestMethod]
        public async Task Test_01_ConnectTo_DevelopmentStorage()
        {
            var dsaConfig = CloudStorageAccount.DevelopmentStorageAccount;
            var client = dsaConfig.CreateCloudBlobClient();

            var myFirstContainerName = "container-one";
            var container = client.GetContainerReference(myFirstContainerName);
            await container.CreateIfNotExistsAsync();
        }

        [TestMethod]
        public async Task Test_02_ConnectTo_Azure()
        {
            var saCreds = new StorageCredentials(saName, saKey);
            var saConfig = new CloudStorageAccount(saCreds, true);

            var client = saConfig.CreateCloudBlobClient();

            var myFirstContainerName = "container-one";
            var container = client.GetContainerReference(myFirstContainerName);
            await container.CreateIfNotExistsAsync();
        }
    }
}
