//Cristian Pop - https://boxophobic.com/

using UnityEditor;
using UnityEngine;

namespace TheVegetationEngine
{
    public class TVEShaderUtils
    {
        public static void SetMaterialSettings(Material material)
        {
            var shaderName = material.shader.name;

            if (!material.HasProperty("_IsTVEShader"))
            {
                return;
            }

            // Set Internal Render Values
            if (material.HasProperty("_RenderMode"))
            {
                material.SetInt("_render_mode", material.GetInt("_RenderMode"));
            }

            if (material.HasProperty("_RenderCull"))
            {
                material.SetInt("_render_cull", material.GetInt("_RenderCull"));
            }

            if (material.HasProperty("_RenderNormals"))
            {
                material.SetInt("_render_normals", material.GetInt("_RenderNormals"));
            }

            if (material.HasProperty("_RenderZWrite"))
            {
                material.SetInt("_render_zw", material.GetInt("_RenderZWrite"));
            }

            if (material.HasProperty("_RenderClip"))
            {
                material.SetInt("_render_clip", material.GetInt("_RenderClip"));
            }

            // Set Render Mode
            if (material.HasProperty("_RenderMode") /* && material.HasProperty("_RenderBlend") */)
            {
                int mode = material.GetInt("_RenderMode");
                //int blend = material.GetInt("_RenderBlend");
                int zwrite = material.GetInt("_RenderZWrite");

                // Opaque
                if (mode == 0)
                {
                    material.SetOverrideTag("RenderType", "AlphaTest");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

                    // Standard and Universal Render Pipeline
                    material.SetInt("_render_src", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_render_dst", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_render_zw", 1);
                    material.SetInt("_render_premul", 0);

                    // Set Main Color alpha to 1
                    if (material.HasProperty("_MainColor"))
                    {
                        var mainColor = material.GetColor("_MainColor");
                        material.SetColor("_MainColor", new Color(mainColor.r, mainColor.g, mainColor.b, 1.0f));
                    }

                    // HD Render Pipeline
                    material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.DisableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                    material.DisableKeyword("_BLENDMODE_ALPHA");
                    material.DisableKeyword("_BLENDMODE_ADD");
                    material.DisableKeyword("_BLENDMODE_PRE_MULTIPLY");

                    material.SetInt("_RenderQueueType", 1);
                    material.SetInt("_SurfaceType", 0);
                    material.SetInt("_BlendMode", 0);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_AlphaSrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_AlphaDstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_TransparentZWrite", 1);
                    material.SetInt("_ZTestDepthEqualForOpaque", 3);
                    material.SetInt("_ZTestGBuffer", 4);
                    material.SetInt("_ZTestTransparent", 4);
                }
                // Transparent
                else
                {
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                    // Alpha Blend
                    //if (blend == 0)
                    {
                        // Standard and Universal Render Pipeline
                        material.SetInt("_render_src", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_render_dst", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_render_premul", 0);

                        // HD Render Pipeline
                        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                        material.EnableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                        material.EnableKeyword("_BLENDMODE_ALPHA");
                        material.DisableKeyword("_BLENDMODE_ADD");
                        material.DisableKeyword("_BLENDMODE_PRE_MULTIPLY");

                        material.SetInt("_RenderQueueType", 5);
                        material.SetInt("_SurfaceType", 1);
                        material.SetInt("_BlendMode", 0);
                        material.SetInt("_SrcBlend", 1);
                        material.SetInt("_DstBlend", 10);
                        material.SetInt("_AlphaSrcBlend", 1);
                        material.SetInt("_AlphaDstBlend", 10);
                        material.SetInt("_ZWrite", zwrite);
                        material.SetInt("_TransparentZWrite", zwrite);
                        material.SetInt("_ZTestDepthEqualForOpaque", 4);
                        material.SetInt("_ZTestGBuffer", 4);
                        material.SetInt("_ZTestTransparent", 4);
                    }
                    // Premultiply Blend
                    //else if (blend == 1)
                    //{
                    //    // Standard and Universal Render Pipeline
                    //    material.SetInt("_render_src", (int)UnityEngine.Rendering.BlendMode.One);
                    //    material.SetInt("_render_dst", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    //    material.SetInt("_render_premul", 1);

                    //    // HD Render Pipeline
                    //    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    //    material.EnableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                    //    material.DisableKeyword("_BLENDMODE_ALPHA");
                    //    material.DisableKeyword("_BLENDMODE_ADD");
                    //    material.EnableKeyword("_BLENDMODE_PRE_MULTIPLY");

                    //    material.SetInt("_RenderQueueType", 5);
                    //    material.SetInt("_SurfaceType", 1);
                    //    material.SetInt("_BlendMode", 4);
                    //    material.SetInt("_SrcBlend", 1);
                    //    material.SetInt("_DstBlend", 10);
                    //    material.SetInt("_AlphaSrcBlend", 1);
                    //    material.SetInt("_AlphaDstBlend", 10);
                    //    material.SetInt("_ZWrite", zwrite);
                    //    material.SetInt("_TransparentZWrite", zwrite);
                    //    material.SetInt("_ZTestDepthEqualForOpaque", 4);
                    //    material.SetInt("_ZTestGBuffer", 4);
                    //    material.SetInt("_ZTestTransparent", 4);
                    //}
                }
            }

            // Set Receive Mode in HDRP
            if (material.GetTag("RenderPipeline", false) == "HDRenderPipeline")
            {
                if (material.HasProperty("_RenderDecals"))
                {
                    int decals = material.GetInt("_RenderDecals");

                    if (decals == 0)
                    {
                        material.EnableKeyword("_DISABLE_DECALS");
                    }
                    else
                    {
                        material.DisableKeyword("_DISABLE_DECALS");
                    }
                }

                if (material.HasProperty("_RenderSSR"))
                {
                    int ssr = material.GetInt("_RenderSSR");

                    if (ssr == 0)
                    {
                        material.EnableKeyword("_DISABLE_SSR");

                        material.SetInt("_StencilRef", 0);
                        material.SetInt("_StencilRefDepth", 0);
                        material.SetInt("_StencilRefDistortionVec", 4);
                        material.SetInt("_StencilRefGBuffer", 2);
                        material.SetInt("_StencilRefMV", 32);
                        material.SetInt("_StencilWriteMask", 6);
                        material.SetInt("_StencilWriteMaskDepth", 8);
                        material.SetInt("_StencilWriteMaskDistortionVec", 4);
                        material.SetInt("_StencilWriteMaskGBuffer", 14);
                        material.SetInt("_StencilWriteMaskMV", 40);
                    }
                    else
                    {
                        material.DisableKeyword("_DISABLE_SSR");

                        material.SetInt("_StencilRef", 0);
                        material.SetInt("_StencilRefDepth", 8);
                        material.SetInt("_StencilRefDistortionVec", 4);
                        material.SetInt("_StencilRefGBuffer", 10);
                        material.SetInt("_StencilRefMV", 40);
                        material.SetInt("_StencilWriteMask", 6);
                        material.SetInt("_StencilWriteMaskDepth", 8);
                        material.SetInt("_StencilWriteMaskDistortionVec", 4);
                        material.SetInt("_StencilWriteMaskGBuffer", 14);
                        material.SetInt("_StencilWriteMaskMV", 40);
                    }
                }
            }

            if (material.HasProperty("_RenderCull"))
            {
                int cull = material.GetInt("_RenderCull");

                material.SetInt("_CullMode", cull);
                material.SetInt("_TransparentCullMode", cull);
                material.SetInt("_CullModeForward", cull);

                // Needed for HD Render Pipeline
                material.DisableKeyword("_DOUBLESIDED_ON");
            }

            // Set Cull Mode
            if (material.HasProperty("_RenderCull"))
            {
                int cull = material.GetInt("_RenderCull");

                material.SetInt("_CullMode", cull);
                material.SetInt("_TransparentCullMode", cull);
                material.SetInt("_CullModeForward", cull);

                // Needed for HD Render Pipeline
                material.DisableKeyword("_DOUBLESIDED_ON");
            }

            // Set Clip Mode
            if (material.HasProperty("_RenderClip"))
            {
                int clip = material.GetInt("_RenderClip");
                float cutoff = material.GetFloat("_Cutoff");

                if (clip == 0)
                {
                    material.DisableKeyword("TVE_ALPHA_CLIP");

                    // HD Render Pipeline
                    material.SetInt("_AlphaCutoffEnable", 0);
                }
                else
                {
                    material.EnableKeyword("TVE_ALPHA_CLIP");

                    // HD Render Pipeline
                    material.SetInt("_AlphaCutoffEnable", 1);
                }

                material.SetFloat("_render_cutoff", cutoff);

                // HD Render Pipeline
                material.SetFloat("_AlphaCutoff", cutoff);
                material.SetFloat("_AlphaCutoffPostpass", cutoff);
                material.SetFloat("_AlphaCutoffPrepass", cutoff);
                material.SetFloat("_AlphaCutoffShadow", cutoff);
            }

            // Set Normals Mode
            if (material.HasProperty("_RenderNormals"))
            {
                int normals = material.GetInt("_RenderNormals");

                // Standard, Universal, HD Render Pipeline
                // Flip 0
                if (normals == 0)
                {
                    material.SetVector("_render_normals_options", new Vector4(-1, -1, -1, 0));
                    material.SetVector("_DoubleSidedConstants", new Vector4(-1, -1, -1, 0));
                }
                // Mirror 1
                else if (normals == 1)
                {
                    material.SetVector("_render_normals_options", new Vector4(1, 1, -1, 0));
                    material.SetVector("_DoubleSidedConstants", new Vector4(1, 1, -1, 0));
                }
                // None 2
                else if (normals == 2)
                {
                    material.SetVector("_render_normals_options", new Vector4(1, 1, 1, 0));
                    material.SetVector("_DoubleSidedConstants", new Vector4(1, 1, 1, 0));
                }
            }

            // Set Normals to 0 if no texture is used
            if (material.HasProperty("_MainNormalTex"))
            {
                if (material.GetTexture("_MainNormalTex") == null)
                {
                    material.SetFloat("_MainNormalValue", 0);
                }
            }

            // Set Normals to 0 if no texture is used
            if (material.HasProperty("_SecondNormalTex"))
            {
                if (material.GetTexture("_SecondNormalTex") == null)
                {
                    material.SetFloat("_SecondNormalValue", 0);
                }
            }

            // Assign Default HD Foliage profile
            if (material.HasProperty("_SubsurfaceDiffusion"))
            {
                if (material.GetFloat("_SubsurfaceDiffusion") == 0)
                {
                    material.SetFloat("_SubsurfaceDiffusion", 3.5648174285888672f);
                    material.SetVector("_SubsurfaceDiffusion_asset", new Vector4(228889264007084710000000000000000000000f, 0.000000000000000000000000012389357880079404f, 0.00000000000000000000000000000000000076932702684439582f, 0.00018220426863990724f));
                    material.SetVector("_SubsurfaceDiffusion_Asset", new Vector4(228889264007084710000000000000000000000f, 0.000000000000000000000000012389357880079404f, 0.00000000000000000000000000000000000076932702684439582f, 0.00018220426863990724f));
                }
            }

            // Set Detail Mode
            if (material.HasProperty("_DetailMode") && material.HasProperty("_SecondColor"))
            {
                if (material.GetInt("_DetailMode") == 0)
                {
                    material.EnableKeyword("TVE_DETAIL_MODE_OFF");
                    material.DisableKeyword("TVE_DETAIL_MODE_ON");
                }
                else
                {
                    material.DisableKeyword("TVE_DETAIL_MODE_OFF");
                    material.EnableKeyword("TVE_DETAIL_MODE_ON");
                }
            }

            if (material.HasProperty("_DetailBlendMode") && material.HasProperty("_SecondColor"))
            {
                if (material.GetInt("_DetailBlendMode") == 0)
                {
                    material.EnableKeyword("TVE_DETAIL_BLEND_OVERLAY");
                    material.DisableKeyword("TVE_DETAIL_BLEND_REPLACE");
                }
                else
                {
                    material.DisableKeyword("TVE_DETAIL_BLEND_OVERLAY");
                    material.EnableKeyword("TVE_DETAIL_BLEND_REPLACE");
                }
            }

            // Set Detail Type
            if (material.HasProperty("_DetailTypeMode") && material.HasProperty("_SecondColor"))
            {
                if (material.GetInt("_DetailTypeMode") == 0)
                {
                    material.EnableKeyword("TVE_DETAIL_TYPE_VERTEX_BLUE");
                    material.DisableKeyword("TVE_DETAIL_TYPE_PROJECTION");
                }
                else
                {
                    material.DisableKeyword("TVE_DETAIL_TYPE_VERTEX_BLUE");
                    material.EnableKeyword("TVE_DETAIL_TYPE_PROJECTION");
                }
            }

            // Set Batching Mode
            if (material.HasProperty("_VertexDataMode"))
            {
                int batching = material.GetInt("_VertexDataMode");

                if (batching == 0)
                {
                    material.DisableKeyword("TVE_VERTEX_DATA_BATCHED");
                }
                else
                {
                    material.EnableKeyword("TVE_VERTEX_DATA_BATCHED");
                }
            }

            //Set Pivots Mode
            if (material.HasProperty("_VertexPivotMode"))
            {
                material.SetInt("_vertex_pivot_mode", material.GetInt("_VertexPivotMode"));
            }

            // Enable Nature Rendered support
            material.SetOverrideTag("NatureRendererInstancing", "True");

            // Set Legacy props for external bakers
            if (material.HasProperty("_MainColor"))
            {
                material.SetColor("_Color", material.GetColor("_MainColor"));
            }

            // Set BlinnPhong Spec Color
            if (material.HasProperty("_SpecColor"))
            {
                material.SetColor("_SpecColor", Color.white);
            }

            if (material.HasProperty("_MainAlbedoTex"))
            {
                material.SetTexture("_MainTex", material.GetTexture("_MainAlbedoTex"));
                material.SetTextureScale("_MainTex", new Vector2(material.GetVector("_MainUVs").x, material.GetVector("_MainUVs").y));
                material.SetTextureOffset("_MainTex", new Vector2(material.GetVector("_MainUVs").z, material.GetVector("_MainUVs").w));
            }

            if (material.HasProperty("_MainNormalTex"))
            {
                material.SetTexture("_BumpMap", material.GetTexture("_MainNormalTex"));
                material.SetTextureScale("_BumpMap", new Vector2(material.GetVector("_MainUVs").x, material.GetVector("_MainUVs").y));
                material.SetTextureOffset("_BumpMap", new Vector2(material.GetVector("_MainUVs").z, material.GetVector("_MainUVs").w));
            }

            // Set Internal properties
            if (shaderName.Contains("Simple Lit"))
            {
                material.SetInt("_IsSimpleShader", 1);
                material.SetInt("_IsStandardShader", 0);
                material.SetInt("_IsSubsurfaceShader", 0);
            }
            else
            {
                material.SetInt("_IsSimpleShader", 0);
                material.SetInt("_IsStandardShader", 1);
                material.SetInt("_IsSubsurfaceShader", 1);
            }

            // Set Catergories for impostor baking 
            if (material.HasProperty("_VertexOcclusionColor"))
            {
                material.SetInt("_OcclusionCat", 1);
            }
            else
            {
                material.SetInt("_OcclusionCat", 0);
            }

            if (material.HasProperty("_SubsurfaceValue"))
            {
                material.SetInt("_SubsurfaceCat", 1);
            }
            else
            {
                material.SetInt("_SubsurfaceCat", 0);
            }

            if (material.HasProperty("_GradientColorOne"))
            {
                material.SetInt("_GradientCat", 1);
            }
            else
            {
                material.SetInt("_GradientCat", 0);
            }

            if (material.HasProperty("_NoiseColorOne"))
            {
                material.SetInt("_NoiseCat", 1);
            }
            else
            {
                material.SetInt("_NoiseCat", 0);
            }

            if (material.HasProperty("_PerspectivePushValue"))
            {
                material.SetInt("_PerspectiveCat", 1);
            }
            else
            {
                material.SetInt("_PerspectiveCat", 0);
            }

            if (material.HasProperty("_EmissiveColor"))
            {
                material.SetInt("_EmissiveCat", 1);
            }
            else
            {
                material.SetInt("_EmissiveCat", 0);
            }
        }

