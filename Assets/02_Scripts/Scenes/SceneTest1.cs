using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneTest1 : MonoBehaviour
{
    async void Start()
    {
        //“RunAsync가 끝날 때까지 기다린다.”
        await GameManager.Instance.RunAsync();

        GameManager.GetLogManager.Log("SceneTest1 시작");

        await GameManager.GetAM.PreloadAsync(AssetType.Character, "Cube", 2);
        await GameManager.GetAM.PreloadAsync(AssetType.Character, "Cylinder", 2);

        await UniTask.Delay(2000);

        GameManager.GetAM.CreateAsync(AssetType.Character, "Cube", (go) =>
        {
            Debug.Log("Cube 생성 완료");
        });

        await UniTask.Delay(2000);

        GameManager.GetAM.CreateAsync(AssetType.Character, "Cylinder", (go) =>
        {
            go.transform.position = new Vector3(8, 0, 0);
            Debug.Log("Cylinder 생성 완료");
        });

        await UniTask.Delay(5000);

        GameManager.GetSM.ChangeSceneAsync("SceneTest2", () =>
            {
                Debug.Log("씬 전환 후 처리 가능!!");
            }).Forget();
    }

}
