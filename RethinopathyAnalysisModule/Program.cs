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
        private const string DvC_TEST_FILE = @"DvC.ds";
        private const string DvH_TEST_FILE = @"DvH.ds";
        private const string HvC_TEST_FILE = @"HvC.ds";

        private const string DvC_MODEL_FILE = @"DvCModel";
        private const string DvH_MODEL_FILE = @"DvHModel";
        private const string HvC_MODEL_FILE = @"HvCModel";

        static double C = 0.8;
        static double gamma = 0.000030518125;

        static svm_problem DvC_prob, DvH_prob, HvC_prob;

        static void Main(string[] args)
        {
            var path = Environment.CurrentDirectory;
            string DvCPath = System.IO.Path.Combine(path, DvC_TEST_FILE);
            string DvHPath = System.IO.Path.Combine(path, DvH_TEST_FILE);
            string HvCPath = System.IO.Path.Combine(path, HvC_TEST_FILE);

            DvC_prob = ProblemHelper.ReadAndScaleProblem(DvCPath);
            DvH_prob = ProblemHelper.ReadAndScaleProblem(DvHPath);
            HvC_prob = ProblemHelper.ReadAndScaleProblem(HvCPath);

            var DvCsvm = new C_SVC(DvC_prob, KernelHelper.RadialBasisFunctionKernel(gamma), C);
            var DvHsvm = new C_SVC(DvH_prob, KernelHelper.RadialBasisFunctionKernel(gamma), C);
            var HvCsvm = new C_SVC(HvC_prob, KernelHelper.RadialBasisFunctionKernel(gamma), C);
            
            var DvCcva = DvCsvm.GetCrossValidationAccuracy(5);
            var DvHcva = DvHsvm.GetCrossValidationAccuracy(2);
            var HvCcva = HvCsvm.GetCrossValidationAccuracy(5);

            DvCsvm.Export(System.IO.Path.Combine(path, DvC_MODEL_FILE));
            DvHsvm.Export(System.IO.Path.Combine(path, DvH_MODEL_FILE));
            HvCsvm.Export(System.IO.Path.Combine(path, HvC_MODEL_FILE));

            Console.WriteLine(String.Format("--------------------------"));
            Console.WriteLine(String.Format("DvC Result: {0}%", (Math.Round(DvCcva*100,2)).ToString()));
            Console.WriteLine(String.Format("DvH Result: {0}%", (Math.Round(DvHcva * 100,2)).ToString()));
            Console.WriteLine(String.Format("HvC Result: {0}%", (Math.Round(HvCcva * 100,2)).ToString()));
            Console.WriteLine(String.Format("--------------------------"));

            Console.ReadKey();
        }
    }
}
