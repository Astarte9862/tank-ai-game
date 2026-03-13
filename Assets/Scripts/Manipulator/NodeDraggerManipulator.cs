using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class NodeDraggerManipulator : PointerManipulator
{
    Vector2 startMouse;
    Vector2 startPos;

    readonly ConnectionManager connectionManager;

    public NodeDraggerManipulator(ConnectionManager manager)
    {
        connectionManager = manager;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnDown);
        target.RegisterCallback<PointerMoveEvent>(OnMove);
        target.RegisterCallback<PointerUpEvent>(OnUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnDown);
        target.UnregisterCallback<PointerMoveEvent>(OnMove);
        target.UnregisterCallback<PointerUpEvent>(OnUp);
    }

    void OnDown(PointerDownEvent evt)
    {
        if (evt.button != 0)
            return;

        startMouse = evt.position;
        startPos = new Vector2(
            target.resolvedStyle.left,
            target.resolvedStyle.top
        );

        target.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    void OnMove(PointerMoveEvent evt)
    {
        if (!target.HasPointerCapture(evt.pointerId))
            return;

        Vector2 screenDelta = (Vector2)evt.position - startMouse;

        Vector2 graphDelta;
        if (NodeEditorManager.Instance != null)
            graphDelta = NodeEditorManager.Instance.ScreenDeltaToGraphDelta(screenDelta);
        else
            graphDelta = screenDelta;

        float newLeft = startPos.x + graphDelta.x;
        float newTop = startPos.y + graphDelta.y;

        target.style.left = newLeft;
        target.style.top = newTop;

        if (target is NodeElement node)
        {
            node.Data.position = new Vector2(newLeft, newTop);

            foreach (var port in node.Query<NodePort>().ToList())
                port.RefreshConnections();

            connectionManager.UpdateConnections();
        }

        evt.StopPropagation();
    }

    void OnUp(PointerUpEvent evt)
    {
        if (evt.button != 0)
            return;

        if (target.HasPointerCapture(evt.pointerId))
            target.ReleasePointer(evt.pointerId);

        evt.StopPropagation();
    }
}