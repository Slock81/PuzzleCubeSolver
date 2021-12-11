using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubixTesterController : MonoBehaviour
{
    public GameObject pedestal;
    public GameObject puzzleCube;
    [SerializeField] private Material pedestalMaterial;
    [SerializeField] private RubixInteractionController puzzleInteraction;
    [SerializeField]
    private MeshRenderer pedestalRenderer;

    [SerializeField] private Color noMovesColor = Color.grey;
    [SerializeField] private Color shuffledColor = Color.grey;
    [SerializeField] private Color solovedColor = Color.grey;
    [SerializeField] private Color failedColor = Color.grey;

    // Start is called before the first frame update
    void Start()
    {
        //puzzleInteraction = puzzleCube.GetComponent<RubixInteractionController>();
        //pedestalRenderer = pedestal.GetComponent<MeshRenderer>();
        //Make a copy of the material so it is unique to this pedestal
        pedestalMaterial = new Material(pedestalRenderer.material);
        pedestalRenderer.material = pedestalMaterial;
        pedestalMaterial.SetColor("_Color", noMovesColor);
    }


    bool hasStarted = false;
    // Update is called once per frame
    void Update()
    {
        if(!hasStarted)
        {
            //Check if any moves happened
            if(puzzleInteraction.numRecordedMoves>0)
            {
                hasStarted = true;
                pedestalMaterial.SetColor("_Color", shuffledColor);
            }
        }
        
            solved();
        

        if (Input.GetKeyDown(KeyCode.F11))
        {
            failed();
        }

    }

    public void solved()
    {
        if(hasStarted)
        {
            if(puzzleInteraction.isSolved)
                pedestalMaterial.SetColor("_Color", solovedColor);
        }
    }

    public void failed()
    {
        if (hasStarted)
        {
            pedestalMaterial.SetColor("_Color", failedColor);
                 puzzleInteraction.BOOM();
        }
    }

    public void shuffle()
    {
        if(!hasStarted)
        {
            puzzleInteraction.shuffleCube();
        }
    }
}
