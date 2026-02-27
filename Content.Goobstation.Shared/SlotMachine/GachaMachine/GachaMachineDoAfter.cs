using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlotMachine.GachaMachine;

[Serializable, NetSerializable]
public sealed partial class GachaMachineDoAfterEvent : SimpleDoAfterEvent;
