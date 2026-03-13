public class StopAllRuntimeNode : RuntimeNode
{
    public override int Execute(RuntimeAIContext context)
    {
        context.StopAllMotion();
        return NextNodeIndex;
    }
}