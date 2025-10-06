using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Scene_Boot : MonoBehaviour
{
    public async void Start()
    {
        //“RunAsync가 끝날 때까지 기다린다.”
        await GameManager.Instance.RunAsync();

        Debug.Log("게임 시작 준비 완료!");

        GameManager.GetLogManager.Log("Hello");
        GameManager.GetLogManager.LogWarning("Hello");
        GameManager.GetLogManager.LogError("Hello");

        await GameManager.GetAM.PreloadAsync(AssetType.Character, "Cube", 5);
        await GameManager.GetAM.PreloadAsync(AssetType.Character, "Cylinder", 5);

        GameManager.GetAM.CreateAsync(AssetType.Character, "Cube", (go) =>
        {
            go.transform.position = Vector3.zero;
            Debug.Log("Cube 생성 완료");
        });

        GameManager.GetAM.CreateAsync(AssetType.Character, "Cylinder", (go) =>
        {
            go.transform.position = new Vector3(30, 0, 0);
            Debug.Log("Cylinder 생성 완료");
        });

        //“RunAsync를 실행하되, 기다리지 않고 그냥 흘려보낸다.”
        /*        GameManager.Instance.RunAsync(results => {
                    Debug.Log("게임 시작 준비 완료!");

                    GameManager.GetLogManager.Log("Hello");
                    GameManager.GetLogManager.LogWarning("Hello");
                    GameManager.GetLogManager.LogError("Hello");

                    GameManager.GetAM.CreateAsync(AssetType.Character, "Cube", (go) =>
                    {
                        go.transform.position = Vector3.zero;
                        Debug.Log("Cube 생성 완료");
                    });

                }).Forget();*/
    }
}
