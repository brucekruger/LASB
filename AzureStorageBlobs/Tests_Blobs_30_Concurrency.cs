using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blobs_30_Concurrency
    {
        private static TestContext _Context = null;
        private static CloudBlobClient _Client = null; 

        [ClassInitialize]
        public static void Class_Init(TestContext ctx)
        {
            _Context = ctx;

            var saName = ConfigurationProvider.saName;
            var saKey = ConfigurationProvider.saKey;
            var saCreds = new StorageCredentials(saName, saKey);
            var saConfig = new CloudStorageAccount(saCreds, true);

            _Client = saConfig.CreateCloudBlobClient();
        }

        [TestMethod]
        public async Task Test_31_Blob_withInfiniteLease()
        {
            CloudBlobContainer cbc = _Client.GetContainerReference("test-append-blob-container");
            await cbc.CreateIfNotExistsAsync();

            CloudAppendBlob blob = cbc.GetAppendBlobReference("test-append-blob-withlease2.data");
            var blobExists = await blob.ExistsAsync();

            if(!blobExists)
            {
                await blob.CreateOrReplaceAsync();
            }

            string data = string.Empty.PadLeft(64 * 1024, '*');

            string leaseIdGuid = Guid.NewGuid().ToString();

            var oc = new OperationContext();
            var ac = new AccessCondition();
            var leaseID = await blob.AcquireLeaseAsync(null, leaseIdGuid, ac, null, null);
            ac.LeaseId = leaseID;

            try
            {
                for (int ix = 0; ix < 10; ix++)
                {
                    await blob.AppendTextAsync(data, null, ac, null, oc);
                }
            }
            finally
            {
                await blob.ReleaseLeaseAsync(ac, null, oc);
            }
        }
    }
}
