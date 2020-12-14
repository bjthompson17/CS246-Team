//#define DEBUG
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public struct ExpressionResult {
  public readonly bool Success;
  public readonly string Message;
  public readonly double[] Values;

  public double Value {
    get {
      double sum = 0;
      foreach(double val in Values) {
        sum += val;
      }
      return sum;
    }
  }

  public ExpressionResult(bool success, string message, double[] result) {
    Success = success;
    Message = message;
    Values = result;
  }
}

public class Expression {
  private string _Expr;
  public string Expr {
    get => _Expr;
    set => _Expr = value.ToUpperInvariant();
  }

  private ExpressionCalculator.VariableCollection Vars = 
    new ExpressionCalculator.VariableCollection();

  public Expression(Expression oldExpr) {
    Expr = oldExpr.Expr;
    Vars = new ExpressionCalculator.VariableCollection(oldExpr.Vars);
  }

  public Expression() {
    Expr = "0";
  }

  public Expression(string expr) {
    Expr = expr;
  }

  public static explicit operator Expression(string expr) {
    return new Expression(expr);
  }
  public static implicit operator string(Expression expr) {
    return expr.Expr;
  }

  public void SetVariable(string id, double[] value) {
    Vars[id] = value;
  }

  public double[] GetVariable(string id) {
    if(!Vars.Contains(id))
      return new double[] { 0 };
    else
      return Vars[id];
  }

  public bool HasVariable(string id) {
    return Vars.GetReferenceCount(id) > 0;
  }

  public int GetReferenceCount(string id) {
    return Vars.GetReferenceCount(id);
  }

  public string[] GetVariableIds() {
    return Vars.GetIds();
  }

  public ExpressionResult Evaluate(Character target = null) {
    return Evaluate(0, target);
  }

  public ExpressionResult Evaluate(double initialValue = 0, Character target = null) {
    return ExpressionCalculator.Evaluate(new double[] { initialValue }, Expr, target, Vars);
  }
  public ExpressionResult Evaluate(double[] initialValues, Character target = null) {
    return ExpressionCalculator.Evaluate(initialValues, Expr, target, Vars);
  }
}

public static class ExpressionCalculator {

/********************* Data Structures *******************/
  public class VariableCollection {
    private struct Variable {
      private double[] _value;
      public double[] Value
      { 
        get { 
          ReferenceCount++;
          return _value;
        }
        private set {
          _value = value;
        }
      }
      public int ReferenceCount { get; private set; }
      public Variable(double[] value) {
        _value = value;
        ReferenceCount = 1;
      }
    }
    
    private Dictionary<string,Variable> variables = new Dictionary<string,Variable>();
    
    public VariableCollection() {}

    public VariableCollection(VariableCollection oldCollection) {
      variables = new Dictionary<string,Variable>(oldCollection.variables);
    }

    public double[] this[string key] {
      get { return Get(key); }
      set { Set(key, value); }
    }

    public bool Contains(string key) {
      return variables.ContainsKey(key.ToUpperInvariant());
    }

    public double[] Get(string key) {
      return variables[key.Trim().ToUpperInvariant()].Value; 
    }

    public void Set(string key, double[] value) {
      if(key.Trim() == "") return;
      variables[key.Trim().ToUpperInvariant()] = new Variable(value);
    }

    public int GetReferenceCount(string id) {
      if(Contains(id)) return variables[id.ToUpperInvariant()].ReferenceCount;
      else return 0;
    }

    public void Remove(string key) {
      variables.Remove(key.ToUpperInvariant());
    }
    public int Count() {
      return variables.Count;
    }
    public string[] GetIds() {
      string[] output = new string[variables.Count];
      variables.Keys.CopyTo(output,0);
      return output;
    }
  }

/******************** Regex Definitions ****************************/
  #region Regex Definitons
  static string inside_parenthesis = @"(?:(?<p>\()|[^()]|(?<-p>\)))*";
  static string inside_braces = @"(?:(?<c>{)|[^{}]|(?<-c>}))*";
  static string inside_brackets = @"(?:(?<b>\[)|[^\[\]]|(?<-b>\]))+";

