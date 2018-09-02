using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{

	public Slider musicSlider;
	public Text musicPercentText;
	public Slider volumeSlider;
	public Text volumePercentText;
	public Slider speedSlider;
	public Text speedPercentText;
	public Toggle invincibilityToggle;
	public Text invincibilityOnOffText;

	public MenuSound sounds;

	public const string KEY_MUSIC_VOLUME = "musicVolume";
	public const string KEY_SOUND_VOLUME = "soundVolume";
	public const string KEY_GAME_SPEED = "gameSpeed";
	public const string KEY_INVINCIBILITY = "invincibility";

	private const float TIME_SLIDER_MULTIPLIER = 100f; //speed slider value is actual value times this

	private void Start()
	{
		musicSlider.value = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f) * musicSlider.maxValue;
		volumeSlider.value = PlayerPrefs.GetFloat(KEY_SOUND_VOLUME, 1f) * volumeSlider.maxValue;
		speedSlider.value = PlayerPrefs.GetFloat(KEY_GAME_SPEED, 1f) * TIME_SLIDER_MULTIPLIER;
		invincibilityToggle.isOn = PlayerPrefs.GetInt(KEY_INVINCIBILITY, 0) == 1;

		//update everything so it gets saved & labels update
		UpdateMusic();
		UpdateSound();
		UpdateSpeed();
		UpdateInvincibility();
	}

	private void UpdateFloat(Slider slider, float sliderScale, string key, Text percentText)
	{
		float value = slider.value / sliderScale;
		PlayerPrefs.SetFloat(key, value);
		PlayerPrefs.Save();

		percentText.text = slider.value + "%";
	}

	public void UpdateMusic()
	{
		UpdateFloat(musicSlider, musicSlider.maxValue, KEY_MUSIC_VOLUME, musicPercentText);
		
		GameObject music = GameObject.Find("Music");
		if (music != null)
		{
			music.GetComponent<Music>().SetVolume();
		}
	}

	public void UpdateSound()
	{
		UpdateFloat(volumeSlider, volumeSlider.maxValue, KEY_SOUND_VOLUME, volumePercentText);

		Utils.SetVolume();
	}

	public void UpdateSpeed()
	{
		UpdateFloat(speedSlider, TIME_SLIDER_MULTIPLIER, KEY_GAME_SPEED, speedPercentText);
		
		GameObject gm = GameObject.Find("GameManager");
		if (gm != null)
		{
			gm.GetComponent<GameManager>().SetTimeScale();
		}
	}

	public void UpdateInvincibility()
	{
		int value = invincibilityToggle.isOn ? 1 : 0;
		PlayerPrefs.SetInt(KEY_INVINCIBILITY, value);
		PlayerPrefs.Save();

		invincibilityOnOffText.text = invincibilityToggle.isOn ? "On" : "Off";
	}

	public void Return()
	{
		sounds.PlayCancel();
		Destroy(gameObject);
	}

	private void Update()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			Return();
		}
	}
}
