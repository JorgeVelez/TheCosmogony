using UnityEngine;
using System.Collections;

namespace Hessburg
{
	[System.Serializable]
	public class SunLightCalculation
	{
		void Awake()
		{
			lastTimeInHours=-9999.0f;
		}
			
		public SunLightCalculation CreateFromJSON(string jsonString) 
		{
			return JsonUtility.FromJson<SunLightCalculation>(jsonString);
		}	

		public void CreateSkyboxLight(Vector3 pos, int skyboxLightLayerMask, GameObject SunLightGO)
		{
			bool found = false;
			foreach (Transform t in SunLightGO.transform)
			{
			     if(t.name == "SL_SkyboxLight")
			     {
			     	GameObject instance = t.transform.gameObject;
			     	skyboxLightTransform = instance.transform;
			        skyboxLightTransform.position = pos;
			        skyboxLight = instance.GetComponent<Light>();
			        skyboxLight.type = LightType.Directional;
			        skyboxLight.intensity = 8.0f; // set SkyboxLight to the highest intensity which should automatically set it as the directional light input of the Unity Skybox – necessary for Unity versions pre Unity 5.5

			        #if UNITY_5_5_OR_NEWER
    					RenderSettings.sun=skyboxLight;
  					#endif  

			       	skyboxLight.cullingMask = skyboxLightLayerMask;
			     	found=true;
			     }
			} 

			if(found==false)
			{
				GameObject instance = GameObject.Instantiate(Resources.Load("Hessburg/SL_SkyboxLight", typeof(GameObject))) as GameObject;
				instance.name="SL_SkyboxLight";
		      	skyboxLightTransform = instance.transform;
		        skyboxLightTransform.position = pos;
		        skyboxLight = instance.GetComponent<Light>();
		        skyboxLight.type = LightType.Directional;
		        skyboxLight.intensity = 8.0f; // set SkyboxLight to the highest intensity which should automatically set it as the directional light input of the Unity Skybox – necessary for Unity versions pre Unity 5.5

		        #if UNITY_5_5_OR_NEWER
					RenderSettings.sun=skyboxLight;
				#endif  

		       	skyboxLight.cullingMask = skyboxLightLayerMask;
	       	}
		}

		public void CreateSceneLight(Vector3 pos, int sceneLightLayerMask, GameObject SunLightGO)
		{
			bool found = false;
			foreach (Transform t in SunLightGO.transform)
			{
			     if(t.name == "SL_SceneLight")
			     {
			     	GameObject instance = t.transform.gameObject;
			     	sceneLightTransform = instance.transform;
			        sceneLightTransform.position = pos;
			        sceneLight = instance.GetComponent<Light>();
			        sceneLight.type = LightType.Directional;
			        sceneLight.intensity = 1.0f;
			        sceneLight.cullingMask = sceneLightLayerMask;
			     	found=true;
			     }
			} 

			if(found==false)
			{
				GameObject instance = GameObject.Instantiate(Resources.Load("Hessburg/SL_SceneLight", typeof(GameObject))) as GameObject;
				instance.name="SL_SceneLight";
				sceneLightTransform = instance.transform;
		        sceneLightTransform.position = pos;
		        sceneLight = instance.GetComponent<Light>();
		        sceneLight.type = LightType.Directional;
		        sceneLight.intensity = 1.0f;
		        sceneLight.cullingMask = sceneLightLayerMask;
			}
		}	

		public void CreateLensFlare(Vector3 pos, GameObject SunLightGO)
		{
			bool found = false;
			foreach (Transform t in SunLightGO.transform)
			{
			     if(t.name == "SL_LensFlare")
			     {
			     	GameObject instance = t.transform.gameObject;
			     	lensFlareTransform = instance.transform;
					lensFlareTransform.position = pos;
					lensFlare=instance.GetComponent<LensFlare>();
			     	found=true;
			     }
			} 

			if(found==false)
			{
				GameObject instance = GameObject.Instantiate(Resources.Load("Hessburg/SL_LensFlare", typeof(GameObject))) as GameObject;
				instance.name="SL_LensFlare";
				lensFlareTransform = instance.transform;
				lensFlareTransform.position = pos;
				lensFlare=instance.GetComponent<LensFlare>();
			}		
		}	

