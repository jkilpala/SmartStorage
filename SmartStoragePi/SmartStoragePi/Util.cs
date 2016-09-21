using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStoragePi
{
    public static class ByteUtils
    {
        public static IEnumerable<bool> GetBitsStartingFromLSB(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return (b % 2 == 0) ? false : true;    //b % 2 == 0
                b = (byte)(b >> 1);
            }
        }
        public static byte[] PackBoolsInByteArray(bool[] bools)
        {
            int len = bools.Length;
            int bytes = len >> 3;
            if ((len & 0x07) != 0) ++bytes;
            byte[] arr2 = new byte[bytes];
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                    arr2[i >> 3] |= (byte)(1 << (i & 0x07));
            }
            return arr2;
        }
        /// <summary>
        /// Bit-packs an array of booleans into bytes, one bit per boolean.
        /// </summary><remarks>
        /// Booleans are bit-packed into bytes, in order, from least significant
        /// bit to most significant bit of each byte.<br/>
        /// If the length of the input array isn't a multiple of eight, then one
        /// or more of the most significant bits in the last byte returned will
        /// be unused. Unused bits are zero / unset.
        /// </remarks>
        /// <param name="bools">An array of booleans to pack into bytes.</param>
        /// <returns>
        /// An IEnumerable&lt;byte&gt; of bytes each containing (up to) eight
        /// bit-packed booleans.
        /// </returns>
        public static IEnumerable<byte> PackBoolsInByteEnumerable(bool[] bools)
        {
            int len = bools.Length;
            int rem = len & 0x07; // hint: rem = len % 8.

            /*
            byte[] byteArr = rem == 0 // length is a multiple of 8? (no remainder?)
                ? new byte[len >> 3] // -yes-
                : new byte[(len >> 3)+ 1]; // -no-
             */

            const byte BZ = 0,
                B0 = 1 << 0, B1 = 1 << 1, B2 = 1 << 2, B3 = 1 << 3,
                B4 = 1 << 4, B5 = 1 << 5, B6 = 1 << 6, B7 = 1 << 7;

            byte b;
            int i = 0;
            for (int mul = len & ~0x07; i < mul; i += 8) // hint: len = mul + rem.
            {
                b = bools[i] ? B0 : BZ;
                if (bools[i + 1]) b |= B1;
                if (bools[i + 2]) b |= B2;
                if (bools[i + 3]) b |= B3;
                if (bools[i + 4]) b |= B4;
                if (bools[i + 5]) b |= B5;
                if (bools[i + 6]) b |= B6;
                if (bools[i + 7]) b |= B7;

                //byteArr[i >> 3] = b;
                yield return b;
            }

            if (rem != 0) // take care of the remainder...
            {
                b = bools[i] ? B0 : BZ; // (there is at least one more bool.)

                switch (rem) // rem is [1:7] (fall-through switch!)
                {
                    case 7:
                        if (bools[i + 6]) b |= B6;
                        goto case 6;
                    case 6:
                        if (bools[i + 5]) b |= B5;
                        goto case 5;
                    case 5:
                        if (bools[i + 4]) b |= B4;
                        goto case 4;
                    case 4:
                        if (bools[i + 3]) b |= B3;
                        goto case 3;
                    case 3:
                        if (bools[i + 2]) b |= B2;
                        goto case 2;
                    case 2:
                        if (bools[i + 1]) b |= B1;
                        break;
                        // case 1 is the statement above the switch!
                }

                //byteArr[i >> 3] = b; // write the last byte to the array.
                yield return b; // yield the last byte.
            }

            //return byteArr;
        }
    }
}
