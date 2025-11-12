using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using ET;

[EnableClass]
public class OpenPersistentDataPath : MonoBehaviour
{
    [MenuItem("Tools/打开持久化数据路径")]
    public static void OpenPersistentDataPathFolder()
    {
        // 获取持久化数据路径
        string persistentDataPath = UnityEngine.Application.persistentDataPath;
        
        // 在文件资源管理器中打开该路径
        Process.Start(persistentDataPath);
        
        // 在控制台输出路径，方便调试
        UnityEngine.Debug.Log($"已打开持久化数据路径: {persistentDataPath}");
    }
}