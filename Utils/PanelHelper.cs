using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventarioSalidas
{
    public static class PanelHelper
    {
        public static void DrawRoundedBorder(Panel panel, PaintEventArgs e, Color borderColor, int borderWidth, int cornerRadius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(
                new Rectangle(0, 0, panel.Width - borderWidth, panel.Height - borderWidth), cornerRadius))
            {
                using (Pen pen = new Pen(borderColor, borderWidth))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; // Suavizar bordes
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
