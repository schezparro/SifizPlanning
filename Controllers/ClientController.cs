using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Reflection;
using System.Data.Entity.Core.Objects;
using System.Web.Script.Serialization;
using SifizPlanning.Models;
using SifizPlanning.Util;
using SifizPlanning.Security;
using System.Web.Routing;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Hangfire;

namespace SifizPlanning.Controllers
{
    public class ClientController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();

        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult Index()
        {
            string emailUser = User.Identity.Name;
            Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
            Persona persona = user.persona;

            ViewBag.nameUser = persona.Nombre1 + " " + persona.Apellido1;
            ViewBag.emailUser = user.Email;
            ViewBag.cliente = persona.persona_cliente.cliente.Descripcion;
            ViewBag.telefono = persona.persona_cliente.Telefono;

            var categorias = (from c in db.CategoriaTicket
                              where c.EstaActiva == 1
                              select new
                              {
                                  id = c.Secuencial,
                                  codigo = c.Codigo,
                                  nombre = c.Codigo + " - " + c.Descripcion
                              }).ToList();

            if (persona.persona_cliente != null)
            {
                var garantias = (from mt in db.MotivoTrabajo
                                 join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
                                 join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
                                 where mt.EstaActivo == 1 && cl.Descripcion == persona.persona_cliente.cliente.Descripcion /*&& tt.Codigo.ToString().ToUpper().Equals("PENDIENTES")*/
                                 orderby mt.Codigo
                                 select new
                                 {
                                     id = mt.Secuencial,
                                     codigo = mt.Codigo,
                                     fechaVencimiento = mt.motivoTrabajoInformacionAdicional != null ? DbFunctions.AddDays(mt.motivoTrabajoInformacionAdicional.FechaProduccion, mt.motivoTrabajoInformacionAdicional.DiasGarantia) : (DateTime?)null,
                                     diasRestantes = mt.motivoTrabajoInformacionAdicional != null ? DbFunctions.DiffDays(DateTime.Now, mt.motivoTrabajoInformacionAdicional.FechaProduccion) + mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0,
                                     entregables = (from e in mt.entregableMotivoTrabajo
                                                    where e.EstaActivo == 1
                                                    select new
                                                    {
                                                        id = e.Secuencial,
                                                        descripcion = e.Nombre,
                                                        e.FechaProduccion,
                                                        FechaVencimiento = DbFunctions.AddDays(e.FechaProduccion, mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0),
                                                        DiasRestantes = DbFunctions.DiffDays(DateTime.Now, DbFunctions.AddDays(e.FechaProduccion, mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0))
                                                    }).OrderByDescending(x => x.DiasRestantes).ToList(),
                                     descripcion = mt.Codigo + " __ " + "Fecha Vencimiento GT: " + (mt.motivoTrabajoInformacionAdicional != null ? DbFunctions.AddDays(mt.motivoTrabajoInformacionAdicional.FechaProduccion, mt.motivoTrabajoInformacionAdicional.DiasGarantia) : (DateTime?)null).ToString() + " __ " + mt.Descripcion
                                 }).ToList();


                garantias = (from g in garantias
                             select new
                             {
                                 id = g.id,
                                 codigo = g.codigo,
                                 fechaVencimiento = g.fechaVencimiento,
                                 diasRestantes = g.diasRestantes,
                                 entregables = (from e in g.entregables
                                                select new
                                                {
                                                    id = e.id,
                                                    descripcion = e.descripcion,
                                                    e.FechaProduccion,
                                                    FechaVencimiento = e.FechaVencimiento != null ? e.FechaVencimiento : g.fechaVencimiento,
                                                    DiasRestantes = e.DiasRestantes != null ? e.DiasRestantes : g.diasRestantes
                                                }).OrderByDescending(x => x.DiasRestantes).ToList(),
                                 descripcion = g.descripcion
                             }).ToList();

                garantias = (from c in garantias
                             where c.diasRestantes > 0 || c.entregables.Where(s => s.DiasRestantes > 0).Count() > 0
                             select c).ToList();

                garantias = (from g in garantias
                             select new
                             {
                                 id = g.id,
                                 codigo = g.codigo,
                                 fechaVencimiento = g.fechaVencimiento,
                                 diasRestantes = g.diasRestantes,
                                 entregables = g.entregables.Where(e => e.DiasRestantes > 0).ToList(),
                                 descripcion = g.descripcion
                             }).ToList();

                if (garantias.Count == 0)
                {
                    categorias = (from c in categorias
                                  where c.codigo != "GARANTÍA TÉCNICA"
                                  select c).ToList();
                }

                ViewData["garantias"] = garantias;
            }

            var contratos = (from mt in db.MotivoTrabajo
                             join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
                             join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
                             where mt.EstaActivo == 1 && mt.Avance != 100 && tt.Codigo == "PENDIENTES" && cl.Descripcion == persona.persona_cliente.cliente.Descripcion
                             orderby mt.Secuencial
                             select new
                             {
                                 id = mt.Secuencial,
                                 codigo = mt.Codigo,
                                 descripcion = mt.Codigo + " - " + mt.Descripcion,
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


            contratos = (from c in contratos
                         where c.estado != "VENCIDO"
                         select c).ToList();

            bool existeContratoAceptaNormativos = (from mt in db.MotivoTrabajo
                                                   join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
                                                   where mt.EstaActivo == 1 && mt.Avance != 100 && (mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.AceptaCambiosNormativos ?? 0 : 0) == 1
                                                   select mt).Any();

            if (!existeContratoAceptaNormativos)
            {
                categorias = (from c in categorias
                              where c.codigo != "CAMBIOS NORMATIVOS"
                              select c).ToList();
            }

            if (contratos.Count == 0)
            {
                categorias = (from c in categorias
                              where c.codigo != "MANTENIMIENTO"
                              select c).ToList();
            }

            ViewData["contratos"] = contratos;
            ViewData["categorias"] = categorias;

            return View();
        }

        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult DarGarantiasTicketsCliente()
        {
            try
            {
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Persona persona = user.persona;

                if (persona.persona_cliente != null)
                {
                    var garantias = (from mt in db.MotivoTrabajo
                                     join tt in db.TipoMotivoTrabajo on mt.SecuencialTipoMotivoTrabajo equals tt.Secuencial
                                     join cl in db.Cliente on mt.SecuencialCliente equals cl.Secuencial
                                     where mt.EstaActivo == 1 && cl.Descripcion == persona.persona_cliente.cliente.Descripcion /*&& tt.Codigo.ToString().ToUpper().Equals("PENDIENTES")*/
                                     orderby mt.Codigo
                                     select new
                                     {
                                         id = mt.Secuencial,
                                         codigo = mt.Codigo,
                                         fechaVencimiento = mt.motivoTrabajoInformacionAdicional != null ? DbFunctions.AddDays(mt.motivoTrabajoInformacionAdicional.FechaProduccion, mt.motivoTrabajoInformacionAdicional.DiasGarantia) : (DateTime?)null,
                                         diasRestantes = mt.motivoTrabajoInformacionAdicional != null ? DbFunctions.DiffDays(DateTime.Now, mt.motivoTrabajoInformacionAdicional.FechaProduccion) + mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0,
                                         entregables = (from e in mt.entregableMotivoTrabajo
                                                        where e.EstaActivo == 1
                                                        select new
                                                        {
                                                            e.Nombre,
                                                            e.FechaProduccion,
                                                            FechaVencimiento = DbFunctions.AddDays(e.FechaProduccion, mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0),
                                                            DiasRestantes = DbFunctions.DiffDays(DateTime.Now, DbFunctions.AddDays(e.FechaProduccion, mt.motivoTrabajoInformacionAdicional != null ? mt.motivoTrabajoInformacionAdicional.DiasGarantia : 0))
                                                        }).OrderByDescending(x => x.DiasRestantes).ToList(),
                                         descripcion = mt.Codigo + " __ " + "Fecha Vencimiento: " + (mt.motivoTrabajoInformacionAdicional != null ? DbFunctions.AddDays(mt.motivoTrabajoInformacionAdicional.FechaProduccion, mt.motivoTrabajoInformacionAdicional.DiasGarantia) : (DateTime?)null).ToString() + " __ " + mt.Descripcion
                                     }).ToList();
                    garantias = (from c in garantias
                                 where c.diasRestantes > 0 || c.entregables.Where(s => s.DiasRestantes > 0).Count() > 0
                                 select c).ToList();

                    var resp = new
                    {
                        garantias = garantias,
                        success = true
                    };
                    return Json(resp);
                }
                else
                {
                    throw new Exception("El usuario en es un cliente");
                }
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

        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult DarCategoriasTicketSegunContratosClientes()
        {
            //Aqui retornar todas las categorías hasta que se informaticen bien los contratos
            var categorias = (from c in db.CategoriaTicket
                              where c.EstaActiva == 1
                              select new
                              {
                                  id = c.Secuencial,
                                  codigo = c.Codigo,
                                  nombre = c.Codigo + " - " + c.Descripcion
                              });

            var resp = new
            {
                success = true,
                categorias = categorias
            };
            return Json(resp);
        }

        [Authorize(Roles = "ADMIN, CLIENTE")]
        public async Task<JsonResult> GuardarPeticionDeTicket(string reportadoPor, int prioridad, string categoriaTicket, string asunto, string detalle, string motivoPrioridad, string motivoTrabajo, string entregableGarantia, int esUrgente, int ticketVersionCliente, int moduloSecuencial, string telefono = "", HttpPostedFileBase[] adjuntos = null)
        {
            try
            {
                string msg = "Se ha registrado correctamente la solicitud de ticket";

                string emailUser = User.Identity.Name;
                // Use var instead of explicit types
                var user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                var persona = user.persona;
                var ahora = DateTime.Now;
                var motivo = int.TryParse(motivoTrabajo, out var resultMotivo) ? resultMotivo : 0;
                var entregable = int.TryParse(entregableGarantia, out var resultEntregable) ? resultEntregable : 0;
                var categoria = db.CategoriaTicket.Single(s => s.EstaActiva == 1 && s.Codigo == categoriaTicket).Secuencial;

                const int estadoTicket = 1; //Abierto

                if (telefono == "")
                    telefono = "-";

                var nuevoTicket = new Ticket
                {
                    SecuencialEstadoTicket = estadoTicket, //Abierto
                    SecuencialPersona_Cliente = persona.Secuencial,
                    SecuencialCategoriaTicket = categoria,
                    SecuencialPrioridadTicket = prioridad,
                    SecuencialProximaActividad = 9,
                    SecuencialTipoRecurso = 1,
                    SecuencialTicketVersionCliente = ticketVersionCliente,
                    Telefono = telefono,
                    ReportadoPor = reportadoPor,
                    Asunto = asunto,
                    Detalle = detalle,
                    FechaCreado = ahora,
                    HorasCreado = 0,
                    Reputacion = 5, //Es el máximo
                    Estimacion = 0,
                    NumeroVerificador = 1,
                    Fecha = ahora,
                    SecuencialModulo = moduloSecuencial
                };

                db.Ticket.Add(nuevoTicket);
                var secuencialTicket = nuevoTicket.Secuencial;

                if (!string.IsNullOrEmpty(motivoPrioridad) && esUrgente == 1)
                {
                    var motivoPrioridadTicket = new MotivoPrioridadTicket
                    {
                        SecuencialTicket = secuencialTicket,
                        Motivo = motivoPrioridad
                    };
                    db.MotivoPrioridadTicket.Add(motivoPrioridadTicket);
                }

                //Guardando el histórico
                var nuevoTicketHistorico = new TicketHistorico
                {
                    SecuencialTicket = secuencialTicket,
                    Version = 0,
                    SecuencialEstadoTicket = estadoTicket, //Abierto
                    SecuencialPersona_Cliente = persona.Secuencial,
                    SecuencialCategoriaTicket = categoria,
                    SecuencialPrioridadTicket = prioridad,
                    SecuencialProximaActividad = 9,
                    SecuencialTipoRecurso = 1,
                    Telefono = telefono,
                    ReportadoPor = reportadoPor,
                    Asunto = asunto,
                    Detalle = detalle,
                    FechaCreado = ahora,
                    Reputacion = 5,
                    Estimacion = 0,
                    NumeroVerificador = 1,
                    FechaOperacion = ahora,
                    usuario = user,
                    Fecha = ahora
                };
                db.TicketHistorico.Add(nuevoTicketHistorico);

                if (motivo != 0)
                {
                    var motivoTrabajoTicket = new MotivoTrabajoTicket
                    {
                        SecuencialMotivoTrabajo = motivo,
                        SecuencialTicket = secuencialTicket
                    };
                    db.MotivoTrabajoTicket.Add(motivoTrabajoTicket);
                }
                if (entregable != 0)
                {
                    var entregableTicket = new EntregableTicket
                    {
                        SecuencialEntregable = entregable,
                        SecuencialTicket = secuencialTicket
                    };
                    db.ENTREGABLETICKET.Add(entregableTicket);
                }

                //Por los ficheros adjuntos
                if (adjuntos?.Length > 0)
                {
                    foreach (var adj in adjuntos.Where(adj => adj != null))
                    {
                        string extFile = Path.GetExtension(adj.FileName);
                        string newNameFile = Utiles.RandomString(10) + extFile;
                        newNameFile = System.IO.Path.GetRandomFileName() + extFile;
                        string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                        adj.SaveAs(path);

                        var adjTicket = new AdjuntoTicket
                        {
                            Url = "/resources/tickets/" + newNameFile,
                            ticket = nuevoTicket
                        };

                        db.AdjuntoTicket.Add(adjTicket);
                    }
                }

                db.SaveChanges();

                var numeroTicket = string.Format("{0:000000}", nuevoTicket.Secuencial);
                var codPrioridadTicket = db.PrioridadTicket.Find(nuevoTicket.SecuencialPrioridadTicket).Codigo;

                var textoInicial = new StringBuilder();
                var saludo = persona.Sexo == "F" ? "Estimada:" : "Estimado:";
                textoInicial.Append($"<div class='textoCuerpo'>{saludo} {persona.Nombre1} {persona.Apellido1}<br>");
                textoInicial.Append($"Hemos recibido correctamente su petición de soporte, la misma responde al ticket: <b>{numeroTicket}.</b> <br/>");
                textoInicial.Append($"<b>Asunto del requerimiento: </b>{nuevoTicket.Asunto}<br>");

                if (nuevoTicket.SecuencialPrioridadTicket == 1)
                {
                    textoInicial.Append($"Como su ticket está catalogado con prioridad {codPrioridadTicket}, <br/>");
                    textoInicial.Append("usted debe comunicarse con nuestra oficina para continuar con la gestión del ticket.<br/> ");
                    textoInicial.Append("(Teléfonos: 02-351-7729, 02-351-8919, 02-450-4616, 02-450-4727) (Teléfono de emergencia: 098-603-7821)<br/>");
                    textoInicial.Append("(Correo Electrónico: vhidalgo@sifizsoft.com) (Skype: victorhidalgo)<br/>");
                }
                else
                {
                    textoInicial.Append("Nuestro equipo de soporte se pondrá en contacto con usted lo más pronto posible.");
                }
                textoInicial.Append("</div>");
                var textoEmail = textoInicial.ToString();
                var usuariosDestinos = new List<string>
                                            {
                                                emailUser
                                            };
                usuariosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));
                var personaCliente = db.Persona_Cliente.Find(nuevoTicket.SecuencialPersona_Cliente);
                var codigoCliente = personaCliente.cliente.Codigo;
                var asuntoEmail = $"{codigoCliente} HESO {numeroTicket} - Adicionado el nuevo ticket ({nuevoTicket.Asunto})";

                var gestores = nuevoTicket.persona_cliente.cliente.gestorServicios.ToList();
                var colaboradores = gestores.Select(g => g.colaborador.persona.usuario.FirstOrDefault()?.Email).ToList();
                usuariosDestinos.AddRange(colaboradores.Distinct());

                Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), textoEmail, asuntoEmail, null, null);

                var destinos = string.Join(", ", usuariosDestinos.ToArray());
                var textoHistoricoCorreo = "<b>Correo de creación de nuevo ticket</b><br/>";
                textoHistoricoCorreo += $"<b>Destinos:</b> {destinos}<br/>";
                textoHistoricoCorreo += $"<b>Asunto:</b> {asuntoEmail}<br/>";
                textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;

                var historicoCorreo = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = nuevoTicketHistorico.SecuencialTicket,
                    VersionTicketHistorico = nuevoTicketHistorico.Version,
                    Fecha = DateTime.Now,
                    Texto = textoHistoricoCorreo
                };
                db.HistoricoInformacionTicket.Add(historicoCorreo);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }

