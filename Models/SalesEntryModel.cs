using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class SalesMasterModel
{
    public int SalesMaster_Id { get; set; }

    public int SalesMaster_CompId { get; set; }
    public int SalesMaster_BranchId { get; set; }

    public int? SalesMaster_CustomerId { get; set; }
    public int? SalesMaster_LedgerId { get; set; }

    public DateTime? SalesMaster_InvoiceDate { get; set; }

    public decimal SalesMaster_TotalQty { get; set; }
    public decimal SalesMaster_SubTotal { get; set; }
    public decimal SalesMaster_TotalDiscount { get; set; }
    public decimal SalesMaster_TotalTaxableAmount { get; set; }

    public decimal SalesMaster_TotalCGST { get; set; }
    public decimal SalesMaster_TotalSGST { get; set; }
    public decimal SalesMaster_TotalIGST { get; set; }
    public decimal SalesMaster_TotalCESS { get; set; }

    public decimal SalesMaster_RoundOff { get; set; }
    public decimal SalesMaster_GrandTotal { get; set; }

    public decimal SalesMaster_PaidAmount { get; set; }
    public decimal SalesMaster_BalanceAmount { get; set; }

    public decimal SalesMaster_CashAmount { get; set; }
    public decimal SalesMaster_UPIAmount { get; set; }
    public decimal SalesMaster_CardAmount { get; set; }
    public decimal SalesMaster_ChequeAmount { get; set; }
    public decimal SalesMaster_BankAmount { get; set; }
    public decimal SalesMaster_OtherAmount { get; set; }
    public decimal SalesMaster_CreditAmt { get; set; }

    [StringLength(100)]
    public string? SalesMaster_ChequeNo { get; set; }
    public DateTime? SalesMaster_ChequeDate { get; set; }

    [StringLength(200)]
    public string? SalesMaster_BankName { get; set; }
    [StringLength(100)]
    public string? SalesMaster_BankReferenceNo { get; set; }
    [StringLength(50)]
    public string? SalesMaster_NEFTType { get; set; }
    [StringLength(100)]
    public string? SalesMaster_NEFTReferenceNo { get; set; }

    [StringLength(100)]
    public string? SalesMaster_OtherPaymentType { get; set; }
    [StringLength(100)]
    public string? SalesMaster_OtherReferenceNo { get; set; }
    public DateTime? SalesMaster_OtherDate { get; set; }
    [StringLength(500)]
    public string? SalesMaster_OtherRemark { get; set; }

    [StringLength(200)]
    public string? SalesMaster_BillingName { get; set; }
    [StringLength(500)]
    public string? SalesMaster_BillingAddress { get; set; }
    [StringLength(20)]
    public string? SalesMaster_BillingMobileNo { get; set; }
    [StringLength(50)]
    public string? SalesMaster_BillingGSTNo { get; set; }

    public int? SalesMaster_BillingStateId { get; set; }
    public int? SalesMaster_BillingCityId { get; set; }
    public int? SalesMaster_BillingAreaId { get; set; }

    [StringLength(200)]
    public string? SalesMaster_ShippingName { get; set; }
    [StringLength(500)]
    public string? SalesMaster_ShippingAddress { get; set; }
    [StringLength(20)]
    public string? SalesMaster_ShippingMobileNo { get; set; }
    [StringLength(50)]
    public string? SalesMaster_ShippingGSTNo { get; set; }

    public int? SalesMaster_ShippingStateId { get; set; }
    public int? SalesMaster_ShippingCityId { get; set; }
    public int? SalesMaster_ShippingAreaId { get; set; }

    [StringLength(1000)]
    public string? SalesMaster_Remark { get; set; }

    [StringLength(50)]
    public string? SalesMaster_Status { get; set; }
    public bool? SalesMaster_IsActive { get; set; }

    public int SalesMaster_CreatedBy { get; set; }
    public int SalesMaster_ModifiedBy { get; set; }
}

public class SalesEntryDetailModel
{
    public int SalesEntryDetail_CompId { get; set; }
    public int SalesEntryDetail_BranchId { get; set; }
    public int SalesEntryDetail_ProductId { get; set; }
    public int? SalesEntryDetail_BatchId { get; set; }

    [StringLength(300)]
    public string? SalesEntryDetail_ProductName { get; set; }
    [StringLength(100)]
    public string? SalesEntryDetail_Barcode { get; set; }
    [StringLength(100)]
    public string? SalesEntryDetail_EANCode { get; set; }
    [StringLength(100)]
    public string? SalesEntryDetail_HSNCode { get; set; }
    public int? SalesEntryDetail_UnitId { get; set; }

    public decimal SalesEntryDetail_Qty { get; set; }
    public decimal SalesEntryDetail_FreeQty { get; set; }
    public decimal SalesEntryDetail_TotalQty { get; set; }

    public decimal SalesEntryDetail_MRP { get; set; }
    public decimal SalesEntryDetail_SellingPrice { get; set; }
    public decimal SalesEntryDetail_Rate { get; set; }

    public decimal SalesEntryDetail_DiscountPercentage { get; set; }
    public decimal SalesEntryDetail_DiscountAmount { get; set; }

    public decimal SalesEntryDetail_TaxableAmount { get; set; }

    public decimal SalesEntryDetail_GSTPercentage { get; set; }
    public decimal SalesEntryDetail_CGSTPercentage { get; set; }
    public decimal SalesEntryDetail_SGSTPercentage { get; set; }
    public decimal SalesEntryDetail_IGSTPercentage { get; set; }
    public decimal SalesEntryDetail_CESSPercentage { get; set; }

    public decimal SalesEntryDetail_CGSTAmount { get; set; }
    public decimal SalesEntryDetail_SGSTAmount { get; set; }
    public decimal SalesEntryDetail_IGSTAmount { get; set; }
    public decimal SalesEntryDetail_CESSAmount { get; set; }
    public decimal SalesEntryDetail_TotalTaxAmount { get; set; }

    public decimal SalesEntryDetail_TotalAmount { get; set; }

    public decimal SalesEntryDetail_LandingPrice { get; set; }
    public decimal SalesEntryDetail_PurchasePrice { get; set; }

    [StringLength(500)]
    public string? SalesEntryDetail_Remark { get; set; }

    public int SalesDetail_CreatedBy { get; set; }
    public int SalesDetail_ModifiedBy { get; set; }
}

public class SalesEntrySaveRequest
{
    [Required]
    public SalesMasterModel MasterData { get; set; } = new();

    [Required]
    public List<SalesEntryDetailModel> DetailData { get; set; } = new();
}

public class SalesEntrySaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SalesMaster_Id { get; set; }
    public string? SalesMaster_InvoiceNo { get; set; }
}
