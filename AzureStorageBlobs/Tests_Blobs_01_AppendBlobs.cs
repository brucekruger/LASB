using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blobs_01_AppendBlobs
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
        public async Task Test_10_AppendBlob()
        {
            CloudBlobContainer cbc = _Client.GetContainerReference("test-append-blob-container");
            await cbc.CreateIfNotExistsAsync();

            CloudAppendBlob blob = cbc.GetAppendBlobReference("test-append-blob.data");
            await blob.CreateOrReplaceAsync();

            for(int ix = 0; ix < 100; ix++)
            {
                string txt =
                    $"Line #{ix + 1:d6}, written at {DateTime.UtcNow.ToLongTimeString() }: " +
                    string.Empty.PadRight(20, '*') +
                    Environment.NewLine;

                await blob.AppendTextAsync(txt);
            }
        }
    }
}