        //[Authorize(Roles = "ADMIN, CLIENTE")]
        //public ActionResult GuardarPeticionDeTicket(string reportadoPor, int prioridad, string categoriaTicket, string asunto, string detalle, string motivoPrioridad, string motivoTrabajo, string entregableGarantia, int esUrgente, int ticketVersionCliente, string telefono = "", HttpPostedFileBase[] adjuntos = null)
        //{
        //    try
        //    {
        //        string msg = "Se ha registrado correctamente la solicitud de ticket";

        //        string emailUser = User.Identity.Name;
        //        Usuario user = db.Usuario.Find(emailUser);
        //        Persona persona = user.persona;
        //        DateTime ahora = DateTime.Now;
        //        int motivo = int.Parse(motivoTrabajo);
        //        int entregable = int.Parse(entregableGarantia);
        //        int categoria = db.CategoriaTicket.Single(s => s.EstaActiva == 1 && s.Codigo == categoriaTicket).Secuencial;

        //        int estadoTicket = 1;//Abierto

        //        if (telefono == "")
        //            telefono = "-";
        //        Ticket nuevoTicket = new Ticket
        //        {
        //            SecuencialEstadoTicket = estadoTicket,//Abierto
        //            SecuencialPersona_Cliente = persona.Secuencial,
        //            SecuencialCategoriaTicket = categoria,
        //            SecuencialPrioridadTicket = prioridad,
        //            SecuencialProximaActividad = 9,
        //            SecuencialTipoRecurso = 1,
        //            SecuencialTicketVersionCliente = ticketVersionCliente,
        //            Telefono = telefono,
        //            ReportadoPor = reportadoPor,
        //            Asunto = asunto,
        //            Detalle = detalle,
        //            FechaCreado = ahora,
        //            Reputacion = 5,//Es el máximo
        //            Estimacion = 0,
        //            NumeroVerificador = 1,
        //            Fecha = ahora
        //        };

