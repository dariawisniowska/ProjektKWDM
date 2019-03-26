using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeStation.PACS
{
    public class Patient
    {
        public string patientID;
        public string patientName;
        public List<string> imagesNames;
        public Dictionary<string, System.Drawing.Image> images;
        public Dictionary<string, Dictionary<string, string>> datas;
        public string path;

        public Patient(string patientID, string patientName, List<string> imagesNames, Dictionary<string, System.Drawing.Image> images, string path, Dictionary<string, Dictionary<string, string>> datas)
        {
            this.path = path;
            this.patientID = patientID;
            this.patientName = patientName;
            this.images = images;
            this.imagesNames = imagesNames;
            this.datas = datas;
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
                        patientID = elements[3];
                        break;
                    case "(0010,0010)":
                        patientName = elements[3];
                        break;
                }
            }
            return new PatientDataReader(patientName, patientID);
        }
    }
}
