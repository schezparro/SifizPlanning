using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SifizPlanning.Models;
using SifizPlanning.Security;
using System.Web.Script.Serialization;
using SifizPlanning.Util;

namespace SifizPlanning.Controllers
{
	public class HomeController : Controller
	{
		SifizPlanningEntidades db = DbCnx.getCnx();
		//
		// GET: /Home/
		[AllowAnonymous]
		public ActionResult Index()
		{
			return View();
		}

		//Para el Login
		[HttpPost]
		[AllowAnonymous]
		public ActionResult Login(string email, string password)
		{
			string emailSifiz = email + "@sifizsoft.com";

			Algorithm alg = Algorithm.SHA1;
			string pass = Checksum.CalculateStringHash(password, alg);

			string msg = "";
			if(ModelState.IsValid)
			{
				var user = db.Usuario.FirstOrDefault(x => x.Email == email && x.Passw == pass && x.EstaActivo == 1);
				if(user == null)
				{
					user = db.Usuario.FirstOrDefault(x => x.Email == emailSifiz && x.Passw == pass && x.EstaActivo == 1);

					if(user == null)
					{
						msg = "Usuario o contraseña incorrectas.";
						var result1 = new
						{
							success = false,
							msg = msg
						};
						return Json(result1);
					}
				}
				FormsAuthentication.SetAuthCookie(user.Email, false);
				var result2 = new
				{
					success = true,
					msg = "AUTENTICADO"
				};
				return Json(result2);
			}

			var result = new
			{
				success = false,
				msg = "Error del servidor"
			};
			return Json(result);
		}

		[AllowAnonymous]
		public ActionResult RedirectUser()
		{
			return RedirectToRoute("MainMenu");
		}

		[Authorize]
		public ActionResult MenuPrincipal()
		{
			string[] roles = Roles.GetRolesForUser();

			List<SysMenu> modulos = (from sm in db.SysMenu
									 join
										 rsm in db.RolSysMenu on sm.Secuencial equals rsm.SecuencialSysMenu
									 join
										 r in db.Rol on rsm.rol equals r
									 where roles.Contains(r.Codigo) && sm.EstaActivo == 1 && rsm.EstaActivo == 1
									 select sm).Distinct<SysMenu>().ToList<SysMenu>();

			//List<SysMenu> modulos = db.SysMenu.Where(x => x.EstaActivo == 1).ToList();

			ViewBag.Modulos = modulos;

			return View();
		}

		[HttpPost]
		[Authorize]
		public ActionResult CambiarPassword(string p, string p1, string p2)
		{
			if(p1 != p2)
			{
				var resp = new
				{
					success = false,
					msg = "Las nuevas contraseñas no coinciden"
				};
				return Json(resp);
			}
			string emailUser = User.Identity.Name;
			Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

			Algorithm alg = Algorithm.SHA1;
			string sha1P = Checksum.CalculateStringHash(p, alg);
			string sha1P1 = Checksum.CalculateStringHash(p1, alg);
			string sha1P2 = Checksum.CalculateStringHash(p2, alg);

			if(user.Passw != sha1P)
			{
				var resp = new
				{
					success = false,
					msg = "Las contraseña actual proporcionada no es correcta"
				};
				return Json(resp);
			}

			user.Passw = sha1P1;
			db.SaveChanges();

			var respf = new
			{
				success = true,
				msg = "Se ha cambido la contraseña"
			};
			return Json(respf);
		}

		[Authorize]
		public ActionResult CerrarSesion()
		{
			FormsAuthentication.SignOut();
			Session.Abandon();

			// clear authentication cookie
			HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
			cookie1.Expires = DateTime.Now.AddYears(-1);
			Response.Cookies.Add(cookie1);

			//// clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
			//HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
			//cookie2.Expires = DateTime.Now.AddYears(-1);
			//Response.Cookies.Add(cookie2);

			//return RedirectToRoute("Home");

			//FormsAuthentication.SignOut();
			//Session.Abandon();
			//Session.Clear();
			//foreach (var cookie in Request.Cookies.AllKeys)
			//{
			//    Request.Cookies.Remove(cookie);
			//}
			//foreach (var cookie in Response.Cookies.AllKeys)
			//{
			//    Response.Cookies.Remove(cookie);
			//}

			return RedirectToRoute("home");
		}

