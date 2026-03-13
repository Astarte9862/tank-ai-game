using UnityEngine;
using UnityEngine.UIElements;

public class ConnectionLine : VisualElement
{
    public NodePort StartPort { get; private set; }
    public NodePort EndPort { get; private set; }

    readonly VisualElement coordinateRoot;

    public ConnectionLine(NodePort start, NodePort end, VisualElement coordinateRoot)
    {
        StartPort = start;
        EndPort = end;
        this.coordinateRoot = coordinateRoot;

        style.position = Position.Absolute;
        this.StretchToParentSize();

        pickingMode = PickingMode.Ignore;
        generateVisualContent += DrawLine;
    }

    void DrawLine(MeshGenerationContext ctx)
    {
        if (StartPort == null || EndPort == null || coordinateRoot == null)
            return;

        if (panel == null || StartPort.panel == null || EndPort.panel == null || coordinateRoot.panel == null)
            return;

        var painter = ctx.painter2D;

        Vector2 start = coordinateRoot.WorldToLocal(StartPort.worldBound.center);
        Vector2 end = coordinateRoot.WorldToLocal(EndPort.worldBound.center);

        float tangent = Mathf.Max(60f, Mathf.Abs(end.x - start.x) * 0.5f);

        Vector2 control1 = start + new Vector2(tangent, 0);
        Vector2 control2 = end - new Vector2(tangent, 0);

        painter.strokeColor = Color.white;
        painter.lineWidth = 3f;

        painter.BeginPath();
        painter.MoveTo(start);
        painter.BezierCurveTo(control1, control2, end);
        painter.Stroke();
    }
}