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
                        nuevaOfertaRequerimiento.Detalle = t.Detalle.Substring(0, 250);
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
        public ActionResult DarCatalogoRequerimientos()
        {
            try
            {
                var ofr = (from or in db.Requerimiento
                           select new
                           {
                               descripcion = or.Descripcion,
                               id = or.Secuencial
                           }).ToList();

                var result = new
                {
                    success = true,
                    requerimientos = ofr
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
        public ActionResult OfertasComercial(int start, int lenght, string filtro = "")
        {

            try
            {
                var ofertasComercial = (from or in db.OfertaOferta
                                        join r in db.OFERTAREQUERIMIENTO on or.SecuencialOfertaRequerimiento equals r.Secuencial
                                        select new
                                        {
                                            secuencial = or.Secuencial,
                                            codigo = or.Codigo,
                                            fechaEstimacion = or.FechaRecepcionEstimacion.HasValue ? or.FechaRecepcionEstimacion.Value : (DateTime?)null,
                                            fechaRevision = or.FechaEnvioRevision.HasValue ? or.FechaEnvioRevision.Value : (DateTime?)null,
                                            fechaEnvioOferta = or.FechaEnvioOfertaCliente.HasValue ? or.FechaEnvioOfertaCliente.Value : (DateTime?)null,
                                            fechaGeneracion = or.FechaGeneracionOferta.HasValue ? or.FechaGeneracionOferta.Value : (DateTime?)null,
                                            fechaAprobacionGerencia = or.FechaAprobacionOfertaGerencia.HasValue ? or.FechaAprobacionOfertaGerencia.Value : (DateTime?)null,
                                            fechaVencimiento = or.FechaVencimientoOferta.HasValue ? or.FechaVencimientoOferta.Value : (DateTime?)null,
                                            tipo = r.Secuencial + "-" + r.Detalle.Substring(0, 30),
                                            idTipo = r.Secuencial
                                        }).ToList();

                var ofertas = (from oc in ofertasComercial
                               select new
                               {
                                   secuencial = oc.secuencial,
                                   codigo = oc.codigo,
                                   fechaEstimacion = oc.fechaEstimacion.HasValue ? oc.fechaEstimacion.Value.ToString("dd/MM/yyyy") : null,
                                   fechaRevision = oc.fechaRevision.HasValue ? oc.fechaRevision.Value.ToString("dd/MM/yyyy") : null,
                                   fechaEnvioOferta = oc.fechaEnvioOferta.HasValue ? oc.fechaEnvioOferta.Value.ToString("dd/MM/yyyy") : null,
                                   fechaGeneracion = oc.fechaGeneracion.HasValue ? oc.fechaGeneracion.Value.ToString("dd/MM/yyyy") : null,
                                   fechaAprobacionGerencia = oc.fechaAprobacionGerencia.HasValue ? oc.fechaAprobacionGerencia.Value.ToString("dd/MM/yyyy") : null,
                                   fechaVencimiento = oc.fechaVencimiento.HasValue ? oc.fechaVencimiento.Value.ToString("dd/MM/yyyy") : null,
                                   tipo = oc.tipo,
                                   idTipo = oc.idTipo
                               }).ToList();


                if (filtro != "")
                {
                    ofertasComercial = ofertasComercial.Where(x =>
                                            x.codigo.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.fechaEstimacion.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.fechaRevision.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.fechaAprobacionGerencia.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.fechaEnvioOferta.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.fechaGeneracion.ToString().ToLower().Contains(filtro.ToLower()) ||
                                            x.fechaVencimiento.ToString().ToLower().Contains(filtro.ToLower())
                                          ).ToList();
                }

                int total = ofertasComercial.Count();
                ofertasComercial = ofertasComercial.Skip(start).Take(lenght).ToList();

                var result = new
                {
                    success = true,
                    total = total,
                    ofertasComercial = ofertasComercial
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
            string fechaGeneracion, string fechaAprobacionGerencia, string fechaVencimiento)
        {
            try
            {
                OfertaOferta nuevaOferta = new OfertaOferta();

                //string[] fechaEst = fechaEstimacion.Split(new Char[] { '/' });
                //int diaE = Int32.Parse(fechaEst[0]);
                //int mesE = Int32.Parse(fechaEst[1]);
                //int annoE = Int32.Parse(fechaEst[2]);
                //DateTime fEstimacion = new DateTime(annoE, mesE, diaE);

                //string[] fechaRev = fechaRevision.Split(new Char[] { '/' });
                //int diaRev = Int32.Parse(fechaRev[0]);
                //int mesRev = Int32.Parse(fechaRev[1]);
                //int annoRev = Int32.Parse(fechaRev[2]);
                //DateTime fRevision = new DateTime(annoRev, mesRev, diaRev);

                //string[] fechaEO = fechaEnvioOferta.Split(new Char[] { '/' });
                //int diaEO = Int32.Parse(fechaEO[0]);
                //int mesEO = Int32.Parse(fechaEO[1]);
                //int annoEO = Int32.Parse(fechaEO[2]);
                //DateTime fEnvioOferta = new DateTime(annoEO, mesEO, diaEO);

                //string[] fechaG = fechaGeneracion.Split(new Char[] { '/' });
                //int diaG = Int32.Parse(fechaG[0]);
                //int mesG = Int32.Parse(fechaG[1]);
                //int annoG = Int32.Parse(fechaG[2]);
                //DateTime fGeneracion = new DateTime(annoG, mesG, diaG);

                //string[] fechaAprobacionG = fechaAprobacionGerencia.Split(new Char[] { '/' });
                //int diaAG = Int32.Parse(fechaAprobacionG[0]);
                //int mesAG = Int32.Parse(fechaAprobacionG[1]);
                //int annoAG = Int32.Parse(fechaAprobacionG[2]);
                //DateTime fAprobacionGerencia = new DateTime(annoAG, mesAG, diaAG);

                //string[] fechaV = fechaVencimiento.Split(new Char[] { '/' });
                //int diaV = Int32.Parse(fechaV[0]);
                //int mesV = Int32.Parse(fechaV[1]);
                //int annoV = Int32.Parse(fechaV[2]);
                //DateTime fVencimiento = new DateTime(annoV, mesV, diaV);

                nuevaOferta.Codigo = codigo;
                //nuevaOferta.FechaRecepcionEstimacion = fEstimacion;
                //nuevaOferta.FechaEnvioRevision = fRevision;
                //nuevaOferta.FechaEnvioOfertaCliente = fEnvioOferta;
                //nuevaOferta.FechaGeneracionOferta = fGeneracion;
                //nuevaOferta.FechaAprobacionOfertaGerencia = fAprobacionGerencia;
                //nuevaOferta.FechaVencimientoOferta = fVencimiento;

                // Método auxiliar para convertir fechas, permitiendo nulos
                DateTime? ConvertirFecha(string fecha)
                {
                    if (DateTime.TryParse(fecha, out DateTime fechaConvertida))
                    {
                        return fechaConvertida;
                    }
                    return null; // Retorna nulo si la fecha no es válida
                }

                nuevaOferta.Codigo = codigo;
                nuevaOferta.FechaRecepcionEstimacion = ConvertirFecha(fechaEstimacion);
                nuevaOferta.FechaEnvioRevision = ConvertirFecha(fechaRevision);
                nuevaOferta.FechaEnvioOfertaCliente = ConvertirFecha(fechaEnvioOferta);
                nuevaOferta.FechaGeneracionOferta = ConvertirFecha(fechaGeneracion);
                nuevaOferta.FechaAprobacionOfertaGerencia = ConvertirFecha(fechaAprobacionGerencia);
                nuevaOferta.FechaVencimientoOferta = ConvertirFecha(fechaVencimiento);

                int ofertaNumerico;

                if (OfertaRequerimiento != "")
                {
                    if (!int.TryParse(OfertaRequerimiento, out ofertaNumerico))
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "El ticket debe ser un valor numérico válido."
                        });
                    }
                    else
                    {
                        nuevaOferta.SecuencialOfertaRequerimiento = ofertaNumerico;
                    }
                }

                db.OfertaOferta.Add(nuevaOferta);
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
        public ActionResult EditarOferta(int id, string ofertaRequerimiento, string fechaEstimacion, string fechaRevision, string fechaEnvioOferta,
            string fechaGeneracion, string fechaAprobacionGerencia, string fechaVencimiento)
        {
            try
            {
                var oferta = db.OfertaOferta.FirstOrDefault(s => s.Secuencial == id);

                string[] fechaEst = fechaEstimacion.Split(new Char[] { '/' });
                int diaE = Int32.Parse(fechaEst[0]);
                int mesE = Int32.Parse(fechaEst[1]);
                int annoE = Int32.Parse(fechaEst[2]);
                DateTime fEstimacion = new DateTime(annoE, mesE, diaE);

                string[] fechaRev = fechaRevision.Split(new Char[] { '/' });
                int diaRev = Int32.Parse(fechaRev[0]);
                int mesRev = Int32.Parse(fechaRev[1]);
                int annoRev = Int32.Parse(fechaRev[2]);
                DateTime fRevision = new DateTime(annoRev, mesRev, diaRev);

                string[] fechaEO = fechaEnvioOferta.Split(new Char[] { '/' });
                int diaEO = Int32.Parse(fechaEO[0]);
                int mesEO = Int32.Parse(fechaEO[1]);
                int annoEO = Int32.Parse(fechaEO[2]);
                DateTime fEnvioOferta = new DateTime(annoEO, mesEO, diaEO);

                string[] fechaG = fechaGeneracion.Split(new Char[] { '/' });
                int diaG = Int32.Parse(fechaG[0]);
                int mesG = Int32.Parse(fechaG[1]);
                int annoG = Int32.Parse(fechaG[2]);
                DateTime fGeneracion = new DateTime(annoG, mesG, diaG);

                string[] fechaAprobacionG = fechaAprobacionGerencia.Split(new Char[] { '/' });
                int diaAG = Int32.Parse(fechaAprobacionG[0]);
                int mesAG = Int32.Parse(fechaAprobacionG[1]);
                int annoAG = Int32.Parse(fechaAprobacionG[2]);
                DateTime fAprobacionGerencia = new DateTime(annoAG, mesAG, diaAG);

                string[] fechaV = fechaVencimiento.Split(new Char[] { '/' });
                int diaV = Int32.Parse(fechaV[0]);
                int mesV = Int32.Parse(fechaV[1]);
                int annoV = Int32.Parse(fechaV[2]);
                DateTime fVencimiento = new DateTime(annoV, mesV, diaV);



                //if (ofertaRequerimiento > 0)
                //{
                //    var ticket = (from r in db.OFERTAREQUERIMIENTO
                //                  join t in db.Ticket on r.SecuencialTicketTarea equals t.Secuencial
                //                  where r.Secuencial == ofertaRequerimiento
                //                  select t).FirstOrDefault();

                //    if (ticket != null)
                //    {
                //        var est = (from r in db.TicketHistorico
                //                   join e in db.EstadoTicket on r.SecuencialEstadoTicket equals e.Secuencial
                //                   where e.Codigo == "COTIZAR"
                //                   select new
                //                   {
                //                       fechaEstimacion = r.FechaOperacion
                //                   }).FirstOrDefault();

                //        var aprobarcion = (from r in db.TicketHistorico
                //                           join e in db.EstadoTicket on r.SecuencialEstadoTicket equals e.Secuencial
                //                           where e.Codigo == "APROBADO"
                //                           select new
                //                           {
                //                               fechaAprobado = r.FechaOperacion
                //                           }).FirstOrDefault();

                //    }
                //};

                if (oferta == null)
                {
                    return Json(new
                    {
                        success = false,
                        msg = "La oferta especificada no existe."
                    });
                }

                DateTime? ConvertirFecha(string fecha)
                {
                    if (DateTime.TryParse(fecha, out DateTime fechaConvertida))
                    {
                        return fechaConvertida;
                    }
                    return null; // Retorna nulo si la fecha no es válida
                }

                oferta.FechaRecepcionEstimacion = ConvertirFecha(fechaEstimacion);
                oferta.FechaEnvioRevision = ConvertirFecha(fechaRevision);
                oferta.FechaEnvioOfertaCliente = ConvertirFecha(fechaEnvioOferta);
                oferta.FechaGeneracionOferta = ConvertirFecha(fechaGeneracion);
                oferta.FechaAprobacionOfertaGerencia = ConvertirFecha(fechaAprobacionGerencia);
                oferta.FechaVencimientoOferta = ConvertirFecha(fechaVencimiento);

                int ofertaNumerico;

                if (ofertaRequerimiento != "")
                {
                    if (!int.TryParse(ofertaRequerimiento, out ofertaNumerico))
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "El ticket debe ser un valor numérico válido."
                        });
                    }
                    else
                    {
                        oferta.SecuencialOfertaRequerimiento = ofertaNumerico;
                    }
                }

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
                var requerimientosComercial = (from or in db.OFERTAREQUERIMIENTO
                                               select new
                                               {
                                                   secuencial = or.Secuencial,
                                                   detalle = or.Secuencial + "-" + or.Detalle.Substring(0, 30)
                                               }).ToList();
                return Json(new
                {
                    success = true,
                    requerimientosComercial = requerimientosComercial,
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
