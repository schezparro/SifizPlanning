using SifizPlanning.Models;
using SifizPlanning.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
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
        public ActionResult DarInfotickets(string fechaInicio, string fechaFin)
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

                var infoTickets = db.InfoTickets.AsNoTracking()
                                             .Where(it => it.FechaIngreso >= fInicio && it.FechaIngreso <= fFin)
                                             .ToList();

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

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso != null && ticket.FechaIngreso.Value >= fInicio && ticket.FechaIngreso.Value <= fFin
                                   select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsList
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

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaCierre != null
                                      && ticket.FechaCierre.Value >= fInicio
                                      && ticket.FechaCierre.Value <= fFin
                                      && ticket.Estado == "CERRADO"
                                   select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsList
                    .GroupBy(ticket => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        ticket.FechaCierre.Value,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday))
                    .Select(g => new
                    {
                        Semana = g.Key,
                        Cantidad = g.Count(),
                        Descripcion = "AL " + g.Max(t => t.FechaCierre.Value).ToString("dd/MM/yyyy")
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
        public ActionResult DarTicketsEnGestion(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicioTEG = new DateTime();
                DateTime fFinTEG = new DateTime();

                // Verificar si fechaInicio es nula o vacía
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    string[] fechas = fechaInicio.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechas[0]);
                    int mes = Int32.Parse(fechas[1]);
                    int anno = Int32.Parse(fechas[2]);
                    fInicioTEG = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
                    fInicioTEG = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                // Verificar si fechaFin es nula o vacía
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    string[] fechasFin = fechaFin.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechasFin[0]);
                    int mes = Int32.Parse(fechasFin[1]);
                    int anno = Int32.Parse(fechasFin[2]);
                    fFinTEG = new DateTime(anno, mes, dia);
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFinTEG = DateTime.Now;
                }

                var ticketsQueryTEG = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso != null
                                      && ticket.FechaIngreso.Value >= fInicioTEG
                                      && ticket.FechaIngreso.Value <= fFinTEG
                                      && ticket.Estado == "EN DESARROLLO"
                                   select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsListTEG = ticketsQueryTEG.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTicketsTEG = ticketsListTEG
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

                var totalCantidades = groupedTicketsTEG.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTicketsTEG,
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
        public ActionResult DarTicketsPorCategorias(string fechaInicio, string fechaFin)
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

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso != null
                                      && ticket.FechaIngreso.Value >= fInicio
                                      && ticket.FechaIngreso.Value <= fFin
                                   select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsList
                   .GroupBy(t => t.Tipo) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Tipo = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
                   })
                   .OrderBy(x => x.Tipo) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
                   .ToList();

                var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

                var categoriasPorcentajes = groupedTickets.Select(g => new
                {
                    Categoria = g.Tipo,
                    Cantidad = g.Cantidad,
                    Porcentaje = CalcularPorcentage(g.Cantidad, totalCantidades)
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
        public ActionResult DarTicketsPorEstados(string fechaInicio, string fechaFin)
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

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso != null
                                      && ticket.FechaIngreso.Value >= fInicio
                                      && ticket.FechaIngreso.Value <= fFin
                                   select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsList
                   .GroupBy(t => t.Estado) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Estado = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
                   })
                   .OrderBy(x => x.Estado) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
                   .ToList();

                var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

                var estadosPorcentajes = groupedTickets.Select(g => new
                {
                    Estado = g.Estado,
                    Cantidad = g.Cantidad,
                    Porcentaje = CalcularPorcentage(g.Cantidad, totalCantidades)
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
                }
                else
                {
                    // Si fechaFin es nula o vacía, tomar el día actual
                    fFin = DateTime.Now;
                }

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso != null
                                      && ticket.FechaIngreso.Value >= fInicio
                                      && ticket.FechaIngreso.Value <= fFin
                                   select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsList
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

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso != null
                                      && ticket.FechaIngreso.Value >= fInicio
                                      && ticket.FechaIngreso.Value <= fFin 
                                      && ticket.Tipo == "MANTENIMIENTO"
                                   select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsList
                   .GroupBy(t => t.Cliente) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Cliente = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
                   })
                   .OrderBy(x => x.Cliente) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
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

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso != null
                                      && ticket.FechaIngreso.Value >= fInicio
                                      && ticket.FechaIngreso.Value <= fFin
                                      && ticket.Tipo == "GARANTÍA TÉCNICA"
                                   select ticket;

                // Convertir la consulta a una lista para trabajar con ella en memoria
                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
                // Agrupar los tickets por semana
                var groupedTickets = ticketsList
                   .GroupBy(t => t.Cliente) // Agrupa los tickets por Tipo
                   .Select(g => new
                   {
                       Cliente = g.Key, // Obtiene el Tipo como clave del grupo
                       Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
                   })
                   .OrderBy(x => x.Cliente) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
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

        private static double CalcularPorcentage(double cantidad, double total)
        {
            double porcentaje = cantidad != 0 ? (cantidad / total) * 100 : 0;
            return Math.Round(porcentaje);
        }

    }
}