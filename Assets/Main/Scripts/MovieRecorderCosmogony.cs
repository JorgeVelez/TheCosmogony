using System.IO;
#if UNITY_EDITOR

using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
#endif
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class MovieRecorderCosmogony : MonoBehaviour
{
    public ColorCorrectionController colorController;

    bool isMonitoring = false;

    int currentVolumeRecorded = 0;

    string state = "iddle";

#if UNITY_EDITOR

    RecorderController TestRecorderController;
    RecorderControllerSettings m_ControllerSettings;
#endif
    string mediaOutputFolder;

    public GameObject cameraToHide;
    
    public GameObject cinemachineGO;

    #if UNITY_EDITOR

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

    public void startCinemachineRecording()
    {

        colorController.StopProcessing();
        colorController.resetAll();

        cameraToHide.SetActive(false);
        cinemachineGO.SetActive(true);

        //string name=colorController.triggerVolume(currentVolumeRecorded);
        m_ControllerSettings.RecorderSettings.FirstOrDefault().OutputFile = Path.Combine(mediaOutputFolder, "promo");

        Invoke("startRecordingWindowCine", .1f);
    }

    

    void startRecordingWindowCine()
    {
        isMonitoring = true;

        state = "recordingcine";
        TestRecorderController.PrepareRecording();
        TestRecorderController.StartRecording();
        //recorderWindow.StartRecording();
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

            if (state == "recordingcine" && !TestRecorderController.IsRecording())
            {

                Debug.Log("finished cine");
                    UnityEditor.EditorApplication.isPlaying = false;
            }
        }
    }
#endif

}
