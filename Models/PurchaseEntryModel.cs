using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class PurchaseMasterModel
{
    public int PurchaseMaster_CompId { get; set; }
    public int PurchaseMaster_BranchId { get; set; }
    public int PurchaseMaster_SupplierId { get; set; }

    [StringLength(100)]
    public string? PurchaseMaster_InvoiceNo { get; set; }
    
    public DateTime? PurchaseMaster_InvoiceDate { get; set; }
    public decimal PurchaseMaster_SubTotal { get; set; }
    public decimal PurchaseMaster_DiscountAmount { get; set; }
    public decimal PurchaseMaster_GSTAmount { get; set; }
    public decimal PurchaseMaster_OtherCharges { get; set; }
    public decimal PurchaseMaster_NetAmount { get; set; }
    public decimal PurchaseMaster_PaidAmount { get; set; }
    public decimal PurchaseMaster_BalanceAmount { get; set; }
    
    [StringLength(30)]
    public string? PurchaseMaster_Status { get; set; }
    
    [StringLength(500)]
    public string? PurchaseMaster_Remark { get; set; }
    
    public int PurchaseMaster_CreatedBy { get; set; }
    public int PurchaseMaster_ModifiedBy { get; set; }
}

public class PurchaseDetailModel
{
    public int PurchaseDetail_CompId { get; set; }
    public int PurchaseDetail_BranchId { get; set; }
    public int PurchaseDetail_ProductId { get; set; }

    [StringLength(100)]
    public string? PurchaseDetail_Barcode { get; set; }

    [StringLength(100)]
    public string? PurchaseDetail_EANCode { get; set; }

    public decimal PurchaseDetail_Qty { get; set; }
    public decimal PurchaseDetail_LandingPrice { get; set; }
    public decimal PurchaseDetail_PurchasePrice { get; set; }
    public decimal PurchaseDetail_MRP { get; set; }
    public decimal PurchaseDetail_SellingPrice { get; set; }
    public decimal PurchaseDetail_DiscountPercent { get; set; }
    public decimal PurchaseDetail_DiscountAmount { get; set; }
    public decimal PurchaseDetail_GSTPercent { get; set; }
    public decimal PurchaseDetail_GSTAmount { get; set; }
    public decimal PurchaseDetail_TotalAmount { get; set; }
}

public class PurchaseEntrySaveRequest
{
    [Required]
    public PurchaseMasterModel MasterData { get; set; } = new();

    [Required]
    public List<PurchaseDetailModel> DetailData { get; set; } = new();
}

public class PurchaseEntrySaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PurchaseMaster_Id { get; set; }
}
