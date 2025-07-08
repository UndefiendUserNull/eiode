using Godot;

namespace EIODE.Components;

[GlobalClass]
public partial class ChargeableComponent : Node, IComponent
{
    [Export] public float FullChargeDuration { get; set; } = 2.0f;
    [Export] public float ChargeRate { get; set; } = 1.0f;
    [Export] public float DischargeRate { get; set; } = 0.5f;
    public bool Charging { get; set; } = false;

    public float CurrentCharge { get; private set; } = 0.0f;

    [Signal] public delegate void ChargeCompletedEventHandler();
    [Signal] public delegate void ChargeProgressChangedEventHandler(float duration);
    [Signal] public delegate void FullyDrainedEventHandler();

    public void Charge(float amount)
    {
        EmitSignalChargeProgressChanged(amount);
        if (CurrentCharge >= FullChargeDuration)
        {
            CurrentCharge = FullChargeDuration;
            EmitSignalChargeCompleted();
            return;
        }
        else if (CurrentCharge < FullChargeDuration)
        {
            CurrentCharge += amount * ChargeRate;
        }
    }

    public void DeCharge(float amount)
    {
        EmitSignalChargeProgressChanged(-amount);
        if (CurrentCharge <= 0)
        {
            CurrentCharge = 0;
            EmitSignalFullyDrained();
            return;
        }
        else if (CurrentCharge > 0)
        {
            CurrentCharge -= amount * DischargeRate;
        }
    }

    public void InstantCharge()
    {
        CurrentCharge = FullChargeDuration;
        EmitSignalChargeCompleted();
    }

    public void InstantDeCharge()
    {
        CurrentCharge = 0;
        EmitSignalFullyDrained();
    }

    public ChargeableComponent() { }
}