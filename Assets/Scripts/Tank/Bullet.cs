using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 12f;
    [SerializeField] float lifeTime = 3f;
    [SerializeField] int damage = 1;

    TankTeam ownerTeam;

    public void Initialize(TankTeam team)
    {
        ownerTeam = team;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        var targetHealth = other.GetComponentInParent<TankHealth>();
        var targetTeam = other.GetComponentInParent<TankTeam>();

        if (targetHealth == null || targetTeam == null)
            return;

        if (ownerTeam != null && targetTeam.Team == ownerTeam.Team)
            return;

        if (targetHealth.IsDead)
            return;

        targetHealth.Damage(damage);
        Destroy(gameObject);
    }
}