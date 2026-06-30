using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;

public static class Util
{
    public static TextReader LoadTextFromRes(string path)
    {
        TextReader retSteam = null;
        int lp = path.LastIndexOf(".");
        string realPath = lp < 0 ? path : path.Remove(lp);
        TextAsset data = Loaded.Load<TextAsset>(realPath);
        TextReader reader = new StringReader(data.text);
        retSteam = reader;
        return retSteam;
    }

    public static string GetStackInfo(int frameCount = 1)
    {
        string stackInfo = "GetStackInfo failed!";
        StackTrace st = new StackTrace(true);

        if (st == null)
            goto Exit0;

        stackInfo = "";

        for (int i = 1; i <= frameCount; i++)
        {
            StackFrame sf = st.GetFrame(i);
            if (sf == null)
                break;

            string stackInfoLine = string.Format("{0}, Method:{1}, Line:{2}\n", sf.GetFileName(), sf.GetMethod().Name, sf.GetFileLineNumber());

            stackInfo = stackInfo + stackInfoLine;
        }

    Exit0:
        return stackInfo;
    }

    [System.Serializable]
    public class Response<T>
    {
        public List<T> list;
    }


    public static List<T> ParseJsonList<T>(string jsonStr)
    {
        jsonStr = "{ \"list\": " + jsonStr + "}";
        Response<T> nodeList = JsonUtility.FromJson<Response<T>>(jsonStr);

        if (nodeList == null)
            return null;

        return nodeList.list;
    }

    public static List<T> LoadCsv<T>(string path) where T : new()
    {
        TextReader file = Util.LoadTextFromRes(path);
        if (file == null)
        {
            UnityEngine.Debug.LogError("Load Config File Error:" + path);
            return null;
        }
        return Sinbad.CsvUtil.LoadObjects<T>(file, false);
    }
}

public class ConfigData
{
    public int id;
    public virtual string GetName() { return null; }
}

public class NamedData : ConfigData
{
    public string name;
    public override string GetName() { return name; }
}

public class ConfigTable<T> : ConfigTableConvert<T, T>
    where T:ConfigData, new()
{

}

public class ConfigTableConvert<T, D> 
    where T : ConfigData, new()
    where D : ConfigData, new()
{
    Dictionary<int, D> m_configTable = new Dictionary<int, D>();
    List<T> m_configList = new List<T>();
    Dictionary<string, D> m_nameIndex = new Dictionary<string, D>();

    public delegate D ParseFunc(T t);

    public bool Load(string path, ParseFunc parseFunc = null, bool useID = true)
    {
        bool result = false;
        TextReader file = Util.LoadTextFromRes(path);
        if (file == null)
        {
            UnityEngine.Debug.LogError("Load Config File Error:" + path);
            goto Exit0;
        }
        _Load(file, parseFunc, useID);

        result = true;
    Exit0:
        if (file != null)
        {
            file.Close();
        }
        return result;
    }

    public bool LoadText(string text, ParseFunc parseFunc = null, bool useID = true)
    {
        _Load(new StringReader(text), parseFunc, useID);
        return true;
    }

    public void _Load(TextReader input, ParseFunc parseFunc = null, bool useID = true)
    {
        m_configList = Sinbad.CsvUtil.LoadObjects<T>(input, false);

        foreach (T obj in m_configList)
        {
            if (useID && m_configTable.ContainsKey(obj.id))
            {
                // 重复ID
                continue;
            }

            if (typeof(D) == typeof(T) || parseFunc == null)
            {
                m_configTable[obj.id] = obj as D;
                var name = obj.GetName();
                if (name != null && name.Length > 0)
                {
                    m_nameIndex[name] = obj as D;
                }
                continue;
            }

            D data = parseFunc(obj);
            if (data != null)
            {
                m_configTable[obj.id] = data;
                var name = data.GetName();
                if (name != null && name.Length > 0)
                {
                    m_nameIndex[name] = data;
                }
            }
        }
    }

    /*
    public virtual D ParseObj(T obj)
    {
        if (typeof(D) == typeof(T))
            return obj as D;
        return default(D);
    }*/

    public virtual D GetInfo(int index)
    {
        if (m_configTable.ContainsKey(index) == false)
            return default(D);
        return m_configTable[index];
    }

    public bool TryGetInfo(int index, out D data)
    {
        return m_configTable.TryGetValue(index, out data);
    }

    public virtual D GetInfo(string name)
    {
        if (m_nameIndex.ContainsKey(name) == false)
            return default(D);
        return m_nameIndex[name];
    }

    public virtual Dictionary<int,D> GetAllInfo()
    {
        return m_configTable;
    }
    public virtual List<T> GetListInfo()
    {
        return m_configList;
    }   
}

