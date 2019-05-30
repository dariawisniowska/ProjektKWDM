using EyeStation.SimpleITK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VesselSegmentatorFilter;

using sitk = itk.simple;

namespace EyeStation.VesselsLengthFilter
{
    public class VesselMeasurements
    {
        Bitmap originalBitmap;
        Bitmap binaryBitmap;
        private List<Point> endPoints;
        private List<Point> branchPoints;
        private List<Point> branchAndEndPoints;
        Graphics graphics;
        public List<int> lengths;

        public VesselMeasurements()
        {
            endPoints = new List<Point>();
            branchPoints = new List<Point>();
            branchAndEndPoints = new List<Point>();
        }

        public void SetInput(byte[][] input)
        {
            string fileName = string.Format("temporary\\{0}.jpg", Guid.NewGuid().ToString());
            BitmapWriter.Save(input, fileName);

            sitk.Image inputImage = ImageFileReader.ReadImage(fileName);
            sitk.Image outputImage = BinaryThinningImageFilter.BinaryThinning(inputImage);

            string newFileName = string.Format("temporary\\{0}.jpg", Guid.NewGuid().ToString());
            ImageFileWriter.WriteImage(outputImage, newFileName);

            originalBitmap = new Bitmap(Image.FromFile(newFileName));
        }

        public Bitmap Calculate()
        {
            FindBranchesAndEnds();
            DetermineLengthFromPointToPoint();

            return originalBitmap;
        }

        private void FindBranchesAndEnds()
        {
            //Binearyzacja obrazu
            binaryBitmap = new Bitmap(originalBitmap);
            for (int i = 0; i < originalBitmap.Width; i++)
            {
                for (int j = 0; j < originalBitmap.Height; j++)
                {
                    Color color = originalBitmap.GetPixel(i, j);
                    binaryBitmap.SetPixel(i, j, (color.R * 0.299 + color.G * 0.578 + color.B * 0.114) > 120 ? Color.FromArgb(255, 255, 255) : Color.FromArgb(0, 0, 0));
                }
            }

            //Wyznaczenie współczynników oraz współrzędnych zakończeń oraz rozwidleń ścieżek
            for (int i = 0; i < binaryBitmap.Width - 1; i++)
            {
                for (int j = 0; j < binaryBitmap.Height - 1; j++)
                {
                    if (binaryBitmap.GetPixel(i, j) == Color.FromArgb(255, 255, 255))
                    {
                        List<Color> neighbors = new List<Color>() {
                            binaryBitmap.GetPixel(i, j - 1), binaryBitmap.GetPixel(i - 1, j - 1), binaryBitmap.GetPixel(i - 1, j),
                            binaryBitmap.GetPixel(i - 1, j + 1), binaryBitmap.GetPixel(i, j + 1), binaryBitmap.GetPixel(i + 1, j + 1),
                            binaryBitmap.GetPixel(i + 1, j), binaryBitmap.GetPixel(i + 1, j - 1), binaryBitmap.GetPixel(i, j - 1) };

                        int coefficient = 0;
                        for (int k = 0; k < neighbors.Count - 1; k++)
                        {
                            coefficient += Convert.ToInt32(neighbors[k].ToArgb() != neighbors[k + 1].ToArgb());
                        }
                        coefficient = coefficient / 2;

                        if (coefficient == 1)
                            endPoints.Add(new Point(i, j));
                        else if (coefficient == 3)
                            branchPoints.Add(new Point(i, j));
                    }
                }
            }

            //Naniesienie wyznaczonych zakończeń oraz rozwidleń
            graphics = Graphics.FromImage(originalBitmap);
            int width = 2, height = 2;
            foreach (Point point in endPoints)
                graphics.DrawEllipse(new Pen(Brushes.Green), point.X - width, point.Y - height, 2 * width, 2 * height);
            foreach (Point point in branchPoints)
                graphics.DrawEllipse(new Pen(Brushes.Green), point.X - width, point.Y - height, 2 * width, 2 * height);

        }

