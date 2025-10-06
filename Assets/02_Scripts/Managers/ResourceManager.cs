using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : MonoBehaviour, IManager
{
    public string Name => "ResourceManager";

    // 리소스 캐싱
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    public void Init()
    {
        Debug.Log("[ResourceManager] Init 완료");
    }

    public async UniTask<bool> InitializeAsync()
    {
        //아무런 작업 없이 완료 처리
        await UniTask.CompletedTask;

        Debug.Log($"[ResourceManager] InitializeAsync 완료");

        return true;
    }

    public GameObject LoadRes(string path)
    {
        GameObject prefabRes = Resources.Load<GameObject>(path);
        return prefabRes;
    }

    // 캐싱 포함 Addressable 로드
    public async UniTask<T> LoadAsync<T>(string addressName) where T : UnityEngine.Object
    {
        // 이미 캐시된 경우
        if(_handles.TryGetValue(addressName, out var handle) && handle.IsValid())
        {
            if(handle.Result is T cachedAsset)
            {
                Debug.Log($"[LoadAsync] Cache hit: {addressName}");
                return cachedAsset;
            }
            else
            {
                Debug.LogWarning($"[ResourceManager] 캐시된 타입 불일치로 재로드: {addressName}");
                Addressables.Release(handle);
                _handles.Remove(addressName);
            }
        }

        // 새로 로드
        Debug.Log($"[LoadAsync] Start Loading: {addressName}");

        try
        {
            var newHandle = Addressables.LoadAssetAsync<T>(addressName);
            await newHandle.Task;

            if(newHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _handles[addressName] = newHandle;
                Debug.Log($"[LoadAsync] Successfully Loaded: {addressName}");
                return newHandle.Result;
            }
            else
            {
                Debug.LogError($"[LoadAsync] Failed to load: {addressName}");
                return null;
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"[LoadAsync] Exception while loading {addressName}\n{e}");
            return null;
        }
    }

    // 리소스 해제
    public void Release(string addressName)
    {
        if(string.IsNullOrEmpty(addressName))
        {
            Debug.LogWarning("[ResourceManager] Release 실패 - address가 비어있음");
            return;
        }

        if(_handles.TryGetValue(addressName, out var handle))
        {
            if(handle.IsValid())
            {
                Addressables.Release(handle);
                Debug.Log($"[ResourceManager] Released: {addressName}");
            }
            _handles.Remove(addressName);
        }
        else
        {
            Debug.LogWarning($"[ResourceManager] Release 실패 - 해당 주소 미존재: {addressName}");
        }
    }

    // 전체 해제
    public void ReleaseAll()
    {
        foreach(var kv in _handles)
        {
            Addressables.Release(kv.Value);
        }
        _handles.Clear();

        Debug.Log("[ResourceManager] 모든 리소스 캐싱 Clear 완료");
    }
}
