using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            RaycastHit hit = RaycastUtil.getMouseRaycastHit();
            Debug.Log(hit.point + " " + hit.transform.name + " " + hit.textureCoord);
        }
    }
}
