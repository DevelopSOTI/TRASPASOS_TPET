using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventarioSalidas.Utils
{
    internal class C_UTILS
    {

        public void EnableDataGridView(DataGridView dgv)
        {
            // Habilitar el DataGridView
            dgv.Enabled = true;

            // Restaurar colores originales para el estado habilitado
            dgv.DefaultCellStyle.BackColor = SystemColors.Window; // Color de fondo de las celdas
            dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control; // Color de fondo de las cabeceras de columnas
            dgv.RowHeadersDefaultCellStyle.BackColor = SystemColors.Control; // Color de fondo de las cabeceras de filas

            dgv.DefaultCellStyle.ForeColor = SystemColors.ControlText;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            dgv.RowHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;


        }

        //public void DisableDataGridView(DataGridView dgv)
        //{
        //    // Deshabilitar el DataGridView para interacciones
        //    dgv.Enabled = false;

        //    // Cambiar el color para simular un estado deshabilitado
        //    dgv.DefaultCellStyle.BackColor = Color.LightGray; // Color de fondo de las celdas
        //    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.Gray; // Color de fondo de las cabeceras de columnas
        //    dgv.RowHeadersDefaultCellStyle.BackColor = Color.Gray; // Color de fondo de las cabeceras de filas

        //    dgv.DefaultCellStyle.ForeColor = Color.DarkGray;
        //    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        //    dgv.RowHeadersDefaultCellStyle.ForeColor = Color.White;

        //    foreach (DataGridViewRow r in dgv.Rows)
        //    {
        //        if ((bool)r.Cells["SELECCIONAR"].EditedFormattedValue)
        //        {
        //            r.Cells["SELECCIONAR"].Value = 0;
        //        }

        //    }
        //}
    }
}
