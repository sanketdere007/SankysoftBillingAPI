using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IPaymentRepository
{
    Task<ApiResponse<PaymentSaveResult>> SavePaymentEntryAsync(PaymentModel payment, CancellationToken cancellationToken = default);
}
