using UnityEngine;
using System.Collections;

public class ToolManager : MonoBehaviour
{
    [Header("Tools")]
    public GameObject mop;
    public GameObject vaccum;
    public GameObject rubbishBinObject;
    public RubbishBin rubbishBinScript;
    public GameObject cameraObject;

    [Header("Camera others")]
    public GameObject photoManager;
    public GameObject cameraUI;
    public GameObject screenUI;

    [Header("Animators")]
    public Animator mopAnimator;
    public Animator vacuumAnimator;
    public Animator rubbishBinAnimator;
    public Animator cameraAnimator;

    [Header("Others")]
    public LayerMask dirtLayer;

    private CleaningTool mopTool;
    private CleaningTool vaccumTool;
    private enum Tool {None, Mop, Vaccum, RubbishBin, Camera }
    private Tool currentTool = Tool.None;
    private bool isSwitching = false;
    private DirtSpot currentDirtSpot = null;

    void Start()
    {
        SetToolActive(Tool.None);

        mopTool = mop.GetComponent<CleaningTool>();
        vaccumTool = vaccum.GetComponent<CleaningTool>();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTool(Tool.Mop);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTool(Tool.Vaccum);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTool(Tool.RubbishBin);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchTool(Tool.Camera);

        if (currentTool == Tool.Mop)
        {
            HandleCleaning(mopTool, mopAnimator, "Using");
        }
        else if (currentTool == Tool.Vaccum)
        {
            HandleCleaning(vaccumTool, vacuumAnimator, "VaccumUsing");
        }
    }

    void SwitchTool(Tool newTool)
    {
        if (isSwitching) return;

        if (currentTool == newTool)
        {
            StartCoroutine(SwitchRoutine(Tool.None));
        }
        else
        {
            StartCoroutine(SwitchRoutine(newTool));
        }
    }

    IEnumerator SwitchRoutine(Tool newTool)
    {
        isSwitching = true;

        //stop cleaning dirt when switching tools
        if (currentDirtSpot != null)
        {
            currentDirtSpot.StopCleaning();
            currentDirtSpot = null;
        }
        
        //play putaway current
        yield return PlayPutAway(currentTool);
        SetToolActive(Tool.None);
        //activate new
        SetToolActive(newTool);
        yield return PlayPickUp(newTool);

        currentTool = newTool;
        isSwitching = false;
    }

    void SetToolActive(Tool tool)
    {
        mop.SetActive(tool == Tool.Mop);
        vaccum.SetActive(tool == Tool.Vaccum);
        rubbishBinObject.SetActive(tool == Tool.RubbishBin);
        cameraObject.SetActive(tool == Tool.Camera);
    }

    IEnumerator PlayPutAway(Tool tool)
    {
        Animator anim = GetAnimator(tool);
        if (anim != null)
        {
            if (tool == Tool.Camera)
            {
                cameraUI.SetActive(false);
                photoManager.SetActive(false);
                anim.Play("PutdownCamera");
                screenUI.SetActive(true);
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                anim.Play("PuttingAway");
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator PlayPickUp(Tool tool)
    {
        Animator anim = GetAnimator(tool);
        if (anim != null)
        {
            if (tool == Tool.Camera)
            {
                screenUI.SetActive(false);
                anim.Play("PickupCamera");
                yield return new WaitForSeconds(1.2f);
                photoManager.SetActive(true);
                cameraUI.SetActive(true);
            }
            else
            {
                anim.Play("PickingUp");
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    Animator GetAnimator(Tool tool)
    {
        switch (tool)
        {
            case Tool.Mop: return mopAnimator;
            case Tool.Vaccum: return vacuumAnimator;
            case Tool.RubbishBin: return rubbishBinAnimator;
            case Tool.Camera: return cameraAnimator;
            default: return null;
        }
    }

    void HandleCleaning(CleaningTool tool, Animator animator, string usingAnimation)
    {
        //dont animate if tool not active
        if (!tool.gameObject.activeInHierarchy)
        {
            StopCurrentDirt();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //only play anim if animator enabled and object active
            if (animator != null && animator.gameObject.activeInHierarchy)
            {
                animator.SetBool(usingAnimation, true);
            }

            //check for dirt in contant
            DirtSpot dirt = GetDirtInContact(tool.toolTip);
            if (dirt != null)
            {
                if (currentDirtSpot != dirt)
                {
                    StopCurrentDirt();
                    currentDirtSpot = dirt;
                    currentDirtSpot.StartCleaning(tool);
                }
            }
            else 
            {
                StopCurrentDirt();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (animator != null && animator.gameObject.activeInHierarchy)
            {
                animator.SetBool(usingAnimation, false);
            }

            StopCurrentDirt();
        }
    }

    DirtSpot GetDirtInContact(Transform tip)
    {
        //check for colliders on dirtlayer
        Collider[] hits = Physics.OverlapSphere(tip.position, 0.5f, dirtLayer);

        foreach (Collider col in hits)
        {
            DirtSpot dirtSpot = col.GetComponent<DirtSpot>();
            if (dirtSpot != null)
            {
                if (!dirtSpot.IsFullyCleaned())
                {
                    return dirtSpot;
                }
            }
        }
        return null;
    }

    private void StopCurrentDirt()
    {
        if (currentDirtSpot != null)
        {
            currentDirtSpot.StopCleaning();
            currentDirtSpot = null;
        }
    }

    public int GetCurrentToolIndex()
    {
        switch (currentTool)
        {
            case Tool.Mop: return 0;
            case Tool.Vaccum: return 1;
            case Tool.RubbishBin: return 2;
            case Tool.Camera: return 3;
            default: return -1;
        }
    }

    public void ForcePutAwayCurrentTool()
    {
        if (currentTool != Tool.None)
        {
            StopAllCoroutines();
            SetToolActive(Tool.None);
            currentTool = Tool.None;
        }
    }

    public void ForcePickUpTool(int index)
    {
        Tool toolToPickUp = Tool.None;
        switch (index)
        {
            case 0: toolToPickUp = Tool.Mop; break;
            case 1: toolToPickUp = Tool.Vaccum; break;
            case 2: toolToPickUp = Tool.RubbishBin; break;
            case 3: toolToPickUp = Tool.Camera; break;
        }

        if (toolToPickUp != Tool.None)
        {
            StopAllCoroutines();
            SetToolActive(toolToPickUp);
            currentTool = toolToPickUp;
        }
    }
}
