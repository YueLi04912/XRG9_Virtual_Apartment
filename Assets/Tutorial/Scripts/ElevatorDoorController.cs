using System.Collections;
using UnityEngine;

public class ElevatorDoorController : MonoBehaviour
{
    [Header("Doors")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Local Opening Offsets")]
    public Vector3 leftOpenOffset;
    public Vector3 rightOpenOffset;

    [Header("Timing")]
    [Min(0.01f)]
    public float openDuration = 1.5f;

    private bool openingOrOpen;

    public void OpenDoors()
    {
        if (openingOrOpen)
            return;

        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogWarning("Assign both elevator doors.", this);
            return;
        }

        openingOrOpen = true;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector3 leftStart = leftDoor.localPosition;
        Vector3 rightStart = rightDoor.localPosition;

        Vector3 leftEnd = leftStart + leftOpenOffset;
        Vector3 rightEnd = rightStart + rightOpenOffset;

        float duration = Mathf.Max(0.01f, openDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f, Mathf.Clamp01(elapsed / duration)
            );

            leftDoor.localPosition =
                Vector3.Lerp(leftStart, leftEnd, t);

            rightDoor.localPosition =
                Vector3.Lerp(rightStart, rightEnd, t);

            yield return null;
        }

        leftDoor.localPosition = leftEnd;
        rightDoor.localPosition = rightEnd;
    }
}