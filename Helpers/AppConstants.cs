namespace Billing_Software_Api.Helpers;

/// <summary>
/// Common application constants.
/// </summary>
public static class AppConstants
{
    public static class Pagination
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
    }

    public static class Messages
    {
        public const string RecordNotFound = "The requested record was not found.";
        public const string CreatedSuccess = "Record created successfully.";
        public const string UpdatedSuccess = "Record updated successfully.";
        public const string DeletedSuccess = "Record deleted successfully.";
        public const string RetrievedSuccess = "Record retrieved successfully.";
        public const string ListRetrievedSuccess = "Records retrieved successfully.";
        public const string ValidationError = "One or more validation errors occurred.";
    }

    public static class Cors
    {
        public const string PolicyName = "BillingSoftwareCorsPolicy";
    }
}
