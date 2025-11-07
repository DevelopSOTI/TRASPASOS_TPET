using FirebirdSql.Data.FirebirdClient;
using InventarioSalidas.Config.Configuracion;
using System;
using System.Windows.Forms;

namespace InventarioSalidas.Utils
{
    public class DoctoHelpers
    {
        /// <summary>
        /// Obtiene el folio de un documento de inventario desde DOCTOS_IN
        /// </summary>
        /// <param name="empresaBD">Nombre de la base de datos (empresa)</param>
        /// <param name="conceptoId">ID del concepto</param>
        /// <param name="almacenId">ID del almacén</param>
        /// <param name="fecha">Fecha del documento</param>
        /// <returns>Objeto con folio y usuario creador, o null si no se encuentra</returns>
        public static DatosDocumento ObtenerDatosDocumento(string empresaBD, int conceptoId,
                                                           int almacenId, DateTime fecha)
        {
            try
            {
                ConexionMicrosip con = new ConexionMicrosip();

                if (!con.ConectarMicrosip(empresaBD))
                {
                    MessageBox.Show($"No se pudo conectar a la empresa {empresaBD}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // Buscar el documento más reciente que coincida con los criterios
                string query = @"
                    SELECT FIRST 1 
                        FOLIO, 
                        USUARIO_CREADOR,
                        FECHA_HORA_CREACION
                    FROM DOCTOS_IN 
                    WHERE CONCEPTO_IN_ID = @CONCEPTO_ID 
                      AND ALMACEN_ID = @ALMACEN_ID 
                      AND FECHA = @FECHA
                      AND APLICADO = 'S'
                      AND CANCELADO = 'N'
                    ORDER BY FECHA_HORA_CREACION DESC";

                FbCommand cmd = new FbCommand(query, con.FBC);
                cmd.Parameters.AddWithValue("@CONCEPTO_ID", conceptoId);
                cmd.Parameters.AddWithValue("@ALMACEN_ID", almacenId);
                cmd.Parameters.AddWithValue("@FECHA", fecha.Date);

                FbDataReader dr = cmd.ExecuteReader();

                DatosDocumento datos = null;

                if (dr.Read())
                {
                    datos = new DatosDocumento
                    {
                        Folio = dr["FOLIO"].ToString().Trim(),
                        UsuarioCreador = dr["USUARIO_CREADOR"].ToString().Trim(),
                        FechaHoraCreacion = Convert.ToDateTime(dr["FECHA_HORA_CREACION"])
                    };
                }

                dr.Close();
                con.Desconectar();

                return datos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error obteniendo datos del documento:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Registra el traspaso completo en ESCANER.FDB
        /// </summary>
        public static bool RegistrarTraspaso(DatosDocumento datosSalida, string empresaOrigen,
                                            DatosDocumento datosEntrada, string empresaDestino,
                                            DateTime fecha)
        {
            try
            {
                ConexionEscaner conEscaner = new ConexionEscaner();

                if (!conEscaner.ConectarEscaner())
                {
                    MessageBox.Show("No se pudo conectar a la base de datos ESCANER",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Usar el usuario de la salida como usuario del registro
                string usuario = datosSalida?.UsuarioCreador ?? Environment.UserName;

                bool resultado = conEscaner.InsertarSalidaEntrada(
                    datosSalida?.Folio ?? "N/A",
                    empresaOrigen,
                    datosEntrada?.Folio ?? "N/A",
                    empresaDestino,
                    fecha,
                    usuario
                );

                conEscaner.Desconectar();

                if (resultado)
                {
                    /*MessageBox.Show(
                        $"✓ Traspaso registrado exitosamente en ESCANER\n\n" +
                        $"Salida: {datosSalida?.Folio} ({empresaOrigen})\n" +
                        $"Entrada: {datosEntrada?.Folio} ({empresaDestino})\n" +
                        $"Usuario: {usuario}",
                        "Registro Exitoso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );*/
                }

                return resultado;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registrando traspaso:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }

    /// <summary>
    /// Clase para almacenar datos de un documento
    /// </summary>
    public class DatosDocumento
    {
        public string Folio { get; set; }
        public string UsuarioCreador { get; set; }
        public DateTime FechaHoraCreacion { get; set; }
    }
}