using System.Collections.Generic;
using UnityEngine;

public class ModifierList : MonoBehaviour
{
    ModifierLayout ModifierPrefab;

    private Dictionary<Modifier, ModifierLayout> DisplayedModifiers;

    void Start(){
        DisplayedModifiers = new Dictionary<Modifier, ModifierLayout>();
        for(int i = transform.childCount - 1;i >= 0;i--) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void DisplayModifiers(ModifierCollection mods) {
        List<Modifier> RemoveModifiers = new List<Modifier>(mods.Count);
        foreach(Modifier key in DisplayedModifiers.Keys) {
            if(!mods.Contains(key)) RemoveModifiers.Add(key);
        }
        foreach(string type in mods.GetTypes()) {
            foreach(Modifier mod in mods.Get(type)){
                SetModifier(mod);
            }
        }
        foreach(Modifier mod in RemoveModifiers) {
            DisplayedModifiers.Remove(mod);
        }
    }

    public void SetModifier(Modifier mod) {
        ModifierLayout instance = null;
        if(!DisplayedModifiers.ContainsKey(mod)) {
            instance = GameObject.Instantiate<ModifierLayout>(ModifierPrefab);
            instance.Name.text = mod.Name;
            instance.Type.text = mod.Type;
            DisplayedModifiers[mod] = instance;
        } else {
            instance = DisplayedModifiers[mod];
        }
        instance.Expression.text = mod.Expr;
        instance.Condition.text = mod.Condition;
        if(GameManager.SelectedToken != null){
            instance.IsActive.isOn = mod.IsActive(GameManager.SelectedToken.LinkedCharacter);
        } else {
            instance.IsActive.isOn = false;
        }
    }

    public void Remove(Modifier mod) {
        GameObject.Destroy(DisplayedModifiers[mod].gameObject);
        DisplayedModifiers.Remove(mod);
    }

    public void Clear() {
        foreach(KeyValuePair<Modifier, ModifierLayout> pair in DisplayedModifiers) {
            GameObject.Destroy(pair.Value.gameObject);
        }
        DisplayedModifiers.Clear();
    }
}
