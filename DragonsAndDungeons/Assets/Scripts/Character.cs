using System;
using System.Collections.Generic;

class Character {
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
  public Dictionary <string,List<Item>> EquippedItems { get; private set; }

  public ModifierCollection Modifiers;
  public int FreeHands;

  private ModifierCollection _CombinedMods = new ModifierCollection();
  
/****************** Modifier Management **********************/  
  public void RefreshModifiers() {
    _CombinedMods = new ModifierCollection(Modifiers);
    foreach(KeyValuePair<string,List<Item>> pair in EquippedItems) {
      foreach(Item item in pair.Value) {
        _CombinedMods.Add(item.Modifiers);
      }
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
    StringComparer comp = Definitions.StringComp;
    if(item.Type == "") return false;
    if(FreeHands < item.Hands) {
      return false;
    }

    ExpressionResult requierment = item.Requirement.Evaluate(this);
    if(!requierment.Success) {
      //ERROR
      Console.WriteLine($"Error evaluating item \"{item.Name}\" requirement:\n{requierment.Message}");
      return false;
    }
    if(requierment.Value == 0) return false;

    if(EquippedItems.ContainsKey(item.Type))
      EquippedItems[item.Type].Add(item);
    else
      EquippedItems.Add(item.Type,new List<Item>() { item });
    FreeHands -= item.Hands;
    RefreshModifiers();
    return true;
  }

  public bool Unequip(Item item) {
    if(!EquippedItems.ContainsKey(item.Type)) return false;
    if(!EquippedItems[item.Type].Contains(item)) return false;
    EquippedItems[item.Type].Remove(item);

    if(EquippedItems[item.Type].Count == 0) {
      EquippedItems.Remove(item.Type);
    }
    FreeHands += item.Hands;
    RefreshModifiers();
    return true;
  }

  public bool HasItemEquipped(params string[] args) {
    StringComparer comp = Definitions.StringComp;
    if(args.Length == 0)
      return EquippedItems.Count > 0;
    string type = args[0];
    if(args.Length == 1)
      return EquippedItems.ContainsKey(type);

    foreach(Item item in EquippedItems[type]) {
      for(int i = 1;i < args.Length;i++) {
        if(item.Properties.FindIndex( x => comp.Equals(x, args[i])) == -1) {
          return false;
        }
      }
    }
    return true;
  }

/******************* Skills and Abilities ************************/
  private bool ValidateSkill(string skill) {
    if(!Skills.ContainsKey(skill)) { 
      Console.WriteLine($"Error: \"{skill}\" is not a valid skill.");
      return false;
    }
    return true;
  }

  private bool ValidateAbility(string ability) {
    if(!Abilities.ContainsKey(ability)) { 
      Console.WriteLine($"Error: \"{ability}\" is not a valid ability.");
      return false;
    }
    return true;
  }
  
  public int SkillCheck(string skill) {
    if(!ValidateSkill(skill)) return 0;
    
    int baseValue = Dice.Roll(1,20, Skills[skill].Advantage).Result +
    GetSkillMod(skill);
    return _CombinedMods.Compute(this,skill + " Check", baseValue);
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

  public int AbilityCheck(string ability){
    if(!ValidateAbility(ability)) return 0;
    RefreshModifiers();

    int baseValue = Dice.Roll(1,20, Abilities[ability].Advantage).Result + 
      GetAbilityMod(ability);
    return _CombinedMods.Compute(this,ability + " Check", baseValue);
  }

  public int AbilitySave(string ability) {
    if(!ValidateAbility(ability)) return 0;
    RefreshModifiers();

    int profBonus = 0;
    if(Proficiencies.Contains(ability)) 
      profBonus = GetProficiencyBonus();

    int baseValue = Dice.Roll(1,20, Abilities[ability].Advantage).Result + 
      GetAbilityMod(ability) +
      profBonus;
    return _CombinedMods.Compute(this,ability + "Save", baseValue);
  }
  
  public int GetAbilityMod(string ability) {
    if(!ValidateAbility(ability)) return 0;
    int baseValue = (GetAbilityScore(ability)) / 2 - 5;
    return  _CombinedMods.Compute(this,ability + "Mod", baseValue);
  }

  public int GetAbilityScore(string ability) {
    if(!ValidateAbility(ability)) return 0;
    int output = _CombinedMods.Compute(this,ability,Abilities[ability].Score);;
    return output;
  }
  

  public void SetAbilityScore(string ability, int newScore) {
    if(!ValidateAbility(ability)) return;
    Abilities[ability].Score = newScore;
  }

  public void SetAdvantage(string type, AdvantageType adv) {
    if(Skills.ContainsKey(type)) {
      Skills[type].Advantage = adv;
    } else if (Abilities.ContainsKey(type)) {
      Abilities[type].Advantage = adv;
    }
  }

/******************** Proficiencies ************************/

public void SetProficiency(string type, bool setProf) {
    if(!setProf) {
      Proficiencies.Remove(type);
    } else if (!Proficiencies.Contains(type)){
      Proficiencies.Add(type);
    }
  }

  public bool HasProficiency(string type) {
    return Proficiencies.Contains(type);
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
  public int CurrentHP;
  public int TempHP;

  public bool Dying { get; private set; }
  public int DeathSaves;
  public int DeathFails;
  public bool Dead;

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
      // fail a death save
      DeathFails += 1;
      if(DeathFails >= 3) Dead = true;
      return;
    }

    if (CurrentHP <= 0) {
      CurrentHP = 0;
      Dying = true;
      DeathSaves = 0;
      DeathFails = 0;
    }
  }

  public void MakeDeathSave() {
    int result = Dice.Roll(1,20).Result;
    if(result == 20){
      DeathSaves += 2;
      if(DeathSaves >= 3) Dying = false;
    }else if(result == 1) {
      DeathFails += 2;
      if(DeathFails >= 3) Dead = true;
    } else if(result >= 10) {
      DeathSaves += 1;
      if(DeathSaves >= 3) Dying = false;
    } else {
      DeathFails += 1;
      if(DeathFails >= 3) Dead = true;
    }
  }

  /******************* Turn Actions **********************/

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
    EquippedItems = new Dictionary<string,List<Item>>(Definitions.StringComp);
    FreeHands = 2;

    Level = 1;

    Dying = false;
    Dead = false;
    DeathSaves = 0;
    DeathFails = 0;

    MaxHP = 6;
    CurrentHP = MaxHP;
    TempHP = 0;

  }
}