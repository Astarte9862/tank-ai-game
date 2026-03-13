using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class NodeEditorManager : MonoBehaviour
{
    VisualElement root;
    VisualElement graphRoot;

    VisualElement contentRoot;
    VisualElement nodeLayer;
    VisualElement lineLayer;

    VisualElement selectedNode;
    VisualElement currentMenu;

    ConnectionManager connectionManager;
    ConnectionLine selectedConnection;
    NodeGraphPersistence persistence;

    [SerializeField] float initialZoom = 0.7f;
    [SerializeField] Vector2 initialPan = new Vector2(60f, 30f);

    float zoom = 1f;
    Vector2 pan = Vector2.zero;

    bool isPanning;
    Vector2 panStartMouse;
    Vector2 panStartOffset;

    GridBackground grid;

    public static NodeEditorManager Instance { get; private set; }

    public ConnectionManager ConnectionManager => connectionManager;
    public float Zoom => zoom;
    public Vector2 Pan => pan;

    [SerializeField] UIDocument uiDocument;

    [SerializeField] bool createDefaultStartNode = true;
    [SerializeField] bool loadOnStart = false;
    [SerializeField] string loadFileName = "PlayerAI.json";

    void Start()
    {
        Instance = this;

        if (!InitializeDocument())
            return;

        zoom = initialZoom;
        pan = initialPan;

        BuildGraphView();
        CreateManagers();
        RegisterCallbacks();
        InitializeGraphContent();

        UpdateView();
    }

    bool InitializeDocument()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("NodeEditorManager: UIDocument is not assigned.");
            enabled = false;
            return false;
        }

        root = uiDocument.rootVisualElement;
        graphRoot = root.Q("graph-root");

        if (graphRoot == null)
        {
            Debug.LogError("NodeEditorManager: graph-root not found in UIDocument.");
            enabled = false;
            return false;
        }

        graphRoot.focusable = true;
        graphRoot.Focus();

        return true;
    }

    void BuildGraphView()
    {
        grid = new GridBackground(() => pan, () => zoom);
        graphRoot.Add(grid);

        contentRoot = new VisualElement();
        contentRoot.name = "content-root";
        contentRoot.style.position = Position.Absolute;
        contentRoot.StretchToParentSize();
        graphRoot.Add(contentRoot);

        lineLayer = new VisualElement();
        lineLayer.name = "line-layer";
        lineLayer.style.position = Position.Absolute;
        lineLayer.StretchToParentSize();
        lineLayer.pickingMode = PickingMode.Ignore;
        contentRoot.Add(lineLayer);

        nodeLayer = new VisualElement();
        nodeLayer.name = "node-layer";
        nodeLayer.style.position = Position.Absolute;
        nodeLayer.StretchToParentSize();
        contentRoot.Add(nodeLayer);

        grid.SendToBack();
    }

    void CreateManagers()
    {
        connectionManager = new ConnectionManager(graphRoot, contentRoot, lineLayer);
        persistence = new NodeGraphPersistence();
    }

    // =============================
    // Event Registration
    // =============================

    void RegisterCallbacks()
    {
        RegisterGlobalInputCallbacks();
        RegisterGraphInteractionCallbacks();
    }

    void RegisterGlobalInputCallbacks()
    {
        root.RegisterCallback<PointerMoveEvent>(OnRootPointerMove);
    }

    void RegisterGraphInteractionCallbacks()
    {
        graphRoot.RegisterCallback<PointerDownEvent>(OnGraphPointerDownMenu);
        graphRoot.RegisterCallback<PointerDownEvent>(OnGraphPointerDownPan);
        graphRoot.RegisterCallback<PointerMoveEvent>(OnGraphPointerMovePan);
        graphRoot.RegisterCallback<PointerUpEvent>(OnGraphPointerUpPan);
        graphRoot.RegisterCallback<MouseDownEvent>(OnGraphMouseDownClearSelection);
        graphRoot.RegisterCallback<WheelEvent>(OnWheel);
        graphRoot.RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    // =============================
    // Graph Initialization
    // =============================

    void InitializeGraphContent()
    {
        if (loadOnStart)
        {
            LoadGraph(loadFileName);
        }
        else if (createDefaultStartNode)
        {
            var node = NodeFactory.CreateNode(
                "Start",
                new Vector2(200, 200),
                connectionManager);

            nodeLayer.Add(node);
        }
    }

    // =============================
    // Input Events
    // =============================

    void OnRootPointerMove(PointerMoveEvent evt)
    {
        connectionManager.UpdateMouse(evt.position);
    }

    void OnGraphPointerDownMenu(PointerDownEvent evt)
    {
        if (evt.button == 1)
        {
            ShowNodeCreationMenu(evt.localPosition, evt.position);
            evt.StopPropagation();
            return;
        }

        if (evt.button == 0 && currentMenu != null)
        {
            CloseCurrentMenu();
        }
    }

    void OnGraphMouseDownClearSelection(MouseDownEvent evt)
    {
        if (evt.button != 0)
            return;

        SelectNode(null);
        SelectConnection(null);
    }

    // =============================
    // Pan
    // =============================

    void OnGraphPointerDownPan(PointerDownEvent evt)
    {
        if (evt.button == 2)
        {
            isPanning = true;
            panStartMouse = evt.position;
            panStartOffset = pan;

            graphRoot.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }
    }

    void OnGraphPointerMovePan(PointerMoveEvent evt)
    {
        if (!isPanning)
            return;

        Vector2 delta = (Vector2)evt.position - panStartMouse;
        pan = panStartOffset + delta;

        UpdateView();
        evt.StopPropagation();
    }

    void OnGraphPointerUpPan(PointerUpEvent evt)
    {
        if (evt.button == 2 && isPanning)
        {
            isPanning = false;

            graphRoot.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }
    }

    // =============================
    // Zoom
    // =============================

    void OnWheel(WheelEvent evt)
    {
        float oldZoom = zoom;

        float zoomDelta = -evt.delta.y * 0.001f;
        zoom = Mathf.Clamp(zoom + zoomDelta, 0.4f, 2f);

        Vector2 mouseGraphBefore = ScreenToGraph(evt.mousePosition, oldZoom, pan);
        Vector2 mouseGraphAfter = ScreenToGraph(evt.mousePosition, zoom, pan);

        pan += (mouseGraphAfter - mouseGraphBefore) * zoom;

        UpdateView();
        evt.StopPropagation();
    }

    // =============================
    // View Update
    // =============================

    void UpdateView()
    {
        contentRoot.style.translate = new Translate(pan.x, pan.y, 0);
        contentRoot.style.scale = new Scale(new Vector3(zoom, zoom, 1));

        grid.MarkDirtyRepaint();
        connectionManager.UpdateConnections();
    }

    // =============================
    // Menu
    // =============================

    void ShowNodeCreationMenu(Vector2 menuLocalPosition, Vector2 mouseWorldPosition)
    {
        CloseCurrentMenu();

        Vector2 graphPosition = ScreenToGraph(mouseWorldPosition);

        currentMenu = NodeMenuFactory.CreateMenu(
            graphRoot,
            menuLocalPosition,
            graphPosition,
            connectionManager,
            nodeLayer,
            CloseCurrentMenu
        );

        graphRoot.Add(currentMenu);
    }

    void CloseCurrentMenu()
    {
        if (currentMenu == null)
            return;

        if (currentMenu.parent != null)
            currentMenu.parent.Remove(currentMenu);

        currentMenu = null;
    }

    // =============================
    // Coordinate
    // =============================

    Vector2 ScreenToGraph(Vector2 screenPos)
    {
        return ScreenToGraph(screenPos, zoom, pan);
    }

    public Vector2 ScreenDeltaToGraphDelta(Vector2 screenDelta)
    {
        return screenDelta / Mathf.Max(0.0001f, zoom);
    }

    Vector2 ScreenToGraph(Vector2 screenPos, float targetZoom, Vector2 targetPan)
    {
        Vector2 local = graphRoot.WorldToLocal(screenPos);
        return (local - targetPan) / targetZoom;
    }

    // =============================
    // Selection
    // =============================

    public void SelectNode(VisualElement node)
    {
        if (selectedNode != null)
            selectedNode.RemoveFromClassList("node-selected");

        selectedNode = node;

        if (selectedNode != null)
        {
            selectedNode.AddToClassList("node-selected");
            selectedNode.BringToFront();
        }
    }

    public void SelectConnection(ConnectionLine line)
    {
        if (selectedConnection != null)
            selectedConnection.RemoveFromClassList("connection-selected");

        selectedConnection = line;

        if (selectedConnection != null)
            selectedConnection.AddToClassList("connection-selected");
    }

    // =============================
    // Keyboard
    // =============================

    string currentGraphFileName = "PlayerAI.json";

    void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Delete)
        {
            DeleteSelection();
            return;
        }

        if (evt.keyCode == KeyCode.Escape)
        {
            connectionManager.CancelConnection();
            return;
        }

        if (evt.keyCode == KeyCode.S)
        {
            SaveGraph(currentGraphFileName);
            return;
        }

        if (evt.keyCode == KeyCode.L)
        {
            LoadGraph(currentGraphFileName);
            return;
        }

        if (evt.keyCode == KeyCode.F1)
        {
            currentGraphFileName = "PlayerAI.json";
            Debug.Log("Current graph file: " + currentGraphFileName);
            return;
        }

        if (evt.keyCode == KeyCode.F2)
        {
            currentGraphFileName = "EnemyBasic.json";
            Debug.Log("Current graph file: " + currentGraphFileName);
            return;
        }
    }

    void DeleteSelection()
    {
        if (selectedConnection != null)
        {
            connectionManager.RemoveConnection(selectedConnection);
            selectedConnection = null;
            return;
        }

        if (selectedNode != null)
        {
            connectionManager.RemoveConnectionsOfNode(selectedNode);
            selectedNode.RemoveFromHierarchy();
            selectedNode = null;
        }
    }

    public List<string> GetSavedGraphNames()
    {
        string dir = Application.persistentDataPath;

        if (!Directory.Exists(dir))
            return new List<string>();

        return Directory.GetFiles(dir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();
    }

    // =============================
    // Graph Query
    // =============================

    public IEnumerable<NodeElement> GetAllNodes()
    {
        if (nodeLayer == null)
            yield break;

        foreach (var child in nodeLayer.Children())
        {
            if (child is NodeElement node)
                yield return node;
        }
    }

    public IEnumerable<ConnectionLine> GetAllConnections()
    {
        if (connectionManager == null)
            yield break;

        foreach (var line in connectionManager.Lines)
            yield return line;
    }

    // =============================
    // Save / Load
    // =============================

    public void SaveGraph(string fileName = "graph.json")
    {
        if (nodeLayer == null || connectionManager == null)
        {
            Debug.LogWarning("SaveGraph failed: editor is not initialized.");
            return;
        }

        persistence.Save(
            fileName,
            GetAllNodes(),
            connectionManager.Lines
        );
    }

    public void LoadGraph(string fileName = "graph.json")
    {
        if (nodeLayer == null || lineLayer == null || connectionManager == null)
        {
            Debug.LogWarning("LoadGraph failed: editor is not initialized.");
            return;
        }

        var data = persistence.Load(fileName);
        if (data == null)
            return;

        ClearCurrentGraph();
        RebuildNodes(data);
        RebuildConnections(data);
    }

    void ClearCurrentGraph()
    {
        SelectNode(null);
        SelectConnection(null);

        connectionManager.CancelConnection();
        lineLayer.Clear();
        connectionManager.ClearAllConnections();
        nodeLayer.Clear();
    }

    void RebuildNodes(NodeGraphData data)
    {
        foreach (var nodeSave in data.nodes)
        {
            NodeData nodeData = new NodeData(nodeSave.nodeType, nodeSave.position);
            nodeData.id = nodeSave.id;

            var node = NodeFactory.CreateNode(
                nodeData.nodeType,
                nodeData.position,
                connectionManager);

            node.Data.id = nodeData.id;
            nodeLayer.Add(node);
        }
    }

    void RebuildConnections(NodeGraphData data)
    {
        Dictionary<string, NodeElement> nodeMap = BuildNodeMap();

        foreach (var connSave in data.connections)
        {
            if (!nodeMap.TryGetValue(connSave.fromNodeId, out var fromNode))
                continue;

            if (!nodeMap.TryGetValue(connSave.toNodeId, out var toNode))
                continue;

            var outPorts = fromNode.Query<NodePort>().Where(p => !p.IsInput).ToList();
            var inPorts = toNode.Query<NodePort>().Where(p => p.IsInput).ToList();

            if (connSave.fromPortIndex < 0 || connSave.fromPortIndex >= outPorts.Count)
                continue;

            if (connSave.toPortIndex < 0 || connSave.toPortIndex >= inPorts.Count)
                continue;

            NodePort fromPort = outPorts[connSave.fromPortIndex];
            NodePort toPort = inPorts[connSave.toPortIndex];

            connectionManager.CreateConnectionDirect(fromPort, toPort);
        }
    }

    Dictionary<string, NodeElement> BuildNodeMap()
    {
        Dictionary<string, NodeElement> nodeMap = new Dictionary<string, NodeElement>();

        foreach (var node in GetAllNodes())
        {
            if (node?.Data == null || string.IsNullOrEmpty(node.Data.id))
                continue;

            nodeMap[node.Data.id] = node;
        }

        return nodeMap;
    }
}