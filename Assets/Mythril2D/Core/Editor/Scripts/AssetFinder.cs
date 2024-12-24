using UnityEditor;
using UnityEngine;

public class AssetFinder
{
    [MenuItem("Tools/Find Asset by Key")]
    public static void FindAssetByKey()
    {
        string keyToSearch = "你的key";  // 将此替换为实际的key

        // 查找所有资源路径
        string[] guids = AssetDatabase.FindAssets("");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset != null && asset.name == keyToSearch)
            {
                Debug.Log($"Found asset: {asset.name} at path: {path}");
                EditorGUIUtility.PingObject(asset); // 在资源管理器中选中对象
                break;
            }
        }
    }
}
