using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Words.NET;

namespace EyeStation.RaportGenerator
{
    public static class RaportGenerator
    {
        public static void GenerateRaport(Dictionary<string, string> tags, string originalImagePath)
        {
            //try
            //{
                System.IO.Directory.CreateDirectory("Raporty");
                string path = @"Raporty/" + tags["(0010,0010)"] + "_" + String.Format("{0:MM/dd/yyyy/HH/mm/ss}", DateTime.Now) + ".docx";

                using (DocX document = DocX.Create(path))
                {
                    Paragraph date = document.InsertParagraph();
                    date.Alignment = Alignment.right;
                    date.Append(String.Format("{0:MM.dd.yyyy r.}", DateTime.Now))
                    .Font("Times New Roman")
                    .FontSize(10)
                    .Color(Color.Black);

                    Paragraph title = document.InsertParagraph();
                    title.Alignment = Alignment.center;
                    title.Append("Raport \r\n\r\n")
                    .Font("Times New Roman")
                    .FontSize(16)
                    .Color(Color.Black)
                    .Bold();

                    Paragraph patientData = document.InsertParagraph();
                    patientData.Alignment = Alignment.left;
                    patientData.Append("Identyfikator pacjenta: " + tags["(0010,0020)"] + "\r\n")
                     .Font("Times New Roman")
                     .FontSize(12)
                     .Color(Color.Black);
                    patientData.Append("Imię i nazwisko pacjenta: " + tags["(0010,0010)"] + "\r\n\r\n")
                    .Font("Times New Roman")
                    .FontSize(12)
                    .Color(Color.Black);

                    Paragraph description = document.InsertParagraph();
                    patientData.Alignment = Alignment.left;
                    string desc = tags["(0008,1080)"];
                    if (desc == "0") desc = "-";
                    patientData.Append("Opis badania: " + desc + "\r\n")
                    .Font("Times New Roman")
                    .FontSize(12)
                    .Color(Color.Black);

                    Paragraph images = document.InsertParagraph();
                    images.Alignment = Alignment.left;
                    images.Append("")
                    .Font("Times New Roman")
                    .FontSize(12)
                    .Color(Color.Black);

                    Xceed.Words.NET.Image image = document.AddImage(originalImagePath);
                    Picture originalImage = image.CreatePicture();
                    originalImage.Height = 200;
                    originalImage.Width = 200;
                    images.AppendPicture(originalImage);


                    Paragraph measurments = document.InsertParagraph();

                    int rows = 5;
                    int columns = 5;
                    Table t = document.AddTable(rows, columns);
                    t.SetBorder(TableBorderType.Bottom, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                    t.SetBorder(TableBorderType.InsideH, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                    t.SetBorder(TableBorderType.InsideV, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                    t.SetBorder(TableBorderType.Left, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                    t.SetBorder(TableBorderType.Right, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                    t.SetBorder(TableBorderType.Top, new Border(Xceed.Words.NET.BorderStyle.Tcbs_single, BorderSize.one, 1, Color.Black));
                    t.Alignment = Alignment.center;

                    for (int i = 0; i < columns; i++)
                    {
                        t.SetColumnWidth(i, 2000);
                        t.Rows[0].Cells[i].Paragraphs[0].Append("Nagłówek")
                                .Font("Times New Roman")
                                .FontSize(10)
                                .Color(Color.Black);
                    }
                    for (int r = 0; r < rows - 1; r++)
                    {
                        for (int c = 0; c < columns; c++)
                        {
                            t.Rows[r + 1].Cells[c].Paragraphs[0].Append("Komórka")
                               .Font("Times New Roman")
                               .FontSize(10)
                               .Color(Color.Black);
                        }

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
