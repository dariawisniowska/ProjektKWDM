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

namespace EyeStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnFourImage_Click(object sender, RoutedEventArgs e)
        {
            btnFourImage.Visibility = Visibility.Collapsed;
            btnOneImage.Visibility = Visibility.Visible;
            gridOneImage.Visibility = Visibility.Collapsed;
            gridFourImage.Visibility = Visibility.Visible;
        }

        private void btnOneImage_Click(object sender, RoutedEventArgs e)
        {
            btnOneImage.Visibility = Visibility.Collapsed;
            btnFourImage.Visibility = Visibility.Visible;
            gridOneImage.Visibility = Visibility.Visible;
            gridFourImage.Visibility = Visibility.Collapsed;
        }

        private void btnMeasure_Checked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Manualny pomiar długości na obrazie (wysegmentowanym?)
             */

            btnAngle.IsChecked = false;
            btnSelect.IsChecked = false;
            btnUnSelect.IsChecked = false;
            btnAddMarker.IsChecked = false;
        }

        private void btnMeasure_Unchecked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Wyłączenie opcji
             */
        }

        private void btnAngle_Checked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Manualny pomiar kąta na obrazie (wysegmentowanym?)
             */

            btnMeasure.IsChecked = false;
            btnSelect.IsChecked = false;
            btnUnSelect.IsChecked = false;
            btnAddMarker.IsChecked = false;
        }

        private void btnAngle_Unchecked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Wyłączenie opcji
             */
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
            //TO DO:
            /* 
             * Manualne dodanie znacznika na obrazie (wysegmentowanym?)
             */

            btnMeasure.IsChecked = false;
            btnAngle.IsChecked = false;
            btnSelect.IsChecked = false;
            btnUnSelect.IsChecked = false;
        }

        private void btnAddMarker_Unchecked(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Wyłączenie opcji
             */
        }

        private void btnGetImage_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Połączenie z serwerem i wyświetlenie rzeczywistych danych
             */
            selectStudyPanel.Visibility = Visibility.Visible;
            mainPanel.Visibility = Visibility.Collapsed;

            List<Study> items = new List<Study>();
            for (int i = 0; i<25; i++)
                items.Add(new Study() { Id = i, Name = "Jan Kowalski", Description = "Opis"});

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
                showDialog("PACS", "W przyszłości wyświetlę obraz.");

                makeEnableAll();

                selectStudyPanel.Visibility = Visibility.Collapsed;
                mainPanel.Visibility = Visibility.Visible;
                makeEnableAll();
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
            }
        }

        private void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Zapis do png
             */
            showDialog("Zapis do .png", "Obraz zapisano prawidłowo.");
        }

        private void btnSegmentation_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Segmentacja naczyń krwionośnych
             */
            showDialog("Segmentacja", "Segmentacja zakończona powodzeniem.");
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Okno z informacją o podejrzeniu choroby
             */
            string result = "Istnieje podejrzenie retinopatii";
            showDialog("Analiza retinopatii", result);

        }

        private void btnDesription_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Okno z możliwością edycji treści opisu
             */
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            //TO DO:
            /* 
             * Automatyczne generowanie raportu z badania
             */
            showDialog("Raport", "Generowanie raportu zakończone powodzeniem.");
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
        }

        private void showDialog(string caption, string message)
        {
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(message, caption, button, icon);
        }
        
    }
}
