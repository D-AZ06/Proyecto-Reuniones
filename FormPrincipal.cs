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
    public partial class FormPrincipal : Form
    {
        private DatosUsuario datosUsuario;

        public FormPrincipal(DatosUsuario datos)
        {
            InitializeComponent();
            datosUsuario = datos;

            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            if (datosUsuario.Rol == "Líder")
            {

            }
            else if (datosUsuario.Rol == "Investigador")
            {
                btnAgregarReunión.Enabled = false; // Deshabilitamos el botón de agregar reunión para los investigadores
                btnAgregarReunión.Visible = false; // Ocultamos el botón de agregar reunión para los investigadores
            }
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            
        }

        private void btnAgregarReunión_Click(object sender, EventArgs e)
        {
            FormAgregar formAgregar = new FormAgregar();
            formAgregar.ShowDialog();
        }

        // Método para obtener las reuniones de un semillero específico (Lider)
        public async Task<List<BsonDocument>> ReunionesPorSemilleroLider(int idSemillero)
        {
            var db = Conexion.ObtenerBaseDatos();
            var coleccion = db.GetCollection<BsonDocument>("Reuniones");

            var filtro = Builders<BsonDocument>.Filter.Eq("ID_Semillero", idSemillero);

            var reuniones = await coleccion.Find(filtro).ToListAsync();

            return reuniones;
        }

        // Método para obtener las reuniones en las que participa un investigador específico (Investigador)
        public async Task<List<BsonDocument>> ReunionesDelInvestigador(int idUsuario)
        {
            var db = Conexion.ObtenerBaseDatos();
            var coleccion = db.GetCollection<BsonDocument>("Reuniones");

            var filtro = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                "Investigadores",
                new BsonDocument("ID", idUsuario)
            );

            var reuniones = await coleccion.Find(filtro).ToListAsync();

            return reuniones;
        }

        private async void btnVerReunion_Click(object sender, EventArgs e)
        {
            try
            {
                var coleccion = Conexion.ObtenerBaseDatos().GetCollection<BsonDocument>("Reuniones");
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
                bool esLider = datosUsuario.Rol == "Líder";

                var filtro = esLider
                    ? Builders<BsonDocument>.Filter.Eq("idLider", idUsuario)
                    : Builders<BsonDocument>.Filter.AnyEq("idInvestigadores", idUsuario);

                var docs = await coleccion.Find(filtro).ToListAsync();

                if (docs.Count == 0)
                {
                    dataGridView1.DataSource = null;
                    MessageBox.Show(
                        esLider ? "No hay reuniones programadas en tu semillero."
                                : "No estás registrado en ninguna reunión.",
                        "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tabla = new DataTable();
                tabla.Columns.Add("Cód.", typeof(int));
                tabla.Columns.Add("Fecha", typeof(string));
                tabla.Columns.Add("Inicio", typeof(string));
                tabla.Columns.Add("Fin", typeof(string));
                tabla.Columns.Add("Motivo", typeof(string));
                tabla.Columns.Add("Lugar", typeof(string));

                if (esLider)
                    tabla.Columns.Add("Asistentes (IDs)", typeof(string));
                else
                    tabla.Columns.Add("Cód. Líder", typeof(int));

                foreach (var r in docs)
                {
                    string asistentes = "";
                    if (esLider && r.Contains("idInvestigadores") && r["idInvestigadores"].IsBsonArray)
                        asistentes = string.Join(", ", r["idInvestigadores"].AsBsonArray.Select(x => x.ToString()));

                    tabla.Rows.Add(
                        r["idReunion"].ToInt32(),
                        r["fechaReunion"].AsString,
                        r["horaInicio"].AsString,
                        r["horaFin"].AsString,
                        r["motivoReunion"].AsString,
                        r["lugarReunion"].AsString,
                        esLider ? (object)asistentes : r["idLider"].ToInt32()
                    );
                }

                dataGridView1.DataSource = tabla;

                // ── Estilo y tamaño del DataGridView ──────────────────────────────
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells; // cada columna se ajusta a su contenido
                dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;    // filas también
                dataGridView1.ScrollBars = ScrollBars.Both;                          // scroll horizontal y vertical
                dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;          // si el texto es largo, hace wrap

                dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSteelBlue;
                dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                dataGridView1.DefaultCellStyle.SelectionBackColor = Color.SteelBlue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar reuniones:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
