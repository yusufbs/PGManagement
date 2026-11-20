-- Database Creation
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PGManagementDb')
BEGIN
    CREATE DATABASE PGManagementDb;
END
GO

USE PGManagementDb;
GO

-- 1. Rooms Table
IF OBJECT_ID('dbo.Rooms', 'U') IS NULL
BEGIN
    CREATE TABLE Rooms (
        RoomId          INT IDENTITY(1,1) PRIMARY KEY,
        RoomNumber      NVARCHAR(20)   NOT NULL UNIQUE,
        Floor           INT            NOT NULL,
        RoomType        INT            NOT NULL,          -- 1: Single, 2: Double, 3: Triple, 4: Dormitory
        Capacity        INT            NOT NULL,
        MonthlyRent     DECIMAL(10,2)  NOT NULL,
        Amenities       NVARCHAR(500)  NULL,
        Status          INT            NOT NULL DEFAULT 1, -- 1: Available, 2: Occupied, 3: Maintenance
        IsDeleted       BIT            NOT NULL DEFAULT 0,
        CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT CK_Room_Capacity CHECK (Capacity > 0)
    );

    CREATE INDEX IX_Rooms_Status ON Rooms(Status);
END
GO

-- 2. Tenants Table
IF OBJECT_ID('dbo.Tenants', 'U') IS NULL
BEGIN
    CREATE TABLE Tenants (
        TenantId        INT IDENTITY(1,1) PRIMARY KEY,
        FirstName       NVARCHAR(100)  NOT NULL,
        LastName        NVARCHAR(100)  NOT NULL,
        Email           NVARCHAR(255)  NOT NULL UNIQUE,
        Phone           NVARCHAR(20)   NOT NULL,
        IdProofType     NVARCHAR(50)   NOT NULL,           -- Aadhar, Passport, DL
        IdProofNumber   NVARCHAR(100)  NOT NULL,
        EmergencyContact NVARCHAR(200) NULL,
        ProfilePhotoUrl NVARCHAR(500)  NULL,
        IsActive        BIT            NOT NULL DEFAULT 1,
        IsDeleted       BIT            NOT NULL DEFAULT 0,
        CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- 3. Room Allocations Table
IF OBJECT_ID('dbo.RoomAllocations', 'U') IS NULL
BEGIN
    CREATE TABLE RoomAllocations (
        AllocationId    INT IDENTITY(1,1) PRIMARY KEY,
        RoomId          INT            NOT NULL REFERENCES Rooms(RoomId),
        TenantId        INT            NOT NULL REFERENCES Tenants(TenantId),
        CheckInDate     DATE           NOT NULL,
        CheckOutDate    DATE           NULL,
        SecurityDeposit DECIMAL(10,2)  NOT NULL DEFAULT 0,
        Status          INT            NOT NULL DEFAULT 1, -- 1: Active, 2: CheckedOut
        CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        INDEX IX_RoomAllocations_RoomId (RoomId),
        INDEX IX_RoomAllocations_TenantId (TenantId)
    );
END
GO

-- 4. Bills Table
IF OBJECT_ID('dbo.Bills', 'U') IS NULL
BEGIN
    CREATE TABLE Bills (
        BillId          INT IDENTITY(1,1) PRIMARY KEY,
        AllocationId    INT            NOT NULL REFERENCES RoomAllocations(AllocationId),
        BillMonth       INT            NOT NULL,
        BillYear        INT            NOT NULL,
        RentAmount      DECIMAL(10,2)  NOT NULL,
        ElectricityCharges DECIMAL(10,2) NOT NULL DEFAULT 0,
        WaterCharges    DECIMAL(10,2)  NOT NULL DEFAULT 0,
        OtherCharges    DECIMAL(10,2)  NOT NULL DEFAULT 0,
        Discount        DECIMAL(10,2)  NOT NULL DEFAULT 0,
        TotalAmount     AS (RentAmount + ElectricityCharges + WaterCharges + OtherCharges - Discount) PERSISTED,
        DueDate         DATE           NOT NULL,
        GeneratedAt     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        Notes           NVARCHAR(500)  NULL,
        CONSTRAINT CK_Bill_Month CHECK (BillMonth BETWEEN 1 AND 12),
        CONSTRAINT UQ_Bill_Allocation_Period UNIQUE (AllocationId, BillMonth, BillYear),
        INDEX IX_Bills_AllocationId (AllocationId)
    );
END
GO

-- 5. Payments Table
IF OBJECT_ID('dbo.Payments', 'U') IS NULL
BEGIN
    CREATE TABLE Payments (
        PaymentId       INT IDENTITY(1,1) PRIMARY KEY,
        BillId          INT            NOT NULL REFERENCES Bills(BillId),
        AmountPaid      DECIMAL(10,2)  NOT NULL,
        PaymentDate     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        PaymentMode     INT            NOT NULL,           -- 1: Cash, 2: UPI, 3: BankTransfer, 4: Card
        TransactionRef  NVARCHAR(200)  NULL,
        ReceivedBy      NVARCHAR(100)  NULL,
        Remarks         NVARCHAR(500)  NULL,
        CONSTRAINT CK_Payment_Amount CHECK (AmountPaid > 0),
        INDEX IX_Payments_BillId (BillId)
    );
END
GO

-- 6. Expenses Table
IF OBJECT_ID('dbo.Expenses', 'U') IS NULL
BEGIN
    CREATE TABLE Expenses (
        ExpenseId       INT IDENTITY(1,1) PRIMARY KEY,
        Category        NVARCHAR(100)  NOT NULL,
        Amount          DECIMAL(10,2)  NOT NULL,
        Description     NVARCHAR(500)  NULL,
        ExpenseDate     DATE           NOT NULL,
        CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Stored Procedure: sp_GetPendingDues
CREATE OR ALTER PROCEDURE sp_GetPendingDues
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.TenantId,
        t.FirstName + ' ' + t.LastName AS TenantName,
        r.RoomNumber,
        COUNT(b.BillId) AS MonthsOverdue,
        SUM(b.TotalAmount - ISNULL(p.PaidTotal, 0)) AS TotalDue,
        MAX(p.LastPaymentDate) AS LastPaymentDate
    FROM Bills b
    INNER JOIN RoomAllocations ra ON b.AllocationId = ra.AllocationId
    INNER JOIN Tenants t ON ra.TenantId = t.TenantId
    INNER JOIN Rooms r ON ra.RoomId = r.RoomId
    OUTER APPLY (
        SELECT 
            SUM(AmountPaid) AS PaidTotal,
            MAX(PaymentDate) AS LastPaymentDate
        FROM Payments
        WHERE BillId = b.BillId
    ) p
    WHERE (b.TotalAmount - ISNULL(p.PaidTotal, 0)) > 0
    GROUP BY t.TenantId, t.FirstName, t.LastName, r.RoomNumber
    ORDER BY TotalDue DESC;
END
GO

-- SEED DATA
-- Seed Rooms
IF NOT EXISTS (SELECT 1 FROM Rooms)
BEGIN
    INSERT INTO Rooms (RoomNumber, Floor, RoomType, Capacity, MonthlyRent, Amenities, Status)
    VALUES
    ('101', 1, 1, 1, 8000.00, 'AC, Attached Bath, WiFi, Balcony', 2), -- Single (Occupied)
    ('102', 1, 2, 2, 5500.00, 'Attached Bath, WiFi, Study Table', 2), -- Double (Occupied)
    ('103', 1, 2, 2, 5500.00, 'Attached Bath, WiFi', 1),             -- Double (Available)
    ('104', 1, 3, 3, 4200.00, 'Common Bath, WiFi', 1),               -- Triple (Available)
    ('201', 2, 1, 1, 8500.00, 'AC, Attached Bath, Balcony, TV', 2),  -- Single (Occupied)
    ('202', 2, 2, 2, 5800.00, 'AC, Attached Bath, WiFi', 1),        -- Double (Available)
    ('203', 2, 3, 3, 4500.00, 'Attached Bath, Geyser', 1),          -- Triple (Available)
    ('204', 2, 4, 4, 3500.00, 'Common Bath, WiFi, Locker', 1),      -- Dormitory (Available)
    ('301', 3, 1, 1, 9000.00, 'Penthouse, AC, Bath, Terrace', 3),   -- Maintenance
    ('302', 3, 2, 2, 6000.00, 'AC, Attached Bath, Sea View', 1);    -- Double (Available)
END
GO

-- Seed Tenants
IF NOT EXISTS (SELECT 1 FROM Tenants)
BEGIN
    INSERT INTO Tenants (FirstName, LastName, Email, Phone, IdProofType, IdProofNumber, EmergencyContact)
    VALUES
    ('Rahul', 'Sharma', 'rahul.sharma@example.com', '+919876543210', 'Aadhar', '1234-5678-9012', '+919876543219'),
    ('Priya', 'Verma', 'priya.verma@example.com', '+919876543211', 'Passport', 'K1234567', '+919876543220'),
    ('Amit', 'Patel', 'amit.patel@example.com', '+919876543212', 'DL', 'DL-0420110012345', '+919876543221'),
    ('Sneha', 'Reddy', 'sneha.reddy@example.com', '+919876543213', 'Aadhar', '5678-1234-9012', '+919876543222'),
    ('Vikram', 'Singh', 'vikram.singh@example.com', '+919876543214', 'Aadhar', '9012-1234-5678', '+919876543223');
END
GO

-- Seed Allocations
IF NOT EXISTS (SELECT 1 FROM RoomAllocations)
BEGIN
    INSERT INTO RoomAllocations (RoomId, TenantId, CheckInDate, SecurityDeposit, Status)
    VALUES
    (1, 1, '2026-01-01', 16000.00, 1),
    (2, 2, '2026-01-15', 11000.00, 1),
    (5, 3, '2026-02-01', 17000.00, 1);
END
GO

-- Seed Bills
IF NOT EXISTS (SELECT 1 FROM Bills)
BEGIN
    INSERT INTO Bills (AllocationId, BillMonth, BillYear, RentAmount, ElectricityCharges, WaterCharges, OtherCharges, Discount, DueDate, Notes)
    VALUES
    (1, 10, 2026, 8000.00, 450.00, 150.00, 100.00, 0.00, '2026-11-05', 'October 2026 Rent + Utilities'),
    (2, 10, 2026, 5500.00, 300.00, 150.00, 50.00, 0.00, '2026-11-05', 'October 2026 Rent');
END
GO

-- Seed Payments
IF NOT EXISTS (SELECT 1 FROM Payments)
BEGIN
    INSERT INTO Payments (BillId, AmountPaid, PaymentDate, PaymentMode, TransactionRef, ReceivedBy, Remarks)
    VALUES
    (1, 8700.00, '2026-11-04 14:30:00', 2, 'UPI-20261104-984712', 'Admin', 'Paid in full via GPay');
END
GO
