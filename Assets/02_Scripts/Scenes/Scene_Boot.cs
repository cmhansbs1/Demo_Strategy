using UnityEngine;

public class Scene_Boot : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.RunAsync(results =>
        {
            Debug.Log("게임 시작 준비 완료!");

            GameManager.Instance.GetLogManager.Log("Hello");
            GameManager.Instance.GetLogManager.LogWarning("Hello");
            GameManager.Instance.GetLogManager.LogError("Hello");

            var CubeRe = GameManager.Instance.GetRM.LoadRes("Prefabs/Cube");
            Instantiate(CubeRe);

        }).Forget();
    }
}
