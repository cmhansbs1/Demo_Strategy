using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.IO;

public class LogManager : MonoBehaviour, IManager
{
    public string Name => "LogManager";

    // 로그 저장 여부
    public bool EnableFileLog { get; private set; } = true;

    // 기존 로그 삭제 여부
    public bool DeleteOldLog { get; set; } = true;

    // 로그 파일 경로
    private string _logFilePath;

    public async UniTask<bool> InitializeAsync()
    {
        try
        {
            string basePath = Application.persistentDataPath;
            string fileName = "game_log.txt";

            if(EnableFileLog)
            {
                // 기존 파일 삭제 여부
                if(File.Exists(Path.Combine(basePath, fileName)))
                {
                    if(DeleteOldLog)
                    {
                        File.Delete(Path.Combine(basePath, fileName));
                    }
                    else
                    {
                        // 파일 이름에 날짜/시간 추가
                        string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        fileName = $"game_log_{timeStamp}.txt";
                    }
                }

                _logFilePath = Path.Combine(basePath, fileName);

                // 실제 파일 생성/접근 테스트
                try
                {
                    await UniTask.Run(() => File.WriteAllText(_logFilePath, "")); 
                    await UniTask.Run(() => File.ReadAllText(_logFilePath));
                }
                catch(Exception ex)
                {
                    Debug.LogError($"[LogManager] 파일 생성/읽기 실패: {ex.Message}");
                    return false;
                }
            }

            Debug.Log($"[LogManager] Initialize 완료. 로그 파일: {_logFilePath}");
            return true;
        }
        catch(Exception ex)
        {
            Debug.LogError($"[LogManager] Initialize 실패: {ex.Message}");
            return false;
        }
    }

    public void Init()
    {
        Debug.Log("[LogManager] Init 완료");
    }

    public void Log(string message)
    {
        string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        Debug.Log(logMessage);

        if(EnableFileLog)
        {
            try
            {
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
            catch(Exception ex)
            {
                Debug.LogError($"[LogManager] 파일 저장 실패: {ex.Message}");
            }
        }
    }

    public void LogWarning(string message)
    {
        string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][Warning] {message}";
        Debug.LogWarning(logMessage);

        if(EnableFileLog)
        {
            try
            {
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
            catch(Exception ex)
            {
                Debug.LogError($"[LogManager] 파일 저장 실패: {ex.Message}");
            }
        }
    }

    public void LogError(string message)
    {
        string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][Error] {message}";
        Debug.LogError(logMessage);

        if(EnableFileLog)
        {
            try
            {
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
            catch(Exception ex)
            {
                Debug.LogError($"[LogManager] 파일 저장 실패: {ex.Message}");
            }
        }
    }
}
