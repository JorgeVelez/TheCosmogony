using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;
using RaindropFX;

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(RaindropFX_HDRP))]
sealed class RaindropFX_HDRPEditor : VolumeComponentEditor {

    SerializedDataParameter fadeout_fadein_switch;
    SerializedDataParameter fastMode;
    SerializedDataParameter fadeSpeed;
    SerializedDataParameter refreshRate;
    SerializedDataParameter _raindropTex_alpha;
    SerializedDataParameter forceRainTextureSize;
    SerializedDataParameter calcRainTextureSize;
    SerializedDataParameter downSampling;

    SerializedDataParameter _rainMask_grayscale;
    SerializedDataParameter pixelization;
    SerializedDataParameter pixResolution;

    SerializedDataParameter mousePainting;
    SerializedDataParameter wipeEffect;
    SerializedDataParameter foggingSpeed;

    SerializedDataParameter calcTimeStep;
    SerializedDataParameter useWind;
    SerializedDataParameter radialWind;
    SerializedDataParameter windTurbulence;
    SerializedDataParameter windTurbScale;
    SerializedDataParameter wind;
    SerializedDataParameter gravity;
    SerializedDataParameter friction;

    SerializedDataParameter generateTrail;
    SerializedDataParameter maxStaticRaindropNumber;
    SerializedDataParameter maxDynamicRaindropNumber;
    SerializedDataParameter raindropSizeRange;

    SerializedDataParameter tintColor;
    SerializedDataParameter tintAmount;
    SerializedDataParameter fusion;
    SerializedDataParameter distortion;
    SerializedDataParameter _inBlack;
    SerializedDataParameter _inWhite;
    SerializedDataParameter _outWhite;
    SerializedDataParameter _outBlack;

    SerializedDataParameter useFog;
    SerializedDataParameter fogTint;
    SerializedDataParameter fogIntensity;
    SerializedDataParameter fogIteration;

    SerializedDataParameter dropletBlur;
    SerializedDataParameter invertBlur;
    SerializedDataParameter edgeSoftness;
    SerializedDataParameter _focalize;
    SerializedDataParameter blurIteration;
    
    public override bool hasAdvancedMode => false;

    public override void OnEnable() {
        base.OnEnable();
        var o = new PropertyFetcher<RaindropFX_HDRP>(serializedObject);

        fadeout_fadein_switch = Unpack(o.Find(x => x.fadeout_fadein_switch));
        fastMode = Unpack(o.Find(x => x.fastMode));
        fadeSpeed = Unpack(o.Find(x => x.fadeSpeed));
        refreshRate = Unpack(o.Find(x => x.refreshRate));
        _raindropTex_alpha = Unpack(o.Find(x => x._raindropTex_alpha));
        forceRainTextureSize = Unpack(o.Find(x => x.forceRainTextureSize));
        calcRainTextureSize = Unpack(o.Find(x => x.calcRainTextureSize));
        downSampling = Unpack(o.Find(x => x.downSampling));

        _rainMask_grayscale = Unpack(o.Find(x => x._rainMask_grayscale));
        pixelization = Unpack(o.Find(x => x.pixelization));
        pixResolution = Unpack(o.Find(x => x.pixResolution));

        mousePainting = Unpack(o.Find(x => x.mousePainting));
        wipeEffect = Unpack(o.Find(x => x.wipeEffect));
        foggingSpeed = Unpack(o.Find(x => x.foggingSpeed));

        calcTimeStep = Unpack(o.Find(x => x.calcTimeStep));
        useWind = Unpack(o.Find(x => x.useWind));
        radialWind = Unpack(o.Find(x => x.radialWind));
        windTurbulence = Unpack(o.Find(x => x.windTurbulence));
        windTurbScale = Unpack(o.Find(x => x.windTurbScale));
        wind = Unpack(o.Find(x => x.wind));
        gravity = Unpack(o.Find(x => x.gravity));
        friction = Unpack(o.Find(x => x.friction));

        generateTrail = Unpack(o.Find(x => x.generateTrail));
        maxStaticRaindropNumber = Unpack(o.Find(x => x.maxStaticRaindropNumber));
        maxDynamicRaindropNumber = Unpack(o.Find(x => x.maxDynamicRaindropNumber));
        raindropSizeRange = Unpack(o.Find(x => x.raindropSizeRange));

        tintColor = Unpack(o.Find(x => x.tintColor));
        tintAmount = Unpack(o.Find(x => x.tintAmount));
        fusion = Unpack(o.Find(x => x.fusion));
        distortion = Unpack(o.Find(x => x.distortion));
        _inBlack = Unpack(o.Find(x => x._inBlack));
        _inWhite = Unpack(o.Find(x => x._inWhite));
        _outWhite = Unpack(o.Find(x => x._outWhite));
        _outBlack = Unpack(o.Find(x => x._outBlack));

        useFog = Unpack(o.Find(x => x.useFog));
        fogTint = Unpack(o.Find(x => x.fogTint));
        fogIntensity = Unpack(o.Find(x => x.fogIntensity));
        fogIteration = Unpack(o.Find(x => x.fogIteration));

        dropletBlur = Unpack(o.Find(x => x.dropletBlur));
        invertBlur = Unpack(o.Find(x => x.invertBlur));
        edgeSoftness = Unpack(o.Find(x => x.edgeSoftness));
        _focalize = Unpack(o.Find(x => x._focalize));
        blurIteration = Unpack(o.Find(x => x.blurIteration));
    }

    public override void OnInspectorGUI() {
        PropertyField(fadeout_fadein_switch);
        PropertyField(fastMode);
        PropertyField(fadeSpeed);
        PropertyField(refreshRate);
        PropertyField(_raindropTex_alpha);
        PropertyField(forceRainTextureSize);
        PropertyField(calcRainTextureSize);
        PropertyField(downSampling);

        PropertyField(generateTrail);
        PropertyField(maxStaticRaindropNumber);
        PropertyField(maxDynamicRaindropNumber);
        PropertyField(raindropSizeRange);

        PropertyField(calcTimeStep);
        PropertyField(useWind);
        PropertyField(radialWind);
        PropertyField(windTurbulence);
        PropertyField(windTurbScale);
        PropertyField(wind);
        PropertyField(gravity);
        PropertyField(friction);

        PropertyField(_rainMask_grayscale);
        PropertyField(pixelization);
        PropertyField(pixResolution);

        PropertyField(mousePainting);
        PropertyField(wipeEffect);
        PropertyField(foggingSpeed);

        PropertyField(tintColor);
        PropertyField(tintAmount);
        PropertyField(fusion);
        PropertyField(distortion);
        PropertyField(_inBlack);
        PropertyField(_inWhite);
        PropertyField(_outWhite);
        PropertyField(_outBlack);

        PropertyField(useFog);
        PropertyField(fogTint);
        PropertyField(fogIntensity);
        PropertyField(fogIteration);

        PropertyField(dropletBlur);
        PropertyField(invertBlur);
        PropertyField(edgeSoftness);
        PropertyField(_focalize);
        PropertyField(blurIteration);
    }

}
#endif