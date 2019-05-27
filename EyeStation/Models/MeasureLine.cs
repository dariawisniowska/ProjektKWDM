using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace EyeStation.Models
{
    public class MeasureLine
    {
        public long Id { get; set; }

        public List<Point> Points { get; set; }

        public double Value { get; set; }

        public string ActualCanvas { get; set; }

        public MeasureLine() { }

        public MeasureLine(MeasureLine line)
        {
            this.Id = line.Id;
            this.Points = line.Points;
            this.Value = line.Value;
            this.ActualCanvas = line.ActualCanvas;
        }
    }
}