        public static void SetElementSettings(Material material)
        {
            if (material.HasProperty("_ElementLayerValue"))
            {
                var layer = material.GetInt("_ElementLayerValue");

                if (layer == 0)
                {
                    material.SetInt("_ElementLayerMessage", 0);
                }
                else
                {
                    material.SetInt("_ElementLayerMessage", 1);
                }
            }

            if (material.HasProperty("_ElementEffect"))
            {
                var effect = material.GetInt("_ElementEffect");

                if (effect == 0)
                {
                    material.SetInt("_render_colormask", 14);
                }
                else
                {
                    material.SetInt("_render_colormask", 15);
                }
            }
        }

        //public static void UpgradeMaterialTo200(Material material)
        //{
        //    //if (material.shader.name.Contains("Bark Simple Lit"))
        //    //{
        //    //    material.shader = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Vegetation/Bark Standard Lit");
        //    //}

        //    //if (material.shader.name.Contains("Cross Simple Lit"))
        //    //{
        //    //    material.shader = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Vegetation/Cross Standard Lit");
        //    //}

        //    //if (material.shader.name.Contains("Grass Simple Lit"))
        //    //{
        //    //    material.shader = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Vegetation/Grass Standard Lit");
        //    //}

        //    //if (material.shader.name.Contains("Leaf Simple Lit"))
        //    //{
        //    //    material.shader = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Vegetation/Leaf Standard Lit");
        //    //}

