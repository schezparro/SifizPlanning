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
                                                   colaborador = or.SecuencialColaborador.HasValue ? 
                                                       (from col in db.Colaborador
                                                        join p in db.Persona on col.SecuencialPersona equals p.Secuencial
                                                        where col.Secuencial == or.SecuencialColaborador
                                                        select p.Nombre1 + " " + p.Apellido1).FirstOrDefault() : "",
                                                   colaboradorId = or.SecuencialColaborador
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
                               colaboradorId = or.SecuencialColaborador,
                               colaborador = or.SecuencialColaborador.HasValue ? 
                                   (from col in db.Colaborador
                                    join p in db.Persona on col.SecuencialPersona equals p.Secuencial
                                    where col.Secuencial == or.SecuencialColaborador
                                    select p.Nombre1 + " " + p.Apellido1).FirstOrDefault() : ""
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
        public ActionResult GuardarRequerimiento(int cliente, int requerimiento, string ticket, string detalle, string fechaPedidoCliente, int? colaborador)
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
                            nuevaOfertaRequerimiento.Detalle = null;
                        }
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
                // Siempre asignar el colaborador si viene informado
                nuevaOfertaRequerimiento.SecuencialColaborador = colaborador;


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
        public ActionResult EditarRequerimiento(int id, int cliente, int requerimiento, string ticket, string detalle, string fechaPedidoCliente, int? colaborador)
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
                nuevaOfertaRequerimiento.SecuencialColaborador = colaborador;

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
                // Buscar la oferta real y sus datos relacionados en una sola consulta
                var oferta = (from or in db.OfertaOferta
                              join r in db.OFERTAREQUERIMIENTO on or.SecuencialOfertaRequerimiento equals r.Secuencial
                              join cli in db.Cliente on r.SecuencialCLiente equals cli.Secuencial
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
                                  fechaGeneracion = or.FechaGeneracionOferta,
                                  fechaVencimiento = or.FechaVencimientoOferta,
                                  // Se optimiza trayendo todo el historial relevante en una lista
                                  historicoTicket = db.TicketHistorico
                                                      .Where(h => h.SecuencialTicket == r.SecuencialTicketTarea)
                                                      .OrderByDescending(h => h.Version)
                                                      .ToList()
                              }).FirstOrDefault();

                if (oferta == null)
                {
                    return Json(new { success = false, msg = "No se encontró la oferta con el ID proporcionado." });
                }

                // Extraer las fechas específicas y la próxima actividad de la lista en memoria
                var fechas = new
                {
                    fechaEstimacion = oferta.historicoTicket.FirstOrDefault(h => h.SecuencialProximaActividad == 3)?.FechaOperacion,
                    fechaRevision = oferta.historicoTicket.FirstOrDefault(h => h.SecuencialProximaActividad == 7)?.FechaOperacion,
                    fechaAprobacionGerencia = oferta.historicoTicket.FirstOrDefault(h => h.SecuencialProximaActividad == 30)?.FechaOperacion,
                    fechaEnvioOferta = oferta.historicoTicket.FirstOrDefault(h => h.SecuencialProximaActividad == 30)?.FechaOperacion,
                    proximaActividad = oferta.historicoTicket.FirstOrDefault()?.proximaActividad?.Descripcion,
                };

                // Construir el objeto de respuesta final con fechas formateadas
                var resultadoOferta = new
                {
                    oferta.secuencial,
                    oferta.codigo,
                    oferta.precio,
                    oferta.formaPago,
                    oferta.descuento,
                    oferta.estado,
                    oferta.tipo,
                    oferta.tema,
                    oferta.cliente,
                    oferta.ticket,
                    oferta.detalleRequerimiento,
                    fechaEstimacion = fechas.fechaEstimacion?.ToString("dd/MM/yyyy"),
                    fechaRevision = fechas.fechaRevision?.ToString("dd/MM/yyyy"),
                    fechaAprobacionGerencia = fechas.fechaAprobacionGerencia?.ToString("dd/MM/yyyy"),
                    fechaEnvioOferta = fechas.fechaEnvioOferta?.ToString("dd/MM/yyyy"),
                    fechaGeneracion = oferta.fechaGeneracion?.ToString("dd/MM/yyyy"),
                    fechaVencimiento = oferta.fechaVencimiento?.ToString("dd/MM/yyyy"),
                    fechas.proximaActividad
                };

                return Json(new { success = true, detalle = resultadoOferta });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = "Ocurrió un error inesperado al obtener los detalles de la oferta. " + e.Message });
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
                var filtros = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(filtrosColumna))
                {
                    filtros = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(filtrosColumna);
                }

                // Crear una consulta LINQ que se ejecutará de forma diferida
                var ofertasQuery = from or in db.OfertaOferta
                                   join r in db.OFERTAREQUERIMIENTO on or.SecuencialOfertaRequerimiento equals r.Secuencial
                                   join cli in db.Cliente on r.SecuencialCLiente equals cli.Secuencial
                                   select new
                                   {
                                       secuencial = or.Secuencial,
                                       codigo = or.Codigo,
                                       secuencialEstadoGestion = or.SecuencialEstadoGestion,
                                       observacion = or.Observacion,
                                       secuencialRequerimiento = r.Secuencial,
                                       fechaGeneracion = or.FechaGeneracionOferta,
                                       fechaVencimiento = or.FechaVencimientoOferta,
                                       tipo = r.Secuencial + "-" + (r.Detalle.Length > 50 ? r.Detalle.Substring(0, 50) : r.Detalle),
                                       idTipo = r.Secuencial,
                                       precio = or.Precio,
                                       formaPago = or.FormaPago,
                                       descuento = (or.Descuento.HasValue && or.Descuento.Value) ? "SI" : "NO",
                                       estado = or.Estado,
                                       tema = or.OfertaRequerimiento != null ? or.OfertaRequerimiento.Detalle : "",
                                       cliente = cli.Descripcion,
                                       ticket = r.SecuencialTicketTarea,
                                   };

                // Aplicar filtros a la consulta ANTES de la paginación y ejecución
                if (!string.IsNullOrEmpty(filtro))
                {
                    var filtroLower = filtro.ToLower();
                    ofertasQuery = ofertasQuery.Where(x =>
                        (x.codigo != null && x.codigo.ToLower().Contains(filtroLower)) ||
                        (x.cliente != null && x.cliente.ToLower().Contains(filtroLower)) ||
                        (x.ticket != null && x.ticket.ToString().Contains(filtro)) ||
                        (x.tema != null && x.tema.ToLower().Contains(filtroLower))
                    );
                }

                if (filtros != null && filtros.Count > 0)
                {
                    if (filtros.ContainsKey("codigoOferta") && !string.IsNullOrEmpty(filtros["codigoOferta"]))
                        ofertasQuery = ofertasQuery.Where(x => x.codigo != null && x.codigo.ToLower().Contains(filtros["codigoOferta"].ToLower()));

                    if (filtros.ContainsKey("ticket") && !string.IsNullOrEmpty(filtros["ticket"]))
                        ofertasQuery = ofertasQuery.Where(x => x.ticket != null && x.ticket.ToString().Contains(filtros["ticket"]));

                    if (filtros.ContainsKey("cliente") && !string.IsNullOrEmpty(filtros["cliente"]))
                        ofertasQuery = ofertasQuery.Where(x => x.cliente != null && x.cliente.ToLower().Contains(filtros["cliente"].ToLower()));

                    if (filtros.ContainsKey("tema") && !string.IsNullOrEmpty(filtros["tema"]))
                        ofertasQuery = ofertasQuery.Where(x => x.tema != null && x.tema.ToLower().Contains(filtros["tema"].ToLower()));

                    if (filtros.ContainsKey("estadoGestion") && !string.IsNullOrEmpty(filtros["estadoGestion"]))
                    {
                        int idEstadoFiltro;
                        if (int.TryParse(filtros["estadoGestion"], out idEstadoFiltro))
                        {
                            ofertasQuery = ofertasQuery.Where(x => x.secuencialEstadoGestion == idEstadoFiltro);
                        }
                    }

                    if (filtros.ContainsKey("observacion") && !string.IsNullOrEmpty(filtros["observacion"]))
                        ofertasQuery = ofertasQuery.Where(x => x.observacion != null && x.observacion.ToLower().Contains(filtros["observacion"].ToLower()));
                }

                int total = ofertasQuery.Count();

                // Paginación y ejecución de la consulta
                var pagedOfertas = ofertasQuery.OrderByDescending(o => o.secuencial).Skip(start).Take(lenght).ToList();

                // Poblar fechas y próxima actividad en memoria para CADA oferta
                var ofertasConFechas = pagedOfertas.Select(oferta =>
                {
                    var historicoTicket = (oferta.ticket.HasValue)
                        ? db.TicketHistorico.Where(h => h.SecuencialTicket == oferta.ticket.Value).OrderByDescending(h => h.Version).ToList()
                        : new List<TicketHistorico>();

                    return new
                    {
                        oferta.secuencial,
                        oferta.codigo,
                        oferta.secuencialEstadoGestion,
                        oferta.observacion,
                        fechaEstimacion = historicoTicket.FirstOrDefault(h => h.SecuencialProximaActividad == 3)?.FechaOperacion,
                        fechaRevision = historicoTicket.FirstOrDefault(h => h.SecuencialProximaActividad == 7)?.FechaOperacion,
                        fechaAprobacionGerencia = historicoTicket.FirstOrDefault(h => h.SecuencialProximaActividad == 30)?.FechaOperacion,
                        fechaEnvioOferta = historicoTicket.FirstOrDefault(h => h.SecuencialProximaActividad == 30)?.FechaOperacion,
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
                        oferta.secuencialRequerimiento,
                        proximaActividad = historicoTicket.FirstOrDefault()?.proximaActividad?.Descripcion,
                    };
                }).ToList();

                var result = new
                {
                    success = true,
                    total = total,
                    ofertasComercial = ofertasConFechas
                };
                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
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
                // Para ofertas no mayores no asociadas a ticket, permitir código vacío
                nuevaOferta.Codigo = string.IsNullOrEmpty(codigo) ? null : codigo;
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
                string emailUser = User.Identity.Name;
                
                // Obtener el colaborador actual
                var colaboradorActual = (from u in db.Usuario
                                        join p in db.Persona on u.SecuencialPersona equals p.Secuencial
                                        join c in db.Colaborador on p.Secuencial equals c.SecuencialPersona
                                        where u.Email == emailUser
                                        select c).FirstOrDefault();

                // Verificar si el usuario tiene rol ADMIN o ADMINCOMERCIAL
                bool esAdmin = User.IsInRole("ADMIN") || User.IsInRole("ADMINCOMERCIAL");

                IQueryable<dynamic> requerimientosQuery;

                if (esAdmin)
                {
                    // Si es ADMIN o ADMINCOMERCIAL, mostrar todos los requerimientos
                    requerimientosQuery = from orq in db.OFERTAREQUERIMIENTO
                                         join cli in db.Cliente on orq.SecuencialCLiente equals cli.Secuencial
                                         join req in db.Requerimiento on orq.SecuencialRequerimiento equals req.Secuencial
                                         select new
                                         {
                                             id = orq.Secuencial,
                                             descripcion = req.Descripcion,
                                             cliente = cli.Descripcion,
                                             ticket = orq.SecuencialTicketTarea,
                                             detalle = orq.Detalle
                                         };
                }
                else
                {
                    // Si no es admin, solo mostrar los requerimientos donde es responsable
                    requerimientosQuery = from orq in db.OFERTAREQUERIMIENTO
                                         join cli in db.Cliente on orq.SecuencialCLiente equals cli.Secuencial
                                         join req in db.Requerimiento on orq.SecuencialRequerimiento equals req.Secuencial
                                         where orq.SecuencialColaborador == colaboradorActual.Secuencial
                                         select new
                                         {
                                             id = orq.Secuencial,
                                             descripcion = req.Descripcion,
                                             cliente = cli.Descripcion,
                                             ticket = orq.SecuencialTicketTarea,
                                             detalle = orq.Detalle
                                         };
                }

                var requerimientos = requerimientosQuery.ToList();

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
        public ActionResult ActualizarEstadoGestionOferta(int idOferta, int? idEstado, string observacion)
        {
            try
            {
                var oferta = db.OfertaOferta.FirstOrDefault(o => o.Secuencial == idOferta);
                if (oferta == null)
                {
                    return Json(new { success = false, msg = "No se encontró la oferta especificada." });
                }

                oferta.SecuencialEstadoGestion = idEstado;
                oferta.Observacion = observacion;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
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
            catch (DbEntityValidationException)
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
