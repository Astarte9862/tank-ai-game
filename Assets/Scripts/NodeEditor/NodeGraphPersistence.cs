using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class NodeGraphPersistence
{
    public void Save(
        string fileName,
        IEnumerable<NodeElement> nodes,
        IEnumerable<ConnectionLine> lines)
    {
        var data = new NodeGraphData();

        foreach (var node in nodes)
        {
            data.nodes.Add(new NodeSaveData
            {
                id = node.Data.id,
                nodeType = node.Data.nodeType,
                position = node.Data.position
            });
        }

        foreach (var line in lines)
        {
            var fromNode = line.StartPort.GetFirstAncestorOfType<NodeElement>();
            var toNode = line.EndPort.GetFirstAncestorOfType<NodeElement>();

            if (fromNode == null || toNode == null)
                continue;

            var outPorts = fromNode.Query<NodePort>().Where(p => !p.IsInput).ToList();
            var inPorts = toNode.Query<NodePort>().Where(p => p.IsInput).ToList();

            int fromPortIndex = outPorts.IndexOf(line.StartPort);
            int toPortIndex = inPorts.IndexOf(line.EndPort);

            if (fromPortIndex < 0 || toPortIndex < 0)
                continue;

            data.connections.Add(new ConnectionSaveData
            {
                fromNodeId = fromNode.Data.id,
                fromPortIndex = fromPortIndex,
                toNodeId = toNode.Data.id,
                toPortIndex = toPortIndex
            });
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);

        Debug.Log($"Graph saved: {path}");
    }

    public NodeGraphData Load(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"LoadGraph failed: file not found: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<NodeGraphData>(json);

        if (data == null)
        {
            Debug.LogWarning("LoadGraph failed: json parse returned null.");
            return null;
        }

        Debug.Log($"Graph loaded: {path}");
        return data;
    }
}