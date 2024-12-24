using UnityEditor;
using UnityEngine;

public class AssetFinder
{
    [MenuItem("Tools/Find Asset by Key")]
    public static void FindAssetByKey()
    {
        string keyToSearch = "���key";  // �����滻Ϊʵ�ʵ�key

        // ����������Դ·��
        string[] guids = AssetDatabase.FindAssets("");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset != null && asset.name == keyToSearch)
            {
                Debug.Log($"Found asset: {asset.name} at path: {path}");
                EditorGUIUtility.PingObject(asset); // ����Դ��������ѡ�ж���
                break;
            }
        }
    }
}
