-- ==============================================================================
-- Stored Procedure: dbo.SP_LowStock_Report
-- Database: SankysoftBillingDB
-- Purpose: Generates Low Stock Report with batch counts, shortage and pagination
-- ==============================================================================

USE [SankysoftBillingDB];
GO

CREATE OR ALTER PROCEDURE dbo.SP_LowStock_Report
(
    @CompId       INT            = 1,
    @BranchId     INT            = 1,
    @LowStockQty  DECIMAL(18,2)  = 10,
    @ProductId    INT            = NULL,
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

    SET @LowStockQty = ISNULL(@LowStockQty, 10);
    SET @Search = NULLIF(LTRIM(RTRIM(@Search)), '');

    ;WITH ProductStock AS
    (
        SELECT
            B.Batch_CompId,
            B.Batch_BranchId,
            B.Batch_ProductId,

            CAST(
                SUM(ISNULL(B.Batch_Stock, 0))
                AS DECIMAL(18,2)
            ) AS TotalStock,

            CAST(
                SUM(ISNULL(B.Batch_AvailableStock, 0))
                AS DECIMAL(18,2)
            ) AS AvailableStock,

            COUNT(B.Batch_Id) AS BatchCount

        FROM dbo.tbl_Batch B
        WHERE
            B.Batch_CompId = @CompId
            AND B.Batch_BranchId = @BranchId
            AND (
                @ProductId IS NULL
                OR B.Batch_ProductId = @ProductId
            )
        GROUP BY
            B.Batch_CompId,
            B.Batch_BranchId,
            B.Batch_ProductId
        HAVING
            SUM(ISNULL(B.Batch_AvailableStock, 0))
                <= @LowStockQty
    )
    SELECT
        PS.Batch_CompId,
        PS.Batch_BranchId,
        PS.Batch_ProductId,

        -- Product Name + Unit Short Name + Unit Value
        CONCAT(
            P.Prod_Name,
            ' (',
            U.Unit_ShortName,
            ' ',
            FORMAT(P.Prod_UnitValue, '0.##'),
            ')'
        ) AS ProductName,

        U.Unit_ShortName AS UnitShortName,
        CAST(P.Prod_UnitValue AS DECIMAL(18,2)) AS UnitValue,

        PS.TotalStock,
        PS.AvailableStock,

        CAST(
            @LowStockQty - PS.AvailableStock
            AS DECIMAL(18,2)
        ) AS StockShortage,

        PS.BatchCount,
        COUNT_BIG(*) OVER() AS TotalRecords

    FROM ProductStock PS

    INNER JOIN dbo.tbl_Product P
        ON P.Prod_Id = PS.Batch_ProductId

    LEFT JOIN dbo.tbl_Unit U
        ON U.Unit_Id = P.Prod_UnitId

    WHERE
        @Search IS NULL
        OR P.Prod_Name LIKE '%' + @Search + '%'
        OR U.Unit_ShortName LIKE '%' + @Search + '%'
        OR EXISTS
        (
            SELECT 1
            FROM dbo.tbl_Batch B
            WHERE
                B.Batch_CompId = PS.Batch_CompId
                AND B.Batch_BranchId = PS.Batch_BranchId
                AND B.Batch_ProductId = PS.Batch_ProductId
                AND B.Batch_Number LIKE '%' + @Search + '%'
        )

    ORDER BY
        PS.AvailableStock ASC,
        PS.Batch_ProductId

    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY

    OPTION (RECOMPILE);
END;
GO