        //    //if (material.shader.name.Contains("Prop Simple Lit") && material.shader.name.Contains("projection") == false)
        //    //{
        //    //    material.shader = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Objects/Prop Standard Lit");
        //    //}

        //    //if (material.shader.name.Contains("Prop Simple Lit") && material.shader.name.Contains("projection") == true)
        //    //{
        //    //    material.shader = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Objects/Prop Standard Lit");
        //    //    material.SetInt("_DetailTypeMode", 1);
        //    //}

        //    //if (material.shader.name.Contains("Prop Standard Lit") && material.shader.name.Contains("projection") == true)
        //    //{
        //    //    material.shader = Shader.Find("BOXOPHOBIC/The Vegetation Engine/Objects/Prop Standard Lit");
        //    //    material.SetInt("_DetailTypeMode", 1);
        //    //}

        //    if (material.HasProperty("_IsVersion"))
        //    {
        //        if (material.GetInt("_IsVersion") < 200)
        //        {
        //            if (material.HasProperty("_render_mode"))
        //            {
        //                material.SetInt("_RenderMode", material.GetInt("_render_mode"));
        //            }

        //            if (material.HasProperty("_render_cull"))
        //            {
        //                material.SetInt("_RenderCull", material.GetInt("_render_cull"));
        //            }

