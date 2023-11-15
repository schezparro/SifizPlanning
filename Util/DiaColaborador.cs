using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SifizPlanning.Util
{
    public class DiaColaborador
    {
        private DateTime fecha;

        public DateTime Fecha
        {
            get { return fecha; }
            set { fecha = value; }
        }
        private int idColaborador;

        public int IdColaborador
        {
            get { return idColaborador; }
            set { idColaborador = value; }
        }
    }
}