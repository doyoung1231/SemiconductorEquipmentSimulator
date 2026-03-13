#pragma once
#include <string>

// 장비 타입
enum class EquipmentType { ETCH, CVD, DIFF };

// 실제 반도체 장비 센서 데이터
struct SensorData {
    std::string equipment_id;
    std::string equipment_type;

    // 공통
    double chamber_temp;    // 챔버 온도 (°C)
    double chamber_pressure; // 챔버 압력

    // ETCH 전용
    double rf_power;        // RF Power (W)
    double gas_flow_cf4;    // CF4 가스 유량 (sccm)
    double chuck_temp;      // 척 온도 (°C)

    // CVD 전용
    double gas_flow_sih4;   // SiH4 가스 유량 (sccm)
    double deposition_rate; // 증착 속도 (Å/min)

    // DIFF 전용
    double furnace_temp;    // 퍼니스 온도 (°C)
    double gas_flow_o2;     // O2 가스 유량 (sccm)
    int    process_time;    // 공정 시간 (초)

    std::string status;     // IDLE/READY/RUN/PAUSE/ERROR/ALARM/PM
    int alarm_level;        // 0=정상 1=WARNING 2=ALARM 3=CRITICAL
    std::string alarm_msg;
};

class SensorGenerator {
public:
    SensorGenerator(const std::string& equipmentId, EquipmentType type);
    SensorData Generate();
    std::string ToJson(const SensorData& data);

private:
    std::string   _equipmentId;
    EquipmentType _type;
    int           _processTime;
    std::string   _currentStatus;
    int           _statusTimer;

    SensorData GenerateEtch();
    SensorData GenerateCvd();
    SensorData GenerateDiff();
    std::string DetermineStatus();
    void CheckAlarm(SensorData& data);
};