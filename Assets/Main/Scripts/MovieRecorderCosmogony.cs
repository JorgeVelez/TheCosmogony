using System.Collections;
using UnityEditor.Recorder;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MovieRecorderCosmogony : MonoBehaviour
{
    RecorderWindow recorderWindow;


    private RecorderWindow GetRecorderWindow()
    {
        return (RecorderWindow)EditorWindow.GetWindow(typeof(RecorderWindow));
    }

    void Awake()
    {
        recorderWindow = GetRecorderWindow();
    }

    public void startRecording()
    {
        if (!recorderWindow.IsRecording())
            recorderWindow.StartRecording();
    }

    public void stopRecording()
    {
        if (recorderWindow.IsRecording())
            recorderWindow.StopRecording();
    }
}
