using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Dot : MonoBehaviour
{
    public static readonly float MERGE_DURATION = 0.2f;
    private const int MAX_MINOR_VALUE = 9;
    private const string SUFFIXES = " KMBTABCDEFGHIJKLMNOPQRSTUVW";

    public event EventDelegate<Dot> eventPressed;
    public event EventDelegate<Dot> eventReentered;
    public event EventDelegate<Dot> eventMergeFinished;
    public event EventDelegate<Dot> eventSpawnAnimFinished;
    public event EventDelegate<Dot> eventFallAnimFinished;

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
    private Dot mergeTargetDot = null;
    private float mergeProgress = 0f;
    private bool dead;


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
                Kill();
                eventMergeFinished.Fire(this);
                mergeTargetDot = null;
            }
        }
    }

    public void SetPressed(bool value)
    {
        pressed = value;
        imageDot.transform.localScale = pressed ? Vector2.one * 1.1f : Vector2.one;
    }

    public void RespawnWithValue(int value)
    {
        Revive();
        Value = value;

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
        if (mergeTargetDot != null)
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
        imageDot.transform.localScale = Vector3.one;
        mergeProgress = 0f;
        mergeTargetDot = other;
    }

    public Dot GetMergeTargetDot()
    {
        return mergeTargetDot;
    }

    public bool IsDead()
    {
        return dead;
    }

    public void Kill()
    {
        dead = true;
        imageDot.gameObject.SetActive(false);
    }

    public void DuplicateAndAnimateFall(Dot otherDot)
    {
        Revive();
        Value = otherDot.value;
        Tweener.RemoveTweensFromTransform(imageDot.transform);
        Tweener.AddTween(imageDot.transform, Tweener.Type.Position, otherDot.transform.position, transform.position, .2f,
            Tweener.InterpolationType.EaseFrom, Tweener.RepeatMode.Once, 0, OnFallAnimFinished);
        otherDot.Kill();
    }

    public void OnPointerEntered()
    {
        if (InputManager.instance.pointerDown)
        {
            OnPointerDown();
        }
    }

    void Revive()
    {
        dead = false;
        imageDot.gameObject.SetActive(true);
        imageDot.transform.position = transform.position;
        mergeTargetDot = null;
        textValue.color = Color.white;
    }

    void OnIncrementAnimFinished(Tweener tweener)
    {
        eventSpawnAnimFinished.Fire(this);
    }

    void OnFallAnimFinished(Tweener tweener)
    {
        eventFallAnimFinished.Fire(this);
        RectTransform imageRect = imageDot.GetComponent<RectTransform>();
        imageRect.pivot = new Vector2(.5f, 0f);
        Tweener.RemoveTweensFromTransform(imageDot.transform);
        Vector3 scaleTo = new Vector3(1f, .7f, 1f);
        Tweener.AddTween(imageRect, Tweener.Type.Scale, Vector3.one, scaleTo, .07f, Tweener.InterpolationType.EaseTo);
        Tweener.AddTween(imageRect, Tweener.Type.Scale, scaleTo, Vector2.one, .07f,
            Tweener.InterpolationType.EaseFrom, Tweener.RepeatMode.Once, .07f, OnBounceAnimFinished);
    }

    void OnBounceAnimFinished(Tweener tweener)
    {
        RectTransform rect = imageDot.GetComponent<RectTransform>();
        rect.pivot = new Vector2(.5f, .5f);
    }

}
