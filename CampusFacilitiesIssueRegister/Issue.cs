using System;

namespace CampusFacilitiesIssueRegister
{
    /// <summary>
    /// Represents a facilities issue entity.
    /// Demonstrates encapsulation using private fields and public properties.
    /// </summary>
    public class Issue
    {
        // Private fields (encapsulation using standard camelCase without leading underscores)
        private int issueId;
        private string location;
        private string category;
        private string description;
        private DateTime dateReported;
        private string priority;
        private string status;
        private string reportedBy;

        // Default Constructor
        public Issue()
        {
            this.issueId = 0;
            this.location = string.Empty;
            this.category = "General";
            this.description = string.Empty;
            this.dateReported = DateTime.Today;
            this.priority = "Medium";
            this.status = "Open";
            this.reportedBy = string.Empty;
        }

        // Parameterized Constructor
        public Issue(int issueId, string location, string category, string description, 
                     DateTime dateReported, string priority, string status, string reportedBy)
        {
            this.issueId = issueId;
            this.location = location;
            this.category = category;
            this.description = description;
            this.dateReported = dateReported;
            this.priority = priority;
            this.status = status;
            this.reportedBy = reportedBy;
        }

        // Public Properties with Getters and Setters
        public int IssueId
        {
            get { return this.issueId; }
            set { this.issueId = value; }
        }

        public string Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        public string Category
        {
            get { return this.category; }
            set { this.category = value; }
        }

        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        public DateTime DateReported
        {
            get { return this.dateReported; }
            set { this.dateReported = value; }
        }

        public string Priority
        {
            get { return this.priority; }
            set { this.priority = value; }
        }

        public string Status
        {
            get { return this.status; }
            set { this.status = value; }
        }

        public string ReportedBy
        {
            get { return this.reportedBy; }
            set { this.reportedBy = value; }
        }

        /// <summary>
        /// Validates the issue data against business rules before database operations.
        /// Enforces at least 3 business rules required by the assignment.
        /// </summary>
        /// <param name="errorMessage">Output error message if validation fails.</param>
        /// <returns>True if all rules pass, otherwise false.</returns>
        public bool Validate(out string errorMessage)
        {
            // Rule 1: Location must not be empty and cannot exceed 100 characters
            if (string.IsNullOrWhiteSpace(this.location))
            {
                errorMessage = "Location is required. Please specify a classroom, office, or space.";
                return false;
            }
            if (this.location.Trim().Length > 100)
            {
                errorMessage = "Location cannot exceed 100 characters.";
                return false;
            }

            // Rule 2: Description must not be empty and cannot exceed 255 characters
            if (string.IsNullOrWhiteSpace(this.description))
            {
                errorMessage = "Description is required. Please provide a brief explanation of the problem.";
                return false;
            }
            if (this.description.Trim().Length > 255)
            {
                errorMessage = "Description cannot exceed 255 characters.";
                return false;
            }

            // Rule 3: Reported By must not be empty and cannot exceed 100 characters
            if (string.IsNullOrWhiteSpace(this.reportedBy))
            {
                errorMessage = "Reported By is required. Please enter the name of the person reporting the issue.";
                return false;
            }
            if (this.reportedBy.Trim().Length > 100)
            {
                errorMessage = "Reported By cannot exceed 100 characters.";
                return false;
            }

            // Rule 4: Priority must be selected from valid options
            if (string.IsNullOrWhiteSpace(this.priority) || 
                (this.priority != "Low" && this.priority != "Medium" && this.priority != "High"))
            {
                errorMessage = "Priority must be selected as Low, Medium, or High.";
                return false;
            }

            // Rule 5: Status must be selected from valid options
            if (string.IsNullOrWhiteSpace(this.status) || 
                (this.status != "Open" && this.status != "In Progress" && this.status != "Resolved"))
            {
                errorMessage = "Status must be selected as Open, In Progress, or Resolved.";
                return false;
            }

            // Rule 6: Date reported cannot be in the future
            if (this.dateReported.Date > DateTime.Today)
            {
                errorMessage = "Date Reported cannot be in the future. Please select today's date or an earlier date.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Business method: Checks if this issue requires high attention.
        /// </summary>
        public bool IsHighPriority()
        {
            return this.priority == "High";
        }

        /// <summary>
        /// Business method: Checks if the issue has been resolved.
        /// </summary>
        public bool IsResolved()
        {
            return this.status == "Resolved";
        }
    }
}
