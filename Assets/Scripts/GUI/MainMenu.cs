using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : BaseMenu
{

	public Button continueButton;
	public Button newGameButton;
	public GameObject optionsOverlayPrefab;
	public GameObject loadingOverlay;
	public MenuSound sounds;
	
	private string savePath;

	private void Awake()
	{
		savePath = GameManager.GetSavePath();
		if (!File.Exists(savePath))
		{
			continueButton.gameObject.SetActive(false);
			eventSystem.firstSelectedGameObject = newGameButton.gameObject;
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
			File.Delete(GameManager.GetSavePath());
		}
		
		loadingOverlay.SetActive(true);
		//load intro
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void Options()
	{
		sounds.PlayConfirm();
		GameObject optionsOverlay = Instantiate(optionsOverlayPrefab);
		optionsOverlay.GetComponent<Options>().SetBaseMenu(this);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
