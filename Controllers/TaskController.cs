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
using System.Threading.Tasks;
using SpreadsheetLight;
using System.Web.UI.WebControls;
using System.Data.Entity;
using System.Globalization;
using System.Transactions;

namespace SifizPlanning.Controllers
{
	public class TaskController : Controller
	{
		SifizPlanningEntidades db = DbCnx.getCnx();
		//
		// GET: /Task/

		[Authorize(Roles = "ADMIN, REC, ACTAS")]
		public ActionResult Index()
		{
			return View();
		}

		//---------------INICIO--------------------
		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult GraficoTareasDia()
		{
			DateTime hoy = DateTime.Today;
			DateTime mannana = hoy.AddDays(1);
			int t1 = (from t in db.Tarea
					  where t.FechaInicio >= hoy && t.FechaInicio < mannana && t.SecuencialEstadoTarea == 1
					  select t).Count();

			int t2 = (from t in db.Tarea
					  where t.FechaInicio >= hoy && t.FechaInicio < mannana && t.SecuencialEstadoTarea == 2
					  select t).Count();

			int t3 = (from t in db.Tarea
					  where t.FechaInicio >= hoy && t.FechaInicio < mannana && t.SecuencialEstadoTarea == 3
					  select t).Count();

			int t4 = (from t in db.Tarea
					  where t.FechaInicio >= hoy && t.FechaInicio < mannana && t.SecuencialEstadoTarea == 5
					  select t).Count();

			List<object> datos = new List<object>();
			datos.Add(new
			{
				name = "ASIGNADA",
				y = t1
			});
			datos.Add(new
			{
				name = "DESARROLLO",
				y = t2
			});
			datos.Add(new
			{
				name = "TERMINADA",
				y = t3
			});
			datos.Add(new
			{
				name = "PAUSA",
				y = t4
			});

			var resp = new
			{
				success = true,
				datos = datos
			};
			return Json(resp);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult GraficoTareasSemana()
		{
			DateTime hoy = DateTime.Today;
			DayOfWeek diaSemana = hoy.DayOfWeek;
			long tiempo = (int)diaSemana;
			TimeSpan time = new TimeSpan(tiempo * 864000000000);
			DateTime domingo = hoy.Subtract(time);

			DateTime lunes = domingo.AddDays(1);
			DateTime nextLunes = lunes.AddDays(7);

			int t1 = (from t in db.Tarea
					  where t.FechaInicio >= lunes && t.FechaInicio < nextLunes && t.SecuencialEstadoTarea == 1
					  select t).Count();

			int t2 = (from t in db.Tarea
					  where t.FechaInicio >= lunes && t.FechaInicio < nextLunes && t.SecuencialEstadoTarea == 2
					  select t).Count();

			int t3 = (from t in db.Tarea
					  where t.FechaInicio >= lunes && t.FechaInicio < nextLunes && t.SecuencialEstadoTarea == 3
					  select t).Count();

			int t4 = (from t in db.Tarea
					  where t.FechaInicio >= lunes && t.FechaInicio < nextLunes && t.SecuencialEstadoTarea == 5
					  select t).Count();

			List<object> datos = new List<object>();
			datos.Add(new
			{
				name = "ASIGNADA",
				y = t1
			});
			datos.Add(new
			{
				name = "DESARROLLO",
				y = t2
			});
			datos.Add(new
			{
				name = "TERMINADA",
				y = t3
			});
			datos.Add(new
			{
				name = "PAUSA",
				y = t4
			});

			var resp = new
			{
				success = true,
				datos = datos
			};
			return Json(resp);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult GraficoTareasDesde7Dias()
		{
			DateTime hoy = DateTime.Today;
			DateTime manana = hoy.AddDays(1);
			DateTime inicio = hoy.AddDays(-7);


			int t1 = (from t in db.Tarea
					  where t.FechaInicio >= inicio && t.FechaInicio < manana && t.SecuencialEstadoTarea == 1
					  select t).Count();

			int t2 = (from t in db.Tarea
					  where t.FechaInicio >= inicio && t.FechaInicio < manana && t.SecuencialEstadoTarea == 2
					  select t).Count();

			int t3 = (from t in db.Tarea
					  where t.FechaInicio >= inicio && t.FechaInicio < manana && t.SecuencialEstadoTarea == 3
					  select t).Count();

			int t4 = (from t in db.Tarea
					  where t.FechaInicio >= inicio && t.FechaInicio < manana && t.SecuencialEstadoTarea == 5
					  select t).Count();

			List<object> datos = new List<object>();
			datos.Add(new
			{
				name = "ASIGNADA",
				y = t1
			});
			datos.Add(new
			{
				name = "DESARROLLO",
				y = t2
			});
			datos.Add(new
			{
				name = "TERMINADA",
				y = t3
			});
			datos.Add(new
			{
				name = "PAUSA",
				y = t4
			});

			var resp = new
			{
				success = true,
				datos = datos
			};
			return Json(resp);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult GraficoTareasDesde30Dias()
		{
			DateTime hoy = DateTime.Today;
			DateTime manana = hoy.AddDays(1);
			DateTime inicio = hoy.AddDays(-30);

			List<Tarea> tareas = (from t in db.Tarea
								  where t.FechaInicio >= inicio && t.FechaInicio < manana && t.SecuencialEstadoTarea != 4
								  orderby t.FechaInicio
								  select t).ToList();

			List<string> axisX = new List<string>();
			List<int> t1 = new List<int>();
			List<int> t2 = new List<int>();
			List<int> t3 = new List<int>();
			List<int> t4 = new List<int>();
			DateTime fechaAux = inicio;
			DateTime fechaAuxFin = inicio.AddDays(1);
			while(fechaAux < manana)
			{
				int cantt1 = (from t in tareas
							  where t.FechaInicio >= fechaAux && t.FechaInicio < fechaAuxFin && t.SecuencialEstadoTarea == 1
							  select t).Count();

				int cantt2 = (from t in tareas
							  where t.FechaInicio >= fechaAux && t.FechaInicio < fechaAuxFin && t.SecuencialEstadoTarea == 2
							  select t).Count();

				int cantt3 = (from t in tareas
							  where t.FechaInicio >= fechaAux && t.FechaInicio < fechaAuxFin && t.SecuencialEstadoTarea == 3
							  select t).Count();
				int cantt4 = (from t in tareas
							  where t.FechaInicio >= fechaAux && t.FechaInicio < fechaAuxFin && t.SecuencialEstadoTarea == 5
							  select t).Count();

				axisX.Add(fechaAux.ToString("dd/MM"));
				t1.Add(cantt1);
				t2.Add(cantt2);
				t3.Add(cantt3);
				t4.Add(cantt4);

				fechaAux = fechaAux.AddDays(1);
				fechaAuxFin = fechaAuxFin.AddDays(1);
			}
			List<object> datos = new List<object>();
			datos.Add(new
			{
				name = "ASIGNADAS",
				data = t1
			}
					);
			datos.Add(new
			{
				name = "DESARROLLO",
				data = t2
			}
					);
			datos.Add(new
			{
				name = "TERMINADAS",
				data = t3
			}
					);
			datos.Add(new
			{
				name = "PAUSA",
				data = t4
			}
					);

			var resp = new
			{
				success = true,
				datos = datos,
				axisX = axisX
			};
			return Json(resp);
		}

		//------------- ACTION DE LAS TAREAS ------------------------


		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult SubirExcel(HttpPostedFileBase file)
		{
			if(file != null)
			{
				try
				{
					//Para que la funcionalidad de excel continue funcionando, debe existir la carpeta Tareas en el disco D: del servidor, o cambiar la ubicacion donde se va a subir y leer
					string formato = "IngresarTareas.xlsx";
					string ruta = Path.Combine(Server.MapPath("~/Web/resources/tasks/"), formato);
					//string ruta = "D:/Tareas/IngresarTareas.xlsx";

					file.SaveAs(ruta);

					return Json(new
					{
						success = true,
						msg = "Archivo guardado exitosamente"
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
			else
			{
				return Json(new
				{
					success = false,
					msg = "No se ha cargado ningun archivo"
				});
			}
		}


		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult LeerExcel()
		{
			try
			{
				string formato = "IngresarTareas.xlsx";
				string ruta = Path.Combine(Server.MapPath("~/Web/resources/tasks/"), formato);
				//string ruta = "D:/Tareas/IngresarTareas.xlsx";

				SLDocument sl = new SLDocument(ruta);

				List<object> datos = new List<object>();

				int fila = 2;

				while(!string.IsNullOrEmpty(sl.GetCellValueAsString(fila, 1)))
				{
					string colaborador = sl.GetCellValueAsString(fila, 1).ToUpper();
					DateTime fecha = sl.GetCellValueAsDateTime(fila, 2);
					string cliente = sl.GetCellValueAsString(fila, 3).ToUpper();
					string ubicacion = sl.GetCellValueAsString(fila, 4).ToUpper();
					string actividad = sl.GetCellValueAsString(fila, 5).ToUpper();
					string modulo = sl.GetCellValueAsString(fila, 6).ToUpper();
					int numeroHoras = sl.GetCellValueAsInt32(fila, 7);
					string coordinador = sl.GetCellValueAsString(fila, 8).ToUpper();
					string detalle = sl.GetCellValueAsString(fila, 9).ToUpper();
					int repetirNumVeces = sl.GetCellValueAsInt32(fila, 10);

					if(numeroHoras > 8)
					{
						return Json(new
						{
							success = false,
							msg = "Error al ingresar tareas. Revisar la duracion de la tarea en la fila " + fila + ", la duracion no puede ser mayor a 8 horas. Se recomienda reducir el tiempo de la tarea y repetirla las veces que sea necesario hasta cubrir las horas deseadas"
						});
					}

					if(fecha < new DateTime(2015, 01, 01))
					{
						return Json(new
						{
							success = false,
							msg = "Error al ingresar tareas. Revisar la fecha en la fila " + fila + " ya que contiene un valor erroneo y no puede ser menor a 01/01/2015"
						});
					}

					//colaborador = colaborador + "@SIFIZSOFT.COM";
					string fechaString = fecha.Day + "/" + fecha.Month + "/" + fecha.Year;
					int secuencialColaborador = 0;
					int secuencialCliente = 0;
					int secuencialLugar = 0;
					int secuencialActividad = 0;
					int secuencialModulo = 0;
					int secuencialCoordinador = 0;

					try
					{
						secuencialColaborador = (from p in db.Persona
												 join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
												 join c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
												 where u.Email.ToUpper().Contains(colaborador)
												 select c.Secuencial).ToList()[0];
					}
					catch(Exception)
					{
						return Json(new
						{
							success = false,
							msg = "Error al ingresar tareas. Revisar la columna Colaborador en la fila " + fila + " ya que contiene un valor erroneo"
						});
					}

					try
					{
						secuencialCliente = (from c in db.Cliente
											 where c.Codigo == cliente
											 select c.Secuencial).ToList()[0];
					}
					catch(Exception)
					{
						return Json(new
						{
							success = false,
							msg = "Error al ingresar tareas. Revisar la columna Cliente en la fila " + fila + " ya que contiene un valor erroneo"
						});
					}

					try
					{
						secuencialLugar = (from lt in db.LugarTarea
										   where lt.Codigo == ubicacion
										   select lt.Secuencial).ToList()[0];
					}
					catch(Exception)
					{
						return Json(new
						{
							success = false,
							msg = "Error al ingresar tareas. Revisar la columna Ubicacion en la fila " + fila + " ya que contiene un valor erroneo"
						});
					}

					try
					{
						secuencialActividad = (from a in db.Actividad
											   where a.Descripcion == actividad
											   select a.Secuencial).ToList()[0];
					}
					catch(Exception)
					{
						return Json(new
						{
							success = false,
							msg = "Error al ingresar tareas. Revisar la columna Actividad en la fila " + fila + " ya que contiene un valor erroneo"
						});
					}

					try
					{
						secuencialModulo = (from m in db.Modulo
											where m.Codigo == modulo
											select m.Secuencial).ToList()[0];
					}
					catch(Exception)
					{
						return Json(new
						{
							success = false,
							msg = "Error al ingresar tareas. Revisar la columna Modulo en la fila " + fila + " ya que contiene un valor erroneo"
						});
					}

					try
					{
						secuencialCoordinador = (from p in db.Persona
												 join c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
												 join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
												 where u.Email.ToUpper().Contains(coordinador)
												 select c.Secuencial).ToList()[0];
					}
					catch(Exception)
					{
						return Json(new
						{
							success = false,
							msg = "Error al ingresar tareas. Revisar la columna Coordinador en la fila " + fila + " ya que contiene un valor erroneo"
						});
					}

					string repetir = "";
					int repetirTipoFin = 0;

					if(repetirNumVeces > 0)
					{
						repetir = "DIARIO";
						repetirTipoFin = 1;
					}

					datos.Add(new
					{
						idColaborador = secuencialColaborador,
						fechaText = fechaString,
						idCliente = secuencialCliente,
						idLugar = secuencialLugar,
						idModulo = secuencialModulo,
						idActividad = secuencialActividad,
						numHoras = numeroHoras,
						detalleText = detalle,
						idReferencia = 0,
						idCoordinador = secuencialCoordinador,
						repetirText = repetir,
						idFinDeSemana = 0,
						idRepetirTipoFin = repetirTipoFin,
						numeroVeces = repetirNumVeces,
						fechaHasta = "",
						idTarea = 0,
						numeroVerificador = 0,
						idEstadoTarea = 6
					});

					fila++;
				}

				sl.CloseWithoutSaving();

				//Una vez leido el archivo, se elimina para evitar que sea leido por error a futuro
				string excel = "IngresarTareas.xlsx";
				string path = Path.Combine(Server.MapPath("~/Web/resources/tasks/"), excel);
				System.IO.File.Delete(path);
				//System.IO.File.Delete("D:/Tareas/IngresarTareas.xlsx");

				foreach(var d in datos)
				{

					JsonResult jresp = NuevaTareaExcel(int.Parse(d.GetType().GetProperty("idColaborador").GetValue(d).ToString()),
						d.GetType().GetProperty("fechaText").GetValue(d).ToString(),
						int.Parse(d.GetType().GetProperty("idCliente").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("idLugar").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("idModulo").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("idActividad").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("numHoras").GetValue(d).ToString()),
						d.GetType().GetProperty("detalleText").GetValue(d).ToString(),
						int.Parse(d.GetType().GetProperty("idReferencia").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("idCoordinador").GetValue(d).ToString()),
						d.GetType().GetProperty("repetirText").GetValue(d).ToString(),
						int.Parse(d.GetType().GetProperty("idFinDeSemana").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("idRepetirTipoFin").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("numeroVeces").GetValue(d).ToString()),
						d.GetType().GetProperty("fechaHasta").GetValue(d).ToString(),
						int.Parse(d.GetType().GetProperty("idTarea").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("numeroVerificador").GetValue(d).ToString()),
						int.Parse(d.GetType().GetProperty("idEstadoTarea").GetValue(d).ToString()));

					var success = jresp.Data.GetType().GetProperty("success").GetValue(jresp.Data).ToString();
					var msg = jresp.Data.GetType().GetProperty("msg").GetValue(jresp.Data).ToString();

					if(success.Equals("False"))
					{
						return Json(new
						{
							success = false,
							msg
						});
					}
				}

				return Json(new
				{
					success = true,
					msg = "Se han ingresado correctamente las tareas"
				});

			}
			catch(Exception e)
			{
				return Json(new
				{
					success = false,
					msg = e.Message
					//Rafael Vinueza programó aqui :v
				});
			}

		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
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
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult DarTareasTrabajadores(string fechaLunes = "", int semanas = 1, string json = "", bool coordinados = false)
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
			List<int> idDepartamentos = new List<int>();
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
				for(int i = 0; i < jsonObj["departamento"].Length; i++)
				{
					idDepartamentos.Add(int.Parse(jsonObj["departamento"][i]["id"]));
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
						 join d in db.Departamento on c.departamento equals d

						 where u.EstaActivo == 1 &&
							   d.Asignable == 1 &&
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
							 departamento = d.Codigo,
							 dDepartamento = d.Descripcion.ToUpper(),
							 idDepartamento = d.Secuencial,
							 clase = (t.SecuencialEstadoTarea == 1 ? "new" : (t.SecuencialEstadoTarea == 2) ? "dev" : (t.SecuencialEstadoTarea == 3) ? "finish" : (t.SecuencialEstadoTarea == 5) ? "pause" : (t.SecuencialEstadoTarea == 6) ? "preassigned" : "no-concluida"),
							 coordinador = (from tc in db.Tarea_Coordinador
											join
												co in db.Colaborador on tc.colaborador equals co
											join
												pe in db.Persona on co.persona equals pe

											where tc.SecuencialTarea == t.Secuencial && tc.EstaActivo == 1
											select (pe.Nombre1 + " " + pe.Apellido1)).FirstOrDefault(),
							 compensatoria = (t.tareaCompensatoria.FirstOrDefault() != null
											  && t.tareaCompensatoria.FirstOrDefault().EstaActiva == 1
											 ) ? "compensatoria" : "no-compensatoria"
						 }).ToList();

			var trabajadores = (from t in db.Colaborador
								join
									p in db.Persona on t.persona equals p
								join
									f in db.FotoColaborador on t.Secuencial equals f.SecuencialColaborador
								join
									s in db.Sede on t.sede equals s
								join
									u in db.Usuario on p.Secuencial equals u.SecuencialPersona
								join d in db.Departamento on t.departamento equals d

								where u.EstaActivo == 1 && (d.Asignable == 1)
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
				datos = datos.Where(x => idColaboradores.Contains(x.idColaborador)).ToList();
				trabajadores = trabajadores.Where(x => idColaboradores.Contains(x.id)).ToList();
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
			List<DateTime> diasFeriados = db.Feriados.Where(x => x.Fecha >= lunes && x.Fecha < fechaFin).Select(x => x.Fecha).ToList<DateTime>();

			List<Object> tareasProgramadores = new List<Object>();
			int cant = trabajadores.Count();

			trabajadores.Sort((x1, x2) => String.Compare(x1.nombre, x2.nombre));

			for(int i = 0; i < cant; i++)
			{
				int idTrabajador = trabajadores[i].id;
				List<Object> tareasPorDia = new List<Object>();
				int countTareas = 0;
				for(int j = 0; j < 7 * semanas; j++)//son 7 Días los de la semana
				{
					DateTime fecha = lunes.AddDays(j);
					DateTime fechaDespues = lunes.AddDays(j + 1);
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
								  departamento = d.departamento,
								  dDepartamento = d.dDepartamento,
								  idDepartamento = d.idDepartamento,
								  clase = d.clase,
								  coordinador = d.coordinador,
								  tipo = "t",
								  compensatoria = d.compensatoria
							  }).ToList<DataTarea>();

					//Sobre los permisos
					List<DataTarea> lPermisos = new List<DataTarea>();
					lPermisos = (from per in permisos
								 where per.idColaborador == idTrabajador &&
									   per.finicio.Date <= fecha.Date && per.ffin.Date >= fecha.Date
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
									 departamento = "",
									 dDepartamento = "",
									 idDepartamento = 0,
									 clase = per.clase,
									 coordinador = null,
									 tipo = "p",
									 compensatoria = "no-compensatoria"
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
					if(idDepartamentos.Count() > 0)
					{
						tareas = tareas.Where(x => idDepartamentos.Contains(x.idDepartamento)).ToList();
					}
					if(idModulos.Count() > 0)
					{
						tareas = tareas.Where(x => idModulos.Contains(x.idModulo)).ToList();
					}
					//Ordenando los permisos
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

				if(json == "" || countTareas > 0 || idColaboradores.Contains(idTrabajador) || idSedes.Contains(trabajadores[i].idSede))
				{
					var trab = new
					{
						trab = trabajadores[i],
						tareasPorDia = tareasPorDia
					};

					//Aquí la lógica de si la vista es de ver los coordinados                                        
					if(coordinados == true && idTrabajador == idColaboradores.First())
					{
						tareasProgramadores.Insert(0, trab);
					}
					else
					{
						tareasProgramadores.Add(trab);
					}
				}
			}

			var respx = new
			{
				success = true,
				trabajadores = tareasProgramadores
			};
			return Json(respx);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public JsonResult NuevaTareaExcel(int idTrabajador, string fecha, int cliente, int ubicacion, int modulo, int actividad, int horas, string detalle, int referencia = 0, int coordinador = 0, string repetir = "", int finSemana = 0, int repetirTipoFin = 0, int numVeces = 0, string fechaHasta = "", int idTarea = 0, int verificador = 0, int idEstado = 2)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				string[] fechas = fecha.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				DateTime diaTarea = new System.DateTime(anno, mes, dia);
				DateTime diaSiguiente = diaTarea.AddDays(1);
				Tarea tareaPrincipal = null;

				//para la actualizacion de los permisos y las tareas
				List<DiaColaborador> listaDiaColaborador = new List<DiaColaborador>();

				//Para la actualizacion de los cambios en IU en los TD de la tabla
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();

				//Buscando la suma de las horas de las tareas del mismo día
				var tareasDelDia = (from t in db.Tarea
									where
										t.SecuencialColaborador == idTrabajador &&
										t.FechaInicio >= diaTarea &&
										t.FechaInicio < diaSiguiente &&
										t.SecuencialEstadoTarea != 4
									select new
									{
										finicio = t.FechaInicio,
										ffin = t.FechaFin
									}).ToList();
				int time = 0;
				int timeMinutos = 0;
				foreach(var tarea in tareasDelDia)
				{
					TimeSpan tiempo = tarea.ffin - tarea.finicio;
					time += tiempo.Hours;
					timeMinutos += tiempo.Minutes;
				}

				DateTime fechaInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
				fechaInicio = fechaInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
				DateTime fechaFin = fechaInicio.AddHours(horas);

				if(fechaInicio.Hour < 13 && fechaFin.Hour > 13)
				{
					fechaFin = fechaFin.AddHours(1);
				}
				else if(fechaInicio.Hour == 13)
				{
					fechaInicio = fechaInicio.AddHours(1);
					fechaFin = fechaFin.AddHours(1);
				}

				Tarea tar = new Tarea
				{
					SecuencialColaborador = idTrabajador,
					SecuencialActividad = actividad,
					SecuencialModulo = modulo,
					SecuencialCliente = cliente,
					SecuencialEstadoTarea = idEstado,
					SecuencialLugarTarea = ubicacion,
					Detalle = detalle.ToUpper(),
					FechaInicio = fechaInicio,
					FechaFin = fechaFin,
					HorasUtilizadas = 0,
					NumeroVerificador = 1
				};

				if(referencia != 0)//Actualizar la referencia
				{
					if((tar.entregableMotivoTrabajo != null && tar.entregableMotivoTrabajo.Secuencial != referencia) || tar.entregableMotivoTrabajo == null)
					{
						EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
						if(entregable != null)
						{
							tar.entregableMotivoTrabajo = entregable;
						}
					}
				}

				db.Tarea.Add(tar);
				//Para la actualizacion de las tareas y de los permisos
				listaDiaColaborador.Add(new DiaColaborador { Fecha = tar.FechaInicio.Date, IdColaborador = tar.SecuencialColaborador });
				listaCambiosTareas.Add(new DiaColaborador { Fecha = tar.FechaInicio.Date, IdColaborador = tar.SecuencialColaborador });

				if(coordinador != 0)
				{
					db.Tarea_Coordinador.Add(
						new Tarea_Coordinador
						{
							tarea = tar,
							SecuencialColaborador = coordinador,
							EstaActivo = 1,
							NumeroVerificador = 1
						}
					);
				}

				HistoricoTareaEstado histET = new HistoricoTareaEstado
				{
					tarea = tar,
					SecuencialEstadoTarea = idEstado,
					FechaOperacion = DateTime.Now,
					usuario = user
				};

				db.HistoricoTareaEstado.Add(histET);

				tareaPrincipal = tar;


				//Ver si se repite la tarea
				if(repetir != "")//Aqui se repite la tarea
				{
					DateTime diaT = diaTarea;

					if(repetirTipoFin == 1)
					{//Veces
						int i = 1;
						while(i < numVeces)
						{

							if(repetir == "SEMANAL")
							{
								diaT = diaT.AddDays(7);
							}
							else if(repetir == "MENSUAL")
							{
								diaT = diaT.AddMonths(1);
							}
							else
							{//Diario
								diaT = diaT.AddDays(1);

								DayOfWeek diaSemana = diaT.DayOfWeek;
								long diaSem = (int)diaSemana;
								if(finSemana == 0)//No se contemplan los fines de semana
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(2);
									}
									else if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 1)//Se incluye el sábado no el domingo
								{
									if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 2)//Se incluye el domingo no el sábado
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(1);
									}
								}
							}

							DateTime diaT2 = diaT.AddDays(1);

							//Buscando la suma de las horas de las tareas del mismo día
							var tareasDia = (from t in db.Tarea
											 where t.SecuencialColaborador == idTrabajador &&
												   t.FechaInicio >= diaT && t.FechaInicio < diaT2 &&
												   t.SecuencialEstadoTarea != 4
											 select new
											 {
												 finicio = t.FechaInicio,
												 ffin = t.FechaFin
											 }).ToList();
							time = 0;
							timeMinutos = 0;
							foreach(var tarea in tareasDia)
							{
								TimeSpan tiempo = tarea.ffin - tarea.finicio;
								time += tiempo.Hours;
								timeMinutos += tiempo.Minutes;
							}

							DateTime fInicio = diaT.AddHours(time).AddMinutes(timeMinutos);
							fInicio = fInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
							DateTime fFin = fInicio.AddHours(horas);

							if(fInicio.Hour < 13 && fFin.Hour > 13)
							{
								fFin = fFin.AddHours(1);
							}
							else if(fInicio.Hour == 13)
							{
								fInicio = fInicio.AddHours(1);
								fFin = fFin.AddHours(1);
							}

							Tarea tar1 = new Tarea
							{
								SecuencialColaborador = idTrabajador,
								SecuencialActividad = actividad,
								SecuencialModulo = modulo,
								SecuencialCliente = cliente,
								SecuencialEstadoTarea = idEstado,
								SecuencialLugarTarea = ubicacion,
								Detalle = detalle.ToUpper(),
								FechaInicio = fInicio,
								FechaFin = fFin,
								HorasUtilizadas = 0,
								NumeroVerificador = 1
							};

							if(referencia != 0)//Actualizar la referencia
							{
								if((tar1.entregableMotivoTrabajo != null && tar1.entregableMotivoTrabajo.Secuencial != referencia) || tar1.entregableMotivoTrabajo == null)
								{
									EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
									if(entregable != null)
									{
										tar1.entregableMotivoTrabajo = entregable;
									}
								}
							}

							db.Tarea.Add(tar1);
							//Para la actualizacion de las tareas y de los permisos
							listaDiaColaborador.Add(new DiaColaborador { Fecha = tar1.FechaInicio.Date, IdColaborador = tar1.SecuencialColaborador });
							listaCambiosTareas.Add(new DiaColaborador { Fecha = tar1.FechaInicio.Date, IdColaborador = tar1.SecuencialColaborador });

							if(coordinador != 0)
							{
								db.Tarea_Coordinador.Add(
									new Tarea_Coordinador
									{
										tarea = tar1,
										SecuencialColaborador = coordinador,
										EstaActivo = 1,
										NumeroVerificador = 1
									}
								);
							}

							HistoricoTareaEstado histET1 = new HistoricoTareaEstado
							{
								tarea = tar1,
								SecuencialEstadoTarea = idEstado,
								FechaOperacion = DateTime.Now,
								usuario = user
							};

							db.HistoricoTareaEstado.Add(histET1);

							Tarea_TareaRelacionada tareaTReacionada = new Tarea_TareaRelacionada
							{
								tarea1 = tar1,
								tarea = tareaPrincipal,
								NumeroVerificador = 1
							};
							db.Tarea_TareaRelacionada.Add(tareaTReacionada);

							i++;
						}
					}
					else if(repetirTipoFin == 2)
					{//Fecha

						string[] fechasHasta = fechaHasta.Split(new Char[] { '/' });
						int diaHasta = Int32.Parse(fechasHasta[0]);
						int mesHasta = Int32.Parse(fechasHasta[1]);
						int annoHasta = Int32.Parse(fechasHasta[2]);
						DateTime diaTareaHasta = new System.DateTime(annoHasta, mesHasta, diaHasta);

						while(diaTareaHasta > diaT)
						{
							if(repetir == "SEMANAL")
							{
								diaT = diaT.AddDays(7);
							}
							else if(repetir == "MENSUAL")
							{
								diaT = diaT.AddMonths(1);
							}
							else
							{//Diario
								diaT = diaT.AddDays(1);
								DayOfWeek diaSemana = diaT.DayOfWeek;
								long diaSem = (int)diaSemana;
								if(finSemana == 0)//No se contemplan los fines de semana
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(2);
									}
									else if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 1)//Se incluye el sábado no el domingo
								{
									if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 2)//Se incluye el domingo no el sábado
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(1);
									}
								}
							}
							if(diaTareaHasta < diaT)//Romper por si se pasa
								break;

							DateTime diaT2 = diaT.AddDays(1);

							//Buscando la suma de las horas de las tareas del mismo día
							var tareasDia = (from t in db.Tarea
											 where t.SecuencialColaborador == idTrabajador &&
												   t.FechaInicio >= diaT && t.FechaInicio < diaT2 &&
												   t.SecuencialEstadoTarea != 4
											 select new
											 {
												 finicio = t.FechaInicio,
												 ffin = t.FechaFin
											 }).ToList();
							time = 0;
							timeMinutos = 0;
							foreach(var tarea in tareasDia)
							{
								TimeSpan tiempo = tarea.ffin - tarea.finicio;
								time += tiempo.Hours;
								timeMinutos += tiempo.Minutes;
							}

							DateTime fInicio = diaT.AddHours(time).AddMinutes(timeMinutos);
							fInicio = fInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
							DateTime fFin = fInicio.AddHours(horas);

							if(fInicio.Hour < 13 && fFin.Hour > 13)
							{
								fFin = fFin.AddHours(1);
							}
							else if(fInicio.Hour == 13)
							{
								fInicio = fInicio.AddHours(1);
								fFin = fFin.AddHours(1);
							}

							Tarea tar1 = new Tarea
							{
								SecuencialColaborador = idTrabajador,
								SecuencialActividad = actividad,
								SecuencialModulo = modulo,
								SecuencialCliente = cliente,
								SecuencialEstadoTarea = idEstado,
								SecuencialLugarTarea = ubicacion,
								Detalle = detalle.ToUpper(),
								FechaInicio = fInicio,
								FechaFin = fFin,
								HorasUtilizadas = 0,
								NumeroVerificador = 1
							};

							if(referencia != 0)//Actualizar la referencia
							{
								if((tar1.entregableMotivoTrabajo != null && tar1.entregableMotivoTrabajo.Secuencial != referencia) || tar1.entregableMotivoTrabajo == null)
								{
									EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
									if(entregable != null)
									{
										tar1.entregableMotivoTrabajo = entregable;
									}
								}
							}

							db.Tarea.Add(tar1);
							//Para la actualizacion de las tareas y de los permisos
							listaDiaColaborador.Add(new DiaColaborador { Fecha = tar1.FechaInicio.Date, IdColaborador = tar1.SecuencialColaborador });
							listaCambiosTareas.Add(new DiaColaborador { Fecha = tar1.FechaInicio.Date, IdColaborador = tar1.SecuencialColaborador });

							if(coordinador != 0)
							{
								db.Tarea_Coordinador.Add(
									new Tarea_Coordinador
									{
										tarea = tar1,
										SecuencialColaborador = coordinador,
										EstaActivo = 1,
										NumeroVerificador = 1
									}
								);
							}

							HistoricoTareaEstado histET1 = new HistoricoTareaEstado
							{
								tarea = tar1,
								SecuencialEstadoTarea = idEstado,
								FechaOperacion = DateTime.Now,
								usuario = user
							};

							db.HistoricoTareaEstado.Add(histET1);

							Tarea_TareaRelacionada tareaTReacionada = new Tarea_TareaRelacionada
							{
								tarea1 = tar1,
								tarea = tareaPrincipal,
								NumeroVerificador = 1
							};
							db.Tarea_TareaRelacionada.Add(tareaTReacionada);
						}
					}
				}

				string msg = "Se ha creado correctamente la tarea";
				if(repetir != "")
				{
					if(idTarea != 0)
					{
						msg = "Se han actualizado correctamente las tareas.";
					}
					else
					{
						msg = "Se han creado correctamente las tareas.";
					}
				}
				else
				{
					if(idTarea != 0)
					{
						msg = "Se ha actualizado correctamente la tarea.";
					}
				}

				db.SaveChanges();//Salvando los cambios   

				//Actualizando las fechas de los dias que existieron los cambios y en las fechas
				foreach(DiaColaborador diaColab in listaDiaColaborador)
				{
					Utiles.OrdenarTareasPermisos(diaColab.Fecha, diaColab.IdColaborador, user, db);
				}

				//Actualizando cambios en la interfaz de usuario
				ActualizarTDTarea(listaCambiosTareas);

