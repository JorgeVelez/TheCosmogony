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
        if(GUILayout.Button("Start Batch Recording")){
            movieRecorderCosmogony.startRecording();
        }
    }
}