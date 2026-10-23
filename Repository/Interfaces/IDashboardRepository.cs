using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IDashboardRepository
{
    Task<ApiResponse<DashboardSummaryResponse>> GetDashboardSummaryAsync(DashboardSummaryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<DashboardChartsResponse>> GetDashboardChartsAsync(DashboardChartsRequest request, CancellationToken cancellationToken = default);
}
