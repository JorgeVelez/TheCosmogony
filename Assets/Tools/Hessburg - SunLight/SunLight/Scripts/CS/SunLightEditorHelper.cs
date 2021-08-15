using UnityEngine;
using System.Collections;

namespace Hessburg
{
	// This script is necessary to to preview the light based on inspector settings and to set back Atmosphere thickness if the users leaves playmode during night times
	[ExecuteInEditMode]
	public class SunLightEditorHelper : MonoBehaviour 
	{	
		#if UNITY_EDITOR
		private Material SkyboxOriginalMaterial;
		private SunLight SunLight;

		void Awake() 
		{
			SunLight=this.GetComponent<SunLight>();
			if(SunLight.SunLightInitialised==false) SunLight.InitialiseSunLight();
			SkyboxOriginalMaterial = UnityEngine.RenderSettings.skybox;
		}
		
		
		void Update() 
		{
			//if(Application.isPlaying==false && SkyboxOriginalMaterial.GetFloat("_AtmosphereThickness")!=1.0f) 
			{
			//	SkyboxOriginalMaterial.SetFloat("_AtmosphereThickness", 1.0f);
			//	RenderSettings.skybox=SkyboxOriginalMaterial;
			}

			if(Application.isPlaying==false && SunLight.inspectorChanged==true)
			{
				SunLight.UpdateLight();
				SunLight.inspectorChanged=false;
			}				
		}
		#endif

	}
}

