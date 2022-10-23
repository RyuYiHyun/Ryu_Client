//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Runtime.InteropServices;
//using System.Threading;

//public class MoveTest
//{
//    private static MoveTest instance;
//    public static MoveTest GetInstance()
//    {
//        if(instance == null)
//        {
//            instance = new MoveTest();
//        }
//        return instance;
//    }

//    public Thread recvThread;
//    public Thread sendThread;

//    public string m_Ip = "127.0.0.1";
//    public int m_Port = 9000;
//    private IPEndPoint m_IpEndPoint;
//    public ToServerMovePacket m_SendPacket = new ToServerMovePacket();
//    public ToClientMovePacket m_ReceivePacket = new ToClientMovePacket();

//    public ToServerProtocolPacket m_Protocol = new ToServerProtocolPacket();

//    private bool m_IsConnected;
//    private Socket m_Socket;

//    public int m_packetCounter = 0;
//    public void InitSocket()
//    {
//        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//        IPAddress ipAddress = IPAddress.Parse(m_Ip);
//        m_IpEndPoint = new IPEndPoint(ipAddress, m_Port);

//        if (!m_IsConnected)
//            ConnectServer();
//    }

//    public enum E_PROTOCOL
//    {
//        SPAWN,	// 서버 -> 클라, 클라 -> 서버 :  스폰 신호
//		MOVE,	// 서버 -> 클라, 클라 -> 서버 :  이동 신호
//		EXIT,	// 서버 -> 클라, 클라 -> 서버 :  종료 신호					
//	};
//    void ConnectServer()
//    {
//        try
//        {
//            m_Socket.Connect(m_IpEndPoint);
//            m_IsConnected = true;

//            //sendThread = new Thread(new ThreadStart(SendThread));
//            recvThread = new Thread(new ThreadStart(RecvThread));
//            //sendThread.Start();
//            recvThread.Start();

//            //recvThread.Join(); 강제 종료
//        }
//        catch (SocketException ex)
//        {
            
//        }
//    }
//    public void CloseSocket()
//    {
//        m_Socket.Close();
//        m_Socket = null;
//    }

//    private Queue<int> _sendQ = new Queue<int>();
//    private Queue<int> _recvQ = new Queue<int>();

//    private bool _running = true;
//    static AutoResetEvent autoEvent = new AutoResetEvent(false);
//    void SendThread()
//    {
//        while (_running)
//        {
//            autoEvent.WaitOne();
//            lock (_sendQ)
//            {
//                while(_sendQ.Count > 0)
//                {
//                    Send();
//                }
//            }
//        }
//    }
//    void RecvThread()
//    {
//        while (true)
//        {
            
//            Receive();
//        }
//    }


//    public void SetSendPacket(int x, int y)
//    {
//        m_SendPacket.Size = 20;
//        m_SendPacket.Protocol = (int)E_PROTOCOL.MOVE;
//        m_SendPacket.PacketNumber = m_packetCounter++;
//        m_SendPacket.m_ID = 0;
//        m_SendPacket.m_IntX = x;
//        m_SendPacket.m_IntY = y;
//    }


//    T ByteArrayToStruct<T>(byte[] buffer) where T : struct
//    {
//        int size = Marshal.SizeOf(typeof(T));
//        if (size > buffer.Length)
//        {
//            throw new Exception();
//        }

//        IntPtr ptr = Marshal.AllocHGlobal(size);
//        Marshal.Copy(buffer, 0, ptr, size);
//        T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
//        Marshal.FreeHGlobal(ptr);
//        return obj;
//    }
//    byte[] StructToByteArray(object obj)
//    {
//        int size = Marshal.SizeOf(obj);
//        byte[] arr = new byte[size];
//        IntPtr ptr = Marshal.AllocHGlobal(size);

//        Marshal.StructureToPtr(obj, ptr, true);
//        Marshal.Copy(ptr, arr, 0, size);
//        Marshal.FreeHGlobal(ptr);
//        return arr;
//    }

//    private bool sizeflag = false;
//    void Receive()
//    {
//        int receive = 0;
//        try
//        {
//            byte[] receivedbytes = new byte[4];
//            receive = m_Socket.Receive(receivedbytes);

//            m_ReceivePacket = ByteArrayToStruct<ToClientMovePacket>(receivedbytes);
//        }
//        catch (Exception ex)
//        {
//            return;
//        }

//        if (receive > 0)
//        {
//            DoReceivePacket(); // 받은 값 처리
//        }
//    }

//    public int X;
//    void DoReceivePacket()
//    {
//        X = m_ReceivePacket.m_IntX;
//    }

//    public void Send()
//    {
//        try
//        {
//            byte[] sendPacket = StructToByteArray(m_SendPacket);
//            m_Socket.Send(sendPacket);
//        }
//        catch (SocketException ex)
//        {
//        }
//    }

//    public void SpawnSend()
//    {
//        try
//        {
//            m_Protocol.Size = 8;
//            m_Protocol.Protocol = (int)E_PROTOCOL.SPAWN;
//            m_Protocol.PacketNumber = m_packetCounter;
//            m_packetCounter++;
//            byte[] sendPacket = StructToByteArray(m_Protocol);
//            m_Socket.Send(sendPacket);
//        }
//        catch (SocketException ex)
//        {

//        }
//    }
//    public void ExitSend()
//    {
//        try
//        {
//            m_Protocol.Size = 8;
//            m_Protocol.Protocol = (int)E_PROTOCOL.EXIT;
//            m_Protocol.PacketNumber = m_packetCounter;
//            m_packetCounter++;
//            byte[] sendPacket = StructToByteArray(m_Protocol);
//            m_Socket.Send(sendPacket);
//        }
//        catch (SocketException ex)
//        {

//        }
//    }
//}