        //            if (material.HasProperty("_render_normals"))
        //            {
        //                material.SetInt("_RenderNormals", material.GetInt("_render_normals"));
        //            }

        //            if (material.HasProperty("_render_blend"))
        //            {
        //                material.SetInt("_RenderBlend", material.GetInt("_render_blend"));
        //            }

        //            if (material.HasProperty("_render_zw"))
        //            {
        //                material.SetInt("_RenderZWrite", material.GetInt("_render_zw"));
        //            }

        //            if (material.HasProperty("_render_clip"))
        //            {
        //                material.SetInt("_RenderClip", material.GetInt("_render_clip"));
        //            }

        //            if (material.HasProperty("_render_priority"))
        //            {
        //                material.SetInt("_RenderPriority", material.GetInt("_render_priority"));
        //            }

        //            if (material.HasProperty("_material_batching"))
        //            {
        //                material.SetInt("_VertexDataMode", material.GetInt("_material_batching"));
        //            }

        //            if (material.HasProperty("_ObjectMetallicValue"))
        //            {
        //                material.SetFloat("_MainMetallicValue", material.GetFloat("_ObjectMetallicValue"));
        //            }

        //            if (material.HasProperty("_ObjectSmoothnessValue"))
        //            {
        //                material.SetFloat("_MainSmoothnessValue", material.GetFloat("_ObjectSmoothnessValue"));
        //            }

