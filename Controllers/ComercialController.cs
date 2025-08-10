using SifizPlanning.Models;
using SifizPlanning.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SifizPlanning.Util;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Data.Entity.Validation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using Microsoft.Ajax.Utilities;

namespace SifizPlanning.Controllers
{
    public class ComercialController : Controller
    {
        SifizPlanningEntidades db = DbCnx.getCnx();

        // GET: Comercial
        //PANTALLA INICIAL
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult Index()
        {
            return View();
        }

        //INCIDENCIAS DE LOS USUARIOS
        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult RequerimientosComercial(int start, int lenght, string filtro = "")
        {

            try
            {
                var requerimientosComercial = (from or in db.OFERTAREQUERIMIENTO
                                               join r in db.Requerimiento on or.SecuencialRequerimiento equals r.Secuencial
                                               join cli in db.Cliente on or.SecuencialCLiente equals cli.Secuencial
                                               select new
                                               {
                                                   secuencial = or.Secuencial,
                                                   cliente = cli.Descripcion,
                                                   clienteId = cli.Secuencial,
                                                   ticket = or.SecuencialTicketTarea,
                                                   detalle = or.Detalle,
                                                   requerimientoId = or.SecuencialRequerimiento,
                                                   requerimiento = r.Descripcion,
                                                   fecha = or.FechaPedidoCLiente.HasValue ? or.FechaPedidoCLiente.Value.ToString() : "",
                                               }).ToList();


                if (filtro != "")
                {
                    requerimientosComercial = requerimientosComercial.Where(x =>
                                            x.cliente.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.ticket.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.detalle.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.requerimiento.ToString().ToLower().Contains(filtro.ToLower())
                                          ).ToList();
                }

                int total = requerimientosComercial.Count();
                requerimientosComercial = requerimientosComercial.Skip(start).Take(lenght).ToList();

                var result = new
                {
                    success = true,
                    total = total,
                    requerimientos = requerimientosComercial
                };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(result);
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult DarDatosRequerimiento(int secuencialRequerimiento)
        {
            try
            {
                OfertaRequerimiento o = db.OFERTAREQUERIMIENTO.Find(secuencialRequerimiento);
                if (o == null)
                {
                    throw new Exception("No se encuentra el requerimiento en el sistema");
                }

                var ofr = (from or in db.OFERTAREQUERIMIENTO
                           join r in db.Requerimiento on or.SecuencialRequerimiento equals r.Secuencial
                           join cli in db.Cliente on or.SecuencialCLiente equals cli.Secuencial
                           where or.Secuencial == secuencialRequerimiento
                           select new
                           {
                               secuencial = or.Secuencial,
                               cliente = cli.Descripcion,
                               clienteId = cli.Secuencial,
                               ticket = or.SecuencialTicketTarea,
                               detalle = or.Detalle,
                               requerimientoId = or.SecuencialRequerimiento,
                               requerimiento = r.Descripcion,
                               fecha = or.FechaPedidoCLiente.HasValue ? or.FechaPedidoCLiente : null,
                           }).FirstOrDefault();

                var result = new
                {
                    success = true,
                    requerimientoResult = ofr
                };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(result);
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult GuardarRequerimiento(int cliente, int requerimiento, string ticket, string detalle, string fechaPedidoCliente)
        {
            try
            {
                OfertaRequerimiento nuevaOfertaRequerimiento = new OfertaRequerimiento();

                if (ticket != "")
                {
                    int ticketNumerico;
                    if (!int.TryParse(ticket, out ticketNumerico))
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "El ticket debe ser un valor numérico válido."
                        });
                    }

                    Ticket t = db.Ticket.Find(ticketNumerico);

                    if (t != null)
                    {
                        nuevaOfertaRequerimiento.SecuencialCLiente = cliente;
                        nuevaOfertaRequerimiento.SecuencialRequerimiento = requerimiento;
                        nuevaOfertaRequerimiento.SecuencialTicketTarea = ticketNumerico;
                            if (t.Detalle != null)
                        {
                            nuevaOfertaRequerimiento.Detalle = t.Detalle.Length > 100
                                ? t.Detalle.Substring(0, 100)
                                : t.Detalle;
                        }
                        else
                        {
                            nuevaOfertaRequerimiento.Detalle = null; }
                        //nuevaOfertaRequerimiento.Detalle = t.Detalle.Substring(0, 100);
                        nuevaOfertaRequerimiento.FechaPedidoCLiente = t.FechaCreado;
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "No se encuentra el ticket en la base de datos. Rectifique."
                        });
                    }
                }
                else
                {
                    string[] fechaI = fechaPedidoCliente.Split(new Char[] { '/' });
                    int dia = Int32.Parse(fechaI[0]);
                    int mes = Int32.Parse(fechaI[1]);
                    int anno = Int32.Parse(fechaI[2]);
                    DateTime fechaPC = new DateTime(anno, mes, dia);

                    nuevaOfertaRequerimiento.SecuencialCLiente = cliente;
                    nuevaOfertaRequerimiento.SecuencialRequerimiento = requerimiento;
                    nuevaOfertaRequerimiento.Detalle = detalle;
                    nuevaOfertaRequerimiento.FechaPedidoCLiente = fechaPC;
                }


                db.OFERTAREQUERIMIENTO.Add(nuevaOfertaRequerimiento);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg = "Se ha realizado la operación correctamente."
                });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";

