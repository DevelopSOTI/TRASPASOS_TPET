using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FirebirdSql.Data.FirebirdClient;
using InventarioSalidas.Config.Configuracion;

namespace InventarioSalidas
{
    public partial class FormHistorial : Form
    {
        private DataGridView dgvHistorial;
        private Panel panelHeader;
        private Label lblTitulo;
        private Button btnActualizar;
        private Button btnCerrar;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Label lblDesde;
        private Label lblHasta;
        private Button btnFiltrar;
        private Button btnLimpiarFiltro;
        private TextBox txtBuscar;
        private Label lblBuscar;
        private Label lblTotal;

        public FormHistorial()
        {
            InitializeComponent();
            CargarHistorial();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuración del formulario
            this.Text = "Historial de Traspasos";
            this.ClientSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(236, 240, 241);

            // ========== PANEL HEADER ==========
            panelHeader = new Panel();
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 80;
            panelHeader.BackColor = Color.FromArgb(41, 128, 185);
            this.Controls.Add(panelHeader);

            // Título
            lblTitulo = new Label();
            lblTitulo.Text = "📊 HISTORIAL DE TRASPASOS";
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(20, 25);
            lblTitulo.AutoSize = true;
            panelHeader.Controls.Add(lblTitulo);

            // ========== PANEL DE FILTROS ==========
            Panel panelFiltros = new Panel();
            panelFiltros.Location = new Point(20, 95);
            panelFiltros.Size = new Size(1160, 80);
            panelFiltros.BackColor = Color.White;
            this.Controls.Add(panelFiltros);

            // Buscar
            lblBuscar = new Label();
            lblBuscar.Text = "🔍 Buscar:";
            lblBuscar.Location = new Point(15, 15);
            lblBuscar.Size = new Size(80, 25);
            lblBuscar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblBuscar.TextAlign = ContentAlignment.MiddleLeft;
            panelFiltros.Controls.Add(lblBuscar);

            txtBuscar = new TextBox();
            txtBuscar.Location = new Point(100, 15);
            txtBuscar.Size = new Size(250, 25);
            txtBuscar.Font = new Font("Segoe UI", 10F);
            txtBuscar.BorderStyle = BorderStyle.FixedSingle;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            panelFiltros.Controls.Add(txtBuscar);

            // Fecha Desde
            lblDesde = new Label();
            lblDesde.Text = "Desde:";
            lblDesde.Location = new Point(380, 15);
            lblDesde.Size = new Size(60, 25);
            lblDesde.Font = new Font("Segoe UI", 10F);
            lblDesde.TextAlign = ContentAlignment.MiddleLeft;
            panelFiltros.Controls.Add(lblDesde);

            dtpDesde = new DateTimePicker();
            dtpDesde.Location = new Point(445, 15);
            dtpDesde.Size = new Size(140, 25);
            dtpDesde.Font = new Font("Segoe UI", 9F);
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Value = DateTime.Now.AddMonths(-1);
            panelFiltros.Controls.Add(dtpDesde);

            // Fecha Hasta
            lblHasta = new Label();
            lblHasta.Text = "Hasta:";
            lblHasta.Location = new Point(600, 15);
            lblHasta.Size = new Size(50, 25);
            lblHasta.Font = new Font("Segoe UI", 10F);
            lblHasta.TextAlign = ContentAlignment.MiddleLeft;
            panelFiltros.Controls.Add(lblHasta);

            dtpHasta = new DateTimePicker();
            dtpHasta.Location = new Point(655, 15);
            dtpHasta.Size = new Size(140, 25);
            dtpHasta.Font = new Font("Segoe UI", 9F);
            dtpHasta.Format = DateTimePickerFormat.Short;
            panelFiltros.Controls.Add(dtpHasta);

            // Botón Filtrar
            btnFiltrar = new Button();
            btnFiltrar.Text = "🔎 Filtrar";
            btnFiltrar.Location = new Point(810, 12);
            btnFiltrar.Size = new Size(120, 30);
            btnFiltrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnFiltrar.BackColor = Color.FromArgb(52, 152, 219);
            btnFiltrar.ForeColor = Color.White;
            btnFiltrar.FlatStyle = FlatStyle.Flat;
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnFiltrar.Cursor = Cursors.Hand;
            btnFiltrar.Click += BtnFiltrar_Click;
            panelFiltros.Controls.Add(btnFiltrar);

            // Botón Limpiar Filtro
            btnLimpiarFiltro = new Button();
            btnLimpiarFiltro.Text = "✕ Limpiar";
            btnLimpiarFiltro.Location = new Point(940, 12);
            btnLimpiarFiltro.Size = new Size(100, 30);
            btnLimpiarFiltro.Font = new Font("Segoe UI", 9F);
            btnLimpiarFiltro.BackColor = Color.FromArgb(189, 195, 199);
            btnLimpiarFiltro.ForeColor = Color.FromArgb(44, 62, 80);
            btnLimpiarFiltro.FlatStyle = FlatStyle.Flat;
            btnLimpiarFiltro.FlatAppearance.BorderSize = 0;
            btnLimpiarFiltro.Cursor = Cursors.Hand;
            btnLimpiarFiltro.Click += BtnLimpiarFiltro_Click;
            panelFiltros.Controls.Add(btnLimpiarFiltro);

            // Label Total
            lblTotal = new Label();
            lblTotal.Text = "Total de registros: 0";
            lblTotal.Location = new Point(15, 50);
            lblTotal.Size = new Size(300, 20);
            lblTotal.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblTotal.ForeColor = Color.FromArgb(127, 140, 141);
            panelFiltros.Controls.Add(lblTotal);

            // ========== DATAGRIDVIEW ==========
            dgvHistorial = new DataGridView();
            dgvHistorial.Location = new Point(20, 190);
            dgvHistorial.Size = new Size(1160, 440);
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.ReadOnly = true;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.MultiSelect = false;
            dgvHistorial.BackgroundColor = Color.White;
            dgvHistorial.BorderStyle = BorderStyle.None;
            dgvHistorial.EnableHeadersVisualStyles = false;
            dgvHistorial.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvHistorial.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvHistorial.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvHistorial.ColumnHeadersHeight = 40;
            dgvHistorial.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvHistorial.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvHistorial.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvHistorial.RowTemplate.Height = 35;
            dgvHistorial.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Columnas
            dgvHistorial.Columns.Add("ID", "ID");
            dgvHistorial.Columns["ID"].Width = 40;

            dgvHistorial.Columns.Add("FOLIOS", "Folio Salida");
            dgvHistorial.Columns["FOLIOS"].Width = 120;
            dgvHistorial.Columns["FOLIOS"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            dgvHistorial.Columns.Add("EMPRESAS", "Empresa Origen");
            dgvHistorial.Columns["EMPRESAS"].Width = 150;
            dgvHistorial.Columns["EMPRESAS"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            dgvHistorial.Columns.Add("FOLIOE", "Folio Entrada");
            dgvHistorial.Columns["FOLIOE"].Width = 120;
            dgvHistorial.Columns["FOLIOE"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            dgvHistorial.Columns.Add("EMPRESAE", "Empresa Destino");
            dgvHistorial.Columns["EMPRESAE"].Width = 150;
            dgvHistorial.Columns["EMPRESAE"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            dgvHistorial.Columns.Add("FECHA", "Fecha");
            dgvHistorial.Columns["FECHA"].Width = 100;
            dgvHistorial.Columns["FECHA"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvHistorial.Columns["FECHA"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            dgvHistorial.Columns.Add("USUARIO", "Usuario");
            dgvHistorial.Columns["USUARIO"].Width = 120;
            dgvHistorial.Columns["USUARIO"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            this.Controls.Add(dgvHistorial);

            // ========== BOTONES INFERIORES ==========
            // Botón Actualizar
            btnActualizar = new Button();
            btnActualizar.Text = "🔄 Actualizar";
            btnActualizar.Location = new Point(940, 645);
            btnActualizar.Size = new Size(120, 40);
            btnActualizar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnActualizar.BackColor = Color.FromArgb(52, 152, 219);
            btnActualizar.ForeColor = Color.White;
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Cursor = Cursors.Hand;
            btnActualizar.Click += BtnActualizar_Click;
            this.Controls.Add(btnActualizar);

            // Botón Cerrar
            btnCerrar = new Button();
            btnCerrar.Text = "✕ Cerrar";
            btnCerrar.Location = new Point(1070, 645);
            btnCerrar.Size = new Size(110, 40);
            btnCerrar.Font = new Font("Segoe UI", 11F);
            btnCerrar.BackColor = Color.FromArgb(189, 195, 199);
            btnCerrar.ForeColor = Color.FromArgb(44, 62, 80);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Click += BtnCerrar_Click;
            this.Controls.Add(btnCerrar);

            this.ResumeLayout(false);
        }

        private void CargarHistorial(string filtroFecha = "")
        {
            try
            {
                ConexionEscaner con = new ConexionEscaner();

                if (!con.ConectarEscaner())
                {
                    MessageBox.Show("No se pudo conectar a la base de datos ESCANER",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string query = @"SELECT ID, FOLIOS, EMPRESAS, FOLIOE, EMPRESAE, FECHA, USUARIO 
                                FROM SALIDAS_ENTRADAS";

                // Agregar filtro de fecha si existe
                if (!string.IsNullOrEmpty(filtroFecha))
                {
                    query += " " + filtroFecha;
                }

                query += " ORDER BY ID DESC";

                FbCommand cmd = new FbCommand(query, con.FBC);

                FbDataAdapter adapter = new FbDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dgvHistorial.Rows.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    dgvHistorial.Rows.Add(
                        row["ID"],
                        row["FOLIOS"],
                        row["EMPRESAS"],
                        row["FOLIOE"],
                        row["EMPRESAE"],
                        Convert.ToDateTime(row["FECHA"]),
                        row["USUARIO"]
                    );
                }

                lblTotal.Text = $"Total de registros: {dgvHistorial.Rows.Count}";

                con.Desconectar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando historial:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            CargarHistorial();
            MessageBox.Show("Datos actualizados correctamente", "Actualización",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime desde = dtpDesde.Value.Date;
                DateTime hasta = dtpHasta.Value.Date;

                if (desde > hasta)
                {
                    MessageBox.Show("La fecha 'Desde' no puede ser mayor que 'Hasta'",
                        "Fechas inválidas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string filtro = $"WHERE FECHA BETWEEN '{desde:yyyy-MM-dd}' AND '{hasta:yyyy-MM-dd}'";
                CargarHistorial(filtro);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error aplicando filtro:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLimpiarFiltro_Click(object sender, EventArgs e)
        {
            dtpDesde.Value = DateTime.Now.AddMonths(-1);
            dtpHasta.Value = DateTime.Now;
            txtBuscar.Clear();
            CargarHistorial();
        }

        private void TxtBuscar_TextChanged(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.ToLower();

            foreach (DataGridViewRow row in dgvHistorial.Rows)
            {
                if (row.IsNewRow) continue;

                string folioS = row.Cells["FOLIOS"].Value?.ToString().ToLower() ?? "";
                string empresaS = row.Cells["EMPRESAS"].Value?.ToString().ToLower() ?? "";
                string folioE = row.Cells["FOLIOE"].Value?.ToString().ToLower() ?? "";
                string empresaE = row.Cells["EMPRESAE"].Value?.ToString().ToLower() ?? "";
                string usuario = row.Cells["USUARIO"].Value?.ToString().ToLower() ?? "";

                row.Visible = folioS.Contains(filtro) ||
                             empresaS.Contains(filtro) ||
                             folioE.Contains(filtro) ||
                             empresaE.Contains(filtro) ||
                             usuario.Contains(filtro);
            }

            // Actualizar contador
            int visibles = dgvHistorial.Rows.Cast<DataGridViewRow>()
                .Count(r => !r.IsNewRow && r.Visible);
            lblTotal.Text = $"Registros mostrados: {visibles}";
        }
    }
}