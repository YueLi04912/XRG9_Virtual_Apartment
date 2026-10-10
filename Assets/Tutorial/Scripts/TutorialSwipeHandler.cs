using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialSwipeHandler : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public TutorialFlowManager flowManager;

    [Header("滑动超过页面宽度的这个比例，触发翻页")]
    [Range(0.01f, 0.5f)]
    public float swipeThreshold = 0.08f;

    [Header("排查时勾选")]
    public bool debugLogs = false;

    private RectTransform pageRect;
    private Vector2 startPosition;
    private Camera pressCamera;
    private int activePointerId;
    private bool tracking;

    private void Awake()
    {
        pageRect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (tracking || pageRect == null)
            return;

        pressCamera = eventData.pressEventCamera;

        if (!GetLocalPosition(eventData, out startPosition))
            return;

        activePointerId = eventData.pointerId;
        tracking = true;

        if (debugLogs)
            Debug.Log("Swipe: 开始检测", this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 起点在按下时记录，拖拽开始时不重置。
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 不移动图片或页面。
        // 松开时统一判断是否翻页。
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        FinishSwipe(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        FinishSwipe(eventData);
    }

    private void FinishSwipe(PointerEventData eventData)
    {
        if (!tracking || eventData.pointerId != activePointerId)
            return;

        // PointerUp 和 EndDrag 可能都会到达。
        // 先结束检测，确保一次手势只翻一页。
        tracking = false;

        if (!GetLocalPosition(eventData, out Vector2 endPosition))
            return;

        Vector2 delta = endPosition - startPosition;
        float requiredDistance =
            pageRect.rect.width * swipeThreshold;

        if (debugLogs)
        {
            Debug.Log(
                $"Swipe: 横向距离={delta.x:F1}, " +
                $"纵向距离={delta.y:F1}, " +
                $"翻页阈值={requiredDistance:F1}",
                this);
        }

        // 距离不足，或主要是上下滑动，都不翻页。
        if (Mathf.Abs(delta.x) < requiredDistance ||
            Mathf.Abs(delta.x) <= Mathf.Abs(delta.y))
            return;

        if (flowManager == null)
        {
            Debug.LogWarning("Swipe: 未绑定 Flow Manager", this);
            return;
        }

        if (delta.x < 0f)
            flowManager.NextPage();
        else
            flowManager.PreviousPage();
    }

    private bool GetLocalPosition(
        PointerEventData eventData,
        out Vector2 position)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pageRect,
            eventData.position,
            pressCamera,
            out position);
    }

    private void OnDisable()
    {
        tracking = false;
    }
}