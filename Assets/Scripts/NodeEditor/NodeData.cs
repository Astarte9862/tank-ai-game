using UnityEngine;

[System.Serializable]
public class NodeData
{
    public string id;
    public string nodeType;
    public Vector2 position;

    public NodeData(string type, Vector2 pos)
    {
        id = System.Guid.NewGuid().ToString();
        nodeType = type;
        position = pos;
    }
}