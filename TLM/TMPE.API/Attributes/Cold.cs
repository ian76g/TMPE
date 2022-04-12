namespace TrafficManager.API.Attributes {
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Used to indicate rarely used code, or code which
    /// is not suitable for use on hotpath.
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
    public class Cold : Attribute {
        public Cold(string note) { }
        public Cold() { }
    }
}
