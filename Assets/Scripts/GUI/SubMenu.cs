﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SubMenu : BaseMenu
{

	public GameObject firstSelected;

	private BaseMenu baseMenu;
	private GameObject baseSelected;

	//Should be run immediately after instantiating
	public void SetBaseMenu(BaseMenu bm)
	{
		baseMenu = bm;
		eventSystem = baseMenu.eventSystem;
		baseSelected = eventSystem.currentSelectedGameObject;
		baseMenu.DisableAll();
		eventSystem.SetSelectedGameObject(firstSelected);
	}

	public void Close()
	{
		baseMenu.EnableAll();
		eventSystem.SetSelectedGameObject(baseSelected);
		Destroy(gameObject);
	}
}
