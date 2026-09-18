using UnityEngine;
using Valve.VR;
using TMPro;
using Whisper;

[RequireComponent(typeof(LineRenderer))]
public class MarkerTool : MonoBehaviour
{
    [Header("按键绑定")]
    public SteamVR_Action_Boolean interactAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI"); // 扳机键
    public SteamVR_Action_Boolean switchColorAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip"); // 握把键
    public SteamVR_Action_Boolean toggleModeAction; // A 键开关
    public SteamVR_Action_Boolean secondaryAction;  // 新增：B 键 (播放/长按删除)
    public SteamVR_Input_Sources handType = SteamVR_Input_Sources.RightHand;
    
    [Header("模式与预制件")]
    public GameObject markerPrefab;
    public float offsetDistance = 0.12f;
    private bool isAnnotationMode = false; 

    [Header("Alyx 风格手腕 UI")]
    public GameObject wristUI; 
    public TextMeshProUGUI statusText;    

    [Header("防抖设置")]
    public float smoothSpeed = 15f; 

    [Header("语音识别")]
    public WhisperManager whisperManager;
    private Quaternion smoothedRotation;

    private LineRenderer laser;
    private Color[] colors = { Color.green, Color.yellow, Color.red };
    private string[] priorities = { "Low", "Medium", "High" };
    private int currentIndex = 0;

    private static int globalMarkerCounter = 0;

    // === 录音系统 ===
    private string micDeviceName;
    private AudioClip continuousMicClip;
    private int recordStartPos;
    private FeedbackMarker activeRecordingMarker = null; 

    // === 新增：删除/播放系统 ===
    private FeedbackMarker activeDeleteMarker = null; // 当前正在长按准备删除的小球
    private float deleteTimer = 0f;
    private const float DELETE_THRESHOLD = 1.5f; // 长按 1.5 秒后删除

    void Start()
    {
        laser = GetComponent<LineRenderer>();
        laser.startWidth = 0.005f;
        laser.endWidth = 0.005f;
        laser.enabled = false;
        if(wristUI != null) wristUI.SetActive(false);
        
        smoothedRotation = transform.rotation;
        UpdateLaserColor();
        InitMicrophone(); 
    }

    void InitMicrophone()
    {
        if (Microphone.devices.Length == 0) return;
        micDeviceName = null;
        foreach (var device in Microphone.devices)
            if (device.ToLower().Contains("steam")) { micDeviceName = device; break; }
        
        if (string.IsNullOrEmpty(micDeviceName))
            foreach (var device in Microphone.devices)
                if (device.ToLower().Contains("oculus") || device.ToLower().Contains("virtual")) { micDeviceName = device; break; }
                
        if (string.IsNullOrEmpty(micDeviceName)) micDeviceName = Microphone.devices[0];

        continuousMicClip = Microphone.Start(micDeviceName, true, 600, 44100);
    }

    void Update()
    {
        if (toggleModeAction != null && toggleModeAction.GetStateDown(handType))
        {
            isAnnotationMode = !isAnnotationMode;
            laser.enabled = isAnnotationMode;
            if(wristUI != null) wristUI.SetActive(isAnnotationMode);
            if (isAnnotationMode) UpdateUI();
        }

        if (!isAnnotationMode) return; 

        if (switchColorAction.GetStateDown(handType) && activeRecordingMarker == null && activeDeleteMarker == null)
        {
            currentIndex = (currentIndex + 1) % colors.Length;
            UpdateLaserColor();
            UpdateUI(); 
        }

        smoothedRotation = Quaternion.Slerp(smoothedRotation, transform.rotation, Time.deltaTime * smoothSpeed);
        Vector3 pointerDir = Quaternion.AngleAxis(40f, smoothedRotation * Vector3.right) * (smoothedRotation * Vector3.forward);
        RaycastHit hit;

        // ==========================================
        // 1. 录音焦点锁定逻辑 (扳机键)
        // ==========================================
        if (activeRecordingMarker != null)
        {
            activeRecordingMarker.OnHoverEnter(); 
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, activeRecordingMarker.transform.position); 

            if (interactAction.GetStateUp(handType))
            {
                AudioClip extractedClip = StopAndExtractAudio();
                if (extractedClip != null && extractedClip.samples > 0)
                {
                    try
                    {
                        string savedFileName = AnnotationExporter.SaveAudioToWav(extractedClip, activeRecordingMarker.markerId);
                        activeRecordingMarker.audioFiles.Add(savedFileName);
                        
                        // 【修改点】：只存入音频，不再调用 Play() 自动播放
                        activeRecordingMarker.SetAudio(extractedClip);

                        // 【核心新增】：触发后台 AI 识别任务
                        ProcessSpeechToText(activeRecordingMarker, extractedClip);
                    }
                    catch (System.Exception e) { Debug.LogError($"[导出失败]: {e.Message}"); }
                }
                activeRecordingMarker = null; 
                UpdateUI(); 
            }
            return; 
        }

        // ==========================================
        // 2. 播放与删除锁定逻辑 (B键长按)
        // ==========================================
        if (activeDeleteMarker != null)
        {
            activeDeleteMarker.OnHoverEnter(); 
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, activeDeleteMarker.transform.position); // 射线死死锁住小球

            deleteTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(deleteTimer / DELETE_THRESHOLD);

            // 绘制精美的全息进度条
            if (statusText != null)
            {
                int filledBlocks = Mathf.FloorToInt(progress * 10);
                string bar = new string('█', filledBlocks) + new string('░', 10 - filledBlocks);
                statusText.text = $"<color=#ffaa00>Deleting ID: #{activeDeleteMarker.markerId}\n{bar}</color>";
            }

