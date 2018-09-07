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

	public void Hover(Selectable selectable)
	{
		if (selectable.gameObject != eventSystem.currentSelectedGameObject) //avoids Unity selectable-lock issue
		{
			eventSystem.SetSelectedGameObject(null);
		}
		sound.PlayHover();
	}

	private void Update()
	{
		bool moving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
		if (moving && eventSystem.currentSelectedGameObject == null)
		{
			eventSystem.SetSelectedGameObject(GetFirstSelected());
		}
	}

	protected virtual GameObject GetFirstSelected()
	{
		return eventSystem.firstSelectedGameObject;
	}
}
