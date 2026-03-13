# Semiconductor Equipment Monitoring System

> 반도체 장비 데이터를 실시간으로 수집 · 저장 · 모니터링하는 Host Control System

---

## 📌 프로젝트 개요

실제 반도체 공장(Fab)의 설비 SW 구조를 기반으로 설계한 시뮬레이터입니다.  
가상의 반도체 장비(ETCH, CVD, DIFF)가 TCP 소켓으로 센서 데이터를 전송하면,  
Host Control Program이 이를 수신하여 DB에 저장하고 실시간으로 모니터링합니다.

---

## 🏗 시스템 구조
```
┌──────────────────────┐
│ Equipment Simulator  │
│ (C++)                │
│ ETCH_01 / CVD_01     │
│ DIFF_01              │
└──────────┬───────────┘
           │ TCP Socket
           ▼
┌──────────────────────┐
│ Host Control Program │
│ (C# / WinForms)      │
│ - 데이터 수신           │
│ - 알람 감지            │
│ - UI 표시             │
└──────────┬───────────┘
           │ SQLite
           ▼
┌──────────────────────┐
│ Database             │
│ - equipment_data     │
│ - alarm_history      │
└──────────────────────┘
```

---

## 🛠 기술 스택

| 구분 | 기술 |
|------|------|
| Equipment Simulator | C++ |
| Host Control Program | C#, .NET Framework, WinForms |
| 통신 | TCP Socket |
| 데이터베이스 | SQLite |
| 개발 환경 | Visual Studio 2026, Windows 11 |

---

## ⚙ 장비 및 센서 데이터

### ETCH_01 — Etcher (식각 장비)
| 센서 | 범위 | 단위 |
|------|------|------|
| Chamber Temperature | 20 ~ 200 | °C |
| Chamber Pressure | 1 ~ 100 | mTorr |
| RF Power | 0 ~ 2000 | W |
| Gas Flow (CF4) | 0 ~ 200 | sccm |
| Chuck Temperature | 15 ~ 80 | °C |

### CVD_01 — CVD (화학 기상 증착)
| 센서 | 범위 | 단위 |
|------|------|------|
| Process Temperature | 300 ~ 900 | °C |
| Chamber Pressure | 0.1 ~ 10 | Torr |
| Gas Flow (SiH4) | 0 ~ 500 | sccm |
| Deposition Rate | 0 ~ 100 | Å/min |

### DIFF_01 — Diffusion (확산 장비)
| 센서 | 범위 | 단위 |
|------|------|------|
| Furnace Temperature | 600 ~ 1200 | °C |
| Chamber Pressure | 1 ~ 5 | atm |
| Gas Flow (O2) | 0 ~ 1000 | sccm |
| Process Time | 실시간 | s |

---

## 🚨 알람 체계

| 레벨 | 조건 | 표시 |
|------|------|------|
| Level 1 WARNING | 임계값 80% 도달 | 노란색 |
| Level 2 ALARM | 임계값 초과 | 주황색 |
| Level 3 CRITICAL | 즉시 공정 중단 필요 | 빨간색 + 팝업 |

---

## 🖥 장비 상태 머신
```
IDLE → READY → RUN → PAUSE → RUN
                 ↓
               IDLE
```

---

## 📊 주요 기능

### 1. Dashboard
- 장비 3개 실시간 상태 모니터링
- 센서값 실시간 업데이트
- 알람 레벨별 색상 표시

### 2. Chart
- 장비별 온도 / 압력 실시간 그래프
- 최근 30개 데이터 포인트 표시

### 3. Alarm History
- 알람 발생 이력 조회
- 레벨별 색상 구분
- 알람 통계 (WARNING / ALARM / CRITICAL 건수)

### 4. DB Viewer
- 장비 / 상태 / 날짜 필터 조회
- CSV Export 기능

---

## 📁 프로젝트 구조
```
SemiconductorEquipmentSimulator/
├── EquipmentSimulator/          # C++ 장비 시뮬레이터
│   ├── EquipmentSimulator.cpp   # Main
│   ├── sensor_generator.h/cpp  # 센서 데이터 생성
│   └── tcp_client.h/cpp        # TCP 클라이언트
│
├── HostControlProgram/          # C# Host 프로그램
│   ├── MainForm.cs              # 메인 UI
│   ├── MainForm.Designer.cs     # UI 레이아웃
│   ├── TcpServer.cs             # TCP 서버
│   ├── DataParser.cs            # JSON 파싱
│   ├── DatabaseManager.cs       # DB 관리
│   └── AlarmManager.cs         # 알람 관리
│
└── README.md
```

---

## 🚀 실행 방법

### 1. HostControlProgram 먼저 실행
```
HostControlProgram 우클릭
→ 디버그 → 새 인스턴스 시작
```

### 2. EquipmentSimulator 실행
```
EquipmentSimulator 우클릭
→ 디버그 → 새 인스턴스 시작
```

### 3. 포트 구성
```
ETCH_01 → Port 9001
CVD_01  → Port 9002
DIFF_01 → Port 9003
```

---

## 📸 실행 화면

### Dashboard
![dashboard](docs/dashboard.png)

### Chart
![chart](docs/chart.png)

### Alarm History
![alarm](docs/alarm.png)

### DB Viewer
![dbviewer](docs/dbviewer.png)
