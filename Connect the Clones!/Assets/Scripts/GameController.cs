using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] DotGrid grid;
    [SerializeField] Dot previewDot;

    void Start()
    {
        Application.targetFrameRate = 60;

        grid.eventDotPressed += OnDotPressed;
        grid.eventDotsReleased += OnDotsReleased;
    }

    void OnDotPressed(Dot dot)
    {
        previewDot.Value = dot.Value;
        previewDot.gameObject.SetActive(true);
    }

    void OnDotsReleased()
    {
        previewDot.gameObject.SetActive(false);
    }
}
