using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace HostControlProgram
{
    public class DatabaseManager
    {
        private string _connectionString;
        private readonly object _lock = new object();  // ← 추가

        public DatabaseManager(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            lock (_lock)  // ← 추가
            {
                using (var conn = new SqliteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                    CREATE TABLE IF NOT EXISTS equipment_data (
                        id               INTEGER PRIMARY KEY AUTOINCREMENT,
                        equipment_id     TEXT,
                        equipment_type   TEXT,
                        chamber_temp     REAL,
                        chamber_pressure REAL,
                        rf_power         REAL,
                        gas_flow_cf4     REAL,
                        chuck_temp       REAL,
                        gas_flow_sih4    REAL,
                        deposition_rate  REAL,
                        furnace_temp     REAL,
                        gas_flow_o2      REAL,
                        process_time     INTEGER,
                        status           TEXT,
                        alarm_level      INTEGER,
                        alarm_msg        TEXT,
                        timestamp        DATETIME
                    );
                    CREATE TABLE IF NOT EXISTS alarm_history (
                        id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        equipment_id TEXT,
                        alarm_level  INTEGER,
                        alarm_msg    TEXT,
                        status       TEXT,
                        timestamp    DATETIME
                    );";
                    new SqliteCommand(sql, conn).ExecuteNonQuery();
                }
            }
        }

        public void Insert(EquipmentData d)
        {
            lock (_lock)
            {
                try
                {
                    using (var conn = new SqliteConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                INSERT INTO equipment_data
                    (equipment_id, equipment_type,
                     chamber_temp, chamber_pressure,
                     rf_power, gas_flow_cf4, chuck_temp,
                     gas_flow_sih4, deposition_rate,
                     furnace_temp, gas_flow_o2, process_time,
                     status, alarm_level, alarm_msg, timestamp)
                VALUES
                    (@eid,@etype,@ct,@cp,@rf,@cf4,@chuck,
                     @sih4,@dep,@ft,@o2,@pt,@st,@al,@am,@ts)";

                        var cmd = new SqliteCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@eid", d.EquipmentId);
                        cmd.Parameters.AddWithValue("@etype", d.EquipmentType);
                        cmd.Parameters.AddWithValue("@ct", d.ChamberTemp);
                        cmd.Parameters.AddWithValue("@cp", d.ChamberPressure);
                        cmd.Parameters.AddWithValue("@rf", d.RfPower);
                        cmd.Parameters.AddWithValue("@cf4", d.GasFlowCf4);
                        cmd.Parameters.AddWithValue("@chuck", d.ChuckTemp);
                        cmd.Parameters.AddWithValue("@sih4", d.GasFlowSih4);
                        cmd.Parameters.AddWithValue("@dep", d.DepositionRate);
                        cmd.Parameters.AddWithValue("@ft", d.FurnaceTemp);
                        cmd.Parameters.AddWithValue("@o2", d.GasFlowO2);
                        cmd.Parameters.AddWithValue("@pt", d.ProcessTime);
                        cmd.Parameters.AddWithValue("@st", d.Status);
                        cmd.Parameters.AddWithValue("@al", d.AlarmLevel);
                        cmd.Parameters.AddWithValue("@am", d.AlarmMsg);
                        cmd.Parameters.AddWithValue("@ts", d.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();

                        if (d.AlarmLevel > 0)
                        {
                            string alarmSql = @"
                    INSERT INTO alarm_history
                        (equipment_id, alarm_level, alarm_msg, status, timestamp)
                    VALUES (@eid, @al, @am, @st, @ts)";
                            var ac = new SqliteCommand(alarmSql, conn);
                            ac.Parameters.AddWithValue("@eid", d.EquipmentId);
                            ac.Parameters.AddWithValue("@al", d.AlarmLevel);
                            ac.Parameters.AddWithValue("@am", d.AlarmMsg);
                            ac.Parameters.AddWithValue("@st", d.Status);
                            ac.Parameters.AddWithValue("@ts", d.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                            ac.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText("db_error.log",
                        $"{DateTime.Now}: {ex.Message}\n\n");
                }
            }
        }

        public List<EquipmentData> Query(string equipmentId = null,
                                         string status = null,
                                         DateTime? from = null,
                                         DateTime? to = null)
        {
            lock (_lock)
            {
                var result = new List<EquipmentData>();
                using (var conn = new SqliteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM equipment_data WHERE 1=1";
                    if (!string.IsNullOrEmpty(equipmentId))
                        sql += $" AND equipment_id='{equipmentId}'";
                    if (!string.IsNullOrEmpty(status))
                        sql += $" AND status='{status}'";
                    if (from.HasValue)
                        sql += $" AND timestamp >= '{from.Value:yyyy-MM-dd HH:mm:ss}'";
                    if (to.HasValue)
                        sql += $" AND timestamp <= '{to.Value:yyyy-MM-dd HH:mm:ss}'";
                    sql += " ORDER BY timestamp DESC LIMIT 500";

                    var reader = new SqliteCommand(sql, conn).ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(new EquipmentData
                        {
                            EquipmentId = reader["equipment_id"].ToString(),
                            EquipmentType = reader["equipment_type"].ToString(),
                            ChamberTemp = Convert.ToDouble(reader["chamber_temp"]),
                            ChamberPressure = Convert.ToDouble(reader["chamber_pressure"]),
                            RfPower = Convert.ToDouble(reader["rf_power"]),
                            GasFlowCf4 = Convert.ToDouble(reader["gas_flow_cf4"]),
                            ChuckTemp = Convert.ToDouble(reader["chuck_temp"]),
                            GasFlowSih4 = Convert.ToDouble(reader["gas_flow_sih4"]),
                            DepositionRate = Convert.ToDouble(reader["deposition_rate"]),
                            FurnaceTemp = Convert.ToDouble(reader["furnace_temp"]),
                            GasFlowO2 = Convert.ToDouble(reader["gas_flow_o2"]),
                            ProcessTime = Convert.ToInt32(reader["process_time"]),
                            Status = reader["status"].ToString(),
                            AlarmLevel = Convert.ToInt32(reader["alarm_level"]),
                            AlarmMsg = reader["alarm_msg"].ToString(),
                            Timestamp = Convert.ToDateTime(reader["timestamp"])
                        });
                    }
                }
                return result;
            }
        }

        public List<EquipmentData> QueryAlarmHistory()
        {
            lock (_lock)
            {
                var result = new List<EquipmentData>();
                using (var conn = new SqliteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT * FROM alarm_history 
                                   ORDER BY timestamp DESC LIMIT 200";
                    var reader = new SqliteCommand(sql, conn).ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(new EquipmentData
                        {
                            EquipmentId = reader["equipment_id"].ToString(),
                            AlarmLevel = Convert.ToInt32(reader["alarm_level"]),
                            AlarmMsg = reader["alarm_msg"].ToString(),
                            Status = reader["status"].ToString(),
                            Timestamp = Convert.ToDateTime(reader["timestamp"])
                        });
                    }
                }
                return result;
            }
        }
    }
}