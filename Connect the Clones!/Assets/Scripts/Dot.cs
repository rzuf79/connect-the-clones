using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Dot : MonoBehaviour
{
    public static readonly float MERGE_DURATION = 0.2f;
    private const int MAX_MINOR_VALUE = 9;
    private const string SUFFIXES = " KMBTABCDEFGHIJKLMNOPQRSTUVW";

    public enum State
    {
        Idle,
        Merging,
        Falling,
    }

    public event EventDelegate<Dot> eventPressed;
    public event EventDelegate<Dot> eventReentered;
    public event EventDelegate<Dot> eventMergeFinished;
    public event EventDelegate<Dot> eventSpawnAnimFinished;

    private int value;
    public int Value
    {
        get { return value; }
        set 
        {
            this.value = value;
            int minor = value % MAX_MINOR_VALUE;
            int major = value / MAX_MINOR_VALUE;

            imageDot.color = GetColor();
            imageFrame.gameObject.SetActive(major % 2 != 0);
            textValue.text = "" + Mathf.Pow(2, minor);
            if (major > 0)
            {
                textValue.text += SUFFIXES[major];
            }
        }
    }

    public Color[] valueColors;

    [HideInInspector] public Vector2Int gridPosition;
    
    [SerializeField] Image imageDot;
    [SerializeField] Image imageFrame;
    [SerializeField] TextMeshProUGUI textValue;

    private bool pressed = false;
    private State state = State.Idle;
    private Dot mergeTargetDot = null;
    private float mergeProgress = 0f;


    void Update()
    {
        if (mergeTargetDot != null && mergeProgress < 1f)
        {
            mergeProgress += Time.deltaTime / MERGE_DURATION;
            float v = mergeProgress;
            v = v * v * (3-2*v);
            imageDot.transform.position = Vector3.Lerp(transform.position, mergeTargetDot.transform.position, v);
            textValue.color = Color.Lerp(Color.white, new Color(1,1,1,.25f), v);
            if (mergeProgress >= 1f)
            {
                mergeProgress = 1f;
                eventMergeFinished.Fire(this);
            }
            return;
        }

        if (pressed)
        {
            imageDot.transform.localScale = Vector2.one * 1.1f;
        }
        else 
        {
            imageDot.transform.localScale = Vector2.one;
        }
    }

    public void SetPressed(bool value)
    {
        pressed = value;
    }

    public void RespawnWithValue(int value)
    {
        mergeTargetDot = null;
        textValue.color = Color.white;
        Value = value;
        imageDot.transform.position = transform.position;

        Tweener.RemoveTweensFromTransform(imageDot.transform);
        Tweener.AddTween(imageDot.transform, Tweener.Type.Scale, Vector3.zero, Vector3.one, .2f,
            Tweener.InterpolationType.EaseFrom, Tweener.RepeatMode.Once, 0);
    }

    public void AnimateIncrement()
    {
        Tweener.RemoveTweensFromTransform(imageDot.transform);
        Tweener.AddTween(imageDot.transform, Tweener.Type.Scale, Vector3.one * 1.2f, Vector3.one, .2f,
            Tweener.InterpolationType.Smooth, Tweener.RepeatMode.Once, 0, OnIncrementAnimFinished);
    }

    public Color GetColor()
    {
        return valueColors[Value % MAX_MINOR_VALUE];
    }

    public bool IsMaxValue()
    {
        return Value >= SUFFIXES.Length * MAX_MINOR_VALUE;
    }

    public void OnPointerDown()
    {
        if (state == State.Merging || IsMerged())
        {
            return;
        }
        if (pressed)
        {
            eventReentered.Fire(this);
        }
        else
        {
            eventPressed.Fire(this);
        }
    }

    public void MergeWith(Dot other)
    {
        mergeProgress = 0f;
        mergeTargetDot = other;
    }

    public bool IsMerged()
    {
        return mergeTargetDot != null;
    }

    public void OnPointerEntered()
    {
        if (InputManager.instance.pointerDown)
        {
            OnPointerDown();
        }
    }

    void OnIncrementAnimFinished(Tweener tween)
    {
        eventSpawnAnimFinished.Fire(this);
    }

}
