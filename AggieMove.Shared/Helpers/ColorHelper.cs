using System;

namespace AggieMove.Helpers
{
    public static class ColorHelper
    {
        public static System.Drawing.Color ParseCSSColorAsDrawingColor(string cssString)
        {
            if (cssString.StartsWith("rgb("))
            {
                cssString = cssString.Remove(0, 4);
                cssString = cssString.Remove(cssString.Length - 1, 1);
                string[] components = cssString.Split(',');
                return System.Drawing.Color.FromArgb(
                    byte.Parse(components[0].Trim()),
                    byte.Parse(components[1].Trim()),
                    byte.Parse(components[2].Trim())
                );
            }
            else if (cssString[0] == '#')
            {
                switch (cssString.Length)
                {
                    case 9:
                        {
                            var cuint = Convert.ToUInt32(cssString.Substring(1), 16);
                            var a = (byte)(cuint >> 24);
                            var r = (byte)((cuint >> 16) & 0xff);
                            var g = (byte)((cuint >> 8) & 0xff);
                            var b = (byte)(cuint & 0xff);

                            return System.Drawing.Color.FromArgb(a, r, g, b);
                        }

                    case 7:
                        {
                            var cuint = Convert.ToUInt32(cssString.Substring(1), 16);
                            var r = (byte)((cuint >> 16) & 0xff);
                            var g = (byte)((cuint >> 8) & 0xff);
                            var b = (byte)(cuint & 0xff);

                            return System.Drawing.Color.FromArgb(255, r, g, b);
                        }

                    case 5:
                        {
                            var cuint = Convert.ToUInt16(cssString.Substring(1), 16);
                            var a = (byte)(cuint >> 12);
                            var r = (byte)((cuint >> 8) & 0xf);
                            var g = (byte)((cuint >> 4) & 0xf);
                            var b = (byte)(cuint & 0xf);
                            a = (byte)(a << 4 | a);
                            r = (byte)(r << 4 | r);
                            g = (byte)(g << 4 | g);
                            b = (byte)(b << 4 | b);

                            return System.Drawing.Color.FromArgb(a, r, g, b);
                        }

                    case 4:
                        {
                            var cuint = Convert.ToUInt16(cssString.Substring(1), 16);
                            var r = (byte)((cuint >> 8) & 0xf);
                            var g = (byte)((cuint >> 4) & 0xf);
                            var b = (byte)(cuint & 0xf);
                            r = (byte)(r << 4 | r);
                            g = (byte)(g << 4 | g);
                            b = (byte)(b << 4 | b);

                            return System.Drawing.Color.FromArgb(255, r, g, b);
                        }

                    default: throw new FormatException("The string passed in the cssString argument is not a recognized Color format.");
                }
            }
            else
            {
                try
                {
                    return System.Drawing.Color.FromName(cssString);
                }
                catch
                {
                    return System.Drawing.Color.Black;
                }
            }
        }

        public static Windows.UI.Color ParseCSSColorAsWinUIColor(string cssString)
        {
            var drawingColor = ParseCSSColorAsDrawingColor(cssString);
            return Windows.UI.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }

        public static System.Drawing.Color ToDrawingColor(this Windows.UI.Color winuiColor)
        {
            return System.Drawing.Color.FromArgb(winuiColor.A, winuiColor.R, winuiColor.G, winuiColor.B);
        }

        public static Windows.UI.Color ToWinUIColor(this System.Drawing.Color drawingColor)
        {
            return Windows.UI.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
    }
}
