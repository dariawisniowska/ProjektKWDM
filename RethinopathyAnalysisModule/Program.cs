using libsvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RethinopathyAnalysisModule
{
    class Program
    {
        private const string LEU_TEST_FILE = @"leu.ds";

        static double C = 0.8;
        static double gamma = 0.000030518125;

        static svm_problem _prob;

        static void Main(string[] args)
        {
            var path = Environment.CurrentDirectory;
            string fullPath = System.IO.Path.Combine(path, LEU_TEST_FILE);

            // get data from file
            // Note that you should always scale your data
            _prob = ProblemHelper.ReadAndScaleProblem(fullPath);

            var svm = new C_SVC(_prob, KernelHelper.RadialBasisFunctionKernel(gamma), C);
            var cva = svm.GetCrossValidationAccuracy(5);

            Console.WriteLine(String.Format("Result: {0}", cva.ToString()));
            Console.ReadKey();
        }
    }
}
