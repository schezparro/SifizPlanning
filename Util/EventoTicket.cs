using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SifizPlanning.Util
{
    public class EventoTicket : IComparable<EventoTicket>
    {
        private int tipo;
        public int Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }
        
        private int secuencialObjeto;
        public int SecuencialObjeto
        {
            get { return secuencialObjeto; }
            set { secuencialObjeto = value; }
        }

        private DateTime fecha;

        public DateTime Fecha
        {
            get { return fecha; }
            set { fecha = value; }
        }
        
        private string descripcion;
        public string Descripcion
        {
            get { return descripcion; }
            set { descripcion = value; }
        }

        //De la interfaz
        public int CompareTo(EventoTicket otroEvento)
        {
            if (this.Fecha < otroEvento.Fecha)
                return -1;
            else if (this.Fecha == otroEvento.Fecha)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}