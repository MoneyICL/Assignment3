-- ========================================================
-- SQL Server Database Backup Script
-- Database: CampusFacilitiesDB
-- Target Backup: CampusFacilitiesDB.bak
-- ========================================================

-- Create database backup containing FacilitiesIssues table and data
BACKUP DATABASE [CampusFacilitiesDB] 
TO DISK = N'CampusFacilitiesDB.bak' 
WITH FORMAT, 
     MEDIANAME = 'CampusFacilitiesBackup', 
     NAME = 'Full Backup of CampusFacilitiesDB',
     INIT;
GO

