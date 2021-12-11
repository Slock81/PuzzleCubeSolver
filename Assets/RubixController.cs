using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubixController : MonoBehaviour
{
    public GameObject omegaCubeObj;
    public float cubeOffset = 2.423f;

    private GameObject[,,] cubeMatrix=null;
    private int numCubesByCube = 3;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(build());
    }

    public void resetCube()
    {
        for (int d = 0; d < cubeMatrix.GetLength(0); d++)
        {
            for (int y = 0; y < cubeMatrix.GetLength(1); y++)
            {
                for (int x = 0; x < cubeMatrix.GetLength(2); x++)
                {
                    GameObject currCube = cubeMatrix[d, y, x];
                   
                    if (currCube == null)
                        continue; //The center cube is unseen
                    currCube.transform.parent = null;
                    GameObject.DestroyImmediate(currCube);
                }
            }

            
        }
        cubeMatrix = null;
        StartCoroutine(build());
    }

    public bool isSolved()
    {
        for(int d = 0; d < cubeMatrix.GetLength(0); d++)
        {
            for (int y = 0; y < cubeMatrix.GetLength(1); y++)
            {
                for (int x = 0; x < cubeMatrix.GetLength(2); x++)
                {
                    GameObject currCube = cubeMatrix[d, y, x];
                    if (currCube == null)
                        continue; //The center cube is unseen
                    Transform t = currCube.transform;
                    if(!t.localRotation.Equals(Quaternion.identity))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void clearAllEmission()
    {
        for (int d = 0; d < cubeMatrix.GetLength(0); d++)
        {
            for (int y = 0; y < cubeMatrix.GetLength(1); y++)
            {
                for (int x = 0; x < cubeMatrix.GetLength(2); x++)
                {
                    GameObject currCube = cubeMatrix[d, y, x];
                    if(currCube==null)
                        continue; //The center cube is unseen

                    currCube.GetComponent<MeshRenderer>().material.SetFloat("_EmissionStrength", 0);
                }
            }


        }
    }

    public void setEmission(GameObject p_cube, float p_brightness)
    {
        p_cube.GetComponent<MeshRenderer>().material.SetFloat("_EmissionStrength", p_brightness);
    }


    private IEnumerator build()
    {

        int radius = (numCubesByCube - 1) / 2;
        cubeMatrix = new GameObject[numCubesByCube, numCubesByCube, numCubesByCube];
        for (int d = -radius; d <= radius; d++)
        {
            int depthIdx = d + radius;
            for (int row = -radius; row <= radius; row++)
            {
                int rowIdx = row + radius;
                for (int col = -radius; col <= radius; col++)
                {
                    if (d == 0 && row == 0 && col == 0)
                        continue; //The center cube is unseen

                    int colIdx = col + radius;

                    Vector3 position = new Vector3(d * cubeOffset, row * cubeOffset, col * cubeOffset);
                    Vector3 worldPosition = transform.TransformPoint(position);
                    GameObject currCube = GameObject.Instantiate(omegaCubeObj, worldPosition, Quaternion.identity, transform);
                    currCube.name = position.ToString();
                    cubeMatrix[depthIdx, rowIdx, colIdx] = currCube;

                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float boomForce = 200f;
    public void BOOM()
    {

        for (int d = 0; d < cubeMatrix.GetLength(0); d++)
        {
            for (int y = 0; y < cubeMatrix.GetLength(1); y++)
            {
                for (int x = 0; x < cubeMatrix.GetLength(2); x++)
                {
                    GameObject currCube = cubeMatrix[d, y, x];
                    if (currCube == null)
                        continue; //The center cube is unseen

                    MeshCollider collider = currCube.GetComponent<MeshCollider>();
                    collider.enabled = false;

                    Rigidbody rb = currCube.AddComponent<Rigidbody>();
                    rb.AddForceAtPosition(Random.onUnitSphere * boomForce, transform.position);
                    rb.AddExplosionForce(boomForce, transform.position, Random.Range(1,10));
                }
            }
        }
        StartCoroutine(turnOffRBS(10.0f));
    }

    private IEnumerator turnOffRBS(float p_timeToWait)
    {
        yield return new WaitForSeconds(p_timeToWait);
        for (int d = 0; d < cubeMatrix.GetLength(0); d++)
        {
            for (int y = 0; y < cubeMatrix.GetLength(1); y++)
            {
                for (int x = 0; x < cubeMatrix.GetLength(2); x++)
                {
                    GameObject currCube = cubeMatrix[d, y, x];
                    if (currCube == null)
                        continue; //The center cube is unseen

                    Rigidbody rb = currCube.GetComponent<Rigidbody>();
                    rb.isKinematic = true;
                }
            }
        }

        resetCube();

    }


    void OnDrawGizmosSelected()
    {
        if (cubeMatrix == null)
        {
            Gizmos.color = Color.green;
            int radius = (numCubesByCube - 1) / 2;
            for (int d = -radius; d <= radius; d++)
            {
                int depthIdx = d + radius;
                for (int row = -radius; row <= radius; row++)
                {
                    int rowIdx = row + radius;
                    for (int col = -radius; col <= radius; col++)
                    {
                        if (d == 0 && row == 0 && col == 0)
                            continue; //The center cube is unseen

                        int colIdx = col + radius;

                        Vector3 position = new Vector3(d * cubeOffset, row * cubeOffset, col * cubeOffset);
                        Vector3 worldPosition = transform.TransformPoint(position);

                        Gizmos.DrawWireCube(worldPosition, new Vector3(cubeOffset, cubeOffset, cubeOffset));

                    }
                }
            }
        }

    }
}
