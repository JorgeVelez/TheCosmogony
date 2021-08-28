using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RaindropFX {
    [ExecuteInEditMode]
    public class CommandBufferBlurRefraction : MonoBehaviour {
        #region parameters
        public bool reset = false;
        public Material targetMat;
        [HideInInspector]
        public Material blurMat;
        
        private Dictionary<Camera, CommandBuffer> camSet = new Dictionary<Camera, CommandBuffer>();
        #endregion


        private void Cleanup() {
            foreach (var cam in camSet) if (cam.Key)
                cam.Key.RemoveCommandBuffer(CameraEvent.AfterSkybox, cam.Value);
            camSet.Clear();
            Object.DestroyImmediate(blurMat);
        }

        private void OnValidate() {
            reset = false;
            Cleanup();
        }

        private void ParamConstraints() {
            if (!targetMat) targetMat = this.GetComponent<MeshRenderer>().sharedMaterial;
            if (!blurMat) {
                //blurMat = new Material(Shader.Find("Custom/RaindropFX/SeparableBlur"));
                //blurMat.hideFlags = HideFlags.HideAndDontSave;
                blurMat = new Material(Shader.Find("Hidden/Custom/GaussianBlur"));
            }
        }

        public void OnEnable() {
            Cleanup();
        }

        public void OnDisable() {
            Cleanup();
        }

        public void OnWillRenderObject() {
            ParamConstraints();
            
            var act = gameObject.activeInHierarchy && enabled;
            if (!act) {
                Cleanup();
                return;
            }

            Camera cam = Camera.current;
            if (!cam) return;
            if (camSet.ContainsKey(cam)) return;

            CommandBuffer buf = new CommandBuffer();
            buf.name = "Grab screen and blur";
            camSet[cam] = buf;

            int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
            buf.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear);
            buf.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            int copyID = Shader.PropertyToID("_Temp3");
            buf.GetTemporaryRT(copyID, -2, -2, 0, FilterMode.Bilinear);
            buf.GetTemporaryRT(blurredID, -2, -2, 0, FilterMode.Bilinear);
            buf.GetTemporaryRT(blurredID2, -2, -2, 0, FilterMode.Bilinear);

            buf.Blit(screenCopyID, copyID);
            buf.Blit(screenCopyID, blurredID);
            buf.ReleaseTemporaryRT(screenCopyID);
            
            for (var i = 1; i <= 2; i++) {
                for (var pass = 1; pass < 3; pass++) {
                    buf.Blit(blurredID, blurredID2, blurMat, pass);
                    var tmpSwap = blurredID;
                    blurredID = blurredID2;
                    blurredID2 = tmpSwap;
                }
            }

            buf.SetGlobalTexture("_GrabOriginTexture", copyID);
            buf.SetGlobalTexture("_GrabBlurTexture", blurredID);

            cam.AddCommandBuffer(CameraEvent.AfterSkybox, buf);
        }
    }
}