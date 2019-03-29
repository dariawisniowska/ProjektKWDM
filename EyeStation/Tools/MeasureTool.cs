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
        public static Line createLine(Point point1, Point point2)
        {
            var brush = new SolidColorBrush(Color.FromRgb(5, 252, 229));
            return new Line() { X1 = point1.X, Y1 = point1.Y, X2 = point2.X, Y2 = point2.Y, Stroke = brush, StrokeThickness = 3 };
        }

        public static TextBlock createTextBox()
        {
            TextBlock textBlock = new TextBlock();

            Color blue = new Color();
            blue = Color.FromRgb(144, 202, 249);
            textBlock.Background = new SolidColorBrush(blue);
            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            textBlock.FontSize = 15;

            return textBlock;
        }

        public static TextBlock getLengthOfActiveLine(List<Point> measurePoints)
        {
            double length = 0;
            int pointCount = measurePoints.Count;
            for (int i = 1; i < measurePoints.Count; i++)
            {
                length += Math.Sqrt((measurePoints[i - 1].X - measurePoints[i].X) * (measurePoints[i - 1].X - measurePoints[i].X)
                            + (measurePoints[i - 1].Y - measurePoints[i].Y) * (measurePoints[i - 1].Y - measurePoints[i].Y));
            }

            TextBlock textBlock = MeasureTool.createTextBox();
            textBlock.Text = " " + Math.Round(length) + "px ";
            Canvas.SetLeft(textBlock, measurePoints[pointCount - 1].X);
            Canvas.SetTop(textBlock, measurePoints[pointCount - 1].Y);

            return textBlock;
        }

        public static TextBlock getAngleOfActiveLine(List<Point> anglePoints)
        {
            int pointCount = anglePoints.Count;
            double x1 = anglePoints[pointCount - 3].X - anglePoints[pointCount - 2].X;
            double y1 = anglePoints[pointCount - 2].Y - anglePoints[pointCount - 3].Y;
            double x2 = anglePoints[pointCount - 1].X - anglePoints[pointCount - 2].X;
            double y2 = anglePoints[pointCount - 2].Y - anglePoints[pointCount - 1].Y;
            double angle = Math.Acos((x1 * x2 + y1 * y2) / (Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2))) * 180 / Math.PI;

            TextBlock textBlock = MeasureTool.createTextBox();
            textBlock.Text = " " + Math.Round(angle) + " stopni ";
            Canvas.SetLeft(textBlock, anglePoints[pointCount - 1].X);
            Canvas.SetTop(textBlock, anglePoints[pointCount - 1].Y);

            return textBlock;
        }
    }
}
