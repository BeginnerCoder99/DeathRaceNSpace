using UnityEngine;

public class ShipBase : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 1f;      // how far per move
    public int maxShields = 3;
    public int damage = 1;

    [Header("State")]
    public int currentShields;
    public bool isReviving = false;

    protected virtual void Awake()
    {
        currentShields = maxShields;
    }

    // Default movement
    public virtual void MoveForward()
    {
        transform.Translate(Vector3.right * moveSpeed);
    }

    // Default attack
    public virtual void FireAt(ShipBase target)
    {
        if (target == null) return;

        target.TakeDamage(damage);
        Debug.Log($"{name} fired at {target.name} for {damage} damage!");
    }

    // Default damage handling
    public virtual void TakeDamage(int amount)
    {
        currentShields -= amount;
        currentShields = Mathf.Max(currentShields, 0);

        if (currentShields <= 0)
        {
            isReviving = true;
            Debug.Log($"{name} is down and will revive next turn!");
        }
    }
}
