using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Reflection;
using System.Data.Entity.Core.Objects;
using System.Web.Script.Serialization;
using SifizPlanning.Models;
using SifizPlanning.Util;
using SifizPlanning.Security;

namespace SifizPlanning.Controllers
{
	public class GestionErroresController : Controller
	{
		SifizPlanningEntidades db = DbCnx.getCnx();

		[Authorize(Roles = "ADMIN")]
		public ActionResult Index()
		{
			return View();
		}

		[Authorize(Roles = "ADMIN")]
		public ActionResult InformacionGeneral(string fechaDesde = "", string fechaHasta = "")
		{
			try
			{
				DateTime fInicio = DateTime.Today;
				DateTime fFin = DateTime.Today;

				if(fechaDesde != "")
				{
					fInicio = DateTime.Parse(fechaDesde);
				}
				if(fechaHasta != "")
				{
					fFin = DateTime.Parse(fechaHasta);
				}

				DateTime auxFechaFin = fFin.AddDays(1);

				var dataFBSLogError = db.FBSLogError.Where(x => x.FechaMaquina > fInicio &&
																 x.FechaMaquina < auxFechaFin
														  ).ToList();

				var tablaErrores = (
										from logs in dataFBSLogError
										group logs by logs.Empresa into gEmpresa
										select new
										{
											cliente = gEmpresa.Key,
											oficinas = gEmpresa.Select(o => o.Oficina).Distinct().Count(),
											modulos = gEmpresa.Select(m => m.NombreModulo).Distinct().Count(),
											usuarios = gEmpresa.Select(u => u.Usuario).Distinct().Count(),
											errores = gEmpresa.Count()
										}
								   ).ToList();

				var resp = new
				{
					success = true,
					tablaErrores = tablaErrores,
					totalErrores = dataFBSLogError.Count,
					fechaDesde = fInicio.ToString("dd/MM/yyyy"),
					fechaHasta = fFin.ToString("dd/MM/yyyy")
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message
				};
				return Json(resp);
			}
		}

		[Authorize(Roles = "ADMIN")]
		public ActionResult ErroresEmpresa(string empresa, string fechaDesde, string fechaHasta, int start = 0, int leng = 8)
		{
			try
			{
				DateTime fInicio = DateTime.Parse(fechaDesde);
				DateTime fFin = DateTime.Parse(fechaHasta).AddDays(1);

				var listaErrores = (from logs in db.FBSLogError
									where
										  logs.Empresa == empresa &&
										  logs.FechaMaquina >= fInicio &&
										  logs.FechaMaquina < fFin
									orderby logs.FechaMaquina descending
									select new
									{
										id = logs.Secuencial,
										oficina = logs.Oficina,
										usuario = logs.Usuario,
										hora = logs.FechaMaquina,
										modulo = logs.NombreModulo,
										ip = logs.fbsLogEquipo.Ip,
										publicacion = logs.Publicacion,
										clase = (logs.TipoError == "S") ? "danger" : "warning"
									}).Skip(start).Take(leng).ToList();

				int cant = db.FBSLogError.Where(x => x.Empresa == empresa &&
													  x.FechaMaquina >= fInicio &&
													  x.FechaMaquina < fFin).Count();

				var listaErroresF = (from log in listaErrores
									 select new
									 {
										 id = log.id,
										 oficina = log.oficina,
										 usuario = log.usuario,
										 hora = log.hora.ToString("dd/MM/yyyy HH:mm"),
										 modulo = log.modulo,
										 ip = log.ip,
										 publicacion = log.publicacion,
										 clase = log.clase
									 }).ToList();

				var resp = new
				{
					success = true,
					listaErrores = listaErroresF,
					cantErrores = cant
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message
				};
				return Json(resp);
			}
		}

		[Authorize(Roles = "ADMIN")]
		public ActionResult DarDataError(int idError)
		{
			try
			{
				var error = db.FBSLogError.Find(idError);
				if(error == null)
					throw new Exception("El Error no está registrado en el sistema");

				var dataError = new
				{
					mensaje = error.MensajeExcepcion,
					hora = error.FechaMaquina.ToString("dd/MM/yyyy HH:mm:ss"),
					user = error.Usuario,
					maquina = error.fbsLogEquipo.Nombre + "/" + error.fbsLogEquipo.Ip,
					modulo = error.NombreModulo,
					fichero = error.NombreFichero,
					clase = error.NombreClase,
					metodo = error.NombreMetodo,
					linea = error.Linea,
					columna = error.Columna,
					stackTrace = error.StackTrace
				};

				var resp = new
				{
					success = true,
					dataError = dataError
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message
				};
				return Json(resp);
			}
		}

		[Authorize(Roles = "ADMIN")]
		public ActionResult ErroresTotalesGraficoCalor()
		{
			DateTime fecha = DateTime.Today;
			DateTime fechaInicio = fecha.AddDays(-6);
			DateTime fechaFin = fecha.AddDays(1);

			try
			{
				var errores = db.FBSLogError.Where(l => l.FechaMaquina > fechaInicio && l.FechaMaquina < fechaFin).ToList();

				List<string> clientes = (from e in errores select e.Empresa).Distinct().ToList();

				List<string> listaDias = new List<string>();
				//Llenando la lista de lista de datos
				List<object> listaDatos = new List<object>();
				int x = 0;
				int cantClientes = clientes.Count;
				while(fechaInicio < fechaFin)
				{
					int y = 0;
					DateTime fechaSiguiente = fechaInicio.AddDays(1);
					while(y < cantClientes)
					{
						List<int> posicionValores = new List<int>();
						string cliente = clientes[y];
						int cant = (from e in errores
									where e.FechaMaquina >= fechaInicio &&
										  e.FechaMaquina < fechaSiguiente &&
										  e.Empresa == cliente
									select e.Secuencial
									).Count();
						posicionValores.Add(x);
						posicionValores.Add(y);
						posicionValores.Add(cant);

						listaDatos.Add(posicionValores);
						y++;
					}
					x++;
					listaDias.Add(fechaInicio.ToString("ddd dd/MM"));
					fechaInicio = fechaInicio.AddDays(1);
				}

				var resp = new
				{
					success = true,
					xDataCalor = listaDias,
					yDataCalor = clientes,
					listaDatosCalor = listaDatos
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message
				};
				return Json(resp);
			}
		}
	}
}
