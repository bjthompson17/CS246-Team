using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterEditor : MonoBehaviour
{
    public Text WindowTitle;
    public InputField Name;
    public InputField Experience;
    public InputField MaxHP;
    public InputField Speed;
    public InputList Abilities;
    public InputList Skills;
    public InputList Modifiers;

    private Token Target;
    private bool edit = true;

    void Start() {
        gameObject.SetActive(false);
        Target = null;
    }
    private Character SelectedCharacter() {
        if(Target != null)
            return Target.LinkedCharacter;
        if(GameManager.SelectedToken != null)
            return GameManager.SelectedToken.LinkedCharacter;
        return null;
    }
    public void SubmitChanges() {
        if(!edit) return;
        Character currentCharacter = SelectedCharacter();
        if(currentCharacter == null) return;
        try {
            currentCharacter.Name = Name.text;
            currentCharacter.MaxHP = Int32.Parse(MaxHP.text);
            currentCharacter.Speed = Int32.Parse(Speed.text);
            currentCharacter.SetExperience(Int32.Parse(Experience.text));
            if(Abilities != null) {
                InputRow[] abilityRows = Abilities.GetAllRows();
                foreach(InputRow row in abilityRows) {
                    string ability = row.RowElements[0].GetComponent<Text>().text;
                    currentCharacter.SetAbilityScore(ability,Int32.Parse(row.RowElements[1].GetComponent<InputField>().text));
                    currentCharacter.SetProficiency(ability, row.RowElements[2].GetComponent<Toggle>().isOn);
                }
            }

            if(Skills != null) {
                InputRow[] skillRows = Skills.GetAllRows();
                foreach(InputRow row in skillRows) {
                    string Skill = row.RowElements[0].GetComponent<Text>().text;
                    currentCharacter.SetSkillType(Skill, row.RowElements[1].GetComponent<InputField>().text);
                    currentCharacter.SetProficiency(Skill, row.RowElements[2].GetComponent<Toggle>().isOn);
                }
            }

            if(Modifiers != null) {
                InputRow[] modifierRows = Modifiers.GetAllRows();
                currentCharacter.Modifiers.Clear();
                foreach(InputRow row in modifierRows) {
                    Modifier newMod = new Modifier(
                        row.RowElements[0].GetComponent<InputField>().text,
                        row.RowElements[1].GetComponent<InputField>().text,
                        (Expression)row.RowElements[3].GetComponent<InputField>().text,
                        (Expression)row.RowElements[4].GetComponent<InputField>().text
                    );
                    newMod.Active = row.RowElements[2].GetComponent<Toggle>().isOn;
                    currentCharacter.Modifiers.Add(newMod);
                }
            }

        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
        if(Target != null)
            Target.RefreshInfo();
        GameManager.windowManager.CharacterManager.UpdateAll();
    }

    private void Populate() {
        Character currentCharacter = SelectedCharacter();
        if(currentCharacter == null) {
            WindowTitle.text = "No Character Selected";
            return;
        }
        if(edit)
            WindowTitle.text = "Edit " + currentCharacter.Name;
        else
            WindowTitle.text = "View " + currentCharacter.Name;
        Name.text = currentCharacter.Name;
        MaxHP.text = currentCharacter.MaxHP.ToString();
        Speed.text = currentCharacter.Speed.ToString();
        Experience.text = currentCharacter.Experience.ToString();

        if(!edit) {
            Name.interactable = false;
            MaxHP.interactable = false;
            Speed.interactable = false;
            Experience.interactable = false;
            Abilities.interactable = false;
            Skills.interactable = false;
            Modifiers.interactable = false;
        } else {
            Name.interactable = true;
            MaxHP.interactable = true;
            Speed.interactable = true;
            Experience.interactable = true;
            Abilities.interactable = true;
            Skills.interactable = true;
            Modifiers.interactable = true;
        }

        if(Abilities == null) return;
        string[] abilities = currentCharacter.GetAbilities();
        foreach(string ability in abilities) {
            InputRow instance = Abilities.AddRow();
            instance.RowElements[0].GetComponent<Text>().text = ability;
            instance.RowElements[1].GetComponent<InputField>().text = 
                currentCharacter.GetAbilityBase(ability).ToString();
            instance.RowElements[2].GetComponent<Toggle>().isOn = 
                currentCharacter.HasProficiency(ability);
            if(!edit) {
                instance.RowElements[1].GetComponent<InputField>().interactable = false;
                instance.RowElements[2].GetComponent<Toggle>().interactable = false;
                instance.interactable = false;
            }
        }

        if(Skills == null) return;
        string[] skills = currentCharacter.GetSkills();
        foreach(string skill in skills) {
            InputRow instance = Skills.AddRow();
            instance.RowElements[0].GetComponent<Text>().text = skill;
            instance.RowElements[1].GetComponent<InputField>().text = 
                currentCharacter.GetSkillType(skill);
            instance.RowElements[2].GetComponent<Toggle>().isOn = 
                currentCharacter.HasProficiency(skill);
            if(!edit) {
                instance.RowElements[1].GetComponent<InputField>().interactable = false;
                instance.RowElements[2].GetComponent<Toggle>().interactable = false;
                instance.interactable = false;
            }
        }

        if(Modifiers == null) return;
        string[] modifierTypes = currentCharacter.Modifiers.GetTypes();
        foreach(string type in modifierTypes) {
            foreach(Modifier mod in currentCharacter.Modifiers.Get(type)) {
                InputRow instance = Modifiers.AddRow();
                instance.RowElements[0].GetComponent<InputField>().SetTextWithoutNotify(mod.Name);
                instance.RowElements[1].GetComponent<InputField>().SetTextWithoutNotify(mod.Type);
                instance.RowElements[2].GetComponent<Toggle>().isOn = mod.Active;
                instance.RowElements[3].GetComponent<InputField>().SetTextWithoutNotify(mod.Expr);
                instance.RowElements[4].GetComponent<InputField>().SetTextWithoutNotify(mod.Condition);
                if(!edit) {
                    instance.RowElements[0].GetComponent<InputField>().interactable = false;
                    instance.RowElements[1].GetComponent<InputField>().interactable = false;
                    instance.RowElements[2].GetComponent<Toggle>().interactable = false;
                    instance.RowElements[3].GetComponent<InputField>().interactable = false;
                    instance.RowElements[4].GetComponent<InputField>().interactable = false;
                    instance.interactable = false;
                }
            }
        }
    }

    public void OpenEditor(Token target = null, bool edit = true){
        Target = target;
        if(Abilities != null)
            Abilities.Clear();
        if(Skills != null)
            Skills.Clear();
        if(Modifiers != null)
            Modifiers.Clear();
        this.edit = edit;
        Populate();
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
        gameObject.SetActive(true);
    }

    public void CloseEditor() {
        Target = null;
        if(Abilities != null)
            Abilities.Clear();
        if(Skills != null)
            Skills.Clear();
        if(Modifiers != null)
            Modifiers.Clear();
        gameObject.SetActive(false);
    }
    
}