        //        db.Ticket.Add(nuevoTicket);
        //        int secuencialTicket = nuevoTicket.Secuencial;

        //        if (motivoPrioridad != "" && esUrgente == 1)
        //        {
        //            MotivoPrioridadTicket motivoPrioridadTicket = new MotivoPrioridadTicket();
        //            motivoPrioridadTicket.SecuencialTicket = secuencialTicket;
        //            motivoPrioridadTicket.Motivo = motivoPrioridad;
        //            db.MotivoPrioridadTicket.Add(motivoPrioridadTicket);
        //        }

        //        //Guardando el histórico
        //        TicketHistorico nuevoTicketHistorico = new TicketHistorico
        //        {
        //            SecuencialTicket = secuencialTicket,
        //            Version = 0,
        //            SecuencialEstadoTicket = estadoTicket,//Abierto
        //            SecuencialPersona_Cliente = persona.Secuencial,
        //            SecuencialCategoriaTicket = categoria,
        //            SecuencialPrioridadTicket = prioridad,
        //            SecuencialProximaActividad = 9,
        //            SecuencialTipoRecurso = 1,
        //            Telefono = telefono,
        //            ReportadoPor = reportadoPor,
        //            Asunto = asunto,
        //            Detalle = detalle,
        //            FechaCreado = ahora,
        //            Reputacion = 5,
        //            Estimacion = 0,
        //            NumeroVerificador = 1,
        //            FechaOperacion = ahora,
        //            usuario = user,
        //            Fecha = ahora
        //        };
        //        db.TicketHistorico.Add(nuevoTicketHistorico);

        //        if (motivo != 0)
        //        {
        //            MotivoTrabajoTicket motivoTrabajoTicket = new MotivoTrabajoTicket();
        //            motivoTrabajoTicket.SecuencialMotivoTrabajo = motivo;
        //            motivoTrabajoTicket.SecuencialTicket = secuencialTicket;
        //            db.MotivoTrabajoTicket.Add(motivoTrabajoTicket);
        //        }
        //        if (entregable != 0)
        //        {
        //            EntregableTicket entregableTicket = new EntregableTicket();
        //            entregableTicket.SecuencialEntregable = entregable;
        //            entregableTicket.SecuencialTicket = secuencialTicket;
        //            db.ENTREGABLETICKET.Add(entregableTicket);
        //        }

        //        //Por los ficheros adjuntos
        //        if (adjuntos != null)
        //            foreach (HttpPostedFileBase adj in adjuntos)
        //            {
        //                if (adj != null)
        //                {
        //                    string extFile = Path.GetExtension(adj.FileName);
        //                    string newNameFile = Utiles.RandomString(10) + extFile;
        //                    newNameFile = System.IO.Path.GetRandomFileName() + extFile;
        //                    string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
        //                    adj.SaveAs(path);

        //                    AdjuntoTicket adjTicket = new AdjuntoTicket
        //                    {
        //                        Url = "/resources/tickets/" + newNameFile,
        //                        ticket = nuevoTicket
        //                    };

        //                    db.AdjuntoTicket.Add(adjTicket);
        //                }
        //            }

        //        db.SaveChanges();

        //        StringBuilder textoInicial = new StringBuilder();
        //        string saludo = persona.Sexo == "F" ? "Estimada:" : "Estimado:";
        //        string numeroTicket = string.Format("{0:000000}", nuevoTicket.Secuencial);
        //        string codPrioridadTicket = db.PrioridadTicket.Find(nuevoTicket.SecuencialPrioridadTicket).Codigo;

        //        textoInicial.Append($"<div class='textoCuerpo'>{saludo} {persona.Nombre1} {persona.Apellido1}<br>");
        //        textoInicial.Append($"Hemos recibido correctamente su petición de soporte, la misma responde al ticket: <b>{numeroTicket}.</b> <br/>");
        //        textoInicial.Append($"<b>Asunto del requerimiento: </b>{nuevoTicket.Asunto}<br>");

        //        if (nuevoTicket.SecuencialPrioridadTicket == 1)
        //        {
        //            textoInicial.Append($"Como su ticket está catalogado con prioridad {codPrioridadTicket}, <br/>");
        //            textoInicial.Append("usted debe comunicarse con nuestra oficina para continuar con la gestión del ticket.<br/> ");
        //            textoInicial.Append("(Teléfonos: 02-351-7729, 02-351-8919, 02-450-4616, 02-450-4727) (Teléfono de emergencia: 098-603-7821)<br/>");
        //            textoInicial.Append("(Correo Electrónico: vhidalgo@sifizsoft.com) (Skype: victorhidalgo)<br/>");
        //        }
        //        else
        //        {
        //            textoInicial.Append("Nuestro equipo de soporte se pondrá en contacto con usted lo más pronto posible.");
        //        }
        //        textoInicial.Append("</div>");
        //        string textoEmail = textoInicial.ToString();

        //        List<string> usuariosDestinos = new List<string>();
        //        //usuariosDestinos.Add( "rcespedes@sifizsoft.com" ); //borrar aqui
        //        usuariosDestinos.Add(emailUser);
        //        usuariosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));

        //        Persona_Cliente personaCliente = db.Persona_Cliente.Find(nuevoTicket.SecuencialPersona_Cliente);
        //        string codigoCliente = personaCliente.cliente.Codigo;
        //        string asuntoEmail = codigoCliente + " HESO " + numeroTicket + " - Adicionado el nuevo ticket (" + nuevoTicket.Asunto + ")";

        //        var gestores = nuevoTicket.persona_cliente.cliente.gestorServicios.ToList();
        //        foreach (var g in gestores)
        //        {
        //            usuariosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
        //        }
        //        usuariosDestinos = usuariosDestinos.Distinct().ToList();
        //        Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), textoEmail, asuntoEmail);

        //        string destinos = String.Join(", ", usuariosDestinos.ToArray());
        //        string textoHistoricoCorreo = "<b>Correo de creación de nuevo ticket</b><br/>";
        //        textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
        //        textoHistoricoCorreo += "<b>Asunto:</b> " + asuntoEmail + "<br/>";
        //        textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;

        //        HistoricoInformacionTicket historicoCorreo = new HistoricoInformacionTicket
        //        {
        //            SecuencialTicketHistorico = nuevoTicketHistorico.SecuencialTicket,
        //            VersionTicketHistorico = nuevoTicketHistorico.Version,
        //            Fecha = DateTime.Now,
        //            Texto = textoHistoricoCorreo
        //        };
        //        db.HistoricoInformacionTicket.Add(historicoCorreo);
        //        db.SaveChanges();

