using System.Windows.Forms.DataVisualization.Charting;

namespace HostControlProgram
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // 공통
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabDashboard, tabChart, tabAlarm, tabDB;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.GroupBox grpLog;

        // 탭1 Dashboard
        private System.Windows.Forms.Panel panelEtch, panelCvd, panelDiff;

        // 탭2 Chart
        private System.Windows.Forms.ComboBox cmbChartEquipment;
        private Chart chartMain;

        // 탭3 Alarm
        private System.Windows.Forms.DataGridView dgvAlarm;
        private System.Windows.Forms.Button btnRefreshAlarm;
        private System.Windows.Forms.Label lblAlarmCount;

        // 탭4 DB Viewer
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.ComboBox cmbQueryEquipment, cmbQueryStatus;
        private System.Windows.Forms.DateTimePicker dateFrom, dateTo;
        private System.Windows.Forms.Label lblQueryCount;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // ── Form ──
            this.Text = "Semiconductor Equipment Monitoring System";
            this.Size = new System.Drawing.Size(1100, 780);
            this.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
            this.ForeColor = System.Drawing.Color.White;
            this.Font = new System.Drawing.Font("Consolas", 9f);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // ── TabControl ──
            tabMain = new System.Windows.Forms.TabControl
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(1065, 580),
                Font = new System.Drawing.Font("Consolas", 9f)
            };

            tabDashboard = new System.Windows.Forms.TabPage("Dashboard");
            tabChart = new System.Windows.Forms.TabPage("Chart");
            tabAlarm = new System.Windows.Forms.TabPage("Alarm History");
            tabDB = new System.Windows.Forms.TabPage("DB Viewer");

            tabMain.TabPages.AddRange(new[] {
                tabDashboard, tabChart, tabAlarm, tabDB });

            // ── 탭1: Dashboard ──
            BuildDashboardTab();

            // ── 탭2: Chart ──
            BuildChartTab();

            // ── 탭3: Alarm ──
            BuildAlarmTab();

            // ── 탭4: DB ──
            BuildDbTab();

            // ── Log ──
            grpLog = new System.Windows.Forms.GroupBox
            {
                Text = "System Log",
                Location = new System.Drawing.Point(10, 598),
                Size = new System.Drawing.Size(1065, 130),
                ForeColor = System.Drawing.Color.Cyan
            };
            listBoxLog = new System.Windows.Forms.ListBox
            {
                Location = new System.Drawing.Point(8, 18),
                Size = new System.Drawing.Size(1045, 104),
                BackColor = System.Drawing.Color.FromArgb(15, 15, 15),
                ForeColor = System.Drawing.Color.LightGray,
                Font = new System.Drawing.Font("Consolas", 8.5f),
                BorderStyle = System.Windows.Forms.BorderStyle.None
            };
            grpLog.Controls.Add(listBoxLog);

            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                tabMain, grpLog });

            this.ResumeLayout(false);
        }

        // ── Dashboard 탭 ──
        private void BuildDashboardTab()
        {
            tabDashboard.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);

            panelEtch = BuildEquipmentPanel("ETCH_01 — Etcher", 10, 10);
            panelCvd = BuildEquipmentPanel("CVD_01  — CVD", 360, 10);
            panelDiff = BuildEquipmentPanel("DIFF_01 — Diffusion", 710, 10);

            tabDashboard.Controls.AddRange(new[] { panelEtch, panelCvd, panelDiff });
        }

        private System.Windows.Forms.Panel BuildEquipmentPanel(string title, int x, int y)
        {
            var panel = new System.Windows.Forms.Panel
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(330, 500),
                BackColor = System.Drawing.Color.FromArgb(35, 35, 35),
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            };

            // 타이틀
            panel.Controls.Add(new System.Windows.Forms.Label
            {
                Text = title,
                Font = new System.Drawing.Font("Consolas", 10f,
                                System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.Cyan,
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(300, 24)
            });

            int row = 45;
            AddPanelRow(panel, "Status", "lblStatus", ref row, System.Drawing.Color.LimeGreen);
            AddPanelRow(panel, "Temp", "lblTemp", ref row, System.Drawing.Color.White);
            AddPanelRow(panel, "Pressure", "lblPress", ref row, System.Drawing.Color.White);
            AddPanelRow(panel, "", "lblExtra1", ref row, System.Drawing.Color.LightBlue);
            AddPanelRow(panel, "", "lblExtra2", ref row, System.Drawing.Color.LightBlue);

            // 구분선
            row += 10;
            panel.Controls.Add(new System.Windows.Forms.Label
            {
                Text = new string('─', 38),
                ForeColor = System.Drawing.Color.DimGray,
                Location = new System.Drawing.Point(10, row),
                Size = new System.Drawing.Size(300, 18)
            });
            row += 22;

            // 알람 라벨
            var lblAlarm = new System.Windows.Forms.Label
            {
                Name = "lblAlarm",
                Text = "✅ NORMAL",
                ForeColor = System.Drawing.Color.LimeGreen,
                Font = new System.Drawing.Font("Consolas", 9f,
                                System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(10, row),
                Size = new System.Drawing.Size(305, 50),
                AutoSize = false
            };
            panel.Controls.Add(lblAlarm);

            return panel;
        }

        private void AddPanelRow(System.Windows.Forms.Panel panel,
            string labelText, string valueName, ref int y,
            System.Drawing.Color valueColor)
        {
            panel.Controls.Add(new System.Windows.Forms.Label
            {
                Text = labelText,
                ForeColor = System.Drawing.Color.Gray,
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(100, 22)
            });
            panel.Controls.Add(new System.Windows.Forms.Label
            {
                Name = valueName,
                Text = "---",
                ForeColor = valueColor,
                Font = new System.Drawing.Font("Consolas", 10f,
                                System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(115, y),
                Size = new System.Drawing.Size(200, 22)
            });
            y += 30;
        }

        // ── Chart 탭 ──
        private void BuildChartTab()
        {
            tabChart.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);

            cmbChartEquipment = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(200, 24),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                ForeColor = System.Drawing.Color.White
            };
            cmbChartEquipment.Items.AddRange(
                new object[] { "ETCH_01", "CVD_01", "DIFF_01" });
            cmbChartEquipment.SelectedIndex = 0;

            chartMain = new Chart
            {
                Location = new System.Drawing.Point(10, 45),
                Size = new System.Drawing.Size(1030, 480),
                BackColor = System.Drawing.Color.FromArgb(25, 25, 25)
            };

            // ChartArea
            var ca = new ChartArea("Main");
            ca.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            ca.AxisX.LabelStyle.ForeColor = System.Drawing.Color.Gray;
            ca.AxisY.LabelStyle.ForeColor = System.Drawing.Color.Gray;
            ca.AxisX.LineColor = System.Drawing.Color.DimGray;
            ca.AxisY.LineColor = System.Drawing.Color.DimGray;
            ca.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(50, 50, 50);
            ca.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(50, 50, 50);
            ca.AxisY.Title = "Temperature / Pressure";
            ca.AxisY.TitleForeColor = System.Drawing.Color.Gray;
            chartMain.ChartAreas.Add(ca);

            // Series
            var sTemp = new Series("Temperature")
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.OrangeRed,
                BorderWidth = 2
            };
            var sPress = new Series("Pressure")
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.DeepSkyBlue,
                BorderWidth = 2
            };
            chartMain.Series.Add(sTemp);
            chartMain.Series.Add(sPress);

            // Legend
            var legend = new Legend
            {
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.White
            };
            chartMain.Legends.Add(legend);

            tabChart.Controls.AddRange(new System.Windows.Forms.Control[] {
                cmbChartEquipment, chartMain });
        }

        // ── Alarm History 탭 ──
        private void BuildAlarmTab()
        {
            tabAlarm.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);

            btnRefreshAlarm = new System.Windows.Forms.Button
            {
                Text = "새로고침",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(100, 28),
                BackColor = System.Drawing.Color.FromArgb(50, 50, 50),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat
            };
            btnRefreshAlarm.Click += BtnRefreshAlarm_Click;

            lblAlarmCount = new System.Windows.Forms.Label
            {
                Text = "총 알람: 0건",
                ForeColor = System.Drawing.Color.Yellow,
                Location = new System.Drawing.Point(120, 14),
                Size = new System.Drawing.Size(700, 22)
            };

            dgvAlarm = BuildDgv(10, 48, 1030, 470);
            dgvAlarm.Columns.Add("timestamp", "시간");
            dgvAlarm.Columns.Add("equipment_id", "장비");
            dgvAlarm.Columns.Add("level", "레벨");
            dgvAlarm.Columns.Add("msg", "메시지");
            dgvAlarm.Columns.Add("status", "상태");
            SetDgvColWidths(dgvAlarm, 160, 100, 90, 480, 90);

            tabAlarm.Controls.AddRange(new System.Windows.Forms.Control[] {
                btnRefreshAlarm, lblAlarmCount, dgvAlarm });
        }

        // ── DB Viewer 탭 ──
        private void BuildDbTab()
        {
            tabDB.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);

            cmbQueryEquipment = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(10, 12),
                Size = new System.Drawing.Size(130, 24),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                ForeColor = System.Drawing.Color.White
            };
            cmbQueryEquipment.Items.AddRange(
                new object[] { "전체", "ETCH_01", "CVD_01", "DIFF_01" });
            cmbQueryEquipment.SelectedIndex = 0;

            cmbQueryStatus = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(150, 12),
                Size = new System.Drawing.Size(110, 24),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                ForeColor = System.Drawing.Color.White
            };
            cmbQueryStatus.Items.AddRange(
                new object[] { "전체", "RUN", "IDLE", "ALARM", "PAUSE", "READY" });
            cmbQueryStatus.SelectedIndex = 0;

            dateFrom = new System.Windows.Forms.DateTimePicker
            {
                Location = new System.Drawing.Point(270, 12),
                Size = new System.Drawing.Size(160, 24),
                Format = System.Windows.Forms.DateTimePickerFormat.Short,
                Value = System.DateTime.Today
            };
            dateTo = new System.Windows.Forms.DateTimePicker
            {
                Location = new System.Drawing.Point(440, 12),
                Size = new System.Drawing.Size(160, 24),
                Format = System.Windows.Forms.DateTimePickerFormat.Short,
                Value = System.DateTime.Today
            };

            btnQuery = new System.Windows.Forms.Button
            {
                Text = "조회",
                Location = new System.Drawing.Point(610, 10),
                Size = new System.Drawing.Size(80, 28),
                BackColor = System.Drawing.Color.FromArgb(0, 80, 160),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat
            };
            btnQuery.Click += BtnQuery_Click;

            var btnExport = new System.Windows.Forms.Button
            {
                Text = "CSV 내보내기",
                Location = new System.Drawing.Point(700, 10),
                Size = new System.Drawing.Size(120, 28),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 60),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat
            };
            btnExport.Click += BtnExport_Click;

            lblQueryCount = new System.Windows.Forms.Label
            {
                Text = "조회 결과: 0건",
                ForeColor = System.Drawing.Color.Cyan,
                Location = new System.Drawing.Point(830, 14),
                Size = new System.Drawing.Size(300, 22)
            };

            dgvData = BuildDgv(10, 48, 1030, 470);
            dgvData.Columns.Add("timestamp", "시간");
            dgvData.Columns.Add("equipment_id", "장비ID");
            dgvData.Columns.Add("type", "타입");
            dgvData.Columns.Add("temp", "온도(°C)");
            dgvData.Columns.Add("pressure", "압력");
            dgvData.Columns.Add("status", "상태");
            dgvData.Columns.Add("alarm_level", "알람레벨");
            dgvData.Columns.Add("alarm_msg", "알람메시지");
            SetDgvColWidths(dgvData, 155, 90, 70, 90, 90, 80, 80, 350);

            tabDB.Controls.AddRange(new System.Windows.Forms.Control[] {
                cmbQueryEquipment, cmbQueryStatus,
                dateFrom, dateTo,
                btnQuery, btnExport,
                btnQuery, lblQueryCount, dgvData });
        }

        // ── 공통 DataGridView 빌더 ──
        private System.Windows.Forms.DataGridView BuildDgv(
            int x, int y, int w, int h)
        {
            var dgv = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(w, h),
                BackgroundColor = System.Drawing.Color.FromArgb(20, 20, 20),
                GridColor = System.Drawing.Color.FromArgb(50, 50, 50),
                ForeColor = System.Drawing.Color.White,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode =
                    System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode =
                    System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor =
                System.Drawing.Color.FromArgb(40, 40, 40);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor =
                System.Drawing.Color.Cyan;
            dgv.DefaultCellStyle.BackColor =
                System.Drawing.Color.FromArgb(20, 20, 20);
            dgv.DefaultCellStyle.ForeColor =
                System.Drawing.Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor =
                System.Drawing.Color.FromArgb(30, 30, 30);
            return dgv;
        }

        private void SetDgvColWidths(
            System.Windows.Forms.DataGridView dgv, params int[] widths)
        {
            for (int i = 0; i < widths.Length && i < dgv.Columns.Count; i++)
                dgv.Columns[i].Width = widths[i];
        }
    }
}