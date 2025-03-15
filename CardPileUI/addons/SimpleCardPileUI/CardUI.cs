using Godot;
using System;
using Godot.Collections;

[Tool]
public partial class CardUI : Control
{
    [Signal]
    public delegate void CardHoveredEventHandler(CardUI card);
    [Signal]
    public delegate void CardUnhoveredEventHandler(CardUI card);
    [Signal]
    public delegate void CardClickedEventHandler(CardUI card);
    [Signal]
    public delegate void CardDroppedEventHandler(CardUI card);
    [Signal] 
    public delegate void CardDataUpdatedEventHandler(CardUI card);

    [Export]
    public CardUIData cardData;

    [Export] protected TextureRect frontface, backface;
    // private string frontfaceTexturePath;
    // private string backfaceTexturePath;

    public bool isClicked = false;
    public bool mouseIsHovering = false;
    public Vector2 targetPosition = Vector2.Zero;
    [Export] protected float returnSpeed = 0.2f;
    [Export] protected int hoverDistance = 10;
    [Export] protected bool dragWhenClicked = true;

    public override void _Ready()
    {

        if (Engine.IsEditorHint())
        {
            SetDisabled(true);
            UpdateConfigurationWarnings();
            return;
        }

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        GuiInput += OnGuiInput;

        
    }

    public virtual void UpdateDisplay(){
        // GD.Print(cardData.GetPropertyList());
        if(cardData != null){
            var frontfaceTexturePath = cardData.Get("frontface_texture_path").As<string>();
            var backfaceTexturePath = cardData.Get("backface_texture_path").As<string>();

            // GD.Print(frontfaceTexturePath);

            if (!string.IsNullOrEmpty(frontfaceTexturePath))
            {
                frontface.Texture = GD.Load<Texture2D>(frontfaceTexturePath);
                CustomMinimumSize = frontface.Texture.GetSize();
                PivotOffset = frontface.Texture.GetSize() / 2;
                MouseFilter = MouseFilterEnum.Pass;
            }

            if(!string.IsNullOrEmpty(backfaceTexturePath)){
                backface.Texture = GD.Load<Texture2D>(backfaceTexturePath);
            }
        }
        
    }

    public void SetControlParameters(float _returnSpeed, int _hoverDistance, bool _dragWhenClicked){

        returnSpeed = _returnSpeed;
        hoverDistance = _hoverDistance;
        dragWhenClicked = _dragWhenClicked;
    }

    public void SetDirection(Vector2 cardIsFacing)
    {
        backface.Visible = cardIsFacing == Vector2.Down;
        frontface.Visible = cardIsFacing == Vector2.Up;
    }

    private void SetDisabled(bool val)
    {
        if (val)
        {
            mouseIsHovering = false;
            isClicked = false;
            Rotation = 0;
            var parent = GetParent();
            if (parent is CardPileUI cardPileUI)
            {
                cardPileUI.ResetCardUiZIndex();
            }
        }
    }

    private bool CardCanBeInteractedWith()
    {
        var parent = GetParent();
        var valid = false;

        if (parent is CardPileUI cardPileUI)
        {
            if (cardPileUI.IsCardInHand(this))
            {
                valid = cardPileUI.HandEnabled && !cardPileUI.IsAnyCardUiClicked();
            }

            var dropzone = cardPileUI.GetCardDropzone(this);
            if (dropzone != null)
            {
                valid = dropzone.GetTopCard() == this && !cardPileUI.IsAnyCardUiClicked();
            }
        }

        return valid;
    }

    private void OnMouseEntered()
    {
        if (CardCanBeInteractedWith())
        {
            mouseIsHovering = true;
            targetPosition.Y -= hoverDistance;
            var parent = GetParent();
            if (parent is CardPileUI cardPileUI)
            {
                cardPileUI.ResetCardUiZIndex();
            }
            EmitSignal(nameof(CardHovered), this);
        }
    }

    private void OnMouseExited()
    {
        if (isClicked)
        {
            return;
        }

        if (mouseIsHovering)
        {
            mouseIsHovering = false;
            targetPosition.Y += hoverDistance;
            var parent = GetParent();
            if (parent is CardPileUI cardPileUI)
            {
                cardPileUI.ResetCardUiZIndex();
            }
            EmitSignal(nameof(CardUnhovered), this);
        }
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            var parent = GetParent();

            if (mouseEvent.Pressed)
            {
                if (CardCanBeInteractedWith())
                {
                    isClicked = true;
                    Rotation = 0;
                    if (parent is CardPileUI cardPileUI)
                    {
                        cardPileUI.ResetCardUiZIndex();
                    }
                    EmitSignal(nameof(CardClicked), this);
                }

                if (parent is CardPileUI cardPileUI2 && 
                    cardPileUI2.GetCardPileSize(CardPileUI.Piles.DrawPile) > 0 && 
                    cardPileUI2.HandEnabled &&
                    cardPileUI2.GetCardsInPile(CardPileUI.Piles.DrawPile).Contains(this) && 
                    !cardPileUI2.IsAnyCardUiClicked() && 
                    cardPileUI2.ClickDrawPileToDraw)
                {
                    cardPileUI2.Draw(1);
                }
            }
            else
            {
                if (isClicked)
                {
                    isClicked = false;
                    mouseIsHovering = false;
                    Rotation = 0;

                    if (parent is CardPileUI cardPileUI3 && cardPileUI3.IsCardInHand(this))
                    {
                        cardPileUI3.CallDeferred("ResetTargetPositions");
                    }

                    var allDropzones = new Godot.Collections.Array();
                    GetDropzones(GetTree().Root, "CardDropzone", allDropzones);

                    foreach (CardDropzone dropzone in allDropzones)
                    {
                        if (dropzone.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                        {
                            if (dropzone.CanDropCard(this))
                            {
                                dropzone.OnCardDropped(this);
                                break;
                            }
                        }
                    }

                    EmitSignal(nameof(CardDropped), this);
                    EmitSignal(nameof(CardUnhovered), this);
                }
            }
        }
    }

    private void GetDropzones(Node node, string className, Godot.Collections.Array result)
    {
        if (node is CardDropzone)
        {
            result.Add(node);
        }

        foreach (Node child in node.GetChildren())
        {
            GetDropzones(child, className, result);
        }
    }



    public override void _Process(double delta)
    {
        if (isClicked && dragWhenClicked)
        {
            targetPosition = GetGlobalMousePosition() - CustomMinimumSize * 0.5f;
        }
        if (isClicked)
        {
            Position = targetPosition;
        }
        else if (Position != targetPosition)
        {
            Position = MathUtils.Vector2Lerp(Position, targetPosition, returnSpeed);
        }

        if (Engine.IsEditorHint() && lastChildCount != GetChildCount())
        {
            UpdateConfigurationWarnings();
            lastChildCount = GetChildCount();
        }
    }

    private int lastChildCount = 0;

    public override string[] _GetConfigurationWarnings()
    {
        if (GetChildCount() != 2)
        {
            return ["This node must have 2 TextureRect as children, one named `Frontface` and one named `Backface`."];
        }

        foreach (Node child in GetChildren())
        {
            if (!(child is TextureRect) || (child.Name != "Frontface" && child.Name != "Backface"))
            {
                return ["This node must have 2 TextureRect as children, one named `Frontface` and one named `Backface`."];
            }
        }

        return [];
    }
}