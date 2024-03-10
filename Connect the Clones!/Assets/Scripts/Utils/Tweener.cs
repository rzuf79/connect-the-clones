using UnityEngine;

public class Tweener : MonoBehaviour
{
	[System.Serializable]
	public enum Type
	{
		Position = 0,
		PositionLocal,
		Rotation,
		Scale,
		Color
	}

	[System.Serializable]
	public enum InterpolationType
	{
		Linear = 0,
		Smooth,
		EaseTo,
		EaseFrom,
		Bounce,
		Elastic
	}

	[System.Serializable]
	public enum RepeatMode
	{
		Once = 0,
		Loop,
		Bounce,
	}

	private class TransformColorSetter
	{
		private CanvasGroup canvasGroup = null;
		private Renderer renderer = null;
		private UnityEngine.UI.Image image = null;
		private UnityEngine.UI.Text text = null;
		private TMPro.TextMeshProUGUI textMeshProUGUI = null;

		public TransformColorSetter(Transform targetTransform)
		{
			if(AssignReference<CanvasGroup>(targetTransform.GetComponent<CanvasGroup>(), canvasGroup)) return;
			if(AssignReference<Renderer>(targetTransform.GetComponent<Renderer>(), renderer)) return;
			if(AssignReference<UnityEngine.UI.Image>(targetTransform.GetComponent<UnityEngine.UI.Image>(), image)) return;
			if(AssignReference<UnityEngine.UI.Text>(targetTransform.GetComponent<UnityEngine.UI.Text>(), text)) return;
			if(AssignReference<TMPro.TextMeshProUGUI>(targetTransform.GetComponent<TMPro.TextMeshProUGUI>(), textMeshProUGUI)) return;
		}

		public void SetColor(Color newColor)
		{
			if(canvasGroup != null) { canvasGroup.alpha = newColor.a; return; }
			if(renderer != null) { renderer.material.color = newColor; return; }
			if(image != null) { image.color = newColor; return; }
			if(text != null) { text.color = newColor; return; }
			if(textMeshProUGUI != null) { textMeshProUGUI.color = newColor; return; }
		}

		public Color GetColor()
		{
			if(canvasGroup != null) return new Color(1.0f, 1.0f, 1.0f, canvasGroup.alpha);
			if(renderer != null) return renderer.material.color;
			if(image != null) return image.color;
			if(text != null) return text.color;
			if(textMeshProUGUI != null) return textMeshProUGUI.color;
			return Color.white;
		}

		private bool AssignReference<T>(T component, T targetReference) where T : UnityEngine.Object
		{
			if(component)
			{
				targetReference = component;
				return true;
			}
			return false;
		}
	}

	public delegate void OnTweenFinishedEvent(Tweener tween);

	[Header("Tween type")]
	public Type	type = Type.Position;
	public InterpolationType interpolationType = InterpolationType.Smooth;
	public RepeatMode repeatMode = RepeatMode.Bounce;

	[Header("Vector values")]
	public Vector3 from = Vector3.zero;
	public Vector3 to = Vector3.zero;

	[Header("Color values")]
	public Color colorFrom = Color.white;
	public Color colorTo = Color.white;

	[Header("Rest 'o values")]
	public float lifetime = 1.0f;
	public float delay = 0.0f;
	public bool	useUnscaledTime = false;

	public OnTweenFinishedEvent onFinished;

	private	float timer = 0.0f;
	private	int	direction = 1;
	private TransformColorSetter colorSetter = null;

	void Start()
	{
		if(type == Type.Color && colorSetter == null)
		{
			colorSetter = new TransformColorSetter(transform);
		}
	}