  static string op = @"(?<op>(?:[><!]=)|[+\-*/><=]|OR|\|\||AND|&&)";
  static string parenthesis = $@"(?:(?<paren_mul_l>-?\d*)\((?<paren>{inside_parenthesis})\)(?<paren_mul_r>\d*))";
  static string dice = @"(?<dice>(?<d_count>\d+)d(?<d_sides>\d+)(?:r\[(?<reroll_nums>\d+(?:,\d+)*)\])?(?<d_adv>[a,d])?)";
  static string variable = @"(?<var>[A-Z_]\w*)";
  static string constant = $@"(?<const>-?\d+|\[(?<const_list>{inside_brackets})\])";
  static string pre_op = @"(?<pre_op>!|NOT)";
  static string access = @"(?<access>\[(?<access_start>\d+)(?<access_range>(?:\.\.(?<access_end>\d*)))?\])";
  static string post_op = @"(?<post_op>!=|=|\+|-|\?)";
  static string value_repeat = @"(?:{(?<repeat>\d+)})";
  static string named_value = $@"(?<named_value>(?<name>[A-Z_]\w*){{(?<named_inside>{inside_braces})}})";
  static string list_op = $@"(?<list_op>[A-Z_]+)(?<list_count>\d+)?\[(?<list_inside>{inside_brackets})\]";
  static string function = $@"(?<func>[A-Z_]\w*)\((?<func_args>{inside_parenthesis})\)";

  static string value = $@"(?:{named_value}|{list_op}|{function}|{parenthesis}|{dice}|{variable}|{constant})";

  static string rx_expr = $@"(?:\s*{op}?\s*(?:{pre_op}?{value}{access}?{post_op}?{value_repeat}?)?\s*)";
  #endregion
  static Regex rx = new Regex(rx_expr,RegexOptions.Compiled|RegexOptions.IgnoreCase);

/********************************************************************/
  
  #region Evaluate Overloads
  public static ExpressionResult Evaluate(string expr, Character charRef = null) {
    return Evaluate(0,expr,charRef,new VariableCollection());
  }
  public static ExpressionResult Evaluate(string expr, VariableCollection variables) {
    return Evaluate(new double[] { 0 },expr,null,variables);
  }
  public static ExpressionResult Evaluate(string expr, Character charRef, VariableCollection variables) {
    return Evaluate(new double[] { 0 },expr,charRef,variables);
  }

  public static ExpressionResult Evaluate(double[] initialValues,string expr, Character charRef = null) {
    return Evaluate(initialValues, expr, charRef,new VariableCollection());
  }
  public static ExpressionResult Evaluate(double[] initialValues,string expr, VariableCollection variables) {
    return Evaluate(initialValues, expr,null,variables);
  }

  public static ExpressionResult Evaluate(double initialValue,string expr, Character charRef = null) {
    return Evaluate(new double[] { initialValue }, expr, charRef,new VariableCollection());
  }
  public static ExpressionResult Evaluate(double initialValue,string expr, VariableCollection variables) {
    return Evaluate(new double[] { initialValue }, expr,null,variables);
  }

