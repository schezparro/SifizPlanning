using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SifizPlanning.Util
{
    public class DataTarea : IComparable<DataTarea>
    {
        public int id;
        public int idColaborador;
        public string sdetalle;
        public string detalle;
        public DateTime dateFechaInicio;
        public string finicio;
        public string ffin;
        public string horas;
        public string modulo;
        public string dModulo;
        public int idModulo;
        public string cliente;
        public int idCliente;
        public string dCliente;
        public string actividad;
        public string dActividad;
        public string estado;
        public int idEstado;
        public string lugar;
        public string dLugar;
        public int idLugar;
        public string departamento;
        public string dDepartamento;
        public int idDepartamento;
        public string clase;
        public string coordinador;
        public string tipo;
        public string compensatoria;

        //De la interfaz        
        public int CompareTo(DataTarea otraTarea)
        {
            if (this.dateFechaInicio < otraTarea.dateFechaInicio)
                return -1;
            else if (this.dateFechaInicio == otraTarea.dateFechaInicio)
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