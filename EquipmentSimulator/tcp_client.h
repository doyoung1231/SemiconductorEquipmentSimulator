#pragma once
#pragma once
#include <string>
#include <WinSock2.h>
#pragma comment(lib, "ws2_32.lib")

class TcpClient {
public:
    TcpClient();
    ~TcpClient();

    bool Connect(const std::string& host, int port);
    bool Send(const std::string& message);
    void Disconnect();

private:
    SOCKET _socket;
    bool _connected;
};