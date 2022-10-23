using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;// Marshal 쓰기우함

public enum E_PROTOCOL
{
    // 초기 필요 값
    CRYPTOKEY,      // 초기 암복호화키 전송 신호
    IDCREATE,       // 아이디 생성

    PLAYTYPE,       // 멀티 or 싱글 선택
    WAIT,           // 멀티 대기
    SINGLE_START,           // 싱글 시작
    MULTI_HOST_START,       // 호스트(주체) 시작
    MULTI_GUEST_START,      // 게스트(의존) 시작

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
    #region 상수
    const int MAXPACKETNUM = 210000000; // 패킷 넘버링 체크
    const int ProtocolOffset = 0; // 버퍼에서의 프로토콜 위치
    const int PacketNumberOffset = 4;// 버퍼에서의 패킷 넘버 위치
    const int DataOffset = 8;// 버퍼에서의 데이터 위치
    #endregion

    #region 데이터 영역
    SocketEx socket;

    bool m_IsGetKey; // 암호화 키 받은 여부 체크

    // uint => unsigned int 암호키
    uint m_cryptionMainKey;
    uint m_cryptionSubKey1;
    uint m_cryptionSubKey2;

    // 패킷 넘버링 
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

    #region Send 관련
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
        if (m_sendCounter > MAXPACKETNUM) // 패킷 넘버 돌리기
        {
            m_sendCounter = 0;
        }
        return l_tempCounter;
    }
    // 프로토콜과 데이터를 보내는 함수
    public void Write<Data>(int _protocol, Data _data)
    {
        byte[] sendBuffer = new byte[1024];
        int offset = 0;
        int size = 0;
        // 프로토콜
        offset += sizeof(int); // 사이즈 뒤로 프로토콜 4
        Buffer.BlockCopy(BitConverter.GetBytes(_protocol), 0, sendBuffer, offset, sizeof(int));
        size += sizeof(int);
        // 패킷넘버
        offset += sizeof(int);// 프로토콜 뒤로 패킷 넘버 8
        Buffer.BlockCopy(BitConverter.GetBytes(SendPacketCountUp()), 0, sendBuffer, offset, sizeof(int));
        size += sizeof(int);

        offset += sizeof(int);// 패킷넘버 뒤에 구조체
        Buffer.BlockCopy(StructToByteArray(_data), 0, sendBuffer, offset, Marshal.SizeOf(typeof(Data)));
        size += Marshal.SizeOf(typeof(Data));

        // 사이즈 넣기
        offset = 0;
        Buffer.BlockCopy(BitConverter.GetBytes(size), 0, sendBuffer, offset, sizeof(int));
        _sendQ.Enqueue(sendBuffer);
        autoEvent.Set();
    }
    // 데이터를 보내지 않는 프로토콜만 보내는 함수
    public void Write(int _protocol)
    {
        byte[] sendBuffer = new byte[32];
        int size = 0;

        // 프로토콜
        size += sizeof(int);
        Buffer.BlockCopy(BitConverter.GetBytes(_protocol), 0, sendBuffer, size, sizeof(int));
        // 패킷넘버
        size += sizeof(int);
        Buffer.BlockCopy(BitConverter.GetBytes(SendPacketCountUp()), 0, sendBuffer, size, sizeof(int));

        // 사이즈 넣기
        Buffer.BlockCopy(BitConverter.GetBytes(size), 0, sendBuffer, 0, sizeof(int));
        _sendQ.Enqueue(sendBuffer);
        autoEvent.Set();
    }
    #endregion

    #region Recv 관련
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
            if (m_recvCounter > MAXPACKETNUM) // 패킷 넘버 돌리기
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
        // size필드 암호화하지 않기위한 offset값
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
