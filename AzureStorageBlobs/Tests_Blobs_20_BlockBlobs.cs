using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blobs_20_BlockBlobs
    {
        private static TestContext _Context = null;
        private static CloudBlobClient _Client = null;

        public string _etag = null;

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
        public async Task Test_20_UploadBlob()
        {
            CloudBlobContainer container = _Client.GetContainerReference("photos");
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blob = container.GetBlockBlobReference("monah.jpg");
            await blob.DeleteIfExistsAsync();

            var localFileName = @"d:\monah.jpg";
            File.Exists(localFileName).Should().BeTrue();

            await blob.UploadFromFileAsync(localFileName);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageException))]
        public async Task Test_21_UploadBlobIfNotExists()
        {
            CloudBlobContainer container = _Client.GetContainerReference("photos");
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blob = container.GetBlockBlobReference("monah.jpg");
            //await blob.DeleteIfExistsAsync();

            var ac = AccessCondition.GenerateIfNoneMatchCondition("*");

            var localFileName = @"d:\monah.jpg";
            File.Exists(localFileName).Should().BeTrue();

            await blob.UploadFromFileAsync(localFileName, ac, null, null);
        }

        [TestMethod]
        public async Task Test_25_BlockBlobUpload()
        {
            await BlockBlobUpload();
        }

        private async Task BlockBlobUpload()
        {
            var container = _Client.GetContainerReference("test-blockblob-blockreference");
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference("myblockblob");
            await blob.DeleteIfExistsAsync();

            var myDataBlockList = new List<string>
            {
                string.Empty.PadRight(50, '*') + Environment.NewLine,
                string.Empty.PadRight(50, '-') + Environment.NewLine,
                string.Empty.PadRight(50, '=') + Environment.NewLine,
                string.Empty.PadRight(50, '-') + Environment.NewLine,
                string.Empty.PadRight(50, '*') + Environment.NewLine
            };

            var blockIDs = new List<string>();

            int id = 0;
            foreach (var myDataBlock in myDataBlockList)
            {
                id++;
                var blockID = Convert.ToBase64String(Encoding.UTF8.GetBytes(id.ToString("d6")));
                var blockData = new MemoryStream(Encoding.UTF8.GetBytes(myDataBlock));

                await blob.PutBlockAsync(blockID, blockData, null);

                blockIDs.Add(blockID);
            }
            await blob.PutBlockListAsync(blockIDs);

            await blob.FetchAttributesAsync();

            _etag = blob.Properties.ETag;

            _etag.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task Test_26_ModifyBlockBlob()
        {
            await ModifyBlockBlobWithoutChecks();
        }

        private static async Task ModifyBlockBlobWithoutChecks()
        {
            var container = _Client.GetContainerReference("test-blockblob-blockreference");
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference("myblockblob");
            bool blobExists = await blob.ExistsAsync();
            blobExists.Should().BeTrue();

            var blockItems = await blob.DownloadBlockListAsync();
            var blockIDs = from blockInfo in blockItems
                           select blockInfo.Name;

            var myNewBlock = string.Empty.PadRight(50, '+') + Environment.NewLine;
            var nBlockID = 3;
            var blockID = Convert.ToBase64String(Encoding.UTF8.GetBytes(nBlockID.ToString("d6")));
            var blockData = new MemoryStream(Encoding.UTF8.GetBytes(myNewBlock));

            blockIDs.Should().Contain(s => s == blockID);

            await blob.PutBlockAsync(blockID, blockData, null);
            await blob.PutBlockListAsync(blockIDs);
        }

        [TestMethod]
        public async Task Test_27_ModifyBlockBlobIfNotModified()
        {
            await BlockBlobUpload();
            //await ModifyBlockBlobWithoutChecks();

            var container = _Client.GetContainerReference("test-blockblob-blockreference");
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference("myblockblob");
            bool blobExists = await blob.ExistsAsync();
            blobExists.Should().BeTrue();

            var blockItems = await blob.DownloadBlockListAsync();
            var blockIDs = from blockInfo in blockItems
                           select blockInfo.Name;

            var myNewBlock = string.Empty.PadRight(50, '+') + Environment.NewLine;
            var nBlockID = 3;
            var blockID = Convert.ToBase64String(Encoding.UTF8.GetBytes(nBlockID.ToString("d6")));
            var blockData = new MemoryStream(Encoding.UTF8.GetBytes(myNewBlock));

            blockIDs.Should().Contain(s => s == blockID);

            var ac = AccessCondition.GenerateIfMatchCondition(_etag);

            await blob.PutBlockAsync(blockID, blockData, null, ac, null, null);
            await blob.PutBlockListAsync(blockIDs, ac, null, null);
        }
    }
}