	void Update()
	{
		timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

		if(delay > 0.0f)
		{
			if(timer >= delay)
			{
				delay = 0.0f;
				timer = 0.0f;
			}
			else 
			{
				return;
			}
		}

		if(timer >= lifetime)
		{
			switch(repeatMode)
			{
				case RepeatMode.Once:
				{
					timer = lifetime;
					onFinished?.Invoke(this);
					Destroy(this);
					return;
				}
				case RepeatMode.Loop:
				{
					timer = 0.0f;
					onFinished?.Invoke(this);
					break;
				}
				case RepeatMode.Bounce:
				{
					timer = 0.0f;
					direction = -direction;
					onFinished?.Invoke(this);
					break;
				}
			}
		}

		float v = timer / lifetime;
		if(direction < 0)
		{
			v = 1 - v;
		}
		v = Interpolate(v, interpolationType);

		switch(type){
			case Type.Position:
			{
				Vector3 newPosition = (to * v) + (from * (1 - v));
				transform.position = newPosition;
				break;
			}
			case Type.PositionLocal:
			{
				Vector3 newPosition = (to * v) + (from * (1 - v));
				transform.localPosition = newPosition;
				break;
			}
			case Type.Rotation:
			{
				Vector3 newRotation = (to * v) + (from * (1 - v));
				transform.localRotation = Quaternion.Euler(newRotation);
				break;
			}
			case Type.Scale:
			{
				Vector3 newScale = (to * v) + (from * (1 - v));
				transform.localScale = newScale;
				break;
			}
			case Type.Color:
			{
				Color newColor = Color.Lerp(colorFrom, colorTo, v);
				colorSetter.SetColor(newColor);
				break;
			}
		}
	}

	public bool IsFinished()
	{
		return repeatMode ==  RepeatMode.Once && timer >= lifetime;
	}

	public static float Interpolate(float v, InterpolationType interpolation)
	{
		switch(interpolation)
		{
			case InterpolationType.Linear: 
			{
				return v;
			}
			case InterpolationType.Smooth: 
			{
				return v * v * (3 - 2 * v);
			}
			case InterpolationType.EaseTo: 
			{
				return 1 - (1 - v) * (1 - v);
			}
			case InterpolationType.EaseFrom: 
			{
				return v * v;
			}
			case InterpolationType.Bounce: 
			{
				if ( v < ( 1.0 / 2.75 ) ) 
				{
					return 7.5625f * v * v;
				}
				else if ( v < ( 2.0 / 2.75 ) ) 
				{
					return 7.5625f * ( v -= ( 1.5f / 2.75f ) ) * v + 0.75f;
				}
				else if ( v < ( 2.5 / 2.75 ) ) 
				{
					return 7.5625f * ( v -= ( 2.25f / 2.75f ) ) * v + 0.9375f;
				}

				return 7.5625f * ( v -= ( 2.625f / 2.75f ) ) * v + 0.984375f;
			}
			case InterpolationType.Elastic: 
			{
				float amplitude = 0.0f;
				float period	= 0.3f;

				if(v == 0)
				{
					return 0;
				} 
				else if(v == 1.0f)
				{
					return 1;
				}

				float s = period / 4.0f;
				if(amplitude < 1.0f)
				{
					amplitude = 1.0f;
				}
				else
				{
					s = period * Mathf.Asin(1.0f / amplitude) / (2 * Mathf.PI);
				}

				return (amplitude * Mathf.Pow(2.0f, -10.0f*v) * Mathf.Sin((v - s) * (2.0f * Mathf.PI) / period) + 1.0f);
			}
		}

		return v;
	}

