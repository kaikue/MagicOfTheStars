using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Movement for when on land.
 * Includes running, rolling, jumping, and walljumping.
 */
public class PlayerMove : PlayerMovement
{

	//private static float SLIDE_THRESHOLD;
	private static Vector2 GRAVITY_NORMAL = new Vector2(0, GRAVITY_ACCEL).normalized;

	private const float RUN_ACCEL = 25.0f; //acceleration of horizontal movement
	private const float MAX_RUN_SPEED = 9.0f; //maximum speed of horizontal movement

	private const float GRAVITY_ACCEL = -30.0f; //acceleration of gravity
	private const float MAX_FALL_SPEED = -50.0f; //maximum speed of fall
	private const float SLIDE_GRAVITY_ACCEL = -15.0f; //acceleration of gravity when sliding down wall
	private const float SLIDE_MAX_FALL_SPEED = -8.0f; //maximum speed of sliding down wall
													  //private const float SNAP_DIST = 0.5f;

	private const float JUMP_SPEED = 16.0f; //jump y speed
	private const float JUMP_RELEASE_FACTOR = 0.5f; //factor to reduce jump by when releasing
	private const float JUMP_GRACE_TIME = 0.1f; //time after leaving ground player can still jump

	private const float WALLJUMP_SPEED = MAX_RUN_SPEED; //speed applied at time of walljump
	private const float WALLJUMP_MIN_FACTOR = 0.75f; //amount of walljump kept at minimum if no input
	private const float WALLJUMP_TIME = 0.575f; //time it takes for walljump to wear off
	private const float WALLJUMP_GRACE_TIME = 0.2f; //time after leaving wall player can still walljump

	private const float ROLL_SPEED = 2 * MAX_RUN_SPEED; //speed of roll
	private const float ROLL_TIME = 1.0f; //time it takes for roll to wear off naturally
	private const float MAX_ROLL_TIME = 2.0f; //time it takes for roll to wear off at the bottom of a long slope
	private const float ROLL_MAX_ADDITION = 5.0f; //amount of roll added on high slopes
	private const float ROLL_FORCE_AMOUNT = 0.1f; //how much to push the player when they can't unroll
	private const float ROLL_RELEASE_FACTOR = 0.5f; //factor to reduce roll by when releasing
	private const float ROLL_HEIGHT = 0.5f; //scale factor of height when rolling

	private const float FEET_CHECK = 0.01f; //used in checking ground collision validity

	private bool canJump = false; //used instead of onGround to allow grace time after leaving platform
	private bool canJumpRelease = false; //to only allow the first jump release to slow the jump
	private List<GameObject> grounds = new List<GameObject>();
	private List<GameObject> walls = new List<GameObject>();
	private int wallSide = 0; //1 for wall on left, 0 for none, -1 for wall on right (i.e. points away from wall in x)- this is non-zero = the player can walljump
	private int lastWallSide = 0;
	private float walljumpTime = 0; //counts down from WALLJUMP_TIME
	private bool walljumpPush = false; //if the player hasn't touched anything and the walljump should keep moving them
	private bool canMidairJump = false;

	private float rollTime = 0;
	private bool canRoll = true;
	private int rollDir = 1; //-1 for left, 1 for right

	private float normalHeight;
	private bool rollingCollider = false;

	private Vector2 groundNormal;

	private Coroutine crtLeaveWall;

	private bool shouldStand = false;

	public PlayerMove(Player player) : base(player)
	{
		normalHeight = ec.points[1].y - ec.points[0].y;
	}

	public override void SetCamera(CinemachineFramingTransposer body)
	{
		body.m_LookaheadIgnoreY = true;
		body.m_ScreenY = 0.6f;
		body.m_DeadZoneHeight = 0.1f;
		body.m_BiasY = -0.1f;
	}

