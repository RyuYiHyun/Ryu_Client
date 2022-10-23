using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ReadProtocol();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void ReadProtocol()
    {
        byte[] buffer = new byte[12];
        Debug.Log(buffer);

        int num1 = 10;
        int num2 = 111;
        int num3 = 356;
        //Array.Copy(BitConverter.GetBytes(num1), 0, buffer, 0, sizeof(int));
        Buffer.BlockCopy(BitConverter.GetBytes(num1), 0, buffer, 0, sizeof(int));
        Array.Copy(BitConverter.GetBytes(num2), 0, buffer, 4, sizeof(int));
        Array.Copy(BitConverter.GetBytes(num3), 0, buffer, 8, sizeof(int));



        
        byte[] protocol1 = new byte[sizeof(int)];
        byte[] protocol2 = new byte[sizeof(int)];
        byte[] protocol3 = new byte[sizeof(int)];
        Buffer.BlockCopy(buffer, 0, protocol1, 0, sizeof(int));
        //Array.Copy(buffer, 0, protocol1, 0, sizeof(int));
        Debug.Log(BitConverter.ToInt32(protocol1));

        Buffer.BlockCopy(buffer, 4, protocol2, 0, sizeof(int));
        Debug.Log(BitConverter.ToInt32(protocol2));

        Buffer.BlockCopy(buffer, 8, protocol3, 0, sizeof(int));
        Debug.Log(BitConverter.ToInt32(protocol3));

        Debug.Log(BitConverter.ToInt32(buffer, 0));
        Debug.Log(BitConverter.ToInt32(buffer, 4));
        Debug.Log(BitConverter.ToInt32(buffer, 8));

        return;
    }
}
