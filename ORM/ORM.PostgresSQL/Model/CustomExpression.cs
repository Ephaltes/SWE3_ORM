using System;
using System.Collections.Generic;
using System.Linq;

namespace ORM.PostgresSQL.Model
{
    public class CustomExpression
    {
        /// <summary>
        /// The left term of the expression; can either be a string term or a nested Expression.
        /// </summary>
        public object LeftSide { get; set; } = null;

        /// <summary>
        /// The operator.
        /// </summary>
        public CustomOperations Operator { get; set; } = CustomOperations.Equals;

        /// <summary>
        /// The right term of the expression; can either be an object for comparison or a nested Expression.
        /// </summary>
        public object RightSide { get; set; } = null;

    

        /// <summary>
        /// A structure in the form of term-operator-term that defines a Boolean evaluation within a WHERE clause.
        /// </summary>
        public CustomExpression()
        {
        }

        /// <summary>
        /// A structure in the form of term-operator-term that defines a Boolean evaluation within a WHERE clause.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested Expression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested Expression.</param>
        public CustomExpression(object left, CustomOperations oper, object right)
        {
            LeftSide = left;
            Operator = oper;
            RightSide = right;
        }

        /// <summary>
        /// An Expression that allows you to determine if an object is between two values, i.e. GreaterThanOrEqualTo the first value, and LessThanOrEqualTo the second value.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested Expression.</param> 
        /// <param name="right">List of two values where the first value is the lower value and the second value is the higher value.</param>
        public static CustomExpression Between(object left, List<object> right)
        {
            if (right == null) throw new ArgumentNullException(nameof(right));
            if (right.Count != 2) throw new ArgumentException("Right term must contain exactly two members.");
            CustomExpression startOfBetween = new CustomExpression(left, CustomOperations.GreaterThanOrEqualTo, right.First());
            CustomExpression endOfBetween = new CustomExpression(left, CustomOperations.LessThanOrEqualTo, right.Last());
            return PrependAndClause(startOfBetween, endOfBetween);
        }


        /// <summary>
        /// Display Expression in a human-readable string.
        /// </summary>
        /// <returns>String containing human-readable version of the Expression.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += "(";

            if (LeftSide is CustomExpression) ret += ((CustomExpression)LeftSide).ToString();
            else ret += LeftSide.ToString();

            ret += " " + Operator.ToString() + " ";

            if (RightSide is CustomExpression) ret += ((CustomExpression)RightSide).ToString();
            else ret += RightSide.ToString();

            ret += ")";
            return ret;
        }

        /// <summary>
        /// Prepends a new Expression using the supplied left term, operator, and right term using an AND clause.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested Expression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested Expression.</param>
        public void PrependAnd(object left, CustomOperations oper, object right)
        {
            CustomExpression e = new CustomExpression(left, oper, right);
            PrependAnd(e);
        }

