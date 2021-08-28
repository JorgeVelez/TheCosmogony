using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;

namespace RaindropFX {
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class Wiper : MonoBehaviour {
        #region public params
        public bool hideWiper = false;
        public Volume postVolumn;
        public int cullLayer = 30;
        public List<GameObject> wipers = new List<GameObject>();
        #endregion

        #region private params
        private RenderTexture wipeTexture;
        private GameObject lutCam = null;
        private Camera cam = null;
        private HDAdditionalCameraData hddata;
        private RaindropFX_HDRP hdrp = null;
        private bool isHide = false;
        #endregion

        private void CreateCamera() {
            lutCam = new GameObject("RFX_LUT_CAM");
            lutCam.transform.SetParent(this.transform);
            lutCam.transform.localPosition = Vector3.zero;
            lutCam.transform.localRotation = Quaternion.identity;

            cam = lutCam.AddComponent<Camera>();
            //cam.CopyFrom(this.GetComponent<Camera>());
            hddata = lutCam.GetComponent<HDAdditionalCameraData>();
            if (hddata == null) hddata = lutCam.AddComponent<HDAdditionalCameraData>();

            // configure lut camera
            RaindropFX_Tools.ConfigureHDData(ref hddata);
            
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            wipeTexture = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24, RenderTextureFormat.ARGBFloat);
            wipeTexture.name = "wipeMask";
            cam.targetTexture = wipeTexture;

            foreach (GameObject obj in wipers) obj.layer = cullLayer;
            cam.cullingMask = 1 << cullLayer;
        }

        public void GetWiped(RaindropFX_HDRP target) {
            StartCoroutine(GetWipedI(target));
        }

        private IEnumerator GetWipedI(RaindropFX_HDRP target) {
            yield return (new WaitForEndOfFrame());
            RaindropFX_Tools.RenderTextureToTexture2D(ref target.tempWip, ref target.solver.calcRainTex);
        }

        private void OnRenderObject() {
            if (lutCam == null) CreateCamera();

            if (hdrp == null) postVolumn.sharedProfile.TryGet<RaindropFX_HDRP>(out hdrp);
            else hdrp.wipeTex.value = wipeTexture;

            if (isHide != hideWiper) {
                isHide = hideWiper;
                foreach (GameObject obj in wipers) {
                    if (obj == null) continue;
                    var renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.enabled = !hideWiper;
                }
            }
        }

    }
}