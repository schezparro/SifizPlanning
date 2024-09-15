using Hangfire;
using Hangfire.Storage;
using SifizPlanning.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SifizPlanning
{
    public class Hangfire
    {
        HangfireController hangfireController = new HangfireController();
        public void Start()
        {
            //RecurringJob.AddOrUpdate("CalcularSemaforoTciket", () => hangfireController.CalcularSemaforoTciket(), "0 * * * *", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("CalcularResolucionSemaforoTciket", () => hangfireController.CalcularResolucionSemaforoTciket(), "5 * * * *", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("ActualizarCapacitaciones", () => hangfireController.ActualizarCapacitaciones(), "30 2 * * *", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("CerrarTicketsResueltos", () => hangfireController.CerrarTicketsResueltos(), "0 3 * * *", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("SetDataInfoTickets", () => hangfireController.SetDataInfoTickets(), "30 3 * * *", TimeZoneInfo.Local);


            //RecurringJob.AddOrUpdate("TareaReporteHorasMantenimiento", () => hangfireController.TareaReporteHorasMantenimiento(), Cron.Monthly(), TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("TareaReporteIncidencias", () => hangfireController.TareaReporteIncidencias(), Cron.Monthly(), TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("TareaReporteEstadoTicketsColaborador", () => hangfireController.TareaReporteEstadoTicketsColaborador(), Cron.Daily(), TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("TareaReporteEstadoContratosColaborador", () => hangfireController.TareaReporteEstadoContratosColaborador(), Cron.Weekly(), TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("TareaReporteSeguimientoColaborador", () => hangfireController.TareaReporteSeguimientoColaborador(), Cron.Daily(), TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("SetDataInfoTickets", () => hangfireController.SetDataInfoTickets(), Cron.Daily(), TimeZoneInfo.Local);
            //BackgroundJob.Enqueue(() => hangfireController.SetDataInfoTickets());
            //using (var connection = JobStorage.Current.GetConnection())
            //{
            //    foreach (var recurringJob in connection.GetRecurringJobs())
            //    {
            //        RecurringJob.RemoveIfExists(recurringJob.Id);
            //    }
            //}
            //BackgroundJob.Enqueue(() => hangfireController.TareaReporteIncidencias());
            //BackgroundJob.Enqueue(() => hangfireController.TareaReporteSeguimiento());
            //BackgroundJob.Enqueue(() => hangfireController.TareaReporteSeguimientoColaborador());
            //BackgroundJob.Enqueue(() => hangfireController.TareaReporteHorasMantenimiento());
            //BackgroundJob.Schedule(() => hangfireController.TareaReporteIncidencias(), TimeSpan.FromMinutes(2));
            //RecurringJob.AddOrUpdate("EnviarEmailReporteTicketsMinuto", () => hangfireController.ReporteHorasMantenimiento(), Cron.Daily, TimeZoneInfo.Local);
        }
    }
}