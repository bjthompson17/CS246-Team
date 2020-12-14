using System;
using System.Collections.Generic;
using UnityEngine;

public class Character {
  private class Ability {
    public int Score;
    public AdvantageType Advantage;

    public Ability(int _score = 10, AdvantageType _adv = AdvantageType.None) {
      Score = _score;
      Advantage = _adv;
    }
    public Ability(Ability oldAbility) {
      Score = oldAbility.Score;
      Advantage = oldAbility.Advantage;
    }
  }


  private class Skill {
    public string Ability;
    public AdvantageType Advantage;

    public Skill(string _ability, AdvantageType _adv = AdvantageType.None) {
      Ability = _ability;
      Advantage = _adv;
    }
    public Skill(Skill oldSkill) {
      Ability = oldSkill.Ability;
      Advantage = oldSkill.Advantage;
    }
  }

  private Dictionary<string,Ability> Abilities;
  private Dictionary<string,Skill> Skills;
  private List<string> Proficiencies;
  
  private Dictionary<string, int> ActionEconomy;

  public List<string> Resistances;
  public List<string> Immunities;
  public List<Item> EquippedItems { get; private set; }

  public ModifierCollection Modifiers;
  public int FreeHands;
  public string Name;

  private ModifierCollection _CombinedMods = new ModifierCollection();
  
/****************** Modifier Management **********************/  
  public void RefreshModifiers() {
    _CombinedMods = new ModifierCollection(Modifiers);
    // TODO: Check for attunement to character
    foreach(Item item in EquippedItems) {
      _CombinedMods.Add(item.Modifiers);
    }
  }

/***************** Levels and Experience ****************/
  public int Level { get; private set; }
  public int Experience { get; private set; }

  public void SetExperience(int newExp) {
    int maxExp = Definitions.ExperienceLvl[Definitions.ExperienceLvl.Length - 1];
    int minExp = Definitions.ExperienceLvl[0];

    if (newExp > maxExp) newExp = maxExp;
    if (newExp < minExp) newExp = minExp;

    Experience = newExp;
    while (Level > 0 && Definitions.ExperienceLvl[Level - 1] > Experience) {
      LevelDown();
    }
  }

  public void AddExperience(int amount) {
    SetExperience(Experience + amount);
  }

  public bool IsLevelPending() {
    return Definitions.ExperienceLvl[Level] <= Experience;
  }

  public void LevelUp() {
    if(IsLevelPending()) {
      Level++;
      // Handle Level up
    }
  }

  public void LevelDown() {
    if(Level <= 1) return;
    Level--;
    // Revert Level up
  }


/****************** Computed Values **********************/
  
  public int GetProficiencyBonus() {
    int baseValue = ((Level - 1) / 4 + 2);
    return _CombinedMods.Compute(this,"Proficiency", baseValue);
  }

  public int GetInitiative() {
    int baseValue = 10 + GetAbilityMod("Dexterity");
    return  _CombinedMods.Compute(this,"Initiative", baseValue);
  }

  public int GetAC() {
    int baseAC = 10 + GetAbilityMod("Dexterity");
    return _CombinedMods.Compute(this,"AC", baseAC);
  }


/******************** Items **********************/
  public bool Equip(Item item) {
    if(FreeHands < item.Hands) {
      return false;
    }

    ExpressionResult requierment = item.Requirement.Evaluate(this);
    if(!requierment.Success) {
      //ERROR
      Debug.LogError($"Error evaluating item \"{item.Name}\" requirement:\n{requierment.Message}");
      return false;
    }
    if(requierment.Value == 0) return false;

    EquippedItems.Add(item);
    FreeHands -= item.Hands;
    RefreshModifiers();
    return true;
  }

  public bool Unequip(Item item) {
    if(!EquippedItems.Contains(item)) return false;
    EquippedItems.Remove(item);

    FreeHands += item.Hands;
    RefreshModifiers();
    return true;
  }

  public bool HasItemEquipped(params string[] args) {
    StringComparer comp = Definitions.StringComp;
    if(args.Length == 0)
      return EquippedItems.Count > 0;

    foreach(Item item in EquippedItems) {
      foreach(string arg in args) {
        if(item.Properties.FindIndex( x => comp.Equals(x, arg)) == -1) {
          return false;
        }
      }
    }
    return true;
  }

/******************* Skills and Abilities ************************/
  private bool ValidateSkill(string skill) {
    if(!Skills.ContainsKey(skill)) { 
      Debug.LogError($"Error: \"{skill}\" is not a valid skill.");
      return false;
    }
    return true;
  }

