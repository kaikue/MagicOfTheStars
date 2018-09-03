using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{

	private const float adjustFactor = 2.0f;

	public static void SetVolume()
	{
		AudioListener.volume = ScaleVolume(PlayerPrefs.GetFloat(Options.KEY_SOUND_VOLUME, 1));
	}

	public static float ScaleVolume(float unscaledVolume)
	{
		return Mathf.Pow(unscaledVolume, adjustFactor);
	}
}
