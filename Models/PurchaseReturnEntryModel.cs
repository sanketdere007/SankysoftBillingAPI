using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class PurchaseReturnMasterModel
{
    public int PurchaseReturnMaster_Id { get; set; }
    public int PurchaseReturnMaster_CompId { get; set; }
    public int PurchaseReturnMaster_BranchId { get; set; }
    public int PurchaseReturnMaster_SupplierId { get; set; }
    public int? PurchaseReturnMaster_LedgerId { get; set; }

    public DateTime? PurchaseReturnMaster_ReturnDate { get; set; }

    public decimal PurchaseReturnMaster_SubTotal { get; set; }
    public decimal PurchaseReturnMaster_DiscountAmount { get; set; }
    public decimal? PurchaseReturnMaster_BillWiseDiscountPercentage { get; set; }
    public decimal? PurchaseReturnMaster_BillWiseDiscountAmount { get; set; }

    public decimal PurchaseReturnMaster_GSTAmount { get; set; }
    public decimal PurchaseReturnMaster_OtherCharges { get; set; }
    public decimal PurchaseReturnMaster_NetAmount { get; set; }
    public decimal PurchaseReturnMaster_RefundAmount { get; set; }
    public decimal PurchaseReturnMaster_BalanceAmount { get; set; }

    [StringLength(30)]
    public string? PurchaseReturnMaster_Status { get; set; }
    
    [StringLength(500)]
    public string? PurchaseReturnMaster_Remark { get; set; }

    public int PurchaseReturnMaster_CreatedBy { get; set; }
    public int PurchaseReturnMaster_ModifiedBy { get; set; }
}

public class PurchaseReturnDetailModel
{
    public int PurchaseReturnDetail_ProductId { get; set; }
    public int PurchaseReturnDetail_BatchId { get; set; }
    public decimal PurchaseReturnDetail_Qty { get; set; }

    public decimal PurchaseReturnDetail_LandingPrice { get; set; }
    public decimal PurchaseReturnDetail_PurchasePrice { get; set; }
    public decimal PurchaseReturnDetail_MRP { get; set; }
    public decimal PurchaseReturnDetail_SellingPrice { get; set; }

    public decimal PurchaseReturnDetail_DiscountPercent { get; set; }
    public decimal PurchaseReturnDetail_DiscountAmount { get; set; }

    public decimal PurchaseReturnDetail_GSTPercent { get; set; }
    public decimal PurchaseReturnDetail_GSTAmount { get; set; }

    public decimal PurchaseReturnDetail_TotalAmount { get; set; }

    public int PurchaseReturnDetail_CreatedBy { get; set; }
}

public class PurchaseReturnEntrySaveRequest
{
    [Required]
    public PurchaseReturnMasterModel MasterData { get; set; } = new();

    [Required]
    public List<PurchaseReturnDetailModel> DetailData { get; set; } = new();
}

public class PurchaseReturnEntrySaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PurchaseReturnMaster_Id { get; set; }
    public string? PurchaseReturnMaster_ReturnNo { get; set; }
}
