using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FirebirdSql.Data.FirebirdClient;
using InventarioSalidas.Config.Configuracion;
using Excel = Microsoft.Office.Interop.Excel;

namespace InventarioSalidas
{
    public partial class F_HIST_APP : Form
    {
        // Enumerador para los tipos de consulta
        private enum TipoConsulta
        {
            SesionesPorFolio,
            ResumenPorUsuario,
            DetalleCompletoPorSesion
        }

        public F_HIST_APP()
        {
            InitializeComponent();
        }

        private void F_HIST_APP_Load(object sender, EventArgs e)
        {
            InicializarFormulario();
        }

        private void InicializarFormulario()
        {
            // Configurar ComboBox de tipo de consulta
            cmbTipoConsulta.Items.Clear();
            cmbTipoConsulta.Items.Add("Sesiones por Folio de Factura");
            cmbTipoConsulta.Items.Add("Resumen por Usuario");
            cmbTipoConsulta.Items.Add("Detalle Completo de Sesiones");
            cmbTipoConsulta.SelectedIndex = 0;

            // Configurar DateTimePickers
            dtpFechaInicio.Value = DateTime.Now.AddMonths(-1);
            dtpFechaFin.Value = DateTime.Now;

            // Inicializar DataGridView
            dgvDatos.AutoGenerateColumns = false;

            // Limpiar datos iniciales
            dgvDatos.Columns.Clear();
            lblTotal.Text = "Total de registros: 0";
        }

        private void cmbTipoConsulta_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfigurarFiltrosSegunConsulta();
        }

