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
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using SifizPlanning.Controllers;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using SpreadsheetLight;
using DocumentFormat.OpenXml.Spreadsheet;
using Hangfire;
using System.Threading;

namespace SifizPlanning.Controllers
{
    public class TicketController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();
        //
        // GET: /Ticket/

        //PANTALLA INICIAL
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarTickets(int start = 0, int lenght = 12, string filtro = "", int order = 0, int asc = 1, bool todos = false, string tipoFacturable = "t")
        {
            string emailUser = User.Identity.Name;
            Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
            Colaborador col = db.Colaborador.FirstOrDefault(c => c.SecuencialPersona == user.SecuencialPersona);
            var clientes = db.GestorServicios.Where(x => x.SecuencialColaborador == col.Secuencial).Select(x => new { cliente = x.cliente.Descripcion }).ToList();

            var s = new JavaScriptSerializer();
            var jsonObj = s.Deserialize<dynamic>(filtro);

            string filtroNumero = jsonObj["numero"].ToString();
            string filtroCliente = jsonObj["cliente"];
            string filtroFecha = jsonObj["fecha"];
            string filtroAsunto = jsonObj["asunto"];
            string filtroAsignado = jsonObj["asignado"];

            /*Filtros de seleccion*/
            List<string> filtroPrioridad = new List<string>();
            bool tieneFiltroPrioridad = false;
            dynamic prioridad = jsonObj["prioridad"];
            if (prioridad != null && prioridad.GetType().FullName != "System.String")
            {
                foreach (var pri in prioridad)
                {
                    filtroPrioridad.Add(pri);
                }
                tieneFiltroPrioridad = true;
            }

            List<string> filtroCategoria = new List<string>();
            bool tieneFiltroCategoria = false;
            dynamic categoria = jsonObj["categoria"];
            if (categoria != null && categoria.GetType().FullName != "System.String")
            {
                foreach (var cat in categoria)
                {
                    filtroCategoria.Add(cat);
                }
                tieneFiltroCategoria = true;
            }

            List<string> filtroEstado = new List<string>();
            bool tieneFiltroEstado = false;
            dynamic estado = jsonObj["estado"];
            if (estado != null && estado.GetType().FullName != "System.String")
            {
                foreach (var est in estado)
                {
                    filtroEstado.Add(est);
                }
                tieneFiltroEstado = true;
            }

            List<string> filtroProximaActividad = new List<string>();
            bool tieneFiltroProximaActividad = false;
            dynamic proximaActividad = jsonObj["proximaActividad"];
            if (proximaActividad != null && proximaActividad.GetType().FullName != "System.String")
            {
                foreach (var prox in proximaActividad)
                {
                    filtroProximaActividad.Add(prox);
                }
                tieneFiltroProximaActividad = true;
            }

            List<string> filtroTicketVersionCliente = new List<string>();
            bool tieneFiltroTicketVersionCliente = false;
            dynamic ticketVersionCliente = jsonObj["ticketVersionCliente"];
            if (ticketVersionCliente != null && ticketVersionCliente.GetType().FullName != "System.String")
            {
                foreach (var tvc in ticketVersionCliente)
                {
                    filtroTicketVersionCliente.Add(tvc);
                }
                tieneFiltroTicketVersionCliente = true;
            }
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
                                  orderby t.Secuencial ascending
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
                                      ticketVersionCliente = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Descripcion : "NO ASIGNADO",
                                      ticketVersionClienteId = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Secuencial : 0,
                                      proximaActividad = pa.Codigo,
                                      clase = (t.estadoTicket.Codigo == "CERRADO") ? "fondoCerrado" :
                                              (t.estadoTicket.Codigo == "ANULADO") ? "fondoAnulado" :
                                              (t.estadoTicket.Codigo == "RECHAZADO") ? "fondoAnulado" :
                                              (t.estadoTicket.Codigo == "ESPERANDO LLAMADA" || t.estadoTicket.Codigo == "ESPERANDO RESPUESTA") ? "fondoCliente" :
                                              (t.estadoTicket.Codigo == "ABIERTO") ? "fondoAbierto" : "fondoDesarrollo",
                                      semaforos = db.SemaforoPrioridad.Where(w => w.SecuencialPrioridad == pr.Secuencial).ToList(),
                                      horasCreado = t.HorasCreado,
                                      semaforosResolucion = db.ResolucionSemaforoPrioridad.Where(w => w.SecuencialPrioridad == pr.Secuencial).ToList(),
                                      horasResolucion = t.HorasResolucion,
                                      pendienteAPago = t.PendienteAPago,
                                      errorInfraestructura = t.ErrorInfraestructura,
                                      revisado = t.Revisado
                                  }).ToList();

            if (User.IsInRole("GESTOR") && !User.IsInRole("ADMIN"))
            {
                var ticketTempParcial = (from t in ticketsParcial
                                         from c in clientes
                                         where t.cliente == c.cliente
                                         select t).ToList();
                ticketsParcial = ticketTempParcial;
            }

            var tickets = ticketsParcial;

            //En el caso que no se muestren todos los tickets se realiza la consulta de asignados aparte lo que optimiza el rendimiento, si se muestran los 6000 y mas tickets se realiza la consulta de asignados dentro de la de tickets
            if (todos == false)
            {
                var asignados = (from t in db.Ticket
                                 join ttar in db.TicketTarea on t.Secuencial equals ttar.SecuencialTicket
                                 join tar in db.Tarea on ttar.SecuencialTarea equals tar.Secuencial
                                 join c in db.Colaborador on tar.SecuencialColaborador equals c.Secuencial
                                 join p in db.Persona on c.SecuencialPersona equals p.Secuencial
                                 where tar.SecuencialEstadoTarea != 4
                                 orderby tar.FechaInicio descending
                                 select new
                                 {
                                     nombre = p.Nombre1 + " " + p.Apellido1,
                                     numero = t.Secuencial
                                 }).ToList();

                ticketsParcial = ticketsParcial.Where(x => x.estado != "CERRADO" && x.estado != "ANULADO" && x.estado != "RECHAZADO").ToList();

                //Se crea la variable tickets con su colaborador asignado
                tickets = (from t in ticketsParcial
                           select new
                           {
                               numero = t.numero,
                               cliente = t.cliente,
                               fecha = t.fecha,
                               asunto = t.asunto,
                               seFactura = t.seFactura,
                               facturado = t.facturado,
                               asignado = (
                               db.TicketTarea.Where(x => x.SecuencialTicket == t.numero && x.EstaActiva == 1 && x.tarea.SecuencialEstadoTarea != 4).Count() > 0
                          ) ?
                              asignados.Where(x => x.numero.ToString().Equals(t.numero.ToString())).FirstOrDefault().nombre.ToString()
                            : "NO ASIGNADO",
                               prioridad = t.prioridad,
                               categoria = t.categoria,
                               estado = t.estado,
                               ticketVersionCliente = t.ticketVersionCliente,
                               ticketVersionClienteId = t.ticketVersionClienteId,
                               proximaActividad = t.proximaActividad,
                               clase = t.clase,
                               semaforos = t.semaforos,
                               horasCreado = t.horasCreado,
                               semaforosResolucion = t.semaforosResolucion,
                               horasResolucion = t.horasResolucion,
                               pendienteAPago = t.pendienteAPago,
                               errorInfraestructura = t.errorInfraestructura,
                               revisado = t.revisado
                           }).ToList();

            }
            else
            {
                tickets = (from t in db.Ticket
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
                           orderby t.FechaCreado ascending
                           select new
                           {
                               numero = t.Secuencial,
                               cliente = pc.cliente.Descripcion,
                               fecha = t.FechaCreado,
                               asunto = t.Asunto,
                               seFactura = t.SeFactura,
                               facturado = t.Facturado,
                               asignado = (
                                        db.TicketTarea.Where(x => x.SecuencialTicket == t.Secuencial && x.EstaActiva == 1 && x.tarea.SecuencialEstadoTarea != 4).Count() > 0
                                   ) ?
                                       (from p in db.Persona
                                        join c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
                                        join tar in db.Tarea on c.Secuencial equals tar.SecuencialColaborador
                                        join ttar in db.TicketTarea on tar.Secuencial equals ttar.SecuencialTarea
                                        orderby tar.FechaInicio descending
                                        where ttar.SecuencialTicket == t.Secuencial
                                        select p.Nombre1 + " " + p.Apellido1).FirstOrDefault()
                                     : "NO ASIGNADO",
                               prioridad = pr.Codigo,
                               categoria = ct.Codigo,
                               estado = et.Codigo,
                               ticketVersionCliente = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Descripcion : "NO ASIGNADO",
                               ticketVersionClienteId = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Secuencial : 0,
                               proximaActividad = pa.Codigo,
                               clase = (t.estadoTicket.Codigo == "CERRADO") ? "fondoCerrado" :
                                       (t.estadoTicket.Codigo == "ANULADO") ? "fondoAnulado" :
                                       (t.estadoTicket.Codigo == "RECHAZADO") ? "fondoAnulado" :
                                       (t.estadoTicket.Codigo == "ESPERANDO LLAMADA" || t.estadoTicket.Codigo == "ESPERANDO RESPUESTA") ? "fondoCliente" :
                                       (t.estadoTicket.Codigo == "ABIERTO") ? "fondoAbierto" : "fondoDesarrollo",
                               semaforos = db.SemaforoPrioridad.Where(w => w.SecuencialPrioridad == pr.Secuencial).ToList(),
                               horasCreado = t.HorasCreado,
                               semaforosResolucion = db.ResolucionSemaforoPrioridad.Where(w => w.SecuencialPrioridad == pr.Secuencial).ToList(),
                               horasResolucion = t.HorasResolucion,
                               pendienteAPago = t.PendienteAPago,
                               errorInfraestructura = t.ErrorInfraestructura,
                               revisado = t.Revisado
                           }).ToList();

                if (User.IsInRole("GESTOR") && !User.IsInRole("ADMIN"))
                {
                    var ticketsAll = (from t in tickets
                                      from c in clientes
                                      where t.cliente == c.cliente
                                      select t).ToList();
                    tickets = ticketsAll;
                }
            }

            //Filtro de si se facturan
            if (tipoFacturable != "t")//Todos
            {
                if (tipoFacturable == "F")//Facturables
                {
                    tickets = tickets.Where(x => x.seFactura == true).ToList();
                }
                else if (tipoFacturable == "FF")//Facturables Facturados
                {
                    tickets = tickets.Where(x => x.seFactura == true && x.facturado == true).ToList();
                }
                else if (tipoFacturable == "FNF")//Facturables No Facturados
                {
                    tickets = tickets.Where(x => x.seFactura == true && x.facturado == false).ToList();
                }
                else if (tipoFacturable == "PAP")
                {
                    tickets = tickets.Where(x => x.pendienteAPago == true).ToList();
                }
                else if (tipoFacturable == "SF")
                {
                    tickets = tickets.Where(x => x.seFactura == false).ToList();
                }
            }

            //Aplicando los filtros
            if (filtroNumero != "")
            {
                tickets = (from t in tickets
                           where t.numero.ToString().PadLeft(6, '0').Contains(filtroNumero)
                           select t).ToList();
            }
            if (filtroCliente != "")
            {
                tickets = (from t in tickets
                           where t.cliente.ToString().ToLower().Contains(filtroCliente.ToLower())
                           select t).ToList();
            }
            if (filtroFecha != "")
            {
                tickets = (from t in tickets
                           where t.fecha.ToString("dd/MM/yyyy").Contains(filtroFecha)
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
            if (tieneFiltroPrioridad)
            {
                tickets = (from t in tickets
                           where filtroPrioridad.Contains(t.prioridad.ToString().ToUpper())
                           select t).ToList();
            }
            if (tieneFiltroCategoria)
            {
                tickets = (from t in tickets
                           where filtroCategoria.Contains(t.categoria.ToString().ToUpper())
                           select t).ToList();
            }
            if (tieneFiltroEstado)
            {
                tickets = (from t in tickets
                           where filtroEstado.Contains(t.estado.ToString().ToUpper())
                           select t).ToList();
            }
            if (tieneFiltroProximaActividad)
            {
                tickets = (from t in tickets
                           where filtroProximaActividad.Contains(t.proximaActividad.ToString().ToUpper())
                           select t).ToList();
            }
            if (tieneFiltroTicketVersionCliente)
            {
                tickets = (from t in tickets
                           where filtroTicketVersionCliente.Contains(t.ticketVersionCliente.ToString().ToUpper())
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

                    case 4:

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

                    case 5:

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

                    case 6:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.prioridad
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.prioridad descending
                                       select t).ToList();
                        }

                        break;

                    case 7:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.categoria
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.categoria descending
                                       select t).ToList();
                        }

                        break;

                    case 8:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.estado
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.estado descending
                                       select t).ToList();
                        }

                        break;
                    case 9:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.proximaActividad
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.proximaActividad descending
                                       select t).ToList();
                        }

                        break;
                    case 10:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.ticketVersionCliente
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.ticketVersionCliente descending
                                       select t).ToList();
                        }

                        break;
                }
            }

            var proximasActividades = tickets.Select(x => x.proximaActividad).Distinct().ToList();
            proximasActividades.Sort((x1, x2) => String.Compare(x1, x2));

            int totalTickets = tickets.Count();
            tickets = tickets.Skip(start).Take(lenght).ToList();



            //Calculando los dias desde la fecha en el servidor
            List<object> ticketsResp = new List<object>();
            for (int i = 0; i < tickets.Count; i++)
            {
                var t = tickets[i];
                ticketsResp.Add(new
                {
                    numero = t.numero,
                    cliente = t.cliente,
                    fecha = t.fecha,
                    dias = DateTime.Now.Subtract(t.fecha).Days,
                    asunto = t.asunto,
                    asignado = t.asignado,
                    prioridad = t.prioridad,
                    categoria = t.categoria,
                    estado = t.estado,
                    proximaActividad = t.proximaActividad,
                    clase = t.clase,
                    ticketVersionCliente = t.ticketVersionCliente,
                    semaforo = t.semaforos != null ? t.semaforos.Where(w => w.horas <= t.horasCreado).OrderByDescending(d => d.horas).FirstOrDefault()?.semaforo.Codigo ?? "VERDE" : "VERDE",
                    semaforoResolucion = t.semaforosResolucion != null ? t.semaforosResolucion.Where(w => w.Horas <= t.horasResolucion).OrderByDescending(d => d.Horas).FirstOrDefault()?.semaforo.Codigo ?? "VERDE" : "VERDE",
                });
            }

            var resp = new
            {
                success = true,
                tickets = ticketsResp,
                totalTickets = totalTickets,
                proximasActividades = proximasActividades
            };
            return Json(resp);
        }

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarTicketsGestores(int start = 0, int lenght = 12, string filtro = "", int order = 0, int asc = 1, bool todos = false, string tipoFacturable = "t")
        {
            string emailUser = User.Identity.Name;
            Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
            Colaborador col = db.Colaborador.FirstOrDefault(c => c.SecuencialPersona == user.SecuencialPersona);

            var clientes = db.GestorServicios.Where(x => x.SecuencialColaborador == col.Secuencial).Select(x => new { cliente = x.cliente.Descripcion }).ToList();

            var s = new JavaScriptSerializer();
            var jsonObj = s.Deserialize<dynamic>(filtro);

            string filtroNumero = jsonObj["numero"].ToString();
            string filtroCliente = jsonObj["cliente"];
            string filtroFecha = jsonObj["fecha"];
            string filtroAsunto = jsonObj["asunto"];
            string filtroAsignado = jsonObj["asignado"];

            /*Filtros de seleccion*/
            List<string> filtroPrioridad = new List<string>();
            bool tieneFiltroPrioridad = false;
            dynamic prioridad = jsonObj["prioridad"];
            if (prioridad != null && prioridad.GetType().FullName != "System.String")
            {
                foreach (var pri in prioridad)
                {
                    filtroPrioridad.Add(pri);
                }
                tieneFiltroPrioridad = true;
            }

            List<string> filtroCategoria = new List<string>();
            bool tieneFiltroCategoria = false;
            dynamic categoria = jsonObj["categoria"];
            if (categoria != null && categoria.GetType().FullName != "System.String")
            {
                foreach (var cat in categoria)
                {
                    filtroCategoria.Add(cat);
                }
                tieneFiltroCategoria = true;
            }

            List<string> filtroEstado = new List<string>();
            bool tieneFiltroEstado = false;
            dynamic estado = jsonObj["estado"];
            if (estado != null && estado.GetType().FullName != "System.String")
            {
                foreach (var est in estado)
                {
                    filtroEstado.Add(est);
                }
                tieneFiltroEstado = true;
            }

            List<string> filtroProximaActividad = new List<string>();
            bool tieneFiltroProximaActividad = false;
            dynamic proximaActividad = jsonObj["proximaActividad"];
            if (proximaActividad != null && proximaActividad.GetType().FullName != "System.String")
            {
                foreach (var prox in proximaActividad)
                {
                    filtroProximaActividad.Add(prox);
                }
                tieneFiltroProximaActividad = true;
            }

            List<string> filtroTicketVersionCliente = new List<string>();
            bool tieneFiltroTicketVersionCliente = false;
            dynamic ticketVersionCliente = jsonObj["ticketVersionCliente"];
            if (ticketVersionCliente != null && ticketVersionCliente.GetType().FullName != "System.String")
            {
                foreach (var tvc in ticketVersionCliente)
                {
                    filtroTicketVersionCliente.Add(tvc);
                }
                tieneFiltroTicketVersionCliente = true;
            }

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
                                  orderby t.Secuencial ascending
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
                                      ticketVersionCliente = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Descripcion : "NO ASIGNADO",
                                      ticketVersionClienteId = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Secuencial : 0,
                                      clase = (t.estadoTicket.Codigo == "CERRADO") ? "fondoCerrado" :
                                              (t.estadoTicket.Codigo == "ANULADO") ? "fondoAnulado" :
                                              (t.estadoTicket.Codigo == "RECHAZADO") ? "fondoAnulado" :
                                              (t.estadoTicket.Codigo == "ESPERANDO LLAMADA" || t.estadoTicket.Codigo == "ESPERANDO RESPUESTA") ? "fondoCliente" :
                                              (t.estadoTicket.Codigo == "ABIERTO") ? "fondoAbierto" : "fondoDesarrollo",
                                      semaforos = db.SemaforoPrioridad.Where(w => w.SecuencialPrioridad == pr.Secuencial).ToList(),
                                      horasCreado = t.HorasCreado,
                                      semaforosResolucion = db.ResolucionSemaforoPrioridad.Where(w => w.SecuencialPrioridad == pr.Secuencial).ToList(),
                                      horasResolucion = t.HorasResolucion
                                  }).ToList();


            var ticketsNew = (from t in ticketsParcial
                              from c in clientes
                              where t.cliente == c.cliente
                              select t).ToList();
            var tickets = ticketsNew;

            //En el caso que no se muestren todos los tickets se realiza la consulta de asignados aparte lo que optimiza el rendimiento, si se muestran los 6000 y mas tickets se realiza la consulta de asignados dentro de la de tickets
            if (todos == false)
            {
                var asignados = (from t in db.Ticket
                                 join ttar in db.TicketTarea on t.Secuencial equals ttar.SecuencialTicket
                                 join tar in db.Tarea on ttar.SecuencialTarea equals tar.Secuencial
                                 join c in db.Colaborador on tar.SecuencialColaborador equals c.Secuencial
                                 join p in db.Persona on c.SecuencialPersona equals p.Secuencial
                                 where tar.SecuencialEstadoTarea != 4
                                 orderby tar.FechaInicio descending
                                 select new
                                 {
                                     nombre = p.Nombre1 + " " + p.Apellido1,
                                     numero = t.Secuencial
                                 }).ToList();

                ticketsNew = ticketsNew.Where(x => x.estado != "CERRADO" && x.estado != "ANULADO" && x.estado != "RECHAZADO").ToList();

                //Se crea la variable tickets con su colaborador asignado
                tickets = (from t in ticketsNew
                           select new
                           {
                               numero = t.numero,
                               cliente = t.cliente,
                               fecha = t.fecha,
                               asunto = t.asunto,
                               seFactura = t.seFactura,
                               facturado = t.facturado,
                               asignado = (
                               db.TicketTarea.Where(x => x.SecuencialTicket == t.numero && x.EstaActiva == 1 && x.tarea.SecuencialEstadoTarea != 4).Count() > 0
                          ) ?
                              asignados.Where(x => x.numero.ToString().Equals(t.numero.ToString())).FirstOrDefault().nombre.ToString()
                            : "NO ASIGNADO",
                               prioridad = t.prioridad,
                               categoria = t.categoria,
                               estado = t.estado,
                               proximaActividad = t.proximaActividad,
                               ticketVersionCliente = t.ticketVersionCliente,
                               ticketVersionClienteId = t.ticketVersionClienteId,
                               clase = t.clase,
                               semaforos = t.semaforos,
                               horasCreado = t.horasCreado,
                               semaforosResolucion = t.semaforosResolucion,
                               horasResolucion = t.horasResolucion

                           }).ToList();

            }
            else
            {
                tickets = (from t in db.Ticket
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
                           orderby t.FechaCreado ascending
                           select new
                           {
                               numero = t.Secuencial,
                               cliente = pc.cliente.Descripcion,
                               fecha = t.FechaCreado,
                               asunto = t.Asunto,
                               seFactura = t.SeFactura,
                               facturado = t.Facturado,
                               asignado = (
                                        db.TicketTarea.Where(x => x.SecuencialTicket == t.Secuencial && x.EstaActiva == 1 && x.tarea.SecuencialEstadoTarea != 4).Count() > 0
                                   ) ?
                                       (from p in db.Persona
                                        join c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
                                        join tar in db.Tarea on c.Secuencial equals tar.SecuencialColaborador
                                        join ttar in db.TicketTarea on tar.Secuencial equals ttar.SecuencialTarea
                                        orderby tar.FechaInicio descending
                                        where ttar.SecuencialTicket == t.Secuencial
                                        select p.Nombre1 + " " + p.Apellido1).FirstOrDefault()
                                     : "NO ASIGNADO",
                               prioridad = pr.Codigo,
                               categoria = ct.Codigo,
                               estado = et.Codigo,
                               proximaActividad = pa.Codigo,
                               ticketVersionCliente = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Descripcion : "NO ASIGNADO",
                               ticketVersionClienteId = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Secuencial : 0,
                               clase = (t.estadoTicket.Codigo == "CERRADO") ? "fondoCerrado" :
                                       (t.estadoTicket.Codigo == "ANULADO") ? "fondoAnulado" :
                                       (t.estadoTicket.Codigo == "RECHAZADO") ? "fondoAnulado" :
                                       (t.estadoTicket.Codigo == "ESPERANDO LLAMADA" || t.estadoTicket.Codigo == "ESPERANDO RESPUESTA") ? "fondoCliente" :
                                       (t.estadoTicket.Codigo == "ABIERTO") ? "fondoAbierto" : "fondoDesarrollo",
                               semaforos = db.SemaforoPrioridad.Where(w => w.SecuencialPrioridad == pr.Secuencial).ToList(),
                               horasCreado = t.HorasCreado,
                               semaforosResolucion = db.ResolucionSemaforoPrioridad.Where(w => w.SecuencialPrioridad == pr.Secuencial).ToList(),
                               horasResolucion = t.HorasResolucion
                           }).ToList();

                var ticketsAll = (from t in tickets
                                  from c in clientes
                                  where t.cliente == c.cliente
                                  select t).ToList();
                tickets = ticketsAll;
            }

            //Filtro de si se facturan
            if (tipoFacturable != "t")//Todos
            {
                if (tipoFacturable == "F")//Facturables
                {
                    tickets = tickets.Where(x => x.seFactura == true).ToList();
                }
                else if (tipoFacturable == "FF")//Facturables Facturados
                {
                    tickets = tickets.Where(x => x.seFactura == true && x.facturado == true).ToList();
                }
                else if (tipoFacturable == "FNF")//Facturables No Facturados
                {
                    tickets = tickets.Where(x => x.seFactura == true && x.facturado == false).ToList();
                }
                else if (tipoFacturable == "SF")
                {
                    tickets = tickets.Where(x => x.seFactura == false).ToList();
                }
            }

            //Aplicando los filtros
            if (filtroNumero != "")
            {
                tickets = (from t in tickets
                           where t.numero.ToString().PadLeft(6, '0').Contains(filtroNumero)
                           select t).ToList();
            }
            if (filtroCliente != "")
            {
                tickets = (from t in tickets
                           where t.cliente.ToString().ToLower().Contains(filtroCliente.ToLower())
                           select t).ToList();
            }
            if (filtroFecha != "")
            {
                tickets = (from t in tickets
                           where t.fecha.ToString("dd/MM/yyyy").Contains(filtroFecha)
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
            if (tieneFiltroPrioridad)
            {
                tickets = (from t in tickets
                           where filtroPrioridad.Contains(t.prioridad.ToString().ToUpper())
                           select t).ToList();
            }
            if (tieneFiltroCategoria)
            {
                tickets = (from t in tickets
                           where filtroCategoria.Contains(t.categoria.ToString().ToUpper())
                           select t).ToList();
            }
            if (tieneFiltroEstado)
            {
                tickets = (from t in tickets
                           where filtroEstado.Contains(t.estado.ToString().ToUpper())
                           select t).ToList();
            }
            if (tieneFiltroProximaActividad)
            {
                tickets = (from t in tickets
                           where filtroProximaActividad.Contains(t.proximaActividad.ToString().ToUpper())
                           select t).ToList();
            }
            if (tieneFiltroTicketVersionCliente)
            {
                tickets = (from t in tickets
                           where filtroTicketVersionCliente.Contains(t.ticketVersionCliente.ToString().ToUpper())
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

                    case 4:

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

                    case 5:

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

                    case 6:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.prioridad
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.prioridad descending
                                       select t).ToList();
                        }

                        break;

                    case 7:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.categoria
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.categoria descending
                                       select t).ToList();
                        }

                        break;

                    case 8:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.estado
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.estado descending
                                       select t).ToList();
                        }

                        break;
                    case 9:

                        if (asc == 1)
                        {
                            tickets = (from t in tickets
                                       orderby t.proximaActividad
                                       select t).ToList();
                        }
                        else
                        {
                            tickets = (from t in tickets
                                       orderby t.proximaActividad descending
                                       select t).ToList();
                        }

                        break;
                }
            }

            var proximasActividades = tickets.Select(x => x.proximaActividad).Distinct().ToList();
            proximasActividades.Sort((x1, x2) => String.Compare(x1, x2));

            int totalTickets = tickets.Count();
            tickets = tickets.Skip(start).Take(lenght).ToList();



            //Calculando los dias desde la fecha en el servidor
            List<object> ticketsResp = new List<object>();
            for (int i = 0; i < tickets.Count; i++)
            {
                var t = tickets[i];
                ticketsResp.Add(new
                {
                    numero = t.numero,
                    cliente = t.cliente,
                    fecha = t.fecha,
                    dias = DateTime.Now.Subtract(t.fecha).Days,
                    asunto = t.asunto,
                    asignado = t.asignado,
                    prioridad = t.prioridad,
                    categoria = t.categoria,
                    estado = t.estado,
                    proximaActividad = t.proximaActividad,
                    ticketVersionCliente = t.ticketVersionCliente,
                    ticketV = t.ticketVersionClienteId,
                    clase = t.clase,
                    semaforo = t.semaforos != null ? t.semaforos.Where(w => w.horas <= t.horasCreado).OrderByDescending(d => d.horas).FirstOrDefault()?.semaforo.Codigo ?? "VERDE" : "VERDE",
                    semaforoResolucion = t.semaforosResolucion != null ? t.semaforosResolucion.Where(w => w.Horas <= t.horasResolucion).OrderByDescending(d => d.Horas).FirstOrDefault()?.semaforo.Codigo ?? "VERDE" : "VERDE",

                }
                );
            }

            var resp = new
            {
                success = true,
                tickets = ticketsResp,
                totalTickets = totalTickets,
                proximasActividades = proximasActividades
            };
            return Json(resp);
        }


        //SELECCION DEL TICKET EN LA PANTALLA INICIAL
        [Authorize(Roles = "COORDINADOR, ADMIN, COTIZADOR, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarDatosTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encuentra en el sistema");
                }
                var motivoTrabajoTicket = db.MotivoTrabajoTicket.Where(w => w.SecuencialTicket == idTicket).
                    Select(s => new
                    {
                        codigo = s.motivoTrabajo != null ? s.motivoTrabajo.Codigo : "",
                        descripcion = s.motivoTrabajo != null ? s.motivoTrabajo.Descripcion : "",
                        nombre = s.motivoTrabajo != null ? s.motivoTrabajo.Nombre : ""
                    }).FirstOrDefault();

                var datosTicketTemp = (from t in db.Ticket
                                       join
                                           pcl in db.Persona_Cliente on t.persona_cliente equals pcl
                                       join
                                           etk in db.EstadoTicket on t.estadoTicket equals etk
                                       join
                                           ptk in db.PrioridadTicket on t.prioridadTicket equals ptk
                                       join
                                           ctk in db.CategoriaTicket on t.categoriaTicket equals ctk
                                       where t.Secuencial == idTicket
                                       select new
                                       {
                                           id = idTicket,
                                           usuarioCliente = pcl.persona.Nombre1 + " " + pcl.persona.Apellido1,
                                           idCliente = pcl.SecuencialCliente,
                                           cliente = pcl.cliente.Descripcion,
                                           clienteTelefono = pcl.Telefono,
                                           telefono = t.Telefono,
                                           reporto = t.ReportadoPor,
                                           reputacion = t.Reputacion,
                                           fecha = t.FechaCreado,
                                           fechaRevisado = t.FechaRevisado ?? new DateTime(0001, 01, 01),
                                           estimacion = t.Estimacion,
                                           asunto = t.Asunto.Length > 100 ? t.Asunto.Substring(0, 100) + "..." : t.Asunto,
                                           detalle = t.Detalle,
                                           estado = etk.Codigo,
                                           ticketVersionCliente = db.TicketVersionCliente.Where(s => s.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(s => s.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Descripcion : "NO ASIGNADO",
                                           ticketVersionClienteId = db.TicketVersionCliente.Where(s => s.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(s => s.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Secuencial : 0,
                                           prioridad = ptk.Secuencial,
                                           justificacionPrioridad = (from mtp in db.MotivoPrioridadTicket
                                                                     where mtp.SecuencialTicket == idTicket
                                                                     select mtp.Motivo).FirstOrDefault() ?? "",
                                           prioridadDesc = ptk.Codigo,
                                           categoria = ctk.Secuencial,
                                           categoriaDesc = ctk.Codigo,
                                           diasGarantia = t.DiasGarantia ?? 30,
                                           seFactura = t.SeFactura,
                                           facturado = t.Facturado,
                                           reprocesos = t.Reprocesos,
                                           ingresoInterno = t.IngresoInterno,
                                           actividadProxima = t.SecuencialProximaActividad,
                                           actividadProximaDesc = t.proximaActividad.Codigo,
                                           tipoRecurso = t.SecuencialTipoRecurso,
                                           tipoRecursoDesc = t.tipoRecurso.Codigo,
                                           requiereTesting = t.RequiereTesting,
                                           pendienteAPago = t.PendienteAPago,
                                           entregableGarantia = (from e in db.EntregableMotivoTrabajo
                                                                 join et in db.ENTREGABLETICKET on e.Secuencial equals et.SecuencialEntregable
                                                                 where et.SecuencialTicket == idTicket && e.EstaActivo == 1
                                                                 select new
                                                                 {
                                                                     nombre = e.Nombre + " __ Fecha Vencimiento GT: " +
                                                                     SqlFunctions.DateName("day", DbFunctions.AddDays(e.FechaProduccion, e.motivoTrabajo.motivoTrabajoInformacionAdicional != null ? e.motivoTrabajo.motivoTrabajoInformacionAdicional.DiasGarantia : 0)).ToString() + " de " +
                                                                     SqlFunctions.DateName("mm", DbFunctions.AddDays(e.FechaProduccion, e.motivoTrabajo.motivoTrabajoInformacionAdicional != null ? e.motivoTrabajo.motivoTrabajoInformacionAdicional.DiasGarantia : 0)).ToString() + " del " +
                                                                     SqlFunctions.DateName("yyyy", DbFunctions.AddDays(e.FechaProduccion, e.motivoTrabajo.motivoTrabajoInformacionAdicional != null ? e.motivoTrabajo.motivoTrabajoInformacionAdicional.DiasGarantia : 0)).ToString()
                                                                 }).FirstOrDefault(),
                                           adjuntos = (from adjt in db.AdjuntoTicket
                                                       where adjt.SecuencialTicket == idTicket
                                                       select new { adjt.Secuencial, adjt.Url }).ToList(),
                                           botones = (from bot in db.BotonEstadoTicket
                                                      join
                          estk in db.EstadoTicket on bot.estadoTicket equals estk
                                                      join
    fet in db.FlujoEstadoTicket on estk.Secuencial equals fet.SecuencialEstadoTicketFinal
                                                      where fet.SecuencialEstadoTicketInicial == ticket.SecuencialEstadoTicket && fet.EstaActivo == 1
                                                      select new
                                                      {
                                                          clase = bot.ClaseCss,
                                                          icono = bot.CssIcono,
                                                          funcion = bot.Funcion,
                                                          texto = bot.Descripcion
                                                      }),
                                           verificador = t.NumeroVerificador,
                                           semaforos = db.SemaforoPrioridad.Where(w => w.SecuencialPrioridad == ptk.Secuencial).Select(e => new { horas = e.horas, value = e.semaforo.Codigo }).ToList(),
                                           horasCreado = t.HorasCreado,
                                           semaforo = "",
                                           semaforosResolucion = db.ResolucionSemaforoPrioridad.Where(w => w.SecuencialPrioridad == ptk.Secuencial).Select(e => new { horas = e.Horas, value = e.semaforo.Codigo }).ToList(),
                                           horasResolucion = t.HorasResolucion,
                                           semaforoResolucion = "",
                                           modulo = db.Modulo.Where(m => m.Secuencial == t.SecuencialModulo).FirstOrDefault().Descripcion ?? "No asignado",
                                           errorInfraestructura = t.ErrorInfraestructura,
                                           revisado = t.Revisado
                                       }).FirstOrDefault();

                //Calculando la reputación
                var ticketsCliente = (from t in db.Ticket
                                      join pc in db.Persona_Cliente on t.persona_cliente equals pc
                                      where pc.SecuencialCliente == datosTicketTemp.idCliente
                                      select new
                                      {
                                          idTicket = t.Secuencial,
                                          reputacion = t.Reputacion
                                      }).ToList();
                int categoriaRev = (from x in db.Ticket_CategoriaRevisada
                                    where x.SecuencialTicket == idTicket
                                    select x.SecuencialCategoriaTicket).FirstOrDefault();
                int prioridadRev = (from y in db.Ticket_PrioridadRevisada
                                    where y.SecuencialTicket == idTicket
                                    select y.SecuencialPrioridadTicket).FirstOrDefault();
                bool error = (from e in db.Ticket_Error
                              where e.SecuencialTicket == idTicket
                              select e.Error).FirstOrDefault();

                int cant = 5;
                int total = 5;
                //foreach (var tc in ticketsCliente)
                //{
                //    cant += 5;//5 es el valor máximo
                //    total += (int)tc.reputacion;
                //}

                float div = (float)total / (float)cant;
                int reputacion = (int)Math.Floor(div * 100);

                //Buscando las asignaciones del ticket
                List<TicketTarea> ticketsTareas = (from ttar in db.TicketTarea
                                                   where ttar.SecuencialTicket == idTicket && ttar.EstaActiva == 1
                                                   orderby ttar.tarea.FechaInicio descending
                                                   select ttar).ToList<TicketTarea>();

                List<object> asignaciones = new List<object>();
                int counter = 1;
                foreach (TicketTarea ttar in ticketsTareas)
                {
                    Tarea tar = ttar.tarea;
                    if (tar.SecuencialEstadoTarea != 4)
                    {

                        int horas = (tar.FechaFin - tar.FechaInicio).Hours;
                        if (tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
                        {
                            horas--;
                        }
                        else if (tar.FechaInicio.Hour == 13)
                        {
                            horas--;
                        }

                        int idCoordinador = 0;
                        Tarea_Coordinador tareaCorrdinador = tar.tarea_coordinador;
                        if (tareaCorrdinador != null)
                        {
                            idCoordinador = tareaCorrdinador.SecuencialColaborador;
                        }

                        var asignacion = new
                        {
                            id = counter++,
                            idTarea = tar.Secuencial,
                            idColaborador = tar.colaborador.Secuencial,
                            colaborador = tar.colaborador.persona.Nombre1 + " " + ttar.tarea.colaborador.persona.Apellido1,
                            fecha = tar.FechaInicio.ToString("dd/MM/yyyy"),
                            ubicacion = tar.SecuencialLugarTarea,
                            idActividad = tar.SecuencialActividad,
                            actividad = "",
                            idModulo = tar.SecuencialModulo,
                            modulo = "",
                            numeroHoras = horas,
                            coordinador = idCoordinador,
                            detalle = ttar.tarea.Detalle
                        };

                        asignaciones.Add(asignacion);
                    }
                }

                var datosTicket = new
                {
                    id = datosTicketTemp.id,
                    usuarioCliente = datosTicketTemp.usuarioCliente,
                    idCliente = datosTicketTemp.idCliente,
                    cliente = datosTicketTemp.cliente,
                    clienteTelefono = datosTicketTemp.clienteTelefono,
                    telefono = datosTicketTemp.telefono,
                    reporto = datosTicketTemp.reporto,
                    reputacion = datosTicketTemp.reputacion,
                    fecha = datosTicketTemp.fecha,
                    fechaRevisado = datosTicketTemp.fechaRevisado,
                    estimacion = datosTicketTemp.estimacion,
                    asunto = datosTicketTemp.asunto,
                    detalle = datosTicketTemp.detalle,
                    estado = datosTicketTemp.estado,
                    ticketVersionCliente = datosTicketTemp.ticketVersionCliente,
                    ticketVersionClienteId = datosTicketTemp.ticketVersionClienteId,
                    prioridad = datosTicketTemp.prioridad,
                    justificacionPrioridad = datosTicketTemp.justificacionPrioridad,
                    prioridadDesc = datosTicketTemp.prioridadDesc,
                    categoria = datosTicketTemp.categoria,
                    categoriaDesc = datosTicketTemp.categoriaDesc,
                    diasGarantia = datosTicketTemp.diasGarantia,
                    seFactura = datosTicketTemp.seFactura,
                    facturado = datosTicketTemp.facturado,
                    reprocesos = datosTicketTemp.reprocesos,
                    ingresoInterno = datosTicketTemp.ingresoInterno,
                    actividadProxima = datosTicketTemp.actividadProxima,
                    actividadProximaDesc = datosTicketTemp.actividadProximaDesc,
                    tipoRecurso = datosTicketTemp.tipoRecurso,
                    tipoRecursoDesc = datosTicketTemp.tipoRecursoDesc,
                    requiereTesting = datosTicketTemp.requiereTesting,
                    pendienteAPago = datosTicketTemp.pendienteAPago,
                    entregableGarantia = datosTicketTemp.entregableGarantia,
                    adjuntos = datosTicketTemp.adjuntos,
                    botones = datosTicketTemp.botones,
                    modulo = datosTicketTemp.modulo,
                    errorInfraestructura = datosTicketTemp.errorInfraestructura,
                    revisado = datosTicketTemp.revisado,
                    verificador = datosTicketTemp.verificador,
                    semaforo = datosTicketTemp.semaforos != null ? datosTicketTemp.semaforos.Where(w => w.horas <= datosTicketTemp.horasCreado).OrderByDescending(d => d.horas).FirstOrDefault()?.value ?? "VERDE" : "VERDE",
                    semaforoResolucion = datosTicketTemp.semaforosResolucion != null ? datosTicketTemp.semaforosResolucion.Where(w => w.horas <= datosTicketTemp.horasResolucion).OrderByDescending(d => d.horas).FirstOrDefault()?.value ?? "VERDE" : "VERDE",
                };

                var resp = new
                {
                    success = true,
                    datosTicket = datosTicket,
                    fechaRevisado = datosTicketTemp.fechaRevisado.ToString("dd/MM/yyyy"),
                    fecha = datosTicketTemp.fecha.ToString("dd/MM/yyyy"),
                    reputacion = reputacion,
                    asignaciones = asignaciones,
                    tecnicoSugerido = (from x in db.Ticket_Resolucion
                                       where x.SecuencialTicket == idTicket
                                       select new
                                       {
                                           idTec = x.SecuencialColaborador,
                                           nombreTecnico = x.colaborador.persona.Apellido1 + " " + x.colaborador.persona.Nombre1,
                                           observaciones = x.Descripcion
                                       }).FirstOrDefault(),
                    categoria = categoriaRev,
                    prioridad = prioridadRev,
                    error = error,
                    motivoTrabajo = motivoTrabajoTicket
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

        //Dar Contratos Mantenimiento Para Ticket
        [Authorize(Roles = "COORDINADOR, ADMIN, COTIZADOR, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarContratosMantenimiento(int idCliente, string fecha)
        {
            try
            {
                DateTime fFecha;
                if (fecha != null)
                {
                    string[] fechas = fecha.Split(new Char[] { '/' });
                    int mes = Int32.Parse(fechas[0]);
                    int anno = Int32.Parse(fechas[1]);
                    fFecha = new System.DateTime(anno, mes, 1);
                }
                else
                {
                    fFecha = DateTime.Now;
                }
                DateTime fechaMenor = new DateTime(fFecha.Year, fFecha.Month, 1);
                DateTime fechaMayor = fechaMenor.AddMonths(1);

                var clienteMotivos = (from mt in db.MotivoTrabajo
                                      join c in db.Cliente on mt.SecuencialCliente equals c.Secuencial
                                      join tm in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tm.Secuencial
                                      where mt.EstaActivo == 1 && mt.SecuencialCliente == idCliente && tm.Codigo == "PENDIENTES"
                                      select new
                                      {
                                          idCliente = c.Secuencial,
                                          codigo = mt.Codigo,
                                          fechaInicio = mt.FechaInicio,
                                          fechaVencimiento = mt.FechaFin,
                                          fechaFirma = mt.FechaInicioPlanificacion,
                                          horasMensuales = mt.HorasMes,
                                          horasConsumidas = (from T in db.Tarea
                                                             join TT in db.TicketTarea on T.Secuencial equals TT.SecuencialTarea into TTGroup
                                                             from TTG in TTGroup.DefaultIfEmpty()
                                                             join TK in db.Ticket on TTG.SecuencialTicket equals TK.Secuencial into TKGroup
                                                             from TKG in TKGroup.DefaultIfEmpty()
                                                             join C in db.Colaborador on T.SecuencialColaborador equals C.Secuencial
                                                             join P in db.Persona on C.SecuencialPersona equals P.Secuencial
                                                             join LT in db.LugarTarea on T.SecuencialLugarTarea equals LT.Secuencial
                                                             join TAR in db.TareaActividadRealizada on T.Secuencial equals TAR.SecuencialTarea
                                                             join CL in db.Cliente on T.SecuencialCliente equals CL.Secuencial
                                                             join ET in db.EstadoTicket on TKG.SecuencialEstadoTicket equals ET.Secuencial into ETGroup
                                                             from ETG in ETGroup.DefaultIfEmpty()
                                                             where
                                                                T.SecuencialCliente == idCliente
                                                                && T.FechaInicio >= fechaMenor
                                                                && T.FechaInicio < fechaMayor
                                                             select new
                                                             {
                                                                 tarea = (int?)T.Secuencial ?? 0,
                                                                 cliente = CL.Descripcion,
                                                                 ticket = (int?)TKG.Secuencial ?? 0,
                                                                 detalle = TKG.Asunto ?? T.Detalle,
                                                                 reportador = TKG.ReportadoPor,
                                                                 tecnico = P.Nombre1 + " " + P.Apellido1,
                                                                 fecha = T.FechaInicio,
                                                                 estado = ETG.Codigo ?? "FINALIZADO",
                                                                 tiempo = (SqlFunctions.DateDiff("MINUTE", TAR.HoraInicio, TAR.HoraFin)) / 60.00
                                                             }
                                                             ).ToList().GroupBy(g => new { g.cliente, g.ticket, g.detalle, g.reportador, g.tecnico, g.fecha, g.estado, g.tarea })
                                                                .Select(a => new
                                                                {
                                                                    tarea = a.Key.tarea,
                                                                    cliente = a.Key.cliente,
                                                                    numero = a.Key.ticket,
                                                                    detalle = a.Key.detalle,
                                                                    reportado = a.Key.reportador,
                                                                    asignado = a.Key.tecnico,
                                                                    estado = a.Key.estado,
                                                                    tiempo =
                                                                             db.TicketsMantenimiento.Where(s => s.SecuencialTicket == a.Key.ticket &&
                                                                                DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault() != null ?
                                                                             db.TicketsMantenimiento.Where(s => s.SecuencialTicket == a.Key.ticket &&
                                                                                DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault().Tiempo :
                                                                             db.TareaMantenimiento.Where(s => s.SecuencialTarea == a.Key.tarea &&
                                                                                DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault() != null ?
                                                                             db.TareaMantenimiento.Where(s => s.SecuencialTarea == a.Key.tarea &&
                                                                                DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault().Tiempo :
                                                                                a.Sum(s => s.tiempo.Value)
                                                                }).OrderBy(b => b.numero),
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
                                               DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) < 0 ? "VENCIDO" : "POR VENCER"
                                      }).ToList();
                var contratos = (from c in clienteMotivos
                                 where c.estado == "VIGENTE"
                                 select new
                                 {
                                     idCliente = c.idCliente,
                                     Codigo = c.codigo,
                                     FechaInicio = c.fechaInicio.ToString("dd/MM/yyyy"),
                                     FechaVencimiento = c.fechaVencimiento.ToString("dd/MM/yyyy"),
                                     FechaFirma = c.fechaFirma.ToString("dd/MM/yyyy"),
                                     HorasMensuales = c.horasMensuales ?? 0,
                                     HorasConsumidas = c.horasConsumidas.Sum(s => Math.Round(s.tiempo, MidpointRounding.AwayFromZero)),
                                     HorasDisponibles = (c.horasMensuales ?? 0) - (c.horasConsumidas.Sum(s => Math.Round(s.tiempo, MidpointRounding.AwayFromZero)))
                                 }).ToList();

                var resp = new
                {
                    success = true,
                    datos = contratos
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

        [Authorize(Roles = "COORDINADOR, ADMIN, COTIZADOR, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult VerificarEstadoProximaActividad(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("Error, no se encontró el ticket.");
                }

                bool concuerda = db.EstadoTicketProximaActividad.Where(x => x.SecuencialEstadoTicket == ticket.SecuencialEstadoTicket && x.SecuencialProximaActividad == ticket.SecuencialProximaActividad).Count() > 0;

                if (!concuerda)
                {

                    var proximasActividades = (from pa in db.ProximaActividad
                                               join etpa in db.EstadoTicketProximaActividad on pa.Secuencial equals etpa.SecuencialProximaActividad
                                               where pa.EstaActivo == 1 && etpa.EstaActivo == 1 && etpa.SecuencialEstadoTicket == ticket.SecuencialEstadoTicket
                                               select new
                                               {
                                                   id = pa.Secuencial,
                                                   nombre = pa.Codigo
                                               }).ToList();

                    var resp1 = new
                    {
                        success = true,
                        concuerda = false,
                        mensaje = "El ticket no puede estar en estado: " + ticket.estadoTicket.Codigo + ", con próxima actividad: " + ticket.proximaActividad.Codigo + ".",
                        proximasActividades = proximasActividades
                    };
                    return Json(resp1);
                }

                var resp = new
                {
                    success = true,
                    concuerda = concuerda
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

        [Authorize(Roles = "COORDINADOR, ADMIN, COTIZADOR, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult CambiarProximaActividad(int idTicket, int idProximaActividad)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("Error, no se encontró el ticket.");
                }

                bool concuerda = db.EstadoTicketProximaActividad.Where(x => x.SecuencialEstadoTicket == ticket.SecuencialEstadoTicket && x.SecuencialProximaActividad == idProximaActividad).Count() > 0;

                if (!concuerda)
                {
                    throw new Exception("La próxima actividad no es válida de acuerdo al flujo definido.");
                }

                ticket.SecuencialProximaActividad = idProximaActividad;

                //Actualizando el histórico
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
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
                    usuario = user,
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

                db.SaveChanges();

                var resp = new
                {
                    //proximaActividad = ticket.SecuencialProximaActividad,
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET")]
        [HttpGet]
        public ActionResult DarModulosFuncionalidadTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encuentra en el sistema");
                }

                List<int> idModulos = null;
                List<object> funcionalidadesContrato = null;
                List<object> modulos = null;

                if (ticket.categoriaTicket.Codigo == "REQUERIMIENTO NUEVO")
                {
                    var funcionalidadesContrato1 = (from fc in db.Funcionalidad
                                                    where fc.EstaActiva == 1 && fc.modulo.EstaActivo == 1
                                                    select new
                                                    {
                                                        id = fc.Secuencial,
                                                        idModulo = fc.SecuencialModulo,
                                                        codigo = fc.Codigo,
                                                        name = fc.Descripcion
                                                    }).ToList();

                    idModulos = (from fx in funcionalidadesContrato1
                                 select fx.idModulo).Distinct().ToList<int>();

                    modulos = (from m in db.Modulo
                               where idModulos.Contains(m.Secuencial)
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

                    funcionalidadesContrato = funcionalidadesContrato1.ToList<object>();
                }
                else
                {
                    var funcionalidadesContrato2 = (from fc in db.Funcionalidad
                                                    join
                                                         cdfunc in db.ContratoDesarrolloFunc on fc.Secuencial equals cdfunc.SecuencialFuncionalidad
                                                    join
                                                         ccli in db.ContratoCliente on cdfunc.SecuencialContratoCliente equals ccli.Secuencial
                                                    where ccli.SecuencialCliente == ticket.persona_cliente.SecuencialCliente &&
                                                         fc.EstaActiva == 1 && fc.modulo.EstaActivo == 1 && ccli.FechaInicio <= DateTime.Today && ccli.FechaFin >= DateTime.Today
                                                    select new
                                                    {
                                                        id = fc.Secuencial,
                                                        idModulo = fc.SecuencialModulo,
                                                        codigo = fc.Codigo,
                                                        name = fc.Descripcion
                                                    }).ToList();

                    idModulos = (from fx in funcionalidadesContrato2
                                 select fx.idModulo).Distinct().ToList<int>();

                    modulos = (from m in db.Modulo
                               where idModulos.Contains(m.Secuencial)
                               select new
                               {
                                   id = m.Secuencial,
                                   type = "group",
                                   name = m.Codigo + " ( " + m.Descripcion + " )",
                                   editable = false,
                                   children = (from f in db.Funcionalidad
                                               join cdfunc in db.ContratoDesarrolloFunc on f.Secuencial equals cdfunc.SecuencialFuncionalidad
                                               join ccli in db.ContratoCliente on cdfunc.SecuencialContratoCliente equals ccli.Secuencial
                                               where ccli.SecuencialCliente == ticket.persona_cliente.SecuencialCliente &&
                                                    f.EstaActiva == 1 && f.modulo.EstaActivo == 1 && ccli.FechaInicio <= DateTime.Today && ccli.FechaFin >= DateTime.Today &&
                                                    f.SecuencialModulo == m.Secuencial
                                               select f.Secuencial).ToList()
                               }).ToList<object>();

                    funcionalidadesContrato = funcionalidadesContrato2.ToList<object>();
                }

                var allSystem = new
                {
                    id = 0,
                    name = "Todo el Sistema",
                    editable = false,
                    children = new object[0],
                    subGroups = (from m in db.Modulo
                                 where idModulos.Contains(m.Secuencial)
                                 select m.Secuencial).ToList()
                };

                modulos.Insert(0, allSystem);

                var resp = new
                {
                    modulos = modulos,
                    funcionalidades = funcionalidadesContrato
                };
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarProximaActividadSegunEstadoTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                    throw new Exception("No se encontró el ticket.");
                var proximasActividades = (from pa in db.ProximaActividad
                                           join etpa in db.EstadoTicketProximaActividad on pa.Secuencial equals etpa.SecuencialProximaActividad
                                           where etpa.SecuencialEstadoTicket == ticket.SecuencialEstadoTicket && etpa.EstaActivo == 1 && pa.EstaActivo == 1
                                           select new
                                           {
                                               id = pa.Secuencial,
                                               nombre = pa.Descripcion
                                           }).ToList();

                var resp = new
                {
                    success = true,
                    proximasActividades = proximasActividades
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

        //CALENDARIO DE LAS ASIGNACIONES DEL COLABORADOR
        [Authorize(Roles = "COORDINADOR, ADMIN, COTIZADOR, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarAsignacionesColaborador(int idColaborador, int addMes = 0)
        {
            try
            {
                DateTime hoy = DateTime.Today;
                int diaMes = hoy.Day;
                DateTime diaInicioMes = hoy.AddDays((diaMes - 1) * (-1));
                if (addMes != 0)
                {
                    diaInicioMes = diaInicioMes.AddMonths(addMes);
                }
                DateTime proximoMes = diaInicioMes.AddMonths(1);

                var datosAsignaciones = (from tar in db.Tarea
                                         join colab in db.Colaborador on tar.colaborador equals colab
                                         where colab.Secuencial == idColaborador &&
                                               tar.estadoTarea.Secuencial != 4 &&
                                               tar.FechaInicio > diaInicioMes && tar.FechaInicio < proximoMes
                                         orderby tar.FechaInicio ascending
                                         select new
                                         {
                                             cliente = tar.cliente.Codigo,
                                             fechaInicio = tar.FechaInicio,
                                             fechaFin = tar.FechaFin,
                                             clase = (tar.SecuencialEstadoTarea == 1 ? "new" : (tar.SecuencialEstadoTarea == 2) ? "dev" : "finish")
                                         }).ToList();

                List<object> semanas = new List<object>();
                DateTime dia = diaInicioMes;
                while (dia.Month == diaInicioMes.Month)
                {
                    int i = (int)dia.DayOfWeek - 1;
                    if (i < 0)
                    {
                        i = 6;
                    }
                    List<object> arregloDias = new List<object>();

                    while (i < 7)
                    {
                        if (dia.Day == 1)
                        {//Aqui es el primer día, rellenar a la izquierda
                            for (int j = 0; j < i; j++)
                            {
                                var obj = new
                                {
                                    dia = "-",
                                    fecha = "-",
                                    asignaciones = new List<object>(),
                                    horasDisponibles = "-",
                                    mostrar = "opacar"
                                };
                                arregloDias.Add(obj);
                            }
                        }

                        var asignacionesFecha = (from da in datosAsignaciones
                                                 where da.fechaInicio > dia && da.fechaInicio < dia.AddDays(1)
                                                 orderby da.fechaInicio ascending
                                                 select da).ToList();


                        List<object> asignaciones = new List<object>();
                        int horasDisponibles = 8;
                        foreach (var asig in asignacionesFecha)
                        {
                            TimeSpan tiempoTarea = asig.fechaFin - asig.fechaInicio;
                            int horas = tiempoTarea.Hours;

                            if (asig.fechaInicio.Hour < 13 && asig.fechaFin.Hour > 13)
                            {
                                horas--;
                            }

                            var asignacion = new
                            {
                                clase = asig.clase,
                                cliente = asig.cliente,
                                horas = horas
                            };
                            horasDisponibles -= horas;
                            asignaciones.Add(asignacion);
                        }

                        string clase = "no-opacar";
                        if ((int)dia.DayOfWeek == 0 || (int)dia.DayOfWeek == 6)
                        {
                            clase = "fin-semana";
                        }

                        if (dia.Date == DateTime.Now.Date)
                        {
                            clase += " dia-hoy bordear";
                        }

                        var obji = new
                        {
                            dia = dia.Day,
                            fecha = dia.ToString("dd/MM/yyyy"),
                            asignaciones = asignaciones,
                            horasDisponibles = horasDisponibles,
                            mostrar = clase
                        };
                        arregloDias.Add(obji);

                        dia = dia.AddDays(1);
                        i++;

                        if (dia.Month != diaInicioMes.Month)
                        {
                            break;
                        }
                    }
                    semanas.Add(arregloDias);
                }

                var resp = new
                {
                    success = true,
                    mesCalendar = Utiles.DarNombreMes(diaInicioMes.Month) + " / " + diaInicioMes.Year.ToString(),
                    semanasAsignaciones = semanas
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

        //GUARDAR INFORMACION GENERAL DEL TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult GuardarTicket(int idTicket, int prioridad, int categoria, int actividadProxima,
            int tipoRecurso, int ticketVC, int reputacion, int estimacion, bool seFactura, bool facturado,
            int reprocesos, bool ingresoInterno, string fechaRevisado, int colaborador = 0, string observaciones = "",
            bool error = false, bool requiereTesting = false, bool pendienteAPago = false,
            HttpPostedFileBase[] adjuntos = null, bool esEstimacion = false, int diasGarantia = 30,
            bool errorInfraestructura = false, bool revisado = false)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se pudo encontrar el ticket.");
                }

                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Colaborador col = user.persona.colaborador.FirstOrDefault();

                ticket.Reputacion = reputacion;
                ticket.SecuencialProximaActividad = actividadProxima;

                ticket.SecuencialTipoRecurso = tipoRecurso;
                if (ticketVC != 0)
                    ticket.SecuencialTicketVersionCliente = ticketVC;
                ticket.Estimacion = estimacion;

                ticket.IngresoInterno = ingresoInterno;
                ticket.Reprocesos = reprocesos;
                ticket.SeFactura = seFactura;
                ticket.ErrorInfraestructura = errorInfraestructura;
                ticket.Revisado = revisado;
                ticket.Facturado = facturado;
                ticket.DiasGarantia = diasGarantia;
                ticket.PendienteAPago = pendienteAPago;
                ticket.RequiereTesting = requiereTesting;
                if (fechaRevisado != "")
                {
                    string[] fechas = fechaRevisado.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    DateTime fRevisado = new System.DateTime(anno, mes, dia);
                    ticket.FechaRevisado = fRevisado;
                }
                //Actualizando el histórico

                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    estadoTicket = ticket.estadoTicket,
                    SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
                    SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
                    SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
                    SecuencialProximaActividad = ticket.SecuencialProximaActividad,
                    SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
                    ReportadoPor = ticket.ReportadoPor,
                    Reputacion = ticket.Reputacion,
                    usuario = user,
                    Telefono = ticket.Telefono,
                    Asunto = ticket.Asunto,
                    Detalle = ticket.Detalle,
                    FechaCreado = ticket.FechaCreado,
                    Estimacion = ticket.Estimacion,
                    IngresoInterno = ticket.IngresoInterno,
                    Reprocesos = ticket.Reprocesos,
                    SeFactura = ticket.SeFactura,
                    Facturado = ticket.Facturado,
                    NumeroVerificador = 1,
                    FechaOperacion = DateTime.Now,
                    RequiereTesting = requiereTesting
                };
                if (fechaRevisado != "")
                {
                    ticketHistorico.FechaRevisado = ticket.FechaRevisado;
                }
                if (categoria != 0)
                {
                    ticketHistorico.SecuencialCategoriaRevisada = categoria;
                }
                if (prioridad != 0)
                {
                    ticketHistorico.SecuencialPrioridadRevisada = prioridad;
                }
                db.TicketHistorico.Add(ticketHistorico);

                if (colaborador != 0)
                {
                    //Busco si existe el Tecnico asociado a ese ticket
                    Ticket_Resolucion trs = db.Ticket_Resolucion.Find(idTicket);
                    if (trs == null)
                    {
                        trs = new Ticket_Resolucion();
                        trs.SecuencialColaborador = colaborador;
                        trs.Descripcion = observaciones;
                        trs.FechaHora = DateTime.Now;
                        trs.NumeroVerificador = 1;
                        trs.SecuencialTicket = idTicket;
                        db.Ticket_Resolucion.Add(trs);
                    }
                    else
                    {
                        trs.SecuencialColaborador = colaborador;
                        trs.FechaHora = DateTime.Now;
                        trs.Descripcion = observaciones;
                        trs.NumeroVerificador = 1;
                    }
                }

                if (categoria != 0)
                {
                    Ticket_CategoriaRevisada ctr = db.Ticket_CategoriaRevisada.Find(idTicket);
                    if (ctr == null)
                    {
                        ctr = new Ticket_CategoriaRevisada();
                        ctr.SecuencialColaborador = user.persona.colaborador.FirstOrDefault().Secuencial;
                        ctr.NumeroVerificador = 1;
                        ctr.SecuencialCategoriaTicket = categoria;
                        ctr.SecuencialTicket = idTicket;
                        db.Ticket_CategoriaRevisada.Add(ctr);
                    }
                    else
                    {
                        ctr.SecuencialColaborador = user.persona.colaborador.FirstOrDefault().Secuencial;
                        ctr.NumeroVerificador = 1;
                        ctr.SecuencialCategoriaTicket = categoria;
                        //ctr.SecuencialTicket = idTicket;
                    }
                }
                else
                {
                    throw new Exception("Debe seleccionar una categoría revisada.");
                }

                if (prioridad != 0)
                {
                    Ticket_PrioridadRevisada prr = db.Ticket_PrioridadRevisada.Find(idTicket);
                    if (prr == null)
                    {
                        prr = new Ticket_PrioridadRevisada();
                        prr.SecuencialColaborador = user.persona.colaborador.FirstOrDefault().Secuencial;
                        prr.SecuencialTicket = idTicket;
                        prr.SecuencialPrioridadTicket = prioridad;
                        prr.NumeroVerificador = 1;
                        db.Ticket_PrioridadRevisada.Add(prr);
                    }
                    else
                    {

                        prr.SecuencialColaborador = user.persona.colaborador.FirstOrDefault().Secuencial;
                        //prr.SecuencialTicket = idTicket;
                        prr.SecuencialPrioridadTicket = prioridad;
                        prr.NumeroVerificador = 1;
                    }
                }

                Ticket_Error err = db.Ticket_Error.Find(idTicket);
                if (err == null)
                {
                    err = new Ticket_Error();
                    err.SecuencialColaborador = user.persona.colaborador.FirstOrDefault().Secuencial;
                    err.SecuencialTicket = idTicket;
                    err.NumeroVerificador = 1;
                    err.Error = error;
                    db.Ticket_Error.Add(err);
                }
                else
                {
                    err.SecuencialColaborador = user.persona.colaborador.FirstOrDefault().Secuencial;
                    err.SecuencialTicket = idTicket;
                    err.NumeroVerificador = 1;
                    err.Error = error;
                }

                //Por los ficheros adjuntos
                if (adjuntos != null)
                {
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string newNameFile = "";
                            if (esEstimacion == true)
                            {
                                string extFile = Path.GetExtension(adj.FileName);
                                newNameFile = "est_" + Utiles.RandomString(10) + ".xlsx";
                                string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                                adj.SaveAs(path);
                            }
                            else
                            {
                                string extFile = Path.GetExtension(adj.FileName);
                                newNameFile = Utiles.RandomString(10) + extFile;
                                string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                                adj.SaveAs(path);
                            }

                            AdjuntoTicket adjTicket = new AdjuntoTicket
                            {
                                Url = "/resources/tickets/" + newNameFile,
                                ticket = ticket
                            };

                            HistoricoAdjunto histAdj = new HistoricoAdjunto
                            {
                                SecuencialHistoInforTicket = ticket.Secuencial,
                                Url = "/resources/tickets/" + newNameFile,
                            };

                            db.AdjuntoTicket.Add(adjTicket);
                            db.HistoricoAdjunto.Add(histAdj);
                        }
                    }
                }

                //Despues de que se agreguen todos los adjuntos por si acaso se agrega uno nuevo
                //Cuando el ticket se guarde con proxima activa "COTIZAR"
                var pa = db.ProximaActividad.Find(actividadProxima).Codigo;
                if (pa == "COTIZAR")
                {
                    Ofertas oferta = new Ofertas();
                    oferta.cliente = ticket.persona_cliente.cliente;
                    oferta.colaborador = col;
                    oferta.Detalle = "Estimación del ticket: " + ticket.Secuencial +" - "+ ticket.Asunto;
                    oferta.HorasEstimacion = ticket.Estimacion;
                    oferta.FechaDisponibilidad = new DateTime(0001 / 01 / 01);
                    oferta.FechaProduccion = new DateTime(0001 / 01 / 01);
                    oferta.FechaRegistro = DateTime.Now;

                    //Agregar el ultimo adjunot del ticket a la oferta
                    var lastAdj = db.AdjuntoTicket.Where(adj => adj.SecuencialTicket == ticket.Secuencial && adj.Url.Contains("est_"))
                        .OrderByDescending(adj => adj.Secuencial).FirstOrDefault();
                    if (lastAdj != null)
                    {
                        oferta.Adjunto = lastAdj.Url;
                    }

                    db.Ofertas.Add(oferta);
                }

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

        //ABRIR UN TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult AbrirTicket(int idTicket, int reputacion)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se pudo encontrar el ticket.");
                }

                ticket.Reputacion = reputacion;

                //pasando el ticket a abierto
                EstadoTicket estadoTicket = db.EstadoTicket.Where(x => x.Codigo == "ABIERTO").FirstOrDefault();
                ticket.estadoTicket = estadoTicket;

                //Actualizando el histórico
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
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
                    usuario = user,
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

        //ESTIMAR EL TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN")]
        [HttpPost]
        public ActionResult OrdenarEstimarTicket(int idTicket, string idColaboradores, string fechaLimite, string prioridad, string categoria, int reputacion)
        {
            string[] fechas = fechaLimite.Split(new Char[] { '/' });
            int dia = Int32.Parse(fechas[0]);
            int mes = Int32.Parse(fechas[1]);
            int anno = Int32.Parse(fechas[2]);
            DateTime fecha = new System.DateTime(anno, mes, dia);

            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }

                var s = new JavaScriptSerializer();
                var idColaboradoresArray = s.Deserialize<dynamic>(idColaboradores);
                List<string> list = new List<string>();

                foreach (var idColab in idColaboradoresArray)
                {
                    Colaborador colab = db.Colaborador.Find(int.Parse(idColab));
                    if (colab == null)
                    {
                        throw new Exception("No se encontró un colaborador");
                    }

                    EstimacionTicket estimacionTicket = new EstimacionTicket
                    {
                        ticket = ticket,
                        colaborador = colab,
                        NumeroHoras = 0,
                        FactorTiempo = 0,
                        EntregablesAdicionales = " ",
                        InformacionComplementaria = " ",
                        EstimacionTerminada = 0,
                        FechaLimite = fecha,
                        EstaActiva = 1
                    };
                    db.EstimacionTicket.Add(estimacionTicket);

                    //envío de correo a los especialistas
                    Persona persona = colab.persona;
                    string saludo = "Estimado " + Utiles.UpperCamelCase(persona.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(persona.Apellido1.ToLower());
                    if (persona.Sexo == "F")
                    {
                        saludo = "Estimada " + Utiles.UpperCamelCase(persona.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(persona.Apellido1.ToLower());
                    }
                    string htmlMail = "<div class=\"textoCuerpo\">" + saludo + ",<br/>Se ha pedido su colaboración, para la estimación del ticket: " + String.Format("{0:000000}", ticket.Secuencial) + "<br/>";
                    htmlMail += "Por favor ingresar al sistema Sifizplanning en el módulo de Desarrolladores y realizar la estimación.<br/> Fecha límite para realizar la estimación:" + fecha.ToString("dd/MM/yyyy");
                    htmlMail += "<br><br>Detalle del Ticket: <br>" + ticket.Detalle + "</div>";

                    string asunto = ticket.persona_cliente.cliente.Codigo + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - Nueva estimación de ticket (" + ticket.Asunto + ")";
                    Usuario userColaborador = persona.usuario.FirstOrDefault();

                    List<string> usuariosDestinos = new List<string>();
                    usuariosDestinos.Add(userColaborador.Email);

                    var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                    foreach (var g in gestores)
                    {
                        usuariosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                    }
                    usuariosDestinos = usuariosDestinos.Distinct().ToList();

                    List<string> listaPathFicheros = new List<string>();
                    var adjuntos = db.AdjuntoTicket.Where(t => t.SecuencialTicket == idTicket).ToList();
                    foreach (var item in adjuntos)
                    {
                        string path = Server.MapPath("~/Web/" + item.Url);
                        listaPathFicheros.Add(path);
                    }
                    string formato = "FORMATO ESTIMACION.xlsx";
                    string pathFormato = Path.Combine(Server.MapPath("~/Web/resources/tickets/"), formato);
                    listaPathFicheros.Add(pathFormato);

                    Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), htmlMail, asunto, listaPathFicheros.ToArray(), String.Format("{0:000000}", ticket.Secuencial));
                    list.Add(htmlMail);
                }

                //Actualizando los datos del ticket
                int secuencialProximaActividad = db.ProximaActividad.Where(x => x.Codigo == "ESTIMAR TECNICO" && x.EstaActivo == 1).FirstOrDefault().Secuencial;
                int secuencialPrioridad = db.PrioridadTicket.Where(x => x.Codigo == prioridad && x.EstaActiva == 1).FirstOrDefault().Secuencial;
                int secuencialCategoria = db.CategoriaTicket.Where(x => x.Codigo == categoria && x.EstaActiva == 1).FirstOrDefault().Secuencial;
                ticket.Reputacion = reputacion;
                ticket.SecuencialPrioridadTicket = secuencialPrioridad;
                ticket.SecuencialCategoriaTicket = secuencialCategoria;
                ticket.SecuencialProximaActividad = secuencialProximaActividad;

                //Actualizando el histórico del ticket. 
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);

                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == idTicket).Count();
                TicketHistorico ticketHistorico = new TicketHistorico
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
                    usuario = user,
                    Telefono = ticket.Telefono,
                    Asunto = ticket.Asunto,
                    Detalle = ticket.Detalle,
                    FechaCreado = ticket.FechaCreado,
                    Estimacion = 0,
                    NumeroVerificador = 1,
                    FechaOperacion = DateTime.Now,
                    SeFactura = ticket.SeFactura,
                    Facturado = ticket.Facturado,
                    IngresoInterno = ticket.IngresoInterno,
                    Reprocesos = ticket.Reprocesos
                };
                db.TicketHistorico.Add(ticketHistorico);

                foreach (var l in list)
                {
                    HistoricoInformacionTicket historicoCorreo = new HistoricoInformacionTicket
                    {
                        SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
                        VersionTicketHistorico = ticketHistorico.Version,
                        Fecha = DateTime.Now,
                        Texto = l
                    };
                    db.HistoricoInformacionTicket.Add(historicoCorreo);
                }

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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarDatosEstimacionTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("Error, no se encontró el ticket");
                }

                var estimaciones = (from et in db.EstimacionTicket
                                    join
                                        colab in db.Colaborador on et.colaborador equals colab
                                    join
                                        per in db.Persona on colab.SecuencialPersona equals per.Secuencial
                                    where et.SecuencialTicket == idTicket && et.EstaActiva == 1
                                    select new
                                    {
                                        id = et.Secuencial,
                                        colaborador = per.Nombre1 + " " + per.Apellido1,
                                        terminada = et.EstimacionTerminada == 1 ? "SI" : "NO",
                                        tiempo = et.NumeroHoras,
                                        catalogada = et.FactorTiempo == 0 ? "SIN CATALOGAR" : et.FactorTiempo == 4 ? "PROBABLE" : et.FactorTiempo == 1 ? "OPTIMISTA" : "PESIMISTA"
                                    }).ToList();

                var resp = new
                {
                    success = true,
                    estimaciones = estimaciones
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

        //ESTIMACIONES mio
        [HttpPost]
        [Authorize(Roles = "USER, ADMIN")]
        public ActionResult Estimaciones(int start, int lenght, string filtro = "", bool todos = false)
        {
            try
            {
                var s = new JavaScriptSerializer();
                var jsonObj = s.Deserialize<dynamic>(filtro);

                string filtroNumero = jsonObj["numero"];
                string filtroColaborador = jsonObj["colaborador"];
                string filtroCliente = jsonObj["cliente"];
                string filtroPrioridad = jsonObj["prioridad"];
                string filtroFechaLimite = jsonObj["fechaLimite"];
                string filtroDetalle = jsonObj["detalle"];
                string filtroEstadoEstimacion = jsonObj["estadoEstimacion"];

                var estimaciones = (from est in db.EstimacionTicket
                                    where est.ticket.estadoTicket.Codigo == "PENDIENTE"
                                    orderby est.FechaLimite descending
                                    select new
                                    {
                                        id = est.Secuencial,
                                        numero = est.SecuencialTicket,
                                        colaborador = est.colaborador.persona.Nombre1 + " " + est.colaborador.persona.Apellido1,
                                        cliente = est.ticket.persona_cliente.cliente.Descripcion,
                                        prioridad = est.ticket.prioridadTicket.Codigo,
                                        asunto = est.ticket.Asunto,
                                        detalle = est.ticket.Detalle,
                                        fechaLimite = est.FechaLimite,
                                        estado = (est.ticket.proximaActividad.Descripcion == "COTIZAR") ? "ESTIMADO" :
                                                 (est.ticket.proximaActividad.Descripcion == "ESTIMAR TECNICO" || est.ticket.proximaActividad.Descripcion == "ESTIMAR") ? "POR ESTIMAR" :
                                                 (est.ticket.proximaActividad.Descripcion == "VALIDAR ESTIMACION") ? "VALIDANDO" :
                                                 (est.ticket.proximaActividad.Descripcion != "COTIZAR" || est.ticket.proximaActividad.Descripcion != "ESTIMAR TECNICO" || est.ticket.proximaActividad.Descripcion != "VALIDAR ESTIMACION")
                                                 ? (est.EstimacionTerminada == 1) ? "ESTIMADO" : "POR ESTIMAR" : "",
                                        proximaActividad = est.ticket.proximaActividad.Descripcion,
                                        adjuntos = (from adj in db.AdjuntoTicket
                                                    where adj.SecuencialTicket == est.SecuencialTicket
                                                    select new
                                                    {
                                                        id = adj.Secuencial,
                                                        url = adj.Url
                                                    }).ToList()
                                    }).ToList();

                if (todos == false)
                {
                    estimaciones = (from d in estimaciones
                                    where d.estado == "POR ESTIMAR" || d.estado == "VALIDANDO"
                                    select d).ToList();
                }

                if (filtroNumero != "")
                {
                    estimaciones = (from d in estimaciones
                                    where d.numero.ToString().ToUpper().Contains(filtroNumero.ToUpper())
                                    select d).ToList();
                }

                if (filtroColaborador != "")
                {
                    estimaciones = (from d in estimaciones
                                    where d.colaborador.ToString().ToUpper().Contains(filtroColaborador.ToUpper())
                                    select d).ToList();
                }

                if (filtroCliente != "")
                {
                    estimaciones = (from d in estimaciones
                                    where d.cliente.ToString().ToUpper().Contains(filtroCliente.ToUpper())
                                    select d).ToList();
                }
                if (filtroPrioridad != "")
                {
                    estimaciones = (from d in estimaciones
                                    where d.prioridad.ToString().ToUpper().Contains(filtroPrioridad.ToUpper())
                                    select d).ToList();
                }
                if (filtroFechaLimite != "")
                {
                    estimaciones = (from d in estimaciones
                                    where d.fechaLimite.ToString("dd/MM/yyyy").Contains(filtroFechaLimite)
                                    select d).ToList();
                }
                if (filtroDetalle != "")
                {
                    estimaciones = (from d in estimaciones
                                    where d.detalle.ToString().ToUpper().Contains(filtroDetalle.ToUpper())
                                    select d).ToList();
                }

                if (filtroEstadoEstimacion != "")
                {
                    estimaciones = (from d in estimaciones
                                    where d.estado.ToString().ToUpper().Contains(filtroEstadoEstimacion.ToUpper())
                                    select d).ToList();
                }

                int total = estimaciones.Count();
                estimaciones = estimaciones.Skip(start).Take(lenght).ToList();

                var result = new
                {
                    success = true,
                    total = total,
                    estimaciones = estimaciones
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


        //ESTIMACIONES mio
        [HttpPost]
        [Authorize(Roles = "USER, ADMIN")]
        public ActionResult DarEstimaciones(int start, int lenght, bool todos = false)
        {
            try
            {

                var estimaciones = (from est in db.EstimacionTicket
                                    orderby est.ticket.Secuencial descending
                                    select new
                                    {
                                        id = est.Secuencial,
                                        numero = est.SecuencialTicket,
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

                int total = estimaciones.Count();
                estimaciones = estimaciones.Skip(start).Take(lenght).ToList();

                var result = new
                {
                    success = true,
                    total = total,
                    estimaciones = estimaciones
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DatosEstimacionUsuario(int idEstimacion)
        {
            try
            {
                EstimacionTicket estimacionTicket = db.EstimacionTicket.Find(idEstimacion);
                if (estimacionTicket != null)
                {
                    var detallesEstimacion = (from de in db.DetalleEstimacionTicket
                                              where de.SecuencialEstimacionTicket == idEstimacion
                                              select new
                                              {
                                                  detalle = de.Detalle,
                                                  tiempoEstimacion = de.TiempoEstimacion,
                                                  tiempoDesarrollo = de.TiempoDesarrollo,
                                                  tiempoPrueba = de.TiempoPrueba,
                                                  tiempoQA = de.TiempoQA,
                                                  nivel = de.SecuencialNivelColaborador
                                              }).ToList();
                    var resp = new
                    {
                        success = true,
                        detallesEstimacion = detallesEstimacion,
                        entregables = estimacionTicket.EntregablesAdicionales,
                        informacion = estimacionTicket.InformacionComplementaria,
                        estimacionTerminada = estimacionTicket.EstimacionTerminada,
                        idEstimacion = idEstimacion
                    };
                    return Json(resp);
                }
                else
                {
                    var resp = new
                    {
                        success = true,
                        detallesEstimacion = new List<object>(),
                        entregables = "",
                        informacion = "",
                        estimacionTerminada = 0
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        /**
         * catalogar si es -1 es pesimista,
         *           si es  1 es optimista,
         *           si es  4 es probable
         */
        public ActionResult CatalogarEstimacionUsuario(int idEstimacion, int catalogar)
        {
            try
            {
                EstimacionTicket et = db.EstimacionTicket.Find(idEstimacion);
                if (et == null)
                {
                    throw new Exception("No se encontró la estimación del usuario");
                }
                if (catalogar != -1 && catalogar != 1 && catalogar != 4)
                {
                    throw new Exception("No se reconoce el código de la catalogación de la estimación");
                }
                if (et.EstimacionTerminada == 0)
                {
                    throw new Exception("La estimación debe estar terminada para poder catalogarla");
                }

                //Buscando las estimaciones que están catalogadas de igual manera
                int secuencialTicket = et.SecuencialTicket;
                var estimaciones = db.EstimacionTicket.Where(x => x.SecuencialTicket == secuencialTicket && x.Secuencial != idEstimacion).ToList();

                foreach (EstimacionTicket estimacion in estimaciones)
                {
                    if (estimacion.FactorTiempo == catalogar)
                    {
                        estimacion.FactorTiempo = 0;
                    }
                }

                et.FactorTiempo = catalogar;
                db.SaveChanges();

                int total = 0;
                int totalFactor = 0;
                //Calculando el tiempo total de la estimación
                estimaciones = db.EstimacionTicket.Where(x => x.SecuencialTicket == secuencialTicket && x.EstaActiva == 1).ToList();
                foreach (EstimacionTicket estimacion in estimaciones)
                {
                    int factor = (int)(estimacion.FactorTiempo);
                    if (factor < 0)
                    {
                        factor *= -1;
                    }
                    totalFactor += factor;
                    total += (int)estimacion.NumeroHoras * factor;
                }

                double totalEstimacionExacta = total / totalFactor;
                int totalEstimacion = (int)Math.Ceiling(totalEstimacionExacta);

                var resp = new
                {
                    success = true,
                    totalEstimacion = totalEstimacion
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult TerminarEstimacionTicket(int idTicket, int estimacion)
        {
            try
            {
                //Cambiar el estado del ticket a estimacion terminada
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("Error, no se encontró en el ticket");
                }
                EstadoTicket estadoTicket = db.EstadoTicket.Where(x => x.Codigo == "ESTIMACION TERMINADA").FirstOrDefault();
                if (estadoTicket == null)
                {
                    throw new Exception("No se encontró el estado de ESTIMACIÓN TERMINADA.");
                }
                //Buscando si tiene estimaciones por terminar
                var cantEstimacionesSinTerminar = db.EstimacionTicket.Where(x => x.SecuencialTicket == idTicket && x.EstimacionTerminada == 0 && x.EstaActiva == 1).Count();
                if (cantEstimacionesSinTerminar > 0)
                {
                    throw new Exception("No se han terminado todas las estimaciones del ticket.");
                }

                int secuencialProximaActividad = db.ProximaActividad.Where(x => x.Codigo == "COTIZAR" && x.EstaActivo == 1).FirstOrDefault().Secuencial;

                ticket.Estimacion = estimacion;
                ticket.estadoTicket = estadoTicket;
                ticket.SecuencialProximaActividad = secuencialProximaActividad;

                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Colaborador colaborador = user.persona.colaborador.FirstOrDefault();

                Ofertas oferta = new Ofertas();
                oferta.cliente = ticket.persona_cliente.cliente;
                oferta.colaborador = colaborador;
                oferta.Detalle = "Estimación del ticket: " + ticket.Secuencial;
                oferta.HorasEstimacion = estimacion;
                oferta.FechaDisponibilidad = new DateTime(0001 / 01 / 01);
                oferta.FechaProduccion = new DateTime(0001 / 01 / 01);
                oferta.FechaRegistro = DateTime.Now;
                db.Ofertas.Add(oferta);

                //Adicionando el histórico del ticket
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    estadoTicket = estadoTicket,
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
                    Estimacion = estimacion,
                    NumeroVerificador = 1,
                    FechaOperacion = DateTime.Now,
                    SeFactura = ticket.SeFactura,
                    Facturado = ticket.Facturado,
                    IngresoInterno = ticket.IngresoInterno,
                    Reprocesos = ticket.Reprocesos
                };
                db.TicketHistorico.Add(ticketHistorico);

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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult AceptarEstimacionTicket(int idEstimacion)
        {
            try
            {
                EstimacionTicket estimacionTicket = db.EstimacionTicket.Find(idEstimacion);

                Ticket ticket = estimacionTicket.ticket;
                ticket.Estimacion = (int)ticket.estimacionTicket.Sum(s => s.detalleEstimacionTicket.Sum(e => e.TiempoDesarrollo + e.TiempoPrueba).Value);
                EstadoTicket estadoTicket = db.EstadoTicket.Where(x => x.Codigo == "PENDIENTE" && x.EstaActivo == 1).FirstOrDefault();
                ticket.estadoTicket = estadoTicket;
                int secuencialProximaActividad = db.ProximaActividad.Where(x => x.Codigo == "COTIZAR" && x.EstaActivo == 1).FirstOrDefault().Secuencial;
                ticket.SecuencialProximaActividad = secuencialProximaActividad;

                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();
                numeroVersion--;
                TicketHistorico ticketHistorico = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial && x.Version == numeroVersion).FirstOrDefault();
                if (ticketHistorico == null)
                {
                    throw new Exception("No se encontró el versionamiento del ticket.");
                }
                numeroVersion++;

                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Colaborador colaborador = user.persona.colaborador.FirstOrDefault();

                Ofertas oferta = new Ofertas();
                oferta.cliente = ticket.persona_cliente.cliente;
                oferta.colaborador = colaborador;
                oferta.Detalle = "Estimación del ticket: " + ticket.Secuencial;
                oferta.HorasEstimacion = ticket.Estimacion;
                oferta.FechaDisponibilidad = new DateTime(0001 / 01 / 01);
                oferta.FechaProduccion = new DateTime(0001 / 01 / 01);
                oferta.FechaRegistro = DateTime.Now;
                db.Ofertas.Add(oferta);

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

                List<string> destinatarios = Utiles.CorreoPorGrupoEmail("COORD");
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
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

                string asunto = ticket.persona_cliente.cliente.Codigo + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - ESTIMACION COMPLETADA(" + ticket.Asunto + ")";

                string htmlMail = "<div class=\"textoCuerpo\">Estimado " + Utiles.UpperCamelCase(personaEmail.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(personaEmail.Apellido1.ToLower()) + ":<br>";
                if (personaEmail.Sexo == "F")
                {
                    htmlMail = "<div class=\"textoCuerpo\">Estimada " + Utiles.UpperCamelCase(personaEmail.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(personaEmail.Apellido1.ToLower()) + ":<br>";
                }

                htmlMail += "La estimación del ticket " + String.Format("{0:000000}", ticket.Secuencial) + @" perteneciente al cliente: " + ticket.persona_cliente.cliente.Descripcion +
                             @" ha sido validada, y está lista para cotizar. </div>";

                string correoCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault().Email;
                List<string> destinatarioCliente = new List<string>();
                destinatarioCliente.Add(correoCliente);

                Utiles.EnviarEmailSistema(destinatarioCliente.ToArray(), htmlMail, asunto, null, String.Format("{0:000000}", ticket.Secuencial));
                Utiles.EnviarEmailSistema(destinatarios.ToArray(), htmlMail, asunto, null, String.Format("{0:000000}", ticket.Secuencial));

                HistoricoInformacionTicket historicoCorreo = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = ticketHis.SecuencialTicket,
                    VersionTicketHistorico = ticketHis.Version,
                    Fecha = DateTime.Now,
                    Texto = htmlMail
                };
                db.HistoricoInformacionTicket.Add(historicoCorreo);
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult RechazarEstimacionesTicket(int numero, string comentario)
        {
            try
            {
                var estimaciones = db.EstimacionTicket.Where(s => s.SecuencialTicket == numero && s.EstaActiva == 1).ToList();
                foreach (var e in estimaciones)
                {
                    RechazarEstimacionTicket(e.Secuencial, comentario);
                }
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult RechazarEstimacionTicket(int idEstimacion, string comentario)
        {
            try
            {
                EstimacionTicket estimacionTicket = db.EstimacionTicket.Find(idEstimacion);

                estimacionTicket.EstimacionTerminada = 0;

                Ticket ticket = estimacionTicket.ticket;
                ticket.Estimacion = (int)ticket.estimacionTicket.Sum(s => s.detalleEstimacionTicket.Sum(e => e.TiempoDesarrollo + e.TiempoPrueba).Value);
                EstadoTicket estadoTicket = db.EstadoTicket.Where(x => x.Codigo == "PENDIENTE" && x.EstaActivo == 1).FirstOrDefault();
                ticket.estadoTicket = estadoTicket;
                int secuencialProximaActividad = db.ProximaActividad.Where(x => x.Codigo == "ESTIMAR TECNICO" && x.EstaActivo == 1).FirstOrDefault().Secuencial;
                ticket.SecuencialProximaActividad = secuencialProximaActividad;

                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();
                numeroVersion--;
                TicketHistorico ticketHistorico = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial && x.Version == numeroVersion).FirstOrDefault();
                if (ticketHistorico == null)
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
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    destinatarios.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                destinatarios.Add(usuarioEmail.Email);
                destinatarios.Add(user.Email);
                destinatarios = destinatarios.Distinct().ToList();
                destinatarios.Remove("gerencia@sifizsoft.com");

                string asunto = ticket.persona_cliente.cliente.Codigo + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - ESTIMACION RECHAZADA(" + ticket.Asunto + ")";

                string htmlMail = "<div class=\"textoCuerpo\">Estimado " + Utiles.UpperCamelCase(personaEmail.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(personaEmail.Apellido1.ToLower()) + ":<br>";
                if (personaEmail.Sexo == "F")
                {
                    htmlMail = "<div class=\"textoCuerpo\">Estimada " + Utiles.UpperCamelCase(personaEmail.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(personaEmail.Apellido1.ToLower()) + ":<br>";
                }

                htmlMail += "La estimación del ticket " + String.Format("{0:000000}", ticket.Secuencial) + @" perteneciente al cliente: " + ticket.persona_cliente.cliente.Descripcion +
                             @" ha sido rechazada, favor de estimar nuevamente, le hacemos el siguiente comentario: </div>" + comentario;

                Utiles.EnviarEmailSistema(destinatarios.ToArray(), htmlMail, asunto, null, String.Format("{0:000000}", ticket.Secuencial));

                HistoricoInformacionTicket historicoCorreo = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = ticketHis.SecuencialTicket,
                    VersionTicketHistorico = ticketHis.Version,
                    Fecha = DateTime.Now,
                    Texto = htmlMail
                };
                db.HistoricoInformacionTicket.Add(historicoCorreo);
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DetalleEstimacionTicket(int idTicket, string estado)
        {
            try
            {
                bool permitirValidar = false;
                var estimacionesParcial = (from est in db.EstimacionTicket
                                           where est.SecuencialTicket == idTicket && est.EstaActiva == 1
                                           orderby est.Secuencial
                                           select new
                                           {
                                               id = est.Secuencial,
                                               colaborador = est.colaborador.persona.Nombre1 + " " + est.colaborador.persona.Apellido1,
                                               numeroHoras = est.NumeroHoras,
                                               informacionComplementaria = est.InformacionComplementaria,
                                               entregablesAdicionales = est.EntregablesAdicionales,
                                               requerimientoNuevo = est.RequerimientoNuevo != null ? est.RequerimientoNuevo : true,
                                               tiempoInicial = est.TiempoInicial != null ? est.TiempoInicial : 0,
                                               tiempoPegado = est.TiempoPegado != null ? est.TiempoPegado : 0,
                                               entregables = (from de in db.DetalleEstimacionTicket
                                                              join et in db.EntregableDetalleEstimacion on de.SecuencialEntregableEstimacion equals et.Secuencial
                                                              where de.SecuencialEstimacionTicket == est.Secuencial
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
                                                                                  nivel = db.NivelColaborador.Where(s => s.Secuencial == d.SecuencialNivelColaborador).FirstOrDefault().Codigo.ToUpper(),
                                                                              }).ToList()
                                                              }).ToList(),
                                               itemsAdicionales = (from i in db.ItemEspecial
                                                                   join nc in db.NivelColaborador on i.SecuencialNivelColaborador equals nc.Secuencial
                                                                   where i.SecuencialEstimacion == est.Secuencial
                                                                   select new
                                                                   {
                                                                       descripcion = i.Descripcion,
                                                                       nivel = nc.Descripcion,
                                                                       tiempoEstimacion = i.TiempoEstimacion
                                                                   }).ToList(),
                                               itemsEstimacion = (from i in db.ItemEspecialEstimacion
                                                                  join nc in db.NivelColaborador on i.SecuencialNivelColaborador equals nc.Secuencial
                                                                  join iec in db.ItemEspecialCatalogo on i.SecuencialItemEspecialCatalogo equals iec.Secuencial
                                                                  where i.SecuencialEstimacion == est.Secuencial
                                                                  select new
                                                                  {
                                                                      descripcion = iec.Descripcion,
                                                                      nivel = nc.Descripcion,
                                                                      tiempoEstimacion = i.TiempoEstimacion
                                                                  }).ToList(),
                                           }).ToList();


                //calculando tiempo total
                var tiempoTotal = 0.0;

                foreach (var estimacion in estimacionesParcial)
                {
                    var entrega = estimacion.entregables;
                    foreach (var estima in entrega)
                    {
                        foreach (var est in estima.detalles)
                        {
                            tiempoTotal += est.tiempoDesarrollo + est.tiempoPrueba;
                        }
                    }
                }
                var estimacionesFinales = (from ep in estimacionesParcial
                                           select new
                                           {
                                               id = ep.id,
                                               colaborador = ep.colaborador,
                                               numeroHoras = ep.numeroHoras,
                                               informacionComplementaria = ep.informacionComplementaria,
                                               entregablesAdicionales = ep.entregablesAdicionales,
                                               entregables = ep.entregables,
                                               requerimientoNuevo = ep.requerimientoNuevo,
                                               tiempoInicial = ep.tiempoInicial,
                                               tiempoPegado = ep.tiempoPegado,
                                               colaboradorHoras = (from det in ep.entregables
                                                                   from detalle in det.detalles
                                                                   select detalle.tiempoDesarrollo + detalle.tiempoPrueba
                                                                   ).Sum(),
                                               itemsEstimacion = ep.itemsEstimacion,
                                               itemsAdicionales = ep.itemsAdicionales
                                           }).ToList();

                if (estimacionesParcial.Count() == 0)
                {
                    throw new Exception("El ticket no se ha enviado a estimar.");
                }
                var cantEstimacionesSinTerminar = db.EstimacionTicket.Where(x => x.SecuencialTicket == idTicket && x.EstimacionTerminada == 0 && x.EstaActiva == 1).Count();
                if (cantEstimacionesSinTerminar > 0)
                {
                    throw new Exception("No se han terminado todas las estimaciones del ticket.");
                }

                db.SaveChanges();

                //if (estado == "VALIDANDO")
                //{
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                if (user != null && (user.Email == "vhidalgo@sifizsoft.com" || user.Email == "rsanchez@sifizsoft.com"))
                    permitirValidar = true;
                //}

                var resp = new
                {
                    estimaciones = estimacionesFinales,
                    idTicketDetalle = idTicket,
                    tiempoTotal = tiempoTotal,
                    permitirValidar = permitirValidar,
                    success = true,
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
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR, USER")]
        public ActionResult DarItemsEspecialesTicket(int idEstimacion)
        {
            try
            {
                EstimacionTicket estimacion = db.EstimacionTicket.Find(idEstimacion);
                if (estimacion == null)
                {
                    throw new Exception("No se encontró la estimación.");
                }

                var itemsEspeciales = (from ie in db.ItemEspecial
                                       join nc in db.NivelColaborador on ie.SecuencialNivelColaborador equals nc.Secuencial
                                       where ie.SecuencialEstimacion == estimacion.Secuencial
                                       select new
                                       {
                                           id = ie.Secuencial,
                                           descripcion = ie.Descripcion,
                                           tiempoEstimacion = ie.TiempoEstimacion,
                                           idEstimacion = ie.SecuencialEstimacion,
                                           idNivelColab = ie.SecuencialNivelColaborador,
                                           nivel = nc.Descripcion
                                       }).ToList();

                var resp = new
                {
                    success = true,
                    items = itemsEspeciales,
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        //Función que retorna los cotizadores de los tickets
        public ActionResult DarCotizadoresTickets()
        {
            try
            {
                var cotizadores = (
                        from c in db.Colaborador
                        join
        p in db.Persona on c.persona equals p
                        join
        u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                        join
        ur in db.UsuarioRol on u.Secuencial equals ur.SecuencialUsuario
                        join
        rol in db.Rol on ur.SecuencialRol equals rol.Secuencial
                        where rol.Codigo == "COTIZADOR" && u.EstaActivo == 1
                        select new
                        {
                            idColaborador = c.Secuencial,
                            nombre = p.Nombre1 + " " + p.Apellido1
                        }).ToList();

                var resp = new
                {
                    success = true,
                    cotizadores = cotizadores
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult PedirCotizacionTickets(int idTicket, int idColaborador, int estimacion, int prioridad, int categoria, int reputacion)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("Error, no se encontró el ticket.");
                }
                EstadoTicket estadoTicket = db.EstadoTicket.Where(x => x.Codigo == "COTIZÁNDOSE").FirstOrDefault();
                if (estadoTicket == null)
                {
                    throw new Exception("Error,  no se encontró el estado COTIZÁNDOSE.");
                }
                Colaborador colaborador = db.Colaborador.Where(x => x.Secuencial == idColaborador).FirstOrDefault();
                if (colaborador == null)
                {
                    throw new Exception("Error,  no se encontró el cotizador.");
                }

                //Actualizando la estimación si no está definida
                if (ticket.Estimacion == 0)
                {
                    ticket.Estimacion = estimacion;
                }
                if (ticket.Estimacion == 0)
                {
                    throw new Exception("Error, el ticket no puede tener 0 horas de estimación.");
                }

                ticket.Reputacion = reputacion;
                ticket.SecuencialPrioridadTicket = prioridad;
                ticket.SecuencialCategoriaTicket = categoria;

                CotizacionTicket cotizacion = new CotizacionTicket
                {
                    SecuencialColaborador = idColaborador,
                    SecuencialTicket = idTicket,
                    PrecioFinal = 0,
                    EstaCotizado = 0,
                    CotizacionAceptada = 0,
                    Renegociar = 0,
                    NumeroVerificador = 1
                };

                db.CotizacionTicket.Add(cotizacion);

                //Cambiando el estado del ticket
                ticket.estadoTicket = estadoTicket;
                //Adicionando el histórico del ticket
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    estadoTicket = estadoTicket,
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

                db.SaveChanges();

                //Enviando el email
                Persona persona = colaborador.persona;

                string saludo = "Estimado: " + Utiles.UpperCamelCase(persona.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(persona.Apellido1.ToLower()) + ",<br/>";
                if (persona.Sexo == "F")
                    saludo = "Estimada: " + Utiles.UpperCamelCase(persona.Nombre1.ToLower()) + " " + Utiles.UpperCamelCase(persona.Apellido1.ToLower()) + ",<br/>";

                string htmlMail = "<div class=\"textoCuerpo\">" + saludo + @"Se ha entrado una nueva petición de cotización de ticket al sistema Sifizplanning,<br/>
                                            Datos del ticket:<br>
                                            <b>Número de ticket:</b> " + String.Format("{0:000000}", ticket.Secuencial) + @"<br/>
                                            <b>Cliente:</b> " + ticket.persona_cliente.cliente.Descripcion + @"<br/>
                                            <b>Asunto del ticket:</b> " + ticket.Asunto + @"<br/>
                                            <b>Prioridad:</b> " + ticket.prioridadTicket.Codigo + @"<br/>
                                            <b>Categoría:</b> " + ticket.categoriaTicket.Codigo + @"<br/>
                                            <b>Estimación:</b> " + ticket.Estimacion + @" horas<br/></div>";

                List<string> usuariosDestinos = new List<string>();
                Usuario usuario = persona.usuario.FirstOrDefault();
                usuariosDestinos.Add(usuario.Email);

                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    usuariosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }

                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), htmlMail, codigoCliente + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - Nueva cotización de ticket (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

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

        [Authorize(Roles = "COTIZADOR, ADMIN, COORDINADOR, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarColaboradoresParaVerAsignaciones()
        {
            try
            {
                var colaboradores = (from colab in db.Colaborador
                                     join
                                         pers in db.Persona on colab.persona equals pers
                                     where colab.departamento.Asignable == 1
                                           && pers.usuario.FirstOrDefault().EstaActivo == 1
                                     orderby pers.Nombre1, pers.Apellido1
                                     select new
                                     {
                                         id = colab.Secuencial,
                                         nombre = pers.Nombre1 + " " + pers.Apellido1,
                                         sede = colab.sede.Secuencial
                                     }).ToList();

                var resp = new
                {
                    success = true,
                    colaboradores = colaboradores
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

        //ADICION DE TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult InformacionUsuariosClientes(int idCliente)
        {
            try
            {
                var clientes = (from pc in db.Persona_Cliente
                                join
                                    p in db.Persona on pc.SecuencialPersona equals p.Secuencial
                                join
                                    u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                where pc.SecuencialCliente == idCliente
                                select new
                                {
                                    idPersonaCliente = pc.SecuencialPersona,
                                    nombre = p.Nombre1 + " " + p.Apellido1,
                                    email = u.Email,
                                    telefono = pc.Telefono
                                }).ToList();

                var resp = new
                {
                    success = true,
                    clientes = clientes
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult AdicionarTicket(int idPersonaCliente, string reportado, string telefono, string asunto, string detalle, int prioridad, int categoria, int ticketVersionCliente, HttpPostedFileBase[] adjuntos = null)
        {
            try
            {
                Ticket ticket = new Ticket
                {
                    SecuencialEstadoTicket = 1,//Abierto
                    SecuencialPersona_Cliente = idPersonaCliente,
                    SecuencialCategoriaTicket = categoria,
                    SecuencialPrioridadTicket = prioridad,
                    SecuencialProximaActividad = 1,//NO ASIGNADO
                    SecuencialTipoRecurso = 1,//NO ASIGNADO
                    SecuencialTicketVersionCliente = ticketVersionCliente,
                    Telefono = telefono,
                    ReportadoPor = reportado,
                    Asunto = asunto,
                    Detalle = detalle,
                    FechaCreado = DateTime.Now,
                    HorasCreado = 0,
                    Reputacion = 5,
                    Estimacion = 0,
                    Reprocesos = 0,
                    IngresoInterno = true,
                    SeFactura = false,
                    Facturado = false,
                    NumeroVerificador = 1,
                    FechaRevisado = DateTime.Now
                };

                db.Ticket.Add(ticket);

                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                TicketHistorico ticketH = new TicketHistorico
                {
                    SecuencialTicket = ticket.Secuencial,
                    Version = 0,
                    SecuencialEstadoTicket = 1,//Abierto
                    SecuencialPersona_Cliente = idPersonaCliente,
                    SecuencialCategoriaTicket = categoria,
                    SecuencialPrioridadTicket = prioridad,
                    SecuencialProximaActividad = 1,//NO ASIGNADO
                    SecuencialTipoRecurso = 1,//NO ASIGNADO
                    usuario = user,
                    Telefono = telefono,
                    ReportadoPor = reportado,
                    Asunto = asunto,
                    Detalle = detalle,
                    FechaCreado = ticket.FechaCreado,
                    FechaRevisado = ticket.FechaRevisado,
                    Reputacion = 5,
                    Estimacion = 0,
                    FechaOperacion = DateTime.Now,
                    Reprocesos = 0,
                    IngresoInterno = true,
                    SeFactura = false,
                    Facturado = false,
                    NumeroVerificador = 1
                };
                db.TicketHistorico.Add(ticketH);
                db.SaveChanges();

                var ticketPersonaCliente = db.Persona_Cliente.Find(ticket.SecuencialPersona_Cliente);

                ComentarioGeneral comentarioGeneral = new ComentarioGeneral
                {
                    usuario = user,
                    FechaHora = DateTime.Now,
                    TipoComentario = "NOTIFICACION",
                    Comentario = "<b>NOTIFICACIÓN</b>-Ticket: <b>" + string.Format("{0:000000}", ticket.Secuencial) + "</b>, Cliente: <b>" + ticketPersonaCliente.cliente.Codigo + "</b>, Asunto: <b>" + ticket.Asunto + "</b>, Se ingresó un nuevo Ticket",
                    Importancia = "Importante"
                };
                db.ComentarioGeneral.Add(comentarioGeneral);
                db.SaveChanges();
                //Llamar el signalR
                Websocket.getInstance().NuevosComentarios();
                Websocket.getInstance().NuevoComentarioMuyImportante();

                //Por los ficheros adjuntos
                if (adjuntos != null)
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string extFile = Path.GetExtension(adj.FileName);
                            string newNameFile = Utiles.RandomString(10) + extFile;
                            newNameFile = System.IO.Path.GetRandomFileName() + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                            adj.SaveAs(path);

                            AdjuntoTicket adjTicket = new AdjuntoTicket
                            {
                                Url = "/resources/tickets/" + newNameFile,
                                ticket = ticket
                            };

                            db.AdjuntoTicket.Add(adjTicket);
                        }
                    }
                db.SaveChanges();

                //Enviando el email de notificación
                Persona_Cliente personaCliente = db.Persona_Cliente.Find(idPersonaCliente);
                Persona persona = personaCliente.persona;
                string emailCliente = persona.usuario.FirstOrDefault().Email;

                string textoEmail = @"<div class='textoCuerpo'>Estimado: ";
                if (persona.Sexo == "F")
                    textoEmail = @"Estimada: ";
                textoEmail += persona.Nombre1 + " " + persona.Apellido1 + "<br>";

                textoEmail += "Hemos registrado internamente su requerimiento, el mismo responde al ticket: ";

                string numeroTicket = string.Format("{0:000000}", ticket.Secuencial);

                textoEmail += "<b>" + numeroTicket + ".</b> <br/>";
                textoEmail += "<b>Asunto del requerimiento: </b>" + ticket.Asunto + "<br>";
                textoEmail += "Nuestro equipo de soporte se pondrá en contacto con usted lo más pronto posible.";
                textoEmail += "</div>";

                List<string> usuariosDestinos = new List<string>();
                //usuariosDestinos.Add( "rcespedes@sifizsoft.com" ); //borrar aqui
                usuariosDestinos.Add(emailCliente);
                usuariosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));

                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    usuariosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                usuariosDestinos = usuariosDestinos.Distinct().ToList();

                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                string asuntoEmail = codigoCliente + " HESO " + numeroTicket + " - Adicionado el nuevo ticket (" + ticket.Asunto + ")";
                Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), textoEmail, asuntoEmail, null, numeroTicket);

                string destinos = String.Join(", ", usuariosDestinos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de creación de nuevo ticket</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
                textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;

                HistoricoInformacionTicket historicoCorreo = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = ticketH.SecuencialTicket,
                    VersionTicketHistorico = ticketH.Version,
                    Fecha = DateTime.Now,
                    Texto = textoHistoricoCorreo
                };
                db.HistoricoInformacionTicket.Add(historicoCorreo);
                db.SaveChanges();

                var resp = new
                {
                    success = true,
                    msg = "Se ha adicionado correctamente el ticket número: " + numeroTicket
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

        //COTIZACIÓN DE TICKET
        [Authorize(Roles = "COTIZADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult CotizacionesTickets(string filtro = "")
        {
            try
            {
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Colaborador colaborador = user.persona.colaborador.FirstOrDefault();
                var cotizaciones = (from ct in db.CotizacionTicket
                                    join
                                        t in db.Ticket on ct.ticket equals t
                                    where t.estadoTicket.Codigo == "COTIZÁNDOSE" &&
                                          ct.SecuencialColaborador == colaborador.Secuencial
                                    select new
                                    {
                                        idCotizacion = ct.Secuencial,
                                        idTicket = t.Secuencial,
                                        cliente = t.persona_cliente.cliente.Descripcion,
                                        email = t.persona_cliente.persona.usuario.FirstOrDefault().Email,
                                        fecha = t.FechaCreado,
                                        prioridad = t.prioridadTicket.Codigo,
                                        categoria = t.categoriaTicket.Codigo
                                    }).ToList();

                if (filtro != "")
                {
                    cotizaciones = (from c in cotizaciones
                                    where (c.categoria.ToLower().Contains(filtro.ToLower()) ||
                                           c.cliente.ToLower().Contains(filtro.ToLower()) ||
                                           c.email.ToLower().Contains(filtro.ToLower()) ||
                                           c.fecha.ToString().Contains(filtro.ToLower()) ||
                                           c.idTicket.ToString().ToLower().Contains(filtro.ToLower()) ||
                                           c.prioridad.ToString().ToLower().Contains(filtro.ToLower())
                                        )
                                    select c).ToList();
                }

                var resp = new
                {
                    success = true,
                    cotizaciones = cotizaciones
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

        [Authorize(Roles = "COTIZADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult DarEstimacionesTickets(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket.");
                }

                var estimaciones = (from et in db.EstimacionTicket
                                    where et.SecuencialTicket == ticket.Secuencial &&
                                          et.EstaActiva == 1 && et.FactorTiempo != 0
                                    select new
                                    {
                                        nombreColaborador = et.colaborador.persona.Nombre1 + " " + et.colaborador.persona.Apellido1,
                                        horas = et.NumeroHoras,
                                        catalogada = (et.FactorTiempo == 1) ? "OPTIMISTA" : (et.FactorTiempo == -1) ? "PESIMISTA" : "PROBABLE",
                                        entregables = et.EntregablesAdicionales,
                                        informacionComplementaria = et.InformacionComplementaria,
                                        detalles = (from d in db.DetalleEstimacionTicket
                                                    where d.SecuencialEstimacionTicket == et.Secuencial
                                                    select new
                                                    {
                                                        detalle = d.Detalle,
                                                        tiempoEstimacion = d.TiempoEstimacion,
                                                        tiempoDesarrollo = d.TiempoDesarrollo,
                                                        tiempoPrueba = d.TiempoPrueba,
                                                        nivel = d.nivelColaborador.Descripcion
                                                    }).ToList()
                                    }).ToList();
                var resp = new
                {
                    success = true,
                    estimacion = ticket.Estimacion,
                    estimaciones = estimaciones
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

        [Authorize(Roles = "COTIZADOR, ADMIN, TICKET")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NuevaOfertaCotizacionTicket(int idCotizacion, string texto, float porcientoIncremento, float totalHoras, float precioHoras, float precioTotal, string fechaLimite, bool renegociable = false, HttpPostedFileBase[] adjuntos = null)
        {
            try
            {
                CotizacionTicket cotizacion = db.CotizacionTicket.Find(idCotizacion);
                if (cotizacion == null)
                {
                    throw new Exception("No se encontró la cotización");
                }

                string[] fechas = fechaLimite.Split(new Char[] { '/' });
                int dia = Int32.Parse(fechas[0]);
                int mes = Int32.Parse(fechas[1]);
                int anno = Int32.Parse(fechas[2]);
                DateTime fecha = new System.DateTime(anno, mes, dia);

                OfertaCotizacionTicket oferta = new OfertaCotizacionTicket
                {
                    cotizacionTicket = cotizacion,
                    Fecha = DateTime.Now,
                    TextoOferta = texto,
                    Precio = (decimal)precioTotal,
                    PrecioHoras = (decimal)precioHoras,
                    TotalHoras = (decimal)totalHoras,
                    PorcientoIncremento = (decimal)porcientoIncremento,
                    OfertaAceptada = 0,
                    FechaLimite = fecha,
                    Renegociable = (renegociable == true) ? 1 : 0
                };

                db.OfertaCotizacionTicket.Add(oferta);

                //Pasando el ticket a esperando respuesta del cliente. codigo 7
                Ticket ticket = cotizacion.ticket;
                ticket.SecuencialEstadoTicket = 7;//  ("ESPERANDO RESPUESTA DEL CLIENTE")

                //Adicionando el histórico del ticket
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("ESPERANDO RESPUESTA DEL CLIENTE")
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

                //Por los ficheros adjuntos
                if (adjuntos != null)
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string extFile = Path.GetExtension(adj.FileName);
                            string newNameFile = "C_" + Utiles.RandomString(10) + extFile;
                            newNameFile = "C_" + System.IO.Path.GetRandomFileName() + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                            adj.SaveAs(path);

                            /*
                            AdjuntoTicket adjTicket = new AdjuntoTicket
                            {
                                Url = "/resources/tickets/" + newNameFile,
                                ticket = nuevoTicket
                            };
                            * db.AdjuntoTicket.Add(adjTicket);
                            */
                            //Aquiiii 
                        }
                    }

                db.SaveChanges();

                //Enviando Email a los Implicados
                string htmlEmail = "<div class=\"textoCuerpo\">" + "Se ha realizado una cotización sobre su ticket: " + String.Format("{0:000000}", ticket.Secuencial) + "<br/>";
                htmlEmail += "<b>Asunto del ticket: </b>" + ticket.Asunto + "<br/> ";
                htmlEmail += "los datos de la misma son:<br/>";
                htmlEmail += texto + "<br/><br/>";

                htmlEmail += "por favor gestione la respuesta a través del sistema Sifizplanning<br/>";
                htmlEmail += "http://186.5.29.67/SifizPlanning" + "</div>";

                string emailCliente = cotizacion.ticket.persona_cliente.persona.usuario.FirstOrDefault().Email;

                List<string> usuariosDestinos = new List<string>();
                usuariosDestinos.Add(emailCliente);
                usuariosDestinos.Add(emailUser);

                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    usuariosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                usuariosDestinos = usuariosDestinos.Distinct().ToList();
                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), htmlEmail, codigoCliente + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - Nueva cotización de ticket (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

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

        [Authorize(Roles = "COTIZADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult TicketsNoCerradosCliente(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el cliente solicitante.");
                }

                var tickets = (from t in db.Ticket
                               where t.persona_cliente.SecuencialCliente == ticket.persona_cliente.SecuencialCliente &&
                               (t.estadoTicket.Codigo != "ANULADO" && t.estadoTicket.Codigo != "CERRADO")
                               select new
                               {
                                   id = t.Secuencial,
                                   cliente = t.persona_cliente.cliente.Descripcion,
                                   fecha = t.FechaCreado,
                                   estado = t.estadoTicket.Codigo,
                                   prioridad = t.prioridadTicket.Codigo,
                                   categoria = t.categoriaTicket.Codigo,
                                   detalle = t.Detalle.Length > 20 ? t.Detalle.Substring(0, 20) + "..." : t.Detalle
                               }).ToList();

                var resp = new
                {
                    success = true,
                    tickets = tickets
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

        [Authorize(Roles = "COTIZADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult CotizacionesCliente(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el cliente solicitante.");
                }

                var cotizaciones = (from c in db.CotizacionTicket
                                    join
          t in db.Ticket on c.ticket equals t
                                    where t.persona_cliente.SecuencialCliente == ticket.persona_cliente.SecuencialCliente
                                    orderby c.Secuencial descending
                                    select new
                                    {
                                        id = t.Secuencial,
                                        cliente = t.persona_cliente.cliente.Descripcion,
                                        fecha = t.FechaCreado,
                                        prioridad = t.prioridadTicket.Codigo,
                                        categoria = t.categoriaTicket.Codigo,
                                        cantOfertas = c.ofertaCotizacionTicket.Count(),
                                        cotizacionAceptada = c.CotizacionAceptada == 1 ? "SI" : "NO",
                                        /*fechaUltima = c.ofertaCotizacionTicket.Count() > 0 ? 
                                                       (from of in db.OfertaCotizacionTicket
                                                        where of.SecuencialCotizacionTicket == c.Secuencial
                                                              orderby of.Secuencial descending
                                                        select of.Fecha).FirstOrDefault().ToString():"",  */
                                        ultimoPrecio = c.ofertaCotizacionTicket.Count() > 0 ?
                                                       (from of in db.OfertaCotizacionTicket
                                                        where of.SecuencialCotizacionTicket == c.Secuencial
                                                        orderby of.Secuencial descending
                                                        select of.Precio).FirstOrDefault() : 0
                                    }).ToList();
                var resp = new
                {
                    success = true,
                    cotizaciones = cotizaciones
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

        //ASIGNACION DE TICKET
        //[Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        //[HttpPost]
        //public ActionResult ComprobarTicketWorkItemTFS(int idTicket, string datos)
        //{
        //    try
        //    {
        //        Ticket ticket = db.Ticket.Find(idTicket);
        //        List<ProyectoTFS> proyectosTfs = ticket.persona_cliente.cliente.proyectoTFS.ToList<ProyectoTFS>().Where(x => x.EstaActivo == 1).ToList<ProyectoTFS>();
        //        if (proyectosTfs.Count == 0)
        //        {
        //            throw new Exception("El Cliente del ticket no tiene proyectos asignados en el TFS para escoger");
        //        }

        //        //Decodificando el json
        //        var s = new JavaScriptSerializer();
        //        var jsonObj = s.Deserialize<dynamic>(datos);

        //        DateTime ahora = DateTime.Now;
        //        for (int i = 0; i < jsonObj.Length; i++)
        //        {
        //            dynamic tipoId = jsonObj[i]["idColaborador"];
        //            int idColaborador = 0;
        //            if (object.ReferenceEquals(tipoId.GetType(), idColaborador.GetType()))
        //            {
        //                idColaborador = tipoId;
        //            }
        //            else
        //            {
        //                idColaborador = int.Parse(jsonObj[i]["idColaborador"]);
        //            }

        //            UsuarioTFS usuarioTfs = db.UsuarioTFS.Where(x => x.SecuencialColaborador == idColaborador && x.FechaInicio <= ahora && x.FechaFin >= ahora).FirstOrDefault();
        //            if (usuarioTfs == null)
        //            {
        //                Persona persona = db.Colaborador.Find(idColaborador).persona;
        //                string nombre = persona.Nombre1 + " " + persona.Apellido1;
        //                throw new Exception("El colaborador " + nombre + " no tiene un usuario vigente en el TFS, gestione este usuario en el módulo de administración 'Gestión de Acceso' e intente nuevamente.");
        //            }
        //        }

        //        List<object> listaProyectos = new List<object>();
        //        foreach (ProyectoTFS proyecto in proyectosTfs)
        //        {
        //            listaProyectos.Add(new
        //            {
        //                id = proyecto.Secuencial,
        //                nombre = proyecto.Nombre
        //            });
        //        }

        //        var resp = new
        //        {
        //            success = true,
        //            proyectosTfs = listaProyectos
        //        };
        //        return Json(resp);
        //    }
        //    catch (Exception e)
        //    {
        //        var resp = new
        //        {
        //            success = false,
        //            msg = e.Message
        //        };
        //        return Json(resp);
        //    }
        //}

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult AsignacionTareaPorTicket(int idTicket, string datos, int reputacion, int idProyectoTFS = 0)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }

                //para la actualizacion de los permisos y las tareas
                List<DiaColaborador> listaDiaColaborador = new List<DiaColaborador>();
                List<int> idColaboradoresTicket = new List<int>();

                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Cliente cliente = ticket.persona_cliente.cliente;
                Tarea tareaPrincipal = null;

                //Decodificando el json
                var s = new JavaScriptSerializer();
                var jsonObj = s.Deserialize<dynamic>(datos);

                int totalHorasAsignadas = 0;
                for (int i = 0; i < jsonObj.Length; i++)
                {
                    dynamic tipoId = jsonObj[i]["idColaborador"];
                    int idColaborador = 0;
                    if (object.ReferenceEquals(tipoId.GetType(), idColaborador.GetType()))
                    {
                        idColaborador = tipoId;
                    }
                    else
                    {
                        idColaborador = int.Parse(jsonObj[i]["idColaborador"]);
                    }

                    int idTarea = 0;
                    try
                    {
                        dynamic tipoIdTarea = jsonObj[i]["idTarea"];
                        //Verificando si la tarea ya esta asignada
                        if (object.ReferenceEquals(tipoId.GetType(), idTarea.GetType()))
                        {
                            idTarea = tipoIdTarea;
                        }
                        else
                        {
                            idTarea = int.Parse(jsonObj[i]["idTarea"]);
                        }
                    }
                    catch (Exception) { }

                    if (idTarea != 0)
                        continue;

                    dynamic idUbicacion = jsonObj[i]["ubicacion"];
                    int ubicacion = 0;
                    if (object.ReferenceEquals(idUbicacion.GetType(), ubicacion.GetType()))
                    {
                        ubicacion = idUbicacion;
                    }
                    else
                    {
                        ubicacion = int.Parse(jsonObj[i]["ubicacion"]);
                    }

                    dynamic nHoras = jsonObj[i]["numeroHoras"];
                    int numeroHoras = 0;
                    if (object.ReferenceEquals(nHoras.GetType(), numeroHoras.GetType()))
                    {
                        numeroHoras = nHoras;
                    }
                    else
                    {
                        numeroHoras = int.Parse(jsonObj[i]["numeroHoras"]);
                    }
                    totalHorasAsignadas += numeroHoras;

                    string fecha = jsonObj[i]["fecha"];
                    int idActividad = int.Parse(jsonObj[i]["idActividad"]);
                    int idModulo = int.Parse(jsonObj[i]["idModulo"]);

                    dynamic pCoordinador = jsonObj[i]["coordinador"];
                    int coordinador = 0;
                    if (object.ReferenceEquals(coordinador.GetType(), pCoordinador.GetType()))
                    {
                        coordinador = pCoordinador;
                    }
                    else
                    {
                        coordinador = (jsonObj[i]["coordinador"] != null && jsonObj[i]["coordinador"] != "") ? int.Parse(jsonObj[i]["coordinador"]) : 0;
                    }

                    string detalle = jsonObj[i]["detalle"] + " asignacion correspondiente al ticket: " + string.Format("{0:000000}", ticket.Secuencial);

                    string[] fechas = fecha.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    DateTime diaTarea = new System.DateTime(anno, mes, dia);
                    DateTime diaSiguiente = diaTarea.AddDays(1);

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
                    foreach (var tarea in tareasDelDia)
                    {
                        TimeSpan tiempo = tarea.ffin - tarea.finicio;
                        time += tiempo.Hours;
                    }

                    DateTime fechaInicio = diaTarea.AddHours(time);
                    fechaInicio = fechaInicio.AddMinutes(30 + (8 * 60));//A las 8.30 empieza
                    DateTime fechaFin = fechaInicio.AddHours(numeroHoras);

                    if (fechaInicio.Hour < 13 && fechaFin.Hour > 13)
                    {
                        fechaFin = fechaFin.AddHours(1);
                    }
                    else if (fechaInicio.Hour == 13)
                    {
                        fechaInicio = fechaInicio.AddHours(1);
                        fechaFin = fechaFin.AddHours(1);
                    }

                    idColaboradoresTicket.Add(idColaborador);

                    Tarea tar = new Tarea
                    {
                        SecuencialColaborador = idColaborador,
                        SecuencialActividad = idActividad,
                        SecuencialModulo = idModulo,
                        SecuencialCliente = cliente.Secuencial,
                        SecuencialEstadoTarea = 1,
                        SecuencialLugarTarea = ubicacion,
                        Detalle = detalle.ToUpper(),
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin,
                        HorasUtilizadas = 0,
                        NumeroVerificador = 1
                    };
                    db.Tarea.Add(tar);

                    if (coordinador != 0)
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

                    if (tareaPrincipal == null)
                        tareaPrincipal = tar;

                    listaDiaColaborador.Add(new DiaColaborador { Fecha = tar.FechaInicio.Date, IdColaborador = tar.SecuencialColaborador });

                    //Asignando la tarea al ticket
                    TicketTarea ticketTarea = new TicketTarea
                    {
                        ticket = ticket,
                        tarea = tar,
                        EstaActiva = 1
                    };
                    db.TicketTarea.Add(ticketTarea);

                    //Adicionando la tarea al TFS del colaborador
                    //string usuarioTFS = db.Colaborador.Find(idColaborador).usuarioTFS.Where(x => x.FechaInicio <= DateTime.Now && x.FechaFin >= DateTime.Now).FirstOrDefault().Usuario;
                    //ProyectoTFS proyectoTFS = null;
                    //if (idProyectoTFS == 0)
                    //{
                    //    proyectoTFS = db.ProyectoTFS.Where(x => x.SecuencialCliente == cliente.Secuencial && x.EstaActivo == 1).FirstOrDefault();
                    //}
                    //else
                    //{
                    //    proyectoTFS = db.ProyectoTFS.Find(idProyectoTFS);
                    //}

                    //string servidorTfs = proyectoTFS.servidorTFS.Url;
                    //string coleccionTfs = proyectoTFS.coleccionTFS.Nombre;
                    //string nombreProyectoTFS = proyectoTFS.Nombre;
                    //int tiempoTarea = Utiles.DarHorasTarea(tar.FechaInicio, tar.FechaFin);

                    //int idWorkItem = 0;
                    //try
                    //{
                    //    idWorkItem = ClientTfs.AdicionarTrabajo(servidorTfs, coleccionTfs, nombreProyectoTFS, "Task", usuarioTFS, "TK-" + ticket.Secuencial.ToString() + " " + ticket.Asunto, ticket.Detalle, ticket.Secuencial.ToString(), tiempoTarea);
                    //}
                    //catch (Exception) { }

                    //int idWorkItem = ClientTfs.AdicionarTrabajo("http://100.100.100.2:8080/tfs", "Financial2010", "PruebasWorkItem", "Task", "rafael.cespedes", "TK-" + ticket.Secuencial.ToString() + " " + ticket.Asunto, ticket.Detalle, ticket.Secuencial.ToString(), tiempoTarea);
                    //int idWorkItem = 0;

                    //Relacionando el ticket con sus workItemId
                    //TicketAsignacionTFS ticketAsignacion = new TicketAsignacionTFS
                    //{
                    //    proyectoTFS = proyectoTFS,
                    //    ticketTarea = ticketTarea,
                    //    WorkItemID = idWorkItem
                    //};
                    //db.TicketAsignacionTFS.Add(ticketAsignacion);
                }

                //En el flujo básico que no se realiza el proceso de estimación
                if (ticket.Estimacion == 0)
                    ticket.Estimacion = totalHorasAsignadas;

                //Actualizando las reputaciones
                ticket.Reputacion = reputacion;

                //Pasando el ticket a asignado. codigo 2
                ticket.SecuencialEstadoTicket = 2;//  ("EL TICKET ESTA ASIGNADO")

                //Adicionando el histórico del ticket                
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA ASIGNADO")
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

                //Actualizando las fechas de los dias que existieron los cambios y en las fechas
                foreach (DiaColaborador diaColab in listaDiaColaborador)
                {
                    Utiles.OrdenarTareasPermisos(diaColab.Fecha, diaColab.IdColaborador, user, db);
                }

                //Enviando los correos.
                List<string> destinatarioCorreos = Utiles.CorreoPorGrupoEmail("COORD");
                List<string> destinatarioCorreosColaboradores = new List<string>();
                List<string> nombresColaboradores = new List<string>();
                Persona personaCliente = ticket.persona_cliente.persona;
                string emailCliente = personaCliente.usuario.FirstOrDefault().Email;
                idColaboradoresTicket = idColaboradoresTicket.Distinct().ToList();
                foreach (int idColab in idColaboradoresTicket)
                {
                    Persona personaColaborador = db.Colaborador.Find(idColab).persona;
                    string email = personaColaborador.usuario.FirstOrDefault().Email;
                    destinatarioCorreos.Add(email);
                    destinatarioCorreosColaboradores.Add(email);
                    nombresColaboradores.Add(personaColaborador.Nombre1 + " " + personaColaborador.Apellido1);
                }

                string textoEmail = "<div class=\"textoCuerpo\">Estimados:<br/>";

                string textoIngenieros = String.Join(", Ing. ", nombresColaboradores);

                string articulo = "al";
                if (nombresColaboradores.Count > 1)
                    articulo = "a los";
                string pronombreRelativo = "quien";
                if (nombresColaboradores.Count > 1)
                    pronombreRelativo = "quienes";
                string verboRelativo = "atenderá";
                if (nombresColaboradores.Count > 1)
                    verboRelativo = "atenderán";

                textoEmail += @"El requerimiento <b>" + string.Format("{0:000000}", ticket.Secuencial) + "</b> ha sido asignado " + articulo + " Ing. " + textoIngenieros + " " + pronombreRelativo + @" <br/>
                                " + verboRelativo + " el mismo a partir del día " + tareaPrincipal.FechaInicio.ToString("dd/MM/yyyy") + @".<br/>
                                <b>Asunto del ticket: </b>" + ticket.Asunto + @"<br/>
                                Ing. " + textoIngenieros + ", <br/> por favor su ayuda en el día señalado.</div>";

                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                string asuntoEmail = codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Asignación del requerimiento (" + ticket.Asunto + ")";

                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    destinatarioCorreos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                destinatarioCorreos = destinatarioCorreos.Distinct().ToList();

                Utiles.EnviarEmailSistema(destinatarioCorreos.ToArray(), textoEmail, asuntoEmail, null, String.Format("{0:000000}", ticket.Secuencial));

                //adicionando email a los historicos
                string destinos = String.Join(", ", destinatarioCorreos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de información, asignación de requerimiento</b><br/>";
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

                //Enviando Email a los colaboradores
                textoEmail = "<div class=\"textoCuerpo\"><b>Nueva asignación de tareas referentes al ticket " + string.Format("{0:000000}", ticket.Secuencial) + ": </b><br/><br/>";

                textoEmail += "<b>Información de contacto:</b>" + "<br/>";
                textoEmail += "Cliente: " + ticket.persona_cliente.cliente.Descripcion + "<br/>";
                textoEmail += "Correo del cliente: " + emailCliente + "<br/>";
                textoEmail += "Teléfono del cliente: " + ticket.persona_cliente.Telefono + "<br/>";
                textoEmail += "Prioridad: " + ticket.prioridadTicket.Codigo + "<br/>";
                textoEmail += "Reportado Por: " + ticket.ReportadoPor + "<br/>";
                if (ticket.Telefono != "-" && ticket.Telefono != null)
                {
                    textoEmail += "Teléfono de contacto para este ticket: " + ticket.Telefono + "<br/>";
                }
                textoEmail += "<br/>";

                textoEmail += "<b>Información sobre el ticket:</b>" + "<br/>";
                textoEmail += "Asunto: " + ticket.Asunto + "<br/>";
                textoEmail += "Detalle: " + ticket.Detalle + "<br/>";

                if (ticket.adjuntoTicket.Count > 0)
                {//El ticket tiene adjuntos
                    textoEmail += "<b>Recursos para desarrollar el ticket:</b> <br/>";
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    int i = 1;
                    foreach (AdjuntoTicket recurso in ticket.adjuntoTicket)
                    {
                        textoEmail += "<a href=\"" + baseUrl + "/Web" + recurso.Url + "\">recurso " + i++ + "</a> <br/>";
                    }
                    textoEmail += "<br/>";
                }

                List<string> diasAsignacion = new List<string>();
                foreach (DiaColaborador diaColab in listaDiaColaborador)
                {
                    diasAsignacion.Add(diaColab.Fecha.ToString("dd/MM/yyyy"));
                }
                string strDiasAsignacion = String.Join(", ", diasAsignacion);
                if (diasAsignacion.Count == 1)
                {
                    textoEmail += "Tarea asignada el: " + strDiasAsignacion;
                }
                else if (diasAsignacion.Count > 1)
                {
                    textoEmail += "Las tareas están asignadas los días: <br/>";
                    textoEmail += strDiasAsignacion + "<br/>";
                }

                textoEmail += "</div>";

                asuntoEmail = codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Detalles del ticket (" + ticket.Asunto + ")";
                Utiles.EnviarEmailSistema(destinatarioCorreosColaboradores.ToArray(), textoEmail, asuntoEmail, null, String.Format("{0:000000}", ticket.Secuencial));

                //adicionando los email a los historicos
                destinos = String.Join(", ", destinatarioCorreosColaboradores.ToArray());
                textoHistoricoCorreo = "<b>Correo de información, asignación de requerimiento</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
                textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
                HistoricoInformacionTicket historicoCorreoTicketC = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
                    VersionTicketHistorico = ticketHistorico.Version,
                    Fecha = DateTime.Now,
                    Texto = textoHistoricoCorreo
                };
                db.HistoricoInformacionTicket.Add(historicoCorreoTicketC);
                db.SaveChanges();

                Thread thread = new Thread(() =>
                {
                    List<string> correosDestinos = new List<string>();
                    correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("TFS"));
                    correosDestinos.AddRange(destinatarioCorreosColaboradores);
                    correosDestinos.Add(emailUser);

                    string textoEmailColab = @"<div class='textoCuerpo'><br/>";
                    textoEmailColab += "Estimados,";
                    textoEmailColab += @"<br/>";

                    textoEmailColab += "Por favor su ayuda con el acceso a las fuentes de: " + cliente.Descripcion + ", al/los colaborador/es: ";
                    string listaNombres = string.Join(", ", nombresColaboradores);
                    textoEmailColab += listaNombres;
                    textoEmailColab += "</div>";

                    string asuntoEmailColab = "Solicitud Acceso";
                    Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmailColab, asuntoEmailColab);
                });
                thread.Start();

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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult AnularTareaPorTicket(int idTarea)
        {
            try
            {
                var tarea = (from t in db.Tarea
                             where t.Secuencial == idTarea
                             select t).FirstOrDefault();

                TicketTarea ticketTarea = tarea.ticketTarea.FirstOrDefault();
                tarea.SecuencialEstadoTarea = 4;
                tarea.SecuencialColaborador = db.Colaborador.Where(s => s.persona.usuario.FirstOrDefault().Email == "canulado@sifizsoft.com").FirstOrDefault().Secuencial;

                db.SaveChanges();

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

        //APROBAR EL TICKET
        public ActionResult AprobarTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encontró");
                }

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 11;//EL TICKET ESTA ANULADO

                //Adicionando el histórico del ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Cliente cliente = ticket.persona_cliente.cliente;
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA APROBADO")
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

        //CERRAR EL TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR, CLIENTE")]
        public ActionResult CerrarTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encontró");
                }

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 14;//EL TICKET ESTA CERRADO
                ticket.SecuencialProximaActividad = 18;//NA

                //Adicionando el histórico del ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Cliente cliente = ticket.persona_cliente.cliente;
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA CERRADO")
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

                string textoEmail = "<div class=\"textoCuerpo\">" + @"Por medio de esta comunicación le informamos que el ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b> ha sido CERRADO; <br/> 
                                     esta acción se ha tomado porque hemos recibido su conformidad con el mismo o en su defecto no se recibió una respuesta de su parte dentro del período de espera de los cinco días laborables contados desde la fecha en la que enviamos nuestra solicitud de certificación de conformidad de este requerimiento vía correo electrónico.<br/>
                                     En el caso de requerir correcciones, solicitamos se ingrese un nuevo requerimiento y se incluya el código " + "\"HESO " + string.Format("{0:000000}", ticket.Secuencial) + "\" en el detalle del mismo.<br/> <b>Asunto del ticket: </b>" + ticket.Asunto + "</div>";

                List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");
                string correoCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault().Email;
                correosDestinos.Insert(0, correoCliente);

                //Borrar aqui
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                correosDestinos = correosDestinos.Distinct().ToList();
                Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket cerrado (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

                //adicionando el email a los historicos
                string destinos = String.Join(", ", correosDestinos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de información, Ticket Cerrado</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket cerrado" + "<br/>";
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

        //CERRAR EL TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR, CLIENTE")]
        public ActionResult CerrarTicketPorCliente(int idTicket, string emailUser)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encontró");
                }

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 14;//EL TICKET ESTA CERRADO
                ticket.SecuencialProximaActividad = 18;//NA

                //Adicionando el histórico del ticket                               
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Cliente cliente = ticket.persona_cliente.cliente;
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA CERRADO")
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

                string textoEmail = "<div class=\"textoCuerpo\">" + @"Por medio de esta comunicación le informamos que el ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b> ha sido CERRADO; <br/> 
                                     esta acción se ha tomado porque hemos recibido su conformidad con el mismo o en su defecto no se recibió una respuesta de su parte dentro del período de espera de los cinco días laborables contados desde la fecha en la que enviamos nuestra solicitud de certificación de conformidad de este requerimiento vía correo electrónico.<br/>
                                     En el caso de requerir correcciones, solicitamos se ingrese un nuevo requerimiento y se incluya el código " + "\"HESO " + string.Format("{0:000000}", ticket.Secuencial) + "\" en el detalle del mismo.<br/> <b>Asunto del ticket: </b>" + ticket.Asunto + "</div>";

                List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");
                string correoCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault().Email;
                correosDestinos.Insert(0, correoCliente);

                //Borrar aqui
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                correosDestinos = correosDestinos.Distinct().ToList();
                Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket cerrado (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

                //adicionando el email a los historicos
                string destinos = String.Join(", ", correosDestinos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de información, Ticket Cerrado</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket cerrado" + "<br/>";
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

        //GESTION DE LOS TICKETS RESUELTOS
        [Authorize(Roles = "COTIZADOR, ADMIN, COORDINADOR, TICKET")]
        [HttpPost]
        public ActionResult DarTicketsResueltos(int cantDias)
        {
            try
            {
                DateTime fecha = Utiles.fechaAtrasDiasLaborables(cantDias).AddDays(1);//Verificar si se suma o no un día para este proposito
                var ticketsResueltos = (
                                from t in db.Ticket
                                join
                                    th in db.TicketHistorico on t.Secuencial equals th.SecuencialTicket
                                join pc in db.Persona_Cliente on t.SecuencialPersona_Cliente equals pc.SecuencialPersona
                                join cl in db.Cliente on pc.SecuencialCliente equals cl.Secuencial
                                where t.SecuencialEstadoTicket == 10 //resuelto
                                  && th.SecuencialEstadoTicket == 10 && th.FechaOperacion < fecha
                                  && th.Version == (db.TicketHistorico.Where(x => x.SecuencialTicket == t.Secuencial).Count() - 1)//Solo el ultimo
                                select new
                                {
                                    id = t.Secuencial,
                                    asunto = t.Asunto,
                                    seFactura = (th.SeFactura) ? "SI" : "NO",
                                    cliente = cl.Descripcion,
                                    fecha = th.FechaOperacion,
                                    selected = 0
                                });

                var resp = new
                {
                    success = true,
                    fechaDesde = fecha.ToString("dd/MM/yyyy"),
                    ticketsResueltos = ticketsResueltos
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


        [Authorize(Roles = "ADMIN, COORDINADOR, TICKET")]
        [HttpPost]
        public ActionResult CerrarTicketsResueltos(int[] idTickets)
        {
            try
            {
                if (idTickets == null || idTickets.Length == 0)
                {
                    throw new Exception("Se debe seleccionar al menos un elemento.");
                }
                foreach (int idTicket in idTickets)
                {
                    ActionResult action = this.CerrarTicket(idTicket);
                }

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

        //GESTION DE LOS TICKETS EN APROBACION
        [Authorize(Roles = "COTIZADOR, ADMIN, COORDINADOR, TICKET")]
        [HttpPost]
        public ActionResult DarTicketsEnAprobacion(int cantDias)
        {
            try
            {
                DateTime fecha = Utiles.fechaAtrasDiasLaborables(cantDias).AddDays(1);//Verificar si se suma o no un día para este proposito

                db.Database.CommandTimeout = 120;
                var ticketsEnAprobacion = (
                                from t in db.Ticket
                                join th in db.TicketHistorico on t.Secuencial equals th.SecuencialTicket
                                join pc in db.Persona_Cliente on t.SecuencialPersona_Cliente equals pc.SecuencialPersona
                                join cl in db.Cliente on pc.SecuencialCliente equals cl.Secuencial
                                where t.proximaActividad.Codigo == "APROBAR CLIENTE"
                                    && t.estadoTicket.Codigo == "PENDIENTE"
                                    && (from thi in db.TicketHistorico
                                        join p in db.ProximaActividad on thi.SecuencialProximaActividad equals p.Secuencial
                                        where thi.SecuencialTicket == t.Secuencial && db.TicketHistorico.Where(h => h.Version == thi.Version - 1 && h.SecuencialTicket == t.Secuencial).FirstOrDefault().proximaActividad.Codigo != "APROBAR CLIENTE"
                                        orderby thi.Version descending
                                        select thi.FechaOperacion
                                             ).FirstOrDefault() < fecha
                                    && th.Version == (db.TicketHistorico.Where(x => x.SecuencialTicket == t.Secuencial).Count() - 1)//Solo el ultimo
                                select new
                                {
                                    id = t.Secuencial,
                                    asunto = t.Asunto,
                                    seFactura = (th.SeFactura) ? "SI" : "NO",
                                    cliente = cl.Descripcion,
                                    fecha = (from thi in db.TicketHistorico
                                             join p in db.ProximaActividad on thi.SecuencialProximaActividad equals p.Secuencial
                                             where thi.SecuencialTicket == t.Secuencial && db.TicketHistorico.Where(h => h.Version == thi.Version - 1 && h.SecuencialTicket == t.Secuencial).FirstOrDefault().proximaActividad.Codigo != "APROBAR CLIENTE"
                                             orderby thi.Version descending
                                             select thi.FechaOperacion
                             ).FirstOrDefault(),
                                    selected = 0
                                }).ToList();

                var resp = new
                {
                    success = true,
                    fechaDesde = fecha.ToString("dd/MM/yyyy"),
                    ticketsEnAprobacion
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

        //GESTION DE LOS TICKETS EN ESPERA
        [Authorize(Roles = "COTIZADOR, ADMIN, COORDINADOR, TICKET")]
        [HttpPost]
        public ActionResult DarTicketsEnEspera(int cantDias)
        {
            try
            {
                DateTime fecha = Utiles.fechaAtrasDiasLaborables(cantDias);
                var ticketsEnEspera = (
                                from t in db.Ticket
                                join
                                    th in db.TicketHistorico on t.Secuencial equals th.SecuencialTicket
                                where t.SecuencialEstadoTicket == 7 //ESPERANDO RESPUESTA
                                  && th.SecuencialEstadoTicket == 7 && th.FechaOperacion < fecha
                                  && th.Version == (db.TicketHistorico.Where(x => x.SecuencialTicket == t.Secuencial).Count() - 1)//Solo el ultimo
                                select new
                                {
                                    id = t.Secuencial,
                                    asunto = t.Asunto,
                                    fecha = th.FechaOperacion
                                });

                var resp = new
                {
                    success = true,
                    fechaDesde = fecha.ToString("dd/MM/yyyy"),
                    ticketsEnEspera = ticketsEnEspera
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

        //ANULAR EL TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, CLIENTE, GESTOR")]
        [HttpPost]
        public ActionResult AnularTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encontró");
                }

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 13;//EL TICKET ESTA ANULADO
                ticket.SecuencialProximaActividad = 18;//NA

                //Adicionando el histórico del ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Cliente cliente = ticket.persona_cliente.cliente;
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA ANULADO")
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

                string textoEmail = "<div class=\"textoCuerpo\">" + @"Por medio de esta comunicación le informamos que los requerimientos <br/>
                                      correspondientes al ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b> han sido ANULADOS; <br/> <b>Asunto del ticket:</b><br/> " + ticket.Asunto + @".<br/>  
                                      Esta acción se ha tomado por una de las siguientes razones:<br/>
                                      1) el ticket fue duplicado.<br/>
                                      2) usted ha solicitado la anulación porque ya se lo resolvió internamente.<br/>
                                      3) No se recibió una aprobación de la cotización enviada dentro del tiempo de vigencia de la misma.<br/><br/>
                                         En el caso de que usted no esté de acuerdo con esta anulación, 
                                         por favor sírvase usted de enviar un correo a soporte@sifizsoft.com y lo contactaremos en la brevedad posible incluya el código <b>" + string.Format("{0:000000}", ticket.Secuencial) + "</b> en el cuerpo del mismo.</div>";

                List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");
                string correoCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault().Email;
                correosDestinos.Insert(0, correoCliente);

                //Borrar aqui
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                correosDestinos = correosDestinos.Distinct().ToList();
                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Anulación de requerimiento (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

                //adicionando el email a los historicos
                string destinos = String.Join(", ", correosDestinos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de información, Anulación de Ticket</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + "Anulación de requerimiento" + "<br/>";
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

        //TICKET PENDIENTE
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult PendienteTicket(int idTicket, string comentario)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encontró");
                }

                //Adicionando el comentario al ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Persona personaUsuario = user.persona;

                db.ComentarioTicket.Add(new ComentarioTicket
                {
                    SecuencialColaborador = personaUsuario.colaborador.FirstOrDefault().Secuencial,
                    SecuencialTicket = idTicket,
                    FechaHora = DateTime.Now,
                    Detalle = comentario,
                    VerTodos = 1
                });

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 4;//EL TICKET ESTA PENDIENTE

                //Adicionando el histórico del ticket
                Cliente cliente = ticket.persona_cliente.cliente;
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA ANULADO")
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

                string textoEmail = "<div class=\"textoCuerpo\">" + @"Por medio de esta comunicación le informamos que el ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b> ha pasado a PENDIENTE. <br/> <b>Asunto del ticket:</b><br/> " + ticket.Asunto + @".<br/> </div>";

                List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");

                //Borrar aqui
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                correosDestinos = correosDestinos.Distinct().ToList();
                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Pendiente (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

                //adicionando el email a los historicos
                string destinos = String.Join(", ", correosDestinos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de información, Ticket Pendiente</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket Pendiente" + "<br/>";
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

        //ESPERAR RESPUESTA EN TICKETS
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult EsperarRespuestaTicket(int idTicket, string texto, HttpPostedFileBase[] adjuntos = null)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encontró");
                }

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 7;//EL TICKET ESTA ESPERANDO RESPUESTA

                //Adicionando el histórico del ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Cliente cliente = ticket.persona_cliente.cliente;
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA ESPERANDO RESPUESTA")
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

                //Adicionando los datos al historico
                HistoricoInformacionTicket historicoInformacion = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
                    VersionTicketHistorico = ticketHistorico.Version,
                    Fecha = ticketHistorico.FechaOperacion,
                    Texto = texto
                };
                db.HistoricoInformacionTicket.Add(historicoInformacion);
                db.SaveChanges();

                //Por los ficheros adjuntos   
                if (adjuntos != null)
                {
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string extFile = Path.GetExtension(adj.FileName);
                            string newNameFile = Utiles.RandomString(10) + extFile;
                            newNameFile = System.IO.Path.GetRandomFileName() + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                            adj.SaveAs(path);

                            HistoricoAdjunto adjTicket = new HistoricoAdjunto
                            {
                                SecuencialHistoInforTicket = historicoInformacion.Secuencial,
                                Url = "/resources/tickets/" + newNameFile
                            };

                            db.HistoricoAdjunto.Add(adjTicket);
                        }
                    }
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
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(resp);
            }
        }

        //ACEPTAR TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult AceptarTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("El ticket no se encontró");
                }

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 17;//EL TICKET ESTA ACEPTADO

                //Adicionando el histórico del ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Cliente cliente = ticket.persona_cliente.cliente;
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA ACEPTADO")
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

        //COMENTARIOS Y DESACUERDOS DEL TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult CargarDesacuerdosTickets(int idTicket)
        {
            try
            {
                var desacuerdos = (from trd in db.Ticket_ResolucionDesacuerdo
                                   where trd.SecuencialTicket_Resolucion == idTicket
                                   select new
                                   {
                                       fechaHora = trd.FechaHora,
                                       colaborador = trd.colaborador.persona.usuario.FirstOrDefault().Email,
                                       detalle = trd.Detalle
                                   }).ToList();

                var resp = new
                {
                    success = true,
                    desacuerdos = desacuerdos
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
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult GuardarDesacuerdoTickets(int idTicket, string desacuerdo)
        {
            try
            {
                Ticket_Resolucion tr = db.Ticket_Resolucion.Find(idTicket);
                if (tr == null)
                {
                    throw new Exception("Error, no puede estar en desacuerdo con la resolucion porque no se ha definido ninguna forma de resolucion en este ticket.");
                }

                //Adicionando el histórico del ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Persona personaUsuario = user.persona;

                db.Ticket_ResolucionDesacuerdo.Add(new Ticket_ResolucionDesacuerdo
                {
                    SecuencialColaborador = personaUsuario.colaborador.FirstOrDefault().Secuencial,
                    SecuencialTicket_Resolucion = idTicket,
                    FechaHora = DateTime.Now,
                    Detalle = desacuerdo
                });

                ComentarioGeneral comentarioGeneral = new ComentarioGeneral
                {
                    usuario = user,
                    FechaHora = DateTime.Now,
                    TipoComentario = "DESACUERDO",
                    Comentario = "<b>DESACUERDO</b>-Ticket: <b>" + string.Format("{0:000000}", tr.ticket.Secuencial) + "</b>, Cliente: <b>" + tr.ticket.persona_cliente.cliente.Codigo + "</b>, Asunto: <b>" + tr.ticket.Asunto + "</b>, " + desacuerdo,
                    Importancia = "Importante"
                };
                db.ComentarioGeneral.Add(comentarioGeneral);

                db.SaveChanges();

                //Llamar al SignalR
                Websocket.getInstance().NuevosComentarios();
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult CargarComentariosTickets(int idTicket)
        {
            try
            {
                var comentarios = (from ct in db.ComentarioTicket
                                   where ct.SecuencialTicket == idTicket
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
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, CLIENTE, GESTOR")]
        [HttpPost]
        public ActionResult GuardarComentariosTickets(int idTicket, string comentario, bool verTodos)
        {
            try
            {
                Ticket t = db.Ticket.Find(idTicket);
                if (t == null)
                {
                    throw new Exception("Error, no puede insertar un comentario porque no se encontró el ticket.");
                }

                //Adicionando el comentario al ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Persona personaUsuario = user.persona;

                string palabra = comentario.Split(' ').First();
                Colaborador col = null;
                if (palabra == "ANULADO:")
                {
                    //string colaborador = System.Configuration.ConfigurationManager.AppSettings["ColaboradorDefault"];
                    string email = System.Configuration.ConfigurationManager.AppSettings["emailApp"];
                    //string colaborador = "sifizplanning@sifizsoft.com";
                    col = (from c in db.Colaborador
                           join p in db.Persona on c.SecuencialPersona equals p.Secuencial
                           join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                           where u.Email == email
                           select c).First();
                }
                db.ComentarioTicket.Add(new ComentarioTicket
                {
                    SecuencialColaborador = col == null ? personaUsuario.colaborador.FirstOrDefault().Secuencial : col.Secuencial,
                    SecuencialTicket = idTicket,
                    FechaHora = DateTime.Now,
                    Detalle = comentario,
                    VerTodos = verTodos ? 1 : 0
                });

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
                Websocket.getInstance().NuevosComentarios();
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR, CLIENTE, USER")]
        [HttpPost]
        public ActionResult EnviarEmailComentario(int idTicket, string destinatariosEmailTicket, string asuntoEmailTicket, string comentarioEmailTicket)
        {
            try
            {
                Ticket t = db.Ticket.Find(idTicket);
                if (t == null)
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
                if (!rgx.IsMatch(destinatariosEmailTicket))
                {
                    throw new Exception("Debe ingresar una lista de correos válida separados por punto y coma(;)");
                };
                string[] emails = destinatariosEmailTicket.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var email in emails)
                {
                    usuariosDestinos.Add(email);
                }
                var gestores = t.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
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

        //RECHAZAR TICKET
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, CLIENTE, GESTOR")]
        [HttpPost]
        public ActionResult RechazarTicket(int idTicket, string comentario)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("Error, no se encontró el ticket");
                }

                bool financial25 = false;
                if (ticket.SecuencialTicketVersionCliente != null && db.Ticket_RequierePublicacion.Find(ticket.Secuencial) != null)
                {
                    financial25 = db.TicketVersionCliente.Find(ticket.SecuencialTicketVersionCliente).Codigo == "FBS 2.5";
                }

                //Adicionando el comentario al ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Persona personaUsuario = user.persona;

                string palabra = comentario.Split(' ').First();
                Colaborador col = null;
                if (palabra == "RECHAZADO:")
                {
                    string colaborador = System.Configuration.ConfigurationManager.AppSettings["ColaboradorDefault"];
                    colaborador += "@sifizsoft.com";
                    col = (from c in db.Colaborador
                           join p in db.Persona on c.SecuencialPersona equals p.Secuencial
                           join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                           where u.Email == colaborador
                           select c).First();
                }
                db.ComentarioTicket.Add(new ComentarioTicket
                {
                    SecuencialColaborador = col == null ? personaUsuario.colaborador.FirstOrDefault().Secuencial : col.Secuencial,
                    SecuencialTicket = idTicket,
                    FechaHora = DateTime.Now,
                    Detalle = comentario,
                    VerTodos = 1
                });

                //Cambiando de estado al ticket
                ticket.SecuencialEstadoTicket = 19;//EL TICKET ESTA RECHAZADO
                ticket.SecuencialProximaActividad = 18;

                //Adicionando el histórico del ticket                
                Cliente cliente = ticket.persona_cliente.cliente;
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA RECHAZADO")
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

                //Enviando correos de rechazo y adicionando al historico de correos
                string textoEmail = "<div class=\"textoCuerpo\">" + @"Por medio de esta comunicación le informamos que el ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b> ha sido RECHAZADO; <br/> 
                                     presentando como justificación el siguiente comentario:<br/>" + comentario + @"<br/></div>";

                List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");
                if (financial25)
                {
                    correosDestinos.Add("publicacionesdoscinco@sifizsoft.com");
                }
                correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("JEDES"));
                string correoCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault().Email;
                correosDestinos.Insert(0, correoCliente);

                //Enviando email
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                correosDestinos = correosDestinos.Distinct().ToList();
                Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket rechazado (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

                //adicionando el email a los historicos
                string destinos = String.Join(", ", correosDestinos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de información, Ticket Rechazado</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket rechazado" + "<br/>";
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

        //HISTÓRICO DE TICKETS
        [Authorize(Roles = "COORDINADOR, COTIZADOR, ADMIN, USER, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult EventosHistoricoTicket(int idTicket, string filtro = "")
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }

                //CAMBIOS EN EL HISTORICO
                List<EventoTicket> eventos = (from th in db.TicketHistorico
                                              where th.SecuencialTicket == idTicket
                                              select new EventoTicket
                                              {
                                                  Tipo = 1,//Cambios en el histórico del ticket
                                                  SecuencialObjeto = th.Version,//Para los cambios en el histótico el id es la version
                                                  Fecha = th.FechaOperacion,
                                                  Descripcion = th.Version == 0 ? "Creación del ticket" : "Actualización del ticket"
                                              }).ToList<EventoTicket>();

                //OFERTAS AL CLIENTE
                List<EventoTicket> eventos2 = (from of in db.OfertaCotizacionTicket
                                               join
               ct in db.CotizacionTicket on of.cotizacionTicket equals ct
                                               where ct.SecuencialTicket == idTicket
                                               select new EventoTicket
                                               {
                                                   Tipo = 2,//Cambios en las ofertas del ticket
                                                   SecuencialObjeto = of.Secuencial,//Para los cambios en las ofertas al cliente de cotizaciones
                                                   Fecha = of.Fecha,
                                                   Descripcion = "Oferta de cotización"
                                               }).ToList<EventoTicket>();
                eventos.AddRange(eventos2);

                //RENEGOCIACION DEL CLIENTE
                List<EventoTicket> eventos3 = (from rn in db.Renegociacion
                                               join
                        of in db.OfertaCotizacionTicket on rn.ofertaCotizacionTicket equals of
                                               join
        ct in db.CotizacionTicket on of.cotizacionTicket equals ct
                                               where ct.SecuencialTicket == idTicket
                                               select new EventoTicket
                                               {
                                                   Tipo = 3,//RENEGOCIACION
                                                   SecuencialObjeto = rn.Secuencial,//Para los cambios en las renegociaciones del cliente
                                                   Fecha = of.Fecha,
                                                   Descripcion = "Renegociación de cotización"
                                               }).ToList<EventoTicket>();
                eventos.AddRange(eventos3);

                if (filtro != "")
                {
                    eventos = (from e in eventos
                               where e.Fecha.ToString().Contains(filtro) ||
                                     e.Descripcion.ToString().ToLower().Contains(filtro.ToLower())
                               select e).ToList<EventoTicket>();
                }

                eventos.Sort();
                eventos.Reverse();

                var resp = new
                {
                    success = true,
                    eventos = eventos,
                    numero = ticket.Secuencial,
                    cliente = ticket.persona_cliente.cliente.Descripcion,
                    categoria = ticket.categoriaTicket.Codigo,
                    prioridad = ticket.prioridadTicket.Codigo,
                    asunto = ticket.Asunto
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

        [Authorize(Roles = "COORDINADOR, COTIZADOR, ADMIN, USER, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult DarDatosEventoTicket(int idTicket, int tipo, int secuencialObjeto)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }

                string dataHtml = "";
                switch (tipo)
                {
                    case 1://Aqui son las actualizaciones de los tickets ver en el historico
                        TicketHistorico ticketH = db.TicketHistorico.Find(idTicket, secuencialObjeto);
                        if (ticketH == null)
                        {
                            throw new Exception("No se encontró el histórico del ticket");
                        }
                        dataHtml += "<b>Fecha de Operación:</b> " + ticketH.FechaOperacion.ToString("dd/MM/yyyy HH:mm:ss") + "<br/>";
                        dataHtml += "<b>Usuario:</b> " + ticketH.usuario.persona.Nombre1 + " " + ticketH.usuario.persona.Apellido1 + "<br/>";
                        dataHtml += "<b>Estado del Ticket:</b> " + ticketH.estadoTicket.Codigo + "<br/>";
                        dataHtml += "<b>Prioridad del Ticket:</b> " + ticketH.prioridadTicket.Codigo + "<br/>";
                        if (ticketH.prioridadRevisadaTicket != null)
                        {
                            dataHtml += "<b>Prioridad Revisada:</b> " + ticketH.prioridadRevisadaTicket.Codigo + "<br/>";
                        }
                        dataHtml += "<b>Categoría del Ticket:</b> " + ticketH.categoriaTicket.Codigo + "<br/>";
                        if (ticketH.categoriaRevisadaTicket != null)
                        {
                            dataHtml += "<b>Categoría Revisada:</b> " + ticketH.categoriaRevisadaTicket.Codigo + "<br/>";
                        }
                        dataHtml += "<b>Tipo de Recurso:</b> " + ticketH.tipoRecurso.Codigo + "<br/>";
                        dataHtml += "<b>Próxima Actividad:</b> " + ticketH.proximaActividad.Codigo + "<br/>";

                        dataHtml += "<b>Tiempo Estimado:</b> " + ticketH.Estimacion + " horas<br/>";
                        dataHtml += "<b>Se Factura:</b> " + ((ticketH.SeFactura == true) ? "SI" : "NO") + "<br/>";
                        dataHtml += "<b>Facturado:</b> " + ((ticketH.Facturado == true) ? "SI" : "NO") + "<br/>";
                        dataHtml += "<b>Reprocesos:</b> " + ticketH.Reprocesos + "<br/>";
                        dataHtml += "<b>Detalle:</b> " + ticketH.Detalle + "<br/>";
                        dataHtml += "<b>Teléfonos:</b> " + ticketH.Telefono + "<br/>";
                        dataHtml += "<b>Fecha de Creación del Ticket:</b> " + ticketH.FechaCreado.ToString("dd/MM/yyyy HH:mm:ss") + "<br/>";
                        if (ticketH.FechaRevisado.HasValue)
                        {
                            dataHtml += "<b>Fecha Revisado:</b> " + ticketH.FechaRevisado.Value.ToString("dd/MM/yyyy") + "<br/>";
                        }
                        else
                        {
                            dataHtml += "<b>Fecha Revisado:</b> " + ticketH.FechaCreado.ToString("dd/MM/yyyy") + "<br/>";
                        }
                        dataHtml += "<b>Reportado Por:</b> " + ticketH.ReportadoPor + "<br/>";
                        dataHtml += "<b>Ingreso Interno:</b> " + ((ticketH.IngresoInterno == true) ? "SI" : "NO") + "<br/>";

                        string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                        List<HistoricoInformacionTicket> historicoInformaciones = ticketH.historicoInformacionTicket.ToList();
                        if (historicoInformaciones.Count > 0)
                        {
                            string correos = @"
                                            -------------------------------------------------------------------------
                                             <br/><br/>";

                            foreach (HistoricoInformacionTicket hct in historicoInformaciones)
                            {
                                correos += hct.Texto;
                                correos += @"<br/>";

                                int i = 1;
                                if (hct.historicoAdjunto.Count > 0)//Aqui tienen adjuntos
                                {
                                    foreach (HistoricoAdjunto ha in hct.historicoAdjunto)
                                    {
                                        correos += "<a href=\"" + baseUrl + "/Web" + ha.Url + "\" target=\"_blank\">Adjunto" + i++ + "</a><br/>";
                                    }
                                }
                                correos += @"<br/>";

                                correos += @"-------------------------------------------------------------------------
                                             <br/><br/>";
                            }

                            dataHtml += correos;
                        }

                        break;

                    case 2://Aqui son las actualizaciones de las cotizaciones de los tickets
                        OfertaCotizacionTicket oferta = db.OfertaCotizacionTicket.Find(secuencialObjeto);
                        if (oferta == null)
                        {
                            throw new Exception("No se encontró la oferta de cotización");
                        }
                        dataHtml += "<b>Fecha de Operación:</b> " + oferta.Fecha.ToString("dd/MM/yyyy HH:mm:ss") + "<br/>";
                        dataHtml += "<b>Usuario:</b> " + oferta.cotizacionTicket.colaborador.persona.Nombre1 + " " + oferta.cotizacionTicket.colaborador.persona.Apellido1 + "<br/>";
                        dataHtml += "<b>Precio de Oferta:</b> $ " + oferta.Precio + "<br/>";
                        dataHtml += "<b>Precio de Hora:</b> $ " + oferta.PrecioHoras + "<br/>";
                        dataHtml += "<b>Total de Horas:</b> $ " + oferta.TotalHoras + "<br/>";
                        dataHtml += "<b>Porciento de Incremento:</b> $ " + oferta.PorcientoIncremento + "<br/>";
                        dataHtml += "<b>Texto Oferta:</b> <br>" + oferta.TextoOferta + "<br/>";

                        string renegociable = (oferta.Renegociable == 1) ? "SI" : "NO";
                        dataHtml += "<b>Oferta Renegociable:</b>" + renegociable + "<br/>";
                        string aceptada = (oferta.OfertaAceptada == 1) ? "SI" : "NO";
                        dataHtml += "<b>Oferta Aceptada:</b>" + aceptada + "<br/>";
                        break;

                    case 3://Aqui son las actualizaciones de las renegociaciones de los clientes
                        Renegociacion renegociacion = db.Renegociacion.Find(secuencialObjeto);
                        if (renegociacion == null)
                        {
                            throw new Exception("No se encontró la renegociación en la cotización");
                        }
                        dataHtml += "<b>Fecha de Operación:</b> " + renegociacion.Fecha.ToString("dd/MM/yyyy HH:mm:ss") + "<br/>";

                        Persona personaCliente = renegociacion.ofertaCotizacionTicket.cotizacionTicket.ticket.persona_cliente.persona;
                        string nombreUsuario = personaCliente.Nombre1 + " " + personaCliente.Apellido1;

                        dataHtml += "<b>Usuario:</b> " + nombreUsuario + "<br/>";
                        dataHtml += "<b>Texto Renegociación:</b> <br>" + renegociacion.TextoRenegociacion + "<br/>";
                        break;
                }

                var resp = new
                {
                    success = true,
                    datos = dataHtml
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

        [Authorize(Roles = "COORDINADOR, COTIZADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult GrabarInformacionHistoricoTicket(int idTicket, int secuencialObjeto, string texto, HttpPostedFileBase[] adjuntos = null)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }

                TicketHistorico ticketHistorico = db.TicketHistorico.Find(idTicket, secuencialObjeto);
                if (ticketHistorico == null)
                {
                    throw new Exception("No se encontró el histórico del ticket");
                }

                HistoricoInformacionTicket historicoCorreo = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = idTicket,
                    VersionTicketHistorico = secuencialObjeto,
                    Fecha = ticketHistorico.FechaOperacion,
                    Texto = texto
                };
                db.HistoricoInformacionTicket.Add(historicoCorreo);
                db.SaveChanges();

                //Por los ficheros adjuntos   
                if (adjuntos != null)
                {
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string extFile = Path.GetExtension(adj.FileName);
                            string newNameFile = Utiles.RandomString(10) + extFile;
                            newNameFile = System.IO.Path.GetRandomFileName() + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                            adj.SaveAs(path);

                            HistoricoAdjunto adjTicket = new HistoricoAdjunto
                            {
                                SecuencialHistoInforTicket = historicoCorreo.Secuencial,
                                Url = "/resources/tickets/" + newNameFile
                            };

                            db.HistoricoAdjunto.Add(adjTicket);
                        }
                    }
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
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(resp);
            }
        }

        [Authorize(Roles = "COORDINADOR, COTIZADOR, ADMIN, TICKET, USER")]
        [HttpPost]
        public ActionResult GrabarInformacionHistoricoDevolucionTicket(int idTicket, string texto)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }

                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    estadoTicket = ticket.estadoTicket,
                    SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
                    SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
                    SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
                    SecuencialProximaActividad = ticket.SecuencialProximaActividad,
                    SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
                    ReportadoPor = ticket.ReportadoPor,
                    Reputacion = ticket.Reputacion,
                    usuario = user,
                    Telefono = ticket.Telefono,
                    Asunto = ticket.Asunto,
                    Detalle = ticket.Detalle,
                    FechaCreado = ticket.FechaCreado,
                    Estimacion = ticket.Estimacion,
                    IngresoInterno = ticket.IngresoInterno,
                    Reprocesos = ticket.Reprocesos,
                    SeFactura = ticket.SeFactura,
                    Facturado = ticket.Facturado,
                    NumeroVerificador = 1,
                    FechaOperacion = DateTime.Now,
                    RequiereTesting = ticket.RequiereTesting
                };
                db.TicketHistorico.Add(ticketHistorico);

                HistoricoInformacionTicket historicoInformacion = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = idTicket,
                    VersionTicketHistorico = numeroVersion,
                    Fecha = ticketHistorico.FechaOperacion,
                    Texto = texto
                };
                db.HistoricoInformacionTicket.Add(historicoInformacion);
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

        //DATOS RESUMEN TICKETS
        [Authorize(Roles = "COORDINADOR, COTIZADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult ResumenTicket(string strFecha = "")
        {
            try
            {
                DateTime fecha = DateTime.Today;
                if (strFecha != "")
                {
                    fecha = DateTime.Parse(strFecha);
                }

                DateTime[] fechasSemana = Utiles.diasSemanaFecha(fecha);
                DateTime fechaLunes = fechasSemana[0];
                DateTime fechaDomingo = fechasSemana[1];
                DateTime fechaProximoLunes = fechaDomingo.AddDays(1);

                var ticketsSemana = db.Ticket.Where(x => x.FechaCreado >= fechaLunes && x.FechaCreado < fechaProximoLunes).ToList();
                var ticketsAnteriores = db.Ticket.Where(x => x.FechaCreado < fechaLunes).ToList();

                var semana = new
                {
                    ingresados = ticketsSemana.Count(),
                    abiertos = ticketsSemana.Where(x => x.SecuencialEstadoTicket == 1).Count(),
                    asignados = ticketsSemana.Where(x => x.SecuencialEstadoTicket == 2).Count(),
                    desarrollos = ticketsSemana.Where(x => x.SecuencialEstadoTicket == 9).Count(),
                    resueltos = ticketsSemana.Where(x => x.SecuencialEstadoTicket == 10).Count(),
                    cerrados = ticketsSemana.Where(x => x.SecuencialEstadoTicket == 14).Count(),
                    anulados = ticketsSemana.Where(x => x.SecuencialEstadoTicket == 13).Count(),
                    facturable = ticketsSemana.Where(x => x.SeFactura == true).Count()
                };

                var anteriores = new
                {
                    ingresados = ticketsAnteriores.Count(),
                    abiertos = ticketsAnteriores.Where(x => x.SecuencialEstadoTicket == 1).Count(),
                    asignados = ticketsAnteriores.Where(x => x.SecuencialEstadoTicket == 2).Count(),
                    desarrollos = ticketsAnteriores.Where(x => x.SecuencialEstadoTicket == 9).Count(),
                    resueltos = ticketsAnteriores.Where(x => x.SecuencialEstadoTicket == 10).Count(),
                    cerrados = ticketsAnteriores.Where(x => x.SecuencialEstadoTicket == 14).Count(),
                    anulados = ticketsAnteriores.Where(x => x.SecuencialEstadoTicket == 13).Count(),
                    facturable = ticketsAnteriores.Where(x => x.SeFactura == true).Count()
                };

                //Escogiendo los tickets resueltos de los de la semana
                var ticketResueltos = ticketsSemana.Where(x => x.SecuencialEstadoTicket == 10).ToList();
                var ticketsClientes = (from tr in ticketResueltos
                                       group tr by tr.SecuencialPersona_Cliente into g
                                       select new
                                       {
                                           cliente = db.Persona_Cliente.Where(x => x.SecuencialPersona == g.Key).FirstOrDefault().cliente.Descripcion,
                                           cantidad = g.Count(),
                                           horasAsignadas = g.Where(x => x.SecuencialPersona_Cliente == g.Key).Sum(x => x.Estimacion)
                                       }).ToList();

                var secuencialTicketResueltos = ticketResueltos.Select(z => z.Secuencial).ToList();

                var tareasTicketTerminadosTecnicos = (from tt in db.TicketTarea
                                                      join t in db.Tarea on tt.tarea equals t
                                                      where secuencialTicketResueltos.Contains(tt.SecuencialTicket)
                                                      select t).Distinct().ToList();

                var ticketsTecnicos = (from t in tareasTicketTerminadosTecnicos
                                       group t.ticketTarea by t.SecuencialColaborador into g
                                       select new
                                       {
                                           tecnico = db.Colaborador.Where(x => x.Secuencial == g.Key).FirstOrDefault().persona.Nombre1 + " " + db.Colaborador.Where(x => x.Secuencial == g.Key).FirstOrDefault().persona.Apellido1,
                                           cantidad = g.Count(),
                                           horasAsignadas = g.Max(x => x.Select(z => z.ticket).Sum(y => y.Estimacion))
                                       }).ToList();

                var resp = new
                {
                    success = true,
                    semana = semana,
                    anteriores = anteriores,
                    ticketsClientes = ticketsClientes,
                    ticketsTecnicos = ticketsTecnicos
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

        //DATOS REPORTE TICKETS
        [Authorize(Roles = "COORDINADOR, COTIZADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult ReporteTicket(int idCliente, string strFechaInicio = "", string strFechaFin = "")
        {
            try
            {

                System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("es-ES");
                DateTime fechaInicio = DateTime.Today;
                DateTime fechaFin = DateTime.Today;
                if (strFechaInicio != "" && strFechaFin != "")
                {
                    fechaInicio = DateTime.Parse(strFechaInicio, cultureinfo);
                    fechaFin = DateTime.Parse(strFechaFin, cultureinfo).AddDays(1);
                }
                DateTime fechaInicioMantenimiento = new DateTime(fechaInicio.Year, fechaInicio.Month, 1);

                var ticketsAgrupados = (from T in db.Tarea
                                        join TT in db.TicketTarea on T.Secuencial equals TT.SecuencialTarea into TTGroup
                                        from TTG in TTGroup.DefaultIfEmpty()
                                        join TK in db.Ticket on TTG.SecuencialTicket equals TK.Secuencial into TKGroup
                                        from TKG in TKGroup.DefaultIfEmpty()
                                        join C in db.Colaborador on T.SecuencialColaborador equals C.Secuencial
                                        join P in db.Persona on C.SecuencialPersona equals P.Secuencial
                                        join LT in db.LugarTarea on T.SecuencialLugarTarea equals LT.Secuencial
                                        join TAR in db.TareaActividadRealizada on T.Secuencial equals TAR.SecuencialTarea
                                        join CL in db.Cliente on T.SecuencialCliente equals CL.Secuencial
                                        join ET in db.EstadoTicket on TKG.SecuencialEstadoTicket equals ET.Secuencial into ETGroup
                                        from ETG in ETGroup.DefaultIfEmpty()
                                        join CR in db.Ticket_CategoriaRevisada on TKG.Secuencial equals CR.SecuencialTicket into CRGroup
                                        from CRG in CRGroup.DefaultIfEmpty()
                                        join CT in db.CategoriaTicket on CRG.SecuencialCategoriaTicket equals CT.Secuencial into CTGroup
                                        from CTG in CTGroup.DefaultIfEmpty()
                                            //join E in db.EntregableMotivoTrabajo on TTG.tarea.entregableMotivoTrabajo.Secuencial equals E.Secuencial into EMTGroup
                                            //from EMTG in EMTGroup.DefaultIfEmpty()
                                            //join MT in db.MotivoTrabajo on EMTG.SecuencialMotivoTrabajo equals MT.Secuencial into MTGroup
                                            //from MTG in MTGroup.DefaultIfEmpty()
                                            //join TMT in db.TipoMotivoTrabajo on MTG.SecuencialTipoMotivoTrabajo equals TMT.Secuencial into TMTGroup
                                            //from TMTG in TMTGroup.DefaultIfEmpty()
                                        where
                                           T.SecuencialCliente == idCliente
                                           && T.FechaInicio >= fechaInicio
                                           && T.FechaInicio < fechaFin
                                           && (TTG != null || (T.entregableMotivoTrabajo != null ? T.entregableMotivoTrabajo.motivoTrabajo.tipoMotivoTrabajo.Secuencial == 3 : true))
                                           && (TKG.SeFactura == false || TKG == null)
                                        select new
                                        {
                                            tarea = (int?)T.Secuencial ?? 0,
                                            cliente = CL.Descripcion,
                                            ticket = (int?)TKG.Secuencial ?? 0,
                                            detalle = TKG.Asunto ?? T.Detalle,
                                            reportador = TKG.ReportadoPor,
                                            tecnico = P.Nombre1 + " " + P.Apellido1,
                                            fecha = T.FechaInicio,
                                            categoria = TKG != null ? TKG.categoriaTicket.Codigo : "NO APLICA",
                                            categoriaRevisada = CRG != null ? CRG.categoriaTicket.Codigo : "NO APLICA",
                                            estado = ETG.Codigo ?? "FINALIZADO",
                                            tiempo = (SqlFunctions.DateDiff("MINUTE", TAR.HoraInicio, TAR.HoraFin)) / 60.00
                                        }).ToList();

                var tickets = ticketsAgrupados.GroupBy(g => new { g.cliente, g.ticket, g.detalle, g.reportador, g.tecnico, g.fecha, g.categoria, g.categoriaRevisada, g.estado, g.tarea })
                                                .Select(a => new
                                                {
                                                    tarea = a.Key.tarea,
                                                    cliente = a.Key.cliente,
                                                    numero = a.Key.ticket,
                                                    detalle = a.Key.detalle,
                                                    reportado = a.Key.reportador,
                                                    asignado = a.Key.tecnico,
                                                    fecha = a.Key.fecha.ToString("dd/MM/yyyy"),
                                                    fechaComparacion = a.Key.fecha,
                                                    categoria = a.Key.categoria,
                                                    categoriaRevisada = a.Key.categoriaRevisada,
                                                    estado = a.Key.estado,
                                                    tiempo =
                                                                db.TicketsMantenimiento.Where(s => s.SecuencialTicket == a.Key.ticket &&
                                                                DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault() != null
                                                                ?
                                                                    db.TicketsMantenimiento.Where(s => s.SecuencialTicket == a.Key.ticket &&
                                                                    DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault().Tiempo > 0
                                                                    ?
                                                                    db.TicketsMantenimiento.Where(s => s.SecuencialTicket == a.Key.ticket &&
                                                                    DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault().Tiempo
                                                                    : 1
                                                                :
                                                                db.TareaMantenimiento.Where(s => s.SecuencialTarea == a.Key.tarea &&
                                                                DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault() != null
                                                                ?
                                                                    db.TareaMantenimiento.Where(s => s.SecuencialTarea == a.Key.tarea &&
                                                                    DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault().Tiempo > 0
                                                                    ?
                                                                    db.TareaMantenimiento.Where(s => s.SecuencialTarea == a.Key.tarea &&
                                                                    DbFunctions.TruncateTime(s.Fecha) == DbFunctions.TruncateTime(a.Key.fecha)).FirstOrDefault().Tiempo
                                                                    : 1
                                                                :
                                                                Math.Round(a.Sum(s => s.tiempo.Value), MidpointRounding.AwayFromZero) > 0 ?
                                                                Math.Round(a.Sum(s => s.tiempo.Value), MidpointRounding.AwayFromZero) : 1
                                                }).OrderBy(b => b.fecha).ToList();

                var mantenimientoTarea = db.TareaMantenimientoBorrar.Where(s => s.Fecha >= fechaInicio && s.Fecha < fechaFin).ToList();
                var mantenimientoTicket = db.TicketMantenimientoBorar.Where(s => s.Fecha >= fechaInicio && s.Fecha < fechaFin).ToList();

                var tick = tickets.ToList();
                foreach (var i in tickets)
                {
                    int countTareas = 0;
                    int countTickets = 0;
                    foreach (var ta in mantenimientoTarea)
                    {
                        if (i.tarea == ta.SecuencialTarea && i.fecha == ta.Fecha.ToString("dd/MM/yyyy"))
                        {
                            countTareas++;
                        }
                    }
                    foreach (var ti in mantenimientoTicket)
                    {
                        if (i.numero == ti.SecuencialTicket && i.fecha == ti.Fecha.ToString("dd/MM/yyyy"))
                        {
                            countTickets++;
                        }
                    }
                    if (countTareas > 0 || countTickets > 0)
                    {
                        tick.Remove(i);
                    }
                }
                tickets = tick;

                var ticketsAdd = db.TicketMantenimientoAgregar.Where(s => s.Fecha >= fechaInicio && s.Fecha < fechaFin).ToList();
                var clienteAdd = db.Cliente.Where(s => s.Secuencial == idCliente).FirstOrDefault();
                var ticketAgregados = ticketsAdd.Where(s => s.Cliente == clienteAdd.Codigo + "-" + clienteAdd.Descripcion).ToList();
                foreach (var c in ticketAgregados)
                {
                    tickets.Add(new { tarea = -1, cliente = c.Cliente, numero = c.TicketTarea, detalle = c.Detalle, reportado = c.Reportado, asignado = c.Tecnico, fecha = c.Fecha.ToString("dd/MM/yyyy"), fechaComparacion = fechaInicio, categoria = "", categoriaRevisada = "", estado = c.Estado, tiempo = (double)c.Tiempo });
                }
                tickets = tickets.OrderBy(s => s.fecha).ToList();

                var contratosPorCliente = (from mt in db.MotivoTrabajo
                                           join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
                                           join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
                                           where mt.EstaActivo == 1 && cl.Secuencial == idCliente && tt.Codigo == "PENDIENTES"
                                           orderby mt.Secuencial
                                           select new
                                           {
                                               horas = mt.HorasMes,
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
                                               fecha = mt.FechaInicio
                                           }).ToList();

                var contratoMantenimiento = contratosPorCliente.Where(c => c.estado != "CERRADO").OrderByDescending(s => s.fecha).FirstOrDefault();
                var horasMensuales = (contratoMantenimiento != null ? contratoMantenimiento.horas : 0) * (fechaFin.AddDays(-1).Month - fechaInicio.Month + 1);
                var totalhoras = tickets.Sum(s => s.tiempo);

                List<string> destinatarioCorreos = Utiles.CorreoPorGrupoEmail("COORD");
                foreach (var personaCliente in db.Cliente.Where(s => s.Secuencial == idCliente).FirstOrDefault().persona_cliente)
                {
                    var usuario = personaCliente.persona.usuario.Where(s => s.EstaActivo == 1).FirstOrDefault();
                    if (usuario != null)
                        destinatarioCorreos.Add(usuario.Email);
                }

                var comentarioMantenimiento = db.ComentarioHorasMantenimiento.Where(s => s.SecuencialCliente == idCliente && s.Fecha == fechaInicioMantenimiento).FirstOrDefault();

                var resp = new
                {
                    success = true,
                    tickets = tickets,
                    horas = horasMensuales,
                    total = totalhoras,
                    horasRestantes = horasMensuales - totalhoras,
                    destinatarios = destinatarioCorreos,
                    comentario = comentarioMantenimiento != null ? comentarioMantenimiento.SecuencialComentarioMantenimiento : 0
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult EditarMantenimientoTicket(int idTicket, int idTarea, int tiempo, string fecha = "", bool esTicket = true, string cliente = "")
        {
            try
            {
                System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("es-ES");
                DateTime fechaMantenimiento = DateTime.Today;
                if (fecha == "")
                {
                    throw new Exception("Error al ingresar la fecha");
                }
                fechaMantenimiento = DateTime.Parse(fecha, cultureinfo);

                if (idTarea == -1)
                {
                    if (esTicket)
                    {
                        TicketMantenimientoAgregar ticketMantenimientoAgregar = db.TicketMantenimientoAgregar.Where(s => s.Fecha == fechaMantenimiento && s.Cliente == cliente && s.TicketTarea == idTicket).FirstOrDefault();
                        ticketMantenimientoAgregar.Tiempo = tiempo;
                    }
                    else
                    {
                        TicketMantenimientoAgregar ticketMantenimientoAgregar = db.TicketMantenimientoAgregar.Where(s => s.Fecha == fechaMantenimiento && s.Cliente == cliente && s.TicketTarea == 0).FirstOrDefault();
                        ticketMantenimientoAgregar.Tiempo = tiempo;
                    }
                }
                else
                {
                    if (esTicket)
                    {

                        TicketsMantenimiento ticket = db.TicketsMantenimiento.Where(t => t.Fecha == fechaMantenimiento && t.SecuencialTicket == idTicket).FirstOrDefault();
                        if (ticket != null)
                        {
                            ticket.Tiempo = tiempo;
                        }
                        else
                        {
                            ticket = new TicketsMantenimiento();
                            ticket.SecuencialTicket = idTicket;
                            ticket.Tiempo = tiempo;
                            ticket.Fecha = fechaMantenimiento;
                            db.TicketsMantenimiento.Add(ticket);
                        }
                    }
                    else
                    {
                        TareaMantenimiento tarea = db.TareaMantenimiento.Where(t => t.Fecha == fechaMantenimiento && t.SecuencialTarea == idTarea).FirstOrDefault();
                        if (tarea != null)
                        {
                            tarea.Tiempo = tiempo;
                        }
                        else
                        {
                            tarea = new TareaMantenimiento();
                            tarea.SecuencialTarea = idTarea;
                            tarea.Tiempo = tiempo;
                            tarea.Fecha = fechaMantenimiento;
                            db.TareaMantenimiento.Add(tarea);
                        }
                    }
                }

                db.SaveChanges();

                var resp = new
                {
                    success = true,
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult AgregarTicketTarea(string cliente = "", int noReporteTicket = -1, string detalleReporteTicket = "", string reportadoReporteTicket = "", string tecnicoReporteTicket = "", string fecha = "", string estadoReporteTicket = "...Seleccione", int tiempo = -1)
        {
            try
            {
                System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("es-ES");
                DateTime fechaMantenimiento = DateTime.Today;
                if (cliente == "Seleccione..." || noReporteTicket == -1 || detalleReporteTicket == "" || reportadoReporteTicket == "Seleccione..." || tecnicoReporteTicket == "" || fecha == "" || estadoReporteTicket == "Seleccione..." || tiempo == -1)
                {
                    throw new Exception("Debe llenar todos los campos");
                }
                fechaMantenimiento = DateTime.Parse(fecha, cultureinfo);
                DateTime fechaInicio = new DateTime(fechaMantenimiento.Year, fechaMantenimiento.Month, 1);
                DateTime fechaFin = fechaInicio.AddMonths(1);

                var ticketTarea = db.TicketMantenimientoAgregar.Where(s => s.Fecha >= fechaInicio && s.Fecha < fechaFin).ToList();
                var ticketT = ticketTarea.Where(s => s.TicketTarea == noReporteTicket && s.Fecha == fechaMantenimiento && s.Cliente == cliente).FirstOrDefault();
                if (ticketT == null)
                {
                    TicketMantenimientoAgregar ticketMantenimientoAgregar = new TicketMantenimientoAgregar();
                    ticketMantenimientoAgregar.Cliente = cliente;
                    ticketMantenimientoAgregar.Detalle = detalleReporteTicket;
                    ticketMantenimientoAgregar.Estado = estadoReporteTicket;
                    ticketMantenimientoAgregar.Fecha = fechaMantenimiento;
                    ticketMantenimientoAgregar.Reportado = reportadoReporteTicket;
                    ticketMantenimientoAgregar.Tecnico = tecnicoReporteTicket;
                    ticketMantenimientoAgregar.TicketTarea = noReporteTicket;
                    ticketMantenimientoAgregar.Tiempo = tiempo;
                    db.TicketMantenimientoAgregar.Add(ticketMantenimientoAgregar);
                }
                else
                {
                    throw new Exception("Ya se ingresó un Ticket o Tarea en esa fecha para ese cliente");
                }
                db.SaveChanges();

                var resp = new
                {
                    success = true,
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        [HttpPost]
        public ActionResult AgregarComentarioHoras(int cliente = 0, string fecha = "", int comentario = 0)
        {
            try
            {
                System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("es-ES");
                DateTime fechaMantenimiento = DateTime.Today;
                if (cliente == 0 || fecha == "" || comentario == 0)
                {
                    throw new Exception("Debe escoger el cliente, la fecha inicio y el comentario");
                }
                fechaMantenimiento = DateTime.Parse(fecha, cultureinfo);
                DateTime fechaInicio = new DateTime(fechaMantenimiento.Year, fechaMantenimiento.Month, 1);

                var ticket = db.ComentarioHorasMantenimiento.Where(s => s.Fecha == fechaInicio && s.SecuencialCliente == cliente).FirstOrDefault();
                if (ticket == null)
                {
                    ComentarioHorasMantenimiento comentarioHorasMantenimiento = new ComentarioHorasMantenimiento();
                    comentarioHorasMantenimiento.SecuencialCliente = cliente;
                    comentarioHorasMantenimiento.Fecha = fechaInicio;
                    comentarioHorasMantenimiento.SecuencialComentarioMantenimiento = comentario;
                    db.ComentarioHorasMantenimiento.Add(comentarioHorasMantenimiento);
                }
                else
                {
                    ticket.SecuencialComentarioMantenimiento = comentario;
                }
                db.SaveChanges();

                var resp = new
                {
                    success = true,
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult EliminarMantenimientoTicket(int numeroTticket, int secuencialTarea, string fecha = "", bool esTicket = true, string cliente = "")
        {
            try
            {
                System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("es-ES");
                DateTime fechaMantenimiento = DateTime.Today;
                if (fecha == "")
                {
                    throw new Exception("Error al ingresar la fecha");
                }
                fechaMantenimiento = DateTime.Parse(fecha, cultureinfo);

                if (secuencialTarea == -1)
                {
                    if (esTicket)
                    {
                        TicketMantenimientoAgregar ticketMantenimientoAgregar = db.TicketMantenimientoAgregar.Where(s => s.Fecha == fechaMantenimiento && s.Cliente == cliente && s.TicketTarea == numeroTticket).FirstOrDefault();
                        db.TicketMantenimientoAgregar.Remove(ticketMantenimientoAgregar);
                    }
                    else
                    {
                        TicketMantenimientoAgregar ticketMantenimientoAgregar = db.TicketMantenimientoAgregar.Where(s => s.Fecha == fechaMantenimiento && s.Cliente == cliente && s.TicketTarea == 0).FirstOrDefault();
                        db.TicketMantenimientoAgregar.Remove(ticketMantenimientoAgregar);
                    }
                }
                else
                {
                    if (esTicket)
                    {

                        TicketMantenimientoBorar ticket = db.TicketMantenimientoBorar.Where(t => t.Fecha == fechaMantenimiento && t.SecuencialTicket == numeroTticket).FirstOrDefault();
                        if (ticket == null)
                        {
                            ticket = new TicketMantenimientoBorar();
                            ticket.SecuencialTicket = numeroTticket;
                            ticket.Fecha = fechaMantenimiento;
                            db.TicketMantenimientoBorar.Add(ticket);
                        }
                    }
                    else
                    {
                        TareaMantenimientoBorrar tarea = db.TareaMantenimientoBorrar.Where(t => t.Fecha == fechaMantenimiento && t.SecuencialTarea == secuencialTarea).FirstOrDefault();
                        if (tarea == null)
                        {
                            tarea = new TareaMantenimientoBorrar();
                            tarea.SecuencialTarea = secuencialTarea;
                            tarea.Fecha = fechaMantenimiento;
                            db.TareaMantenimientoBorrar.Add(tarea);
                        }
                    }
                }
                db.SaveChanges();

                var resp = new
                {
                    success = true,
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

        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET")]
        [HttpPost]
        public ActionResult EliminarAdjuntoTicket(int secuencial = 0)
        {
            try
            {
                if (secuencial != 0)
                {
                    AdjuntoTicket adjunto = db.AdjuntoTicket.Where(s => s.Secuencial == secuencial).FirstOrDefault();
                    if (adjunto != null)
                    {
                        db.AdjuntoTicket.Remove(adjunto);
                    }
                    else
                    {
                        throw new Exception("No se encuentra el archivo adjunto");
                    }
                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("No se ha seleccionado ningún archivo adjunto");
                }
                var resp = new
                {
                    success = true,
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

        //ENVIAR EMAIL REPORTE TICKETS
        [HttpPost]
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        public ActionResult EnviarEmailReporteTickets(string destinatariosEmail, string asuntoEmail, string comentarioEmail, string tickets)
        {
            try
            {
                var s = new JavaScriptSerializer();
                var jsonObj = s.Deserialize<dynamic>(tickets);

                var destinatarios = destinatariosEmail.Replace(',', ';');
                List<string> usuariosDestinos = new List<string>();
                Regex rgx = new Regex(@"^(([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)(\s*;\s*|\s*$))*$");
                if (!rgx.IsMatch(destinatarios))
                {
                    throw new Exception("Debe ingresar una lista de correos válida separados por coma ó punto y coma");
                };
                string[] emails = destinatarios.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var email in emails)
                {
                    usuariosDestinos.Add(email);
                }
                string htmlMail = @"<div class='textoCuerpo'><br/>
                                <table>
                                    <thead>";
                htmlMail += "<tr>" +
                                "<th> No.Ticket </th>" +
                                "<th> Detalle </th>" +
                                "<th> Reportado_Por </th>" +
                                "<th> Técnico_Asignado </th>" +
                                "<th> Fecha </th>" +
                                "<th> Estado </th>" +
                                "<th> Tiempo </th>" +
                            "</tr>" +
                       "</thead>";
                htmlMail += "<tbody>";

                for (int i = 0; i < jsonObj.Length; i++)
                {
                    htmlMail += "<tr>" +
                                    "<td>" + jsonObj[i]["numero"] + "</td>" +
                                    "<td>" + jsonObj[i]["detalle"] + "</td>" +
                                    "<td>" + jsonObj[i]["reportado"] + "</td>" +
                                    "<td>" + jsonObj[i]["asignado"] + "</td>" +
                                    "<td>" + jsonObj[i]["fecha"] + "</td>" +
                                    "<td>" + jsonObj[i]["estado"] + "</td>" +
                                    "<td>" + jsonObj[i]["tiempo"] + "</td>" +
                                "</tr>";
                }
                htmlMail += "</tbody></table><br/></div>";

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
                                               table th {
                                                    border: 1px solid black;
                                                    border-collapse: collapse;
                                                    font-size: 8pt;
                                                    background: #B0C4DE;
                                                    font-family: ""Calibri"", sans-serif;
                                               }
                                               table, td {
                                                    border: 1px solid black;
                                                    border-collapse: collapse;
                                                    font-size: 8pt;
                                                    background: #eeeedd;
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

                string htmlFinal = htmlCss + htmlMail;

                //await EnviarEmailPorApi(usuariosDestinos.ToArray(), asuntoEmail, htmlFinal);
                Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), htmlFinal, asuntoEmail);

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

        //CATALOGOS UTILIZADOS PARA LA GESTION DE LOS TICKETS
        [HttpPost]
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
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
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        public ActionResult DarPrioridadesTickets()
        {
            try
            {
                var prioridades = (from pt in db.PrioridadTicket
                                   where pt.EstaActiva == 1
                                   orderby pt.Secuencial ascending
                                   select new
                                   {
                                       id = pt.Secuencial,
                                       name = pt.Codigo
                                   }).ToList();

                var resp = new
                {
                    success = true,
                    prioridades = prioridades
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
        [Authorize(Roles = "COORDINADOR, ADMIN, TICKET, GESTOR")]
        public ActionResult DarCategoriasTickets()
        {
            try
            {
                var categorias = (from ct in db.CategoriaTicket
                                  where ct.EstaActiva == 1
                                  orderby ct.Codigo ascending
                                  select new
                                  {
                                      id = ct.Secuencial,
                                      name = ct.Codigo
                                  }).ToList();

                var resp = new
                {
                    success = true,
                    categorias = categorias
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
        [Authorize(Roles = "ADMIN, TICKET, GESTOR")]
        public ActionResult DarInformacionGaugesTicket()
        {
            try
            {
                var gaugesHabilitados = (from et in db.EstadoTicket
                                         join etpa in db.EstadoTicketProximaActividad on et.Secuencial equals etpa.SecuencialEstadoTicket
                                         join pa in db.ProximaActividad on etpa.SecuencialProximaActividad equals pa.Secuencial
                                         where et.EstaActivo == 1 && etpa.EstaActivo == 1 && pa.EstaActivo == 1 && etpa.TieneGauge == 1
                                         select new
                                         {
                                             SecuencialEstadoTicket = et.Secuencial,
                                             SecuencialProximaActividad = pa.Secuencial,
                                             estadoTicket = et.Codigo,
                                             proximaActividad = pa.Codigo,
                                             minimo = etpa.Minimo,
                                             maximo = etpa.Maximo,
                                             medio = etpa.ValorMedio,
                                             alto = etpa.ValorAlto
                                         }).ToList();


                List<object> listaGauges = new List<object>();

                foreach (var gauge in gaugesHabilitados)
                {
                    int cant = db.Ticket.Where(x => x.SecuencialEstadoTicket == gauge.SecuencialEstadoTicket && x.SecuencialProximaActividad == gauge.SecuencialProximaActividad).Count();
                    string texto = cant < gauge.medio ? "BAJO" : cant < gauge.alto ? "MEDIO" : "ALTO";

                    listaGauges.Add(new
                    {
                        nombre = gauge.estadoTicket + "<br />" + gauge.proximaActividad,
                        minimo = gauge.minimo,
                        maximo = gauge.maximo,
                        textoEstado = texto,
                        indicador = cant
                    });
                }

                var resp = new
                {
                    success = true,
                    gaugesDataList = listaGauges
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

        public string GenerarExcelEstimaciones(int id)
        {
            using (SLDocument sl = new SLDocument())
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
                if (estimacion.ticket.Fecha.HasValue)
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
                foreach (var item in entregables.Select((value, index) => new { value, index }))
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

                    foreach (var it in item.value.detalles.Select((v, i) => new { v, i }))
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

        private async Task EnviarEmailPorApi(string[] destinatarios, string asunto, string cuerpoHtml, string[] adjuntos = null)
        {
            string apiKey = "54b0d60f-0464-4e86-9c49-f85d33800126";
            string url = "https://messagebroker.sifizsoft.com/Email/AddEmailToQueue";

            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-Api-Key", apiKey);
                client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

                var listaDestinatarios = destinatarios.Select(d => new { nombre = "", correoElectronico = d }).ToList();

                var bodyObj = new
                {
                    to = listaDestinatarios,
                    toCC = new List<string>(),
                    tittle = asunto,
                    body = cuerpoHtml,
                    attachments = adjuntos ?? new string[0],
                    areTheyPhysicalAttachments = true
                };

                var bodyContent = JsonConvert.SerializeObject(bodyObj);
                Console.WriteLine("bodyContent: " + bodyContent); // Agregar esta línea para imprimir el contenido de la solicitud en la consola.

                var content = new StringContent(bodyContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error al enviar el correo: " + response.ReasonPhrase + ":" + content);
                }
            }
        }

        private int CalculoHorasLaborables(DateTime fecha)
        {
            DateTime fechaFinal = DateTime.Now;
            DateTime fechaInicial = fecha;

            DateTime fechaActual = new DateTime(fechaFinal.Year, fechaFinal.Month, fechaFinal.Day, fechaFinal.Hour, 0, 0);
            DateTime fechaTicket = new DateTime(fechaInicial.Year, fechaInicial.Month, fechaInicial.Day, fechaInicial.Hour, 0, 0);

            int horasLaborables = 0;
            fechaTicket = fechaTicket.AddHours(1);

            while (fechaTicket <= fechaActual)
            {
                DateTime horaInicio = new DateTime(fechaTicket.Year, fechaTicket.Month, fechaTicket.Day, 8, 30, 0);
                DateTime horaFin = new DateTime(fechaTicket.Year, fechaTicket.Month, fechaTicket.Day, 17, 30, 0);
                DateTime horaAlmuerzoInicio = new DateTime(fechaTicket.Year, fechaTicket.Month, fechaTicket.Day, 13, 0, 0);
                DateTime horaAlmuerzoFin = new DateTime(fechaTicket.Year, fechaTicket.Month, fechaTicket.Day, 14, 0, 0);

                if (fechaTicket.DayOfWeek != DayOfWeek.Saturday && fechaTicket.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (fechaTicket > horaInicio && fechaTicket <= horaAlmuerzoInicio)
                    {
                        horasLaborables++;
                    }
                    else if (fechaTicket > horaAlmuerzoFin && fechaTicket <= horaFin)
                    {
                        horasLaborables++;
                    }
                }

                fechaTicket = fechaTicket.AddHours(1);
            }
            if (fechaFinal.Minute >= fechaInicial.Minute && (fechaFinal - fechaInicial).TotalHours >= 1)
            {
                TimeSpan horaFinal = fechaFinal.TimeOfDay;
                TimeSpan horaInicial = fechaInicial.TimeOfDay;
                if (fechaFinal.DayOfWeek != DayOfWeek.Saturday &&
                   fechaFinal.DayOfWeek != DayOfWeek.Sunday &&
                   horaFinal >= new TimeSpan(8, 30, 0) &&
                   horaFinal < new TimeSpan(13, 0, 0) &&
                   horaFinal >= new TimeSpan(14, 0, 0) &&
                   horaFinal < new TimeSpan(17, 30, 0) &&
                   fechaInicial.DayOfWeek != DayOfWeek.Saturday &&
                   fechaInicial.DayOfWeek != DayOfWeek.Sunday &&
                   horaInicial >= new TimeSpan(8, 30, 0) &&
                   horaInicial < new TimeSpan(13, 0, 0) &&
                   horaInicial >= new TimeSpan(14, 0, 0) &&
                   horaInicial < new TimeSpan(17, 30, 0))
                    horasLaborables++;
            }
            return horasLaborables;
        }

    }
}