using SifizPlanning.Models;
using SifizPlanning.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using SifizPlanning.Util;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.Caching;
using System.Net;

namespace SifizPlanning.Controllers
{
    public class ReportController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();

        private const string ReportAccessKey = "_ReportAccess_Secret_Key";

        // GET: Report
        //PANTALLA INICIAL
        [Authorize(Roles = "COORDINADOR, ADMIN")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult GetReportAccess(int cliente)
        {
            string accessCode = Guid.NewGuid().ToString("N");

            Session[ReportAccessKey + accessCode] = cliente;

            HttpContext.Cache.Insert(
                ReportAccessKey + accessCode,
                cliente,
                null,
                DateTime.Now.AddMinutes(5),
                Cache.NoSlidingExpiration
            );

            return Json(new { code = accessCode }, JsonRequestBehavior.AllowGet);
        }



        //MUESTRA EL REPORTE SELECCIONADO
        [Authorize(Roles = "COORDINADOR, ADMIN")]
        public ActionResult VerReporte(string modulo, string reporte)
        {
            var url = (from r in db.Reporte
                       where r.Modulo.Equals(modulo) && r.Nombre.Equals(reporte)
                       select r.Url).ToList()[0];

            string reportServerUser = ConfigurationManager.AppSettings.Get("ReportServerUser");
            string reportServerPass = ConfigurationManager.AppSettings.Get("ReportServerPass");
            string reportServerUrl = ConfigurationManager.AppSettings.Get("ReportServerUrl");

            ReportViewer reportViewer = new ReportViewer();
            reportViewer.ProcessingMode = ProcessingMode.Remote;
            reportViewer.ServerReport.ReportServerCredentials = new ReportServerCredentials(reportServerUser, reportServerPass, "");

            reportViewer.SizeToReportContent = true;
            reportViewer.Width = Unit.Percentage(100);
            reportViewer.Height = Unit.Percentage(100);

            reportViewer.ServerReport.ReportServerUrl = new Uri(reportServerUrl);
            //reportViewer.ServerReport.ReportPath = "/Comercializacion/ClientePorModulo";
            reportViewer.ServerReport.ReportPath = url;

            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        //MUESTRA EL REPORTE SELECCIONADO
        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult VerReporteCliente(int cliente)
        {
            string modulo = "Clientes";
            string reporte = "Detalle Ticket Cliente";

            var url = (from r in db.Reporte
                       where r.Modulo.Equals(modulo) && r.Nombre.Equals(reporte)
                       select r.Url).ToList()[0];

            string reportServerUser = ConfigurationManager.AppSettings.Get("ReportServerUser");
            string reportServerPass = ConfigurationManager.AppSettings.Get("ReportServerPass");
            string reportServerUrl = ConfigurationManager.AppSettings.Get("ReportServerUrl");

            ReportViewer reportViewer = new ReportViewer();
            reportViewer.ProcessingMode = ProcessingMode.Remote;
            reportViewer.ServerReport.ReportServerCredentials = new ReportServerCredentials(reportServerUser, reportServerPass, "");

            reportViewer.SizeToReportContent = true;
            reportViewer.Width = Unit.Percentage(100);
            reportViewer.Height = Unit.Percentage(100);

            reportViewer.ServerReport.ReportServerUrl = new Uri(reportServerUrl);
            reportViewer.ServerReport.ReportPath = url;

            ReportParameter secuencialClienteParam = new ReportParameter("secuencialCliente", cliente.ToString());
            reportViewer.ServerReport.SetParameters(new ReportParameter[] { secuencialClienteParam });

            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        //MUESTRA EL REPORTE SELECCIONADO
        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult VerReporteMantenimientoCliente(string code)
        {
            try
            {
                var clienteObj = HttpContext.Cache[ReportAccessKey + code];
                if (clienteObj == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                int cliente = (int)clienteObj;

                string modulo = "Clientes";
                string reporte = "Horas Mantenimiento Cliente Web";
                var url = (from r in db.Reporte
                           where r.Modulo.Equals(modulo) && r.Nombre.Equals(reporte)
                           select r.Url).FirstOrDefault();

                if (string.IsNullOrEmpty(url))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Reporte no encontrado.");
                }

                string reportServerUrl = ConfigurationManager.AppSettings.Get("ReportServerUrl");

                // Calcular las fechas de inicio y fin del mes anterior
                DateTime currentDate = DateTime.Now;
                DateTime firstDayOfCurrentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                DateTime lastDayOfPreviousMonth = firstDayOfCurrentMonth.AddDays(-1);
                DateTime firstDayOfPreviousMonth = new DateTime(lastDayOfPreviousMonth.Year, lastDayOfPreviousMonth.Month, 1);

                ReportViewer reportViewer = new ReportViewer();
                reportViewer.ProcessingMode = ProcessingMode.Remote;
                reportViewer.SizeToReportContent = true;
                reportViewer.Width = Unit.Percentage(100);
                reportViewer.Height = Unit.Percentage(100);
                reportViewer.ServerReport.ReportServerUrl = new Uri(reportServerUrl);
                reportViewer.ServerReport.ReportPath = url;

                // Crear parámetros para las fechas de inicio y fin
                ReportParameter secuencialClienteParam = new ReportParameter("secuencialCliente", cliente.ToString());
                ReportParameter fechaInicioParam = new ReportParameter("fechaInicio", firstDayOfPreviousMonth.ToString("yyyy-MM-dd"));
                ReportParameter fechaFinParam = new ReportParameter("fechaFin", lastDayOfPreviousMonth.ToString("yyyy-MM-dd"));

                reportViewer.ServerReport.SetParameters(new ReportParameter[] { secuencialClienteParam, fechaInicioParam, fechaFinParam });
                ViewBag.ReportViewer = reportViewer;

                HttpContext.Cache.Remove(ReportAccessKey + code);

                return View();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


    }
}