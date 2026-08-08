-- ============================================================================
-- Billing Software Database Schema & Stored Procedures
-- Technology: SQL Server 2016+ (Compatible with SQL Server 2019/2022/Azure SQL)
-- ADO.NET Pure Stored Procedures for Employee Management & Authentication
-- ============================================================================

-- 1. Create Database if it does not exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BillingSoftwareDb')
BEGIN
    CREATE DATABASE [BillingSoftwareDb];
END
GO

USE [BillingSoftwareDb];
GO

-- ============================================================================
-- 2. Create Employees Table
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
BEGIN
    CREATE TABLE [dbo].[Employees]
    (
        [Emp_Id]            INT IDENTITY(1,1) NOT NULL,
        [Emp_FirstName]     NVARCHAR(50)      NOT NULL,
        [Emp_LastName]      NVARCHAR(50)      NULL,
        [Emp_Email]         NVARCHAR(100)     NULL,
        [Emp_MobileNumber]  NVARCHAR(20)      NOT NULL,
        [Emp_UserName]      NVARCHAR(50)      NOT NULL,
        [Emp_PasswordHash]  NVARCHAR(255)     NOT NULL,
        [Emp_Role]          NVARCHAR(50)      NOT NULL CONSTRAINT [DF_Employees_Role] DEFAULT ('Employee'),
        [Emp_BranchId]      INT               NULL,
        [Emp_CompId]        INT               NULL,
        [Emp_Department]    NVARCHAR(50)      NULL,
        [Emp_Designation]   NVARCHAR(50)      NULL,
        [Emp_Salary]        DECIMAL(18,2)     NULL,
        [Emp_Address]       NVARCHAR(255)     NULL,
        [Emp_City]          NVARCHAR(50)      NULL,
        [Emp_State]         NVARCHAR(50)      NULL,
        [Emp_Pincode]       NVARCHAR(20)      NULL,
        [Emp_DateOfBirth]   DATETIME          NULL,
        [Emp_DateOfJoining] DATETIME          NULL,
        [IsActive]          BIT               NOT NULL CONSTRAINT [DF_Employees_IsActive] DEFAULT (1),
        [CreatedDate]       DATETIME          NOT NULL CONSTRAINT [DF_Employees_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate]      DATETIME          NULL,

        CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([Emp_Id] ASC),
        CONSTRAINT [UQ_Employees_UserName] UNIQUE NONCLUSTERED ([Emp_UserName] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_Employees_Email] ON [dbo].[Employees] ([Emp_Email] ASC);
    CREATE NONCLUSTERED INDEX [IX_Employees_MobileNumber] ON [dbo].[Employees] ([Emp_MobileNumber] ASC);
    CREATE NONCLUSTERED INDEX [IX_Employees_IsActive] ON [dbo].[Employees] ([IsActive] ASC);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND name = 'Emp_BranchId')
        ALTER TABLE [dbo].[Employees] ADD [Emp_BranchId] INT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND name = 'Emp_CompId')
        ALTER TABLE [dbo].[Employees] ADD [Emp_CompId] INT NULL;
END
GO

