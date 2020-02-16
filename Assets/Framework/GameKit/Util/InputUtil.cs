using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputUtil
{
    public static bool _isOnUI = false;

    private static List<RaycastResult> _list = new List<RaycastResult>();

    public static bool CheckMouseOnUI()
    {
        if (_isOnUI) return true;
        return IsPointerOverGameObject();
    }

    static bool IsPointerOverGameObject()
    {
        if (EventSystem.current)
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    {
                        return true;
                    }
                }
            }
            else
            {
                return EventSystem.current.IsPointerOverGameObject();
            }
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.pressPosition = Input.mousePosition;
            eventData.position = Input.mousePosition;

            _list.Clear();
            EventSystem.current.RaycastAll(eventData, _list);
            return _list.Count > 0;
        }
        return false;
    }

    public static bool IsPointerOverGameObject(Canvas canvas, Vector2 screenPosition)
    {
        if (_isOnUI) return true;

        if (EventSystem.current == null) return false;

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = screenPosition;
        UnityEngine.UI.GraphicRaycaster uiRaycaster = canvas.gameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>();

        _list.Clear();
        uiRaycaster.Raycast(eventDataCurrentPosition, _list);

        return _list.Count > 0;
    }
}
