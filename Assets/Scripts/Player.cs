﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

enum AnimState
{
	STAND,
	JUMP,
	WALLSLIDE,
	RUN,
	ROLL
}

public class Player : MonoBehaviour
{

	public GameObject spriteObject;
	public GameObject hurtZone;
	public GameObject grabZone;
	public GameObject holdSpot;

	public AudioClip jumpSound;
	public AudioClip rollSound;
	public AudioClip skidSound;
	public AudioClip bounceSound;
	public AudioClip starCollectSound;
	public AudioClip oldStarCollectSound;
	public AudioClip deathSound;

	private const float RUN_ACCEL = 0.5f; //acceleration of horizontal movement
	private const float MAX_RUN_SPEED = 9.0f; //maximum speed of horizontal movement

	private const float GRAVITY_ACCEL = -0.6f; //acceleration of gravity
	private const float MAX_FALL_SPEED = -50.0f; //maximum speed of fall
	private const float SLIDE_GRAVITY_ACCEL = -0.3f; //acceleration of gravity when sliding down wall
	private const float SLIDE_MAX_FALL_SPEED = -8.0f; //maximum speed of sliding down wall
	private const float SNAP_DIST = 0.5f;

	private const float JUMP_SPEED = 16.0f; //jump y speed
	private const float JUMP_RELEASE_FACTOR = 0.5f; //factor to reduce jump by when releasing
	private const float JUMP_GRACE_TIME = 0.1f; //time after leaving ground player can still jump
	private const float JUMP_BUFFER_TIME = 0.1f; //time before hitting ground a jump will still be queued

	private const float WALLJUMP_SPEED = MAX_RUN_SPEED; //speed applied at time of walljump
	private const float WALLJUMP_MIN_FACTOR = 0.75f; //amount of walljump kept at minimum if no input
	private const float WALLJUMP_TIME = 0.575f; //time it takes for walljump to wear off
	private const float WALLJUMP_GRACE_TIME = 0.2f; //time after leaving wall player can still walljump

	private const float ROLL_SPEED = 2 * MAX_RUN_SPEED; //speed of roll
	private const float ROLL_TIME = 1.0f; //time it takes for roll to wear off naturally
	private const float MAX_ROLL_TIME = 2.0f; //time it takes for roll to wear off at the bottom of a long slope
	private const float ROLL_MAX_ADDITION = 5.0f; //amount of roll added on high slopes
	private const float ROLL_HEIGHT = 0.5f; //scale factor of height when rolling
	private const float ROLL_FORCE_AMOUNT = 0.1f; //how much to push the player when they can't unroll
	private const float ROLL_RELEASE_FACTOR = 0.5f; //factor to reduce roll by when releasing

	private const float THROW_X_FACTOR = 75.0f; //velocity x multiplier for throwing a grabbable object
	private const float THROW_Y_SPEED = 100.0f; //velocity y addition for throwing a grabbable object

	//private static float SLIDE_THRESHOLD;
	private static Vector2 GRAVITY_NORMAL = new Vector2(0, GRAVITY_ACCEL).normalized;

	private const float HURTZONE_SIZE_FACTOR = 0.7f; //size of squish collider as factor of normal collider (should be between 0 and 1)

	private const float FEET_CHECK = 0.01f; //used in checking ground collision validity

	private const int NUM_RUN_FRAMES = 20;
	private const int NUM_ROLL_FRAMES = 4;

	private const float RUN_FRAME_TIME = 0.02f; //time in seconds per frame of run animation
	private const float ROLL_FRAME_TIME = 0.1f; //time in seconds per frame of roll animation

	private const float PITCH_VARIATION = 0.1f;

	private GameManager gm;
	private EdgeCollider2D ec;
	private Rigidbody2D rb;
	private AudioSource audioSrc;

	private Vector2 groundNormal;

	private bool jumpQueued = false;
	private bool jumpReleaseQueued = false;
	private bool canJump = false; //used instead of onGround to allow grace time after leaving platform
	private bool canJumpRelease = false; //to only allow the first jump release to slow the jump
	private List<GameObject> grounds = new List<GameObject>();
	private List<GameObject> walls = new List<GameObject>();
	private int wallSide = 0; //1 for wall on left, 0 for none, -1 for wall on right (i.e. points away from wall in x)- this is non-zero = the player can walljump
	private int lastWallSide = 0;
	private float walljumpTime = 0; //counts down from WALLJUMP_TIME
	private bool walljumpPush = false; //if the player hasn't touched anything and the walljump should keep moving them
	private bool canMidairJump = false;

