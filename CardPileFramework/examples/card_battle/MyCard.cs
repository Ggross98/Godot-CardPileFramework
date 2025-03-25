using Ggross.CardPileFramework;
using Godot;
using System;

public partial class MyCard : SimpleCard{
    [Export] 
    private Label nameLabel, typeLabel, costLabel;

    [Export]
    private TextureRect image;

    public override void _Ready()
    {
        base._Ready();

        if(cardData != null){
            cardData.Changed += UpdateDisplay;
            UpdateDisplay();
        }
        
    }

    public override void UpdateDisplay(){
        base.UpdateDisplay();
        
        var data = (MyCardData)cardData;

        nameLabel.Text = data.nice_name;
        typeLabel.Text = data.type;
        costLabel.Text = data.cost + "";

        image.Texture = GD.Load<Texture2D>(data.image_texture_path);
    }

}
