public class TurretLeftRuntimeNode : RuntimeNode
{
    public float TurnPower = 1f;

    public override int Execute(RuntimeAIContext context)
    {
        context.turretTurn = TurnPower;
        return NextNodeIndex;
    }
}