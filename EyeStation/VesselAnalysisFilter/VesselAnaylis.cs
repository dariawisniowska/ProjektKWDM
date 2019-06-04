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
                    var DvCsvm = new C_SVC(System.IO.Path.Combine(path, DvC_MODEL_FILE));
                    var DvHsvm = new C_SVC(System.IO.Path.Combine(path, DvH_MODEL_FILE));
                    var HvCsvm = new C_SVC(System.IO.Path.Combine(path, HvC_MODEL_FILE));

                    svm_node[] x = new svm_node[lengths.Count];
                    for (int j = 0; j < lengths.Count; j++) 
                    {
                        x[j] = new svm_node() { index = j, value = lengths[j] };
                    }
                    double DvCresult = DvCsvm.Predict(x);
                    double DvHresult = DvCsvm.Predict(x);
                    double HvCresult = DvCsvm.Predict(x);
                    
                    if(DvCresult==1)
                    {
                        if (DvHresult == 1)
                            return 1;
                    }
                    else
                    {
                        if (HvCresult==-1)
                            return 0;
                        else
                            if (DvHresult == -1)
                                return 2;
                    }
                    return -1;
                }
                else
                    return -1;
            }
            catch
            {
                return -1;
            }
        }

    }
}