	public override void Move(PlayerInput input)
	{
		Vector2 velocity = rb.velocity; //for changing the player's actual velocity
		Vector2 offset = Vector2.zero; //for any additional movement nudges
		shouldStand = false;
		float inputXSpeed = input.GetAxes().x;
		if (inputXSpeed == 0)
		{
			velocity.x = 0;
			shouldStand = true;
		}
		else
		{
			MoveHorizontal(inputXSpeed, ref velocity);
		}

		bool onGround = grounds.Count > 0;
		/*if (!onGround && velocity.y == 0)
		{
			//not on ground but not moving up/down- try to snap to ground
			RaycastHit2D[] hits = BoxCast(GRAVITY_NORMAL, SNAP_DIST);
			if (hits.Length > 0)
			{
				RaycastHit2D hit = hits[0];
				float hitDist = hit.distance;
				//not quite the right distance (it's from center of bbox), but moveposition will handle that
				offset.y -= hitDist;
				onGround = true;
				grounds.Add(hit.transform.gameObject);
			}
		}*/

		if (onGround && velocity.y <= 0) //on the ground, didn't just jump
		{
			StayOnGround(ref velocity, ref offset);
		}
		else //in midair
		{
			if (input.jumpReleaseQueued)
			{
				JumpRelease(ref velocity);
			}

			if (!onGround && input.jumpQueued && wallSide != 0 && !IsRolling())
			{
				WallJump(input, ref velocity);
			}

			Fall(ref velocity);
		}

		if (!input.jumpQueued) //in case the player is about to jump, in which case their jump should be cut short
		{
			input.jumpReleaseQueued = false;
		}

		//continued moving past wall corner- clear wallslide
		//this should happen if:
		//	the player moves past a corner sliding up
		//this should not happen if:
		//	the player is moving away from the wall
		//	the player is in the middle of a walljump (e.g. slime side bounce)
		if (wallSide != 0 && Math.Sign(velocity.x) != wallSide && !IsWalljumping() && walls.Count == 0)
		{
			wallSide = 0;
		}

		/*
		//apply moving wall velocity
		Rigidbody2D wallRB = GetWallRigidbody();
		if (wallRB != null)
		{
			offset += wallRB.velocity * Time.fixedDeltaTime;
			//velocity.y = 0; //???
			//velocity.y = wallRB.velocity.y;
		}*/

		if (input.jumpQueued && ((canJump && velocity.y <= 0) || canMidairJump))
		{
			Jump(input, ref velocity);
		}

		if (input.grabQueued)
		{
			if (player.heldObject != null)
			{
				player.ThrowHeldObject(velocity);
			}
			else if (onGround && !IsRolling())
			{
				player.GrabObject();
			}
			input.grabQueued = false;
		}

		if (IsWalljumping())
		{
			ApplyWalljumpPush(ref velocity);
		}

		if (velocity.y != 0 && !IsRolling())
		{
			if (wallSide != 0 && velocity.y < 0 && Math.Sign(velocity.x) != wallSide)
			{
				player.SetAnimState(AnimState.WALLSLIDE);
				shouldStand = false;
			}
			else
			{
				player.SetAnimState(AnimState.JUMP);
				shouldStand = false;
			}
		}

		if (input.rollQueued && canRoll && player.heldObject == null)
		{
			Roll(ref velocity);
		}
		input.rollQueued = false;

		if (IsRolling())
		{
			if (input.rollReleaseQueued)
			{
				//slow your roll
				rollTime *= ROLL_RELEASE_FACTOR;
			}

			ApplyRoll(onGround, ref velocity, ref offset);

			if (IsRolling())
			{
				player.SetAnimState(AnimState.ROLL);
				shouldStand = false;
			}
		}
		input.rollReleaseQueued = false;

		if (shouldStand)
		{
			player.SetAnimState(AnimState.STAND);
		}

		if (velocity.x != 0)
		{
			player.facingLeft = velocity.x < 0;
			if (inputXSpeed == 0)
			{
				rollDir = Math.Sign(velocity.x);
			}
		}

		bool left;
		if (player.animState == AnimState.WALLSLIDE)
		{
			left = wallSide < 0;
		}
		else
		{
			left = player.facingLeft;
		}

		player.sr.flipX = left; //change this if sprites face left
		int xSign = left ? 1 : -1; //change this if sprites face left
		Vector3 spritePos = player.sr.transform.localPosition;
		spritePos.x = Mathf.Abs(spritePos.x) * xSign;
		player.sr.transform.localPosition = spritePos;

		Vector2 movement = velocity * Time.fixedDeltaTime + offset;

		rb.velocity = velocity;
		rb.MovePosition(rb.position + movement);
	}


