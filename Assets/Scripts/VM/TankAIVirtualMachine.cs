using UnityEngine;
using System.Collections.Generic;

public class TankAIVirtualMachine
{
    readonly List<RuntimeNode> nodes = new List<RuntimeNode>();

    public int CurrentNodeIndex { get; private set; } = -1;

    public void SetNodes(List<RuntimeNode> runtimeNodes)
    {
        nodes.Clear();
        nodes.AddRange(runtimeNodes);
        CurrentNodeIndex = FindStartNode();
    }

    int FindStartNode()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] is StartRuntimeNode)
                return i;
        }

        return nodes.Count > 0 ? 0 : -1;
    }

    public void Tick(RuntimeAIContext context, int maxStepsPerFrame = 1)
    {
        if (CurrentNodeIndex < 0 || CurrentNodeIndex >= nodes.Count)
            return;

        int steps = 0;

        while (steps < maxStepsPerFrame)
        {
            if (CurrentNodeIndex < 0 || CurrentNodeIndex >= nodes.Count)
                return;

            RuntimeNode node = nodes[CurrentNodeIndex];
            node.CurrentNodeIndex = CurrentNodeIndex;

            Debug.Log($"VM Execute: index={CurrentNodeIndex}, type={node.GetType().Name}");

            int next = node.Execute(context);

            Debug.Log($"VM Next: {next}");

            if (next < 0 || next >= nodes.Count)
            {
                Debug.Log("VM reset to Start");
                CurrentNodeIndex = FindStartNode();
                return;
            }

            CurrentNodeIndex = next;
            steps++;
        }
    }
}