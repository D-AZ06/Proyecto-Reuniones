using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes; 



namespace Proyecto_Reuniones
{
    public partial class FormAgregar : Form
    {
        private IMongoCollection<BsonDocument> usuariosCol;
        private IMongoCollection<BsonDocument> reunionesCol;
        private string connectionString;
        private DatosUsuario usuarioLogueado;

        public FormAgregar(DatosUsuario datosRecibidos)
        {
            ConfigurarConexion();
            InitializeComponent();
            this.usuarioLogueado = datosRecibidos;
            
        }

        private void ConfigurarConexion()
        {
            try
            {
                string cadena = File.ReadAllText("LlaveAcceso.txt").Trim();
                var cliente = new MongoClient(cadena);
                var db = cliente.GetDatabase("BD-ProReuniones"); // Verifica este nombre en Atlas

                // Si en tu DB se llama "usuarios" (minúscula), cámbialo aquí:
                usuariosCol = db.GetCollection<BsonDocument>("Usuarios");
                reunionesCol = db.GetCollection<BsonDocument>("Reuniones");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en ConfigurarConexion: " + ex.Message);
            }
        }





        private void btnCancelar_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Estás seguro de cancelar la creación de la reunión?", "Cancelar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void FormAgregar_Load(object sender, EventArgs e)
        {
            try
            {
                if (usuarioLogueado != null)
                {
                    // OPCIÓN A: Usar los datos que ya traemos del Login (Recomendado)
                    // Esto evita hacer otra consulta a Mongo y que falle por el ID
                    txtLiderResponsable.Text = usuarioLogueado.Nombre;
                    txtIdLider.Text = usuarioLogueado.IdUsuario.ToString();
                    txtIdSemillero.Text = usuarioLogueado.IdSemillero.ToString();

                    txtLiderResponsable.ReadOnly = true;
                    txtIdLider.ReadOnly = true;
                    txtIdSemillero.ReadOnly = true;

                    CargarInvestigadores(usuarioLogueado.IdSemillero);
                }
                else
                {
                    MessageBox.Show("No se recibieron datos del usuario logueado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos automáticos: " + ex.Message);
            }
        }

        private void CargarInvestigadores(int idSemillero)
        {
            if (usuariosCol == null) return;

            try
            {
                var filtro = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("idSemillero", idSemillero),
                    Builders<BsonDocument>.Filter.Eq("rolUsuario", "Investigador")
                );

                var lista = usuariosCol.Find(filtro).ToList();
                clbListaInvestigadores.Items.Clear();

                foreach (var doc in lista)
                {
                    // Creamos el objeto con Nombre e ID
                    var item = new ItemInvestigador
                    {
                        Nombre = doc["nombreUsuario"].AsString,
                        Id = doc["idUsuario"].AsInt32
                    };

                    clbListaInvestigadores.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar investigadores: " + ex.Message);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            
            // 1. Preguntar si está seguro
            DialogResult resultado = MessageBox.Show("¿Está seguro de que desea guardar esta reunión?","Confirmar Guardado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    // Extraer IDs de los investigadores seleccionados
                    BsonArray seleccionadosIds = new BsonArray();
                    foreach (object item in clbListaInvestigadores.CheckedItems)
                    {
                        var inv = (ItemInvestigador)item;
                        seleccionadosIds.Add(inv.Id);
                    }

                    // Crear el documento para MongoDB
                    var nuevaReunion = new BsonDocument
            {
                { "idReunion", int.Parse(txtIdReunion.Text) },
                { "fechaReunion", dtpFechaReunion.Value.ToString("yyyy-MM-dd") },
                { "horaInicio", dtpHoraInicioReunion.Value.ToString("HH:mm") },
                { "horaFin", dtpHoraFinalReunion.Value.ToString("HH:mm") },
                { "motivoReunion", txtMotivoReunion.Text },
                { "lugarReunion", cboLugarReunion.Text },
                { "idLider", int.Parse(txtIdLider.Text) },
                { "idInvestigadores", seleccionadosIds }
            };

                    // Insertar en la base de datos
                    reunionesCol.InsertOne(nuevaReunion);

                    MessageBox.Show("Reunión guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 2. Limpiar el formulario para la siguiente reunión
                    LimpiarFormulario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // Si presiona 'No', no pasa nada y el usuario puede seguir editando
        }
        

        private void LimpiarFormulario()
        {
            // Limpiamos los campos de texto de la reunión
            txtIdReunion.Clear();
            txtMotivoReunion.Clear();
            cboLugarReunion.SelectedIndex = -1; // Deseleccionamos cualquier lugar seleccionado

            // Resetear las fechas y horas al momento actual
            dtpFechaReunion.Value = DateTime.Now;
            dtpHoraInicioReunion.Value = DateTime.Now;
            dtpHoraFinalReunion.Value = DateTime.Now.AddHours(1); // Sugiere una hora después

            // Desmarcar todos los investigadores de la lista
            for (int i = 0; i < clbListaInvestigadores.Items.Count; i++)
            {
                clbListaInvestigadores.SetItemChecked(i, false);
            }

            // Ponemos el foco en el ID de la nueva reunión
            txtIdReunion.Focus();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void cboLugarReunion_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
    public class ItemInvestigador
    {
        public string Nombre { get; set; }
        public int Id { get; set; }
        // Este método es lo que el ListBox mostrará al usuario
        public override string ToString() => Nombre;
    }
}
