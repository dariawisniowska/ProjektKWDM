using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeStation.Models
{
    public class StudyDrawing
    {
        public List<Marker> MarkerList { get; set; }

        public List<Angle> AngleList { get; set; }

        public bool Modyfied { get; set; }
    }
}