            // 进度条满了，彻底销毁小球
            if (deleteTimer >= DELETE_THRESHOLD)
            {
                AnnotationExporter.DeleteAudioFiles(activeDeleteMarker.audioFiles);

                Destroy(activeDeleteMarker.gameObject);
                activeDeleteMarker = null;
                UpdateUI();
                return;
            }

            // 如果松开了 B 键 (只是短按播放，没有长按到底)
            if (secondaryAction != null && secondaryAction.GetStateUp(handType))
            {
                activeDeleteMarker = null; // 解除锁定
                UpdateUI(); // 恢复正常 UI
            }
            return;
        }

        // ==========================================
        // 3. 正常射线检测
        // ==========================================
        if (Physics.Raycast(transform.position, pointerDir, out hit, 10f))
        {
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, hit.point);

            FeedbackMarker hitMarker = hit.collider.GetComponent<FeedbackMarker>();
            if (hitMarker != null)
            {
                hitMarker.OnHoverEnter();
                
                // 按下扳机：开始录音
                if (interactAction.GetStateDown(handType))
                {
                    activeRecordingMarker = hitMarker;
                    recordStartPos = Microphone.GetPosition(micDeviceName);
                    if (statusText != null) statusText.text = $"<color=red>● Recording (ID: #{activeRecordingMarker.markerId})...</color>";
                }
                // 新增：按下 B 键：立即播放声音，并开启长按删除锁定
                else if (secondaryAction != null && secondaryAction.GetStateDown(handType))
                {
                    activeDeleteMarker = hitMarker;
                    deleteTimer = 0f;
                    hitMarker.PlayAudio(); // 一按下就出声音
                }
            }
            else
            {
                // 指向空地：生成新小球
                if (interactAction.GetStateDown(handType)) SpawnMarker(hit);
            }
        }
        else
        {
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, transform.position + pointerDir * 10f);
        }
    }

    private AudioClip StopAndExtractAudio()
    {
        if (continuousMicClip == null) return null;
        int recordEndPos = Microphone.GetPosition(micDeviceName);
        int length = recordEndPos - recordStartPos;
        if (length < 0) length += continuousMicClip.samples; 
        if (length == 0) return null;

        float[] samples = new float[length];
        if (recordStartPos + length <= continuousMicClip.samples)
            continuousMicClip.GetData(samples, recordStartPos);
        else
        {
            int tailLength = continuousMicClip.samples - recordStartPos;
            float[] tailData = new float[tailLength];
            continuousMicClip.GetData(tailData, recordStartPos);
            int headLength = length - tailLength;
            float[] headData = new float[headLength];
            continuousMicClip.GetData(headData, 0);
            tailData.CopyTo(samples, 0);
            headData.CopyTo(samples, tailLength);
        }

        float boostFactor = 4.0f; 
        float maxSampleValue = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= boostFactor;
            if (Mathf.Abs(samples[i]) > maxSampleValue) maxSampleValue = Mathf.Abs(samples[i]);
        }
        if (maxSampleValue > 1.0f)
        {
            float reduceFactor = 1.0f / maxSampleValue;
            for (int i = 0; i < samples.Length; i++) samples[i] *= reduceFactor;
        }

        AudioClip extracted = AudioClip.Create("MarkerAudio", length, 1, 44100, false);
        extracted.SetData(samples, 0);
        return extracted;
    }

    private void SpawnMarker(RaycastHit hit)
    {
        Vector3 spawnPosition = hit.point + hit.normal * offsetDistance;
        GameObject newMarker = Instantiate(markerPrefab, spawnPosition, Quaternion.identity);
        globalMarkerCounter++;
        FeedbackMarker markerScript = newMarker.GetComponent<FeedbackMarker>();
        markerScript.InitData(globalMarkerCounter, colors[currentIndex], priorities[currentIndex]);
    }

    private void UpdateLaserColor()
    {
        laser.material.color = colors[currentIndex];
    }

    private void UpdateUI()
    {
        if (statusText != null)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(colors[currentIndex]);
            statusText.text = $"Mode: Annotation\nPriority: <color=#{colorHex}>{priorities[currentIndex]}</color>";
        }
    }

    private void OnApplicationQuit()
    {
        if (continuousMicClip != null && Microphone.IsRecording(micDeviceName))
            Microphone.End(micDeviceName);
        AnnotationExporter.ExportSessionCSV();
    }

    // === 全新的后台 AI 语音识别任务 ===
    private async void ProcessSpeechToText(FeedbackMarker marker, AudioClip clip)
    {
        if (whisperManager == null)
        {
            Debug.LogError("[STT] 缺少 WhisperManager 引用，请在 Inspector 中赋值！");
            return;
        }

        Debug.Log($"[STT] 开始后台处理小球 #{marker.markerId} 的语音识别...");

        try
        {
            // 直接将音频丢给本地 AI 模型，不卡顿主线程
            var result = await whisperManager.GetTextAsync(clip);
            string recognizedText = result.Result; 

            if (marker != null && !string.IsNullOrEmpty(recognizedText))
            {
                // 将真实的文字塞进小球的记录列表里
                marker.transcribedTexts.Add(recognizedText);
                Debug.Log($"[STT] 小球 #{marker.markerId} 识别成功: {recognizedText}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[STT] 识别失败: {e.Message}");
        }
    }
}