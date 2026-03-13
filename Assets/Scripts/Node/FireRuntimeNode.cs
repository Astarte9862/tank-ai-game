public class FireRuntimeNode : RuntimeNode
{
    public override int Execute(RuntimeAIContext context)
    {
        context.fire = true;
        return NextNodeIndex;
    }
}