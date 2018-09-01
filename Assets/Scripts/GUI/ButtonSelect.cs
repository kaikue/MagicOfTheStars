using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelect : MonoBehaviour
{

	public EventSystem eventSystem;
	public Button[] buttons; //list of buttons in top-to-bottom order

	private Button selectedButton;
	private int selectedIndex;

	private void Start()
	{
		selectedButton = buttons[0];
	}

	private void Update()
	{
		if (Input.GetButtonDown("Jump"))
		{
			ActivateSelectedButton();
		}

		if (false) //up
		{
			PrevButton();
		}
		else if (false) //down
		{
			NextButton();
		}
	}

	private void ActivateSelectedButton()
	{
		selectedButton.onClick.Invoke();
	}

	private void NextButton()
	{
		ChangeButton(1);
	}

	private void PrevButton()
	{
		ChangeButton(-1);
	}

	private void ChangeButton(int change)
	{
		//selectedButton.OnDeselect();
		//selectedButton.hover = false; //deselect old button

		int numButtons = buttons.Length;
		selectedIndex += change;
		if (selectedIndex < 0)
		{
			selectedIndex += numButtons;
		}
		if (selectedIndex > numButtons)
		{
			selectedIndex -= numButtons;
		}
		selectedButton = buttons[selectedIndex];
		//TODO: if button is inactive or disabled, change again (check for loop)

		//selectedButton.hover = true; //select new button
		eventSystem.SetSelectedGameObject(selectedButton.gameObject);
	}
}
