using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class EditorGraphCompiler
{
    public static List<RuntimeNode> Compile(
        IEnumerable<NodeElement> editorNodes,
        IEnumerable<ConnectionLine> connections)
    {
        var nodeList = new List<NodeElement>(editorNodes);
        var runtimeNodes = new List<RuntimeNode>();
        var nodeToIndex = new Dictionary<NodeElement, int>();

        for (int i = 0; i < nodeList.Count; i++)
        {
            NodeElement editorNode = nodeList[i];
            RuntimeNode runtimeNode = CreateRuntimeNode(editorNode.Data.nodeType);

            runtimeNodes.Add(runtimeNode);
            nodeToIndex[editorNode] = i;
        }

        foreach (var line in connections)
        {
            NodeElement fromNode = line.StartPort.GetFirstAncestorOfType<NodeElement>();
            NodeElement toNode = line.EndPort.GetFirstAncestorOfType<NodeElement>();

            if (fromNode == null || toNode == null)
                continue;

            if (!nodeToIndex.TryGetValue(fromNode, out int fromIndex))
                continue;

            if (!nodeToIndex.TryGetValue(toNode, out int toIndex))
                continue;

            RuntimeNode runtimeNode = runtimeNodes[fromIndex];

            if (runtimeNode is IfEnemyAheadRuntimeNode ||
                runtimeNode is IfWallAheadRuntimeNode ||
                runtimeNode is IfTurretAimedRuntimeNode)
            {
                var outputPorts = fromNode.Query<NodePort>()
                    .Where(p => !p.IsInput)
                    .ToList();

                if (outputPorts.Count >= 2)
                {
                    if (line.StartPort == outputPorts[0])
                        runtimeNode.TrueNodeIndex = toIndex;
                    else if (line.StartPort == outputPorts[1])
                        runtimeNode.FalseNodeIndex = toIndex;
                }
            }
            else
            {
                runtimeNode.NextNodeIndex = toIndex;
            }
        }

        return runtimeNodes;
    }

    public static List<RuntimeNode> Compile(NodeGraphData graphData)
    {
        var runtimeNodes = new List<RuntimeNode>();
        var nodeIdToIndex = new Dictionary<string, int>();
        var nodeIdToSave = new Dictionary<string, NodeSaveData>();

        for (int i = 0; i < graphData.nodes.Count; i++)
        {
            NodeSaveData nodeSave = graphData.nodes[i];
            RuntimeNode runtimeNode = CreateRuntimeNode(nodeSave.nodeType);

            runtimeNodes.Add(runtimeNode);
            nodeIdToIndex[nodeSave.id] = i;
            nodeIdToSave[nodeSave.id] = nodeSave;
        }

        foreach (var conn in graphData.connections)
        {
            if (!nodeIdToIndex.TryGetValue(conn.fromNodeId, out int fromIndex))
                continue;

            if (!nodeIdToIndex.TryGetValue(conn.toNodeId, out int toIndex))
                continue;

            if (!nodeIdToSave.TryGetValue(conn.fromNodeId, out NodeSaveData fromNodeSave))
                continue;

            RuntimeNode runtimeNode = runtimeNodes[fromIndex];

            if (IsBranchNodeType(fromNodeSave.nodeType))
            {
                if (conn.fromPortIndex == 0)
                    runtimeNode.TrueNodeIndex = toIndex;
                else if (conn.fromPortIndex == 1)
                    runtimeNode.FalseNodeIndex = toIndex;
            }
            else
            {
                runtimeNode.NextNodeIndex = toIndex;
            }
        }

        return runtimeNodes;
    }

    static bool IsBranchNodeType(string nodeType)
    {
        return nodeType == "IfEnemyAhead"
            || nodeType == "IfWallAhead"
            || nodeType == "IfTurretAimed";
    }

    static RuntimeNode CreateRuntimeNode(string type)
    {
        switch (type)
        {
            case "Start":
                return new StartRuntimeNode();

            case "Move":
                return new MoveForwardRuntimeNode();

            case "Back":
                return new BackRuntimeNode();

            case "TurnRight":
                return new TurnRightRuntimeNode();

            case "TurnLeft":
                return new TurnLeftRuntimeNode();

            case "TurretRight":
                return new TurretRightRuntimeNode();

            case "TurretLeft":
                return new TurretLeftRuntimeNode();

            case "Wait":
                return new WaitRuntimeNode(60);

            case "Fire":
                return new FireRuntimeNode();

            case "IfEnemyAhead":
                return new IfEnemyAheadRuntimeNode();

            case "IfTurretAimed":
                return new IfTurretAimedRuntimeNode();

            case "IfWallAhead":
                return new IfWallAheadRuntimeNode();

            case "Stop":
                return new StopAllRuntimeNode();

            default:
                Debug.LogWarning($"Unknown node type: {type}. Fallback to StartRuntimeNode.");
                return new StartRuntimeNode();
        }
    }
}