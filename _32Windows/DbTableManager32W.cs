using System;
using System.Windows.Forms;
using System.Drawing;
using FDCommon.LibBase.LibBase13LB;
using FDCommon.LibBase.LibBase14LB;
using Krypton.Toolkit;

namespace ERD2DB.Windows
{
    public class DbTableManager32W : UserControl
    {
        private readonly Logging14LB _logger;
        private readonly KryptonManager14LB _kryptonManager;
        private KryptonDataGridView tableGrid;
        private KryptonComboBox databaseSelector;
        private KryptonButton addTableButton;
        private KryptonButton removeTableButton;
        private KryptonButton editTableButton;

        public DbTableManager32W()
        {
            _logger = Logging14LB.Instance;
            _kryptonManager = KryptonManager14LB.Instance;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeComponents), nameof(DbTableManager32W));

                // Initialize database selector
                databaseSelector = _kryptonManager.CreateComboBox();
                databaseSelector.Dock = DockStyle.Top;
                databaseSelector.SelectedIndexChanged += DatabaseSelector_SelectedIndexChanged;

                // Initialize table grid
                tableGrid = _kryptonManager.CreateDataGridView();
                tableGrid.Dock = DockStyle.Fill;
                tableGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                tableGrid.MultiSelect = false;
                tableGrid.AllowUserToAddRows = false;
                tableGrid.AllowUserToDeleteRows = false;
                tableGrid.ReadOnly = true;
                tableGrid.SelectionChanged += TableGrid_SelectionChanged;

                // Initialize columns
                tableGrid.Columns.AddRange(new DataGridViewColumn[]
                {
                    new KryptonDataGridViewTextBoxColumn
                    {
                        Name = "TableName",
                        HeaderText = "테이블명",
                        Width = 200
                    },
                    new KryptonDataGridViewTextBoxColumn
                    {
                        Name = "ColumnCount",
                        HeaderText = "컬럼 수",
                        Width = 80
                    },
                    new KryptonDataGridViewTextBoxColumn
                    {
                        Name = "RecordCount",
                        HeaderText = "레코드 수",
                        Width = 100
                    },
                    new KryptonDataGridViewTextBoxColumn
                    {
                        Name = "LastModified",
                        HeaderText = "최종 수정일",
                        Width = 150
                    }
                });

                // Initialize button panel
                var buttonPanel = _kryptonManager.CreatePanel();
                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 40;

                // Initialize buttons
                addTableButton = _kryptonManager.CreateButton();
                removeTableButton = _kryptonManager.CreateButton();
                editTableButton = _kryptonManager.CreateButton();

                addTableButton.Text = "테이블 추가";
                removeTableButton.Text = "테이블 제거";
                editTableButton.Text = "테이블 편집";

                addTableButton.Click += AddTableButton_Click;
                removeTableButton.Click += RemoveTableButton_Click;
                editTableButton.Click += EditTableButton_Click;

                // Layout buttons in panel
                buttonPanel.Controls.Add(addTableButton);
                buttonPanel.Controls.Add(removeTableButton);
                buttonPanel.Controls.Add(editTableButton);

                addTableButton.Location = new Point(10, 5);
                removeTableButton.Location = new Point(120, 5);
                editTableButton.Location = new Point(230, 5);

                // Add controls to form
                this.Controls.Add(tableGrid);
                this.Controls.Add(buttonPanel);
                this.Controls.Add(databaseSelector);

