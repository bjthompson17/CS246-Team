using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityList : MonoBehaviour
{

    
    private Dictionary<string,AbilityLayout> Abilities;

    public AbilityLayout AbilityPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        Abilities = new Dictionary<string, AbilityLayout>();
        for(int i = transform.childCount - 1;i >= 0;i--) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void SetAbility(string name, int baseScore, int totalScore, int bonus, bool prof) {
        AbilityLayout instance = null;
        if(Abilities.ContainsKey(name)) {
            instance = Abilities[name];
        } else {
            instance = GameObject.Instantiate<AbilityLayout>(AbilityPrefab, transform);
            instance.transform.SetParent(transform);
            instance.Name.text = name;
            Abilities[name] = instance;
        }
        instance.BaseScore.text = baseScore.ToString();
        instance.TotalScore.text = totalScore.ToString();
        if(bonus >= 0) instance.Bonus.text = "+" + bonus;
        else instance.Bonus.text = "-" + bonus;
        instance.Proficiency.isOn = prof;
    }

    public void ChangeAbilityName(string name, string newName) {
        if(!Abilities.ContainsKey(name)) return;
        Abilities[newName] = Abilities[name];
        Abilities.Remove(name);
        Abilities[newName].Name.text = newName;
    }

    public void RemoveAbility(string name) {
        GameObject.Destroy(Abilities[name].gameObject);
        Abilities.Remove(name);
    }


    public void Clear() {
        foreach(AbilityLayout ar in Abilities.Values) {
            GameObject.Destroy(ar.gameObject);
        }
        Abilities.Clear();
    }
}
