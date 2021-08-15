using UnityEngine;
using System.Collections;

// This very simple demo shows how to access SunLight with a C# script
using Hessburg; 

public class SimpleSunLightExampleCS : MonoBehaviour 
{
	public SunLight SunLight; // set in inspector
	public UnityEngine.UI.Text Tex; // set in inspector
	public bool ExperimentalOn; // Demonstrates the experimental overcast skybox

	void Start() 
	{
		UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;  // to make sure that the ambient light will based on the skybox

		SunLight.latitude = 0.712778f; // Set latitude (float, -90.0 to 90.0)
		SunLight.longitude = -74.005833f; // Set longitude (float -180.0 to 180.0)
		SunLight.offsetUTC = -5.0f; // Timezone / Offset from UTC in hours (float -12.0 to 14.0)
		SunLight.dayOfYear = 120; // The day of the year (1 to 355 (366 in leap years)
		SunLight.timeInHours = 12.5f; // Time in decimal hours – 8.5 equals 12:30 PM (float 0.0 to 24.0)
		SunLight.SetTime(12, 30, 0); // Sets time by Hour, Minute and Seconds (int Hour, int Minute, int Seconds)

		SunLight.progressTime = true; // Time will progress automatically (boolean)
		SunLight.timeProgressFactor = 3000.0f; // How fast time will progress, real time would be 1.0 (float 0.0 – )
		SunLight.leapYear = false; // Set to true if you want to simulate a leap year (boolean)

		// optional settings:		
		SunLight.overrideAzimuth = false; // Set to true if you want to set the horizontal rotation of the light manually (boolean)
		SunLight.artisticSunAzimuth = 180.0f; // The horizontal rotation of the sunlight in degrees – requires overrideAzimuth set to true (0.0 – 360.0)

		/* 
		Light mySceneLight = SunLight.GetSceneLight(); // Returns the light used to lit the scene (Light)
		Light mySkyboxLight = SunLight.GetSkyboxLight(); // Returns the light used to trigger the Unity Skybox (Light)
		LensFlare myLensFlare = SunLight.GetLensFlare(); // Returns the lens flare (LensFlare)
		mySceneLight.enabled=true;
		mySkyboxLight.enabled=true;
		myLensFlare.enabled=true;	
		*/

		SunLight.overcastFactor=1.0f;
	}

	void Update() 
	{
		// in this example the time progresses at 15000x speed of real time if time is later than 20:00 or time is earlier than 04:00
		// otherwise (at daytime) set time progress to 3000x speed of real time
		if(SunLight.timeInHours>20.0f || SunLight.timeInHours <4.0f)
		{
			SunLight.timeProgressFactor = 15000.0f;
		}
		else
		{
			SunLight.timeProgressFactor = 3000.0f;
		}	

		string TS  = "SunLight\nTime: ";
		if(SunLight.GetHours()<10) TS = "SunLight\nTime: 0";
		TS=TS+SunLight.GetHours()+":";
		if(SunLight.GetMinutes()<10) TS = TS + "0";
		TS=TS+SunLight.GetMinutes()+":";
		if(SunLight.GetSeconds()<10) TS = TS + "0";
		TS=TS+SunLight.GetSeconds();
		TS=TS+"\nSun Altitude: "+SunLight.GetSunAltitude()+"\nSun Azimuth: "+SunLight.GetSunAzimuth();
		TS=TS+"\nSun Color: "+SunLight.GetSceneLight().color;// .ToString; 


		Color C;
		if(SunLight.GetSunAltitude()>180.0)
		{
			C = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp((355.0f-SunLight.GetSunAltitude())*0.05f, 0.0f, 1.0f));
		}	
		else
		{
			C = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		}	

		// Experimental – simulate overcast by converting the color of the sky to grayscale
		if(ExperimentalOn)
		{
			SunLight.overcastFactor = Mathf.PingPong(Time.time*0.1f, 1.0f);
	 		SunLight.useOvercastSkyboxShader = true;
	 		TS=TS+"\nOvercast Factor (experimental): "+SunLight.overcastFactor;
	 	}
	 	else
	 	{
	 		SunLight.overcastFactor=0.0f;
	 		SunLight.useOvercastSkyboxShader=true;
	 	}	
	 	Tex.text = TS;

	}	
}
