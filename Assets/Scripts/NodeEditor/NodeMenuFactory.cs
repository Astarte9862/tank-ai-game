using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class NodeMenuFactory
{
    public static VisualElement CreateMenu(
        VisualElement root,
        Vector2 mousePosition,
        Vector2 graphPosition,
        ConnectionManager connectionManager,
        VisualElement nodeLayer,
        System.Action onClose = null)
    {
        var items = CreateMenuItems(graphPosition, connectionManager, nodeLayer);

        var menuRoot = new VisualElement();
        menuRoot.name = "node-context-menu-root";
        menuRoot.style.position = Position.Absolute;
        menuRoot.style.left = mousePosition.x;
        menuRoot.style.top = mousePosition.y;
        menuRoot.style.flexDirection = FlexDirection.Row;
        menuRoot.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.98f);
        menuRoot.style.borderTopLeftRadius = 6;
        menuRoot.style.borderTopRightRadius = 6;
        menuRoot.style.borderBottomLeftRadius = 6;
        menuRoot.style.borderBottomRightRadius = 6;
        menuRoot.style.borderLeftWidth = 1;
        menuRoot.style.borderRightWidth = 1;
        menuRoot.style.borderTopWidth = 1;
        menuRoot.style.borderBottomWidth = 1;
        menuRoot.style.borderLeftColor = new Color(0.25f, 0.25f, 0.25f);
        menuRoot.style.borderRightColor = new Color(0.25f, 0.25f, 0.25f);
        menuRoot.style.borderTopColor = new Color(0.25f, 0.25f, 0.25f);
        menuRoot.style.borderBottomColor = new Color(0.25f, 0.25f, 0.25f);
        menuRoot.style.minWidth = 320;

        var categoryPanel = CreatePanel();
        var itemPanel = CreatePanel();

        categoryPanel.style.borderRightWidth = 1;
        categoryPanel.style.borderRightColor = new Color(0.25f, 0.25f, 0.25f);

        menuRoot.Add(categoryPanel);
        menuRoot.Add(itemPanel);

        var grouped = items
            .GroupBy(x => x.Category)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var group in grouped)
        {
            var groupItems = group.OrderBy(x => x.Name).ToList();

            var button = CreateButton(group.Key, () =>
            {
                RebuildItemPanel(itemPanel, groupItems, onClose);
            });

            categoryPanel.Add(button);
        }

        if (grouped.Count > 0)
        {
            RebuildItemPanel(itemPanel, grouped[0].OrderBy(x => x.Name).ToList(), onClose);
        }

        menuRoot.schedule.Execute(() =>
        {
            ClampMenuToRoot(root, menuRoot, mousePosition);
        });

        return menuRoot;
    }

    static VisualElement CreatePanel()
    {
        var panel = new VisualElement();
        panel.style.flexDirection = FlexDirection.Column;
        panel.style.paddingTop = 4;
        panel.style.paddingBottom = 4;
        panel.style.minWidth = 160;
        return panel;
    }

    static Button CreateButton(string text, System.Action onClick)
    {
        var button = new Button(onClick);
        button.text = text;
        button.style.unityTextAlign = TextAnchor.MiddleLeft;
        button.style.height = 28;
        button.style.marginLeft = 4;
        button.style.marginRight = 4;
        button.style.marginTop = 2;
        button.style.marginBottom = 2;
        button.style.paddingLeft = 8;
        button.style.paddingRight = 8;
        button.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
        button.style.color = Color.white;
        return button;
    }

    static void RebuildItemPanel(
        VisualElement itemPanel,
        List<NodeMenuItemData> items,
        System.Action onClose)
    {
        itemPanel.Clear();

        foreach (var item in items)
        {
            var capturedItem = item;

            var button = CreateButton(capturedItem.Name, () =>
            {
                capturedItem.CreateAction?.Invoke();
                onClose?.Invoke();
            });

            itemPanel.Add(button);
        }
    }

    static List<NodeMenuItemData> CreateMenuItems(
        Vector2 graphPosition,
        ConnectionManager connectionManager,
        VisualElement nodeLayer)
    {
        return new List<NodeMenuItemData>
        {
            new NodeMenuItemData("Flow", "Start", () => AddNode("Start", graphPosition, connectionManager, nodeLayer)),
            new NodeMenuItemData("Flow", "Wait", () => AddNode("Wait", graphPosition, connectionManager, nodeLayer)),
            new NodeMenuItemData("Flow", "Stop", () => AddNode("Stop", graphPosition, connectionManager, nodeLayer)),

            new NodeMenuItemData("Movement", "Move", () => AddNode("Move", graphPosition, connectionManager, nodeLayer)),
            new NodeMenuItemData("Movement", "Back", () => AddNode("Back", graphPosition, connectionManager, nodeLayer)),
            new NodeMenuItemData("Movement", "TurnLeft", () => AddNode("TurnLeft", graphPosition, connectionManager, nodeLayer)),
            new NodeMenuItemData("Movement", "TurnRight", () => AddNode("TurnRight", graphPosition, connectionManager, nodeLayer)),

            new NodeMenuItemData("Turret", "TurretLeft", () => AddNode("TurretLeft", graphPosition, connectionManager, nodeLayer)),
            new NodeMenuItemData("Turret", "TurretRight", () => AddNode("TurretRight", graphPosition, connectionManager, nodeLayer)),

            new NodeMenuItemData("Combat", "Fire", () => AddNode("Fire", graphPosition, connectionManager, nodeLayer)),

            new NodeMenuItemData("Sensor", "IfEnemyAhead", () => AddNode("IfEnemyAhead", graphPosition, connectionManager, nodeLayer)),
            new NodeMenuItemData("Sensor", "IfWallAhead", () => AddNode("IfWallAhead", graphPosition, connectionManager, nodeLayer)),
            new NodeMenuItemData("Sensor", "IfTurretAimed", () => AddNode("IfTurretAimed", graphPosition, connectionManager, nodeLayer)),
        };
    }

    static void AddNode(
        string nodeType,
        Vector2 graphPosition,
        ConnectionManager connectionManager,
        VisualElement nodeLayer)
    {
        var node = NodeFactory.CreateNode(nodeType, graphPosition, connectionManager);
        nodeLayer.Add(node);
    }

    static void ClampMenuToRoot(VisualElement root, VisualElement menu, Vector2 mousePosition)
    {
        float x = mousePosition.x;
        float y = mousePosition.y;

        float menuWidth = menu.resolvedStyle.width;
        float menuHeight = menu.resolvedStyle.height;

        float rootWidth = root.resolvedStyle.width;
        float rootHeight = root.resolvedStyle.height;

        if (x + menuWidth > rootWidth)
            x = rootWidth - menuWidth;

        if (y + menuHeight > rootHeight)
            y = rootHeight - menuHeight;

        if (x < 0)
            x = 0;

        if (y < 0)
            y = 0;

        menu.style.left = x;
        menu.style.top = y;
    }
}