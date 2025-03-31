namespace Ggross.CardPileFramework;

using Godot;
using System;
using Godot.Collections;
using System.Buffers;

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
    

    [Export] public CardData cardData;
    [Export] protected TextureRect frontface, backface;    

    public bool IsClicked {get; protected set;}
    public bool IsMouseHovering {get; protected set;}
    public Vector2 TargetPosition {get; set;}
    public float ReturnSpeed {get; set;}
    public int HoverDistance {get; set;}
    public bool DragWhenClicked {get; set;}

    protected CardManager manager;

    public override void _Ready()
    {
        manager = GetParent<CardManager>();

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        GuiInput += OnGuiInput;

    }

    public override void _Process(double delta)
    {
        if (IsClicked)
        {
            if(DragWhenClicked){
                TargetPosition = GetGlobalMousePosition() - CustomMinimumSize * 0.5f;
            }
            Position = TargetPosition;
        }
        else{
            if(Position != TargetPosition){
                Position = MathUtils.Vector2Lerp(Position, TargetPosition, ReturnSpeed);
            }
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
            }

            if(!string.IsNullOrEmpty(backfaceTexturePath)){
                backface.Texture = GD.Load<Texture2D>(backfaceTexturePath);
            }

            
            MouseFilter = MouseFilterEnum.Pass;
        }
        
    }

    public void SetControlParameters(float _returnSpeed, int _hoverDistance, bool _dragWhenClicked){
        ReturnSpeed = _returnSpeed;
        HoverDistance = _hoverDistance;
        DragWhenClicked = _dragWhenClicked;
    }

    public void SetDirection(Vector2 cardIsFacing)
    {
        backface.Visible = cardIsFacing == Vector2.Down;
        frontface.Visible = cardIsFacing == Vector2.Up;
    }

    // protected virtual void SetDisabled(bool val)
    // {
    //     if (val)
    //     {
    //         IsMouseHovering = false;
    //         IsClicked = false;
    //         Rotation = 0;
    //     }
    // }

    public virtual bool IsInteractive()
    {
        if(!Visible) return false;
        if(manager == null) return true;
        else{
            var dropzone = manager.GetCardDropzone(this);
            if (dropzone != null)
            {
                return dropzone.IsCardInteractive(this);
            }
            else{
                return true;
            }
        }
    }

    protected virtual void OnMouseEntered()
    {
        if (IsInteractive())
        {
            IsMouseHovering = true;

            TargetPosition = TargetPosition += new Vector2(0, -HoverDistance);
            
            EmitSignal(SignalName.CardHovered, this);
        }
    }

    protected virtual void OnMouseExited()
    {

        if (!IsClicked && IsMouseHovering)
        {
            IsMouseHovering = false;

            TargetPosition = TargetPosition += new Vector2(0, HoverDistance);

            EmitSignal(SignalName.CardUnhovered, this);
        }
    }

    protected virtual void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {

            if (mouseEvent.Pressed)
            {
                if (IsInteractive())
                {
                    IsClicked = true;
                    Rotation = 0;
                    EmitSignal(SignalName.CardClicked, this);
                }
            }
            else
            {
                if (IsClicked)
                {
                    IsClicked = false;
                    IsMouseHovering = false;
                    Rotation = 0;

                    var allDropzones = new Godot.Collections.Array();
                    GetDropzones(GetTree().Root, "CardDropzone", allDropzones);

                    foreach (CardDropzone dropzone in allDropzones)
                    {
                        if (dropzone.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                        {
                            if (dropzone.CanDropCard(this))
                            {
                                dropzone.DropCard(this);
                                break;
                            }
                        }
                    }
                    
                    EmitSignal(SignalName.CardUnhovered, this);
                    EmitSignal(SignalName.CardDropped, this);
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
}