				var respOk = new
				{
					success = true,
					msg = msg
				};
				return Json(respOk);

			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message
					//msg = "Hay errores en los datos por favor verifíquelos"
				};
				return Json(resp);
			}

		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public JsonResult NuevaTarea(int idTrabajador, string fecha, int cliente, int ubicacion, int modulo, int actividad, int horas, int minutos, int horasEstimadas, int minutosEstimados, string detalle, int referencia = 0, int coordinador = 0, string repetir = "", int finSemana = 0, int repetirTipoFin = 0, int numVeces = 0, string fechaHasta = "", bool extraordinaria = false, int ticketTarea = 0, int idTarea = 0, int verificador = 0, bool esReproceso = false)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				string[] fechas = fecha.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				DateTime diaTarea = new System.DateTime(anno, mes, dia);
				DateTime diaSiguiente = diaTarea.AddDays(1);
				Tarea tareaPrincipal = null;

				//para la actualizacion de los permisos y las tareas
				List<DiaColaborador> listaDiaColaborador = new List<DiaColaborador>();

				//Para la actualizacion de los cambios en IU en los TD de la tabla
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();

				if(idTarea != 0)
				{
					//Buscando la tarea
					Tarea tarea = (from t in db.Tarea
								   where t.Secuencial == idTarea
								   select t).FirstOrDefault();
					if(tarea == null)
					{
						throw new Exception("No se encontró la tarea");
					}

					//verificando el numero de concurrencia
					if(tarea.NumeroVerificador != verificador)
					{
						var resp = new
						{
							success = false,
							msg = "No se ha podido realizar la operacion porque la tarea ha sido actualizada antes por otro usuario, por favor intente nuevamente la actualización"
						};
						return Json(resp);
					}

					tarea.EsReproceso = esReproceso == true ? 1 : 0;

                    //Verificando si han cambiado o no al colaborador o la fecha
                    if (tarea.SecuencialColaborador != idTrabajador || diaTarea != tarea.FechaInicio.Date)//Hay un cambio en la asignacion de la tarea
					{
						DateTime diaAnteriorTarea = tarea.FechaInicio.Date;
						DateTime diaSiguienteTarea = diaAnteriorTarea.AddDays(1);
						DateTime fechaInicio = diaAnteriorTarea.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
						DateTime fechaInicioExtra = diaAnteriorTarea.AddMinutes(30 + (1 * 60));//A las 8.30 empieza
																							   //Las tareas del antiguo colaborador o el mismo, de el dia antiguo o el mismo
						if(tarea.FechaInicio >= fechaInicio)
						{
							List<Tarea> tareas = (from t in db.Tarea
												  where t.FechaInicio >= fechaInicio &&
														t.FechaInicio < diaSiguienteTarea &&
														t.SecuencialColaborador == tarea.SecuencialColaborador &&
														t.Secuencial != tarea.Secuencial &&
														t.SecuencialEstadoTarea != 4
												  orderby t.FechaInicio
												  select t).ToList<Tarea>();//Las tareas del mismo día pero que son distintas

							//Cambiando las horas de inicio y de fin de estas tareas
							foreach(Tarea tareaAnt in tareas)
							{
								TimeSpan tiempo = tareaAnt.FechaFin - tareaAnt.FechaInicio;
								tareaAnt.FechaInicio = fechaInicio;
								tareaAnt.FechaFin = fechaInicio.Add(tiempo);
								if(tareaAnt.FechaInicio.Hour < 13 && tareaAnt.FechaFin.Hour > 13)
								{
									tareaAnt.FechaFin = tareaAnt.FechaFin.AddHours(1);
								}
								else if(tareaAnt.FechaInicio.Hour == 13)
								{
									tareaAnt.FechaInicio = tareaAnt.FechaInicio.AddHours(1);
									tareaAnt.FechaFin = tareaAnt.FechaFin.AddHours(1);
								}

								fechaInicio = tareaAnt.FechaFin;
							};
						}
						else
						{
							List<Tarea> tareas = (from t in db.Tarea
												  where t.FechaInicio >= fechaInicioExtra &&
														t.FechaInicio < fechaInicio &&
														t.SecuencialColaborador == tarea.SecuencialColaborador &&
														t.Secuencial != tarea.Secuencial &&
														t.SecuencialEstadoTarea != 4
												  orderby t.FechaInicio
												  select t).ToList<Tarea>();//Las tareas del mismo día pero que son distintas

							//Cambiando las horas de inicio y de fin de estas tareas
							foreach(Tarea tareaAnt in tareas)
							{
								TimeSpan tiempo = tareaAnt.FechaFin - tareaAnt.FechaInicio;
								tareaAnt.FechaInicio = fechaInicioExtra;
								tareaAnt.FechaFin = fechaInicioExtra.Add(tiempo);

								fechaInicioExtra = tareaAnt.FechaFin;
							};
						}

						//Para la actualizacion de las tareas y de los permisos
						listaDiaColaborador.Add(new DiaColaborador { Fecha = diaAnteriorTarea, IdColaborador = tarea.SecuencialColaborador });
						listaCambiosTareas.Add(new DiaColaborador { Fecha = diaAnteriorTarea, IdColaborador = tarea.SecuencialColaborador });

						//Moviendo al nuevo colaborador o al mismo
						tarea.SecuencialColaborador = idTrabajador;
						//Buscando las tareas del colaborador en la fecha nueva

						//Buscando la suma de las horas de las tareas del mismo día
						DateTime fechaInicioTareaDia = diaTarea.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
						DateTime fechaInicioTareaDiaExtra = diaTarea.AddMinutes(30 + (1 * 60));//A las 8.30 empieza

						if(tarea.FechaInicio >= fechaInicio)
						{
							var tareasDia = (from t in db.Tarea
											 where t.SecuencialColaborador == idTrabajador &&
												   t.FechaInicio >= fechaInicioTareaDia && t.FechaInicio < diaSiguiente &&
												   t.SecuencialEstadoTarea != 4
											 select new
											 {
												 finicio = t.FechaInicio,
												 ffin = t.FechaFin
											 }).ToList();
							int time = 0;
							int timeMinutos = 0;
							foreach(var tareaD in tareasDia)
							{
								TimeSpan tiempo = tareaD.ffin - tareaD.finicio;
								time += tiempo.Hours;
								timeMinutos += tiempo.Minutes;
							}

							DateTime fInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
							fInicio = fInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza

							//Calculando las horas de la tarea
							int horasTar = (tarea.FechaFin - tarea.FechaInicio).Hours;
							if(tarea.FechaInicio.Hour < 13 && tarea.FechaFin.Hour > 13)
							{
								horasTar--;
							}
							else if(tarea.FechaInicio.Hour == 13)
							{
								horasTar--;
							}

							tarea.FechaInicio = fInicio;
							tarea.FechaFin = fInicio.AddHours(horas).AddMinutes(minutos);

							if(tarea.FechaInicio.Hour < 13 && tarea.FechaFin.Hour > 13)
							{
								tarea.FechaFin = tarea.FechaFin.AddHours(1);
							}
							else if(tarea.FechaInicio.Hour == 13)
							{
								tarea.FechaInicio = tarea.FechaInicio.AddHours(1);
								tarea.FechaFin = tarea.FechaFin.AddHours(1);
							}
						}
						else
						{
							var tareasDia = (from t in db.Tarea
											 where t.SecuencialColaborador == idTrabajador &&
												   t.FechaInicio >= fechaInicioTareaDiaExtra && t.FechaInicio < fechaInicioTareaDia &&
												   t.SecuencialEstadoTarea != 4
											 select new
											 {
												 finicio = t.FechaInicio,
												 ffin = t.FechaFin
											 }).ToList();
							int time = 0;
							int timeMinutos = 0;
							foreach(var tareaD in tareasDia)
							{
								TimeSpan tiempo = tareaD.ffin - tareaD.finicio;
								time += tiempo.Hours;
								timeMinutos += tiempo.Minutes;
							}

							DateTime fInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
							fInicio = fInicio.AddMinutes(30 + (1 * 60));//A las 1.30 empieza

							//Calculando las horas de la tarea
							int horasTar = (tarea.FechaFin - tarea.FechaInicio).Hours;

							tarea.FechaInicio = fInicio;
							tarea.FechaFin = fInicio.AddHours(horas).AddMinutes(minutos);
						}

						//Actualizando los otros datos
						verificador++;
						tarea.SecuencialActividad = actividad;
						tarea.SecuencialModulo = modulo;
						tarea.SecuencialCliente = cliente;
						tarea.SecuencialLugarTarea = ubicacion;
						tarea.Detalle = detalle.ToUpper();
						tarea.NumeroVerificador = verificador;
						tarea.TiempoEstimacion = new TimeSpan(horasEstimadas, minutosEstimados, 0);
						
						if(referencia != 0)//Actualizar la referencia
						{
							if((tarea.entregableMotivoTrabajo != null && tarea.entregableMotivoTrabajo.Secuencial != referencia) || tarea.entregableMotivoTrabajo == null)
							{
								EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
								if(entregable != null)
								{
									tarea.entregableMotivoTrabajo = entregable;
								}
							}
						}
						else//Buscando si tiene referencias y quitandolas
						{
							if(tarea.entregableMotivoTrabajo != null)
							{
								tarea.entregableMotivoTrabajo = null;
							}
						}

						//Para la actualizacion de las tareas y de los permisos
						listaDiaColaborador.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tarea.SecuencialColaborador });
						listaCambiosTareas.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tarea.SecuencialColaborador });
					}
					else
					{//Mismo trabajador y misma fecha
						DateTime finicio = tarea.FechaInicio;
						DateTime ffin = tarea.FechaInicio.AddHours(horas).AddMinutes(minutos);

						if(finicio.Hour < 13 && ffin.Hour > 13)
						{
							ffin = ffin.AddHours(1);
						}

						verificador++;
						tarea.SecuencialActividad = actividad;
						tarea.SecuencialModulo = modulo;
						tarea.SecuencialCliente = cliente;
						tarea.SecuencialLugarTarea = ubicacion;
						tarea.Detalle = detalle.ToUpper();
						tarea.FechaFin = ffin;
						tarea.NumeroVerificador = verificador;
						tarea.TiempoEstimacion = new TimeSpan(horasEstimadas, minutosEstimados, 0);


						if(referencia != 0)//Actualizar la referencia
						{
							if((tarea.entregableMotivoTrabajo != null && tarea.entregableMotivoTrabajo.Secuencial != referencia) || tarea.entregableMotivoTrabajo == null)
							{
								EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
								if(entregable != null)
								{
									tarea.entregableMotivoTrabajo = entregable;
								}
							}
						}
						else//Buscando si tiene referencias y quitandolas
						{
							if(tarea.entregableMotivoTrabajo != null)
							{
								tarea.entregableMotivoTrabajo = null;
							}
						}

						//Buscando las tareas que siguen y actualizarlas....por la hora de inicio.
						DateTime fInicioTareas = diaTarea.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
						DateTime fInicioTareasExtra = diaTarea.AddMinutes(30 + (1 * 60));//A las 1.30 empieza

						if(tarea.FechaInicio >= fInicioTareas)
						{
							var tareasQueSiguen = (from t in db.Tarea
												   where t.SecuencialColaborador == idTrabajador &&
														   t.FechaInicio >= diaTarea && t.FechaInicio < diaSiguiente &&
														   t.FechaInicio > tarea.FechaInicio && t.SecuencialEstadoTarea != 4
												   select t).ToList();

							Tarea tAnterior = tarea;
							for(int i = 0; i < tareasQueSiguen.Count(); i++)
							{
								Tarea tActual = tareasQueSiguen[i];
								TimeSpan tiempoTarea = tActual.FechaFin - tActual.FechaInicio;
								int horasTarea = tiempoTarea.Hours;
								int minutosTarea = tiempoTarea.Minutes;

								if(tActual.FechaInicio.Hour < 13 && tActual.FechaFin.Hour > 13)
								{
									horasTarea--;
								}

								tActual.FechaInicio = tAnterior.FechaFin;
								tActual.FechaFin = tActual.FechaInicio.AddHours(horasTarea).AddMinutes(minutosTarea);

								if(tActual.FechaInicio.Hour < 13 && tActual.FechaFin.Hour > 13)
								{
									tActual.FechaFin = tActual.FechaFin.AddHours(1);
								}
								else if(tActual.FechaInicio.Hour == 13)
								{
									tActual.FechaInicio = tActual.FechaInicio.AddHours(1);
									tActual.FechaFin = tActual.FechaFin.AddHours(1);
								}

								tAnterior = tActual;
							}
						}
						else
						{
							var tareasQueSiguen = (from t in db.Tarea
												   where t.SecuencialColaborador == idTrabajador &&
														   t.FechaInicio >= fInicioTareasExtra && t.FechaInicio < fInicioTareas &&
														   t.FechaInicio > tarea.FechaInicio && t.SecuencialEstadoTarea != 4
												   select t).ToList();

							Tarea tAnterior = tarea;
							for(int i = 0; i < tareasQueSiguen.Count(); i++)
							{
								Tarea tActual = tareasQueSiguen[i];
								TimeSpan tiempoTarea = tActual.FechaFin - tActual.FechaInicio;

								tActual.FechaInicio = tAnterior.FechaFin;
								tActual.FechaFin = tActual.FechaInicio.Add(tiempoTarea);

								tAnterior = tActual;
							}
						}

						//Para la actualizacion de las tareas y de los permisos
						listaDiaColaborador.Add(new DiaColaborador { Fecha = tarea.FechaInicio.Date, IdColaborador = tarea.SecuencialColaborador });
						listaCambiosTareas.Add(new DiaColaborador { Fecha = tarea.FechaInicio.Date, IdColaborador = tarea.SecuencialColaborador });
					}

					//Buscando si la tarea tiene coordinador
					var tareaCoordinador = (from tc in db.Tarea_Coordinador
											where tc.SecuencialTarea == idTarea
											select tc).FirstOrDefault();
					if(tareaCoordinador != null)//Tiene un coordinador
					{
						if(coordinador != 0)
						{
							tareaCoordinador.EstaActivo = 1;
							if(coordinador != tareaCoordinador.SecuencialColaborador)//Actualizarlo
							{
								tareaCoordinador.SecuencialColaborador = coordinador;
							}
						}
						else//Quitar el coordinador
						{
							tareaCoordinador.EstaActivo = 0;
						}
					}
					else//No tiene un coordinador
					{
						if(coordinador != 0)//Entrar un nuevo coordinador
						{
							db.Tarea_Coordinador.Add(
								new Tarea_Coordinador
								{
									tarea = tarea,
									SecuencialColaborador = coordinador,
									EstaActivo = 1,
									NumeroVerificador = 1
								}
							);
						}
					}
					if(ticketTarea != 0)
					{
						Ticket t = db.Ticket.Where(s => s.Secuencial == ticketTarea).FirstOrDefault();
						if(t != null)
						{
							TicketTarea tt = db.TicketTarea.Where(s => s.SecuencialTarea == tarea.Secuencial && s.EstaActiva == 1).FirstOrDefault();
							if(tt != null)
							{
								tt.SecuencialTicket = ticketTarea;
							}
							else
							{
								db.TicketTarea.Add(new TicketTarea
								{
									SecuencialTarea = tarea.Secuencial,
									SecuencialTicket = ticketTarea,
									EstaActiva = 1
								});
							}
						}
						else
						{
							throw new Exception("No se encontró el ticket");
						}
					}

					tareaPrincipal = tarea;
				}
				else
				{
					//Buscando la suma de las horas de las tareas del mismo día
					DateTime fechaInicioTareas = diaTarea.AddMinutes(30 + (8 * 60));
					DateTime fechaInicioTareasExtraordinarias = diaTarea.AddMinutes(30 + (1 * 60));

					Tarea tar = null;
					if(!extraordinaria)
					{
						var tareasDelDia = (from t in db.Tarea
											where
												t.SecuencialColaborador == idTrabajador &&
												t.FechaInicio >= fechaInicioTareas &&
												t.FechaInicio < diaSiguiente &&
												t.SecuencialEstadoTarea != 4
											select new
											{
												finicio = t.FechaInicio,
												ffin = t.FechaFin
											}).ToList();

						int time = 0;
						int timeMinutos = 0;
						foreach(var tarea in tareasDelDia)
						{
							TimeSpan tiempo = tarea.ffin - tarea.finicio;
							time += tiempo.Hours;
							timeMinutos += tiempo.Minutes;
						}

						DateTime fechaInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
						fechaInicio = fechaInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
						DateTime fechaFin = fechaInicio.AddHours(horas).AddMinutes(minutos);

						if(fechaInicio.Hour < 13 && fechaFin.Hour > 13)
						{
							fechaFin = fechaFin.AddHours(1);
						}
						else if(fechaInicio.Hour == 13)
						{
							fechaInicio = fechaInicio.AddHours(1);
							fechaFin = fechaFin.AddHours(1);
						}

						if(fechaInicio < diaSiguiente)
						{
							tar = new Tarea
							{
								SecuencialColaborador = idTrabajador,
								SecuencialActividad = actividad,
								SecuencialModulo = modulo,
								SecuencialCliente = cliente,
								SecuencialEstadoTarea = 1,
								SecuencialLugarTarea = ubicacion,
								Detalle = detalle.ToUpper(),
								FechaInicio = fechaInicio,
								FechaFin = fechaFin,
								HorasUtilizadas = 0,
								NumeroVerificador = 1,
								TiempoEstimacion = new TimeSpan(horasEstimadas, minutosEstimados, 0),
								EsReproceso = esReproceso == true ? 1 : 0
							};
						}
						else
						{
							var esRepro = esReproceso == true ? 1 : 0;
                            return NuevaTarea(idTrabajador, diaSiguiente.ToString("dd/MM/yyyy"), cliente, ubicacion, modulo, actividad, horas, minutos, horasEstimadas, minutosEstimados, detalle, referencia, coordinador, repetir, finSemana, repetirTipoFin, numVeces, fechaHasta, extraordinaria, idTarea, verificador, esRepro);
						}
					}
					else
					{
						var tareasExtraordinariasDelDia = (from t in db.Tarea
														   where
															   t.SecuencialColaborador == idTrabajador &&
															   t.FechaInicio >= fechaInicioTareasExtraordinarias &&
															   t.FechaInicio < fechaInicioTareas &&
															   t.SecuencialEstadoTarea != 4
														   select new
														   {
															   finicio = t.FechaInicio,
															   ffin = t.FechaFin
														   }).ToList();
						int timeExtra = 0;
						int timeExtraMinutos = 0;
						foreach(var tarea in tareasExtraordinariasDelDia)
						{
							TimeSpan tiempo = tarea.ffin - tarea.finicio;
							timeExtra += tiempo.Hours;
							timeExtraMinutos += tiempo.Minutes;
						}

						DateTime fechaInicioExtra = fechaInicioTareasExtraordinarias.AddHours(timeExtra).AddMinutes(timeExtraMinutos);
						DateTime fechaFinExtra = fechaInicioExtra.AddHours(horas).AddMinutes(minutos);

						if(fechaFinExtra > fechaInicioTareas)
						{
							throw new Exception("Las tareas extraordinarias no pueden pasarse de las 8:30 AM");
						}

						tar = new Tarea
						{
							SecuencialColaborador = idTrabajador,
							SecuencialActividad = actividad,
							SecuencialModulo = modulo,
							SecuencialCliente = cliente,
							SecuencialEstadoTarea = 1,
							SecuencialLugarTarea = ubicacion,
							Detalle = detalle.ToUpper(),
							FechaInicio = fechaInicioExtra,
							FechaFin = fechaFinExtra,
							HorasUtilizadas = 0,
							NumeroVerificador = 1,
							TiempoEstimacion = new TimeSpan(horasEstimadas, minutosEstimados, 0),
                            EsReproceso = esReproceso == true ? 1 : 0
                        };
					}

					if(referencia != 0)//Actualizar la referencia
					{
						if((tar.entregableMotivoTrabajo != null && tar.entregableMotivoTrabajo.Secuencial != referencia) || tar.entregableMotivoTrabajo == null)
						{
							EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
							if(entregable != null)
							{
								tar.entregableMotivoTrabajo = entregable;
							}
						}
					}

					db.Tarea.Add(tar);
					//Para la actualizacion de las tareas y de los permisos
					listaDiaColaborador.Add(new DiaColaborador { Fecha = tar.FechaInicio.Date, IdColaborador = tar.SecuencialColaborador });
					listaCambiosTareas.Add(new DiaColaborador { Fecha = tar.FechaInicio.Date, IdColaborador = tar.SecuencialColaborador });

					if(coordinador != 0)
					{
						db.Tarea_Coordinador.Add(
							new Tarea_Coordinador
							{
								tarea = tar,
								SecuencialColaborador = coordinador,
								EstaActivo = 1,
								NumeroVerificador = 1
							}
						);
					}

					HistoricoTareaEstado histET = new HistoricoTareaEstado
					{
						tarea = tar,
						SecuencialEstadoTarea = 1,
						FechaOperacion = DateTime.Now,
						usuario = user
					};

					db.HistoricoTareaEstado.Add(histET);

					if(ticketTarea != 0)
					{
						Ticket t = db.Ticket.Where(s => s.Secuencial == ticketTarea).FirstOrDefault();
						if(t != null)
						{
							db.TicketTarea.Add(new TicketTarea
							{
								SecuencialTarea = tar.Secuencial,
								SecuencialTicket = ticketTarea,
								EstaActiva = 1
							});
						}
						else
						{
							throw new Exception("No se encontró el ticket");
						}
					}

					tareaPrincipal = tar;
				}

				//Ver si se repite la tarea
				if(repetir != "" && extraordinaria == false)//Aqui se repite la tarea
				{
					DateTime diaT = diaTarea;

					if(repetirTipoFin == 1)
					{//Veces
						int i = 1;
						while(i < numVeces)
						{

							if(repetir == "SEMANAL")
							{
								diaT = diaT.AddDays(7);
							}
							else if(repetir == "MENSUAL")
							{
								diaT = diaT.AddMonths(1);
							}
							else
							{//Diario
								diaT = diaT.AddDays(1);

								DayOfWeek diaSemana = diaT.DayOfWeek;
								long diaSem = (int)diaSemana;
								if(finSemana == 0)//No se contemplan los fines de semana
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(2);
									}
									else if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 1)//Se incluye el sábado no el domingo
								{
									if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 2)//Se incluye el domingo no el sábado
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(1);
									}
								}
							}

							DateTime diaT2 = diaT.AddDays(1);

							//Buscando la suma de las horas de las tareas del mismo día
							var tareasDia = (from t in db.Tarea
											 where t.SecuencialColaborador == idTrabajador &&
												   t.FechaInicio >= diaT && t.FechaInicio < diaT2 &&
												   t.SecuencialEstadoTarea != 4
											 select new
											 {
												 finicio = t.FechaInicio,
												 ffin = t.FechaFin
											 }).ToList();
							int time = 0;
							int timeMinutos = 0;
							foreach(var tarea in tareasDia)
							{
								TimeSpan tiempo = tarea.ffin - tarea.finicio;
								time += tiempo.Hours;
								timeMinutos += tiempo.Minutes;
							}

							DateTime fInicio = diaT.AddHours(time).AddMinutes(timeMinutos);
							fInicio = fInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
							DateTime fFin = fInicio.AddHours(horas).AddMinutes(minutos);

							if(fInicio.Hour < 13 && fFin.Hour > 13)
							{
								fFin = fFin.AddHours(1);
							}
							else if(fInicio.Hour == 13)
							{
								fInicio = fInicio.AddHours(1);
								fFin = fFin.AddHours(1);
							}

							Tarea tar1 = new Tarea
							{
								SecuencialColaborador = idTrabajador,
								SecuencialActividad = actividad,
								SecuencialModulo = modulo,
								SecuencialCliente = cliente,
								SecuencialEstadoTarea = tareaPrincipal.SecuencialEstadoTarea,
								SecuencialLugarTarea = ubicacion,
								Detalle = detalle.ToUpper(),
								FechaInicio = fInicio,
								FechaFin = fFin,
								HorasUtilizadas = 0,
								NumeroVerificador = 1,
								TiempoEstimacion = new TimeSpan(horasEstimadas, minutosEstimados, 0)
							};

							if(referencia != 0)//Actualizar la referencia
							{
								if((tar1.entregableMotivoTrabajo != null && tar1.entregableMotivoTrabajo.Secuencial != referencia) || tar1.entregableMotivoTrabajo == null)
								{
									EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
									if(entregable != null)
									{
										tar1.entregableMotivoTrabajo = entregable;
									}
								}
							}

							db.Tarea.Add(tar1);
							//Para la actualizacion de las tareas y de los permisos
							listaDiaColaborador.Add(new DiaColaborador { Fecha = tar1.FechaInicio.Date, IdColaborador = tar1.SecuencialColaborador });
							listaCambiosTareas.Add(new DiaColaborador { Fecha = tar1.FechaInicio.Date, IdColaborador = tar1.SecuencialColaborador });

							if(coordinador != 0)
							{
								db.Tarea_Coordinador.Add(
									new Tarea_Coordinador
									{
										tarea = tar1,
										SecuencialColaborador = coordinador,
										EstaActivo = 1,
										NumeroVerificador = 1
									}
								);
							}

							HistoricoTareaEstado histET1 = new HistoricoTareaEstado
							{
								tarea = tar1,
								SecuencialEstadoTarea = tareaPrincipal.SecuencialEstadoTarea,
								FechaOperacion = DateTime.Now,
								usuario = user
							};

							db.HistoricoTareaEstado.Add(histET1);

							Tarea_TareaRelacionada tareaTReacionada = new Tarea_TareaRelacionada
							{
								tarea1 = tar1,
								tarea = tareaPrincipal,
								NumeroVerificador = 1
							};
							db.Tarea_TareaRelacionada.Add(tareaTReacionada);

							i++;
						}
					}
					else if(repetirTipoFin == 2)
					{//Fecha

						string[] fechasHasta = fechaHasta.Split(new Char[] { '/' });
						int diaHasta = Int32.Parse(fechasHasta[0]);
						int mesHasta = Int32.Parse(fechasHasta[1]);
						int annoHasta = Int32.Parse(fechasHasta[2]);
						DateTime diaTareaHasta = new System.DateTime(annoHasta, mesHasta, diaHasta);

						while(diaTareaHasta > diaT)
						{
							if(repetir == "SEMANAL")
							{
								diaT = diaT.AddDays(7);
							}
							else if(repetir == "MENSUAL")
							{
								diaT = diaT.AddMonths(1);
							}
							else
							{//Diario
								diaT = diaT.AddDays(1);
								DayOfWeek diaSemana = diaT.DayOfWeek;
								long diaSem = (int)diaSemana;
								if(finSemana == 0)//No se contemplan los fines de semana
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(2);
									}
									else if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 1)//Se incluye el sábado no el domingo
								{
									if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 2)//Se incluye el domingo no el sábado
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(1);
									}
								}
							}
							if(diaTareaHasta < diaT)//Romper por si se pasa
								break;

							DateTime diaT2 = diaT.AddDays(1);

							//Buscando la suma de las horas de las tareas del mismo día
							var tareasDia = (from t in db.Tarea
											 where t.SecuencialColaborador == idTrabajador &&
												   t.FechaInicio >= diaT && t.FechaInicio < diaT2 &&
												   t.SecuencialEstadoTarea != 4
											 select new
											 {
												 finicio = t.FechaInicio,
												 ffin = t.FechaFin
											 }).ToList();
							int time = 0;
							int timeMinutos = 0;
							foreach(var tarea in tareasDia)
							{
								TimeSpan tiempo = tarea.ffin - tarea.finicio;
								time += tiempo.Hours;
								timeMinutos += tiempo.Minutes;
							}

							DateTime fInicio = diaT.AddHours(time).AddMinutes(timeMinutos);
							fInicio = fInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
							DateTime fFin = fInicio.AddHours(horas).AddMinutes(minutos);

							if(fInicio.Hour < 13 && fFin.Hour > 13)
							{
								fFin = fFin.AddHours(1);
							}
							else if(fInicio.Hour == 13)
							{
								fInicio = fInicio.AddHours(1);
								fFin = fFin.AddHours(1);
							}

							Tarea tar1 = new Tarea
							{
								SecuencialColaborador = idTrabajador,
								SecuencialActividad = actividad,
								SecuencialModulo = modulo,
								SecuencialCliente = cliente,
								SecuencialEstadoTarea = tareaPrincipal.SecuencialEstadoTarea,
								SecuencialLugarTarea = ubicacion,
								Detalle = detalle.ToUpper(),
								FechaInicio = fInicio,
								FechaFin = fFin,
								HorasUtilizadas = 0,
								NumeroVerificador = 1,
								TiempoEstimacion = new TimeSpan(horasEstimadas, minutosEstimados, 0)
							};

							if(referencia != 0)//Actualizar la referencia
							{
								if((tar1.entregableMotivoTrabajo != null && tar1.entregableMotivoTrabajo.Secuencial != referencia) || tar1.entregableMotivoTrabajo == null)
								{
									EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
									if(entregable != null)
									{
										tar1.entregableMotivoTrabajo = entregable;
									}
								}
							}

							db.Tarea.Add(tar1);
							//Para la actualizacion de las tareas y de los permisos
							listaDiaColaborador.Add(new DiaColaborador { Fecha = tar1.FechaInicio.Date, IdColaborador = tar1.SecuencialColaborador });
							listaCambiosTareas.Add(new DiaColaborador { Fecha = tar1.FechaInicio.Date, IdColaborador = tar1.SecuencialColaborador });

							if(coordinador != 0)
							{
								db.Tarea_Coordinador.Add(
									new Tarea_Coordinador
									{
										tarea = tar1,
										SecuencialColaborador = coordinador,
										EstaActivo = 1,
										NumeroVerificador = 1
									}
								);
							}

							HistoricoTareaEstado histET1 = new HistoricoTareaEstado
							{
								tarea = tar1,
								SecuencialEstadoTarea = tareaPrincipal.SecuencialEstadoTarea,
								FechaOperacion = DateTime.Now,
								usuario = user
							};

							db.HistoricoTareaEstado.Add(histET1);

							Tarea_TareaRelacionada tareaTReacionada = new Tarea_TareaRelacionada
							{
								tarea1 = tar1,
								tarea = tareaPrincipal,
								NumeroVerificador = 1
							};
							db.Tarea_TareaRelacionada.Add(tareaTReacionada);
						}
					}
				}

				string msg = "Se ha creado correctamente la tarea";
				if(repetir != "")
				{
					if(idTarea != 0)
					{
						msg = "Se han actualizado correctamente las tareas.";
					}
					else
					{
						msg = "Se han creado correctamente las tareas.";
					}
				}
				else
				{
					if(idTarea != 0)
					{
						msg = "Se ha actualizado correctamente la tarea.";
					}
				}

				db.SaveChanges();//Salvando los cambios   

				foreach(DiaColaborador diaColab in listaDiaColaborador)
				{
					Utiles.OrdenarTareasPermisos(diaColab.Fecha, diaColab.IdColaborador, user, db);
				}
				//Actualizando las fechas de los dias que existieron los cambios y en las fechas

				//Actualizando cambios en la interfaz de usuario
				ActualizarTDTarea(listaCambiosTareas);

				//try
				//{
				//    List<string> correosDestinos = new List<string>();
				//    correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("TFS"));
				//    correosDestinos.Add(emailUser);
				//    correosDestinos.Add(colaboradorTarea.persona.usuario.FirstOrDefault().Email);

				//    string textoEmail = @"<div class='textoCuerpo'><br/>";
				//    textoEmail += "Buen día,";
				//    textoEmail += @"<br/>";

				//    textoEmail += "Con el presente correo se solicita acceso a las fuentes del cliente: " + clienteTarea.Descripcion + ", al colaborador: " + colaboradorTarea.persona.Nombre1 + " " + colaboradorTarea.persona.Apellido1;
				//    textoEmail += "</div>";


				//    string asuntoEmail = "Solicitud Acceso";
				//    Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail);

				//}
				//catch (Exception e)
				//{
				//    throw new Exception(e.Message);
				//}


				var respOk = new
				{
					success = true,
					msg = msg
				};
				return Json(respOk);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message
					//msg = "Hay errores en los datos por favor verifíquelos"
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public JsonResult EmailNuevaTarea(int idTrabajador, string fecha, int cliente, int ubicacion, int modulo, int actividad, int horas, int minutos, string detalle, int referencia = 0, int coordinador = 0, string repetir = "", int finSemana = 0, int repetirTipoFin = 0, int numVeces = 0, string fechaHasta = "", bool extraordinaria = false, int ticketTarea = 0, int idTarea = 0, int verificador = 0)
		{
			try
			{
				if(referencia != 0)
				{
					string emailUser = User.Identity.Name;
					Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

					var colaboradorTarea = db.Colaborador.Find(idTrabajador);
					var clienteTarea = db.Cliente.Find(cliente);

					List<string> correosDestinos = new List<string>();
					correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("TFS"));
					correosDestinos.Add(emailUser);
					correosDestinos.Add(colaboradorTarea.persona.usuario.FirstOrDefault().Email);

					string textoEmail = @"<div class='textoCuerpo'><br/>";
					textoEmail += "Estimados,";
					textoEmail += @"<br/>";

					textoEmail += "Por favor su ayuda con el acceso a las fuentes de: " + clienteTarea.Descripcion + ", al colaborador: " + colaboradorTarea.persona.Nombre1 + " " + colaboradorTarea.persona.Apellido1;
					textoEmail += "</div>";


					string asuntoEmail = "Solicitud Acceso";
					Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail);
				}

				var respOk = new
				{
					success = true,
					msg = "EMAIL ENVIADO"
				};
				return Json(respOk);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message
					//msg = "Hay errores en los datos por favor verifíquelos"
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult AnularTarea(int idTarea, int tipoAnulacion = 0)
		{
			try
			{
				//para la actualizacion de los permisos y las tareas
				List<DiaColaborador> listaDiaColaborador = new List<DiaColaborador>();
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();

				var tarea = (from t in db.Tarea
							 where t.Secuencial == idTarea
							 select t).FirstOrDefault();

				//Buscando si pertene a un ticket, no puede anular la tarea
				TicketTarea ticketTarea = tarea.ticketTarea.FirstOrDefault();
				if(ticketTarea != null)//Aqui la tarea está asociada a un ticket
				{
					throw new Exception("La tarea está asociada a un ticket, no se puede anular");
				}

				if(tipoAnulacion == 0)//Anular normalmente
				{
					//Buscando si pertenece a una secuencia, de tareas.
					int cant = db.Tarea_TareaRelacionada.Where(x => x.SecuencialTarea == tarea.Secuencial).Count();//tarea es tarea principal
					if(cant == 0)
					{
						cant = db.Tarea_TareaRelacionada.Where(x => x.SecuencialTareaRelacionada == tarea.Secuencial).Count();//tarea es tarea repetida
					}

					if(cant > 0)
					{
						var respSerie = new
						{
							success = true,
							tareaSeriada = true,
							msg = "Esta tarea pertenece a una serie de tareas. Usted puede anular todas las tareas de la serie o solo una. ¿Que desea hacer?"
						};
						return Json(respSerie);
					}

				}

				tarea.SecuencialEstadoTarea = 4;//Anulada
				tarea.SecuencialColaborador = db.Colaborador.Where(s => s.persona.usuario.FirstOrDefault().Email == "canulado@sifizsoft.com").FirstOrDefault().Secuencial;

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				HistoricoTareaEstado histET = new HistoricoTareaEstado
				{
					tarea = tarea,
					SecuencialEstadoTarea = 4,
					FechaOperacion = DateTime.Now,
					usuario = user
				};
				db.HistoricoTareaEstado.Add(histET);

				//Actualizando la hora en las tareas que siguen
				//Buscando las tareas que siguen y actualizarlas....por la hora de inicio.
				DateTime diaTarea = tarea.FechaInicio.Date;
				DateTime diaSiguiente = diaTarea.AddDays(1);
				DateTime fechaInicioTareas = diaTarea.AddMinutes(30 + (8 * 60));

				if(tarea.FechaInicio >= fechaInicioTareas)
				{
					var tareasQueSiguen = (from t in db.Tarea
										   where t.SecuencialColaborador == tarea.SecuencialColaborador &&
												 t.FechaInicio >= diaTarea && t.FechaInicio < diaSiguiente &&
												 t.FechaInicio > tarea.FechaInicio &&
												 t.SecuencialEstadoTarea != 4
										   orderby t.FechaInicio ascending
										   select t).ToList();

					Tarea tAnterior = tarea;
					for(int i = 0; i < tareasQueSiguen.Count(); i++)
					{
						Tarea tActual = tareasQueSiguen[i];
						TimeSpan tiempoTarea = tActual.FechaFin - tActual.FechaInicio;
						int horasTarea = tiempoTarea.Hours;
						int minutosTarea = tiempoTarea.Minutes;

						if(tActual.FechaInicio.Hour < 13 && tActual.FechaFin.Hour > 13)
						{
							horasTarea--;
						}

						tActual.FechaInicio = tAnterior.FechaInicio;
						tActual.FechaFin = tActual.FechaInicio.AddHours(horasTarea).AddMinutes(minutosTarea);

						if(tActual.FechaInicio.Hour < 13 && tActual.FechaFin.Hour > 13)
						{
							tActual.FechaFin = tActual.FechaFin.AddHours(1);
						}
						else if(tActual.FechaInicio.Hour == 13)
						{
							tActual.FechaInicio = tActual.FechaInicio.AddHours(1);
							tActual.FechaFin = tActual.FechaFin.AddHours(1);
						}

						tAnterior = tActual;
					}

				}
				else
				{
					var tareasQueSiguen = (from t in db.Tarea
										   where t.SecuencialColaborador == tarea.SecuencialColaborador &&
												 t.FechaInicio >= diaTarea && t.FechaInicio < fechaInicioTareas &&
												 t.FechaInicio > tarea.FechaInicio &&
												 t.SecuencialEstadoTarea != 4
										   orderby t.FechaInicio ascending
										   select t).ToList();

					Tarea tAnterior = tarea;
					for(int i = 0; i < tareasQueSiguen.Count(); i++)
					{
						Tarea tActual = tareasQueSiguen[i];
						TimeSpan tiempoTarea = tActual.FechaFin - tActual.FechaInicio;
						int horasTarea = tiempoTarea.Hours;
						int minutosTarea = tiempoTarea.Minutes;

						if(tActual.FechaInicio.Hour < 13 && tActual.FechaFin.Hour > 13)
						{
							horasTarea--;
						}

						tActual.FechaInicio = tAnterior.FechaInicio;
						tActual.FechaFin = tActual.FechaInicio.AddHours(horasTarea).AddMinutes(minutosTarea);

						if(tActual.FechaInicio.Hour < 13 && tActual.FechaFin.Hour > 13)
						{
							tActual.FechaFin = tActual.FechaFin.AddHours(1);
						}
						else if(tActual.FechaInicio.Hour == 13)
						{
							tActual.FechaInicio = tActual.FechaInicio.AddHours(1);
							tActual.FechaFin = tActual.FechaFin.AddHours(1);
						}

						tAnterior = tActual;
					}
				}

				//para la actualizacion de los permisos y las tareas
				listaDiaColaborador.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tarea.SecuencialColaborador });
				listaCambiosTareas.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tarea.SecuencialColaborador });

				if(tipoAnulacion == 2)//Anular a todas las tareas de la secuencia
				{
					//Buscando la secuencia de tareas
					List<Tarea> tareas = new List<Tarea>();
					//Buscando si pertenece a una secuencia, de tareas.
					int cant = db.Tarea_TareaRelacionada.Where(x => x.SecuencialTarea == tarea.Secuencial).Count(); //tarea es tarea principal
					if(cant > 0)
					{
						tareas = (from t in db.Tarea
								  join
									  tr in db.Tarea_TareaRelacionada on t equals tr.tarea1
								  where tr.SecuencialTarea == tarea.Secuencial && t.SecuencialEstadoTarea != 4
								  select t).ToList();//tengo todas las tareas relacionadas
					}
					else
					{
						var tareaRelacion = (from tr in db.Tarea_TareaRelacionada
											 where tr.SecuencialTareaRelacionada == tarea.Secuencial
											 select tr).FirstOrDefault();

						tareas = (from t in db.Tarea
								  join tr in db.Tarea_TareaRelacionada on t equals tr.tarea1
								  where tr.SecuencialTarea == tareaRelacion.SecuencialTarea &&
										tr.SecuencialTareaRelacionada != tareaRelacion.SecuencialTareaRelacionada &&
										t.SecuencialEstadoTarea != 4
								  select t).ToList();
						//Anadiendo la tarea principal
						Tarea tareaPrincipal = db.Tarea.Where(x => x.Secuencial == tareaRelacion.SecuencialTarea && x.SecuencialEstadoTarea != 4).FirstOrDefault();
						if(tareaPrincipal != null)
							tareas.Add(tareaPrincipal);
					}

					foreach(Tarea tar in tareas)
					{
						tar.SecuencialEstadoTarea = 4;//Anulada
						tarea.SecuencialColaborador = db.Colaborador.Where(s => s.persona.usuario.FirstOrDefault().Email == "canulado@sifizsoft.com").FirstOrDefault().Secuencial;

						HistoricoTareaEstado histETI = new HistoricoTareaEstado
						{
							tarea = tar,
							SecuencialEstadoTarea = 4,
							FechaOperacion = DateTime.Now,
							usuario = user
						};
						db.HistoricoTareaEstado.Add(histETI);

						//Actualizando la hora en las tareas que siguen
						//Buscando las tareas que siguen y actualizarlas....por la hora de inicio.
						DateTime diaTareaI = tar.FechaInicio.Date;
						DateTime diaSiguienteI = diaTareaI.AddDays(1);
						var tareasQueSiguenI = (from t in db.Tarea
												where t.SecuencialColaborador == tar.SecuencialColaborador &&
												t.FechaInicio >= diaTareaI && t.FechaInicio < diaSiguienteI &&
												t.FechaInicio > tar.FechaInicio &&
												t.SecuencialEstadoTarea != 4
												orderby t.FechaInicio ascending
												select t).ToList();
						Tarea tAnteriorI = tar;
						for(int i = 0; i < tareasQueSiguenI.Count(); i++)
						{
							Tarea tActual = tareasQueSiguenI[i];
							TimeSpan tiempoTarea = tActual.FechaFin - tActual.FechaInicio;
							int horasTarea = tiempoTarea.Hours;
							int minutosTarea = tiempoTarea.Minutes;

							if(tActual.FechaInicio.Hour < 13 && tActual.FechaFin.Hour > 13)
							{
								horasTarea--;
							}

							tActual.FechaInicio = tAnteriorI.FechaInicio;
							tActual.FechaFin = tActual.FechaInicio.AddHours(horasTarea).AddMinutes(minutosTarea);

							if(tActual.FechaInicio.Hour < 13 && tActual.FechaFin.Hour > 13)
							{
								tActual.FechaFin = tActual.FechaFin.AddHours(1);
							}
							else if(tActual.FechaInicio.Hour == 13)
							{
								tActual.FechaInicio = tActual.FechaInicio.AddHours(1);
								tActual.FechaFin = tActual.FechaFin.AddHours(1);
							}

							tAnteriorI = tActual;
						}

						//para la actualizacion de los permisos y las tareas
						listaDiaColaborador.Add(new DiaColaborador { Fecha = diaTareaI, IdColaborador = tar.SecuencialColaborador });
						listaCambiosTareas.Add(new DiaColaborador { Fecha = diaTareaI, IdColaborador = tar.SecuencialColaborador });
					}
				}

				db.SaveChanges();

				//Actualizando las fechas de los dias que existieron los cambios y en las fechas
				foreach(DiaColaborador diaColab in listaDiaColaborador)
				{
					Utiles.OrdenarTareasPermisos(diaColab.Fecha, diaColab.IdColaborador, user, db);
				}

				//Actualizando cambios en la interfaz de usuario                
				ActualizarTDTarea(listaCambiosTareas);

				var resp = new
				{
					success = true,
					msg = "Se ha anulado correctamente la tarea"
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					msg = e.Message
					//msg = "Ha ocurrido un error en la operación"
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult ActualizarTareaUsuario(int idTarea, int estado)
		{
			try
			{
				Tarea tarea = db.Tarea.FirstOrDefault(x => x.Secuencial == idTarea);
				if(tarea == null)
				{
					throw new Exception("No se encontró la tarea, contacte a soporte del sitio");
				}
				if(estado == 2 && tarea.SecuencialEstadoTarea == 3)//En desarrollo
				{
					throw new Exception("La tarea no puede ser cambiada a desarrollo porque ya ha sido terminada");
				}
				if(estado == 3 && tarea.tareaActividadRealizada.Count() == 0)//Terminada
				{
					throw new Exception("La tarea no se puede terminar puesto que no contiene actividades realizadas.");
				}
				if(estado == 4)//Anulada
				{
					if(tarea.ticketTarea.Where(x => x.EstaActiva == 1).Count() > 0)
					{
						throw new Exception("La tarea no se puede anular porque que está asociada a un ticket, puede reasignarla o moverla de fecha.");
					}
				}

				tarea.SecuencialEstadoTarea = estado;

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				HistoricoTareaEstado histET = new HistoricoTareaEstado
				{
					tarea = tarea,
					SecuencialEstadoTarea = estado,
					FechaOperacion = DateTime.Now,
					usuario = user
				};
				db.HistoricoTareaEstado.Add(histET);

				db.SaveChanges();

				//Actualizando la IU en la actualizacion de la tarea
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();
				listaCambiosTareas.Add(new DiaColaborador
				{
					Fecha = tarea.FechaInicio,
					IdColaborador = tarea.SecuencialColaborador
				});
				ActualizarTDTarea(listaCambiosTareas);

				if(estado == 3)//Terminado
				{
					/*Verificando si la tarea está relacionada a un ticket y si es la última de las tareas por terminar*/
					TicketTarea ticketTarea = tarea.ticketTarea.FirstOrDefault();
					if(ticketTarea != null)//Aqui la tarea está asociada a un ticket
					{
						int idTicket = ticketTarea.SecuencialTicket;

						int cantNoTerminados = (from tt in db.TicketTarea
												join t in db.Tarea on tt.SecuencialTarea equals t.Secuencial
												where (t.SecuencialEstadoTarea != 3 && t.SecuencialEstadoTarea != 4) &&
													   tt.SecuencialTicket == idTicket
												select (t.Secuencial)
											   ).ToList().Count();

						if(cantNoTerminados == 0)//Todas las asignaciones están terminadas
						{
							Ticket ticket = db.Ticket.Find(idTicket);
							//Pasando el ticket de estado a Resuelto

							if(ticket.SecuencialEstadoTicket != 10)
							{
								//Cambiando el estado del ticket
								ticket.SecuencialEstadoTicket = 10;//EL TICKET ESTA RESUELTO

								//Adicionando el histórico del ticket                                                        
								int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

								TicketHistorico ticketHistorico = new TicketHistorico
								{
									ticket = ticket,
									Version = numeroVersion,
									SecuencialEstadoTicket = 10,//  ("EL TICKET ESTA RESUELTO")
									SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
									SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
									SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
									SecuencialProximaActividad = ticket.SecuencialProximaActividad,
									SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
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

								//Enviando el correo a los usuarios
								List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");
								Persona personaCliente = ticket.persona_cliente.persona;
								string nombreCliente = personaCliente.Nombre1 + " " + personaCliente.Apellido1;
								string correoCliente = personaCliente.usuario.FirstOrDefault().Email;
								string correoColaborador = tarea.colaborador.persona.usuario.FirstOrDefault().Email;
								correosDestinos.Insert(0, correoColaborador);

								string textoEmail = "<div class=\"textoCuerpo\">Por medio del presente correo le informamos que la terminación de esta tarea dio por <b>'RESUELTO'</b> el ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b>.<br/>                                       
                                      <b>Asunto del ticket: </b>" + ticket.Asunto + @"<br/>
                                      Por favor comuníquese lo antes posible con el cliente.<br/>
                                      Nombre del cliente: " + nombreCliente + @"<br/>
                                      Correo del cliente: " + correoCliente + @"<br/></div>";

								//Borrar aqui
								string codigoCliente = ticket.persona_cliente.cliente.Codigo;
								Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Resuelto (" + ticket.Asunto + ")");

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

							//Adicionando el histórico del ticket                                                        
							int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

							TicketHistorico ticketHistorico = new TicketHistorico
							{
								ticket = ticket,
								Version = numeroVersion,
								SecuencialEstadoTicket = 9,//EL TICKET ESTA EN DESARROLLO
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

							var idColaboradoresTicket = (from tt in db.TicketTarea
														 join tk in db.Ticket on tt.SecuencialTicket equals tk.Secuencial
														 join ta in db.Tarea on tt.SecuencialTarea equals ta.Secuencial
														 where tt.SecuencialTicket == ticket.Secuencial
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
		[Authorize]
		public ActionResult DarDiasActividadesTareas()
		{
			try
			{
				int cantDias = 2;
				//if ((int)DateTime.Today.DayOfWeek == 1)//lunes
				//    cantDias = 4;
				//List<string> dias = new List<string>();
				//for (int i = 0; i < cantDias; i++)
				//{
				//    dias.Add( DateTime.Today.AddDays(-1 * i).ToString("dd/MM/yyyy") );
				//}
				List<string> dias = new List<string>();
				dias.Add(DateTime.Today.ToString("dd/MM/yyyy"));

				cantDias = 7;
				List<string> diasCord = new List<string>();
				for(int i = 0; i < cantDias; i++)
				{
					diasCord.Add(DateTime.Today.AddDays(i).ToString("dd/MM/yyyy"));
				}

				var resp = new
				{
					success = true,
					dias = dias,
					diasCord = diasCord
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult AdicionarActividadTarea(int idTarea, int tipoTarea, string fecha, string horaInicio, string horaFin)
		{
			try
			{
				Tarea tar = db.Tarea.Find(idTarea);
				if(tar == null)
				{
					throw new Exception("No se encontró la tarea, contacte el admin del sistema");
				}

				DateTime fechaActividad = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

				string textoFechaInicio = fecha + " " + horaInicio;
				DateTime horaInicioActividad = DateTime.ParseExact(textoFechaInicio, "d/M/yyyy HH:mm", CultureInfo.InvariantCulture);
				//DateTime horaInicioActividad = DateTime.Parse(textoFechaInicio);

				string textoFechaFin = fecha + " " + horaFin;
				//DateTime horaFinActividad = DateTime.Parse(textoFechaFin);
				DateTime horaFinActividad = DateTime.ParseExact(textoFechaFin, "d/M/yyyy HH:mm", CultureInfo.InvariantCulture);

				//Verificaciones de 2 dias
				int cantDias = 1;
				if((int)DateTime.Today.DayOfWeek == 1)//Si es lunes dejar hasta el viernes
				{
					cantDias = 3;
				}
				DateTime diaAnterior = DateTime.Today.AddDays(-1 * cantDias);
				if(fechaActividad < diaAnterior)
				{
					throw new Exception("La actividad no se puede entrar al sistema, la fecha es anterior a " + cantDias + " días atrás.");
				}
				if(horaFinActividad < horaInicioActividad)
				{
					throw new Exception("La actividad no se puede entrar al sistema, la hora final es menor que la hora inicial.");
				}

				//Verificando que las horas ya hayan ocurrido
				if(horaFinActividad > DateTime.Now || horaInicioActividad > DateTime.Now)
				{
					//throw new Exception("La actividad no se puede entrar al sistema, las horas de inicio y final son mayores que la hora actual.");
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

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

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
							horaInicio = act.horaInicio.ToString("HH:mm tt"),
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
		[Authorize(Roles = "ADMIN")]
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
							horaInicio = act.horaInicio.ToString("HH:mm tt"),
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
		[Authorize(Roles = "ADMIN, REC")]
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
		[Authorize(Roles = "ADMIN")]
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult AdicionarComentarioActividad(int idActividad, string descripcion, string importancia = "Normal")
		{
			try
			{
				TareaActividadRealizada tareaActividad = db.TareaActividadRealizada.Find(idActividad);
				if(tareaActividad == null)
				{
					throw new Exception("No se encontró la actividad.");
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
		[Authorize(Roles = "ADMIN")]
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult MoverTarea(int idTarea, int idColaborador, string fecha)
		{
			try
			{
				bool success = true;
				string msg = "Se ha movido correctamente la tarea";

				//para la actualizacion de los permisos y las tareas
				List<DiaColaborador> listaDiaColaborador = new List<DiaColaborador>();
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();

				//buscando la tarea
				Tarea tar = db.Tarea.Find(idTarea);
				if(tar != null)//Se encontró la tarea
				{
					if(tar.SecuencialEstadoTarea == 3)//Tarea ya terminada no se puede mover
					{
						throw new Exception("La tarea no se puede mover porque está ya se terminó");
					}

					DateTime diaAnteriorTarea = tar.FechaInicio.Date;
					DateTime diaSiguiente = diaAnteriorTarea.AddDays(1);
					DateTime fechaInicio = diaAnteriorTarea.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
					DateTime fechaInicioExtra = diaAnteriorTarea.AddMinutes(30 + (1 * 60));//A las 1.30 empieza
					int horas = 0;
					int minutos = 0;

					if(tar.FechaInicio >= fechaInicio)
					{
						List<Tarea> tareas = (from t in db.Tarea
											  where t.FechaInicio >= fechaInicio &&
													t.FechaInicio < diaSiguiente &&
													t.SecuencialColaborador == tar.SecuencialColaborador &&
													t.Secuencial != tar.Secuencial &&
													t.SecuencialEstadoTarea != 4
											  orderby t.FechaInicio ascending
											  select t).ToList<Tarea>();//Las tareas del mismo día pero que son distintas
																		//Cambiando las horas de inicio y de fin de estas tareas

						foreach(Tarea tarea in tareas)
						{
							TimeSpan tiempo = tarea.FechaFin - tarea.FechaInicio;
							horas = tiempo.Hours;
							minutos = tiempo.Minutes;
							if(tarea.FechaInicio.Hour < 13 && tarea.FechaFin.Hour > 13)
							{
								horas--;
							}

							tarea.FechaInicio = fechaInicio;
							tarea.FechaFin = fechaInicio.AddHours(horas).AddMinutes(minutos);
							if(tarea.FechaInicio.Hour < 13 && tarea.FechaFin.Hour > 13)
							{
								tarea.FechaFin = tarea.FechaFin.AddHours(1);
							}
							else if(tarea.FechaInicio.Hour == 13)
							{
								tarea.FechaInicio = tarea.FechaInicio.AddHours(1);
								tarea.FechaFin = tarea.FechaFin.AddHours(1);
							}

							fechaInicio = tarea.FechaFin;
						};
					}
					else
					{
						fechaInicio = diaAnteriorTarea.AddMinutes(30 + (8 * 60));
						List<Tarea> tareas = (from t in db.Tarea
											  where t.FechaInicio >= fechaInicioExtra &&
													t.FechaInicio < fechaInicio &&
													t.SecuencialColaborador == tar.SecuencialColaborador &&
													t.Secuencial != tar.Secuencial &&
													t.SecuencialEstadoTarea != 4
											  orderby t.FechaInicio ascending
											  select t).ToList<Tarea>();//Las tareas del mismo día pero que son distintas
																		//Cambiando las horas de inicio y de fin de estas tareas

						horas = 0;
						foreach(Tarea tarea in tareas)
						{
							TimeSpan tiempo = tarea.FechaFin - tarea.FechaInicio;

							tarea.FechaInicio = fechaInicioExtra;
							tarea.FechaFin = fechaInicioExtra.Add(tiempo);

							fechaInicioExtra = tarea.FechaFin;
						};
					}

					//para la actualizacion de los permisos y las tareas
					listaDiaColaborador.Add(new DiaColaborador { Fecha = diaAnteriorTarea, IdColaborador = tar.SecuencialColaborador });
					listaCambiosTareas.Add(new DiaColaborador { Fecha = diaAnteriorTarea, IdColaborador = tar.SecuencialColaborador });

					//Moviendo al nuevo colaborador o al mismo
					tar.SecuencialColaborador = idColaborador;
					//Buscando las tareas del colaborador en la fecha nueva
					string[] fechas = fecha.Split(new Char[] { '/' });
					int dia = Int32.Parse(fechas[0]);
					int mes = Int32.Parse(fechas[1]);
					int anno = Int32.Parse(fechas[2]);
					DateTime diaTarea = new System.DateTime(anno, mes, dia);
					DateTime diaTareaHoraInicial = diaTarea.AddMinutes(30 + (8 * 60));
					DateTime diaTareaHoraExtra = diaTarea.AddMinutes(30 + (1 * 60));
					diaSiguiente = diaTarea.AddDays(1);
					fechaInicio = diaAnteriorTarea.AddMinutes(30 + (8 * 60));

					if(tar.FechaInicio >= fechaInicio)
					{
						var tareasDia = (from t in db.Tarea
										 where t.SecuencialColaborador == idColaborador &&
											   t.FechaInicio >= diaTareaHoraInicial && t.FechaInicio < diaSiguiente &&
											   t.SecuencialEstadoTarea != 4
										 select new
										 {
											 finicio = t.FechaInicio,
											 ffin = t.FechaFin
										 }).ToList();
						int time = 0;
						int timeMinutos = 0;
						foreach(var tarea in tareasDia)
						{
							TimeSpan tiempo = tarea.ffin - tarea.finicio;
							time += tiempo.Hours;
							timeMinutos += tiempo.Minutes;
						}

						DateTime fInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
						fInicio = fInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza

						//Calculando las horas de la tarea
						horas = 0;
						horas = (tar.FechaFin - tar.FechaInicio).Hours;
						minutos = (tar.FechaFin - tar.FechaInicio).Minutes;
						if(tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
						{
							horas--;
						}
						else if(tar.FechaInicio.Hour == 13)
						{
							horas--;
						}

						tar.FechaInicio = fInicio;
						tar.FechaFin = fInicio.AddHours(horas).AddMinutes(minutos);

						if(tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
						{
							tar.FechaFin = tar.FechaFin.AddHours(1);
						}
						else if(tar.FechaInicio.Hour == 13)
						{
							tar.FechaInicio = tar.FechaInicio.AddHours(1);
							tar.FechaFin = tar.FechaFin.AddHours(1);
						}
					}
					else
					{
						var tareasDia = (from t in db.Tarea
										 where t.SecuencialColaborador == idColaborador &&
											   t.FechaInicio >= diaTareaHoraExtra && t.FechaInicio < diaTareaHoraInicial &&
											   t.SecuencialEstadoTarea != 4
										 select new
										 {
											 finicio = t.FechaInicio,
											 ffin = t.FechaFin
										 }).ToList();
						int time = 0;
						int timeMinutos = 0;
						foreach(var tarea in tareasDia)
						{
							TimeSpan tiempo = tarea.ffin - tarea.finicio;
							time += tiempo.Hours;
							timeMinutos += tiempo.Minutes;
						}

						DateTime fInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
						fInicio = fInicio.AddMinutes(30 + (1 * 60));//A las 1.30 empieza

						//Calculando las horas de la tarea
						horas = 0;
						horas = (tar.FechaFin - tar.FechaInicio).Hours;
						minutos = (tar.FechaFin - tar.FechaInicio).Minutes;

						tar.FechaInicio = fInicio;
						tar.FechaFin = fInicio.AddHours(horas).AddMinutes(minutos);
					}

					//para la actualizacion de los permisos y las tareas
					listaDiaColaborador.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tar.SecuencialColaborador });
					listaCambiosTareas.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tar.SecuencialColaborador });
				}
				else
				{
					success = false;
					msg = "Error, No se encontró la tarea";
				}

				db.SaveChanges();

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				//Actualizando las fechas de los dias que existieron los cambios y en las fechas
				foreach(DiaColaborador diaColab in listaDiaColaborador)
				{
					Utiles.OrdenarTareasPermisos(diaColab.Fecha, diaColab.IdColaborador, user, db);
				}

				//Actualizando cambios en la interfaz de usuario                
				ActualizarTDTarea(listaCambiosTareas);

				var resp = new
				{
					success = success,
					msg = msg
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult OrdenarTareas(string idOrden)
		{
			try
			{
				var s = new JavaScriptSerializer();
				var arrayId = s.Deserialize<dynamic>(idOrden);
				int idTarea1 = int.Parse(arrayId[0]);

				Tarea tar = db.Tarea.Find(idTarea1);
				DateTime fecha = tar.FechaInicio.Date;

				DateTime fInicio = fecha.AddMinutes(30 + (8 * 60));//A las 8.30 empieza

				foreach(string idTar in arrayId)
				{
					Tarea tarea = db.Tarea.Find(int.Parse(idTar));
					//Buscando las horas que tarda la tarea
					int horas = (tarea.FechaFin - tarea.FechaInicio).Hours;
					int minutos = (tarea.FechaFin - tarea.FechaInicio).Minutes;
					if(tarea.FechaInicio.Hour < 13 && tarea.FechaFin.Hour > 13)
					{
						horas--;
					}
					else if(tarea.FechaInicio.Hour == 13)
					{
						horas--;
					}

					tarea.FechaInicio = fInicio;
					tarea.FechaFin = fInicio.AddHours(horas).AddMinutes(minutos);

					if(tarea.FechaInicio.Hour < 13 && tarea.FechaFin.Hour > 13)
					{
						tarea.FechaFin = tarea.FechaFin.AddHours(1);
					}
					else if(tarea.FechaInicio.Hour == 13)
					{
						tarea.FechaInicio = tarea.FechaInicio.AddHours(1);
						tarea.FechaFin = tarea.FechaFin.AddHours(1);
					}

					fInicio = tarea.FechaFin;
				}

				db.SaveChanges();

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				//Actualizando las fechas de los dias que existieron los cambios y en las fechas
				Utiles.OrdenarTareasPermisos(fecha, tar.SecuencialColaborador, user, db);
				//Actualizando la IU en la actualizacion de la tarea
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();
				listaCambiosTareas.Add(new DiaColaborador
				{
					Fecha = fecha,
					IdColaborador = tar.SecuencialColaborador
				});
				ActualizarTDTarea(listaCambiosTareas);

				var resp = new
				{
					success = true,
					msg = "Operación realizada correctamente"
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarDatosTarea(int idTarea)
		{
			var tarea = (from t in db.Tarea
						 join
							 c in db.Colaborador on t.colaborador equals c
						 join
							 p in db.Persona on c.persona equals p
						 where t.Secuencial == idTarea
						 select new
						 {
							 id = t.Secuencial,
							 idTrabajador = t.SecuencialColaborador,
							 nombre = p.Nombre1 + " " + p.Apellido1,
							 sede = c.sede.Codigo,
							 detalle = t.Detalle,
							 referencia = (t.entregableMotivoTrabajo != null) ? t.entregableMotivoTrabajo.Secuencial : 0,
							 finicio = t.FechaInicio,
							 ffin = t.FechaFin,
							 horasEstimadas = t.TiempoEstimacion.HasValue ? t.TiempoEstimacion.Value.Hours : 0,
							 minutosEstimados = t.TiempoEstimacion.HasValue ? t.TiempoEstimacion.Value.Minutes : 0,
							 modulo = t.SecuencialModulo,
							 cliente = t.SecuencialCliente,
							 actividad = t.SecuencialActividad,
							 estado = t.SecuencialEstadoTarea,
							 verificador = t.NumeroVerificador,
							 ubicacion = t.SecuencialLugarTarea,
							 esReproceso = t.EsReproceso == 1 ? true : false,
							 coordinador = (from tc in db.Tarea_Coordinador
											where tc.SecuencialTarea == t.Secuencial && tc.EstaActivo == 1
											select tc.SecuencialColaborador).FirstOrDefault()
						 }).First();

			DateTime horaInicioTareas = tarea.finicio.Date;
			horaInicioTareas = horaInicioTareas.AddHours(8).AddMinutes(30);

			TicketTarea tt = db.TicketTarea.Where(s => s.SecuencialTarea == tarea.id && s.EstaActiva == 1).FirstOrDefault();

			TimeSpan tiempoTarea = tarea.ffin - tarea.finicio;
			int horas = tiempoTarea.Hours;
			int minutos = tiempoTarea.Minutes;

			if(tarea.finicio.Hour < 13 && tarea.ffin.Hour > 13)
			{
				horas--;
			}

			var resp = new
			{
				success = true,
				tarea = tarea,
				fecha = tarea.finicio.ToString("dd/MM/yyyy"),
				horas = horas,
				minutos = minutos,
				extraordinaria = DateTime.Compare(tarea.finicio, horaInicioTareas) < 0,
				ticketTarea = tt != null ? tt.SecuencialTicket : 0
			};
			return Json(resp);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult SolicitudAccesoTFS(int idCliente, int idColaborador)
		{
			try
			{
				string emailUser = User.Identity.Name;
				var clienteTarea = db.Cliente.Find(idCliente);
				var colaboradorTarea = db.Colaborador.Find(idColaborador);
				List<string> correosDestinos = new List<string>();
				correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("TFS"));
				correosDestinos.Add(emailUser);
				correosDestinos.Add(colaboradorTarea.persona.usuario.FirstOrDefault().Email);

				string textoEmail = @"<div class='textoCuerpo'><br/>";
				textoEmail += "Buen día,";
				textoEmail += @"<br/>";

				textoEmail += "Con el presente correo se solicita acceso a las fuentes del cliente: " + clienteTarea.Descripcion + ", al colaborador: " + colaboradorTarea.persona.Nombre1 + " " + colaboradorTarea.persona.Apellido1;
				textoEmail += "</div>";


				string asuntoEmail = "Solicitud Acceso";
				Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail);

				var resp = new
				{
					success = true,
					msg = "Se ha realizado el envío del correo."
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult AsignacionesPersona(int idTrabajador, string password = "", string fechaLunes = "", string jsonCC = "")
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

			try
			{
				DateTime fechaFin = lunes.AddDays(7);
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
								 sd in db.Sede on c.sede equals sd
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
							 join d in db.Departamento on c.departamento equals d

							 where u.EstaActivo == 1 && d.Asignable == 1 &&
								   t.FechaInicio >= lunes && t.FechaInicio < fechaFin && //Entre las dos fechas
								   t.SecuencialEstadoTarea != 4//Es cuando están anuladas
							 orderby t.FechaInicio, p.Nombre1, p.Apellido1
							 select new
							 {
								 idColaborador = c.Secuencial,
								 nombre = p.Nombre1 + " " + p.Apellido1,
								 email = u.Email.ToUpper(),
								 sede = sd.Codigo,
								 url = f.Url,
								 idTarea = t.Secuencial,
								 sdetalle = t.Detalle.Substring(0, 20) + "...",
								 detalle = t.Detalle,
								 finicio = t.FechaInicio,
								 ffin = t.FechaFin,
								 modulo = m.Codigo,
								 idModulo = m.Secuencial,
								 cliente = cl.Codigo,
								 idCliente = cl.Secuencial,
								 dCliente = cl.Descripcion.ToUpper(),
								 actividad = a.Codigo,
								 dActividad = a.Descripcion.ToUpper(),
								 estado = e.Codigo,
								 idEstado = e.Secuencial,
								 lugar = l.Codigo,
								 idLugar = l.Secuencial,
								 clase = (t.SecuencialEstadoTarea == 1 ? "new" : (t.SecuencialEstadoTarea == 2) ? "dev" : "finish"),
								 idCoordinador = (from tc in db.Tarea_Coordinador
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
									join
										p in db.Persona on t.persona equals p
									join
										f in db.FotoColaborador on t.Secuencial equals f.SecuencialColaborador
									join
										sd in db.Sede on t.sede equals sd
									join
										u in db.Usuario on p.Secuencial equals u.SecuencialPersona
									join d in db.Departamento on t.departamento equals d
									where u.EstaActivo == 1 && d.Asignable == 1
									orderby p.Nombre1, p.Apellido1
									select new
									{
										id = t.Secuencial,
										nombre = p.Nombre1 + " " + p.Apellido1,
										nombre1 = p.Nombre1,
										apellido1 = p.Apellido1,
										sexo = p.Sexo,
										email = u.Email.ToLower(),
										sede = sd.Codigo,
										url = f.Url
									}).ToList();



				string htmlCss = @"<style>
                                               .textoCuerpo{
                                                    font-size: 11pt;
                                                    font-family: ""Calibri"", sans-serif;
                                                    color: #1F497D;
                                               }
                                               .cabecera{
                                                    font-size: 8pt;
                                                    font-family: ""Calibri"", sans-serif;
                                                    border-bottom: 1px solid #222;
                                               }                                                                              
                                               table td{
                                                    width: 160px;
                                               }
                                               table th {
                                                    border: 1px solid black;
                                                    border-collapse: collapse;
                                                    font-size: 8pt;
                                                    background: #aaa;
                                                    font-family: ""Calibri"", sans-serif;
                                               }
                                               table, td {
                                                    border: 1px solid black;
                                                    border-collapse: collapse;
                                                    font-size: 8pt;
                                                    background: #ccc;
                                                    font-family: ""Calibri"", sans-serif;
                                                    vertical-align: top;
                                                }
                                                th, td {
                                                    padding: 10px;
                                                }
                                                .resaltar{
                                                    background: #FFFF00 !important;
                                                    font-size: 8pt;
                                                    font-family: ""Calibri"", sans-serif;
                                                    border-bottom: 1px solid #222;
                                                }

                                                /* Font Definitions */
                                                @font-face
	                                                {font-family:""Cambria Math"";
	                                                panose-1:2 4 5 3 5 4 6 3 2 4;}
                                                @font-face
	                                                {font-family:Calibri;
	                                                panose-1:2 15 5 2 2 2 4 3 2 4;}
                                                @font-face
	                                                {font-family:Verdana;
	                                                panose-1:2 11 6 4 3 5 4 4 2 4;}
                                                @font-face
	                                                {font-family:""Palatino Linotype"";
	                                                panose-1:2 4 5 2 5 5 5 3 3 4;}
                                                /* Style Definitions */
                                                p.MsoNormal, li.MsoNormal, div.MsoNormal
	                                                {margin:0cm;
	                                                margin-bottom:.0001pt;
	                                                font-size:11.0pt;
	                                                font-family:""Calibri"",sans-serif;
	                                                mso-fareast-language:EN-US;}
                                                a:link, span.MsoHyperlink
	                                                {mso-style-priority:99;
	                                                color:#0563C1;
	                                                text-decoration:underline;}
                                                a:visited, span.MsoHyperlinkFollowed
	                                                {mso-style-priority:99;
	                                                color:#954F72;
	                                                text-decoration:underline;}
                                                span.EstiloCorreo17
	                                                {mso-style-type:personal-compose;
	                                                font-family:""Calibri"",sans-serif;
	                                                color:windowtext;}
                                                .MsoChpDefault
	                                                {mso-style-type:export-only;
	                                                font-family:""Calibri"",sans-serif;
	                                                mso-fareast-language:EN-US;}
                                                @page WordSection1
	                                                {size:612.0pt 792.0pt;
	                                                margin:70.85pt 3.0cm 70.85pt 3.0cm;}
                                                div.WordSection1
	                                                {page:WordSection1;}
                                           </style>";

				string emailUser = User.Identity.Name;
				var envia = (from t in db.Colaborador
							 join
								 p in db.Persona on t.persona equals p
							 join
								 u in db.Usuario on p.Secuencial equals u.SecuencialPersona
							 join
								 c in db.Cargo on t.cargo equals c
							 where u.Email == emailUser
							 select new
							 {
								 nombre1 = p.Nombre1.ToLower(),
								 apellido1 = p.Apellido1.ToLower(),
								 cargo = c.Descripcion.ToLower()
							 }).FirstOrDefault();
				string htmlfirma = @"<div class=WordSection1>
                                        <p class=MsoNormal><o:p>&nbsp;</o:p></p><p class=MsoNormal><o:p>&nbsp;</o:p></p><p class=MsoNormal><span lang=ES-EC style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>Atentamente,<o:p></o:p></span></p><p class=MsoNormal><i><span style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>"
								+
								Utiles.UpperCamelCase(envia.nombre1) + " " + Utiles.UpperCamelCase(envia.apellido1)
								+
								@"<o:p></o:p></span></i></p><p class=MsoNormal><span style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>"
								+
								Utiles.PrimeraMayuscula(envia.cargo)
								+
								@"<br> 
                                <b style='color:#1F497D !important;'>
                                    <i>
                                        02-450-4616 <br/>
                                        Quito - Ecuador
                                    </i>
                                </b></span><span lang=IT style='font-size:12.0pt;font-family:'Times New Roman',serif;color:#203864;mso-fareast-language:ES'><o:p></o:p></span></p><div class=MsoNormal align=center style='text-align:center'><span lang=EN style='font-size:9.0pt;font-family:'Verdana',sans-serif;color:#1F497D;mso-fareast-language:ES-EC'><hr size=3 width='100%' align=center></span></div><p class=MsoNormal><b><span lang=ES style='color:#1F497D;mso-fareast-language:ES-EC'>Somos líderes en la producción de software financiero-contable de última tecnología. </span></b><b><span lang=ES style='font-family:'Times New Roman',serif;color:#1F497D;mso-fareast-language:ES'><o:p></o:p></span></b></p><p class=MsoNormal><a href='http://www.sifizsoft.com/'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:2.0pt;mso-text-raise:-2.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=129 height=49 id='Imagen_x0020_2' src='cid:sifizsoft.jpg' alt='cid:image001.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;&nbsp;<span style='position:relative;top:-3.0pt;mso-text-raise:3.0pt'>&nbsp;</span></span><span lang=EN-US style='font-size:9.0pt;color:#1F497D;position:relative;top:-18.0pt;mso-text-raise:18.0pt;letter-spacing:.2pt;mso-fareast-language:ES-EC'>Like us in</span><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp; </span><a href='http://www.facebook.com/pages/SifizSoft/287494208026463?sk=app_129982580378550'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=41 height=41 id='Imagen_x0020_3' src='cid:fb.jpg' alt='cid:image002.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;</span><span lang=EN-US style='font-size:9.0pt;color:#1F497D;position:relative;top:-18.0pt;mso-text-raise:18.0pt;mso-fareast-language:ES-EC'>and Follow us on</span><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'> </span><a href='https://twitter.com/SifizSoftSA'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=41 height=41 id='Imagen_x0020_4' src='cid:tw.jpg' alt='cid:image003.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;</span><a href='http://lnkd.in/GYc2-s'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=106 height=41 id='Imagen_x0020_5' src='cid:linkedin.jpg' alt='cid:image004.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp; &nbsp;</span><a href='http://www.efqm.org/en/'><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=101 height=55 id='Imagen_x0020_6' src='cid:efqm.jpg' alt='cid:image005.png@01D244E9.77AAB2B0'></span></a><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'> </span><a href='http://www.openkm.com/en/'><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=109 height=53 id='Imagen_x0020_7' src='cid:openkm.jpg' alt='cid:image006.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'><o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center;line-height:17.0pt;mso-line-height-rule:exactly;text-autospace:none'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>OFICINA QUITO: Rumipamba E2-214 y Av. República, edificio Signature, piso 09, oficina 901</span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>.&nbsp; Teléfonos&nbsp; </span><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>02-351-7729, &nbsp;02-351-8919, 02-450-4616, 02-450-4727, </span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>desde USA 1(407)255 8532<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>OFICINA AMBATO: Av. Atahualpa y Pasaje Arajuno S/N a una cuadra del nuevo municipio.&nbsp; Teléfono 03-241-6586&nbsp; 03-241-9127<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>PO&nbsp; BOX 780066&nbsp;Orlando, FL 32878-0066 Toll free (800) 793-8369<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>Este correo electrónico es solo para uso del destinatario y puede contener información confidencial. Cualquier distribución uso o lectura de este material está expresamente prohibido. Si usted no es el destinatario o si usted ha recibido este correo electrónico por error por favor contacte al remitente y destruya todas las copias y el mensaje original.</span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES'><o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>This E-mail message is for the sole use of the intended recipient(s) and may contain confidential and privileged information. Any unauthorized review, use, disclosure or distribution is prohibited. If you are not the intended recipient, please contact the sender by reply E-mail and destroy all copies of the original message.<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>Copyrights ©SifizSoft&nbsp;2004-2016 carefully reserved and preserved</span><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES'><o:p></o:p></span></p><p class=MsoNormal><o:p>&nbsp;</o:p></p></div></body></html>";
				string[] imagenes = new string[6] { "sifizsoft.jpg", "fb.jpg", "tw.jpg", "linkedin.jpg", "efqm.jpg", "openkm.jpg" };

				string[] dias = new string[7] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

				List<int> listaIdColaboradoresDestinos = new List<int>();

				var trabajador = (from t in trabajadores
								  where t.id == idTrabajador
								  select t).FirstOrDefault();

				bool notificarViatico = false;
				string htmlMail = "";
				List<string> HtmlDiasTarea = new List<string>();
				for(int j = 0; j < 7; j++)//son 7 Días los de la semana
				{
					string htmlDia = "";
					DateTime fecha = lunes.AddDays(j);
					DateTime fechaDespues = lunes.AddDays(j + 1);
					var tareas = (from d in datos
								  where d.idColaborador == idTrabajador &&
									  d.finicio > fecha && d.ffin < fechaDespues
								  select new
								  {
									  id = d.idTarea,
									  sdetalle = d.sdetalle,
									  detalle = d.detalle,
									  fechaInicio = d.finicio,
									  fechaFin = d.ffin,
									  finicio = d.finicio.ToString("t"),
									  ffin = d.ffin.ToString("t"),
									  modulo = d.modulo,
									  idModulo = d.idModulo,
									  cliente = d.cliente,
									  idCliente = d.idCliente,
									  dCliente = d.dCliente,
									  actividad = d.actividad,
									  dActividad = d.dActividad,
									  estado = d.estado,
									  idEstado = d.idEstado,
									  lugar = d.lugar,
									  idLugar = d.idLugar,
									  clase = d.clase,
									  idCoordinador = d.idCoordinador,
									  coordinador = d.coordinador
								  }).ToList();

					//Aquí están las tareas del día
					int it = 1;
					foreach(var tarea in tareas)
					{
						bool cambiarColorViatico = false;
						if(("OF" + trabajador.sede) != tarea.lugar)
						{
							notificarViatico = true;
							cambiarColorViatico = true;
						}

						string strTiempoTarea = Utiles.CalcularHorasTarea(tarea.fechaInicio, tarea.fechaFin);

						if(cambiarColorViatico)
						{
							htmlDia += "<div class=\"resaltar\"><b>" + it + ". " + tarea.lugar + " (" + tarea.dCliente + ") " + strTiempoTarea + "</b></div>";
							htmlDia += "<div>" + Utiles.PrimeraMayuscula(tarea.detalle.ToLower()) + "</div>";
						}
						else
						{
							htmlDia += "<div class=\"cabecera\"><b>" + it + ". " + tarea.lugar + " (" + tarea.dCliente + ") " + strTiempoTarea + "</b></div>";
							htmlDia += "<div>" + Utiles.PrimeraMayuscula(tarea.detalle.ToLower()) + "</div>";
						}

						if(tarea.coordinador != null)
						{
							htmlDia += "<div>Coordinar con: " + tarea.coordinador + "";
							int cantidad = listaIdColaboradoresDestinos.Where(x => x == tarea.idCoordinador).Count();//Verificando que no se repita el coordinador
							if(cantidad == 0)
							{
								listaIdColaboradoresDestinos.Add(tarea.idCoordinador);
							}
						}
						htmlDia += "<br/><br/>";
						it++;
					}

					HtmlDiasTarea.Add(htmlDia);
				}

				htmlMail += @"<table>
                                        <thead>
                                            <th>&nbsp;</th>";
				for(int k = 0; k < 7; k++)
				{
					htmlMail += "<th>" + "<div style=\"text-align: center;\"><b>" + dias[k] + lunes.AddDays(k).ToString(" dd") + "</b><div>" + "</th>";
				}
				htmlMail += "</thead><tr><td>" + trabajador.nombre + " (" + trabajador.sede + ")" + "</td>";

				foreach(string htmlDia in HtmlDiasTarea)
				{
					htmlMail += "<td>" + htmlDia + "</td>";
				}
				htmlMail += "</tr></table> <br/>";

				string saludo = "Estimado " + Utiles.UpperCamelCase(trabajador.nombre1.ToLower()) + " " + Utiles.UpperCamelCase(trabajador.apellido1.ToLower());
				if(trabajador.sexo == "F")
				{
					saludo = "Estimada " + Utiles.UpperCamelCase(trabajador.nombre1.ToLower()) + " " + Utiles.UpperCamelCase(trabajador.apellido1.ToLower());
				}

				string htmlMailFinal = "<div class=\"textoCuerpo\">" + saludo + ",<br/>Para su información, esta es la asignación para la semana del " + lunes.ToString("dd/MM/yyyy") + ". Si hay algún cambio se lo haremos conocer inmediatamente.<p><br/>" + htmlMail;

				htmlMailFinal = htmlCss + htmlMailFinal;

				//Enviando el correo                    
				htmlMailFinal += htmlfirma;
				string asunto = "Actualización de Asignaciones Semana " + lunes.ToString("dd/MM");

				List<string> listaEmailDestinos = new List<string>();
				listaEmailDestinos = (from p in db.Persona
									  join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
									  join pge in db.PersonaGrupoEmail on p.Secuencial equals pge.SecuencialPersona
									  join ge in db.GrupoEmail on pge.grupoEmail equals ge
									  where ge.Codigo == "CASG" && ge.EstaActivo == 1 && u.EstaActivo == 1 && pge.EstaActivo == 1
									  select u.Email).ToList<string>();
				listaEmailDestinos.Insert(0, trabajador.email);

				//Insertando en la copia del email tambien los coordinadores
				foreach(int idCoordinador in listaIdColaboradoresDestinos)
				{
					var trab = trabajadores.Where(x => x.id == idCoordinador).FirstOrDefault();
					string email = "";
					if(trab != null)//Buscar directamente en la tabla
					{
						email = trab.email;
					}
					else
					{
						email = (from u in db.Usuario
								 join
				 p in db.Persona on u.persona equals p
								 join
c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
								 where c.Secuencial == idCoordinador && u.EstaActivo == 1
								 select u.Email).FirstOrDefault();
					}
					if(listaEmailDestinos.Where(x => x == email).Count() == 0)
					{
						listaEmailDestinos.Add(email);
					}
				}

				var s = new JavaScriptSerializer();
				var jsonObj = s.Deserialize<dynamic>(jsonCC);
				foreach(string email in jsonObj)
				{
					listaEmailDestinos.Add(email);
				}

				string[] emailDestinos = listaEmailDestinos.ToArray<string>();
				//string[] emailDestinos = new string[] { "rcespedes@sifizsoft.com" };//Borrar Aqui                  

				if(notificarViatico)//Para la notificacion de los viáticos.
				{
					//List<string> emails = new List<string>();
					//emails.Add("rcespedes@sifizsoft.com");
					//emails.Add("recepcion@sifizsoft.com");
					//emails.Add("ndelgado@sifizsoft.com");
					//string[] emailsViaticos = emails.ToArray();
					//if (trabajador.sede == "AMB")
					//{
					//    //emails.Add("fbarrionuevo@sifizsoft.com");
					//}

					//Buscando los directivos posibles viáticos en la BD
					string[] emailsViaticos = (from p in db.Persona
											   join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
											   join pge in db.PersonaGrupoEmail on p.Secuencial equals pge.SecuencialPersona
											   join ge in db.GrupoEmail on pge.grupoEmail equals ge
											   where ge.Codigo == "CASV" && ge.EstaActivo == 1 && u.EstaActivo == 1 && pge.EstaActivo == 1
											   select u.Email).ToArray<string>();

					string htmlMailFinalV = @"<div class=""textoCuerpo"">Estimados, mediante este correo se les notifican cambios en las asignaciones a colaboradores,        
                                                estas asignaciones pudieran necesitar gestión logística para la semana del " + lunes.ToString("dd/MM/yyyy") + ". Si hay algún cambio se lo haremos conocer inmediatamente.<p><br/>" + htmlMail + "</div>";

					htmlMailFinalV = htmlCss + htmlMailFinalV;
					htmlMailFinalV += htmlfirma;
					string asuntoV = "Actualización de Asignaciones Semana " + lunes.ToString("dd/MM");

					Utiles.EnviarEmail(emailUser, emailsViaticos, htmlMailFinalV, asuntoV, password, true, imagenes);
				}

				Utiles.EnviarEmail(emailUser, emailDestinos, htmlMailFinal, asunto, password, true, imagenes);

				var resp1 = new
				{
					success = true,
					msg = "Se ha realizado el envío del correo."
				};
				return Json(resp1);
			}
			catch(Exception e)
			{
				var resp3 = new
				{
					success = false,
					msg = e.Message
				};
				return Json(resp3);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarCoordinadosColaborador(int idTrabajador, string fechaLunes, int semanas)
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
								   t.FechaInicio > lunes && t.FechaInicio < fechaFin && //Entre las dos fechas
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
								   where u.EstaActivo == 1 && c.Secuencial == idTrabajador
								   select new
								   {
									   idColaborador = c.Secuencial,
									   email = u.Email.ToUpper()
								   }).FirstOrDefault();

				List<object> datosFormateados = new List<object>();
				string[] arrayString1 = colaborador.email.Split(new Char[] { '@' });
				string colaboradorLogin = arrayString1[0];
				var newData1 = new
				{
					id = colaborador.idColaborador,
					text = colaboradorLogin
				};

				datosFormateados.Add(newData1);

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
		[Authorize(Roles = "ADMIN, RRHH")]
		public ActionResult AprobarPermiso(int idPermiso)
		{
			try
			{
				Permiso permiso = db.Permiso.Find(idPermiso);
				if(permiso == null)
				{
					throw new Exception("Error, no se encontró el permiso");
				}

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				permiso.SecuencialEstadoPermiso = 2;

				//Guardando la gestión del permiso                
				GestionPermiso gestion = new GestionPermiso
				{
					permiso = permiso,
					SecuencialEstadoPermisoInicial = 1,
					SecuencialEstadoPermisoFinal = 2,
					usuario = user,
					FechaOperacion = DateTime.Now
				};
				db.GestionPermiso.Add(gestion);

				db.SaveChanges();

				Utiles.OrdenarTareasPermisos(permiso.FechaInicio.Date, permiso.SecuencialColaborador, user, db);

				//Actualizando la IU en la actualizacion de la tarea
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();
				listaCambiosTareas.Add(new DiaColaborador
				{
					Fecha = permiso.FechaInicio,
					IdColaborador = permiso.SecuencialColaborador
				});
				ActualizarTDTarea(listaCambiosTareas);

				//Informando del la aprobación del permiso
				string[] emails = new string[2] { permiso.colaborador.persona.usuario.First().Email, user.Email };
				string email = "<div class=\"textoCuerpo\">NOTIFICACIÓN VAPER: <br/>" +
								@"Le informamos que el permiso solicitado por usted ha sido aprobado,
                                los datos del permiso son los siguientes:<br/>
                                Fecha de Inicio: " + permiso.FechaInicio.ToString("dd/MM/yyyy HH:mm") + @"<br/>
                                Fecha de Fin: " + permiso.FechaFin.ToString("dd/MM/yyyy HH:mm") + @"<br/>
                                Motivo: " + permiso.Motivo + @"<br/>
                                Aprobado por: " + user.persona.Nombre1 + " " + user.persona.Apellido1 + "</div>";

				Utiles.EnviarEmailSistema(emails, email, "Permiso aprobado");

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
		[Authorize(Roles = "ADMIN")]
		public ActionResult MoverPermiso(int idPermiso, int idColaborador, string fecha)
		{
			try
			{
				Permiso permiso = db.Permiso.Find(idPermiso);
				if(permiso == null)
				{
					throw new Exception("No se encontró el permiso");
				}
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				string[] fechas = fecha.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				DateTime newFechaPermiso = new System.DateTime(anno, mes, dia);

				if(permiso.FechaInicio.Date < DateTime.Today || newFechaPermiso < DateTime.Today)
				{
					throw new Exception("Error, el permiso no se puede mover porque la fecha del permiso o la nueva fecha es/son anterior(es) al día de hoy.");
				}

				DateTime fechaInicialPermiso = permiso.FechaInicio.Date;
				int idColaboradorInicial = permiso.SecuencialColaborador;

				if(newFechaPermiso.Date != fechaInicialPermiso || idColaborador != idColaboradorInicial)//Hay un cambio en el permiso
				{
					int minutosInicial = (int)((permiso.FechaInicio - fechaInicialPermiso).TotalMinutes);

					int minutosFinal = minutosInicial + (int)((permiso.FechaFin - permiso.FechaInicio).TotalMinutes);

					permiso.FechaInicio = newFechaPermiso.AddMinutes(minutosInicial);
					permiso.FechaFin = newFechaPermiso.AddMinutes(minutosFinal);

					permiso.SecuencialColaborador = idColaborador;

					db.SaveChanges();

					Utiles.OrdenarTareasPermisos(fechaInicialPermiso, idColaboradorInicial, user, db);
					Utiles.OrdenarTareasPermisos(newFechaPermiso, idColaborador, user, db);

					//Actualizando la IU en la actualizacion de la tarea
					List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();
					listaCambiosTareas.Add(new DiaColaborador
					{
						Fecha = fechaInicialPermiso,
						IdColaborador = idColaboradorInicial
					});
					listaCambiosTareas.Add(new DiaColaborador
					{
						Fecha = newFechaPermiso,
						IdColaborador = idColaborador
					});
					ActualizarTDTarea(listaCambiosTareas);

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

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult GestionarFeriado(string fecha)
		{
			try
			{
				DateTime diaFeriado = Utiles.strToDateTime(fecha);

				DiaInactivo diaInactivo = db.DiaInactivo.Where(x => x.Fecha == diaFeriado).FirstOrDefault();
				if(diaInactivo == null)//Adicionarlo
				{
					diaInactivo = new DiaInactivo
					{
						Fecha = diaFeriado,
						Descripcion = "Día Feriado",
						EstaActivo = 1,
						NumeroVerificador = 1
					};
					db.DiaInactivo.Add(diaInactivo);
				}
				else//Hacerle un toogle
				{
					diaInactivo.EstaActivo = 1 - diaInactivo.EstaActivo;
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult RepetirTareasVerticales(int idTarea, string idColaboradores)
		{
			try
			{
				//para la actualizacion de los permisos y las tareas
				List<DiaColaborador> listaDiaColaborador = new List<DiaColaborador>();
				//Para la actualizacion de los cambios en IU en los TD de la tabla
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				Tarea task = db.Tarea.Find(idTarea);
				if(task == null)
				{
					throw new Exception("Error, la tarea no es válida.");
				}
				DateTime diaTarea = task.FechaInicio.Date;
				DateTime diaSiguiente = diaTarea.AddDays(1);

				TimeSpan tiempoTarea = task.FechaFin - task.FechaInicio;
				int horasTarea = tiempoTarea.Hours;
				int minutosTarea = tiempoTarea.Minutes;

				if(task.FechaInicio.Hour < 13 && task.FechaFin.Hour > 13)
				{
					horasTarea--;
				}

				var s = new JavaScriptSerializer();
				var arrayIdColaboradores = s.Deserialize<dynamic>(idColaboradores);

				foreach(string stringIdColaborador in arrayIdColaboradores)
				{
					int idColaborador = int.Parse(stringIdColaborador);
					//Buscando la suma de las horas de las tareas del mismo día
					var tareasDelDia = (from t in db.Tarea
										where
											t.SecuencialColaborador == idColaborador &&
											t.FechaInicio >= diaTarea &&
											t.FechaInicio < diaSiguiente &&
											t.SecuencialEstadoTarea != 4
										select new
										{
											finicio = t.FechaInicio,
											ffin = t.FechaFin
										}).ToList();
					int time = 0;
					int timeMinutos = 0;
					foreach(var tarea in tareasDelDia)
					{
						TimeSpan tiempo = tarea.ffin - tarea.finicio;
						time += tiempo.Hours;
						timeMinutos += tiempo.Minutes;
					}

					DateTime fechaInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
					fechaInicio = fechaInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
					DateTime fechaFin = fechaInicio.AddHours(horasTarea).AddMinutes(minutosTarea);

					if(fechaInicio.Hour < 13 && fechaFin.Hour > 13)
					{
						fechaFin = fechaFin.AddHours(1);
					}
					else if(fechaInicio.Hour == 13)
					{
						fechaInicio = fechaInicio.AddHours(1);
						fechaFin = fechaFin.AddHours(1);
					}

					Tarea tar = new Tarea
					{
						SecuencialColaborador = idColaborador,
						SecuencialActividad = task.SecuencialActividad,
						SecuencialModulo = task.SecuencialModulo,
						SecuencialCliente = task.SecuencialCliente,
						SecuencialEstadoTarea = 1,
						SecuencialLugarTarea = task.SecuencialLugarTarea,
						Detalle = task.Detalle,
						FechaInicio = fechaInicio,
						FechaFin = fechaFin,
						HorasUtilizadas = 0,
						NumeroVerificador = 1
					};

					if(task.entregableMotivoTrabajo != null)//Actualizar la referencia
					{
						tar.entregableMotivoTrabajo = task.entregableMotivoTrabajo;
					}

					db.Tarea.Add(tar);
					//Para la actualizacion de las tareas y de los permisos
					listaDiaColaborador.Add(new DiaColaborador { Fecha = tar.FechaInicio.Date, IdColaborador = tar.SecuencialColaborador });
					listaCambiosTareas.Add(new DiaColaborador { Fecha = tar.FechaInicio.Date, IdColaborador = tar.SecuencialColaborador });

					if(task.tarea_coordinador != null)
					{
						db.Tarea_Coordinador.Add(
							new Tarea_Coordinador
							{
								tarea = tar,
								SecuencialColaborador = task.tarea_coordinador.SecuencialColaborador,
								EstaActivo = 1,
								NumeroVerificador = 1
							}
						);
					}

					HistoricoTareaEstado histET = new HistoricoTareaEstado
					{
						tarea = tar,
						SecuencialEstadoTarea = 1,
						FechaOperacion = DateTime.Now,
						usuario = user
					};

					db.HistoricoTareaEstado.Add(histET);
				}

				db.SaveChanges();

				//Actualizando las fechas de los dias que existieron los cambios y en las fechas
				foreach(DiaColaborador diaColab in listaDiaColaborador)
				{
					Utiles.OrdenarTareasPermisos(diaColab.Fecha, diaColab.IdColaborador, user, db);
				}

				//Actualizando cambios en la interfaz de usuario
				ActualizarTDTarea(listaCambiosTareas);

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
		[Authorize(Roles = "ADMIN")]
		public ActionResult EstablecerQuitarTareaCompensatoria(int idTarea)
		{
			try
			{
				Tarea tarea = db.Tarea.Find(idTarea);
				if(tarea == null)
					throw new Exception("Error, no se encontró la tarea.");

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				//Para la actualizacion de los cambios en IU en los TD de la tabla
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();

				//Buscando si la tarea es compensatoria
				TareaCompensatoria tareaCompensatoria = tarea.tareaCompensatoria.FirstOrDefault();
				if(tareaCompensatoria != null)//Ya existe hay que hacer un toggle
				{
					if(tareaCompensatoria.TiempoConsumido == 0)
					{
						tareaCompensatoria.EstaActiva = 1 - tareaCompensatoria.EstaActiva;//toggle
					}
					tareaCompensatoria.usuario = user;
				}
				else
				{
					tareaCompensatoria = new TareaCompensatoria
					{
						tarea = tarea,
						usuario = user,
						TiempoMinutos = (int)((tarea.FechaFin - tarea.FechaInicio).TotalMinutes),
						TiempoConsumido = 0,
						FechaOperacion = DateTime.Now,
						EstaActiva = 1
					};
					db.TareaCompensatoria.Add(tareaCompensatoria);
				}

				listaCambiosTareas.Add(new DiaColaborador { Fecha = tarea.FechaInicio.Date, IdColaborador = tarea.SecuencialColaborador });

				db.SaveChanges();

				//Actualizando cambios en la interfaz de usuario
				ActualizarTDTarea(listaCambiosTareas);

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
		[Authorize(Roles = "ADMIN")]
		public ActionResult DatosDiaTarea(string fecha, int idColaborador, string json = "")
		{
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

			DateTime diaTarea = Utiles.strToDateTime(fecha);
			DateTime diaSiguiente = diaTarea.AddDays(1);
			var datosTareas = (from t in db.Tarea
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
							   join d in db.Departamento on c.departamento equals d
							   where c.Secuencial == idColaborador &&
									 u.EstaActivo == 1 &&
									 d.Asignable == 1 &&
									 t.FechaInicio >= diaTarea && t.FechaInicio < diaSiguiente && //Entre las dos fechas
									 t.SecuencialEstadoTarea != 4//Es cuando están anuladas
							   orderby t.FechaInicio
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
								   coordinador = (from tc in db.Tarea_Coordinador
												  join
													  co in db.Colaborador on tc.colaborador equals co
												  join
													  pe in db.Persona on co.persona equals pe
												  where tc.SecuencialTarea == t.Secuencial && tc.EstaActivo == 1
												  select (pe.Nombre1 + " " + pe.Apellido1)).FirstOrDefault(),
								   compensatoria = (t.tareaCompensatoria.FirstOrDefault() != null
											   && t.tareaCompensatoria.FirstOrDefault().EstaActiva == 1
											  ) ? "compensatoria" : "no-compensatoria"
							   }).ToList();

			//Permisos del colaborador en la fecha
			var permisos = (from per in db.Permiso
							where per.FechaInicio >= diaTarea && per.FechaInicio < diaSiguiente &&
								  per.SecuencialEstadoPermiso != 3 && per.SecuencialEstadoPermiso != 4 && per.SecuencialColaborador == idColaborador
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

			var tareasTD = (from d in datosTareas
							select new DataTarea
							{
								id = d.idTarea,
								idColaborador = d.idColaborador,
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
								tipo = "t",
								compensatoria = d.compensatoria
							}).ToList<DataTarea>();

			var lPermisos = (from per in permisos
							 select new DataTarea
							 {
								 id = per.id,
								 idColaborador = per.idColaborador,
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
								 tipo = "p",
								 compensatoria = "no-compensatoria"
							 }).ToList<DataTarea>();
			tareasTD.AddRange(lPermisos);

			if(idClientes.Count() > 0)
			{
				tareasTD = tareasTD.Where(x => idClientes.Contains(x.idCliente)).ToList();
			}
			if(idEstados.Count() > 0)
			{
				tareasTD = tareasTD.Where(x => idEstados.Contains(x.idEstado)).ToList();
			}
			if(idLugares.Count() > 0)
			{
				tareasTD = tareasTD.Where(x => idLugares.Contains(x.idLugar)).ToList();
			}
			if(idModulos.Count() > 0)
			{
				tareasTD = tareasTD.Where(x => idModulos.Contains(x.idModulo)).ToList();
			}

			tareasTD.Sort();

			try
			{
				var resp = new
				{
					success = true,
					tareasTD = tareasTD
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

		private void ActualizarTDTarea(List<DiaColaborador> lista)
		{
			Websocket instance = Websocket.getInstance();
			List<object> listaDatos = (from l in lista
									   select new
									   {
										   fecha = l.Fecha.ToString("dd/MM/yyyy"),
										   idColaborador = l.IdColaborador
									   }).ToList<object>();
			instance.ActualizarTDTareas(listaDatos);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult DatosNivelColaboradorModulo(string filtro)
		{
			try
			{
				var colaboradores = (from c in db.Colaborador
									 where c.departamento.Asignable == 1 &&
										   c.persona.usuario.FirstOrDefault().EstaActivo == 1
									 select new
									 {
										 idColaborador = c.Secuencial,
										 email = c.persona.usuario.FirstOrDefault().Email
									 }).ToList();

				var modulos = (from m in db.Modulo
							   where m.EstaActivo == 1
							   select new
							   {
								   idModulo = m.Secuencial,
								   cod = m.Codigo,
								   desc = m.Descripcion
							   }).ToList();

				var nivelesModulos = (from nc in db.NivelColaboradorModulo
									  where
										colaboradores.Select(x => x.idColaborador).ToList().Contains(nc.SecuencialColaborador)
									  select new
									  {
										  id = nc.Secuencial,
										  idColaborador = nc.SecuencialColaborador,
										  idModulo = nc.SecuencialModulo,
										  nivel = nc.Nivel
									  }).ToList();



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
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarConocimientosNivelColaboradores(int modulo = 0, int tecnologia = 0, int nivel = 0)
		{
			try
			{
				var competencias = (from ncm in db.NivelColaboradorModulo
									join
  c in db.Colaborador on ncm.colaborador equals c
									join
p in db.Persona on c.persona equals p
									where p.usuario.FirstOrDefault().EstaActivo == 1 &&
									   ncm.Nivel > 0
									select new
									{
										competencia = ncm.modulo.Descripcion,
										colaborador = p.Nombre1 + " " + p.Apellido1,
										nivel = ncm.Nivel,
										idModulo = ncm.SecuencialModulo,
										idTecnologia = 0
									}
								   ).ToList();

				var tecnologias = (from cnt in db.NivelColaboradorTecnologia
								   join c in db.Colaborador on cnt.Colaborador equals c
								   join p in db.Persona on c.persona equals p
								   where p.usuario.FirstOrDefault().EstaActivo == 1 && cnt.Nivel > 0
								   select new
								   {
									   competencia = cnt.TECNOLOGIASYPROCESOS.Codigo,
									   colaborador = p.Nombre1 + " " + p.Apellido1,
									   nivel = cnt.Nivel,
									   idModulo = 0,
									   idTecnologia = cnt.SecuencialTecnologia
								   }).ToList();

				competencias.AddRange(tecnologias);

				if(modulo != 0)
				{
					competencias = (from c in competencias
									where c.idModulo == modulo
									select c).ToList();
				}

				if(tecnologia != 0)
				{
					competencias = (from c in competencias
									where c.idTecnologia == tecnologia
									select c).ToList();
				}

				if(nivel != 0)
				{
					competencias = (from c in competencias
									where c.nivel >= nivel
									select c).ToList();
				}

				var resp = new
				{
					success = true,
					filasConocimientos = competencias
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

		//---------------SOLICITUDES DE LAS TAREAS-------------------
		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarSolicitudesTareas(bool mostrarTodas = false, string filtro = "", int start = 0, int length = 10)
		{
			try
			{
				List<object> solicitudes = new List<object>();
				DateTime ayer = DateTime.Today.AddDays(-1);

				if(!mostrarTodas)
				{
					var solicitudes1 = (from s in db.SolicitudTarea
										where ((s.Aceptada == 0 && s.Rechazada == 0) || s.HoraSolicitud > ayer)
										select new
										{
											id = s.Secuencial,
											idTrabajador = s.SecuencialColaborador,
											idSede = s.colaborador.SecuencialSede,
											sede = s.colaborador.sede.Descripcion,
											usuario = s.colaborador.persona.Nombre1 + " " + s.colaborador.persona.Apellido1,
											fecha = s.HoraSolicitud,
											desc = s.Descripcion.Length > 50 ? s.Descripcion.Substring(0, 50) + "..." : s.Descripcion,
											descripcion = s.Descripcion,
											gestiono = s.gestion_solicitudTarea != null ? (s.gestion_solicitudTarea.usuario.persona.Nombre1 + " " + s.gestion_solicitudTarea.usuario.persona.Apellido1) : "POR GESTIÓN",
											estado = (s.Aceptada == 0 && s.Rechazada == 0) ? "EN ESPERA DE GESTIÓN" : (s.Aceptada == 1) ? "ACEPTADA" : "RECHAZADA",
											gestionable = (s.Aceptada == 0 && s.Rechazada == 0) ? 1 : 0
										}).ToList();

					if(filtro != "")
					{
						solicitudes1 = solicitudes1.Where(x => x.descripcion.ToString().ToLower().Contains(filtro.ToLower()) ||
															   x.gestiono.ToString().ToLower().Contains(filtro.ToLower()) ||
															   x.estado.ToString().ToLower().Contains(filtro.ToLower()) ||
															   x.fecha.ToString().ToLower().Contains(filtro.ToLower()) ||
															   x.usuario.ToString().ToLower().Contains(filtro.ToLower())
														  ).ToList();
					}
					solicitudes = solicitudes1.ToList<object>();
				}
				else
				{
					var solicitudes2 = (from s in db.SolicitudTarea
										select new
										{
											id = s.Secuencial,
											idTrabajador = s.SecuencialColaborador,
											idSede = s.colaborador.SecuencialSede,
											sede = s.colaborador.sede.Descripcion,
											usuario = s.colaborador.persona.Nombre1 + " " + s.colaborador.persona.Apellido1,
											fecha = s.HoraSolicitud,
											desc = s.Descripcion.Length > 50 ? s.Descripcion.Substring(0, 50) + "..." : s.Descripcion,
											descripcion = s.Descripcion,
											gestiono = s.gestion_solicitudTarea != null ? (s.gestion_solicitudTarea.usuario.persona.Nombre1 + " " + s.gestion_solicitudTarea.usuario.persona.Apellido1) : "POR GESTIÓN",
											estado = (s.Aceptada == 0 && s.Rechazada == 0) ? "EN ESPERA DE GESTIÓN" : (s.Aceptada == 1) ? "ACEPTADA" : "RECHAZADA",
											gestionable = (s.Aceptada == 0 && s.Rechazada == 0) ? 1 : 0
										}).ToList();

					if(filtro != "")
					{
						solicitudes2 = solicitudes2.Where(x => x.descripcion.ToString().ToLower().Contains(filtro.ToLower()) ||
															   x.gestiono.ToString().ToLower().Contains(filtro.ToLower()) ||
															   x.estado.ToString().ToLower().Contains(filtro.ToLower()) ||
															   x.fecha.ToString().ToLower().Contains(filtro.ToLower()) ||
															   x.usuario.ToString().ToLower().Contains(filtro.ToLower())
														  ).ToList();
					}
					solicitudes = solicitudes2.ToList<object>();
				}

				int cantidad = solicitudes.Count();
				solicitudes = solicitudes.Skip(start).Take(length).ToList<object>();

				var resp = new
				{
					success = true,
					cantidad = cantidad,
					solicitudes = solicitudes
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult AceptarSolicitudTarea(int idSolicitud)
		{
			try
			{
				SolicitudTarea solicitud = db.SolicitudTarea.Find(idSolicitud);
				if(solicitud == null)
				{
					throw new Exception("No se encontró la solicitud");
				}
				if(solicitud.Rechazada != 0 || solicitud.Aceptada != 0)
				{
					throw new Exception("Ya se ha gestionado la solicitud");
				}

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				solicitud.Aceptada = 1;
				solicitud.NumeroVerificador = solicitud.NumeroVerificador + 1;

				Gestion_SolicitudTarea gestionSolicitud = new Gestion_SolicitudTarea
				{
					solicitudTarea = solicitud,
					HoraGestion = DateTime.Now,
					usuario = user
				};
				db.Gestion_SolicitudTarea.Add(gestionSolicitud);

				db.SaveChanges();

				//Enviando el email de solicitud rechazada
				string personaSolicita = solicitud.colaborador.persona.Nombre1 + " " + solicitud.colaborador.persona.Apellido1;
				string emailSolicita = solicitud.colaborador.persona.usuario.FirstOrDefault().Email;

				string[] emails = new string[] { emailSolicita, emailUser };

				string textoHtml = "<div class=\"textoCuerpo\">Se ha aceptado la solicitud de tarea.<br/>";
				textoHtml += "Solicitud por: <b>" + personaSolicita + "</b><br/>";
				textoHtml += "Detalle de solicitud: <b>" + solicitud.Descripcion + "</b><br/>";
				textoHtml += "Fecha y hora de solicitud: <b>" + solicitud.HoraSolicitud.ToString("dd/MM/yyyy HH:mm:ss") + "</b><br/>";
				textoHtml += "Fecha y hora de la gestión: <b>" + gestionSolicitud.HoraGestion.ToString("dd/MM/yyyy HH:mm:ss") + "<br/></div>";

				Utiles.EnviarEmailSistema(emails, textoHtml, "solicitud de tarea aceptada");

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
		[Authorize(Roles = "ADMIN")]
		public ActionResult NuevaTareasPorSolicitud(int idTrabajador, int idSolicitud, string fecha, int cliente, int ubicacion, int modulo, int actividad, int horas, string detalle, int referencia = 0, int coordinador = 0, string repetir = "", int finSemana = 0, int repetirTipoFin = 0, int numVeces = 0, string fechaHasta = "")
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				SolicitudTarea solicitud = db.SolicitudTarea.Find(idSolicitud);
				if(solicitud == null)
				{
					throw new Exception("No se encontró la solicitud");
				}
				if(solicitud.Rechazada != 0 || solicitud.Aceptada != 0)
				{
					throw new Exception("Ya se ha gestionado la solicitud");
				}

				string[] fechas = fecha.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				DateTime diaTarea = new System.DateTime(anno, mes, dia);
				DateTime diaSiguiente = diaTarea.AddDays(1);
				Tarea tareaPrincipal = null;

				//para la actualizacion de los permisos y las tareas
				List<DiaColaborador> listaDiaColaborador = new List<DiaColaborador>();
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();

				//Buscando la suma de las horas de las tareas del mismo día
				var tareasDelDia = (from t in db.Tarea
									where
										t.SecuencialColaborador == idTrabajador &&
										t.FechaInicio >= diaTarea &&
										t.FechaInicio < diaSiguiente &&
										t.SecuencialEstadoTarea != 4
									select new
									{
										finicio = t.FechaInicio,
										ffin = t.FechaFin
									}).ToList();
				int time = 0;
				int timeMinutos = 0;
				foreach(var tarea in tareasDelDia)
				{
					TimeSpan tiempo = tarea.ffin - tarea.finicio;
					time += tiempo.Hours;
					timeMinutos += tiempo.Minutes;
				}

				DateTime fechaInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
				fechaInicio = fechaInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
				DateTime fechaFin = fechaInicio.AddHours(horas);

				if(fechaInicio.Hour < 13 && fechaFin.Hour > 13)
				{
					fechaFin = fechaFin.AddHours(1);
				}
				else if(fechaInicio.Hour == 13)
				{
					fechaInicio = fechaInicio.AddHours(1);
					fechaFin = fechaFin.AddHours(1);
				}

				Tarea tar = new Tarea
				{
					SecuencialColaborador = idTrabajador,
					SecuencialActividad = actividad,
					SecuencialModulo = modulo,
					SecuencialCliente = cliente,
					SecuencialEstadoTarea = 1,
					SecuencialLugarTarea = ubicacion,
					Detalle = detalle.ToUpper(),
					FechaInicio = fechaInicio,
					FechaFin = fechaFin,
					HorasUtilizadas = 0,
					NumeroVerificador = 1
				};

				if(referencia != 0)//Actualizar la referencia
				{
					if((tar.entregableMotivoTrabajo != null && tar.entregableMotivoTrabajo.Secuencial != referencia) || tar.entregableMotivoTrabajo == null)
					{
						EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
						if(entregable != null)
						{
							tar.entregableMotivoTrabajo = entregable;
						}
					}
				}

				db.Tarea.Add(tar);

				if(coordinador != 0)
				{
					db.Tarea_Coordinador.Add(
						new Tarea_Coordinador
						{
							tarea = tar,
							SecuencialColaborador = coordinador,
							EstaActivo = 1,
							NumeroVerificador = 1
						}
					);
				}

				HistoricoTareaEstado histET = new HistoricoTareaEstado
				{
					tarea = tar,
					SecuencialEstadoTarea = 1,
					FechaOperacion = DateTime.Now,
					usuario = user
				};

				db.HistoricoTareaEstado.Add(histET);

				tareaPrincipal = tar;

				solicitud.Aceptada = 1;
				solicitud.NumeroVerificador = solicitud.NumeroVerificador + 1;

				//GUARDANDO LA HORA EN QUE SE GESTIONÓ
				Gestion_SolicitudTarea gestionSolicitud = new Gestion_SolicitudTarea
				{
					solicitudTarea = solicitud,
					HoraGestion = DateTime.Now,
					usuario = user
				};
				db.Gestion_SolicitudTarea.Add(gestionSolicitud);

				//Adicionando la tarea a la gestion de solicitud
				gestionSolicitud.tarea.Add(tar);

				//Para la actualizacion de las tareas y de los permisos
				listaDiaColaborador.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tar.SecuencialColaborador });
				listaCambiosTareas.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tar.SecuencialColaborador });

				//Ver si se repite la tarea
				if(repetir != "")//Aqui se repite la tarea
				{
					DateTime diaT = diaTarea;

					if(repetirTipoFin == 1)
					{//Veces
						int i = 1;
						while(i < numVeces)
						{

							if(repetir == "SEMANAL")
							{
								diaT = diaT.AddDays(7);
							}
							else if(repetir == "MENSUAL")
							{
								diaT = diaT.AddMonths(1);
							}
							else
							{//Diario
								diaT = diaT.AddDays(1);

								DayOfWeek diaSemana = diaT.DayOfWeek;
								long diaSem = (int)diaSemana;
								if(finSemana == 0)//No se contemplan los fines de semana
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(2);
									}
									else if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 1)//Se incluye el sábado no el domingo
								{
									if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 2)//Se incluye el domingo no el sábado
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(1);
									}
								}
							}

							DateTime diaT2 = diaT.AddDays(1);

							//Buscando la suma de las horas de las tareas del mismo día
							var tareasDia = (from t in db.Tarea
											 where t.SecuencialColaborador == idTrabajador &&
												   t.FechaInicio >= diaT && t.FechaInicio < diaT2 &&
												   t.SecuencialEstadoTarea != 4
											 select new
											 {
												 finicio = t.FechaInicio,
												 ffin = t.FechaFin
											 }).ToList();
							time = 0;
							timeMinutos = 0;
							foreach(var tarea in tareasDia)
							{
								TimeSpan tiempo = tarea.ffin - tarea.finicio;
								time += tiempo.Hours;
								timeMinutos += tiempo.Minutes;
							}

							DateTime fInicio = diaT.AddHours(time).AddMinutes(timeMinutos);
							fInicio = fInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
							DateTime fFin = fInicio.AddHours(horas);

							if(fInicio.Hour < 13 && fFin.Hour > 13)
							{
								fFin = fFin.AddHours(1);
							}
							else if(fInicio.Hour == 13)
							{
								fInicio = fInicio.AddHours(1);
								fFin = fFin.AddHours(1);
							}

							Tarea tar1 = new Tarea
							{
								SecuencialColaborador = idTrabajador,
								SecuencialActividad = actividad,
								SecuencialModulo = modulo,
								SecuencialCliente = cliente,
								SecuencialEstadoTarea = 1,
								SecuencialLugarTarea = ubicacion,
								Detalle = detalle.ToUpper(),
								FechaInicio = fInicio,
								FechaFin = fFin,
								HorasUtilizadas = 0,
								NumeroVerificador = 1
							};

							if(referencia != 0)//Actualizar la referencia
							{
								if((tar1.entregableMotivoTrabajo != null && tar1.entregableMotivoTrabajo.Secuencial != referencia) || tar1.entregableMotivoTrabajo == null)
								{
									EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
									if(entregable != null)
									{
										tar1.entregableMotivoTrabajo = entregable;
									}
								}
							}
							db.Tarea.Add(tar1);

							//Adicionando la tarea a la gestion de solicitud
							gestionSolicitud.tarea.Add(tar1);

							if(coordinador != 0)
							{
								db.Tarea_Coordinador.Add(
									new Tarea_Coordinador
									{
										tarea = tar1,
										SecuencialColaborador = coordinador,
										EstaActivo = 1,
										NumeroVerificador = 1
									}
								);
							}

							HistoricoTareaEstado histET1 = new HistoricoTareaEstado
							{
								tarea = tar1,
								SecuencialEstadoTarea = 1,
								FechaOperacion = DateTime.Now,
								usuario = user
							};

							db.HistoricoTareaEstado.Add(histET1);

							Tarea_TareaRelacionada tareaTReacionada = new Tarea_TareaRelacionada
							{
								tarea1 = tar1,
								tarea = tareaPrincipal,
								NumeroVerificador = 1
							};
							db.Tarea_TareaRelacionada.Add(tareaTReacionada);

							//Para la actualizacion de las tareas y de los permisos
							listaDiaColaborador.Add(new DiaColaborador { Fecha = diaT, IdColaborador = tar1.SecuencialColaborador });
							listaCambiosTareas.Add(new DiaColaborador { Fecha = diaT, IdColaborador = tar1.SecuencialColaborador });

							i++;
						}
					}
					else if(repetirTipoFin == 2)
					{//Fecha

						string[] fechasHasta = fechaHasta.Split(new Char[] { '/' });
						int diaHasta = Int32.Parse(fechasHasta[0]);
						int mesHasta = Int32.Parse(fechasHasta[1]);
						int annoHasta = Int32.Parse(fechasHasta[2]);
						DateTime diaTareaHasta = new System.DateTime(annoHasta, mesHasta, diaHasta);

						while(diaTareaHasta > diaT)
						{
							if(repetir == "SEMANAL")
							{
								diaT = diaT.AddDays(7);
							}
							else if(repetir == "MENSUAL")
							{
								diaT = diaT.AddMonths(1);
							}
							else
							{//Diario
								diaT = diaT.AddDays(1);
								DayOfWeek diaSemana = diaT.DayOfWeek;
								long diaSem = (int)diaSemana;
								if(finSemana == 0)//No se contemplan los fines de semana
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(2);
									}
									else if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 1)//Se incluye el sábado no el domingo
								{
									if(diaSem == 0)//Domingo
									{
										diaT = diaT.AddDays(1);
									}
								}
								else if(finSemana == 2)//Se incluye el domingo no el sábado
								{
									if(diaSem == 6)//Sábado
									{
										diaT = diaT.AddDays(1);
									}
								}
							}
							if(diaTareaHasta < diaT)//Romper por si se pasa
								break;

							DateTime diaT2 = diaT.AddDays(1);

							//Buscando la suma de las horas de las tareas del mismo día
							var tareasDia = (from t in db.Tarea
											 where t.SecuencialColaborador == idTrabajador &&
												   t.FechaInicio >= diaT && t.FechaInicio < diaT2 &&
												   t.SecuencialEstadoTarea != 4
											 select new
											 {
												 finicio = t.FechaInicio,
												 ffin = t.FechaFin
											 }).ToList();
							time = 0;
							timeMinutos = 0;
							foreach(var tarea in tareasDia)
							{
								TimeSpan tiempo = tarea.ffin - tarea.finicio;
								time += tiempo.Hours;
								timeMinutos += tiempo.Minutes;
							}

							DateTime fInicio = diaT.AddHours(time).AddMinutes(timeMinutos);
							fInicio = fInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
							DateTime fFin = fInicio.AddHours(horas);

							if(fInicio.Hour < 13 && fFin.Hour > 13)
							{
								fFin = fFin.AddHours(1);
							}
							else if(fInicio.Hour == 13)
							{
								fInicio = fInicio.AddHours(1);
								fFin = fFin.AddHours(1);
							}

							Tarea tar1 = new Tarea
							{
								SecuencialColaborador = idTrabajador,
								SecuencialActividad = actividad,
								SecuencialModulo = modulo,
								SecuencialCliente = cliente,
								SecuencialEstadoTarea = 1,
								SecuencialLugarTarea = ubicacion,
								Detalle = detalle.ToUpper(),
								FechaInicio = fInicio,
								FechaFin = fFin,
								HorasUtilizadas = 0,
								NumeroVerificador = 1
							};

							if(referencia != 0)//Actualizar la referencia
							{
								if((tar1.entregableMotivoTrabajo != null && tar1.entregableMotivoTrabajo.Secuencial != referencia) || tar1.entregableMotivoTrabajo == null)
								{
									EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(referencia);
									if(entregable != null)
									{
										tar1.entregableMotivoTrabajo = entregable;
									}
								}
							}
							db.Tarea.Add(tar1);

							//Adicionando la tarea a la gestion de solicitud
							gestionSolicitud.tarea.Add(tar1);

							if(coordinador != 0)
							{
								db.Tarea_Coordinador.Add(
									new Tarea_Coordinador
									{
										tarea = tar1,
										SecuencialColaborador = coordinador,
										EstaActivo = 1,
										NumeroVerificador = 1
									}
								);
							}

							HistoricoTareaEstado histET1 = new HistoricoTareaEstado
							{
								tarea = tar1,
								SecuencialEstadoTarea = 1,
								FechaOperacion = DateTime.Now,
								usuario = user
							};

							db.HistoricoTareaEstado.Add(histET1);

							Tarea_TareaRelacionada tareaTReacionada = new Tarea_TareaRelacionada
							{
								tarea1 = tar1,
								tarea = tareaPrincipal,
								NumeroVerificador = 1
							};
							db.Tarea_TareaRelacionada.Add(tareaTReacionada);

							//Para la actualizacion de las tareas y de los permisos
							listaDiaColaborador.Add(new DiaColaborador { Fecha = diaT, IdColaborador = tar1.SecuencialColaborador });
							listaCambiosTareas.Add(new DiaColaborador { Fecha = diaT, IdColaborador = tar1.SecuencialColaborador });
						}
					}
				}

				//Enviando el email de solicitud rechazada
				string personaSolicita = solicitud.colaborador.persona.Nombre1 + " " + solicitud.colaborador.persona.Apellido1;
				string emailSolicita = solicitud.colaborador.persona.usuario.FirstOrDefault().Email;

				string[] emails = new string[] { emailSolicita, emailUser };

				string textoHtml = "<div class=\"textoCuerpo\">Se ha aceptado la solicitud de tarea.<br/>";
				textoHtml += "Solicitud por: <b>" + personaSolicita + "</b><br/>";
				textoHtml += "Detalle de solicitud: <b>" + solicitud.Descripcion + "</b><br/>";
				textoHtml += "Fecha y hora de solicitud: <b>" + solicitud.HoraSolicitud.ToString("dd/MM/yyyy HH:mm:ss") + "</b><br/>";
				textoHtml += "Fecha y hora de la gestión: <b>" + gestionSolicitud.HoraGestion.ToString("dd/MM/yyyy HH:mm:ss") + "<br/></div>";

				Utiles.EnviarEmailSistema(emails, textoHtml, "solicitud de tarea aceptada");

				db.SaveChanges();//Salvando los cambios

				//Actualizando las fechas de los dias que existieron los cambios y en las fechas
				foreach(DiaColaborador diaColab in listaDiaColaborador)
				{
					Utiles.OrdenarTareasPermisos(diaColab.Fecha, diaColab.IdColaborador, user, db);
				}

				//Actualizando la IU en la actualizacion de la tarea                
				ActualizarTDTarea(listaCambiosTareas);

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
		[Authorize(Roles = "ADMIN")]
		public ActionResult RechazarSolicitudTarea(int idSolicitud)
		{
			try
			{
				SolicitudTarea solicitud = db.SolicitudTarea.Find(idSolicitud);
				if(solicitud == null)
				{
					throw new Exception("No se encontró la solicitud");
				}
				if(solicitud.Rechazada != 0 || solicitud.Aceptada != 0)
				{
					throw new Exception("Ya se ha gestionado la solicitud");
				}

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				solicitud.Rechazada = 1;
				solicitud.NumeroVerificador = solicitud.NumeroVerificador + 1;

				Gestion_SolicitudTarea gestionSolicitud = new Gestion_SolicitudTarea
				{
					solicitudTarea = solicitud,
					HoraGestion = DateTime.Now,
					usuario = user
				};
				db.Gestion_SolicitudTarea.Add(gestionSolicitud);

				db.SaveChanges();

				//Enviando el email de solicitud rechazada
				string personaSolicita = solicitud.colaborador.persona.Nombre1 + " " + solicitud.colaborador.persona.Apellido1;
				string emailSolicita = solicitud.colaborador.persona.usuario.FirstOrDefault().Email;

				string[] emails = new string[] { emailSolicita, emailUser };

				string textoHtml = "<div class=\"textoCuerpo\">Se ha rechazado la solicitud de tarea.<br/>";
				textoHtml += "Solicitud por: <b>" + personaSolicita + "</b><br/>";
				textoHtml += "Detalle de solicitud: <b>" + solicitud.Descripcion + "</b><br/>";
				textoHtml += "Fecha y hora de solicitud: <b>" + solicitud.HoraSolicitud.ToString("dd/MM/yyyy HH:mm:ss") + "</b><br/>";
				textoHtml += "Fecha y hora de la gestión: <b>" + gestionSolicitud.HoraGestion.ToString("dd/MM/yyyy HH:mm:ss") + "<br/></div>";

				Utiles.EnviarEmailSistema(emails, textoHtml, "solicitud de tarea rechazada");

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

		//-------------- CONSOLIDACION DE ASIGACIONES----------------
		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult VerConsolidacionTarea(int varSemanas = 0, string filtro = "")
		{
			try
			{
				int cantSemanas = 14;
				DateTime hoy = DateTime.Now.Date;
				DateTime lunes = hoy;
				int diaSemana = (int)hoy.DayOfWeek;
				if(diaSemana == 0)//Domingo
				{
					lunes = hoy.AddDays(-6);
				}
				else
				{
					lunes = hoy.AddDays(-1 * (diaSemana - 1));
				}

				//Desplazandoce en la fecha
				lunes = lunes.AddDays(varSemanas * 7);
				DateTime lunesAux = lunes;//Variable auxiliar para comparaciones relativas

				List<object> semanas = new List<object>();
				for(int i = 0; i < cantSemanas; i++)//Catorce semanas
				{
					DateTime domingo = lunes.AddDays(6);
					List<object> dias = new List<object>();
					for(int j = 0; j < 7; j++)
					{
						dias.Add(new
						{
							fecha = lunes.AddDays(j).ToString("dd/MM/yyyy"),
							mes = lunes.AddDays(j).ToString("MMMM").ToUpper()
						});
					}

					var semana = new
					{
						lunes = lunes.ToString("dd"),
						lunesFecha = lunes.ToString("dd/MM/yyyy"),
						domingo = domingo.ToString("dd"),
						domingoFecha = domingo.ToString("dd/MM/yyyy"),
						dias = dias
					};
					semanas.Add(semana);
					lunes = lunes.AddDays(7);
				}

				//Tipos de motivos de trabajo
				var tiposTrabajo = (from tmt in db.TipoMotivoTrabajo
									where tmt.EstaActivo == 1
									orderby tmt.Secuencial
									select new
									{
										id = tmt.Secuencial,
										nombre = tmt.Codigo.Equals("PENDIENTES") ? "SOPORTE Y MANTENIMIENTO" : tmt.Codigo,
										trabajos = (
											from mt in db.MotivoTrabajo
											where
												(mt.SecuencialTipoMotivoTrabajo == tmt.Secuencial) && mt.EstaActivo == 1 &&
												(
													(mt.FechaInicio >= lunesAux && mt.FechaInicio < lunes) ||
													(mt.FechaInicioPlanificacion >= lunesAux && mt.FechaInicioPlanificacion < lunes) ||
													(mt.FechaFin >= lunesAux && mt.FechaFin < lunes) ||
													(mt.FechaFinPlanificacion >= lunesAux && mt.FechaFinPlanificacion < lunes) ||
													(mt.FechaInicio <= lunesAux && mt.FechaFin >= lunes) ||
													(mt.FechaInicioPlanificacion <= lunesAux && mt.FechaFinPlanificacion >= lunes) ||
													 mt.Avance < 100
												) && mt.Avance < 100
											orderby mt.Secuencial
											select new
											{
												id = mt.Secuencial,
												codigo = mt.Codigo,
												nombre = mt.Nombre,
												desc = mt.Descripcion,
												verificador = mt.NumeroVerificador,
												diaInicial = mt.FechaInicio,
												diaFinal = mt.FechaFin,
												diaInicialP = mt.FechaInicioPlanificacion,
												diaFinalP = mt.FechaFinPlanificacion,
												avance = mt.Avance,
												entregables = (
													from emt in db.EntregableMotivoTrabajo
													where emt.SecuencialMotivoTrabajo == mt.Secuencial && emt.EstaActivo == 1
														  && emt.Avance < 100
													orderby emt.Secuencial
													select new
													{
														id = emt.Secuencial,
														nombre = emt.Nombre,
														desc = emt.Descripcion,
														verificador = emt.NumeroVerificador,
														diaInicial = emt.FechaInicio,
														diaFinal = emt.FechaFin,
														avance = emt.Avance,
														tareas = (
															from t in db.Tarea
															where t.entregableMotivoTrabajo.Secuencial == emt.Secuencial &&
																	t.SecuencialEstadoTarea != 4 &&//Anulada
																	t.colaborador.persona.usuario.FirstOrDefault().EstaActivo == 1 &&
																	t.FechaInicio > lunesAux && t.FechaFin < lunes
															orderby t.FechaInicio
															select new
															{
																id = t.Secuencial,
																fInicio = t.FechaInicio,
																fFin = t.FechaFin,
																idColaborador = t.SecuencialColaborador,
																nombre = t.colaborador.persona.Nombre1,
																apellido = t.colaborador.persona.Apellido1
															}
														).ToList()
													}
												).ToList()
											}
										).ToList()
									}).ToList();

				if(filtro != "")
				{
					tiposTrabajo = tiposTrabajo.Where(
														x => x.nombre.ToLower().Contains(filtro.ToLower()) ||
														x.trabajos.Where(
															y => y.desc.ToLower().Contains(filtro.ToLower()) || y.codigo.ToLower().Contains(filtro.ToLower()) || y.nombre.ToLower().Contains(filtro.ToLower()) ||
															y.entregables.Where(
																z => z.nombre.ToLower().Contains(filtro.ToLower()) || z.desc.ToLower().Contains(filtro.ToLower())
															).Count() > 0
														).Count() > 0

													  ).ToList();

					int i = 0;
					int cantTiposTrabajo = tiposTrabajo.Count;
					while(i < cantTiposTrabajo)
					{
						int j = 0;
						while(j < tiposTrabajo[i].trabajos.Count)
						{
							var trabajo = tiposTrabajo[i].trabajos[j];
							bool encontrado = false;
							if(
								trabajo.desc.ToLower().Contains(filtro.ToLower()) ||
								trabajo.codigo.ToLower().Contains(filtro.ToLower()) ||
								trabajo.nombre.ToLower().Contains(filtro.ToLower())
							)
							{
								encontrado = true;
							}
							else
							{
								encontrado = trabajo.entregables.Where(x => x.nombre.ToLower().Contains(filtro.ToLower()) || x.desc.ToLower().Contains(filtro.ToLower())).Count() > 0;
							}

							if(!encontrado)
							{
								tiposTrabajo[i].trabajos.RemoveAt(j);
							}
							else
							{
								j++;
							}
						}
						i++;
					}

				}

				//Haciendo los cálculos, y convirtiendo los datos
				List<object> listaDatos = new List<object>();
				foreach(var tipoTrabajo in tiposTrabajo)
				{

					List<object> trabajos = new List<object>();

					foreach(var trabajo in tipoTrabajo.trabajos)
					{

						List<object> entregables = new List<object>();

						foreach(var entregable in trabajo.entregables)
						{

							List<object> tareas = new List<object>();
							int cant = entregable.tareas.Count;
							int i = 0;
							int counterSemanas = 0;
							while(counterSemanas < cantSemanas)
							{
								DateTime lunesTarea = lunesAux.AddDays(counterSemanas * 7);
								DateTime lunesTareaProximo = lunesTarea.AddDays(7);

								List<object> objTareasConsolidadas = new List<object>();

								while(i < cant)
								{
									var tarea = entregable.tareas[i];

									if(tarea.fInicio > lunesTarea && tarea.fInicio < lunesTareaProximo)
									{
										int horas = (tarea.fFin - tarea.fInicio).Hours;
										if(tarea.fInicio.Hour < 13 && tarea.fFin.Hour > 13)
										{
											horas--;
										}

										//Insertando los datos de la tarea consolidada en la lista
										bool encontrado = false;
										int pos = 0;
										foreach(var obj in objTareasConsolidadas)
										{
											var auxObj = (dynamic)obj;
											if(auxObj.idColaborador == tarea.idColaborador)
											{
												encontrado = true;
												horas += auxObj.horas;
											}
											if(encontrado)
											{
												break;
											}
											pos++;
										}

										if(encontrado)
										{
											objTareasConsolidadas.RemoveAt(pos);
										}

										var datosTarea = new
										{
											idColaborador = tarea.idColaborador,
											horas = horas,
											iniciales = tarea.nombre.Substring(0, 1) + tarea.apellido.Substring(0, 1),
											nombreCompleto = tarea.nombre + " " + tarea.apellido
										};

										objTareasConsolidadas.Add(
											datosTarea
										);

										i++;
									}
									else
									{//La tarea no está en esa semana, como están ordenadas se pasa a la siguiente                                        
										break;
									}
								}

								tareas.Add(objTareasConsolidadas);
								counterSemanas++;
							}

							DateTime diaFinalEntregable = entregable.diaFinal.AddDays(1);//Para que se calcule hasta el final del día
							int cantDias = (diaFinalEntregable - entregable.diaInicial).Days;
							int diasAvance = (cantDias * entregable.avance) / 100;
							DateTime diaFinalVerde = entregable.diaInicial.AddDays(diasAvance);

							//Comprobando los intervalos para que no se salgan
							int difInicial = 0, difFinalV = 0, difFinal = 0;
							if(diaFinalEntregable < lunes)
							{
								difInicial = (entregable.diaInicial < lunesAux) ? 0 : (entregable.diaInicial - lunesAux).Days;
								difFinalV = (diaFinalVerde < lunesAux) ? 0 : (diaFinalVerde - lunesAux).Days;
								difFinal = (diaFinalEntregable < lunesAux) ? 0 : (diaFinalEntregable - lunesAux).Days;
							}
							else
							{
								difInicial = (entregable.diaInicial < lunesAux) ? 0 : (entregable.diaInicial >= lunes) ? (lunes - lunesAux).Days : (entregable.diaInicial - lunesAux).Days;
								difFinalV = (diaFinalVerde < lunesAux) ? 0 : (diaFinalVerde >= lunes) ? (lunes - lunesAux).Days : (diaFinalVerde - lunesAux).Days;
								difFinal = (diaFinalEntregable < lunesAux) ? 0 : (diaFinalEntregable >= lunes) ? (lunes - lunesAux).Days : (diaFinalEntregable - lunesAux).Days;
							}

							var datoEntregable = new
							{
								id = entregable.id,
								nombre = entregable.nombre,
								desc = entregable.desc,
								verificador = entregable.verificador,
								diaInicial = entregable.diaInicial,
								diaFinal = entregable.diaFinal,

								difInicial = difInicial,
								difFinalV = difFinalV,
								difFinal = difFinal,

								avance = entregable.avance,
								tareas = tareas
							};

							entregables.Add(datoEntregable);
						}

						DateTime diaI = trabajo.diaInicial;
						DateTime diaF = trabajo.diaFinal.AddDays(1);//Para el calculo exacto del día
						if(entregables.Count > 0)
						{
							diaI = trabajo.diaInicialP;
							diaF = trabajo.diaFinalP.AddDays(1);
						}

						int cantDiasTrabajo = (diaF - diaI).Days;
						int diasAvanceTrabajo = (cantDiasTrabajo * (int)trabajo.avance) / 100;
						DateTime diaFinalVerdeTrabajo = diaI.AddDays(diasAvanceTrabajo);

						//Comprobando los intervalos para que no se salgan
						int difInicialT = 0, difFinalVT = 0, difFinalT = 0;
						if(diaF < lunes)
						{
							difInicialT = (diaI < lunesAux) ? 0 : (diaI - lunesAux).Days;
							difFinalVT = (diaFinalVerdeTrabajo < lunesAux) ? 0 : (diaFinalVerdeTrabajo - lunesAux).Days;
							difFinalT = (diaF < lunesAux) ? 0 : (diaF - lunesAux).Days;
						}
						else
						{
							difInicialT = (diaI < lunesAux) ? 0 : (diaI >= lunes) ? (lunes - lunesAux).Days : (diaI - lunesAux).Days;
							difFinalVT = (diaFinalVerdeTrabajo < lunesAux) ? 0 : (diaFinalVerdeTrabajo >= lunes) ? (lunes - lunesAux).Days : (diaFinalVerdeTrabajo - lunesAux).Days;
							difFinalT = (diaF < lunesAux) ? 0 : (diaF >= lunes) ? (lunes - lunesAux).Days : (diaF - lunesAux).Days;
						}

						var datoTrabajo = new
						{
							id = trabajo.id,
							codigo = trabajo.codigo,
							nombre = trabajo.nombre,
							desc = trabajo.desc,
							verificador = trabajo.verificador,
							diaInicial = trabajo.diaInicial,
							diaFinal = trabajo.diaFinal,

							difInicial = difInicialT,
							difFinalV = difFinalVT,
							difFinal = difFinalT,

							entregables = entregables,
							avance = trabajo.avance
						};

						trabajos.Add(datoTrabajo);
					}

					var tipoTrab = new
					{
						id = tipoTrabajo.id,
						nombre = tipoTrabajo.nombre,
						trabajos = trabajos
					};
					listaDatos.Add(tipoTrab);
				}

				var resp = new
				{
					success = true,
					semanas = semanas,
					tiposTrabajo = listaDatos,
					diferenciaDias = (DateTime.Today - lunesAux).Days
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult EliminarAdenda(int idAdenda)
		{
			try
			{
				Adenda adenda = db.Adenda.Find(idAdenda);
				if(adenda == null)
				{
					throw new Exception("Error, no se encontró la adenda.");
				}

				adenda.EstaActivo = 0;
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
					success = true,
					resp = e.Message
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, GESTOR")]
		public ActionResult GuardarSeguimientoContrato(string datosNuevos, string datosEliminar, string datosEditar, int secuencialMotivo)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				var serNuevos = new JavaScriptSerializer();
				var serEliminar = new JavaScriptSerializer();
				var serEditar = new JavaScriptSerializer();
				var jsonObjNuevos = serNuevos.Deserialize<dynamic>(datosNuevos);
				var jsonObjEliminar = serEliminar.Deserialize<dynamic>(datosEliminar);
				var jsonObjEditar = serEditar.Deserialize<dynamic>(datosEditar);

				for(int i = 0; i < jsonObjNuevos.Length; i++)
				{
					MotivoTrabajoSeguimiento motivoTrabajoSeguimiento = new MotivoTrabajoSeguimiento();
					var fechaString = jsonObjNuevos[i]["FechaSeguimiento"].Split('/');
					motivoTrabajoSeguimiento.Fecha = new DateTime(int.Parse(fechaString[2]), int.Parse(fechaString[1]), int.Parse(fechaString[0]));
					motivoTrabajoSeguimiento.Realizado = jsonObjNuevos[i]["Realizado"].ToString();
					motivoTrabajoSeguimiento.PorRealizar = jsonObjNuevos[i]["PorRealizar"].ToString();
					motivoTrabajoSeguimiento.Pendiente = jsonObjNuevos[i]["Pendiente"].ToString();
					motivoTrabajoSeguimiento.SecuencialUsuario = user.Secuencial;
					motivoTrabajoSeguimiento.EstaActivo = 1;
					motivoTrabajoSeguimiento.NumeroVerificador = 1;
					motivoTrabajoSeguimiento.SecuencialMotivoTrabajoInformacionAdicional = secuencialMotivo;
					db.MotivoTrabajoSeguimiento.Add(motivoTrabajoSeguimiento);
				}
				for(int i = 0; i < jsonObjEliminar.Length; i++)
				{
					MotivoTrabajoSeguimiento motivoTrabajoSeguimiento = db.MotivoTrabajoSeguimiento.Find(int.Parse(jsonObjEliminar[i]["Secuencial"].ToString()));
					if(motivoTrabajoSeguimiento != null)
					{
						motivoTrabajoSeguimiento.EstaActivo = 0;
					}
				}
				for(int i = 0; i < jsonObjEditar.Length; i++)
				{
					MotivoTrabajoSeguimiento motivoTrabajoSeguimientoEditar = db.MotivoTrabajoSeguimiento.Find(int.Parse(jsonObjEditar[i]["Secuencial"].ToString()));
					if(motivoTrabajoSeguimientoEditar != null)
					{
						if(motivoTrabajoSeguimientoEditar.NumeroVerificador != int.Parse(jsonObjEditar[i]["NumeroVerificador"].ToString()))
						{
							throw new Exception("Se ha actualizado la entidad por otro usuario, por favor actualice la información e intente nuevamente.");
						}
						else
						{
							var fechaString = jsonObjEditar[i]["FechaSeguimiento"].Split('/');
							motivoTrabajoSeguimientoEditar.Fecha = new DateTime(int.Parse(fechaString[2]), int.Parse(fechaString[1]), int.Parse(fechaString[0]));
							motivoTrabajoSeguimientoEditar.Realizado = jsonObjEditar[i]["Realizado"].ToString();
							motivoTrabajoSeguimientoEditar.PorRealizar = jsonObjEditar[i]["PorRealizar"].ToString();
							motivoTrabajoSeguimientoEditar.Pendiente = jsonObjEditar[i]["Pendiente"].ToString();
							motivoTrabajoSeguimientoEditar.NumeroVerificador += 1;
						}
					}
				}
				db.SaveChanges();
				var resp = new
				{
					success = true,
					msg = "Se han guardado correctamente los Seguimientos"
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
		[Authorize(Roles = "ADMIN, GESTOR, ACTA")]
		public ActionResult GuardarUnSeguimientoContrato(int secuencialMotivo, string pendiente, string enProceso, string realizado, string fecha)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				MotivoTrabajoSeguimiento motivoTrabajoSeguimiento = new MotivoTrabajoSeguimiento();
				var fechaString = fecha.Split('/');
				motivoTrabajoSeguimiento.Fecha = new DateTime(int.Parse(fechaString[2]), int.Parse(fechaString[1]), int.Parse(fechaString[0]));
				motivoTrabajoSeguimiento.Realizado = realizado;
				motivoTrabajoSeguimiento.PorRealizar = enProceso;
				motivoTrabajoSeguimiento.Pendiente = pendiente;
				motivoTrabajoSeguimiento.SecuencialUsuario = user.Secuencial;
				motivoTrabajoSeguimiento.EstaActivo = 1;
				motivoTrabajoSeguimiento.NumeroVerificador = 1;
				motivoTrabajoSeguimiento.SecuencialMotivoTrabajoInformacionAdicional = secuencialMotivo;
				db.MotivoTrabajoSeguimiento.Add(motivoTrabajoSeguimiento);

				db.SaveChanges();
				var resp = new
				{
					success = true,
					msg = "Se ha guardado correctamente el seguimiento"
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
		[Authorize(Roles = "ADMIN, GESTOR")]
		public ActionResult GuardarAdendasContrato(string datosNuevos, string datosEliminar, int secuencialMotivo)
		{
			try
			{
				var serNuevos = new JavaScriptSerializer();
				var jsonObjNuevos = serNuevos.Deserialize<dynamic>(datosNuevos);
				var serEliminar = new JavaScriptSerializer();
				var jsonObjEliminar = serEliminar.Deserialize<dynamic>(datosEliminar);

				for(int i = 0; i < jsonObjNuevos.Length; i++)
				{
					Adenda adenda = new Adenda();
					var fechaString = jsonObjNuevos[i]["FechaVencimiento"].Split('/');
					adenda.FechaVencimiento = new DateTime(int.Parse(fechaString[2]), int.Parse(fechaString[1]), int.Parse(fechaString[0]));
					adenda.Codigo = jsonObjNuevos[i]["Codigo"].ToString();
					adenda.LinkOpenKM = jsonObjNuevos[i]["LinkOpenKM"].ToString();
					adenda.EstaActivo = 1;
					adenda.NumeroVerificador = 1;
					adenda.SecuencialMotivoTrabajoInformacionAdicional = secuencialMotivo;
					db.Adenda.Add(adenda);
				}
				for(int i = 0; i < jsonObjEliminar.Length; i++)
				{
					Adenda adenda = db.Adenda.Find(int.Parse(jsonObjEliminar[i]["Secuencial"].ToString()));
					if(adenda != null)
					{
						adenda.EstaActivo = 0;
					}
				}
				db.SaveChanges();
				var resp = new
				{
					success = true,
					msg = "Se han guardado correctamente las Adendas"
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
		[Authorize(Roles = "ADMIN, GESTOR")]
		public ActionResult GuardarMotivoTrabajoInformacionAdicional(bool tieneCronograma, bool pagado, int secuencialFase, int estimacion, int estadoContrato, bool aceptaNormativos, string linkOpenKm, string fechaActa, string fechaProduccion, int diasGarantia, int secuencialMotivoTrabajo = 0, int colaborador = 0, int numeroVerificador = 0)
		{
			try
			{
				string msg = "Se ha ingresado correctamente la nueva información";

				MotivoTrabajoInformacionAdicional motivoDetalle = db.MotivoTrabajoInformacionAdicional.Find(secuencialMotivoTrabajo);
				MotivoTrabajo motivoTrabajo = db.MotivoTrabajo.Find(secuencialMotivoTrabajo);
				var estado = db.EstadoContrato.Where(s => s.Secuencial == estadoContrato).FirstOrDefault();
				if(estado != null)
					motivoTrabajo.SecuencialEstadoContrato = estado.Secuencial;

				if(motivoDetalle == null)
				{
					if(secuencialFase == 0 || secuencialMotivoTrabajo == 0)
					{
						throw new Exception("Debe escoger una Fase para el Contrato");
					}
					MotivoTrabajoInformacionAdicional nuevoMotivoDetalle = new MotivoTrabajoInformacionAdicional();
					nuevoMotivoDetalle.SecuencialMotivoTrabajo = secuencialMotivoTrabajo;
					nuevoMotivoDetalle.TieneCronograma = tieneCronograma ? 1 : 0;
					nuevoMotivoDetalle.PagadoTotalmente = pagado ? 1 : 0;
					nuevoMotivoDetalle.SecuencialFase = secuencialFase;
					nuevoMotivoDetalle.AceptaCambiosNormativos = aceptaNormativos ? 1 : 0;
					nuevoMotivoDetalle.EstaActivo = 1;
					nuevoMotivoDetalle.NumeroVerificador = 1;
					nuevoMotivoDetalle.LinkOpenKm = linkOpenKm;
					nuevoMotivoDetalle.Estimacion = estimacion;
					if(fechaActa != "")
					{
						string[] fechas = fechaActa.Split(new Char[] { '/' });
						int dia = Int32.Parse(fechas[0]);
						int mes = Int32.Parse(fechas[1]);
						int anno = Int32.Parse(fechas[2]);
						DateTime fActa = new System.DateTime(anno, mes, dia);
						nuevoMotivoDetalle.FechaActa = fActa;
					}
					if(fechaProduccion != "")
					{
						string[] fechas = fechaProduccion.Split(new Char[] { '/' });
						int dia = Int32.Parse(fechas[0]);
						int mes = Int32.Parse(fechas[1]);
						int anno = Int32.Parse(fechas[2]);
						DateTime fProduccion = new System.DateTime(anno, mes, dia);
						nuevoMotivoDetalle.FechaProduccion = fProduccion;
					}
					nuevoMotivoDetalle.DiasGarantia = diasGarantia;

					db.MotivoTrabajoInformacionAdicional.Add(nuevoMotivoDetalle);
				}
				else
				{
					if(motivoDetalle.NumeroVerificador != numeroVerificador)
					{
						throw new Exception("Se ha actualizado la entidad por otro usuario, por favor actualice la información e intente nuevamente.");
					}

					motivoDetalle.TieneCronograma = tieneCronograma ? 1 : 0;
					motivoDetalle.PagadoTotalmente = pagado ? 1 : 0;
					motivoDetalle.SecuencialFase = secuencialFase;
					motivoDetalle.AceptaCambiosNormativos = aceptaNormativos ? 1 : 0;
					motivoDetalle.NumeroVerificador = motivoDetalle.NumeroVerificador + 1;
					motivoDetalle.Estimacion = estimacion;
					motivoDetalle.LinkOpenKm = linkOpenKm;
					if(fechaActa != "")
					{
						string[] fechas = fechaActa.Split(new Char[] { '/' });
						int dia = Int32.Parse(fechas[0]);
						int mes = Int32.Parse(fechas[1]);
						int anno = Int32.Parse(fechas[2]);
						DateTime fActa = new System.DateTime(anno, mes, dia);
						motivoDetalle.FechaActa = fActa;
					}
					else
					{
						motivoDetalle.FechaActa = new DateTime(0001, 01, 01);
					}
					if(fechaProduccion != "")
					{
						string[] fechas = fechaProduccion.Split(new Char[] { '/' });
						int dia = Int32.Parse(fechas[0]);
						int mes = Int32.Parse(fechas[1]);
						int anno = Int32.Parse(fechas[2]);
						DateTime fProduccion = new System.DateTime(anno, mes, dia);
						motivoDetalle.FechaProduccion = fProduccion;
					}
					else
					{
						motivoDetalle.FechaProduccion = new DateTime(0001, 01, 01);
					}
					motivoDetalle.DiasGarantia = diasGarantia;

					msg = "Se ha actualizado correctamente la Información del Trabajo";
				}
				if(colaborador != 0)
				{
					motivoTrabajo.SecuencialColaborador = colaborador;
				}

				db.SaveChanges();

				var resp = new
				{
					success = true,
					msg = msg
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult NuevoMotivoTrabajo(int tipo, string codigo, int cliente, string nombre, string descripcion, string fechaInicio, string fechaFin, int horasMes, string nombreTipoMotivoTrabajo, HttpPostedFileBase[] adjuntos = null, int estadoContrato = 0, int colaborador = 0, int tipoPlazo = 0, int diasPlazo = 0, int idMotivoTrabajo = 0, int numeroVerificador = 0)
		{
			try
			{
				string[] fechas = fechaInicio.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				DateTime fInicio = new System.DateTime(anno, mes, dia);

				fechas = fechaFin.Split(new Char[] { '/' });
				dia = Int32.Parse(fechas[0]);
				mes = Int32.Parse(fechas[1]);
				anno = Int32.Parse(fechas[2]);
				DateTime fFin = new System.DateTime(anno, mes, dia);

				if(fFin < fInicio)
				{
					throw new Exception("La fecha de fin del trabajo no puede ser menor que la fecha de inicio del mismo.");
				}

				string msg = "Se ha ingresado correctamente el nuevo trabajo";

				if(idMotivoTrabajo != 0)//Actualización
				{
					MotivoTrabajo motivo = db.MotivoTrabajo.Find(idMotivoTrabajo);
					if(motivo == null)
					{
						throw new Exception("Error, No se encontró el Trabajo");
					}
					if(motivo.NumeroVerificador != numeroVerificador)
					{
						throw new Exception("Se ha actualizado la entidad por otro usuario, por favor actualice la información e intente nuevamente.");
					}

					motivo.SecuencialTipoMotivoTrabajo = tipo;
					motivo.Codigo = codigo;
					motivo.SecuencialCliente = cliente;
					if(estadoContrato != 0)
					{
						motivo.SecuencialEstadoContrato = estadoContrato;
					}
					if(colaborador != 0)
					{
						motivo.SecuencialColaborador = colaborador;
					}
					else
					{
						motivo.SecuencialColaborador = null;
					}
					if(tipoPlazo != 0)
					{
						motivo.SecuencialTipoPlazo = tipoPlazo;
					}
					if(diasPlazo != 0)
					{
						motivo.Plazo = diasPlazo;
					}
					if(nombreTipoMotivoTrabajo == "SOPORTE Y MANTENIMIENTO")
					{
						motivo.HorasMes = horasMes;
					}
					motivo.Nombre = nombre;
					motivo.Descripcion = descripcion;
					motivo.FechaInicio = fInicio;
					motivo.FechaFin = fFin;
					motivo.NumeroVerificador = motivo.NumeroVerificador + 1;

					//Guardando los adjuntos
					if(adjuntos != null)
					{
						foreach(HttpPostedFileBase adj in adjuntos)
						{
							if(adj != null)
							{
								string extFile = Path.GetExtension(adj.FileName);
								string newNameFile = Utiles.RandomString(10) + extFile;
								string path = Path.Combine(Server.MapPath("~/Web/resources/contracts"), newNameFile);
								adj.SaveAs(path);

								AdjuntoContrato contratoAdj = new AdjuntoContrato();
								contratoAdj.Url = "/resources/contracts/" + newNameFile;
								contratoAdj.SecuencialContrato = motivo.Secuencial;
								contratoAdj.Descripcion = adj.FileName;

								db.AdjuntoContrato.Add(contratoAdj);
							}
						}
					}

					msg = "Se ha actualizado correctamente el nuevo trabajo";
				}
				else//Nuevo ingreso de motivo de trabajo
				{
					MotivoTrabajo newMotivo = new MotivoTrabajo();
					newMotivo.SecuencialTipoMotivoTrabajo = tipo;
					newMotivo.Codigo = codigo;
					newMotivo.SecuencialCliente = cliente;
					newMotivo.Nombre = nombre;
					newMotivo.Descripcion = descripcion;
					newMotivo.FechaInicio = fInicio;
					newMotivo.FechaFin = fFin;
					newMotivo.FechaInicioPlanificacion = new System.DateTime(9999, 12, 31);
					newMotivo.FechaFinPlanificacion = new System.DateTime(1, 1, 1);
					newMotivo.Avance = 0;
					newMotivo.EstaActivo = 1;
					newMotivo.NumeroVerificador = 1;
					if(estadoContrato != 0)
					{
						newMotivo.SecuencialEstadoContrato = estadoContrato;
					}
					if(colaborador != 0)
					{
						newMotivo.SecuencialColaborador = colaborador;
					}
					if(tipoPlazo != 0)
					{
						newMotivo.SecuencialTipoPlazo = tipoPlazo;
					}
					if(diasPlazo != 0)
					{
						newMotivo.Plazo = diasPlazo;
					}
					if(nombreTipoMotivoTrabajo == "SOPORTE Y MANTENIMIENTO")
					{
						newMotivo.HorasMes = horasMes;
					}

					db.MotivoTrabajo.Add(newMotivo);

					//Guardando los adjuntos
					if(adjuntos != null)
					{
						foreach(HttpPostedFileBase adj in adjuntos)
						{
							if(adj != null)
							{
								string extFile = Path.GetExtension(adj.FileName);
								string newNameFile = Utiles.RandomString(10) + extFile;
								string path = Path.Combine(Server.MapPath("~/Web/resources/contracts"), newNameFile);
								adj.SaveAs(path);

								AdjuntoContrato contratoAdj = new AdjuntoContrato();
								contratoAdj.Url = "/resources/contracts/" + newNameFile;
								contratoAdj.SecuencialContrato = newMotivo.Secuencial;
								contratoAdj.Descripcion = adj.FileName;

								db.AdjuntoContrato.Add(contratoAdj);
							}
						}
					}
				}

				db.SaveChanges();

				var resp = new
				{
					success = true,
					msg = msg
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult EliminarAdjuntoContrato(int idAdjunto)
		{
			try
			{
				string msg = "Operacion realizada correctamente";
				bool success = true;
				var contratoAdj = db.AdjuntoContrato.Find(idAdjunto);
				List<object> adjuntos = new List<object>();
				if(contratoAdj != null)
				{
					string file = Server.MapPath("~/Web/" + contratoAdj.Url);
					//var file = Path.Combine( path1, contratoAdj.Url);
					if(System.IO.File.Exists(file))
					{
						System.IO.File.Delete(file);
					}
					else
					{
						msg = "No se encontró el fichero en el sistema";
					}

					int idContrato = contratoAdj.SecuencialContrato;

					db.AdjuntoContrato.Remove(contratoAdj);
					db.SaveChanges();

					adjuntos = (from cAdj in db.AdjuntoContrato
								where cAdj.SecuencialContrato == idContrato
								select new
								{
									idAdj = cAdj.Secuencial,
									desc = cAdj.Descripcion,
									url = cAdj.Url
								}).ToList<object>();
				}
				else
				{
					msg = "No se encontró adjunto en la base de datos";
					success = false;
				}

				var resp = new
				{
					success = success,
					msg = msg,
					adjuntos = adjuntos
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult NuevoEntregableMotivoTrabajo(int motivo, string nombre, string descripcion, string fechaInicio, string fechaFin, string fechaProduccionEntregable, int avance = 0, int colaboradorMotivoTrabajo = 0, int idEntregable = 0, int numeroVerificador = 0)
		{
			try
			{
				string[] fechas = fechaInicio.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				DateTime fInicio = new System.DateTime(anno, mes, dia);

				fechas = fechaFin.Split(new Char[] { '/' });
				dia = Int32.Parse(fechas[0]);
				mes = Int32.Parse(fechas[1]);
				anno = Int32.Parse(fechas[2]);
				DateTime fFin = new System.DateTime(anno, mes, dia);

				if(fFin < fInicio)
				{
					throw new Exception("La fecha de fin del trabajo no puede ser menor que la fecha de inicio del mismo.");
				}

				if(idEntregable != 0)//Actualización
				{
					EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(idEntregable);
					if(entregable == null)
					{
						throw new Exception("Error, no se encontró el Trabajo");
					}
					if(entregable.NumeroVerificador != numeroVerificador)
					{
						throw new Exception("Se ha actualizado la entidad por otro usuario, por favor actualice la información e intente nuevamente.");
					}

					entregable.SecuencialMotivoTrabajo = motivo;
					entregable.Nombre = nombre;
					entregable.Descripcion = descripcion;
					entregable.FechaInicio = fInicio;
					entregable.FechaFin = fFin;
					entregable.Avance = avance;

					if(fechaProduccionEntregable != null && fechaProduccionEntregable != "")
					{
						fechas = fechaProduccionEntregable.Split(new Char[] { '/' });
						dia = Int32.Parse(fechas[0]);
						mes = Int32.Parse(fechas[1]);
						anno = Int32.Parse(fechas[2]);
						DateTime fProduccion = new System.DateTime(anno, mes, dia);
						entregable.FechaProduccion = fProduccion;
					}

					if(colaboradorMotivoTrabajo != 0)
					{
						entregable.SecuencialColaborador = colaboradorMotivoTrabajo;
					}
					else
					{
						entregable.SecuencialColaborador = null;
					}
					entregable.NumeroVerificador = entregable.NumeroVerificador + 1;
				}
				else//Nuevo ingreso de motivo de trabajo
				{
					EntregableMotivoTrabajo newEntregable = new EntregableMotivoTrabajo();
					newEntregable.SecuencialMotivoTrabajo = motivo;
					newEntregable.Nombre = nombre;
					newEntregable.Descripcion = descripcion;
					newEntregable.FechaInicio = fInicio;
					newEntregable.FechaFin = fFin;
					newEntregable.Avance = avance;
					if(fechaProduccionEntregable != null && fechaProduccionEntregable != "")
					{
						fechas = fechaProduccionEntregable.Split(new Char[] { '/' });
						dia = Int32.Parse(fechas[0]);
						mes = Int32.Parse(fechas[1]);
						anno = Int32.Parse(fechas[2]);
						DateTime fProduccion = new System.DateTime(anno, mes, dia);
						newEntregable.FechaProduccion = fProduccion;
					}
					if(colaboradorMotivoTrabajo != 0)
					{
						newEntregable.SecuencialColaborador = colaboradorMotivoTrabajo;
					}
					newEntregable.EstaActivo = 1;
					newEntregable.NumeroVerificador = 1;
					db.EntregableMotivoTrabajo.Add(newEntregable);
				}

				MotivoTrabajo motivoTrabajo = db.MotivoTrabajo.Find(motivo);
				if(motivoTrabajo == null)
				{
					throw new Exception("Error, no se encontró el motivo de trabajo.");
				}

				//Actualizando las fechas de planificacion
				if(motivoTrabajo.FechaInicioPlanificacion > fInicio)
				{
					motivoTrabajo.FechaInicioPlanificacion = fInicio;
				}
				if(motivoTrabajo.FechaFinPlanificacion < fFin)
				{
					motivoTrabajo.FechaFinPlanificacion = fFin;
				}
				db.SaveChanges();

				//Actualizando los avances
				var relacionesAvance = (from emt in db.EntregableMotivoTrabajo
										where emt.SecuencialMotivoTrabajo == motivoTrabajo.Secuencial && emt.EstaActivo == 1
										select new
										{
											diaInicial = emt.FechaInicio,
											diaFinal = emt.FechaFin,
											avance = emt.Avance
										}).ToList();

				int totalDias = 0;
				double totalDiasAvance = 0;
				foreach(var relacion in relacionesAvance)
				{
					int tdias = (relacion.diaFinal - relacion.diaInicial).Days;
					totalDias += tdias;
					totalDiasAvance += (tdias * (double)relacion.avance / 100);
				}
				int porciento = (int)Math.Ceiling(Math.Round(totalDiasAvance * 100 / totalDias, 2));

				motivoTrabajo.Avance = porciento;
				db.SaveChanges();

				var resp = new
				{
					success = true,
					msg = "Se ha ingresado correctamente el entregable"
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult GuardarListaEntregables(string motivo = "", string entregables = "")
		{
			var s = new JavaScriptSerializer();
			var jsonEntregables = s.Deserialize<dynamic>(entregables);

			try
			{
				var motivoTrabajo = db.MotivoTrabajo.Where(m => m.Codigo == motivo && m.EstaActivo == 1).FirstOrDefault();
				if(motivoTrabajo != null && jsonEntregables != null)
				{
					if(motivoTrabajo.entregableMotivoTrabajo.Count > 0)
					{
						foreach(var e in jsonEntregables)
						{
							bool flag = true;
							string descripcion = (string)e["descripcion"];
							string nombre = (string)e["nombre"];
							foreach(var ent in motivoTrabajo.entregableMotivoTrabajo)
							{
								if(ent.Nombre == descripcion) { flag = false; }
							}
							if(flag)
							{
								EntregableMotivoTrabajo newEntregable = new EntregableMotivoTrabajo();
								newEntregable.SecuencialMotivoTrabajo = motivoTrabajo.Secuencial;
								newEntregable.Nombre = descripcion;
								newEntregable.Descripcion = nombre;
								newEntregable.FechaInicio = motivoTrabajo.FechaInicio;
								newEntregable.FechaFin = motivoTrabajo.FechaFin;
								newEntregable.Avance = 0;
								newEntregable.EstaActivo = 1;
								newEntregable.NumeroVerificador = 1;
								db.EntregableMotivoTrabajo.Add(newEntregable);
								db.SaveChanges();
							}
						}

					}
					else
					{
						foreach(var e in jsonEntregables)
						{
							string descripcion = (string)e["descripcion"];
							string nombre = (string)e["nombre"];

							EntregableMotivoTrabajo newEntregable = new EntregableMotivoTrabajo();
							newEntregable.SecuencialMotivoTrabajo = motivoTrabajo.Secuencial;
							newEntregable.Nombre = descripcion;
							newEntregable.Descripcion = nombre;
							newEntregable.FechaInicio = motivoTrabajo.FechaInicio;
							newEntregable.FechaFin = motivoTrabajo.FechaFin;
							newEntregable.Avance = 0;
							newEntregable.EstaActivo = 1;
							newEntregable.NumeroVerificador = 1;
							db.EntregableMotivoTrabajo.Add(newEntregable);
							db.SaveChanges();
						}
					}

					var relacionesAvance = (from emt in db.EntregableMotivoTrabajo
											where emt.SecuencialMotivoTrabajo == motivoTrabajo.Secuencial && emt.EstaActivo == 1
											select new
											{
												diaInicial = emt.FechaInicio,
												diaFinal = emt.FechaFin,
												avance = emt.Avance
											}).ToList();

					int totalDias = 0;
					double totalDiasAvance = 0;
					foreach(var relacion in relacionesAvance)
					{
						int tdias = (relacion.diaFinal - relacion.diaInicial).Days;
						totalDias += tdias;
						totalDiasAvance += (tdias * (double)relacion.avance / 100);
					}
					int porciento = (int)Math.Ceiling(Math.Round(totalDiasAvance * 100 / totalDias, 2));

					motivoTrabajo.Avance = porciento;
					db.SaveChanges();
				}
				else
				{
					throw new Exception("Debe seleccionar al menos 1 entregable para añadir");
				}

				var resp = new
				{
					success = true,
					msg = "Se han ingresado correctamente los entregables"
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
		[Authorize(Roles = "ADMIN, REC, ACTAS, GESTOR")]
		public ActionResult ObtenerContrato(string numeroContrato)
		{
			try
			{

				var datosContrato = (from mt in db.MotivoTrabajo
									 join c in db.Cliente on mt.SecuencialCliente equals c.Secuencial
									 where mt.Codigo == numeroContrato && mt.EstaActivo == 1
									 select new
									 {
										 numero = numeroContrato,
										 cliente = c.Descripcion,
										 asunto = mt.Nombre
									 }).ToList();

				var resp = new
				{
					success = true,
					datos = datosContrato
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
		[Authorize(Roles = "ADMIN, REC, ACTAS")]
		public ActionResult EliminarActa(int secuencialActa)
		{
			try
			{
				Type typeAccesoDatos = db.GetType();
				PropertyInfo propertyTabla = typeAccesoDatos.GetProperty("Acta");
				MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
				object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
				Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

				var actaEliminar = db.Acta.Where(x => x.Secuencial == secuencialActa).ToList()[0];

				MethodInfo metodoRemove = typePropertyTabla.GetMethod("Remove");
				object newObj = metodoRemove.Invoke(dbSetTable, new object[1] { actaEliminar });
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


		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, ACTAS")]
		public ActionResult DarCodigoActa(string numeroContrato)
		{
			try
			{

				var ultimaActa = (from ac in db.Acta
								  join mt in db.MotivoTrabajo on ac.SecuencialContrato equals mt.Secuencial
								  where mt.Codigo == numeroContrato
								  orderby ac.Codigo descending
								  select ac.Codigo).FirstOrDefault();

				var codigoActa = "";

				if(ultimaActa != null)
				{
					var ultimaActaSplit = ultimaActa.Split('-');
					int numeroActa = int.Parse(ultimaActaSplit.Last()) + 1;
					if(numeroActa < 10)
						codigoActa = "0" + numeroActa;
					else
						codigoActa = numeroActa + "";
				}
				else
				{
					codigoActa = "01";
				}

				return Json(new
				{
					success = true,
					numeroActa = codigoActa
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
		[Authorize(Roles = "ADMIN, REC, ACTAS")]
		public ActionResult GuardarActa(string datos)
		{
			try
			{
				var s = new JavaScriptSerializer();
				var jsonObj = s.Deserialize<dynamic>(datos);

				int id = jsonObj["id"];
				string cliente = (string)jsonObj["cliente"];
				string colaborador = (string)jsonObj["colaborador"];
				string estado = (string)jsonObj["estado"];
				string tipo = (string)jsonObj["tipo"];
				string numeroContrato = (string)jsonObj["numeroContrato"];
				string fecha = (string)jsonObj["fechaFirma"];
				string linkOpenkm = (string)jsonObj["linkOpenkm"];
				int diasFacturacion = (int)jsonObj["diasFacturacion"];
				string codigoActa = "";
				int entregable = Convert.ToInt32(jsonObj["entregable"]);


				if(id == 0)
					codigoActa = numeroContrato + "-" + (string)jsonObj["numeroActa"];
				else
					codigoActa = (string)jsonObj["numeroActa"];

				var fechaString = fecha.Split('/');
				DateTime fechaFirma = new DateTime(int.Parse(fechaString[2]), int.Parse(fechaString[1]), int.Parse(fechaString[0]));

				int secuencialCliente = (from cli in db.Cliente
										 where cli.Descripcion == cliente
										 select cli.Secuencial).ToList()[0];

				int secuencialColaborador = (from co in db.Colaborador
											 join pe in db.Persona on co.SecuencialPersona equals pe.Secuencial
											 where pe.Nombre1 + " " + pe.Apellido1 == colaborador
											 select co.Secuencial).ToList()[0];

				int secuencialEstado = (from ea in db.EstadoActa
										where ea.Descripcion == estado
										select ea.Secuencial).ToList()[0];

				int secuencialTipo = (from ta in db.TipoActa
									  where ta.Descripcion == tipo
									  select ta.Secuencial).ToList()[0];

				int secuencialContrato = (from mt in db.MotivoTrabajo
										  where /*mt.SecuencialTipoMotivoTrabajo == 2 &&*/ mt.Codigo == numeroContrato
										  select mt.Secuencial).ToList()[0];

				//Una vez definidos los datos del acta se procede a guardar
				Type typeAccesoDatos = db.GetType();
				PropertyInfo propertyTabla = typeAccesoDatos.GetProperty("Acta");
				MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
				object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
				Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

				Type typeNewObj = Type.GetType("SifizPlanning.Models.Acta");
				object newObj = null;

				if(id == 0)
				{
					newObj = Activator.CreateInstance(typeNewObj);
				}
				else
				{
					MethodInfo metodoFind = typePropertyTabla.GetMethod("Find");
					object[] pId = new object[1] { new object[1] { id } };
					newObj = metodoFind.Invoke(dbSetTable, pId);
				}


				typeNewObj.GetProperty("SecuencialCliente").SetValue(newObj, secuencialCliente);
				typeNewObj.GetProperty("SecuencialColaborador").SetValue(newObj, secuencialColaborador);
				typeNewObj.GetProperty("SecuencialContrato").SetValue(newObj, secuencialContrato);
				typeNewObj.GetProperty("SecuencialEstado").SetValue(newObj, secuencialEstado);
				typeNewObj.GetProperty("SecuencialTipo").SetValue(newObj, secuencialTipo);
				typeNewObj.GetProperty("FechaFirma").SetValue(newObj, fechaFirma);
				typeNewObj.GetProperty("Codigo").SetValue(newObj, codigoActa);
				typeNewObj.GetProperty("LinkOpenkm").SetValue(newObj, linkOpenkm);
				typeNewObj.GetProperty("DiasFacturacion").SetValue(newObj, diasFacturacion);
				typeNewObj.GetProperty("Fecha").SetValue(newObj, DateTime.Now);
				if(entregable != 0)
				{
					typeNewObj.GetProperty("SecuencialEntregable").SetValue(newObj, entregable);
				}
				typeNewObj.GetProperty("EstaActivo").SetValue(newObj, decimal.Parse("1"));
				typeNewObj.GetProperty("NumeroVerificador").SetValue(newObj, 0);

				if(id == 0)
				{
					MethodInfo metodoAdd = typePropertyTabla.GetMethod("Add");
					metodoAdd.Invoke(dbSetTable, new object[] { newObj });
				}

				if(id == 0)
				{
					string emailUser = User.Identity.Name;
					Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

					MotivoTrabajoSeguimiento motivoTrabajoSeguimiento = new MotivoTrabajoSeguimiento();
					motivoTrabajoSeguimiento.Fecha = DateTime.Now;
					motivoTrabajoSeguimiento.Realizado = "";
					motivoTrabajoSeguimiento.PorRealizar = "Se agregó el Acta: " + codigoActa + " de tipo: " + tipo + ", en estado: " + estado;
					motivoTrabajoSeguimiento.Pendiente = "";
					motivoTrabajoSeguimiento.SecuencialUsuario = user.Secuencial;
					motivoTrabajoSeguimiento.EstaActivo = 1;
					motivoTrabajoSeguimiento.NumeroVerificador = 1;
					motivoTrabajoSeguimiento.SecuencialMotivoTrabajoInformacionAdicional = secuencialContrato;
					db.MotivoTrabajoSeguimiento.Add(motivoTrabajoSeguimiento);
				}


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

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, ACTAS")]
		public ActionResult ObtenerDatosActa(int secuencialActa)
		{

			try
			{
				var datosActa = (from ac in db.Acta
								 join mt in db.MotivoTrabajo on ac.SecuencialContrato equals mt.Secuencial
								 join cl in db.Cliente on ac.SecuencialCliente equals cl.Secuencial
								 join co in db.Colaborador on ac.SecuencialColaborador equals co.Secuencial
								 join ta in db.TipoActa on ac.SecuencialTipo equals ta.Secuencial
								 join ea in db.EstadoActa on ac.SecuencialEstado equals ea.Secuencial
								 join pe in db.Persona on co.SecuencialPersona equals pe.Secuencial
								 where ac.Secuencial == secuencialActa
								 select new
								 {
									 secuencial = ac.Secuencial,
									 cliente = cl.Descripcion,
									 asunto = mt.Nombre,
									 colaborador = pe.Nombre1 + " " + pe.Apellido1,
									 tipo = ta.Descripcion,
									 estado = ea.Descripcion,
									 codigo = ac.Codigo,
									 numeroContrato = mt.Codigo,
									 linkopenkm = ac.LinkOpenkm,
									 entregable = ac.SecuencialEntregable,
									 fecha = ac.Fecha.HasValue ? ac.Fecha : (DateTime?)null,
									 diasFacturacion = ac.DiasFacturacion.HasValue ? ac.DiasFacturacion : 0
								 }).ToList();

				var fechaDate = (from ac in db.Acta
								 where ac.Secuencial == secuencialActa
								 select ac.FechaFirma).ToList();

				string fecha = fechaDate[0].Day + "/" + fechaDate[0].Month + "/" + fechaDate[0].Year;

				return Json(new
				{
					fechaFirma = fecha,
					success = true,
					datos = datosActa
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
		[Authorize(Roles = "ADMIN, REC, ACTAS")]
		public ActionResult ComprobarContrato(string codigoContrato)
		{
			try
			{
				string tipoMotivoTrabajo = db.MotivoTrabajo.Where(c => c.Codigo == codigoContrato && c.EstaActivo == 1).FirstOrDefault().tipoMotivoTrabajo.Codigo;

				bool esProyecto = false;
				if(tipoMotivoTrabajo == "PROYECTOS" || tipoMotivoTrabajo == "CONTRATOS")
				{
					esProyecto = true;
				}

				return Json(new
				{
					success = true,
					esProyecto = esProyecto
				});
			}
			catch(Exception)
			{
				return Json(new
				{
					success = false
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, ACTAS, CLIENTE, GESTOR")]
		public ActionResult CargarMotivosTrabajo(int start, int lenght, string filtro = "", int order = 0, int asc = 1, bool todos = false, string tipoMotivoTrabajo = "TODOS")
		{
			DateTime fechaActual = DateTime.Now;

			var s = new JavaScriptSerializer();
			var jsonObj = s.Deserialize<dynamic>(filtro);

			string filtroCodigo = jsonObj["codigo"].ToString();
			string filtroCliente = jsonObj["cliente"];
			string filtroDescripcion = jsonObj["descripcion"];
			string filtroFechaInicio = jsonObj["fechaInicio"];
			string filtroFechaVencimiento = jsonObj["fechaVencimiento"];
			string filtroEstado = jsonObj["estado"];
			string filtroDiasRestantes = jsonObj["diasRestantes"].ToString();
			string filtroResponsable = jsonObj["responsable"].ToString();

			try
			{
				var contratos = (from mt in db.MotivoTrabajo
								 join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
								 join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
								 where mt.EstaActivo == 1
								 orderby mt.Secuencial
								 select new
								 {
									 secuencial = mt.Secuencial,
									 codigo = mt.Codigo,
									 cliente = cl.Descripcion,
									 descripcion = mt.Descripcion,
									 fechaInicio = mt.FechaInicio,
									 fechaVencimiento = mt.FechaFin,
									 estado = mt.estadoContrato != null
											   ?
												mt.estadoContrato.Codigo == "AUTOMATICO"
												?
												 mt.Avance == 100 ? "CERRADO" :
												 DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) > 30 ? "VIGENTE" :
												 DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) < 0 ? "VENCIDO" : "POR VENCER"
												:
												mt.estadoContrato.Codigo
											   :
											   mt.Avance == 100 ? "CERRADO" :
											   DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) > 30 ? "VIGENTE" :
											   DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) < 0 ? "VENCIDO" : "POR VENCER",
									 diasRestantes = DbFunctions.DiffDays(DateTime.Now, mt.FechaFin),
									 formaPago = DbFunctions.Right(mt.Codigo, 1),
									 responsable = mt.colaborador != null
													?
													mt.colaborador.persona.Nombre1 + " " + mt.colaborador.persona.Apellido1
													: "NO ASIGNADO",
									 tipoMotivoTrabajo = tt.Codigo,
									 fechaFacturacionActa = db.Acta
															 .Where(a => a.EstaActivo == 1 && a.SecuencialContrato == mt.Secuencial && a.EstadoActa.Descripcion != "TERMINADA" && a.TipoActa.Descripcion == "ENTREGA-RECEPCION")
															 .Select(a => a.Fecha ?? a.FechaFirma)
															 .FirstOrDefault(),
									 diasFacturacionActa = db.Acta
															 .Where(a => a.EstaActivo == 1 && a.SecuencialContrato == mt.Secuencial && a.EstadoActa.Descripcion != "TERMINADA" && a.TipoActa.Descripcion == "ENTREGA-RECEPCION")
															 .Select(a => a.DiasFacturacion)
															 .FirstOrDefault() ?? 0,
									 avance = mt.Avance,
									 semaforoActa = db.Acta.Where(a => a.EstaActivo == 1 && a.SecuencialContrato == mt.Secuencial && a.EstadoActa.Descripcion != "TERMINADA" && a.TipoActa.Descripcion == "ENTREGA-RECEPCION").Count() == 0 ? "WHITE" : db.Acta.Where(a => a.EstaActivo == 1 && a.SecuencialContrato == mt.Secuencial && a.EstadoActa.Descripcion != "TERMINADA" && a.TipoActa.Descripcion == "ENTREGA-RECEPCION").Count() > 1 ? "NARANJA" : "VERDE"
								 }).ToList();

				var contratosList = (from c in contratos
									 select new
									 {
										 secuencial = c.secuencial,
										 codigo = c.codigo,
										 cliente = c.cliente,
										 descripcion = c.descripcion,
										 fechaInicio = c.fechaInicio,
										 fechaVencimiento = c.fechaVencimiento,
										 estado = c.estado,
										 diasRestantes = c.diasRestantes,
										 formaPago = c.formaPago,
										 responsable = c.responsable,
										 tipoMotivoTrabajo = c.tipoMotivoTrabajo,
										 fechaFacturacionActa = c.fechaFacturacionActa,
										 diasFacturacionActa = c.diasFacturacionActa,
										 avance = c.avance,
										 semaforoActa = (c.semaforoActa != "WHITE" && c.semaforoActa != "NARANJA" && c.fechaFacturacionActa != (DateTime?)null) ? c.fechaFacturacionActa.AddDays(c.diasFacturacionActa) > fechaActual ? "VERDE" : c.fechaFacturacionActa.AddDays(c.diasFacturacionActa + 2) >= fechaActual ? "AMARILLO" : "ROJO" : c.semaforoActa
									 }).ToList();

				if(todos == false)
				{
					contratosList = (from c in contratosList
									 where c.avance != 100
									 select c).ToList();
				}
				if(tipoMotivoTrabajo != "TODOS")
				{
					if(tipoMotivoTrabajo == "SOPORTE Y MANTENIMIENTO")
					{
						tipoMotivoTrabajo = "PENDIENTES";
					}
					contratosList = (from c in contratosList
									 where c.tipoMotivoTrabajo.ToString().ToLower().Equals(tipoMotivoTrabajo.ToLower())
									 select c).ToList();
				}


				//Se aplican los filtros
				if(filtroCodigo != "")
				{
					contratosList = (from c in contratosList
									 where c.codigo.ToString().ToLower().Contains(filtroCodigo.ToLower())
									 select c).ToList();
				}

				if(filtroCliente != "")
				{
					contratosList = (from c in contratosList
									 where c.cliente.ToString().ToLower().Contains(filtroCliente.ToLower())
									 select c).ToList();
				}
				if(filtroDescripcion != "")
				{
					contratosList = (from c in contratosList
									 where c.descripcion.ToString().ToLower().Contains(filtroDescripcion.ToLower())
									 select c).ToList();
				}
				if(filtroFechaInicio != "")
				{
					contratosList = (from c in contratosList
									 where c.fechaInicio.ToString("dd/MM/yyyy").Contains(filtroFechaInicio)
									 select c).ToList();
				}
				if(filtroFechaVencimiento != "")
				{
					contratosList = (from c in contratosList
									 where c.fechaVencimiento.ToString("dd/MM/yyyy").Contains(filtroFechaVencimiento)
									 select c).ToList();
				}
				if(filtroEstado != "")
				{
					contratosList = (from c in contratosList
									 where c.estado.ToString().ToLower().Contains(filtroEstado.ToLower())
									 select c).ToList();
				}

				string v = "VENCIDO";
				if(filtroDiasRestantes != "")
				{
					if(v.Contains(filtroDiasRestantes.ToUpper()) && v.Contains(filtroDiasRestantes.ToUpper()))
					{
						contratosList = (from c in contratosList
										 where c.diasRestantes < 0
										 select c).ToList();
					}
					else
					{
						contratosList = (from c in contratosList
										 where c.diasRestantes >= 0 && c.diasRestantes.ToString().Contains(filtroDiasRestantes)
										 select c).ToList();
					}
				}

				if(filtroResponsable != "")
				{
					contratosList = (from c in contratosList
									 where c.responsable.ToString().ToLower().Contains(filtroResponsable.ToLower())
									 select c).ToList();
				}

				//Se ordena
				if(order > 0)
				{
					switch(order)
					{
						case 1:

							if(asc == 1)
							{
								contratosList = (from c in contratosList
												 orderby c.codigo
												 select c).ToList();
							}
							else
							{
								contratosList = (from c in contratosList
												 orderby c.codigo descending
												 select c).ToList();
							}

							break;

						case 2:

							if(asc == 1)
							{
								contratosList = (from c in contratosList
												 orderby c.cliente
												 select c).ToList();
							}
							else
							{
								contratosList = (from c in contratosList
												 orderby c.cliente descending
												 select c).ToList();
							}

							break;

						case 3:

							if(asc == 1)
							{
								contratosList = (from c in contratosList
												 orderby c.descripcion
												 select c).ToList();
							}
							else
							{
								contratosList = (from c in contratosList
												 orderby c.descripcion descending
												 select c).ToList();
							}

							break;

						case 4:

							if(asc == 1)
							{
								contratosList = (from c in contratosList
												 orderby c.fechaInicio
												 select c).ToList();
							}
							else
							{
								contratosList = (from c in contratosList
												 orderby c.fechaInicio descending
												 select c).ToList();
							}

							break;

						case 5:

							if(asc == 1)
							{
								contratosList = (from c in contratosList
												 orderby c.fechaVencimiento
												 select c).ToList();
							}
							else
							{
								contratosList = (from c in contratosList
												 orderby c.fechaVencimiento descending
												 select c).ToList();
							}

							break;

						case 6:

							if(asc == 1)
							{
								contratosList = (from c in contratosList
												 orderby c.estado
												 select c).ToList();
							}
							else
							{
								contratosList = (from c in contratosList
												 orderby c.estado descending
												 select c).ToList();
							}

							break;

						case 7:

							if(asc == 1)
							{
								contratosList = (from c in contratosList
												 orderby c.diasRestantes
												 select c).ToList();
							}
							else
							{
								contratosList = (from c in contratosList
												 orderby c.diasRestantes descending
												 select c).ToList();
							}

							break;

						case 8:

							if(asc == 1)
							{
								contratosList = (from c in contratosList
												 orderby c.responsable
												 select c).ToList();
							}
							else
							{
								contratosList = (from c in contratosList
												 orderby c.responsable descending
												 select c).ToList();
							}

							break;
					}
				}

				var cantidad = contratosList.Count;

				contratosList = contratosList.Skip(start).Take(lenght).ToList();

				return Json(new
				{
					contratos = contratosList,
					cantidadContratos = cantidad,
					success = true
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					msg = e.Message,
					success = false
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, ACTAS, GESTOR")]
		public ActionResult CargarGarantiasTrabajo(int start, int lenght, string filtro = "", int order = 0, int asc = 1, bool todos = false, string tipoMotivoTrabajo = "TODOS")
		{

			var s = new JavaScriptSerializer();
			var jsonObj = s.Deserialize<dynamic>(filtro);
			DateTime dateDefault = DateTime.Parse("0001-01-01");

			string filtroCodigo = jsonObj["codigo"].ToString();
			string filtroCliente = jsonObj["cliente"];
			string filtroDescripcion = jsonObj["descripcion"];
			string filtroFechaProduccion = jsonObj["fechaProduccion"];
			string filtroFechaVencimiento = jsonObj["fechaVencimiento"];
			string filtroDiasRestantes = jsonObj["diasRestantes"].ToString();

			try
			{
				var garantiasTemporal = (from mt in db.MotivoTrabajo
										 join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
										 join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
										 where mt.EstaActivo == 1
										 orderby mt.Codigo
										 select new
										 {
											 secuencial = mt.Secuencial,
											 codigo = mt.Codigo,
											 cliente = cl.Descripcion,
											 descripcion = mt.Descripcion,
											 fechaProduccion = mt.motivoTrabajoInformacionAdicional != null ? (mt.motivoTrabajoInformacionAdicional.FechaProduccion != dateDefault ? mt.motivoTrabajoInformacionAdicional.FechaProduccion : (DateTime?)null) : (DateTime?)null,
											 fechaVencimiento = mt.motivoTrabajoInformacionAdicional.FechaProduccion != dateDefault ? DbFunctions.AddDays(mt.motivoTrabajoInformacionAdicional.FechaProduccion, mt.motivoTrabajoInformacionAdicional.DiasGarantia) : (DateTime?)null,
											 diasRestantes = mt.motivoTrabajoInformacionAdicional != null ? DbFunctions.DiffDays(DateTime.Now, mt.motivoTrabajoInformacionAdicional.FechaProduccion) + mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0,
											 tipoMotivoTrabajo = tt.Codigo,
											 avance = mt.Avance,
											 entregables = (from e in mt.entregableMotivoTrabajo
															where e.EstaActivo == 1
															select new
															{
																e.Nombre,
																e.FechaProduccion,
																FechaVencimiento = DbFunctions.AddDays(e.FechaProduccion, mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0),
																DiasRestantes = DbFunctions.DiffDays(DateTime.Now, DbFunctions.AddDays(e.FechaProduccion, mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0))
															}).OrderByDescending(x => x.DiasRestantes).ToList()
										 }).ToList();


				var garantias = garantiasTemporal.Select(e => new
				{
					e.secuencial,
					e.codigo,
					e.cliente,
					e.descripcion,
					fechaProduccion = e.fechaProduccion.HasValue ? e.fechaProduccion.Value.ToString("dd/MM/yyyy") : "",
					fechaVencimiento = e.fechaVencimiento.HasValue ? e.fechaVencimiento.Value.ToString("dd/MM/yyyy") : "",
					e.diasRestantes,
					e.tipoMotivoTrabajo,
					e.avance,
					e.entregables
				}).ToList();


				if(todos == false)
				{
					garantias = (from c in garantias
								 where c.diasRestantes > 0
								 select c).ToList();
				}
				if(tipoMotivoTrabajo != "TODOS")
				{
					if(tipoMotivoTrabajo == "SOPORTE Y MANTENIMIENTO")
					{
						tipoMotivoTrabajo = "PENDIENTES";
					}
					garantias = (from c in garantias
								 where c.tipoMotivoTrabajo.ToString().ToLower().Equals(tipoMotivoTrabajo.ToLower())
								 select c).ToList();
				}

				//Se aplican los filtros
				if(filtroCodigo != "")
				{
					garantias = (from c in garantias
								 where c.codigo.ToString().ToLower().Contains(filtroCodigo.ToLower())
								 select c).ToList();
				}

				if(filtroCliente != "")
				{
					garantias = (from c in garantias
								 where c.cliente.ToString().ToLower().Contains(filtroCliente.ToLower())
								 select c).ToList();
				}
				if(filtroDescripcion != "")
				{
					garantias = (from c in garantias
								 where c.descripcion.ToString().ToLower().Contains(filtroDescripcion.ToLower())
								 select c).ToList();
				}
				string nAsig = "NO ASIGNADA";
				if(filtroFechaVencimiento != "")
				{
					if(nAsig.Contains(filtroFechaVencimiento.ToUpper()))
					{
						garantias = (from c in garantias
									 where c.fechaVencimiento == ""
									 select c).ToList();
					}
					else
					{
						garantias = (from c in garantias
									 where c.fechaVencimiento != "" && c.fechaVencimiento.Contains(filtroFechaVencimiento)
									 select c).ToList();
					}
				}

				if(filtroFechaProduccion != "")
				{
					if(nAsig.Contains(filtroFechaProduccion.ToUpper()))
					{
						garantias = (from c in garantias
									 where c.fechaProduccion == ""
									 select c).ToList();
					}
					else
					{
						garantias = (from c in garantias
									 where c.fechaProduccion != "" && c.fechaProduccion.Contains(filtroFechaProduccion)
									 select c).ToList();
					}
				}

				string na = "NO APLICA";
				string v = "VENCIDA";
				if(filtroDiasRestantes != "")
				{
					if(na.Contains(filtroDiasRestantes.ToUpper()) && v.Contains(filtroDiasRestantes.ToUpper()))
					{
						garantias = (from c in garantias
									 where c.diasRestantes < 0
									 select c).ToList();
					}
					else
					{
						if(na.Contains(filtroDiasRestantes.ToUpper()))
						{
							garantias = (from c in garantias
										 where c.fechaProduccion == "" || c.fechaVencimiento == ""
										 select c).ToList();
						}
						else if(v.Contains(filtroDiasRestantes.ToUpper()))
						{
							garantias = (from c in garantias
										 where c.diasRestantes < 0 && c.fechaVencimiento != ""
										 select c).ToList();
						}
						else
						{
							garantias = (from c in garantias
										 where c.diasRestantes > 0 && c.diasRestantes.ToString().Contains(filtroDiasRestantes)
										 select c).ToList();
						}
					}
				}

				//Se ordena
				if(order > 0)
				{
					switch(order)
					{
						case 1:

							if(asc == 1)
							{
								garantias = (from c in garantias
											 orderby c.codigo
											 select c).ToList();
							}
							else
							{
								garantias = (from c in garantias
											 orderby c.codigo descending
											 select c).ToList();
							}

							break;

						case 2:

							if(asc == 1)
							{
								garantias = (from c in garantias
											 orderby c.cliente
											 select c).ToList();
							}
							else
							{
								garantias = (from c in garantias
											 orderby c.cliente descending
											 select c).ToList();
							}

							break;

						case 3:

							if(asc == 1)
							{
								garantias = (from c in garantias
											 orderby c.descripcion
											 select c).ToList();
							}
							else
							{
								garantias = (from c in garantias
											 orderby c.descripcion descending
											 select c).ToList();
							}

							break;

						case 4:

							if(asc == 1)
							{
								garantias = (from c in garantias
											 orderby c.fechaProduccion
											 select c).ToList();
							}
							else
							{
								garantias = (from c in garantias
											 orderby c.fechaProduccion descending
											 select c).ToList();
							}

							break;

						case 5:

							if(asc == 1)
							{
								garantias = (from c in garantias
											 orderby c.fechaVencimiento
											 select c).ToList();
							}
							else
							{
								garantias = (from c in garantias
											 orderby c.fechaVencimiento descending
											 select c).ToList();
							}

							break;

						case 6:

							if(asc == 1)
							{
								garantias = (from c in garantias
											 orderby c.diasRestantes
											 select c).ToList();
							}
							else
							{
								garantias = (from c in garantias
											 orderby c.diasRestantes descending
											 select c).ToList();
							}

							break;
					}
				}

				var cantidad = garantias.Count;

				garantias = garantias.Skip(start).Take(lenght).ToList();

				return Json(new
				{
					garantias,
					cantidadGarantias = cantidad,
					success = true
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					msg = e.Message,
					success = false
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, GESTOR")]
		public ActionResult CargarSeguimientosTrabajo(int start, int lenght, string codigoContrato, string datosNuevos, string datosEliminar, string datosEditar)
		{
			try
			{
				var serNuevo = new JavaScriptSerializer();
				var serEliminar = new JavaScriptSerializer();
				var serEditar = new JavaScriptSerializer();
				var jsonObjNuevos = serNuevo.Deserialize<dynamic>(datosNuevos);
				var jsonObjEliminados = serEliminar.Deserialize<dynamic>(datosEliminar);
				var jsonObjEditados = serEliminar.Deserialize<dynamic>(datosEditar);

				var seguimientosContrato = (from mt in db.MotivoTrabajo
											join mi in db.MotivoTrabajoInformacionAdicional on mt.Secuencial equals mi.SecuencialMotivoTrabajo
											join ms in db.MotivoTrabajoSeguimiento on mi.SecuencialMotivoTrabajo equals ms.SecuencialMotivoTrabajoInformacionAdicional
											where mt.Codigo.ToUpper() == codigoContrato.ToUpper() && mt.EstaActivo == 1 && mi.EstaActivo == 1 && ms.EstaActivo == 1
											orderby ms.Fecha
											select new
											{
												Secuencial = ms.Secuencial,
												Fecha = ms.Fecha,
												Realizado = ms.Realizado,
												PorRealizar = ms.PorRealizar,
												Pendiente = ms.Pendiente,
												SecuencialUsuario = ms.SecuencialUsuario,
												NumeroVerificador = ms.NumeroVerificador
											}).ToList().Select(x => new MotivoTrabajoSeguimiento
											{
												Secuencial = x.Secuencial,
												Fecha = x.Fecha,
												Realizado = x.Realizado,
												PorRealizar = x.PorRealizar,
												Pendiente = x.Pendiente,
												SecuencialUsuario = x.SecuencialUsuario,
												NumeroVerificador = x.NumeroVerificador
											}).ToList();

				var cantidad = seguimientosContrato.Count + jsonObjNuevos.Length - jsonObjEliminados.Length;

				for(int i = 0; i < jsonObjEliminados.Length; i++)
				{
					foreach(var s in seguimientosContrato.ToList())
					{
						if(s.Secuencial == int.Parse(jsonObjEliminados[i]["Secuencial"].ToString()))
						{
							seguimientosContrato.Remove(s);
							break;
						}
					}
				}

				for(int i = 0; i < jsonObjEditados.Length; i++)
				{
					foreach(var s in seguimientosContrato.ToList())
					{
						if(s.Secuencial == int.Parse(jsonObjEditados[i]["Secuencial"].ToString()))
						{
							var fechaStringFormat = jsonObjEditados[i]["FechaSeguimiento"].Split('/');
							s.Fecha = new DateTime(int.Parse(fechaStringFormat[2]), int.Parse(fechaStringFormat[1]), int.Parse(fechaStringFormat[0]));
							s.Realizado = jsonObjEditados[i]["Realizado"].ToString();
							s.PorRealizar = jsonObjEditados[i]["PorRealizar"].ToString();
							s.Pendiente = jsonObjEditados[i]["Pendiente"].ToString();
							break;
						}
					}
				}

				for(int i = 0; i < jsonObjNuevos.Length; i++)
				{
					MotivoTrabajoSeguimiento motivoTrabajoSeguimiento = new MotivoTrabajoSeguimiento();
					motivoTrabajoSeguimiento.Secuencial = int.Parse(jsonObjNuevos[i]["Secuencial"].ToString());
					var fechaString = jsonObjNuevos[i]["FechaSeguimiento"].Split('/');
					motivoTrabajoSeguimiento.Fecha = new DateTime(int.Parse(fechaString[2]), int.Parse(fechaString[1]), int.Parse(fechaString[0]));
					motivoTrabajoSeguimiento.Realizado = jsonObjNuevos[i]["Realizado"].ToString();
					motivoTrabajoSeguimiento.PorRealizar = jsonObjNuevos[i]["PorRealizar"].ToString();
					motivoTrabajoSeguimiento.Pendiente = jsonObjNuevos[i]["Pendiente"].ToString();
					motivoTrabajoSeguimiento.NumeroVerificador = int.Parse(jsonObjNuevos[i]["NumeroVerificador"].ToString());

					seguimientosContrato.Add(motivoTrabajoSeguimiento);
				}

				var seguimientos = (from s in seguimientosContrato
									orderby s.Fecha descending
									select new
									{
										SecuencialMotivoTrabajo = s.Secuencial,
										FechaSeguimiento = s.Fecha.ToString("dd/MM/yyyy"),
										s.Realizado,
										s.PorRealizar,
										s.Pendiente,
										Usuario = s.SecuencialUsuario != null ? db.Usuario.Where(u => u.Secuencial == s.SecuencialUsuario).FirstOrDefault().persona.Nombre1 + " " + db.Usuario.Where(u => u.Secuencial == s.SecuencialUsuario).FirstOrDefault().persona.Apellido1 : "",
										s.NumeroVerificador
									}).ToList();

				seguimientos = seguimientos.Skip(start).Take(lenght).ToList();

				return Json(new
				{
					seguimientos,
					cantidadSeguimientos = cantidad,
					success = true
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					msg = e.Message,
					success = false
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, ACTAS, GESTOR")]
		public ActionResult DarActas(int start, int lenght, string filtro = "", int order = 0, int asc = 1, string tipoMotivoTrabajo = "TODOS")
		{

			var s = new JavaScriptSerializer();
			var jsonObj = s.Deserialize<dynamic>(filtro);

			string filtroCodigo = jsonObj["codigo"].ToString();
			string filtroCliente = jsonObj["cliente"];
			string filtroAsunto = jsonObj["asunto"];
			string filtroFecha = jsonObj["fecha"];
			string filtroColaborador = jsonObj["colaborador"];
			string filtroTipo = jsonObj["tipo"];
			string filtroEstado = jsonObj["estado"];
			string filtroNumeroContrato = jsonObj["numeroContrato"];
			string filtroContrato = jsonObj["contrato"];

			try
			{
				var actas = (from ac in db.Acta
							 join mt in db.MotivoTrabajo on ac.SecuencialContrato equals mt.Secuencial
							 join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
							 join cl in db.Cliente on ac.SecuencialCliente equals cl.Secuencial
							 join co in db.Colaborador on ac.SecuencialColaborador equals co.Secuencial
							 join ta in db.TipoActa on ac.SecuencialTipo equals ta.Secuencial
							 join ea in db.EstadoActa on ac.SecuencialEstado equals ea.Secuencial
							 join pe in db.Persona on co.SecuencialPersona equals pe.Secuencial
							 where ac.EstaActivo == 1
							 orderby ac.Codigo
							 select new
							 {
								 secuencial = ac.Secuencial,
								 cliente = cl.Descripcion,
								 asunto = mt.Nombre,
								 fechaFirma = ac.FechaFirma,
								 colaborador = pe.Nombre1 + " " + pe.Apellido1,
								 tipo = ta.Descripcion,
								 estado = ea.Descripcion,
								 codigo = ac.Codigo,
								 numeroContrato = mt.Codigo,
								 linkopenkm = ac.LinkOpenkm,
								 tipoMotivoTrabajo = tt.Codigo
							 }).ToList();

				if(tipoMotivoTrabajo != "TODOS")
				{
					if(tipoMotivoTrabajo == "SOPORTE Y MANTENIMIENTO")
					{
						tipoMotivoTrabajo = "PENDIENTES";
					}
					actas = (from a in actas
							 where a.tipoMotivoTrabajo.ToString().ToLower().Equals(tipoMotivoTrabajo.ToLower())
							 select a).ToList();
				}
				//Se aplican los filtros
				if(filtroNumeroContrato != "")
				{
					actas = (from a in actas
							 where a.numeroContrato.ToString().Equals(filtroNumeroContrato)
							 select a).ToList();
				}

				if(filtroContrato != "")
				{
					actas = (from a in actas
							 where a.numeroContrato.ToString().ToLower().Contains(filtroContrato.ToLower())
							 select a).ToList();
				}

				if(filtroCodigo != "")
				{
					actas = (from a in actas
							 where a.codigo.ToString().ToLower().Contains(filtroCodigo.ToLower())
							 select a).ToList();
				}
				if(filtroCliente != "")
				{
					actas = (from a in actas
							 where a.cliente.ToString().ToLower().Contains(filtroCliente.ToLower())
							 select a).ToList();
				}
				if(filtroAsunto != "")
				{
					actas = (from a in actas
							 where a.asunto.ToString().ToLower().Contains(filtroAsunto.ToLower())
							 select a).ToList();
				}
				if(filtroFecha != "")
				{
					actas = (from a in actas
							 where a.fechaFirma.ToString("dd/MM/yyyy").Contains(filtroFecha)
							 select a).ToList();
				}
				if(filtroColaborador != "")
				{
					actas = (from a in actas
							 where a.colaborador.ToString().ToLower().Contains(filtroColaborador.ToLower())
							 select a).ToList();
				}
				if(filtroTipo != "")
				{
					actas = (from a in actas
							 where a.tipo.ToString().Contains(filtroTipo.ToUpper())
							 select a).ToList();
				}
				if(filtroEstado != "")
				{
					actas = (from a in actas
							 where a.estado.ToString().Contains(filtroEstado.ToUpper())
							 select a).ToList();
				}

				//Se ordena
				if(order > 0)
				{
					switch(order)
					{
						case 1:

							if(asc == 1)
							{
								actas = (from a in actas
										 orderby a.codigo
										 select a).ToList();
							}
							else
							{
								actas = (from a in actas
										 orderby a.codigo descending
										 select a).ToList();
							}

							break;

						case 2:

							if(asc == 1)
							{
								actas = (from a in actas
										 orderby a.numeroContrato
										 select a).ToList();
							}
							else
							{
								actas = (from a in actas
										 orderby a.numeroContrato descending
										 select a).ToList();
							}

							break;

						case 3:

							if(asc == 1)
							{
								actas = (from a in actas
										 orderby a.cliente
										 select a).ToList();
							}
							else
							{
								actas = (from a in actas
										 orderby a.cliente descending
										 select a).ToList();
							}

							break;

						case 4:

							if(asc == 1)
							{
								actas = (from a in actas
										 orderby a.asunto
										 select a).ToList();
							}
							else
							{
								actas = (from a in actas
										 orderby a.asunto descending
										 select a).ToList();
							}

							break;

						case 5:

							if(asc == 1)
							{
								actas = (from a in actas
										 orderby a.fechaFirma
										 select a).ToList();
							}
							else
							{
								actas = (from a in actas
										 orderby a.fechaFirma descending
										 select a).ToList();
							}

							break;

						case 6:

							if(asc == 1)
							{
								actas = (from a in actas
										 orderby a.colaborador
										 select a).ToList();
							}
							else
							{
								actas = (from a in actas
										 orderby a.colaborador descending
										 select a).ToList();
							}

							break;

						case 7:

							if(asc == 1)
							{
								actas = (from a in actas
										 orderby a.tipo
										 select a).ToList();
							}
							else
							{
								actas = (from a in actas
										 orderby a.tipo descending
										 select a).ToList();
							}

							break;

						case 8:

							if(asc == 1)
							{
								actas = (from a in actas
										 orderby a.estado
										 select a).ToList();
							}
							else
							{
								actas = (from a in actas
										 orderby a.estado descending
										 select a).ToList();
							}

							break;
					}
				}

				var cantidad = actas.Count;

				actas = actas.Skip(start).Take(lenght).ToList();

				return Json(new
				{
					actas,
					cantidadActas = cantidad,
					success = true
				});
			}
			catch(Exception e)
			{
				return Json(new
				{
					msg = e.Message,
					success = false
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, ACTAS, GESTOR")]
		public ActionResult DarTiposActas()
		{
			var datos = (from ta in db.TipoActa
						 orderby ta.Descripcion
						 select new
						 {
							 nombre = ta.Descripcion
						 }).ToList();

			return Json(new
			{
				success = true,
				tiposActas = datos
			});
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, ACTAS, GESTOR")]
		public ActionResult DarEstadosActas()
		{
			var datos = (from ea in db.EstadoActa
						 orderby ea.Descripcion
						 select new
						 {
							 nombre = ea.Descripcion
						 }).ToList();

			return Json(new
			{
				success = true,
				estadosActas = datos
			});
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult DarDatosMotivoTrabajo(int idMotivoTrabajo)
		{
			try
			{
				MotivoTrabajo motivo = db.MotivoTrabajo.Find(idMotivoTrabajo);
				if(motivo == null)
				{
					throw new Exception("Error, No se encontró el motivo de trabajo");
				}

				var datos = new
				{
					id = motivo.Secuencial,
					codigo = motivo.Codigo,
					cliente = motivo.SecuencialCliente,
					estadoContrato = motivo.SecuencialEstadoContrato,
					colaborador = motivo.SecuencialColaborador,
					tipoPlazo = motivo.SecuencialTipoPlazo,
					diasPlazo = motivo.Plazo,
					nombre = motivo.Nombre,
					fechaInicio = motivo.FechaInicio.ToString("dd/MM/yyyy"),
					fechaFin = motivo.FechaFin.ToString("dd/MM/yyyy"),
					detalle = motivo.Descripcion,
					verificador = motivo.NumeroVerificador,
					tipo = motivo.tipoMotivoTrabajo.Codigo,
					idTipo = motivo.SecuencialTipoMotivoTrabajo,
					horasMes = motivo.HorasMes,
					adjuntos = (from cAdj in db.AdjuntoContrato
								where cAdj.SecuencialContrato == idMotivoTrabajo
								select new
								{
									idAdj = cAdj.Secuencial,
									url = cAdj.Url,
									desc = cAdj.Descripcion
								}).ToList(),
				};

				var resp = new
				{
					success = true,
					datos = datos
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					resp = e.Message
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, GESTOR")]
		public ActionResult DarDetallesGarantia(string codigoContrato)
		{
			try
			{
				var detallesContrato = (from mt in db.MotivoTrabajo
										where mt.Codigo.ToUpper() == codigoContrato.ToUpper() && mt.EstaActivo == 1
										select new
										{
											codigo = mt.Codigo,
											entregables = (from e in mt.entregableMotivoTrabajo
														   where e.EstaActivo == 1
														   select new
														   {
															   e.Nombre,
															   e.FechaProduccion,
															   FechaVencimiento = DbFunctions.AddDays(e.FechaProduccion, mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0),
															   DiasRestantes = DbFunctions.DiffDays(DateTime.Now, DbFunctions.AddDays(e.FechaProduccion, mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0))
														   }).ToList()
										}).First();



				if(detallesContrato == null)
				{
					throw new Exception("Error, No se encontró el motivo de trabajo");
				}

				var entregables = (from e in detallesContrato.entregables
								   select new
								   {
									   e.Nombre,
									   FechaProduccion = e.FechaProduccion != null ? e.FechaProduccion.Value.ToString("dd/MM/yyyy") : "NO ASIGNADA",
									   FechaVencimiento = e.FechaVencimiento != null ? e.FechaVencimiento.Value.ToString("dd/MM/yyyy") : "NO ASIGNADA",
									   DiasRestantes = e.DiasRestantes != null ? e.DiasRestantes : 0
								   }
								   ).ToList();

				var resp = new
				{
					success = true,
					datos = entregables
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					resp = e.Message
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC, GESTOR")]
		public ActionResult DarDetallesMotivoTrabajo(string codigoContrato)
		{
			try
			{
				string emailUser = User.Identity.Name;

				var detallesContrato = (from mt in db.MotivoTrabajo
										where mt.Codigo.ToUpper() == codigoContrato.ToUpper() && mt.EstaActivo == 1
										select new
										{
											secuencial = mt.Secuencial,
											codigo = mt.Codigo,
											cliente = mt.cliente.Descripcion,
											idCliente = mt.cliente.Secuencial,
											descripcion = mt.Descripcion,
											avance = mt.Avance,
											entregables = (from e in mt.entregableMotivoTrabajo
														   where e.EstaActivo == 1
														   orderby e.Secuencial ascending
														   select new
														   {
															   Id = e.Secuencial,
															   e.Nombre,
															   e.Avance,
															   Colaborador = e.colaborador != null ? e.colaborador.persona.Nombre1.Substring(0, 1) + "." + e.colaborador.persona.Apellido1 : "No_Asignado",
															   ColaboradorId = e.colaborador != null ? e.colaborador.Secuencial : 0
														   }).ToList(),
											tipo = mt.tipoMotivoTrabajo.Codigo,
											estado = mt.estadoContrato != null
											   ?
												mt.estadoContrato.Codigo == "AUTOMATICO"
												?
												 mt.Avance == 100 ? "CERRADO" :
												 DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) > 30 ? "VIGENTE" :
												 DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) < 0 ? "VENCIDO" : "POR VENCER"
												:
												mt.estadoContrato.Codigo
											   :
											   mt.Avance == 100 ? "CERRADO" :
											   DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) > 30 ? "VIGENTE" :
											   DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) < 0 ? "VENCIDO" : "POR VENCER",
											estadoId = mt.estadoContrato != null ? mt.estadoContrato.Secuencial : 0,
											fechaInicio = mt.FechaInicio,
											fechaFin = mt.FechaFin,
											fechaInicioPlanificacion = mt.FechaInicioPlanificacion,
											fechaFinPlanificacion = mt.FechaFinPlanificacion,
											nombre = mt.Nombre,

											adendas = (from a in mt.motivoTrabajoInformacionAdicional.adenda
													   where a.EstaActivo == 1
													   select new
													   {
														   a.Secuencial,
														   a.Codigo,
														   a.FechaVencimiento,
														   a.LinkOpenKM,
														   a.NumeroVerificador
													   }).ToList(),
											fase = mt.motivoTrabajoInformacionAdicional.faseContrato != null ? mt.motivoTrabajoInformacionAdicional.faseContrato.Secuencial : 0,
											estimacion = mt.motivoTrabajoInformacionAdicional.Estimacion ?? 0,
											isRossi = (emailUser == "rlandave@sifizsoft.com") || (emailUser == "rsanchez@sifizsoft.com"),
											cronograma = mt.motivoTrabajoInformacionAdicional.TieneCronograma == 1 ? true : false,
											pagado = mt.motivoTrabajoInformacionAdicional.PagadoTotalmente == 1 ? true : false,
											numeroVerificador = mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.NumeroVerificador : 0,
											fechaActa = mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.FechaActa ?? new DateTime(0001, 01, 01) : new DateTime(0001, 01, 01),
											fechaProduccion = mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.FechaProduccion : new DateTime(0001, 01, 01),
											diasGarantia = mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0,
											diasRestantes = DbFunctions.DiffDays(DateTime.Now, mt.FechaFin),
											formaPago = DbFunctions.Right(mt.Codigo, 1),
											linkOpenKm = mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.LinkOpenKm : "",
											colaborador = mt.colaborador != null ? mt.colaborador.Secuencial : 0,
											plazo = mt.Plazo != null ? mt.Plazo : -1,
											tipoPlazo = mt.tipoPlazo != null ? mt.tipoPlazo.Codigo : "NO ASIGNADO",
											horasMes = mt.HorasMes,
											aceptaNormativos = (mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.AceptaCambiosNormativos ?? 0 : 0) == 1,
											adjuntos = (from adjt in db.AdjuntoContrato
														where adjt.SecuencialContrato == mt.Secuencial
														select adjt.Url).ToList()
										}).First();

				var adendas = (from a in detallesContrato.adendas
							   select new
							   {
								   a.Codigo,
								   FechaVencimiento = a.FechaVencimiento.ToString("dd/MM/yyyy"),
								   a.Secuencial,
								   a.LinkOpenKM,
								   a.NumeroVerificador
							   }).ToList();

				if(detallesContrato == null)
				{
					throw new Exception("Error, No se encontró el motivo de trabajo");
				}

				var resp = new
				{
					success = true,
					adendas = adendas,
					datos = detallesContrato,
					fechaProduccion = detallesContrato.fechaProduccion.ToString("dd/MM/yyyy"),
					fechaActa = detallesContrato.fechaActa.ToString("dd/MM/yyyy")
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					resp = e.Message
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult DarDatosEntregableTrabajo(int idEntregableTrabajo)
		{
			try
			{
				EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(idEntregableTrabajo);
				if(entregable == null)
				{
					throw new Exception("Error, No se encontró el entregable");
				}

				var datos = new
				{
					id = entregable.Secuencial,
					nombre = entregable.Nombre,
					fechaInicio = entregable.FechaInicio.ToString("dd/MM/yyyy"),
					fechaFin = entregable.FechaFin.ToString("dd/MM/yyyy"),
					fechaProduccion = entregable.FechaProduccion.HasValue ? entregable.FechaProduccion.Value.ToString("dd/MM/yyyy") : "",
					detalle = entregable.Descripcion,
					avance = entregable.Avance,
					colaboradorMotivoTrabajo = entregable.SecuencialColaborador,
					verificador = entregable.NumeroVerificador,

					motivo = entregable.motivoTrabajo.Nombre,
					idMotivo = entregable.SecuencialMotivoTrabajo
				};

				var resp = new
				{
					success = true,
					datos = datos
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = false,
					resp = e.Message
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult EliminarMotivoTrabajo(int idMotivoTrabajo)
		{
			try
			{
				MotivoTrabajo motivo = db.MotivoTrabajo.Find(idMotivoTrabajo);
				if(motivo == null)
				{
					throw new Exception("Error, no se encontró el trabajo.");
				}

				motivo.EstaActivo = 0;
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
					success = true,
					resp = e.Message
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult EliminarEntregableTrabajo(int idEntregableTrabajo)
		{
			try
			{
				EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(idEntregableTrabajo);
				if(entregable == null)
				{
					throw new Exception("Error, no se encontró el entregable");
				}

				MotivoTrabajo motivoTrabajo = entregable.motivoTrabajo;
				if(motivoTrabajo == null)
				{
					throw new Exception("Error, no se encontró el motivo de trabajo.");
				}

				entregable.EstaActivo = 0;
				db.SaveChanges();

				//Actualizando las fechas de planificacion
				DateTime fechaInicio = (from emt in db.EntregableMotivoTrabajo
										where emt.SecuencialMotivoTrabajo == motivoTrabajo.Secuencial && emt.EstaActivo == 1
										orderby emt.FechaInicio ascending
										select emt.FechaInicio).FirstOrDefault();

				DateTime fechaFin = (from emt in db.EntregableMotivoTrabajo
									 where emt.SecuencialMotivoTrabajo == motivoTrabajo.Secuencial && emt.EstaActivo == 1
									 orderby emt.FechaFin descending
									 select emt.FechaFin).FirstOrDefault();

				if(fechaFin != null && fechaInicio != null)
				{
					motivoTrabajo.FechaInicioPlanificacion = fechaInicio;
					motivoTrabajo.FechaFinPlanificacion = fechaFin;
				}
				else
				{
					motivoTrabajo.FechaInicioPlanificacion = new System.DateTime(9999, 12, 31);
					motivoTrabajo.FechaFinPlanificacion = new System.DateTime(1, 1, 1);
				}

				//Actualizando los avances
				var relacionesAvance = (from emt in db.EntregableMotivoTrabajo
										where emt.SecuencialMotivoTrabajo == motivoTrabajo.Secuencial && emt.EstaActivo == 1
										select new
										{
											diaInicial = emt.FechaInicio,
											diaFinal = emt.FechaFin,
											avance = emt.Avance
										}).ToList();

				int totalDias = 0;
				int totalDiasAvance = 0;
				foreach(var relacion in relacionesAvance)
				{
					int tdias = (relacion.diaFinal - relacion.diaInicial).Days;
					totalDias += tdias;
					totalDiasAvance += (tdias * relacion.avance / 100);
				}

				int porciento = 0;
				if(totalDias > 0)
					porciento = (int)Math.Ceiling((double)totalDiasAvance * 100 / (double)totalDias);

				motivoTrabajo.Avance = porciento;
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
					success = true,
					resp = e.Message
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN, GESTOR")]
		public ActionResult EditarPorcentajeEntregable(int idEntregableTrabajo, int porcentaje, int colaboradorID = 0, string codigoContrato = "")
		{
			try
			{
				EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(idEntregableTrabajo);
				if(entregable == null)
				{
					throw new Exception("Error, no se encontró el entregable");
				}
				if(porcentaje < 0 || porcentaje > 100)
				{
					throw new Exception("Error, el porcentaje debe estar entre 0 y 100");
				}

				MotivoTrabajo motivoTrabajo = entregable.motivoTrabajo;
				if(motivoTrabajo == null)
				{
					throw new Exception("Error, no se encontró el motivo de trabajo.");
				}

				entregable.Avance = porcentaje;
				if(colaboradorID != 0)
				{
					entregable.SecuencialColaborador = colaboradorID;
				}
				db.SaveChanges();

				//Actualizando las fechas de planificacion
				DateTime fechaInicio = (from emt in db.EntregableMotivoTrabajo
										where emt.SecuencialMotivoTrabajo == motivoTrabajo.Secuencial && emt.EstaActivo == 1
										orderby emt.FechaInicio ascending
										select emt.FechaInicio).FirstOrDefault();

				DateTime fechaFin = (from emt in db.EntregableMotivoTrabajo
									 where emt.SecuencialMotivoTrabajo == motivoTrabajo.Secuencial && emt.EstaActivo == 1
									 orderby emt.FechaFin descending
									 select emt.FechaFin).FirstOrDefault();

				if(fechaFin != null && fechaInicio != null)
				{
					motivoTrabajo.FechaInicioPlanificacion = fechaInicio;
					motivoTrabajo.FechaFinPlanificacion = fechaFin;
				}
				else
				{
					motivoTrabajo.FechaInicioPlanificacion = new System.DateTime(9999, 12, 31);
					motivoTrabajo.FechaFinPlanificacion = new System.DateTime(1, 1, 1);
				}

				//Actualizando los avances
				var relacionesAvance = (from emt in db.EntregableMotivoTrabajo
										where emt.SecuencialMotivoTrabajo == motivoTrabajo.Secuencial && emt.EstaActivo == 1
										select new
										{
											diaInicial = emt.FechaInicio,
											diaFinal = emt.FechaFin,
											avance = emt.Avance
										}).ToList();

				int totalDias = 0;
				int totalDiasAvance = 0;
				foreach(var relacion in relacionesAvance)
				{
					int tdias = (relacion.diaFinal - relacion.diaInicial).Days;
					totalDias += tdias;
					totalDiasAvance += (tdias * relacion.avance / 100);
				}

				int porciento = 0;
				if(totalDias > 0)
					porciento = (int)Math.Ceiling((double)totalDiasAvance * 100 / (double)totalDias);

				motivoTrabajo.Avance = porciento;

				var motivo = db.MotivoTrabajo.Where(m => m.Codigo == codigoContrato).FirstOrDefault();
				var entregables = (from e in db.EntregableMotivoTrabajo
								   where e.SecuencialMotivoTrabajo == motivo.Secuencial && e.EstaActivo == 1
								   orderby e.Secuencial ascending
								   select new
								   {
									   Id = e.Secuencial,
									   e.Nombre,
									   e.Avance,
									   Colaborador = e.colaborador != null ? e.colaborador.persona.Nombre1.Substring(0, 1) + "." + e.colaborador.persona.Apellido1 : "No_Asignado",
									   ColaboradorId = e.colaborador != null ? e.colaborador.Secuencial : 0
								   }).ToList();

				db.SaveChanges();

				var resp = new
				{
					success = true,
					datos = new
					{
						porcentaje = porciento,
						colaborador = colaboradorID,
						entregables
					}
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				var resp = new
				{
					success = true,
					resp = e.Message
				};
				return Json(resp);
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarDatosNewTaskConsolidacion(int idEntregable, int semanaRelativa)
		{
			try
			{
				DateTime hoy = DateTime.Today;
				DateTime lunes = hoy;
				int diaSemana = (int)hoy.DayOfWeek;
				if(diaSemana == 0)
				{//domingo
					lunes = hoy.AddDays(-6);
				}
				else
				{
					lunes = hoy.AddDays(-1 * (diaSemana - 1));
				}

				DateTime lunesSemana = lunes.AddDays(semanaRelativa * 7);
				DateTime domingoSemana = lunesSemana.AddDays(6);

				EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(idEntregable);
				if(entregable == null)
				{
					throw new Exception("Error, no se encontró el entregable");
				}

				var datos = new
				{
					lunesSemana = lunesSemana.ToString("dd/MM/yyyy"),
					domingoSemana = domingoSemana.ToString("dd/MM/yyyy"),
					cliente = entregable.motivoTrabajo.cliente.Descripcion,
					entregable = entregable.motivoTrabajo.Codigo + "-" + entregable.Nombre
				};

				var resp = new
				{
					success = true,
					data = datos
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult NuevaTareaConsolidacion(int idColaborador, string fechaLunes, int idLugar, int idActividad, int idModulo, string detalle, int idEntregable, string horasDias, int coordinador = 0)
		{
			try
			{
				string[] fechas = fechaLunes.Split(new Char[] { '/' });
				int dia = Int32.Parse(fechas[0]);
				int mes = Int32.Parse(fechas[1]);
				int anno = Int32.Parse(fechas[2]);
				DateTime lunes = new System.DateTime(anno, mes, dia);

				//para la actualizacion de los permisos y las tareas
				List<DiaColaborador> listaDiaColaborador = new List<DiaColaborador>();
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				EntregableMotivoTrabajo entregable = db.EntregableMotivoTrabajo.Find(idEntregable);
				if(entregable == null)
				{
					throw new Exception("Error, no se encontró el entregable.");
				}

				int idCliente = entregable.motivoTrabajo.SecuencialCliente;

				var s = new JavaScriptSerializer();
				var jsonHoras = s.Deserialize<dynamic>(horasDias);
				bool entrados = false;
				for(int i = 0; i < jsonHoras.Length; i++)
				{
					dynamic valorHoras = jsonHoras[i];
					int horas = 0;
					if(Object.ReferenceEquals(valorHoras.GetType(), horas.GetType()))
					{
						horas = valorHoras;
					}
					else
					{
						horas = int.Parse(valorHoras);
					}

					if(jsonHoras[i] != null && horas > 0)
					{
						entrados = true;
						DateTime diaTarea = lunes.AddDays(i);
						DateTime diaSiguiente = diaTarea.AddDays(1);

						//Ingresar la tarea
						//Buscando la suma de las horas de las tareas del mismo día
						var tareasDelDia = (from t in db.Tarea
											where
												t.SecuencialColaborador == idColaborador &&
												t.FechaInicio >= diaTarea &&
												t.FechaInicio < diaSiguiente &&
												t.SecuencialEstadoTarea != 4
											select new
											{
												finicio = t.FechaInicio,
												ffin = t.FechaFin
											}).ToList();
						int time = 0;
						int timeMinutos = 0;
						foreach(var tarea in tareasDelDia)
						{
							TimeSpan tiempo = tarea.ffin - tarea.finicio;
							time += tiempo.Hours;
							timeMinutos += tiempo.Minutes;
						}

						DateTime fechaInicio = diaTarea.AddHours(time).AddMinutes(timeMinutos);
						fechaInicio = fechaInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
						DateTime fechaFin = fechaInicio.AddHours(horas);

						if(fechaInicio.Hour < 13 && fechaFin.Hour > 13)
						{
							fechaFin = fechaFin.AddHours(1);
						}
						else if(fechaInicio.Hour == 13)
						{
							fechaInicio = fechaInicio.AddHours(1);
							fechaFin = fechaFin.AddHours(1);
						}

						Tarea tar = new Tarea
						{
							SecuencialColaborador = idColaborador,
							SecuencialActividad = idActividad,
							SecuencialModulo = idModulo,
							SecuencialCliente = idCliente,
							SecuencialEstadoTarea = 6,
							SecuencialLugarTarea = idLugar,
							Detalle = detalle.ToUpper(),
							FechaInicio = fechaInicio,
							FechaFin = fechaFin,
							HorasUtilizadas = 0,
							NumeroVerificador = 1
						};

						//Actualizar la referencia
						tar.entregableMotivoTrabajo = entregable;

						db.Tarea.Add(tar);

						if(coordinador != 0)
						{
							db.Tarea_Coordinador.Add(
								new Tarea_Coordinador
								{
									tarea = tar,
									SecuencialColaborador = coordinador,
									EstaActivo = 1,
									NumeroVerificador = 1
								}
							);
						}

						HistoricoTareaEstado histET = new HistoricoTareaEstado
						{
							tarea = tar,
							SecuencialEstadoTarea = 6,
							FechaOperacion = DateTime.Now,
							usuario = user
						};

						db.HistoricoTareaEstado.Add(histET);

						//Para la actualizacion de las tareas y de los permisos
						listaDiaColaborador.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tar.SecuencialColaborador });
						listaCambiosTareas.Add(new DiaColaborador { Fecha = diaTarea, IdColaborador = tar.SecuencialColaborador });
					}
				}

				if(!entrados)
				{
					throw new Exception("No se insertaron tareas porque debe especificar al menos una hora en la semana.");
				}

				db.SaveChanges();

				//Actualizando las fechas de los dias que existieron los cambios y en las fechas
				foreach(DiaColaborador diaColab in listaDiaColaborador)
				{
					Utiles.OrdenarTareasPermisos(diaColab.Fecha, diaColab.IdColaborador, user, db);
				}

				//Actualizando la IU en la actualizacion de la tarea                
				ActualizarTDTarea(listaCambiosTareas);

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
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarProximosEntregables(int pagina = 1, int length = 10, string desde = "", string hasta = "", int cliente = 0)
		{
			try
			{
				int start = (pagina - 1) * length;

				DateTime fechaDesde = new DateTime(1, 1, 1);
				DateTime fechaHasta = new DateTime(9999, 12, 31);
				if(desde != "")
				{
					fechaDesde = Utiles.strToDateTime(desde);
				}
				if(hasta != "")
				{
					fechaHasta = Utiles.strToDateTime(hasta);
				}

				var entregables = (from ent in db.EntregableMotivoTrabajo
								   where ent.Avance < 100 && ent.FechaFin >= fechaDesde && ent.FechaFin <= fechaHasta && ent.EstaActivo == 1
								   orderby ent.FechaFin ascending
								   select new
								   {
									   id = ent.Secuencial,
									   entregable = ent.Nombre,
									   trabajo = ent.motivoTrabajo.Nombre + "(" + ent.motivoTrabajo.Codigo + ")",
									   cliente = ent.motivoTrabajo.cliente.Descripcion,
									   idCliente = ent.motivoTrabajo.SecuencialCliente,
									   colaboradores = (from tar in ent.tarea
														select tar.colaborador.persona.Nombre1 + " " + tar.colaborador.persona.Apellido1).Distinct().ToList(),
									   fechaInicio = ent.FechaInicio,
									   fechaFin = ent.FechaFin
								   }).ToList();
				if(cliente != 0)
					entregables = entregables.Where(x => x.idCliente == cliente).ToList();

				int cantidad = entregables.Count();
				int cantPaginas = (int)Math.Ceiling((double)cantidad / length);
				entregables = entregables.Skip(start).Take(length).ToList();

				var entregablesMostrar = (from ent in entregables
										  select new
										  {
											  id = ent.id,
											  entregable = ent.entregable,
											  trabajo = ent.trabajo,
											  cliente = ent.cliente,
											  colaboradores = string.Join(",", ent.colaboradores),
											  fechaInicio = ent.fechaInicio.ToString("dd/MM/yyyy"),
											  fechaFin = ent.fechaFin.ToString("dd/MM/yyyy"),
											  terminaEn = (ent.fechaFin.Date - DateTime.Today.Date).Days,
											  clase = (ent.fechaFin.Date - DateTime.Today.Date).Days < 0 ? "entregable-atrazado" : (ent.fechaFin.Date - DateTime.Today.Date).Days <= 7 ? "entregable-proximo" : "entregable-normal"
										  }).ToList();

				var resp = new
				{
					success = true,
					entregables = entregablesMostrar,
					cantPaginas = cantPaginas
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarAsignacionesSemanaEntregable(int idColaborador, int posSemana, int idEntregable)
		{
			try
			{
				DateTime hoy = DateTime.Today;
				DateTime lunes = hoy;
				int diaSemana = (int)hoy.DayOfWeek;
				if(diaSemana == 0)
				{//domingo
					lunes = hoy.AddDays(-6);
				}
				else
				{
					lunes = hoy.AddDays(-1 * (diaSemana - 1));
				}

				DateTime lunesSemana = lunes.AddDays(posSemana * 7);
				DateTime proximaSemana = lunesSemana.AddDays(7);

				var tareasI = (from t in db.Tarea
							   where t.SecuencialColaborador == idColaborador &&
								  t.entregableMotivoTrabajo.Secuencial == idEntregable &&
								  t.FechaInicio > lunesSemana && t.FechaFin < proximaSemana &&
								  t.SecuencialEstadoTarea != 4
							   select new
							   {
								   id = t.Secuencial,
								   actividad = t.actividad.Codigo,
								   modulo = t.modulo.Codigo,
								   fechaInicio = t.FechaInicio,
								   fechafin = t.FechaFin
							   }).ToList();


				var tareas = (from t in tareasI
							  select new
							  {
								  id = t.id,
								  fecha = t.fechaInicio.ToString("dd/MM/yyyy"),
								  actividad = t.actividad,
								  modulo = t.modulo,
								  horas = Utiles.DarHorasTarea(t.fechaInicio, t.fechafin)
							  }).ToList();

				string email = db.Colaborador.Find(idColaborador).persona.usuario.FirstOrDefault().Email;
				var userColaborador = email.Split(new char[1] { '@' })[0];


				var asignacionesAgrupadas = (from t in tareas
											 group t by t.fecha into grupoFecha
											 select new
											 {
												 fecha = grupoFecha.Key,
												 grupos = (from f in grupoFecha.ToList()
														   group f by f.modulo into grupoMod
														   select new
														   {
															   modulo = grupoMod.Key,
															   actividades = (from g in grupoMod.ToList()
																			  group g by g.actividad into grupoAct
																			  select new
																			  {
																				  fecha = grupoFecha.Key,
																				  modulo = grupoMod.Key,
																				  actividad = grupoAct.Key,
																				  horas = grupoAct.Sum(x => x.horas)
																			  }).ToList()
														   }).ToList()
											 }).ToList();


				List<object> actividades = new List<object>();

				foreach(var asig in asignacionesAgrupadas)
				{
					foreach(var grupo in asig.grupos)
					{
						foreach(var act in grupo.actividades)
						{
							actividades.Add(act);
						}
					}
				}

				var resp = new
				{
					success = true,
					emailColaborador = userColaborador,
					actividades = actividades
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

		//--------------  DISPONIBILIDAD DE LOS RECURSOS ------------
		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult VerDisponibilidadRecursos(int varSemanas = 0, string filtro = "")
		{
			try
			{
				int cantSemanas = 14;
				DateTime hoy = DateTime.Now.Date;
				DateTime lunes = hoy;
				int diaSemana = (int)hoy.DayOfWeek;
				if(diaSemana == 0)//Domingo
				{
					lunes = hoy.AddDays(-6);
				}
				else
				{
					lunes = hoy.AddDays(-1 * (diaSemana - 1));
				}

				//Desplazandoce en la fecha
				lunes = lunes.AddDays(varSemanas * 7);
				DateTime lunesAux = lunes;//Variable auxiliar para comparaciones relativas

				List<object> semanas = new List<object>();
				for(int i = 0; i < cantSemanas; i++)//Catorce semanas
				{
					DateTime domingo = lunes.AddDays(6);
					List<object> dias = new List<object>();
					for(int j = 0; j < 7; j++)
					{
						dias.Add(new
						{
							fecha = lunes.AddDays(j).ToString("dd/MM/yyyy"),
							mes = lunes.AddDays(j).ToString("MMMM").ToUpper()
						});
					}

					var semana = new
					{
						lunes = lunes.ToString("dd"),
						lunesFecha = lunes.ToString("dd/MM/yyyy"),
						domingo = domingo.ToString("dd"),
						domingoFecha = domingo.ToString("dd/MM/yyyy"),
						dias = dias
					};
					semanas.Add(semana);
					lunes = lunes.AddDays(7);
				}

				//Buncando los días de trabajo por cada uno de los colaboradores                
				var tareasColaboradores = (from colab in db.Colaborador
										   join
				   pers in db.Persona on colab.persona equals pers
										   join
user in db.Usuario on pers.Secuencial equals user.SecuencialPersona
										   join
dep in db.Departamento on colab.departamento equals dep
										   join
tar in db.Tarea on colab.Secuencial equals tar.SecuencialColaborador
										   where user.EstaActivo == 1 && dep.Asignable == 1 && tar.SecuencialEstadoTarea != 4 &&
												 tar.FechaInicio >= lunesAux && tar.FechaInicio < lunes
										   select new
										   {
											   id = tar.Secuencial,
											   type = "tar",
											   idColaborador = colab.Secuencial,
											   nombre = pers.Nombre1 + " " + pers.Apellido1,
											   email = user.Email,
											   fechaI = tar.FechaInicio,
											   fechaF = tar.FechaFin
										   }
										   ).ToList();

				//Buscando las vacaciones por cada unos de los trabajadores
				var vacaciones = (from colab in db.Colaborador
								  join
		  pers in db.Persona on colab.persona equals pers
								  join
user in db.Usuario on pers.Secuencial equals user.SecuencialPersona
								  join
dep in db.Departamento on colab.departamento equals dep
								  join
vac in db.Vacaciones on colab.Secuencial equals vac.SecuencialColaborador
								  where user.EstaActivo == 1 && dep.Asignable == 1 && vac.Fecha >= lunesAux && vac.Fecha < lunes
								  select new
								  {
									  id = vac.Secuencial,
									  type = "vac",
									  idColaborador = colab.Secuencial,
									  nombre = pers.Nombre1 + " " + pers.Apellido1,
									  email = user.Email,
									  fechaI = vac.Fecha,
									  fechaF = vac.Fecha
								  }
								  ).ToList();

				tareasColaboradores.AddRange(vacaciones);

				var colaboradores = (from colab in db.Colaborador
									 where colab.departamento.Asignable == 1 &&
										   colab.persona.usuario.FirstOrDefault().EstaActivo == 1
									 orderby colab.persona.Nombre1, colab.persona.Apellido1
									 select new
									 {
										 idColaborador = colab.Secuencial,
										 nombre = colab.persona.Nombre1 + " " + colab.persona.Apellido1,
										 email = colab.persona.usuario.FirstOrDefault().Email
									 }).ToList();
				if(filtro != "")
				{
					colaboradores = colaboradores.Where(x => x.nombre.ToLower().Contains(filtro.ToLower()) || x.email.ToLower().Contains(filtro.ToLower())).ToList();
				}

				List<object> dataDisponibilidad = new List<object>();
				foreach(var colaborador in colaboradores)
				{
					List<object> lineaAsignacion = new List<object>();
					int longitud = 0;
					bool asignadoAnterior = false;
					string typeAnterior = "";
					DateTime fechaInicial = lunesAux;
					DateTime fechaAsignacionI = fechaInicial;
					while(fechaInicial < lunes)
					{
						DateTime fechaFinal = fechaInicial.AddDays(1);
						var tarea = tareasColaboradores.Where(x => x.idColaborador == colaborador.idColaborador && x.fechaI >= fechaInicial && x.fechaI < fechaFinal).FirstOrDefault();
						if(tarea != null)
						{//Tiene tareas
							asignadoAnterior = true;
							string typeActual = tarea.type;
							if(typeActual == typeAnterior)
							{//Se mantiene el mismo type
								longitud++;
							}
							else
							{//Cambio de type pero se mantienen las asignaciones
								if(typeAnterior == "no-asignado")
								{
									lineaAsignacion.Add(new
									{
										asignado = false,
										clase = typeAnterior,
										length = longitud,
										fechaI = fechaAsignacionI.ToString("dd/MM/yyyy"),
										fechaF = fechaInicial.AddDays(-1).ToString("dd/MM/yyyy")
									});
								}
								else
								{
									lineaAsignacion.Add(new
									{
										asignado = true,
										clase = typeAnterior,
										length = longitud,
										fechaI = fechaAsignacionI.ToString("dd/MM/yyyy"),
										fechaF = fechaInicial.AddDays(-1).ToString("dd/MM/yyyy")
									});
								}
								typeAnterior = typeActual;
								longitud = 1;
								fechaAsignacionI = fechaInicial;
							}
						}
						else
						{//No tiene tareas en esta fecha
							asignadoAnterior = false;
							string typeActual = "no-asignado";
							if(typeActual == typeAnterior)
							{//Se mantiene el mismo type
								longitud++;
							}
							else
							{//Cambia a alguna asignacion                              
								lineaAsignacion.Add(new
								{
									asignado = true,
									clase = typeAnterior,
									length = longitud,
									fechaI = fechaAsignacionI.ToString("dd/MM/yyyy"),
									fechaF = fechaInicial.AddDays(-1).ToString("dd/MM/yyyy")
								});

								typeAnterior = typeActual;
								longitud = 1;
								fechaAsignacionI = fechaInicial;
							}
						}

						fechaInicial = fechaInicial.AddDays(1);
					}
					lineaAsignacion.Add(new
					{
						asignado = asignadoAnterior,
						clase = typeAnterior,
						length = longitud,
						fechaI = fechaAsignacionI.ToString("dd/MM/yyyy"),
						fechaF = fechaInicial.AddDays(-1).ToString("dd/MM/yyyy")
					});

					dataDisponibilidad.Add(new
					{
						colab = new
						{
							idColaborador = colaborador.idColaborador,
							nombre = colaborador.nombre,
							email = colaborador.email
						},
						lineaAsignacion = lineaAsignacion
					});
				}

				var resp = new
				{
					success = true,
					semanas = semanas,
					data = dataDisponibilidad,
					diferenciaDias = (DateTime.Today - lunesAux).Days
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

		//-------------- INCIDENCIAS DE COLABORADORES ---------------
		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult InsertarIncidenciaColaborador(int idColaborador, string fecha, int cliente, int tipoError, int implicacion, string hecho, string justificacion, int idIncidencia = 0, int verificador = 0)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				if(idIncidencia != 0)
				{
					ColaboradorIncidencia incidencia = db.ColaboradorIncidencia.Find(idIncidencia);
					if(incidencia == null)
					{
						throw new Exception("No se encontró la incidencia");
					}

					if(incidencia.NumeroVerificador != verificador)
					{
						throw new Exception("La incidencia ha cambiado desde que usted la abrió por favor actualice de nuevo y vuelva a intentarlo.");
					}

					incidencia.SecuencialCliente = cliente;
					incidencia.SecuencialTipoError = tipoError;
					incidencia.SecuencialImplicacionError = implicacion;
					incidencia.Puntos = db.ImplicacionError.Find(implicacion).NivelGravedad;
					incidencia.Hecho = hecho;
					incidencia.Justificacion = justificacion;
					incidencia.usuario = user;
					incidencia.FechaOperacion = DateTime.Now;
					incidencia.NumeroVerificador = verificador + 1;
				}
				else
				{
					ColaboradorIncidencia newIncidencia = new ColaboradorIncidencia
					{
						SecuencialCliente = cliente,
						SecuencialTipoError = tipoError,
						SecuencialColaborador = idColaborador,
						SecuencialImplicacionError = implicacion,
						usuario = user,
						FechaIncidencia = Utiles.strToDateTime(fecha),
						Puntos = db.ImplicacionError.Find(implicacion).NivelGravedad,
						Hecho = hecho,
						Justificacion = justificacion,
						FechaOperacion = DateTime.Now,
						EstaActiva = 1,
						NumeroVerificador = 1
					};
					db.ColaboradorIncidencia.Add(newIncidencia);
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarIncidenciasColaborador(int idColaborador, string fecha)
		{
			try
			{
				DateTime fechaI = Utiles.strToDateTime(fecha);
				var incidencias = (
									from inc in db.ColaboradorIncidencia
									where inc.FechaIncidencia == fechaI && inc.SecuencialColaborador == idColaborador && inc.EstaActiva == 1
									select new
									{
										idIncidencia = inc.Secuencial,
										idCliente = inc.SecuencialCliente,
										cliente = inc.cliente.Descripcion,
										idTipoError = inc.SecuencialTipoError,
										tipoError = inc.tipoError.Descripcion,
										idImplicacion = inc.SecuencialImplicacionError,
										implicacion = inc.implicacionError.Descripcion,
										dhecho = inc.Hecho.ToString().Substring(0, 15) + "...",
										hecho = inc.Hecho,
										justificacion = inc.Justificacion,
										verificador = inc.NumeroVerificador
									}
								  ).ToList();
				var resp = new
				{
					incidencias = incidencias,
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult EliminarIncidencia(int idIncidencia)
		{
			try
			{
				ColaboradorIncidencia incidencia = db.ColaboradorIncidencia.Find(idIncidencia);
				if(incidencia == null)
				{
					throw new Exception("Error, no se encontró la incidencia");
				}
				incidencia.EstaActiva = 0;
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult VerIncidenciasRecursos(int varSemanas = 0, string filtro = "")
		{
			try
			{
				//variables para los puntos
				int totalPuntos = 30;
				DateTime date11 = DateTime.Parse("01/01/" + DateTime.Now.Year.ToString());

				int cantSemanas = 14;
				varSemanas = (-1 * cantSemanas) + varSemanas + 1;

				DateTime hoy = DateTime.Now.Date;
				DateTime lunes = hoy;
				int diaSemana = (int)hoy.DayOfWeek;
				if(diaSemana == 0)//Domingo
				{
					lunes = hoy.AddDays(-6);
				}
				else
				{
					lunes = hoy.AddDays(-1 * (diaSemana - 1));
				}

				//Desplazandoce en la fecha
				lunes = lunes.AddDays(varSemanas * 7);
				DateTime lunesAux = lunes;//Variable auxiliar para comparaciones relativas

				List<object> semanas = new List<object>();
				for(int i = 0; i < cantSemanas; i++)//Catorce semanas
				{
					DateTime domingo = lunes.AddDays(6);
					List<object> dias = new List<object>();
					for(int j = 0; j < 7; j++)
					{
						dias.Add(new
						{
							fecha = lunes.AddDays(j).ToString("dd/MM/yyyy"),
							mes = lunes.AddDays(j).ToString("MMMM").ToUpper()
						});
					}

					var semana = new
					{
						lunes = lunes.ToString("dd"),
						lunesFecha = lunes.ToString("dd/MM/yyyy"),
						domingo = domingo.ToString("dd"),
						domingoFecha = domingo.ToString("dd/MM/yyyy"),
						dias = dias
					};
					semanas.Add(semana);
					lunes = lunes.AddDays(7);
				}

				//Buncando las incidencias de trabajo por cada uno de los colaboradores                
				var incidenciasColaboradores = (from colab in db.Colaborador
												join
													pers in db.Persona on colab.persona equals pers
												join
													user in db.Usuario on pers.Secuencial equals user.SecuencialPersona
												join
													dep in db.Departamento on colab.departamento equals dep
												join
													cincidencia in db.ColaboradorIncidencia on colab.Secuencial equals cincidencia.SecuencialColaborador
												where user.EstaActivo == 1 && dep.Asignable == 1 && cincidencia.EstaActiva == 1 &&
													  cincidencia.FechaIncidencia >= lunesAux && cincidencia.FechaIncidencia < lunes
												select new
												{
													id = cincidencia.Secuencial,
													idColaborador = cincidencia.SecuencialColaborador,
													typeError = cincidencia.tipoError.Codigo,
													implicacion = cincidencia.implicacionError.Codigo,
													nivelGravedad = cincidencia.implicacionError.NivelGravedad,
													puntos = cincidencia.Puntos,
													color = cincidencia.implicacionError.RgbColor,
													fecha = cincidencia.FechaIncidencia
												}
											   ).ToList();

				var colaboradores = (from colab in db.Colaborador
									 where colab.departamento.Asignable == 1 &&
										   colab.persona.usuario.FirstOrDefault().EstaActivo == 1
									 orderby colab.persona.Nombre1, colab.persona.Apellido1
									 select new
									 {
										 idColaborador = colab.Secuencial,
										 nombre = colab.persona.Nombre1 + " " + colab.persona.Apellido1,
										 email = colab.persona.usuario.FirstOrDefault().Email
									 }).ToList();
				if(filtro != "")
				{
					colaboradores = colaboradores.Where(x => x.nombre.ToLower().Contains(filtro.ToLower()) || x.email.ToLower().Contains(filtro.ToLower())).ToList();
				}

				List<object> dataIncidencias = new List<object>();
				foreach(var colaborador in colaboradores)
				{
					List<object> lineaIncidencias = new List<object>();
					int longitud = 0;
					DateTime fechaInicial = lunesAux;
					bool incidenciaAnterior = false;
					while(fechaInicial < lunes)
					{
						var incidencias = incidenciasColaboradores.Where(
												x => x.idColaborador == colaborador.idColaborador
											 && x.fecha == fechaInicial).OrderByDescending(x => x.nivelGravedad).ToList();

						if(incidencias.Count > 0)//Hay incidencias
						{
							if(!incidenciaAnterior)
							{
								lineaIncidencias.Add(new
								{
									type = "-",
									id = "",
									idColaborador = "",
									color = "",
									cant = "",
									nivel = "",
									fecha = "",
									length = longitud
								});
							}

							var incidencia = incidencias.FirstOrDefault();
							lineaIncidencias.Add(new
							{
								type = "I",
								id = incidencia.id,
								idColaborador = incidencia.idColaborador,
								color = incidencia.color,
								cant = incidencias.Count,
								nivel = incidencia.implicacion,
								fecha = incidencia.fecha.ToString("dd/MM/yyyy"),
								length = 1
							});
							incidenciaAnterior = true;
							longitud = 0;
						}
						else//No Hay incidencias
						{
							longitud++;
							incidenciaAnterior = false;
						}

						fechaInicial = fechaInicial.AddDays(1);
					}
					lineaIncidencias.Add(new
					{
						type = "-",
						id = "",
						idColaborador = "",
						color = "",
						cant = "",
						nivel = "",
						fecha = "",
						length = longitud
					});

					//Puntos del colaborador en este año
					int puntosColaborador = incidenciasColaboradores.Where(
												x => x.idColaborador == colaborador.idColaborador
												&& x.fecha >= date11).Select(x => x.puntos).Sum();

					dataIncidencias.Add(new
					{
						colab = new
						{
							idColaborador = colaborador.idColaborador,
							nombre = colaborador.nombre,
							email = colaborador.email,
							totalPuntos = totalPuntos + puntosColaborador
						},
						lineaIncidencias = lineaIncidencias
					});
				}

				var resp = new
				{
					success = true,
					semanas = semanas,
					data = dataIncidencias
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
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarIncidenciasDeDiaColaboradorPorIdIncidencia(int idIncidencia)
		{
			try
			{
				ColaboradorIncidencia incidencia = db.ColaboradorIncidencia.Find(idIncidencia);
				if(incidencia == null)
				{
					throw new Exception("No se encuentra la incidencia");
				}
				Persona persona = incidencia.colaborador.persona;
				string nombre = persona.Nombre1 + " " + persona.Apellido1;

				var resp = new
				{
					success = true,
					colaborador = nombre,
					fecha = incidencia.FechaIncidencia.ToString("dd/MM/yyyy"),
					datos = (
								from inc in db.ColaboradorIncidencia
								where inc.FechaIncidencia == incidencia.FechaIncidencia &&
									  inc.SecuencialColaborador == incidencia.SecuencialColaborador &&
									  inc.EstaActiva == 1
								select new
								{
									idIncidencia = inc.Secuencial,
									idCliente = inc.SecuencialCliente,
									cliente = inc.cliente.Descripcion,
									idTipoError = inc.SecuencialTipoError,
									tipoError = inc.tipoError.Descripcion,
									idImplicacion = inc.SecuencialImplicacionError,
									implicacion = inc.implicacionError.Descripcion,
									dhecho = inc.Hecho.ToString().Substring(0, 15) + "...",
									hecho = inc.Hecho,
									justificacion = inc.Justificacion,
									verificador = inc.NumeroVerificador
								}
							).ToList()
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

		//Eliminar los permisos
		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult AnularPermiso(int id)
		{
			try
			{
				Permiso permiso = db.Permiso.Find(id);
				if(permiso == null)
				{
					throw new Exception("No se encuentra el permiso en la bd.");
				}

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				permiso.SecuencialEstadoPermiso = 4;//ANULADO
				db.SaveChanges();

				Utiles.OrdenarTareasPermisos(permiso.FechaInicio.Date, permiso.SecuencialColaborador, user, db);

				//Actualizando la IU en la actualizacion de la tarea
				List<DiaColaborador> listaCambiosTareas = new List<DiaColaborador>();
				listaCambiosTareas.Add(new DiaColaborador
				{
					Fecha = permiso.FechaInicio,
					IdColaborador = permiso.SecuencialColaborador
				});
				ActualizarTDTarea(listaCambiosTareas);

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

		//------------------- MONITOREO DE TAREAS -------------------
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult MonitoreoTareas()
		{
			return View();
		}

		//---------------- FUNCIONES GENERALES ----------------------
		[HttpPost]
		[Authorize(Roles = "ADMIN, REC")]
		public ActionResult DarEntregablesMotivosTrabajo(int idCliente, bool todos = true)
		{
			try
			{
				if(todos)
				{
					var datos = (from emt in db.EntregableMotivoTrabajo
								 where emt.EstaActivo == 1 && emt.motivoTrabajo.SecuencialCliente == idCliente && emt.Avance != 100
								 select new
								 {
									 id = emt.Secuencial,
									 nombre = emt.motivoTrabajo.Codigo + "-" + emt.Nombre
								 }).ToList();

					var resp = new
					{
						success = true,
						datos = datos
					};
					return Json(resp);
				}
				else
				{
					var datos = (from emt in db.EntregableMotivoTrabajo
								 where emt.EstaActivo == 1 && emt.Avance < 100 && emt.motivoTrabajo.SecuencialCliente == idCliente
								 select new
								 {
									 id = emt.Secuencial,
									 nombre = emt.motivoTrabajo.Codigo + "-" + emt.Nombre
								 }).ToList();

					var resp = new
					{
						success = true,
						datos = datos
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
		[Authorize(Roles = "ADMIN, REC, ACTAS")]
		public ActionResult DarEntregablesParaActas(string numeroContrato)
		{
			try
			{
				var datos = (from emt in db.EntregableMotivoTrabajo
							 join mt in db.MotivoTrabajo on emt.SecuencialMotivoTrabajo equals mt.Secuencial
							 where emt.EstaActivo == 1 && mt.Codigo == numeroContrato
							 select new
							 {
								 secuencial = emt.Secuencial,
								 nombre = emt.Nombre,
								 descripcion = emt.Descripcion
							 }).ToList();

				var resp = new
				{
					success = true,
					datos = datos
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
		[Authorize(Roles = "ADMIN, LIDERES, OPERACIONES")]
		public ActionResult SubirInformesRecursos(HttpPostedFileBase file)
		{
			try
			{

				string ext = file.FileName.Split('.')[1];
				string formato = "INFORME DE RECURSOS." + ext;
				string ruta = Path.Combine(Server.MapPath("~/Web/resources/informesrecursos/"), formato);

				file.SaveAs(ruta);

				var archivo = new
				{
					name = Path.GetFileName(ruta),
					lastModified = System.IO.File.GetLastWriteTime(ruta),
					size = (new FileInfo(ruta).Length / 1024.0 / 1024.0).ToString("F2"),
					ext = ext,
					path = ruta,
				};

				var resp = new
				{
					success = true,
					msg = "Informe Recursos Guardado correctamente",
					file = archivo,
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
		[Authorize(Roles = "ADMIN, LIDERES, OPERACIONES")]
		public ActionResult DarInformesRecursos()
		{
			try
			{

				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
				Persona persona = user.persona;
				Colaborador colab = persona.colaborador.FirstOrDefault();

				//Buscando el usuario por el idtrabajador
				var roles = (from t in db.Colaborador
							 join
								 p in db.Persona on t.persona equals p
							 join
								 u in db.Usuario on p equals u.persona
							 join
								 ur in db.UsuarioRol on u.Secuencial equals ur.SecuencialUsuario
							 join
								catrol in db.Rol on ur.SecuencialRol equals catrol.Secuencial
							 where t.Secuencial == colab.Secuencial
							 select new
							 {
								 id = catrol.Secuencial,
								 rol = catrol.Codigo
							 }).ToList();

				string rootdir = Server.MapPath("~/Web/resources/informesrecursos/");

				string[] files = Directory.GetFiles(rootdir);
				var file = files.Select(f => new
				{
					fullname = Path.GetFileName(f),
					name = Path.GetFileNameWithoutExtension(f),
					ext = Path.GetExtension(f).Split('.')[1],
					path = f,
					size = (new FileInfo(f).Length / 1024.0 / 1024.0).ToString("F2"),
					lastModified = System.IO.File.GetLastWriteTime(f)
				}).FirstOrDefault();

				var resp = new
				{
					success = true,
					file = file,
					roles = roles
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
		[Authorize(Roles = "ADMIN, LIDERES, OPERACIONES")]
		public ActionResult DescargarInformesRecursos()
		{
			try
			{

				string rootdir = Server.MapPath("~/Web/resources/informesrecursos/");

				string[] files = Directory.GetFiles(rootdir);
				var file = files.Select(f => new
				{
					fullname = Path.GetFileName(f),
					name = Path.GetFileNameWithoutExtension(f),
					ext = Path.GetExtension(f).Split('.')[1],
					path = f,
					size = (new FileInfo(f).Length / 1024.0 / 1024.0).ToString("F2"),
					lastModified = System.IO.File.GetLastWriteTime(f)
				}).FirstOrDefault();

				var fs = new FileStream(file.path, FileMode.Open);
				return File(fs, "application/octet-stream", file.fullname);

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
