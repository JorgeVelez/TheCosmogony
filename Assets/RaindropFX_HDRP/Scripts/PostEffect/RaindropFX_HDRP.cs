using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace RaindropFX {

    [Serializable, VolumeComponentMenu("Post-processing/RaindropFX/RaindropFX_HDRP")]
    public sealed class RaindropFX_HDRP : CustomPostProcessVolumeComponent, IPostProcessComponent {

        #region system properties
        // basic properties -------------------------------------------------------------------------------
        [Header("Basic Settings")]
        [Tooltip("Waterdrops will fade in/out automatically if you disable/enable this.")]
        public BoolParameter fadeout_fadein_switch = new BoolParameter(false);

        [Tooltip("Waterdrops will fade in/out with higher frame rate but lower accuracy.")]
        public BoolParameter fastMode = new BoolParameter(false);

        [Tooltip("Control the speed of fadeout, the bigger, the faster.")]
        public ClampedFloatParameter fadeSpeed = new ClampedFloatParameter(0.02f, 0.01f, 1);

        [Tooltip("Frame interval of texture rendering.")]
        public ClampedIntParameter refreshRate = new ClampedIntParameter(1, 0, 10);

        [Tooltip("Please use droplet texture with alpha channel.")]
        public TextureParameter _raindropTex_alpha = new TextureParameter(null);

        [Tooltip("Specifies the size of the rendered texture.")]
        public BoolParameter forceRainTextureSize = new BoolParameter(true);

        [Tooltip("Specifies the size of the rendered texture.")]
        public Vector2Parameter calcRainTextureSize = new Vector2Parameter(new Vector2Int(800, 450));

        [Tooltip("If rain texture size is not forced, size = current screen resolution * downSampling.")]
        public ClampedFloatParameter downSampling = new ClampedFloatParameter(0.5f, 0.125f, 8.0f);

        // post properties --------------------------------------------------------------------------------
        [Header("Special Post Effects")]
        [Tooltip("Use a grayscale image to specify the screen area affected by water droplets, " +
            "with black color representing culling area.")]
        public TextureParameter _rainMask_grayscale = new TextureParameter(null);

        [Tooltip("Enable this if you want to pixelize the raindrops.")]
        public BoolParameter pixelization = new BoolParameter(false);

        [Range(1, 1024), Tooltip("Pixelization the raindrop texture, set size of a pixel.")]
        public FloatParameter pixResolution = new ClampedFloatParameter(1, 1, 1024);

        // interactive properties ------------------------------------------------------------------------
        [Header("Interactive Settings")]
        [Tooltip("Hold left mouse to draw raindrops on screen.")]
        public BoolParameter mousePainting = new BoolParameter(false);

        [Tooltip("Allow you to wipe the raindrops via GameObject.")]
        public BoolParameter wipeEffect = new BoolParameter(false);

        [HideInInspector]
        [Tooltip("Add grayscale texture here to wipe the raindrops.")]
        public TextureParameter wipeTex = new TextureParameter(null);

        [Tooltip("The speed of screen fog recovery after being wiped.")]
        public FloatParameter foggingSpeed = new ClampedFloatParameter(0.98f, 0, 1);

        // physical properties ----------------------------------------------------------------------------
        [Header("Physical Settings")]
        [Tooltip("Time step of physical computing.")]
        public ClampedFloatParameter calcTimeStep = new ClampedFloatParameter(0.1f, 0, 1);

        [Tooltip("Enable this if you want to use wind.")]
        public BoolParameter useWind = new BoolParameter(false);

        [Tooltip("Enable radial wind, mostly for driving simulation.")]
        public BoolParameter radialWind = new BoolParameter(false);

        [Tooltip("Enable wind turbulence.")]
        public ClampedFloatParameter windTurbulence = new ClampedFloatParameter(0.1f, 0, 1);

        [Tooltip("Adjust scale of wind turbulence.")]
        public ClampedFloatParameter windTurbScale = new ClampedFloatParameter(1.0f, 0.01f, 10);

        [Tooltip("Wind power adjustment.")]
        public Vector2Parameter wind = new Vector2Parameter(new Vector2(0.0f, 0.0f));

        [Tooltip("Gravity adjustment.")]
        public Vector2Parameter gravity = new Vector2Parameter(new Vector2(0.0f, -9.8f));

        [Tooltip("Friction adjustment.")]
        public ClampedFloatParameter friction = new ClampedFloatParameter(0.8f, 0, 1);

        // raindrop properties ------------------------------------------------------------------------
        [Header("Raindrop Settings")]
        [Tooltip("Dynamic water droplets produce a tail when they slide if you enable this.")]
        public BoolParameter generateTrail = new BoolParameter(true);

        [Tooltip("Max number of static raindrops.")]
        public ClampedIntParameter maxStaticRaindropNumber = new ClampedIntParameter(5000, 0, 10000);

        [Tooltip("Max number of dynamic raindrops.")]
        public ClampedIntParameter maxDynamicRaindropNumber = new ClampedIntParameter(10, 0, 1000);

        [HideInInspector]
        public ClampedIntParameter spawnRateS = new ClampedIntParameter(5, 0, 10000);
        [HideInInspector]
        public ClampedIntParameter spawnRateD = new ClampedIntParameter(1, 0, 10000);

        [Tooltip("Random droplet size range.")]
        public Vector2Parameter raindropSizeRange = new Vector2Parameter(new Vector2(0.1f, 0.25f));

        // rednergin properties ------------------------------------------------------------------------
        [Header("Rendering Settings")]
        [Tooltip("Tint color of droplets.")]
        public ColorParameter tintColor = new ColorParameter(Color.white);

        [Tooltip("The larger the value, the thicker the color.")]
        public ClampedFloatParameter tintAmount = new ClampedFloatParameter(0.5f, 0.0f, 2.0f);

        [Tooltip("Fusion droplets.")]
        public ClampedIntParameter fusion = new ClampedIntParameter(2, 0, 15);

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

        // Fog properties ---------------------------------------------------------------------------------
        [Header("Fog Settings")]
        [Tooltip("If you want to use screen fog, enable this.")]
        public BoolParameter useFog = new BoolParameter(false);

        [Tooltip("Tint color of fog.")]
        public ColorParameter fogTint = new ColorParameter(Color.white);

        [Tooltip("Screen fog effect intensity.")]
        public ClampedFloatParameter fogIntensity = new ClampedFloatParameter(0.5f, 0.01f, 1);

        [Tooltip("Controls the effect of water droplet wake on fog.")]
        public ClampedIntParameter fogIteration = new ClampedIntParameter(2, 1, 5);

        // DOF properties ---------------------------------------------------------------------------------
        [Header("Depth-of-Field Settings")]
        [Tooltip("Enable this if you want to blur waterdrops.")]
        public BoolParameter dropletBlur = new BoolParameter(false);

        [Tooltip("Invert blur mask.")]
        public BoolParameter invertBlur = new BoolParameter(false);

        [Tooltip("Smoothness of depth mixing.")]
        public ClampedIntParameter edgeSoftness = new ClampedIntParameter(10, 1, 100);

        [Tooltip("Adjust focal length.")]
        public ClampedFloatParameter _focalize = new ClampedFloatParameter(1.0f, 0.0f, 10.0f);

        [Tooltip("Adjust blur strength.")]
        public ClampedIntParameter blurIteration = new ClampedIntParameter(1, 0, 10);

        // TODO: detail properties ------------------------------------------------------------------------
        //[Header("Detail Settings")]
        //[Tooltip("Add more detail to frosted glass(works with 'invertBlur' on)")]
        //public TextureParameter detailMask = new TextureParameter(null);
        #endregion

        #region parameters
        Material BTS_Mat;
        Material GRAB_Mat;
        Material BLEND_Mat;
        Material WIPE_Mat;
        Material TIME_Mat;
        Material DEPTH_Mat;
        Wiper wiper = null;
        bool initFlag = false;
        bool cnt = false;

        RenderTexture temp;
        RenderTexture temp2;

        public RenderTexture tempWip;
        public RenderTexture wipDelta;

        public RaindropGenerator solver;
        public bool IsActive() => (_raindropTex_alpha.value != null && !(Application.isEditor && !Application.isPlaying));
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
        #endregion

        public override void Setup() {
            if (Shader.Find("Shader/RaindropFX/GrabScr") != null)
                GRAB_Mat = new Material(Shader.Find("Shader/RaindropFX/GrabScr"));
            if (Shader.Find("Shader/RaindropFX/BTS") != null)
                BTS_Mat = new Material(Shader.Find("Shader/RaindropFX/BTS"));
            if (Shader.Find("Shader/RaindropFX/Blend_HDRP") != null)
                BLEND_Mat = new Material(Shader.Find("Shader/RaindropFX/Blend_HDRP"));
            if (Shader.Find("Shader/RaindropFX/Wiper") != null)
                WIPE_Mat = new Material(Shader.Find("Shader/RaindropFX/Wiper"));
            if (Shader.Find("Shader/RaindropFX/TimeDelta") != null)
                TIME_Mat = new Material(Shader.Find("Shader/RaindropFX/TimeDelta"));
            if (Shader.Find("Custom/ForwardDecal_HDRP") != null)
                DEPTH_Mat = new Material(Shader.Find("Custom/ForwardDecal_HDRP"));
        }

        public void ClearAll() {
            if (solver != null)
                solver.ReInit(
                    new Vector2Int(
                        (int)calcRainTextureSize.value.x, 
                        (int)calcRainTextureSize.value.y
                    )
                );
        }

        //---------------------------------
        // Calculate raindrops animation
        //---------------------------------
        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination) {
            if (Application.isEditor && !Application.isPlaying) return;
            if (_raindropTex_alpha.value != null) {
                Constraints();
                solver.UpdateProps(
                    fadeout_fadein_switch.value, fastMode.value, fadeSpeed.value,
                    false, Vector2Int.zero, calcTimeStep.value, refreshRate.value,
                    generateTrail.value, maxStaticRaindropNumber.value, maxDynamicRaindropNumber.value,
                    raindropSizeRange.value, useWind.value, radialWind.value, windTurbulence.value,
                    windTurbScale.value, wind.value, gravity.value, friction.value,
                    distortion.value, useFog.value, fogIntensity.value, fogIteration.value, 
                    fusion.value, _inBlack.value, _inWhite.value, _outWhite.value, _outBlack.value,
                    dropletBlur.value, _focalize.value, blurIteration.value, tintColor.value, 
                    fogTint.value, edgeSoftness.value, pixelization.value, pixResolution.value,
                    downSampling.value, foggingSpeed.value, spawnRateS.value, spawnRateD.value
                ); solver.CalcRainTex();

                // apply wipe effect
                if (wipeEffect.value && wipeTex.value != null) {
                    var tempD = RenderTexture.GetTemporary(solver.calcTexSize.x, solver.calcTexSize.y, 0);
                    if (cnt) cmd.Blit(wipDelta, tempD);
                    else RaindropFX_Tools.Blur(cmd, wipDelta, tempD, 1, solver.blur_material, solver.calcTexSize);
                    cnt = !cnt;
                    TIME_Mat.SetFloat("_ratio", (float)solver.calcTexSize.x / solver.calcTexSize.y);
                    TIME_Mat.SetFloat("_speed", foggingSpeed.value * 0.986f);
                    TIME_Mat.SetTexture("_WipeTex", wipeTex.value);
                    cmd.Blit(tempD, wipDelta, TIME_Mat);

                    WIPE_Mat.SetTexture("_WipeTex", wipeTex.value);
                    cmd.Blit(solver.calcRainTex, tempWip, WIPE_Mat);

                    wiper.GetWiped(this);
                    RenderTexture.ReleaseTemporary(tempD);
                } //else RaindropFX_Tools.PrintLog("wiper is null!");

                // apply blur effect to solver.calcRainTex
                RaindropFX_Tools.Blur(cmd, solver.calcRainTex, temp, fusion.value, solver.blur_material, solver.calcTexSize);
                
                // apply color level to solver.calcRainTex
                solver.SetLevelMat();
                cmd.Blit(temp, temp2, solver.level_material);

                // pixelize the raindrop
                if (solver.pixelization) {
                    BLEND_Mat.SetFloat("_PixelSize", solver.pixResolution);
                    solver.blend_material.SetFloat("_PixelSize", solver.pixResolution);
                } else {
                    BLEND_Mat.SetFloat("_PixelSize", -1);
                    solver.blend_material.SetFloat("_PixelSize", -1);
                }

                // convert height map to normal map and create screen blend effect
                SetBlendMat(ref temp2, ref temp, ref wipDelta, ref source);
                
                // output final result
                if (dropletBlur.value) {
                    // blur foreground droplets
                    var scrSize = RaindropFX_Tools.GetDownSize(1.0f);
                    var temp3 = RenderTexture.GetTemporary(scrSize.x, scrSize.y, 0);
                    var temp4 = RenderTexture.GetTemporary(scrSize.x, scrSize.y, 0);
                    var result = RenderTexture.GetTemporary(scrSize.x, scrSize.y, 0);
                    var grab = RenderTexture.GetTemporary(scrSize.x, scrSize.y, 0);

                    GRAB_Mat.SetTexture("_MainTex", source);
                    cmd.Blit(temp, grab, GRAB_Mat);
                    cmd.Blit(grab, temp3, solver.blend_material);
                    RaindropFX_Tools.Blur(cmd, temp3, temp4, blurIteration.value, solver.blur_material, solver.calcTexSize);

                    if (invertBlur.value) {
                        //solver.dropblur_material.SetTexture("_DetailMask", detailMask.value);
                        solver.SetDropblurMat(ref temp3, ref temp);
                        cmd.Blit(temp4, result, solver.dropblur_material);
                    } else {
                        solver.SetDropblurMat(ref temp4, ref temp);
                        cmd.Blit(temp3, result, solver.dropblur_material);
                    }
                    BTS_Mat.SetTexture("_Origin", grab);
                    BTS_Mat.SetTexture("_MainTex", result);
                    BTS_Mat.SetTexture("_CullMask", _rainMask_grayscale.value);
                    HDUtils.DrawFullScreen(cmd, BTS_Mat, destination);

                    RenderTexture.ReleaseTemporary(temp3);
                    RenderTexture.ReleaseTemporary(temp4);
                    RenderTexture.ReleaseTemporary(result);
                    RenderTexture.ReleaseTemporary(grab);
                } else {
                    HDUtils.DrawFullScreen(cmd, BLEND_Mat, destination);
                }
            } else {
                RaindropFX_Tools.PrintLog("raindrop tex is null!");
            }
        }

        public override void Cleanup() {
            CoreUtils.Destroy(BTS_Mat);
            CoreUtils.Destroy(BLEND_Mat);
            CoreUtils.Destroy(GRAB_Mat);
            CoreUtils.Destroy(WIPE_Mat);
            CoreUtils.Destroy(TIME_Mat);
        }

        private void SetBlendMat(ref RenderTexture heightMap, ref RenderTexture wetMap, ref RenderTexture wipeMap, ref RTHandle background) {
            solver.SetScreenBlendMat(ref heightMap, ref wetMap, ref wipeMap, _rainMask_grayscale.value, wipeEffect.value);
            BLEND_Mat.SetColor("_TintColor", tintColor.value);
            BLEND_Mat.SetFloat("_TintAmt", tintAmount.value);
            BLEND_Mat.SetColor("_FogTint", fogTint.value);
            BLEND_Mat.SetInt("_IsUseFog", useFog.value ? 1 : 0);
            BLEND_Mat.SetInt("_IsUseWipe", wipeEffect.value ? 1 : 0);
            BLEND_Mat.SetFloat("_Distortion", distortion.value);
            BLEND_Mat.SetFloat("_FogIntensity", fogIntensity.value);
            BLEND_Mat.SetFloat("_FogIteration", fogIteration.value);
            BLEND_Mat.SetTexture("_HeightMap", heightMap);
            BLEND_Mat.SetTexture("_WetMap", wetMap);
            BLEND_Mat.SetTexture("_WipeMap", wipeMap);
            BLEND_Mat.SetTexture("_CullMask", _rainMask_grayscale.value);
            BLEND_Mat.SetTexture("_MainTex", background);
            BLEND_Mat.SetVector("_MainTex_TexelSize", new Vector4(solver.calcTexSize.x, solver.calcTexSize.y, 0, 0));
        }

        //------------------------------------------------------
        // Use this method to auto fadeout and fadein the effect
        // Example: RaindropFX.RaindropFX_HDRPRenderer.SetEnable(true/false);
        //------------------------------------------------------
        public void SetEnable(bool set) {
            solver.fadeout_fadein_switch = set;
        }

        //---------------------------------------------
        // Use this method to set up the wind
        //---------------------------------------------
        public void SetWind(bool isEnabled, Vector2 windForce, float turbulenceWeight, float turbScale) {
            useWind.value = isEnabled;
            wind.value = windForce;
            windTurbulence.value = turbulenceWeight;
            windTurbScale.value = turbScale;
        }

        //---------------------------------
        // Initialize raindrop solver
        //---------------------------------
        private void InitSys() {
            solver.Init(
                _raindropTex_alpha.value, 
                new Vector2Int(800, 450)
            );
        }

        //---------------------------------
        // Parameter security detection
        //---------------------------------
        private void Constraints() {
            if (solver == null) solver = new RaindropGenerator();
            if (solver.calcRainTex == null) initFlag = true;
            if (initFlag) { InitSys(); initFlag = false; }
            if (wiper == null) wiper = GameObject.FindObjectOfType(typeof(Wiper)) as Wiper;
            if (temp == null) temp = new RenderTexture(solver.calcTexSize.x, solver.calcTexSize.y, 16);
            if (temp2 == null) temp2 = new RenderTexture(solver.calcTexSize.x, solver.calcTexSize.y, 16);
            if (tempWip == null) tempWip = new RenderTexture(solver.calcTexSize.x, solver.calcTexSize.y, 16);
            if (wipDelta == null) wipDelta = new RenderTexture(solver.calcTexSize.x, solver.calcTexSize.y, 16);
            
            if (maxDynamicRaindropNumber.value < 0) maxDynamicRaindropNumber.value = 0;
            if (calcTimeStep.value < 0.1f) calcTimeStep.value = 0.1f;
            if (raindropSizeRange.value.x <= 0.01f)
                raindropSizeRange.value = new Vector2(0.01f, raindropSizeRange.value.y);
            if (raindropSizeRange.value.y < raindropSizeRange.value.x)
                raindropSizeRange.value = new Vector2(raindropSizeRange.value.x, raindropSizeRange.value.x);
            // Raindrop_HDRP.sFriction = friction;
        }
    }

}