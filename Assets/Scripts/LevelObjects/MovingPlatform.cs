using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	public Transform[] nodes;

	protected Rigidbody2D rb;
	protected int targetNode = 0;

	protected virtual void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}
}
