using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IRouteRepository
{
    Task<ApiResponse<RouteSaveResult>> SaveRouteAsync(RouteModel route, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<RouteListModel>>> GetAllRoutesAsync(RouteFilterDto? filter = null, CancellationToken cancellationToken = default);
}
