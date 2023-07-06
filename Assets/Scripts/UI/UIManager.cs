using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NeoCambion;

public class UIManager : Core
{
	#region [ OBJECTS / COMPONENTS ]

	public HUDManager HUD;

	[SerializeField] UIObject combatTransitionOverlay;

	#endregion

	#region [ PROPERTIES ]



	#endregion

	#region [ COROUTINES ]

	private Coroutine c_CombatTransitionOverlay = null;

	#endregion

	/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

	void Awake()
	{

	}

	void Start()
	{

	}

	void Update()
	{

	}

	void FixedUpdate()
	{

	}

	/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

	public void CombatTransitionOverlay(float duration = 1.0f)
    {
		if (c_CombatTransitionOverlay != null)
			StopCoroutine(c_CombatTransitionOverlay);
		c_CombatTransitionOverlay = StartCoroutine(ICombatTransitionOverlay(duration));

	}

	Vector2[] transitionOffsets = new Vector2[]
	{
			new Vector2(Screen.width, 0.0f),
			Vector2.zero,
			new Vector2(-Screen.width, 0.0f)
	};

	private IEnumerator ICombatTransitionOverlay(float duration)
    {
		combatTransitionOverlay.rTransform.offsetMin = transitionOffsets[0];
		combatTransitionOverlay.rTransform.offsetMax = transitionOffsets[0];
		combatTransitionOverlay.Show(true);

		float t = 0.0f, delta, d1 = duration * 0.35f, d2 = duration * 0.5f;
		while (t <= d1)
		{
			yield return null;
			t += Time.deltaTime;
			delta = t / d1;
			combatTransitionOverlay.rTransform.offsetMin = Vector2.Lerp(transitionOffsets[0], transitionOffsets[1], delta);
			combatTransitionOverlay.rTransform.offsetMax = Vector2.Lerp(transitionOffsets[0], transitionOffsets[1], delta);
		}

		combatTransitionOverlay.rTransform.offsetMin = transitionOffsets[1];
		combatTransitionOverlay.rTransform.offsetMax = transitionOffsets[1];

		yield return new WaitForSeconds(d2);

		t = 0.0f;
		while (t <= d1)
		{
			yield return null;
			t += Time.deltaTime;
			delta = t / d1;
			combatTransitionOverlay.rTransform.offsetMin = Vector2.Lerp(transitionOffsets[1], transitionOffsets[2], delta);
			combatTransitionOverlay.rTransform.offsetMax = Vector2.Lerp(transitionOffsets[1], transitionOffsets[2], delta);
		}

		combatTransitionOverlay.rTransform.offsetMin = transitionOffsets[2];
		combatTransitionOverlay.rTransform.offsetMax = transitionOffsets[2];
		combatTransitionOverlay.Show(false);
	}
}
