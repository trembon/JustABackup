using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Base.Attributes
{
    /// <summary>
    /// Attribute that defines a string property to be transform before being used in a provider.
    /// Example: {date} will be transformed for the current date using ToShortDateString().
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TransformAttribute : Attribute
    {
    }
}