	public static Tweener AddTween(Transform target,
		Type type,
		Vector3 from,
		Vector3 to,
		float time,
		InterpolationType interpolationType = InterpolationType.Smooth,
		RepeatMode repeatMode = RepeatMode.Once,
		float delay = 0.0f,
		OnTweenFinishedEvent onFinishedEvent = null)
	{

		if(target == null)
		{
			Debug.LogError("Trying to add a tween to something fishy... Aborting.");
			return null;
		}

		Tweener newTween = target.gameObject.AddComponent<Tweener>();

		newTween.type				= type;
		newTween.from 				= from;
		newTween.to 				= to;
		newTween.lifetime 			= time;
		newTween.interpolationType 	= interpolationType;
		newTween.repeatMode 		= repeatMode;
		newTween.delay 				= delay;
		newTween.onFinished 		= onFinishedEvent;
		newTween.direction			= 1;
		newTween.timer				= 0.0f;

		if(type == Type.Color)
		{
			newTween.colorSetter = new TransformColorSetter(target);
		}
		if(delay <= 0.0f){
			switch(newTween.type)
			{
				case Type.Position:
				{
					target.position = from;
					break;
				}
				case Type.PositionLocal:
				{
					target.localPosition = from;
					break;
				}
				case Type.Rotation:
				{
					target.localRotation = Quaternion.Euler(from);
					break;
				}
				case Type.Scale:
				{
					target.localScale = from;
					break;
				}
			}
		}

		return newTween;
	}

	public static Tweener AddColorTween(Transform target, Color from, Color to, float time, float delay = 0.0f, OnTweenFinishedEvent onFinishedEvent = null)
	{
		Tweener newTween = AddTween(target, Type.Color, Vector3.one, Vector3.one, time);

		if(newTween == null)
		{
			return null;
		}

		newTween.colorFrom = from;
		newTween.colorTo = to;
		newTween.delay = delay;
		newTween.onFinished	= onFinishedEvent;

		if(delay <= 0)
		{
			newTween.colorSetter.SetColor(from);
		}

		return newTween;
	}

	public static Tweener FadeIn(Transform target, float time, float delay = 0.0f, OnTweenFinishedEvent onFinishedEvent = null)
	{
		Color alphedOut = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		SetObjectColor(target, alphedOut);
		return AddColorTween(target, alphedOut, Color.white, time, delay, onFinishedEvent);
	}

	public static Tweener FadeOut(Transform target, float time, float delay = 0.0f, OnTweenFinishedEvent onFinishedEvent = null)
	{
		Color alphedOut = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		return AddColorTween(target, Color.white, alphedOut, time, delay, onFinishedEvent);
	}

	public static void FinishTween(Tweener tween)
	{
		tween.timer 		= tween.lifetime;
		tween.direction		= 1;
		tween.repeatMode	= RepeatMode.Once;

		switch(tween.type)
		{
			case Type.Position:
			{
				tween.transform.position = tween.to;
				break;
			}
			case Type.PositionLocal:
			{
				tween.transform.localPosition = tween.to;
				break;
			}
			case Type.Rotation:
			{
				tween.transform.localRotation = Quaternion.Euler(tween.to);
				break;
			}
			case Type.Scale:
			{
				tween.transform.localScale = tween.to;
				break;
			}
			case Type.Color:
			{
				tween.colorSetter.SetColor(tween.colorTo);
				break;
			}
		}
	}

	public static bool AreAnyTweensActiveForTransform(Transform transform)
	{
		Tweener[] tweens = transform.GetComponentsInChildren<Tweener>(true);
		for(int i = 0; i < tweens.Length; ++i)
		{
			if(!tweens[i].IsFinished())
			{
				return true;
			}
		}
		return false;
	}

	public static void RemoveDelaysFromTransformTweens(Transform transform)
	{
		Tweener[] tweens = transform.GetComponentsInChildren<Tweener>(true);
		for(int i = 0; i < tweens.Length; ++i)
		{
			tweens[i].delay = 0.001f;
		}
	}

	public static void RemoveTweensFromTransform(Transform transform, bool finish = false, bool recursive = false)
	{
		Tweener[] tweens = recursive ? transform.GetComponentsInChildren<Tweener>(true) : transform.GetComponents<Tweener>();
		for(int i = 0; i < tweens.Length; ++i)
		{
			if(finish)
			{
				FinishTween(tweens[i]);
			}
			Destroy(tweens[i]);
		}
	}

	public static void SetObjectColor(Transform target, Color newColor)
	{
		(new TransformColorSetter(target)).SetColor(newColor);
	}

	public static Color GetObjectColor(Transform target)
	{
		return (new TransformColorSetter(target)).GetColor();
	}
}