		[AllowAnonymous]
		public ActionResult Error(string mensaje)
		{
			@ViewBag.mensaje = mensaje;
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarComentariosNoLeidos()
		{
			string emailUser = User.Identity.Name;
			Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

			var comentariosNoLeidos = (from cg in db.ComentarioGeneral
									   join u in db.Usuario on cg.SecuencialUsuario equals u.Secuencial
									   where
											 db.UsuarioLeeComentarioGeneral.Where(x => x.SecuencialUsuario == user.Secuencial)
											 .Select(x => x.SecuencialComentarioGeneral).ToList().Contains(cg.Secuencial) == false
									   orderby cg.FechaHora descending
									   select new
									   {
										   idComentarioGeneral = cg.Secuencial,
										   nombreusuario = cg.usuario.persona.Nombre1.Substring(0, 1).ToUpper() + cg.usuario.persona.Apellido1.ToUpper(),
										   idUsuario = u.Secuencial,
										   fechahora = cg.FechaHora.ToString().Substring(0, 16),
										   tipocomentario = cg.TipoComentario,
										   textoEncabezado = "",
										   textocomentario = cg.Comentario,
										   idTicket = (cg.Comentario.IndexOf("Ticket: <b>") != -1) ? cg.Comentario.Substring(cg.Comentario.IndexOf("Ticket: <b>") + 11, 6) : "",
										   importancia = (cg.Importancia.ToLower() == "normal") ? "success" : ((cg.Importancia.ToLower() == "importante") ? "warning" : "danger"),
										   tipo = (cg.Importancia.ToLower() == "normal") ? "1" : ((cg.Importancia.ToLower() == "importante") ? "2" : "3")
									   }
									).ToList();

			var resp = new
			{
				success = true,
				comentariosDeTareas = comentariosNoLeidos.Where(x => x.tipocomentario == "TAREA").ToList(),
				comentariosDeTickets = comentariosNoLeidos.Where(x => x.tipocomentario == "NOTIFICACION" || x.tipocomentario == "DESACUERDO").ToList()
			};
			return Json(resp);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ActionResult DarTodosLosComentarios(string tipoComentario, int noLeidos = 1)
		{
			try
			{
				var resp = (noLeidos == 1) ? DarComentariosNoLeidosPorTipo(tipoComentario)
										   : DarComentariosLeidosYNoLeidosPorTipo(tipoComentario);
				return resp;
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
		public ActionResult DarComentariosNoLeidosPorTipo(string tipoComentario)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				List<string> listadoComentarioTipo = new List<string>();

				if(tipoComentario == "tareas")
				{
					listadoComentarioTipo.Add("TAREA");
				}
				else
				{ //tickets
					listadoComentarioTipo.Add("NOTIFICACION");
					listadoComentarioTipo.Add("DESACUERDO");
				}

				var comentarios = (from cg in db.ComentarioGeneral
								   join u in db.Usuario on cg.SecuencialUsuario equals u.Secuencial
								   where
										db.UsuarioLeeComentarioGeneral.Where(x => x.SecuencialUsuario == user.Secuencial)
										.Select(x => x.SecuencialComentarioGeneral).ToList().Contains(cg.Secuencial) == false
										&& listadoComentarioTipo.Contains(cg.TipoComentario)
								   select new
								   {
									   idComentarioGeneral = cg.Secuencial,
									   nombreusuario = cg.usuario.persona.Nombre1.Substring(0, 1).ToUpper() + cg.usuario.persona.Apellido1.ToUpper(),
									   idUsuario = u.Secuencial,
									   fechahora = cg.FechaHora.ToString().Substring(0, 16),
									   tipocomentario = cg.TipoComentario,
									   textoEncabezado = "",
									   textocomentario = cg.Comentario,
									   idTicket = (cg.Comentario.IndexOf("Ticket: <b>") != -1) ? cg.Comentario.Substring(cg.Comentario.IndexOf("Ticket: <b>") + 11, 6) : "",
									   importancia = (cg.Importancia.ToLower() == "normal") ? "success" : ((cg.Importancia.ToLower() == "importante") ? "warning" : "danger"),
									   tipo = (cg.Importancia.ToLower() == "normal") ? "1" : ((cg.Importancia.ToLower() == "importante") ? "2" : "3")
								   }
								  ).ToList();

				var resp = new
				{
					success = true,
					comentariosDeTareas = comentarios.Where(x => x.tipocomentario == "TAREA").ToList(),
					comentariosDeTickets = comentarios.Where(x => x.tipocomentario == "NOTIFICACION" || x.tipocomentario == "DESACUERDO").ToList()
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
		public ActionResult DarComentariosLeidosYNoLeidosPorTipo(string tipoComentario)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				List<string> listadoComentarioTipo = new List<string>();

				if(tipoComentario == "tareas")
				{
					listadoComentarioTipo.Add("TAREA");
				}
				else
				{ //tickets
					listadoComentarioTipo.Add("NOTIFICACION");
					listadoComentarioTipo.Add("DESACUERDO");
				}

				var comentarios = (from cg in db.ComentarioGeneral
								   join u in db.Usuario on cg.SecuencialUsuario equals u.Secuencial
								   where
										listadoComentarioTipo.Contains(cg.TipoComentario)
								   select new
								   {
									   idComentarioGeneral = cg.Secuencial,
									   nombreusuario = cg.usuario.persona.Nombre1.Substring(0, 1).ToUpper() + cg.usuario.persona.Apellido1.ToUpper(),
									   idUsuario = u.Secuencial,
									   fechahora = cg.FechaHora.ToString().Substring(0, 16),
									   tipocomentario = cg.TipoComentario,
									   textoEncabezado = "",
									   textocomentario = cg.Comentario,
									   idTicket = (cg.Comentario.IndexOf("Ticket: <b>") != -1) ? cg.Comentario.Substring(cg.Comentario.IndexOf("Ticket: <b>") + 11, 6) : "",
									   importancia = (cg.Importancia.ToLower() == "normal") ? "success" : ((cg.Importancia.ToLower() == "importante") ? "warning" : "danger"),
									   tipo = (cg.Importancia.ToLower() == "normal") ? "1" : ((cg.Importancia.ToLower() == "importante") ? "2" : "3")
								   }
								  ).ToList();

				var resp = new
				{
					success = true,
					comentariosDeTareas = comentarios.Where(x => x.tipocomentario == "TAREA").ToList(),
					comentariosDeTickets = comentarios.Where(x => x.tipocomentario == "NOTIFICACION" || x.tipocomentario == "DESACUERDO").ToList()
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
		public ActionResult MarcarComentarioLeido(int idComentario = 0, string tipoComentario = "", int importanciaSeleccionada = 0)
		{
			try
			{
				string emailUser = User.Identity.Name;
				Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

				if(idComentario > 0)
				{
					var comentario = db.ComentarioGeneral.Find(idComentario);
					if(comentario == null)
						throw new Exception("Error, no se encontró el comentario");

					UsuarioLeeComentarioGeneral comentarioLeido = new UsuarioLeeComentarioGeneral
					{
						FechaHora = DateTime.Now,
						SecuencialComentarioGeneral = comentario.Secuencial,
						SecuencialUsuario = user.Secuencial,
						Leido = true
					};
					db.UsuarioLeeComentarioGeneral.Add(comentarioLeido);
				}
				else
				{
					string importanciaSelecc = importanciaSeleccionada == 1 ? "normal" : importanciaSeleccionada == 2 ? "importante" : "muy importante";
					//string tipoComent = tipoComentario.ToLower() == "tareas" ? "TAREA" : tipoComentario.ToLower() == "tickets" ? "NOTIFICACION" : "DESACUERDO";

					List<string> listadoComentarioTipo = new List<string>();

					if(tipoComentario == "tareas")
					{
						listadoComentarioTipo.Add("TAREA");
					}
					else
					{ //tickets
						listadoComentarioTipo.Add("NOTIFICACION");
						listadoComentarioTipo.Add("DESACUERDO");
					}

					var comentariosNoLeidos = (from cg in db.ComentarioGeneral
											   join u in db.Usuario on cg.SecuencialUsuario equals u.Secuencial
											   where
													 db.UsuarioLeeComentarioGeneral.Where(x => x.SecuencialUsuario == user.Secuencial)
													 .Select(x => x.SecuencialComentarioGeneral).ToList().Contains(cg.Secuencial) == false
													 && cg.Importancia.ToLower() == importanciaSelecc
													 && listadoComentarioTipo.Contains(cg.TipoComentario)
											   select new
											   {
												   idComentarioGeneral = cg.Secuencial
											   }
											).ToList();



					foreach(var comentario in comentariosNoLeidos)
					{
						UsuarioLeeComentarioGeneral comentarioLeido = new UsuarioLeeComentarioGeneral
						{
							FechaHora = DateTime.Now,
							SecuencialComentarioGeneral = comentario.idComentarioGeneral,
							SecuencialUsuario = user.Secuencial,
							Leido = true
						};
						db.UsuarioLeeComentarioGeneral.Add(comentarioLeido);
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

	}
}
