using System.Collections; // 必须引入这个来使用协程
using UnityEngine;
using Valve.VR;

public class RightController : MonoBehaviour
{
    public Transform realController;           
    public SteamVR_Input_Sources handType;     

    private float lastHapticTime = 0f; 

    void FixedUpdate()
    {
        transform.position = realController.position;
        transform.rotation = realController.rotation;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            if (Time.time - lastHapticTime > 0.5f) // 延长冷却，覆盖两次震动的总时间
            {
                lastHapticTime = Time.time;
                
                // 触发协程，由 Unity 来数秒，而不是丢给 SteamVR
                StartCoroutine(DoubleHapticRoutine());
            }
        }
    }

    // 新增：用协程控制的“双连震”
    IEnumerator DoubleHapticRoutine()
    {
        // 第一下（立刻震）：延迟0, 时长0.03, 频率150, 强度1
        SteamVR_Actions.default_Haptic.Execute(0f, 0.03f, 150f, 1.0f, handType);
        
        // Unity 强制等待 0.15 秒（马达彻底停转，且不依赖 SteamVR 队列）
        yield return new WaitForSeconds(0.15f); 
        
        // 第二下（等待结束后再发新指令）：延迟0, 时长0.03, 频率150, 强度0.6
        SteamVR_Actions.default_Haptic.Execute(0f, 0.03f, 150f, 0.6f, handType);
    }
}
