using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lab6App
{
    internal class Union
    {
        [StructLayout(LayoutKind.Explicit)]
        protected struct union
        {
            [FieldOffset(0)]
            public byte byte1;

            [FieldOffset(1)]
            public byte byte2;

            [FieldOffset(2)]
            public byte byte3;

            [FieldOffset(3)]
            public byte byte4;

            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public Int32 i32;

            [FieldOffset(0)]
            public Int16 i16_1;

            [FieldOffset(2)]
            public Int16 i16_2;

            [FieldOffset(0)]
            public UInt32 ui32;

            [FieldOffset(0)]
            public UInt16 ui16_1;

            [FieldOffset(2)]
            public UInt16 ui16_2;

        };

        union uni;

        public Union() { uni = new union(); }

        public byte byte1 { get { return uni.byte1; } set { uni.byte1 = value; } }
        public byte byte2 { get { return uni.byte2; } set { uni.byte2 = value; } }
        public byte byte3 { get { return uni.byte3; } set { uni.byte3 = value; } }
        public byte byte4 { get { return uni.byte4; } set { uni.byte4 = value; } }
        public float f { get { return uni.f; } set { uni.f = value; } }
        public UInt16 ui16_1 { get { return uni.ui16_1; } set { uni.ui16_1 = value; } }
        public UInt16 ui16_2 { get { return uni.ui16_2; } set { uni.ui16_2 = value; } }
        public Int16 i16_1 { get { return uni.i16_1; } set { uni.i16_1 = value; } }
        public Int16 i16_2 { get { return uni.i16_2; } set { uni.i16_2 = value; } }
        public Int32 i32 { get { return uni.i32; } set { uni.i32 = value; } }
        public UInt32 ui32 { get { return uni.ui32; } set { uni.ui32 = value; } }
    }
}
