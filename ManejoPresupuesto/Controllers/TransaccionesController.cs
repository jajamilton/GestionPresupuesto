﻿using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Transactions;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController: Controller
    {
        private readonly IServiciosUsuarios servicios;
        private readonly IRespositorioTransaccion respositorioTransaccion;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCtagorias repositorioCtagorias;
        private readonly IMapper mapper;

        public TransaccionesController(IServiciosUsuarios servicios, 
            IRespositorioTransaccion respositorioTransaccion, 
            IRepositorioCuentas repositorioCuentas,
            IRepositorioCtagorias repositorioCtagorias,
            IMapper mapper )
        {
            this.servicios = servicios;
            this.respositorioTransaccion = respositorioTransaccion;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCtagorias = repositorioCtagorias;
            this.mapper = mapper;
        }



        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]   
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicios.ObtenerUusarioId();
            var modelo = new TrasaccionViewModel();

            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);

        }


        public async Task<IActionResult> Crear(TrasaccionViewModel modelo)
        {
            var usuarioId = servicios.ObtenerUusarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }


            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCtagorias.ObtenerId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;

            if (modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.Monto *= -1;
            }

            await respositorioTransaccion.Crear(modelo);
            return View("Index");

        }


        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()));


        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicios.ObtenerUusarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }


        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCtagorias.ObtenerCategorias(usuarioId, tipoOperacion);
            return categorias.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()));


        }


        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicios.ObtenerUusarioId();
            var transaccion = await respositorioTransaccion.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("No Encontrado", "Home");
            }

            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if(modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categoria = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);

            return View(modelo);

        }


        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicios.ObtenerUusarioId();

            if (!ModelState.IsValid)
            {
                modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                modelo.Cuentas = await ObtenerCuentas(usuarioId);

                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }


            var categorias = await repositorioCtagorias.ObtenerId(modelo.CategoriaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }


            var transaccion = mapper.Map<Transaccion>(modelo);
            if (transaccion.TipoOperacionId == TipoOperacion.Gastos) 
            {
                transaccion.Monto *= -1;
            }

            await respositorioTransaccion.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

            return RedirectToAction("Index");


        }



        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicios.ObtenerUusarioId();
            var transaccion = await respositorioTransaccion.ObtenerPorId(id, usuarioId);

            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await respositorioTransaccion.Borrar(id);
            return RedirectToAction("Index");
        }




    }
}
