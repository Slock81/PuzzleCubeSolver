using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoColorer : MonoBehaviour
{
    public float offset = 3.5f;
    [SerializeField] private Vector3[] facesToPaint = new Vector3[] { Vector3.forward, -Vector3.forward, Vector3.up, -Vector3.up, Vector3.left, -Vector3.left };
    public Vector2 textureSize = new Vector2(2048, 2048);
    public Transform cameraTransform; //For Debug
    public KeyCode paintKey = KeyCode.F5;
    public string saveDir;
    private bool isAnimating = false;
    public Gradient colorGradient;
    public MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(paintKey) && !isAnimating)
        {
            Debug.Log("Going to spray paint");
            StartCoroutine(sprayPaint());   
        }
    }

    public IEnumerator sprayPaint()
    {

        Texture2D sprayTexture = new Texture2D((int)textureSize.x, (int)textureSize.y, TextureFormat.ARGB32,false);
        meshRenderer.material.mainTexture = sprayTexture;
        isAnimating = true;
        for (int i = 0; i < facesToPaint.Length; i++)
        {
            float perc = i / (float)(facesToPaint.Length - 1);
            Color sprayColor = colorGradient.Evaluate(perc);
            Vector3 currDir = facesToPaint[i];
            Debug.Log("currDir:" + currDir +" with color " + sprayColor +" ("+perc+")");
            Vector3 updatedPosition = (currDir * offset) + transform.position;
            cameraTransform.position = updatedPosition;
            cameraTransform.LookAt(transform);
            yield return null;

            int numPixelsY = Screen.height;
            int numPixelsX = Screen.width;

            for(int y=0; y<numPixelsX; y++)
            {
                for(int x=0; x<numPixelsX; x++)
                {
                    Vector2 currPixel = new Vector2(x, y);
                    GameObject objectInPixel = RaycastUtil.getPixelObject(currPixel);
                    if(objectInPixel!=null && objectInPixel.transform.Equals(transform))
                    {
                        //It's me!
                        Vector2 uvs = RaycastUtil.getPixelUVs(currPixel);
                        if(uvs!=null)
                        {
                            int colorX = (int)(uvs.x * sprayTexture.width);
                            int colorY = (int)(uvs.y * sprayTexture.height);
                            sprayTexture.SetPixel(colorX, colorY, sprayColor);
                        }
                    }
                }
                sprayTexture.Apply();
                yield return null;
            }
        }

        byte[] bytes = sprayTexture.EncodeToPNG();

        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        string name = saveDir + "Auto_SprayPaninted.png";
        File.WriteAllBytes(name, bytes);
        Debug.Log(name);

        Debug.Log("All Done!");
        isAnimating = false;
        yield return null;
    }
}
