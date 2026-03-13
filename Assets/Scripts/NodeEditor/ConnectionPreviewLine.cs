using UnityEngine;
using UnityEngine.UIElements;

public class ConnectionPreviewLine : VisualElement
{
    readonly VisualElement startPort;
    readonly VisualElement coordinateRoot;

    Vector2 endWorldPos;

    public ConnectionPreviewLine(VisualElement start, VisualElement coordinateRoot)
    {
        startPort = start;
        this.coordinateRoot = coordinateRoot;

        style.position = Position.Absolute;
        this.StretchToParentSize();

        pickingMode = PickingMode.Ignore;
        generateVisualContent += DrawLine;
    }

    public void SetEndWorldPosition(Vector2 worldPos)
    {
        endWorldPos = worldPos;
        MarkDirtyRepaint();
    }

    void DrawLine(MeshGenerationContext ctx)
    {
        if (startPort == null || coordinateRoot == null)
            return;

        if (panel == null || startPort.panel == null || coordinateRoot.panel == null)
            return;

        var painter = ctx.painter2D;

        Vector2 start = coordinateRoot.WorldToLocal(startPort.worldBound.center);
        Vector2 end = coordinateRoot.WorldToLocal(endWorldPos);

        float tangent = Mathf.Max(60f, Mathf.Abs(end.x - start.x) * 0.5f);

        Vector2 c1 = start + new Vector2(tangent, 0);
        Vector2 c2 = end - new Vector2(tangent, 0);

        painter.strokeColor = Color.gray;
        painter.lineWidth = 2f;

        painter.BeginPath();
        painter.MoveTo(start);
        painter.BezierCurveTo(c1, c2, end);
        painter.Stroke();
    }
}