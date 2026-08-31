using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class SupplierModel
{
    public int Supp_Id { get; set; } = 0;
    public string? Supp_Code { get; set; }

    [Required(ErrorMessage = "Supplier Name is required.")]
    [StringLength(150, ErrorMessage = "Supplier Name cannot exceed 150 characters.")]
    public string Supp_Name { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Company Name cannot exceed 150 characters.")]
    public string? Supp_CompanyName { get; set; }

    [Required(ErrorMessage = "Mobile Number is required.")]
    [StringLength(15, MinimumLength = 10, ErrorMessage = "Mobile Number must be between 10 and 15 digits.")]
    public string Supp_MobileNo { get; set; } = string.Empty;

    [StringLength(15, ErrorMessage = "Alternate Mobile Number cannot exceed 15 digits.")]
    public string? Supp_AlternateMobileNo { get; set; }

    public string? Supp_Email { get; set; }

    [StringLength(20, ErrorMessage = "GST Number cannot exceed 20 characters.")]
    public string? Supp_GSTNo { get; set; }

    [StringLength(10, ErrorMessage = "PAN Number cannot exceed 10 characters.")]
    public string? Supp_PANNo { get; set; }

    [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
    public string? Supp_Address { get; set; }

    [Required(ErrorMessage = "Area is required.")]
    public int Supp_AreaId { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public int Supp_CityId { get; set; }

    [Required(ErrorMessage = "State is required.")]
    public int Supp_StateId { get; set; }

    [StringLength(10, ErrorMessage = "Pincode cannot exceed 10 characters.")]
    public string? Supp_Pincode { get; set; }

    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
    public string Supp_Country { get; set; } = "India";

    [StringLength(100)]
    public string? Supp_PaymentTerms { get; set; }

    public decimal Supp_CreditLimit { get; set; } = 0;
    public int Supp_CreditDays { get; set; } = 0;

    public bool Supp_IsActive { get; set; } = true;

    public int Supp_CreatedBy { get; set; } = 0;
    public DateTime? Supp_CreatedDate { get; set; }

    public int Supp_ModifiedBy { get; set; } = 0;
    public DateTime? Supp_ModifiedDate { get; set; }

    [Required(ErrorMessage = "Company is required.")]
    public int Supp_CompId { get; set; }

    [Required(ErrorMessage = "Branch is required.")]
    public int Supp_BranchId { get; set; }
}

public class SupplierListModel : SupplierModel
{
    public int? Supp_LeadgerId { get; set; }
    public string? Supp_AreaName { get; set; }
    public string? Supp_CityName { get; set; }
    public string? Supp_StateName { get; set; }
    public string? Supp_CompanyDisplayName { get; set; }
    public string? Supp_BranchName { get; set; }
}

public class SupplierFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? Supp_Id { get; set; }
    public bool? Supp_IsActive { get; set; }
}

public class SupplierPendingInvoiceFilterDto
{
    public int SupplierId { get; set; } = 1003;
    public int CompId { get; set; } = 0;
    public int BranchId { get; set; } = 0;
    public int LedgerId { get; set; } = 0;
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SupplierPendingInvoiceModel
{
    public int PurchaseMaster_Id { get; set; }
    public int PurchaseMaster_CompId { get; set; }
    public string? Comp_Name { get; set; }
    public int PurchaseMaster_BranchId { get; set; }
    public string? Branch_Name { get; set; }
    public int PurchaseMaster_SupplierId { get; set; }
    public string? Supp_Code { get; set; }
    public string? Supp_Name { get; set; }
    public int? AccLedger_Id { get; set; }
    public string? AccLedger_Name { get; set; }
    public int? PurchaseMaster_LedgerId { get; set; }
    public string? PurchaseMaster_InvoiceNo { get; set; }
    public DateTime? PurchaseMaster_InvoiceDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GSTAmount { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string? PurchaseMaster_Status { get; set; }
    public string? PurchaseMaster_Remark { get; set; }
    public DateTime? PurchaseMaster_CreatedDate { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class SupplierOutstandingReportFilterDto
{
    public int CompId { get; set; } = 0;
    public int BranchId { get; set; } = 0;
    public int LedgerId { get; set; } = 0;
    public int SupplierId { get; set; } = 1003;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SupplierOutstandingReportModel
{
    public int Supp_Id { get; set; }
    public string? Supp_Code { get; set; }
    public string? Supp_Name { get; set; }
    public string? Supp_CompanyName { get; set; }
    public string? Supp_MobileNo { get; set; }
    public string? Supp_AlternateMobileNo { get; set; }
    public string? Supp_Email { get; set; }
    public string? Supp_GSTNo { get; set; }
    public string? Supp_PANNo { get; set; }
    public int? Supp_AreaId { get; set; }
    public string? Supp_AreaName { get; set; }
    public int? Supp_CityId { get; set; }
    public string? Supp_CityName { get; set; }
    public int? Supp_StateId { get; set; }
    public string? Supp_StateName { get; set; }
    public string? Supp_Pincode { get; set; }
    public string? Supp_Country { get; set; }
    public int? Supp_CompId { get; set; }
    public string? Comp_Name { get; set; }
    public int? Supp_BranchId { get; set; }
    public string? Branch_Name { get; set; }
    public int? AccLedger_Id { get; set; }
    public string? AccLedger_Name { get; set; }
    public int? PurchaseLedgerId { get; set; }
    public int TotalPurchaseCount { get; set; }
    public decimal TotalSubTotal { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalGSTAmount { get; set; }
    public decimal TotalOtherCharges { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public bool? Supp_IsActive { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