        private void ConfigurarFiltrosSegunConsulta()
        {
            // Habilitar/Deshabilitar filtros según el tipo de consulta
            switch (cmbTipoConsulta.SelectedIndex)
            {
                case (int)TipoConsulta.SesionesPorFolio:
                    // Sesiones por folio - se requiere folio, opcional fechas y usuario
                    lblFolio.Enabled = true;
                    txtFolio.Enabled = true;
                    lblUsuario.Enabled = true;
                    txtUsuario.Enabled = true;
                    lblFechaInicio.Enabled = true;
                    dtpFechaInicio.Enabled = true;
                    lblFechaFin.Enabled = true;
                    dtpFechaFin.Enabled = true;
                    break;

                case (int)TipoConsulta.ResumenPorUsuario:
                    // Resumen por usuario - solo filtro de usuario opcional
                    lblFolio.Enabled = false;
                    txtFolio.Enabled = false;
                    lblUsuario.Enabled = true;
                    txtUsuario.Enabled = true;
                    lblFechaInicio.Enabled = true;
                    dtpFechaInicio.Enabled = true;
                    lblFechaFin.Enabled = true;
                    dtpFechaFin.Enabled = true;
                    break;

                case (int)TipoConsulta.DetalleCompletoPorSesion:
                    // Detalle completo - todos los filtros habilitados
                    lblFolio.Enabled = true;
                    txtFolio.Enabled = true;
                    lblUsuario.Enabled = true;
                    txtUsuario.Enabled = true;
                    lblFechaInicio.Enabled = true;
                    dtpFechaInicio.Enabled = true;
                    lblFechaFin.Enabled = true;
                    dtpFechaFin.Enabled = true;
                    break;
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            LimpiarFiltros();
        }

        private void LimpiarFiltros()
        {
            txtUsuario.Clear();
            txtFolio.Clear();
            dtpFechaInicio.Value = DateTime.Now.AddMonths(-1);
            dtpFechaFin.Value = DateTime.Now;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            lblTotal.Text = "Total de registros: 0";
        }

        private void CargarDatos()
        {
            try
            {
                ConexionEscaner con = new ConexionEscaner();
                if (!con.ConectarEscaner())
                {
                    MessageBox.Show("No se pudo conectar a la base de datos ESCANER",
                        "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string query = "";
                string filtros = "";

                // Construir consulta según el tipo seleccionado
                switch (cmbTipoConsulta.SelectedIndex)
                {
                    case (int)TipoConsulta.SesionesPorFolio:
                        query = @"SELECT SESION_ID, FOLIO_FACTURA, ESTADO,
                                FECHA_INICIO as FECHA_HORA_INICIO_ESCANEO,
                                FECHA_FIN as FECHA_HORA_FIN_ESCANEO,
                                USUARIO
                                FROM SESIONES_ESCANEO 
                                WHERE 1=1";

                        if (!string.IsNullOrWhiteSpace(txtFolio.Text))
                        {
                            filtros += $" AND FOLIO_FACTURA = '{txtFolio.Text.Trim()}'";
                        }

                        if (!string.IsNullOrWhiteSpace(txtUsuario.Text))
                        {
                            filtros += $" AND USUARIO LIKE '%{txtUsuario.Text.Trim()}%'";
                        }

                        filtros += $" AND FECHA_INICIO BETWEEN '{dtpFechaInicio.Value:yyyy-MM-dd}' AND '{dtpFechaFin.Value:yyyy-MM-dd 23:59:59}'";

                        query += filtros + " ORDER BY SESION_ID DESC";
                        break;

                    case (int)TipoConsulta.ResumenPorUsuario:
                        query = @"SELECT USUARIO, TOTAL_SESIONES, TOTAL_ARTICULOS_PROCESADOS, 
                                TOTAL_COMPLETOS, PROMEDIO_EFICIENCIA, 
                                PRIMERA_SESION, ULTIMA_SESION
                                FROM V_RESUMEN_POR_USUARIO
                                WHERE 1=1";

                        if (!string.IsNullOrWhiteSpace(txtUsuario.Text))
                        {
                            filtros += $" AND USUARIO LIKE '%{txtUsuario.Text.Trim()}%'";
                        }

                        filtros += $" AND PRIMERA_SESION >= '{dtpFechaInicio.Value:yyyy-MM-dd}'";
                        filtros += $" AND ULTIMA_SESION <= '{dtpFechaFin.Value:yyyy-MM-dd 23:59:59}'";

                        query += filtros + " ORDER BY TOTAL_SESIONES DESC";
                        break;

                    case (int)TipoConsulta.DetalleCompletoPorSesion:
                        query = @"SELECT SESION_ID, FOLIO_FACTURA, USUARIO, FECHA_INICIO, FECHA_FIN,
                                LINEA_INDEX, CLAVE_ARTICULO, NOMBRE_ARTICULO, 
                                UNIDADES_ESPERADAS, UNIDADES_ESCANEADAS, 
                                NUMERO_ESCANEOS, ESTADO_LINEA, COMPLETO
                                FROM V_DETALLE_SESION_COMPLETO
                                WHERE 1=1";

                        if (!string.IsNullOrWhiteSpace(txtFolio.Text))
                        {
                            filtros += $" AND FOLIO_FACTURA = '{txtFolio.Text.Trim()}'";
                        }

                        if (!string.IsNullOrWhiteSpace(txtUsuario.Text))
                        {
                            filtros += $" AND USUARIO LIKE '%{txtUsuario.Text.Trim()}%'";
                        }

                        filtros += $" AND FECHA_INICIO BETWEEN '{dtpFechaInicio.Value:yyyy-MM-dd}' AND '{dtpFechaFin.Value:yyyy-MM-dd 23:59:59}'";

                        query += filtros + " ORDER BY SESION_ID DESC, LINEA_INDEX";
                        break;
                }

                // Ejecutar consulta
                FbCommand cmd = new FbCommand(query, con.FBC);
                FbDataAdapter adapter = new FbDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Configurar DataGridView según el tipo de consulta
                ConfigurarDataGridView(dt);

                // Actualizar contador
                lblTotal.Text = $"Total de registros: {dgvDatos.Rows.Count}";

                con.Desconectar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarDataGridView(DataTable dt)
        {
            dgvDatos.Columns.Clear();
            dgvDatos.AutoGenerateColumns = false;
            dgvDatos.DataSource = dt;

            switch (cmbTipoConsulta.SelectedIndex)
            {
                case (int)TipoConsulta.SesionesPorFolio:
                    ConfigurarColumnasSesionesPorFolio();
                    break;

                case (int)TipoConsulta.ResumenPorUsuario:
                    ConfigurarColumnasResumenPorUsuario();
                    break;

                case (int)TipoConsulta.DetalleCompletoPorSesion:
                    ConfigurarColumnasDetalleCompleto();
                    break;
            }

            // Aplicar formato condicional si es necesario
            AplicarFormatoCondicional();
        }

        private void ConfigurarColumnasSesionesPorFolio()
        {
            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SESION_ID",
                HeaderText = "ID Sesión",
                Name = "colSesionId",
                Width = 100
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FOLIO_FACTURA",
                HeaderText = "Folio Factura",
                Name = "colFolioFactura",
                Width = 120
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ESTADO",
                HeaderText = "Estado",
                Name = "colEstado",
                Width = 100
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FECHA_HORA_INICIO_ESCANEO",
                HeaderText = "Fecha/Hora Inicio",
                Name = "colFechaInicio",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm:ss" }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FECHA_HORA_FIN_ESCANEO",
                HeaderText = "Fecha/Hora Fin",
                Name = "colFechaFin",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm:ss" }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "USUARIO",
                HeaderText = "Usuario",
                Name = "colUsuario",
                Width = 150
            });
        }

        private void ConfigurarColumnasResumenPorUsuario()
        {
            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "USUARIO",
                HeaderText = "Usuario",
                Name = "colUsuario",
                Width = 150
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TOTAL_SESIONES",
                HeaderText = "Total Sesiones",
                Name = "colTotalSesiones",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TOTAL_ARTICULOS_PROCESADOS",
                HeaderText = "Total Artículos",
                Name = "colTotalArticulos",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TOTAL_COMPLETOS",
                HeaderText = "Total Completos",
                Name = "colTotalCompletos",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PROMEDIO_EFICIENCIA",
                HeaderText = "% Eficiencia",
                Name = "colPromedioEficiencia",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PRIMERA_SESION",
                HeaderText = "Primera Sesión",
                Name = "colPrimeraSesion",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm:ss" }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ULTIMA_SESION",
                HeaderText = "Última Sesión",
                Name = "colUltimaSesion",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm:ss" }
            });
        }

        private void ConfigurarColumnasDetalleCompleto()
        {
            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SESION_ID",
                HeaderText = "Sesión",
                Name = "colSesionId",
                Width = 80
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FOLIO_FACTURA",
                HeaderText = "Folio",
                Name = "colFolioFactura",
                Width = 100
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "USUARIO",
                HeaderText = "Usuario",
                Name = "colUsuario",
                Width = 100
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FECHA_INICIO",
                HeaderText = "Inicio",
                Name = "colFechaInicio",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LINEA_INDEX",
                HeaderText = "Línea",
                Name = "colLineaIndex",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CLAVE_ARTICULO",
                HeaderText = "Clave",
                Name = "colClaveArticulo",
                Width = 100
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NOMBRE_ARTICULO",
                HeaderText = "Artículo",
                Name = "colNombreArticulo",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UNIDADES_ESPERADAS",
                HeaderText = "Esperadas",
                Name = "colUnidadesEsperadas",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UNIDADES_ESCANEADAS",
                HeaderText = "Escaneadas",
                Name = "colUnidadesEscaneadas",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NUMERO_ESCANEOS",
                HeaderText = "# Escaneos",
                Name = "colNumeroEscaneos",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ESTADO_LINEA",
                HeaderText = "Estado",
                Name = "colEstadoLinea",
                Width = 100
            });

