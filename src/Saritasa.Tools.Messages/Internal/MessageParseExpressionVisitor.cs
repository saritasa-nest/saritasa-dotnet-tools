// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Internal
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Common;

    /// <summary>
    /// Search for conditions in expression tree. Does not support complex expressions.
    /// </summary>
    internal class MessageParseExpressionVisitor : ExpressionVisitor
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        DateTime GetDate(BinaryExpression conditionalNode)
        {
            if (conditionalNode.Left.NodeType == ExpressionType.MemberAccess && conditionalNode.Right.NodeType == ExpressionType.MemberAccess)
            {
                var leftMemberNode = (MemberExpression)conditionalNode.Left;
                if (leftMemberNode.Member.Name != nameof(Message.CreatedAt))
                {
                    return DateTime.MinValue;
                }
                var memberNode = (MemberExpression)conditionalNode.Right;
                var constantExpression = (ConstantExpression)memberNode.Expression;
                return ((DateTime)((FieldInfo)memberNode.Member).GetValue(constantExpression.Value)).Date;
            }
            return DateTime.MinValue;
        }

        public override Expression Visit(Expression node)
        {
            if (node.NodeType == ExpressionType.GreaterThan || node.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                var date = GetDate((BinaryExpression)node);
                if (date > StartDate)
                {
                    StartDate = date;
                }
            }
            else if (node.NodeType == ExpressionType.LessThan || node.NodeType == ExpressionType.LessThanOrEqual)
            {
                var date = GetDate((BinaryExpression)node);
                if (date < EndDate)
                {
                    EndDate = date;
                }
            }

            return base.Visit(node);
        }
    }
}
