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
using System.Globalization;
using SifizPlanning.Models.ViewModel;
using System.Data.Entity;

namespace SifizPlanning.Controllers
{
    public class RrhhController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();

        //Compara los componentes de la fecha de dos datetime
        private bool FechasIguales(DateTime fecha1, DateTime fecha2)
        {
            return (fecha1.Day == fecha2.Day && fecha1.Month == fecha2.Month && fecha1.Year == fecha2.Year);
        }

        //
        // GET: /Rrhh/
        [Authorize(Roles = "ADMIN, RRHH")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, RRHH")]
        public ActionResult GuardarFeriado(DateTime fecha)
        {
            try
            {
                // Verificar si ya existe un feriado con la misma fecha
                bool existe = db.Feriados.Any(f => f.Fecha == fecha);

                // Si ya existe un feriado con la misma fecha, devolver un error
                if (existe)
                {
                    var resp = new
                    {
                        success = false,
                        msg = "Ya existe un feriado con la misma fecha."
                    };
                    return Json(resp);
                }
                else
                {
                    // Si no existe un feriado con la misma fecha, agregar el nuevo feriado
                    var feriado = new Feriados
                    {
                        Fecha = fecha
                    };
                    db.Feriados.Add(feriado);
                    db.SaveChanges();

                    var resp = new
                    {
                        success = true,
                        msg = "Nuevo feriado agregado."
                    };
                    return Json(resp);
                }
            }
            catch (Exception e)
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
        public ActionResult EliminarFeriado(int id)
        {
            try
            {
                var feriado = db.Feriados.Find(id);
                db.Feriados.Remove(feriado);
                db.SaveChanges();

                var resp = new
                {
                    success = true,
                    msg = "Feriado eliminado.",
                };
                return Json(resp);
            }
            catch (TargetInvocationException e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.InnerException?.Message ?? e.Message
                };
                return Json(resp);
            }
        }




        [HttpPost]
        [Authorize(Roles = "ADMIN, RRHH, USER")]
        public ActionResult DarFeriados()
        {
            try
            {
                var feriados = db.Feriados
                       .Select(f => new { id = f.Secuencial, fecha = f.Fecha })
                       .ToList();

                var resp = new
                {
                    success = true,
                    feriados = feriados
                };
                return Json(resp);
            }
            catch (Exception e)
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
        [Authorize(Roles = "RRHH, ADMIN")]
        public ActionResult VacacionesUsuarios()
        {
            try
            {
                var solicitudes = db.SolicitudVacaciones.AsNoTracking().ToList();

                List<SolicitudVacacionesDTO> solicitudesVacacionesDTO = new List<SolicitudVacacionesDTO>();

                foreach (var solicitud in solicitudes)
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
                    solVacaciones.ApellidosNombres = solicitud.ApellidosNombres;
                    solVacaciones.DiasCorresponden = solicitud.DiasCorresponden;
                    solVacaciones.DiasDisfrutar = solicitud.DiasDisfrutar;
                    solVacaciones.DiasPendientes = solicitud.DiasPendientes;
                    solVacaciones.FechaIngresoInstitucion = solicitud.FechaPresentarseTrabajar;//No usar -> solicitud.FechaIngresoInstitucion;
                    solVacaciones.FechaIngresoSolicitud = solicitud.FechaIngresoSolicitud;
                    solVacaciones.Observaciones = solicitud.Observaciones;
                    solVacaciones.DelAnio = solicitud.DelAnio;
                    solVacaciones.Jefe = solicitud.Jefe;
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
        [Authorize(Roles = "RRHH, ADMIN")]
        public ActionResult PermisosUsuarios()
        {
            try
            {
                var solicitudes = db.SolicitudPermisos
                    .AsNoTracking()
                    .ToList();

                List<SolicitudPermisosDTO> solicitudesPermisosDTO = new List<SolicitudPermisosDTO>();

                foreach (var solicitud in solicitudes)
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
        [Authorize(Roles = "ADMIN, RRHH")]
        public ActionResult GuardarVacaciones(int idColaborador, string desde, string hasta)
        {
            try
            {
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

                Colaborador colaborador = db.Colaborador.Find(idColaborador);
                if (colaborador == null)
                {
                    throw new Exception("No se encontró el colaborador");
                }
                if (colaborador.persona.usuario.First().EstaActivo == 0)
                {
                    throw new Exception("Error, el colaborador no está activo en el sistema");
                }

                string[] fechas = desde.Split(new Char[] { '/' });
                int dia = Int32.Parse(fechas[0]);
                int mes = Int32.Parse(fechas[1]);
                int anno = Int32.Parse(fechas[2]);
                DateTime fechaDesde = new System.DateTime(anno, mes, dia);

                fechas = hasta.Split(new Char[] { '/' });
                dia = Int32.Parse(fechas[0]);
                mes = Int32.Parse(fechas[1]);
                anno = Int32.Parse(fechas[2]);
                DateTime fechaHasta = new System.DateTime(anno, mes, dia);

                if (fechaDesde > fechaHasta)
                {
                    throw new Exception("Error, la fecha de fin no debe ser menor a la fecha de inicio.");
                }

                while (fechaDesde <= fechaHasta)
                {
                    Vacaciones diaVacaciones = new Vacaciones
                    {
                        colaborador = colaborador,
                        Fecha = fechaDesde
                    };
                    db.Vacaciones.Add(diaVacaciones);
                    fechaDesde = fechaDesde.AddDays(1);
                }

                db.SaveChanges();

                //Enviar email al grupo de RRHH y a l usuario de las vacaciones
                string[] emails = new string[2] { colaborador.persona.usuario.First().Email, user.Email };
                string email = "<div class=\"textoCuerpo\">NOTIFICACIÓN VAPER: <br/>" +
                                @"Le informamos que las vacaciones solicitadas por usted han sido aprobadas,
                                los datos son los siguientes:<br/>
                                Fecha de Inicio: " + desde + @"<br/>
                                Fecha de Fin: " + hasta + @"<br/>
                                Aprobado por: " + user.persona.Nombre1 + " " + user.persona.Apellido1 + @"</div><br/><br/>
                                Esperamos disfrute sus vacaciones junto a su familia y amigos...";

                Utiles.EnviarEmailSistema(emails, email, "Vacaciones aprobadas");

                var resp = new { success = true };
                return Json(resp);
            }
            catch (Exception e)
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
        public ActionResult GuardarPermiso(int idColaborador, int tipoPermiso, string desde, string hasta, string motivo)
        {
            try
            {
                string[] datosFechaDesde = desde.Split(new Char[] { ' ' });
                string[] fechas = datosFechaDesde[0].Split(new Char[] { '/' });
                int dia = Int32.Parse(fechas[0]);
                int mes = Int32.Parse(fechas[1]);
                int anno = Int32.Parse(fechas[2]);
                DateTime fechaDesde = new System.DateTime(anno, mes, dia);
                string[] horas = datosFechaDesde[1].Split(new Char[] { ':' });
                int hora = Int32.Parse(horas[0]);
                int minutos = Int32.Parse(horas[1]);
                fechaDesde = fechaDesde.AddMinutes(hora * 60 + minutos);

                string[] datosFechaHasta = hasta.Split(new Char[] { ' ' });
                fechas = datosFechaHasta[0].Split(new Char[] { '/' });
                dia = Int32.Parse(fechas[0]);
                mes = Int32.Parse(fechas[1]);
                anno = Int32.Parse(fechas[2]);
                DateTime fechaHasta = new System.DateTime(anno, mes, dia);
                horas = datosFechaHasta[1].Split(new Char[] { ':' });
                hora = Int32.Parse(horas[0]);
                minutos = Int32.Parse(horas[1]);
                fechaHasta = fechaHasta.AddMinutes(hora * 60 + minutos);

                if (fechaHasta <= fechaDesde)
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
                                   p.SecuencialColaborador == idColaborador
                            select p).Count();

                if (cant > 1)
                {
                    throw new Exception("Error, las fechas de este permiso se solapan con las de otro");
                }

                List<Tarea> tareas = (from t in db.Tarea
                                      where t.colaborador.Secuencial == idColaborador
                                      && t.FechaInicio >= fechaDesde && t.FechaInicio <= fechaHasta
                                      select t).ToList<Tarea>();

                foreach (Tarea t in tareas)
                {
                    DateTime nuevaFechaInicio = fechaHasta.AddDays(1);
                    while (nuevaFechaInicio.DayOfWeek == DayOfWeek.Saturday || nuevaFechaInicio.DayOfWeek == DayOfWeek.Sunday || db.Feriados.Any(f => f.Fecha == nuevaFechaInicio))
                    {
                        nuevaFechaInicio = nuevaFechaInicio.AddDays(1);
                    }
                    db.Tarea.Where(tarea => t.Secuencial == tarea.Secuencial).First().FechaInicio = nuevaFechaInicio;
                }

                Permiso permiso = new Permiso
                {
                    SecuencialTipoPermiso = tipoPermiso,
                    SecuencialEstadoPermiso = 1,//Solicitado
                    SecuencialColaborador = idColaborador,
                    FechaInicio = fechaDesde,
                    FechaFin = fechaHasta,
                    Motivo = motivo
                };


                db.Permiso.Add(permiso);
                db.SaveChanges();


                var resp = new
                {
                    success = true,
                    idPermiso = permiso.Secuencial,
                };
                return Json(resp);
            }
            catch (Exception e)
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
        public ActionResult DarVacacionesTrabajador(int idColaborador, int difYear = 0)
        {
            try
            {
                Colaborador colaborador = db.Colaborador.Find(idColaborador);
                if (colaborador == null)
                {
                    throw new Exception("Error, no se encontró el colaborador");
                }

                int year = DateTime.Today.AddYears(difYear).Year;
                DateTime diaInicial = new DateTime(year, 1, 1);
                DateTime diaFinal = diaInicial.AddYears(1);

                var diasVacaciones = colaborador.vacaciones.Where(x => x.Fecha >= diaInicial && x.Fecha < diaFinal).ToList();

                List<object> mesesVacaciones = new List<object>();
                for (int i = 0; i < 12; i++)
                {
                    DateTime fechaMes = diaInicial.AddMonths(i);
                    DateTime fechaDia = fechaMes;
                    int mes = fechaMes.Month;
                    string textMes = Utiles.PrimeraMayuscula(fechaMes.ToString("MMMM"));

                    List<object> diasVacacionesMes = new List<object>();
                    while (mes == fechaDia.Month)
                    {
                        if (diasVacaciones.Where(x => x.Fecha.Date == fechaDia.Date).Count() > 0)
                        {
                            diasVacacionesMes.Add(new { clase = "diaV", dia = fechaDia.ToString("dd/MM/yyyy"), id = diasVacaciones.Where(x => x.Fecha.Date == fechaDia.Date).FirstOrDefault().Secuencial });
                        }
                        else
                        {
                            diasVacacionesMes.Add(new { clase = "diaN", dia = fechaDia.ToString("dd/MM/yyyy") });
                        }
                        fechaDia = fechaDia.AddDays(1);
                    }

                    mesesVacaciones.Add(
                                        new { mes = textMes, dias = diasVacacionesMes }
                                       );
                }

                var resp = new
                {
                    success = true,
                    year = diaInicial.Year,
                    datos = mesesVacaciones,
                    nombreColaborador = colaborador.persona.Nombre1 + " " + colaborador.persona.Apellido1
                };
                return Json(resp);
            }
            catch (Exception e)
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
        public ActionResult EliminarDiaVacacion(int id)
        {
            try
            {
                Vacaciones vacacion = db.Vacaciones.Find(id);
                db.Vacaciones.Remove(vacacion);
                db.SaveChanges();

                var resp = new
                {
                    success = true
                };
                return Json(resp);
            }
            catch (Exception e)
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
        public ActionResult ColaboradoresTiempoCompesatorio(int start = 0, int limit = 0)
        {
            try
            {
                var trabajadores = (from c in db.Colaborador
                                    join
                                     p in db.Persona on c.persona equals p
                                    join
                                     u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                    where u.EstaActivo == 1
                                    orderby u.Email
                                    select new
                                    {
                                        id = c.Secuencial,
                                        foto = c.fotoColaborador.FirstOrDefault().Url,
                                        nombre = p.Nombre1 + " " + p.Apellido1,
                                        email = u.Email,
                                        departamento = c.departamento.Descripcion,
                                        cargo = c.cargo.Descripcion,
                                        sede = c.sede.Descripcion
                                    }
                                   );

                List<object> trabajadoresTiempo = new List<object>();
                foreach (var trab in trabajadores)
                {
                    int tiempo = 0;

                    int cantTareas = (
                                        from tc in db.TareaCompensatoria
                                        where tc.EstaActiva == 1 && tc.tarea.SecuencialEstadoTarea == 3 &&
                                              tc.tarea.SecuencialColaborador == trab.id &&
                                              tc.TiempoMinutos > tc.TiempoConsumido
                                        select (tc.Secuencial)
                                    ).Count();
                    if (cantTareas > 0)
                        tiempo = (
                            from tc in db.TareaCompensatoria
                            where tc.EstaActiva == 1 && tc.tarea.SecuencialEstadoTarea == 3 &&
                                  tc.tarea.SecuencialColaborador == trab.id &&
                                  tc.TiempoMinutos > tc.TiempoConsumido
                            select
                             (tc.TiempoMinutos - tc.TiempoConsumido)
                        ).Sum();

                    int horas = tiempo / 60;
                    int minutos = tiempo % 60;

                    string strMinutos = minutos.ToString();
                    if (minutos < 10)
                        strMinutos = "0" + strMinutos;

                    trabajadoresTiempo.Add(new
                    {
                        id = trab.id,
                        foto = trab.foto,
                        nombre = trab.nombre,
                        email = trab.email,
                        departamento = trab.departamento,
                        cargo = trab.cargo,
                        sede = trab.sede,
                        tiempo = horas + ":" + strMinutos
                    });
                }

                var resp = new
                {
                    success = true,
                    trabajadoresTiempo = trabajadoresTiempo
                };
                return Json(resp);
            }
            catch (Exception e)
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
        public ActionResult ColaboradorTareasCompensatorias(int idColaborador, bool todas = false)
        {
            try
            {
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

                if (!todas)
                {
                    tareasCompensatorias = tareasCompensatorias.Where(x => x.tiempo > x.consumido).ToList();
                }

                List<object> listaTareasCompensatorias = new List<object>();

                int totalTiempo = 0;
                int totalConsumido = 0;
                foreach (var tarComp in tareasCompensatorias)
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
            catch (Exception e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(resp);
            }
        }

        //Funcionalidades para control de tiempo
        [HttpPost]
        [Authorize(Roles = "ADMIN, RRHH")]
        public ActionResult DevolverMarcasElectronicas(string sede, string fechaDesde, string fechaHasta)
        {
            try
            {
                var listaColaboradores = new List<object>();
                if (fechaDesde != string.Empty && fechaHasta != string.Empty)
                {
                    //Buscando los colaboradores
                    var listaColaboradoresTmp = (from c in db.Colaborador
                                                 join f in db.FotoColaborador on c.Secuencial equals f.SecuencialColaborador
                                                 join s in db.Sede on c.sede equals s
                                                 join p in db.Persona on c.persona equals p
                                                 join u in db.Usuario on p equals u.persona
                                                 orderby p.Nombre1
                                                 where s.Descripcion == sede
                                                 select new
                                                 {
                                                     idColaborador = c.Secuencial,
                                                     nombreColaborador = p.Nombre1 + " " + p.Apellido1,
                                                     url = f.Url,
                                                     email = u.Email.ToUpper(),
                                                     idOficina = s.Secuencial,
                                                     nombreOficina = s.Descripcion,
                                                 }).ToList<object>();

                    //Buscando las marcas de los colaboradores
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime fechaHoraDesde = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", provider);
                    DateTime fechaHoraHasta = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", provider);
                    foreach (var colaborador in listaColaboradoresTmp)
                    {
                        int id = ((dynamic)colaborador).idColaborador;
                        DateTime fechaHoraDesdeTmp = fechaHoraDesde;
                        var marcasElectronicas = new List<object>();
                        //Buscando por cada dia las marcas del colaborador
                        //La primera marca es de entrada y la ultima es la salida 
                        Func<TimeSpan, string> TimetoString = (x => (x.Hours < 10 ? "0" + x.Hours.ToString() : x.Hours.ToString()) + ":" +
                                                                    (x.Minutes < 10 ? "0" + x.Minutes.ToString() : x.Minutes.ToString()) + ":" +
                                                                    (x.Seconds < 10 ? "0" + x.Seconds.ToString() : x.Seconds.ToString()));
                        while (fechaHoraDesdeTmp < fechaHoraHasta)
                        {
                            var marcas = (from me in db.MarcaElectronica
                                          where me.SecuencialColaborador == id &&
                                          me.Fecha.Day == fechaHoraDesdeTmp.Day &&
                                          me.Fecha.Month == fechaHoraDesdeTmp.Month &&
                                          me.Fecha.Year == fechaHoraDesdeTmp.Year
                                          group me by me.Fecha into g
                                          select new
                                          {
                                              fecha = g.Key,
                                              horaEntrada = g.Min(m => m.Hora),
                                              horaSalida = g.Max(m => m.Hora)
                                          }).ToList();
                            //Si no tiene marca en el día genero una marca vacia
                            object marca = null;
                            string claseHoraEntrada = string.Empty;
                            string claseHoraSalida = string.Empty;
                            string claseHorasTrab = string.Empty;
                            string claseTipoImp = "tipo-impunt-ninguna";
                            //No muestro ninguna marca si:
                            //La fecha es posterior al dia de hoy o
                            //es sabado o domingo y no tiene marcas en esos dias
                            if (fechaHoraDesdeTmp > DateTime.Now || ((fechaHoraDesdeTmp.DayOfWeek == DayOfWeek.Saturday || fechaHoraDesdeTmp.DayOfWeek == DayOfWeek.Sunday) && marcas.Count == 0))
                            {
                                claseHoraEntrada = "ocultar-marcas";
                                claseHoraSalida = "ocultar-marcas";
                                claseHorasTrab = "ocultar-marcas";
                            }
                            else
                            {
                                claseHoraEntrada = "finish";
                                claseHoraSalida = "new";
                            }
                            if (marcas.Count == 0)
                            {
                                claseHoraEntrada = "ocultar-marcas";
                                claseHoraSalida = "ocultar-marcas";
                                if (claseHorasTrab == string.Empty)
                                {
                                    claseHorasTrab = "horas-trab-sin-marcas";
                                }
                                marca = new { fecha = fechaHoraDesdeTmp.ToShortDateString(), horaEntrada = "", horaSalida = "", horasTrabajadas = "00:00:00", claseHoraEntrada = claseHoraEntrada, claseHoraSalida = claseHoraSalida, claseHorasTrab = claseHorasTrab, claseTipoImp = claseTipoImp };
                            }
                            else
                            {
                                if (claseHorasTrab == string.Empty)
                                {
                                    claseHorasTrab = "horas-trab-con-2marcas";
                                }
                                marca = marcas.ElementAt(0);
                                //Si la hora de entrada coincide con la hora salida entonces tiene una sola marca en
                                //el dia y se toma esa marca como de entrada
                                if (((dynamic)marca).horaEntrada == ((dynamic)marca).horaSalida)
                                {
                                    if (claseHorasTrab != "ocultar-marcas")
                                    {
                                        claseHorasTrab = "horas-trab-con-1marca";
                                        claseHoraSalida = "ocultar-marcas";
                                    }
                                    object marcaTmp = new { fecha = ((DateTime)((dynamic)marca).fecha).ToShortDateString(), horaEntrada = TimetoString(((dynamic)marca).horaEntrada), horaSalida = "", horasTrabajadas = "00:00:00", claseHoraEntrada = claseHoraEntrada, claseHoraSalida = claseHoraSalida, claseHorasTrab = claseHorasTrab, claseTipoImp = claseTipoImp };
                                    marca = marcaTmp;
                                }
                                //Convirtiendo los campos del objeto marca a string
                                if (((dynamic)marca).fecha is DateTime)
                                {
                                    //1 hora de almuerzo y 15 de pausa activa y refrigerio
                                    DateTime horaDescuento = DateTime.Parse(DateTime.Now.ToShortDateString() + " 01:15:00");
                                    //hora de entrada
                                    DateTime horaEntradaEmpresa = DateTime.Parse(DateTime.Now.ToShortDateString() + " 08:30:00");
                                    TimeSpan horasTrabajadas = ((dynamic)marca).horaSalida - ((dynamic)marca).horaEntrada - horaDescuento.TimeOfDay;
                                    if (((dynamic)marca).horaEntrada > horaEntradaEmpresa.TimeOfDay)
                                    {
                                        TimeSpan impuntualidad = ((dynamic)marca).horaEntrada - horaEntradaEmpresa.TimeOfDay;
                                        if (impuntualidad.Hours == 0 && impuntualidad.Minutes <= 12)
                                            claseTipoImp = "tipo-impunt1";
                                        else if (impuntualidad.Hours == 0 && impuntualidad.Minutes >= 13 && impuntualidad.Minutes <= 30)
                                            claseTipoImp = "tipo-impunt2";
                                        else
                                            claseTipoImp = "tipo-impunt3";
                                    }
                                    object marcaTmp = new { fecha = ((DateTime)((dynamic)marca).fecha).ToShortDateString(), horaEntrada = TimetoString(((dynamic)marca).horaEntrada), horaSalida = TimetoString(((dynamic)marca).horaSalida), horasTrabajadas = TimetoString(horasTrabajadas), claseHoraEntrada = claseHoraEntrada, claseHoraSalida = claseHoraSalida, claseHorasTrab = claseHorasTrab, claseTipoImp = claseTipoImp };
                                    marca = marcaTmp;
                                }
                            }
                            marcasElectronicas.Add(marca);
                            fechaHoraDesdeTmp = fechaHoraDesdeTmp.AddDays(1);
                        }
                        //Guardando los colaboradores y sus marcas
                        listaColaboradores.Add(new
                        {
                            idColaborador = id,
                            nombreColaborador = ((dynamic)colaborador).nombreColaborador,
                            url = ((dynamic)colaborador).url,
                            email = ((dynamic)colaborador).email,
                            idOficina = ((dynamic)colaborador).idOficina,
                            nombreOficina = ((dynamic)colaborador).nombreOficina,
                            marcas = marcasElectronicas
                        });
                    }
                }
                var resp = new
                {
                    success = true,
                    colaboradores = listaColaboradores
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(resp);
            }
        }

        //Funcion de disponibilidad de recursos
        [HttpPost]
        [Authorize(Roles = "ADMIN, RRHH")]
        public ActionResult DisponibilidadRecursosFechas(int semana = 0, int cantSemanas = 8, string fechaSemanaInicio = "")
        {
            try
            {
                DateTime fechaDesde = DateTime.Today;

                if (fechaSemanaInicio != string.Empty)
                {
                    fechaDesde = DateTime.Parse(fechaSemanaInicio);
                }

                fechaDesde = fechaDesde.AddDays(7 * semana);
                //Buscando el lunes
                int diaSemana = (int)fechaDesde.DayOfWeek;
                if (diaSemana == 0)
                {
                    fechaDesde = fechaDesde.AddDays(-6);
                }
                else
                {
                    fechaDesde = fechaDesde.AddDays((diaSemana - 1) * -1);
                }
                DateTime fechaHasta = fechaDesde.AddDays(cantSemanas * 7);

                var colaboradores = (
                                      from c in db.Colaborador
                                      join
                                          p in db.Persona on c.SecuencialPersona equals p.Secuencial
                                      join
                                          s in db.Sede on c.SecuencialSede equals s.Secuencial
                                      join
                                          d in db.Departamento on c.SecuencialDepartamento equals d.Secuencial
                                      join
                                          u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                      where u.EstaActivo == 1
                                      orderby p.Apellido1, p.Nombre1 descending
                                      select new
                                      {
                                          idColaborador = c.Secuencial,
                                          idPersona = p.Secuencial,
                                          nombre = p.Apellido1 + " " + p.Nombre1,
                                          departamento = d.Descripcion,
                                          idDepartamento = d.Secuencial,
                                          sede = s.Descripcion,
                                          idSede = d.Secuencial
                                      }
                                    );

                var vacaciones = db.Vacaciones.Where(x => x.Fecha >= fechaDesde && x.Fecha < fechaHasta).ToList();
                var permisos = db.Permiso.Where(x => x.FechaInicio >= fechaDesde && x.FechaFin < fechaHasta).ToList();

                var datosColaboradores = new List<object>();
                foreach (var colab in colaboradores)
                {
                    var listaSegmento = new List<object>();
                    string tipo = "", tipoAux = "";
                    int tamanioSegmento = 0;
                    DateTime fechaDesdeAux = fechaDesde;
                    DateTime fechaInicio = fechaDesdeAux;
                    DateTime fechaInicioPermiso = DateTime.Now;
                    DateTime fechaFinPermiso = DateTime.Now;
                    while (fechaDesdeAux < fechaHasta)
                    {
                        bool tieneVacaciones = vacaciones.Where(x => x.SecuencialColaborador == colab.idColaborador && x.Fecha.Date == fechaDesdeAux).Count() > 0;
                        if (tieneVacaciones)
                        {
                            if (tipo == "" || tipo == "V")
                            {
                                tamanioSegmento++;
                            }
                            tipoAux = "V";
                        }
                        else
                        {
                            bool tienePermisos = permisos.Where(x => x.SecuencialColaborador == colab.idColaborador && x.FechaInicio.Date == fechaDesdeAux.Date).Count() > 0;
                            if (tienePermisos)
                            {
                                var permiso = permisos.Where(x => x.SecuencialColaborador == colab.idColaborador && x.FechaInicio.Date == fechaDesdeAux.Date).FirstOrDefault();
                                fechaInicioPermiso = permiso.FechaInicio;
                                fechaFinPermiso = permiso.FechaFin;
                                //if (tipo == "" || tipo == "P")
                                //{
                                //    tamanioSegmento++;
                                //}                                
                                tipoAux = "P";
                            }
                            else
                            {
                                if (tipo == "" || tipo == "D")
                                {
                                    tamanioSegmento++;
                                }
                                tipoAux = "D";
                            }
                        }

                        if (tipo != tipoAux || tipoAux == "P")//Aquí hay un cambio, los permisos siempre son cambios
                        {
                            if (tipo == "") //La primera vez es vacío por eso es que no se guarda
                            {
                                tipo = tipoAux;
                            }
                            else
                            {
                                string toolTip = "";
                                if (tipo == "P")
                                {
                                    toolTip += "Permiso: ";
                                    toolTip += colab.nombre + ", \n";
                                    toolTip += "Fecha: " + fechaInicio.ToString("dd/MM") + ", \n";
                                    toolTip += "Hora: " + fechaInicioPermiso.ToString("HH:mm") + " - " + fechaFinPermiso.AddDays(-1).ToString("HH:mm");
                                }
                                else if (tipo == "V")
                                {
                                    toolTip += "Vacaciones: ";
                                    toolTip += colab.nombre + ", \n";
                                    toolTip += fechaInicio.ToString("dd/MM");
                                    toolTip += " - " + fechaDesdeAux.AddDays(-1).ToString("dd/MM");
                                }
                                else
                                {
                                    toolTip += colab.nombre + ", \n";
                                    toolTip += fechaInicio.ToString("dd/MM");
                                    toolTip += " - " + fechaDesdeAux.AddDays(-1).ToString("dd/MM");
                                }

                                var segmento = new
                                {
                                    tipo = tipo,
                                    clase = "div-rrhh-" + tipo,
                                    tamanio = tamanioSegmento == 0 ? 1 : tamanioSegmento,
                                    toolTip = toolTip
                                };
                                tipo = tipoAux;
                                listaSegmento.Add(segmento);
                                tamanioSegmento = 1;

                                fechaInicio = fechaDesdeAux;
                            }
                        }

                        fechaDesdeAux = fechaDesdeAux.AddDays(1);
                    }
                    //Aqui se acaba por lo tanto hay un cambio
                    string toolTipFinal = "";
                    if (tipo == "P")
                    {
                        toolTipFinal = "Permiso: ";
                        toolTipFinal += colab.nombre + ", \n";
                        toolTipFinal += "Fecha: " + fechaInicio.ToString("dd/MM") + ", \n";
                        toolTipFinal += "Hora: " + fechaInicio.ToString("HH:mm") + " - " + fechaDesdeAux.AddDays(-1).ToString("HH:mm");
                    }
                    else if (tipo == "V")
                    {
                        toolTipFinal = "Vacaciones: ";
                        toolTipFinal += colab.nombre + ", \n";
                        toolTipFinal += fechaInicio.ToString("dd/MM");
                        toolTipFinal += " - " + fechaDesdeAux.AddDays(-1).ToString("dd/MM");
                    }
                    else
                    {
                        toolTipFinal += colab.nombre + ", \n";
                        toolTipFinal += fechaInicio.ToString("dd/MM");
                        toolTipFinal += " - " + fechaDesdeAux.AddDays(-1).ToString("dd/MM");
                    }

                    var segmentoFinal = new
                    {
                        tipo = tipo,
                        clase = "div-rrhh-" + tipo,
                        tamanio = tamanioSegmento,
                        toolTip = toolTipFinal
                    };

                    listaSegmento.Add(segmentoFinal);

                    datosColaboradores.Add(new
                    {
                        colaborador = colab,
                        segmentos = listaSegmento,
                        departamento = colab.departamento,
                        sede = colab.sede
                    });
                }

                List<object> semanasDisponibilidad = new List<object>();
                DateTime fechaDesdeInicio = fechaDesde;
                while (fechaDesdeInicio < fechaHasta)
                {
                    DateTime fechaDomingo = fechaDesdeInicio.AddDays(6);

                    semanasDisponibilidad.Add(new
                    {
                        ini = fechaDesdeInicio.ToString("dd/MM"),
                        fin = fechaDomingo.ToString("dd/MM/yy")
                    });

                    fechaDesdeInicio = fechaDesdeInicio.AddDays(7);
                }

                var resp = new
                {
                    success = true,
                    cantDias = cantSemanas * 7,
                    datos = datosColaboradores,
                    intervalo = fechaDesde.ToString("dd/MM") + " - " + fechaHasta.ToString("dd/MM"),
                    departamentos = colaboradores.Select(x => x.departamento).Distinct().ToList(),
                    sedes = colaboradores.Select(x => x.sede).Distinct().ToList(),
                    semanasDisponibilidad = semanasDisponibilidad
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(resp);
            }
        }

        //FUNCIONES GENERALES
        [HttpPost]
        [Authorize(Roles = "ADMIN, RRHH")]
        public ActionResult DarTiposPermisos()
        {
            try
            {
                var tiposPermisos = (from tp in db.TipoPermiso
                                     where tp.EstaActivo == 1
                                     select new
                                     {
                                         id = tp.Secuencial,
                                         nombre = tp.Codigo
                                     }).ToList();
                var resp = new
                {
                    success = true,
                    tiposPermisos = tiposPermisos
                };
                return Json(resp);
            }
            catch (Exception e)
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
