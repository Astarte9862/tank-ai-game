using UnityEngine;

public enum TeamType
{
    A,
    B
}

public class TankTeam : MonoBehaviour
{
    [SerializeField] TeamType team = TeamType.A;

    public TeamType Team => team;
}