public class BackRuntimeNode : RuntimeNode
{
    public float MovePower = -1f;

    public override int Execute(RuntimeAIContext context)
    {
        context.moveValue = MovePower;
        return NextNodeIndex;
    }
}