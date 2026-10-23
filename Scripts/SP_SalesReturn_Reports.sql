USE [SankysoftBillingDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================================================
-- 1. Product-Wise Sales Return Report
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_ProductWise_SalesReturnReport
(
    @CompId       INT            = 1,
    @BranchId     INT            = 1,

    @FromDate     DATE           = NULL,
    @ToDate       DATE           = NULL,

    @ProductId    INT            = NULL,

    @Search       NVARCHAR(200)  = NULL,
 
    @Status       NVARCHAR(50)   = NULL,
    @IsActive     BIT            = 1,

    @PageNumber   INT            = 1,
    @PageSize     INT            = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    ------------------------------------------------------------
    -- 1. NORMALIZE PARAMETERS
    ------------------------------------------------------------
    SET @PageNumber = CASE
        WHEN ISNULL(@PageNumber, 0) < 1 THEN 1
        ELSE @PageNumber
    END;

    SET @PageSize = CASE
        WHEN ISNULL(@PageSize, 0) < 1 THEN 50
        WHEN @PageSize > 500 THEN 500
        ELSE @PageSize
    END;

    SET @Search = NULLIF(LTRIM(RTRIM(@Search)), N'');
    SET @Status = NULLIF(LTRIM(RTRIM(@Status)), N'');

    ------------------------------------------------------------
    -- 2. FILTER AND AGGREGATE PRODUCT-WISE RETURNS
    ------------------------------------------------------------
    ;WITH FilteredReturns AS
    (
        SELECT
            M.SalesReturnMaster_Id,
            M.SalesReturnMaster_CompId,
            M.SalesReturnMaster_BranchId,
            M.SalesReturnMaster_InvoiceNo,
            M.SalesReturnMaster_Date,
            M.SalesReturnMaster_CustomerId,
            M.SalesReturnMaster_LedgerId,
            M.SalesReturnMaster_Status,
            M.SalesReturnMaster_Remark,

            D.SalesReturnDetail_ProductId,
            D.SalesReturnDetail_BatchId,
            D.SalesReturnDetail_UnitId,
            ISNULL(D.SalesReturnDetail_Qty, 0) AS ReturnQty,
            ISNULL(D.SalesReturnDetail_FreeQty, 0) AS FreeQty,
            ISNULL(D.SalesReturnDetail_SellingPrice, 0) AS SellingPrice,
            ISNULL(D.SalesReturnDetail_MRP, 0) AS MRP,
            ISNULL(D.SalesReturnDetail_DiscountAmount, 0) AS DiscountAmount,
            ISNULL(D.SalesReturnDetail_TaxAmount, 0) AS TaxAmount,
            ISNULL(D.SalesReturnDetail_SubTotal, 0) AS SubTotal,
            ISNULL(D.SalesReturnDetail_TotalAmount, 0) AS TotalAmount

        FROM dbo.tbl_SalesReturnEntryMaster AS M

        INNER JOIN dbo.tbl_SalesReturnEntryDetail AS D
            ON D.SalesReturnDetail_MasterId =
               M.SalesReturnMaster_Id

        WHERE
            (@CompId IS NULL
             OR M.SalesReturnMaster_CompId = @CompId)

            AND
            (@BranchId IS NULL
             OR M.SalesReturnMaster_BranchId = @BranchId)

            AND
            (@FromDate IS NULL
             OR M.SalesReturnMaster_Date >= @FromDate)

            AND
            (@ToDate IS NULL
             OR M.SalesReturnMaster_Date <
                DATEADD(DAY, 1, @ToDate))

            AND
            (@ProductId IS NULL
             OR D.SalesReturnDetail_ProductId = @ProductId)

            AND
            (@Status IS NULL
             OR M.SalesReturnMaster_Status = @Status)

            AND
            (@IsActive IS NULL
             OR M.SalesReturnMaster_IsActive = @IsActive)

            AND
            (
                @Search IS NULL
                OR M.SalesReturnMaster_InvoiceNo LIKE
                   N'%' + @Search + N'%'
                OR M.SalesReturnMaster_Remark LIKE
                   N'%' + @Search + N'%'
            )

            AND ISNULL(D.SalesReturnDetail_IsActive, 1) = 1
    ),
    ProductSummary AS
    (
        SELECT
            F.SalesReturnDetail_ProductId AS ProductId,

            COUNT(DISTINCT F.SalesReturnMaster_Id)
                AS TotalReturnEntries,

            COUNT(DISTINCT F.SalesReturnDetail_BatchId)
                AS TotalBatches,

            MIN(F.SalesReturnMaster_Date) AS FirstReturnDate,
            MAX(F.SalesReturnMaster_Date) AS LastReturnDate,

            SUM(F.ReturnQty) AS TotalReturnQty,
            SUM(F.FreeQty) AS TotalFreeQty,
            SUM(F.ReturnQty + F.FreeQty) AS TotalReturnedUnits,

            SUM(F.SubTotal) AS TotalSubTotal,
            SUM(F.DiscountAmount) AS TotalDiscountAmount,
            SUM(F.TaxAmount) AS TotalTaxAmount,
            SUM(F.TotalAmount) AS TotalReturnAmount,

            SUM(F.ReturnQty * F.SellingPrice)
                AS ReturnQtyValue,

            SUM(F.ReturnQty * F.MRP)
                AS ReturnMRPValue

        FROM FilteredReturns AS F

        GROUP BY F.SalesReturnDetail_ProductId
    ),
    EnrichedSummary AS
    (
        SELECT
            S.*,
            P.Prod_Name AS ProductName

        FROM ProductSummary AS S

        INNER JOIN dbo.tbl_Product AS P
            ON P.Prod_Id = S.ProductId

        WHERE
            @Search IS NULL
            OR P.Prod_Name LIKE N'%' + @Search + N'%'
    ),
    NumberedSummary AS
    (
        SELECT
            E.*,

            COUNT_BIG(*) OVER () AS TotalRecords,

            ROW_NUMBER() OVER
            (
                ORDER BY
                    E.TotalReturnAmount DESC,
                    E.ProductId ASC
            ) AS RowNumber

        FROM EnrichedSummary AS E
    )
    ------------------------------------------------------------
    -- 3. PAGINATED PRODUCT-WISE REPORT
    ------------------------------------------------------------
    SELECT
        N.ProductId,
        N.ProductName,

        N.TotalReturnEntries,
        N.TotalBatches,

        N.FirstReturnDate,
        N.LastReturnDate,

        N.TotalReturnQty,
        N.TotalFreeQty,
        N.TotalReturnedUnits,

        N.TotalSubTotal,
        N.TotalDiscountAmount,
        N.TotalTaxAmount,
        N.TotalReturnAmount,

        N.ReturnQtyValue,
        N.ReturnMRPValue,

        N.TotalRecords,
        N.RowNumber AS RowNumber,
        @PageNumber AS PageNumber,
        @PageSize AS PageSize

    FROM NumberedSummary AS N

    WHERE N.RowNumber BETWEEN
          ((CONVERT(BIGINT, @PageNumber) - 1) * @PageSize) + 1
          AND
          (CONVERT(BIGINT, @PageNumber) * @PageSize)

    ORDER BY N.RowNumber
    OPTION (RECOMPILE);
END
GO

-- ============================================================================
-- 2. Product-Wise Sales Return Detail Report
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_ProductWise_SalesReturnDetailReport
(
    @CompId       INT = 1,
    @BranchId     INT = 1,
    @ProductId    INT = 4188,
    @FromDate     DATE = NULL,
    @ToDate       DATE = NULL,
    @CustomerId   INT = NULL,
    @BatchId      INT = NULL,
    @InvoiceNo    NVARCHAR(50) = NULL,
    @Search       NVARCHAR(200) = NULL,
    @IsActive     BIT = 1,
    @PageNumber   INT = 1,
    @PageSize     INT = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @PageNumber =
        CASE WHEN ISNULL(@PageNumber, 0) < 1
             THEN 1 ELSE @PageNumber END;

    SET @PageSize =
        CASE WHEN ISNULL(@PageSize, 0) < 1 THEN 50
             WHEN @PageSize > 500 THEN 500
             ELSE @PageSize END;

    SET @Search = NULLIF(LTRIM(RTRIM(@Search)), N'');
    SET @InvoiceNo = NULLIF(LTRIM(RTRIM(@InvoiceNo)), N'');

    IF @ProductId IS NULL OR @ProductId <= 0
    BEGIN
        THROW 50001, 'Valid ProductId is required.', 1;
    END;

    ;WITH DetailRows AS
    (
        SELECT
            M.SalesReturnMaster_Id,
            M.SalesReturnMaster_InvoiceNo,
            M.SalesReturnMaster_Date,
            M.SalesReturnMaster_CompId,
            M.SalesReturnMaster_BranchId,
            M.SalesReturnMaster_CustomerId,
            C.Cust_Name AS CustomerName,
            M.SalesReturnMaster_LedgerId,
            M.SalesReturnMaster_Status,
            M.SalesReturnMaster_Remark,

            D.SalesReturnDetail_ProductId,
            D.SalesReturnDetail_BatchId,
            D.SalesReturnDetail_UnitId,
            D.SalesReturnDetail_Qty,
            D.SalesReturnDetail_FreeQty,
            D.SalesReturnDetail_SellingPrice,
            D.SalesReturnDetail_MRP,
            D.SalesReturnDetail_DiscountPercentage,
            D.SalesReturnDetail_DiscountAmount,
            D.SalesReturnDetail_TaxPercentage,
            D.SalesReturnDetail_TaxAmount,
            D.SalesReturnDetail_SubTotal,
            D.SalesReturnDetail_TotalAmount,
            D.SalesReturnDetail_Remark,

            COUNT_BIG(*) OVER () AS TotalRecords,

            SUM(ISNULL(D.SalesReturnDetail_Qty, 0))
                OVER () AS TotalReturnQty,

            SUM(ISNULL(D.SalesReturnDetail_FreeQty, 0))
                OVER () AS TotalFreeQty,

            SUM(ISNULL(D.SalesReturnDetail_TotalAmount, 0))
                OVER () AS TotalReturnAmount,

            ROW_NUMBER() OVER
            (
                ORDER BY
                    M.SalesReturnMaster_Date DESC,
                    M.SalesReturnMaster_Id DESC,
                    D.SalesReturnDetail_BatchId
            ) AS RowNumber

        FROM dbo.tbl_SalesReturnEntryMaster M

        LEFT JOIN dbo.tbl_Customer C
            ON C.Cust_Id = M.SalesReturnMaster_CustomerId

        INNER JOIN dbo.tbl_SalesReturnEntryDetail D
            ON D.SalesReturnDetail_MasterId =
               M.SalesReturnMaster_Id

        WHERE
            M.SalesReturnMaster_CompId = @CompId
            AND M.SalesReturnMaster_BranchId = @BranchId

            AND D.SalesReturnDetail_ProductId = @ProductId

            AND (@FromDate IS NULL OR
                 M.SalesReturnMaster_Date >= @FromDate)

            AND (@ToDate IS NULL OR
                 M.SalesReturnMaster_Date <
                 DATEADD(DAY, 1, @ToDate))

            AND (@CustomerId IS NULL OR
                 M.SalesReturnMaster_CustomerId = @CustomerId)

            AND (@BatchId IS NULL OR
                 D.SalesReturnDetail_BatchId = @BatchId)

            AND (@InvoiceNo IS NULL OR
                 M.SalesReturnMaster_InvoiceNo LIKE
                 N'%' + @InvoiceNo + N'%')

            AND (@IsActive IS NULL OR
                 M.SalesReturnMaster_IsActive = @IsActive)

            AND ISNULL(D.SalesReturnDetail_IsActive, 1) = 1

            AND
            (
                @Search IS NULL
                OR M.SalesReturnMaster_InvoiceNo LIKE
                   N'%' + @Search + N'%'
                OR M.SalesReturnMaster_Remark LIKE
                   N'%' + @Search + N'%'
                OR D.SalesReturnDetail_Remark LIKE
                   N'%' + @Search + N'%'
                OR C.Cust_Name LIKE
                   N'%' + @Search + N'%'
            )
    )
    SELECT *
    FROM DetailRows
    WHERE RowNumber BETWEEN
        ((CONVERT(BIGINT, @PageNumber) - 1) * @PageSize) + 1
        AND (CONVERT(BIGINT, @PageNumber) * @PageSize)
    ORDER BY RowNumber
    OPTION (RECOMPILE);
END
GO

-- ============================================================================
-- 3. Customer-Wise Sales Return Report
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_CustomerWise_SalesReturnReport
(
    @CompId          INT = 1,
    @BranchId        INT = 1,
    @FromDate        DATE = '2026-04-01',
    @ToDate          DATE = '2027-03-31',
    @CustomerId      INT = NULL,
    @Search          NVARCHAR(200) = NULL,
    @OnlyOutstanding BIT = 0,
    @PageNumber      INT = 1,
    @PageSize        INT = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        SET @PageNumber = CASE
            WHEN ISNULL(@PageNumber, 0) < 1 THEN 1
            ELSE @PageNumber
        END;

        SET @PageSize = CASE
            WHEN ISNULL(@PageSize, 0) < 1 THEN 50
            WHEN @PageSize > 500 THEN 500
            ELSE @PageSize
        END;

        SET @FromDate = ISNULL(@FromDate, '19000101');
        SET @ToDate   = ISNULL(@ToDate, '99991231');

        IF @FromDate > @ToDate
            THROW 50001, 'From Date cannot be greater than To Date.', 1;

        /* =====================================================
           FILTERED CUSTOMER-WISE RETURN DATA
        ===================================================== */

        SELECT
            M.SalesReturnMaster_Id,
            M.SalesReturnMaster_CompId,
            M.SalesReturnMaster_BranchId,

            M.SalesReturnMaster_CustomerId,
            M.SalesReturnMaster_LedgerId,
            C.Cust_Id,
            C.Cust_Code,
            C.Cust_Name,
            C.Cust_MobileNo,

            C.Cust_RouteId,
            C.Cust_AreaId,
            C.Cust_CityId,
            C.Cust_StateId,

            M.SalesReturnMaster_InvoiceNo,
            M.SalesReturnMaster_Date,

            CAST(
                ISNULL(M.SalesReturnMaster_GrandTotal, 0)
                AS DECIMAL(18,2)
            ) AS BillAmount,

            CAST(
                ISNULL(M.SalesReturnMaster_PaidAmount, 0)
                AS DECIMAL(18,2)
            ) AS PaidAmount,

            CAST(
                ISNULL(M.SalesReturnMaster_BalanceAmount, 0)
                AS DECIMAL(18,2)
            ) AS BalanceAmount,

            DATEDIFF(
                DAY,
                CONVERT(DATE, M.SalesReturnMaster_Date),
                CONVERT(DATE, GETDATE())
            ) AS Days,

            M.SalesReturnMaster_CreatedBy AS SalesmanId

        INTO #ReturnData

        FROM dbo.tbl_SalesReturnEntryMaster M

        LEFT JOIN dbo.tbl_Customer C
            ON C.Cust_Id = M.SalesReturnMaster_CustomerId

        WHERE
            M.SalesReturnMaster_CompId = @CompId
            AND M.SalesReturnMaster_BranchId = @BranchId

            AND ISNULL(M.SalesReturnMaster_IsActive, 1) = 1

            AND M.SalesReturnMaster_Date >= @FromDate
            AND M.SalesReturnMaster_Date < DATEADD(
                DAY, 1, CONVERT(DATETIME, @ToDate)
            )

            AND (
                @CustomerId IS NULL
                OR M.SalesReturnMaster_CustomerId = @CustomerId
            )

            AND (
                NULLIF(LTRIM(RTRIM(@Search)), '') IS NULL
                OR C.Cust_Name LIKE '%' + @Search + '%'
                OR C.Cust_Code LIKE '%' + @Search + '%'
                OR C.Cust_MobileNo LIKE '%' + @Search + '%'
                OR M.SalesReturnMaster_InvoiceNo LIKE '%' + @Search + '%'
            )

            AND (
                @OnlyOutstanding = 0
                OR ISNULL(M.SalesReturnMaster_BalanceAmount, 0) > 0
            );

        /* =====================================================
           RESULT 1: PAGINATED RETURN STATEMENT
        ===================================================== */

        SELECT
            Cust_Id AS CustomerId,
            Cust_Code AS CustomerCode,
            Cust_Name AS CustomerName,
            Cust_MobileNo AS MobileNo,

            SalesReturnMaster_Id AS ReturnId,
            SalesReturnMaster_InvoiceNo AS BillNo,
            SalesReturnMaster_Date AS ReturnDate,

            BillAmount,
            PaidAmount,
            BalanceAmount,
         
            Days,
            SalesmanId,

            'DETAIL' AS RowType

        FROM #ReturnData

        ORDER BY
            Cust_Name,
            SalesReturnMaster_Date,
            SalesReturnMaster_Id

        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        /* =====================================================
           RESULT 2: CUSTOMER / PARTY TOTAL
           Totals cover all filtered rows, not only current page.
        ===================================================== */

        SELECT
            Cust_Id AS CustomerId,
            Cust_Code AS CustomerCode,
            Cust_Name AS CustomerName,

            COUNT(*) AS TotalReturns,

            CAST(SUM(BillAmount) AS DECIMAL(18,2))
                AS TotalBillAmount,

            CAST(SUM(PaidAmount) AS DECIMAL(18,2))
                AS TotalPaidAmount,

            CAST(SUM(BalanceAmount) AS DECIMAL(18,2))
                AS TotalBalanceAmount,

            'PARTY_TOTAL' AS RowType

        FROM #ReturnData

        GROUP BY
            Cust_Id,
            Cust_Code,
            Cust_Name

        ORDER BY Cust_Name;

        /* =====================================================
           RESULT 3: GRAND TOTAL
        ===================================================== */

        SELECT
            COUNT(*) AS TotalReturns,

            COUNT(DISTINCT Cust_Id) AS TotalCustomers,

            CAST(
                ISNULL(SUM(BillAmount), 0)
                AS DECIMAL(18,2)
            ) AS GrandTotalBillAmount,

            CAST(
                ISNULL(SUM(PaidAmount), 0)
                AS DECIMAL(18,2)
            ) AS GrandTotalPaidAmount,

            CAST(
                ISNULL(SUM(BalanceAmount), 0)
                AS DECIMAL(18,2)
            ) AS GrandTotalBalanceAmount,

            CEILING(
                COUNT(*) * 1.0 / @PageSize
            ) AS TotalPages,

            @PageNumber AS CurrentPage,
            @PageSize AS PageSize

        FROM #ReturnData;

        DROP TABLE #ReturnData;

    END TRY
    BEGIN CATCH

        IF OBJECT_ID('tempdb..#ReturnData') IS NOT NULL
            DROP TABLE #ReturnData;

        THROW;

    END CATCH
END
GO