		// Changed in version 1.1:
		//public void SetSun(float latitude, float longitude, float offsetUTC, int dayOfYear, float timeInHours, float overcastFactor, float artisticSunAzimuth, bool overrideAzimuth)
		public void SetSun(float latitude, float longitude, float offsetUTC, int dayOfYear, float timeInHours, float northDirection, float overcastFactor, float artisticSunAzimuth, bool overrideAzimuth)
		{
			// New in version 1.2.1:
			if(lastTimeInHours != timeInHours || lastLatitude != latitude || lastLongitude != longitude || lastDayOfYear != dayOfYear || lastOffsetUTC != offsetUTC || lastOvercastFactor != overcastFactor || lastOverrideAzimuth != overrideAzimuth || lastArtisticSunAzimuth != artisticSunAzimuth || lastNorthDirection != northDirection) 
			{
				lastLatitude=latitude; 
				lastLongitude=longitude; 
				lastOffsetUTC=offsetUTC; 
				lastDayOfYear=dayOfYear; 
				lastTimeInHours=timeInHours;
				lastOvercastFactor=overcastFactor; 
				lastOverrideAzimuth=overrideAzimuth; 
				lastArtisticSunAzimuth=artisticSunAzimuth;
				lastNorthDirection=northDirection;	
			// end of New in version 1.2.1:

				//ensure inputs stay within required limits
				latitude=Mathf.Clamp(latitude, -90.0f, 90.0f);
				longitude=Mathf.Clamp(longitude, -180.0f, 180.0f);
				offsetUTC=Mathf.Clamp(offsetUTC, -12.0f, 14.0f);
				dayOfYear=Mathf.Clamp(dayOfYear, 1, 366);
				timeInHours=Mathf.Clamp(timeInHours, 0.0f, 24.00f);
				overcastFactor=Mathf.Clamp(overcastFactor, 0.0f, 1.0f);
				artisticSunAzimuth=Mathf.Clamp(artisticSunAzimuth, 0.0f, 360.0f);

				// calculate azimut & altitute
				cosLatitude = Mathf.Cos(latitude*Mathf.Deg2Rad);
				sinLatitude = Mathf.Sin(latitude*Mathf.Deg2Rad);		
				D=DeclinationOfTheSun(dayOfYear);
				sinDeclination=Mathf.Sin(D);
				cosDeclination=Mathf.Cos(D);
				W = 360.0f/365.24f;
				A = W * (dayOfYear + 10.0f);
				B = A + (360.0f/Mathf.PI) * 0.0167f * Mathf.Sin(W * (dayOfYear-2) * Mathf.Deg2Rad);
				C = (A - (Mathf.Atan(Mathf.Tan(B*Mathf.Deg2Rad)/Mathf.Cos(23.44f*Mathf.Deg2Rad))*Mathf.Rad2Deg)) / 180.0f;		
				EquationOfTime = 720.0f * (C - Mathf.Round(C)) / 60.0f;
				hourAngle = (timeInHours + longitude / (360.0f / 24.0f) - offsetUTC - 12.0f + EquationOfTime) * (1.00273790935f-1.0f/365.25f)*(360.0f / 24.0f)*Mathf.Deg2Rad;

				azimuth = Mathf.Atan2(-cosDeclination * Mathf.Sin(hourAngle), sinDeclination * cosLatitude - cosDeclination * Mathf.Cos(hourAngle) * sinLatitude);	
				if (azimuth<0) azimuth = azimuth + 6.28318530717959f; 		
				altitude = Mathf.Asin(sinDeclination * sinLatitude + cosDeclination * Mathf.Cos(hourAngle) * cosLatitude );		
				
				// Refraction removed in version 1.2 (see readme):
				/*
				refraction = 0.0f;

				if (altitude>-0.01745329251994f)
				{
					refA = 0.00027754917f/Mathf.Tan(altitude);
					if(altitude>=0.2617993877991f)
					{
						refraction = refA;
					}
					else
					{
						refB = 17.68454864582921f * (0.1594f + (0.0196f * altitude) + (0.00002f * altitude * altitude)) / (288.0f * (1.0f + (0.505f * altitude) + (0.0845f *  altitude * altitude)));
						refB = Mathf.Lerp(0.0f, refB, Mathf.Clamp(altitude, -0.01745329251994f, 0.0f)/-0.01745329251994f);
					}			
					refraction = Mathf.Lerp(refB, refA, Mathf.Clamp(altitude, 0.0f, 0.2617993877991f)/0.2617993877991f);
				}
				*/

				azimuth = azimuth / Mathf.Deg2Rad; // Azimuth (in degrees): 0 = North, East = 90, South = 180, West = 270

				//New in version 1.1: North adjustment
					azimuth = azimuth + Mathf.Clamp(northDirection, 0.0f, 360.0f);
					if(azimuth>=360f) azimuth = azimuth - 360.0f;
				//

				if(overrideAzimuth==true) azimuth = artisticSunAzimuth;

				// Refraction removed in version 1.2 (see readme):
				//altitude = (altitude+refraction) / Mathf.Deg2Rad; // Height of the center of the sun (in degrees) - 0 = horizon - 90 = full zenit
				altitude = (altitude) / Mathf.Deg2Rad; // Height of the center of the sun (in degrees) - 0 = horizon - 90 = full zenit

				if(altitude<0.0f) altitude=360.0f+altitude;

				//Set rotations
				skyboxLightTransform.eulerAngles = new Vector3(altitude, azimuth, 0.0f); // Sun light is used to trigger the skybox only (it goes below horizon).	
				if(altitude>90.0f || altitude<0.0f) // Scene Light altitude angle is limited to avoid objects being lit from below the horizon during twilight conditions.
				{
					sceneLightTransform.eulerAngles = new Vector3(0.0f, azimuth, 0.0f); 
					lensFlareTransform.eulerAngles = new Vector3(0.0f, azimuth, 0.0f); 
				}
				else
				{
					sceneLightTransform.eulerAngles = new Vector3(altitude, azimuth, 0.0f);
					lensFlareTransform.eulerAngles = new Vector3(altitude, azimuth, 0.0f);
				}	

				//Set Light & Flare colors
				skyboxLight.color = Color.white; 
				sceneLight.color = SunlightColor(altitude, overcastFactor); // color of Scene light
				lensFlare.color = sceneLight.color; // color of Lens Flare
				if(altitude>90.0f)
				{
					lensFlare.brightness = Mathf.Clamp((altitude-355.0f)*0.2f, 0.0f, 1.0f)*0.22f; // brightness/size of LensFlare when center of sun is below horizon
				}	
				else
				{
					lensFlare.brightness = Mathf.Clamp((altitude*0.2f)+0.22f, 0.0f, 0.5f); // brightness/size of LensFlare when center of sun is above horizon
				}

				// Atmosphere Thickness
				if(altitude<360.0f && altitude>180.0f)
				{
					SkyboxMaterial.SetFloat("_AtmosphereThickness", Mathf.Clamp(1.0f + (360.0f-altitude)*0.04f, 1.0f, 2.0f)); // icrease atmosphere thickness at night to avoid red horizon
				}
				else
				{
					SkyboxMaterial.SetFloat("_AtmosphereThickness", 1.0f);
				}	

				sunAltitude=altitude;
				sunAzimuth=azimuth;
			}	
		}
		
