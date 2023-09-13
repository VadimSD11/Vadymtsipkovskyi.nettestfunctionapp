using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System.Net.Mail;
using Azure.Storage;

namespace blobfunction
{
    public static class BlobTriggerFunction
    {
        [FunctionName("BlobTriggerFunction")]
        public static void Run(
            [BlobTrigger("vadymtsipkovskyinettest/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob,
            string name,
            string recipientEmail, // Add recipientEmail as a parameter
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function processed blob\n Name:{name} \n  Size: {myBlob.Length} Bytes");

            // Generate a SAS token for the BLOB with a 1-hour expiration
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var containerClient = new BlobContainerClient(connectionString, "vadymtsipkovskyinettest");
            var blobClient = containerClient.GetBlobClient(name);
            var sharedKeyCredential = new StorageSharedKeyCredential("vadymtsipkovskyinettest", "n0HHuJITXNc+nm1qYIBUUie8car67fEY62hpHsIQeYNy3uaOHATh5JKzRM9NNksjhk/RAQaa9+Ro+AStQxI5GA==");
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1), // 1-hour expiration
                Protocol = SasProtocol.Https
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read); // Adjust permissions as needed

            var sasToken = sasBuilder.ToSasQueryParameters(sharedKeyCredential).ToString();

            // Send an email notification with the SAS token
            SendEmail(name, sasToken, recipientEmail);
        }

        private static void SendEmail(string fileName, string sasToken, string recipientEmail) // Add recipientEmail parameter here
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("VadimSD11@gmail.com", "Your Name");
            var subject = "File Uploaded Successfully";
            var to = new EmailAddress(recipientEmail, "Recipient Name");
            var plainTextContent = $"The file {fileName} has been uploaded to the BLOB storage. You can access it using the following link:\n\n" +
                                   $"https://your-storage-account.blob.core.windows.net/your-container-name/{fileName}?{sasToken}";
            var htmlContent = plainTextContent; // You can customize the HTML email body if needed
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            client.SendEmailAsync(msg).Wait();
        }
    }
}