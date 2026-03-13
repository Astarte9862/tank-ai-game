public class IfEnemyAheadRuntimeNode : RuntimeNode
{
    public override int Execute(RuntimeAIContext context)
    {
        return context.enemyAhead ? TrueNodeIndex : FalseNodeIndex;
    }
}