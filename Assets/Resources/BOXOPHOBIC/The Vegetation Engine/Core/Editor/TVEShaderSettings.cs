// Cristian Pop - https://boxophobic.com/

using UnityEngine;
using UnityEditor;
using Boxophobic.StyledGUI;
using Boxophobic.Utils;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace TheVegetationEngine
{
    public class TVEShaderSettings : EditorWindow
    {
#if UNITY_2019_3_OR_NEWER
        const int GUI_HEIGHT = 18;
#else
        const int GUI_HEIGHT = 14;
#endif
        string[] engineVegetationStudio = new string[]
        {
        "           //Vegetation Studio (Instanced Indirect)",
        "           #include \"XXX/Core/Includes/VS_Indirect.cginc\"",
        "           #pragma instancing_options procedural:setup forwardadd",
        "           #pragma multi_compile GPU_FRUSTUM_ON __",
        };

        string[] engineVegetationStudioHD = new string[]
        {
        "           //Vegetation Studio (Instanced Indirect)",
        "           #include \"XXX/Core/Includes/VS_IndirectHD.cginc\"",
        "           #pragma instancing_options procedural:setupVSPro forwardadd",
        "           #pragma multi_compile GPU_FRUSTUM_ON __",
        };

        string[] engineVegetationStudio145 = new string[]
{
        "           //Vegetation Studio 1.4.5+ (Instanced Indirect)",
        "           #include \"XXX/Core/Includes/VS_Indirect145.cginc\"",
        "           #pragma instancing_options procedural:setupVSPro forwardadd",
        "           #pragma multi_compile GPU_FRUSTUM_ON __",
};

        string[] engineQuadroRenderer = new string[]
        {
        "           //Mega World Quadro Renderer (Instanced Indirect)",
        "           #include \"XXX\"",
        "           #pragma instancing_options procedural:setupQuadroRenderer",
        "           #pragma multi_compile_instancing",
        };

        string[] engineGPUInstancer = new string[]
        {
        "           //GPU Instancer (Instanced Indirect)",
        "           #include \"XXX\"",
        "           #pragma instancing_options procedural:setupGPUI",
        "           #pragma multi_compile_instancing",
        };

        string[] engineNatureRenderer = new string[]
        {
        "           //Nature Renderer (Procedural Instancing)",
        "           #include \"XXX\"",
        "           #pragma instancing_options procedural:SetupNatureRenderer",
        };

        readonly string[] engineOptions =
        {
        "Unity Default Renderer",
        "Vegetation Studio (Instanced Indirect)",
        "Vegetation Studio 1.4.5+ (Instanced Indirect)",
        "Mega World Quadro Renderer (Instanced Indirect)",
        "Nature Renderer (Procedural Instancing)",
        "GPU Instancer (Instanced Indirect)",
        };

        string assetFolder = "Assets/BOXOPHOBIC/The Vegetation Engine";
        string userFolder = "Assets/BOXOPHOBIC/User";

        List<string> coreShaderPaths;
        List<int> engineOverridesIndices;

        int coreEngineIndex = 0;

        bool showAdvancedSettings = false;
        bool showAdvancedSettingsOld = false;

        GUIStyle stylePopup;

        Color bannerColor;
        string bannerText;
        string helpURL;
        static TVEShaderSettings window;
        Vector2 scrollPosition = Vector2.zero;

        [MenuItem("Window/BOXOPHOBIC/The Vegetation Engine/Shader Settings", false, 1003)]
        public static void ShowWindow()
        {
            window = GetWindow<TVEShaderSettings>(false, "Shader Settings", true);
            window.minSize = new Vector2(389, 220);
        }

        void OnEnable()
        {
            //Safer search, there might be many user folders
            string[] searchFolders;

            searchFolders = AssetDatabase.FindAssets("The Vegetation Engine");

            for (int i = 0; i < searchFolders.Length; i++)
            {
                if (AssetDatabase.GUIDToAssetPath(searchFolders[i]).EndsWith("The Vegetation Engine.pdf"))
                {
                    assetFolder = AssetDatabase.GUIDToAssetPath(searchFolders[i]);
                    assetFolder = assetFolder.Replace("/The Vegetation Engine.pdf", "");
                }
            }

            searchFolders = AssetDatabase.FindAssets("User");

            for (int i = 0; i < searchFolders.Length; i++)
            {
                if (AssetDatabase.GUIDToAssetPath(searchFolders[i]).EndsWith("User.pdf"))
                {
                    userFolder = AssetDatabase.GUIDToAssetPath(searchFolders[i]);
                    userFolder = userFolder.Replace("/User.pdf", "");
                    userFolder = userFolder + "/The Vegetation Engine/";
                }
            }

            var cgincQR = "Assets/Mega World/Quadro Renderer/Shaders/Include/QuadroRendererInclude.cginc";
            searchFolders = AssetDatabase.FindAssets("QuadroRendererInclude");

            for (int i = 0; i < searchFolders.Length; i++)
            {
                if (AssetDatabase.GUIDToAssetPath(searchFolders[i]).EndsWith("QuadroRendererInclude.cginc"))
                {
                    cgincQR = AssetDatabase.GUIDToAssetPath(searchFolders[i]);
                }
            }

            var cgincNR = "Assets/Visual Design Cafe/Nature Shaders/Common/Nodes/Integrations/Nature Renderer.cginc";
            searchFolders = AssetDatabase.FindAssets("Nature Renderer");

            for (int i = 0; i < searchFolders.Length; i++)
            {
                if (AssetDatabase.GUIDToAssetPath(searchFolders[i]).EndsWith("Nature Renderer.cginc"))
                {
                    cgincNR = AssetDatabase.GUIDToAssetPath(searchFolders[i]);
                }
            }

            var cgincGPUI = "Assets/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc";
            searchFolders = AssetDatabase.FindAssets("GPUInstancerInclude");

            for (int i = 0; i < searchFolders.Length; i++)
            {
                if (AssetDatabase.GUIDToAssetPath(searchFolders[i]).EndsWith("GPUInstancerInclude.cginc"))
                {
                    cgincGPUI = AssetDatabase.GUIDToAssetPath(searchFolders[i]);
                }
            }

            // Add correct paths for VSP and GPUI
            engineNatureRenderer[1] = engineNatureRenderer[1].Replace("XXX", cgincNR);
            engineVegetationStudio[1] = engineVegetationStudio[1].Replace("XXX", assetFolder);
            engineVegetationStudioHD[1] = engineVegetationStudioHD[1].Replace("XXX", assetFolder);
            engineVegetationStudio145[1] = engineVegetationStudio145[1].Replace("XXX", assetFolder);
            engineGPUInstancer[1] = engineGPUInstancer[1].Replace("XXX", cgincGPUI);
            engineQuadroRenderer[1] = engineQuadroRenderer[1].Replace("XXX", cgincQR);

            GetCoreShaders();

            // GetUser Settings
            for (int i = 0; i < engineOptions.Length; i++)
            {
                if (engineOptions[i] == SettingsUtils.LoadSettingsData(userFolder + "Engine.asset", ""))
                {
                    coreEngineIndex = i;
                }
            }

            bannerColor = new Color(0.890f, 0.745f, 0.309f);
            bannerText = "Shader Settings";
            helpURL = "https://docs.google.com/document/d/145JOVlJ1tE-WODW45YoJ6Ixg23mFc56EnB_8Tbwloz8/edit#heading=h.x8fx57dtj0qi";
        }

        void OnGUI()
        {
            SetGUIStyles();

            StyledGUI.DrawWindowBanner(bannerColor, bannerText, helpURL);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);

            GUILayout.BeginVertical();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(this.position.width - 28), GUILayout.Height(this.position.height - 80));

            EditorGUILayout.HelpBox("Click the Render Engine Install button to enable instanced indirect rendering support for all shader or enable the Advanced settings to choose per shader render engine!", MessageType.Info, true);
            EditorGUILayout.HelpBox("GPU Instancer and Quadro Renderer create compatible shaders automatically and adding support is not required. You can still enable the support if you need the instanced indirect to be added to the original shaders specifically!", MessageType.Warning, true);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Render Engine Support", GUILayout.Width(220));
            coreEngineIndex = EditorGUILayout.Popup(coreEngineIndex, engineOptions, stylePopup);

            if (GUILayout.Button("Install", GUILayout.Width(80), GUILayout.Height(GUI_HEIGHT)))
            {
                SettingsUtils.SaveSettingsData(userFolder + "Engine.asset", engineOptions[coreEngineIndex]);

                UpdateShaders();

                // Refresh overrides if opened
                showAdvancedSettingsOld = false;

                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Show Advanced Settings", GUILayout.Width(220));
            showAdvancedSettings = EditorGUILayout.Toggle(showAdvancedSettings);

            GUILayout.EndHorizontal();

            if (showAdvancedSettings == true)
            {
                if (showAdvancedSettingsOld != showAdvancedSettings)
                {
                    for (int i = 0; i < coreShaderPaths.Count; i++)
                    {
                        GetRenderEngineFromShader(coreShaderPaths[i], i);
                    }

                    showAdvancedSettingsOld = showAdvancedSettings;
                }

                GUILayout.Space(10);

                for (int i = 0; i < coreShaderPaths.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(Path.GetFileNameWithoutExtension(coreShaderPaths[i].Replace("(TVE Shader)", "(user)")), GUILayout.Width(220));
                    engineOverridesIndices[i] = EditorGUILayout.Popup(engineOverridesIndices[i], engineOptions, stylePopup);

                    if (GUILayout.Button("Install", GUILayout.Width(80), GUILayout.Height(GUI_HEIGHT)))
                    {
                        InjectShaderFeature(coreShaderPaths[i], engineOptions[engineOverridesIndices[i]]);

                        SettingsUtils.SaveSettingsData(userFolder + "Engine.asset", "Mixed Render Engines");
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.FlexibleSpace();
            GUI.enabled = false;

            GUILayout.Space(20);


            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUILayout.Space(13);
            GUILayout.EndHorizontal();
        }

        void SetGUIStyles()
        {
            stylePopup = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }

        void GetCoreShaders()
        {
            coreShaderPaths = new List<string>();
            engineOverridesIndices = new List<int>();

            var allShaderPaths = Directory.GetFiles("Assets/", "*.shader", SearchOption.AllDirectories);

            for (int i = 0; i < allShaderPaths.Length; i++)
            {
                // No direct path usage to awoid Windows/Mac slash convention issues
                if (allShaderPaths[i].Contains("The Vegetation Engine") || allShaderPaths[i].Contains("TVE Shader"))
                {
                    if (allShaderPaths[i].Contains("Bakers") == false &&
                        allShaderPaths[i].Contains("Elements") == false &&
                        allShaderPaths[i].Contains("Helpers") == false &&
                        allShaderPaths[i].Contains("Legacy") == false &&
                        allShaderPaths[i].Contains("Templates") == false &&
                        allShaderPaths[i].Contains("Texture Packer") == false &&
                        allShaderPaths[i].Contains("Terrain Details") == false &&
                        allShaderPaths[i].Contains("GPUI") == false)
                    {
                        // Auto generated GPUI Shaders need to be removed to avoid issues
                        //if (allShaderPaths[i].Contains("GPUI"))
                        //{
                        //    FileUtil.DeleteFileOrDirectory(allShaderPaths[i]);
                        //    AssetDatabase.Refresh();
                        //}
                        //else
                        {
                            coreShaderPaths.Add(allShaderPaths[i]);
                            engineOverridesIndices.Add(0);
                        }
                    }
                }
            }
        }

        void UpdateShaders()
        {
            for (int i = 0; i < coreShaderPaths.Count; i++)
            {
                InjectShaderFeature(coreShaderPaths[i], engineOptions[coreEngineIndex]);
            }

            Debug.Log("[The Vegetation Engine] " + "Shader features updated!");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void GetRenderEngineFromShader(string shaderAssetPath, int index)
        {
            StreamReader reader = new StreamReader(shaderAssetPath);

            string lines = reader.ReadToEnd();

            for (int i = 0; i < engineOptions.Length; i++)
            {
                if (lines.Contains(engineOptions[i]))
                {
                    engineOverridesIndices[index] = i;
                }
            }

            reader.Close();
        }

        void InjectShaderFeature(string shaderAssetPath, string renderEngine)
        {
            StreamReader reader = new StreamReader(shaderAssetPath);

            List<string> lines = new List<string>();

            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            reader.Close();

            // Delete Features before adding new ones
            int count = lines.Count;

            for (int i = 0; i < count; i++)
            {
                if (lines[i].Contains("SHADER INJECTION POINT BEGIN"))
                {
                    int c = 0;
                    int j = i + 1;

                    while (lines[j].Contains("SHADER INJECTION POINT END") == false)
                    {
                        j++;
                        c++;
                    }

                    lines.RemoveRange(i + 1, c);
                    count = count - c;
                }
            }

            var pipeline = SettingsUtils.LoadSettingsData(userFolder + "Pipeline.asset", "Standard");

            // Delete GPUI added lines    
            count = lines.Count;

            //if (pipeline.Contains("Standard"))
            //{
            //    for (int i = 0; i < count; i++)
            //    {
            //        if (lines[i].StartsWith("#"))
            //        {
            //            lines.RemoveRange(i + 1, 4);
            //            count = count - 4;

            //            i--;
            //        }

            //        if (lines[i].Contains("#pragma target 3.0"))
            //        {
            //            if (lines[i + 1].Contains("multi_compile_instancing") == false)
            //            {
            //                lines.Insert(i + 1, "		#pragma multi_compile_instancing");
            //            }
            //        }
            //    }
            //}
            //else if (pipeline.Contains("Universal"))
            //{
            //    for (int i = 0; i < count; i++)
            //    {
            //        if (lines[i].StartsWith("#"))
            //        {
            //            lines.RemoveRange(i, 3);
            //            count = count - 3;

            //            i--;
            //        }

            //        if (lines[i].Contains("HLSLPROGRAM"))
            //        {
            //            if (lines[i + 1].Contains("multi_compile_instancing") == false)
            //            {
            //                lines.Insert(i + 1, "		    #pragma multi_compile_instancing");
            //            }
            //        }
            //    }
            //}
            //else if (pipeline.Contains("High"))
            //{
            //    for (int i = 0; i < count; i++)
            //    {
            //        if (lines[i].StartsWith("#"))
            //        {
            //            lines.RemoveRange(i, 3);
            //            count = count - 3;

            //            i--;
            //        }

            //        if (lines[i].Contains("HLSLINCLUDE"))
            //        {
            //            if (lines[i + 3].Contains("multi_compile_instancing") == false)
            //            {
            //                lines.Insert(i + 3, "		    #pragma multi_compile_instancing");
            //            }
            //        }
            //    }
            //}

            //for (int i = 0; i < count; i++)
            //{
            //    if (lines[i].Contains("GPUInstancerInclude.cginc"))
            //    {
            //        if (pipeline.Contains("Standard"))
            //        {
            //            lines.RemoveAt(i - 1);
            //            lines.RemoveAt(i);
            //            lines.RemoveAt(i + 1);
            //        }

            //        count = count - 3;
            //    }
            //}

            //Inject 3rd Party Support
            if (renderEngine.Contains("Vegetation Studio (Instanced Indirect)"))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("SHADER INJECTION POINT BEGIN"))
                    {
                        if (pipeline.Contains("High"))
                        {
                            lines.InsertRange(i + 1, engineVegetationStudioHD);
                        }
                        else
                        {
                            lines.InsertRange(i + 1, engineVegetationStudio);
                        }
                    }
                }
            }

            if (renderEngine.Contains("Vegetation Studio 1.4.5+ (Instanced Indirect)"))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("SHADER INJECTION POINT BEGIN"))
                    {
                        lines.InsertRange(i + 1, engineVegetationStudio145);
                    }
                }
            }

            if (renderEngine.Contains("Mega World Quadro Renderer (Instanced Indirect)"))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("SHADER INJECTION POINT BEGIN"))
                    {
                        lines.InsertRange(i + 1, engineQuadroRenderer);
                    }
                }
            }

            if (renderEngine.Contains("Nature Renderer (Procedural Instancing)"))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("SHADER INJECTION POINT BEGIN"))
                    {
                        lines.InsertRange(i + 1, engineNatureRenderer);
                    }
                }
            }

            if (renderEngine.Contains("GPU Instancer (Instanced Indirect)"))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("SHADER INJECTION POINT BEGIN"))
                    {
                        lines.InsertRange(i + 1, engineGPUInstancer);
                    }
                }
            }

            for (int i = 0; i < lines.Count; i++)
            {
                // Disable ASE Drawers
                if (lines[i].Contains("[ASEBegin]"))
                {
                    lines[i] = lines[i].Replace("[ASEBegin]", "");
                }

                if (lines[i].Contains("[ASEnd]"))
                {
                    lines[i] = lines[i].Replace("[ASEnd]", "");
                }
            }

