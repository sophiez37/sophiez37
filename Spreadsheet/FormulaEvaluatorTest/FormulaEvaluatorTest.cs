using FormulaEvaluator;

namespace FormulaEvaluatorTest
{
    /// <summary>
    /// This class contains tester method for the Evaluate method in the Evaluator class.
    /// </summary>
    internal class EvaluatorTest
    {
        /// <summary>
        /// This main method contains test for the Evaluate method in the Evaluator class.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Addition and subtraction
            int test1 = Evaluator.Evaluate("5 + 1-        2", VariableEvaluator);
            Console.WriteLine("5 + 1-        2" + " = " + test1); // Expect 4
            Console.WriteLine();

            // Multiplication and division
            int test2 = Evaluator.Evaluate("3  *4  / 2", VariableEvaluator);
            Console.WriteLine("3  *4  / 2" + " = " + test2); // Expect 6
            Console.WriteLine();

            // Mix
            int test3 = Evaluator.Evaluate("9 - 3*2 + 5/3", VariableEvaluator);
            Console.WriteLine("9 - 3*2 + 5/3" + " = " + test3); // Expect 4
            Console.WriteLine();

            // Mix with parenthesis
            int test4 = Evaluator.Evaluate("5+12/8*(4+2)", VariableEvaluator);
            Console.WriteLine("5 + 12/6*(4+2)" + " = " + test4); // Expect 11
            Console.WriteLine();

            int test5 = Evaluator.Evaluate("(2) * 15 / (7 - (2 + 1))", VariableEvaluator);
            Console.WriteLine("(2) * 15 / (7 - (2 + 1))" + " = " + test5); // Expect 7
            Console.WriteLine();

            int test6 = Evaluator.Evaluate("((2 + 3) / 4 + 4 / 3 * (5))", VariableEvaluator);
            Console.WriteLine("((2 + 3) / 4 + 4 / 3 * (5))" + " = " + test6); // Expect 6
            Console.WriteLine();

            // With variables
            int test7 = Evaluator.Evaluate("5 + 12 / A1 / (1 + a1)", VariableEvaluator);
            Console.WriteLine("5 + 12 / A1 / (1 + a1)" + " = " + test7); // Expect 11
            Console.WriteLine();

            int test8 = Evaluator.Evaluate(" 1 +    (    abc123 -7)/ AB2 ", VariableEvaluator);
            Console.WriteLine("1 +    (    abc123 -7)/ AB2" + " = " + test8); // Expect -1
            Console.WriteLine();

            int test9 = Evaluator.Evaluate("a1*AB2/abc123 - abc123/A1 + a1", VariableEvaluator);
            Console.WriteLine("a1*AB2/abc123 - abc123/A1 + a1" + " = " + test9); //Expect -2
            Console.WriteLine();

            // Empty string input
            try
            {
                int test10 = Evaluator.Evaluate("", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Empty expression.");
                Console.WriteLine();
            }

            // Missing operators
            try
            {
                int test11 = Evaluator.Evaluate(" 5 8 + 1", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Missing operators.");
                Console.WriteLine();
            }

            // Missing numbers
            try
            {
                int test12 = Evaluator.Evaluate("* - 1 / 5", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Missing numbers.");
                Console.WriteLine();
            }

            // Division by 0
            try
            {
                int test13 = Evaluator.Evaluate("5 + 4 / (2 - 2)", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Division by 0.");
                Console.WriteLine();
            }

            // Missing parenthesis
            try
            {
                int test14 = Evaluator.Evaluate("3 * (1 - 2", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Missing right parenthesis.");
                Console.WriteLine();
            }

            try
            {
                int test15 = Evaluator.Evaluate("3 * 1 - 2)", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Missing left parenthesis.");
                Console.WriteLine();
            }

            // Invalid variables
            try
            {
                int test16 = Evaluator.Evaluate("3 * a1a", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid variable.");
                Console.WriteLine();
            }

            try
            {
                int test17 = Evaluator.Evaluate("3 * a&$", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid variable.");
                Console.WriteLine();
            }

            try
            {
                int test18 = Evaluator.Evaluate("3 * 1a#a1", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid variable.");
                Console.WriteLine();
            }

            // Values of variables not found
            try
            {
                int test19 = Evaluator.Evaluate("2 + b2", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Value of variable not found.");
                Console.WriteLine();
            }

            try
            {
                int test19 = Evaluator.Evaluate("2 + bc23", VariableEvaluator);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Value of variable not found.");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// This method gives the int values of some variables
        /// </summary>
        /// <param name="name">Varible's name</param>
        /// <returns>Variable's int value</returns>
        /// <exception cref="ArgumentException"></exception>
        private static int VariableEvaluator(String name)
        {
            if (name.Equals("a1") || name.Equals("A1"))
                return 1;
            if (name.Equals("AB2"))
                return 2;
            if (name.Equals("abc123"))
                return 3;
            else
                throw new ArgumentException("Variable value not found.");
        }
    }
}