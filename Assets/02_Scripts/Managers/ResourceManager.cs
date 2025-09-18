using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResourceManager : MonoBehaviour, IManager
{
    public string Name => "ResourceManager";

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
}
