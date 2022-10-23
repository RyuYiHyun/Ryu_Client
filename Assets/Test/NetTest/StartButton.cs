using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    public bool isMulti;
    private bool isStart = false;
    public void StartGame()
    {
        if(!isStart)
        {
            if (isMulti)
            {
                MainManager.Network.Session.Write((int)E_PROTOCOL.PLAYTYPE, 2); // 요청
            }
            else
            {
                MainManager.Network.Session.Write((int)E_PROTOCOL.PLAYTYPE, 1); // 요청
            }
            isStart = true;
        }
    }
}
