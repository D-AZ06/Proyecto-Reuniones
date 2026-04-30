using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_Reuniones
{
    public partial class FormAgregar : Form
    {
        private IMongoCollection<BsonDocument> usuariosCol;
        private IMongoCollection<BsonDocument> reunionesCol;
        private DatosUsuario usuarioLogueado;
        private bool cargandoFormulario = true;

        private DateTime valorAnteriorInicio;
        private DateTime valorAnteriorFin;
        

        public FormAgregar(DatosUsuario datosRecibidos)
        {
            
            InitializeComponent();

            // Llamada directa al método estático de tu clase Conexion
            var database = Conexion.ObtenerBaseDatos();

            if (database != null)
            {
                usuariosCol = database.GetCollection<BsonDocument>("Usuarios");
                reunionesCol = database.GetCollection<BsonDocument>("Reuniones");
            }

            this.usuarioLogueado = datosRecibidos;

            // Eventos
            dtpFechaReunion.ValueChanged += dtpFechaReunion_ValueChanged;
            dtpHoraInicioReunion.ValueChanged += dtpHoraInicioReunion_ValueChanged;
            dtpHoraFinalReunion.ValueChanged += dtpHoraFinalReunion_ValueChanged;
        }

        

        private async void FormAgregar_Load(object sender, EventArgs e)
        {
            cargandoFormulario = true;
            txtIdReunion.ReadOnly = true;
            txtLiderResponsable.ReadOnly = true;
            txtIdLider.ReadOnly = true;
            txtIdSemillero.ReadOnly = true;

         
            txtIdReunion.BackColor = SystemColors.ControlLight;
            txtLiderResponsable.BackColor = SystemColors.ControlLight;
            try
            {
                DateTime ahora = DateTime.Now;
                dtpFechaReunion.MinDate = DateTime.Today;
                dtpFechaReunion.Value = ahora.Date;

                SincronizarHoras(ahora, ahora.AddMinutes(30));

                if (usuarioLogueado != null)
                {
                    txtLiderResponsable.Text = usuarioLogueado.Nombre;
                    txtIdLider.Text = usuarioLogueado.IdUsuario.ToString();
                    txtIdSemillero.Text = usuarioLogueado.IdSemillero.ToString();
                    GenerarIdReunion();
                    CargarInvestigadores(usuarioLogueado.IdSemillero);
                    await CargarSugerenciasLugares();
                }
            }
            finally { cargandoFormulario = false; }
        }

        private async Task CargarSugerenciasLugares()
        {
            try
            {
                var db = Conexion.ObtenerBaseDatos();
                var colReuniones = db.GetCollection<BsonDocument>("Reuniones");
                int idUsuario = usuarioLogueado.IdUsuario;

                // Filtro: Si es Líder busca por idLider, si es Investigador busca dentro del array
                FilterDefinition<BsonDocument> filtroPorRol;

                if (usuarioLogueado.Rol == "Líder")
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.Eq("idLider", idUsuario);
                }
                else
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                        "idInvestigadores",
                        new BsonDocument("idInvestigador", idUsuario)
                    );
                }

                // Obtenemos los lugares de las reuniones donde ha participado
                var reunionesDelUsuario = await colReuniones.Find(filtroPorRol).ToListAsync();

                var lugares = reunionesDelUsuario
                    .Select(r => r["lugarReunion"].AsString)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Distinct()
                    .OrderBy(l => l)
                    .ToList();

                // Llenar el ComboBox en el hilo de la interfaz
                this.Invoke(new Action(() => {
                    cboLugarReunion.Items.Clear();
                    foreach (var lugar in lugares)
                    {
                        cboLugarReunion.Items.Add(lugar);
                    }
                }));
            }
            catch (Exception ex)
            {
                // Silencioso o un log pequeño para no interrumpir al usuario
                Console.WriteLine("Error cargando sugerencias: " + ex.Message);
            }
        }

        // --- NUEVA LÓGICA DE CAMBIO DE FECHA ---
        private void dtpFechaReunion_ValueChanged(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            DateTime ahora = DateTime.Now;

            // Si regresan a "Hoy", ponemos las horas actuales sin mandar errores
            if (dtpFechaReunion.Value.Date == ahora.Date)
            {
                SincronizarHoras(ahora, ahora.AddMinutes(30));
            }
            else
            {
                // Si es un día futuro, validamos que el bloque actual sea coherente (6 AM mínimo)
                TimeSpan min = new TimeSpan(6, 0, 0);
                if (dtpHoraInicioReunion.Value.TimeOfDay < min)
                {
                    SincronizarHoras(dtpFechaReunion.Value.Date.AddHours(6), dtpFechaReunion.Value.Date.AddHours(6).AddMinutes(30));
                }
            }
        }

        // Método para mover ambos controles sin disparar los MessageBox de error
        private void SincronizarHoras(DateTime inicio, DateTime fin)
        {
            cargandoFormulario = true;

            dtpHoraInicioReunion.Value = inicio;
            dtpHoraFinalReunion.Value = fin;

            valorAnteriorInicio = inicio;
            valorAnteriorFin = fin;

            cargandoFormulario = false;
        }

        // --- VALIDACIONES CON FRENO SECO ---

        private void dtpHoraInicioReunion_ValueChanged(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            DateTime ahora = DateTime.Now;
            TimeSpan inicio = dtpHoraInicioReunion.Value.TimeOfDay;

            if (inicio < new TimeSpan(6, 0, 0))
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio, "Mínimo 06:00 a.m.");
                return;
            }

            if (dtpFechaReunion.Value.Date == ahora.Date && dtpHoraInicioReunion.Value < ahora.AddMinutes(-1))
            {
                RestaurarValor(dtpHoraInicioReunion, ahora, "La hora ya pasó.");
                return;
            }

            if (dtpHoraInicioReunion.Value >= dtpHoraFinalReunion.Value)
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio, "Debe ser menor a la hora de fin.");
                return;
            }

            valorAnteriorInicio = dtpHoraInicioReunion.Value;
        }

        private void dtpHoraFinalReunion_ValueChanged(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            if (dtpHoraFinalReunion.Value.TimeOfDay > new TimeSpan(18, 0, 0) || dtpHoraFinalReunion.Value.Hour >= 18)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin, "Máximo 06:00 p.m.");
                return;
            }

            if (dtpHoraFinalReunion.Value <= dtpHoraInicioReunion.Value)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin, "Debe ser posterior al inicio.");
                return;
            }

            valorAnteriorFin = dtpHoraFinalReunion.Value;
        }

        private void RestaurarValor(DateTimePicker control, DateTime valor, string msg)
        {
            cargandoFormulario = true;
            MessageBox.Show(msg, "Validación");
            control.Value = valor;
            cargandoFormulario = false;
        }

        // --- MÉTODOS DE BASE DE DATOS (MANTENIDOS) ---

        private void GenerarIdReunion()
        {
            try
            {
                var ultimo = reunionesCol.Find(new BsonDocument()).Sort("{idReunion: -1}").Limit(1).FirstOrDefault();
                txtIdReunion.Text = (ultimo != null ? ultimo["idReunion"].AsInt32 + 1 : 1).ToString();
            }
            catch { }
        }

        private void CargarInvestigadores(int idSemillero)
        {
            try
            {
                var filtro = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("idSemillero", idSemillero),
                    Builders<BsonDocument>.Filter.Eq("rolUsuario", "Investigador")
                );
                var lista = usuariosCol.Find(filtro).ToList();
                clbListaInvestigadores.Items.Clear();
                foreach (var doc in lista)
                    clbListaInvestigadores.Items.Add(new ItemInvestigador { Nombre = doc["nombreUsuario"].AsString, Id = doc["idUsuario"].AsInt32 });
            }
            catch { }
        }

        private void clbListaInvestigadores_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                var inv = (ItemInvestigador)clbListaInvestigadores.Items[e.Index];

                if (ExisteConflicto(dtpFechaReunion.Value.Date, dtpHoraInicioReunion.Value.TimeOfDay, dtpHoraFinalReunion.Value.TimeOfDay, new List<int> { inv.Id }))
                {
                    MessageBox.Show($"El investigador {inv.Nombre} tiene un conflicto de horario.", "Conflicto Detectado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.NewValue = CheckState.Unchecked;
                }
            }
        }

        private bool ExisteConflicto(DateTime fecha, TimeSpan tI, TimeSpan tF, List<int> ids)
        {
            try
            {
                var filtro = Builders<BsonDocument>.Filter.Eq("fechaReunion", fecha.ToString("yyyy-MM-dd"));
                var reuniones = reunionesCol.Find(filtro).ToList();
                foreach (var doc in reuniones)
                {
                    var idsDB = doc["investigadoresConvocados"].AsBsonArray.Select(x => x.AsBsonDocument["idInvestigador"].AsInt32);
                    if (ids.Any(id => idsDB.Contains(id)))
                    {
                        TimeSpan dbI = TimeSpan.Parse(doc["horaInicio"].AsString);
                        TimeSpan dbF = TimeSpan.Parse(doc["horaFin"].AsString);
                        if (tI < dbF && tF > dbI)
                        {
                            MessageBox.Show($"Conflicto con la reunión: {doc["motivoReunion"]}");
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private void LimpiarFormulario()
        {
            cargandoFormulario = true;
            txtMotivoReunion.Clear();
            cboLugarReunion.SelectedIndex = -1;
            DateTime ahora = DateTime.Now;
            dtpFechaReunion.Value = ahora.Date;
            SincronizarHoras(ahora, ahora.AddMinutes(30));
            for (int i = 0; i < clbListaInvestigadores.Items.Count; i++) clbListaInvestigadores.SetItemChecked(i, false);
            GenerarIdReunion();
            cargandoFormulario = false;
        }
        private void btnCancelar_Click(object sender, EventArgs e) => this.Close();

        private void btnAgregarReunion_Click(object sender, EventArgs e)
        {
            // 1. Validar que el motivo no esté vacío
            if (string.IsNullOrWhiteSpace(txtMotivoReunion.Text))
            {
                MessageBox.Show("Por favor, ingrese el motivo de la reunión.", "Datos Faltantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMotivoReunion.Focus();
                return;
            }

            // 2. Validar que se haya seleccionado un lugar
            if (cboLugarReunion.SelectedIndex == -1 && string.IsNullOrWhiteSpace(cboLugarReunion.Text))
            {
                MessageBox.Show("Por favor, seleccione o ingrese un lugar para la reunión.", "Datos Faltantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLugarReunion.Focus();
                return;
            }

            // 3. Validar que haya al menos un investigador seleccionado
            if (clbListaInvestigadores.CheckedItems.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos un investigador para la reunión.", "Sin Participantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMotivoReunion.Text) || clbListaInvestigadores.CheckedItems.Count == 0)
            {
                MessageBox.Show("Faltan datos."); return;
            }
            DialogResult resultado = MessageBox.Show("¿Está seguro de que desea agendar esta reunión?", "Confirmar Guardado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    // Nueva lógica para crear el arreglo de objetos con asistencia
                    BsonArray invs = new BsonArray();
                    foreach (object item in clbListaInvestigadores.CheckedItems)
                    {
                        var investigador = (ItemInvestigador)item;

                        // Creamos un sub-documento para cada investigador
                        var convocadoDoc = new BsonDocument {
                            { "idInvestigador", investigador.Id },
                            { "asistencia", "pendiente" } // El campo que pediste
                        };

                        invs.Add(convocadoDoc);
                    }

                    // Crear el documento principal para MongoDB
                    var doc = new BsonDocument {
                        { "idReunion", int.Parse(txtIdReunion.Text) },
                        { "fechaReunion", dtpFechaReunion.Value.ToString("yyyy-MM-dd") },
                        { "horaInicio", dtpHoraInicioReunion.Value.ToString("HH:mm") },
                        { "horaFin", dtpHoraFinalReunion.Value.ToString("HH:mm") },
                        { "motivoReunion", txtMotivoReunion.Text.Trim() },
                        { "lugarReunion", cboLugarReunion.Text.Trim() },
                        { "idLider", int.Parse(txtIdLider.Text) },
                        { "investigadoresConvocados", invs } // Aquí ahora se guarda el arreglo de objetos
                    };

                    reunionesCol.InsertOne(doc);
                    // --- HASTA AQUÍ ---

                    MessageBox.Show("La reunión ha sido guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message);
                }
            }
        }
    }

    public class ItemInvestigador
    {
        public string Nombre { get; set; }
        public int Id { get; set; }
        public override string ToString() => Nombre;
    }
}