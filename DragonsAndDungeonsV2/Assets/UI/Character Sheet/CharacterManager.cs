using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public InputField NameField;
    public Text PassivePreception;
    public Text AC;
    public Text Speed;
    public Slider HealthBar;
    public Text HealthFraction;
    public Text HitDice;
    public AbilityList AbilityListObject;
    public SkillList SkillListObject;
    public ModifierList ModifierListObject;

    public void UpdateAll() {
        Character currentCharacter = GetCurrentCharacter();
        if(currentCharacter != null) currentCharacter.RefreshModifiers();
        UpdateName(currentCharacter);
        UpdateHealth(currentCharacter);
        UpdateAC(currentCharacter);
        UpdatePassivePerception(currentCharacter);
        UpdateSpeed(currentCharacter);
        UpdateAbilites(currentCharacter);
        UpdateSkills(currentCharacter);
    }

    private Character GetCurrentCharacter() {
        if(GameManager.SelectedToken == null) return null;
        return GameManager.SelectedToken.LinkedCharacter;
    }

    public void UpdateHealth() {
        UpdateHealth(GetCurrentCharacter());
    }

    private void UpdateHealth(Character currentCharacter) {
        if(HealthBar == null) return;
        if(currentCharacter == null) {
            HealthBar.SetValueWithoutNotify(1);
            HealthFraction.text = "--/--";
        } else {
            HealthBar.SetValueWithoutNotify(((float)currentCharacter.CurrentHP)/currentCharacter.MaxHP);
            HealthFraction.text = $"{currentCharacter.CurrentHP}/{currentCharacter.MaxHP}";
        }
    }

    public void UpdateAC() {
        UpdateAC(GetCurrentCharacter());
    }

    private void UpdateAC(Character currentCharacter) {
        if(AC == null) return;
        if(currentCharacter != null){
            AC.text = currentCharacter.GetAC().ToString();
        } else {
            AC.text = "--";
        }
    }

    public void UpdateName() {
        UpdateName(GetCurrentCharacter());
    }

    private void UpdateName(Character currentCharacter) {
        if(NameField == null) return;
        if(currentCharacter != null){
            NameField.SetTextWithoutNotify(currentCharacter.Name);
        } else {
            NameField.SetTextWithoutNotify("Select A Character");
        }
    }
    public void UpdatePassivePerception() {
        UpdatePassivePerception(GetCurrentCharacter());
    }

    private void UpdatePassivePerception(Character currentCharacter){
        if(PassivePreception == null) return;
        if(currentCharacter != null){
            PassivePreception.text = currentCharacter.GetPassiveSkill("Perception").ToString();
        } else {
            PassivePreception.text = "--";
        }
    }

    public void UpdateSpeed() {
        UpdateSpeed(GetCurrentCharacter());
    }
    private void UpdateSpeed(Character currentCharacter){
        if(Speed == null) return;
        if(currentCharacter != null){
            Speed.text = currentCharacter.Speed.ToString();
        } else {
            Speed.text = "--";
        }
    }
    public void UpdateSkills() {
        UpdateSkills(GetCurrentCharacter());
    }

    private void UpdateSkills(Character currentCharacter) {
        if(SkillListObject == null) return;
        if(currentCharacter == null) {
            SkillListObject.Clear();
            return;
        }
        foreach(string skillname in currentCharacter.GetSkills()) {
            SkillListObject.SetSkill(skillname,
            currentCharacter.GetSkillType(skillname),
            currentCharacter.GetSkillMod(skillname),
            currentCharacter.HasProficiency(skillname));
        }
    }

    public void UpdateAbilites() {
        UpdateAbilites(GetCurrentCharacter());
    }
    private void UpdateAbilites(Character currentCharacter) {
        if(AbilityListObject == null) return;
        if(currentCharacter == null) {
            AbilityListObject.Clear();
            return;
        }
        foreach(string abilityName in currentCharacter.GetAbilities()) {
            AbilityListObject.SetAbility(abilityName,
                currentCharacter.GetAbilityBase(abilityName), 
                currentCharacter.GetAbilityScore(abilityName),
                currentCharacter.GetAbilityMod(abilityName),
                currentCharacter.HasProficiency(abilityName));
        }
    }

    public void UpdateModifiers() {
        UpdateModifiers(GetCurrentCharacter());
    }
    private void UpdateModifiers(Character currentCharacter) {
        if(ModifierListObject == null) return;
        if(currentCharacter == null) {
            ModifierListObject.Clear();
        } else {
            ModifierListObject.DisplayModifiers(currentCharacter.Modifiers);
        }
    }
}
