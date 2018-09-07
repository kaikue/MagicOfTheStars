using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableEvents : EventTrigger
{

	private BaseMenu menu;
	private Selectable selectable;

	private void Awake()
	{
		menu = GetComponentInParent<BaseMenu>();
		selectable = GetComponent<Selectable>();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		menu.Hover(selectable);
		base.OnSelect(eventData);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		menu.Hover(selectable);
		base.OnPointerEnter(eventData);
	}

}
