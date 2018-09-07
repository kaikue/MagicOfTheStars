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
	public GameObject confirmOverlayPrefab;
	public GameObject optionsOverlayPrefab;
	public GameObject loadingOverlay;

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
		sound.PlayConfirm();

		string[] lines = File.ReadAllLines(savePath);
		string levelName = lines[0];
		loadingOverlay.SetActive(true);
		SceneManager.LoadScene(levelName);
	}

	public void NewGame()
	{
		sound.PlayConfirm();

		if (File.Exists(savePath))
		{
			GameObject confirmOverlay = Instantiate(confirmOverlayPrefab);
			ConfirmOverlay co = confirmOverlay.GetComponent<ConfirmOverlay>();
			co.SetBaseMenu(this);
			co.SetContents(DeleteAndStart, co.Close, "Start new game?", "This will overwrite your old save!", "Clear Save", "Cancel");
		}
		else
		{
			StartGame();
		}
	}

	public void Options()
	{
		sound.PlayConfirm();
		GameObject optionsOverlay = Instantiate(optionsOverlayPrefab);
		optionsOverlay.GetComponent<Options>().SetBaseMenu(this);
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void DeleteAndStart()
	{
		//this might not need to be in a File.Exists check, but it's probably safer
		File.Delete(GameManager.GetSavePath());
		StartGame();
	}

	private void StartGame()
	{
		loadingOverlay.SetActive(true);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}
}
