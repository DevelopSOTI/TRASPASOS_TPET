using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventarioSalidas.Utils
{
    public static class ButtonHelper
    {
        public static void DrawRoundedButton(Button button, PaintEventArgs e, Color borderColor, int borderWidth, int cornerRadius, Color backColor, Color textColor)
        {
            // Suavizar bordes
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Dibujar fondo redondeado
            using (GraphicsPath path = CreateRoundedRectanglePath(new Rectangle(0, 0, button.Width, button.Height), cornerRadius))
            {
                using (Brush brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillPath(brush, path); // Fondo del botón
                }

                // Dibujar borde redondeado
                using (Pen pen = new Pen(borderColor, borderWidth))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            // Dibujar texto centrado
            TextRenderer.DrawText(
                e.Graphics,
                button.Text,
                button.Font,
                button.ClientRectangle,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        internal static void DrawRoundedButton(Button button, EventArgs e, Color color, int v1, int v2, Color backColor, Color foreColor)
        {
            throw new NotImplementedException();
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            // Bordes redondeados
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