	private void MoveHorizontal(float inputXSpeed, ref Vector2 velocity)
	{
		if (velocity.x != 0 && Mathf.Sign(inputXSpeed) != Mathf.Sign(velocity.x)) //don't slide if switching directions on same frame
		{
			velocity.x = 0;
			shouldStand = true;
		}
		else
		{
			velocity.x += RUN_ACCEL * Time.fixedDeltaTime * inputXSpeed;
			float speedCap = Mathf.Abs(inputXSpeed * MAX_RUN_SPEED); //use input to clamp max speed so half tilted joystick is slower
			velocity.x = Mathf.Clamp(velocity.x, -speedCap, speedCap);
			player.SetAnimState(AnimState.RUN);
		}

		//received horizontal input, so don't let player get pushed by natural walljump velocity
		walljumpPush = false;

		if (!IsRolling())
		{
			rollDir = Math.Sign(inputXSpeed);
		}
	}

	private void StayOnGround(ref Vector2 velocity, ref Vector2 offset)
	{
		velocity.y = 0; //TODO: remove?

		//apply moving platform velocity
		Rigidbody2D platformRB = GetGroundRigidbody();
		if (platformRB != null)
		{
			offset += platformRB.velocity * Time.fixedDeltaTime;
		}

		/*if (groundAngle >= SLIDE_THRESHOLD)
        {
            //slide
            velocity.y += GRAVITY_ACCEL; //* slope (perp. to ground angle), * friction?
        }
        */
		ResetWalljump();

		if (!IsRolling() && !canRoll)
		{
			SetCanRoll();
		}
	}

	private void Fall(ref Vector2 velocity)
	{
		float gravAccel = GRAVITY_ACCEL;
		float maxFall = MAX_FALL_SPEED;
		if (velocity.y < 0 && walls.Count > 0)
		{
			//slide down wall more slowly
			gravAccel = SLIDE_GRAVITY_ACCEL;
			maxFall = SLIDE_MAX_FALL_SPEED;
			//TODO: slide sound/particles?
		}

		velocity.y += gravAccel * Time.fixedDeltaTime;
		velocity.y = Mathf.Max(velocity.y, maxFall); //max since they're negative
	}

	private void ApplyWalljumpPush(ref Vector2 velocity)
	{
		float timeFactor = walljumpTime / WALLJUMP_TIME;
		if (walljumpPush)
		{
			timeFactor = Mathf.Max(timeFactor, WALLJUMP_MIN_FACTOR);
		}
		float walljumpVel = WALLJUMP_SPEED * lastWallSide * timeFactor;

		velocity.x += walljumpVel;
		velocity.x = Mathf.Clamp(velocity.x, -MAX_RUN_SPEED, MAX_RUN_SPEED);

		if (walljumpTime > 0)
		{
			walljumpTime -= Time.fixedDeltaTime;
		}
	}

	private void ApplyRoll(bool onGround, ref Vector2 velocity, ref Vector2 offset)
	{
		float timeFactor = rollTime / ROLL_TIME;
		float rollVel = rollDir * ROLL_SPEED * timeFactor;

		/*bool shouldStop = false;
		if (Mathf.Abs(rollVel) < Mathf.Abs(velocity.x))
		{
			//rolling would be slower than running
			shouldStop = true;
		}*/

		//roll in direction of ground
		Vector2 groundVec;
		if (onGround)
		{
			groundVec = Vector3.Cross(groundNormal, Vector3.forward); //left handed
			groundVec.Normalize();
		}
		else
		{
			groundVec = Vector2.right;
		}
		Vector2 rollVec = rollVel * groundVec;
		velocity.x += rollVec.x;
		float speedCapX = Mathf.Abs(rollVec.x);
		velocity.x = Mathf.Clamp(velocity.x, -speedCapX, speedCapX);

		offset.y += rollVec.y * Time.fixedDeltaTime; //do this with offset so it doesn't persist when rolling up

		//roll for longer on slope
		if (rollVec.y < 0)
		{
			float maxAddition = ROLL_MAX_ADDITION * Time.fixedDeltaTime;
			float groundAngle = Vector2.Dot(groundNormal.normalized, Vector2.right * rollDir);
			float rollTimeAddition = groundAngle * maxAddition;
			rollTime = Mathf.Min(rollTime + rollTimeAddition, ROLL_TIME);
		}

		rollTime -= Time.fixedDeltaTime;

		walljumpPush = false; //don't keep pushing after roll finishes

		if (!IsRolling()) // || shouldStop)
		{
			StopRoll();
		}
	}

