using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public InventoryManager.CollectedEntry linkedItem;
    [HideInInspector] public Computer sourceComputer;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private bool droppedSuccessfully = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        droppedSuccessfully = false;
        originalParent = transform.parent;
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out var localPoint);
        rectTransform.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!droppedSuccessfully)
        {
            transform.SetParent(originalParent, true);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        canvasGroup.blocksRaycasts = true;
    }

    public void MarkDropped()
    {
        droppedSuccessfully = true;
    }

    public void RemoveFromSource()
    {
        if (sourceComputer != null)
        {
            sourceComputer.RemoveItemSlot(gameObject);
        }
    }
}
