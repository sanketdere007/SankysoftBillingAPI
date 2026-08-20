using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class PaymentModel
{
    public int PaymentMaster_Id { get; set; }
    
    [Required]
    public int PaymentMaster_CompId { get; set; }
    
    [Required]
    public int PaymentMaster_BranchId { get; set; }

    public string? PaymentMaster_PaymentNo { get; set; }
    
    public DateTime? PaymentMaster_PaymentDate { get; set; }
    
    [StringLength(30)]
    public string? PaymentMaster_Type { get; set; }
    
    public decimal PaymentMaster_TotalAmount { get; set; }

    public int? PaymentMaster_InvoiceId { get; set; }
    
    [StringLength(100)]
    public string? PaymentMaster_InvoiceNo { get; set; }
    
    public DateTime? PaymentMaster_InvoiceDate { get; set; }

    public int? PaymentMaster_AccountId { get; set; }

    public decimal PaymentMaster_CashAmount { get; set; } = 0;
    public decimal PaymentMaster_UPIAmount { get; set; } = 0;
    public decimal PaymentMaster_ChequeAmount { get; set; } = 0;
    public decimal PaymentMaster_BankAmount { get; set; } = 0;
    public decimal PaymentMaster_CardAmount { get; set; } = 0;
    public decimal PaymentMaster_OtherAmount { get; set; } = 0;

    [StringLength(500)]
    public string? PaymentMaster_CashRemark { get; set; }

    [StringLength(200)]
    public string? PaymentMaster_UPITransactionNo { get; set; }
    
    [StringLength(200)]
    public string? PaymentMaster_UPIReferenceNo { get; set; }

    [StringLength(100)]
    public string? PaymentMaster_ChequeNo { get; set; }
    
    public DateTime? PaymentMaster_ChequeDate { get; set; }
    
    [StringLength(200)]
    public string? PaymentMaster_ChequeBankName { get; set; }
    
    [StringLength(200)]
    public string? PaymentMaster_ChequeBranchName { get; set; }

    [StringLength(20)]
    public string? PaymentMaster_BankTransferType { get; set; }
    
    [StringLength(200)]
    public string? PaymentMaster_BankName { get; set; }
    
    [StringLength(100)]
    public string? PaymentMaster_BankAccountNo { get; set; }
    
    [StringLength(200)]
    public string? PaymentMaster_BankTransactionNo { get; set; }
    
    [StringLength(200)]
    public string? PaymentMaster_BankReferenceNo { get; set; }
    
    public DateTime? PaymentMaster_BankDate { get; set; }

    [StringLength(100)]
    public string? PaymentMaster_OtherPaymentType { get; set; }
    
    [StringLength(200)]
    public string? PaymentMaster_OtherReferenceNo { get; set; }
    
    public DateTime? PaymentMaster_OtherDate { get; set; }
    
    [StringLength(500)]
    public string? PaymentMaster_OtherRemark { get; set; }

    [StringLength(500)]
    public string? PaymentMaster_Remark { get; set; }
    
    [StringLength(30)]
    public string? PaymentMaster_Status { get; set; }

    public int PaymentMaster_CreatedBy { get; set; }

    public int PaymentMaster_ModifiedBy { get; set; }
}

public class PaymentSaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PaymentMaster_Id { get; set; }
    public string? PaymentMaster_PaymentNo { get; set; }
}