                return Json(new
                {
                    success = false,
                    msg = exceptionMessage
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

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult EditarRequerimiento(int id, int cliente, int requerimiento, string ticket, string detalle, string fechaPedidoCliente)
        {
            try
            {
                var nuevaOfertaRequerimiento = db.OFERTAREQUERIMIENTO.FirstOrDefault(s => s.Secuencial == id);

                if (nuevaOfertaRequerimiento == null)
                {
                    return Json(new
                    {
                        success = false,
                        msg = "El recurso especificado no existe."
                    });
                }

                int ticketNumerico;
                if (ticket != "")
                {
                    if (!int.TryParse(ticket, out ticketNumerico))
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "El ticket debe ser un valor numérico válido."
                        });
                    }
                    else
                    {
                        nuevaOfertaRequerimiento.SecuencialTicketTarea = ticketNumerico;
                    }
                }
                else
                {
                    nuevaOfertaRequerimiento.SecuencialTicketTarea = null;
                }

                string[] fechaI = fechaPedidoCliente.Split(new Char[] { '/' });
                int dia = Int32.Parse(fechaI[0]);
                int mes = Int32.Parse(fechaI[1]);
                int anno = Int32.Parse(fechaI[2]);
                DateTime fechaPC = new DateTime(anno, mes, dia);

                nuevaOfertaRequerimiento.SecuencialCLiente = cliente;
                nuevaOfertaRequerimiento.SecuencialRequerimiento = requerimiento;
                nuevaOfertaRequerimiento.Detalle = detalle;
                nuevaOfertaRequerimiento.FechaPedidoCLiente = fechaPC;

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg = "Se ha realizado la operación correctamente."
                });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";

                return Json(new
                {
                    success = false,
                    msg = exceptionMessage
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

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult EliminarRequerimiento(int id)
        {
            try
            {
                var ofertaRequerimiento = db.OFERTAREQUERIMIENTO.Find(id);

                if (ofertaRequerimiento == null)
                {
                    return Json(new
                    {
                        success = false,
                        msg = "El recurso especificado no existe."
                    });
                }

                db.OFERTAREQUERIMIENTO.Remove(ofertaRequerimiento);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg = "Se ha realizado la operación correctamente."
                });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";

                return Json(new
                {
                    success = false,
                    msg = exceptionMessage
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

        
        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult DetalleOferta(int id)
        {
            try
            {
                // Buscar oferta real
                var oferta = (from or in db.OfertaOferta
                              join r in db.OFERTAREQUERIMIENTO on or.SecuencialOfertaRequerimiento equals r.Secuencial
                              join cli in db.Cliente on r.SecuencialCLiente equals cli.Secuencial
                              join ti in db.Ticket on r.SecuencialTicketTarea equals ti.Secuencial
                              where or.Secuencial == id
                              select new
                              {
                                  secuencial = or.Secuencial,
                                  codigo = or.Codigo,
                                  precio = or.Precio,
                                  formaPago = or.FormaPago,
                                  descuento = (or.Descuento.HasValue && or.Descuento.Value) ? "SI" : "NO",
                                  estado = or.Estado,
                                  tipo = r.Secuencial + "-" + (r.Detalle.Length > 30 ? r.Detalle.Substring(0, 30) : r.Detalle),
                                  tema = or.OfertaRequerimiento != null ? or.OfertaRequerimiento.Detalle : "",
                                  cliente = cli.Descripcion,
                                  ticket = r.SecuencialTicketTarea,
                                  detalleRequerimiento = r.Detalle,
                                  // Fechas desde TicketHistorico
                                  fechaEstimacion = db.TicketHistorico.Where(h => h.SecuencialTicket == r.SecuencialTicketTarea && h.SecuencialProximaActividad == 3).OrderByDescending(h => h.Version).Select(h => (DateTime?)h.FechaOperacion).FirstOrDefault(),
                                  fechaRevision = db.TicketHistorico.Where(h => h.SecuencialTicket == r.SecuencialTicketTarea && h.SecuencialProximaActividad == 7).OrderByDescending(h => h.Version).Select(h => (DateTime?)h.FechaOperacion).FirstOrDefault(),
                                  fechaAprobacionGerencia = db.TicketHistorico.Where(h => h.SecuencialTicket == r.SecuencialTicketTarea && h.SecuencialProximaActividad == 30).OrderByDescending(h => h.Version).Select(h => (DateTime?)h.FechaOperacion).FirstOrDefault(),
                                  fechaEnvioOferta = db.TicketHistorico.Where(h => h.SecuencialTicket == r.SecuencialTicketTarea && h.SecuencialProximaActividad == 30).OrderByDescending(h => h.Version).Select(h => (DateTime?)h.FechaOperacion).FirstOrDefault(),
                                  fechaGeneracion = or.FechaGeneracionOferta,
                                  fechaVencimiento = or.FechaVencimientoOferta
                              }).FirstOrDefault();
                if (oferta != null)
                    return Json(new { success = true, detalle = oferta });

                // Si no es oferta, buscar como ticket pendiente
                var t = db.Ticket.FirstOrDefault(x => x.Secuencial == id);
                if (t != null)
                {
                    var estadosExcluidos = new[] { "ANULADO", "CERRADO", "DEVUELTO", "RECHAZADO", "SUSPENDIDO", "INCONCLUSO" };
                    if (t.estadoTicket != null && !estadosExcluidos.Contains(t.estadoTicket.Codigo) && t.proximaActividad.Codigo == "COTIZAR")
                    {
                        DateTime? fechaRecepcionEstimacion = db.TicketHistorico
                            .Where(h => h.SecuencialTicket == t.Secuencial && h.SecuencialProximaActividad == 3)
                            .OrderByDescending(h => h.Version)
                            .Select(h => (DateTime?)h.FechaOperacion)
                            .FirstOrDefault();
                        DateTime? fechaEnvioRevision = db.TicketHistorico
                            .Where(h => h.SecuencialTicket == t.Secuencial && h.SecuencialProximaActividad == 7)
                            .OrderByDescending(h => h.Version)
                            .Select(h => (DateTime?)h.FechaOperacion)
                            .FirstOrDefault();
                        DateTime? fechaAprobacionGerencia = db.TicketHistorico
                            .Where(h => h.SecuencialTicket == t.Secuencial && h.SecuencialProximaActividad == 30)
                            .OrderByDescending(h => h.Version)
                            .Select(h => (DateTime?)h.FechaOperacion)
                            .FirstOrDefault();
                        DateTime? fechaEnvioOferta = fechaAprobacionGerencia;
                        string proximaActividad = null;
                        var historico = db.TicketHistorico
                            .Where(h => h.SecuencialTicket == t.Secuencial)
                            .OrderByDescending(h => h.Version)
                            .FirstOrDefault();
                        if (historico != null && historico.proximaActividad != null)
                        {
                            proximaActividad = historico.proximaActividad.Descripcion;
                        }
                        var detalleTicket = new
                        {
                            secuencial = 0,
                            codigo = (string)null,
                            precio = (decimal?)null,
                            formaPago = (string)null,
                            descuento = "NO",
                            estado = t.estadoTicket.Codigo,
                            tipo = "TICKET",
                            tema = t.Asunto,
                            cliente = t.persona_cliente != null && t.persona_cliente.cliente != null ? t.persona_cliente.cliente.Descripcion : "",
                            ticket = t.Secuencial,
                            detalleRequerimiento = t.Asunto,
                            fechaEstimacion = fechaRecepcionEstimacion,
                            fechaRevision = fechaEnvioRevision,
                            fechaAprobacionGerencia = fechaAprobacionGerencia,
                            fechaEnvioOferta = fechaEnvioOferta,
                            fechaGeneracion = (DateTime?)null,
                            fechaVencimiento = (DateTime?)null,
                            proximaActividad = proximaActividad
                        };
                        return Json(new { success = true, detalle = detalleTicket });
                    }
                }
                return Json(new { success = false, msg = "No se encontró la oferta ni ticket pendiente." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult DarCatalogoRequerimientos()
        {
            try
            {
                var requerimientos = (from r in db.Requerimiento
                                      select new
                                      {
                                          id = r.Secuencial,
                                          descripcion = r.Descripcion
                                      }).ToList();

                var result = new
                {
                    success = true,
                    requerimientos = requerimientos
                };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(result);
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult OfertasComercial(int start, int lenght, string filtro = "", string filtrosColumna = null)
        {
            try
            {
                // Deserializar filtros de columna si vienen
                var filtros = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(filtrosColumna))
                {
                    filtros = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(filtrosColumna);
                }

                var estadosExcluidos = new[] { "ANULADO", "CERRADO", "DEVUELTO", "RECHAZADO", "SUSPENDIDO", "INCONCLUSO" };
                var ofertasComercial = (from or in db.OfertaOferta
                                        join r in db.OFERTAREQUERIMIENTO on or.SecuencialOfertaRequerimiento equals r.Secuencial
                                        join cli in db.Cliente on r.SecuencialCLiente equals cli.Secuencial
                                        join t in db.Ticket on r.SecuencialTicketTarea equals t.Secuencial
                                        select new
                                        {
                                            secuencial = or.Secuencial,
                                            codigo = or.Codigo,
                                            requerimientoTicket = r.SecuencialTicketTarea,
                                            secuencialRequerimiento = r.Secuencial, // <-- AGREGADO
                                            fechaGeneracion = or.FechaGeneracionOferta,
                                            fechaVencimiento = or.FechaVencimientoOferta,
                                            tipo = r.Secuencial + "-" + (r.Detalle.Length > 30 ? r.Detalle.Substring(0, 30) : r.Detalle),
                                            idTipo = r.Secuencial,
                                            precio = or.Precio,
                                            formaPago = or.FormaPago,
                                            descuento = (or.Descuento.HasValue && or.Descuento.Value) ? "SI" : "NO",
                                            estado = or.Estado,
                                            tema = or.OfertaRequerimiento != null ? or.OfertaRequerimiento.Detalle : "",
                                            cliente = cli.Descripcion,
                                            ticket = r.SecuencialTicketTarea
                                        }).ToList();

                // Poblar fechas y próxima actividad desde TicketHistorico para ofertas
                var ofertasConFechas = ofertasComercial.Select(oferta =>
                {
                    DateTime? fechaRecepcionEstimacion = null;
                    DateTime? fechaEnvioRevision = null;
                    DateTime? fechaAprobacionGerencia = null;
                    DateTime? fechaEnvioOferta = null;
                    string proximaActividad = null;
                    if (oferta.requerimientoTicket != null)
                    {
                        int idTicket = oferta.requerimientoTicket.Value;
                        fechaRecepcionEstimacion = db.TicketHistorico
                            .Where(h => h.SecuencialTicket == idTicket && h.SecuencialProximaActividad == 3)
                            .OrderByDescending(h => h.Version)
                            .Select(h => (DateTime?)h.FechaOperacion)
                            .FirstOrDefault();
                        fechaEnvioRevision = db.TicketHistorico
                            .Where(h => h.SecuencialTicket == idTicket && h.SecuencialProximaActividad == 7)
                            .OrderByDescending(h => h.Version)
                            .Select(h => (DateTime?)h.FechaOperacion)
                            .FirstOrDefault();
                        fechaAprobacionGerencia = db.TicketHistorico
                            .Where(h => h.SecuencialTicket == idTicket && h.SecuencialProximaActividad == 30)
                            .OrderByDescending(h => h.Version)
                            .Select(h => (DateTime?)h.FechaOperacion)
                            .FirstOrDefault();
                        fechaEnvioOferta = fechaAprobacionGerencia;
                        // Obtener la última actividad del ticket
                        var historico = db.TicketHistorico
                            .Where(h => h.SecuencialTicket == idTicket)
                            .OrderByDescending(h => h.Version)
                            .FirstOrDefault();
                        if (historico != null && historico.proximaActividad != null)
                        {
                            proximaActividad = historico.proximaActividad.Descripcion;
                        }
                    }
                    return new
                    {
                        oferta.secuencial,
                        oferta.codigo,
                        fechaEstimacion = fechaRecepcionEstimacion,
                        fechaRevision = fechaEnvioRevision,
                        fechaAprobacionGerencia = fechaAprobacionGerencia,
                        fechaEnvioOferta = fechaEnvioOferta,
                        oferta.fechaGeneracion,
                        oferta.fechaVencimiento,
                        oferta.tipo,
                        oferta.idTipo,
                        oferta.precio,
                        oferta.formaPago,
                        oferta.descuento,
                        oferta.estado,
                        oferta.tema,
                        oferta.cliente,
                        oferta.ticket,
                        oferta.secuencialRequerimiento, // <-- PROPAGADO
                        proximaActividad // nueva columna
                    };
                }).ToList();

                // 2. Tickets pendientes de oferta
                var ticketsConOferta = db.OFERTAREQUERIMIENTO
                    .Where(orq => db.OfertaOferta.Any(of => of.SecuencialOfertaRequerimiento == orq.Secuencial))
                    .Select(orq => orq.SecuencialTicketTarea)
                    .ToList();

                var ticketsPendientes = (from t in db.Ticket
                                         where t.estadoTicket != null && !estadosExcluidos.Contains(t.estadoTicket.Codigo)
                                         && !ticketsConOferta.Contains(t.Secuencial)
                                         && (t.proximaActividad.Codigo == "COTIZAR")
                                         select new
                                         {
                                             secuencial = 0, // No hay oferta aún
                                             codigo = (string)null,
                                             requerimientoTicket = t.Secuencial,
                                             fechaGeneracion = (DateTime?)null,
                                             fechaVencimiento = (DateTime?)null,
                                             tipo = "TICKET",
                                             idTipo = 0,
                                             precio = (decimal?)null,
                                             formaPago = (string)null,
                                             descuento = "NO",
                                             estado = t.estadoTicket.Codigo,
                                             tema = t.Asunto,
                                             cliente = t.persona_cliente != null && t.persona_cliente.cliente != null ? t.persona_cliente.cliente.Descripcion : "",
                                             ticket = t.Secuencial,
                                             secuencialRequerimiento = 0 // <-- AGREGADO para igualar estructura
                                         }).ToList();

                // Tickets pendientes: poblar fechas desde TicketHistorico
                var ticketsConFechas = ticketsPendientes.Select(t =>
                {
                    DateTime? fechaRecepcionEstimacion = db.TicketHistorico
                        .Where(h => h.SecuencialTicket == t.ticket && h.SecuencialProximaActividad == 3)
                        .OrderByDescending(h => h.Version)
                        .Select(h => (DateTime?)h.FechaOperacion)
                        .FirstOrDefault();
                    DateTime? fechaEnvioRevision = db.TicketHistorico
                        .Where(h => h.SecuencialTicket == t.ticket && h.SecuencialProximaActividad == 7)
                        .OrderByDescending(h => h.Version)
                        .Select(h => (DateTime?)h.FechaOperacion)
                        .FirstOrDefault();
                    DateTime? fechaAprobacionGerencia = db.TicketHistorico
                        .Where(h => h.SecuencialTicket == t.ticket && h.SecuencialProximaActividad == 30)
                        .OrderByDescending(h => h.Version)
                        .Select(h => (DateTime?)h.FechaOperacion)
                        .FirstOrDefault();
                    DateTime? fechaEnvioOferta = fechaAprobacionGerencia;
                    string proximaActividad = null;
                    var historico = db.TicketHistorico
                        .Where(h => h.SecuencialTicket == t.ticket)
                        .OrderByDescending(h => h.Version)
                        .FirstOrDefault();
                    if (historico != null && historico.proximaActividad != null)
                    {
                        proximaActividad = historico.proximaActividad.Descripcion;
                    }
                    return new
                    {
                        t.secuencial,
                        t.codigo,
                        fechaEstimacion = fechaRecepcionEstimacion,
                        fechaRevision = fechaEnvioRevision,
                        fechaAprobacionGerencia = fechaAprobacionGerencia,
                        fechaEnvioOferta = fechaEnvioOferta,
                        t.fechaGeneracion,
                        t.fechaVencimiento,
                        t.tipo,
                        t.idTipo,
                        t.precio,
                        t.formaPago,
                        t.descuento,
                        t.estado,
                        t.tema,
                        t.cliente,
                        ticket = (int?)t.ticket,
                        secuencialRequerimiento = t.secuencialRequerimiento, // <-- ahora existe y es int
                        proximaActividad
                    };
                }).ToList();

                // Unir ambos listados
                var listadoSinInconclusos = ofertasConFechas.Concat(ticketsConFechas).ToList();

                // Filtro global
                if (!string.IsNullOrEmpty(filtro))
                {
                    listadoSinInconclusos = listadoSinInconclusos.Where(x =>
                        (x.codigo != null && x.codigo.ToLower().Contains(filtro.ToLower())) ||
                        (x.cliente != null && x.cliente.ToLower().Contains(filtro.ToLower())) ||
                        (x.ticket != null && x.ticket.ToString().Contains(filtro)) ||
                        (x.tema != null && x.tema.ToLower().Contains(filtro.ToLower()))
                    ).ToList();
                }
                // Filtros por columna
                if (filtros != null && filtros.Count > 0)
                {
                    if (filtros.ContainsKey("codigoOferta") && !string.IsNullOrEmpty(filtros["codigoOferta"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.codigo != null && x.codigo.ToLower().Contains(filtros["codigoOferta"].ToLower())).ToList();
                    if (filtros.ContainsKey("ticket") && !string.IsNullOrEmpty(filtros["ticket"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.ticket != null && x.ticket.ToString().Contains(filtros["ticket"])).ToList();
                    if (filtros.ContainsKey("cliente") && !string.IsNullOrEmpty(filtros["cliente"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.cliente != null && x.cliente.ToLower().Contains(filtros["cliente"].ToLower())).ToList();
                    if (filtros.ContainsKey("tema") && !string.IsNullOrEmpty(filtros["tema"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.tema != null && x.tema.ToLower().Contains(filtros["tema"].ToLower())).ToList();
                    if (filtros.ContainsKey("fechaEstimacion") && !string.IsNullOrEmpty(filtros["fechaEstimacion"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.fechaEstimacion != null && x.fechaEstimacion.Value.ToString("dd/MM/yyyy").Contains(filtros["fechaEstimacion"])).ToList();
                    if (filtros.ContainsKey("fechaGeneracion") && !string.IsNullOrEmpty(filtros["fechaGeneracion"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.fechaGeneracion != null && x.fechaGeneracion.Value.ToString("dd/MM/yyyy").Contains(filtros["fechaGeneracion"])).ToList();
                    if (filtros.ContainsKey("fechaRevision") && !string.IsNullOrEmpty(filtros["fechaRevision"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.fechaRevision != null && x.fechaRevision.Value.ToString("dd/MM/yyyy").Contains(filtros["fechaRevision"])).ToList();
                    if (filtros.ContainsKey("fechaAprobacionGerencia") && !string.IsNullOrEmpty(filtros["fechaAprobacionGerencia"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.fechaAprobacionGerencia != null && x.fechaAprobacionGerencia.Value.ToString("dd/MM/yyyy").Contains(filtros["fechaAprobacionGerencia"])).ToList();
                    if (filtros.ContainsKey("fechaEnvioOferta") && !string.IsNullOrEmpty(filtros["fechaEnvioOferta"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.fechaEnvioOferta != null && x.fechaEnvioOferta.Value.ToString("dd/MM/yyyy").Contains(filtros["fechaEnvioOferta"])).ToList();
                    if (filtros.ContainsKey("fechaVencimiento") && !string.IsNullOrEmpty(filtros["fechaVencimiento"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.fechaVencimiento != null && x.fechaVencimiento.Value.ToString("dd/MM/yyyy").Contains(filtros["fechaVencimiento"])).ToList();
                    if (filtros.ContainsKey("tipo") && !string.IsNullOrEmpty(filtros["tipo"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.tipo != null && x.tipo.ToLower().Contains(filtros["tipo"].ToLower())).ToList();
                    if (filtros.ContainsKey("precioOferta") && !string.IsNullOrEmpty(filtros["precioOferta"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.precio != null && x.precio.ToString().Contains(filtros["precioOferta"])).ToList();
                    if (filtros.ContainsKey("formaPago") && !string.IsNullOrEmpty(filtros["formaPago"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.formaPago != null && x.formaPago.ToLower().Contains(filtros["formaPago"].ToLower())).ToList();
                    if (filtros.ContainsKey("validezOferta") && !string.IsNullOrEmpty(filtros["validezOferta"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.fechaVencimiento != null && x.fechaVencimiento.Value.ToString("dd/MM/yyyy").Contains(filtros["validezOferta"])).ToList();
                    if (filtros.ContainsKey("descuento") && !string.IsNullOrEmpty(filtros["descuento"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.descuento != null && x.descuento.ToString().ToLower().Contains(filtros["descuento"].ToLower())).ToList();
                    if (filtros.ContainsKey("estado") && !string.IsNullOrEmpty(filtros["estado"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.estado != null && x.estado.ToLower().Contains(filtros["estado"].ToLower())).ToList();
                    if (filtros.ContainsKey("proximaActividad") && !string.IsNullOrEmpty(filtros["proximaActividad"]))
                        listadoSinInconclusos = listadoSinInconclusos.Where(x => x.proximaActividad != null && x.proximaActividad.ToLower().Contains(filtros["proximaActividad"].ToLower())).ToList();
                }
                int total = listadoSinInconclusos.Count();
                var paged = listadoSinInconclusos.Skip(start).Take(lenght).ToList();
                var result = new
                {
                    success = true,
                    total = total,
                    ofertasComercial = paged
                };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(result);
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult DarDatosOferta(string codigo)
        {
            try
            {
                OfertaOferta o = db.OfertaOferta.Where(d => d.Codigo.ToString() == codigo).FirstOrDefault();
                if (o == null)
                {
                    throw new Exception("No se encuentra la oferta en el sistema");
                }

                var ofr = (from or in db.OfertaOferta
                           join r in db.OFERTAREQUERIMIENTO on or.SecuencialOfertaRequerimiento equals r.Secuencial
                           where or.Secuencial == o.Secuencial
                           select new
                           {
                               secuencial = or.Secuencial,
                               codigo = or.Codigo,
                               fechaEstimacion = or.FechaRecepcionEstimacion.HasValue ? or.FechaRecepcionEstimacion : null,
                               fechaRevision = or.FechaEnvioRevision.HasValue ? or.FechaEnvioRevision : null,
                               fechaEnvioOferta = or.FechaEnvioOfertaCliente.HasValue ? or.FechaEnvioOfertaCliente : null,
                               fechaGeneracion = or.FechaGeneracionOferta.HasValue ? or.FechaGeneracionOferta : null,
                               fechaAprobacionGerencia = or.FechaAprobacionOfertaGerencia.HasValue ? or.FechaAprobacionOfertaGerencia : null,
                               fechaVencimiento = or.FechaVencimientoOferta.HasValue ? or.FechaVencimientoOferta : null,
                               OfertaRequerimiento = r.Secuencial
                           }).FirstOrDefault();

                var result = new
                {
                    success = true,
                    oferta = ofr
                };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(result);
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult GuardarOferta(string OfertaRequerimiento, string codigo, string fechaEstimacion, string fechaRevision, string fechaEnvioOferta,
            string fechaGeneracion, string fechaAprobacionGerencia, string fechaVencimiento, decimal? precio, string formaPago, string descuento, string estado, string tipo, string tema)
        {
            try
            {
                int idRequerimiento;
                bool esPedidoRequerimiento = false;
                if (!int.TryParse(OfertaRequerimiento, out idRequerimiento))
                {
                    return Json(new { success = false, msg = "Identificador de requerimiento inválido." });
                }

                // Verificar si el idRequerimiento existe en OFERTAREQUERIMIENTO
                var requerimientoExistente = db.OFERTAREQUERIMIENTO.Find(idRequerimiento);
                if (requerimientoExistente == null)
                {
                    // No existe, entonces es un pedido/requerimiento que hay que crear
                    esPedidoRequerimiento = true;
                }

                if (esPedidoRequerimiento)
                {
                    // Crear el requerimiento nuevo basado en el pedido/requerimiento recibido
                    // Para esto, se necesita obtener datos del pedido/requerimiento, que no están en los parámetros
                    // Asumiremos que el campo 'tipo' contiene el tipo de requerimiento a crear
                    // Y que el 'codigo' contiene el número de ticket pendiente para obtener datos
                    int numeroTicket;
                    if (!int.TryParse(codigo, out numeroTicket))
                    {
                        return Json(new { success = false, msg = "Número de ticket inválido para crear requerimiento." });
                    }

                    Ticket ticket = db.Ticket.Find(numeroTicket);
                    if (ticket == null)
                    {
                        return Json(new { success = false, msg = "No se encontró el ticket para crear el requerimiento." });
                    }

                    OfertaRequerimiento nuevoRequerimiento = new OfertaRequerimiento();
                    nuevoRequerimiento.SecuencialCLiente = ticket.persona_cliente.SecuencialCliente;
                    nuevoRequerimiento.SecuencialRequerimiento = idRequerimiento; // Aquí se usa el id recibido como tipo
                    nuevoRequerimiento.SecuencialTicketTarea = numeroTicket;
                    nuevoRequerimiento.Detalle = ticket.Detalle.Length > 250 ? ticket.Detalle.Substring(0, 250) : ticket.Detalle;
                    nuevoRequerimiento.FechaPedidoCLiente = ticket.FechaCreado;

                    db.OFERTAREQUERIMIENTO.Add(nuevoRequerimiento);
                    db.SaveChanges();

                    idRequerimiento = nuevoRequerimiento.Secuencial;
                }

                OfertaOferta nuevaOferta = new OfertaOferta();
                nuevaOferta.Codigo = codigo;
                DateTime? ConvertirFecha(string fecha)
                {
                    if (DateTime.TryParse(fecha, out DateTime fechaConvertida))
                        return fechaConvertida;
                    return null;
                }
                nuevaOferta.FechaRecepcionEstimacion = ConvertirFecha(fechaEstimacion);
                nuevaOferta.FechaEnvioRevision = ConvertirFecha(fechaRevision);
                nuevaOferta.FechaEnvioOfertaCliente = ConvertirFecha(fechaEnvioOferta);
                nuevaOferta.FechaGeneracionOferta = ConvertirFecha(fechaGeneracion);
                nuevaOferta.FechaAprobacionOfertaGerencia = ConvertirFecha(fechaAprobacionGerencia);
                nuevaOferta.FechaVencimientoOferta = ConvertirFecha(fechaVencimiento);
                nuevaOferta.SecuencialOfertaRequerimiento = idRequerimiento;
                nuevaOferta.Precio = precio;
                nuevaOferta.FormaPago = formaPago;
                // Conversión de descuento string a bool?
                bool? descuentoBool = null;
                if (descuento is string descuentoStr && !string.IsNullOrEmpty(descuentoStr))
                {
                    if (descuentoStr.ToUpper() == "SI") descuentoBool = true;
                    else descuentoBool = false;
                }
                else
                {
                    descuentoBool = false;
                }
                nuevaOferta.Descuento = descuentoBool;
                nuevaOferta.Estado = estado;
                nuevaOferta.Tipo = tipo;
                // Tema se guarda en el requerimiento, no en la oferta
                db.OfertaOferta.Add(nuevaOferta);
                db.SaveChanges();
                return Json(new { success = true, msg = "Se ha realizado la operación correctamente." });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";
                return Json(new { success = false, msg = exceptionMessage });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult GuardarOfertaModern(
            string OfertaRequerimiento, string codigo, string fechaEstimacion, string fechaRevision, string fechaEnvioOferta,
            string fechaGeneracion, string fechaAprobacionGerencia, string fechaVencimiento, decimal? precio, string formaPago, string descuento, string estado, string tipo, string tema)
        {
            // Reutiliza la lógica del método principal
            return GuardarOferta(OfertaRequerimiento, codigo, fechaEstimacion, fechaRevision, fechaEnvioOferta,
                fechaGeneracion, fechaAprobacionGerencia, fechaVencimiento, precio, formaPago, descuento, estado, tipo, tema);
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult EditarOferta(int id, string ofertaRequerimiento, string fechaEstimacion, string fechaRevision, string fechaEnvioOferta,
            string fechaGeneracion, string fechaAprobacionGerencia, string fechaVencimiento, decimal? precio, string formaPago, string descuento, string estado, string tipo, string tema)
        {
            try
            {
                var oferta = db.OfertaOferta.FirstOrDefault(s => s.Secuencial == id);
                if (oferta == null)
                    return Json(new { success = false, msg = "La oferta especificada no existe." });
                DateTime? ConvertirFecha(string fecha)
                {
                    if (DateTime.TryParse(fecha, out DateTime fechaConvertida))
                        return fechaConvertida;
                    return null;
                }
                oferta.FechaRecepcionEstimacion = ConvertirFecha(fechaEstimacion);
                oferta.FechaEnvioRevision = ConvertirFecha(fechaRevision);
                oferta.FechaEnvioOfertaCliente = ConvertirFecha(fechaEnvioOferta);
                oferta.FechaGeneracionOferta = ConvertirFecha(fechaGeneracion);
                oferta.FechaAprobacionOfertaGerencia = ConvertirFecha(fechaAprobacionGerencia);
                oferta.FechaVencimientoOferta = ConvertirFecha(fechaVencimiento);
                int ofertaNumerico;
                if (!string.IsNullOrEmpty(ofertaRequerimiento) && int.TryParse(ofertaRequerimiento, out ofertaNumerico))
                    oferta.SecuencialOfertaRequerimiento = ofertaNumerico;
                oferta.Precio = precio;
                oferta.FormaPago = formaPago;
                // Conversión de descuento string a bool?
                bool? descuentoBool = null;
                if (descuento is string descuentoStr && !string.IsNullOrEmpty(descuentoStr))
                {
                    if (descuentoStr.ToUpper() == "SI") descuentoBool = true;
                    else descuentoBool = false;
                }
                else
                {
                    descuentoBool = false;
                }
                oferta.Descuento = descuentoBool;
                oferta.Estado = estado;
                oferta.Tipo = tipo;
                // Tema se guarda en el requerimiento, no en la oferta
                db.SaveChanges();
                return Json(new { success = true, msg = "Se ha realizado la operación correctamente." });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";
                return Json(new { success = false, msg = exceptionMessage });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        [ActionName("editar-oferta")]
        public ActionResult EditarOfertaModern(
            int id, string ofertaRequerimiento, string fechaEstimacion, string fechaRevision, string fechaEnvioOferta,
            string fechaGeneracion, string fechaAprobacionGerencia, string fechaVencimiento, decimal? precio, string formaPago, string descuento, string estado, string tipo, string tema)
        {
            // Reutiliza la lógica del método principal
            return EditarOferta(id, ofertaRequerimiento, fechaEstimacion, fechaRevision, fechaEnvioOferta,
                fechaGeneracion, fechaAprobacionGerencia, fechaVencimiento, precio, formaPago, descuento, estado, tipo, tema);
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult EliminarOferta(string codigo)
        {
            try
            {
                var oferta = db.OfertaOferta.Where(d => d.Codigo == codigo).FirstOrDefault();

                if (oferta == null)
                {
                    return Json(new
                    {
                        success = false,
                        msg = "La oferta especificada no existe."
                    });
                }

                db.OfertaOferta.Remove(oferta);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg = "Se ha realizado la operación correctamente."
                });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";

                return Json(new
                {
                    success = false,
                    msg = exceptionMessage
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

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult DarRequerimientosOfertas()
        {
            try
            {
                // Devuelve la relación cliente-requerimiento-ticket, NO el catálogo puro
                var requerimientos = (from orq in db.OFERTAREQUERIMIENTO
                                      join cli in db.Cliente on orq.SecuencialCLiente equals cli.Secuencial
                                      join req in db.Requerimiento on orq.SecuencialRequerimiento equals req.Secuencial
                                      join t in db.Ticket on orq.SecuencialTicketTarea equals t.Secuencial
                                      select new
                                      {
                                          id = orq.Secuencial,
                                          descripcion = req.Descripcion,
                                          cliente = cli.Descripcion,
                                          ticket = t.Secuencial,
                                          detalle = orq.Detalle
                                      }).ToList();
                return Json(new
                {
                    success = true,
                    requerimientos = requerimientos,
                    msg = "Se ha realizado la operación correctamente."
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

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult GenerarCodigoOferta()
        {
            string annoActual = DateTime.Now.Year.ToString().Substring(2, 2);

            // Obtener el último código de la tabla OfertaOferta
            var ultimoCodigo = db.OfertaOferta
                                 .OrderByDescending(o => o.Codigo)
                                 .Select(o => o.Codigo)
                                 .FirstOrDefault();

            int nuevoNumero = 1; // Número inicial si no hay códigos previos

            if (!string.IsNullOrEmpty(ultimoCodigo) && ultimoCodigo.Length >= 6)
            {
                // Extraer los últimos 4 dígitos del último código y convertirlos a entero
                string ultimoNumeroStr = ultimoCodigo.Substring(2, 4);
                if (int.TryParse(ultimoNumeroStr, out int ultimoNumero))
                {
                    nuevoNumero = ultimoNumero + 1;
                }
            }

            // Formatear el nuevo número a 4 dígitos con ceros a la izquierda
            string nuevoNumeroStr = nuevoNumero.ToString().PadLeft(4, '0');

            // Concatenar el año actual con el nuevo número
            string nuevoCodigo = $"{annoActual}{nuevoNumeroStr}";

            return Json(new
            {
                success = true,
                nuevoCodigo = nuevoCodigo
            });

        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult DarEstadoOfertaComercial()
        {
            try
            {
                var ofr = (from or in db.EstadoOferta
                           select new
                           {
                               descripcion = or.Descripcion,
                               secuencial = or.Secuencial
                           }).ToList();

                var result = new
                {
                    success = true,
                    estadosOferta = ofr
                };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(result);
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult FormalizacionComercial(int start, int lenght, string filtro = "")
        {

            try
            {
                var formalizacionComercial = (from f in db.FormalizacionOfertas
                                              join o in db.OfertaOferta on f.SecuencialOferta equals o.Secuencial
                                              join r in db.EstadoOferta on f.SecuencialEstadoOferta equals r.Secuencial into estadoJoin
                                              from r in estadoJoin.DefaultIfEmpty()
                                              select new
                                              {
                                                  secuencial = f.Secuencial,
                                                  valor = f.Valor,
                                                  estadoSecuencial = f.SecuencialEstadoOferta,
                                                  estadoDescripcion = r != null ? r.Descripcion : null, // Si no hay estado (null), asignamos null a la descripción
                                                  secuencialOferta = f.SecuencialOferta,
                                                  fechaAprobacionOferta = f.FechaAprobacionOferta.HasValue ? f.FechaAprobacionOferta.Value : (DateTime?)null,
                                                  noContrato = f.NoContrato,
                                                  fomalizacionContrato = f.Formalizado,
                                                  seguimientoContrato = f.SeguimientoFormalizacion,
                                                  valorAnticipo = f.ValorAnticipo,
                                                  fechaFacturacionAnticipo = f.FechaOrdenFacturacionAnticipo.HasValue ? f.FechaOrdenFacturacionAnticipo.Value : (DateTime?)null,
                                                  ordenFacturacionAnticipo = f.NoOrdenFacturacionAnticipo,
                                                  valorEntrega = f.ValorEntrega,
                                                  ordenFacturacionEntrega = f.NoOrdenFacturacionEntrega,
                                                  valorFacturacionProgramada = f.ValorFacturacionProgramada,
                                                  ordenFacturacionProgramada = f.NoOrdenFacturacionProgramada
                                              }).ToList();

                var formalizaciones = (from f in formalizacionComercial
                                       select new
                                       {
                                           secuencial = f.secuencial,
                                           valor = f.valor,
                                           estadoSecuencial = f.estadoSecuencial,
                                           estadoDescripcion = f.estadoDescripcion,
                                           fechaAprobacionOferta = f.fechaAprobacionOferta.HasValue ? f.fechaAprobacionOferta.Value.ToString("dd/MM/yyyy") : null,
                                           noContrato = f.noContrato,
                                           formalizacionContrato = f.fomalizacionContrato == 1 ? true : false,
                                           seguimientoContrato = f.seguimientoContrato,
                                           valorAnticipo = f.valorAnticipo,
                                           fechaFacturacionAnticipo = f.fechaFacturacionAnticipo.HasValue ? f.fechaFacturacionAnticipo.Value.ToString("dd/MM/yyyy") : null,
                                           ordenFacturacionAnticipo = f.ordenFacturacionAnticipo,
                                           valorEntrega = f.valorEntrega,
                                           ordenFacturacionEntrega = f.ordenFacturacionEntrega,
                                           valorFacturacionProgramada = f.valorFacturacionProgramada,
                                           ordenFacturacionProgramada = f.ordenFacturacionProgramada
                                       }).ToList();


                if (filtro != "")
                {
                    formalizacionComercial = formalizacionComercial.Where(x =>
                                            x.valor.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.estadoDescripcion.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.fechaAprobacionOferta.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.noContrato.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.seguimientoContrato.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.valorAnticipo.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.fechaFacturacionAnticipo.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.ordenFacturacionAnticipo.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.valorEntrega.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.ordenFacturacionEntrega.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.valorFacturacionProgramada.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.ordenFacturacionProgramada.ToString().ToLower().Contains(filtro.ToLower())
                                          ).ToList();
                }

                int total = formalizacionComercial.Count();
                formalizacionComercial = formalizacionComercial.Skip(start).Take(lenght).ToList();

                var result = new
                {
                    success = true,
                    total = total,
                    formalizacionLista = formalizacionComercial
                };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(result);
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult DarDatosFormalizacion(string secuencial)
        {
            try
            {
                var sec = int.Parse(secuencial);
                FormalizacionOfertas o = db.FormalizacionOfertas.Where(d => d.Secuencial == sec).FirstOrDefault();
                if (o == null)
                {
                    throw new Exception("No se encuentra la formalización en el sistema");
                }

                var form = (from f in db.FormalizacionOfertas
                            join oo in db.OfertaOferta on f.SecuencialOferta equals oo.Secuencial
                            join r in db.EstadoOferta on f.SecuencialEstadoOferta equals r.Secuencial into estadoJoin
                            from r in estadoJoin.DefaultIfEmpty()
                            where f.Secuencial == sec
                            select new
                            {
                                secuencial = f.Secuencial,
                                valor = f.Valor,
                                estadoSecuencial = f.SecuencialEstadoOferta,
                                ofertaSecuencial = f.SecuencialOferta,
                                estadoDescripcion = r != null ? r.Descripcion : null, // Si no hay coincidencia con EstadoOferta, asignamos null
                                fechaAprobacionOferta = f.FechaAprobacionOferta.HasValue ? f.FechaAprobacionOferta.Value : (DateTime?)null,
                                noContrato = f.NoContrato,
                                fomalizacionContrato = f.Formalizado,
                                seguimientoContrato = f.SeguimientoFormalizacion,
                                valorAnticipo = f.ValorAnticipo,
                                fechaFacturacionAnticipo = f.FechaOrdenFacturacionAnticipo.HasValue ? f.FechaOrdenFacturacionAnticipo.Value : (DateTime?)null,
                                ordenFacturacionAnticipo = f.NoOrdenFacturacionAnticipo,
                                valorEntrega = f.ValorEntrega,
                                ordenFacturacionEntrega = f.NoOrdenFacturacionEntrega,
                                valorFacturacionProgramada = f.ValorFacturacionProgramada,
                                ordenFacturacionProgramada = f.NoOrdenFacturacionProgramada
                            }).FirstOrDefault();


                var result = new
                {
                    success = true,
                    formalizacion = form
                };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    success = false,
                    msg = e.Message
                };
                return Json(result);
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult GuardarFormalizacion(string valor, string estadoSecuencial, string secuencialOferta, string fechaAprobacionOferta, string noContrato, int formalizacionContrato,
    string seguimientoContrato, string valorAnticipo, string fechaFacturacionAnticipo, string ordenFacturacionAnticipo, string valorEntrega,
    string ordenFacturacionEntrega, string valorFacturacionProgramada, string ordenFacturacionProgramada)
        {
            try
            {
                FormalizacionOfertas nuevaFormalizacion = new FormalizacionOfertas();

                // Método auxiliar para convertir fechas, permitiendo nulos
                DateTime? ConvertirFecha(string fecha)
                {
                    if (DateTime.TryParse(fecha, out DateTime fechaConvertida))
                    {
                        return fechaConvertida;
                    }
                    return null; // Retorna nulo si la fecha no es válida
                }

                // Método auxiliar para convertir a Double, permitiendo valores vacíos
                double? ConvertirDouble(string val)
                {
                    if (!string.IsNullOrEmpty(val))
                    {
                        return Double.Parse(val);
                    }
                    return null; // Retorna nulo si el valor es vacío o nulo
                }

                // Usar el método ConvertirDouble para la conversión de valores
                nuevaFormalizacion.Valor = ConvertirDouble(valor) ?? 0;
                nuevaFormalizacion.SecuencialEstadoOferta = string.IsNullOrEmpty(estadoSecuencial) ? (int?)null : int.Parse(estadoSecuencial);
                nuevaFormalizacion.SecuencialOferta = int.Parse(secuencialOferta);
                nuevaFormalizacion.FechaAprobacionOferta = ConvertirFecha(fechaAprobacionOferta);
                nuevaFormalizacion.NoContrato = noContrato;
                nuevaFormalizacion.Formalizado = formalizacionContrato;
                nuevaFormalizacion.SeguimientoFormalizacion = seguimientoContrato;
                nuevaFormalizacion.ValorAnticipo = ConvertirDouble(valorAnticipo) ?? 0;
                nuevaFormalizacion.FechaOrdenFacturacionAnticipo = ConvertirFecha(fechaFacturacionAnticipo);
                nuevaFormalizacion.NoOrdenFacturacionAnticipo = ordenFacturacionAnticipo;
                nuevaFormalizacion.ValorEntrega = ConvertirDouble(valorEntrega) ?? 0;
                nuevaFormalizacion.NoOrdenFacturacionEntrega = ordenFacturacionEntrega;
                nuevaFormalizacion.ValorFacturacionProgramada = ConvertirDouble(valorFacturacionProgramada) ?? 0;
                nuevaFormalizacion.NoOrdenFacturacionProgramada = ordenFacturacionProgramada;

                db.FormalizacionOfertas.Add(nuevaFormalizacion);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg = "Se ha realizado la operación correctamente."
                });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";
                return Json(new { success = false, msg = exceptionMessage });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult EditarFormalizacion(int secuencial, string valor, string estadoSecuencial, string secuencialOferta, string fechaAprobacionOferta, string noContrato, int formalizacionContrato,
    string seguimientoContrato, string valorAnticipo, string fechaFacturacionAnticipo, string ordenFacturacionAnticipo, string valorEntrega,
    string ordenFacturacionEntrega, string valorFacturacionProgramada, string ordenFacturacionProgramada)
        {
            try
            {
                var formalizacion = db.FormalizacionOfertas.FirstOrDefault(s => s.Secuencial == secuencial);

                if (formalizacion == null)
                {
                    return Json(new
                    {
                        success = false,
                        msg = "La formalización especificada no existe."
                    });
                }

                // Método auxiliar para convertir fechas, permitiendo nulos
                DateTime? ConvertirFecha(string fecha)
                {
                    if (DateTime.TryParse(fecha, out DateTime fechaConvertida))
                    {
                        return fechaConvertida;
                    }
                    return null; // Retorna nulo si la fecha no es válida
                }

                // Método auxiliar para convertir a Double, permitiendo valores vacíos
                double? ConvertirDouble(string val)
                {
                    if (!string.IsNullOrEmpty(val))
                    {
                        return Double.Parse(val);
                    }
                    return null; // Retorna nulo si el valor es vacío o nulo
                }

                // Método auxiliar para convertir a int, permitiendo valores vacíos
                int? ConvertirInt(string val)
                {
                    if (!string.IsNullOrEmpty(val))
                    {
                        return int.Parse(val);
                    }
                    return null; // Retorna nulo si el valor es vacío o nulo
                }

                // Usar los métodos ConvertirDouble y ConvertirInt para la conversión de valores
                formalizacion.Valor = ConvertirDouble(valor) ?? (double?)null;
                formalizacion.SecuencialEstadoOferta = ConvertirInt(estadoSecuencial);
                formalizacion.SecuencialOferta = int.Parse(secuencialOferta);
                formalizacion.FechaAprobacionOferta = ConvertirFecha(fechaAprobacionOferta);
                formalizacion.NoContrato = noContrato;
                formalizacion.Formalizado = formalizacionContrato;
                formalizacion.SeguimientoFormalizacion = seguimientoContrato;
                formalizacion.ValorAnticipo = ConvertirDouble(valorAnticipo) ?? (double?)null;
                formalizacion.FechaOrdenFacturacionAnticipo = ConvertirFecha(fechaFacturacionAnticipo);
                formalizacion.NoOrdenFacturacionAnticipo = ordenFacturacionAnticipo;
                formalizacion.ValorEntrega = ConvertirDouble(valorEntrega) ?? (double?)null;
                formalizacion.NoOrdenFacturacionEntrega = ordenFacturacionEntrega;
                formalizacion.ValorFacturacionProgramada = ConvertirDouble(valorFacturacionProgramada) ?? (double?)null;
                formalizacion.NoOrdenFacturacionProgramada = ordenFacturacionProgramada;

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg = "Se ha realizado la operación correctamente."
                });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";

                return Json(new
                {
                    success = false,
                    msg = exceptionMessage
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

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult EliminarFormalizacion(string secuencial)
        {
            try
            {
                var sec = int.Parse(secuencial);
                var formalizacion = db.FormalizacionOfertas.Where(d => d.Secuencial == sec).FirstOrDefault();

                if (formalizacion == null)
                {
                    return Json(new
                    {
                        success = false,
                        msg = "La formalización especificada no existe."
                    });
                }

                db.FormalizacionOfertas.Remove(formalizacion);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg = "Se ha realizado la operación correctamente."
                });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Error de validación: {fullErrorMessage}";

                return Json(new
                {
                    success = false,
                    msg = exceptionMessage
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

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult DarOfertasOfertas()
        {
            try
            {
                var ofertasComercial = (from or in db.OfertaOferta
                                        select new
                                        {
                                            secuencial = or.Secuencial,
                                            detalle = or.Secuencial + "-" + or.Codigo.Substring(0, 30)
                                        }).ToList();
                return Json(new
                {
                    success = true,
                    ofertasComercial = ofertasComercial,
                    msg = "Se ha realizado la operación correctamente."
                });
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new
                {
                    success = false
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

        [HttpPost]
        [Authorize(Roles = "COMERCIAL, ADMIN")]
        public ActionResult GenerarCodigoFormalizacion()
        {
            string annoActual = DateTime.Now.Year.ToString().Substring(2, 2);

            // Obtener el último código de la tabla OfertaOferta
            var ultimoCodigo = db.FormalizacionOfertas
                                 .OrderByDescending(o => o.Secuencial)
                                 .Select(o => o.Secuencial)
                                 .FirstOrDefault();

            int nuevoNumero = 1; // Número inicial si no hay códigos previos

            if (ultimoCodigo != 0)
            {
                // Extraer los últimos 4 dígitos del último código y convertirlos a entero
                nuevoNumero = ultimoCodigo + 1;
            }

            // Formatear el nuevo número a 4 dígitos con ceros a la izquierda
            string nuevoNumeroStr = nuevoNumero.ToString().PadLeft(4, '0');

            // Concatenar el año actual con el nuevo número
            string nuevoCodigo = $"{annoActual}-{nuevoNumeroStr}";

            return Json(new
            {
                success = true,
                nuevoCodigo = nuevoCodigo
            });

        }

    }

}
