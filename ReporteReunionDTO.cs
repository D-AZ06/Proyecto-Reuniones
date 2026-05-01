using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Reuniones
{
    internal class ReporteReunionDTO
    {
        public int IdReunion { get; set; }
        public string Fecha { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public string Motivo { get; set; }
        public string Lugar { get; set; }
        public string Asistentes { get; set; }
        public string Estado { get; set; }
    }
}
