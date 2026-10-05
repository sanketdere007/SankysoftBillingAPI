using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new System.ArgumentNullException("Connection string 'DefaultConnection' is not found.");
        }

        public async Task<OutstandingReceivableResponse> GetOutstandingReceivableReportAsync(OutstandingReceivableRequest request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CompId", request.CompId);
                parameters.Add("@BranchId", request.BranchId);
                parameters.Add("@FromDate", request.FromDate);
                parameters.Add("@ToDate", request.ToDate);
                parameters.Add("@Search", request.Search);
                parameters.Add("@CustomerId", request.CustomerId);
                parameters.Add("@RouteId", request.RouteId);
                parameters.Add("@AreaId", request.AreaId);
                parameters.Add("@CityId", request.CityId);
                parameters.Add("@StateId", request.StateId);
                parameters.Add("@OnlyOutstanding", request.OnlyOutstanding);

                var response = new OutstandingReceivableResponse();

                // Call the Stored Procedure using QueryMultipleAsync
                using (var multi = await db.QueryMultipleAsync("SP_OutstandingReceivableReport", parameters, commandType: CommandType.StoredProcedure))
                {
                    // Result 1: Invoice Detail
                    var invoiceDetails = await multi.ReadAsync<InvoiceDetailDto>();
                    response.InvoiceDetails = invoiceDetails.ToList();

                    // Result 2: Party Total
                    var partyTotals = await multi.ReadAsync<PartyTotalDto>();
                    response.PartyTotals = partyTotals.ToList();

                    // Result 3: Grand Total
                    var grandTotal = await multi.ReadFirstOrDefaultAsync<GrandTotalDto>();
                    if (grandTotal != null)
                    {
                        response.GrandTotal = grandTotal;
                    }
                }

                return response;
            }
        }
    }
}
