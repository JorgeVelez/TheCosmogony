﻿// Cristian Pop - https://boxophobic.com/

using UnityEngine;
using UnityEditor;
using Boxophobic.StyledGUI;
using Boxophobic.Utils;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System;

namespace TheVegetationEngine
{
    public class TVEPrefabConverter : EditorWindow
    {
        const int SPACE_SMALL = 5;
        const int GUI_MESH = 24;

        const int NONE = 0;
        const bool SRGB = true;
        const bool ALPHA_DEFAULT = true;

        const int SQUARE_BUTTON_WIDTH = 20;
        const int SQUARE_BUTTON_HEIGHT = 18;
        float GUI_HALF_EDITOR_WIDTH = 200;

        const string BACKUP_DATA_PATH = "/Assets Data/Backup Data";
        const string ORIGINAL_DATA_PATH = "/Assets Data/Original Data";
        const string PREFABS_DATA_PATH = "/Assets Data/Prefabs Data";
        const string SHARED_DATA_PATH = "/Assets Data/Shared Data";

        readonly int[] MaxTextureSizes = new int[]
        {
        0,
        32,
        64,
        128,
        256,
        512,
        1024,
        2048,
        4096,
        8192,
        };

        string[] SourceMaskEnum = new string[]
        {
        "None", "Channel", "Procedural", "Texture", "3rd Party",
        };

        string[] SourceMaskMeshEnum = new string[]
        {
        "[0]  Vertex R", "[1]  Vertex G", "[2]  Vertex B", "[3]  Vertex A",
        "[4]  UV 0 X", "[5]  UV 0 Y", "[6]  UV 0 Z", "[7]  UV 0 W",
        "[8]  UV 2 X", "[9]  UV 2 Y", "[10]  UV 2 Z", "[11]  UV 2 W",
        "[12]  UV 3 X", "[13]  UV 3 Y", "[14]  UV 3 Z", "[16]  UV 3 W",
        "[16]  UV 4 X", "[17]  UV 4 Y", "[18]  UV 4 Z", "[19]  UV 4 W",
        };

        string[] SourceMaskProceduralEnum = new string[]
        {
        "[0]  Constant Black", "[1]  Constant White", "[2]  Random Element Variation", "[3]  Predictive Element Variation", "[4]  Height", "[5]  Sphere", "[6]  Cylinder", "[7]  Capsule",
        "[8]  Bottom To Top", "[9]  Top To Bottom", "[10]  Bottom Projection", "[11]  Top Projection", "[12]  Height Exp", "[13]  Hemi Sphere", "[14]  Hemi Cylinder", "[15]  Hemi Capsule",
        "[16]  Height Offset (Low)", "[17]  Height Offset (Medium)", "[18]  Height Offset (High)"
        };

        string[] SourceMask3rdPartyEnum = new string[]
        {
        "[0]  CTI Leaves Mask", "[1]  CTI Leaves Variation", "[2]  ST8 Leaves Mask", "[3]  NM Leaves Mask"
        };

        string[] SourceFromTextureEnum = new string[]
        {
        "[0]  Channel R", "[1]  Channel G", "[2]  Channel B", "[3]  Channel A"
        };

        string[] SourceActionEnum = new string[]
        {
        "None", "Invert", "Remap 0-1"
        };

        string[] SourceCoordEnum = new string[]
        {
        "None", "Channel", "Procedural", "3rd Party",
        };

        string[] SourceCoordMeshEnum = new string[]
        {
        "[0]  UV 0", "[1]  UV 2", "[2]  UV 3", "[3]  UV 4",
        };

        string[] SourceCoordProceduralEnum = new string[]
        {
        "[0]  Planar XZ", "[1]  Planar XY", "[2]  Planar ZY", "[3]  Procedural Pivots",
        };

        string[] SourceCoord3rdPartyEnum = new string[]
        {
        "[0]  NM Trunk Blend"
        };

        string[] SourceNormalsEnum = new string[]
        {
        "From Mesh", "Procedural",
        };

        string[] SourceNormalsProceduralEnum = new string[]
        {
        "[0]  Recalculate Normals", "[1]  Flat Shading (Low)", "[2]  Flat Shading (Medium)", "[3]  Flat Shading (Full)", "[4]  Spherical Shading",
        };

        enum OutputTexture
        {
            UseCurrentResolution = 0,
            UseHighestResolution = 1,
        }

        enum OutputMesh
        {
            Off = 0,
            Default = 10,
            Custom = 20,
            Detail = 30,
            DEEnvironment = 100,
            Polyverse = 110,
        }

        enum OutputMaterial
        {
            Off = 0,
            Default = 10,
        }

        OutputMesh outputMeshIndex = OutputMesh.Default;
        OutputTexture outputTextureIndex = OutputTexture.UseHighestResolution;
        OutputMaterial outputMaterialIndex = OutputMaterial.Default;
        string outputSuffix = "TVE";

        string infoTitle = "";
        string infoPreset = "";
        string infoStatus = "";
        string infoOnline = "";
        string infoWarning = "";

        int sourceVariation = 0;
        int optionVariation = 0;
        int actionVariation = 0;
        Texture2D textureVariation;

        int sourceOcclusion = 0;
        int optionOcclusion = 0;
        int actionOcclusion = 0;
        Texture2D textureOcclusion;

        int sourceDetail = 0;
        int optionDetail = 0;
        int actionDetail = 0;
        Texture2D textureDetail;

        int sourceMulti = 0;
        int optionMulti = 0;
        int actionMulti = 0;
        Texture2D textureMulti;

        int sourceDetailCoord = 0;
        int optionDetailCoord = 0;

        int sourceMotion1 = 0;
        int optionMotion1 = 0;
        int actionMotion1 = 0;
        Texture2D textureMotion1;

        int sourceMotion2 = 0;
        int optionMotion2 = 0;
        int actionMotion2 = 0;
        Texture2D textureMotion2;

        int sourceMotion3 = 0;
        int optionMotion3 = 0;
        int actionMotion3 = 0;
        Texture2D textureMotion3;

        int sourceNormals = 0;
        int optionNormals = 0;

        string projectDataFolder;
        string prefabDataFolder;
        string userFolder = "Assets/BOXOPHOBIC/User";

        List<GameObject> prefabObjects;
        int convertedPrefabCount;

        GameObject prefabObject;
        GameObject prefabInstance;
        GameObject prefabBackup;
        string prefabName;

        List<GameObject> gameObjectsInPrefab;
        List<Vector3> transformPositions;
        List<Vector3> transformRotations;
        List<Vector3> transformScales;
        List<MeshRenderer> meshRenderersInPrefab;
        List<Material[]> materialArraysInPrefab;
        List<Material[]> materialArraysInstances;
        //List<Material> convertedMaterials;
        List<MeshFilter> meshFiltersInPrefab;
        List<Mesh> meshesInPrefab;
        List<Mesh> meshInstances;
        Vector4 maxBoundsInfo;

        Material blitMaterial;
        Texture blitTexture;
        TextureImporter[] sourceTexImporters;
        TextureImporterSettings[] sourceTexSettings;
        TextureImporterCompression[] sourceTexCompressions;
        int[] sourceimportSizes;

        int[] maskChannels;
        int[] maskActions;
        Texture[] maskTextures;
        List<string> packedTextureNames;
        List<Texture> packedTextureObjcts;

        Mesh convertedMesh;
        Material convertedMaterial;

        int presetIndex = 0;
        bool presetAutoDetected = false;
        List<int> overrideIndices;

        bool showAdvancedSettings = false;
        bool collectConvertedData = false;
        bool collectOriginalTextures = false;
        bool collectInCustomFolder = false;
        bool shareCommonMaterials = false;
        //bool keepConvertedMaterials = false;

        string[] allPresetPaths;
        List<string> presetPaths;
        string[] PresetsEnum;
        List<string> presetLines;
        List<string> overridePaths;
        string[] OverridesEnum;
        List<string> detectLines;

        Shader shaderCross;
        Shader shaderLeaf;
        Shader shaderBark;
        Shader shaderGrass;
        Shader shaderProp;

        bool isValid = true;
        bool showSelectedPrefabs = true;
        float seed = 1;

        GUIStyle stylePopup;
        GUIStyle styleCenteredHelpBox;
        Color bannerColor;
        string bannerText;
        string helpURL;
        static TVEPrefabConverter window;
        Vector2 scrollPosition = Vector2.zero;

        [MenuItem("Window/BOXOPHOBIC/The Vegetation Engine/Prefab Converter", false, 1001)]
        public static void ShowWindow()
        {
            window = GetWindow<TVEPrefabConverter>(false, "Prefab Converter", true);
            window.minSize = new Vector2(400, 280);
        }

        void OnEnable()
        {
            bannerColor = new Color(0.890f, 0.745f, 0.309f);
            bannerText = "Prefab Converter";
            helpURL = "https://docs.google.com/document/d/145JOVlJ1tE-WODW45YoJ6Ixg23mFc56EnB_8Tbwloz8/edit#heading=h.46l51yqt2zky";

            if (GameObject.Find("The Vegetation Engine") == null)
            {
                isValid = false;
            }

            string[] searchFolders = AssetDatabase.FindAssets("User");

            for (int i = 0; i < searchFolders.Length; i++)
            {
                if (AssetDatabase.GUIDToAssetPath(searchFolders[i]).EndsWith("User.pdf"))
                {
                    userFolder = AssetDatabase.GUIDToAssetPath(searchFolders[i]);
                    userFolder = userFolder.Replace("/User.pdf", "");
                    userFolder += "/The Vegetation Engine/";
                }
            }

            collectConvertedData = Convert.ToBoolean(SettingsUtils.LoadSettingsData(userFolder + "Converter Collect.asset", "False"));
            collectOriginalTextures = Convert.ToBoolean(SettingsUtils.LoadSettingsData(userFolder + "Converter Textures.asset", "True"));
            collectInCustomFolder = Convert.ToBoolean(SettingsUtils.LoadSettingsData(userFolder + "Converter Folder.asset", "True"));
            shareCommonMaterials = Convert.ToBoolean(SettingsUtils.LoadSettingsData(userFolder + "Converter Share.asset", "False"));

            int intSeed = UnityEngine.Random.Range(1, 99);
            float floatSeed = UnityEngine.Random.Range(0.1f, 0.9f);
            seed = intSeed + floatSeed;

            InitTexturePacker();
            GetDefaultShaders();

            GetPresets();
            Initialize();
        }

        void OnSelectionChange()
        {
            GetPrefabObjects();

            if (showAdvancedSettings == false)
            {
                GetPrefabPresets();
                GetAllPresetInfo();
            }

            Repaint();
        }

        void OnFocus()
        {
            GetPrefabObjects();

            if (showAdvancedSettings == false)
            {
                GetPrefabPresets();
                GetAllPresetInfo();
            }

            Repaint();
        }

        void Initialize()
        {
            overrideIndices = new List<int>();

            GetPrefabObjects();
            GetPrefabPresets();

            if (overrideIndices.Count == 0)
            {
                overrideIndices.Add(0);
            }

            GetAllPresetInfo();
        }

        void OnGUI()
        {
            GUI_HALF_EDITOR_WIDTH = this.position.width / 2.0f - 24;

            SetGUIStyles();

            StyledGUI.DrawWindowBanner(bannerColor, bannerText, helpURL);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);

            GUILayout.BeginVertical();

            DrawMessage();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(this.position.width - 28), GUILayout.Height(this.position.height - 220));

            if (isValid == false || prefabObjects.Count == 0)
            {
                GUI.enabled = false;
            }

            DrawPrefabObjects();
            DrawConversionSettings();
            DrawConvert();

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

            styleCenteredHelpBox = new GUIStyle(GUI.skin.GetStyle("HelpBox"))
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        void DrawMessage()
        {
            GUILayout.Space(-2);

            if (isValid && prefabObjects.Count > 0)
            {
                if (presetIndex != 0)
                {
                    //var warning = "";

                    //if (infoWarning != "")
                    //{
                    //    if (EditorGUIUtility.isProSkin)
                    //    {
                    //        warning = "\n<b><color=#f6d161>Warning! " + infoWarning + "</color></b>";
                    //    }
                    //    else
                    //    {
                    //        warning = "\n<b><color=#e16f00>Warning! " + infoWarning + "</color></b>";
                    //    }
                    //}

                    if (GUILayout.Button("\n<size=14>" + infoTitle + "</size>"
                                        + "\n\n" + infoPreset + " Click here for more details!"
                                        /*+ warning*/
                                        + "\n\n" + infoStatus + "\n"

                                        , styleCenteredHelpBox))
                    {
                        Application.OpenURL(infoOnline);
                    }

                    if (infoWarning != "")
                    {
                        EditorGUILayout.HelpBox(infoWarning, MessageType.Warning, true);
                    }
                }
                else
                {
                    //EditorGUILayout.HelpBox("Choose a preset to convert the selected prefabs! Please note that not all material properties will be copied after conversion and the new materials will have different shading!", MessageType.Info, true);
                    GUILayout.Button("\n<size=14>Choose a preset to convert the selected prefabs!</size>\n", styleCenteredHelpBox);
                }
            }
            else
            {
                if (isValid == false)
                {
                    EditorGUILayout.HelpBox("The Vegetation Engine manager is missing from your scene. Make sure setup it up first and the reopen the Prefab Converter!", MessageType.Warning, true);
                }
                else if (prefabObjects.Count == 0)
                {
                    //EditorGUILayout.HelpBox("Select a prefab or multiple prefabs to get started!", MessageType.Info, true);
                    GUILayout.Button("\n<size=14>Select a prefab or multiple prefabs to get started!</size>\n", styleCenteredHelpBox);
                }
            }
        }

        void DrawPrefabObjects()
        {
            if (prefabObjects.Count > 0)
            {
                GUILayout.Space(10);

                if (showSelectedPrefabs)
                {
                    if (StyledButton("▼ Hide Prefab Selection"))
                        showSelectedPrefabs = !showSelectedPrefabs;
                }
                else
                {
                    if (StyledButton("► Show Prefab Selection"))
                        showSelectedPrefabs = !showSelectedPrefabs;
                }

                if (showSelectedPrefabs)
                {
                    for (int i = 0; i < Selection.gameObjects.Length; i++)
                    {
                        StyledGameObject(i, prefabObjects);
                    }
                }
            }
        }

