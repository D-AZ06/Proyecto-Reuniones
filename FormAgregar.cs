using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
            ConfigurarConexion();
            InitializeComponent();
            this.usuarioLogueado = datosRecibidos;

            // Eventos
            dtpFechaReunion.ValueChanged += dtpFechaReunion_ValueChanged;
            dtpHoraInicioReunion.ValueChanged += dtpHoraInicioReunion_ValueChanged;
            dtpHoraFinalReunion.ValueChanged += dtpHoraFinalReunion_ValueChanged;
        }

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
            catch (Exception ex) { MessageBox.Show("Error de conexión: " + ex.Message); }
        }

        private void FormAgregar_Load(object sender, EventArgs e)
        {
            cargandoFormulario = true;
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
                }
            }
            finally { cargandoFormulario = false; }
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

            if (dtpHoraFinalReunion.Value.TimeOfDay > new TimeSpan(17, 59, 0) || dtpHoraFinalReunion.Value.Hour >= 18)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin, "Máximo 05:59 p.m.");
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
                    e.NewValue = CheckState.Unchecked;
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
                    var idsDB = doc["idInvestigadores"].AsBsonArray.Select(x => x.AsInt32);
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

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMotivoReunion.Text) || clbListaInvestigadores.CheckedItems.Count == 0)
            {
                MessageBox.Show("Faltan datos."); return;
            }
            try
            {
                BsonArray invs = new BsonArray();
                foreach (object item in clbListaInvestigadores.CheckedItems) invs.Add(((ItemInvestigador)item).Id);

                var doc = new BsonDocument {
                    { "idReunion", int.Parse(txtIdReunion.Text) },
                    { "fechaReunion", dtpFechaReunion.Value.ToString("yyyy-MM-dd") },
                    { "horaInicio", dtpHoraInicioReunion.Value.ToString("HH:mm") },
                    { "horaFin", dtpHoraFinalReunion.Value.ToString("HH:mm") },
                    { "motivoReunion", txtMotivoReunion.Text },
                    { "lugarReunion", cboLugarReunion.Text },
                    { "idLider", int.Parse(txtIdLider.Text) },
                    { "idInvestigadores", invs }
                };
                reunionesCol.InsertOne(doc);
                MessageBox.Show("Guardado.");
                LimpiarFormulario();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
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
    }

    public class ItemInvestigador
    {
        public string Nombre { get; set; }
        public int Id { get; set; }
        public override string ToString() => Nombre;
    }
}