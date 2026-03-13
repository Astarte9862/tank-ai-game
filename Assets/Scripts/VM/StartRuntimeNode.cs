public class StartRuntimeNode : RuntimeNode
{
    public override int Execute(RuntimeAIContext context)
    {
        return NextNodeIndex;
    }
}