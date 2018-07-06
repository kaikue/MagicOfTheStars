using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{

	public float holdTime; //time (seconds) spent not rotating per cycle
	public float rotateTime; //time (seconds) spent rotating per cycle
	public int rotateAmount; //amount (degrees) to rotate each cycle
	public bool clockwise; //direction to rotate

	private float angle;
	private Rigidbody2D rb;

	private void Start()
	{
		angle = 0;
		rb = GetComponent<Rigidbody2D>();
		StartCoroutine(Hold());
	}
	
	private IEnumerator Hold()
	{
		yield return new WaitForSeconds(holdTime);
		StartCoroutine(Rotate());
	}

	private IEnumerator Rotate()
	{
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
}