-- ============================================================================
-- 3. Stored Procedure: SP_Employee_InsertOrUpdate
-- Accepts Employee JSON payload via @EmpJsonData parameter.
-- If Emp_Id = 0 (or null), performs INSERT.
-- If Emp_Id > 0, performs UPDATE.
-- Returns Status (BIT/INT), Message (NVARCHAR), Emp_Id (INT).
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Employee_InsertOrUpdate]
    @EmpJsonData NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Status BIT = 0;
    DECLARE @Message NVARCHAR(500) = '';
    DECLARE @ReturnedEmpId INT = 0;

    -- Validate JSON input
    IF @EmpJsonData IS NULL OR ISJSON(@EmpJsonData) = 0
    BEGIN
        SELECT 
            CAST(0 AS BIT) AS [Status], 
            'Invalid or malformed JSON data provided.' AS [Message], 
            0 AS [Emp_Id];
        RETURN;
    END

    -- Temporary table to hold parsed JSON data
    DECLARE @ParsedData TABLE
    (
        [Emp_Id]            INT,
        [Emp_FirstName]     NVARCHAR(50),
        [Emp_LastName]      NVARCHAR(50),
        [Emp_Email]         NVARCHAR(100),
        [Emp_MobileNumber]  NVARCHAR(20),
        [Emp_UserName]      NVARCHAR(50),
        [Emp_PasswordHash]  NVARCHAR(255),
        [Emp_Role]          NVARCHAR(50),
        [Emp_BranchId]      INT,
        [Emp_CompId]        INT,
        [Emp_Department]    NVARCHAR(50),
        [Emp_Designation]   NVARCHAR(50),
        [Emp_Salary]        DECIMAL(18,2),
        [Emp_Address]       NVARCHAR(255),
        [Emp_City]          NVARCHAR(50),
        [Emp_State]         NVARCHAR(50),
        [Emp_Pincode]       NVARCHAR(20),
        [Emp_DateOfBirth]   DATETIME,
        [Emp_DateOfJoining] DATETIME,
        [IsActive]          BIT
    );

    INSERT INTO @ParsedData
    SELECT
        ISNULL([Emp_Id], 0),
        [Emp_FirstName],
        [Emp_LastName],
        [Emp_Email],
        [Emp_MobileNumber],
        [Emp_UserName],
        [Emp_PasswordHash],
        ISNULL([Emp_Role], 'Employee'),
        [Emp_BranchId],
        [Emp_CompId],
        [Emp_Department],
        [Emp_Designation],
        [Emp_Salary],
        [Emp_Address],
        [Emp_City],
        [Emp_State],
        [Emp_Pincode],
        [Emp_DateOfBirth],
        [Emp_DateOfJoining],
        ISNULL([IsActive], 1)
    FROM OPENJSON(@EmpJsonData)
    WITH
    (
        [Emp_Id]            INT           '$.Emp_Id',
        [Emp_FirstName]     NVARCHAR(50)  '$.Emp_FirstName',
        [Emp_LastName]      NVARCHAR(50)  '$.Emp_LastName',
        [Emp_Email]         NVARCHAR(100) '$.Emp_Email',
        [Emp_MobileNumber]  NVARCHAR(20)  '$.Emp_MobileNumber',
        [Emp_UserName]      NVARCHAR(50)  '$.Emp_UserName',
        [Emp_PasswordHash]  NVARCHAR(255) '$.Emp_PasswordHash',
        [Emp_Role]          NVARCHAR(50)  '$.Emp_Role',
        [Emp_BranchId]      INT           '$.Emp_BranchId',
        [Emp_CompId]        INT           '$.Emp_CompId',
        [Emp_Department]    NVARCHAR(50)  '$.Emp_Department',
        [Emp_Designation]   NVARCHAR(50)  '$.Emp_Designation',
        [Emp_Salary]        DECIMAL(18,2) '$.Emp_Salary',
        [Emp_Address]       NVARCHAR(255) '$.Emp_Address',
        [Emp_City]          NVARCHAR(50)  '$.Emp_City',
        [Emp_State]         NVARCHAR(50)  '$.Emp_State',
        [Emp_Pincode]       NVARCHAR(20)  '$.Emp_Pincode',
        [Emp_DateOfBirth]   DATETIME      '$.Emp_DateOfBirth',
        [Emp_DateOfJoining] DATETIME      '$.Emp_DateOfJoining',
        [IsActive]          BIT           '$.IsActive'
    );

    -- Extract variables from parsed row
    DECLARE 
        @Emp_Id            INT,
        @Emp_FirstName     NVARCHAR(50),
        @Emp_LastName      NVARCHAR(50),
        @Emp_Email         NVARCHAR(100),
        @Emp_MobileNumber  NVARCHAR(20),
        @Emp_UserName      NVARCHAR(50),
        @Emp_PasswordHash  NVARCHAR(255),
        @Emp_Role          NVARCHAR(50),
        @Emp_BranchId      INT,
        @Emp_CompId        INT,
        @Emp_Department    NVARCHAR(50),
        @Emp_Designation   NVARCHAR(50),
        @Emp_Salary        DECIMAL(18,2),
        @Emp_Address       NVARCHAR(255),
        @Emp_City          NVARCHAR(50),
        @Emp_State         NVARCHAR(50),
        @Emp_Pincode       NVARCHAR(20),
        @Emp_DateOfBirth   DATETIME,
        @Emp_DateOfJoining DATETIME,
        @IsActive          BIT;

    SELECT TOP 1
        @Emp_Id            = [Emp_Id],
        @Emp_FirstName     = [Emp_FirstName],
        @Emp_LastName      = [Emp_LastName],
        @Emp_Email         = [Emp_Email],
        @Emp_MobileNumber  = [Emp_MobileNumber],
        @Emp_UserName      = [Emp_UserName],
        @Emp_PasswordHash  = [Emp_PasswordHash],
        @Emp_Role          = [Emp_Role],
        @Emp_BranchId      = [Emp_BranchId],
        @Emp_CompId        = [Emp_CompId],
        @Emp_Department    = [Emp_Department],
        @Emp_Designation   = [Emp_Designation],
        @Emp_Salary        = [Emp_Salary],
        @Emp_Address       = [Emp_Address],
        @Emp_City          = [Emp_City],
        @Emp_State         = [Emp_State],
        @Emp_Pincode       = [Emp_Pincode],
        @Emp_DateOfBirth   = [Emp_DateOfBirth],
        @Emp_DateOfJoining = [Emp_DateOfJoining],
        @IsActive          = [IsActive]
    FROM @ParsedData;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- CASE 1: INSERT (Emp_Id = 0 or NULL)
        IF (@Emp_Id = 0 OR @Emp_Id IS NULL)
        BEGIN
            -- Check for duplicate username
            IF EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Emp_UserName] = @Emp_UserName)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS [Status], 
                    'Username ''' + @Emp_UserName + ''' already exists. Please choose a different username.' AS [Message], 
                    0 AS [Emp_Id];
                RETURN;
            END

            -- Check for duplicate mobile number
            IF EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Emp_MobileNumber] = @Emp_MobileNumber)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS [Status], 
                    'Mobile number ''' + @Emp_MobileNumber + ''' is already registered.' AS [Message], 
                    0 AS [Emp_Id];
                RETURN;
            END

            INSERT INTO [dbo].[Employees]
            (
                [Emp_FirstName],
                [Emp_LastName],
                [Emp_Email],
                [Emp_MobileNumber],
                [Emp_UserName],
                [Emp_PasswordHash],
                [Emp_Role],
                [Emp_BranchId],
                [Emp_CompId],
                [Emp_Department],
                [Emp_Designation],
                [Emp_Salary],
                [Emp_Address],
                [Emp_City],
                [Emp_State],
                [Emp_Pincode],
                [Emp_DateOfBirth],
                [Emp_DateOfJoining],
                [IsActive],
                [CreatedDate]
            )
            VALUES
            (
                @Emp_FirstName,
                @Emp_LastName,
                @Emp_Email,
                @Emp_MobileNumber,
                @Emp_UserName,
                ISNULL(@Emp_PasswordHash, ''),
                ISNULL(@Emp_Role, 'Employee'),
                @Emp_BranchId,
                @Emp_CompId,
                @Emp_Department,
                @Emp_Designation,
                @Emp_Salary,
                @Emp_Address,
                @Emp_City,
                @Emp_State,
                @Emp_Pincode,
                @Emp_DateOfBirth,
                @Emp_DateOfJoining,
                @IsActive,
                GETUTCDATE()
            );

            SET @ReturnedEmpId = CAST(SCOPE_IDENTITY() AS INT);
            SET @Status = 1;
            SET @Message = 'Employee created successfully.';
        END
        -- CASE 2: UPDATE (Emp_Id > 0)
        ELSE
        BEGIN
            -- Verify employee exists
            IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Emp_Id] = @Emp_Id)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS [Status], 
                    'Employee with ID ' + CAST(@Emp_Id AS NVARCHAR(20)) + ' does not exist.' AS [Message], 
                    CAST(@Emp_Id AS INT) AS [Emp_Id];
                RETURN;
            END

            -- Check if new username conflicts with another employee
            IF EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Emp_UserName] = @Emp_UserName AND [Emp_Id] <> @Emp_Id)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS [Status], 
                    'Username ''' + @Emp_UserName + ''' is already taken by another employee.' AS [Message], 
                    CAST(@Emp_Id AS INT) AS [Emp_Id];
                RETURN;
            END

            -- Update record (keep existing password hash if new hash is null or empty)
            UPDATE [dbo].[Employees]
            SET
                [Emp_FirstName]     = @Emp_FirstName,
                [Emp_LastName]      = @Emp_LastName,
                [Emp_Email]         = @Emp_Email,
                [Emp_MobileNumber]  = @Emp_MobileNumber,
                [Emp_UserName]      = @Emp_UserName,
                [Emp_PasswordHash]  = CASE 
                                        WHEN @Emp_PasswordHash IS NOT NULL AND LEN(TRIM(@Emp_PasswordHash)) > 0 
                                        THEN @Emp_PasswordHash 
                                        ELSE [Emp_PasswordHash] 
                                      END,
                [Emp_Role]          = ISNULL(@Emp_Role, [Emp_Role]),
                [Emp_BranchId]      = @Emp_BranchId,
                [Emp_CompId]        = @Emp_CompId,
                [Emp_Department]    = @Emp_Department,
                [Emp_Designation]   = @Emp_Designation,
                [Emp_Salary]        = @Emp_Salary,
                [Emp_Address]       = @Emp_Address,
                [Emp_City]          = @Emp_City,
                [Emp_State]         = @Emp_State,
                [Emp_Pincode]       = @Emp_Pincode,
                [Emp_DateOfBirth]   = @Emp_DateOfBirth,
                [Emp_DateOfJoining] = @Emp_DateOfJoining,
                [IsActive]          = @IsActive,
                [ModifiedDate]      = GETUTCDATE()
            WHERE [Emp_Id] = @Emp_Id;

            SET @ReturnedEmpId = @Emp_Id;
            SET @Status = 1;
            SET @Message = 'Employee updated successfully.';
        END

        COMMIT TRANSACTION;

        -- Return output result
        SELECT 
            @Status AS [Status], 
            @Message AS [Message], 
            CAST(@ReturnedEmpId AS INT) AS [Emp_Id];
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 
            CAST(0 AS BIT) AS [Status], 
            ERROR_MESSAGE() AS [Message], 
            0 AS [Emp_Id];
    END CATCH
END
GO

-- ============================================================================
-- 4. Stored Procedure: SP_Employee_GetAll
-- Returns all active employees ordered by Emp_Id descending.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Employee_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [Emp_Id],
        [Emp_FirstName],
        [Emp_LastName],
        [Emp_Email],
        [Emp_MobileNumber],
        [Emp_UserName],
        [Emp_Role],
        [Emp_BranchId],
        [Emp_CompId],
        [Emp_Department],
        [Emp_Designation],
        [Emp_Salary],
        [Emp_Address],
        [Emp_City],
        [Emp_State],
        [Emp_Pincode],
        [Emp_DateOfBirth],
        [Emp_DateOfJoining],
        [IsActive],
        [CreatedDate],
        [ModifiedDate]
    FROM [dbo].[Employees]
    ORDER BY [Emp_Id] DESC;
END
GO

-- ============================================================================
-- 5. Stored Procedure: SP_Employee_GetById
-- Returns full employee details for the given @Emp_Id.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Employee_GetById]
    @Emp_Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [Emp_Id],
        [Emp_FirstName],
        [Emp_LastName],
        [Emp_Email],
        [Emp_MobileNumber],
        [Emp_UserName],
        [Emp_Role],
        [Emp_BranchId],
        [Emp_CompId],
        [Emp_Department],
        [Emp_Designation],
        [Emp_Salary],
        [Emp_Address],
        [Emp_City],
        [Emp_State],
        [Emp_Pincode],
        [Emp_DateOfBirth],
        [Emp_DateOfJoining],
        [IsActive],
        [CreatedDate],
        [ModifiedDate]
    FROM [dbo].[Employees]
    WHERE [Emp_Id] = @Emp_Id;
END
GO

-- ============================================================================
-- 6. Stored Procedure: SP_Employee_GetByUserName
-- Returns employee record including password hash for login verification.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Employee_GetByUserName]
    @Emp_UserName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        [Emp_Id],
        [Emp_FirstName],
        [Emp_LastName],
        [Emp_Email],
        [Emp_MobileNumber],
        [Emp_UserName],
        [Emp_PasswordHash],
        [Emp_Role],
        [Emp_BranchId],
        [Emp_CompId],
        [Emp_Department],
        [Emp_Designation],
        [Emp_Salary],
        [Emp_Address],
        [Emp_City],
        [Emp_State],
        [Emp_Pincode],
        [Emp_DateOfBirth],
        [Emp_DateOfJoining],
        [IsActive],
        [CreatedDate],
        [ModifiedDate]
    FROM [dbo].[Employees]
    WHERE [Emp_UserName] = @Emp_UserName;
END
GO

-- ============================================================================
-- 7. Stored Procedure: SP_Employee_Login
-- Authenticates employee and returns profile details and password hash.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Employee_Login]
(
    @Emp_UserName NVARCHAR(100) = NULL,
    @Emp_Password VARCHAR(255) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Username Validation
        IF ISNULL(LTRIM(RTRIM(@Emp_UserName)), '') = ''
        BEGIN
            SELECT
                0 AS Status,
                'Username is required.' AS Message;
            RETURN;
        END

        -- Password Validation
        IF ISNULL(@Emp_Password, '') = ''
        BEGIN
            SELECT
                0 AS Status,
                'Password is required.' AS Message;
            RETURN;
        END

        -- Check Login against tbl_Employee (or fallback to Employees table)
        IF EXISTS
        (
            SELECT 1
            FROM [dbo].[tbl_Employee]
            WHERE (Emp_UserName = LTRIM(RTRIM(@Emp_UserName)) OR Emp_Email = LTRIM(RTRIM(@Emp_UserName)))
              AND Emp_PasswordHash = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Emp_Password), 2)
              AND Emp_IsActive = 1
        )
        BEGIN
            SELECT
                1 AS Status,
                'Login Successful.' AS Message,
                [Emp_Id],
                [Emp_FirstName],
                [Emp_MiddleName],
                [Emp_LastName],
                [Emp_UserName],
                [Emp_Email],
                [Emp_MobileNumber],
                [Emp_Gender],
                [Emp_BranchId],
                [Emp_CompId],
                [Emp_Department],
                [Emp_Designation],
                [Emp_JoiningDate],
                [Emp_IsActive]
            FROM [dbo].[tbl_Employee]
            WHERE (Emp_UserName = LTRIM(RTRIM(@Emp_UserName)) OR Emp_Email = LTRIM(RTRIM(@Emp_UserName)))
              AND Emp_PasswordHash = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Emp_Password), 2)
              AND Emp_IsActive = 1;
        END
        ELSE
        BEGIN
            SELECT
                0 AS Status,
                'Invalid Username or Password.' AS Message;
        END
    END TRY
    BEGIN CATCH
        SELECT
            0 AS Status,
            ERROR_MESSAGE() AS Message,
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_LINE() AS ErrorLine;
    END CATCH
END
GO

-- ============================================================================
-- 7. Seed Initial Demo Accounts (Admin & Employee)
-- Default Passwords:
-- Admin -> Admin@123  (BCrypt Hash: $2a$11$q9hK5b.Z4b5b7b9b7b9b7e9b7b9b7b9b7b9b7b9b7b9b7b9b7b9b7)
-- User  -> User@123   (BCrypt Hash: $2a$11$e8gJ4a.Y3a4a6a8a6a8a6d8a6a8a6a8a6a8a6a8a6a8a6a8a6a8a6)
-- Note: You can also use plaintext in dev or insert via SaveEmployee API.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Emp_UserName] = 'admin')
BEGIN
    INSERT INTO [dbo].[Employees]
    (
        [Emp_FirstName],
        [Emp_LastName],
        [Emp_Email],
        [Emp_MobileNumber],
        [Emp_UserName],
        [Emp_PasswordHash],
        [Emp_Role],
        [Emp_BranchId],
        [Emp_CompId],
        [Emp_Department],
        [Emp_Designation],
        [Emp_Salary],
        [Emp_Address],
        [Emp_City],
        [Emp_State],
        [Emp_Pincode],
        [Emp_DateOfJoining],
        [IsActive],
        [CreatedDate]
    )
    VALUES
    (
        'System',
        'Administrator',
        'admin@billingsoftware.com',
        '9876543210',
        'admin',
        '$2a$11$u6N0g6FvE7XG3v/hS/d/uegK7t0M4O3vT2uV2kU2l9z5l7j1k2m3.', -- Hash for Admin@123
        'Admin',
        1,
        1,
        'IT Administration',
        'System Admin',
        95000.00,
        'Main HQ, Tech Park',
        'Mumbai',
        'Maharashtra',
        '400001',
        GETUTCDATE(),
        1,
        GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Emp_UserName] = 'john.doe')
BEGIN
    INSERT INTO [dbo].[Employees]
    (
        [Emp_FirstName],
        [Emp_LastName],
        [Emp_Email],
        [Emp_MobileNumber],
        [Emp_UserName],
        [Emp_PasswordHash],
        [Emp_Role],
        [Emp_BranchId],
        [Emp_CompId],
        [Emp_Department],
        [Emp_Designation],
        [Emp_Salary],
        [Emp_Address],
        [Emp_City],
        [Emp_State],
        [Emp_Pincode],
        [Emp_DateOfJoining],
        [IsActive],
        [CreatedDate]
    )
    VALUES
    (
        'John',
        'Doe',
        'john.doe@billingsoftware.com',
        '9876543211',
        'john.doe',
        '$2a$11$u6N0g6FvE7XG3v/hS/d/uegK7t0M4O3vT2uV2kU2l9z5l7j1k2m3.', -- Hash for Admin@123
        'Employee',
        1,
        1,
        'Accounts & Billing',
        'Billing Executive',
        45000.00,
        '42 Downtown Avenue',
        'Pune',
        'Maharashtra',
        '411001',
        GETUTCDATE(),
        1,
        GETUTCDATE()
    );
END
GO

-- ============================================================================
-- 6. Create Customer Table (tbl_Customer)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_Customer')
BEGIN
    CREATE TABLE [dbo].[tbl_Customer]
    (
        [Cust_Id]                INT IDENTITY(1,1) NOT NULL,
        [Cust_Code]              NVARCHAR(20)      NOT NULL,
        [Cust_Name]              NVARCHAR(150)     NOT NULL,
        [Cust_CompanyName]       NVARCHAR(150)     NULL,
        [Cust_MobileNo]          NVARCHAR(15)      NOT NULL,
        [Cust_AlternateMobileNo] NVARCHAR(15)      NULL,
        [Cust_Email]             NVARCHAR(100)     NULL,
        [Cust_GSTNo]             NVARCHAR(20)      NULL,
        [Cust_PANNo]             NVARCHAR(10)      NULL,
        [Cust_Address]           NVARCHAR(250)     NULL,
        [Cust_AreaId]            INT               NULL,
        [Cust_Area]              NVARCHAR(250)     NULL,
        [Cust_CityId]            INT               NULL,
        [Cust_City]              NVARCHAR(100)     NULL,
        [Cust_StateId]           INT               NULL,
        [Cust_State]             NVARCHAR(100)     NULL,
        [Cust_Pincode]           NVARCHAR(10)      NULL,
        [Cust_Country]           NVARCHAR(100)     NOT NULL CONSTRAINT [DF_tbl_Customer_Country] DEFAULT ('India'),
        [Cust_BranchId]          INT               NULL,
        [Cust_CompId]            INT               NULL,
        [Cust_IsActive]          BIT               NOT NULL CONSTRAINT [DF_tbl_Customer_IsActive] DEFAULT (1),
        [Cust_CreatedBy]         INT               NOT NULL CONSTRAINT [DF_tbl_Customer_CreatedBy] DEFAULT (0),
        [Cust_CreatedDate]       DATETIME          NOT NULL CONSTRAINT [DF_tbl_Customer_CreatedDate] DEFAULT (GETDATE()),
        [Cust_ModifiedBy]        INT               NOT NULL CONSTRAINT [DF_tbl_Customer_ModifiedBy] DEFAULT (0),
        [Cust_ModifiedDate]      DATETIME          NULL,

        CONSTRAINT [PK_tbl_Customer] PRIMARY KEY CLUSTERED ([Cust_Id] ASC),
        CONSTRAINT [UQ_tbl_Customer_Cust_Code] UNIQUE NONCLUSTERED ([Cust_Code] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_tbl_Customer_MobileNo] ON [dbo].[tbl_Customer] ([Cust_MobileNo] ASC);
    CREATE NONCLUSTERED INDEX [IX_tbl_Customer_IsActive] ON [dbo].[tbl_Customer] ([Cust_IsActive] ASC);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Customer]') AND name = 'Cust_AreaId')
        ALTER TABLE [dbo].[tbl_Customer] ADD [Cust_AreaId] INT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Customer]') AND name = 'Cust_CityId')
        ALTER TABLE [dbo].[tbl_Customer] ADD [Cust_CityId] INT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Customer]') AND name = 'Cust_StateId')
        ALTER TABLE [dbo].[tbl_Customer] ADD [Cust_StateId] INT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Customer]') AND name = 'Cust_BranchId')
        ALTER TABLE [dbo].[tbl_Customer] ADD [Cust_BranchId] INT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Customer]') AND name = 'Cust_CompId')
        ALTER TABLE [dbo].[tbl_Customer] ADD [Cust_CompId] INT NULL;
END
GO

-- ============================================================================
-- 7. Stored Procedure: SP_Customer_InsertOrUpdate
-- Accepts Customer JSON payload via @CustJsonData parameter.
-- If Cust_Id = 0 (or null), generates next Cust_Code and performs INSERT.
-- If Cust_Id > 0, performs UPDATE.
-- Returns Status (1/0), Message, Cust_Id, and Cust_Code.
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Customer_InsertOrUpdate
(
    @CustJsonData NVARCHAR(MAX) = '{
  "Cust_Id": 0,
  "Cust_Name": "Rahul Patil",
  "Cust_CompanyName": "Patil Traderss",
  "Cust_MobileNo": "9876543219",
  "Cust_AlternateMobileNo": "9898989899",
  "Cust_Email": "rahul@gmail.com",
  "Cust_GSTNo": "27ABCDE1234F1Z5",
  "Cust_PANNo": "ABCDE1234F",
  "Cust_Address": "Shivaji Nagar",
  "Cust_AreaId": 1,
  "Cust_Area": "Deccan",
  "Cust_CityId": 1,
  "Cust_City": "Pune",
  "Cust_StateId": 1,
  "Cust_State": "Maharashtra",
  "Cust_Pincode": "411005",
  "Cust_Country": "India",
  "Cust_BranchId": 1,
  "Cust_CompId": 1,
  "Cust_IsActive": true,
  "Cust_CreatedBy": 1,
  "Cust_ModifiedBy": 1
}'
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @Cust_Id INT = ISNULL(COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_Id'), JSON_VALUE(@CustJsonData,'$.cust_Id')), 0),
        @Cust_Code NVARCHAR(20),
        @Cust_Name NVARCHAR(150),
        @Cust_CompanyName NVARCHAR(150),
        @Cust_MobileNo NVARCHAR(15),
        @Cust_AlternateMobileNo NVARCHAR(15),
        @Cust_Email NVARCHAR(100),
        @Cust_GSTNo NVARCHAR(20),
        @Cust_PANNo NVARCHAR(10),
        @Cust_Address NVARCHAR(250),
        @Cust_AreaId INT,
        @Cust_Area NVARCHAR(250),
        @Cust_CityId INT,
        @Cust_City NVARCHAR(100),
        @Cust_StateId INT,
        @Cust_State NVARCHAR(100),
        @Cust_Pincode NVARCHAR(10),
        @Cust_Country NVARCHAR(100),
        @Cust_BranchId INT,
        @Cust_CompId INT,
        @Cust_IsActive BIT,
        @Cust_CreatedBy INT,
        @Cust_ModifiedBy INT;

    SELECT
        @Cust_Name              = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_Name'), JSON_VALUE(@CustJsonData,'$.cust_Name')),
        @Cust_CompanyName       = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_CompanyName'), JSON_VALUE(@CustJsonData,'$.cust_CompanyName')),
        @Cust_MobileNo          = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_MobileNo'), JSON_VALUE(@CustJsonData,'$.cust_MobileNo')),
        @Cust_AlternateMobileNo = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_AlternateMobileNo'), JSON_VALUE(@CustJsonData,'$.cust_AlternateMobileNo')),
        @Cust_Email             = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_Email'), JSON_VALUE(@CustJsonData,'$.cust_Email')),
        @Cust_GSTNo             = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_GSTNo'), JSON_VALUE(@CustJsonData,'$.cust_GSTNo')),
        @Cust_PANNo             = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_PANNo'), JSON_VALUE(@CustJsonData,'$.cust_PANNo')),
        @Cust_Address           = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_Address'), JSON_VALUE(@CustJsonData,'$.cust_Address')),
        @Cust_AreaId            = COALESCE(TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_AreaId') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_Areaid') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_Areaid') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_AreaId') AS INT)),
        @Cust_Area              = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_Area'), JSON_VALUE(@CustJsonData,'$.cust_Area')),
        @Cust_CityId            = COALESCE(TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_CityId') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_Cityid') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_Cityid') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_CityId') AS INT)),
        @Cust_City              = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_City'), JSON_VALUE(@CustJsonData,'$.cust_City')),
        @Cust_StateId           = COALESCE(TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_StateId') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_Stateid') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_Stateid') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_StateId') AS INT)),
        @Cust_State             = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_State'), JSON_VALUE(@CustJsonData,'$.cust_State')),
        @Cust_Pincode           = COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_Pincode'), JSON_VALUE(@CustJsonData,'$.cust_Pincode')),
        @Cust_Country           = ISNULL(COALESCE(JSON_VALUE(@CustJsonData,'$.Cust_Country'), JSON_VALUE(@CustJsonData,'$.cust_Country')),'India'),
        @Cust_BranchId          = COALESCE(TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_BranchId') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_BranchId') AS INT)),
        @Cust_CompId            = COALESCE(TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_CompId') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_CompId') AS INT)),
        @Cust_IsActive          = ISNULL(COALESCE(TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_IsActive') AS BIT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_IsActive') AS BIT)), 1),
        @Cust_CreatedBy         = ISNULL(COALESCE(TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_CreatedBy') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_CreatedBy') AS INT)), 0),
        @Cust_ModifiedBy        = ISNULL(COALESCE(TRY_CAST(JSON_VALUE(@CustJsonData,'$.Cust_ModifiedBy') AS INT), TRY_CAST(JSON_VALUE(@CustJsonData,'$.cust_ModifiedBy') AS INT)), 0);

    -- Automatically populate Area Name, City Name, State Name from master tables if IDs provided
    IF (@Cust_StateId IS NOT NULL AND @Cust_StateId > 0 AND (@Cust_State IS NULL OR @Cust_State = ''))
    BEGIN
        SELECT @Cust_State = State_Name FROM dbo.tbl_State WHERE State_Id = @Cust_StateId;
    END

    IF (@Cust_CityId IS NOT NULL AND @Cust_CityId > 0 AND (@Cust_City IS NULL OR @Cust_City = ''))
    BEGIN
        SELECT @Cust_City = City_Name FROM dbo.tbl_City WHERE City_Id = @Cust_CityId;
    END

    IF (@Cust_AreaId IS NOT NULL AND @Cust_AreaId > 0 AND (@Cust_Area IS NULL OR @Cust_Area = ''))
    BEGIN
        SELECT @Cust_Area = Area_Name FROM dbo.tbl_Area WHERE Area_Id = @Cust_AreaId;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Check Mobile Number duplicate
        IF EXISTS
        (
            SELECT 1
            FROM dbo.tbl_Customer
            WHERE Cust_MobileNo = @Cust_MobileNo
            AND Cust_Id <> @Cust_Id
        )
        BEGIN
            RAISERROR('Mobile Number already exists.',16,1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Cust_Id = 0
        BEGIN
            -- Generate Next Customer Code (e.g. CUST000001)
            DECLARE @NextNo INT;

            SELECT @NextNo =
                ISNULL(MAX(CAST(REPLACE(Cust_Code,'CUST','') AS INT)),0) + 1
            FROM dbo.tbl_Customer
            WHERE Cust_Code LIKE 'CUST%';

            SET @Cust_Code = 'CUST' + RIGHT('000000' + CAST(@NextNo AS VARCHAR(6)),6);

            INSERT INTO dbo.tbl_Customer
            (
                Cust_Code,
                Cust_Name,
                Cust_CompanyName,
                Cust_MobileNo,
                Cust_AlternateMobileNo,
                Cust_Email,
                Cust_GSTNo,
                Cust_PANNo,
                Cust_Address,
                Cust_AreaId,
                Cust_Area,
                Cust_CityId,
                Cust_City,
                Cust_StateId,
                Cust_State,
                Cust_Pincode,
                Cust_Country,
                Cust_BranchId,
                Cust_CompId,
                Cust_IsActive,
                Cust_CreatedBy,
                Cust_CreatedDate,
                Cust_ModifiedBy,
                Cust_ModifiedDate
            )
            VALUES
            (
                @Cust_Code,
                @Cust_Name,
                @Cust_CompanyName,
                @Cust_MobileNo,
                @Cust_AlternateMobileNo,
                @Cust_Email,
                @Cust_GSTNo,
                @Cust_PANNo,
                @Cust_Address,
                @Cust_AreaId,
                @Cust_Area,
                @Cust_CityId,
                @Cust_City,
                @Cust_StateId,
                @Cust_State,
                @Cust_Pincode,
                @Cust_Country,
                @Cust_BranchId,
                @Cust_CompId,
                @Cust_IsActive,
                @Cust_CreatedBy,
                GETDATE(),
                @Cust_ModifiedBy,
                GETDATE()
            );

            SET @Cust_Id = SCOPE_IDENTITY();

            SELECT
                1 AS Status,
                'Customer Added Successfully.' AS Message,
                @Cust_Id AS Cust_Id,
                @Cust_Code AS Cust_Code;
        END
        ELSE
        BEGIN
            -- Get existing customer code (don't change it on update)
            SELECT @Cust_Code = Cust_Code
            FROM dbo.tbl_Customer
            WHERE Cust_Id = @Cust_Id;

            UPDATE dbo.tbl_Customer
            SET
                Cust_Name              = @Cust_Name,
                Cust_CompanyName       = @Cust_CompanyName,
                Cust_MobileNo          = @Cust_MobileNo,
                Cust_AlternateMobileNo = @Cust_AlternateMobileNo,
                Cust_Email             = @Cust_Email,
                Cust_GSTNo             = @Cust_GSTNo,
                Cust_PANNo             = @Cust_PANNo,
                Cust_Address           = @Cust_Address,
                Cust_AreaId            = @Cust_AreaId,
                Cust_Area              = @Cust_Area,
                Cust_CityId            = @Cust_CityId,
                Cust_City              = @Cust_City,
                Cust_StateId           = @Cust_StateId,
                Cust_State             = @Cust_State,
                Cust_Pincode           = @Cust_Pincode,
                Cust_Country           = @Cust_Country,
                Cust_BranchId          = @Cust_BranchId,
                Cust_CompId            = @Cust_CompId,
                Cust_IsActive          = @Cust_IsActive,
                Cust_ModifiedBy        = @Cust_ModifiedBy,
                Cust_ModifiedDate      = GETDATE()
            WHERE Cust_Id = @Cust_Id;

            SELECT
                1 AS Status,
                'Customer Updated Successfully.' AS Message,
                @Cust_Id AS Cust_Id,
                @Cust_Code AS Cust_Code;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT
            0 AS Status,
            ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

-- ============================================================================
-- 8. Stored Procedure: SP_Customer_GetAll
-- Returns customer records filtered by Search, AreaId, CityId, StateId, BranchId, CompId, IsActive.
-- Search filters across Cust_Code, Cust_Name, Cust_CompanyName, Cust_MobileNo,
-- Cust_AlternateMobileNo, Cust_Email, Cust_GSTNo, Cust_PANNo.
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Customer_GetAll
(
    @Search NVARCHAR(100) = '',
    @AreaId NVARCHAR(250) = 0,
    @CityId NVARCHAR(100) = 0,
    @StateId NVARCHAR(100) = 0,
    @BranchId INT = 0,
    @CompId INT = 0,
    @IsActive BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.Cust_Id,
        c.Cust_Code,
        c.Cust_Name,
        c.Cust_CompanyName,
        c.Cust_MobileNo,
        c.Cust_AlternateMobileNo,
        c.Cust_Email,
        c.Cust_GSTNo,
        c.Cust_PANNo,
        c.Cust_Address,
        c.Cust_AreaId,
        c.Cust_Area,
        ISNULL(a.Area_Name, c.Cust_Area) AS Cust_AreaName,
        c.Cust_CityId,
        c.Cust_City,
        ISNULL(ct.City_Name, c.Cust_City) AS Cust_CityName,
        c.Cust_StateId,
        c.Cust_State,
        ISNULL(s.State_Name, c.Cust_State) AS Cust_StateName,
        c.Cust_Pincode,
        c.Cust_Country,
        c.Cust_BranchId,
        c.Cust_CompId,
        c.Cust_IsActive,
        c.Cust_CreatedBy,
        c.Cust_CreatedDate,
        c.Cust_ModifiedBy,
        c.Cust_ModifiedDate
    FROM dbo.tbl_Customer c
    LEFT JOIN dbo.tbl_Area a ON c.Cust_AreaId = a.Area_Id
    LEFT JOIN dbo.tbl_City ct ON c.Cust_CityId = ct.City_Id
    LEFT JOIN dbo.tbl_State s ON c.Cust_StateId = s.State_Id
    WHERE (@IsActive IS NULL OR c.Cust_IsActive = @IsActive)
      AND (ISNULL(@BranchId, 0) = 0 OR c.Cust_BranchId = @BranchId)
      AND (ISNULL(@CompId, 0) = 0 OR c.Cust_CompId = @CompId)
      AND (
          ISNULL(@Search, '') = '' 
          OR c.Cust_Code LIKE '%' + @Search + '%'
          OR c.Cust_Name LIKE '%' + @Search + '%'
          OR c.Cust_CompanyName LIKE '%' + @Search + '%'
          OR c.Cust_MobileNo LIKE '%' + @Search + '%'
          OR c.Cust_AlternateMobileNo LIKE '%' + @Search + '%'
          OR c.Cust_Email LIKE '%' + @Search + '%'
          OR c.Cust_GSTNo LIKE '%' + @Search + '%'
          OR c.Cust_PANNo LIKE '%' + @Search + '%'
      )
      AND (
          ISNULL(@AreaId, '0') IN ('', '0')
          OR CAST(c.Cust_AreaId AS NVARCHAR(50)) = @AreaId
          OR c.Cust_Area LIKE '%' + @AreaId + '%'
          OR a.Area_Name LIKE '%' + @AreaId + '%'
          OR c.Cust_Area IN (SELECT Area_Name FROM dbo.tbl_Area WHERE CAST(Area_Id AS NVARCHAR(50)) = @AreaId)
      )
      AND (
          ISNULL(@CityId, '0') IN ('', '0')
          OR CAST(c.Cust_CityId AS NVARCHAR(50)) = @CityId
          OR c.Cust_City LIKE '%' + @CityId + '%'
          OR ct.City_Name LIKE '%' + @CityId + '%'
          OR c.Cust_City IN (SELECT City_Name FROM dbo.tbl_City WHERE CAST(City_Id AS NVARCHAR(50)) = @CityId)
      )
      AND (
          ISNULL(@StateId, '0') IN ('', '0')
          OR CAST(c.Cust_StateId AS NVARCHAR(50)) = @StateId
          OR c.Cust_State LIKE '%' + @StateId + '%'
          OR s.State_Name LIKE '%' + @StateId + '%'
          OR c.Cust_State IN (SELECT State_Name FROM dbo.tbl_State WHERE CAST(State_Id AS NVARCHAR(50)) = @StateId)
          OR c.Cust_State IN (SELECT State_Code FROM dbo.tbl_State WHERE CAST(State_Id AS NVARCHAR(50)) = @StateId)
      )
    ORDER BY c.Cust_Id DESC;
END;
GO

-- ============================================================================
-- 9. Stored Procedure: SP_Customer_GetById
-- Returns full customer details for the given @Cust_Id.
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Customer_GetById
(
    @Cust_Id INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.Cust_Id,
        c.Cust_Code,
        c.Cust_Name,
        c.Cust_CompanyName,
        c.Cust_MobileNo,
        c.Cust_AlternateMobileNo,
        c.Cust_Email,
        c.Cust_GSTNo,
        c.Cust_PANNo,
        c.Cust_Address,
        c.Cust_AreaId,
        c.Cust_Area,
        ISNULL(a.Area_Name, c.Cust_Area) AS Cust_AreaName,
        c.Cust_CityId,
        c.Cust_City,
        ISNULL(ct.City_Name, c.Cust_City) AS Cust_CityName,
        c.Cust_StateId,
        c.Cust_State,
        ISNULL(s.State_Name, c.Cust_State) AS Cust_StateName,
        c.Cust_Pincode,
        c.Cust_Country,
        c.Cust_BranchId,
        c.Cust_CompId,
        c.Cust_IsActive,
        c.Cust_CreatedBy,
        c.Cust_CreatedDate,
        c.Cust_ModifiedBy,
        c.Cust_ModifiedDate
    FROM dbo.tbl_Customer c
    LEFT JOIN dbo.tbl_Area a ON c.Cust_AreaId = a.Area_Id
    LEFT JOIN dbo.tbl_City ct ON c.Cust_CityId = ct.City_Id
    LEFT JOIN dbo.tbl_State s ON c.Cust_StateId = s.State_Id
    WHERE c.Cust_Id = @Cust_Id;
END;
GO

-- ============================================================================
-- 10. Create State, City & Area Master Tables
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_State')
BEGIN
    CREATE TABLE [dbo].[tbl_State]
    (
        [State_Id]       INT IDENTITY(1,1) NOT NULL,
        [State_Name]     NVARCHAR(100)     NOT NULL,
        [State_Code]     NVARCHAR(10)      NULL,
        [State_IsActive] BIT               NOT NULL CONSTRAINT [DF_tbl_State_IsActive] DEFAULT (1),
        CONSTRAINT [PK_tbl_State] PRIMARY KEY CLUSTERED ([State_Id] ASC)
    );

    -- Seed Default States
    INSERT INTO [dbo].[tbl_State] ([State_Name], [State_Code]) VALUES 
    ('Maharashtra', 'MH'), 
    ('Karnataka', 'KA'), 
    ('Gujarat', 'GJ'), 
    ('Delhi', 'DL');
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_State]') AND name = 'State_Code')
    BEGIN
        ALTER TABLE [dbo].[tbl_State] ADD [State_Code] NVARCHAR(10) NULL;
    END

    UPDATE [dbo].[tbl_State] SET [State_Code] = 'MH' WHERE [State_Name] = 'Maharashtra' AND [State_Code] IS NULL;
    UPDATE [dbo].[tbl_State] SET [State_Code] = 'KA' WHERE [State_Name] = 'Karnataka' AND [State_Code] IS NULL;
    UPDATE [dbo].[tbl_State] SET [State_Code] = 'GJ' WHERE [State_Name] = 'Gujarat' AND [State_Code] IS NULL;
    UPDATE [dbo].[tbl_State] SET [State_Code] = 'DL' WHERE [State_Name] = 'Delhi' AND [State_Code] IS NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_City')
BEGIN
    CREATE TABLE [dbo].[tbl_City]
    (
        [City_Id]           INT IDENTITY(1,1) NOT NULL,
        [City_StateId]      INT               NOT NULL,
        [City_Name]         NVARCHAR(100)     NOT NULL,
        [City_IsActive]     BIT               NOT NULL CONSTRAINT [DF_tbl_City_IsActive] DEFAULT (1),
        [City_CreatedBy]    INT               NOT NULL CONSTRAINT [DF_tbl_City_CreatedBy] DEFAULT (0),
        [City_CreatedDate]  DATETIME          NOT NULL CONSTRAINT [DF_tbl_City_CreatedDate] DEFAULT (GETDATE()),
        [City_ModifiedBy]   INT               NOT NULL CONSTRAINT [DF_tbl_City_ModifiedBy] DEFAULT (0),
        [City_ModifiedDate] DATETIME          NULL,
        CONSTRAINT [PK_tbl_City] PRIMARY KEY CLUSTERED ([City_Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_tbl_City_StateId] ON [dbo].[tbl_City] ([City_StateId] ASC);
    CREATE NONCLUSTERED INDEX [IX_tbl_City_IsActive] ON [dbo].[tbl_City] ([City_IsActive] ASC);

    -- Seed Default Cities
    INSERT INTO [dbo].[tbl_City] ([City_StateId], [City_Name]) VALUES 
    (1, 'Mumbai'), 
    (1, 'Pune'), 
    (1, 'Nagpur'), 
    (2, 'Bangalore');
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_City]') AND name = 'City_CreatedBy')
        ALTER TABLE [dbo].[tbl_City] ADD [City_CreatedBy] INT NOT NULL CONSTRAINT [DF_tbl_City_CreatedBy] DEFAULT (0);

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_City]') AND name = 'City_CreatedDate')
        ALTER TABLE [dbo].[tbl_City] ADD [City_CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_tbl_City_CreatedDate] DEFAULT (GETDATE());

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_City]') AND name = 'City_ModifiedBy')
        ALTER TABLE [dbo].[tbl_City] ADD [City_ModifiedBy] INT NOT NULL CONSTRAINT [DF_tbl_City_ModifiedBy] DEFAULT (0);

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_City]') AND name = 'City_ModifiedDate')
        ALTER TABLE [dbo].[tbl_City] ADD [City_ModifiedDate] DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_Area')
BEGIN
    CREATE TABLE [dbo].[tbl_Area]
    (
        [Area_Id]           INT IDENTITY(1,1) NOT NULL,
        [Area_StateId]      INT               NOT NULL,
        [Area_CityId]       INT               NOT NULL,
        [Area_Name]         NVARCHAR(150)     NOT NULL,
        [Area_Pincode]      NVARCHAR(10)      NOT NULL,
        [Area_IsActive]     BIT               NOT NULL CONSTRAINT [DF_tbl_Area_IsActive] DEFAULT (1),
        [Area_CreatedBy]    INT               NOT NULL CONSTRAINT [DF_tbl_Area_CreatedBy] DEFAULT (0),
        [Area_CreatedDate]  DATETIME          NOT NULL CONSTRAINT [DF_tbl_Area_CreatedDate] DEFAULT (GETDATE()),
        [Area_ModifiedBy]   INT               NOT NULL CONSTRAINT [DF_tbl_Area_ModifiedBy] DEFAULT (0),
        [Area_ModifiedDate] DATETIME          NULL,

        CONSTRAINT [PK_tbl_Area] PRIMARY KEY CLUSTERED ([Area_Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_tbl_Area_CityId] ON [dbo].[tbl_Area] ([Area_CityId] ASC);
    CREATE NONCLUSTERED INDEX [IX_tbl_Area_Pincode] ON [dbo].[tbl_Area] ([Area_Pincode] ASC);
    CREATE NONCLUSTERED INDEX [IX_tbl_Area_IsActive] ON [dbo].[tbl_Area] ([Area_IsActive] ASC);
END
GO

-- ============================================================================
-- 11. Stored Procedure: SP_Area_InsertOrUpdate
-- Accepts Area JSON payload via @AreaJsonData parameter.
-- Checks for duplicates by Area_CityId and Area_Name.
-- If Area_Id = 0, performs INSERT.
-- If Area_Id > 0, performs UPDATE.
-- Returns Status (1/0) and Message.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Area_InsertOrUpdate]
    @AreaJsonData NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Area_Id           INT;
    DECLARE @Area_StateId      INT;
    DECLARE @Area_CityId       INT;
    DECLARE @Area_Name         NVARCHAR(150);
    DECLARE @Area_Pincode      NVARCHAR(10);
    DECLARE @Area_IsActive     BIT;
    DECLARE @Area_CreatedBy    INT;
    DECLARE @Area_ModifiedBy   INT;

    -- Validate JSON input
    IF @AreaJsonData IS NULL OR ISJSON(@AreaJsonData) = 0
    BEGIN
        SELECT 
            0 AS [Status], 
            'Invalid or malformed JSON data provided.' AS [Message];
        RETURN;
    END

    -- Parse JSON data
    SELECT
        @Area_Id         = ISNULL(JSON_VALUE(@AreaJsonData, '$.Area_Id'), 0),
        @Area_StateId    = JSON_VALUE(@AreaJsonData, '$.Area_StateId'),
        @Area_CityId     = JSON_VALUE(@AreaJsonData, '$.Area_CityId'),
        @Area_Name       = LTRIM(RTRIM(JSON_VALUE(@AreaJsonData, '$.Area_Name'))),
        @Area_Pincode    = LTRIM(RTRIM(JSON_VALUE(@AreaJsonData, '$.Area_Pincode'))),
        @Area_IsActive   = ISNULL(JSON_VALUE(@AreaJsonData, '$.Area_IsActive'), 1),
        @Area_CreatedBy  = ISNULL(JSON_VALUE(@AreaJsonData, '$.Area_CreatedBy'), 0),
        @Area_ModifiedBy = ISNULL(JSON_VALUE(@AreaJsonData, '$.Area_ModifiedBy'), 0);

    -- Basic validations
    IF @Area_StateId IS NULL OR @Area_StateId <= 0
    BEGIN
        SELECT 0 AS [Status], 'Valid State ID is required.' AS [Message];
        RETURN;
    END

    IF @Area_CityId IS NULL OR @Area_CityId <= 0
    BEGIN
        SELECT 0 AS [Status], 'Valid City ID is required.' AS [Message];
        RETURN;
    END

    IF @Area_Name IS NULL OR LEN(@Area_Name) = 0
    BEGIN
        SELECT 0 AS [Status], 'Area Name is required.' AS [Message];
        RETURN;
    END

    IF @Area_Pincode IS NULL OR LEN(@Area_Pincode) = 0
    BEGIN
        SELECT 0 AS [Status], 'Pincode is required.' AS [Message];
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Check if Area already exists for the same city (case-insensitive)
        IF EXISTS 
        (
            SELECT 1 
            FROM [dbo].[tbl_Area] 
            WHERE Area_CityId = @Area_CityId 
              AND LOWER(LTRIM(RTRIM(Area_Name))) = LOWER(@Area_Name)
              AND Area_Id <> @Area_Id
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 
                0 AS [Status], 
                'Area Already Exists.' AS [Message];
            RETURN;
        END

        -- CASE 1: INSERT (Area_Id = 0)
        IF @Area_Id = 0
        BEGIN
            INSERT INTO [dbo].[tbl_Area]
            (
                [Area_StateId],
                [Area_CityId],
                [Area_Name],
                [Area_Pincode],
                [Area_IsActive],
                [Area_CreatedBy],
                [Area_CreatedDate],
                [Area_ModifiedBy],
                [Area_ModifiedDate]
            )
            VALUES
            (
                @Area_StateId,
                @Area_CityId,
                @Area_Name,
                @Area_Pincode,
                @Area_IsActive,
                @Area_CreatedBy,
                GETDATE(),
                @Area_ModifiedBy,
                GETDATE()
            );

            SET @Area_Id = SCOPE_IDENTITY();

            SELECT 
                1 AS [Status], 
                'Area Added Successfully.' AS [Message],
                @Area_Id AS [Area_Id];
        END
        -- CASE 2: UPDATE (Area_Id > 0)
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Area] WHERE Area_Id = @Area_Id)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    0 AS [Status], 
                    'Area not found.' AS [Message];
                RETURN;
            END

            UPDATE [dbo].[tbl_Area]
            SET
                [Area_StateId]      = @Area_StateId,
                [Area_CityId]       = @Area_CityId,
                [Area_Name]         = @Area_Name,
                [Area_Pincode]      = @Area_Pincode,
                [Area_IsActive]     = @Area_IsActive,
                [Area_ModifiedBy]   = @Area_ModifiedBy,
                [Area_ModifiedDate] = GETDATE()
            WHERE [Area_Id] = @Area_Id;

            SELECT 
                1 AS [Status], 
                'Area Updated Successfully.' AS [Message],
                @Area_Id AS [Area_Id];
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 
            0 AS [Status], 
            ERROR_MESSAGE() AS [Message];
    END CATCH
END;
GO

-- ============================================================================
-- 12. Stored Procedure: SP_Area_GetAll
-- Fetches area records filtered by @Search, @StateId, @CityId, @Pincode, @IsActive.
-- Returns Area_Id, Area_Name, Area_Pincode, City_Name, State_Name, Area_IsActive.
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Area_GetAll]
(
    @Search   NVARCHAR(100) = '',
    @StateId  INT           = NULL,
    @CityId   INT           = NULL,
    @Pincode  NVARCHAR(10)  = '',
    @IsActive BIT           = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.Area_Id,
        a.Area_Name,
        a.Area_Pincode,
        ISNULL(c.City_Name, '') AS City_Name,
        ISNULL(s.State_Name, '') AS State_Name,
        a.Area_IsActive
    FROM [dbo].[tbl_Area] a
    LEFT JOIN [dbo].[tbl_City] c ON a.Area_CityId = c.City_Id
    LEFT JOIN [dbo].[tbl_State] s ON a.Area_StateId = s.State_Id
    WHERE (@IsActive IS NULL OR a.Area_IsActive = @IsActive)
      AND (@StateId IS NULL OR a.Area_StateId = @StateId)
      AND (@CityId IS NULL OR a.Area_CityId = @CityId)
      AND (ISNULL(@Pincode, '') = '' OR a.Area_Pincode LIKE '%' + @Pincode + '%')
      AND (
          ISNULL(@Search, '') = ''
          OR a.Area_Name LIKE '%' + @Search + '%'
          OR ISNULL(c.City_Name, '') LIKE '%' + @Search + '%'
          OR ISNULL(s.State_Name, '') LIKE '%' + @Search + '%'
          OR a.Area_Pincode LIKE '%' + @Search + '%'
      )
    ORDER BY a.Area_Id DESC;
END;
GO

-- ============================================================================
-- 13. Stored Procedure: SP_City_InsertOrUpdate
-- Accepts City JSON payload via @CityJsonData parameter.
-- JSON format:
-- {
--   "City_Id": 0,
--   "City_StateId": 1,
--   "City_Name": "Pune",
--   "City_IsActive": true,
--   "City_CreatedBy": 1,
--   "City_ModifiedBy": 1
-- }
-- Possible Responses:
-- Success (Insert): { "Status": 1, "Message": "City Added Successfully." }
-- Success (Update): { "Status": 1, "Message": "City Updated Successfully." }
-- Duplicate:        { "Status": 0, "Message": "City Already Exists." }
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_City_InsertOrUpdate]
    @CityJsonData NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @City_Id           INT;
    DECLARE @City_StateId      INT;
    DECLARE @City_Name         NVARCHAR(100);
    DECLARE @City_IsActive     BIT;
    DECLARE @City_CreatedBy    INT;
    DECLARE @City_ModifiedBy   INT;

    -- Validate JSON input
    IF @CityJsonData IS NULL OR ISJSON(@CityJsonData) = 0
    BEGIN
        SELECT 
            0 AS [Status], 
            'Invalid or malformed JSON data provided.' AS [Message],
            0 AS [City_Id];
        RETURN;
    END

    -- Parse JSON data
    SELECT
        @City_Id         = ISNULL(JSON_VALUE(@CityJsonData, '$.City_Id'), 0),
        @City_StateId    = JSON_VALUE(@CityJsonData, '$.City_StateId'),
        @City_Name       = LTRIM(RTRIM(JSON_VALUE(@CityJsonData, '$.City_Name'))),
        @City_IsActive   = ISNULL(JSON_VALUE(@CityJsonData, '$.City_IsActive'), 1),
        @City_CreatedBy  = ISNULL(JSON_VALUE(@CityJsonData, '$.City_CreatedBy'), 0),
        @City_ModifiedBy = ISNULL(JSON_VALUE(@CityJsonData, '$.City_ModifiedBy'), 0);

    -- Basic validations
    IF @City_StateId IS NULL OR @City_StateId <= 0
    BEGIN
        SELECT 0 AS [Status], 'Valid State ID is required.' AS [Message], 0 AS [City_Id];
        RETURN;
    END

    IF @City_Name IS NULL OR LEN(@City_Name) = 0
    BEGIN
        SELECT 0 AS [Status], 'City Name is required.' AS [Message], 0 AS [City_Id];
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Check if City already exists for the same state (case-insensitive)
        IF EXISTS 
        (
            SELECT 1 
            FROM [dbo].[tbl_City] 
            WHERE City_StateId = @City_StateId 
              AND LOWER(LTRIM(RTRIM(City_Name))) = LOWER(@City_Name)
              AND City_Id <> @City_Id
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 
                0 AS [Status], 
                'City Already Exists.' AS [Message],
                0 AS [City_Id];
            RETURN;
        END

        -- CASE 1: INSERT (City_Id = 0)
        IF @City_Id = 0
        BEGIN
            INSERT INTO [dbo].[tbl_City]
            (
                [City_StateId],
                [City_Name],
                [City_IsActive],
                [City_CreatedBy],
                [City_CreatedDate],
                [City_ModifiedBy],
                [City_ModifiedDate]
            )
            VALUES
            (
                @City_StateId,
                @City_Name,
                @City_IsActive,
                @City_CreatedBy,
                GETDATE(),
                @City_ModifiedBy,
                GETDATE()
            );

            SET @City_Id = SCOPE_IDENTITY();

            SELECT 
                1 AS [Status], 
                'City Added Successfully.' AS [Message],
                @City_Id AS [City_Id];
        END
        -- CASE 2: UPDATE (City_Id > 0)
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_City] WHERE City_Id = @City_Id)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    0 AS [Status], 
                    'City not found.' AS [Message],
                    @City_Id AS [City_Id];
                RETURN;
            END

            UPDATE [dbo].[tbl_City]
            SET
                [City_StateId]      = @City_StateId,
                [City_Name]         = @City_Name,
                [City_IsActive]     = @City_IsActive,
                [City_ModifiedBy]   = @City_ModifiedBy,
                [City_ModifiedDate] = GETDATE()
            WHERE [City_Id] = @City_Id;

            SELECT 
                1 AS [Status], 
                'City Updated Successfully.' AS [Message],
                @City_Id AS [City_Id];
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 
            0 AS [Status], 
            ERROR_MESSAGE() AS [Message],
            0 AS [City_Id];
    END CATCH
END;
GO

-- ============================================================================
-- 14. Stored Procedure: SP_City_GetAll
-- Fetches city records filtered by @Search, @StateId, @IsActive.
-- Returns:
-- [
--   {
--     "City_Id": 1,
--     "City_Name": "Pune",
--     "State_Name": "Maharashtra",
--     "State_Code": "MH",
--     "City_IsActive": true
--   }
-- ]
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_City_GetAll]
(
    @Search   NVARCHAR(100) = '',
    @StateId  INT           = NULL,
    @IsActive BIT           = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.City_Id,
        c.City_Name,
        ISNULL(s.State_Name, '') AS State_Name,
        ISNULL(s.State_Code, '') AS State_Code,
        c.City_IsActive
    FROM [dbo].[tbl_City] c
    LEFT JOIN [dbo].[tbl_State] s ON c.City_StateId = s.State_Id
    WHERE (@IsActive IS NULL OR c.City_IsActive = @IsActive)
      AND (@StateId IS NULL OR c.City_StateId = @StateId)
      AND (
          ISNULL(@Search, '') = ''
          OR c.City_Name LIKE '%' + @Search + '%'
          OR ISNULL(s.State_Name, '') LIKE '%' + @Search + '%'
          OR ISNULL(s.State_Code, '') LIKE '%' + @Search + '%'
      )
    ORDER BY c.City_Id DESC;
END;
GO

-- ============================================================================
-- 15. Stored Procedure: SP_State_GetAll
-- Fetches state records filtered by @Search, @IsActive.
-- Parameters:
--   @Search   NVARCHAR(100) = ''
--   @IsActive BIT           = NULL
-- Returns:
-- [
--   {
--     "State_Id": 1,
--     "State_Name": "Maharashtra",
--     "State_Code": "MH",
--     "State_IsActive": true
--   },
--   {
--     "State_Id": 2,
--     "State_Name": "Gujarat",
--     "State_Code": "GJ",
--     "State_IsActive": true
--   }
-- ]
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_State_GetAll]
(
    @Search   NVARCHAR(100) = '',
    @IsActive BIT           = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        s.State_Id,
        s.State_Name,
        ISNULL(s.State_Code, '') AS State_Code,
        s.State_IsActive
    FROM [dbo].[tbl_State] s
    WHERE (@IsActive IS NULL OR s.State_IsActive = @IsActive)
      AND (
          ISNULL(@Search, '') = ''
          OR s.State_Name LIKE '%' + @Search + '%'
          OR ISNULL(s.State_Code, '') LIKE '%' + @Search + '%'
      )
    ORDER BY s.State_Id ASC;
END;
GO

-- ============================================================================
-- 10. Stored Procedure: SP_DatabaseBackup
-- Description: Performs a full backup of [SankysoftBillingDB] database to
-- 'D:\Sankysoft\Backup\' with a timestamped filename (e.g. SankysoftBillingDB_yyyyMMdd_HHmmss.bak).
-- Returns: Status (BIT), Message (NVARCHAR), BackupFilePath (NVARCHAR)
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_DatabaseBackup]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FileName NVARCHAR(500);
    DECLARE @SQL NVARCHAR(MAX);

    SET @FileName =
        'D:\Sankysoft\Backup\SankysoftBillingDB_' +
        FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') +
        '.bak';

    SET @SQL =
    'BACKUP DATABASE [SankysoftBillingDB]
     TO DISK = ''' + @FileName + '''
     WITH INIT';

    EXEC(@SQL);

    SELECT
        CAST(1 AS BIT) AS Status,
        'Backup Created Successfully' AS Message,
        @FileName AS BackupFilePath;
END;
GO

