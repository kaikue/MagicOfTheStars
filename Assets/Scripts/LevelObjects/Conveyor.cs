using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Doesn't use any nodes.
 * Stays in place but fakes movement by having a velocity.
 * Moves in same direction on all sides... not really like a real conveyor...
 *	will this ever come up?
 *	maybe if gravity flipping
 * Could also fake conveyor with a lot of small looping moving platforms
 */
public class Conveyor : MovingPlatform
{
	
	public enum Direction
	{
		Left,
		Right,
		Down,
		Up
	}

	private static readonly Vector2[] directions =
	{
		new Vector2(-1, 0),
		new Vector2(1, 0),
		new Vector2(0, -1),
		new Vector2(0, 1)
	};

	public float speed;
	public Direction direction;
	
	private void FixedUpdate()
	{
		//stay in place with MovePosition to current position
		rb.MovePosition(rb.position);

		//set velocity
		rb.velocity = speed * directions[(int)direction];
	}
}
