using System;
using System.Collections.Generic;

/*******************************************
Modifier Collection
A collection of Modifiers sorted into buckets by type.
Wraps Dictionary<string,List<Modifier>>

All modifiers of a specific type can be computed for any
given Character by calling the Compute method:

Methods:
  void Add:
   + Add(Modifier)
   + Add(IEnumerable<Modifier>)
   + Add(ModifierCollection)
  void Remove:
   + Remove(Modifier)
   + Remove(IEnumerable<Modifier>)
   + Remove(ModifierCollection)
   + Remove(string: type, int: index)
   + Remove(string: type, string: name)
   + Remove(string: name)
  void RemoveAll:
   + RemoveAll(string: name)
   + RemoveAll(string: type, string: name)

  bool Contains(Modifier: mod)

  Get:
   + Modifier Get(string: type, int: index)
   + List<Modifier> Get(string: type)
   + string[] GetTypes()
  
  int Compute(Character: target, string: type, int: baseValue)

******************************************/
class ModifierCollection {
  private Dictionary<string,List<Modifier>> Mods = new Dictionary<string,List<Modifier>>(Definitions.StringComp);

  public ModifierCollection() {}
  public ModifierCollection(IEnumerable<Modifier> mods) {
    Add(mods);
  }
  public ModifierCollection(Modifier mod) {
    Add(mod);
  }

  public ModifierCollection(ModifierCollection oldCollection) {
    foreach(KeyValuePair<string,List<Modifier>> oldPair in oldCollection.Mods) {
      Mods.Add(oldPair.Key, new List<Modifier>());
      foreach(Modifier oldMod in oldPair.Value) {
        Mods[oldPair.Key].Add(new Modifier(oldMod));
      }
    }
  }

  private bool ValidateType(string type) {
    if(Mods.ContainsKey(type)) return true;
    Console.WriteLine("Error: " + type + " is not a valid modifier in this collection");
    return false;
  }

  public void Add(Modifier newMod) {
    if(!Mods.ContainsKey(newMod.Type))
      Mods.Add(newMod.Type, new List<Modifier>() { newMod });
    else
      Mods[newMod.Type].Add(newMod);
  }
  public void Add(IEnumerable<Modifier> newMods) {
    foreach(Modifier mod in newMods) {
      Add(mod);
    }
  }
  public void Add(ModifierCollection newMods) {
    foreach(KeyValuePair<string,List<Modifier>> pair in newMods.Mods) {
      if(!Mods.ContainsKey(pair.Key))
        Mods.Add(pair.Key,new List<Modifier>(pair.Value));
      else
        Mods[pair.Key].AddRange(pair.Value);
    }
  }

  public void Remove(string type, int index) {
    if(!ValidateType(type)) return;
    if(index < 0 || index >= Mods[type].Count) return;
    Mods[type].RemoveAt(index);
    if(Mods[type].Count == 0)
      Mods.Remove(type);
  }
  public void Remove(string type, string name) {
    if(!ValidateType(type)) return;
    for(int i = 0;i < Mods[type].Count;i++) {
      if(Definitions.StringComp.Equals(name, Mods[type][i].Name)) {
        Mods[type].RemoveAt(i);
        return;
      }
    }
  }
  public void Remove(string name) {
    foreach(KeyValuePair<string,List<Modifier>> pair in Mods){
      for(int i = 0;i < pair.Value.Count;i++) {
        if(Definitions.StringComp.Equals(name, pair.Value[i].Name)) {
          pair.Value.RemoveAt(i);
          return;
        }
      }
    }
  }
  public void Remove(Modifier mod) {
    if(!ValidateType(mod.Type)) return;
    Mods[mod.Type].Remove(mod);
    if(Mods[mod.Type].Count == 0)
      Mods.Remove(mod.Type);
  }
  public void Remove(IEnumerable<Modifier> oldMods) {
    foreach(Modifier mod in oldMods) {
      Remove(mod);
    }
  }
  public void Remove(ModifierCollection oldMods) {
    foreach(KeyValuePair<string,List<Modifier>> pair in oldMods.Mods) {
      if(Mods.ContainsKey(pair.Key)) {
        foreach(Modifier mod in pair.Value) {
          Mods[pair.Key].Remove(mod);
        }
      }else continue;
    }
  }

