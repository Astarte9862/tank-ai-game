public class WaitRuntimeNode : RuntimeNode
{
    int waitFrames;
    int counter;

    public WaitRuntimeNode(int frames = 30)
    {
        waitFrames = frames;
    }

    public override int Execute(RuntimeAIContext context)
    {
        counter++;

        if (counter >= waitFrames)
        {
            counter = 0;
            return NextNodeIndex;
        }

        return CurrentNodeIndex;
    }
}