using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.Extras;

public class SteamVRUIButtonLaser : MonoBehaviour
{
    [Header("教程")]
    public TutorialFlowManager flowManager;
    public RectTransform tutorialPage;

    [Header("手柄输入")]
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean triggerAction;
    public Transform rayOrigin;

    [Header("滑动距离：页面宽度的比例")]
    [Range(0.01f, 0.5f)]
    public float swipeThreshold = 0.08f;

    [Header("Viewer")]
    public Transform viewReference;

    public bool debugLogs = true;

    private SteamVR_LaserPointer laserPointer;
    private Plane pagePlane;
    private Vector2 startPosition;
    private Vector2 lastPosition;
    private bool tracking;

    private void Awake()
    {
        laserPointer = GetComponent<SteamVR_LaserPointer>();

        if (controllerPose == null)
            controllerPose = GetComponentInParent<SteamVR_Behaviour_Pose>();

        if (rayOrigin == null)
            rayOrigin = transform;
    }

    private void OnEnable()
    {
        if (laserPointer != null)
            laserPointer.PointerClick += OnPointerClick;
    }

    private void OnDisable()
    {
        if (laserPointer != null)
            laserPointer.PointerClick -= OnPointerClick;

        tracking = false;
    }

    private void LateUpdate()
    {
        if (controllerPose == null || triggerAction == null ||
            rayOrigin == null || tutorialPage == null ||
            flowManager == null)
        {
            tracking = false;
            return;
        }

        if (!tutorialPage.gameObject.activeInHierarchy)
        {
            tracking = false;
            return;
        }

        SteamVR_Input_Sources hand = controllerPose.inputSource;

        if (triggerAction.GetStateDown(hand))
            BeginSwipe();

        if (!tracking)
            return;

        // 射线离开页面边缘后，仍在同一个平面上计算位置。
        if (TryGetPagePosition(out Vector2 position))
            lastPosition = position;

        if (triggerAction.GetStateUp(hand))
        {
            FinishSwipe();
        }
        else if (!triggerAction.GetState(hand))
        {
            tracking = false;
        }
    }

    private void BeginSwipe()
    {
        tracking = false;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        // 从按钮上按下时，交给原来的按钮点击逻辑。
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Button button = hit.transform.GetComponentInParent<Button>();

            if (button != null)
                return;
        }

        pagePlane = new Plane(
            tutorialPage.forward,
            tutorialPage.position);

        if (!TryGetPagePosition(out Vector2 position))
            return;

        // 必须从教程页面内部开始滑动。
        if (!tutorialPage.rect.Contains(position))
            return;

        startPosition = position;
        lastPosition = position;
        tracking = true;

        if (debugLogs)
            Debug.Log("VR Swipe: 开始检测", this);
    }

    private bool TryGetPagePosition(out Vector2 position)
    {
        position = Vector2.zero;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (!pagePlane.Raycast(ray, out float distance))
            return false;

        Vector3 localPoint = tutorialPage.InverseTransformPoint(
            ray.GetPoint(distance));

        position = new Vector2(localPoint.x, localPoint.y);
        return true;
    }

    private void FinishSwipe()
    {
        tracking = false;

        if (viewReference == null)
        {
            Debug.LogWarning("Assign View Reference to the VR camera.", this);
            return;
        }

        Vector2 delta = lastPosition - startPosition;
        float threshold = tutorialPage.rect.width * swipeThreshold;

        if (Mathf.Abs(delta.x) < threshold ||
            Mathf.Abs(delta.x) <= Mathf.Abs(delta.y))
            return;

        // Convert movement from page coordinates to world coordinates.
        Vector3 worldDelta = tutorialPage.TransformVector(
            new Vector3(delta.x, delta.y, 0f)
        );

        // Negative means left from the viewer's perspective.
        float horizontalMovement =
            Vector3.Dot(worldDelta, viewReference.right);

        if (Mathf.Abs(horizontalMovement) < 0.0001f)
            return;

        if (horizontalMovement < 0f)
        {
            flowManager.NextPage();

            if (debugLogs)
                Debug.Log("VR Swipe: Left -> Next page", this);
        }
        else
        {
            flowManager.PreviousPage();

            if (debugLogs)
                Debug.Log("VR Swipe: Right -> Previous page", this);
        }

    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        // 页面滑动期间不触发误碰到的按钮。
        if (tracking || e.target == null)
            return;

        Button button = e.target.GetComponentInParent<Button>();

        if (button != null &&
            button.isActiveAndEnabled &&
            button.IsInteractable())
        {
            button.onClick.Invoke();
            Debug.Log("VR UI clicked: " + button.name);
        }
    }
}