	private void Jump(PlayerInput input, ref Vector2 velocity)
	{
		input.jumpQueued = false;
		player.TryStopCoroutine(input.crtCancelQueuedJump);
		canJump = false;
		canMidairJump = false;
		canJumpRelease = true;
		StopRoll();
		if (!IsRolling()) //don't jump if forced roll
		{
			velocity.y = JUMP_SPEED;
			//add moving platform vertical velocity
			Rigidbody2D groundRB = GetGroundRigidbody();
			if (groundRB != null)
			{
				velocity.y += groundRB.velocity.y;
			}

			player.PlayJumpSound();
		}
	}

	private void WallJump(PlayerInput input, ref Vector2 velocity)
	{
		walljumpTime = WALLJUMP_TIME;
		lastWallSide = wallSide;
		wallSide = 0; //so player can't walljump any more
		velocity.y = JUMP_SPEED;
		walljumpPush = true;
		input.jumpQueued = false;
		canJumpRelease = true;
		input.StopCancelQueuedJump(player);

		player.PlayJumpSound();
		//SkidSound.Stop();
	}

	private void ResetWalljump()
	{
		walljumpPush = false;
		walljumpTime = 0;

		//SkidSound.Stop();
	}

	private void JumpRelease(ref Vector2 velocity)
	{
		if (canJumpRelease && velocity.y > 0)
		{
			velocity.y = velocity.y * JUMP_RELEASE_FACTOR;
		}
		canJumpRelease = false;
	}

	private void Roll(ref Vector2 velocity)
	{
		canRoll = false;
		rollTime = ROLL_TIME;
		SetRollCollider();
		ResetWalljump();
		velocity.y = 0;
		player.PlayRollSound();
	}

	private bool IsRolling()
	{
		return rollTime > 0;
	}

	private bool IsWalljumping()
	{
		return walljumpPush || walljumpTime > 0;
	}

	private void StopRoll()
	{
		//if it fits, otherwise keep anim state and rolling
		if (!rollingCollider) return;

		rollingCollider = false;
		float ecBottom = ec.points[0].y;
		float normalTop = ecBottom + normalHeight;
		SetColliderHeight(normalTop);

		RaycastHit2D[] hits = BoxCast(Vector2.zero, 0);
		if (hits.Length > 0) //collided with something when trying to unroll- force roll to continue
		{
			canRoll = false;
			rollTime = Mathf.Max(rollTime + Time.fixedDeltaTime, ROLL_FORCE_AMOUNT);
			SetRollCollider();
		}
		else
		{
			rollTime = 0;
		}
	}
	
	private RaycastHit2D[] BoxCast(Vector2 direction, float distance)
	{
		Vector2 size = ec.points[2] - ec.points[0];
		return Physics2D.BoxCastAll(rb.position, size, 0, direction, distance, LayerMask.GetMask("LevelGeometry"));
	}

	public void SetRollCollider()
	{
		rollingCollider = true;
		float ecBottom = ec.points[0].y;
		float rollTop = ecBottom + normalHeight * ROLL_HEIGHT;
		SetColliderHeight(rollTop);
	}

	private void SetColliderHeight(float height)
	{
		Vector2[] points = ec.points;
		points[1].y = height;
		points[2].y = height;
		ec.points = points;
		player.RefreshHurtZone();
	}

