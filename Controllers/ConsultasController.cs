using DocumentFormat.OpenXml.Drawing.Diagrams;
using SifizPlanning.Models;
using SifizPlanning.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SifizPlanning.Controllers
{
	public class ConsultasController : Controller
	{
		SifizPlanningEntidades db = DbCnx.getCnx();

		// GET: Consultas
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
		public ActionResult DarModulosClientes(int start, int lenght, string filtro = "", int order = 0, int asc = 1)
		{

			var s = new JavaScriptSerializer();
			var jsonObj = s.Deserialize<dynamic>(filtro);

			string filtroCliente = jsonObj["cliente"];
			string filtroModulo = jsonObj["modulo"];
			string filtroSubModulo = jsonObj["submod"];
			string filtroEstado = jsonObj["estado"];

			try
			{

				var datos = (from pmc in db.ProyectoModuloCliente
							 join mo in db.Modulo on pmc.SecuencialModulo equals mo.Secuencial
							 join es in db.EstadoEntregable on pmc.SecuencialEstadoEntregable equals es.Secuencial
							 join cl in db.Cliente on pmc.SecuencialCliente equals cl.Secuencial
							 orderby cl.Descripcion
							 where pmc.EstaActivo == 1 && mo.EstaActivo == 1 && cl.EstaActivo == 1 && es.EstaActivo == 1
							 select new
							 {

								 secuencial = pmc.Secuencial,
								 cliente = cl.Descripcion,
								 modulo = mo.Descripcion,
								 ordenModulo = mo.Ordenar,
								 estado = es.Descripcion,
								 subMod = pmc.subModulo != null ? pmc.subModulo.Descripcion : "No Asignado"

							 }).ToList();

				//Se aplican los filtros
				if(filtroCliente != "")
				{
					datos = (from d in datos
							 where d.cliente.ToString().ToUpper().Contains(filtroCliente.ToUpper())
							 select d).ToList();
				}

				if(filtroModulo != "")
				{
					datos = (from d in datos
							 where d.modulo.ToString().ToUpper().Contains(filtroModulo.ToUpper())
							 select d).ToList();
				}
				if(filtroSubModulo != "")
				{
					datos = (from d in datos
							 where d.subMod.ToString().ToUpper().Contains(filtroSubModulo.ToUpper())
							 select d).ToList();
				}
				if(filtroEstado != "")
				{
					datos = (from d in datos
							 where d.estado.ToString().ToUpper().Contains(filtroEstado.ToUpper())
							 select d).ToList();
				}


				//Se ordena
				if(order > 0)
				{
					switch(order)
					{
						case 1:

							if(asc == 1)
							{
								datos = (from d in datos
										 orderby d.cliente
										 select d).ToList();
							}
							else
							{
								datos = (from d in datos
										 orderby d.cliente descending
										 select d).ToList();
							}

							break;

						case 2:

							if(asc == 1)
							{
								datos = (from d in datos
										 orderby int.Parse(d.ordenModulo)
										 select d).ToList();
							}
							else
							{
								datos = (from d in datos
										 orderby int.Parse(d.ordenModulo) descending
										 select d).ToList();
							}

							break;

						case 3:

							if(asc == 1)
							{
								datos = (from d in datos
										 orderby d.subMod
										 select d).ToList();
							}
							else
							{
								datos = (from d in datos
										 orderby d.subMod descending
										 select d).ToList();
							}

							break;

						case 4:

							if(asc == 1)
							{
								datos = (from d in datos
										 orderby d.estado
										 select d).ToList();
							}
							else
							{
								datos = (from d in datos
										 orderby d.estado descending
										 select d).ToList();
							}

							break;

					}
				}

				var cantidad = datos.Count;

				datos = datos.Skip(start).Take(lenght).ToList();

				return Json(new
				{
					modulosClientes = datos,
					cantidadModulosClientes = cantidad,
					success = true
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}

		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, CLIENTE")]
		public ActionResult DarDatosModuloCliente(int secuencialModuloCliente)
		{
			try
			{

				var moduloCliente = (from pmc in db.ProyectoModuloCliente
									 join mo in db.Modulo on pmc.SecuencialModulo equals mo.Secuencial
									 join es in db.EstadoEntregable on pmc.SecuencialEstadoEntregable equals es.Secuencial
									 join cl in db.Cliente on pmc.SecuencialCliente equals cl.Secuencial
									 where pmc.Secuencial == secuencialModuloCliente
									 select new
									 {

										 secuencial = secuencialModuloCliente,
										 secuencialModulo = mo.Secuencial,
										 secuencialCliente = cl.Secuencial,
										 cliente = cl.Descripcion,
										 modulo = mo.Descripcion,
										 estado = es.Descripcion,
										 subModulo = pmc.SecuencialSubModulo

									 }).ToList();

				int secuencialCliente = moduloCliente[0].secuencialCliente;
				int secuencialModulo = moduloCliente[0].secuencialModulo;
				var submodulos = (from s in db.SubModulo
								  where s.SecuencialModulo == secuencialModulo
								  select new
								  {
									  s.Secuencial,
									  s.Codigo
								  }).ToList();

				var func = (from fun in db.Funcionalidad
							join fc in db.FuncionalidadCliente on fun.Secuencial equals fc.SecuencialFuncionalidad
							where fc.SecuencialCliente == secuencialCliente
							select new
							{
								id = fun.Secuencial,
								nombre = fun.Descripcion
							}).ToList();

				return Json(new
				{
					success = true,
					datos = moduloCliente,
					funcionalidades = func,
					submodulos = submodulos
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
		public ActionResult DarEstadosModuloCliente()
		{

			var datos = (from es in db.EstadoEntregable
						 select new
						 {
							 nombre = es.Descripcion
						 }).ToList();

			try
			{
				return Json(new
				{
					estados = datos,
					success = true
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}

		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, CLIENTE")]
		public ActionResult DarFuncionalidadesExtra(string[] funcionalidades, string modulo)
		{
			try
			{

				var secuencialModulo = (from mo in db.Modulo
										where mo.Descripcion == modulo
										select mo.Secuencial).ToList()[0];

				var funcionalidadesCompletas = (from fc in db.Funcionalidad
												where fc.EstaActiva == 1 && fc.SecuencialModulo == secuencialModulo
												select new
												{
													id = fc.Secuencial,
													nombre = fc.Descripcion
												}).ToList();
				if(funcionalidades != null)
				{
					if(funcionalidades.Length == funcionalidadesCompletas.Count)
					{
						funcionalidadesCompletas.RemoveAll(x => true);
					}
					else
					{
						for(int i = 0; i < funcionalidadesCompletas.Count; i++)
						{
							for(int j = 0; j < funcionalidades.Length; j++)
							{
								if(i < funcionalidadesCompletas.Count && funcionalidadesCompletas.Count > 0 && funcionalidadesCompletas[i].id == int.Parse(funcionalidades[j]))
								{
									funcionalidadesCompletas.Remove(funcionalidadesCompletas[i]);
									j = 0;
								}
							}
						}
					}
				}

				return Json(new
				{
					success = true,
					funcionalidadesExtra = funcionalidadesCompletas
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult GuardarDatos(int secuencialModuloCliente, string estado, string funcionalidades, int subModulo)
		{
			try
			{

				var funcionalidadesArray = funcionalidades.Split('/');
				funcionalidadesArray = funcionalidadesArray.Skip(1).ToArray();

				var secuencialEstado = (from es in db.EstadoEntregable
										where es.Descripcion == estado
										select es.Secuencial).ToList()[0];

				var secuencialCliente = (from pmc in db.ProyectoModuloCliente
										 where pmc.Secuencial == secuencialModuloCliente
										 select pmc.SecuencialCliente).ToList()[0];

				ProyectoModuloCliente proyectoModuloCliente = db.ProyectoModuloCliente.Find(secuencialModuloCliente);
				if(subModulo != 0)
				{
					proyectoModuloCliente.SecuencialSubModulo = subModulo;
					db.SaveChanges();
				}

				Type typeAccesoDatos = db.GetType();
				PropertyInfo propertyTabla = typeAccesoDatos.GetProperty("ProyectoModuloCliente");
				MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
				object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
				Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

				Type typeNewObj = Type.GetType("SifizPlanning.Models.ProyectoModuloCliente");
				object newObj = null;

				MethodInfo metodoFind = typePropertyTabla.GetMethod("Find");
				object[] pId = new object[1] { new object[1] { secuencialModuloCliente } };
				newObj = metodoFind.Invoke(dbSetTable, pId);

				typeNewObj.GetProperty("SecuencialEstadoEntregable").SetValue(newObj, secuencialEstado);

				db.SaveChanges();

				typeAccesoDatos = db.GetType();
				propertyTabla = typeAccesoDatos.GetProperty("FuncionalidadCliente");
				methodPropertyTabla = propertyTabla.GetMethod;
				dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
				typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

				var funcionalidadesEliminar = db.FuncionalidadCliente.Where(x => x.SecuencialCliente == secuencialCliente).ToList();

				for(int i = 0; i < funcionalidadesEliminar.Count; i++)
				{
					MethodInfo metodoRemove = typePropertyTabla.GetMethod("Remove");
					newObj = metodoRemove.Invoke(dbSetTable, new object[1] { funcionalidadesEliminar[i] });
				}
				db.SaveChanges();


				for(int i = 0; i < funcionalidadesArray.Length; i++)
				{
					var datosFunc = funcionalidadesArray[i].Split(':');

					typeNewObj = Type.GetType("SifizPlanning.Models.FuncionalidadCliente");
					newObj = Activator.CreateInstance(typeNewObj);

					typeNewObj.GetProperty("SecuencialCliente").SetValue(newObj, secuencialCliente);
					typeNewObj.GetProperty("SecuencialFuncionalidad").SetValue(newObj, int.Parse(datosFunc[0]));
					typeNewObj.GetProperty("EstaActivo").SetValue(newObj, decimal.Parse("1"));
					typeNewObj.GetProperty("NumeroVerificador").SetValue(newObj, 0);

					MethodInfo metodoAdd = typePropertyTabla.GetMethod("Add");
					metodoAdd.Invoke(dbSetTable, new object[] { newObj });

				}

				db.SaveChanges();

				return Json(new
				{
					success = true
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
		public ActionResult CargarGarantiaTickets(int start = 0, int lenght = 10, string filtro = "", int order = 0, int asc = 1, bool todos = false)
		{
			var s = new JavaScriptSerializer();
			var jsonObj = s.Deserialize<dynamic>(filtro);

			string filtroNoTicket = jsonObj["noTicket"].ToString();
			string filtroCliente = jsonObj["cliente"];
			string filtroAsunto = jsonObj["asunto"];
			string filtroAsignado = jsonObj["asignado"];
			string filtroFecha = jsonObj["fecha"];
			string filtroFechaVencimiento = jsonObj["fechaVencimiento"];
			string filtroDiasRestantes = jsonObj["diasRestantes"].ToString();

			var ticketsParcial = (from t in db.Ticket
								  join
									  et in db.EstadoTicket on t.SecuencialEstadoTicket equals et.Secuencial
								  join
									  pc in db.Persona_Cliente on t.persona_cliente equals pc
								  orderby t.Secuencial ascending
								  where et.Codigo == "CERRADO"
								  select new
								  {
									  numero = t.Secuencial,
									  cliente = pc.cliente.Descripcion,
									  asunto = t.Asunto,
									  asignado = "",
									  fecha = (from thi in t.ticketHistorico
											   where thi.estadoTicket.Codigo == "CERRADO" && db.TicketHistorico.Where(h => h.Version == thi.Version - 1 && h.SecuencialTicket == t.Secuencial).FirstOrDefault().estadoTicket.Codigo != "CERRADO"
											   orderby thi.Version descending
											   select thi.FechaOperacion
											 ).FirstOrDefault(),
									  fechaVencimiento = DateTime.Now,
									  diasRestantes = 0,
									  diasGarantia = t.DiasGarantia ?? 30
								  }).ToList();

			var tickets = ticketsParcial;

			var asignados = (from t in db.Ticket
							 join ttar in db.TicketTarea on t.Secuencial equals ttar.SecuencialTicket
							 join tar in db.Tarea on ttar.SecuencialTarea equals tar.Secuencial
							 join c in db.Colaborador on tar.SecuencialColaborador equals c.Secuencial
							 join p in db.Persona on c.SecuencialPersona equals p.Secuencial
							 orderby tar.FechaInicio descending
							 select new
							 {
								 nombre = p.Nombre1 + " " + p.Apellido1,
								 numero = t.Secuencial
							 }).ToList();

			if(todos == false)
			{
				//Se crea la variable tickets con su colaborador asignado
				var ticketsSinAsignado = (from t in ticketsParcial
										  where (new DateTime(t.fecha.Year, t.fecha.Month, t.fecha.Day).AddDays(30) - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Days <= 30 && 0 <= (new DateTime(t.fecha.Year, t.fecha.Month, t.fecha.Day).AddDays(30) - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Days
										  select new
										  {
											  numero = t.numero,
											  cliente = t.cliente,
											  asunto = t.asunto,
											  asignado = t.asignado,
											  fecha = t.fecha,
											  fechaVencimiento = t.fecha.AddDays(t.diasGarantia),
											  diasRestantes = (new DateTime(t.fecha.Year, t.fecha.Month, t.fecha.Day).AddDays(t.diasGarantia) - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Days,
											  diasGarantia = t.diasGarantia
										  }).ToList();

				tickets = (from t in ticketsSinAsignado
						   select new
						   {
							   numero = t.numero,
							   cliente = t.cliente,
							   asunto = t.asunto,
							   asignado = (
							   db.TicketTarea.Where(x => x.SecuencialTicket == t.numero && x.EstaActiva == 1).Count() > 0
						  ) ?
							  asignados.Where(x => x.numero.ToString().Equals(t.numero.ToString())).FirstOrDefault().nombre.ToString()
							: "NO ASIGNADO",
							   fecha = t.fecha,
							   fechaVencimiento = t.fechaVencimiento,
							   diasRestantes = t.diasRestantes,
							   diasGarantia = t.diasGarantia
						   }).ToList();
			}
			else
			{
				tickets = (from t in ticketsParcial
						   select new
						   {
							   numero = t.numero,
							   cliente = t.cliente,
							   asunto = t.asunto,
							   asignado = (
							   db.TicketTarea.Where(x => x.SecuencialTicket == t.numero && x.EstaActiva == 1).Count() > 0
						  ) ?
							  asignados.Where(x => x.numero.ToString().Equals(t.numero.ToString())).FirstOrDefault().nombre.ToString()
							: "NO ASIGNADO",
							   fecha = t.fecha,
							   fechaVencimiento = t.fecha.AddDays(t.diasGarantia),
							   diasRestantes = (new DateTime(t.fecha.Year, t.fecha.Month, t.fecha.Day) - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Days + t.diasGarantia,
							   diasGarantia = t.diasGarantia
						   }).ToList();
			}

			//Aplicando los filtros
			if(filtroNoTicket != "")
			{
				tickets = (from t in tickets
						   where t.numero.ToString().PadLeft(6, '0').Contains(filtroNoTicket)
						   select t).ToList();
			}
			if(filtroCliente != "")
			{
				tickets = (from t in tickets
						   where t.cliente.ToString().ToLower().Contains(filtroCliente.ToLower())
						   select t).ToList();
			}
			if(filtroAsunto != "")
			{
				tickets = (from t in tickets
						   where t.asunto.ToString().ToLower().Contains(filtroAsunto.ToLower())
						   select t).ToList();
			}
			if(filtroAsignado != "")
			{

				tickets = (from t in tickets
						   where t.asignado.ToString().ToUpper().Contains(filtroAsignado.ToUpper())
						   select t).ToList();
			}
			if(filtroFecha != "")
			{
				tickets = (from t in tickets
						   where t.fecha.ToString("dd/MM/yyyy").Contains(filtroFecha)
						   select t).ToList();
			}
			if(filtroFechaVencimiento != "")
			{
				tickets = (from t in tickets
						   where t.fechaVencimiento.ToString("dd/MM/yyyy").Contains(filtroFechaVencimiento)
						   select t).ToList();
			}
			if(filtroDiasRestantes != "")
			{
				tickets = (from t in tickets
						   where t.diasRestantes.ToString().ToUpper().Equals(filtroDiasRestantes.ToUpper())
						   select t).ToList();
			}
			//Se Ordena
			if(order > 0)
			{
				switch(order)
				{
					case 1:

						if(asc == 1)
						{
							tickets = (from t in tickets
									   orderby t.numero
									   select t).ToList();
						}
						else
						{
							tickets = (from t in tickets
									   orderby t.numero descending
									   select t).ToList();
						}

						break;

					case 2:

						if(asc == 1)
						{
							tickets = (from t in tickets
									   orderby t.cliente
									   select t).ToList();
						}
						else
						{
							tickets = (from t in tickets
									   orderby t.cliente descending
									   select t).ToList();
						}

						break;

					case 3:

						if(asc == 1)
						{
							tickets = (from t in tickets
									   orderby t.asunto
									   select t).ToList();
						}
						else
						{
							tickets = (from t in tickets
									   orderby t.asunto descending
									   select t).ToList();
						}

						break;

					case 4:

						if(asc == 1)
						{
							tickets = (from t in tickets
									   orderby t.asignado
									   select t).ToList();
						}
						else
						{
							tickets = (from t in tickets
									   orderby t.asignado descending
									   select t).ToList();
						}

						break;

					case 5:

						if(asc == 1)
						{
							tickets = (from t in tickets
									   orderby t.fecha
									   select t).ToList();
						}
						else
						{
							tickets = (from t in tickets
									   orderby t.fecha descending
									   select t).ToList();
						}

						break;


					case 6:

						if(asc == 1)
						{
							tickets = (from t in tickets
									   orderby t.fechaVencimiento
									   select t).ToList();
						}
						else
						{
							tickets = (from t in tickets
									   orderby t.fechaVencimiento descending
									   select t).ToList();
						}

						break;

					case 7:

						if(asc == 1)
						{
							tickets = (from t in tickets
									   orderby t.diasRestantes
									   select t).ToList();
						}
						else
						{
							tickets = (from t in tickets
									   orderby t.diasRestantes descending
									   select t).ToList();
						}

						break;
				}
			}

			int totalTickets = tickets.Count();
			tickets = tickets.Skip(start).Take(lenght).ToList();

			var resp = new
			{
				success = true,
				tickets = tickets,
				totalTickets = totalTickets
			};
			return Json(resp);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
		public ActionResult DarOfertasTickets(string filtro = "")
		{
			try
			{
				var fechaHoy = DateTime.Now.Date;
				var fechaPermisible = fechaHoy.AddDays(3);

				var ofertas = (from o in db.Ofertas
							   join c in db.Cliente on o.SecuencialCliente equals c.Secuencial
							   join col in db.Colaborador on o.SecuencialColaborador equals col.Secuencial
							   join p in db.Persona on col.SecuencialPersona equals p.Secuencial
							   orderby o.FechaRegistro descending
							   select new
							   {
								   id = o.Secuencial,
								   FechaRegistro = o.FechaRegistro.Year != 1 ? o.FechaRegistro : (DateTime?)null,
								   FechaProduccion = o.FechaProduccion.HasValue ? o.FechaProduccion.Value.Year != 1 ? o.FechaProduccion.Value : (DateTime?)null : (DateTime?)null,
								   FechaDisponibilidad = o.FechaDisponibilidad.Year != 1 ? o.FechaDisponibilidad : (DateTime?)null,
								   Detalle = o.Detalle,
								   semaforo = o.FechaDisponibilidad < fechaHoy ? "ROJO" : o.FechaDisponibilidad <= fechaPermisible ? "AMARILLO" : "VERDE",
								   HorasEstimacion = o.HorasEstimacion,
								   cliente = c.Secuencial != 78 ? new
								   {
									   id = c.Secuencial,
									   nombre = c.Descripcion
								   } : new
								   {
									   id = 0,
									   nombre = "NO ASIGNADO"
								   },
								   colaborador = col.Secuencial != 2122 ? new
								   {
									   id = col.Secuencial,
									   nombre = p.Nombre1 + " " + p.Apellido1
								   } : new
								   {
									   id = 0,
									   nombre = "NO ASIGNADO"
								   },
								   editable = false,
								   adjunto = o.Adjunto,
							   }).ToList();

				if(filtro != "")
				{
					ofertas = ofertas.Where(s =>
											s.cliente.nombre.ToString().ToUpper().Contains(filtro.ToUpper()) ||
											s.colaborador.nombre.ToString().ToUpper().Contains(filtro.ToUpper()) ||
											s.Detalle.ToString().ToUpper().Contains(filtro.ToUpper()) ||
											s.FechaDisponibilidad.ToString().ToUpper().Contains(filtro.ToUpper()) ||
											s.FechaProduccion.ToString().ToUpper().Contains(filtro.ToUpper()) ||
											s.FechaRegistro.ToString().ToUpper().Contains(filtro.ToUpper()) ||
											s.HorasEstimacion.ToString().ToUpper().Contains(filtro.ToUpper())
										).ToList();
				}

				var resp = new
				{
					success = true,
					ofertas = ofertas
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, GESTOR")]
		public ActionResult AgregarOfertaTickets(string fechaRegistro, string fechaProduccion, string fechaDisponibilidad, string detalle, int? horasEstimacion, int? cliente, int? colaborador, HttpPostedFileBase adjunto = null)
		{
			try
			{

				var adjuntoUrl = "";
				if(adjunto != null)
				{
					string path = Path.Combine(Server.MapPath("~/Web/resources/ofertas"), adjunto.FileName);
					adjunto.SaveAs(path);

					adjuntoUrl = "/resources/ofertas" + "/" + adjunto.FileName;
				}

				Ofertas oferta = new Ofertas();
				oferta.FechaRegistro = DateTime.Today; //fechaRegistro != null ? DateTime.Parse(fechaRegistro) : new DateTime(0001 / 01 / 01);
				oferta.FechaProduccion = DateTime.Today; //fechaProduccion != null ? DateTime.Parse(fechaProduccion) : new DateTime(0001 / 01 / 01);
				oferta.FechaDisponibilidad = DateTime.Today; //fechaDisponibilidad != null ? DateTime.Parse(fechaDisponibilidad) : new DateTime(0001 / 01 / 01);
				oferta.Detalle = detalle;
				oferta.HorasEstimacion = horasEstimacion ?? 0;
				oferta.cliente = cliente != null ? db.Cliente.Find(cliente) : db.Cliente.Find(78);
				oferta.colaborador = colaborador != null ? db.Colaborador.Find(colaborador) : db.Colaborador.Find(2122);
				oferta.Adjunto = adjuntoUrl;

				db.Ofertas.Add(oferta);
				db.SaveChanges();

				var resp = new
				{
					success = true,
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, GESTOR")]
		public ActionResult EditarOfertaTickets(int ID, string FechaRegistro, string FechaProduccion, string FechaDisponibilidad, string Detalle, int HorasEstimacion, int cliente, int colaborador, HttpPostedFileBase adjunto = null)
		{
			try
			{

				Ofertas oferta = db.Ofertas.FirstOrDefault(s => s.Secuencial == ID);
				if(oferta != null)
				{
					oferta.FechaRegistro = DateTime.Today; //DateTime.Parse(FechaRegistro);
					oferta.FechaProduccion = DateTime.Today;

					/*DateTime fechaProduccion;
                    if (DateTime.TryParse(FechaProduccion, out fechaProduccion))
                    {
                        oferta.FechaProduccion = fechaProduccion;
                    }
                    else
                    {
                        oferta.FechaProduccion = null;
                    }*/

					var adjuntoUrl = "";
					if(adjunto != null)
					{
						string path = Path.Combine(Server.MapPath("~/Web/resources/ofertas"), adjunto.FileName);
						adjunto.SaveAs(path);

						adjuntoUrl = "/resources/ofertas" + "/" + adjunto.FileName;
						oferta.Adjunto = adjuntoUrl;
					}

					oferta.FechaDisponibilidad = DateTime.Today; //DateTime.Parse(FechaDisponibilidad);
					oferta.Detalle = Detalle;
					oferta.HorasEstimacion = HorasEstimacion;
					oferta.cliente = db.Cliente.Find(cliente);
					oferta.colaborador = db.Colaborador.Find(colaborador);
				}
				db.SaveChanges();

				var resp = new
				{
					success = true,
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}
		}


		[HttpPost]
		[Authorize(Roles = "ADMIN, GESTOR")]
		public ActionResult EliminarOfertaTickets(int ID)
		{
			try
			{
				Ofertas oferta = db.Ofertas.Find(ID);
				db.Ofertas.Remove(oferta);
				db.SaveChanges();

				var resp = new
				{
					success = true,
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}
		}
	}
}