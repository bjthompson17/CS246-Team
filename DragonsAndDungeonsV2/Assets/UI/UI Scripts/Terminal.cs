
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Terminal : MonoBehaviour
{
    public InputField TargetInput;
    public Text Output;

    private Expression TerminalExpr = new Expression("0");
    private Character TargetCharacter;
    private double[] previousResult = new double[] { 0 };
    // Start is called before the first frame update
    void Start()
    {
        
    }

    string[] ParseArgs(string args) {
        List<string> output = new List<string>();
        bool inQuotes = false;
        bool escaped = false;
        string substring = "";
        for(int i = 0;i < args.Length;i++) {

            if (args[i] == ' ' && !inQuotes) {
                output.Add(substring);
                substring = "";
                continue;
            }
            if(args[i] == '\"' && !escaped) {
                inQuotes = !inQuotes;
                continue;
            }
            if (args[i] == '\\' && !escaped) {
                escaped = true;
                continue;
            }
            if(escaped && args[i] != '\"') {
                substring += '\\' + args[i];
            } else {
                substring += args[i];
            }
            escaped = false;
        }
        output.Add(substring);
        return output.ToArray();
    }

    public void Submit() {
        if(GameManager.SelectedToken != null)
            TargetCharacter = GameManager.SelectedToken.LinkedCharacter;
        else
            TargetCharacter = null;
        string input = TargetInput.text;

        if(input.StartsWith("/")) {
            string[] args = ParseArgs(input);
            switch(args[0]) {
                case "/say":
                    string message = "";
                    for(int i = 1;i < args.Length;i++) {
                        if(i != 1) message += " ";
                        message += args[i];
                    }
                    Log(message);
                break;
                default:
                    Log(args[0] + " is not a valid command");
                break;
            }
        } else {
            if(input.Length <= 0) return;
            TerminalExpr.Expr = input;
            ExpressionResult result = TerminalExpr.Evaluate(previousResult, TargetCharacter);
            if(result.Success)
                previousResult = result.Values;
            Log($"{result.Message} => [{String.Join(",",previousResult)}]");
        }
    }

    public void Log(string info = "") {
        Output.text += "\n" + info;
    }

    public void Clear() {
        Output.text = "";
        previousResult = new double[] { 0 };
    }
}
