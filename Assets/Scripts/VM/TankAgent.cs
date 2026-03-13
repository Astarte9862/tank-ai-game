using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAgent : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] bool enableAI = true;

    [SerializeField] NodeEditorManager editorManager;

    TankController controller;
    TankTeam team;
    TankHealth health;

    readonly TankAIVirtualMachine vm = new TankAIVirtualMachine();
    readonly RuntimeAIContext aiContext = new RuntimeAIContext();

    bool compiled;

    [Header("Enemy Sensor")]
    [SerializeField] Transform turretPivot;
    [SerializeField] float enemyCheckDistance = 8f;
    [SerializeField] float enemyCheckAngle = 12f;
    [SerializeField] LayerMask enemyLayer;

    [Header("Wall Sensor")]
    [SerializeField] float wallCheckDistance = 2.0f;
    [SerializeField] float wallCheckAngle = 20f;
    [SerializeField] LayerMask wallLayer;

    [Header("Aim Sensor")]
    [SerializeField] Transform firePoint;
    [SerializeField] float aimCheckDistance = 10f;
    [SerializeField] LayerMask aimHitMask;

    bool hitCenter;
    bool hitLeft;
    bool hitRight;

    bool hitWallCenter;
    bool hitWallLeft;
    bool hitWallRight;

    bool hitAim;

    [SerializeField] bool alwaysShowGizmos = false;
    [Header("AI File")]
    [SerializeField] string aiFileName = "PlayerAI.json";

    void OnEnable()
    {
        if (TankRegistry.Instance != null)
            TankRegistry.Instance.Register(this);
    }

    void OnDisable()
    {
        if (TankRegistry.Instance != null)
            TankRegistry.Instance.Unregister(this);
    }

    void Awake()
    {
        if (editorManager == null)
            editorManager = FindFirstObjectByType<NodeEditorManager>();
    }

    void EnsureRegistered()
    {
        if (TankRegistry.Instance != null)
            TankRegistry.Instance.Register(this);
    }

    IEnumerator Start()
    {
        EnsureRegistered();

        controller = GetComponent<TankController>();
        team = GetComponent<TankTeam>();
        health = GetComponent<TankHealth>();

        if (controller != null)
            controller.SetManualInputEnabled(false);

        if (!enableAI)
        {
            Debug.Log($"{name}: AI disabled");
            yield break;
        }

        Debug.Log("TankAgent Start");
        Debug.Log("controller = " + controller);
        Debug.Log("editorManager = " + editorManager);

        yield return null;

        CompileFromFile();
    }

    void Update()
    {
        if (!enableAI)
            return;

        if (health != null && health.IsDead)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R pressed");
            CompileFromEditor();
        }

        if (!compiled)
            return;

        aiContext.ClearFrameCommands();

        UpdateSensors();

        vm.Tick(aiContext, 4);
        ApplyAIContext();
    }

    public void CompileFromEditor()
    {
        if (editorManager == null)
        {
            Debug.LogWarning("TankAgent: editorManager is not assigned.");
            compiled = false;
            return;
        }

        List<RuntimeNode> runtimeNodes = EditorGraphCompiler.Compile(
            editorManager.GetAllNodes(),
            editorManager.GetAllConnections()
        );

        Debug.Log("Compiled runtime nodes: " + runtimeNodes.Count);

        vm.SetNodes(runtimeNodes);
        compiled = runtimeNodes.Count > 0;
    }

    public void CompileFromFile()
    {
        var persistence = new NodeGraphPersistence();
        NodeGraphData graphData = persistence.Load(aiFileName);

        if (graphData == null)
        {
            Debug.LogWarning($"{name}: AI load failed: {aiFileName}");
            compiled = false;
            return;
        }

        List<RuntimeNode> runtimeNodes = EditorGraphCompiler.Compile(graphData);

        Debug.Log($"{name}: Compiled runtime nodes from file: {runtimeNodes.Count}");

        vm.SetNodes(runtimeNodes);
        compiled = runtimeNodes.Count > 0;
    }

    void ApplyAIContext()
    {
        if (controller == null)
            return;

        controller.SetMove(aiContext.moveValue);
        controller.SetHullTurn(aiContext.turn);
        controller.SetTurretTurn(aiContext.turretTurn);

        if (aiContext.fire)
        {
            controller.Fire();
        }
    }

    void UpdateSensors()
    {
        aiContext.enemyAhead = CheckEnemyAhead();
        aiContext.wallAhead = CheckWallAhead();
        aiContext.turretAimed = CheckTurretAimed();
    }

    bool CheckEnemyAhead()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            turretPivot.position,
            enemyCheckDistance,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            if (hit.transform.root == transform)
                continue;

            TankHealth health = hit.GetComponentInParent<TankHealth>();
            if (health == null || health.IsDead)
                continue;

            TankTeam otherTeam = hit.GetComponentInParent<TankTeam>();
            if (otherTeam == null)
                continue;

            if (team != null && otherTeam.Team == team.Team)
                continue;

            Vector2 toEnemy = hit.transform.position - turretPivot.position;
            float angle = Vector2.Angle(turretPivot.up, toEnemy);

            if (angle <= enemyCheckAngle)
                return true;
        }

        return false;
    }

    bool CheckWallAhead()
    {
        Vector2 origin = transform.position;
        Vector2 centerDir = transform.up;

        hitWallCenter = RayHitsWall(origin, centerDir);

        Vector2 leftDir = RotateVector(centerDir, wallCheckAngle);
        hitWallLeft = RayHitsWall(origin, leftDir);

        Vector2 rightDir = RotateVector(centerDir, -wallCheckAngle);
        hitWallRight = RayHitsWall(origin, rightDir);

        return hitWallCenter || hitWallLeft || hitWallRight;
    }

    bool CheckTurretAimed()
    {
        if (firePoint == null)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            firePoint.up,
            aimCheckDistance,
            aimHitMask
        );

        if (hit.collider == null)
            return false;

        // 自分を除外
        if (hit.collider.transform.root == transform)
            return false;

        TankTeam otherTeam = hit.collider.GetComponentInParent<TankTeam>();
        TankHealth otherHealth = hit.collider.GetComponentInParent<TankHealth>();

        if (otherTeam == null || otherHealth == null)
            return false;

        if (otherHealth.IsDead)
            return false;

        if (team != null && otherTeam.Team == team.Team)
            return false;

        hitAim = true;
        return true;
    }

    bool RayHitsEnemy(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            enemyCheckDistance,
            enemyLayer
        );

        if (hit.collider == null)
            return false;

        TankTeam otherTeam = hit.collider.GetComponentInParent<TankTeam>();
        TankHealth otherHealth = hit.collider.GetComponentInParent<TankHealth>();

        if (otherTeam == null || otherHealth == null)
            return false;

        if (otherHealth.IsDead)
            return false;

        if (team != null && otherTeam.Team == team.Team)
            return false;

        return true;
    }

    bool RayHitsWall(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            wallCheckDistance,
            wallLayer
        );

        return hit.collider != null;
    }

    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos)
            return;

        DrawEnemySensorGizmos();
        DrawWallSensorGizmos();
        DrawAimSensorGizmos();
    }

    void OnDrawGizmosSelected()
    {
        if (alwaysShowGizmos)
            return;

        DrawEnemySensorGizmos();
        DrawWallSensorGizmos();
        DrawAimSensorGizmos();
    }

    void DrawEnemySensorGizmos()
    {
        if (turretPivot == null)
            return;

        Vector2 origin = turretPivot.position;
        Vector2 centerDir = turretPivot.up;
        Vector2 leftDir = RotateVector(centerDir, enemyCheckAngle);
        Vector2 rightDir = RotateVector(centerDir, -enemyCheckAngle);

        Gizmos.color = hitCenter ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + centerDir * enemyCheckDistance);

        Gizmos.color = hitLeft ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + leftDir * enemyCheckDistance);

        Gizmos.color = hitRight ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + rightDir * enemyCheckDistance);
    }

    void DrawWallSensorGizmos()
    {
        Vector2 origin = transform.position;
        Vector2 centerDir = transform.up;
        Vector2 leftDir = RotateVector(centerDir, wallCheckAngle);
        Vector2 rightDir = RotateVector(centerDir, -wallCheckAngle);

        Gizmos.color = hitWallCenter ? Color.cyan : Color.blue;
        Gizmos.DrawLine(origin, origin + centerDir * wallCheckDistance);

        Gizmos.color = hitWallLeft ? Color.cyan : Color.blue;
        Gizmos.DrawLine(origin, origin + leftDir * wallCheckDistance);

        Gizmos.color = hitWallRight ? Color.cyan : Color.blue;
        Gizmos.DrawLine(origin, origin + rightDir * wallCheckDistance);
    }

    void DrawAimSensorGizmos()
    {
        if (firePoint == null)
            return;

        Gizmos.color = hitAim ? Color.yellow : new Color(1f, 1f, 0f, 0.35f);

        Vector2 origin = firePoint.position;
        Vector2 dir = firePoint.up;

        Gizmos.DrawLine(origin, origin + dir * aimCheckDistance);
    }
}