using System;
using System.Collections.Generic;

enum AdvantageType {
  Disadvantage = -1,
  None = 0,
  Advantage = 1
}

enum ItemRarity {
  Common,
  Uncommon,
  Rare,
  VeryRare,
  Legendary
}

static class Definitions {
  public static int Copper = 1;
  public static int Silver = 10;
  public static int Gold = 100;
  public static int Platinum = 1000;

  public static StringComparer StringComp = 
    StringComparer.InvariantCultureIgnoreCase;
  public static List<string> Abilities = new List<string> {
    "Strength",
    "Consititution",
    "Dexterity",
    "Intelligence",
    "Wisdom",
    "Charisma"
  };

  public static List<string> Skills = new List<string> {
    "Acrobatics",
    "Animal Handling",
    "Arcana",
    "Athletics",
    "Deception",
    "History",
    "Insight",
    "Intimidation",
    "Investigation",
    "Medicine",
    "Nature",
    "Perception",
    "Performance",
    "Persuasion",
    "Religion",
    "Sleight of Hand",
    "Stealth",
    "Survival"
  };

  public static Dictionary<string,string> SkillType = new Dictionary<string,string>() {
    ["Acrobatics"]      = "Dexterity",
    ["Animal Handling"] = "Wisdom",
    ["Arcana"]          = "Intelligence",
    ["Athletics"]       = "Strength",
    ["Deception"]       = "Charisma",
    ["History"]         = "Intelligence",
    ["Insight"]         = "Wisdom",
    ["Intimidation"]    = "Charisma",
    ["Investigation"]   = "Intelligence",
    ["Medicine"]        = "Wisdom",
    ["Nature"]          = "Intelligence",
    ["Perception"]      = "Wisdom",
    ["Performance"]     = "Charisma",
    ["Persuasion"]      = "Charisma",
    ["Religion"]        = "Intelligence",
    ["Slight of Hand"]  = "Dexterity",
    ["Stealth"]         = "Dexterity",
    ["Survival"]        = "Wisdom"
  };

  public static List<string> WeaponTypes = new List<string>() {
    "Simple",
    "Martial"
  };


  public static readonly string[] WeaponProperties = {
    "Melee",
    "Ranged",
    "Thrown",
    "Loading",
    "Two Handed",
    "Light",
    "Heavy",
    "Finesse",
    "Ammunition",
    "Reach",
    "Special",
    "Versitile"
  };

  public static readonly string[] TurnAcitons = {
    "Action",
    "Bonus Action",
    "Reaction"
  };

  public static readonly string[] Actions = {
    "Attack",
    "Dodge",
    "Improvise"
  };


  public static int[] ExperienceLvl = {0,300,900,2700,6500,14000,23000,34000,48000,64000,85000,100000,120000,140000,165000,195000,225000,265000,305000,355000};
}