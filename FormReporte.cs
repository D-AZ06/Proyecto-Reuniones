using CrystalDecisions.CrystalReports.Engine;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_Reuniones
{
    public partial class FormReporte : Form
    {
        private DatosUsuario datosUsuario;

        public FormReporte(DatosUsuario datos)
        {
            InitializeComponent();
            this.datosUsuario = datos;
        }

        private async void FormReporte_Load(object sender, EventArgs e)
        {
            try
            {
                var colReuniones = Conexion.ObtenerBaseDatos().GetCollection<BsonDocument>("Reuniones");
                var colUsuarios = Conexion.ObtenerBaseDatos().GetCollection<BsonDocument>("Usuarios");

                int idLider = Convert.ToInt32(datosUsuario.IdUsuario);
                var filtro = Builders<BsonDocument>.Filter.Eq("idLider", idLider);
                var reuniones = await colReuniones.Find(filtro).ToListAsync();

                // Recolectamos IDs para resolver nombres
                var ids = new HashSet<int>();

                foreach (var r in reuniones)
                {
                    ids.Add(r["idLider"].ToInt32());

                    if (r.Contains("investigadoresConvocados"))
                    {
                        foreach (var conv in r["investigadoresConvocados"].AsBsonArray)
                        {
                            ids.Add(conv["idInvestigador"].ToInt32());
                        }
                    }
                }

                var nombrePorId = new Dictionary<int, string>();

                if (ids.Count > 0)
                {
                    var filtroUsuarios = Builders<BsonDocument>.Filter.In("idUsuario", ids);
                    var usuarios = await colUsuarios.Find(filtroUsuarios).ToListAsync();

                    foreach (var u in usuarios)
                    {
                        nombrePorId[u["idUsuario"].ToInt32()] = u["nombreUsuario"].AsString;
                    }
                }

                // Construimos la lista tipada para Crystal Reports
                var lista = new List<ReporteReunionDTO>();

                foreach (var r in reuniones)
                {
                    string asistentes = "";

                    if (r.Contains("investigadoresConvocados"))
                    {
                        foreach (var conv in r["investigadoresConvocados"].AsBsonArray)
                        {
                            int idConv = conv["idInvestigador"].ToInt32();
                            string estadoA = conv["asistencia"].AsString;
                            string nombre;

                            if (nombrePorId.ContainsKey(idConv))
                            {
                                nombre = nombrePorId[idConv];
                            }
                            else
                            {
                                nombre = "ID " + idConv;
                            }

                            if (asistentes != "")
                            {
                                asistentes += Environment.NewLine;
                            }

                            asistentes += nombre + " (" + estadoA + ")";
                        }
                    }

                    lista.Add(new ReporteReunionDTO
                    {
                        IdReunion = r["idReunion"].ToInt32(),
                        Fecha = r["fechaReunion"].AsString,
                        HoraInicio = r["horaInicio"].AsString,
                        HoraFin = r["horaFin"].AsString,
                        Motivo = r["motivoReunion"].AsString,
                        Lugar = r["lugarReunion"].AsString,
                        Asistentes = asistentes,
                        Estado = ObtenerEstado(r)
                    });
                }

                // Convertimos la lista a DataTable para Crystal Reports
                var tabla = ConvertirADataTable(lista);

                // Cargamos el reporte
                ReportDocument reporte = new ReportDocument();
                reporte.Load(Application.StartupPath + @"\ReporteReuniones.rpt");
                reporte.SetDataSource(tabla);

                crystalReportViewer1.ReportSource = reporte;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerEstado(BsonDocument r)
        {
            DateTime inicio, fin;
            bool ok1 = DateTime.TryParse(r["fechaReunion"].AsString + " " + r["horaInicio"].AsString, out inicio);
            bool ok2 = DateTime.TryParse(r["fechaReunion"].AsString + " " + r["horaFin"].AsString, out fin);

            if (!ok1 || !ok2)
            {
                return "Desconocido";
            }

            DateTime ahora = DateTime.Now;

            if (ahora < inicio)
            {
                return "Programada";
            }
            else if (ahora >= inicio && ahora <= fin)
            {
                return "En ejecución";
            }
            else
            {
                return "Finalizada";
            }
        }

        // Crystal Reports necesita un DataTable, no una List<T>
        private DataTable ConvertirADataTable(List<ReporteReunionDTO> lista)
        {
            var tabla = new DataTable();
            tabla.Columns.Add("IdReunion", typeof(int));
            tabla.Columns.Add("Fecha", typeof(string));
            tabla.Columns.Add("HoraInicio", typeof(string));
            tabla.Columns.Add("HoraFin", typeof(string));
            tabla.Columns.Add("Motivo", typeof(string));
            tabla.Columns.Add("Lugar", typeof(string));
            tabla.Columns.Add("Asistentes", typeof(string));
            tabla.Columns.Add("Estado", typeof(string));

            foreach (var item in lista)
            {
                tabla.Rows.Add(
                    item.IdReunion,
                    item.Fecha,
                    item.HoraInicio,
                    item.HoraFin,
                    item.Motivo,
                    item.Lugar,
                    item.Asistentes,
                    item.Estado
                );
            }

            return tabla;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            var resultado = MessageBox.Show("¿Desea cerrar el reporte?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
