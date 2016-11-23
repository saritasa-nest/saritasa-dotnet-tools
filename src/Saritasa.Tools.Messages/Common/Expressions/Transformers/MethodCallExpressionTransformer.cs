using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Saritasa.Tools.Messages.Common.Expressions.Transformers
{
    /// <summary>
    /// Method call tranformer.
    /// </summary>
    /// <remarks>
    /// TODO: add transform for Get(10 + 1, 20 + 2)
    /// </remarks>
    public class MethodCallExpressionTransformer : IExpressionTransformer
    {
        private Dictionary<MemberTypes, Func<MemberInfo, Type>> typeGetters = new Dictionary<MemberTypes, Func<MemberInfo, Type>>
        {
            [MemberTypes.Field] = (memberInfo) => RetrieveFieldType(memberInfo),
            [MemberTypes.Property] = (memberInfo) => RetrievePropertyType(memberInfo)
        };

        /// <inheritdoc/>
        public bool SupportTransform(ExpressionType nodeFrom)
        {
            return nodeFrom == ExpressionType.Call;
        }

        /// <inheritdoc/>
        public Expression Transform(Expression input, ExpressionTransformVisitor visitor)
        {
            var methodCallExpression = input as MethodCallExpression;
            if (methodCallExpression == null)
            {
                throw new ArgumentException("Can't cast input to method call expression.");
            }

            var parameters = new List<Expression>();
            var argumentIndex = 0;

            foreach (var methodCallArgument in methodCallExpression.Arguments)
            {
                if (methodCallArgument.NodeType == ExpressionType.Constant)
                {
                    var constantMethodCallArgument = methodCallArgument as ConstantExpression;
                    var parameterExpression = Expression.Parameter(constantMethodCallArgument.Type, $"p{argumentIndex}");
                    parameters.Add(parameterExpression);
                    visitor.TransformedParameterExpressions.Add(parameterExpression);
                }
                else if (methodCallArgument.NodeType == ExpressionType.MemberAccess)
                {
                    var memberAccessMethodCall = methodCallArgument as MemberExpression;
                    var propertyOrFieldType = typeGetters[memberAccessMethodCall.Member.MemberType](memberAccessMethodCall.Member);

                    var parameterExpression = Expression.Parameter(propertyOrFieldType, $"p{argumentIndex}");
                    parameters.Add(parameterExpression);
                    visitor.TransformedParameterExpressions.Add(parameterExpression);
                }
                else
                {
                    parameters.Add(methodCallArgument);
                }

                argumentIndex++;
            }

            return Expression.Call(methodCallExpression.Object, methodCallExpression.Method, parameters);
        }

        private static Type RetrieveFieldType(MemberInfo info)
        {
            var fieldInfo = info as FieldInfo;

            return fieldInfo.FieldType;
        }

        private static Type RetrievePropertyType(MemberInfo info)
        {
            var propertyInfo = info as PropertyInfo;

            return propertyInfo.PropertyType;
        }
    }
}
