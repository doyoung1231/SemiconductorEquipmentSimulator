using System;
using System.Text.RegularExpressions;

namespace HostControlProgram
{
    public class EquipmentData
    {
        public string EquipmentId { get; set; }
        public string EquipmentType { get; set; }

        // 공통
        public double ChamberTemp { get; set; }
        public double ChamberPressure { get; set; }

        // ETCH
        public double RfPower { get; set; }
        public double GasFlowCf4 { get; set; }
        public double ChuckTemp { get; set; }

        // CVD
        public double GasFlowSih4 { get; set; }
        public double DepositionRate { get; set; }

        // DIFF
        public double FurnaceTemp { get; set; }
        public double GasFlowO2 { get; set; }
        public int ProcessTime { get; set; }

        public string Status { get; set; }
        public int AlarmLevel { get; set; }
        public string AlarmMsg { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DataParser
    {
        public static EquipmentData Parse(string json)
        {
            try
            {
                var d = new EquipmentData();
                d.Timestamp = DateTime.Now;
                d.EquipmentId = GetString(json, "equipment_id");
                d.EquipmentType = GetString(json, "equipment_type");
                d.ChamberTemp = GetDouble(json, "chamber_temp");
                d.ChamberPressure = GetDouble(json, "chamber_pressure");
                d.RfPower = GetDouble(json, "rf_power");
                d.GasFlowCf4 = GetDouble(json, "gas_flow_cf4");
                d.ChuckTemp = GetDouble(json, "chuck_temp");
                d.GasFlowSih4 = GetDouble(json, "gas_flow_sih4");
                d.DepositionRate = GetDouble(json, "deposition_rate");
                d.FurnaceTemp = GetDouble(json, "furnace_temp");
                d.GasFlowO2 = GetDouble(json, "gas_flow_o2");
                d.ProcessTime = (int)GetDouble(json, "process_time");
                d.Status = GetString(json, "status");
                d.AlarmLevel = (int)GetDouble(json, "alarm_level");
                d.AlarmMsg = GetString(json, "alarm_msg");
                return d;
            }
            catch { return null; }
        }

        private static string GetString(string json, string key)
        {
            var m = Regex.Match(json, $"\"{key}\"\\s*:\\s*\"([^\"]+)\"");
            return m.Success ? m.Groups[1].Value : "";
        }

        private static double GetDouble(string json, string key)
        {
            var m = Regex.Match(json, $"\"{key}\"\\s*:\\s*([\\d\\.\\-]+)");
            return m.Success ? double.Parse(m.Groups[1].Value) : 0;
        }
    }
}