        //            if (material.HasProperty("_ObjectOcclusionValue"))
        //            {
        //                material.SetFloat("_VertexOcclusionValue", material.GetFloat("_ObjectOcclusionValue") * 0.5f);
        //            }

        //            if (material.HasProperty("_OverlayContrast"))
        //            {
        //                material.SetFloat("_OverlayContrastValue", material.GetFloat("_OverlayContrast"));
        //            }

        //            if (material.HasProperty("_OverlayVariation"))
        //            {
        //                material.SetFloat("_OverlayVariationValue", material.GetFloat("_OverlayVariation"));
        //            }

        //            if (material.HasProperty("_MaskMode"))
        //            {
        //                material.SetFloat("_DetailMaskMode", material.GetFloat("_MaskMode"));
        //            }

        //            if (material.HasProperty("_MotionAmplitude_30"))
        //            {
        //                material.SetFloat("_MotionAmplitude_30", material.GetFloat("_MotionAmplitude_30") * 2.0f);
        //            }

        //            if (material.HasProperty("_MotionAmplitude_32"))
        //            {
        //                material.SetFloat("_MotionAmplitude_32", material.GetFloat("_MotionAmplitude_32") * 2.0f);
        //            }

        //            if (material.HasProperty("_SubsurfaceMinValue"))
        //            {
        //                material.SetFloat("_MainMaskMinValue", material.GetFloat("_SubsurfaceMinValue"));
        //            }

        //            if (material.HasProperty("_SubsurfaceMaxValue"))
        //            {
        //                material.SetFloat("_MainMaskMaxValue", material.GetFloat("_SubsurfaceMaxValue"));
        //            }

        //            if (material.HasProperty("_GrassPerspectivePushValue"))
        //            {
        //                material.SetFloat("_PerspectivePushValue", material.GetFloat("_GrassPerspectivePushValue"));
        //            }

        //            if (material.HasProperty("_GrassPerspectiveNoiseValue"))
        //            {
        //                material.SetFloat("_PerspectiveNoiseValue", material.GetFloat("_GrassPerspectiveNoiseValue"));
        //            }

        //            if (material.HasProperty("_GrassPerspectiveAngleValue"))
        //            {
        //                material.SetFloat("_PerspectiveAngleValue", material.GetFloat("_GrassPerspectiveAngleValue"));
        //            }

        //            material.SetInt("_IsVersion", 200);
        //        }
        //    }
        //}

        //public static void UpgradeMaterialTo210(Material material)
        //{
        //    if (material.HasProperty("_IsVersion"))
        //    {
        //        if (material.GetInt("_IsVersion") < 210)
        //        {
        //            material.DisableKeyword("_ALPHATEST_ON");

        //            if (material.HasProperty("_NoiseTintOne"))
        //            {
        //                material.SetColor("_NoiseColorOne", material.GetColor("_NoiseTintOne"));
        //            }

        //            if (material.HasProperty("_NoiseTintTwo"))
        //            {
        //                material.SetColor("_NoiseColorTwo", material.GetColor("_NoiseTintTwo"));
        //            }

        //            if (material.HasProperty("_MainMaskMinValue"))
        //            {
        //                material.SetFloat("_SubsurfaceMaskMinValue", material.GetFloat("_MainMaskMinValue"));
        //            }

        //            if (material.HasProperty("_MainMaskMaxValue"))
        //            {
        //                material.SetFloat("_SubsurfaceMaskMaxValue", material.GetFloat("_MainMaskMaxValue"));
        //            }

        //            if (material.HasProperty("_SubsurfaceMaskValue"))
        //            {
        //                if (material.GetFloat("_SubsurfaceMaskValue") < 0.2f)
        //                {
        //                    material.SetFloat("_SubsurfaceMaskMinValue", 0);
        //                    material.SetFloat("_SubsurfaceMaskMaxValue", 0);
        //                }
        //            }

        //            if (material.HasProperty("_ColorMaskValue"))
        //            {
        //                if (material.GetFloat("_ColorMaskValue") < 0.2f)
        //                {
        //                    material.SetFloat("_ColorsMaskMinValue", 0);
        //                    material.SetFloat("_ColorsMaskMaxValue", 0);
        //                }
        //            }

        //            if (material.HasProperty("_DetailMaskContrast"))
        //            {
        //                material.SetFloat("_DetailBlendMinValue", material.GetFloat("_DetailMaskContrast"));
        //                material.SetFloat("_DetailBlendMaxValue", 1.0f - material.GetFloat("_DetailMaskContrast"));
        //            }

        //            if (material.HasProperty("_VertexOcclusionValue"))
        //            {
        //                if (material.GetFloat("_VertexOcclusionValue") < 0.1f)
        //                {
        //                    material.SetColor("_VertexOcclusionColor", Color.white);
        //                }
        //            }

