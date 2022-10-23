using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class SocketEx
{
    private Socket m_Socket;

    private IPAddress m_serverAddress;
    public bool m_isConnected;

    public SocketEx()
    {
        m_isConnected = false;
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);// socket 설정
    }



    public bool Connect(string _serverIp, int _port)
    {
        try
        {
            m_serverAddress = IPAddress.Parse(_serverIp); // IP 자료형 변환
            m_Socket.Connect(m_serverAddress, _port);
            m_isConnected = true;
            return true;
        }
        catch(SocketException ex)
        {
            return false;
        }
    }

    public void CloseSocket()
    {
        m_isConnected = false;
        if(m_Socket != null)
        {
            m_Socket.Close();
            m_Socket = null;
        }
    }

    public bool Send(byte[] _sendBuffer, int _size)
    {
        if (m_isConnected == false)
        {
            return false;
        }

        int offset = 0;
        int size = _size;
        int sendValue = 0;
        int totalSendValue = 0;

        while (true)
        {
            sendValue = 0;
            try
            {
                sendValue = m_Socket.Send(_sendBuffer, offset, size, SocketFlags.None);
            }
            catch(SocketException ex)
            {
                return false;
            }

            totalSendValue += sendValue;

            if (totalSendValue == _size)
            {
                return true;
            }
            else
            {
                offset += sendValue;
                size -= sendValue;
            }
        }
    }


    public bool Receive(byte[] _buffer, out int _recvedSize)
    {
        _recvedSize = -1;

        if (m_isConnected == false)
        {
            return false;
        }
        int size;
        int value;
        while (true)
        {
            try
            {
                value = Receiven(_buffer, sizeof(int), SocketFlags.None);
            }
            catch (SocketException ex)
            {
                return false;
            }
            if (value == 0)
            {
                return false;
            }

            size = BitConverter.ToInt32(_buffer, 0);
            try
            {
                value = Receiven(_buffer, size, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                return false;
            }
            if (value == 0)
            {
                return false;
            }
            break;
        }

        _recvedSize = size;
        return true;
    }

    private int Receiven(byte[] _buffer, int _len, SocketFlags _socketFlag)
    {
        int received;
        int left = _len;
        int offset = 0;
        while (left > 0)
        {
            try
            {
                received = m_Socket.Receive(_buffer, offset, left, _socketFlag);
            }
            catch(SocketException ex)
            {
                return -1;
            }
            if (received == 0)
            {
                break;
            }
            left -= received;
            offset += received;
        }
        return (_len - left);
    }
}
