namespace Client;

public class Health
{
    public event Action OnChange;
    public event Action OnDamage;
    
    public float MaxValue { get; private set; }
    public float Value { get; private set; }

    public Health(float maxValue, float startingValue)
    {
        MaxValue = maxValue;
        Value = startingValue;
    }

    public void Damage(float amount)
    {
        System.Diagnostics.Debug.Assert(amount > 0, "Damage amount must be positive and greater than 0");
        OffsetHealth(-amount);
        OnDamage?.Invoke();
    }

    private void OffsetHealth(float amount)
    {
        Value += amount;
        Value = Math.Clamp(Value, 0f, MaxValue);
        
        if (Value <= 0f)
        {
            // ded. todo: handle
        }
        
        OnChange?.Invoke();
    }
}