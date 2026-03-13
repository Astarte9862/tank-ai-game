using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeGraphData
{
    public List<NodeSaveData> nodes = new List<NodeSaveData>();
    public List<ConnectionSaveData> connections = new List<ConnectionSaveData>();
}

[System.Serializable]
public class NodeSaveData
{
    public string id;
    public string nodeType;
    public Vector2 position;

    public string parametersJson;
}

[System.Serializable]
public class ConnectionSaveData
{
    public string fromNodeId;
    public int fromPortIndex;

    public string toNodeId;
    public int toPortIndex;
}