using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : BaseMenu
{
	
	private GameManager gm;

	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	public void Resume()
	{
		gm.TogglePauseMenu();
	}

	public void Exit()
	{
		gm.QuitToTitle();
	}
}
