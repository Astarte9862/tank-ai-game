public class IfWallAheadRuntimeNode : RuntimeNode
{
    public override int Execute(RuntimeAIContext context)
    {
        if (context.wallAhead)
            return TrueNodeIndex;

        return FalseNodeIndex;
    }
}