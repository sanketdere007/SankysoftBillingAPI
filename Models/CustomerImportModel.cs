using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class CustomerImportExcelDto
{
    public int SrNo { get; set; }
    public string? Name { get; set; }
    public string? Number { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Village { get; set; }
    public string? Taluka { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? FarmerType { get; set; }
    public string? AnimalType { get; set; }
    public int? AnimalCount { get; set; }
    public string? BirdType { get; set; }
    public int? BirdCount { get; set; }
}

public class CustomerImportRequestDto
{
    [Required]
    public List<CustomerImportExcelDto> Data { get; set; } = new();

    public int CompId { get; set; } = 1;
    public int BranchId { get; set; } = 1;
    public int CreatedBy { get; set; } = 1;
}

public class CustomerImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int InsertedRecords { get; set; }
    public int DuplicateRecords { get; set; }
}
