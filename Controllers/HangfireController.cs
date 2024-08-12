using SifizPlanning.Models;
using SifizPlanning.Security;
using SifizPlanning.Util;
using SpreadsheetLight;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Hosting;
using SpreadsheetLight.Charts;
using System.Threading.Tasks;

namespace SifizPlanning.Controllers
{
    public class HangfireController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();
        public ActionResult TareaReporteHorasMantenimiento()
        {
            List<string> listaPath = new List<string>();
            try
            {
                DateTimeFormatInfo dateTimeFormatInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                DateTime fecha = DateTime.Today.AddDays(-15);
                DateTime fechaInicioMes = new DateTime(fecha.Year, fecha.Month, 1);
                DateTime fechaFinMes = fechaInicioMes.AddMonths(1);

                var clientes = db.Cliente.Where(c => c.EstaActivo == 1).OrderBy(s => s.Codigo).ToList();
                foreach (var cliente in clientes)
                {
                    List<string> listaPathFicheros = new List<string>();
                    List<string> usuariosDestinos = new List<string>();
                    //usuariosDestinos.Add("rsanchez@sifizsoft.com");
                    usuariosDestinos.Add("asistenteoperaciones@sifizsoft.com");

                    var contratosPorCliente = (from mt in db.MotivoTrabajo
                                               join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
                                               join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
                                               where mt.EstaActivo == 1 && cl.Secuencial == cliente.Secuencial && tt.Codigo == "PENDIENTES"
                                               orderby mt.Secuencial
                                               select new
                                               {
                                                   codigo = mt.Codigo,
                                                   fecha = mt.FechaInicio,
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
                                                             DbFunctions.DiffDays(DateTime.Now, mt.FechaFin) < 0 ? "VENCIDO" : "POR VENCER"
                                               }).ToList();
                    if (contratosPorCliente.Count > 0)
                    {
                        var contratoMantenimiento = contratosPorCliente.Where(c => c.estado != "CERRADO").OrderByDescending(s => s.fecha).FirstOrDefault();
                        if (contratoMantenimiento != null)
                        {
                            var horasMensuales = contratoMantenimiento != null ?
                                contratoMantenimiento.horas != null ?
                                (int)contratoMantenimiento.horas : 0 : 0;

                            DateTime fechaInicioContrato = new DateTime(contratoMantenimiento.fecha.Year, contratoMantenimiento.fecha.Month, 1);

                            using (SLDocument sl = new SLDocument())
                            {
                                SLStyle style1 = sl.CreateStyle();
                                style1.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
                                SLStyle style2 = sl.CreateStyle();
                                style2.SetHorizontalAlignment(HorizontalAlignmentValues.Left);
                                SLStyle style3 = sl.CreateStyle();
                                style3.SetHorizontalAlignment(HorizontalAlignmentValues.Left);
                                SLStyle style4 = sl.CreateStyle();
                                style4.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
                                style4.SetVerticalAlignment(VerticalAlignmentValues.Justify);
                                SLStyle style5 = sl.CreateStyle();
                                style5.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
                                style5.SetVerticalAlignment(VerticalAlignmentValues.Justify);
                                SLStyle style6 = sl.CreateStyle();
                                style6.SetHorizontalAlignment(HorizontalAlignmentValues.Left);
                                style6.SetVerticalAlignment(VerticalAlignmentValues.Justify);
                                SLStyle style7 = sl.CreateStyle();
                                style7.SetHorizontalAlignment(HorizontalAlignmentValues.Right);
                                style7.SetVerticalAlignment(VerticalAlignmentValues.Justify);
                                SLStyle style8 = sl.CreateStyle();
                                style8.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
                                style8.SetVerticalAlignment(VerticalAlignmentValues.Center);

                                style1.Font.Bold = true;
                                style1.Font.FontSize = 12;
                                style2.Font.Bold = true;
                                style2.Font.FontSize = 8;
                                style3.Font.Bold = false;
                                style3.Font.FontSize = 8;
                                style4.Font.Bold = true;
                                style4.Font.FontSize = 9;
                                style4.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.Blue);
                                style5.Font.Bold = false;
                                style5.Font.FontSize = 9;
                                style6.Font.Bold = false;
                                style6.Font.FontSize = 9;
                                style7.Font.Bold = true;
                                style7.Font.FontSize = 9;
                                style7.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.FromArgb(1, 244, 176, 132), System.Drawing.Color.Blue);
                                style8.Font.Bold = true;
                                style8.Font.FontSize = 9;
                                style8.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.FromArgb(1, 244, 176, 132), System.Drawing.Color.Blue);
                                style8.Border.BottomBorder.BorderStyle = BorderStyleValues.Double;
                                style7.Border.BottomBorder.BorderStyle = BorderStyleValues.Double;
                                style7.Border.BottomBorder.Color = System.Drawing.Color.Black;
                                style8.Border.BottomBorder.Color = System.Drawing.Color.Black;
                                sl.SetCellStyle(1, 1, style1);
                                sl.SetCellStyle(2, 1, style1);
                                sl.SetCellValue("A1", "INFORME DE HORAS POR MANTENIMIENTO Y SOPORTE");
                                sl.SetCellValue("A2", "CONTRATO " + contratoMantenimiento.codigo.ToUpper());
                                sl.MergeWorksheetCells("A1", "G1");
                                sl.MergeWorksheetCells("A2", "G2");

                                int contador = 0;
                                int cantidadTicketsActual = 0;
                                List<string> usuariosClientes = new List<string>();
                                usuariosClientes.Add("Raul");
                                usuariosClientes.Add("Elizabeth");
                                List<ClienteHoras> listaHorasMes = new List<ClienteHoras>();
                                while (fechaInicioContrato != fechaFinMes)
                                {
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
                                                            where
                                                               T.SecuencialCliente == cliente.Secuencial
                                                               && T.FechaInicio >= fechaInicioContrato
                                                               && T.FechaInicio < SqlFunctions.DateAdd("month", 1, fechaInicioContrato)
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
                                                                estado = ETG.Codigo ?? "FINALIZADO",
                                                                tiempo = (SqlFunctions.DateDiff("MINUTE", TAR.HoraInicio, TAR.HoraFin)) / 60.00
                                                            }).ToList();

                                    var tickets = ticketsAgrupados.GroupBy(g => new { g.cliente, g.ticket, g.detalle, g.reportador, g.tecnico, g.fecha, g.estado, g.tarea })
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

                                    var mantenimientoTarea = db.TareaMantenimientoBorrar.Where(s => s.Fecha >= fechaInicioContrato && s.Fecha < SqlFunctions.DateAdd("month", 1, fechaInicioContrato)).ToList();
                                    var mantenimientoTicket = db.TicketMantenimientoBorar.Where(s => s.Fecha >= fechaInicioContrato && s.Fecha < SqlFunctions.DateAdd("month", 1, fechaInicioContrato)).ToList();

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

                                    var ticketsAdd = db.TicketMantenimientoAgregar.Where(s => s.Fecha >= fechaInicioContrato && s.Fecha < SqlFunctions.DateAdd("month", 1, fechaInicioContrato)).ToList();
                                    var clienteAdd = db.Cliente.Where(s => s.Secuencial == cliente.Secuencial).FirstOrDefault();
                                    var ticketAgregados = ticketsAdd.Where(s => s.Cliente == clienteAdd.Codigo + "-" + clienteAdd.Descripcion).ToList();
                                    foreach (var c in ticketAgregados)
                                    {
                                        tickets.Add(new { tarea = -1, cliente = c.Cliente, numero = c.TicketTarea, detalle = c.Detalle, reportado = c.Reportado, asignado = c.Tecnico, fecha = c.Fecha.ToString("dd/MM/yyyy"), fechaComparacion = fechaInicioContrato, estado = c.Estado, tiempo = (double)c.Tiempo });
                                    }
                                    tickets = tickets.OrderBy(s => s.fecha).ToList();

                                    if (tickets.Count > 0)
                                    {
                                        var totalhoras = tickets.Sum(s => s.tiempo);

                                        sl.SetCellStyle(5 + (contador * 11) + cantidadTicketsActual, 1, style2);
                                        sl.SetCellStyle(5 + (contador * 11) + cantidadTicketsActual, 4, style2);
                                        sl.SetCellStyle(7 + (contador * 11) + cantidadTicketsActual, 1, style2);
                                        sl.SetCellStyle(8 + (contador * 11) + cantidadTicketsActual, 1, style2);
                                        sl.SetCellStyle(5 + (contador * 11) + cantidadTicketsActual, 2, style3);
                                        sl.SetCellStyle(5 + (contador * 11) + cantidadTicketsActual, 5, style3);
                                        sl.SetCellStyle(7 + (contador * 11) + cantidadTicketsActual, 2, style3);
                                        sl.SetCellStyle(8 + (contador * 11) + cantidadTicketsActual, 2, style3);
                                        sl.SetCellStyle(10 + (contador * 11) + cantidadTicketsActual, 1, style4);
                                        sl.SetCellStyle(10 + (contador * 11) + cantidadTicketsActual, 2, style4);
                                        sl.SetCellStyle(10 + (contador * 11) + cantidadTicketsActual, 3, style4);
                                        sl.SetCellStyle(10 + (contador * 11) + cantidadTicketsActual, 4, style4);
                                        sl.SetCellStyle(10 + (contador * 11) + cantidadTicketsActual, 5, style4);
                                        sl.SetCellStyle(10 + (contador * 11) + cantidadTicketsActual, 6, style4);
                                        sl.SetCellStyle(10 + (contador * 11) + cantidadTicketsActual, 7, style4);

                                        sl.SetCellValue($"A{5 + (contador * 11) + cantidadTicketsActual}", "CLIENTE:");
                                        sl.SetCellValue($"A{7 + (contador * 11) + cantidadTicketsActual}", "DESDE:");
                                        sl.SetCellValue($"A{8 + (contador * 11) + cantidadTicketsActual}", "HASTA:");
                                        sl.SetCellValue($"D{5 + (contador * 11) + cantidadTicketsActual}", "FECHA CORTE:");
                                        sl.SetCellValue($"B{5 + (contador * 11) + cantidadTicketsActual}", cliente.Descripcion);
                                        sl.SetCellValue($"B{7 + (contador * 11) + cantidadTicketsActual}", fechaInicioContrato.ToString("dd/MM/yyyy"));
                                        sl.SetCellValue($"B{8 + (contador * 11) + cantidadTicketsActual}", fechaInicioContrato.AddMonths(1).AddDays(-1).ToString("dd/MM/yyyy"));
                                        sl.SetCellValue($"E{5 + (contador * 11) + cantidadTicketsActual}", "FIN DE MES");
                                        sl.SetCellValue($"A{10 + (contador * 11) + cantidadTicketsActual}", "TICKET");
                                        sl.SetCellValue($"B{10 + (contador * 11) + cantidadTicketsActual}", "DETALLE");
                                        sl.SetCellValue($"C{10 + (contador * 11) + cantidadTicketsActual}", "REPORTADO POR");
                                        sl.SetCellValue($"D{10 + (contador * 11) + cantidadTicketsActual}", "TECNICO ASIGNADO");
                                        sl.SetCellValue($"E{10 + (contador * 11) + cantidadTicketsActual}", "FECHA");
                                        sl.SetCellValue($"F{10 + (contador * 11) + cantidadTicketsActual}", "ESTADO");
                                        sl.SetCellValue($"G{10 + (contador * 11) + cantidadTicketsActual}", "TIEMPO (h)");

                                        foreach (var ticket in tickets.Select((value, index) => new { value, index }))
                                        {
                                            sl.SetCellStyle(ticket.index + 11 + (contador * 11) + cantidadTicketsActual, 1, style5);
                                            sl.SetCellStyle(ticket.index + 11 + (contador * 11) + cantidadTicketsActual, 2, style6);
                                            sl.SetCellStyle(ticket.index + 11 + (contador * 11) + cantidadTicketsActual, 3, style5);
                                            sl.SetCellStyle(ticket.index + 11 + (contador * 11) + cantidadTicketsActual, 4, style5);
                                            sl.SetCellStyle(ticket.index + 11 + (contador * 11) + cantidadTicketsActual, 5, style5);
                                            sl.SetCellStyle(ticket.index + 11 + (contador * 11) + cantidadTicketsActual, 6, style5);
                                            sl.SetCellStyle(ticket.index + 11 + (contador * 11) + cantidadTicketsActual, 7, style5);
                                            sl.SetCellValue("A" + (ticket.index + 11 + (contador * 11) + cantidadTicketsActual), ticket.value.numero);
                                            sl.SetCellValue("B" + (ticket.index + 11 + (contador * 11) + cantidadTicketsActual), ticket.value.detalle);
                                            sl.SetCellValue("C" + (ticket.index + 11 + (contador * 11) + cantidadTicketsActual), ticket.value.reportado);
                                            sl.SetCellValue("D" + (ticket.index + 11 + (contador * 11) + cantidadTicketsActual), ticket.value.asignado);
                                            sl.SetCellValue("E" + (ticket.index + 11 + (contador * 11) + cantidadTicketsActual), ticket.value.fecha);
                                            sl.SetCellValue("F" + (ticket.index + 11 + (contador * 11) + cantidadTicketsActual), ticket.value.estado);
                                            sl.SetCellValue("G" + (ticket.index + 11 + (contador * 11) + cantidadTicketsActual), ticket.value.tiempo);
                                        }

                                        sl.SetCellValue("F" + (tickets.Count + 11 + (contador * 11) + cantidadTicketsActual), "HORAS TRABAJADAS");
                                        sl.SetCellValue("F" + (tickets.Count + 12 + (contador * 11) + cantidadTicketsActual), "HORAS CONTRATADAS");
                                        sl.SetCellValue("F" + (tickets.Count + 13 + (contador * 11) + cantidadTicketsActual), "DIFERENCIA DE HORAS");
                                        sl.SetCellValue("G" + (tickets.Count + 11 + (contador * 11) + cantidadTicketsActual), totalhoras);
                                        sl.SetCellValue("G" + (tickets.Count + 12 + (contador * 11) + cantidadTicketsActual), horasMensuales);
                                        sl.SetCellValue("G" + (tickets.Count + 13 + (contador * 11) + cantidadTicketsActual), horasMensuales - totalhoras);
                                        sl.SetCellStyle((tickets.Count + 11 + (contador * 11) + cantidadTicketsActual), 6, style7);
                                        sl.SetCellStyle((tickets.Count + 12 + (contador * 11) + cantidadTicketsActual), 6, style7);
                                        sl.SetCellStyle((tickets.Count + 13 + (contador * 11) + cantidadTicketsActual), 6, style7);
                                        sl.SetCellStyle((tickets.Count + 11 + (contador * 11) + cantidadTicketsActual), 7, style8);
                                        sl.SetCellStyle((tickets.Count + 12 + (contador * 11) + cantidadTicketsActual), 7, style8);
                                        sl.SetCellStyle((tickets.Count + 13 + (contador * 11) + cantidadTicketsActual), 7, style8);

                                        SLTable tbl = sl.CreateTable($"A{10 + (contador * 11) + cantidadTicketsActual}", "G" + (tickets.Count + 10 + (contador * 11) + cantidadTicketsActual));
                                        tbl.SetTableStyle(SLTableStyleTypeValues.Light8);
                                        tbl.HasBandedColumns = true;
                                        tbl.HasBandedRows = true;
                                        tbl.Sort(5, true);
                                        sl.InsertTable(tbl);

                                        sl.SetColumnWidth(1, 11);
                                        sl.SetColumnWidth(2, 43);
                                        sl.SetColumnWidth(3, 11);
                                        sl.SetColumnWidth(4, 20);
                                        sl.SetColumnWidth(5, 11);
                                        sl.SetColumnWidth(6, 15);
                                        sl.SetColumnWidth(7, 11);
                                        sl.SetColumnWidth(8, 11);

                                        ClienteHoras clienteHoras = new ClienteHoras();
                                        clienteHoras.mes = dateTimeFormatInfo.GetMonthName(fechaInicioContrato.Month).ToLower().Substring(0, 3);
                                        clienteHoras.contratadas = horasMensuales;
                                        clienteHoras.trabajadas = (int)totalhoras;
                                        clienteHoras.total = horasMensuales - (int)totalhoras;
                                        listaHorasMes.Add(clienteHoras);

                                        contador++;
                                        cantidadTicketsActual += tickets.Count;

                                    }
                                    fechaInicioContrato = fechaInicioContrato.AddMonths(1);
                                }
                                string path = HostingEnvironment.MapPath("~/Web/" + cliente.Codigo + ".xlsx");
                                if (contador > 0)
                                {
                                    sl.AddWorksheet("GRAFICO");
                                    SLStyle style10 = sl.CreateStyle();
                                    style10.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
                                    style10.SetVerticalAlignment(VerticalAlignmentValues.Justify);
                                    style10.Font.Bold = true;
                                    style10.Font.FontSize = 9;
                                    style10.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.Blue);

                                    sl.SetCellStyle(1, 1, style10);
                                    sl.SetCellStyle(1, 2, style10);
                                    sl.SetCellStyle(1, 3, style10);
                                    sl.SetCellStyle(1, 4, style10);
                                    sl.SetCellStyle(1, 5, style10);
                                    sl.SetCellValue(1, 1, "FECHA");
                                    sl.SetCellValue(1, 2, "HORAS TRABAJADAS");
                                    sl.SetCellValue(1, 3, "HORAS CONTRATADAS");
                                    sl.SetCellValue(1, 4, "TOTAL HORAS");
                                    sl.SetCellValue(1, 5, "DESCONTAR");

                                    int count = 2;
                                    foreach (var list in listaHorasMes)
                                    {
                                        sl.SetCellStyle(count, 1, style5);
                                        sl.SetCellStyle(count, 2, style5);
                                        sl.SetCellStyle(count, 3, style5);
                                        sl.SetCellStyle(count, 4, style5);
                                        sl.SetCellStyle(count, 5, style5);
                                        sl.SetCellValue(count, 1, list.mes);
                                        sl.SetCellValue(count, 2, list.trabajadas);
                                        sl.SetCellValue(count, 3, list.contratadas);
                                        sl.SetCellValue(count, 4, list.total);
                                        sl.SetCellValue(count, 5, "-");
                                        count++;
                                    }
                                    sl.SetCellStyle(count, 1, style10);
                                    sl.SetCellStyle(count, 2, style10);
                                    sl.SetCellStyle(count, 3, style10);
                                    sl.SetCellStyle(count, 4, style10);
                                    sl.SetCellStyle(count, 5, style10);
                                    sl.SetCellValue(count, 1, "Total General");
                                    sl.SetCellValue(count, 2, listaHorasMes.Sum(s => s.trabajadas));
                                    sl.SetCellValue(count, 3, listaHorasMes.Sum(s => s.contratadas));
                                    sl.SetCellValue(count, 4, listaHorasMes.Sum(s => s.total));
                                    sl.SetCellValue(count, 5, "-");

                                    SLTable tbl1 = sl.CreateTable("A1", "E" + (listaHorasMes.Count + 2));
                                    tbl1.SetTableStyle(SLTableStyleTypeValues.Light8);
                                    tbl1.HasBandedColumns = true;
                                    tbl1.HasBandedRows = true;
                                    sl.InsertTable(tbl1);

                                    sl.SetColumnWidth(1, 11);
                                    sl.SetColumnWidth(2, 11);
                                    sl.SetColumnWidth(3, 11);
                                    sl.SetColumnWidth(4, 11);
                                    sl.SetColumnWidth(5, 11);

                                    SLChart chart;
                                    chart = sl.CreateChart("A1", "C" + (listaHorasMes.Count + 1));
                                    chart.SetChartType(SLColumnChartType.ClusteredColumn);
                                    chart.SetChartStyle(SLChartStyle.Style2);
                                    chart.SetChartPosition(1, 7, 13.5, listaHorasMes.Count > 1 ? listaHorasMes.Count > 5 ? (7 + listaHorasMes.Count + (listaHorasMes.Count * 0.2)) : 14 : 10.5);

                                    SLGroupDataLabelOptions gdloptions;
                                    gdloptions = chart.CreateGroupDataLabelOptions();
                                    gdloptions.ShowValue = true;
                                    chart.SetGroupDataLabelOptions(2, gdloptions);
                                    chart.SetGroupDataLabelOptions(1, gdloptions);
                                    SLRstType rst = sl.CreateRstType();
                                    rst.AppendText("Horas Mensuales Consumidas por Contrato");
                                    chart.Title.SetTitle(rst);
                                    chart.ShowChartTitle(false);

                                    sl.InsertChart(chart);

                                    sl.SaveAs(path);

                                    string asuntoEmail = cliente.Codigo + " - " + "INFORME DE HORAS DE SOPORTE DE " + dateTimeFormatInfo.GetMonthName(fecha.Month).ToUpper();
                                    string comentarioEmail = "Buenos dias<br>" +
                                                             "Estimado/s " + usuariosClientes.Aggregate((x, y) => x + ", " + y) + ",<br>" +
                                                             "Por medio de la presente, adjunto el informe de las horas consumidas por el contrato de Soporte y Mantenimiento asignado con el Número " + contratoMantenimiento.codigo + " suscrito el " + contratoMantenimiento.fecha + " por " + (horasMensuales) + " horas mensuales, las cuales se detallan a continuación:<br>";
                                    string htmlMail = @"<div class='textoCuerpo'><h4>";
                                    htmlMail += comentarioEmail +
                                        "</h4><table> <thead>";
                                    htmlMail += "<tr>" +
                                                    "<th> FECHA </th>" +
                                                    "<th> HORAS TRABAJADAS POR MES </th>" +
                                                    "<th> HORAS CONTRATADAS </th>" +
                                                    "<th> HORAS FAVOR (+) ó EXCEDENTE (-) </th>" +
                                                    "<th> SE ACUMULA/FACTURA </th>" +
                                                "</tr>" +
                                           "</thead>";
                                    htmlMail += "<tbody>";

                                    htmlMail += "<tr>" +
                                                    "<td>" + listaHorasMes.Last().mes + "</td>" +
                                                    "<td>" + listaHorasMes.Last().trabajadas + "</td>" +
                                                    "<td>" + listaHorasMes.Last().contratadas + "</td>" +
                                                    "<td>" + listaHorasMes.Last().total + "</td>" +
                                                    "<td>" + " - " + "</td>" +
                                                "</tr>";
                                    htmlMail += "</tbody></table><br/></div>";
                                    string htmlCss = @"<style>
                                  .textoCuerpo {
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    color: #1f497d
                                  }
                                  .cabecera {
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  table th {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #b0c4de;
                                    font-family: ""Calibri"", sans-serif
                                  }
                                  table,
                                  td {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #eed;
                                    font-family: ""Calibri"", sans-serif;
                                    vertical-align: top;
                                    text-align: center;
                                  }
                                  td,
                                  th {
                                    padding: 10px
                                  }
                                  .resaltar {
                                    background: #ff0 !important;
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  @font-face {
                                    font-family: ""Cambria Math"";
                                    panose-1: 2 4 5 3 5 4 6 3 2 4
                                  }
                                  @font-face {
                                    font-family: Calibri;
                                    panose-1: 2 15 5 2 2 2 4 3 2 4
                                  }
                                  @font-face {
                                    font-family: Verdana;
                                    panose-1: 2 11 6 4 3 5 4 4 2 4
                                  }
                                  @font-face {
                                    font-family: ""Palatino Linotype"";
                                    panose-1: 2 4 5 2 5 5 5 3 3 4
                                  }
                                  div.MsoNormal,
                                  li.MsoNormal,
                                  p.MsoNormal {
                                    margin: 0;
                                    margin-bottom: .0001pt;
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  a:link,
                                  span.MsoHyperlink {
                                    mso-style-priority: 99;
                                    color: #0563c1;
                                    text-decoration: underline
                                  }
                                  a:visited,
                                  span.MsoHyperlinkFollowed {
                                    mso-style-priority: 99;
                                    color: #954f72;
                                    text-decoration: underline
                                  }
                                  span.EstiloCorreo17 {
                                    mso-style-type: personal-compose;
                                    font-family: ""Calibri"", sans-serif;
                                    color: windowtext
                                  }
                                  .MsoChpDefault {
                                    mso-style-type: export-only;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  @page WordSection1 {
                                    size: 612pt 792pt;
                                    margin: 70.85pt 3cm 70.85pt 3cm
                                  }
                                  div.WordSection1 {
                                    page: WordSection1
                                  }
                                </style>";

                                    listaPathFicheros.Add(path);
                                    listaPath.Add(path);

                                    Utiles.EnviarEmailSistemaPersonalizadoAsync(usuariosDestinos.ToArray(), htmlMail, htmlCss, asuntoEmail, listaPathFicheros.ToArray());
                                }
                            }
                        }
                    }
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
            finally
            {
                if (listaPath.Count > 0)
                {
                    foreach (string path in listaPath)
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);
                }
            }
        }
        public ActionResult TareaReporteIncidencias()
        {
            string asuntoEmail = "SFZ-REPORTE DE INDICENCIAS DEL REGISTRO DE TAREAS";
            try
            {
                DateTimeFormatInfo dateTimeFormatInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                DateTime fecha = DateTime.Today.AddDays(-15);
                DateTime fechaInicio = new DateTime(fecha.Year, fecha.Month, 1);
                DateTime fechaFin = fechaInicio.AddMonths(1);

                var incidencias = (from ci in db.ColaboradorIncidencia
                                   join c in db.Colaborador on ci.colaborador equals c
                                   join p in db.Persona on c.persona equals p
                                   join e in db.TipoError on ci.tipoError equals e
                                   where (e.Codigo == "NO_REG" || e.Codigo == "REG_INCOM" || e.Codigo == "REG_INCOR")
                                          && (ci.FechaIncidencia >= fechaInicio && ci.FechaIncidencia <= fechaFin)
                                   group new { e, ci, ci.SecuencialColaborador } by new { e.Codigo, nombre = (p.Nombre1 + " " + p.Apellido1) } into gro
                                   select new
                                   {
                                       codigo = gro.Key.Codigo,
                                       cantidad = gro.Where(g => g.e.Secuencial == g.ci.SecuencialTipoError).Select(s => s.SecuencialColaborador).Count(),
                                       colaborador = gro.Key.nombre
                                   }).OrderBy(o => o.colaborador).ToList();

                var result = (from e in incidencias
                              group e by new { e.colaborador } into g
                              select new
                              {
                                  colaborador = g.Key.colaborador,
                                  NO_REG = g.Where(w => w.codigo == "NO_REG").Sum(s => s.cantidad),
                                  REG_INCOM = g.Where(w => w.codigo == "REG_INCOM").Sum(s => s.cantidad),
                                  REG_INCOR = g.Where(w => w.codigo == "REG_INCOR").Sum(s => s.cantidad)
                              }).ToList();

                List<string> usuariosDestinos = new List<string>();
                usuariosDestinos.Add("gerencia@sifizsoft.com");
                usuariosDestinos.Add("asistenteoperaciones@sifizsoft.com");
                //usuariosDestinos.Add("rsanchez@sifizsoft.com");

                var colaboradores = (from c in db.Colaborador
                                     join p in db.Persona on c.persona equals p
                                     join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                     join d in db.Departamento on c.departamento equals d
                                     where u.EstaActivo == 1 && d.Asignable == 1
                                     select new
                                     {
                                         c.Secuencial
                                     }).ToList();

                decimal porcentaje = Math.Round(((decimal)result.Count / (decimal)colaboradores.Count) * 100, 2);
                string comentarioEmail = "Buenos días<br>" +
                    "Estimado Ing. Santiago,<br>" +
                    "Por medio de la presente, se envía reporte de registro de tareas en el Sifizplanning que se revisa a diario a cada colaborador.<br>" +
                    "De los " + colaboradores.Count + " colaboradores que se encuentran en el Sifizplanning, tenemos " + result.Count + " personas con incidencias que no han registrado el detalle de sus tareas que equivale al " + porcentaje + "%.<br>" +
                    "En este reporte se puede visualizar por cada colaborador que tipo de incidencia tiene y cuantas tuvo durante el mes de " + fecha.ToString("MMMM", dateTimeFormatInfo) + ".<br>" +
                    "Quedamos atentos a sus comentarios.<br><br>";
                string htmlMail = @"<div class='textoCuerpo'>";
                htmlMail += comentarioEmail;
                htmlMail += "<table><thead>";
                htmlMail +=
                    "<tr> " +
                        "<th colspan='1' rowspan='3'>No.</th>" +
                        "<th colspan='2' rowspan='3'>COLABORADOR</th>" +
                        "<th colspan='3'>TIPOS DE INCIDENCIA</th>" +
                    "</tr>" +
                    "<tr>" +
                        "<th colspan='1'>REGISTRO INCORRECTO</th>" +
                        "<th colspan='1'>REGISTRO INCOMPLETO</th>" +
                        "<th colspan='1'>NO REGISTRO</th>" +
                    "</tr>" +
                    "<tr>" +
                        "<th colspan='1'>(-1) BAJO</th>" +
                        "<th colspan='1'>(-2) MEDIO</th>" +
                        "<th colspan='1'>(-3) ALTO</th>" +
                    "</tr>" +
                "</thead>";
                htmlMail += "<tbody>";

                foreach (var i in result.Select((value, index) => new { value, index }))
                {
                    htmlMail += "<tr>" +
                                    "<td>" + (i.index + 1) + "</td>" +
                                    "<td colspan='2'>" + i.value.colaborador + "</td>" +
                                    "<td>" + i.value.REG_INCOR + "</td>" +
                                    "<td>" + i.value.REG_INCOM + "</td>" +
                                    "<td>" + i.value.NO_REG + "</td>" +
                                "</tr>";
                }
                htmlMail += "<tr>" +
                                    "<th colspan='3'>" + "TOTAL" + "</th>" +
                                    "<th>" + result.Sum(c => c.REG_INCOR) + "</th>" +
                                    "<th>" + result.Sum(c => c.REG_INCOM) + "</th>" +
                                    "<th>" + result.Sum(c => c.NO_REG) + "</th>" +
                                "</tr>";
                htmlMail += "</tbody></table><br></div>";

                string htmlCss = @"<style>
                                  .textoCuerpo {
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    color: #1f497d
                                  }
                                  .cabecera {
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  table th {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #b0c4de;
                                    font-family: ""Calibri"", sans-serif
                                  }
                                  table,
                                  td {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #eed;
                                    font-family: ""Calibri"", sans-serif;
                                    vertical-align: top;
                                    text-align: center;
                                  }
                                  td,
                                  th {
                                    padding: 10px
                                  }
                                  .resaltar {
                                    background: #ff0 !important;
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  @font-face {
                                    font-family: ""Cambria Math"";
                                    panose-1: 2 4 5 3 5 4 6 3 2 4
                                  }
                                  @font-face {
                                    font-family: Calibri;
                                    panose-1: 2 15 5 2 2 2 4 3 2 4
                                  }
                                  @font-face {
                                    font-family: Verdana;
                                    panose-1: 2 11 6 4 3 5 4 4 2 4
                                  }
                                  @font-face {
                                    font-family: ""Palatino Linotype"";
                                    panose-1: 2 4 5 2 5 5 5 3 3 4
                                  }
                                  div.MsoNormal,
                                  li.MsoNormal,
                                  p.MsoNormal {
                                    margin: 0;
                                    margin-bottom: .0001pt;
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  a:link,
                                  span.MsoHyperlink {
                                    mso-style-priority: 99;
                                    color: #0563c1;
                                    text-decoration: underline
                                  }
                                  a:visited,
                                  span.MsoHyperlinkFollowed {
                                    mso-style-priority: 99;
                                    color: #954f72;
                                    text-decoration: underline
                                  }
                                  span.EstiloCorreo17 {
                                    mso-style-type: personal-compose;
                                    font-family: ""Calibri"", sans-serif;
                                    color: windowtext
                                  }
                                  .MsoChpDefault {
                                    mso-style-type: export-only;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  @page WordSection1 {
                                    size: 612pt 792pt;
                                    margin: 70.85pt 3cm 70.85pt 3cm
                                  }
                                  div.WordSection1 {
                                    page: WordSection1
                                  }
                                </style>";

                string htmlFinal = htmlCss + htmlMail;
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
        public ActionResult TareaReporteSeguimiento()
        {
            string asuntoEmail = "Reporte de Seguimiento Diario";
            try
            {
                DateTimeFormatInfo dateTimeFormatInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                DateTime fecha = DateTime.Now.AddDays(-1);

                var detalleSeguimientos = (from t in db.Tarea
                                           join et in db.EstadoTarea on t.estadoTarea equals et
                                           join c in db.Colaborador on t.colaborador equals c
                                           join p in db.Persona on c.persona equals p
                                           where (t.FechaInicio.Year == fecha.Year && t.FechaInicio.Month == fecha.Month && t.FechaInicio.Day == fecha.Day && et.Codigo != "ANULADA" && et.Codigo != "PREASIGNADA")
                                           select new
                                           {
                                               tarea = t.Detalle,
                                               cliente = t.cliente.Descripcion,
                                               colaborador = p.Nombre1 + " " + p.Apellido1,
                                               asignado = (t.FechaInicio.Hour < 13 && t.FechaFin.Hour > 13) ? DbFunctions.DiffHours(t.FechaInicio, t.FechaFin) - 1 :
                                                           DbFunctions.DiffHours(t.FechaInicio, t.FechaFin),
                                               reportado = t.HorasUtilizadas,
                                               estado = et.Codigo
                                           }).OrderBy(o => o.colaborador).ToList();

                var seguimientos = (from a in detalleSeguimientos
                                    group new { a.tarea, a.asignado, a.reportado, a.estado } by new { a.colaborador } into gro
                                    select new
                                    {
                                        gro.Key.colaborador,
                                        completadas = gro.Where(w => w.estado == "TERMINADA").Count(),
                                        inconclusas = gro.Where(w => w.estado == "INCONCLUSA").Count(),
                                        asignada = gro.Where(w => w.estado == "ASIGNADA").Count(),
                                        pausa = gro.Where(w => w.estado == "PAUSA").Count(),
                                        desarrollo = gro.Where(w => w.estado == "DESARROLLO").Count(),
                                        asignado = gro.Sum(s => s.asignado),
                                        reportado = gro.Sum(s => s.reportado)
                                    }).ToList();

                List<string> usuariosDestinos = new List<string>();
                //usuariosDestinos.Add("rsanchez@sifizsoft.com");

                string comentarioEmail = "Seguimiento por colaborador del " + fecha.ToString("dd/MM/yyyy") + ".";
                string htmlMail = @"<div class='textoCuerpo'><h4>";
                htmlMail += comentarioEmail +
                       "</h4><table><thead>";
                htmlMail += "<tr>" +
                                "<th> COLABORADOR </th>" +
                                "<th> TIEMPO ASIGNADO </th>" +
                                "<th> TIEMPO REPORTADO </th>" +
                                "<th> TAREAS COMPLETADAS </th>" +
                                "<th> TAREAS INCONCLUSAS </th>" +
                                "<th> TAREAS NO REALIZADAS </th>" +
                                "<th> TAREAS EN PAUSA </th>" +
                                "<th> TAREAS EN DESARROLLO </th>" +
                            "</tr>" +
                       "</thead>";
                htmlMail += "<tbody>";
                foreach (var item in seguimientos)
                {
                    htmlMail += "<tr>" +
                                    "<td>" + item.colaborador + "</td>" +
                                    "<td>" + item.asignado + "</td>" +
                                    "<td>" + item.reportado + "</td>" +
                                    "<td>" + item.completadas + "</td>" +
                                    "<td>" + item.inconclusas + "</td>" +
                                    "<td>" + item.asignada + "</td>" +
                                    "<td>" + item.pausa + "</td>" +
                                    "<td>" + item.desarrollo + "</td>" +
                                "</tr>";
                }
                htmlMail += "</tbody></table><br></div>";

                string htmlCss = @"<style>
                                  .textoCuerpo {
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    color: #1f497d
                                  }
                                  .cabecera {
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  table th {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #b0c4de;
                                    font-family: ""Calibri"", sans-serif
                                  }
                                  table,
                                  td {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #eed;
                                    font-family: ""Calibri"", sans-serif;
                                    vertical-align: top;
                                    text-align: center;
                                  }
                                  td,
                                  th {
                                    padding: 10px
                                  }
                                  .resaltar {
                                    background: #ff0 !important;
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  @font-face {
                                    font-family: ""Cambria Math"";
                                    panose-1: 2 4 5 3 5 4 6 3 2 4
                                  }
                                  @font-face {
                                    font-family: Calibri;
                                    panose-1: 2 15 5 2 2 2 4 3 2 4
                                  }
                                  @font-face {
                                    font-family: Verdana;
                                    panose-1: 2 11 6 4 3 5 4 4 2 4
                                  }
                                  @font-face {
                                    font-family: ""Palatino Linotype"";
                                    panose-1: 2 4 5 2 5 5 5 3 3 4
                                  }
                                  div.MsoNormal,
                                  li.MsoNormal,
                                  p.MsoNormal {
                                    margin: 0;
                                    margin-bottom: .0001pt;
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  a:link,
                                  span.MsoHyperlink {
                                    mso-style-priority: 99;
                                    color: #0563c1;
                                    text-decoration: underline
                                  }
                                  a:visited,
                                  span.MsoHyperlinkFollowed {
                                    mso-style-priority: 99;
                                    color: #954f72;
                                    text-decoration: underline
                                  }
                                  span.EstiloCorreo17 {
                                    mso-style-type: personal-compose;
                                    font-family: ""Calibri"", sans-serif;
                                    color: windowtext
                                  }
                                  .MsoChpDefault {
                                    mso-style-type: export-only;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  @page WordSection1 {
                                    size: 612pt 792pt;
                                    margin: 70.85pt 3cm 70.85pt 3cm
                                  }
                                  div.WordSection1 {
                                    page: WordSection1
                                  }
                                </style>";

                string htmlFinal = htmlCss + htmlMail;
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
        public ActionResult TareaReporteSeguimientoColaborador()
        {
            string asuntoEmail = "Reporte de Seguimiento Diario por Colaborador";
            try
            {
                DateTimeFormatInfo dateTimeFormatInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                DateTime fecha = DateTime.Now.AddHours(5).AddDays(-1);

                var detalleSeguimientos = (from t in db.Tarea
                                           join et in db.EstadoTarea on t.estadoTarea equals et
                                           join c in db.Colaborador on t.colaborador equals c
                                           join p in db.Persona on c.persona equals p
                                           where (t.FechaInicio.Year == fecha.Year && t.FechaInicio.Month == fecha.Month && t.FechaInicio.Day == fecha.Day && et.Codigo != "ANULADA")
                                           select new
                                           {
                                               tarea = t.Detalle,
                                               cliente = t.cliente.Descripcion,
                                               colaborador = p.Nombre1 + " " + p.Apellido1,
                                               asignado = (t.FechaInicio.Hour < 13 && t.FechaFin.Hour > 13) ? DbFunctions.DiffHours(t.FechaInicio, t.FechaFin) - 1 :
                                                           DbFunctions.DiffHours(t.FechaInicio, t.FechaFin),
                                               reportado = t.HorasUtilizadas,
                                               estado = et.Codigo,
                                               email = db.Usuario.Where(w => w.SecuencialPersona == p.Secuencial).FirstOrDefault() != null
                                                                    ?
                                                                    db.Usuario.Where(w => w.SecuencialPersona == p.Secuencial).FirstOrDefault().Email
                                                                    : ""
                                           }).OrderBy(o => o.colaborador).ToList();

                var seguimientos = (from a in detalleSeguimientos
                                    group new { a.tarea, a.asignado, a.reportado, a.estado } by new { a.colaborador } into gro
                                    select new
                                    {
                                        gro.Key.colaborador,
                                        completadas = gro.Where(w => w.estado == "TERMINADA").Count(),
                                        inconclusas = gro.Where(w => w.estado == "INCONCLUSA").Count(),
                                        asignada = gro.Where(w => w.estado == "ASIGNADA").Count(),
                                        pausa = gro.Where(w => w.estado == "PAUSA").Count(),
                                        desarrollo = gro.Where(w => w.estado == "DESARROLLO").Count(),
                                        asignado = gro.Sum(s => s.asignado),
                                        reportado = gro.Sum(s => s.reportado)
                                    }).ToList();

                string htmlCss = @"<style>
                                  .textoCuerpo {
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    color: #1f497d
                                  }
                                  .cabecera {
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  table th {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #b0c4de;
                                    font-family: ""Calibri"", sans-serif
                                  }
                                  table,
                                  td {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #eed;
                                    font-family: ""Calibri"", sans-serif;
                                    vertical-align: top;
                                    text-align: center;
                                  }
                                  td,
                                  th {
                                    padding: 10px
                                  }
                                  .resaltar {
                                    background: #ff0 !important;
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  @font-face {
                                    font-family: ""Cambria Math"";
                                    panose-1: 2 4 5 3 5 4 6 3 2 4
                                  }
                                  @font-face {
                                    font-family: Calibri;
                                    panose-1: 2 15 5 2 2 2 4 3 2 4
                                  }
                                  @font-face {
                                    font-family: Verdana;
                                    panose-1: 2 11 6 4 3 5 4 4 2 4
                                  }
                                  @font-face {
                                    font-family: ""Palatino Linotype"";
                                    panose-1: 2 4 5 2 5 5 5 3 3 4
                                  }
                                  div.MsoNormal,
                                  li.MsoNormal,
                                  p.MsoNormal {
                                    margin: 0;
                                    margin-bottom: .0001pt;
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  a:link,
                                  span.MsoHyperlink {
                                    mso-style-priority: 99;
                                    color: #0563c1;
                                    text-decoration: underline
                                  }
                                  a:visited,
                                  span.MsoHyperlinkFollowed {
                                    mso-style-priority: 99;
                                    color: #954f72;
                                    text-decoration: underline
                                  }
                                  span.EstiloCorreo17 {
                                    mso-style-type: personal-compose;
                                    font-family: ""Calibri"", sans-serif;
                                    color: windowtext
                                  }
                                  .MsoChpDefault {
                                    mso-style-type: export-only;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  @page WordSection1 {
                                    size: 612pt 792pt;
                                    margin: 70.85pt 3cm 70.85pt 3cm
                                  }
                                  div.WordSection1 {
                                    page: WordSection1
                                  }
                                </style>";

                foreach (var item in seguimientos)
                {
                    var detalle = detalleSeguimientos.Where(s => s.colaborador == item.colaborador).ToList();
                    List<string> usuariosDestinos = new List<string>();
                    //usuariosDestinos.Add("rsanchez@sifizsoft.com");
                    usuariosDestinos.Add(detalle.FirstOrDefault().email);
                    string comentarioEmail = "Seguimiento de tareas de " + item.colaborador + " del " + fecha.ToString("dd/MM/yyyy") + ".";

                    string htmlMail = @"<div class='textoCuerpo'><h4>";
                    htmlMail += comentarioEmail +
                           "</h4><table><thead>";
                    htmlMail += "<tr>" +
                                    "<th> TIEMPO ASIGNADO </th>" +
                                    "<th> TIEMPO REPORTADO </th>" +
                                    "<th> TAREAS COMPLETADAS </th>" +
                                    "<th> TAREAS INCONCLUSAS </th>" +
                                    "<th> TAREAS NO REALIZADAS </th>" +
                                    "<th> TAREAS EN PAUSA </th>" +
                                    "<th> TAREAS EN DESARROLLO </th>" +
                                "</tr>" +
                           "</thead>";
                    htmlMail += "<tbody>";
                    htmlMail += "<tr>" +
                                    "<td>" + item.asignado + "</td>" +
                                    "<td>" + item.reportado + "</td>" +
                                    "<td>" + item.completadas + "</td>" +
                                    "<td>" + item.inconclusas + "</td>" +
                                    "<td>" + item.asignada + "</td>" +
                                    "<td>" + item.pausa + "</td>" +
                                    "<td>" + item.desarrollo + "</td>" +
                                "</tr>";
                    htmlMail += "</tbody></table><br>";

                    htmlMail +=
                           "<table><thead>";
                    htmlMail += "<tr>" +
                                    "<th> TAREA </th>" +
                                    "<th> ESTADO </th>" +
                                    "<th> CLIENTE </th>" +
                                    "<th> TIEMPO ASIGNADO </th>" +
                                    "<th> TIEMPO REPORTADO </th>" +
                                    "<th> DIFERENCIA </th>" +
                                "</tr>" +
                           "</thead>";
                    htmlMail += "<tbody>";
                    foreach (var i in detalle)
                    {
                        htmlMail += "<tr>" +
                                        "<td style='text-align: left;'>" + i.tarea + "</td>" +
                                        "<td>" + i.estado + "</td>" +
                                        "<td>" + i.cliente + "</td>" +
                                        "<td>" + i.asignado + "</td>" +
                                        "<td>" + i.reportado + "</td>" +
                                        "<td>" + (i.reportado - i.asignado) + "</td>" +
                                    "</tr>";
                    }
                    htmlMail += "<tr>" +
                                    "<td colspan='3' rowspan='1' style='text-align: right;'> TOTAL </td>" +
                                    "<td>" + detalle.Sum(s => s.asignado) + "</td>" +
                                    "<td>" + detalle.Sum(s => s.reportado) + "</td>" +
                                    "<td>" + (detalle.Sum(s => s.reportado) - detalle.Sum(s => s.asignado)) + "</td>" +
                                "</tr>";
                    htmlMail += "</tbody></table><br></div>";

                    string htmlFinal = htmlCss + htmlMail;
                    Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), htmlFinal, asuntoEmail);
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
        public ActionResult TareaReporteEstadoTicketsColaborador()
        {
            string asuntoEmail = "Resumen Estado de Tickets";
            try
            {
                DateTimeFormatInfo dateTimeFormatInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                DateTime fecha = DateTime.Now.AddHours(5).AddDays(-1);

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
                                          email = "",
                                          clase = (t.estadoTicket.Codigo == "CERRADO") ? "fondoCerrado" :
                                                  (t.estadoTicket.Codigo == "ANULADO") ? "fondoAnulado" :
                                                  (t.estadoTicket.Codigo == "RECHAZADO") ? "fondoAnulado" :
                                                  (t.estadoTicket.Codigo == "ESPERANDO LLAMADA" || t.estadoTicket.Codigo == "ESPERANDO RESPUESTA") ? "fondoCliente" :
                                                  (t.estadoTicket.Codigo == "ABIERTO") ? "fondoAbierto" : "fondoDesarrollo"
                                      }).ToList();

                var tickets = ticketsParcial;

                var asignados = (from t in db.Ticket
                                 join ttar in db.TicketTarea on t.Secuencial equals ttar.SecuencialTicket
                                 join tar in db.Tarea on ttar.SecuencialTarea equals tar.Secuencial
                                 join c in db.Colaborador on tar.SecuencialColaborador equals c.Secuencial
                                 join p in db.Persona on c.SecuencialPersona equals p.Secuencial
                                 orderby tar.FechaInicio descending
                                 select new
                                 {
                                     nombre = p.Nombre1 + " " + p.Apellido1,
                                     numero = t.Secuencial,
                                     email = db.Usuario.Where(w => w.SecuencialPersona == p.Secuencial).FirstOrDefault() != null
                                                                    ?
                                                                    db.Usuario.Where(w => w.SecuencialPersona == p.Secuencial).FirstOrDefault().Email
                                                                    : ""
                                 }).ToList();

                ticketsParcial = ticketsParcial.Where(x => x.estado != "CERRADO" && x.estado != "ANULADO" && x.estado != "RECHAZADO").ToList();

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
                               db.TicketTarea.Where(x => x.SecuencialTicket == t.numero && x.EstaActiva == 1).Count() > 0
                          ) ?
                              asignados.Where(x => x.numero.ToString().Equals(t.numero.ToString())).FirstOrDefault().nombre.ToString()
                            : "NO ASIGNADO",
                               prioridad = t.prioridad,
                               categoria = t.categoria,
                               estado = t.estado,
                               proximaActividad = t.proximaActividad,
                               email = (
                               db.TicketTarea.Where(x => x.SecuencialTicket == t.numero && x.EstaActiva == 1).Count() > 0
                          ) ?
                              asignados.Where(x => x.numero.ToString().Equals(t.numero.ToString())).FirstOrDefault().email.ToString()
                            : "",
                               clase = t.clase
                           }).ToList();
                tickets = tickets.Where(w => w.asignado != "NO ASIGNADO").OrderBy(o => o.asignado).ToList();

                var resumen = tickets.GroupBy(g => new { g.asignado, g.estado, g.email }).Select(s => new
                {
                    colaborador = s.Key.asignado,
                    estado = s.Key.estado,
                    email = s.Key.email,
                    cantidad = s.Count()
                }).ToList();

                var estadosTicket = db.EstadoTicket.Where(s => s.EstaActivo == 1 && s.Codigo != "CERRADO" && s.Codigo != "ANULADO" && s.Codigo != "RECHAZADO").Select(s => new { s.Codigo }).ToList();
                var colaboradores = tickets.GroupBy(g => g.asignado).Select(s => new { colaborador = s.Key }).ToList();

                string htmlCss = @"<style>
                                  .textoCuerpo {
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    color: #1f497d
                                  }
                                  .cabecera {
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  table th {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #b0c4de;
                                    font-family: ""Calibri"", sans-serif
                                  }
                                  table,
                                  td {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #eed;
                                    font-family: ""Calibri"", sans-serif;
                                    vertical-align: top;
                                    text-align: center;
                                  }
                                  td,
                                  th {
                                    padding: 10px
                                  }
                                  .resaltar {
                                    background: #ff0 !important;
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  @font-face {
                                    font-family: ""Cambria Math"";
                                    panose-1: 2 4 5 3 5 4 6 3 2 4
                                  }
                                  @font-face {
                                    font-family: Calibri;
                                    panose-1: 2 15 5 2 2 2 4 3 2 4
                                  }
                                  @font-face {
                                    font-family: Verdana;
                                    panose-1: 2 11 6 4 3 5 4 4 2 4
                                  }
                                  @font-face {
                                    font-family: ""Palatino Linotype"";
                                    panose-1: 2 4 5 2 5 5 5 3 3 4
                                  }
                                  div.MsoNormal,
                                  li.MsoNormal,
                                  p.MsoNormal {
                                    margin: 0;
                                    margin-bottom: .0001pt;
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  a:link,
                                  span.MsoHyperlink {
                                    mso-style-priority: 99;
                                    color: #0563c1;
                                    text-decoration: underline
                                  }
                                  a:visited,
                                  span.MsoHyperlinkFollowed {
                                    mso-style-priority: 99;
                                    color: #954f72;
                                    text-decoration: underline
                                  }
                                  span.EstiloCorreo17 {
                                    mso-style-type: personal-compose;
                                    font-family: ""Calibri"", sans-serif;
                                    color: windowtext
                                  }
                                  .MsoChpDefault {
                                    mso-style-type: export-only;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  @page WordSection1 {
                                    size: 612pt 792pt;
                                    margin: 70.85pt 3cm 70.85pt 3cm
                                  }
                                  div.WordSection1 {
                                    page: WordSection1
                                  }
                                </style>";

                foreach (var item in colaboradores)
                {
                    var resumenTicket = resumen.Where(s => s.colaborador == item.colaborador).ToList();
                    List<string> usuariosDestinos = new List<string>();
                    usuariosDestinos.Add(resumenTicket.FirstOrDefault().email);
                    //usuariosDestinos.Add("rsanchez@sifizsoft.com");

                    var detalleTicket = tickets.Where(s => s.asignado == item.colaborador).ToList();
                    string comentarioEmail = "Estado de los tickets de " + item.colaborador + " del " + fecha.ToString("dd/MM/yyyy") + ".";

                    string htmlMail = @"<div class='textoCuerpo'><h4>";
                    htmlMail += comentarioEmail +
                           "</h4><table><thead>";
                    htmlMail += "<tr>";
                    foreach (var estado in estadosTicket)
                    {
                        htmlMail += "<th>" + estado.Codigo + "</th>";
                    }
                    htmlMail += "</tr>" +
                        "</thead>";
                    htmlMail += "<tbody>";
                    htmlMail += "<tr>";

                    foreach (var estado in estadosTicket)
                    {
                        var est = resumenTicket.Where(s => s.estado == estado.Codigo).FirstOrDefault();
                        if (est != null)
                        {
                            htmlMail += "<td>" + est.cantidad + "</td>";
                        }
                        else
                        {
                            htmlMail += "<td>" + 0 + "</td>";
                        }
                    }

                    htmlMail += "</tr>";
                    htmlMail += "</tbody></table><br>";

                    htmlMail +=
                           "<table><thead>";
                    htmlMail += "<tr>" +
                                    "<th> NUMERO </th>" +
                                    "<th> CLIENTE </th>" +
                                    "<th> FECHA </th>" +
                                    "<th> ASUNTO </th>" +
                                    "<th> ESTADO </th>" +
                                    "<th> PRIORIDAD </th>" +
                                    "<th> CATEGORIA </th>" +
                                // "<th> PROXIMA ACTIVIDAD </th>" +
                                "</tr>" +
                           "</thead>";
                    htmlMail += "<tbody>";

                    foreach (var i in detalleTicket)
                    {
                        htmlMail += "<tr>" +
                                        "<td>" + i.numero + "</td>" +
                                        "<td>" + i.cliente + "</td>" +
                                        "<td>" + i.fecha.ToString("dd/MM/yyyy") + "</td>" +
                                        "<td style='text-align: left;'>" + i.asunto + "</td>" +
                                        "<td>" + i.estado + "</td>" +
                                        "<td>" + i.prioridad + "</td>" +
                                        "<td>" + i.categoria + "</td>" +
                                    //"<td>" + i.proximaActividad + "</td>" +
                                    "</tr>";
                    }
                    htmlMail += "</tbody></table><br></div>";

                    string htmlFinal = htmlCss + htmlMail;
                    if (usuariosDestinos.Count > 0)
                    {
                        Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), htmlFinal, asuntoEmail);
                    }
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
        public ActionResult TareaReporteEstadoContratosColaborador()
        {
            string asuntoEmail = "Resumen Estado de los Contratos";
            try
            {
                DateTimeFormatInfo dateTimeFormatInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                DateTime fecha = DateTime.Now.AddHours(5).AddDays(-1);

                var estadoContratosColaborador = (from mt in db.MotivoTrabajo
                                                  join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
                                                  join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
                                                  where mt.EstaActivo == 1 && mt.Avance != 100
                                                  select new
                                                  {
                                                      cliente = cl.Descripcion,
                                                      descripcion = mt.Descripcion,
                                                      codigo = mt.Codigo,
                                                      fechaVencimiento = mt.FechaFin,
                                                      estado = mt.estadoContrato != null ?
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
                                                      diasRestantes = DbFunctions.DiffDays(DateTime.Now, mt.FechaFin),
                                                      responsable = mt.colaborador != null
                                                                    ?
                                                                    mt.colaborador.persona.Nombre1 + " " + mt.colaborador.persona.Apellido1
                                                                    : "NO ASIGNADO",
                                                      tipoMotivoTrabajo = tt.Codigo,
                                                      email = mt.colaborador != null
                                                                    ?
                                                                    db.Usuario.Where(w => w.SecuencialPersona == mt.colaborador.SecuencialPersona).FirstOrDefault().Email
                                                                    : ""
                                                  }).OrderBy(o => o.responsable).ToList();
                var responsableContratos = estadoContratosColaborador.Where(w => w.responsable != "NO ASIGNADO").ToList();

                var resumen = responsableContratos.GroupBy(g => new { g.responsable, g.email, g.estado }).Select(s => new
                {
                    email = s.Key.email,
                    responsable = s.Key.responsable,
                    estado = s.Key.estado,
                    cantidad = s.Count()
                }).ToList();

                var estadosContrato = db.EstadoContrato.Where(s => s.EstaActivo == 1 && s.Codigo != "AUTOMATICO").Select(s => new { s.Codigo }).ToList();

                var responsables = responsableContratos.GroupBy(g => g.responsable).Select(s => new { colaborador = s.Key }).ToList();

                string htmlCss = @"<style>
                                  .textoCuerpo {
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    color: #1f497d
                                  }
                                  .cabecera {
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  table th {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #b0c4de;
                                    font-family: ""Calibri"", sans-serif
                                  }
                                  table,
                                  td {
                                    border: 1px solid #000;
                                    border-collapse: collapse;
                                    font-size: 8pt;
                                    background: #eed;
                                    font-family: ""Calibri"", sans-serif;
                                    vertical-align: top;
                                    text-align: center;
                                  }
                                  td,
                                  th {
                                    padding: 10px
                                  }
                                  .resaltar {
                                    background: #ff0 !important;
                                    font-size: 8pt;
                                    font-family: ""Calibri"", sans-serif;
                                    border-bottom: 1px solid #222
                                  }
                                  @font-face {
                                    font-family: ""Cambria Math"";
                                    panose-1: 2 4 5 3 5 4 6 3 2 4
                                  }
                                  @font-face {
                                    font-family: Calibri;
                                    panose-1: 2 15 5 2 2 2 4 3 2 4
                                  }
                                  @font-face {
                                    font-family: Verdana;
                                    panose-1: 2 11 6 4 3 5 4 4 2 4
                                  }
                                  @font-face {
                                    font-family: ""Palatino Linotype"";
                                    panose-1: 2 4 5 2 5 5 5 3 3 4
                                  }
                                  div.MsoNormal,
                                  li.MsoNormal,
                                  p.MsoNormal {
                                    margin: 0;
                                    margin-bottom: .0001pt;
                                    font-size: 11pt;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  a:link,
                                  span.MsoHyperlink {
                                    mso-style-priority: 99;
                                    color: #0563c1;
                                    text-decoration: underline
                                  }
                                  a:visited,
                                  span.MsoHyperlinkFollowed {
                                    mso-style-priority: 99;
                                    color: #954f72;
                                    text-decoration: underline
                                  }
                                  span.EstiloCorreo17 {
                                    mso-style-type: personal-compose;
                                    font-family: ""Calibri"", sans-serif;
                                    color: windowtext
                                  }
                                  .MsoChpDefault {
                                    mso-style-type: export-only;
                                    font-family: ""Calibri"", sans-serif;
                                    mso-fareast-language: EN-US
                                  }
                                  @page WordSection1 {
                                    size: 612pt 792pt;
                                    margin: 70.85pt 3cm 70.85pt 3cm
                                  }
                                  div.WordSection1 {
                                    page: WordSection1
                                  }
                                </style>";

                foreach (var item in responsables)
                {
                    List<string> usuariosDestinos = new List<string>();
                    var resumenContrato = resumen.Where(s => s.responsable == item.colaborador).ToList();
                    usuariosDestinos.Add(resumenContrato[0].email);
                    //usuariosDestinos.Add("rsanchez@sifizsoft.com");

                    var detalleContrato = estadoContratosColaborador.Where(s => s.responsable == item.colaborador).ToList();
                    string comentarioEmail = "Estado de los contratos de " + item.colaborador + " del " + fecha.ToString("dd/MM/yyyy") + ".";

                    string htmlMail = @"<div class='textoCuerpo'><h4>";
                    htmlMail += comentarioEmail +
                           "</h4><table><thead>";
                    htmlMail += "<tr>" +
                                    "<th>VIGENTE</th>" +
                                    "<th>VENCIDO</th>" +
                                    "<th>POR VENCER</th>";
                    foreach (var estado in estadosContrato)
                    {
                        htmlMail += "<th>" + estado.Codigo + "</th>";
                    }
                    htmlMail += "</tr>" +
                        "</thead>";
                    htmlMail += "<tbody>";
                    htmlMail += "<tr>";
                    var vigente = resumenContrato.Where(s => s.estado == "VIGENTE").FirstOrDefault();
                    if (vigente != null)
                    {
                        htmlMail += "<td>" + vigente.cantidad + "</td>";
                    }
                    else
                    {
                        htmlMail += "<td>" + 0 + "</td>";
                    }
                    var vencido = resumenContrato.Where(s => s.estado == "VENCIDO").FirstOrDefault();
                    if (vencido != null)
                    {
                        htmlMail += "<td>" + vencido.cantidad + "</td>";
                    }
                    else
                    {
                        htmlMail += "<td>" + 0 + "</td>";
                    }
                    var porVencer = resumenContrato.Where(s => s.estado == "POR VENCER").FirstOrDefault();
                    if (porVencer != null)
                    {
                        htmlMail += "<td>" + porVencer.cantidad + "</td>";
                    }
                    else
                    {
                        htmlMail += "<td>" + 0 + "</td>";
                    }
                    foreach (var estado in estadosContrato)
                    {
                        var est = resumenContrato.Where(s => s.estado == estado.Codigo).FirstOrDefault();
                        if (est != null)
                        {
                            htmlMail += "<td>" + est.cantidad + "</td>";
                        }
                        else
                        {
                            htmlMail += "<td>" + 0 + "</td>";
                        }
                    }

                    htmlMail += "</tr>";
                    htmlMail += "</tbody></table><br>";

                    htmlMail +=
                           "<table><thead>";
                    htmlMail += "<tr>" +
                                    "<th> CLIENTE </th>" +
                                    "<th> CONTRATO </th>" +
                                    "<th> ESTADO </th>" +
                                    "<th> FECHA VENCIMIENTO </th>" +
                                    "<th> DIAS RESTANTES </th>" +
                                "</tr>" +
                           "</thead>";
                    htmlMail += "<tbody>";

                    foreach (var i in detalleContrato)
                    {
                        var entregables = (from e in db.MotivoTrabajo.Where(w => w.Codigo == i.codigo).FirstOrDefault().entregableMotivoTrabajo
                                           where e.EstaActivo == 1
                                           orderby e.Secuencial ascending
                                           select new
                                           {
                                               Id = e.Secuencial,
                                               e.Nombre,
                                               e.Avance,
                                               Colaborador = e.colaborador != null ? e.colaborador.persona.Nombre1.Substring(0, 1) + "." + e.colaborador.persona.Apellido1 : "No_Asignado",
                                               ColaboradorId = e.colaborador != null ? e.colaborador.Secuencial : 0
                                           }).ToList();
                        htmlMail += "<tr>" +
                                        "<td style='text-align: left;'>" + i.cliente + "</td>" +
                                        "<td style='text-align: left;'>" + i.descripcion;
                        if (entregables.Count > 0)
                        {
                            htmlMail += "<br><table><thead>" +
                                "<tr>" +
                                "<td><b>" + "Entregable" + "</b></td>" +
                                                "<td><b>" + "Avance" + "</b></td>" +
                                                "<td><b>" + "Técnico Asignado" + "</b></td>" +
                                            "</tr>" +
                            "</thead>";
                            htmlMail += "<tbody>";
                            foreach (var e in entregables)
                            {
                                htmlMail += "<tr>" +
                                                "<td>" + e.Nombre + "</td>" +
                                                "<td>" + e.Avance + " %</td>" +
                                                "<td>" + e.Colaborador + "</td>" +
                                            "</tr>";
                            }
                            htmlMail += "</tbody></table>";
                        }

                        htmlMail += "</td>" +
                                        "<td>" + i.estado + "</td>" +
                                        "<td>" + i.fechaVencimiento.ToString("dd/MM/yyyy") + "</td>" +
                                        "<td>" + i.diasRestantes + "</td>" +
                                    "</tr>";
                    }
                    htmlMail += "</tbody></table><br></div>";

                    string htmlFinal = htmlCss + htmlMail;
                    Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), htmlFinal, asuntoEmail);
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
        public void EnviarEmailReporteTicketsMensual()
        {
            //string destinatariosEmail;
            string asuntoEmail = "Prueba tarea automatica";
            string comentarioEmail = "Prueba Comentario";
            //string tickets;
            try
            {
                //var s = new JavaScriptSerializer();
                //var jsonObj = s.Deserialize<dynamic>(tickets);

                //var destinatarios = destinatariosEmail.Replace(',', ';');
                List<string> usuariosDestinos = new List<string>();
                //Regex rgx = new Regex(@"^(([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)(\s*;\s*|\s*$))*$");
                //if (!rgx.IsMatch(destinatarios))
                //{
                //    throw new Exception("Debe ingresar una lista de correos válida separados por coma ó punto y coma");
                //};
                //string[] emails = destinatarios.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                //foreach (var email in emails)
                //{
                //usuariosDestinos.Add("rsanchez@sifizsoft.com");
                //}
                string htmlMail = @"<div class='textoCuerpo'><br/>";
                htmlMail += comentarioEmail +
                    "<br/><table> <thead>";
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

                for (int i = 0; i < 10; i++)
                {
                    htmlMail += "<tr>" +
                                    "<td>" + "numero" + "</td>" +
                                    "<td>" + "detalle" + "</td>" +
                                    "<td>" + "reportado" + "</td>" +
                                    "<td>" + "asignado" + "</td>" +
                                    "<td>" + "fecha" + "</td>" +
                                    "<td>" + "estado" + "</td>" +
                                    "<td>" + "tiempo" + "</td>" +
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
                Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), htmlFinal, asuntoEmail);

                var resp = new
                {
                    success = true
                };
                //return Json(resp);
            }
            catch (Exception e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
                // return Json(resp);
            }
        }

        public void SetDataInfoTickets()
        {
            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    db.InfoTickets.RemoveRange(db.InfoTickets);

                    var tickets = db.Ticket
                        .AsNoTracking()
                        .Include(t => t.persona_cliente.cliente)
                        .Include(t => t.persona_cliente.persona)
                        .Include(t => t.prioridadTicket)
                        .Include(t => t.categoriaTicket)
                        .Include(t => t.ticketHistorico)
                        .Include(t => t.estadoTicket)
                        .Include(t => t.ticketVersionClliente)
                        .ToList();

                    var ticketTareas = db.TicketTarea
                        .AsNoTracking()
                        .Where(tt => tt.EstaActiva == 1)
                        .Include(tt => tt.tarea)
                        .ToList();

                    var personas = db.Persona
                        .AsNoTracking()
                        .ToDictionary(p => p.Secuencial);

                    var colaboradores = db.Colaborador
                        .AsNoTracking()
                        .ToDictionary(c => c.SecuencialPersona);

                    var infoTickets = new List<InfoTickets>();

                    Parallel.ForEach(tickets, item =>
                    {
                        var infoTicket = new InfoTickets
                        {
                            Id = item.Secuencial,
                            Cliente = item.persona_cliente.cliente.Descripcion,
                            Prioridad = item.prioridadTicket.Codigo,
                            Tipo = item.categoriaTicket.Codigo,
                            Usuario = item.persona_cliente.persona.Nombre1 + " " + item.persona_cliente.persona.Apellido1,
                            ProbadoPor = "",
                            FechaIngreso = item.FechaCreado,
                            FechaRespuesta = item.FechaCreado,
                            FechaAsignacion = item.ticketHistorico
                                .Where(s => s.estadoTicket != null && s.estadoTicket.Codigo == "ASIGNADO")
                                .GroupBy(s => s.Version)
                                .Select(g => g.First())
                                .OrderBy(s => s.Version)
                                .FirstOrDefault()?.FechaOperacion,
                            FechaEntrega = GetFechaEntrega(item.ticketHistorico),
                            FechaCierre = item.ticketHistorico
                                .Where(s => s.estadoTicket != null && s.estadoTicket.Codigo == "CERRADO")
                                .GroupBy(s => s.Version)
                                .Select(g => g.First())
                                .OrderBy(s => s.Version)
                                .FirstOrDefault()?.FechaOperacion,
                            NumeroReprocesos = int.Parse((item.ticketHistorico
                                .GroupBy(s => s.Version)
                                .Select(g => g.First())
                                .OrderByDescending(s => s.Version)
                                .FirstOrDefault()?.Reprocesos ?? 0).ToString()),
                            EstimadoPor = "",
                            AsignadoA = GetAsignadoA(item.Secuencial, ticketTareas, personas, colaboradores),
                            EntregadoPor = "",
                            Estado = item.estadoTicket?.Codigo ?? "DESCONOCIDO",
                            AplicaA = item.ticketVersionClliente?.Descripcion
                        };

                        var tiempos = CalcularTiempos(item.Secuencial, ticketTareas);
                        infoTicket.HorasAsignadas = DateTime.MinValue + tiempos.Item1;
                        infoTicket.HorasEmpleadas = DateTime.MinValue + tiempos.Item2;
                        infoTicket.HorasEstimadas = DateTime.MinValue + new TimeSpan(item.Estimacion, 0, 0);
                        infoTicket.HorasEntrega = DateTime.MinValue;
                        infoTicket.HorasPrueba = DateTime.MinValue;

                        lock (infoTickets)
                        {
                            infoTickets.Add(infoTicket);
                        }
                    });

                    db.InfoTickets.AddRange(infoTickets);
                    db.SaveChanges();
                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private DateTime? GetFechaEntrega(IEnumerable<TicketHistorico> ticketHistorico)
        {
            var orderedHistory = ticketHistorico
                .GroupBy(h => h.Version)
                .Select(g => g.First())
                .OrderByDescending(h => h.Version)
                .ToList();

            for (int i = 1; i < orderedHistory.Count; i++)
            {
                if (orderedHistory[i - 1].estadoTicket != null &&
                    orderedHistory[i - 1].estadoTicket.Codigo == "RESUELTO" &&
                    (orderedHistory[i].estadoTicket == null ||
                     orderedHistory[i].estadoTicket.Codigo != "RESUELTO"))
                {
                    return orderedHistory[i - 1].FechaOperacion;
                }
            }
            return null;
        }

        private string GetAsignadoA(int secuencialTicket, List<TicketTarea> ticketTareas, Dictionary<int, Persona> personas, Dictionary<int, Colaborador> colaboradores)
        {
            var ticketTarea = ticketTareas.FirstOrDefault(tt => tt.SecuencialTicket == secuencialTicket);
            if (ticketTarea == null) return "NO ASIGNADO";

            if (!colaboradores.TryGetValue(ticketTarea.tarea.SecuencialColaborador, out var colaborador))
            {
                return "COLABORADOR NO ENCONTRADO";
            }

            if (!personas.TryGetValue(colaborador.SecuencialPersona, out var persona))
            {
                return "PERSONA NO ENCONTRADA";
            }

            return persona.Nombre1 + " " + persona.Apellido1;
        }

        private Tuple<TimeSpan, TimeSpan> CalcularTiempos(int secuencialTicket, List<TicketTarea> ticketTareas)
        {
            var tareas = ticketTareas.Where(tt => tt.SecuencialTicket == secuencialTicket).ToList();
            var totalAsignado = TimeSpan.Zero;
            var totalUtilizado = TimeSpan.Zero;

            foreach (var ta in tareas)
            {
                var tiempoAsignado = ta.tarea.FechaFin - ta.tarea.FechaInicio;
                var tiempoUtilizado = TimeSpan.FromMinutes(Math.Round(60 * (double)ta.tarea.HorasUtilizadas));

                if (ta.tarea.FechaInicio.Hour < 13 && ta.tarea.FechaFin.Hour > 13)
                {
                    tiempoAsignado -= TimeSpan.FromHours(1);
                }

                totalAsignado += tiempoAsignado;
                totalUtilizado += tiempoUtilizado;
            }

            return System.Tuple.Create(totalAsignado, totalUtilizado);
        }

        public void CalcularSemaforoTciket()
        {
            try
            {
                var tickets = db.Ticket.ToList();
                tickets.ForEach(tick => tick.HorasCreado = CalculoHorasLaborables(tick.FechaCreado));
                db.SaveChanges();

                var resp = new
                {
                    success = true
                };
            }
            catch (Exception e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
            }
        }

        public void CalcularResolucionSemaforoTciket()
        {
            try
            {
                var tickets = db.Ticket.ToList();
                tickets.ForEach(tick => tick.HorasResolucion = CalculoHorasLaborables(tick.FechaCreado));
                db.SaveChanges();

                var resp = new
                {
                    success = true
                };
            }
            catch (Exception e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
            }
        }

        private int CalculoHorasLaborables(DateTime fecha)
        {
            var fechaFinal = DateTime.Now;
            var fechaInicial = fecha;

            var fechaActual = fechaFinal.Date.AddHours(fechaFinal.Hour);
            var fechaTicket = fechaInicial.Date.AddHours(fechaInicial.Hour);

            int horasLaborables = 0;
            fechaTicket = fechaTicket.AddHours(1);

            while (fechaTicket <= fechaActual)
            {
                var horaInicio = fechaTicket.Date.AddHours(8).AddMinutes(30);
                var horaFin = fechaTicket.Date.AddHours(17).AddMinutes(30);
                var horaAlmuerzoInicio = fechaTicket.Date.AddHours(13);
                var horaAlmuerzoFin = fechaTicket.Date.AddHours(14);

                if (fechaTicket.DayOfWeek != DayOfWeek.Saturday && fechaTicket.DayOfWeek != DayOfWeek.Sunday)
                {
                    if ((fechaTicket > horaInicio && fechaTicket <= horaAlmuerzoInicio) || (fechaTicket > horaAlmuerzoFin && fechaTicket <= horaFin))
                    {
                        horasLaborables++;
                    }
                }

                fechaTicket = fechaTicket.AddHours(1);
            }

            if (fechaFinal.Minute >= fechaInicial.Minute && (fechaFinal - fechaInicial).TotalHours >= 1)
            {
                var horaFinal = fechaFinal.TimeOfDay;
                var horaInicial = fechaInicial.TimeOfDay;

                if (IsInWorkingHours(fechaFinal, horaFinal) && IsInWorkingHours(fechaInicial, horaInicial))
                    horasLaborables++;
            }

            return horasLaborables;
        }

        private bool IsInWorkingHours(DateTime fecha, TimeSpan hora)
        {
            return fecha.DayOfWeek != DayOfWeek.Saturday &&
                   fecha.DayOfWeek != DayOfWeek.Sunday &&
                   ((hora >= new TimeSpan(8, 30, 0) && hora < new TimeSpan(13, 0, 0)) ||
                    (hora >= new TimeSpan(14, 0, 0) && hora < new TimeSpan(17, 30, 0)));
        }

        public void CerrarTicketsResueltos()
        {
            try
            {
                TicketController ticketController = new TicketController();
                var tiemposCerradoTicket = (from t in db.TiempoCerradoTicket
                                            select new
                                            {
                                                diasCerrado = t.DiasCerrado,
                                                cliente = t.SecuencialCliente
                                            }).ToList();

                foreach (var tiempo in tiemposCerradoTicket)
                {
                    Persona_Cliente personaCliente = (from p in db.Persona_Cliente
                                                      where p.SecuencialCliente == tiempo.cliente
                                                      select p).FirstOrDefault();

                    if (personaCliente != null)
                    {
                        var ticketsCliente = (from t in db.Ticket
                                              where t.persona_cliente.SecuencialCliente == personaCliente.SecuencialCliente && t.estadoTicket.Codigo == "RESUELTO"
                                              select new
                                              {
                                                  idTicket = t.Secuencial,
                                                  ticketHistorico = (from th in db.TicketHistorico
                                                                     where th.SecuencialTicket == t.Secuencial && th.estadoTicket.Codigo == "RESUELTO"
                                                                     orderby th.FechaOperacion descending
                                                                     select th).FirstOrDefault()
                                              }).ToList();

                        var cliente = db.Cliente.FirstOrDefault(s => s.Secuencial == personaCliente.SecuencialCliente && s.EstaActivo == 1);
                        //string emailUser = cliente.persona_cliente.FirstOrDefault().persona.usuario.FirstOrDefault().Email;
                        string emailUser = System.Configuration.ConfigurationManager.AppSettings["emailComercial"];
                        emailUser += "@sifizsoft.com";

                        foreach (var ticket in ticketsCliente)
                        {
                            if (ticket.ticketHistorico != null)
                            {
                                var diasTranscurridos = (DateTime.Now - ticket.ticketHistorico.FechaOperacion).Days;
                                if (diasTranscurridos >= tiempo.diasCerrado)
                                {
                                    ticketController.CerrarTicketPorCliente(ticket.idTicket, emailUser);
                                }
                            }
                        }
                    }
                }

                var resp = new
                {
                    success = true
                };
            }
            catch (Exception e)
            {
                var resp = new
                {
                    success = false,
                    msg = e.Message
                };
                throw;
            }
        }

        public class ClienteHoras
        {
            public string mes { get; set; }
            public int trabajadas { get; set; }
            public int contratadas { get; set; }
            public int total { get; set; }
        }
    }
}