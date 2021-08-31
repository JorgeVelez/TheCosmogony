using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class MovieRecorderCosmogony : MonoBehaviour
{
    public ColorCorrectionController colorController;

    bool isMonitoring = false;

    int currentVolumeRecorded = 0;

    string state = "iddle";

    RecorderController TestRecorderController;
    RecorderControllerSettings m_ControllerSettings;

    string mediaOutputFolder;
    void Awake()
    {
        m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + "/../Library/Recorder/recorder.pref");

        TestRecorderController = new RecorderController(m_ControllerSettings);

        mediaOutputFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "movies");

    }


    public void startRecording()
    {

        colorController.StopProcessing();
        colorController.resetAll();


        string name=colorController.triggerVolume(currentVolumeRecorded);
        m_ControllerSettings.RecorderSettings.FirstOrDefault().OutputFile = Path.Combine(mediaOutputFolder, name);

        Invoke("startRecordingWindow", 2f);
    }

    void startRecordingWindow()
    {
        isMonitoring = true;

        state = "recording";
        TestRecorderController.PrepareRecording();
        TestRecorderController.StartRecording();
        //recorderWindow.StartRecording();
    }

    void Update()
    {
        if (isMonitoring)
        {
            if (state == "recording" && !TestRecorderController.IsRecording())
            {
                state = "finishedVolume";

                Debug.Log("finished");
                currentVolumeRecorded++;
                if (currentVolumeRecorded < colorController.volumes.Count)
                    startRecording();
                else
                    UnityEditor.EditorApplication.isPlaying = false;
            }
        }
    }


}
