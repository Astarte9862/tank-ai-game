public class IfTurretAimedRuntimeNode : RuntimeNode
{
    public override int Execute(RuntimeAIContext context)
    {
        return context.turretAimed ? TrueNodeIndex : FalseNodeIndex;
    }
}