#if !AMPLIFY_SHADER_EDITOR && !UNITY_2020_2_OR_NEWER

            // Add diffusion profile support for HDRP 10
            if (pipeline.Contains("High"))
            {
                if (shaderAssetPath.Contains("Subsurface Lit"))
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("DiffusionProfile"))
                        {
                            lines[i] = lines[i].Replace("[DiffusionProfile]", "[StyledDiffusionMaterial(_SubsurfaceDiffusion)]");
                        }
                    }
                }
            }

#elif AMPLIFY_SHADER_EDITOR && !UNITY_2020_2_OR_NEWER

            // Add diffusion profile support
            if (pipeline.Contains("High"))
            {
                if (shaderAssetPath.Contains("Subsurface Lit"))
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("ASEDiffusionProfile"))
                        {
                            lines[i] = lines[i].Replace("[HideInInspector][Space(10)][ASEDiffusionProfile(_SubsurfaceDiffusion)]", "[Space(10)][ASEDiffusionProfile(_SubsurfaceDiffusion)]");
                        }

                        if (lines[i].Contains("DiffusionProfile"))
                        {
                            lines[i] = lines[i].Replace("[DiffusionProfile]", "[HideInInspector][DiffusionProfile]");
                        }

                        if (lines[i].Contains("StyledDiffusionMaterial"))
                        {
                            lines[i] = lines[i].Replace("[StyledDiffusionMaterial(_SubsurfaceDiffusion)]", "[HideInInspector][StyledDiffusionMaterial(_SubsurfaceDiffusion)]");
                        }
                    }
                }
            }
#endif

            StreamWriter writer = new StreamWriter(shaderAssetPath);

            for (int i = 0; i < lines.Count; i++)
            {
                writer.WriteLine(lines[i]);
            }

            writer.Close();

            lines = new List<string>();

            //AssetDatabase.ImportAsset(shaderAssetPath);
        }
    }
}
