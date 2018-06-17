using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public int flip = -1;
    private Rigidbody2D rb;

	private void Start()
	{
        rb = GetComponent<Rigidbody2D>();
	}
	
	private void Update()
	{
        //transform.position = new Vector2(startPosition.x, startPosition.y + 3 * Mathf.Sin(Time.time));
        rb.velocity = new Vector2(flip * Mathf.Cos(Time.time), 0);
	}
}
