using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] DotGrid grid;
    [SerializeField] Dot previewDot;

    void Start()
    {
        Application.targetFrameRate = 60;

        grid.eventDotsChainUpdated += OnDotsChainUpdated;
    }

    void OnDotsChainUpdated(Dot dot, int dotsCount)
    {
        int newValue = dot.Value + DotGrid.GetChainValue(dotsCount);
        previewDot.Value = dotsCount > 1 ? newValue : dot.Value;
        previewDot.gameObject.SetActive(dotsCount > 0);
    }

}
