using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneTest2 : MonoBehaviour
{
    async void Start()
    {
        //“RunAsync가 끝날 때까지 기다린다.”
        await GameManager.Instance.RunAsync();

        GameManager.GetLogManager.Log("SceneTest2 시작");

        await GameManager.GetAM.PreloadAsync(AssetType.Character, "Cube", 10);

        for(int i = 0; i < 10; i++)
        {
            GameManager.GetAM.CreateAsync(AssetType.Character, "Cube", (go) =>
            {
                go.transform.position = new Vector3(i * 2 - 10, .0f, .0f);
                Debug.Log("Cube 생성 완료");
            });

            await UniTask.Delay(1000);
        }
    }

}
