
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class TutorialFlowManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialData
    {
        [TextArea(2, 3)]
        public string title;

        public Sprite image;
    }

    [Header("Pages")]
    public GameObject welcomePage;
    public GameObject instructionBoard;
    public GameObject tutorialPage;
    public GameObject finishPage;

    [Header("Tutorial Content")]
    public TMP_Text tutorialTitle;
    public Image tutorialImage;
    public TMP_Text pageNumberText;

    public TutorialData[] tutorials = new TutorialData[6];

    [Header("Buttons")]
    public Button startButton;
    public Button previousButton;
    public Button nextButton;
    public Button finishButton;

    [Header("Elevator Door Event")]
    public UnityEvent onTutorialFinished;

    private int currentPage = 0;
    private bool hasStarted = false;
    private bool tutorialFinished = false;

    private void Awake()
    {
        // Register button events once.
        if (startButton != null)
            startButton.onClick.AddListener(StartTutorial);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (finishButton != null)
            finishButton.onClick.AddListener(FinishTutorial);
    }

    private void Start()
    {
        hasStarted = false;
        tutorialFinished = false;
        currentPage = 0;

        // Initial state: welcome page only.
        welcomePage.SetActive(true);
        instructionBoard.SetActive(false);
        finishPage.SetActive(false);

        if (tutorials == null || tutorials.Length == 0)
        {
            Debug.LogWarning(
                "TutorialFlowManager: No tutorial pages configured.");
        }
    }

    // Called when the player clicks START.
    public void StartTutorial()
    {
        if (hasStarted || tutorialFinished)
            return;

        if (tutorials == null || tutorials.Length == 0)
        {
            Debug.LogWarning("No tutorial pages available.");
            return;
        }

        hasStarted = true;
        currentPage = 0;

        welcomePage.SetActive(false);
        finishPage.SetActive(false);
        instructionBoard.SetActive(true);

        ShowPage(currentPage);

        Debug.Log("Tutorial started.");
    }

    // Update the content of the single tutorial page.
    private void ShowPage(int index)
    {
        if (!hasStarted)
            return;

        if (index < 0 || index >= tutorials.Length)
            return;

        currentPage = index;

        if (pageNumberText != null)
        {
            pageNumberText.text =
                (currentPage + 1) + " / " + tutorials.Length;
        }

        instructionBoard.SetActive(true);
        tutorialPage.SetActive(true);
        finishPage.SetActive(false);

        TutorialData data = tutorials[currentPage];

        if (tutorialTitle != null)
            tutorialTitle.text = data.title;

        if (tutorialImage != null)
        {
            tutorialImage.sprite = data.image;

            // Image can be assigned later.
            // Keep the image area available for swipe input.
            tutorialImage.color =
                data.image == null
                ? Color.white
                : Color.white;

            tutorialImage.preserveAspect = true;
        }

        if (previousButton != null)
            previousButton.interactable = currentPage > 0;

        if (nextButton != null)
            nextButton.interactable = true;

        Debug.Log(
            "Showing tutorial page " +
            (currentPage + 1) + "/" + tutorials.Length);
    }

    // Right arrow or swipe left.
    public void NextPage()
    {
        Debug.Log("NextButton click received!");

        if (!hasStarted || !tutorialPage.activeInHierarchy)
            return;

        if (currentPage < tutorials.Length - 1)
        {
            ShowPage(currentPage + 1);
        }
        else
        {
            ShowFinishPage();
        }
    }

    // Left arrow or swipe right.
    public void PreviousPage()
    {
        if (!hasStarted || !tutorialPage.activeInHierarchy)
            return;

        if (currentPage > 0)
        {
            ShowPage(currentPage - 1);
        }
    }

    // Hide the large board and display the small finish page.
    private void ShowFinishPage()
    {
        tutorialPage.SetActive(false);
        instructionBoard.SetActive(false);

        finishPage.SetActive(true);

        Debug.Log("Tutorial completed. Finish page displayed.");
    }

    // Called when player clicks FINISH.
    public void FinishTutorial()
    {
        if (!hasStarted || tutorialFinished)
            return;

        if (!finishPage.activeInHierarchy)
            return;

        tutorialFinished = true;
        hasStarted = false;

        finishPage.SetActive(false);
        instructionBoard.SetActive(false);

        // Will call the elevator door controller
        // after it is connected in the Inspector.
        onTutorialFinished?.Invoke();

        Debug.Log(
            "FINISH clicked. Elevator door event triggered.");
    }

    private void OnDestroy()
    {
        // Remove only listeners registered by this script.
        if (startButton != null)
            startButton.onClick.RemoveListener(StartTutorial);

        if (previousButton != null)
            previousButton.onClick.RemoveListener(PreviousPage);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);

        if (finishButton != null)
            finishButton.onClick.RemoveListener(FinishTutorial);
    }
}
