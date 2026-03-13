using System.Collections.Generic;
using UnityEngine.UIElements;

public class NodePort : VisualElement
{
    public bool IsInput { get; private set; }

    public List<ConnectionLine> Connections { get; } = new();

    public NodePort(bool isInput)
    {
        IsInput = isInput;

        AddToClassList("port");

        if (IsInput)
            AddToClassList("port-input");
        else
            AddToClassList("port-output");
    }

    public void SetConnected(bool connected)
    {
        EnableInClassList("port-connected", connected);
    }

    public void RefreshConnections()
    {
        foreach (var line in Connections)
            line.MarkDirtyRepaint();
    }
}