  public static ExpressionResult Evaluate(double initialValue,string expr, Character charRef,VariableCollection variables) {
    return Evaluate(new double[] { initialValue }, expr,null,variables);
  }
  #endregion
  public static ExpressionResult Evaluate(double[] initialValues, string expr, Character charRef, VariableCollection variables) {
    expr = expr.ToUpperInvariant();
    MatchCollection matches = rx.Matches(expr);
    Stack<double[]> values = new Stack<double[]>();
    // Stack of tuples of the form ([operator],[order of operation])
    // where higher orders of operation are evaluated first.
    Stack<(string,int)> operators = new Stack<(string,int)>();

    // String used to keep track of what is being evaluated vs. ignored.
    // Return this with the result for useful feedback
    string evaluated_string = "";

    variables["ANS"] = initialValues;

    foreach(Match match in matches) {
      if(match.Value == "") continue;
      GroupCollection groups = match.Groups;
      double[] value = new double[]{ 0 };
      int opOrder = -1;

      // Insert the operator into the evaluated string
      if(groups["op"].Value != "") {
        if(values.Count <= 0) {
          evaluated_string += $"ANS";
          values.Push(variables["ANS"]);
        }
        evaluated_string += $" {groups["op"].Value} ";
      }

      // Insert preoperator into the evaluated string
      if (groups["pre_op"].Value != "") {
        evaluated_string += groups["pre_op"].Value;
      }

      // Prepare the parsed value
      #region Dice
      if(groups["dice"].Value != "") {
        evaluated_string += groups["dice"].Value;

        int d_count = Int32.Parse(groups["d_count"].Value);
        int d_sides = Int32.Parse(groups["d_sides"].Value);

        AdvantageType adv = AdvantageType.None;
        if(groups["d_adv"].Value == "a") adv = AdvantageType.Advantage;
        else if (groups["d_adv"].Value == "d") adv = AdvantageType.Disadvantage;

        (int[] results, int sum) = Dice.Roll(d_count, d_sides, adv);

        if(groups["reroll_nums"].Value != "") {
          evaluated_string += " -";
          int[] nums = Array.ConvertAll(groups["reroll_nums"].Value.Split(','), Int32.Parse);

          for(int i = 0;i < results.Length;i++) {
            if(Array.IndexOf(nums, results[i]) != -1) {
              #if (DEBUG)
              Console.WriteLine("rerolling: {0}", results[i]);
              #endif
              evaluated_string += results[i] + "-";
              sum -= results[i];
              results[i] = Dice.Roll(1,d_sides).Item2;
              sum += results[i];
            }
          }
        }

        value = Array.ConvertAll<int,double>(results,x => x);
        evaluated_string += $" ({String.Join(",",results)})";
      }
      #endregion
      #region Constant
      else if(groups["const"].Value != "") {
        if(groups["const_list"].Value != "") {
          string[] args = SplitBalanced(',',groups["const_list"].Value);
          List<double> tempValue = new List<double>(args.Length);
          evaluated_string += "[";
          for(int i = 0;i < args.Length;i++) {
            args[i] = args[i].Trim();
            ExpressionResult result = Evaluate(args[i],charRef,variables);
            if(!result.Success) {
              return result;
            }

            if(i != 0) evaluated_string += ",";
            evaluated_string += result.Message;

            tempValue.AddRange(result.Values);
          }
          value = tempValue.ToArray();
          evaluated_string += "]";
        }else {
          value = new double[] { Int32.Parse(groups["const"].Value) };
          evaluated_string += groups["const"].Value;
        }
      }
      #endregion
      #region Variable
      else if(groups["var"].Value != "") {
        string id = groups["var"].Value;
        if(id == "TRUE") {
          value[0] = 1;
        }else if (id == "FALSE"){
          value[0] = 0;
        }else if(variables.Contains(id)) {
          value = variables[id];
        } else if(charRef == null){
          return new ExpressionResult(false, "No character given to reference", new double[] { 0 });
        } else {
          switch(id) {
            case "STR":
              value[0] = charRef.GetAbilityMod("Strength");
            break;
            case "CON":
              value[0] = charRef.GetAbilityMod("Constitution");
            break;
            case "DEX":
              value[0] = charRef.GetAbilityMod("Dexterity");
            break;
            case "WIS":
              value[0] = charRef.GetAbilityMod("Wisdom");
            break;
            case "INT":
              value[0] = charRef.GetAbilityMod("Intelligence");
            break;
            case "CHA":
              value[0] = charRef.GetAbilityMod("Charisma");
            break;
            case "PRO":
              value[0] = charRef.GetProficiencyBonus();
            break;
            case "LVL":
              value[0] = charRef.Level;
            break;
            case "AC":
              value[0] = charRef.GetAC();
            break;
            case "HP":
              value[0] = charRef.CurrentHP;
            break;
            default:
              return new ExpressionResult(false, $"Invalid variable name \"{id}\"",new double[] { 0 });
          }
        }
        evaluated_string += String.Format("{0} ({1})",id,String.Join(",",value));
      }
      #endregion
      #region Parenthesis
      else if(groups["paren"].Value != "") {
        #if (DEBUG)
          Console.WriteLine("{0}({1}){2}",groups["paren_mul_l"],groups["paren"],groups["paren_mul_r"]);
          Console.WriteLine("=================");
        #endif

        ExpressionResult result = Evaluate(groups["paren"].Value,charRef,variables);
        if(!result.Success)
          return result;
        value = result.Values;
        if(groups["paren_mul_l"].Value != "") {
          if(groups["paren_mul_l"].Value == "-") {
            value = calculate("*",value,new double[] { (double)Int32.Parse(groups["paren_mul_l"].Value + "1") });
          } else {
            value = calculate("*",value,new double[] { (double)Int32.Parse(groups["paren_mul_l"].Value) });
          }
          evaluated_string += groups["paren_mul_l"].Value;
        }

        evaluated_string += $"({result.Message})";

        if(groups["paren_mul_r"].Value != "") {
          value = calculate("*",new double[] { (double)Int32.Parse(groups["paren_mul_r"].Value) },value);
          evaluated_string += groups["paren_mul_r"].Value;
        }

        
        #if (DEBUG)
          Console.WriteLine("=================");
        #endif
      } 
      #endregion
      #region Named Value
      else if (groups["named_value"].Value != ""){
        string key = groups["name"].Value;
        ExpressionResult result = Evaluate(groups["named_inside"].Value, charRef, variables);
        if(!result.Success)
          return result;
        value = result.Values;
        evaluated_string += $"{key}{{{result.Message}}}";
        variables[key] = value;
      }
      #endregion
      #region List Operations
      else if (groups["list_op"].Value != "") {
        string opName = groups["list_op"].Value;
        
        int resultCount = 1;
        if(groups["list_count"].Value != "") {
          resultCount = Int32.Parse(groups["list_count"].Value);
        }
        if(resultCount <= 0) resultCount = 1;

        // Deal with the operation name
        Func<double, double, bool> compare;
        switch(opName) {
          case "MAX":
            compare = delegate (double _new, double _old) { return _new > _old; };
          break;
          case "MIN":
            compare = delegate (double _new, double _old) { return _new < _old; };
          break;
          default:
            return new ExpressionResult(false, $"{opName} is not a valid list operation",new double[] { 0 });
        }

        // Parse the arguments
        string[] exprs = SplitBalanced(',',groups["list_inside"].Value);
        if(exprs.Length <= 0) return new ExpressionResult(false, $"{opName} must have at least 1 value", new double[] { 0 });
        
        evaluated_string += $"{opName}{resultCount}[";

        // initialize comparison values:
        double[] compValues = new double[resultCount];
        int[] indicies = new int[resultCount];
        int totalSize = 0;
        int initIndex = 0;
        
        for(int i = 0;i < exprs.Length;i++){
          ExpressionResult result = Evaluate(exprs[i],charRef,variables);
          if(!result.Success)
            return result;

          if(i != 0) evaluated_string += ", ";
          evaluated_string += result.Message;
          
          // set comparison values to the first item in the first expression's result
          // finish comparison if needed
          for(int j = 0;j < result.Values.Length;j++) {
            double newVal = result.Values[j];
            // if our initial values aren't filled in, fill them first.
            // no comparison is needed for the initial set.
            if(initIndex < resultCount) {
              compValues[initIndex] = newVal;
              indicies[initIndex] = initIndex;
              initIndex++;
              totalSize++;
              continue;
            }

            int k = 0;
            double oldVal = 0;
            int oldIndex = 0;
            for(;k < resultCount;k++)
            { 
              // if we have a match, store the old value,
              // plug in the new one, and break out of this loop
              if(compare(newVal,compValues[k])){
                oldVal = compValues[k];
                oldIndex = indicies[k];
                compValues[k] = newVal;
                indicies[k] = totalSize;
                break;
              }
            }
            // sift the old value down the rest of the results
            for(;k < resultCount;k++) {
              if(compare(oldVal,compValues[k])) {
                compValues[k] = oldVal;
                indicies[k] = oldIndex;
              }
            }
            totalSize++;
          }
        }

        value = new double[totalSize];
        for(int i = 0;i < compValues.Length;i++) {
          value[indicies[i]] = compValues[i];
        }
        evaluated_string += $"] ({String.Join(",",value)})";
      }
      #endregion
      #region Functions
      else if (groups["func"].Value != "") {
        string funcName = groups["func"].Value;

        string[] funcArgs = SplitBalanced(',',groups["func_args"].Value);
        if(funcArgs[0] == "") funcArgs = new string[0];
        for(int i = 0;i < funcArgs.Length;i++) {
          funcArgs[i] = funcArgs[i].Trim();
        }

        if(charRef == null) {
          return new ExpressionResult(false, "No character given to reference", new double[] { 0 });
        } else if (funcName == "EQUIPPED"){
          if(charRef.HasItemEquipped(funcArgs)) value[0] = 1;
          else value[0] = 0;
        }else{
          switch(funcName + funcArgs.Length.ToString()) {
            case "HASPROF1":
              if(charRef.HasProficiency(funcArgs[0])) value[0] = 1;
              else value[0] = 0;
            break;

            case "ABILITYSCORE1":
              value[0] = charRef.GetAbilityScore(funcArgs[0]);
            break;

            case "ABILITYMOD1":
              value[0] = charRef.GetAbilityMod(funcArgs[0]);
            break;

            case "SKILLMOD1":
              value[0] = charRef.GetSkillMod(funcArgs[0]);
            break;
            
            default:
            return new ExpressionResult(false, $"{funcName} with {funcArgs.Length} arguments is not a valid function",new double[] { 0 });
          }
        }
        evaluated_string += $"{funcName}({String.Join(",",funcArgs)}) ({value[0]})";
      }
      #endregion
      
      else {
        return new ExpressionResult(false, 
        $"Invalid Operator\n \"{expr}\"\n"+
        " " + underlineError(match), new double[] { 0 });
      }

      #region Post value operations
      // Handle preoperator
      if (groups["pre_op"].Value != "") {
        value = calculate_single(groups["pre_op"].Value,value);
        evaluated_string += $"{groups["pre_op"].Value}({String.Join(",",value)})";
      }
      if (groups["post_op"].Value != "") {
        value = calculate_single(groups["post_op"].Value,value);
        evaluated_string += $"{groups["post_op"].Value}({String.Join(",",value)})";
      }
      if(groups["access"].Value != "") {
        int start = Int32.Parse(groups["access_start"].Value);
        int end = start + 1;
        if(groups["access_range"].Value != "") {
          end = value.Length;
          if(groups["access_end"].Value != "") {
            end = Int32.Parse(groups["access_end"].Value) + 1;
          }
        }
        double[] newValue = new double[end - start];
        for(int i = start, newIndex = 0;i < end;i++,newIndex++) {
          if(i < 0 || i > value.Length) newValue[newIndex] = 0;
          else newValue[newIndex] = value[i];
        }
        value = newValue;
        evaluated_string += $"{groups["access"].Value}";
      }
      if(groups["repeat"].Value != "") {
        int repeatCount = Int32.Parse(groups["repeat"].Value);
        double[] newValue = new double[value.Length * repeatCount];
        for(int i = 0;i < repeatCount;i++) {
          for(int j = 0;j < value.Length;j++) {
            newValue[i * value.Length + j] = value[j];
          }
        }
        value = newValue;
        evaluated_string += $"{{{groups["repeat"].Value}}}";
      }
      #endregion
      #region Operator
      // Handle Operator

      switch(groups["op"].Value){
        case "&&":
        case "AND":
        case "||":
        case "OR":
          opOrder = 0;
        break;

        case "<":
        case ">":
        case "=":
        case "<=":
        case ">=":
        case "!=":
          opOrder = 1;
        break;

        case "+":
        case "-":
          opOrder = 2;
        break;

        case "*":
        case "/":
          opOrder = 3;
        break;
        
        default:
          if(values.Count > 0){
            return new ExpressionResult(false, 
            $"Invalid Syntax\n \"{expr}\"\n" +
            " " + underlineError(match), new double[] { 0 });
          }else
            values.Push(value);
        break;
      }

      if(opOrder > -1) {
        #if (DEBUG)
          debugStack<double[]>(values);
          debugStack<(string,int)>(operators);
        #endif

        while(operators.Count > 0 && operators.Peek().Item2 > opOrder) {
          values.Push(calculate(operators.Pop().Item1,values.Pop(),values.Pop()));
        }
        operators.Push((groups["op"].Value,opOrder));
        values.Push(value);
      }
      #endregion
    }

    // Check to make sure stacks are alligned properly. There should always be one more values than operators
    if(values.Count != operators.Count + 1) {
      return new ExpressionResult(false, $"Invalid Expression\n \"{expr}\"" ,new double[] { 0 });
    }

    // Compute remaining stack elements
    while(operators.Count > 0) {
      #if (DEBUG)
        debugStack<double[]>(values);
        debugStack<(string,int)>(operators);
      #endif

      values.Push(calculate(operators.Pop().Item1,values.Pop(),values.Pop()));
    }

    // The last remaining value is the answer
    double[] total = values.Pop();

    #if (DEBUG)
      Console.WriteLine(total);
    #endif
    
    return new ExpressionResult(true, evaluated_string, total);
  }

