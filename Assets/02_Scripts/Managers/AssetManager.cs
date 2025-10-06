using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
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

    private Transform _poolRoot; // Pool 폴더 루트

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

        ReInit();
    }

    public void ReInit()
    { //씬 이동하며 모두 Clear하므로 다시 생성..
        // Pool 폴더 자동 생성
        var poolObj = GameObject.Find("Pool");
        if(poolObj == null)
        {
            poolObj = new GameObject("Pool");
            poolObj.transform.position = Vector3.one * 999999.0f;
        }
        _poolRoot = poolObj.transform;

        foreach(AssetType type in System.Enum.GetValues(typeof(AssetType)))
        {
            if(type == AssetType.None) continue;
            pool[type] = new Dictionary<string, Queue<GameObject>>();
        }
    }

    public async UniTask<bool> InitializeAsync()
    {
        // 별도 초기화할 Addressable 그룹이 있다면 여기에 추가
   

        await UniTask.CompletedTask;
        Debug.Log("[AssetManager] InitializeAsync 완료");
        return true;
    }
    #endregion

    #region Create / Release / Pooling
    public void CreateAsync(AssetType type, string addressName, System.Action<GameObject> callback)
    {
        //Addressable 네이밍 규칙: 상위폴더_프리팹이름.확장자(ex:Character_Cube.prefab)
        addressName = $"{type}_{addressName}.prefab";
        CreateInternalAsync(type, addressName, callback).Forget();
    }

    /// <summary>
    /// GameObject 생성 (풀링 기반)
    /// </summary>
    private async UniTaskVoid CreateInternalAsync(AssetType type, string addressName, Action<GameObject> callback)
    {
        GameObject go = null;

        // 풀에서 재사용 가능한 오브젝트 확인
        if(pool[type].TryGetValue(addressName, out var queue) && queue.Count > 0)
        {
            go = queue.Dequeue();
            go.SetActive(true);
            go.transform.SetParent(null); // 씬 루트로 복귀
        }
        else
        {
            // ResourceManager에서 로드 후 Instantiate
            var prefab = await LoadAsync<GameObject>(addressName);
            if(prefab != null)
            {
                go = Instantiate(prefab);
            }
            else
            {
                Debug.LogError($"[AssetManager] 로드 실패: {addressName}");
            }
        }

        // 콜백 호출
        callback?.Invoke(go);
    }

    /// <summary>
    /// 반환 (풀링)
    /// </summary>
    public void Release(AssetType type, string addressName, GameObject obj)
    {
        obj.SetActive(false);

        // addressName 하위 폴더(Transform) 확보
        var poolParent = GetOrCreatePoolParent(type, addressName);
        obj.transform.SetParent(poolParent);

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
    public void ClearPoolAll()
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

        // Pool 폴더 하위 정리
        if(_poolRoot != null)
        {
            for(int i = _poolRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_poolRoot.GetChild(i).gameObject);
            }
        }

        Debug.Log("[AssetManager] 모든 Pooling Clear 완료");
    }
    #endregion

    #region Preload 기능

    /// <summary>
    /// Addressable 프리팹 미리 로드 및 풀링
    /// </summary>
    public async UniTask PreloadAsync(AssetType type, string addressName, int count)
    {
        if(count <= 0) return;

        addressName = $"{type}_{addressName}.prefab";

        var prefab = await LoadAsync<GameObject>(addressName);
        if(prefab == null)
        {
            Debug.LogError($"[AssetManager] Preload 실패: {addressName}");
            return;
        }

        var poolParent = GetOrCreatePoolParent(type, addressName);

        if(!pool[type].TryGetValue(addressName, out var queue))
        {
            queue = new Queue<GameObject>();
            pool[type][addressName] = queue;
        }

        for(int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, poolParent);
            go.SetActive(false);
            queue.Enqueue(go);
        }

        Debug.Log($"[AssetManager] Preload 완료: {addressName} ({count}개)");
    }

    #endregion

    #region Private

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

    /// <summary>
    /// Pool 폴더 구조 생성
    /// ex) Pool/Character_Enemy.prefab/
    /// </summary>
    private Transform GetOrCreatePoolParent(AssetType type, string addressName)
    {
        string folderName = $"{Path.GetFileNameWithoutExtension(addressName)}";
        var child = _poolRoot.Find(folderName);
        if(child == null)
        {
            var newFolder = new GameObject(folderName);
            newFolder.transform.SetParent(_poolRoot);
            child = newFolder.transform;
        }
        return child;
    }

    #endregion
}
