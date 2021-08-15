using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(Hessburg.SunLight))]
public class SunLightEditorCS : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        // Location Settings
        EditorGUILayout.LabelField("Location:", new GUILayoutOption[] {});
        LocPre = (SunLightEditorCS.SunLightLocationPresets) selectedLocationPreset.intValue;
        lastLocationPreset = selectedLocationPreset.intValue;
        LocPre = (SunLightEditorCS.SunLightLocationPresets) EditorGUILayout.EnumPopup(new GUIContent("Location Presets", "Here is a tooltip"), (System.Enum) LocPre, new GUILayoutOption[] {});
        selectedLocationPreset.intValue = (int) LocPre;
        if (selectedLocationPreset.intValue != lastLocationPreset)
        {
            latitude.floatValue = lat[selectedLocationPreset.intValue];
            longitude.floatValue = lon[selectedLocationPreset.intValue];
            offsetUTC.floatValue = offs[selectedLocationPreset.intValue];
        }
        else
        {
            if (((latitude.floatValue != lat[selectedLocationPreset.intValue]) || (longitude.floatValue != lon[selectedLocationPreset.intValue])) || (offsetUTC.floatValue != offs[selectedLocationPreset.intValue]))
            {
                lastLocationPreset = 0;
                selectedLocationPreset.intValue = 0;
                LocPre = (SunLightEditorCS.SunLightLocationPresets) 0;
            }
            else
            {
                lastLocationPreset = selectedLocationPreset.intValue;
            }
        }
        latitude.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Latitude", "Example: Paris = 48.85, New York = 40.71, Sydney = -33.85"), latitude.floatValue, new GUILayoutOption[] {}), -90f, 90f);
        longitude.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Longitude", "Example: Paris = 2.35, New York = -74.00, Sydney = 151.2"), longitude.floatValue, new GUILayoutOption[] {}), -180f, 180f);
        offsetUTC.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Offset UTC", "Example: GMT = 0, Paris = 1, New York = -5, Sydney = 10"), offsetUTC.floatValue, new GUILayoutOption[] {}), -12f, 14f);
        // Time Settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Time:", new GUILayoutOption[] {});
        timeInHours.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Time In Hours", "The local time in decimal hours - Example: 20:30 = 20.5"), timeInHours.floatValue, new GUILayoutOption[] {}), 0f, 24f);
        progressTime.boolValue = EditorGUILayout.Toggle(new GUIContent("Progress Time", "Set to true if you want this script to handle the progress of time"), progressTime.boolValue, new GUILayoutOption[] {});
        if (progressTime.boolValue == true)
        {
            timeProgressFactor.floatValue = EditorGUILayout.FloatField(new GUIContent("Time Progress Factor", "Multiplier for the speed of the progress of time. Set to 1.0 for real time."), timeProgressFactor.floatValue, new GUILayoutOption[] {});
            leapYear.boolValue = EditorGUILayout.Toggle(new GUIContent("Leap Year", "The year will have 366 days if set to true."), leapYear.boolValue, new GUILayoutOption[] {});
            dayOfYear.intValue = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Day Of Year", "Example: December 31th = 366 (in leapyears), January 1st = 1"), dayOfYear.intValue, new GUILayoutOption[] {}), 1, 366);
        }
        else
        {
            dayOfYear.intValue = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Day Of Year", "Example: December 31th = 365, January 1st = 1"), dayOfYear.intValue, new GUILayoutOption[] {}), 1, 365);
        }
        // Optional Settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Optional Settings:", new GUILayoutOption[] {});
        //overcastFactor.floatValue = EditorGUILayout.FloatField("Overcast Factor", overcastFactor.floatValue);   // working but disabled and not advertised
        // New in version 1.1
        northDirection.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("North Direction", "North direction – will change azimuth of the sun to fit to a custom north direction (0.0-360.0)."), northDirection.floatValue, new GUILayoutOption[] {}), 0f, 360f);
        //
        overrideAzimuth.boolValue = EditorGUILayout.Toggle(new GUIContent("Override Azimuth", "Set to true if you want to override the Azimuth (horizontal rotation) of the sun with the variable artisticSunAzimuth"), overrideAzimuth.boolValue, new GUILayoutOption[] {});
        if (overrideAzimuth.boolValue == true)
        {
            artisticSunAzimuth.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Artistic Sun Azimuth", "Azimuth (horizontal rotation) of the sun (0.0-360.0). For artistic purposes."), artisticSunAzimuth.floatValue, new GUILayoutOption[] {}), 0f, 360f);
        }
        // Light Settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Light:", new GUILayoutOption[] {});
        sceneLightLayerMask.intValue = (int) EditorMask("Culling Mask", (LayerMask) sceneLightLayerMask.intValue);
      //  Hessburg.SunLight SunLight = target;

        Hessburg.SunLight SunLight = (Hessburg.SunLight)target;

        SunLight.ChangeSceneLightLayer((LayerMask) sceneLightLayerMask.intValue);
        if ((((((((lastLatitude != latitude.floatValue) || (lastLongitude != longitude.floatValue)) || (lastOffsetUTC != offsetUTC.floatValue)) || (lastDayOfYear != dayOfYear.intValue)) || (lastTimeInHours != timeInHours.floatValue)) || (lastLeapYear != leapYear.boolValue)) || (lastOverrideAzimuth != overrideAzimuth.boolValue)) || (lastArtisticSunAzimuth != artisticSunAzimuth.floatValue))
        {
            inspectorChanged.boolValue = true;
        }
        lastLatitude = latitude.floatValue;
        lastLongitude = longitude.floatValue;
        lastOffsetUTC = offsetUTC.floatValue;
        lastDayOfYear = dayOfYear.intValue;
        lastTimeInHours = timeInHours.floatValue;
        lastLeapYear = leapYear.boolValue;
        lastOverrideAzimuth = overrideAzimuth.boolValue;
        lastArtisticSunAzimuth = artisticSunAzimuth.floatValue;
        // New in version 1.1:
        if (lastNorthDirection != northDirection.floatValue)
        {
            inspectorChanged.boolValue = true;
        }
        lastNorthDirection = northDirection.floatValue;
        //
        serializedObject.ApplyModifiedProperties();
    }

    private enum SunLightLocationPresets
    {
        custom_location = 0,
        Reykjavik = 1,
        Berlin = 2,
        New_York = 3,
        Baghdad = 4,
        Hongkong = 5,
        Panama = 6,
        Singapore = 7,
        Rio_de_Janeiro = 8,
        Sydney = 9,
        Falkland_Islands = 10
    }

    private SunLightEditorCS.SunLightLocationPresets LocPre;

    private int selectedLayer;

    private float[] lat;

    private float[] lon;

    private float[] offs;

    private int lastLocationPreset;

    private int lastLayerMask;

    private SerializedProperty latitude;

    private SerializedProperty longitude;

    private SerializedProperty offsetUTC;

    private SerializedProperty dayOfYear;

    private SerializedProperty timeInHours;

    private SerializedProperty progressTime;

    private SerializedProperty timeProgressFactor;

    private SerializedProperty leapYear;

    private SerializedProperty overrideAzimuth;

    private SerializedProperty artisticSunAzimuth;

    // private SerializedProperty overcastFactor;    // working but disabled and not advertised

    private SerializedProperty sceneLightLayerMask;

    private SerializedProperty selectedLocationPreset;

    private SerializedProperty inspectorChanged;

    private System.Collections.Generic.List<string> layers;

    private string[] namesArray;

    private float lastLatitude;

    private float lastLongitude;

    private float lastOffsetUTC;

    private int lastDayOfYear;

    private float lastTimeInHours;

    private bool lastLeapYear;

    private bool lastOverrideAzimuth;

    private float lastArtisticSunAzimuth;

    // New in version 1.1:
    private SerializedProperty northDirection;

    private float lastNorthDirection;

    //
    public virtual void OnEnable()
    {
        latitude = serializedObject.FindProperty("latitude");
        longitude = serializedObject.FindProperty("longitude");
        offsetUTC = serializedObject.FindProperty("offsetUTC");
        dayOfYear = serializedObject.FindProperty("dayOfYear");
        timeInHours = serializedObject.FindProperty("timeInHours");
        progressTime = serializedObject.FindProperty("progressTime");
        timeProgressFactor = serializedObject.FindProperty("timeProgressFactor");
        leapYear = serializedObject.FindProperty("leapYear");
        overrideAzimuth = serializedObject.FindProperty("overrideAzimuth");
        artisticSunAzimuth = serializedObject.FindProperty("artisticSunAzimuth");
        // overcastFactor = serializedObject.FindProperty("overcastFactor");    // working but disabled and not advertised
        sceneLightLayerMask = serializedObject.FindProperty("sceneLightLayerMask");
        selectedLocationPreset = serializedObject.FindProperty("selectedLocationPreset");
        inspectorChanged = serializedObject.FindProperty("inspectorChanged");
        // New in version 1.1
        northDirection = serializedObject.FindProperty("northDirection");
        //
        /*
		serializedObject.Update();	
		progressTime.boolValue = true;
		timeProgressFactor.floatValue=1.0;
		dayOfYear.intValue = 90;
		timeInHours.floatValue = 12.0;
		sceneLightLayerMask.intValue = -1;
		serializedObject.ApplyModifiedProperties();
		*/
        lat = new float[11];
        lon = new float[11];
        offs = new float[11];
        //custom_location
        lat[0] = 0f;
        lon[0] = 0f;
        offs[0] = 0f;
        //Reykjavik
        lat[1] = 64.133333f;
        lon[1] = -21.933333f;
        offs[1] = 0f;
        //Berlin
        lat[2] = 52.518611f;
        lon[2] = 13.408333f;
        offs[2] = 1f;
        //New_York
        lat[3] = 40.712778f;
        lon[3] = -74.005833f;
        offs[3] = -5f;
        //Baghdad
        lat[4] = 33.333333f;
        lon[4] = 44.383333f;
        offs[4] = 3f;
        //Hongkong
        lat[5] = 22.3f;
        lon[5] = 114.166667f;
        offs[5] = 8f;
        //Panama
        lat[6] = 8.983333f;
        lon[6] = -79.516667f;
        offs[6] = -5f;
        //Singapore
        lat[7] = 1.283333f;
        lon[7] = 103.833333f;
        offs[7] = 8f;
        //Rio_de_Janeiro
        lat[8] = -22.908333f;
        lon[8] = -43.196389f;
        offs[8] = -3f;
        //Sydney
        lat[9] = -33.85f;
        lon[9] = 151.2f;
        offs[9] = 10f;
        //Falkland_Islands
        lat[10] = -51.683333f;
        lon[10] = -59.166667f;
        offs[10] = -4f;
    }

    public virtual LayerMask EditorMask(string editorLabel, LayerMask selection)
    {
        int i = 0;
        int undefinedLayers = 0;
        if (layers == null)
        {
            layers = new List<string>();
            namesArray = new string[4];
        }
        else
        {
            layers.Clear();
        }
        i = 0;
        while (i < 32)
        {
            string layerName = LayerMask.LayerToName(i);
            if (layerName != "")
            {
                while (undefinedLayers > 0)
                {
                    layers.Add("Layer " + (i - undefinedLayers));
                    undefinedLayers--;
                }
                layers.Add(layerName);
            }
            else
            {
                undefinedLayers++;
            }
            i++;
        }
        if (namesArray.Length != layers.Count)
        {
            namesArray = new string[layers.Count];
        }
        i = 0;
        while (i < namesArray.Length)
        {
            namesArray[i] = layers[i];
            i++;
        }
        selection.value = EditorGUILayout.MaskField(new GUIContent(editorLabel, "The mask used for selectively lighting objects in the scene."), selection.value, namesArray, new GUILayoutOption[] {});
        return selection;
    }


}