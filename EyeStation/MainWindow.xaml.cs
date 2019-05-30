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
using VesselSegmentatorFilter;
using EyeStation.VesselSegmentatorFilter;
using System.Web.Script.Serialization;
using EyeStation.VesselsLengthFilter;
using System.Drawing.Imaging;

namespace EyeStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PACSObj serwer;
        public List<int> lengths;

        public MainWindow()
        {
            InitializeComponent();
            serwer = new PACSObj("127.0.0.1", 10100, "KLIENTL", "ARCHIWUM");
            serwer.Connect();
            List<PACSDAO.Patient> data = serwer.data;
            vesselSegmentator = new VesselSegmentator()
            {
                VesselSegmentatioMethodType = VesselSegmentatioMethod.Thresholding
            };
            vesselMeasurements = new VesselMeasurements();
            List<Study> items = new List<Study>();
            items = serwer.GetStudies();
            lvStudy.ItemsSource = items;

            jss = new JavaScriptSerializer();
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
        private bool isMarkerTool = false;
        private Line line;
        private Line startLine;
        private Canvas actualCanvas;
        private VesselSegmentator vesselSegmentator;
        private VesselMeasurements vesselMeasurements;
        private BitmapImage imageSource;
        private BitmapImage maskImage;
        private StudyDrawing studyDrawing;
        private Study actualStudy;
        private JavaScriptSerializer jss;
        private byte[][] maskInBytes;

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
            btnAddMarker.IsChecked = false;

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
                int srcHeight = imageSource.PixelHeight;
                double actualHeight = cnvBig.ActualHeight;
                double pixelSize = 1;

                double lineLength = MeasureTool.getLengthValue(this.measurePoints, srcHeight, actualHeight, pixelSize);
                TextBlock textBlock = MeasureTool.createTextBlockForLine(this.measurePoints, lineLength);
                this.actualCanvas.Children.Add(textBlock);
            }
            measurePoints = new List<Point>();
        }

        private void btnAngle_Checked(object sender, RoutedEventArgs e)
        {
            btnMeasure.IsChecked = false;
            btnAddMarker.IsChecked = false;

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
                        if (e.ClickCount == 2 && this.actualCanvas == cnv)
                        {
                            Line line = MeasureTool.createLine(measurePoints[pointCount - 2], measurePoints[pointCount - 1]);
                            cnv.Children.Add(line);
                            endOfLine(cnv);
                            measurePoints = new List<Point>();
                        }
                        else if (this.actualCanvas != cnv)
                        {
                            if (pointCount == 2)
                                this.actualCanvas.Children.Remove(startLine);
                            else
                            {
                                this.measurePoints.RemoveAt(pointCount - 1);
                                endOfLine(this.actualCanvas);
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
                    double angleValue = MeasureTool.getAngleValue(this.anglePoints);
                    TextBlock textBlock = MeasureTool.createTextBlockForAngle(this.anglePoints, angleValue);
                    cnv.Children.Add(textBlock);

                    Angle angle = new Angle();
                    angle.Id = studyDrawing.AngleList.Count + 1;
                    angle.Points = anglePoints;
                    angle.Value = angleValue;
                    angle.ActualCanvas = cnv.Name;

                    if (cnv.Name == "cnvBig")
                    {
                        Angle smallAngle = new Angle(angle);
                        List<Point> newPoints = new List<Point>();
                        foreach (Point point in angle.Points) {
                            newPoints.Add(bigToSmallPoint(point));
                        }
                        smallAngle.Points = newPoints;
                        smallAngle.ActualCanvas = "cnvSmall";
                        drawAngles(smallAngle);
                        studyDrawing.AngleList.Add(smallAngle);
                    }
                    else if (cnv.Name == "cnvSmall")
                    {
                        Angle bigAngle = new Angle(angle);
                        List<Point> newPoints = new List<Point>();
                        foreach (Point point in angle.Points)
                        {
                            newPoints.Add(smallToBigPoint(point));
                        }
                        bigAngle.Points = newPoints;
                        bigAngle.ActualCanvas = "cnvBig";
                        drawAngles(bigAngle);
                        studyDrawing.AngleList.Add(angle);
                    }
                    else
                    {
                        studyDrawing.AngleList.Add(angle);
                    }
                    studyDrawing.Modyfied = true;

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

                InputDialog inputDialog = new InputDialog("Nowy znacznik", "Wprowadź opis dla wskazanego znacznika:", "");
                if (inputDialog.ShowDialog() == true && inputDialog.Answer != null && inputDialog.Answer != "")
                {
                    Marker marker = new Marker();
                    marker.Id = studyDrawing.MarkerList.Count + 1;
                    marker.Point = point;
                    marker.Description = inputDialog.Answer;
                    marker.ActualCanvas = cnv.Name;
                    drawMarker(marker, cnv);
                    if (cnv.Name == "cnvBig")
                    {
                        Marker smallMarker = new Marker(marker);
                        smallMarker.Point = bigToSmallPoint(marker.Point);
                        smallMarker.ActualCanvas = "cnvSmall";
                        drawMarker(smallMarker, cnvSmall);
                        studyDrawing.MarkerList.Add(smallMarker);
                    }
                    else if (cnv.Name == "cnvSmall")
                    {
                        Marker bigMarker = new Marker(marker);
                        bigMarker.Point = smallToBigPoint(marker.Point);
                        bigMarker.ActualCanvas = "cnvBig";
                        drawMarker(bigMarker, cnvBig);
                        studyDrawing.MarkerList.Add(marker);
                    }
                    else
                    {
                        studyDrawing.MarkerList.Add(marker);
                    }
                }
            }
        }

        private Point smallToBigPoint(Point point)
        {
            Point realPoint = MeasureTool.toRealPoint(point.X, point.Y, imageSource.PixelHeight, cnvSmall.Height);
            return MeasureTool.toActualPoint(realPoint.X, realPoint.Y, imageSource.PixelHeight, cnvBig.Height);
        }

        private Point bigToSmallPoint(Point point) {
            Point realPoint = MeasureTool.toRealPoint(point.X, point.Y, imageSource.PixelHeight, cnvBig.Height);
            return MeasureTool.toActualPoint(realPoint.X, realPoint.Y, imageSource.PixelHeight, cnvSmall.Height);
        }

        private void drawMarker(Marker marker, Canvas cnv)
        {
            if (cnv == null)
                cnv = getActualCanvas(marker.ActualCanvas);
            else
                studyDrawing.Modyfied = true;

            Ellipse elipse = MeasureTool.createMarker(marker.Point);
            ToolTip tt = new ToolTip();
            tt.Content = wrapText(100, marker.Description);
            elipse.ToolTip = tt;
            cnv.Children.Add(elipse);
            TextBlock textBlock = MeasureTool.createTextBox(MeasureTool.TextBlockColor.Mint);
            textBlock.Text = (marker.Id).ToString();
            textBlock.FontSize = 11;

            Canvas.SetLeft(textBlock, marker.Point.X - 3);
            Canvas.SetTop(textBlock, marker.Point.Y - 5);
            textBlock.ToolTip = tt;
            cnv.Children.Add(textBlock);
        }

        private void drawAngles(Angle angle)
        {
            List<Point> points = angle.Points;
            int pointCount = points.Count;
            Line firstLine = MeasureTool.createLine(points[pointCount - 3], points[pointCount - 2]);
            Canvas cnv = getActualCanvas(angle.ActualCanvas);
            cnv.Children.Add(firstLine);

            Line secondLine = MeasureTool.createLine(points[pointCount - 2], points[pointCount - 1]);
            cnv.Children.Add(secondLine);
            TextBlock textBlock = MeasureTool.createTextBlockForAngle(points, angle.Value);
            cnv.Children.Add(textBlock);
        }

        private void endOfLine(Canvas cnv)
        {
            int srcHeight = this.imageSource.PixelHeight;
            double actualHeight = 0;
            if (cnv == cnvBig)
                actualHeight = cnvBig.ActualHeight;
            else
                actualHeight = cnvSmall.ActualHeight;
            double pixelSize = 1;

            double lineLength = MeasureTool.getLengthValue(this.measurePoints, srcHeight, actualHeight, pixelSize);
            TextBlock textBlock = MeasureTool.createTextBlockForLine(this.measurePoints, lineLength);
            cnv.Children.Add(textBlock);

            MeasureLine mLine = new MeasureLine();
            mLine.Id = studyDrawing.LineList.Count + 1;
            mLine.Points = measurePoints;
            mLine.Value = lineLength;
            mLine.ActualCanvas = cnv.Name;

            if (cnv.Name == "cnvBig")
            {
                MeasureLine smallLine = new MeasureLine(mLine);
                List<Point> newPoints = new List<Point>();
                foreach (Point point in mLine.Points)
                {
                    newPoints.Add(bigToSmallPoint(point));
                }
                smallLine.Points = newPoints;
                smallLine.ActualCanvas = "cnvSmall";
                drawLine(smallLine);
                studyDrawing.LineList.Add(smallLine);
            }
            else if (cnv.Name == "cnvSmall")
            {
                MeasureLine bigLine = new MeasureLine(mLine);
                List<Point> newPoints = new List<Point>();
                foreach (Point point in mLine.Points)
                {
                    newPoints.Add(smallToBigPoint(point));
                }
                bigLine.Points = newPoints;
                bigLine.ActualCanvas = "cnvBig";
                drawLine(bigLine);
                studyDrawing.LineList.Add(mLine);
            }
            else
            {
                studyDrawing.LineList.Add(mLine);
            }
            studyDrawing.Modyfied = true;
        }

        private void drawLine(MeasureLine measureLine)
        {
            List<Point> points = measureLine.Points;
            int pointCount = points.Count;
            Canvas cnv = getActualCanvas(measureLine.ActualCanvas);
            for (int i = 1; i < pointCount; i++)
            {
                Line line = MeasureTool.createLine(points[i - 1], points[i]);
                cnv.Children.Add(line);
                if (i == pointCount - 1)
                {
                    TextBlock textBlock = MeasureTool.createTextBlockForLine(points, measureLine.Value);
                    cnv.Children.Add(textBlock);
                }
            }
        }

        private Canvas getActualCanvas(String actualCanvas)
        {
            Canvas cnv = null;
            switch (actualCanvas)
            {
                case "cnvSmall":
                    return cnv = cnvSmall;
                case "cnvGreen":
                    return cnv = cnvGreen;
                case "cnvMask":
                    return cnv = cnvMask;
                case "cnvMaskAndImage":
                    return cnv = cnvMaskAndImage;
                case "cnvBig":
                    return cnv = cnvBig;
                default:
                    return cnv = cnvSmall;
            }
        }

        private void btnAddMarker_Checked(object sender, RoutedEventArgs e)
        {
            btnMeasure.IsChecked = false;
            btnAngle.IsChecked = false;
            this.isMarkerTool = true;
        }

        private void btnAddMarker_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isMarkerTool = false;
        }

        private void btnGetImage_Click(object sender, RoutedEventArgs e)
        {
            selectStudyPanel.Visibility = Visibility.Visible;
            mainPanel.Visibility = Visibility.Collapsed;
        }

        private void lvStudy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvStudy.SelectedItems.Count != 0)
            {
                Study study = (Study)lvStudy.SelectedItems[0];

                try
                {
                    saveAllStudyDrawing();

                    displayImage(new BitmapImage(study.ImageSource.UriSource));

                    btnBack.Visibility = Visibility.Visible;
                    selectStudyPanel.Visibility = Visibility.Collapsed;
                    mainPanel.Visibility = Visibility.Visible;
                    makeEnableAll();
                    setSizeOfCanvas();
                    clearAllCanvas();
                    uncheckedAll();

                    studyDrawing = new StudyDrawing();
                    studyDrawing.MarkerList = new List<Marker>();
                    studyDrawing.AngleList = new List<Angle>();
                    studyDrawing.LineList = new List<MeasureLine>();
                    this.actualStudy = (Study)lvStudy.SelectedItems[0];

                    if (study.Markers != null && study.Markers != "[]" && study.Markers != "- ")
                    {
                        string markers = study.Markers;
                        markers = markers.Replace('*', '"');
                        studyDrawing.MarkerList = jss.Deserialize<List<Marker>>(markers);

                        foreach (Marker marker in studyDrawing.MarkerList)
                        {
                            marker.Point = MeasureTool.toActualPoint(marker.Point.X, marker.Point.Y, imageSource.PixelHeight, cnvSmall.Height);
                            drawMarker(marker, null);
                            if (marker.ActualCanvas == "cnvSmall")
                            {
                                Marker bigMarker = new Marker(marker);
                                bigMarker.Point = smallToBigPoint(marker.Point);
                                bigMarker.ActualCanvas = "cnvBig";
                                drawMarker(bigMarker, null);
                            }
                        }
                    }
                    if (study.Angles != null && study.Angles != "[]" && study.Angles != "- ")
                    {
                        string angles = study.Angles;
                        angles = angles.Replace('*', '"');
                        studyDrawing.AngleList = jss.Deserialize<List<Angle>>(angles);

                        foreach (Angle angle in studyDrawing.AngleList)
                        {
                            for (int i = 0; i < angle.Points.Count; i++)
                            {
                                Point actualPoint = MeasureTool.toActualPoint(angle.Points[i].X, angle.Points[i].Y, imageSource.PixelHeight, cnvSmall.Height);
                                angle.Points[i] = actualPoint;
                            }
                            drawAngles(angle);
                            if (angle.ActualCanvas == "cnvSmall")
                            {
                                List<Point> newPoints = new List<Point>();
                                foreach (Point point in angle.Points)
                                {
                                    newPoints.Add(smallToBigPoint(point));
                                }
                                Angle bigAngle = new Angle(angle);
                                bigAngle.Points = newPoints;
                                bigAngle.ActualCanvas = "cnvBig";
                                drawAngles(bigAngle);
                            }
                        }
                    }
                    if (study.Lengths != null && study.Lengths != "[]" && study.Lengths != "- ")
                    {
                        string lengths = study.Lengths;
                        lengths = lengths.Replace('*', '"');
                        studyDrawing.LineList = jss.Deserialize<List<MeasureLine>>(lengths);

                        foreach (MeasureLine line in studyDrawing.LineList)
                        {
                            for (int i = 0; i < line.Points.Count; i++)
                            {
                                Point actualPoint = MeasureTool.toActualPoint(line.Points[i].X, line.Points[i].Y, imageSource.PixelHeight, cnvSmall.Height);
                                line.Points[i] = actualPoint;
                            }
                            drawLine(line);
                            if (line.ActualCanvas == "cnvSmall")
                            {
                                List<Point> newPoints = new List<Point>();
                                foreach (Point point in line.Points)
                                {
                                    newPoints.Add(smallToBigPoint(point));
                                }
                                MeasureLine bigLine = new MeasureLine(line);
                                bigLine.Points = newPoints;
                                bigLine.ActualCanvas = "cnvBig";
                                drawLine(bigLine);
                            }
                        }
                    }
                }
                catch (NullReferenceException ex)
                {
                    SimpleDialog simpleDialog = new SimpleDialog("Brak obrazu", "Plik DICOM nie posiada danych obrazowych. Wybierz inne badanie.");
                    simpleDialog.ShowDialog();
                    selectStudyPanel.Visibility = Visibility.Visible;
                    mainPanel.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    SimpleDialog simpleDialog = new SimpleDialog("Błąd", "Nieoczekiwany błąd. Wybierz inne badanie.");
                    simpleDialog.ShowDialog();
                    selectStudyPanel.Visibility = Visibility.Visible;
                    mainPanel.Visibility = Visibility.Collapsed;
                }
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
            uncheckedAll();
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Wybierz plik DICOM";
            op.Filter = "DICOM (*.dcm;*.vtk)|*.dcm;*.vtk";
            if (op.ShowDialog() == true)
            {
                serwer.Store(op.FileName);

                List<Study> items = new List<Study>();
                items = serwer.GetStudies();
                lvStudy.ItemsSource = items;

                SimpleDialog simpleDialog = new SimpleDialog("Nowy plik", "Plik został dodany prawidłowo.");

                simpleDialog.ShowDialog();

                lvStudy.SelectedItem = items[items.Count - 1];
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

                saveImagesToPng(path);

                if (studyDrawing.MarkerList.Count > 0)
                {
                    StreamWriter file = new StreamWriter(path.Replace(".png", ".txt"));
                    foreach (Marker marker in studyDrawing.MarkerList)
                    {
                        string line = marker.Id + ". " + marker.Description;
                        file.WriteLine(line);
                    }
                    file.Close();
                }

                SimpleDialog simpleDialog = new SimpleDialog("Zapis do .png", "Obrazy zapisano prawidłowo.");
                simpleDialog.ShowDialog();
            }
        }

        private void btnSegmentationImage_Click(object sender, RoutedEventArgs e)
        {
            uncheckedAll();
            CorrectSegmentationDialog csd = new CorrectSegmentationDialog(this.imageSource, this.maskImage, this.maskInBytes);
            if (csd.ShowDialog() == true)
            {
                if (csd.IsNewMask && csd.NewMask != null)
                {
                    this.maskImage = BitmapWriter.Bitmap2BitmapImage(csd.NewMask);
                    ImageBrush ibMask = new ImageBrush();
                    ibMask.ImageSource = this.maskImage;
                    cnvMask.Background = ibMask;
                }
            }
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Okno z informacją o podejrzeniu choroby
             */
            uncheckedAll();
            string result = "Nie istnieje podejrzenie retinopatii cukrzycowej ani nadciśnieniowej.";
            EyeStation.VesselAnalysisFilter.VesselAnaylis vesselAnalysis = new EyeStation.VesselAnalysisFilter.VesselAnaylis(vesselMeasurements.lengths);
            int result_code = vesselAnalysis.Classify();
            if (result_code != -1)
            {
                if (result_code == 1)
                    result = "Istnieje podejrzenie retinopatii cukrzycowej.";
                if (result_code == 2)
                    result = "Istnieje podejrzenie retinopatii nadciśnieniowej.";
                SimpleDialog simpleDialog = new SimpleDialog("Analiza retinopatii", result);
                simpleDialog.ShowDialog();
            }
            else
            {
                SimpleDialog simpleDialog = new SimpleDialog("Błąd", "Analiza pod kątem retinopatii nie powiodła się.");
                simpleDialog.ShowDialog();
            }
        }

        private void btnDesription_Click(object sender, RoutedEventArgs e)
        {
            uncheckedAll();
            int index = lvStudy.SelectedIndex;
            Study study = (Study)lvStudy.SelectedItems[0];
            string aktualnyOpis = study.Description;
            InputDialog inputDialog = new InputDialog("Edytuj opis badania", "Wprowadź nowy opis aktualnego badania:", aktualnyOpis);
            if (inputDialog.ShowDialog() == true)
            {
                SimpleDialog simpleDialog = new SimpleDialog("", "");
                if (Study.EditDescription(serwer, (Study)lvStudy.SelectedItems[0], inputDialog.Answer))
                {
                    simpleDialog = new SimpleDialog("Zmiana opisu", "Edytowanie opisu zakończone powodzeniem.");
                    lvStudy.ItemsSource = serwer.GetStudies();
                    lvStudy.SelectedIndex = index;
                }
                else
                    simpleDialog = new SimpleDialog("Zmiana opisu", "Edytowanie opisu nie powiodło się.");

                simpleDialog.ShowDialog();
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

        private void btnBackToImage_Click(object sender, RoutedEventArgs e)
        {
            selectStudyPanel.Visibility = Visibility.Collapsed;
            mainPanel.Visibility = Visibility.Visible;
        }

        private void makeEnableAll()
        {
            btnMeasure.IsEnabled = true;
            btnAngle.IsEnabled = true;
            btnAddMarker.IsEnabled = true;
            btnSaveImage.IsEnabled = true;
            btnAnalysis.IsEnabled = true;
            btnDesription.IsEnabled = true;
            btnReport.IsEnabled = true;
            btnSegmentation.IsEnabled = true;
        }

        private void setSizeOfCanvas()
        {
            double actualHeight = gridCanvas.ActualHeight;
            cnvBig.Height = actualHeight;
            cnvBig.Width = imageSource.PixelWidth * actualHeight / imageSource.PixelHeight;
            double smallHeight = actualHeight / 2 - 30;
            double smallWidth = imageSource.PixelWidth * (actualHeight / 2) / imageSource.PixelHeight - 30;
            cnvSmall.Height = smallHeight;
            cnvSmall.Width = smallWidth;
            cnvGreen.Height = smallHeight;
            cnvGreen.Width = smallWidth;
            cnvMask.Height = smallHeight;
            cnvMask.Width = smallWidth;
            cnvMaskAndImage.Height = smallHeight;
            cnvMaskAndImage.Width = smallWidth;
        }

        private void uncheckedAll()
        {
            btnMeasure.IsChecked = false;
            btnAngle.IsChecked = false;
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

        private void saveImagesToPng(string path)
        {
            int gridHeight = (int)gridCanvas.RenderSize.Height;
            int gridWidth = (int)gridCanvas.RenderSize.Width;
            int cnvHeight = gridHeight / 2 - 30;
            int cnvWidth = (int)imageSource.PixelWidth * cnvHeight / imageSource.PixelHeight;

            string newPath = "";
            for (int i = 0; i < 4; i++)
            {
                int positionX = 0;
                int positionY = 0;
                RenderTargetBitmap rtb = new RenderTargetBitmap(gridWidth, gridHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);
                newPath = path.Insert(path.Length - 4, "-" + i);
                positionX = Convert.ToInt32((gridWidth / 2 - cnvWidth) / 2);
                switch (i)
                {
                    case 0:
                        rtb.Render(cnvSmall);
                        break;
                    case 1:
                        positionX += gridWidth / 2;
                        rtb.Render(cnvGreen);
                        break;
                    case 2:
                        positionY += gridHeight / 2;
                        rtb.Render(cnvMask);
                        break;
                    case 3:
                        positionX += gridWidth / 2;
                        positionY += gridHeight / 2;
                        rtb.Render(cnvMaskAndImage);
                        break;
                }

                var crop = new CroppedBitmap(rtb, new Int32Rect(positionX, positionY, cnvWidth, cnvHeight));

                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(crop));

                using (var fs = System.IO.File.OpenWrite(newPath))
                {
                    pngEncoder.Save(fs);
                }
            }
        }

        private void displayImage(BitmapImage image)
        {
            ImageBrush ib = new ImageBrush();
            this.imageSource = image;
            ib.ImageSource = imageSource;
            cnvBig.Background = ib;

            cnvSmall.Background = ib;

            vesselSegmentator.SetInput(BitmapWriter.BitmapImage2Bitmap(image));
            var greenCanal = BitmapWriter.GetBitmap(vesselSegmentator.CanalPixels);
            ImageBrush ibGreen = new ImageBrush();
            ibGreen.ImageSource = BitmapWriter.Bitmap2BitmapImage(greenCanal);
            cnvGreen.Background = ibGreen;

            //TEMPORARY
            vesselSegmentator.SetInput(BitmapWriter.BitmapImage2Bitmap(image));
            vesselSegmentator.Calculate();
            this.maskInBytes = vesselSegmentator.Result;
            var result = BitmapWriter.GetBitmap(maskInBytes);
            ImageBrush ibMask = new ImageBrush();
            this.maskImage = BitmapWriter.Bitmap2BitmapImage(result);
            ibMask.ImageSource = this.maskImage;
            cnvMask.Background = ibMask;

            //END TEMPORARTY

            vesselMeasurements.SetInput(vesselSegmentator.Result);
            Bitmap result2 = vesselMeasurements.Calculate();
            ImageBrush ibMaskAndImage = new ImageBrush();
            ibMaskAndImage.ImageSource = BitmapWriter.Bitmap2BitmapImage(result2);
            cnvMaskAndImage.Background = ibMaskAndImage;
        }

        private void saveAllStudyDrawing()
        {
            if (studyDrawing != null && studyDrawing.Modyfied)
            {
                studyDrawing.Modyfied = false;
                int index = lvStudy.SelectedIndex;
                bool refreshStudy = false;
                bool showError = false;

                foreach (Marker marker in studyDrawing.MarkerList)
                {
                    Point realPoint = MeasureTool.toRealPoint(marker.Point.X, marker.Point.Y, imageSource.PixelHeight, cnvSmall.ActualHeight);
                    marker.Point = realPoint;
                }
                string markerJson = jss.Serialize(studyDrawing.MarkerList);
                markerJson = markerJson.Replace('"', '*');
                if (markerJson != "[]" && markerJson != null && markerJson != "- " && markerJson != this.actualStudy.Markers)
                {
                    if (Study.EditMarkers(serwer, this.actualStudy, markerJson))
                        refreshStudy = true;
                    else
                        showError = true;
                }

                foreach (Angle angle in studyDrawing.AngleList)
                {
                    for (int i = 0; i < angle.Points.Count; i++)
                    {
                        Point realPoint = MeasureTool.toRealPoint(angle.Points[i].X, angle.Points[i].Y, imageSource.PixelHeight, cnvSmall.ActualHeight);
                        angle.Points[i] = realPoint;
                    }
                }
                string angleJson = jss.Serialize(studyDrawing.AngleList);
                angleJson = angleJson.Replace('"', '*');
                if (angleJson != "[]" && angleJson != null && angleJson != "- " && angleJson != this.actualStudy.Angles)
                {
                    if (Study.EditAngles(serwer, this.actualStudy, angleJson))
                        refreshStudy = true;
                    else
                        showError = true;
                }

                foreach (MeasureLine line in studyDrawing.LineList)
                {
                    for (int i = 0; i < line.Points.Count; i++)
                    {
                        Point realPoint = MeasureTool.toRealPoint(line.Points[i].X, line.Points[i].Y, imageSource.PixelHeight, cnvSmall.ActualHeight);
                        line.Points[i] = realPoint;
                    }
                }
                string lineJson = jss.Serialize(studyDrawing.LineList);
                lineJson = lineJson.Replace('"', '*');
                if (lineJson != "[]" && lineJson != null && lineJson != "- " && lineJson != this.actualStudy.Lengths)
                {
                    if (Study.EditLengths(serwer, this.actualStudy, lineJson))
                        refreshStudy = true;
                    else
                        showError = true;
                }

                if (refreshStudy)
                {
                    SimpleDialog simpleDialog = new SimpleDialog("", "");
                    if (!showError)
                        simpleDialog = new SimpleDialog("Sukces", "Wprowadzone zmiany zostały zapisane prawidłowo.");
                    else
                        simpleDialog = new SimpleDialog("Bład", "Zapis nie powiodł się.");
                    simpleDialog.ShowDialog();
                    lvStudy.ItemsSource = serwer.GetStudies();
                    lvStudy.SelectedIndex = index;
                }
            }
        }

        private void SlBright_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (imageSource != null)
            {
                Bitmap inputImage = BitmapWriter.BitmapImage2Bitmap(imageSource);
                Bitmap outputImage = AdjustBrightness(inputImage, (int)slBright.Value * 5);

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = BitmapWriter.Bitmap2BitmapImage(outputImage);
                cnvSmall.Background = imageBrush;
                cnvBig.Background = imageBrush;
            }
        }

        private Bitmap AdjustBrightness(Bitmap inputBitmap, int value)
        {
            Bitmap newBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height);
            Graphics graphics = Graphics.FromImage(newBitmap);

            float finalValue = (float)value / 255.0f;
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] { finalValue, finalValue, finalValue, 1, 1}
                });

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);
            graphics.DrawImage(inputBitmap, new System.Drawing.Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height), 
                0, 0, inputBitmap.Width, inputBitmap.Height, GraphicsUnit.Pixel, imageAttributes);

            imageAttributes.Dispose();
            graphics.Dispose();

            return newBitmap;
        }

        private void SlContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (imageSource != null)
            {
                Bitmap inputImage = BitmapWriter.BitmapImage2Bitmap(imageSource);
                Bitmap outputImage = AdjustContrast(inputImage, (int)slContrast.Value * 5);

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = BitmapWriter.Bitmap2BitmapImage(outputImage);
                cnvSmall.Background = imageBrush;
                cnvBig.Background = imageBrush;
            }
        }

        private Bitmap AdjustContrast(Bitmap inputBitmap, int value)
        {
            Bitmap newBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height);
            Graphics graphics = Graphics.FromImage(newBitmap);

            float finalValue = 0.04f * (float)value;
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                    new float[] { finalValue, 0, 0, 0, 0},
                    new float[] {0, finalValue, 0, 0, 0},
                    new float[] {0, 0, finalValue, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0.001f, 0.001f, 0.001f, 0, 1}
                });

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);
            graphics.DrawImage(inputBitmap, new System.Drawing.Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height),
                0, 0, inputBitmap.Width, inputBitmap.Height, GraphicsUnit.Pixel, imageAttributes);

            imageAttributes.Dispose();
            graphics.Dispose();

            return newBitmap;
        }
    }
}