        private void DetermineLengthFromPointToPoint()
        {
            branchAndEndPoints.AddRange(branchPoints);
            branchAndEndPoints.AddRange(endPoints);
            List<Tuple<Point, Point, int>> pointPairs = new List<Tuple<Point, Point, int>>();

            foreach (Point startPoint in branchAndEndPoints)
            {
                List<Point> startNeighbors = new List<Point>() {
                            new Point(startPoint.X, startPoint.Y - 1), new Point(startPoint.X - 1, startPoint.Y), new Point(startPoint.X, startPoint.Y + 1),
                            new Point(startPoint.X + 1, startPoint.Y), new Point(startPoint.X - 1, startPoint.Y - 1), new Point(startPoint.X - 1, startPoint.Y + 1),
                            new Point(startPoint.X + 1, startPoint.Y + 1), new Point(startPoint.X + 1, startPoint.Y - 1) };


                foreach (Point startNeighbor in startNeighbors)
                {
                    if (binaryBitmap.GetPixel(startNeighbor.X, startNeighbor.Y) == Color.FromArgb(255, 255, 255))
                    {
                        Point previousPoint = new Point();
                        Point currentPoint = new Point(startNeighbor.X, startNeighbor.Y);
                        List<Point> visitedPoints = new List<Point>() { startPoint, currentPoint };
                        int length = visitedPoints.Count;

                        while (previousPoint != currentPoint && !(branchAndEndPoints.Any(point => point == currentPoint)))
                        {
                            previousPoint = currentPoint;

                            List<Point> currentNeighbors = new List<Point>(){
                                new Point(currentPoint.X, currentPoint.Y - 1), new Point(currentPoint.X - 1, currentPoint.Y), new Point(currentPoint.X, currentPoint.Y + 1),
                                new Point(currentPoint.X + 1, currentPoint.Y), new Point(currentPoint.X - 1, currentPoint.Y - 1), new Point(currentPoint.X - 1, currentPoint.Y + 1),
                                new Point(currentPoint.X + 1, currentPoint.Y + 1), new Point(currentPoint.X + 1, currentPoint.Y - 1) };

                            foreach (Point currentNeighbor in currentNeighbors)
                            {
                                if (binaryBitmap.GetPixel(currentNeighbor.X, currentNeighbor.Y) == Color.FromArgb(255, 255, 255) && !visitedPoints.Any(point => point == currentNeighbor))
                                {
                                    length++;
                                    currentPoint = currentNeighbor;
                                    visitedPoints.Add(currentPoint);

                                    //Analizowany jest tylko pierwszy punkt sąsiadujący nalżący do ścieżki
                                    break;
                                }
                            }
                        }
                        pointPairs.Add(new Tuple<Point, Point, int>(startPoint, currentPoint, length));
                    }
                }
            }

            //Usunięcie duplikujących się wpisów
            List<Tuple<Point, Point, int>> results2 = new List<Tuple<Point, Point, int>>();

            foreach (Tuple<Point, Point, int> pairPair in pointPairs)
            {
                if (!results2.Any(point => point.Item1 == pairPair.Item2 && point.Item2 == pairPair.Item1) &&
                    !results2.Any(point => point.Item1 == pairPair.Item1 && point.Item2 == pairPair.Item2))
                    results2.Add(pairPair);
            }
            pointPairs = results2;

            lengths = new List<int>();
            foreach (Tuple<Point, Point, int> pairPair in pointPairs)
            {
                float x = (pairPair.Item1.X + pairPair.Item2.X) / 2;
                float y = (pairPair.Item1.Y + pairPair.Item2.Y) / 2;
                graphics.DrawString(pairPair.Item3.ToString(), new Font("Tahoma", 6), Brushes.Red, x - 6, y - 6);
                lengths.Add(Convert.ToInt32(pairPair.Item3.ToString()));
            }
        }
    }
}
