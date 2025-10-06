using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour, IManager
{
    public string Name => "SceneManager";

    private string _currentScene;   // 현재 씬 이름
    private string _nextScene;      // 다음 씬 이름
    private bool _isLoading = false;

    public void Init()
    {
        Debug.Log("[SceneManager] Init 완료");
    }

    public async UniTask<bool> InitializeAsync()
    {
        //##[SceneManager] 별로 초기화가 필요하면 여기에..

        await UniTask.CompletedTask;

        Debug.Log($"[ResourceManager] InitializeAsync 완료");

        return true;
    }

    /// <summary>
    /// 씬 이동 요청
    /// </summary>
    public async UniTaskVoid ChangeSceneAsync(string nextScene, Action onCompleted = null)
    {
        if(_isLoading)
        {
            Debug.LogWarning("[SceneManager] 씬 전환 중입니다.");
            return;
        }

        _isLoading = true;
        _nextScene = nextScene;

        Debug.Log($"[SceneManager] 씬 이동 요청: {_currentScene} → {_nextScene}");

        // 현재 씬 리소스 해제
        await CleanupCurrentSceneAsync();

        // EmptyScene 로드
        await LoadSceneByAddressAsync("EmptyScene");

        // 3️⃣ 다음 씬 Addressables로 로드
        await LoadSceneByAddressAsync(_nextScene);

        _currentScene = _nextScene;
        _isLoading = false;

        onCompleted?.Invoke();

        Debug.Log($"[SceneManager] 씬 전환 완료: {_currentScene}");
    }

    /// <summary>
    /// 현재 씬에서 사용한 리소스 초기화
    /// </summary>
    private async UniTask CleanupCurrentSceneAsync()
    {
        Debug.Log($"[SceneManager] 현재 씬({_currentScene}) 리소스 해제 중...");

        //TODO: ##[SceneManager]씬 이동에서 해제해야 하는 것들은 여기에..
        try
        {
            GameManager.GetRM.ReleaseAll();
            GameManager.GetAM.ClearPoolAll();
        }
        catch(Exception e)
        {
            Debug.LogError($"[SceneManager] CleanupCurrentSceneAsync 에러: {e}");
        }

        // GC / 메모리 정리
        await Resources.UnloadUnusedAssets();
        System.GC.Collect();
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        await UniTask.Yield();
    }

    /// <summary>
    /// Addressables 씬 로드
    /// </summary>
    private async UniTask LoadSceneByAddressAsync(string sceneAddressName)
    {
        sceneAddressName = $"{sceneAddressName}.unity";
        Debug.Log($"[SceneManager] Addressables에서 씬 로드 시작: {sceneAddressName}");

        var handle = Addressables.LoadSceneAsync(sceneAddressName, LoadSceneMode.Single);
        await handle.Task;

        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"[SceneManager] 씬 로드 완료: {sceneAddressName}");
        }
        else
        {
            Debug.LogError($"[SceneManager] 씬 로드 실패: {sceneAddressName}");
        }
    }

}
