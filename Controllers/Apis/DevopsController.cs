using System.Web.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using SifizPlanning.Models;
using SifizPlanning.Util;
using SifizPlanning.Security;
using System.Web;
using System.IO;

namespace SifizPlanning.Controllers
{
    public class DevopsController : ApiController
    {
        SifizPlanningEntidades db = DbCnx.getCnx();


        [HttpPost]
        public IHttpActionResult EnviarPublicacion([FromBody] TicketRequest ticketReq)
        {
            if (ticketReq == null)
            {
                return BadRequest("La solicitud del ticket es nula.");
            }

            string emailUser = User.Identity.Name;
            var user = db.Usuario.FirstOrDefault(x => x.Email == "sfzdevops@sifizsoft.com");
            if (user == null)
            {
                return BadRequest("Usuario no encontrado.");
            }

            var ticket = db.Ticket.Find(Int32.Parse(ticketReq.identifier));
            if (ticket == null)
            {
                return NotFound();
            }

            var ticketVersionCliente = ticket.SecuencialTicketVersionCliente != null
                ? db.TicketVersionCliente.Find(ticket.SecuencialTicketVersionCliente)
                : null;
            bool financial25 = ticketVersionCliente?.Codigo == "FBS 2.5";

            ticket.SecuencialEstadoTicket = 10; // EL TICKET ESTA RESUELTO
            ticket.SecuencialProximaActividad = 17; // CERTIFICAR
            var numeroVersion = db.TicketHistorico.Count(x => x.SecuencialTicket == ticket.Secuencial);

            var ticketHistorico = new TicketHistorico
            {
                ticket = ticket,
                Version = numeroVersion,
                SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA RESUELTO")
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

            db.SaveChanges(); // Salvando los cambios

            return Ok($"Se informó la publicación del ticket {ticketReq.identifier}. La próxima actividad ha sido actualizada a 'CERTIFICAR', el ticket ha sido sacado de la lista de publicaciones y se ha dejado en el histórico la acción de 'publicar'.");
        }


        //[HttpPost]
        //public IHttpActionResult AceptarTicket([FromBody] AceptarTicketRequest ticketReq)
        //{
        //    try
        //    {
        //        // Buscar el ticket en la base de datos
        //        Ticket ticket = db.Ticket.Find(ticketReq.idTicket);
        //        if (ticket == null)
        //        {
        //            return NotFound();
        //        }

        //        // Verificar el estado del ticket
        //        if (ticket.SecuencialEstadoTicket != 10)
        //        {
        //            return BadRequest("El ticket no está en estado resuelto");
        //        }

        //        // Obtener el email del cliente
        //        string emailCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault()?.Email;
        //        if (string.IsNullOrEmpty(emailCliente))
        //        {
        //            return BadRequest("Email del cliente no encontrado");
        //        }

        //        // Actualizar el estado del ticket
        //        ticket.SecuencialEstadoTicket = 14; // EL TICKET ESTA CERRADO
        //        ticket.SecuencialProximaActividad = 18; // NA porque está cerrado

        //        // Crear un nuevo historico del ticket
        //        int numeroVersion = db.TicketHistorico.Count(x => x.SecuencialTicket == ticket.Secuencial) + 1;
        //        TicketHistorico ticketHistorico = new TicketHistorico
        //        {
        //            ticket = ticket,
        //            Version = numeroVersion,
        //            SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA CERRADO")
        //            SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
        //            SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
        //            SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
        //            SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
        //            SecuencialProximaActividad = ticket.SecuencialProximaActividad,
        //            usuario = db.Usuario.FirstOrDefault(x => x.Email == emailCliente),
        //            ReportadoPor = ticket.ReportadoPor,
        //            Reputacion = ticket.Reputacion,
        //            Telefono = ticket.Telefono,
        //            Asunto = ticket.Asunto,
        //            Detalle = ticket.Detalle,
        //            FechaCreado = ticket.FechaCreado,
        //            Estimacion = ticket.Estimacion,
        //            NumeroVerificador = 1,
        //            FechaOperacion = DateTime.Now,
        //            SeFactura = ticket.SeFactura,
        //            Facturado = ticket.Facturado,
        //            IngresoInterno = ticket.IngresoInterno,
        //            Reprocesos = ticket.Reprocesos
        //        };

        //        // Guardar los cambios en la base de datos
        //        db.TicketHistorico.Add(ticketHistorico);
        //        db.SaveChanges();

        //        // Enviar un correo electrónico al cliente y a los gestores
        //        string textoEmail = "<div class=\"textoCuerpo\">" + @"Por medio de esta comunicación le informamos que el ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b> ha sido CERRADO; <br/> 
        //                     esta acción se ha tomado porque hemos recibido su conformidad con el mismo o en su defecto no se recibió una respuesta de su parte dentro del período de espera de los cinco días laborables contados desde la fecha en la que enviamos nuestra solicitud de certificación de conformidad de este requerimiento vía correo electrónico.<br/>
        //                     En el caso de requerir correcciones, solicitamos se ingrese un nuevo requerimiento y se incluya el código " + "\"HESO " + string.Format("{0:000000}", ticket.Secuencial) + "\" en el detalle del mismo.<br/> <b>Asunto del ticket: </b>" + ticket.Asunto + "</div>";

        //        List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");
        //        correosDestinos.Insert(0, emailCliente);

        //        var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
        //        foreach (var g in gestores)
        //        {
        //            correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
        //        }
        //        correosDestinos = correosDestinos.Distinct().ToList();

        //        string codigoCliente = ticket.persona_cliente.cliente.Codigo;
        //        Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket cerrado (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

        //        // Adicionar el email a los historicos
        //        string destinos = String.Join(", ", correosDestinos.ToArray());
        //        string textoHistoricoCorreo = "<b>Correo de información, Ticket Cerrado</b><br/>";
        //        textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
        //        textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket cerrado" + "<br/>";
        //        textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
        //        HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
        //        {
        //            SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
        //            VersionTicketHistorico = ticketHistorico.Version,
        //            Fecha = DateTime.Now,
        //            Texto = textoHistoricoCorreo
        //        };
        //        db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
        //        db.SaveChanges();

        //        return Ok("Ticket cerrado");
        //    }
        //    catch (Exception e)
        //    {
        //        return InternalServerError(e);
        //    }
        //}

        //[HttpPost]
        //public IHttpActionResult DevolverTicket([FromBody] DevolverTicketRequest ticketReq)
        //{
        //    try
        //    {
        //        // Buscar el ticket en la base de datos
        //        Ticket ticket = db.Ticket.Find(ticketReq.idTicket);
        //        if (ticket == null)
        //        {
        //            return NotFound();
        //        }

        //        // Verificar el estado del ticket
        //        if (ticket.SecuencialEstadoTicket != 10)
        //        {
        //            return BadRequest("El ticket no está en estado resuelto");
        //        }

        //        // Obtener los datos del cliente
        //        Persona personaCliente = ticket.persona_cliente.persona;
        //        Usuario usuarioCliente = personaCliente?.usuario.FirstOrDefault();
        //        if (personaCliente == null || usuarioCliente == null)
        //        {
        //            return BadRequest("Error encontrando los datos del cliente");
        //        }

        //        // Cambiar el estado del ticket
        //        ticket.SecuencialEstadoTicket = 18; // EL TICKET ESTA DEVUELTO
        //        ticket.SecuencialProximaActividad = 15; // AUDITAR

        //        // Añadir el histórico del ticket
        //        int numeroVersion = db.TicketHistorico.Count(x => x.SecuencialTicket == ticket.Secuencial) + 1;
        //        TicketHistorico ticketHistorico = new TicketHistorico
        //        {
        //            ticket = ticket,
        //            Version = numeroVersion,
        //            SecuencialEstadoTicket = 18, // EL TICKET ESTA DEVUELTO
        //            SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
        //            SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
        //            SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
        //            SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
        //            SecuencialProximaActividad = ticket.SecuencialProximaActividad, // AUDITAR
        //            usuario = usuarioCliente,
        //            ReportadoPor = ticket.ReportadoPor,
        //            Reputacion = ticket.Reputacion,
        //            Telefono = ticket.Telefono,
        //            Asunto = ticket.Asunto,
        //            Detalle = ticket.Detalle,
        //            FechaCreado = ticket.FechaCreado,
        //            Estimacion = ticket.Estimacion,
        //            NumeroVerificador = 1,
        //            FechaOperacion = DateTime.Now,
        //            SeFactura = ticket.SeFactura,
        //            Facturado = ticket.Facturado,
        //            IngresoInterno = ticket.IngresoInterno,
        //            Reprocesos = ticket.Reprocesos
        //        };
        //        db.TicketHistorico.Add(ticketHistorico);
        //        db.SaveChanges(); // Guardar los cambios

        //        // Procesar los ficheros adjuntos
        //        List<string> listaFicheros = new List<string>();
        //        List<string> listaPathFicheros = new List<string>();
        //        if (ticketReq.adjuntoDesacuerdo != null)
        //        {
        //            foreach (HttpPostedFileBase adj in ticketReq.adjuntoDesacuerdo)
        //            {
        //                if (adj != null)
        //                {
        //                    string extFile = Path.GetExtension(adj.FileName);
        //                    string newNameFile = System.IO.Path.GetRandomFileName() + extFile;
        //                    string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Web/resources/tickets"), newNameFile);
        //                    adj.SaveAs(path);

        //                    listaPathFicheros.Add(path);
        //                    listaFicheros.Add("/resources/tickets/" + newNameFile);
        //                }
        //            }
        //        }

        //        // Enviar los correos electrónicos
        //        string textoEmail = "<div class='textoCuerpo'>";
        //        textoEmail += "El presente correo electrónico es sobre la <b>No Aceptación</b> del ticket: " + string.Format("{0:000000}", ticket.Secuencial) + "<br/>";
        //        textoEmail += "Cliente: " + personaCliente.Nombre1 + " " + personaCliente.Apellido1 + "<br/>";
        //        textoEmail += "Detalle:<br> " + ticketReq.textoDesacuerdo;

        //        List<string> correosDestinos = new List<string>();
        //        string emailCliente = personaCliente.usuario.FirstOrDefault().Email;
        //        correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));
        //        correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("JEDES"));
        //        correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("AUDIT"));

        //        // Buscar los desarrolladores del ticket
        //        var correosColaboradores = (from c in db.Colaborador
        //                                    join p in db.Persona on c.SecuencialPersona equals p.Secuencial
        //                                    join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
        //                                    join t in db.Tarea on c.Secuencial equals t.SecuencialColaborador
        //                                    join tt in db.TicketTarea on t.Secuencial equals tt.SecuencialTarea
        //                                    where tt.SecuencialTicket == ticket.Secuencial
        //                                    select u.Email).Distinct().ToList();
        //        correosDestinos.AddRange(correosColaboradores);
        //        var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
        //        foreach (var g in gestores)
        //        {
        //            correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
        //        }
        //        correosDestinos = correosDestinos.Distinct().ToList();

        //        string asuntoEmail = ticket.persona_cliente.cliente.Codigo + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Devuelto (" + ticket.Asunto + ")";
        //        Utiles.EnviarEmailSistema(new string[] { emailCliente }, textoEmail, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));
        //        Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));

        //        // Añadir el email a los historicos
        //        correosDestinos.Add(emailCliente);
        //        string destinos = String.Join(", ", correosDestinos.ToArray());
        //        string textoHistoricoCorreo = "<b>Correo de información, Ticket Devuelto</b><br/>";
        //        textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
        //        textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket Devuelto" + "<br/>";
        //        textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
        //        HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
        //        {
        //            SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
        //            VersionTicketHistorico = ticketHistorico.Version,
        //            Fecha = DateTime.Now,
        //            Texto = textoHistoricoCorreo
        //        };
        //        db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
        //        db.SaveChanges();

        //        // Añadir los adjuntos
        //        foreach (string url in listaFicheros)
        //        {
        //            HistoricoAdjunto historicoAdjunto = new HistoricoAdjunto();
        //            historicoAdjunto.historicoInformacionTicket = historicoCorreoTicket;
        //            historicoAdjunto.Url = url;
        //            db.HistoricoAdjunto.Add(historicoAdjunto);
        //        }
        //        db.SaveChanges();

        //        return Ok("Ticket devuelto");
        //    }
        //    catch (Exception e)
        //    {
        //        return InternalServerError(e);
        //    }
        //}

    }

    public class TicketRequest
    {
        public string identifier{ get; set; }
    }
    public class AceptarTicketRequest
    {
        public int idTicket { get; set; }
    }
    public class DevolverTicketRequest
    {
        public int idTicket { get; set; }
        public string textoDesacuerdo { get; set; }
        public HttpPostedFileBase[] adjuntoDesacuerdo { get; set; }
    }
}
