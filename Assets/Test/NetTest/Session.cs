using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;// Marshal �������

public enum E_PROTOCOL
{
    // �ʱ� �ʿ� ��
    CRYPTOKEY,      // �ʱ� �Ϻ�ȣȭŰ ���� ��ȣ
    IDCREATE,       // ���̵� ����

    PLAYTYPE,       // ��Ƽ or �̱� ����
    WAIT,           // ��Ƽ ���
    SINGLE_START,           // �̱� ����
    MULTI_HOST_START,       // ȣ��Ʈ(��ü) ����
    MULTI_GUEST_START,      // �Խ�Ʈ(����) ����

    SPAWN,
    MOVE,
    JUMP,
    DODGE,
    FIRE,
    LEAVE,
    EXIT,
};
public class Session
{
    #region ���
    const int MAXPACKETNUM = 210000000; // ��Ŷ �ѹ��� üũ
    const int ProtocolOffset = 0; // ���ۿ����� �������� ��ġ
    const int PacketNumberOffset = 4;// ���ۿ����� ��Ŷ �ѹ� ��ġ
    const int DataOffset = 8;// ���ۿ����� ������ ��ġ
    #endregion

    #region ������ ����
    SocketEx socket;

    bool m_IsGetKey; // ��ȣȭ Ű ���� ���� üũ

    // uint => unsigned int ��ȣŰ
    uint m_cryptionMainKey;
    uint m_cryptionSubKey1;
    uint m_cryptionSubKey2;

    // ��Ŷ �ѹ��� 
    int m_sendCounter = 0;
    int m_recvCounter = 0;

    public Thread recvThread;
    public Thread sendThread;
    static AutoResetEvent autoEvent = new AutoResetEvent(false);
    static AutoResetEvent autoSendEndEvent = new AutoResetEvent(false);
    static AutoResetEvent autoRecvEndEvent = new AutoResetEvent(false);

    private bool _running = true;
    public bool Running { get => _running; }
    private Queue<byte[]> _sendQ = new Queue<byte[]>();
    private Queue<byte[]> _recvQ = new Queue<byte[]>();
    #endregion

    public Session()
    {
        socket = new SocketEx();
    }

    public bool CheckConnecting()
    {
        return socket.m_isConnected;
    }

