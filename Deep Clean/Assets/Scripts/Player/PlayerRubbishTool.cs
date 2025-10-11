using UnityEngine;
using System.Collections;

public class PlayerRubbishTool : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public GameObject openRubbishBag;
    public GameObject tiedRubbishBag;

    public RubbishBag CurrentBag { get; private set; }

    private bool hasBag = false;

    void Awake()
    {
        CurrentBag = openRubbishBag.GetComponent<RubbishBag>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (!hasBag)
            {
                StartCoroutine(PlayerToolManager.Instance.SwitchTool(() => PickupRoutine()));
            }
            else
            {
                if (CurrentBag.isTied)
                {
                    StartCoroutine(PutdownRoutine());
                }
                else
                {
                    if (CurrentBag.IsFull())
                    {
                        StartCoroutine(TieRoutine());
                    }
                    else
                    {
                        StartCoroutine(PutdownRoutine());
                    }
                }
            }
        }
    }



    public bool HasOpenBag() => hasBag && !CurrentBag.isTied;
    public bool IsHoldingBag() => hasBag;

    public IEnumerator PickupRoutine()
    {
        animator.SetTrigger("Pickup");
        yield return new WaitForSeconds(0.05f);

        if (CurrentBag.isTied)
        {
            tiedRubbishBag.SetActive(true);
            openRubbishBag.SetActive(false);
        }
        else
        {
            openRubbishBag.SetActive(true);
            tiedRubbishBag.SetActive(false);
        }

        hasBag = true;
    }

    public IEnumerator TieRoutine()
    {
        GameManager.Instance.Notify("Bag is full!");
        
        animator.SetTrigger("Tie");
        yield return new WaitForSeconds(0.1f);

        CurrentBag.TieBag();

        openRubbishBag.SetActive(false);
        tiedRubbishBag.SetActive(true);
    }

    public IEnumerator PutdownRoutine()
    {
        animator.SetTrigger("Putdown");
        yield return new WaitForSeconds(0.05f);

        openRubbishBag.SetActive(false);
        tiedRubbishBag.SetActive(false);
        hasBag = false;
    }

    // Called by CollectRubbish when E is pressed on a bin
    public void EmptyBagAtBin()
    {
        if (!hasBag || CurrentBag.IsEmpty()) return;

        int collected = CurrentBag.currentAmount;

        if (!CurrentBag.isTied)
        {
            // Open bag → just empty rubbish
            CurrentBag.currentAmount = 0;
            Debug.Log("Open bag emptied!");
        }
        else
        {
            // Tied bag → putdown + reset to fresh open bag
            StartCoroutine(DisposeTiedBagRoutine());
        }
    }

    private IEnumerator DisposeTiedBagRoutine()
    {
        animator.SetTrigger("Putdown");
        yield return new WaitForSeconds(0.05f);

        tiedRubbishBag.SetActive(false);
        hasBag = false;

        // Reset to fresh empty open bag
        CurrentBag = openRubbishBag.GetComponent<RubbishBag>();
        CurrentBag.currentAmount = 0;
        CurrentBag.isTied = false;
        Debug.Log("Tied bag disposed, fresh open bag ready.");
    }
}
