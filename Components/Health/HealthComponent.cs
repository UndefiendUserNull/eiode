using EIODE.Core;
using Godot;

namespace EIODE.Components;
public partial class HealthComponent : Node, IComponent
{
    [Export] public int MaxHealth { get; private set; } = 100;
    [Export] private float _criticalHitMultiplier = 2f;
    [Export] private float _regenerationRate = 2f;
    [Export] private float _regenerationDelay = 2f;
    [Export] public bool CanRegenerate { get; private set; } = false;
    public int CurrentHealth { get; private set; } = 0;

    [Signal] public delegate void OnHealthChangeEventHandler(int health);
    [Signal] public delegate void OnTakeDamageEventHandler();
    [Signal] public delegate void OnHealEventHandler();
    [Signal] public delegate void OnDeathEventHandler();

    private bool _dead = false;
    private float _timeSinceLastDamage = 0.0f;
    public bool IsDead => _dead;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }
    public override void _Process(double delta)
    {
        if (CanRegenerate && !_dead && CurrentHealth < MaxHealth)
        {
            _timeSinceLastDamage += (float)delta;
            if (_timeSinceLastDamage >= _regenerationDelay)
            {
                Heal((int)(_regenerationRate * (float)delta));
            }
        }
    }
    public void TakeDamage(int damage, bool criticalHit = false)
    {
        if (IsDead) return;
        if (criticalHit) damage = (int)Mathf.Ceil(damage * _criticalHitMultiplier);

        CurrentHealth -= Mathf.Max(damage, 0);
        _timeSinceLastDamage = 0f;

        EmitSignal(SignalName.OnTakeDamage);
        EmitSignal(SignalName.OnHealthChange, CurrentHealth);

        if (CurrentHealth <= 0) { Die(); }
    }
    public void Heal(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        EmitSignal(SignalName.OnHeal);
        EmitSignal(SignalName.OnHealthChange, CurrentHealth);
    }
    private void Die()
    {
        _dead = true;
        CurrentHealth = 0;
        EmitSignal(SignalName.OnDeath);
        Game.GetGame(this).Console?.Log($"{Name} Died");
    }

    public HealthComponent(int maxHealth, float critMultiplier, bool canRegenerate = false, float regenerationDelay = 0f, float regenerationRate = 0f)
    {
        MaxHealth = maxHealth;
        CanRegenerate = canRegenerate;
        _criticalHitMultiplier = critMultiplier;
        _regenerationDelay = regenerationDelay;
        _regenerationRate = regenerationRate;
    }
    public HealthComponent() { }
}
