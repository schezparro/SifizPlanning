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
        public ActionResult DarInfotickets()
        {
            try
            {
                var infoTickets = db.InfoTickets.AsNoTracking().ToList();

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