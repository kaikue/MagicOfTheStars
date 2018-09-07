using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : BaseMenu
{

	public GameObject optionsOverlayPrefab;

	private GameManager gm;

	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	public void Resume()
	{
		gm.TogglePauseMenu();
	}

	public void Options()
	{
		sound.PlayConfirm();
		GameObject optionsOverlay = Instantiate(optionsOverlayPrefab);
		optionsOverlay.GetComponent<Options>().SetBaseMenu(this);
	}

	public void Exit()
	{
		gm.QuitToTitle();
	}
}