  /************************Helper Functions ***********************/

  private static string underlineError(Match match) {
    string output = "";
    for(int i = 0;i <= match.Index;i++) {
      output += " ";
    }
    for(int i = match.Index;i <= match.Index + match.Value.Length;i++) {
      output += "-";
    }
    return output;
  }

  private static double[] calculate(string op, double[] value2, double[] value1) {
    int maxLength = value2.Length;
    if(value1.Length > maxLength)
      maxLength = value1.Length;

    double[] result = new double[] { 0 };

    switch(op) {
        case "+":
        case "-":
          result = new double[maxLength];
          for(int i = 0;i < result.Length;i++) {
            if(i < value1.Length && i < value2.Length){
              if(op == "+") result[i] = value1[i] + value2[i];
              else if (op == "-") result[i] = value1[i] - value2[i];
            }else if (i < value1.Length){
              result[i] = value1[i];
            }else if (i < value2.Length){
              if(op == "+") result[i] = value2[i];
              else if (op == "-") result[i] = - value2[i];
            }else{
              result[i] = 0;
            }
          }
          return result;
        case "*":
        case "/":
          result = new double[value1.Length * value2.Length];
          for(int i = 0;i < value2.Length;i++) {
            for(int j = 0;j < value1.Length;j++) {
              if(op == "*") result[j + i * value1.Length] = value1[j] * value2[i];
              else if (op == "/") result[j + i * value1.Length] = value1[j] / value2[i];
            }
          }
          return result;
        case "<":
        case ">":
        case "=":
        case "<=":
        case ">=":
        case "!=":
        case "&&":
        case "AND":
        case "||":
        case "OR":
          result = new double[value2.Length];
          for(int i = 0;i < value2.Length;i++) {
            double val2 = value2[i];
            result[i] = 0;
            foreach(double val1 in value1) {
              if((op == "<" && val1 < val2) ||
                 (op == ">" && val1 > val2) ||
                 (op == "=" && val1 == val2) ||
                 (op == "!=" && val1 != val2) ||
                 (op == "<=" && val1 <= val2) ||
                 (op == ">=" && val1 >= val2) ||
                 ((op == "&&" || op == "AND") && val1 != 0 && val2 != 0) ||
                 ((op == "||" || op == "OR") && (val1 != 0 || val2 != 0))
              ) { result[i]++; }
            }
          }
          return result;
        default:
          return result;
    }
  }

