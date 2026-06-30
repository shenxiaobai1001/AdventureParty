using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

public class LogServer : MonoBehaviour
{
    [Header("日志配置")]
    [Tooltip("接口")]
    public string endpoint = "";
    [Tooltip("游戏名称")]
    public string gameName = "鸡蛋历险记";
    [Tooltip("是否启用错误自动上报")]
    public bool enableAutoUpload = true;
    [Tooltip("是否在控制台打印上报结果")]
    public bool verboseLog = true;


    private readonly ConcurrentQueue<QueuedLog> _queue = new ConcurrentQueue<QueuedLog>();
    private bool _subscribed = false;
    private static int _suppressInternalLogs = 0;

    // 缓存IP，避免每次上报都重新获取
    private string _ip;

    public GameObject demo;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        _ip = TryGetLocalIPv4();
    }

    void OnEnable()
    {
        SubscribeLogCallback();
    }

    void OnDisable()
    {
        UnsubscribeLogCallback();
    }

    private void SubscribeLogCallback()
    {
        if (_subscribed) return;
        Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
        _subscribed = true;
    }

    private void UnsubscribeLogCallback()
    {
        if (!_subscribed) return;
        Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
        _subscribed = false;
    }

    private void OnLogMessageReceivedThreaded(string condition, string stackTrace, LogType type)
    {
        if (!enableAutoUpload) return;

        if (_suppressInternalLogs > 0) return;
        if (!string.IsNullOrEmpty(condition))
        {
            if (condition.StartsWith("日志上报失败", StringComparison.Ordinal) ||
                condition.StartsWith("日志上报成功", StringComparison.Ordinal) ||
                condition.StartsWith("日志已写入本地", StringComparison.Ordinal))
            {
                return;
            }
        }
        // 只处理 Error/Exception/Assert
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            var log = new QueuedLog
            {
                level = "ERROR",
                action = type.ToString(),
                detail = BuildDetail(condition, stackTrace)
            };
            _queue.Enqueue(log);
        }
    }

    private string BuildDetail(string condition, string stack)
    {
        var sb = new StringBuilder();
        sb.Append(condition);
        if (!string.IsNullOrEmpty(stack))
        {
            sb.Append("\n");
            sb.Append(stack);
        }
        return sb.ToString();
    }

    private void Update()
    {
        // 主线程消费队列并发起网络请求
        while (_queue.TryDequeue(out var log))
        {
            StartCoroutine(UploadLogCoroutine(log.level, log.action, log.detail));
        }
    }

    private IEnumerator UploadLogCoroutine(string level, string action, string detail)
    {
        if (string.IsNullOrEmpty(endpoint))
        {
            // 未配置接口，直接本地存储日志
            PersistLocally(level, action, detail, 0, "Endpoint not set", null);
            yield break;
        }

        var payload = new LogPayload
        {
            game_name = gameName,
            level = level,
            action = action,
            detail = detail,
            ip = string.IsNullOrEmpty(_ip) ? TryGetLocalIPv4() : _ip,
        };
        string json = JsonUtility.ToJson(payload);

        using (var request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool isError = request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError;

            if (isError)
            {
                if (verboseLog)
                {
                    LogInternalWarning($"日志上报失败: {request.responseCode} {request.error}\n{request.downloadHandler.text}");
                }
                PersistLocally(level, action, detail, request.responseCode, request.error, request.downloadHandler?.text);
            }
            else
            {
                if (verboseLog)
                {
                    LogInternal($"日志上报成功: {request.responseCode}\n{request.downloadHandler.text}");
                }

            }
        }
    }

    private void PersistLocally(string level, string action, string detail, long responseCode, string error, string responseText)
    {
        try
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "ErrorLogs");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, DateTime.Now.ToString("yyyy-MM-dd") + ".json");

            var record = new LocalRecord
            {
                timestamp = DateTime.Now.ToString("o"),
                game_name = gameName,
                level = level,
                action = action,
                detail = detail,
                ip = string.IsNullOrEmpty(_ip) ? TryGetLocalIPv4() : _ip,
                responseCode = responseCode,
                error = error,
                response = responseText
            };
            string json = JsonUtility.ToJson(record);
            File.AppendAllText(file, json + "\n", Encoding.UTF8);

            if (verboseLog)
            {
                LogInternalWarning($"日志已写入本地: {file}");
            }
        }
        catch (Exception ex)
        {
            LogInternalError("写入本地日志失败: " + ex.Message);
        }
    }

    private string TryGetLocalIPv4()
    {
        try
        {
            foreach (var ni in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ni.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipStr = ni.ToString();
                    // 排除自动私有地址 169.254.* 和回环 127.*
                    if (!ipStr.StartsWith("169.254.") && !ipStr.StartsWith("127."))
                    {
                        return ipStr;
                    }
                }
            }
        }
        catch { }
        return null;
    }

    private void LogInternal(string msg)
    {
        try { Interlocked.Increment(ref _suppressInternalLogs); Debug.Log(msg); }
        finally { Interlocked.Decrement(ref _suppressInternalLogs); }
    }

    private void LogInternalWarning(string msg)
    {
        try { Interlocked.Increment(ref _suppressInternalLogs); Debug.LogWarning(msg); }
        finally { Interlocked.Decrement(ref _suppressInternalLogs); }
    }

    private void LogInternalError(string msg)
    {
        try { Interlocked.Increment(ref _suppressInternalLogs); Debug.LogError(msg); }
        finally { Interlocked.Decrement(ref _suppressInternalLogs); }
    }

    [Serializable]
    private struct LogPayload
    {
        public string game_name;
        public string level;
        public string action;
        public string detail;
        public string ip;
    }

    private struct QueuedLog
    {
        public string level;
        public string action;
        public string detail;
    }

    [Serializable]
    private struct LocalRecord
    {
        public string timestamp;
        public string game_name;
        public string level;
        public string action;
        public string detail;
        public string ip;
        public long responseCode;
        public string error;
        public string response;
    }
}