	private bool rollQueued = false;
	private bool rollReleaseQueued = false;
	private bool triggerWasHeld = false;
	private float rollTime = 0;
	private bool canRoll = true;
	private int rollDir = 1; //-1 for left, 1 for right
	private float normalHeight;
	private bool rollingCollider = false;

	private bool grabQueued = false;
	private GrabZone gz;
	private Grabbable heldObject;

	private BoxCollider2D hurtCollider;

	private Vector2 respawnPos;

	private Coroutine crtLeaveWall;
	private Coroutine crtCancelQueuedJump;

	private SpriteRenderer sr;
	private AnimState animState = AnimState.STAND;
	private int animFrame = 0;
	private float frameTime; //max time of frame
	private float frameTimer; //goes from frameTime down to 0
	private bool facingLeft = false; //for animation (images face left)
	private bool shouldStand = false;

	private Sprite standSprite;
	private Sprite jumpSprite;
	private Sprite wallslideSprite;
	private Sprite[] runSprites;
	private Sprite[] rollSprites;

	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		rb = GetComponent<Rigidbody2D>();
		audioSrc = GetComponent<AudioSource>();
		ec = GetComponent<EdgeCollider2D>();
		normalHeight = ec.points[1].y - ec.points[0].y;
		hurtCollider = hurtZone.GetComponent<BoxCollider2D>();
		RefreshHurtZone();
		gz = grabZone.GetComponent<GrabZone>();

		//SLIDE_THRESHOLD = -Mathf.Sqrt(2) / 2; //player will slide down 45 degree angle slopes

		respawnPos = transform.position;

