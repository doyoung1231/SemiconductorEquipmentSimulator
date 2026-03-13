#include "sensor_generator.h"
#include <cstdlib>
#include <ctime>
#include <sstream>
#include <iomanip>
#include <cmath>

SensorGenerator::SensorGenerator(const std::string& equipmentId, EquipmentType type)
    : _equipmentId(equipmentId), _type(type), _processTime(0),
    _currentStatus("IDLE"), _statusTimer(0)
{
    srand((unsigned int)time(nullptr) + (int)type * 1000);
}

// 상태 머신: IDLE→READY→RUN→RUN→...→IDLE
std::string SensorGenerator::DetermineStatus() {
    _statusTimer++;

    if (_currentStatus == "IDLE" && _statusTimer > 3) {
        _currentStatus = "READY"; _statusTimer = 0;
    }
    else if (_currentStatus == "READY" && _statusTimer > 2) {
        _currentStatus = "RUN"; _statusTimer = 0;
    }
    else if (_currentStatus == "RUN" && _statusTimer > 20) {
        // 가끔 PAUSE 또는 다시 IDLE
        int r = rand() % 10;
        if (r < 1) _currentStatus = "PAUSE";
        else       _currentStatus = "IDLE";
        _statusTimer = 0;
    }
    else if (_currentStatus == "PAUSE" && _statusTimer > 3) {
        _currentStatus = "RUN"; _statusTimer = 0;
    }

    return _currentStatus;
}

void SensorGenerator::CheckAlarm(SensorData& data) {
    data.alarm_level = 0;
    data.alarm_msg = "NORMAL";

    if (_type == EquipmentType::ETCH) {
        if (data.chamber_temp > 190) {
            data.alarm_level = 3;
            data.alarm_msg = "CRITICAL: Chamber Temp Overheat";
            data.status = "ALARM";
        }
        else if (data.chamber_temp > 170) {
            data.alarm_level = 2;
            data.alarm_msg = "ALARM: Chamber Temp High";
        }
        else if (data.chamber_temp > 150) {
            data.alarm_level = 1;
            data.alarm_msg = "WARNING: Chamber Temp Warning";
        }
        if (data.rf_power > 1800) {
            data.alarm_level = std::max(data.alarm_level, 2);
            data.alarm_msg = "ALARM: RF Power Overload";
        }
    }
    else if (_type == EquipmentType::CVD) {
        if (data.chamber_temp > 850) {
            data.alarm_level = 3;
            data.alarm_msg = "CRITICAL: Process Temp Critical";
            data.status = "ALARM";
        }
        else if (data.chamber_temp > 800) {
            data.alarm_level = 2;
            data.alarm_msg = "ALARM: Process Temp High";
        }
        else if (data.chamber_temp > 750) {
            data.alarm_level = 1;
            data.alarm_msg = "WARNING: Process Temp Warning";
        }
    }
    else if (_type == EquipmentType::DIFF) {
        if (data.furnace_temp > 1150) {
            data.alarm_level = 3;
            data.alarm_msg = "CRITICAL: Furnace Temp Critical";
            data.status = "ALARM";
        }
        else if (data.furnace_temp > 1100) {
            data.alarm_level = 2;
            data.alarm_msg = "ALARM: Furnace Temp High";
        }
        else if (data.furnace_temp > 1050) {
            data.alarm_level = 1;
            data.alarm_msg = "WARNING: Furnace Temp Warning";
        }
    }
}

double randRange(double min, double max) {
    return min + (double)rand() / RAND_MAX * (max - min);
}

SensorData SensorGenerator::GenerateEtch() {
    SensorData d{};
    d.equipment_id = _equipmentId;
    d.equipment_type = "ETCH";
    d.status = DetermineStatus();

    if (d.status == "RUN") {
        d.chamber_temp = randRange(80, 200);
        d.chamber_pressure = randRange(1, 100);    // mTorr
        d.rf_power = randRange(500, 2000); // W
        d.gas_flow_cf4 = randRange(50, 200);   // sccm
        d.chuck_temp = randRange(20, 80);
    }
    else {
        d.chamber_temp = randRange(20, 40);
        d.chamber_pressure = randRange(700, 760);  // 대기압 근처
        d.rf_power = 0;
        d.gas_flow_cf4 = 0;
        d.chuck_temp = randRange(15, 25);
    }
    return d;
}

SensorData SensorGenerator::GenerateCvd() {
    SensorData d{};
    d.equipment_id = _equipmentId;
    d.equipment_type = "CVD";
    d.status = DetermineStatus();

    if (d.status == "RUN") {
        d.chamber_temp = randRange(300, 900);
        d.chamber_pressure = randRange(0.1, 10);   // Torr
        d.gas_flow_sih4 = randRange(100, 500);  // sccm
        d.deposition_rate = randRange(10, 100);   // Å/min
    }
    else {
        d.chamber_temp = randRange(20, 50);
        d.chamber_pressure = 760;
        d.gas_flow_sih4 = 0;
        d.deposition_rate = 0;
    }
    return d;
}

SensorData SensorGenerator::GenerateDiff() {
    SensorData d{};
    d.equipment_id = _equipmentId;
    d.equipment_type = "DIFF";
    d.status = DetermineStatus();

    if (d.status == "RUN") {
        d.furnace_temp = randRange(600, 1200);
        d.chamber_pressure = randRange(1, 5);      // atm
        d.gas_flow_o2 = randRange(200, 1000); // sccm
        d.process_time = ++_processTime;
    }
    else {
        d.furnace_temp = randRange(20, 50);
        d.chamber_pressure = 1.0;
        d.gas_flow_o2 = 0;
        _processTime = 0;
        d.process_time = 0;
    }
    return d;
}

SensorData SensorGenerator::Generate() {
    SensorData data;
    switch (_type) {
    case EquipmentType::ETCH: data = GenerateEtch(); break;
    case EquipmentType::CVD:  data = GenerateCvd();  break;
    case EquipmentType::DIFF: data = GenerateDiff(); break;
    }
    CheckAlarm(data);
    return data;
}

std::string SensorGenerator::ToJson(const SensorData& d) {
    std::ostringstream o;
    o << std::fixed << std::setprecision(1);
    o << "{"
        << "\"equipment_id\":\"" << d.equipment_id << "\","
        << "\"equipment_type\":\"" << d.equipment_type << "\","
        << "\"chamber_temp\":" << d.chamber_temp << ","
        << "\"chamber_pressure\":" << d.chamber_pressure << ","
        << "\"rf_power\":" << d.rf_power << ","
        << "\"gas_flow_cf4\":" << d.gas_flow_cf4 << ","
        << "\"chuck_temp\":" << d.chuck_temp << ","
        << "\"gas_flow_sih4\":" << d.gas_flow_sih4 << ","
        << "\"deposition_rate\":" << d.deposition_rate << ","
        << "\"furnace_temp\":" << d.furnace_temp << ","
        << "\"gas_flow_o2\":" << d.gas_flow_o2 << ","
        << "\"process_time\":" << d.process_time << ","
        << "\"status\":\"" << d.status << "\","
        << "\"alarm_level\":" << d.alarm_level << ","
        << "\"alarm_msg\":\"" << d.alarm_msg << "\""
        << "}";
    return o.str();
}