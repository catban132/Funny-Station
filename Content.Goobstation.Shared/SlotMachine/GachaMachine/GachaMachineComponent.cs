using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlotMachine.GachaMachine;

/// <summary>
/// This is used for the claw game machine.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GachaMachineComponent : Component
{
    [DataField, AutoNetworkedField]
    public float DoAfterTime = 0.1f;

    [DataField]
    public SoundSpecifier PlaySound = new SoundPathSpecifier("/Audio/Effects/kaching.ogg");

    [DataField]
    public SoundSpecifier LoseSound = new SoundPathSpecifier("/Audio/Machines/buzz-two.ogg");

    [DataField]
    public SoundSpecifier WinSound = new SoundPathSpecifier("/Audio/Machines/machine_vend.ogg");

    [DataField, AutoNetworkedField]
    public float WinChance = 1f;

    [DataField, AutoNetworkedField]
    public int SpinCost = 250;

    [DataField, AutoNetworkedField]
    public bool IsSpinning;

    [DataField, AutoNetworkedField]
    public List<EntProtoId>? Rewards;

    [DataField, AutoNetworkedField]
    public List<EntProtoId>? EvilRewards;

    [DataField, AutoNetworkedField]
    public bool Emagged;
}

[Serializable, NetSerializable]
public enum ClawMachineVisuals : byte
{
    Spinning,
    NormalSprite
}
