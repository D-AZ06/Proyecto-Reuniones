using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private int _idReunionGenerado;

        // ── Constantes de reglas de negocio ──────────────────────────────────
        private const int HORA_MINIMA = 6;
        private const int HORA_MAXIMA = 22;
        private const int DURACION_MIN_MIN = 30;
        private const int DURACION_MAX_HRS = 5;
        private const int MOTIVO_MIN_CHARS = 10;
        private const int MOTIVO_MAX_CHARS = 500;
        private const double RATIO_VOCALES_MIN = 0.20;

        public bool modoEdicion = false;
        public BsonDocument reunionAEditar = null;

        public FormAgregar(DatosUsuario datosRecibidos)
        {
            InitializeComponent();
            this.usuarioLogueado = datosRecibidos;

            dtpFechaReunion.ValueChanged += dtpFechaReunion_ValueChanged;
            dtpHoraInicioReunion.ValueChanged += dtpHoraInicioReunion_ValueChanged;
            dtpHoraFinalReunion.ValueChanged += dtpHoraFinalReunion_ValueChanged;
            cboLugarReunion.Leave += cboLugarReunion_Leave;

            // ── Evento en tiempo real para el label del motivo ──────────────
            txtMotivoReunion.TextChanged += txtMotivoReunion_TextChanged;
        }

        // ════════════════════════════════════════════════════════════════════
        //  CARGA DEL FORMULARIO
        // ════════════════════════════════════════════════════════════════════
        private void FormAgregar_Load(object sender, EventArgs e)
        {
            timer1.Start();
            ActualizarReloj();
            lblNombreYApellido.Text = usuarioLogueado.Nombre;

            try
            {
                var database = Conexion.ObtenerBaseDatos();
                if (database == null)
                {
                    MessageBox.Show(
                        "No fue posible establecer conexión con la base de datos.\n" +
                        "Verifique su conexión e intente de nuevo.",
                        "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
                usuariosCol = database.GetCollection<BsonDocument>("Usuarios");
                reunionesCol = database.GetCollection<BsonDocument>("Reuniones");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error al conectar con la base de datos:\n\n{ex.Message}",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            if (usuarioLogueado == null)
            {
                MessageBox.Show(
                    "No se encontró una sesión activa.\nPor favor, inicie sesión nuevamente.",
                    "Sesión inválida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            cargandoFormulario = true;

            txtIdReunion.ReadOnly = true;
            txtLiderResponsable.ReadOnly = true;
            txtIdLider.ReadOnly = true;
            txtIdSemillero.ReadOnly = true;
            txtIdReunion.BackColor = SystemColors.ControlLight;
            txtLiderResponsable.BackColor = SystemColors.ControlLight;

            txtMotivoReunion.MaxLength = MOTIVO_MAX_CHARS;

            // Estado inicial del label y botón
            lblMensajeError.Text = string.Empty;
            lblMensajeError.ForeColor = Color.Gray;
            btnAgregarReunion.Enabled = true;

            try
            {
                DateTime ahora = DateTime.Now;
                dtpFechaReunion.MinDate = DateTime.Today;
                dtpFechaReunion.Value = ahora.Date;

                SincronizarHoras(ahora, ahora.AddMinutes(DURACION_MIN_MIN));

                txtLiderResponsable.Text = usuarioLogueado.Nombre;
                txtIdLider.Text = usuarioLogueado.IdUsuario.ToString();
                txtIdSemillero.Text = usuarioLogueado.IdSemillero.ToString();
                GenerarIdReunion();
                CargarInvestigadores(usuarioLogueado.IdSemillero);
                CargarLugares();

                if (modoEdicion && reunionAEditar != null)
                {
                    txtIdReunion.Text = reunionAEditar["idReunion"].ToInt32().ToString();
                    this.Text = "Editar reunión";
                    btnAgregarReunion.Text = "Modificar Reunión";

                    txtMotivoReunion.Text = reunionAEditar["motivoReunion"].AsString;
                    cboLugarReunion.Text = reunionAEditar["lugarReunion"].AsString;

                    if (DateTime.TryParse(reunionAEditar["fechaReunion"].AsString, out DateTime fecha))
                        dtpFechaReunion.Value = fecha;

                    if (DateTime.TryParse(reunionAEditar["fechaReunion"].AsString + " " + reunionAEditar["horaInicio"].AsString, out DateTime ini) &&
                        DateTime.TryParse(reunionAEditar["fechaReunion"].AsString + " " + reunionAEditar["horaFin"].AsString, out DateTime fin))
                        SincronizarHoras(ini, fin);

                    if (reunionAEditar.Contains("investigadoresConvocados"))
                    {
                        var idsConvocados = new HashSet<int>();
                        foreach (var conv in reunionAEditar["investigadoresConvocados"].AsBsonArray)
                            idsConvocados.Add(conv["idInvestigador"].ToInt32());

                        for (int i = 0; i < clbListaInvestigadores.Items.Count; i++)
                        {
                            var inv = (ItemInvestigador)clbListaInvestigadores.Items[i];
                            if (idsConvocados.Contains(inv.Id))
                                clbListaInvestigadores.SetItemChecked(i, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Se produjo un error al cargar el formulario:\n\n{ex.Message}",
                    "Error al cargar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { cargandoFormulario = false; }
        }

        // ════════════════════════════════════════════════════════════════════
        //  LABEL EN TIEMPO REAL — MOTIVO
        // ════════════════════════════════════════════════════════════════════
        private void txtMotivoReunion_TextChanged(object sender, EventArgs e)
        {
            int len = txtMotivoReunion.Text.Trim().Length;

            if (len == 0)
            {
                lblMensajeError.Text = string.Empty;
                lblMensajeError.ForeColor = Color.Gray;
                btnAgregarReunion.Enabled = true;
                return;
            }

            if (len < MOTIVO_MIN_CHARS)
            {
                lblMensajeError.Text = $"⚠ Mínimo {MOTIVO_MIN_CHARS} caracteres  ({len}/{MOTIVO_MIN_CHARS})";
                lblMensajeError.ForeColor = Color.OrangeRed;
                btnAgregarReunion.Enabled = false;
                return;
            }

            if (len > MOTIVO_MAX_CHARS)
            {
                lblMensajeError.Text = $"⚠ Máximo {MOTIVO_MAX_CHARS} caracteres  ({len}/{MOTIVO_MAX_CHARS})";
                lblMensajeError.ForeColor = Color.OrangeRed;
                btnAgregarReunion.Enabled = false;
                return;
            }

            // Dentro del rango válido
            lblMensajeError.Text = $"✔ {len}/{MOTIVO_MAX_CHARS} caracteres";
            lblMensajeError.ForeColor = Color.SeaGreen;
            btnAgregarReunion.Enabled = true;
        }

        // ════════════════════════════════════════════════════════════════════
        //  EVENTOS DE FECHA Y HORA
        // ════════════════════════════════════════════════════════════════════
        private void dtpFechaReunion_ValueChanged(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            if (dtpFechaReunion.Value.DayOfWeek == DayOfWeek.Sunday)
            {
                cargandoFormulario = true;
                MessageBox.Show(
                    "Las reuniones no pueden agendarse los domingos.\n" +
                    "Por favor seleccione un día hábil (lunes a sábado).",
                    "Día no permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                DateTime anteriorValido = dtpFechaReunion.Value.AddDays(-1);
                while (anteriorValido.DayOfWeek == DayOfWeek.Sunday)
                    anteriorValido = anteriorValido.AddDays(-1);

                if (anteriorValido < DateTime.Today)
                    anteriorValido = DateTime.Today;

                dtpFechaReunion.Value = anteriorValido;
                cargandoFormulario = false;
                return;
            }

            DateTime ahora = DateTime.Now;
            if (dtpFechaReunion.Value.Date == ahora.Date)
            {
                SincronizarHoras(ahora, ahora.AddMinutes(DURACION_MIN_MIN));
            }
            else
            {
                TimeSpan min = new TimeSpan(HORA_MINIMA, 0, 0);
                if (dtpHoraInicioReunion.Value.TimeOfDay < min)
                    SincronizarHoras(
                        dtpFechaReunion.Value.Date.AddHours(HORA_MINIMA),
                        dtpFechaReunion.Value.Date.AddHours(HORA_MINIMA).AddMinutes(DURACION_MIN_MIN));
            }
        }

        private void dtpHoraInicioReunion_ValueChanged(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            DateTime ahora = DateTime.Now;
            TimeSpan inicio = dtpHoraInicioReunion.Value.TimeOfDay;

            if (inicio < new TimeSpan(HORA_MINIMA, 0, 0))
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio,
                    "El horario de reuniones comienza a las 6:00 a.m.\nNo es posible agendar antes de esa hora.");
                return;
            }

            if (dtpFechaReunion.Value.Date == ahora.Date && dtpHoraInicioReunion.Value < ahora.AddMinutes(-1))
            {
                RestaurarValor(dtpHoraInicioReunion, ahora,
                    "La hora de inicio seleccionada ya ha pasado.\nPor favor elija una hora futura.");
                return;
            }

            if (dtpHoraInicioReunion.Value >= dtpHoraFinalReunion.Value)
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio,
                    "La hora de inicio debe ser anterior a la hora de finalización.");
                return;
            }

            if ((dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalMinutes < DURACION_MIN_MIN)
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio,
                    $"La duración mínima de una reunión es de {DURACION_MIN_MIN} minutos.\nAjuste el horario de inicio o finalización.");
                return;
            }

            valorAnteriorInicio = dtpHoraInicioReunion.Value;
        }

        private void dtpHoraFinalReunion_ValueChanged(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            if (dtpHoraFinalReunion.Value.TimeOfDay > new TimeSpan(HORA_MAXIMA, 0, 0))
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin,
                    "Las reuniones no pueden extenderse más allá de las 10:00 p.m.\nAjuste la hora de finalización.");
                return;
            }

            if (dtpHoraFinalReunion.Value <= dtpHoraInicioReunion.Value)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin,
                    "La hora de finalización debe ser posterior a la hora de inicio.");
                return;
            }

            if ((dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalMinutes < DURACION_MIN_MIN)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin,
                    $"La duración mínima de una reunión es de {DURACION_MIN_MIN} minutos.\nAjuste el horario de inicio o finalización.");
                return;
            }

            if ((dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalHours > DURACION_MAX_HRS)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin,
                    $"La duración máxima permitida para una reunión es de {DURACION_MAX_HRS} horas.\nAjuste la hora de finalización.");
                return;
            }

            valorAnteriorFin = dtpHoraFinalReunion.Value;
        }

        private void SincronizarHoras(DateTime inicio, DateTime fin)
        {
            cargandoFormulario = true;
            dtpHoraInicioReunion.Value = inicio;
            dtpHoraFinalReunion.Value = fin;
            valorAnteriorInicio = inicio;
            valorAnteriorFin = fin;
            cargandoFormulario = false;
        }

        private void RestaurarValor(DateTimePicker control, DateTime valor, string msg)
        {
            cargandoFormulario = true;
            MessageBox.Show(msg, "Hora no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Value = valor;
            cargandoFormulario = false;
        }

        // ════════════════════════════════════════════════════════════════════
        //  LISTA DE INVESTIGADORES
        // ════════════════════════════════════════════════════════════════════
        private void clbListaInvestigadores_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                var inv = (ItemInvestigador)clbListaInvestigadores.Items[e.Index];

                if (ExisteConflictoInvestigador(
                        dtpFechaReunion.Value.Date,
                        dtpHoraInicioReunion.Value.TimeOfDay,
                        dtpHoraFinalReunion.Value.TimeOfDay,
                        new List<int> { inv.Id },
                        out int idConflicto))
                {
                    MessageBox.Show(
                        $"No es posible agregar a {inv.Nombre} como participante.\n\n" +
                        $"Ya se encuentra convocado en la Reunión N.° {idConflicto},\n" +
                        $"la cual se superpone con el horario seleccionado.\n\n" +
                        $"Seleccione otro investigador o cambie el horario.",
                        "Conflicto de participante", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    e.NewValue = CheckState.Unchecked;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BASE DE DATOS
        // ════════════════════════════════════════════════════════════════════
        private void GenerarIdReunion()
        {
            try
            {
                var ultimo = reunionesCol
                    .Find(new BsonDocument())
                    .Sort("{idReunion: -1}")
                    .Limit(1)
                    .FirstOrDefault();

                _idReunionGenerado = (ultimo != null ? ultimo["idReunion"].AsInt32 + 1 : 1);
                txtIdReunion.Text = _idReunionGenerado.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No fue posible generar el ID de la reunión.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    clbListaInvestigadores.Items.Add(new ItemInvestigador
                    {
                        Nombre = doc["nombreUsuario"].AsString,
                        Id = doc["idUsuario"].AsInt32
                    });

                if (lista.Count == 0)
                    MessageBox.Show(
                        "No se encontraron investigadores registrados en su semillero.\n" +
                        "Contacte al administrador si cree que esto es un error.",
                        "Sin investigadores", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No fue posible cargar la lista de investigadores.\n\n{ex.Message}",
                    "Error al cargar investigadores", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  VALIDACIÓN DE MOTIVO (detecta texto sin sentido)
        // ════════════════════════════════════════════════════════════════════
        private string ValidarMotivo(string motivo)
        {
            string texto = motivo.Trim();

            if (texto.Length < MOTIVO_MIN_CHARS)
                return $"El motivo debe tener al menos {MOTIVO_MIN_CHARS} caracteres.";

            if (texto.Length > MOTIVO_MAX_CHARS)
                return $"El motivo no puede superar los {MOTIVO_MAX_CHARS} caracteres.";

            string textoLower = texto.ToLower();

            int vocales = textoLower.Count(c => "aeiouáéíóúü".Contains(c));
            if ((double)vocales / texto.Length < RATIO_VOCALES_MIN)
                return "El texto ingresado no parece ser una descripción válida.\n" +
                       "Por favor describa el propósito de la reunión con claridad.";

            int maxRepetidos = 0, cont = 1;
            for (int i = 1; i < textoLower.Length; i++)
            {
                cont = textoLower[i] == textoLower[i - 1] ? cont + 1 : 1;
                if (cont > maxRepetidos) maxRepetidos = cont;
            }
            if (maxRepetidos >= 4)
                return "El motivo contiene caracteres repetidos en exceso.\n" +
                       "Sea más descriptivo sobre el propósito de la reunión.";

            var palabras = texto.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var palabra in palabras)
            {
                if (palabra.Length > 15)
                    return "El motivo contiene palabras demasiado largas o sin espacios.\n" +
                           "Utilice un texto descriptivo normal.";

                if (palabra.Length > 6)
                {
                    int vP = palabra.ToLower().Count(c => "aeiouáéíóúü".Contains(c));
                    if ((double)vP / palabra.Length < 0.15)
                        return $"La palabra \"{palabra}\" no parece ser válida.\n" +
                               "Escriba un motivo que describa claramente la reunión.";
                }
            }

            if (palabras.Length < 2)
                return "El motivo debe contener al menos dos palabras.";

            return null;
        }

        // ════════════════════════════════════════════════════════════════════
        //  GESTIÓN DE LUGARES
        // ════════════════════════════════════════════════════════════════════
        private void CargarLugares()
        {
            try
            {
                var todasReuniones = reunionesCol
                    .Find(new BsonDocument())
                    .Project("{lugarReunion: 1, _id: 0}")
                    .ToList();

                var lugaresUnicos = todasReuniones
                    .Where(d => d.Contains("lugarReunion") && !string.IsNullOrWhiteSpace(d["lugarReunion"].AsString))
                    .Select(d => d["lugarReunion"].AsString.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(l => l)
                    .ToList();

                cboLugarReunion.Items.Clear();
                foreach (var lugar in lugaresUnicos)
                    cboLugarReunion.Items.Add(lugar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No fue posible cargar la lista de lugares.\n\n{ex.Message}",
                    "Error al cargar lugares", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AgregarLugarAlComboSiEsNuevo(string lugar)
        {
            if (string.IsNullOrWhiteSpace(lugar)) return;

            bool yaEsta = cboLugarReunion.Items
                .Cast<string>()
                .Any(l => l.Equals(lugar, StringComparison.OrdinalIgnoreCase));

            if (!yaEsta)
                cboLugarReunion.Items.Add(lugar);
        }

        private void cboLugarReunion_Leave(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            string lugar = cboLugarReunion.Text.Trim();
            if (string.IsNullOrWhiteSpace(lugar)) return;

            if (ExisteConflictoLugar(
                    dtpFechaReunion.Value.Date,
                    dtpHoraInicioReunion.Value.TimeOfDay,
                    dtpHoraFinalReunion.Value.TimeOfDay,
                    lugar,
                    out int idOcupado))
            {
                MessageBox.Show(
                    $"El lugar \"{lugar}\" no está disponible en el horario seleccionado.\n\n" +
                    $"Ya se encuentra reservado por la Reunión N.° {idOcupado}.\n\n" +
                    $"Seleccione un lugar diferente o cambie el horario de la reunión.",
                    "Lugar no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                cboLugarReunion.Text = string.Empty;
                cboLugarReunion.Focus();
                return;
            }

            AgregarLugarAlComboSiEsNuevo(lugar);
        }

        // ════════════════════════════════════════════════════════════════════
        //  CONFLICTOS (devuelven ID de la reunión, no el motivo)
        // ════════════════════════════════════════════════════════════════════
        private bool ExisteConflictoInvestigador(DateTime fecha, TimeSpan tI, TimeSpan tF,
                                                  List<int> ids, out int idReunionConflicto)
        {
            idReunionConflicto = 0;
            try
            {
                var filtro = Builders<BsonDocument>.Filter.Eq("fechaReunion", fecha.ToString("yyyy-MM-dd"));
                var reuniones = reunionesCol.Find(filtro).ToList();

                foreach (var doc in reuniones)
                {
                    if (!doc.Contains("investigadoresConvocados")) continue;

                    if (modoEdicion && reunionAEditar != null &&
                        doc["idReunion"].ToInt32() == reunionAEditar["idReunion"].ToInt32())
                        continue;

                    var idsDB = doc["investigadoresConvocados"]
                                    .AsBsonArray
                                    .Select(x => x.AsBsonDocument["idInvestigador"].AsInt32);

                    if (ids.Any(id => idsDB.Contains(id)))
                    {
                        TimeSpan dbI = TimeSpan.Parse(doc["horaInicio"].AsString);
                        TimeSpan dbF = TimeSpan.Parse(doc["horaFin"].AsString);

                        if (tI < dbF && tF > dbI)
                        {
                            idReunionConflicto = doc["idReunion"].AsInt32;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al verificar conflictos de participantes:\n\n{ex.Message}",
                    "Error de validación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private bool ExisteConflictoLugar(DateTime fecha, TimeSpan tI, TimeSpan tF,
                                           string lugar, out int idReunionConflicto)
        {
            idReunionConflicto = 0;
            try
            {
                var filtro = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("fechaReunion", fecha.ToString("yyyy-MM-dd")),
                    Builders<BsonDocument>.Filter.Eq("lugarReunion", lugar)
                );
                var reuniones = reunionesCol.Find(filtro).ToList();

                foreach (var doc in reuniones)
                {
                    if (modoEdicion && reunionAEditar != null &&
                        doc["idReunion"].ToInt32() == reunionAEditar["idReunion"].ToInt32())
                        continue;

                    TimeSpan dbI = TimeSpan.Parse(doc["horaInicio"].AsString);
                    TimeSpan dbF = TimeSpan.Parse(doc["horaFin"].AsString);

                    if (tI < dbF && tF > dbI)
                    {
                        idReunionConflicto = doc["idReunion"].AsInt32;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al verificar la disponibilidad del lugar:\n\n{ex.Message}",
                    "Error de validación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        // ════════════════════════════════════════════════════════════════════
        //  GUARDAR REUNIÓN
        // ════════════════════════════════════════════════════════════════════
        private void btnAgregarReunion_Click(object sender, EventArgs e)
        {
            // ── 1. Domingo ─────────────────────────────────────────────────
            if (dtpFechaReunion.Value.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show(
                    "Las reuniones no pueden agendarse los domingos.\nPor favor seleccione otro día.",
                    "Día no permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 2. Si es hoy, la hora de inicio no puede haber pasado ──────────
            if (dtpFechaReunion.Value.Date == DateTime.Today &&
                dtpHoraInicioReunion.Value.TimeOfDay < DateTime.Now.TimeOfDay)
            {
                MessageBox.Show(
                    "La hora de inicio seleccionada ya ha pasado.\n" +
                    "Por favor elija una hora futura.",
                    "Hora no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpHoraInicioReunion.Focus();
                return;
            }

            // ── 3. Motivo ──────────────────────────────────────────────────
            string errorMotivo = ValidarMotivo(txtMotivoReunion.Text);
            if (errorMotivo != null)
            {
                MessageBox.Show(errorMotivo, "Motivo inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMotivoReunion.Focus();
                return;
            }

            // ── 4. Lugar ───────────────────────────────────────────────────
            if (cboLugarReunion.SelectedIndex == -1 && string.IsNullOrWhiteSpace(cboLugarReunion.Text))
            {
                MessageBox.Show(
                    "Debe indicar el lugar donde se realizará la reunión.\n" +
                    "Seleccione una opción de la lista o escriba una nueva.",
                    "Lugar requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLugarReunion.Focus();
                return;
            }

            // ── 5. Duración mínima ─────────────────────────────────────────
            double duracionMin = (dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalMinutes;
            if (duracionMin < DURACION_MIN_MIN)
            {
                MessageBox.Show(
                    $"La reunión debe tener una duración mínima de {DURACION_MIN_MIN} minutos.\n" +
                    "Ajuste el horario de inicio o finalización.",
                    "Duración insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 6. Duración máxima ─────────────────────────────────────────
            if (duracionMin > DURACION_MAX_HRS * 60)
            {
                MessageBox.Show(
                    $"La duración máxima permitida para una reunión es de {DURACION_MAX_HRS} horas.\n" +
                    "Ajuste el horario de finalización.",
                    "Duración excedida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 7. Al menos un participante ────────────────────────────────
            if (clbListaInvestigadores.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    "Debe seleccionar al menos un investigador para convocar a la reunión.",
                    "Sin participantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 8. Disponibilidad del lugar ────────────────────────────────
            string lugarSeleccionado = cboLugarReunion.Text.Trim();
            if (ExisteConflictoLugar(
                    dtpFechaReunion.Value.Date,
                    dtpHoraInicioReunion.Value.TimeOfDay,
                    dtpHoraFinalReunion.Value.TimeOfDay,
                    lugarSeleccionado,
                    out int idLugarOcupado))
            {
                MessageBox.Show(
                    $"El lugar \"{lugarSeleccionado}\" no está disponible en el horario seleccionado.\n\n" +
                    $"Ya se encuentra reservado por la Reunión N.° {idLugarOcupado}.\n\n" +
                    $"Seleccione un lugar diferente o cambie el horario de la reunión.",
                    "Lugar no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLugarReunion.Focus();
                return;
            }

            // ── 9. Conflicto de participantes ──────────────────────────────
            var idsSeleccionados = clbListaInvestigadores.CheckedItems
                                        .Cast<ItemInvestigador>()
                                        .Select(i => i.Id)
                                        .ToList();

            if (ExisteConflictoInvestigador(
                    dtpFechaReunion.Value.Date,
                    dtpHoraInicioReunion.Value.TimeOfDay,
                    dtpHoraFinalReunion.Value.TimeOfDay,
                    idsSeleccionados,
                    out int idInvOcupado))
            {
                MessageBox.Show(
                    $"Uno o más participantes seleccionados ya están convocados\n" +
                    $"en la Reunión N.° {idInvOcupado} con el mismo horario.\n\n" +
                    $"Revise la lista de participantes o cambie el horario.",
                    "Conflicto de participantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── Confirmación ───────────────────────────────────────────────
            string msgConf = modoEdicion
                ? "¿Está seguro de que desea guardar los cambios en esta reunión?"
                : "¿Está seguro de que desea agendar esta reunión?";
            string titConf = modoEdicion ? "Confirmar edición" : "Confirmar guardado";

            if (MessageBox.Show(msgConf, titConf,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                BsonArray invs = new BsonArray();
                foreach (ItemInvestigador inv in clbListaInvestigadores.CheckedItems)
                    invs.Add(new BsonDocument {
                        { "idInvestigador", inv.Id },
                        { "asistencia", "pendiente" }
                    });

                if (modoEdicion && reunionAEditar != null)
                {
                    var filtroUpdate = Builders<BsonDocument>.Filter.Eq("idReunion", reunionAEditar["idReunion"].ToInt32());
                    var update = Builders<BsonDocument>.Update
                        .Set("fechaReunion", dtpFechaReunion.Value.ToString("yyyy-MM-dd"))
                        .Set("horaInicio", dtpHoraInicioReunion.Value.ToString("HH:mm"))
                        .Set("horaFin", dtpHoraFinalReunion.Value.ToString("HH:mm"))
                        .Set("motivoReunion", txtMotivoReunion.Text.Trim())
                        .Set("lugarReunion", lugarSeleccionado)
                        .Set("investigadoresConvocados", invs);

                    reunionesCol.UpdateOne(filtroUpdate, update);
                    MessageBox.Show(
                        "Los cambios de la reunión han sido guardados exitosamente.",
                        "Reunión actualizada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    var doc = new BsonDocument {
                        { "idReunion",        _idReunionGenerado },
                        { "fechaReunion",     dtpFechaReunion.Value.ToString("yyyy-MM-dd") },
                        { "horaInicio",       dtpHoraInicioReunion.Value.ToString("HH:mm") },
                        { "horaFin",          dtpHoraFinalReunion.Value.ToString("HH:mm") },
                        { "motivoReunion",    txtMotivoReunion.Text.Trim() },
                        { "lugarReunion",     lugarSeleccionado },
                        { "idLider",          usuarioLogueado.IdUsuario },
                        { "investigadoresConvocados", invs }
                    };

                    reunionesCol.InsertOne(doc);
                    MessageBox.Show(
                        "La reunión ha sido agendada exitosamente.",
                        "Reunión guardada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error al guardar la reunión:\n\n{ex.Message}",
                    "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  LIMPIAR / CANCELAR
        // ════════════════════════════════════════════════════════════════════
        private void LimpiarFormulario()
        {
            cargandoFormulario = true;
            txtMotivoReunion.Clear();
            cboLugarReunion.SelectedIndex = -1;
            cboLugarReunion.Text = string.Empty;  // ← vacía también lo escrito
            lblMensajeError.Text = string.Empty;
            lblMensajeError.ForeColor = Color.Gray;
            btnAgregarReunion.Enabled = true;
            DateTime ahora = DateTime.Now;
            dtpFechaReunion.Value = ahora.Date;
            SincronizarHoras(ahora, ahora.AddMinutes(DURACION_MIN_MIN));
            for (int i = 0; i < clbListaInvestigadores.Items.Count; i++)
                clbListaInvestigadores.SetItemChecked(i, false);
            GenerarIdReunion();
            cargandoFormulario = false;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "¿Está seguro de que desea cancelar?\nLos datos ingresados se perderán.",
                    "Confirmar cancelación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                this.Close();
        }

        // ════════════════════════════════════════════════════════════════════
        //  RELOJ
        // ════════════════════════════════════════════════════════════════════
        private void ActualizarReloj()
        {
            lblFechaHora.Text = DateTime.Now.ToString("yyyy/MM/dd  HH:mm:ss");
        }
        private void timer1_Tick(object sender, EventArgs e) => ActualizarReloj();

        // ── Stubs vacíos generados por el diseñador ──────────────────────────
        private void button1_Click(object sender, EventArgs e) { }
        private void dtpHoraFinalReunion_ValueChanged_1(object sender, EventArgs e) { }
        private void dtpHoraInicioReunion_ValueChanged_1(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void dtpFechaReunion_ValueChanged_1(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void pictureBox6_Click(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CLASE AUXILIAR
    // ════════════════════════════════════════════════════════════════════════
    public class ItemInvestigador
    {
        public string Nombre { get; set; }
        public int Id { get; set; }
        public override string ToString() => Nombre;
    }
}