            dgvDatos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "COMPLETO",
                HeaderText = "Completo",
                Name = "colCompleto",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
        }

        private void AplicarFormatoCondicional()
        {
            // Aplicar colores según valores en columnas específicas
            foreach (DataGridViewRow row in dgvDatos.Rows)
            {
                switch (cmbTipoConsulta.SelectedIndex)
                {
                    case (int)TipoConsulta.SesionesPorFolio:
                        if (row.Cells["colEstado"].Value != null)
                        {
                            string estado = row.Cells["colEstado"].Value.ToString();
                            if (estado.ToUpper() == "COMPLETADO")
                            {
                                row.Cells["colEstado"].Style.BackColor = Color.LightGreen;
                                row.Cells["colEstado"].Style.ForeColor = Color.DarkGreen;
                            }
                            else if (estado.ToUpper() == "EN PROCESO")
                            {
                                row.Cells["colEstado"].Style.BackColor = Color.LightYellow;
                                row.Cells["colEstado"].Style.ForeColor = Color.DarkOrange;
                            }
                        }
                        break;

                    case (int)TipoConsulta.ResumenPorUsuario:
                        if (row.Cells["colPromedioEficiencia"].Value != null)
                        {
                            decimal eficiencia = Convert.ToDecimal(row.Cells["colPromedioEficiencia"].Value);
                            if (eficiencia >= 95)
                            {
                                row.Cells["colPromedioEficiencia"].Style.BackColor = Color.LightGreen;
                                row.Cells["colPromedioEficiencia"].Style.ForeColor = Color.DarkGreen;
                            }
                            else if (eficiencia >= 80)
                            {
                                row.Cells["colPromedioEficiencia"].Style.BackColor = Color.LightYellow;
                                row.Cells["colPromedioEficiencia"].Style.ForeColor = Color.DarkOrange;
                            }
                            else
                            {
                                row.Cells["colPromedioEficiencia"].Style.BackColor = Color.LightPink;
                                row.Cells["colPromedioEficiencia"].Style.ForeColor = Color.DarkRed;
                            }
                        }
                        break;

                    case (int)TipoConsulta.DetalleCompletoPorSesion:
                        if (row.Cells["colCompleto"].Value != null)
                        {
                            string completo = row.Cells["colCompleto"].Value.ToString();
                            if (completo.ToUpper() == "SI")
                            {
                                row.Cells["colCompleto"].Style.BackColor = Color.LightGreen;
                                row.Cells["colCompleto"].Style.ForeColor = Color.DarkGreen;
                                row.Cells["colCompleto"].Style.Font = new Font(dgvDatos.Font, FontStyle.Bold);
                            }
                            else
                            {
                                row.Cells["colCompleto"].Style.BackColor = Color.LightPink;
                                row.Cells["colCompleto"].Style.ForeColor = Color.DarkRed;
                                row.Cells["colCompleto"].Style.Font = new Font(dgvDatos.Font, FontStyle.Bold);
                            }
                        }
                        break;
                }
            }
        }

        private void btnExportarExcel_Click(object sender, EventArgs e)
        {
            if (dgvDatos.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Preguntar dónde guardar el archivo
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                    FileName = $"Historial_AppMovil_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    Title = "Guardar reporte en Excel"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    ExportarAExcel(saveDialog.FileName);
                    Cursor = Cursors.Default;

                    MessageBox.Show("Archivo exportado exitosamente.",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Preguntar si desea abrir el archivo
                    if (MessageBox.Show("¿Desea abrir el archivo?", "Confirmación",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error al exportar a Excel:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarAExcel(string rutaArchivo)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                // Crear aplicación Excel
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                // Crear libro y hoja
                workbook = excelApp.Workbooks.Add();
                worksheet = (Excel.Worksheet)workbook.Worksheets[1];

                // Título del reporte
                worksheet.Cells[1, 1] = "HISTORIAL APLICACIÓN MÓVIL";
                Excel.Range rangoTitulo = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, dgvDatos.Columns.Count]];
                rangoTitulo.Merge();
                rangoTitulo.Font.Size = 16;
                rangoTitulo.Font.Bold = true;
                rangoTitulo.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                rangoTitulo.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(41, 128, 185));
                rangoTitulo.Font.Color = ColorTranslator.ToOle(Color.White);

                // Información del reporte
                worksheet.Cells[2, 1] = "Tipo de Consulta:";
                worksheet.Cells[2, 2] = cmbTipoConsulta.Text;
                worksheet.Cells[3, 1] = "Fecha de Generación:";
                worksheet.Cells[3, 2] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cells[4, 1] = "Total de Registros:";
                worksheet.Cells[4, 2] = dgvDatos.Rows.Count;

                // Aplicar estilo a información del reporte
                Excel.Range rangoInfo = worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[4, 1]];
                rangoInfo.Font.Bold = true;

                // Encabezados
                int filaEncabezado = 6;
                for (int i = 0; i < dgvDatos.Columns.Count; i++)
                {
                    worksheet.Cells[filaEncabezado, i + 1] = dgvDatos.Columns[i].HeaderText;
                }

                // Aplicar estilo a encabezados
                Excel.Range rangoEncabezados = worksheet.Range[worksheet.Cells[filaEncabezado, 1],
                    worksheet.Cells[filaEncabezado, dgvDatos.Columns.Count]];
                rangoEncabezados.Font.Bold = true;
                rangoEncabezados.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(52, 73, 94));
                rangoEncabezados.Font.Color = ColorTranslator.ToOle(Color.White);
                rangoEncabezados.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Datos
                int filaActual = filaEncabezado + 1;
                foreach (DataGridViewRow row in dgvDatos.Rows)
                {
                    for (int i = 0; i < dgvDatos.Columns.Count; i++)
                    {
                        if (row.Cells[i].Value != null)
                        {
                            // Manejar diferentes tipos de datos
                            if (row.Cells[i].Value is DateTime)
                            {
                                worksheet.Cells[filaActual, i + 1] = Convert.ToDateTime(row.Cells[i].Value);
                                worksheet.Cells[filaActual, i + 1].NumberFormat = "dd/mm/yyyy hh:mm:ss";
                            }
                            else if (row.Cells[i].Value is decimal || row.Cells[i].Value is double || row.Cells[i].Value is float)
                            {
                                worksheet.Cells[filaActual, i + 1] = row.Cells[i].Value;
                                worksheet.Cells[filaActual, i + 1].NumberFormat = "#,##0.00";
                            }
                            else
                            {
                                worksheet.Cells[filaActual, i + 1] = row.Cells[i].Value.ToString();
                            }

                            // Aplicar formato condicional de colores
                            if (row.Cells[i].Style.BackColor != Color.Empty &&
                                row.Cells[i].Style.BackColor != Color.White)
                            {
                                worksheet.Cells[filaActual, i + 1].Interior.Color =
                                    ColorTranslator.ToOle(row.Cells[i].Style.BackColor);
                                worksheet.Cells[filaActual, i + 1].Font.Color =
                                    ColorTranslator.ToOle(row.Cells[i].Style.ForeColor);
                            }
                        }
                    }
                    filaActual++;
                }

                // Aplicar bordes a toda la tabla
                Excel.Range rangoTabla = worksheet.Range[worksheet.Cells[filaEncabezado, 1],
                    worksheet.Cells[filaActual - 1, dgvDatos.Columns.Count]];
                rangoTabla.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                rangoTabla.Borders.Weight = Excel.XlBorderWeight.xlThin;

                // Ajustar ancho de columnas
                worksheet.Columns.AutoFit();

                // Guardar archivo
                workbook.SaveAs(rutaArchivo);
            }
            finally
            {
                // Limpiar objetos COM
                if (worksheet != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                }
                if (workbook != null)
                {
                    workbook.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}