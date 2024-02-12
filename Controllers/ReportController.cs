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

namespace SifizPlanning.Controllers
{
	public class ReportController : Controller
	{
		SifizPlanningEntidades db = DbCnx.getCnx();

		// GET: Report
		//PANTALLA INICIAL
		[Authorize(Roles = "COORDINADOR, ADMIN")]
		public ActionResult Index()
		{
			return View();
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

    }
}