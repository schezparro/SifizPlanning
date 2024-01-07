using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using System.Reflection;
using System.Data.Entity.Core.Objects;
using System.Web.Script.Serialization;
using SifizPlanning.Models;
using SifizPlanning.Util;
using SifizPlanning.Security;
using System.Globalization;
using System.Text.RegularExpressions;
using SpreadsheetLight;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Web.Hosting;
using SifizPlanning.Models.ViewModel;
using DocumentFormat.OpenXml.Bibliography;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Data.Entity.Validation;
using System.Web.Mvc.Html;

namespace SifizPlanning.Controllers
{
	public class UserController : Controller
	{
		SifizPlanningEntidades db = DbCnx.getCnx();
		//
		// GET: /User/

		public ActionResult Index()
		{
			return View();
		}

		//GESTION DE TAREAS POR EL USUARIO
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult Sistema()
		{
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN, RRHH")]
		public ActionResult DarUltimoLunes()
		{
			DateTime hoy = DateTime.Today;
			DayOfWeek diaSemana = hoy.DayOfWeek;
			DateTime lunes = hoy;
			if((int)diaSemana == 0)//Domingo
			{
				lunes = hoy.AddDays(-6);
			}
			else
			{
				long tiempo = (int)diaSemana;
				TimeSpan time = new TimeSpan(tiempo * 864000000000);
				DateTime domingo = hoy.Subtract(time);
				lunes = domingo.AddDays(1);
			}

			var resp = new
			{
				success = true,
				lunes = lunes.ToString("dd/MM/yyyy"),
				hoy = hoy.ToString("dd/MM/yyyy")
			};
			return Json(resp);
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarTareasUsuario(string fechaLunes = "", int semanas = 1, string json = "", bool coordinados = false)
		{
			DateTime lunes = DateTime.Today;
			if(fechaLunes != "")
			{
				string[] fechas = fechaLunes.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				lunes = new System.DateTime(anno, mes, dia);
			}
			else
			{
				DateTime hoy = DateTime.Today;
				DayOfWeek diaSemana = hoy.DayOfWeek;
				if((int)diaSemana == 0)//Domingo
				{
					lunes = hoy.AddDays(-6);
				}
				else
				{
					long tiempo = (int)diaSemana;
					TimeSpan time = new TimeSpan(tiempo * 864000000000);
					DateTime domingo = hoy.Subtract(time);
					lunes = domingo.AddDays(1);
				}
			}

			List<int> idColaboradores = new List<int>();
			List<int> idClientes = new List<int>();
			List<int> idEstados = new List<int>();
			List<int> idLugares = new List<int>();
			List<int> idModulos = new List<int>();
			List<int> idSedes = new List<int>();
			if(json != "")
			{
				var s = new JavaScriptSerializer();
				var jsonObj = s.Deserialize<dynamic>(json);
				for(int i = 0; i < jsonObj["colaboradores"].Length; i++)
				{
					idColaboradores.Add(int.Parse(jsonObj["colaboradores"][i]["id"]));
				}
				for(int i = 0; i < jsonObj["clientes"].Length; i++)
				{
					idClientes.Add(int.Parse(jsonObj["clientes"][i]["id"]));
				}
				for(int i = 0; i < jsonObj["estadoTarea"].Length; i++)
				{
					idEstados.Add(int.Parse(jsonObj["estadoTarea"][i]["id"]));
				}
				for(int i = 0; i < jsonObj["lugarTarea"].Length; i++)
				{
					idLugares.Add(int.Parse(jsonObj["lugarTarea"][i]["id"]));
				}
				for(int i = 0; i < jsonObj["modulo"].Length; i++)
				{
					idModulos.Add(int.Parse(jsonObj["modulo"][i]["id"]));
				}
				for(int i = 0; i < jsonObj["sede"].Length; i++)
				{
					idSedes.Add(int.Parse(jsonObj["sede"][i]["id"]));
				}
			}

			string emailUser = User.Identity.Name;
			Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
			Persona persona = user.persona;
			Colaborador colaborador = db.Colaborador.FirstOrDefault(x => x.persona.Secuencial == persona.Secuencial);

			//Buscando los que se van a mostrar en cada caso
			List<int> colabSubordinados = new List<int>();
			if(coordinados == false)
			{
				colabSubordinados = (from cji in db.Colaborador_JefeInmediato
									 where cji.SecuencialJefeInmediato == colaborador.Secuencial
									 select cji.SecuencialColaborador
									).ToList();
			}
			else
			{
				colabSubordinados = idColaboradores;
			}

			DateTime fechaFin = lunes.AddDays((7 * semanas));
			var datos = (from t in db.Tarea
						 join
							 c in db.Colaborador on t.colaborador equals c
						 join
							 p in db.Persona on c.persona equals p
						 join
							 u in db.Usuario on p.Secuencial equals u.SecuencialPersona
						 join
							 f in db.FotoColaborador on c.Secuencial equals f.SecuencialColaborador
						 join
							 s in db.Sede on c.sede equals s
						 join
							 m in db.Modulo on t.modulo equals m
						 join
							 cl in db.Cliente on t.cliente equals cl
						 join
							 a in db.Actividad on t.actividad equals a
						 join
							 e in db.EstadoTarea on t.estadoTarea equals e
						 join
							 l in db.LugarTarea on t.lugarTarea equals l

						 where u.EstaActivo == 1 &&
							   (c.Secuencial == colaborador.Secuencial || colabSubordinados.Contains(c.Secuencial)) &&
							   t.FechaInicio >= lunes && t.FechaInicio < fechaFin && //Entre las dos fechas
							   t.SecuencialEstadoTarea != 4//Es cuando están anuladas
						 orderby t.FechaInicio, p.Nombre1, p.Apellido1
						 select new
						 {
							 idColaborador = c.Secuencial,
							 nombre = p.Nombre1 + " " + p.Apellido1,
							 email = u.Email.ToUpper(),
							 sede = s.Codigo,
							 url = f.Url,
							 idTarea = t.Secuencial,
							 sdetalle = t.Detalle.Substring(0, 20) + "...",
							 detalle = t.Detalle,
							 finicio = t.FechaInicio,
							 ffin = t.FechaFin,
							 modulo = m.Codigo,
							 dModulo = m.Descripcion.ToUpper(),
							 idModulo = m.Secuencial,
							 cliente = cl.Codigo,
							 idCliente = cl.Secuencial,
							 dCliente = cl.Descripcion.ToUpper(),
							 actividad = a.Codigo,
							 dActividad = a.Descripcion.ToUpper(),
							 estado = e.Codigo,
							 idEstado = e.Secuencial,
							 lugar = l.Codigo,
							 dLugar = l.Descripcion.ToUpper(),
							 idLugar = l.Secuencial,
							 clase = (t.SecuencialEstadoTarea == 1 ? "new" : (t.SecuencialEstadoTarea == 2) ? "dev" : (t.SecuencialEstadoTarea == 3) ? "finish" : (t.SecuencialEstadoTarea == 5) ? "pause" : (t.SecuencialEstadoTarea == 6) ? "preassigned" : "no-concluida"),
							 idColaboradorCoordinador = (from tc in db.Tarea_Coordinador
														 where tc.SecuencialTarea == t.Secuencial && tc.EstaActivo == 1
														 select tc.SecuencialColaborador).FirstOrDefault(),
							 coordinador = (from tc in db.Tarea_Coordinador
											join
												co in db.Colaborador on tc.colaborador equals co
											join
												pe in db.Persona on co.persona equals pe
											where tc.SecuencialTarea == t.Secuencial && tc.EstaActivo == 1
											select (pe.Nombre1 + " " + pe.Apellido1)).FirstOrDefault()
						 }).ToList();

			var trabajadores = (from t in db.Colaborador
								join p in db.Persona on t.persona equals p
								join f in db.FotoColaborador on t.Secuencial equals f.SecuencialColaborador
								join s in db.Sede on t.sede equals s
								join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
								where (t.Secuencial == colaborador.Secuencial || colabSubordinados.Contains(t.Secuencial)) &&
									  u.EstaActivo == 1 &&
									  (idSedes.Count == 0 || idSedes.Contains(s.Secuencial)) &&
									  (idColaboradores.Count == 0 || idColaboradores.Contains(t.Secuencial))
								orderby u.Email
								select new
								{
									id = t.Secuencial,
									nombre = p.Nombre1 + " " + p.Apellido1,
									email = u.Email.ToUpper(),
									idSede = s.Secuencial,
									sede = s.Codigo,
									dSede = s.Descripcion.ToUpper(),
									url = f.Url
								}).ToList();

			if(idSedes.Count() > 0)
			{
				trabajadores = trabajadores.Where(x => idSedes.Contains(x.idSede)).ToList();
			}

			if(idColaboradores.Count() > 0)
			{
				trabajadores = trabajadores.Where(x => idColaboradores.Contains(x.id)).ToList();
				datos = datos.Where(x => idColaboradores.Contains(x.idColaborador)).ToList();
			}

			//Buscando los colaboradores independientes
			List<int> idColaboradoresFiltrados = trabajadores.Select(x => x.id).ToList();
			//Buscando las vacaciones en la fecha para cada uno de los trabajadores           
			var vacaciones = (from vac in db.Vacaciones
							  where vac.Fecha >= lunes && vac.Fecha <= fechaFin &&
								   idColaboradoresFiltrados.Contains(vac.SecuencialColaborador)
							  select new
							  {
								  id = vac.Secuencial,
								  fecha = vac.Fecha,
								  idColaborador = vac.SecuencialColaborador
							  }).ToList();

			//Permisos de todos los colaboradores
			var permisos = (from per in db.Permiso
							where per.FechaInicio >= lunes && per.FechaInicio < fechaFin &&
								  per.SecuencialEstadoPermiso != 3 && per.SecuencialEstadoPermiso != 4 && idColaboradoresFiltrados.Contains(per.SecuencialColaborador)
							select new
							{
								id = per.Secuencial,
								idColaborador = per.SecuencialColaborador,
								smotivo = per.Motivo.Substring(0, 20) + "...",
								motivo = per.Motivo,
								finicio = per.FechaInicio,
								ffin = per.FechaFin,
								clase = (per.SecuencialEstadoPermiso == 1) ? "permiso-solicitado" : "permiso-aprobado"
							}).ToList();
			//Buscando los dias feriados en en intervalo de tiempo
			List<DateTime> diasFeriados = db.DiaInactivo.Where(x => x.Fecha >= lunes && x.Fecha < fechaFin && x.EstaActivo == 1).Select(x => x.Fecha).ToList<DateTime>();

			List<Object> tareasProgramadores = new List<Object>();
			Object trabUser = null;
			int cant = trabajadores.Count();
			for(int i = 0; i < cant; i++)
			{
				int idTrabajador = trabajadores[i].id;
				List<Object> tareasPorDia = new List<Object>();
				int countTareas = 0;
				for(int j = 0; j < 7 * semanas; j++)//son 7 Días los de la semana
				{
					DateTime fecha = lunes.AddDays(j);
					DateTime fechaDespues = lunes.AddDays(j + 1);

					if(coordinados == false)
					{
						List<DataTarea> tareas = new List<DataTarea>();
						tareas = (from d in datos
								  where d.idColaborador == idTrabajador &&
										d.finicio >= fecha && d.finicio < fechaDespues
								  select new DataTarea
								  {
									  id = d.idTarea,
									  sdetalle = d.sdetalle,
									  detalle = d.detalle,
									  dateFechaInicio = d.finicio,
									  finicio = d.finicio.ToString("t"),
									  ffin = d.ffin.ToString("t"),
									  horas = Utiles.CalcularHorasTarea(d.finicio, d.ffin),
									  modulo = d.modulo,
									  dModulo = d.dModulo,
									  idModulo = d.idModulo,
									  cliente = d.cliente,
									  idCliente = d.idCliente,
									  dCliente = d.dCliente,
									  actividad = d.actividad,
									  dActividad = d.dActividad,
									  estado = d.estado,
									  idEstado = d.idEstado,
									  lugar = d.lugar,
									  dLugar = d.dLugar,
									  idLugar = d.idLugar,
									  clase = d.clase,
									  coordinador = d.coordinador,
									  tipo = "t"
								  }).ToList<DataTarea>();

						//Sobre los permisos
						List<DataTarea> lPermisos = new List<DataTarea>();
						lPermisos = (from per in permisos
									 where per.idColaborador == idTrabajador &&
                                           per.finicio <= fecha && per.finicio < fechaDespues
                                     select new DataTarea
									 {
										 id = per.id,
										 sdetalle = per.smotivo,
										 detalle = per.motivo,
										 dateFechaInicio = per.finicio,
										 finicio = per.finicio.ToString("t"),
										 ffin = per.ffin.ToString("t"),
										 horas = Utiles.CalcularHorasTarea(per.finicio, per.ffin),
										 modulo = "",
										 dModulo = "",
										 idModulo = 0,
										 cliente = "PERMISO",
										 idCliente = 0,
										 dCliente = "PERMISO",
										 actividad = "",
										 dActividad = "",
										 estado = "",
										 idEstado = 0,
										 lugar = "",
										 dLugar = "",
										 idLugar = 0,
										 clase = per.clase,
										 coordinador = null,
										 tipo = "p"
									 }).ToList<DataTarea>();
						tareas.AddRange(lPermisos);

						if(idClientes.Count() > 0)
						{
							tareas = tareas.Where(x => idClientes.Contains(x.idCliente)).ToList();
						}
						if(idEstados.Count() > 0)
						{
							tareas = tareas.Where(x => idEstados.Contains(x.idEstado)).ToList();
						}
						if(idLugares.Count() > 0)
						{
							tareas = tareas.Where(x => idLugares.Contains(x.idLugar)).ToList();
						}
						if(idModulos.Count() > 0)
						{
							tareas = tareas.Where(x => idModulos.Contains(x.idModulo)).ToList();
						}
						//Ordenando las tareas                        
						tareas.Sort();

						countTareas += tareas.Count();

						string claseDia = "dia-normal";
						long diaSemana = (int)fecha.DayOfWeek;
						int hayVacaciones = vacaciones.Where(x => x.idColaborador == idTrabajador &&
														x.fecha == fecha).Count();
						if(hayVacaciones > 0)
						{
							claseDia = "dia-vacaciones";
						}
						else if(diasFeriados.Contains(fecha.Date))
						{
							claseDia = "dia-feriado";
						}
						else if(fecha == DateTime.Today)
						{
							claseDia = "dia-hoy";
						}
						else if(diaSemana == 0 || diaSemana == 6)
						{
							claseDia = "fin-semana";
						}

						tareasPorDia.Add(
							new
							{
								tareas = tareas,
								claseDia = claseDia
							}
						);
					}
					else
					{
						List<DataTarea> tareas = new List<DataTarea>();
						tareas = (from d in datos
								  where d.idColaborador == idTrabajador &&
									  (d.idColaborador == colaborador.Secuencial || d.idColaboradorCoordinador == colaborador.Secuencial) &&
									  d.finicio >= fecha && d.finicio < fechaDespues
								  select new DataTarea
								  {
									  id = d.idTarea,
									  sdetalle = d.sdetalle,
									  detalle = d.detalle,
									  dateFechaInicio = d.finicio,
									  finicio = d.finicio.ToString("t"),
									  ffin = d.ffin.ToString("t"),
									  horas = Utiles.CalcularHorasTarea(d.finicio, d.ffin),
									  modulo = d.modulo,
									  dModulo = d.dModulo,
									  idModulo = d.idModulo,
									  cliente = d.cliente,
									  idCliente = d.idCliente,
									  dCliente = d.dCliente,
									  actividad = d.actividad,
									  dActividad = d.dActividad,
									  estado = d.estado,
									  idEstado = d.idEstado,
									  lugar = d.lugar,
									  dLugar = d.dLugar,
									  idLugar = d.idLugar,
									  clase = d.clase,
									  coordinador = d.coordinador,
									  tipo = "t"
								  }).ToList<DataTarea>();

						//Sobre los permisos
						List<DataTarea> lPermisos = new List<DataTarea>();
						lPermisos = (from per in permisos
									 where per.idColaborador == idTrabajador &&
										   per.finicio >= fecha && per.finicio < fechaDespues
									 select new DataTarea
									 {
										 id = per.id,
										 sdetalle = per.smotivo,
										 detalle = per.motivo,
										 dateFechaInicio = per.finicio,
										 finicio = per.finicio.ToString("t"),
										 ffin = per.ffin.ToString("t"),
										 horas = Utiles.CalcularHorasTarea(per.finicio, per.ffin),
										 modulo = "",
										 dModulo = "",
										 idModulo = 0,
										 cliente = "PERMISO",
										 idCliente = 0,
										 dCliente = "PERMISO",
										 actividad = "",
										 dActividad = "",
										 estado = "",
										 idEstado = 0,
										 lugar = "",
										 dLugar = "",
										 idLugar = 0,
										 clase = per.clase,
										 coordinador = null,
										 tipo = "p"
									 }).ToList<DataTarea>();
						tareas.AddRange(lPermisos);

						if(idClientes.Count() > 0)
						{
							tareas = tareas.Where(x => idClientes.Contains(x.idCliente)).ToList();
						}
						if(idEstados.Count() > 0)
						{
							tareas = tareas.Where(x => idEstados.Contains(x.idEstado)).ToList();
						}
						if(idLugares.Count() > 0)
						{
							tareas = tareas.Where(x => idLugares.Contains(x.idLugar)).ToList();
						}
						if(idModulos.Count() > 0)
						{
							tareas = tareas.Where(x => idModulos.Contains(x.idModulo)).ToList();
						}
						//Ordenando las tareas                        
						tareas.Sort();

						countTareas += tareas.Count();

						string claseDia = "dia-normal";
						long diaSemana = (int)fecha.DayOfWeek;
						int hayVacaciones = vacaciones.Where(x => x.idColaborador == idTrabajador &&
														x.fecha == fecha).Count();
						if(hayVacaciones > 0)
						{
							claseDia = "dia-vacaciones";
						}
						else if(diasFeriados.Contains(fecha.Date))
						{
							claseDia = "dia-feriado";
						}
						else if(fecha == DateTime.Today)
						{
							claseDia = "dia-hoy";
						}
						else if(diaSemana == 0 || diaSemana == 6)
						{
							claseDia = "fin-semana";
						}

						tareasPorDia.Add(
							new
							{
								tareas = tareas,
								claseDia = claseDia
							}
						);
					}
				}

				if(json == "" || countTareas > 0 || idColaboradores.Contains(idTrabajador) || idSedes.Contains(trabajadores[i].idSede))
				{
					var trab = new
					{
						trab = trabajadores[i],
						tareasPorDia = tareasPorDia
					};

					if(trab.trab.id == colaborador.Secuencial)//Es el usuario
					{
						trabUser = trab;
					}
					else
					{
						tareasProgramadores.Add(trab);
					}
				}
			}

			if(trabUser != null)
				tareasProgramadores.Insert(0, trabUser);//Se pone el colaborador de primero

			var resp = new
			{
				success = true,
				trabajadores = tareasProgramadores
			};
			return Json(resp);
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult ActualizarTareaUsuario(int idTarea, int estado, bool publicar = false)
		{
			try
			{

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Tarea tarea = db.Tarea.FirstOrDefault(x => x.Secuencial == idTarea);
				if(tarea == null)
				{
					throw new Exception("No se encontró la tarea, contacte a soporte del sitio");
				}
				DateTime diaTarea = new System.DateTime(tarea.FechaInicio.Year, tarea.FechaInicio.Month, tarea.FechaInicio.Day);
				DateTime fechaInicioTareas = diaTarea.AddMinutes(30 + (8 * 60));

				if(tarea.FechaInicio.Date != DateTime.Now.Date)
				{
					if(tarea.FechaInicio >= fechaInicioTareas)
					{
						throw new Exception("No puede modificar la tarea, no corresponde al día de hoy.");
					}
				}

				if(estado == 3 && tarea.tareaActividadRealizada.Count() == 0)//Terminada
				{
					throw new Exception("La tarea no se puede terminar puesto que no contiene actividades realizadas.");
				}
				Colaborador colab = user.persona.colaborador.FirstOrDefault();
				if(tarea.SecuencialColaborador != colab.Secuencial)
				{
					throw new Exception("No puede actualizar tareas de otro colaborador");
				}

				tarea.SecuencialEstadoTarea = estado;

				HistoricoTareaEstado histET = new HistoricoTareaEstado
				{
					tarea = tarea,
					SecuencialEstadoTarea = estado,
					FechaOperacion = DateTime.Now,
					usuario = user
				};
				db.HistoricoTareaEstado.Add(histET);

				db.SaveChanges();

				if(estado == 3)//Terminado
				{
					/*Verificando si la tarea está relacionada a un ticket y si es la última de las tareas por terminar*/
					TicketTarea ticketTarea = tarea.ticketTarea.FirstOrDefault();
					if(ticketTarea != null)//Aqui la tarea está asociada a un ticket
					{
						int idTicket = ticketTarea.SecuencialTicket;

						DateTime fechaHoy = DateTime.Now.Date;
						int cantNoTerminados = (
												  from tt in db.TicketTarea
												  join t in db.Tarea on tt.SecuencialTarea equals t.Secuencial
												  where (t.SecuencialEstadoTarea != 3 && t.SecuencialEstadoTarea != 4) && t.FechaInicio >= fechaHoy && tt.SecuencialTicket == idTicket
												  select (t.Secuencial)
											   ).ToList().Count();

						if(cantNoTerminados == 0)//Todas las asignaciones están terminadas
						{
							Ticket ticket = db.Ticket.Find(idTicket);
							//Pasando el ticket de estado a Resuelto

							if(ticket.SecuencialEstadoTicket != 10)
							{
								ticket.SecuencialEstadoTicket = 10;//EL TICKET ESTA RESUELTO
								if(!publicar)//Si se publica se queda el mismo estado del ticket
								{
									//Cambiando el estado del ticket                                    
									ticket.SecuencialProximaActividad = 17;//CERTIFICAR
								}
								else
								{
									ticket.SecuencialProximaActividad = 16;//PUBLICAR
								}

								//Adicionando el histórico del ticket                                                        
								int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

								TicketHistorico ticketHistorico = new TicketHistorico
								{
									ticket = ticket,
									Version = numeroVersion,
									SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA RESUELTO")
									SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
									SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
									SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
									SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
									SecuencialProximaActividad = ticket.SecuencialProximaActividad,
									usuario = user,
									ReportadoPor = ticket.ReportadoPor,
									Reputacion = ticket.Reputacion,
									Telefono = ticket.Telefono,
									Asunto = ticket.Asunto,
									Detalle = ticket.Detalle,
									FechaCreado = ticket.FechaCreado,
									Estimacion = ticket.Estimacion,
									NumeroVerificador = 1,
									FechaOperacion = DateTime.Now,
									SeFactura = ticket.SeFactura,
									Facturado = ticket.Facturado,
									IngresoInterno = ticket.IngresoInterno,
									Reprocesos = ticket.Reprocesos
								};
								db.TicketHistorico.Add(ticketHistorico);

								db.SaveChanges();//Salvando los cambios

								/*
                                //Enviando el correo a los usuarios
                                List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");
                                Persona personaCliente = ticket.persona_cliente.persona;
                                string nombreCliente = personaCliente.Nombre1 + " " + personaCliente.Apellido1;
                                string correoCliente = personaCliente.usuario.FirstOrDefault().Email;
                                correosDestinos.Insert(0, emailUser);
                                
                                string textoEmail = "<div class=\"textoCuerpo\">Por medio del presente correo le informamos que la terminación de esta tarea dio por <b>'RESUELTO'</b> el ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b>.<br/>                                       
                                      <b>Asunto del ticket: </b>" + ticket.Asunto + @"<br/>
                                      Por favor comuníquese lo antes posible con el cliente.<br/>
                                      Nombre del cliente: " + nombreCliente + @"<br/>
                                      Correo del cliente: " + correoCliente + @"<br/></div>";

                                //Borrar aqui
                                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                                Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Resuelto (" + ticket.Asunto + ")", null, string.Format("{0:000000}", ticket.Secuencial));

                                //adicionando el email a los historicos
                                string destinos = String.Join(", ", correosDestinos.ToArray());
                                string textoHistoricoCorreo = "<b>Correo de información, Ticket Resuelto</b><br/>";
                                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                                textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket Resuelto" + "<br/>";
                                textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
                                HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
                                {
                                    SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
                                    VersionTicketHistorico = ticketHistorico.Version,
                                    Fecha = DateTime.Now,
                                    Texto = textoHistoricoCorreo
                                };
                                db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
                                db.SaveChanges();
                                */
							}
						}
					}
				}
				else if(estado == 2)//Desarrollo
				{
					/*Verificando si la tarea está relacionada a un ticket*/
					TicketTarea ticketTarea = tarea.ticketTarea.FirstOrDefault();
					if(ticketTarea != null)//Aqui la tarea está asociada a un ticket
					{
						int idTicket = ticketTarea.SecuencialTicket;

						Ticket ticket = db.Ticket.Find(idTicket);
						//Pasando el ticket de estado a EN DESARROLLO
						if(ticket.SecuencialEstadoTicket == 2)//SI ES IGUAL A ASIGNADO LO PASO A EN DESARROLLO
						{
							//Cambiando el estado del ticket
							ticket.SecuencialEstadoTicket = 9;//EL TICKET ESTA EN DESARROLLO
							ticket.SecuencialProximaActividad = 8;

							//Adicionando el histórico del ticket                                                        
							int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

							TicketHistorico ticketHistorico = new TicketHistorico
							{
								ticket = ticket,
								Version = numeroVersion,
								SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//EL TICKET ESTA EN DESARROLLO
								SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
								SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
								SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
								SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
								SecuencialProximaActividad = ticket.SecuencialProximaActividad,
								usuario = user,
								ReportadoPor = ticket.ReportadoPor,
								Reputacion = ticket.Reputacion,
								Telefono = ticket.Telefono,
								Asunto = ticket.Asunto,
								Detalle = ticket.Detalle,
								FechaCreado = ticket.FechaCreado,
								Estimacion = ticket.Estimacion,
								NumeroVerificador = 1,
								FechaOperacion = DateTime.Now,
								SeFactura = ticket.SeFactura,
								Facturado = ticket.Facturado,
								IngresoInterno = ticket.IngresoInterno,
								Reprocesos = ticket.Reprocesos
							};
							db.TicketHistorico.Add(ticketHistorico);

							db.SaveChanges();//Salvando los cambios

							//Enviando Email de inicio de desarrollo
							List<string> destinatarioCorreos = Utiles.CorreoPorGrupoEmail("COORD");
							List<string> nombresColaboradores = new List<string>();
							Persona personaCliente = ticket.persona_cliente.persona;
							string emailCliente = personaCliente.usuario.FirstOrDefault().Email;

							//var idColaboradoresTicket = (from tt in db.TicketTarea
							//                             join tk in db.Ticket on tt.SecuencialTicket equals tk.Secuencial
							//                             join ta in db.Tarea on tt.SecuencialTarea equals ta.Secuencial
							//                             where tt.SecuencialTicket == ticket.Secuencial
							//                             select ta.SecuencialColaborador
							//                             ).Distinct().ToList();

							var idColaboradoresTicket = (from tt in db.TicketTarea
														 join tk in db.Ticket on tt.SecuencialTicket equals tk.Secuencial
														 join ta in db.Tarea on tt.SecuencialTarea equals ta.Secuencial
														 where tt.SecuencialTicket == ticket.Secuencial
															   && ta.FechaInicio >= DateTime.Today
														 select ta.SecuencialColaborador
														 ).Distinct().ToList();

							destinatarioCorreos.Insert(0, emailCliente);//Borrar aqui                
							idColaboradoresTicket = idColaboradoresTicket.Distinct().ToList();
							foreach(int idColab in idColaboradoresTicket)
							{
								Persona personaColaborador = db.Colaborador.Find(idColab).persona;
								string email = personaColaborador.usuario.FirstOrDefault().Email;
								destinatarioCorreos.Add(email);
								nombresColaboradores.Add(personaColaborador.Nombre1 + " " + personaColaborador.Apellido1);
							}

							string textoEmail = "<div class=\"textoCuerpo\">Estimado: ";
							if(personaCliente.Sexo == "F")
								textoEmail = "<div class=\"textoCuerpo\">Estimada: ";
							textoEmail += personaCliente.Nombre1 + " " + personaCliente.Apellido1 + ",<br/>";

							string textoIngenieros = String.Join(", Ing. ", nombresColaboradores);

							string articulo = "el";
							if(nombresColaboradores.Count > 1)
								articulo = "los";
							string pronombreRelativo = "quien";
							if(nombresColaboradores.Count > 1)
								pronombreRelativo = "quienes";
							string verboRelativo = "atenderá";
							if(nombresColaboradores.Count > 1)
								verboRelativo = "atenderán";

							textoEmail += @"Su requerimiento <b>" + string.Format("{0:000000}", ticket.Secuencial) + "</b> se está desarrollando por " + articulo + " Ing. " + textoIngenieros + ", " + pronombreRelativo + @" <br/>
                                                            " + verboRelativo + @" el mismo a partir de este momento.<br/>
                                                            <b>Asunto del ticket: </b>" + ticket.Asunto + @"<br/>
                                                            <b>Detalle: </b> " + ticket.Detalle + @"<br/>
                                                            Ing. " + textoIngenieros + ", <br/> por favor su ayuda.</div>";

							string codigoCliente = ticket.persona_cliente.cliente.Codigo;
							string asuntoEmail = codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Asignación del requerimiento (" + ticket.Asunto + ")";
							Utiles.EnviarEmailSistema(destinatarioCorreos.ToArray(), textoEmail, asuntoEmail, null, string.Format("{0:000000}", ticket.Secuencial));

							//adicionando email a los historicos
							string destinos = String.Join(", ", destinatarioCorreos.ToArray());
							string textoHistoricoCorreo = "<b>Correo de información, inicio de desarrollo de ticket</b><br/>";
							textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
							textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
							textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
							HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
							{
								SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
								VersionTicketHistorico = ticketHistorico.Version,
								Fecha = DateTime.Now,
								Texto = textoHistoricoCorreo
							};
							db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
							db.SaveChanges();
						}
					}
				}
				else if(estado == 5)//EN PAUSA
				{
					/*Verificando si la tarea está relacionada a un ticket*/
					TicketTarea ticketTarea = tarea.ticketTarea.FirstOrDefault();
					if(ticketTarea != null)//Aqui la tarea está asociada a un ticket
					{
						int idTicket = ticketTarea.SecuencialTicket;

						Ticket ticket = db.Ticket.Find(idTicket);
						//Pasando el ticket de estado a SUSPENDIDO
						if(ticket.SecuencialEstadoTicket != 20)//SI EL TICKET NO ESTA SUSPENDIDO
						{
							//Cambiando el estado del ticket
							ticket.SecuencialEstadoTicket = 20;//EL TICKET ESTA EN SUSPENDIDO
							ticket.SecuencialProximaActividad = 28; //DUDA SI MANTENGO ESTE ESTADO

							//Adicionando el histórico del ticket                                                        
							int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

							TicketHistorico ticketHistorico = new TicketHistorico
							{
								ticket = ticket,
								Version = numeroVersion,
								SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//EL TICKET ESTA EN SUSPENDIDO
								SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
								SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
								SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
								SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
								SecuencialProximaActividad = ticket.SecuencialProximaActividad,
								usuario = user,
								ReportadoPor = ticket.ReportadoPor,
								Reputacion = ticket.Reputacion,
								Telefono = ticket.Telefono,
								Asunto = ticket.Asunto,
								Detalle = ticket.Detalle,
								FechaCreado = ticket.FechaCreado,
								Estimacion = ticket.Estimacion,
								NumeroVerificador = 1,
								FechaOperacion = DateTime.Now,
								SeFactura = ticket.SeFactura,
								Facturado = ticket.Facturado,
								IngresoInterno = ticket.IngresoInterno,
								Reprocesos = ticket.Reprocesos
							};
							db.TicketHistorico.Add(ticketHistorico);

							db.SaveChanges();//Salvando los cambios

							//Enviando Email de inicio de desarrollo
							List<string> destinatarioCorreos = Utiles.CorreoPorGrupoEmail("COORD");
							List<string> nombresColaboradores = new List<string>();
							Persona personaCliente = ticket.persona_cliente.persona;

							var idColaboradoresTicket = (from tt in db.TicketTarea
														 join tk in db.Ticket on tt.SecuencialTicket equals tk.Secuencial
														 join ta in db.Tarea on tt.SecuencialTarea equals ta.Secuencial
														 where tt.SecuencialTicket == ticket.Secuencial
														 select ta.SecuencialColaborador
														 ).Distinct().ToList();

							idColaboradoresTicket = idColaboradoresTicket.Distinct().ToList();
							foreach(int idColab in idColaboradoresTicket)
							{
								Persona personaColaborador = db.Colaborador.Find(idColab).persona;
								string email = personaColaborador.usuario.FirstOrDefault().Email;
								destinatarioCorreos.Add(email);
								nombresColaboradores.Add(personaColaborador.Nombre1 + " " + personaColaborador.Apellido1);
							}

							string textoEmail = "<div class=\"textoCuerpo\">Estimados: <br/>";
							string textoIngenieros = String.Join(", Ing. ", nombresColaboradores);

							string articulo = "el";
							if(nombresColaboradores.Count > 1)
								articulo = "los";

							textoEmail += @"El requerimiento <b>" + string.Format("{0:000000}", ticket.Secuencial) + "</b> que se estaba desarrollando por " + articulo + " Ing. " + textoIngenieros + @". <br/>
                                                            se encuentra en el estado: 'SUSPENDIDO' a partir de este momento.<br/>
                                                            <b>Asunto del ticket: </b>" + ticket.Asunto + @"<br/>
                                                            <b>Detalle: </b> " + ticket.Detalle + @"<br/>
                                                            El ticket fue suspendido por: <b>" + user.persona.Nombre1 + " " + user.persona.Apellido1 + "</b><br/></div>";

							string codigoCliente = ticket.persona_cliente.cliente.Codigo;
							string asuntoEmail = codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket suspendido (" + ticket.Asunto + ")";
							Utiles.EnviarEmailSistema(destinatarioCorreos.ToArray(), textoEmail, asuntoEmail, null, string.Format("{0:000000}", ticket.Secuencial));

							//adicionando email a los historicos
							string destinos = String.Join(", ", destinatarioCorreos.ToArray());
							string textoHistoricoCorreo = "<b>Correo de información, ticket suspendido</b><br/>";
							textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
							textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
							textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
							HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
							{
								SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
								VersionTicketHistorico = ticketHistorico.Version,
								Fecha = DateTime.Now,
								Texto = textoHistoricoCorreo
							};
							db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
							db.SaveChanges();
						}
					}
				}

				var result = new
				{
					success = true,
					msg = "Operación realizada correctamente"
				};
				return Json(result);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult TareaPerteneceTicket(int idTarea, int estado = 3)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Tarea tarea = db.Tarea.FirstOrDefault(x => x.Secuencial == idTarea);
				if(tarea == null)
				{
					throw new Exception("No se encontró la tarea, contacte a soporte del sitio");
				}
				DateTime diaTarea = new System.DateTime(tarea.FechaInicio.Year, tarea.FechaInicio.Month, tarea.FechaInicio.Day);
				DateTime fechaInicioTareas = diaTarea.AddMinutes(30 + (8 * 60));
				if(tarea.FechaInicio.Date != DateTime.Today)
				{
					if(tarea.FechaInicio >= fechaInicioTareas)
						throw new Exception("La tarea no se puede cambiar de estado, no es del día de hoy.");
				}
				if(estado == 3 && tarea.tareaActividadRealizada.Count() == 0)//Terminada
				{
					throw new Exception("La tarea no se puede terminar puesto que no contiene actividades realizadas.");
				}
				Colaborador colab = user.persona.colaborador.FirstOrDefault();
				if(tarea.SecuencialColaborador != colab.Secuencial)
				{
					throw new Exception("No puede actualizar tareas de otro colaborador");
				}

				/*Verificando si la tarea está relacionada a un ticket y si es la última de las tareas por terminar*/
				TicketTarea ticketTarea = tarea.ticketTarea.FirstOrDefault();
				if(ticketTarea != null)//Aqui la tarea está asociada a un ticket
				{
					int idTicket = ticketTarea.SecuencialTicket;

					DateTime fechaHoy = DateTime.Now.Date;
					int cantNoTerminados = (from tt in db.TicketTarea
											join t in db.Tarea on tt.SecuencialTarea equals t.Secuencial
											where (t.SecuencialEstadoTarea != 3 && t.SecuencialEstadoTarea != 4) &&
													t.FechaInicio >= fechaHoy &&
													tt.SecuencialTicket == idTicket
											select (t.Secuencial)
											).ToList().Count();

					if(cantNoTerminados == 1)//Todas las asignaciones están terminadas excepto esta
					{
						var result = new
						{
							success = true,
							pertenece = true,
							tipo = "ticket"
						};
						return Json(result);
					}
				}
				else if(tarea.entregableMotivoTrabajo != null)
				{/*VERIFICANDO SI LA TAREA PERTENECE A UN CONJUNTO DE MOTIVO DE TRABAJO*/

					int secuencialEntregableMotivoTrabajo = tarea.entregableMotivoTrabajo.Secuencial;
					DateTime fechaHoy = DateTime.Now.Date;
					int cantNoTerminados = (from t in db.Tarea
											where t.entregableMotivoTrabajo.Secuencial == secuencialEntregableMotivoTrabajo &&
												  t.SecuencialEstadoTarea != 3 && t.SecuencialEstadoTarea != 4 && t.SecuencialEstadoTarea != 6 && t.SecuencialEstadoTarea != 1006 &&
												  t.FechaInicio >= fechaHoy
											select (t.Secuencial)
											).ToList().Count();
					if(cantNoTerminados == 1)//Todas las asignaciones están terminadas excepto esta
					{
						var result = new
						{
							success = true,
							pertenece = true,
							tipo = "contrato"
						};
						return Json(result);
					}
				}

				var result1 = new
				{
					success = true,
					pertenece = false
				};
				return Json(result1);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult EnviarEmailFinTicket(int idTarea, string texto, bool publicar = false, HttpPostedFileBase[] adjuntos = null)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				List<string> listaFicheros = new List<string>();
				List<string> listaPathFicheros = new List<string>();

				Tarea tarea = db.Tarea.Find(idTarea);
				Ticket ticket = tarea.ticketTarea.FirstOrDefault().ticket;
				if(ticket == null)
				{
					throw new Exception("Error, no se encontró el ticket");
				}
				int idTicket = ticket.Secuencial;
				TicketHistorico ticketHistorico = db.TicketHistorico.Where(x => x.SecuencialTicket == idTicket)
													.OrderByDescending(x => x.Version).FirstOrDefault();
				if(ticketHistorico == null)
				{
					throw new Exception("Error, no se encontró el ticket histórico");
				}

				//Por los ficheros adjuntos
				if(adjuntos != null)
					foreach(HttpPostedFileBase adj in adjuntos)
					{
						if(adj != null)
						{
							string extFile = Path.GetExtension(adj.FileName);
							string newNameFile = "tec_" + System.IO.Path.GetRandomFileName() + extFile;
							string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
							adj.SaveAs(path);

							AdjuntoTicket adjTicket = new AdjuntoTicket
							{
								Url = "/resources/tickets/" + newNameFile,
								ticket = db.Ticket.Where(s => s.Secuencial == idTicket).FirstOrDefault()
							};
							db.AdjuntoTicket.Add(adjTicket);
							db.SaveChanges();

							listaPathFicheros.Add(path);
							listaFicheros.Add("/resources/tickets/" + newNameFile);
						}
					}

				if(publicar)
				{
					Ticket_RequierePublicacion ticketRQ = db.Ticket_RequierePublicacion.Find(ticket.Secuencial);
					if(ticketRQ == null)
					{
						ticketRQ = new Ticket_RequierePublicacion
						{
							ticket = ticket
						};
						db.Ticket_RequierePublicacion.Add(ticketRQ);
					}

					bool financial25 = false;
					if(ticket.SecuencialTicketVersionCliente != null)
					{
						financial25 = db.TicketVersionCliente.Find(ticket.SecuencialTicketVersionCliente).Codigo == "FBS 2.5";
					}

					string textoEmail = @"<div class='textoCuerpo'><br/>";
					textoEmail += texto;
					textoEmail += @"<br/>";

					textoEmail += "<br/><i>La presente actividad concluyó el desarrollo del <b>TICKET</b> y se dispone a pasar el mismo a la actividad <b>PUBLICAR</b>.</i>";
					textoEmail += "</div>";

					Persona_Cliente personaCliente = db.Persona_Cliente.Find(ticket.SecuencialPersona_Cliente);
					Persona persona = personaCliente.persona;
					string emailCliente = persona.usuario.FirstOrDefault().Email;

					List<string> correosDestinos = new List<string>();
					correosDestinos.Add(emailCliente);
					correosDestinos.Add(emailUser);
					if(financial25)
					{
						correosDestinos.Add("publicacionesdoscinco@sifizsoft.com");
					}
					correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));
					correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("TFS"));

					string asuntoEmail = personaCliente.cliente.Codigo + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Resuelto (" + ticket.Asunto + ")";
					Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));

					//Adicionando el comentario al ticket                                
					Persona personaUsuario = user.persona;
					ComentarioTicket comentarioTicket = new ComentarioTicket
					{
						SecuencialColaborador = personaUsuario.colaborador.FirstOrDefault().Secuencial,
						SecuencialTicket = idTicket,
						FechaHora = DateTime.Now,
						Detalle = "TICKET RESUELTO: " + texto,
						VerTodos = 1
					};
					db.ComentarioTicket.Add(comentarioTicket);
					db.SaveChanges();

					//adicionando el email a los historicos
					string destinos = String.Join(", ", correosDestinos.ToArray());
					string textoHistoricoCorreo = "<b>Correo de información, Ticket Implementado</b><br/>";
					textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
					textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
					textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
					HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
					{
						SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
						VersionTicketHistorico = ticketHistorico.Version,
						Fecha = DateTime.Now,
						Texto = textoHistoricoCorreo
					};
					db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
					db.SaveChanges();

					//Adicionando los adjuntos
					foreach(string url in listaFicheros)
					{
						HistoricoAdjunto historicoAdjunto = new HistoricoAdjunto();
						historicoAdjunto.historicoInformacionTicket = historicoCorreoTicket;
						historicoAdjunto.Url = url;
						db.HistoricoAdjunto.Add(historicoAdjunto);
					}
					db.SaveChanges();
				}
				else
				{
					Ticket_RequierePublicacion ticketRQ = db.Ticket_RequierePublicacion.Find(ticket.Secuencial);
					if(ticketRQ != null)
					{
						db.Ticket_RequierePublicacion.Remove(ticketRQ);
					}

					string textoEmail = @"<div class='textoCuerpo'><br/>";
					textoEmail += texto;

					textoEmail += @"<br/>";

					//Los link que se le envían al usuario para aceptar o rechazar la resolución
					string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

					string linkAceptar = baseUrl + "/clientes/respuesta-resolucion?cod=" + Server.UrlEncode(Utiles.EncriptacionSimetrica(ticket.Secuencial + ":ACEPTADO"));
					string linkRechazar = baseUrl + "/clientes/respuesta-resolucion?cod=" + Server.UrlEncode(Utiles.EncriptacionSimetrica(ticket.Secuencial + ":NOACEPTADO"));
					string textoEmailCliente = textoEmail + @"<br/><div style='font-size: 11pt; font-family: sans-serif; color: #1F497D;'>
			                                                <span style='color:#128812'>
				                                                Si usted <b>ACEPTA</b> este requerimiento por favor presione el siguiente link:
			                                                </span><br/>                                                            
			                                                <a href='" + linkAceptar + @"'>
                                                                <i>" + linkAceptar + @"</i>
			                                                </a>			
			                                                <br/><br/>
			                                                <span style='color:#EE1212'>Si por el contrario <b>NO ACEPTA</b> este requerimiento por favor presione el siguiente link:</span><br/>
			                                                <a href='" + linkRechazar + @"'>
				                                                <i>" + linkRechazar + @"</i>
			                                                </a>
                                                            <br/>
                                                            <br/>
                                                            <i>El presente correo concluye la resolución de este requerimiento, formalmente solicitamos la certificación del mismo o sus observaciones. Si dentro de los próximos 5 días laborables no recibimos su respuesta, procederemos a cerrar el ticket. En caso de requerir correcciones será necesario que ingrese otro ticket.</i>
		                                                </div>";

					Persona_Cliente personaCliente = db.Persona_Cliente.Find(ticket.SecuencialPersona_Cliente);
					Persona persona = personaCliente.persona;
					string emailCliente = persona.usuario.FirstOrDefault().Email;

					textoEmail += "<br/><i>El presente correo concluye la resolución de este requerimiento, formalmente solicitamos la certificación del mismo o sus observaciones. Si dentro de los próximos 5 días laborables no recibimos su respuesta, procederemos a cerrar el ticket. En caso de requerir correcciones será necesario que ingrese otro ticket.</i><br/><i>Se ha enviado una copia de este email al cliente. Email:" + emailCliente + "</i>";
					textoEmail += "</div>";
					textoEmailCliente += "</div>";

					List<string> correosDestinos = new List<string>();
					correosDestinos.Add(emailUser);
					correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));

