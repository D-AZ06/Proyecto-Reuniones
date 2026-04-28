using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Proyecto_Reuniones
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnIniciarSesión_Click(object sender, EventArgs e)
        {
            if (txtCorreo.Text == "" || txtContraseña.Text == "")
            {
                MessageBox.Show("Campos vacios, por favor rellene todos los campos correspondientes");
                return;
            }

            btnIniciarSesión.Enabled = false;
            lblConexion.Text = "Experando conexión con la base de datos...";
            
            try
            {
                // Conectamos a la base de datos
                var db = Conexion.ObtenerBaseDatos();
                var usuariosCollection = db.GetCollection<BsonDocument>("Usuarios"); // Asegúramos al nombre de la colección por el cual accederá

                // Creamos un filtro para buscar el usuario por correo y contraseña
                var filtro = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("correoUsuario", txtCorreo.Text.Trim()), // Aseguramos que el campo de correo en la DB se llame "correoUsuario"
                    Builders<BsonDocument>.Filter.Eq("contraseñaUsuario", txtContraseña.Text.Trim()) // Aseguramos que el campo de contraseña en la DB se llame "contraseñaUsuario"
                );

                // Buscamos el usuario en la colección
                var usuario = usuariosCollection.Find(filtro).FirstOrDefault();

                if (usuario != null) // Si encontramos un usuario que coincida con el correo y contraseña
                {
                    // Extraer el rol tal cual está en la DB
                    string rolDB = usuario["rolUsuario"].ToString().Trim();

                    // Validamos que el rol sea uno de los esperados
                    if (rolDB == "Líder" || rolDB == "Investigador")
                    {
                        DatosUsuario datos = new DatosUsuario // Creamos un objeto de datos de usuario para pasar a la pantalla principal
                        {
                            IdUsuario = usuario["idUsuario"].AsInt32, // Obtenemos el ID del usuario desde la DB
                            Nombre = usuario["nombreUsuario"].ToString(), // Obtenemos el nombre del usuario desde la DB
                            Rol = rolDB, // Asignamos el rol tal cual está en la DB
                            IdSemillero = usuario["idSemillero"].AsInt32 // Obtenemos el ID del semillero desde la DB
                        };


                        // Enviar a la pantalla principal
                        FormPrincipal frm = new FormPrincipal(datos);
                        frm.Show();
                        this.Hide();
                    }
                    else
                    {
                        // Este es el error por si el rol en la DB está mal escrito o no existe
                        MessageBox.Show($"El rol asignado '{rolDB}' no es válido. Contacte al administrador.",
                                        "Error de Configuración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        
                        btnIniciarSesión.Enabled = true;
                        lblConexion.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("Correo o contraseña incorrectos.", "Acceso Denegado",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnIniciarSesión.Enabled = true;
                    lblConexion.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
                btnIniciarSesión.Enabled = true;
                lblConexion.Text = "";
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Estás seguro de salir de la aplicación?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
