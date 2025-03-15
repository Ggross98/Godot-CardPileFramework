using Godot;
using System;

public partial class MathUtils
{
    public static Vector2 Vector2Lerp(Vector2 from, Vector2 to, float weight){
        return new Vector2(Mathf.Lerp(from.X, to.X, weight), Mathf.Lerp(from.Y, to.Y, weight));
    }
}
