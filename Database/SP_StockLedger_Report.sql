USE [SankysoftBillingDB];
GO

CREATE OR ALTER PROCEDURE dbo.SP_StockLedger_Report
(
    @CompId       INT            = 1,
    @BranchId     INT            = 1,
    @FromDate     DATE           = '2026-10-01',
    @ToDate       DATE           = '2026-10-23',
    @ProductId    INT            = NULL,
    @BatchId      INT            = NULL,
    @TransactionType NVARCHAR(50) = NULL,
    @Type         NVARCHAR(10)   = NULL,
    @Search       NVARCHAR(200)  = NULL,
    @PageNumber   INT            = 1,
    @PageSize     INT            = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @PageNumber = CASE
        WHEN ISNULL(@PageNumber, 0) < 1 THEN 1
        ELSE @PageNumber
    END;

    SET @PageSize = CASE
        WHEN ISNULL(@PageSize, 0) < 1 THEN 50
        ELSE @PageSize
    END;

    SET @Search = NULLIF(LTRIM(RTRIM(@Search)), '');
    SET @TransactionType =
        NULLIF(LTRIM(RTRIM(@TransactionType)), '');
    SET @Type = NULLIF(LTRIM(RTRIM(@Type)), '');

    ;WITH LedgerData AS
    (
        SELECT
            SL.StockLedger_Id,
            SL.StockLedger_CompId,
            SL.StockLedger_BranchId,
            SL.StockLedger_ProductId,
            SL.StockLedger_BatchId,
            SL.StockLedger_Date,
            SL.StockLedger_Type,
            SL.StockLedger_TransactionType,
            SL.StockLedger_ReferenceId,
            SL.StockLedger_ReferenceNo,
            CAST(SL.StockLedger_Quantity AS DECIMAL(18,2))
                AS Quantity,
            CAST(SL.StockLedger_BeforeStock AS DECIMAL(18,2))
                AS BeforeStock,
            CAST(SL.StockLedger_AfterStock AS DECIMAL(18,2))
                AS AfterStock,
            CAST(SL.StockLedger_PhysicalStock AS DECIMAL(18,2))
                AS PhysicalStock,
            CAST(SL.StockLedger_AdjustmentQty AS DECIMAL(18,2))
                AS AdjustmentQty,
            SL.StockLedger_Remark,
            SL.StockLedger_CreatedBy,
            SL.StockLedger_CreatedDate
        FROM dbo.tbl_StockLedger SL
        WHERE
            SL.StockLedger_CompId = @CompId
            AND SL.StockLedger_BranchId = @BranchId

            AND (
                @FromDate IS NULL
                OR SL.StockLedger_Date >= @FromDate
            )
            AND (
                @ToDate IS NULL
                OR SL.StockLedger_Date < DATEADD(DAY, 1, @ToDate)
            )
            AND (
                @ProductId IS NULL
                OR SL.StockLedger_ProductId = @ProductId
            )
            AND (
                @BatchId IS NULL
                OR SL.StockLedger_BatchId = @BatchId
            )
            AND (
                @TransactionType IS NULL
                OR SL.StockLedger_TransactionType = @TransactionType
            )
            AND (
                @Type IS NULL
                OR SL.StockLedger_Type = @Type
            )
            AND (
                @Search IS NULL
                OR SL.StockLedger_ReferenceNo LIKE '%' + @Search + '%'
                OR SL.StockLedger_Remark LIKE '%' + @Search + '%'
                OR SL.StockLedger_TransactionType LIKE '%' + @Search + '%'
            )
    )
    SELECT
        *,
        COUNT_BIG(*) OVER() AS TotalRecords
    FROM LedgerData
    ORDER BY StockLedger_Date DESC, StockLedger_Id DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE);
END;
GO
