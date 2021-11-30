using System;

namespace Maple
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class NodeCategoryAttribute : Attribute
    {
        public string[] Category;
        public NodeCategoryAttribute(params string[] category) => Category = category;
    }
}
