using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class SalesReturnMasterModel
{
    public int SalesReturnMaster_Id { get; set; }
    public int SalesReturnMaster_CompId { get; set; }
    public int SalesReturnMaster_BranchId { get; set; }
    public DateTime? SalesReturnMaster_Date { get; set; }
    public int? SalesReturnMaster_CustomerId { get; set; }
    public int? SalesReturnMaster_LedgerId { get; set; }
    public decimal SalesReturnMaster_TotalQty { get; set; }
    public decimal SalesReturnMaster_SubTotal { get; set; }
    public decimal SalesReturnMaster_DiscountAmount { get; set; }
    public decimal SalesReturnMaster_TaxAmount { get; set; }
    public decimal SalesReturnMaster_BillWiseDiscountPercentage { get; set; }
    public decimal SalesReturnMaster_BillWiseDiscountAmount { get; set; }
    public decimal SalesReturnMaster_GrandTotal { get; set; }
    public decimal SalesReturnMaster_PaidAmount { get; set; }
    public decimal SalesReturnMaster_BalanceAmount { get; set; }
    public decimal SalesReturnMaster_CashAmount { get; set; }
    public decimal SalesReturnMaster_UPIAmount { get; set; }
    public decimal SalesReturnMaster_ChequeAmount { get; set; }
    public decimal SalesReturnMaster_CreditAmount { get; set; }
    
    [StringLength(50)]
    public string? SalesReturnMaster_Status { get; set; }
    
    [StringLength(500)]
    public string? SalesReturnMaster_Remark { get; set; }
    public bool? SalesReturnMaster_IsActive { get; set; }
    
    public int SalesReturnMaster_CreatedBy { get; set; }
    public int? SalesReturnMaster_ModifiedBy { get; set; }
}

public class SalesReturnDetailModel
{
    public int SalesReturnDetail_ProductId { get; set; }
    public int? SalesReturnDetail_BatchId { get; set; }
    public int? SalesReturnDetail_UnitId { get; set; }
    public decimal SalesReturnDetail_Qty { get; set; }
    public decimal SalesReturnDetail_FreeQty { get; set; }
    public decimal SalesReturnDetail_SellingPrice { get; set; }
    public decimal SalesReturnDetail_MRP { get; set; }
    public decimal SalesReturnDetail_DiscountPercentage { get; set; }
    public decimal SalesReturnDetail_DiscountAmount { get; set; }
    public decimal SalesReturnDetail_TaxPercentage { get; set; }
    public decimal SalesReturnDetail_TaxAmount { get; set; }
    public decimal SalesReturnDetail_SubTotal { get; set; }
    public decimal SalesReturnDetail_TotalAmount { get; set; }
    
    [StringLength(500)]
    public string? SalesReturnDetail_Remark { get; set; }
    public bool? SalesReturnDetail_IsActive { get; set; }
    public int? SalesReturnDetail_ModifiedBy { get; set; }
}

public class SalesReturnEntrySaveRequest
{
    [Required]
    public SalesReturnMasterModel MasterData { get; set; } = new();

    [Required]
    public List<SalesReturnDetailModel> DetailData { get; set; } = new();
}

public class SalesReturnEntrySaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SalesReturnMaster_Id { get; set; }
    public string? SalesReturnMaster_InvoiceNo { get; set; }
}

public class SalesReturnEntryDeleteResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? SalesReturnMaster_Id { get; set; }
    public int? ErrorNumber { get; set; }
    public int? ErrorLine { get; set; }
}

public class SalesReturnListRequest
{
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class SalesReturnListResponse
{
    public int SalesReturnMaster_Id { get; set; }
    public int SalesReturnMaster_CompId { get; set; }
    public int SalesReturnMaster_BranchId { get; set; }
    public string? SalesReturnMaster_InvoiceNo { get; set; }
    public DateTime? SalesReturnMaster_Date { get; set; }
    public int? SalesReturnMaster_CustomerId { get; set; }
    public int? SalesReturnMaster_LedgerId { get; set; }
    public decimal SalesReturnMaster_TotalQty { get; set; }
    public decimal SalesReturnMaster_SubTotal { get; set; }
    public decimal SalesReturnMaster_DiscountAmount { get; set; }
    public decimal SalesReturnMaster_TaxAmount { get; set; }
    public decimal SalesReturnMaster_BillWiseDiscountPercentage { get; set; }
    public decimal SalesReturnMaster_BillWiseDiscountAmount { get; set; }
    public decimal SalesReturnMaster_GrandTotal { get; set; }
    public decimal SalesReturnMaster_PaidAmount { get; set; }
    public decimal SalesReturnMaster_BalanceAmount { get; set; }
    public decimal SalesReturnMaster_CashAmount { get; set; }
    public decimal SalesReturnMaster_UPIAmount { get; set; }
    public decimal SalesReturnMaster_ChequeAmount { get; set; }
    public decimal SalesReturnMaster_CreditAmount { get; set; }
    public string? SalesReturnMaster_Status { get; set; }
    public string? SalesReturnMaster_Remark { get; set; }
    public bool? SalesReturnMaster_IsActive { get; set; }
    public int SalesReturnMaster_CreatedBy { get; set; }
    public DateTime? SalesReturnMaster_CreatedDate { get; set; }
    public int? SalesReturnMaster_ModifiedBy { get; set; }
    public DateTime? SalesReturnMaster_ModifiedDate { get; set; }
    public string? Cust_Name { get; set; }
    public string? Cust_MobileNo { get; set; }
}

public class SalesReturnEntryDetailResponse
{
    public int SalesReturnDetail_Id { get; set; }
    public int SalesReturnDetail_MasterId { get; set; }
    public int SalesReturnDetail_ProductId { get; set; }
    public string? Prod_Name { get; set; }
    public string? Prod_Code { get; set; }
    public int? SalesReturnDetail_BatchId { get; set; }
    public int? SalesReturnDetail_UnitId { get; set; }
    public decimal SalesReturnDetail_Qty { get; set; }
    public decimal SalesReturnDetail_FreeQty { get; set; }
    public decimal SalesReturnDetail_SellingPrice { get; set; }
    public decimal SalesReturnDetail_MRP { get; set; }
    public decimal SalesReturnDetail_DiscountPercentage { get; set; }
    public decimal SalesReturnDetail_DiscountAmount { get; set; }
    public decimal SalesReturnDetail_TaxPercentage { get; set; }
    public decimal SalesReturnDetail_TaxAmount { get; set; }
    public decimal SalesReturnDetail_SubTotal { get; set; }
    public decimal SalesReturnDetail_TotalAmount { get; set; }
    public string? SalesReturnDetail_Remark { get; set; }
    public bool? SalesReturnDetail_IsActive { get; set; }
    public DateTime? SalesReturnDetail_CreatedDate { get; set; }
    public int? SalesReturnDetail_ModifiedBy { get; set; }
    public DateTime? SalesReturnDetail_ModifiedDate { get; set; }
}
