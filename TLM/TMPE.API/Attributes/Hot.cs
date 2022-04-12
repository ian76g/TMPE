namespace TrafficManager.API.Attributes {
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Used to indicate code on hotpaths. Ensure its
    /// performance tuned.
    ///
    /// This attribute is cosmetic and will be removed
    /// from non-DEBUG builds.
    /// </summary>
    [AttributeUsage(
       AttributeTargets.Constructor |
       AttributeTargets.Field |
       AttributeTargets.Method |
       AttributeTargets.Property)]
    [Conditional("DEBUG")]
    public class Hot : Attribute {
        public Hot(string note) { }
        public Hot() { }
    }
}
