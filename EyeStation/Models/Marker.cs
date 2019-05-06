using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Point = System.Windows.Point;

namespace EyeStation.Models
{
    public class Marker
    {
        public long Id { get; set; }

        public Point Point { get; set; }

        public string Description { get; set; }

        public string ActualCanvas { get; set; }
    }
}
