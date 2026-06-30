using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CSVData : Singleton<CSVData>
{
    //µ¯´°
    public ConfigTable<Level> level = new ConfigTable<Level>();
    public bool Init()
    {
        level.Load("Level.csv");
        return true;
    }

    #region ·Ö¸î·½·¨
    List<int> SplitInt(string input)
    {
        var retList = new List<int>();
        if (input == null || input.Length == 0)
        {
            return retList;
        }

        var list = input.Split('|');
        foreach (var it in list)
        {
            var value = 0;
            var ret = int.TryParse(it, out value);
            if (!ret)
                continue;

            retList.Add(value);
        }
        return retList;
    }

    List<string> SplitStr(string input)
    {
        var retList = new List<string>();
        if (input == null || input.Length == 0)
        {
            return retList;
        }

        var list = input.Split('|');
        foreach (var it in list)
        {
            retList.Add(it);
        }
        return retList;
    }

    List<float> SplitFloat(string input, int type)
    {
        var retList = new List<float>();
        if (input == null || input.Length == 0)
        {
            return retList;
        }

        var list = type == 1 ? input.Split(':') : input.Split('|');
        foreach (var it in list)
        {
            float value = 0;
            var ret = float.TryParse(it, out value);
            if (!ret)
                continue;

            retList.Add(value);
        }
        return retList;
    }

    public List<Tuple<int, int>> SplitPairInt(string input, int defaultV2)
    {
        var retList = new List<Tuple<int, int>>();

        if (input == null || input.Length == 0)
        {
            return retList;
        }

        var list = input.Split('|');
        foreach (var it in list)
        {
            var t = it.Split(":");
            if (t.Length < 1) continue;

            var v1 = 0;
            var v2 = defaultV2;
            var ret = int.TryParse(t[0], out v1);
            if (!ret || v1 <= 0)
                continue;

            if (t.Length == 2)
            {
                ret = int.TryParse(t[1], out v2);
                if (!ret || v2 <= 0)
                    continue;
            }
            retList.Add(Tuple.Create(v1, v2));
        }

        return retList;
    }

    public List<Tuple<int, int>> SplitPairIntMin(string input, int defaultV2)
    {
        var retList = new List<Tuple<int, int>>();

        if (input == null || input.Length == 0)
        {
            return retList;
        }

        var list = input.Split('|');
        foreach (var it in list)
        {
            var t = it.Split(":");
            if (t.Length < 1) continue;

            var v1 = 0;
            var v2 = defaultV2;
            var ret = int.TryParse(t[0], out v1);
            if (!ret)
                continue;

            if (t.Length == 2)
            {
                ret = int.TryParse(t[1], out v2);
                if (!ret)
                    continue;
            }
            retList.Add(Tuple.Create(v1, v2));
        }

        return retList;
    }
    public List<Tuple<float, float>> SplitPairFloatMin(string input, int defaultV2)
    {
        var retList = new List<Tuple<float, float>>();

        if (input == null || input.Length == 0)
        {
            return retList;
        }

        var list = input.Split('|');
        foreach (var it in list)
        {
            var t = it.Split(":");
            if (t.Length < 1) continue;

            float v1 = 0;
            float v2 = defaultV2;
            var ret = float.TryParse(t[0], out v1);
            if (!ret)
                continue;

            if (t.Length == 2)
            {
                ret = float.TryParse(t[1], out v2);
                if (!ret)
                    continue;
            }
            retList.Add(Tuple.Create(v1, v2));
        }

        return retList;
    }
    public List<int> SplitListInt(string input)
    {
        var retList = new List<int>();

        if (input == null || input.Length == 0)
        {
            return retList;
        }

        var List = input.Split(':');
        for (int i = 0; i < List.Length; i++)
        {
            retList.Add(int.Parse(List[i]));
        }

        return retList;
    }
    public List<string> SplitStringInt(string input)
    {
        if (input == null || input.Length == 0)
        {
            return null;
        }

        var List = input.Split(':').ToList();
        return List;
    }

    int[] ParseBossID(string bossidstr)
    {
        if (bossidstr == null || bossidstr == "")
            return new int[0];
        var bosslist = bossidstr.Split("|");
        int[] result = new int[bosslist.Length];
        for (int i = 0; i < bosslist.Length; i++)
            result[i] = int.Parse(bosslist[i]);
        return result;
    }

    #endregion
}

public class Level : ConfigData
{
    public int group;
    public int count;
    public int pickType;
    public string position;
    public int type;
    public int hp;
    public int flySpeed;
    public float interval;
    public int atk;
    public int reward;
    public int maptype;
    public int maxMonsters;
    public string bgm;
    public string userName;
    public int endless;
}