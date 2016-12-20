using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Saritasa.Tools.Messages.Common.Expressions.Reduce
{
    /// <summary>
    /// Visitor for reducing expression.
    /// </summary>
    public class ExpressionReduceVisitor : ExpressionVisitor, IExpressionReduceVisitor
    {
        private static readonly Dictionary<MemberTypes, Func<MemberInfo, Type>> TypeGetters = new Dictionary<MemberTypes, Func<MemberInfo, Type>>(2)
        {
            [MemberTypes.Field] = (memberInfo) => RetrieveFieldType(memberInfo),
            [MemberTypes.Property] = (memberInfo) => RetrievePropertyType(memberInfo)
        };

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

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int index = 0; index < arguments.Count; index++)
            {
                var arg = arguments[index];
                var canReduce = false;

                if (arg is BinaryExpression)
                {
                    var binaryExpression = arg as BinaryExpression;

                    canReduce |= binaryExpression.Left.NodeType == ExpressionType.MemberAccess &&
                                 binaryExpression.Right.NodeType == ExpressionType.MemberAccess;
                    canReduce |= binaryExpression.Left.NodeType == ExpressionType.Constant &&
                                 binaryExpression.Right.NodeType != ExpressionType.Constant;
                    canReduce |= binaryExpression.Left.NodeType != ExpressionType.Constant &&
                                 binaryExpression.Right.NodeType == ExpressionType.Constant;

                    if (canReduce)
                    {
                        var typeOfLeftExpression = RetrieveMemberType(binaryExpression.Left);
                        var typeOfFunc = Expression.GetFuncType(typeOfLeftExpression);

                        dynamic compiled = Expression.Lambda(typeOfFunc, arg).Compile();

                        var result = compiled.Invoke();

                        reducedArguments.Add(argumentIndex, Expression.Constant(result));
                    }
                }

                argumentIndex++;
            }

            if (reducedArguments.Count == 0)
            {
                return base.VisitMethodCall(node);
            }

            var reducedArgs = new List<Expression>(arguments.Count);

            for (int i = 0; i < arguments.Count; i++)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
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

        private static Type RetrieveMemberType(Expression expression)
        {
            if (expression is MemberExpression && TypeGetters.ContainsKey(((MemberExpression)expression).Member.MemberType))
            {
                var memberExpression = (MemberExpression)expression;
                return TypeGetters[memberExpression.Member.MemberType](memberExpression.Member);
            }
            if (expression is ConstantExpression)
            {
                var constantExpression = (ConstantExpression)expression;
                return constantExpression.Value.GetType();
            }

            return null;
        }

        private static Type RetrieveFieldType(MemberInfo info)
        {
            var fieldInfo = (FieldInfo)info;

            return fieldInfo.FieldType;
        }

        private static Type RetrievePropertyType(MemberInfo info)
        {
            var propertyInfo = (PropertyInfo)info;

            return propertyInfo.PropertyType;
        }
    }
}
