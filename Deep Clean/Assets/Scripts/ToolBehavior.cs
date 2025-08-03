using UnityEngine;

public class ToolBehavior : MonoBehaviour
{
    public enum ToolState
    {
        Hidden,
        PickingUp,
        Idle,
        Using,
        PuttingAway
    }

    [System.Serializable]
    public class ToolData
    {
        public GameObject toolObject;
        [HideInInspector] public ToolState state = ToolState.Hidden;
    }

    public ToolData[] tools;

    private int currentIndex = -1;
    private bool torchOn = false;
    private int nextIndex = -1;


    // Update is called once per frame
    void Update()
    {
        //number key press handling
        for (int i = 0; i < tools.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (currentIndex == i)
                {
                    StartPutAway(i);
                }
                else
                {
                    if (currentIndex == -1)
                    {
                        StartPickUp(i); //no tool active
                    }
                    else
                    {
                        nextIndex = i; //put current tool away first
                        StartPutAway(currentIndex);
                    }
                }
            }
        }

        //check for left mouse hold to use tool
        if (currentIndex != -1)
        {
            ToolData tool = tools[currentIndex];

            //if current tool is torch (ind=2)
            if (currentIndex == 2) 
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    torchOn = !torchOn;

                    if (torchOn)
                        StartUsing(currentIndex);
                    else
                        StopUsing(currentIndex);
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    if (tool.state == ToolState.Idle)
                        StartUsing(currentIndex);
                }
                else
                {
                    if (tool.state == ToolState.Using)
                        StopUsing(currentIndex);
                }
            }
        }
    }

    void StartPickUp(int index)
    {
        currentIndex = index;

        ToolData tool = tools[index];
        tool.toolObject.SetActive(true);
        Animator animator = tool.toolObject.GetComponent<Animator>();
        if (animator != null)
            animator.Play("PickingUp");
        tool.state = ToolState.PickingUp;

        Invoke(nameof(SetIdleState), 0.8f); //wait until animation is done
    }

    void SetIdleState()
    {
        if (currentIndex == -1) return;
        ToolData tool = tools[currentIndex];
        Animator animator = tool.toolObject.GetComponent<Animator>();
        if (animator != null)
            animator.Play("Idle");
        tool.state = ToolState.Idle;
    }

    void StartPutAway(int index)
    {
        ToolData tool = tools[index];
        if (tool.state == ToolState.Hidden || tool.state == ToolState.PuttingAway) return;

        Animator animator = tool.toolObject.GetComponent<Animator>();
        if (animator != null)
            animator.Play("PuttingAway");
        tool.state = ToolState.PuttingAway;

        Invoke(nameof(FinishPutAway), 0.8f);
    }

    void FinishPutAway()
    {
        if (currentIndex == -1) return;

        ToolData tool = tools[currentIndex];
        tool.toolObject.SetActive(false);
        tool.state = ToolState.Hidden;
        currentIndex = -1;

        if (nextIndex != -1)
        {
            StartPickUp(nextIndex);
            nextIndex = -1;
        }
    }

    void StartUsing(int index)
    {
        ToolData tool = tools[index];


        Animator animator = tool.toolObject.GetComponent<Animator>();
        if (animator != null && index==0)
            animator.SetBool("Using", true);
        tool.state = ToolState.Using;

        //torch behavior
        Light light = tool.toolObject.GetComponentInChildren<Light>();
        if (light != null)
            light.enabled = true;
    }

    void StopUsing(int index)
    {
        ToolData tool = tools[index];

        Animator animator = tool.toolObject.GetComponent<Animator>();
        if (animator != null && index==0)
            animator.SetBool("Using", false);
        tool.state = ToolState.Idle;

        //torch behavior
        Light light = tool.toolObject.GetComponentInChildren<Light>();
        if (light != null)
            light.enabled = false;
    }
}