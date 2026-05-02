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

        public FormPrincipal(DatosUsuario datos)
        {
            InitializeComponent();
            datosUsuario = datos;
            ConfigurarInterfaz();
            cboEstadoReunion.SelectedIndex = 0;
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            timer1.Start();
            ActualizarReloj();
            lblNombreYApellido.Text = datosUsuario.Nombre;

            if (datosUsuario.Rol == "Investigador")
            {
                _ = DetectarYMarcarConflictos();
            }
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        private void ConfigurarInterfaz()
        /*----------------------------------------------------------------------------------------------------------------*/
        {
            MessageBox.Show($"Bienvenido, {datosUsuario.Nombre} ({datosUsuario.Rol})", "Bienvenida",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            var opciones = new List<KeyValuePair<string, string>>();
            opciones.Add(new KeyValuePair<string, string>("", ""));
            opciones.Add(new KeyValuePair<string, string>("ID reunión", "idReunion"));
            opciones.Add(new KeyValuePair<string, string>("Fecha", "fechaReunion"));
            opciones.Add(new KeyValuePair<string, string>("Hora inicio", "horaInicio"));
            opciones.Add(new KeyValuePair<string, string>("Hora fin", "horaFin"));
            opciones.Add(new KeyValuePair<string, string>("Motivo", "motivoReunion"));
            opciones.Add(new KeyValuePair<string, string>("Lugar", "lugarReunion"));
            opciones.Add(new KeyValuePair<string, string>("Nombre investigador", "investigadoresConvocados"));

            cboFiltro.DataSource = opciones;
            cboFiltro.DisplayMember = "Key";
            cboFiltro.ValueMember = "Value";

            if (datosUsuario.Rol == "Investigador")
            {
                btnAgregarReunión.Enabled = false;
                btnAgregarReunión.Visible = false;
                btnReporte.Enabled = false;
                btnReporte.Visible = false;
                btnEliminarReunion.Enabled = false;
                btnEliminarReunion.Visible = false;
                btnModificarReunion.Enabled = false;
                btnModificarReunion.Visible = false;

                cboAsistencia.Items.Add("Todas");
                cboAsistencia.Items.Add("pendiente");
                cboAsistencia.Items.Add("confirmado");
                cboAsistencia.Items.Add("rechazado");
                cboAsistencia.Items.Add("conflicto");
                cboAsistencia.SelectedIndex = 0;
            }
            else
            {
                cboAsistencia.Visible = false;
                cboAsistencia.Enabled = false;
                btnConfirmarAsistencia.Visible = false;
                btnConfirmarAsistencia.Enabled = false;
            }
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // AJUSTE 1: método pequeño que solo dice si dos rangos de hora se solapan el mismo día
        /*----------------------------------------------------------------------------------------------------------------*/
        private bool HayConflictoDeHorario(BsonDocument r1, BsonDocument r2)
        {
            // Si no son el mismo día, no hay conflicto posible
            if (r1["fechaReunion"].AsString != r2["fechaReunion"].AsString)
            {
                return false;
            }

            DateTime ini1, fin1, ini2, fin2;

            bool ok1 = DateTime.TryParse(r1["fechaReunion"].AsString + " " + r1["horaInicio"].AsString, out ini1);
            bool ok2 = DateTime.TryParse(r1["fechaReunion"].AsString + " " + r1["horaFin"].AsString, out fin1);
            bool ok3 = DateTime.TryParse(r2["fechaReunion"].AsString + " " + r2["horaInicio"].AsString, out ini2);
            bool ok4 = DateTime.TryParse(r2["fechaReunion"].AsString + " " + r2["horaFin"].AsString, out fin2);

            if (!ok1 || !ok2 || !ok3 || !ok4)
            {
                return false;
            }

            // Hay solapamiento si ini1 < fin2 Y ini2 < fin1
            return ini1 < fin2 && ini2 < fin1;
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // AJUSTE 2: método pequeño que solo devuelve el estado de asistencia de un investigador en una reunión
        /*----------------------------------------------------------------------------------------------------------------*/
        private string ObtenerAsistencia(BsonDocument r, int idInvestigador)
        {
            if (!r.Contains("investigadoresConvocados"))
            {
                return "";
            }

            foreach (var conv in r["investigadoresConvocados"].AsBsonArray)
            {
                if (conv["idInvestigador"].ToInt32() == idInvestigador)
                {
                    return conv["asistencia"].AsString;
                }
            }

            return "";
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Detecta reuniones con conflicto de horario y las marca en la BD — ahora usa HayConflictoDeHorario y ObtenerAsistencia
        /*----------------------------------------------------------------------------------------------------------------*/
        private async Task DetectarYMarcarConflictos()
        {
            try
            {
                var db = Conexion.ObtenerBaseDatos();
                var colReuniones = db.GetCollection<BsonDocument>("Reuniones");
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);

                var filtro = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                    "investigadoresConvocados",
                    new BsonDocument("idInvestigador", idUsuario)
                );
                var reuniones = await colReuniones.Find(filtro).ToListAsync();

                // Solo comparamos reuniones que no están finalizadas
                var reunionesActivas = new List<BsonDocument>();

                foreach (var r in reuniones)
                {
                    if (ObtenerEstadoReunion(r) != "Finalizadas")
                    {
                        reunionesActivas.Add(r);
                    }
                }

                foreach (var r1 in reunionesActivas)
                {
                    // Solo marcamos conflicto si está en pendiente
                    if (ObtenerAsistencia(r1, idUsuario) != "pendiente") // AJUSTE 2 aplicado
                    {
                        continue;
                    }

                    bool hayConflicto = false;

                    foreach (var r2 in reunionesActivas)
                    {
                        if (r1["idReunion"].ToInt32() == r2["idReunion"].ToInt32())
                        {
                            continue;
                        }

                        if (HayConflictoDeHorario(r1, r2)) // AJUSTE 1 aplicado
                        {
                            hayConflicto = true;
                            break;
                        }
                    }

                    if (hayConflicto)
                    {
                        var filtroUpdate = Builders<BsonDocument>.Filter.And(
                            Builders<BsonDocument>.Filter.Eq("idReunion", r1["idReunion"].ToInt32()),
                            Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                                "investigadoresConvocados",
                                new BsonDocument("idInvestigador", idUsuario)
                            )
                        );

                        var update = Builders<BsonDocument>.Update.Set(
                            "investigadoresConvocados.$.asistencia", "conflicto"
                        );

                        await colReuniones.UpdateOneAsync(filtroUpdate, update);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detectar conflictos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Botón para confirmar, rechazar o dejar pendiente la asistencia del investigador
        /*----------------------------------------------------------------------------------------------------------------*/
        private async void btnConfirmarAsistencia_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Selecciona una reunión en la tabla primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (HayMasDeUnaFilaSeleccionada("confirmar asistencia")) return;

            int idReunionSeleccionada = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Cód."].Value);

            // Mini formulario para elegir la opción de asistencia
            string opcionElegida = "";

            using (Form frmOpciones = new Form())
            {
                frmOpciones.Text = "Confirmar asistencia";
                frmOpciones.Size = new Size(300, 180);
                frmOpciones.StartPosition = FormStartPosition.CenterParent;
                frmOpciones.FormBorderStyle = FormBorderStyle.FixedDialog;
                frmOpciones.MaximizeBox = false;
                frmOpciones.MinimizeBox = false;

                Label lbl = new Label();
                lbl.Text = "¿Cuál es tu decisión para esta reunión?";
                lbl.Location = new Point(20, 20);
                lbl.AutoSize = true;

                ComboBox cbo = new ComboBox();
                cbo.DropDownStyle = ComboBoxStyle.DropDownList;
                cbo.Location = new Point(20, 55);
                cbo.Width = 240;
                cbo.Items.Add("confirmado");
                cbo.Items.Add("rechazado");
                cbo.Items.Add("pendiente");
                cbo.SelectedIndex = 0;

                Button btnAceptar = new Button();
                btnAceptar.Text = "Aceptar";
                btnAceptar.Location = new Point(90, 95);
                btnAceptar.Width = 100;

                btnAceptar.Click += (s, ev) =>
                {
                    opcionElegida = cbo.SelectedItem.ToString();
                    frmOpciones.DialogResult = DialogResult.OK;
                    frmOpciones.Close();
                };

                frmOpciones.Controls.Add(lbl);
                frmOpciones.Controls.Add(cbo);
                frmOpciones.Controls.Add(btnAceptar);
                frmOpciones.ShowDialog();
            }

            if (string.IsNullOrEmpty(opcionElegida))
            {
                return;
            }

            try
            {
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
                var db = Conexion.ObtenerBaseDatos();
                var colReuniones = db.GetCollection<BsonDocument>("Reuniones");

                // Si quiere confirmar, validamos que no haya conflicto con reuniones ya confirmadas
                if (opcionElegida == "confirmado")
                {
                    var filtroActual = Builders<BsonDocument>.Filter.Eq("idReunion", idReunionSeleccionada);
                    var reunionActual = await colReuniones.Find(filtroActual).FirstOrDefaultAsync();

                    if (reunionActual != null)
                    {
                        var filtroTodas = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                            "investigadoresConvocados",
                            new BsonDocument("idInvestigador", idUsuario)
                        );
                        var todasLasReuniones = await colReuniones.Find(filtroTodas).ToListAsync();

                        foreach (var otra in todasLasReuniones)
                        {
                            if (otra["idReunion"].ToInt32() == idReunionSeleccionada)
                            {
                                continue;
                            }

                            // Solo chequeamos contra las que ya confirmó
                            if (ObtenerAsistencia(otra, idUsuario) != "confirmado") // AJUSTE 2 aplicado
                            {
                                continue;
                            }

                            if (HayConflictoDeHorario(reunionActual, otra)) // AJUSTE 1 aplicado
                            {
                                MessageBox.Show(
                                    $"No puedes confirmar esta reunión. Tienes conflicto de horario con la reunión {otra["idReunion"].ToInt32()}.",
                                    "Conflicto de horario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }
                }

                // Actualizamos el estado en la BD
                var filtroParaUpdate = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("idReunion", idReunionSeleccionada),
                    Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                        "investigadoresConvocados",
                        new BsonDocument("idInvestigador", idUsuario)
                    )
                );

                var update = Builders<BsonDocument>.Update.Set(
                    "investigadoresConvocados.$.asistencia", opcionElegida
                );

                await colReuniones.UpdateOneAsync(filtroParaUpdate, update);

                MessageBox.Show($"Asistencia actualizada a: {opcionElegida}", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await DetectarYMarcarConflictos();
                await RecargarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al confirmar asistencia: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Recarga el DataGrid respetando los filtros actuales de estado y asistencia
        /*----------------------------------------------------------------------------------------------------------------*/
        private async Task RecargarGrid()
        {
            int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
            FilterDefinition<BsonDocument> filtroPorRol;

            if (datosUsuario.Rol == "Líder")
            {
                filtroPorRol = Builders<BsonDocument>.Filter.Eq("idLider", idUsuario);
            }
            else
            {
                filtroPorRol = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                    "investigadoresConvocados",
                    new BsonDocument("idInvestigador", idUsuario)
                );
            }

            var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(filtroPorRol);

            string estadoElegido = cboEstadoReunion.SelectedItem.ToString();
            reuniones = FiltrarPorEstado(reuniones, estadoElegido);

            if (datosUsuario.Rol == "Investigador")
            {
                string asistenciaElegida = cboAsistencia.SelectedItem.ToString();
                reuniones = FiltrarPorAsistencia(reuniones, asistenciaElegida, idUsuario);
            }

            if (reuniones.Count == 0)
            {
                dataGridView1.DataSource = null;
                return;
            }

            dataGridView1.DataSource = ConstruirTabla(reuniones, nombrePorId);
            DiseñarGrid();
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Consulta MongoDB: trae reuniones según filtro y resuelve nombres de usuarios
        /*----------------------------------------------------------------------------------------------------------------*/
        private async Task<(List<BsonDocument> reuniones, Dictionary<int, string> nombrePorId)>
            ObtenerReunionesYUsuarios(FilterDefinition<BsonDocument> filtroPrincipal)
        {
            var db = Conexion.ObtenerBaseDatos();
            var colReuniones = db.GetCollection<BsonDocument>("Reuniones");
            var colUsuarios = db.GetCollection<BsonDocument>("Usuarios");

            var reuniones = await colReuniones.Find(filtroPrincipal).ToListAsync();

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

            var nombrePorId = new Dictionary<int, string>();

            if (ids.Count > 0)
            {
                var filtroUsuarios = Builders<BsonDocument>.Filter.In("idUsuario", ids);
                var usuarios = await colUsuarios.Find(filtroUsuarios).ToListAsync();

                foreach (var u in usuarios)
                {
                    int idU = u["idUsuario"].ToInt32();
                    string nom = u["nombreUsuario"].AsString;
                    nombrePorId[idU] = nom;
                }
            }

            return (reuniones, nombrePorId);
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Construye el DataTable para mostrar en el DataGridView
        /*----------------------------------------------------------------------------------------------------------------*/
        private DataTable ConstruirTabla(List<BsonDocument> reuniones, Dictionary<int, string> nombrePorId)
        {
            int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);

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

            if (datosUsuario.Rol == "Investigador")
            {
                tabla.Columns.Add("Mi asistencia", typeof(string));
            }

            foreach (var r in reuniones)
            {
                int idLider = r["idLider"].ToInt32();
                string nombreLider;

                if (nombrePorId.ContainsKey(idLider))
                {
                    nombreLider = nombrePorId[idLider];
                }
                else
                {
                    nombreLider = "ID " + idLider;
                }

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

                if (datosUsuario.Rol == "Investigador")
                {
                    tabla.Rows.Add(
                        r["idReunion"].ToInt32(),
                        r["fechaReunion"].AsString,
                        r["horaInicio"].AsString,
                        r["horaFin"].AsString,
                        r["motivoReunion"].AsString,
                        r["lugarReunion"].AsString,
                        nombreLider,
                        asistentes,
                        estado,
                        miAsistencia
                    );
                }
                else
                {
                    tabla.Rows.Add(
                        r["idReunion"].ToInt32(),
                        r["fechaReunion"].AsString,
                        r["horaInicio"].AsString,
                        r["horaFin"].AsString,
                        r["motivoReunion"].AsString,
                        r["lugarReunion"].AsString,
                        nombreLider,
                        asistentes,
                        estado
                    );
                }
            }

            return tabla;
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Calcula el estado de una reunión comparando su fecha/hora con el momento actual
        /*----------------------------------------------------------------------------------------------------------------*/
        private string ObtenerEstadoReunion(BsonDocument r)
        {
            DateTime inicio, fin;
            bool inicioOk = DateTime.TryParse(r["fechaReunion"].AsString + " " + r["horaInicio"].AsString, out inicio);
            bool finOk = DateTime.TryParse(r["fechaReunion"].AsString + " " + r["horaFin"].AsString, out fin);

            if (!inicioOk || !finOk)
            {
                return "Desconocido";
            }

            DateTime ahora = DateTime.Now;

            if (ahora < inicio)
            {
                return "Programadas";
            }
            else if (ahora >= inicio && ahora <= fin)
            {
                return "En ejecución";
            }
            else
            {
                return "Finalizadas";
            }
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Filtra la lista de reuniones por estado (Programadas / En ejecución / Finalizadas / Todas)
        /*----------------------------------------------------------------------------------------------------------------*/
        private List<BsonDocument> FiltrarPorEstado(List<BsonDocument> reuniones, string estado)
        {
            if (estado == "Todas")
            {
                return reuniones;
            }

            var resultado = new List<BsonDocument>();

            foreach (var r in reuniones)
            {
                if (ObtenerEstadoReunion(r) == estado)
                {
                    resultado.Add(r);
                }
            }

            return resultado;
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Filtra la lista de reuniones por estado de asistencia personal del investigador
        /*----------------------------------------------------------------------------------------------------------------*/
        private List<BsonDocument> FiltrarPorAsistencia(List<BsonDocument> reuniones, string asistencia, int idUsuario)
        {
            if (asistencia == "Todas")
            {
                return reuniones;
            }

            var resultado = new List<BsonDocument>();

            foreach (var r in reuniones)
            {
                if (ObtenerAsistencia(r, idUsuario) == asistencia) // AJUSTE 2 aplicado
                {
                    resultado.Add(r);
                }
            }

            return resultado;
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Botón Ver reuniones
        /*----------------------------------------------------------------------------------------------------------------*/
        private async void btnVerReunion_Click(object sender, EventArgs e)
        {
            cboFiltro.Text = "";
            dataGridView1.DataSource = null;

            try
            {
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
                FilterDefinition<BsonDocument> filtroPorRol;

                if (datosUsuario.Rol == "Líder")
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.Eq("idLider", idUsuario);
                }
                else
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                        "investigadoresConvocados",
                        new BsonDocument("idInvestigador", idUsuario)
                    );
                }

                var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(filtroPorRol);

                string estadoElegido = cboEstadoReunion.SelectedItem.ToString();
                reuniones = FiltrarPorEstado(reuniones, estadoElegido);

                if (datosUsuario.Rol == "Investigador")
                {
                    string asistenciaElegida = cboAsistencia.SelectedItem.ToString();
                    reuniones = FiltrarPorAsistencia(reuniones, asistenciaElegida, idUsuario);
                }

                if (reuniones.Count == 0)
                {
                    dataGridView1.DataSource = null;
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

        /*----------------------------------------------------------------------------------------------------------------*/
        // Botón Consultar con parámetros
        /*----------------------------------------------------------------------------------------------------------------*/
        private async void btn_Consultar_con_parametros_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;

            try
            {
                if (cboFiltro.SelectedIndex <= 0)
                {
                    MessageBox.Show("Selecciona un campo para filtrar.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string valorFiltro = ObtenerValorDelControl().Trim();

                if (string.IsNullOrEmpty(valorFiltro))
                {
                    MessageBox.Show("Ingresa un valor para buscar.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
                FilterDefinition<BsonDocument> filtroPorRol;

                if (datosUsuario.Rol == "Líder")
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.Eq("idLider", idUsuario);
                }
                else
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                        "investigadoresConvocados",
                        new BsonDocument("idInvestigador", idUsuario)
                    );
                }

                string campo = cboFiltro.SelectedValue.ToString();
                FilterDefinition<BsonDocument> filtroCampo;

                if (campo == "idReunion")
                {
                    if (!int.TryParse(valorFiltro, out int codReunion))
                    {
                        MessageBox.Show("El código de reunión debe ser un número.", "Aviso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("No se encontró ningún investigador con ese nombre.", "Sin resultados",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var idsEncontrados = new List<int>();

                    foreach (var u in usuariosEncontrados)
                    {
                        idsEncontrados.Add(u["idUsuario"].ToInt32());
                    }

                    filtroCampo = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                        "investigadoresConvocados",
                        new BsonDocument("idInvestigador", new BsonDocument("$in", new BsonArray(idsEncontrados)))
                    );
                }
                else
                {
                    filtroCampo = Builders<BsonDocument>.Filter.Regex(campo, new BsonRegularExpression(valorFiltro, "i"));
                }

                var filtroFinal = Builders<BsonDocument>.Filter.And(filtroPorRol, filtroCampo);

                var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(filtroFinal);

                string estadoElegido = cboEstadoReunion.SelectedItem.ToString();
                reuniones = FiltrarPorEstado(reuniones, estadoElegido);

                if (datosUsuario.Rol == "Investigador")
                {
                    string asistenciaElegida = cboAsistencia.SelectedItem.ToString();
                    reuniones = FiltrarPorAsistencia(reuniones, asistenciaElegida, idUsuario);
                }

                if (reuniones.Count == 0)
                {
                    dataGridView1.DataSource = null;
                    MessageBox.Show("No se encontraron reuniones con ese criterio.", "Sin resultados",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        /*----------------------------------------------------------------------------------------------------------------*/
        private void DiseñarGrid()
        /*----------------------------------------------------------------------------------------------------------------*/
        {
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ScrollBars = ScrollBars.Both;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSteelBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        }

        private async void cboFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelFiltro.Controls.Clear();
            controlActual = null;

            string campo = cboFiltro.SelectedValue.ToString();

            if (campo == "idReunion" || campo == "lugarReunion" || campo == "investigadoresConvocados")
            {
                ComboBox cbo = new ComboBox();
                cbo.DropDownStyle = ComboBoxStyle.DropDownList;
                var opciones = await ObtenerOpcionesDesdeBD(campo);

                foreach (var op in opciones)
                {
                    cbo.Items.Add(op);
                }

                controlActual = cbo;
            }
            else if (campo == "fechaReunion")
            {
                DateTimePicker dtp = new DateTimePicker();
                dtp.Format = DateTimePickerFormat.Short;
                controlActual = dtp;
            }
            else if (campo == "horaInicio" || campo == "horaFin")
            {
                DateTimePicker dtp = new DateTimePicker();
                dtp.Format = DateTimePickerFormat.Custom;
                dtp.CustomFormat = "HH:mm";
                dtp.ShowUpDown = true;
                controlActual = dtp;
            }
            else if (campo == "motivoReunion")
            {
                TextBox txt = new TextBox();
                controlActual = txt;
            }
            else
            {
                return;
            }

            controlActual.Dock = DockStyle.Fill;
            panelFiltro.Controls.Add(controlActual);
        }

        private string ObtenerValorDelControl()
        {
            if (controlActual is TextBox txt)
            {
                return txt.Text;
            }

            if (controlActual is ComboBox cbo)
            {
                return cbo.Text;
            }

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

            return "";
        }

        private async Task<List<string>> ObtenerOpcionesDesdeBD(string campo)
        {
            var db = Conexion.ObtenerBaseDatos();
            var colReuniones = db.GetCollection<BsonDocument>("Reuniones");
            var lista = new List<string>();
            int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);

            FilterDefinition<BsonDocument> filtroPorRol;

            if (datosUsuario.Rol == "Líder")
            {
                filtroPorRol = Builders<BsonDocument>.Filter.Eq("idLider", idUsuario);
            }
            else
            {
                filtroPorRol = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                    "investigadoresConvocados",
                    new BsonDocument("idInvestigador", idUsuario)
                );
            }

            var reunionesDelUsuario = await colReuniones.Find(filtroPorRol).ToListAsync();

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

        private void btnAtras_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Estás seguro de cerrar la sesión?", "Salida",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
            formAgregar.Show();
        }

        private void ActualizarReloj()
        {
            lblFechaHora.Text = DateTime.Now.ToString("yyyy/MM/dd  HH:mm:ss");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ActualizarReloj();
        }

        private void btnReporte_Click(object sender, EventArgs e)
        {
            FormReporte formReporte = new FormReporte(this.datosUsuario);
            formReporte.Show();
            this.Hide();
        }

        private async void btnEliminarReunion_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Selecciona una reunión para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (HayMasDeUnaFilaSeleccionada("eliminar")) return;

            int idReunion = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Cód."].Value);
            string estado = dataGridView1.CurrentRow.Cells["Estado"].Value.ToString();

            if (estado == "En ejecución")
            {
                MessageBox.Show("No puedes eliminar una reunión que está en curso.", "No permitido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (estado == "Finalizadas")
            {
                MessageBox.Show("No puedes eliminar una reunión que ya finalizó.", "No permitido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verificar si algún investigador ya confirmó asistencia
            try
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
                            MessageBox.Show("No puedes eliminar esta reunión, uno o más investigadores ya confirmaron asistencia.", "No permitido",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                var resultado = MessageBox.Show(
                    $"¿Estás seguro de eliminar la reunión {idReunion}?",
                    "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    await colReuniones.DeleteOneAsync(filtro);
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

        private async void btnModificarReunion_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Selecciona una reunión para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (HayMasDeUnaFilaSeleccionada("modificar")) return;

            int idReunion = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Cód."].Value);
            string estado = dataGridView1.CurrentRow.Cells["Estado"].Value.ToString();

            if (estado == "En ejecución")
            {
                MessageBox.Show("No puedes editar una reunión que está en curso.", "No permitido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (estado == "Finalizadas")
            {
                MessageBox.Show("No puedes editar una reunión que ya finalizó.", "No permitido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
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
                            MessageBox.Show("No puedes editar esta reunión, uno o más investigadores ya confirmaron asistencia.", "No permitido",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                FormAgregar formAgregar = new FormAgregar(this.datosUsuario);
                formAgregar.modoEdicion = true;
                formAgregar.reunionAEditar = reunion;
                formAgregar.ShowDialog();
                await RecargarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir edición: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool HayMasDeUnaFilaSeleccionada(string accion)
        {
            if (dataGridView1.SelectedRows.Count > 1)
            {
                MessageBox.Show($"Solo puedes seleccionar una reunión para {accion}.",
                    "Selección múltiple", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }
    }
}