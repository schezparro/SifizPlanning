using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SifizPlanning.Models.ViewModel
{
    public class SolicitudVacacionesDTO
    {
        public int? ID { get; set; }
        [Required]
        public string ApellidosNombres { get; set; }
        [Required]
        public string Cargo { get; set; }
        [Required]
        public string Cedula { get; set; }
        [Required]
        public DateTime FechaIngresoSolicitud { get; set; }
        [Required]
        public string Empresa { get; set; }
        [Required]
        public DateTime FechaIngresoInstitucion { get; set; }
        [Required]
        public int AniosServicio { get; set; }
        [Required]
        public int DiasCorresponden { get; set; }
        [Required]
        public int DiasDisfrutar { get; set; }
        [Required]
        public int DiasPendientes { get; set; }
        [Required]
        public int DelAnio { get; set; }
        [Required]
        public int AlAnio { get; set; }
        [Required]
        public DateTime FechaInicioVacaciones { get; set; }
        [Required]
        public DateTime FechaFinVacaciones { get; set; }
        [Required]
        public DateTime FechaPresentarseTrabajar { get; set; }
        [Required]
        public string Observaciones { get; set; }
        public string Estado { get; set; }
        public string Jefe { get; set; }
    }
}