using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
	private Rigidbody2D rb;
	private BoxCollider2D bc;

	private bool held = false;
	private float gravityScale;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		bc = GetComponent<BoxCollider2D>();
		gravityScale = rb.gravityScale;
	}

	public Vector2 GetSize()
	{
		return new Vector2(bc.size.x * transform.lossyScale.x, bc.size.y * transform.lossyScale.y);
	}

	public void PickUp(Vector2 pos)
	{
		gameObject.layer = LayerMask.NameToLayer("HeldObject");
		rb.isKinematic = true;
		rb.gravityScale = 0;
		MoveTo(pos);
		held = true;
	}

	public void Drop(Vector2 pos)
	{
		gameObject.layer = LayerMask.NameToLayer("LevelGeometry");
		MoveTo(pos);
		rb.isKinematic = false;
		rb.gravityScale = gravityScale;
		held = false;
	}

	public void AddForce(Vector2 force)
	{
		rb.AddForce(force);
	}

	public bool IsHeld()
	{
		return held;
	}

	private void MoveTo(Vector2 pos)
	{
		bc.enabled = false;
		transform.position = pos;
		bc.enabled = true;
	}

	//TODO: when held, forward LevelGeometry collisions onto player (so HitCeiling etc. will still happen)

	private void OnTriggerStay2D(Collider2D collider)
	{
		GameObject other = collider.gameObject;
		GrabZone g = other.GetComponent<GrabZone>();
		if (g != null)
		{
			g.AddGrabbable(this);
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		GameObject other = collider.gameObject;
		GrabZone g = other.GetComponent<GrabZone>();
		if (g != null)
		{
			g.RemoveGrabbable(this);
		}
	}
}
