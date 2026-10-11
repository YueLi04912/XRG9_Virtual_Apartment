using System.Collections;
using UnityEngine;

public class SafeQuitManager : MonoBehaviour
{
    [Header("退出快捷键 (供 Facilitator 使用)")]
    [SerializeField] private KeyCode quitKey = KeyCode.Escape;

    private bool isQuitting = false;

    private void Update()
    {
        // 引导员按下 Esc 触发安全退出
        if (Input.GetKeyDown(quitKey) && !isQuitting)
        {
            StartCoroutine(SafeQuitRoutine());
        }
    }

    private IEnumerator SafeQuitRoutine()
    {
        isQuitting = true;
        Debug.Log("[SafeQuit] 审查结束，正在导出数据报表...");

        // 直接通过类名调用静态方法，无需实例变量
        AnnotationExporter.ExportSessionCSV();

        // 等待当前帧完成，确保文件流完全刷入物理磁盘
        yield return new WaitForEndOfFrame();

        Debug.Log("[SafeQuit] 数据已落盘，正在关闭程序...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnApplicationQuit()
    {
        // 拦截 Alt+F4 或窗口关闭按钮
        if (!isQuitting)
        {
            Debug.Log("[SafeQuit] 检测到外部强制关闭信号，紧急执行数据保存！");
            AnnotationExporter.ExportSessionCSV();
        }
    }
}