using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSalidas.Config.Configuracion;
using InventarioSalidas.Modelo;
using FirebirdSql.Data.FirebirdClient;
using InventarioSalidas.Configuracion;
using System.Linq;
using System.Collections.Generic;
using InventarioSalidas.Utils;
using System.Text;

using ApiIn = ApisMicrosip.ApiMspInventExt;
using ApiBa = ApisMicrosip.ApiMspBasicaExt;
using System.IO;
using System.Runtime.InteropServices;


namespace InventarioSalidas
{
    public partial class Form1 : Form
    {
        private Label lblFecha;
        private DateTimePicker dtpFecha;
        private Button btnConfiguracion;
        private Label lblDescripcion;
        private TextBox txtDescripcion;
        private GroupBox gbEmpresaOrigen;
        private Label lblOrigenTexto;
        private ComboBox cboOrigenOrigen;
        private Label lblConceptoOrigen;
        private ComboBox cboConceptoOrigen;
        private Label lblAlmacenOrigen;
        private ComboBox cboAlmacenOrigen;
        private GroupBox gbEmpresaDestino;
        private Label lblDestinoTexto;
        private ComboBox cboOrigenDestino;
        private Label lblConceptoDestino;
        private ComboBox cboConceptoDestino;
        private Label lblAlmacenDestino;
        private ComboBox cboAlmacenDestino;
        private Button btnSeleccionarArticulos;
        private DataGridView dgvOrigen;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridView dgvDestino;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private Button btnGenerar;
        private Panel panelHeader;
        private Label lblTitulo;
        private Button btnQuitarArticulo;
        private Button btnHistorial;
        private CheckBox chkUsarCostos;
        private Label lblCostoInfo;

        private ConexionMicrosip conexionmsp;
        private RegistrosWindows reg;

        // Variables de control para evitar AccessViolationException
        private int dbHandle = -1;
        private int tr_empresa = -1;
        private bool conexionActiva = false;
        private bool inventarioConfigurado = false;
        private int DB_Inv;
        private int API_ENCABEZADO;
        private int API_RENGLON;
        private int connectResult;

        private DateTime fechaInicioDocumento;
       
