using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.Owin;
using SifizPlanning.Models;
using SifizPlanning.Security;
using SifizPlanning.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SifizPlanning.Controllers
{

    public class ConsultasController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();

        // GET: Consultas
        public ActionResult Index()
        {
            LoggerManager.LogInfo("Index method called in ConsultasController");
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult GetClientes()
        {
            try
            {
                var clientes = db.Cliente
                    .Where(c => c.EstaActivo == 1)
                    .Select(c => new { c.Secuencial, c.Descripcion })
                    .OrderBy(c => c.Descripcion)
                    .ToList();

                return Json(new { success = true, clientes = clientes }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult GetVersionesDesarrollo()
        {
            try
            {
                var versiones = db.VersionDesarrollo
                    .Where(v => v.EstaActivo == 1)
                    .Select(v => new { v.Secuencial, v.Descripcion })
                    .OrderBy(v => v.Descripcion)
                    .ToList();

                return Json(new { success = true, versiones = versiones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult GetRepositorios()
        {
            try
            {
                var repositorios = db.Repositorio
                    .Where(r => r.EstaActivo == 1)
                    .Select(r => new { r.Secuencial, r.Descripcion })
                    .OrderBy(r => r.Descripcion)
                    .ToList();

                return Json(new { success = true, repositorios = repositorios }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult GetResponsablesProyectos()
        {
            try
            {
                var responsables = db.ResponsableProyectos
                    .Where(rp => rp.EstaActivo == 1)
                    .Select(rp => new { rp.Secuencial, rp.Nombre })
                    .OrderBy(rp => rp.Nombre)
                    .ToList();

                return Json(new { success = true, responsables = responsables }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult GetColaboradores()
        {
            try
            {
                var colaboradores = db.Colaborador
                    .Where(c => c.persona.usuario.First().EstaActivo == 1)
                    .Select(c => new { c.Secuencial, NombreCompleto = c.persona.Nombre1 + " " + c.persona.Apellido1 })
                    .OrderBy(c => c.NombreCompleto)
                    .ToList();

                return Json(new { success = true, colaboradores = colaboradores }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult CrearProyecto([System.Web.Http.FromBody] dynamic proyecto)
        {
            try
            {
                if (proyecto == null)
                {
                    return Json(new { success = false, msg = "Datos del proyecto no proporcionados." });
                }

                string cooperativa = proyecto.cooperativa;
                string version = proyecto.version;
                string repositorioStr = proyecto.repositorio;
                string admCodStr = proyecto.admCod;
                string publicacionStr = proyecto.publicacion;
                string solucionStr = proyecto.solucion;
                string liderProyectoStr = proyecto.liderProyecto;
                string fechaSalida = proyecto.fechaSalida;

                var cliente = db.Cliente.FirstOrDefault(c => c.Descripcion == cooperativa);
                if (cliente == null)
                {
                    return Json(new { success = false, msg = "Cliente no encontrado." });
                }

                var versionDesarrollo = db.VersionDesarrollo.FirstOrDefault(v => v.Descripcion == version);
                var repositorio = db.Repositorio.FirstOrDefault(r => r.Descripcion == repositorioStr);
                var responsableCodigo = db.ResponsableProyectos.FirstOrDefault(rp => rp.Nombre == admCodStr);
                var responsablePublicacion = db.ResponsableProyectos.FirstOrDefault(rp => rp.Nombre == publicacionStr);
                var liderProyecto = db.Colaborador.FirstOrDefault(c => (c.persona.Nombre1 + " " + c.persona.Apellido1) == liderProyectoStr);

                var nuevoProyecto = new ClienteAuxiliar
                {
                    SecuencialCliente = cliente.Secuencial,
                    SecuencialVersionDesarrollo = versionDesarrollo?.Secuencial ?? 0,
                    SecuencialRepositorio = repositorio?.Secuencial ?? 0,
                    SecuencialResponsableCodigo = responsableCodigo?.Secuencial ?? 0,
                    SecuencialResponsablePublicacion = responsablePublicacion?.Secuencial ?? 0,
                    SecuencialLiderProyecto = liderProyecto?.Secuencial ?? 0,
                    TieneCodigoFuente = solucionStr == "Sí" ? 1 : 0,
                    FechaProduccion = DateTime.ParseExact(fechaSalida, "dd/MM/yyyy", null)
                };

                db.ClienteAuxiliar.Add(nuevoProyecto);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult ActualizarProyecto([System.Web.Http.FromBody] dynamic proyecto)
        {
            try
            {
                if (proyecto == null || proyecto.id == 0)
                {
                    return Json(new { success = false, msg = "Datos del proyecto no válidos." });
                }

                int id = proyecto.id;
                string cooperativa = proyecto.cooperativa;
                string version = proyecto.version;
                string repositorioStr = proyecto.repositorio;
                string admCodStr = proyecto.admCod;
                string publicacionStr = proyecto.publicacion;
                string solucionStr = proyecto.solucion;
                string liderProyectoStr = proyecto.liderProyecto;
                string fechaSalida = proyecto.fechaSalida;

                var proyectoExistente = db.ClienteAuxiliar.Find(id);
                if (proyectoExistente == null)
                {
                    return Json(new { success = false, msg = "Proyecto no encontrado." });
                }

                var cliente = db.Cliente.FirstOrDefault(c => c.Descripcion == cooperativa);
                if (cliente == null)
                {
                    return Json(new { success = false, msg = "Cliente no encontrado." });
                }

                var versionDesarrollo = db.VersionDesarrollo.FirstOrDefault(v => v.Descripcion == version);
                var repositorio = db.Repositorio.FirstOrDefault(r => r.Descripcion == repositorioStr);
                var responsableCodigo = db.ResponsableProyectos.FirstOrDefault(rp => rp.Nombre == admCodStr);
                var responsablePublicacion = db.ResponsableProyectos.FirstOrDefault(rp => rp.Nombre == publicacionStr);
                var liderProyecto = db.Colaborador.FirstOrDefault(c => (c.persona.Nombre1 + " " + c.persona.Apellido1) == liderProyectoStr);

                proyectoExistente.SecuencialCliente = cliente.Secuencial;
                proyectoExistente.SecuencialVersionDesarrollo = versionDesarrollo?.Secuencial ?? 0;
                proyectoExistente.SecuencialRepositorio = repositorio?.Secuencial ?? 0;
                proyectoExistente.SecuencialResponsableCodigo = responsableCodigo?.Secuencial ?? 0;
                proyectoExistente.SecuencialResponsablePublicacion = responsablePublicacion?.Secuencial ?? 0;
                proyectoExistente.SecuencialLiderProyecto = liderProyecto?.Secuencial ?? 0;
                proyectoExistente.TieneCodigoFuente = solucionStr == "Sí" ? 1 : 0;
                proyectoExistente.FechaProduccion = DateTime.ParseExact(fechaSalida, "dd/MM/yyyy", null);

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult DarProyectosExcel()
        {
            try
            {
                var datosRaw = (from ca in db.ClienteAuxiliar
                                join c in db.Cliente on ca.SecuencialCliente equals c.Secuencial
                                join vd in db.VersionDesarrollo on ca.SecuencialVersionDesarrollo equals vd.Secuencial into vd_join
                                from vd in vd_join.DefaultIfEmpty()
                                join r in db.Repositorio on ca.SecuencialRepositorio equals r.Secuencial into r_join
                                from r in r_join.DefaultIfEmpty()
                                join rc in db.ResponsableProyectos on ca.SecuencialResponsableCodigo equals rc.Secuencial into rc_join
                                from rc in rc_join.DefaultIfEmpty()
                                join rp in db.ResponsableProyectos on ca.SecuencialResponsablePublicacion equals rp.Secuencial into rp_join
                                from rp in rp_join.DefaultIfEmpty()
                                join lp in db.Colaborador on ca.SecuencialLiderProyecto equals lp.Secuencial into lp_join
                                from lp in lp_join.DefaultIfEmpty()
                                where c.EstaActivo == 1
                                select new
                                {
                                    ca,
                                    c,
                                    vd,
                                    r,
                                    rc,
                                    rp,
                                    lp
                                }).ToList(); // Se ejecuta en la base

                var proyectos = datosRaw.Select(x =>
                {
                    var personaCliente = x.c.persona_cliente.FirstOrDefault();
                    var usuarioCliente = personaCliente?.persona?.usuario.FirstOrDefault();

                    var gestor = x.c.gestorServicios.FirstOrDefault();
                    var personaGestor = gestor?.colaborador?.persona;
                    var usuarioGestor = personaGestor?.usuario.FirstOrDefault();

                    return new
                    {
                        cooperativa = x.c.Descripcion,
                        codificacion = x.c.Codigo,
                        contactoCliente = usuarioCliente?.Email ?? "",
                        correoContactoCliente = usuarioCliente?.Email ?? "",
                        version = x.vd?.Descripcion ?? "",
                        repositorio = x.r?.Descripcion ?? "",
                        admCod = x.rc?.Nombre ?? "",
                        publicacion = x.rp?.Nombre ?? "",
                        solucion = x.ca.TieneCodigoFuente == 1 ? "Sí" : "No",
                        liderProyecto = x.lp?.persona != null
                            ? $"{x.lp.persona.Nombre1} {x.lp.persona.Apellido1}"
                            : "",
                        gestorServicio = personaGestor != null
                            ? $"{personaGestor.Nombre1} {personaGestor.Apellido1}"
                            : "",
                        correoGestorServicio = usuarioGestor?.Email ?? "",
                        fechaSalida = x.ca.FechaProduccion.ToString("dd/MM/yyyy")
                    };
                }).ToList();

                return Json(new
                {
                    success = true,
                    proyectos = proyectos
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


        [HttpPost]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult DarModulosClientes(int start, int lenght, string filtro = "", int order = 0, int asc = 1)
        {
            LoggerManager.LogInfo($"DarModulosClientes called with start={start}, lenght={lenght}, filtro={filtro}, order={order}, asc={asc}");
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
                if (filtroCliente != "")
                {
                    datos = (from d in datos
                             where d.cliente.ToString().ToUpper().Contains(filtroCliente.ToUpper())
                             select d).ToList();
                }

                if (filtroModulo != "")
                {
                    datos = (from d in datos
                             where d.modulo.ToString().ToUpper().Contains(filtroModulo.ToUpper())
                             select d).ToList();
                }
                if (filtroSubModulo != "")
                {
                    datos = (from d in datos
                             where d.subMod.ToString().ToUpper().Contains(filtroSubModulo.ToUpper())
                             select d).ToList();
                }
                if (filtroEstado != "")
                {
                    datos = (from d in datos
                             where d.estado.ToString().ToUpper().Contains(filtroEstado.ToUpper())
                             select d).ToList();
                }

                //Se ordena
                if (order > 0)
                {
                    switch (order)
                    {
                        case 1:
                            if (asc == 1)
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
                            if (asc == 1)
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
                            if (asc == 1)
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
                            if (asc == 1)
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

                LoggerManager.LogInfo($"DarModulosClientes returning {datos.Count} records out of {cantidad}");

                return Json(new
                {
                    modulosClientes = datos,
                    cantidadModulosClientes = cantidad,
                    success = true
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error in DarModulosClientes: {e.Message}");
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
            LoggerManager.LogInfo($"DarDatosModuloCliente called with secuencialModuloCliente={secuencialModuloCliente}");
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

                LoggerManager.LogInfo($"DarDatosModuloCliente returning data for secuencialModuloCliente={secuencialModuloCliente}");

                return Json(new
                {
                    success = true,
                    datos = moduloCliente,
                    funcionalidades = func,
                    submodulos = submodulos
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error in DarDatosModuloCliente: {e.Message}");
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
            LoggerManager.LogInfo("DarEstadosModuloCliente called");
            try
            {
                var datos = (from es in db.EstadoEntregable
                             select new
                             {
                                 nombre = es.Descripcion
                             }).ToList();

                LoggerManager.LogInfo($"DarEstadosModuloCliente returning {datos.Count} records");
                return Json(new
                {
                    estados = datos,
                    success = true
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error in DarEstadosModuloCliente: {e.Message}");
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
            LoggerManager.LogInfo($"DarFuncionalidadesExtra called with modulo={modulo} and funcionalidades count={(funcionalidades != null ? funcionalidades.Length : 0)}");
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
                if (funcionalidades != null)
                {
                    if (funcionalidades.Length == funcionalidadesCompletas.Count)
                    {
                        funcionalidadesCompletas.RemoveAll(x => true);
                    }
                    else
                    {
                        for (int i = 0; i < funcionalidadesCompletas.Count; i++)
                        {
                            for (int j = 0; j < funcionalidades.Length; j++)
                            {
                                if (i < funcionalidadesCompletas.Count && funcionalidadesCompletas.Count > 0 && funcionalidadesCompletas[i].id == int.Parse(funcionalidades[j]))
                                {
                                    funcionalidadesCompletas.Remove(funcionalidadesCompletas[i]);
                                    j = 0;
                                }
                            }
                        }
                    }
                }

                LoggerManager.LogInfo($"DarFuncionalidadesExtra returning {funcionalidadesCompletas.Count} funcionalidades");

                return Json(new
                {
                    success = true,
                    funcionalidadesExtra = funcionalidadesCompletas
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error in DarFuncionalidadesExtra: {e.Message}");
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
            LoggerManager.LogInfo($"GuardarDatos called with secuencialModuloCliente={secuencialModuloCliente}, estado={estado}, funcionalidades count={(funcionalidades != null ? funcionalidades.Split(',').Length : 0)}, subModulo={subModulo}");
            try
            {
                var secuencialEstado = (from es in db.EstadoEntregable
                                        where es.Descripcion == estado
                                        select es.Secuencial).ToList()[0];

                var secuencialCliente = (from pmc in db.ProyectoModuloCliente
                                         where pmc.Secuencial == secuencialModuloCliente
                                         select pmc.SecuencialCliente).ToList()[0];

                string emailUser = User.Identity.Name;
                // Log inicio de operación
                LoggerManager.LogInfo($"Usuario {emailUser} iniciando actualización de datos para módulo cliente {secuencialModuloCliente}");

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

                // Log cambio de estado
                ProyectoModuloCliente proyectoModuloCliente = db.ProyectoModuloCliente.Find(secuencialModuloCliente);
                if (proyectoModuloCliente != null)
                {
                    var estadoAnterior = proyectoModuloCliente.SecuencialEstadoEntregable;
                    if (estadoAnterior != secuencialEstado)
                    {
                        LoggerManager.LogSensitiveOperation(
                            "Cambio Estado Módulo",
                            $"Módulo: {secuencialModuloCliente}, Estado anterior: {estadoAnterior}, Nuevo estado: {secuencialEstado}",
                            emailUser
                        );
                    }
                    proyectoModuloCliente.SecuencialEstadoEntregable = secuencialEstado;
                    proyectoModuloCliente.SecuencialSubModulo = subModulo;
                    db.SaveChanges();
                }

                // Log eliminación de funcionalidades existentes
                var funcionalidadesEliminar = db.FuncionalidadCliente.Where(x => x.SecuencialCliente == secuencialCliente).ToList();
                LoggerManager.LogInfo($"Eliminando {funcionalidadesEliminar.Count} funcionalidades existentes del cliente");

                for (int i = 0; i < funcionalidadesEliminar.Count; i++)
                {
                    MethodInfo metodoRemove = typePropertyTabla.GetMethod("Remove");
                    newObj = metodoRemove.Invoke(dbSetTable, new object[1] { funcionalidadesEliminar[i] });
                }
                db.SaveChanges();


                // Log agregado de nuevas funcionalidades
                string[] funcionalidadesArray = funcionalidades.Split(',');
                LoggerManager.LogInfo($"Agregando {funcionalidadesArray.Length} nuevas funcionalidades al cliente");

                for (int i = 0; i < funcionalidadesArray.Length; i++)
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

                LoggerManager.LogInfo($"Actualización de datos completada exitosamente para módulo cliente {secuencialModuloCliente}");

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error al guardar datos del módulo cliente {secuencialModuloCliente}: {e.Message}");
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
            LoggerManager.LogInfo($"CargarGarantiaTickets called with start={start}, lenght={lenght}, filtro={filtro}, order={order}, asc={asc}, todos={todos}");
            try
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

                if (todos == false)
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
                if (filtroNoTicket != "")
                {
                    tickets = (from t in tickets
                               where t.numero.ToString().PadLeft(6, '0').Contains(filtroNoTicket)
                               select t).ToList();
                }
                if (filtroCliente != "")
                {
                    tickets = (from t in tickets
                               where t.cliente.ToString().ToLower().Contains(filtroCliente.ToLower())
                               select t).ToList();
                }
                if (filtroAsunto != "")
                {
                    tickets = (from t in tickets
                               where t.asunto.ToString().ToLower().Contains(filtroAsunto.ToLower())
                               select t).ToList();
                }
                if (filtroAsignado != "")
                {

                    tickets = (from t in tickets
                               where t.asignado.ToString().ToUpper().Contains(filtroAsignado.ToUpper())
                               select t).ToList();
                }
                if (filtroFecha != "")
                {
                    tickets = (from t in tickets
                               where t.fecha.ToString("dd/MM/yyyy").Contains(filtroFecha)
                               select t).ToList();
                }
                if (filtroFechaVencimiento != "")
                {
                    tickets = (from t in tickets
                               where t.fechaVencimiento.ToString("dd/MM/yyyy").Contains(filtroFechaVencimiento)
                               select t).ToList();
                }
                if (filtroDiasRestantes != "")
                {
                    tickets = (from t in tickets
                               where t.diasRestantes.ToString().ToUpper().Equals(filtroDiasRestantes.ToUpper())
                               select t).ToList();
                }
                //Se Ordena
                if (order > 0)
                {
                    switch (order)
                    {
                        case 1:

                            if (asc == 1)
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

                            if (asc == 1)
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

                            if (asc == 1)
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

                            if (asc == 1)
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

                            if (asc == 1)
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

                            if (asc == 1)
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

                            if (asc == 1)
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

                LoggerManager.LogInfo($"CargarGarantiaTickets returning {tickets.Count} tickets out of {totalTickets}");

                var resp = new
                {
                    success = true,
                    tickets = tickets,
                    totalTickets = totalTickets
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error in CargarGarantiaTickets: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, CLIENTE, GESTOR")]
        public ActionResult DarOfertasTickets(int start, int length, string filtro = "")
        {
            LoggerManager.LogInfo($"DarOfertasTickets called with start={start}, length={length}, filtro={filtro}");
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
                                   FechaRegistro = o.FechaRegistro.HasValue ? o.FechaRegistro.Value.Year != 1 ? o.FechaRegistro : (DateTime?)null : (DateTime?)null,
                                   FechaProduccion = o.FechaProduccion.HasValue ? o.FechaProduccion.Value.Year != 1 ? o.FechaProduccion.Value : (DateTime?)null : (DateTime?)null,
                                   FechaDisponibilidad = o.FechaDisponibilidad.HasValue ? o.FechaDisponibilidad.Value.Year != 1 ? o.FechaDisponibilidad : (DateTime?)null : (DateTime?)null,
                                   FechaAprobacion = o.FechaAprobacion.HasValue ? o.FechaAprobacion : (DateTime?)null,
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

                if (filtro != "")
                {
                    ofertas = ofertas.Where(s =>
                                            s.cliente.nombre.ToString().ToUpper().Contains(filtro.ToUpper()) ||
                                            s.colaborador.nombre.ToString().ToUpper().Contains(filtro.ToUpper()) ||
                                            s.Detalle.ToString().ToUpper().Contains(filtro.ToUpper()) ||
                                            s.FechaDisponibilidad.ToString().ToUpper().Contains(filtro.ToUpper()) ||
                                            s.FechaAprobacion.ToString().ToUpper().Contains(filtro.ToUpper()) ||
                                            s.FechaProduccion.ToString().ToUpper().Contains(filtro.ToUpper()) ||
                                            s.FechaRegistro.ToString().ToUpper().Contains(filtro.ToUpper()) ||
                                            s.HorasEstimacion.ToString().ToUpper().Contains(filtro.ToUpper())
                                        ).ToList();
                }

                var cantidad = ofertas?.Count;
                ofertas = ofertas.Skip(start).Take(length).ToList();

                LoggerManager.LogInfo($"DarOfertasTickets returning {ofertas.Count} records out of {cantidad}");

                var resp = new
                {
                    success = true,
                    ofertas = ofertas,
                    cantidad = cantidad
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error in DarOfertasTickets: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, GESTOR")]
        public ActionResult AgregarOfertaTickets(string fechaRegistro, string fechaProduccion, string fechaDisponibilidad, string fechaAprobacion, string detalle, int? horasEstimacion, int? cliente, int? colaborador, HttpPostedFileBase adjunto = null)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"Usuario {emailUser} iniciando creación de nueva oferta de ticket");

                var adjuntoUrl = "";
                if (adjunto != null)
                {
                    string path = Path.Combine(Server.MapPath("~/Web/resources/ofertas"), adjunto.FileName);
                    adjunto.SaveAs(path);
                    adjuntoUrl = "/resources/ofertas" + "/" + adjunto.FileName;
                    LoggerManager.LogInfo($"Archivo adjunto guardado: {adjuntoUrl}");
                }

                DateTime dateRegistro = new DateTime(0001 / 01 / 01);
                DateTime dateProduccion = new DateTime(0001 / 01 / 01);
                DateTime dateDisponibilidad = new DateTime(0001 / 01 / 01);
                DateTime? dateAprobacion = null;
                string format = "ddd MMM dd yyyy HH:mm:ss 'GMT'K";

                if (fechaRegistro != "null")
                {
                    fechaRegistro = fechaRegistro.Split(new[] { " (" }, StringSplitOptions.None)[0];
                    dateRegistro = DateTime.ParseExact(fechaRegistro, format, CultureInfo.InvariantCulture);
                }
                if (fechaProduccion != "null")
                {
                    fechaProduccion = fechaProduccion.Split(new[] { " (" }, StringSplitOptions.None)[0];
                    dateProduccion = DateTime.ParseExact(fechaProduccion, format, CultureInfo.InvariantCulture);
                }
                if (fechaDisponibilidad != "null")
                {
                    fechaDisponibilidad = fechaDisponibilidad.Split(new[] { " (" }, StringSplitOptions.None)[0];
                    dateDisponibilidad = DateTime.ParseExact(fechaDisponibilidad, format, CultureInfo.InvariantCulture);
                }
                if (fechaAprobacion != "null")
                {
                    fechaAprobacion = fechaAprobacion.Split(new[] { " (" }, StringSplitOptions.None)[0];
                    dateAprobacion = DateTime.ParseExact(fechaAprobacion, format, CultureInfo.InvariantCulture);
                }

                Ofertas oferta = new Ofertas();
                oferta.FechaRegistro = dateRegistro.Date;
                oferta.FechaProduccion = dateProduccion.Date;
                oferta.FechaDisponibilidad = dateDisponibilidad.Date;
                oferta.FechaAprobacion = dateAprobacion.HasValue ? dateAprobacion.Value.Date : (DateTime?)null;
                oferta.Detalle = detalle;
                oferta.HorasEstimacion = horasEstimacion ?? 0;
                oferta.cliente = cliente != null ? db.Cliente.Find(cliente) : db.Cliente.Find(78);
                oferta.colaborador = colaborador != null ? db.Colaborador.Find(colaborador) : db.Colaborador.Find(2122);
                oferta.Adjunto = adjuntoUrl;

                // Log datos de la oferta
                LoggerManager.LogSensitiveOperation(
                    "Creación Oferta",
                    $"Cliente: {oferta.cliente.Descripcion}, Colaborador: {oferta.colaborador.persona.Nombre1} {oferta.colaborador.persona.Apellido1}, " +
                    $"Horas: {oferta.HorasEstimacion}, Detalle: {detalle.Substring(0, Math.Min(100, detalle.Length))}...",
                    emailUser
                );

                db.Ofertas.Add(oferta);
                db.SaveChanges();

                LoggerManager.LogInfo($"Oferta de ticket creada exitosamente");

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error al crear oferta de ticket: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, GESTOR")]
        public ActionResult EditarOfertaTickets(int ID, string FechaRegistro, string FechaProduccion, string FechaDisponibilidad, string FechaAprobacion, string Detalle, int HorasEstimacion, int cliente, int colaborador, HttpPostedFileBase adjunto = null)
        {
            try
            {
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"Usuario {emailUser} iniciando edición de oferta ID: {ID}");

                Ofertas oferta = db.Ofertas.FirstOrDefault(s => s.Secuencial == ID);
                if (oferta != null)
                {
                    // Log estado anterior
                    LoggerManager.LogSensitiveOperation(
                        "Estado Anterior Oferta",
                        $"ID: {ID}, Cliente: {oferta.cliente?.Descripcion}, Colaborador: {oferta.colaborador?.persona.Nombre1} {oferta.colaborador?.persona.Apellido1}, " +
                        $"Horas: {oferta.HorasEstimacion}, Detalle: {oferta.Detalle?.Substring(0, Math.Min(100, oferta.Detalle.Length))}...",
                        emailUser
                    );

                    DateTime dateRegistro = new DateTime(0001 / 01 / 01);
                    DateTime dateProduccion = new DateTime(0001 / 01 / 01);
                    DateTime dateDisponibilidad = new DateTime(0001 / 01 / 01);
                    DateTime? dateAprobacion = null;
                    string format = "ddd MMM dd yyyy HH:mm:ss 'GMT'K";

                    if (FechaRegistro != "null" && FechaRegistro != "Invalid Date")
                    {
                        FechaRegistro = FechaRegistro.Split(new[] { " (" }, StringSplitOptions.None)[0];
                        dateRegistro = DateTime.ParseExact(FechaRegistro, format, CultureInfo.InvariantCulture);
                        oferta.FechaRegistro = dateRegistro.Date;
                    }
                    if (FechaProduccion != "null" && FechaProduccion != "Invalid Date")
                    {
                        FechaProduccion = FechaProduccion.Split(new[] { " (" }, StringSplitOptions.None)[0];
                        dateProduccion = DateTime.ParseExact(FechaProduccion, format, CultureInfo.InvariantCulture);
                        oferta.FechaProduccion = dateProduccion.Date;
                    }
                    if (FechaDisponibilidad != "null" && FechaDisponibilidad != "Invalid Date")
                    {
                        FechaDisponibilidad = FechaDisponibilidad.Split(new[] { " (" }, StringSplitOptions.None)[0];
                        dateDisponibilidad = DateTime.ParseExact(FechaDisponibilidad, format, CultureInfo.InvariantCulture);
                        oferta.FechaDisponibilidad = dateDisponibilidad.Date;
                    }
                    if (FechaAprobacion != "null" && FechaAprobacion != "Invalid Date")
                    {
                        FechaAprobacion = FechaAprobacion.Split(new[] { " (" }, StringSplitOptions.None)[0];
                        dateAprobacion = DateTime.ParseExact(FechaAprobacion, format, CultureInfo.InvariantCulture);
                        oferta.FechaAprobacion = dateAprobacion.HasValue ? dateAprobacion.Value.Date : (DateTime?)null;
                    }

                    oferta.Detalle = Detalle;
                    oferta.HorasEstimacion = HorasEstimacion;
                    oferta.cliente = db.Cliente.Find(cliente);
                    oferta.colaborador = db.Colaborador.Find(colaborador);

                    var adjuntoUrl = "";
                    if (adjunto != null)
                    {
                        string path = Path.Combine(Server.MapPath("~/Web/resources/ofertas"), adjunto.FileName);
                        adjunto.SaveAs(path);

                        adjuntoUrl = "/resources/ofertas" + "/" + adjunto.FileName;
                        oferta.Adjunto = adjuntoUrl;
                        LoggerManager.LogInfo($"Nuevo archivo adjunto guardado: {adjuntoUrl}");
                    }

                    // Log nuevos datos
                    LoggerManager.LogSensitiveOperation(
                        "Nuevos Datos Oferta",
                        $"ID: {ID}, Cliente: {oferta.cliente.Descripcion}, Colaborador: {oferta.colaborador.persona.Nombre1} {oferta.colaborador.persona.Apellido1}, " +
                        $"Horas: {HorasEstimacion}, Detalle: {Detalle.Substring(0, Math.Min(100, Detalle.Length))}...",
                        emailUser
                    );
                }

                db.SaveChanges();
                LoggerManager.LogInfo($"Oferta ID: {ID} actualizada exitosamente");

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error al editar oferta ID {ID}: {e.Message}");
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
                string emailUser = User.Identity.Name;
                LoggerManager.LogInfo($"Usuario {emailUser} iniciando eliminación de oferta ID: {ID}");

                Ofertas oferta = db.Ofertas.Find(ID);
                if (oferta != null)
                {
                    // Log datos de la oferta a eliminar
                    LoggerManager.LogSensitiveOperation(
                        "Eliminación Oferta",
                        $"ID: {ID}, Cliente: {oferta.cliente?.Descripcion}, Colaborador: {oferta.colaborador?.persona.Nombre1} {oferta.colaborador?.persona.Apellido1}, " +
                        $"Horas: {oferta.HorasEstimacion}, Detalle: {oferta.Detalle?.Substring(0, Math.Min(100, oferta.Detalle.Length))}...",
                        emailUser
                    );

                    db.Ofertas.Remove(oferta);
                    db.SaveChanges();

                    LoggerManager.LogInfo($"Oferta ID: {ID} eliminada exitosamente");
                }

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                LoggerManager.LogError(e, $"Error al eliminar oferta ID {ID}: {e.Message}");
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }
    }
}