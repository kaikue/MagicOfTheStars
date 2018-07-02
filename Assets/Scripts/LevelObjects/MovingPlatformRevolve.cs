using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Currently only uses one node (the center it orbits around).
 * Could be extended to use multiple nodes (e.g. infinity sign-shaped path).
 */
public class MovingPlatformRevolve : MovingPlatform
{
	private const float TAU = 2 * Mathf.PI;

	public float speed;
	public bool clockwise;
	
	private Vector2 center;
	private float radius;
	private float theta;

	protected override void Start()
	{
		base.Start();
		center = nodes[0].position;
		radius = ((Vector2)transform.position - center).magnitude;
		float xDiff = transform.position.x - nodes[0].position.x;
		float yDiff = transform.position.y - nodes[0].position.y;
		theta = Mathf.Atan2(yDiff, xDiff);
	}

	private void FixedUpdate()
	{
		//can't use velocity-driven movement because the tangents would need to be applied every frame
		//so just set the position directly
		theta += speed / radius * Time.fixedDeltaTime;
		if (theta > TAU)
		{
			theta -= TAU;
		}
		Vector2 pos = center + new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * radius;
		rb.MovePosition(pos);

		//set velocity (describing this frame's movement)
		Vector2 vel = new Vector2(pos.x - transform.position.x, pos.y - transform.position.y) / Time.fixedDeltaTime;
		rb.velocity = vel;
	}
}