        /// <summary>
        /// Prepends the Expression with the supplied Expression using an AND clause.
        /// </summary>
        /// <param name="prepend">The Expression to prepend.</param> 
        public void PrependAnd(CustomExpression prepend)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));

            CustomExpression orig = new CustomExpression(this.LeftSide, this.Operator, this.RightSide);
            CustomExpression e = PrependAndClause(prepend, orig);
            LeftSide = e.LeftSide;
            Operator = e.Operator;
            RightSide = e.RightSide;
        }

        /// <summary>
        /// Prepends a new Expression using the supplied left term, operator, and right term using an OR clause.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested Expression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested Expression.</param>
        public void PrependOr(object left, CustomOperations oper, object right)
        {
            CustomExpression e = new CustomExpression(left, oper, right);
            PrependOr(e);
        }

        /// <summary>
        /// Prepends the Expression with the supplied Expression using an OR clause.
        /// </summary>
        /// <param name="prepend">The Expression to prepend.</param> 
        public void PrependOr(CustomExpression prepend)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));

            CustomExpression orig = new CustomExpression(this.LeftSide, this.Operator, this.RightSide);
            CustomExpression e = PrependOrClause(prepend, orig);
            LeftSide = e.LeftSide;
            Operator = e.Operator;
            RightSide = e.RightSide;

            return;
        }

        /// <summary>
        /// Prepends the Expression in prepend to the Expression original using an AND clause.
        /// </summary>
        /// <param name="prepend">The Expression to prepend.</param>
        /// <param name="original">The original Expression.</param>
        /// <returns>A new Expression.</returns>
        public static CustomExpression PrependAndClause(CustomExpression prepend, CustomExpression original)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));
            if (original == null) throw new ArgumentNullException(nameof(original));
            CustomExpression ret = new CustomExpression
            {
                LeftSide = prepend,
                Operator = CustomOperations.And,
                RightSide = original
            };
            return ret;
        }

        /// <summary>
        /// Prepends the Expression in prepend to the Expression original using an OR clause.
        /// </summary>
        /// <param name="prepend">The Expression to prepend.</param>
        /// <param name="original">The original Expression.</param>
        /// <returns>A new Expression.</returns>
        public static CustomExpression PrependOrClause(CustomExpression prepend, CustomExpression original)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));
            if (original == null) throw new ArgumentNullException(nameof(original));
            CustomExpression ret = new CustomExpression
            {
                LeftSide = prepend,
                Operator = CustomOperations.Or,
                RightSide = original
            };
            return ret;
        }

        /// <summary>
        /// Convert a List of Expression objects to a nested Expression containing AND between each Expression in the list. 
        /// </summary>
        /// <param name="exprList">List of Expression objects.</param>
        /// <returns>A nested Expression.</returns>
        public static CustomExpression ListToNestedAndExpression(List<CustomExpression> exprList)
        {
            if (exprList == null) throw new ArgumentNullException(nameof(exprList));
            if (exprList.Count < 1) return null;

            int evaluated = 0;
            CustomExpression ret = null;
            CustomExpression left = null;
            List<CustomExpression> remainder = new List<CustomExpression>();

            if (exprList.Count == 1)
            {
                foreach (CustomExpression curr in exprList)
                {
                    ret = curr;
                    break;
                }

                return ret;
            }
            else
            {
                foreach (CustomExpression curr in exprList)
                {
                    if (evaluated == 0)
                    {
                        left = new CustomExpression();
                        left.LeftSide = curr.LeftSide;
                        left.Operator = curr.Operator;
                        left.RightSide = curr.RightSide;
                        evaluated++;
                    }
                    else
                    {
                        remainder.Add(curr);
                        evaluated++;
                    }
                }

                ret = new CustomExpression();
                ret.LeftSide = left;
                ret.Operator = CustomOperations.And;
                CustomExpression right = ListToNestedAndExpression(remainder);
                ret.RightSide = right;

                return ret;
            }
        }

        /// <summary>
        /// Convert a List of Expression objects to a nested Expression containing OR between each Expression in the list. 
        /// </summary>
        /// <param name="exprList">List of Expression objects.</param>
        /// <returns>A nested Expression.</returns>
        public static CustomExpression ListToNestedOrExpression(List<CustomExpression> exprList)
        {
            if (exprList == null) throw new ArgumentNullException(nameof(exprList));
            if (exprList.Count < 1) return null;

            int evaluated = 0;
            CustomExpression ret = null;
            CustomExpression left = null;
            List<CustomExpression> remainder = new List<CustomExpression>();

            if (exprList.Count == 1)
            {
                foreach (CustomExpression curr in exprList)
                {
                    ret = curr;
                    break;
                }

                return ret;
            }
            else
            {
                foreach (CustomExpression curr in exprList)
                {
                    if (evaluated == 0)
                    {
                        left = new CustomExpression();
                        left.LeftSide = curr.LeftSide;
                        left.Operator = curr.Operator;
                        left.RightSide = curr.RightSide;
                        evaluated++;
                    }
                    else
                    {
                        remainder.Add(curr);
                        evaluated++;
                    }
                }

                ret = new CustomExpression();
                ret.LeftSide = left;
                ret.Operator = CustomOperations.Or;
                CustomExpression right = ListToNestedOrExpression(remainder);
                ret.RightSide = right;

                return ret;
            }
        }
    }
    
}