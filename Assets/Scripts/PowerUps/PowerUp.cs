using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private const float REACTIVATE_TIME = 3.0f; //time in seconds to reactivate after player collects
    private const float COLOR_TRANSITION_TIME = 0.5f; //time in seconds to switch colors

    public Color inactiveColor;

    private AudioSource audioSrc;
    private BoxCollider2D trigger;
    private SpriteRenderer sr;
    private Color activeColor;

    protected virtual void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        trigger = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        activeColor = sr.color;
    }

    public virtual void Activate(Player player)
    {
        //TODO: particles
        audioSrc.Play();
        trigger.enabled = false;
        StartCoroutine(TransitionToColor(inactiveColor));
        StartCoroutine(Reactivate());
    }

    private IEnumerator Reactivate()
    {
        yield return new WaitForSeconds(REACTIVATE_TIME);
        trigger.enabled = true;
        StartCoroutine(TransitionToColor(activeColor));
    }

    private IEnumerator TransitionToColor(Color newColor)
    {
        Color oldColor = sr.color;
        for (float t = 0; t < COLOR_TRANSITION_TIME; t += Time.deltaTime)
        {
            sr.color = Color.Lerp(oldColor, newColor, t / COLOR_TRANSITION_TIME);
            yield return new WaitForEndOfFrame();
        }
        sr.color = newColor;
    }
}
