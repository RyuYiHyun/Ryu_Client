using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class MainManager : MonoBehaviour
{

    private static MainManager m_instance = null;
    public static MainManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject mainM = GameObject.Find("@MainManager");
                m_instance = mainM.GetComponent<MainManager>();
            }
            return m_instance;
        }
    }


    private NetWorkManager m_network = new NetWorkManager();
    public static NetWorkManager Network { get => Instance.m_network; }

    private void Awake()
    {
        Screen.SetResolution(900, 600, false);
    }
    public FollowCam followCam;
    public GameObject playerUnit;
    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    

    public PacketMoveData m_mainData;
    public PacketFireData packetFireData;

    public Player mainPlayer;


    void Start()
    {
        m_network.Register(E_PROTOCOL.PLAYER_SPAWN, SpawnProcess);
        m_network.Register(E_PROTOCOL.PLAYER_TRANSFORM, MoveProcess);
        m_network.Register(E_PROTOCOL.LEAVE, OutProcess);
        m_network.Register(E_PROTOCOL.PLAYER_DODGE, DodgeProcess);
        m_network.Register(E_PROTOCOL.PLAYER_JUMP, JumpProcess);
        m_network.Register(E_PROTOCOL.PLAYER_FIRE, FireProcess);

        m_network.Initialize();
    }
    void Update()
    {
        #region 플레이어 컨트롤러
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_network.End();
            //m_network.protocolThread.Join();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit(); // 어플리케이션 종료
#endif
        }
        #endregion


        #region 메세지 처리 루프
        m_network.UpdateRecvProcess();
        #endregion
    }
    public void SendMove(int _id,Vector3 _velocity, Vector3 _position, Quaternion _quaternion, int state)
    {
        m_mainData.m_id = _id;
        m_mainData.m_velocity.x = _velocity.x;
        m_mainData.m_velocity.y = _velocity.y;
        m_mainData.m_velocity.z = _velocity.z;

        m_mainData.m_position.x = _position.x;
        m_mainData.m_position.y = _position.y;
        m_mainData.m_position.z = _position.z;



        m_mainData.m_rotation.x = _quaternion.x;
        m_mainData.m_rotation.y = _quaternion.y;
        m_mainData.m_rotation.z = _quaternion.z;
        m_mainData.m_rotation.w = _quaternion.w;

        m_mainData.m_state = state;

        m_network.Session.Write((int)E_PROTOCOL.PLAYER_TRANSFORM, m_mainData);
    }
    public void SendJump(int _id)
    {
        m_network.Session.Write((int)E_PROTOCOL.PLAYER_JUMP, _id);
    }
    public void SendDodge(int _id)
    {
        m_network.Session.Write((int)E_PROTOCOL.PLAYER_DODGE, _id);
    }
    public void SendFire(int _id, Vector3 _position, Vector3 _direction)
    {
        packetFireData.m_id = _id;

        packetFireData.m_position.x = _position.x;
        packetFireData.m_position.y = _position.y;
        packetFireData.m_position.z = _position.z;

        packetFireData.m_direction.x = _direction.x;
        packetFireData.m_direction.y = _direction.y;
        packetFireData.m_direction.z = _direction.z;

        m_network.Session.Write((int)E_PROTOCOL.PLAYER_FIRE, packetFireData);
    }

    const int m_SendTimeCounterF = 5;
    int m_sendTimeCounter = 0;
    private void FixedUpdate()
    {
            
        //if (mainPlayer != null)
        //{
        //    if (m_SendTimeCounterF <= m_sendTimeCounter)
        //    {
        //        m_mainPlayerData.m_position.x = mainPlayerPos.x;
        //        m_mainPlayerData.m_position.y = mainPlayer.trans.position.y;//mainPlayerPos.y;
        //        m_mainPlayerData.m_position.z = mainPlayerPos.z;

        //        m_mainPlayerData.m_rotation.x = mainPlayerRot.x;
        //        m_mainPlayerData.m_rotation.y = mainPlayerRot.y;
        //        m_mainPlayerData.m_rotation.z = mainPlayerRot.z;
        //        m_mainPlayerData.m_rotation.w = mainPlayerRot.w;

        //        if (walkDown && runDown)
        //        {
        //            m_mainPlayerData.m_state = 1;
        //        }
        //        else if (runDown)
        //        {
        //            m_mainPlayerData.m_state = 2;
        //        }
        //        else
        //        {
        //            m_mainPlayerData.m_state = 0;
        //        }

        //        m_network.Session.Write((int)E_PROTOCOL.CTS_MOVE, m_mainPlayerData);
        //        m_sendTimeCounter = 0;
        //    }
        //    else
        //    {
        //        ++m_sendTimeCounter;
        //    }
        //}
    }

    /*=================<기능 함수(TEST용 -> 클라에 적용시에 각자 해당하는 메니저에서 유사한 기능의 함수를 제작하는 것이 좋음)>=====================*/
    void SpawnProcess()
    {
        ListData liddata;
        m_network.Session.GetData<ListData>(out liddata);

        for (int i = 0; i < liddata.m_size; i++)
        {
            if (liddata.m_list[i] == -1)
            {
                continue;
            }
            if (!players.ContainsKey(liddata.m_list[i]))
            {
                GameObject temp = GameObject.Instantiate(playerUnit, Vector3.zero, Quaternion.identity);
                temp.GetComponent<Player>().id = liddata.m_list[i];
                players.Add(liddata.m_list[i], temp);
                temp.SetActive(true);
                if (liddata.m_list[i] != m_network.ClientId)
                {
                    temp.GetComponent<Player>().rigbody.isKinematic = false;//true;
                }
                else
                {
                    temp.GetComponent<Player>().rigbody.isKinematic = false;
                }

            }
        }

        if (mainPlayer == null)
        {
            m_mainData.m_id = m_network.ClientId;


            mainPlayer = players[m_network.ClientId].GetComponent<Player>();
            mainPlayer.id = m_mainData.m_id;

            followCam.target = mainPlayer.transform;
        }
    }

    
    void DodgeProcess()
    {
        IDData lData;
        m_network.Session.GetData<IDData>(out lData);
        if (players.ContainsKey(lData.m_id))
        {
            players[lData.m_id].GetComponent<LeapRigidbody>().targetstate = 4;
            players[lData.m_id].GetComponent<Player>().animaotr.SetTrigger("doDodge");
            players[lData.m_id].GetComponent<Player>().moveSpeed *= 2;


            StartCoroutine(players[lData.m_id].GetComponent<Player>().DodgeEnd());
        }
    }

    void JumpProcess()
    {
        IDData lData;
        m_network.Session.GetData<IDData>(out lData);
        if (players.ContainsKey(lData.m_id))
        {
            players[lData.m_id].GetComponent<LeapRigidbody>().targetstate = 3;
            players[lData.m_id].GetComponent<Player>().animaotr.SetBool("IsJump", true);
            players[lData.m_id].GetComponent<Player>().animaotr.SetTrigger("doJump");
            players[lData.m_id].GetComponent<Player>().rigbody.AddForce(Vector3.up * Player.jumpPower, ForceMode.Impulse);
        }
    }
    void FireProcess()
    {
        PacketFireData lData;

        m_network.Session.GetData<PacketFireData>(out lData);
        if (players.ContainsKey(lData.m_id))
        {
            players[lData.m_id].GetComponent<Player>().MakeFire(
                new Vector3 (lData.m_position.x, lData.m_position.y, lData.m_position.z)
                , new Vector3(lData.m_direction.x, lData.m_direction.y, lData.m_direction.z));
        }
    }
    void MoveProcess()
    {
        PacketMoveData lData;

        m_network.Session.GetData<PacketMoveData>(out lData);
        //if(lData.m_id != mainPlayer.id)
        {
            if (players.ContainsKey(lData.m_id))
            {
                players[lData.m_id].GetComponent<LeapRigidbody>().targetVelocity =
                    new Vector3(lData.m_velocity.x, lData.m_velocity.y, lData.m_velocity.z);
                players[lData.m_id].GetComponent<LeapRigidbody>().targetPosition =
                    new Vector3(lData.m_position.x, lData.m_position.y, lData.m_position.z);
                players[lData.m_id].GetComponent<LeapRigidbody>().targetRotation =
                    new Quaternion(lData.m_rotation.x, lData.m_rotation.y, lData.m_rotation.z, lData.m_rotation.w);

                if (lData.m_state == 1)
                {
                    players[lData.m_id].GetComponent<Player>().animaotr.SetBool("IsRun", true);
                    players[lData.m_id].GetComponent<Player>().animaotr.SetBool("IsWalk", true);
                }
                else if (lData.m_state == 2)
                {
                    players[lData.m_id].GetComponent<Player>().animaotr.SetBool("IsRun", true);
                    players[lData.m_id].GetComponent<Player>().animaotr.SetBool("IsWalk", false);
                }
                else if (lData.m_state == 0)
                {
                    players[lData.m_id].GetComponent<Player>().animaotr.SetBool("IsRun", false);
                    players[lData.m_id].GetComponent<Player>().animaotr.SetBool("IsWalk", false);
                }
            }
        }
    }
    void OutProcess()
    {
        IDData liddata;
        m_network.Session.GetData<IDData>(out liddata);
        Destroy(players[liddata.m_id]);
        players.Remove(liddata.m_id);
    }
    /*======================================*/
}
