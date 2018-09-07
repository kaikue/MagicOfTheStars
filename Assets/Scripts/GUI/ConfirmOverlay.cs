using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmOverlay : SubMenu
{

	public Text questionText;
	public Text descriptionText;
	public Text yesText;
	public Text noText;

	private Action yesAction;
	private Action noAction;

	public void SetContents(Action yesAction, Action noAction, string question = "Are you sure?", string description = "", string yesLabel = "Yes", string noLabel = "No")
	{
		this.yesAction = yesAction;
		this.noAction = noAction;
		questionText.text = question;
		descriptionText.text = description;
		yesText.text = yesLabel;
		noText.text = noLabel;
	}

	public void Yes()
	{
		yesAction();
	}

	public void No()
	{
		noAction();
	}
}
