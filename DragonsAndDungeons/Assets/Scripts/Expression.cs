//#define DEBUG
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

struct ExpressionResult {
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

class Expression {
  public string Expr { get; private set; }

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
    Expr = expr.ToUpperInvariant();
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
    return ExpressionCalculator.Evaluate(initialValue, Expr, target, Vars);
  }
}

static class ExpressionCalculator {

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
  static string inside_parenthesis = @"(?:(?<p>\()|[^()]|(?<-p>\)))*";
  static string inside_braces = @"(?:(?<c>{)|[^{}]|(?<-c>}))*";
  static string inside_brackets = @"(?:(?<b>\[)|[^\[\]]|(?<-b>\]))+";

  static string op = @"(?<op>(?:[><!]=)|[+\-*/><=]|OR|\|\||AND|&&)";
  static string parenthesis = $@"(?:(?<paren_mul_l>-?\d*)\((?<paren>{inside_parenthesis})\)(?<paren_mul_r>\d*))";
  static string dice = @"(?<dice>(?<d_count>\d+)D(?<d_sides>\d+)(?:r\[(?<reroll_nums>\d+(?:,\d+)*)\])?(?<d_adv>[a,d])?)";
  static string variable = @"(?<var>[A-Z_]\w*)";
  static string constant = $@"(?<const>-?\d+|\[(?<const_list>{inside_brackets})\])";
  static string pre_op = @"(?<pre_op>\?|!|NOT)";
  static string named_value = $@"(?<named_value>(?<name>[A-Z_]\w*){{(?<named_inside>{inside_braces})}})";
  static string list_op = $@"(?<list_op>[A-Z_]\w*)\[(?<list_inside>{inside_brackets})\]";
  static string function = $@"(?<func>[A-Z_]\w*)\((?<func_args>{inside_parenthesis})\)";

  static string value = $@"(?:{named_value}|{list_op}|{function}|{parenthesis}|{dice}|{variable}|{constant})";

  static string rx_expr = $@"\s*{op}?\s*{pre_op}?{value}?\s*";

  static Regex rx = new Regex(rx_expr,RegexOptions.Compiled);

/********************************************************************/
  
  public static ExpressionResult Evaluate(string expr, Character charRef = null) {
    return Evaluate(0,expr,charRef,new VariableCollection());
  }
  public static ExpressionResult Evaluate(string expr, VariableCollection variables) {
    return Evaluate(0,expr,null,variables);
  }
  public static ExpressionResult Evaluate(string expr, Character charRef, VariableCollection variables) {
    return Evaluate(0,expr,charRef,variables);
  }

  public static ExpressionResult Evaluate(int initialValue,string expr, Character charRef = null) {
    return Evaluate(initialValue, expr, charRef,new VariableCollection());
  }
  public static ExpressionResult Evaluate(int initialValue,string expr, VariableCollection variables) {
    return Evaluate(initialValue, expr,null,variables);
  }
  public static ExpressionResult Evaluate(double initialValue, string expr, Character charRef, VariableCollection variables) {
    expr = expr.ToUpperInvariant();
    MatchCollection matches = rx.Matches(expr);
    Stack<double[]> values = new Stack<double[]>();
    // Stack of tuples of the form ([operator],[order of operation])
    // where higher orders of operation are evaluated first.
    Stack<(string,int)> operators = new Stack<(string,int)>();

    // String used to keep track of what is being evaluated vs. ignored.
    // Return this with the result for useful feedback
    string evaluated_string = "";

    variables["ANS"] = new double[] { initialValue };

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
      if(groups["dice"].Value != "") {
        evaluated_string += groups["dice"].Value;

        int d_count = Int32.Parse(groups["d_count"].Value);
        int d_sides = Int32.Parse(groups["d_sides"].Value);
        AdvantageType adv = AdvantageType.None;
        if(groups["d_adv"].Value != "") {
          if(groups["d_adv"].Value == "a") adv = AdvantageType.Advantage;
          else if (groups["d_adv"].Value == "d") adv = AdvantageType.Disadvantage;
        }
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
        evaluated_string += String.Format(" ([{0}])", String.Join(",",results));
      }

      else if(groups["const"].Value != "") {
        if(groups["const_list"].Value != "") {
          string[] args = groups["const_list"].Value.Split(',');
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
            default:
              return new ExpressionResult(false, $"Invalid variable name \"{id}\"",new double[] { 1 });
          }
        }
        evaluated_string += String.Format("{0} ({1})",id,String.Join(",",value));
      }
      
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

