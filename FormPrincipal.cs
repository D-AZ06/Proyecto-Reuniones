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
        private DatosUsuario datosUsuario; // Almacena la información del usuario actual (ID, nombre, rol)
        private Control controlActual; // Referencia al control dinámico que se muestra para el filtro (TextBox o DateTimePicker)

        public FormPrincipal(DatosUsuario datos) // Recibe los datos del usuario que inició sesión para personalizar la experiencia
        {
            InitializeComponent();
            datosUsuario = datos; // Guardamos los datos del usuario para usarlos en toda la interfaz

            ConfigurarInterfaz(); // Configura saludos, permisos y opciones de filtro según el rol del usuario
            cboEstadoReunion.SelectedIndex = 0; // Seleccionamos "Todas" por defecto para mostrar todas las reuniones sin filtrar por estado
        }

        /*----------------------------------------------------------------------------------------------------------------*/
        // Configura la interfaz según el rol del usuario, muestra un mensaje de bienvenida y carga el ComboBox de filtros
        /*----------------------------------------------------------------------------------------------------------------*/
        private void ConfigurarInterfaz()
        {
            MessageBox.Show($"Bienvenido, {datosUsuario.Nombre} ({datosUsuario.Rol})", "Bienvenida", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

            cboFiltro.DataSource = opciones; // asignamos la lista de opciones al ComboBox
            cboFiltro.DisplayMember = "Key"; // Opciones que se muestran al usuario
            cboFiltro.ValueMember = "Value"; // Valores que se usan internamente para construir los filtros

            // Según el rol, ocultamos o mostramos el botón de agregar reunión (solo Líderes pueden agregar)
            if (datosUsuario.Rol == "Investigador")
            {
                btnAgregarReunión.Enabled = false;
                btnAgregarReunión.Visible = false;
            }
        }

        // Botón para cerrar sesión, vuelve al formulario de login
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

        // Botón para abrir el formulario de agregar reunión, solo visible para Líderes
        private void btnAgregarReunión_Click(object sender, EventArgs e)
        {
            FormAgregar formAgregar = new FormAgregar(this.datosUsuario);
            formAgregar.Show(); 
        }

        // ─────────────────────────────────────────────────────────────────
        // Método compartido: obtiene las reuniones según un filtro, y también los nombres de los usuarios involucrados para mostrar en la tabla
        // ─────────────────────────────────────────────────────────────────
        private async Task<(List<BsonDocument> reuniones, Dictionary<int, string> nombrePorId)> 
            ObtenerReunionesYUsuarios(FilterDefinition<BsonDocument> filtroPrincipal)
        {
            var db = Conexion.ObtenerBaseDatos(); // Obtenemos la base de datos usando la clase de conexión (conexión a MongoDB)
            var colReuniones = db.GetCollection<BsonDocument>("Reuniones"); // Colección de reuniones donde se almacenan los documentos con la información de cada reunión
            var colUsuarios = db.GetCollection<BsonDocument>("Usuarios"); // Colección de usuarios donde se almacenan los documentos con la información de cada usuario (ID, nombre, rol)

            var reuniones = await colReuniones.Find(filtroPrincipal).ToListAsync(); // Obtenemos la lista de reuniones que cumplen el filtro (puede ser por rol, por búsqueda de parámetros, etc.)

            var ids = new HashSet<int>(); // Para almacenar los IDs de líderes e investigadores involucrados en las reuniones obtenidas, para luego buscar sus nombres en la colección de usuarios. Usamos HashSet para evitar duplicados.

            // Recorremos las reuniones obtenidas para extraer los IDs de líderes e investigadores
            foreach (var r in reuniones)
            {
                ids.Add(r["idLider"].ToInt32()); // Agregamos el ID del líder de la reunión a la lista de IDs a buscar

                if (r.Contains("idInvestigadores")) // Si la reunión tiene investigadores asignados, recorremos su lista de IDs y los agregamos a la lista de IDs a buscar
                {
                    // La propiedad "idInvestigadores" es un arreglo de IDs, por eso usamos AsBsonArray para recorrerlo
                    foreach (var id in r["idInvestigadores"].AsBsonArray)
                    {
                        ids.Add(id.ToInt32()); // Agregamos el ID del investigador a la lista de IDs a buscar
                    }
                }
            }

            var nombrePorId = new Dictionary<int, string>(); // Diccionario para almacenar el nombre de cada usuario por su ID, para mostrar en la tabla de reuniones. La clave es el ID del usuario y el valor es su nombre.

            // Si hay IDs para buscar, hacemos una consulta a la colección de usuarios usando un filtro $in para obtener los documentos de los usuarios cuyos IDs están en la lista de IDs que construimos. Esto nos permite obtener el nombre de cada usuario involucrado en las reuniones que vamos a mostrar.
            if (ids.Count > 0)
            {
                // Construimos un filtro $in para buscar los usuarios cuyos IDs están en la lista de IDs que construimos a partir de las reuniones obtenidas. Esto nos permitirá obtener el nombre de cada usuario involucrado en las reuniones que vamos a mostrar.
                var filtroUsuarios = Builders<BsonDocument>.Filter.In("idUsuario", ids);
                var usuarios = await colUsuarios.Find(filtroUsuarios).ToListAsync();

                // Recorremos los usuarios obtenidos para llenar el diccionario de nombrePorId, donde la clave es el ID del usuario y el valor es su nombre. Esto nos permitirá mostrar el nombre de cada líder e investigador en la tabla de reuniones en lugar de solo su ID.
                foreach (var u in usuarios)
                {
                    int idU = u["idUsuario"].ToInt32();
                    string nom = u["nombreUsuario"].AsString;
                    nombrePorId[idU] = nom;
                }
            }

            // Devolvemos la lista de reuniones obtenidas según el filtro, y el diccionario con el nombre de cada usuario por su ID para mostrar en la tabla.
            return (reuniones, nombrePorId);
        }

        // ─────────────────────────────────────────────────────────────────
        // Metodo queconvierte lista de docs en DataTable
        // ─────────────────────────────────────────────────────────────────
        private DataTable ConstruirTabla(List<BsonDocument> reuniones, Dictionary<int, string> nombrePorId)
        {
            // Creamos una tabla con las columnas que queremos mostrar en el DataGridView, y luego la llenamos con los datos de las reuniones obtenidas. Para cada reunión, obtenemos el nombre del líder e investigadores usando el diccionario de nombrePorId para mostrar su nombre en lugar de su ID. También calculamos el estado de la reunión (programada, en ejecución o finalizada) según su fecha y hora.
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

            // Recorremos la lista de reuniones obtenidas para llenar la tabla. Para cada reunión, obtenemos el nombre del líder e investigadores usando el diccionario de nombrePorId para mostrar su nombre en lugar de su ID. También calculamos el estado de la reunión (programada, en ejecución o finalizada) según su fecha y hora.
            foreach (var r in reuniones)
            {
                // Obtenemos el ID del líder de la reunión y buscamos su nombre en el diccionario de nombrePorId para mostrarlo en la tabla. Si no se encuentra el ID en el diccionario, mostramos "ID " seguido del número del ID como fallback.
                int idLider = r["idLider"].ToInt32();
                string nombreLider;

                // Buscamos el nombre del líder en el diccionario de nombrePorId usando su ID. Si no se encuentra el ID en el diccionario, mostramos "ID " seguido del número del ID como fallback.
                if (nombrePorId.ContainsKey(idLider))
                {
                    nombreLider = nombrePorId[idLider];
                }
                else
                {
                    nombreLider = "ID " + idLider;
                }

                string asistentes = ""; // Variable para concatenar los nombres de los asistentes a la reunión (líder + investigadores). Se mostrará en una sola celda del DataGridView, separados por saltos de línea.

                // Agregamos el líder como primer asistente a la reunión, ya que siempre está presente. Luego, si hay investigadores asignados a la reunión, los agregamos también a la lista de asistentes. Para cada investigador,
                // buscamos su nombre en el diccionario de nombrePorId usando su ID. Si no se encuentra el ID en el diccionario, mostramos "ID " seguido del número del ID como fallback. Cada asistente se muestra en una nueva
                // línea dentro de la misma celda del DataGridView.
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

                // Calculamos el estado de la reunión (programada, en ejecución o finalizada) según su fecha y hora usando el método ObtenerEstadoReunion, que compara la fecha y hora de la reunión con la fecha y hora actual para determinar su estado.
                string estado = ObtenerEstadoReunion(r);

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

            return tabla;
        }

        // ─────────────────────────────────────────────────────────────────
        // Metodo que filtra la reunión según su fecha y hora, para mostrar su estado (programada, en ejecución o finalizada)
        // ─────────────────────────────────────────────────────────────────
        private string ObtenerEstadoReunion(BsonDocument r)
        {
            // Combinamos fecha + hora para construir un DateTime real
            string fechaStr = r["fechaReunion"].AsString;  // "2026-07-22"
            string inicioStr = r["horaInicio"].AsString;    // "09:00"
            string finStr = r["horaFin"].AsString;       // "11:00"

            // Parseamos inicio y fin como DateTime completo
            DateTime inicio;
            DateTime fin;
            bool inicioOk = DateTime.TryParse(fechaStr + " " + inicioStr, out inicio);
            bool finOk = DateTime.TryParse(fechaStr + " " + finStr, out fin);

            // Si no se pudo parsear, no clasificamos
            if (!inicioOk || !finOk)
            {
                return "Desconocido";
            }

            DateTime ahora = DateTime.Now; // Obtenemos la fecha y hora actual para comparar con la fecha y hora de la reunión y determinar su estado (programada, en ejecución o finalizada)

            if (ahora < inicio) // Si la fecha y hora actual es anterior a la fecha y hora de inicio de la reunión, entonces la reunión está programada para el futuro, por lo que su estado es "Programada".
            {
                return "Programadas";
            }
            else if (ahora >= inicio && ahora <= fin) // Si la fecha y hora actual está entre la fecha y hora de inicio y fin de la reunión, entonces la reunión está en curso, por lo que su estado es "En ejecución".
            {
                return "En ejecución";
            }
            else // Si la fecha y hora actual es posterior a la fecha y hora de fin de la reunión, entonces la reunión ya terminó, por lo que su estado es "Finalizada".
            {
                return "Finalizadas";
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Metodo que filtra una lista de reuniones por estado ───────────────
        // ─────────────────────────────────────────────────────────────────
        private List<BsonDocument> FiltrarPorEstado(List<BsonDocument> reuniones, string estado)
        {
            // Si es "Todas" devolvemos todo sin filtrar
            if (estado == "Todas")
            {
                return reuniones;
            }

            // Si se eligió un estado específico (Programadas, En ejecución o Finalizadas), filtramos la lista de reuniones obtenidas para quedarnos solo con las que tienen el estado seleccionado. Para cada reunión, usamos el método ObtenerEstadoReunion para determinar su estado actual según su fecha y hora, y comparamos con el estado seleccionado para decidir si la incluimos en el resultado o no.
            var resultado = new List<BsonDocument>();

            // Recorremos la lista de reuniones obtenidas para filtrar solo las que tienen el estado seleccionado. Para cada reunión, usamos el método ObtenerEstadoReunion para determinar su estado actual según su fecha y hora, y comparamos con el estado seleccionado para decidir si la incluimos en el resultado o no.
            foreach (var r in reuniones)
            {
                if (ObtenerEstadoReunion(r) == estado) // Si el estado de la reunión coincide con el estado seleccionado en el ComboBox, entonces la incluimos en la lista de resultados que se mostrará en la tabla.
                {
                    resultado.Add(r); // Agregamos la reunión a la lista de resultados que se mostrará en la tabla, ya que su estado coincide con el estado seleccionado en el ComboBox.
                }
            }

            return resultado; // Devolvemos la lista de reuniones filtrada por el estado seleccionado, que se mostrará en la tabla del DataGridView.
        }


        // Botón para consultar las reuniones según el rol del usuario y el estado seleccionado. Para Líderes muestra las reuniones donde son líderes, para Investigadores muestra las reuniones donde son asistentes. Luego filtra por estado (programada, en ejecución, finalizada) según lo seleccionado en el ComboBox de estado.
        private async void btnVerReunion_Click(object sender, EventArgs e)
        {
            cboFiltro.Text = ""; // Limpiamos el ComboBox de filtro para que no quede ningún filtro aplicado al mostrar las reuniones, y así mostrar todas las reuniones según el estado seleccionado sin filtrar por otros parámetros. Esto garantiza que al hacer clic en "Ver reuniones" se muestren todas las reuniones correspondientes al rol del usuario y al estado seleccionado, sin que queden filtros anteriores aplicados.
            dataGridView1.DataSource = null; // Limpiamos el DataGridView para que no muestre datos antiguos mientras se cargan las reuniones según el rol del usuario y el estado seleccionado. Esto mejora la experiencia del usuario al evitar confusiones con datos anteriores mientras se realiza la consulta de las reuniones actuales.

            try
            {
                // Validar que el usuario haya seleccionado un estado para filtrar (posición 0 = "Todas")
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
                FilterDefinition<BsonDocument> filtroPorRol;

                if (datosUsuario.Rol == "Líder") // Si el usuario es Líder, el filtro se construye para buscar reuniones donde el campo "idLider" sea igual al ID del usuario, lo que significa que el usuario es el líder de esas reuniones.
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.Eq("idLider", idUsuario); // Construimos un filtro para buscar reuniones donde el campo "idLider" sea igual al ID del usuario actual, lo que significa que el usuario es el líder de esas reuniones. Este filtro se usará para obtener solo las reuniones donde el usuario es líder.
                }
                else // Si el usuario es Investigador, el filtro se construye para buscar reuniones donde el campo "idInvestigadores" contenga el ID del usuario, lo que significa que el usuario es un asistente en esas reuniones.
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.AnyEq("idInvestigadores", idUsuario); // Construimos un filtro para buscar reuniones donde el campo "idInvestigadores" contenga el ID del usuario actual, lo que significa que el usuario es un asistente en esas reuniones. Este filtro se usará para obtener solo las reuniones donde el usuario es asistente.
                }

                // Usamos el filtro construido según el rol del usuario para obtener la lista de reuniones correspondientes, y también obtenemos el diccionario con el nombre de cada usuario por su ID para mostrar en la tabla. Luego, filtramos la lista de reuniones obtenida por el estado seleccionado en el ComboBox de estado (programada, en ejecución, finalizada) usando el método FiltrarPorEstado. Finalmente, mostramos las reuniones filtradas en el DataGridView construyendo una tabla con los datos de las reuniones y los nombres de los usuarios.
                var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(filtroPorRol);

                // Filtramos la lista de reuniones obtenida por el estado seleccionado en el ComboBox de estado (programada, en ejecución, finalizada) usando el método FiltrarPorEstado. Esto nos permitirá mostrar solo las reuniones que tienen el estado seleccionado por el usuario en la interfaz.
                string estadoElegido = cboEstadoReunion.SelectedItem.ToString();
                reuniones = FiltrarPorEstado(reuniones, estadoElegido);


                // Si después de aplicar el filtro por estado no quedan reuniones para mostrar, mostramos un mensaje informativo al usuario indicando que no se encontraron reuniones con ese criterio, y limpiamos el DataGridView para que no muestre datos antiguos. El mensaje se personaliza según el rol del usuario: si es Líder, se indica que no hay reuniones en su semillero; si es Investigador, se indica que no está en ninguna reunión.
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
                // Si hay reuniones para mostrar después de aplicar el filtro por estado, construimos una tabla con los datos de las reuniones y los nombres de los usuarios usando el método ConstruirTabla,
                // y asignamos esa tabla como fuente de datos del DataGridView para mostrarla en la interfaz. Luego, aplicamos un diseño al DataGridView para mejorar su apariencia y legibilidad.
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


        // Evento que se dispara al cambiar la selección del ComboBox de filtro, para mostrar el control adecuado (TextBox o DateTimePicker) según el campo seleccionado. Si se selecciona un campo de texto (ID reunión, motivo, lugar), se muestra un TextBox para ingresar el valor a buscar. Si se selecciona un campo de fecha (fecha reunión), se muestra un DateTimePicker con formato de fecha. Si se selecciona un campo de hora (hora inicio, hora fin), se muestra un DateTimePicker con formato de hora. El control dinámico se agrega al panelFiltro para que el usuario pueda ingresar el valor a buscar según el campo seleccionado.
        private void cboFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Limpiamos el panel donde se muestra el control dinámico para ingresar el valor a buscar, y también limpiamos la referencia al control actual para evitar confusiones. Luego, según el campo seleccionado en el ComboBox de filtro, creamos el control adecuado (TextBox o DateTimePicker) y lo agregamos al panelFiltro para que el usuario pueda ingresar el valor a buscar según el campo seleccionado.
            panelFiltro.Controls.Clear();
            controlActual = null;

            string campo = cboFiltro.SelectedValue.ToString();

            // Según el campo seleccionado en el ComboBox de filtro, creamos el control adecuado (TextBox o DateTimePicker) y lo agregamos al panelFiltro para que el usuario pueda ingresar el valor a buscar según el campo seleccionado. Para campos de texto (ID reunión, motivo, lugar), se muestra un TextBox. Para campo de fecha (fecha reunión), se muestra un DateTimePicker con formato de fecha. Para campos de hora (hora inicio, hora fin), se muestra un DateTimePicker con formato de hora.
            if (campo == "idReunion" || campo == "motivoReunion" || campo == "lugarReunion" || campo == "idInvestigadores")
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

            // Agregamos el control dinámico al panelFiltro para que el usuario pueda ingresar el valor a buscar según el campo seleccionado. El control se ajusta al tamaño del panel para mejorar la experiencia de usuario.
            controlActual.Dock = DockStyle.Fill;
            panelFiltro.Controls.Add(controlActual);
        }

        // Método para obtener el valor ingresado por el usuario en el control dinámico (TextBox o DateTimePicker) según el campo seleccionado en el ComboBox de filtro. Este método se utiliza para construir el filtro de búsqueda cuando el usuario hace clic en el botón de consultar con parámetros, y permite obtener el valor ingresado por el usuario para buscar reuniones según ese valor.
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

        // Botón para consultar las reuniones según el parámetro ingresado por el usuario en el control dinámico (TextBox o DateTimePicker) y el campo seleccionado en el ComboBox de filtro. El método valida que se haya seleccionado un campo para filtrar y que se haya ingresado un valor, luego construye un filtro de búsqueda según el campo y valor ingresados, combinándolo con el filtro por rol del usuario. Luego obtiene las reuniones que cumplen ese filtro, las filtra por estado según lo seleccionado en el ComboBox de estado, y muestra los resultados en el DataGridView. Si no se encuentran reuniones con ese criterio, muestra un mensaje informativo al usuario.
        private async void btn_Consultar_con_parametros_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null; // Limpiamos el DataGridView para que no muestre datos antiguos mientras se cargan las reuniones según el filtro aplicado por el usuario. Esto mejora la experiencia del usuario al evitar confusiones con datos anteriores mientras se realiza la consulta de las reuniones actuales según el filtro ingresado.

            try
            {
                // Validar selección del ComboBox (posición 0 = vacío)
                if (cboFiltro.SelectedIndex <= 0)
                {
                    MessageBox.Show("Selecciona un campo para filtrar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Obtener el valor ingresado por el usuario en el control dinámico (TextBox o DateTimePicker) según el campo seleccionado en el ComboBox de filtro, y construir un filtro de búsqueda para consultar las reuniones según ese valor. El filtro se combina con el filtro por rol del usuario para obtener solo las reuniones correspondientes a su rol (Líder o Investigador). Luego, se obtienen las reuniones que cumplen ese filtro, se filtran por estado según lo seleccionado en el ComboBox de estado, y se muestran los resultados en el DataGridView. Si no se encuentran reuniones con ese criterio, se muestra un mensaje informativo al usuario.
                string valorFiltro = ObtenerValorDelControl().Trim();

                if (string.IsNullOrEmpty(valorFiltro))
                {
                    MessageBox.Show("Ingresa un valor para buscar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Construimos el filtro de búsqueda según el campo seleccionado en el ComboBox de filtro y el valor ingresado por el usuario en el control dinámico. El filtro se combina con el filtro por rol del usuario para obtener solo las reuniones correspondientes a su rol (Líder o Investigador). Luego, se obtienen las reuniones que cumplen ese filtro, se filtran por estado según lo seleccionado en el ComboBox de estado, y se muestran los resultados en el DataGridView. Si no se encuentran reuniones con ese criterio, se muestra un mensaje informativo al usuario.
                int idUsuario = Convert.ToInt32(datosUsuario.IdUsuario);
                FilterDefinition<BsonDocument> filtroPorRol;

                // Según el rol del usuario, construimos el filtro de búsqueda para obtener solo las reuniones correspondientes a su rol. Si el usuario es Líder, el filtro se construye para buscar reuniones donde el campo "idLider" sea igual al ID del usuario, lo que significa que el usuario es el líder de esas reuniones. Si el usuario es Investigador, el filtro se construye para buscar reuniones donde el campo "idInvestigadores" contenga el ID del usuario, lo que significa que el usuario es un asistente en esas reuniones. Este filtro se usará para obtener solo las reuniones correspondientes al rol del usuario, y luego se combinará con el filtro construido según el campo y valor ingresados por el usuario para obtener las reuniones que cumplen ambos criterios.
                if (datosUsuario.Rol == "Líder")
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.Eq("idLider", idUsuario);
                }
                else
                {
                    filtroPorRol = Builders<BsonDocument>.Filter.AnyEq("idInvestigadores", idUsuario);
                }

                // Según el campo seleccionado en el ComboBox de filtro, construimos el filtro de búsqueda para consultar las reuniones según el valor ingresado por el usuario en el control dinámico. Para el campo "idReunion", validamos que el valor ingresado sea un número y construimos un filtro de igualdad. Para el campo "idInvestigadores", buscamos los usuarios cuyo nombre coincida con el valor ingresado (usando una búsqueda parcial sin distinguir mayúsculas) y obtenemos sus IDs para construir un filtro $in que busque reuniones donde el campo "idInvestigadores" contenga alguno de esos IDs. Para los campos de texto (motivoReunion, lugarReunion), construimos un filtro de expresión regular para buscar coincidencias parciales sin distinguir mayúsculas. Para el campo de fecha (fechaReunion) y horas (horaInicio, horaFin), construimos un filtro de igualdad con el formato adecuado.
                string campo = cboFiltro.SelectedValue.ToString();
                FilterDefinition<BsonDocument> filtroCampo;

                // Construimos el filtro de búsqueda según el campo seleccionado en el ComboBox de filtro y el valor ingresado por el usuario en el control dinámico. El filtro se combina con el filtro por rol del usuario para obtener solo las reuniones correspondientes
                // a su rol (Líder o Investigador). Luego, se obtienen las reuniones que cumplen ese filtro, se filtran por estado según lo seleccionado en el ComboBox de estado, y se muestran los resultados en el DataGridView. Si no se encuentran reuniones con ese criterio, se muestra un mensaje informativo al usuario.
                if (campo == "idReunion")
                {
                    // Para el campo "idReunion", validamos que el valor ingresado sea un número y construimos un filtro de igualdad para buscar reuniones donde el campo "idReunion" sea igual al número ingresado por el usuario. Esto nos permitirá obtener la reunión específica cuyo ID coincida con el valor ingresado.
                    if (!int.TryParse(valorFiltro, out int codReunion))
                    {
                        MessageBox.Show("El código de reunión debe ser un número.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    filtroCampo = Builders<BsonDocument>.Filter.Eq("idReunion", codReunion);
                }

                // Para el campo "idInvestigadores", realizamos una búsqueda en la colección de usuarios para encontrar los IDs de los usuarios cuyo nombre coincida con el valor ingresado por el usuario (usando una búsqueda parcial sin distinguir mayúsculas). Luego, construimos un filtro $in para buscar reuniones donde el campo "idInvestigadores" contenga alguno de esos IDs. Esto nos permitirá obtener las reuniones donde alguno de los investigadores asignados tenga un nombre que coincida con el valor ingresado por el usuario.
                else if (campo == "idInvestigadores")
                {
                    // El usuario escribe un nombre, buscamos su ID en Usuarios
                    var db = Conexion.ObtenerBaseDatos();
                    var colUsuarios = db.GetCollection<BsonDocument>("Usuarios");

                    // Búsqueda de usuarios cuyo nombre coincida con el valor ingresado por el usuario, usando una búsqueda parcial sin distinguir mayúsculas. Esto nos permitirá encontrar los usuarios cuyo nombre contenga el valor ingresado, sin importar si escribieron mayúsculas o minúsculas, y así obtener sus IDs para buscar las reuniones donde estén asignados como investigadores.
                    var filtroNombre = Builders<BsonDocument>.Filter.Regex("nombreUsuario", new BsonRegularExpression(valorFiltro, "i"));
                    var usuariosEncontrados = await colUsuarios.Find(filtroNombre).ToListAsync();

                    if (usuariosEncontrados.Count == 0)
                    {
                        MessageBox.Show("No se encontró ningún investigador con ese nombre.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Obtenemos los IDs de los usuarios encontrados para construir el filtro $in que busque reuniones donde el campo "idInvestigadores" contenga alguno de esos IDs. Esto nos permitirá obtener las reuniones donde alguno de los investigadores asignados tenga un nombre que coincida con el valor ingresado por el usuario.
                    var idsEncontrados = new List<int>();

                    // Recorremos los usuarios encontrados para extraer sus IDs y agregarlos a la lista de IDs a buscar en el filtro $in. Esto nos permitirá construir un filtro que busque reuniones donde el campo "idInvestigadores" contenga alguno de esos IDs, lo que significa que alguno de los investigadores asignados a esas reuniones tiene un nombre que coincide con el valor ingresado por el usuario.
                    foreach (var u in usuariosEncontrados)
                    {
                        idsEncontrados.Add(u["idUsuario"].ToInt32());
                    }

                    // Construimos un filtro $in para buscar reuniones donde el campo "idInvestigadores" contenga alguno de los IDs de los usuarios encontrados, lo que significa que alguno de los investigadores asignados a esas reuniones tiene un nombre que coincide con el valor ingresado por el usuario. Esto nos permitirá obtener las reuniones correspondientes a los investigadores cuyo nombre coincide con el valor ingresado.
                    filtroCampo = Builders<BsonDocument>.Filter.AnyIn("idInvestigadores", idsEncontrados);
                }

                // Para los campos de texto (motivoReunion, lugarReunion), construimos un filtro de expresión regular para buscar coincidencias parciales sin distinguir mayúsculas. Para el campo de fecha (fechaReunion) y horas (horaInicio, horaFin), construimos un filtro de igualdad con el formato adecuado. Esto nos permitirá obtener las reuniones que coincidan con el valor ingresado por el usuario según el campo seleccionado, ya sea buscando coincidencias parciales para campos de texto o buscando igualdad para campos de fecha y hora.
                else
                {
                    // fechaReunion, horaInicio, horaFin, motivoReunion, lugarReunion
                    // Búsqueda parcial sin distinguir mayúsculas
                    filtroCampo = Builders<BsonDocument>.Filter.Regex(campo, new BsonRegularExpression(valorFiltro, "i"));
                }

                // Combinar filtro de rol + filtro del parámetro
                var filtroFinal = Builders<BsonDocument>.Filter.And(filtroPorRol, filtroCampo);

                // Usamos el filtro combinado para obtener la lista de reuniones correspondientes, y también obtenemos el diccionario con el nombre de cada usuario por su ID para mostrar en la tabla. Luego, filtramos la lista de reuniones obtenida por el estado seleccionado en el ComboBox de estado (programada, en ejecución, finalizada) usando el método FiltrarPorEstado. Finalmente, mostramos las reuniones filtradas en el DataGridView construyendo una tabla con los datos de las reuniones y los nombres de los usuarios.
                var (reuniones, nombrePorId) = await ObtenerReunionesYUsuarios(filtroFinal);

                string estadoElegido = cboEstadoReunion.SelectedItem.ToString();
                reuniones = FiltrarPorEstado(reuniones, estadoElegido);

                if (reuniones.Count == 0)
                {
                    dataGridView1.DataSource = null;
                    MessageBox.Show("No se encontraron reuniones con ese criterio.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
