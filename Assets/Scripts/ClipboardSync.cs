using UnityEngine;
using Valve.VR; 

public class ClipboardSync : MonoBehaviour
{
    [Header("按键设置")]
    public SteamVR_Action_Boolean toggleAction; 
    public SteamVR_Input_Sources handType = SteamVR_Input_Sources.LeftHand; 

    [Header("核心节点分配 (拆分职责)")]
    [Tooltip("负责移动和旋转的外壳容器")]
    public Transform clipboardAnchor;     // 专门负责对准玩家的外壳
    [Tooltip("负责显示隐藏的实际模型")]
    public GameObject clipboardVisual;    // 专门负责显隐的内部写字板
    public ParticleSystem summonVFX;      // 粒子特效

    [Header("悬浮生成设置")]
    public Transform playerHead;       
    public float spawnDistance = 0.5f; 
    public float heightOffset = -0.2f; 

    private bool isShowing = false;

    void Start()
    {
        // 初始时只隐藏内部模型，保持外壳和特效节点存活
        if (clipboardVisual != null)
            clipboardVisual.SetActive(false);
    }

    void Update()
    {
        if (toggleAction.GetStateDown(handType))
        {
            ToggleClipboard();
        }
    }

    void ToggleClipboard()
    {
        isShowing = !isShowing;

        if (isShowing && playerHead != null && clipboardAnchor != null)
        {
            // 1. 计算悬浮目标位置
            Vector3 forwardDir = playerHead.forward;
            forwardDir.y = 0; 
            forwardDir.Normalize();

            Vector3 targetPos = playerHead.position + forwardDir * spawnDistance;
            targetPos.y = playerHead.position.y + heightOffset; 
            
            // 2. 移动和旋转【外壳】 (这样内部模型的 90 度修正就完美保留了)
            clipboardAnchor.position = targetPos;
            Vector3 lookAtPos = playerHead.position;
            lookAtPos.y = targetPos.y; 
            clipboardAnchor.LookAt(lookAtPos);
        }

        // 3. 独立控制【内部模型】的显隐
        if (clipboardVisual != null)
        {
            clipboardVisual.SetActive(isShowing);
        }

        // 4. 触发特效 (外壳一直处于 Active 状态，所以特效可以随时响应 Play 指令)
        if (summonVFX != null)
        {
            summonVFX.Stop(); 
            summonVFX.Play(); 
        }
    }
}