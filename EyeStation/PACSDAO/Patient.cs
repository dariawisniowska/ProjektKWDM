using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeStation.PACSDAO
{
    public class Patient
    {
        public string patientID;
        public string patientName;
        public string name;
        public Dictionary<string, Dictionary<string, string>> datas;
        public string path;
        public string segmentation_name;

        public Patient(string patientID, string patientName, string name, string path, Dictionary<string, Dictionary<string, string>> datas, string segmentation_name)
        {
            this.path = path;
            this.patientID = patientID;
            this.name = name;
            this.patientName = patientName;
            this.datas = datas;
            this.segmentation_name = segmentation_name;
        }
    }

    public class PatientDataReader
    {
        public string PatientName;
        public string PatientID;

        private PatientDataReader(string patientName, string patientID)
        {
            this.PatientName = patientName;
            this.PatientID = patientID;
        }

        public PatientDataReader(string dataElement)
        {
            PatientDataReader de = Read(dataElement);
            this.PatientName = de.PatientName;
            this.PatientID = de.PatientID;
        }

        public static PatientDataReader Read(string dataElement)
        {
            string patientID = "No data";
            string patientName = "";
            string[] data = dataElement.Split('\n');
            foreach (string d in data)
            {
                string[] elements = d.Split('\t');
                switch (elements[0])
                {
                    case "(0010,0020)":
                        patientID = elements[elements.Length-1];
                        break;
                    case "(0010,0010)":
                        patientName = elements[elements.Length - 1];
                        break;
                }
            }
            return new PatientDataReader(patientName, patientID);
        }
    }
}
