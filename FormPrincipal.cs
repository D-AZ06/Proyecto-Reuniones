using DnsClient.Protocol;
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
        private Control controlActual;

        public FormPrincipal(DatosUsuario datos)
        {
            InitializeComponent();
            datosUsuario = datos;

            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            MessageBox.Show($"Bienvenido, {datosUsuario.Nombre} ({datosUsuario.Rol})", "Bienvenida",
        MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Cargar ComboBox con texto visible y valor interno separados
            var opciones = new List<KeyValuePair<string, string>>();
            opciones.Add(new KeyValuePair<string, string>("", ""));
            opciones.Add(new KeyValuePair<string, string>("ID reunión", "idReunion"));
            opciones.Add(new KeyValuePair<string, string>("Fecha", "fechaReunion"));
            opciones.Add(new KeyValuePair<string, string>("Hora inicio", "horaInicio"));
            opciones.Add(new KeyValuePair<string, string>("Hora fin", "horaFin"));
            opciones.Add(new KeyValuePair<string, string>("Motivo", "motivoReunion"));
            opciones.Add(new KeyValuePair<string, string>("Lugar", "lugarReunion"));
            opciones.Add(new KeyValuePair<string, string>("Nombre investigador", "idInvestigadores"));

            cboFiltro.DataSource = opciones;
            cboFiltro.DisplayMember = "Key";   // lo que ve el usuario
            cboFiltro.ValueMember = "Value"; // lo que usa el código

            if (datosUsuario.Rol == "Investigador")
            {
                btnAgregarReunión.Enabled = false;
                btnAgregarReunión.Visible = false;
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

        // ─────────────────────────────────────────────────────────────────
        // MÉTODO COMPARTIDO: trae reuniones filtradas por rol + sus usuarios
        // ─────────────────────────────────────────────────────────────────
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

                if (r.Contains("idInvestigadores"))
                {
                    foreach (var id in r["idInvestigadores"].AsBsonArray)
                    {
                        ids.Add(id.ToInt32());
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

        // ─────────────────────────────────────────────────────────────────
        // MÉTODO COMPARTIDO: convierte lista de docs en DataTable
        // ─────────────────────────────────────────────────────────────────
        private DataTable ConstruirTabla(List<BsonDocument> reuniones, Dictionary<int, string> nombrePorId)
        {
            var tabla = new DataTable();
            tabla.Columns.Add("Cód.", typeof(int));
            tabla.Columns.Add("Fecha", typeof(string));
            tabla.Columns.Add("Inicio", typeof(string));
            tabla.Columns.Add("Fin", typeof(string));
            tabla.Columns.Add("Motivo", typeof(string));
            tabla.Columns.Add("Lugar", typeof(string));
            tabla.Columns.Add("Líder", typeof(string));
            tabla.Columns.Add("Asistentes", typeof(string));

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

                if (r.Contains("idInvestigadores"))
                {
                    foreach (var x in r["idInvestigadores"].AsBsonArray)
                    {
                        int id = x.ToInt32();
                        string nombre;

                        if (nombrePorId.ContainsKey(id))
                        {
                            nombre = nombrePorId[id];
                        }
                        else
                        {
                            nombre = "ID " + id;
                        }

                        if (asistentes != "")
                        {
                            asistentes += Environment.NewLine;
                        }

                        asistentes += "• " + nombre;
                    }
                }

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

            return tabla;
        }

        private async void btnVerReunion_Click(object sender, EventArgs e)
        {
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
                    filtroPorRol = Builders<BsonDocument>.Filter.AnyEq("idInvestigadores", idUsuario);
                }

                var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(filtroPorRol);

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

        private void cboFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelFiltro.Controls.Clear();
            controlActual = null;

            string campo = cboFiltro.SelectedValue.ToString();

            if (campo == "idReunion" || campo == "motivoReunion" ||
                campo == "lugarReunion" || campo == "idInvestigadores")
            {
                TextBox txt = new TextBox();
                controlActual = txt;
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

            if (controlActual is DateTimePicker dtp)
            {
                if (dtp.Format == DateTimePickerFormat.Short)
                {
                    return dtp.Value.ToString("yyyy-MM-dd"); // fecha
                }

                else
                {
                    return dtp.Value.ToString("HH:mm"); // hora
                } 
            }
            return "";
        }

        private async void btn_Consultar_con_parametros_Click(object sender, EventArgs e)
        {
            try
            {
                // Validar selección del ComboBox (posición 0 = vacío)
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
                    filtroPorRol = Builders<BsonDocument>.Filter.AnyEq("idInvestigadores", idUsuario);
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
                else if (campo == "idInvestigadores")
                {
                    // El usuario escribe un nombre, buscamos su ID en Usuarios
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

                    filtroCampo = Builders<BsonDocument>.Filter.AnyIn("idInvestigadores", idsEncontrados);
                }
                else
                {
                    // fechaReunion, horaInicio, horaFin, motivoReunion, lugarReunion
                    // Búsqueda parcial sin distinguir mayúsculas
                    filtroCampo = Builders<BsonDocument>.Filter.Regex(campo, new BsonRegularExpression(valorFiltro, "i"));
                }

                // Combinar filtro de rol + filtro del parámetro
                var filtroFinal = Builders<BsonDocument>.Filter.And(filtroPorRol, filtroCampo);

                var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(filtroFinal);

                if (reuniones.Count == 0)
                {
                    dataGridView1.DataSource = null;
                    MessageBox.Show("No se encontraron reuniones con ese criterio.", "Sin resultados",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dataGridView1.DataSource = ConstruirTabla(reuniones, nombrePorId);
                DisenarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
