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
            txtMotivoReunion.TextChanged += txtMotivoReunion_TextChanged;
        }

        // Se ejecuta cuando el formulario abre. Conecta la BD, verifica la sesión,
        // deja los campos de solo lectura en gris, y carga todo lo necesario:
        // investigadores del semillero, lugares anteriores, y si es edición, los datos de la reunión.
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

            lblMensajeError.Text = string.Empty;
            lblMensajeError.ForeColor = Color.Gray;
            btnAgregarReunion.Enabled = true;

            try
            {
                DateTime ahora = DateTime.Now;
                dtpFechaReunion.MinDate = DateTime.Today;

                DateTime inicioValido = ahora.AddHours(1);
                if (inicioValido.TimeOfDay < new TimeSpan(HORA_MINIMA, 0, 0))
                    inicioValido = ahora.Date.AddHours(HORA_MINIMA);
                else if (inicioValido.TimeOfDay >= new TimeSpan(HORA_MAXIMA, 0, 0))
                    inicioValido = ahora.Date.AddDays(1).AddHours(HORA_MINIMA);

                dtpFechaReunion.Value = inicioValido.Date;
                SincronizarHoras(inicioValido, inicioValido.AddMinutes(DURACION_MIN_MIN));

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

        // Muestra en tiempo real cuántos caracteres lleva el motivo y bloquea
        // el botón si está fuera del rango permitido (10 a 500 caracteres).
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

            lblMensajeError.Text = $"✔ {len}/{MOTIVO_MAX_CHARS} caracteres";
            lblMensajeError.ForeColor = Color.SeaGreen;
            btnAgregarReunion.Enabled = true;
        }

        // Los tres eventos de los DateTimePicker. Cada uno frena al usuario si intenta
        // poner un valor inválido. Al cambiar fecha u hora se recarga la lista de
        // investigadores y lugares para reflejar en tiempo real quién está disponible.
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
                DateTime inicioHoy = ahora.AddHours(1);
                if (inicioHoy.TimeOfDay < new TimeSpan(HORA_MINIMA, 0, 0))
                    inicioHoy = ahora.Date.AddHours(HORA_MINIMA);
                else if (inicioHoy.TimeOfDay >= new TimeSpan(HORA_MAXIMA, 0, 0))
                {
                    DateTime manana = ahora.Date.AddDays(1);
                    while (manana.DayOfWeek == DayOfWeek.Sunday)
                        manana = manana.AddDays(1);
                    dtpFechaReunion.Value = manana;
                    SincronizarHoras(manana.AddHours(HORA_MINIMA), manana.AddHours(HORA_MINIMA).AddMinutes(DURACION_MIN_MIN));
                    CargarInvestigadores(usuarioLogueado.IdSemillero);
                    CargarLugares();
                    return;
                }
                SincronizarHoras(inicioHoy, inicioHoy.AddMinutes(DURACION_MIN_MIN));
            }
            else
            {
                TimeSpan min = new TimeSpan(HORA_MINIMA, 0, 0);
                if (dtpHoraInicioReunion.Value.TimeOfDay < min)
                    SincronizarHoras(
                        dtpFechaReunion.Value.Date.AddHours(HORA_MINIMA),
                        dtpFechaReunion.Value.Date.AddHours(HORA_MINIMA).AddMinutes(DURACION_MIN_MIN));
            }

            // Recargamos listas al final para que reflejen el nuevo horario
            CargarInvestigadores(usuarioLogueado.IdSemillero);
            CargarLugares();
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

            if (dtpFechaReunion.Value.Date == ahora.Date &&
                dtpHoraInicioReunion.Value < ahora.AddHours(1))
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio,
                    "Las reuniones deben agendarse con al menos 1 hora de anticipación.\n" +
                    "Así los participantes tienen tiempo de confirmar su asistencia.");
                return;
            }

            DateTime finPropuesto = dtpHoraInicioReunion.Value.AddMinutes(DURACION_MIN_MIN);
            if (finPropuesto.TimeOfDay > new TimeSpan(HORA_MAXIMA, 0, 0))
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio,
                    $"Con esa hora de inicio la reunión mínima ({DURACION_MIN_MIN} min) superaría las 10:00 p.m.\n" +
                    "Elija una hora de inicio más temprana.");
                return;
            }

            // Hora válida: arrastrar hora fin y recargar listas
            valorAnteriorInicio = dtpHoraInicioReunion.Value;
            cargandoFormulario = true;
            dtpHoraFinalReunion.Value = finPropuesto;
            valorAnteriorFin = finPropuesto;
            cargandoFormulario = false;

            CargarInvestigadores(usuarioLogueado.IdSemillero);
            CargarLugares();
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

            CargarInvestigadores(usuarioLogueado.IdSemillero);
            CargarLugares();
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

        // Métodos que hablan con MongoDB: ID, investigadores disponibles y lugares disponibles.
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

        // Carga solo los investigadores que están libres en el horario seleccionado.
        // Se llama al abrir el form y cada vez que cambia la fecha, hora inicio o hora fin.
        // Los que están ocupados simplemente no aparecen; si el horario cambia y quedan libres,
        // reaparecen. Las reuniones canceladas no cuentan como ocupación.
        private void CargarInvestigadores(int idSemillero)
        {
            try
            {
                // Traer todos los investigadores del semillero
                var filtro = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("idSemillero", idSemillero),
                    Builders<BsonDocument>.Filter.Eq("rolUsuario", "Investigador")
                );
                var todosLosInvestigadores = usuariosCol.Find(filtro).ToList();

                // Guardar los que ya estaban marcados para no perder la selección al recargar
                var marcados = new HashSet<int>();
                foreach (ItemInvestigador item in clbListaInvestigadores.CheckedItems)
                    marcados.Add(item.Id);

                // Calcular quiénes están ocupados en el horario actual
                string fechaStr = dtpFechaReunion.Value.ToString("yyyy-MM-dd");
                TimeSpan tI = dtpHoraInicioReunion.Value.TimeOfDay;
                TimeSpan tF = dtpHoraFinalReunion.Value.TimeOfDay;

                var reunionesDelDia = reunionesCol
                    .Find(Builders<BsonDocument>.Filter.Eq("fechaReunion", fechaStr))
                    .ToList();

                var idsOcupados = new HashSet<int>();
                foreach (var r in reunionesDelDia)
                {
                    if (!r.Contains("investigadoresConvocados")) continue;

                    // Las reuniones canceladas no bloquean el horario de los investigadores
                    if (r.Contains("estadoReunion") && r["estadoReunion"].AsString == "Cancelado") continue;

                    // En modo edición, la reunión que estamos editando no bloquea a nadie
                    if (modoEdicion && reunionAEditar != null &&
                        r["idReunion"].ToInt32() == reunionAEditar["idReunion"].ToInt32()) continue;

                    TimeSpan dbI = TimeSpan.Parse(r["horaInicio"].AsString);
                    TimeSpan dbF = TimeSpan.Parse(r["horaFin"].AsString);

                    if (tI >= dbF || tF <= dbI) continue; // no se solapan

                    foreach (var conv in r["investigadoresConvocados"].AsBsonArray)
                        idsOcupados.Add(conv.AsBsonDocument["idInvestigador"].AsInt32);
                }

                // Reconstruir la lista mostrando solo los disponibles
                clbListaInvestigadores.Items.Clear();
                foreach (var doc in todosLosInvestigadores)
                {
                    int idInv = doc["idUsuario"].AsInt32;
                    if (idsOcupados.Contains(idInv)) continue; // ocupado: no aparece

                    clbListaInvestigadores.Items.Add(
                        new ItemInvestigador { Nombre = doc["nombreUsuario"].AsString, Id = idInv },
                        marcados.Contains(idInv)); // si ya estaba marcado, se mantiene marcado
                }

                if (todosLosInvestigadores.Count > 0 && clbListaInvestigadores.Items.Count == 0)
                    MessageBox.Show(
                        "Todos los investigadores del semillero están ocupados en ese horario.\n" +
                        "Seleccione una fecha u hora diferente.",
                        "Sin investigadores disponibles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else if (todosLosInvestigadores.Count == 0)
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

        // Intenta detectar que el motivo sea texto real y no teclas al azar.
        // Revisa longitud, proporción de vocales, caracteres repetidos y palabras sin sentido.
        // Retorna null si todo está bien, o el mensaje de error si algo falla.
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

        // Los lugares se sacan de las reuniones ya guardadas en BD, excluyendo
        // los que ya están reservados en el horario actual. Las canceladas no bloquean.
        // Si el usuario escribe uno nuevo, el evento Leave verifica si está disponible.
        private void CargarLugares()
        {
            try
            {
                var todasReuniones = reunionesCol.Find(new BsonDocument()).ToList();

                TimeSpan tI = dtpHoraInicioReunion.Value.TimeOfDay;
                TimeSpan tF = dtpHoraFinalReunion.Value.TimeOfDay;
                string fechaActual = dtpFechaReunion.Value.ToString("yyyy-MM-dd");

                // Lugares ocupados en el horario actual (ignorando canceladas)
                var lugaresOcupados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in todasReuniones)
                {
                    if (!r.Contains("lugarReunion")) continue;
                    if (r.Contains("estadoReunion") && r["estadoReunion"].AsString == "Cancelado") continue;
                    if (r["fechaReunion"].AsString != fechaActual) continue;
                    if (modoEdicion && reunionAEditar != null &&
                        r["idReunion"].ToInt32() == reunionAEditar["idReunion"].ToInt32()) continue;

                    TimeSpan dbI = TimeSpan.Parse(r["horaInicio"].AsString);
                    TimeSpan dbF = TimeSpan.Parse(r["horaFin"].AsString);
                    if (tI < dbF && tF > dbI)
                        lugaresOcupados.Add(r["lugarReunion"].AsString.Trim());
                }

                // Todos los lugares conocidos menos los ocupados ahora
                var lugaresUnicos = todasReuniones
                    .Where(d => d.Contains("lugarReunion") && !string.IsNullOrWhiteSpace(d["lugarReunion"].AsString))
                    .Select(d => d["lugarReunion"].AsString.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(l => !lugaresOcupados.Contains(l))
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

        // Busca en la BD si ya hay una reunión activa (no cancelada) en la misma fecha,
        // lugar y horario. Retorna el ID de la que lo ocupa, o false si está libre.
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
                    // Las reuniones canceladas no bloquean el lugar
                    if (doc.Contains("estadoReunion") && doc["estadoReunion"].AsString == "Cancelado") continue;

                    if (modoEdicion && reunionAEditar != null &&
                        doc["idReunion"].ToInt32() == reunionAEditar["idReunion"].ToInt32()) continue;

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

        // Botón principal. Corre todas las validaciones en orden antes de guardar.
        // Si algo falla, corta y avisa sin continuar. Al final, si es nueva reunión
        // limpia el formulario; si es edición, cierra el form.
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

            // ── 2. Motivo ──────────────────────────────────────────────────
            string errorMotivo = ValidarMotivo(txtMotivoReunion.Text);
            if (errorMotivo != null)
            {
                MessageBox.Show(errorMotivo, "Motivo inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMotivoReunion.Focus();
                return;
            }

            // ── 3. Lugar ───────────────────────────────────────────────────
            if (cboLugarReunion.SelectedIndex == -1 && string.IsNullOrWhiteSpace(cboLugarReunion.Text))
            {
                MessageBox.Show(
                    "Debe indicar el lugar donde se realizará la reunión.\n" +
                    "Seleccione una opción de la lista o escriba una nueva.",
                    "Lugar requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLugarReunion.Focus();
                return;
            }

            // ── 4. Duración mínima ─────────────────────────────────────────
            double duracionMin = (dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalMinutes;
            if (duracionMin < DURACION_MIN_MIN)
            {
                MessageBox.Show(
                    $"La reunión debe tener una duración mínima de {DURACION_MIN_MIN} minutos.\n" +
                    "Ajuste el horario de inicio o finalización.",
                    "Duración insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 5. Duración máxima ─────────────────────────────────────────
            if (duracionMin > DURACION_MAX_HRS * 60)
            {
                MessageBox.Show(
                    $"La duración máxima permitida para una reunión es de {DURACION_MAX_HRS} horas.\n" +
                    "Ajuste el horario de finalización.",
                    "Duración excedida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 6. Al menos un participante ────────────────────────────────
            if (clbListaInvestigadores.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    "Debe seleccionar al menos un investigador para convocar a la reunión.",
                    "Sin participantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 7. Disponibilidad del lugar ────────────────────────────────
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
                        { "idReunion",               _idReunionGenerado },
                        { "fechaReunion",            dtpFechaReunion.Value.ToString("yyyy-MM-dd") },
                        { "horaInicio",              dtpHoraInicioReunion.Value.ToString("HH:mm") },
                        { "horaFin",                 dtpHoraFinalReunion.Value.ToString("HH:mm") },
                        { "motivoReunion",           txtMotivoReunion.Text.Trim() },
                        { "lugarReunion",            lugarSeleccionado },
                        { "idLider",                 usuarioLogueado.IdUsuario },
                        { "investigadoresConvocados", invs }
                    };

                    reunionesCol.InsertOne(doc);
                    MessageBox.Show(
                        "La reunión ha sido agendada exitosamente.",
                        "Reunión guardada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error al guardar la reunión:\n\n{ex.Message}",
                    "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // LimpiarFormulario resetea todo al estado inicial después de guardar una reunión.
        // btnCancelar pide confirmación antes de cerrar para no perder datos accidentalmente.
        private void LimpiarFormulario()
        {
            cargandoFormulario = true;
            txtMotivoReunion.Clear();
            cboLugarReunion.SelectedIndex = -1;
            cboLugarReunion.Text = string.Empty;
            lblMensajeError.Text = string.Empty;
            lblMensajeError.ForeColor = Color.Gray;
            btnAgregarReunion.Enabled = true;

            DateTime ahora = DateTime.Now;
            DateTime inicioLimpio = ahora.AddHours(1);
            if (inicioLimpio.TimeOfDay < new TimeSpan(HORA_MINIMA, 0, 0))
                inicioLimpio = ahora.Date.AddHours(HORA_MINIMA);
            else if (inicioLimpio.TimeOfDay >= new TimeSpan(HORA_MAXIMA, 0, 0))
                inicioLimpio = ahora.Date.AddDays(1).AddHours(HORA_MINIMA);

            dtpFechaReunion.Value = inicioLimpio.Date;
            SincronizarHoras(inicioLimpio, inicioLimpio.AddMinutes(DURACION_MIN_MIN));
            cargandoFormulario = false;

            // Recargar con el nuevo horario ya calculado.
            // El CheckedListBox se limpia primero para que nadie quede preseleccionado
            // en la nueva reunión, independientemente de quién se escogió en la anterior.
            clbListaInvestigadores.Items.Clear();
            GenerarIdReunion();
            CargarInvestigadores(usuarioLogueado.IdSemillero);
            CargarLugares();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "¿Está seguro de que desea cancelar?\nLos datos ingresados se perderán.",
                    "Confirmar cancelación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                this.Close();
        }

        // El timer actualiza el label de fecha/hora cada segundo.
        // Los métodos de abajo los generó el diseñador automáticamente; no hacen nada.
        private void ActualizarReloj()
        {
            lblFechaHora.Text = DateTime.Now.ToString("yyyy/MM/dd  HH:mm:ss");
        }
        private void timer1_Tick(object sender, EventArgs e) => ActualizarReloj();

        
        private void dtpHoraFinalReunion_ValueChanged_1(object sender, EventArgs e) { }
        private void dtpHoraInicioReunion_ValueChanged_1(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void dtpFechaReunion_ValueChanged_1(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
    }

    public class ItemInvestigador
    {
        public string Nombre { get; set; }
        public int Id { get; set; }
        public override string ToString() => Nombre;
    }
}