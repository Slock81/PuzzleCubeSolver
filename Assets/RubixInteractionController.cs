using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RubixController))]
public class RubixInteractionController : MonoBehaviour
{
    private RubixController rubixController;
    public LayerMask rubixLayer;
    public float emissStrength = 2;
    [Range(0.05f,1)]
    [SerializeField] private float shuffleSpeed = 0.1f;

    [Range(0.05f, 1)]
    [SerializeField] private float solveSpeed = 0.05f;


    public KeyCode rotateCubeLeft = KeyCode.LeftArrow;
    public KeyCode rotateCubeRight = KeyCode.RightArrow;
    public KeyCode rotateCubeBack = KeyCode.UpArrow;
    public KeyCode rotateCubeForward = KeyCode.DownArrow;
    public KeyCode restRotation = KeyCode.Home;
    // Start is called before the first frame update

    [Header("Face Rotation Controls")]
    public KeyCode counterClockWise = KeyCode.LeftAlt;
    
    public KeyCode rotateForwardFace = KeyCode.Keypad5;
    public KeyCode rotateRightFace = KeyCode.Keypad4;
    public KeyCode rotateTopFace = KeyCode.Keypad8;

    public KeyCode rotateBackFace = KeyCode.Keypad0;
    public KeyCode rotateLeftFace = KeyCode.Keypad6;
    public KeyCode rotateBottomFace = KeyCode.Keypad2;

    private bool isAnimating = false;
    GameObject temporaryParent;

    public int numRecordedMoves;
    private List<Vector4> facesMoved = new List<Vector4>();


    
    void Start()
    {
        rotateSpeed = shuffleSpeed;
        temporaryParent = new GameObject("RotateParent");
        temporaryParent.transform.parent = transform;
        rubixController = GetComponent<RubixController>();
    }

    public void BOOM()
    {

        rubixController.BOOM();


    }

    public enum CUBE_TYPE { CENTER,CORNER,SIDE};
    private GameObject lastHighLighted = null;
    private Vector3 rotations;
    // Update is called once per frame
    void Update()
    {
        if (isAnimating)
            return;

        if(Input.GetKey(rotateCubeLeft))
        {
            rotations.y = 1;
        }else if(Input.GetKey(rotateCubeRight))
        {
            rotations.y = -1;
        }

        if(Input.GetKey(rotateCubeBack))
        {
            rotations.z = 1;
        }else if(Input.GetKey(rotateCubeForward))
        {
            rotations.z = -1;
        }

        if(Input.GetKeyDown(restRotation))
        {
            Vector3 currRotation = transform.rotation.eulerAngles;
            currRotation.x = Mathf.RoundToInt(currRotation.x/90)*90;
            currRotation.z = Mathf.RoundToInt(currRotation.z / 90) * 90;
            transform.rotation = Quaternion.Euler(currRotation);
        }


        checkFaceRotations();

        
        if (Input.GetKeyDown(KeyCode.F2))
            StartCoroutine(solve());

        if (Input.GetKeyDown(KeyCode.F5))
            StartCoroutine(shuffle());
        
    }

    public void rewindSolve()
    {
        StartCoroutine(solve());
    }

    public void shuffleCube()
    {
        StartCoroutine(shuffle());
    }

    public bool isSolved = false;


    private IEnumerator solve()
    {
        rotateSpeed = solveSpeed;
        for(int i=facesMoved.Count-1; i>=0; i--)
        {
            Vector4 currMove = facesMoved[i];
            Vector3 faceMoved = new Vector3(currMove.x, currMove.y, currMove.z);
            bool isCC = currMove.w == 1;
            yield return rotateFace(faceMoved, !isCC); //Invert is CC to undo the move
            yield return null;
        }
        facesMoved.Clear();
        rotateSpeed = shuffleSpeed;
        yield return null;
    }


    List<Ray> testRays = new List<Ray>();
    private List<GameObject> getSide(Vector3 p_localDir)
    {
        testRays.Clear();
        List<GameObject> cubesOnSide = new List<GameObject>();

        float castDist = 2.5f * rubixController.cubeOffset;
        castPosition = transform.TransformPoint(p_localDir * castDist);

        HashSet<GameObject> foundCubes = new HashSet<GameObject>();

        Vector3 castDirection = (transform.position - castPosition).normalized;
        
        float dirStep = 10;
        float angle = 30;
        for (float zAngle = -angle; zAngle <= angle; zAngle += dirStep)
        {
            for (float yAngle = -angle; yAngle <= angle; yAngle += dirStep)
            {
                for (float xAngle = -angle; xAngle <= angle; xAngle += dirStep)
                {
                    Vector3 eulerAngles = new Vector3(xAngle, yAngle, zAngle);
                    Quaternion rotation = Quaternion.Euler(eulerAngles);
                    Vector3 rotDir = rotation * castDirection;
                    Ray currRay = new Ray(castPosition, rotDir);
                    testRays.Add(currRay);
                    GameObject foundObj = RaycastUtil.GetGameObject(currRay);
                    if (foundObj != null)
                    {
                        int maskValue = (int)Mathf.Pow(2, foundObj.layer);
                        if (maskValue == rubixLayer && !foundCubes.Contains(foundObj))
                        {
                            foundCubes.Add(foundObj);
                            cubesOnSide.Add(foundObj);
                        }
                    }
                }
            }
        }
        return cubesOnSide;
    }

