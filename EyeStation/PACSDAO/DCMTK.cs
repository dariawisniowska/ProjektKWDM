using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeStation.PACSDAO
{
    public static class DCMTK
    {
        public static void DCM2JPG(List<string> exList)
        {
            Process exeProcess = new Process();
            foreach (string ex in exList)
            {
                // Use ProcessStartInfo class
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "dcm2jpg.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = "-f j -o " + ex + ".jpg -z 1.0 -s y " + ex + ".dcm";

                exeProcess.StartInfo = startInfo;
                exeProcess.Start();
                exeProcess.WaitForExit();
            }
        }
    }
}
