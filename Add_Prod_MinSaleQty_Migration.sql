-- ============================================================================
-- Migration Script: Add Prod_MinSaleQty to Product and Batch APIs
-- Database: SankysoftBillingDB / BillingSoftwareDb
-- Target Table: dbo.tbl_Product
-- Stored Procedures:
--   1. dbo.SP_Product_InsertOrUpdate
--   2. dbo.SP_Product_GetAll
--   3. dbo.SP_Batch_GetAll
--   4. dbo.SP_Batch_GetAllProductStock
-- ============================================================================

-- ============================================================================
-- 1. Ensure Column [Prod_MinSaleQty] Exists in [dbo].[tbl_Product]
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Product]') 
      AND name = 'Prod_MinSaleQty'
)
BEGIN
    ALTER TABLE [dbo].[tbl_Product]
    ADD [Prod_MinSaleQty] DECIMAL(18, 2) NULL CONSTRAINT [DF_tbl_Product_Prod_MinSaleQty] DEFAULT (0);
    PRINT 'Column [Prod_MinSaleQty] added successfully to [dbo].[tbl_Product].';
END
ELSE
BEGIN
    PRINT 'Column [Prod_MinSaleQty] already exists in [dbo].[tbl_Product].';
END
GO

-- ============================================================================
-- 2. Stored Procedure: dbo.SP_Product_InsertOrUpdate
-- Accepts JSON data via @ProdJsonData and handles insertion & update of products,
-- including the new Prod_MinSaleQty column.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Product_InsertOrUpdate]
    @ProdJsonData NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Status BIT = 0;
    DECLARE @Message NVARCHAR(500) = '';
    DECLARE @ReturnedProdId INT = 0;

    -- Validate JSON input
    IF @ProdJsonData IS NULL OR ISJSON(@ProdJsonData) = 0
    BEGIN
        SELECT 
            CAST(0 AS BIT) AS [Status], 
            'Invalid or malformed JSON data provided.' AS [Message], 
            0 AS [Prod_Id];
        RETURN;
    END

    -- Temporary table to hold parsed JSON data
    DECLARE @ParsedData TABLE
    (
        [Prod_Id]               INT,
        [Prod_CompId]           INT,
        [Prod_BranchId]         INT,
        [Prod_Code]             NVARCHAR(50),
        [Prod_Name]             NVARCHAR(200),
        [Prod_BrandId]          INT,
        [Prod_CategoryId]       INT,
        [Prod_SubCategoryId]    INT,
        [Prod_UnitId]           INT,
        [Prod_UnitValue]        DECIMAL(18,2),
        [Prod_MinSaleQty]       DECIMAL(18,2),
        [Prod_HSNCode]          NVARCHAR(50),
        [Prod_GSTPercent]       DECIMAL(18,2),
        [Batch_Number]          NVARCHAR(50),
        [Batch_Barcode]         NVARCHAR(50),
        [Batch_EANCode]         NVARCHAR(50),
        [Batch_Stock]           DECIMAL(18,2),
        [Batch_LandingPrice]    DECIMAL(18,2),
        [Batch_PurchasePrice]   DECIMAL(18,2),
        [Batch_MRP]             DECIMAL(18,2),
        [Batch_SellingPrice]    DECIMAL(18,2),
        [Prod_IsActive]         BIT,
        [Prod_CreatedBy]        INT,
        [Prod_ModifiedBy]       INT
    );

    INSERT INTO @ParsedData
    SELECT
        ISNULL(COALESCE([Prod_Id], [prod_Id]), 0),
        COALESCE([Prod_CompId], [prod_CompId]),
        COALESCE([Prod_BranchId], [prod_BranchId]),
        COALESCE([Prod_Code], [prod_Code]),
        COALESCE([Prod_Name], [prod_Name]),
        COALESCE([Prod_BrandId], [prod_BrandId]),
        COALESCE([Prod_CategoryId], [prod_CategoryId]),
        COALESCE([Prod_SubCategoryId], [prod_SubCategoryId]),
        COALESCE([Prod_UnitId], [prod_UnitId]),
        COALESCE([Prod_UnitValue], [prod_UnitValue]),
        COALESCE([Prod_MinSaleQty], [prod_MinSaleQty]),
        COALESCE([Prod_HSNCode], [prod_HSNCode]),
        COALESCE([Prod_GSTPercent], [prod_GSTPercent]),
        COALESCE([Batch_Number], [batch_Number]),
        COALESCE([Batch_Barcode], [batch_Barcode]),
        COALESCE([Batch_EANCode], [batch_EANCode]),
        COALESCE([Batch_Stock], [batch_Stock]),
        COALESCE([Batch_LandingPrice], [batch_LandingPrice]),
        COALESCE([Batch_PurchasePrice], [batch_PurchasePrice]),
        COALESCE([Batch_MRP], [batch_MRP]),
        COALESCE([Batch_SellingPrice], [batch_SellingPrice]),
        ISNULL(COALESCE([Prod_IsActive], [prod_IsActive]), 1),
        ISNULL(COALESCE([Prod_CreatedBy], [prod_CreatedBy]), 0),
        ISNULL(COALESCE([Prod_ModifiedBy], [prod_ModifiedBy]), 0)
    FROM OPENJSON(@ProdJsonData)
    WITH
    (
        [Prod_Id]               INT             '$.Prod_Id',
        [prod_Id]               INT             '$.prod_Id',
        [Prod_CompId]           INT             '$.Prod_CompId',
        [prod_CompId]           INT             '$.prod_CompId',
        [Prod_BranchId]         INT             '$.Prod_BranchId',
        [prod_BranchId]         INT             '$.prod_BranchId',
        [Prod_Code]             NVARCHAR(50)    '$.Prod_Code',
        [prod_Code]             NVARCHAR(50)    '$.prod_Code',
        [Prod_Name]             NVARCHAR(200)   '$.Prod_Name',
        [prod_Name]             NVARCHAR(200)   '$.prod_Name',
        [Prod_BrandId]          INT             '$.Prod_BrandId',
        [prod_BrandId]          INT             '$.prod_BrandId',
        [Prod_CategoryId]       INT             '$.Prod_CategoryId',
        [prod_CategoryId]       INT             '$.prod_CategoryId',
        [Prod_SubCategoryId]    INT             '$.Prod_SubCategoryId',
        [prod_SubCategoryId]    INT             '$.prod_SubCategoryId',
        [Prod_UnitId]           INT             '$.Prod_UnitId',
        [prod_UnitId]           INT             '$.prod_UnitId',
        [Prod_UnitValue]        DECIMAL(18,2)   '$.Prod_UnitValue',
        [prod_UnitValue]        DECIMAL(18,2)   '$.prod_UnitValue',
        [Prod_MinSaleQty]       DECIMAL(18,2)   '$.Prod_MinSaleQty',
        [prod_MinSaleQty]       DECIMAL(18,2)   '$.prod_MinSaleQty',
        [Prod_HSNCode]          NVARCHAR(50)    '$.Prod_HSNCode',
        [prod_HSNCode]          NVARCHAR(50)    '$.prod_HSNCode',
        [Prod_GSTPercent]       DECIMAL(18,2)   '$.Prod_GSTPercent',
        [prod_GSTPercent]       DECIMAL(18,2)   '$.prod_GSTPercent',
        [Batch_Number]          NVARCHAR(50)    '$.Batch_Number',
        [batch_Number]          NVARCHAR(50)    '$.batch_Number',
        [Batch_Barcode]         NVARCHAR(50)    '$.Batch_Barcode',
        [batch_Barcode]         NVARCHAR(50)    '$.batch_Barcode',
        [Batch_EANCode]         NVARCHAR(50)    '$.Batch_EANCode',
        [batch_EANCode]         NVARCHAR(50)    '$.batch_EANCode',
        [Batch_Stock]           DECIMAL(18,2)   '$.Batch_Stock',
        [batch_Stock]           DECIMAL(18,2)   '$.batch_Stock',
        [Batch_LandingPrice]    DECIMAL(18,2)   '$.Batch_LandingPrice',
        [batch_LandingPrice]    DECIMAL(18,2)   '$.batch_LandingPrice',
        [Batch_PurchasePrice]   DECIMAL(18,2)   '$.Batch_PurchasePrice',
        [batch_PurchasePrice]   DECIMAL(18,2)   '$.batch_PurchasePrice',
        [Batch_MRP]             DECIMAL(18,2)   '$.Batch_MRP',
        [batch_MRP]             DECIMAL(18,2)   '$.batch_MRP',
        [Batch_SellingPrice]    DECIMAL(18,2)   '$.Batch_SellingPrice',
        [batch_SellingPrice]    DECIMAL(18,2)   '$.batch_SellingPrice',
        [Prod_IsActive]         BIT             '$.Prod_IsActive',
        [prod_IsActive]         BIT             '$.prod_IsActive',
        [Prod_CreatedBy]        INT             '$.Prod_CreatedBy',
        [prod_CreatedBy]        INT             '$.prod_CreatedBy',
        [Prod_ModifiedBy]       INT             '$.Prod_ModifiedBy',
        [prod_ModifiedBy]       INT             '$.prod_ModifiedBy'
    );

    DECLARE 
        @Prod_Id               INT,
        @Prod_CompId           INT,
        @Prod_BranchId         INT,
        @Prod_Code             NVARCHAR(50),
        @Prod_Name             NVARCHAR(200),
        @Prod_BrandId          INT,
        @Prod_CategoryId       INT,
        @Prod_SubCategoryId    INT,
        @Prod_UnitId           INT,
        @Prod_UnitValue        DECIMAL(18,2),
        @Prod_MinSaleQty       DECIMAL(18,2),
        @Prod_HSNCode          NVARCHAR(50),
        @Prod_GSTPercent       DECIMAL(18,2),
        @Batch_Number          NVARCHAR(50),
        @Batch_Barcode         NVARCHAR(50),
        @Batch_EANCode         NVARCHAR(50),
        @Batch_Stock           DECIMAL(18,2),
        @Batch_LandingPrice    DECIMAL(18,2),
        @Batch_PurchasePrice   DECIMAL(18,2),
        @Batch_MRP             DECIMAL(18,2),
        @Batch_SellingPrice    DECIMAL(18,2),
        @Prod_IsActive         BIT,
        @Prod_CreatedBy        INT,
        @Prod_ModifiedBy       INT;

    SELECT TOP 1
        @Prod_Id               = [Prod_Id],
        @Prod_CompId           = [Prod_CompId],
        @Prod_BranchId         = [Prod_BranchId],
        @Prod_Code             = LTRIM(RTRIM([Prod_Code])),
        @Prod_Name             = LTRIM(RTRIM([Prod_Name])),
        @Prod_BrandId          = [Prod_BrandId],
        @Prod_CategoryId       = [Prod_CategoryId],
        @Prod_SubCategoryId    = [Prod_SubCategoryId],
        @Prod_UnitId           = [Prod_UnitId],
        @Prod_UnitValue        = [Prod_UnitValue],
        @Prod_MinSaleQty       = [Prod_MinSaleQty],
        @Prod_HSNCode          = [Prod_HSNCode],
        @Prod_GSTPercent       = [Prod_GSTPercent],
        @Batch_Number          = [Batch_Number],
        @Batch_Barcode         = [Batch_Barcode],
        @Batch_EANCode         = [Batch_EANCode],
        @Batch_Stock           = [Batch_Stock],
        @Batch_LandingPrice    = [Batch_LandingPrice],
        @Batch_PurchasePrice   = [Batch_PurchasePrice],
        @Batch_MRP             = [Batch_MRP],
        @Batch_SellingPrice    = [Batch_SellingPrice],
        @Prod_IsActive         = [Prod_IsActive],
        @Prod_CreatedBy        = [Prod_CreatedBy],
        @Prod_ModifiedBy       = [Prod_ModifiedBy]
    FROM @ParsedData;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- CASE 1: INSERT (Prod_Id = 0 or NULL)
        IF (@Prod_Id = 0 OR @Prod_Id IS NULL)
        BEGIN
            -- Generate Prod_Code if empty
            IF @Prod_Code IS NULL OR LEN(@Prod_Code) = 0
            BEGIN
                DECLARE @NextProdNo INT;
                SELECT @NextProdNo = ISNULL(MAX(Prod_Id), 0) + 1 FROM dbo.tbl_Product;
                SET @Prod_Code = 'PRD' + RIGHT('00000' + CAST(@NextProdNo AS VARCHAR(5)), 5);
            END

            INSERT INTO dbo.tbl_Product
            (
                [Prod_CompId],
                [Prod_BranchId],
                [Prod_Code],
                [Prod_Name],
                [Prod_BrandId],
                [Prod_CategoryId],
                [Prod_SubCategoryId],
                [Prod_UnitId],
                [Prod_UnitValue],
                [Prod_MinSaleQty],
                [Prod_HSNCode],
                [Prod_GSTPercent],
                [Prod_IsActive],
                [Prod_CreatedBy],
                [Prod_CreatedDate]
            )
            VALUES
            (
                @Prod_CompId,
                @Prod_BranchId,
                @Prod_Code,
                @Prod_Name,
                @Prod_BrandId,
                @Prod_CategoryId,
                @Prod_SubCategoryId,
                @Prod_UnitId,
                @Prod_UnitValue,
                @Prod_MinSaleQty,
                @Prod_HSNCode,
                @Prod_GSTPercent,
                ISNULL(@Prod_IsActive, 1),
                @Prod_CreatedBy,
                GETDATE()
            );

            SET @ReturnedProdId = CAST(SCOPE_IDENTITY() AS INT);

            -- If batch details provided, insert initial batch record
            IF @Batch_Number IS NOT NULL AND LEN(TRIM(@Batch_Number)) > 0
            BEGIN
                INSERT INTO dbo.tbl_Batch
                (
                    [Batch_CompId],
                    [Batch_BranchId],
                    [Batch_ProductId],
                    [Batch_Number],
                    [Batch_Barcode],
                    [Batch_EANCode],
                    [Batch_Stock],
                    [Batch_AvailableStock],
                    [Batch_LandingPrice],
                    [Batch_PurchasePrice],
                    [Batch_MRP],
                    [Batch_SellingPrice],
                    [Batch_IsActive],
                    [Batch_CreatedBy],
                    [Batch_CreatedDate]
                )
                VALUES
                (
                    @Prod_CompId,
                    @Prod_BranchId,
                    @ReturnedProdId,
                    @Batch_Number,
                    @Batch_Barcode,
                    @Batch_EANCode,
                    ISNULL(@Batch_Stock, 0),
                    ISNULL(@Batch_Stock, 0),
                    ISNULL(@Batch_LandingPrice, 0),
                    ISNULL(@Batch_PurchasePrice, 0),
                    ISNULL(@Batch_MRP, 0),
                    ISNULL(@Batch_SellingPrice, 0),
                    1,
                    @Prod_CreatedBy,
                    GETDATE()
                );
            END

            SET @Status = 1;
            SET @Message = 'Product created successfully.';
        END
        -- CASE 2: UPDATE (Prod_Id > 0)
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Product WHERE Prod_Id = @Prod_Id)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS [Status], 
                    'Product with ID ' + CAST(@Prod_Id AS NVARCHAR(20)) + ' does not exist.' AS [Message], 
                    CAST(@Prod_Id AS INT) AS [Prod_Id];
                RETURN;
            END

            UPDATE dbo.tbl_Product
            SET
                [Prod_CompId]        = @Prod_CompId,
                [Prod_BranchId]      = @Prod_BranchId,
                [Prod_Code]          = ISNULL(@Prod_Code, [Prod_Code]),
                [Prod_Name]          = @Prod_Name,
                [Prod_BrandId]       = @Prod_BrandId,
                [Prod_CategoryId]    = @Prod_CategoryId,
                [Prod_SubCategoryId] = @Prod_SubCategoryId,
                [Prod_UnitId]        = @Prod_UnitId,
                [Prod_UnitValue]     = @Prod_UnitValue,
                [Prod_MinSaleQty]    = CASE 
                                         WHEN @Prod_MinSaleQty IS NOT NULL THEN @Prod_MinSaleQty 
                                         ELSE [Prod_MinSaleQty] 
                                       END,
                [Prod_HSNCode]       = @Prod_HSNCode,
                [Prod_GSTPercent]    = @Prod_GSTPercent,
                [Prod_IsActive]      = ISNULL(@Prod_IsActive, [Prod_IsActive]),
                [Prod_ModifiedBy]    = @Prod_ModifiedBy,
                [Prod_ModifiedDate]  = GETDATE()
            WHERE [Prod_Id] = @Prod_Id;

            SET @ReturnedProdId = @Prod_Id;

            -- Update or insert batch if batch number provided
            IF @Batch_Number IS NOT NULL AND LEN(TRIM(@Batch_Number)) > 0
            BEGIN
                IF EXISTS (SELECT 1 FROM dbo.tbl_Batch WHERE Batch_ProductId = @Prod_Id AND Batch_Number = @Batch_Number)
                BEGIN
                    UPDATE dbo.tbl_Batch
                    SET
                        [Batch_Barcode]       = ISNULL(@Batch_Barcode, [Batch_Barcode]),
                        [Batch_EANCode]       = ISNULL(@Batch_EANCode, [Batch_EANCode]),
                        [Batch_Stock]         = ISNULL(@Batch_Stock, [Batch_Stock]),
                        [Batch_LandingPrice]  = ISNULL(@Batch_LandingPrice, [Batch_LandingPrice]),
                        [Batch_PurchasePrice] = ISNULL(@Batch_PurchasePrice, [Batch_PurchasePrice]),
                        [Batch_MRP]           = ISNULL(@Batch_MRP, [Batch_MRP]),
                        [Batch_SellingPrice]  = ISNULL(@Batch_SellingPrice, [Batch_SellingPrice]),
                        [Batch_ModifiedBy]    = @Prod_ModifiedBy,
                        [Batch_ModifiedDate]  = GETDATE()
                    WHERE Batch_ProductId = @Prod_Id AND Batch_Number = @Batch_Number;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.tbl_Batch
                    (
                        [Batch_CompId],
                        [Batch_BranchId],
                        [Batch_ProductId],
                        [Batch_Number],
                        [Batch_Barcode],
                        [Batch_EANCode],
                        [Batch_Stock],
                        [Batch_AvailableStock],
                        [Batch_LandingPrice],
                        [Batch_PurchasePrice],
                        [Batch_MRP],
                        [Batch_SellingPrice],
                        [Batch_IsActive],
                        [Batch_CreatedBy],
                        [Batch_CreatedDate]
                    )
                    VALUES
                    (
                        @Prod_CompId,
                        @Prod_BranchId,
                        @Prod_Id,
                        @Batch_Number,
                        @Batch_Barcode,
                        @Batch_EANCode,
                        ISNULL(@Batch_Stock, 0),
                        ISNULL(@Batch_Stock, 0),
                        ISNULL(@Batch_LandingPrice, 0),
                        ISNULL(@Batch_PurchasePrice, 0),
                        ISNULL(@Batch_MRP, 0),
                        ISNULL(@Batch_SellingPrice, 0),
                        1,
                        @Prod_ModifiedBy,
                        GETDATE()
                    );
                END
            END

            SET @Status = 1;
            SET @Message = 'Product updated successfully.';
        END

        COMMIT TRANSACTION;

        SELECT 
            @Status AS [Status], 
            @Message AS [Message], 
            CAST(@ReturnedProdId AS INT) AS [Prod_Id];
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 
            CAST(0 AS BIT) AS [Status], 
            ERROR_MESSAGE() AS [Message], 
            0 AS [Prod_Id];
    END CATCH
