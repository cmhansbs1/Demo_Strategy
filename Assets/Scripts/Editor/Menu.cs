using UnityEditor;

public class Menu : EditorWindow
{
    private const string ROOT_NAME = "Tools/";
    private const string text = ROOT_NAME + "Game Log/Open Window";

    [MenuItem(text)]
    public static void ShowWindow()
    {
        GetWindow<OpenGameLog>("Game Log");
    }
}
