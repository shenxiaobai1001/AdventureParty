using System.Collections.Generic;
using UnityEngine;

/// <summary>数据存储</summary>
public class DataStorage {
    public static string GetKeyStr(string key) {
        return key;
    }

    public static void Set(string key, string value) {
        PlayerPrefs.SetString(GetKeyStr(key), value);
    }
    public static void Set(string key, bool value) {
        PlayerPrefs.SetInt(GetKeyStr(key), value ? 1 : 0);
    }
    public static void Set(string key, int value) {
        PlayerPrefs.SetInt(GetKeyStr(key), value);
    }
    public static void Set(string key, float value) {
        PlayerPrefs.SetFloat(GetKeyStr(key), value);
    }
    public static void Set(string key, int[] value) {
        var str = "";
        foreach(var i in value) {
            if(str != "")
                str += ",";
            str += i;
        }
        PlayerPrefs.SetString(GetKeyStr(key), str);
    }
    public static void Set(string key, List<int> value) {
        Set(key, value.ToArray());
    }
    public static string GetString(string key) {
        string value = PlayerPrefs.GetString(GetKeyStr(key), "");

        return value;
    }

    public static void Set(string key, List<string> value) {
        var str = "";
        foreach(var i in value) {
            if(str != "")
                str += ",";
            str += i;
        }
        PlayerPrefs.SetString(GetKeyStr(key), str);
    }
    public static bool GetBool(string key) {
        bool value = PlayerPrefs.GetInt(GetKeyStr(key), 0) != 0;
        return value;
    }
    public static int GetInt(string key) {
        int value = PlayerPrefs.GetInt(GetKeyStr(key), 0);
        return value;
    }
    public static float GetFloat(string key) {
        float value = PlayerPrefs.GetFloat(GetKeyStr(key), 0F);
        return value;
    }
    public static int[] GetIntArray(string key) {
        return GetIntList(key).ToArray();
    }
    public static List<int> GetIntList(string key) {
        string value = PlayerPrefs.GetString(GetKeyStr(key), "");
        if(value == null || value == "")
            return new();

        List<int> d = new();

        string[] s = value.Split(",");

        foreach(var i in s) {
            int d1 = 0;
            var ok = int.TryParse(i, out d1);
            if(ok)
                d.Add(d1);
        }

        return d;
    }
    public static List<string> GetStringList(string key) {
        string value = PlayerPrefs.GetString(GetKeyStr(key), "");
        if(value == null || value == "")
            return new();

        List<string> d = new();

        string[] s = value.Split(",");

        foreach(var i in s) {
            d.Add(i);
        }

        return d;
    }
    public static bool HasKey(string key) => PlayerPrefs.HasKey(key);

    public static void Delete(string key) {
        PlayerPrefs.DeleteKey(GetKeyStr(key));
    }
    public static void DeleteAll() {
        PlayerPrefs.DeleteAll();
    }
}