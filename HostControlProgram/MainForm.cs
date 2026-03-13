using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HostControlProgram
{
    public partial class MainForm : Form
    {
        private TcpServer _server;
        private DatabaseManager _db;
        private AlarmManager _alarm;

        // 장비별 최신 데이터 저장
        private Dictionary<string, EquipmentData> _latestData
            = new Dictionary<string, EquipmentData>();

        // 차트용 데이터 버퍼 (장비별 최근 30개)
        private Dictionary<string, Queue<double>> _tempHistory
            = new Dictionary<string, Queue<double>>();
        private Dictionary<string, Queue<double>> _pressHistory
            = new Dictionary<string, Queue<double>>();

        private const int PORT = 9000;
        private const string DB_PATH = "equipment.db";
        private const int MAX_POINTS = 30;

        public MainForm()
        {
            InitializeComponent();
            InitBuffers();
            InitializeSystem();
        }

        private void InitBuffers()
        {
            foreach (var id in new[] { "ETCH_01", "CVD_01", "DIFF_01" })
            {
                _tempHistory[id] = new Queue<double>();
                _pressHistory[id] = new Queue<double>();
            }
        }

        private void InitializeSystem()
        {
            _db = new DatabaseManager(DB_PATH);
            _alarm = new AlarmManager();
            _alarm.OnAlarm += OnAlarmReceived;

            _server = new TcpServer();
            _server.OnClientConnected += ip => AppendLog($"[연결] Equipment 접속: {ip}");
            _server.OnClientDisconnected += () => AppendLog("[연결] Equipment 연결 해제");
            _server.OnDataReceived += ProcessData;
            _server.Start(new int[] { 9001, 9002, 9003 });

            AppendLog($"[시스템] Host 시작 — Port {PORT} 대기 중");
        }

        private void ProcessData(string json)
        {
            var data = DataParser.Parse(json);
            if (data == null) return;

            _db.Insert(data);
            _alarm.Check(data);

            // 히스토리 버퍼 업데이트
            UpdateBuffer(_tempHistory[data.EquipmentId],
                data.EquipmentType == "DIFF" ? data.FurnaceTemp : data.ChamberTemp);
            UpdateBuffer(_pressHistory[data.EquipmentId], data.ChamberPressure);

            _latestData[data.EquipmentId] = data;

            this.Invoke((Action)(() => {
                UpdateDashboard(data);
                UpdateChart();
            }));
        }

        private void UpdateBuffer(Queue<double> q, double val)
        {
            q.Enqueue(val);
            if (q.Count > MAX_POINTS) q.Dequeue();
        }

        // ──────────────────────────────────────────
        //  탭1: Dashboard 업데이트
        // ──────────────────────────────────────────
        private void UpdateDashboard(EquipmentData d)
        {
            Panel panel = null;
            if (d.EquipmentId == "ETCH_01") panel = panelEtch;
            else if (d.EquipmentId == "CVD_01") panel = panelCvd;
            else if (d.EquipmentId == "DIFF_01") panel = panelDiff;
            if (panel == null) return;

            // 패널 안의 라벨들 업데이트
            SetLabel(panel, "lblStatus", d.Status);
            SetLabel(panel, "lblTemp",
                d.EquipmentType == "DIFF"
                    ? $"{d.FurnaceTemp:F1} °C"
                    : $"{d.ChamberTemp:F1} °C");
            SetLabel(panel, "lblPress", $"{d.ChamberPressure:F2}");
            SetLabel(panel, "lblAlarm",
                d.AlarmLevel == 0 ? "✅ NORMAL" : $"⚠ {d.AlarmMsg}");

            // ETCH 전용
            if (d.EquipmentType == "ETCH")
            {
                SetLabel(panel, "lblExtra1", $"RF: {d.RfPower:F0} W");
                SetLabel(panel, "lblExtra2", $"CF4: {d.GasFlowCf4:F1} sccm");
            }
            // CVD 전용
            else if (d.EquipmentType == "CVD")
            {
                SetLabel(panel, "lblExtra1", $"SiH4: {d.GasFlowSih4:F1} sccm");
                SetLabel(panel, "lblExtra2", $"DepRate: {d.DepositionRate:F1} Å/min");
            }
            // DIFF 전용
            else if (d.EquipmentType == "DIFF")
            {
                SetLabel(panel, "lblExtra1", $"O2: {d.GasFlowO2:F1} sccm");
                SetLabel(panel, "lblExtra2", $"Time: {d.ProcessTime} s");
            }

            // 상태 색상
            var lblSt = panel.Controls["lblStatus"] as Label;
            if (lblSt != null)
            {
                switch (d.Status)
                {
                    case "RUN": lblSt.ForeColor = Color.LimeGreen; break;
                    case "ALARM": lblSt.ForeColor = Color.Red; break;
                    case "PAUSE": lblSt.ForeColor = Color.Orange; break;
                    case "IDLE": lblSt.ForeColor = Color.Gray; break;
                    default: lblSt.ForeColor = Color.White; break;
                }
            }

            // 알람 색상
            var lblAl = panel.Controls["lblAlarm"] as Label;
            if (lblAl != null)
            {
                switch (d.AlarmLevel)
                {
                    case 0: lblAl.ForeColor = Color.LimeGreen; break;
                    case 1: lblAl.ForeColor = Color.Yellow; break;
                    case 2: lblAl.ForeColor = Color.Orange; break;
                    case 3: lblAl.ForeColor = Color.Red; break;
                }
            }

            AppendLog($"[{d.EquipmentId}] Status:{d.Status} " +
                      $"T:{(d.EquipmentType == "DIFF" ? d.FurnaceTemp : d.ChamberTemp):F1}°C " +
                      $"P:{d.ChamberPressure:F2} AL:{d.AlarmLevel}");
        }

        private void SetLabel(Panel panel, string name, string text)
        {
            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl.Name == name && ctrl is Label lbl)
                {
                    lbl.Text = text;
                    return;
                }
            }
        }

        // ──────────────────────────────────────────
        //  탭2: Chart 업데이트
        // ──────────────────────────────────────────
        private void UpdateChart()
        {
            string selected = cmbChartEquipment.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;
            if (!_tempHistory.ContainsKey(selected)) return;

            chartMain.Series["Temperature"].Points.Clear();
            chartMain.Series["Pressure"].Points.Clear();

            var temps = _tempHistory[selected].ToArray();
            var press = _pressHistory[selected].ToArray();

            for (int i = 0; i < temps.Length; i++)
                chartMain.Series["Temperature"].Points.AddY(temps[i]);
            for (int i = 0; i < press.Length; i++)
                chartMain.Series["Pressure"].Points.AddY(press[i]);
        }

        // ──────────────────────────────────────────
        //  탭3: Alarm History 새로고침
        // ──────────────────────────────────────────
        private void BtnRefreshAlarm_Click(object sender, EventArgs e)
        {
            var list = _db.QueryAlarmHistory();
            dgvAlarm.Rows.Clear();
            foreach (var d in list)
            {
                string level = d.AlarmLevel == 1 ? "WARNING"
                             : d.AlarmLevel == 2 ? "ALARM"
                             : "CRITICAL";
                int idx = dgvAlarm.Rows.Add(
                    d.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    d.EquipmentId, level, d.AlarmMsg, d.Status);

                // 레벨별 색상
                Color c = d.AlarmLevel == 1 ? Color.FromArgb(60, 60, 0)
                        : d.AlarmLevel == 2 ? Color.FromArgb(80, 30, 0)
                        : Color.FromArgb(100, 0, 0);
                dgvAlarm.Rows[idx].DefaultCellStyle.BackColor = c;
            }
            lblAlarmCount.Text =
                $"총 알람: {list.Count}건  " +
                $"WARNING: {list.FindAll(x => x.AlarmLevel == 1).Count}  " +
                $"ALARM: {list.FindAll(x => x.AlarmLevel == 2).Count}  " +
                $"CRITICAL: {list.FindAll(x => x.AlarmLevel == 3).Count}";
        }

        // ──────────────────────────────────────────
        //  탭4: DB Viewer 조회
        // ──────────────────────────────────────────
        private void BtnQuery_Click(object sender, EventArgs e)
        {
            string eqId = cmbQueryEquipment.SelectedIndex == 0
                            ? null : cmbQueryEquipment.SelectedItem.ToString();
            string status = cmbQueryStatus.SelectedIndex == 0
                            ? null : cmbQueryStatus.SelectedItem.ToString();

            var list = _db.Query(eqId, status,
                dateFrom.Value.Date,
                dateTo.Value.Date.AddDays(1));

            dgvData.Rows.Clear();
            foreach (var d in list)
            {
                dgvData.Rows.Add(
                    d.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    d.EquipmentId,
                    d.EquipmentType,
                    $"{(d.EquipmentType == "DIFF" ? d.FurnaceTemp : d.ChamberTemp):F1}",
                    $"{d.ChamberPressure:F2}",
                    d.Status,
                    d.AlarmLevel,
                    d.AlarmMsg);
            }
            lblQueryCount.Text = $"조회 결과: {list.Count}건";
        }
        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvData.Rows.Count == 0)
            {
                MessageBox.Show("먼저 데이터를 조회해주세요.",
                    "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV 파일 (*.csv)|*.csv",
                FileName = $"equipment_data_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();

                // 헤더
                sb.AppendLine("시간,장비ID,타입,온도(°C),압력,상태,알람레벨,알람메시지");

                // 데이터
                foreach (DataGridViewRow row in dgvData.Rows)
                {
                    sb.AppendLine(string.Join(",",
                        row.Cells["timestamp"].Value,
                        row.Cells["equipment_id"].Value,
                        row.Cells["type"].Value,
                        row.Cells["temp"].Value,
                        row.Cells["pressure"].Value,
                        row.Cells["status"].Value,
                        row.Cells["alarm_level"].Value,
                        $"\"{row.Cells["alarm_msg"].Value}\""));
                }

                System.IO.File.WriteAllText(dialog.FileName, sb.ToString(),
                    System.Text.Encoding.UTF8);

                MessageBox.Show($"CSV 저장 완료!\n{dialog.FileName}",
                    "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ──────────────────────────────────────────
        //  알람 수신
        // ──────────────────────────────────────────
        private void OnAlarmReceived(EquipmentData data)
        {
            if (data.AlarmLevel >= 3)
            {
                this.Invoke((Action)(() => {
                    MessageBox.Show(
                        $"장비: {data.EquipmentId}\n{data.AlarmMsg}",
                        "🚨 CRITICAL ALARM",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }));
            }
        }

        private void AppendLog(string message)
        {
            if (listBoxLog.InvokeRequired)
            {
                listBoxLog.Invoke((Action)(() => AppendLog(message)));
                return;
            }
            listBoxLog.Items.Insert(0, $"{DateTime.Now:HH:mm:ss}  {message}");
            if (listBoxLog.Items.Count > 300)
                listBoxLog.Items.RemoveAt(listBoxLog.Items.Count - 1);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _server?.Stop();
            base.OnFormClosing(e);
        }
    }
}