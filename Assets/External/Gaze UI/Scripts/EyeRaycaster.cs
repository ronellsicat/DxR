using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRSettings = UnityEngine.VR.VRSettings;
#endif

[System.Serializable]
public class FloatUnityEvent : UnityEvent<float> { }

public class EyeRaycaster : MonoBehaviour
{
    [SerializeField, Tooltip("In seconds")]
    float m_loadingTime;
    [SerializeField]
    FloatUnityEvent m_onLoad;

    float m_elapsedTime = 0;

    // Prevents loop over the same selectable
    Selectable m_excluded; 
    Selectable m_currentSelectable;
    RaycastResult m_currentRaycastResult;

    IPointerClickHandler m_clickHandler;
    IDragHandler m_dragHandler;

    EventSystem m_eventSystem;
    PointerEventData m_pointerEvent;

    private void Start()
    {
        m_eventSystem = EventSystem.current;
        m_pointerEvent = new PointerEventData(m_eventSystem);
        m_pointerEvent.button = PointerEventData.InputButton.Left;
    }

    void Update()
    {
        // Set pointer position
        m_pointerEvent.position =
#if UNITY_EDITOR
            new Vector2(Screen.width / 2, Screen.height / 2);
#else
            new Vector2(XRSettings.eyeTextureWidth / 2, XRSettings.eyeTextureHeight / 2);
#endif

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        m_eventSystem.RaycastAll(m_pointerEvent, raycastResults);

        // Detect selectable
        if (raycastResults.Count > 0)
        {
            foreach(var result in raycastResults)
            {
                var newSelectable = result.gameObject.GetComponentInParent<Selectable>();

                if (newSelectable)
                {
                    if(newSelectable != m_excluded && newSelectable != m_currentSelectable)
                    {
                        Select(newSelectable);
                        m_currentRaycastResult = result;
                    }
                    break;
                }
            }
        }
        else
        {
            if(m_currentSelectable || m_excluded)
            {
                Select(null, null);
            }
        }

        // Target is being activating
        if (m_currentSelectable)
        {
            m_elapsedTime += Time.deltaTime;
            m_onLoad.Invoke(m_elapsedTime / m_loadingTime);

            if (m_elapsedTime > m_loadingTime)
            {
                if (m_clickHandler != null)
                {
                    m_clickHandler.OnPointerClick(m_pointerEvent);
                    Select(null, m_currentSelectable);
                }
                else if (m_dragHandler != null)
                {
                    m_pointerEvent.pointerPressRaycast = m_currentRaycastResult;
                    m_dragHandler.OnDrag(m_pointerEvent);
                }
            }
        }
    }

    void Select(Selectable s, Selectable exclude = null)
    {
        m_excluded = exclude;

        if (m_currentSelectable)
            m_currentSelectable.OnPointerExit(m_pointerEvent);

        m_currentSelectable = s;

        if (m_currentSelectable)
        {
            m_currentSelectable.OnPointerEnter(m_pointerEvent);
            m_clickHandler = m_currentSelectable.GetComponent<IPointerClickHandler>();
            m_dragHandler = m_currentSelectable.GetComponent<IDragHandler>();
        }

        m_elapsedTime = 0;
        m_onLoad.Invoke(m_elapsedTime / m_loadingTime);
    }
}