        void DrawConversionSettings()
        {
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();

            presetIndex = StyledPresetPopup("Conversion Preset", "The preset used to convert the selected prefab or prefabs.", presetIndex, PresetsEnum);

            if (presetIndex != 0)
            {
                for (int i = 0; i < overrideIndices.Count; i++)
                {
                    overrideIndices[i] = StyledPresetPopup("Conversion Overrides", "Adds extra functionality over the currently used preset.", overrideIndices[i], OverridesEnum);
                }

                var overridesCount = overrideIndices.Count;

                if (overrideIndices[0] != 0 || overridesCount > 1)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("");

                    if (overridesCount > 1)
                    {
                        if (GUILayout.Button("-", GUILayout.MaxWidth(SQUARE_BUTTON_WIDTH), GUILayout.MaxHeight(SQUARE_BUTTON_HEIGHT)))
                        {
                            overrideIndices.RemoveAt(overridesCount - 1);
                        }
                    }

                    if (GUILayout.Button("+", GUILayout.MaxWidth(SQUARE_BUTTON_WIDTH), GUILayout.MaxHeight(SQUARE_BUTTON_HEIGHT)))
                    {
                        overrideIndices.Add(0);
                    }

                    GUILayout.EndHorizontal();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    GetAllPresetInfo();
                }

                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show Advanced Settings", GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                showAdvancedSettings = EditorGUILayout.Toggle(showAdvancedSettings);
                GUILayout.EndHorizontal();

                if (showAdvancedSettings)
                {
                    GUILayout.Space(10);
                    StyledGUI.DrawWindowCategory("Saving Settings");
                    GUILayout.Space(10);

                    if (collectConvertedData)
                    {
                        EditorGUILayout.HelpBox("When Collect Converted Data is enabled, all prefabs and the associated assets are moved to a new folder of your choice for better project organization! The following settings are saved for futhur conversions!", MessageType.Info, true);
                    }

                    if (collectConvertedData && shareCommonMaterials)
                    {
                        EditorGUILayout.HelpBox("Sharing common materials can lead to unexpected results if the assets have the same name! The new common assets will be saved in the Assets Data/Shared Data folder. Use it at your own risk!", MessageType.Warning, true);
                    }

                    if (collectConvertedData || shareCommonMaterials)
                    {
                        GUILayout.Space(10);
                    }

                    EditorGUI.BeginChangeCheck();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Collect Converted Data", "Collect all prefabs and the associated assets in a new folder called Assets Data for better project organization."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                    collectConvertedData = EditorGUILayout.Toggle(collectConvertedData);
                    GUILayout.EndHorizontal();

                    if (collectConvertedData)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Collect Original Textures", "Collect all original textures to the Assets Data/Original Data folder."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                        collectOriginalTextures = EditorGUILayout.Toggle(collectOriginalTextures);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Collect In Custom Folder", "Show a new folder saving dialogue to choose where the converted assets are saved. If disabled, the latest folder path will be used if available."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                        collectInCustomFolder = EditorGUILayout.Toggle(collectInCustomFolder);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Share Common Materials", "Save the converted materials by sharing the common assets."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                        shareCommonMaterials = EditorGUILayout.Toggle(shareCommonMaterials);
                        GUILayout.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        SettingsUtils.SaveSettingsData(userFolder + "Converter Collect.asset", collectConvertedData.ToString());
                        SettingsUtils.SaveSettingsData(userFolder + "Converter Textures.asset", collectOriginalTextures.ToString());
                        SettingsUtils.SaveSettingsData(userFolder + "Converter Folder.asset", collectInCustomFolder.ToString());
                        SettingsUtils.SaveSettingsData(userFolder + "Converter Share.asset", shareCommonMaterials.ToString());
                    }

                    GUILayout.Space(10);
                    StyledGUI.DrawWindowCategory("Output Settings");
                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Output Meshes", "Mesh packing for the current preset."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                    outputMeshIndex = (OutputMesh)EditorGUILayout.EnumPopup(outputMeshIndex, stylePopup);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Output Materials", "Material conversion for the current preset."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                    outputMaterialIndex = (OutputMaterial)EditorGUILayout.EnumPopup(outputMaterialIndex, stylePopup);
                    GUILayout.EndHorizontal();

                    if (outputMaterialIndex > 0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Output Texture", "The Use Current Resolution conversion packs the textures at the currently used texture resolutions and it is faster. The Use Highest Resolution conversion packs the textures at the highest available resolution and it can be slower."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                        outputTextureIndex = (OutputTexture)EditorGUILayout.EnumPopup(outputTextureIndex, stylePopup);
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Output Suffix", "Suffix used when saving assets to disk."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                    outputSuffix = EditorGUILayout.TextField(outputSuffix);
                    GUILayout.EndHorizontal();

                    if (outputMeshIndex == OutputMesh.Default)
                    {
                        GUILayout.Space(10);
                        StyledGUI.DrawWindowCategory("Mesh Settings");
                        GUILayout.Space(10);

                        sourceNormals = StyledSourcePopup("Normals", "Mesh vertex normals override.", sourceNormals, SourceNormalsEnum);
                        optionNormals = StyledNormalsOptionEnum("Normals", "", sourceNormals, optionNormals, true);

                        GUILayout.Space(10);

                        sourceVariation = StyledSourcePopup("Variation", "Variation mask used for wind animation and global effects. Stored in Vertex Red.", sourceVariation, SourceMaskEnum);
                        optionVariation = StyledMaskOptionEnum("Variation", "", sourceVariation, optionVariation, false);
                        textureVariation = StyledTexture("Variation", "", sourceVariation, textureVariation);
                        actionVariation = StyledActionOptionEnum("Variation", "", sourceVariation, actionVariation, true);

                        sourceOcclusion = StyledSourcePopup("Occlusion", "Vertex Occlusion mask used to add depth and light scattering mask. Stored in Vertex Green.", sourceOcclusion, SourceMaskEnum);
                        optionOcclusion = StyledMaskOptionEnum("Occlusion", "", sourceOcclusion, optionOcclusion, false);
                        textureOcclusion = StyledTexture("Occlusion", "", sourceOcclusion, textureOcclusion);
                        actionOcclusion = StyledActionOptionEnum("Occlusion", "", sourceOcclusion, actionOcclusion, true);

                        sourceDetail = StyledSourcePopup("Detail Mask", "Detail mask used for layer blending for bark. Stored in Vertex Blue.", sourceDetail, SourceMaskEnum);
                        optionDetail = StyledMaskOptionEnum("Detail Mask", "", sourceDetail, optionDetail, false);
                        textureDetail = StyledTexture("Detail Mask", "", sourceDetail, textureDetail);
                        actionDetail = StyledActionOptionEnum("Detail Mask", "", sourceDetail, actionDetail, true);

                        sourceMulti = StyledSourcePopup("Multi Mask", "Multi mask used for gradinet colors for leaves and subsurface/overlay mask for grass. The default value is set to height. Stored in Vertex Alpha.", sourceMulti, SourceMaskEnum);
                        optionMulti = StyledMaskOptionEnum("Multi Mask", "", sourceMulti, optionMulti, false);
                        textureMulti = StyledTexture("Multi Mask", "", sourceMulti, textureMulti);
                        actionMulti = StyledActionOptionEnum("Multi Mask", "", sourceMulti, actionMulti, true);

                        GUILayout.Space(10);

                        sourceDetailCoord = StyledSourcePopup("Detail Coord or Pivots", "Detail UVs used for layer blending for bark. Pivots storing for grass when multiple grass blades are combined into a single mesh. Stored in UV0.ZW.", sourceDetailCoord, SourceCoordEnum);
                        optionDetailCoord = StyledCoordOptionEnum("Detail Coord or Pivots", "", sourceDetailCoord, optionDetailCoord, true);

                        GUILayout.Space(10);

                        sourceMotion1 = StyledSourcePopup("Motion Primary", "Motion mask used for bending animation. Stored in UV3.X.", sourceMotion1, SourceMaskEnum);
                        optionMotion1 = StyledMaskOptionEnum("Motion Primary", "", sourceMotion1, optionMotion1, false);
                        textureMotion1 = StyledTexture("Motion Primary", "", sourceMotion1, textureMotion1);
                        actionMotion1 = StyledActionOptionEnum("Motion Primary", "", sourceMotion1, actionMotion1, true);

                        sourceMotion2 = StyledSourcePopup("Motion Secondary", "Motion mask used for rolling animation. Stored in UV3.Y.", sourceMotion2, SourceMaskEnum);
                        optionMotion2 = StyledMaskOptionEnum("Motion Secondary", "", sourceMotion2, optionMotion2, false);
                        textureMotion2 = StyledTexture("Motion Secondary", "", sourceMotion2, textureMotion2);
                        actionMotion2 = StyledActionOptionEnum("Motion Secondary", "", sourceMotion2, actionMotion2, true);

                        sourceMotion3 = StyledSourcePopup("Motion Leaves", "Motion mask used for leaves and flutter animation. Stored in UV3.Z.", sourceMotion3, SourceMaskEnum);
                        optionMotion3 = StyledMaskOptionEnum("Motion Leaves", "", sourceMotion3, optionMotion3, false);
                        textureMotion3 = StyledTexture("Motion Leaves", "", sourceMotion3, textureMotion3);
                        actionMotion3 = StyledActionOptionEnum("Motion Leaves", "", sourceMotion3, actionMotion3, true);
                    }

                    if (outputMeshIndex == OutputMesh.Custom)
                    {
                        GUILayout.Space(10);
                        StyledGUI.DrawWindowCategory("Mesh Settings");
                        GUILayout.Space(10);

                        sourceNormals = StyledSourcePopup("Normals", "", sourceNormals, SourceNormalsEnum);
                        optionNormals = StyledNormalsOptionEnum("Normals", "", sourceNormals, optionNormals, true);

                        GUILayout.Space(10);

                        sourceVariation = StyledSourcePopup("Vertex Red", "", sourceVariation, SourceMaskEnum);
                        optionVariation = StyledMaskOptionEnum("Vertex Red", "", sourceVariation, optionVariation, false);
                        textureVariation = StyledTexture("Vertex Red", "", sourceVariation, textureVariation);
                        actionVariation = StyledActionOptionEnum("Vertex Red", "", sourceVariation, actionVariation, true);

                        sourceOcclusion = StyledSourcePopup("Vertex Green", "", sourceOcclusion, SourceMaskEnum);
                        optionOcclusion = StyledMaskOptionEnum("Vertex Green", "", sourceOcclusion, optionOcclusion, false);
                        textureOcclusion = StyledTexture("Vertex Green", "", sourceOcclusion, textureOcclusion);
                        actionOcclusion = StyledActionOptionEnum("Vertex Green", "", sourceOcclusion, actionOcclusion, true);

                        sourceDetail = StyledSourcePopup("Vertex Blue", "", sourceDetail, SourceMaskEnum);
                        optionDetail = StyledMaskOptionEnum("Vertex Blue", "", sourceDetail, optionDetail, false);
                        textureDetail = StyledTexture("Vertex Blue", "", sourceDetail, textureDetail);
                        actionDetail = StyledActionOptionEnum("Vertex Blue", "", sourceDetail, actionDetail, true);

                        sourceMulti = StyledSourcePopup("Vertex Alpha", "", sourceMulti, SourceMaskEnum);
                        optionMulti = StyledMaskOptionEnum("Vertex Alpha", "", sourceMulti, optionMulti, false);
                        textureMulti = StyledTexture("Vertex Blue", "", sourceMulti, textureMulti);
                        actionMulti = StyledActionOptionEnum("Vertex Alpha", "", sourceMulti, actionMulti, true);
                    }

                    if (outputMeshIndex == OutputMesh.Detail)
                    {
                        GUILayout.Space(10);
                        StyledGUI.DrawWindowCategory("Mesh Settings");
                        GUILayout.Space(10);

                        sourceMulti = StyledSourcePopup("Height", "", sourceMulti, SourceMaskEnum);
                        optionMulti = StyledMaskOptionEnum("Height", "", sourceMulti, optionMulti, false);
                        textureMulti = StyledTexture("Vertex Blue", "", sourceMulti, textureMulti);
                        actionMulti = StyledActionOptionEnum("Height", "", sourceMulti, actionMulti, true);
                    }

                    if (outputMeshIndex == OutputMesh.DEEnvironment)
                    {
                        GUILayout.Space(10);
                        StyledGUI.DrawWindowCategory("Mesh Settings");
                        GUILayout.Space(10);

                        sourceNormals = StyledSourcePopup("Normals", "", sourceNormals, SourceNormalsEnum);
                        optionNormals = StyledNormalsOptionEnum("Normals", "", sourceNormals, optionNormals, true);

                        GUILayout.Space(10);

                        sourceVariation = StyledSourcePopup("Variation", "", sourceVariation, SourceMaskEnum);
                        optionVariation = StyledMaskOptionEnum("Variation", "", sourceVariation, optionVariation, false);
                        textureVariation = StyledTexture("Variation", "", sourceVariation, textureVariation);
                        actionVariation = StyledActionOptionEnum("Variation", "", sourceVariation, actionVariation, true);

                        sourceOcclusion = StyledSourcePopup("Occlusion", "", sourceOcclusion, SourceMaskEnum);
                        optionOcclusion = StyledMaskOptionEnum("Occlusion", "", sourceOcclusion, optionOcclusion, false);
                        textureOcclusion = StyledTexture("Occlusion", "", sourceOcclusion, textureOcclusion);
                        actionOcclusion = StyledActionOptionEnum("Occlusion", "", sourceOcclusion, actionOcclusion, true);

                        sourceMotion1 = StyledSourcePopup("Motion Primary", "", sourceMotion1, SourceMaskEnum);
                        optionMotion1 = StyledMaskOptionEnum("Motion Primary", "", sourceMotion1, optionMotion1, false);
                        textureMotion1 = StyledTexture("Motion Primary", "", sourceMotion1, textureMotion1);
                        actionMotion1 = StyledActionOptionEnum("Motion Primary", "", sourceMotion1, actionMotion1, true);

                        sourceMotion2 = StyledSourcePopup("Motion Detail", "", sourceMotion2, SourceMaskEnum);
                        optionMotion2 = StyledMaskOptionEnum("Motion Detail", "", sourceMotion2, optionMotion2, false);
                        textureMotion2 = StyledTexture("Motion Detail", "", sourceMotion2, textureMotion2);
                        actionMotion2 = StyledActionOptionEnum("Motion Detail", "", sourceMotion2, actionMotion2, true);
                    }

                    if (outputMeshIndex == OutputMesh.Polyverse)
                    {
                        GUILayout.Space(10);
                        StyledGUI.DrawWindowCategory("Mesh Settings");
                        GUILayout.Space(10);

                        sourceNormals = StyledSourcePopup("Normals", "", sourceNormals, SourceNormalsEnum);
                        optionNormals = StyledNormalsOptionEnum("Normals", "", sourceNormals, optionNormals, true);

                        GUILayout.Space(10);

                        sourceMulti = StyledSourcePopup("Gradient Mask", "", sourceMulti, SourceMaskEnum);
                        optionMulti = StyledMaskOptionEnum("Gradient Mask", "", sourceMulti, optionMulti, false);
                        textureMulti = StyledTexture("Vertex Blue", "", sourceMulti, textureMulti);
                        actionMulti = StyledActionOptionEnum("Gradient Mask", "", sourceMulti, actionMulti, true);

                        sourceMotion1 = StyledSourcePopup("Motion Primary", "", sourceMotion1, SourceMaskEnum);
                        optionMotion1 = StyledMaskOptionEnum("Motion Primary", "", sourceMotion1, optionMotion1, false);
                        textureMotion1 = StyledTexture("Motion Primary", "", sourceMotion1, textureMotion1);
                        actionMotion1 = StyledActionOptionEnum("Motion Primary", "", sourceMotion1, actionMotion1, true);

                        sourceMotion2 = StyledSourcePopup("Motion Detail", "", sourceMotion2, SourceMaskEnum);
                        optionMotion2 = StyledMaskOptionEnum("Motion Detail", "", sourceMotion2, optionMotion2, false);
                        textureMotion2 = StyledTexture("Motion Primary", "", sourceMotion2, textureMotion2);
                        actionMotion2 = StyledActionOptionEnum("Motion Detail", "", sourceMotion2, actionMotion2, true);
                    }
                }
            }
        }

        void DrawConvert()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            if (prefabObjects.Count == 0)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }

            if (convertedPrefabCount == 0)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }

            if (GUILayout.Button("Revert", GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 2)))
            {
                for (int i = 0; i < prefabObjects.Count; i++)
                {
                    prefabObject = prefabObjects[i];

                    if (prefabObject.GetComponent<TVEPrefab>() != null && (prefabObject.GetComponent<TVEPrefab>().storedPrefabBackup != null || prefabObject.GetComponent<TVEPrefab>().storedPrefabBackupGUID != ""))
                    {
                        RevertPrefab(true);
                    }
                }

                GetPrefabObjects();
            }

            if (prefabObjects.Count == 0)
            {
                GUI.enabled = false;
            }
            else
            {
                if (presetIndex == 0)
                {
                    GUI.enabled = false;
                }
                else
                {
                    GUI.enabled = true;
                }
            }

            if (GUILayout.Button("Convert"))
            {
                if (collectConvertedData)
                {
                    var latestDataFolder = SettingsUtils.LoadSettingsData(userFolder + "Converter Latest.asset", "Assets");

                    if (!Directory.Exists(latestDataFolder))
                    {
                        latestDataFolder = "Assets";
                    }

                    if (collectInCustomFolder)
                    {
                        projectDataFolder = EditorUtility.OpenFolderPanel("Save Converted Assets to Folder", latestDataFolder, "");
                        
                        if (projectDataFolder != "")
                        {
                            projectDataFolder = "Assets" + projectDataFolder.Substring(Application.dataPath.Length);
                        }
                    }
                    else
                    {
                        if (latestDataFolder == "Assets")
                        {
                            projectDataFolder = EditorUtility.OpenFolderPanel("Save Converted Assets to Folder", latestDataFolder, "");

                            if (projectDataFolder != "")
                            {
                                projectDataFolder = "Assets" + projectDataFolder.Substring(Application.dataPath.Length);
                            }
                        }
                        else
                        {
                            projectDataFolder = latestDataFolder;
                        }
                    }

                    if (projectDataFolder != "")
                    {
                        if (!Directory.Exists(projectDataFolder))
                        {
                            Directory.CreateDirectory(projectDataFolder);
                            AssetDatabase.Refresh();
                        }

                        SettingsUtils.SaveSettingsData(userFolder + "Converter Latest.asset", projectDataFolder);

                        CreateAssetsDataSubFolders();
                    }
                    else
                    {
                        GUIUtility.ExitGUI();
                        return;
                    }
                }

                //if (convertedPrefabCount > 0)
                //{
                //    keepConvertedMaterials = EditorUtility.DisplayDialog("Reconvert Materials?", "Choose if the original materials should be reconverted or keep the current converted materials.", "Keep", "Reconvert Materials");

                //    if (keepConvertedMaterials)
                //    {
                //        StoreConvertedMaterials();
                //    }
                //}

                for (int i = 0; i < prefabObjects.Count; i++)
                {
                    prefabObject = prefabObjects[i];

                    EditorUtility.DisplayProgressBar("The Vegetation Engine", "Converting " + prefabObjects[i].name + " (" + (i + 1).ToString() + "/" + prefabObjects.Count.ToString() + ")", i * 1f / prefabObjects.Count);

                    if (prefabObject.GetComponent<TVEPrefab>() != null)
                    {
                        RevertPrefab(true);
                    }

                    ConvertPrefab();
                }

                GetPrefabObjects();

                EditorUtility.ClearProgressBar();
            }

            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        void StyledGameObject(int index, List<GameObject> gameObjects)
        {
            if (gameObjects.Count > index)
            {
                if (gameObjects[index].GetComponent<TVEPrefab>() == null)
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        if (GUILayout.Button("<size=10><b><color=#87b8ff>" + gameObjects[index].name + "</color></b></size>", styleCenteredHelpBox, GUILayout.Height(GUI_MESH)))
                        {
                            EditorGUIUtility.PingObject(gameObjects[index].gameObject);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("<size=10><b><color=#0b448b>" + gameObjects[index].name + "</color></b></size>", styleCenteredHelpBox, GUILayout.Height(GUI_MESH)))
                        {
                            EditorGUIUtility.PingObject(gameObjects[index].gameObject);
                        }
                    }
                }
                else
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        if (GUILayout.Button("<size=10><b><color=#f6d161>" + gameObjects[index].name + "</color></b></size>", styleCenteredHelpBox, GUILayout.Height(GUI_MESH)))
                        {
                            EditorGUIUtility.PingObject(gameObjects[index].gameObject);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("<size=10><b><color=#e16f00>" + gameObjects[index].name + "</color></b></size>", styleCenteredHelpBox, GUILayout.Height(GUI_MESH)))
                        {
                            EditorGUIUtility.PingObject(gameObjects[index].gameObject);
                        }
                    }
                }
            }
        }

        int StyledPopup(string name, string tooltip, int index, string[] options)
        {
            if (index >= options.Length)
            {
                index = 0;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(name, tooltip), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
            index = EditorGUILayout.Popup(index, options, stylePopup);
            GUILayout.EndHorizontal();

            return index;
        }

        int StyledPresetPopup(string name, string tooltip, int index, string[] options)
        {
            if (index >= options.Length)
            {
                index = 0;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(name, tooltip), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
            index = EditorGUILayout.Popup(index, options, stylePopup);

            var lastRect = GUILayoutUtility.GetLastRect();
            GUI.Label(lastRect, new GUIContent("", options[index]));

            GUILayout.EndHorizontal();

            return index;
        }

        int StyledSourcePopup(string name, string tooltip, int index, string[] options)
        {
            index = StyledPopup(name + " Source", tooltip, index, options);

            return index;
        }

        int StyledActionOptionEnum(string name, string tooltip, int source, int option, bool space)
        {
            if (source > 0)
            {
                option = StyledPopup(name + " Action", tooltip, option, SourceActionEnum);
            }

            if (space)
            {
                GUILayout.Space(SPACE_SMALL);
            }

            return option;
        }

        int StyledMaskOptionEnum(string name, string tooltip, int source, int option, bool space)
        {
            if (source == 1)
            {
                option = StyledPopup(name + " Channel", tooltip, option, SourceMaskMeshEnum);
            }
            if (source == 2)
            {
                option = StyledPopup(name + " Procedural", tooltip, option, SourceMaskProceduralEnum);
            }
            if (source == 3)
            {
                option = StyledPopup(name + " Channel", tooltip, option, SourceFromTextureEnum);
            }
            if (source == 4)
            {
                option = StyledPopup(name + " 3rd Party", tooltip, option, SourceMask3rdPartyEnum);
            }

            if (space)
            {
                GUILayout.Space(SPACE_SMALL);
            }

            return option;
        }

        Texture2D StyledTexture(string name, string tooltip, int source, Texture2D texture)
        {
            if (source == 3)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(name + " Texture", ""), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 5));
                texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
            }

            return texture;
        }

        int StyledCoordOptionEnum(string name, string tooltip, int source, int option, bool space)
        {
            if (source == 1)
            {
                option = StyledPopup(name + " Channel", tooltip, option, SourceCoordMeshEnum);
            }
            if (source == 2)
            {
                option = StyledPopup(name + " Procedural", tooltip, option, SourceCoordProceduralEnum);
            }
            if (source == 3)
            {
                option = StyledPopup(name + " 3rd Party", tooltip, option, SourceCoord3rdPartyEnum);
            }

            GUILayout.Space(SPACE_SMALL);

            return option;
        }

        int StyledNormalsOptionEnum(string name, string tooltip, int source, int option, bool space)
        {
            if (source == 1)
            {
                option = StyledPopup(name + " Procedural", tooltip, option, SourceNormalsProceduralEnum);
            }

            GUILayout.Space(SPACE_SMALL);

            return option;
        }

        bool StyledButton(string text)
        {
            bool value = GUILayout.Button("<b>" + text + "</b>", styleCenteredHelpBox, GUILayout.Height(GUI_MESH));

            return value;
        }

        /// <summary>
        /// Convert and Revert Macros
        /// </summary>

        void ConvertPrefab()
        {
            prefabName = prefabObject.name;

            string dataPath;

            if (collectConvertedData)
            {
                dataPath = projectDataFolder + "/" + prefabName + ".prefab";

                if (File.Exists(dataPath))
                {
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(dataPath) != prefabObject)
                    {
                        dataPath = AssetDatabase.GenerateUniqueAssetPath(dataPath);
                        prefabName = Path.GetFileNameWithoutExtension(dataPath);
                    }
                }
            }
            else
            {
                dataPath = AssetDatabase.GetAssetPath(prefabObject);
                dataPath = Path.GetDirectoryName(dataPath);
                dataPath = dataPath + "/" + prefabName;
                prefabDataFolder = dataPath;
            }

            var prefabScale = prefabObject.transform.localScale;

            prefabInstance = Instantiate(prefabObject, Vector3.zero, Quaternion.identity);
            prefabInstance.transform.localScale = Vector3.one;
            prefabInstance.AddComponent<TVEPrefab>();

            var prefabComponent = prefabInstance.GetComponent<TVEPrefab>();
            prefabComponent.storedPreset = PresetsEnum[presetIndex];
            prefabComponent.storedOverrides = new List<string>();

            for (int i = 0; i < overrideIndices.Count; i++)
            {
                if (overrideIndices[i] != 0)
                {
                    prefabComponent.storedOverrides.Add(OverridesEnum[overrideIndices[i]]);
                }
            }

            CreatePrefabDataFolder();
            CreatePrefabBackupFile();

            prefabComponent.storedPrefabBackup = null;
            prefabComponent.storedPrefabBackupGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefabBackup));

            GetGameObjectsInPrefab();
            DisableInvalidGameObjectsInPrefab();
            GetMeshRenderersInPrefab();
            GetMaterialArraysInPrefab();

            if (outputMeshIndex != OutputMesh.Off)
            {
                GetTransformsInPrefab();
                GetMeshFiltersInPrefab();
                GetMeshesInPrefab();
                CreateMeshInstances();
                TransformMeshesToWorldSpace();
                GetMaxBoundsInPrefab();
                prefabComponent.storedMaxBoundsInfo = maxBoundsInfo;
                //ApplyOriginalTransforms();

                ConvertMeshes();

                prefabInstance.transform.localScale = prefabScale;
            }

            if (outputMaterialIndex != OutputMaterial.Off)
            {
                CreateMaterialArraysInstances();

                ConvertMaterials();
                AssignConvertedMaterials();
            }

            EnableInvalidGameObjectsInPrefab();

            //PrefabUtility.SaveAsPrefabAssetAndConnect is breaking the prefab connection, the old version works as expected
            //PrefabUtility.SaveAsPrefabAssetAndConnect(prefabInstance, AssetDatabase.GetAssetPath(prefabObject), InteractionMode.AutomatedAction);

            PrefabUtility.ReplacePrefab(prefabInstance, prefabObject, ReplacePrefabOptions.ReplaceNameBased);

            if (collectConvertedData)
            {
                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(prefabObject), dataPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(prefabObject);

            DestroyImmediate(prefabInstance);
        }

        void RevertPrefab(bool keepData)
        {
            //if (prefabInstance != null)
            //{
            //    DestroyImmediate(prefabInstance);
            //}

            prefabInstance = Instantiate(prefabObject);

            var prefabBackup = GetPrefabBackupFile(prefabInstance);

            PrefabUtility.ReplacePrefab(prefabBackup, prefabObject, ReplacePrefabOptions.ReplaceNameBased);

            //PrefabUtility.SaveAsPrefabAssetAndConnect is breaking the prefab connection, the old version works as expected
            //PrefabUtility.SaveAsPrefabAssetAndConnect(prefabInstance, AssetDatabase.GetAssetPath(prefabObject), InteractionMode.AutomatedAction);

            if (keepData == false)
            {
                //DeletePrefabDataFolder();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            DestroyImmediate(prefabInstance);
        }

        //void StoreConvertedMaterials()
        //{
        //    convertedMaterials = new List<Material>();

        //    var gameObjects = new List<GameObject>();
        //    var meshRenderers = new List<MeshRenderer>();

        //    for (int i = 0; i < prefabObjects.Count; i++)
        //    {
        //        if (prefabObjects[i].GetComponent<TVEPrefab>() != null)
        //        {
        //            gameObjects.Add(prefabObjects[i]);
        //            GetChildRecursive(prefabObjects[i], gameObjects);
        //        }
        //    }

        //    for (int i = 0; i < gameObjects.Count; i++)
        //    {
        //        if (gameObjects[i].GetComponent<MeshRenderer>() != null)
        //        {
        //            meshRenderers.Add(gameObjects[i].GetComponent<MeshRenderer>());
        //        }
        //    }

        //    for (int i = 0; i < meshRenderers.Count; i++)
        //    {
        //        if (meshRenderers[i].sharedMaterials != null)
        //        {
        //            for (int j = 0; j < meshRenderers[i].sharedMaterials.Length; j++)
        //            {
        //                var material = meshRenderers[i].sharedMaterials[j];

        //                if (material != null)
        //                {
        //                    if (!convertedMaterials.Contains(material))
        //                    {
        //                        convertedMaterials.Add(material);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Get GameObjects, Materials and MeshFilters in Prefab
        /// </summary>

        void CreateAssetsDataSubFolders()
        {
            if (!Directory.Exists(projectDataFolder + BACKUP_DATA_PATH))
            {
                Directory.CreateDirectory(projectDataFolder + BACKUP_DATA_PATH);
            }

            if (!Directory.Exists(projectDataFolder + PREFABS_DATA_PATH))
            {
                Directory.CreateDirectory(projectDataFolder + PREFABS_DATA_PATH);
            }

            if (!Directory.Exists(projectDataFolder + ORIGINAL_DATA_PATH))
            {
                Directory.CreateDirectory(projectDataFolder + ORIGINAL_DATA_PATH);
            }

            if (!Directory.Exists(projectDataFolder + SHARED_DATA_PATH))
            {
                Directory.CreateDirectory(projectDataFolder + SHARED_DATA_PATH);
            }

            AssetDatabase.Refresh();
        }

        GameObject GetPrefabObjectRoot(GameObject selection)
        {
            GameObject prefabRoot = null;

            if (selection != null)
            {
                if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selection).Length > 0)
                {
                    var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selection);
                    var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var prefabAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(prefabPath);
                    var prefabType = PrefabUtility.GetPrefabAssetType(prefabAsset);

                    if (prefabAssets.Length == 0)
                    {
                        if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                        {
                            prefabRoot = prefabAsset;
                        }
                        else if (prefabType == PrefabAssetType.MissingAsset || prefabType == PrefabAssetType.Model || prefabType == PrefabAssetType.NotAPrefab)
                        {
                            prefabRoot = null;
                        }
                    }
                    else
                    {
                        Debug.Log("[Warning][The Vegetation Engine] " + "Prefabs with sub-asset such as Tree Creator of SpeedTree cannot be converted directly. Please create a new prefab from it first!");
                        prefabRoot = null;
                    }
                }
                else
                {
                    prefabRoot = null;
                }
            }

            return prefabRoot;
        }

        void GetPrefabObjects()
        {
            prefabObjects = new List<GameObject>();
            convertedPrefabCount = 0;

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                var prefabRoot = GetPrefabObjectRoot(Selection.gameObjects[i]);

                if (prefabRoot != null && prefabObjects.Contains(prefabRoot) == false)
                {
                    prefabObjects.Add(prefabRoot);
                }

                if (prefabRoot != null && prefabRoot.GetComponent<TVEPrefab>() != null && (prefabRoot.GetComponent<TVEPrefab>().storedPrefabBackup != null || prefabRoot.GetComponent<TVEPrefab>().storedPrefabBackupGUID != ""))
                {
                    convertedPrefabCount = 1;
                }
            }
        }

        void GetPrefabPresets()
        {
            if (prefabObjects.Count > 0)
            {
                var prefabComponent = prefabObjects[0].GetComponent<TVEPrefab>();

                if (prefabComponent != null)
                {
                    if (prefabComponent.storedPreset != null && prefabComponent.storedPreset != "")
                    {
                        for (int i = 0; i < PresetsEnum.Length; i++)
                        {
                            if (prefabComponent.storedPreset == PresetsEnum[i])
                            {
                                presetIndex = i;
                            }
                        }
                    }

                    if (prefabComponent.storedOverrides != null && prefabComponent.storedOverrides.Count > 0)
                    {
                        for (int i = 0; i < prefabComponent.storedOverrides.Count; i++)
                        {
                            for (int j = 0; j < OverridesEnum.Length; j++)
                            {
                                if (prefabComponent.storedOverrides[i] == OverridesEnum[j])
                                {
                                    if (overrideIndices.Count < prefabComponent.storedOverrides.Count)
                                    {
                                        overrideIndices.Add(j);
                                    }
                                }
                            }
                        }
                    }

                    presetAutoDetected = false;
                }
                else
                {
                    // Try to autodetect preset
                    for (int i = 0; i < detectLines.Count; i++)
                    {
                        if (detectLines[i].StartsWith("Detect"))
                        {
                            var detect = detectLines[i].Replace("Detect ", "");
                            var preset = detectLines[i + 1].Replace("Preset ", "").Replace(" - ", "/");

                            if (AssetDatabase.GetAssetPath(prefabObjects[0]).Contains(detect))
                            {
                                for (int j = 0; j < PresetsEnum.Length; j++)
                                {
                                    if (PresetsEnum[j] == preset)
                                    {
                                        presetIndex = j;
                                        presetAutoDetected = true;

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void CreatePrefabDataFolder()
        {
            string dataPath;
            string savePath = "/" + prefabName;

            if (collectConvertedData)
            {
                dataPath = projectDataFolder + PREFABS_DATA_PATH + savePath;
            }
            else
            {
                dataPath = AssetDatabase.GetAssetPath(prefabObject);
                dataPath = Path.GetDirectoryName(dataPath);
                dataPath = dataPath + savePath;
                prefabDataFolder = dataPath;
            }

            if (Directory.Exists(dataPath) == false)
            {
                Directory.CreateDirectory(dataPath);
                AssetDatabase.Refresh();
            }
        }

        //void DeletePrefabDataFolder()
        //{
        //    var prefabPath = AssetDatabase.GetAssetPath(prefabObject);

        //    var folderPath = prefabPath.Replace(Path.GetFileName(prefabPath), "");
        //    folderPath = folderPath + prefabObject.name;

        //    if (Directory.Exists(folderPath))
        //    {
        //        var allFolderFiles = Directory.GetFiles(folderPath);

        //        for (int i = 0; i < allFolderFiles.Length; i++)
        //        {
        //            if (allFolderFiles[i].Contains("TVE"))
        //            {
        //                FileUtil.DeleteFileOrDirectory(allFolderFiles[i]);
        //            }
        //        }

        //        allFolderFiles = Directory.GetFiles(folderPath);

        //        if (allFolderFiles == null || allFolderFiles.Length == 0)
        //        {
        //            FileUtil.DeleteFileOrDirectory(folderPath);
        //        }

        //        AssetDatabase.Refresh();
        //    }
        //}

        void CreatePrefabBackupFile()
        {
            string dataPath;
            string savePath = "/" + prefabName + " (TVE Backup).prefab";

            if (collectConvertedData)
            {
                dataPath = projectDataFolder + BACKUP_DATA_PATH + savePath;
            }
            else
            {
                dataPath = prefabDataFolder + savePath;
            }

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(prefabObject), dataPath);
            AssetDatabase.Refresh();

            prefabBackup = AssetDatabase.LoadAssetAtPath<GameObject>(dataPath);
        }

        GameObject GetPrefabBackupFile(GameObject prefabInstance)
        {
            GameObject prefabBackupGO = null;

            var prefabBackup = prefabInstance.GetComponent<TVEPrefab>().storedPrefabBackup;
            var prefabBackupGUID = prefabInstance.GetComponent<TVEPrefab>().storedPrefabBackupGUID;

            if (prefabBackupGUID != null || prefabBackupGUID != "")
            {
                var prefabBackupPath = AssetDatabase.GUIDToAssetPath(prefabBackupGUID);
                prefabBackupGO = AssetDatabase.LoadAssetAtPath<GameObject>(prefabBackupPath);
            }

            if (prefabBackup != null)
            {
                prefabBackupGO = prefabBackup;
            }

            return prefabBackupGO;
        }

        void GetGameObjectsInPrefab()
        {
            gameObjectsInPrefab = new List<GameObject>();
            gameObjectsInPrefab.Add(prefabInstance);

            GetChildRecursive(prefabInstance);

            //for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            //{
            //    Debug.Log(gameObjectsInPrefab[i]);
            //}
        }

        void GetChildRecursive(GameObject go)
        {
            foreach (Transform child in go.transform)
            {
                if (child == null)
                    continue;

                gameObjectsInPrefab.Add(child.gameObject);
                GetChildRecursive(child.gameObject);
            }
        }

        void GetChildRecursive(GameObject go, List<GameObject> gameObjects)
        {
            foreach (Transform child in go.transform)
            {
                if (child == null)
                    continue;

                gameObjects.Add(child.gameObject);
                GetChildRecursive(child.gameObject, gameObjects);
            }
        }

        void DisableInvalidGameObjectsInPrefab()
        {
            for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            {
                if (gameObjectsInPrefab[i].name.Contains("Impostor") == true)
                {
                    gameObjectsInPrefab[i].SetActive(false);
                    Debug.Log("[Warning][The Vegetation Engine] " + "Impostor Mesh are not supported! The " + gameObjectsInPrefab[i].name + " gameobject remains unchanged!");
                }

                if (gameObjectsInPrefab[i].GetComponent<BillboardRenderer>() != null)
                {
                    gameObjectsInPrefab[i].SetActive(false);
                    Debug.Log("[Warning][The Vegetation Engine] " + "Billboard Renderers are not supported! The " + gameObjectsInPrefab[i].name + " gameobject has been disabled. You can manually enable them after the conversion is done!");
                }

                if (gameObjectsInPrefab[i].GetComponent<MeshRenderer>() != null)
                {
                    var material = gameObjectsInPrefab[i].GetComponent<MeshRenderer>().sharedMaterial;

                    if (material != null)
                    {
                        if (material.shader.name.Contains("BK/Billboards"))
                        {
                            gameObjectsInPrefab[i].SetActive(false);
                            Debug.Log("[Warning][The Vegetation Engine] " + "BK Billboard Renderers are not supported! The " + gameObjectsInPrefab[i].name + " gameobject has been disabled. You can manually enable them after the conversion is done!");
                        }
                    }
                }

                if (gameObjectsInPrefab[i].GetComponent<Tree>() != null)
                {
                    DestroyImmediate(gameObjectsInPrefab[i].GetComponent<Tree>());
                }
            }
        }

        void EnableInvalidGameObjectsInPrefab()
        {
            for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            {
                if (gameObjectsInPrefab[i].name.Contains("Impostor") == true)
                {
                    gameObjectsInPrefab[i].SetActive(true);
                }
            }
        }

        void GetTransformsInPrefab()
        {
            transformPositions = new List<Vector3>();
            transformRotations = new List<Vector3>();
            transformScales = new List<Vector3>();

            for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            {
                transformPositions.Add(gameObjectsInPrefab[i].transform.localPosition);
                transformRotations.Add(gameObjectsInPrefab[i].transform.localEulerAngles);
                transformScales.Add(gameObjectsInPrefab[i].transform.localScale);
            }
        }

        void GetMeshRenderersInPrefab()
        {
            meshRenderersInPrefab = new List<MeshRenderer>();

            for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            {
                if (IsValidGameObject(gameObjectsInPrefab[i]) && gameObjectsInPrefab[i].GetComponent<MeshRenderer>() != null)
                {
                    meshRenderersInPrefab.Add(gameObjectsInPrefab[i].GetComponent<MeshRenderer>());
                }
                else
                {
                    meshRenderersInPrefab.Add(null);
                }
            }

            //for (int i = 0; i < meshRenderersInPrefab.Count; i++)
            //{
            //    Debug.Log(meshRenderersInPrefab[i]);
            //}
        }

        void GetMaterialArraysInPrefab()
        {
            materialArraysInPrefab = new List<Material[]>();

            for (int i = 0; i < meshRenderersInPrefab.Count; i++)
            {
                if (meshRenderersInPrefab[i] != null)
                {
                    materialArraysInPrefab.Add(meshRenderersInPrefab[i].sharedMaterials);
                }
                else
                {
                    materialArraysInPrefab.Add(null);
                }
            }
        }

        void CreateMaterialArraysInstances()
        {
            materialArraysInstances = new List<Material[]>();

            for (int i = 0; i < materialArraysInPrefab.Count; i++)
            {
                if (materialArraysInPrefab[i] != null)
                {
                    var materials = materialArraysInPrefab[i];
                    var materialsInstances = new Material[materials.Length];

                    for (int j = 0; j < materials.Length; j++)
                    {
                        if (materials[j] != null)
                        {
                            if (materials[j].name.Contains("Impostor") == true)
                            {
                                materialsInstances[j] = materials[j];
                            }
                            else
                            {
                                materialsInstances[j] = new Material(shaderLeaf);
                                materialsInstances[j].name = materials[j].name;
                            }
                        }
                        else
                        {
                            materialsInstances[j] = null;
                        }
                    }

                    materialArraysInstances.Add(materialsInstances);
                }
                else
                {
                    materialArraysInstances.Add(null);
                }
            }
        }

        void GetMeshFiltersInPrefab()
        {
            meshFiltersInPrefab = new List<MeshFilter>();

            for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            {
                if (IsValidGameObject(gameObjectsInPrefab[i]) && gameObjectsInPrefab[i].GetComponent<MeshFilter>() != null)
                {
                    meshFiltersInPrefab.Add(gameObjectsInPrefab[i].GetComponent<MeshFilter>());
                }
                else
                {
                    meshFiltersInPrefab.Add(null);
                }
            }

            //for (int i = 0; i < meshFiltersInPrefab.Count; i++)
            //{
            //    Debug.Log(meshFiltersInPrefab[i].sharedMesh);
            //}
        }

        void GetMeshesInPrefab()
        {
            meshesInPrefab = new List<Mesh>();

            for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            {
                if (meshFiltersInPrefab[i] != null)
                {
                    meshesInPrefab.Add(meshFiltersInPrefab[i].sharedMesh);
                }
                else
                {
                    meshesInPrefab.Add(null);
                }
            }

            //for (int i = 0; i < meshFiltersInPrefab.Count; i++)
            //{
            //    Debug.Log(meshFiltersInPrefab[i].sharedMesh);
            //}
        }

        void CreateMeshInstances()
        {
            meshInstances = new List<Mesh>();

            for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            {
                if (meshesInPrefab[i] != null)
                {
                    var meshPath = AssetDatabase.GetAssetPath(meshesInPrefab[i]);

                    if (meshesInPrefab[i].isReadable == true)
                    {
                        var meshInstance = Instantiate(meshesInPrefab[i]);
                        meshInstance.name = meshesInPrefab[i].name;
                        meshInstances.Add(meshInstance);
                    }
                    else
                    {
                        //Workaround when the mesh is not readable (Unity Bug)
                        ModelImporter modelImporter = AssetImporter.GetAtPath(meshPath) as ModelImporter;

                        if (modelImporter != null)
                        {
                            modelImporter.isReadable = true;
                            modelImporter.SaveAndReimport();
                        }

                        if (meshPath.EndsWith(".asset"))
                        {
                            string filePath = Path.Combine(Directory.GetCurrentDirectory(), meshPath);
                            filePath = filePath.Replace("/", "\\");
                            string fileText = File.ReadAllText(filePath);
                            fileText = fileText.Replace("m_IsReadable: 0", "m_IsReadable: 1");
                            File.WriteAllText(filePath, fileText);
                            AssetDatabase.Refresh();
                        }

                        var meshInstance = Instantiate(meshesInPrefab[i]);
                        meshInstance.name = meshesInPrefab[i].name;
                        meshInstances.Add(meshInstance);
                    }
                }
                else
                {
                    meshInstances.Add(null);
                }
            }

            //for (int i = 0; i < meshFiltersInPrefab.Count; i++)
            //{
            //    Debug.Log(meshFiltersInPrefab[i].sharedMesh);
            //}
        }

        void TransformMeshesToWorldSpace()
        {
            //for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            //{
            //    // Reset only the pos/scale for meshes to capture the WS rotation
            //    // Reset all transform for gos without mesh for proper children baking
            //    if (meshInstances[i] != null)
            //    {
            //        //gameObjectsInPrefab[i].transform.localPosition = Vector3.zero;
            //        gameObjectsInPrefab[i].transform.localScale = Vector3.one;
            //    }
            //    else
            //    {
            //        //gameObjectsInPrefab[i].transform.localPosition = Vector3.zero;
            //        gameObjectsInPrefab[i].transform.localEulerAngles = Vector3.zero;
            //        gameObjectsInPrefab[i].transform.localScale = Vector3.one;
            //    }
            //}

            for (int i = 0; i < gameObjectsInPrefab.Count; i++)
            {
                if (meshInstances[i] != null)
                {
                    var meshInstance = meshInstances[i];
                    var transforms = gameObjectsInPrefab[i].transform;

                    Vector3[] verticesOS = meshInstance.vertices;
                    Vector3[] verticesWS = new Vector3[meshInstance.vertices.Length];

                    // Transform vertioces OS pos to WS pos
                    for (int j = 0; j < verticesOS.Length; j++)
                    {
                        verticesWS[j] = transforms.TransformPoint(verticesOS[j]);
                    }

                    meshInstances[i].vertices = verticesWS;

                    // Some meshes don't have normals, check is needed
                    if (meshInstance.normals != null && meshInstance.normals.Length > 0)
                    {
                        Vector3[] normalsOS = meshInstance.normals;
                        Vector3[] normalsWS = new Vector3[meshInstance.vertices.Length];

                        for (int j = 0; j < normalsOS.Length; j++)
                        {
                            normalsWS[j] = transforms.TransformDirection(normalsOS[j]);
                        }

                        meshInstances[i].normals = normalsWS;
                    }

                    //// Some meshes don't have tangenst, check is needed
                    //if (meshInstance.tangents != null && meshInstance.tangents.Length > 0)
                    //{
                    //    Vector4[] tangentsOS = meshInstance.tangents;
                    //    Vector4[] tangentsWS = new Vector4[meshInstance.vertices.Length];

                    //    for (int j = 0; j < tangentsOS.Length; j++)
                    //    {
                    //        var tan = transforms.TransformPoint(tangentsOS[j]);
                    //        tangentsWS[j] = new Vector4(tan.x, tan.y, tan.z, tangentsOS[j].w);
                    //    }

                    //    meshInstances[i].tangents = tangentsWS;
                    //}

                    meshInstances[i].RecalculateTangents();
                    meshInstances[i].RecalculateBounds();

                    gameObjectsInPrefab[i].transform.localPosition = Vector3.zero;
                    gameObjectsInPrefab[i].transform.localEulerAngles = Vector3.zero;
                    gameObjectsInPrefab[i].transform.localScale = Vector3.one;
                }
            }
        }

        //void ApplyOriginalTransforms()
        //{
        //    //for (int i = 0; i < gameObjectsInPrefab.Count; i++)
        //    //{
        //    //    // Apply original pos/scale only for meshes
        //    //    // Apply all original transforms for nonmeshes
        //    //    if (meshInstances[i] != null)
        //    //    {
        //    //        //gameObjectsInPrefab[i].transform.localPosition = transformPositions[i];
        //    //        gameObjectsInPrefab[i].transform.localScale = transformScales[i];
        //    //    }
        //    //    else
        //    //    {
        //    //        //gameObjectsInPrefab[i].transform.localPosition = transformPositions[i];
        //    //        gameObjectsInPrefab[i].transform.localEulerAngles = transformRotations[i];
        //    //        gameObjectsInPrefab[i].transform.localScale = transformScales[i];
        //    //    }
        //    //}
        //}

        void GetMaxBoundsInPrefab()
        {
            maxBoundsInfo = Vector4.zero;

            var bounds = new Bounds(Vector3.zero, Vector3.zero);

            for (int i = 0; i < meshInstances.Count; i++)
            {
                if (meshInstances[i] != null)
                {
                    bounds.Encapsulate(meshInstances[i].bounds);
                }
            }

            var maxX = Mathf.Max(Mathf.Abs(bounds.min.x), Mathf.Abs(bounds.max.x));
            var maxZ = Mathf.Max(Mathf.Abs(bounds.min.z), Mathf.Abs(bounds.max.z));

            var maxR = Mathf.Max(maxX, maxZ);
            var maxH = Mathf.Max(Mathf.Abs(bounds.min.y), Mathf.Abs(bounds.max.y));
            var maxS = Mathf.Max(maxR, maxH);

            maxBoundsInfo = new Vector4(maxR, maxH, maxS, 0.0f);

            //Debug.Log(maxBoundsInfo);
        }

        bool IsValidGameObject(GameObject gameObject)
        {
            bool valid = true;

            if (gameObject.activeInHierarchy == false)
            {
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Mesh Packing Macros
        /// </summary>

        void GetMeshConversionFromPreset()
        {
            for (int i = 0; i < presetLines.Count; i++)
            {
                if (presetLines[i].StartsWith("Mesh"))
                {
                    string[] splitLine = presetLines[i].Split(char.Parse(" "));
                    string name = "";
                    string source = "";
                    int sourceIndex = 0;
                    string option = "";
                    int optionIndex = 0;
                    string action = "";
                    int actionIndex = 0;

                    if (splitLine.Length > 1)
                    {
                        name = splitLine[1];
                    }

                    if (splitLine.Length > 2)
                    {
                        source = splitLine[2];

                        if (source == "NONE")
                        {
                            sourceIndex = 0;
                        }

                        if (source == "AUTO")
                        {
                            sourceIndex = 0;
                        }

                        // Available options for Float masks
                        if (source == "GET_MASK_FROM_CHANNEL")
                        {
                            sourceIndex = 1;
                        }

                        if (source == "GET_MASK_PROCEDURAL")
                        {
                            sourceIndex = 2;
                        }

                        if (source == "GET_MASK_FROM_TEXTURE")
                        {
                            sourceIndex = 3;
                        }

                        if (source == "GET_MASK_3RD_PARTY")
                        {
                            sourceIndex = 4;
                        }

                        // Available options for Coord masks
                        if (source == "GET_COORD_FROM_CHANNEL")
                        {
                            sourceIndex = 1;
                        }

                        if (source == "GET_COORD_PROCEDURAL")
                        {
                            sourceIndex = 2;
                        }

                        if (source == "GET_COORD_3RD_PARTY")
                        {
                            sourceIndex = 3;
                        }

                        // Available options for Coord masks
                        if (source == "GET_NORMALS_PROCEDURAL")
                        {
                            sourceIndex = 1;
                        }
                    }

                    if (splitLine.Length > 3)
                    {
                        option = splitLine[3];

                        optionIndex = int.Parse(option);
                    }

                    action = splitLine[splitLine.Length - 1];

                    if (action == "ACTION_INVERT")
                    {
                        actionIndex = 1;
                    }

                    if (action == "ACTION_REMAP01")
                    {
                        actionIndex = 2;
                    }

                    if (name == "SetVariation" || name == "SetRed")
                    {
                        sourceVariation = sourceIndex;
                        optionVariation = optionIndex;
                        actionVariation = actionIndex;
                    }

                    if (name == "SetOcclusion" || name == "SetGreen")
                    {
                        sourceOcclusion = sourceIndex;
                        optionOcclusion = optionIndex;
                        actionOcclusion = actionIndex;
                    }

                    if (name == "SetDetailMask" || name == "SetBlue")
                    {
                        sourceDetail = sourceIndex;
                        optionDetail = optionIndex;
                        actionDetail = actionIndex;
                    }

                    if (name == "SetMultiMask" || name == "SetAlpha")
                    {
                        sourceMulti = sourceIndex;
                        optionMulti = optionIndex;
                        actionMulti = actionIndex;
                    }

                    if (name == "SetDetailCoord" || name == "SetPivots")
                    {
                        sourceDetailCoord = sourceIndex;
                        optionDetailCoord = optionIndex;
                    }

                    if (name == "SetMotion1")
                    {
                        sourceMotion1 = sourceIndex;
                        optionMotion1 = optionIndex;
                        actionMotion1 = actionIndex;
                    }

                    if (name == "SetMotion2")
                    {
                        sourceMotion2 = sourceIndex;
                        optionMotion2 = optionIndex;
                        actionMotion2 = actionIndex;
                    }

                    if (name == "SetMotion3")
                    {
                        sourceMotion3 = sourceIndex;
                        optionMotion3 = optionIndex;
                        actionMotion3 = actionIndex;
                    }

                    if (name == "SetNormals")
                    {
                        sourceNormals = sourceIndex;
                        optionNormals = optionIndex;
                    }
                }
            }
        }

        void GetMeshConversionWithTextures(int meshIndex)
        {
            for (int i = 0; i < presetLines.Count; i++)
            {
                if (presetLines[i].StartsWith("Mesh"))
                {
                    string[] splitLine = presetLines[i].Split(char.Parse(" "));
                    string name = "";
                    string source = "";

                    string prop = "";
                    Texture2D texture = null;

                    if (splitLine.Length > 1)
                    {
                        name = splitLine[1];
                    }

                    if (splitLine.Length > 4)
                    {
                        source = splitLine[2];
                        prop = splitLine[4];

                        if (source == "GET_MASK_FROM_TEXTURE")
                        {
                            for (int t = 0; t < materialArraysInPrefab[meshIndex].Length; t++)
                            {
                                if (materialArraysInPrefab[meshIndex] != null)
                                {
                                    var material = materialArraysInPrefab[meshIndex][t];

                                    if (material != null && material.HasProperty(prop))
                                    {
                                        texture = (Texture2D)material.GetTexture(prop);
                                        break;
                                    }
                                }
                            }

                            if (name == "SetVariation" || name == "SetRed")
                            {
                                textureVariation = texture;
                            }

                            if (name == "SetOcclusion" || name == "SetGreen")
                            {
                                textureOcclusion = texture;
                            }

                            if (name == "SetDetailMask" || name == "SetBlue")
                            {
                                textureDetail = texture;
                            }

                            if (name == "SetMultiMask" || name == "SetAlpha")
                            {
                                textureMulti = texture;
                            }

                            if (name == "SetMotion1")
                            {
                                textureMotion1 = texture;
                            }

                            if (name == "SetMotion2")
                            {
                                textureMotion2 = texture;
                            }

                            if (name == "SetMotion3")
                            {
                                textureMotion3 = texture;
                            }
                        }
                    }
                }
            }
        }

        void ConvertMeshes()
        {
            if (outputMeshIndex == OutputMesh.Default)
            {
                for (int i = 0; i < meshInstances.Count; i++)
                {
                    if (meshInstances[i] != null)
                    {
                        GetMeshConversionWithTextures(i);
                        ConvertMeshDefault(meshInstances[i]);
                        meshFiltersInPrefab[i].sharedMesh = convertedMesh;
                    }
                }
            }
            else if (outputMeshIndex == OutputMesh.Custom)
            {
                for (int i = 0; i < meshInstances.Count; i++)
                {
                    if (meshInstances[i] != null)
                    {
                        GetMeshConversionWithTextures(i);
                        ConvertMeshCustom(meshInstances[i]);
                        meshFiltersInPrefab[i].sharedMesh = convertedMesh;
                    }
                }
            }
            else if (outputMeshIndex == OutputMesh.Detail)
            {
                for (int i = 0; i < meshInstances.Count; i++)
                {
                    if (meshInstances[i] != null)
                    {
                        GetMeshConversionWithTextures(i);
                        ConvertMeshDetail(meshInstances[i]);
                        meshFiltersInPrefab[i].sharedMesh = convertedMesh;
                    }
                }
            }
            else if (outputMeshIndex == OutputMesh.DEEnvironment)
            {
                for (int i = 0; i < meshInstances.Count; i++)
                {
                    if (meshInstances[i] != null)
                    {
                        GetMeshConversionWithTextures(i);
                        ConvertMeshDEEnvironment(meshInstances[i]);
                        meshFiltersInPrefab[i].sharedMesh = convertedMesh;
                    }
                }
            }
            else if (outputMeshIndex == OutputMesh.Polyverse)
            {
                for (int i = 0; i < meshInstances.Count; i++)
                {
                    if (meshInstances[i] != null)
                    {
                        GetMeshConversionWithTextures(i);
                        ConvertMeshPolyverse(meshInstances[i]);
                        meshFiltersInPrefab[i].sharedMesh = convertedMesh;
                    }
                }
            }
        }

        void ConvertMeshDefault(Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;

            var colors = new Color[vertexCount];

            var UV0 = GetCoordData(mesh, 0, 0);
            var UV4 = GetCoordData(mesh, 0, 0);

            var multiMask = new List<float>(vertexCount);

            if (sourceMulti == 0)
            {
                multiMask = GetMaskData(mesh, 2, 4, textureMulti, 0, 1.0f);
            }
            else
            {
                multiMask = GetMaskData(mesh, sourceMulti, optionMulti, textureMulti, actionMulti, 1.0f);
            }

            var occlusion = GetMaskData(mesh, sourceOcclusion, optionOcclusion, textureOcclusion, actionOcclusion, 1.0f);
            var detailMask = GetMaskData(mesh, sourceDetail, optionDetail, textureDetail, actionDetail, 1.0f);
            var variation = GetMaskData(mesh, sourceVariation, optionVariation, textureVariation, actionVariation, 1.0f);

            var detailCoordOrPivots = GetCoordData(mesh, sourceDetailCoord, optionDetailCoord);

            var motion1 = GetMaskData(mesh, sourceMotion1, optionMotion1, textureMotion1, actionMotion1, 1.0f);
            var motion2 = GetMaskData(mesh, sourceMotion2, optionMotion2, textureMotion2, actionMotion2, 1.0f);
            var motion3 = GetMaskData(mesh, sourceMotion3, optionMotion3, textureMotion3, actionMotion3, 1.0f);

            for (int i = 0; i < vertexCount; i++)
            {
                colors[i] = new Color(variation[i], occlusion[i], detailMask[i], multiMask[i]);
                UV0[i] = new Vector4(UV0[i].x, UV0[i].y, detailCoordOrPivots[i].x, detailCoordOrPivots[i].y);
                UV4[i] = new Vector4(motion1[i], motion2[i], motion3[i], 0);
            }

            mesh.colors = colors;
            mesh.SetUVs(0, UV0);
            mesh.SetUVs(3, UV4);

            GetNormalsData(mesh, sourceNormals, optionNormals);

            SaveMesh(mesh);
        }

        void ConvertMeshCustom(Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;

            var colors = new Color[vertexCount];

            var red = GetMaskData(mesh, sourceVariation, optionVariation, textureVariation, actionVariation, 1.0f);
            var green = GetMaskData(mesh, sourceOcclusion, optionOcclusion, textureOcclusion, actionOcclusion, 1.0f);
            var blue = GetMaskData(mesh, sourceDetail, optionDetail, textureDetail, actionDetail, 1.0f);
            var alpha = GetMaskData(mesh, sourceMulti, optionMulti, textureMulti, actionMulti, 1.0f);

            for (int i = 0; i < vertexCount; i++)
            {
                colors[i] = new Color(red[i], green[i], blue[i], alpha[i]);
            }

            mesh.colors = colors;

            GetNormalsData(mesh, sourceNormals, optionNormals);

            SaveMesh(mesh);
        }

        void ConvertMeshDetail(Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;

            var colors = new Color[vertexCount];

            var multiMask = new List<float>(vertexCount);

            if (sourceMulti == 0)
            {
                multiMask = GetMaskData(mesh, 2, 4, textureMulti, 0, 1.0f);
            }
            else
            {
                multiMask = GetMaskData(mesh, sourceMulti, optionMulti, textureMulti, actionMulti, 1.0f);
            }

            for (int i = 0; i < vertexCount; i++)
            {
                colors[i] = new Color(1, 1, 1, multiMask[i]);
            }

            mesh.colors = colors;

            SaveMesh(mesh);
        }

        void ConvertMeshDEEnvironment(Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;

            var colors = new Color[vertexCount];

            var occlusion = GetMaskData(mesh, sourceOcclusion, optionOcclusion, textureOcclusion, actionOcclusion, 1.0f);
            var variation = GetMaskData(mesh, sourceVariation, optionVariation, textureVariation, actionVariation, 1.0f);
            var motion1 = GetMaskData(mesh, sourceMotion1, optionMotion1, textureMotion1, actionMotion1, 1.0f);
            var motion2 = GetMaskData(mesh, sourceMotion2, optionMotion2, textureMotion2, actionMotion2, 1.0f);

            for (int i = 0; i < vertexCount; i++)
            {
                colors[i] = new Color(motion1[i], variation[i], motion2[i], occlusion[i]);
            }

            mesh.colors = colors;

            GetNormalsData(mesh, sourceNormals, optionNormals);

            SaveMesh(mesh);
        }

        void ConvertMeshPolyverse(Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;

            var UV0 = GetCoordData(mesh, 0, 0);
            var UV4 = GetCoordData(mesh, 0, 0);

            var vertices = mesh.vertices;

            //var height = GetMaskData(mesh, 2, 4, 0, 1.0f);
            var multiMask = GetMaskData(mesh, sourceMulti, optionMulti, textureMulti, actionMulti, 1.0f);
            var motion1 = GetMaskData(mesh, sourceMotion1, optionMotion1, textureMotion1, actionMotion1, 1.0f);
            var motion2 = GetMaskData(mesh, sourceMotion2, optionMotion2, textureMotion2, actionMotion2, 1.0f);

            for (int i = 0; i < vertexCount; i++)
            {
                UV0[i] = new Vector4(UV0[i].x, UV0[i].y, motion1[i], motion2[i]);
                UV4[i] = new Vector4(vertices[i].x, vertices[i].y, vertices[i].z, multiMask[i]);
            }

            mesh.SetUVs(0, UV0);
            mesh.SetUVs(3, UV4);

            GetNormalsData(mesh, sourceNormals, optionNormals);

            SaveMesh(mesh);
        }

        void SaveMesh(Mesh mesh)
        {
            string dataPath;
            string savePath = "/" + mesh.name.Replace(":", "") + " (" + outputSuffix + " Mesh).mesh";

            mesh.UploadMeshData(true);

            if (collectConvertedData)
            {
                //if (shareCommonAssets)
                //{
                //    dataPath = projectDataFolder + SHARED_DATA_PATH + savePath;
                //}
                //else
                {
                    dataPath = projectDataFolder + PREFABS_DATA_PATH + "/" + prefabName + savePath;
                }
            }
            else
            {
                dataPath = prefabDataFolder + savePath;
            }

            if (!File.Exists(dataPath))
            {
                AssetDatabase.CreateAsset(mesh, dataPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                var tveMeshFile = AssetDatabase.LoadAssetAtPath<Mesh>(dataPath);
                tveMeshFile.Clear();
                EditorUtility.CopySerialized(mesh, tveMeshFile);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            convertedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(dataPath);
        }

        // Get Float data
        List<float> GetMaskData(Mesh mesh, int source, int option, Texture2D texture, int action, float defaulValue)
        {
            var meshChannel = new List<float>();

            if (source == 0)
            {
                meshChannel = GetMaskDefaultValue(mesh, defaulValue);
            }

            else if (source == 1)
            {
                meshChannel = GetMaskMeshData(mesh, option, defaulValue);
            }

            else if (source == 2)
            {
                meshChannel = GetMaskProceduralData(mesh, option);
            }

            else if (source == 3)
            {
                meshChannel = GetMaskFromTextureData(mesh, option, texture);
            }

            else if (source == 4)
            {
                meshChannel = GetMask3rdPartyData(mesh, option);
            }

            if (action > 0)
            {
                meshChannel = MathAction(meshChannel, action);
            }

            return meshChannel;
        }

        List<float> GetMaskDefaultValue(Mesh mesh, float defaulValue)
        {
            var vertexCount = mesh.vertexCount;

            var meshChannel = new List<float>(vertexCount);

            for (int i = 0; i < vertexCount; i++)
            {
                meshChannel.Add(defaulValue);
            }

            return meshChannel;
        }

        List<float> GetMaskMeshData(Mesh mesh, int option, float defaulValue)
        {
            var vertexCount = mesh.vertexCount;

            var meshChannel = new List<float>(vertexCount);

            // Vertex Color Data
            if (option == 0)
            {
                var channel = new List<Color>(vertexCount);
                mesh.GetColors(channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].r);
                }
            }

            else if (option == 1)
            {
                var channel = new List<Color>(vertexCount);
                mesh.GetColors(channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].g);
                }
            }

            else if (option == 2)
            {
                var channel = new List<Color>(vertexCount);
                mesh.GetColors(channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].b);
                }
            }

            else if (option == 3)
            {
                var channel = new List<Color>(vertexCount);
                mesh.GetColors(channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].a);
                }
            }

            // UV 0 Data
            else if (option == 4)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(0, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].x);
                }
            }

            else if (option == 5)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(0, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].y);
                }
            }

            else if (option == 6)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(0, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].z);
                }
            }

            else if (option == 7)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(0, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].w);
                }
            }

            // UV 2 Data
            else if (option == 8)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(1, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].x);
                }
            }

            else if (option == 9)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(1, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].y);
                }
            }

            else if (option == 10)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(1, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].z);
                }
            }

            else if (option == 11)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(1, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].w);
                }
            }

            // UV 3 Data
            else if (option == 12)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(2, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].x);
                }
            }

            else if (option == 13)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(2, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].y);
                }
            }

            else if (option == 14)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(2, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].z);
                }
            }

            else if (option == 15)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(2, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].w);
                }
            }

            // UV 4 Data
            else if (option == 16)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(3, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].x);
                }
            }

            else if (option == 17)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(3, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].y);
                }
            }

            else if (option == 18)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(3, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].z);
                }
            }

            else if (option == 19)
            {
                var channel = new List<Vector4>(vertexCount);
                mesh.GetUVs(3, channel);
                var channelCount = channel.Count;

                for (int i = 0; i < channelCount; i++)
                {
                    meshChannel.Add(channel[i].w);
                }
            }

            if (meshChannel.Count == 0)
            {
                meshChannel = GetMaskDefaultValue(mesh, defaulValue);

                //Debug.Log("[The Vegetation Engine] " + mesh.name + " does not have " + SourceMaskMeshEnum[option] + ". A default value has been assigned instead!");
            }

            //meshChannel = HandleMeshAction(meshChannel);

            return meshChannel;
        }

        List<float> GetMaskProceduralData(Mesh mesh, int option)
        {
            var vertexCount = mesh.vertexCount;
            var vertices = mesh.vertices;
            var normals = mesh.normals;

            var meshChannel = new List<float>(vertexCount);

            if (option == 0)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    meshChannel.Add(0.0f);
                }
            }
            else if (option == 1)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    meshChannel.Add(1.0f);
                }
            }
            // Random Variation
            else if (option == 2)
            {
                // Good Enough approach
                var triangles = mesh.triangles;
                var trianglesCount = mesh.triangles.Length;

                for (int i = 0; i < vertexCount; i++)
                {
                    meshChannel.Add(-99);
                }

                for (int i = 0; i < trianglesCount; i += 3)
                {
                    var index1 = triangles[i + 0];
                    var index2 = triangles[i + 1];
                    var index3 = triangles[i + 2];

                    float variation = 0;

                    if (meshChannel[index1] != -99)
                    {
                        variation = meshChannel[index1];
                    }
                    else if (meshChannel[index2] != -99)
                    {
                        variation = meshChannel[index2];
                    }
                    else if (meshChannel[index3] != -99)
                    {
                        variation = meshChannel[index3];
                    }
                    else
                    {
                        variation = UnityEngine.Random.Range(0.0f, 1.0f);
                    }

                    meshChannel[index1] = variation;
                    meshChannel[index2] = variation;
                    meshChannel[index3] = variation;
                }
            }
            // Predictive Variation
            else if (option == 3)
            {
                var triangles = mesh.triangles;
                var trianglesCount = mesh.triangles.Length;

                var elementIndices = new List<int>(vertexCount);
                int elementCount = 0;

                for (int i = 0; i < vertexCount; i++)
                {
                    elementIndices.Add(-99);
                }

                for (int i = 0; i < trianglesCount; i += 3)
                {
                    var index1 = triangles[i + 0];
                    var index2 = triangles[i + 1];
                    var index3 = triangles[i + 2];

                    int element = 0;

                    if (elementIndices[index1] != -99)
                    {
                        element = elementIndices[index1];
                    }
                    else if (elementIndices[index2] != -99)
                    {
                        element = elementIndices[index2];
                    }
                    else if (elementIndices[index3] != -99)
                    {
                        element = elementIndices[index3];
                    }
                    else
                    {
                        element = elementCount;
                        elementCount++;
                    }

                    elementIndices[index1] = element;
                    elementIndices[index2] = element;
                    elementIndices[index3] = element;
                }

                for (int i = 0; i < elementIndices.Count; i++)
                {
                    var variation = (float)elementIndices[i] / elementCount;
                    variation = Mathf.Repeat(variation * seed, 1.0f);
                    meshChannel.Add(variation);
                }
            }
            // Normalized in bounds height
            else if (option == 4)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var mask = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);

                    meshChannel.Add(mask);
                }
            }
            // Procedural Sphere
            else if (option == 5)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var mask = Mathf.Clamp01(Vector3.Distance(vertices[i], Vector3.zero) / maxBoundsInfo.x);

                    meshChannel.Add(mask);
                }
            }
            // Procedural Cylinder no Cap
            else if (option == 6)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var mask = Mathf.Clamp01(MathRemap(Vector3.Distance(vertices[i], new Vector3(0, vertices[i].y, 0)), maxBoundsInfo.x * 0.1f, maxBoundsInfo.x, 0f, 1f));

                    meshChannel.Add(mask);
                }
            }
            // Procedural Capsule
            else if (option == 7)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var maskCyl = Mathf.Clamp01(MathRemap(Vector3.Distance(vertices[i], new Vector3(0, vertices[i].y, 0)), maxBoundsInfo.x * 0.1f, maxBoundsInfo.x, 0f, 1f));
                    var maskCap = Vector3.Magnitude(new Vector3(0, Mathf.Clamp01(MathRemap(vertices[i].y / maxBoundsInfo.y, 0.8f, 1f, 0f, 1f)), 0));
                    var maskBase = Mathf.Clamp01(MathRemap(vertices[i].y / maxBoundsInfo.y, 0f, 0.1f, 0f, 1f));
                    var mask = Mathf.Clamp01(maskCyl + maskCap) * maskBase;

                    meshChannel.Add(mask);
                }
            }
            // Bottom To Top
            else if (option == 8)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var mask = 1.0f - Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);

                    meshChannel.Add(mask);
                }
            }
            // Top To Bottom
            else if (option == 9)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var mask = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);

                    meshChannel.Add(mask);
                }
            }
            // Bottom Projection
            else if (option == 10)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var mask = Mathf.Clamp01(Vector3.Dot(new Vector3(0, -1, 0), normals[i]) * 0.5f + 0.5f);

                    meshChannel.Add(mask);
                }
            }
            // Top Projection
            else if (option == 11)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var mask = Mathf.Clamp01(Vector3.Dot(new Vector3(0, 1, 0), normals[i]) * 0.5f + 0.5f);

                    meshChannel.Add(mask);
                }
            }
            // Height Exp
            else if (option == 12)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var oneMinusMask = 1 - Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);
                    var powerMask = oneMinusMask * oneMinusMask * oneMinusMask * oneMinusMask;
                    var mask = 1 - powerMask;

                    meshChannel.Add(mask);
                }
            }
            //Hemi Sphere
            else if (option == 13)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var height = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);
                    var sphere = Mathf.Clamp01(Vector3.Distance(vertices[i], Vector3.zero) / maxBoundsInfo.x);
                    var mask = height * sphere;

                    meshChannel.Add(mask);
                }
            }
            //Hemi Cylinder
            else if (option == 14)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var height = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);
                    var cyl = Mathf.Clamp01(MathRemap(Vector3.Distance(vertices[i], new Vector3(0, vertices[i].y, 0)), maxBoundsInfo.x * 0.1f, maxBoundsInfo.x, 0f, 1f));
                    var mask = height * cyl;

                    meshChannel.Add(mask);
                }
            }
            //Hemi Capsule
            else if (option == 15)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var height = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);
                    var maskCyl = Mathf.Clamp01(MathRemap(Vector3.Distance(vertices[i], new Vector3(0, vertices[i].y, 0)), maxBoundsInfo.x * 0.1f, maxBoundsInfo.x, 0f, 1f));
                    var maskCap = Vector3.Magnitude(new Vector3(0, Mathf.Clamp01(MathRemap(vertices[i].y / maxBoundsInfo.y, 0.8f, 1f, 0f, 1f)), 0));
                    var maskBase = Mathf.Clamp01(MathRemap(vertices[i].y / maxBoundsInfo.y, 0f, 0.1f, 0f, 1f));
                    var mask = Mathf.Clamp01(maskCyl + maskCap) * maskBase * height;

                    meshChannel.Add(mask);
                }
            }
            // Normalized in bounds height with black Offset at the bottom
            else if (option == 16)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var height = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);
                    var mask = Mathf.Clamp01((height - 0.2f) / (1 - 0.2f));

                    meshChannel.Add(mask);
                }
            }
            // Normalized in bounds height with black Offset at the bottom
            else if (option == 17)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var height = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);
                    var mask = Mathf.Clamp01((height - 0.4f) / (1 - 0.4f));

                    meshChannel.Add(mask);
                }
            }
            // Normalized in bounds height with black Offset at the bottom
            else if (option == 18)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var height = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);
                    var mask = Mathf.Clamp01((height - 0.6f) / (1 - 0.6f));

                    meshChannel.Add(mask);
                }
            }

            return meshChannel;
        }

        List<float> GetMask3rdPartyData(Mesh mesh, int option)
        {
            var vertexCount = mesh.vertexCount;
            var vertices = mesh.vertices;

            var meshChannel = new List<float>();

            // CTI Leaves Mask
            if (option == 0)
            {
                var UV3 = mesh.uv3;

                for (int i = 0; i < vertexCount; i++)
                {
                    var pivotX = (Mathf.Repeat(UV3[i].x, 1.0f) * 2.0f) - 1.0f;
                    var pivotZ = (Mathf.Repeat(32768.0f * UV3[i].x, 1.0f) * 2.0f) - 1.0f;
                    var pivotY = Mathf.Sqrt(1.0f - Mathf.Clamp01(Vector2.Dot(new Vector2(pivotX, pivotZ), new Vector2(pivotX, pivotZ))));

                    var pivot = new Vector3(pivotX * UV3[i].y, pivotY * UV3[i].y, pivotZ * UV3[i].y);
                    var pos = vertices[i];

                    var mask = Vector3.Magnitude(pos - pivot) / (maxBoundsInfo.x * 1f);

                    meshChannel.Add(mask);
                }
            }
            // CTI Leaves Variation
            else if (option == 1)
            {
                var UV3 = mesh.uv3;

                for (int i = 0; i < vertexCount; i++)
                {
                    var pivotX = (Mathf.Repeat(UV3[i].x, 1.0f) * 2.0f) - 1.0f;
                    var pivotZ = (Mathf.Repeat(32768.0f * UV3[i].x, 1.0f) * 2.0f) - 1.0f;
                    var pivotY = Mathf.Sqrt(1.0f - Mathf.Clamp01(Vector2.Dot(new Vector2(pivotX, pivotZ), new Vector2(pivotX, pivotZ))));

                    var pivot = new Vector3(pivotX * UV3[i].y, pivotY * UV3[i].y, pivotZ * UV3[i].y);

                    var variX = Mathf.Repeat(pivot.x * 33.3f, 1.0f);
                    var variY = Mathf.Repeat(pivot.y * 33.3f, 1.0f);
                    var variZ = Mathf.Repeat(pivot.z * 33.3f, 1.0f);

                    var mask = variX + variY + variZ;

                    if (UV3[i].x < 0.01f)
                    {
                        mask = 0.0f;
                    }

                    meshChannel.Add(mask);
                }
            }
            // ST8 Leaves Mask
            else if (option == 2)
            {
                var UV2 = new List<Vector4>();
                var UV3 = new List<Vector4>();
                var UV4 = new List<Vector4>();

                mesh.GetUVs(1, UV2);
                mesh.GetUVs(2, UV3);
                mesh.GetUVs(3, UV4);

                if (UV4.Count != 0)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var anchor = new Vector3(UV2[i].z - vertices[i].x, UV2[i].w - vertices[i].y, UV3[i].w - vertices[i].z);
                        var length = Vector3.Magnitude(anchor);
                        var leaves = UV2[i].w * UV4[i].w;

                        var mask = (length * leaves) / maxBoundsInfo.x;

                        meshChannel.Add(mask);
                    }
                }
                else
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var mask = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);

                        meshChannel.Add(mask);
                    }
                }
            }
            // NM Leaves Mask
            else if (option == 3)
            {
                var tempColors = new List<Color>(vertexCount);
                mesh.GetColors(tempColors);

                if (tempColors.Count != 0)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        if (tempColors[i].a > 0.99f)
                        {
                            meshChannel.Add(0.0f);
                        }
                        else
                        {
                            meshChannel.Add(tempColors[i].a);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var mask = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);

                        meshChannel.Add(mask);
                    }
                }
            }

            return meshChannel;
        }

        List<float> GetMaskFromTextureData(Mesh mesh, int option, Texture2D texture)
        {
            var vertexCount = mesh.vertexCount;
            var meshChannel = new List<float>(vertexCount);

            if (texture == null)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    meshChannel.Add(1);
                }
            }
            else
            {
                string texPath = AssetDatabase.GetAssetPath(texture);
                TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

                texImporter.isReadable = true;
                texImporter.SaveAndReimport();
                AssetDatabase.Refresh();

                var meshCoord = new List<Vector2>(vertexCount);

                mesh.GetUVs(0, meshCoord);

                if (option == 0)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var pixel = texture.GetPixelBilinear(meshCoord[i].x, meshCoord[i].y);
                        meshChannel.Add(pixel.r);
                    }
                }

                else if (option == 1)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var pixel = texture.GetPixelBilinear(meshCoord[i].x, meshCoord[i].y);
                        meshChannel.Add(pixel.g);
                    }
                }

                else if (option == 2)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var pixel = texture.GetPixelBilinear(meshCoord[i].x, meshCoord[i].y);
                        meshChannel.Add(pixel.b);
                    }
                }

                else if (option == 3)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var pixel = texture.GetPixelBilinear(meshCoord[i].x, meshCoord[i].y);
                        meshChannel.Add(pixel.a);
                    }
                }

                texImporter.isReadable = false;
                texImporter.SaveAndReimport();
                AssetDatabase.Refresh();
            }

            return meshChannel;
        }

        List<Vector4> GetCoordData(Mesh mesh, int source, int option)
        {
            var vertexCount = mesh.vertexCount;

            var meshCoord = new List<Vector4>(vertexCount);

            if (source == 0)
            {
                mesh.GetUVs(0, meshCoord);
            }
            else if (source == 1)
            {
                meshCoord = GetCoordMeshData(mesh, option);
            }
            else if (source == 2)
            {
                meshCoord = GetCoordProceduralData(mesh, option);
            }
            else if (source == 3)
            {
                meshCoord = GetCoord3rdPartyData(mesh, option);
            }

            if (meshCoord.Count == 0)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    meshCoord.Add(Vector4.zero);
                }

                //Debug.Log("[The Vegetation Engine] " + mesh.name + " does not have " + SourceCoordMeshEnum[option] + ". UV Coord 0 has been assigned instead!");
            }

            return meshCoord;
        }

        List<Vector4> GetCoordMeshData(Mesh mesh, int option)
        {
            var vertexCount = mesh.vertexCount;

            var meshCoord = new List<Vector4>(vertexCount);

            if (option == 0)
            {
                mesh.GetUVs(0, meshCoord);
            }

            else if (option == 1)
            {
                mesh.GetUVs(1, meshCoord);
            }

            else if (option == 2)
            {
                mesh.GetUVs(2, meshCoord);
            }

            else if (option == 3)
            {
                mesh.GetUVs(3, meshCoord);
            }

            return meshCoord;
        }

        List<Vector4> GetCoordProceduralData(Mesh mesh, int option)
        {
            var vertexCount = mesh.vertexCount;
            var vertices = mesh.vertices;

            var meshCoord = new List<Vector4>(vertexCount);

            // Planar XZ
            if (option == 0)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    meshCoord.Add(new Vector4(vertices[i].x, vertices[i].z, 0, 0));
                }
            }
            // Planar XY
            else if (option == 1)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    meshCoord.Add(new Vector4(vertices[i].x, vertices[i].y, 0, 0));
                }
            }
            // Planar ZY
            else if (option == 2)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    meshCoord.Add(new Vector4(vertices[i].z, vertices[i].y, 0, 0));
                }
            }
            // Procedural Pivots XZ
            else if (option == 3)
            {
                meshCoord = GenerateElementPivot(mesh);
            }

            return meshCoord;
        }

        List<Vector4> GetCoord3rdPartyData(Mesh mesh, int option)
        {
            var vertexCount = mesh.vertexCount;

            var meshCoord = new List<Vector4>(vertexCount);

            // NM Trunk Blend
            if (option == 0)
            {
                mesh.GetUVs(2, meshCoord);

                if (meshCoord.Count == 0)
                {
                    mesh.GetUVs(1, meshCoord);
                }
            }

            return meshCoord;
        }

        List<Vector4> GenerateElementPivot(Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var trianglesCount = mesh.triangles.Length;

            var elementIndices = new List<int>(vertexCount);
            var meshPivots = new List<Vector4>(vertexCount);
            int elementCount = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                elementIndices.Add(-99);
            }

            for (int i = 0; i < vertexCount; i++)
            {
                meshPivots.Add(Vector3.zero);
            }

            for (int i = 0; i < trianglesCount; i += 3)
            {
                var index1 = triangles[i + 0];
                var index2 = triangles[i + 1];
                var index3 = triangles[i + 2];

                int element = 0;

                if (elementIndices[index1] != -99)
                {
                    element = elementIndices[index1];
                }
                else if (elementIndices[index2] != -99)
                {
                    element = elementIndices[index2];
                }
                else if (elementIndices[index3] != -99)
                {
                    element = elementIndices[index3];
                }
                else
                {
                    element = elementCount;
                    elementCount++;
                }

                elementIndices[index1] = element;
                elementIndices[index2] = element;
                elementIndices[index3] = element;
            }

            for (int e = 0; e < elementCount; e++)
            {
                var positions = new List<Vector3>();

                for (int i = 0; i < elementIndices.Count; i++)
                {
                    if (elementIndices[i] == e)
                    {
                        positions.Add(vertices[i]);
                    }
                }

                float x = 0;
                float z = 0;

                for (int p = 0; p < positions.Count; p++)
                {
                    x = x + positions[p].x;
                    z = z + positions[p].z;
                }

                for (int i = 0; i < elementIndices.Count; i++)
                {
                    if (elementIndices[i] == e)
                    {
                        meshPivots[i] = new Vector3(x / positions.Count, z / positions.Count);
                    }
                }
            }

            return meshPivots;
        }

        void GetNormalsData(Mesh mesh, int source, int option)
        {
            if (source == 1)
            {
                var vertexCount = mesh.vertexCount;
                var vertices = mesh.vertices;

                if (option == 0)
                {
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }

                // Flat Shading Low
                else if (option == 1)
                {
                    var normals = mesh.normals;

                    Vector3[] customNormals = new Vector3[vertexCount];

                    if (normals != null)
                    {
                        for (int i = 0; i < vertexCount; i++)
                        {
                            var height = Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y);

                            customNormals[i] = Vector3.Lerp(normals[i], new Vector3(0, 1, 0), height);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < vertexCount; i++)
                        {
                            customNormals[i] = new Vector3(0, 1, 0);
                        }
                    }

                    mesh.normals = customNormals;
                    mesh.RecalculateTangents();
                }

                // Flat Shading Medium
                else if (option == 2)
                {
                    var normals = mesh.normals;

                    Vector3[] customNormals = new Vector3[vertexCount];

                    if (normals != null)
                    {
                        for (int i = 0; i < vertexCount; i++)
                        {
                            var height = Mathf.Clamp01(Mathf.Clamp01(vertices[i].y / maxBoundsInfo.y) + 0.5f);

                            customNormals[i] = Vector3.Lerp(normals[i], new Vector3(0, 1, 0), height);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < vertexCount; i++)
                        {
                            customNormals[i] = new Vector3(0, 1, 0);
                        }
                    }

                    mesh.normals = customNormals;
                    mesh.RecalculateTangents();
                }

                // Flat Shading Full
                else if (option == 3)
                {
                    Vector3[] customNormals = new Vector3[vertexCount];

                    for (int i = 0; i < vertexCount; i++)
                    {
                        customNormals[i] = new Vector3(0, 1, 0);
                    }

                    mesh.normals = customNormals;
                    mesh.RecalculateTangents();
                }

                // Spherical Shading
                else if (option == 4)
                {
                    Vector3[] customNormals = new Vector3[vertexCount];

                    for (int i = 0; i < vertexCount; i++)
                    {
                        customNormals[i] = Vector3.Normalize(vertices[i]);
                    }

                    mesh.normals = customNormals;
                    mesh.RecalculateTangents();
                }
            }
        }

        //Encode Vector3 to 8bit per channel Float based on: https://developer.download.nvidia.com/cg/pack.html
        List<float> EncodeVector3ToFloat(int vertexCount, List<float> source1, List<float> source2, List<float> source3)
        {
            var encoded = new List<float>();

            for (int i = 0; i < vertexCount; i++)
            {
                var x = Mathf.RoundToInt(255.0f * Mathf.Clamp01(source1[i]));
                var y = Mathf.RoundToInt(255.0f * Mathf.Clamp01(source2[i]));
                var z = Mathf.RoundToInt(255.0f * Mathf.Clamp01(source3[i]));
                //int w = 0;

                int result = /*(w << 24) |*/ (z << 16) | (y << 8) | x;

                encoded.Add(result);
            }

            return encoded;
        }

        List<float> EncodeVector3ToFloat(int vertexCount, List<Vector3> source)
        {
            var encoded = new List<float>();

            for (int i = 0; i < vertexCount; i++)
            {
                var x = Mathf.RoundToInt(255.0f * Mathf.Clamp01(source[i].x));
                var y = Mathf.RoundToInt(255.0f * Mathf.Clamp01(source[i].y));
                var z = Mathf.RoundToInt(255.0f * Mathf.Clamp01(source[i].z));
                //int w = 0;

                int result = /*(w << 24) |*/ (z << 16) | (y << 8) | x;

                encoded.Add(result);
            }

            return encoded;
        }

        float GetMeshArea(Mesh mesh)
        {
            float result = 0;

            for (int p = mesh.vertices.Length - 1, q = 0; q < mesh.vertices.Length; p = q++)
            {
                result += Vector3.Cross(mesh.vertices[q], mesh.vertices[p]).magnitude;
            }
            return result * 0.5f;
        }

        /// <summary>
        /// Mesh Actions
        /// </summary>

        List<float> MathAction(List<float> source, int action)
        {
            if (action == 1)
            {
                source = MathInvert(source);
            }
            else if (action == 2)
            {
                source = MathRemap01(source);
            }

            return source;
        }

        List<float> MathInvert(List<float> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                source[i] = 1.0f - source[i];
            }

            return source;
        }

        List<float> MathRemap01(List<float> source)
        {
            float min = source[0];
            float max = source[0];

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] < min)
                    min = source[i];

                if (source[i] > max)
                    max = source[i];
            }

            // Avoid divide by 0
            if (min != max)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    source[i] = (source[i] - min) / (max - min);
                }
            }
            else
            {
                for (int i = 0; i < source.Count; i++)
                {
                    source[i] = 0.0f;
                }
            }

            return source;
        }

        List<float> MathRemap01InBounds(List<float> source, int channel)
        {
            float max = 0;


            if (channel == 1)
            {
                max = maxBoundsInfo.y;
            }


            for (int i = 0; i < source.Count; i++)
            {
                source[i] = source[i] / max;
            }

            return source;
        }

        float MathRemap(float value, float low1, float high1, float low2, float high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }

        /// <summary>
        /// Convert Macros
        /// </summary>

        void GetDefaultShadersFromPreset()
        {
            for (int i = 0; i < presetLines.Count; i++)
            {
                if (presetLines[i].StartsWith("Shader"))
                {
                    string[] splitLine = presetLines[i].Split(char.Parse(" "));

                    var type = "";

                    if (splitLine.Length > 1)
                    {
                        type = splitLine[1];
                    }

                    if (type == "SHADER_DEFAULT_CROSS")
                    {
                        var shader = presetLines[i].Replace("Shader SHADER_DEFAULT_CROSS ", "");

                        if (Shader.Find(shader) != null)
                        {
                            shaderCross = Shader.Find(shader);
                        }
                    }
                    else if (type == "SHADER_DEFAULT_LEAF")
                    {
                        var shader = presetLines[i].Replace("Shader SHADER_DEFAULT_LEAF ", "");

                        if (Shader.Find(shader) != null)
                        {
                            shaderLeaf = Shader.Find(shader);
                        }
                    }
                    else if (type == "SHADER_DEFAULT_BARK")
                    {
                        var shader = presetLines[i].Replace("Shader SHADER_DEFAULT_BARK ", "");

                        if (Shader.Find(shader) != null)
                        {

                            shaderBark = Shader.Find(shader);
                        }
                    }
                    else if (type == "SHADER_DEFAULT_GRASS")
                    {
                        var shader = presetLines[i].Replace("Shader SHADER_DEFAULT_GRASS ", "");

                        if (Shader.Find(shader) != null)
                        {
                            shaderGrass = Shader.Find(shader);
                        }
                    }
                    else if (type == "SHADER_DEFAULT_PROP")
                    {
                        var shader = presetLines[i].Replace("Shader SHADER_DEFAULT_PROP ", "");

                        if (Shader.Find(shader) != null)
                        {
                            shaderProp = Shader.Find(shader);
                        }
                    }
                }
            }
        }

        void GetMaterialConversionFromPreset(Material materialOriginal, Material materialInstance)
        {
            var material = materialOriginal;
            var useLine = true;

            var texName = "_MainMaskTex";

            var doPacking = false;
            //var doCleanup = false;

            int packChannel = 0;
            int maskIndex = 0;
            int actionIndex = 0;

            InitTextureStorage();

            for (int i = 0; i < presetLines.Count; i++)
            {
                useLine = GetConditionFromPreset(useLine, presetLines[i], material);

                if (presetLines[i].StartsWith("Utility") && useLine)
                {
                    string[] splitLine = presetLines[i].Split(char.Parse(" "));

                    var type = "";
                    var file = "";

                    if (splitLine.Length > 1)
                    {
                        type = splitLine[1];
                    }

                    if (splitLine.Length > 2)
                    {
                        file = splitLine[2];
                    }

                    // Create a copy of the material instance at this point
                    if (type == "SET_CURRENT_MATERIAL_AS_BASE")
                    {
                        material = new Material(materialInstance);
                    }

                    // Reset material to original
                    if (type == "SET_ORIGINAL_MATERIAL_AS_BASE")
                    {
                        material = materialOriginal;
                    }

                    if (type == "START_TEXTURE_PACKING")
                    {
                        doPacking = true;
                        //doCleanup = false;
                    }

                    if (type == "DELETE_FILES_BY_NAME")
                    {
                        string dataPath;

                        if (collectConvertedData)
                        {
                            if (shareCommonMaterials)
                            {
                                dataPath = projectDataFolder + SHARED_DATA_PATH;
                            }
                            else
                            {
                                dataPath = projectDataFolder + PREFABS_DATA_PATH + "/" + prefabName;
                            }
                        }
                        else
                        {
                            dataPath = prefabDataFolder;
                        }

                        if (Directory.Exists(dataPath) && file != "")
                        {
                            var allFolderFiles = Directory.GetFiles(dataPath);

                            for (int f = 0; f < allFolderFiles.Length; f++)
                            {
                                if (allFolderFiles[f].Contains(file))
                                {
                                    FileUtil.DeleteFileOrDirectory(allFolderFiles[f]);
                                }
                            }

                            AssetDatabase.Refresh();
                        }
                    }
                }

                if (presetLines[i].StartsWith("Material") && useLine)
                {
                    string[] splitLine = presetLines[i].Split(char.Parse(" "));

                    var type = "";
                    var src = "";
                    var dst = "";
                    var val = "";
                    var set = "";

                    var x = "0";
                    var y = "0";
                    var z = "0";
                    var w = "0";

                    if (splitLine.Length > 1)
                    {
                        type = splitLine[1];
                    }

                    if (splitLine.Length > 2)
                    {
                        src = splitLine[2];
                        set = splitLine[2];
                    }

                    if (splitLine.Length > 3)
                    {
                        dst = splitLine[3];
                        x = splitLine[3];
                    }

                    if (splitLine.Length > 4)
                    {
                        val = splitLine[4];
                        y = splitLine[4];
                    }

                    if (splitLine.Length > 5)
                    {
                        z = splitLine[5];
                    }

                    if (splitLine.Length > 6)
                    {
                        w = splitLine[6];
                    }

                    // Set the shader first
                    if (type == "SET_SHADER")
                    {
                        materialInstance.shader = GetShaderFromPreset(set);
                    }

                    else if (type == "SET_SHADER_BY_NAME")
                    {
                        var shader = presetLines[i].Replace("Material SET_SHADER_BY_NAME ", "");

                        if (Shader.Find(shader) != null)
                        {
                            materialInstance.shader = Shader.Find(shader);
                        }
                    }

                    else if (type == "SET_FLOAT")
                    {
                        materialInstance.SetFloat(set, float.Parse(x, CultureInfo.InvariantCulture));
                    }

                    else if (type == "SET_COLOR")
                    {
                        materialInstance.SetColor(set, new Color(float.Parse(x, CultureInfo.InvariantCulture), float.Parse(y, CultureInfo.InvariantCulture), float.Parse(z, CultureInfo.InvariantCulture), float.Parse(w, CultureInfo.InvariantCulture)));
                    }

                    else if (type == "SET_VECTOR")
                    {
                        materialInstance.SetVector(set, new Vector4(float.Parse(x, CultureInfo.InvariantCulture), float.Parse(y, CultureInfo.InvariantCulture), float.Parse(z, CultureInfo.InvariantCulture), float.Parse(w, CultureInfo.InvariantCulture)));
                    }

                    else if (type == "COPY_FLOAT")
                    {
                        if (material.HasProperty(src))
                        {
                            materialInstance.SetFloat(dst, material.GetFloat(src));
                        }
                    }

                    else if (type == "COPY_TEX")
                    {
                        if (material.HasProperty(src))
                        {
                            var srcTex = material.GetTexture(src);

                            materialInstance.SetTexture(dst, srcTex);

                            if (collectConvertedData && collectOriginalTextures)
                            {
                                var srcPath = AssetDatabase.GetAssetPath(srcTex);
                                var dataPath = projectDataFolder + ORIGINAL_DATA_PATH + "/" + Path.GetFileName(srcPath);

                                if (File.Exists(dataPath))
                                {
                                    if (AssetDatabase.LoadMainAssetAtPath(dataPath) != srcTex)
                                    {
                                        dataPath = AssetDatabase.GenerateUniqueAssetPath(dataPath);
                                    }
                                }

                                AssetDatabase.MoveAsset(srcPath, dataPath);
                                AssetDatabase.Refresh();
                            }
                        }
                    }

                    else if (type == "COPY_COLOR")
                    {
                        if (material.HasProperty(src))
                        {
                            materialInstance.SetColor(dst, material.GetColor(src).linear);
                        }
                    }

                    else if (type == "COPY_VECTOR")
                    {
                        if (material.HasProperty(src))
                        {
                            materialInstance.SetVector(dst, material.GetVector(src));
                        }
                    }

                    else if (type == "COPY_ST_AS_VECTOR")
                    {
                        if (material.HasProperty(src))
                        {
                            Vector4 uvs = new Vector4(material.GetTextureScale(src).x, material.GetTextureScale(src).y,
                                                      material.GetTextureOffset(src).x, material.GetTextureOffset(src).y);

                            materialInstance.SetVector(dst, uvs);
                        }
                    }

                    else if (type == "COPY_VECTOR_X_AS_FLOAT")
                    {
                        if (material.HasProperty(src))
                        {
                            materialInstance.SetFloat(dst, material.GetVector(src).x);
                        }
                    }

                    else if (type == "COPY_VECTOR_Y_AS_FLOAT")
                    {
                        if (material.HasProperty(src))
                        {
                            materialInstance.SetFloat(dst, material.GetVector(src).y);
                        }
                    }

                    else if (type == "COPY_VECTOR_Z_AS_FLOAT")
                    {
                        if (material.HasProperty(src))
                        {
                            materialInstance.SetFloat(dst, material.GetVector(src).z);
                        }
                    }

                    else if (type == "COPY_VECTOR_W_AS_FLOAT")
                    {
                        if (material.HasProperty(src))
                        {
                            materialInstance.SetFloat(dst, material.GetVector(src).w);
                        }
                    }

                    else if (type == "ENABLE_KEYWORD")
                    {
                        materialInstance.EnableKeyword(set);
                    }

                    else if (type == "DISABLE_KEYWORD")
                    {
                        materialInstance.DisableKeyword(set);
                    }

                    else if (type == "ENABLE_INSTANCING")
                    {
                        materialInstance.enableInstancing = true;
                    }

                    else if (type == "DISABLE_INSTANCING")
                    {
                        materialInstance.enableInstancing = false;
                    }
                }

                if (presetLines[i].StartsWith("Texture") && useLine)
                {
                    string[] splitLine = presetLines[i].Split(char.Parse(" "));
                    string type = "";
                    string prop = "";
                    string pack = "";
                    string tex = "";
                    string action = "";

                    if (splitLine.Length > 2)
                    {
                        type = splitLine[1];
                        prop = splitLine[2];

                        if (type == "PropName")
                        {
                            if (prop != "")
                            {
                                texName = prop;
                            }
                        }
                    }

                    if (splitLine.Length > 3)
                    {
                        tex = splitLine[3];
                    }

                    if (material.HasProperty(tex) && material.GetTexture(tex) != null)
                    {
                        if (splitLine.Length > 1)
                        {
                            type = splitLine[1];

                            if (type == "SetRed")
                            {
                                maskIndex = 0;
                            }

                            if (type == "SetGreen")
                            {
                                maskIndex = 1;
                            }

                            if (type == "SetBlue")
                            {
                                maskIndex = 2;
                            }

                            if (type == "SetAlpha")
                            {
                                maskIndex = 3;
                            }
                        }

                        if (splitLine.Length > 2)
                        {
                            pack = splitLine[2];

                            if (pack == "NONE")
                            {
                                packChannel = 0;
                            }

                            if (pack == "GET_RED")
                            {
                                packChannel = 1;
                            }

                            if (pack == "GET_GREEN")
                            {
                                packChannel = 2;
                            }

                            if (pack == "GET_BLUE")
                            {
                                packChannel = 3;
                            }

                            if (pack == "GET_ALPHA")
                            {
                                packChannel = 4;
                            }

                            if (pack == "GET_GRAY")
                            {
                                packChannel = 555;
                            }

                            if (pack == "GET_GREY")
                            {
                                packChannel = 555;
                            }
                        }

                        action = splitLine[splitLine.Length - 1];

                        if (action == "ACTION_INVERT")
                        {
                            actionIndex = 1;
                        }
                        else
                        {
                            actionIndex = 0;
                        }

                        maskChannels[maskIndex] = packChannel;
                        maskActions[maskIndex] = actionIndex;
                        maskTextures[maskIndex] = material.GetTexture(tex);
                    }
                }

                if (doPacking)
                {
                    var id = packedTextureNames.Count;

                    if (maskTextures[0] != null || maskTextures[1] != null || maskTextures[2] != null || maskTextures[3] != null)
                    {
                        var internalName = GetPackedTextureName(maskTextures[0], maskChannels[0], maskTextures[1], maskChannels[1], maskTextures[2], maskChannels[2], maskTextures[3], maskChannels[3]);
                        bool exist = false;

                        for (int n = 0; n < packedTextureNames.Count; n++)
                        {
                            if (packedTextureNames[n] == internalName)
                            {
                                materialInstance.SetTexture(texName, packedTextureObjcts[n]);
                                exist = true;
                            }
                        }

                        if (exist == false)
                        {
                            PackTexture(materialInstance, id, internalName, texName);
                        }
                    }

                    InitTextureStorage();

                    doPacking = false;
                }
            }
        }

        Shader GetShaderFromPreset(string check)
        {
            var shader = shaderLeaf;

            if (check == "SHADER_DEFAULT_CROSS")
            {
                shader = shaderCross;
            }
            else if (check == "SHADER_DEFAULT_LEAF")
            {
                shader = shaderLeaf;
            }
            else if (check == "SHADER_DEFAULT_BARK")
            {
                shader = shaderBark;
            }
            else if (check == "SHADER_DEFAULT_GRASS")
            {
                shader = shaderGrass;
            }
            else if (check == "SHADER_DEFAULT_PROP")
            {
                shader = shaderProp;
            }

            return shader;
        }

        void ConvertMaterials()
        {
            packedTextureNames = new List<string>();
            packedTextureObjcts = new List<Texture>();

            for (int i = 0; i < materialArraysInPrefab.Count; i++)
            {
                if (materialArraysInPrefab[i] != null)
                {
                    for (int j = 0; j < materialArraysInPrefab[i].Length; j++)
                    {
                        var material = materialArraysInPrefab[i][j];
                        var materialInstance = materialArraysInstances[i][j];

                        //var materialPath = prefabDataFolder + "/" + materialInstance.name + " (" + outputSuffix + " Material).mat";

                        //if (keepConvertedMaterials && File.Exists(materialPath))
                        //{
                        //    materialArraysInstances[i][j] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                        //}
                        //else
                        {
                            if (IsValidMaterial(material))
                            {
                                ConvertMaterial(material, materialInstance);
                                materialArraysInstances[i][j] = convertedMaterial;
                            }
                            else
                            {
                                materialArraysInstances[i][j] = material;
                            }
                        }
                    }
                }
            }
        }

        void ConvertMaterial(Material material, Material materialInstance)
        {
            materialInstance.enableInstancing = true;

            SetMaterialInitSettings(materialInstance);
            GetMaterialConversionFromPreset(material, materialInstance);
            SetMaterialPostSettings(materialInstance);

            SaveMaterial(materialInstance);
        }

        void SaveMaterial(Material materialInstance)
        {
            string dataPath;
            string savePath = "/" + materialInstance.name + " (" + outputSuffix + " Material).mat";

            if (collectConvertedData)
            {
                if (shareCommonMaterials)
                {
                    dataPath = projectDataFolder + SHARED_DATA_PATH + savePath;
                }
                else
                {
                    dataPath = projectDataFolder + PREFABS_DATA_PATH + "/" + prefabName + savePath;
                }
            }
            else
            {
                dataPath = prefabDataFolder + savePath;
            }

            if (File.Exists(dataPath))
            {
                var materialFile = AssetDatabase.LoadAssetAtPath<Material>(dataPath);
                EditorUtility.CopySerialized(materialInstance, materialFile);
            }
            else
            {
                AssetDatabase.CreateAsset(materialInstance, dataPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            convertedMaterial = AssetDatabase.LoadAssetAtPath<Material>(dataPath);

            TVEShaderUtils.SetMaterialSettings(convertedMaterial);
        }

        void AssignConvertedMaterials()
        {
            for (int i = 0; i < meshRenderersInPrefab.Count; i++)
            {
                if (meshRenderersInPrefab[i] != null)
                {
                    meshRenderersInPrefab[i].sharedMaterials = materialArraysInstances[i];
                }
            }
        }

        void SetMaterialInitSettings(Material material)
        {
            // Set bounds info
            material.SetVector("_MaxBoundsInfo", maxBoundsInfo);

            // Set some initial motion settings
            material.SetFloat("_MotionAmplitude_10", 0.05f);
            material.SetFloat("_MotionSpeed_10", 2);
            material.SetFloat("_MotionScale_10", 0);
            material.SetFloat("_MotionVariation_10", 0);

            material.SetFloat("_MotionAmplitude_20", 0.1f);
            material.SetFloat("_MotionSpeed_20", 6);
            material.SetFloat("_MotionScale_20", 0);
            material.SetFloat("_MotionVariation_20", 5);

            material.SetFloat("_MotionAmplitude_30", 0.5f);
            material.SetFloat("_MotionSpeed_30", 8);
            material.SetFloat("_MotionScale_30", 7);
            material.SetFloat("_MotionVariation_30", 8);

            material.SetFloat("_MotionAmplitude_32", 0.2f);
            material.SetFloat("_MotionSpeed_32", 20);
            material.SetFloat("_MotionScale_32", 19);
            material.SetFloat("_MotionVariation_32", 20);

            material.SetFloat("_InteractionAmplitude", 1f);
        }

        void SetMaterialPostSettings(Material material)
        {
            if (sourceDetailCoord > 0 && optionDetailCoord == 3)
            {
                material.SetInt("_VertexPivotMode", 1);
            }

            if (sourceVariation > 0 && (optionVariation == 2 || optionVariation == 3))
            {
                material.SetInt("_VertexVariationMode", 1);
            }
        }

        bool IsValidMaterial(Material material)
        {
            bool valid = true;
            int i = 0;

            if (material == null)
            {
                i++;
            }

            if (material != null && material.HasProperty("_IsTVEShader") == true)
            {
                i++;
            }

            if (i > 0)
            {
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Packed Texture Utils
        /// </summary>

        void InitTexturePacker()
        {
            blitTexture = Texture2D.whiteTexture;
            blitMaterial = new Material(Shader.Find("Hidden/BOXOPHOBIC/Texture Packer Blit"));

            sourceTexCompressions = new TextureImporterCompression[4];
            sourceimportSizes = new int[4];
            sourceTexImporters = new TextureImporter[4];
            sourceTexSettings = new TextureImporterSettings[4];

            for (int i = 0; i < 4; i++)
            {
                sourceTexCompressions[i] = new TextureImporterCompression();
                sourceTexImporters[i] = new TextureImporter();
                sourceTexSettings[i] = new TextureImporterSettings();
            }
        }

        void InitTextureStorage()
        {
            maskChannels = new int[4];
            maskActions = new int[4];
            maskTextures = new Texture[4];

            for (int i = 0; i < 4; i++)
            {
                maskChannels[i] = 0;
                maskActions[i] = 0;
                maskTextures[i] = null;
            }
        }

        void PackTexture(Material materialInstance, int id, string internalName, string propName)
        {
            string saveName = propName.Replace("_", "");

            string dataPath;
            string savePath = "/" + materialInstance.name + " - " + saveName /*+ " " + id.ToString()*/ + " (" + outputSuffix + " Texture).png";

            if (collectConvertedData)
            {
                if (shareCommonMaterials)
                {
                    dataPath = projectDataFolder + SHARED_DATA_PATH + savePath;
                }
                else
                {
                    dataPath = projectDataFolder + PREFABS_DATA_PATH + "/" + prefabName + savePath;
                }
            }
            else
            {
                dataPath = prefabDataFolder + savePath;
            }

            int initSize = GetPackedInitSize(maskTextures[0], maskTextures[1], maskTextures[2], maskTextures[3]);

            ResetBlitMaterial();

            //Set Packer Metallic channel
            if (maskTextures[0] != null)
            {
                PrepareSourceTexture(maskTextures[0], 0);
                ResetSourceTexture(0);

                blitMaterial.SetTexture("_Packer_TexR", maskTextures[0]);
                blitMaterial.SetInt("_Packer_ChannelR", maskChannels[0]);
                blitMaterial.SetInt("_Packer_ActionR", maskActions[0]);
            }
            else
            {
                blitMaterial.SetInt("_Packer_ChannelR", NONE);
                blitMaterial.SetFloat("_Packer_FloatR", 1.0f);
            }

            //Set Packer Occlusion channel
            if (maskTextures[1] != null)
            {
                PrepareSourceTexture(maskTextures[1], 1);
                ResetSourceTexture(1);

                blitMaterial.SetTexture("_Packer_TexG", maskTextures[1]);
                blitMaterial.SetInt("_Packer_ChannelG", maskChannels[1]);
                blitMaterial.SetInt("_Packer_ActionG", maskActions[1]);
            }
            else
            {
                blitMaterial.SetInt("_Packer_ChannelG", NONE);
                blitMaterial.SetFloat("_Packer_FloatG", 1.0f);
            }

            //Set Packer Mask channel
            if (maskTextures[2] != null)
            {
                PrepareSourceTexture(maskTextures[2], 2);
                ResetSourceTexture(2);

                blitMaterial.SetTexture("_Packer_TexB", maskTextures[2]);
                blitMaterial.SetInt("_Packer_ChannelB", maskChannels[2]);
                blitMaterial.SetInt("_Packer_ActionB", maskActions[2]);
            }
            else
            {
                blitMaterial.SetInt("_Packer_ChannelB", NONE);
                blitMaterial.SetFloat("_Packer_FloatB", 1.0f);
            }

            //Set Packer Smothness channel
            if (maskTextures[3] != null)
            {
                PrepareSourceTexture(maskTextures[3], 3);
                ResetSourceTexture(3);

                blitMaterial.SetTexture("_Packer_TexA", maskTextures[3]);
                blitMaterial.SetInt("_Packer_ChannelA", maskChannels[3]);
                blitMaterial.SetInt("_Packer_ActionA", maskActions[3]);
            }
            else
            {
                blitMaterial.SetInt("_Packer_ChannelA", NONE);
                blitMaterial.SetFloat("_Packer_FloatA", 1.0f);
            }

            Vector2 pixelSize = GetPackedPixelSize(maskTextures[0], maskTextures[1], maskTextures[2], maskTextures[3]);
            int importSize = GetPackedImportSize(initSize, pixelSize);

            Texture savedPacked = SavePackedTexture(dataPath, pixelSize);

            packedTextureNames.Add(internalName);
            packedTextureObjcts.Add(savedPacked);

            SetTextureImporterSettings(savedPacked, importSize, SRGB, ALPHA_DEFAULT);

            materialInstance.SetTexture(propName, savedPacked);
        }

        string GetPackedTextureName(Texture tex1, int ch1, Texture tex2, int ch2, Texture tex3, int ch3, Texture tex4, int ch4)
        {
            var texName1 = "NULL";
            var texName2 = "NULL";
            var texName3 = "NULL";
            var texName4 = "NULL";

            if (tex1 != null)
            {
                texName1 = tex1.name;
            }

            if (tex2 != null)
            {
                texName2 = tex2.name;
            }

            if (tex3 != null)
            {
                texName3 = tex3.name;
            }

            if (tex4 != null)
            {
                texName4 = tex4.name;
            }

            var name = texName1 + ch1 + texName2 + ch2 + texName3 + ch3 + texName4 + ch4;

            return name;
        }

        Vector2 GetPackedPixelSize(Texture tex1, Texture tex2, Texture tex3, Texture tex4)
        {
            int x = 32;
            int y = 32;

            if (tex1 != null)
            {
                x = Mathf.Max(x, tex1.width);
                y = Mathf.Max(y, tex1.height);
            }

            if (tex2 != null)
            {
                x = Mathf.Max(x, tex2.width);
                y = Mathf.Max(y, tex2.height);
            }

            if (tex3 != null)
            {
                x = Mathf.Max(x, tex3.width);
                y = Mathf.Max(y, tex3.height);
            }

            if (tex4 != null)
            {
                x = Mathf.Max(x, tex4.width);
                y = Mathf.Max(y, tex4.height);
            }

            return new Vector2(x, y);
        }

        int GetPackedInitSize(Texture tex1, Texture tex2, Texture tex3, Texture tex4)
        {
            int initSize = 32;

            if (tex1 != null)
            {
                string texPath = AssetDatabase.GetAssetPath(tex1);
                TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

                initSize = Mathf.Max(initSize, texImporter.maxTextureSize);
            }

            if (tex2 != null)
            {
                string texPath = AssetDatabase.GetAssetPath(tex2);
                TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

                initSize = Mathf.Max(initSize, texImporter.maxTextureSize);
            }

            if (tex3 != null)
            {
                string texPath = AssetDatabase.GetAssetPath(tex3);
                TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

                initSize = Mathf.Max(initSize, texImporter.maxTextureSize);
            }

            if (tex4 != null)
            {
                string texPath = AssetDatabase.GetAssetPath(tex4);
                TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

                initSize = Mathf.Max(initSize, texImporter.maxTextureSize);
            }

            return initSize;
        }

        int GetPackedImportSize(int initTexImportSize, Vector2 pixelTexSize)
        {
            int pixelSize = (int)Mathf.Max(pixelTexSize.x, pixelTexSize.y);
            int importSize = initTexImportSize;

            if (pixelSize < importSize)
            {
                importSize = pixelSize;
            }

            for (int i = 1; i < MaxTextureSizes.Length - 1; i++)
            {
                int a;
                int b;

                if ((importSize > MaxTextureSizes[i]) && (importSize < MaxTextureSizes[i + 1]))
                {
                    a = Mathf.Abs(MaxTextureSizes[i] - importSize);
                    b = Mathf.Abs(MaxTextureSizes[i + 1] - importSize);

                    if (a < b)
                    {
                        importSize = MaxTextureSizes[i];
                    }
                    else
                    {
                        importSize = MaxTextureSizes[i + 1];
                    }

                    break;
                }
            }

            //if (maxPackedImportSize > 0)
            //{
            //    if (importSize > maxPackedImportSize)
            //    {
            //        importSize = MaxTextureSizes[maxPackedImportSize];
            //    }
            //}

            return importSize;
        }

        Texture SavePackedTexture(string path, Vector2 size)
        {
            if (File.Exists(path))
            {
                FileUtil.DeleteFileOrDirectory(path);
                FileUtil.DeleteFileOrDirectory(path + ".meta");
            }

            RenderTexture renderTexure = new RenderTexture((int)size.x, (int)size.y, 0, RenderTextureFormat.ARGB32);

            Graphics.Blit(blitTexture, renderTexure, blitMaterial, 0);

            RenderTexture.active = renderTexure;
            Texture2D packedTexure = new Texture2D(renderTexure.width, renderTexure.height, TextureFormat.ARGB32, false);
            packedTexure.ReadPixels(new Rect(0, 0, renderTexure.width, renderTexure.height), 0, 0);
            packedTexure.Apply();
            RenderTexture.active = null;

            renderTexure.Release();

            byte[] bytes;
            bytes = packedTexure.EncodeToPNG();

            File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<Texture>(path);
        }

        void SetTextureImporterSettings(Texture texture, int importSize, bool colorSpace, bool alphaSourceIsOn)
        {
            string texPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

            texImporter.maxTextureSize = importSize;
            texImporter.sRGBTexture = colorSpace;
            if (alphaSourceIsOn)
            {
                texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            }
            else
            {
                texImporter.alphaSource = TextureImporterAlphaSource.None;
            }

            texImporter.SaveAndReimport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void ResetBlitMaterial()
        {
            blitMaterial = new Material(Shader.Find("Hidden/BOXOPHOBIC/Texture Packer Blit"));
        }

        void PrepareSourceTexture(Texture texture, int channel)
        {
            if (outputTextureIndex == OutputTexture.UseCurrentResolution)
            {
                return;
            }

            string texPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

            sourceTexCompressions[channel] = texImporter.textureCompression;
            sourceimportSizes[channel] = texImporter.maxTextureSize;

            texImporter.ReadTextureSettings(sourceTexSettings[channel]);

            texImporter.textureType = TextureImporterType.Default;
            texImporter.sRGBTexture = false;
            texImporter.mipmapEnabled = false;
            texImporter.maxTextureSize = 8192;
            texImporter.textureCompression = TextureImporterCompression.Uncompressed;

            sourceTexImporters[channel] = texImporter;

            texImporter.SaveAndReimport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void ResetSourceTexture(int index)
        {
            if (outputTextureIndex == OutputTexture.UseCurrentResolution)
            {
                return;
            }

            sourceTexImporters[index].textureCompression = sourceTexCompressions[index];
            sourceTexImporters[index].maxTextureSize = sourceimportSizes[index];
            sourceTexImporters[index].SetTextureSettings(sourceTexSettings[index]);
            sourceTexImporters[index].SaveAndReimport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Get Project Presets
        /// </summary>

        void GetDefaultShaders()
        {
            shaderProp = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Objects/Prop Standard Lit");
            shaderBark = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Vegetation/Bark Standard Lit");
            shaderCross = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Vegetation/Cross Subsurface Lit");
            shaderGrass = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Vegetation/Grass Subsurface Lit");
            shaderLeaf = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Vegetation/Leaf Subsurface Lit");
        }

        void GetPresets()
        {
            // FindObjectsOfTypeAll not working properly for unloaded assets
            allPresetPaths = Directory.GetFiles(Application.dataPath, "*.tveconverter", SearchOption.AllDirectories);

            presetPaths = new List<string>();

            for (int i = 0; i < allPresetPaths.Length; i++)
            {
                string assetPath = "Assets" + allPresetPaths[i].Replace(Application.dataPath, "").Replace('\\', '/');

                if (assetPath.Contains("[INCLUDE]") == false && assetPath.Contains("[HIDDEN]") == false && assetPath.Contains("[OVERRIDE]") == false)
                {
                    presetPaths.Add(assetPath);
                }
            }

            presetPaths.Sort();
            presetPaths.Insert(0, "");

            PresetsEnum = new string[presetPaths.Count];
            PresetsEnum[0] = "Choose a preset";

            for (int i = 1; i < presetPaths.Count; i++)
            {
                PresetsEnum[i] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(presetPaths[i]).name;
                PresetsEnum[i] = PresetsEnum[i].Replace(" - ", "/");
            }

            overridePaths = new List<string>();

            for (int i = 0; i < allPresetPaths.Length; i++)
            {
                string assetPath = "Assets" + allPresetPaths[i].Replace(Application.dataPath, "").Replace('\\', '/');

                if (assetPath.Contains("[OVERRIDE]") == true)
                {
                    overridePaths.Add(assetPath);
                }
            }

            overridePaths.Sort();
            overridePaths.Insert(0, "");

            OverridesEnum = new string[overridePaths.Count];
            OverridesEnum[0] = "None";

            for (int i = 1; i < overridePaths.Count; i++)
            {
                OverridesEnum[i] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(overridePaths[i]).name;
                OverridesEnum[i] = OverridesEnum[i].Replace("[OVERRIDE] ", "");
                OverridesEnum[i] = OverridesEnum[i].Replace(" - ", "/");
            }

            // FindObjectsOfTypeAll not working properly for unloaded assets
            var allDetectPaths = Directory.GetFiles(Application.dataPath, "*.tvedetect", SearchOption.AllDirectories);

            detectLines = new List<string>();

            for (int i = 0; i < allDetectPaths.Length; i++)
            {
                StreamReader reader = new StreamReader(allDetectPaths[i]);

                while (!reader.EndOfStream)
                {
                    detectLines.Add(reader.ReadLine());
                }

                reader.Close();
            }

            //for (int i = 0; i < detectLines.Count; i++)
            //{
            //    Debug.Log(detectLines[i]);
            //}
        }

        void GetPresetLines()
        {
            presetLines = new List<string>();

            StreamReader reader = new StreamReader(presetPaths[presetIndex]);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                presetLines.Add(line);

                if (line.StartsWith("Include"))
                {
                    GetIncludeLines(line);
                }
            }

            reader.Close();

            for (int i = 0; i < overrideIndices.Count; i++)
            {
                if (overrideIndices[i] != 0)
                {
                    reader = new StreamReader(overridePaths[overrideIndices[i]]);

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        presetLines.Add(line);

                        if (line.StartsWith("Include"))
                        {
                            GetIncludeLines(line);
                        }
                    }

                    reader.Close();
                }
            }
        }

        void GetIncludeLines(string line)
        {
            var import = line.Replace("Include ", "");

            for (int i = 0; i < allPresetPaths.Length; i++)
            {
                if (allPresetPaths[i].Contains(import))
                {
                    StreamReader reader = new StreamReader(allPresetPaths[i]);

                    while (!reader.EndOfStream)
                    {
                        presetLines.Add(reader.ReadLine());
                    }

                    reader.Close();
                }
            }
        }

        void GetDescriptionFromPreset()
        {
            infoTitle = "Preset";
            infoPreset = "No preset info found in preset!";
            infoStatus = "○ ○ ○ ○ ○";
            infoOnline = "";
            infoWarning = "";

            for (int i = 0; i < presetLines.Count; i++)
            {
                if (presetLines[i].StartsWith("InfoTitle"))
                {
                    infoTitle = presetLines[i].Replace("InfoTitle ", "");
                }

                if (presetLines[i].StartsWith("InfoPreset"))
                {
                    infoPreset = presetLines[i].Replace("InfoPreset ", "");
                }

                if (presetLines[i].StartsWith("InfoStatus"))
                {
                    infoStatus = presetLines[i].Replace("InfoStatus ", "");
                }

                if (presetLines[i].StartsWith("InfoOnline"))
                {
                    infoOnline = presetLines[i].Replace("InfoOnline ", "");
                }

                if (presetLines[i].StartsWith("InfoWarning"))
                {
                    infoWarning = presetLines[i].Replace("InfoWarning ", "");
                }
            }

            if (presetAutoDetected)
            {
                infoTitle = infoTitle + " (Auto Detected)";
            }
        }

        void GetOutputsFromPreset()
        {
            if (presetIndex == 0)
            {
                return;
            }

            for (int i = 0; i < presetLines.Count; i++)
            {
                //if (presetLines[i].StartsWith("OutputCategory"))
                //{
                //    string source = presetLines[i].Replace("OutputCategory ", "");

                //    for (int c = 0; c < CategoryEnum.Length; c++)
                //    {
                //        if (CategoryEnum[c] == source)
                //        {
                //            categoryIndex = c;
                //        }
                //    }
                //}

                if (presetLines[i].StartsWith("OutputMesh"))
                {
                    string source = presetLines[i].Replace("OutputMesh ", "");

                    if (source == "NONE")
                    {
                        outputMeshIndex = OutputMesh.Off;
                    }
                    else if (source == "DEFAULT")
                    {
                        outputMeshIndex = OutputMesh.Default;
                    }
                    else if (source == "CUSTOM")
                    {
                        outputMeshIndex = OutputMesh.Custom;
                    }
                    else if (source == "DETAIL")
                    {
                        outputMeshIndex = OutputMesh.Detail;
                    }
                    else if (source == "DEENVIRONMENT")
                    {
                        outputMeshIndex = OutputMesh.DEEnvironment;
                    }
                    else if (source == "POLYVERSE")
                    {
                        outputMeshIndex = OutputMesh.Polyverse;
                    }
                }

                if (presetLines[i].StartsWith("OutputMaterial"))
                {
                    string source = presetLines[i].Replace("OutputMaterial ", "");

                    if (source == "NONE")
                    {
                        outputMaterialIndex = OutputMaterial.Off;
                    }
                    else
                    {
                        outputMaterialIndex = OutputMaterial.Default;
                    }
                }

                if (presetLines[i].StartsWith("OutputTexture"))
                {
                    string source = presetLines[i].Replace("OutputTexture ", "");

                    if (source == "USE_CURRENT_RESOLUTION")
                    {
                        outputTextureIndex = OutputTexture.UseCurrentResolution;
                    }
                    else
                    {
                        outputTextureIndex = OutputTexture.UseHighestResolution;
                    }
                }

                if (presetLines[i].StartsWith("OutputSuffix"))
                {
                    outputSuffix = presetLines[i].Replace("OutputSuffix ", "");
                }
            }
        }

        //void GetLineConversionFromPreset()
        //{
        //    if (presetIndex == 0)
        //    {
        //        return;
        //    }

        //    for (int i = 0; i < presetLines.Count; i++)
        //    {
        //        if (presetLines[i].StartsWith("Line"))
        //        {
        //            string[] splitLine = presetLines[i].Split(char.Parse(" "));

        //            var type = "";
        //            var src = "";
        //            var dst = "";

        //            if (splitLine.Length > 1)
        //            {
        //                type = splitLine[1];
        //            }

        //            if (splitLine.Length > 2)
        //            {
        //                src = splitLine[2];
        //            }

        //            if (splitLine.Length > 3)
        //            {
        //                dst = splitLine[3];
        //            }

        //            if (type == "REPLACE")
        //            {
        //                for (int j = 0; j < presetLines.Count; j++)
        //                {
        //                    presetLines[j] = presetLines[j].Replace(src, dst);
        //                }
        //            }
        //            else if (type == "DELETE")
        //            {
        //                for (int j = 0; j < presetLines.Count; j++)
        //                {
        //                    presetLines[j] = "";
        //                }
        //            }
        //        }
        //    }
        //}

        void GetAllPresetInfo()
        {
            if (presetIndex != 0)
            {

                outputMeshIndex = OutputMesh.Default;
                outputMaterialIndex = OutputMaterial.Default;
                outputSuffix = "TVE";

                GetDefaultShaders();

                GetPresetLines();
                GetDescriptionFromPreset();
                GetOutputsFromPreset();
                GetMeshConversionFromPreset();
                GetDefaultShadersFromPreset();

                //for (int i = 0; i < presetLines.Count; i++)
                //{
                //    Debug.Log(presetLines[i]);
                //}
            }
        }

        bool GetConditionFromPreset(bool useLine, string line, Material material)
        {
            if (line.StartsWith("if"))
            {
                useLine = false;

                string[] splitLine = line.Split(char.Parse(" "));

                var type = "";
                var check = "";
                var val = "";

                if (splitLine.Length > 1)
                {
                    type = splitLine[1];
                }

                if (splitLine.Length > 2)
                {
                    check = splitLine[2];
                }

                if (splitLine.Length > 3)
                {
                    val = splitLine[3];
                }

                if (type.Contains("MATERIAL_NAME_CONTAINS"))
                {
                    if (material.name.Contains(check))
                    {
                        useLine = true;
                    }
                }
                else if (type.Contains("SHADER_NAME_CONTAINS"))
                {
                    if (material.shader.name.Contains(check))
                    {
                        useLine = true;
                    }
                }
                else if (type.Contains("RENDERTYPE_TAG_CONTAINS"))
                {
                    if (material.GetTag("RenderType", false).Contains(check))
                    {
                        useLine = true;
                    }
                }
                else if (type.Contains("HAS_PROP"))
                {
                    if (material.HasProperty(check))
                    {
                        useLine = true;
                    }
                }
                else if (type.Contains("FLOAT_EQUALS"))
                {
                    var min = float.Parse(val, CultureInfo.InvariantCulture) - 0.1f;
                    var max = float.Parse(val, CultureInfo.InvariantCulture) + 0.1f;

                    if (material.HasProperty(check) && material.GetFloat(check) > min && material.GetFloat(check) < max)
                    {
                        useLine = true;
                    }
                }
                else if (type.Contains("KEYWORD_ENABLED"))
                {
                    if (material.IsKeywordEnabled(check))
                    {
                        useLine = true;
                    }
                }
                else if (type.Contains("KEYWORD_DISABLED"))
                {
                    if (material.IsKeywordEnabled(check))
                    {
                        useLine = true;
                    }
                }
            }

            if (line.StartsWith("if") && line.Contains("!"))
            {
                useLine = !useLine;
            }

            if (line.StartsWith("endif"))
            {
                useLine = true;
            }

            return useLine;
        }
    }
}
