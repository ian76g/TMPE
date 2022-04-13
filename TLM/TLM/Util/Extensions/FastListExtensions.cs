namespace TrafficManager.Util.Extensions {
    using System;

    public static class FastListExtensions {

        public static void MakeRoomFor<T1>(this FastList<T1> @this, int numItems) =>
            @this.EnsureCapacity(@this.m_size + numItems);

        public static int Count<T1>(this FastList<T1> @this) =>
            @this.m_size;

        public static void Each<T1>(this FastList<T1> @this, Action<T1> action) {
            for (var i = 0; i < @this.m_size; i++)
                action(@this.m_buffer[i]);
        }
    }
}
