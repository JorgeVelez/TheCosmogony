//using UnityEditor;
//using UnityEngine;

//namespace TheVegetationEngine
//{
//    [InitializeOnLoad]
//    class TVEListener : AssetPostprocessor
//    {
//        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//        {
//            foreach (var asset in importedAssets)
//            {
//                if (!asset.ToLowerInvariant().EndsWith(".mat"))
//                    continue;

//                var material = AssetDatabase.LoadAssetAtPath<Material>(asset);

//                TheVegetationEngine.TVEShaderUtils.SetMaterialRenderSettings(material);
//            }
//        }
//    }
//}