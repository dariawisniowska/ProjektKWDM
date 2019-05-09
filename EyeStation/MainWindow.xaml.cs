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
			vesselSegmentator = new VesselSegmentator()
			{
				VesselSegmentatioMethodType = VesselSegmentatioMethod.Thresholding
			};
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
		private BitmapImage imageSource;
		private StudyDrawing studyDrawing;
		private Study actualStudy;
		private JavaScriptSerializer jss;

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
						int srcHeight = this.imageSource.PixelHeight;
						double actualHeight = 0;
						if (cnv == cnvBig)
							actualHeight = cnvBig.ActualHeight;
						else
							actualHeight = cnvSmall.ActualHeight;
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
                    double angleValue = MeasureTool.getAngleValue(this.anglePoints);
                    TextBlock textBlock = MeasureTool.getAngleOfActiveLine(this.anglePoints, angleValue);
					cnv.Children.Add(textBlock);
					
                    Angle angle = new Angle();
                    angle.Id = studyDrawing.AngleList.Count + 1;
                    angle.Points = anglePoints;
                    angle.Value = angleValue;
                    angle.ActualCanvas = cnv.Name;
                    studyDrawing.AngleList.Add(angle);
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
					studyDrawing.MarkerList.Add(marker);
				}
			}
		}

		private void drawMarker(Marker marker, Canvas cnv)
		{
			if (cnv == null)
			{
				cnv = getActualCanvas(marker.ActualCanvas);
			}
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
            TextBlock textBlock = MeasureTool.getAngleOfActiveLine(points, angle.Value);
            cnv.Children.Add(textBlock);
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
				default:
					return cnv = cnvSmall;
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
			btnMeasure.IsChecked = false;
			btnAngle.IsChecked = false;
			btnSelect.IsChecked = false;
			btnUnSelect.IsChecked = false;

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
			btnSelect.IsEnabled = true;
			btnUnSelect.IsEnabled = true;
			btnAddMarker.IsEnabled = true;
			btnSaveImage.IsEnabled = true;
			btnAnalysis.IsEnabled = true;
			btnDesription.IsEnabled = true;
			btnReport.IsEnabled = true;
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

			cnvMaskAndImage.Background = ib;
			//TEMPORARY
			vesselSegmentator.SetInput(BitmapWriter.BitmapImage2Bitmap(image));
			vesselSegmentator.Calculate();
			var result = BitmapWriter.GetBitmap(vesselSegmentator.Result);
			ImageBrush ibMask = new ImageBrush();
			ibMask.ImageSource = BitmapWriter.Bitmap2BitmapImage(result);
			cnvMask.Background = ibMask;

			//END TEMPORARTY
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
                    for(int i=0; i<angle.Points.Count; i++)
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
                //tu linie 
                if (refreshStudy)
                {
                    SimpleDialog simpleDialog = new SimpleDialog("", "");
                    if (!showError)
                    {
                        simpleDialog = new SimpleDialog("Sukces", "Wprowadzone zmiany zostały zapisane prawidłowo.");
                    }
                    else
                    {
                        simpleDialog = new SimpleDialog("Bład", "Zapis nie powiodł się.");
                    }
                    simpleDialog.ShowDialog();
                    lvStudy.ItemsSource = serwer.GetStudies();
                    lvStudy.SelectedIndex = index;
                }

            }
		}
	}
}
