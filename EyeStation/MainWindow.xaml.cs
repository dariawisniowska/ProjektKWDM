﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MaterialDesignThemes.Wpf;
using EyeStation.Model;
using EyeStation.Tools;
using System.Drawing;
using Point = System.Windows.Point;
using EyeStation.PACSDAO;
using EyeStation.CustomDialogs;
using EyeStation.Models;
using System.IO;

namespace EyeStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PACSObj serwer;
       
        public MainWindow()
        {
            InitializeComponent();
            serwer = new PACSObj("127.0.0.1", 10100, "KLIENTL", "ARCHIWUM");
            serwer.Connect();
            List<PACSDAO.Patient> data = serwer.data;
        }
        void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                this.Close();
        }

        private List<Point> measurePoints;
        private bool isMeasureTool = false;
        private List<Point> anglePoints;
        private bool isAngleTool = false;
        private List<Marker> markersPoints;
        private bool isMarkerTool = false;
        private Line line;
        private Line startLine;
        private Canvas actualCanvas;

        private void btnFourImage_Click(object sender, RoutedEventArgs e)
        {
            btnFourImage.Visibility = Visibility.Collapsed;
            btnOneImage.Visibility = Visibility.Visible;
            gridOneImage.Visibility = Visibility.Collapsed;
            gridFourImage.Visibility = Visibility.Visible;
            uncheckedAll();
        }

        private void btnOneImage_Click(object sender, RoutedEventArgs e)
        {
            btnOneImage.Visibility = Visibility.Collapsed;
            btnFourImage.Visibility = Visibility.Visible;
            gridOneImage.Visibility = Visibility.Visible;
            gridFourImage.Visibility = Visibility.Collapsed;
            uncheckedAll();
        }

        private void btnMeasure_Checked(object sender, RoutedEventArgs e)
        {
            btnAngle.IsChecked = false;
            btnSelect.IsChecked = false;
            btnUnSelect.IsChecked = false;
            btnAddMarker.IsChecked = false;

            setSizeOfCanvas();

            this.isMeasureTool = true;
            this.measurePoints = new List<Point>();
        }

        private void btnMeasure_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isMeasureTool = false;
            if (this.measurePoints.Count == 1)
            {
                this.actualCanvas.Children.Remove(this.startLine);
            }
            else if (this.measurePoints.Count > 1)
            {
                BitmapImage bmp = (BitmapImage)imgBig.Source;
                int srcHeight = bmp.PixelHeight;
                double actualHeight = imgBig.ActualHeight;
                double pixelSize = 1;

                TextBlock textBlock = MeasureTool.getLengthOfActiveLine(this.measurePoints, srcHeight, actualHeight, pixelSize);
                this.actualCanvas.Children.Add(textBlock);
            }
            measurePoints = new List<Point>();
        }

        private void btnAngle_Checked(object sender, RoutedEventArgs e)
        {
            btnMeasure.IsChecked = false;
            btnSelect.IsChecked = false;
            btnUnSelect.IsChecked = false;
            btnAddMarker.IsChecked = false;

            setSizeOfCanvas();

            this.isAngleTool = true;
            this.anglePoints = new List<Point>();
        }

        private void btnAngle_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isAngleTool = false;

            int pointCount = anglePoints.Count;
            if (pointCount == 1)
                this.actualCanvas.Children.Remove(this.startLine);
            else if (pointCount == 2)
            {
                this.actualCanvas.Children.Remove(this.line);
                this.actualCanvas.Children.Remove(this.startLine);
            }
        }

        private void cnv_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Canvas cnv = (Canvas)sender;

            if (isMeasureTool)
            {
                this.measurePoints.Add(e.GetPosition(cnv));
                int pointCount = measurePoints.Count;
                if (pointCount == 1)
                {
                    this.actualCanvas = cnv;
                    Point startPoint = new Point(measurePoints[0].X + 1, measurePoints[0].Y + 1);
                    this.startLine = MeasureTool.createLine(measurePoints[0], startPoint);
                    cnv.Children.Add(this.startLine);
                }
                else if (pointCount > 1)
                {
                    if (e.ClickCount == 1 && this.actualCanvas == cnv)
                    {
                        Line line = MeasureTool.createLine(measurePoints[pointCount - 2], measurePoints[pointCount - 1]);
                        cnv.Children.Add(line);
                    }
                    else
                    {
                        BitmapImage bmp = (BitmapImage)imgBig.Source;
                        int srcHeight = bmp.PixelHeight;
                        double actualHeight = 0;
                        if (cnv == cnvBig)
                            actualHeight = imgBig.ActualHeight;
                        else
                            actualHeight = imgSmall.ActualHeight;
                        double pixelSize = 1;
                        if (e.ClickCount == 2 && this.actualCanvas == cnv)
                        {
                            Line line = MeasureTool.createLine(measurePoints[pointCount - 2], measurePoints[pointCount - 1]);
                            cnv.Children.Add(line);
                            TextBlock textBlock = MeasureTool.getLengthOfActiveLine(this.measurePoints, srcHeight, actualHeight, pixelSize);
                            cnv.Children.Add(textBlock);
                            measurePoints = new List<Point>();
                        }
                        else if (this.actualCanvas != cnv)
                        {
                            if (pointCount == 2)
                                this.actualCanvas.Children.Remove(startLine);
                            else
                            {
                                this.measurePoints.RemoveAt(pointCount - 1);
                                TextBlock textBlock = MeasureTool.getLengthOfActiveLine(this.measurePoints, srcHeight, actualHeight, pixelSize);
                                this.actualCanvas.Children.Add(textBlock);
                            }
                            measurePoints = new List<Point>();
                            this.measurePoints.Add(e.GetPosition(cnv));
                            this.actualCanvas = cnv;
                            Point startPoint = new Point(measurePoints[0].X + 1, measurePoints[0].Y + 1);
                            this.startLine = MeasureTool.createLine(measurePoints[0], startPoint);
                            cnv.Children.Add(this.startLine);
                        }
                    }
                }
            }
            if (isAngleTool)
            {
                this.anglePoints.Add(e.GetPosition(cnv));
                int pointCount = anglePoints.Count;
                if (pointCount == 1)
                {
                    this.actualCanvas = cnv;
                    Point startPoint = new Point(anglePoints[0].X + 1, anglePoints[0].Y + 1);
                    this.startLine = MeasureTool.createLine(anglePoints[0], startPoint);
                    cnv.Children.Add(this.startLine);
                }
                else if (pointCount == 2 && this.actualCanvas == cnv)
                {
                    this.line = MeasureTool.createLine(anglePoints[pointCount - 2], anglePoints[pointCount - 1]);
                    cnv.Children.Add(line);
                }
                else if (pointCount == 3 && this.actualCanvas == cnv)
                {
                    this.line = MeasureTool.createLine(anglePoints[pointCount - 2], anglePoints[pointCount - 1]);
                    cnv.Children.Add(line);
                    TextBlock textBlock = MeasureTool.getAngleOfActiveLine(this.anglePoints);
                    cnv.Children.Add(textBlock);
                    this.anglePoints = new List<Point>();
                }
                else
                {
                    if (pointCount == 2)
                    {
                        this.actualCanvas.Children.Remove(this.startLine);
                    }
                    else if (pointCount == 3)
                    {
                        this.actualCanvas.Children.Remove(this.line);
                        this.actualCanvas.Children.Remove(this.startLine);
                    }
                    this.actualCanvas = cnv;
                    this.anglePoints = new List<Point>();
                    this.anglePoints.Add(e.GetPosition(cnv));
                    Point startPoint = new Point(anglePoints[0].X + 1, anglePoints[0].Y + 1);
                    this.startLine = MeasureTool.createLine(anglePoints[0], startPoint);
                    cnv.Children.Add(this.startLine);
                }
            }
            if (isMarkerTool)
            {
                Point point = e.GetPosition(cnv);

                InputDialog inputDialog = new InputDialog("Nowy znacznik", "Wprowadź opis dle wskazanego znacznika:", "");
                if (inputDialog.ShowDialog() == true && inputDialog.Answer != null && inputDialog.Answer != "")
                {
                    Ellipse elipse = MeasureTool.createMarker(point);
                    ToolTip tt = new ToolTip();
                    tt.Content = wrapText(100, inputDialog.Answer);
                    elipse.ToolTip = tt;
                    cnv.Children.Add(elipse);
                    TextBlock textBlock = MeasureTool.createTextBox(MeasureTool.TextBlockColor.Mint);
                    textBlock.Text = (this.markersPoints.Count + 1).ToString();
                    textBlock.FontSize = 11;

                    Canvas.SetLeft(textBlock, point.X - 3);
                    Canvas.SetTop(textBlock, point.Y - 5);
                    textBlock.ToolTip = tt;
                    cnv.Children.Add(textBlock);

                    Marker marker = new Marker();
                    marker.Id = this.markersPoints.Count + 1;
                    marker.point = point;
                    marker.Description = inputDialog.Answer;
                    this.markersPoints.Add(marker);
                }
            }
        }

        private void btnSelect_Checked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Manualna segmentacja na obrazie (na którym? mamy tylko maskę)
             */

            btnMeasure.IsChecked = false;
            btnAngle.IsChecked = false;
            btnUnSelect.IsChecked = false;
            btnAddMarker.IsChecked = false;

            setSizeOfCanvas();
        }
        private void btnSelect_Unchecked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Wyłączenie opcji
             */
        }

        private void btnUnSelect_Checked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Manualna poprawa segmentacji na obrazie - usuwanie zaznaczenia (na wysegmentowanym)
             */

            btnMeasure.IsChecked = false;
            btnAngle.IsChecked = false;
            btnSelect.IsChecked = false;
            btnAddMarker.IsChecked = false;

            setSizeOfCanvas();
        }

        private void btnUnSelect_Unchecked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Wyłączenie opcji
             */
        }

        private void btnAddMarker_Checked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Manualne dodanie znacznika na obrazie (wysegmentowanym?)
             */

            btnMeasure.IsChecked = false;
            btnAngle.IsChecked = false;
            btnSelect.IsChecked = false;
            btnUnSelect.IsChecked = false;

            setSizeOfCanvas();
            this.isMarkerTool = true;
        }

        private void btnAddMarker_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isMarkerTool = false;
        }

        private void btnGetImage_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Połączenie z serwerem i wyświetlenie rzeczywistych danych
             */
            uncheckedAll();
            selectStudyPanel.Visibility = Visibility.Visible;
            mainPanel.Visibility = Visibility.Collapsed;

            List<Study> items = new List<Study>();
            //for (int i = 0; i<25; i++)
            //    items.Add(new Study() { Id = i, Name = "Jan Kowalski", Description = "Opis"});

            items = serwer.GetStudies();

            lvStudy.ItemsSource = items;
        }

        private void lvStudy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TO DO:
            /* 
             * Wyświetlenie odpowiedniego obrazu
             */
            if (lvStudy.SelectedItems.Count != 0)
            {
                Study study = (Study)lvStudy.SelectedItems[0];

                makeEnableAll();

                selectStudyPanel.Visibility = Visibility.Collapsed;
                mainPanel.Visibility = Visibility.Visible;
                makeEnableAll();

                imgBig.Source = study.ImageSource;
                imgSmall.Source = study.ImageSource;
                imgGreen.Source = study.ImageSource;
                imgMask.Source = study.ImageSource;
                imgMaskAndImage.Source = study.ImageSource;
                clearAllCanvas();
            }
        }

        private void svSelectStydyPanel_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void btnAddImage_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Dodanie dicoma do pacsa
             */
            uncheckedAll();
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Wybierz plik DICOM";
            //op.Filter = "DICOM (*.dcm;*.vtk)|*.dcm;*.vtk";
            if (op.ShowDialog() == true)
            {
                imgBig.Source = new BitmapImage(new Uri(op.FileName));
                imgSmall.Source = new BitmapImage(new Uri(op.FileName));
                imgGreen.Source = new BitmapImage(new Uri(op.FileName));
                imgMask.Source = new BitmapImage(new Uri(op.FileName));
                imgMaskAndImage.Source = new BitmapImage(new Uri(op.FileName));
                makeEnableAll();
                selectStudyPanel.Visibility = Visibility.Collapsed;
                mainPanel.Visibility = Visibility.Visible;

                clearAllCanvas();
            }
        }

        private void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            String path = "";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = ".png (*.png)|*.png";
            if (sfd.ShowDialog() == true)
            {
                path = sfd.FileName;

                System.Windows.Controls.Image image = null;
                for (int i = 0; i < 4; i++)
                {
                    if (btnOneImage.Visibility == Visibility.Collapsed)
                    {
                        image = imgBig;
                    }
                    else
                    {
                        switch (i)
                        {
                            case 0:
                                image = imgSmall;
                                break;
                            case 1:
                                image = imgGreen;
                                break;
                            case 2:
                                image = imgMask;
                                break;
                            case 3:
                                image = imgMaskAndImage;
                                break;
                        }
                    }
                    double width = image.ActualWidth;
                    double height = image.ActualHeight;

                    Point relativePoint = image.TransformToAncestor(Application.Current.MainWindow)
                                  .Transform(new Point(0, 0));
                    using (Bitmap bmp = new Bitmap((int)width,
                        (int)height))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            Opacity = .0;
                            g.CopyFromScreen((int)relativePoint.X, (int)relativePoint.Y, 0, 0, bmp.Size);
                            string newPath = path;
                            if (image != imgBig)
                            {
                                newPath = path.Insert(path.Length - 4, "-" + i);
                            }
                            bmp.Save(newPath);
                            Opacity = 1;
                        }
                    }


                    if (this.markersPoints.Count > 0)
                    {
                        StreamWriter file = new StreamWriter(path.Replace(".png", ".txt"));
                        foreach (Marker marker in markersPoints)
                        {
                            string line = marker.Id + ". " + marker.Description;
                            file.WriteLine(line);
                        }
                        file.Close();
                    }

                    if (image == imgBig)
                    {
                        SimpleDialog simpleDialog = new SimpleDialog("Zapis do .png", "Obraz zapisano prawidłowo.");
                        simpleDialog.ShowDialog();
                        break;
                    }
                    if (i == 3)
                    {
                        SimpleDialog simpleDialog = new SimpleDialog("Zapis do .png", "Obrazy zapisano prawidłowo.");
                        simpleDialog.ShowDialog();
                    }
                }
            }
        }

        private void btnSegmentation_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Segmentacja naczyń krwionośnych
             */
            uncheckedAll();
            SimpleDialog simpleDialog = new SimpleDialog("Segmentacja", "Segmentacja zakończona powodzeniem.");
            simpleDialog.ShowDialog();
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Okno z informacją o podejrzeniu choroby
             */
            uncheckedAll();
            string result = "Istnieje podejrzenie retinopatii";
            SimpleDialog simpleDialog = new SimpleDialog("Analiza retinopatii", result);
            simpleDialog.ShowDialog();
        }

        private void btnDesription_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Podłączyć pod PACSA
             * Zmienić "Opis" na rzeczywiste dane
             * Answer wpisać do DICOMa
             */
            uncheckedAll();

            InputDialog inputDialog = new InputDialog("Edytuj opis badania", "Wprowadź nowy opis aktualnego badania:", "Opis");
            if (inputDialog.ShowDialog() == true)
            {
                //lbl_logo.Content = inputDialog.Answer;
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Automatyczne generowanie raportu z badania
             */
            uncheckedAll();
            SimpleDialog simpleDialog = new SimpleDialog("Raport", "Generowanie raportu zakończone powodzeniem.");
            simpleDialog.ShowDialog();
        }

        private void makeEnableAll()
        {
            btnMeasure.IsEnabled = true;
            btnAngle.IsEnabled = true;
            btnSelect.IsEnabled = true;
            btnUnSelect.IsEnabled = true;
            btnAddMarker.IsEnabled = true;
            btnSaveImage.IsEnabled = true;
            btnSegmentation.IsEnabled = true;
            btnAnalysis.IsEnabled = true;
            btnDesription.IsEnabled = true;
            btnReport.IsEnabled = true;
            this.markersPoints = new List<Marker>();
        }

        private void setSizeOfCanvas()
        {
            cnvBig.Height = imgBig.ActualHeight;
            cnvBig.Width = imgBig.ActualWidth;

            cnvSmall.Height = imgSmall.ActualHeight;
            cnvSmall.Width = imgSmall.ActualWidth;

            cnvGreen.Height = imgGreen.ActualHeight;
            cnvGreen.Width = imgGreen.ActualWidth;

            cnvMask.Height = imgMask.ActualHeight;
            cnvMask.Width = imgMask.ActualWidth;

            cnvMaskAndImage.Height = imgMaskAndImage.ActualHeight;
            cnvMaskAndImage.Width = imgMaskAndImage.ActualWidth;
        }

        private void uncheckedAll()
        {
            btnMeasure.IsChecked = false;
            btnAngle.IsChecked = false;
            btnSelect.IsChecked = false;
            btnUnSelect.IsChecked = false;
            btnAddMarker.IsChecked = false;
        }

        private void clearAllCanvas()
        {
            cnvBig.Children.Clear();
            cnvSmall.Children.Clear();
            cnvGreen.Children.Clear();
            cnvMask.Children.Clear();
            cnvMaskAndImage.Children.Clear();
        }

        private string wrapText(int limitInOneLine, string sentence)
        {
            string[] words = sentence.Split(' ');

            StringBuilder newSentence = new StringBuilder();

            string line = "";
            foreach (string word in words)
            {
                if ((line + word).Length > limitInOneLine)
                {
                    newSentence.AppendLine(line);
                    line = "";
                }

                line += string.Format("{0} ", word);
            }

            if (line.Length > 0)
                newSentence.AppendLine(line);

            return newSentence.ToString();
        }
    }
}