		public Color SunlightColor(float altitude, float overcastFactor)
		{
			colorAltitude = altitude;
			// convert atitude to match the sorting of the precalculated color arrays
			if(altitude>=0.0f && altitude<180.0f) colorAltitude = altitude+15.0f;
			if(altitude>=345.0f) colorAltitude = 15.0f-(360.0f-altitude);

			if(altitude>180.0f && altitude<345.0f) colorAltitude = 0.0f;
			if(altitude>90.0f && altitude<=180.0f) colorAltitude = 105.0f; // should never happen, but I do it just in case of Gremlins

			floorAlt = (int)Mathf.Floor(colorAltitude);
			clearColor = Color.Lerp(sunColorClear[floorAlt], sunColorClear[floorAlt+1], colorAltitude-floorAlt);

			// PLEASE NOTE: This feature is not advertised: Use by scripting or enable overcast in the SunLightEditor script if you want to simulate the effect of an overcast sky on the clouds.
			if(overcastFactor!=0.0f)
			{	
				OCColor = Color.Lerp(sunColorOC[floorAlt], sunColorOC[floorAlt+1], colorAltitude-floorAlt);
				return Color.Lerp(clearColor, OCColor, overcastFactor);
			}
			else
			{
				return clearColor;
			}
		}	

		public float DeclinationOfTheSun(int dayOfYear)
		{
			WD = 360.0f/365.24f;
			AD = WD * (dayOfYear + 10.0f);
			BD = AD + (360.0f/Mathf.PI) * 0.0167f * Mathf.Sin(WD * (dayOfYear-2) * Mathf.Deg2Rad);
			return -Mathf.Asin(Mathf.Sin(23.44f * Mathf.Deg2Rad) * Mathf.Cos(BD * Mathf.Deg2Rad) );
		}

		void OnApplicationQuit()
		{
			RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1.0f);
		}	

		[HideInInspector]
		public Color[] sunColorClear;
		[HideInInspector]
		public Color[] sunColorOC;
		[HideInInspector]
		public Light skyboxLight; // This directional light will be instantiated from the prefab of the resources folder at runtime and it is used as input for the skybox
		[HideInInspector]
		public Light sceneLight; // This directional light will be instantiated from the prefab of the resources folder at runtime and it is used for lighting the scene only
		[HideInInspector]
		public Transform skyboxLightTransform;
		[HideInInspector]
		public Transform sceneLightTransform;
		[HideInInspector]
		public LensFlare lensFlare; // The lensFlare will be instantiated from the prefab of the resources folder at runtime 
		[HideInInspector]
		public Transform lensFlareTransform;
		[HideInInspector]
		public Material SkyboxMaterial;

		private float cosLatitude;
		private float sinLatitude;
		private float cosDeclination;
		private float sinDeclination;
		private float EquationOfTime;		
		private float hourAngle;  	
		private float azimuth;
		// Refraction removed in version 1.2 (see readme):
		//private float refraction;
		//private float refA; 
		//private float refB;
		private float altitude;
		private float colorAltitude;
		private int floorAlt;
		private Color clearColor; 
		private Color OCColor;
		private float W;
		private float A;
		private float B;	
		private float C;
		private float D;
		private float WD;
		private float AD;
		private float BD;	

		[HideInInspector]
		public float sunAltitude;
		[HideInInspector]
		public float sunAzimuth;

		// New in version 1.2.1
			private float lastLatitude; 
			private float lastLongitude; 
			private float lastOffsetUTC; 
			private int lastDayOfYear; 
			private float lastTimeInHours;

			private float lastOvercastFactor; 
			private bool lastOverrideAzimuth; 
			private float lastArtisticSunAzimuth;
			private float lastNorthDirection;
	}
}