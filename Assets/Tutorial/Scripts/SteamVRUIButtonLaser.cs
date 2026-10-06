using UnityEngine;
using UnityEngine.UI;
using Valve.VR.Extras;

public class SteamVRUIButtonLaser : MonoBehaviour
{
    private SteamVR_LaserPointer laserPointer;

    private void Awake()
    {
        laserPointer = GetComponent<SteamVR_LaserPointer>();
    }

    private void OnEnable()
    {
        if (laserPointer != null)
            laserPointer.PointerClick += OnPointerClick;
    }

    private void OnDisable()
    {
        if (laserPointer != null)
            laserPointer.PointerClick -= OnPointerClick;
    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        if (e.target == null)
            return;

        Button button = e.target.GetComponentInParent<Button>();

        if (button != null && button.IsInteractable())
        {
            button.onClick.Invoke();
            Debug.Log("VR UI clicked: " + button.name);
        }
    }
}