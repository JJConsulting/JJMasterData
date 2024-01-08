using System;

namespace JJMasterData.Commons.Data.Entity.Models;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public abstract class ExpressionAttribute : Attribute;