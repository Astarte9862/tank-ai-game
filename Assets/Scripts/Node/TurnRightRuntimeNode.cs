public class TurnRightRuntimeNode : RuntimeNode
{
    public float TurnPower = -1f;

    public override int Execute(RuntimeAIContext context)
    {
        context.turn = TurnPower;
        return NextNodeIndex;
    }
}