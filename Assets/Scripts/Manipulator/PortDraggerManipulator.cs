using UnityEngine.UIElements;

public class PortDraggerManipulator : PointerManipulator
{
    readonly ConnectionManager connectionManager;
    bool isDragging;

    public PortDraggerManipulator(ConnectionManager manager)
    {
        connectionManager = manager;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnDown);
        target.RegisterCallback<PointerMoveEvent>(OnMove);
        target.RegisterCallback<PointerUpEvent>(OnUp);
        target.RegisterCallback<PointerEnterEvent>(OnEnter);
        target.RegisterCallback<PointerLeaveEvent>(OnLeave);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnDown);
        target.UnregisterCallback<PointerMoveEvent>(OnMove);
        target.UnregisterCallback<PointerUpEvent>(OnUp);
        target.UnregisterCallback<PointerEnterEvent>(OnEnter);
        target.UnregisterCallback<PointerLeaveEvent>(OnLeave);
    }

    void OnDown(PointerDownEvent evt)
    {
        if (evt.button != 0)
            return;

        if (target is not NodePort port)
            return;

        isDragging = true;
        target.CapturePointer(evt.pointerId);

        connectionManager.UpdateMouse(evt.position);
        connectionManager.BeginConnection(port);

        evt.StopPropagation();
    }

    void OnMove(PointerMoveEvent evt)
    {
        if (!isDragging || !target.HasPointerCapture(evt.pointerId))
            return;

        connectionManager.UpdateMouse(evt.position);
        evt.StopPropagation();
    }

    void OnUp(PointerUpEvent evt)
    {
        if (evt.button != 0)
            return;

        if (!isDragging)
            return;

        isDragging = false;

        connectionManager.UpdateMouse(evt.position);
        connectionManager.EndConnection();

        if (target.HasPointerCapture(evt.pointerId))
            target.ReleasePointer(evt.pointerId);

        evt.StopPropagation();
    }

    void OnEnter(PointerEnterEvent evt)
    {
        if (target is NodePort port)
            connectionManager.SetHoverPort(port);
    }

    void OnLeave(PointerLeaveEvent evt)
    {
        if (target is NodePort port)
            connectionManager.ClearHoverPort(port);
    }
}