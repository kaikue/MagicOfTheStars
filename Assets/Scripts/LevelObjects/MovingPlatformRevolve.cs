using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Doesn't work due to discrete timesteps causing incorrect velocity tangent calculations.
 * 
 * Currently only uses one node (the center it orbits around).
 * Could be extended to use multiple nodes (e.g. infinity sign-shaped path).
 */
public class MovingPlatformRevolve : MovingPlatform
{
	public float speed;
	public bool clockwise;

	private float radius;
	private float initialTheta;

	protected override void Start()
	{
		base.Start();
		radius = (transform.position - nodes[0].position).magnitude;
		initialTheta = 0; //???
	}

	private void FixedUpdate()
	{
		Vector2 diff = transform.position - nodes[0].position;
		Vector2 tangent = Vector2.Perpendicular(diff).normalized;
		int direction = clockwise ? -1 : 1;
		rb.velocity = tangent * speed * direction;

		//set position directly- can't use velocity because the tangents would need to be applied every frame
		//rb.MovePosition(); //need to keep in sync with velocity somehow- angular velocity?
	}
}
