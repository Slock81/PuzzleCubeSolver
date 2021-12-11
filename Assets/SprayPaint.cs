using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayPaint : MonoBehaviour
{
    [Range(0.1f, 10)]
    public float radius;
    public Color colorToSpray;
    private bool isModified = false;
    public Material materialToSpray;
    private Texture2D textureToSpray;
    [SerializeField]private string SAVE_DIR = @"C:\Users\Batman\CubePuzzle\Assets\SprayPainted\";
    // Start is called before the first frame update
    void Start()
    {
        textureToSpray = new Texture2D(1024, 1024);
        materialToSpray.mainTexture = textureToSpray;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit = RaycastUtil.getMouseRaycastHit();
            //Debug.Log(hit.point + " " + hit.transform.name + " " + hit.textureCoord);
            Vector2 uv = hit.textureCoord;
            float xPos = textureToSpray.width * uv.x;
            float yPos = textureToSpray.height * uv.y;
            sprayPos(xPos, yPos);
        }
    }

    private void sprayPos(float p_x, float p_y)
    {

        int leftX = Mathf.FloorToInt(p_x - radius);
        int rightX = Mathf.CeilToInt(p_x + radius);
        int topY = Mathf.FloorToInt(p_y - radius);
        int botY = Mathf.CeilToInt(p_y + radius);

        Debug.Log(topY + "," + leftX + " to " + botY + "," + rightX);
        float sqRadius = radius * radius; //Square this, so we don't have to take many square roots

        for(int y=topY; y<=botY; y++)
        {
            if (y < 0 || y >= textureToSpray.height)
                continue;

            float yDelta = y - p_y;
            float sqY = yDelta * yDelta;
            for(int x=leftX; x<=rightX; x++)
            {
                if (x < 0 || x >= textureToSpray.width)
                    continue;

                float xDelta = x - p_x;
                float sqX = xDelta - xDelta;

                float sqDist = sqY + sqX;
                Debug.Log(sqDist + " vs" + sqRadius);
                if(sqDist<sqRadius)
                {
                    //Paint
                    isModified = true;
                    textureToSpray.SetPixel(x, y, colorToSpray);
                }
            }
        }
        textureToSpray.Apply();
        //materialToSpray.mainTexture = textureToSpray;
    }

    void OnDestroy()
    {
        if (isModified)
        {
            byte[] bytes = textureToSpray.EncodeToPNG();

            if (!Directory.Exists(SAVE_DIR))
            {
                Directory.CreateDirectory(SAVE_DIR);
            }
            string name = SAVE_DIR + "SprayPaninted_.png";
            File.WriteAllBytes(name, bytes);
            Debug.Log(name);
        }
    }
}
