using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace EyeStation.Models
{
    class Marker
    {
        public long Id { get; set; }

        public Point point { get; set; }

        public string Description { get; set; }
    }
}
