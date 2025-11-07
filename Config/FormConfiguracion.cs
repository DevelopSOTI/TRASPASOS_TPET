using FirebirdSql.Data.FirebirdClient;
using InventarioSalidas.Utils;
using InventarioSalidas.Config.Configuracion;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Data.Common;
using InventarioSalidas.Configuracion;

namespace InventarioSalidas.Config
{
    public partial class FormConfiguracion : Form
    {
        const string caption = "Mensaje de: Conexiones";
        public bool guardados = false;

        private Panel panelMicrosip;
        private Panel panel1;
        private Panel panel2;
        private Label label3;
        private Label lblServidor;
        private Label lblCarpeta;
        private Label lblContrasena;
        private TextBox tbServidorMicro;
        private TextBox tbCarpetaMicro;
        private TextBox tbContrasenaMicro;
        private Button btnProbar;
        private Button btnGuardar;

        public FormConfiguracion()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuración del Form principal
            this.Text = "Conexión";
            this.Size = new Size(370, 274);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Shown += F_CONFIG_Shown;

            // Panel principal Microsip
            this.panelMicrosip = new Panel();
            this.panelMicrosip.Dock = DockStyle.Fill;
            this.panelMicrosip.Location = new Point(0, 0);
            this.panelMicrosip.Size = new Size(354, 235);
            this.Controls.Add(this.panelMicrosip);

            // Panel superior (header)
            this.panel1 = new Panel();
            this.panel1.BackColor = Color.FromArgb(33, 37, 41);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(0, 0);
            this.panel1.Size = new Size(354, 50);
            this.panelMicrosip.Controls.Add(this.panel1);

            // Label título
            this.label3 = new Label();
            this.label3.AutoSize = true;
            this.label3.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.label3.ForeColor = Color.White;
            this.label3.Location = new Point(30, 9);
            this.label3.Text = "Configurar Conexión a Microsip";
            this.panel1.Controls.Add(this.label3);

            // Label Servidor
            this.lblServidor = new Label();
            this.lblServidor.AutoSize = true;
            this.lblServidor.Location = new Point(55, 80);
            this.lblServidor.Text = "Servidor";
            this.panelMicrosip.Controls.Add(this.lblServidor);

            // TextBox Servidor
            this.tbServidorMicro = new TextBox();
            this.tbServidorMicro.Name = "tbServidorMicro";
            this.tbServidorMicro.Location = new Point(118, 77);
            this.tbServidorMicro.Size = new Size(165, 20);
            this.panelMicrosip.Controls.Add(this.tbServidorMicro);

            // Label Carpeta
            this.lblCarpeta = new Label();
            this.lblCarpeta.AutoSize = true;
            this.lblCarpeta.Location = new Point(15, 115);
            this.lblCarpeta.Text = "Carpeta de datos";
            this.panelMicrosip.Controls.Add(this.lblCarpeta);

            // TextBox Carpeta
            this.tbCarpetaMicro = new TextBox();
            this.tbCarpetaMicro.Name = "tbCarpetaMicro";
            this.tbCarpetaMicro.Location = new Point(118, 112);
            this.tbCarpetaMicro.Size = new Size(165, 20);
            this.panelMicrosip.Controls.Add(this.tbCarpetaMicro);

            // Label Contraseña
            this.lblContrasena = new Label();
            this.lblContrasena.AutoSize = true;
            this.lblContrasena.Location = new Point(42, 149);
            this.lblContrasena.Text = "Contraseña";
            this.panelMicrosip.Controls.Add(this.lblContrasena);

            // TextBox Contraseña
            this.tbContrasenaMicro = new TextBox();
            this.tbContrasenaMicro.Name = "tbContrasenaMicro";
            this.tbContrasenaMicro.Location = new Point(118, 146);
            this.tbContrasenaMicro.Size = new Size(165, 20);
            this.tbContrasenaMicro.PasswordChar = '•';
            this.panelMicrosip.Controls.Add(this.tbContrasenaMicro);

            // Panel inferior (botones)
            this.panel2 = new Panel();
            this.panel2.BackColor = Color.White;
            this.panel2.Dock = DockStyle.Bottom;
            this.panel2.Location = new Point(0, 189);
            this.panel2.Size = new Size(354, 46);
            this.panel2.Paint += panel2_Paint;
            this.panelMicrosip.Controls.Add(this.panel2);

            // Botón Probar
            this.btnProbar = new Button();
            this.btnProbar.Name = "btnProbar";
            this.btnProbar.BackColor = Color.FromArgb(13, 110, 253);
            this.btnProbar.ForeColor = Color.White;
            this.btnProbar.Location = new Point(83, 11);
            this.btnProbar.Size = new Size(97, 23);
            this.btnProbar.Text = "Probar conexión";
            this.btnProbar.UseVisualStyleBackColor = false;
            this.btnProbar.Cursor = Cursors.Hand;
            this.btnProbar.FlatStyle = FlatStyle.Flat;
            this.btnProbar.FlatAppearance.BorderSize = 0;
            this.btnProbar.Click += btnProbar_Click;
            this.btnProbar.Paint += btnProbar_Paint;
            this.btnProbar.MouseEnter += btnProbar_MouseHover;
            this.btnProbar.MouseLeave += btnProbar_MouseLeave;
            this.panel2.Controls.Add(this.btnProbar);