END;
GO

-- ============================================================================
-- 3. Stored Procedure: dbo.SP_Product_GetAll
-- Fetches all products with joined master and batch data, including Prod_MinSaleQty.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Product_GetAll]
(
    @Prod_Id INT = NULL,
    @Prod_IsActive BIT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.[Prod_Id],
        p.[Prod_CompId],
        c.[Comp_Name] AS [Prod_CompanyName],
        p.[Prod_BranchId],
        br.[Branch_Name] AS [Prod_BranchName],
        p.[Prod_Code],
        p.[Prod_Name],
        p.[Prod_BrandId],
        b.[Brand_Name] AS [Prod_BrandName],
        p.[Prod_CategoryId],
        cat.[Category_Name] AS [Prod_CategoryName],
        p.[Prod_SubCategoryId],
        sc.[SubCategory_Name] AS [Prod_SubCategoryName],
        p.[Prod_UnitId],
        u.[Unit_Name] AS [Prod_UnitName],
        u.[Unit_ShortName] AS [Prod_UnitShortName],
        p.[Prod_UnitValue],
        p.[Prod_MinSaleQty],
        p.[Prod_HSNCode],
        p.[Prod_GSTPercent],
        bt.[Batch_Number],
        bt.[Batch_Barcode],
        bt.[Batch_EANCode],
        bt.[Batch_Stock],
        bt.[Batch_LandingPrice],
        bt.[Batch_PurchasePrice],
        bt.[Batch_MRP],
        bt.[Batch_SellingPrice],
        bt.[Batch_LandingPrice] AS [Prod_LandingPrice],
        bt.[Batch_PurchasePrice] AS [Prod_PurchasePrice],
        bt.[Batch_MRP] AS [Prod_MRP],
        bt.[Batch_SellingPrice] AS [Prod_SellingPrice],
        p.[Prod_IsActive],
        p.[Prod_CreatedBy],
        p.[Prod_CreatedDate],
        p.[Prod_ModifiedBy],
        p.[Prod_ModifiedDate]
    FROM [dbo].[tbl_Product] p
    LEFT JOIN [dbo].[tbl_Company] c ON p.[Prod_CompId] = c.[Comp_Id]
    LEFT JOIN [dbo].[tbl_Branch] br ON p.[Prod_BranchId] = br.[Branch_Id]
    LEFT JOIN [dbo].[tbl_Brand] b ON p.[Prod_BrandId] = b.[Brand_Id]
    LEFT JOIN [dbo].[tbl_Category] cat ON p.[Prod_CategoryId] = cat.[Category_Id]
    LEFT JOIN [dbo].[tbl_SubCategory] sc ON p.[Prod_SubCategoryId] = sc.[SubCategory_Id]
    LEFT JOIN [dbo].[tbl_Unit] u ON p.[Prod_UnitId] = u.[Unit_Id]
    OUTER APPLY (
        SELECT TOP 1 *
        FROM [dbo].[tbl_Batch] bt
        WHERE bt.[Batch_ProductId] = p.[Prod_Id]
        ORDER BY bt.[Batch_Id] DESC
    ) bt
    WHERE (@Prod_Id IS NULL OR @Prod_Id = 0 OR p.[Prod_Id] = @Prod_Id)
      AND (@Prod_IsActive IS NULL OR p.[Prod_IsActive] = @Prod_IsActive)
    ORDER BY p.[Prod_Id] DESC;
END;
GO

-- ============================================================================
-- 4. Stored Procedure: dbo.SP_Batch_GetAll
-- Returns batches with joined product details including Prod_MinSaleQty.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Batch_GetAll]
(
    @CompId INT = NULL,
    @BranchId INT = NULL,
    @ProductId INT = NULL,
    @Search NVARCHAR(200) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        b.[Batch_Id],
        b.[Batch_ProductId],
        p.[Prod_Name],
        p.[Prod_Code],
        u.[Unit_Name],
        p.[Prod_UnitValue],
        p.[Prod_MinSaleQty],
        b.[Batch_CompId],
        c.[Comp_Name],
        b.[Batch_BranchId],
        br.[Branch_Name],
        b.[Batch_Number],
        b.[Batch_Stock],
        b.[Batch_AvailableStock],
        b.[Batch_LandingPrice],
        b.[Batch_PurchasePrice],
        b.[Batch_MRP],
        b.[Batch_SellingPrice]
    FROM [dbo].[tbl_Batch] b
    LEFT JOIN [dbo].[tbl_Product] p ON b.[Batch_ProductId] = p.[Prod_Id]
    LEFT JOIN [dbo].[tbl_Unit] u ON p.[Prod_UnitId] = u.[Unit_Id]
    LEFT JOIN [dbo].[tbl_Company] c ON b.[Batch_CompId] = c.[Comp_Id]
    LEFT JOIN [dbo].[tbl_Branch] br ON b.[Batch_BranchId] = br.[Branch_Id]
    WHERE (@CompId IS NULL OR @CompId = 0 OR b.[Batch_CompId] = @CompId)
      AND (@BranchId IS NULL OR @BranchId = 0 OR b.[Batch_BranchId] = @BranchId)
      AND (@ProductId IS NULL OR @ProductId = 0 OR b.[Batch_ProductId] = @ProductId)
      AND (
          NULLIF(LTRIM(RTRIM(@Search)), '') IS NULL
          OR b.[Batch_Number] LIKE '%' + @Search + '%'
          OR p.[Prod_Name] LIKE '%' + @Search + '%'
          OR p.[Prod_Code] LIKE '%' + @Search + '%'
          OR b.[Batch_Barcode] LIKE '%' + @Search + '%'
          OR b.[Batch_EANCode] LIKE '%' + @Search + '%'
      )
    ORDER BY b.[Batch_Id] DESC;
END;
GO

-- ============================================================================
-- 5. Stored Procedure: dbo.SP_Batch_GetAllProductStock
-- Returns paginated product stock details including Prod_MinSaleQty.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Batch_GetAllProductStock]
(
    @CompId INT = NULL,
    @BranchId INT = NULL,
    @ProductId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@PageNumber, 0) < 1
        SET @PageNumber = 1;

    IF ISNULL(@PageSize, 0) < 1
        SET @PageSize = 10;

    ;WITH StockData AS
    (
        SELECT 
            b.[Batch_CompId],
            b.[Batch_BranchId],
            b.[Batch_ProductId],
            b.[Batch_Number],
            p.[Prod_Name] AS [ProductName],
            p.[Prod_UnitId],
            p.[Prod_UnitValue],
            p.[Prod_MinSaleQty],
            u.[Unit_Name] AS [UnitName],
            u.[Unit_ShortName] AS [UnitShortName],
            p.[Prod_BrandId],
            brd.[Brand_Name] AS [BrandName],
            p.[Prod_CategoryId],
            cat.[Category_Name] AS [CategoryName],
            p.[Prod_SubCategoryId],
            sc.[SubCategory_Name] AS [SubCategoryName],
            b.[Batch_Stock],
            b.[Batch_AvailableStock],
            b.[Batch_LandingPrice],
            b.[Batch_PurchasePrice],
            b.[Batch_MRP],
            b.[Batch_SellingPrice],
            b.[Batch_IsActive],
            b.[Batch_CreatedDate],
            b.[Batch_ModifiedDate]
        FROM [dbo].[tbl_Batch] b
        LEFT JOIN [dbo].[tbl_Product] p ON b.[Batch_ProductId] = p.[Prod_Id]
        LEFT JOIN [dbo].[tbl_Unit] u ON p.[Prod_UnitId] = u.[Unit_Id]
        LEFT JOIN [dbo].[tbl_Brand] brd ON p.[Prod_BrandId] = brd.[Brand_Id]
        LEFT JOIN [dbo].[tbl_Category] cat ON p.[Prod_CategoryId] = cat.[Category_Id]
        LEFT JOIN [dbo].[tbl_SubCategory] sc ON p.[Prod_SubCategoryId] = sc.[SubCategory_Id]
        WHERE (@CompId IS NULL OR @CompId = 0 OR b.[Batch_CompId] = @CompId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR b.[Batch_BranchId] = @BranchId)
          AND (@ProductId IS NULL OR @ProductId = 0 OR b.[Batch_ProductId] = @ProductId)
          AND (@IsActive IS NULL OR b.[Batch_IsActive] = @IsActive)
          AND (
              NULLIF(LTRIM(RTRIM(@Search)), '') IS NULL
              OR b.[Batch_Number] LIKE '%' + @Search + '%'
              OR p.[Prod_Name] LIKE '%' + @Search + '%'
              OR p.[Prod_Code] LIKE '%' + @Search + '%'
              OR b.[Batch_Barcode] LIKE '%' + @Search + '%'
              OR b.[Batch_EANCode] LIKE '%' + @Search + '%'
              OR brd.[Brand_Name] LIKE '%' + @Search + '%'
              OR cat.[Category_Name] LIKE '%' + @Search + '%'
              OR sc.[SubCategory_Name] LIKE '%' + @Search + '%'
          )
    )
    SELECT *
    FROM StockData
    ORDER BY [Batch_ProductId] DESC, [Batch_CreatedDate] DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO
