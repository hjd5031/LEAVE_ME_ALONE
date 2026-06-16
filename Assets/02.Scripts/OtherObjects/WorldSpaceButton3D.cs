using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class WorldSpaceButton3D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private UnityEvent onClick;
    [SerializeField] private bool animateScale = true;
    [SerializeField] private bool useMouseFallback = true;
    [SerializeField] private bool warnWhenNoClickListeners = true;
    [SerializeField] private float hoverScaleMultiplier = 1.08f;
    [SerializeField] private float pressedScaleMultiplier = 0.95f;

    private Vector3 _initialScale;
    private bool _isPointerOver;
    private int _lastClickFrame = -1;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        SetScale(hoverScaleMultiplier);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        SetScale(1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetScale(pressedScaleMultiplier);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetScale(_isPointerOver ? hoverScaleMultiplier : 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        InvokeClick();
    }

    private void OnMouseEnter()
    {
        if (!useMouseFallback)
        {
            return;
        }

        _isPointerOver = true;
        SetScale(hoverScaleMultiplier);
    }

    private void OnMouseExit()
    {
        if (!useMouseFallback)
        {
            return;
        }

        _isPointerOver = false;
        SetScale(1f);
    }

    private void OnMouseDown()
    {
        if (!useMouseFallback)
        {
            return;
        }

        SetScale(pressedScaleMultiplier);
    }

    private void OnMouseUp()
    {
        if (!useMouseFallback)
        {
            return;
        }

        SetScale(_isPointerOver ? hoverScaleMultiplier : 1f);
    }

    private void OnMouseUpAsButton()
    {
        if (!useMouseFallback)
        {
            return;
        }

        InvokeClick();
    }

    private void InvokeClick()
    {
        if (_lastClickFrame == Time.frameCount)
        {
            return;
        }

        _lastClickFrame = Time.frameCount;

        if (warnWhenNoClickListeners && onClick.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning($"[WorldSpaceButton3D] {name} was clicked, but no On Click event is assigned.", this);
        }

        onClick.Invoke();
    }

    private void SetScale(float multiplier)
    {
        if (!animateScale)
        {
            return;
        }

        transform.localScale = _initialScale * multiplier;
    }
}
