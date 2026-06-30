using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class PFunc
{
    //打日志
    static public void Log(params object[] objl)
    {
        // 将所有对象转换为字符串，并用空格分隔
        string logMessage = string.Join("=====", objl);
        // 输出到调试日志
        Debug.Log(logMessage);
    }


    public static void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Object.Destroy(child.gameObject);
        }
    }
    public static void SetChildShow(GameObject obj, int index )
    {
        Transform trans= obj.transform;
        for (int i = 0; i < trans.childCount; i++) 
        {
            trans.GetChild(i).gameObject.SetActive(i==index);
        }
    }

    public static void Shuffle<T>(List<T> list)
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    #region 切割字符串

    /// <summary>
    ///分割：|字符串
    /// </summary>
    public static Dictionary<TKey, TValue> FuncSplitString<TKey, TValue>(string desc, Func<string, TKey> keyConverter,
        Func<string, TValue> valueConverter)
    {
        Dictionary<TKey, TValue> keyValues = new Dictionary<TKey, TValue>();
        string[] type = desc.Split('|');

        for (int i = 0; i < type.Length; i++)
        {
            string[] item = type[i].Split(':');
            TKey key = keyConverter(item[0]);
            TValue value = valueConverter(item[1]);
            keyValues.Add(key, value);
        }

        return keyValues;
    }

    public static List<KeyValuePair<TKey, TValue>> FuncSplitStringKeyValuePair<TKey, TValue>(string desc,
        Func<string, TKey> keyConverter, Func<string, TValue> valueConverter)
    {
        List<KeyValuePair<TKey, TValue>> keyValues = new List<KeyValuePair<TKey, TValue>>();
        string[] type = desc.Split('|');

        for (int i = 0; i < type.Length; i++)
        {
            string[] item = type[i].Split(':');
            KeyValuePair<TKey, TValue> keyValuePair =
                new KeyValuePair<TKey, TValue>(keyConverter(item[0]), valueConverter(item[1]));
            keyValues.Add(keyValuePair);
        }

        return keyValues;
    }

    public static List<Value> FuncSplitString<Value>(string desc, Func<string, Value> valueConverter)
    {
        List<Value> Values = new List<Value>();
        string[] type = desc.Split(':');
        if (type.Length < 0)
            return null;

        for (int i = 0; i < type.Length; i++)
        {
            Value value = valueConverter(type[i]);
            Values.Add(value);
        }

        return Values;
    }

    public static List<Value> FuncSplitStringP<Value>(string desc, Func<string, Value> valueConverter)
    {
        List<Value> Values = new List<Value>();
        string[] type = desc.Split('.');
        if (type.Length < 0)
            return null;

        for (int i = 0; i < type.Length; i++)
        {
            Value value = valueConverter(type[i]);
            Values.Add(value);
        }

        return Values;
    }

    public static List<string> FuncSplitStringShu(string desc)
    {
        List<string> Values = new List<string>();
        string[] type = desc.Split('|');
        if (type.Length < 0)
            return null;
        for (int i = 0; i < type.Length; i++)
        {
            Values.Add(type[i]);
        }

        return Values;
    }

    public static List<string> FuncSplitStringMao(string desc)
    {
        List<string> Values = new List<string>();
        string[] type = desc.Split(':');
        if (type.Length < 0)
            return null;
        for (int i = 0; i < type.Length; i++)
        {
            Values.Add(type[i]);
        }

        return Values;
    }

    public static int[,] ConvertStringTo2DArray(string input)
    {
        // 首先，用'|'将字符串进行分割，得到每一行的数据
        string[] rows = input.Split('|');

        // 获取总行数和列数
        int rowCount = rows.Length;
        int colCount = rows[0].Split(':').Length;

        // 创建二维整型数组
        int[,] result = new int[rowCount, colCount];

        // 遍历每一行，并进一步用':'分割每个元素
        for (int i = 0; i < rowCount; i++)
        {
            string[] cols = rows[i].Split(':');
            for (int j = 0; j < colCount; j++)
            {
                // 将字符串转换为整数并储存到二维数组中
                result[i, j] = int.Parse(cols[j]);
            }
        }

        return result;
    }

    public static Vector3 FuncSplitStringToVector(string value)
    {
        var ves = value.Split(':');
        float x = float.Parse(ves[0]);
        float y = float.Parse(ves[1]);
        float z = 0;

        if (ves.Length > 2)
            z = float.Parse(ves[2]);
        return new Vector3(x, y, z);
    }

    public static Vector2 FuncSplitStringToVector2(string value)
    {
        var ves = value.Split(':');
        float x = float.Parse(ves[0]);
        float y = float.Parse(ves[1]);
        return new Vector2(x, y);
    }

    #endregion

    //数字显示转换
    static public string NumToNum(int num)
    {
        return UInt64ToNum((UInt64)num);
    }

    //数字显示转换
    static public string NumToNum(long num)
    {
        return UInt64ToNum((UInt64)num);
    }

    static public string UInt64ToNum(UInt64 num)
    {
        if (num < 1e4) return num.ToString();

        if (num < 1e7) return ((int)(num / 1e3)).ToString() + "千";

        if (num < 1e10) return ((int)(num / 1e6)).ToString() + "万";

        return ((int)(num / 1e9)).ToString() + "B";
    }


    /// <summary> 传入毫秒获取分秒格式 </summary>
    public static string FormatTime(double timeInSeconds)
    {
        int minutes = (int)Math.Round(timeInSeconds) / 60;
        int seconds = (int)Math.Round(timeInSeconds) % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public static void OnCloseGame()
    {

    }
}