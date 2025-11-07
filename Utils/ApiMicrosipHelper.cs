using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ApisMicrosip;
using InventarioSalidas.Config.Configuracion;
using InventarioSalidas.Configuracion;
using FirebirdSql.Data.FirebirdClient;

namespace InventarioSalidas.Utils
{
    public class ApiMicrosipHelper
    {
        private int dbHandle = -1;
        private string empresaActual = "";

        public bool InicializarApi()
        {
            try
            {
                ApiMspBasicaExt.SetErrorHandling(0, 0);
                ApiMspInventExt.inSetErrorHandling(0, 0);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al inicializar API: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool ConectarEmpresa(string nombreEmpresa)
        {
            try
            {
                // Si ya estamos conectados a esta empresa, no reconectar
                if (empresaActual == nombreEmpresa && dbHandle != -1)
                    return true;

                // Desconectar anterior si existe
                if (dbHandle != -1)
                {
                    ApiMspBasicaExt.DBDisconnect(dbHandle);
                }

                // Crear nuevo handle de base de datos
                dbHandle = ApiMspBasicaExt.NewDB();
                if (dbHandle == -1)
                {
                    MessageBox.Show("No se pudo crear handle de base de datos", "Error");
                    return false;
                }

                // Obtener configuración de Microsip
                RegistrosWindows reg = new RegistrosWindows();
                if (!reg.LeerRegistros(false))
                {
                    MessageBox.Show("No se pudieron leer los registros", "Error");
                    return false;
                }

                // Construir ruta de la base de datos
                string rutaDB = ObtenerRutaBaseDatos(nombreEmpresa, reg);
                if (string.IsNullOrEmpty(rutaDB))
                {
                    MessageBox.Show($"No se encontró la ruta de la empresa {nombreEmpresa}", "Error");
                    return false;
                }

                // DIAGNÓSTICO: Mostrar información de conexión
                string diagnostico = $"Intentando conectar:\n" +
                                   $"DB Handle: {dbHandle}\n" +
                                   $"Ruta BD: {rutaDB}\n" +
                                   $"Usuario: {reg.MICRO_USER}\n" +
                                   $"¿Archivo existe?: {System.IO.File.Exists(rutaDB)}";

                MessageBox.Show(diagnostico, "Diagnóstico de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Verificar que el archivo existe
                if (!System.IO.File.Exists(rutaDB))
                {
                    MessageBox.Show($"El archivo de base de datos no existe: {rutaDB}", "Error");
                    return false;
                }

                // Conectar usando la API básica
                int resultado = ApiMspBasicaExt.DBConnect(dbHandle, rutaDB, reg.MICRO_USER ?? "SYSDBA", reg.MICRO_PASS ?? "masterkey");

                MessageBox.Show($"Resultado de DBConnect: {resultado}", "Resultado Conexión");

                if (resultado != 0)
                {
                    // Obtener códigos de error específicos
                    int errorCode = ApiMspBasicaExt.GetLastErrorCode();
                    int ibErrorCode = ApiMspBasicaExt.GetLastIBErroCode();
                    string errorMsg = ObtenerMensajeErrorBasico();

                    string errorDetallado = $"Error al conectar a la base de datos:\n" +
                                          $"Código de error: {errorCode}\n" +
                                          $"Código IB: {ibErrorCode}\n" +
                                          $"Resultado DBConnect: {resultado}\n" +
                                          $"Mensaje: {errorMsg}";

                    MessageBox.Show(errorDetallado, "Error Detallado");
                    return false;
                }

                // Establecer la conexión en la API de inventarios
                resultado = ApiMspInventExt.SetDBInventarios(dbHandle);

                if (resultado == 0)
                {
                    empresaActual = nombreEmpresa;
                    MessageBox.Show("Conexión exitosa!", "Éxito");
                    return true;
                }
                else
                {
                    string error = ObtenerMensajeError();
                    MessageBox.Show($"Error al establecer BD en API de inventarios: {error}", "Error");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar empresa {nombreEmpresa}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error");
                return false;
            }
        }

        private string ObtenerRutaBaseDatos(string nombreEmpresa, RegistrosWindows reg)
        {
            try
            {
                string diagnostico = $"Configuración leída:\n" +
                                   $"MICRO_BD: '{reg.MICRO_BD}'\n" +
                                   $"MICRO_ROOT: '{reg.MICRO_ROOT}'\n" +
                                   $"MICRO_SERVER: '{reg.MICRO_SERVER}'";

                MessageBox.Show(diagnostico, "Configuración Microsip", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Si hay una ruta específica configurada, usarla
                if (!string.IsNullOrEmpty(reg.MICRO_BD))
                {
                    return reg.MICRO_BD;
                }

                // Si hay una ruta raíz configurada, construir la ruta
                if (!string.IsNullOrEmpty(reg.MICRO_ROOT))
                {
                    // Intentar diferentes nombres de archivo comunes
                    string[] posiblesNombres = {
                $"{nombreEmpresa}.FDB",
                $"{nombreEmpresa}.GDB",
                "MICROSIP.FDB",
                "MICROSIP.GDB"
            };

                    foreach (string nombre in posiblesNombres)
                    {
                        string rutaCompleta = System.IO.Path.Combine(reg.MICRO_ROOT, nombre);
                        if (System.IO.File.Exists(rutaCompleta))
                        {
                            MessageBox.Show($"Archivo encontrado: {rutaCompleta}", "Archivo BD Encontrado");
                            return rutaCompleta;
                        }
                    }
                }

                // Si no se encuentra, usar ConexionMicrosip como fallback
                MessageBox.Show("Intentando usar ConexionMicrosip como fallback...", "Fallback");
                ConexionMicrosip con = new ConexionMicrosip();
                if (con.ConectarMicrosip(nombreEmpresa))
                {
                    string rutaBD = con.FBC.Database;
                    con.Desconectar();
                    MessageBox.Show($"Ruta obtenida del fallback: {rutaBD}", "Fallback Exitoso");
                    return rutaBD;
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en ObtenerRutaBaseDatos: {ex.Message}", "Error");
                return null;
            }
        }

        public ResultadoSalida CrearSalida(DatosSalida datos, string nombreEmpresa)
        {
            try
            {
                if (!ConectarEmpresa(nombreEmpresa))
                {
                    return new ResultadoSalida
                    {
                        Exitoso = false,
                        Mensaje = $"No se pudo conectar a la empresa {nombreEmpresa}"
                    };
                }

                // Crear nueva salida
                int resultado = ApiMspInventExt.NuevaSalida(
                    datos.ConceptoId,
                    datos.AlmacenId,
                    0, // AlmacenDestinoId - 0 para salidas normales
                    datos.Fecha,
                    "", // Folio vacío para auto-generar
                    datos.Descripcion,
                    0 // CentroCostoId
                );

                if (resultado != 0)
                {
                    return new ResultadoSalida
                    {
                        Exitoso = false,
                        Mensaje = $"Error al crear salida: {ObtenerMensajeError()}"
                    };
                }

                // Agregar artículos
                foreach (var articulo in datos.Articulos)
                {
                    resultado = ApiMspInventExt.RenglonSalida(
                        articulo.ArticuloId,
                        articulo.Cantidad,
                        0, // CostoUnitario - 0 para auto-calcular
                        0  // CostoTotal - 0 para auto-calcular
                    );

                    if (resultado != 0)
                    {
                        ApiMspInventExt.AbortaDoctoInventarios();
                        return new ResultadoSalida
                        {
                            Exitoso = false,
                            Mensaje = $"Error al agregar artículo {articulo.ClaveArticulo}: {ObtenerMensajeError()}"
                        };
                    }
                }

                // Aplicar salida
                resultado = ApiMspInventExt.AplicaSalida();
                if (resultado != 0)
                {
                    return new ResultadoSalida
                    {
                        Exitoso = false,
                        Mensaje = $"Error al aplicar salida: {ObtenerMensajeError()}"
                    };
                }

                // Obtener el folio generado
                string folioGenerado = ObtenerUltimoFolioGenerado("S", nombreEmpresa);

                return new ResultadoSalida
                {
                    Exitoso = true,
                    Folio = folioGenerado,
                    Mensaje = $"Salida creada exitosamente con folio: {folioGenerado}"
                };
            }
            catch (Exception ex)
            {
                try
                {
                    ApiMspInventExt.AbortaDoctoInventarios();
                }
                catch { }

                return new ResultadoSalida
                {
                    Exitoso = false,
                    Mensaje = $"Error inesperado: {ex.Message}"
                };
            }
        }

        public ResultadoEntrada CrearEntrada(DatosEntrada datos, string nombreEmpresa)
        {
            try
            {
                if (!ConectarEmpresa(nombreEmpresa))
                {
                    return new ResultadoEntrada
                    {
                        Exitoso = false,
                        Mensaje = $"No se pudo conectar a la empresa {nombreEmpresa}"
                    };
                }

                // Crear nueva entrada
                int resultado = ApiMspInventExt.NuevaEntrada(
                    datos.ConceptoId,
                    datos.AlmacenId,
                    datos.Fecha,
                    "", // Folio vacío para auto-generar
                    datos.Descripcion,
                    0 // CentroCostoId
                );

                if (resultado != 0)
                {
                    return new ResultadoEntrada
                    {
                        Exitoso = false,
                        Mensaje = $"Error al crear entrada: {ObtenerMensajeError()}"
                    };
                }

                // Agregar artículos
                foreach (var articulo in datos.Articulos)
                {
                    resultado = ApiMspInventExt.RenglonEntrada(
                        articulo.ArticuloId,
                        articulo.Cantidad,
                        0, // CostoUnitario - 0 para auto-calcular
                        0  // CostoTotal - 0 para auto-calcular
                    );

                    if (resultado != 0)
                    {
                        ApiMspInventExt.AbortaDoctoInventarios();
                        return new ResultadoEntrada
                        {
                            Exitoso = false,
                            Mensaje = $"Error al agregar artículo {articulo.ClaveArticulo}: {ObtenerMensajeError()}"
                        };
                    }
                }

                // Aplicar entrada
                resultado = ApiMspInventExt.AplicaEntrada();
                if (resultado != 0)
                {
                    return new ResultadoEntrada
                    {
                        Exitoso = false,
                        Mensaje = $"Error al aplicar entrada: {ObtenerMensajeError()}"
                    };
                }

                // Obtener el folio generado
                string folioGenerado = ObtenerUltimoFolioGenerado("E", nombreEmpresa);

                return new ResultadoEntrada
                {
                    Exitoso = true,
                    Folio = folioGenerado,
                    Mensaje = $"Entrada creada exitosamente con folio: {folioGenerado}"
                };
            }
            catch (Exception ex)
            {
                try
                {
                    ApiMspInventExt.AbortaDoctoInventarios();
                }
                catch { }

                return new ResultadoEntrada
                {
                    Exitoso = false,
                    Mensaje = $"Error inesperado: {ex.Message}"
                };
            }
        }

        private string ObtenerUltimoFolioGenerado(string tipo, string empresa)
        {
            try
            {
                ConexionMicrosip con = new ConexionMicrosip();
                if (con.ConectarMicrosip(empresa))
                {
                    // Consultar el último documento generado
                    string query = @"
                        SELECT FIRST 1 FOLIO 
                        FROM DOCTOS_IN 
                        WHERE FECHA = @FECHA 
                        AND CONCEPTO_IN_ID IN (
                            SELECT CONCEPTO_IN_ID FROM CONCEPTOS_IN WHERE TIPO = @TIPO
                        )
                        ORDER BY DOCTO_IN_ID DESC";

                    FbCommand cmd = new FbCommand(query, con.FBC);
                    cmd.Parameters.AddWithValue("@FECHA", DateTime.Now.Date);
                    cmd.Parameters.AddWithValue("@TIPO", tipo);

                    object result = cmd.ExecuteScalar();
                    con.Desconectar();

                    if (result != null)
                        return result.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"{tipo}-{DateTime.Now:yyyyMMddHHmmss}";
            }

            return $"{tipo}-GENERADO";
        }

        private string ObtenerMensajeError()
        {
            try
            {
                StringBuilder mensaje = new StringBuilder(255);
                int codigo = ApiMspInventExt.inGetLastErrorMessage(mensaje);
                return codigo != 0 ? mensaje.ToString().Trim() : "Error desconocido";
            }
            catch
            {
                return "Error al obtener mensaje de error";
            }
        }

        private string ObtenerMensajeErrorBasico()
        {
            try
            {
                StringBuilder mensaje = new StringBuilder(255);
                int codigo = ApiMspBasicaExt.GetLastErrorMessage(mensaje);
                return codigo == 0 ? mensaje.ToString().Trim() : "Error desconocido";
            }
            catch
            {
                return "Error al obtener mensaje de error";
            }
        }

        public void Dispose()
        {
            try
            {
                ApiMspInventExt.AbortaDoctoInventarios();
                if (dbHandle != -1)
                {
                    ApiMspBasicaExt.DBDisconnect(dbHandle);
                    dbHandle = -1;
                }
            }
            catch { }
        }
    }

    // Clases de datos (sin cambios)
    public class DatosSalida
    {
        public int ConceptoId { get; set; }
        public int AlmacenId { get; set; }
        public string Fecha { get; set; }
        public string Descripcion { get; set; }
        public List<ArticuloMovimiento> Articulos { get; set; } = new List<ArticuloMovimiento>();
    }

    public class DatosEntrada
    {
        public int ConceptoId { get; set; }
        public int AlmacenId { get; set; }
        public string Fecha { get; set; }
        public string Descripcion { get; set; }
        public List<ArticuloMovimiento> Articulos { get; set; } = new List<ArticuloMovimiento>();
    }

    public class ArticuloMovimiento
    {
        public string ClaveArticulo { get; set; }
        public double Cantidad { get; set; }
        public int ArticuloId { get; set; }

        // NUEVAS PROPIEDADES PARA COSTOS
        public double CostoUnitario { get; set; }
        public double CostoTotal { get; set; }
    }

    public class ResultadoSalida
    {
        public bool Exitoso { get; set; }
        public string Folio { get; set; }
        public string Mensaje { get; set; }
    }

    public class ResultadoEntrada
    {
        public bool Exitoso { get; set; }
        public string Folio { get; set; }
        public string Mensaje { get; set; }
    }
}