#include "tcp_client.h"
#include <iostream>
#include <WS2tcpip.h>

TcpClient::TcpClient() : _socket(INVALID_SOCKET), _connected(false) {
    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);
}

TcpClient::~TcpClient() {
    Disconnect();
    WSACleanup();
}

bool TcpClient::Connect(const std::string& host, int port) {
    _socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (_socket == INVALID_SOCKET) return false;

    sockaddr_in serverAddr{};
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(port);
    inet_pton(AF_INET, host.c_str(), &serverAddr.sin_addr);

    if (connect(_socket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "[TCP] Connection failed\n";
        return false;
    }

    _connected = true;
    std::cout << "[TCP] Connected to " << host << ":" << port << "\n";
    return true;
}

bool TcpClient::Send(const std::string& message) {
    if (!_connected) return false;
    int result = send(_socket, message.c_str(), (int)message.size(), 0);
    return result != SOCKET_ERROR;
}

void TcpClient::Disconnect() {
    if (_socket != INVALID_SOCKET) {
        closesocket(_socket);
        _socket = INVALID_SOCKET;
        _connected = false;
    }
}