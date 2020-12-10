using System;
using System.Collections.Generic;

class Item {
  public string Name = "";
  public double Weight = 0;
  public double Cost = 0;
  public string Description = "";
  private string _Type = "Adventuring Gear";
  public string Type { 
    get {return _Type;} 
    set { _Type = value.Trim(); } 
  }

  public int Hands = 0;
  public Expression Requirement = new Expression("TRUE");
  public ModifierCollection Modifiers = new ModifierCollection();
  public bool RequiresAttunement = false;

  // Weapon info
  public Expression Damage1H = new Expression("1d4");
  public Expression Damage2H = new Expression("1d4");
  public string DamageType = "Bludgeoning";
  public int Range = 20;
  public int LongRange = 60;
  public List<string> Properties = new List<string>();
  public ItemRarity Rarity = ItemRarity.Common;

  // Default Constructor
  public Item() {}


  // Copy Constructor
  public Item(Item oldItem) {
      this.Weight = oldItem.Weight;
      this.Cost = oldItem.Cost;
      this.Description = oldItem.Description;
      this._Type = oldItem._Type;
      this.Name = oldItem.Name;
      this.Requirement = oldItem.Requirement;
      this.Hands = oldItem.Hands;
      this.Modifiers = new ModifierCollection(oldItem.Modifiers);
      this.Damage1H = oldItem.Damage1H;
      this.Damage2H = oldItem.Damage2H;
      this.DamageType = oldItem.DamageType;
      this.Range = oldItem.Range;
      this.LongRange = oldItem.LongRange;
      this.Properties = new List<string>(oldItem.Properties);
      this.RequiresAttunement = oldItem.RequiresAttunement;
  }
}