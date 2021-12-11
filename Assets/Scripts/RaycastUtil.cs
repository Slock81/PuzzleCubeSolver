using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastUtil : MonoBehaviour
{
    public static Vector2 ERROR_VECTOR = Vector2.positiveInfinity;
    public static GameObject objectUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue))
        {
            return hit.transform.gameObject;
        }
        return null;

    }

    public static Vector3 objectUnderMouseNormal()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue))
        {
            return hit.normal;
        }
        return Vector3.zero;

    }

    public static GameObject GetGameObject(Ray p_ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(p_ray, out hit, float.MaxValue))
        {
            return hit.transform.gameObject;
        }
        return null;
    }

    public static Vector2 getPixelUVs(Vector2 p_pixel)
    {
        Ray ray = Camera.main.ScreenPointToRay(p_pixel);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue))
        {
            return hit.textureCoord;
        }
        return ERROR_VECTOR;
    }

    public static GameObject getPixelObject(Vector2 p_mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(p_mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue))
        {
            return hit.transform.gameObject;
        }
        return null;
    }

    public static Vector2 getClickedUVS()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue))
        {
            return hit.textureCoord;
        }
        return ERROR_VECTOR;
    }

    public static RaycastHit getMouseRaycastHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, float.MaxValue);
        return hit;
    }
}