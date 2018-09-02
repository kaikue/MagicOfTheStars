using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{

	public Slider speedSlider;
	public Text speedPercentText;
	public Toggle invincibilityToggle;

	public MenuSound sounds;

	public const string KEY_GAME_SPEED = "gameSpeed";
	public const string KEY_INVINCIBILITY = "invincibility";

	private void Start()
	{
		speedSlider.value = PlayerPrefs.GetFloat(KEY_GAME_SPEED, 1) * speedSlider.maxValue;
		invincibilityToggle.isOn = PlayerPrefs.GetInt(KEY_INVINCIBILITY, 0) == 1;
	}

	public void UpdateSpeed()
	{
		float value = speedSlider.value / speedSlider.maxValue;
		PlayerPrefs.SetFloat(KEY_GAME_SPEED, value);
		PlayerPrefs.Save();

		speedPercentText.text = value * 100 + "%";
	}

	public void UpdateInvincibility()
	{
		int value = invincibilityToggle.isOn ? 1 : 0;
		PlayerPrefs.SetInt(KEY_INVINCIBILITY, value);
		PlayerPrefs.Save();
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