            // Botón Guardar
            this.btnGuardar = new Button();
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.BackColor = Color.FromArgb(13, 110, 253);
            this.btnGuardar.ForeColor = Color.White;
            this.btnGuardar.Location = new Point(208, 11);
            this.btnGuardar.Size = new Size(75, 23);
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Cursor = Cursors.Hand;
            this.btnGuardar.FlatStyle = FlatStyle.Flat;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.Click += btnGuardar_Click;
            this.btnGuardar.Paint += btnProbar_Paint;
            this.btnGuardar.MouseEnter += btnProbar_MouseHover;
            this.btnGuardar.MouseLeave += btnProbar_MouseLeave;
            this.panel2.Controls.Add(this.btnGuardar);

            this.ResumeLayout(false);
        }

        private void btnProbar_Paint(object sender, PaintEventArgs e)
        {
            Button button = (Button)sender;

            // Llama al método `DrawRoundedButton` de la clase `ButtonHelper`
            ButtonHelper.DrawRoundedButton(
                button,
                e,
                Color.FromArgb(13, 110, 253), // Color del margen
                2,                            // Grosor del margen
                10,                           // Radio de las esquinas
                button.BackColor,             // Color de fondo
                button.ForeColor              // Color del texto
            );
        }

        private void btnProbar_MouseHover(object sender, EventArgs e)
        {
            Button button = sender as Button;
            button.BackColor = Color.FromArgb(11, 94, 215); // Color del hover
            Cursor = Cursors.Hand;
        }

        private void btnProbar_MouseLeave(object sender, EventArgs e)
        {
            Button button = sender as Button;
            button.BackColor = Color.FromArgb(13, 110, 253); // Color normal del botón
            Cursor = Cursors.Default;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            PanelHelper.DrawRoundedBorder((Panel)sender, e, Color.FromArgb(210, 210, 210), 2, 15);
        }

        private void btnProbar_Click(object sender, EventArgs e)
        {
            if (ValidarDatosFB())
            {
                if (ProbarConexionFB())
                {
                    if (MessageBox.Show("Conexion a Microsip exitosa.\r\n¿Desea guardar los datos de Configuración?", caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        btnGuardar.PerformClick();
                }
                else
                    MessageBox.Show("Datos incorrectos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Datos incompletos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool ValidarDatosFB()
        {
            bool _estatus = false;
            string _FBContraseña = "", _FBRoot = "", _FBServer = "";

            _FBContraseña = tbContrasenaMicro.Text;
            _FBRoot = tbCarpetaMicro.Text;
            _FBServer = tbServidorMicro.Text;

            if (_FBContraseña.Length > 0)
                if (_FBRoot.Length > 0)
                    if (_FBServer.Length > 0)
                        _estatus = true;
                    else
                        _estatus = false;
                else
                    _estatus = false;
            else
                _estatus = false;

            return _estatus;
        }

        private bool ProbarConexionFB()
        {
            bool _exito = false;
            string conectionString = "",
                  _FBContraseña = tbContrasenaMicro.Text,
                  _FBRoot = tbCarpetaMicro.Text,
                  _FBServer = tbServidorMicro.Text;

            conectionString = @"User=SYSDBA;";
            conectionString += "Password=" + _FBContraseña + ";";
            conectionString += "Database=" + _FBRoot + "System\\Config.FDB" + ";";
            conectionString += "Datasource=" + _FBServer + ";";
            conectionString += "Dialect=3;";
            conectionString += "Charset=ISO8859_1;";
            try
            {
                FbConnection fbc = new FbConnection(conectionString);
                fbc.Open();
                if (fbc.State == ConnectionState.Open)
                {
                    fbc.Close();
                    _exito = true;
                }
                else
                    _exito = false;
            }
            catch
            {
                _exito = false;
            }
            return _exito;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (ValidarDatosFB())
            {
                if (ProbarConexionFB())
                {
                    string _FBContraseña = tbContrasenaMicro.Text,
                          _FBRoot = tbCarpetaMicro.Text,
                          _FBServer = tbServidorMicro.Text;

                    RegistrosWindows _reg = new RegistrosWindows();
                    if (_reg.LeerRegistros(true))
                    {
                        if (_reg.CrearRegistros(true))
                        {
                            _reg.EscribirRegistros("MICRO_PASS", _FBContraseña, false);
                            _reg.EscribirRegistros("MICRO_ROOT", _FBRoot, false);
                            _reg.EscribirRegistros("MICRO_SERV", _FBServer, false);
                            _reg.EscribirRegistros("MICRO_USER", "SYSDBA", false);
                            _reg.EscribirRegistros("MICRO_CONFIG_DB", "CONFIGURADOR_IS", false);
                            MessageBox.Show("Datos Guardados satisfactoriamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            guardados = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Registros no leidos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                    MessageBox.Show("Datos incorrectos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Datos incompletos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void F_CONFIG_Shown(object sender, EventArgs e)
        {
            RegistrosWindows _registros = new RegistrosWindows();

            if (_registros.LeerRegistros(false))
            {
                //Registros de Microsip
                tbCarpetaMicro.Text = string.IsNullOrEmpty(_registros.MICRO_ROOT) ? "" : _registros.MICRO_ROOT.Trim();
                tbServidorMicro.Text = string.IsNullOrEmpty(_registros.MICRO_SERVER) ? "" : _registros.MICRO_SERVER.Trim();
                tbContrasenaMicro.Text = string.IsNullOrEmpty(_registros.MICRO_PASS) ? "" : _registros.MICRO_PASS.Trim();
            }
            else
            {
                _registros.CrearRegistros(false);
            }
        }
    }
}