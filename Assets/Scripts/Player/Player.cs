using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimState
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

	public GameObject rollTrailPrefab;

	private const float THROW_X_FACTOR = 75.0f; //velocity x multiplier for throwing a grabbable object
	private const float THROW_Y_SPEED = 100.0f; //velocity y addition for throwing a grabbable object

	private const float HURTZONE_WIDTH_FACTOR = 0.7f; //width of squish collider as factor of normal collider (should be between 0 and 1)
	private const float HURTZONE_HEIGHT_FACTOR = 0.5f; //height of squish collider as factor of normal collider (should be between 0 and 1)

	private const int NUM_RUN_FRAMES = 20;
	private const int NUM_ROLL_FRAMES = 4;

	private const float RUN_FRAME_TIME = 0.02f; //time in seconds per frame of run animation
	private const float ROLL_FRAME_TIME = 0.1f; //time in seconds per frame of roll animation

	private const float TRAIL_BETWEEN_TIME = 0.05f; //time in seconds between roll trail echoes

	//private const float PITCH_VARIATION = 0.1f;

	[HideInInspector]
	public EdgeCollider2D ec;
	[HideInInspector]
	public Rigidbody2D rb;
	private GameManager gm;
	private AudioSource audioSrc;

	protected PlayerInput input;
	protected PlayerMovement movement;

	private GrabZone gz;
	[HideInInspector]
	public Grabbable heldObject;

	private BoxCollider2D hurtCollider;

	private Vector2 respawnPos;

	[HideInInspector]
	public SpriteRenderer sr;
	[HideInInspector]
	public AnimState animState = AnimState.STAND;
	private int animFrame = 0;
	private float frameTime; //max time of frame
	private float frameTimer; //goes from frameTime down to 0
	[HideInInspector]
	public bool facingLeft = false; //for animation (images face left)
	private float trailTime;

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
		hurtCollider = hurtZone.GetComponent<BoxCollider2D>();
		RefreshHurtZone();
		gz = grabZone.GetComponent<GrabZone>();
		input = new PlayerInput();
		movement = new PlayerMove(this);

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
		input.Update(this);

		CheckRollTrail();

		//TODO replace this with unity animator stuff
		AdvanceAnim();
		sr.sprite = GetAnimSprite();
	}

	private void CheckRollTrail()
	{
		if (movement.IsRolling())
		{
			trailTime += Time.deltaTime;
			if (trailTime > TRAIL_BETWEEN_TIME)
			{
				trailTime = 0;
				SpawnRollTrail();
			}
		}
		else
		{
			trailTime = 0;
		}
	}

	private void SpawnRollTrail()
	{
		GameObject rollTrail = Instantiate(rollTrailPrefab, spriteObject.transform.position, spriteObject.transform.rotation);
		rollTrail.transform.localScale = spriteObject.transform.lossyScale;
		SpriteRenderer rtSR = rollTrail.GetComponent<SpriteRenderer>();
		rtSR.sprite = sr.sprite;
		rtSR.flipX = sr.flipX;
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
		movement.Move(input);
	}

	public void SetCanRoll()
	{
		movement.SetCanRoll();
		//TODO: fancy effects
	}

	public void SetCanMidairJump()
	{
		movement.SetCanMidairJump();
		//TODO: fancy glowy effects
	}

	private void ResetMovement()
	{
		input = new PlayerInput();
		movement = new PlayerMove(this);

		rb.velocity = Vector3.zero;
	}

	public void Kill()
	{
		ResetMovement();

		rb.position = respawnPos;
		audioSrc.PlayOneShot(deathSound);
		//TODO: subtract health if not invincible
	}

	//Should be called after changing the size of the player's collider
	public void RefreshHurtZone()
	{
		Vector2[] points = ec.points;
		float left = points[1].x;
		float right = points[2].x;
		float bottom = points[0].y;
		float top = points[1].y;
		float ecWidth = right - left;
		float ecHeight = top - bottom;
		float offsetY = (bottom + top) / 2;
		float width = ecWidth * HURTZONE_WIDTH_FACTOR;
		float height = ecHeight * HURTZONE_HEIGHT_FACTOR;
		Vector2 size = new Vector2(width, height);
		hurtCollider.offset = new Vector2(0, offsetY);
		hurtCollider.size = size;
	}

	public void SetAnimState(AnimState state)
	{
		animState = state;
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
		movement.CollisionEnter(collision);
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		movement.CollisionStay(collision);
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		movement.CollisionExit(collision);
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

		if (other.layer == LayerMask.NameToLayer("Water") && movement is PlayerMove)
		{
			//enter swim state
			movement = new PlayerSwim(this);
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

	private void OnTriggerExit2D(Collider2D collision)
	{
		GameObject other = collision.gameObject;

		if (other.layer == LayerMask.NameToLayer("Water") && movement is PlayerSwim)
		{
			//enter land state
			movement = new PlayerMove(this);
		}
	}

	public void PlayJumpSound()
	{
		//audioSrc.pitch = UnityEngine.Random.Range(2 - PITCH_VARIATION, 2 + PITCH_VARIATION);
		audioSrc.PlayOneShot(jumpSound);
	}

	public void PlayRollSound()
	{
		audioSrc.PlayOneShot(rollSound);
	}

	public void PlayBounceSound()
	{
		audioSrc.PlayOneShot(bounceSound);
	}

	public void ThrowHeldObject(Vector2 velocity)
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

	public void GrabObject()
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

	public void TryStopCoroutine(Coroutine crt)
	{
		if (crt != null)
		{
			StopCoroutine(crt);
		}
	}
}
