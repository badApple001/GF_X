using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using System.Linq;

public class GeneratePrefabCombinations : EditorWindow
{
    // 要处理的预制体
    public GameObject root;
    // 保存路径
    public string savePath = "Assets/AAAGame/Prefabs/Entity/Characters/";
    // 部位和对应的预设列表
    [System.Serializable]
    public class RootBone
    {
        [ReadOnly]
        public string Name;
        public GameObject Root;
        public List<Part> parts;
    }

    [System.Serializable]
    public class Part
    {
        [ReadOnly]
        public string Name;
        public List<GameObject> Skins;
    }


    public List<RootBone> rootBone = new List<RootBone>();

    [MenuItem("Tools/Prefab Tool/Generate Prefab Combinations")]
    public static void ShowWindow()
    {
        GetWindow<GeneratePrefabCombinations>("Generate Prefab Combinations");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Prefab Combinations", EditorStyles.boldLabel);

        // 选择源预制体
        root = (GameObject)EditorGUILayout.ObjectField("Root GameObject", root, typeof(GameObject), true);

        // 序列化部位和预设列表
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty partsProperty = serializedObject.FindProperty("rootBone");
        EditorGUILayout.PropertyField(partsProperty, true);
        serializedObject.ApplyModifiedProperties();

        if (rootBone.Count > 0)
        {
            foreach (var bone in rootBone)
            {
                if (bone.Root != null)
                {
                    bone.Name = GetRelativePath(bone.Root.transform, root.transform);
                    if (bone.parts == null || bone.parts.Count == 0)
                    {
                        bone.parts = new List<Part>();
                        for (int i = 0; i < bone.Root.transform.childCount; i++)
                        {
                            var child = bone.Root.transform.GetChild(i);
                            var skins = new List<GameObject>();
                            for (int j = 0; j < child.childCount; j++)
                            {
                                skins.Add(child.GetChild(j).gameObject);
                            }
                            bone.parts.Add(new Part()
                            {
                                Name = GetRelativePath(child, root.transform),
                                Skins = skins
                            });
                        }
                    }
                }
            }
        }


        // 保存路径
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Generate Combinations"))
        {
            if (root == null || rootBone.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", "Please assign a source prefab and at least one part with prefabs.", "OK");
                return;
            }

            GenerateAllCombinations();
        }
    }

    /// <summary>
    /// 获取当前节点相对于指定父节点的相对路径
    /// </summary>
    /// <param name="current">当前节点</param>
    /// <param name="parent">目标父节点</param>
    /// <returns>相对路径字符串，例如 "Player/Visual/Bone"，如果不是父节点的后代则返回空字符串</returns>
    public static string GetRelativePath(Transform current, Transform parent)
    {
        if (current == null || parent == null)
        {
            Debug.LogWarning("Current or parent Transform is null.");
            return string.Empty;
        }

        // 检查当前节点是否是父节点的后代
        if (!IsChildOf(current, parent))
        {
            Debug.LogWarning($"{current.name} is not a child of {parent.name}.");
            return string.Empty;
        }

        // 构建路径
        string path = current.name;
        Transform temp = current;
        while (temp != parent && temp.parent != parent)
        {
            temp = temp.parent;
            path = $"{temp.name}/{path}";
        }
        return path;
    }

    /// <summary>
    /// 检查当前节点是否是父节点的后代
    /// </summary>
    private static bool IsChildOf(Transform child, Transform parent)
    {
        Transform current = child;
        while (current != null)
        {
            if (current == parent)
                return true;
            current = current.parent;
        }
        return false;
    }

    private void GenerateAllCombinations()
    {
        // 确保保存路径存在
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        EditorUtility.DisplayProgressBar("Generating Prefabs", $"Generating prefabs....", 0);


        //找到最小值
        int skinsCount = 0;
        for (int i = 0; i < rootBone.Count; i++)
        {
            for (int j = 0; j < rootBone[i].parts.Count; j++)
            {
                int currentSkinsCount = rootBone[i].parts[j].Skins.Count;
                if (currentSkinsCount > skinsCount)
                    skinsCount = currentSkinsCount;
            }
        }

        for (int x = 0; x < skinsCount; x++)
        {
            GameObject newRoot = Instantiate(root);
            newRoot.name = $"Character_{x}";
            for (int i = 0; i < rootBone.Count; i++)
            {
                var bone = rootBone[i];
                for (int j = 0; j < bone.parts.Count; j++)
                {
                    var part = bone.parts[j];
                    var partTr = newRoot.transform.Find(part.Name);
                    Debug.Assert(partTr != null, $"找不到part: {part.Name}");
                    var skins = partTr.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList();
                    // string activeSkinName = part.Skins.Count > x ? part.Skins[x].name : part.Skins[Random.Range(0, part.Skins.Count)].name;
                    // skins.ForEach(skin =>
                    // {
                    //     if (skin.name != activeSkinName) DestroyImmediate(skin.gameObject);
                    // });

                    //这个方式更安全，避免资源命名错误
                    int activeIndex = part.Skins.Count > x ? x : Random.Range(0, part.Skins.Count);
                    for (int k = 0; k < skins.Count; k++)
                    {
                        if (activeIndex != k) DestroyImmediate(skins[k].gameObject);
                        else skins[k].gameObject.SetActive(true);
                    }
                }
            }


            // 保存为预制体
            string prefabPath = $"{savePath}{newRoot.name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(newRoot, prefabPath);


            // 更新进度条
            EditorUtility.DisplayProgressBar("Generating Prefabs", $"Generating {newRoot.name}", (float)x / skinsCount);

            // 销毁临时对象
            DestroyImmediate(newRoot);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Generated {skinsCount} prefabs at {savePath}", "OK");
    }


}