using System.Threading.Tasks;
using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repositories
{
    public interface IReportRepository
    {
        Task<OutstandingReceivableResponse> GetOutstandingReceivableReportAsync(OutstandingReceivableRequest request);
    }
}
