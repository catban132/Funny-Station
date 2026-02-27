using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Random.Helpers;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.SlotMachine.GachaMachine;

/// <summary>
/// This handles the coinflipper machine logic
/// </summary>
public sealed class GachaMachineSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedStackSystem _stackSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GachaMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<GachaMachineComponent, GachaMachineDoAfterEvent>(OnSlotMachineDoAfter);
        SubscribeLocalEvent<GachaMachineComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, GachaMachineComponent comp, ref GotEmaggedEvent args)
    {
        if(comp.Emagged)
            return;

        args.Handled = true;
        comp.Emagged = true;

        comp.Rewards = comp.EvilRewards; //My name is nhoj nhoj and I am EVIL
    }
    private void OnInteractHandEvent(EntityUid uid, GachaMachineComponent comp, ActivateInWorldEvent args)
    {
        if (comp.IsSpinning || !_power.IsPowered(uid))
            return;

        if (!_itemSlots.TryGetSlot(uid, "money", out var slot)
            || slot.Item == null
            || !TryComp<StackComponent>(slot.Item.Value, out var stack)
            || stack.Count < comp.SpinCost)
        {
            _popupSystem.PopupPredicted(Loc.GetString("slotmachine-no-money"), uid, uid, PopupType.Small); // No Money
            return;
        }

        var doAfter =
         new DoAfterArgs(EntityManager, args.User, comp.DoAfterTime, new GachaMachineDoAfterEvent(), uid)
         {
             BreakOnMove = true,
             BreakOnDamage = true,
             MultiplyDelay = false,
         };
        _stackSystem.SetCount(stack.Owner, stack.Count - comp.SpinCost, stack);
        Dirty(stack.Owner, stack);
        comp.IsSpinning = true;

        if (_net.IsServer)
        {
            _audio.PlayPvs(comp.PlaySound, uid);
            _doAfter.TryStartDoAfter(doAfter);
        }
    }

    private void OnSlotMachineDoAfter(EntityUid uid, GachaMachineComponent comp, GachaMachineDoAfterEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args.Cancelled)
        {
            var selfMsgFail = Loc.GetString("GachaMachine-fail-self");
            var othersMsgFail = Loc.GetString("GachaMachine-fail-other", ("user", args.User));
            comp.IsSpinning = false;
            _popupSystem.PopupPredicted(selfMsgFail, othersMsgFail, args.User, args.User, PopupType.Small);
            Dirty(uid, comp);
            return;
        }
        comp.IsSpinning = false;
        Dirty(uid, comp);
        if (!_net.IsServer)
            return;

        if (_random.Prob(comp.WinChance) && comp.Rewards != null)
        {
            _audio.PlayPvs(comp.WinSound, uid);

            var rewardToSpawn = _random.Pick(comp.Rewards);

            var coordinates = Transform(uid).Coordinates;
            EntityManager.SpawnEntity(rewardToSpawn, coordinates);

            return;
        }

        _popupSystem.PopupEntity(Loc.GetString("GachaMachine-fail-generic"), uid);
        _audio.PlayPvs(comp.LoseSound, uid);
    }
}
