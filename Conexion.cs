using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_Reuniones
{
    internal class Conexion
    {
        
        public static IMongoDatabase ObtenerBaseDatos()
        {
            try
            {
                string uri = File.ReadAllText("llaveAcceso.txt").Trim();
                var cliente = new MongoClient(uri);
                return cliente.GetDatabase("BD-ProReuniones");
            }
            catch
            {
                MessageBox.Show("No se encontró la llave de conexión (llave.txt)");
                return null;
            }
        }
    }
}
