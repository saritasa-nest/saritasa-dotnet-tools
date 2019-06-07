// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Base middleware to resolve objects.
    /// </summary>
    public class BaseHandlerResolverMiddleware
    {
        /// <summary>
        /// Handle object key.
        /// </summary>
        public const string HandlerObjectKey = "handler-object";

        /// <summary>
        /// Resolve handler object public properties using service provider. Default is <c>false</c>.
        /// </summary>
        public bool UsePropertiesResolving { get; set; } = false;

        /// <summary>
        /// If <c>true</c> the middleware resolves dependencies using internal resolver.
        /// </summary>
        protected bool UseInternalObjectResolver { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="useInternalObjectResolver">Use internal object resolver for handlers.
        /// Otherwise <see cref="IServiceProvider" /> will be used.</param>
        public BaseHandlerResolverMiddleware(bool useInternalObjectResolver)
        {
            this.UseInternalObjectResolver = useInternalObjectResolver;
        }

        private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object>> objectFactoriesCache =
            new ConcurrentDictionary<Type, Func<IServiceProvider, object>>();

        private static readonly Expression<Func<IServiceProvider, object>> returnNullExpression = sp => null;

        /// <summary>
        /// Creates handler object from type. Also it creates expression tree and cache it for
        /// better performance.
        /// </summary>
        /// <param name="type">Object type.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>Handler object.</returns>
        protected object CreateHandlerWithCache(Type type, IServiceProvider serviceProvider)
        {
            var handlerFactory = objectFactoriesCache.GetOrAdd(
                type,
                t => CreateNewObjectFactory(t).Compile()
            );
            return handlerFactory(serviceProvider);
        }

        /// <summary>
        /// Creates new object factory. Find public ctor and inject services.
        /// </summary>
        /// <param name="type">Object type.</param>
        /// <returns>Expression.</returns>
        protected Expression<Func<IServiceProvider, object>> CreateNewObjectFactory(Type type)
        {
            // Find most descriptive ctor.
            var ctor = type
                .GetTypeInfo()
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .OrderByDescending(x => x.GetParameters().Length)
                .FirstOrDefault();

            // Local vars.
            if (ctor == null)
            {
                return returnNullExpression;
            }
            var expressions = new List<Expression>();
            var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "sp");
            var handlerVariableExpression = Expression.Variable(type, "handler");

            // Prepare expressions statements.
            expressions.Add(Expression.Assign(handlerVariableExpression,
                CreateInstantiateHandlerExpression(ctor, serviceProviderParam)));
            if (UsePropertiesResolving)
            {
                expressions.Add(CreatePropertiesResolveExpression(type, handlerVariableExpression, serviceProviderParam));
            }

            // Return.
            expressions.Add(handlerVariableExpression);
            return Expression.Lambda<Func<IServiceProvider, object>>(
                Expression.Block(new[] { handlerVariableExpression }, expressions), serviceProviderParam);
        }

        /// <summary>
        /// Prepares expression: NEW UserHandlers((IInterface1)sp.GetService(typeof(IInterface1))) .
        /// </summary>
        /// <param name="ctor">Constructor descriptor.</param>
        /// <param name="serviceProviderParam">Parameter of type <see cref="IServiceProvider" />.</param>
        /// <returns>Expression.</returns>
        private static Expression CreateInstantiateHandlerExpression(ConstructorInfo ctor,
            ParameterExpression serviceProviderParam)
        {
            var ctorParams = ctor.GetParameters();
            var ctorExpressionArguments = new List<Expression>(ctorParams.Length);
            var getServiceMethod = typeof(IServiceProvider).GetTypeInfo().GetMethod("GetService");

            foreach (ParameterInfo paramInfo in ctorParams)
            {
                ctorExpressionArguments.Add(
                    Expression.Convert(
                        Expression.Call(serviceProviderParam, getServiceMethod,
                            Expression.Constant(paramInfo.ParameterType)),
                        paramInfo.ParameterType));
            }
            return Expression.New(ctor, ctorExpressionArguments);
        }

        private static readonly Expression nullConstantExpression = Expression.Constant(null, typeof(object));

        /// <summary>
        /// Prepares expression for every public property:
        /// IF (obj.Prop1 == null) { (obj.Prop1.GetType()) sp.GetService(obj.Prop1.GetType()); }.
        /// </summary>
        /// <param name="type">Handler type.</param>
        /// <param name="handlerParam">Handler parameter expression.</param>
        /// <param name="serviceProviderParam">Parameter of type <see cref="IServiceProvider" />.</param>
        /// <returns>Expression.</returns>
        private static Expression CreatePropertiesResolveExpression(Type type, ParameterExpression handlerParam,
            ParameterExpression serviceProviderParam)
        {
            var getServiceMethod = typeof(IServiceProvider).GetTypeInfo().GetMethod("GetService");
            var props = type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance |
                                                            BindingFlags.FlattenHierarchy);
            var expressions = new List<Expression>();
            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite)
                {
                    continue;
                }
                var expr = Expression.IfThen(
                    Expression.Equal(
                        Expression.Call(handlerParam, prop.GetGetMethod(true)), nullConstantExpression),
                    Expression.Call(handlerParam, prop.GetSetMethod(true),
                        Expression.Convert(
                            Expression.Call(serviceProviderParam, getServiceMethod, Expression.Constant(prop.PropertyType)),
                            prop.PropertyType))
                );
                expressions.Add(expr);
            }

            return expressions.Count > 0 ? Expression.Block(expressions) : (Expression) Expression.Empty();
        }
    }
}
