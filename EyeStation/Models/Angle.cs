using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;


namespace EyeStation.Models
{
    public class Angle
    {
        public long Id { get; set; }

        public List<Point> Points { get; set; }

        public double Value { get; set; }

        public string ActualCanvas { get; set; }
    }
}
