using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IUsuarioEspacioService
{
    // Crear UsuarioEspacio
    Task<UsuarioEspacioDto?> CrearUsuarioEspacioAsync(
        CreateUsuarioEspacioDto dto,
        CancellationToken ct = default
    );

    // Obtener UsuarioEspacio por Id
    Task<UsuarioEspacioDto?> ObtenerUsuarioEspacioAsync(
        string id,
        CancellationToken ct = default
    );

    // Obtener UsuariosEspacios por EspacioId
    Task<IEnumerable<UsuarioEspacioDto>> ObtenerPorEspacioAsync(
        string espacioId,
        CancellationToken ct = default
    );

    // Obtener UsuariosEspacios por UsuarioId
    Task<IEnumerable<UsuarioEspacioDto>> ObtenerPorUsuarioAsync(
        string usuarioId,
        CancellationToken ct = default
    );

    // Obtener todos
    Task<IEnumerable<UsuarioEspacioDto>> ListarTodasAsync(
        CancellationToken ct = default
    );

    // Actualizar UsuarioEspacio (overwrite completo)
    Task<UsuarioEspacioDto?> ActualizarUsuarioEspacioCompletoAsync(
        string id,
        UpdateUsuarioEspacioDto dto,
        CancellationToken ct = default
    );

    // Actualizar UsuarioEspacio (merge)
    Task<UsuarioEspacioDto?> ActualizarUsuarioEspacioMergeAsync(
        string id,
        UpdateUsuarioEspacioDto dto,
        CancellationToken ct = default
    );

    // Actualización parcial
    Task<UsuarioEspacioDto?> ActualizarUsuarioEspacioParcialAsync(
        string id,
        UpdateUsuarioEspacioDto dto,
        CancellationToken ct = default
    );

    // Eliminar UsuarioEspacio
    Task<bool> EliminarUsuarioEspacioAsync(
        string id,
        CancellationToken ct = default
    );
}
