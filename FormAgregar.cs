using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_Reuniones
{
    public partial class FormAgregar : Form
    {
        // ─────────────────────────────────────────────────────────────────
        // CAMPOS PRIVADOS
        // usuariosCol  → colección "Usuarios" en MongoDB (para buscar investigadores)
        // reunionesCol → colección "Reuniones" en MongoDB (para insertar la nueva reunión)
        // connectionString → cadena leída desde el archivo LlaveAcceso.txt
        // usuarioLogueado  → objeto con los datos del líder que inició sesión
        // ─────────────────────────────────────────────────────────────────
        private IMongoCollection<BsonDocument> usuariosCol;
        private IMongoCollection<BsonDocument> reunionesCol;
        private string connectionString;
        private DatosUsuario usuarioLogueado;

        // ─────────────────────────────────────────────────────────────────
        // CONSTRUCTOR
        // Recibe el objeto DatosUsuario desde el formulario de Login.
        // Ese objeto trae: Nombre, IdUsuario e IdSemillero del líder activo.
        // ─────────────────────────────────────────────────────────────────
        public FormAgregar(DatosUsuario datosRecibidos)
        {
            ConfigurarConexion();       // Primero conectamos a MongoDB
            InitializeComponent();     // Luego inicializamos los controles visuales
            this.usuarioLogueado = datosRecibidos;
        }

        // ─────────────────────────────────────────────────────────────────
        // CONFIGURAR CONEXIÓN A MONGODB
        //
        // Lee la cadena de conexión (URI de Atlas) desde un archivo externo
        // llamado "LlaveAcceso.txt". Esto evita escribir credenciales
        // directamente en el código fuente (buena práctica de seguridad).
        //
        // Luego obtiene la base de datos "BD-ProReuniones" y dentro de ella
        // las dos colecciones que usará este formulario:
        //   → "Usuarios"  : para consultar qué investigadores hay en el semillero
        //   → "Reuniones" : para insertar el nuevo documento de reunión
        // ─────────────────────────────────────────────────────────────────
        private void ConfigurarConexion()
        {
            try
            {
                string cadena = File.ReadAllText("LlaveAcceso.txt").Trim();
                var cliente = new MongoClient(cadena);
                var db = cliente.GetDatabase("BD-ProReuniones");

                usuariosCol = db.GetCollection<BsonDocument>("Usuarios");
                reunionesCol = db.GetCollection<BsonDocument>("Reuniones");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con MongoDB: " + ex.Message,
                                "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // EVENTO: CARGA DEL FORMULARIO
        //
        // Se ejecuta automáticamente cuando el formulario se abre.
        // Usa los datos del usuario logueado (ya obtenidos desde el Login)
        // para rellenar automáticamente los campos del líder y del semillero.
        // Esos campos se marcan como ReadOnly para que el usuario no los edite.
        //
        // Después llama a CargarInvestigadores para poblar el CheckedListBox
        // solo con los investigadores del semillero correcto.
        // ─────────────────────────────────────────────────────────────────
        private void FormAgregar_Load(object sender, EventArgs e)
        {
            try
            {
                if (usuarioLogueado != null)
                {
                    txtLiderResponsable.Text = usuarioLogueado.Nombre;
                    txtIdLider.Text = usuarioLogueado.IdUsuario.ToString();
                    txtIdSemillero.Text = usuarioLogueado.IdSemillero.ToString();

                    // Se bloquean para que no sean editables manualmente
                    txtLiderResponsable.ReadOnly = true;
                    txtIdLider.ReadOnly = true;
                    txtIdSemillero.ReadOnly = true;

                    // Genera automáticamente el próximo ID de reunión
                    GenerarIdReunion();

                    // Carga solo los investigadores del semillero del líder logueado
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

        // ─────────────────────────────────────────────────────────────────
        // GENERAR ID DE REUNIÓN AUTOMÁTICAMENTE
        //
        // MEJORA: antes el usuario escribía el ID manualmente, lo que podía
        // generar duplicados o errores. Ahora se consulta a MongoDB cuántos
        // documentos existen en "Reuniones" y se asigna el siguiente número.
        //
        // Consulta MongoDB:
        //   CountDocuments(filtro vacío) → devuelve el total de documentos
        //   nuevoId = total + 1
        //
        // El campo txtIdReunion se pone ReadOnly para que el usuario no lo cambie.
        // ─────────────────────────────────────────────────────────────────
        private void GenerarIdReunion()
        {
            try
            {
                // Cuenta todos los documentos existentes en la colección Reuniones
                long total = reunionesCol.CountDocuments(new BsonDocument());
                int nuevoId = (int)total + 1;

                txtIdReunion.Text = nuevoId.ToString();
                txtIdReunion.ReadOnly = true; // No permitir edición manual
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar ID de reunión: " + ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // CARGAR INVESTIGADORES DEL SEMILLERO
        //
        // Consulta MongoDB con un filtro doble:
        //   1) idSemillero == idSemillero del líder logueado
        //   2) rolUsuario  == "Investigador"
        //
        // Solo se muestran los investigadores que pertenecen al mismo
        // semillero del líder, garantizando que no aparezcan personas
        // de otros semilleros.
        //
        // Cada resultado se convierte en un objeto ItemInvestigador
        // (que guarda Nombre e Id) y se agrega al CheckedListBox.
        // El CheckedListBox mostrará el nombre (via ToString()) pero
        // internamente guarda el objeto completo para recuperar el Id al guardar.
        // ─────────────────────────────────────────────────────────────────
        private void CargarInvestigadores(int idSemillero)
        {
            if (usuariosCol == null) return;

            try
            {
                // Filtro compuesto: mismo semillero Y rol Investigador
                var filtro = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("idSemillero", idSemillero),
                    Builders<BsonDocument>.Filter.Eq("rolUsuario", "Investigador")
                );

                var lista = usuariosCol.Find(filtro).ToList();
                clbListaInvestigadores.Items.Clear();

                foreach (var doc in lista)
                {
                    var item = new ItemInvestigador
                    {
                        Nombre = doc["nombreUsuario"].AsString,
                        Id = doc["idUsuario"].AsInt32
                    };

                    // Se agrega el objeto; el ListBox muestra Nombre via ToString()
                    clbListaInvestigadores.Items.Add(item);
                }

                if (lista.Count == 0)
                {
                    MessageBox.Show("No hay investigadores registrados en este semillero.",
                                    "Sin investigadores", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar investigadores: " + ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // BOTÓN GUARDAR REUNIÓN
        //
        // Proceso:
        //   1. Valida que todos los campos estén completos (MEJORA agregada)
        //   2. Valida que la hora fin sea mayor a la hora inicio (MEJORA agregada)
        //   3. Pide confirmación al usuario
        //   4. Construye un BsonDocument con los datos de la reunión
        //   5. Inserta el documento en la colección "Reuniones" de MongoDB
        //   6. Limpia el formulario para permitir crear otra reunión
        //
        // El BsonDocument es el formato nativo de MongoDB (similar a JSON).
        // Cada campo { "clave", valor } se convierte en un campo del documento.
        //
        // idInvestigadores se guarda como BsonArray (arreglo de enteros),
        // lo que permite asociar múltiples investigadores a una reunión.
        // ─────────────────────────────────────────────────────────────────
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            // ── MEJORA: Validaciones antes de intentar guardar ────────────
            if (string.IsNullOrWhiteSpace(txtMotivoReunion.Text))
            {
                MessageBox.Show("El motivo de la reunión es obligatorio.",
                                "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMotivoReunion.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(cboLugarReunion.Text))
            {
                MessageBox.Show("Debes seleccionar un lugar para la reunión.",
                                "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLugarReunion.Focus();
                return;
            }

            if (clbListaInvestigadores.CheckedItems.Count == 0)
            {
                MessageBox.Show("Debes seleccionar al menos un investigador.",
                                "Sin investigadores", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── MEJORA: Validar que hora fin > hora inicio ────────────────
            if (dtpHoraFinalReunion.Value <= dtpHoraInicioReunion.Value)
            {
                MessageBox.Show("La hora de finalización debe ser mayor a la hora de inicio.",
                                "Hora inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpHoraFinalReunion.Focus();
                return;
            }
            // ─────────────────────────────────────────────────────────────

            DialogResult resultado = MessageBox.Show(
                "¿Está seguro de que desea guardar esta reunión?",
                "Confirmar Guardado",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    // Recorre los investigadores marcados en el CheckedListBox
                    // y extrae solo sus IDs para guardarlos en MongoDB
                    BsonArray seleccionadosIds = new BsonArray();
                    foreach (object item in clbListaInvestigadores.CheckedItems)
                    {
                        var inv = (ItemInvestigador)item;
                        seleccionadosIds.Add(inv.Id); // Solo guarda el ID numérico
                    }

                    // ── DOCUMENTO BSON ────────────────────────────────────
                    // BsonDocument es el equivalente a un objeto JSON en MongoDB.
                    // Cada línea { "campo", valor } define un campo del documento
                    // que se almacenará en la colección "Reuniones".
                    // ─────────────────────────────────────────────────────
                    var nuevaReunion = new BsonDocument
                    {
                        { "idReunion",      int.Parse(txtIdReunion.Text) },
                        { "fechaReunion",   dtpFechaReunion.Value.ToString("yyyy-MM-dd") },
                        { "horaInicio",     dtpHoraInicioReunion.Value.ToString("HH:mm") },
                        { "horaFin",        dtpHoraFinalReunion.Value.ToString("HH:mm") },
                        { "motivoReunion",  txtMotivoReunion.Text.Trim() },
                        { "lugarReunion",   cboLugarReunion.Text },
                        { "idLider",        int.Parse(txtIdLider.Text) },
                        { "idInvestigadores", seleccionadosIds }  // Array de IDs
                    };

                    // InsertOne → inserta un único documento en la colección
                    reunionesCol.InsertOne(nuevaReunion);

                    MessageBox.Show("Reunión guardada exitosamente.",
                                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LimpiarFormulario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // LIMPIAR FORMULARIO
        //
        // Se llama después de guardar exitosamente una reunión.
        // Resetea todos los campos editables para permitir crear
        // una nueva reunión sin necesidad de cerrar y reabrir el formulario.
        // También regenera el ID para la próxima reunión automáticamente.
        // ─────────────────────────────────────────────────────────────────
        private void LimpiarFormulario()
        {
            txtMotivoReunion.Clear();
            cboLugarReunion.SelectedIndex = -1;

            dtpFechaReunion.Value = DateTime.Now;
            dtpHoraInicioReunion.Value = DateTime.Now;
            dtpHoraFinalReunion.Value = DateTime.Now.AddHours(1);

            for (int i = 0; i < clbListaInvestigadores.Items.Count; i++)
                clbListaInvestigadores.SetItemChecked(i, false);

            // Regenera el siguiente ID automáticamente
            GenerarIdReunion();

            txtMotivoReunion.Focus();
        }

        // ─────────────────────────────────────────────────────────────────
        // BOTÓN CANCELAR
        // Pregunta confirmación antes de cerrar el formulario.
        // ─────────────────────────────────────────────────────────────────
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "¿Estás seguro de cancelar la creación de la reunión?",
                "Cancelar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                this.Close();
        }

        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void cboLugarReunion_SelectedIndexChanged(object sender, EventArgs e) { }
    }

    // ─────────────────────────────────────────────────────────────────────
    // CLASE AUXILIAR: ItemInvestigador
    //
    // Encapsula los datos de cada investigador que se carga en el
    // CheckedListBox. Guarda tanto el Nombre (para mostrarlo al usuario)
    // como el Id (para guardarlo en MongoDB al crear la reunión).
    //
    // ToString() devuelve el Nombre, que es lo que el ListBox renderiza
    // visualmente en cada fila del CheckedListBox.
    // ─────────────────────────────────────────────────────────────────────
    public class ItemInvestigador
    {
        public string Nombre { get; set; }
        public int Id { get; set; }

        public override string ToString() => Nombre;
    }
}