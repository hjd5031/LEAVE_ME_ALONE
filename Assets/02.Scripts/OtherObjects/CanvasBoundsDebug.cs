using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CanvasBoundsDebug : MonoBehaviour
{
    [SerializeField] private Canvas[] targetCanvases;
    [SerializeField] private Color boundsColor = new Color(0f, 1f, 0.2f, 0.9f);
    [SerializeField] private bool drawDiagonalLines = true;
    [SerializeField] private bool drawLabels = true;

    private readonly Vector3[] _corners = new Vector3[4];

    private void Reset()
    {
        CollectChildCanvases();
    }

    private void OnValidate()
    {
        if (targetCanvases == null || targetCanvases.Length == 0)
        {
            CollectChildCanvases();
        }
    }

    private void OnDrawGizmos()
    {
        Canvas[] canvases = targetCanvases;
        if (canvases == null || canvases.Length == 0)
        {
            canvases = GetComponentsInChildren<Canvas>(true);
        }

        Gizmos.color = boundsColor;
        foreach (Canvas canvas in canvases)
        {
            DrawCanvasBounds(canvas);
        }
    }

    private void CollectChildCanvases()
    {
        targetCanvases = GetComponentsInChildren<Canvas>(true);
    }

    private void DrawCanvasBounds(Canvas canvas)
    {
        if (canvas == null || canvas.transform is not RectTransform rectTransform)
        {
            return;
        }

        rectTransform.GetWorldCorners(_corners);

        Gizmos.DrawLine(_corners[0], _corners[1]);
        Gizmos.DrawLine(_corners[1], _corners[2]);
        Gizmos.DrawLine(_corners[2], _corners[3]);
        Gizmos.DrawLine(_corners[3], _corners[0]);

        if (drawDiagonalLines)
        {
            Gizmos.DrawLine(_corners[0], _corners[2]);
            Gizmos.DrawLine(_corners[1], _corners[3]);
        }

#if UNITY_EDITOR
        if (drawLabels)
        {
            Vector3 center = (_corners[0] + _corners[1] + _corners[2] + _corners[3]) * 0.25f;
            Handles.color = boundsColor;
            Handles.Label(center, canvas.name);
        }
#endif
    }
}
