using UnityEngine;

public class TankHealth : MonoBehaviour
{
    [SerializeField] int maxHp = 3;

    public int CurrentHp { get; private set; }
    public int MaxHp => maxHp;
    public bool IsDead => CurrentHp <= 0;

    void Awake()
    {
        CurrentHp = maxHp;
    }

    public void Damage(int amount)
    {
        if (IsDead)
            return;

        CurrentHp -= amount;

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            Die();
        }

        Debug.Log($"{gameObject.name} HP: {CurrentHp}/{maxHp}");
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} destroyed");

        gameObject.SetActive(false);
    }
}