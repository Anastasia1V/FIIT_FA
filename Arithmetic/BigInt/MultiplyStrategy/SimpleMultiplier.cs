using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        ReadOnlySpan<uint> digitsA = a.GetDigits();
        ReadOnlySpan<uint> digitsB = b.GetDigits();
        if (digitsA.Length == 1 && digitsA[0] == 0) {
            return new BetterBigInteger(new uint[] { 0 });
        }
        if (digitsB.Length == 1 && digitsB[0] == 0) {
            return new BetterBigInteger(new uint[] { 0 });
        }
        int lengthA = digitsA.Length;
        int lengthB = digitsB.Length;
        uint[] result = new uint[lengthA + lengthB];
        for (int i = 0; i < lengthA; i++) {
            ulong carry = 0;
            ulong digitA = digitsA[i];
            for (int j = 0; j < lengthB; j++) {
                ulong digitB = digitsB[j];
                ulong currentResult = digitA * digitB + result[i + j] + carry;
                result[i + j] = (uint)currentResult;
                carry = currentResult >> 32;
            }
            if (carry != 0) {
                result[i + lengthB] = (uint)carry;
            }
        }
        bool negative;
        if (a.IsNegative != b.IsNegative) {
            negative = true;
        } else {
            negative = false;
        }
        return new BetterBigInteger(result, negative);
    }
}