using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{

	private const float ROTATE_PUSH_FACTOR = 5.0f; //multiplier to move player by when standing on platform while rotating

	public float holdTime; //time (seconds) spent not rotating per cycle
	public float rotateTime; //time (seconds) spent rotating per cycle
	public int rotateAmount; //amount (degrees) to rotate each cycle
	public bool clockwise; //direction to rotate

	private float angle;
	private Rigidbody2D rb;
	private float rotateSpeed;

	private void Start()
	{
		angle = 0;
		rb = GetComponent<Rigidbody2D>();
		StartCoroutine(Hold());
	}
	
	private IEnumerator Hold()
	{
		rotateSpeed = 0;
		yield return new WaitForSeconds(holdTime);
		StartCoroutine(Rotate());
	}

	private IEnumerator Rotate()
	{
		rotateSpeed = (ROTATE_PUSH_FACTOR * rotateAmount / 360) / rotateTime; //TODO: should this depend on size?

		float prevAngle = angle;
		for (float t = 0; t < rotateTime; t += Time.fixedDeltaTime)
		{
			float a = t / rotateTime * rotateAmount;
			SetAngle(prevAngle, a);
			yield return new WaitForFixedUpdate();
		}
		SetAngle(prevAngle, rotateAmount);
		StartCoroutine(Hold());
	}

	private void SetAngle(float prevAngle, float partialAddition)
	{
		if (clockwise)
		{
			partialAddition = -partialAddition;
		}
		angle = prevAngle + partialAddition;
		rb.rotation = angle;
	}

	private void FixedUpdate()
	{
		//stay in place with MovePosition to current position
		rb.MovePosition(rb.position);

		//set velocity
		Vector2 moveDir = clockwise ? Vector2.right : Vector2.left;
		rb.velocity = rotateSpeed * moveDir;
	}
}
