-- ==============================================================================
-- Migration Script: Implement Minimum Sales Quantity (MinSalesQty) Feature
-- Database: SankysoftBillingDB
-- ==============================================================================

USE [SankysoftBillingDB];
GO

-- 1. Ensure Prod_MinSaleQty Column exists on Product table (tbl_Product or Product)
DECLARE @ProductTable NVARCHAR(128) = NULL;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_Product')
    SET @ProductTable = 'tbl_Product';
ELSE IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Product')
    SET @ProductTable = 'Product';
ELSE IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_Products')
    SET @ProductTable = 'tbl_Products';
ELSE IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
    SET @ProductTable = 'Products';

IF @ProductTable IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE object_id = OBJECT_ID(@ProductTable) AND name = 'Prod_MinSaleQty'
    )
    BEGIN
        DECLARE @SqlAdd NVARCHAR(MAX) = N'ALTER TABLE [' + @ProductTable + N'] ADD [Prod_MinSaleQty] DECIMAL(18, 2) NOT NULL CONSTRAINT [DF_' + @ProductTable + N'_Prod_MinSaleQty] DEFAULT 1.00;';
        EXEC sp_executesql @SqlAdd;
        PRINT 'Added Prod_MinSaleQty column to ' + @ProductTable;
    END
    ELSE
    BEGIN
        PRINT 'Prod_MinSaleQty column already exists on ' + @ProductTable;
    END

    -- Ensure any existing rows have a valid default of at least 1.00
    DECLARE @SqlUpdateExisting NVARCHAR(MAX) = N'UPDATE [' + @ProductTable + N'] SET [Prod_MinSaleQty] = 1.00 WHERE [Prod_MinSaleQty] IS NULL OR [Prod_MinSaleQty] <= 0;';
    EXEC sp_executesql @SqlUpdateExisting;
    PRINT 'Updated existing products with default Prod_MinSaleQty = 1.00 where needed.';
END
ELSE
BEGIN
    PRINT 'WARNING: Could not find Product table by standard names (tbl_Product, Product, tbl_Products, Products).';
END
GO

