using UnityEngine;
//https://github.com/Cysharp/UniTask/releases
//https://github.com/Cysharp/UniTask.git
using Cysharp.Threading.Tasks;

public interface IManager
{
    string Name { get; }
    UniTask<bool> InitializeAsync();
    void Init();
}