    [Range(2, 500)]
    public int maxShuffle = 200;
    private Vector3[] validFaces = new Vector3[] { Vector3.forward, Vector3.right, Vector3.up, -Vector3.forward, -Vector3.right, -Vector3.up };
    public IEnumerator shuffle()
    {
        rotateSpeed = shuffleSpeed;
        int numMoves = Random.Range(1, maxShuffle);
        Debug.Log("Shuffing " + numMoves);
        for(int i=0; i<numMoves; i++)
        {
            int faceIdx = Random.Range(0, 5);
            Vector3 face = validFaces[faceIdx];
            bool isCC = Random.value > 0.5f;
            yield return rotateFace(face, isCC);
        }
        yield return null;
    }

    private IEnumerator checkIsSolved()
    {
        isSolved = rubixController.isSolved();
        yield return null;
      
    }

    [SerializeField] private Vector3 castPosition = new Vector3(-999, -999, -999);
    private Ray castRay;
    [SerializeField] public float timeToRotate = 1;
    private IEnumerator rotateFace(Vector3 p_faceDirection, bool isCounterClockWise)
    {

        
        isAnimating = true;
        float degreesToRotate = 90f;
        degreesToRotate *= isCounterClockWise ? -1 : 1;

        //Debug.Log("Rotating " + p_faceDirection + " by " + degreesToRotate);

        rubixController.clearAllEmission();

        List<GameObject> sideObjects = new List<GameObject>();
        int numTries = 0;
        while(sideObjects.Count!=9 && numTries++ < 10)
        {
            sideObjects = getSide(p_faceDirection);
            yield return null;
        }

        if (sideObjects.Count == 9)
        {
            //p_faceDirection is in World Space
            Vector4 recordedMove = new Vector4();
            recordedMove.x = p_faceDirection.x;
            recordedMove.y = p_faceDirection.y;
            recordedMove.z = p_faceDirection.z;
            //Encode the counter clockwise movement into the 4th element
            recordedMove.w = isCounterClockWise ? 1 : 0;

            facesMoved.Add(recordedMove);
            numRecordedMoves = facesMoved.Count;

            Transform tempParentTrans = temporaryParent.transform;
            tempParentTrans.parent = transform;

            Vector3 avgPosition = Vector3.zero;
            for (int i = 0; i < sideObjects.Count; i++)
            {
                avgPosition += sideObjects[i].transform.position;
            }
            avgPosition /= sideObjects.Count;
            tempParentTrans.position = avgPosition;//We want the origin to be the center block

            for (int i = 0; i < sideObjects.Count; i++)
            {
                sideObjects[i].transform.parent = tempParentTrans;
            }
            Quaternion origQ = tempParentTrans.rotation;
            tempParentTrans.RotateAround(tempParentTrans.position, transform.TransformDirection(p_faceDirection), degreesToRotate);
            yield return null;
            Quaternion toQ = tempParentTrans.rotation;

            tempParentTrans.rotation = origQ;
            yield return null;

            //Now animate it. Super hokey...sorry
            float startTime = Time.time;
            float endTime = startTime + timeToRotate;
            while (Time.time < endTime)
            {
                float percThru = (Time.time - startTime) / timeToRotate;
                Quaternion lerpQ = Quaternion.Lerp(origQ, toQ, percThru);
                tempParentTrans.rotation = lerpQ;
                yield return null;
            }
            tempParentTrans.rotation = toQ;
            yield return null;
            //Now put everyone's parent back to normal
            int numChildren = tempParentTrans.childCount;
            for (int i = numChildren - 1; i >= 0; i--)
            {
                tempParentTrans.GetChild(i).transform.parent = transform;
            }
            yield return null;
        }    
        else
        {
            Debug.LogError("We did NOT find 9 objects we found " + sideObjects.Count);
        }
        isAnimating = false;

        yield return checkIsSolved();
        
    }




