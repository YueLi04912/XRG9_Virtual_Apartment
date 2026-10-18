using UnityEngine;
using Valve.VR;
using TMPro;
using Whisper;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class MarkerTool : MonoBehaviour
{
    [Header("按键绑定")]
    public SteamVR_Action_Boolean interactAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
    public SteamVR_Action_Boolean switchColorAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
    public SteamVR_Action_Boolean toggleModeAction; 
    public SteamVR_Action_Boolean secondaryAction;  
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

    // === 录音与删除系统 ===
    private FeedbackMarker activeRecordingMarker = null; 
    private FeedbackMarker activeDeleteMarker = null; 
    private float deleteTimer = 0f;
    private const float DELETE_THRESHOLD = 1.5f; 

    // === 终极方案：多路并发监听系统 ===
    private class MicTracker
    {
        public string deviceName;
        public AudioClip clip;
        public int startPos;
    }
    private List<MicTracker> activeMics = new List<MicTracker>();

    void Start()
    {
        laser = GetComponent<LineRenderer>();
        laser.startWidth = 0.005f;
        laser.endWidth = 0.005f;
        laser.enabled = false;
        if(wristUI != null) wristUI.SetActive(false);
        
        smoothedRotation = transform.rotation;
        UpdateLaserColor();
        InitOmniMicrophones(); 
    }

    // 同时开启所有相关的 VR 虚拟麦克风通道
    void InitOmniMicrophones()
    {
        activeMics.Clear();
        if (Microphone.devices.Length == 0) return;

        HashSet<string> targetMics = new HashSet<string>();
        foreach (var dev in Microphone.devices)
        {
            string lower = dev.ToLower();
            if (lower.Contains("oculus") || lower.Contains("virtual") || lower.Contains("steam"))
            {
                targetMics.Add(dev);
            }
        }
        // 如果都没匹配到，加入系统默认麦克风兜底
        if (targetMics.Count == 0) targetMics.Add(Microphone.devices[0]); 

        foreach (var micName in targetMics)
        {
            try 
            {
                // 获取麦克风原生支持的采样率，防止 Oculus 驱动因为频率不匹配而输出纯 0.0f 的静音数据
                int minFreq, maxFreq;
                Microphone.GetDeviceCaps(micName, out minFreq, out maxFreq);
                
                // 如果 maxFreq 返回 0，说明驱动允许任意频率，我们优先给 Oculus 分配它的原生 48000Hz
                int recordFreq = (maxFreq == 0) ? 48000 : maxFreq; 

                AudioClip c = Microphone.Start(micName, true, 600, recordFreq);
                activeMics.Add(new MicTracker { deviceName = micName, clip = c, startPos = 0 });
                Debug.Log($"[并发收音] 开启全天候监听通道: {micName} | 采样率: {recordFreq}Hz");
            } 
            catch { Debug.LogWarning($"[并发收音] 无法启动通道: {micName}"); }
        }
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
                // 松开扳机时，调用波形仲裁提取最佳音频
                AudioClip extractedClip = StopAndExtractBestAudio();
                
                if (extractedClip != null && extractedClip.samples > 0)
                {
                    try
                    {
                        string savedFileName = AnnotationExporter.SaveAudioToWav(extractedClip, activeRecordingMarker.markerId);
                        activeRecordingMarker.audioFiles.Add(savedFileName);
                        activeRecordingMarker.SetAudio(extractedClip);
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
            laser.SetPosition(1, activeDeleteMarker.transform.position); 

            deleteTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(deleteTimer / DELETE_THRESHOLD);

            if (statusText != null)
            {
                int filledBlocks = Mathf.FloorToInt(progress * 10);
                string bar = new string('█', filledBlocks) + new string('░', 10 - filledBlocks);
                statusText.text = $"<color=#ffaa00>Deleting ID: #{activeDeleteMarker.markerId}\n{bar}</color>";
            }

            if (deleteTimer >= DELETE_THRESHOLD)
            {
                AnnotationExporter.DeleteAudioFiles(activeDeleteMarker.audioFiles);
                Destroy(activeDeleteMarker.gameObject);
                activeDeleteMarker = null;
                UpdateUI();
                return;
            }

            if (secondaryAction != null && secondaryAction.GetStateUp(handType))
            {
                activeDeleteMarker = null; 
                UpdateUI(); 
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
                
                if (interactAction.GetStateDown(handType))
                {
                    activeRecordingMarker = hitMarker;
                    // 按下扳机时，让所有麦克风通道同时打点标记起始位置
                    foreach (var mic in activeMics)
                    {
                        mic.startPos = Microphone.GetPosition(mic.deviceName);
                    }
                    if (statusText != null) statusText.text = $"<color=red>● Recording (ID: #{activeRecordingMarker.markerId})...</color>";
                }
                else if (secondaryAction != null && secondaryAction.GetStateDown(handType))
                {
                    activeDeleteMarker = hitMarker;
                    deleteTimer = 0f;
                    hitMarker.PlayAudio(); 
                }
            }
            else
            {
                if (interactAction.GetStateDown(handType)) SpawnMarker(hit);
            }
        }
        else
        {
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, transform.position + pointerDir * 10f);
        }
    }

    // === 核心逻辑：分析所有通道波形，提取物理振幅最大的真实音频 ===
    private AudioClip StopAndExtractBestAudio()
    {
        AudioClip bestClip = null;
        float highestAmplitude = -1f;
        string winningMicName = "None";
        float[] bestSamples = null;

        foreach (var mic in activeMics)
        {
            if (mic.clip == null) continue;
            
            int endPos = Microphone.GetPosition(mic.deviceName);
            int length = endPos - mic.startPos;
            if (length < 0) length += mic.clip.samples; 
            if (length <= 0) continue; 

            float[] samples = new float[length];
            if (mic.startPos + length <= mic.clip.samples)
                mic.clip.GetData(samples, mic.startPos);
            else
            {
                int tailLength = mic.clip.samples - mic.startPos;
                float[] tailData = new float[tailLength];
                mic.clip.GetData(tailData, mic.startPos);
                int headLength = length - tailLength;
                float[] headData = new float[headLength];
                mic.clip.GetData(headData, 0);
                tailData.CopyTo(samples, 0);
                headData.CopyTo(samples, tailLength);
            }

            // 寻找当前通道的波形峰值
            float currentMaxAmp = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                if (Mathf.Abs(samples[i]) > currentMaxAmp) 
                    currentMaxAmp = Mathf.Abs(samples[i]);
            }

            // 如果这个通道捕捉到了更大的声音（排除空载的纯0.0f）
            if (currentMaxAmp > highestAmplitude)
            {
                highestAmplitude = currentMaxAmp;
                bestSamples = samples;
                winningMicName = mic.deviceName;
            }
        }

        // 阈值保护：如果所有麦克风录到的最大峰值都小于 0.005f，说明全是在录空气/底噪
        if (bestSamples != null && highestAmplitude > 0.005f)
        {
            Debug.Log($"<color=green>[波形仲裁] 胜出通道: {winningMicName} | 声音峰值: {highestAmplitude}</color>");
            
            // 对最终胜出的真实录音，执行自适应音量拉满补偿 (Auto-Gain)
            float targetVolume = 0.95f; 
            float dynamicBoost = targetVolume / highestAmplitude; 
            for (int i = 0; i < bestSamples.Length; i++) 
            {
                bestSamples[i] *= dynamicBoost;
            }

            bestClip = AudioClip.Create("MarkerAudio", bestSamples.Length, 1, 44100, false);
            bestClip.SetData(bestSamples, 0);
        }
        else
        {
            Debug.LogWarning("[波形仲裁] 未检测到有效输入，或声音太小被判定为静音。");
        }

        return bestClip;
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
        foreach (var mic in activeMics)
        {
            if (mic.clip != null && Microphone.IsRecording(mic.deviceName))
                Microphone.End(mic.deviceName);
        }
        //AnnotationExporter.ExportSessionCSV();
    }

    private async void ProcessSpeechToText(FeedbackMarker marker, AudioClip clip)
    {
        if (whisperManager == null) return;
        Debug.Log($"[STT] 开始后台处理小球 #{marker.markerId} 的语音识别...");
        try
        {
            var result = await whisperManager.GetTextAsync(clip);
            string recognizedText = result.Result; 
            if (marker != null && !string.IsNullOrEmpty(recognizedText))
            {
                marker.transcribedTexts.Add(recognizedText);
                Debug.Log($"[STT] 小球 #{marker.markerId} 识别成功: {recognizedText}");
            }
        }
        catch (System.Exception e) { Debug.LogError($"[STT] 识别失败: {e.Message}"); }
    }
}