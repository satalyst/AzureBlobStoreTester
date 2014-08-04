Azure Blob Store Tester
=============

This is a simple command line utility that will upload and optionally download randomly generated files of a specified size to an Azure Blob Storage account. Blobs are automatically deleted once the test has been performed.

Upload and download times will be printed to the console in a CSV format for consumption in Microsoft Excel.


Usage
-----

Example (in powershell, printing to console and file):

AzureBlobStoreTester.exe --count=10 --accountName=_\(blob store account name\)_  --accessKey=_\(blob store access key\)_ --fileSize=5242880 --verify=true --delay=1000 | Tee-Object _\(output_file.csv\)_

This will upload a randonly generated 5 megabyte file 10 times, with a 1 second delay between uploads. It will also verify that the file has been successfully uploaded by downloading the file and comparing the contents using a SHA265 hash sum.


Arguments
---------

--accountName=\(string\) : The name of the blob storage account to use (__mandatory__)

--accessKey=\(string\) : The access key for the blob storage account to use (__mandatory__)

--fileSize=\(n\)\[G|M|K|B\]: The size of the random file to generate. Must end with a G (Gigabyte), M (Megabyte), K (Kilobyte) or B (Byte). (_shortcut for setting --fileSizeMax and --fileSizeMin to the same value._)

--fileSizeMax=\(n\)\[G|M|K|B\]: The maximum size of the random file to generate. Must end with a G (Gigabyte), M (Megabyte), K (Kilobyte) or B (Byte)  (_optional_, defaults to 1024 bytes)

--fileSizeMin=\(n\)\[G|M|K|B\]: The minimum size of the random file to generate. Must end with a G (Gigabyte), M (Megabyte), K (Kilobyte) or B (Byte)  (_optional_, defaults to 1024 bytes)

--count=\(n\): The number of times to perform the test (_optional_, defaults to 1)

--delay=\(n\): The time to wait between runs of the test in milliseconds (_optional_, defaults to 30,000)

--containerName=\(string\) : The name of the container to store the blobs in (_optional_, defaults to "blobtest")

--useHttps=\(true|false\) : Whether to use HTTPS for transfers (_optional_, defaults to true)

--verify=\(true | false\) : Whether to download the file and verify it against the uploaded data. (_optional_, defaults to true)


Sample Output
--------------


With --verify=true --fileSize=1048576B --delay=1000 and --useHttps=true


| RUN_NUMBER | UPLOAD_START_TIME   | UPLOAD_END_TIME     | UPLOAD_TIME(ms) | DOWNLOAD_START_TIME | DOWNLOAD_END_TIME   | DOWNLOAD_TIME(ms) | VERIFIED | FILE_SIZE |
|------------|---------------------|---------------------|------------------|---------------------|---------------------|--------------------|----------|-----------|
| 1          | 02/08/2014 16:46:00 | 02/08/2014 16:46:12 | 950              | 02/08/2014 16:46:12 | 02/08/2014 16:46:14 | 842                | True     | 1048576   |
| 2          | 02/08/2014 16:46:15 | 02/08/2014 16:46:26 | 386              | 02/08/2014 16:46:26 | 02/08/2014 16:46:33 | 413                | True     | 1048576   |
| 3          | 02/08/2014 16:46:34 | 02/08/2014 16:46:45 | 700              | 02/08/2014 16:46:45 | 02/08/2014 16:46:49 | 128                | True     | 1048576   |
| 4          | 02/08/2014 16:46:50 | 02/08/2014 16:47:02 | 409              | 02/08/2014 16:47:02 | 02/08/2014 16:47:06 | 593                | True     | 1048576   |
| 5          | 02/08/2014 16:47:08 | 02/08/2014 16:47:18 | 867              | 02/08/2014 16:47:18 | 02/08/2014 16:47:22 | 905                | True     | 1048576   |



With --verify=false --fileSize=1048576B and --useHttps=true


| RUN_NUMBER | UPLOAD_START_TIME   | UPLOAD_END_TIME     | UPLOAD_TIME(ms) | DOWNLOAD_START_TIME | DOWNLOAD_END_TIME | DOWNLOAD_TIME(ms) | VERIFIED | FILE_SIZE |
|------------|---------------------|---------------------|------------------|---------------------|-------------------|--------------------|----------|-----------|
| 1          | 02/08/2014 16:47:42 | 02/08/2014 16:47:54 | 994              | N/A                 | N/A               | N/A                | N/A      | 1048576   |
| 2          | 02/08/2014 16:47:56 | 02/08/2014 16:48:06 | 514              | N/A                 | N/A               | N/A                | N/A      | 1048576   |
| 3          | 02/08/2014 16:48:08 | 02/08/2014 16:48:18 | 532              | N/A                 | N/A               | N/A                | N/A      | 1048576   |
| 4          | 02/08/2014 16:48:19 | 02/08/2014 16:48:31 | 309              | N/A                 | N/A               | N/A                | N/A      | 1048576   |
| 5          | 02/08/2014 16:48:32 | 02/08/2014 16:48:43 | 663              | N/A                 | N/A               | N/A                | N/A      | 1048576   |





