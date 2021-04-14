using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blobs_40_SAS
    {
        private static TestContext _Context = null;
        private static CloudBlobClient _Client = null;
        private static string _sasToken_Read = null;

        [ClassInitialize]
        public static void Class_Init(TestContext ctx)
        {
            _Context = ctx;

            var saName = ConfigurationProvider.saName;
            var saKey = ConfigurationProvider.saKey;
            var saCreds = new StorageCredentials(saName, saKey);
            var saConfig = new CloudStorageAccount(saCreds, true);

            _Client = saConfig.CreateCloudBlobClient();

            var cr = _Client.GetContainerReference("photos");
            var br = cr.GetBlobReference("monah.jpg");
            _sasToken_Read = br.GetSharedAccessSignature(
                new SharedAccessBlobPolicy()
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessExpiryTime = new DateTimeOffset(DateTime.Now.AddMinutes(2))
                });
        }

        [TestMethod]
        public async Task Test_40_SAS()
        {
            var blob = new CloudBlob(
                new Uri("https://learnazureblobstoday2021.blob.core.windows.net/photos/monah.jpg"),
                new StorageCredentials(_sasToken_Read)
                );

            var imageStream = new MemoryStream();

            await blob.DownloadToStreamAsync(imageStream);

            imageStream.Length.Should().BeGreaterThan(0);
        }
    }
}