    public bool Initialize()
    {
        if (socket.Connect("127.0.0.1", 9000))
        {
            _running = true;
            sendThread = new Thread(new ThreadStart(SendThread));
            recvThread = new Thread(new ThreadStart(RecvThread));
            sendThread.Start();
            recvThread.Start();
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public void TreadEnd()
    {
        recvThread.Join();
        sendThread.Join();
        autoSendEndEvent.WaitOne();
        autoRecvEndEvent.WaitOne();
    }
    public void CloseSocket()
    {
        socket.CloseSocket();
    }

    #region Send ����
    void SendThread()
    {
        int l_sendSize = 0;

        while (_running)
        {
            autoEvent.WaitOne();
            if (!_running)
            {
                break;
            }
            while (_sendQ.Count > 0)
            {
                byte[] sendBuffer = _sendQ.Dequeue();
                l_sendSize = BitConverter.ToInt32(sendBuffer, 0) + 4;

                // encryption
                if (m_IsGetKey)
                {
                    Encryption(l_sendSize, sendBuffer);
                }
                socket.Send(sendBuffer, l_sendSize);
            }
        }
        autoSendEndEvent.Set();
    }
    private int SendPacketCountUp()
    {
        int l_tempCounter = m_sendCounter;
        ++m_sendCounter;
        if (m_sendCounter > MAXPACKETNUM) // ��Ŷ �ѹ� ������
        {
            m_sendCounter = 0;
        }
        return l_tempCounter;
    }
    // �������ݰ� �����͸� ������ �Լ�
    public void Write<Data>(int _protocol, Data _data)
    {
        byte[] sendBuffer = new byte[1024];
        int offset = 0;
        int size = 0;
        // ��������
        offset += sizeof(int); // ������ �ڷ� �������� 4
        Buffer.BlockCopy(BitConverter.GetBytes(_protocol), 0, sendBuffer, offset, sizeof(int));
        size += sizeof(int);
        // ��Ŷ�ѹ�
        offset += sizeof(int);// �������� �ڷ� ��Ŷ �ѹ� 8
        Buffer.BlockCopy(BitConverter.GetBytes(SendPacketCountUp()), 0, sendBuffer, offset, sizeof(int));
        size += sizeof(int);

        offset += sizeof(int);// ��Ŷ�ѹ� �ڿ� ����ü
        Buffer.BlockCopy(StructToByteArray(_data), 0, sendBuffer, offset, Marshal.SizeOf(typeof(Data)));
        size += Marshal.SizeOf(typeof(Data));

        // ������ �ֱ�
        offset = 0;
        Buffer.BlockCopy(BitConverter.GetBytes(size), 0, sendBuffer, offset, sizeof(int));
        _sendQ.Enqueue(sendBuffer);
        autoEvent.Set();
    }
    // �����͸� ������ �ʴ� �������ݸ� ������ �Լ�
    public void Write(int _protocol)
    {
        byte[] sendBuffer = new byte[32];
        int size = 0;

        // ��������
        size += sizeof(int);
        Buffer.BlockCopy(BitConverter.GetBytes(_protocol), 0, sendBuffer, size, sizeof(int));
        // ��Ŷ�ѹ�
        size += sizeof(int);
        Buffer.BlockCopy(BitConverter.GetBytes(SendPacketCountUp()), 0, sendBuffer, size, sizeof(int));

        // ������ �ֱ�
        Buffer.BlockCopy(BitConverter.GetBytes(size), 0, sendBuffer, 0, sizeof(int));
        _sendQ.Enqueue(sendBuffer);
        autoEvent.Set();
    }
    #endregion

    #region Recv ����
    void RecvThread()
    {
        int l_recvedSize = 0;

        while (_running)
        {
            byte[] recvBuffer = new byte[2048];
            if (socket.Receive(recvBuffer, out l_recvedSize))
            {
                // decryption
                if (m_IsGetKey)
                {
                    Decryption(l_recvedSize, recvBuffer);
                }

                if (BitConverter.ToInt32(recvBuffer, 0) == (int)E_PROTOCOL.EXIT)
                {
                    _running = false;
                    autoEvent.Set();
                }
                else
                {
                    if (RecvPacketCountUp(BitConverter.ToInt32(recvBuffer, 4)))
                    {
                        _recvQ.Enqueue(recvBuffer);
                    }
                }
            }
        }
        autoRecvEndEvent.Set();
    }
    private bool RecvPacketCountUp(int _counter)
    {
        if (m_recvCounter != _counter)
        {
            return false;
        }
        else
        {
            ++m_recvCounter;
            if (m_recvCounter > MAXPACKETNUM) // ��Ŷ �ѹ� ������
            {
                m_recvCounter = 0;
            }
            return true;
        }
    }
    public bool CheckRead()
    {
        if (_recvQ.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public int GetProtocol()
    {
        byte[] recvBuffer = _recvQ.Peek();
        return BitConverter.ToInt32(recvBuffer, ProtocolOffset);
    }
    public int GetPacketNumber()
    {
        byte[] recvBuffer = _recvQ.Peek();
        return BitConverter.ToInt32(recvBuffer, PacketNumberOffset);
    }

    public void GetData()
    {
        _recvQ.Dequeue();
    }

    public void GetData<Data>(out Data _data) where Data : struct
    {
        byte[] recvBuffer = _recvQ.Dequeue();
        _data = ByteArrayToStruct<Data>(SubArray(recvBuffer, DataOffset,
            (Marshal.SizeOf(typeof(Data))) + DataOffset)
            );
    }
    public void GetListData<Data>(out List<Data> _listData) 
    {
        byte[] recvBuffer = null;
        int l_offset = DataOffset;
        int l_listSize = 0;

        recvBuffer = _recvQ.Dequeue();

        l_listSize = BitConverter.ToInt32(recvBuffer, l_offset);
        l_offset += sizeof(int);

        _listData = new List<Data>();

        for (int i = 0; i < l_listSize; i++)
        {
            Data temp = ByteArrayToData<Data>(recvBuffer, l_offset);
            _listData.Add(temp);
            l_offset += Marshal.SizeOf(typeof(Data));
        }
    }
    #endregion

    #region Util Function
    private byte[] StructToByteArray(object obj)
    {
        int size = Marshal.SizeOf(obj);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    private T ByteArrayToStruct<T>(byte[] buffer) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        if (size > buffer.Length)
        {
            throw new Exception();
        }

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, 0, ptr, size);
        T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
        Marshal.FreeHGlobal(ptr);
        return obj;
    }
    private T ByteArrayToData<T>(byte[] buffer , int startIndex)
    {
        int size = Marshal.SizeOf(typeof(T));
        if (size > buffer.Length)
        {
            throw new Exception();
        }

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, startIndex, ptr, size);
        T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
        Marshal.FreeHGlobal(ptr);
        return obj;
    }
    private byte[] SubArray(byte[] _data, int _startIndex, int _endIndex)
    {
        int length = _endIndex - _startIndex + 1;

        byte[] result = new byte[length];
        Array.Copy(_data, _startIndex, result, 0, length);
        return result;
    }
    #endregion

    #region Encryption & Decryption
    private void Encryption(int _dataSize, byte[] _data)
    {
        // size�ʵ� ��ȣȭ���� �ʱ����� offset��
        int l_offset = sizeof(int);
        uint l_tempKey = m_cryptionMainKey;
        byte[] l_tempData = new byte[2048];

        for (int i = l_offset; i < _dataSize; i++)
        {
            l_tempData[i] = (byte)(_data[i] ^ l_tempKey >> 8);
            l_tempKey = (l_tempData[i] + l_tempKey) * m_cryptionSubKey1 + m_cryptionSubKey2;
        }

        Array.Copy(l_tempData, l_offset, _data, l_offset, _dataSize - l_offset);
    }
    private void Decryption(int _dataSize, byte[] _data)
    {
        uint l_tempKey = m_cryptionMainKey;
        byte l_previousBlock;
        byte[] l_tempData = new byte[2048];

        for (int i = 0; i < _dataSize; i++)
        {
            l_previousBlock = _data[i];
            l_tempData[i] = (byte)(_data[i] ^ l_tempKey >> 8);
            l_tempKey = (l_previousBlock + l_tempKey) * m_cryptionSubKey1 + m_cryptionSubKey2;
        }

        Array.Copy(l_tempData, _data, _dataSize);
    }

    public void CryptoKeyDataSetting()
    {
        byte[] l_recvBuffer = _recvQ.Dequeue();

        m_cryptionMainKey = BitConverter.ToUInt32(l_recvBuffer, 8);
        m_cryptionSubKey1 = BitConverter.ToUInt32(l_recvBuffer, 12);
        m_cryptionSubKey2 = BitConverter.ToUInt32(l_recvBuffer, 16);

        m_IsGetKey = true;
    }
    #endregion
}
