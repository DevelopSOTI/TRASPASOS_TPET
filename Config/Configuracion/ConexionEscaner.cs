using FirebirdSql.Data.FirebirdClient;
using InventarioSalidas.Configuracion;
using System;
using System.Windows.Forms;

namespace InventarioSalidas.Config.Configuracion
{
    public class ConexionEscaner
    {
        private string connectionString;
        private FbConnection fbc;
        private RegistrosWindows reg;

        public FbConnection FBC
        {
            get { return fbc; }
            set { fbc = value; }
        }

        public ConexionEscaner()
        {
            reg = new RegistrosWindows();
        }

        /// <summary>
        /// Conecta a la base de datos ESCANER.FDB
        /// Lee la ruta desde los registros de Windows (ESCANER_ROOT)
        /// Si no existe, usa ruta por defecto: C:\Microsip datos\SOTI
        /// </summary>
        public bool ConectarEscaner()
        {
            try
            {
                reg.LeerRegistros(false);

                // Intentar leer ruta desde registros, si no existe usar default
                string rutaEscaner = reg.ESCANER_ROOT;

               

                connectionString = @"User=" + reg.MICRO_USER +
                                  "; Password=" + reg.MICRO_PASS +
                                  "; Database=" + rutaEscaner + "\\ESCANER.FDB" +
                                  "; Datasource=" + reg.MICRO_SERVER +
                                  "; Dialect=3" +
                                  "; Charset=ISO8859_1";

                fbc = new FbConnection(connectionString);
                fbc.Open();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error conectando a ESCANER.FDB:\n{ex.Message}\n\nVerifique la ruta en configuración.",
                    "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Desconecta de la base de datos
        /// </summary>
        public void Desconectar()
        {
            try
            {
                if (fbc != null && fbc.State == System.Data.ConnectionState.Open)
                {
                    fbc.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desconectar: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Inserta un registro en la tabla SALIDAS_ENTRADAS
        /// </summary>
        public bool InsertarSalidaEntrada(string folioS, string empresaS, string folioE,
                                         string empresaE, DateTime fecha, string usuario)
        {
            try
            {
                if (fbc == null || fbc.State != System.Data.ConnectionState.Open)
                {
                    if (!ConectarEscaner())
                        return false;
                }

                string query = @"INSERT INTO SALIDAS_ENTRADAS 
                                (FOLIOS, EMPRESAS, FOLIOE, EMPRESAE, FECHA, USUARIO) 
                                VALUES 
                                (@FOLIOS, @EMPRESAS, @FOLIOE, @EMPRESAE, @FECHA, @USUARIO)";

                using (FbCommand cmd = new FbCommand(query, fbc))
                {
                    cmd.Parameters.AddWithValue("@FOLIOS", folioS);
                    cmd.Parameters.AddWithValue("@EMPRESAS", empresaS);
                    cmd.Parameters.AddWithValue("@FOLIOE", folioE);
                    cmd.Parameters.AddWithValue("@EMPRESAE", empresaE);
                    cmd.Parameters.AddWithValue("@FECHA", fecha);
                    cmd.Parameters.AddWithValue("@USUARIO", usuario);

                    int result = cmd.ExecuteNonQuery();

                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error insertando en SALIDAS_ENTRADAS:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}