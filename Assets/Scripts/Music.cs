using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{

	private AudioSource audioSrc;

	private void Awake()
	{
		audioSrc = GetComponent<AudioSource>();
	}

	private void Start()
	{
			SetVolume();
	}
	
	public void SetVolume()
	{
		float vol = Utils.ScaleVolume(PlayerPrefs.GetFloat(Options.KEY_MUSIC_VOLUME, 1));
		audioSrc.volume = vol;
	}
}
