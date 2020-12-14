using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillList : MonoBehaviour
{
    private Dictionary<string,SkillLayout> Skills;

    public SkillLayout SkillPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        Skills = new Dictionary<string, SkillLayout>();
        for(int i = transform.childCount - 1;i >= 0;i--) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void SetSkill(string name, string type, int bonus, bool prof) {
        SkillLayout instance = null;
        if(Skills.ContainsKey(name)) {
            instance = Skills[name];
        } else {
            instance = GameObject.Instantiate<SkillLayout>(SkillPrefab, transform);
            instance.transform.SetParent(transform);
            instance.Name.text = name;
            Skills[name] = instance;
        }
        if(type.Length > 3) instance.Ability.text = type.Substring(0,3).ToUpper();
        else instance.Ability.text = type.ToUpper();
        if(bonus >= 0) instance.Bonus.text = "+" + bonus;
        else instance.Bonus.text = "-" + bonus;
        instance.Proficiency.isOn = prof;
    }

    public void ChangeSkillName(string name, string newName) {
        if(!Skills.ContainsKey(name)) return;
        Skills[newName] = Skills[name];
        Skills.Remove(name);
        Skills[newName].Name.text = newName;
    }

    public void RemoveSkill(string name) {
        GameObject.Destroy(Skills[name].gameObject);
        Skills.Remove(name);
    }


    public void Clear() {
        foreach(KeyValuePair<string, SkillLayout> pair in Skills) {
            GameObject.Destroy(pair.Value.gameObject);
        }
        Skills.Clear();
    }
}
