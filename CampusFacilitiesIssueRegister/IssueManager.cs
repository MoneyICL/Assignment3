using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace CampusFacilitiesIssueRegister
{
    /// <summary>
    /// Manages business operations and database interactions for facilities issues.
    /// Interacts directly with the Issue entity class using standard ADO.NET.
    /// </summary>
    public class IssueManager
    {
        // Private field for database connection string (no leading underscore)
        private string connectionString;

        // Constructor: initializes connection string from App.config or fallback
        public IssueManager()
        {
            try
            {
                if (ConfigurationManager.ConnectionStrings["CampusFacilitiesDB"] != null)
                {
                    this.connectionString = ConfigurationManager.ConnectionStrings["CampusFacilitiesDB"].ConnectionString;
                }
                else
                {
                    this.connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\App_Data\CampusFacilitiesDB.mdf;Integrated Security=True;Connect Timeout=30";
                }
            }
            catch
            {
                this.connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\App_Data\CampusFacilitiesDB.mdf;Integrated Security=True;Connect Timeout=30";
            }
        }

        // Custom constructor if specific connection string is passed
        public IssueManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Public property to read connection string
        public string ConnectionString
        {
            get { return this.connectionString; }
            set { this.connectionString = value; }
        }

        /// <summary>
        /// Ensures the LocalDB .mdf file and FacilitiesIssues table exist,
        /// and seeds initial sample records if the table is empty.
        /// </summary>
        public void EnsureDatabaseAndTable()
        {
            try
            {
                // Ensure |DataDirectory| is resolved
                string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(dataDirectory))
                {
                    dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
                }

                // If connection string uses AttachDbFilename, ensure the physical .mdf file exists
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(this.connectionString);
                string attachFile = builder.AttachDBFilename;

                if (!string.IsNullOrEmpty(attachFile))
                {
                    string resolvedMdfPath = attachFile.Replace("|DataDirectory|", dataDirectory.TrimEnd('\\'));
                    string folder = Path.GetDirectoryName(resolvedMdfPath);

                    if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    if (!File.Exists(resolvedMdfPath))
                    {
                        // Create the physical .mdf database file via LocalDB master
                        string masterConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30";
                        using (SqlConnection masterConn = new SqlConnection(masterConnection))
                        {
                            masterConn.Open();
                            string dbName = "CampusFacilitiesDB";

                            // If database name is already registered, detach it first
                            string checkSql = "IF EXISTS (SELECT name FROM sys.databases WHERE name = @db) EXEC sp_detach_db @db";
                            using (SqlCommand checkCmd = new SqlCommand(checkSql, masterConn))
                            {
                                checkCmd.Parameters.AddWithValue("@db", dbName);
                                checkCmd.ExecuteNonQuery();
                            }

                            // Create the .mdf database at the target path
                            string createDbSql = string.Format("CREATE DATABASE [{0}] ON (NAME = N'{0}', FILENAME = N'{1}')", dbName, resolvedMdfPath);
                            using (SqlCommand createCmd = new SqlCommand(createDbSql, masterConn))
                            {
                                createCmd.ExecuteNonQuery();
                            }

                            // Detach from master so AttachDbFilename in connection string can attach cleanly
                            using (SqlCommand detachCmd = new SqlCommand("EXEC sp_detach_db '" + dbName + "'", masterConn))
                            {
                                detachCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch
            {
                // If master creation fails (e.g., file already exists or attach mode is active), proceed to table creation
            }

            try
            {
                // Connect to the database and create the FacilitiesIssues table if it does not exist
                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    conn.Open();
                    string createTableSql = @"
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

                            -- Seed initial sample records for testing
                            INSERT INTO FacilitiesIssues (Location, Category, Description, DateReported, Priority, Status, ReportedBy)
                            VALUES 
                            ('Room 301, Building A', 'Lighting', 'Fluorescent light tube flickering continuously.', DATEADD(day, -2, GETDATE()), 'Medium', 'Open', 'Sarah Jenkins'),
                            ('Computer Lab 2', 'Heating/Cooling', 'Air conditioning unit not cooling properly.', DATEADD(day, -4, GETDATE()), 'High', 'In Progress', 'David Chen'),
                            ('Library 1st Floor', 'Furniture', 'Broken chair wheel near study desk 12.', DATEADD(day, -1, GETDATE()), 'Low', 'Open', 'Emily Taylor'),
                            ('Staff Room B4', 'Plumbing', 'Water tap in kitchenette is leaking constantly.', DATEADD(day, -6, GETDATE()), 'Medium', 'Resolved', 'Michael Brown');
                        END";

                    using (SqlCommand cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Automatically generate database backup (.bak) in the Database folder
                CreateDatabaseBackup();
            }
            catch (Exception ex)
            {
                throw new Exception("Database Initialization Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a SQL Server .bak backup file containing tables and records,
        /// and stores it in the Database folder of the project.
        /// </summary>
        public void CreateDatabaseBackup()
        {
            try
            {
                string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(dataDirectory))
                {
                    dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }

                // Locate target Database folders: project folder, solution folder, and runtime folder
                string[] candidateFolders = new string[]
                {
                    Path.GetFullPath(Path.Combine(dataDirectory, @"..\..\Database")),
                    Path.GetFullPath(Path.Combine(dataDirectory, @"..\..\..\Database")),
                    Path.Combine(dataDirectory, "Database")
                };

                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    conn.Open();

                    foreach (string folder in candidateFolders)
                    {
                        try
                        {
                            if (!Directory.Exists(folder))
                            {
                                Directory.CreateDirectory(folder);
                            }

                            string bakFilePath = Path.Combine(folder, "CampusFacilitiesDB.bak");
                            string backupSql = @"
                                DECLARE @currentDb NVARCHAR(256) = DB_NAME();
                                DECLARE @sql NVARCHAR(MAX) = N'BACKUP DATABASE [' + @currentDb + N'] TO DISK = @path WITH FORMAT, INIT, NAME = N''CampusFacilitiesDB-Full Backup'';';
                                EXEC sp_executesql @sql, N'@path NVARCHAR(MAX)', @path = @BakPath;";

                            using (SqlCommand backupCmd = new SqlCommand(backupSql, conn))
                            {
                                backupCmd.Parameters.AddWithValue("@BakPath", bakFilePath);
                                backupCmd.ExecuteNonQuery();
                            }
                        }
                        catch
                        {
                            // Continue to other candidate folders if one path fails
                        }
                    }
                }
            }
            catch
            {
                // Backup execution is non-fatal for UI launch
            }
        }

        /// <summary>
        /// Retrieves all facility issues from the database (READ / VIEW).
        /// </summary>
        public DataTable GetAllIssues()
        {
            DataTable table = new DataTable();
            string query = "SELECT IssueId, Location, Category, Description, DateReported, Priority, Status, ReportedBy FROM FacilitiesIssues ORDER BY IssueId DESC";

            using (SqlConnection conn = new SqlConnection(this.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Inserts a new issue into the database (CREATE / ADD).
        /// Validates the Issue entity before executing SQL.
        /// </summary>
        public bool AddIssue(Issue issue, out string errorMessage)
        {
            // Validate issue object first using its own business logic method
            if (!issue.Validate(out errorMessage))
            {
                return false;
            }

            string query = @"INSERT INTO FacilitiesIssues 
                             (Location, Category, Description, DateReported, Priority, Status, ReportedBy) 
                             VALUES 
                             (@Location, @Category, @Description, @DateReported, @Priority, @Status, @ReportedBy)";

            try
            {
                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Location", issue.Location);
                        cmd.Parameters.AddWithValue("@Category", issue.Category);
                        cmd.Parameters.AddWithValue("@Description", issue.Description);
                        cmd.Parameters.AddWithValue("@DateReported", issue.DateReported);
                        cmd.Parameters.AddWithValue("@Priority", issue.Priority);
                        cmd.Parameters.AddWithValue("@Status", issue.Status);
                        cmd.Parameters.AddWithValue("@ReportedBy", issue.ReportedBy);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            errorMessage = string.Empty;
                            return true;
                        }
                        else
                        {
                            errorMessage = "Failed to insert issue into database.";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Database Error: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Updates an existing issue in the database (UPDATE / EDIT).
        /// Validates the Issue entity before executing SQL.
        /// </summary>
        public bool UpdateIssue(Issue issue, out string errorMessage)
        {
            if (issue.IssueId <= 0)
            {
                errorMessage = "Please select a valid issue record to update.";
                return false;
            }

            // Validate issue data
            if (!issue.Validate(out errorMessage))
            {
                return false;
            }

            string query = @"UPDATE FacilitiesIssues 
                             SET Location = @Location,
                                 Category = @Category,
                                 Description = @Description,
                                 DateReported = @DateReported,
                                 Priority = @Priority,
                                 Status = @Status,
                                 ReportedBy = @ReportedBy
                             WHERE IssueId = @IssueId";

            try
            {
                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IssueId", issue.IssueId);
                        cmd.Parameters.AddWithValue("@Location", issue.Location);
                        cmd.Parameters.AddWithValue("@Category", issue.Category);
                        cmd.Parameters.AddWithValue("@Description", issue.Description);
                        cmd.Parameters.AddWithValue("@DateReported", issue.DateReported);
                        cmd.Parameters.AddWithValue("@Priority", issue.Priority);
                        cmd.Parameters.AddWithValue("@Status", issue.Status);
                        cmd.Parameters.AddWithValue("@ReportedBy", issue.ReportedBy);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            errorMessage = string.Empty;
                            return true;
                        }
                        else
                        {
                            errorMessage = "Record not found or no changes were made.";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Database Error: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Deletes an existing issue by its ID (DELETE).
        /// </summary>
        public bool DeleteIssue(int issueId, out string errorMessage)
        {
            if (issueId <= 0)
            {
                errorMessage = "Please select a valid issue record to delete.";
                return false;
            }

            string query = "DELETE FROM FacilitiesIssues WHERE IssueId = @IssueId";

            try
            {
                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IssueId", issueId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            errorMessage = string.Empty;
                            return true;
                        }
                        else
                        {
                            errorMessage = "Record not found or could not be deleted.";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Database Error: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Searches and filters issues by keyword, status, and category (SEARCH / FILTER).
        /// </summary>
        public DataTable SearchAndFilterIssues(string searchKeyword, string statusFilter, string categoryFilter)
        {
            DataTable table = new DataTable();
            string query = @"SELECT IssueId, Location, Category, Description, DateReported, Priority, Status, ReportedBy 
                             FROM FacilitiesIssues 
                             WHERE 1=1 ";

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                query += " AND (Location LIKE @Keyword OR Description LIKE @Keyword OR ReportedBy LIKE @Keyword) ";
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query += " AND Status = @Status ";
            }

            if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "All")
            {
                query += " AND Category = @Category ";
            }

            query += " ORDER BY IssueId DESC";

            using (SqlConnection conn = new SqlConnection(this.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                    {
                        cmd.Parameters.AddWithValue("@Keyword", "%" + searchKeyword.Trim() + "%");
                    }
                    if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                    {
                        cmd.Parameters.AddWithValue("@Status", statusFilter);
                    }
                    if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "All")
                    {
                        cmd.Parameters.AddWithValue("@Category", categoryFilter);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }
    }
}
