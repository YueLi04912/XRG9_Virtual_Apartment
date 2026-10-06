using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialFlowManager : MonoBehaviour
{
    public enum TutorialStep
    {
        TeleportPoint,
        Thumbstick,
        Trigger,
        Grip,
        Look,
        Marker

    }

    [Header("Popup")]
    public GameObject tutorialPopupPanel;
    public TMP_Text popupTitle;
    public Image popupImage;
    public TMP_Text popupHint;
    public Button completeButton;

    [Header("Tutorial Buttons")]
    public Button btnTeleportPoint;
    public Button btnThumbstick;
    public Button btnTrigger;
    public Button btnGrip;
    public Button btnLook;
    public Button btnMarker;

    [Header("Tutorial Texts")]
    public TMP_Text txtTeleportPoint;
    public TMP_Text txtThumbstick;
    public TMP_Text txtTrigger;
    public TMP_Text txtGrip;
    public TMP_Text txtLook;
    public TMP_Text txtMarker;

    [Header("Tutorial Images")]
    public Sprite imgTeleportPoint;
    public Sprite imgThumbstick;
    public Sprite imgTrigger;
    public Sprite imgGrip;
    public Sprite imgLook;
    public Sprite imgMarker;

    [Header("Start Button")]
    public Button startButton;
    public TMP_Text startButtonText;

    [Header("Colors")]
    public Color incompleteColor = Color.white;
    public Color completeColor = Color.green;
    public Color lockedColor = Color.gray;
    public Color readyColor = Color.green;

    private TutorialStep currentStep;

    private bool teleportDone = false;
    private bool thumbstickDone = false;
    private bool triggerDone = false;
    private bool gripDone = false;
    private bool lookDone = false;
    private bool markerDone = false;

    private void Start()
    {
        tutorialPopupPanel.SetActive(false);

        startButton.interactable = false;

        if (startButtonText != null)
            startButtonText.color = lockedColor;

        btnTeleportPoint.onClick.AddListener(
            () => OpenStep(TutorialStep.TeleportPoint));

        btnThumbstick.onClick.AddListener(
            () => OpenStep(TutorialStep.Thumbstick));

        btnTrigger.onClick.AddListener(
            () => OpenStep(TutorialStep.Trigger));

        btnGrip.onClick.AddListener(
            () => OpenStep(TutorialStep.Grip));

        btnLook.onClick.AddListener(
            () => OpenStep(TutorialStep.Look));

        btnMarker.onClick.AddListener(
            () => OpenStep(TutorialStep.Marker));

        completeButton.onClick.AddListener(CompleteCurrentStep);

        RefreshTutorialText();
    }

    public void OpenStep(TutorialStep step)
    {
        currentStep = step;

        tutorialPopupPanel.SetActive(true);

        switch (step)
        {
            case TutorialStep.TeleportPoint:
                popupTitle.text = "TELEPORT DESTINATION";
                popupHint.text =
                    "Point at the floor to choose a teleport destination.";
                popupImage.sprite = imgTeleportPoint;
                break;

            case TutorialStep.Thumbstick:
                popupTitle.text = "THUMBSTICK";
                popupHint.text =
                    "Use the thumbstick to teleport and turn.";
                popupImage.sprite = imgThumbstick;
                break;

            case TutorialStep.Trigger:
                popupTitle.text = "TRIGGER";
                popupHint.text =
                    "Press the trigger to select or confirm.";
                popupImage.sprite = imgTrigger;
                break;

            case TutorialStep.Grip:
                popupTitle.text = "GRIP";
                popupHint.text =
                    "Hold the grip button to grab objects.";
                popupImage.sprite = imgGrip;
                break;

            case TutorialStep.Look:
                popupTitle.text = "LOOK AROUND";
                popupHint.text =
                    "Move your head to look around the kitchen.";
                popupImage.sprite = imgLook;
                break;

            case TutorialStep.Marker:
                popupTitle.text = "MARKER & VOICE FEEDBACK";
                popupHint.text =
                    "Press A to enter marker mode.\n" +
                    "Aim at a location and press Trigger to place a marker.\n" +
                    "Point at the marker and hold Trigger to record voice feedback.\n" +
                    "Press B to play recorded feedback.";
                popupImage.sprite = imgMarker;
                break;

        }
    }

    public void CompleteCurrentStep()
    {
        switch (currentStep)
        {
            case TutorialStep.TeleportPoint:
                teleportDone = true;
                break;

            case TutorialStep.Thumbstick:
                thumbstickDone = true;
                break;

            case TutorialStep.Trigger:
                triggerDone = true;
                break;

            case TutorialStep.Grip:
                gripDone = true;
                break;

            case TutorialStep.Look:
                lookDone = true;
                break;

            case TutorialStep.Marker:
                markerDone = true;
                break;

        }

        tutorialPopupPanel.SetActive(false);

        RefreshTutorialText();
        CheckAllCompleted();
    }

    private void RefreshTutorialText()
    {
        SetText(
            txtTeleportPoint,
            teleportDone,
            "Point at the floor to choose a teleport destination");

        SetText(
            txtThumbstick,
            thumbstickDone,
            "Use the thumbstick to teleport and turn");

        SetText(
            txtTrigger,
            triggerDone,
            "Press the trigger to select or confirm");

        SetText(
            txtGrip,
            gripDone,
            "Hold the grip button to grab objects");

        SetText(
            txtLook,
            lookDone,
            "Move your head to look around the kitchen");

        SetText(
            txtMarker,
            markerDone,
            "Use the marker tool to leave feedback");

    }

    private void SetText(
        TMP_Text textObject,
        bool completed,
        string message)
    {
        if (textObject == null)
            return;

        if (completed)
        {
            textObject.text = message;
            textObject.color = completeColor;
        }
        else
        {
            textObject.text = message;
            textObject.color = incompleteColor;
        }
    }

    private void CheckAllCompleted()
    {
        if (teleportDone &&
            thumbstickDone &&
            triggerDone &&
            gripDone &&
            lookDone &&
            markerDone)
        {
            startButton.interactable = true;

            if (startButtonText != null)
                startButtonText.color = readyColor;

            Debug.Log("START TOUR unlocked!");
        }
    }

    public void StartTour()
    {
        Debug.Log("START TOUR clicked!");

        gameObject.SetActive(false);
    }
}