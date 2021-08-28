using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace RaindropFX {

    [Serializable, VolumeComponentMenu("Post-processing/RaindropFX/RaindropFX_GPU")]
    public sealed class RaindropFX_GPU : CustomPostProcessVolumeComponent, IPostProcessComponent {

        #region system properties
        [Tooltip("Waterdrops will fade in/out automatically if you disable/enable this.")]
        public BoolParameter fadeout_fadein_switch = new BoolParameter(false);

        [Tooltip("Noise texture is used to specify where raindrops should be generated.")]
        public TextureParameter _noiseTex = new TextureParameter(null);

        [Tooltip("Frame interval of texture rendering.")]
        public IntParameter refreshRate = new IntParameter(10);

        [Tooltip("Spawn rate of static raindrops.")]
        public ClampedFloatParameter StaticSpawnRate = new ClampedFloatParameter(5.0f, 0f, 16f);

        [Tooltip("Spawn rate of dynamic raindrops.")]
        public ClampedFloatParameter DynamicSpawnRate = new ClampedFloatParameter(1.0f, 0f, 3.6f);

        [Tooltip("Fusion small droplets.")]
        public ClampedIntParameter fusion = new ClampedIntParameter(2, 0, 15);

        [Tooltip("Gravity adjustment.")]
        public Vector2Parameter gravity = new Vector2Parameter(new Vector2(0, -9.8f));

        [Tooltip("Wind power adjustment.")]
        public Vector2Parameter wind = new Vector2Parameter(Vector2.zero);

        [Tooltip("Adjust scale of wind turbulence.")]
        public ClampedFloatParameter windTurbulence = new ClampedFloatParameter(1.0f, 0f, 10f);

        [Tooltip("Screen blend effect intensity.")]
        public ClampedFloatParameter distortion = new ClampedFloatParameter(0.6f, 0, 10);

        [Tooltip("Color level parameter.")]
        public ClampedFloatParameter _inBlack = new ClampedFloatParameter(55.0f, 0f, 255f);

        [Tooltip("Color level parameter.")]
        public ClampedFloatParameter _inWhite = new ClampedFloatParameter(180.0f, 0f, 255f);

        [Tooltip("Color level parameter.")]
        public ClampedFloatParameter _outWhite = new ClampedFloatParameter(160.0f, 0f, 255f);

        [Tooltip("Color level parameter.")]
        public ClampedFloatParameter _outBlack = new ClampedFloatParameter(5.0f, 0f, 255f);

        [Tooltip("Debug raindrop texture.")]
        public BoolParameter debug = new BoolParameter(false);
        #endregion

        #region parameters
        Material BTS_Mat;
        Material SIM_Mat;
        Material BLEND_Mat;
        Material BLUR_Mat;

        int clock = 0;
        bool swapBuffer = true;
        private RenderTexture[] _simBuffer = null;

        public bool IsActive() => (_noiseTex.value != null && !(Application.isEditor && !Application.isPlaying));
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
        #endregion

        public override void Setup() {
            if (Shader.Find("Shader/RaindropFX/BTS") != null)
                BTS_Mat = new Material(Shader.Find("Shader/RaindropFX/BTS"));
            if (Shader.Find("Shader/RaindropFX/GPUCore") != null)
                SIM_Mat = new Material(Shader.Find("Shader/RaindropFX/GPUCore"));
            if (Shader.Find("Shader/RaindropFX/Blend_HDRP_GPU") != null)
                BLEND_Mat = new Material(Shader.Find("Shader/RaindropFX/Blend_HDRP_GPU"));
            if (Shader.Find("Hidden/Custom/GaussianBlur_GPU") != null)
                BLUR_Mat = new Material(Shader.Find("Hidden/Custom/GaussianBlur_GPU"));
        }

        //---------------------------------
        // Calculate raindrops animation
        //---------------------------------
        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination) {
            if (Application.isEditor && !Application.isPlaying) return;
            if (_noiseTex.value == null) return;

            SafetyCheck();
            if (clock++ >= refreshRate.value) {
                //HDUtils.DrawFullScreen(cmd, BTS_Mat, destination);
                //} else {
                clock = 0;
                SIM_Mat.SetTexture("_NoiseTex", _noiseTex.value);
                SIM_Mat.SetFloat("_staDens", StaticSpawnRate.value);
                SIM_Mat.SetFloat("_dynDens", DynamicSpawnRate.value);
                SIM_Mat.SetFloat("_TurbScale", windTurbulence.value);
                SIM_Mat.SetInt("_fadeout", fadeout_fadein_switch.value ? 0 : 1);
                SIM_Mat.SetVector("_Force", new Vector4(wind.value.x + gravity.value.x, wind.value.y + gravity.value.y));
                cmd.Blit(_simBuffer[swapBuffer ? 1 : 0], _simBuffer[swapBuffer ? 0 : 1], SIM_Mat);
                swapBuffer = !swapBuffer;
                if (debug.value) {
                    BTS_Mat.SetTexture("_MainTex", _simBuffer[swapBuffer ? 0 : 1]);
                    HDUtils.DrawFullScreen(cmd, BTS_Mat, destination);
                    return;
                }
            }

            // apply blur effect to solver.calcRainTex
            var result = RenderTexture.GetTemporary(
                _simBuffer[0].width, _simBuffer[0].height, 0
            );
            RaindropFX_Tools.Blur(
                _simBuffer[swapBuffer ? 0 : 1],
                result,
                fusion.value, BLUR_Mat
            );

            // pixelize the raindrop
            BLEND_Mat.SetFloat("_PixelSize", -1);

            // convert height map to normal map and create screen blend effect
            SetBlendMat(ref result, ref result, ref result, ref source);

            // output final result
            HDUtils.DrawFullScreen(cmd, BLEND_Mat, destination);

            RenderTexture.ReleaseTemporary(result);
        }

        private void SetBlendMat(ref RenderTexture heightMap, ref RenderTexture wetMap, ref RenderTexture wipeMap, ref RTHandle background) {
            //BLEND_Mat.SetColor("_TintColor", tintColor.value);
            //BLEND_Mat.SetColor("_FogTint", fogTint.value);
            //BLEND_Mat.SetInt("_IsUseFog", useFog.value ? 1 : 0);
            //BLEND_Mat.SetInt("_IsUseWipe", wipeEffect.value ? 1 : 0);
            BLEND_Mat.SetFloat("_Distortion", distortion.value);
            //BLEND_Mat.SetFloat("_FogIntensity", fogIntensity.value);
            //BLEND_Mat.SetFloat("_FogIteration", fogIteration.value);
            BLEND_Mat.SetTexture("_HeightMap", heightMap);
            //BLEND_Mat.SetTexture("_WetMap", wetMap);
            //BLEND_Mat.SetTexture("_WipeMap", wipeMap);
            //BLEND_Mat.SetTexture("_CullMask", _rainMask_grayscale.value);
            BLEND_Mat.SetTexture("_MainTex", background);
            BLEND_Mat.SetVector("_MainTex_TexelSize", new Vector4(
                heightMap.width, heightMap.height, 0, 0
            ));

            BLEND_Mat.SetInt("_IsUseFog", 0);
            BLEND_Mat.SetInt("_IsUseWipe", 0);

            BLEND_Mat.SetFloat("_inBlack", _inBlack.value);
            BLEND_Mat.SetFloat("_inWhite", _inWhite.value);
            BLEND_Mat.SetFloat("_outWhite", _outWhite.value);
            BLEND_Mat.SetFloat("_outBlack", _outBlack.value);
        }

        public override void Cleanup() {
            CoreUtils.Destroy(BTS_Mat);
            CoreUtils.Destroy(SIM_Mat);
            CoreUtils.Destroy(BLEND_Mat);
            CoreUtils.Destroy(BLUR_Mat);
        }
        
        private void SafetyCheck() {
            if (_simBuffer == null) {
                _simBuffer = new RenderTexture[2];
                _simBuffer[0] = new RenderTexture(Screen.width, Screen.height, 0);
                _simBuffer[1] = new RenderTexture(Screen.width, Screen.height, 0);
            }
        }
    }

}