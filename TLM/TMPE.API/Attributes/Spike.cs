namespace TrafficManager.API.Attributes {
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Used to indicate code which has potential to
    /// cause a lag spike when invoked. Usually this
    /// is due to batch processing of long lists.
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
    public class Spike : Attribute {
        public Spike(string note) { }
        public Spike() { }
    }
}
