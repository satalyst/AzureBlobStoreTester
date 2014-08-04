using System;
using System.Collections;
using System.IO;

namespace AzureBlobStoreTester
{
    public class TestResult
    {
        public long RunNumber { get; set; }
        public DateTime StartUpload { get; set; }
        public DateTime EndUpload { get; set; }

        public double UploadDuration
        {
            get { return EndUpload.Subtract(StartUpload).TotalMilliseconds; }
        }

        public byte[] UploadHash { get; set; }

        public DateTime? StartDownload { get; set; }
        public DateTime? EndDownload { get; set; }

        public double DownloadDuration
        {
            get
            {
                if (!IsDownload)
                {
                    throw new InvalidOperationException("Cannot determine DownloadDuration.");
                }

                return EndDownload.Value.Subtract(StartDownload.Value).TotalMilliseconds;
            }
        }

        public byte[] DownloadHash { get; set; }

        public long FileSize { get; set; }

        public bool IsDownload
        {
            get { return StartDownload.HasValue && EndDownload.HasValue; }
        }

        public bool IsVerified
        {
            get
            {
                return IsDownload && StructuralComparisons.StructuralEqualityComparer.Equals(DownloadHash, UploadHash);
            }
        }

        public void PrintResults(TextWriter tw)
        {
            if (IsDownload)
            {
                tw.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", RunNumber, StartUpload, EndUpload,
                    UploadDuration, StartDownload, EndDownload, DownloadDuration, IsVerified, FileSize);
            }
            else
            {
                tw.WriteLine("{0}, {1}, {2}, {3}, N/A, N/A, N/A, N/A, {4}", RunNumber, StartUpload, EndUpload,
                    UploadDuration, FileSize);
            }
        }
    }
}