using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataView : MonoBehaviour
{
    public List<ElementData> elements = new List<ElementData>();
    public List<ElementData> elementsRecv = new List<ElementData>();
    public void SendElementList()
    {
        //ElementListData elementListData = new ElementListData();
        //elementListData.m_size = elements.Count;
        //elementListData.m_list = new ElementData[10];
        //for (int i=0; i< elements.Count;i++)
        //{
        //    elementListData.m_list[i] = elements[i];
        //}
        //MainManager.Network.Session.Write((int)E_PROTOCOL.TEST, elementListData);


        MainManager.Network.Session.Write((int)E_PROTOCOL.TEST, elements);
    }

    public void RecvElementList()
    {
        //ElementListData elementListData;
        //MainManager.Network.Session.GetData<ElementListData>(out elementListData);
        //int b = elementListData.m_size;
        //int a = elementListData.m_list.Length;
        //for (int i = 0; i < elementListData.m_size; i++)
        //{
        //    elementsRecv[i] = elementListData.m_list[i];
        //}
        List<ElementData> tt;
        MainManager.Network.Session.GetListData<ElementData>(out tt);
        elementsRecv = tt;
    }

    private void Start()
    {
        MainManager.Network.NetWorkProcess.Add((int)E_PROTOCOL.TEST, RecvElementList);
    }
    void Update()
    {
        
    }
}
