using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollTrail : MonoBehaviour
{

	private const float FADE_TIME = 0.4f;

	public AnimationCurve fadeCurve;

	private SpriteRenderer sr;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		for (float t = 0; t < FADE_TIME; t += Time.fixedDeltaTime)
		{
			float a = fadeCurve.Evaluate(t / FADE_TIME);
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a);
			yield return null;
		}
		Destroy(gameObject);
	}
}
