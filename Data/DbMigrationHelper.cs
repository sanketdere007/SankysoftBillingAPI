using System.Data;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Data;

public static class DbMigrationHelper
{
    public static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            var dbHelper = serviceProvider.GetRequiredService<DbHelper>();

            logger.LogInformation("Checking database schema for Prod_MinSaleQty column...");

            const string checkColumnScript = @"
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
                    END

                    DECLARE @SqlUpdateExisting NVARCHAR(MAX) = N'UPDATE [' + @ProductTable + N'] SET [Prod_MinSaleQty] = 1.00 WHERE [Prod_MinSaleQty] IS NULL OR [Prod_MinSaleQty] <= 0;';
                    EXEC sp_executesql @SqlUpdateExisting;
                END
            ";

            await dbHelper.ExecuteNonQueryAsync(checkColumnScript, CommandType.Text);
            logger.LogInformation("Database column Prod_MinSaleQty verified/migrated successfully.");

            // Update stored procedures
            const string updateProcsScript = @"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_Product')
                BEGIN
                    EXEC(N'
                    CREATE OR ALTER PROCEDURE dbo.SP_Product_InsertOrUpdate
                        @ProdJsonData NVARCHAR(MAX)
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        BEGIN TRY
                            DECLARE @Prod_Id INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_Id'') AS INT), 0);
                            DECLARE @Prod_CompId INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_CompId'') AS INT), 1);
                            DECLARE @Prod_BranchId INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_BranchId'') AS INT), 1);
                            DECLARE @Prod_Code NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, ''$.Prod_Code''), ''''));
                            DECLARE @Prod_Name NVARCHAR(200) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, ''$.Prod_Name''), ''''));
                            DECLARE @Prod_BrandId INT = TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_BrandId'') AS INT);
                            DECLARE @Prod_CategoryId INT = TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_CategoryId'') AS INT);
                            DECLARE @Prod_SubCategoryId INT = TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_SubCategoryId'') AS INT);
                            DECLARE @Prod_UnitId INT = TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_UnitId'') AS INT);
                            DECLARE @Prod_UnitValue DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_UnitValue'') AS DECIMAL(18, 2)), 0.00);
                            DECLARE @Prod_MinSaleQty DECIMAL(18, 2) = COALESCE(
                                TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_MinSaleQty'') AS DECIMAL(18, 2)),
                                TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.prod_MinSaleQty'') AS DECIMAL(18, 2)),
                                1.00
                            );
                            IF @Prod_MinSaleQty <= 0 SET @Prod_MinSaleQty = 1.00;

                            DECLARE @Prod_HSNCode NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, ''$.Prod_HSNCode''), ''''));
                            DECLARE @Prod_GSTPercent DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_GSTPercent'') AS DECIMAL(18, 2)), 0.00);
                            DECLARE @Prod_IsActive BIT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_IsActive'') AS BIT), 1);
                            DECLARE @Prod_CreatedBy INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_CreatedBy'') AS INT), 0);
                            DECLARE @Prod_ModifiedBy INT = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Prod_ModifiedBy'') AS INT), 0);

                            DECLARE @Batch_Number NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, ''$.Batch_Number''), ''''));
                            DECLARE @Batch_Barcode NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, ''$.Batch_Barcode''), ''''));
                            DECLARE @Batch_EANCode NVARCHAR(50) = TRIM(ISNULL(JSON_VALUE(@ProdJsonData, ''$.Batch_EANCode''), ''''));
                            DECLARE @Batch_Stock DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Batch_Stock'') AS DECIMAL(18, 2)), 0.00);
                            DECLARE @Batch_LandingPrice DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Batch_LandingPrice'') AS DECIMAL(18, 2)), 0.00);
                            DECLARE @Batch_PurchasePrice DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Batch_PurchasePrice'') AS DECIMAL(18, 2)), 0.00);
                            DECLARE @Batch_MRP DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Batch_MRP'') AS DECIMAL(18, 2)), 0.00);
                            DECLARE @Batch_SellingPrice DECIMAL(18, 2) = ISNULL(TRY_CAST(JSON_VALUE(@ProdJsonData, ''$.Batch_SellingPrice'') AS DECIMAL(18, 2)), 0.00);

                            IF @Prod_Id = 0
                            BEGIN
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

                                IF @Prod_Id > 0 AND (LEN(@Batch_Number) > 0 OR @Batch_Stock > 0 OR @Batch_SellingPrice > 0)
                                BEGIN
                                    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = ''tbl_Batch'')
                                    BEGIN
                                        INSERT INTO dbo.tbl_Batch (
                                            Batch_CompId, Batch_BranchId, Batch_ProductId,
                                            Batch_Number, Batch_Barcode, Batch_EANCode,
                                            Batch_Stock, Batch_AvailableStock,
                                            Batch_LandingPrice, Batch_PurchasePrice, Batch_MRP, Batch_SellingPrice,
                                            Batch_IsActive, Batch_CreatedBy, Batch_CreatedDate
                                        ) VALUES (
                                            @Prod_CompId, @Prod_BranchId, @Prod_Id,
                                            ISNULL(NULLIF(@Batch_Number, ''''), ''DEFAULT''), @Batch_Barcode, @Batch_EANCode,
                                            @Batch_Stock, @Batch_Stock,
                                            @Batch_LandingPrice, @Batch_PurchasePrice, @Batch_MRP, @Batch_SellingPrice,
                                            1, @Prod_CreatedBy, GETDATE()
                                        );
                                    END
                                END

                                SELECT 1 AS [Status], ''Product created successfully.'' AS [Message], @Prod_Id AS [Prod_Id];
                            END
                            ELSE
                            BEGIN
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

                                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = ''tbl_Batch'')
                                BEGIN
                                    UPDATE dbo.tbl_Batch
                                    SET
                                        Batch_Barcode = @Batch_Barcode,
                                        Batch_EANCode = @Batch_EANCode,
                                        Batch_Number = ISNULL(NULLIF(@Batch_Number, ''''), Batch_Number),
                                        Batch_LandingPrice = @Batch_LandingPrice,
                                        Batch_PurchasePrice = @Batch_PurchasePrice,
                                        Batch_MRP = @Batch_MRP,
                                        Batch_SellingPrice = @Batch_SellingPrice,
                                        Batch_ModifiedBy = @Prod_ModifiedBy,
                                        Batch_ModifiedDate = GETDATE()
                                    WHERE Batch_ProductId = @Prod_Id;
                                END

                                SELECT 1 AS [Status], ''Product updated successfully.'' AS [Message], @Prod_Id AS [Prod_Id];
                            END
                        END TRY
                        BEGIN CATCH
                            SELECT 0 AS [Status], ERROR_MESSAGE() AS [Message], @Prod_Id AS [Prod_Id];
                        END CATCH
                    END;
                    '');
                END
            ";

            await dbHelper.ExecuteNonQueryAsync(updateProcsScript, CommandType.Text);
            logger.LogInformation("Stored procedure SP_Product_InsertOrUpdate verified/updated successfully.");
        }
        catch (SqlException sqlEx)
        {
            logger.LogWarning(sqlEx, "Database migration check skipped or failed (SQL Server connection error). Script can be applied manually: Update_MinSalesQty_Feature.sql");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unexpected error during database migration check.");
        }
    }
}
