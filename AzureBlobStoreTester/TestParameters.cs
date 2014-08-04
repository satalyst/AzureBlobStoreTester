using System;
using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace AzureBlobStoreTester
{
    public class TestParameters
    {
        private const string FILE_SIZE_ARG = "--fileSize";
        private const string COUNT_ARG = "--count";
        private const string DELAY_ARG = "--delay";
        private const string ACCOUNT_NAME_ARG = "--accountName";
        private const string ACCESS_KEY_ARG = "--accessKey";
        private const string CONTAINER_NAME_ARG = "--containerName";
        private const string USE_HTTPS_ARG = "--useHttps";
        private const string VERIFY_UPLOAD_ARG = "--verify";
        private const string RANDOMISE_MAX_SIZE_ARG = "--fileSizeMax";
        private const string RANDOMISE_MIN_SIZE_ARG = "--fileSizeMin";

        private const string THREAD_COUNT_ARG = "--threadCount";

        private static readonly IDictionary<string, int> FileSizeMultipliers = new Dictionary<string, int>()
        {
            {"G", 1024 * 1024 * 1024},
            {"M", 1024*1024},
            {"K", 1024},
            {"B", 1}
        };

        public int TestCount { get; set; }

        public int TestDelay { get; set; }

        public string StorageAccountName { get; set; }

        public string StorageAccountKey { get; set; }

        public string ContainerName { get; set; }

        public bool UseHttps { get; set; }

        public bool VerifyUpload { get; set; }

        public long RandomMaxSize { get; set; }

        public long RandomMinSize { get; set; }

        public long ThreadCount { get; set; }

        private TestParameters()
        {
            ContainerName = "blobtest";
            UseHttps = true;
            VerifyUpload = true;

            ThreadCount = 1;

            RandomMaxSize = 1024;
            RandomMinSize = 1024;
            TestCount = 1;
            TestDelay = 30000;
        }


        public static TestParameters Create(string[] args)
        {
            var testParameters = new TestParameters();

            foreach (string arg in args)
            {
                string commandName = arg.Split('=')[0];

                switch (commandName)
                {
                    case DELAY_ARG:
                        {
                            testParameters.TestDelay = NumberValue(arg, 1000);
                            break;
                        }

                    case COUNT_ARG:
                        {
                            testParameters.TestCount = NumberValue(arg, 1);
                            break;
                        }

                    case ACCOUNT_NAME_ARG:
                        {
                            testParameters.StorageAccountName = StringValue(arg);
                            break;
                        }

                    case ACCESS_KEY_ARG:
                        {
                            testParameters.StorageAccountKey = StringValue(arg);
                            break;
                        }

                    case CONTAINER_NAME_ARG:
                        {
                            testParameters.ContainerName = StringValue(arg);
                            break;
                        }

                    case USE_HTTPS_ARG:
                        {
                            testParameters.UseHttps = BoolValue(arg);
                            break;
                        }

                    case FILE_SIZE_ARG:
                        {
                            testParameters.RandomMaxSize = BytesValue(arg, 1);
                            testParameters.RandomMinSize = BytesValue(arg, 1);
                            break;
                        }

                    case VERIFY_UPLOAD_ARG:
                        {
                            testParameters.VerifyUpload = BoolValue(arg);
                            break;
                        }

                    case RANDOMISE_MAX_SIZE_ARG:
                        {
                            testParameters.RandomMaxSize = BytesValue(arg, 1);
                            break;
                        }

                    case RANDOMISE_MIN_SIZE_ARG:
                        {
                            testParameters.RandomMinSize = BytesValue(arg, 1);
                            break;
                        }

                    case THREAD_COUNT_ARG:
                        {
                            testParameters.ThreadCount = NumberValue(arg, 1);
                            break;
                        }


                    default:
                        {
                            throw new ArgumentException(string.Format("Unrecognised argument: {0}", arg));
                        }
                }
            }

            if (string.IsNullOrEmpty(testParameters.StorageAccountName))
            {
                throw new ArgumentException(string.Format("{0} must be specified.", ACCOUNT_NAME_ARG));
            }


            if (string.IsNullOrEmpty(testParameters.StorageAccountKey))
            {
                throw new ArgumentException(string.Format("{0} must be specified.", ACCESS_KEY_ARG));
            }

            if (testParameters.RandomMaxSize < testParameters.RandomMinSize)
            {
                throw new ArgumentException(string.Format("Random maximum must be greater than minimum. ({0}, {1})", testParameters.RandomMinSize, testParameters.RandomMaxSize));
            }

            return testParameters;
        }

        private static int NumberValue(string arg, int min)
        {
            string val = StringValue(arg);

            int value;

            if (!int.TryParse(val, out value))
            {
                throw new ArgumentException(string.Format("Could not extract number from argument: {0}", arg));
            }

            if (value < min)
            {
                throw new ArgumentException(string.Format("Could not set value, {0} is less than {1}", value, min));
            }

            return value;
        }

        /// <summary>
        /// A special case of <see cref="NumberValue"/> that accepts a letter at the end of the number indicating the
        /// multiplier to use. Accepts (G, M, K, B) to determine the multiplier value.
        /// </summary>
        private static long BytesValue(string arg, int min)
        {
            string val = StringValue(arg);

            long multiplier = ExtractMultiplierValue(val);
            long numberValue = ExtractSizeValue(val);

            long result = multiplier * numberValue;

            if (result < min)
            {
                throw new ArgumentException(string.Format("Could not set value, {0} is less than {1}", result, min));
            }

            return result;
        }

        private static long ExtractSizeValue(string arg)
        {
            string val = arg.Substring(0, arg.Length - 1);
            long numberValue = 0;

            if (long.TryParse(val, out numberValue))
            {
                return numberValue;
            }
            else
            {
                throw new ArgumentException(string.Format("Could not parse provided file size value {0}.", val));
            }

        }

        private static long ExtractMultiplierValue(string val)
        {
            string suffix = val.ToUpper().Substring(val.Length - 1, 1);

            long multiplier = 0;

            if (!FileSizeMultipliers.ContainsKey(suffix))
            {
                throw new ArgumentException("File Size must end with [G|M|K|B]");
            }
            else
            {
                multiplier = FileSizeMultipliers[suffix];
            }
            return multiplier;
        }

        private static string StringValue(string arg)
        {
            int index = arg.IndexOf("=", StringComparison.Ordinal);
            string val = arg.Substring(index + 1).Trim();

            if (string.IsNullOrEmpty(val))
            {
                throw new ArgumentException(string.Format("Could not extract value from argument: {0}", arg));
            }

            return val;
        }

        private static bool BoolValue(string arg)
        {
            string val = StringValue(arg);

            bool value = false;

            if (bool.TryParse(val, out value))
            {
                return value;
            }

            throw new ArgumentException(string.Format("Could not extract bool from argument: {0}", arg));
        }

        public void PrintSettings(TextWriter txt)
        {
            txt.WriteLine("Account Name: {0}", StorageAccountName);
            txt.WriteLine("Blob Store Container: {0}", ContainerName);
            txt.WriteLine("Number of Test Runs: {0}", TestCount);
            txt.WriteLine("Delay between Test Runs: {0}", TestDelay);

            if (RandomMaxSize == RandomMinSize)
            {
                txt.WriteLine("File Size: {0} bytes.", RandomMaxSize);
            }
            else
            {
                txt.WriteLine("File Size: Max={0} bytes, Min={1} bytes.", RandomMaxSize, RandomMinSize);
            }


            txt.WriteLine("Using HTTPS: {0}", UseHttps);
            txt.WriteLine("Verify Uploaded Data: {0}", VerifyUpload);
            txt.WriteLine("Thread Count: {0}", ThreadCount);
            txt.WriteLine();
            txt.WriteLine();
        }
    }
}