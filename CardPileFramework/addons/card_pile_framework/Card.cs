namespace Ggross.CardPileFramework;

using Godot;

[Tool]
public partial class Card : Control
{
    #region Signals
    [Signal]
    public delegate void CardHoveredEventHandler(Card card);

    [Signal]
    public delegate void CardUnhoveredEventHandler(Card card);

    [Signal]
    public delegate void CardLeftClickedEventHandler(Card card);

    [Signal]
    public delegate void CardLeftReleasedEventHandler(Card card);

    [Signal]
    public delegate void CardRightClickedEventHandler(Card card);

    [Signal]
    public delegate void CardRightReleasedEventHandler(Card card);

    [Signal]
    public delegate void CardDataUpdatedEventHandler(Card card);
    #endregion

    protected CardData cardData;
    public CardData CardData
    {
        get => cardData;
        set
        {
            if (cardData != null)
                cardData.Changed -= OnCardDataUpdated;
            cardData = value;
            if (cardData != null)
                cardData.Changed += OnCardDataUpdated;
            OnCardDataUpdated();
        }
    }

    [Export]
    protected TextureRect frontface,
        backface;

    public bool IsClicked { get; protected set; }
    public bool IsMouseHovering { get; protected set; }
    public Vector2 TargetPosition { get; set; }
    public float ReturnSpeed { get; set; }
    public int HoverDistance { get; set; }
    public bool DragWhenClicked { get; set; }

    public CardManager Manager { get; set; }

    public override void _Ready()
    {
        if (Manager == null && GetParent() is CardManager parentManager)
            Manager = parentManager;

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        GuiInput += OnGuiInput;
    }

    public override void _Process(double delta)
    {
        if (IsClicked)
        {
            if (DragWhenClicked)
                TargetPosition = GetGlobalMousePosition() - CustomMinimumSize * 0.5f;
            Position = TargetPosition;
        }
        else if (Position != TargetPosition)
        {
            Position = Position.Lerp(TargetPosition, ReturnSpeed);
        }
    }

    public virtual void UpdateDisplay()
    {
        if (cardData == null)
            return;

        if (!string.IsNullOrEmpty(cardData.FrontfaceTexturePath))
        {
            frontface.Texture = GD.Load<Texture2D>(cardData.FrontfaceTexturePath);
            CustomMinimumSize = frontface.Texture.GetSize();
            PivotOffset = frontface.Texture.GetSize() / 2;
        }

        if (!string.IsNullOrEmpty(cardData.BackfaceTexturePath))
            backface.Texture = GD.Load<Texture2D>(cardData.BackfaceTexturePath);

        MouseFilter = MouseFilterEnum.Pass;
    }

    public void SetControlParameters(float returnSpeed, int hoverDistance, bool dragWhenClicked)
    {
        ReturnSpeed = returnSpeed;
        HoverDistance = hoverDistance;
        DragWhenClicked = dragWhenClicked;
    }

    public void SetDirection(Vector2 cardIsFacing)
    {
        if (backface != null)
            backface.Visible = cardIsFacing == Vector2.Down;
        if (frontface != null)
            frontface.Visible = cardIsFacing == Vector2.Up;
    }

    public virtual bool IsInteractive()
    {
        if (!Visible)
            return false;
        if (Manager == null)
            return true;

        var pile = Manager.GetPile(this);
        return pile == null || pile.IsCardInteractive(this);
    }

    protected virtual void OnCardDataUpdated()
    {
        UpdateDisplay();
        EmitSignal(SignalName.CardDataUpdated, this);
    }

    protected virtual void OnMouseEntered()
    {
        if (!IsInteractive())
            return;

        IsMouseHovering = true;
        Manager?.UpdateCardsTargetPosition();
        Manager?.UpdateCardsZIndex();
        EmitSignal(SignalName.CardHovered, this);
    }

    protected virtual void OnMouseExited()
    {
        if (IsClicked || !IsMouseHovering)
            return;

        IsMouseHovering = false;
        Manager?.UpdateCardsTargetPosition();
        Manager?.UpdateCardsZIndex();
        EmitSignal(SignalName.CardUnhovered, this);
    }

    protected virtual void OnGuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent)
            return;

        if (mouseEvent.IsPressed())
        {
            var interactive = IsInteractive();
            if (interactive)
            {
                IsClicked = true;
                Rotation = 0;
                Manager?.UpdateCardsZIndex();
            }

            if (mouseEvent.ButtonIndex == MouseButton.Left)
                EmitSignal(SignalName.CardLeftClicked, this);
            else if (mouseEvent.ButtonIndex == MouseButton.Right)
                EmitSignal(SignalName.CardRightClicked, this);
        }
        else if (mouseEvent.IsReleased() && IsClicked)
        {
            IsClicked = false;
            Rotation = 0;

            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                Manager?.HandleCardReleased(this);
                EmitSignal(SignalName.CardLeftReleased, this);
                EmitSignal(SignalName.CardUnhovered, this);
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                EmitSignal(SignalName.CardRightReleased, this);
                EmitSignal(SignalName.CardUnhovered, this);
            }

            IsMouseHovering = false;
            Manager?.UpdateCardsTargetPosition();
            Manager?.UpdateCardsZIndex();
        }
    }
}
