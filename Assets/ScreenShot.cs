using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    public string dir = @"C:\Users\Batman\RUBIX\";
    public KeyCode takeShot = KeyCode.F12;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(takeShot))
        {
            StartCoroutine(takeScreenShot());
        }
    }

    private IEnumerator takeScreenShot()
    {
        yield return new WaitForEndOfFrame();
        string png = dir + "Rubix_Build_" + System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".png";
        ScreenCapture.CaptureScreenshot(png);
        Debug.Log(png);


    }
}
