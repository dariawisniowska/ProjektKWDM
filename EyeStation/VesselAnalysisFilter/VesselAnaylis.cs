using System;
using libsvm;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EyeStation.VesselAnalysisFilter
{    
    public class VesselAnaylis
    {
        List<int> lengths;
        private const string DvC_MODEL_FILE = @"DvCModel";
        private const string DvH_MODEL_FILE = @"DvHModel";
        private const string HvC_MODEL_FILE = @"HvCModel";

        private const string TEST_FILE = @"probka.ds";

        public VesselAnaylis(List<int> lengths)
        {
            this.lengths = lengths;
        }

        public int Classify()
        {
            try
            {
                if (lengths != null)
                {
                    var path = Environment.CurrentDirectory.Remove(Environment.CurrentDirectory.Length - 20, 20);
                    path = path + "RethinopathyAnalysisModule\\bin\\Debug\\";
                    SaveTestFile(path);
                    var DvCsvm = new C_SVC(System.IO.Path.Combine(path, DvC_MODEL_FILE));
                    var DvHsvm = new C_SVC(System.IO.Path.Combine(path, DvH_MODEL_FILE));
                    var HvCsvm = new C_SVC(System.IO.Path.Combine(path, HvC_MODEL_FILE));
                    var prob = ProblemHelper.ReadProblem(System.IO.Path.Combine(path, TEST_FILE));
                    DvCsvm.Predict(prob.x[0]);
                    if (path == "")
                        return 1;
                    else
                        return 0;
                }
                else
                    return -1;
            }
            catch
            {
                return -1;
            }
        }

        private string GetTestFileContent()
        {
            string content = "1.000000 ";
            for (int i = 0; i < lengths.Count; i++)
                content += string.Format("{0}: {1} ", i+1, lengths[i]);
            content += "\r\n -1.000000 ";
            for (int i = 0; i < lengths.Count; i++)
                content += string.Format("{0}: {1} ", i + 1, lengths[i]);
            return content;
        }

        private void SaveTestFile(string path)
        {
            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(path, TEST_FILE)))
            {
                sw.Write(GetTestFileContent());
            }
        }
    }
}