                UpdateButtonStates();
                _logger.LogInformation("Components initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize components", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(InitializeComponents), nameof(DbTableManager32W));
            }
        }

        private void TableGrid_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var hasSelection = tableGrid.SelectedRows.Count > 0;
            removeTableButton.Enabled = hasSelection;
            editTableButton.Enabled = hasSelection;
        }

        private void AddTableButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new TableEditorDialog24W())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        RefreshTableList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to add table", ex);
                MessageBox.Show("테이블 추가 중 오류가 발생했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveTableButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show(
                    "선택한 테이블을 제거하시겠습니까?",
                    "테이블 제거",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    RefreshTableList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to remove table", ex);
                MessageBox.Show("테이블 제거 중 오류가 발생했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditTableButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new TableEditorDialog24W())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        RefreshTableList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to edit table", ex);
                MessageBox.Show("테이블 편집 중 오류가 발생했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DatabaseSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTables(databaseSelector.Text);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to handle database selection change", ex);
                MessageBox.Show("데이터베이스 변경 중 오류가 발생했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTables(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName)) return;

            try
            {
                _logger.LogMethodEntry(nameof(LoadTables), nameof(DbTableManager32W));
                RefreshTableList();
                _logger.LogInformation($"Tables loaded for database: {databaseName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load tables for database: {databaseName}", ex);
                MessageBox.Show("테이블 목록을 불러오는 중 오류가 발생했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _logger.LogMethodExit(nameof(LoadTables), nameof(DbTableManager32W));
            }
        }

        private void RefreshTableList()
        {
            try
            {
                _logger.LogMethodEntry(nameof(RefreshTableList), nameof(DbTableManager32W));
                tableGrid.Rows.Clear();
                UpdateButtonStates();
                _logger.LogInformation("Table list refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to refresh table list", ex);
                MessageBox.Show("테이블 목록 새로고침 중 오류가 발생했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _logger.LogMethodExit(nameof(RefreshTableList), nameof(DbTableManager32W));
            }
        }
    }

    public class TableEditorDialog24W : KryptonForm
    {
        private KryptonTextBox tableNameTextBox;
        private KryptonTextBox schemaTextBox;
        private KryptonDataGridView columnsGrid;
        private readonly KryptonManager14LB _kryptonManager;
        private readonly Logging14LB _logger;

        public string TableName
        {
            get => tableNameTextBox.Text;
            set => tableNameTextBox.Text = value;
        }

        public string Schema
        {
            get => schemaTextBox.Text;
            set => schemaTextBox.Text = value;
        }

        public TableEditorDialog24W()
        {
            _kryptonManager = KryptonManager14LB.Instance;
            _logger = Logging14LB.Instance;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeComponents), nameof(TableEditorDialog24W));

                this.Size = new Size(600, 400);
                this.Text = "테이블 편집";

                var tableNameLabel = _kryptonManager.CreateLabel();
                tableNameLabel.Text = "테이블 이름:";
                tableNameLabel.Location = new Point(10, 10);

                tableNameTextBox = _kryptonManager.CreateTextBox();
                tableNameTextBox.Location = new Point(100, 10);
                tableNameTextBox.Width = 200;

                var schemaLabel = _kryptonManager.CreateLabel();
                schemaLabel.Text = "스키마:";
                schemaLabel.Location = new Point(10, 40);

                schemaTextBox = _kryptonManager.CreateTextBox();
                schemaTextBox.Location = new Point(100, 40);
                schemaTextBox.Width = 200;

                columnsGrid = _kryptonManager.CreateDataGridView();
                columnsGrid.Location = new Point(10, 70);
                columnsGrid.Size = new Size(565, 250);
                columnsGrid.AllowUserToAddRows = true;
                columnsGrid.AllowUserToDeleteRows = true;

                columnsGrid.Columns.AddRange(new DataGridViewColumn[]
                {
                    new KryptonDataGridViewTextBoxColumn
                    {
                        Name = "ColumnName",
                        HeaderText = "컬럼명",
                        Width = 150
                    },
                    new KryptonDataGridViewComboBoxColumn
                    {
                        Name = "DataType",
                        HeaderText = "데이터 타입",
                        Width = 100,
                        Items = { "int", "varchar", "datetime", "decimal", "bit" }
                    },
                    new KryptonDataGridViewCheckBoxColumn
                    {
                        Name = "IsNullable",
                        HeaderText = "Null 허용",
                        Width = 80
                    },
                    new KryptonDataGridViewCheckBoxColumn
                    {
                        Name = "IsPrimaryKey",
                        HeaderText = "기본키",
                        Width = 80
                    },
                    new KryptonDataGridViewTextBoxColumn
                    {
                        Name = "DefaultValue",
                        HeaderText = "기본값",
                        Width = 100
                    }
                });

                var okButton = _kryptonManager.CreateButton();
                okButton.Text = "확인";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(410, 330);

                var cancelButton = _kryptonManager.CreateButton();
                cancelButton.Text = "취소";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.Location = new Point(500, 330);

                this.Controls.AddRange(new Control[] {
                    tableNameLabel,
                    tableNameTextBox,
                    schemaLabel,
                    schemaTextBox,
                    columnsGrid,
                    okButton,
                    cancelButton
                });

                this.AcceptButton = okButton;
                this.CancelButton = cancelButton;

                _logger.LogInformation("Table editor dialog initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize table editor dialog", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(InitializeComponents), nameof(TableEditorDialog24W));
            }
        }
    }
}