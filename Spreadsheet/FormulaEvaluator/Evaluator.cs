using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// This class contains a method that evaluates integer arithmetic expressions written using standard infix notation
    /// and its helper methods.
    /// </summary>
    public static class Evaluator
    {
        // Two stacks to keep track of the values and operators in the expression
        private static Stack<int> values;
        private static Stack<string> operators;

        /// <summary>
        /// Delegate that takes in the name of the variable and return its int value.
        /// </summary>
        /// <param name="v">Variable name</param>
        /// <returns>Int value of variable</returns>
        public delegate int Lookup(string v);

        /// <summary>
        /// This method evaluates integer arithmetic expressions written using standard infix notation.
        /// </summary>
        /// <param name="exp">The arithmetic expression to be evaluated</param>
        /// <param name="variableEvaluator">A delegate that returns an int value of a variable</param>
        /// <returns>The result value of the expression</returns>
        /// <exception cref="ArgumentException"></exception>
        public static int Evaluate(string exp, Lookup variableEvaluator)
        {
            values = new Stack<int>();
            operators = new Stack<string>();

            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            foreach (string s in substrings)
            {
                // Ignore whitespace
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                string token = s.Trim();

                // If the token is an integer
                if (int.TryParse(token, out int intValue))
                {
                    if (IsOnTop(operators, "*"))
                        values.Push(Operate(values.Pop(), intValue, operators.Pop()));

                    else if (IsOnTop(operators, "/"))
                    {
                        if (intValue == 0)
                            throw new ArgumentException("Division by 0.");
                        else
                            values.Push(Operate(values.Pop(), intValue, operators.Pop()));
                    }

                    else
                        values.Push(intValue);
                }

                // If the token is operator "+" or "-"
                else if (token.Equals("+") || token.Equals("-"))
                {
                    if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                        PushToValues();
                    operators.Push(token);
                }

                // If the token is operator "*" or "/" or "("
                else if (token.Equals("*") || token.Equals("/") || token.Equals("("))
                    operators.Push(token);

                // If the token is operator ")"
                else if (token.Equals(")"))
                {
                    if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                        PushToValues();

                    if (IsOnTop(operators,"("))
                        operators.Pop();

                    else
                        throw new ArgumentException("Parenthesis not found.");

                    if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                        PushToValues();
                }

                // If the token is a variable
                else if (IsVariable(token))
                {
                    int var = variableEvaluator(token);

                    if (IsOnTop(operators, "*"))
                        values.Push(Operate(values.Pop(), var, operators.Pop()));

                    else if (IsOnTop(operators, "/"))
                    {
                        if (var == 0)
                            throw new ArgumentException("Division by 0.");
                        else
                            values.Push(Operate(values.Pop(), var, operators.Pop()));
                    }

                    else
                        values.Push(var);
                }

                // If the token is non of the above then throw exception
                else
                    throw new ArgumentException("Illegal tokens.");
            }

            // When there is no operator left on the operator stack
            if (operators.Count == 0)
            {
                if (values.Count == 1)
                    return values.Pop();
                else
                    throw new ArgumentException("There isn't exactly 1 value on the value stack.");
            }

            // When there is exactly one operator left on the operator stack and two values left on the value stack
            else if (operators.Count == 1 && (operators.Peek().Equals("+") || operators.Peek().Equals("-")))
            {
                if (values.Count == 2)
                {
                    int i1 = values.Pop();
                    int i2 = values.Pop();
                    return Operate(i2, i1, operators.Pop());
                }
                else
                    throw new ArgumentException("There isn't exactly 2 values on the value stack.");
            }

            // Throw exception if it's non of the cases above
            else
                throw new ArgumentException("There isn't exactly 1 operator on the operator stack.");

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
        /// <exception cref="ArgumentException"></exception>
        private static int Operate(int i1, int i2, string s)
        {
            if (s.Equals("+"))
                return i1 + i2;
            if (s.Equals("-"))
                return i1 - i2;
            if (s.Equals("*"))
                return i1 * i2;
            if (s.Equals("/"))
                return i1 / i2;
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// This method pops the value stack twice and the operator stack once,
        /// then applies the popped operator to the popped numbers and pushes the result to the value stack.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        private static void PushToValues()
        {
            if (values.Count < 2)
                throw new ArgumentException();
            else
            {
                int i1 = values.Pop();
                int i2 = values.Pop();
                values.Push(Operate(i2, i1, operators.Pop()));
            }
        }

        /// <summary>
        /// This method checks if a string contains a variable name.
        /// </summary>
        /// <param name="token">The string which represents a token</param>
        /// <returns></returns>
        private static bool IsVariable(string token)
        {
            char[] charArr = token.ToCharArray();

            // If the first character of the string is not a letter then return false
            if (!Char.IsLetter(charArr[0]))
                return false;

            // If the last character of the string is not a digit then return false
            if (!Char.IsDigit(charArr[charArr.Length - 1]))
                return false;

            for (int i = 0; i < charArr.Length; i++)
            {
                // If any of the characters in the string is not a letter or a digit then return false
                if (!Char.IsLetter(charArr[i]) && !Char.IsDigit(charArr[i]))
                    return false;

                else if (Char.IsLetter(charArr[i]))
                    continue;

                else // If the character is a digit
                    // Check every characters after it to ensure that all of them are also digits
                    for (int j = i + 1; j < charArr.Length; j++)
                        if (!Char.IsDigit(charArr[j]))
                            return false;
            }
            return true;
        }
    }
}