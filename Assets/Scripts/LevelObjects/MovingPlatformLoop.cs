using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Moves around nodes in a loop.
 * When it gets to the last node, it will move back towards node 0 and repeat.
 */
public class MovingPlatformLoop : MovingPlatform
{
	public float speed;

	protected override void Start()
	{
		base.Start();
		NextNode();
	}

	private void NextNode()
	{
		transform.position = nodes[targetNode].position;
		int oldNode = targetNode;
		targetNode = (targetNode + 1) % nodes.Length;
		Vector2 diff = nodes[targetNode].position - nodes[oldNode].position;
		rb.velocity = diff.normalized * speed;

		float time = diff.magnitude / speed;
		Invoke("NextNode", time);
	}
}
