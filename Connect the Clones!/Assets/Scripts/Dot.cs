using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dot : MonoBehaviour
{
    private const int MAX_MINOR_VALUE = 9;
    private const string SUFFIXES = " KMBTABCDEFGHIJKLMNOPQRSTUVW";

    public event EventDelegate<Dot> eventPressed;
    public event EventDelegate<Dot> eventReentered;

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

    private int value;
    private bool pressed = false;


    void Update()
    {
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
        if (pressed)
            {
                eventReentered.Fire(this);
            }
            else
            {
                eventPressed.Fire(this);
            }
    }

    public void OnPointerEntered()
    {
        if (InputManager.instance.pointerDown)
        {
            OnPointerDown();
        }
    }

}
