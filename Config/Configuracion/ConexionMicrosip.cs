using FirebirdSql.Data.FirebirdClient;
using InventarioSalidas.Config.Configuracion;
using InventarioSalidas.Configuracion;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace InventarioSalidas.Config.Configuracion
{
    public class ConexionMicrosip
    {
        private string conectionString;
        private string user, password, database, root, dataSource;
        private FbConnection fbc;
        private RegistrosWindows reg;
        private string caption = "Mensaje de la aplicación";

        public ConexionMicrosip()
        {
            user = password = database = root = dataSource = "";
        }
        public string USER
        {
            set { user = value; }
            get { return user; }
        }
        public string PASSWORD
        {
            set { password = value; }
            get { return password; }
        }
        public string DATABASE
        {
            set { database = value; }
            get { return database; }
        }
        public string ROOT
        {
            set { root = value; }
            get { return root; }
        }
        public string DATASOURSE
        {
            set { dataSource = value; }
            get { return dataSource; }
        }
        public string CONECTIONSTRING
        {
            set { conectionString = value; }
            get { return conectionString; }
        }
        public FbConnection FBC
        {
            set { fbc = value; }
            get { return fbc; }
        }
        public RegistrosWindows REG
        {
            set { reg = value; }
            get { return reg; }
        }
        public bool ConectarMicrosip(string db)
        {
            bool band = false;
            try
            {
                reg = new RegistrosWindows();
                reg.LeerRegistros();
                conectionString = @"User=" + reg.MICRO_USER + "; Password=" + reg.MICRO_PASS
                        + "; Database=" + reg.MICRO_ROOT + "\\" + db + ".FDB"
                        + "; Datasource=" + reg.MICRO_SERVER + "; Dialect=3" + "; Charset=ISO8859_1";

                fbc = new FbConnection(conectionString);
                fbc.Open();
                band = true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            return band;

        }
        public int GenDocto(string BDMicrosip)
        {
            int docto_cm_id = 0;
            try
            {
                ConexionMicrosip cn = new ConexionMicrosip();

                cn.ConectarMicrosip(BDMicrosip);
                FbConnection con_microsip = new FbConnection(cn.CONECTIONSTRING);
                con_microsip.Open();
                FbCommand gen_docto_cm_id = new FbCommand("GEN_DOCTO_ID");
                gen_docto_cm_id.CommandType = CommandType.StoredProcedure;
                gen_docto_cm_id.Connection = con_microsip;
                docto_cm_id = Convert.ToInt32(gen_docto_cm_id.ExecuteScalar());
                gen_docto_cm_id.Cancel();
                con_microsip.Close();
                cn.Desconectar();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
            return docto_cm_id;
        }
        public bool ConectarConfigMicrosip()
        {
            bool band = false;
            try
            {
                reg = new RegistrosWindows();
                reg.LeerRegistros();
                conectionString = @"User=" + reg.MICRO_USER + "; Password=" + reg.MICRO_PASS
                        + "; Database=" + reg.MICRO_ROOT + "\\" + "System" + "\\" + "CONFIG" + ".FDB"
                        + "; Datasource=" + reg.MICRO_SERVER + "; Dialect=3" + "; Charset=ISO8859_1";

                fbc = new FbConnection(conectionString);
                fbc.Open();
                band = true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

            return band;

        }

        public bool ConectarFB(string usuario, string pass, string ruta, string servidor, out string mensaje)
        {
            mensaje = "";
            try
            {
                reg = new RegistrosWindows();

                if (reg.LeerRegistros(true))
                {
                    conectionString = @"User= " + usuario + ";";
                    conectionString += "Password=" + pass + ";";
                    conectionString += "Database=" + ruta + "\\System\\" + "config.FDB" + ";";
                    conectionString += "Datasource=" + servidor + ";";
                    conectionString += "Dialect=3;";
                    conectionString += "Charset=ISO8859_1;";

                    fbc = new FbConnection(conectionString);
                    fbc.Open();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensaje = "No fue posible establecer conexión con la empresa  .\n\n" + ex.Message;
                MessageBox.Show(mensaje);

                return false;
            }
        }
        public void Desconectar()
        {
            fbc.Close();
        }

        public bool ConectarFB_Test(string serv, string root, string pass)
        {
            try
            {
                conectionString = @"User=SYSDBA;";
                conectionString += "Password=" + pass + ";";
                conectionString += "Database=" + root + "\\System\\Config.FDB" + ";";
                conectionString += "Datasource=" + serv + ";";
                conectionString += "Dialect=3;";
                conectionString += "Charset=ISO8859_1;";

                fbc = new FbConnection(conectionString);
                fbc.Open();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No fue posible establecer conexión con Microsip.\n\n" + ex.Message, "Mensaje de la aplicación", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        public bool ConectarMetadatos()
        {
            try
            {
                reg = new RegistrosWindows();

                if (reg.LeerRegistros(true))
                {
                    conectionString = @"User=SYSDBA;";
                    conectionString += "Password=" + reg.MICRO_PASS + ";";
                    conectionString += "Database=" + reg.MICRO_ROOT + "\\System\\Metadatos.FDB" + ";";
                    conectionString += "Datasource=" + reg.MICRO_SERVER + ";";
                    conectionString += "Dialect=3;";
                    conectionString += "Charset=ISO8859_1;";

                    fbc = new FbConnection(conectionString);
                    fbc.Open();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No fue posible establecer conexión con el metadatos.\n\n" + ex.Message, "Mensaje de la aplicación", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        public int VersionActualMsp(ConexionMicrosip con)
        {
            int versionActualMsp = 0;
            try
            {
                string query = "SELECT FIRST 1 VERSION_DB ";
                query += "FROM CONVER_BASE_DATOS ";
                query += "ORDER BY VERSION_DB DESC";

                FbCommand fb = new FbCommand(query, con.FBC);
                FbDataReader fdr = fb.ExecuteReader();

                while (fdr.Read())
                    versionActualMsp = Convert.ToInt32(Convert.ToString(fdr["VERSION_DB"]));
                fdr.Close();
                fb.Dispose();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

            return versionActualMsp;
        }

        public bool ConectarFB_Metadatos()
        {
            try
            {
                RegistrosWindows reg = new RegistrosWindows();

                if (reg.LeerRegistros(false))
                {
                    conectionString = @"User=SYSDBA;";
                    conectionString += "Password=" + reg.MICRO_PASS + ";";
                    conectionString += "Database=" + reg.MICRO_ROOT + "\\System\\Metadatos.FDB" + ";";
                    conectionString += "Datasource=" + reg.MICRO_SERVER + ";";
                    conectionString += "Dialect=3;";
                    conectionString += "Charset=ISO8859_1;";

                    fbc = new FbConnection(conectionString);
                    fbc.Open();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No fue posible establecer conexión con Microsip.\n\n" + ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }
    }
}
