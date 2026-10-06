using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Texts")]
    public TMP_Text teleportPointStep;
    public TMP_Text thumbstickStep;
    public TMP_Text triggerStep;
    public TMP_Text gripStep;
    public TMP_Text lookStep;

    [Header("Start Button")]
    public Button startButton;
    public TMP_Text startButtonText;

    [Header("Colors")]
    public Color incompleteColor = Color.white;
    public Color completeColor = Color.green;

    private bool teleportPointCompleted = false;
    private bool thumbstickCompleted = false;
    private bool triggerCompleted = false;
    private bool gripCompleted = false;
    private bool lookCompleted = false;

    private void Start()
    {
        // 游戏开始时，先不允许按 START TOUR
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        if (startButtonText != null)
        {
            startButtonText.color = Color.gray;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CompleteTeleportPoint();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CompleteThumbstick();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CompleteTrigger();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CompleteGrip();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CompleteLook();
        }
    }

    public void CompleteTeleportPoint()
    {
        if (teleportPointCompleted)
            return;

        teleportPointCompleted = true;

        teleportPointStep.text =
            "✓ Point at the floor to choose a teleport destination";

        teleportPointStep.color = completeColor;

        CheckAllCompleted();
    }

    public void CompleteThumbstick()
    {
        if (thumbstickCompleted)
            return;

        thumbstickCompleted = true;

        thumbstickStep.text =
            "✓ Use the thumbstick to teleport and turn";

        thumbstickStep.color = completeColor;

        CheckAllCompleted();
    }

    public void CompleteTrigger()
    {
        if (triggerCompleted)
            return;

        triggerCompleted = true;

        triggerStep.text =
            "✓ Press the trigger to select or confirm";

        triggerStep.color = completeColor;

        CheckAllCompleted();
    }

    public void CompleteGrip()
    {
        if (gripCompleted)
            return;

        gripCompleted = true;

        gripStep.text =
            "✓ Hold the grip button to grab objects";

        gripStep.color = completeColor;

        CheckAllCompleted();
    }

    public void CompleteLook()
    {
        if (lookCompleted)
            return;

        lookCompleted = true;

        lookStep.text =
            "✓ Move your head to look around the kitchen";

        lookStep.color = completeColor;

        CheckAllCompleted();
    }

    private void CheckAllCompleted()
    {
        if (teleportPointCompleted &&
            thumbstickCompleted &&
            triggerCompleted &&
            gripCompleted &&
            lookCompleted)
        {
            Debug.Log("All tutorial steps completed!");

            if (startButton != null)
            {
                startButton.interactable = true;
            }

            if (startButtonText != null)
            {
                startButtonText.color = Color.green;
            }
        }
    }
}