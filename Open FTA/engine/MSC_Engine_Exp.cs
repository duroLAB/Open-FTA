using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

    internal class MCSEngine_Exp
    {
        // Processes a given boolean expression (string), parses it into an internal representation(split string into terms, which are readable and sorted in tree),
        // simplifies it, and returns the resulting string.
        public static string ProcessExpression(string expression)
        {

            Debug.WriteLine($"[ProcessExpression] Input: {expression}");

            // 1) Create a parser with input "expression".
            var parser = new BooleanExpressionParser(expression);

            // 2) Parse the expression into a BooleanExpression object.
            BooleanExpression expr = parser.ParseExpression();
            Debug.WriteLine($"[ProcessExpression] Parsed expression: {expr}");

            // 3) Simplify the parsed expression (including distribution).
            BooleanExpression simplified = expr.Simplify();
            Debug.WriteLine($"[ProcessExpression] Simplified expression: {simplified}");

            // 4) Return the final string.
            return simplified.ToString();
        }

        // Abstract base class for all BooleanExpressions (literal, AND, OR).
        public abstract class BooleanExpression
        {
            // Simplifies the current expression based on boolean algebra rules.
            public abstract BooleanExpression Simplify();

            // Returns a string representation of this expression ( "A", "A+B", etc.).
            public abstract override string ToString();
        }
        // Represents a single literal ( "A", "BE1", etc.).
        public class LiteralExpression : BooleanExpression
        {
            public string Value;

            public LiteralExpression(string value)
            {
                Value = value;
            }
            public override BooleanExpression Simplify() { return this; }
            public override string ToString() { return Value; }

            // Override equality checks so that two LiteralExpressions with the same text are considered equal.
            public override bool Equals(object obj)
            {
                if (obj is LiteralExpression other)
                {
                    return string.Equals(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }

            public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();
        }

        // Represents a boolean AND operation with multiple factors (A * B * C etc.).
        public class AndExpression : BooleanExpression
        {
            public List<BooleanExpression> Factors;

            public AndExpression(IEnumerable<BooleanExpression> factors)
            {
                Factors = new List<BooleanExpression>(factors);
            }

            // Simplifies the AND expression by:
            // 1) Recursively simplifying each factor,
            // 2) Flattening nested ANDs,
            // 3) Removing duplicate factors,
            // 4) If there's only one factor left, return it directly,
            // 5) Otherwise, return a new AndExpression with the cleaned-up list,and then applying distribution over OR.
            public override BooleanExpression Simplify()
            {
                // 1) Recursively simplify each factor. Algorithm iterates over every factor (or subexpression) that is part of the AND expression and calls the Simplify() method on each one.
                List<BooleanExpression> newFactors = new List<BooleanExpression>();
                foreach (var factor in Factors)
                {
                    var simp = factor.Simplify();
                    // 2) Flatten nested AND expressions. Flattening is important to eliminate unnecessary grouping. ( A*(BC)--->ABC, etc.)
                    if (simp is AndExpression andExp)
                        newFactors.AddRange(andExp.Factors);
                    else
                        newFactors.Add(simp);
                }

                // 3) Remove duplicate factors. (A*A----->A , etc.)
                newFactors = newFactors.Distinct().ToList();

                // 4) If there's only one factor, return it as single factor.
                if (newFactors.Count == 1)
                    return newFactors[0];

                // Sort for consistency.
                newFactors.Sort((a, b) => a.ToString().CompareTo(b.ToString()));

                // Create a new AndExpression from the simplified factors.
                AndExpression simplified = new AndExpression(newFactors);

                // 5) Apply distribution over OR (once only). If more than one unique factor remains, the method constructs a new AndExpression from this cleaned-up list of factors. After that, it applies the distribution step.
                BooleanExpression distributed = simplified.Distribute();

                return distributed;
            }

            // Joins all factors with the '*' operator (e.g. "A*B*C").
            // If a factor is an OrExpression, wraps it in parentheses to preserve order of operations.
            public override string ToString() =>
                string.Join("*", Factors.Select(f => f is OrExpression ? "(" + f.ToString() + ")" : f.ToString()));

            // Distributes AND over OR.
            // This method returns a distributed (expanded) expression.
            // It only applies distribution if it results in a change,preventing infinite recursion.
            public BooleanExpression Distribute()
            {
                // Loop through factors to find an OrExpression.
                for (int i = 0; i < Factors.Count; i++)
                {
                    if (Factors[i] is OrExpression orExp)
                    {
                        // Split the factors into "before", the OrExpression, and "after".
                        var before = Factors.Take(i).ToList();
                        var after = Factors.Skip(i + 1).ToList();

                        List<BooleanExpression> distributedTerms = new List<BooleanExpression>();

                        // For each term in the OrExpression, build a new AndExpression.
                        foreach (var term in orExp.Terms)
                        {
                            var newFactors = new List<BooleanExpression>();
                            newFactors.AddRange(before);
                            newFactors.Add(term);
                            newFactors.AddRange(after);

                            // Create a new AndExpression from newFactors.
                            // We call Simplify() to ensure the new expression is reduced.
                            distributedTerms.Add(new AndExpression(newFactors).Simplify());
                        }

                        // Create an OrExpression from the distributed terms.
                        var result = new OrExpression(distributedTerms).Simplify();

                        // If the distribution did not change the expression, return this to avoid infinite recursion.
                        if (result.ToString() == this.ToString())
                            return this;
                        else
                            return result;
                    }
                }
                // If no factor is an OrExpression, return this expression as is.
                return this;
            }

            //This method overrides Equals to determine if another object is an AndExpression that, once simplified, has exactly the same factors (in the same order) as this one.
            public override bool Equals(object obj)
            {
                if (obj is AndExpression other)
                {
                    var thisSimp = this.Simplify() as AndExpression;
                    var otherSimp = other.Simplify() as AndExpression;
                    return thisSimp.Factors.SequenceEqual(otherSimp.Factors);
                }
                return false;
            }


            //This method generates a hash code for an AndExpression by iterating over its factors.
            //It multiplies a running hash (starting at 17) by 23 and adds each factor's hash code.
            //This ensures that two AndExpressions with the same factors (in the same order) produce the same hash, which is essential for using these objects in hash-based collections like Dictionary or HashSet.
            public override int GetHashCode()
            {
                int hash = 17;
                foreach (var factor in Factors)
                    hash = hash * 23 + factor.GetHashCode();
                return hash;
            }
        }

        // Represents a boolean OR operation with multiple terms (e.g. A+B+C).
        public class OrExpression : BooleanExpression
        {
            public List<BooleanExpression> Terms;

            public OrExpression(IEnumerable<BooleanExpression> terms)
            {
                Terms = new List<BooleanExpression>(terms);
            }

            //Simplifies the OR expression by:
            // 1) Recursively simplifying each term,
            // 2) Flattening nested ORs,
            // 3) Removing duplicate terms,
            // 4) Applying the absorption rule (A + A*B = A),
            // 5) If there's only one term left, return it,
            // 6) Otherwise, return a new OrExpression with the cleaned-up list.
            public override BooleanExpression Simplify()
            {
                List<BooleanExpression> newTerms = new List<BooleanExpression>();
                foreach (var term in Terms)
                {
                    var simp = term.Simplify();
                    // Flatten nested OR expressions.
                    if (simp is OrExpression orExp)
                        newTerms.AddRange(orExp.Terms);
                    else
                        newTerms.Add(simp);
                }

                // Remove duplicates.
                newTerms = newTerms.Distinct().ToList();

                // Apply absorption rules (e.g. A + A*B = A).
                newTerms = RemoveAbsorbedTerms(newTerms);

                if (newTerms.Count == 1)
                    return newTerms[0];

                newTerms.Sort((a, b) => a.ToString().CompareTo(b.ToString()));
                return new OrExpression(newTerms);
            }

            // Joins all terms with the '+' operator (e.g. "A+B+C*D").
            public override string ToString() => string.Join("+", Terms.Select(t => t.ToString()));

            public override bool Equals(object obj)
            {
                if (obj is OrExpression other)
                {
                    var thisSimp = this.Simplify() as OrExpression;
                    var otherSimp = other.Simplify() as OrExpression;
                    return thisSimp.Terms.SequenceEqual(otherSimp.Terms);
                }
                return false;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                foreach (var term in Terms)
                    hash = hash * 23 + term.GetHashCode();
                return hash;
            }

            //Absorption rule in Boolean algebra, uses IsSubexpression methode
            private List<BooleanExpression> RemoveAbsorbedTerms(List<BooleanExpression> terms)
            {
                List<BooleanExpression> result = new List<BooleanExpression>(terms);
                for (int i = 0; i < terms.Count; i++)
                {
                    for (int j = 0; j < terms.Count; j++)
                    {
                        if (i != j)
                        {
                            if (IsSubexpression(terms[i], terms[j]))
                            {
                                result.Remove(terms[j]);
                            }
                        }
                    }
                }
                return result;
            }

            //This method checks if expression a is considered a subexpression of expression b.
            //This check is used, when applying absorption rules (like A + A*B = A) in Boolean algebra, to determine if one expression is contained within another.
            private bool IsSubexpression(BooleanExpression a, BooleanExpression b)
            {
                if (a.Equals(b))
                    return true;

                if (b is AndExpression andB)
                {
                    if (a is LiteralExpression)
                        return andB.Factors.Any(f => f.Equals(a));
                    else if (a is AndExpression andA)
                        return andA.Factors.All(f => andB.Factors.Any(x => x.Equals(f)));
                }
                return false;
            }
        }

        // A simple recursive-descent parser that recognizes:
        //  - Parentheses (...)
        //  - '+' as OR
        //  - '*' as AND
        private class BooleanExpressionParser
        {
            private string input;
            private int pos;

            public BooleanExpressionParser(string input)
            {
                this.input = input;
                pos = 0;
            }

            // Entry point for parsing the expression (starts with ParseOr).
            public BooleanExpression ParseExpression() => ParseOr();

            // ParseOr handles an OR(+) expression: OR is lower level, so we parse AND expressions first,
            // then look for '+' operators to chain them.
            private BooleanExpression ParseOr()
            {
                BooleanExpression left = ParseAnd();
                while (true)
                {
                    SkipWhitespace();
                    if (Match('+'))
                    {
                        Advance(); // skip '+'
                        BooleanExpression right = ParseAnd();
                        left = new OrExpression(new List<BooleanExpression> { left, right });
                    }
                    else
                        break;
                }
                return left;
            }

            // ParseAnd handles an AND (*) expression: we parse Factor expressions, then look for '*'.
            private BooleanExpression ParseAnd()
            {
                BooleanExpression left = ParseFactor();
                while (true)
                {
                    SkipWhitespace();
                    if (Match('*'))
                    {
                        Advance(); // skip '*'
                        BooleanExpression right = ParseFactor();
                        left = new AndExpression(new List<BooleanExpression> { left, right });
                    }
                    else
                        break;
                }
                return left;
            }

            // ParseFactor is the lowest level:
            // If we see '(', parse a subexpression inside parentheses,
            // otherwise parse a literal until we hit whitespace, '+', '*', or ')'.
            private BooleanExpression ParseFactor()
            {
                SkipWhitespace();
                if (Match('('))
                {
                    Advance(); // skip '('
                    BooleanExpression expr = ParseExpression();
                    SkipWhitespace();
                    if (Match(')'))
                    {
                        Advance(); // skip ')'
                    }
                    return expr;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    while (!IsAtEnd() && !Char.IsWhiteSpace(CurrentChar())
                           && CurrentChar() != '+' && CurrentChar() != '*' && CurrentChar() != ')')
                    {
                        sb.Append(CurrentChar());
                        Advance();
                    }
                    string literalText = sb.ToString().Trim();
                    var lit = new LiteralExpression(literalText);
                    return lit;
                }
            }

            // Skips any whitespace characters in the input.
            private void SkipWhitespace()
            {
                while (!IsAtEnd() && Char.IsWhiteSpace(CurrentChar()))
                    Advance();
            }

            // Checks if the current character matches 'c'.
            private bool Match(char c) => !IsAtEnd() && CurrentChar() == c;

            // Returns the current character in the input, without consuming it.
            private char CurrentChar() => input[pos];

            // Moves 'position' forward by one character.
            private void Advance() => pos++;

            // Checks if we've reached the end of the input string.
            private bool IsAtEnd() => pos >= input.Length;
        }
    }

