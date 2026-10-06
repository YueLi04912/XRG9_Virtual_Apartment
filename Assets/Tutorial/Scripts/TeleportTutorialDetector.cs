using UnityEngine;

public class TeleportTutorialDetector : MonoBehaviour
{
    public TutorialManager tutorialManager;
    public GameObject teleportPointer;

    private bool completed = false;

    private void Update()
    {
        if (completed)
            return;

        if (teleportPointer != null && teleportPointer.activeInHierarchy)
        {
            completed = true;

            if (tutorialManager != null)
            {
                tutorialManager.CompleteTeleportPoint();
            }
        }
    }
}