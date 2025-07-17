using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkinnedMeshRebinder : EditorWindow
{
    [MenuItem("GameObject/SkinnedMesh Tool/Rebinder", false, 0)]
    private static void BakeMeshes(MenuCommand menuCommand)
    {

        if (Selection.objects.Length == 0 || null == Selection.activeGameObject)
        {
            return;
        }

        if (Selection.objects.Length > 1 && menuCommand.context != Selection.objects[0])
            return;


        if (Selection.gameObjects.Length == 2)
        {

            var sourceRendererTr = Selection.gameObjects[0].GetComponent<SkinnedMeshRenderer>() == null ? Selection.gameObjects[1] : Selection.gameObjects[0];
            var targetRoot = Selection.gameObjects[1];
            var sourceRenderer = sourceRendererTr.GetComponent<SkinnedMeshRenderer>();
            if (sourceRenderer == null || targetRoot == null)
            {
                Debug.Log("sourceRenderer 组件 或者 targetRoot对象为 null");
                return;
            }
            Rebind(sourceRenderer, targetRoot.transform);

        }
        else
        {
            Debug.Log("请同时选择 需要替换Bone的SkinnedMeshRenderer组件对象和新的RootBone对象");
        }


    }


    public static void Rebind(SkinnedMeshRenderer sourceRenderer, Transform targetRoot)
    {
        var boneMap = new Dictionary<string, Transform>();
        foreach (var t in targetRoot.GetComponentsInChildren<Transform>())
        {
            boneMap[t.name] = t;
        }

        Transform[] newBones = new Transform[sourceRenderer.bones.Length];
        for (int i = 0; i < newBones.Length; i++)
        {
            string boneName = sourceRenderer.bones[i].name;
            if (!boneMap.TryGetValue(boneName, out newBones[i]))
            {
                Debug.LogError($"目标角色缺少骨骼: {boneName}");
                return;
            }
        }

        SkinnedMeshRenderer newRenderer = Object.Instantiate(sourceRenderer.gameObject).GetComponent<SkinnedMeshRenderer>();
        newRenderer.bones = newBones;
        string rootBoneName = sourceRenderer.rootBone.name;
        if (!boneMap.TryGetValue(rootBoneName, out var newRootBone))
        {
            Debug.LogError($"目标角色缺少 RootBone: {rootBoneName}");
            return;
        }

        newRenderer.rootBone = newRootBone;
        newRenderer.enabled = true;

        Debug.Log("替换新的RootBone成功");
    }
}
