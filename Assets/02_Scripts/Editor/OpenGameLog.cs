using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class OpenGameLog : EditorWindow
{
    private string logPath;

    private void OnEnable()
    {
        // Unity에서 자동으로 정해주는 로그 저장 경로
        string basePath = Application.persistentDataPath;
        logPath = Path.Combine(basePath, "game_log.txt");
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Log Path", EditorStyles.boldLabel);
        EditorGUILayout.TextField("Log Path", logPath);

        if(GUILayout.Button("Open Log File")) OpenLogFile();
        if(GUILayout.Button("Reveal in Explorer/Finder")) RevealInOS();
    }

    private void OpenLogFile()
    {
        if(File.Exists(logPath))
        {
            var psi = new ProcessStartInfo(logPath) { UseShellExecute = true };
            Process.Start(psi);
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", $"File not found:\n{logPath}", "OK");
        }
    }

    private void RevealInOS()
    {
        if(File.Exists(logPath))
        {
            EditorUtility.RevealInFinder(logPath);
        }
        else
        {
            string folder = Path.GetDirectoryName(logPath);
            if(Directory.Exists(folder))
                EditorUtility.RevealInFinder(folder);
            else
                EditorUtility.DisplayDialog("Not Found", $"Folder not found:\n{folder}", "OK");
        }
    }
}
