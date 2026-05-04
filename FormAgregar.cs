using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private int _idReunionGenerado; // ID real, no dependemos del TextBox al guardar

        // ── Constantes de reglas de negocio ──────────────────────────────────
        private const int HORA_MINIMA = 6;   // 06:00 a.m.
        private const int HORA_MAXIMA = 22;  // 10:00 p.m.
        private const int DURACION_MIN_MIN = 30;  // 30 minutos mínimo
        private const int DURACION_MAX_HRS = 5;   // 5 horas máximo
        private const int MOTIVO_MIN_CHARS = 10;  // mínimo de caracteres en el motivo
        private const double RATIO_VOCALES_MIN = 0.20; // al menos 20% del texto deben ser vocales

        // Variable para controlar si estamos en modo edición (reutilizando el form) o creando nueva reunión.
        public bool modoEdicion = false;
        public BsonDocument reunionAEditar = null;

        public FormAgregar(DatosUsuario datosRecibidos)
        {
            InitializeComponent();
            this.usuarioLogueado = datosRecibidos;

            dtpFechaReunion.ValueChanged += dtpFechaReunion_ValueChanged;
            dtpHoraInicioReunion.ValueChanged += dtpHoraInicioReunion_ValueChanged;
            dtpHoraFinalReunion.ValueChanged += dtpHoraFinalReunion_ValueChanged;
            cboLugarReunion.Leave += cboLugarReunion_Leave;   // validar al salir del campo
        }

        // ════════════════════════════════════════════════════════════════════
        //  CARGA DEL FORMULARIO
        // ════════════════════════════════════════════════════════════════════
        private void FormAgregar_Load(object sender, EventArgs e)
        {
            timer1.Start();
            ActualizarReloj();
            lblNombreYApellido.Text = usuarioLogueado.Nombre;

            // ── Conexión a BD (en Load para poder mostrar error si falla) ───
            try
            {
                var database = Conexion.ObtenerBaseDatos();
                if (database == null)
                {
                    MessageBox.Show(
                        "No se pudo conectar a la base de datos.\nVerifique la conexión e intente de nuevo.",
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
                    $"Error al conectar con la base de datos:\n{ex.Message}",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // ── Guard: el formulario no sirve sin usuario logueado ──────────
            if (usuarioLogueado == null)
            {
                MessageBox.Show(
                    "No hay una sesión activa. Por favor, inicie sesión nuevamente.",
                    "Sesión inválida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            cargandoFormulario = true;

            // Campos de solo lectura
            txtIdReunion.ReadOnly = true;
            txtLiderResponsable.ReadOnly = true;
            txtIdLider.ReadOnly = true;
            txtIdSemillero.ReadOnly = true;
            txtIdReunion.BackColor = SystemColors.ControlLight;
            txtLiderResponsable.BackColor = SystemColors.ControlLight;

            // Limitar caracteres en el motivo para evitar textos excesivamente largos
            txtMotivoReunion.MaxLength = 200;

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

                // Si estamos en modo edición, cargar los datos de la reunión a editar en los campos correspondientes
                if (modoEdicion && reunionAEditar != null)
                {
                    txtIdReunion.Text = reunionAEditar["idReunion"].ToInt32().ToString(); // ← primera línea del bloque
                    this.Text = "Editar reunión";
                    btnAgregarReunion.Text = "Modificar Reunión";

                    txtMotivoReunion.Text = reunionAEditar["motivoReunion"].AsString;
                    cboLugarReunion.Text = reunionAEditar["lugarReunion"].AsString;

                    if (DateTime.TryParse(reunionAEditar["fechaReunion"].AsString, out DateTime fecha))
                    {
                        dtpFechaReunion.Value = fecha;
                    }

                    if (DateTime.TryParse(reunionAEditar["fechaReunion"].AsString + " " + reunionAEditar["horaInicio"].AsString, out DateTime ini) &&
                        DateTime.TryParse(reunionAEditar["fechaReunion"].AsString + " " + reunionAEditar["horaFin"].AsString, out DateTime fin))
                    {
                        SincronizarHoras(ini, fin);
                    }

                    if (reunionAEditar.Contains("investigadoresConvocados"))
                    {
                        var idsConvocados = new HashSet<int>();

                        foreach (var conv in reunionAEditar["investigadoresConvocados"].AsBsonArray)
                        {
                            idsConvocados.Add(conv["idInvestigador"].ToInt32());
                        }

                        for (int i = 0; i < clbListaInvestigadores.Items.Count; i++)
                        {
                            var inv = (ItemInvestigador)clbListaInvestigadores.Items[i];

                            if (idsConvocados.Contains(inv.Id))
                            {
                                clbListaInvestigadores.SetItemChecked(i, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar el formulario:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { cargandoFormulario = false; }
        }

        // ════════════════════════════════════════════════════════════════════
        //  EVENTOS DE FECHA Y HORA
        // ════════════════════════════════════════════════════════════════════
        private void dtpFechaReunion_ValueChanged(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            // ── VALIDACIÓN 1: No se puede agendar en domingo ────────────────
            if (dtpFechaReunion.Value.DayOfWeek == DayOfWeek.Sunday)
            {
                cargandoFormulario = true;
                MessageBox.Show(
                    "No se puede agendar una reunión el domingo.",
                    "Día no permitido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                // Retrocedemos al día anterior que no sea domingo
                DateTime anteriorValido = dtpFechaReunion.Value.AddDays(-1);
                while (anteriorValido.DayOfWeek == DayOfWeek.Sunday)
                    anteriorValido = anteriorValido.AddDays(-1);

                // Nunca retroceder antes de hoy
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

            // Mínimo 06:00
            if (inicio < new TimeSpan(HORA_MINIMA, 0, 0))
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio, "La hora de inicio mínima es 06:00 a.m.");
                return;
            }

            // No puede ser en el pasado (solo si es hoy)
            if (dtpFechaReunion.Value.Date == ahora.Date && dtpHoraInicioReunion.Value < ahora.AddMinutes(-1))
            {
                RestaurarValor(dtpHoraInicioReunion, ahora, "La hora de inicio ya pasó.");
                return;
            }

            // Debe ser menor a la hora de fin
            if (dtpHoraInicioReunion.Value >= dtpHoraFinalReunion.Value)
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio, "La hora de inicio debe ser menor a la hora de fin.");
                return;
            }

            // ── VALIDACIÓN 4: Duración mínima de 30 minutos ────────────────
            if ((dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalMinutes < DURACION_MIN_MIN)
            {
                RestaurarValor(dtpHoraInicioReunion, valorAnteriorInicio,
                    $"La reunión debe durar al menos {DURACION_MIN_MIN} minutos.");
                return;
            }

            valorAnteriorInicio = dtpHoraInicioReunion.Value;
        }

        private void dtpHoraFinalReunion_ValueChanged(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            // ── VALIDACIÓN 7: Hora máxima 10:00 p.m. ───────────────────────
            if (dtpHoraFinalReunion.Value.TimeOfDay > new TimeSpan(HORA_MAXIMA, 0, 0))
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin,
                    "La hora de fin máxima es las 10:00 p.m.");
                return;
            }

            // Debe ser mayor al inicio
            if (dtpHoraFinalReunion.Value <= dtpHoraInicioReunion.Value)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin,
                    "La hora de fin debe ser posterior al inicio.");
                return;
            }

            // ── VALIDACIÓN 4: Duración mínima ──────────────────────────────
            if ((dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalMinutes < DURACION_MIN_MIN)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin,
                    $"La reunión debe durar al menos {DURACION_MIN_MIN} minutos.");
                return;
            }

            // ── VALIDACIÓN 7: Duración máxima 5 horas ──────────────────────
            if ((dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalHours > DURACION_MAX_HRS)
            {
                RestaurarValor(dtpHoraFinalReunion, valorAnteriorFin,
                    $"La duración máxima de una reunión es de {DURACION_MAX_HRS} horas.");
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
            MessageBox.Show(msg, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                // ── VALIDACIÓN: Investigador con conflicto de horario ───────
                if (ExisteConflictoInvestigador(
                        dtpFechaReunion.Value.Date,
                        dtpHoraInicioReunion.Value.TimeOfDay,
                        dtpHoraFinalReunion.Value.TimeOfDay,
                        new List<int> { inv.Id },
                        out string motivoConflicto))
                {
                    MessageBox.Show(
                        $"❌ {inv.Nombre} ya tiene una reunión en ese horario:\n\"{motivoConflicto}\"\n\nNo se puede agregar.",
                        "Conflicto de horario",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    e.NewValue = CheckState.Unchecked; // desmarca automáticamente
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
                    $"No se pudo generar el ID de reunión:\n{ex.Message}",
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
                        "No hay investigadores registrados en su semillero.",
                        "Sin investigadores", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar investigadores:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  VALIDACIÓN DE MOTIVO (detecta texto sin sentido)
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Retorna null si el motivo es válido, o un mensaje de error si no lo es.
        /// Detecta: texto muy corto, sin vocales, secuencias repetitivas de teclas,
        /// y palabras demasiado largas sin sentido.
        /// </summary>
        private string ValidarMotivo(string motivo)
        {
            string texto = motivo.Trim();

            // 1. Longitud mínima
            if (texto.Length < MOTIVO_MIN_CHARS)
                return $"El motivo debe tener al menos {MOTIVO_MIN_CHARS} caracteres.";

            string textoLower = texto.ToLower();

            // 2. Ratio de vocales: texto real siempre tiene vocales
            int vocales = textoLower.Count(c => "aeiouáéíóúü".Contains(c));
            double ratioVocales = (double)vocales / texto.Length;
            if (ratioVocales < RATIO_VOCALES_MIN)
                return "El motivo no parece tener sentido. Por favor describa el propósito de la reunión.";

            // 3. Secuencias de caracteres repetidos excesivos (ej: "aaaaaaa", "jjjjjj")
            int maxRepetidos = 0, contRepetidos = 1;
            for (int i = 1; i < textoLower.Length; i++)
            {
                if (textoLower[i] == textoLower[i - 1]) contRepetidos++;
                else contRepetidos = 1;
                if (contRepetidos > maxRepetidos) maxRepetidos = contRepetidos;
            }
            if (maxRepetidos >= 4)
                return "El motivo contiene caracteres repetidos excesivamente. Sea más descriptivo.";

            // 4. Palabras demasiado largas sin espacios (ej: "ahjsahsjahjs")
            var palabras = texto.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var palabra in palabras)
            {
                // Palabra de más de 15 letras seguidas sin sentido de vocal/consonante
                if (palabra.Length > 15)
                    return "El motivo contiene palabras demasiado largas. Use un texto descriptivo normal.";

                // Si la palabra tiene más de 6 letras y menos del 15% son vocales, es garba
                if (palabra.Length > 6)
                {
                    int vPalabra = palabra.ToLower().Count(c => "aeiouáéíóúü".Contains(c));
                    if ((double)vPalabra / palabra.Length < 0.15)
                        return $"La palabra \"{palabra}\" no parece válida. Escriba un motivo real.";
                }
            }

            // 5. Que haya al menos 2 palabras (no solo una tecla repetida)
            if (palabras.Length < 2)
                return "El motivo debe contener al menos dos palabras.";

            return null; // todo bien
        }

        /// <summary>
        /// Lee todos los lugarReunion únicos que ya existen en la colección
        /// Reuniones y los carga en el ComboBox, ordenados alfabéticamente.
        /// </summary>
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
                    $"Error al cargar los lugares:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Si el lugar escrito no está en el ComboBox todavía, lo agrega en memoria
        /// para esta sesión. La próxima vez que abra el form aparecerá solo,
        /// porque ya quedó guardado en Reuniones cuando se guardó la reunión.
        /// </summary>
        private void AgregarLugarAlComboSiEsNuevo(string lugar)
        {
            if (string.IsNullOrWhiteSpace(lugar)) return;

            bool yaEsta = cboLugarReunion.Items
                .Cast<string>()
                .Any(l => l.Equals(lugar, StringComparison.OrdinalIgnoreCase));

            if (!yaEsta)
                cboLugarReunion.Items.Add(lugar);
        }

        /// <summary>
        /// Al salir del ComboBox: valida disponibilidad del lugar inmediatamente
        /// y guarda el lugar nuevo si no existía.
        /// </summary>
        private void cboLugarReunion_Leave(object sender, EventArgs e)
        {
            if (cargandoFormulario) return;

            string lugar = cboLugarReunion.Text.Trim();
            if (string.IsNullOrWhiteSpace(lugar)) return;

            // ── Validar disponibilidad al instante ──────────────────────────
            if (ExisteConflictoLugar(
                    dtpFechaReunion.Value.Date,
                    dtpHoraInicioReunion.Value.TimeOfDay,
                    dtpHoraFinalReunion.Value.TimeOfDay,
                    lugar,
                    out string motivoConflicto))
            {
                MessageBox.Show(
                    $"❌ El lugar \"{lugar}\" ya está ocupado en ese horario\npor la reunión: \"{motivoConflicto}\".\n\nSeleccione otro lugar u otro horario.",
                    "Lugar no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                cboLugarReunion.Text = string.Empty;
                cboLugarReunion.Focus();
                return;
            }

            // ── Si pasó la validación, agregar al combo si es lugar nuevo ───
            AgregarLugarAlComboSiEsNuevo(lugar);
        }

        private bool ExisteConflictoInvestigador(DateTime fecha, TimeSpan tI, TimeSpan tF,
                                                  List<int> ids, out string motivoConflicto)
        {
            motivoConflicto = string.Empty;
            try
            {
                var filtro = Builders<BsonDocument>.Filter.Eq("fechaReunion", fecha.ToString("yyyy-MM-dd"));
                var reuniones = reunionesCol.Find(filtro).ToList();

                foreach (var doc in reuniones)
                {
                    if (!doc.Contains("investigadoresConvocados")) // Si la reunión no tiene investigadores convocados, no hay conflicto
                    {
                        continue;
                    }

                    // Extraemos solo los IDs de los investigadores convocados para esta reunión
                    var idsDB = doc["investigadoresConvocados"]
                                    .AsBsonArray
                                    .Select(x => x.AsBsonDocument["idInvestigador"].AsInt32);

                    // Excluimos la reunión actual para que no choque consigo misma
                    if (modoEdicion && reunionAEditar != null &&
                        doc["idReunion"].ToInt32() == reunionAEditar["idReunion"].ToInt32())
                    {
                        continue;
                    }

                    if (ids.Any(id => idsDB.Contains(id)))
                    {
                        TimeSpan dbI = TimeSpan.Parse(doc["horaInicio"].AsString);
                        TimeSpan dbF = TimeSpan.Parse(doc["horaFin"].AsString);

                        if (tI < dbF && tF > dbI)
                        {
                            motivoConflicto = doc["motivoReunion"].AsString;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al verificar conflictos de investigadores:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private bool ExisteConflictoLugar(DateTime fecha, TimeSpan tI, TimeSpan tF,
                                           string lugar, out string motivoConflicto)
        {
            motivoConflicto = string.Empty;
            try
            {
                var filtro = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("fechaReunion", fecha.ToString("yyyy-MM-dd")),
                    Builders<BsonDocument>.Filter.Eq("lugarReunion", lugar)
                );
                var reuniones = reunionesCol.Find(filtro).ToList();

                foreach (var doc in reuniones)
                {
                    TimeSpan dbI = TimeSpan.Parse(doc["horaInicio"].AsString);
                    TimeSpan dbF = TimeSpan.Parse(doc["horaFin"].AsString);

                    // Excluimos la reunión actual para que no choque consigo misma
                    if (modoEdicion && reunionAEditar != null &&
                        doc["idReunion"].ToInt32() == reunionAEditar["idReunion"].ToInt32())
                    {
                        continue;
                    }

                    if (tI < dbF && tF > dbI)
                    {
                        motivoConflicto = doc["motivoReunion"].AsString;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al verificar disponibilidad del lugar:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        // ════════════════════════════════════════════════════════════════════
        //  LIMPIAR / CANCELAR
        // ════════════════════════════════════════════════════════════════════
        private void LimpiarFormulario()
        {
            cargandoFormulario = true;
            txtMotivoReunion.Clear();
            cboLugarReunion.SelectedIndex = -1;
            DateTime ahora = DateTime.Now;
            dtpFechaReunion.Value = ahora.Date;
            SincronizarHoras(ahora, ahora.AddMinutes(DURACION_MIN_MIN));
            for (int i = 0; i < clbListaInvestigadores.Items.Count; i++)
                clbListaInvestigadores.SetItemChecked(i, false);
            GenerarIdReunion(); // actualiza _idReunionGenerado y el TextBox
            cargandoFormulario = false;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show(
                "¿Está seguro de que desea cancelar? Se perderán los datos ingresados.",
                "Confirmar Cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (r == DialogResult.Yes)
                this.Close();
        }


        // ════════════════════════════════════════════════════════════════════
        //  GUARDAR REUNIÓN
        // ════════════════════════════════════════════════════════════════════
        private void btnAgregarReunion_Click(object sender, EventArgs e)
        {
            // ── 1. Validar día domingo ──────────────────────────────────────
            if (dtpFechaReunion.Value.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show("No se puede agendar una reunión el domingo.",
                    "Día no permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 2. Motivo: mínimo de caracteres ────────────────────────────
            if (string.IsNullOrWhiteSpace(txtMotivoReunion.Text) ||
                txtMotivoReunion.Text.Trim().Length < MOTIVO_MIN_CHARS)
            {
                MessageBox.Show(
                    $"El motivo de la reunión debe tener al menos {MOTIVO_MIN_CHARS} caracteres.",
                    "Datos insuficientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMotivoReunion.Focus();
                return;
            }

            // ── 3. Lugar seleccionado ───────────────────────────────────────
            if (cboLugarReunion.SelectedIndex == -1 && string.IsNullOrWhiteSpace(cboLugarReunion.Text))
            {
                MessageBox.Show("Por favor, seleccione o ingrese un lugar para la reunión.",
                    "Datos Faltantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLugarReunion.Focus();
                return;
            }

            // ── 4. Duración mínima 30 minutos ──────────────────────────────
            double duracionMin = (dtpHoraFinalReunion.Value - dtpHoraInicioReunion.Value).TotalMinutes;
            if (duracionMin < DURACION_MIN_MIN)
            {
                MessageBox.Show($"La reunión debe tener una duración mínima de {DURACION_MIN_MIN} minutos.",
                    "Duración inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 5. Duración máxima 5 horas ─────────────────────────────────
            if (duracionMin > DURACION_MAX_HRS * 60)
            {
                MessageBox.Show($"La duración máxima de una reunión es de {DURACION_MAX_HRS} horas.",
                    "Duración inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 6. Al menos un investigador ────────────────────────────────
            if (clbListaInvestigadores.CheckedItems.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos un investigador para la reunión.",
                    "Sin Participantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 7. Disponibilidad del lugar ────────────────────────────────
            string lugarSeleccionado = cboLugarReunion.Text.Trim();
            if (ExisteConflictoLugar(
                    dtpFechaReunion.Value.Date,
                    dtpHoraInicioReunion.Value.TimeOfDay,
                    dtpHoraFinalReunion.Value.TimeOfDay,
                    lugarSeleccionado,
                    out string motivoLugar))
            {
                MessageBox.Show(
                    $"❌ El lugar \"{lugarSeleccionado}\" ya está ocupado en ese horario por la reunión:\n\"{motivoLugar}\"",
                    "Lugar no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLugarReunion.Focus();
                return;
            }

            // ── 8. Re-verificar conflictos de todos los investigadores seleccionados ─
            //    (por si el horario cambió después de marcarlos)
            var idsSeleccionados = clbListaInvestigadores.CheckedItems
                                        .Cast<ItemInvestigador>()
                                        .Select(i => i.Id)
                                        .ToList();

            if (ExisteConflictoInvestigador(
                    dtpFechaReunion.Value.Date,
                    dtpHoraInicioReunion.Value.TimeOfDay,
                    dtpHoraFinalReunion.Value.TimeOfDay,
                    idsSeleccionados,
                    out string motivoInv))
            {
                MessageBox.Show(
                    $"❌ Uno o más investigadores seleccionados tienen conflicto de horario con:\n\"{motivoInv}\"\nRevise la lista.",
                    "Conflicto de horario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── Confirmación ───────────────────────────────────────────────
            string mensajeConfirmacion;
            string tituloConfirmacion;

            if (modoEdicion)
            {
                mensajeConfirmacion = "¿Está seguro de que desea guardar los cambios en esta reunión?";
                tituloConfirmacion = "Confirmar Edición";
            }
            else
            {
                mensajeConfirmacion = "¿Está seguro de que desea agendar esta reunión?";
                tituloConfirmacion = "Confirmar Guardado";
            }

            DialogResult resultado = MessageBox.Show(mensajeConfirmacion, tituloConfirmacion,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado != DialogResult.Yes) return;

            try
            {
                BsonArray invs = new BsonArray();
                foreach (ItemInvestigador investigador in clbListaInvestigadores.CheckedItems)
                {
                    invs.Add(new BsonDocument {
                        { "idInvestigador", investigador.Id },
                        { "asistencia", "pendiente" }
                    });
                }

                var doc = new BsonDocument {
                    { "idReunion",        _idReunionGenerado },
                    { "fechaReunion",    dtpFechaReunion.Value.ToString("yyyy-MM-dd") },
                    { "horaInicio",      dtpHoraInicioReunion.Value.ToString("HH:mm") },
                    { "horaFin",         dtpHoraFinalReunion.Value.ToString("HH:mm") },
                    { "motivoReunion",   txtMotivoReunion.Text.Trim() },
                    { "lugarReunion",    lugarSeleccionado },
                    { "idLider",         usuarioLogueado.IdUsuario },
                    { "idInvestigadores", invs }
                };

                // Si estamos editando, actualizamos el documento existente. Si es nuevo, insertamos uno nuevo.
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
                    MessageBox.Show("Reunión actualizada exitosamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    reunionesCol.InsertOne(doc);
                    MessageBox.Show("La reunión ha sido guardada exitosamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarReloj()
        {
            lblFechaHora.Text = DateTime.Now.ToString("yyyy/MM/dd  HH:mm:ss");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            ActualizarReloj();
        }
        private void dtpHoraFinalReunion_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void dtpHoraInicioReunion_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dtpFechaReunion_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }

        
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