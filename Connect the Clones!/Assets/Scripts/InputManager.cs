using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }

    public event EventDelegate eventPointerPressed;
    public event EventDelegate eventPointerReleased;

    [HideInInspector]
    public bool pointerDown { get; private set; } = false;

    void Awake()
    {
        Debug.Assert(InputManager.instance == null);
        InputManager.instance = this;
    }

    void Update() 
    {
        bool currentPointerDown = Input.GetMouseButton(0) 
                                    || (Input.touchCount > 0 && Input.GetTouch(0).phase < TouchPhase.Ended);
        
        if (currentPointerDown && !pointerDown) 
        {
            eventPointerPressed.Fire();
        }
        else if (!currentPointerDown && pointerDown)
        {
            eventPointerReleased.Fire();
        }

        pointerDown = currentPointerDown;
    }
}
