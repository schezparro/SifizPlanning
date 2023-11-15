using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SifizPlanning.Models.ViewModel
{
	using System;
	using System.ComponentModel.DataAnnotations;

	public class SolicitudPermisosDTO
	{
		public int? ID { get; set; }
		[Required]
		public string ApellidosNombres { get; set; }
		[Required]
		public string Cedula { get; set; }
		[Required]
		public string Cargo { get; set; }
		[Required]
		public string Area { get; set; }
		[Required]
		public DateTime FechaIngresoSolicitud { get; set; }
		[Required]
		public string Empresa { get; set; }
		public string Personal { get; set; }
		public string Matrimonio { get; set; }
		public string Comida { get; set; }
		public string Paternidad { get; set; }
		public string Otros { get; set; }
		[Required]
		public DateTime FechaDesde { get; set; }
		[Required]
		public string HoraSalida { get; set; }
		[Required]
		public DateTime FechaHasta { get; set; }
		[Required]
		public string HoraRetorno { get; set; }
		[Required]
		public string Motivo { get; set; }
		[Required]
		public string Jefe { get; set; }
		public string Estado { get; set; }
	}
}