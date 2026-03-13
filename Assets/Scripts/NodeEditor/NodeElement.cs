using UnityEngine;
using UnityEngine.UIElements;

public class NodeElement : VisualElement
{
    Label title;

    VisualElement inputContainer;
    VisualElement outputContainer;

    public NodeData Data { get; private set; }

    public NodeElement(NodeData data)
    {
        Data = data;

        AddToClassList("node");

        if (data.nodeType == "Start")
            AddToClassList("node-start");

        title = new Label(data.nodeType);
        title.AddToClassList("node-title");

        if (data.nodeType == "Start")
            title.AddToClassList("node-title-start");

        Add(title);

        var portRow = new VisualElement();
        portRow.AddToClassList("node-port-row");

        inputContainer = new VisualElement();
        inputContainer.AddToClassList("node-inputs");

        outputContainer = new VisualElement();
        outputContainer.AddToClassList("node-outputs");

        portRow.Add(inputContainer);
        portRow.Add(outputContainer);

        Add(portRow);

        style.left = data.position.x;
        style.top = data.position.y;

        RegisterCallback<MouseDownEvent>(evt =>
        {
            NodeEditorManager.Instance.SelectNode(this);

            if (evt.button == 0)
                evt.StopPropagation();
        });
    }

    public NodePort AddInputPort()
    {
        var port = new NodePort(true);
        inputContainer.Add(port);
        return port;
    }

    public NodePort AddOutputPort()
    {
        var port = new NodePort(false);
        outputContainer.Add(port);
        return port;
    }
}