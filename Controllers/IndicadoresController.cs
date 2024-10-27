using SifizPlanning.Models;
using SifizPlanning.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SifizPlanning.Controllers
{
    public class IndicadoresController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();
        // GET: Indicadores
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarInfotickets(string fechaInicio, string fechaFin, string fechaHasta, string cliente)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = new DateTime();
                DateTime fFin = new DateTime();
                DateTime fHasta = new DateTime();
                var infoTickets = new List<InfoTickets>();

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicio = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
                    fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                // Verificar si fechaFin es nula o vacía
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFin = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFin = DateTime.Now;
                }

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaHasta))
                {
                    string[] fechas = fechaHasta.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fHasta = new DateTime(anno, mes, dia);

                    infoTickets = db.InfoTickets.AsNoTracking()
                                             .Where(it => it.FechaIngreso <= fHasta)
                                             .ToList();
                }
                else
                {
                    infoTickets = db.InfoTickets.AsNoTracking()
                                             .Where(it => it.FechaIngreso >= fInicio && it.FechaIngreso <= fFin)
                                             .ToList();
                }

                if (!string.IsNullOrEmpty(cliente))
                {
                    infoTickets = infoTickets.Where(it => it.Cliente == cliente).ToList();
                }

                var resp = new
                {
                    success = true,
                    infoTickets = infoTickets
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarNuevosTickets(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = new DateTime();
                DateTime fFin = new DateTime();

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicio = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
                    fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                // Verificar si fechaFin es nula o vacía
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFin = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFin = DateTime.Now;
                }
                fFin = fFin.AddDays(1);

                var ticketsQueryN = db.InfoTickets.AsNoTracking()
                                             .Where(it => it.FechaIngreso.Value >= fInicio && it.FechaIngreso.Value <= fFin && it.Estado != "ANULADO" && it.Estado != "RECHAZADO")
                                             .ToList();

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsListN = ticketsQueryN.ToList();

                var groupedTickets = ticketsListN
                    .GroupBy(ticket => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        ticket.FechaIngreso.Value,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday))
                    .Select(g => new
                    {
                        Semana = g.Key,
                        Cantidad = g.Count(),
                        Descripcion = "AL " + g.Max(t => t.FechaIngreso.Value).ToString("dd/MM/yyyy")
                    })
                    .OrderBy(x => x.Semana)
                    .ToList();

                var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTickets,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsCerrados(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = new DateTime();
                DateTime fFin = new DateTime();

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicio = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
                    fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                // Verificar si fechaFin es nula o vacía
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFin = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFin = DateTime.Now;
                }
                fFin = fFin.AddDays(1);

                var ticketsQueryC = from ticket in db.InfoTickets
                                    where ticket.FechaIngreso.Value >= fInicio
                                       && ticket.FechaIngreso.Value <= fFin
                                       && ticket.Estado == "CERRADO"
                                    select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsListC = ticketsQueryC.ToList();

                var groupedTicketsC = ticketsListC
                    .GroupBy(ticket => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        ticket.FechaIngreso.Value,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday))
                    .Select(g => new
                    {
                        Semana = g.Key,
                        Cantidad = g.Count(),
                        Descripcion = "AL " + g.Max(t => t.FechaIngreso.Value).ToString("dd/MM/yyyy")
                    })
                    .OrderBy(x => x.Semana).Distinct()
                    .ToList();

                var totalCantidades = groupedTicketsC.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTicketsC,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorAplica(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = new DateTime();
                DateTime fFin = new DateTime();

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicio = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
                    fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                // Verificar si fechaFin es nula o vacía
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFin = new DateTime(anno, mes, dia);
                    fFin = fFin.AddDays(1);
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFin = DateTime.Now;
                }

                var ticketsQueryPA = (from ticket in db.InfoTickets
                                      where ticket.FechaIngreso != null
                                         && ticket.FechaIngreso.Value >= fInicio
                                         && ticket.FechaIngreso.Value <= fFin
                                      select ticket).ToList();

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsListPA = ticketsQueryPA.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsListPA
                   .GroupBy(t => t.AplicaA) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Aplica = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
                   })
                   .OrderBy(x => x.Aplica) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
                   .ToList();

                var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTickets,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorMantenimiento(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = new DateTime();
                DateTime fFin = new DateTime();

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicio = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
                    fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                // Verificar si fechaFin es nula o vacía
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFin = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFin = DateTime.Now;
                }

                var ticketsQueryPM = (from ticket in db.InfoTickets
                                      where ticket.FechaIngreso != null
                                         && ticket.FechaIngreso.Value >= fInicio
                                         && ticket.FechaIngreso.Value <= fFin
                                         && ticket.Tipo == "MANTENIMIENTO"
                                      select ticket).ToList();

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsListPM = ticketsQueryPM.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsListPM
                   .GroupBy(t => t.Cliente) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Cliente = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
                   })
                   .OrderByDescending(x => x.Cantidad) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
                   .ToList();

                var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTickets,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorGarantia(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = new DateTime();
                DateTime fFin = new DateTime();

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicio = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
                    fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                // Verificar si fechaFin es nula o vacía
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFin = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFin = DateTime.Now;
                }

                var ticketsQueryPG = (from ticket in db.InfoTickets
                                      where ticket.FechaIngreso != null
                                         && ticket.FechaIngreso.Value >= fInicio
                                         && ticket.FechaIngreso.Value <= fFin
                                         && ticket.Tipo == "GARANTÍA TÉCNICA"
                                      select ticket).ToList();

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsListPG = ticketsQueryPG.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsListPG
                   .GroupBy(t => t.Cliente) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Cliente = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
                   })
                   .OrderByDescending(x => x.Cantidad) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
                   .ToList();

                var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTickets,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorReqNuevo(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = new DateTime();
                DateTime fFin = new DateTime();

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicio = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
                    fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                // Verificar si fechaFin es nula o vacía
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFin = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFin = DateTime.Now;
                }

                var ticketsQueryPRN = (from ticket in db.InfoTickets
                                       where ticket.FechaIngreso != null
                                          && ticket.FechaIngreso.Value >= fInicio
                                          && ticket.FechaIngreso.Value <= fFin
                                          && ticket.Tipo == "REQUERIMIENTO NUEVO"
                                       select ticket).ToList();

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsListPRN = ticketsQueryPRN.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsListPRN
                   .GroupBy(t => t.Cliente) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Cliente = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
                   })
                   .OrderByDescending(x => x.Cantidad) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
                   .ToList();

                var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTickets,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorGestor(string fechaInicio, string fechaFin, string gestor)
        {
            try
            {
                DateTime fInicio = new DateTime();
                DateTime fFin = new DateTime();

                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicio = new DateTime(anno, mes, dia);
                }
                else
                {
                    fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFin = new DateTime(anno, mes, dia);
                }
                else
                {
                    fFin = DateTime.Now;
                }

                int gestorServicio = Int32.Parse(gestor);
                List<string> clientes = db.GestorServicios
                                        .Where(s => s.SecuencialColaborador == gestorServicio)
                                        .Select(s => s.cliente.Descripcion)
                                        .ToList();

                var ticketsList = (from ticket in db.InfoTickets
                                   where ticket.FechaIngreso != null
                                      && ticket.FechaIngreso.Value >= fInicio
                                      && ticket.FechaIngreso.Value <= fFin
                                      && clientes.Contains(ticket.Cliente)
                                   select ticket).ToList();

                var ticketsAgrupados = ticketsList
                    .GroupBy(ticket => new { Anio = ticket.FechaIngreso.Value.Year, Mes = ticket.FechaIngreso.Value.Month, ticket.Cliente })
                    .Select(group => new { cliente = group.Key.Cliente, mes = group.Key.Mes, anio = group.Key.Anio, cantidad = group.Count() })
                    .ToList();

                var resp = new
                {
                    success = true,
                    tickets = ticketsAgrupados
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


        //*******************************************************************************TICKETS AL DIA********************************************************************************

        // Función para obtener el número de semana
        Func<DateTime, int> GetWeekNumber = (date) =>
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        };

        [HttpPost]
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsEnGestionAlDia(int cliente)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fActual = DateTime.Today;
                String nombreCliente = "";

                if (cliente != 0)
                {
                    nombreCliente = (from cli in db.Cliente where cli.Secuencial == cliente select cli.Descripcion).FirstOrDefault();
                }

                var ticketsQueryTEGAD = (from ticket in db.Ticket
                                         join et in db.EstadoTicket on ticket.SecuencialEstadoTicket equals et.Secuencial
                                         join pc in db.Persona_Cliente on ticket.SecuencialPersona_Cliente equals pc.SecuencialPersona
                                         join c in db.Cliente on pc.SecuencialCliente equals c.Secuencial
                                         where ticket.FechaCreado != null
                                            && ticket.FechaCreado <= fActual
                                            && et.Codigo != "CERRADO"
                                            && et.Codigo != "ANULADO"
                                            && et.Codigo != "RECHAZADO"
                                            && (c.Descripcion == nombreCliente || nombreCliente == "")
                                         select ticket).ToList();

                List<Ticket> ticketsListTEGAD = ticketsQueryTEGAD.ToList();

                // Encontrar la fecha más antigua y la más reciente en los tickets
                var fechaInicial = ticketsQueryTEGAD.Min(t => t.FechaCreado);

                // Agrupar los tickets por año y semana
                var groupedTickets = ticketsQueryTEGAD
                    .GroupBy(ticket => new
                    {
                        Year = ticket.FechaCreado.Year,
                        Week = GetWeekNumber(ticket.FechaCreado)
                    })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Week,
                        Cantidad = g.Count(),
                        UltimaFecha = g.Max(t => t.FechaCreado),
                        Tickets = g.Select(t => new
                        {
                            FechaCreado = t.FechaCreado.ToString("dd/MM/yyyy"), // Format the date here
                            t.Secuencial,
                            t.Detalle
                        }).ToList()
                    })
                    .ToList();

                // Crear una lista de todas las semanas desde la fecha inicial hasta la actual
                var allWeeks = Enumerable.Range(0, (int)(fActual - fechaInicial).TotalDays / 7 + 1)
                    .Select(i => fechaInicial.AddDays(i * 7))
                    .Select(date => new
                    {
                        Year = date.Year,
                        Week = GetWeekNumber(date),
                        Date = date
                    })
                    .Distinct()
                    .ToList();

                // Combinar todas las semanas con los tickets agrupados
                var groupedTicketsTEGAD = allWeeks
                .GroupJoin(groupedTickets,
                    aw => new { aw.Year, aw.Week },
                    gt => new { gt.Year, gt.Week },
                    (aw, gt) => new
                    {
                        aw.Year,
                        aw.Week,
                        WeekIdentifier = $"{aw.Week}/{aw.Year}", // Create a unique identifier
                        Cantidad = gt.Sum(x => x.Cantidad),
                        UltimaFecha = gt.Any() ? gt.Max(x => x.UltimaFecha) : aw.Date.AddDays(6),
                        Tickets = gt.SelectMany(x => x.Tickets).ToList()
                    })
                .Where(x => x.Cantidad > 0) // Exclude weeks with quantity 0
                .OrderByDescending(x => x.Cantidad)
                .ThenByDescending(x => x.Cantidad)
                .Select(x => new
                {
                    Semana = x.Week,
                    Anno = x.Year,
                    WeekIdentifier = x.WeekIdentifier, // Include the unique identifier
                    Cantidad = x.Cantidad,
                    Descripcion = $"AL {x.UltimaFecha:dd/MM/yyyy}",
                    Tickets = x.Tickets
                })
                .ToList();


                var totalCantidadesTEGAD = groupedTicketsTEGAD.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTicketsTEGAD,
                    totalCantidades = totalCantidadesTEGAD
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorCategoriasAlDia(int cliente)
        {
            try
            {
                DateTime fActual = DateTime.Today;
                String nombreCliente = "";

                if (cliente != 0)
                {
                    nombreCliente = (from cli in db.Cliente where cli.Secuencial == cliente select cli.Descripcion).FirstOrDefault();
                }

                var ticketsQueryPCAD = (from ticket in db.Ticket
                                        join et in db.EstadoTicket on ticket.SecuencialEstadoTicket equals et.Secuencial
                                        join pc in db.Persona_Cliente on ticket.SecuencialPersona_Cliente equals pc.SecuencialPersona
                                        join c in db.Cliente on pc.SecuencialCliente equals c.Secuencial
                                        join ca in db.CategoriaTicket on ticket.SecuencialCategoriaTicket equals ca.Secuencial
                                        where ticket.FechaCreado != null
                                           && ticket.FechaCreado <= fActual
                                           && et.Codigo != "CERRADO"
                                           && et.Codigo != "ANULADO"
                                           && et.Codigo != "RECHAZADO"
                                           && (c.Descripcion == nombreCliente || nombreCliente == "")
                                        select new
                                        {
                                            Ticket = ticket,
                                            Tipo = ca.Codigo
                                        }).ToList();

                var ticketsListPCAD = ticketsQueryPCAD.ToList();

                // Encontrar la fecha más antigua y la más reciente en los tickets
                var fechaInicial = ticketsListPCAD.Min(t => t.Ticket.FechaCreado);

                // Agrupar los tickets por tipo
                var groupedTicketsPCAD = ticketsListPCAD
                   .GroupBy(t => t.Tipo)
                   .Select(g => new
                   {
                       Tipo = g.Key,
                       Cantidad = g.Count(),
                       Tickets = g.Select(t => new
                       {
                           FechaCreado = t.Ticket.FechaCreado.ToString("dd/MM/yyyy"), // Format the date here
                           t.Ticket.Secuencial,
                           t.Ticket.Detalle
                       }).ToList()
                   })
                   .OrderBy(x => x.Tipo)
                   .ToList();

                var totalCantidades = groupedTicketsPCAD.Sum(ticket => ticket.Cantidad);

                var categoriasPorcentajes = groupedTicketsPCAD.Select(g => new
                {
                    Categoria = g.Tipo,
                    Cantidad = g.Cantidad,
                    Porcentaje = CalcularPorcentage(g.Cantidad, totalCantidades),
                    Tickets = g.Tickets
                }).ToList();

                var resp = new
                {
                    success = true,
                    infoTickets = categoriasPorcentajes,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorEstadosAlDia(int cliente)
        {
            try
            {
                DateTime fActual = DateTime.Today;
                String nombreCliente = "";

                if (cliente != 0)
                {
                    nombreCliente = (from cli in db.Cliente where cli.Secuencial == cliente select cli.Descripcion).FirstOrDefault();
                }

                var ticketsQueryPEAD = (from ticket in db.Ticket
                                        join et in db.EstadoTicket on ticket.SecuencialEstadoTicket equals et.Secuencial
                                        join pc in db.Persona_Cliente on ticket.SecuencialPersona_Cliente equals pc.SecuencialPersona
                                        join c in db.Cliente on pc.SecuencialCliente equals c.Secuencial
                                        where ticket.FechaCreado != null
                                           && ticket.FechaCreado <= fActual
                                           && et.Codigo != "CERRADO"
                                           && et.Codigo != "ANULADO"
                                           && et.Codigo != "RECHAZADO"
                                           && (c.Descripcion == nombreCliente || nombreCliente == "")
                                        select new
                                        {
                                            Ticket = ticket,
                                            Estado = et.Codigo
                                        }).ToList();


                var ticketsListPEAD = ticketsQueryPEAD;

                var groupedTicketsPEAD = ticketsListPEAD
                   .GroupBy(t => t.Estado)
                   .Select(g => new
                   {
                       Estado = g.Key,
                       Cantidad = g.Count(),
                       Tickets = g.Select(t => new
                       {
                           FechaCreado = t.Ticket.FechaCreado.ToString("dd/MM/yyyy"), // Format the date here
                           t.Ticket.Secuencial,
                           t.Ticket.Detalle
                       })
                   })
                   .OrderBy(x => x.Estado)
                   .ToList();

                var totalCantidades = groupedTicketsPEAD.Sum(ticket => ticket.Cantidad);

                var estadosPorcentajes = groupedTicketsPEAD.Select(g => new
                {
                    Estado = g.Estado,
                    Cantidad = g.Cantidad,
                    Porcentaje = CalcularPorcentage(g.Cantidad, totalCantidades),
                    Tickets = g.Tickets
                }).ToList();

                var resp = new
                {
                    success = true,
                    infoTickets = estadosPorcentajes,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorClientesPorEstadoAlDia(int cliente)
        {
            try
            {
                DateTime fActual = DateTime.Today;
                String nombreCliente = "";

                if (cliente != 0)
                {
                    nombreCliente = (from cli in db.Cliente where cli.Secuencial == cliente select cli.Descripcion).FirstOrDefault();
                }

                var ticketsQueryPCEAD = (from ticket in db.Ticket
                                         join et in db.EstadoTicket on ticket.SecuencialEstadoTicket equals et.Secuencial
                                         join pc in db.Persona_Cliente on ticket.SecuencialPersona_Cliente equals pc.SecuencialPersona
                                         join c in db.Cliente on pc.SecuencialCliente equals c.Secuencial
                                         where ticket.FechaCreado != null
                                            && ticket.FechaCreado <= fActual
                                            && et.Codigo != "CERRADO"
                                            && et.Codigo != "ANULADO"
                                            && et.Codigo != "RECHAZADO"
                                            && (c.Descripcion == nombreCliente || nombreCliente == "")
                                         select new
                                         {
                                             Ticket = ticket,
                                             Cliente = c.Descripcion,
                                             Estado = et.Codigo
                                         }).ToList();

                var ticketsListPCEAD = ticketsQueryPCEAD;

                var groupedTicketsPCEAD = ticketsListPCEAD
                    .GroupBy(ticket => new { ticket.Cliente, ticket.Estado })
                    .Select(group => new
                    {
                        Cliente = group.Key.Cliente,
                        Estado = group.Key.Estado,
                        Cantidad = group.Count(),
                        Tickets = group.Select(t => new
                         {
                             FechaCreado = t.Ticket.FechaCreado.ToString("dd/MM/yyyy"), // Format the date here
                             t.Ticket.Secuencial,
                             t.Ticket.Detalle
                         })
                    }).Distinct()
                    .ToList();

                var totalCantidades = groupedTicketsPCEAD.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTicketsPCEAD,
                    totalCantidades = totalCantidades
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsAplicadosAlDia(int cliente)
        {
            try
            {
                DateTime fActual = DateTime.Today;
                String nombreCliente = "";

                if (cliente != 0)
                {
                    nombreCliente = (from cli in db.Cliente where cli.Secuencial == cliente select cli.Descripcion).FirstOrDefault();
                }

                var ticketsQueryTPAD = (from ticket in db.Ticket
                                        join et in db.EstadoTicket on ticket.SecuencialEstadoTicket equals et.Secuencial
                                        join pc in db.Persona_Cliente on ticket.SecuencialPersona_Cliente equals pc.SecuencialPersona
                                        join c in db.Cliente on pc.SecuencialCliente equals c.Secuencial
                                        join tvc in db.TicketVersionCliente on ticket.SecuencialTicketVersionCliente equals tvc.Secuencial
                                        where ticket.FechaCreado != null
                                           && ticket.FechaCreado <= fActual
                                           && et.Codigo != "CERRADO"
                                           && et.Codigo != "ANULADO"
                                           && et.Codigo != "RECHAZADO"
                                           && (c.Descripcion == nombreCliente || nombreCliente == "")
                                        select new
                                        {
                                            Ticket = ticket,
                                            AplicaA = tvc.Descripcion
                                        }).ToList();

                var ticketsListTPAD = ticketsQueryTPAD;

                var groupedTicketsTPAD = ticketsListTPAD
                   .GroupBy(t => t.AplicaA) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Aplica = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count(), // Cuenta la cantidad de tickets en cada grupo
                       Tickets = g.Select(t => new
                       {
                           FechaCreado = t.Ticket.FechaCreado.ToString("dd/MM/yyyy"), // Format the date here
                           t.Ticket.Secuencial,
                           t.Ticket.Detalle
                       })
                   })
                   .OrderBy(x => x.Aplica) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
                   .ToList();

                var totalCantidadesTPAD = groupedTicketsTPAD.Sum(ticket => ticket.Cantidad);

                var aplicasPorcentajes = groupedTicketsTPAD.Select(g => new
                {
                    Aplica = g.Aplica,
                    Cantidad = g.Cantidad,
                    Porcentaje = CalcularPorcentage(g.Cantidad, totalCantidadesTPAD),
                    Tickets = g.Tickets
                }).ToList();

                var resp = new
                {
                    success = true,
                    infoTickets = aplicasPorcentajes,
                    totalCantidades = totalCantidadesTPAD
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsRankingPorCategoriasAlDia()
        {
            try
            {
                DateTime fActual = DateTime.Today;

                // Consulta base
                var ticketsQuery = (from ticket in db.Ticket
                                    join et in db.EstadoTicket on ticket.SecuencialEstadoTicket equals et.Secuencial
                                    join pc in db.Persona_Cliente on ticket.SecuencialPersona_Cliente equals pc.SecuencialPersona
                                    join c in db.Cliente on pc.SecuencialCliente equals c.Secuencial
                                    join ca in db.CategoriaTicket on ticket.SecuencialCategoriaTicket equals ca.Secuencial
                                    where ticket.FechaCreado != null
                                       && ticket.FechaCreado <= fActual
                                       && et.Codigo != "CERRADO"
                                       && et.Codigo != "ANULADO"
                                       && et.Codigo != "RECHAZADO"
                                    select new
                                    {
                                        Ticket = ticket,
                                        Tipo = ca.Codigo,
                                        Cliente = c.Descripcion
                                    }).ToList();

                // Ejecutar la consulta
                var ticketsList = ticketsQuery.ToList();

                var ticketsPorCategorias = ticketsList
            .GroupBy(t => new { Tipo = t.Tipo, t.Cliente })
            .Select(g => new
            {
                Tipo = g.Key.Tipo,
                Cliente = g.Key.Cliente,
                Cantidad = g.Count(),
                Tickets = g.Select(t => new
                {
                    FechaCreado = t.Ticket.FechaCreado.ToString("dd/MM/yyyy"), // Format the date here
                    t.Ticket.Secuencial,
                    t.Ticket.Detalle
                })
            })
            .GroupBy(x => x.Tipo)
            .Select(g => new
            {
                Tipo = g.Key,
                Clientes = g.OrderByDescending(x => x.Cantidad)
                               .Take(10)
                               .Select(x => new
                               {
                                   Cliente = x.Cliente,
                                   Cantidad = x.Cantidad,
                                   Tickets = x.Tickets
                               })
                               .ToList(),
                Total = g.Sum(x => x.Cantidad)
            })
            .OrderBy(x => x.Tipo)
            .ToList();

                return Json(new
                {
                    success = true,
                    ticketsPorCategorias = ticketsPorCategorias,
                }, JsonRequestBehavior.AllowGet);
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


        //******************************************** Indicadores generales *********************************************

        [HttpPost]
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorAplicaIndicadoresGenerales(List<string> annos, List<string> meses)
        {
            try
            {
                var annosInt = new List<int>();
                var mesesInt = new List<int>();

                if (annos == null || !annos.Any())
                {
                    annosInt.Add(2024);
                }
                else
                {
                    annosInt = annos.Select(int.Parse).ToList();
                }

                if (meses == null || !meses.Any())
                {
                    mesesInt = Enumerable.Range(1, 12).ToList();
                }
                else
                {
                    mesesInt = meses.Select(int.Parse).ToList();
                }

                // Consulta base
                var ticketsQuery = db.InfoTickets.Where(t => t.FechaIngreso != null).Select(t => new
                {
                    AnnoFechaIngreso = t.FechaIngreso.Value.Year,
                    MesFechaIngreso = t.FechaIngreso.Value.Month,
                    Aplica = t.AplicaA
                });

                // Filtrar por años y meses seleccionados
                ticketsQuery = ticketsQuery.Where(t =>
                    annosInt.Contains(t.AnnoFechaIngreso) &&
                    mesesInt.Contains(t.MesFechaIngreso)
                );

                // Ejecutar la consulta
                var ticketsList = ticketsQuery.ToList();

                // Filtrar los resultados en memoria para asegurar la correspondencia año-mes
                ticketsList = ticketsList.Where(t =>
                    annosInt.Contains(t.AnnoFechaIngreso) &&
                    mesesInt.Contains(t.MesFechaIngreso) &&
                    t.Aplica != null
                ).ToList();

                // Agrupar por año y AplicaA
                var ticketsPorAnno = ticketsList
                    .GroupBy(t => new { Anno = t.AnnoFechaIngreso, t.Aplica })
                    .Select(g => new
                    {
                        Anno = g.Key.Anno,
                        Aplica = g.Key.Aplica,
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Anno)
                    .ThenBy(x => x.Aplica)
                    .ToList();

                // Agrupar por año, mes y AplicaA
                var ticketsPorAnnoMes = ticketsList
                    .GroupBy(t => new
                    {
                        Anno = t.AnnoFechaIngreso,
                        Mes = t.MesFechaIngreso,
                        t.Aplica
                    })
                    .Select(g => new
                    {
                        Anno = g.Key.Anno,
                        Mes = g.Key.Mes,
                        Aplica = g.Key.Aplica,
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Anno)
                    .ThenBy(x => x.Mes)
                    .ThenBy(x => x.Aplica)
                    .ToList();

                return Json(new
                {
                    success = true,
                    ticketsPorAnno = ticketsPorAnno,
                    ticketsPorAnnoMes = ticketsPorAnnoMes
                }, JsonRequestBehavior.AllowGet);
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorEstadosIndicadoresGenerales(List<string> annos, List<string> meses)
        {
            try
            {
                var annosInt = new List<int>();
                var mesesInt = new List<int>();

                if (annos == null || !annos.Any())
                {
                    annosInt.Add(2024);
                }
                else
                {
                    annosInt = annos.Select(int.Parse).ToList();
                }

                if (meses == null || !meses.Any())
                {
                    mesesInt = Enumerable.Range(1, 12).ToList();
                }
                else
                {
                    mesesInt = meses.Select(int.Parse).ToList();
                }

                // Consulta base
                var ticketsQuery = db.InfoTickets.Where(t => t.FechaIngreso != null).Select(t => new
                {
                    AnnoFechaIngreso = t.FechaIngreso.Value.Year,
                    MesFechaIngreso = t.FechaIngreso.Value.Month,
                    Estado = t.Estado
                });

                // Filtrar por años y meses seleccionados
                ticketsQuery = ticketsQuery.Where(t =>
                    annosInt.Contains(t.AnnoFechaIngreso) &&
                    mesesInt.Contains(t.MesFechaIngreso)
                );

                // Ejecutar la consulta
                var ticketsList = ticketsQuery.ToList();

                // Filtrar los resultados en memoria para asegurar la correspondencia año-mes
                ticketsList = ticketsList.Where(t =>
                    annosInt.Contains(t.AnnoFechaIngreso) &&
                    mesesInt.Contains(t.MesFechaIngreso) &&
                    t.Estado != null
                ).ToList();

                // Agrupar por año y AplicaA
                var ticketsPorAnnoEstados = ticketsList
                    .GroupBy(t => new { Anno = t.AnnoFechaIngreso, t.Estado })
                    .Select(g => new
                    {
                        Anno = g.Key.Anno,
                        Estado = g.Key.Estado,
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Anno)
                    .ThenBy(x => x.Estado)
                    .ToList();

                // Agrupar por año, mes y AplicaA
                var ticketsPorAnnoMesEstados = ticketsList
                    .GroupBy(t => new
                    {
                        Anno = t.AnnoFechaIngreso,
                        Mes = t.MesFechaIngreso,
                        t.Estado
                    })
                    .Select(g => new
                    {
                        Anno = g.Key.Anno,
                        Mes = g.Key.Mes,
                        Estado = g.Key.Estado,
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Anno)
                    .ThenBy(x => x.Mes)
                    .ThenBy(x => x.Estado)
                    .ToList();

                return Json(new
                {
                    success = true,
                    ticketsPorAnnoEstados = ticketsPorAnnoEstados,
                    ticketsPorAnnoMesEstados = ticketsPorAnnoMesEstados
                }, JsonRequestBehavior.AllowGet);
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorGarantiaIndicadoresGenerales(List<string> annos, List<string> meses)
        {
            try
            {
                var annosInt = new List<int>();
                var mesesInt = new List<int>();

                if (annos == null || !annos.Any())
                {
                    annosInt.Add(2024);
                }
                else
                {
                    annosInt = annos.Select(int.Parse).ToList();
                }

                if (meses == null || !meses.Any())
                {
                    mesesInt = Enumerable.Range(1, 12).ToList();
                }
                else
                {
                    mesesInt = meses.Select(int.Parse).ToList();
                }

                // Consulta base
                var ticketsQuery = db.InfoTickets.Where(t => t.FechaIngreso != null && t.Tipo == "GARANTÍA TÉCNICA").Select(t => new
                {
                    AnnoFechaIngreso = t.FechaIngreso.Value.Year,
                    MesFechaIngreso = t.FechaIngreso.Value.Month,
                    Cliente = t.Cliente,
                });

                // Filtrar por años y meses seleccionados
                ticketsQuery = ticketsQuery.Where(t =>
                    annosInt.Contains(t.AnnoFechaIngreso) &&
                    mesesInt.Contains(t.MesFechaIngreso)
                );

                // Ejecutar la consulta
                var ticketsList = ticketsQuery.ToList();

                // Filtrar los resultados en memoria para asegurar la correspondencia año-mes
                ticketsList = ticketsList.Where(t =>
                    annosInt.Contains(t.AnnoFechaIngreso) &&
                    mesesInt.Contains(t.MesFechaIngreso) &&
                    t.Cliente != null
                ).ToList();

                // Agrupar por año y AplicaA
                var ticketsPorAnnoGarantia = ticketsList
            .GroupBy(t => new { Anno = t.AnnoFechaIngreso, t.Cliente })
            .Select(g => new
            {
                Anno = g.Key.Anno,
                Cliente = g.Key.Cliente,
                Cantidad = g.Count()
            })
            .GroupBy(x => x.Anno)
            .Select(g => new
            {
                Anno = g.Key,
                Clientes = g.OrderByDescending(x => x.Cantidad)
                               .Take(10)
                               .Select(x => new
                               {
                                   Cliente = x.Cliente,
                                   Cantidad = x.Cantidad
                               })
                               .ToList(),
                Total = g.Sum(x => x.Cantidad)
            })
            .OrderBy(x => x.Anno)
            .ToList();

                // Agrupar por año, mes y Cliente, y obtener los top 10 por año y mes
                var ticketsPorAnnoMesGarantia = ticketsList
                 .GroupBy(t => new
                 {
                     Anno = t.AnnoFechaIngreso,
                     Mes = t.MesFechaIngreso,
                     t.Cliente
                 })
                 .Select(g => new
                 {
                     Anno = g.Key.Anno,
                     Mes = g.Key.Mes,
                     Cliente = g.Key.Cliente,
                     Cantidad = g.Count()
                 })
                 .GroupBy(x => new { x.Anno, x.Mes })
                 .Select(g => new
                 {
                     Anno = g.Key.Anno,
                     DatosMes = new
                     {
                         Mes = g.Key.Mes,
                         Clientes = g.OrderByDescending(x => x.Cantidad)
                                     .Take(10)
                                     .Select(x => new
                                     {
                                         Cliente = x.Cliente,
                                         Cantidad = x.Cantidad
                                     })
                                     .ToList(),
                         Total = g.Sum(x => x.Cantidad)
                     }
                 })
                 .OrderBy(x => x.Anno)
                 .ThenBy(x => x.DatosMes.Mes)
                 .ToList();

                return Json(new
                {
                    success = true,
                    ticketsPorAnnoGarantia = ticketsPorAnnoGarantia,
                    ticketsPorAnnoMesGarantia = ticketsPorAnnoMesGarantia
                }, JsonRequestBehavior.AllowGet);
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

        //********************** OTROS TICKETS *******************

        [HttpPost]
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsOtrosIndicadores(List<string> clientes, List<string> annos, List<string> meses, List<string> prioridades, List<string> categorias)
        {
            try
            {
                var fechaActual = DateTime.Now;
                var annoActual = fechaActual.Year;
                var mesActual = fechaActual.Month;

                // Manejo de años
                var annosInt = new List<int>();
                if (annos == null || !annos.Any())
                {
                    annosInt.Add(annoActual);
                }
                else
                {
                    annosInt = annos.Select(int.Parse).ToList();
                }

                // Manejo de meses
                var mesesInt = new List<int>();
                if (meses == null || !meses.Any())
                {
                    if (annosInt.Contains(annoActual))
                    {
                        mesesInt = Enumerable.Range(1, mesActual).ToList();
                    }
                    else
                    {
                        mesesInt = Enumerable.Range(1, 12).ToList();
                    }
                }
                else
                {
                    mesesInt = meses.Select(int.Parse).ToList();
                }

                // Consulta base con joins
                var ticketsQuery = from t in db.Ticket
                                   join tpc in db.Persona_Cliente on t.SecuencialPersona_Cliente equals tpc.SecuencialPersona
                                   join tc in db.Cliente on tpc.SecuencialCliente equals tc.Secuencial
                                   join tp in db.PrioridadTicket on t.SecuencialPrioridadTicket equals tp.Secuencial
                                   join ct in db.CategoriaTicket on t.SecuencialCategoriaTicket equals ct.Secuencial
                                   where t.FechaCreado != null
                                   select new
                                   {
                                       FechaIngreso = t.FechaCreado,
                                       Cliente = tc.Descripcion,
                                       ClienteSecuencial = tc.Secuencial,
                                       Prioridad = tp.Codigo,
                                       PrioridadSecuencial = tp.Secuencial,
                                       Categoria = ct.Codigo,
                                       CategoriaSecuencial = ct.Secuencial
                                   };

                // Aplicar filtros solo si tienen valores
                if (clientes != null && clientes.Any())
                    ticketsQuery = ticketsQuery.Where(t => clientes.Contains(t.ClienteSecuencial.ToString()));

                if (prioridades != null && prioridades.Any())
                    ticketsQuery = ticketsQuery.Where(t => prioridades.Contains(t.PrioridadSecuencial.ToString()));

                if (categorias != null && categorias.Any())
                    ticketsQuery = ticketsQuery.Where(t => categorias.Contains(t.CategoriaSecuencial.ToString()));

                // Filtrar por años y meses seleccionados
                ticketsQuery = ticketsQuery.Where(t =>
                    annosInt.Contains(t.FechaIngreso.Year) &&
                    mesesInt.Contains(t.FechaIngreso.Month)
                );

                // Seleccionar los campos necesarios
                var ticketsList = ticketsQuery.Select(t => new
                {
                    AnnoFechaIngreso = t.FechaIngreso.Year,
                    MesFechaIngreso = t.FechaIngreso.Month,
                    t.Cliente,
                    t.Prioridad,
                    t.Categoria
                }).ToList();

                // Agrupar por año, mes, cliente y prioridad
                var ticketsPorClientePrioridad = ticketsList
                    .GroupBy(t => new { t.AnnoFechaIngreso, t.MesFechaIngreso, t.Cliente, t.Prioridad })
                    .Select(g => new
                    {
                        Anno = g.Key.AnnoFechaIngreso,
                        Mes = g.Key.MesFechaIngreso,
                        Cliente = g.Key.Cliente,
                        Prioridad = g.Key.Prioridad,
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Prioridad)
                    .ThenBy(x => x.Anno)
                    .ThenBy(x => x.Mes)
                    .ThenBy(x => x.Cliente)
                    .ToList();

                // Agrupar por año, mes, cliente y categoría
                var ticketsPorClienteCategoria = ticketsList
                    .GroupBy(t => new { t.AnnoFechaIngreso, t.MesFechaIngreso, t.Cliente, t.Categoria })
                    .Select(g => new
                    {
                        Anno = g.Key.AnnoFechaIngreso,
                        Mes = g.Key.MesFechaIngreso,
                        Cliente = g.Key.Cliente,
                        Categoria = g.Key.Categoria,
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Anno)
                    .ThenBy(x => x.Mes)
                    .ThenBy(x => x.Cliente)
                    .ThenBy(x => x.Categoria)
                    .ToList();

                return Json(new
                {
                    success = true,
                    ticketsPorClientePrioridad = ticketsPorClientePrioridad,
                    ticketsPorClienteCategoria = ticketsPorClienteCategoria
                }, JsonRequestBehavior.AllowGet);
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsIntervaloGestores(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime fFin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso.HasValue &&
                                         DbFunctions.TruncateTime(ticket.FechaIngreso.Value) >= DbFunctions.TruncateTime(fInicio) &&
                                         DbFunctions.TruncateTime(ticket.FechaIngreso.Value) <= DbFunctions.TruncateTime(fFin)
                                   select ticket;

                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                var resumenTickets = ticketsList
                    .Where(ticket => ticket.Cliente != null && ticket.FechaIngreso.HasValue)
                    .Select(ticket => new
                    {
                        Cliente = ticket.Cliente,
                        FechaIngreso = ticket.FechaIngreso.Value,
                        GestorServicio = db.GestorServicios.FirstOrDefault(s => s.cliente != null && s.cliente.Descripcion == ticket.Cliente)
                    })
                    .GroupBy(ticket => new
                    {
                        Gestor = ticket.GestorServicio != null
                            ? ticket.GestorServicio.colaborador.persona.Nombre1 + " " + ticket.GestorServicio.colaborador.persona.Apellido1
                            : "Desconocido"
                    })
                    .Select(group => new
                    {
                        Gestor = group.Key.Gestor,
                        NumeroTickets = group.Count(),
                        TiempoMinutos = group.Count() * 5,
                        TiempoHoras = Math.Round((double)(group.Count() * 5) / 60, 2),
                        ClientesAtendidos = group.Select(ticket => ticket.Cliente).Distinct().Count(),
                        CarteraAsignada = db.GestorServicios.Count(s => (s.colaborador.persona.Nombre1 + " " + s.colaborador.persona.Apellido1) == group.Key.Gestor),
                    })
                    .ToList();

                var resp = new
                {
                    success = true,
                    ticketIntervaloGestores = resumenTickets
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsTiempoGestores(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime fFin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var tiempoQuery = from tiempo in db.InfoTickets
                                  where tiempo.FechaIngreso.HasValue &&
                                        DbFunctions.TruncateTime(tiempo.FechaIngreso.Value) >= DbFunctions.TruncateTime(fInicio) &&
                                        DbFunctions.TruncateTime(tiempo.FechaIngreso.Value) <= DbFunctions.TruncateTime(fFin)
                                  select tiempo;

                List<InfoTickets> tiempoList = tiempoQuery.ToList();

                var resumenTiempo = tiempoList
                    .Where(tiempo => tiempo.Cliente != null && tiempo.FechaIngreso.HasValue)
                    .Select(tiempo => new
                    {
                        Cliente = tiempo.Cliente,
                        FechaIngreso = tiempo.FechaIngreso.Value,
                        GestorServicio = db.GestorServicios.FirstOrDefault(s => s.cliente != null && s.cliente.Descripcion == tiempo.Cliente)
                    })
                    .GroupBy(tiempo => new
                    {
                        Gestor = tiempo.GestorServicio != null
                            ? tiempo.GestorServicio.colaborador.persona.Nombre1 + " " + tiempo.GestorServicio.colaborador.persona.Apellido1
                            : "Desconocido",
                        Año = tiempo.FechaIngreso.Year,
                        Mes = tiempo.FechaIngreso.Month
                    })
                    .Select(group => new
                    {
                        Gestor = group.Key.Gestor,
                        Mes = group.Key.Mes,
                        Anio = group.Key.Año,
                        TiempoTotal = Math.Round((double)(group.Count() * 5) / 60, 2)
                    })
                    .ToList();

                var resp = new
                {
                    success = true,
                    tiempoIntervaloGestores = resumenTiempo
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public async Task<ActionResult> DarTicketsAnuladosRechazados(string fechaInicio, string fechaFin)
        {
            try
            {
                DateTime fInicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime fFin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var ticketsPorMes = from ticket in db.InfoTickets
                                    where ticket.FechaIngreso.HasValue &&
                                          ticket.FechaIngreso.Value.Year >= fInicio.Year &&
                                          ticket.FechaIngreso.Value.Month >= fInicio.Month &&
                                          ticket.FechaIngreso.Value.Day >= fInicio.Day &&
                                          ticket.FechaIngreso.Value.Year <= fFin.Year &&
                                          ticket.FechaIngreso.Value.Month <= fFin.Month &&
                                          ticket.FechaIngreso.Value.Day <= fFin.Day
                                    let gestorServicio = db.GestorServicios.FirstOrDefault(s => s.cliente != null && s.cliente.Descripcion == ticket.Cliente)
                                    let nombreGestor = gestorServicio != null && gestorServicio.colaborador != null && gestorServicio.colaborador.persona != null ? gestorServicio.colaborador.persona.Nombre1 + " " + gestorServicio.colaborador.persona.Apellido1 : "Desconocido"
                                    group ticket by new { Gestor = nombreGestor, Anio = ticket.FechaIngreso.Value.Year, Mes = ticket.FechaIngreso.Value.Month } into g
                                    select new
                                    {
                                        g.Key.Gestor,
                                        g.Key.Anio,
                                        g.Key.Mes,
                                        Ingresado = g.Count(),
                                        AnuladoRechazado = g.Count(t => t.Estado == "Anulado" || t.Estado == "Rechazado")
                                    };

                var listaTicketsPorMes = ticketsPorMes.ToList();
                var resp = new
                {
                    success = true,
                    anuladosRechazados = listaTicketsPorMes
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public async Task<ActionResult> DarTicketsAnuladosRechazadosIndicadores(string fechaInicio, string fechaFin, int secuencialGestor)
        {
            try
            {
                DateTime fInicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime fFin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var ticketsPorMes = from ticket in db.InfoTickets
                                    where ticket.FechaIngreso.HasValue &&
                                          ticket.FechaIngreso.Value >= fInicio &&
                                          ticket.FechaIngreso.Value <= fFin
                                    let gestorServicio = db.GestorServicios.FirstOrDefault(s => s.cliente != null && s.cliente.Descripcion == ticket.Cliente)
                                    let nombreGestor = gestorServicio != null && gestorServicio.colaborador != null && gestorServicio.colaborador.persona != null ? gestorServicio.colaborador.persona.Nombre1 + " " + gestorServicio.colaborador.persona.Apellido1 : "Desconocido"
                                    where secuencialGestor == 0 || (gestorServicio != null && gestorServicio.Secuencial == secuencialGestor)
                                    group ticket by new { Gestor = nombreGestor, Anio = ticket.FechaIngreso.Value.Year, Mes = ticket.FechaIngreso.Value.Month } into g
                                    select new
                                    {
                                        g.Key.Gestor,
                                        g.Key.Anio,
                                        g.Key.Mes,
                                        Ingresado = g.Count(),
                                        AnuladoRechazado = g.Count(t => t.Estado == "Anulado" || t.Estado == "Rechazado"),
                                        PorcentajeAnuladoRechazado = Math.Round((double)g.Count(t => t.Estado == "Anulado" || t.Estado == "Rechazado") / g.Count() * 100)
                                    };

                var listaTicketsPorMes = ticketsPorMes.ToList();
                var resp = new
                {
                    success = true,
                    anuladosRechazados = listaTicketsPorMes
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsAnalisadosGestores(string fechaInicio, string fechaFin)
        {
            try
            {
                if (string.IsNullOrEmpty(fechaInicio) || string.IsNullOrEmpty(fechaFin))
                {
                    return Json(new { success = false, msg = "Las fechas no pueden ser nulas" });
                }

                DateTime fInicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime fFin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso.HasValue &&
                                         ticket.FechaIngreso.Value >= fInicio &&
                                         ticket.FechaIngreso.Value <= fFin
                                   select ticket;

                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                var resumenTickets = ticketsList
                    .Where(ticket => ticket.Cliente != null && ticket.FechaIngreso.HasValue)
                    .GroupBy(ticket => new { ticket.Cliente, Año = ticket.FechaIngreso.Value.Year, Mes = ticket.FechaIngreso.Value.Month })
                    .Select(group =>
                    {
                        var gestorServicio = db.GestorServicios.FirstOrDefault(s => s.cliente != null && s.cliente.Descripcion == group.Key.Cliente);
                        var nombreGestor = gestorServicio != null && gestorServicio.colaborador != null && gestorServicio.colaborador.persona != null ? gestorServicio.colaborador.persona.Nombre1 + " " + gestorServicio.colaborador.persona.Apellido1 : "Desconocido";
                        return new
                        {
                            Gestor = nombreGestor,
                            Anio = group.Key.Año,
                            Mes = group.Key.Mes,
                            NumeroTickets = group.Count()
                        };
                    })
                    .ToList();

                var resumenFinal = resumenTickets
                    .GroupBy(resumen => new { resumen.Gestor, resumen.Anio, resumen.Mes })
                    .Select(group => new
                    {
                        Gestor = group.Key.Gestor,
                        Anio = group.Key.Anio,
                        Mes = group.Key.Mes,
                        NumeroTickets = group.Sum(resumen => resumen.NumeroTickets)
                    })
                    .ToList();

                var resp = new
                {
                    success = true,
                    ticketsAnalizados = resumenFinal
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
        [Authorize(Roles = "ADMIN, INDICADORES")]
        public ActionResult DarTicketsPorAnioGestor(int anio, string gestor)
        {
            try
            {
                if (anio <= 0)
                    anio = DateTime.Now.Year; // Usar el año actual si no se proporciona uno válido

                if (string.IsNullOrEmpty(gestor))
                    throw new ArgumentException("El gestor no puede estar vacío.");

                var gestorUpperCase = gestor.ToUpper();
                var clientes = (from c in db.Cliente
                                join g in db.GestorServicios on c.Secuencial equals g.SecuencialCliente
                                where gestorUpperCase == "TODOS" || g.colaborador.persona.Nombre1.ToUpper() + " " + g.colaborador.persona.Apellido1.ToUpper() == gestorUpperCase
                                select c.Descripcion.ToUpper()).ToList();

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso.HasValue &&
                                         ticket.FechaIngreso.Value.Year == anio &&
                                         clientes.Contains(ticket.Cliente.ToUpper())
                                   select ticket;

                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                var groupedTickets = ticketsList
                    .GroupBy(ticket => new
                    {
                        Cliente = ticket.Cliente.ToUpper(),
                        Mes = ticket.FechaIngreso.Value.Month
                    })
                    .Select(group => new
                    {
                        Cliente = group.Key.Cliente,
                        Mes = group.Key.Mes,
                        CantidadTickets = group.Count()
                    })
                    .ToList();

                var resumenFinal = groupedTickets
                    .GroupBy(t => t.Cliente)
                    .Select(g => new
                    {
                        Cliente = g.Key,
                        TicketsPorMes = g.OrderBy(r => r.Mes).Select(r => r.CantidadTickets).ToList(),
                        Total = g.Sum(r => r.CantidadTickets)
                    })
                    .OrderByDescending(r => r.Total)
                    .ToList();

                var totalTickets = resumenFinal.Sum(r => r.Total);

                // Calcular los totales por mes
                var totalesPorMes = new int[12];
                foreach (var cliente in resumenFinal)
                {
                    for (int i = 0; i < cliente.TicketsPorMes.Count; i++)
                    {
                        totalesPorMes[i] += cliente.TicketsPorMes[i];
                    }
                }

                var resp = new
                {
                    success = true,
                    resumenTickets = resumenFinal,
                    totalTickets = totalTickets,
                    totalesPorMes = totalesPorMes
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


        private static double CalcularPorcentage(double cantidad, double total)
        {
            double porcentaje = cantidad != 0 ? (cantidad / total) * 100 : 0;
            return Math.Round(porcentaje);
        }
    }
    public class ClienteResumen
    {
        public string Cliente { get; set; }
        public Dictionary<int, int> TicketsPorMes { get; set; }
        public int Total { get; set; }
    }
}