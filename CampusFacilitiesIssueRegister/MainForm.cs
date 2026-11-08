using System;
using System.Data;
using System.Windows.Forms;

namespace CampusFacilitiesIssueRegister
{
    public partial class MainForm : Form
    {
        // Reference to IssueManager business logic and data access class (no leading underscore)
        private readonly IssueManager issueManager;

        public MainForm()
        {
            InitializeComponent();
            this.issueManager = new IssueManager();
        }

        /// <summary>
        /// Form Load Event: Initializes the database and populates initial data.
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Ensure the database and table exist
                this.issueManager.EnsureDatabaseAndTable();

                // Set default dropdown selections
                cboCategory.SelectedIndex = 0;       // Lighting
                cboPriority.SelectedIndex = 1;       // Medium
                cboStatus.SelectedIndex = 0;         // Open
                cboFilterStatus.SelectedIndex = 0;   // All
                cboFilterCategory.SelectedIndex = 0; // All

                // Load all existing issues into DataGridView
                LoadIssues();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialization Error: " + ex.Message, 
                                "Database Error", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads all issues from the database into the DataGridView.
        /// </summary>
        private void LoadIssues()
        {
            try
            {
                DataTable table = this.issueManager.GetAllIssues();
                dgvIssues.DataSource = table;
                FormatGridHeaders();

                lblTotalRecords.Text = "Total records found: " + table.Rows.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load records: " + ex.Message, 
                                "Error", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Formats column headers and widths for the DataGridView to ensure friendly labels.
        /// </summary>
        private void FormatGridHeaders()
        {
            if (dgvIssues.Columns.Count > 0)
            {
                if (dgvIssues.Columns.Contains("IssueId"))
                {
                    dgvIssues.Columns["IssueId"].HeaderText = "ID";
                    dgvIssues.Columns["IssueId"].Width = 50;
                }
                if (dgvIssues.Columns.Contains("Location"))
                {
                    dgvIssues.Columns["Location"].HeaderText = "Location";
                }
                if (dgvIssues.Columns.Contains("Category"))
                {
                    dgvIssues.Columns["Category"].HeaderText = "Category";
                }
                if (dgvIssues.Columns.Contains("Description"))
                {
                    dgvIssues.Columns["Description"].HeaderText = "Description";
                }
                if (dgvIssues.Columns.Contains("DateReported"))
                {
                    dgvIssues.Columns["DateReported"].HeaderText = "Date Reported";
                }
                if (dgvIssues.Columns.Contains("Priority"))
                {
                    dgvIssues.Columns["Priority"].HeaderText = "Priority";
                }
                if (dgvIssues.Columns.Contains("Status"))
                {
                    dgvIssues.Columns["Status"].HeaderText = "Status";
                }
                if (dgvIssues.Columns.Contains("ReportedBy"))
                {
                    dgvIssues.Columns["ReportedBy"].HeaderText = "Reported By";
                }
            }
        }

        /// <summary>
        /// Handles the Add (Create) button click.
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Construct a new Issue object using values from the UI
                Issue newIssue = new Issue
                {
                    Location = txtLocation.Text.Trim(),
                    Category = cboCategory.SelectedItem != null ? cboCategory.SelectedItem.ToString() : string.Empty,
                    Description = txtDescription.Text.Trim(),
                    DateReported = dtpDateReported.Value,
                    Priority = cboPriority.SelectedItem != null ? cboPriority.SelectedItem.ToString() : string.Empty,
                    Status = cboStatus.SelectedItem != null ? cboStatus.SelectedItem.ToString() : string.Empty,
                    ReportedBy = txtReportedBy.Text.Trim()
                };

                // Add the issue using IssueManager
                string errorMessage;
                bool success = this.issueManager.AddIssue(newIssue, out errorMessage);

                if (success)
                {
                    MessageBox.Show("Facilities issue recorded successfully!", 
                                    "Success", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Information);
                    LoadIssues();
                    ClearInputs();
                }
                else
                {
                    MessageBox.Show(errorMessage, 
                                    "Validation Notice", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, 
                                "System Error", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Update (Edit) button click.
        /// </summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if an issue ID is present in the txtIssueId box
                if (string.IsNullOrWhiteSpace(txtIssueId.Text) || !int.TryParse(txtIssueId.Text, out int issueId))
                {
                    MessageBox.Show("Please select an issue from the list to update.", 
                                    "Selection Required", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Warning);
                    return;
                }

                // Construct Issue object with existing ID
                Issue updatedIssue = new Issue
                {
                    IssueId = issueId,
                    Location = txtLocation.Text.Trim(),
                    Category = cboCategory.SelectedItem != null ? cboCategory.SelectedItem.ToString() : string.Empty,
                    Description = txtDescription.Text.Trim(),
                    DateReported = dtpDateReported.Value,
                    Priority = cboPriority.SelectedItem != null ? cboPriority.SelectedItem.ToString() : string.Empty,
                    Status = cboStatus.SelectedItem != null ? cboStatus.SelectedItem.ToString() : string.Empty,
                    ReportedBy = txtReportedBy.Text.Trim()
                };

                // Update via IssueManager
                string errorMessage;
                bool success = this.issueManager.UpdateIssue(updatedIssue, out errorMessage);

                if (success)
                {
                    MessageBox.Show("Facilities issue updated successfully!", 
                                    "Success", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Information);
                    LoadIssues();
                    ClearInputs();
                }
                else
                {
                    MessageBox.Show(errorMessage, 
                                    "Update Warning", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating issue: " + ex.Message, 
                                "System Error", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Delete button click with user confirmation.
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Verify selection
                if (string.IsNullOrWhiteSpace(txtIssueId.Text) || !int.TryParse(txtIssueId.Text, out int issueId))
                {
                    MessageBox.Show("Please select an issue from the list to delete.", 
                                    "Selection Required", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion with the user
                DialogResult dialogResult = MessageBox.Show(
                    "Are you sure you want to permanently delete Issue #" + issueId + "?", 
                    "Confirm Deletion", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    string errorMessage;
                    bool success = this.issueManager.DeleteIssue(issueId, out errorMessage);

                    if (success)
                    {
                        MessageBox.Show("Issue #" + issueId + " has been deleted.", 
                                        "Deleted", 
                                        MessageBoxButtons.OK, 
                                        MessageBoxIcon.Information);
                        LoadIssues();
                        ClearInputs();
                    }
                    else
                    {
                        MessageBox.Show(errorMessage, 
                                        "Delete Failed", 
                                        MessageBoxButtons.OK, 
                                        MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting issue: " + ex.Message, 
                                "System Error", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Search / Filter button click.
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string statusFilter = cboFilterStatus.SelectedItem != null ? cboFilterStatus.SelectedItem.ToString() : "All";
                string categoryFilter = cboFilterCategory.SelectedItem != null ? cboFilterCategory.SelectedItem.ToString() : "All";

                DataTable filteredTable = this.issueManager.SearchAndFilterIssues(keyword, statusFilter, categoryFilter);
                dgvIssues.DataSource = filteredTable;
                FormatGridHeaders();
                lblTotalRecords.Text = "Total records found: " + filteredTable.Rows.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search Error: " + ex.Message, 
                                "Error", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Resets the search filters and reloads all records.
        /// </summary>
        private void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cboFilterStatus.SelectedIndex = 0;   // "All"
            cboFilterCategory.SelectedIndex = 0; // "All"
            LoadIssues();
        }

        /// <summary>
        /// Clears all input fields on the form.
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        /// <summary>
        /// Populates input fields when a user clicks on any row in the DataGridView.
        /// </summary>
        private void dgvIssues_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvIssues.Rows.Count)
            {
                DataGridViewRow row = dgvIssues.Rows[e.RowIndex];

                txtIssueId.Text = row.Cells["IssueId"].Value != null ? row.Cells["IssueId"].Value.ToString() : string.Empty;
                txtLocation.Text = row.Cells["Location"].Value != null ? row.Cells["Location"].Value.ToString() : string.Empty;
                
                string category = row.Cells["Category"].Value != null ? row.Cells["Category"].Value.ToString() : string.Empty;
                cboCategory.SelectedItem = category;

                txtDescription.Text = row.Cells["Description"].Value != null ? row.Cells["Description"].Value.ToString() : string.Empty;

                if (DateTime.TryParse(row.Cells["DateReported"].Value != null ? row.Cells["DateReported"].Value.ToString() : string.Empty, out DateTime dt))
                {
                    dtpDateReported.Value = dt;
                }
                else
                {
                    dtpDateReported.Value = DateTime.Today;
                }

                string priority = row.Cells["Priority"].Value != null ? row.Cells["Priority"].Value.ToString() : string.Empty;
                cboPriority.SelectedItem = priority;

                string status = row.Cells["Status"].Value != null ? row.Cells["Status"].Value.ToString() : string.Empty;
                cboStatus.SelectedItem = status;

                txtReportedBy.Text = row.Cells["ReportedBy"].Value != null ? row.Cells["ReportedBy"].Value.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Helper to reset input fields to initial blank state.
        /// </summary>
        private void ClearInputs()
        {
            txtIssueId.Clear();
            txtLocation.Clear();
            cboCategory.SelectedIndex = 0;
            txtDescription.Clear();
            dtpDateReported.Value = DateTime.Today;
            cboPriority.SelectedIndex = 1; // Medium
            cboStatus.SelectedIndex = 0;   // Open
            txtReportedBy.Clear();
            txtLocation.Focus();
        }
    }
}
