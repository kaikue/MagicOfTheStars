using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwim : PlayerMovement
{
	private const float SWIM_LERP = 4.0f;
	private const float SWIM_SPEED = 10.0f;

	public PlayerSwim(Player player) : base(player)
	{

	}

	public override void Move(PlayerInput input)
	{
		Vector2 velocity = rb.velocity;

		Vector2 inputVel = input.GetAxes() * SWIM_SPEED;

		velocity = Vector2.Lerp(velocity, inputVel, SWIM_LERP * Time.fixedDeltaTime);

		Vector2 movement = velocity * Time.fixedDeltaTime;
		rb.velocity = velocity;
		rb.MovePosition(rb.position + movement);
	}
}
