using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RaindropFX;

namespace RaindropFX {
    [ExecuteInEditMode]
    public class MaterialLinker : MaterialLinkerBase {
        #region public params
        public string normalMap_PropName = "_BumpMap";
        public string heightMap_PropName = "_HeightMap";
        public string fogMask_PropName = "_WetMap";
        public string wipeMask_PropName = "_WipeMap";

        [HideInInspector]
        public Vector2Int ScreenCaptureSize = new Vector2Int(800, 450);
        #endregion

        #region private params
        private RenderTexture screenColor = null;
        #endregion

        private void CaptureScreen() {
            if (screenColor == null || screenColor.width != ScreenCaptureSize.x ||
                screenColor.height != ScreenCaptureSize.y)
                screenColor = new RenderTexture(
                    ScreenCaptureSize.x, ScreenCaptureSize.y, 
                    16, RenderTextureFormat.ARGBFloat, 5
                );
            this.gameObject.SetActive(false);
            Camera.main.targetTexture = screenColor;
            Camera.main.Render();
            Camera.main.targetTexture = null;
            this.gameObject.SetActive(true);
        }
        
        private void Update() {
            Solve(ref wipeDelta);

            // crest ocean fix
            //CaptureScreen();

            targetMat.SetFloat("_IsUseWipe", wipeEffect ? 1.0f : 0.0f);
            if (wipeEffect && wipeDelta != null) {
                //targetMat.SetTexture("_MainTex", wipeMask); // debug
                targetMat.SetTexture(wipeMask_PropName, wipeMask);
            }

            if (normalMap != null && heightMap != null && fogMask != null) {
                targetMat.SetTexture(normalMap_PropName, normalMap);
                targetMat.SetTexture(heightMap_PropName, heightMap);
                targetMat.SetTexture(fogMask_PropName, fogMask);

                //targetMat.SetTexture("_ScreenColor", screenColor);
            }
        }

    }

}