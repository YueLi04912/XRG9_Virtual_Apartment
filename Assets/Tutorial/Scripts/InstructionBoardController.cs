using UnityEngine;

public class InstructionBoardController : MonoBehaviour
{
    public void CloseBoard()
    {
        gameObject.SetActive(false);
    }
}