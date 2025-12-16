using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using SAPbobsCOM;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using AutoMapper;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FactoresController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        private readonly IMapper _mapper;
        private readonly IEmpresaRuntimeService _empresaRuntime;
        private readonly ILogger<AccountController> _logger;

        public FactoresController(IDocumentService documentService, ILoginService loginService
            , IMapper mapper
            , IEmpresaRuntimeService empresaRuntime, ILogger<AccountController> logger)
        {
            _documentService = documentService;
            _loginService = loginService;
            _mapper = mapper;
            _empresaRuntime = empresaRuntime;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseInformation>> GetFactores([FromQuery] string Empresa,
                                                   [FromQuery] string periodo,
                                                   [FromQuery] string tiendas)
        {
            try
            {
                // Validación de empresa
                ResponseInformation rp = await _documentService.ValidaDatos(Empresa);
                if (!rp.Registered)
                {
                    _logger.LogWarning("Validación fallida para empresa: {Empresa}", Empresa);
                    return Ok(rp);
                }

                // Obtener menú desde SAP HANA
                rp = await _documentService.GetFactoresDB(Empresa, periodo, tiendas);

                if (!rp.Registered)
                {
                    _logger.LogWarning("Datos de factores no disponible para la empresa: {Empresa}", Empresa);
                    return Ok(rp);
                }

                // Éxito
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fecha de recepción para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("tiendas")]
        public async Task<ActionResult<ResponseInformation>> GetTiendas([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetTiendasActivas(Empresa);

                if (!rp.Registered)
                {
                    _logger.LogWarning("No se encontraron tiendas activas para la empresa: {Empresa}", Empresa);
                    return Ok(rp);
                }

                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las tiendas activas para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("nuevo")]
        public async Task<ActionResult<ResponseInformation>> GetFactoresPrevios([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetFactoresNuevoDB(Empresa);

                if (!rp.Registered)
                {
                    _logger.LogWarning("No hay información previa para la empresa: {Empresa}", Empresa);
                    return Ok(rp);
                }

                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información previa para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpPost("actualizar")]
        public async Task<ActionResult<ResponseInformation>> Actualizar([FromQuery] string docEntry, [FromQuery] string Empresa, [FromBody] MatrizFactorUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(docEntry))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "DocEntry inválido",
                    Content = string.Empty
                });
            }

            if (request?.MGS_CL_FACDETCollection == null || request.MGS_CL_FACDETCollection.Count == 0)
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "No se enviaron factores para actualizar",
                    Content = string.Empty
                });
            }

            // Llama al helper una sola vez
            var prep = await _empresaRuntime.ResolveAndLoginAsync(Empresa);
            if (!prep.Ok) return BadRequest(prep.Error);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };


            
            // 5) Preparar request hacia DocumentService
            var requestInformation = new RequestInformation
            {
                // Route = "Orders(" + docEntry + ")",

                Route = $"MGS_CL_FACCAB({docEntry})",
                Token = prep.Token,
                Doc = JsonConvert.SerializeObject(request, settings)

            };

            // 6) Enviar usando cfg (overload que usa ConnectionString de la empresa)
            var rp = await _documentService.UpdateInfo(requestInformation, "PYP", prep.Cfg);

            // 7) Responder
            return Ok(rp);
            //-----------------------

            //if (string.IsNullOrWhiteSpace(docEntry))
            //{
            //    return BadRequest(new ResponseInformation
            //    {
            //        Registered = false,
            //        Message = "DocEntry inválido",
            //        Content = string.Empty
            //    });
            //}

            //if (factores == null || factores.Count == 0)
            //{
            //    return BadRequest(new ResponseInformation
            //    {
            //        Registered = false,
            //        Message = "No se enviaron factores para actualizar",
            //        Content = string.Empty
            //    });
            //}

            //var rp = await _documentService.ValidaDatos(Empresa);
            //if (!rp.Registered)
            //{
            //    return BadRequest(rp);
            //}

            //var token = await _loginService.Login(Empresa);
            //if (string.IsNullOrEmpty(token))
            //{
            //    return StatusCode(503, new ResponseInformation
            //    {
            //        Registered = false,
            //        Message = "No fue posible conectarse al Service Layer",
            //        Content = string.Empty
            //    });
            //}

            //var settings = new JsonSerializerSettings
            //{
            //    NullValueHandling = NullValueHandling.Ignore
            //};

            //var requestInformation = new RequestInformation
            //{
            //    Route = $"MGS_CL_FACCAB({docEntry})",
            //    Token = token,
            //    Doc = JsonConvert.SerializeObject(factores, settings)
            //};

            //var result = await _documentService.UpdateInfo(requestInformation, "PYP", Empresa);
            //return Ok(result);
        }

        //[HttpPatch]
        //public async Task<ActionResult<ResponseInformation>> Patch(string docEntry, string Empresa, MatrizFactorDTO MatrizFactor)
        //{
        //    ResponseInformation rp = new ResponseInformation();

        //    rp = await _documentService.ValidaDatos(Empresa);
        //    if (!rp.Registered)
        //    {
        //        return BadRequest(rp);
        //    }

        //    string token = await _loginService.Login(Empresa);

        //    if (token != "")
        //    {
        //        var settings = new JsonSerializerSettings
        //        {
        //            NullValueHandling = NullValueHandling.Ignore
        //        };

        //        //string docEntry = await _documentService.GetOVByProject(docEntry, true, Empresa);
        //        //if (docEntry == "")
        //        //    return NotFound();

        //        RequestInformation requestInformation = new RequestInformation();
        //        requestInformation.Route = "MGS_CL_FACCAB(" + docEntry + ")";
        //        requestInformation.Token = token;
        //        requestInformation.Doc = JsonConvert.SerializeObject(MatrizFactor, settings);

        //        return Ok(await _documentService.UpdateInfo(requestInformation, "PYP", Empresa));

        //    }
        //    else
        //    {
        //        rp.Content = "";
        //        rp.Message = "No fue posible conectarse al SL";
        //        rp.Registered = false;
        //    }

        //    return BadRequest(rp);

        //}

        [HttpPost("crear")]
        public async Task<ActionResult<ResponseInformation>> Crear([FromQuery] string Empresa, [FromBody] MatrizFactorCreateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(Empresa))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "Parámetros incompletos para crear la matriz.",
                    Content = string.Empty
                });
            }

            if (string.IsNullOrWhiteSpace(request.U_MGS_CL_PERIODO))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "El periodo de destino es obligatorio.",
                    Content = string.Empty
                });
            }

            if (request.MGS_CL_FACDETCollection == null || request.MGS_CL_FACDETCollection.Count == 0)
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "No se enviaron registros de tiendas para crear la matriz.",
                    Content = string.Empty
                });
            }

            var prep = await _empresaRuntime.ResolveAndLoginAsync(Empresa);
            if (!prep.Ok) return BadRequest(prep.Error);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var requestInformation = new RequestInformation
            {
                Route = "MGS_CL_FACCAB",
                Token = prep.Token,
                Doc = JsonConvert.SerializeObject(request, settings)
            };

            var rp = await _documentService.PostInfo(requestInformation, "PYP", prep.Cfg);
            return Ok(rp);
        }
    }
}