        //            if (material.HasProperty("_GlobalSizeFade"))
        //            {
        //                if (material.GetFloat("_GlobalSizeFade") < 0.5f)
        //                {
        //                    material.SetFloat("_SizeFadeMode", 0.0f);
        //                }
        //                else
        //                {
        //                    material.SetFloat("_SizeFadeMode", 1.0f);
        //                }
        //            }

        //            material.SetInt("_IsVersion", 210);
        //        }
        //    }
        //}

        //public static void UpgradeMaterialTo220(Material material)
        //{
        //    if (material.GetFloat("_IsVersion") < 220)
        //    {
        //        if (material.HasProperty("_IsPropShader"))
        //        {
        //            material.SetInt("_IsPropShader", 1);
        //        }

        //        if (material.HasProperty("_IsBarkShader"))
        //        {
        //            material.SetInt("_IsBarkShader", 1);
        //        }

        //        if (material.HasProperty("_IsLeafShader"))
        //        {
        //            material.SetInt("_IsLeafShader", 1);
        //        }

        //        if (material.HasProperty("_IsCrossShader"))
        //        {
        //            material.SetInt("_IsCrossShader", 1);
        //        }

        //        if (material.HasProperty("_IsGrassShader"))
        //        {
        //            material.SetInt("_IsGrassShader", 1);
        //        }

        //        if (material.HasProperty("_IsStandardShader"))
        //        {
        //            material.SetInt("_IsStandardShader", 1);
        //        }

        //        if (material.HasProperty("_IsLitShader"))
        //        {
        //            material.SetInt("_IsLitShader", 1);
        //        }

        //        if (material.HasProperty("_IsAnyPathShader"))
        //        {
        //            material.SetInt("_IsAnyPathShader", 1);
        //        }

        //        if (material.HasProperty("_IsForwardPathShader"))
        //        {
        //            material.SetInt("_IsForwardPathShader", 1);
        //        }

        //        material.SetInt("_IsVersion", 220);
        //    }

        //}

        //public static void UpgradeMaterialTo230(Material material)
        //{
        //    if (material.HasProperty("_IsVersion"))
        //    {
        //        if (material.GetInt("_IsVersion") < 230)
        //        {
        //            if (material.HasProperty("_GlobalLeaves"))
        //            {
        //                material.SetFloat("_GlobalAlpha", material.GetFloat("_GlobalLeaves"));
        //            }

        //            if (material.HasProperty("_LocalLeaves"))
        //            {
        //                material.SetFloat("_LocalAlpha", material.GetFloat("_LocalLeaves"));
        //            }

        //            if (material.HasProperty("_LeavesVariationValue"))
        //            {
        //                material.SetFloat("_AlphaVariationValue", material.GetFloat("_LeavesVariationValue"));
        //            }

        //            material.SetInt("_IsVersion", 230);
        //        }
        //    }
        //}

        public static void UpgradeMaterialTo300(Material material)
        {
            if (material.HasProperty("_IsVersion"))
            {
                if (material.GetInt("_IsVersion") < 300)
                {
                    //if (material.HasProperty("_BillboardFadeHValue"))
                    //{
                    //    material.SetFloat("_FadeHorizontalValue", Mathf.Clamp01(material.GetFloat("_BillboardFadeHValue")));
                    //}

                    //if (material.HasProperty("_BillboardFadeVValue"))
                    //{
                    //    material.SetFloat("_FadeVerticalValue", Mathf.Clamp01(material.GetFloat("_BillboardFadeVValue")));
                    //}

                    if (material.HasProperty("_Cutoff"))
                    {
                        if (material.GetFloat("_Cutoff") > 0.9f)
                        {
                            material.SetFloat("_Cutoff", 0.5f);
                        }
                    }

                    material.SetInt("_IsVersion", 300);
                }
            }
        }

        public static void UpgradeMaterialTo320(Material material)
        {
            if (material.HasProperty("_IsVersion"))
            {
                if (material.GetInt("_IsVersion") < 320)
                {
                    material.DisableKeyword("TVE_DETAIL_MODE_OVERLAY");
                    material.DisableKeyword("TVE_DETAIL_MODE_REPLACE");
                    material.DisableKeyword("TVE_DETAIL_MAPS_STANDARD");
                    material.DisableKeyword("TVE_DETAIL_MAPS_PACKED");

                    if (material.HasProperty("_DetailMode"))
                    {
                        if (material.GetInt("_DetailMode") == 1)
                        {
                            material.SetInt("_DetailBlendMode", 0);
                        }

                        if (material.GetInt("_DetailMode") == 2)
                        {
                            material.SetInt("_DetailMode", 1);
                            material.SetInt("_DetailBlendMode", 1);
                        }
                    }

                    material.SetInt("_IsVersion", 320);
                }
            }
        }

        //public static void UpgradeElementTo200(Material material)
        //{
        //    if (material.HasProperty("_IsVersion"))
        //    {
        //        if (material.GetInt("_IsVersion") < 200)
        //        {
        //            if (material.HasProperty("_WinterColor"))
        //            {
        //                material.SetColor("_AdditionalColor1", material.GetColor("_WinterColor"));
        //            }

