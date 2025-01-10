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
                } else
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

    }
}