using UnityEngine;

public class Scene_Boot : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.RunAsync(results =>
        {
            foreach(var r in results)
            {
                if(r.Success)
                    Debug.Log($"{r.Name} 초기화 완료");
                else
                    Debug.LogError($"❌ {r.Name} 초기화 실패: {r.ErrorMessage}");
            }
        }).Forget();

        Debug.Log("게임 시작 준비 완료!");

        GameManager.Instance.GetLogManager.Log("Hello");
        GameManager.Instance.GetLogManager.LogWarning("Hello");
        GameManager.Instance.GetLogManager.LogError("Hello");

        //GM.GetLogManager.Log("Hello");
    }
}