  private static double[] calculate_single(string op, double[] values) {
    double[] output = new double[] { 0 };
    switch(op) {
      case "-":
      case "+":
        foreach(double value in values) {
          if(op == "+") output[0] += value; else
          if(op == "-") output[0] -= value;
        }
        return output;
      case "=":
      case "!=":
        output[0] = 1;
        for(int i = 1;i < values.Length;i++) {
          bool equal = values[i] == values[i-1];
          if(op == "="  && !equal) output[0] = 0; else
          if(op == "!=") output[0] = 0;
        }
        return output;
      case "!":
      case "NOT":
        output[0] = 1;
        foreach(double value in values) {
          if(value != 0) {
              output[0] = 0;
              break;
          }
        }
        return output;
      case "?":
        output[0] = 0;
        foreach(double value in values) {
          if(value != 0) { 
            output[0] = 1;
            break;
          }
        }
        return output;
    }
    return output;
  }
  private static string[] SplitBalanced(char delimiter, string text) {
    int p_counter = 0;
    int b_counter = 0;
    int c_counter = 0;
    List<string> output = new List<string>();
    string substr = "";
    for(int i = 0;i < text.Length;i++){
      switch(text[i]) {
        case '(': p_counter++;
          break;
        case ')': p_counter--;
          break;
        case '[': b_counter++;
          break;
        case ']': b_counter--;
          break;
        case '{': c_counter++;
          break;
        case '}': c_counter--;
          break;
        default:
          if(text[i] == delimiter) {
            if(p_counter + b_counter + c_counter == 0){
              output.Add(substr);
              substr = "";
              continue;
            }
          }
          break;
      }
      substr += text[i];
    }
    output.Add(substr);
    return output.ToArray();
  }

  #if (DEBUG)
  private static void debugStack<T>(Stack<T> s) {
    if(s.Count <= 0) return;
    T[] values = s.ToArray();
    for(int i = values.Length - 1;i >= 0;i--) {
      if(i != values.Length - 1) 
        Console.Write(",");
      Console.Write(values[i]);
    }
    Console.WriteLine();
  }
  #endif

  
}