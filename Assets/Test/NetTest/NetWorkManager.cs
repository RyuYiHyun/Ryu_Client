using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class NetWorkManager
{
    public Session session;
    public Session Session { get => session; }

    private Dictionary<int, Action> m_NetWorkProcess = new Dictionary<int, Action>();
    public Dictionary<int, Action> NetWorkProcess { get => m_NetWorkProcess; }

    private int m_clientId = -1;
    public int ClientId { get => m_clientId; }

    public void Initialize()
    {
        Register();
        session = new Session();
        if (session.Initialize())
        {

        }
    }
    public void Register()
    {
        m_NetWorkProcess.Add((int)E_PROTOCOL.CRYPTOKEY, KeyProcess);
        m_NetWorkProcess.Add((int)E_PROTOCOL.IDCREATE, IdProcess);
        m_NetWorkProcess.Add((int)E_PROTOCOL.SINGLE_START, SingleProcess);
        m_NetWorkProcess.Add((int)E_PROTOCOL.WAIT, WaitProcess);
        m_NetWorkProcess.Add((int)E_PROTOCOL.MULTI_HOST_START, MultiProcess);
        m_NetWorkProcess.Add((int)E_PROTOCOL.MULTI_GUEST_START, MultiProcess);
    }
    public void Register(E_PROTOCOL _protocol, Action _action)
    {
        if (m_NetWorkProcess.ContainsKey((int)_protocol) == false)
        {
            m_NetWorkProcess.Add((int)_protocol, _action);
        }
    }
    private void KeyProcess()
    {
        session.CryptoKeyDataSetting();
        session.Write((int)E_PROTOCOL.IDCREATE); // 접속
    }
    private void IdProcess()
    {
        int l_id = -1;
        session.GetData(out l_id);
        m_clientId = l_id;
        //session.Write((int)E_PROTOCOL.SPAWN); // 스폰요청
    }
    
    private void SingleProcess()
    {
        session.GetData();
        session.Write((int)E_PROTOCOL.SPAWN); // 스폰요청
    }

    private void WaitProcess()
    {
        session.GetData();
    }

    private void MultiProcess()
    {
        session.GetData();
        session.Write((int)E_PROTOCOL.SPAWN); // 스폰요청
    }

    public void End()
    {
        if (session.CheckConnecting())
        {
            session.Write((int)E_PROTOCOL.EXIT);// 종료
            session.TreadEnd();
            session.CloseSocket();
        }
    }
    public void UpdateRecvProcess()
    {
        bool flag = true;
        while(flag)
        {
            if (session.CheckRead())
            {
                if (m_NetWorkProcess.ContainsKey(session.GetProtocol()) == true)
                {
                    m_NetWorkProcess[session.GetProtocol()].Invoke();
                }
                else
                {
                    flag = false;
                }
            }
            else
            {
                flag = false;
            }
        }
    }
}
