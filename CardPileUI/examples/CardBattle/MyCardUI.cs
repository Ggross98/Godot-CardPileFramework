using Godot;
using System;

public partial class MyCardUI : CardUI{
    [Export] 
    private Label nameLabel, typeLabel, costLabel;

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
    }
}
