using UnityEngine;

public class TankController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float hullTurnSpeed = 120f;

    [Header("Turret")]
    [SerializeField] Transform turretPivot;
    [SerializeField] float turretTurnSpeed = 180f;

    [Header("Control")]
    [SerializeField] bool useManualInput = true;

    [Header("Fire")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float fireCooldown = 3f;

    [Header("Tuning")]
    [SerializeField] bool disableMoveWhileTurning = true;
    [SerializeField] float turnPriorityThreshold = 0.01f;

    float moveInput;
    float hullTurnInput;
    float turretTurnInput;

    float fireTimer;

    TankTeam team;
    Rigidbody2D rb;

    void Awake()
    {
        team = GetComponent<TankTeam>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody2D is missing.");
        }
    }

    void Update()
    {
        if (useManualInput)
        {
            ReadManualInput();
        }

        fireTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    void ReadManualInput()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.W)) moveInput = 1f;
        if (Input.GetKey(KeyCode.S)) moveInput = -1f;

        hullTurnInput = 0f;
        if (Input.GetKey(KeyCode.A)) hullTurnInput = 1f;
        if (Input.GetKey(KeyCode.D)) hullTurnInput = -1f;

        turretTurnInput = 0f;
        if (Input.GetKey(KeyCode.Q)) turretTurnInput = 1f;
        if (Input.GetKey(KeyCode.E)) turretTurnInput = -1f;
    }

    void ApplyMovement()
    {
        if (rb == null)
            return;

        ApplyHullTurn();
        ApplyMove();
        ApplyTurretTurn();
    }

    void ApplyMove()
    {
        if (moveInput == 0f)
            return;

        if (disableMoveWhileTurning && Mathf.Abs(hullTurnInput) > turnPriorityThreshold)
            return;

        Vector2 moveDelta = (Vector2)transform.up * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDelta);
    }

    void ApplyHullTurn()
    {
        if (hullTurnInput == 0f)
            return;

        float rot = hullTurnInput * hullTurnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + rot);
    }

    void ApplyTurretTurn()
    {
        if (turretPivot == null || turretTurnInput == 0f)
            return;

        turretPivot.Rotate(Vector3.forward, turretTurnInput * turretTurnSpeed * Time.fixedDeltaTime);
    }

    public void SetMove(float value)
    {
        moveInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void SetHullTurn(float value)
    {
        hullTurnInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void SetTurretTurn(float value)
    {
        turretTurnInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void AimTurretAt(Vector2 targetPosition)
    {
        if (turretPivot == null)
            return;

        Vector2 dir = targetPosition - (Vector2)turretPivot.position;
        if (dir.sqrMagnitude <= 0.0001f)
            return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        turretPivot.rotation = Quaternion.RotateTowards(
            turretPivot.rotation,
            targetRotation,
            turretTurnSpeed * Time.fixedDeltaTime
        );
    }

    public void StopAllMotion()
    {
        moveInput = 0f;
        hullTurnInput = 0f;
        turretTurnInput = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void SetManualInputEnabled(bool enabled)
    {
        useManualInput = enabled;

        if (enabled)
        {
            moveInput = 0f;
            hullTurnInput = 0f;
            turretTurnInput = 0f;
        }
    }

    public void Fire()
    {
        if (fireTimer > 0f)
            return;

        if (bulletPrefab == null || firePoint == null)
            return;

        fireTimer = fireCooldown;

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(team);
        }
    }
}