      else if (groups["named_value"].Value != ""){
        string key = groups["name"].Value;
        ExpressionResult result = Evaluate(groups["named_inside"].Value, charRef, variables);
        if(!result.Success)
          return result;
        value = result.Values;
        evaluated_string += $"{key}{{{result.Message}}}";
        variables[key] = value;
      }

      else if (groups["list_op"].Value != "") {
        string opName = groups["list_op"].Value;
        Func<double, double, bool> compare;
        switch(opName) {
          case "MAX":
            compare = delegate (double a, double b) { return a > b; };
          break;
          case "MIN":
            compare = delegate (double a, double b) { return a < b; };
          break;
          default:
            return new ExpressionResult(false, $"{opName} is not a valid list operation",new double[] { 2 });
        }

        evaluated_string += $"{opName}[";
        string[] exprs = groups["list_inside"].Value.Split(',');
        ExpressionResult result = Evaluate(exprs[0],charRef,variables);
        if(!result.Success)
          return result;
        
        evaluated_string += result.Message;
        double compValue = result.Values[0];
        foreach(double val in result.Values) {
          if(compare(val,compValue)) {
            compValue = val;
          }
        }

        for(int i = 1;i < exprs.Length;i++) {
          result = Evaluate(exprs[i],charRef,variables);
          if(!result.Success)
            return result;
          evaluated_string += ", " + result.Message;
          foreach(double val in result.Values) {
            if(compare(val,compValue)) {
              compValue = val;
            }
          }
          
        }
        value = new double[] { compValue };
        evaluated_string += $"] ({String.Join(",",value)})";
      }

      else if (groups["func"].Value != "") {
        string funcName = groups["func"].Value;

        string[] funcArgs = groups["func_args"].Value.Split(',');
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
            return new ExpressionResult(false, $"{funcName} with {funcArgs.Length} arguments is not a valid function",new double[] { 3 });
          }
        }
        evaluated_string += $"{funcName}({String.Join(",",funcArgs)}) ({value[0]})";
      }
      
      else {
        return new ExpressionResult(false, 
        $"Invalid Operator\n \"{expr}\"\n"+
        " " + underlineError(match), new double[] { 4 });
      }

      // Handle preoperator
      if (groups["pre_op"].Value != "") {
        value = calculate(groups["pre_op"].Value,value,new double[] { 0 });
        evaluated_string += $"({String.Join(",",value)})";
      } 

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
            " " + underlineError(match), new double[] { 5 });
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
    }

    // Check to make sure stacks are alligned properly. There should always be one more values than operators
    if(values.Count != operators.Count + 1) {
      return new ExpressionResult(false, $"Invalid Expression\n \"{expr}\"" ,new double[] { 6 });
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
    
    double sum1 = 0;
    double sum2 = 0;

    for(int i = 0;i < maxLength;i++) {
      if(i < value1.Length) {
        sum1 += value1[i];
      }
      if (i < value2.Length){
        sum2 += value2[i];
      }
    }

    double[] _true = new double[] { 1 };
    double[] _false = new double[] { 0 };
    double[] result;

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
              result[i] = value2[i];
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
          if (sum1 < sum2) return _true;
          else return _false;
        case ">":
          if(sum1 > sum2) return _true;
          else return _false;
        case "=":
          if(sum1 == sum2) return _true;
          else return _false;
        case "<=":
          if(sum1 <= sum2) return _true;
          else return _false;
        case ">=":
          if(sum1 >= sum2) return _true;
          else return _false;
        case "!=":
          if(sum1 != sum2) return _true;
          else return _false;
        case "!":
        case "NOT":
          if(sum2 == 0) return _true;
          else return _false;
        case "?":
          if(sum2 != 0) return _true;
          else return _false;
        case "&&":
        case "AND":
          if(sum2 != 0 && sum1 != 0) return _true;
          else return _false;
        case "||":
        case "OR":
          if(sum2 != 0 || sum1 != 0) return _true;
          else return _false;
        default:
          return _false;
    }
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