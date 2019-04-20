using EyeStation.PACSDAO;
using gdcm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EyeStation.PACSDAO
{
    public class PACSObj
    {
        private string ip;
        private ushort port;
        private string aet;
        private string call;
        public List<PACSDAO.Patient> data = new List<Patient>();

        public PACSObj(string ip, ushort port, string aet, string call)
        {
            this.ip = ip;
            this.port = port;
            this.aet = aet;
            this.call = call;
        }

        public bool Connect()
        {
            try
            {
                bool result = CompositeNetworkFunctions.CEcho(this.ip, this.port, this.aet, this.call);
                if (result) GetData();
                return result;

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(),"Error");
                return false;
            }
        }

        public List<EyeStation.Model.Study> GetStudies()
        {
            List<EyeStation.Model.Study> studies = new List<EyeStation.Model.Study>();
            foreach (Patient p in data)
            {               
                    string path = System.IO.Directory.GetCurrentDirectory() + p.name.Remove(0, 1)+".jpg";
                    string klucz = p.name.Replace("\\", "\\\\");
                    Dictionary<string,string> dict = p.datas[klucz];
                    string desc = dict["(0008,1080)"];
                    if (desc == "0") desc = "-";
                    studies.Add(new EyeStation.Model.Study(p.patientID, p.patientName, desc, path));
            }
            return studies;
        }

        private void GetData()
        {
            gdcm.DataSetArrayType wynik = PatientQuery();
            List<string> ex1 = new List<string>();
            // pokaż wyniki
            foreach (gdcm.DataSet x in wynik)
            {
                EyeStation.PACSDAO.PatientDataReader de = new EyeStation.PACSDAO.PatientDataReader(x.toString());

                string dane;
                string name;
                Dictionary<string, Dictionary<string, string>> Datas;

                FramesQuery(de.PatientID, out dane, out name, out Datas);


                data.Add(new PACSDAO.Patient(de.PatientID, de.PatientName, name, dane, Datas));
                ex1.Add(String.Format("{0}", name));
             
            }
            DCMTK.DCM2JPG(ex1);
        }
      
        private void FramesQuery(string PatientId, out string dane, out string name, out Dictionary<string, Dictionary<string, string>> Datas)
        {

            gdcm.ERootType typ = gdcm.ERootType.ePatientRootType;

            gdcm.EQueryLevel poziom = gdcm.EQueryLevel.ePatient; // zobacz inne 

            gdcm.KeyValuePairArrayType klucze = new gdcm.KeyValuePairArrayType();
            gdcm.KeyValuePairType klucz1 = new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0020), PatientId); // NIE WOLNO TU STOSOWAC *; tutaj PatientID="01"
            klucze.Add(klucz1);

            // skonstruuj zapytanie
            gdcm.BaseRootQuery zapytanie = gdcm.CompositeNetworkFunctions.ConstructQuery(typ, poziom, klucze, true);

            // sprawdź, czy zapytanie spełnia kryteria
            if (!zapytanie.ValidateQuery())
            {
                MessageBox.Show("Wrong frames query.", "Error");
            }

            // przygotuj katalog na wyniki
            String odebrane = System.IO.Path.Combine(".", "temporary");

            if (!System.IO.Directory.Exists(odebrane))
                System.IO.Directory.CreateDirectory(odebrane);

            dane = System.IO.Path.Combine(odebrane, System.IO.Path.GetRandomFileName());  //katalog
            System.IO.Directory.CreateDirectory(dane);

            // wykonaj zapytanie - pobierz do katalogu _dane_
            bool stan = gdcm.CompositeNetworkFunctions.CMove(ip, port, zapytanie, 10104, aet, call, dane);

            // sprawdź stan
            if (!stan)
            {
                MessageBox.Show("PACS server doesn't work.", "Error");
            }
            // skasowanie listy zdjęć
            name = "";
            Datas = new Dictionary<string, Dictionary<string, string>>();

            List<string> pliki = new List<string>(System.IO.Directory.EnumerateFiles(dane));  //nazwy plikow

            foreach (String plik in pliki)
            {
                gdcm.Reader dataReader = new gdcm.Reader();
                dataReader.SetFileName(plik);

                if (!dataReader.Read())
                {
                    continue;
                }
                gdcm.File file = dataReader.GetFile();
                gdcm.DataSet dataSet = file.GetDataSet();
                string data = dataSet.toString();
                gdcm.Global g = gdcm.Global.GetInstance();
                gdcm.Dicts dicts = g.GetDicts();
                gdcm.Dict dict = dicts.GetPublicDict();
                string[] dataArray = dataSet.toString().Split('\n');
                Dictionary<string, string> dataValues = new Dictionary<string, string>();
                String[] id = plik.Split('\\');

                foreach (string s in dataArray)
                {
                    string[] dataArrayRow = s.Split('\t');
                    if (dataArrayRow.Length > 1)
                    {
                        string[] tags = dataArrayRow[0].Remove(0, 1).Remove(dataArrayRow[0].Length - 2, 1).Split(',');

                        //Pobranie nazwy Tagu
                        //gdcm.Tag tag = new gdcm.Tag(Convert.ToUInt16(tags[0]),Convert.ToUInt16(tags[1]));
                        //string dictDorTag = dict.GetKeywordFromTag(tag);
                        //if (dictDorTag != null)
                        //   dataValues.Add(dictDorTag, dataArrayRow[dataArrayRow.Length - 1]);

                        dataValues.Add(dataArrayRow[0], dataArrayRow[dataArrayRow.Length - 1]);

                    }
                }

                // przeczytaj pixele
                gdcm.PixmapReader reader = new gdcm.PixmapReader();
                reader.SetFileName(plik);
                if (!reader.Read())
                {
                    continue;
                }

              
                    name = String.Format("{0}", plik.Substring(0,plik.Length-4));
                    Datas.Add(String.Format("{0}", plik.Substring(0, plik.Length - 4)).Replace("\\","\\\\"), dataValues);
                
                
            }
        }

        private gdcm.DataSetArrayType PatientQuery()
        {
            // typ wyszukiwania (rozpoczynamy od pacjenta)
            gdcm.ERootType typ = gdcm.ERootType.ePatientRootType;

            // do jakiego poziomu wyszukujemy 
            gdcm.EQueryLevel poziom = gdcm.EQueryLevel.ePatient; // zobacz inne 

            // klucze (filtrowanie lub określenie, które dane są potrzebne)
            gdcm.KeyValuePairArrayType klucze = new gdcm.KeyValuePairArrayType();
            klucze.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), "*"));
            klucze.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0020), ""));

            // skonstruuj zapytanie
            gdcm.BaseRootQuery zapytanie = gdcm.CompositeNetworkFunctions.ConstructQuery(typ, poziom, klucze);

            gdcm.DataSetArrayType wynik = new gdcm.DataSetArrayType();

            // sprawdź, czy zapytanie spełnia kryteria
            if (!zapytanie.ValidateQuery())
            {
                MessageBox.Show("Wrong patient query.", "Error");
            }
            else
            {
                // wykonaj zapytanie

                bool stan = gdcm.CompositeNetworkFunctions.CFind(ip, port, zapytanie, wynik,aet,call);

                // sprawdź stan
                if (!stan)
                {
                    MessageBox.Show("PACS server doesn't work.", "Error");
                }
            }
            return wynik;

        }

    }
}
