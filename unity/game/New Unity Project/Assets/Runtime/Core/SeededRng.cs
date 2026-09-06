namespace Game.Runtime.Core
{
    /// <summary>
    /// Deterministic PCG-XSH-RR generator. Same seed yields the same sequence.
    /// seed=0 is mixed so internal state is never 0.
    /// </summary>
    public sealed class SeededRng
    {
        const ulong Multiplier = 6364136223846793005UL;
        const ulong Increment = 1442695040888963407UL; // must be odd
        const ulong SeedZeroMix = 0xA341316C9614F5B7UL;

        ulong _state;

        public SeededRng(uint seed)
        {
            ulong mixed = seed == 0 ? SeedZeroMix : seed;
            _state = 0UL;
            Advance();
            _state += mixed;
            Advance();
            if (_state == 0UL)
                _state = SeedZeroMix;
        }

        public uint NextUInt()
        {
            ulong old = _state;
            Advance();
            uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        /// <summary>
        /// Half-open unit interval [0, 1).
        /// </summary>
        public float NextFloat01()
        {
            return NextUInt() * (1.0f / 4294967296.0f);
        }

        void Advance()
        {
            _state = _state * Multiplier + Increment;
        }
    }
}
