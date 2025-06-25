#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace UGF.EditorTools
{
    public class UIScriptAutoGenerator : AssetPostprocessor
    {
        public readonly static string s_NamespaceForGenerateCls = "Game";  //可以配置生成类的命名空间
        public const string KEY_GENERATE_CLASS = "KEY_GENERATE_CLASS";
        public const string KEY_GENERATE_PREFAB = "KEY_GENERATE_PREFAB";
        delegate string WriteScriptHandler( string formClsName );


        public static void OnPostprocessAllAssets( string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
        {
            List<string> movedAssetsList = new List<string>( movedAssets );
            foreach ( string url in importedAsset )
            {
                if ( url.StartsWith( "Assets/AAAGame/Prefabs/UI" ) && url.EndsWith( ".prefab" ) && !movedAssetsList.Contains( url ) )
                {
                    string prefabName = Path.GetFileNameWithoutExtension( url );
                    string relativeAddress = url.Replace( "Assets/AAAGame/Prefabs/UI/", "" );
                    string scriptAddress = Path.Combine( "Assets/AAAGame/Scripts/UI", Path.GetFileNameWithoutExtension( relativeAddress ) ) + ".cs";

#if UNITY_2020_1_OR_NEWER
                    if ( !Directory.Exists( scriptAddress ) )
#endif
                    {
                        if ( relativeAddress.Contains( "Items/" ) )
                        {
                            NewItem( url, scriptAddress );
                        }
                        else if ( prefabName.EndsWith( "Form" ) )
                        {
                            NewForm( url, scriptAddress );
                        }
                        else if ( prefabName.EndsWith( "Dialog" ) )
                        {
                            NewDialog( url, scriptAddress );
                        }
                        else
                        {
                            Debug.LogError( $"Prefab {prefabName} not end with Form or Dialog" );
                        }
                    }
                }
            }
        }


        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptUpdateLoaded( )
        {
            string className = EditorPrefs.GetString( KEY_GENERATE_CLASS );
            if ( string.IsNullOrEmpty( className ) )
            {
                return;
            }

            EditorUtility.DisplayProgressBar( "Mount scripts", $"{className} install...", 0f );

            var assemblies = AppDomain.CurrentDomain.GetAssemblies( );
            var defaultAssembly = assemblies.First( assembly => assembly.GetName( ).Name == "Assembly-CSharp" );
            var typeName = string.IsNullOrEmpty( s_NamespaceForGenerateCls ) ? className : s_NamespaceForGenerateCls + "." + className;
            var type = defaultAssembly.GetType( typeName );

            //尝试用Hotfix
            if ( type == null )
            {
                defaultAssembly = assemblies.First( assembly => assembly.GetName( ).Name == "Hotfix" );
                typeName = string.IsNullOrEmpty( s_NamespaceForGenerateCls ) ? className : s_NamespaceForGenerateCls + "." + className;
                type = defaultAssembly.GetType( typeName );
                Debug.Log( type );
            }

            if ( type == null )
            {
                Debug.Log( $"编译失败: {className}" );
                EditorUtility.ClearProgressBar( );
#if UNITY_2021_1_OR_NEWER
                EditorPrefs.DeleteKey( KEY_GENERATE_CLASS );
#endif
                return;
            }

            string prefabPath = EditorPrefs.GetString( KEY_GENERATE_PREFAB );

            //为开发者提供额外处理接口
            try
            {
                GameObject prefabInstance = PrefabUtility.LoadPrefabContents( prefabPath );

                EditorUtility.DisplayProgressBar( "Mount scripts", $"{className} install...", 0.618f );

                var scriptComponent = prefabInstance.GetComponent( type );

                if ( !scriptComponent )
                {
                    scriptComponent = prefabInstance.AddComponent( type );
                }

                AdditionalConfigurationForNewForm( prefabInstance );

                EditorUtility.DisplayProgressBar( "Mount scripts", $"{className} install...", 1f );

                PrefabUtility.SaveAsPrefabAssetAndConnect( prefabInstance, prefabPath, InteractionMode.AutomatedAction );
            }
            catch ( Exception e )
            {
                Debug.LogError( e );
            }

            AssetDatabase.Refresh( );

            EditorUtility.ClearProgressBar( );

            EditorPrefs.DeleteKey( KEY_GENERATE_CLASS );
        }

        /// <summary>
        /// 提供给开发者额外操作的接口
        /// </summary>
        /// <param name="gameobject"> 你可以像运行时对GameObject做的任何操作 </param>
        private static void AdditionalConfigurationForNewForm( GameObject gameobject )
        {

            //    if ( !gameobject.TryGetComponent( out Animation animation ) )
            //    {
            //        animation = gameobject.AddComponent<Animation>( );
            //    }

            //    var closeClip = AssetDatabase.LoadAssetAtPath<AnimationClip>( "Assets/AssetBundle/UI/Animations/CloseUIForm.anim" );
            //    var openClip = AssetDatabase.LoadAssetAtPath<AnimationClip>( "Assets/AssetBundle/UI/Animations/OpenUIForm.anim" );

            //    AnimationUtility.SetAnimationClips( animation, new AnimationClip[] {
            //    openClip,
            //    closeClip,
            //} );

            //    if ( gameobject.TryGetComponent( out Animation component ) )
            //    {
            //        component.clip = openClip;
            //    }
        }

        private static void CheckPathValid( string path )
        {
            if ( string.IsNullOrWhiteSpace( path ) || string.IsNullOrEmpty( path ) )
            {
                throw new UnityException( $"传入路径为空或者空格: {path}" );
            }
        }

        private static void CreateScript( string assetPath, string scriptAddress, WriteScriptHandler writeScriptHandler )
        {
            CheckPathValid( assetPath );
            var formClsName = Path.GetFileNameWithoutExtension( scriptAddress );
            var outputFolder = Path.GetDirectoryName( scriptAddress );
            if ( !Directory.Exists( outputFolder ) )
            {
                Directory.CreateDirectory( outputFolder );
            }

            EditorUtility.DisplayProgressBar( "Generate UIForm Derived ClassScript", $"class {formClsName}", 0.667f );

            string scriptContent = writeScriptHandler( formClsName );
            if ( !string.IsNullOrEmpty( s_NamespaceForGenerateCls ) )
            {
                File.WriteAllText( scriptAddress, scriptContent.Replace( "#____1", $"namespace {s_NamespaceForGenerateCls} {{" ).Replace( "#____2", "}" ) );
            }
            else
            {
                File.WriteAllText( scriptAddress, scriptContent.Replace( "#____1", "" ).Replace( "#____2", "" ) );
            }

            EditorUtility.ClearProgressBar( );
            EditorPrefs.SetString( KEY_GENERATE_CLASS, formClsName );
            EditorPrefs.SetString( KEY_GENERATE_PREFAB, assetPath );
            AssetDatabase.SaveAssets( );
            AssetDatabase.Refresh( );
        }

        private static void NewForm( string assetPath, string scriptAddress )
        {

            CreateScript( assetPath, scriptAddress, formClsName =>
            {
                return $@"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#____1

public partial class {formClsName} : UIFormBase
{{
    protected override void OnOpen(object userData)
    {{
        base.OnOpen(userData);
        
    }}
}}

#____2

";
            } );
        }

        private static void NewDialog( string assetPath, string scriptAddress )
        {
            //Dialog 你可以自定一个基础类，而不是直接使用 UIFormBase
            //一般可能存在的需求是 弹窗队列， 可能需要你实现一套基础 UIFormBase, 但是会在初始化的时候讲自己注册进 Queue里
            //比如： 实现多个弹窗，依次摊开，而非直接一堆弹窗拍脸
            CreateScript( assetPath, scriptAddress, formClsName =>
            {
                return $@"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#____1

public partial class {formClsName} : UIFormBase
{{
    protected override void OnOpen(object userData)
    {{
        base.OnOpen(userData);

    }}
}}

#____2
";
            } );
        }

        private static void NewItem( string assetPath, string scriptAddress )
        {
            CreateScript( assetPath, scriptAddress, formClsName =>
            {
                return $@"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#____1

public partial class {formClsName} : UIItemBase
{{
    protected override void OnInit()
    {{
        base.OnInit();
        
    }}
}}

#____2
";
            } );
        }
    }
}

#endif