using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovieRecorderCosmogony))]
[CanEditMultipleObjects]
public class MovieRecorderCosmogonyEditor : Editor 
{
    MovieRecorderCosmogony movieRecorderCosmogony;
    void OnEnable()
    {
        movieRecorderCosmogony=(MovieRecorderCosmogony)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Start Weather Batch Recording")){
            movieRecorderCosmogony.startRecording();
        }

        if(GUILayout.Button("Start Cinemachine Recording")){
            movieRecorderCosmogony.startCinemachineRecording();
        }
    }
}