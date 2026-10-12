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

public class PurchaseReturnMasterViewListRequest
{
    public int CompId { get; set; } = 1;
    public int BranchId { get; set; } = 1;
    public int? PurchaseReturnMaster_Id { get; set; }
    public int? SupplierId { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PurchaseReturnMasterViewListResponse
{
    public int PurchaseReturnMaster_Id { get; set; }
    public int PurchaseReturnMaster_CompId { get; set; }
    public int PurchaseReturnMaster_BranchId { get; set; }
    public int PurchaseReturnMaster_SupplierId { get; set; }
    public string? SupplierCode { get; set; }
    public string? SupplierMobile { get; set; }
    public int? PurchaseReturnMaster_LedgerId { get; set; }
    public string? LedgerName { get; set; }
    public string? PurchaseReturnMaster_ReturnNo { get; set; }
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
    public string? PurchaseReturnMaster_Status { get; set; }
    public string? PurchaseReturnMaster_Remark { get; set; }
    public int PurchaseReturnMaster_CreatedBy { get; set; }
    public DateTime? PurchaseReturnMaster_CreatedDate { get; set; }
    public int PurchaseReturnMaster_ModifiedBy { get; set; }
    public DateTime? PurchaseReturnMaster_ModifiedDate { get; set; }
}

public class PurchaseReturnDetailViewListRequest
{
    public int PurchaseReturnMaster_Id { get; set; }
    public int CompId { get; set; } = 1;
    public int BranchId { get; set; } = 1;
}

public class PurchaseReturnDetailViewListResponse
{
    public int PurchaseReturnDetail_Id { get; set; }
    public int PurchaseReturnDetail_MasterId { get; set; }
    public int PurchaseReturnDetail_CompId { get; set; }
    public int PurchaseReturnDetail_BranchId { get; set; }
    public int PurchaseReturnDetail_ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int PurchaseReturnDetail_BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public decimal? BatchStock { get; set; }
    public decimal? BatchAvailableStock { get; set; }
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
    public DateTime? PurchaseReturnDetail_CreatedDate { get; set; }
    public int PurchaseReturnDetail_ModifiedBy { get; set; }
    public DateTime? PurchaseReturnDetail_ModifiedDate { get; set; }
}
