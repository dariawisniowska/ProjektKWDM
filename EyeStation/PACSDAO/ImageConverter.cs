using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EyeStation.PACSDAO
{
    public static class ImageConverter
    {
        public static Bitmap[] gdcmBitmap2Bitmap(gdcm.Bitmap bmjpeg2000)
        {
            // przekonwertuj teraz na bitmapę C#
            uint cols = bmjpeg2000.GetColumns();
            uint rows = bmjpeg2000.GetRows();

            // wartość zwracana - tyle obrazków, ile warstw
            Bitmap[] ret = new Bitmap[1];


            // bufor
            byte[] bufor = new byte[bmjpeg2000.GetBufferLength()];
            if (!bmjpeg2000.GetBuffer(bufor))
                throw new Exception("błąd pobrania bufora");

            // w strumieniu na każdy piksel 2 bajty; tutaj LittleEndian (mnie znaczący bajt wcześniej)

                Bitmap X = new Bitmap((int)rows/3, (int)cols/3);

                double[,] Y = new double[rows, cols];
                double m = 0;
                int stride = (int)cols * 4;
                int size = (int)rows * stride;

                for (int r = 0; r < bufor.Length - 1; r++)
                    {
                        // przeskalujemy potem do wartości max.
                        if (bufor[r] * 256 + bufor[r + 1] > m)
                            m = bufor[r]*256 + bufor[r + 1];
                    }

                // wolniejsza metoda tworzenia bitmapy
                for (int r = 0; r < rows/3; r++)
                    for (int c = 0; c < cols/3; c++)
                    {
                        int index = c*3 * stride + 3 * r*3;

                        if (index + 2 < bufor.Length)
                        {
                            int red = (int)(255 * (bufor[index]*256 + bufor[index + 1]) / m);
                            int green = (int)(255 * (bufor[index + 1] * 256 + bufor[index + 2]) / m);
                            int blue = (int)(255 * (bufor[index + 2] * 256 + bufor[index + 3]) / m);
                            X.SetPixel(r, c, Color.FromArgb(red, green, blue));
                        }
                    }
                // kolejna bitmapa
                ret[0] = X;

            return ret;
        }


        // przekonwertuj do formatu bezstratnego JPEG2000
        // bezpośrednio z http://gdcm.sourceforge.net/html/StandardizeFiles_8cs-example.html
        public static gdcm.Bitmap pxmap2jpeg2000(gdcm.Pixmap px)
        {
            gdcm.ImageChangeTransferSyntax change = new gdcm.ImageChangeTransferSyntax();
            change.SetForce(false);
            change.SetCompressIconImage(false);
            change.SetTransferSyntax(new gdcm.TransferSyntax(gdcm.TransferSyntax.TSType.JPEG2000Lossless));

            change.SetInput(px);
            if (!change.Change())
                throw new Exception("Nie przekonwertowano typu bitmapy na jpeg2000");

            return change.GetOutput();

        }
    }
}
