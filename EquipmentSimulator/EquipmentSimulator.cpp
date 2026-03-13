#include <iostream>
#include <thread>
#include <chrono>
#include <vector>
#include <memory>
#include "tcp_client.h"
#include "sensor_generator.h"

// 장비 하나를 독립 스레드로 실행
void RunEquipment(const std::string& id, EquipmentType type,
    const std::string& host, int port)
{
    SensorGenerator generator(id, type);

    while (true) {
        TcpClient client;
        if (client.Connect(host, port)) {
            std::cout << "[" << id << "] Connected\n";
            while (true) {
                SensorData data = generator.Generate();
                std::string json = generator.ToJson(data);
                std::cout << "[" << id << "] " << json << "\n";
                if (!client.Send(json + "\n")) {
                    std::cerr << "[" << id << "] Send failed. Reconnecting...\n";
                    break;
                }
                std::this_thread::sleep_for(std::chrono::seconds(2));
            }
        }
        else {
            std::cout << "[" << id << "] Waiting for Host...\n";
            std::this_thread::sleep_for(std::chrono::seconds(3));
        }
    }
}

int main() {
    const std::string HOST = "127.0.0.1";
    const int PORT = 9000;

    std::cout << "=== Semiconductor Equipment Simulator ===\n";
    std::cout << "Equipments: ETCH_01, CVD_01, DIFF_01\n";
    std::cout << "Host: " << HOST << ":" << PORT << "\n\n";

    // 장비 3개 각각 별도 스레드로 실행
    std::thread t1(RunEquipment, "ETCH_01", EquipmentType::ETCH, HOST, 9001);
    std::thread t2(RunEquipment, "CVD_01", EquipmentType::CVD, HOST, 9002);
    std::thread t3(RunEquipment, "DIFF_01", EquipmentType::DIFF, HOST, 9003);

    t1.join();
    t2.join();
    t3.join();

    return 0;
}