  public void RemoveAll(string type, string name) {
    if(!ValidateType(type)) return;
    for(int i = 0;i < Mods[type].Count;i++) {
      if(Definitions.StringComp.Equals(name, Mods[type][i].Name)) {
        Remove(type,i);
      }
    }
  }
  public void RemoveAll(string name) {
    foreach(KeyValuePair<string,List<Modifier>> pair in Mods){
      for(int i = 0;i < pair.Value.Count;i++) {
        if(Definitions.StringComp.Equals(name, pair.Value[i].Name)) {
          pair.Value.RemoveAt(i);
        }
      }
    }
  }

  public bool Contains(Modifier mod) {
    if(!Mods.ContainsKey(mod.Type)) return false;
    return Mods[mod.Type].Contains(mod);
  }

  public Modifier Get(string type, int index) {
    if(!ValidateType(type)) return null;
    if(index < 0 || index >= Mods[type].Count) return null;
    return Mods[type][index];
  }
  public List<Modifier> Get(string type) {
    if(!Mods.ContainsKey(type)) return new List<Modifier>();
    return Mods[type];
  }
  public string[] GetTypes() {
    string[] output = new string[Mods.Count];
    Mods.Keys.CopyTo(output,0);
    return output;
  }

  private bool Compute_RecursionLock = false;
  public int Compute(Character target, string type, int baseValue) {
    if(Compute_RecursionLock) return baseValue;
    if(!Mods.ContainsKey(type)) return baseValue;
    Compute_RecursionLock = true;

    int totalMod = 0;

    int maxOverwrite = 0;
    int maxBase = 0;

    bool overwritten = false;
    bool baseSet = false;

    foreach(Modifier mod in Mods[type]) {
      if(!mod.IsActive(target)) continue;
      // Debugging info
      //Console.WriteLine($"Evaluating modifier {mod.Name}: {mod.Type} => {(string)mod.Expr}");
      ExpressionResult result = mod.Evaluate(target);
      int modValue = (int)result.Value;
      if(mod.Expr.HasVariable("SET")) {
        int setValue = 0;
        foreach(double d in mod.Expr.GetVariable("SET")){
          setValue += (int)d;
        }

        if(!overwritten || setValue > maxOverwrite)
          maxOverwrite = setValue;
        overwritten = true;
      } else if (result.Message.StartsWith("ANS")) {
        totalMod += modValue;
      } else {
        int newBase = modValue;
        if(!baseSet || newBase > maxBase) 
          maxBase = newBase;
        baseSet = true;
      }
    }

    if(overwritten) {
      totalMod = 0;
      baseValue = maxOverwrite;
    } else if(baseSet) {
      baseValue = maxBase;
    }
    Compute_RecursionLock = false;
    return baseValue + totalMod;
  }
  
}

//TODO: Maybe modify advantage, damages, and dice rules?
class Modifier {
  public string Name { get; private set; }
  public string Type { get; private set; }
  public Expression Expr;
  public Expression Condition;
  public bool Active = true;

  public Modifier(string type, Expression exprIn) 
    : this(type + ": " + exprIn, type, exprIn, new Expression("TRUE")) {}

  public Modifier(string type, Expression exprIn, Expression condition) 
    : this(type + ": " + exprIn, type, exprIn, condition) {}

  public Modifier(string nameIn, string type, Expression exprIn)
    : this(nameIn, type, exprIn, new Expression("TRUE")) {}
    
  public Modifier(string nameIn, string type, Expression exprIn, Expression condition) {
    Name = nameIn;
    Type = type;
    Expr = exprIn;
    Condition = condition;
  }

  public Modifier (Modifier oldMod) {
    Name = oldMod.Name;
    Type = oldMod.Type;
    Expr = new Expression(oldMod.Expr);
    Condition = new Expression(oldMod.Condition);
  }

  public bool IsActive(Character target) {
    if(!Active) return false;
    ExpressionResult result = Condition.Evaluate(target);
    if(!result.Success) {
      //ERORR
      Console.WriteLine("Modifier \"{0}\" Condition Error:\n{1}", Name, result.Message);
      return false;
    }
    if(result.Value == 0) return false;
    return true;
  }

  public ExpressionResult Evaluate(Character target) {
    if(!IsActive(target)) return new ExpressionResult(false, "Modifier not active", new double[] { 0 });

    ExpressionResult result = Expr.Evaluate(target);
    if(!result.Success) {
      //ERORR
      Console.WriteLine("Modifier \"{0}\" Expression Error:\n{1}", Name, result.Message);
      return result;
    }
    return result;
  }
}
