namespace QuantumCore.Core.Types
{
    /// <summary>
    /// LZO1Z implementation based on lzokay.
    /// https://github.com/jackoalan/lzokay/blob/master/lzokay.cpp
    /// </summary>
    public class Lzo
    {
        private const uint M1Marker = 0x0;
        private const uint M2Marker = 0x40;
        private const uint M3Marker = 0x20;
        private const uint M4Marker = 0x10;

        private uint _size;
        
        public Lzo(uint size)
        {
            _size = size;
        }
        
        public byte[] Decode(byte[] data)
        {
            using var src = new MemoryStream(data);
            src.Seek(4, SeekOrigin.Begin);
            var buffer = new byte[_size];
            Decompress(src, buffer);
            return buffer;
        }

        private int ConsumeZeroByteLength(MemoryStream src)
        {
            var pos = src.Position;
            while (src.ReadByte() == 0)
            {
            }

            src.Seek(-1, SeekOrigin.Current);

            return (int) src.Position - (int) pos;
        }

        private void Decompress(MemoryStream src, byte[] dest)
        {
            var srcSize = src.Length;
            var destSize = src.Length;

            var destPos = 0;

            var lbcur = destPos;
            var lblen = 0;
            var nstate = 0;
            var state = 0;

            // First byte encoding
            var firstByte = src.ReadByte();
            src.Seek(-1, SeekOrigin.Current);
            if (firstByte >= 22)
            {
                /* 22..255 : copy literal string
                 *           length = (byte - 17) = 4..238
                 *           state = 4 [ don't copy extra literals ]
                 *           skip byte
                 */
                var length = src.ReadByte() - 17;
                for (var i = 0; i < length; i++)
                {
                    dest[destPos++] = (byte) src.ReadByte();
                }

                state = 4;
            } else if (firstByte >= 18)
            {
                /* 18..21 : copy 0..3 literals
                 *          state = (byte - 17) = 0..3  [ copy <state> literals ]
                 *          skip byte
                 */
                nstate = src.ReadByte() - 17;
                state = nstate;
                for (var i = 0; i < nstate; i++)
                {
                    dest[destPos++] = (byte) src.ReadByte();
                }
            }
            
            /* 0..17 : follow regular instruction encoding, see below. It is worth
             *         noting that codes 16 and 17 will represent a block copy from
             *         the dictionary which is empty, and that they will always be
             *         invalid at this place.
             */
            while (true)
            {
                var pos = src.Position - 3;
                var inst = src.ReadByte();
                //Console.WriteLine($"Inst {inst} src.position {pos}");
                if ((inst & 0xC0) != 0)
                {
                    /* [M2]
                     * 1 L L D D D S S  (128..255)
                     *   Copy 5-8 bytes from block within 2kB distance
                     *   state = S (copy S literals after this block)
                     *   length = 5 + L
                     * Always followed by exactly one byte : H H H H H H H H
                     *   distance = (H << 3) + D + 1
                     *
                     * 0 1 L D D D S S  (64..127)
                     *   Copy 3-4 bytes from block within 2kB distance
                     *   state = S (copy S literals after this block)
                     *   length = 3 + L
                     * Always followed by exactly one byte : H H H H H H H H
                     *   distance = (H << 3) + D + 1
                     */
                    lbcur = destPos - ((src.ReadByte() << 3) + ((inst >> 2) & 0x7) + 1);
                    lblen = (inst >> 5) + 1;
                    nstate = inst & 0x3;
                } else if ((inst & M3Marker) != 0)
                {
                    /* [M3]
                     * 0 0 1 L L L L L  (32..63)
                     *   Copy of small block within 16kB distance (preferably less than 34B)
                     *   length = 2 + (L ?: 31 + (zero_bytes * 255) + non_zero_byte)
                     * Always followed by exactly one LE16 :  D D D D D D D D : D D D D D D S S
                     *   distance = D + 1
                     *   state = S (copy S literals after this block)
                     */
                    lblen = (inst & 0x1f) + 2;
                    if (lblen == 2)
                    {
                        var offset = ConsumeZeroByteLength(src);
                        // CONSUME_ZERO_BYTE_LENGTH
                        lblen += offset * 255 + 31 + src.ReadByte();
                    }

                    nstate = src.ReadByte() + (src.ReadByte() << 8);
                    lbcur = destPos - ((nstate >> 2) + 1);
                    nstate &= 0x3;
                } else if ((inst & M4Marker) != 0)
                {
                    /* [M4]
                     * 0 0 0 1 H L L L  (16..31)
                     *   Copy of a block within 16..48kB distance (preferably less than 10B)
                     *   length = 2 + (L ?: 7 + (zero_bytes * 255) + non_zero_byte)
                     * Always followed by exactly one LE16 :  D D D D D D D D : D D D D D D S S
                     *   distance = 16384 + (H << 14) + D
                     *   state = S (copy S literals after this block)
                     *   End of stream is reached if distance == 16384
                     */
                    lblen = (inst & 0x7) + 2;
                    if (lblen == 2)
                    {
                        var offset = ConsumeZeroByteLength(src);
                        lblen += offset * 255 + 7 + src.ReadByte();
                    }
                    
                    nstate = src.ReadByte() + (src.ReadByte() << 8);
                    lbcur = destPos - (((inst & 0x8) << 11) + (nstate >> 2));
                    nstate &= 0x3;
                    
                    if (lbcur == destPos)
                        break; /* Stream finished */
                    lbcur -= 16384;
                }
                else
                {
                    /* [M1] Depends on the number of literals copied by the last instruction. */
                    if (state == 0)
                    {
                        /* If last instruction did not copy any literal (state == 0), this
                         * encoding will be a copy of 4 or more literal, and must be interpreted
                         * like this :
                         *
                         *    0 0 0 0 L L L L  (0..15)  : copy long literal string
                         *    length = 3 + (L ?: 15 + (zero_bytes * 255) + non_zero_byte)
                         *    state = 4  (no extra literals are copied)
                         */
                        var len = inst + 3;
                        if (len == 3)
                        {
                            var offset = ConsumeZeroByteLength(src);
                            len += offset * 255 + 15 + src.ReadByte();
                        }

                        for (var i = 0; i < len; i++)
                        {
                            dest[destPos++] = (byte) src.ReadByte();
                        }

                        state = 4;
                        continue;
                    } else if (state != 4) {
                        /* If last instruction used to copy between 1 to 3 literals (encoded in
                         * the instruction's opcode or distance), the instruction is a copy of a
                         * 2-byte block from the dictionary within a 1kB distance. It is worth
                         * noting that this instruction provides little savings since it uses 2
                         * bytes to encode a copy of 2 other bytes but it encodes the number of
                         * following literals for free. It must be interpreted like this :
                         *
                         *    0 0 0 0 D D S S  (0..15)  : copy 2 bytes from <= 1kB distance
                         *    length = 2
                         *    state = S (copy S literals after this block)
                         *  Always followed by exactly one byte : H H H H H H H H
                         *    distance = (H << 2) + D + 1
                         */
                        nstate = inst & 0x3;
                        lbcur = destPos - ((inst >> 2) + (src.ReadByte() << 2) + 1);
                        lblen = 2;
                    } else {
                        /* If last instruction used to copy 4 or more literals (as detected by
                         * state == 4), the instruction becomes a copy of a 3-byte block from the
                         * dictionary from a 2..3kB distance, and must be interpreted like this :
                         *
                         *    0 0 0 0 D D S S  (0..15)  : copy 3 bytes from 2..3 kB distance
                         *    length = 3
                         *    state = S (copy S literals after this block)
                         *  Always followed by exactly one byte : H H H H H H H H
                         *    distance = (H << 2) + D + 2049
                         */
                        nstate = inst & 0x3;
                        lbcur = destPos - ((inst >> 2) + (src.ReadByte() << 2) + 2049);
                        lblen = 3;
                    }
                }

                //Console.WriteLine($"lbcur {lbcur} lblen {lblen} destpos {destPos} srcPos {src.Position - 4}");
                if (lbcur < 0)
                {
                    throw new Exception("LookbehindOverrun");
                }
                
                /* Copy lookbehind */
                for (var i = 0; i < lblen; i++)
                {
                    dest[destPos++] = dest[lbcur++];
                }
                
                state = nstate;
                
                /* Copy literal */
                for (var i = 0; i < nstate; ++i)
                {
                    dest[destPos++] = (byte) src.ReadByte();
                }
            }

            if (lblen != 3)
            {
                throw new Exception("Ensure terminating M4 was encountered");
            }
        }
    }
}