        public Form1()
        {
            InitializeComponent();
            ConfigurarFormulario();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                conexionmsp = new ConexionMicrosip();
                reg = new RegistrosWindows();

                // Inicializar conexión de forma segura
                InicializarConexionSegura();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en Form1_Load: {ex.Message}\n\nTipo: {ex.GetType().Name}",
                    "Error de Inicialización", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InicializarConexionSegura()
        {
            try
            {
                // 1. Leer registros
                if (!reg.LeerRegistros(false))
                {
                    throw new Exception("No se pudieron leer los registros de configuración");
                }

                // 2. Crear objetos de BD
                dbHandle = ApiBa.NewDB();
                if (dbHandle < 0)
                {
                    throw new Exception("No se pudo crear el objeto de base de datos");
                }

                tr_empresa = ApiBa.NewTrn(dbHandle, 3);
                if (tr_empresa < 0)
                {
                    throw new Exception("No se pudo crear la transacción");
                }

                // 3. Configurar manejo de errores ANTES de cualquier operación
                ApiBa.SetErrorHandling(0, 0);
                ApiIn.inSetErrorHandling(0, 0);

                // 4. Limpiar cualquier transacción pendiente
                LimpiarTransaccionesPendientes();

                // 5. CONEXIÓN INICIAL BÁSICA - Solo para validar que la API funciona
                // NO conectar a ninguna BD específica todavía
                conexionActiva = false; // Marcar como no conectado a empresa específica
                inventarioConfigurado = false;

                // La conexión a empresa específica se hará en ProcesarSoloSalida()
                /*MessageBox.Show("API inicializada correctamente. Listo para procesar salidas.",
                    "Inicialización Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);*/

            }
            catch (Exception ex)
            {
                LimpiarRecursosBD();
                MessageBox.Show($"Error inicializando API: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarTransaccionesPendientes()
        {
            try
            {
                // Verificar si hay transacción activa y limpiarla
                if (ApiBa.TrnInTransaction(tr_empresa) == 1)
                {
                    ApiBa.TrnRollback(tr_empresa);
                }
            }
            catch
            {
                
            }
        }

        private void LimpiarRecursosBD()
        {
            try
            {
                // 1. Limpiar transacciones activas primero
                if (tr_empresa >= 0 && ApiBa.TrnInTransaction(tr_empresa) == 1)
                {
                    try { ApiBa.TrnRollback(tr_empresa); } catch { }
                }

                // 2. Abortar documentos en proceso
                try { ApiIn.AbortaDoctoInventarios(); } catch { }

                // 3. Desconectar BD si está conectada
                if (conexionActiva && dbHandle >= 0)
                {
                    try { ApiBa.DBDisconnect(dbHandle); } catch { }
                }

                // 4. Liberar recursos de API
                try { ApiBa.LiberarRecursos(); } catch { }
            }
            catch { }
            finally
            {
                conexionActiva = false;
                inventarioConfigurado = false;
                dbHandle = -1;
                tr_empresa = -1;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LimpiarRecursosBD();
            base.OnFormClosed(e);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panelHeader = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.btnConfiguracion = new System.Windows.Forms.Button();
            this.btnHistorial = new System.Windows.Forms.Button();
            this.lblFecha = new System.Windows.Forms.Label();
            this.dtpFecha = new System.Windows.Forms.DateTimePicker();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.gbEmpresaOrigen = new System.Windows.Forms.GroupBox();
            this.lblOrigenTexto = new System.Windows.Forms.Label();
            this.cboOrigenOrigen = new System.Windows.Forms.ComboBox();
            this.lblConceptoOrigen = new System.Windows.Forms.Label();
            this.cboConceptoOrigen = new System.Windows.Forms.ComboBox();
            this.lblAlmacenOrigen = new System.Windows.Forms.Label();
            this.cboAlmacenOrigen = new System.Windows.Forms.ComboBox();
            this.gbEmpresaDestino = new System.Windows.Forms.GroupBox();
            this.lblDestinoTexto = new System.Windows.Forms.Label();
            this.cboOrigenDestino = new System.Windows.Forms.ComboBox();
            this.lblConceptoDestino = new System.Windows.Forms.Label();
            this.cboConceptoDestino = new System.Windows.Forms.ComboBox();
            this.lblAlmacenDestino = new System.Windows.Forms.Label();
            this.cboAlmacenDestino = new System.Windows.Forms.ComboBox();
            this.btnSeleccionarArticulos = new System.Windows.Forms.Button();
            this.dgvOrigen = new System.Windows.Forms.DataGridView();
            this.ClaveOrigen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NombreOrigen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CantidadOrigen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CostoUnitario = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CostoTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDestino = new System.Windows.Forms.DataGridView();
            this.ClaveDestino = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NombreDestino = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CantidadDestino = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CostoUnitarioDestino = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CostoTotalDestino = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnGenerar = new System.Windows.Forms.Button();
            this.btnQuitarArticulo = new System.Windows.Forms.Button();
            this.chkUsarCostos = new System.Windows.Forms.CheckBox();
            this.lblCostoInfo = new System.Windows.Forms.Label();
            this.panelHeader.SuspendLayout();
            this.gbEmpresaOrigen.SuspendLayout();
            this.gbEmpresaDestino.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrigen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDestino)).BeginInit();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.panelHeader.Controls.Add(this.button1);
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Controls.Add(this.btnConfiguracion);
            this.panelHeader.Controls.Add(this.btnHistorial);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(900, 70);
            this.panelHeader.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(434, 17);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(140, 36);
            this.button1.TabIndex = 3;
            this.button1.Text = "📱App Movil";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(20, 20);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(397, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "CONTROL DE INVENTARIO - SALIDAS";
            // 
            // btnConfiguracion
            // 
            this.btnConfiguracion.BackColor = System.Drawing.Color.White;
            this.btnConfiguracion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfiguracion.FlatAppearance.BorderSize = 0;
            this.btnConfiguracion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfiguracion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnConfiguracion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnConfiguracion.Location = new System.Drawing.Point(730, 17);
            this.btnConfiguracion.Name = "btnConfiguracion";
            this.btnConfiguracion.Size = new System.Drawing.Size(140, 36);
            this.btnConfiguracion.TabIndex = 1;
            this.btnConfiguracion.Text = "⚙ Configuración";
            this.btnConfiguracion.UseVisualStyleBackColor = false;
            this.btnConfiguracion.Click += new System.EventHandler(this.BtnConfiguracion_Click);
            this.btnConfiguracion.MouseEnter += new System.EventHandler(this.BtnConfiguracion_MouseEnter);
            this.btnConfiguracion.MouseLeave += new System.EventHandler(this.BtnConfiguracion_MouseLeave);
            // 
            // btnHistorial
            // 
            this.btnHistorial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnHistorial.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHistorial.FlatAppearance.BorderSize = 0;
            this.btnHistorial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHistorial.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnHistorial.ForeColor = System.Drawing.Color.White;
            this.btnHistorial.Location = new System.Drawing.Point(580, 17);
            this.btnHistorial.Name = "btnHistorial";
            this.btnHistorial.Size = new System.Drawing.Size(140, 36);
            this.btnHistorial.TabIndex = 2;
            this.btnHistorial.Text = "📊 Historial";
            this.btnHistorial.UseVisualStyleBackColor = false;
            this.btnHistorial.Click += new System.EventHandler(this.BtnHistorial_Click);
            this.btnHistorial.MouseEnter += new System.EventHandler(this.BtnHistorial_MouseEnter);
            this.btnHistorial.MouseLeave += new System.EventHandler(this.BtnHistorial_MouseLeave);
            // 
            // lblFecha
            // 
            this.lblFecha.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFecha.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblFecha.Location = new System.Drawing.Point(12, 90);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(104, 20);
            this.lblFecha.TabIndex = 1;
            this.lblFecha.Text = "Fecha:";
            this.lblFecha.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // dtpFecha
            // 
            this.dtpFecha.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpFecha.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFecha.Location = new System.Drawing.Point(136, 90);
            this.dtpFecha.Name = "dtpFecha";
            this.dtpFecha.Size = new System.Drawing.Size(140, 25);
            this.dtpFecha.TabIndex = 2;
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblDescripcion.Location = new System.Drawing.Point(16, 125);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(100, 20);
            this.lblDescripcion.TabIndex = 3;
            this.lblDescripcion.Text = "Descripción:";
            this.lblDescripcion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescripcion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDescripcion.Location = new System.Drawing.Point(136, 125);
            this.txtDescripcion.Multiline = true;
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(350, 50);
            this.txtDescripcion.TabIndex = 4;
            // 
            // gbEmpresaOrigen
            // 
            this.gbEmpresaOrigen.BackColor = System.Drawing.Color.White;
            this.gbEmpresaOrigen.Controls.Add(this.lblOrigenTexto);
            this.gbEmpresaOrigen.Controls.Add(this.cboOrigenOrigen);
            this.gbEmpresaOrigen.Controls.Add(this.lblConceptoOrigen);
            this.gbEmpresaOrigen.Controls.Add(this.cboConceptoOrigen);
            this.gbEmpresaOrigen.Controls.Add(this.lblAlmacenOrigen);
            this.gbEmpresaOrigen.Controls.Add(this.cboAlmacenOrigen);
            this.gbEmpresaOrigen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbEmpresaOrigen.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbEmpresaOrigen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.gbEmpresaOrigen.Location = new System.Drawing.Point(30, 195);
            this.gbEmpresaOrigen.Name = "gbEmpresaOrigen";
            this.gbEmpresaOrigen.Size = new System.Drawing.Size(400, 200);
            this.gbEmpresaOrigen.TabIndex = 5;
            this.gbEmpresaOrigen.TabStop = false;
            this.gbEmpresaOrigen.Text = "🏢 Empresa Origen";
            // 
            // lblOrigenTexto
            // 
            this.lblOrigenTexto.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblOrigenTexto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblOrigenTexto.Location = new System.Drawing.Point(20, 35);
            this.lblOrigenTexto.Name = "lblOrigenTexto";
            this.lblOrigenTexto.Size = new System.Drawing.Size(100, 20);
            this.lblOrigenTexto.TabIndex = 0;
            this.lblOrigenTexto.Text = "Origen:";
            // 
            // cboOrigenOrigen
            // 
            this.cboOrigenOrigen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOrigenOrigen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboOrigenOrigen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboOrigenOrigen.Location = new System.Drawing.Point(140, 33);
            this.cboOrigenOrigen.Name = "cboOrigenOrigen";
            this.cboOrigenOrigen.Size = new System.Drawing.Size(240, 25);
            this.cboOrigenOrigen.TabIndex = 1;
            this.cboOrigenOrigen.SelectedIndexChanged += new System.EventHandler(this.CboOrigenOrigen_SelectedIndexChanged);
            // 
            // lblConceptoOrigen
            // 
            this.lblConceptoOrigen.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblConceptoOrigen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblConceptoOrigen.Location = new System.Drawing.Point(20, 85);
            this.lblConceptoOrigen.Name = "lblConceptoOrigen";
            this.lblConceptoOrigen.Size = new System.Drawing.Size(100, 20);
            this.lblConceptoOrigen.TabIndex = 2;
            this.lblConceptoOrigen.Text = "Concepto:";
            // 
            // cboConceptoOrigen
            // 
            this.cboConceptoOrigen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboConceptoOrigen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboConceptoOrigen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboConceptoOrigen.Location = new System.Drawing.Point(140, 83);
            this.cboConceptoOrigen.Name = "cboConceptoOrigen";
            this.cboConceptoOrigen.Size = new System.Drawing.Size(240, 25);
            this.cboConceptoOrigen.TabIndex = 3;
            // 
            // lblAlmacenOrigen
            // 
            this.lblAlmacenOrigen.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblAlmacenOrigen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblAlmacenOrigen.Location = new System.Drawing.Point(20, 135);
            this.lblAlmacenOrigen.Name = "lblAlmacenOrigen";
            this.lblAlmacenOrigen.Size = new System.Drawing.Size(100, 20);
            this.lblAlmacenOrigen.TabIndex = 4;
            this.lblAlmacenOrigen.Text = "Almacén:";
            // 
            // cboAlmacenOrigen
            // 
            this.cboAlmacenOrigen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAlmacenOrigen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboAlmacenOrigen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboAlmacenOrigen.Location = new System.Drawing.Point(140, 133);
            this.cboAlmacenOrigen.Name = "cboAlmacenOrigen";
            this.cboAlmacenOrigen.Size = new System.Drawing.Size(240, 25);
            this.cboAlmacenOrigen.TabIndex = 5;
            // 
            // gbEmpresaDestino
            // 
            this.gbEmpresaDestino.BackColor = System.Drawing.Color.White;
            this.gbEmpresaDestino.Controls.Add(this.lblDestinoTexto);
            this.gbEmpresaDestino.Controls.Add(this.cboOrigenDestino);
            this.gbEmpresaDestino.Controls.Add(this.lblConceptoDestino);
            this.gbEmpresaDestino.Controls.Add(this.cboConceptoDestino);
            this.gbEmpresaDestino.Controls.Add(this.lblAlmacenDestino);
            this.gbEmpresaDestino.Controls.Add(this.cboAlmacenDestino);
            this.gbEmpresaDestino.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbEmpresaDestino.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbEmpresaDestino.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.gbEmpresaDestino.Location = new System.Drawing.Point(470, 195);
            this.gbEmpresaDestino.Name = "gbEmpresaDestino";
            this.gbEmpresaDestino.Size = new System.Drawing.Size(400, 200);
            this.gbEmpresaDestino.TabIndex = 6;
            this.gbEmpresaDestino.TabStop = false;
            this.gbEmpresaDestino.Text = "🏭 Empresa Destino (Referencia)";
            // 
            // lblDestinoTexto
            // 
            this.lblDestinoTexto.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDestinoTexto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblDestinoTexto.Location = new System.Drawing.Point(20, 35);
            this.lblDestinoTexto.Name = "lblDestinoTexto";
            this.lblDestinoTexto.Size = new System.Drawing.Size(100, 20);
            this.lblDestinoTexto.TabIndex = 0;
            this.lblDestinoTexto.Text = "Destino:";
            // 
            // cboOrigenDestino
            // 
            this.cboOrigenDestino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOrigenDestino.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboOrigenDestino.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboOrigenDestino.Location = new System.Drawing.Point(140, 33);
            this.cboOrigenDestino.Name = "cboOrigenDestino";
            this.cboOrigenDestino.Size = new System.Drawing.Size(240, 25);
            this.cboOrigenDestino.TabIndex = 1;
            this.cboOrigenDestino.SelectedIndexChanged += new System.EventHandler(this.CboOrigenDestino_SelectedIndexChanged);
            // 
            // lblConceptoDestino
            // 
            this.lblConceptoDestino.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblConceptoDestino.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblConceptoDestino.Location = new System.Drawing.Point(20, 85);
            this.lblConceptoDestino.Name = "lblConceptoDestino";
            this.lblConceptoDestino.Size = new System.Drawing.Size(100, 20);
            this.lblConceptoDestino.TabIndex = 2;
            this.lblConceptoDestino.Text = "Concepto:";
            // 
            // cboConceptoDestino
            // 
            this.cboConceptoDestino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboConceptoDestino.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboConceptoDestino.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboConceptoDestino.Location = new System.Drawing.Point(140, 83);
            this.cboConceptoDestino.Name = "cboConceptoDestino";
            this.cboConceptoDestino.Size = new System.Drawing.Size(240, 25);
            this.cboConceptoDestino.TabIndex = 3;
            // 
            // lblAlmacenDestino
            // 
            this.lblAlmacenDestino.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblAlmacenDestino.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblAlmacenDestino.Location = new System.Drawing.Point(20, 135);
            this.lblAlmacenDestino.Name = "lblAlmacenDestino";
            this.lblAlmacenDestino.Size = new System.Drawing.Size(100, 20);
            this.lblAlmacenDestino.TabIndex = 4;
            this.lblAlmacenDestino.Text = "Almacén:";
            // 
            // cboAlmacenDestino
            // 
            this.cboAlmacenDestino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAlmacenDestino.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboAlmacenDestino.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboAlmacenDestino.Location = new System.Drawing.Point(140, 133);
            this.cboAlmacenDestino.Name = "cboAlmacenDestino";
            this.cboAlmacenDestino.Size = new System.Drawing.Size(240, 25);
            this.cboAlmacenDestino.TabIndex = 5;
            // 
            // btnSeleccionarArticulos
            // 
            this.btnSeleccionarArticulos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnSeleccionarArticulos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSeleccionarArticulos.FlatAppearance.BorderSize = 0;
            this.btnSeleccionarArticulos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSeleccionarArticulos.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnSeleccionarArticulos.ForeColor = System.Drawing.Color.White;
            this.btnSeleccionarArticulos.Location = new System.Drawing.Point(30, 410);
            this.btnSeleccionarArticulos.Name = "btnSeleccionarArticulos";
            this.btnSeleccionarArticulos.Size = new System.Drawing.Size(200, 40);
            this.btnSeleccionarArticulos.TabIndex = 7;
            this.btnSeleccionarArticulos.Text = "📦 Seleccionar Artículos";
            this.btnSeleccionarArticulos.UseVisualStyleBackColor = false;
            this.btnSeleccionarArticulos.Click += new System.EventHandler(this.BtnSeleccionarArticulos_Click);
            this.btnSeleccionarArticulos.MouseEnter += new System.EventHandler(this.BtnSeleccionarArticulos_MouseEnter);
            this.btnSeleccionarArticulos.MouseLeave += new System.EventHandler(this.BtnSeleccionarArticulos_MouseLeave);
            // 
            // dgvOrigen
            // 
            this.dgvOrigen.AllowUserToAddRows = false;
            this.dgvOrigen.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.dgvOrigen.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvOrigen.BackgroundColor = System.Drawing.Color.White;
            this.dgvOrigen.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(5);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvOrigen.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvOrigen.ColumnHeadersHeight = 35;
            this.dgvOrigen.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ClaveOrigen,
            this.NombreOrigen,
            this.CantidadOrigen,
            this.CostoUnitario,
            this.CostoTotal});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvOrigen.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvOrigen.EnableHeadersVisualStyles = false;
            this.dgvOrigen.Location = new System.Drawing.Point(30, 465);
            this.dgvOrigen.MultiSelect = false;
            this.dgvOrigen.Name = "dgvOrigen";
            this.dgvOrigen.RowTemplate.Height = 30;
            this.dgvOrigen.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOrigen.Size = new System.Drawing.Size(400, 260);
            this.dgvOrigen.TabIndex = 8;
            this.dgvOrigen.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvOrigen_CellEndEdit);
            this.dgvOrigen.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvOrigen_CellValueChanged);
            // 
            // ClaveOrigen
            // 
            this.ClaveOrigen.HeaderText = "Clave";
            this.ClaveOrigen.Name = "ClaveOrigen";
            this.ClaveOrigen.ReadOnly = true;
            // 
            // NombreOrigen
            // 
            this.NombreOrigen.HeaderText = "Nombre";
            this.NombreOrigen.Name = "NombreOrigen";
            this.NombreOrigen.ReadOnly = true;
            this.NombreOrigen.Width = 180;
            // 
            // CantidadOrigen
            // 
            this.CantidadOrigen.HeaderText = "Cantidad";
            this.CantidadOrigen.Name = "CantidadOrigen";
            this.CantidadOrigen.Width = 80;
            // 
            // CostoUnitario
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N2";
            this.CostoUnitario.DefaultCellStyle = dataGridViewCellStyle3;
            this.CostoUnitario.HeaderText = "Costo Unit.";
            this.CostoUnitario.Name = "CostoUnitario";
            this.CostoUnitario.Visible = false;
            this.CostoUnitario.Width = 110;
            // 
            // CostoTotal
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Format = "N2";
            this.CostoTotal.DefaultCellStyle = dataGridViewCellStyle4;
            this.CostoTotal.HeaderText = "Costo Total";
            this.CostoTotal.Name = "CostoTotal";
            this.CostoTotal.Visible = false;
            this.CostoTotal.Width = 110;
            // 
            // dgvDestino
            // 
            this.dgvDestino.AllowUserToAddRows = false;
            this.dgvDestino.AllowUserToDeleteRows = false;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.dgvDestino.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvDestino.BackgroundColor = System.Drawing.Color.White;
            this.dgvDestino.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.Padding = new System.Windows.Forms.Padding(5);
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDestino.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvDestino.ColumnHeadersHeight = 35;
            this.dgvDestino.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ClaveDestino,
            this.NombreDestino,
            this.CantidadDestino,
            this.CostoUnitarioDestino,
            this.CostoTotalDestino});
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDestino.DefaultCellStyle = dataGridViewCellStyle11;
            this.dgvDestino.EnableHeadersVisualStyles = false;
            this.dgvDestino.Location = new System.Drawing.Point(470, 465);
            this.dgvDestino.MultiSelect = false;
            this.dgvDestino.Name = "dgvDestino";
            this.dgvDestino.ReadOnly = true;
            this.dgvDestino.RowTemplate.Height = 30;
            this.dgvDestino.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDestino.Size = new System.Drawing.Size(400, 260);
            this.dgvDestino.TabIndex = 9;
            // 
            // ClaveDestino
            // 
            this.ClaveDestino.HeaderText = "Clave";
            this.ClaveDestino.Name = "ClaveDestino";
            this.ClaveDestino.ReadOnly = true;
            // 
            // NombreDestino
            // 
            this.NombreDestino.HeaderText = "Nombre";
            this.NombreDestino.Name = "NombreDestino";
            this.NombreDestino.ReadOnly = true;
            this.NombreDestino.Width = 180;
            // 
            // CantidadDestino
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.CantidadDestino.DefaultCellStyle = dataGridViewCellStyle8;
            this.CantidadDestino.HeaderText = "Cantidad";
            this.CantidadDestino.Name = "CantidadDestino";
            this.CantidadDestino.ReadOnly = true;
            this.CantidadDestino.Width = 80;
            // 
            // CostoUnitarioDestino
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle9.Format = "N2";
            this.CostoUnitarioDestino.DefaultCellStyle = dataGridViewCellStyle9;
            this.CostoUnitarioDestino.HeaderText = "Costo Unit.";
            this.CostoUnitarioDestino.Name = "CostoUnitarioDestino";
            this.CostoUnitarioDestino.ReadOnly = true;
            this.CostoUnitarioDestino.Visible = false;
            this.CostoUnitarioDestino.Width = 110;
            // 
            // CostoTotalDestino
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle10.Format = "N2";
            this.CostoTotalDestino.DefaultCellStyle = dataGridViewCellStyle10;
            this.CostoTotalDestino.HeaderText = "Costo Total";
            this.CostoTotalDestino.Name = "CostoTotalDestino";
            this.CostoTotalDestino.ReadOnly = true;
            this.CostoTotalDestino.Visible = false;
            this.CostoTotalDestino.Width = 110;
            // 
            // btnGenerar
            // 
            this.btnGenerar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGenerar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGenerar.FlatAppearance.BorderSize = 0;
            this.btnGenerar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnGenerar.ForeColor = System.Drawing.Color.White;
            this.btnGenerar.Location = new System.Drawing.Point(320, 740);
            this.btnGenerar.Name = "btnGenerar";
            this.btnGenerar.Size = new System.Drawing.Size(260, 45);
            this.btnGenerar.TabIndex = 10;
            this.btnGenerar.Text = "✓ Generar Salida";
            this.btnGenerar.UseVisualStyleBackColor = false;
            this.btnGenerar.Click += new System.EventHandler(this.BtnGenerar_Click);
            this.btnGenerar.MouseEnter += new System.EventHandler(this.BtnGenerar_MouseEnter);
            this.btnGenerar.MouseLeave += new System.EventHandler(this.BtnGenerar_MouseLeave);
            // 
            // btnQuitarArticulo
            // 
            this.btnQuitarArticulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnQuitarArticulo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuitarArticulo.FlatAppearance.BorderSize = 0;
            this.btnQuitarArticulo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitarArticulo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnQuitarArticulo.ForeColor = System.Drawing.Color.White;
            this.btnQuitarArticulo.Location = new System.Drawing.Point(246, 410);
            this.btnQuitarArticulo.Name = "btnQuitarArticulo";
            this.btnQuitarArticulo.Size = new System.Drawing.Size(184, 40);
            this.btnQuitarArticulo.TabIndex = 11;
            this.btnQuitarArticulo.Text = "🗑 Quitar Artículo";
            this.btnQuitarArticulo.UseVisualStyleBackColor = false;
            this.btnQuitarArticulo.Click += new System.EventHandler(this.BtnQuitarArticulo_Click);
            // 
            // chkUsarCostos
            // 
            this.chkUsarCostos.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkUsarCostos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.chkUsarCostos.Location = new System.Drawing.Point(520, 90);
            this.chkUsarCostos.Name = "chkUsarCostos";
            this.chkUsarCostos.Size = new System.Drawing.Size(250, 25);
            this.chkUsarCostos.TabIndex = 0;
            this.chkUsarCostos.Text = "Usar costos específicos en salida";
            // 
            // lblCostoInfo
            // 
            this.lblCostoInfo.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            this.lblCostoInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblCostoInfo.Location = new System.Drawing.Point(520, 118);
            this.lblCostoInfo.Name = "lblCostoInfo";
            this.lblCostoInfo.Size = new System.Drawing.Size(350, 40);
            this.lblCostoInfo.TabIndex = 1;
            this.lblCostoInfo.Text = "ℹ️ Si está habilitado, deberás capturar costos manualmente en la tabla";
            this.lblCostoInfo.Visible = false;
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(900, 800);
            this.Controls.Add(this.chkUsarCostos);
            this.Controls.Add(this.lblCostoInfo);
            this.Controls.Add(this.btnQuitarArticulo);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.lblFecha);
            this.Controls.Add(this.dtpFecha);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.gbEmpresaOrigen);
            this.Controls.Add(this.gbEmpresaDestino);
            this.Controls.Add(this.btnSeleccionarArticulos);
            this.Controls.Add(this.dgvOrigen);
            this.Controls.Add(this.dgvDestino);
            this.Controls.Add(this.btnGenerar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sistema de Salidas - Inventario";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.gbEmpresaOrigen.ResumeLayout(false);
            this.gbEmpresaDestino.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrigen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDestino)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void ConfigurarFormulario()
        {
            // Cargar datos iniciales
            CargarEmpresas();
            CargarConceptos();
            CargarAlmacenes();
        }

        private void CargarEmpresas()
        {
            try
            {
                RegistrosWindows reg = new RegistrosWindows();

                if (reg.LeerRegistros(false))
                {
                    // Validar que existan los datos de conexión
                    if (string.IsNullOrEmpty(reg.MICRO_SERVER) || string.IsNullOrEmpty(reg.MICRO_ROOT))
                    {
                        MessageBox.Show("No se encontraron datos de configuración.\nPor favor configure la conexión a Microsip.",
                            "Configuración Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        BtnConfiguracion_Click(null, null);
                        return;
                    }

                    ConexionMicrosip con = new ConexionMicrosip();

                    if (con.ConectarFB_Test(reg.MICRO_SERVER, reg.MICRO_ROOT, reg.MICRO_PASS))
                    {
                        // Limpiar ComboBox
                        cboOrigenOrigen.Items.Clear();
                        cboOrigenDestino.Items.Clear();

                        // Consultar empresas
                        FbCommand fb = new FbCommand("SELECT * FROM EMPRESAS ORDER BY NOMBRE_CORTO", con.FBC);
                        FbDataReader fdr = fb.ExecuteReader();

                        while (fdr.Read())
                        {
                            EMPRESAS empresa = new EMPRESAS
                            {
                                NOMBRE = Convert.ToString(fdr["NOMBRE_CORTO"]),
                                ID = Convert.ToInt32(fdr["EMPRESA_ID"])
                            };

                            cboOrigenOrigen.Items.Add(empresa);
                            cboOrigenDestino.Items.Add(empresa);
                        }

                        fdr.Close();
                        con.Desconectar();

                        // Seleccionar primer elemento si hay datos
                        if (cboOrigenOrigen.Items.Count > 0)
                        {
                            cboOrigenOrigen.SelectedIndex = 0;
                            cboOrigenDestino.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        MessageBox.Show("No se pudo conectar a la base de datos de Microsip.\nVerifique la configuración.",
                            "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No se encontraron registros de configuración.\nPor favor configure la conexión.",
                        "Configuración Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    BtnConfiguracion_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las empresas: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Dictionary<string, int> conceptosSalidaIds = new Dictionary<string, int>();
        private Dictionary<string, int> conceptosEntradaIds = new Dictionary<string, int>();

        private void CargarConceptos()
        {
            try
            {
                RegistrosWindows reg = new RegistrosWindows();
                reg.LeerRegistros(false);

                ConexionMicrosip con = new ConexionMicrosip();

                // ---------- CONCEPTOS ORIGEN (SALIDAS) ----------
                if (cboOrigenOrigen.SelectedItem != null)
                {
                    EMPRESAS empresaOrigen = (EMPRESAS)cboOrigenOrigen.SelectedItem;

                    if (con.ConectarMicrosip(empresaOrigen.NOMBRE))
                    {
                        cboConceptoOrigen.Items.Clear();
                        conceptosSalidaIds.Clear(); // Limpiar IDs anteriores

                        string query = "SELECT CONCEPTO_IN_ID, NOMBRE FROM CONCEPTOS_IN WHERE TIPO = 'S' ORDER BY NOMBRE";
                        FbCommand fb = new FbCommand(query, con.FBC);
                        FbDataReader fdr = fb.ExecuteReader();

                        while (fdr.Read())
                        {
                            string nombre = Convert.ToString(fdr["NOMBRE"]);
                            int id = Convert.ToInt32(fdr["CONCEPTO_IN_ID"]);

                            cboConceptoOrigen.Items.Add(nombre);
                            conceptosSalidaIds[nombre] = id; // Guardar el ID
                        }

                        fdr.Close();
                        con.Desconectar();

                        if (cboConceptoOrigen.Items.Count > 0)
                            cboConceptoOrigen.SelectedIndex = 0;
                    }
                }

                // ---------- CONCEPTOS DESTINO (ENTRADAS) - Solo para referencia ----------
                if (cboOrigenDestino.SelectedItem != null)
                {
                    EMPRESAS empresaDestino = (EMPRESAS)cboOrigenDestino.SelectedItem;

                    if (con.ConectarMicrosip(empresaDestino.NOMBRE))
                    {
                        cboConceptoDestino.Items.Clear();
                        conceptosEntradaIds.Clear(); // Limpiar IDs anteriores

                        string query = "SELECT CONCEPTO_IN_ID, NOMBRE FROM CONCEPTOS_IN WHERE TIPO = 'E' ORDER BY NOMBRE";
                        FbCommand fb = new FbCommand(query, con.FBC);
                        FbDataReader fdr = fb.ExecuteReader();

                        while (fdr.Read())
                        {
                            string nombre = Convert.ToString(fdr["NOMBRE"]);
                            int id = Convert.ToInt32(fdr["CONCEPTO_IN_ID"]);

                            cboConceptoDestino.Items.Add(nombre);
                            conceptosEntradaIds[nombre] = id; // Guardar el ID
                        }

                        fdr.Close();
                        con.Desconectar();

                        if (cboConceptoDestino.Items.Count > 0)
                            cboConceptoDestino.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar conceptos: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarAlmacenes()
        {
            try
            {
                RegistrosWindows reg = new RegistrosWindows();
                reg.LeerRegistros(false);

                ConexionMicrosip con = new ConexionMicrosip();

                // ---------- ALMACENES ORIGEN ----------
                if (cboOrigenOrigen.SelectedItem != null)
                {
                    EMPRESAS empresaOrigen = (EMPRESAS)cboOrigenOrigen.SelectedItem;

                    if (con.ConectarMicrosip(empresaOrigen.NOMBRE))
                    {
                        cboAlmacenOrigen.Items.Clear();

                        string query = "SELECT ALMACEN_ID, NOMBRE FROM ALMACENES ORDER BY NOMBRE";
                        FbCommand fb = new FbCommand(query, con.FBC);
                        FbDataReader fdr = fb.ExecuteReader();

                        while (fdr.Read())
                        {
                            cboAlmacenOrigen.Items.Add(new ComboBoxItem
                            {
                                Text = fdr["NOMBRE"].ToString(),
                                Value = Convert.ToInt32(fdr["ALMACEN_ID"])
                            });
                        }

                        fdr.Close();
                        con.Desconectar();

                        if (cboAlmacenOrigen.Items.Count > 0)
                            cboAlmacenOrigen.SelectedIndex = 0;
                    }
                }

                // ---------- ALMACENES DESTINO - Solo para referencia ----------
                if (cboOrigenDestino.SelectedItem != null)
                {
                    EMPRESAS empresaDestino = (EMPRESAS)cboOrigenDestino.SelectedItem;

                    if (con.ConectarMicrosip(empresaDestino.NOMBRE))
                    {
                        cboAlmacenDestino.Items.Clear();

                        string query = "SELECT ALMACEN_ID, NOMBRE FROM ALMACENES ORDER BY NOMBRE";
                        FbCommand fb = new FbCommand(query, con.FBC);
                        FbDataReader fdr = fb.ExecuteReader();

                        while (fdr.Read())
                        {
                            cboAlmacenDestino.Items.Add(new ComboBoxItem
                            {
                                Text = fdr["NOMBRE"].ToString(),
                                Value = Convert.ToInt32(fdr["ALMACEN_ID"])
                            });
                        }

                        fdr.Close();
                        con.Desconectar();

                        if (cboAlmacenDestino.Items.Count > 0)
                            cboAlmacenDestino.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar almacenes: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Eventos de hover para btnConfiguracion
        private void BtnConfiguracion_MouseEnter(object sender, EventArgs e)
        {
            btnConfiguracion.BackColor = Color.FromArgb(52, 152, 219);
            btnConfiguracion.ForeColor = Color.White;
        }

        private void BtnConfiguracion_MouseLeave(object sender, EventArgs e)
        {
            btnConfiguracion.BackColor = Color.White;
            btnConfiguracion.ForeColor = Color.FromArgb(41, 128, 185);
        }

        // Eventos de hover para btnSeleccionarArticulos
        private void BtnSeleccionarArticulos_MouseEnter(object sender, EventArgs e)
        {
            btnSeleccionarArticulos.BackColor = Color.FromArgb(41, 128, 185);
        }

        private void BtnSeleccionarArticulos_MouseLeave(object sender, EventArgs e)
        {
            btnSeleccionarArticulos.BackColor = Color.FromArgb(52, 152, 219);
        }

        // Eventos de hover para btnGenerar
        private void BtnGenerar_MouseEnter(object sender, EventArgs e)
        {
            btnGenerar.BackColor = Color.FromArgb(39, 174, 96);
        }

        private void BtnGenerar_MouseLeave(object sender, EventArgs e)
        {
            btnGenerar.BackColor = Color.FromArgb(46, 204, 113);
        }

        private void BtnConfiguracion_Click(object sender, EventArgs e)
        {
            Config.FormConfiguracion formConfiguracion = new Config.FormConfiguracion();
            formConfiguracion.ShowDialog();
        }

        private void BtnQuitarArticulo_Click(object sender, EventArgs e)
        {
            if (dgvOrigen.SelectedRows.Count > 0)
            {
                DialogResult result = MessageBox.Show("¿Deseas eliminar el artículo seleccionado?",
                    "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string clave = dgvOrigen.SelectedRows[0].Cells[0].Value?.ToString();
                    int indexOrigen = dgvOrigen.SelectedRows[0].Index;

                    // Eliminar de origen
                    dgvOrigen.Rows.RemoveAt(indexOrigen);

                    // Eliminar también de destino
                    var filaDestino = dgvDestino.Rows.Cast<DataGridViewRow>()
                        .FirstOrDefault(row => row.Cells[0].Value?.ToString() == clave);
                    if (filaDestino != null)
                        dgvDestino.Rows.Remove(filaDestino);
                }
            }
        }

        private void BtnSeleccionarArticulos_Click(object sender, EventArgs e)
        {
            if (cboOrigenOrigen.SelectedItem is EMPRESAS empresaOrigen &&
                cboOrigenDestino.SelectedItem is EMPRESAS empresaDestino &&
                cboAlmacenOrigen.SelectedItem != null)
            {
                int almacenId = ((ComboBoxItem)cboAlmacenOrigen.SelectedItem).Value;

                // Pasar también la empresa destino
                FormArticulos formArticulos = new FormArticulos(
                    empresaOrigen.NOMBRE,
                    almacenId,
                    empresaDestino.NOMBRE);

                formArticulos.ArticulosSeleccionados += (clave, nombre) =>
                {
                    bool existeOrigen = dgvOrigen.Rows.Cast<DataGridViewRow>()
                        .Any(row => row.Cells[0].Value?.ToString() == clave);

                    if (!existeOrigen)
                    {
                        // Agregar con columnas de costo inicializadas en 0
                        dgvOrigen.Rows.Add(clave, nombre, "1", "0.00", "0.00");

                        // Si está habilitado usar costos, calcular costo promedio automáticamente
                        if (chkUsarCostos.Checked)
                        {
                            int rowIndex = dgvOrigen.Rows.Count - 1;
                            empresaOrigen = (EMPRESAS)cboOrigenOrigen.SelectedItem;

                            // Obtener ID del artículo
                            int articuloId = ObtenerArticuloId(clave, empresaOrigen.NOMBRE);
                            if (articuloId > 0)
                            {
                                double costoPromedio = ObtenerCostoPromedioArticulo(articuloId);
                                dgvOrigen.Rows[rowIndex].Cells["CostoUnitario"].Value = costoPromedio.ToString("N2");
                                dgvOrigen.Rows[rowIndex].Cells["CostoTotal"].Value = (costoPromedio * 1).ToString("N2");
                            }
                        }

                        // Agregar automáticamente a destino CON COSTOS
                        bool existeDestino = dgvDestino.Rows.Cast<DataGridViewRow>()
                            .Any(row => row.Cells[0].Value?.ToString() == clave);

                        if (!existeDestino)
                        {
                            // Obtener los costos del artículo recién agregado en origen
                            int origenIndex = dgvOrigen.Rows.Count - 1;
                            string costoUnit = dgvOrigen.Rows[origenIndex].Cells["CostoUnitario"].Value?.ToString() ?? "0.00";
                            string costoTot = dgvOrigen.Rows[origenIndex].Cells["CostoTotal"].Value?.ToString() ?? "0.00";

                            dgvDestino.Rows.Add(clave, nombre, "1", costoUnit, costoTot);
                        }
                    }
                };

                formArticulos.ShowDialog();
            }
            else
            {
                MessageBox.Show("Debes seleccionar empresa origen, destino y almacén antes de continuar.",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CboOrigenOrigen_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarConceptos();
            CargarAlmacenes();
            dgvOrigen.Rows.Clear();
            dgvDestino.Rows.Clear();
        }

        private void CboOrigenDestino_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarConceptos();
            CargarAlmacenes();
        }

        private void BtnHistorial_Click(object sender, EventArgs e)
        {
            FormHistorial formHistorial = new FormHistorial();
            formHistorial.ShowDialog();
        }

        // Eventos hover para btnHistorial
        private void BtnHistorial_MouseEnter(object sender, EventArgs e)
        {
            btnHistorial.BackColor = Color.FromArgb(41, 128, 185);
        }

        private void BtnHistorial_MouseLeave(object sender, EventArgs e)
        {
            btnHistorial.BackColor = Color.FromArgb(52, 152, 219);
        }

        private void DgvOrigen_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string clave = dgvOrigen.Rows[e.RowIndex].Cells[0].Value?.ToString();
            if (string.IsNullOrEmpty(clave)) return;

            // Buscar la fila correspondiente en destino
            var filaDestino = dgvDestino.Rows.Cast<DataGridViewRow>()
                .FirstOrDefault(row => row.Cells[0].Value?.ToString() == clave);

            if (filaDestino == null) return;

            // Si cambió la cantidad (columna 2)
            if (e.ColumnIndex == 2)
            {
                string nuevaCantidad = dgvOrigen.Rows[e.RowIndex].Cells[2].Value?.ToString();
                filaDestino.Cells[2].Value = nuevaCantidad;

                // Recalcular costo total si cambió la cantidad y hay costo unitario
                if (chkUsarCostos.Checked)
                {
                    if (double.TryParse(nuevaCantidad, out double cantidad) &&
                        double.TryParse(dgvOrigen.Rows[e.RowIndex].Cells["CostoUnitario"].Value?.ToString(), out double costoUnit))
                    {
                        double costoTotal = cantidad * costoUnit;
                        dgvOrigen.Rows[e.RowIndex].Cells["CostoTotal"].Value = costoTotal.ToString("N2");
                        filaDestino.Cells["CostoTotalDestino"].Value = costoTotal.ToString("N2");
                    }
                }
            }

            // Si cambió el costo unitario
            if (e.ColumnIndex == dgvOrigen.Columns["CostoUnitario"].Index)
            {
                string nuevoCostoUnit = dgvOrigen.Rows[e.RowIndex].Cells["CostoUnitario"].Value?.ToString();
                filaDestino.Cells["CostoUnitarioDestino"].Value = nuevoCostoUnit;

                // Recalcular costo total
                if (double.TryParse(nuevoCostoUnit, out double costoUnit) &&
                    double.TryParse(dgvOrigen.Rows[e.RowIndex].Cells[2].Value?.ToString(), out double cantidad))
                {
                    double costoTotal = cantidad * costoUnit;
                    dgvOrigen.Rows[e.RowIndex].Cells["CostoTotal"].Value = costoTotal.ToString("N2");
                    filaDestino.Cells["CostoTotalDestino"].Value = costoTotal.ToString("N2");
                }
            }

            // Si cambió el costo total directamente
            if (e.ColumnIndex == dgvOrigen.Columns["CostoTotal"].Index)
            {
                string nuevoCostoTotal = dgvOrigen.Rows[e.RowIndex].Cells["CostoTotal"].Value?.ToString();
                filaDestino.Cells["CostoTotalDestino"].Value = nuevoCostoTotal;
            }

            ResaltarArticulosSinCostos();
            dgvOrigen.Refresh(); // Forzar redibujado
        }

        private void DgvOrigen_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Validar que la cantidad sea numérica y mayor a 0
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                var cell = dgvOrigen.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (!decimal.TryParse(cell.Value?.ToString(), out decimal cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("La cantidad debe ser un número mayor a 0", "Cantidad inválida",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cell.Value = "1"; // Valor por defecto
                }
            }
        }
        private void ChkUsarCostos_CheckedChanged(object sender, EventArgs e)
        {
            bool usarCostos = chkUsarCostos.Checked;

            // Mostrar/ocultar columnas de costos en ORIGEN
            dgvOrigen.Columns["CostoUnitario"].Visible = usarCostos;
            dgvOrigen.Columns["CostoTotal"].Visible = usarCostos;

            // Mostrar/ocultar columnas de costos en DESTINO
            dgvDestino.Columns["CostoUnitarioDestino"].Visible = usarCostos;
            dgvDestino.Columns["CostoTotalDestino"].Visible = usarCostos;

            // Mostrar/ocultar label informativo
            lblCostoInfo.Visible = usarCostos;

            // Si se desactiva, limpiar valores de costos
            if (!usarCostos)
            {
                foreach (DataGridViewRow row in dgvOrigen.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        row.Cells["CostoUnitario"].Value = "0.00";
                        row.Cells["CostoTotal"].Value = "0.00";
                    }
                }

                foreach (DataGridViewRow row in dgvDestino.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        row.Cells["CostoUnitarioDestino"].Value = "0.00";
                        row.Cells["CostoTotalDestino"].Value = "0.00";
                    }
                }
            }

            if (usarCostos)
            {
                ResaltarArticulosSinCostos();
            }
            else
            {
                lblCostoInfo.Text = "ℹ️ Si está habilitado, deberás capturar costos manualmente en la tabla";
                lblCostoInfo.ForeColor = Color.FromArgb(127, 140, 141);
            }
        }

        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return Text; // Esto hace que se muestre el nombre en el combo
            }
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            if (chkUsarCostos.Checked)
            {
                ResaltarArticulosSinCostos();
            }

            if (!ValidarDatosParaGenerar())
                return;

            // Mostrar indicador de carga
            btnGenerar.Enabled = false;
            btnGenerar.Text = "⏳ Procesando Traspaso...";
            Application.DoEvents();

            try
            {
                var resultado = ProcesarTraspasoCompleto(); // Nueva función para salida + entrada

                if (resultado.Exitoso)
                {
                    string mensaje = $"✓ Traspaso completado exitosamente\n\n";
                    if (!string.IsNullOrEmpty(resultado.FolioSalida))
                        mensaje += $"📤 Salida: {resultado.FolioSalida}\n";
                    if (!string.IsNullOrEmpty(resultado.FolioEntrada))
                        mensaje += $"📥 Entrada: {resultado.FolioEntrada}";

                    MessageBox.Show(mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                }
                else
                {
                    MessageBox.Show($"Error en el traspaso:\n{resultado.Mensaje}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (AccessViolationException ex)
            {
                MessageBox.Show($"Error de acceso a memoria:\n{ex.Message}\n\nPosible causa: API no inicializada correctamente",
                    "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Abortar cualquier documento en proceso
                try
                {
                    ApiIn.AbortaDoctoInventarios();
                }
                catch { }
            }
            finally
            {
                // Restaurar botón
                btnGenerar.Enabled = true;
                btnGenerar.Text = "✓ Generar Traspaso";
            }
        }

        // AGREGAR AL FINAL DEL MÉTODO ProcesarTraspasoCompleto() en Form1.cs

        private ResultadoTraspaso ProcesarTraspasoCompleto()
        {
            var resultado = new ResultadoTraspaso();

            try
            {
                // 1. PROCESAR SALIDA PRIMERO
                /*MessageBox.Show("🔄 Paso 1/3: Procesando salida...", "Progreso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);*/

                bool salidaExitosa = ProcesarSalidaParaTraspaso();
                if (!salidaExitosa)
                {
                    resultado.Mensaje = "Error en la salida del traspaso";
                    return resultado;
                }

                // 2. PROCESAR ENTRADA DESPUÉS
               /* MessageBox.Show("🔄 Paso 2/3: Procesando entrada...", "Progreso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);*/

                bool entradaExitosa = ProcesarEntradaParaTraspaso();
                if (!entradaExitosa)
                {
                    resultado.Mensaje = "Salida exitosa, pero error en la entrada. Revise manualmente.";
                    return resultado;
                }

                // 3. OBTENER FOLIOS DE LOS DOCUMENTOS GENERADOS
                /*MessageBox.Show("🔄 Paso 3/3: Registrando traspaso...", "Progreso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);*/

                var empresaOrigen = (EMPRESAS)cboOrigenOrigen.SelectedItem;
                var empresaDestino = (EMPRESAS)cboOrigenDestino.SelectedItem;
                var fechaMovimiento = dtpFecha.Value;

                int conceptoSalidaId = ObtenerConceptoSalidaId(empresaOrigen.NOMBRE);
                int conceptoEntradaId = ObtenerConceptoEntradaId(empresaDestino.NOMBRE);

                var almacenOrigenItem = (ComboBoxItem)cboAlmacenOrigen.SelectedItem;
                var almacenDestinoItem = (ComboBoxItem)cboAlmacenDestino.SelectedItem;

                // Obtener datos de la salida
                var datosSalida = Utils.DoctoHelpers.ObtenerDatosDocumento(
                    empresaOrigen.NOMBRE,
                    conceptoSalidaId,
                    almacenOrigenItem.Value,
                    fechaMovimiento
                );

                if (datosSalida == null)
                {
                    resultado.Mensaje = "No se pudo obtener el folio de salida";
                    return resultado;
                }

                // Obtener datos de la entrada
                var datosEntrada = Utils.DoctoHelpers.ObtenerDatosDocumento(
                    empresaDestino.NOMBRE,
                    conceptoEntradaId,
                    almacenDestinoItem.Value,
                    fechaMovimiento
                );

                if (datosEntrada == null)
                {
                    resultado.Mensaje = "No se pudo obtener el folio de entrada";
                    return resultado;
                }

                // 4. REGISTRAR EN ESCANER
                bool registroExitoso = Utils.DoctoHelpers.RegistrarTraspaso(
                    datosSalida,
                    empresaOrigen.NOMBRE,
                    datosEntrada,
                    empresaDestino.NOMBRE,
                    fechaMovimiento
                );

                if (!registroExitoso)
                {
                    resultado.Mensaje = "Traspaso completado pero no se pudo registrar en ESCANER. Verifique manualmente.";
                    resultado.FolioSalida = datosSalida.Folio;
                    resultado.FolioEntrada = datosEntrada.Folio;
                    return resultado;
                }

                // 5. ÉXITO COMPLETO
                resultado.Exitoso = true;
                resultado.Mensaje = "Traspaso completado y registrado exitosamente";
                resultado.FolioSalida = datosSalida.Folio;
                resultado.FolioEntrada = datosEntrada.Folio;

                return resultado;
            }
            catch (Exception ex)
            {
                resultado.Mensaje = $"Error durante el traspaso: {ex.Message}";
                return resultado;
            }
        }

        private bool ProcesarSalidaParaTraspaso()
        {
            StringBuilder ErrorMessage = new StringBuilder(512);

            try
            {
                // 1. Obtener empresa origen primero
                var empresaOrigen = (EMPRESAS)cboOrigenOrigen.SelectedItem;
                if (empresaOrigen == null)
                {
                    MessageBox.Show("Debe seleccionar una empresa origen válida",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 2. Leer configuración
                reg.LeerRegistros(false);

                // 3. Construir ruta de BD de empresa origen
                string rutaBDOrigen = $"{reg.MICRO_SERVER}:{reg.MICRO_ROOT}\\{empresaOrigen.NOMBRE}.FDB";

                // 4. Si hay conexión activa, desconectar primero
                if (conexionActiva)
                {
                    try
                    {
                        ApiBa.DBDisconnect(dbHandle);
                        conexionActiva = false;
                        inventarioConfigurado = false;
                    }
                    catch { }
                }

                // 5. Conectar a empresa origen específica
                int connectResult = ApiBa.DBConnect(dbHandle, rutaBDOrigen, reg.MICRO_USER, reg.MICRO_PASS);
                if (connectResult != 0)
                {
                    ApiBa.GetLastErrorMessage(ErrorMessage);
                    MessageBox.Show($"Error conectando a {empresaOrigen.NOMBRE}:\n{ErrorMessage}",
                        "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                conexionActiva = true;

                // 6. Configurar API de inventarios para esta conexión
                int dbInv = ApiIn.SetDBInventarios(dbHandle);
                if (dbInv != 0)
                {
                    ApiIn.inGetLastErrorMessage(ErrorMessage);
                    MessageBox.Show($"Error configurando inventarios:\n{ErrorMessage}",
                        "Error en inventarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                inventarioConfigurado = true;

                // 7. Configurar manejo de errores
                ApiIn.inSetErrorHandling(0, 0);

                // 8. Permitir existencias negativas temporalmente (para traspasos)
                ApiIn.SetReglasInventarios(0);

                // 9. Validar existencias antes de procesar
                if (!ValidarExistenciasDisponibles())
                {
                    return false;
                }

                // 10. Obtener datos del formulario
                var empresaDestino = (EMPRESAS)cboOrigenDestino.SelectedItem;
                var fechaMovimiento = dtpFecha.Value.ToString("d/M/yyyy");
                var descripcion = $"Traspaso hacia {empresaDestino.NOMBRE} - {txtDescripcion.Text}";
                var conceptoId = ObtenerConceptoSalidaId(empresaOrigen.NOMBRE);

                if (conceptoId <= 0)
                {
                    MessageBox.Show("No se pudo obtener el concepto de salida",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var almacenItem = (ComboBoxItem)cboAlmacenOrigen.SelectedItem;
                if (almacenItem == null)
                {
                    MessageBox.Show("Debe seleccionar un almacén origen",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 11. Preparar artículos con validación
                var articulos = PrepararArticulosParaSalida();
                if (!articulos.Any())
                {
                    MessageBox.Show("No se pudieron procesar los artículos seleccionados",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 12. MANEJAR TRANSACCIONES CORRECTAMENTE
                // Verificar si ya hay una transacción activa
                int transactionStatus = ApiBa.TrnInTransaction(tr_empresa);
                if (transactionStatus == 1)
                {
                    // Ya hay transacción activa, hacer rollback primero
                    /*MessageBox.Show("Hay una transacción activa previa. Haciendo rollback...",
                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
                    ApiBa.TrnRollback(tr_empresa);
                }

                // Iniciar nueva transacción
                int resultTrn = ApiBa.TrnStart(tr_empresa);
                if (resultTrn != 0)
                {
                    ApiBa.GetLastErrorMessage(ErrorMessage);
                    MessageBox.Show($"Error iniciando transacción:\n{ErrorMessage}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 13. Crear nueva salida con validación de cada parámetro
                API_ENCABEZADO = ApiIn.NuevaSalida(
                    conceptoId,
                    almacenItem.Value,
                    0,  // AlmacenDestinoId = 0 (no es traspaso interno)
                    fechaMovimiento,
                    "",  // Folio automático (string vacío, no null)
                    descripcion ?? "",  // Asegurar que no sea null
                    0    // CentroCostoId = 0
                );

                if (API_ENCABEZADO != 0)
                {
                    ApiIn.inGetLastErrorMessage(ErrorMessage);
                    string errorDetails = $"Error creando encabezado de salida:\n" +
                                        $"Código: {API_ENCABEZADO}\n" +
                                        $"Detalle: {ErrorMessage}\n" +
                                        $"Empresa: {empresaOrigen.NOMBRE}\n" +
                                        $"Concepto ID: {conceptoId}\n" +
                                        $"Almacén ID: {almacenItem.Value}";

                    MessageBox.Show(errorDetails, "Error en encabezado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApiBa.TrnRollback(tr_empresa);
                    return false;
                }

                // 14. Agregar renglones uno por uno con validación
                foreach (var articulo in articulos)
                {
                    if (!AgregarRenglonSalida(articulo))
                    {
                        // Error ya mostrado, hacer rollback
                        ApiBa.TrnRollback(tr_empresa);
                        return false;
                    }
                }

                // 15. Aplicar salida
                int aplicar = ApiIn.AplicaSalida();
                if (aplicar != 0)
                {
                    ApiIn.inGetLastErrorMessage(ErrorMessage);
                    string errorAplicar = InterpretarErrorAplicacion(aplicar, ErrorMessage.ToString());
                    MessageBox.Show($"Error aplicando salida:\n{errorAplicar}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    ApiBa.TrnRollback(tr_empresa);
                    return false;
                }

                // 16. Confirmar transacción
                int commit = ApiBa.TrnCommit(tr_empresa);
                if (commit != 0)
                {
                    ApiBa.GetLastErrorMessage(ErrorMessage);
                    MessageBox.Show($"Error confirmando transacción:\n{ErrorMessage}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 17. Log de éxito
                LogOperacionExitosa(empresaOrigen.NOMBRE, articulos.Count);

                return true;

            }
            catch (AccessViolationException ex)
            {
                MessageBox.Show($"Error de acceso a memoria:\n{ex.Message}\n\nPosible causa: API no inicializada correctamente",
                    "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Intentar rollback seguro
                try { ApiBa.TrnRollback(tr_empresa); } catch { }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excepción durante el proceso:\n{ex.Message}\n\nTipo: {ex.GetType().Name}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try { ApiBa.TrnRollback(tr_empresa); } catch { }

                return false;
            }
        }

        private bool ProcesarEntradaParaTraspaso()
        {
            StringBuilder ErrorMessage = new StringBuilder(512);

            try
            {
                // 1. Obtener empresa destino
                var empresaDestino = (EMPRESAS)cboOrigenDestino.SelectedItem;
                if (empresaDestino == null)
                {
                    MessageBox.Show("Debe seleccionar una empresa destino válida",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 2. Desconectar de empresa origen y conectar a empresa destino
                if (conexionActiva)
                {
                    ApiBa.DBDisconnect(dbHandle);
                    conexionActiva = false;
                    inventarioConfigurado = false;
                }

                reg.LeerRegistros(false);
                string rutaBDDestino = $"{reg.MICRO_SERVER}:{reg.MICRO_ROOT}\\{empresaDestino.NOMBRE}.FDB";

                int connectResult = ApiBa.DBConnect(dbHandle, rutaBDDestino, reg.MICRO_USER, reg.MICRO_PASS);
                if (connectResult != 0)
                {
                    ApiBa.GetLastErrorMessage(ErrorMessage);
                    MessageBox.Show($"Error conectando a empresa destino {empresaDestino.NOMBRE}:\n{ErrorMessage}",
                        "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                conexionActiva = true;

                // 3. Configurar API de inventarios para empresa destino
                int dbInv = ApiIn.SetDBInventarios(dbHandle);
                if (dbInv != 0)
                {
                    ApiIn.inGetLastErrorMessage(ErrorMessage);
                    MessageBox.Show($"Error configurando inventarios en destino:\n{ErrorMessage}",
                        "Error en inventarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                inventarioConfigurado = true;
                ApiIn.inSetErrorHandling(0, 0);

                // 4. Limpiar y preparar transacción
                int transactionStatus = ApiBa.TrnInTransaction(tr_empresa);
                if (transactionStatus == 1)
                {
                    ApiBa.TrnRollback(tr_empresa);
                }

                int resultTrn = ApiBa.TrnStart(tr_empresa);
                if (resultTrn != 0)
                {
                    ApiBa.GetLastErrorMessage(ErrorMessage);
                    MessageBox.Show($"Error iniciando transacción para entrada:\n{ErrorMessage}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 5. Obtener datos para la entrada
                var fechaMovimiento = dtpFecha.Value.ToString("d/M/yyyy");
                var empresaOrigen = (EMPRESAS)cboOrigenOrigen.SelectedItem;
                var descripcion = $"Traspaso desde {empresaOrigen.NOMBRE} - {txtDescripcion.Text}";

                int conceptoEntradaId = ObtenerConceptoEntradaId(empresaDestino.NOMBRE);
                if (conceptoEntradaId <= 0)
                {
                    MessageBox.Show("No se pudo obtener el concepto de entrada para la empresa destino",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApiBa.TrnRollback(tr_empresa);
                    return false;
                }

                var almacenDestinoItem = (ComboBoxItem)cboAlmacenDestino.SelectedItem;
                if (almacenDestinoItem == null)
                {
                    MessageBox.Show("Debe seleccionar un almacén destino",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApiBa.TrnRollback(tr_empresa);
                    return false;
                }

                // 6. Crear nueva entrada
                int resultEntrada = ApiIn.NuevaEntrada(
                    conceptoEntradaId,
                    almacenDestinoItem.Value,
                    fechaMovimiento,
                    "",  // Folio automático
                    descripcion ?? "",
                    0    // CentroCostoId = 0
                );

                if (resultEntrada != 0)
                {
                    ApiIn.inGetLastErrorMessage(ErrorMessage);
                    string errorDetails = $"Error creando encabezado de entrada:\n" +
                                        $"Código: {resultEntrada}\n" +
                                        $"Detalle: {ErrorMessage}\n" +
                                        $"Empresa: {empresaDestino.NOMBRE}\n" +
                                        $"Concepto ID: {conceptoEntradaId}\n" +
                                        $"Almacén ID: {almacenDestinoItem.Value}";

                    MessageBox.Show(errorDetails, "Error en encabezado de entrada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApiBa.TrnRollback(tr_empresa);
                    return false;
                }

                // 7. Preparar artículos para entrada (en empresa destino)
                var articulos = PrepararArticulosParaEntrada(empresaDestino.NOMBRE);
                if (!articulos.Any())
                {
                    MessageBox.Show("No se pudieron procesar los artículos para la entrada",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApiBa.TrnRollback(tr_empresa);
                    return false;
                }

                // 8. Agregar renglones de entrada
                foreach (var articulo in articulos)
                {
                    if (!AgregarRenglonEntrada(articulo))
                    {
                        ApiBa.TrnRollback(tr_empresa);
                        return false;
                    }
                }

                // 9. Aplicar entrada
                int aplicar = ApiIn.AplicaEntrada();
                if (aplicar != 0)
                {
                    ApiIn.inGetLastErrorMessage(ErrorMessage);
                    string errorAplicar = InterpretarErrorAplicacionEntrada(aplicar, ErrorMessage.ToString());
                    MessageBox.Show($"Error aplicando entrada:\n{errorAplicar}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    ApiBa.TrnRollback(tr_empresa);
                    return false;
                }

                // 10. Confirmar transacción
                int commit = ApiBa.TrnCommit(tr_empresa);
                if (commit != 0)
                {
                    ApiBa.GetLastErrorMessage(ErrorMessage);
                    MessageBox.Show($"Error confirmando transacción de entrada:\n{ErrorMessage}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                LogOperacionExitosa($"ENTRADA - {empresaDestino.NOMBRE}", articulos.Count);
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excepción durante entrada:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try { ApiBa.TrnRollback(tr_empresa); } catch { }
                return false;
            }
        }

        private void DgvOrigen_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Solo aplicar formato si el checkbox de costos está activo
            if (!chkUsarCostos.Checked)
                return;

            if (e.RowIndex < 0 || e.RowIndex >= dgvOrigen.Rows.Count)
                return;

            var row = dgvOrigen.Rows[e.RowIndex];
            if (row.IsNewRow)
                return;

            // Verificar si es una columna de costo
            string columnName = dgvOrigen.Columns[e.ColumnIndex].Name;

            if (columnName == "CostoUnitario" || columnName == "CostoTotal")
            {
                // Obtener el valor
                if (!double.TryParse(e.Value?.ToString(), out double valor) || valor <= 0)
                {
                    // Marcar en ROJO si el costo es 0 o inválido
                    e.CellStyle.BackColor = Color.FromArgb(255, 220, 220); // Rojo claro
                    e.CellStyle.ForeColor = Color.FromArgb(192, 57, 43);   // Rojo oscuro
                    e.CellStyle.Font = new Font(dgvOrigen.Font, FontStyle.Bold);
                }
                else
                {
                    // Marcar en VERDE si el costo es válido
                    e.CellStyle.BackColor = Color.FromArgb(220, 255, 220); // Verde claro
                    e.CellStyle.ForeColor = Color.FromArgb(39, 174, 96);   // Verde oscuro
                }
            }
        }

        private void ResaltarArticulosSinCostos()
        {
            if (!chkUsarCostos.Checked)
                return;

            int articulosSinCosto = 0;

            foreach (DataGridViewRow row in dgvOrigen.Rows)
            {
                if (row.IsNewRow)
                    continue;

                bool costoValido = true;

                // Verificar costo unitario
                if (!double.TryParse(row.Cells["CostoUnitario"].Value?.ToString(), out double costoUnit) || costoUnit <= 0)
                {
                    costoValido = false;
                }

                // Verificar costo total
                if (!double.TryParse(row.Cells["CostoTotal"].Value?.ToString(), out double costoTotal) || costoTotal <= 0)
                {
                    costoValido = false;
                }

                if (!costoValido)
                {
                    articulosSinCosto++;
                    // Marcar toda la fila con borde especial
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(231, 76, 60);
                }
                else
                {
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
                }
            }

            // Actualizar el label informativo con contador
            if (articulosSinCosto > 0)
            {
                lblCostoInfo.Text = $"⚠️ {articulosSinCosto} artículo(s) sin costo capturado. Capture todos los costos antes de generar.";
                lblCostoInfo.ForeColor = Color.FromArgb(231, 76, 60);
            }
            else
            {
                lblCostoInfo.Text = "✓ Todos los artículos tienen costos capturados correctamente";
                lblCostoInfo.ForeColor = Color.FromArgb(39, 174, 96);
            }
        }

        // REEMPLAZAR EL MÉTODO ValidarExistenciasDisponibles() EN Form1.cs

        private bool ValidarExistenciasDisponibles()
        {
            try
            {
                var empresaOrigen = (EMPRESAS)cboOrigenOrigen.SelectedItem;
                var almacenItem = (ComboBoxItem)cboAlmacenOrigen.SelectedItem;

                if (empresaOrigen == null || almacenItem == null)
                {
                    MessageBox.Show("Debe seleccionar empresa y almacén de origen",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var errores = new List<string>();
                ConexionMicrosip con = new ConexionMicrosip();

                if (!con.ConectarMicrosip(empresaOrigen.NOMBRE))
                {
                    MessageBox.Show("No se pudo conectar a la base de datos",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                foreach (DataGridViewRow row in dgvOrigen.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        string clave = row.Cells[0].Value.ToString();
                        double cantidadRequerida = Convert.ToDouble(row.Cells[2].Value ?? "0");

                        if (cantidadRequerida <= 0)
                        {
                            errores.Add($"• {clave}: Cantidad requerida debe ser mayor a 0");
                            continue;
                        }

                        // CONSULTAR EXISTENCIA REAL
                        string query = @"
                    SELECT 
                        SUM(S.ENTRADAS_UNIDADES - S.SALIDAS_UNIDADES) AS EXISTENCIA
                    FROM ARTICULOS A
                    JOIN CLAVES_ARTICULOS CA ON CA.ARTICULO_ID = A.ARTICULO_ID
                    JOIN SALDOS_IN S ON S.ARTICULO_ID = A.ARTICULO_ID
                    WHERE CA.CLAVE_ARTICULO = @CLAVE
                      AND S.ALMACEN_ID = @ALMACEN_ID
                    GROUP BY A.ARTICULO_ID";

                        FbCommand cmd = new FbCommand(query, con.FBC);
                        cmd.Parameters.AddWithValue("@CLAVE", clave);
                        cmd.Parameters.AddWithValue("@ALMACEN_ID", almacenItem.Value);

                        object resultado = cmd.ExecuteScalar();
                        double existenciaReal = resultado != null && resultado != DBNull.Value
                            ? Convert.ToDouble(resultado)
                            : 0;

                        // VALIDAR SI HAY SUFICIENTE EXISTENCIA
                        if (existenciaReal < cantidadRequerida)
                        {
                            errores.Add($"• {clave}: Requiere {cantidadRequerida:N2}, solo hay {existenciaReal:N2} disponibles");
                        }
                    }
                }

                con.Desconectar();

                if (errores.Any())
                {
                    string mensaje = "⚠ EXISTENCIAS INSUFICIENTES:\n\n" + string.Join("\n", errores);
                    mensaje += "\n\nNo se puede completar el traspaso.";

                    MessageBox.Show(mensaje, "Validación de Existencias",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validando existencias: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private List<Utils.ArticuloMovimiento> PrepararArticulosParaSalida()
        {
            var articulos = new List<Utils.ArticuloMovimiento>();

            try
            {
                var empresaOrigen = (EMPRESAS)cboOrigenOrigen.SelectedItem;
                if (empresaOrigen == null) return articulos;

                foreach (DataGridViewRow row in dgvOrigen.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        string clave = row.Cells[0].Value.ToString();

                        if (!double.TryParse(row.Cells[2].Value?.ToString(), out double cantidad) || cantidad <= 0)
                        {
                            MessageBox.Show($"Cantidad inválida para artículo {clave}",
                                "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        int articuloId = ObtenerArticuloIdConConexionActiva(clave);

                        if (articuloId > 0)
                        {
                            double costoUnitario = 0;
                            double costoTotal = 0;

                            // Si usa costos, obtener de la tabla
                            if (chkUsarCostos.Checked)
                            {
                                double.TryParse(row.Cells["CostoUnitario"].Value?.ToString(), out costoUnitario);
                                double.TryParse(row.Cells["CostoTotal"].Value?.ToString(), out costoTotal);
                            }
                            else
                            {
                                // Si no usa costos, dejar en 0
                                costoUnitario = 0;
                                costoTotal = 0;
                            }

                            articulos.Add(new Utils.ArticuloMovimiento
                            {
                                ClaveArticulo = clave,
                                Cantidad = cantidad,
                                ArticuloId = articuloId,
                                CostoUnitario = costoUnitario,
                                CostoTotal = costoTotal
                            });
                        }
                        else
                        {
                            MessageBox.Show($"No se encontró el artículo: {clave}",
                                "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparando artículos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return articulos;
        }

        private List<Utils.ArticuloMovimiento> PrepararArticulosParaEntrada(string empresaDestino)
        {
            var articulos = new List<Utils.ArticuloMovimiento>();

            try
            {
                foreach (DataGridViewRow row in dgvDestino.Rows)
                {
                    if (row.Cells[0].Value != null && !row.IsNewRow)
                    {
                        string clave = row.Cells[0].Value.ToString();

                        if (!double.TryParse(row.Cells[2].Value?.ToString(), out double cantidad) || cantidad <= 0)
                        {
                            MessageBox.Show($"Cantidad inválida para artículo {clave} en entrada",
                                "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        // Obtener ID del artículo en empresa DESTINO
                        int articuloId = ObtenerArticuloIdConConexionActiva(clave);

                        if (articuloId > 0)
                        {
                            double costoUnitario = 0;
                            double costoTotal = 0;

                            // Si usa costos, obtenerlos del dgvDestino
                            if (chkUsarCostos.Checked)
                            {
                                double.TryParse(row.Cells["CostoUnitarioDestino"].Value?.ToString(), out costoUnitario);
                                double.TryParse(row.Cells["CostoTotalDestino"].Value?.ToString(), out costoTotal);
                            }

                            articulos.Add(new Utils.ArticuloMovimiento
                            {
                                ClaveArticulo = clave,
                                Cantidad = cantidad,
                                ArticuloId = articuloId,
                                CostoUnitario = costoUnitario,
                                CostoTotal = costoTotal
                            });
                        }
                        else
                        {
                            MessageBox.Show($"Artículo {clave} no encontrado en empresa destino {empresaDestino}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparando artículos para entrada: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return articulos;
        }

        private int ObtenerArticuloIdConConexionActiva(string claveArticulo)
        {
            try
            {
                // Verificar que tenemos conexión activa
                if (!conexionActiva || ApiBa.DBConnected(dbHandle) != 1)
                {
                    MessageBox.Show("Error: No hay conexión activa a la base de datos",
                        "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }

                // MÉTODO ALTERNATIVO - Usar consulta directa sin parámetros
                // Escapar comillas simples en la clave para prevenir errores SQL
                string claveEscapada = claveArticulo.Replace("'", "''");

                string query = $@"
                    SELECT a.ARTICULO_ID 
                    FROM ARTICULOS a 
                    INNER JOIN CLAVES_ARTICULOS ca ON a.ARTICULO_ID = ca.ARTICULO_ID 
                    WHERE ca.CLAVE_ARTICULO = '{claveEscapada}'";

                int dtHandle = ApiBa.NewDtst(tr_empresa);
                if (dtHandle < 0)
                {
                    MessageBox.Show("Error creando dataset para consulta",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }

                int result = ApiBa.DtstSelQry(dtHandle, query);
                if (result != 0)
                {
                    StringBuilder errorMsg = new StringBuilder(512);
                    ApiBa.GetLastErrorMessage(errorMsg);
                    MessageBox.Show($"Error en consulta SQL: {errorMsg}\n\nQuery: {query}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApiBa.DtstClose(dtHandle);
                    return 0;
                }

                result = ApiBa.DtstOpen(dtHandle);
                if (result != 0)
                {
                    StringBuilder errorMsg = new StringBuilder(512);
                    ApiBa.GetLastErrorMessage(errorMsg);
                    MessageBox.Show($"Error ejecutando consulta: {errorMsg}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApiBa.DtstClose(dtHandle);
                    return 0;
                }

                int articuloId = 0;
                if (ApiBa.DtstEof(dtHandle) == 0) // Si hay datos
                {
                    result = ApiBa.DtstGetFieldAsInteger(dtHandle, "ARTICULO_ID", ref articuloId);
                    if (result != 0)
                    {
                        StringBuilder errorMsg = new StringBuilder(512);
                        ApiBa.GetLastErrorMessage(errorMsg);
                        MessageBox.Show($"Error obteniendo campo ARTICULO_ID: {errorMsg}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        articuloId = 0;
                    }
                    else
                    {
                        // ÉXITO - artículo encontrado
                        /*MessageBox.Show($"✓ Artículo {claveArticulo} encontrado con ID: {articuloId}",
                            "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
                    }
                }
                else
                {
                    // No se encontró el artículo
                    MessageBox.Show($"❌ El artículo '{claveArticulo}' no existe en la empresa {((EMPRESAS)cboOrigenOrigen.SelectedItem).NOMBRE}\n\nQuery ejecutada:\n{query}\n\nVerifique que:\n- La clave sea correcta\n- El artículo exista en esta empresa",
                        "Artículo no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                ApiBa.DtstClose(dtHandle);
                return articuloId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excepción obteniendo ID del artículo {claveArticulo}: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        private bool AgregarRenglonSalida(Utils.ArticuloMovimiento articulo)
        {
            try
            {
                if (articulo.ArticuloId <= 0)
                {
                    MessageBox.Show($"ID de artículo inválido: {articulo.ClaveArticulo}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (articulo.Cantidad <= 0)
                {
                    MessageBox.Show($"Cantidad inválida para artículo: {articulo.ClaveArticulo}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Usar costos del artículo (si no están capturados, serán 0)
                double costoUnitario = articulo.CostoUnitario;
                double costoTotal = articulo.CostoTotal;

                // Agregar renglón con los costos
                int resultado = ApiIn.RenglonSalida(
                    articulo.ArticuloId,
                    articulo.Cantidad,
                    costoUnitario,
                    costoTotal
                );

                if (resultado != 0)
                {
                    StringBuilder errorMsg = new StringBuilder(512);
                    ApiIn.inGetLastErrorMessage(errorMsg);

                    string error = InterpretarErrorRenglon(resultado, errorMsg.ToString());
                    MessageBox.Show($"Error agregando artículo {articulo.ClaveArticulo}:\n{error}",
                        "Error en renglón", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excepción agregando artículo {articulo.ClaveArticulo}: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool AgregarRenglonEntrada(Utils.ArticuloMovimiento articulo)
        {
            try
            {
                if (articulo.ArticuloId <= 0 || articulo.Cantidad <= 0)
                {
                    MessageBox.Show($"Datos inválidos para artículo: {articulo.ClaveArticulo}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Usar costos del artículo (si no están capturados, serán 0)
                double costoUnitario = articulo.CostoUnitario;
                double costoTotal = articulo.CostoTotal;

                // Agregar renglón de entrada CON COSTOS
                int resultado = ApiIn.RenglonEntrada(
                    articulo.ArticuloId,
                    articulo.Cantidad,
                    costoUnitario,
                    costoTotal
                );

                if (resultado != 0)
                {
                    StringBuilder errorMsg = new StringBuilder(512);
                    ApiIn.inGetLastErrorMessage(errorMsg);

                    string error = InterpretarErrorRenglonEntrada(resultado, errorMsg.ToString());
                    MessageBox.Show($"Error agregando artículo a entrada {articulo.ClaveArticulo}:\n{error}",
                        "Error en renglón", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excepción agregando artículo a entrada {articulo.ClaveArticulo}: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private double ObtenerCostoPromedioArticulo(int articuloId)
        {
            try
            {
                int dtHandle = ApiBa.NewDtst(tr_empresa);

                string query = "SELECT COALESCE(COSTO_PROMEDIO, 0) as COSTO FROM ARTICULOS WHERE ARTICULO_ID = ?";

                int result = ApiBa.DtstSelQry(dtHandle, query);
                if (result != 0) return 0;

                result = ApiBa.DtstSetParamAsInteger(dtHandle, "ARTICULO_ID", articuloId);
                if (result != 0) return 0;

                result = ApiBa.DtstOpen(dtHandle);
                if (result != 0) return 0;

                double costo = 0;
                if (ApiBa.DtstEof(dtHandle) == 0)
                {
                    ApiBa.DtstGetFieldAsDouble(dtHandle, "COSTO", ref costo);
                }

                ApiBa.DtstClose(dtHandle);
                return costo;
            }
            catch
            {
                return 0; // Costo 0 si no se puede obtener
            }
        }

        private string InterpretarErrorAplicacion(int codigo, string mensaje)
        {
            switch (codigo)
            {
                case 1: return "No hay salida en proceso";
                case 2: return "No se han registrado artículos en la salida";
                case 3: return "Algunos artículos quedarían con existencia negativa";
                case 98: return "Datos siendo modificados por otro usuario";
                case 99: return $"Error de API básica: {mensaje}";
                default: return $"Código {codigo}: {mensaje}";
            }
        }

        private string InterpretarErrorRenglon(int codigo, string mensaje)
        {
            switch (codigo)
            {
                case 1: return "No hay salida en proceso";
                case 2: return "Artículo inexistente";
                case 3: return "El artículo debe ser almacenable";
                case 7: return "Cantidad fuera de rango";
                case 8: return "Costo unitario fuera de rango";
                case 9: return "Costo total fuera de rango";
                case 12: return "No se permiten cantidades fraccionarias para artículos con series";
                case 98: return "Datos siendo modificados por otro usuario";
                case 99: return $"Error de API básica: {mensaje}";
                default: return $"Código {codigo}: {mensaje}";
            }
        }

        private string InterpretarErrorAplicacionEntrada(int codigo, string mensaje)
        {
            switch (codigo)
            {
                case 1: return "No hay entrada en proceso";
                case 2: return "No se han registrado renglones a la entrada";
                case 3: return "Algunos artículos importados no tienen número de pedimento especificado";
                case 4: return "Algunos artículos quedarían con existencia negativa";
                case 5: return "Algunos números de serie están duplicados";
                case 98: return "Datos siendo modificados por otro usuario";
                case 99: return $"Error de API básica: {mensaje}";
                default: return $"Código {codigo}: {mensaje}";
            }
        }

        private string InterpretarErrorRenglonEntrada(int codigo, string mensaje)
        {
            switch (codigo)
            {
                case 1: return "No hay entrada en proceso";
                case 2: return "Artículo inexistente";
                case 3: return "El artículo debe ser almacenable";
                case 4: return "Los juegos no almacenables sólo se pueden vender o devolver";
                case 5: return "El artículo debe ser Juego almacenable en los ensambles";
                case 6: return "El juego debe tener por lo menos un componente almacenable";
                case 7: return "Valor de unidades fuera de rango";
                case 8: return "Valor de costo unitario fuera de rango";
                case 9: return "Valor de costo total fuera de rango";
                case 10: return "El costo total y el costo unitario deben ser 0 para este concepto";
                case 11: return "El costo total debe ser mayor que 0 en un ajuste con 0 unidades";
                case 12: return "Unidades fraccionarias en artículo de series";
                case 98: return "Datos siendo modificados por otro usuario";
                case 99: return $"Error de API básica: {mensaje}";
                default: return $"Código {codigo}: {mensaje}";
            }
        }

        private void LogOperacionExitosa(string empresa, int cantidadArticulos)
        {
            try
            {
                string mensaje = $"SALIDA EXITOSA - Empresa: {empresa}, Artículos: {cantidadArticulos}, Usuario: {Environment.UserName}, Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                // Log a archivo
                string logPath = Path.Combine(Application.StartupPath, "Logs");
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                string logFile = Path.Combine(logPath, $"Salidas_{DateTime.Now:yyyyMM}.log");
                File.AppendAllText(logFile, mensaje + Environment.NewLine);
            }
            catch
            {
                // Si falla el log, no interrumpir
            }
        }

        // VALIDACIONES ACTUALIZADAS

        private bool ValidarCostosCapturados()
        {
            if (!chkUsarCostos.Checked)
                return true; // Si no usa costos, no validar

            var errores = new List<string>();

            foreach (DataGridViewRow row in dgvOrigen.Rows)
            {
                if (row.Cells[0].Value != null && !row.IsNewRow)
                {
                    string clave = row.Cells[0].Value.ToString();

                    // Validar que el costo unitario exista y sea mayor a 0
                    if (!double.TryParse(row.Cells["CostoUnitario"].Value?.ToString(), out double costoUnit) || costoUnit <= 0)
                    {
                        errores.Add($"• {clave}: Debe capturar un costo unitario mayor a 0");
                    }

                    // Validar que el costo total exista y sea mayor a 0
                    if (!double.TryParse(row.Cells["CostoTotal"].Value?.ToString(), out double costoTotal) || costoTotal <= 0)
                    {
                        errores.Add($"• {clave}: Debe capturar un costo total mayor a 0");
                    }

                    // Validación adicional: verificar coherencia entre costo unitario, cantidad y costo total
                    if (costoUnit > 0 && costoTotal > 0)
                    {
                        if (double.TryParse(row.Cells[2].Value?.ToString(), out double cantidad))
                        {
                            double costoCalculado = costoUnit * cantidad;
                            double diferencia = Math.Abs(costoCalculado - costoTotal);

                            // Permitir una pequeña diferencia por redondeo (0.01)
                            if (diferencia > 0.01)
                            {
                                errores.Add($"• {clave}: Los costos no coinciden (Unit: {costoUnit:N2} × Cant: {cantidad:N2} ≠ Total: {costoTotal:N2})");
                            }
                        }
                    }
                }
            }

            if (errores.Any())
            {
                MessageBox.Show(
                    "⚠️ DEBE CAPTURAR TODOS LOS COSTOS\n\n" +
                    "Cuando está habilitado 'Usar costos específicos', todos los artículos deben tener:\n" +
                    "• Costo unitario mayor a 0\n" +
                    "• Costo total mayor a 0\n\n" +
                    "Errores encontrados:\n\n" +
                    string.Join("\n", errores),
                    "Costos Requeridos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }


        private bool ValidarDatosParaGenerar()
        {
            var errores = new List<string>();

            if (dgvOrigen.Rows.Count == 0)
                errores.Add("- No hay artículos seleccionados");

            if (cboOrigenOrigen.SelectedItem == null)
                errores.Add("- Debe seleccionar la empresa origen");

            if (cboConceptoOrigen.SelectedItem == null)
                errores.Add("- Debe seleccionar el concepto de salida");

            if (cboAlmacenOrigen.SelectedItem == null)
                errores.Add("- Debe seleccionar el almacén origen");

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
                errores.Add("- Debe agregar una descripción");

            // AGREGAR VALIDACIÓN DE COSTOS
            if (chkUsarCostos.Checked && !ValidarCostosCapturados())
                return false;

            // Validar que todas las cantidades sean válidas
            foreach (DataGridViewRow row in dgvOrigen.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    if (!double.TryParse(row.Cells[2].Value?.ToString(), out double cantidad) || cantidad <= 0)
                    {
                        errores.Add($"- Cantidad inválida para artículo {row.Cells[0].Value}");
                    }
                }
            }

            if (errores.Any())
            {
                MessageBox.Show("Corrija los siguientes errores:\n\n" + string.Join("\n", errores),
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // MÉTODOS AUXILIARES EXISTENTES
        private int ObtenerArticuloId(string claveArticulo, string empresa)
        {
            // Los IDs ya se capturan en FormArticulos, pero necesitamos una forma de pasarlos
            // Por ahora, consultar directamente
            try
            {
                ConexionMicrosip con = new ConexionMicrosip();
                if (con.ConectarMicrosip(empresa))
                {
                    string query = "SELECT A.ARTICULO_ID FROM ARTICULOS A " +
                                  "JOIN CLAVES_ARTICULOS CA ON CA.ARTICULO_ID = A.ARTICULO_ID " +
                                  "WHERE CA.CLAVE_ARTICULO = @CLAVE";

                    FbCommand cmd = new FbCommand(query, con.FBC);
                    cmd.Parameters.AddWithValue("@CLAVE", claveArticulo);

                    object result = cmd.ExecuteScalar();
                    con.Desconectar();

                    if (result != null)
                        return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener ID del artículo {claveArticulo}: {ex.Message}");
            }

            return 0; // Si no se encuentra
        }

        private int ObtenerConceptoSalidaId(string empresa)
        {
            if (cboConceptoOrigen.SelectedItem != null)
            {
                string conceptoSeleccionado = cboConceptoOrigen.SelectedItem.ToString();
                if (conceptosSalidaIds.ContainsKey(conceptoSeleccionado))
                    return conceptosSalidaIds[conceptoSeleccionado];
            }
            return 0; // Concepto por defecto
        }

        private int ObtenerConceptoEntradaId(string empresa)
        {
            if (cboConceptoDestino.SelectedItem != null)
            {
                string conceptoSeleccionado = cboConceptoDestino.SelectedItem.ToString();
                if (conceptosEntradaIds.ContainsKey(conceptoSeleccionado))
                    return conceptosEntradaIds[conceptoSeleccionado];
            }
            return 0; // Concepto por defecto
        }

        private void LimpiarFormulario()
        {
            dgvOrigen.Rows.Clear();
            dgvDestino.Rows.Clear();
            txtDescripcion.Clear();
        }

        // Clase para resultado del traspaso
        public class ResultadoTraspaso
        {
            public bool Exitoso { get; set; }
            public string Mensaje { get; set; }
            public string FolioSalida { get; set; }
            public string FolioEntrada { get; set; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            F_HIST_APP app = new F_HIST_APP();
            app.ShowDialog();
        }
    }
}