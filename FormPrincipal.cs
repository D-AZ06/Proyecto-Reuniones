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
            var db = Conexion.ObtenerBaseDatos();
            var coleccion = db.GetCollection<BsonDocument>("Reuniones");

            FilterDefinition<BsonDocument> filtro;

            // Convertimos a int para que coincida con el color azul de tu Compass
            int idUsuarioLogueado = Convert.ToInt32(datosUsuario.IdUsuario);
            int idSemilleroUsuario = Convert.ToInt32(datosUsuario.IdSemillero);

            if (datosUsuario.Rol == "Líder")
            {
                // El líder ve las reuniones donde idLider coincide con su ID
                // (O si tienes un campo idSemillero en la reunión, úsalo aquí)
                filtro = Builders<BsonDocument>.Filter.Eq("idLider", idUsuarioLogueado);
            }
            else
            {
                // El investigador ve donde su ID está dentro del array idInvestigadores
                filtro = Builders<BsonDocument>.Filter.AnyEq("idInvestigadores", idUsuarioLogueado);
            }

            var listaDocs = await coleccion.Find(filtro).ToListAsync();

            var consulta = listaDocs.Select(r => new
            {
                Cod = r["idReunion"].ToInt32(),
                Fecha = r["fechaReunion"].AsString,
                Inicio = r["horaInicio"].AsString,
                Fin = r["horaFin"].AsString,
                Asunto = r["motivoReunion"].AsString,
                Líder = r["idLider"].ToInt32()
            }).ToList();

            dataGridView1.DataSource = null;
            if (consulta.Count > 0)
            {
                dataGridView1.DataSource = consulta;

                // Mejoras visuales que pediste al principio
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSteelBlue;
            }
            else
            {
                MessageBox.Show("No se encontraron reuniones para este usuario.");
            }
        }
    }
}
