using System.IO;
using UnityEditor;
using UnityEngine;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace VaporUIElementsEditor
{
    public static class FolderSetupUtility
    {
        public const string EditorNamespace = "VaporUIEditor";
        public const string RelativePath = "Vapor/Editor/Inspectors";
        
        [InitializeOnLoadMethod]
        private static void SetupFolders()
        {
            AssetDatabase.StartAssetEditing();
            var changed = false;

            if (!AssetDatabase.IsValidFolder("Assets/Vapor"))
            {
                AssetDatabase.CreateFolder("Assets", "Vapor");
                changed = true;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Vapor/Editor"))
            {
                AssetDatabase.CreateFolder("Assets/Vapor", "Editor");
                changed = true;
            }
            
            if (!AssetDatabase.IsValidFolder("Assets/Vapor/Editor/Inspectors"))
            {
                AssetDatabase.CreateFolder("Assets/Vapor/Editor", "Inspectors");
                changed = true;
            }

            // var asmdef = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Vapor/Editor/Inspectors/VaporUIEditor.asmdef");
            // if (!asmdef)
            // {
            //     Debug.Log(Application.dataPath + "/Vapor/Editor/Inspectors/VaporUIEditor.asmdef");
            //     StreamWriter w = new(Application.dataPath + "/Vapor/Editor/Inspectors/VaporUIEditor.asmdef");
            //     const string format = "{\n" +
            //                           "\t\"name\": \"VaporUIEditor\",\n" +
            //                           "\t\"rootNamespace\": \"VaporUIEditor\",\n" +
            //                           "\t\"references\": [\n" +
            //                           "\t\t\"CarbonFiberGames.VaporUIElements\",\n" +
            //                           "\t\t\"CarbonFiberGames.VaporUIElementsEditor\"\n" +
            //                           "\t],\n" +
            //                           "\t\"includePlatforms\": [\"Editor\"],\n" +
            //                           "\t\"excludePlatforms\": [],\n" +
            //                           "\t\"allowUnsafeCode\": false,\n" +
            //                           "\t\"overrideReferences\": false,\n" +
            //                           "\t\"precompiledReferences\": [],\n" +
            //                           "\t\"autoReferenced\": true,\n" +
            //                           "\t\"defineConstraints\": [],\n" +
            //                           "\t\"versionDefines\": [],\n" +
            //                           "\t\"noEngineReferences\": false\n" +
            //                           "}";
            //     w.Write(format);
            //     w.Close();
            //     changed = true;
            // }

            AssetDatabase.StopAssetEditing();
            if (!changed) return;
            
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}
