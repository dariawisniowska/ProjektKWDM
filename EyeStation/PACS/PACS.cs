using EyeStation.PACS;
using gdcm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EyeStation
{
    public class PACSObj
    {
        private string ip;
        private ushort port;
        private string aet;
        private string call;
        private Dictionary<string, System.Drawing.Image> Images = new Dictionary<string, System.Drawing.Image>();
        private Dictionary<string, Dictionary<string, string>> Datas = new Dictionary<string, Dictionary<string, string>>();
        private List<string> ImageNames = new List<string>();
        private string dane;
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
                return CompositeNetworkFunctions.CEcho(this.ip, this.port, this.aet, this.call);

            }
            catch
            {
                return false;
            }
        }

        public List<PACS.Patient> GetData()
        {
            List<PACS.Patient> data = new List<EyeStation.PACS.Patient>();
            gdcm.DataSetArrayType wynik = PatientQuery();
            // pokaż wyniki
            foreach (gdcm.DataSet x in wynik)
            {
                EyeStation.PACS.PatientDataReader de = new EyeStation.PACS.PatientDataReader(x.toString());
                                
                FramesQuery(de.PatientID);

                data.Add(new PACS.Patient(de.PatientID, de.PatientName, ImageNames, Images, dane, Datas));
            }
            return data;
        }

        private void FramesQuery(string PatientId)
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
            bool stan = gdcm.CompositeNetworkFunctions.CMove(ip, port, zapytanie, port, aet, call, dane);

            // sprawdź stan
            if (!stan)
            {
                MessageBox.Show("PACS server doesn't work.", "Error");
            }
            // skasowanie listy zdjęć
            Images.Clear();
            ImageNames.Clear();
            Datas.Clear();
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


                // przekonwertuj na "znany format"
                gdcm.Bitmap bmjpeg2000 = PACS.ImageConverter.pxmap2jpeg2000(reader.GetPixmap());
                // przekonwertuj na .NET bitmapy
                System.Drawing.Bitmap[] X = PACS.ImageConverter.gdcmBitmap2Bitmap(bmjpeg2000);

                // zapisz
                for (int i = 0; i < X.Length; i++)
                {
                    String name = "";
                    if (X.Length > 1)
                    {
                        name = String.Format("{0}_slice{1}.jpg", plik, i);
                        Images.Add(String.Format("{0}_slice{1}", id[id.Length - 1], i), X[i]);
                        ImageNames.Add(String.Format("{0}_slice{1}", id[id.Length - 1], i));
                        Datas.Add(String.Format("{0}_slice{1}", id[id.Length - 1], i), dataValues);
                    }
                    else
                    {
                        name = String.Format("{0}.jpg", plik);
                        Images.Add(String.Format("{0}", id[id.Length - 1]), X[i]);
                        ImageNames.Add(String.Format("{0}", id[id.Length - 1]));
                        Datas.Add(String.Format("{0}", id[id.Length - 1]), dataValues);
                    }

                    X[i].Save(name);


                }
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
            //gdcm.Tag tag = new gdcm.Tag(0x0010, 0x0010);
            gdcm.KeyValuePairType klucz1 = new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), "*");
            klucze.Add(klucz1);
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
