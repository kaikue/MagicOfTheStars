using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseMenu : MonoBehaviour
{
	//These should only be contained in base-level menus- only one per scene
	public EventSystem eventSystem;
	public MenuSound sound;

	public void DisableAll()
	{
		SetAllEnabled(false);
	}

	public void EnableAll()
	{
		SetAllEnabled(true);
	}

	protected void SetAllEnabled(bool enabled)
	{
		Selectable[] selectables = GetComponentsInChildren<Selectable>();
		foreach (Selectable s in selectables)
		{
			s.interactable = enabled;
		}
	}

	public void Hover()
	{
		eventSystem.SetSelectedGameObject(null);
		sound.PlayHover();
	}
}
