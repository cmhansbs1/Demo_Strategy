using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class Scene_Boot : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.RunAsync(results => {
            Debug.Log("게임 시작 준비 완료!");

            GameManager.GetLogManager.Log("Hello");
            GameManager.GetLogManager.LogWarning("Hello");
            GameManager.GetLogManager.LogError("Hello");
            /*
                        var CubeRe = GameManager.Instance.GetRM.LoadRes("Prefabs/Cube");
                        Instantiate(CubeRe);*/

            //    GameManager.Instance.GetRM.CreateGameObject("Boot_Cube").Forget();

            GameManager.GetAM.CreateAsync(AssetType.Character, "Cube", (go) =>
            {
                go.transform.position = Vector3.zero;
                Debug.Log("Cube 생성 완료");
            });

        }).Forget();
    }
}