		sr = spriteObject.GetComponent<SpriteRenderer>();
		LoadSprites();
	}

	private void LoadSprites()
	{
		standSprite = LoadSprite("stand");
		jumpSprite = LoadSprite("jump");
		wallslideSprite = LoadSprite("wallslide");

		runSprites = new Sprite[NUM_RUN_FRAMES];
		LoadAnim("run", runSprites, NUM_RUN_FRAMES);

		rollSprites = new Sprite[NUM_ROLL_FRAMES];
		LoadAnim("roll", rollSprites, NUM_ROLL_FRAMES);
	}

	private Sprite LoadSprite(string name)
	{
		return Resources.Load<Sprite>("Images/player/" + name);
	}

	private void LoadAnim(string name, Sprite[] sprites, int numFrames)
	{
		for (int i = 0; i < numFrames; i++)
		{
			sprites[i] = LoadSprite(name + "/frame" + (i + 1));
		}
	}

	private void Update()
	{
		//Get input here and queue it up to be processed by FixedUpdate- can't get in FixedUpdate since it may miss inputs

		//if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7)) //Start
		if (Input.GetButtonDown("Pause"))
		{
			gm.TogglePauseMenu();
		}

		//if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) //A
		if (Input.GetButtonDown("Jump"))
		{
			TryStopCoroutine(crtCancelQueuedJump);
			jumpQueued = true;
			crtCancelQueuedJump = StartCoroutine(CancelQueuedJump());
		}

		//if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0))
		if (Input.GetButtonUp("Jump"))
		{
			jumpReleaseQueued = true;
		}

		bool triggerHeld = Input.GetAxis("LTrigger") > 0 || Input.GetAxis("RTrigger") > 0;
		bool triggerPressed = !triggerWasHeld && triggerHeld;
		//bool shiftPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
		if (triggerPressed)
		{
			rollQueued = true;
		}

		bool triggerReleased = triggerWasHeld && !triggerHeld;
		//bool shiftReleased = Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift);
		if (triggerReleased) //shiftReleased
		{
			rollReleaseQueued = true;
		}
		triggerWasHeld = triggerHeld;

		//bool ctrlPressed = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
		//bool actionPressed = Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton2); //B, X
		//if (ctrlPressed || actionPressed)
		if (Input.GetButtonDown("ActionL") || Input.GetButtonDown("ActionR"))
		{
			grabQueued = true;
		}

		AdvanceAnim();
		sr.sprite = GetAnimSprite();
	}

	private Sprite GetAnimSprite()
	{
		switch (animState)
		{
			case AnimState.STAND:
				return standSprite;
			case AnimState.JUMP:
				return jumpSprite;
			case AnimState.WALLSLIDE:
				return wallslideSprite;
			case AnimState.RUN:
				return runSprites[animFrame];
			case AnimState.ROLL:
				return rollSprites[animFrame];
		}
		return standSprite;
	}

	private void FixedUpdate()
	{
		//Process movement- running, walljumping, jumping, rolling, etc.

		Vector2 velocity = rb.velocity; //for changing the player's actual velocity
		Vector2 offset = Vector2.zero; //for any additional movement nudges
		shouldStand = false;
		float inputXSpeed = Input.GetAxisRaw("Horizontal");
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
			if (jumpReleaseQueued)
			{
				JumpRelease(ref velocity);
			}

			if (!onGround && jumpQueued && wallSide != 0 && !IsRolling())
			{
				WallJump(ref velocity);
			}

			Fall(ref velocity);
		}

		if (!jumpQueued) //in case the player is about to jump, in which case their jump should be cut short
		{
			jumpReleaseQueued = false;
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

		if (jumpQueued && ((canJump && velocity.y <= 0) || canMidairJump))
		{
			Jump(ref velocity);
		}

		if (grabQueued)
		{
			if (heldObject != null)
			{
				ThrowHeldObject(velocity);
			}
			else if (onGround && !IsRolling())
			{
				GrabObject();
			}
			grabQueued = false;
		}

		if (IsWalljumping())
		{
			ApplyWalljumpPush(ref velocity);
		}

		if (velocity.y != 0 && !IsRolling())
		{
			if (wallSide != 0 && velocity.y < 0 && Math.Sign(velocity.x) != wallSide)
			{
				SetAnimState(AnimState.WALLSLIDE);
			}
			else
			{
				SetAnimState(AnimState.JUMP);
			}
		}

		if (rollQueued && canRoll && heldObject == null)
		{
			Roll(ref velocity);
		}
		rollQueued = false;

		if (IsRolling())
		{
			if (rollReleaseQueued)
			{
				//slow your roll
				rollTime *= ROLL_RELEASE_FACTOR;
			}

			ApplyRoll(onGround, ref velocity, ref offset);

			if (IsRolling())
			{
				SetAnimState(AnimState.ROLL);
			}
		}
		rollReleaseQueued = false;

		if (shouldStand)
		{
			SetAnimState(AnimState.STAND);
		}

		if (velocity.x != 0)
		{
			facingLeft = velocity.x < 0;
			if (inputXSpeed == 0)
			{
				rollDir = Math.Sign(velocity.x);
			}
		}

		bool left;
		if (animState == AnimState.WALLSLIDE)
		{
			left = wallSide < 0;
		}
		else
		{
			left = facingLeft;
		}

		sr.flipX = left; //change this if sprites face left
		int xSign = left ? 1 : -1; //change this if sprites face left
		Vector3 spritePos = sr.transform.localPosition;
		spritePos.x = Mathf.Abs(spritePos.x) * xSign;
		sr.transform.localPosition = spritePos;

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
			velocity.x += RUN_ACCEL * inputXSpeed;
			float speedCap = Mathf.Abs(inputXSpeed * MAX_RUN_SPEED); //use input to clamp max speed so half tilted joystick is slower
			velocity.x = Mathf.Clamp(velocity.x, -speedCap, speedCap);
			SetAnimState(AnimState.RUN);
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

		velocity.y += gravAccel;
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

	private void Jump(ref Vector2 velocity)
	{
		jumpQueued = false;
		TryStopCoroutine(crtCancelQueuedJump);
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

			PlayJumpSound();
		}
	}

	private void WallJump(ref Vector2 velocity)
	{
		walljumpTime = WALLJUMP_TIME;
		lastWallSide = wallSide;
		wallSide = 0; //so player can't walljump any more
		velocity.y = JUMP_SPEED;
		walljumpPush = true;
		jumpQueued = false;
		canJumpRelease = true;
		TryStopCoroutine(crtCancelQueuedJump);

		PlayJumpSound();
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

		audioSrc.PlayOneShot(rollSound);
	}

	public void SetCanRoll()
	{
		canRoll = true;
		//TODO: fancy effects
	}

	public void SetCanMidairJump()
	{
		canMidairJump = true;
		//TODO: fancy glowy effects
	}

	private IEnumerator CancelQueuedJump()
	{
		yield return new WaitForSeconds(JUMP_BUFFER_TIME);
		jumpQueued = false;
	}

	private bool IsRolling()
	{
		return rollTime > 0;
	}

	private bool IsWalljumping()
	{
		return walljumpPush || walljumpTime > 0;
	}

	private void ThrowHeldObject(Vector2 velocity)
	{
		float dropX = grabZone.transform.position.x;
		float side = Mathf.Sign(grabZone.transform.localPosition.x);
		dropX += side * heldObject.GetSize().x / 2;
		Vector2 dropPos = new Vector2(dropX, holdSpot.transform.position.y);
		if (CanMoveGrabbable(heldObject, dropPos))
		{
			heldObject.transform.parent = null;
			heldObject.Drop(dropPos);
			if (velocity.x != 0)
			{
				Vector2 force = new Vector2(velocity.x * THROW_X_FACTOR, velocity.y + THROW_Y_SPEED);
				heldObject.AddForce(force);
			}
			heldObject = null;
		}	
	}

	private void GrabObject()
	{
		Grabbable toGrab = gz.GetGrabbable();
		if (toGrab != null)
		{
			Vector3 holdPos = holdSpot.transform.position;
			float headY = ec.points[1].y + ec.offset.y + transform.position.y;
			holdPos.y = headY + toGrab.GetSize().y / 2;
			holdSpot.transform.position = holdPos;
			if (CanMoveGrabbable(toGrab, holdPos))
			{
				gz.RemoveGrabbable(toGrab);
				heldObject = toGrab;
				toGrab.transform.parent = holdSpot.transform;
				toGrab.PickUp(holdPos);
				//TODO: set animstate to carrying? can just check if heldObject != null
			}
		}
	}

	private bool CanMoveGrabbable(Grabbable g, Vector2 pos)
	{
		Vector2 size = g.GetSize();
		RaycastHit2D[] hits = Physics2D.BoxCastAll(pos, size, 0, Vector2.zero, 0, LayerMask.GetMask("LevelGeometry"));
		return hits.Length == 0 || (hits.Length == 1 && hits[0].collider.gameObject == g.gameObject);
	}

	private void ResetMovement()
	{
		//TODO: make sure this sets all relevant properties

		jumpQueued = false;
		jumpReleaseQueued = false;
		canJump = false;
		canJumpRelease = false;
		grounds.Clear();
		walls.Clear();
		wallSide = 0;
		lastWallSide = 0;
		ResetWalljump();

		StopRoll();
		rollQueued = false;
		rollReleaseQueued = false;

		grabQueued = false;
		//TODO release heldObject? or nah

		canMidairJump = false;

		rb.velocity = Vector3.zero;
	}

	public void Kill()
	{
		ResetMovement();

		rb.position = respawnPos;
		audioSrc.PlayOneShot(deathSound);
	}

	private void SetRollCollider()
	{
		rollingCollider = true;
		float ecBottom = ec.points[0].y;
		float rollTop = ecBottom + normalHeight * ROLL_HEIGHT;
		SetColliderHeight(rollTop);
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

	private void SetColliderHeight(float height)
	{
		Vector2[] points = ec.points;
		points[1].y = height;
		points[2].y = height;
		ec.points = points;
		RefreshHurtZone();
	}

	private void RefreshHurtZone()
	{
		Vector2[] points = ec.points;
		float left = points[1].x;
		float right = points[2].x;
		float bottom = points[0].y;
		float top = points[1].y;
		float ecWidth = right - left;
		float ecHeight = top - bottom;
		float offsetY = (bottom + top) / 2;
		float width = ecWidth * HURTZONE_SIZE_FACTOR;
		float height = ecHeight * HURTZONE_SIZE_FACTOR;
		Vector2 size = new Vector2(width, height);
		hurtCollider.offset = new Vector2(0, offsetY);
		hurtCollider.size = size;
	}

	private RaycastHit2D[] BoxCast(Vector2 direction, float distance)
	{
		Vector2 size = ec.points[2] - ec.points[0];
		return Physics2D.BoxCastAll(rb.position, size, 0, direction, distance, LayerMask.GetMask("LevelGeometry"));
	}

	private void ResetWalljump()
	{
		walljumpPush = false;
		walljumpTime = 0;

		//SkidSound.Stop();
	}

	private void SetAnimState(AnimState state)
	{
		animState = state;

		if (state != AnimState.STAND)
		{
			shouldStand = false;
		}
	}

	private void AdvanceAnim()
	{
		if (animState == AnimState.RUN)
		{
			frameTime = RUN_FRAME_TIME;
			AdvanceFrame(NUM_RUN_FRAMES);
		}
		else if (animState == AnimState.ROLL)
		{
			frameTime = ROLL_FRAME_TIME;
			AdvanceFrame(NUM_ROLL_FRAMES);
		}
		else
		{
			animFrame = 0;
			frameTimer = frameTime;
		}
	}

	private void AdvanceFrame(int numFrames)
	{
		if (animFrame >= numFrames)
		{
			animFrame = 0;
		}

		frameTimer -= Time.deltaTime;
		if (frameTimer <= 0)
		{
			frameTimer = frameTime;
			animFrame = (animFrame + 1) % numFrames;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
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

			audioSrc.PlayOneShot(bounceSound);
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
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
				StartCoroutine(TempDisableGround(collision.collider));
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

		TryStopCoroutine(crtLeaveWall);
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
		float feetY = ec.points[0].y + ec.offset.y + transform.position.y;
		return Mathf.Abs(point.y - feetY) < FEET_CHECK + Mathf.Abs(vel.y * Time.fixedDeltaTime);
	}

	private IEnumerator TempDisableGround(Collider2D col)
	{
		col.enabled = false;
		yield return new WaitForSeconds(0.2f); //seems to give best results
		col.enabled = true;
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.layer != LayerMask.NameToLayer("LevelGeometry"))
		{
			return;
		}

		RemoveContact(grounds, collision.gameObject, LeaveGround());
		crtLeaveWall = RemoveContact(walls, collision.gameObject, LeaveWall());
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
			return StartCoroutine(ifEmpty);
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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		GameObject other = collision.gameObject;

		if (collision.CompareTag("Checkpoint"))
		{
			respawnPos = other.transform.position;
		}

		Star star = other.GetComponent<Star>();
		if (star != null)
		{
			bool newStar = gm.CollectStar(star);
			Destroy(other);
			if (newStar)
			{
				audioSrc.PlayOneShot(starCollectSound);
			}
			else
			{
				audioSrc.PlayOneShot(oldStarCollectSound);
			}
		}

		Door door = other.GetComponent<Door>();
		if (door != null)
		{
			gm.ShowHUDDoorStars(door);
			door.TryOpen();
		}

		PowerUp powerUp = other.GetComponent<PowerUp>();
		if (powerUp != null)
		{
			powerUp.Activate(this);
		}

		/*Portal portalCollided = other.GetComponent<Portal>();
		if (portalCollided != null)
		{
			int starType = (int)gm.levelStarPrefab.GetComponent<Star>().starType;
			if (gm.starsCollected[starType] >= portalCollided.starsRequired)
			{
				NextLevel();
			}
		}

		StarSpot starSpot = other.GetComponent<StarSpot>();
		if (starSpot != null && !starSpot.touched)
		{
			starSpot.Fill();
		}*/
	}

	private void PlayJumpSound()
	{
		//audioSrc.pitch = UnityEngine.Random.Range(2 - PITCH_VARIATION, 2 + PITCH_VARIATION);
		audioSrc.PlayOneShot(jumpSound);
	}

	private void TryStopCoroutine(Coroutine crt)
	{
		if (crt != null)
		{
			StopCoroutine(crt);
		}
	}

	public void Shove()
	{
		if (grounds.Count > 0)
		{
			//apply some velocity- use walljump code
			walljumpTime = WALLJUMP_TIME;
			lastWallSide = UnityEngine.Random.Range(0f, 1f) < 0.5f ? -1 : 1;
			Vector2 velocity = rb.velocity;
			velocity.y = JUMP_SPEED / 2;
			rb.velocity = velocity;
			walljumpPush = true;
			audioSrc.PlayOneShot(deathSound);
		}
	}
}
