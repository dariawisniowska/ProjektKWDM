using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Words.NET;
using EyeStation.Models;

namespace EyeStation.RaportGenerator
{
    public static class RaportGenerator
    {
        public static void GenerateRaport(Dictionary<string, string> tags, string originalImagePath, List<Marker> markerList, string analysisResult)
        {
            //try
            //{
            System.IO.Directory.CreateDirectory("Raporty");
            string path = @"Raporty/" + tags["(0010,0010)"] + "_" + String.Format("{0:MM/dd/yyyy/HH/mm/ss}", DateTime.Now) + ".docx";

            using (DocX document = DocX.Create(path))
            {
                Paragraph title = document.InsertParagraph();

                Table tTitle = document.AddTable(1, 4);

                tTitle.SetColumnWidth(0, 2000);
                string logoPath = System.IO.Directory.GetCurrentDirectory().Substring(0, System.IO.Directory.GetCurrentDirectory().Length - 10);
                Xceed.Words.NET.Image logo = document.AddImage(logoPath + "\\Images\\icon.png");
                Picture logoImage = logo.CreatePicture();
                logoImage.Height = 60;
                logoImage.Width = 100;
                tTitle.Rows[0].Cells[0].Paragraphs[0].AppendPicture(logoImage);

                tTitle.SetColumnWidth(1, 5000);
                tTitle.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                tTitle.Rows[0].Cells[1].Paragraphs[0].Append("Stacja komputerowego wspomagania diagnostyki medycznej na podstawie obrazów siatkówki oka")
                .Font("Times New Roman")
                .FontSize(14)
                .Color(Color.Black)
                .Bold();

                tTitle.SetColumnWidth(3, 2000);
                tTitle.Rows[0].Cells[3].Paragraphs[0].Append("\r\n"+String.Format("{0:MM.dd.yyyy r.}", DateTime.Now))
                    .Font("Times New Roman")
                    .FontSize(10)
                    .Color(Color.Black);

                title.InsertTableAfterSelf(tTitle);

                Paragraph patientData = document.InsertParagraph();
                patientData.Alignment = Alignment.center;
                patientData.Append("\r\n\r\nDane pacjenta").Font("Times New Roman")
                 .FontSize(12)
                 .Color(Color.Black).Bold(); 

                Paragraph patient = document.InsertParagraph();

                Table tPatient = document.AddTable(4, 2);

                tPatient.SetColumnWidth(0, 2000);
                tPatient.SetColumnWidth(1, 5000);

                tPatient.Rows[0].Cells[0].Paragraphs[0].Append("Imię i nazwisko:")
                    .Font("Times New Roman")
                    .FontSize(10)
                    .Color(Color.Black)
                    .Bold();

                tPatient.Rows[1].Cells[0].Paragraphs[0].Append("Identyfikator:")
                    .Font("Times New Roman")
                    .FontSize(10)
                    .Color(Color.Black).Bold();

                tPatient.Rows[2].Cells[0].Paragraphs[0].Append("Data urodzenia:")
                  .Font("Times New Roman")
                  .FontSize(10)
                  .Color(Color.Black).Bold();

                tPatient.Rows[3].Cells[0].Paragraphs[0].Append("Płeć:")
                   .Font("Times New Roman")
                   .FontSize(10)
                   .Color(Color.Black).Bold();

                tPatient.Rows[1].Cells[1].Paragraphs[0].Append( tags["(0010,0020)"])
                     .Font("Times New Roman")
                     .FontSize(10)
                     .Color(Color.Black);

                tPatient.Rows[0].Cells[1].Paragraphs[0].Append(tags["(0010,0010)"] )
                    .Font("Times New Roman")
                    .FontSize(10)
                    .Color(Color.Black);
                
                patient.InsertTableAfterSelf(tPatient);

                Paragraph studyData = document.InsertParagraph();
                studyData.Alignment = Alignment.center;
                studyData.Append("Opis badania\r\n").Font("Times New Roman")
                     .FontSize(12)
                     .Color(Color.Black).Bold();
              
                Paragraph description = document.InsertParagraph();
                description.Alignment = Alignment.left;
                string desc = tags["(0008,1080)"];
                if (desc == "0") desc = "-";
                description.Append(desc).Font("Times New Roman")
                     .FontSize(10)
                     .Color(Color.Black);

                Paragraph analysisData = document.InsertParagraph();
                analysisData.Alignment = Alignment.center;
                analysisData.Append("Wynik analizy\r\n").Font("Times New Roman")
                     .FontSize(12)
                     .Color(Color.Black).Bold();

                Paragraph anlysisResult = document.InsertParagraph();
                anlysisResult.Alignment = Alignment.left;
                anlysisResult.Append(analysisResult).Font("Times New Roman")
                     .FontSize(10)
                     .Color(Color.Black);

                Paragraph images = document.InsertParagraph();

                Table tImages = document.AddTable(4, 2);
                tImages.SetColumnWidth(0, 5000);
                tImages.SetColumnWidth(1, 5000);

                tImages.Rows[0].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                tImages.Rows[0].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                tImages.Rows[1].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                tImages.Rows[1].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                tImages.Rows[2].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                tImages.Rows[2].Cells[1].Paragraphs[0].Alignment = Alignment.center;
                tImages.Rows[3].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                tImages.Rows[3].Cells[1].Paragraphs[0].Alignment = Alignment.center;

                tImages.Rows[0].Cells[0].Paragraphs[0].Append("\r\nObraz oryginalny")
                       .Font("Times New Roman")
                       .FontSize(10)
                       .Color(Color.Black);
                tImages.Rows[0].Cells[1].Paragraphs[0].Append("\r\nKanał zielony obrazu oryginalnego")
                      .Font("Times New Roman")
                      .FontSize(10)
                      .Color(Color.Black);
                tImages.Rows[2].Cells[0].Paragraphs[0].Append("\r\nWysegmentowane naczynia krwionośne\r\n")
                    .Font("Times New Roman")
                    .FontSize(10)
                    .Color(Color.Black);
                tImages.Rows[2].Cells[1].Paragraphs[0].Append("\r\nSzkielet naczyń krwionośnych\r\n")
                    .Font("Times New Roman")
                    .FontSize(10)
                    .Color(Color.Black);

                Xceed.Words.NET.Image image = document.AddImage(originalImagePath+".jpg");
                Picture originalImage = image.CreatePicture();
                originalImage.Height = 250;
                originalImage.Width = 300;
                tImages.Rows[1].Cells[0].Paragraphs[0].AppendPicture(originalImage);

                Xceed.Words.NET.Image image4 = document.AddImage(originalImagePath + "-1.png");
                Picture greenInameg = image4.CreatePicture();
                greenInameg.Height = 250;
                greenInameg.Width = 300;
                tImages.Rows[1].Cells[1].Paragraphs[0].AppendPicture(greenInameg);

                Xceed.Words.NET.Image image2 = document.AddImage(originalImagePath + "-2.png");
                Picture segmentationImage = image2.CreatePicture();
                segmentationImage.Height = 250;
                segmentationImage.Width = 300;
                tImages.Rows[3].Cells[0].Paragraphs[0].AppendPicture(segmentationImage);

                Xceed.Words.NET.Image image3 = document.AddImage(originalImagePath + "-3.png");
                Picture skelImage = image3.CreatePicture();
                skelImage.Height = 250;
                skelImage.Width = 300;
                tImages.Rows[3].Cells[1].Paragraphs[0].AppendPicture(skelImage);

                images.InsertTableAfterSelf(tImages);

                Paragraph measurments = document.InsertParagraph();

                measurments.Alignment = Alignment.center;
                measurments.Append("Pomiary na obrazie\r\n\r\n").Font("Times New Roman")
                     .FontSize(12)
                     .Color(Color.Black).Bold();

                Xceed.Words.NET.Image image5 = document.AddImage(originalImagePath + "-0.png");
                Picture measurmentsImage = image5.CreatePicture();
                measurmentsImage.Height = 450;
                measurmentsImage.Width = 500;
                measurments.AppendPicture(measurmentsImage);

                int rows = markerList.Count+1;
                Table t = document.AddTable(rows, 2);
                t.SetBorder(TableBorderType.Bottom, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                t.SetBorder(TableBorderType.InsideH, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                t.SetBorder(TableBorderType.InsideV, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                t.SetBorder(TableBorderType.Left, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                t.SetBorder(TableBorderType.Right, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                t.SetBorder(TableBorderType.Top, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                t.Alignment = Alignment.center;

                measurments.Append("\r\n").Font("Times New Roman")
                    .FontSize(12)
                    .Color(Color.Black).Bold();

                t.Rows[0].Cells[0].Paragraphs[0].Append("L.p.")
                          .Font("Times New Roman")
                          .FontSize(10)
                          .Color(Color.Black);
                t.Rows[0].Cells[1].Paragraphs[0].Append("Opis")
                          .Font("Times New Roman")
                          .FontSize(10)
                          .Color(Color.Black);

                for (int r = 1; r < rows; r++)
                {
                    t.Rows[r].Cells[0].Paragraphs[0].Append(markerList[r-1].Id.ToString())
                           .Font("Times New Roman")
                           .FontSize(10)
                           .Color(Color.Black);
                    t.Rows[r].Cells[1].Paragraphs[0].Append(markerList[r - 1].Description.ToString())
                          .Font("Times New Roman")
                          .FontSize(10)
                          .Color(Color.Black);

                }

                measurments.InsertTableAfterSelf(t);

                document.Save();
            }
            //}
            //catch
            //{
            //    MessageBox.Show("Nie można wydrukować dokumentu", "Błąd");
            //}
        }
    }
}
