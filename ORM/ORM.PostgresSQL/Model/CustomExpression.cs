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
        /// Prepends the Expression with the supplied Expression using an AND clause.
        /// </summary>
        /// <param name="prepend">The Expression to prepend.</param> 
        public void PrependAnd(CustomExpression prepend)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));

            CustomExpression orig = new CustomExpression(LeftSide, Operator, RightSide);
            CustomExpression e = PrependAndClause(prepend, orig);
            LeftSide = e.LeftSide;
            Operator = e.Operator;
            RightSide = e.RightSide;
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
        
    }
    
}