    private void checkFaceRotations()
    {

        bool isCounterClockWise = Input.GetKey(counterClockWise);
        if (Input.GetKeyDown(rotateForwardFace))
        {
            
            StartCoroutine(rotateFace(Vector3.forward, isCounterClockWise));
        }

        else if(Input.GetKeyDown(rotateRightFace))
        {
            StartCoroutine(rotateFace(Vector3.right, isCounterClockWise));
        }else if (Input.GetKeyDown(rotateTopFace))
        {
            StartCoroutine(rotateFace(Vector3.up, isCounterClockWise));
        }else if(Input.GetKeyDown(rotateBackFace))
        {
            StartCoroutine(rotateFace(-Vector3.forward, isCounterClockWise));
        }else if(Input.GetKeyDown(rotateLeftFace))
        {
            StartCoroutine(rotateFace(-Vector3.right, isCounterClockWise));
        }else if(Input.GetKeyDown(rotateBottomFace))
        {
            StartCoroutine(rotateFace(-Vector3.up, isCounterClockWise));
        }
    }

    

    public CUBE_TYPE getCubeType(GameObject p_object,Vector3 p_normal)
    {
        int numNeighbors = getNumNeighbors(p_object, p_normal);
        //Debug.Log(p_object.name + " has " + numNeighbors + " neighbors");
        if (numNeighbors == 3)
            return CUBE_TYPE.CORNER;
        if (numNeighbors == 5)
            return CUBE_TYPE.SIDE;

        return CUBE_TYPE.CENTER;
    }

    private int getNumNeighbors(GameObject p_testObject, Vector3 p_normal)
    {
        List<GameObject> neighbors = getNeighbors(p_testObject, p_normal);
        if (neighbors == null)
            return 0;
        return neighbors.Count;
    }

   [SerializeField] private List<Vector3> searchSpaces = new List<Vector3>();
    [SerializeField] private Vector3 normalClick;

    public bool drawGizmos = true;
    private List<GameObject> getNeighbors(GameObject p_testObject, Vector3 p_normal)
    {
        List<GameObject> neighborObjects = new List<GameObject>();
        searchSpaces.Clear();
        float worldOffset = rubixController.cubeOffset;
        float castRadius = worldOffset * 0.5f;
        Vector3 originPos = p_testObject.transform.position;
        normalClick = p_normal;

        Quaternion rotation = Quaternion.FromToRotation(transform.forward, p_normal);
        Quaternion parentRotation = transform.rotation;

        for (int x=-1; x<=1; x++)
        {
            float testX = (x * worldOffset);
            for(int y=-1; y<=1; y++)
            {
                if (x == 0 && y == 0)
                    continue; //This is us
                float testY = (y * worldOffset);

                Vector3 castPos = new Vector3(testX, testY, 0);
                //Cast Position is in local space, we want to rotate it according to the click direction
                Vector3 rotCastPos = parentRotation * rotation * castPos;
                //Now offset it based on the origin
                Vector3 worldSpaceCastPos = rotCastPos + originPos;

                if(drawGizmos)
                    searchSpaces.Add(worldSpaceCastPos);
                Collider[] hits = Physics.OverlapSphere(worldSpaceCastPos, castRadius);
                if(hits!=null)
                {
                    for(int k=0; k<hits.Length; k++)
                    {
                        int maskValue = (int)Mathf.Pow(2, hits[k].gameObject.layer);
                        if (maskValue == rubixLayer)
                            neighborObjects.Add(hits[k].gameObject);
                    }
                }
            }
        }

        return neighborObjects;
    }

    void OnDrawGizmosSelected()
    {
        if (searchSpaces != null)
        {
            // Draw a yellow sphere at the transform's position
            for (int i = 0; i < searchSpaces.Count; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(searchSpaces[i], 0.1f);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(new Ray(searchSpaces[i], normalClick));
            }
        }
        if(!castPosition.Equals(new Vector3(-999,-999,-999)))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(castPosition, 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(castRay);

            for(int i=0; i< testRays.Count; i++)
            {
                Gizmos.DrawRay(testRays[i]);
            }

        }

        if (rubixController != null)
        {
            
            float castDist = 5 * rubixController.cubeOffset;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.TransformPoint(Vector3.zero), new Vector3(rubixController.cubeOffset, rubixController.cubeOffset, rubixController.cubeOffset)); //Move out from the cube\
            Vector3 outWardVector = transform.position + (transform.forward * castDist);
            Gizmos.DrawWireSphere(outWardVector, 0.5f); //Move out from the cube
            Gizmos.DrawLine(transform.TransformPoint(Vector3.zero), outWardVector);
            Gizmos.DrawWireCube(transform.TransformPoint(Vector3.forward * castDist) , new Vector3(rubixController.cubeOffset, rubixController.cubeOffset, rubixController.cubeOffset));
        }

    }

    private float rotateSpeed;
    private void LateUpdate()
    {
        transform.Rotate(rotations* rotateSpeed*Time.deltaTime, Space.World);
        rotations = Vector3.zero;
    }
}
