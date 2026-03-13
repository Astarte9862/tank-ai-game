using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ConnectionManager
{
    readonly VisualElement graphRoot;
    readonly VisualElement contentRoot;
    readonly VisualElement lineLayer;

    readonly List<ConnectionLine> lines = new List<ConnectionLine>();

    NodePort startPort;
    NodePort hoverPort;
    ConnectionPreviewLine previewLine;

    Vector2 mouseScreenPos;

    public IReadOnlyList<ConnectionLine> Lines => lines;

    public ConnectionManager(VisualElement graphRoot, VisualElement contentRoot, VisualElement lineLayer)
    {
        this.graphRoot = graphRoot;
        this.contentRoot = contentRoot;
        this.lineLayer = lineLayer;
    }

    public void BeginConnection(NodePort port)
    {
        CancelConnection();

        startPort = port;
        hoverPort = null;

        previewLine = new ConnectionPreviewLine(port, lineLayer);
        lineLayer.Add(previewLine);

        previewLine.SetEndWorldPosition(mouseScreenPos);
    }

    public void EndConnection()
    {
        NodePort endPort = hoverPort;

        if (previewLine != null)
        {
            previewLine.RemoveFromHierarchy();
            previewLine = null;
        }

        if (startPort == null)
        {
            hoverPort = null;
            return;
        }

        if (endPort == null || endPort == startPort)
        {
            startPort = null;
            hoverPort = null;
            return;
        }

        if (startPort.IsInput == endPort.IsInput)
        {
            startPort = null;
            hoverPort = null;
            return;
        }

        var startNode = startPort.GetFirstAncestorOfType<NodeElement>();
        var endNode = endPort.GetFirstAncestorOfType<NodeElement>();

        if (startNode != null && endNode != null && startNode == endNode)
        {
            startPort = null;
            hoverPort = null;
            return;
        }

        NodePort from = startPort.IsInput ? endPort : startPort;
        NodePort to = startPort.IsInput ? startPort : endPort;

        // 出力1本 / 入力1本 の想定で既存接続を削除
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].StartPort == from || lines[i].EndPort == to)
            {
                var l = lines[i];

                l.StartPort.Connections.Remove(l);
                l.EndPort.Connections.Remove(l);

                l.StartPort.SetConnected(l.StartPort.Connections.Count > 0);
                l.EndPort.SetConnected(l.EndPort.Connections.Count > 0);

                l.RemoveFromHierarchy();
                lines.RemoveAt(i);
            }
        }

        var line = new ConnectionLine(from, to, lineLayer);
        lines.Add(line);

        from.Connections.Add(line);
        to.Connections.Add(line);

        from.SetConnected(true);
        to.SetConnected(true);

        lineLayer.Add(line);

        startPort = null;
        hoverPort = null;
    }

    public void SetHoverPort(NodePort port)
    {
        hoverPort = port;
    }

    public void ClearHoverPort(NodePort port)
    {
        if (hoverPort == port)
            hoverPort = null;
    }

    public void UpdateMouse(Vector2 screenPos)
    {
        mouseScreenPos = screenPos;

        if (previewLine != null)
            previewLine.SetEndWorldPosition(mouseScreenPos);
    }

    public void UpdateConnections()
    {
        foreach (var l in lines)
            l.MarkDirtyRepaint();

        if (previewLine != null)
            previewLine.MarkDirtyRepaint();
    }

    public void RemoveConnection(ConnectionLine line)
    {
        if (!lines.Contains(line))
            return;

        line.StartPort.Connections.Remove(line);
        line.EndPort.Connections.Remove(line);

        line.StartPort.SetConnected(line.StartPort.Connections.Count > 0);
        line.EndPort.SetConnected(line.EndPort.Connections.Count > 0);

        lines.Remove(line);
        line.RemoveFromHierarchy();
    }

    public void RemoveConnectionsOfNode(VisualElement node)
    {
        var removeList = new List<ConnectionLine>();

        foreach (var l in lines)
        {
            if (node.Contains(l.StartPort) || node.Contains(l.EndPort))
                removeList.Add(l);
        }

        foreach (var l in removeList)
        {
            l.StartPort.Connections.Remove(l);
            l.EndPort.Connections.Remove(l);

            l.StartPort.SetConnected(l.StartPort.Connections.Count > 0);
            l.EndPort.SetConnected(l.EndPort.Connections.Count > 0);

            l.RemoveFromHierarchy();
            lines.Remove(l);
        }
    }

    public void CancelConnection()
    {
        if (previewLine != null)
        {
            previewLine.RemoveFromHierarchy();
            previewLine = null;
        }

        startPort = null;
        hoverPort = null;
    }

    public void ClearAllConnections()
    {
        foreach (var line in lines)
        {
            if (line.StartPort != null)
            {
                line.StartPort.Connections.Remove(line);
                line.StartPort.SetConnected(line.StartPort.Connections.Count > 0);
            }

            if (line.EndPort != null)
            {
                line.EndPort.Connections.Remove(line);
                line.EndPort.SetConnected(line.EndPort.Connections.Count > 0);
            }

            line.RemoveFromHierarchy();
        }

        lines.Clear();
        previewLine = null;
        startPort = null;
        hoverPort = null;
    }

    public void CreateConnectionDirect(NodePort from, NodePort to)
    {
        if (from == null || to == null)
            return;

        if (from.IsInput || !to.IsInput)
            return;

        // 出力1本 / 入力1本 制約を維持
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].StartPort == from || lines[i].EndPort == to)
            {
                var l = lines[i];

                l.StartPort.Connections.Remove(l);
                l.EndPort.Connections.Remove(l);

                l.StartPort.SetConnected(l.StartPort.Connections.Count > 0);
                l.EndPort.SetConnected(l.EndPort.Connections.Count > 0);

                l.RemoveFromHierarchy();
                lines.RemoveAt(i);
            }
        }

        var line = new ConnectionLine(from, to, lineLayer);
        lines.Add(line);

        from.Connections.Add(line);
        to.Connections.Add(line);

        from.SetConnected(true);
        to.SetConnected(true);

        lineLayer.Add(line);
    }
}