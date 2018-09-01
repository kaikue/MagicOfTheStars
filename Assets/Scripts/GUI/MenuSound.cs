using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSound : MonoBehaviour
{

	public AudioClip hoverSound;
	public AudioClip confirmSound;
	public AudioClip cancelSound;
	public AudioSource audioSrc;

	public void PlayHover()
	{
		PlaySound(hoverSound);
	}

	public void PlayConfirm()
	{
		PlaySound(confirmSound);
	}

	public void PlayCancel()
	{
		PlaySound(cancelSound);
	}

	private void PlaySound(AudioClip sound)
	{
		audioSrc.PlayOneShot(sound);
	}
}
