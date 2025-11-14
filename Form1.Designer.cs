namespace InventarioSalidas
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.DataGridViewTextBoxColumn ClaveOrigen;
        private System.Windows.Forms.DataGridViewTextBoxColumn NombreOrigen;
        private System.Windows.Forms.DataGridViewTextBoxColumn CantidadOrigen;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClaveDestino;
        private System.Windows.Forms.DataGridViewTextBoxColumn NombreDestino;
        private System.Windows.Forms.DataGridViewTextBoxColumn CantidadDestino;
        private System.Windows.Forms.DataGridViewTextBoxColumn CostoUnitario;
        private System.Windows.Forms.DataGridViewTextBoxColumn CostoTotal;
        private System.Windows.Forms.DataGridViewTextBoxColumn CostoUnitarioDestino;
        private System.Windows.Forms.DataGridViewTextBoxColumn CostoTotalDestino;
        private System.Windows.Forms.Button button1;
    }
}