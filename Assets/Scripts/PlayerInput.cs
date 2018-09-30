using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput
{

	public bool jumpQueued = false;
	public bool jumpReleaseQueued = false;
	public bool rollQueued = false;
	public bool rollReleaseQueued = false;
	public bool triggerWasHeld = false;
	public bool grabQueued = false;

	public Coroutine crtCancelQueuedJump;
	private const float JUMP_BUFFER_TIME = 0.1f; //time before hitting ground a jump will still be queued

	public Vector2 GetAxes()
	{
		return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
	}

	public void Update(Player player)
	{
		if (Input.GetButtonDown("Jump"))
		{
			StopCancelQueuedJump(player);
			jumpQueued = true;
			crtCancelQueuedJump = player.StartCoroutine(CancelQueuedJump());
		}

		if (Input.GetButtonUp("Jump"))
		{
			jumpReleaseQueued = true;
		}

		bool triggerHeld = Input.GetAxis("LTrigger") > 0 || Input.GetAxis("RTrigger") > 0;
		bool triggerPressed = !triggerWasHeld && triggerHeld;
		if (triggerPressed)
		{
			rollQueued = true;
		}

		bool triggerReleased = triggerWasHeld && !triggerHeld;
		if (triggerReleased)
		{
			rollReleaseQueued = true;
		}
		triggerWasHeld = triggerHeld;

		if (Input.GetButtonDown("ActionL") || Input.GetButtonDown("ActionR"))
		{
			grabQueued = true;
		}
	}

	public void StopCancelQueuedJump(Player player)
	{
		player.TryStopCoroutine(crtCancelQueuedJump);
	}

	private IEnumerator CancelQueuedJump()
	{
		yield return new WaitForSeconds(JUMP_BUFFER_TIME);
		jumpQueued = false;
	}
	
}
