using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabZone : MonoBehaviour
{

	private Player player;
	private SpriteRenderer playerSR;
	private List<Grabbable> grabbables;
	private float xDistance;
	
	private void Start()
	{
		grabbables = new List<Grabbable>();
		player = transform.parent.GetComponent<Player>();
		playerSR = player.spriteObject.GetComponent<SpriteRenderer>();
		xDistance = Mathf.Abs(transform.localPosition.x);
	}

	private void Update()
	{
		Vector3 pos = transform.localPosition;
		//change this if sprites face left
		pos.x = (playerSR.flipX ? -1 : 1) * xDistance;
		transform.localPosition = pos;
	}

	public void AddGrabbable(Grabbable g)
	{
		if (!g.IsHeld() && !grabbables.Contains(g))
		{
			grabbables.Add(g);
		}
	}

	public void RemoveGrabbable(Grabbable g)
	{
		grabbables.Remove(g);
	}

	public Grabbable GetGrabbable()
	{
		int numGrabbables = grabbables.Count;
		if (numGrabbables > 0)
		{
			return grabbables[numGrabbables - 1];
		}
		return null;
	}
}
