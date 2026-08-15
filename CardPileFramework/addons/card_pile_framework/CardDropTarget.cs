namespace Ggross.CardPileFramework;

using Godot;

/// <summary>
/// Hit-test region for a released card. Emits a signal only; never changes pile membership.
/// </summary>
[GlobalClass]
public partial class CardDropTarget : Control
{
    [Signal]
    public delegate void CardDroppedOnTargetEventHandler(Card card, CardDropTarget target);

    public CardManager Manager { get; set; }

    public bool IsMouseHovering { get; protected set; }

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public override void _Process(double delta)
    {
        var hovering = GetGlobalRect().HasPoint(GetGlobalMousePosition());
        if (IsMouseHovering && !hovering)
            EmitSignal(SignalName.MouseExited);
        if (!IsMouseHovering && hovering)
            EmitSignal(SignalName.MouseEntered);
        IsMouseHovering = hovering;
    }

    public bool ContainsGlobalPoint(Vector2 point) => GetGlobalRect().HasPoint(point);

    public void NotifyDropped(Card card)
    {
        OnCardDropped(card);
        EmitSignal(SignalName.CardDroppedOnTarget, card, this);
    }

    protected virtual void OnCardDropped(Card card) { }

    protected virtual void OnMouseEntered() { }

    protected virtual void OnMouseExited() { }
}
