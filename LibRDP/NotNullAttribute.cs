using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate | AttributeTargets.GenericParameter)]
public sealed class NotNullAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Class)]
public sealed class VersionAttribute : Attribute
{
    public string Name { get; set; }

    public string Date { get; set; }

    public string Describtion { get; set; }
}
