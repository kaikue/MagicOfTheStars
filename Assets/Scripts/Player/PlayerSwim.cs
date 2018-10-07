using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Movement for swimming underwater.
 * Player has full 360 degree control of movement.
 * Rolling boosts forward.
 */
public class PlayerSwim : PlayerMovement
{
	private const float SWIM_LERP = 4.0f; //acceleration factor of swimming (higher = snappier acceleration)
	private const float SWIM_SPEED = 10.0f; //natural swimming velocity

	private const float ROLL_SPEED = 2 * SWIM_SPEED; //speed of roll
	private const float ROLL_TIME = 1.0f; //time it takes for roll to wear off naturally
	private const float ROLL_RELEASE_FACTOR = 0.5f; //factor to reduce roll by when releasing
	private const float ROLL_RECHARGE_TIME = 0.1f; //time it takes for roll to recharge after finishing (total time = ROLL_TIME + ROLL_RECHARGE_TIME)

	private float rollTime = 0;
	private bool canRoll = true;
	private float rollRecharge = 0;
	private Vector2 lastInputDir = Vector2.zero;
	private Vector2 rollDir = Vector2.zero;

	public PlayerSwim(Player player) : base(player)
	{
		
	}

	public override void SetCamera(CinemachineFramingTransposer body)
	{
		body.m_LookaheadIgnoreY = false;
		body.m_ScreenY = 0.5f;
		body.m_DeadZoneHeight = 0.2f;
		body.m_BiasY = 0;
	}

	public override void Move(PlayerInput input)
	{
		Vector2 velocity = rb.velocity;

		Vector2 inputDir = input.GetAxes().normalized;
		if (inputDir.magnitude > 0)
		{
			lastInputDir = inputDir;
		}

		Vector2 inputVel = inputDir * SWIM_SPEED;
		velocity = Vector2.Lerp(velocity, inputVel, SWIM_LERP * Time.fixedDeltaTime);

		if (input.rollQueued && canRoll)
		{
			//roll
			rollDir = lastInputDir;
			rollTime = ROLL_TIME;
			rollRecharge = ROLL_RECHARGE_TIME;
			canRoll = false;
			player.PlayRollSound();
		}
		input.rollQueued = false;

		if (IsRolling())
		{
			if (input.rollReleaseQueued)
			{
				rollTime *= ROLL_RELEASE_FACTOR;
			}

			float timeFactor = rollTime / ROLL_TIME;
			Vector2 rollVel = rollDir * ROLL_SPEED * timeFactor;

			if (rollVel.magnitude > velocity.magnitude)
			{
				//this will get smoothed out by the lerp, so it's fine to set it this way
				velocity = rollVel;
			}

			rollTime -= Time.fixedDeltaTime;
		}
		else if (rollRecharge > 0) //roll is recharging
		{
			rollRecharge -= Time.fixedDeltaTime;
		}
		else if (!canRoll)
		{
			SetCanRoll();
		}
		input.rollReleaseQueued = false;

		Vector2 movement = velocity * Time.fixedDeltaTime;
		rb.velocity = velocity;
		rb.MovePosition(rb.position + movement);
	}

	public override bool IsRolling()
	{
		return rollTime > 0;
	}

	public override void SetCanRoll()
	{
		base.SetCanRoll();

		rollRecharge = 0;
		canRoll = true;
	}
}
