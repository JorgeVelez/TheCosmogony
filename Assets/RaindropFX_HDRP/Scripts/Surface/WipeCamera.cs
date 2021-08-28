using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;

namespace RaindropFX {
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class WipeCamera : MonoBehaviour {

        #region public params
        public bool hideWiper = false;
        public MaterialLinker linker;
        public int cullLayer = 30;
        public Vector2Int maskSize = new Vector2Int(256, 256);
        public List<GameObject> wipers = new List<GameObject>();
        #endregion

        #region private params
        private RenderTexture wipeTexture = null;
        private bool isHide = false;
        #endregion

        private void Start() {
            Camera cam = this.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.cullingMask = 1 << this.gameObject.layer;
            cam.backgroundColor = Color.black;

            var hddata = cam.GetComponent<HDAdditionalCameraData>();
            if (hddata == null) hddata = cam.gameObject.AddComponent<HDAdditionalCameraData>();
            RaindropFX_Tools.ConfigureHDData(ref hddata);

            wipeTexture = new RenderTexture(maskSize.x, maskSize.y, 0);
            wipeTexture.name = "wipeMask";
            cam.targetTexture = wipeTexture;

            foreach (GameObject obj in wipers) obj.layer = cullLayer;
            cam.cullingMask = 1 << cullLayer;
        }
        
        private void Update() {
            if (linker != null && wipeTexture != null)
                linker.wipeDelta = wipeTexture;

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