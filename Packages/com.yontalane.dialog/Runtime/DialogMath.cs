using UnityEngine;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Provides static methods to parse, evaluate, and compare mathematical expressions encoded as strings.
    /// Handles basic arithmetic operations and comparisons, optionally incorporating variable storage.
    /// </summary>
    internal static class DialogMath
    {
        #region Structs

        /// <summary>
        /// Represents parsed elements of a mathematical command (left operand, right operand, and operator).
        /// </summary>
        private struct MathData
        {
            public float a;
            public float b;
            public string cmd;
            public bool isVarA;
            public string varNameA;
            public bool isVarB;
            public string varNameB;
        }

        #endregion

        #region Parsing Functions

        /// <summary>
        /// Attempts to get a float value for one side of an equation, either by looking up a stored value or parsing directly.
        /// </summary>
        /// <param name="s">The input string, potentially a variable or numeric literal.</param>
        /// <param name="isVariable">Whether or not the value is a variable.</param>
        /// <param name="variableName">If it's a variable, this is its name.</param>
        /// <param name="value">The resolved float value if successful, otherwise 0.</param>
        /// <returns>True if the value could be resolved, false otherwise.</returns>
        private static bool TryGetValueForEquationSide(string s, out bool isVariable, out string variableName, out float value)
        {
            // Trim extra whitespace
            variableName = s.Trim();

            // Try to get value from DataStorage and parse if present
            if (DataStorage.TryGetValue(variableName, out string storedValue) && float.TryParse(storedValue, out value))
            {
                isVariable = true;
                return true;
            }

            isVariable = false;

            // Try to parse directly as float
            if (float.TryParse(s, out value))
            {
                return true;
            }

            // Could not resolve value
            return false;
        }

        /// <summary>
        /// Attempts to parse a string for a specific mathematical command and extract both operands.
        /// </summary>
        /// <param name="parameter">The string to parse, e.g., "3+5".</param>
        /// <param name="cmd">The mathematical command, e.g., "+", "*".</param>
        /// <param name="mathData">Parsed data with operands and command if successful.</param>
        /// <returns>True if parsing and value resolution is successful, false otherwise.</returns>
        private static bool TryGetMathDataForCmd(string parameter, string cmd, out MathData mathData)
        {
            // Initialize mathData
            mathData = new();

            // Find index of the operator
            int index = parameter.IndexOf(cmd);

            // Validate operator position
            if (index <= 0 || index >= parameter.Length - 1)
            {
                return false;
            }

            // Split into left and right sides by the command/operator
            string[] parts = parameter.Split(cmd);

            // Try to resolve left side value
            if (!TryGetValueForEquationSide(parts[0], out bool leftIsVariable, out string leftVariableName, out float leftSide))
            {
                return false;
            }

            // Try to resolve right side value
            if (!TryGetValueForEquationSide(parts[1], out bool rightIsVariable, out string rightVariableName, out float rightSide))
            {
                return false;
            }

            // Fill the mathData struct with parsed data
            mathData.cmd = cmd;
            mathData.a = leftSide;
            mathData.b = rightSide;
            mathData.isVarA = leftIsVariable;
            mathData.varNameA = leftVariableName;
            mathData.isVarB = rightIsVariable;
            mathData.varNameB = rightVariableName;

            return true;
        }

        /// <summary>
        /// Attempts to parse the input string with multiple mathematical commands, returning the first successful match.
        /// </summary>
        /// <param name="parameter">The string to parse.</param>
        /// <param name="mathData">Outputs the matched operands and operator if successful.</param>
        /// <param name="cmds">A variable list of supported operators to check.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private static bool TryGetMathData(string parameter, out MathData mathData, params string[] cmds)
        {
            // Trim whitespace on input string
            parameter = parameter.Trim();

            // Loop over candidate commands/operators
            foreach (string cmd in cmds)
            {
                // Return true on first operator match and successful parsing
                if (TryGetMathDataForCmd(parameter, cmd, out mathData))
                {
                    return true;
                }
            }

            // No matching operator or parsing failed
            mathData = default;
            return false;
        }

        #endregion

        #region Arithmetic Execution

        /// <summary>
        /// Attempts to execute a compound assignment operation (e.g., "+=", "-=", "*=", etc.) provided as a string.
        /// </summary>
        /// <param name="parameter">Compound assignment string (e.g., "a += 5")</param>
        /// <param name="roundToInt">If true, rounds operands to integers before calculation and rounds the final result.</param>
        /// <param name="result">The computed float result, if execution was successful.</param>
        /// <returns>True if a supported operation was successfully executed; otherwise, false.</returns>
        private static bool TryExecuteCompoundMath(this string parameter, bool roundToInt, out float result)
        {
            // Try to parse the string as a compound operation and extract operands and operator.
            if (!TryGetMathData(parameter, out MathData mathData, "+=", "-=", "/=", "*=", "%=", "^="))
            {
                result = default;
                return false;
            }

            // Ensure the left operand is a variable (i.e., can be assigned to).
            if (!mathData.isVarA)
            {
                result = default;
                return false;
            }

            // Optionally round operands if required.
            if (roundToInt)
            {
                mathData.a = Mathf.Round(mathData.a);
                mathData.b = Mathf.Round(mathData.b);
            }

            // Execute the corresponding operation based on the parsed operator.
            switch (mathData.cmd)
            {
                case "+=":
                    result = mathData.a + mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(mathData.varNameA, result.ToString());
                    return true;
                case "-=":
                    result = mathData.a - mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(mathData.varNameA, result.ToString());
                    return true;
                case "/=":
                    result = mathData.a / mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(mathData.varNameA, result.ToString());
                    return true;
                case "*=":
                    result = mathData.a * mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(mathData.varNameA, result.ToString());
                    return true;
                case "%=":
                    result = mathData.a % mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(mathData.varNameA, result.ToString());
                    return true;
                case "^=":
                    result = Mathf.Pow(mathData.a, mathData.b);
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(mathData.varNameA, result.ToString());
                    return true;
            }

            // If no valid operation was matched, fail.
            result = default;
            return false;
        }

        /// <summary>
        /// Attempts to execute a standard assignment with a mathematical operation (e.g., "a = b + c") provided as a string.
        /// </summary>
        /// <param name="parameter">Assignment string (e.g., "a = 5 + 2")</param>
        /// <param name="roundToInt">If true, rounds operands to integers before calculation and rounds the final result.</param>
        /// <param name="result">The computed float result, if execution was successful.</param>
        /// <returns>True if a valid operation was successfully executed; otherwise, false.</returns>
        private static bool TryExecuteTargetMath(this string parameter, bool roundToInt, out float result)
        {
            // Split parameter string on first '=' to separate target and expression.
            string[] parts = parameter.Split('=');

            // Parameter must be of the form "target = expression"
            if (parts.Length != 2)
            {
                result = default;
                return false;
            }

            // Trim and validate the target variable.
            string target = parts[0].Trim();
            if (string.IsNullOrEmpty(target))
            {
                result = default;
                return false;
            }

            // Try to parse the right-side expression for supported operations.
            if (!TryGetMathData(parts[1].Trim(), out MathData mathData, "+", "-", "/", "*", "%", "^"))
            {
                result = default;
                return false;
            }

            // Ensure the target variable exists in storage.
            if (!DataStorage.ContainsKey(target))
            {
                DataStorage.Add(target, string.Empty);
            }

            // Optionally round operands if required.
            if (roundToInt)
            {
                mathData.a = Mathf.Round(mathData.a);
                mathData.b = Mathf.Round(mathData.b);
            }

            // Execute the corresponding operation and store result to the target variable.
            switch (mathData.cmd)
            {
                case "+":
                    result = mathData.a + mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(target, result.ToString());
                    return true;
                case "-":
                    result = mathData.a - mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(target, result.ToString());
                    return true;
                case "/":
                    result = mathData.a / mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(target, result.ToString());
                    return true;
                case "*":
                    result = mathData.a * mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(target, result.ToString());
                    return true;
                case "%":
                    result = mathData.a % mathData.b;
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(target, result.ToString());
                    return true;
                case "^":
                    result = Mathf.Pow(mathData.a, mathData.b);
                    if (roundToInt)
                    {
                        result = Mathf.Round(result);
                    }
                    DataStorage.SetValue(target, result.ToString());
                    return true;
            }

            // No valid operation; fail.
            result = default;
            return false;
        }

        /// <summary>
        /// Tries to execute either a compound or a target math assignment, returning true on first success.
        /// </summary>
        /// <param name="parameter">Math expression or assignment string.</param>
        /// <param name="roundToInt">If true, rounds operands to integers before calculation and rounds the final result.</param>
        /// <param name="result">The computed float result, if execution was successful.</param>
        /// <returns>True if any mathematical operation was performed successfully, otherwise false.</returns>
        private static bool TryExecuteMath(this string parameter, bool roundToInt, out float result)
        {
            // Try compound assignment math (e.g., "a += b")
            if (TryExecuteCompoundMath(parameter, roundToInt, out result))
            {
                return true;
            }

            // Try target assignment math (e.g., "a = b + c")
            if (TryExecuteTargetMath(parameter, roundToInt, out result))
            {
                return true;
            }

            // Both attempts failed.
            return false;
        }

        /// <summary>
        /// Executes a mathematical operation specified as a string and returns a float result.
        /// </summary>
        /// <param name="parameter">The equation string.</param>
        /// <param name="result">The computed float result if successful.</param>
        /// <returns>True if computation was successful, false otherwise.</returns>
        public static bool TryExecuteMathF(this string parameter, out float result)
        {
            // Try to execute as float (no integer rounding)
            return parameter.TryExecuteMath(false, out result);
        }

        /// <summary>
        /// Executes a mathematical operation specified as a string and returns a rounded integer result.
        /// </summary>
        /// <param name="parameter">The equation string.</param>
        /// <param name="result">The computed integer result if successful.</param>
        /// <returns>True if computation was successful, false otherwise.</returns>
        public static bool TryExecuteMathI(this string parameter, out int result)
        {
            // Try to execute as integer (operands and result rounded)
            bool success = parameter.TryExecuteMath(true, out float floatResult);
            result = Mathf.RoundToInt(floatResult);
            return success;
        }

        #endregion

        #region Comparison Functions

        /// <summary>
        /// Compares two sides of an equation with various comparison operators encoded as a string. Optionally rounds operands.
        /// </summary>
        /// <param name="parameter">The comparison string, e.g., "5 >= 2".</param>
        /// <param name="roundToInt">If true, operands are rounded to integers before comparison.</param>
        /// <param name="result">The boolean result of the comparison if successful.</param>
        /// <returns>True if parsing and comparison was successful, false otherwise.</returns>
        private static bool TryCompareMath(this string parameter, bool roundToInt, out bool result)
        {
            // Parse the equation for operands and operator, limited to comparison operators
            if (!TryGetMathData(parameter, out MathData mathData, "==", "<=", ">=", "!=", "<>", "=", "<", ">"))
            {
                result = default;
                return false;
            }

            // Optionally round operands to integer
            if (roundToInt)
            {
                mathData.a = Mathf.Round(mathData.a);
                mathData.b = Mathf.Round(mathData.b);
            }

            // Execute the comparison based on the operator
            switch (mathData.cmd)
            {
                case "=":
                case "==":
                    result = Mathf.Approximately(mathData.a, mathData.b);
                    return true;
                case "!=":
                case "<>":
                    result = !Mathf.Approximately(mathData.a, mathData.b);
                    return true;
                case "<=":
                    result = mathData.a <= mathData.b;
                    return true;
                case ">=":
                    result = mathData.a >= mathData.b;
                    return true;
                case "<":
                    result = mathData.a < mathData.b;
                    return true;
                case ">":
                    result = mathData.a > mathData.b;
                    return true;
            }

            // Operator not matched or supported
            result = default;
            return false;
        }

        /// <summary>
        /// Compares two sides of an equation with comparison operators, using floating point comparison.
        /// </summary>
        /// <param name="parameter">The comparison string.</param>
        /// <param name="result">Boolean result of comparison if successful.</param>
        /// <returns>True if the comparison was successful, false otherwise.</returns>
        public static bool TryCompareMathF(this string parameter, out bool result)
        {
            // Try to compare as float (no integer rounding)
            return parameter.TryCompareMath(false, out result);
        }

        /// <summary>
        /// Compares two sides of an equation with comparison operators, using rounded integer comparison.
        /// </summary>
        /// <param name="parameter">The comparison string.</param>
        /// <param name="result">Boolean result of comparison if successful.</param>
        /// <returns>True if the comparison was successful, false otherwise.</returns>
        public static bool TryCompareMathI(this string parameter, out bool result)
        {
            // Try to compare as integers (operands rounded)
            return parameter.TryCompareMath(true, out result);
        }

        #endregion
    }
}