	private Rigidbody2D GetWallRigidbody()
	{
		return GetRigidbody(walls);
	}

	private Rigidbody2D GetGroundRigidbody()
	{
		return GetRigidbody(grounds);
	}

	private Rigidbody2D GetRigidbody(List<GameObject> platforms)
	{
		int count = platforms.Count;
		if (count == 0) return null;

		//TODO: use last where the player is more than half on it
		return platforms[count - 1].GetComponent<Rigidbody2D>();
	}


	private void HitGround()
	{
		canJump = true;
		canMidairJump = false;
	}

	private void HitCeiling()
	{
		Vector2 velocity = rb.velocity;
		velocity.y = Mathf.Min(velocity.y, 0);
		rb.velocity = velocity;
	}

	private void HitWall(int newWallSide)
	{
		wallSide = newWallSide;
		lastWallSide = newWallSide;

		player.TryStopCoroutine(crtLeaveWall);
		ResetWalljump();
		StopRoll();
		//if still rolling: bounce (stuck under ledge)
		if (IsRolling())
		{
			rollDir *= -1;
		}

		//SkidSound.PlayScheduled(0.1);
	}

	private int GetWallSide(ContactPoint2D wallPoint)
	{
		float x = Vector2.Dot(Vector2.right, wallPoint.normal);
		return Mathf.RoundToInt(x);
	}

	private bool IsOneWayPlatform(Collision2D collision)
	{
		return collision.gameObject.GetComponent<PlatformEffector2D>() != null;
	}

	private bool IsFeet(Vector2 point, Vector2 vel)
	{
		float feetY = ec.points[0].y + ec.offset.y + player.transform.position.y;
		return Mathf.Abs(point.y - feetY) < FEET_CHECK + Mathf.Abs(vel.y * Time.fixedDeltaTime);
	}

	private IEnumerator TempDisableGround(Collider2D col)
	{
		col.enabled = false;
		yield return new WaitForSeconds(0.2f); //seems to give best results
		col.enabled = true;
	}

	private float NormalDot(ContactPoint2D contact)
	{
		return Vector2.Dot(contact.normal, GRAVITY_NORMAL);
	}

	private ContactPoint2D? GetWithDotCheck(Collision2D collision, Func<float, bool> dotCheck)
	{
		if (collision == null) return null;

		int len = collision.contacts.Length;
		for (int i = 0; i < collision.contacts.Length; i++)
		{
			float dot = NormalDot(collision.contacts[i]);
			if (dotCheck(dot))
			{
				return collision.contacts[i];
			}
		}
		return null;
	}

	private ContactPoint2D? GetGround(Collision2D collision)
	{
		return GetWithDotCheck(collision, d => d < 0);
	}

	private ContactPoint2D? GetCeiling(Collision2D collision)
	{
		return GetWithDotCheck(collision, d => d > 0.1);
	}

	private ContactPoint2D? GetWall(Collision2D collision)
	{
		return GetWithDotCheck(collision, d => Mathf.Abs(d) < 0.01f);
	}

	private Coroutine RemoveContact(List<GameObject> contacts, GameObject contact, IEnumerator ifEmpty)
	{
		bool removed = contacts.Remove(contact);
		if (removed && contacts.Count == 0)
		{
			return player.StartCoroutine(ifEmpty);
		}
		return null;
	}

	private IEnumerator LeaveGround()
	{
		yield return new WaitForSeconds(JUMP_GRACE_TIME);
		canJump = false;
	}

	private IEnumerator LeaveWall()
	{
		yield return new WaitForSeconds(WALLJUMP_GRACE_TIME);
		wallSide = 0;
	}

	public override void SetCanRoll()
	{
		base.SetCanRoll();
		canRoll = true;
	}

	public override void SetCanMidairJump()
	{
		base.SetCanMidairJump();
		canMidairJump = true;
	}