  private bool ValidateAbility(string ability) {
    if(!Abilities.ContainsKey(ability)) { 
      Debug.LogError($"Error: \"{ability}\" is not a valid ability.");
      return false;
    }
    return true;
  }

  public int GetSkillMod(string skill) {
    if(!ValidateSkill(skill)) return 0;
    int profBonus = 0;
    if(Proficiencies.Contains(skill)) 
      profBonus = GetProficiencyBonus();

    int baseValue = GetAbilityMod(Skills[skill].Ability) + profBonus;
    return _CombinedMods.Compute(this,skill, baseValue);
  }

  public int GetPassiveSkill(string skill) {
    return 10 + GetSkillMod(skill);
  }

  public string[] GetSkills() {
    string[] output = new string[Skills.Count];
    Skills.Keys.CopyTo(output,0);
    return output;
  }

  public string GetSkillType(string skill) {
    if(!Skills.ContainsKey(skill)) return "";
    return Skills[skill].Ability;
  }

  public void SetSkillType(string skill, string type) {
    if(!Skills.ContainsKey(skill)) return;
    if(!Abilities.ContainsKey(type)) return;
    Skills[skill].Ability = type;
  }

  public int GetAbilitySaveMod(string ability) {
    int profBonus = 0;
    if(Proficiencies.Contains(ability)) 
      profBonus = GetProficiencyBonus();
    
    int baseValue = GetAbilityMod(ability);
    return _CombinedMods.Compute(this, ability + "Save", baseValue);
  }
  
  public int GetAbilityMod(string ability) {
    int baseValue = GetAbilityScore(ability) / 2 - 5;
    return  _CombinedMods.Compute(this,ability + "Mod", baseValue);
  }

  public int GetAbilityScore(string ability) {
    int output = _CombinedMods.Compute(this,ability,GetAbilityBase(ability));;
    return output;
  }

  public int GetAbilityBase(string ability) {
    if(!ValidateAbility(ability)) return 0;
    return Abilities[ability].Score;
  }

  public void SetAbilityScore(string ability, int newScore) {
    if(!ValidateAbility(ability)) return;
    Abilities[ability].Score = newScore;
  }

  public string[] GetAbilities() {
    string[] output = new string[Abilities.Count];
    Abilities.Keys.CopyTo(output,0);
    return output;
  }

  public void SetAdvantage(string type, AdvantageType adv) {
    if(Skills.ContainsKey(type)) {
      Skills[type].Advantage = adv;
    } else if (Abilities.ContainsKey(type)) {
      Abilities[type].Advantage = adv;
    }
  }

  public int GetMod(string type) {
    if(Skills.ContainsKey(type)) {
      return GetSkillMod(type);
    } else if (Abilities.ContainsKey(type)) {
      return GetAbilityMod(type);
    } else {
      return _CombinedMods.Compute(this, type, 0);
    }
  }

/******************** Proficiencies ************************/

public void SetProficiency(string type, bool setProf) {
    if(!setProf) {
      Proficiencies.Remove(type.ToUpperInvariant());
    } else if (!Proficiencies.Contains(type)){
      Proficiencies.Add(type.ToUpperInvariant());
    }
  }

  public bool HasProficiency(string type) {
    return Proficiencies.Contains(type.ToUpperInvariant());
  }

/********************** Movement *************************/

  public int Speed;
  public int Movement { get; private set; }

  // TODO: affect Movement calculations with conditions
  public bool CanMove(int amount = 1) {
    return Movement >= amount;
  }

  public bool Move(int amount) {
    if(!CanMove(amount)) return false;
    Movement -= amount;
    return true;
  }


/*************** Damage and DeathSaves ******************/

  public int MaxHP; // Determined by class
  public int CurrentHP { get; private set; }
  public int TempHP;

  public bool Dying { get; private set; }
  public int DeathSaves { get; private set; }
  public int DeathFails { get; private set; }
  public bool Dead { get; private set; }

  public void FailDeathSave(bool critical = false) {
    if(!Dying) return;
    if(critical) DeathFails += 2;
    else DeathFails += 1;
    if(DeathFails >= 3) Dead = true;
  }

  public void SucceedDeathSave(bool critical = false) {
    if(!Dying) return;
    if(critical) DeathSaves += 2;
    else DeathSaves += 1;
    if(DeathSaves >= 3) Dying = false;
  }

