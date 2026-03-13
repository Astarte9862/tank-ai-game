using UnityEngine;
using UnityEngine.UIElements;

public static class NodeFactory
{
    public static NodeElement CreateNode(
        string type,
        Vector2 pos,
        ConnectionManager connectionManager)
    {
        NodeData data = new NodeData(type, pos);

        var node = new NodeElement(data);

        node.AddManipulator(
            new NodeDraggerManipulator(connectionManager));

        bool hasInputPort = type != "Start";

        if (hasInputPort)
        {
            NodePort input = node.AddInputPort();
            input.AddManipulator(
                new PortDraggerManipulator(connectionManager));
        }

        if (type == "IfEnemyAhead" || type == "IfTurretAimed" || type == "IfWallAhead")
        {
            NodePort trueOutput = node.AddOutputPort();
            trueOutput.name = "True";

            NodePort falseOutput = node.AddOutputPort();
            falseOutput.name = "False";

            trueOutput.AddManipulator(
                new PortDraggerManipulator(connectionManager));

            falseOutput.AddManipulator(
                new PortDraggerManipulator(connectionManager));
        }
        else
        {
            NodePort output = node.AddOutputPort();
            output.AddManipulator(
                new PortDraggerManipulator(connectionManager));
        }

        return node;
    }
}