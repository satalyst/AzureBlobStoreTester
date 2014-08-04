using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Hosting;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStoreTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TestParameters parameters = TestParameters.Create(args);
            TestExecutor executor = new TestExecutor(parameters);
            executor.Run();
        }
    }
}
