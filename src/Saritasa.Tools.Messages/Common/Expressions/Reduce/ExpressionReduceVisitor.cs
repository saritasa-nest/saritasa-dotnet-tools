using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions.Reduce
{
    /// <summary>
    /// Visitor for reducing expression.
    /// </summary>
    public class ExpressionReduceVisitor : ExpressionVisitor, IExpressionReduceVisitor
    {
        /// <summary>
        /// Visiting node and reducing if we need this.
        /// </summary>
        /// <param name="node">Original expression before reduce.</param>
        public Expression VisitAndReduce(Expression node)
        {
            return Visit(node);
        }

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var arguments = node.Arguments;
            var argumentIndex = 0;
            var reducedArguments = new Dictionary<int, ConstantExpression>();

            foreach (var arg in arguments)
            {
                var canReduce = false;

                if (arg is BinaryExpression)
                {
                    var binaryExpression = arg as BinaryExpression;

                    canReduce |= binaryExpression.Left.NodeType == ExpressionType.MemberAccess && binaryExpression.Right.NodeType == ExpressionType.MemberAccess;
                    canReduce |= binaryExpression.Left.NodeType == ExpressionType.Constant && binaryExpression.Right.NodeType != ExpressionType.Constant;
                    canReduce |= binaryExpression.Left.NodeType != ExpressionType.Constant && binaryExpression.Right.NodeType == ExpressionType.Constant;

                    if (canReduce)
                    {
                        var leftMemberExpression = binaryExpression.Left as MemberExpression;
                        var rightMemberExpression = binaryExpression.Right as MemberExpression;

                        var compiled = Expression.Lambda(arg).Compile();
                        var result = compiled.DynamicInvoke();

                        reducedArguments.Add(argumentIndex, Expression.Constant(result));
                    }
                }

                argumentIndex++;
            }

            var reducedArgs = new List<Expression>();

            for (int i = 0; i < arguments.Count; i++)
            {
                if (reducedArguments.ContainsKey(i))
                {
                    reducedArgs.Add(reducedArguments[i]);
                }
                else
                {
                    reducedArgs.Add(node.Arguments[i]);
                }
            }

            return base.VisitMethodCall(Expression.Call(node.Object, node.Method, reducedArgs));
        }
    }
}
