using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct ToClientMovePacket
{
    [MarshalAs(UnmanagedType.I4)]
    public int Size;
    [MarshalAs(UnmanagedType.I4)]
    public int Protocol;
    [MarshalAs(UnmanagedType.I4)]
    public int PacketNumber;
    [MarshalAs(UnmanagedType.I4)]
    public int m_ID;
    [MarshalAs(UnmanagedType.I4)]
    public int m_IntX;
    [MarshalAs(UnmanagedType.I4)]
    public int m_IntY;
}


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct ToServerMovePacket
{
    [MarshalAs(UnmanagedType.I4)]
    public int Size;
    [MarshalAs(UnmanagedType.I4)]
    public int Protocol;
    [MarshalAs(UnmanagedType.I4)]
    public int PacketNumber;
    [MarshalAs(UnmanagedType.I4)]
    public int m_ID;
    [MarshalAs(UnmanagedType.I4)]
    public int m_IntX;
    [MarshalAs(UnmanagedType.I4)]
    public int m_IntY;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct ToServerProtocolPacket
{
    [MarshalAs(UnmanagedType.I4)]
    public int Size;
    [MarshalAs(UnmanagedType.I4)]
    public int Protocol;
    [MarshalAs(UnmanagedType.I4)]
    public int PacketNumber;
}