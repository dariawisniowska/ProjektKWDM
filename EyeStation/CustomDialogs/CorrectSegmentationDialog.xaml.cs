using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;

namespace EyeStation.CustomDialogs
{
    /// <summary>
    /// Interaction logic for CorrectSegmentationDialog.xaml
    /// </summary>
    public partial class CorrectSegmentationDialog : Window
    {
        public CorrectSegmentationDialog(BitmapImage image, BitmapImage mask, byte[][] maskInBytes)
        {
            InitializeComponent();
            window.Width = image.Width;
            window.Height = 60 + image.Height + 100;
            window.Left = 5;
            window.Top = 5;

            img.Height = image.Height;
            img.Width = image.Width;
            inkCnv.Height = image.Height;
            inkCnv.Width = image.Width;
            
            img.Source = image;
            ImageBrush ibMask = new ImageBrush();
            ibMask.ImageSource = mask;
            inkCnv.Background = ibMask;

            this.isNewMask = false;
        }

        private void slSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (inkCnv != null)
            {
                var drawingAttributes = inkCnv.DefaultDrawingAttributes;
                Double newSize = Math.Round(slSize.Value, 0);
                drawingAttributes.Width = newSize;
                drawingAttributes.Height = newSize;
            }
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sigPath = @"..\..\tempMask.jpg";

                MemoryStream ms = new MemoryStream();
                FileStream fs = new FileStream(sigPath, FileMode.Create);

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)inkCnv.RenderSize.Width, (int)inkCnv.RenderSize.Height, 96d, 96d, PixelFormats.Default);
                rtb.Render(inkCnv);

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                encoder.Save(fs);
                fs.Close();
                this.isNewMask = true;
                this.DialogResult = true;
            }
            catch(Exception ex)
            {
                SimpleDialog sd = new SimpleDialog("Błąd zapisu", "Zapis segmentacji manulanej zakończył się niepowodzeniem.");
                sd.ShowDialog();
            }
        }

        private bool isNewMask;
        public bool IsNewMask
        {
            get { return this.isNewMask; }
        }
        public Bitmap NewMask {
            get { return new Bitmap(@"..\..\tempMask.jpg"); }
        }

        private void btnDraw_Checked(object sender, RoutedEventArgs e)
        {
            if (btnErase != null)
            {
                btnErase.IsChecked = false;
                inkCnv.DefaultDrawingAttributes.Color = Colors.White;
            }
        }

        private void btnDraw_Unchecked(object sender, RoutedEventArgs e)
        {
            btnErase.IsChecked = true;
            inkCnv.DefaultDrawingAttributes.Color = Colors.Black;
        }

        private void btnErase_Checked(object sender, RoutedEventArgs e)
        {
            btnDraw.IsChecked = false;
            inkCnv.DefaultDrawingAttributes.Color = Colors.Black;
        }

        private void btnErase_Unchecked(object sender, RoutedEventArgs e)
        {
            btnDraw.IsChecked = true;
            inkCnv.DefaultDrawingAttributes.Color = Colors.White;
        }
    }
}
