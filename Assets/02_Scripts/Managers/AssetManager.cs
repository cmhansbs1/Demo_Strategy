using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum AssetType
{
    None,
    Character,
    Effect,
    UI,
    Environment,
    Data,       // ScriptableObject 등 데이터용
    Sound,      // AudioClip
}

/// <summary>
/// AssetManager는 단순히 “생성/풀링/타입 관리”만
/// ResourceManager가 이미 로드/캐싱을 담당
/// ResourceManager에서 로드한 GameObject만 생성/풀링 관리
/// ScriptableObject, Texture, AudioClip 등은 ResourceManager에서 로드
/// ex: var itemData = await resourceManager.LoadAddressRes<ScriptableObject>("Data/ItemTable");
/// </summary>
public class AssetManager : MonoBehaviour, IManager
{
    private readonly Dictionary<AssetType, Dictionary<string, Queue<GameObject>>> pool =
        new Dictionary<AssetType, Dictionary<string, Queue<GameObject>>>();

    [SerializeField] private ResourceManager _resourceManager;

    public string Name => "AssetManager";

    #region Init / Lifecycle
    public void Init()
    {
        if(GameManager.Instance)
        {
            _resourceManager = GameManager.GetRM;
        }

        if(_resourceManager == null)
        {
            Debug.LogError("AssetManager Init ResourceManager 참조 실패!");
            return;
        }
    }

    public async UniTask<bool> InitializeAsync()
    {
        // 별도 초기화할 Addressable 그룹이 있다면 여기에 추가
        foreach(AssetType type in System.Enum.GetValues(typeof(AssetType)))
        {
            if(type == AssetType.None) continue;
            pool[type] = new Dictionary<string, Queue<GameObject>>();
        }

        await UniTask.CompletedTask;
        Debug.Log("[AssetManager] InitializeAsync 완료");
        return true;
    }
    #endregion

    #region Create / Release / Pooling
    public void CreateAsync(AssetType type, string addressName, System.Action<GameObject> callback)
    {
        addressName = $"{type}_{addressName}";
        CreateInternalAsync(type, addressName, callback).Forget();
    }

    /// <summary>
    /// GameObject 생성 (풀링 기반)
    /// </summary>
    private async UniTaskVoid CreateInternalAsync(AssetType type, string addressName, Action<GameObject> callback)
    {
        GameObject instance = null;

        // 풀에서 재사용 가능한 오브젝트 확인
        if(pool[type].TryGetValue(addressName, out var queue) && queue.Count > 0)
        {
            instance = queue.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            // ResourceManager에서 로드 후 Instantiate
            var prefab = await LoadAsync<GameObject>(addressName);
            if(prefab != null)
            {
                instance = Instantiate(prefab);
            }
            else
            {
                Debug.LogError($"[AssetManager] 로드 실패: {addressName}");
            }
        }

        // 콜백 호출
        callback?.Invoke(instance);
    }

    /// <summary>
    /// 반환 (풀링)
    /// </summary>
    public void Release(AssetType type, string addressName, GameObject obj)
    {
        obj.SetActive(false);

        if(!pool[type].TryGetValue(addressName, out var queue))
        {
            queue = new Queue<GameObject>();
            pool[type][addressName] = queue;
        }

        queue.Enqueue(obj);
    }

    /// <summary>
    /// 풀링된 모든 오브젝트 제거 (메모리 해제)
    /// </summary>
    public void ClearPool()
    {
        foreach(var typeDict in pool.Values)
        {
            foreach(var q in typeDict.Values)
            {
                while(q.Count > 0)
                {
                    var obj = q.Dequeue();
                    if(obj != null)
                        Destroy(obj); // 필요 시 ResourceManager.Release 호출 가능
                }
            }
        }

        Debug.Log("[AssetManager] 풀 초기화 완료");
    }
    #endregion

    #region Private Load (ResourceManager 사용)

    //ResourceManager 통해 Addressables 로드
    private async UniTask<T> LoadAsync<T>(string addressName) where T : UnityEngine.Object
    {
        var asset = await _resourceManager.LoadAsync<T>(addressName);
        if(asset == null)
        {
            Debug.LogError($"[AssetManager] LoadAsync 실패: {addressName}");
        }
        return asset;
    }

    #endregion
}
