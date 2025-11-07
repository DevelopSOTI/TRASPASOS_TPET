using FirebirdSql.Data.FirebirdClient;
using InventarioSalidas.Config.Configuracion;
using InventarioSalidas.Modelo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace InventarioSalidas
{
    public partial class FormArticulos : Form
    {
        private string empresa;
        private int almacenId;
        private string empresaDes;

        // Delegado para pasar los artículos seleccionados a Form1
        public delegate void ArticulosSeleccionadosHandler(string clave, string nombre);
        public event ArticulosSeleccionadosHandler ArticulosSeleccionados;

        private Label lblTitulo;
        private DataGridView dgvArticulos;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private Button btnAceptar;
        private Button btnCancelar;
        private Panel panelHeader;
        private Label lblInstrucciones;
        private TextBox txtBuscar;
        private Label lblBuscar;

        public FormArticulos(string empresaOrigen, int almacenSeleccionado, string empresaDestino)
        {
            InitializeComponent();
            empresa = empresaOrigen;
            almacenId = almacenSeleccionado;
            empresaDes = empresaDestino;
          

            CargarArticulos();
            dgvArticulos.CellDoubleClick += DgvArticulos_CellDoubleClick;

            // Permitir selección múltiple de filas
            dgvArticulos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvArticulos.MultiSelect = true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuración del Form
            this.Text = "Seleccionar Artículos";
            this.ClientSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(236, 240, 241);

            // ========== PANEL HEADER ==========
            panelHeader = new Panel();
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 70;
            panelHeader.BackColor = Color.FromArgb(41, 128, 185);
            this.Controls.Add(panelHeader);

            // Título
            lblTitulo = new Label();
            lblTitulo.Text = "📦 SELECCIONAR ARTÍCULOS";
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(20, 20);
            lblTitulo.AutoSize = true;
            panelHeader.Controls.Add(lblTitulo);

            // ========== INSTRUCCIONES ==========
            lblInstrucciones = new Label();
            lblInstrucciones.Text = "Seleccione uno o varios artículos. Doble clic para seleccionar individual.";
            lblInstrucciones.Location = new Point(20, 85);
            lblInstrucciones.Size = new Size(860, 20);
            lblInstrucciones.Font = new Font("Segoe UI", 10F);
            lblInstrucciones.ForeColor = Color.FromArgb(44, 62, 80);
            this.Controls.Add(lblInstrucciones);

            // ========== BUSCADOR ==========
            lblBuscar = new Label();
            lblBuscar.Text = "🔍 Buscar:";
            lblBuscar.Location = new Point(20, 115);
            lblBuscar.Size = new Size(80, 25);
            lblBuscar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblBuscar.ForeColor = Color.FromArgb(44, 62, 80);
            lblBuscar.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblBuscar);

            txtBuscar = new TextBox();
            txtBuscar.Location = new Point(105, 115);
            txtBuscar.Size = new Size(300, 25);
            txtBuscar.Font = new Font("Segoe UI", 10F);
            txtBuscar.BorderStyle = BorderStyle.FixedSingle;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            this.Controls.Add(txtBuscar);

            // ========== DATAGRIDVIEW ==========
            dgvArticulos = new DataGridView();
            dgvArticulos.Location = new Point(20, 155);
            dgvArticulos.Size = new Size(860, 380);
            dgvArticulos.AllowUserToAddRows = false;
            dgvArticulos.AllowUserToDeleteRows = false;
            dgvArticulos.ReadOnly = true;
            dgvArticulos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvArticulos.MultiSelect = true;
            dgvArticulos.BackgroundColor = Color.White;
            dgvArticulos.BorderStyle = BorderStyle.None;
            dgvArticulos.EnableHeadersVisualStyles = false;
            dgvArticulos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            dgvArticulos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvArticulos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvArticulos.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvArticulos.ColumnHeadersHeight = 40;
            dgvArticulos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvArticulos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvArticulos.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvArticulos.RowTemplate.Height = 35;
            dgvArticulos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);
            dgvArticulos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Columnas
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn1.HeaderText = "Clave";
            dataGridViewTextBoxColumn1.Name = "Clave";
            dataGridViewTextBoxColumn1.FillWeight = 20;

            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2.HeaderText = "Nombre del Artículo";
            dataGridViewTextBoxColumn2.Name = "Nombre";
            dataGridViewTextBoxColumn2.FillWeight = 60;

            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3.HeaderText = "Existencia";
            dataGridViewTextBoxColumn3.Name = "Existencia";
            dataGridViewTextBoxColumn3.FillWeight = 20;
            dataGridViewTextBoxColumn3.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvArticulos.Columns.AddRange(new DataGridViewColumn[] {
                dataGridViewTextBoxColumn1,
                dataGridViewTextBoxColumn2,
                dataGridViewTextBoxColumn3
            });

            this.Controls.Add(dgvArticulos);

            // ========== BOTONES ==========
            // Botón Aceptar
            btnAceptar = new Button();
            btnAceptar.Text = "✓ Aceptar Selección";
            btnAceptar.Location = new Point(660, 550);
            btnAceptar.Size = new Size(200, 40);
            btnAceptar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnAceptar.BackColor = Color.FromArgb(46, 204, 113);
            btnAceptar.ForeColor = Color.White;
            btnAceptar.FlatStyle = FlatStyle.Flat;
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Cursor = Cursors.Hand;
            btnAceptar.DialogResult = DialogResult.OK;
            btnAceptar.Click += BtnAceptar_Click;
            btnAceptar.MouseEnter += BtnAceptar_MouseEnter;
            btnAceptar.MouseLeave += BtnAceptar_MouseLeave;
            this.Controls.Add(btnAceptar);

            // Botón Cancelar
            btnCancelar = new Button();
            btnCancelar.Text = "✕ Cancelar";
            btnCancelar.Location = new Point(460, 550);
            btnCancelar.Size = new Size(180, 40);
            btnCancelar.Font = new Font("Segoe UI", 11F);
            btnCancelar.BackColor = Color.FromArgb(189, 195, 199);
            btnCancelar.ForeColor = Color.FromArgb(44, 62, 80);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.MouseEnter += BtnCancelar_MouseEnter;
            btnCancelar.MouseLeave += BtnCancelar_MouseLeave;
            this.Controls.Add(btnCancelar);

            this.ResumeLayout(false);
        }

        // Eventos de hover para botones
        private void BtnAceptar_MouseEnter(object sender, EventArgs e)
        {
            btnAceptar.BackColor = Color.FromArgb(39, 174, 96);
        }

        private void BtnAceptar_MouseLeave(object sender, EventArgs e)
        {
            btnAceptar.BackColor = Color.FromArgb(46, 204, 113);
        }

        private void BtnCancelar_MouseEnter(object sender, EventArgs e)
        {
            btnCancelar.BackColor = Color.FromArgb(149, 165, 166);
        }

        private void BtnCancelar_MouseLeave(object sender, EventArgs e)
        {
            btnCancelar.BackColor = Color.FromArgb(189, 195, 199);
        }

        private void TxtBuscar_TextChanged(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.ToLower();

            foreach (DataGridViewRow row in dgvArticulos.Rows)
            {
                if (row.IsNewRow) continue;

                string clave = row.Cells[0].Value?.ToString().ToLower() ?? "";
                string nombre = row.Cells[1].Value?.ToString().ToLower() ?? "";

                // Mostrar u ocultar según el filtro
                row.Visible = clave.Contains(filtro) || nombre.Contains(filtro);
            }
        }

        private void DgvArticulos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvArticulos.Rows[e.RowIndex];
                string clave = row.Cells[0].Value?.ToString() ?? "";
                string nombre = row.Cells[1].Value?.ToString() ?? "";

                // AQUÍ VALIDAR ANTES DE INVOCAR EL EVENTO
                if (!ValidarExistenciaEnDestino(clave, empresaDes))
                {
                    MessageBox.Show($"El artículo '{clave} - {nombre}' no existe en la empresa destino.\nNo se puede realizar el traspaso.",
                        "Artículo no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // No invocar el evento si no existe
                }

                // Solo si existe en destino, invocar el evento
                ArticulosSeleccionados?.Invoke(clave, nombre);
            }
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (dgvArticulos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor seleccione al menos un artículo.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<string> articulosNoEncontrados = new List<string>();

            // VALIDAR TODOS LOS ARTÍCULOS SELECCIONADOS
            foreach (DataGridViewRow row in dgvArticulos.SelectedRows)
            {
                string clave = row.Cells[0].Value?.ToString() ?? "";
                string nombre = row.Cells[1].Value?.ToString() ?? "";

                if (!ValidarExistenciaEnDestino(clave, empresaDes))
                {
                    articulosNoEncontrados.Add($"{clave} - {nombre}");
                }
            }

            // Si hay artículos no encontrados, mostrar lista y no continuar
            if (articulosNoEncontrados.Count > 0)
            {
                string mensaje = "Los siguientes artículos no existen en la empresa destino:\n\n";
                mensaje += string.Join("\n", articulosNoEncontrados);
                mensaje += "\n\nNo se puede realizar el traspaso de estos artículos.";

                MessageBox.Show(mensaje, "Artículos no encontrados", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Solo procesar artículos que SÍ existen en destino
            foreach (DataGridViewRow row in dgvArticulos.SelectedRows)
            {
                string clave = row.Cells[0].Value?.ToString() ?? "";
                string nombre = row.Cells[1].Value?.ToString() ?? "";

                ArticulosSeleccionados?.Invoke(clave, nombre);
            }

            this.Close();
        }

    
        private Dictionary<string, int> articulosIds = new Dictionary<string, int>();

        private void CargarArticulos()
        {
            try
            {
                ConexionMicrosip con = new ConexionMicrosip();

                if (con.ConectarMicrosip(empresa))
                {
                    string query = @"
                SELECT 
                    A.ARTICULO_ID,
                    CA.CLAVE_ARTICULO AS CLAVE,
                    A.NOMBRE,
                    SUM(S.ENTRADAS_UNIDADES - S.SALIDAS_UNIDADES) AS EXISTENCIA
                FROM ARTICULOS A
                JOIN CLAVES_ARTICULOS CA ON CA.ARTICULO_ID = A.ARTICULO_ID
                JOIN SALDOS_IN S ON S.ARTICULO_ID = A.ARTICULO_ID
                WHERE S.ALMACEN_ID = @ALMACEN_ID
                GROUP BY A.ARTICULO_ID, CA.CLAVE_ARTICULO, A.NOMBRE
                HAVING SUM(S.ENTRADAS_UNIDADES - S.SALIDAS_UNIDADES) > 0
                ORDER BY A.NOMBRE";

                    FbCommand cmd = new FbCommand(query, con.FBC);
                    cmd.Parameters.AddWithValue("@ALMACEN_ID", almacenId);
                    FbDataReader dr = cmd.ExecuteReader();

                    dgvArticulos.Rows.Clear();
                    articulosIds.Clear(); // Limpiar IDs anteriores

                    while (dr.Read())
                    {
                        string clave = dr["CLAVE"].ToString();
                        int articuloId = Convert.ToInt32(dr["ARTICULO_ID"]);

                        dgvArticulos.Rows.Add(
                            clave,
                            dr["NOMBRE"].ToString(),
                            Convert.ToDecimal(dr["EXISTENCIA"]).ToString("N2")
                        );

                        articulosIds[clave] = articuloId; // Guardar el ID del artículo
                    }

                    dr.Close();
                    con.Desconectar();

                    lblInstrucciones.Text = $"Se encontraron {dgvArticulos.Rows.Count} artículos disponibles.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar artículos: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarExistenciaEnDestino(string claveArticulo, string empresaDestino)
        {
            ConexionMicrosip con = new ConexionMicrosip();
            if (con.ConectarMicrosip(empresaDestino))
            {
                string query = "SELECT COUNT(*) FROM CLAVES_ARTICULOS WHERE CLAVE_ARTICULO = @CLAVE";
                FbCommand cmd = new FbCommand(query, con.FBC);
                cmd.Parameters.AddWithValue("@CLAVE", claveArticulo);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                con.Desconectar();
                return count > 0;
            }
            return false;
        }
    }
}