        //            if (material.HasProperty("_SpringColor"))
        //            {
        //                material.SetColor("_AdditionalColor2", material.GetColor("_SpringColor"));
        //            }

        //            if (material.HasProperty("_SummerColor"))
        //            {
        //                material.SetColor("_AdditionalColor3", material.GetColor("_SummerColor"));
        //            }

        //            if (material.HasProperty("_AutumnColor"))
        //            {
        //                material.SetColor("_AdditionalColor4", material.GetColor("_AutumnColor"));
        //            }

        //            if (material.HasProperty("_WinterValue"))
        //            {
        //                material.SetFloat("_AdditionalValue1", material.GetFloat("_WinterValue"));
        //            }

        //            if (material.HasProperty("_SpringValue"))
        //            {
        //                material.SetFloat("_AdditionalValue2", material.GetFloat("_SpringValue"));
        //            }

        //            if (material.HasProperty("_SummerValue"))
        //            {
        //                material.SetFloat("_AdditionalValue3", material.GetFloat("_SummerValue"));
        //            }

        //            if (material.HasProperty("_AutumnColor"))
        //            {
        //                material.SetFloat("_AdditionalValue4", material.GetFloat("_AutumnValue"));
        //            }

        //            material.SetInt("_IsVersion", 200);
        //        }
        //    }
        //}

        public static void CopyMaterialProperties(Material oldMaterial, Material newMaterial)
        {
            var oldShader = oldMaterial.shader;
            var newShader = newMaterial.shader;

            for (int i = 0; i < ShaderUtil.GetPropertyCount(oldShader); i++)
            {
                for (int j = 0; j < ShaderUtil.GetPropertyCount(newShader); j++)
                {
                    var propertyName = ShaderUtil.GetPropertyName(oldShader, i);
                    var propertyType = ShaderUtil.GetPropertyType(oldShader, i);

                    if (propertyName == ShaderUtil.GetPropertyName(newShader, j))
                    {
                        if (propertyType == ShaderUtil.ShaderPropertyType.Color || propertyType == ShaderUtil.ShaderPropertyType.Vector)
                        {
                            newMaterial.SetVector(propertyName, oldMaterial.GetVector(propertyName));
                        }

                        if (propertyType == ShaderUtil.ShaderPropertyType.Float || propertyType == ShaderUtil.ShaderPropertyType.Range)
                        {
                            if (propertyName != "_IsVersion")
                            {
                                newMaterial.SetFloat(propertyName, oldMaterial.GetFloat(propertyName));
                            }
                        }

                        if (propertyType == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            newMaterial.SetTexture(propertyName, oldMaterial.GetTexture(propertyName));
                        }
                    }
                }
            }
        }

        public static void DrawPivotModeSupport(Material material)
        {
            if (material.HasProperty("_VertexPivotMode"))
            {
                var pivot = material.GetInt("_VertexPivotMode");

                bool toggle = false;

                if (pivot > 0.5f)
                {
                    toggle = true;

                    EditorGUILayout.HelpBox("The Baked Pivots feature allows for using per mesh element interaction and elements influence. This feature requires pre baked pivots on prefab conversion. Useful for large grass mesh wind and interaction.", MessageType.Info);

                    GUILayout.Space(10);
                }

                toggle = EditorGUILayout.Toggle("Enable Pre Baked Pivots", toggle);

                if (toggle)
                {
                    material.SetInt("_VertexPivotMode", 1);
                }
                else
                {
                    material.SetInt("_VertexPivotMode", 0);
                }
            }
        }

        public static void DrawBatchingSupport(Material material)
        {
            if (material.HasProperty("_VertexDataMode"))
            {
                var batching = material.GetInt("_VertexDataMode");

                bool toggle = false;

                if (batching > 0.5f)
                {
                    toggle = true;

                    EditorGUILayout.HelpBox("Use the Batching Support option when the object is statically batched. All vertex calculations are done in world space and features like Baked Pivots and Size options are not supported because the object pivot data is missing with static batching.", MessageType.Info);
                    
                    GUILayout.Space(10);
                }

                toggle = EditorGUILayout.Toggle("Enable Batching Support", toggle);

                if (toggle)
                {
                    material.SetInt("_VertexDataMode", 1);
                }
                else
                {
                    material.SetInt("_VertexDataMode", 0);
                }
            }
        }

        public static void DrawPoweredByTheVegetationEngine()
        {
            var styleLabelCentered = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
            };

            Rect lastRect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(0, lastRect.yMax, 1000, 1), new Color(0, 0, 0, 0.5f));

            GUILayout.Space(15);

            DrawTechincalLabel("Powered by The Vegetation Engine", styleLabelCentered);

            Rect labelRect = GUILayoutUtility.GetLastRect();

            if (GUI.Button(labelRect, "", new GUIStyle()))
            {
                Application.OpenURL("http://u3d.as/1H9u");
            }

