using System;
using System.Collections;
using System.Collections.Generic;
public class GameData : Singleton<GameData>
{
    //杂项
    public ConfigTable<BarrageFuncData> barrage_info = new ConfigTable<BarrageFuncData>();

    public bool Init()
    {
        barrage_info.Load("barrage.csv");

        return true;
    }
}

/// <summary> 数据类 </summary>
public class BarrageFuncData : ConfigData//全局数据
{
    public string name;
    public int type;
    public int group;
    public int executionlevel;
    public int queuestate;
    public int createlevel;
    public int movelevel;
    public int controllevel;
    public int breakfinsh;
    public int queue;
    public string interactivefunc;

    public override string GetName()
    {
        return name;  // 必须重写GetName返回非空字符串
    }
}