					string asuntoEmail = personaCliente.cliente.Codigo + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Resuelto (" + ticket.Asunto + ")";
					Utiles.EnviarEmailSistema(new string[] { emailCliente }, textoEmailCliente, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));
					Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));

					//Adicionando el comentario al ticket                                
					Persona personaUsuario = user.persona;
					ComentarioTicket comentarioTicket = new ComentarioTicket
					{
						SecuencialColaborador = personaUsuario.colaborador.FirstOrDefault().Secuencial,
						SecuencialTicket = idTicket,
						FechaHora = DateTime.Now,
						Detalle = "TICKET RESUELTO: " + texto,
						VerTodos = 1
					};
					db.ComentarioTicket.Add(comentarioTicket);
					db.SaveChanges();

					//adicionando el email a los historicos
					string destinos = String.Join(", ", correosDestinos.ToArray());
					string textoHistoricoCorreo = "<b>Correo de información, Ticket Resuelto</b><br/>";
					textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
					textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
					textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
					HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
					{
						SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
						VersionTicketHistorico = ticketHistorico.Version,
						Fecha = DateTime.Now,
						Texto = textoHistoricoCorreo
					};
					db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
					db.SaveChanges();

					//Adicionando los adjuntos
					foreach(string url in listaFicheros)
					{
						HistoricoAdjunto historicoAdjunto = new HistoricoAdjunto();
						historicoAdjunto.historicoInformacionTicket = historicoCorreoTicket;
						historicoAdjunto.Url = url;
						db.HistoricoAdjunto.Add(historicoAdjunto);
					}
					db.SaveChanges();
				}

				var result = new
				{
					success = true
				};
				return Json(result);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult guardarComentarioNoTerminacion(int idTarea, int proximaActividad, int causaNT, string comentario)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Tarea tarea = db.Tarea.Find(idTarea);
				if(tarea == null)
				{
					throw new Exception("No se encontró la tarea");
				}

				tarea.SecuencialEstadoTarea = 1006; //INCONCLUSA

				HistoricoTareaEstado histET = new HistoricoTareaEstado
				{
					tarea = tarea,
					SecuencialEstadoTarea = tarea.SecuencialEstadoTarea,
					FechaOperacion = DateTime.Now,
					usuario = user
				};
				db.HistoricoTareaEstado.Add(histET);

				Ticket ticket = null;
				try
				{
					ticket = tarea.ticketTarea.FirstOrDefault().ticket;
				}
				catch(Exception)
				{
					throw new Exception("No se encontró el ticket, asociado a la tarea.");
				}

				NoTerminacionTicket noterminacion = new NoTerminacionTicket
				{
					SecuencialActividadPropuesta = proximaActividad,
					SecuencialCausaNoTerminacion = causaNT,
					ticket = ticket,
					FechaHora = DateTime.Now,
					Comentario = comentario,
					NumeroVerificador = 1,
					SecuencialColaborador = user.persona.colaborador.FirstOrDefault().Secuencial
				};
				db.NoTerminacionTicket.Add(noterminacion);

				db.SaveChanges();

				//Actualizando el ticket
				ticket.SecuencialEstadoTicket = 21; //INCONCLUSO O POR TERMINAR
				ticket.SecuencialProximaActividad = 2;

				//Adicionando el histórico del ticket                                                        
				int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

				TicketHistorico ticketHistorico = new TicketHistorico
				{
					ticket = ticket,
					Version = numeroVersion,
					SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA RESUELTO")
					SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
					SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
					SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
					SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
					SecuencialProximaActividad = ticket.SecuencialProximaActividad,
					usuario = user,
					ReportadoPor = ticket.ReportadoPor,
					Reputacion = ticket.Reputacion,
					Telefono = ticket.Telefono,
					Asunto = ticket.Asunto,
					Detalle = ticket.Detalle,
					FechaCreado = ticket.FechaCreado,
					Estimacion = ticket.Estimacion,
					NumeroVerificador = 1,
					FechaOperacion = DateTime.Now,
					SeFactura = ticket.SeFactura,
					Facturado = ticket.Facturado,
					IngresoInterno = ticket.IngresoInterno,
					Reprocesos = ticket.Reprocesos
				};
				db.TicketHistorico.Add(ticketHistorico);

				db.SaveChanges();//Salvando los cambios

				//Enviando Email de no terminación de la tarea
				List<string> destinatarioCorreos = Utiles.CorreoPorGrupoEmail("COORD");
				List<string> nombresColaboradores = new List<string>();
				Persona personaCliente = ticket.persona_cliente.persona;

				var idColaboradoresTicket = (from tt in db.TicketTarea
											 join tk in db.Ticket on tt.SecuencialTicket equals tk.Secuencial
											 join ta in db.Tarea on tt.SecuencialTarea equals ta.Secuencial
											 where tt.SecuencialTicket == ticket.Secuencial
											 select ta.SecuencialColaborador
											 ).Distinct().ToList();

				idColaboradoresTicket = idColaboradoresTicket.Distinct().ToList();
				foreach(int idColab in idColaboradoresTicket)
				{
					Persona personaColaborador = db.Colaborador.Find(idColab).persona;
					string email = personaColaborador.usuario.FirstOrDefault().Email;
					destinatarioCorreos.Add(email);
					nombresColaboradores.Add(personaColaborador.Nombre1 + " " + personaColaborador.Apellido1);
				}

				string textoEmail = "<div class=\"textoCuerpo\">Estimados: <br/>";
				textoEmail += "Al ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + "</b> se le han terminado sus asignaciones y el ticket está como <b>INCONCLUSO</b>, <br/>";

				string textoIngenieros = String.Join(", ", nombresColaboradores);

				string articulo = "El desarrollador ( ";
				if(nombresColaboradores.Count > 1)
					articulo = "Los desarrolladores ( ";
				string pronombreRelativo = "expone";
				if(nombresColaboradores.Count > 1)
					pronombreRelativo = "exponen";

				textoEmail += articulo + textoIngenieros + " ) " + pronombreRelativo + " lo siguiente: <br/>";
				textoEmail += "<b>Próxima actividad:</b> " + db.ActividadPropuesta.Find(proximaActividad).Descripcion + "<br/>";
				textoEmail += "<b>Causa de la no terminación:</b> " + db.CausaNoTerminacion.Find(causaNT).Descripcion + "<br/>";
				textoEmail += "<b>Comentario:</b> <br/>" + comentario + "<br/></div>";

				string codigoCliente = ticket.persona_cliente.cliente.Codigo;
				string asuntoEmail = codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - No terminación del ticket (" + ticket.Asunto + ")";
				Utiles.EnviarEmailSistema(destinatarioCorreos.ToArray(), textoEmail, asuntoEmail, null, string.Format("{0:000000}", ticket.Secuencial));

				//adicionando email a los historicos
				string destinos = String.Join(", ", destinatarioCorreos.ToArray());
				string textoHistoricoCorreo = "<b>Correo de información, no terminación del ticket</b><br/>";
				textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
				textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
				textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
				HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
				{
					SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
					VersionTicketHistorico = ticketHistorico.Version,
					Fecha = DateTime.Now,
					Texto = textoHistoricoCorreo
				};
				db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
				db.SaveChanges();

				var resp = new
				{
					success = true
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

		//ACTIVIDADES DE LAS TAREAS
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarActividadesTareaSegunActividadTarea(int idTarea)
		{
			try
			{
				Tarea tarea = db.Tarea.Find(idTarea);
				if(tarea == null)
					throw new Exception("No se encontró la tarea");

				bool edicion = true;
				if(!User.IsInRole("ADMIN"))
				{
					if(tarea.FechaInicio.Date != DateTime.Now.Date)
						edicion = false;
					//throw new Exception("No puede modificar la tarea, no corresponde al día de hoy.");
				}


				Actividad actividad = tarea.actividad;
				var actividadesTarea = (from ar in db.ActividadRealizada
										join aar in db.ActividadActividadRealizada on ar.Secuencial equals aar.SecuencialActividadRealizada
										where aar.SecuencialActividad == tarea.SecuencialActividad && aar.EstaActiva == 1 && ar.EstaActiva == 1
										select new
										{
											id = ar.Secuencial,
											nombre = ar.Descripcion
										}).ToList();

				var resp = new
				{
					success = true,
					actividadesTarea = actividadesTarea,
					edicion = edicion
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult guardarComentarioNoTerminacionTarea(int idTarea, int proximaActividad, int causaNT, string comentario)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Tarea tarea = db.Tarea.Find(idTarea);
				if(tarea == null)
				{
					throw new Exception("No se encontró la tarea");
				}

				tarea.SecuencialEstadoTarea = 1006; //INCONCLUSA

				HistoricoTareaEstado histET = new HistoricoTareaEstado
				{
					tarea = tarea,
					SecuencialEstadoTarea = tarea.SecuencialEstadoTarea,
					FechaOperacion = DateTime.Now,
					usuario = user
				};
				db.HistoricoTareaEstado.Add(histET);

				NoTerminacionTarea noterminacion = new NoTerminacionTarea
				{
					tarea = tarea,
					SecuencialActividadPropuesta = proximaActividad,
					SecuencialCausaNoTerminacion = causaNT,
					FechaHora = DateTime.Now,
					Comentario = comentario,
					NumeroVerificador = 1
				};

				db.NoTerminacionTarea.Add(noterminacion);

				db.SaveChanges();

				var resp = new
				{
					success = true
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult AdicionarActividadTarea(int idTarea, int tipoTarea, string fecha, string horaInicio, string horaFin)
		{
			try
			{
				Tarea tar = db.Tarea.Find(idTarea);
				if(tar == null)
				{
					throw new Exception("No se encontró la tarea, contacte el admin del sistema");
				}

				DateTime diaTarea = new System.DateTime(tar.FechaInicio.Year, tar.FechaInicio.Month, tar.FechaInicio.Day);
				DateTime fechaInicioTareas = diaTarea.AddMinutes(30 + (8 * 60));
				if(tar.FechaInicio.Date != DateTime.Today)
				{
					if(tar.FechaInicio >= fechaInicioTareas)
						throw new Exception("No puede ingresar actividades porque la tarea no es del día de hoy.");
				}

				string[] fechas = fecha.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				//DateTime fechaActividad = new System.DateTime(anno, mes, dia);
				DateTime fechaActividad = new System.DateTime(tar.FechaInicio.Year, tar.FechaInicio.Month, tar.FechaInicio.Day);

				string textoFechaInicio = fecha + " " + horaInicio;
				DateTime horaInicioActividad = DateTime.ParseExact(textoFechaInicio, "dd/MM/yyyy H:mm", CultureInfo.InvariantCulture);
				//DateTime horaInicioActividad = DateTime.Parse( textoFechaInicio );

				string textoFechaFin = fecha + " " + horaFin;
				DateTime horaFinActividad = DateTime.ParseExact(textoFechaFin, "dd/MM/yyyy H:mm", CultureInfo.InvariantCulture);
				//DateTime horaFinActividad = DateTime.Parse( textoFechaFin );

				//Verificaciones de 2 dias
				int cantDias = 1;
				if((int)tar.FechaInicio.Date.DayOfWeek == 1)//Si es lunes dejar hasta el viernes
				{
					cantDias = 3;
				}
				DateTime diaAnterior = tar.FechaInicio.Date.AddDays(-1 * cantDias);
				if(fechaActividad < diaAnterior)
				{
					throw new Exception("La actividad no se puede entrar al sistema, la fecha es anterior a " + cantDias + " días atrás.");
				}
				if(horaFinActividad < horaInicioActividad)
				{
					throw new Exception("La actividad no se puede entrar al sistema, la hora final es menor que la hora inicial.");
				}

				//Verificando que las horas ya hayan ocurrido, si es del mismo usuario
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				if(tar.colaborador.Secuencial == user.persona.colaborador.FirstOrDefault().Secuencial)
				{
					if(horaFinActividad > DateTime.Now || horaInicioActividad > DateTime.Now)
					{
						throw new Exception("La actividad no se puede entrar al sistema, las horas de inicio y final son mayores que la hora actual.");
					}
				}

				//Verificando solapamiento de tareas
				int cant = (from act in db.TareaActividadRealizada
							where act.tarea.SecuencialColaborador == tar.SecuencialColaborador &&
								  (
									(act.HoraInicio > horaInicioActividad && act.HoraInicio < horaFinActividad) ||
									(act.HoraFin > horaInicioActividad && act.HoraFin < horaFinActividad) ||
									(act.HoraInicio <= horaInicioActividad && act.HoraFin >= horaFinActividad)
								  )
							select act.Secuencial).Count();
				if(cant > 0)
				{
					throw new Exception("No puede adicionar la actividad, ya que las horas de esta se solapan con las de otra actividad.");
				}

				TareaActividadRealizada tareaActividadRealizada = new TareaActividadRealizada
				{
					tarea = tar,
					SecuencialActividadRealizada = tipoTarea,
					usuario = user,
					Fecha = fechaActividad,
					HoraInicio = horaInicioActividad,
					HoraFin = horaFinActividad,
					HoraOperacion = DateTime.Now
				};
				db.TareaActividadRealizada.Add(tareaActividadRealizada);
				db.SaveChanges();

				int minutosUtilizados = 0;
				var actividadesRealizadas = tar.tareaActividadRealizada.ToList();
				foreach(var actividad in actividadesRealizadas)
				{
					TimeSpan tiempo = actividad.HoraFin - actividad.HoraInicio;
					minutosUtilizados += (int)tiempo.TotalMinutes;
				}

				tar.HorasUtilizadas = (decimal)Math.Round(((double)minutosUtilizados / 60), 2);
				db.SaveChanges();

				var actividadesTarea = (from tActRel in db.TareaActividadRealizada
										where tActRel.SecuencialTarea == tar.Secuencial
										orderby tActRel.HoraInicio
										select new
										{
											id = tActRel.Secuencial,
											tipo = tActRel.actividadRealizada.Descripcion,
											fecha = tActRel.Fecha,
											horaInicio = tActRel.HoraInicio,
											horaFin = tActRel.HoraFin
										}).ToList();

				List<object> actividadesListTarea = new List<object>();
				foreach(var act in actividadesTarea)
				{
					int horas = (int)(act.horaFin - act.horaInicio).Hours;
					int minutos = (int)(act.horaFin - act.horaInicio).Minutes;
					string strMin = minutos.ToString();
					if(minutos < 10)
						strMin = "0" + strMin;
					string time = horas + ":" + strMin;

					actividadesListTarea.Add(
						new
						{
							id = act.id,
							tipo = act.tipo,
							fecha = act.fecha.ToString("dd/MM/yyyy"),
							horaInicio = act.horaInicio.ToString("HH:mm"),
							horaFin = act.horaFin.ToString("HH:mm"),
							horas = time
						}
					);
				}

				int minutosTarea = (int)(tar.HorasUtilizadas * 60);
				int horasTarea = minutosTarea / 60;
				int minutosRestaTarea = minutosTarea % 60;
				string strMinutosRestaTarea = minutosRestaTarea.ToString();
				if(minutosRestaTarea < 10)
					strMinutosRestaTarea = "0" + strMinutosRestaTarea;

				var resp = new
				{
					success = true,
					actividadesTarea = actividadesListTarea,
					totalHoras = horasTarea + ":" + strMinutosRestaTarea
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult ActualizarHoraActividadTarea(int idActividadTarea, string horaInicio, string horaFin)
		{
			try
			{
				var actividadTarea = db.TareaActividadRealizada.Find(idActividadTarea);
				if(actividadTarea == null)
					throw new Exception("No se encontró la actividad");
				Tarea tar = actividadTarea.tarea;

				DateTime diaActividad = actividadTarea.Fecha;
				horaInicio = diaActividad.ToString("dd/MM/yyyy ") + horaInicio;
				horaFin = diaActividad.ToString("dd/MM/yyyy ") + horaFin;
				DateTime dateHoraInicio = DateTime.Parse(horaInicio);
				DateTime dateHoraFin = DateTime.Parse(horaFin);

				//Verificaciones de 2 dias
				int cantDias = 1;
				if((int)DateTime.Today.DayOfWeek == 1)//Si es lunes dejar hasta el viernes
				{
					cantDias = 3;
				}
				DateTime diaAnterior = DateTime.Today.AddDays(-1 * cantDias);
				if(diaActividad < diaAnterior)
				{
					throw new Exception("La actividad no se puede entrar al sistema, la fecha es anterior a " + cantDias + " días atrás.");
				}
				if(dateHoraFin < dateHoraInicio)
				{
					throw new Exception("La actividad no se puede entrar al sistema, la hora final es menor que la hora inicial.");
				}

				//Verificando que las horas ya hayan ocurrido, si es del mismo usuario
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				if(tar.colaborador.Secuencial == user.persona.colaborador.FirstOrDefault().Secuencial)
				{
					if(dateHoraFin > DateTime.Now || dateHoraInicio > DateTime.Now)
					{
						throw new Exception("La actividad no se puede entrar al sistema, las horas de inicio y final son mayores que la hora actual.");
					}
				}

				//Verificando solapamiento de tareas
				int cant = (from act in db.TareaActividadRealizada
							where act.tarea.SecuencialColaborador == tar.SecuencialColaborador &&
								  act.Secuencial != actividadTarea.Secuencial &&
								  (
									(act.HoraInicio > dateHoraInicio && act.HoraInicio < dateHoraFin) ||
									(act.HoraFin > dateHoraInicio && act.HoraFin < dateHoraFin) ||
									(act.HoraInicio <= dateHoraInicio && act.HoraFin >= dateHoraFin)
								  )
							select act.Secuencial).Count();
				if(cant > 0)
				{
					throw new Exception("No puede adicionar la actividad, ya que las horas de esta se solapan con las de otra actividad.");
				}

				actividadTarea.HoraInicio = dateHoraInicio;
				actividadTarea.HoraFin = dateHoraFin;

				db.SaveChanges();

				int minutosUtilizados = 0;

				var actividadesRealizadas = tar.tareaActividadRealizada.ToList();
				foreach(var actividad in actividadesRealizadas)
				{
					TimeSpan tiempo = actividad.HoraFin - actividad.HoraInicio;
					minutosUtilizados += (int)tiempo.TotalMinutes;
				}

				tar.HorasUtilizadas = (decimal)Math.Round(((double)minutosUtilizados / 60), 2);
				db.SaveChanges();

				var actividadesTarea = (from tActRel in db.TareaActividadRealizada
										where tActRel.SecuencialTarea == tar.Secuencial
										orderby tActRel.HoraInicio
										select new
										{
											id = tActRel.Secuencial,
											tipo = tActRel.actividadRealizada.Descripcion,
											fecha = tActRel.Fecha,
											horaInicio = tActRel.HoraInicio,
											horaFin = tActRel.HoraFin
										}).ToList();

				List<object> actividadesListTarea = new List<object>();
				foreach(var act in actividadesTarea)
				{
					int horas = (int)(act.horaFin - act.horaInicio).Hours;
					int minutos = (int)(act.horaFin - act.horaInicio).Minutes;
					string strMin = minutos.ToString();
					if(minutos < 10)
						strMin = "0" + strMin;
					string time = horas + ":" + strMin;

					actividadesListTarea.Add(
						new
						{
							id = act.id,
							tipo = act.tipo,
							fecha = act.fecha.ToString("dd/MM/yyyy"),
							horaInicio = act.horaInicio.ToString("HH:mm"),
							horaFin = act.horaFin.ToString("HH:mm"),
							horas = time
						}
					);
				}

				int minutosTarea = (int)(tar.HorasUtilizadas * 60);
				int horasTarea = minutosTarea / 60;
				int minutosRestaTarea = minutosTarea % 60;
				string strMinutosRestaTarea = minutosRestaTarea.ToString();
				if(minutosRestaTarea < 10)
					strMinutosRestaTarea = "0" + strMinutosRestaTarea;

				var resp = new
				{
					success = true,
					actividadesTarea = actividadesListTarea,
					totalHoras = horasTarea + ":" + strMinutosRestaTarea
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarActividadesTarea(int idTarea)
		{
			try
			{
				var tar = db.Tarea.Find(idTarea);

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				bool tareaPropia = false;//Para saber si la tarea es propia o de un subordinado o coordinado
				if(tar.colaborador.Secuencial == user.persona.colaborador.FirstOrDefault().Secuencial)
				{
					tareaPropia = true;
				}

				var actividadesTarea = (from tActRel in db.TareaActividadRealizada
										where tActRel.SecuencialTarea == idTarea
										orderby tActRel.HoraInicio
										select new
										{
											id = tActRel.Secuencial,
											tipo = tActRel.actividadRealizada.Descripcion,
											fecha = tActRel.Fecha,
											horaInicio = tActRel.HoraInicio,
											horaFin = tActRel.HoraFin
										}).ToList();

				List<object> actividadesListTarea = new List<object>();
				foreach(var act in actividadesTarea)
				{
					int horas = (int)(act.horaFin - act.horaInicio).Hours;
					int minutos = (int)(act.horaFin - act.horaInicio).Minutes;
					string strMin = minutos.ToString();
					if(minutos < 10)
						strMin = "0" + strMin;
					string time = horas + ":" + strMin;

					actividadesListTarea.Add(
						new
						{
							id = act.id,
							tipo = act.tipo,
							fecha = act.fecha.ToString("dd/MM/yyyy"),
							horaInicio = act.horaInicio.ToString("HH:mm"),
							horaFin = act.horaFin.ToString("HH:mm tt"),
							horas = time
						}
					);
				}

				int minutosTarea = (int)(tar.HorasUtilizadas * 60);
				int horasTarea = minutosTarea / 60;
				int minutosRestaTarea = minutosTarea % 60;
				string strMinutosRestaTarea = minutosRestaTarea.ToString();
				if(minutosRestaTarea < 10)
					strMinutosRestaTarea = "0" + strMinutosRestaTarea;

				var resp = new
				{
					success = true,
					actividadesTarea = actividadesListTarea,
					totalHoras = horasTarea + ":" + strMinutosRestaTarea,
					tareaPropia = tareaPropia
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult EliminarActividadesTarea(int idActividadTarea)
		{
			try
			{
				TareaActividadRealizada tarActividadRealizada = db.TareaActividadRealizada.Find(idActividadTarea);
				if(tarActividadRealizada == null)
				{
					throw new Exception("No se encontré la actividad en la Tarea");
				}
				Tarea tar = tarActividadRealizada.tarea;

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				Persona persona = user.persona;

				if(tarActividadRealizada.SecuencialUsuario != user.Secuencial)
				{
					Persona personaActividadRealizada = tarActividadRealizada.usuario.persona;
					string mensaje = "Solo el usuario " + personaActividadRealizada.Nombre1 + " " + personaActividadRealizada.Apellido1 + " puede eliminar la actividad, porque fue quien la creó.";
					throw new Exception(mensaje);
				}

				List<ComentarioTarea> comentarios = tarActividadRealizada.comentarioTarea.ToList();
				var cant = (from c in comentarios
							where c.SecuencialUsuario != user.Secuencial
							select c).Count();
				if(cant > 0)
				{
					throw new Exception("No puede eliminar la tarea porque tiene comentarios no realizados por usted.");
				}
				foreach(ComentarioTarea c in comentarios)
				{
					db.ComentarioTarea.Remove(c);
				}

				db.TareaActividadRealizada.Remove(tarActividadRealizada);
				db.SaveChanges();

				int minutosUtilizados = 0;
				var actividadesRealizadas = tar.tareaActividadRealizada.ToList();
				foreach(var actividad in actividadesRealizadas)
				{
					TimeSpan tiempo = actividad.HoraFin - actividad.HoraInicio;
					minutosUtilizados += (int)tiempo.TotalMinutes;
				}
				tar.HorasUtilizadas = minutosUtilizados / 60;
				db.SaveChanges();

				var resp = new
				{
					success = true
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarCoordinadosUsuario(int idTrabajador, string fechaLunes, int semanas)
		{
			try
			{
				DateTime lunes = DateTime.Today;
				if(fechaLunes != "")
				{
					string[] fechas = fechaLunes.Split(new Char[] { '/' });
					int dia = Int32.Parse(fechas[0]);
					int mes = Int32.Parse(fechas[1]);
					int anno = Int32.Parse(fechas[2]);
					lunes = new System.DateTime(anno, mes, dia);
				}
				else
				{
					DateTime hoy = DateTime.Today;
					DayOfWeek diaSemana = hoy.DayOfWeek;
					if((int)diaSemana == 0)//Domingo
					{
						lunes = hoy.AddDays(-6);
					}
					else
					{
						long tiempo = (int)diaSemana;
						TimeSpan time = new TimeSpan(tiempo * 864000000000);
						DateTime domingo = hoy.Subtract(time);
						lunes = domingo.AddDays(1);
					}
				}

				DateTime fechaFin = lunes.AddDays((7 * semanas));
				var datos = (from t in db.Tarea
							 join
								 c in db.Colaborador on t.colaborador equals c
							 join
								 p in db.Persona on c.persona equals p
							 join
								 u in db.Usuario on p.Secuencial equals u.SecuencialPersona
							 join
								 d in db.Departamento on c.departamento equals d
							 join
								tc in db.Tarea_Coordinador on t.Secuencial equals tc.SecuencialTarea

							 where u.EstaActivo == 1 &&
								   d.Asignable == 1 &&
								   t.FechaInicio > lunes && t.FechaFin < fechaFin && //Entre las dos fechas
								   t.SecuencialEstadoTarea != 4 &&//Es cuando están anuladas
								   tc.SecuencialColaborador == idTrabajador
							 orderby t.FechaInicio, p.Nombre1, p.Apellido1
							 select new
							 {
								 idColaborador = c.Secuencial,
								 email = u.Email.ToUpper()
							 }).Distinct().ToList();

				//Buscando el principal
				var colaborador = (from c in db.Colaborador
								   join
									   p in db.Persona on c.persona equals p
								   join
									   u in db.Usuario on p.Secuencial equals u.SecuencialPersona
								   where c.Secuencial == idTrabajador
								   select new
								   {
									   idColaborador = c.Secuencial,
									   email = u.Email.ToUpper()
								   }).FirstOrDefault();

				List<object> datosFormateados = new List<object>();
				foreach(var dato in datos)
				{
					string[] arrayString = dato.email.Split(new Char[] { '@' });
					string login = arrayString[0];

					var newData = new
					{
						id = dato.idColaborador,
						text = login
					};

					datosFormateados.Add(newData);
				}

				string[] arrayString1 = colaborador.email.Split(new Char[] { '@' });
				string colaboradorLogin = arrayString1[0];
				var newData1 = new
				{
					id = colaborador.idColaborador,
					text = colaboradorLogin
				};

				datosFormateados.Add(newData1);

				var resp = new
				{
					dataFilter = datosFormateados,
					success = true
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult SolicitarTarea(string descripcion)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				Persona persona = user.persona;
				Colaborador colaborador = db.Colaborador.FirstOrDefault(x => x.persona.Secuencial == persona.Secuencial);

				SolicitudTarea solicitud = new SolicitudTarea
				{
					colaborador = colaborador,
					HoraSolicitud = DateTime.Now,
					Descripcion = descripcion,
					Aceptada = 0,
					Rechazada = 0,
					NumeroVerificador = 1
				};

				db.SolicitudTarea.Add(solicitud);
				db.SaveChanges();

				//Buscando los email de los colaboradores
				List<string> emails = (from u in db.Usuario
									   join
					  ur in db.UsuarioRol on u.Secuencial equals ur.SecuencialUsuario
									   join
r in db.Rol on ur.rol equals r
									   where u.EstaActivo == 1 && r.EstaActivo == 1 &&
											 r.Codigo == "COORDINADOR"
									   select u.Email).ToList<string>();
				emails.Add(emailUser);

				string textoHtml = "<div class=\"textoCuerpo\">Se ha ingresado una nueva solicitud de tarea.<br/>";
				textoHtml += "Solicitud por: <b>" + persona.Nombre1 + " " + persona.Apellido1 + "</b><br/>";
				textoHtml += "Fecha y hora de solicitud: <b>" + solicitud.HoraSolicitud.ToString("dd/MM/yyyy HH:mm:ss") + "</b><br/>";
				textoHtml += "Por favor entre cuanto antes al sistema Sifizplanning y gestione la solicitud,<br/></div>";

				Utiles.EnviarEmailSistema(emails.ToArray(), textoHtml, "nueva solicitud de tarea", null, "Solicitud de Tarea: " + emailUser);

				var resp = new
				{
					success = true,
					msg = "Se ha registrado correctamente la solicitud"
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult SolicitudVacacionesUsuario([System.Web.Http.FromBody] SolicitudVacacionesDTO solicitud, string rechazoVacacionesComentario = "")
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				Persona persona = user.persona;
				Colaborador colaborador = db.Colaborador.FirstOrDefault(x => x.persona.Secuencial == persona.Secuencial);

				SolicitudVacaciones solVacaciones = new SolicitudVacaciones();
				if(solicitud.ID == null)
				{
					solVacaciones.AlAnio = solicitud.AlAnio;
					solVacaciones.FechaInicioVacaciones = solicitud.FechaInicioVacaciones;
					solVacaciones.FechaFinVacaciones = solicitud.FechaFinVacaciones;
					solVacaciones.FechaPresentarseTrabajar = solicitud.FechaPresentarseTrabajar;
					solVacaciones.Cargo = solicitud.Cargo;
					solVacaciones.Empresa = solicitud.Empresa;
					solVacaciones.Cedula = solicitud.Cedula;
					solVacaciones.AniosServicio = solicitud.AniosServicio;
					solVacaciones.ApellidosNombres = persona.Nombre1 + " " + persona.Apellido1;
					solVacaciones.DiasCorresponden = solicitud.DiasCorresponden;
					solVacaciones.DiasDisfrutar = solicitud.DiasDisfrutar;
					solVacaciones.DiasPendientes = solicitud.DiasPendientes;
					solVacaciones.FechaIngresoInstitucion = solicitud.FechaIngresoInstitucion;
					solVacaciones.FechaIngresoSolicitud = solicitud.FechaIngresoSolicitud;
					solVacaciones.Observaciones = solicitud.Observaciones;
					solVacaciones.DelAnio = solicitud.DelAnio;
				}
				else
				{
					solVacaciones = db.SolicitudVacaciones.Find(solicitud.ID);
				}

				if(solicitud.Estado != null)
				{
					solVacaciones.Estado = solicitud.Estado == "APROBADA" ? 1 : 0;
				}

				List<string> emails = new List<string>();
				if(solicitud.ID == null)
				{
					db.SolicitudVacaciones.Add(solVacaciones);
					//emails.AddRange(Utiles.CorreoPorGrupoEmail("RRHH"));
					emails.Add(emailUser);
					emails.Add("galvarez@sifizsoft.com"); emails.Add("asistenterrhh@sifizsoft.com");

					string textoHtml = "<div class=\"textoCuerpo\">Se ha ingresado una nueva solicitud de vacaciones.<br/>";
					textoHtml += "Solicitada por: <b>" + persona.Nombre1 + " " + persona.Apellido1 + "</b><br/>";
					textoHtml += "Ya puede entrar al módulo de RRHH y gestionar la solicitud<br/></div>";

					Utiles.EnviarEmailSistema(emails.ToArray(), textoHtml, "solicitud de vacaciones");
				}
				else
				{
					var per = db.Persona.Where(s => s.Nombre1 + " " + s.Apellido1 == solVacaciones.ApellidosNombres).First();
					Colaborador col = db.Colaborador.FirstOrDefault(x => x.persona.Secuencial == per.Secuencial);

					DateTime fechaDesde = solVacaciones.FechaInicioVacaciones;
					DateTime fechaHasta = solVacaciones.FechaFinVacaciones;
					if(solicitud.Estado == "APROBADA")
					{
						if(fechaDesde > fechaHasta)
						{
							throw new Exception("Error, la fecha de fin no debe ser menor a la fecha de inicio.");
						}

						while(fechaDesde <= fechaHasta)
						{
							Vacaciones diaVacaciones = new Vacaciones
							{
								colaborador = col,
								Fecha = fechaDesde
							};
							db.Vacaciones.Add(diaVacaciones);
							fechaDesde = fechaDesde.AddDays(1);
						}

						//emails.AddRange(Utiles.CorreoPorGrupoEmail("RRHH"));
						emails.Add(per.usuario.First().Email);
						emails.Add("galvarez@sifizsoft.com"); emails.Add("asistenterrhh@sifizsoft.com");

						string textoHtml = "<div class=\"textoCuerpo\">NOTIFICACIÓN VAPER: <br/>" +
										@"Le informamos que las vacaciones solicitadas por usted han sido aprobadas,
                                los datos son los siguientes:<br/>
                                Fecha de Inicio: " + solVacaciones.FechaInicioVacaciones + @"<br/>
                                Fecha de Fin: " + solVacaciones.FechaFinVacaciones + @"<br/>
                                Aprobado por: " + solVacaciones.ApellidosNombres + @"</div><br/><br/>
                                Esperamos disfrute sus vacaciones junto a su familia y amigos...";

						Utiles.EnviarEmailSistema(emails.ToArray(), textoHtml, "Vacaciones Aprobadas");
					}
					else if(solicitud.Estado == "RECHAZADA")
					{
						if(fechaDesde > fechaHasta)
						{
							throw new Exception("Error, la fecha de fin no debe ser menor a la fecha de inicio.");
						}

						while(fechaDesde <= fechaHasta)
						{
							Vacaciones vaca = db.Vacaciones.Where(x => x.SecuencialColaborador == col.Secuencial && x.Fecha == fechaDesde).FirstOrDefault();
							if(vaca != null)
							{
								db.Vacaciones.Remove(vaca);
							}

							fechaDesde = fechaDesde.AddDays(1);
						}

						//emails.AddRange(Utiles.CorreoPorGrupoEmail("RRHH"));
						//emails.Add(emailUser);
						emails.Add(per.usuario.First().Email);
						emails.Add("galvarez@sifizsoft.com"); emails.Add("asistenterrhh@sifizsoft.com");

						string textoHtml = "<div class=\"textoCuerpo\">Se ha rechazado la solicitud de vacaciones.<br/>";
						textoHtml += "Solicitada por: <b>" + solVacaciones.ApellidosNombres + "</b><br/>";
						textoHtml += "Comentario de rechazo: <b>" + rechazoVacacionesComentario + "</b><br/>";
						textoHtml += "Puede contactar con RRHH para más información<br/></div>";

						Utiles.EnviarEmailSistema(emails.ToArray(), textoHtml, "Vacaciones Rechazadas");
					}
				}

				db.SaveChanges();

				var resp = new
				{
					success = true,
					msg = "Se ha registrado correctamente la solicitud"
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message,
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult SolicitudPermisosUsuario([System.Web.Http.FromBody] SolicitudPermisosDTO solicitud, string rechazoPermisoComentario = "")
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				Persona persona = user.persona;
				Colaborador colaborador = db.Colaborador.FirstOrDefault(x => x.persona.Secuencial == persona.Secuencial);

				SolicitudPermisos solPermisos = new SolicitudPermisos();
				if(solicitud.ID == null)
				{
					solPermisos.ApellidosNombres = persona.Nombre1 + " " + persona.Apellido1;
					solPermisos.Area = solicitud.Area;
					solPermisos.Cargo = solicitud.Cargo;
					solPermisos.Cedula = solicitud.Cedula;
					solPermisos.Comida = solicitud.Comida == "True" ? true : false;
					solPermisos.Empresa = solicitud.Empresa;
					solPermisos.FechaDesde = solicitud.FechaDesde;
					solPermisos.FechaHasta = solicitud.FechaHasta;
					solPermisos.FechaIngresoSolicitud = solicitud.FechaIngresoSolicitud;
					solPermisos.HoraRetorno = DateTime.ParseExact(solicitud.HoraRetorno, @"h:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
					solPermisos.HoraSalida = DateTime.ParseExact(solicitud.HoraSalida, @"h:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
					solPermisos.Jefe = solicitud.Jefe;
					solPermisos.Matrimonio = solicitud.Matrimonio == "True" ? true : false;
					solPermisos.Motivo = solicitud.Motivo;
					solPermisos.Otros = solicitud.Otros == "True" ? true : false;
					solPermisos.Paternidad = solicitud.Paternidad == "True" ? true : false;
					solPermisos.Personal = solicitud.Personal == "True" ? true : false;
				}
				else
				{
					solPermisos = db.SolicitudPermisos.Find(solicitud.ID);
				}

				if(solicitud.Estado != null)
				{
					solPermisos.Estado = solicitud.Estado == "APROBADA" ? 1 : 0;
				}

				List<string> emails = new List<string>();
				if(solicitud.ID == null)
				{
					db.SolicitudPermisos.Add(solPermisos);
					//emails.AddRange(Utiles.CorreoPorGrupoEmail("RRHH"));
					emails.Add(emailUser);
					emails.Add("galvarez@sifizsoft.com"); emails.Add("asistenterrhh@sifizsoft.com");

					string textoHtml = "<div class=\"textoCuerpo\">Se ha ingresado una nueva solicitud de permiso.<br/>";
					textoHtml += "Solicitada por: <b>" + persona.Nombre1 + " " + persona.Apellido1 + "</b><br/>";
					textoHtml += "Ya puede entrar al módulo de RRHH y gestionar la solicitud<br/></div>";

					Utiles.EnviarEmailSistema(emails.ToArray(), textoHtml, "solicitud de permisos");
				}
				else
				{
					var per = db.Persona.Where(s => s.Nombre1 + " " + s.Apellido1 == solPermisos.ApellidosNombres).First();
					Colaborador col = db.Colaborador.FirstOrDefault(x => x.persona.Secuencial == per.Secuencial);

					DateTime fechaDesde = solPermisos.FechaDesde;
					DateTime fechaHasta = solPermisos.FechaHasta;
					fechaDesde = fechaDesde.AddMinutes(solPermisos.HoraSalida.TotalMinutes);
					fechaHasta = fechaHasta.AddMinutes(solPermisos.HoraRetorno.TotalMinutes);

					if(solicitud.Estado == "APROBADA")
					{
						if(fechaHasta <= fechaDesde)
						{
							throw new Exception("Error, la fecha de inicio del permiso debe ser menor que la fecha fin del permiso");
						}

						//Buscando no se solapen los permisos
						int cant = (from p in db.Permiso
									where
										   ((p.FechaInicio <= fechaDesde && p.FechaFin > fechaDesde) ||
										   (p.FechaInicio < fechaHasta && p.FechaFin > fechaHasta) ||
										   (p.FechaInicio >= fechaDesde && p.FechaFin <= fechaHasta)) &&
										   p.SecuencialEstadoPermiso != 3 &&
										   p.SecuencialColaborador == col.Secuencial
									select p).Count();

						if(cant > 1)
						{
							throw new Exception("Error, las fechas de este permiso se solapan con las de otro");
						}

						int tipoPermiso = solPermisos.Personal ? 1 : solPermisos.Matrimonio ? 2 : solPermisos.Comida ? 4 : solPermisos.Paternidad ? 5 : 2;
						Permiso permiso = new Permiso
						{
							SecuencialTipoPermiso = tipoPermiso,
							SecuencialEstadoPermiso = 1,
							SecuencialColaborador = col.Secuencial,
							FechaInicio = fechaDesde,
							FechaFin = fechaHasta,
							Motivo = solPermisos.Motivo
						};

						db.Permiso.Add(permiso);
						db.SaveChanges();

						//emails.AddRange(Utiles.CorreoPorGrupoEmail("RRHH"));
						//emails.Add(emailUser);
						emails.Add(per.usuario.First().Email);
						emails.Add("galvarez@sifizsoft.com"); emails.Add("asistenterrhh@sifizsoft.com");

						string textoHtml = "<div class=\"textoCuerpo\">Se ha aprobado la solicitud de permiso.<br/>";
						textoHtml += "Solicitada por: <b>" + solPermisos.ApellidosNombres + "</b><br/>";
						textoHtml += "Puede contactar con RRHH para más información<br/></div>";

						Utiles.EnviarEmailSistema(emails.ToArray(), textoHtml, "Permiso Aprobado");
					}
					else if(solicitud.Estado == "RECHAZADA")
					{
						if(fechaHasta <= fechaDesde)
						{
							throw new Exception("Error, la fecha de inicio del permiso debe ser menor que la fecha fin del permiso");
						}

						int tipoPermiso = solPermisos.Personal ? 1 : solPermisos.Matrimonio ? 2 : solPermisos.Comida ? 4 : solPermisos.Paternidad ? 5 : 2;
						Permiso permiso = db.Permiso.Where(s =>
							s.SecuencialTipoPermiso == tipoPermiso &&
							s.SecuencialEstadoPermiso == 1 &&
							s.SecuencialColaborador == col.Secuencial &&
							s.FechaInicio == fechaDesde &&
							s.FechaFin == fechaHasta &&
							s.Motivo == solPermisos.Motivo
						).FirstOrDefault();

						if(permiso != null)
						{
							db.Permiso.Remove(permiso);
							db.SaveChanges();
						}

						//emails.AddRange(Utiles.CorreoPorGrupoEmail("RRHH"));
						//emails.Add(emailUser);
						emails.Add(per.usuario.First().Email);
						emails.Add("galvarez@sifizsoft.com"); emails.Add("asistenterrhh@sifizsoft.com");

						string textoHtml = "<div class=\"textoCuerpo\">Se ha rechazado la solicitud de permiso.<br/>";
						textoHtml += "Solicitada por: <b>" + solPermisos.ApellidosNombres + "</b><br/>";
						textoHtml += "Comentario de rechazo: <b>" + rechazoPermisoComentario + "</b><br/>";
						textoHtml += "Puede contactar con RRHH para más información<br/></div>";

						Utiles.EnviarEmailSistema(emails.ToArray(), textoHtml, "Permiso Rechazado");

					}
				}

				db.SaveChanges();

				var resp = new
				{
					success = true,
					msg = "Se ha registrado correctamente la solicitud"
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarComentariosActividad(int idActividad)
		{
			try
			{
				TareaActividadRealizada tareaActividad = db.TareaActividadRealizada.Find(idActividad);
				if(tareaActividad == null)
				{
					throw new Exception("Actividad no encontrada.");
				}
				List<object> comentarios = (from c in db.ComentarioTarea
											join
					u in db.Usuario on c.usuario equals u
											where c.SecuencialTareaActividad == idActividad && c.EstaActivo == 1
											orderby c.Fecha descending
											select new
											{
												id = c.Secuencial,
												usuario = u.Email,
												texto = c.Descripcion,
												fecha = c.Fecha,
												importancia = (c.Importancia.ToLower() == "normal") ? "success" : ((c.Importancia.ToLower() == "importante") ? "warning" : "danger"),
											}).ToList<object>();

				var resp = new
				{
					success = true,
					comentarios = comentarios
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult AdicionarComentarioActividad(int idActividad, string descripcion, string importancia = "Normal")
		{
			try
			{
				TareaActividadRealizada tareaActividad = db.TareaActividadRealizada.Find(idActividad);
				if(tareaActividad == null)
				{
					throw new Exception("No se encontró la actividad.");
				}

				DateTime diaTarea = new System.DateTime(tareaActividad.tarea.FechaInicio.Year, tareaActividad.tarea.FechaInicio.Month, tareaActividad.tarea.FechaInicio.Day);
				DateTime fechaInicioTareas = diaTarea.AddMinutes(30 + (8 * 60));
				if(tareaActividad.tarea.FechaInicio.Date != DateTime.Today)
				{
					if(tareaActividad.tarea.FechaInicio.Date >= fechaInicioTareas)
						throw new Exception("La tarea no pertenece al día de hoy, no puede adicionar comentarios.");
				}

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				ComentarioTarea comentario = new ComentarioTarea
				{
					SecuencialUsuario = user.Secuencial,
					SecuencialTareaActividad = idActividad,
					Descripcion = descripcion,
					Fecha = DateTime.Now,
					EstaActivo = 1,
					Importancia = importancia
				};
				db.ComentarioTarea.Add(comentario);
				db.SaveChanges();

				string textoComentarioGeneral = "<b>TAREA</b>-Cliente: <b>" + tareaActividad.tarea.cliente.Descripcion + "</b>, Actividad: <b>" + tareaActividad.actividadRealizada.Descripcion + "</b>, ";

				if(tareaActividad.tarea.ticketTarea.Count > 0)
				{
					textoComentarioGeneral += "Ticket: <b>" + string.Format("{0:000000}", tareaActividad.tarea.ticketTarea.First().ticket.Secuencial) + "</b>, ";
				}

				if(tareaActividad.tarea.entregableMotivoTrabajo != null)
				{
					textoComentarioGeneral += "Motivo de Trabajo: ";
					textoComentarioGeneral += "<b>" + tareaActividad.tarea.entregableMotivoTrabajo.motivoTrabajo.Descripcion + "</b>, ";
				}

				//Adicionando los comentarios a comentarios generales
				ComentarioGeneral comentarioGeneral = new ComentarioGeneral
				{
					FechaHora = DateTime.Now,
					usuario = user,
					TipoComentario = "TAREA",
					Comentario = textoComentarioGeneral + descripcion,
					Importancia = importancia
				};
				db.ComentarioGeneral.Add(comentarioGeneral);
				db.SaveChanges();

				//Llamar el signalR
				Websocket.getInstance().NuevosComentarios();
				if(importancia.ToLower() == "muy importante")
					Websocket.getInstance().NuevoComentarioMuyImportante();

				List<object> comentarios = (from c in db.ComentarioTarea
											join
												u in db.Usuario on c.usuario equals u
											where c.SecuencialTareaActividad == idActividad && c.EstaActivo == 1
											orderby c.Fecha descending
											select new
											{
												id = c.Secuencial,
												usuario = u.Email,
												texto = c.Descripcion,
												fecha = c.Fecha,
												importancia = (c.Importancia.ToLower() == "normal") ? "success" : ((c.Importancia.ToLower() == "importante") ? "warning" : "danger"),
											}).ToList<object>();

				var resp = new
				{
					success = true,
					comentarios = comentarios
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult EliminarComentarioActividad(int idComentario)
		{
			try
			{
				ComentarioTarea comentario = db.ComentarioTarea.Find(idComentario);
				if(comentario == null)
				{
					throw new Exception("No se encontró el comentario.");
				}
				int idActividad = comentario.SecuencialTareaActividad;

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				//Comprovando que el usuario es el mismo que entró el comentario
				if(user.Secuencial != comentario.SecuencialUsuario)
				{
					throw new Exception("Solo el usuario que creó el comentario puede eliminarlo.");
				}

				comentario.EstaActivo = 0;
				db.SaveChanges();

				List<object> comentarios = (from c in db.ComentarioTarea
											join
												u in db.Usuario on c.usuario equals u
											where c.SecuencialTareaActividad == idActividad && c.EstaActivo == 1
											orderby c.Fecha descending
											select new
											{
												id = c.Secuencial,
												usuario = u.Email,
												texto = c.Descripcion,
												fecha = c.Fecha,
												importancia = (c.Importancia.ToLower() == "normal") ? "success" : ((c.Importancia.ToLower() == "importante") ? "warning" : "danger"),
											}).ToList<object>();

				var resp = new
				{
					success = true,
					comentarios = comentarios
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult VerDatosDesarrolloTicket(int idTarea)
		{
			try
			{
				Tarea tarea = db.Tarea.Find(idTarea);
				if(tarea == null)
				{
					throw new Exception("La tarea no existe.");
				}
				Ticket ticket = tarea.ticketTarea.Where(x => x.EstaActiva == 1).FirstOrDefault().ticket;
				if(ticket == null)
				{
					throw new Exception("No existe el ticket asociado a la tarea.");
				}

				var resp = new
				{
					success = true,
					detalleTicket = ticket.Detalle,
					textoResolucion = ticket.ticket_resolucion != null ? ticket.ticket_resolucion.Descripcion : "",
					adjuntosTicket = ticket.adjuntoTicket.Select(x => x.Url).ToList()
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

		//ESTIMACIONES DE LOS USUARIOS
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult EstimacionesUsuario(int start, int lenght, string filtro = "", bool todos = false)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Persona persona = user.persona;
				Colaborador colab = persona.colaborador.FirstOrDefault();

				if(colab == null)
				{
					throw new Exception("No se encontró el colaborador, por favor contacte el administrador del sistema.");
				}

				var estimacionesUsuario = (from est in db.EstimacionTicket
										   where est.SecuencialColaborador == colab.Secuencial
										   orderby est.Secuencial descending
										   select new
										   {
											   id = est.Secuencial,
											   ticket = est.SecuencialTicket,
											   cliente = est.ticket.persona_cliente.cliente.Descripcion,
											   prioridad = est.ticket.prioridadTicket.Codigo,
											   asunto = est.ticket.Asunto,
											   detalle = est.ticket.Detalle,
											   fechaLimite = est.FechaLimite,
											   estado = est.EstimacionTerminada == 1 ? "ESTIMADO" : "POR ESTIMAR",
											   adjuntos = (from adj in db.AdjuntoTicket
														   where adj.SecuencialTicket == est.SecuencialTicket
														   select new
														   {
															   id = adj.Secuencial,
															   url = adj.Url
														   }).ToList()
										   }).ToList();

				if(!todos)
				{
					estimacionesUsuario = estimacionesUsuario.Where(s => s.estado == "POR ESTIMAR").ToList();
				}

				if(filtro != "")
				{
					estimacionesUsuario = estimacionesUsuario.Where(x =>
																	x.ticket.ToString().Contains(filtro) ||
																	x.cliente.ToString().ToLower().Contains(filtro.ToLower()) ||
																	x.prioridad.ToString().ToLower().Contains(filtro.ToLower()) ||
																	x.asunto.ToString().ToLower().Contains(filtro.ToLower()) ||
																	x.detalle.ToString().ToLower().Contains(filtro.ToLower()) ||
																	x.estado.ToString().ToLower().Contains(filtro.ToLower())
																).ToList();
				}

				int total = estimacionesUsuario.Count();
				estimacionesUsuario = estimacionesUsuario.Skip(start).Take(lenght).ToList();

				var result = new
				{
					success = true,
					total = total,
					estimaciones = estimacionesUsuario
				};
				return Json(result);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}

		//INCIDENCIAS DE LOS USUARIOS
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult IncidenciasUsuario(int start, int lenght, string filtro = "", bool todos = false)
		{


			try
			{
				var incidenciasUsuario = (from inc in db.Incidencias
										  join md in db.Modulo on inc.SecuencialModulo equals md.Secuencial
										  select new
										  {
											  version = inc.Version,
											  modulo = md.Descripcion,
											  incidente = inc.Incidente,
											  adjunto = inc.Adjunto
										  }).ToList();

				int total = incidenciasUsuario.Count();
				incidenciasUsuario = incidenciasUsuario.Skip(start).Take(lenght).ToList();

				var result = new
				{
					success = true,
					total = total,
					incidencias = incidenciasUsuario
				};
				return Json(result);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}



		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarTipoModulo()
		{
			var datos = (from tm in db.Modulo
						 orderby tm.Descripcion
						 select new
						 {
							 id = tm.Secuencial,
							 nombre = tm.Descripcion
						 }).ToList();

			return Json(new
			{
				success = true,
				tipoModulo = datos
			});
		}


		//Guardar modal nuevas incidencias
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult GuardarIncidencia(string version, string modulo, string incidente, HttpPostedFileBase[] adjuntos = null)
		{
			try
			{
				var s = new JavaScriptSerializer();
				var Url = "";
				foreach(var adj in adjuntos.Where(adj => adj != null))
				{
					string extFile = Path.GetExtension(adj.FileName);
					string newNameFile = Utiles.RandomString(10) + extFile;
					newNameFile = System.IO.Path.GetRandomFileName() + extFile;
					string path = Path.Combine(Server.MapPath("~/Web/resources/incidencias"), newNameFile);
					adj.SaveAs(path);

					Url = "/resources/incidencias" + "/" + newNameFile;
					break;
				}

				int moduloid = int.Parse(modulo);
				Incidencias nuevaIncidencia = new Incidencias
				{
					Version = version,
					SecuencialModulo = moduloid,
					Incidente = incidente,
					Adjunto = Url
				};

				db.Incidencias.Add(nuevaIncidencia);
				db.SaveChanges();

				return Json(new
				{
					success = true,
					msg = "Se ha realizado la operación correctamente."
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

		//RECURSOS DE LOS USUARIOS
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult RecursosUsuario(int start, int lenght, string filtro = "", bool todos = false)
		{


			try
			{
				var recursosUsuario = (from rec in db.Recursos
									   join catrec in db.CategoriaRecursos on rec.SecuencialCategoriaRecursos equals catrec.Secuencial
									   select new
										{
											titulo = rec.Titulo,
											detalle = rec.Detalle,
											fecha = rec.Fecha,
											categoria = rec.CategoriaRecursos.Descripcion,
											adjunto = rec.Adjunto
								}).ToList();

				int total = recursosUsuario.Count();
				recursosUsuario = recursosUsuario.Skip(start).Take(lenght).ToList();

				var result = new
				{
					success = true,
					total = total,
					recursos = recursosUsuario
				};
				return Json(result);
			}
			catch (Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}



		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarCategoriaRecurso()
		{
			var datos = (from cat in db.CategoriaRecursos
						 orderby cat.Descripcion
						 select new
						 {
							 id = cat.Secuencial,
							 nombre = cat.Descripcion
						 }).ToList();
			 
			return Json(new
			{
				success = true,
				categoriaRecursos = datos
			});
		}


		//Guardar modal nuevos recursos
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult GuardarRecurso(string titulo, string detalle, DateTime fecha, int categoria, HttpPostedFileBase[] adjuntos = null)
		{
			try
			{
				var s = new JavaScriptSerializer();
				var Url = "";
				foreach (var adj in adjuntos.Where(adj => adj != null))
				{
					string extFile = Path.GetExtension(adj.FileName);
					string newNameFile = Utiles.RandomString(10) + extFile;
					newNameFile = System.IO.Path.GetRandomFileName() + extFile;
					string path = Path.Combine(Server.MapPath("~/Web/resources/recursos"), newNameFile);
					adj.SaveAs(path);

					Url = "/resources/recursos" + "/" + newNameFile;
					break;
				}
				Recursos nuevoRecurso = new Recursos
				{
					Titulo = titulo,
					Detalle = detalle,
					Fecha = fecha,
					SecuencialCategoriaRecursos = categoria,
					Adjunto = Url
				};

				db.Recursos.Add(nuevoRecurso);
				db.SaveChanges();

				return Json(new
				{
					success = true,
					msg = "Se ha realizado la operación correctamente."
				});
			}
			catch (Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
				});
			}
		}



		//ESTIMACIONES DE LOS USUARIOS
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult ProyectosUsuario(int start, int lenght, string filtro = "")
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Persona persona = user.persona;
				Colaborador colab = persona.colaborador.FirstOrDefault();

				if(colab == null)
				{
					throw new Exception("No se encontró el colaborador, por favor contacte el administrador del sistema.");
				}

				var proyectosUsuario = (from c in db.ClienteAuxiliar
										where c.EstaActivo == 1
										orderby c.cliente.Codigo
										select new
										{
											nombre = c.cliente.Codigo,
											descripcion = c.cliente.Descripcion,
											versionDesarrollo = c.versionDesarrollo.Descripcion,
											repositorio = c.repositorio.Descripcion,
											ubicacion = c.ubicacion.Nombre,
											tieneCodigoFuente = c.TieneCodigoFuente == 1 ? "SI" : "NO",
											responsableAcceso = c.responsableAcceso.Nombre,
											responsableCodigo = c.responsableCodigo.Nombre,
											responsablePublicacion = c.responsablePublicacion.Nombre,
											versionBaseDatos = c.versionBaseDatos.Descripcion,
											lider = db.Colaborador.Where(s => s.Secuencial == c.SecuencialLiderProyecto).Select(e => e.persona.Nombre1 + " " + e.persona.Apellido1).FirstOrDefault(),
											gestor = (from ges in db.GestorServicios
													  join col in db.Colaborador on ges.SecuencialColaborador equals col.Secuencial
													  join per in db.Persona on col.SecuencialPersona equals per.Secuencial
													  where ges.SecuencialCliente == c.cliente.Secuencial
													  select per.Nombre1 + " " + per.Apellido1
													  ).FirstOrDefault(),
											tieneSolucionProxies = c.TieneSolucionProxies == 1 ? "SI" : "NO",
											fechaProduccion = c.FechaProduccion
										}).ToList();

				var proyectos = proyectosUsuario.Select(c => new
				{
					nombre = c.nombre,
					descripcion = c.descripcion,
					versionDesarrollo = c.versionDesarrollo,
					repositorio = c.repositorio,
					ubicacion = c.ubicacion,
					tieneCodigoFuente = c.tieneCodigoFuente,
					responsableAcceso = c.responsableAcceso,
					responsableCodigo = c.responsableCodigo,
					responsablePublicacion = c.responsablePublicacion,
					versionBaseDatos = c.versionBaseDatos,
					lider = c.lider,
					gestor = c.gestor,
					tieneSolucionProxies = c.tieneSolucionProxies,
					fechaProduccion = c.fechaProduccion.ToString("dd/MM/yyy")
				}).ToList();

				if(filtro != "")
				{
					proyectos = proyectos.Where(x =>
					  x.nombre.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.descripcion.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.versionDesarrollo.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.repositorio.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.ubicacion.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.tieneCodigoFuente.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.responsableAcceso.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.responsableCodigo.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.responsablePublicacion.ToString().ToLower().Contains(filtro.ToLower()) ||
					  x.versionBaseDatos.ToString().ToLower().Contains(filtro.ToLower())
					  ).ToList();
				}

				int total = proyectos.Count();
				proyectos = proyectos.Skip(start).Take(lenght).ToList();

				var result = new
				{
					success = true,
					total = total,
					proyectos = proyectos
				};
				return Json(result);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}

		//ESTIMACIONES DE LOS USUARIOS
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult TicketsUsuario(string filtro = "", bool todos = false)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				Persona persona = user.persona;

				//Persona_Cliente personaCliente = persona.persona_cliente;
				//Cliente cliente = personaCliente.cliente;

				var ticketsParcial = (from t in db.Ticket
									  join
										  et in db.EstadoTicket on t.estadoTicket equals et
									  join
										  pr in db.PrioridadTicket on t.prioridadTicket equals pr
									  join
										  ct in db.CategoriaTicket on t.categoriaTicket equals ct
									  join
										  pc in db.Persona_Cliente on t.persona_cliente equals pc
									  join
										  pa in db.ProximaActividad on t.proximaActividad equals pa
									  orderby t.Fecha descending
									  select new
									  {
										  numero = t.Secuencial,
										  cliente = pc.cliente.Descripcion,
										  fecha = t.FechaCreado,
										  asunto = t.Asunto,
										  seFactura = t.SeFactura,
										  facturado = t.Facturado,
										  asignado = "",
										  prioridad = pr.Codigo,
										  categoria = ct.Codigo,
										  estado = et.Codigo,
										  proximaActividad = pa.Codigo,
										  clase = (t.estadoTicket.Codigo == "CERRADO") ? "fondoCerrado" :
									   (t.estadoTicket.Codigo == "ANULADO") ? "fondoAnulado" :
									   (t.estadoTicket.Codigo == "RECHAZADO") ? "fondoAnulado" :
									   (t.estadoTicket.Codigo == "ESPERANDO LLAMADA" || t.estadoTicket.Codigo == "ESPERANDO RESPUESTA") ? "fondoCliente" :
									   (t.estadoTicket.Codigo == "ABIERTO") ? "fondoAbierto" : "fondoDesarrollo"
									  }).ToList();

				var tickets = ticketsParcial;

				if(todos == false)
				{
					tickets = tickets.Where(x => x.estado != "CERRADO" && x.estado != "ANULADO" && x.estado != "RECHAZADO").ToList();
				}

				var asignados = (from t in db.Ticket
								 join ttar in db.TicketTarea on t.Secuencial equals ttar.SecuencialTicket
								 join tar in db.Tarea on ttar.SecuencialTarea equals tar.Secuencial
								 join c in db.Colaborador on tar.SecuencialColaborador equals c.Secuencial
								 join p in db.Persona on c.SecuencialPersona equals p.Secuencial
								 where (/*p.Secuencial == persona.Secuencial && */ttar.EstaActiva == 1)
								 orderby tar.FechaInicio descending
								 select new
								 {
									 numero = t.Secuencial,
									 persona = p.Secuencial
								 }).ToList();

				asignados = asignados.GroupBy(g => g.numero).Select(g => g.First()).ToList();
				asignados = asignados.Where(s => s.persona == persona.Secuencial).ToList();

				var ticketsAsg = (from t in tickets
								  from a in asignados
								  where t.numero == a.numero
								  orderby t.fecha descending
								  select t).ToList();

				if(filtro != "")
				{
					ticketsAsg = ticketsAsg.Where(x =>
											   x.numero.ToString().ToLower().Contains(filtro.ToLower()) ||
											   x.cliente.ToLower().Contains(filtro.ToLower()) ||
											   x.fecha.ToString().ToLower().Contains(filtro.ToLower()) ||
											   x.prioridad.ToLower().Contains(filtro.ToLower()) ||
											   x.categoria.ToLower().Contains(filtro.ToLower()) ||
											   x.asunto.ToLower().Contains(filtro.ToLower()) ||
											   x.asignado.ToLower().Contains(filtro.ToLower()) ||
											   x.estado.ToLower().Contains(filtro.ToLower())
											).ToList();
				}

				var resp = new
				{
					success = true,
					tickets = ticketsAsg
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult VacacionesUsuario()
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Persona persona = user.persona;
				Colaborador colab = persona.colaborador.FirstOrDefault();

				if(colab == null)
				{
					throw new Exception("No se encontró el colaborador, por favor contacte el administrador del sistema.");
				}

				var solicitudes = db.SolicitudVacaciones
					.Where(s => s.ApellidosNombres == persona.Nombre1 + " " + persona.Apellido1)
					.ToList();

				List<SolicitudVacacionesDTO> solicitudesVacacionesDTO = new List<SolicitudVacacionesDTO>();

				foreach(var solicitud in solicitudes)
				{
					SolicitudVacacionesDTO solVacaciones = new SolicitudVacacionesDTO();
					solVacaciones.ID = solicitud.Secuencial;
					solVacaciones.AlAnio = solicitud.AlAnio;
					solVacaciones.FechaInicioVacaciones = solicitud.FechaInicioVacaciones;
					solVacaciones.FechaFinVacaciones = solicitud.FechaFinVacaciones;
					solVacaciones.FechaPresentarseTrabajar = solicitud.FechaPresentarseTrabajar;
					solVacaciones.Cargo = solicitud.Cargo;
					solVacaciones.Empresa = solicitud.Empresa;
					solVacaciones.Cedula = solicitud.Cedula;
					solVacaciones.AniosServicio = solicitud.AniosServicio;
					solVacaciones.ApellidosNombres = persona.Nombre1 + " " + persona.Apellido1;
					solVacaciones.DiasCorresponden = solicitud.DiasCorresponden;
					solVacaciones.DiasDisfrutar = solicitud.DiasDisfrutar;
					solVacaciones.DiasPendientes = solicitud.DiasPendientes;
					solVacaciones.FechaIngresoInstitucion = solicitud.FechaIngresoInstitucion;
					solVacaciones.FechaIngresoSolicitud = solicitud.FechaIngresoSolicitud;
					solVacaciones.Observaciones = solicitud.Observaciones;
					solVacaciones.DelAnio = solicitud.DelAnio;
					solVacaciones.Estado = solicitud.Estado != null ? solicitud.Estado == 1 ? "APROBADA" : "RECHAZADA" : "PENDIENTE";
					solicitudesVacacionesDTO.Add(solVacaciones);
				}

				var result = new
				{
					success = true,
					solicitudes = solicitudesVacacionesDTO
				};
				return Json(result);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult PermisosUsuario()
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Persona persona = user.persona;
				Colaborador colab = persona.colaborador.FirstOrDefault();

				if(colab == null)
				{
					throw new Exception("No se encontró el colaborador, por favor contacte el administrador del sistema.");
				}

				var solicitudes = db.SolicitudPermisos
					.Where(s => s.ApellidosNombres == persona.Nombre1 + " " + persona.Apellido1)
					.ToList();

				List<SolicitudPermisosDTO> solicitudesPermisosDTO = new List<SolicitudPermisosDTO>();

				foreach(var solicitud in solicitudes)
				{
					SolicitudPermisosDTO solPermisos = new SolicitudPermisosDTO();
					solPermisos.ID = solicitud.Secuencial;
					solPermisos.ApellidosNombres = solicitud.ApellidosNombres;
					solPermisos.Area = solicitud.Area;
					solPermisos.Cargo = solicitud.Cargo;
					solPermisos.Cedula = solicitud.Cedula;
					solPermisos.Comida = solicitud.Comida ? "SI" : "NO";
					solPermisos.Empresa = solicitud.Empresa;
					solPermisos.FechaDesde = solicitud.FechaDesde;
					solPermisos.FechaHasta = solicitud.FechaHasta;
					solPermisos.FechaIngresoSolicitud = solicitud.FechaIngresoSolicitud;
					solPermisos.HoraRetorno = solicitud.HoraRetorno.ToString();
					solPermisos.HoraSalida = solicitud.HoraSalida.ToString();
					solPermisos.Jefe = solicitud.Jefe;
					solPermisos.Matrimonio = solicitud.Matrimonio ? "SI" : "NO";
					solPermisos.Motivo = solicitud.Motivo;
					solPermisos.Otros = solicitud.Otros ? "SI" : "NO";
					solPermisos.Paternidad = solicitud.Paternidad ? "SI" : "NO";
					solPermisos.Personal = solicitud.Personal ? "SI" : "NO";
					solPermisos.Estado = solicitud.Estado != null ? solicitud.Estado == 1 ? "APROBADA" : "RECHAZADA" : "PENDIENTE";
					solicitudesPermisosDTO.Add(solPermisos);
				}

				var result = new
				{
					success = true,
					solicitudes = solicitudesPermisosDTO
				};
				return Json(result);
			}
			catch(Exception e)
			{
				var result = new
				{
					success = false,
					msg = e.Message
				};
				return Json(result);
			}
		}

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DatosEstimacionUsuario(int idEstimacion)
		{
			try
			{
				EstimacionTicket estimacionTicket = db.EstimacionTicket.Find(idEstimacion);
				if(estimacionTicket != null)
				{
					var detallesEstimacion = (from de in db.DetalleEstimacionTicket
											  join et in db.EntregableDetalleEstimacion on de.SecuencialEntregableEstimacion equals et.Secuencial
											  where de.SecuencialEstimacionTicket == idEstimacion
											  group et by new { et.Secuencial, et.Nombre } into g
											  select new
											  {
												  id = g.Key.Secuencial,
												  nombre = g.Key.Nombre.ToUpper(),
												  detalles = (from d in db.DetalleEstimacionTicket
															  where d.SecuencialEntregableEstimacion == g.Key.Secuencial
															  select new
															  {
																  detalle = d.Detalle,
																  tiempoEstimacion = d.TiempoEstimacion,
																  tiempoDesarrollo = d.TiempoDesarrollo ?? 0,
																  tiempoPrueba = d.TiempoPrueba ?? 0,
																  tiempoQA = d.TiempoQA ?? 0,
																  nivel = db.NivelColaborador.Where(s => s.Secuencial == d.SecuencialNivelColaborador).FirstOrDefault().Codigo,
															  }).ToList()
											  }).ToList();

					var tiempoTotal = (from de in detallesEstimacion
									   from d in de.detalles
									   select d.tiempoDesarrollo + d.tiempoPrueba).Sum();

					var resp = new
					{
						success = true,
						detallesEstimacion = detallesEstimacion,
						tiempoTotal = tiempoTotal,
						entregables = estimacionTicket.EntregablesAdicionales,
						informacion = estimacionTicket.InformacionComplementaria,
						estimacionTerminada = estimacionTicket.EstimacionTerminada
					};
					return Json(resp);
				}
				else
				{
					var resp = new
					{
						success = true,
						detallesEstimacion = new List<object>(),
						tiempoTotal = 0,
						entregables = "",
						informacion = "",
						estimacionTerminada = 0
					};
					return Json(resp);
				}
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DatosEntregabelEstimacionUsuario(int idEntregableEstimacion)
		{
			try
			{
				EntregableDetalleEstimacion entregable = db.EntregableDetalleEstimacion.Where(s => s.Secuencial == idEntregableEstimacion).FirstOrDefault();
				var detalles = (from d in db.DetalleEstimacionTicket
								where d.SecuencialEntregableEstimacion == idEntregableEstimacion
								select new
								{
									detalle = d.Detalle,
									tiempoEstimacion = d.TiempoEstimacion,
									tiempoDesarrollo = d.TiempoDesarrollo ?? 0,
									tiempoPrueba = d.TiempoPrueba ?? 0,
									tiempoQA = d.TiempoQA ?? 0,
									nivel = d.SecuencialNivelColaborador,
								}).ToList();

				var resp = new
				{
					success = true,
					detalles = detalles,
					entregable = new { id = entregable.Secuencial, nombre = entregable.Nombre }
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult GuardarEstimacionUsuario(int idEstimacion, string entregables, string informacionComplementaria)
		{
			try
			{
				EstimacionTicket estimacionTicket = db.EstimacionTicket.Find(idEstimacion);
				if(estimacionTicket == null)
				{
					throw new Exception("No se encontró la estimación del usuario");
				}

				if(estimacionTicket.EstimacionTerminada == 1)
				{
					estimacionTicket.EntregablesAdicionales = entregables.Trim();
					estimacionTicket.InformacionComplementaria = informacionComplementaria.Trim();

					int cantidadEstimacionesSinTerminar = (from et in db.EstimacionTicket
														   where et.EstaActiva == 1 && et.SecuencialTicket == estimacionTicket.SecuencialTicket &&
																 et.EstimacionTerminada == 0
														   select new
														   {
															   et.Secuencial
														   }).Count();

					if(cantidadEstimacionesSinTerminar == 0)
					{
						Ticket ticket = estimacionTicket.ticket;
						ticket.Estimacion = ticket.estimacionTicket.Sum(s => s.detalleEstimacionTicket.Sum(e => e.TiempoDesarrollo + e.TiempoPrueba).Value);

						int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();
						numeroVersion--;
						TicketHistorico ticketHistorico = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial && x.Version == numeroVersion).FirstOrDefault();
						if(ticketHistorico == null)
						{
							throw new Exception("No se encontró el versionamiento del ticket.");
						}
						numeroVersion++;
						string emailUser = User.Identity.Name;
						Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

						TicketHistorico ticketHis = new TicketHistorico
						{
							ticket = ticket,
							Version = numeroVersion,
							estadoTicket = ticket.estadoTicket,
							SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
							SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
							SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
							SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
							SecuencialProximaActividad = ticket.SecuencialProximaActividad,
							ReportadoPor = ticket.ReportadoPor,
							Reputacion = ticket.Reputacion,
							usuario = ticketHistorico.usuario,
							Telefono = ticket.Telefono,
							Asunto = ticket.Asunto,
							Detalle = ticket.Detalle,
							FechaCreado = ticket.FechaCreado,
							Estimacion = ticket.Estimacion,
							NumeroVerificador = 1,
							FechaOperacion = DateTime.Now,
							SeFactura = ticket.SeFactura,
							Facturado = ticket.Facturado,
							IngresoInterno = ticket.IngresoInterno,
							Reprocesos = ticket.Reprocesos
						};
						db.TicketHistorico.Add(ticketHis);

						Usuario usuarioEmail = ticketHistorico.usuario;
						Persona personaEmail = usuarioEmail.persona;
						List<string> destinatarios = Utiles.CorreoPorGrupoEmail("COORD");
						var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
						foreach(var g in gestores)
						{
							destinatarios.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
						}
						destinatarios.Add(usuarioEmail.Email);
						destinatarios.Add(user.Email);
						string comercial = System.Configuration.ConfigurationManager.AppSettings["emailComercial"];
						comercial += "@sifizsoft.com";
						destinatarios.Add(comercial);
						destinatarios = destinatarios.Distinct().ToList();
						destinatarios.Remove("gerencia@sifizsoft.com");
						string asunto = ticket.persona_cliente.cliente.Codigo + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - Estimación completada (" + ticket.Asunto + ")";


						string htmlMail = "<div class=\"textoCuerpo\">Estimado " + Utiles.UpperCamelCase(personaEmail.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(personaEmail.Apellido1.ToLower()) + ":<br>";
						if(personaEmail.Sexo == "F")
						{
							htmlMail = "<div class=\"textoCuerpo\">Estimada " + Utiles.UpperCamelCase(personaEmail.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(personaEmail.Apellido1.ToLower()) + ":<br>";
						}

						htmlMail += "La estimación del ticket " + String.Format("{0:000000}", ticket.Secuencial) + @" perteneciente al cliente: " + ticket.persona_cliente.cliente.Descripcion +
									 @"  ha sido editada por " + user.persona.Nombre1 + " " + user.persona.Apellido1 + ", favor verificar nuevamente la estimación.</div>";

						List<string> listaPathFicheros = new List<string>();
						string pathExcel = GenerarExcelEstimaciones(idEstimacion);
						string pathAdjuntoExcel = Path.Combine(Server.MapPath("~/Web/resources/tickets/"), pathExcel);
						listaPathFicheros.Add(pathAdjuntoExcel);

						string correoCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault().Email;
						List<string> destinatarioCliente = new List<string>();
						destinatarioCliente.Add(correoCliente);

						Utiles.EnviarEmailSistema(destinatarioCliente.ToArray(), htmlMail, asunto, null, String.Format("{0:000000}", ticket.Secuencial));
						Utiles.EnviarEmailSistema(destinatarios.ToArray(), htmlMail, asunto, listaPathFicheros.ToArray(), String.Format("{0:000000}", ticket.Secuencial));

						HistoricoInformacionTicket historicoCorreo = new HistoricoInformacionTicket
						{
							SecuencialTicketHistorico = ticketHis.SecuencialTicket,
							VersionTicketHistorico = ticketHis.Version,
							Fecha = DateTime.Now,
							Texto = htmlMail
						};
						db.HistoricoInformacionTicket.Add(historicoCorreo);
					}

				}
				else
				{
					estimacionTicket.EntregablesAdicionales = entregables.Trim();
					estimacionTicket.InformacionComplementaria = informacionComplementaria.Trim();
					estimacionTicket.EstimacionTerminada = 1;

					//Verificando si s el último de los colaboradores por estimar
					int cantidadEstimacionesSinTerminar = (from et in db.EstimacionTicket
														   where et.EstaActiva == 1 && et.SecuencialTicket == estimacionTicket.SecuencialTicket &&
																 et.EstimacionTerminada == 0
														   select new
														   {
															   et.Secuencial
														   }).Count();

					if(cantidadEstimacionesSinTerminar == 1)//Esta es la última que queda
					{   //Enviar email al que mandó a estimar para decirle que el ticket esta estimado
						Ticket ticket = estimacionTicket.ticket;
						ticket.Estimacion = ticket.estimacionTicket.Sum(s => s.detalleEstimacionTicket.Sum(e => e.TiempoDesarrollo + e.TiempoPrueba).Value);
						EstadoTicket estadoTicket = db.EstadoTicket.Where(x => x.Codigo == "PENDIENTE" && x.EstaActivo == 1).FirstOrDefault();
						ticket.estadoTicket = estadoTicket;
						int secuencialProximaActividad = db.ProximaActividad.Where(x => x.Codigo == "VALIDAR ESTIMACION" && x.EstaActivo == 1).FirstOrDefault().Secuencial;
						ticket.SecuencialProximaActividad = secuencialProximaActividad;

						int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();
						numeroVersion--;
						TicketHistorico ticketHistorico = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial && x.Version == numeroVersion).FirstOrDefault();
						if(ticketHistorico == null)
						{
							throw new Exception("No se encontró el versionamiento del ticket.");
						}
						numeroVersion++;
						string emailUser = User.Identity.Name;
						Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

						TicketHistorico ticketHis = new TicketHistorico
						{
							ticket = ticket,
							Version = numeroVersion,
							estadoTicket = estadoTicket,
							SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
							SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
							SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
							SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
							SecuencialProximaActividad = ticket.SecuencialProximaActividad,
							ReportadoPor = ticket.ReportadoPor,
							Reputacion = ticket.Reputacion,
							usuario = ticketHistorico.usuario,
							Telefono = ticket.Telefono,
							Asunto = ticket.Asunto,
							Detalle = ticket.Detalle,
							FechaCreado = ticket.FechaCreado,
							Estimacion = ticket.Estimacion,
							NumeroVerificador = 1,
							FechaOperacion = DateTime.Now,
							SeFactura = ticket.SeFactura,
							Facturado = ticket.Facturado,
							IngresoInterno = ticket.IngresoInterno,
							Reprocesos = ticket.Reprocesos
						};
						db.TicketHistorico.Add(ticketHis);

						Usuario usuarioEmail = ticketHistorico.usuario;
						Persona personaEmail = usuarioEmail.persona;

						List<string> destinatarios = new List<string>();
						destinatarios.Add("victor@sifizsoft.com");

						string asunto = ticket.persona_cliente.cliente.Codigo + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - VALIDAR ESTIMACION (" + ticket.Asunto + ")";

						string htmlMail = "<div class=\"textoCuerpo\">Estimado " + Utiles.UpperCamelCase(personaEmail.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(personaEmail.Apellido1.ToLower()) + ":<br>";
						if(personaEmail.Sexo == "F")
						{
							htmlMail = "<div class=\"textoCuerpo\">Estimada " + Utiles.UpperCamelCase(personaEmail.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(personaEmail.Apellido1.ToLower()) + ":<br>";
						}

						htmlMail += "El ticket " + String.Format("{0:000000}", ticket.Secuencial) + @" perteneciente al cliente: " + ticket.persona_cliente.cliente.Descripcion +
									 @" se ha terminado de estimar por parte de los colaboradores seleccionados, favor de validar la estimación.</div>";

						List<string> listaPathFicheros = new List<string>();
						string pathExcel = GenerarExcelEstimaciones(idEstimacion);
						string pathAdjuntoExcel = Path.Combine(Server.MapPath("~/Web/resources/tickets/"), pathExcel);
						listaPathFicheros.Add(pathAdjuntoExcel);

						Utiles.EnviarEmailSistema(destinatarios.ToArray(), htmlMail, asunto, listaPathFicheros.ToArray(), String.Format("{0:000000}", ticket.Secuencial));

						HistoricoInformacionTicket historicoCorreo = new HistoricoInformacionTicket
						{
							SecuencialTicketHistorico = ticketHis.SecuencialTicket,
							VersionTicketHistorico = ticketHis.Version,
							Fecha = DateTime.Now,
							Texto = htmlMail
						};
						db.HistoricoInformacionTicket.Add(historicoCorreo);
					}

				}
				db.SaveChanges();

				var resp = new
				{
					success = true
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult GuardarEntregableEstimacionUsuario(int idEstimacion, string entregable, string jsonDetalles)
		{
			try
			{
				EstimacionTicket estimacionTicket = db.EstimacionTicket.Find(idEstimacion);
				if(estimacionTicket == null)
				{
					throw new Exception("No se encontró la estimación del usuario");
				}
				EntregableDetalleEstimacion entregableDetalleEstimacion = new EntregableDetalleEstimacion();
				entregableDetalleEstimacion.Nombre = entregable;
				db.EntregableDetalleEstimacion.Add(entregableDetalleEstimacion);

				var s = new JavaScriptSerializer();
				var jsonDetallesObj = s.Deserialize<dynamic>(jsonDetalles);
				for(int i = 0; i < jsonDetallesObj.Length; i++)
				{
					var obj = jsonDetallesObj[i];
					string detalle = (string)obj["detalle"];
					int tiempoEstimacion = int.Parse(obj["tiempoEstimacion"]);
					int tiempoDesarrollo = int.Parse(obj["tiempoDesarrollo"]);
					int tiempoPrueba = int.Parse(obj["tiempoPrueba"]);
					int tiempoQA = int.Parse(obj["tiempoQA"]);
					int nivel = int.Parse(obj["nivel"]);

					DetalleEstimacionTicket newDetalle = new DetalleEstimacionTicket
					{
						SecuencialEstimacionTicket = idEstimacion,
						SecuencialNivelColaborador = nivel,
						Detalle = detalle,
						TiempoEstimacion = tiempoEstimacion,
						TiempoDesarrollo = tiempoDesarrollo,
						TiempoPrueba = tiempoPrueba,
						TiempoQA = tiempoQA,
						SecuencialEntregableEstimacion = entregableDetalleEstimacion.Secuencial
					};
					db.DetalleEstimacionTicket.Add(newDetalle);
				}
				//estimacionTicket
				estimacionTicket.EstimacionTerminada = 0;

				db.SaveChanges();

				var resp = new
				{
					success = true
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult EditarEntregableEstimacionUsuario(int idEstimacion, int idEntregable, string nombreEntregable, string jsonDetalles)
		{
			try
			{
				EstimacionTicket estimacionTicket = db.EstimacionTicket.Find(idEstimacion);
				if(estimacionTicket == null)
				{
					throw new Exception("No se encontró la estimación del usuario");
				}
				EntregableDetalleEstimacion entregableDetalleEstimacion = db.EntregableDetalleEstimacion.Find(idEntregable);
				entregableDetalleEstimacion.Nombre = nombreEntregable;

				foreach(DetalleEstimacionTicket detalle in db.DetalleEstimacionTicket.Where(d => d.SecuencialEntregableEstimacion == idEntregable).ToList())
				{
					db.DetalleEstimacionTicket.Remove(detalle);
				}

				int totalHoras = 0;
				var s = new JavaScriptSerializer();
				var jsonDetallesObj = s.Deserialize<dynamic>(jsonDetalles);
				for(int i = 0; i < jsonDetallesObj.Length; i++)
				{
					var obj = jsonDetallesObj[i];
					string detalle = (string)obj["detalle"];
					int tiempoEstimacion = int.Parse(obj["tiempoEstimacion"]);
					int tiempoDesarrollo = int.Parse(obj["tiempoDesarrollo"]);
					int tiempoPrueba = int.Parse(obj["tiempoPrueba"]);
					int tiempoQA = int.Parse(obj["tiempoQA"]);
					int nivel = int.Parse(obj["nivel"]);

					totalHoras += tiempoEstimacion + tiempoDesarrollo + tiempoPrueba;

					DetalleEstimacionTicket newDetalle = new DetalleEstimacionTicket
					{
						SecuencialEstimacionTicket = idEstimacion,
						SecuencialNivelColaborador = nivel,
						Detalle = detalle,
						TiempoEstimacion = tiempoEstimacion,
						TiempoDesarrollo = tiempoDesarrollo,
						TiempoPrueba = tiempoPrueba,
						TiempoQA = tiempoQA,
						SecuencialEntregableEstimacion = entregableDetalleEstimacion.Secuencial
					};
					db.DetalleEstimacionTicket.Add(newDetalle);
				}
				estimacionTicket.NumeroHoras = totalHoras;
				estimacionTicket.EstimacionTerminada = 0;

				db.SaveChanges();

				var resp = new
				{
					success = true
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult EliminarEntregableEstimacionUsuario(int idEstimacion, int idEntregable)
		{
			try
			{
				EstimacionTicket estimacionTicket = db.EstimacionTicket.Find(idEstimacion);
				if(estimacionTicket == null)
				{
					throw new Exception("No se encontró la estimación del usuario");
				}
				EntregableDetalleEstimacion entregableDetalleEstimacion = db.EntregableDetalleEstimacion.Find(idEntregable);

				foreach(DetalleEstimacionTicket detalle in db.DetalleEstimacionTicket.Where(d => d.SecuencialEntregableEstimacion == idEntregable).ToList())
				{
					db.DetalleEstimacionTicket.Remove(detalle);
				}
				db.EntregableDetalleEstimacion.Remove(entregableDetalleEstimacion);
				estimacionTicket.EstimacionTerminada = 0;
				db.SaveChanges();

				var resp = new
				{
					success = true
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

		[Authorize(Roles = "ADMIN, USER")]
		[HttpPost]
		public ActionResult CargarComentariosTicketsUsuario(int idTicket)
		{
			try
			{
				var comentarios = (from ct in db.ComentarioTicket
								   where ct.SecuencialTicket == idTicket && ct.VerTodos == 1
								   orderby ct.FechaHora descending
								   select new
								   {
									   fechaHora = ct.FechaHora,
									   colaborador = db.ComentarioTicketCliente.Where(s => s.SecuencialComentarioTicket == ct.Secuencial).FirstOrDefault() != null ? db.ComentarioTicketCliente.Where(s => s.SecuencialComentarioTicket == ct.Secuencial).FirstOrDefault().cliente.persona_cliente.FirstOrDefault().persona.usuario.FirstOrDefault().Email : ct.colaborador.persona.usuario.FirstOrDefault().Email,
									   detalle = ct.Detalle
								   }).ToList();

				var resp = new
				{
					success = true,
					comentarios = comentarios
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

		[Authorize(Roles = "ADMIN, USER")]
		[HttpPost]
		public ActionResult GuardarComentariosTicketsUsuario(int idTicket, string comentario)
		{
			try
			{
				Ticket t = db.Ticket.Find(idTicket);
				if(t == null)
				{
					throw new Exception("Error, no puede insertar un comentario porque no se encontró el ticket.");
				}

				//Adicionando el comentario al ticket                                
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				Persona personaUsuario = user.persona;

				ComentarioTicket comentarioTicket = new ComentarioTicket
				{
					SecuencialColaborador = personaUsuario.colaborador.FirstOrDefault().Secuencial,
					SecuencialTicket = idTicket,
					FechaHora = DateTime.Now,
					Detalle = comentario,
					VerTodos = 1
				};
				db.ComentarioTicket.Add(comentarioTicket);

				ComentarioGeneral comentarioGeneral = new ComentarioGeneral
				{
					usuario = user,
					FechaHora = DateTime.Now,
					TipoComentario = "NOTIFICACION",
					Comentario = "<b>NOTIFICACIÓN</b>-Ticket: <b>" + string.Format("{0:000000}", t.Secuencial) + "</b>, Cliente: <b>" + t.persona_cliente.cliente.Codigo + "</b>, Asunto: <b>" + t.Asunto + "</b>, " + comentario,
					Importancia = "Normal"
				};
				db.ComentarioGeneral.Add(comentarioGeneral);

				db.SaveChanges();

				//Llamar al SignalR
				//Websocket.getInstance().NuevosComentarios();
				var resp = new
				{
					success = true
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

		[Authorize(Roles = "ADMIN, USER")]
		[HttpPost]
		public ActionResult EnviarEmailComentario(int idTicket, string destinatariosEmailTicket, string asuntoEmailTicket, string comentarioEmailTicket)
		{
			try
			{
				Ticket t = db.Ticket.Find(idTicket);
				if(t == null)
				{
					throw new Exception("Error, no puede insertar un comentario porque no se encontró el ticket.");
				}
				string numeroTicket = string.Format("{0:000000}", t.Secuencial);
				string textoEmail = @"<div class='textoCuerpo'>Estimado(a): ";
				textoEmail += "<br>";
				textoEmail += "Se ha realizado el siguiente comentario del ticket: ";
				textoEmail += "<b>" + numeroTicket + ".</b> <br/>";
				textoEmail += comentarioEmailTicket;
				textoEmail += "</div>";

				List<string> usuariosDestinos = new List<string>();
				Regex rgx = new Regex(@"^(([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)(\s*;\s*|\s*$))*$");
				if(!rgx.IsMatch(destinatariosEmailTicket))
				{
					throw new Exception("Debe ingresar una lista de correos válida separados por punto y coma(;)");
				};
				string[] emails = destinatariosEmailTicket.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
				foreach(var email in emails)
				{
					usuariosDestinos.Add(email);
				}
				var gestores = t.persona_cliente.cliente.gestorServicios.ToList();
				foreach(var g in gestores)
				{
					usuariosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
				}
				usuariosDestinos = usuariosDestinos.Distinct().ToList();
				string asuntoEmail = asuntoEmailTicket;
				Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), textoEmail, asuntoEmail);

				var resp = new
				{
					success = true
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


		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarDatosTickets(int idEstimacion)
		{
			try
			{
				EstimacionTicket estimacion = db.EstimacionTicket.Find(idEstimacion);
				if(estimacion == null)
				{
					throw new Exception("No se encontró la estimación.");
				}

				Ticket ticket = estimacion.ticket;
				var datosTicket = (from t in db.Ticket
								   join
									   pcl in db.Persona_Cliente on t.persona_cliente equals pcl
								   join
									   etk in db.EstadoTicket on t.estadoTicket equals etk
								   join
									   ptk in db.PrioridadTicket on t.prioridadTicket equals ptk
								   join
									   ctk in db.CategoriaTicket on t.categoriaTicket equals ctk
								   where t.Secuencial == ticket.Secuencial
								   select new
								   {
									   id = t.Secuencial,
									   usuarioCliente = pcl.persona.Nombre1 + " " + pcl.persona.Apellido1,
									   idCliente = pcl.SecuencialCliente,
									   cliente = pcl.cliente.Descripcion,
									   clienteTelefono = pcl.Telefono,
									   telefono = t.Telefono,
									   reporto = t.ReportadoPor,
									   reputacion = t.Reputacion,
									   fecha = t.FechaCreado,
									   estimacion = t.Estimacion,
									   asunto = t.Asunto,
									   detalle = t.Detalle,
									   estado = etk.Codigo,
									   prioridad = ptk.Codigo,
									   categoria = ctk.Codigo,
									   adjuntos = (from adjt in db.AdjuntoTicket
												   where adjt.SecuencialTicket == t.Secuencial
												   select adjt.Url).ToList(),
									   verificador = t.NumeroVerificador
								   }).FirstOrDefault();

				//Calculando la reputación
				var ticketsCliente = (from t in db.Ticket
									  join pc in db.Persona_Cliente on t.persona_cliente equals pc
									  where pc.SecuencialCliente == datosTicket.idCliente
									  select new
									  {
										  idTicket = t.Secuencial,
										  reputacion = t.Reputacion
									  }).ToList();

				int cant = 5;
				int total = 5;
				foreach(var tc in ticketsCliente)
				{
					cant += 5;//5 es el valor máximo
					total += (int)tc.reputacion;
				}

				float div = (float)total / (float)cant;
				int reputacion = (int)Math.Floor(div * 100);

				var resp = new
				{
					success = true,
					datosTicket = datosTicket,
					reputacion = reputacion
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

		[HttpGet]
		public ActionResult verficacionDeEstimacion()
		{
			DateTime fecha = DateTime.Today;
			DateTime diaProximo = fecha.AddDays(1);
			DateTime diaProximo2 = fecha.AddDays(2);

			//Buscando los email de los colaboradores que tienen estimaciones que terminan en próximo día
			string[] emails = (from et in db.EstimacionTicket
							   join
								   colab in db.Colaborador on et.colaborador equals colab
							   join
								   pers in db.Persona on colab.persona equals pers
							   join
								   user in db.Usuario on pers.Secuencial equals user.SecuencialPersona
							   where et.EstimacionTerminada == 0 && et.EstaActiva == 1 &&
									 et.FechaLimite >= diaProximo && et.FechaLimite < diaProximo2 &&
									 user.EstaActivo == 1
							   select user.Email
								).ToArray<string>();

			string HtmlEmail = @"Buenas, este correo es un recordatorio de que usted debe completar estimaciones de tickets<br/>
                                que tienen como límite el día de mañana.<br/>";

			Utiles.EnviarEmailSistema(emails, HtmlEmail, "Debe estimar tickets que vencen mañana", null, "Estimaciones de Ticket");

			return View();
		}

		//FUNCIONES DE LAS TAREAS COMPENSATORIAS
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult UsuarioTareasCompensatorias(bool todas = false)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				int idColaborador = user.persona.colaborador.FirstOrDefault().Secuencial;
				var tareasCompensatorias = (
						from tc in db.TareaCompensatoria
						join
							t in db.Tarea on tc.tarea equals t
						where t.SecuencialEstadoTarea == 3 && t.SecuencialColaborador == idColaborador &&
							  tc.EstaActiva == 1
						orderby tc.Secuencial descending
						select new
						{
							id = t.Secuencial,
							desc = t.Detalle,
							cliente = t.cliente.Descripcion,
							fecha = t.FechaInicio,
							tiempo = tc.TiempoMinutos,
							consumido = tc.TiempoConsumido
						}
					).ToList();

				if(!todas)
				{
					tareasCompensatorias = tareasCompensatorias.Where(x => x.tiempo > x.consumido).ToList();
				}

				List<object> listaTareasCompensatorias = new List<object>();

				int totalTiempo = 0;
				int totalConsumido = 0;
				foreach(var tarComp in tareasCompensatorias)
				{
					totalTiempo += tarComp.tiempo;
					totalConsumido += tarComp.consumido;
					listaTareasCompensatorias.Add(
						new
						{
							id = tarComp.id,
							desc = tarComp.desc,
							cliente = tarComp.cliente,
							fecha = tarComp.fecha.ToString("dd/MM/yyyy"),
							tiempo = Utiles.DarHorasMinutosPorMinutos(tarComp.tiempo),
							consumido = Utiles.DarHorasMinutosPorMinutos(tarComp.consumido),
							queda = Utiles.DarHorasMinutosPorMinutos(tarComp.tiempo - tarComp.consumido)
						});
				}

				string tiempoCompensatorio = Utiles.DarHorasMinutosPorMinutos(totalTiempo - totalConsumido);

				var resp = new
				{
					success = true,
					tareasCompensatorias = listaTareasCompensatorias,
					tiempoCompensatorio = tiempoCompensatorio
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

		//FUNCIONES DE LAS TAREAS COMPENSATORIAS
		[HttpPost]
		[Authorize(Roles = "ADMINTFS, ADMIN")]
		public ActionResult TicketsPorPublicar()
		{
			try
			{
				var data = (from t in db.Ticket
								//join trp in db.Ticket_RequierePublicacion on t.Secuencial equals trp.SecuencialTicket
							join et in db.EstadoTicket on t.SecuencialEstadoTicket equals et.Secuencial
							join cat in db.CategoriaTicket on t.SecuencialCategoriaTicket equals cat.Secuencial
							join pri in db.PrioridadTicket on t.SecuencialPrioridadTicket equals pri.Secuencial
							join pcli in db.Persona_Cliente on t.SecuencialPersona_Cliente equals pcli.SecuencialPersona
							join cli in db.Cliente on pcli.SecuencialCliente equals cli.Secuencial
							where t.SecuencialProximaActividad == 16
							select new
							{
								idTicket = t.Secuencial,
								cliente = cli.Descripcion,
								estadoTicket = et.Codigo,
								categoria = cat.Codigo,
								prioridad = pri.Codigo,
								asunto = t.Asunto,
								asignado = (
										db.TicketTarea.Where(x => x.SecuencialTicket == t.Secuencial && x.EstaActiva == 1).Count() > 0
								   ) ?
									   (from p in db.Persona
										join c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
										join tar in db.Tarea on c.Secuencial equals tar.SecuencialColaborador
										join ttar in db.TicketTarea on tar.Secuencial equals ttar.SecuencialTarea
										orderby tar.FechaInicio descending
										where ttar.SecuencialTicket == t.Secuencial
										select p.Nombre1 + " " + p.Apellido1).FirstOrDefault()
									 : "NO ASIGNADO",
							}).ToList();

				var resp = new
				{
					success = true,
					ticketsPorPublicar = data
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

		[HttpPost]
		[Authorize(Roles = "ADMINTFS, ADMIN")]
		public ActionResult TicketPublicado(int id, string publicacionClienteServidor, string publicacionPruebasProd, string dirFTP, string usuarioFTP, string claveFTP, string pathFTP, bool publicarEnUno, int[] idPublicacionLote)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				if(publicarEnUno)
				{
					int secuencialCliente = 0;
					foreach(int idTicket in idPublicacionLote)
					{
						Ticket ticket = db.Ticket.Find(idTicket);
						if(ticket == null)
						{
							throw new Exception("No se encontró el ticket");
						}

						int pSecuencialCliente = ticket.persona_cliente.SecuencialCliente;
						if(secuencialCliente != 0 && secuencialCliente != pSecuencialCliente)
						{
							throw new Exception("Los tickets deben pertenecer al mismo cliente.");
						}
						secuencialCliente = pSecuencialCliente;
					}

					foreach(int idTicket in idPublicacionLote)
					{
						Ticket ticket = db.Ticket.Find(idTicket);

						bool financial25 = false;
						if(ticket.SecuencialTicketVersionCliente != null)
						{
							financial25 = db.TicketVersionCliente.Find(ticket.SecuencialTicketVersionCliente).Codigo == "FBS 2.5";
						}

						//Cambiando el estado del ticket
						ticket.SecuencialEstadoTicket = 10;//EL TICKET ESTA RESUELTO
						ticket.SecuencialProximaActividad = 17;//CERTIFICAR

						//Adicionando el histórico del ticket                                                        
						int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

						TicketHistorico ticketHistorico = new TicketHistorico
						{
							ticket = ticket,
							Version = numeroVersion,
							SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA RESUELTO")
							SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
							SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
							SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
							SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
							SecuencialProximaActividad = ticket.SecuencialProximaActividad,
							usuario = user,
							ReportadoPor = ticket.ReportadoPor,
							Reputacion = ticket.Reputacion,
							Telefono = ticket.Telefono,
							Asunto = ticket.Asunto,
							Detalle = ticket.Detalle,
							FechaCreado = ticket.FechaCreado,
							Estimacion = ticket.Estimacion,
							NumeroVerificador = 1,
							FechaOperacion = DateTime.Now,
							SeFactura = ticket.SeFactura,
							Facturado = ticket.Facturado,
							IngresoInterno = ticket.IngresoInterno,
							Reprocesos = ticket.Reprocesos
						};

						db.TicketHistorico.Add(ticketHistorico);

						db.SaveChanges();//Salvando los cambios

						//Enviando los emails
						string texto = "Estimado(a):<br/>";
						texto += "El motivo de este correo es para hacerle llegar la publicación de " + publicacionClienteServidor + " de " + publicacionPruebasProd + " solicitada.<br/>";
						texto += "La publicación se encuentra almacenada en el ftp, a continuación se especifica el <b>link</b>, <b>usuario</b>, <b>clave</b> y <b>path</b>.<br/><br/>";
						texto += "<span class=\"tab-campo\"><b>FTP: </b></span>" + dirFTP + "<br/>";
						texto += "<span class=\"tab-campo\"><b>Usuario: </b></span>" + usuarioFTP + "<br/>";
						texto += "<span class=\"tab-campo\"><b>Clave: </b></span>" + claveFTP + "<br/>";
						texto += "<span class=\"tab-campo\"><b>Path: </b></span>" + pathFTP + "<br/></br>";

						texto += "cualquier duda en el acceso al ftp, por favor comunicarse con: " + emailUser + ".</br>";

						string textoEmail = @"<div class='textoCuerpo'><br/>";
						textoEmail += texto;

						textoEmail += @"<br/>";

						//Los link que se le envían al usuario para aceptar o rechazar la resolución
						string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

						string linkAceptar = baseUrl + "/clientes/respuesta-resolucion?cod=" + Server.UrlEncode(Utiles.EncriptacionSimetrica(ticket.Secuencial + ":ACEPTADO"));
						string linkRechazar = baseUrl + "/clientes/respuesta-resolucion?cod=" + Server.UrlEncode(Utiles.EncriptacionSimetrica(ticket.Secuencial + ":NOACEPTADO"));
						string textoEmailCliente = textoEmail + @"<br/><div style='font-size: 11pt; font-family: sans-serif; color: #1F497D;'>
			                                                <span style='color:#128812'>
				                                                Si usted <b>ACEPTA</b> este requerimiento por favor presione el siguiente link:
			                                                </span><br/>                                                            
			                                                <a href='" + linkAceptar + @"'>
                                                                <i>" + linkAceptar + @"</i>
			                                                </a>			
			                                                <br/><br/>
			                                                <span style='color:#EE1212'>Si por el contrario <b>NO ACEPTA</b> este requerimiento por favor presione el siguiente link:</span><br/>
			                                                <a href='" + linkRechazar + @"'>
				                                                <i>" + linkRechazar + @"</i>
			                                                </a>
                                                            <br/>
                                                            <br/>
                                                            <i>El presente correo concluye la resolución de este requerimiento, formalmente solicitamos la certificación del mismo o sus observaciones. Si dentro de los próximos 5 días laborables no recibimos su respuesta, procederemos a cerrar el ticket. En caso de requerir correcciones será necesario que ingrese otro ticket.</i>
		                                                </div>";

						Persona_Cliente personaCliente = db.Persona_Cliente.Find(ticket.SecuencialPersona_Cliente);
						Persona persona = personaCliente.persona;
						string emailCliente = persona.usuario.FirstOrDefault().Email;

						textoEmail += "<br/><i>El presente correo concluye la resolución de este requerimiento, formalmente solicitamos la certificación del mismo o sus observaciones. Si dentro de los próximos 5 días laborables no recibimos su respuesta, procederemos a cerrar el ticket. En caso de requerir correcciones será necesario que ingrese otro ticket.</i><br/><i>Se ha enviado una copia de este email al cliente. Email:" + emailCliente + "</i>";
						textoEmail += "</div>";
						textoEmailCliente += "</div>";

						List<string> correosDestinos = new List<string>();
						correosDestinos.Add(emailUser);
						if(financial25)
						{
							correosDestinos.Add("publicacionesdoscinco@sifizsoft.com");
						}
						correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));

						string asuntoEmail = personaCliente.cliente.Codigo + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Publicado (" + ticket.Asunto + ")";
						Utiles.EnviarEmailSistema(new string[] { emailCliente }, textoEmailCliente, asuntoEmail, null, string.Format("{0:000000}", ticket.Secuencial));
						Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail, null, string.Format("{0:000000}", ticket.Secuencial));

						//adicionando el email a los historicos
						string destinos = String.Join(", ", correosDestinos.ToArray());
						string textoHistoricoCorreo = "<b>Correo de información, Ticket Publicado</b><br/>";
						textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
						textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
						textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
						HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
						{
							SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
							VersionTicketHistorico = ticketHistorico.Version,
							Fecha = DateTime.Now,
							Texto = textoHistoricoCorreo
						};
						db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
						db.SaveChanges();
					}
				}
				else
				{
					Ticket ticket = db.Ticket.Find(id);
					if(ticket == null)
					{
						throw new Exception("No se encontró el ticket");
					}

					bool financial25 = false;
					if(ticket.SecuencialTicketVersionCliente != null)
					{
						financial25 = db.TicketVersionCliente.Find(ticket.SecuencialTicketVersionCliente).Codigo == "FBS 2.5";
					}

					//Cambiando el estado del ticket
					ticket.SecuencialEstadoTicket = 10;//EL TICKET ESTA RESUELTO
					ticket.SecuencialProximaActividad = 17;//CERTIFICAR

					//Adicionando el histórico del ticket                                                        
					int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

					TicketHistorico ticketHistorico = new TicketHistorico
					{
						ticket = ticket,
						Version = numeroVersion,
						SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA RESUELTO")
						SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
						SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
						SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
						SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
						SecuencialProximaActividad = ticket.SecuencialProximaActividad,
						usuario = user,
						ReportadoPor = ticket.ReportadoPor,
						Reputacion = ticket.Reputacion,
						Telefono = ticket.Telefono,
						Asunto = ticket.Asunto,
						Detalle = ticket.Detalle,
						FechaCreado = ticket.FechaCreado,
						Estimacion = ticket.Estimacion,
						NumeroVerificador = 1,
						FechaOperacion = DateTime.Now,
						SeFactura = ticket.SeFactura,
						Facturado = ticket.Facturado,
						IngresoInterno = ticket.IngresoInterno,
						Reprocesos = ticket.Reprocesos
					};
					db.TicketHistorico.Add(ticketHistorico);

					db.SaveChanges();//Salvando los cambios

					//Enviando los emails
					string texto = "Estimado:<br/>";
					if(user.persona.Sexo == "F")
						texto = "Estimada:<br/>";
					texto += "El motivo de este correo es para hacerle llegar la publicación de " + publicacionClienteServidor + " de " + publicacionPruebasProd + " solicitada.<br/>";
					texto += "La publicación se encuentra almacenada en el ftp, a continuación se especifica el <b>link</b>, <b>usuario</b>, <b>clave</b> y <b>path</b>.<br/><br/>";
					texto += "<span class=\"tab-campo\"><b>FTP: </b></span>" + dirFTP + "<br/>";
					texto += "<span class=\"tab-campo\"><b>Usuario: </b></span>" + usuarioFTP + "<br/>";
					texto += "<span class=\"tab-campo\"><b>Clave: </b></span>" + claveFTP + "<br/>";
					texto += "<span class=\"tab-campo\"><b>Path: </b></span>" + pathFTP + "<br/></br>";

					texto += "cualquier duda en el acceso al ftp, por favor comunicarse con: " + emailUser + ".</br>";

					string textoEmail = @"<div class='textoCuerpo'><br/>";
					textoEmail += texto;

					textoEmail += @"<br/>";

					//Los link que se le envían al usuario para aceptar o rechazar la resolución
					string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

					string linkAceptar = baseUrl + "/clientes/respuesta-resolucion?cod=" + Server.UrlEncode(Utiles.EncriptacionSimetrica(ticket.Secuencial + ":ACEPTADO"));
					string linkRechazar = baseUrl + "/clientes/respuesta-resolucion?cod=" + Server.UrlEncode(Utiles.EncriptacionSimetrica(ticket.Secuencial + ":NOACEPTADO"));
					string textoEmailCliente = textoEmail + @"<br/><div style='font-size: 11pt; font-family: sans-serif; color: #1F497D;'>
			                                                <span style='color:#128812'>
				                                                Si usted <b>ACEPTA</b> este requerimiento por favor presione el siguiente link:
			                                                </span><br/>                                                            
			                                                <a href='" + linkAceptar + @"'>
                                                                <i>" + linkAceptar + @"</i>
			                                                </a>			
			                                                <br/><br/>
			                                                <span style='color:#EE1212'>Si por el contrario <b>NO ACEPTA</b> este requerimiento por favor presione el siguiente link:</span><br/>
			                                                <a href='" + linkRechazar + @"'>
				                                                <i>" + linkRechazar + @"</i>
			                                                </a>
                                                            <br/>
                                                            <br/>
                                                            <i>El presente correo concluye la resolución de este requerimiento, formalmente solicitamos la certificación del mismo o sus observaciones. Si dentro de los próximos 5 días laborables no recibimos su respuesta, procederemos a cerrar el ticket. En caso de requerir correcciones será necesario que ingrese otro ticket.</i>
		                                                </div>";

					Persona_Cliente personaCliente = db.Persona_Cliente.Find(ticket.SecuencialPersona_Cliente);
					Persona persona = personaCliente.persona;
					string emailCliente = persona.usuario.FirstOrDefault().Email;

					textoEmail += "<br/><i>El presente correo concluye la resolución de este requerimiento, formalmente solicitamos la certificación del mismo o sus observaciones. Si dentro de los próximos 5 días laborables no recibimos su respuesta, procederemos a cerrar el ticket. En caso de requerir correcciones será necesario que ingrese otro ticket.</i><br/><i>Se ha enviado una copia de este email al cliente. Email:" + emailCliente + "</i>";
					textoEmail += "</div>";
					textoEmailCliente += "</div>";

					List<string> correosDestinos = new List<string>();
					correosDestinos.Add(emailUser);
					if(financial25)
					{
						correosDestinos.Add("publicacionesdoscinco@sifizsoft.com");
					}
					correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));

					string asuntoEmail = personaCliente.cliente.Codigo + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Publicado (" + ticket.Asunto + ")";
					Utiles.EnviarEmailSistema(new string[] { emailCliente }, textoEmailCliente, asuntoEmail, null, string.Format("{0:000000}", ticket.Secuencial));
					Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail, null, string.Format("{0:000000}", ticket.Secuencial));

					//adicionando el email a los historicos
					string destinos = String.Join(", ", correosDestinos.ToArray());
					string textoHistoricoCorreo = "<b>Correo de información, Ticket Publicado</b><br/>";
					textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
					textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
					textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
					HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
					{
						SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
						VersionTicketHistorico = ticketHistorico.Version,
						Fecha = DateTime.Now,
						Texto = textoHistoricoCorreo
					};
					db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
					db.SaveChanges();
				}

				var resp = new
				{
					success = true
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

		//CATALOGOS UTILIZADOS POR EL USUARIO
		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarNivelesColaboradores()
		{
			try
			{
				var niveles = (from nc in db.NivelColaborador
							   where nc.EstaActivo == 1
							   orderby nc.Codigo ascending
							   select new
							   {
								   id = nc.Secuencial,
								   name = nc.Descripcion
							   }).ToList();

				var resp = new
				{
					success = true,
					niveles = niveles
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarTipoNotificacionTicket()
		{
			try
			{
				var tipos = (from tn in db.TipoNotificacionTicket
							 where tn.EstaActivo == 1
							 orderby tn.Descripcion ascending
							 select new
							 {
								 id = tn.Secuencial,
								 name = tn.Descripcion
							 }).ToList();

				var resp = new
				{
					success = true,
					tipos = tipos
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult EnviarNotificacionTicket(int idTarea, int tipoNotificacion, string detalle)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				string nombreColaborador = user.persona.Nombre1 + " " + user.persona.Apellido1;

				Tarea tarea = db.Tarea.Find(idTarea);
				if(tarea == null)
				{
					throw new Exception("Error, no se econtró la tarea.");
				}

				Ticket ticket = tarea.ticketTarea.FirstOrDefault().ticket;
				if(ticket == null)
				{
					throw new Exception("Error, la tarea no está asociada a un ticket.");
				}

				NotificacionTicket notificacion = new NotificacionTicket
				{
					ticket = ticket,
					SecuencialTipoNotificTicket = tipoNotificacion,
					FechaHora = DateTime.Now,
					Detalle = detalle,
					EstaActivo = 1,
					NumeroVerificador = 1
				};
				db.NotificacionTicket.Add(notificacion);
				db.SaveChanges();

				//Adicionando los comentarios a comentarios generales
				ComentarioGeneral comentarioGeneral = new ComentarioGeneral
				{
					FechaHora = DateTime.Now,
					usuario = user,
					TipoComentario = "NOTIFICACION",
					Comentario = "<b>NOTIFICACIÓN</b>-Ticket: <b>" + string.Format("{0:000000}", ticket.Secuencial) + "</b>, Cliente: <b>" + ticket.persona_cliente.cliente.Codigo + "</b>, Asunto: <b>" + ticket.Asunto + "</b>, " + detalle,
					Importancia = "Muy Importante"
				};
				db.ComentarioGeneral.Add(comentarioGeneral);
				db.SaveChanges();
				//Llamar al SignalR
				Websocket.getInstance().NuevosComentarios();

				//Enviando la notificación por correo
				//Adicionando el histórico del ticket                                                        
				TicketHistorico ticketHistorico = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).OrderByDescending(x => x.Version).FirstOrDefault();

				//Enviando los emails
				string texto = "Se ha adicionado una nueva notificación en la resolucion de un ticket:<br/></br>";
				texto += "<b>colaborador: </b>" + nombreColaborador + "<br/>";

				TipoNotificacionTicket objTipoNotificacion = db.TipoNotificacionTicket.Find(tipoNotificacion);

				texto += "<b>Fecha: </b>" + notificacion.FechaHora.ToString("dd/MM/yyyy HH:mm") + "<br/>";
				texto += "<b>Tipo de motivo: </b>" + objTipoNotificacion.Descripcion + "<br/>";
				texto += "<b>Detalle: </b>" + notificacion.Detalle + "<br/><br/>";

				texto += "Por favor aternder cuanto antes la notificación.";

				string textoEmail = @"<div class='textoCuerpo'><br/>";
				textoEmail += texto;

				textoEmail += @"<br/></div>";

				List<string> correosDestinos = new List<string>();
				correosDestinos.Add(emailUser);
				correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));
				correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("JEDES"));

				string asuntoEmail = ticket.persona_cliente.cliente.Codigo + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Nueva notificación de ticket (" + ticket.Asunto + ")";
				Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail, null, string.Format("{0:000000}", ticket.Secuencial));

				//adicionando el email a los historicos
				string destinos = String.Join(", ", correosDestinos.ToArray());
				string textoHistoricoCorreo = "<b>Correo de información, Nueva notificación de ticket</b><br/>";
				textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
				textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
				textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
				HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
				{
					SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
					VersionTicketHistorico = ticketHistorico.Version,
					Fecha = DateTime.Now,
					Texto = textoHistoricoCorreo
				};
				db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
				db.SaveChanges();

				//--------------------------------------------------------------------------------------                
				var resp = new
				{
					success = true
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarDatosUsuario()
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				Persona persona = user.persona;
				Colaborador colab = persona.colaborador.FirstOrDefault();

				string nombre = persona.Nombre1 + " " + persona.Apellido1;
				string cargo = colab.cargo.Descripcion;
				string departamento = colab.departamento.Descripcion;
				string empresa = "null";

				if(emailUser.Contains("sifizsoft"))
				{
					empresa = "SIFIZSOFT";
				}
				else if(emailUser.Contains("intecsoft"))
				{
					empresa = "INTECSOFT";
				}
				else
				{
					empresa = "null";
				}

				var datosUsuario = new
				{
					nombre = nombre,
					cargo = cargo,
					departamento = departamento,
					empresa = empresa,
					fechaIngreso = colab.FechaIngreso.HasValue ? colab.FechaIngreso.Value.ToString("dd/MM/yyyy") : "N/A"
				};

				var resp = new
				{
					success = true,
					datosUsuario = datosUsuario,
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DarFTP()
		{
			try
			{
				string rootdir = Server.MapPath("~/Web/resources/FTP");

				string[] files = Directory.GetFiles(rootdir);
				var fileList = files.Select(file => new
				{
					fullname = Path.GetFileName(file),
					name = Path.GetFileNameWithoutExtension(file),
					ext = Path.GetExtension(file).Split('.')[1],
					path = file,
					size = (new FileInfo(file).Length / 1024.0 / 1024.0).ToString("F2"),
					lastModified = System.IO.File.GetLastWriteTime(file)
				}).ToList();

				string[] dirs = Directory.GetDirectories(rootdir);
				var dirList = dirs.Select(dir => new
				{
					name = Path.GetFileName(dir),
					path = dir,
					size = 0.0,
					lastModified = Directory.GetLastWriteTime(dir)
				}).ToList();

				var resp = new
				{
					success = true,
					rootdir = rootdir,
					files = fileList,
					dirs = dirList,
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

		[HttpPost]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult NavegarFTP(string path)
		{
			try
			{
				string[] files = Directory.GetFiles(path);
				var fileList = files.Select(file => new
				{
					fullname = Path.GetFileName(file),
					name = Path.GetFileNameWithoutExtension(file),
					ext = Path.GetExtension(file).Split('.')[1],
					path = file,
					size = (new FileInfo(file).Length / 1024.0 / 1024.0).ToString("F2"),
					lastModified = System.IO.File.GetLastWriteTime(file)
				}).ToList();

				string[] dirs = Directory.GetDirectories(path);
				var dirList = dirs.Select(dir => new
				{
					name = Path.GetFileName(dir),
					path = dir,
					size = 0.0,
					lastModified = Directory.GetLastWriteTime(dir)
				}).ToList();

				var resp = new
				{
					success = true,
					rootdir = path,
					files = fileList,
					dirs = dirList,
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

		[HttpGet]
		[Authorize(Roles = "USER, ADMIN")]
		public ActionResult DescargarArchivo(string rutaArchivo, string nombreArchivo)
		{
			try
			{
				var fs = new FileStream(rutaArchivo, FileMode.Open);
				return File(fs, "application/octet-stream", nombreArchivo);
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

		[HttpPost]
		[Authorize]
		public ActionResult DarUsersActivos()
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Colaborador colab = user.persona.colaborador.FirstOrDefault();
				if(colab == null)
				{
					var resp1 = new
					{
						success = true,
						noColaborador = true
					};
					return Json(resp1);
				}

				var usuarios = (from u in db.Usuario
								join p in db.Persona
		   on u.persona equals p
								join c in db.Colaborador
		  on p equals c.persona
								where u.EstaActivo == 1 && u.Secuencial != user.Secuencial
								orderby p.Nombre1, p.Apellido1
								select new
								{
									id = u.Secuencial,
									email = u.Email,
									nombre = u.persona.Nombre1 + " " + u.persona.Apellido1
								}).ToList();

				var resp = new
				{
					success = true,
					idUsuario = user.Secuencial,
					nombreUsuario = user.persona.Nombre1 + " " + user.persona.Apellido1,
					usuarios = usuarios
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

		[HttpPost]
		[Authorize]
		public ActionResult EnviarMensajeChat(int idUsuarioRecive, string texto)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				var fecha = DateTime.Now;
				Mensaje newMsg = new Mensaje
				{
					Fecha = fecha,
					SecuencialUsuarioEnvia = user.Secuencial,
					SecuencialUsuarioRecibe = idUsuarioRecive,
					Texto = texto,
					Enviado = false
				};

				db.Mensaje.Add(newMsg);
				db.SaveChanges();

				//Enviando la notificacion
				Websocket webSocket = Websocket.getInstance();
				webSocket.ExistenCambios(idUsuarioRecive);

				var resp = new
				{
					success = true,
					fecha = fecha.ToString("dd/MM/yyyy HH:mm:ss")
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

		[HttpPost]
		[Authorize]
		public ActionResult DarMensajeChat()
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				var mensajes = (from m in db.Mensaje
								where m.SecuencialUsuarioRecibe == user.Secuencial && m.Enviado == false
								orderby m.Secuencial
								group m by m.SecuencialUsuarioEnvia into g
								select new
								{
									id = g.Key,
									nombre = g.FirstOrDefault().usuario.persona.Nombre1 + " " + g.FirstOrDefault().usuario.persona.Apellido1,
									mensajes = (from m1 in g
												orderby m1.Secuencial
												select new
												{
													fecha = m1.Fecha,
													texto = m1.Texto
												}
											  )
								}
							   ).ToList<object>();

				//Actualizando a enviados los mensajes
				var mensajesEnviados = (from m in db.Mensaje
										where m.SecuencialUsuarioRecibe == user.Secuencial && m.Enviado == false
										select m).ToList();
				foreach(Mensaje m in mensajesEnviados)
				{
					m.Enviado = true;
				}
				db.SaveChanges();

				var resp = new
				{
					success = true,
					mensajes = mensajes
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

		[HttpPost]
		[Authorize]
		public ActionResult DarNMensajeChat(int idUsuario, int cantidad, int idAnterior = 0, bool ultimo = true)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				if(idAnterior == 0)
				{
					idAnterior = (from m in db.Mensaje
								  where (m.SecuencialUsuarioEnvia == user.Secuencial && m.SecuencialUsuarioRecibe == idUsuario) ||
										(m.SecuencialUsuarioEnvia == idUsuario && m.SecuencialUsuarioRecibe == user.Secuencial)
								  orderby m.Secuencial descending
								  select m.Secuencial).FirstOrDefault();
				}

				var mensajesUser = new List<object>();
				if(ultimo)
				{
					mensajesUser = (from m in db.Mensaje
									where ((m.SecuencialUsuarioEnvia == user.Secuencial && m.SecuencialUsuarioRecibe == idUsuario) ||
											(m.SecuencialUsuarioEnvia == idUsuario && m.SecuencialUsuarioRecibe == user.Secuencial)) &&
											m.Secuencial <= idAnterior
									orderby m.Secuencial descending
									select new
									{
										id = m.Secuencial,
										fecha = m.Fecha,
										nombre = m.SecuencialUsuarioEnvia == user.Secuencial ? "yo" : (db.Usuario.Where(x => x.Secuencial == m.SecuencialUsuarioEnvia).FirstOrDefault().persona.Nombre1 + " " + db.Usuario.Where(x => x.Secuencial == m.SecuencialUsuarioEnvia).FirstOrDefault().persona.Apellido1),
										texto = m.Texto
									}
					).Take(cantidad).ToList<object>();
				}
				else
				{
					mensajesUser = (from m in db.Mensaje
									where ((m.SecuencialUsuarioEnvia == user.Secuencial && m.SecuencialUsuarioRecibe == idUsuario) ||
											(m.SecuencialUsuarioEnvia == idUsuario && m.SecuencialUsuarioRecibe == user.Secuencial)) &&
											m.Secuencial < idAnterior
									orderby m.Secuencial descending
									select new
									{
										id = m.Secuencial,
										fecha = m.Fecha,
										nombre = m.SecuencialUsuarioEnvia == user.Secuencial ? "yo" : (db.Usuario.Where(x => x.Secuencial == m.SecuencialUsuarioEnvia).FirstOrDefault().persona.Nombre1 + " " + db.Usuario.Where(x => x.Secuencial == m.SecuencialUsuarioEnvia).FirstOrDefault().persona.Apellido1),
										texto = m.Texto
									}
					).Take(cantidad).ToList<object>();
				}

				//invirtiendo la lista
				Stack<object> pilaElementos = new Stack<object>();
				List<object> mensajes = new List<object>();

				foreach(var msg in mensajesUser)
				{
					pilaElementos.Push(msg);
				}
				while(pilaElementos.Count > 0)
				{
					mensajes.Add(pilaElementos.Pop());
				}

				var resp = new
				{
					success = true,
					mensajes = mensajes
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

		public string GenerarExcelEstimaciones(int id)
		{
			using(SLDocument sl = new SLDocument())
			{
				var estimacion = db.EstimacionTicket.Find(id);

				sl.SetColumnWidth(1, 13.22);
				sl.SetColumnWidth(2, 46.67);
				sl.SetColumnWidth(3, 22.56);
				sl.SetColumnWidth(4, 10.78);
				sl.SetColumnWidth(5, 10.78);
				sl.SetColumnWidth(6, 10.78);
				sl.SetColumnWidth(7, 10.78);

				SLStyle style8 = sl.CreateStyle();
				style8.SetHorizontalAlignment(HorizontalAlignmentValues.Left);
				style8.SetVerticalAlignment(VerticalAlignmentValues.Justify);
				style8.Font.FontSize = 10;

				SLStyle style1 = sl.CreateStyle();
				style1.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
				style1.Font.Bold = true;
				style1.Font.FontSize = 12;
				style1.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.Blue);
				sl.SetCellStyle(1, 1, style1);
				sl.SetCellValue("A1", "ESTIMACIÓN DE REQUERIMIENTOS");
				sl.MergeWorksheetCells("A1", "G1");

				SLStyle style2 = sl.CreateStyle();
				style2.SetHorizontalAlignment(HorizontalAlignmentValues.Left);
				style2.Font.Bold = true;
				style2.Font.FontSize = 11;
				sl.SetCellStyle(2, 1, style2);
				sl.SetCellStyle(3, 1, style2);
				sl.SetCellStyle(4, 1, style2);
				sl.SetCellStyle(5, 1, style2);
				sl.SetCellStyle(2, 6, style2);
				sl.SetCellStyle(3, 6, style2);
				sl.SetCellStyle(4, 6, style2);
				sl.SetCellValue("A2", "Cliente:");
				sl.SetCellValue("B2", estimacion.ticket.persona_cliente.cliente.Descripcion);
				sl.SetCellValue("A3", "Responsable:");
				string responsable = (
									db.TicketTarea.Where(x => x.SecuencialTicket == estimacion.SecuencialTicket && x.EstaActiva == 1).Count() > 0
							   ) ?
								   (from p in db.Persona
									join c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
									join tar in db.Tarea on c.Secuencial equals tar.SecuencialColaborador
									join ttar in db.TicketTarea on tar.Secuencial equals ttar.SecuencialTarea
									orderby tar.FechaInicio descending
									where ttar.SecuencialTicket == estimacion.SecuencialTicket
									select p.Nombre1 + " " + p.Apellido1).FirstOrDefault()
								 : "NO ASIGNADO";
				sl.SetCellValue("B3", responsable);
				sl.SetCellValue("A4", "Tema:");
				sl.SetCellValue("B4", estimacion.ticket.Asunto);
				sl.SetCellStyle(4, 2, style8);
				sl.SetCellValue("A5", "Descripción:");
				sl.SetCellValue("B5", estimacion.ticket.Detalle);
				sl.SetCellStyle(5, 2, style8);
				sl.SetCellValue("F2", "Fecha Sol.:");
				if(estimacion.ticket.Fecha.HasValue)
				{
					sl.SetCellValue("G2", estimacion.ticket.Fecha.Value.ToString("dd/MM/yyyy"));
				}
				sl.SetCellValue("F3", "Ticket:");
				sl.SetCellValue("G3", estimacion.ticket.Secuencial);
				sl.SetCellValue("F4", "Fecha Est.:");
				sl.SetCellValue("G4", DateTime.Now.ToString("dd/MM/yyyy"));
				sl.MergeWorksheetCells("B5", "G5");

				SLStyle style3 = sl.CreateStyle();
				style3.SetVerticalAlignment(VerticalAlignmentValues.Justify);
				style3.Font.Bold = true;
				style3.Font.FontSize = 10;
				style3.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.Yellow, System.Drawing.Color.Blue);

				SLStyle style4 = sl.CreateStyle();
				style4.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
				style4.SetVerticalAlignment(VerticalAlignmentValues.Justify);
				style4.Font.Bold = true;
				style4.Font.FontSize = 10;
				style4.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.Blue);

				SLStyle style5 = sl.CreateStyle();
				style5.SetVerticalAlignment(VerticalAlignmentValues.Justify);
				style5.Font.Bold = true;
				style5.Font.FontSize = 11;

				SLStyle style6 = sl.CreateStyle();
				style6.SetVerticalAlignment(VerticalAlignmentValues.Justify);
				style6.Font.FontSize = 10;
				style6.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.Blue);

				SLStyle style7 = sl.CreateStyle();
				style7.SetHorizontalAlignment(HorizontalAlignmentValues.Right);
				style7.SetVerticalAlignment(VerticalAlignmentValues.Justify);
				style7.Font.FontSize = 10;
				style7.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.YellowGreen, System.Drawing.Color.Blue);


				var entregables = (from de in db.DetalleEstimacionTicket
								   join et in db.EntregableDetalleEstimacion on de.SecuencialEntregableEstimacion equals et.Secuencial
								   where de.SecuencialEstimacionTicket == id
								   group et by new { et.Secuencial, et.Nombre } into g
								   select new
								   {
									   id = g.Key.Secuencial,
									   nombre = g.Key.Nombre.ToUpper(),
									   detalles = (from d in db.DetalleEstimacionTicket
												   where d.SecuencialEntregableEstimacion == g.Key.Secuencial
												   select new
												   {
													   detalle = d.Detalle,
													   tiempoHoras = d.TiempoDesarrollo + d.TiempoPrueba,
													   tiempoDesarrollo = d.TiempoDesarrollo ?? 0,
													   tiempoPrueba = d.TiempoPrueba ?? 0,
													   nivel = db.NivelColaborador.Where(s => s.Secuencial == d.SecuencialNivelColaborador).FirstOrDefault().Codigo,
												   }).ToList()
								   }).ToList();
				int sum = 0;
				int tiempoTotal = 0;
				foreach(var item in entregables.Select((value, index) => new { value, index }))
				{
					tiempoTotal += (int)item.value.detalles.Sum(s => s.tiempoHoras);
					int cantidad = item.value.detalles.Count;
					int index = item.index;

					sl.SetCellValue("A" + (6 + 2 * (index + 1) + 1 * index - 2 + sum), "ENTREGABLE" + (index + 1) + ": " + item.value.nombre);
					sl.MergeWorksheetCells("A" + (6 + 2 * (index + 1) + 1 * index - 2 + sum), "G" + (6 + 2 * (index + 1) + 1 * index - 2 + sum));

					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index - 2 + sum), 1, style5);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index - 1 + sum), 1, style4);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index - 1 + sum), 2, style4);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index - 1 + sum), 3, style4);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index - 1 + sum), 4, style4);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index - 1 + sum), 5, style4);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index - 1 + sum), 6, style4);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index - 1 + sum), 7, style4);
					sl.SetCellValue("A" + (6 + 2 * (index + 1) + 1 * index - 1 + sum), "N°");
					sl.SetCellValue("B" + (6 + 2 * (index + 1) + 1 * index - 1 + sum), "TAREA PROPUESTA");
					sl.SetCellValue("C" + (6 + 2 * (index + 1) + 1 * index - 1 + sum), "TIPO RECURSO");
					sl.SetCellValue("D" + (6 + 2 * (index + 1) + 1 * index - 1 + sum), "REF. FORMATOS ADAPTAR");
					sl.SetCellValue("E" + (6 + 2 * (index + 1) + 1 * index - 1 + sum), "TIEMPO (H)");
					sl.SetCellValue("F" + (6 + 2 * (index + 1) + 1 * index - 1 + sum), "TIEMPO DES");
					sl.SetCellValue("G" + (6 + 2 * (index + 1) + 1 * index - 1 + sum), "TIEMPO PRU");

					foreach(var it in item.value.detalles.Select((v, i) => new { v, i }))
					{
						sl.SetCellValue("A" + (6 + 2 * (index + 1) + 1 * index + sum + it.i), "TAR" + (it.i + 1));
						sl.SetCellValue("B" + (6 + 2 * (index + 1) + 1 * index + sum + it.i), it.v.detalle);
						sl.SetCellValue("C" + (6 + 2 * (index + 1) + 1 * index + sum + it.i), it.v.nivel);
						sl.SetCellValue("D" + (6 + 2 * (index + 1) + 1 * index + sum + it.i), "");
						sl.SetCellValue("E" + (6 + 2 * (index + 1) + 1 * index + sum + it.i), it.v.tiempoHoras ?? 0);
						sl.SetCellValue("F" + (6 + 2 * (index + 1) + 1 * index + sum + it.i), it.v.tiempoDesarrollo);
						sl.SetCellValue("G" + (6 + 2 * (index + 1) + 1 * index + sum + it.i), it.v.tiempoPrueba);
					}
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), 4, style3);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), 5, style3);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), 6, style6);
					sl.SetCellStyle((6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), 7, style6);
					sl.MergeWorksheetCells("A" + (6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), "C" + (6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count));
					sl.SetCellValue("D" + (6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), "TOTAL:");
					sl.SetCellValue("E" + (6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), item.value.detalles.Sum(s => s.tiempoHoras ?? 0));
					sl.SetCellValue("F" + (6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), item.value.detalles.Sum(s => s.tiempoDesarrollo));
					sl.SetCellValue("G" + (6 + 2 * (index + 1) + 1 * index + sum + item.value.detalles.Count), item.value.detalles.Sum(s => s.tiempoPrueba));
					sum += cantidad;
				}

				sl.SetCellStyle((6 + 3 * entregables.Count + sum + 1), 1, style7);
				sl.SetCellStyle((6 + 3 * entregables.Count + sum + 1), 5, style7);
				sl.MergeWorksheetCells("A" + (6 + 3 * entregables.Count + sum + 1), "D" + (6 + 3 * entregables.Count + sum + 1));
				sl.SetCellValue("A" + (6 + 3 * entregables.Count + sum + 1), "TIEMPO FINAL:");
				sl.SetCellValue("E" + (6 + 3 * entregables.Count + sum + 1), tiempoTotal);

				string newNameFile = "est_" + Utiles.RandomString(10) + ".xlsx";
				string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
				sl.SaveAs(path);

				AdjuntoTicket adjTicket = new AdjuntoTicket
				{
					Url = "/resources/tickets/" + newNameFile,
					ticket = db.Ticket.Where(s => s.Secuencial == estimacion.SecuencialTicket).FirstOrDefault()
				};
				db.AdjuntoTicket.Add(adjTicket);
				db.SaveChanges();
				return newNameFile;
			}
		}
	}
}