            GUILayout.Space(10);
        }

        public static void DrawTechnicalDetails(Material material)
        {
            var shaderName = material.shader.name;

            var styleLabel = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
            };

            if (shaderName.Contains("Standard Lit") || shaderName.Contains("Subsurface Lit") || shaderName.Contains("Translucency Lit"))
            {
                DrawTechincalLabel("Shader Complexity: Balanced", styleLabel);
            }

            if (shaderName.Contains("Simple Lit"))
            {
                DrawTechincalLabel("Shader Complexity: Optimized", styleLabel);
            }

            if (!material.HasProperty("_IsElementShader"))
            {
                if (material.GetTag("RenderPipeline", false) == "HDRenderPipeline")
                {
                    DrawTechincalLabel("Render Pipeline: High Definition Render Pipeline", styleLabel);
                }
                else if (material.GetTag("RenderPipeline", false) == "UniversalPipeline")
                {
                    DrawTechincalLabel("Render Pipeline: Universal Render Pipeline", styleLabel);
                }
                else
                {
                    DrawTechincalLabel("Render Pipeline: Standard Render Pipeline", styleLabel);
                }
            }
            else
            {
                DrawTechincalLabel("Render Pipeline: Any Render Pipeline", styleLabel);
            }

            if (material.GetTag("RenderPipeline", false) == "HDRenderPipeline")
            {
                DrawTechincalLabel("Render Target: Shader Model 4.5 or higher", styleLabel);
            }
            else
            {
                DrawTechincalLabel("Render Target: Shader Model 4.0 or higher", styleLabel);
            }

            if (shaderName.Contains("Standard Lit") || shaderName.Contains("Simple Lit"))
            {
                DrawTechincalLabel("Render Path: Rendered in both Forward and Deferred path", styleLabel);
            }

            if (shaderName.Contains("Subsurface Lit") || shaderName.Contains("Translucency Lit"))
            {
                DrawTechincalLabel("Render Path: Always rendered in Forward path", styleLabel);
            }

            if (shaderName.Contains("Standard Lit") || shaderName.Contains("Subsurface Lit") || shaderName.Contains("Translucency Lit"))
            {
                DrawTechincalLabel("Lighting Model: Physicaly Based Shading", styleLabel);
            }

            if (shaderName.Contains("Simple Lit"))
            {
                DrawTechincalLabel("Lighting Model: Blinn Phong Shading", styleLabel);
            }

            if (shaderName.Contains("Standard Lit") && (shaderName.Contains("Cross") || shaderName.Contains("Grass") || shaderName.Contains("Leaf")))
            {
                DrawTechincalLabel("Subsurface Model: Transmission Approximation + View Based Light Scattering", styleLabel);
            }

            if (shaderName.Contains("Subsurface Lit") && (shaderName.Contains("Cross") || shaderName.Contains("Grass") || shaderName.Contains("Leaf")))
            {
                DrawTechincalLabel("Subsurface Model: Transmission + View Based Light Scattering", styleLabel);
            }

            if (shaderName.Contains("Simple Lit") && (shaderName.Contains("Cross") || shaderName.Contains("Grass") || shaderName.Contains("Leaf")))
            {
                DrawTechincalLabel("Subsurface Model: Transmission Approximation + View Based Light Scattering", styleLabel);
            }

            if (material.HasProperty("_IsColorsShader"))
            {
                DrawTechincalLabel("Render Buffer: Rendered by the Colors Volume", styleLabel);
            }

            if (material.HasProperty("_IsExtrasShader"))
            {
                DrawTechincalLabel("Render Buffer: Rendered by the Extras Volume", styleLabel);
            }

            if (material.HasProperty("_IsVertexShader"))
            {
                DrawTechincalLabel("Render Buffer: Rendered by the Vertex Volume", styleLabel);
            }

            if (material.HasProperty("_IsCustomShader"))
            {
                DrawTechincalLabel("Render Buffer: Rendered by the Custom Volumes", styleLabel);
            }

            if (material.HasProperty("_IsPropShader"))
            {
                DrawTechincalLabel("Batching Support: Yes", styleLabel);
            }
            else if (material.HasProperty("_IsTVEAIShader") || material.HasProperty("_IsElementShader"))
            {
                DrawTechincalLabel("Batching Support: No", styleLabel);
            }
            else
            {
                DrawTechincalLabel("Batching Support: Yes, with limited features", styleLabel);
            }
        }

        public static void DrawTechincalLabel(string label, GUIStyle style)
        {
            GUILayout.Label("<color=#7f7f7f><size=10>" + label + "</size></color>", style);
        }

        public static void DrawCopySettingsFromGameObject(Material material)
        {
            GameObject go = null;
            go = (GameObject)EditorGUILayout.ObjectField("Copy Settings From GameObject", go, typeof(GameObject), true);

            if (go != null)
            {
                var oldMaterials = go.GetComponent<MeshRenderer>().sharedMaterials;

                for (int i = 0; i < oldMaterials.Length; i++)
                {
                    var oldMaterial = oldMaterials[i];

                    if (oldMaterial != null)
                    {
                        CopyMaterialProperties(oldMaterial, material);
                        material.SetFloat("_IsInitialized", 1);
                        go = null;
                    }
                }
            }
        }




    }
}
