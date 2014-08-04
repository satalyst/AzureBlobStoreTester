using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStoreTester
{
    public class TestExecutor
    {
        private readonly Random _random;
        private readonly TestParameters _parameters;

        public TestExecutor(TestParameters p)
        {
            if (null == p)
            {
                throw new ArgumentException("Cannot create a TestExecution instance with a null TestParameters instance.");
            }

            _random = new Random();
            _parameters = p;
        }


        public void Run()
        {
            var container = GetOrCreateCloudBlobContainer();
            _parameters.PrintSettings(Console.Out);

            Console.WriteLine("RUN_NUMBER, UPLOAD_START_TIME, UPLOAD_END_TIME, UPLOAD_DURATION(ms), DOWNLOAD_START_TIME, DOWNLOAD_END_TIME, DOWNLOAD_DURATION(ms), VERIFIED, FILE_SIZE(bytes)");

            RunTests(container);
        }

        private void RunTests(CloudBlobContainer container)
        {
            long testsPerThread = _parameters.TestCount / _parameters.ThreadCount;
            
            CountdownLatch latch = new CountdownLatch((int)_parameters.ThreadCount);

            for (long i = 0; i < _parameters.ThreadCount; i++)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += bw_DoWork;

                BackgroundWorkerParams arg = new BackgroundWorkerParams
                {
                    Container = container, 
                    Latch = latch,
                    TestCount = testsPerThread,
                    Offset = i * testsPerThread
                };

                worker.RunWorkerAsync(arg);
            }

            latch.Wait();
        }


        private void bw_DoWork(object sender, DoWorkEventArgs eventArgs)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            BackgroundWorkerParams para = eventArgs.Argument as BackgroundWorkerParams;

            try
            {
                for (long i = 0; i < para.TestCount; i++)
                {
                    if (worker.CancellationPending)
                    {
                        eventArgs.Cancel = true;
                        para.Latch.Signal();
                        return;
                    }

                    string blobId = Guid.NewGuid().ToString();

                    try
                    {
                        TestResult result = PerformOneTest(para.Container, blobId, para.Offset + i);
                        result.PrintResults(Console.Out);

                        // if this isn't the last test we want to make sure that we sleep for whatever delay has been specified.
                        if (i < para.TestCount)
                        {
                            Thread.Sleep(new TimeSpan(0, 0, 0, 0, _parameters.TestDelay));
                        }
                    }
                    finally
                    {
                        // try and delete the blob, even though something has gone wrong...
                        para.Container.GetBlockBlobReference(blobId).DeleteIfExists();
                    }
                }
            }
            finally
            {
                para.Latch.Signal();
            }
        }


        private long GetFileSize()
        {
            return (long)Math.Ceiling(_random.NextDouble() * (double)((_parameters.RandomMaxSize - _parameters.RandomMinSize))) +
                                     _parameters.RandomMinSize;
        }

        private TestResult PerformOneTest(CloudBlobContainer container, string blobId, long i)
        {
            TestResult result = new TestResult
            {
                FileSize = GetFileSize(),
                RunNumber = i,
            };

            result.StartUpload = DateTime.Now;
            result.UploadHash = UploadData(container, blobId, result.FileSize);
            result.EndUpload = DateTime.Now;

            if (_parameters.VerifyUpload)
            {
                result.StartDownload = DateTime.Now;
                result.DownloadHash = DownloadData(container, blobId);
                result.EndDownload = DateTime.Now;
            }

            return result;
        }


        private byte[] UploadData(CloudBlobContainer container, string name, long size)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(name);

            using (HashAlgorithm hash = new SHA256Cng())
            {
                using (var stream = new CryptoStream(new RandomStream(size), hash, CryptoStreamMode.Read))
                {
                    blob.UploadFromStream(stream);
                }

                return hash.Hash;
            }
        }

        private byte[] DownloadData(CloudBlobContainer container, string name)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(name);

            using (HashAlgorithm hash = new SHA256Cng())
            {
                using (var stream = new CryptoStream(new MemoryStream(), hash, CryptoStreamMode.Write))
                {
                    blob.DownloadToStream(stream);
                }

                return hash.Hash;
            }
        }

        private CloudBlobContainer GetOrCreateCloudBlobContainer()
        {
            CloudStorageAccount account =
                new CloudStorageAccount(
                    new StorageCredentials(
                        _parameters.StorageAccountName,
                        _parameters.StorageAccountKey),
                    _parameters.UseHttps);

            CloudBlobClient client = account.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(_parameters.ContainerName);
            container.CreateIfNotExists();

            return container;
        }

        private class BackgroundWorkerParams
        {
            public long TestCount { get; set; }
            public CloudBlobContainer Container { get; set; }
            public CountdownLatch Latch { get; set; }
            public long Offset { get; set; }
        }
    }

}