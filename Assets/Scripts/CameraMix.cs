using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMix : MonoBehaviour
{
	public CinemachineMixingCamera mixingCam;
	public CinemachineVirtualCamera playerCam;
	public CinemachineVirtualCamera starCam;

	private const float FAR_DISTANCE = 8.0f; //distance at which to start zooming
	private const float CLOSEST_DISTANCE = 2.5f; //distance at which to stop zooming
	private const float LERP_AMOUNT = 2.0f; //factor to zoom out after collecting star

	private Transform player;
	private Star nearestStar;
	private Star[] stars;
	private float closestDistance;
	private float weight = 1.0f;

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerCam.m_Follow = player;
		stars = FindObjectsOfType<Star>();
		UpdateNearestStar();
	}

	private void FixedUpdate()
	{
		UpdateNearestStar();
		if (nearestStar != null)
		{
			starCam.m_Follow = nearestStar.transform;
		}
		//update blend weights based on player distance
		//weight = GetPlayerWeight();
		weight = Mathf.Lerp(weight, GetPlayerWeight(), LERP_AMOUNT * Time.fixedDeltaTime);
		mixingCam.m_Weight0 = weight;
		mixingCam.m_Weight1 = 1 - weight;
	}

	private void UpdateNearestStar()
	{
		closestDistance = -1;
		foreach (Star star in stars)
		{
			if (!star.WasCollected())
			{
				float distance = Vector3.Distance(star.transform.position, player.position);
				if (distance < closestDistance || closestDistance < 0) //negative = first star
				{
					closestDistance = distance;
					nearestStar = star;
				}
			}
		}
	}

	private float GetPlayerWeight()
	{
		if (nearestStar == null || nearestStar.WasCollected())
		{
			return 1.0f;
			//return Mathf.Lerp(weight, 1.0f, LERP_AMOUNT * Time.fixedDeltaTime);
		}

		float dist = Mathf.Clamp(closestDistance, CLOSEST_DISTANCE, FAR_DISTANCE);
		return (dist - CLOSEST_DISTANCE) / (FAR_DISTANCE - CLOSEST_DISTANCE);
	}
}
