using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class FeedbackMarker : MonoBehaviour
{
    [Header("Data Parameters")]
    public int markerId;
    public string priorityText;
    
    public List<string> audioFiles = new List<string>(); 
    private List<AudioClip> memoryAudioClips = new List<AudioClip>();

    [Header("UI Reference")]
    public TextMeshProUGUI idTextLabel; 

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isHovered = false;
    
    private Material mat;
    private Color baseColor;
    private float floatTimer;
    private AudioSource audioSource;
    private Camera mainCam;

    public List<string> transcribedTexts = new List<string>();

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position; 
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        mat = GetComponent<Renderer>().material;
        mat.EnableKeyword("_EMISSION");
        
        mainCam = Camera.main; 
    }

    public void InitData(int id, Color color, string priority)
    {
        markerId = id;
        priorityText = priority;

        baseColor = color;
        if(mat == null) mat = GetComponent<Renderer>().material;
        mat.color = baseColor;
        mat.SetColor("_EmissionColor", baseColor * 0.5f); 
        mat.EnableKeyword("_EMISSION");

        // 初始化时刷新一遍文字
        UpdateTextDisplay();
    }

    void Update()
    {
        float targetScaleMult = isHovered ? 1.5f : 1.0f;
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale * targetScaleMult, Time.deltaTime * 12f);

        Color targetGlow = isHovered ? baseColor * 2.5f : baseColor * 0.5f; 
        mat.SetColor("_EmissionColor", Color.Lerp(mat.GetColor("_EmissionColor"), targetGlow, Time.deltaTime * 12f));

        floatTimer += Time.deltaTime;
        float floatOffset = Mathf.Sin(floatTimer * 3f) * 0.01f;
        transform.position = originalPosition + Vector3.up * floatOffset;

        isHovered = false; 

        if (idTextLabel != null && mainCam != null)
        {
            Vector3 lookDirection = idTextLabel.transform.position - mainCam.transform.position;
            idTextLabel.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    public void OnHoverEnter()
    {
        isHovered = true;
    }

    // === 新增：统一的文字刷新方法 ===
    private void UpdateTextDisplay()
    {
        if (idTextLabel != null)
        {
            if (memoryAudioClips.Count == 0)
            {
                // 没有录音时，只显示纯白色的编号
                idTextLabel.text = $"#{markerId}";
            }
            else
            {
                // 有录音时，加上亮绿色的 [X Audio] 标签，并把这部分字号缩放到 70% 增加层次感
                idTextLabel.text = $"#{markerId} <size=70%><color=#55ff55>[{memoryAudioClips.Count} Audio]</color></size>";
            }
        }
    }

    public void SetAudio(AudioClip clip)
    {
        if (clip != null)
        {
            memoryAudioClips.Add(clip);
            // 录音追加成功后，立刻刷新文字状态
            UpdateTextDisplay();
        }
    }

    public void PlayAudio()
    {
        if (memoryAudioClips.Count == 0 || audioSource.isPlaying) return;
        StartCoroutine(PlayAllClipsRoutine());
    }

    private IEnumerator PlayAllClipsRoutine()
    {
        for (int i = 0; i < memoryAudioClips.Count; i++)
        {
            audioSource.clip = memoryAudioClips[i];
            audioSource.Play();

            if (idTextLabel != null)
            {
                // 播放时变成黄色提示当前进度 (全英文)
                idTextLabel.text = $"#{markerId} <color=yellow>[Playing {i + 1}/{memoryAudioClips.Count}]</color>";
            }

            yield return new WaitForSeconds(audioSource.clip.length + 0.3f);
        }

        // 播放结束后，重新恢复绿色的 [X Audio] 待机状态
        UpdateTextDisplay();
    }
}