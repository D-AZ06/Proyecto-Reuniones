using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_Reuniones
{
    public partial class FormPrincipal : Form
    {
        private DatosUsuario datosUsuario;
        private Control controlActual;

        // ══════════════════════════════════════════════════════════════
        //  1. INICIALIZACIÓN DEL FORMULARIO Y CONFIGURACIÓN DE LA INTERFAZ
        // ═══════════════════════════════════════════════════════════

        public FormPrincipal(DatosUsuario datos)
        {
            InitializeComponent();
            datosUsuario = datos;
            ConfigurarInterfaz();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            timer1.Start();
            ActualizarReloj();
            lblNombreYApellido.Text = datosUsuario.Nombre;
            lblRol.Text = datosUsuario.Rol;

            if (datosUsuario.Rol == "Investigador")
            {
                _ = DetectarYMarcarConflictos();
            }

            // 1. Estilo General (Fondo Blanco)
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.GridColor = Color.FromArgb(230, 230, 230); // Gris muy tenue para las líneas

            // 2. Cabecera (Azul Profesional - Basado en tus botones de "Consultar")
            // Este es el azul que se ve en la parte superior de tu captura
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(70, 130, 180);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 130, 180);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial Rounded MT", 8, FontStyle.Bold);
            dataGridView1.ColumnHeadersHeight = 35;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // 3. Filas (Ajuste de Blancos y Grises)
            // Esto asegura que la fila 306 sea BLANCA
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            // Azul muy clarito para cuando hagas clic en una fila
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 230, 245);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT", 8);

            // 4. Filas Alternas (Gris muy suave para lectura)
            // Si quieres que la fila 306 sea blanca, la 307 será de este color gris:
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // 5. Limpieza de Interfaz
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // IMPORTANTE: Pon esto después de cargar tus datos para que la 306 no se vea azul al inicio
            dataGridView1.ClearSelection();
        }

        private void ConfigurarInterfaz()
        {
            MessageBox.Show($"Bienvenido, {datosUsuario.Nombre} ({datosUsuario.Rol})", "Bienvenida", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Opciones del combo de filtro de búsqueda
            var opciones = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("", ""),
                new KeyValuePair<string, string>("ID reunión", "idReunion"),
                new KeyValuePair<string, string>("Fecha", "fechaReunion"),
                new KeyValuePair<string, string>("Hora inicio", "horaInicio"),
                new KeyValuePair<string, string>("Hora fin", "horaFin"),
                new KeyValuePair<string, string>("Motivo", "motivoReunion"),
                new KeyValuePair<string, string>("Lugar", "lugarReunion"),
                new KeyValuePair<string, string>("Nombre investigador", "investigadoresConvocados"),
                new KeyValuePair<string, string>("Estado de la reunión", "estadoReunion") // NUEVO
            };

            if (datosUsuario.Rol == "Investigador")
            {
                // El investigador NO puede crear/editar/eliminar/reportar
                btnAgregarReunión.Enabled = false; btnAgregarReunión.Visible = false;
                btnReporte.Enabled = false; btnReporte.Visible = false;
                btnEliminarReunion.Enabled = false; btnEliminarReunion.Visible = false;
                btnModificarReunion.Enabled = false; btnModificarReunion.Visible = false;

                //pictureBox4.Visible = false;
                pictureBox6.Visible = false;
                pictureBox8.Visible = false;


                // Pero sí puede filtrar por su asistencia
                opciones.Add(new KeyValuePair<string, string>("Mi Asistencia", "asistenciaReunion")); // NUEVO
            }
            else
            {
                btnConfirmarAsistencia.Visible = false; btnConfirmarAsistencia.Enabled = false;
                icono_asistencia.Visible = false;
            }

            cboFiltro.DataSource = null; // Limpiamos por si acaso
            cboFiltro.DataSource = opciones;
            cboFiltro.DisplayMember = "Key";
            cboFiltro.ValueMember = "Value";
        }

        // ══════════════════════════════════════════════════════════════
        //  2. HELPERS DE NEGOCIO
        //     Métodos pequeños y reutilizables que encapsulan reglas del dominio.
        // ═══════════════════════════════════════════════════════════


        // Devuelve el filtro de MongoDB que corresponde al rol del usuario actual:
        // Líder  → reuniones donde él es el líder.
        // Investigador → reuniones donde está en la lista de convocados.
        private FilterDefinition<BsonDocument> ObtenerFiltroPorRol()
        {
            int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);

            if (datosUsuario.Rol == "Líder")
            {
                return Builders<BsonDocument>.Filter.Eq("idLider", idUsuario);
            }

            return Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                "investigadoresConvocados",
                new BsonDocument("idInvestigador", idUsuario)
            );
        }


        // Indica si dos reuniones se chocan en horario (mismo día + rangos de hora intersectados).
        private bool HayConflictoDeHorario(BsonDocument r1, BsonDocument r2)
        {
            if (r1["fechaReunion"].AsString != r2["fechaReunion"].AsString)
                return false;

            bool ok1 = DateTime.TryParse(r1["fechaReunion"].AsString + " " + r1["horaInicio"].AsString, out DateTime ini1);
            bool ok2 = DateTime.TryParse(r1["fechaReunion"].AsString + " " + r1["horaFin"].AsString, out DateTime fin1);
            bool ok3 = DateTime.TryParse(r2["fechaReunion"].AsString + " " + r2["horaInicio"].AsString, out DateTime ini2);
            bool ok4 = DateTime.TryParse(r2["fechaReunion"].AsString + " " + r2["horaFin"].AsString, out DateTime fin2);

            if (!ok1 || !ok2 || !ok3 || !ok4) return false;

            // choque: ini1 < fin2  &&  ini2 < fin1
            return ini1 < fin2 && ini2 < fin1;
        }

        // Metodo que devuelve el estado de asistencia de un investigador en una reunión ("pendiente", "confirmado", etc.).
        // Retorna string vacío si no está en la lista.
        private string ObtenerAsistencia(BsonDocument reunion, int idInvestigador)
        {
            if (!reunion.Contains("investigadoresConvocados"))
                return "";

            foreach (var conv in reunion["investigadoresConvocados"].AsBsonArray)
            {
                if (conv["idInvestigador"].ToInt32() == idInvestigador)
                    return conv["asistencia"].AsString;
            }

            return "";
        }

        // Calcula el estado de una reunión en función de la hora actual:
        // "Programadas" | "En ejecución" | "Finalizadas" | "Desconocido"
        private string ObtenerEstadoReunion(BsonDocument reunion)
        {
            bool inicioOk = DateTime.TryParse(reunion["fechaReunion"].AsString + " " + reunion["horaInicio"].AsString, out DateTime inicio);
            bool finOk = DateTime.TryParse(reunion["fechaReunion"].AsString + " " + reunion["horaFin"].AsString, out DateTime fin);

            if (!inicioOk || !finOk) return "Desconocido";

            DateTime ahora = DateTime.Now;

            if (ahora < inicio) return "Programadas";
            if (ahora >= inicio && ahora <= fin) return "En ejecución";
            return "Finalizadas";
        }

        // Valida que una reunión pueda ser editada o eliminada según su estado.
        // Muestra el mensaje de error apropiado y devuelve true si la operación debe bloquearse.
        private bool ReunionNoPuedeModificarse(string estadoReunion, string accion)
        {
            if (estadoReunion == "En ejecución")
            {
                MessageBox.Show($"No puedes {accion} una reunión que está en curso.", "No permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }

            if (estadoReunion == "Finalizadas")
            {
                MessageBox.Show($"No puedes {accion} una reunión que ya finalizó.", "No permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }

            return false;
        }

        // Consulta MongoDB y devuelve true si al menos un investigador ya confirmó asistencia
        // en la reunión indicada. Muestra mensaje de error si es el caso.
        private async Task<bool> TieneInvestigadoresConfirmados(int idReunion, string accion)
        {
            var colReuniones = Conexion.ObtenerBaseDatos().GetCollection<BsonDocument>("Reuniones");
            var filtro = Builders<BsonDocument>.Filter.Eq("idReunion", idReunion);
            var reunion = await colReuniones.Find(filtro).FirstOrDefaultAsync();

            if (reunion != null && reunion.Contains("investigadoresConvocados"))
            {
                foreach (var conv in reunion["investigadoresConvocados"].AsBsonArray)
                {
                    if (conv["asistencia"].AsString == "confirmado")
                    {
                        MessageBox.Show(
                            $"No puedes {accion} esta reunión, uno o más investigadores ya confirmaron asistencia.",
                            "No permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return true;
                    }
                }
            }

            return false;
        }

        //══════════════════════════════════════════════════════════════
        //  3. LÓGICA DE CONFLICTOS DE HORARIO
        //══════════════════════════════════════════════════════════════

        // Detecta reuniones con conflicto de horario para el investigador actual
        // y les actualiza el estado de asistencia a "conflicto" en la BD.
        // Solo actúa sobre reuniones en estado "pendiente" que no hayan finalizado.
        private async Task DetectarYMarcarConflictos()
        {
            try
            {
                var db = Conexion.ObtenerBaseDatos();
                var colReuniones = db.GetCollection<BsonDocument>("Reuniones");
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);

                // Traer todas las reuniones donde participa este investigador
                var filtroParticipante = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                    "investigadoresConvocados",
                    new BsonDocument("idInvestigador", idUsuario)
                );
                var todasLasReuniones = await colReuniones.Find(filtroParticipante).ToListAsync();

                // Solo comparamos reuniones activas (no finalizadas)
                var reunionesActivas = todasLasReuniones
                    .Where(r => ObtenerEstadoReunion(r) != "Finalizadas")
                    .ToList();

                foreach (var r1 in reunionesActivas)
                {
                    // Solo marcamos conflicto si el estado actual es "pendiente"
                    if (ObtenerAsistencia(r1, idUsuario) != "pendiente") continue;

                    bool hayConflicto = reunionesActivas.Any(r2 =>
                        r2["idReunion"].ToInt32() != r1["idReunion"].ToInt32() &&
                        HayConflictoDeHorario(r1, r2)
                    );

                    if (!hayConflicto) continue;

                    var filtroUpdate = Builders<BsonDocument>.Filter.And(
                        Builders<BsonDocument>.Filter.Eq("idReunion", r1["idReunion"].ToInt32()),
                        Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                            "investigadoresConvocados",
                            new BsonDocument("idInvestigador", idUsuario)
                        )
                    );
                    var update = Builders<BsonDocument>.Update.Set("investigadoresConvocados.$.asistencia", "conflicto");
                    await colReuniones.UpdateOneAsync(filtroUpdate, update);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detectar conflictos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //══════════════════════════════════════════════════════════════
        //  4. CONSULTA Y DATOS (MongoDB)
        //══════════════════════════════════════════════════════════════

        // Trae reuniones de MongoDB según el filtro dado y resuelve los nombres
        // de todos los usuarios (líderes e investigadores) que aparecen en ellas.
        private async Task<(List<BsonDocument> reuniones, Dictionary<int, string> nombrePorId)>
            ObtenerReunionesYUsuarios(FilterDefinition<BsonDocument> filtroPrincipal)
        {
            var db = Conexion.ObtenerBaseDatos();
            var colReuniones = db.GetCollection<BsonDocument>("Reuniones");
            var colUsuarios = db.GetCollection<BsonDocument>("Usuarios");

            var reuniones = await colReuniones.Find(filtroPrincipal).ToListAsync();

            // Recolectar todos los IDs de usuario que aparecen en las reuniones
            var ids = new HashSet<int>();
            foreach (var r in reuniones)
            {
                ids.Add(r["idLider"].ToInt32());
                if (r.Contains("investigadoresConvocados"))
                {
                    foreach (var conv in r["investigadoresConvocados"].AsBsonArray)
                    {
                        ids.Add(conv["idInvestigador"].ToInt32());
                    } 
                }   
            }

            // Resolver nombres en una sola consulta
            var nombrePorId = new Dictionary<int, string>();
            if (ids.Count > 0)
            {
                var filtroUsuarios = Builders<BsonDocument>.Filter.In("idUsuario", ids);
                var usuarios = await colUsuarios.Find(filtroUsuarios).ToListAsync();
                foreach (var u in usuarios)
                {
                    nombrePorId[u["idUsuario"].ToInt32()] = u["nombreUsuario"].AsString;
                }
            }

            return (reuniones, nombrePorId);
        }

        // Obtiene los valores distintos del campo indicado para poblar el combo de filtro dinámico.
        private async Task<List<string>> ObtenerOpcionesDesdeBD(string campo)
        {
            var db = Conexion.ObtenerBaseDatos();
            var colReuniones = db.GetCollection<BsonDocument>("Reuniones");
            var lista = new List<string>();

            var reunionesDelUsuario = await colReuniones.Find(ObtenerFiltroPorRol()).ToListAsync();

            if (campo == "idReunion")
            {
                foreach (var r in reunionesDelUsuario)
                {
                    lista.Add(r["idReunion"].ToInt32().ToString());
                }
                    
            }
            else if (campo == "lugarReunion")
            {
                foreach (var r in reunionesDelUsuario)
                {
                    lista.Add(r["lugarReunion"].AsString);
                }

            }
            else if (campo == "investigadoresConvocados")
            {
                var ids = new HashSet<int>();
                foreach (var r in reunionesDelUsuario)
                {
                    if (r.Contains("investigadoresConvocados"))
                    {
                        foreach (var conv in r["investigadoresConvocados"].AsBsonArray)
                        {
                            ids.Add(conv["idInvestigador"].ToInt32());
                        }
                    }
                }

                var colUsuarios = db.GetCollection<BsonDocument>("Usuarios");
                var filtroUsuarios = Builders<BsonDocument>.Filter.In("idUsuario", ids);
                var usuarios = await colUsuarios.Find(filtroUsuarios).ToListAsync();
                foreach (var u in usuarios)
                {
                    lista.Add(u["nombreUsuario"].AsString);
                }
            }

            return lista.Distinct().OrderBy(x => x).ToList();
        }

        // Recarga el DataGrid respetando los filtros actuales de estado y asistencia.
        private async Task RecargarGrid()
        {
            int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);

            var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(ObtenerFiltroPorRol());

            if (reuniones.Count == 0)
            {
                dataGridView1.DataSource = null;
            }
            else
            {
                dataGridView1.DataSource = ConstruirTabla(reuniones, nombrePorId);
            }

            if (reuniones.Count > 0) DiseñarGrid();
        }

        //══════════════════════════════════════════════════════════════
        //  5. CONSTRUCCIÓN Y DISEÑO DEL GRID
        //══════════════════════════════════════════════════════════════

        // Construye el DataTable con las columnas y filas que se muestran en el DataGridView.
        private DataTable ConstruirTabla(List<BsonDocument> reuniones, Dictionary<int, string> nombrePorId)
        {
            int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
            bool esInvestigador = datosUsuario.Rol == "Investigador";

            var tabla = new DataTable();
            tabla.Columns.Add("Cód.", typeof(int));
            tabla.Columns.Add("Fecha", typeof(string));
            tabla.Columns.Add("Inicio", typeof(string));
            tabla.Columns.Add("Fin", typeof(string));
            tabla.Columns.Add("Motivo", typeof(string));
            tabla.Columns.Add("Lugar", typeof(string));
            tabla.Columns.Add("Líder", typeof(string));
            tabla.Columns.Add("Asistentes", typeof(string));
            tabla.Columns.Add("Estado", typeof(string));

            if (esInvestigador)
            {
                tabla.Columns.Add("Mi asistencia", typeof(string));
            }

            foreach (var r in reuniones)
            {
                int idLider = r["idLider"].ToInt32();
                string nombreLider = nombrePorId.TryGetValue(idLider, out var nl) ? nl : "ID " + idLider;
                string asistentes = "";
                string miAsistencia = "";

                if (r.Contains("investigadoresConvocados"))
                {
                    foreach (var conv in r["investigadoresConvocados"].AsBsonArray)
                    {
                        int idConv = conv["idInvestigador"].ToInt32();
                        string estadoAsist = conv["asistencia"].AsString;
                        string nombreConv;

                        if (nombrePorId.ContainsKey(idConv))
                        {
                            nombreConv = nombrePorId[idConv];
                        }
                        else
                        {
                            nombreConv = "ID " + idConv;
                        }

                        if (asistentes != "")
                        {
                            asistentes += Environment.NewLine;
                        }

                        if (datosUsuario.Rol == "Líder")
                        {
                            asistentes += "• " + nombreConv + " (" + estadoAsist + ")";
                        }
                        else
                        {
                            asistentes += "• " + nombreConv;
                        }

                        if (idConv == idUsuario)
                        {
                            miAsistencia = estadoAsist; // AJUSTE 2: reemplaza el bloque que había aquí
                        }
                    }
                }

                string estado = ObtenerEstadoReunion(r);

                if (esInvestigador)
                {
                    tabla.Rows.Add(
                        r["idReunion"].ToInt32(), r["fechaReunion"].AsString,
                        r["horaInicio"].AsString, r["horaFin"].AsString,
                        r["motivoReunion"].AsString, r["lugarReunion"].AsString,
                        nombreLider, asistentes, estado, miAsistencia
                    );
                }
                else
                {
                    tabla.Rows.Add(
                        r["idReunion"].ToInt32(), r["fechaReunion"].AsString,
                        r["horaInicio"].AsString, r["horaFin"].AsString,
                        r["motivoReunion"].AsString, r["lugarReunion"].AsString,
                        nombreLider, asistentes, estado
                    );
                }
            }

            return tabla;
        }

        // Aplica estilos visuales al DataGridView después de asignarle datos.
        private void DiseñarGrid()
        {
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ScrollBars = ScrollBars.Both;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSteelBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);


        }

        private List<BsonDocument> FiltrarPorEstado(List<BsonDocument> reuniones, string estado)
        {
            if (estado == "Todas") return reuniones;

            return reuniones.Where(r => ObtenerEstadoReunion(r) == estado).ToList();
        }

        /// Filtra la lista de reuniones por el estado de asistencia del investigador actual.
        /// "Todas" devuelve la lista sin filtrar.
        private List<BsonDocument> FiltrarPorAsistencia(List<BsonDocument> reuniones, string asistencia, int idUsuario)
        {
            if (asistencia == "Todas") return reuniones;

            return reuniones.Where(r => ObtenerAsistencia(r, idUsuario) == asistencia).ToList();
        }

        //══════════════════════════════════════════════════════════════
        //  6. FILTRO DINÁMICO (combo de búsqueda)
        //══════════════════════════════════════════════════════════════

        private async void cboFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelFiltro.Controls.Clear();
            controlActual = null;

            string campo = cboFiltro.SelectedValue.ToString();

            if (campo == "idReunion" || campo == "lugarReunion" || campo == "investigadoresConvocados")
            {
                ComboBox cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                var opciones = await ObtenerOpcionesDesdeBD(campo);
                foreach (var op in opciones) cbo.Items.Add(op);
                controlActual = cbo;
            }
            else if (campo == "fechaReunion")
            {
                controlActual = new DateTimePicker { Format = DateTimePickerFormat.Short };
            }
            else if (campo == "horaInicio" || campo == "horaFin")
            {
                controlActual = new DateTimePicker
                {
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true
                };
            }
            else if (campo == "motivoReunion")
            {
                controlActual = new TextBox();
            }
            else if (campo == "estadoReunion")
            {
                ComboBox cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                cbo.Items.AddRange(new[] { "Todas", "Programadas", "En ejecución", "Finalizadas", "Desconocido" });
                cbo.SelectedIndex = 0;
                controlActual = cbo;
            }
            else if (campo == "asistenciaReunion")
            {
                ComboBox cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                cbo.Items.AddRange(new[] { "Todas", "pendiente", "confirmado", "rechazado", "conflicto" });
                cbo.SelectedIndex = 0;
                controlActual = cbo;
            }
            else
            {
                panelFiltro.BorderStyle = BorderStyle.FixedSingle;
                return;
            }

            controlActual.Dock = DockStyle.Fill;
            panelFiltro.Controls.Add(controlActual);
            panelFiltro.BorderStyle = BorderStyle.None;
        }

        // Lee el valor del control dinámico que se usa para filtrar búsquedas.
        private string ObtenerValorDelControl()
        {
            if (controlActual is TextBox txt)
            {
                return txt.Text;
            }

            // Si es un ComboBox
            if (controlActual is ComboBox cbo)
            {
                return cbo.Text;
            }

            // Si es un DateTimePicker, verificamos el formato
            if (controlActual is DateTimePicker dtp)
            {
                if (dtp.Format == DateTimePickerFormat.Short)
                {
                    return dtp.Value.ToString("yyyy-MM-dd");
                }
                else
                {
                    return dtp.Value.ToString("HH:mm");
                }
            }

            // Si no es ninguno de los anteriores
            return "";
        }

        //══════════════════════════════════════════════════════════════
        //  7. BOTONES DE ACCIÓN
        //══════════════════════════════════════════════════════════════

        // ── Ver reuniones ────────────────────────────────────────────────────
        private async void btnVerReunion_Click(object sender, EventArgs e)
        {
            cboFiltro.SelectedIndex = 0;
            if (panelFiltro.Controls.Count > 0) panelFiltro.Controls.Clear();

            await RecargarGrid();

            try
            {
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);

                var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(ObtenerFiltroPorRol());

                if (reuniones.Count == 0)
                {
                    string mensaje;

                    if (datosUsuario.Rol == "Líder")
                    {
                        mensaje = "No hay reuniones en tu semillero.";
                    }
                    else
                    {
                        mensaje = "No estás en ninguna reunión.";
                    }

                    MessageBox.Show(mensaje, "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dataGridView1.DataSource = ConstruirTabla(reuniones, nombrePorId);
                DiseñarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Consultar con parámetros ─────────────────────────────────────────
        private async void btn_Consultar_con_parametros_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;

            try
            {
                // 1. Validaciones iniciales
                if (cboFiltro.SelectedIndex <= 0)
                {
                    MessageBox.Show("Selecciona un campo para filtrar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string valorFiltro = ObtenerValorDelControl().Trim();
                if (string.IsNullOrEmpty(valorFiltro))
                {
                    MessageBox.Show("Ingresa un valor para buscar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
                string campo = cboFiltro.SelectedValue.ToString();

                // 2. LÓGICA ESPECIAL PARA CAMPOS CALCULADOS (Estado y Asistencia)
                // Como estos no están en la BD o requieren lógica interna, filtramos en memoria.
                if (campo == "estadoReunion" || campo == "asistenciaReunion")
                {
                    // Traemos TODAS las reuniones del usuario según su rol
                    var (reunionesBase, nombres) = await ObtenerReunionesYUsuarios(ObtenerFiltroPorRol());

                    List<BsonDocument> filtradas;
                    if (campo == "estadoReunion")
                    {
                        // Usamos tu método FiltrarPorEstado que calcula el estado al vuelo
                        filtradas = FiltrarPorEstado(reunionesBase, valorFiltro);
                    }
                    else // campo == "asistenciaReunion"
                    {
                        // Usamos tu método FiltrarPorAsistencia
                        filtradas = FiltrarPorAsistencia(reunionesBase, valorFiltro, idUsuario);
                    }

                    if (filtradas.Count == 0)
                    {
                        MessageBox.Show("No se encontraron reuniones con ese criterio.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    dataGridView1.DataSource = ConstruirTabla(filtradas, nombres);
                    DiseñarGrid();
                    return; // Termina aquí para no ejecutar la consulta de MongoDB de abajo
                }

                // 3. LÓGICA PARA CAMPOS DE BASE DE DATOS (ID, Lugar, Motivo, etc.)
                FilterDefinition<BsonDocument> filtroCampo;

                if (campo == "idReunion")
                {
                    if (!int.TryParse(valorFiltro, out int codReunion))
                    {
                        MessageBox.Show("El código de reunión debe ser un número.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    filtroCampo = Builders<BsonDocument>.Filter.Eq("idReunion", codReunion);
                }
                else if (campo == "investigadoresConvocados")
                {
                    var db = Conexion.ObtenerBaseDatos();
                    var colUsuarios = db.GetCollection<BsonDocument>("Usuarios");
                    var filtroNombre = Builders<BsonDocument>.Filter.Regex("nombreUsuario", new BsonRegularExpression(valorFiltro, "i"));
                    var usuariosEncontrados = await colUsuarios.Find(filtroNombre).ToListAsync();

                    if (usuariosEncontrados.Count == 0)
                    {
                        MessageBox.Show("No se encontró ningún investigador con ese nombre.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var idsEncontrados = usuariosEncontrados.Select(u => u["idUsuario"].ToInt32()).ToList();
                    filtroCampo = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                        "investigadoresConvocados",
                        new BsonDocument("idInvestigador", new BsonDocument("$in", new BsonArray(idsEncontrados)))
                    );
                }
                else
                {
                    filtroCampo = Builders<BsonDocument>.Filter.Regex(campo, new BsonRegularExpression(valorFiltro, "i"));
                }

                // Combinamos el filtro de seguridad (por rol) con el filtro de búsqueda
                var filtroFinal = Builders<BsonDocument>.Filter.And(ObtenerFiltroPorRol(), filtroCampo);
                var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(filtroFinal);

                // IMPORTANTE: Aquí NO aplicamos AplicarFiltrosActivos() porque el usuario 
                // ya eligió un parámetro específico para buscar.

                if (reuniones.Count == 0)
                {
                    MessageBox.Show("No se encontraron reuniones con ese criterio.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dataGridView1.DataSource = ConstruirTabla(reuniones, nombrePorId);
                DiseñarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // ── Confirmar asistencia (solo Investigador) ─────────────────────────
        private async void btnConfirmarAsistencia_Click(object sender, EventArgs e)
        {
            if (ValidarSeleccionFila("Confirmar Asistencia")) return;

            int idReunionSeleccionada = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Cód."].Value);
            string opcionElegida = MostrarDialogoOpcionAsistencia(idReunionSeleccionada);

            if (string.IsNullOrEmpty(opcionElegida)) return;

            try
            {
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
                var db = Conexion.ObtenerBaseDatos();
                var colReuniones = db.GetCollection<BsonDocument>("Reuniones");

                // Si quiere confirmar, validar que no choque con reuniones ya confirmadas
                if (opcionElegida == "confirmado")
                {
                    var reunionActual = await colReuniones
                        .Find(Builders<BsonDocument>.Filter.Eq("idReunion", idReunionSeleccionada))
                        .FirstOrDefaultAsync();

                    if (reunionActual != null)
                    {
                        var todasLasReuniones = await colReuniones
                            .Find(ObtenerFiltroPorRol())
                            .ToListAsync();

                        foreach (var otra in todasLasReuniones)
                        {
                            if (otra["idReunion"].ToInt32() == idReunionSeleccionada) continue;
                            if (ObtenerAsistencia(otra, idUsuario) != "confirmado") continue;

                            if (HayConflictoDeHorario(reunionActual, otra))
                            {
                                MessageBox.Show(
                                    $"No puedes confirmar esta reunión. Tienes conflicto de horario con la reunión {otra["idReunion"].ToInt32()}.",
                                    "Conflicto de horario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }
                }

                // Actualizar asistencia en la BD
                var filtroUpdate = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("idReunion", idReunionSeleccionada),
                    Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                        "investigadoresConvocados",
                        new BsonDocument("idInvestigador", idUsuario)
                    )
                );
                var update = Builders<BsonDocument>.Update.Set("investigadoresConvocados.$.asistencia", opcionElegida);
                await colReuniones.UpdateOneAsync(filtroUpdate, update);

                MessageBox.Show($"Asistencia actualizada a: {opcionElegida}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                await DetectarYMarcarConflictos();
                await RecargarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al confirmar asistencia: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Muestra un mini diálogo para que el investigador elija su estado de asistencia.
        // Devuelve la opción elegida, o string vacío si cerró sin elegir.
        private string MostrarDialogoOpcionAsistencia(int idReunion)
        {
            string opcionElegida = "";

            using (Form frm = new Form())
            {
                frm.Text = "Confirmar asistencia";
                frm.Size = new Size(300, 180);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = false;
                frm.MinimizeBox = false;

                var lbl = new Label
                {
                    Text = $"¿Cuál es tu decisión para la reunión {idReunion}?",
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                var cbo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(20, 55),
                    Width = 240
                };
                cbo.Items.AddRange(new[] { "confirmado", "rechazado", "pendiente" });
                cbo.SelectedIndex = 0;

                var btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Location = new Point(90, 95),
                    Width = 100
                };
                btnAceptar.Click += (s, ev) =>
                {
                    opcionElegida = cbo.SelectedItem.ToString();
                    frm.DialogResult = DialogResult.OK;
                    frm.Close();
                };

                frm.Controls.AddRange(new Control[] { lbl, cbo, btnAceptar });
                frm.ShowDialog();
            }

            return opcionElegida;
        }

        // ── Agregar reunión (solo Líder) ─────────────────────────────────────
        private void btnAgregarReunión_Click(object sender, EventArgs e)
        {
            FormAgregar formAgregar = new FormAgregar(this.datosUsuario);
            formAgregar.Show();
        }

        // ── Eliminar reunión (solo Líder) ────────────────────────────────────
        private async void btnEliminarReunion_Click(object sender, EventArgs e)
        {
            if (ValidarSeleccionFila("eliminar")) return;

            int idReunion = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Cód."].Value);
            string estado = dataGridView1.CurrentRow.Cells["Estado"].Value.ToString();

            if (ReunionNoPuedeModificarse(estado, "eliminar")) return;

            try
            {
                if (await TieneInvestigadoresConfirmados(idReunion, "eliminar")) return;

                var resultado = MessageBox.Show(
                    $"¿Estás seguro de eliminar la reunión {idReunion}?",
                    "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    var colReuniones = Conexion.ObtenerBaseDatos().GetCollection<BsonDocument>("Reuniones");
                    await colReuniones.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("idReunion", idReunion));

                    MessageBox.Show("Reunión eliminada correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await RecargarGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Modificar reunión (solo Líder) ───────────────────────────────────
        private async void btnModificarReunion_Click(object sender, EventArgs e)
        {
            if (ValidarSeleccionFila("modificar")) return;

            int idReunion = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Cód."].Value);
            string estado = dataGridView1.CurrentRow.Cells["Estado"].Value.ToString();

            if (ReunionNoPuedeModificarse(estado, "editar")) return;

            try
            {
                if (await TieneInvestigadoresConfirmados(idReunion, "editar")) return;

                var colReuniones = Conexion.ObtenerBaseDatos().GetCollection<BsonDocument>("Reuniones");
                var reunion = await colReuniones
                    .Find(Builders<BsonDocument>.Filter.Eq("idReunion", idReunion))
                    .FirstOrDefaultAsync();

                FormAgregar formAgregar = new FormAgregar(this.datosUsuario)
                {
                    modoEdicion = true,
                    reunionAEditar = reunion
                };
                formAgregar.ShowDialog();
                await RecargarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir edición: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Reporte (solo Líder) ─────────────────────────────────────────────
        private void btnReporte_Click(object sender, EventArgs e)
        {
            FormReporte formReporte = new FormReporte(this.datosUsuario);
            formReporte.Show();
        }

        // ══════════════════════════════════════════════════════════════
        //  8. UTILIDADES Y NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════

        // Valida que haya exactamente una fila seleccionada en el DataGridView.
        // Muestra mensajes de error y devuelve true si la selección es inválida.
        private bool ValidarSeleccionFila(string accion)
        {
            int filasSeleccionadas = dataGridView1.SelectedCells.Cast<DataGridViewCell>().Select(c => c.RowIndex).Distinct().Count();

            if (filasSeleccionadas == 0)
            {
                MessageBox.Show($"Selecciona una reunión en la tabla primero para {accion}.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }

            if (filasSeleccionadas > 1)
            {
                MessageBox.Show(
                    $"Selecciona solo una reunión para {accion}. Has seleccionado elementos de {filasSeleccionadas} reuniones distintas.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }

            return false;
        }

        private void ActualizarReloj()
        {
            lblFechaHora.Text = DateTime.Now.ToString("yyyy/MM/dd  HH:mm:ss");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ActualizarReloj();
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Estás seguro de cerrar la sesión?", "Salida",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                new Form1().Show();
                this.Close();
            }
        }

        private void cboAsistencia_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panelFiltro_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cboEstadoReunion_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void lblNombreYApellido_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

       
    }
}
