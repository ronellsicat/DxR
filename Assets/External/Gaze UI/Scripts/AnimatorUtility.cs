using UnityEngine;

public class AnimatorUtility : MonoBehaviour
{
	[SerializeField]
	Animator m_animator;
	[SerializeField]
	string m_parameterName;

	public void SetFloat(float value)
	{
		m_animator.SetFloat(m_parameterName, value);
	}
}
