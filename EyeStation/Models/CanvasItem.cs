using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EyeStation.Models
{
    class CanvasItem
    {
        public long Id { get; set; }

        public UIElement Element { get; set; }

        public string ActualCanvas { get; set; }

        public CanvasItemType Type { get; set; }

        public CanvasItem(long id, UIElement el, string canvasName, CanvasItemType type)
        {
            this.Id = id;
            this.Element = el;
            this.ActualCanvas = canvasName;
            this.Type = type;
        }
    }
}
