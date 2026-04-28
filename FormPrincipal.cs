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
            MessageBox.Show($"Bienvenido, {datosUsuario.Nombre} ({datosUsuario.Rol})", "Bienvenida", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
            var result = MessageBox.Show("¿Estás seguro de cerrar la sesión?", "Salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Form1 form1 = new Form1();
                form1.Show();
                this.Close();
            }
        }

        private void btnAgregarReunión_Click(object sender, EventArgs e)
        {
            FormAgregar formAgregar = new FormAgregar(this.datosUsuario);
            formAgregar.ShowDialog();
        }

        private async void btnVerReunion_Click(object sender, EventArgs e)
        {
            try
            {
                var db = Conexion.ObtenerBaseDatos(); // Obtener la base de datos
                var colReuniones = db.GetCollection<BsonDocument>("Reuniones"); // Obtener la colección de reuniones
                var colUsuarios = db.GetCollection<BsonDocument>("Usuarios"); // Obtener la colección de usuarios

                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario); // Convertir el ID del usuario a entero para compararlo con los IDs en la base de datos

                // FILTRO según rol
                FilterDefinition<BsonDocument> filtro;

                if (datosUsuario.Rol == "Líder")
                {
                    filtro = Builders<BsonDocument>.Filter.Eq("idLider", idUsuario); // Si es Líder, buscamos coincidencia exacta en el campo idLider
                }
                else
                {
                    filtro = Builders<BsonDocument>.Filter.AnyEq("idInvestigadores", idUsuario); // Si no es Líder, buscamos si el idUsuario está dentro de la lista de investigadores
                }

                var reuniones = await colReuniones.Find(filtro).ToListAsync(); // Obtener las reuniones que corresponden al filtro

                // Verificar si se encontraron reuniones antes de intentar procesarlas
                if (reuniones.Count == 0)
                {
                    dataGridView1.DataSource = null;

                    string mensajeAMostrar; // 1. Declaramos una variable para el mensaje

                    if (datosUsuario.Rol == "Líder")
                    {
                        mensajeAMostrar = "No hay reuniones en tu semillero.";
                    }
                    else
                    {
                        mensajeAMostrar = "No estás en ninguna reunión.";
                    }

                    // 3. Pasamos la variable al MessageBox
                    MessageBox.Show(mensajeAMostrar, "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information
                    );

                    return;
                }

                // Recolectar IDs necesarios (lider + investigadores)
                var ids = new HashSet<int>();

                foreach (var r in reuniones) // Agregar ID del líder
                {
                    ids.Add(r["idLider"].ToInt32()); // Agregar IDs de investigadores

                    // Verificar si el campo "idInvestigadores" existe antes de intentar acceder a él
                    if (r.Contains("idInvestigadores"))
                    {
                        foreach (var id in r["idInvestigadores"].AsBsonArray) // Agregar cada ID de investigador a la colección de IDs
                        {
                            ids.Add(id.ToInt32()); // Agregar el ID del investigador al HashSet (esto automáticamente evita duplicados)
                        }    
                    }  
                }

                // Traer usuarios una sola vez
                var filtroUsuarios = Builders<BsonDocument>.Filter.In("idUsuario", ids); // Filtrar usuarios por los IDs recolectados
                var usuarios = await colUsuarios.Find(filtroUsuarios).ToListAsync(); // Obtener los usuarios que corresponden al filtro

                // Crear diccionario para mapear ID de usuario a nombre
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

                foreach (var r in reuniones) // Llenar la tabla con los datos de las reuniones
                {
                    // 1. Obtenemos el ID del líder desde el documento BSON
                    int idLider = r["idLider"].ToInt32();
                    string nombreLider;

                    // 2. Verificamos si ese ID existe en nuestro "diccionario" de nombres
                    if (nombrePorId.ContainsKey(idLider))
                    {
                        nombreLider = nombrePorId[idLider]; // Si existe, asignamos el nombre correspondiente
                    }
                    else
                    {
                        nombreLider = "ID " + idLider; // Si no existe, creamos un texto genérico con el ID para no dejar el campo vacío
                    }

                    string asistentes = ""; // Inicializar variable para asistentes

                    if (r.Contains("idInvestigadores")) // Verificar si el campo "idInvestigadores" existe antes de intentar acceder a él
                    {
                        // Crear una lista de nombres de asistentes usando el diccionario, o mostrar el ID si no se encuentra
                        asistentes = string.Join(Environment.NewLine,
                            r["idInvestigadores"].AsBsonArray.Select(x =>
                            {
                                int id = x.ToInt32();
                                string nombre;

                                // 1. Decidimos qué nombre asignar
                                if (nombrePorId.ContainsKey(id))
                                {
                                    
                                    nombre = nombrePorId[id]; // Si el ID está en el diccionario, usamos el nombre real
                                }
                                else
                                {
                                    nombre = "ID " + id; // Si no está, usamos el texto de respaldo con el ID
                                }

                                return "• " + nombre; // 2. Retornamos el nombre con el formato de viñeta
                            }));
                    }

                    // Agregar la fila a la tabla
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
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
