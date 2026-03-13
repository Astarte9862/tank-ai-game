using System.Collections.Generic;
using UnityEngine;

public class TankRegistry : MonoBehaviour
{
    public static TankRegistry Instance { get; private set; }

    readonly List<TankAgent> agents = new List<TankAgent>();

    public IReadOnlyList<TankAgent> Agents => agents;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(TankAgent agent)
    {
        if (agent == null)
            return;

        if (!agents.Contains(agent))
            agents.Add(agent);
    }

    public void Unregister(TankAgent agent)
    {
        if (agent == null)
            return;

        agents.Remove(agent);
    }
}