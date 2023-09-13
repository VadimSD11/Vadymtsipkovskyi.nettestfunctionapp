using System;
using System.IO;
using Azure.Storage.Blobs;
using blobfunction;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace YourNamespace.Tests
{
    [TestClass]
    public class BlobTriggerFunctionTests
    {
        [TestMethod]
        public void BlobTriggerFunction_Run_Successfully()
        {
            // Arrange
            var logMock = new Mock<ILogger>();
            var name = "testfile.txt";
            var recipientEmail = "recipient@example.com";
            var myBlob = new MemoryStream(new byte[1024]); 

        
            var connectionString = "YourConnectionStringHere"; 
            var containerName = "vadymtsipkovskyinettest";
            var blobContainerClientMock = new Mock<BlobContainerClient>(connectionString, containerName);

          
            BlobTriggerFunction.Run(myBlob, name, recipientEmail, logMock.Object);

            logMock.Verify(mock => mock.LogInformation(It.Is<string>(s => s.Contains(name) && s.Contains(myBlob.Length.ToString()))), Times.Once);

        }

        [TestMethod]
        public void BlobTriggerFunction_Run_With_Null_Stream()
        {
            // Arrange
            var logMock = new Mock<ILogger>();
            var name = "testfile.txt";
            var recipientEmail = "recipient@example.com";
            Stream myBlob = null; 

            // Mock the BlobContainerClient
            var connectionString = "YourConnectionStringHere"; 
            var containerName = "vadymtsipkovskyinettest";
            var blobContainerClientMock = new Mock<BlobContainerClient>(connectionString, containerName);

       
            var blobClientMock = new Mock<BlobClient>();
            blobContainerClientMock.Setup(x => x.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);

            Assert.ThrowsException<NullReferenceException>(() => BlobTriggerFunction.Run(myBlob, name, recipientEmail, logMock.Object));

        }
    }
}