-- 2. Update/Create Stored Procedure: dbo.SP_Product_InsertOrUpdate
IF OBJECT_ID('dbo.SP_Product_InsertOrUpdate', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Product_InsertOrUpdate;
GO

CREATE PROCEDURE dbo.SP_Product_InsertOrUpdate
    @ProdJsonData NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @Prod_Id INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_Id') AS INT), 0);
        DECLARE @Prod_CompId INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_CompId') AS INT), 1);
        DECLARE @Prod_BranchId INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_BranchId') AS INT), 1);
        DECLARE @Prod_Code NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, '$.Prod_Code'), ''));
        DECLARE @Prod_Name NVARCHAR(200) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, '$.Prod_Name'), ''));
        DECLARE @Prod_BrandId INT = TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_BrandId') AS INT);
        DECLARE @Prod_CategoryId INT = TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_CategoryId') AS INT);
        DECLARE @Prod_SubCategoryId INT = TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_SubCategoryId') AS INT);
        DECLARE @Prod_UnitId INT = TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_UnitId') AS INT);
        DECLARE @Prod_UnitValue DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_UnitValue') AS DECIMAL(18, 2)), 0.00);
        DECLARE @Prod_MinSaleQty DECIMAL(18, 2) = COALESCE(
            TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_MinSaleQty') AS DECIMAL(18, 2)),
            TRY_CAST(JSON_VALUE(@ProdJsonData, '$.prod_MinSaleQty') AS DECIMAL(18, 2)),
            1.00
        );
        IF @Prod_MinSaleQty <= 0 SET @Prod_MinSaleQty = 1.00;

        DECLARE @Prod_HSNCode NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, '$.Prod_HSNCode'), ''));
        DECLARE @Prod_GSTPercent DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_GSTPercent') AS DECIMAL(18, 2)), 0.00);
        DECLARE @Prod_IsActive BIT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_IsActive') AS BIT), 1);
        DECLARE @Prod_CreatedBy INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_CreatedBy') AS INT), 0);
        DECLARE @Prod_ModifiedBy INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Prod_ModifiedBy') AS INT), 0);

        -- Batch & Pricing Fields
        DECLARE @Batch_Number NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, '$.Batch_Number'), ''));
        DECLARE @Batch_Barcode NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, '$.Batch_Barcode'), ''));
        DECLARE @Batch_EANCode NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, '$.Batch_EANCode'), ''));
        DECLARE @Batch_Stock DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Batch_Stock') AS DECIMAL(18, 2)), 0.00);
        DECLARE @Batch_LandingPrice DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Batch_LandingPrice') AS DECIMAL(18, 2)), 0.00);
        DECLARE @Batch_PurchasePrice DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Batch_PurchasePrice') AS DECIMAL(18, 2)), 0.00);
        DECLARE @Batch_MRP DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Batch_MRP') AS DECIMAL(18, 2)), 0.00);
        DECLARE @Batch_SellingPrice DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, '$.Batch_SellingPrice') AS DECIMAL(18, 2)), 0.00);

        IF @Prod_Id = 0
        BEGIN
            -- INSERT Product
            INSERT INTO dbo.tbl_Product (
                Prod_CompId, Prod_BranchId, Prod_Code, Prod_Name,
                Prod_BrandId, Prod_CategoryId, Prod_SubCategoryId, Prod_UnitId,
                Prod_UnitValue, Prod_MinSaleQty, Prod_HSNCode, Prod_GSTPercent,
                Prod_IsActive, Prod_CreatedBy, Prod_CreatedDate,
                Prod_LandingPrice, Prod_PurchasePrice, Prod_MRP, Prod_SellingPrice
            ) VALUES (
                @Prod_CompId, @Prod_BranchId, @Prod_Code, @Prod_Name,
                @Prod_BrandId, @Prod_CategoryId, @Prod_SubCategoryId, @Prod_UnitId,
                @Prod_UnitValue, @Prod_MinSaleQty, @Prod_HSNCode, @Prod_GSTPercent,
                @Prod_IsActive, @Prod_CreatedBy, GETDATE(),
                @Batch_LandingPrice, @Batch_PurchasePrice, @Batch_MRP, @Batch_SellingPrice
            );

            SET @Prod_Id = SCOPE_IDENTITY();

            -- Create Initial Batch record if batch info provided
            IF @Prod_Id > 0 AND (LEN(@Batch_Number) > 0 OR @Batch_Stock > 0 OR @Batch_SellingPrice > 0)
            BEGIN
                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_Batch')
                BEGIN
                    INSERT INTO dbo.tbl_Batch (
                        Batch_CompId, Batch_BranchId, Batch_ProductId,
                        Batch_Number, Batch_Barcode, Batch_EANCode,
                        Batch_Stock, Batch_AvailableStock,
                        Batch_LandingPrice, Batch_PurchasePrice, Batch_MRP, Batch_SellingPrice,
                        Batch_IsActive, Batch_CreatedBy, Batch_CreatedDate
                    ) VALUES (
                        @Prod_CompId, @Prod_BranchId, @Prod_Id,
                        ISNULL(NULLIF(@Batch_Number, ''), 'DEFAULT'), @Batch_Barcode, @Batch_EANCode,
                        @Batch_Stock, @Batch_Stock,
                        @Batch_LandingPrice, @Batch_PurchasePrice, @Batch_MRP, @Batch_SellingPrice,
                        1, @Prod_CreatedBy, GETDATE()
                    );
                END
            END

            SELECT 1 AS [Status], 'Product created successfully.' AS [Message], @Prod_Id AS [Prod_Id];
        END
        ELSE
        BEGIN
            -- UPDATE Product
            UPDATE dbo.tbl_Product
            SET
                Prod_CompId = @Prod_CompId,
                Prod_BranchId = @Prod_BranchId,
                Prod_Code = @Prod_Code,
                Prod_Name = @Prod_Name,
                Prod_BrandId = @Prod_BrandId,
                Prod_CategoryId = @Prod_CategoryId,
                Prod_SubCategoryId = @Prod_SubCategoryId,
                Prod_UnitId = @Prod_UnitId,
                Prod_UnitValue = @Prod_UnitValue,
                Prod_MinSaleQty = @Prod_MinSaleQty,
                Prod_HSNCode = @Prod_HSNCode,
                Prod_GSTPercent = @Prod_GSTPercent,
                Prod_IsActive = @Prod_IsActive,
                Prod_ModifiedBy = @Prod_ModifiedBy,
                Prod_ModifiedDate = GETDATE(),
                Prod_LandingPrice = @Batch_LandingPrice,
                Prod_PurchasePrice = @Batch_PurchasePrice,
                Prod_MRP = @Batch_MRP,
                Prod_SellingPrice = @Batch_SellingPrice
            WHERE Prod_Id = @Prod_Id;

            -- Update existing batch if any
            IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_Batch')
            BEGIN
                UPDATE dbo.tbl_Batch
                SET
                    Batch_Barcode = @Batch_Barcode,
                    Batch_EANCode = @Batch_EANCode,
                    Batch_Number = ISNULL(NULLIF(@Batch_Number, ''), Batch_Number),
                    Batch_LandingPrice = @Batch_LandingPrice,
                    Batch_PurchasePrice = @Batch_PurchasePrice,
                    Batch_MRP = @Batch_MRP,
                    Batch_SellingPrice = @Batch_SellingPrice,
                    Batch_ModifiedBy = @Prod_ModifiedBy,
                    Batch_ModifiedDate = GETDATE()
                WHERE Batch_ProductId = @Prod_Id;
            END

            SELECT 1 AS [Status], 'Product updated successfully.' AS [Message], @Prod_Id AS [Prod_Id];
        END
    END TRY
    BEGIN CATCH
        SELECT 0 AS [Status], ERROR_MESSAGE() AS [Message], @Prod_Id AS [Prod_Id];
    END CATCH
