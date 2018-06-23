using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtZone : MonoBehaviour
{

	private Player player;

	private void Start()
	{
		player = transform.parent.GetComponent<Player>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		GameObject other = collision.gameObject;

		if (other.layer == LayerMask.NameToLayer("LevelGeometry") && other.GetComponent<PlatformEffector2D>() == null)
		{
			player.Kill();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		GameObject other = collision.gameObject;

		Damage damage = other.GetComponent<Damage>();
		if (damage != null)
		{
			//TODO: damage, respawn if necessary (like for spikes)
			player.Kill();
		}

	}
}
