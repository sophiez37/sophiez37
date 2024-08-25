// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)

// Modified by: Phuong Anh Nguyen
// Date: Sep 14 2023
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private string formula;
    private HashSet<string> variables; // Set of varables in the formula

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        variables = new HashSet<string>();
        List<string> tokenList = GetTokens(formula).ToList();

        // Check One Token Rule
        if (tokenList.Count < 1)
            throw new FormulaFormatException("There must be at least one token in the formula. Add tokens into your formula.");

        // Check Starting Token Rule and Ending Token Rule
        string firstToken = tokenList.First();
        string lastToken = tokenList.Last();
        if (!Double.TryParse(firstToken, out double firstVal) && !IsVariable(firstToken) && !firstToken.Equals("("))
            throw new FormulaFormatException("The first token this an expression is not a number, a variable, or an opening parenthesis. Rewrite your formula.");
        if (!Double.TryParse(lastToken, out double lastVal) && !IsVariable(lastToken) && !lastToken.Equals(")"))
            throw new FormulaFormatException("The last token of this expression is not a number, a variable, or a closing parenthesis. Rewrite your formula.");

        int openParenCount = 0;
        int closeParenCount = 0;
        String validFormula = ""; // Initialize variable that holds the valid formula
        for (int i = 0; i < tokenList.Count; i++)
        {
            String token = tokenList[i];

            if (token.Equals("("))
            {
                openParenCount++;
                FollowingRuleCheck(tokenList, i);
            }

            else if (token.Equals(")"))
            {
                closeParenCount++;
                ExtraFollowingRuleCheck(tokenList, i);
            }

            else if (token.Equals("+") || token.Equals("-") || token.Equals("*") || token.Equals("/"))
            {
                FollowingRuleCheck(tokenList, i);
            }

            else if (Double.TryParse(token, out double val))
            {
                token = "" + val; // "Normalize" the double value and store it back into token
                ExtraFollowingRuleCheck(tokenList, i);
            }

            else if (IsVariable(token))
            {
                if (IsValidVariable(token, normalize, isValid))
                {
                    token = normalize(token); // Normalize the variable and store it back into token
                    variables.Add(token); // Add the variable into our variable set
                }
                else
                    throw new FormulaFormatException("Invalid variable found. Reformat your variable.");
                ExtraFollowingRuleCheck(tokenList, i);
            }

            // Check Right Parentheses Rule
            if (closeParenCount > openParenCount)
                throw new FormulaFormatException("The number of closing parentheses seen so far is greater than the number of opening parentheses seen so far. Deleted excess closing parentheses.");

            validFormula += token; // Update valid formula
        }

        // Check Balanced Parentheses Rule
        if (openParenCount != closeParenCount)
            throw new FormulaFormatException("The total number of opening parentheses is not equal the total number of closing parentheses. Delete excess parentheses.");

        // If all conditions met the initialize formula
        this.formula = validFormula;
    }

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        Stack<double> values = new Stack<double>(); // Stack containing values
        Stack<string> operators = new Stack<string>(); // Stack containin operators
        string[] substrings = Regex.Split(formula, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

        foreach (string token in substrings)
        {
            // If the token is a double
            if (double.TryParse(token, out double doubleValue))
            {
                if (IsOnTop(operators, "*"))
                    values.Push(Operate(values.Pop(), doubleValue, operators.Pop()));

                else if (IsOnTop(operators, "/"))
                {
                    if (doubleValue == 0)
                        return new FormulaError("Division by 0.");
                    else
                        values.Push(Operate(values.Pop(), doubleValue, operators.Pop()));
                }

                else
                    values.Push(doubleValue);
            }

            // If the token is operator "+" or "-"
            else if (token.Equals("+") || token.Equals("-"))
            {
                if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                    PushToValues(values, operators);
                operators.Push(token);
            }

            // If the token is operator "*" or "/" or "("
            else if (token.Equals("*") || token.Equals("/") || token.Equals("("))
                operators.Push(token);

            // If the token is operator ")"
            else if (token.Equals(")"))
            {
                if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                    PushToValues(values, operators);

                if (IsOnTop(operators, "("))
                    operators.Pop();

                if (IsOnTop(operators, "*"))
                    PushToValues(values, operators);

                if (IsOnTop(operators, "/"))
                {
                    if (values.Peek() == 0)
                        return new FormulaError("Division by 0.");
                    else
                        PushToValues(values, operators);
                }
            }

            // If the token is a variable
            else if (IsVariable(token))
            {
                double var;
                try { var = lookup(token); }
                catch (ArgumentException) { return new FormulaError("Undefined variable."); }

                if (IsOnTop(operators, "*"))
                    values.Push(Operate(values.Pop(), var, operators.Pop()));

                else if (IsOnTop(operators, "/"))
                {
                    if (var == 0)
                        return new FormulaError("Division by 0.");
                    else
                        values.Push(Operate(values.Pop(), var, operators.Pop()));
                }

                else
                    values.Push(var);
            }
        }

        // When there is no operators left on the operator stack and one value left on the value stack
        if (operators.Count == 0)
            return values.Pop();

        // When there is one operator left on the operator stack and two values left on the value stack
        else
        {
            double i1 = values.Pop();
            double i2 = values.Pop();

            return Operate(i2, i1, operators.Pop());
        }
    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {
        return variables;
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        return formula;
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is Formula))
            return false;
        else
            return this.formula.Equals(((Formula)obj).formula);
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.formula.Equals(f2.formula);
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !f1.formula.Equals(f2.formula);
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        return formula.GetHashCode();
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
    {
        // Patterns for individual tokens
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }
    }

    /// <summary>
    /// Checks if a string contains a legal variable or not
    /// </summary>
    /// <param name="token">String that needs to be checked</param>
    /// <returns>True if the string contains a legal variable, false otherwise</returns>
    private static bool IsVariable(string token)
    {
        string pattern = @"^[a-zA-Z_][a-zA-Z_0-9]*$";
        bool legal = Regex.IsMatch(token, pattern);
        return legal;
    }

    /// <summary>
    /// Checks if a string contains a valid variable or not
    /// </summary>
    /// <param name="token">String that needs to be checked</param>
    /// <param name="normalize">Delegate that normalizes the string</param>
    /// <param name="isValid">Delegate that checks if the variable format is valid</param>
    /// <returns>True if the string contains a valid variable, false otherwise</returns>
    private static bool IsValidVariable(string token, Func<string, string> normalize, Func<string, bool> isValid)
    {
        return isValid(normalize(token));
    }

    /// <summary>
    /// Checks parenthesis/operator following rule, which states: Any token that immediately follows
    /// an opening parenthesis or an operator must be either a number, a variable, or an opening parenthesis.
    /// </summary>
    /// <param name="tokenList">List of tokens to check</param>
    /// <param name="i">Index of the token that needs to be checked in the list</param>
    /// <exception cref="FormulaFormatException">Throw if the the next token of this token does not follow the rule</exception>
    private static void FollowingRuleCheck(List<string> tokenList, int i)
    {
        if (i <= tokenList.Count - 2)
        {
            String nextToken = tokenList[i + 1];
            if (!Double.TryParse(nextToken, out double nextVal) && !nextToken.Equals("(") && !IsVariable(nextToken))
                throw new FormulaFormatException("Any token that immediately follows an opening parenthesis or an operator must be either a number, a variable, or an opening parenthesis. Rewrite your formula.");
        }
    }

    /// <summary>
    /// Checks extra following rule, which states: Any token that immediately follows a number,
    /// a variable, or a closing parenthesis must be either an operator or a closing parenthesis.
    /// </summary>
    /// <param name="tokenList">List of tokens to check</param>
    /// <param name="i">Index of the token that needs to be checked in the list</param>
    /// <exception cref="FormulaFormatException">Throw if the the next token of this token does not follow the rule</exception>
    private static void ExtraFollowingRuleCheck(List<string> tokenList, int i)
    {
        if (i <= tokenList.Count - 2)
        {
            String nextToken = tokenList[i + 1];
            if (!nextToken.Equals("+") && !nextToken.Equals("-") && !nextToken.Equals("*") && !nextToken.Equals("/") && !nextToken.Equals(")"))
                throw new FormulaFormatException("Any token that immediately follows a number, a variable, or a closing parenthesis must be either an operator or a closing parenthesis. Rewrite your formula.");
        }
    }

    /// <summary>
    /// This method checks if an element is on top of the specified stack or not.
    /// </summary>
    /// <param name="stack">The stack under consideration</param>
    /// <param name="s">The element to check</param>
    /// <returns>True if the element is on top of the stack, false otherwise</returns>
    private static bool IsOnTop(Stack<string> stack, string s)
    {
        return stack.Count > 0 && stack.Peek().Equals(s);
    }

    /// <summary>
    /// This method applies the operation that is carried out by the operator in the string to the two integers.
    /// </summary>
    /// <param name="i1">The first integer of the operation</param>
    /// <param name="i2">The second integer of the operation</param>
    /// <param name="s">The string that contains the operator "+", "-", "*", or "/"</param>
    /// <returns>The int result of the operation</returns>
    private static double Operate(double i1, double i2, string s)
    {
        if (s.Equals("+"))
            return i1 + i2;
        else if (s.Equals("-"))
            return i1 - i2;
        else if (s.Equals("*"))
            return i1 * i2;
        else // s.Equals("/")
            return i1 / i2;
    }

    /// <summary>
    /// This method pops the value stack twice and the operator stack once,
    /// then applies the popped operator to the popped numbers and pushes the result to the value stack.
    /// </summary>
    /// <param name="values">The value stack</param>
    /// <param name="operators">The operator stack</param>
    private static void PushToValues(Stack<double> values, Stack<string> operators)
    {
        double i1 = values.Pop();
        double i2 = values.Pop();
        values.Push(Operate(i2, i1, operators.Pop()));
    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