  public void DealDamage(int amount, string damageType) {
    if(Immunities.Contains(damageType)) return;
    if(Resistances.Contains(damageType))
      amount /= 2;
    
    CurrentHP -= amount;
    if(CurrentHP <= -MaxHP) {
      Dead = true;
      return;
    }
    
    if(Dying) {
      FailDeathSave();
      return;
    }

    if (CurrentHP <= 0) {
      CurrentHP = 0;
      DeathSaves = 0;
      DeathFails = 0;
      Dying = true;
    }
  }

  public void MakeDeathSave() {
    int result = Dice.Roll(1,20).Result;
    if(result == 20){
      SucceedDeathSave(true);
    }else if(result == 1) {
      FailDeathSave(true);
    } else if(result >= 10) {
      SucceedDeathSave();
    } else {
      FailDeathSave();
    }
  }

  /******************* Turn Actions **********************/
  // Eh. I think all of this will change later
  // Ideas:
  // - Make "Action", "Bonus Action", "Reaction" all general trackers.
  //    Give definition for when trackers reset. i.e. Tracker(Name = "Action", InitialValue = 1, Reset = GameTrigger.TurnStart)
  //    GameTrigger may include: None, TurnStart, RoundStart, DayStart, LongRest, ShortRest, ... Other Triggers we may need
  //    Tracker Stores Name:string, InitialValue:int, CurrentValue: int, Reset:GameTrigger
  //    Store Trackers in Trackers Dictionary
  public void StartTurn() {
    ActionEconomy["Action"] = 1;
    ActionEconomy["Bonus Action"] = 1;
    ActionEconomy["Reaction"] = 1;
    Movement = Speed;
  }
  /**Action**/
  public void Attack(Character target) {
    if(ActionEconomy["Action"] < 1) return;
    
    ActionEconomy["Action"] -= 1;
  }

  public void Dodge() {
    if(ActionEconomy["Action"] < 1) return;
    
    ActionEconomy["Action"] -= 1;
  }

  /**Bonus Action**/

  public void TakeBonusAction(string type) {
    if(ActionEconomy["Bonus Action"] < 1) return;
    // take the Action
    ActionEconomy["Bonus Action"] -= 1;
  }
 /**Reaction**/
  public void TakeReaction(string type) {
    if(ActionEconomy["Reaction"] < 1) return;
    // take the Action
    ActionEconomy["Reaction"] -= 1;
  }


/****************** Constructor *****************/
  public Character() {
    Abilities  = new Dictionary<string, Ability>(Definitions.StringComp) {
      ["Strength"]      = new Ability(),
      ["Constitution"]  = new Ability(),
      ["Intelligence"]  = new Ability(),
      ["Wisdom"]        = new Ability(),
      ["Dexterity"]     = new Ability(),
      ["Charisma"]      = new Ability()
    };

    Skills = new Dictionary<string,Skill>(Definitions.StringComp) {
      ["Acrobatics"]      = new Skill("Dexterity"),
      ["Animal Handling"] = new Skill("Wisdom"),
      ["Arcana"]          = new Skill("Intelligence"),
      ["Athletics"]       = new Skill("Strength"),
      ["Deception"]       = new Skill("Charisma"),
      ["History"]         = new Skill("Intelligence"),
      ["Insight"]         = new Skill("Wisdom"),
      ["Intimidation"]    = new Skill("Charisma"),
      ["Investigation"]   = new Skill("Intelligence"),
      ["Medicine"]        = new Skill("Wisdom"),
      ["Nature"]          = new Skill("Intelligence"),
      ["Perception"]      = new Skill("Wisdom"),
      ["Performance"]     = new Skill("Charisma"),
      ["Persuasion"]      = new Skill("Charisma"),
      ["Religion"]        = new Skill("Intelligence"),
      ["Slight of Hand"]  = new Skill("Dexterity"),
      ["Stealth"]         = new Skill("Dexterity"),
      ["Survival"]        = new Skill("Wisdom")
    };

    ActionEconomy = new Dictionary<string,int>() {
      ["Action"]        = 1,
      ["Bonus Action"]  = 1,
      ["Reaction"]      = 1
    };

    Modifiers = new ModifierCollection();
    Resistances = new List<string>();
    Immunities = new List<string>();
    Proficiencies = new List<string>();
    EquippedItems = new List<Item>();
    FreeHands = 2;

    Level = 1;

    Dying = false;
    Dead = false;
    DeathSaves = 0;
    DeathFails = 0;

    MaxHP = 6;
    CurrentHP = MaxHP;
    TempHP = 0;

    Name = "Arnold";
  }
}