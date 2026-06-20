using UnityEngine;

[CreateAssetMenu(fileName = "New Sell Item", menuName = "DOHYEONG/Sell Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic")]
    public string itemName;
    public Sprite icon;

    [Header("Trade")]
    public int sellPrice = 5;

    [Header("Stack")]
    public int maxStack = 99;
}