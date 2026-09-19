using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARTelemetryLogger : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public ARTrackedImageManager trackedImageManager;

    private long lastCameraTimestampNs; // 用于缓存底层相机帧生成瞬间的纳秒级时间戳
    private ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>(); // 无锁并发队列，确保在异步写入文件时不阻塞 Unity 的主线程
    private string logFilePath;

    void Start()
    {
        // 在iOS设备上，文件将保存在沙盒的 Documents 目录下
        logFilePath = Path.Combine(UnityEngine.Application.persistentDataPath, "ar_latency_log.csv");
        File.WriteAllText(logFilePath, "EngineTime,PipelineDeley_ms,TrackingState\n");
        InvokeRepeating(nameof(DataToFile), 1f, 0.5f);
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived; // 订阅摄像头的 frameReceived 事件（捕获底层纳秒级时间戳）
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged); // 订阅追踪管理器的 trackablesChanged 事件（获取位姿解算完成的时刻）
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged); 
        DataToFile();
    }

    void OnApplicationQuit()
    {
        CancelInvoke(nameof(DataToFile)); DataToFile();
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.timestampNs.HasValue)
        {
            lastCameraTimestampNs = args.timestampNs.Value; // 获取底层硬件曝光结束的纳秒时间戳
        }
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        // 获取当前系统时间的纳秒值
        long currentTimestampNs = (long)(Stopwatch.GetTimestamp() * (1_000_000_000.0 / Stopwatch.Frequency));
        // 计算管线延迟（毫秒）
        long pipelineLatencyMs = (currentTimestampNs - lastCameraTimestampNs) / 1_000_000;

        foreach (var img in args.updated) // 使用 foreach 循环遍历获取每一张图片的 trackingState
        {
            logQueue.Enqueue($"{Time.time},{pipelineLatencyMs},{img.trackingState}");
        }
    }

    private void DataToFile()
    {
        if (logQueue.Count == 0)return;
        using (StreamWriter sw = File.AppendText(logFilePath))
        {
            while (logQueue.TryDequeue(out string logEntry))
            {
                sw.WriteLine(logEntry);
            }
        }
    }
}