	/*public void Shove()
	{
		if (grounds.Count > 0)
		{
			//apply some velocity- use walljump code
			walljumpTime = WALLJUMP_TIME;
			lastWallSide = Random.Range(0f, 1f) < 0.5f ? -1 : 1;
			Vector2 velocity = rb.velocity;
			velocity.y = JUMP_SPEED / 2;
			rb.velocity = velocity;
			walljumpPush = true;
			audioSrc.PlayOneShot(deathSound);
		}
	}*/

	public override void CollisionEnter(Collision2D collision)
	{
		base.CollisionEnter(collision);

		GameObject other = collision.gameObject;

		Slime slime = other.GetComponent<Slime>();
		if (slime != null)
		{
			ContactPoint2D? groundPoint = GetGround(collision);
			if (groundPoint.HasValue)
			{
				float bounce = Mathf.Max(slime.bounceSpeed, -rb.velocity.y);
				rb.velocity = new Vector2(rb.velocity.x, bounce);
			}
			ContactPoint2D? ceilingPoint = GetCeiling(collision);
			if (ceilingPoint.HasValue)
			{
				float bounce = Mathf.Min(-slime.bounceSpeed, -rb.velocity.y);
				rb.velocity = new Vector2(rb.velocity.x, bounce);
			}
			ContactPoint2D? wallPoint = GetWall(collision);
			if (wallPoint.HasValue)
			{
				//apply some velocity- use walljump code
				lastWallSide = GetWallSide(wallPoint.Value);
				wallSide = 0; //don't allow further walljump
				walls.Add(other); //so we don't hit the wall in collision stay
				walljumpTime = WALLJUMP_TIME;
				Vector2 velocity = rb.velocity;
				velocity.y = slime.bounceSpeed;
				rb.velocity = velocity;
				walljumpPush = true;

				if (IsRolling())
				{
					rollDir *= -1;
				}
			}

			//prevent jump-release-braking the slime bounce
			canJumpRelease = false;

			player.PlayBounceSound();
		}
	}

	public override void CollisionStay(Collision2D collision)
	{
		base.CollisionStay(collision);

		if (collision.contacts.Length == 0) return; //not sure what happened

		if (collision.gameObject.layer != LayerMask.NameToLayer("LevelGeometry"))
		{
			return;
		}

		ContactPoint2D? groundPoint = GetGround(collision);
		if (groundPoint.HasValue)
		{
			Rigidbody2D groundRB = collision.gameObject.GetComponent<Rigidbody2D>();
			Vector2 groundVel = groundRB == null ? Vector2.zero : groundRB.velocity;
			if (IsFeet(groundPoint.Value.point, groundVel) || !IsOneWayPlatform(collision))
			{
				if (!grounds.Contains(collision.gameObject))
				{
					grounds.Add(collision.gameObject);
				}
				groundNormal = groundPoint.Value.normal;
				HitGround();
			}
			else if (rb.velocity.y < 0)
			{
				//don't collide with one-way platform above feet while moving down
				player.StartCoroutine(TempDisableGround(collision.collider));
			}
		}
		else
		{
			RemoveContact(grounds, collision.gameObject, LeaveGround());
		}

		ContactPoint2D? ceilingPoint = GetCeiling(collision);
		if (ceilingPoint.HasValue && !IsOneWayPlatform(collision))
		{
			HitCeiling();
		}

		ContactPoint2D? wallPoint = GetWall(collision);
		if (wallPoint.HasValue && !IsOneWayPlatform(collision))
		{
			if (!walls.Contains(collision.gameObject))
			{
				walls.Add(collision.gameObject);

				int newWallSide = GetWallSide(wallPoint.Value);
				HitWall(newWallSide);
			}
		}
		else
		{
			crtLeaveWall = RemoveContact(walls, collision.gameObject, LeaveWall());
		}
	}

	public override void CollisionExit(Collision2D collision)
	{
		base.CollisionExit(collision);

		if (collision.gameObject.layer != LayerMask.NameToLayer("LevelGeometry"))
		{
			return;
		}

		RemoveContact(grounds, collision.gameObject, LeaveGround());
		crtLeaveWall = RemoveContact(walls, collision.gameObject, LeaveWall());
	}
}
