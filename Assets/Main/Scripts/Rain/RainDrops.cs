using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [Serializable, VolumeComponentMenu("Post-processing/Rainy")]
    public class RainDrops : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        Material m_Material;
        public bool IsActive() => m_Material != null;
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
        public override void Setup()
        {
            if (Shader.Find("Post-processing/Custom/RainDrops") != null)
                m_Material = new Material(Shader.Find("Post-processing/Custom/RainDrops"));
        }

        [Tooltip("Rain Amount")]
        public ClampedFloatParameter rainAmount = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);

        [Tooltip("Zoom")]
        public ClampedFloatParameter zoom = new ClampedFloatParameter(1.0f, 0.0f, 3.14f);

        [Tooltip("Speed")]
        public ClampedFloatParameter speed = new ClampedFloatParameter(0.25f, 0.0f, 3.14f);

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (m_Material == null)
                return;
            m_Material.SetFloat("_RainAmount", rainAmount.value);
            m_Material.SetFloat("_Zoom", zoom.value);
            m_Material.SetFloat("_Speed", speed.value);
            m_Material.SetTexture("_InputTexture", source);
            HDUtils.DrawFullScreen(cmd, m_Material, destination);

        }
        public override void Cleanup() => CoreUtils.Destroy(m_Material);
    }
}
