using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{

	public GameObject ContinueButton;
	public GameObject loadingOverlay;

	public string FirstLevelName = "Hub";

	private void Start()
	{
		if (!File.Exists(GameManager.GetSavePath()))
		{
			ContinueButton.SetActive(false);
		}
	}

	public void Continue()
	{
		string[] lines = File.ReadAllLines(GameManager.GetSavePath());
		string levelName = lines[0];
		loadingOverlay.SetActive(true);
		SceneManager.LoadScene(levelName);
	}

	public void NewGame()
	{
		string savePath = GameManager.GetSavePath();
		if (File.Exists(savePath))
		{
			//delete old save
			File.Delete(GameManager.GetSavePath());
		}

		using (StreamWriter sw = File.CreateText(savePath))
		{
			sw.WriteLine(FirstLevelName);
		}

		loadingOverlay.SetActive(true);
		//load intro
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
