using InventarioSalidas;
using Microsoft.Win32;
using InventarioSalidas.Config.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace InventarioSalidas.Configuracion
{
    public static class ExtensionMethods
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
    }
    public class RegistrosWindows
    {


        private const string ruta_registros = @"SOFTWARE\SOTI\IS";
        RegistryKey rk1 = Registry.CurrentUser;
        RegistryKey rk2 = Registry.CurrentUser;


        public RegistrosWindows()
        {

        }

        #region Creacion de propiedades
        //Propiedades correo


        //propiedades Microsip
        public string MICRO_USER { set; get; }
        public string MICRO_PASS { set; get; }
        public string MICRO_SERVER { set; get; }
        public string MICRO_ROOT { set; get; }
        public string MICRO_BD { set; get; }
        public string MICRO_OC { set; get; }
        public string VERSION { get; internal set; }

        // NUEVA PROPIEDAD PARA ESCANER
        public string ESCANER_ROOT { get; set; }

        #endregion

        public string LICENCIA_VERIFICADA { get; set; }
        public string VERSION_VERIFICADA { get; set; }

        public string MENSAJE_LICENCIA { get; set; }


        public bool SO64bits()
        {
            bool bits;
            if (Environment.Is64BitOperatingSystem == true)
            {
                bits = true;
            }
            else
            {
                bits = false;
            }

            return bits;
        }
        public bool SO32bits()
        {
            bool bits;
            if (Environment.Is64BitOperatingSystem == false)
            {
                bits = true;
            }
            else
            {
                bits = false;
            }
            return bits;
        }
        public bool ExisteRegistro(string ruta_registros)
        {
            try
            {
                // return Key.GetValue(Value) != null;
                RegistryKey rkSubKey = Registry.LocalMachine.OpenSubKey(ruta_registros, false);
                //string aux =Convert.ToString(rkSubKey.GetValue("SERVICE_NAME")).Trim();
                //if (aux == null||aux.Length==0)     
                string aux = (string)rkSubKey.GetValue("SERVICE_NAME");
                if (aux is null)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }
        public bool LeerRegistros()
        {
            string msg_local = "";
            bool _exito = false;
            try
            {
                if (SO64bits() == true)
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                else
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                if (!LeerRegistros(false))
                    CrearLlaveRegistro(ruta_registros, out msg_local);
                if (msg_local.Length == 0)
                {
                    rk2 = rk1.OpenSubKey(ruta_registros, false);

                    //REGSITROS MICROSIP
                    MICRO_USER = (string)rk2.GetValue("MICRO_USER");
                    MICRO_PASS = (string)rk2.GetValue("MICRO_PASS");
                    MICRO_SERVER = (string)rk2.GetValue("MICRO_SERV");
                    MICRO_ROOT = (string)rk2.GetValue("MICRO_ROOT");
                    MICRO_BD = (string)rk2.GetValue("MICRO_BD");
                    ESCANER_ROOT = (string)rk2.GetValue("ESCANER_ROOT"); // AGREGADO
                    VERSION = (string)rk2.GetValue("VERSION");

                    VERSION_VERIFICADA = (string)rk2.GetValue("VERSION_VERIFICADA");
                    LICENCIA_VERIFICADA = (string)rk2.GetValue("LICENCIA_VERIFICADA");
                    MENSAJE_LICENCIA = (string)rk2.GetValue("MENSAJE_LICENCIA");

                    _exito = true;
                }
                else
                    _exito = false;

            }
            catch (Exception Ex)
            {
                msg_local = Ex.Message;
                _exito = false;
            }
            return _exito;
        }

        public bool LeerRegistros(bool mostrar_alerta, bool es_mantenimientos = false)
        {
            try
            {
                if (SO64bits())
                {
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                }
                else
                {
                    if (SO32bits())
                    {
                        rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                    }
                }

                rk2 = rk1.OpenSubKey(ruta_registros, false);

                MICRO_USER = (string)rk2.GetValue("MICRO_USER");
                MICRO_PASS = (string)rk2.GetValue("MICRO_PASS");
                MICRO_SERVER = (string)rk2.GetValue("MICRO_SERV");
                MICRO_ROOT = (string)rk2.GetValue("MICRO_ROOT");
                MICRO_BD = (string)rk2.GetValue("MICRO_BD");
                ESCANER_ROOT = (string)rk2.GetValue("ESCANER_ROOT"); // AGREGADO

                VERSION_VERIFICADA = (string)rk2.GetValue("VERSION_VERIFICADA");
                LICENCIA_VERIFICADA = (string)rk2.GetValue("LICENCIA_VERIFICADA");
                MENSAJE_LICENCIA = (string)rk2.GetValue("MENSAJE_LICENCIA");

                if (!es_mantenimientos)
                {
                    VERSION = (string)rk2.GetValue("VERSION");
                }
                else
                {
                    rk2 = rk1.OpenSubKey(ruta_registros, false);

                    if (rk2 != null)
                    {
                        VERSION = (string)rk2.GetValue("VERSION");
                    }
                    else
                    {
                        Registry.CurrentUser.CreateSubKey(ruta_registros);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (mostrar_alerta)
                {
                    MessageBox.Show("No fue posible leer los registros de Windows.\n\n" + ex.Message, mbox.title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
        }


        public bool EscribirRegistros(string ruta_registros, string nombre_registro, string valor_registro, out string msg)
        {
            bool _exito = false;
            string msg_local = "";
            try
            {
                if (SO64bits())
                {
                    /*rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                    rk2 = rk1.OpenSubKey(ruta_registros, true);
                    rk2.SetValue(nombre_registro, valor_registro);*/
                    using (var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                    {
                        using (var key = root.OpenSubKey(ruta_registros, true))
                        {
                            //var registeredOwner = key.GetValue("RegisteredOwner");
                            key.SetValue(nombre_registro, valor_registro);
                            _exito = true;
                        };
                    };
                }
                else
                {
                    if (SO32bits())
                    {
                        rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                        rk2 = rk1.OpenSubKey(ruta_registros, true);
                        rk2.SetValue(nombre_registro, valor_registro);
                        _exito = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _exito = false;
                msg_local += "No fue posible escribir en los registros de Windows.\n\n" + ex.Message;
            }
            msg = msg_local;
            return _exito;
        }
        public bool CrearLlaveRegistro(string ruta_registro, out string msg)
        {
            bool _exito = false;
            string msg_local = "";
            try
            {
                rk2 = rk1.CreateSubKey(ruta_registro);
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(ruta_registro);
                List<string[,]> _valores_cadena = new List<string[,]>();
                _valores_cadena.Add(new string[,] { { "MICRO_SERV", "" } });
                _valores_cadena.Add(new string[,] { { "MICRO_ROOT", "" } });
                _valores_cadena.Add(new string[,] { { "MICRO_USER", "SYSDBA" } });
                _valores_cadena.Add(new string[,] { { "MICRO_PASS", "" } });
                _valores_cadena.Add(new string[,] { { "MICRO_BD", "" } });
                _valores_cadena.Add(new string[,] { { "ESCANER_ROOT", @"C:\Microsip datos\SOTI" } }); // AGREGADO
                _valores_cadena.Add(new string[,] { { "VERSION_VERIFICADA", "0" } });
                _valores_cadena.Add(new string[,] { { "LICENCIA_VERIFICADA", "" } });
                _valores_cadena.Add(new string[,] { { "MENSAJE_LICENCIA", "-" } });
                foreach (string[,] nombre_registro in _valores_cadena)
                    EscribirRegistros(ruta_registro, nombre_registro[0, 0], nombre_registro[0, 1], out msg_local);
                _exito = true;
            }
            catch (Exception ex)
            {
                msg_local = ex.Message;
                _exito = false;
            }
            msg = msg_local;
            return _exito;

        }
        public bool ExisteValorCadena(RegistryKey Key, string valor_cadena)
        {
            bool _existe = false;
            try
            {
                _existe = Key.GetValue(valor_cadena) != null;
            }
            catch
            {
                _existe = false;
            }
            return _existe;
        }
        public bool LeerRegistros(bool mostrar_alerta)
        {
            try
            {
                if (SO64bits())
                {
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                }
                else
                {
                    if (SO32bits())
                    {
                        rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                    }
                }

                rk2 = rk1.OpenSubKey(ruta_registros, false);

                if (rk2 != null)
                {

                    //REGSITROS MICROSIP
                    MICRO_USER = (string)rk2.GetValue("MICRO_USER");
                    MICRO_PASS = (string)rk2.GetValue("MICRO_PASS");
                    MICRO_SERVER = (string)rk2.GetValue("MICRO_SERV");
                    MICRO_ROOT = (string)rk2.GetValue("MICRO_ROOT");
                    MICRO_BD = (string)rk2.GetValue("MICRO_BD");
                    ESCANER_ROOT = (string)rk2.GetValue("ESCANER_ROOT"); 
                    VERSION = (string)rk2.GetValue("VERSION");

                    VERSION_VERIFICADA = (string)rk2.GetValue("VERSION_VERIFICADA");
                    LICENCIA_VERIFICADA = (string)rk2.GetValue("LICENCIA_VERIFICADA");
                    MENSAJE_LICENCIA = (string)rk2.GetValue("MENSAJE_LICENCIA");
                    //MICRO_OC = (string)rk2.GetValue("MICRO_OC");

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch //(Exception ex)
            {
                if (mostrar_alerta)
                {
                    // MessageBox.Show("No fue posible leer los registros de Windows.\n\n" + ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
        }
        public bool CrearRegistros(bool mostrar_alerta)
        {
            try
            {
                //Registry.CurrentUser.OpenSubKey("Software", true);
                // Registry.CurrentUser.CreateSubKey(ruta_registros);
                rk2 = rk1.CreateSubKey(ruta_registros);


                return true;
            }
            catch //(Exception ex)
            {
                if (mostrar_alerta)
                {
                    // MessageBox.Show("No fue posible crear la nueva clave en los registros de Windows.\n\n" + ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
        }

        public void EscribirRegistros(string nombre_registro, string valor_registro, bool mostrar_alerta)
        {
            try
            {
                if (SO64bits())
                {
                    //rk1 = Registry.CurrentUser.OpenSubKey("Software", true);
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                    rk2 = rk1.OpenSubKey(ruta_registros, true);
                    rk2.SetValue(nombre_registro, valor_registro);
                }
                else
                {
                    if (SO32bits())
                    {
                        //rk1 = Registry.CurrentUser.OpenSubKey("Software", true);
                        rk1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                        rk2 = rk1.OpenSubKey(ruta_registros, true);
                        rk2.SetValue(nombre_registro, valor_registro);
                    }
                }
            }
            catch (Exception)
            {
                if (mostrar_alerta)
                {
                    // MessageBox.Show("No fue posible escribir en los registros de Windows.\n\n" + ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}