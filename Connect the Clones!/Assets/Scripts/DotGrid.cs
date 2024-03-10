using System.Collections.Generic;
using UnityEngine;

public class DotGrid : MonoBehaviour
{
    [SerializeField] GameObject dotPrefab;
    [SerializeField] float dotsSpacing = 45f;

    public bool debugButton = false;

    private Dot[] dots;
    private List<Dot> dotsChain = new List<Dot>();
    private Dot mergeTargetDot = null; // Dot that is currently merget into
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
                
        // Spawn Dots
        {
            const int gridSize = Constants.GRID_SIZE;
            dots = new Dot[gridSize * gridSize];
            for (int i = 0; i < gridSize * gridSize; ++i)
            {
                Dot newDot = GameObject.Instantiate(dotPrefab, transform).GetComponent<Dot>();
                newDot.gridPosition.Set(i % gridSize, i / gridSize);
                newDot.eventPressed += OnDotPressed;
                newDot.eventReentered += OnDotReentered;
                dots[i] = newDot;
            }
            PositionDotsOnGrid();
            RandomizeGrid(1, 4);
        }

        InputManager.instance.eventPointerReleased += OnInputPointerReleased;
    }

    void Update()
    {
        if (debugButton)
        {
            debugButton = false;
            RandomizeGrid(1, 200);
        }
    }

    void PositionDotsOnGrid()
    {
        RectTransform dotRect = dots[0].GetComponent<RectTransform>();
        Vector2 spacing = new Vector2(dotRect.sizeDelta.x + dotsSpacing, dotRect.sizeDelta.y + dotsSpacing);
        Vector2 gridSize = new Vector2(spacing.x * (Constants.GRID_SIZE-1), spacing.y * (Constants.GRID_SIZE-1));
        Vector2 gridStartPosition = new Vector2(-gridSize.x / 2, gridSize.y / 2);
        
        for (int i = 0; i < dots.Length; ++i)
        {
            dots[i].transform.localPosition = new Vector3(
                gridStartPosition.x + dots[i].gridPosition.x * spacing.x,
                gridStartPosition.y - dots[i].gridPosition.y * spacing.y,
                0
            );
        }
    }

    void RandomizeGrid(int minValue, int maxValue)
    {
        int tries = 0;
        do
        {
            for (int i = 0; i < dots.Length; ++i)
            {
                dots[i].Value = Random.Range(minValue, maxValue+1);
            }
            tries ++;
        } while(IsBoardStuck());
    }

    bool IsBoardStuck()
    {
        for (int i = 0; i < dots.Length; ++i)
        {
            Vector2Int gridPos = dots[i].gridPosition;
            int currentValue = dots[i].Value;
            for(int r = gridPos.y - 1; r <= gridPos.y + 1; ++r)
            {
                for(int c = gridPos.x - 1; c <= gridPos.x + 1; ++c)
                {
                    if ((r == gridPos.y && c == gridPos.x) 
                        || r < 0 || r >= Constants.GRID_SIZE 
                        || c < 0 || c >= Constants.GRID_SIZE)
                    {
                        continue;
                    }

                    int neighbourIndex = r * Constants.GRID_SIZE + c;
                    if (dots[neighbourIndex].Value == currentValue)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    void OnDotPressed(Dot dot)
    {
        if (dot == mergeTargetDot)
        {
            return;
        }

        if (dotsChain.Count > 0)
        {
            Dot last = dotsChain[^1];
            if ((last.gridPosition - dot.gridPosition).magnitude > 1.5f)
            {
                return;
            }

            if (last.Value != dot.Value)
            {
                return;
            }
        }

        dot.SetPressed(true);
        if (dotsChain.Count == 0)
        {
            Color lineColor = dot.GetColor();
            lineRenderer.startColor = lineRenderer.endColor = lineColor;
        }
        dotsChain.Add(dot);
        lineRenderer.positionCount ++;
        lineRenderer.SetPosition(lineRenderer.positionCount-1, dot.transform.localPosition);
    }

    void OnDotReentered(Dot dot)
    {
        // detect going back the chain and remove the last dot if so
        int cc = dotsChain.Count;
        if (dotsChain.IndexOf(dot) == cc - 2)
        {
            dotsChain[cc-1].SetPressed(false);
            dotsChain.RemoveAt(cc-1);
            lineRenderer.positionCount --;
        }
    }

    void OnInputPointerReleased()
    {
        for (int i = 0; i < dots.Length; ++i)
        {
            dots[i].SetPressed(false);
        }
        lineRenderer.positionCount = 0;

        if (dotsChain.Count > 1)
        {
            mergeTargetDot = dotsChain[^1];
            mergeTargetDot.transform.SetAsLastSibling();
            for (int i = 0; i < dotsChain.Count - 1; ++i)
            {
                dotsChain[i].MergeWith(mergeTargetDot);
            }
            dotsChain[0].eventMergeFinished += OnDotsMergeFinished;
        }

        dotsChain.Clear();
    }

    void OnDotsMergeFinished(Dot dot)
    {
        dot.eventMergeFinished -= OnDotsMergeFinished;
        mergeTargetDot.Value ++;
        for (int i = 0; i < dots.Length; ++i)
        {
            if (dots[i].IsMerged())
            {
                dots[i].RespawnWithValue(Random.Range(1, 4));
            }
        }

        mergeTargetDot = null;
    }
}
