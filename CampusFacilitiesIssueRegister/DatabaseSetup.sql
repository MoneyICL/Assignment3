-- ========================================================
-- Database Setup Script for Campus Facilities Issue Register
-- Course: BBIS 5101 Introduction to Object Oriented Programming
-- Table: FacilitiesIssues
-- ========================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FacilitiesIssues')
BEGIN
    CREATE TABLE FacilitiesIssues (
        IssueId INT IDENTITY(1,1) PRIMARY KEY,
        Location NVARCHAR(100) NOT NULL,
        Category NVARCHAR(50) NOT NULL,
        Description NVARCHAR(255) NOT NULL,
        DateReported DATETIME NOT NULL,
        Priority NVARCHAR(20) NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        ReportedBy NVARCHAR(100) NOT NULL
    );

    -- Insert initial sample records for demonstration and testing
    INSERT INTO FacilitiesIssues (Location, Category, Description, DateReported, Priority, Status, ReportedBy)
    VALUES 
    ('Room 301', 'Lighting', 'Flickering ceiling light near the whiteboard', '2026-11-01', 'Medium', 'Open', 'John Smith'),
    ('Library Level 2', 'Furniture', 'Broken chair leg at study desk 4', '2026-11-02', 'Low', 'In Progress', 'Sarah Lee'),
    ('Lab B14', 'Heating/Cooling', 'Air conditioning unit not cooling properly', '2026-11-03', 'High', 'Open', 'David Brown'),
    ('Staff Room 105', 'Plumbing', 'Leaking kitchen tap in staff kitchenette', '2026-11-04', 'Medium', 'Resolved', 'Emma Wilson');
END
GO

