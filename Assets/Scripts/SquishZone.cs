using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishZone : MonoBehaviour
{

    private Player player;

	private void Start()
	{
        player = transform.parent.GetComponent<Player>();
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("LevelGeometry"))
        {
            player.Kill();
        }
    }
}
