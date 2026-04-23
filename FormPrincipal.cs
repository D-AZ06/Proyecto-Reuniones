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

        public FormPrincipal(DatosUsuario datos)
        {
            InitializeComponent();
            datosUsuario = datos;

            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            if (datosUsuario.Rol == "Líder")
            {

            }
            else if (datosUsuario.Rol == "Investigador")
            {

            }
        }
    }
}
