namespace Ggross.CardPileFramework;

using Godot;
using System;
using Godot.Collections;

[Tool]
public partial class Card : Control
{
    #region Signals
    [Signal]
    public delegate void CardHoveredEventHandler(Card card);
    [Signal]
    public delegate void CardUnhoveredEventHandler(Card card);
    [Signal]
    public delegate void CardClickedEventHandler(Card card);
    [Signal]
    public delegate void CardDroppedEventHandler(Card card);
    [Signal] 
    public delegate void CardDataUpdatedEventHandler(Card card);    
    #endregion
    

    [Export]
    public CardData cardData;

    [Export] protected TextureRect frontface, backface;    
    public bool isClicked = false;
    public bool mouseIsHovering = false;
    public Vector2 targetPosition = Vector2.Zero;
    [Export] protected float returnSpeed = 0.2f;
    [Export] protected int hoverDistance = 10;
    [Export] protected bool dragWhenClicked = true;
    protected int lastChildCount = 0;

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

    protected virtual void SetDisabled(bool val)
    {
        if (val)
        {
            mouseIsHovering = false;
            isClicked = false;
            Rotation = 0;
        }
    }

    protected virtual bool IsInteractable()
    {
        return true;
    }

    protected virtual void OnMouseEntered()
    {
        if (IsInteractable())
        {
            mouseIsHovering = true;
            targetPosition.Y -= hoverDistance;
            EmitSignal(nameof(CardHovered), this);
        }
    }

    protected virtual void OnMouseExited()
    {

        if (!isClicked && mouseIsHovering)
        {
            mouseIsHovering = false;
            targetPosition.Y += hoverDistance;
            EmitSignal(nameof(CardUnhovered), this);
        }
    }

    protected virtual void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {

            if (mouseEvent.Pressed)
            {
                if (IsInteractable())
                {
                    isClicked = true;
                    Rotation = 0;
                    EmitSignal(nameof(CardClicked), this);
                }
            }
            else
            {
                if (isClicked)
                {
                    isClicked = false;
                    mouseIsHovering = false;
                    Rotation = 0;

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

    protected void GetDropzones(Node node, string className, Godot.Collections.Array result)
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