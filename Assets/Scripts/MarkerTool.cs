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

    [Header("交互音效")]
    public AudioSource sfxSource;
    public AudioClip switchColorClip;  // 切换颜色/优先级音效
    public AudioClip modeOnClip;       // 进入标记模式音效
    public AudioClip modeOffClip;      // 退出标记模式音效
    public AudioClip spawnMarkerClip;  // 放置标记球音效
    public AudioClip deletingLoopClip; // 长按删除进度条音效（自带渐进升调）
    public AudioClip deleteDoneClip;   // 满1.5秒删除成功音效
    public AudioClip recordStartClip;  // 按下扳机开始录音提示音
    public AudioClip recordStopClip;   // 松开扳机结束录音提示音

    [Header("语音识别")]
    public WhisperManager whisperManager;

    [Header("激光发射点设置")]
    [Tooltip("独立的激光发射点物体（可在手部下建子物体手动调位置和旋转；留空则默认使用当前脚本所在物体的位置）")]
    public Transform rayOrigin;
    [Tooltip("激光绕局部 X 轴的俯仰偏转角（度）。如果直接旋转 RayOrigin 物体来控制角度，可设为 0")]
    public float pitchOffsetAngle = 40f;
    [Tooltip("激光最大射程（米）")]
    public float maxRayDistance = 10f;

    private Quaternion smoothedRotation;

    private Transform Origin => rayOrigin != null ? rayOrigin : transform;

    private Vector3 GetPointerDirection(Quaternion baseRotation)
    {
        return Quaternion.AngleAxis(pitchOffsetAngle, baseRotation * Vector3.right) * (baseRotation * Vector3.forward);
    }

    private LineRenderer laser;
    private Color[] colors = { Color.green, Color.yellow, Color.red };
    private string[] priorities = { "Low", "Medium", "High" };
    private int currentIndex = 0;
    private static int globalMarkerCounter = 0;

    // === 录音与删除系统 ===
    private FeedbackMarker activeRecordingMarker = null; 
    private FeedbackMarker activeDeleteMarker = null; 
    private float deleteTimer = 0f;
    private bool hasStartedDeleteSfx = false;
    private const float TAP_THRESHOLD = 0.25f;    // 短按判定上限（秒）
    private const float DELETE_THRESHOLD = 1.5f;  // 长按删除总时长（秒）

    // === 多路并发监听系统 ===
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
        
        smoothedRotation = Origin.rotation;
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
        if (targetMics.Count == 0) targetMics.Add(Microphone.devices[0]); 

        foreach (var micName in targetMics)
        {
            try 
            {
                int minFreq, maxFreq;
                Microphone.GetDeviceCaps(micName, out minFreq, out maxFreq);
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
        // ==========================================
        // 0. 切换标记模式 (A键)
        // ==========================================
        if (toggleModeAction != null && toggleModeAction.GetStateDown(handType))
        {
            isAnnotationMode = !isAnnotationMode;
            laser.enabled = isAnnotationMode;
            if(wristUI != null) wristUI.SetActive(isAnnotationMode);
            if (isAnnotationMode) UpdateUI();

            if (sfxSource != null)
            {
                sfxSource.pitch = 1.0f;
                if (isAnnotationMode && modeOnClip != null)
                    sfxSource.PlayOneShot(modeOnClip);
                else if (!isAnnotationMode && modeOffClip != null)
                    sfxSource.PlayOneShot(modeOffClip);
            }
        }

        if (!isAnnotationMode) return; 

        // ==========================================
        // 0.5 切换优先级颜色 (Grip键)
        // ==========================================
        if (switchColorAction.GetStateDown(handType) && activeRecordingMarker == null && activeDeleteMarker == null)
        {
            currentIndex = (currentIndex + 1) % colors.Length;
            UpdateLaserColor();
            UpdateUI(); 

            if (sfxSource != null && switchColorClip != null)
            {
                sfxSource.pitch = 1.0f + currentIndex * 0.25f;
                sfxSource.PlayOneShot(switchColorClip);
            }
        }

        Vector3 originPos = Origin.position;
        smoothedRotation = Quaternion.Slerp(smoothedRotation, Origin.rotation, Time.deltaTime * smoothSpeed);
        Vector3 pointerDir = GetPointerDirection(smoothedRotation);
        RaycastHit hit;

        // ==========================================
        // 1. 录音焦点锁定逻辑 (扳机键)
        // ==========================================
        if (activeRecordingMarker != null)
        {
            activeRecordingMarker.OnHoverEnter(); 
            laser.SetPosition(0, originPos);
            laser.SetPosition(1, activeRecordingMarker.transform.position); 

            if (interactAction.GetStateUp(handType))
            {
                AudioClip extractedClip = StopAndExtractBestAudio();

                if (sfxSource != null && recordStopClip != null)
                {
                    sfxSource.pitch = 1.0f;
                    sfxSource.PlayOneShot(recordStopClip);
                }
                
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
        // 2. 播放与删除锁定逻辑 (B键短按播放 / 长按删除)
        // ==========================================
        if (activeDeleteMarker != null)
        {
            activeDeleteMarker.OnHoverEnter(); 
            laser.SetPosition(0, originPos);
            laser.SetPosition(1, activeDeleteMarker.transform.position); 

            deleteTimer += Time.deltaTime;

            // 超过短按阈值 (0.25s) 后，确认为长按删除状态，开始走进度条与渐进音效
            if (deleteTimer >= TAP_THRESHOLD)
            {
                if (!hasStartedDeleteSfx && sfxSource != null && deletingLoopClip != null)
                {
                    hasStartedDeleteSfx = true;
                    sfxSource.Stop();
                    sfxSource.pitch = 1.0f;
                    sfxSource.loop = false;
                    sfxSource.clip = deletingLoopClip;
                    sfxSource.Play();
                }

                float progress = Mathf.Clamp01((deleteTimer - TAP_THRESHOLD) / (DELETE_THRESHOLD - TAP_THRESHOLD));

                if (statusText != null)
                {
                    int filledBlocks = Mathf.FloorToInt(progress * 10);
                    string bar = new string('█', filledBlocks) + new string('░', 10 - filledBlocks);
                    statusText.text = $"<color=#ffaa00>Deleting ID: #{activeDeleteMarker.markerId}\n{bar}</color>";
                }
            }

            // 满 1.5 秒：完成删除
            if (deleteTimer >= DELETE_THRESHOLD)
            {
                if (sfxSource != null)
                {
                    if (sfxSource.clip == deletingLoopClip)
                    {
                        sfxSource.Stop();
                        sfxSource.clip = null;
                    }
                    sfxSource.pitch = 1.0f;
                    if (deleteDoneClip != null)
                        sfxSource.PlayOneShot(deleteDoneClip);
                }

                AnnotationExporter.DeleteAudioFiles(activeDeleteMarker.audioFiles);
                Destroy(activeDeleteMarker.gameObject);
                activeDeleteMarker = null;
                UpdateUI();
                return;
            }

            // 松开 B 键：区分短按与中途取消删除
            if (secondaryAction != null && secondaryAction.GetStateUp(handType))
            {
                if (sfxSource != null && sfxSource.clip == deletingLoopClip)
                {
                    sfxSource.Stop();
                    sfxSource.clip = null;
                }

                // 按住时长小于 0.25 秒 -> 判定为短按，播放录音
                if (deleteTimer < TAP_THRESHOLD)
                {
                    activeDeleteMarker.PlayAudio();
                }

                activeDeleteMarker = null; 
                UpdateUI(); 
            }
            return;
        }

        // ==========================================
        // 3. 正常射线检测
        // ==========================================
        if (Physics.Raycast(originPos, pointerDir, out hit, maxRayDistance))
        {
            laser.SetPosition(0, originPos);
            laser.SetPosition(1, hit.point);

            FeedbackMarker hitMarker = hit.collider.GetComponent<FeedbackMarker>();
            if (hitMarker != null)
            {
                hitMarker.OnHoverEnter();
                
                if (interactAction.GetStateDown(handType))
                {
                    activeRecordingMarker = hitMarker;
                    foreach (var mic in activeMics)
                    {
                        mic.startPos = Microphone.GetPosition(mic.deviceName);
                    }
                    if (statusText != null) statusText.text = $"<color=red>● Recording (ID: #{activeRecordingMarker.markerId})...</color>";

                    if (sfxSource != null && recordStartClip != null)
                    {
                        sfxSource.pitch = 1.0f;
                        sfxSource.PlayOneShot(recordStartClip);
                    }
                }
                else if (secondaryAction != null && secondaryAction.GetStateDown(handType))
                {
                    // 仅锁定目标并归零计时器，等待松开或长按阈值判定
                    activeDeleteMarker = hitMarker;
                    deleteTimer = 0f;
                    hasStartedDeleteSfx = false;
                }
            }
            else
            {
                if (interactAction.GetStateDown(handType)) SpawnMarker(hit);
            }
        }
        else
        {
            laser.SetPosition(0, originPos);
            laser.SetPosition(1, originPos + pointerDir * maxRayDistance);
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

            float currentMaxAmp = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                if (Mathf.Abs(samples[i]) > currentMaxAmp) 
                    currentMaxAmp = Mathf.Abs(samples[i]);
            }

            if (currentMaxAmp > highestAmplitude)
            {
                highestAmplitude = currentMaxAmp;
                bestSamples = samples;
                winningMicName = mic.deviceName;
            }
        }

        if (bestSamples != null && highestAmplitude > 0.005f)
        {
            Debug.Log($"<color=green>[波形仲裁] 胜出通道: {winningMicName} | 声音峰值: {highestAmplitude}</color>");
            
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

        if (sfxSource != null && spawnMarkerClip != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(spawnMarkerClip);
        }
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

    private void OnDrawGizmosSelected()
    {
        Transform origin = Origin;
        Gizmos.color = colors != null && colors.Length > 0 ? colors[currentIndex] : Color.green;
        Gizmos.DrawSphere(origin.position, 0.01f);
        Gizmos.DrawRay(origin.position, GetPointerDirection(origin.rotation) * 2f);
    }
}