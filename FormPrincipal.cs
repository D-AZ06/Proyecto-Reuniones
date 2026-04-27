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

        private async void btnVerReunion_Click(object sender, EventArgs e)
        {
            try
            {
                var db = Conexion.ObtenerBaseDatos();
                var colReuniones = db.GetCollection<BsonDocument>("Reuniones");
                var colUsuarios = db.GetCollection<BsonDocument>("Usuarios");

                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);

                // FILTRO según rol (sin variable extra)
                var filtro = datosUsuario.Rol == "Líder"
                    ? Builders<BsonDocument>.Filter.Eq("idLider", idUsuario)
                    : Builders<BsonDocument>.Filter.AnyEq("idInvestigadores", idUsuario);

                var reuniones = await colReuniones.Find(filtro).ToListAsync();

                if (reuniones.Count == 0)
                {
                    dataGridView1.DataSource = null;
                    MessageBox.Show(
                        datosUsuario.Rol == "Líder"
                            ? "No hay reuniones en tu semillero."
                            : "No estás en ninguna reunión.",
                        "Sin resultados",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Recolectar IDs necesarios (lider + investigadores)
                var ids = new HashSet<int>();

                foreach (var r in reuniones)
                {
                    ids.Add(r["idLider"].ToInt32());

                    if (r.Contains("idInvestigadores"))
                        foreach (var id in r["idInvestigadores"].AsBsonArray)
                            ids.Add(id.ToInt32());
                }

                // Traer usuarios una sola vez
                var filtroUsuarios = Builders<BsonDocument>.Filter.In("idUsuario", ids);
                var usuarios = await colUsuarios.Find(filtroUsuarios).ToListAsync();

                var nombrePorId = usuarios.ToDictionary(
                    u => u["idUsuario"].ToInt32(),
                    u => u["nombreUsuario"].AsString
                );

                // Crear tabla
                var tabla = new DataTable();
                tabla.Columns.Add("Cód.", typeof(int));
                tabla.Columns.Add("Fecha", typeof(string));
                tabla.Columns.Add("Inicio", typeof(string));
                tabla.Columns.Add("Fin", typeof(string));
                tabla.Columns.Add("Motivo", typeof(string));
                tabla.Columns.Add("Lugar", typeof(string));
                tabla.Columns.Add("Líder", typeof(string));
                tabla.Columns.Add("Asistentes", typeof(string));

                foreach (var r in reuniones)
                {
                    int idLider = r["idLider"].ToInt32();
                    string nombreLider = nombrePorId.ContainsKey(idLider)
                        ? nombrePorId[idLider]
                        : $"ID {idLider}";

                    string asistentes = "";

                    if (r.Contains("idInvestigadores"))
                    {
                        asistentes = string.Join(Environment.NewLine,
                            r["idInvestigadores"].AsBsonArray.Select(x =>
                            {
                                int id = x.ToInt32();
                                string nombre = nombrePorId.ContainsKey(id)
                                    ? nombrePorId[id]
                                    : $"ID {id}";

                                return "• " + nombre;
                            }));
                    }

                    tabla.Rows.Add(
                        r["idReunion"].ToInt32(),
                        r["fechaReunion"].AsString,
                        r["horaInicio"].AsString,
                        r["horaFin"].AsString,
                        r["motivoReunion"].AsString,
                        r["lugarReunion"].AsString,
                        nombreLider,
                        asistentes
                    );
                }

                dataGridView1.DataSource = tabla;
                DisenarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void DisenarGrid()
        {
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ScrollBars = ScrollBars.Both;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSteelBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        }
    }
}
