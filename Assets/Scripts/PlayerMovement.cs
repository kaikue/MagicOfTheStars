using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMovement
{
	protected Player player;
	protected Rigidbody2D rb;
	protected EdgeCollider2D ec;


	public PlayerMovement(Player player)
	{
		this.player = player;
		rb = player.rb;
		ec = player.ec;
	}

	public abstract void Move(PlayerInput input);

	public virtual void SetCanRoll()
	{

	}

	public virtual void SetCanMidairJump()
	{

	}

	public virtual void CollisionEnter(Collision2D collision)
	{

	}

	public virtual void CollisionStay(Collision2D collision)
	{

	}

	public virtual void CollisionExit(Collision2D collision)
	{

	}
}
