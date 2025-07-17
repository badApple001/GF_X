using UnityEngine;


public class CharacterEntity : CombatUnitEntity
{
    const string Animator_Node_Path = "Visual/Free Modular Character";
    readonly static string[] After_Combine_DestoryGameObject_Name = {
        "ARMOR PARTS",
        "FACE DETAILS PARTS"
    };

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);

        Transform animatorRoot = CachedTransform.Find(Animator_Node_Path);
        if (animatorRoot != null)
        {
            //优化Bone和Mesh
            RuntimeMeshCombiner.Bake(animatorRoot.gameObject);
            foreach (var destroyObjectName in After_Combine_DestoryGameObject_Name)
            {
                Transform child = animatorRoot.Find(destroyObjectName);
                if (null != child) DestroyImmediate(child.gameObject);
            }
        }
    }


    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        
    }

}
