using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using AsciiIt;
using AsciiIt.Contracts;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace AsciiQueueProcessor
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "imageprocessing";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            Client.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        var imageMessage = receivedMessage.GetBody<ImageMessage>();

                        var blobConnectionString = CloudConfigurationManager.GetSetting("BlobStorage.ConnectionString");
                        var storageAccount = CloudStorageAccount.Parse(blobConnectionString);

                        var blobClient = storageAccount.CreateCloudBlobClient();
                        var container = blobClient.GetContainerReference("images");
                        var blobBlock = container.GetBlockBlobReference(imageMessage.BlobBlockName);

                        var stream = new MemoryStream();
                        blobBlock.DownloadToStream(stream);
                        stream.Position = 0;
                        var bitmap = (Bitmap)Image.FromStream(stream);

                        var converterService = new AsciiImageCoverterService();

                        string result = converterService.ConvertImage(bitmap);

                        var convertedContainer = blobClient.GetContainerReference("converted-images");
                        convertedContainer.CreateIfNotExists();
                        convertedContainer.GetBlockBlobReference(imageMessage.BlobBlockName).UploadText(result);
                        
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                    }
                    catch(Exception ex)
                    {
                        // Handle any message processing specific exceptions here
                    }
                });

            CompletedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            // Initialize the connection to Service Bus Queue
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            Client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
