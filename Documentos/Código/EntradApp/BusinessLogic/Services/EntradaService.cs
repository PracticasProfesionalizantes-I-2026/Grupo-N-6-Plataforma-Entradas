using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Entrada;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EntradApp.BusinessLogic.Services;

public class EntradaService : IEntradaService
{
    private readonly IEntradaRepository _entradaRepository;
    private readonly ICompraRepository _compraRepository;

    public EntradaService(IEntradaRepository entradaRepository, ICompraRepository compraRepository)
    {
        _entradaRepository = entradaRepository;
        _compraRepository = compraRepository;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<PagedResult<EntradaResponseDTO>> GetMisEntradasAsync(Guid usuarioId, int page, int pageSize)
    {
        var result = await _entradaRepository.GetByUsuarioAsync(usuarioId, page, pageSize);
        var items = result.Items
            .Where(e => e.Estado == EstadoEntrada.Activa)
            .Select(MapToResponseDTO)
            .ToList();
        return new PagedResult<EntradaResponseDTO>(items, result.PageNumber, result.PageSize, result.TotalCount);
    }

    public async Task<PagedResult<EntradaResponseDTO>> GetUtilizadasAsync(Guid usuarioId, int page, int pageSize)
    {
        var result = await _entradaRepository.GetByUsuarioAsync(usuarioId, page, pageSize);
        var items = result.Items
            .Where(e => e.Estado == EstadoEntrada.Utilizada)
            .Select(MapToResponseDTO)
            .ToList();
        return new PagedResult<EntradaResponseDTO>(items, result.PageNumber, result.PageSize, result.TotalCount);
    }

    public async Task<PagedResult<EntradaResponseDTO>> GetDevueltasAsync(Guid usuarioId, int page, int pageSize)
    {
        var result = await _entradaRepository.GetByUsuarioAsync(usuarioId, page, pageSize);
        var items = result.Items
            .Where(e => e.Estado == EstadoEntrada.Devuelta)
            .Select(MapToResponseDTO)
            .ToList();
        return new PagedResult<EntradaResponseDTO>(items, result.PageNumber, result.PageSize, result.TotalCount);
    }

    public async Task<byte[]> GenerarComprobanteAsync(Guid usuarioId, Guid entradaId)
    {
        var entrada = await _entradaRepository.GetByIdAsync(entradaId);
        if (entrada == null) throw new NotFoundException("Entrada no encontrada");
        if (entrada.Compra.UsuarioId != usuarioId) throw new ForbiddenException();
        if (entrada.Estado != EstadoEntrada.Activa && entrada.Estado != EstadoEntrada.Utilizada)
            throw new ValidationException("No hay comprobante disponible para este estado");

        var compra = await _compraRepository.GetByIdAsync(entrada.CompraId);
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));
                
                page.Header().Column(col =>
                {
                    col.Item().Text("ENTRADAPP - Comprobante de Compra")
                        .FontSize(24).Bold().FontColor(Colors.Blue.Medium);
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Text($"Evento: {compra!.Evento!.Nombre}").FontSize(18).Bold();
                    col.Item().Text($"Fecha: {compra.Evento.FechaInicio:dd/MM/yyyy HH:mm}");
                    col.Item().Text($"Lugar: {compra.Evento.Lugar}, {compra.Evento.Direccion}");
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Light).MarginVertical(10);
                    
                    col.Item().Text($"Sector: {entrada.Sector!.Nombre}").FontSize(14).Bold();
                    col.Item().Text($"DNI: {entrada.Dni}");
                    col.Item().Text($"Código único: {entrada.CodigoUnico}").FontSize(12).FontColor(Colors.Grey.Dark);
                    col.Item().Text($"Estado: {entrada.Estado}");
                    
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Light).MarginVertical(10);
                    col.Item().Text($"Total pagado: ${compra.Total:F2}").FontSize(16).Bold().FontColor(Colors.Green.Medium);
                    col.Item().Text($"Fecha de compra: {compra.FechaCreacion:dd/MM/yyyy HH:mm}");
                });

                page.Footer().AlignCenter().Text("Este comprobante incluye código único alfanumérico para validación externa en control de acceso.")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }

    public async Task<Result<EntradaResponseDTO>> ValidarEntradaAsync(string codigoUnico)
    {
        var entrada = await _entradaRepository.GetByCodigoUnicoAsync(codigoUnico);
        if (entrada == null) return Result.Failure<EntradaResponseDTO>("Código de entrada inválido");
        if (entrada.Estado == EstadoEntrada.Utilizada) return Result.Failure<EntradaResponseDTO>("La entrada ya fue utilizada");
        if (entrada.Estado != EstadoEntrada.Activa) return Result.Failure<EntradaResponseDTO>("La entrada no está activa");

        await _entradaRepository.UpdateEstadoAsync(entrada.Id, EstadoEntrada.Utilizada);
        return Result.Success(MapToResponseDTO(entrada));
    }

    private static EntradaResponseDTO MapToResponseDTO(Entrada e) => new()
    {
        Id = e.Id,
        CompraId = e.CompraId,
        SectorId = e.SectorId,
        SectorNombre = e.Sector?.Nombre ?? "",
        Dni = e.Dni,
        CodigoUnico = e.CodigoUnico,
        Estado = e.Estado,
        FechaValidacion = e.FechaValidacion,
        FechaCreacion = e.FechaCreacion
    };
}