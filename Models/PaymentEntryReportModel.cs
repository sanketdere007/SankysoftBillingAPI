using System;

namespace Billing_Software_Api.Models;

public class PaymentEntryReportFilterDto
{
    public int CompId { get; set; } = 1;
    public int BranchId { get; set; } = 1;

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int? PaymentMaster_Id { get; set; }
    public string? PaymentNo { get; set; }
    public string? InvoiceNo { get; set; }
    public string? Search { get; set; }

    public string? PaymentType { get; set; }
    public string? Status { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PaymentEntryReportModel
{
    public int PaymentMaster_Id { get; set; }
    public string? PaymentMaster_PaymentNo { get; set; }
    public DateTime? PaymentMaster_PaymentDate { get; set; }
    public string? PaymentMaster_Type { get; set; }
    public string? PaymentMaster_Status { get; set; }
    public int? PaymentMaster_InvoiceId { get; set; }
    public string? PaymentMaster_InvoiceNo { get; set; }
    public DateTime? PaymentMaster_InvoiceDate { get; set; }
    public int? PaymentMaster_AccountId { get; set; }
    
    public decimal PaymentMaster_TotalAmount { get; set; }
    public decimal PaymentMaster_CashAmount { get; set; }
    public decimal PaymentMaster_UPIAmount { get; set; }
    public decimal PaymentMaster_ChequeAmount { get; set; }
    public decimal PaymentMaster_BankAmount { get; set; }
    public decimal PaymentMaster_CardAmount { get; set; }
    public decimal PaymentMaster_OtherAmount { get; set; }
    
    public string? PaymentModes { get; set; }
    public string? PaymentMaster_CashRemark { get; set; }
    public string? PaymentMaster_UPITransactionNo { get; set; }
    public string? PaymentMaster_UPIReferenceNo { get; set; }
    public string? PaymentMaster_ChequeNo { get; set; }
    public DateTime? PaymentMaster_ChequeDate { get; set; }
    public string? PaymentMaster_ChequeBankName { get; set; }
    public string? PaymentMaster_ChequeBranchName { get; set; }
    public string? PaymentMaster_BankTransferType { get; set; }
    public string? PaymentMaster_BankName { get; set; }
    public string? PaymentMaster_BankAccountNo { get; set; }
    public string? PaymentMaster_BankTransactionNo { get; set; }
    public string? PaymentMaster_BankReferenceNo { get; set; }
    public DateTime? PaymentMaster_BankDate { get; set; }
    public string? PaymentMaster_Remark { get; set; }
    
    public int? PaymentMaster_CreatedBy { get; set; }
    public DateTime? PaymentMaster_CreatedDate { get; set; }
    public int? PaymentMaster_ModifiedBy { get; set; }
    public DateTime? PaymentMaster_ModifiedDate { get; set; }
}
