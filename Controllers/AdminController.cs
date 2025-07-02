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

using SifizPlanning.Controllers;
using System.Threading.Tasks;

namespace SifizPlanning.Controllers
{
    public class AdminController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();
        //
        // GET: /Admin/

        [Authorize(Roles = "ADMIN")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DarModulosReportes()
        {
            try
            {
                var data = (from r in db.Reporte
                            where r.EstaActivo == 1
                            select r.Modulo).Distinct().ToList();

                return Json(new
                {
                    success = true,
                    datos = data
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarModulosReportes] Error al devolrver modulos reportes: {e.Message}");
                return Json(new
                {
                    success = false,
                    datos = new { }
                });
            }

        }


        public ActionResult DarReportes(string modulo)
        {
            try
            {
                var data = (from r in db.Reporte
                            where r.Modulo.Equals(modulo) && r.EstaActivo == 1
                            select new
                            {
                                nombre = r.Nombre,
                                url = r.Url
                            }).ToList();

                return Json(new
                {
                    success = true,
                    datos = data
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarReportes] Error al devolver reportes: {e.Message}");
                return Json(new
                {
                    success = false,
                    datos = new { }
                });
            }

        }

        //-----------CORREOS NO ENVIADOS-----------

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarCorreosNoEnviados(string fIni = "", string fFin = "")
        {
            try
            {
                var correosNoEnviados = (from c in db.CorreoNoEnviado
                                         where c.EstaActivo == true
                                         orderby c.FechaHora descending
                                         select new
                                         {
                                             idcorreo = c.Secuencial,
                                             fechahora = c.FechaHora.ToString().Substring(0, 19),
                                             emailFuente = c.EmailEnvia,
                                             emailsDestinos = c.EmailDestinos,
                                             asunto = c.MensajeError,
                                             emailBody = c.Texto,
                                             password = c.Password,
                                             adjuntarimagen = c.AdjuntarImagen,
                                             imagenes = c.Imagenes,
                                             adjuntos = c.Adjuntos
                                         }
                                ).ToList();

                correosNoEnviados = correosNoEnviados.Take(200).ToList();

                var resp = new
                {
                    success = true,
                    correosNoEnviados = correosNoEnviados
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarCorreosNoEnviados] Error al devolver correos no enviados: {e.Message}");
                return Json(new
                {
                    success = false,
                    datos = new { }
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult ReenviarCorreo(string emailFuente, string emailsDestinos, string emailBody, int idCorreo, string asunto = "Información", string password = "", bool adjuntarimagen = false, string imagenes = null, string adjuntos = null)
        {
            try
            {
                var emailsDestinosArray = new string[1] { emailsDestinos };
                if (emailsDestinos.IndexOf('|') >= 0)
                    emailsDestinosArray = emailsDestinos.Split('|');

                var imagenesArray = new string[1] { imagenes };
                if (string.IsNullOrEmpty(imagenes))
                    imagenesArray = null;
                else
                {
                    if (imagenes.IndexOf('|') >= 0)
                        imagenesArray = imagenes.Split('|');
                }

                var adjuntosArray = new string[1] { adjuntos };
                if (string.IsNullOrEmpty(adjuntos))
                    adjuntosArray = null;
                else
                {
                    if (adjuntos.IndexOf('|') >= 0)
                        adjuntosArray = adjuntos.Split('|');
                }

                Utiles.EnviarEmail(emailFuente, emailsDestinosArray, emailBody, asunto, Utiles.DesencriptacionSimetrica(password), adjuntarimagen, imagenesArray, adjuntosArray, idCorreo);

                var resp = new
                {
                    success = true
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[ReenviarCorreo] Error al reenviar correo: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al reenviar correo."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DesactivarCorreo(int idCorreo)
        {
            try
            {
                var objCorreoNoEnviado = db.CorreoNoEnviado.Find(idCorreo);

                if (objCorreoNoEnviado != null)
                {
                    objCorreoNoEnviado.EstaActivo = false;
                    db.SaveChanges();
                }

                var resp = new
                {
                    success = true
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[ReenviarCorreo] Error al desactivar correo: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al desactivar correo."
                };
                return Json(resp);
            }
        }

        //---------------TRABAJADORES--------------

        [HttpPost]
        [Authorize(Roles = "ADMIN, RRHH")]
        public ActionResult DarTrabajadores(int start = 0, int limit = 0, string filtro = "", string filtroColumna = "")
        {
            try
            {

                var stemp = new JavaScriptSerializer();
                var jsonObj = stemp.Deserialize<dynamic>(filtroColumna);
                string filtroNombre = "";
                string filtroEmail = "";
                string filtroDepartamento = "";
                string filtroCargo = "";
                string filtroSede = "";
                string filtroUsuario = "";
                string filtroPublicacionesDosCinco = "";
                if (jsonObj != null)
                {
                    filtroNombre = jsonObj["nombre"];
                    filtroEmail = jsonObj["email"];
                    filtroDepartamento = jsonObj["departamento"];
                    filtroCargo = jsonObj["cargo"];
                    filtroSede = jsonObj["sede"];
                    filtroUsuario = jsonObj["usuario"];
                    filtroPublicacionesDosCinco = jsonObj["publicacionesDosCinco"];
                }

                var trabajadores = (from p in db.Persona
                                    join
                    u in db.Usuario on p equals u.persona
                                    join t in db.Colaborador on p equals t.persona
                                    join c in db.Cargo on t.cargo equals c
                                    join s in db.Sede on t.sede equals s
                                    join f in db.FotoColaborador on t equals f.colaborador
                                    join d in db.Departamento on t.departamento equals d
                                    orderby p.Apellido1, p.Apellido2, p.Nombre1, p.Nombre2
                                    select new
                                    {
                                        idtrabajador = t.Secuencial,
                                        url = f.Url,
                                        nombre = p.Apellido1 + " " + p.Apellido2 + " " + p.Nombre1 + " " + p.Nombre2,
                                        email = u.Email,
                                        userAct = u.EstaActivo == 1 ? "SI" : "NO",
                                        cargo = c.Descripcion,
                                        sede = s.Descripcion,
                                        publicacionesDosCinco = t.PublicacionesDosCinco.HasValue ? t.PublicacionesDosCinco.Value == true ? "SI" : "NO" : "NO",
                                        departamento = d.Descripcion
                                    }
                                     ).ToList();

                if (filtro != "")
                {
                    trabajadores = (from p in db.Persona
                                    join
                                        u in db.Usuario on p equals u.persona
                                    join
                                        t in db.Colaborador on p equals t.persona
                                    join
                                        c in db.Cargo on t.cargo equals c
                                    join
                                        s in db.Sede on t.sede equals s
                                    join
                                        f in db.FotoColaborador on t equals f.colaborador
                                    join
                                        d in db.Departamento on t.departamento equals d
                                    where p.Nombre1.ToString().Contains(filtro) ||
                                          p.Nombre2.ToString().Contains(filtro) ||
                                          p.Apellido1.ToString().Contains(filtro) ||
                                          p.Apellido2.ToString().Contains(filtro) ||
                                          u.Email.ToString().Contains(filtro) ||
                                          c.Descripcion.ToString().Contains(filtro) ||
                                          d.Descripcion.ToString().Contains(filtro) ||
                                          s.Descripcion.ToString().Contains(filtro)
                                    orderby p.Apellido1, p.Apellido2, p.Nombre1, p.Nombre2
                                    select new
                                    {
                                        idtrabajador = t.Secuencial,
                                        url = f.Url,
                                        nombre = p.Apellido1 + " " + p.Apellido2 + " " + p.Nombre1 + " " + p.Nombre2,
                                        email = u.Email,
                                        userAct = u.EstaActivo == 1 ? "SI" : "NO",
                                        cargo = c.Descripcion,
                                        sede = s.Descripcion,
                                        publicacionesDosCinco = t.PublicacionesDosCinco.HasValue ? t.PublicacionesDosCinco.Value == true ? "SI" : "NO" : "NO",
                                        departamento = d.Descripcion
                                    }
                                    ).ToList();
                }

                if (filtroNombre != "")
                {
                    trabajadores = (from c in trabajadores
                                    where c.nombre.ToString().ToLower().Contains(filtroNombre.ToLower())
                                    select c).ToList();
                }

                if (filtroEmail != "")
                {
                    trabajadores = (from c in trabajadores
                                    where c.email.ToString().ToLower().Contains(filtroEmail.ToLower())
                                    select c).ToList();
                }
                if (filtroDepartamento != "")
                {
                    trabajadores = (from c in trabajadores
                                    where c.departamento.ToString().ToLower().Contains(filtroDepartamento.ToLower())
                                    select c).ToList();
                }
                if (filtroCargo != "")
                {
                    trabajadores = (from c in trabajadores
                                    where c.cargo.ToString().ToLower().Contains(filtroCargo.ToLower())
                                    select c).ToList();
                }
                if (filtroSede != "")
                {
                    trabajadores = (from c in trabajadores
                                    where c.sede.ToString().ToUpper().Contains(filtroSede.ToUpper())
                                    select c).ToList();
                }
                if (filtroUsuario != "")
                {
                    trabajadores = (from c in trabajadores
                                    where c.userAct.ToString().ToLower().Contains(filtroUsuario.ToLower())
                                    select c).ToList();
                }

                if (filtroPublicacionesDosCinco != "")
                {
                    trabajadores = (from c in trabajadores
                                    where c.publicacionesDosCinco.ToString().ToLower().Contains(filtroPublicacionesDosCinco.ToLower())
                                    select c).ToList();
                }

                var resp = new
                {
                    success = true,
                    trabajadores = trabajadores
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarTrabajadores] Error al dar trabajadores: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al dar trabajadores."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarRolTrabajador(int idTrabajador)
        {
            try
            {

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
                             where t.Secuencial == idTrabajador
                             select new
                             {
                                 id = catrol.Secuencial,
                                 rol = catrol.Codigo
                             }).ToList();

                DateTime ahora = DateTime.Now;
                UsuarioTFS usuarioTFS = db.UsuarioTFS.Where(x => x.SecuencialColaborador == idTrabajador && x.FechaInicio <= ahora && x.FechaFin >= ahora).FirstOrDefault();
                string userTfs = "";
                if (usuarioTFS != null)
                {
                    userTfs = usuarioTFS.Usuario;
                }
                var resp = new
                {
                    success = true,
                    roles = roles,
                    usuarioTFS = userTfs
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarRolTrabajador] Error al dar rol de trabajadores: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al dar rol de trabajadores."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult NuevoTrabajador(string nombre1, string apellido1, string sexo, string fechaNac, int nacionalidad, string email, int cargo, int sede, bool? publicacionesDosCinco, int departamento, DateTime fechaIngreso, HttpPostedFileBase foto = null, string nombre2 = "", string apellido2 = "", int idTrabajador = 0)
        {
            try
            {
                string[] fechas = fechaNac.Split(new Char[] { '/' });
                int dia = Int32.Parse(fechas[0]);
                int mes = Int32.Parse(fechas[1]);
                int anno = Int32.Parse(fechas[2]);

                if (idTrabajador > 0)//Editando la persona
                {
                    //Buscando el trabajador
                    var trab = (from t in db.Colaborador
                                where t.Secuencial == idTrabajador
                                select t).SingleOrDefault();
                    //Buscando la persona
                    var per = (from p in db.Persona
                               where p.Secuencial == trab.SecuencialPersona
                               select p).SingleOrDefault();
                    //Buscando el usuario
                    var user = (from u in db.Usuario
                                where u.SecuencialPersona == per.Secuencial
                                select u).SingleOrDefault();
                    //Buscando la foto
                    var fotoTrab = (from f in db.FotoColaborador
                                    where f.SecuencialColaborador == idTrabajador
                                    select f).SingleOrDefault();

                    //Actualizando los datos
                    trab.SecuencialCargo = cargo;
                    trab.SecuencialSede = sede;
                    trab.SecuencialDepartamento = departamento;
                    trab.PublicacionesDosCinco = publicacionesDosCinco == true ? true : false;
                    trab.FechaIngreso = fechaIngreso;

                    per.Nombre1 = nombre1.ToUpper();
                    per.Nombre2 = nombre2.ToUpper();
                    per.Apellido1 = apellido1.ToUpper();
                    per.Apellido2 = apellido2.ToUpper();
                    per.Sexo = sexo;
                    per.FechaNac = new System.DateTime(anno, mes, dia);
                    per.SecuencialPais = nacionalidad;

                    user.Email = email;

                    //actualizando la foto
                    if (foto != null)//Viene la foto
                    {
                        if (fotoTrab.Url != "workers/no-foto.jpg")//Hay que borrar un fichero
                        {
                            var file = Path.Combine(Server.MapPath("~/Web/images/workers"), fotoTrab.Url);
                            if (System.IO.File.Exists(file))
                                System.IO.File.Delete(file);
                        }
                        //Subiendo la nueva foto
                        string extFile = Path.GetExtension(foto.FileName);
                        string newNameFile = Utiles.RandomString(10) + extFile;
                        string path = Path.Combine(Server.MapPath("~/Web/images/workers"), newNameFile);
                        foto.SaveAs(path);//Renombrado y Salvado
                        fotoTrab.Url = "workers/" + newNameFile;
                    }

                    db.SaveChanges();//Salvando los cambios

                    string msg = "Se ha actualizado correctamente el trabajador";
                    if (sexo != "M")
                        msg = "Se ha actualizado correctamente la trabajadora";

                    var resp = new
                    {
                        success = true,
                        msg = msg
                    };
                    return Json(resp);
                }
                else//Una nueva persona
                {
                    //Entrando la Persona.
                    Persona newp = new Persona
                    {
                        Nombre1 = nombre1.ToUpper(),
                        Nombre2 = nombre2.ToUpper(),
                        Apellido1 = apellido1.ToUpper(),
                        Apellido2 = apellido2.ToUpper(),
                        Sexo = sexo.ToUpper(),
                        FechaNac = new System.DateTime(anno, mes, dia),
                        SecuencialPais = nacionalidad,
                        NumeroVerificador = 1
                    };
                    db.Persona.Add(newp);

                    //Entrando el usuario
                    Usuario user = new Usuario
                    {
                        persona = newp,
                        Passw = Utiles.RandomString(8),
                        EstaActivo = 1,
                        Email = email,
                        NumeroVerificador = 1
                    };
                    db.Usuario.Add(user);

                    //Entrando el trabajador
                    Colaborador trab = new Colaborador
                    {
                        SecuencialCargo = cargo,
                        persona = newp,
                        SecuencialSede = sede,
                        SecuencialDepartamento = departamento,
                        PublicacionesDosCinco = publicacionesDosCinco,
                        NumeroVerificador = 1,
                        FechaIngreso = fechaIngreso
                    };
                    db.Colaborador.Add(trab);

                    //Procesando el fichero
                    string url = "workers/no-foto.jpg";
                    if (foto != null)
                    {
                        string fileName = Path.GetFileName(foto.FileName);
                        if (fileName != null)
                        {
                            string extFile = Path.GetExtension(foto.FileName);
                            string newNameFile = Utiles.RandomString(10) + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/images/workers"), newNameFile);
                            foto.SaveAs(path);//Renombrado y Salvado
                            url = "workers/" + newNameFile;
                        }
                    }
                    //Entrando el fichero a la BD
                    FotoColaborador fotoTrab = new FotoColaborador
                    {
                        Url = url,
                        colaborador = trab,
                        NumeroVerificador = 1
                    };
                    db.FotoColaborador.Add(fotoTrab);

                    db.SaveChanges();//Salvando los cambios

                    string msg = "Se ha registrado correctamente el trabajador";
                    if (sexo != "M")
                        msg = "Se ha registrado correctamente la trabajadora";

                    var resp = new
                    {
                        success = true,
                        msg = msg
                    };
                    return Json(resp);
                }
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[NuevoTrabajador] Error al dar nuevo trabajador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al dar nuevo trabajador."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult EditarUsuario(int idTrabajador, int activo, string jsonRoles = "", string passw1 = "", string passw2 = "", string usuarioTFS = "")
        {
            string emailUser = User.Identity.Name;
            LoggerManager.LogInfo($"[EditarUsuario] Usuario {emailUser} solicitó editar usuario.");

            if (passw1 != passw2)
            {
                var resp1 = new
                {
                    success = false,
                    msg = "Las contraseñas no coinciden."
                };
                return Json(resp1);
            }

            try
            {
                List<int> roles = new List<int>();
                if (jsonRoles != "")
                {
                    var s = new JavaScriptSerializer();
                    var jsonIds = s.Deserialize<dynamic>(jsonRoles);
                    for (int i = 0; i < jsonIds.Length; i++)
                    {
                        roles.Add(int.Parse(jsonIds[i]));
                    }
                }

                //Buscando el usuario por el idtrabajador
                var usuario = (from t in db.Colaborador
                               join p in db.Persona on t.persona equals p
                               join u in db.Usuario on p equals u.persona
                               where t.Secuencial == idTrabajador
                               select u).SingleOrDefault();

                if (passw1 != "")
                {
                    string hashPassw = Checksum.CalculateStringHash(passw1, Algorithm.SHA1);
                    usuario.Passw = hashPassw;
                }

                usuario.EstaActivo = activo;

                //Buscando el ROL
                var listUserRol = (from r in db.Rol
                                   join
                      ur in db.UsuarioRol on r.Secuencial equals ur.SecuencialRol
                                   where ur.SecuencialUsuario == usuario.Secuencial
                                   select ur).ToList();

                //Usuarios del TFS
                Colaborador colaborador = db.Colaborador.Find(idTrabajador);
                DateTime ahora = DateTime.Now;
                UsuarioTFS userTfs = db.UsuarioTFS.Where(x => x.SecuencialColaborador == colaborador.Secuencial && x.FechaInicio <= ahora && x.FechaFin >= ahora).FirstOrDefault();
                if (userTfs != null && userTfs.Usuario != usuarioTFS && !string.IsNullOrEmpty(usuarioTFS))
                {
                    userTfs.FechaFin = ahora;
                    UsuarioTFS newUserTfs = new UsuarioTFS
                    {
                        colaborador = colaborador,
                        Usuario = usuarioTFS,
                        FechaInicio = ahora,
                        FechaFin = DateTime.Parse("31/12/9999 23:59:59")
                    };
                    db.UsuarioTFS.Add(newUserTfs);
                }
                else if (userTfs == null && !string.IsNullOrEmpty(usuarioTFS))
                {
                    UsuarioTFS newUserTfs = new UsuarioTFS
                    {
                        colaborador = colaborador,
                        Usuario = usuarioTFS,
                        FechaInicio = ahora,
                        FechaFin = DateTime.Parse("31/12/9999 23:59:59")
                    };
                    db.UsuarioTFS.Add(newUserTfs);
                }

                //Borrando los roles que tenía y ya no están vigentes
                foreach (var userRol in listUserRol)
                {
                    bool encontrado = false;
                    foreach (int rol in roles)
                    {
                        if (rol == userRol.SecuencialRol)
                        {
                            encontrado = true;
                            break;
                        }
                    }
                    if (encontrado == false)
                    {
                        db.UsuarioRol.Remove(userRol);
                    }
                }

                //Agregando los roles que no tiene
                foreach (int rol in roles)
                {
                    bool encontrado = false;
                    foreach (var userRol in listUserRol)
                    {
                        if (rol == userRol.SecuencialRol)
                        {
                            encontrado = true;
                            break;
                        }
                    }
                    if (encontrado == false)
                    {
                        UsuarioRol userRol = new UsuarioRol
                        {
                            SecuencialRol = rol,
                            SecuencialUsuario = usuario.Secuencial,
                            NumeroVerificador = 1
                        };

                        db.UsuarioRol.Add(userRol);
                    }
                }

                db.SaveChanges();
                LoggerManager.LogSensitiveOperation(
                    "Editar usuaruio",
                    $"El usuario {emailUser} editó un usuario.",
                    emailUser
                );
                var resp = new
                {
                    success = true,
                    msg = "Operación realizada correctamente."
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EditarUsuario] Error al editar usuario: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = " Error al editar usuario."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult EliminarTrabajador(int idTrabajador)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[EditarUsuario] Usuario {emailUser} solicitó eliminar trabajador.");
                //Borrando los jefes inmediatos 
                var jefesInmediatos = (from cji in db.Colaborador_JefeInmediato
                                       where cji.SecuencialColaborador == idTrabajador || cji.SecuencialJefeInmediato == idTrabajador
                                       select cji).ToList();
                foreach (var cji in jefesInmediatos)
                {
                    db.Colaborador_JefeInmediato.Remove(cji);
                }

                //Buscando si el colaborador tiene tareas asignadas.
                var cantTareas = (from t in db.Tarea
                                  where t.SecuencialColaborador == idTrabajador
                                  select t).Count();
                if (cantTareas > 0)
                {//Tiene tareas, en este caso vamos a deshabilitarlo solamente
                    Usuario usuario1 = (from u in db.Usuario
                                        join p in db.Persona on u.persona equals p
                                        join t in db.Colaborador on p equals t.persona
                                        where t.Secuencial == idTrabajador
                                        select u).FirstOrDefault();
                    usuario1.EstaActivo = 0;
                    db.SaveChanges();
                    var resp1 = new
                    {
                        success = true,
                        msg = "Operación realizada correctamente, deshabilitando el usuario."
                    };
                    return Json(resp1);
                }

                //Buscando las fotos
                var fotos = (from f in db.FotoColaborador
                             where f.SecuencialColaborador == idTrabajador
                             select f);
                //Borrando las fotos
                foreach (var foto in fotos)
                {

                    if (foto.Url != "workers/no-foto.jpg")//Hay que borrar un fichero
                    {
                        var file = Path.Combine(Server.MapPath("~/Web/images/workers"), foto.Url);
                        if (System.IO.File.Exists(file))
                            System.IO.File.Delete(file);
                    }

                    db.FotoColaborador.Remove(foto);
                }

                var trabajador = db.Colaborador.Where(x => x.Secuencial == idTrabajador).Single();
                var persona = db.Persona.Where(x => x.Secuencial == trabajador.SecuencialPersona).Single();
                var usuario = db.Usuario.Where(x => x.SecuencialPersona == persona.Secuencial).Single();

                //Buscando los Roles
                var userroles = (from ur in db.UsuarioRol
                                 where ur.SecuencialUsuario == usuario.Secuencial
                                 select ur);
                //Borrando los roles
                foreach (var urol in userroles)
                {
                    db.UsuarioRol.Remove(urol);
                }

                db.Colaborador.Remove(trabajador);
                db.Usuario.Remove(usuario);
                db.Persona.Remove(persona);

                db.SaveChanges();
                LoggerManager.LogSensitiveOperation(
                    "Eliminar trabajador",
                    $"El usuario {emailUser} eliminó un trabajador.",
                    emailUser
                );
                var resp = new
                {
                    success = true,
                    msg = "Operación realizada correctamente."
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EditarUsuario] Error al eliminar un trabajador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al eliminar un trabajador."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DatosTrabajador(int idTrabajador)
        {
            try
            {
                var trabajador = (from t in db.Colaborador
                                  join
              p in db.Persona on t.persona equals p
                                  join
    u in db.Usuario on p equals u.persona
                                  join
    f in db.FotoColaborador on t equals f.colaborador
                                  where t.Secuencial == idTrabajador
                                  select new
                                  {
                                      nombre1 = p.Nombre1,
                                      nombre2 = p.Nombre2,
                                      apellido1 = p.Apellido1,
                                      apellido2 = p.Apellido2,
                                      sexo = p.Sexo,
                                      nacionalidad = p.SecuencialPais,
                                      fn = p.FechaNac,
                                      sede = t.SecuencialSede,
                                      publicacionesDosCinco = t.PublicacionesDosCinco,
                                      cargo = t.SecuencialCargo,
                                      email = u.Email,
                                      departamento = t.SecuencialDepartamento,
                                      fi = t.FechaIngreso,
                                      foto = f.Url
                                  }).FirstOrDefault();

                var resp = new
                {
                    success = true,
                    trabajador = trabajador,
                    fechaNac = trabajador.fn.ToString("dd/MM/yyyy"),
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DatosTrabajador] Error al dar datos trabajador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al dar datos trabajador."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult FuncionesTrabajador(int idTrabajador)
        {
            try
            {
                var funciones = (from e in db.Especialidad
                                 where e.EstaActiva == 1
                                 select new
                                 {
                                     id = e.Secuencial,
                                     especialidad = e.Descripcion,
                                     activa = (from ce in db.ColaboradorEspecialidad
                                               where ce.SecuencialColaborador == idTrabajador &&
                                                     ce.SecuencialEspecialidad == e.Secuencial
                                               select ce).Count() > 0 ? 1 : 0
                                 });
                var colaboradores = (from c in db.Colaborador
                                     join
                 p in db.Persona on c.persona equals p
                                     select new
                                     {
                                         id = c.Secuencial,
                                         nombre = p.Nombre1 + " " + p.Apellido1,
                                         jefe = (from cj in db.Colaborador_JefeInmediato
                                                 where cj.SecuencialColaborador == idTrabajador &&
                                                       cj.SecuencialJefeInmediato == c.Secuencial
                                                 select cj).Count() > 0 ? 1 : 0
                                     });
                var resp = new
                {
                    success = true,
                    funciones = funciones,
                    colaboradores = colaboradores
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[FuncionesTrabajador] Error al dar funciones del trabajador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al dar funciones del trabajador."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult GuardarFuncionesTrabajador(int idTrabajador, string jsonFunciones, int idJefe = 0)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[GuardarFuncionesTrabajador] Usuario {emailUser} solicitó guardar funciones trabajador.");
                if (idJefe != 0)
                {
                    //Guardando el Jefe Inmediato
                    var jefeInm = (from ji in db.Colaborador_JefeInmediato
                                   where ji.SecuencialColaborador == idTrabajador
                                   select ji).FirstOrDefault();
                    if (jefeInm == null)//Entrando un nuevo jefe inmediato
                    {
                        Colaborador_JefeInmediato newJefeInm = new Colaborador_JefeInmediato
                        {
                            SecuencialColaborador = idTrabajador,
                            SecuencialJefeInmediato = idJefe,
                            NumeroVerificador = 1
                        };

                        db.Colaborador_JefeInmediato.Add(newJefeInm);
                    }
                    else
                    {
                        if (jefeInm.SecuencialJefeInmediato != idJefe)
                            jefeInm.SecuencialJefeInmediato = idJefe;
                    }
                }

                //Borrando las especialidades
                var colabEspecialidades = (from ce in db.ColaboradorEspecialidad
                                           where ce.SecuencialColaborador == idTrabajador
                                           select ce).ToList();

                foreach (var colabEspecialidad in colabEspecialidades)
                {
                    db.ColaboradorEspecialidad.Remove(colabEspecialidad);
                }

                List<int> idEspecialidades = new List<int>();
                var s = new JavaScriptSerializer();
                var jsonObj = s.Deserialize<dynamic>(jsonFunciones);
                for (int i = 0; i < jsonObj.Length; i++)//Por cada una de las especialidades
                {

                    dynamic valorId = jsonObj[i]["id"];
                    int idEspecialidad = 0;
                    if (Object.ReferenceEquals(valorId.GetType(), idEspecialidad.GetType()))
                    {
                        idEspecialidad = valorId;
                    }
                    else
                    {
                        idEspecialidad = int.Parse(jsonObj[i]["id"]);
                    }

                    var colabEspecialidad = new ColaboradorEspecialidad
                    {
                        SecuencialColaborador = idTrabajador,
                        SecuencialEspecialidad = idEspecialidad,
                        NumeroVerificador = 1
                    };
                    db.ColaboradorEspecialidad.Add(colabEspecialidad);
                    //idEspecialidades.Add(int.Parse(jsonObj["colaboradores"][i]["id"]));
                }

                db.SaveChanges();
                LoggerManager.LogSensitiveOperation(
                    "Guardar Funciones Trabajador",
                    $"El usuario {emailUser} guardó funciones del trabajador.",
                    emailUser
                );
                var resp = new
                {
                    success = true,
                    msg = "Operación realizada correctamente"
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[GuardarFuncionesTrabajador] Error al guardar funciones del trabajador: {e.Message}");
                var resp = new
                {
                    success = true,
                    msg = "Error al guardar funciones del trabajador."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarNivelesModuloColaborador(int idColaborador)
        {
            try
            {
                List<object> niveles = new List<object>();
                var modulos = db.Modulo.Where(x => x.EstaActivo == 1).OrderBy(x => x.Descripcion).ToList();

                foreach (var mod in modulos)
                {
                    NivelColaboradorModulo nivelCM = db.NivelColaboradorModulo.Where(x =>
                                                                                x.SecuencialModulo == mod.Secuencial &&
                                                                                x.SecuencialColaborador == idColaborador).FirstOrDefault();
                    int nivel = 0;
                    if (nivelCM != null)
                    {
                        nivel = (int)nivelCM.Nivel;
                    }

                    niveles.Add(new
                    {
                        idModulo = mod.Secuencial,
                        modulo = mod.Descripcion,
                        nivel = nivel
                    });
                }

                var resp = new
                {
                    success = true,
                    niveles = niveles
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarNivelesModuloColaborador] Error al dar niveles módulo trabajador: {e.Message}");
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
        public ActionResult EstablecerNivelModuloColaborador(int idModulo, int idColaborador, int nivel)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[EstablecerNivelModuloColaborador] Usuario {emailUser} solicitó establecer nivel modulo trabajador.");
                //Buscando si ya existe
                NivelColaboradorModulo nivelColaborador = db.NivelColaboradorModulo.Where(x =>
                                                            x.SecuencialColaborador == idColaborador &&
                                                            x.SecuencialModulo == idModulo).FirstOrDefault();
                if (nivelColaborador != null)
                {
                    nivelColaborador.Nivel = nivel;
                }
                else
                {
                    nivelColaborador = new NivelColaboradorModulo
                    {
                        SecuencialModulo = idModulo,
                        SecuencialColaborador = idColaborador,
                        Nivel = nivel,
                        NumeroVerificador = 1
                    };
                    db.NivelColaboradorModulo.Add(nivelColaborador);
                }

                db.SaveChanges();
                LoggerManager.LogSensitiveOperation(
                    "Establecer Nivel Modulo Colaborador",
                    $"El usuario {emailUser} estableció nivel para colaborador.",
                    emailUser
                );
                var resp = new
                {
                    success = true
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EstablecerNivelModuloColaborador] Error al establecer nivel en colaborador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = " Error al establecer nivel en colaborador."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarNivelesCompetenciasColaborador(int idColaborador, int idCategoria = 0, int idSubCategoria = 0)
        {
            try
            {
                var competencias = (from c in db.Competencia
                                    join
                                        scc in db.SubCategoriaCompetencia on c.subCategoriaCompetencia equals scc
                                    join
                                        cc in db.CategoriaCompetencia on scc.categoriaCompetencia equals cc
                                    where c.EstaActiva == 1 && scc.EstaActiva == 1 && cc.EstaActiva == 1
                                    select new
                                    {
                                        idCompetencia = c.Secuencial,
                                        codigoCompetencia = c.Codigo,
                                        descCompetencia = c.Descripcion,
                                        idSubCategoria = scc.Secuencial,
                                        codigoSubCategoria = scc.Codigo,
                                        descSubCategoria = scc.Descripcion,
                                        idCategoria = cc.Secuencial,
                                        codigoCategoria = cc.Codigo,
                                        descCategoria = cc.Descripcion,
                                        nivel = (from ct in db.ColaboradorNivelCompetencia
                                                 where ct.SecuencialColaborador == idColaborador &&
                                                        ct.SecuencialCompetencia == c.Secuencial
                                                 select ct.Nivel).FirstOrDefault()
                                    }).ToList();

                var categorias = (from c in competencias
                                  select new
                                  {
                                      id = c.idCategoria,
                                      codigo = c.codigoCategoria,
                                      desc = c.descCategoria
                                  }).Distinct().ToList();

                var subCategorias = (from c in competencias
                                     select new
                                     {
                                         id = c.idSubCategoria,
                                         codigo = c.codigoSubCategoria,
                                         desc = c.descSubCategoria
                                     }).Distinct().ToList();

                if (idCategoria != 0)
                {
                    competencias = (from c in competencias
                                    where c.idCategoria == idCategoria
                                    select c).ToList();

                    subCategorias = (from c in competencias
                                     select new
                                     {
                                         id = c.idSubCategoria,
                                         codigo = c.codigoSubCategoria,
                                         desc = c.descSubCategoria
                                     }).Distinct().ToList();

                    if (idSubCategoria != 0)
                    {
                        competencias = (from c in competencias
                                        where c.idSubCategoria == idSubCategoria
                                        select c).ToList();
                    }
                }

                var resp = new
                {
                    success = true,
                    categoriasSoft = categorias,
                    subCategoriasSoft = subCategorias,
                    competenciasSoft = competencias
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarNivelesCompetenciasColaborador] Error al dar niveles de competencias en colaborador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al dar niveles de competencias,"
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult EstablecerNivelCompetenciaColaborador(int idColaborador, int idCompetencia, int nivel)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[EstablecerNivelCompetenciaColaborador] Usuario {emailUser} solicitó establecer nivel competencia modulo trabajador.");
                //Buscando si ya existe una competencia
                ColaboradorNivelCompetencia nivelColaborador = db.ColaboradorNivelCompetencia.Where(
                                                                x => x.SecuencialColaborador == idColaborador &&
                                                                x.SecuencialCompetencia == idCompetencia).FirstOrDefault();
                if (nivelColaborador != null)
                {
                    nivelColaborador.Nivel = nivel;
                }
                else
                {
                    nivelColaborador = new ColaboradorNivelCompetencia
                    {
                        SecuencialCompetencia = idCompetencia,
                        SecuencialColaborador = idColaborador,
                        Nivel = nivel,
                        NumeroVerificador = 1
                    };
                    db.ColaboradorNivelCompetencia.Add(nivelColaborador);
                }
                db.SaveChanges();
                LoggerManager.LogSensitiveOperation(
                   "Establecer Nivel Competencia Modulo Colaborador",
                   $"El usuario {emailUser} estableció nivel de competencia para colaborador.",
                   emailUser
               );
                var resp = new
                {
                    success = true
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EstablecerNivelCompetenciaColaborador] Error al establecer nivel de competencia en colaborador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al establecer nivel de competencia en colaborador."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarNivelesTecnologiaColaborador(int idColaborador)
        {
            try
            {
                List<object> niveles = new List<object>();
                var tecnologias = db.TecnologiasYProcesos.Where(x => x.EstaActivo == 1).OrderBy(x => x.Descripcion).ToList();

                foreach (var tec in tecnologias)
                {
                    NivelColaboradorTecnologia nivelCT = db.NivelColaboradorTecnologia.Where(x =>
                                                                                x.SecuencialTecnologia == tec.Secuencial &&
                                                                                x.SecuencialColaborador == idColaborador).FirstOrDefault();
                    int nivel = 0;
                    if (nivelCT != null)
                    {
                        nivel = (int)nivelCT.Nivel;
                    }

                    niveles.Add(new
                    {
                        idTecnologia = tec.Secuencial,
                        tecnologia = tec.Descripcion,
                        nivel = nivel
                    });
                }

                var resp = new
                {
                    success = true,
                    niveles = niveles
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarNivelesTecnologiaColaborador] Error al establecer nivel de tegnologías en colaborador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al establecer nivel de tegnologías en colaborador."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult EstablecerNivelTecnologiaColaborador(int idTecnologia, int idColaborador, int nivel)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[EstablecerNivelTecnologiaColaborador] Usuario {emailUser} solicitó establecer nivel tegnología modulo trabajador.");
                NivelColaboradorTecnologia nivelColaborador = db.NivelColaboradorTecnologia.Where(x =>
                                                            x.SecuencialColaborador == idColaborador &&
                                                            x.SecuencialTecnologia == idTecnologia).FirstOrDefault();
                if (nivelColaborador != null)
                {
                    nivelColaborador.Nivel = nivel;
                }
                else
                {
                    nivelColaborador = new NivelColaboradorTecnologia
                    {
                        SecuencialTecnologia = idTecnologia,
                        SecuencialColaborador = idColaborador,
                        Nivel = nivel,
                        NumeroVerificador = 1
                    };
                    db.NivelColaboradorTecnologia.Add(nivelColaborador);
                }

                db.SaveChanges();
                LoggerManager.LogSensitiveOperation(
                   "Establecer Nivel Tegnoligía Modulo Colaborador",
                   $"El usuario {emailUser} estableció nivel de tegnología para colaborador.",
                   emailUser
               );
                var resp = new
                {
                    success = true
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EstablecerNivelTecnologiaColaborador] Error al establecer nivel de tegnología en colaborador: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al establecer nivel de tegnología en colaborador."
                };
                return Json(resp);
            }
        }

        //----------------- Gestion de Clientes --------------------
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarUsuariosClientes(string filtro = "")
        {
            try
            {
                List<object> usuariosClientes = new List<object>();
                if (filtro != "")
                {
                    usuariosClientes = (from u in db.Usuario
                                        join
                        p in db.Persona on u.persona equals p
                                        join
 pc in db.Persona_Cliente on p.persona_cliente equals pc
                                        join ur in db.UsuarioRol on u.Secuencial equals ur.SecuencialUsuario
                                        where
                                              (ur.rol.Codigo == "CLIENTE")
                                              &&
                                              (
                                                  u.Email.ToString().ToLower().Contains(filtro.ToLower()) ||
                                                  pc.cliente.Descripcion.ToString().ToLower().Contains(filtro.ToLower()) ||
                                                  p.Nombre1.ToString().ToLower().Contains(filtro.ToLower()) ||
                                                  p.Nombre2.ToString().ToLower().Contains(filtro.ToLower()) ||
                                                  p.Apellido1.ToString().ToLower().Contains(filtro.ToLower()) ||
                                                  p.Apellido2.ToString().ToLower().Contains(filtro.ToLower()) ||
                                                  pc.Telefono.ToString().ToLower().Contains(filtro.ToLower())
                                              )
                                        select new
                                        {
                                            nombre = p.Nombre1 + " " + p.Nombre2 + " " + p.Apellido1 + " " + p.Apellido2,
                                            email = u.Email,
                                            cliente = pc.cliente.Descripcion,
                                            userAct = (u.EstaActivo == 1) ? "SI" : "NO",
                                            idUser = u.Secuencial,
                                            verificador = u.NumeroVerificador,
                                            nombre1 = p.Nombre1,
                                            nombre2 = p.Nombre2,
                                            apellido1 = p.Apellido1,
                                            apellido2 = p.Apellido2,
                                            sexo = p.Sexo,
                                            fechaNac = p.FechaNac,
                                            nacionalidad = p.SecuencialPais,
                                            telefono = pc.Telefono,
                                            idCliente = pc.SecuencialCliente,
                                            userActivoNumber = u.EstaActivo
                                        }).ToList<object>();
                }
                else
                {
                    usuariosClientes = (from u in db.Usuario
                                        join
                                            p in db.Persona on u.persona equals p
                                        join
                                            pc in db.Persona_Cliente on p.persona_cliente equals pc
                                        join ur in db.UsuarioRol on u.Secuencial equals ur.SecuencialUsuario
                                        where ur.rol.Codigo == "CLIENTE"
                                        select new
                                        {
                                            nombre = p.Nombre1 + " " + p.Nombre2 + " " + p.Apellido1 + " " + p.Apellido2,
                                            email = u.Email,
                                            cliente = pc.cliente.Descripcion,
                                            userAct = (u.EstaActivo == 1) ? "SI" : "NO",
                                            idUser = u.Secuencial,
                                            verificador = u.NumeroVerificador,
                                            nombre1 = p.Nombre1,
                                            nombre2 = p.Nombre2,
                                            apellido1 = p.Apellido1,
                                            apellido2 = p.Apellido2,
                                            sexo = p.Sexo,
                                            fechaNac = p.FechaNac,
                                            nacionalidad = p.SecuencialPais,
                                            telefono = pc.Telefono,
                                            idCliente = pc.SecuencialCliente,
                                            userActivoNumber = u.EstaActivo
                                        }).ToList<object>();
                }

                var resp = new
                {
                    success = true,
                    usuariosClientes = usuariosClientes
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarUsuariosClientes] Error al dar usuarios clientes: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al dar usuarios clientes."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult GuardarUsuarioCliente(string nombre1, string apellido1, string sexo, string fechaNac, int nacionalidad, string email, int cliente, int userActivo, string telefono, string pass1, string pass2, string nombre2 = "", string apellido2 = "", int idUsuarioCliente = 0, int numeroVerificador = 0)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[GuardarUsuarioCliente] Usuario {emailUser} solicitó guardar usuario.");
                string[] fechas = fechaNac.Split(new Char[] { '/' });
                int dia = Int32.Parse(fechas[0]);
                int mes = Int32.Parse(fechas[1]);
                int anno = Int32.Parse(fechas[2]);

                string msg = "Se ha registrado el nuevo usuario de cliente.";
                bool success = true;
                if (pass1 != pass2)
                {
                    msg = "Las contraseñas no coinciden";
                    success = false;
                }
                else
                {
                    if (idUsuarioCliente != 0)//Actualizacion
                    {
                        Usuario user = db.Usuario.Find(idUsuarioCliente);
                        Persona personaUser = user.persona;
                        Persona_Cliente personaCliente = personaUser.persona_cliente;

                        if (numeroVerificador != user.NumeroVerificador)
                        {
                            msg = "Se ha actualizado el usuario cliente que usted lo seleccionó, por favor actualice la página e intente nuevamente actualizar el usuario cliente.";
                            success = false;
                        }
                        else
                        {

                            if (pass1 != "")
                            {
                                string hashPassw = Checksum.CalculateStringHash(pass1, Algorithm.SHA1);
                                user.Passw = hashPassw;
                            }

                            user.Email = email;
                            user.EstaActivo = userActivo;
                            user.NumeroVerificador = user.NumeroVerificador + 1;

                            personaUser.Nombre1 = nombre1;
                            personaUser.Nombre2 = nombre2;
                            personaUser.Apellido1 = apellido1;
                            personaUser.Apellido2 = apellido2;
                            personaUser.Sexo = sexo;
                            personaUser.FechaNac = new DateTime(anno, mes, dia);
                            personaUser.SecuencialPais = nacionalidad;

                            personaCliente.Telefono = telefono;
                            personaCliente.SecuencialCliente = cliente;

                            msg = "Se ha actualizado correctamente el usuario de cliente";
                        }
                    }
                    else//Nuevo usuario cliente
                    {
                        string hashPassw = Checksum.CalculateStringHash(pass1, Algorithm.SHA1);

                        Rol rol = db.Rol.Where(x => x.Codigo == "CLIENTE").FirstOrDefault();
                        if (rol != null)
                        {
                            Persona personaUser = new Persona
                            {
                                Nombre1 = nombre1,
                                Nombre2 = nombre2,
                                Apellido1 = apellido1,
                                Apellido2 = apellido2,
                                Sexo = sexo,
                                FechaNac = new DateTime(anno, mes, dia),
                                SecuencialPais = nacionalidad,
                                NumeroVerificador = 1
                            };
                            db.Persona.Add(personaUser);

                            Usuario user = new Usuario
                            {
                                Passw = hashPassw,
                                Email = email,
                                EstaActivo = userActivo,
                                NumeroVerificador = 1,
                                persona = personaUser
                            };
                            db.Usuario.Add(user);

                            UsuarioRol userRol = new UsuarioRol
                            {
                                rol = rol,
                                usuario = user,
                                NumeroVerificador = 1
                            };
                            db.UsuarioRol.Add(userRol);

                            Persona_Cliente personaCliente = new Persona_Cliente
                            {
                                persona = personaUser,
                                Telefono = telefono,
                                SecuencialCliente = cliente,
                                NumeroVerificador = 1
                            };

                            db.Persona_Cliente.Add(personaCliente);
                        }
                        else
                        {
                            msg = "No está definido el Rol - CLIENTE, por favor contacte con los administrdores del sistema";
                            success = false;
                        }
                    }

                    db.SaveChanges();
                    LoggerManager.LogSensitiveOperation(
                           "Guardar usuario cliente",
                           $"El usuario {emailUser} guardó un usuario.",
                           emailUser
                       );
                }

                var resp = new
                {
                    success = success,
                    msg = msg
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[GuardarUsuarioCliente] Error al guardar usuario: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error al guardar usuario."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult EliminarUsuarioCliente(int idUsuarioCliente)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[EliminarUsuarioCliente] Usuario {emailUser} solicitó eliminar usuario.");
                Usuario user = db.Usuario.Find(idUsuarioCliente);
                List<UsuarioRol> usuarioRol = user.usuarioRol.ToList();
                //Persona persona = user.persona;
                //Persona_Cliente personCliente = persona.persona_cliente;                
                //db.Persona_Cliente.Remove(personCliente);

                UsuarioRol userRol = usuarioRol.Where(x => x.rol.Codigo == "CLIENTE").FirstOrDefault();
                if (userRol != null)
                    db.UsuarioRol.Remove(userRol);

                //db.Usuario.Remove(user);
                //db.Persona.Remove(persona);
                db.SaveChanges();
                LoggerManager.LogSensitiveOperation(
                           "Eliminar usuario cliente",
                           $"El usuario {emailUser} eliminó un usuario.",
                           emailUser
                       );

                var resp = new
                {
                    success = true,
                    msg = "Se ha eliminado correctamente el usuario cliente"
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EliminarUsuarioCliente] Error al eliminar usuario: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = " Error al eliminar usuario."
                };
                return Json(resp);
            }
        }

        //---------------- Contratos de los Clientes ---------------
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarContratosCliente(int idCliente, string filtro = "")
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[DarContratosCliente] El usuario {emailUser} está consultando contratos para el cliente ID {idCliente} con filtro '{filtro}'.");
                List<object> contratos = new List<object>();

                if (filtro != "")
                {
                    contratos = (from ct in db.ContratoCliente
                                 join
                                     tc in db.TipoContratoCliente on ct.tipoContratoCliente equals tc
                                 join
                                     cl in db.Cliente on ct.cliente equals cl
                                 where ct.SecuencialCliente == idCliente
                                 select new
                                 {
                                     id = ct.Secuencial,
                                     cliente = cl.Descripcion,
                                     tipoContrato = tc.Descripcion,
                                     descripcion = ct.Descripcion,
                                     finicio = ct.FechaInicio,
                                     ffin = ct.FechaFin
                                 }).ToList<object>();
                }
                else
                {
                    contratos = (from ct in db.ContratoCliente
                                 join
                                     tc in db.TipoContratoCliente on ct.tipoContratoCliente equals tc
                                 join
                                     cl in db.Cliente on ct.cliente equals cl
                                 where ct.SecuencialCliente == idCliente
                                 select new
                                 {
                                     id = ct.Secuencial,
                                     cliente = cl.Descripcion,
                                     tipoContrato = tc.Descripcion,
                                     descripcion = ct.Descripcion,
                                     finicio = ct.FechaInicio,
                                     ffin = ct.FechaFin
                                 }).ToList<object>();
                }

                var resp = new
                {
                    success = true,
                    contratosClientes = contratos
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarContratosCliente] Error inesperado al consultar contratos del cliente: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al consultar contratos del cliente."
                };
                return Json(resp);
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public ActionResult ModulosFuncionalidades()
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[ModulosFuncionalidades] El usuario {emailUser} está consultando los módulos y funcionalidades del sistema.");
                List<object> modulos = new List<object>();
                modulos = (from m in db.Modulo
                           where m.EstaActivo == 1 &&
                                 m.funcionalidad.Count > 0 //Que muestre si tiene funcionalidades
                           select new
                           {
                               id = m.Secuencial,
                               type = "group",
                               name = m.Codigo + " ( " + m.Descripcion + " )",
                               editable = false,
                               children = (from f in db.Funcionalidad
                                           where f.SecuencialModulo == m.Secuencial && f.EstaActiva == 1
                                           select f.Secuencial).ToList()

                           }).ToList<object>();

                var allSystem = new
                {
                    id = 0,
                    name = "Todo el Sistema",
                    editable = false,
                    children = new object[0],
                    subGroups = (from m in db.Modulo
                                 where m.EstaActivo == 1 && m.funcionalidad.Count > 0
                                 select m.Secuencial).ToList()
                };

                modulos.Insert(0, allSystem);

                var funcionalidades = (from f in db.Funcionalidad
                                       where f.EstaActiva == 1
                                       select new
                                       {
                                           id = f.Secuencial,
                                           name = f.Descripcion
                                       }).ToList();

                var treeObj = new
                {
                    modulos = modulos,
                    funcionalidades = funcionalidades
                };

                return Json(treeObj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[ModulosFuncionalidades] Error inesperado al consultar módulos y funcionalidades: {e.Message}");

                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al consultar módulos y funcionalidades."
                };
                return Json(resp);
            }

        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult NuevoContratoCliente(int cliente, int tipoContrato, string fechaInicio, string fechaFin, string descripcion, HttpPostedFileBase[] adjuntos = null, int horas = 0, int[] funcionalidades = null, int idContratoCliente = 0, int verificador = 0)
        {
            try
            {
                string emailUser = User.Identity.Name;
                string clienteNombre = db.Cliente.Find(cliente)?.Descripcion ?? cliente.ToString();
                if (idContratoCliente != 0)
                {
                    LoggerManager.LogInfo($"[NuevoContratoCliente] El usuario {emailUser} está modificando el contrato ID {idContratoCliente} del cliente {clienteNombre}.");
                }
                else
                {
                    LoggerManager.LogInfo($"[NuevoContratoCliente] El usuario {emailUser} está creando un nuevo contrato para el cliente {clienteNombre}.");
                }

                string[] fechas = fechaInicio.Split(new Char[] { '/' });
                int dia = Int32.Parse(fechas[0]);
                int mes = Int32.Parse(fechas[1]);
                int anno = Int32.Parse(fechas[2]);
                DateTime fInicio = new DateTime(anno, mes, dia);

                string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                dia = Int32.Parse(fechasFin[0]);
                mes = Int32.Parse(fechasFin[1]);
                anno = Int32.Parse(fechasFin[2]);
                DateTime fFin = new DateTime(anno, mes, dia);

                if (idContratoCliente != 0)
                {
                    ContratoCliente ct = db.ContratoCliente.Find(idContratoCliente);
                    if (ct.NumeroVerificador != verificador)
                    {//Error de concurrencia
                        var r = new
                        {
                            success = false,
                            msg = "No se pudo actualizar porque cambió el contrato, actualice e intente nuevamente"
                        };
                        return Json(r);
                    }
                    else
                    {
                        //Actualizando el contrato
                        ct.SecuencialCliente = cliente;
                        ct.SecuencialTipoContratoCliente = tipoContrato;
                        ct.Descripcion = descripcion;
                        ct.FechaInicio = fInicio;
                        ct.FechaFin = fFin;
                        ct.NumeroVerificador = ct.NumeroVerificador + 1;

                        //Guardando los adjunto
                        if (adjuntos != null)
                        {
                            foreach (HttpPostedFileBase adj in adjuntos)
                            {
                                if (adj != null)
                                {
                                    string extFile = Path.GetExtension(adj.FileName);
                                    string newNameFile = Utiles.RandomString(10) + extFile;
                                    string path = Path.Combine(Server.MapPath("~/Web/resources/contracts"), newNameFile);
                                    adj.SaveAs(path);

                                    ContratoAdjunto contratoAdj = new ContratoAdjunto
                                    {
                                        contratoCliente = ct,
                                        Url = "/resources/contracts/" + newNameFile,
                                        Descripcion = adj.FileName,
                                        NumeroVerificador = 1
                                    };

                                    db.ContratoAdjunto.Add(contratoAdj);
                                }
                            }
                        }

                        if (tipoContrato == 1)
                        {//Mantenimiento
                            ContratoMantenimiento cMant = db.ContratoMantenimiento.Find(ct.Secuencial);
                            if (cMant == null)//Cambio el tipo de contrato
                            {
                                //Eliminando el contrato de desarrollo
                                ContratoDesarrollo cDev = db.ContratoDesarrollo.Find(ct.Secuencial);
                                List<ContratoDesarrolloFunc> cDevFuncs = cDev.contratoDesarrolloFunc.ToList<ContratoDesarrolloFunc>();
                                foreach (ContratoDesarrolloFunc cDevFenc in cDevFuncs)//Eliminar a cada una de las funcionalidades
                                {
                                    db.ContratoDesarrolloFunc.Remove(cDevFenc);
                                }
                                db.ContratoDesarrollo.Remove(cDev);
                                //Creando el tipo de mantenimiento
                                cMant = new ContratoMantenimiento
                                {
                                    SecuencialContratoCliente = ct.Secuencial,
                                    HorasMensuales = horas
                                };
                                db.ContratoMantenimiento.Add(cMant);
                            }
                            else
                            {//Cambiando las horas de mantenimiento
                                cMant.HorasMensuales = horas;
                            }
                        }
                        else if (tipoContrato == 2)//DESARROLLO
                        {
                            ContratoDesarrollo cDev = db.ContratoDesarrollo.Find(ct.Secuencial);
                            if (cDev == null)//Aqui el contrato cambio y era de mantenimiento
                            {
                                //Borrando el contrato de mantenimiento
                                ContratoMantenimiento cMant = db.ContratoMantenimiento.Find(ct.Secuencial);
                                List<HorasConsumidas> horasConsumidas = cMant.horasConsumidas.ToList<HorasConsumidas>();
                                foreach (HorasConsumidas h in horasConsumidas)
                                {
                                    db.HorasConsumidas.Remove(h);
                                }
                                db.ContratoMantenimiento.Remove(cMant);

                                //Creando el contrato de desarrollo
                                cDev = new ContratoDesarrollo
                                {
                                    SecuencialContratoCliente = ct.Secuencial
                                };
                                db.ContratoDesarrollo.Add(cDev);
                                //Entrando las funcionalidades
                                foreach (int func in funcionalidades)
                                {
                                    ContratoDesarrolloFunc contratoDesarrolloFunc = new ContratoDesarrolloFunc
                                    {
                                        contratoDesarrollo = cDev,
                                        SecuencialFuncionalidad = func,
                                        NumeroVerificador = 1
                                    };
                                    db.ContratoDesarrolloFunc.Add(contratoDesarrolloFunc);
                                }
                            }
                            else//Actualizando las funcionalidades
                            {
                                //Actualizando las nuevas primero
                                foreach (int func in funcionalidades)
                                {
                                    ContratoDesarrolloFunc cDevFun = db.ContratoDesarrolloFunc.Where(x => x.SecuencialContratoCliente == ct.Secuencial && x.SecuencialFuncionalidad == func).FirstOrDefault();
                                    if (cDevFun == null)//Insertarlo
                                    {
                                        ContratoDesarrolloFunc contratoDesarrolloFunc = new ContratoDesarrolloFunc
                                        {
                                            contratoDesarrollo = cDev,
                                            SecuencialFuncionalidad = func,
                                            NumeroVerificador = 1
                                        };
                                        db.ContratoDesarrolloFunc.Add(contratoDesarrolloFunc);
                                    }
                                }
                                //Eliminando las que sobran
                                List<ContratoDesarrolloFunc> funcionalidadesContrato = (from cDevF in db.ContratoDesarrolloFunc
                                                                                        where cDevF.SecuencialContratoCliente == ct.Secuencial &&
                                                                                              !funcionalidades.Contains(cDevF.SecuencialFuncionalidad)
                                                                                        select cDevF).ToList<ContratoDesarrolloFunc>();
                                foreach (ContratoDesarrolloFunc funcionalidadContrato in funcionalidadesContrato)
                                {
                                    db.ContratoDesarrolloFunc.Remove(funcionalidadContrato);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ContratoCliente contrato = new ContratoCliente
                    {
                        SecuencialCliente = cliente,
                        SecuencialTipoContratoCliente = tipoContrato,
                        Descripcion = descripcion,
                        FechaInicio = fInicio,
                        FechaFin = fFin,
                        NumeroVerificador = 1
                    };
                    db.ContratoCliente.Add(contrato);

                    //Guardando los adjuntos                    
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string extFile = Path.GetExtension(adj.FileName);
                            string newNameFile = Utiles.RandomString(10) + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/resources/contracts"), newNameFile);
                            adj.SaveAs(path);

                            ContratoAdjunto contratoAdj = new ContratoAdjunto
                            {
                                contratoCliente = contrato,
                                Url = "/resources/contracts/" + newNameFile,
                                Descripcion = adj.FileName,
                                NumeroVerificador = 1
                            };

                            db.ContratoAdjunto.Add(contratoAdj);
                        }
                    }

                    if (tipoContrato == 1)//Mantenimiento
                    {
                        ContratoMantenimiento contratoMant = new ContratoMantenimiento
                        {
                            contratoCliente = contrato,
                            HorasMensuales = horas
                        };
                        db.ContratoMantenimiento.Add(contratoMant);
                    }
                    else if (tipoContrato == 2)//Desarrollo
                    {
                        ContratoDesarrollo contratoDev = new ContratoDesarrollo
                        {
                            contratoCliente = contrato
                        };
                        db.ContratoDesarrollo.Add(contratoDev);

                        //Las funcionalidades
                        foreach (int func in funcionalidades)
                        {
                            ContratoDesarrolloFunc contratoDesarrolloFunc = new ContratoDesarrolloFunc
                            {
                                contratoDesarrollo = contratoDev,
                                SecuencialFuncionalidad = func,
                                NumeroVerificador = 1
                            };
                            db.ContratoDesarrolloFunc.Add(contratoDesarrolloFunc);
                        }
                    }
                }
                if (idContratoCliente != 0)
                {
                    LoggerManager.LogSensitiveOperation(
                        "Modificación de contrato",
                        $"El usuario {emailUser} modificó el contrato ID {idContratoCliente} del cliente {clienteNombre}, descripción: {descripcion}, tipo: {tipoContrato}.",
                        emailUser
                    );
                }
                else
                {
                    LoggerManager.LogSensitiveOperation(
                        "Creación de contrato",
                        $"El usuario {emailUser} creó un nuevo contrato para el cliente {clienteNombre}, descripción: {descripcion}, tipo: {tipoContrato}.",
                        emailUser
                    );
                }
                db.SaveChanges();

                var resp = new
                {
                    success = true,
                    msg = "Se ha registrado correctamente el contrato"
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[NuevoContratoCliente] Error inesperado al procesar contrato del cliente: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al procesar contrato del cliente."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarDatosContratosClientes(int idContrato)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[DarDatosContratosClientes] El usuario {emailUser} está consultando los datos del contrato ID {idContrato}.");
                var contrato = (from ct in db.ContratoCliente
                                join
       tc in db.TipoContratoCliente on ct.tipoContratoCliente equals tc
                                where ct.Secuencial == idContrato
                                select new
                                {
                                    id = ct.Secuencial,
                                    idCliente = ct.SecuencialCliente,
                                    idTipoContrato = ct.SecuencialTipoContratoCliente,
                                    tipoContrato = tc.Descripcion,
                                    descripcion = ct.Descripcion,
                                    fechaInicio = ct.FechaInicio,
                                    fechaFin = ct.FechaFin,
                                    verificador = ct.NumeroVerificador,
                                    adjuntos = (from cAdj in db.ContratoAdjunto
                                                where cAdj.SecuencialContratoCliente == idContrato
                                                select new
                                                {
                                                    idAdj = cAdj.Secuencial,
                                                    desc = cAdj.Descripcion,
                                                    url = cAdj.Url
                                                }).ToList(),
                                    horas = (from ctMant in db.ContratoMantenimiento
                                             where ctMant.SecuencialContratoCliente == idContrato
                                             select ctMant.HorasMensuales).FirstOrDefault(),
                                    funcionalidades = (from cdFun in db.ContratoDesarrolloFunc
                                                       join func in db.Funcionalidad
               on cdFun.funcionalidad equals func
                                                       where cdFun.SecuencialContratoCliente == idContrato
                                                       select new
                                                       {
                                                           idFunc = func.Secuencial
                                                       }).ToList()
                                }).FirstOrDefault();
                var resp = new
                {
                    success = true,
                    contrato = contrato
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarDatosContratosClientes] Error inesperado al consultar datos del contrato: {e.Message}");

                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al consultar datos del contrato."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult EliminarAdjuntoContratoCliente(int idAdjunto)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[EliminarAdjuntoContratoCliente] El usuario {emailUser} está eliminando el adjunto ID {idAdjunto}.");
                string msg = "Operacion realizada correctamente";
                bool success = true;
                var contratoAdj = db.ContratoAdjunto.Find(idAdjunto);
                List<object> adjuntos = new List<object>();
                if (contratoAdj != null)
                {
                    string file = Server.MapPath("~/Web/" + contratoAdj.Url);
                    //var file = Path.Combine( path1, contratoAdj.Url);
                    if (System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                    }
                    else
                    {
                        msg = "No se encontró el fichero en el sistema";
                    }

                    int idContrato = contratoAdj.SecuencialContratoCliente;

                    db.ContratoAdjunto.Remove(contratoAdj);
                    LoggerManager.LogSensitiveOperation(
                        "Eliminación de adjunto de contrato",
                        $"El usuario {emailUser} eliminó el adjunto ID {idAdjunto} del contrato ID {idContrato}, archivo: {contratoAdj.Descripcion}.",
                        emailUser
                    );
                    db.SaveChanges();

                    adjuntos = (from cAdj in db.ContratoAdjunto
                                where cAdj.SecuencialContratoCliente == idContrato
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
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EliminarAdjuntoContratoCliente] Error inesperado al eliminar adjunto del contrato: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al eliminar adjunto del contrato."
                };
                return Json(resp);
            }
        }

        //----------------- Gestion de Catalogos -------------------
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarDatosTabla(int idTabla, string nombreTabla, string filtro = "")
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[DarDatosTabla] El usuario {emailUser} está consultando datos de la tabla '{nombreTabla}' (ID: {idTabla}) con filtro: '{filtro}'.");
                var campos = (from cc in db.CampoCatalogo
                              join
                                  tc in db.TipoCampo on cc.tipoCampo equals tc
                              where cc.SecuencialTablaCatalogo == idTabla && cc.NombreNegocio != "NumeroVerificador"
                              orderby cc.Secuencial
                              select new
                              {
                                  id = cc.Secuencial,
                                  nombre = cc.NombreMostrar,
                                  nombreN = cc.NombreNegocio,
                                  tipoCampo = tc.Codigo,
                                  relacion = (from cr in db.CampoCatalogo_TablaCatalogo
                                              where cr.SecuencialCampoCatalogo == cc.Secuencial
                                              select cr.SecuencialTablaCatalogo).FirstOrDefault()
                              }).ToList();

                //Tratando de hacerlo dinámico            
                string nombreNegocioTabla = db.TablaCatalogo.Where(x => x.NombreMostrar == nombreTabla && x.EstaActivo == 1).Select(x => x.NombreNegocio).FirstOrDefault();

                if (nombreTabla == "MODULO")
                {
                    var aux = campos[3];
                    campos[3] = campos[4];
                    campos[4] = aux;
                }

                //Obteniendo el dbSet de la tabla
                Type typeAccesoDatos = db.GetType();
                PropertyInfo propertyTabla = typeAccesoDatos.GetProperty(nombreNegocioTabla);
                MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

                //Buscando todas            
                var allData = System.Linq.Enumerable.ToList((IEnumerable<object>)dbSetTable);

                //Data de tabla catalogo
                List<TablaCatalogo> dataTablaCatalogos = db.TablaCatalogo.ToList();

                List<object> result = new List<object>();
                foreach (var data in allData)
                {
                    Dictionary<object, object> newDic = new Dictionary<object, object>();
                    int i = 1;
                    //Filtrado
                    bool pasafiltro = true;
                    if (filtro != "") pasafiltro = false;
                    foreach (var campo in campos)
                    {
                        string key = "d" + i;
                        if (i > 9)
                        {
                            key = "d9" + i % 10;
                        }
                        PropertyInfo property = data.GetType().GetProperty(campo.nombreN);
                        object value = property.GetValue(data);

                        if (campo.relacion != 0)
                        {//Buscando las relaciones, aquí tiene relacion
                            TablaCatalogo tablaRelacionCampo = dataTablaCatalogos.Where(x => x.Secuencial == campo.relacion).First();
                            string nombreNegocioTablaRelacion = tablaRelacionCampo.NombreNegocio;

                            string nombreRelacion = Utiles.PrimeraMinuscula(nombreNegocioTablaRelacion);
                            PropertyInfo propertyR = data.GetType().GetProperty(nombreRelacion);

                            object tuplaRelacion = propertyR.GetValue(data);

                            Type typeTablaRelacionCampo = tuplaRelacion.GetType();

                            PropertyInfo propiedadNM = typeTablaRelacionCampo.GetProperty("NombreMostrar");

                            if (nombreNegocioTablaRelacion == "EstadoTicket")
                            {
                                EstadoTicket estadoTicket = db.EstadoTicket.Find(value);
                                value = estadoTicket.Codigo;
                            }
                            else if (propiedadNM != null)
                            {
                                value = propiedadNM.GetValue(tuplaRelacion);
                            }
                            else
                            {
                                PropertyInfo propiedadCod = typeTablaRelacionCampo.GetProperty("Codigo");
                                if (propiedadCod != null)
                                {
                                    value = propiedadCod.GetValue(tuplaRelacion);
                                }
                                else
                                {
                                    PropertyInfo propiedadDesc = typeTablaRelacionCampo.GetProperty("Descripcion");
                                    if (propiedadDesc != null)
                                    {
                                        value = propiedadDesc.GetValue(tuplaRelacion);
                                    }
                                    else
                                    {
                                        if (nombreNegocioTablaRelacion == "Persona")
                                        {
                                            Persona persona = db.Persona.Find(value);
                                            value = persona.Nombre1 + " " + persona.Apellido1;
                                        }
                                    }
                                }
                            }
                        }

                        if (nombreNegocioTabla == "Modulo")
                        {
                            if (campo.nombre == "TIPO DE MODULO")
                            {
                                int secuencialTipo = (int)value;
                                value = (from ta in db.TipoModulo
                                         where ta.Secuencial == secuencialTipo
                                         select ta.Descripcion).ToList()[0];
                            }
                        }

                        if (nombreNegocioTabla == "GestorServicios")
                        {
                            if (campo.nombre == "COLABORADOR")
                            {
                                int secuencialColaborador = (int)value;
                                value = db.Colaborador.Where(s => s.Secuencial == secuencialColaborador).Select(p => p.persona.Nombre1 + " " + p.persona.Apellido1).FirstOrDefault();
                            }
                        }

                        if (nombreNegocioTabla == "ClienteAuxiliar")
                        {
                            if (campo.nombre == "LIDER PROYECTO")
                            {
                                int secuencialLider = (int)value;
                                value = db.Colaborador.Where(s => s.Secuencial == secuencialLider).Select(p => p.persona.Nombre1 + " " + p.persona.Apellido1).FirstOrDefault();
                            }
                            if (campo.nombre == "UBICACION")
                            {
                                int secuencialUbicacion = (int)value;
                                value = db.ResponsableProyectos.Where(s => s.Secuencial == secuencialUbicacion).Select(p => p.Nombre).FirstOrDefault();
                            }
                            if (campo.nombre == "RESPONSABLE CODIGO")
                            {
                                int secuencialResponsable = (int)value;
                                value = db.ResponsableProyectos.Where(s => s.Secuencial == secuencialResponsable).Select(p => p.Nombre).FirstOrDefault();
                            }
                            if (campo.nombre == "RESPONSABLE PUBLICACION")
                            {
                                int secuencialResponsable = (int)value;
                                value = db.ResponsableProyectos.Where(s => s.Secuencial == secuencialResponsable).Select(p => p.Nombre).FirstOrDefault();
                            }
                            if (campo.nombre == "RESPONSABLE ACCESO")
                            {
                                int secuencialResponsable = (int)value;
                                value = db.ResponsableProyectos.Where(s => s.Secuencial == secuencialResponsable).Select(p => p.Nombre).FirstOrDefault();
                            }
                        }

                        if (nombreNegocioTabla == "TiempoCerradoTicket")
                        {
                            if (campo.nombre == "CLIENTE")
                            {
                                int secuencialCliente = (int)value;
                                value = (from cl in db.Cliente
                                         where cl.Secuencial == secuencialCliente
                                         select cl.Descripcion).ToList()[0];
                            }
                        }

                        if (nombreNegocioTabla == "FuncionalidadCliente")
                        {
                            if (campo.nombre == "CLIENTE")
                            {
                                int secuencialCliente = (int)value;
                                value = (from cl in db.Cliente
                                         where cl.Secuencial == secuencialCliente
                                         select cl.Descripcion).ToList()[0];
                            }

                            if (campo.nombre == "FUNCIONALIDAD")
                            {
                                int secuencialFuncionalidad = (int)value;
                                value = (from fn in db.Funcionalidad
                                         where fn.Secuencial == secuencialFuncionalidad
                                         select fn.Descripcion).ToList()[0];
                            }
                        }

                        if (campo.tipoCampo == "BOOL")
                        {
                            value = (int.Parse(value.ToString()) == 1) ? "SI" : "NO";
                        }
                        if (campo.tipoCampo == "DATE")
                        {
                            value = DateTime.Parse(value.ToString()).ToString("dd/MM/yyyy");
                        }

                        if (pasafiltro == false)
                        {
                            if (value.ToString().ToLower().Contains(filtro.ToLower())) pasafiltro = true;
                        }

                        newDic.Add(key, value);
                        i++;
                    }
                    if (pasafiltro == true)
                        result.Add(newDic);
                }

                var respD = new
                {
                    success = true,
                    propiedades = campos,
                    datos = result
                };
                return Json(respD);

                //Fin de tratando de hacerlo dinámico
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarDatosTabla] Error inesperado al consultar datos de la tabla '{nombreTabla}': {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al consultar datos de la tabla '{nombreTabla}'."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DarVerificadorCatalogo(string tabla, int id)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[DarVerificadorCatalogo] El usuario {emailUser} está consultando el verificador de la tabla '{tabla}' para el ID {id}.");
                int verificador = 0;

                /*Tratando de hacerlo dinámico*/
                string nombreNegocioTabla = db.TablaCatalogo.Where(x => x.NombreMostrar == tabla && x.EstaActivo == 1).Select(x => x.NombreNegocio).FirstOrDefault();

                //Obteniendo el dbSet de la tabla
                Type typeAccesoDatos = db.GetType();
                PropertyInfo propertyTabla = typeAccesoDatos.GetProperty(nombreNegocioTabla);
                MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

                //Buscando el elemento
                MethodInfo metodoFind = typePropertyTabla.GetMethod("Find");
                object[] pId = new object[1] { new object[1] { id } };
                object findObj = metodoFind.Invoke(dbSetTable, pId);

                //Type del model relativo a la tabla
                Type typeNewObj = Type.GetType("SifizPlanning.Models." + nombreNegocioTabla);

                PropertyInfo property = typeNewObj.GetProperty("NumeroVerificador");

                verificador = (int)property.GetValue(findObj);
                //verificador = (int)property.GetMethod.Invoke(findObj, new object[] { });

                var respD = new
                {
                    success = true,
                    numero = verificador
                };
                return Json(respD);
                /*Fin de Tratando de hacerlo dinámico*/
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarVerificadorCatalogo] Error inesperado al consultar verificador de catálogo: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al consultar verificador de catálogo."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult DatosActividadesRealizadas(string actividad)
        {
            try
            {
                string emailUser = User.Identity.Name;
                if (actividad.Equals(""))
                {
                    LoggerManager.LogInfo($"[DatosActividadesRealizadas] El usuario {emailUser} está consultando todas las actividades realizadas.");
                }
                else
                {
                    LoggerManager.LogInfo($"[DatosActividadesRealizadas] El usuario {emailUser} está consultando actividades realizadas para la actividad '{actividad}'.");
                }
                if (actividad.Equals(""))
                {
                    var actividadesRealizadas = (from ar in db.ActividadRealizada
                                                 where ar.EstaActiva == 1
                                                 orderby ar.Descripcion
                                                 select new
                                                 {
                                                     nombre = ar.Descripcion,
                                                     id = ar.Secuencial
                                                 }).ToList();

                    var resp = new
                    {
                        datos = actividadesRealizadas,
                        success = true
                    };

                    return Json(resp);
                }
                else
                {
                    var actividadesRealizadas = (from ar in db.ActividadRealizada
                                                 where ar.EstaActiva == 1
                                                 orderby ar.Descripcion
                                                 select new
                                                 {
                                                     nombre = ar.Descripcion,
                                                     id = ar.Secuencial
                                                 }).ToList();

                    var actividadesRealizadasEspecificas = (from aar in db.ActividadActividadRealizada
                                                            join ar in db.ActividadRealizada on aar.SecuencialActividadRealizada equals ar.Secuencial
                                                            join a in db.Actividad on aar.SecuencialActividad equals a.Secuencial
                                                            where a.Descripcion.Equals(actividad) && ar.EstaActiva == 1 && aar.EstaActiva == 1 && a.EstaActiva == 1
                                                            orderby ar.Descripcion
                                                            select new
                                                            {
                                                                nombre = ar.Descripcion,
                                                                id = ar.Secuencial
                                                            }).Distinct().ToList();

                    var secuencialActividad = (from a in db.Actividad
                                               where a.Descripcion.Equals(actividad)
                                               select a.Secuencial
                                       ).ToList();

                    var resp = new
                    {
                        idActividad = secuencialActividad[0],
                        datos = actividadesRealizadas,
                        datosE = actividadesRealizadasEspecificas,
                        success = true
                    };

                    return Json(resp);

                }
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DatosActividadesRealizadas] Error inesperado al consultar actividades realizadas: {e.Message}");
                var resp = new
                {
                    success = false,
                    message = "Error inesperado al consultar actividades realizadas."
                };

                return Json(resp);
            }

        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        /*Solo para las descripciones de las relaciones, no para las otras*/
        public ActionResult DatosDescripcionCatalogo(int idTabla, int pos)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[DatosDescripcionCatalogo] El usuario {emailUser} está consultando descripciones del catálogo para la tabla ID {idTabla} en posición {pos}.");
                string nombreTabla = (from t in db.TablaCatalogo
                                      where t.Secuencial == idTabla
                                      select t.NombreMostrar).FirstOrDefault();

                /*Tratando de hacerlo dinámico*/
                string nombreNegocioTabla = db.TablaCatalogo.Where(x => x.NombreMostrar == nombreTabla && x.EstaActivo == 1).Select(x => x.NombreNegocio).FirstOrDefault();

                //Obteniendo el dbSet de la tabla
                Type typeAccesoDatos = db.GetType();
                PropertyInfo propertyTabla = typeAccesoDatos.GetProperty(nombreNegocioTabla);
                MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet

                //Buscando todas            
                var allData = System.Linq.Enumerable.ToList((IEnumerable<object>)dbSetTable);

                List<object> resultD = new List<object>();
                foreach (var data in allData)
                {
                    //Buscando si tiene el atributo esta activo
                    Type typeTable = data.GetType();
                    PropertyInfo tablePropertyActivo = typeTable.GetProperty("EstaActivo");
                    if (tablePropertyActivo == null)
                    {
                        tablePropertyActivo = typeTable.GetProperty("EstaActiva");
                    }

                    if (tablePropertyActivo != null)
                    {
                        if (int.Parse(tablePropertyActivo.GetValue(data).ToString()) == 1)//Esta activo el dato
                        {
                            PropertyInfo tablePropertySecuencial = typeTable.GetProperty("Secuencial");

                            PropertyInfo tablePropertyDesc = typeTable.GetProperty("NombreMostrar");
                            string desc = "";
                            if (tablePropertyDesc == null)
                            {
                                tablePropertyDesc = typeTable.GetProperty("Codigo");
                                if (tablePropertyDesc == null)
                                {
                                    tablePropertyDesc = typeTable.GetProperty("Descripcion");
                                    if (tablePropertyDesc == null)
                                    {
                                        if (nombreNegocioTabla == "Persona")
                                        {
                                            PropertyInfo tablePropertyNombre = typeTable.GetProperty("Nombre1");
                                            PropertyInfo tablePropertyApellido = typeTable.GetProperty("Apellido1");

                                            desc = tablePropertyNombre.GetValue(data).ToString() + " " + tablePropertyApellido.GetValue(data).ToString();
                                        }
                                    }
                                }
                            }

                            if (tablePropertyDesc != null)
                            {
                                desc = tablePropertyDesc.GetValue(data).ToString();
                            }

                            resultD.Add(new
                            {
                                id = tablePropertySecuencial.GetValue(data),
                                desc = desc
                            });
                        }
                    }
                    else if (nombreNegocioTabla == "Persona")
                    {
                        var usuario = System.Linq.Enumerable.ToList((IEnumerable<object>)typeTable.GetProperty("usuario").GetValue(data));


                        foreach (var user in usuario)
                        {
                            Type tipoUsuario = user.GetType();
                            PropertyInfo propertyEstaActivo = tipoUsuario.GetProperty("EstaActivo");
                            string EstaActivo = propertyEstaActivo.GetValue(user).ToString();
                            if (EstaActivo.Equals("1"))
                            {
                                PropertyInfo tablePropertySecuencial = typeTable.GetProperty("Secuencial");
                                PropertyInfo tablePropertyNombre = typeTable.GetProperty("Nombre1");
                                PropertyInfo tablePropertyApellido = typeTable.GetProperty("Apellido1");
                                string desc = "";
                                desc = tablePropertyNombre.GetValue(data).ToString() + " " + tablePropertyApellido.GetValue(data).ToString();

                                resultD.Add(new
                                {
                                    id = tablePropertySecuencial.GetValue(data),
                                    desc = desc.ToUpper()
                                });

                                resultD.Sort((x1, x2) => String.Compare(x1.GetType().GetProperty("desc").GetValue(x1).ToString(), x2.GetType().GetProperty("desc").GetValue(x2).ToString()));
                            }
                        }
                    }

                }

                var respD = new
                {
                    success = true,
                    datos = resultD,
                    pos = pos
                };
                return Json(respD);

                /*Fin de Tratando de hacerlo dinámico*/

            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DatosDescripcionCatalogo] Error inesperado al consultar descripciones del catálogo: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al consultar descripciones del catálogo."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult GuardarDatosCatalogo(string datos)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[GuardarDatosCatalogo] El usuario {emailUser} está iniciando una operación en el catálogo de datos.");
                //variables auxiliares para una insercion en el catalogo ACTIVIDAD
                string nombreActividad = "";
                string codigoActividad = "";

                var s = new JavaScriptSerializer();
                var jsonObj = s.Deserialize<dynamic>(datos);

                string tabla = (string)jsonObj["tabla"];
                var data = jsonObj["datos"];

                object idTupla = data["idTuplaCatalogo"];
                string nombreTipo = idTupla.GetType().Name;
                int id = 0;
                if (nombreTipo == "Int32")
                {
                    id = (int)data["idTuplaCatalogo"];
                }
                else
                {
                    id = int.Parse(data["idTuplaCatalogo"]);
                }

                int key = id;

                /*tratando de hacerlo dinamico*/
                string nombreNegocioTabla = db.TablaCatalogo.Where(x => x.NombreMostrar == tabla && x.EstaActivo == 1).Select(x => x.NombreNegocio).FirstOrDefault();

                //Obteniendo el dbSet de la tabla
                Type typeAccesoDatos = db.GetType();
                PropertyInfo propertyTabla = typeAccesoDatos.GetProperty(nombreNegocioTabla);
                MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet   

                //Type del model relativo a la tabla
                Type typeNewObj = Type.GetType("SifizPlanning.Models." + nombreNegocioTabla);

                //Creando el objeto u opteniendolo según el caso
                object newObj = null;
                if (id != 0)//Actualizar la tupla en la bd
                {
                    MethodInfo metodoFind = typePropertyTabla.GetMethod("Find");
                    object[] pId = new object[1] { new object[1] { id } };
                    newObj = metodoFind.Invoke(dbSetTable, pId);

                    PropertyInfo property = typeNewObj.GetProperty("NumeroVerificador");

                    if ((int)data["verificador"] != (int)property.GetValue(newObj))
                    {
                        var r = new
                        {
                            success = false,
                            errorVerificador = 1,
                            msg = "No se pudo actualizar porque cambió el valor del objeto, actualice e intente nuevamente"
                        };
                        return Json(r);
                    }
                }
                else //Nueva tupla a la bd
                {
                    newObj = Activator.CreateInstance(typeNewObj);
                }

                //Adicionando los parámetros
                foreach (var d in data)
                {

                    string keyAdd = d.Key;
                    if (keyAdd == "idTuplaCatalogo")
                        keyAdd = "Secuencial";
                    else if (keyAdd == "verificador")
                        keyAdd = "NumeroVerificador";

                    if (keyAdd == "Secuencial")
                        continue;

                    PropertyInfo property = typeNewObj.GetProperty(keyAdd);
                    Type propertyType = property.PropertyType;

                    object valor = d.Value;
                    if (nombreNegocioTabla == "FuncionalidadCliente" && keyAdd == "SecuencialCliente")
                    {
                        valor = (from cl in db.Cliente
                                 where cl.Descripcion == (string)valor
                                 select cl.Secuencial).ToList()[0];
                    }
                    else if (nombreNegocioTabla == "FuncionalidadCliente" && keyAdd == "SecuencialFuncionalidad")
                    {
                        valor = (from fn in db.Funcionalidad
                                 where fn.Descripcion == (string)valor
                                 select fn.Secuencial).ToList()[0];
                    }
                    else if (nombreNegocioTabla == "GestorServicios" && keyAdd == "SecuencialColaborador")
                    {
                        valor = db.Colaborador.Where(p => p.persona.Nombre1 + " " + p.persona.Apellido1 == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    }
                    //else if (nombreNegocioTabla == "Proyectos" && keyAdd == "SecuencialRepositorio")
                    //{
                    //    valor = db.Repositorio.Where(p => p.Descripcion == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    //}
                    //else if (nombreNegocioTabla == "Proyectos" && keyAdd == "SecuencialVersionDesarrollo")
                    //{
                    //    valor = db.VersionDesarrollo.Where(p => p.Descripcion == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    //}
                    //else if (nombreNegocioTabla == "Proyectos" && keyAdd == "SecuencialVersionBaseDatos")
                    //{
                    //    valor = db.VersionBaseDatos.Where(p => p.Descripcion == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    //}
                    //else if (nombreNegocioTabla == "Proyectos" && keyAdd == "SecuencialCliente")
                    //{
                    //    valor = db.Cliente.Where(p => p.Descripcion == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    //}
                    else if (nombreNegocioTabla == "ClienteAuxiliar" && keyAdd == "SecuencialLiderProyecto")
                    {
                        valor = db.Colaborador.Where(p => p.persona.Nombre1 + " " + p.persona.Apellido1 == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    }
                    else if (nombreNegocioTabla == "ClienteAuxiliar" && keyAdd == "SecuencialUbicacion")
                    {
                        valor = db.ResponsableProyectos.Where(p => p.Nombre == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    }
                    else if (nombreNegocioTabla == "ClienteAuxiliar" && keyAdd == "SecuencialResponsableCodigo")
                    {
                        valor = db.ResponsableProyectos.Where(p => p.Nombre == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    }
                    else if (nombreNegocioTabla == "ClienteAuxiliar" && keyAdd == "SecuencialResponsableAcceso")
                    {
                        valor = db.ResponsableProyectos.Where(p => p.Nombre == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    }
                    else if (nombreNegocioTabla == "ClienteAuxiliar" && keyAdd == "SecuencialResponsablePublicacion")
                    {
                        valor = db.ResponsableProyectos.Where(p => p.Nombre == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    }
                    //else if (nombreNegocioTabla == "Proyectos" && keyAdd == "SecuencialGestor")
                    //{
                    //    valor = db.Colaborador.Where(p => p.persona.Nombre1 + " " + p.persona.Apellido1 == (string)valor).Select(c => c.Secuencial).FirstOrDefault();
                    //}


                    else if (propertyType.FullName == "System.Int32" && d.Value.GetType().FullName != "System.Int32")
                    {
                        valor = int.Parse((string)valor);
                    }
                    else if (propertyType.FullName == "System.Decimal")
                    {
                        valor = decimal.Parse((string)valor);
                    }
                    else if (propertyType.FullName == "System.Double")
                    {
                        valor = double.Parse((string)valor);
                    }
                    else if (propertyType.FullName == "System.DateTime")
                    {
                        valor = DateTime.Parse((string)valor);
                    }
                    else if (nombreNegocioTabla == "Modulo" && keyAdd == "SecuencialTipoModulo")
                    {
                        valor = (from tm in db.TipoModulo
                                 where tm.Descripcion == (string)valor
                                 select tm.Secuencial).ToList()[0];
                    }


                    property.SetValue(newObj, valor);
                }

                if (id == 0)
                {
                    //Adicionando a la bd
                    MethodInfo metodoAdd = typePropertyTabla.GetMethod("Add");
                    metodoAdd.Invoke(dbSetTable, new object[] { newObj });
                    if (nombreNegocioTabla.Equals("Actividad"))
                    {
                        nombreActividad = newObj.GetType().GetProperty("Descripcion").GetValue(newObj).ToString();
                        codigoActividad = newObj.GetType().GetProperty("Codigo").GetValue(newObj).ToString();
                    }
                }
                //type.getMethod("Add").Invoke(dbSetTable, new object[] { newObj })
                LoggerManager.LogSensitiveOperation(
                    "Creación de registro en catálogo",
                    $"El usuario {emailUser} creó un nuevo registro en la tabla {tabla} ({nombreNegocioTabla}).",
                    emailUser
                );

                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    msg = "Se ha realizado la operación correctamente.",
                    nombreActividad,
                    codigoActividad
                });
                /*Fin de tratando de hacerlo dinamico*/
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[GuardarDatosCatalogo] Error inesperado al procesar datos del catálogo para el usuario: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al procesar datos del catálogo."
                };
                return Json(resp);
            }
        }


        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult GuardarActividadesRealizadas(int idActividad, string actividad, string codigo, bool actividadNueva, string datos)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[GuardarActividadesRealizadas] El usuario {emailUser} está guardando actividades realizadas para la actividad {actividad} (código: {codigo}), ID: {idActividad}.");
                if (actividadNueva)
                {
                    //Si se esta ingresando una nueva actividad se debe buscar en la BD el secuencial de la actividad creada 
                    idActividad = (from a in db.Actividad
                                   where a.Descripcion == actividad && a.Codigo.ToString() == codigo
                                   select a.Secuencial).ToList()[0];
                }
                else
                {
                    //Si se esta editando una actividad, se borran sus actividades realizadas actuales para ingresar las nuevas
                    Type typeAccesoDatos = db.GetType();
                    PropertyInfo propertyTabla = typeAccesoDatos.GetProperty("ActividadActividadRealizada");
                    MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                    object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                    Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

                    var actividadesBorrar = db.ActividadActividadRealizada.Where(x => x.SecuencialActividad == idActividad).ToList();

                    MethodInfo metodoRemove = typePropertyTabla.GetMethod("Remove");

                    foreach (var act in actividadesBorrar)
                    {
                        object newObj = metodoRemove.Invoke(dbSetTable, new object[1] { act });
                        db.SaveChanges();
                    }
                }

                string[] actividadesRealizadas = datos.Split('|');
                actividadesRealizadas = actividadesRealizadas.Skip(1).ToArray();


                foreach (var act in actividadesRealizadas)
                {
                    object[] actividadRealizada = act.Split(':');
                    int idActividadRealizada = int.Parse((string)actividadRealizada[1]);
                    string descripcionActividadRealizada = (string)actividadRealizada[0];

                    Type typeAccesoDatos = db.GetType();
                    PropertyInfo propertyTabla = typeAccesoDatos.GetProperty("ActividadActividadRealizada");
                    MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                    object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                    Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet  


                    Type typeNewObj = Type.GetType("SifizPlanning.Models.ActividadActividadRealizada");
                    object newObj = Activator.CreateInstance(typeNewObj);

                    typeNewObj.GetProperty("SecuencialActividad").SetValue(newObj, idActividad);
                    typeNewObj.GetProperty("SecuencialActividadRealizada").SetValue(newObj, idActividadRealizada);
                    typeNewObj.GetProperty("EstaActiva").SetValue(newObj, decimal.Parse("1"));
                    typeNewObj.GetProperty("NumeroVerificador").SetValue(newObj, 1);

                    MethodInfo metodoAdd = typePropertyTabla.GetMethod("Add");
                    metodoAdd.Invoke(dbSetTable, new object[] { newObj });

                    db.SaveChanges();
                    LoggerManager.LogSensitiveOperation(
                    "Creación de actividad realizada",
                    $"El usuario {emailUser} creó una nueva actividad realizada '{descripcionActividadRealizada}' para la actividad {actividad} (ID: {idActividad}).",
                    emailUser
                );

                }
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[GuardarActividadesRealizadas] Error inesperado al guardar actividades realizadas para el usuario: {e.Message}");

                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al guardar actividades."
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult EliminarTuplasActividadesRealizadas(int idActividad)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[EliminarTuplasActividadesRealizadas] El usuario {emailUser} está eliminando actividades realizadas para la actividad con ID: {idActividad}.");
                //Obteniendo el dbSet de la tabla
                Type typeAccesoDatos = db.GetType();
                PropertyInfo propertyTabla = typeAccesoDatos.GetProperty("ActividadActividadRealizada");
                MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

                var actividadesBorrar = db.ActividadActividadRealizada.Where(x => x.SecuencialActividad == idActividad).ToList();

                MethodInfo metodoRemove = typePropertyTabla.GetMethod("Remove");

                foreach (var act in actividadesBorrar)
                {
                    object newObj = metodoRemove.Invoke(dbSetTable, new object[1] { act });
                    db.SaveChanges();
                    LoggerManager.LogSensitiveOperation(
                    "Eliminación de actividad realizada",
                    $"El usuario {emailUser} eliminó una actividad realizada de la actividad con ID: {idActividad}.",
                    emailUser
                );
                }

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EliminarTuplasActividadesRealizadas] Error inesperado al eliminar actividades realizadas: {e.Message}");

                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al eliminar actividades."
                });
            }

        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult EliminarTuplaCatalogos(string tabla, int id)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"[EliminarTuplaCatalogos] El usuario {emailUser} está eliminando un registro de la tabla {tabla} con ID: {id}.");
                /*Tratando de hacer dinámico el eliminar*/
                string nombreNegocioTabla = db.TablaCatalogo.Where(x => x.NombreMostrar == tabla && x.EstaActivo == 1).Select(x => x.NombreNegocio).FirstOrDefault();

                //Obteniendo el dbSet de la tabla
                Type typeAccesoDatos = db.GetType();
                PropertyInfo propertyTabla = typeAccesoDatos.GetProperty(nombreNegocioTabla);
                MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

                //Buscando el elemento
                MethodInfo metodoFind = typePropertyTabla.GetMethod("Find");
                object[] pId = new object[1] { new object[1] { id } };
                object findObj = metodoFind.Invoke(dbSetTable, pId);

                //Eliminando el elemento
                MethodInfo metodoRemove = typePropertyTabla.GetMethod("Remove");
                object newObj = metodoRemove.Invoke(dbSetTable, new object[1] { findObj });
                db.SaveChanges();
                LoggerManager.LogSensitiveOperation(
                    "Eliminación de registro en catálogo",
                    $"El usuario {emailUser} eliminó el registro con ID {id} de la tabla {tabla} ({nombreNegocioTabla}).",
                    emailUser
                );

                var respD = new
                {
                    success = true,
                    msg = "Se ha realizado correctamente la operación"
                };
                return Json(respD);
                /*Fin de Tratando de hacer dinámico el eliminar*/
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[EliminarTuplaCatalogos] Error inesperado al eliminar registro de catálogo: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al eliminar registro de catálogo."
                };
                return Json(resp);
            }
        }

        //------------------- Action de Catálogos -------------------
        [HttpPost]
        [Authorize]
        public ActionResult DarPaises()
        {
            try
            {
                var paises = (from p in db.Pais
                              where p.EstaActivo == 1
                              select new
                              {
                                  id = p.Secuencial,
                                  nombre = p.Descripcion
                              }
                              ).ToList();
                var resp = new
                {
                    success = true,
                    paises = paises
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarPaises] Error inesperado al obtener datos de paises: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "rror inesperado al obtener datos de paises."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarCargos()
        {
            try
            {
                var cargos = (from p in db.Cargo
                              where p.EstaActivo == 1
                              select new
                              {
                                  id = p.Secuencial,
                                  nombre = p.Descripcion
                              }
                              ).ToList();
                var resp = new
                {
                    success = true,
                    cargos = cargos
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarCargos] Error inesperado al obtener datos de los cargos: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al obtener datos de los cargos."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarSedes()
        {
            try
            {
                var sedes = (from p in db.Sede
                             where p.EstaActivo == 1
                             orderby p.Descripcion
                             select new
                             {
                                 id = p.Secuencial,
                                 nombre = p.Descripcion
                             }
                              ).ToList();
                var resp = new
                {
                    success = true,
                    sedes = sedes
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarSedes] Error inesperado al obtener datos de las sedes: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al obtener datos de las sedes."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarRoles()
        {
            try
            {
                var roles = (from r in db.Rol
                             where r.EstaActivo == 1
                             select new
                             {
                                 id = r.Secuencial,
                                 nombre = r.Codigo
                             }
                              ).ToList();
                var resp = new
                {
                    success = true,
                    roles = roles
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarRoles] Error inesperado al obtener los roles: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al obtener los roles."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarTiposModulosSistema()
        {

            try
            {

                var data = (from tm in db.TipoModulo
                            where tm.EstaActivo == 1
                            select tm.Descripcion).ToList();

                return Json(new
                {
                    success = true,
                    datos = data
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarTiposModulosSistema] Error inesperado al obtener los tipos de módulos del sistema: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los tipos de módulos del sistema."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarModulosSistema()
        {
            try
            {
                var data = (from s in db.SysMenu
                            select s.Descripcion).ToList();

                return Json(new
                {
                    success = true,
                    datos = data
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarModulosSistema] Error inesperado al obtener los módulos del sistema: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los módulos del sistema."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarModulos()
        {
            try
            {
                var modulos = (from m in db.Modulo
                               where m.EstaActivo == 1
                               orderby m.Codigo, m.Descripcion
                               select new
                               {
                                   id = m.Secuencial,
                                   nombre = m.Codigo.ToUpper() + "-" + m.Descripcion.ToUpper(),
                                   descripcion = m.Descripcion.ToUpper(),
                               }
                              ).ToList();
                var resp = new
                {
                    success = true,
                    modulos = modulos
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarModulos] Error inesperado al obtener los módulos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los módulos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarTecnologias()
        {
            try
            {
                var tecnologias = (from tec in db.TecnologiasYProcesos
                                   where tec.EstaActivo == 1
                                   orderby tec.Codigo, tec.Descripcion
                                   select new
                                   {
                                       id = tec.Secuencial,
                                       nombre = tec.Codigo.ToUpper() + "-" + tec.Descripcion.ToUpper(),
                                       descripcion = tec.Descripcion.ToUpper(),
                                   }
                              ).ToList();
                var resp = new
                {
                    success = true,
                    tecnologias = tecnologias
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarTecnologias] Error inesperado al obtener las tecnologías: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener las tecnologías."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarEstadosTickets()
        {
            try
            {
                var estados = (from e in db.EstadoTicket
                               where e.EstaActivo == 1
                               orderby e.Codigo, e.Descripcion
                               select new
                               {
                                   id = e.Secuencial,
                                   codigo = e.Codigo.ToUpper(),
                                   descripcion = e.Descripcion.ToUpper(),
                               }
                              ).ToList();
                var resp = new
                {
                    success = true,
                    estados = estados
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarEstadosTickets] Error inesperado al obtener los estados de ticket: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los estados de ticket."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarTicketVersionCliente()
        {
            try
            {
                var ticketVersionClientes = (from tvc in db.TicketVersionCliente
                                             where tvc.EstaActivo == 1
                                             select new
                                             {
                                                 id = tvc.Secuencial,
                                                 codigo = tvc.Codigo,
                                                 nombre = tvc.Descripcion
                                             });
                var resp = new
                {
                    success = true,
                    ticketVersionClientes = ticketVersionClientes
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarTicketVersionCliente] Error inesperado al obtener los ticket versión cliente: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los ticket versión cliente."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarClientes(string filtro = "")
        {
            try
            {
                List<object> clientes = new List<object>();
                if (filtro != "")
                {
                    clientes = (from c in db.Cliente
                                where c.EstaActivo == 1 &&
                                      (c.Descripcion.Contains(filtro) || c.Codigo.Contains(filtro))
                                orderby c.Codigo, c.Descripcion
                                select new
                                {
                                    id = c.Secuencial,
                                    nombre = c.Codigo.ToUpper() + "-" + c.Descripcion.ToUpper()
                                }).ToList<object>();
                }
                else
                {
                    clientes = (from c in db.Cliente
                                where c.EstaActivo == 1
                                orderby c.Codigo, c.Descripcion
                                select new
                                {
                                    id = c.Secuencial,
                                    nombre = c.Codigo.ToUpper() + "-" + c.Descripcion.ToUpper()
                                }).ToList<object>();
                }
                var resp = new
                {
                    success = true,
                    clientes = clientes
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarClientes] Error inesperado al obtener los clientes: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los clientes."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarEstadosContrato(string filtro = "")
        {
            try
            {
                List<object> estadosContrato = new List<object>();
                if (filtro != "")
                {
                    estadosContrato = (from e in db.EstadoContrato
                                       where e.EstaActivo == 1 &&
                                             (e.Descripcion.Contains(filtro) || e.Codigo.Contains(filtro))
                                       orderby e.Codigo
                                       select new
                                       {
                                           id = e.Secuencial,
                                           nombre = e.Codigo.ToUpper()
                                       }).ToList<object>();
                }
                else
                {
                    estadosContrato = (from e in db.EstadoContrato
                                       where e.EstaActivo == 1
                                       orderby e.Codigo
                                       select new
                                       {
                                           id = e.Secuencial,
                                           nombre = e.Codigo.ToUpper()
                                       }).ToList<object>();
                }
                var resp = new
                {
                    success = true,
                    estadosContrato = estadosContrato
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarEstadosContrato] Error inesperado al obtener los estados contratos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los estados contratos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarTiposPlazo(string filtro = "")
        {
            try
            {
                List<object> tiposPlazo = new List<object>();
                if (filtro != "")
                {
                    tiposPlazo = (from e in db.TipoPlazo
                                  where e.EstaActivo == 1 &&
                                        (e.Descripcion.Contains(filtro) || e.Codigo.Contains(filtro))
                                  orderby e.Codigo
                                  select new
                                  {
                                      id = e.Secuencial,
                                      nombre = e.Codigo.ToUpper()
                                  }).ToList<object>();
                }
                else
                {
                    tiposPlazo = (from e in db.TipoPlazo
                                  where e.EstaActivo == 1
                                  orderby e.Codigo
                                  select new
                                  {
                                      id = e.Secuencial,
                                      nombre = e.Codigo.ToUpper()
                                  }).ToList<object>();
                }
                var resp = new
                {
                    success = true,
                    tiposPlazo = tiposPlazo
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarTiposPlazo] Error inesperado al obtener los tipos de plazos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los tipos de plazos:."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarFasesContrato(string filtro = "")
        {
            try
            {
                List<object> fasesContrato = new List<object>();
                if (filtro != "")
                {
                    fasesContrato = (from e in db.FaseContrato
                                     where e.EstaActivo == 1 &&
                                           (e.Descripcion.Contains(filtro) || e.Codigo.Contains(filtro))
                                     orderby e.Codigo
                                     select new
                                     {
                                         id = e.Secuencial,
                                         nombre = e.Codigo.ToUpper()
                                     }).ToList<object>();
                }
                else
                {
                    fasesContrato = (from e in db.FaseContrato
                                     where e.EstaActivo == 1
                                     orderby e.Codigo
                                     select new
                                     {
                                         id = e.Secuencial,
                                         nombre = e.Codigo.ToUpper()
                                     }).ToList<object>();
                }
                var resp = new
                {
                    success = true,
                    fasesContrato = fasesContrato
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarFasesContrato] Error inesperado al obtener las fases del contrato: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener las fases del contrato."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarTiposMotivoTrabajo(string filtro = "")
        {
            try
            {
                List<object> tipoMotivoTrabajo = new List<object>();
                if (filtro != "")
                {
                    if (filtro == "SOPORTE Y MANTENIMIENTO")
                    {
                        filtro = "PENDIENTES";
                    }
                    tipoMotivoTrabajo = (from t in db.TipoMotivoTrabajo
                                         where t.EstaActivo == 1 &&
                                               (t.Descripcion.Contains(filtro) || t.Codigo.Contains(filtro))
                                         orderby t.Codigo
                                         select new
                                         {
                                             id = t.Secuencial,
                                             codigo = t.Codigo.ToUpper() != "PENDIENTES" ? t.Codigo.ToUpper() : "SOPORTE Y MANTENIMIENTO",
                                             descripcion = t.Descripcion
                                         }).ToList<object>();
                }
                else
                {
                    tipoMotivoTrabajo = (from t in db.TipoMotivoTrabajo
                                         where t.EstaActivo == 1
                                         orderby t.Codigo
                                         select new
                                         {
                                             id = t.Secuencial,
                                             codigo = t.Codigo.ToUpper() != "PENDIENTES" ? t.Codigo.ToUpper() : "SOPORTE Y MANTENIMIENTO",
                                             descripcion = t.Descripcion
                                         }).ToList<object>();
                }
                var resp = new
                {
                    success = true,
                    tipoMotivoTrabajo = tipoMotivoTrabajo
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarTiposMotivoTrabajo] Error inesperado al obtener los tipos motivo de trabajo: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los tipos motivo de trabajo."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarNombresClientes(string filtro = "")
        {
            try
            {
                var clientes = (from c in db.Cliente
                                where c.EstaActivo == 1
                                orderby c.Descripcion
                                select new
                                {
                                    id = c.Secuencial,
                                    nombre = c.Descripcion.ToUpper()
                                }).ToList();
                return Json(new
                {
                    success = true,
                    clientes = clientes
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarNombresClientes] Error inesperado al obtener los nombre de los clientes: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los nombre de los clientes."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarLugarTareas()
        {
            try
            {
                var lugares = (from l in db.LugarTarea
                               where l.EstaActivo == 1
                               orderby l.Codigo, l.Descripcion
                               select new
                               {
                                   id = l.Secuencial,
                                   nombre = l.Codigo.ToUpper() + "-" + l.Descripcion.ToUpper()
                               }
              ).ToList();
                var resp = new
                {
                    success = true,
                    lugares = lugares
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarLugarTareas] Error inesperado al obtener los lugares de las tareas: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los lugares de las tareas."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarActividades()
        {
            try
            {
                var actividades = (from a in db.Actividad
                                   where a.EstaActiva == 1
                                   orderby a.Codigo, a.Descripcion
                                   select new
                                   {
                                       id = a.Secuencial,
                                       nombre = a.Codigo.ToUpper() + "-" + a.Descripcion.ToUpper()
                                   }).ToList();
                var resp = new
                {
                    success = true,
                    actividades = actividades
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarActividades] Error inesperado al obtener las actividades: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener las actividades."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarCoordinadores()
        {
            try
            {
                var coordinadores = (from t in db.Colaborador
                                     join
                 p in db.Persona on t.persona equals p
                                     join
                                     us in db.Usuario on p.Secuencial equals us.SecuencialPersona
                                     where us.EstaActivo == 1 && us.Email != "canulado@sifizsoft.com" && us.Email != "sifizplanning@sifizsoft.com"
                                     select new
                                     {
                                         id = t.Secuencial,
                                         nombre = p.Nombre1 + " " + p.Apellido1
                                     }
                                   ).ToList();

                coordinadores.Sort((x1, x2) => String.Compare(x1.nombre, x2.nombre));

                var resp = new
                {
                    success = true,
                    coordinadores = coordinadores
                };

                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarCoordinadores] Error inesperado al obtener los cordinadores: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los cordinadores."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarRepositorios()
        {
            try
            {
                var repositorios = (from t in db.Repositorio
                                    where t.EstaActivo == 1
                                    select new
                                    {
                                        id = t.Secuencial,
                                        nombre = t.Descripcion
                                    }).ToList();
                var resp = new
                {
                    success = true,
                    repositorios = repositorios
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarRepositorios] Error inesperado al obtener los repositorios: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los repositorios."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarResponsablesProyecto(bool esTecnico, bool todos)
        {
            try
            {
                var responsables = db.ResponsableProyectos.Select(s => s).AsQueryable();
                if (!todos && esTecnico)
                {
                    responsables = responsables.Where(s => s.EsTecnico == 1);
                }
                if (!todos && !esTecnico)
                {
                    responsables = responsables.Where(s => s.EsTecnico == 0);
                }
                var result = responsables.Select(s => new { id = s.Secuencial, nombre = s.Nombre }).ToList();
                var resp = new
                {
                    success = true,
                    responsables = result
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarResponsablesProyecto] Error inesperado al obtener los responsables de proyectos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los responsables de proyectos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarVersionesDesarrollo()
        {
            try
            {
                var versionesDesarrollo = (from t in db.VersionDesarrollo
                                           where t.EstaActivo == 1
                                           select new
                                           {
                                               id = t.Secuencial,
                                               nombre = t.Descripcion
                                           }).ToList();

                var resp = new
                {
                    success = true,
                    versionesDesarrollo = versionesDesarrollo
                };

                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarVersionesDesarrollo] Error inesperado al obtener las versiones de desarrollo: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener las versiones de desarrollos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarVersionesBaseDatos()
        {
            try
            {
                var versionesBaseDatos = (from t in db.VersionBaseDatos
                                          where t.EstaActivo == 1
                                          select new
                                          {
                                              id = t.Secuencial,
                                              nombre = t.Descripcion
                                          }).ToList();
                var resp = new
                {
                    success = true,
                    versionesBaseDatos = versionesBaseDatos
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarVersionesBaseDatos] Error inesperado al obtener las versiones de base de datos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener las versiones de base de datos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarMotivosDevolucionTicket()
        {
            try
            {
                var motivos = (from m in db.MotivoDevolucionTicket
                               where m.EstaActivo == 1
                               orderby m.Codigo, m.Descripcion
                               select new
                               {
                                   id = m.Secuencial,
                                   nombre = m.Codigo.ToUpper()/* + "-" + m.Descripcion.ToUpper()*/,
                                   descripcion = m.Descripcion.ToUpper()
                               }).ToList();
                var resp = new
                {
                    success = true,
                    motivos = motivos
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarMotivosDevolucionTicket] Error inesperado al obtener los motivos devolución ticket: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los motivos devolución ticket."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarCatalogos(string filtro = "")
        {
            try
            {
                List<object> catalogos = new List<object>();
                if (filtro != "")
                {
                    catalogos = (from tc in db.TablaCatalogo
                                 where tc.EstaActivo == 1 &&
                                     (tc.Descripcion.Contains(filtro) || tc.NombreMostrar.Contains(filtro))
                                 orderby tc.NombreMostrar
                                 select new
                                 {
                                     id = tc.Secuencial,
                                     nombre = tc.NombreMostrar
                                 }).ToList<object>();
                }
                else
                {
                    catalogos = (from tc in db.TablaCatalogo
                                 where tc.EstaActivo == 1
                                 orderby tc.NombreMostrar
                                 select new
                                 {
                                     id = tc.Secuencial,
                                     nombre = tc.NombreMostrar
                                 }).ToList<object>();
                }
                var resp = new
                {
                    success = true,
                    catalogos = catalogos
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarCatalogos] Error inesperado al obtener los catálogos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los catálogos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarDepartamentos()
        {
            try
            {
                var departamentos = (from tc in db.Departamento
                                     where tc.EstaActivo == 1
                                     select new
                                     {
                                         id = tc.Secuencial,
                                         nombre = tc.Descripcion
                                     }).ToList();
                var resp = new
                {
                    success = true,
                    departamentos = departamentos
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarDepartamentos] Error inesperado al obtener los departamentos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los departamentos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarTiposContratosCliente()
        {
            try
            {
                var tiposContrato = (from tc in db.TipoContratoCliente
                                     where tc.EstaActivo == 1
                                     select new
                                     {
                                         id = tc.Secuencial,
                                         nombre = tc.Descripcion
                                     }).ToList();
                var resp = new
                {
                    success = true,
                    tiposContrato = tiposContrato
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarTiposContratosCliente] Error inesperado al obtener los tipos de contratos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los tipos de contratos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarNombresFuncionalidades()
        {
            try
            {
                var funcionalidades = (from fn in db.Funcionalidad
                                       where fn.EstaActiva == 1
                                       orderby fn.Descripcion
                                       select new
                                       {
                                           id = fn.Secuencial,
                                           nombre = fn.Descripcion
                                       }).ToList();
                return Json(new
                {
                    success = true,
                    funcionalidades = funcionalidades
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarNombresFuncionalidades] Error inesperado al obtener los nombres de funcionalidades: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los nombres de funcionalidades."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarColaboradores()
        {
            try
            {
                var colaboradores = (from colab in db.Colaborador
                                     where colab.departamento.Asignable == 1 &&
                                           colab.persona.usuario.FirstOrDefault().EstaActivo == 1
                                     orderby colab.persona.Nombre1, colab.persona.Apellido1
                                     select new
                                     {
                                         id = colab.Secuencial,
                                         nombre = colab.persona.Nombre1 + " " + colab.persona.Apellido1
                                     }).ToList();
                return Json(new
                {
                    success = true,
                    colaboradores = colaboradores
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarColaboradores] Error inesperado al obtener los colaboradores: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los colaboradores."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarFullColaboradores()
        {
            try
            {
                var colaboradores = (from colab in db.Colaborador
                                     where colab.persona.usuario.FirstOrDefault().EstaActivo == 1
                                     && colab.persona.Nombre1 != "ADMIN"
                                     orderby colab.persona.Nombre1, colab.persona.Apellido1
                                     select new
                                     {
                                         id = colab.Secuencial,
                                         nombre = colab.persona.Nombre1 + " " + colab.persona.Apellido1
                                     }).ToList();
                return Json(new
                {
                    success = true,
                    colaboradores = colaboradores
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarFullColaboradores] Error inesperado al obtener los todos los colaboradores: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener los todos los colaboradores."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarFuncionalidades(string filtro = "")
        {
            try
            {
                List<object> tiposContrato = new List<object>();
                if (filtro != "")
                {
                    tiposContrato = (from tc in db.Funcionalidad//todo
                                     where tc.EstaActiva == 1
                                     select new
                                     {
                                         id = tc.Secuencial,
                                         nombre = tc.Descripcion
                                     }).ToList<object>();
                }
                else
                {
                    tiposContrato = (from tc in db.Funcionalidad
                                     where tc.EstaActiva == 1
                                     select new
                                     {
                                         id = tc.Secuencial,
                                         nombre = tc.Descripcion
                                     }).ToList<object>();
                }
                var resp = new
                {
                    success = true,
                    tiposContrato = tiposContrato
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarFuncionalidades] Error inesperado al obtener las funcionalidades: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener las funcionalidades."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarPrioridadesTicket()
        {
            try
            {
                var prioridadesTicket = (from tc in db.PrioridadTicket
                                         where tc.EstaActiva == 1
                                         select new
                                         {
                                             id = tc.Secuencial,
                                             codigo = tc.Codigo,
                                             nombre = tc.Codigo + " - " + tc.Descripcion
                                         }).ToList();
                var resp = new
                {
                    success = true,
                    prioridades = prioridadesTicket
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarPrioridadesTicket] Error inesperado al obtener las prioridades: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener las prioridades."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarActividadRealizada()
        {
            try
            {
                var actividadesRealizadas = (from tc in db.ActividadRealizada
                                             where tc.EstaActiva == 1
                                             orderby tc.Descripcion
                                             select new
                                             {
                                                 id = tc.Secuencial,
                                                 nombre = tc.Descripcion
                                             }).ToList();
                var resp = new
                {
                    success = true,
                    actividadesRealizadas = actividadesRealizadas
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarActividadRealizada] Error inesperado al obtener actividad realizada: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener actividad realizada."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarImplicacionErrorPuntos()
        {
            try
            {
                var implicacionesError = (from tc in db.ImplicacionError
                                          where tc.EstaActivo == 1
                                          orderby tc.NivelGravedad
                                          select new
                                          {
                                              id = tc.Secuencial,
                                              nombre = tc.Descripcion,
                                              nivel = tc.NivelGravedad
                                          }).ToList();
                var resp = new
                {
                    success = true,
                    implicacionesError = implicacionesError
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarImplicacionErrorPuntos] Error inesperado al obtener implicación error puntos: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener implicación error puntos."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarImplicacionErrorPorTipoError(int idTipoError)
        {
            try
            {
                var implicacionError = (from ie in db.ImplicacionError
                                        join tei in db.TipoErrorImplicacion on ie.Secuencial equals tei.SecuencialImplicacionError
                                        join te in db.TipoError on tei.tipoError equals te
                                        where ie.EstaActivo == 1 && tei.EstaActivo == 1 && te.EstaActivo == 1 && te.Secuencial == idTipoError
                                        select new
                                        {
                                            id = ie.Secuencial,
                                            codigo = ie.Codigo,
                                            nivel = ie.NivelGravedad
                                        }).FirstOrDefault();
                var resp = new
                {
                    success = true,
                    implicacionError = implicacionError
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarImplicacionErrorPorTipoError] Error inesperado al obtener implicación error por tipo error: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = "Error inesperado al obtener implicación error por tipo error."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarDatosCatalogo(string nombre, int vigente = 1)
        {
            try
            {
                string nombreClase = db.TablaCatalogo.Where(x => x.EstaActivo == 1 && x.NombreMostrar == nombre).Select(x => x.NombreNegocio).FirstOrDefault();

                if (nombreClase == null)
                {
                    throw new Exception("No se encontró el catálogo " + nombre);
                }

                Type clase = Type.GetType("SifizPlanning.Models." + nombreClase);

                //Obteniendo el dbSet de la tabla
                Type typeAccesoDatos = db.GetType();
                PropertyInfo propertyTabla = typeAccesoDatos.GetProperty(nombreClase);
                MethodInfo methodPropertyTabla = propertyTabla.GetMethod;
                object dbSetTable = methodPropertyTabla.Invoke(db, new object[] { });
                Type typePropertyTabla = dbSetTable.GetType();//Esto es un tipo dbSet 

                //Buscando todas            
                var allData = System.Linq.Enumerable.ToList((IEnumerable<object>)dbSetTable);

                List<object> datosCatalogo = new List<object>();
                PropertyInfo metodoGet = clase.GetProperty("Codigo");
                if (metodoGet == null)
                    metodoGet = clase.GetProperty("Descripcion");
                if (metodoGet == null)
                {
                    throw new Exception("Error, No se encontró el método get.");
                }

                if (vigente == 1)
                {//Filtrar por Vigente
                    PropertyInfo propiedadTabla = clase.GetProperty("EstaActivo");
                    if (propiedadTabla == null)
                    {
                        propiedadTabla = clase.GetProperty("EstaActiva");
                    }
                    if (propiedadTabla == null)
                    {
                        throw new Exception("Error, No se encontró el verificador de vigencia.");
                    }

                    foreach (var data in allData)
                    {
                        if (int.Parse(propiedadTabla.GetValue(data).ToString()) == 1)
                        {
                            var dato = new
                            {
                                id = clase.GetProperty("Secuencial").GetValue(data),
                                nombre = metodoGet.GetValue(data)
                            };
                            datosCatalogo.Add(dato);
                        }
                    }
                }
                else
                {
                    foreach (var data in allData)
                    {
                        var dato = new
                        {
                            id = clase.GetProperty("Secuencial").GetValue(data),
                            nombre = metodoGet.GetValue(data)
                        };
                        datosCatalogo.Add(dato);
                    }
                }
                var resp = new
                {
                    success = true,
                    datos = datosCatalogo
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarDatosCatalogo] Error inesperado al obtener datos de catálogos: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al obtener datos de catálogos."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarActividadPropuesta()
        {
            try
            {
                var actividadesPropuestas = (from tc in db.ActividadPropuesta
                                             where tc.EstaActivo == 1
                                             orderby tc.Descripcion
                                             select new
                                             {
                                                 id = tc.Secuencial,
                                                 nombre = tc.Descripcion
                                             }).ToList();
                var resp = new
                {
                    success = true,
                    proximasActividadesNT = actividadesPropuestas
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarActividadPropuesta] Error inesperado al obtener actividad propuesta: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al obtener actividad propuesta."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarCausasNoTerminacion()
        {
            try
            {
                var causasNT = (from tc in db.CausaNoTerminacion
                                where tc.EstaActivo == 1
                                orderby tc.Descripcion
                                select new
                                {
                                    id = tc.Secuencial,
                                    nombre = tc.Descripcion
                                }).ToList();
                var resp = new
                {
                    success = true,
                    causasNoTerminacion = causasNT
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarCausasNoTerminacion] Error inesperado al obtener causas no terminación: {e.Message}");

                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al obtener causas no terminación."
                };
                return Json(resp);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DarGestores()
        {
            try
            {
                var gestores = db.GestorServicios.Where(s => s.EstaActivo == 1)
                            .GroupBy(s => new
                            {
                                s.colaborador.Secuencial,
                                Nombre = s.colaborador.persona.Nombre1 + " " + s.colaborador.persona.Apellido1
                            }).Select(g => new
                            {
                                idColaborador = g.Key.Secuencial,
                                nombre = g.Key.Nombre
                            }).ToList();
                var resp = new
                {
                    success = true,
                    gestores = gestores
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"[DarGestores] Error inesperado al obtener los gestores: {e.Message}");
                var resp = new
                {
                    success = false,
                    msg = "Error inesperado al obtener los gestores."
                };
                return Json(resp);
            }
        }
    }
}


