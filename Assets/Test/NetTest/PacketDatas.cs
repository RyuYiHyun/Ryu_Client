using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct QuaternionData
{
    [MarshalAs(UnmanagedType.R4)]
    public float x;
    [MarshalAs(UnmanagedType.R4)]
    public float y;
    [MarshalAs(UnmanagedType.R4)]
    public float z;
    [MarshalAs(UnmanagedType.R4)]
    public float w;
}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct Vector3Data
{
    [MarshalAs(UnmanagedType.R4)]
    public float x;
    [MarshalAs(UnmanagedType.R4)]
    public float y;
    [MarshalAs(UnmanagedType.R4)]
    public float z;
}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct Vector2Data
{
    [MarshalAs(UnmanagedType.R4)]
    public float x;
    [MarshalAs(UnmanagedType.R4)]
    public float y;
}


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct PacketMoveData
{
    [MarshalAs(UnmanagedType.I4)]
    public int m_id;

    public Vector3Data m_position;

    public Vector3Data m_velocity;

    public QuaternionData m_rotation;

    public Vector2Data m_move;

    [MarshalAs(UnmanagedType.R4)]
    public float m_animing;

    [MarshalAs(UnmanagedType.I4)]
    public int m_state;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct PacketFireData
{
    [MarshalAs(UnmanagedType.I4)]
    public int m_id;
    public Vector3Data m_position;
    public Vector3Data m_direction;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct IDData
{
    [MarshalAs(UnmanagedType.I4)]
    public int m_id;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct ListData
{
    [MarshalAs(UnmanagedType.I4)]
    public int m_size;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public int[] m_list;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct TestListData
{
    [MarshalAs(UnmanagedType.I4)]
    public int m_int;
    [MarshalAs(UnmanagedType.I2)]
    public short m_short;
    [MarshalAs(UnmanagedType.R4)]
    public float m_float;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct ElementData
{
    [MarshalAs(UnmanagedType.I4)]
    public int m_int;
    [MarshalAs(UnmanagedType.I2)]
    public short m_short;
    [MarshalAs(UnmanagedType.R4)]
    public float m_float;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Serializable]
public struct ElementListData
{
    [MarshalAs(UnmanagedType.I4)]
    public int m_size;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public ElementData[] m_list;
}