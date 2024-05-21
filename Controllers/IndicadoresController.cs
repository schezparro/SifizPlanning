using SifizPlanning.Models;
using SifizPlanning.Security;
using System;
using System.Collections.Generic;
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
    }
}