END;
GO

-- 3. Update/Create Stored Procedure: dbo.SP_Product_GetAll
IF OBJECT_ID('dbo.SP_Product_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Product_GetAll;
GO

CREATE PROCEDURE dbo.SP_Product_GetAll
    @Prod_Id INT = NULL,
    @Prod_IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.Prod_Id,
        p.Prod_CompId,
        c.Comp_Name AS Prod_CompanyName,
        p.Prod_BranchId,
        br.Branch_Name AS Prod_BranchName,
        p.Prod_Code,
        p.Prod_Name,
        p.Prod_BrandId,
        b.Brand_Name AS Prod_BrandName,
        p.Prod_CategoryId,
        cat.Category_Name AS Prod_CategoryName,
        p.Prod_SubCategoryId,
        sub.SubCategory_Name AS Prod_SubCategoryName,
        p.Prod_UnitId,
        u.Unit_Name AS Prod_UnitName,
        u.Unit_ShortName AS Prod_UnitShortName,
        ISNULL(p.Prod_UnitValue, 0.00) AS Prod_UnitValue,
        ISNULL(p.Prod_MinSaleQty, 1.00) AS Prod_MinSaleQty,
        p.Prod_HSNCode,
        ISNULL(p.Prod_GSTPercent, 0.00) AS Prod_GSTPercent,
        p.Prod_IsActive,
        p.Prod_CreatedBy,
        p.Prod_CreatedDate,
        p.Prod_ModifiedBy,
        p.Prod_ModifiedDate,
        ISNULL(p.Prod_LandingPrice, 0.00) AS Prod_LandingPrice,
        ISNULL(p.Prod_PurchasePrice, 0.00) AS Prod_PurchasePrice,
        ISNULL(p.Prod_MRP, 0.00) AS Prod_MRP,
        ISNULL(p.Prod_SellingPrice, 0.00) AS Prod_SellingPrice,
        ISNULL(bt.Batch_Number, '') AS Batch_Number,
        ISNULL(bt.Batch_Barcode, '') AS Batch_Barcode,
        ISNULL(bt.Batch_EANCode, '') AS Batch_EANCode,
        ISNULL(bt.Batch_Stock, 0.00) AS Batch_Stock,
        ISNULL(bt.Batch_LandingPrice, 0.00) AS Batch_LandingPrice,
        ISNULL(bt.Batch_PurchasePrice, 0.00) AS Batch_PurchasePrice,
        ISNULL(bt.Batch_MRP, 0.00) AS Batch_MRP,
        ISNULL(bt.Batch_SellingPrice, 0.00) AS Batch_SellingPrice
    FROM dbo.tbl_Product p WITH (NOLOCK)
    LEFT JOIN dbo.tbl_Company c WITH (NOLOCK) ON p.Prod_CompId = c.Comp_Id
    LEFT JOIN dbo.tbl_Branch br WITH (NOLOCK) ON p.Prod_BranchId = br.Branch_Id
    LEFT JOIN dbo.tbl_Brand b WITH (NOLOCK) ON p.Prod_BrandId = b.Brand_Id
    LEFT JOIN dbo.tbl_Category cat WITH (NOLOCK) ON p.Prod_CategoryId = cat.Category_Id
    LEFT JOIN dbo.tbl_SubCategory sub WITH (NOLOCK) ON p.Prod_SubCategoryId = sub.SubCategory_Id
    LEFT JOIN dbo.tbl_Unit u WITH (NOLOCK) ON p.Prod_UnitId = u.Unit_Id
    OUTER APPLY (
        SELECT TOP 1 
            Batch_Number, Batch_Barcode, Batch_EANCode,
            Batch_Stock, Batch_LandingPrice, Batch_PurchasePrice, Batch_MRP, Batch_SellingPrice
        FROM dbo.tbl_Batch bt WITH (NOLOCK)
        WHERE bt.Batch_ProductId = p.Prod_Id
        ORDER BY bt.Batch_Id DESC
    ) bt
    WHERE (@Prod_Id IS NULL OR @Prod_Id = 0 OR p.Prod_Id = @Prod_Id)
      AND (@Prod_IsActive IS NULL OR p.Prod_IsActive = @Prod_IsActive)
    ORDER BY p.Prod_Id DESC;
END;
GO

-- 4. Update/Create Stored Procedure: dbo.SP_Batch_GetAll
IF OBJECT_ID('dbo.SP_Batch_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Batch_GetAll;
GO

CREATE PROCEDURE dbo.SP_Batch_GetAll
    @CompId INT = NULL,
    @BranchId INT = NULL,
    @ProductId INT = NULL,
    @Search NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        b.Batch_Id,
        b.Batch_ProductId,
        p.Prod_Name,
        p.Prod_Code,
        u.Unit_Name,
        ISNULL(p.Prod_UnitValue, 0.00) AS Prod_UnitValue,
        ISNULL(p.Prod_MinSaleQty, 1.00) AS Prod_MinSaleQty,
        b.Batch_CompId,
        c.Comp_Name,
        b.Batch_BranchId,
        br.Branch_Name,
        b.Batch_Number,
        ISNULL(b.Batch_Stock, 0.00) AS Batch_Stock,
        ISNULL(b.Batch_AvailableStock, 0.00) AS Batch_AvailableStock,
        ISNULL(b.Batch_LandingPrice, 0.00) AS Batch_LandingPrice,
        ISNULL(b.Batch_PurchasePrice, 0.00) AS Batch_PurchasePrice,
        ISNULL(b.Batch_MRP, 0.00) AS Batch_MRP,
        ISNULL(b.Batch_SellingPrice, 0.00) AS Batch_SellingPrice,
        ISNULL(p.Prod_GSTPercent, 0.00) AS Prod_GSTPercent,
        ISNULL(b.Batch_Barcode, '') AS Batch_Barcode,
        ISNULL(b.Batch_EANCode, '') AS Batch_EANCode,
        b.Batch_IsActive
    FROM dbo.tbl_Batch b WITH (NOLOCK)
    INNER JOIN dbo.tbl_Product p WITH (NOLOCK) ON b.Batch_ProductId = p.Prod_Id
    LEFT JOIN dbo.tbl_Unit u WITH (NOLOCK) ON p.Prod_UnitId = u.Unit_Id
    LEFT JOIN dbo.tbl_Company c WITH (NOLOCK) ON b.Batch_CompId = c.Comp_Id
    LEFT JOIN dbo.tbl_Branch br WITH (NOLOCK) ON b.Batch_BranchId = br.Branch_Id
    WHERE (@CompId IS NULL OR @CompId = 0 OR b.Batch_CompId = @CompId)
      AND (@BranchId IS NULL OR @BranchId = 0 OR b.Batch_BranchId = @BranchId)
      AND (@ProductId IS NULL OR @ProductId = 0 OR b.Batch_ProductId = @ProductId)
      AND (
          @Search IS NULL OR @Search = '' OR
          p.Prod_Name LIKE '%' + @Search + '%' OR
          p.Prod_Code LIKE '%' + @Search + '%' OR
          b.Batch_Number LIKE '%' + @Search + '%' OR
          b.Batch_Barcode LIKE '%' + @Search + '%'
      )
    ORDER BY b.Batch_Id DESC;
END;
GO

-- 5. Update/Create Stored Procedure: dbo.SP_Batch_GetAllProductStock
IF OBJECT_ID('dbo.SP_Batch_GetAllProductStock', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Batch_GetAllProductStock;
GO

CREATE PROCEDURE dbo.SP_Batch_GetAllProductStock
    @CompId INT = NULL,
    @BranchId INT = NULL,
    @ProductId INT = NULL,
    @Search NVARCHAR(100) = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        b.Batch_CompId,
        b.Batch_BranchId,
        b.Batch_ProductId,
        b.Batch_Number,
        p.Prod_Name AS ProductName,
        p.Prod_UnitId,
        ISNULL(p.Prod_UnitValue, 0.00) AS Prod_UnitValue,
        ISNULL(p.Prod_MinSaleQty, 1.00) AS Prod_MinSaleQty,
        u.Unit_Name AS UnitName,
        u.Unit_ShortName AS UnitShortName,
        p.Prod_BrandId,
        br.Brand_Name AS BrandName,
        p.Prod_CategoryId,
        cat.Category_Name AS CategoryName,
        p.Prod_SubCategoryId,
        sub.SubCategory_Name AS SubCategoryName,
        ISNULL(b.Batch_Stock, 0.00) AS Batch_Stock,
        ISNULL(b.Batch_AvailableStock, 0.00) AS Batch_AvailableStock,
        ISNULL(b.Batch_LandingPrice, 0.00) AS Batch_LandingPrice,
        ISNULL(b.Batch_PurchasePrice, 0.00) AS Batch_PurchasePrice,
        ISNULL(b.Batch_MRP, 0.00) AS Batch_MRP,
        ISNULL(b.Batch_SellingPrice, 0.00) AS Batch_SellingPrice,
        CAST(b.Batch_IsActive AS INT) AS Batch_IsActive,
        b.Batch_CreatedDate,
        b.Batch_ModifiedDate
    FROM dbo.tbl_Batch b WITH (NOLOCK)
    INNER JOIN dbo.tbl_Product p WITH (NOLOCK) ON b.Batch_ProductId = p.Prod_Id
    LEFT JOIN dbo.tbl_Unit u WITH (NOLOCK) ON p.Prod_UnitId = u.Unit_Id
    LEFT JOIN dbo.tbl_Brand br WITH (NOLOCK) ON p.Prod_BrandId = br.Brand_Id
    LEFT JOIN dbo.tbl_Category cat WITH (NOLOCK) ON p.Prod_CategoryId = cat.Category_Id
    LEFT JOIN dbo.tbl_SubCategory sub WITH (NOLOCK) ON p.Prod_SubCategoryId = sub.SubCategory_Id
    WHERE (@CompId IS NULL OR @CompId = 0 OR b.Batch_CompId = @CompId)
      AND (@BranchId IS NULL OR @BranchId = 0 OR b.Batch_BranchId = @BranchId)
      AND (@ProductId IS NULL OR @ProductId = 0 OR b.Batch_ProductId = @ProductId)
      AND (@IsActive IS NULL OR b.Batch_IsActive = @IsActive)
      AND (
          @Search IS NULL OR @Search = '' OR
          p.Prod_Name LIKE '%' + @Search + '%' OR
          p.Prod_Code LIKE '%' + @Search + '%' OR
          b.Batch_Number LIKE '%' + @Search + '%'
      )
    ORDER BY b.Batch_Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO
