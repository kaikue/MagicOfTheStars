using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{

	public GameObject continueButton;
	public GameObject loadingOverlay;
	public MenuSound sounds;
	
	private string savePath;

	private void Start()
	{
		savePath = GameManager.GetSavePath();
		if (!File.Exists(savePath))
		{
			continueButton.SetActive(false);
		}
	}

	public void Continue()
	{
		sounds.PlayConfirm();

		string[] lines = File.ReadAllLines(savePath);
		string levelName = lines[0];
		loadingOverlay.SetActive(true);
		SceneManager.LoadScene(levelName);
	}

	public void NewGame()
	{
		sounds.PlayConfirm();

		if (File.Exists(savePath))
		{
			//TODO: confirm deletion
			//delete old save
			File.Delete(GameManager.GetSavePath());
		}
		
		loadingOverlay.SetActive(true);
		//load intro
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void Options()
	{
		sounds.PlayConfirm();
		//TODO
	}

	public void Quit()
	{
		Application.Quit();
	}
}
