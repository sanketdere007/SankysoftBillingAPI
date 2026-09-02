using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class ReceiptMasterModel
{
    public int? ReceiptMaster_Id { get; set; }

    public int ReceiptMaster_CompId { get; set; }
    public int ReceiptMaster_BranchId { get; set; }

    public DateTime? ReceiptMaster_ReceiptDate { get; set; }

    public int? ReceiptMaster_CustomerId { get; set; }
    public int? ReceiptMaster_LedgerId { get; set; }

    public decimal ReceiptMaster_TotalAmount { get; set; }

    public decimal ReceiptMaster_CashAmount { get; set; }
    public decimal ReceiptMaster_UPIAmount { get; set; }
    public decimal ReceiptMaster_CardAmount { get; set; }
    public decimal ReceiptMaster_ChequeAmount { get; set; }
    public decimal ReceiptMaster_BankAmount { get; set; }
    public decimal ReceiptMaster_OtherAmount { get; set; }

    [StringLength(100)]
    public string? ReceiptMaster_ChequeNo { get; set; }
    public DateTime? ReceiptMaster_ChequeDate { get; set; }

    [StringLength(200)]
    public string? ReceiptMaster_BankName { get; set; }
    [StringLength(100)]
    public string? ReceiptMaster_BankReferenceNo { get; set; }

    [StringLength(50)]
    public string? ReceiptMaster_NEFTType { get; set; }
    [StringLength(100)]
    public string? ReceiptMaster_NEFTReferenceNo { get; set; }

    [StringLength(100)]
    public string? ReceiptMaster_OtherPaymentType { get; set; }
    [StringLength(100)]
    public string? ReceiptMaster_OtherReferenceNo { get; set; }
    public DateTime? ReceiptMaster_OtherDate { get; set; }
    [StringLength(500)]
    public string? ReceiptMaster_OtherRemark { get; set; }

    [StringLength(1000)]
    public string? ReceiptMaster_Remark { get; set; }

    [StringLength(50)]
    public string? ReceiptMaster_Status { get; set; }
    public bool? ReceiptMaster_IsActive { get; set; }

    public int ReceiptMaster_CreatedBy { get; set; }
    public int ReceiptMaster_ModifiedBy { get; set; }
}

public class ReceiptDetailModel
{
    public int ReceiptDetail_CompId { get; set; }
    public int ReceiptDetail_BranchId { get; set; }

    public int? ReceiptDetail_CustomerId { get; set; }
    public int? ReceiptDetail_LedgerId { get; set; }

    public decimal ReceiptDetail_InvoiceAmount { get; set; }
    public decimal ReceiptDetail_PendingAmount { get; set; }

    public decimal ReceiptDetail_ReceivedAmount { get; set; }
    public decimal ReceiptDetail_RemainingAmount { get; set; }

    [StringLength(500)]
    public string? ReceiptDetail_Remark { get; set; }

    public int ReceiptDetail_CreatedBy { get; set; }
    public int ReceiptDetail_ModifiedBy { get; set; }
}

public class ReceiptEntrySaveRequest
{
    [Required]
    public ReceiptMasterModel MasterData { get; set; } = new();

    [Required]
    public List<ReceiptDetailModel> DetailData { get; set; } = new();
}

public class ReceiptEntrySaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ReceiptMaster_Id { get; set; }
    public string? ReceiptMaster_ReceiptNo { get; set; }
}

public class CollectionReportRequest
{
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? PaymentMode { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CollectionReportResponse
{
    public int? ReceiptMaster_Id { get; set; }
    public string? ReceiptMaster_ReceiptNo { get; set; }
    public DateTime? ReceiptMaster_ReceiptDate { get; set; }
    public string? ReceiptMaster_Status { get; set; }
    public bool? ReceiptMaster_IsActive { get; set; }

    public int? ReceiptMaster_CompId { get; set; }
    public int? Comp_Id { get; set; }
    public string? Comp_Name { get; set; }

    public int? ReceiptMaster_BranchId { get; set; }
    public int? Branch_Id { get; set; }
    public string? Branch_Name { get; set; }

    public int? ReceiptMaster_CustomerId { get; set; }
    public string? Cust_Code { get; set; }
    public string? Cust_Name { get; set; }
    public string? Cust_MobileNo { get; set; }
    public string? Cust_Email { get; set; }

    public int? ReceiptMaster_LedgerId { get; set; }
    public string? AccLedger_Name { get; set; }

    public decimal? TotalCollection { get; set; }
    public decimal? CashAmount { get; set; }
    public decimal? UPIAmount { get; set; }
    public decimal? CardAmount { get; set; }
    public decimal? ChequeAmount { get; set; }
    public decimal? BankAmount { get; set; }
    public decimal? OtherAmount { get; set; }

    public string? ReceiptMaster_ChequeNo { get; set; }
    public DateTime? ReceiptMaster_ChequeDate { get; set; }

    public string? ReceiptMaster_BankName { get; set; }
    public string? ReceiptMaster_BankReferenceNo { get; set; }
    public string? ReceiptMaster_NEFTType { get; set; }
    public string? ReceiptMaster_NEFTReferenceNo { get; set; }

    public string? ReceiptMaster_OtherPaymentType { get; set; }
    public string? ReceiptMaster_OtherReferenceNo { get; set; }
    public DateTime? ReceiptMaster_OtherDate { get; set; }
    public string? ReceiptMaster_OtherRemark { get; set; }

    public string? ReceiptMaster_Remark { get; set; }

    public int? ReceiptMaster_CreatedBy { get; set; }
    public DateTime? ReceiptMaster_CreatedDate { get; set; }
    public int? ReceiptMaster_ModifiedBy { get; set; }
    public DateTime? ReceiptMaster_ModifiedDate { get; set; }

    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
