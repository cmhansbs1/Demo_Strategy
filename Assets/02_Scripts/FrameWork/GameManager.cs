using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Resources;

/// <summary>
/// 초기화 결과
/// </summary>
public struct InitResult
{
    public string Name;
    public bool Success;
    public string ErrorMessage;
}

/// <summary>
/// GameManager 싱글톤
/// - Instance 접근 시 자동 생성
/// - InitializeAllAsync() 완료 후 콜백 호출
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                var obj = new GameObject(nameof(GameManager));
                _instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private bool Initialized { get; set; } = false;

    //TODO: 매니저 레퍼런스 추가
    private LogManager logManager { get; set; }
    private ResourceManager resourceManager { get; set; }

    public LogManager GetLogManager => logManager;
    public ResourceManager GetRM => resourceManager;

    /// <summary>
    /// RunAsync() - 외부에서 호출용
    /// Instance 접근 + 초기화 + Init 처리
    /// </summary>
    public async UniTaskVoid RunAsync(Action<InitResult[]> onComplete = null)
    {
        var results = await InitializeAllAsync();

        bool hasFailed = false;
        for(int i = 0; i < results.Length; i++)
        {
            if(!results[i].Success)
            {
                Debug.LogError($"초기화 실패: {results[i].Name}");
                hasFailed = true;
                break;
            }
        }

        if(hasFailed == true)
        {
            Debug.LogError($"매니저 초기화 중 오류 발생 > onComplete 실행 불가!!");
            return;
        }

        onComplete?.Invoke(results);
    }

    /// <summary>
    /// InitializeAllAsync 호출 시 완료 콜백 등록 가능
    /// </summary>
    public async UniTask<InitResult[]> InitializeAllAsync()
    {
        if(Initialized)
        {
            Debug.Log(" == 이미 초기화됨 == ");

            return new InitResult[]
            {
                 new InitResult { Name = logManager.Name, Success = true },
                 new InitResult { Name = resourceManager.Name, Success = true }
            };
        }

        Debug.Log("모든 매니저 생성 및 초기화 시작...");

        //TODO: 매니저 생성
        logManager = AddManager<LogManager>();
        resourceManager = AddManager<ResourceManager>();

        var managers = new IManager [] {
            logManager,
            resourceManager,
        };

        // 비동기 초기화 + 결과 수집
        var results = await UniTask.WhenAll(managers.Select(m => RunWithResult(m)));

        // Init() 실행
        foreach(var m in managers)
            m.Init();

        Initialized = true;

        Debug.Log("== 모든 매니저 Init() 완료 ==");

        // 완료 콜백 호출
        return results; 
    }

    private async UniTask<InitResult> RunWithResult(IManager manager)
    {
        try
        {
            bool success = await manager.InitializeAsync();
            // 이름 자동화: IManager.Name이 비어있거나 null이어도 Type으로 이름 기록
            string managerName = string.IsNullOrEmpty(manager.Name)
                ? manager.GetType().Name
                : manager.Name;

            return new InitResult { Name = managerName, Success = success };
        }
        catch(Exception ex)
        {
            string managerName = string.IsNullOrEmpty(manager.Name)
            ? manager.GetType().Name
            : manager.Name;

            return new InitResult { Name = managerName, Success = false, ErrorMessage = ex.Message };
        }
    }

    private T AddManager<T>() where T : Component, IManager
    {
        var obj = new GameObject(typeof(T).Name);
        var manager = obj.AddComponent<T>();
        manager.transform.parent = transform;
      
        return manager;
    }
}
