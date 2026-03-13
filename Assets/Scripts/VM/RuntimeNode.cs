public abstract class RuntimeNode
{
    public int NextNodeIndex = -1;
    public int TrueNodeIndex = -1;
    public int FalseNodeIndex = -1;

    public int CurrentNodeIndex;

    public abstract int Execute(RuntimeAIContext context);
}