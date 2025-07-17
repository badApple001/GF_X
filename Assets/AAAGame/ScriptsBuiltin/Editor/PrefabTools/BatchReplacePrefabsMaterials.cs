using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BatchReplacePrefabsMaterials : EditorWindow
{

    public List<GameObject> prefabs = new List<GameObject>();
    Material newMaterial;



    #region 滑动列表
    private Vector2 m_ScrollPosition;
    private List<Renderer> renderers = new List<Renderer>();
    private int selectedIndex = -1;
    #endregion


    [MenuItem("Tools/Prefab Tool/批量替换预制体中的材质")]
    public static void ShowWindow()
    {
        GetWindow<BatchReplacePrefabsMaterials>("材质替换窗口");
    }

    void OnGUI()
    {
        GUILayout.Label("拖入带网格预制体", EditorStyles.boldLabel);

        // 序列化部位和预设列表
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        EditorGUILayout.PropertyField(prefabsProperty, true);
        serializedObject.ApplyModifiedProperties();

        if (prefabs.Count > 0)
        {
            GUILayout.Label("需要替换的材质", EditorStyles.boldLabel);
            newMaterial = (Material)EditorGUILayout.ObjectField("New Material", newMaterial, typeof(Material), true);

            //新的材质
            if (newMaterial != null)
            {
                //
                if (GUILayout.Button("更新需要替换的材质器"))
                {
                    renderers.Clear();
                    prefabs.ForEach(prefab => renderers.AddRange(prefab.GetComponentsInChildren<Renderer>()));
                }

                if (renderers.Count > 0)
                {
                    EditorGUILayout.LabelField("待替换的材质器列表", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.Space();

                    // 开始滚动区域
                    m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.Height(200));
                    for (int i = 0; i < renderers.Count; i++)
                    {
                        GUIStyle style = (i == selectedIndex) ? EditorStyles.helpBox : EditorStyles.label;

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(renderers[i].name, style))
                        {
                            selectedIndex = i;
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    // 结束滚动区域
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                }


                if (GUILayout.Button("批量替换"))
                {

                    if (!EditorUtility.DisplayDialog("确认替换", $"是否替换 {prefabs.Count} 个 Prefab 的材质？", "确认", "取消"))
                    {
                        return;
                    }

                    foreach (var prefab in prefabs)
                    {

                        string path = AssetDatabase.GetAssetPath(prefab);
                        if (string.IsNullOrEmpty(path))
                        {
                            Debug.LogWarning($"Prefab '{prefab.name}' 路径无效，已跳过。");
                            continue;
                        }

                        GameObject root = PrefabUtility.LoadPrefabContents(path);

                        //移除所有丢失脚本
                        int totalMissing = 0;
                        foreach (var go in root.GetComponentsInChildren<Transform>(true))
                        {
                            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go.gameObject);
                            if (count > 0)
                            {
                                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go.gameObject);
                                totalMissing += count;
                            }
                        }

                        Debug.Log($"[{prefab.name}] 移除缺失脚本: {totalMissing} 个");

                        //替换材质
                        Renderer[] prefabRenderers = root.GetComponentsInChildren<Renderer>();
                        foreach (var r in prefabRenderers)
                        {
                            r.sharedMaterial = newMaterial;
                        }
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        PrefabUtility.UnloadPrefabContents(root);
                    }

                    AssetDatabase.Refresh();
                }
            }
        }


    }

}