        //        var resp = new
        //        {
        //            success = true,
        //            msg = msg
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

        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult DarSeguimientosTicketsCliente(int start, int lenght, string filtro = "", int order = 0, int asc = 1, bool todos = false)
        {
            var s = new JavaScriptSerializer();
            var jsonObj = s.Deserialize<dynamic>(filtro);

            string filtroNumero = jsonObj["numero"].ToString();
            string filtroClienteSolicitud = jsonObj["clienteSolicitud"].ToString();
            string filtroFecha = jsonObj["fecha"].ToString();
            string filtrofechaVencimiento = jsonObj["fechaVencimiento"].ToString();
            string filtroPrioridad = jsonObj["prioridad"].ToString();
            string filtroCategoria = jsonObj["categoria"].ToString();
            string filtroAsunto = jsonObj["asunto"].ToString();
            string filtroResponsable = jsonObj["responsable"].ToString();
            string filtroEstado = jsonObj["estado"].ToString();

            try
            {
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Persona persona = user.persona;
                Persona_Cliente personaCliente = persona.persona_cliente;
                Cliente cliente = personaCliente.cliente;
                DateTime defaultDate = new DateTime();
                defaultDate = defaultDate.AddDays(30);

                var tickets = (from t in db.Ticket
                               join pc in db.Persona_Cliente on t.persona_cliente equals pc
                               join et in db.EstadoTicket on t.estadoTicket equals et
                               join pr in db.PrioridadTicket on t.prioridadTicket equals pr
                               join ct in db.CategoriaTicket on t.categoriaTicket equals ct
                               where pc.SecuencialCliente == cliente.Secuencial
                               orderby t.FechaCreado descending
                               select new
                               {
                                   id = t.Secuencial,
                                   fecha = t.FechaCreado,
                                   fechaVencimiento = DbFunctions.AddDays((from thi in t.ticketHistorico
                                                                           where thi.estadoTicket.Codigo == "CERRADO" && db.TicketHistorico.Where(h => h.Version == thi.Version - 1 && h.SecuencialTicket == t.Secuencial).FirstOrDefault().estadoTicket.Codigo != "CERRADO"
                                                                           orderby thi.Version descending
                                                                           select thi.FechaOperacion
                                             ).FirstOrDefault(), 30),
                                   prioridad = pr.Codigo,
                                   categoria = ct.Codigo,
                                   asunto = t.Asunto,
                                   persona = pc.persona.Nombre1 + " " + pc.persona.Nombre2,
                                   cliente = pc.cliente.Descripcion,
                                   ticketVersionCliente = db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(tvc => tvc.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Descripcion : "NO ASIGNADO",
                                   asignado = (
                                                    db.TicketTarea.Where(x => x.SecuencialTicket == t.Secuencial && x.EstaActiva == 1).Count() > 0
                                               ) ?
                                                   (from p in db.Persona
                                                    join c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
                                                    join tar in db.Tarea on c.Secuencial equals tar.SecuencialColaborador
                                                    join ttar in db.TicketTarea on tar.Secuencial equals ttar.SecuencialTarea
                                                    where ttar.SecuencialTicket == t.Secuencial
                                                    select p.Nombre1 + " " + p.Apellido1).FirstOrDefault()
                                                 : "NO ASIGNADO",
                                   fechaAsignado = db.TicketTarea.Where(tt => tt.SecuencialTicket == t.Secuencial).OrderByDescending(tt => tt.Secuencial).Select(tt => tt.tarea.FechaInicio).FirstOrDefault(),
                                   estado = et.Codigo,
                                   clase = (t.estadoTicket.Codigo == "CERRADO") ? "fondoCerrado" :
                                           (t.estadoTicket.Codigo == "ANULADO") ? "fondoAnulado" :
                                           (t.estadoTicket.Codigo == "ESPERANDO RESPUESTA") ? "fondoEsperandoRespuesta" :
                                           (t.estadoTicket.Codigo == "ABIERTO" || t.estadoTicket.Codigo == "ESPERANDO LLAMADA") ? "fondoAbierto" : "fondoDesarrollo"
                               }).ToList();

                if (todos == false)
                {
                    tickets = tickets.Where(x => x.estado != "CERRADO" && x.estado != "ANULADO" && x.estado != "RECHAZADO").ToList();
                }

                //Se aplican los filtros
                if (filtroNumero != "")
                {
                    tickets = (from c in tickets
                               where c.id.ToString().ToLower().Contains(filtroNumero.ToLower())
                               select c).ToList();
                }

                if (filtroClienteSolicitud != "")
                {
                    tickets = (from c in tickets
                               where c.persona.ToString().ToLower().Contains(filtroClienteSolicitud.ToLower())
                               select c).ToList();
                }
                if (filtroFecha != "")
                {
                    tickets = (from c in tickets
                               where c.fecha.ToString("dd/MM/yyyy").Contains(filtroFecha)
                               select c).ToList();
                }
                if (filtrofechaVencimiento != "")
                {
                    tickets = (from c in tickets
                               where c.fechaVencimiento != defaultDate && c.fechaVencimiento.Value.ToString("dd/MM/yyyy").Contains(filtrofechaVencimiento)
                               select c).ToList();
                }
                if (filtroPrioridad != "")
                {
                    tickets = (from c in tickets
                               where c.prioridad.ToLower().Contains(filtroPrioridad.ToLower())
                               select c).ToList();
                }
                if (filtroCategoria != "")
                {
                    tickets = (from c in tickets
                               where c.categoria.ToString().ToUpper().Contains(filtroCategoria.ToUpper())
                               select c).ToList();
                }
                if (filtroAsunto != "")
                {
                    tickets = (from c in tickets
                               where c.asunto.ToString().ToUpper().Contains(filtroAsunto.ToUpper())
                               select c).ToList();
                }
                if (filtroResponsable != "")
                {
                    tickets = (from c in tickets
                               where c.asignado.ToString().ToLower().Contains(filtroResponsable.ToLower())
                               select c).ToList();
                }
                if (filtroEstado != "")
                {
                    tickets = (from c in tickets
                               where c.estado.ToString().ToLower().Contains(filtroEstado.ToLower())
                               select c).ToList();
                }

                //Se ordena
                if (order > 0)
                {
                    switch (order)
                    {
                        case 1:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.id
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.id descending
                                           select c).ToList();
                            }
                            break;
                        case 2:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.persona
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.persona descending
                                           select c).ToList();
                            }
                            break;
                        case 3:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.fecha
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.fecha descending
                                           select c).ToList();
                            }
                            break;
                        case 4:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.fechaVencimiento
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.fechaVencimiento descending
                                           select c).ToList();
                            }
                            break;
                        case 5:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.prioridad
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.prioridad descending
                                           select c).ToList();
                            }
                            break;
                        case 6:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.categoria
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.categoria descending
                                           select c).ToList();
                            }
                            break;
                        case 7:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.asunto
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.asunto descending
                                           select c).ToList();
                            }
                            break;
                        case 8:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.asignado
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.asignado descending
                                           select c).ToList();
                            }
                            break;
                        case 9:
                            if (asc == 1)
                            {
                                tickets = (from c in tickets
                                           orderby c.estado
                                           select c).ToList();
                            }
                            else
                            {
                                tickets = (from c in tickets
                                           orderby c.estado descending
                                           select c).ToList();
                            }
                            break;
                    }
                }

                //Calculando la reputación
                var ticketsCliente = (from t in db.Ticket
                                      join pc in db.Persona_Cliente on t.persona_cliente equals pc
                                      where pc.SecuencialCliente == cliente.Secuencial
                                      select new
                                      {
                                          idTicket = t.Secuencial,
                                          reputacion = t.Reputacion
                                      }).ToList();

                int cant = 5;
                int total = 5;
                foreach (var tc in ticketsCliente)
                {
                    cant += 5;//5 es el valor máximo
                    total += (int)tc.reputacion;
                }

                float div = (float)total / (float)cant;
                int reputacion = (int)Math.Floor(div * 100);

                var cantidad = tickets.Count;


                DateTime fechaInicial = new DateTime(1900, 1, 1);
                var ticketFinal = (from t in tickets
                                   select new
                                   {
                                       id = t.id,
                                       fecha = t.fecha,
                                       fechaVencimiento = t.fechaVencimiento,
                                       prioridad = t.prioridad,
                                       categoria = t.categoria,
                                       asunto = t.asunto,
                                       persona = t.persona,
                                       cliente = t.cliente,
                                       ticketVersionCliente = t.ticketVersionCliente,
                                       asignado = t.asignado,
                                       fechaAsignado = t.fechaAsignado > fechaInicial ? t.fechaAsignado.ToString("dd/MM/yyyy") : "NO ASIGNADO",
                                       estado = t.estado,
                                       clase = t.clase
                                   }).ToList();



                ticketFinal = ticketFinal.Skip(start).Take(lenght).ToList();

                var resp = new
                {
                    success = true,
                    tickets = ticketFinal,
                    cantidadTickets = cantidad,
                    reputacion = reputacion
                };
                return Json(resp);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    msg = e.Message,
                    success = false
                });
            }
        }

        [Authorize(Roles = "ADMIN, CLIENTE, USER")]
        public ActionResult DarDatosTicket(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket.");
                }

                var datosTicket = (from t in db.Ticket
                                   join
                                       pcl in db.Persona_Cliente on t.persona_cliente equals pcl
                                   join
                                       etk in db.EstadoTicket on t.estadoTicket equals etk
                                   join
                                       ptk in db.PrioridadTicket on t.prioridadTicket equals ptk
                                   join
                                       ctk in db.CategoriaTicket on t.categoriaTicket equals ctk
                                   where t.Secuencial == ticket.Secuencial
                                   select new
                                   {
                                       id = t.Secuencial,
                                       usuarioCliente = pcl.persona.Nombre1 + " " + pcl.persona.Apellido1,
                                       idCliente = pcl.SecuencialCliente,
                                       cliente = pcl.cliente.Descripcion,
                                       clienteTelefono = pcl.Telefono,
                                       telefono = t.Telefono,
                                       reporto = t.ReportadoPor,
                                       reputacion = t.Reputacion,
                                       fecha = t.FechaCreado,
                                       estimacion = t.Estimacion,
                                       asunto = t.Asunto,
                                       detalle = t.Detalle,
                                       estado = etk.Codigo,
                                       prioridad = ptk.Codigo,
                                       categoria = ctk.Codigo,
                                       adjuntos = (from adjt in db.AdjuntoTicket
                                                   where adjt.SecuencialTicket == t.Secuencial && !adjt.Url.Contains("est_")
                                                   select adjt.Url).ToList(),
                                       verificador = t.NumeroVerificador,
                                       ticketVersionCliente = db.TicketVersionCliente.Where(s => s.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault() != null ? db.TicketVersionCliente.Where(s => s.Secuencial == t.SecuencialTicketVersionCliente).FirstOrDefault().Descripcion : "NO ASIGNADO"

                                   }).FirstOrDefault();

                //Calculando la reputación
                var ticketsCliente = (from t in db.Ticket
                                      join pc in db.Persona_Cliente on t.persona_cliente equals pc
                                      where pc.SecuencialCliente == datosTicket.idCliente
                                      select new
                                      {
                                          idTicket = t.Secuencial,
                                          reputacion = t.Reputacion
                                      }).ToList();

                int cant = 5;
                int total = 5;
                foreach (var tc in ticketsCliente)
                {
                    cant += 5;//5 es el valor máximo
                    total += (int)tc.reputacion;
                }

                float div = (float)total / (float)cant;
                int reputacion = (int)Math.Floor(div * 100);

                var resp = new
                {
                    success = true,
                    datosTicket = datosTicket,
                    reputacion = reputacion
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


        [Authorize(Roles = "ADMIN, CLIENTE")]
        [HttpPost]
        public ActionResult CargarComentariosTicketsCliente(int idTicket)
        {
            try
            {
                var comentarios = (from ct in db.ComentarioTicket
                                   where ct.SecuencialTicket == idTicket && ct.VerTodos == 1
                                   orderby ct.FechaHora descending
                                   select new
                                   {
                                       fechaHora = ct.FechaHora,
                                       colaborador = db.ComentarioTicketCliente.Where(s => s.SecuencialComentarioTicket == ct.Secuencial).FirstOrDefault() != null ? db.ComentarioTicketCliente.Where(s => s.SecuencialComentarioTicket == ct.Secuencial).FirstOrDefault().cliente.persona_cliente.FirstOrDefault().persona.usuario.FirstOrDefault().Email : ct.colaborador.persona.usuario.FirstOrDefault().Email,
                                       detalle = ct.Detalle
                                   }).ToList();

                var resp = new
                {
                    success = true,
                    comentarios = comentarios
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

        [Authorize(Roles = "ADMIN, CLIENTE")]
        [HttpPost]
        public ActionResult GuardarComentariosTicketsCliente(int idTicket, string comentario)
        {
            try
            {
                Ticket t = db.Ticket.Find(idTicket);
                if (t == null)
                {
                    throw new Exception("Error, no puede insertar un comentario porque no se encontró el ticket.");
                }

                //Adicionando el comentario al ticket                                
                string emailUser = User.Identity.Name;
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                Persona personaUsuario = user.persona;

                string colaborador = System.Configuration.ConfigurationManager.AppSettings["ColaboradorDefault"];
                colaborador += "@sifizsoft.com";
                Colaborador col = (from c in db.Colaborador
                                   join p in db.Persona on c.SecuencialPersona equals p.Secuencial
                                   join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                   where u.Email == colaborador
                                   select c).First();

                ComentarioTicket comentarioTicket = new ComentarioTicket
                {
                    SecuencialColaborador = col.Secuencial,
                    SecuencialTicket = idTicket,
                    FechaHora = DateTime.Now,
                    Detalle = comentario,
                    VerTodos = 1
                };
                db.ComentarioTicket.Add(comentarioTicket);

                ComentarioTicketCliente comentarioTicketCliente = new ComentarioTicketCliente();
                comentarioTicketCliente.SecuencialCliente = personaUsuario.persona_cliente.SecuencialCliente;
                comentarioTicketCliente.SecuencialComentarioTicket = comentarioTicket.Secuencial;
                db.ComentarioTicketCliente.Add(comentarioTicketCliente);

                ComentarioGeneral comentarioGeneral = new ComentarioGeneral
                {
                    usuario = user,
                    FechaHora = DateTime.Now,
                    TipoComentario = "NOTIFICACION",
                    Comentario = "<b>NOTIFICACIÓN</b>-Ticket: <b>" + string.Format("{0:000000}", t.Secuencial) + "</b>, Cliente: <b>" + t.persona_cliente.cliente.Codigo + "</b>, Asunto: <b>" + t.Asunto + "</b>, " + comentario,
                    Importancia = "Normal"
                };
                db.ComentarioGeneral.Add(comentarioGeneral);

                db.SaveChanges();

                //Llamar al SignalR
                //Websocket.getInstance().NuevosComentarios();
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

        [Authorize(Roles = "ADMIN, CLIENTE")]
        [HttpPost]
        public ActionResult EnviarEmailComentario(int idTicket, string destinatariosEmailTicket, string asuntoEmailTicket, string comentarioEmailTicket)
        {
            try
            {
                Ticket t = db.Ticket.Find(idTicket);
                if (t == null)
                {
                    throw new Exception("Error, no puede insertar un comentario porque no se encontró el ticket.");
                }
                string numeroTicket = string.Format("{0:000000}", t.Secuencial);
                string textoEmail = @"<div class='textoCuerpo'>Estimado(a): ";
                textoEmail += "<br>";
                textoEmail += "Se ha realizado el siguiente comentario del ticket: ";
                textoEmail += "<b>" + numeroTicket + ".</b> <br/>";
                textoEmail += comentarioEmailTicket;
                textoEmail += "</div>";

                List<string> usuariosDestinos = new List<string>();
                Regex rgx = new Regex(@"^(([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)(\s*;\s*|\s*$))*$");
                if (!rgx.IsMatch(destinatariosEmailTicket))
                {
                    throw new Exception("Debe ingresar una lista de correos válida separados por punto y coma(;)");
                };
                string[] emails = destinatariosEmailTicket.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var email in emails)
                {
                    usuariosDestinos.Add(email);
                }
                var gestores = t.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    usuariosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                usuariosDestinos = usuariosDestinos.Distinct().ToList();
                string asuntoEmail = asuntoEmailTicket;
                Utiles.EnviarEmailSistema(usuariosDestinos.ToArray(), textoEmail, asuntoEmail);

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


        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult AdicionarAdjuntosTicket(int idTicket, string asunto = "", string detalle = "", bool edit = false, HttpPostedFileBase[] adjuntos = null)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("Error, no se encontró el ticket");
                }

                if (edit)
                {
                    ticket.Asunto = asunto;
                    ticket.Detalle = detalle;
                }

                //Por los ficheros adjuntos   
                if (adjuntos != null)
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string extFile = Path.GetExtension(adj.FileName);
                            string newNameFile = System.IO.Path.GetRandomFileName() + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                            adj.SaveAs(path);

                            AdjuntoTicket adjTicket = new AdjuntoTicket
                            {
                                Url = "/resources/tickets/" + newNameFile,
                                ticket = ticket
                            };

                            db.AdjuntoTicket.Add(adjTicket);
                        }
                    }

                db.SaveChanges();

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

        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult DarPropuestaCotizacion(int idTicket)
        {
            try
            {
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }
                bool propuesta = false;
                bool renegociar = false;
                string texto = "";
                if (ticket.SecuencialEstadoTicket == 7)
                {
                    propuesta = true;
                    if (ticket.cotizacionTicket.FirstOrDefault().ofertaCotizacionTicket.LastOrDefault().Renegociable == 1)
                    {
                        renegociar = true;
                    }
                    texto = ticket.cotizacionTicket.FirstOrDefault().ofertaCotizacionTicket.LastOrDefault().TextoOferta;
                }
                var resp = new
                {
                    success = true,
                    propuesta = propuesta,
                    renegociar = renegociar,
                    texto = texto
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

        public ActionResult RespuestaCotizacion(int idTicket, bool respuesta)
        {
            try
            {
                string msg = "Se ha aceptado correctamente la cotización";

                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }

                CotizacionTicket cotizacion = ticket.cotizacionTicket.FirstOrDefault();
                if (cotizacion == null)
                {
                    throw new Exception("No se encontró la cotización del ticket");
                }

                OfertaCotizacionTicket oferta = cotizacion.ofertaCotizacionTicket.LastOrDefault();
                if (oferta == null)
                {
                    throw new Exception("No se encontró la oferta");
                }

                if (respuesta == true)
                {
                    oferta.OfertaAceptada = 1;
                    cotizacion.PrecioFinal = oferta.Precio;
                    cotizacion.CotizacionAceptada = 1;

                    //Enviando Email a los Implicados
                    string htmlEmail = "<div class=\"textoCuerpo\">" + "Se ha aceptado la cotización sobre el ticket: " + String.Format("{0:000000}", ticket.Secuencial) + "<br/>";
                    htmlEmail += "<b>Asunto del requerimiento: </b>" + ticket.Asunto + "<br>";
                    htmlEmail += "los datos de la oferta son:<br/>";
                    htmlEmail += oferta.TextoOferta + "<br/><br/>";

                    htmlEmail += "El equipo de coordinación de Sifizsoft.SA asignará el ticket lo antes posible.<br/> Puede hacer el seguimiento de los tickets entrando en nuestra web.<br/>";
                    //htmlEmail += "http://186.5.29.67/SifizPlanning" + "</div>";

                    string emailUser = User.Identity.Name;
                    List<string> emails = (from u in db.Usuario
                                           join
                                               ur in db.UsuarioRol on u.Secuencial equals ur.SecuencialUsuario
                                           join
                                               r in db.Rol on ur.rol equals r
                                           where u.EstaActivo == 1 && r.EstaActivo == 1 &&
                                                 (r.Codigo == "COORDINADOR" || r.Codigo == "COTIZADOR")
                                           select u.Email).Distinct().ToList<string>();
                    emails.Add(emailUser);
                    var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                    foreach (var g in gestores)
                    {
                        emails.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                    }
                    emails = emails.Distinct().ToList();

                    string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                    Utiles.EnviarEmailSistema(emails.ToArray(), htmlEmail, codigoCliente + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - Cotización de ticket aceptada (" + ticket.Asunto + ")");

                    //CAMBIANDO EL ESTADO DEL TICKET
                    ticket.SecuencialEstadoTicket = 8;//  ("RESPUESTA DEL CLIENTE RECIBIDA")
                    //ENTRANDO TICKET HISTÓRICO
                    Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                    int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                    TicketHistorico ticketHistorico = new TicketHistorico
                    {
                        ticket = ticket,
                        Version = numeroVersion,
                        SecuencialEstadoTicket = 8,//  ("RESPUESTA DEL CLIENTE RECIBIDA")
                        SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
                        SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
                        SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
                        SecuencialProximaActividad = ticket.SecuencialProximaActividad,
                        SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
                        usuario = user,
                        ReportadoPor = ticket.ReportadoPor,
                        Reputacion = ticket.Reputacion,
                        Telefono = ticket.Telefono,
                        Asunto = ticket.Asunto,
                        Detalle = ticket.Detalle,
                        FechaCreado = ticket.FechaCreado,
                        Estimacion = ticket.Estimacion,
                        NumeroVerificador = 1,
                        FechaOperacion = DateTime.Now
                    };
                    db.TicketHistorico.Add(ticketHistorico);
                }
                else
                {
                    msg = "Se ha rechazado correctamente la cotización";
                    oferta.OfertaAceptada = 0;
                    cotizacion.CotizacionAceptada = 0;
                    cotizacion.NumeroVerificador = cotizacion.NumeroVerificador + 1;

                    //Enviando Email a los Implicados
                    string htmlEmail = "<div class=\"textoCuerpo\">" + "Se ha rechazado la cotización sobre el ticket: " + String.Format("{0:000000}", cotizacion.ticket.Secuencial) + "<br/>";
                    htmlEmail += "<b>Asunto del requerimiento: </b>" + ticket.Asunto + "<br>";
                    htmlEmail += "los datos de la oferta son:<br/>";
                    htmlEmail += oferta.TextoOferta + "<br/><br/>";

                    htmlEmail += "El equipo de coordinación de Sifizsoft.SA anulará la solicitud del ticket.<br/> Puede hacer el seguimiento de sus tickets entrando en nuestra web.<br/>";
                    //htmlEmail += "http://sifizsoft.com/soporte-sifizsoft/" + "</div>";

                    string emailUser = User.Identity.Name;
                    List<string> emails = (from u in db.Usuario
                                           join
                                               ur in db.UsuarioRol on u.Secuencial equals ur.SecuencialUsuario
                                           join
                                               r in db.Rol on ur.rol equals r
                                           where u.EstaActivo == 1 && r.EstaActivo == 1 &&
                                                 (r.Codigo == "COORDINADOR" || r.Codigo == "COTIZADOR")
                                           select u.Email).Distinct().ToList<string>();

                    emails.Add(emailUser);
                    var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                    foreach (var g in gestores)
                    {
                        emails.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                    }
                    emails = emails.Distinct().ToList();
                    string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                    Utiles.EnviarEmailSistema(emails.ToArray(), htmlEmail, codigoCliente + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " cotización de ticket rechazada (" + ticket.Asunto + ")");

                    //CAMBIANDO EL ESTADO DEL TICKET
                    ticket.SecuencialEstadoTicket = 13;//  ("ANULADO")
                    //ENTRANDO TICKET HISTÓRICO
                    Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                    int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                    TicketHistorico ticketHistorico = new TicketHistorico
                    {
                        ticket = ticket,
                        Version = numeroVersion,
                        SecuencialEstadoTicket = 13,//  ("ANULADO")
                        SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
                        SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
                        SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
                        SecuencialProximaActividad = ticket.SecuencialProximaActividad,
                        SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
                        usuario = user,
                        ReportadoPor = ticket.ReportadoPor,
                        Reputacion = ticket.Reputacion,
                        Telefono = ticket.Telefono,
                        Asunto = ticket.Asunto,
                        Detalle = ticket.Detalle,
                        FechaCreado = ticket.FechaCreado,
                        Estimacion = ticket.Estimacion,
                        NumeroVerificador = 1,
                        FechaOperacion = DateTime.Now
                    };
                    db.TicketHistorico.Add(ticketHistorico);
                }

                db.SaveChanges();

                var resp = new
                {
                    success = true,
                    msg = msg
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

        public ActionResult RenegociarOferta(int idTicket, string texto)
        {
            try
            {
                string msg = "Se ha enviado correctamente la solicitud de renegociar";

                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("No se encontró el ticket");
                }

                CotizacionTicket cotizacion = ticket.cotizacionTicket.FirstOrDefault();
                if (cotizacion == null)
                {
                    throw new Exception("No se encontró la cotización del ticket");
                }

                OfertaCotizacionTicket oferta = cotizacion.ofertaCotizacionTicket.LastOrDefault();
                if (oferta == null)
                {
                    throw new Exception("No se encontró la oferta");
                }

                if (oferta.Renegociable != 1)
                {
                    throw new Exception("La oferta no es renegociable");
                }

                Renegociacion renegociacion = new Renegociacion
                {
                    ofertaCotizacionTicket = oferta,
                    TextoRenegociacion = texto,
                    Fecha = DateTime.Now
                };
                db.Renegociacion.Add(renegociacion);
                string emailUser = User.Identity.Name;

                //ENVIANDO EL EMAIL DE RENEGOCIACION                
                string htmlEmail = "<div class=\"textoCuerpo\">" + "Se ha realizado una petición de renegociación en el ticket: " + String.Format("{0:000000}", ticket.Secuencial) + "<br/>";
                htmlEmail += "<b>Asunto del requerimiento: </b>" + ticket.Asunto + "<br>";
                htmlEmail += "los datos de la renegociación son:<br/>";
                htmlEmail += texto + "<br/><br/>";

                htmlEmail += "Los cotizadores de Sifizsoft SA valorarán la petición y le responderán lo antes posible.<br/> Puede hacer el seguimiento de este y otros tickets entrando a nuestra web.<br/>";
                htmlEmail += "http://sifizsoft.com/soporte-sifizsoft/" + "</div>";

                List<string> emails = (from u in db.Usuario
                                       join
                                           ur in db.UsuarioRol on u.Secuencial equals ur.SecuencialUsuario
                                       join
                                           r in db.Rol on ur.rol equals r
                                       where u.EstaActivo == 1 && r.EstaActivo == 1 &&
                                             (r.Codigo == "COORDINADOR" || r.Codigo == "COTIZADOR")
                                       select u.Email).Distinct().ToList<string>();

                emails.Add(emailUser);
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    emails.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                emails = emails.Distinct().ToList();
                string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                Utiles.EnviarEmailSistema(emails.ToArray(), htmlEmail, codigoCliente + " HESO " + String.Format("{0:000000}", ticket.Secuencial) + " - Renegociación de cotización (" + ticket.Asunto + ")");

                //CAMBIANDO EL ESTADO DEL TICKET
                ticket.SecuencialEstadoTicket = 3;//  ("COTIZANDOCE")
                //ENTRANDO TICKET HISTÓRICO                
                Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailUser);
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = 3,//  ("COTIZANDOCE")
                    SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
                    SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
                    SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
                    SecuencialProximaActividad = ticket.SecuencialProximaActividad,
                    SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
                    usuario = user,
                    ReportadoPor = ticket.ReportadoPor,
                    Reputacion = ticket.Reputacion,
                    Telefono = ticket.Telefono,
                    Asunto = ticket.Asunto,
                    Detalle = ticket.Detalle,
                    FechaCreado = ticket.FechaCreado,
                    Estimacion = ticket.Estimacion,
                    NumeroVerificador = 1,
                    FechaOperacion = DateTime.Now
                };
                db.TicketHistorico.Add(ticketHistorico);

                db.SaveChanges();
                var resp = new
                {
                    success = true,
                    msg = msg
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

        //Funcion para la respuesta de la resolucion de un ticket
        [Authorize(Roles = "CLIENTE")]
        public ActionResult RespuestaResolucion(string cod)
        {
            try
            {
                string codigoResultado = Utiles.DesencriptacionSimetrica(cod);
                //El código llega de esta forma: numeroTicket:ACEPTADO ó numeroTicket:NOACEPTADO

                string[] resultadosCliente = codigoResultado.Split(new char[] { ':' });
                int idTicket = Convert.ToInt32(resultadosCliente[0]);
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("ticket no encontrado");
                }
                if (ticket.SecuencialEstadoTicket != 10)
                {
                    throw new Exception("El ticket no está en estado resuelto");
                }

                Persona personaCliente = ticket.persona_cliente.persona;
                ViewBag.numero = string.Format("{0:000000}", ticket.Secuencial);
                string email = personaCliente.usuario.FirstOrDefault().Email;

                if (email == "soporte@insotec-ec.com")
                {
                    if (!User.Identity.IsAuthenticated)
                    {
                        var returnUrl = "respuesta-resolucion" + cod;
                        return RedirectToAction("Index", "Home", new { ReturnUrl = returnUrl });
                    }
                }

                ViewBag.asunto = ticket.Asunto;
                if (resultadosCliente[1] == "ACEPTADO")
                {
                    string emailCliente = email;
                    ViewBag.ingresado = personaCliente.Nombre1 + " " + personaCliente.Apellido1;
                    ViewBag.fechaIngreso = ticket.FechaCreado.ToString("dd/MM/yyyy");

                    //Cambiando el estado del ticket
                    ticket.SecuencialEstadoTicket = 14;//EL TICKET ESTA CERRADO
                    ticket.SecuencialProximaActividad = 18;//NA porque está cerrado

                    //Adicionando el histórico del ticket                                
                    Usuario user = db.Usuario.FirstOrDefault(x => x.Email == emailCliente);
                    int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                    TicketHistorico ticketHistorico = new TicketHistorico
                    {
                        ticket = ticket,
                        Version = numeroVersion,
                        SecuencialEstadoTicket = ticket.SecuencialEstadoTicket,//  ("EL TICKET ESTA CERRADO")
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

                    db.SaveChanges();//Salvando los cambios

                    string textoEmail = "<div class=\"textoCuerpo\">" + @"Por medio de esta comunicación le informamos que el ticket <b>" + string.Format("{0:000000}", ticket.Secuencial) + @"</b> ha sido CERRADO; <br/> 
                                     esta acción se ha tomado porque hemos recibido su conformidad con el mismo o en su defecto no se recibió una respuesta de su parte dentro del período de espera de los cinco días laborables contados desde la fecha en la que enviamos nuestra solicitud de certificación de conformidad de este requerimiento vía correo electrónico.<br/>
                                     En el caso de requerir correcciones, solicitamos se ingrese un nuevo requerimiento y se incluya el código " + "\"HESO " + string.Format("{0:000000}", ticket.Secuencial) + "\" en el detalle del mismo.<br/> <b>Asunto del ticket: </b>" + ticket.Asunto + "</div>";

                    List<string> correosDestinos = Utiles.CorreoPorGrupoEmail("COORD");
                    correosDestinos.Insert(0, emailCliente);

                    //Borrar aqui
                    var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                    foreach (var g in gestores)
                    {
                        correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                    }
                    correosDestinos = correosDestinos.Distinct().ToList();

                    string codigoCliente = ticket.persona_cliente.cliente.Codigo;
                    Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, codigoCliente + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket cerrado (" + ticket.Asunto + ")", null, String.Format("{0:000000}", ticket.Secuencial));

                    //adicionando el email a los historicos
                    string destinos = String.Join(", ", correosDestinos.ToArray());
                    string textoHistoricoCorreo = "<b>Correo de información, Ticket Cerrado</b><br/>";
                    textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                    textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket cerrado" + "<br/>";
                    textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
                    HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
                    {
                        SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
                        VersionTicketHistorico = ticketHistorico.Version,
                        Fecha = DateTime.Now,
                        Texto = textoHistoricoCorreo
                    };
                    db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
                    db.SaveChanges();
                }
                else
                {
                    ViewBag.estimadoa = "Estimado";
                    if (personaCliente.Sexo == "F")
                    {
                        ViewBag.estimadoa = "Estimada";
                    }
                    ViewBag.nombre = personaCliente.Nombre1 + " " + personaCliente.Apellido1;
                    ViewBag.codTicket = cod;

                    return View("TicketDevuelto");
                }

                return View();
            }
            catch (Exception e)
            {
                string mensaje = e.Message;
                return RedirectToAction("Error", "Home", new RouteValueDictionary {
                    {"mensaje", mensaje}
                });
            }
        }

        public ActionResult EnviarEmailTicketDevuelto(string cod, string detalle, HttpPostedFileBase[] adjuntos = null)
        {
            try
            {
                string codigoResultado = Utiles.DesencriptacionSimetrica(cod);

                string[] resultadosCliente = codigoResultado.Split(new char[] { ':' });
                int idTicket = Convert.ToInt32(resultadosCliente[0]);
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("ticket no encontrado");
                }
                if (ticket.SecuencialEstadoTicket != 10)
                {
                    throw new Exception("El ticket no está en estado resuelto");
                }

                Persona personaCliente = ticket.persona_cliente.persona;
                Usuario usuarioCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault();

                if (personaCliente == null || usuarioCliente == null)
                {
                    throw new Exception("Error encontrando los datos del cliente");
                }

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 18;//EL TICKET ESTA DEVUELTO
                ticket.SecuencialProximaActividad = 15;//AUDITAR

                //Adicionando el histórico del ticket                                                        
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = 18,//  ("EL TICKET ESTA DEVUELTO")
                    SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
                    SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
                    SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
                    SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
                    SecuencialProximaActividad = ticket.SecuencialProximaActividad,//AUDITAR
                    usuario = usuarioCliente,
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

                db.SaveChanges();//Salvando los cambios

                //Por los ficheros adjuntos
                List<string> listaFicheros = new List<string>();
                List<string> listaPathFicheros = new List<string>();
                if (adjuntos != null)
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string extFile = Path.GetExtension(adj.FileName);
                            string newNameFile = System.IO.Path.GetRandomFileName() + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                            adj.SaveAs(path);

                            listaPathFicheros.Add(path);
                            listaFicheros.Add("/resources/tickets/" + newNameFile);
                        }
                    }

                //Enviando los correos electrónicos
                string textoEmail = "<div class='textoCuerpo'>";
                textoEmail += "El presente correo electrónico es sobre la <b>No Aceptación</b> del ticket: " + string.Format("{0:000000}", ticket.Secuencial) + "<br/>";
                textoEmail += "Cliente: " + personaCliente.Nombre1 + " " + personaCliente.Apellido1 + "<br/>";
                textoEmail += "Detalle:<br> " + detalle;

                List<string> correosDestinos = new List<string>();
                string emailCliente = personaCliente.usuario.FirstOrDefault().Email;
                correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));
                correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("JEDES"));
                correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("AUDIT"));

                //Buscando los desarrolladores del ticket
                var correosColaboradores = (from c in db.Colaborador
                                            join
                                                p in db.Persona on c.SecuencialPersona equals p.Secuencial
                                            join
                                                u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                            join
                                                t in db.Tarea on c.Secuencial equals t.SecuencialColaborador
                                            join
                                                tt in db.TicketTarea on t.Secuencial equals tt.SecuencialTarea
                                            where tt.SecuencialTicket == ticket.Secuencial
                                            select u.Email).Distinct().ToList();
                correosDestinos.AddRange(correosColaboradores);
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                correosDestinos = correosDestinos.Distinct().ToList();

                string asuntoEmail = ticket.persona_cliente.cliente.Codigo + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Devuelto (" + ticket.Asunto + ")";
                Utiles.EnviarEmailSistema(new string[] { emailCliente }, textoEmail, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));
                Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));

                //Adicionando email a los historicos
                correosDestinos.Add(emailCliente);
                string destinos = String.Join(", ", correosDestinos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de información, Ticket Devuelto</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket Devuelto" + "<br/>";
                textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
                HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
                    VersionTicketHistorico = ticketHistorico.Version,
                    Fecha = DateTime.Now,
                    Texto = textoHistoricoCorreo
                };
                db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
                db.SaveChanges();

                //Adicionando los adjuntos
                foreach (string url in listaFicheros)
                {
                    HistoricoAdjunto historicoAdjunto = new HistoricoAdjunto();
                    historicoAdjunto.historicoInformacionTicket = historicoCorreoTicket;
                    historicoAdjunto.Url = url;
                    db.HistoricoAdjunto.Add(historicoAdjunto);
                }
                db.SaveChanges();

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

        [Authorize(Roles = "ADMIN, CLIENTE")]
        public ActionResult EnviarEmailTicketDevueltoCliente(int numeroTicket = 0, string detalle = "", HttpPostedFileBase[] adjuntos = null)
        {
            try
            {
                int idTicket = numeroTicket;
                Ticket ticket = db.Ticket.Find(idTicket);
                if (ticket == null)
                {
                    throw new Exception("ticket no encontrado");
                }
                if (ticket.SecuencialEstadoTicket != 10)
                {
                    throw new Exception("El ticket no está en estado resuelto");
                }

                Persona personaCliente = ticket.persona_cliente.persona;
                Usuario usuarioCliente = ticket.persona_cliente.persona.usuario.FirstOrDefault();

                if (personaCliente == null || usuarioCliente == null)
                {
                    throw new Exception("Error encontrando los datos del cliente");
                }

                //Cambiando el estado del ticket
                ticket.SecuencialEstadoTicket = 18;//EL TICKET ESTA DEVUELTO
                ticket.SecuencialProximaActividad = 15;//AUDITAR

                //Adicionando el histórico del ticket                                                        
                int numeroVersion = db.TicketHistorico.Where(x => x.SecuencialTicket == ticket.Secuencial).Count();

                TicketHistorico ticketHistorico = new TicketHistorico
                {
                    ticket = ticket,
                    Version = numeroVersion,
                    SecuencialEstadoTicket = 18,//  ("EL TICKET ESTA DEVUELTO")
                    SecuencialPersona_Cliente = ticket.SecuencialPersona_Cliente,
                    SecuencialPrioridadTicket = ticket.SecuencialPrioridadTicket,
                    SecuencialCategoriaTicket = ticket.SecuencialCategoriaTicket,
                    SecuencialTipoRecurso = ticket.SecuencialTipoRecurso,
                    SecuencialProximaActividad = ticket.SecuencialProximaActividad,//AUDITAR
                    usuario = usuarioCliente,
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

                db.SaveChanges();//Salvando los cambios

                //Por los ficheros adjuntos
                List<string> listaFicheros = new List<string>();
                List<string> listaPathFicheros = new List<string>();
                if (adjuntos != null)
                    foreach (HttpPostedFileBase adj in adjuntos)
                    {
                        if (adj != null)
                        {
                            string extFile = Path.GetExtension(adj.FileName);
                            string newNameFile = System.IO.Path.GetRandomFileName() + extFile;
                            string path = Path.Combine(Server.MapPath("~/Web/resources/tickets"), newNameFile);
                            adj.SaveAs(path);

                            listaPathFicheros.Add(path);
                            listaFicheros.Add("/resources/tickets/" + newNameFile);
                        }
                    }

                //Enviando los correos electrónicos
                string textoEmail = "<div class='textoCuerpo'>";
                textoEmail += "El presente correo electrónico es sobre la <b>No Aceptación</b> del ticket: " + string.Format("{0:000000}", ticket.Secuencial) + "<br/>";
                textoEmail += "Cliente: " + personaCliente.Nombre1 + " " + personaCliente.Apellido1 + "<br/>";
                textoEmail += "Detalle:<br> " + detalle;

                List<string> correosDestinos = new List<string>();
                string emailCliente = personaCliente.usuario.FirstOrDefault().Email;
                correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("COORD"));
                correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("JEDES"));
                correosDestinos.AddRange(Utiles.CorreoPorGrupoEmail("AUDIT"));

                //Buscando los desarrolladores del ticket
                var correosColaboradores = (from c in db.Colaborador
                                            join
                                                p in db.Persona on c.SecuencialPersona equals p.Secuencial
                                            join
                                                u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                            join
                                                t in db.Tarea on c.Secuencial equals t.SecuencialColaborador
                                            join
                                                tt in db.TicketTarea on t.Secuencial equals tt.SecuencialTarea
                                            where tt.SecuencialTicket == ticket.Secuencial
                                            select u.Email).Distinct().ToList();
                correosDestinos.AddRange(correosColaboradores);
                var gestores = ticket.persona_cliente.cliente.gestorServicios.ToList();
                foreach (var g in gestores)
                {
                    correosDestinos.Add(g.colaborador.persona.usuario.FirstOrDefault().Email);
                }
                correosDestinos = correosDestinos.Distinct().ToList();

                string asuntoEmail = ticket.persona_cliente.cliente.Codigo + " HESO " + string.Format("{0:000000}", ticket.Secuencial) + " - Ticket Devuelto (" + ticket.Asunto + ")";
                Utiles.EnviarEmailSistema(new string[] { emailCliente }, textoEmail, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));
                Utiles.EnviarEmailSistema(correosDestinos.ToArray(), textoEmail, asuntoEmail, listaPathFicheros.ToArray(), string.Format("{0:000000}", ticket.Secuencial));

                //Adicionando email a los historicos
                correosDestinos.Add(emailCliente);
                string destinos = String.Join(", ", correosDestinos.ToArray());
                string textoHistoricoCorreo = "<b>Correo de información, Ticket Devuelto</b><br/>";
                textoHistoricoCorreo += "<b>Destinos:</b> " + destinos + "<br/>";
                textoHistoricoCorreo += "<b>Asunto:</b> " + "Ticket Devuelto" + "<br/>";
                textoHistoricoCorreo += "<b>Texto del correo:</b> <br/>" + textoEmail;
                HistoricoInformacionTicket historicoCorreoTicket = new HistoricoInformacionTicket
                {
                    SecuencialTicketHistorico = ticketHistorico.SecuencialTicket,
                    VersionTicketHistorico = ticketHistorico.Version,
                    Fecha = DateTime.Now,
                    Texto = textoHistoricoCorreo
                };
                db.HistoricoInformacionTicket.Add(historicoCorreoTicket);
                db.SaveChanges();

                //Adicionando los adjuntos
                foreach (string url in listaFicheros)
                {
                    HistoricoAdjunto historicoAdjunto = new HistoricoAdjunto();
                    historicoAdjunto.historicoInformacionTicket = historicoCorreoTicket;
                    historicoAdjunto.Url = url;
                    db.HistoricoAdjunto.Add(historicoAdjunto);
                }
                db.SaveChanges();

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
    }
}
