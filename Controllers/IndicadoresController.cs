using SifizPlanning.Models;
using SifizPlanning.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
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
                } else
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

                var ticketsQueryN = db.InfoTickets.AsNoTracking()
                                             .Where(it => it.FechaIngreso.Value >= fInicio && it.FechaIngreso.Value <= fFin)
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

                var ticketsQueryC = from ticket in db.InfoTickets
                                    where ticket.FechaIngreso.Value >= fInicio
                                       && ticket.FechaIngreso.Value <= fFin
                                       && ticket.Estado == "CERRADO"
                                       && ticket.FechaCierre != null
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

        //[HttpPost]
        //[Authorize(Roles = "ADMIN, INDICADORES")]
        //public ActionResult DarTicketsEnGestion(string fechaInicio, string fechaFin)
        //{
        //    try
        //    {
        //        // Crear objetos DateTime para almacenar las fechas
        //        DateTime fInicioTEG = new DateTime();
        //        DateTime fFinTEG = new DateTime();

        //        // Verificar si fechaInicio es nula o vacía
        //        if (!string.IsNullOrEmpty(fechaInicio))
        //        {
        //            string[] fechas = fechaInicio.Split(new Char[] { '/' });
        //            int dia = Int32.Parse(fechas[0]);
        //            int mes = Int32.Parse(fechas[1]);
        //            int anno = Int32.Parse(fechas[2]);
        //            fInicioTEG = new DateTime(anno, mes, dia);
        //        }
        //        else
        //        {
        //            // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
        //            fInicioTEG = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //        }

        //        // Verificar si fechaFin es nula o vacía
        //        if (!string.IsNullOrEmpty(fechaFin))
        //        {
        //            string[] fechasFin = fechaFin.Split(new Char[] { '/' });
        //            int dia = Int32.Parse(fechasFin[0]);
        //            int mes = Int32.Parse(fechasFin[1]);
        //            int anno = Int32.Parse(fechasFin[2]);
        //            fFinTEG = new DateTime(anno, mes, dia);
        //        }
        //        else
        //        {
        //            // Si fechaFin es nula o vacía, tomar el día actual
        //            fFinTEG = DateTime.Now;
        //        }

        //        var ticketsQueryTEG = (from ticket in db.InfoTickets
        //                              where ticket.FechaIngreso != null
        //                                 && ticket.FechaIngreso.Value >= fInicioTEG
        //                                 && ticket.FechaIngreso.Value <= fFinTEG
        //                                 && ticket.Estado != "CERRADO"
        //                                 && ticket.Estado != "ANULADO"
        //                              select ticket).ToList();

        //        // Convertir la consulta a una lista para trabajar con ella en memoria
        //        List<InfoTickets> ticketsListTEG = ticketsQueryTEG.ToList();

        //        var groupedTicketsTEG = ticketsListTEG
        //            .GroupBy(ticket => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
        //                ticket.FechaIngreso.Value,
        //                CalendarWeekRule.FirstDay,
        //                DayOfWeek.Monday))
        //            .Select(g => new
        //            {
        //                Semana = g.Key,
        //                Cantidad = g.Count(),
        //                Descripcion = "AL " + g.Max(t => t.FechaIngreso.Value).ToString("dd/MM/yyyy")
        //            })
        //            .OrderBy(x => x.Semana)
        //            .ToList();

        //        var totalCantidades = groupedTicketsTEG.Sum(ticket => ticket.Cantidad);

        //        var resp = new
        //        {
        //            success = true,
        //            infoTickets = groupedTicketsTEG,
        //            totalCantidades = totalCantidades
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

        //[HttpPost]
        //[Authorize(Roles = "ADMIN, INDICADORES")]
        //public ActionResult DarTicketsPorCategorias(string fechaInicio, string fechaFin)
        //{
        //    try
        //    {
        //        // Crear objetos DateTime para almacenar las fechas
        //        DateTime fInicio = new DateTime();
        //        DateTime fFin = new DateTime();

        //        // Verificar si fechaInicio es nula o vacía
        //        if (!string.IsNullOrEmpty(fechaInicio))
        //        {
        //            string[] fechas = fechaInicio.Split(new Char[] { '/' });
        //            int dia = Int32.Parse(fechas[0]);
        //            int mes = Int32.Parse(fechas[1]);
        //            int anno = Int32.Parse(fechas[2]);
        //            fInicio = new DateTime(anno, mes, dia);
        //        }
        //        else
        //        {
        //            // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
        //            fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //        }

        //        // Verificar si fechaFin es nula o vacía
        //        if (!string.IsNullOrEmpty(fechaFin))
        //        {
        //            string[] fechasFin = fechaFin.Split(new Char[] { '/' });
        //            int dia = Int32.Parse(fechasFin[0]);
        //            int mes = Int32.Parse(fechasFin[1]);
        //            int anno = Int32.Parse(fechasFin[2]);
        //            fFin = new DateTime(anno, mes, dia);
        //        }
        //        else
        //        {
        //            // Si fechaFin es nula o vacía, tomar el día actual
        //            fFin = DateTime.Now;
        //        }

        //        var ticketsQueryPC = (from ticket in db.InfoTickets
        //                           where ticket.FechaIngreso != null
        //                              && ticket.FechaIngreso.Value >= fInicio
        //                              && ticket.FechaIngreso.Value <= fFin
        //                           select ticket).ToList();

        //        // Convertir la consulta a una lista para trabajar con ella en memoria
        //        List<InfoTickets> ticketsListPC = ticketsQueryPC.ToList();

        //        // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
        //        // Agrupar los tickets por semana
        //        var groupedTickets = ticketsListPC
        //           .GroupBy(t => t.Tipo) // Agrupa los tickets por Tipo
        //           .Select(g => new
        //           {
        //               Tipo = g.Key, // Obtiene el Tipo como clave del grupo
        //               Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
        //           })
        //           .OrderBy(x => x.Tipo) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
        //           .ToList();

        //        var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

        //        var categoriasPorcentajes = groupedTickets.Select(g => new
        //        {
        //            Categoria = g.Tipo,
        //            Cantidad = g.Cantidad,
        //            Porcentaje = CalcularPorcentage(g.Cantidad, totalCantidades)
        //        }).ToList();

        //        var resp = new
        //        {
        //            success = true,
        //            infoTickets = categoriasPorcentajes,
        //            totalCantidades = totalCantidades
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

        //[HttpPost]
        //[Authorize(Roles = "ADMIN, INDICADORES")]
        //public ActionResult DarTicketsPorEstados(string fechaInicio, string fechaFin)
        //{
        //    try
        //    {
        //        // Crear objetos DateTime para almacenar las fechas
        //        DateTime fInicio = new DateTime();
        //        DateTime fFin = new DateTime();

        //        // Verificar si fechaInicio es nula o vacía
        //        if (!string.IsNullOrEmpty(fechaInicio))
        //        {
        //            string[] fechas = fechaInicio.Split(new Char[] { '/' });
        //            int dia = Int32.Parse(fechas[0]);
        //            int mes = Int32.Parse(fechas[1]);
        //            int anno = Int32.Parse(fechas[2]);
        //            fInicio = new DateTime(anno, mes, dia);
        //        }
        //        else
        //        {
        //            // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
        //            fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //        }

        //        // Verificar si fechaFin es nula o vacía
        //        if (!string.IsNullOrEmpty(fechaFin))
        //        {
        //            string[] fechasFin = fechaFin.Split(new Char[] { '/' });
        //            int dia = Int32.Parse(fechasFin[0]);
        //            int mes = Int32.Parse(fechasFin[1]);
        //            int anno = Int32.Parse(fechasFin[2]);
        //            fFin = new DateTime(anno, mes, dia);
        //        }
        //        else
        //        {
        //            // Si fechaFin es nula o vacía, tomar el día actual
        //            fFin = DateTime.Now;
        //        }

        //        var ticketsQueryPE = (from ticket in db.InfoTickets
        //                           where ticket.FechaIngreso != null
        //                              && ticket.FechaIngreso.Value >= fInicio
        //                              && ticket.FechaIngreso.Value <= fFin
        //                           where ticket.Estado != "CERRADO"
        //                           select ticket).ToList();

        //        // Convertir la consulta a una lista para trabajar con ella en memoria
        //        List<InfoTickets> ticketsListPE = ticketsQueryPE.ToList();

        //        // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
        //        // Agrupar los tickets por semana
        //        var groupedTickets = ticketsListPE
        //           .GroupBy(t => t.Estado) // Agrupa los tickets por Tipo
        //           .Select(g => new
        //           {
        //               Estado = g.Key, // Obtiene el Tipo como clave del grupo
        //               Cantidad = g.Count() // Cuenta la cantidad de tickets en cada grupo
        //           })
        //           .OrderBy(x => x.Estado) // Ordena por Tipo (opcional, dependiendo de tus necesidades)
        //           .ToList();

        //        var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

        //        var estadosPorcentajes = groupedTickets.Select(g => new
        //        {
        //            Estado = g.Estado,
        //            Cantidad = g.Cantidad,
        //            Porcentaje = CalcularPorcentage(g.Cantidad, totalCantidades)
        //        }).ToList();

        //        var resp = new
        //        {
        //            success = true,
        //            infoTickets = estadosPorcentajes,
        //            totalCantidades = totalCantidades
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
                    .GroupBy(ticket => new { Anio = ticket.FechaIngreso.Value.Year, Mes = ticket.FechaIngreso.Value.Month, ticket.Cliente  })
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

        //[HttpPost]
        //[Authorize(Roles = "ADMIN, INDICADORES")]
        //public ActionResult DarTicketsPorClientesPorEstado(string fechaInicio, string fechaFin)
        //{
        //    try
        //    {
        //        // Crear objetos DateTime para almacenar las fechas
        //        DateTime fInicio = new DateTime();
        //        DateTime fFin = new DateTime();

        //        // Verificar si fechaInicio es nula o vacía
        //        if (!string.IsNullOrEmpty(fechaInicio))
        //        {
        //            string[] fechas = fechaInicio.Split(new Char[] { '/' });
        //            int dia = Int32.Parse(fechas[0]);
        //            int mes = Int32.Parse(fechas[1]);
        //            int anno = Int32.Parse(fechas[2]);
        //            fInicio = new DateTime(anno, mes, dia);
        //        }
        //        else
        //        {
        //            // Si fechaInicio es nula o vacía, tomar el primer día del mes en curso
        //            fInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //        }

        //        // Verificar si fechaFin es nula o vacía
        //        if (!string.IsNullOrEmpty(fechaFin))
        //        {
        //            string[] fechasFin = fechaFin.Split(new Char[] { '/' });
        //            int dia = Int32.Parse(fechasFin[0]);
        //            int mes = Int32.Parse(fechasFin[1]);
        //            int anno = Int32.Parse(fechasFin[2]);
        //            fFin = new DateTime(anno, mes, dia);
        //        }
        //        else
        //        {
        //            // Si fechaFin es nula o vacía, tomar el día actual
        //            fFin = DateTime.Now;
        //        }

        //        var ticketsQueryPCE = (from ticket in db.InfoTickets
        //                           where ticket.FechaIngreso != null
        //                              && ticket.FechaIngreso.Value >= fInicio
        //                              && ticket.FechaIngreso.Value <= fFin
        //                           select ticket).ToList();

        //        // Convertir la consulta a una lista para trabajar con ella en memoria
        //        List<InfoTickets> ticketsListPCE = ticketsQueryPCE.ToList();

        //        // Paso 2: Aplicar el cálculo de la semana y otros procesamientos en memoria
        //        // Agrupar los tickets por semana
        //        var groupedTickets = ticketsListPCE
        //            .GroupBy(ticket => new { ticket.Cliente, ticket.Estado })
        //            .Select(group => new
        //            {
        //                Cliente = group.Key.Cliente,
        //                Estado = group.Key.Estado,
        //                Cantidad = group.Count()
        //            }).Distinct()
        //            .ToList();

        //        var totalCantidades = groupedTickets.Sum(ticket => ticket.Cantidad);

        //        var resp = new
        //        {
        //            success = true,
        //            infoTickets = groupedTickets,
        //            totalCantidades = totalCantidades
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
        //*******************************************************************************TICKETS AL DIA********************************************************************************

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

                var ticketsQueryTEGAD = (from ticket in db.InfoTickets
                                       where ticket.FechaIngreso != null
                                          && ticket.FechaIngreso.Value <= fActual
                                          && ticket.Estado != "CERRADO"
                                          && ticket.Estado != "ANULADO"
                                          && (ticket.Cliente == nombreCliente || nombreCliente == "")
                                       select ticket).ToList();

                List<InfoTickets> ticketsListTEGAD = ticketsQueryTEGAD.ToList();

                var groupedTicketsTEGAD = ticketsListTEGAD
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

                var ticketsQueryPCAD = (from ticket in db.InfoTickets
                                    where ticket.FechaIngreso != null
                                       && ticket.FechaIngreso.Value <= fActual
                                        && (ticket.Cliente == nombreCliente || nombreCliente == "")
                                        select ticket).ToList();


                List<InfoTickets> ticketsListPCAD = ticketsQueryPCAD.ToList();

                var groupedTicketsPCAD = ticketsListPCAD
                   .GroupBy(t => t.Tipo)
                   .Select(g => new
                   {
                       Tipo = g.Key,
                       Cantidad = g.Count() 
                   })
                   .OrderBy(x => x.Tipo) 
                   .ToList();

                var totalCantidades = groupedTicketsPCAD.Sum(ticket => ticket.Cantidad);

                var categoriasPorcentajes = groupedTicketsPCAD.Select(g => new
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

                var ticketsQueryPEAD = (from ticket in db.InfoTickets
                                    where ticket.FechaIngreso != null
                                       && ticket.FechaIngreso.Value <= fActual
                                        && ticket.Estado != "CERRADO"
                                        && (ticket.Cliente == nombreCliente || nombreCliente == "")
                                        select ticket).ToList();


                List<InfoTickets> ticketsListPEAD = ticketsQueryPEAD.ToList();

                var groupedTicketsPEAD = ticketsListPEAD
                   .GroupBy(t => t.Estado) 
                   .Select(g => new
                   {
                       Estado = g.Key, 
                       Cantidad = g.Count() 
                   })
                   .OrderBy(x => x.Estado) 
                   .ToList();

                var totalCantidades = groupedTicketsPEAD.Sum(ticket => ticket.Cantidad);

                var estadosPorcentajes = groupedTicketsPEAD.Select(g => new
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

                var ticketsQueryPCEAD = (from ticket in db.InfoTickets
                                    where ticket.FechaIngreso != null
                                       && ticket.FechaIngreso.Value <= fActual
                                       && (ticket.Cliente == nombreCliente || nombreCliente == "")
                                         select ticket).ToList();

                List<InfoTickets> ticketsListPCEAD = ticketsQueryPCEAD.ToList();

                var groupedTicketsPCEAD = ticketsListPCEAD
                    .GroupBy(ticket => new { ticket.Cliente, ticket.Estado })
                    .Select(group => new
                    {
                        Cliente = group.Key.Cliente,
                        Estado = group.Key.Estado,
                        Cantidad = group.Count()
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
        public ActionResult DarTicketsPendientesAlDia(int cliente)
        {
            try
            {
                DateTime fActual = DateTime.Today;
                String nombreCliente = "";

                if (cliente != 0)
                {
                    nombreCliente = (from cli in db.Cliente where cli.Secuencial == cliente select cli.Descripcion).FirstOrDefault();
                }

                var ticketsQueryTPAD = (from ticket in db.InfoTickets
                                         where ticket.FechaIngreso != null
                                            && ticket.FechaIngreso.Value <= fActual
                                            && ticket.Estado == "PENDIENTE"
                                            && (ticket.Cliente == nombreCliente || nombreCliente == "")
                                         select ticket).ToList();

                List<InfoTickets> ticketsListTPAD = ticketsQueryTPAD.ToList();

                var groupedTicketsTPAD = ticketsListTPAD
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

                var totalCantidadesTPAD = groupedTicketsTPAD.Sum(ticket => ticket.Cantidad);

                var resp = new
                {
                    success = true,
                    infoTickets = groupedTicketsTPAD,
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
        public ActionResult DarTicketsIntervaloGestores(string fechaInicio, string fechaFin)
        {
            try
            {
                // Crear objetos DateTime para almacenar las fechas
                DateTime fInicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime fFin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso.HasValue &&
                                         ticket.FechaIngreso.Value.Year >= fInicio.Year &&
                                         ticket.FechaIngreso.Value.Month >= fInicio.Month &&
                                         ticket.FechaIngreso.Value.Day >= fInicio.Day &&
                                         ticket.FechaIngreso.Value.Year <= fFin.Year &&
                                         ticket.FechaIngreso.Value.Month <= fFin.Month &&
                                         ticket.FechaIngreso.Value.Day <= fFin.Day
                                   select ticket;

                List<InfoTickets> ticketsList = ticketsQuery.ToList();

                var resumenTickets = ticketsList
                    .GroupBy(ticket => ticket.AsignadoA)
                    .Select(group => new
                    {
                        Gestor = group.Key,
                        NumeroTickets = group.Count(),
                        TiempoMinutos = Math.Round(group.Sum(ticket => ((ticket.HorasEmpleadas ?? DateTime.MinValue) - (ticket.HorasEmpleadas ?? DateTime.MinValue).Date).TotalMinutes), 2),
                        TiempoHoras = Math.Round(group.Sum(ticket => ((ticket.HorasEmpleadas ?? DateTime.MinValue) - (ticket.HorasEmpleadas ?? DateTime.MinValue).Date).TotalHours), 2),
                        ClientesAtendidos = group.Select(ticket => ticket.Cliente).Distinct().Count(),
                        CarteraAsignada = group.Select(ticket => ticket.Cliente).Distinct().Count() // Ajusta según tus necesidades
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
                    .Select(group => {
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

                var ticketsQuery = from ticket in db.InfoTickets
                                   where ticket.FechaIngreso.HasValue &&
                                         ticket.FechaIngreso.Value.Year == anio &&
                                         ticket.AsignadoA.ToUpper() == gestorUpperCase
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

                var resp = new
                {
                    success = true,
                    resumenTickets = resumenFinal,
                    totalTickets = totalTickets
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