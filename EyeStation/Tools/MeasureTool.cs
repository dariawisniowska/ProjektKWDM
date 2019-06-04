using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace EyeStation.Tools
{
    public static class MeasureTool
    {
        public enum TextBlockColor {
            Blue,
            Mint
        }
        public static Line createLine(Point point1, Point point2)
        {
            var brush = new SolidColorBrush(Color.FromRgb(5, 252, 229));
            return new Line() { X1 = point1.X, Y1 = point1.Y, X2 = point2.X, Y2 = point2.Y, Stroke = brush, StrokeThickness = 3 };
        }

        public static TextBlock createTextBox(TextBlockColor txbColor)
        {
            TextBlock textBlock = new TextBlock();
            Color color = new Color();
            if (txbColor.Equals(TextBlockColor.Blue))
                color = Color.FromRgb(144, 202, 249);
            else
                color = Color.FromRgb(5, 252, 229);
            textBlock.Background = new SolidColorBrush(color);
            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            textBlock.FontSize = 15;

            return textBlock;
        }

        public static double getLengthValue(List<Point> measurePoints, int srcHeight, double actualHeight, double pixelSize)
        {
            double length = 0;
            for (int i = 1; i < measurePoints.Count; i++)
            {
                length += Math.Sqrt((measurePoints[i - 1].X - measurePoints[i].X) * (measurePoints[i - 1].X - measurePoints[i].X)
                            + (measurePoints[i - 1].Y - measurePoints[i].Y) * (measurePoints[i - 1].Y - measurePoints[i].Y));
            }
            return length = pixelSize * length * srcHeight / actualHeight;
        }

        public static TextBlock createTextBlockForLine(List<Point> measurePoints, double length)
        {
            int pointCount = measurePoints.Count;
            TextBlock textBlock = MeasureTool.createTextBox(TextBlockColor.Blue);
            textBlock.Text = " " + Math.Round(length/275) + "mm ";
            Canvas.SetLeft(textBlock, measurePoints[pointCount - 1].X);
            Canvas.SetTop(textBlock, measurePoints[pointCount - 1].Y);

            return textBlock;
        }

        public static double getAngleValue(List<Point> anglePoints) {
            int pointCount = anglePoints.Count;
            double x1 = anglePoints[pointCount - 3].X - anglePoints[pointCount - 2].X;
            double y1 = anglePoints[pointCount - 2].Y - anglePoints[pointCount - 3].Y;
            double x2 = anglePoints[pointCount - 1].X - anglePoints[pointCount - 2].X;
            double y2 = anglePoints[pointCount - 2].Y - anglePoints[pointCount - 1].Y;
            return Math.Acos((x1 * x2 + y1 * y2) / (Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2))) * 180 / Math.PI;
        }

        public static TextBlock createTextBlockForAngle(List<Point> anglePoints, double angle)
        {
            int pointCount = anglePoints.Count;
            TextBlock textBlock = MeasureTool.createTextBox(TextBlockColor.Blue);
            textBlock.Text = " " + Math.Round(angle) + "° ";
            Canvas.SetLeft(textBlock, anglePoints[pointCount - 2].X);
            Canvas.SetTop(textBlock, anglePoints[pointCount - 2].Y);

            return textBlock;
        }

        public static Ellipse createMarker(Point point)
        {
            var brush = new SolidColorBrush(Color.FromRgb(5, 252, 229));
            return new Ellipse() { Margin = new Thickness(point.X - 13, point.Y - 13, 0,0), Height=26, Width=26, Stroke = brush, StrokeThickness = 3 , Fill = brush};
        }

        public static double toRealValue(double actualX, int realHeight, double actualHeight) {
            return actualX * realHeight / actualHeight;
        }

        public static double toActualValue(double realX, int realHeight, double actualHeight)
        {
            return realX * actualHeight / realHeight;
        }

        public static Point toRealPoint(double actualX, double actualY, int realHeight, double actualHeight) {
            Point p = new Point();
            p.X = toRealValue(actualX, realHeight, actualHeight);
            p.Y = toRealValue(actualY, realHeight, actualHeight);
            return p;
        }

        public static Point toActualPoint(double realX, double realY, int realHeight, double actualHeight)
        {
            Point p = new Point();
            p.X = toActualValue(realX, realHeight, actualHeight);
            p.Y = toActualValue(realY, realHeight, actualHeight);
            return p;
        }


    }
}
