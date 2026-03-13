using System;

public class NodeMenuItemData
{
    public string Category;
    public string Name;
    public Action CreateAction;

    public NodeMenuItemData(string category, string name, Action createAction)
    {
        Category = category;
        Name = name;
        CreateAction = createAction;
    }
}