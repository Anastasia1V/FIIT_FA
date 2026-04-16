using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
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
        int length = Math.Max(digitsA.Length, digitsB.Length);
        if (length <= 32) {
            SimpleMultiplier multiplier = new SimpleMultiplier();
            return multiplier.Multiply(a, b);
        }
        int halfLength = length / 2;
        if (length % 2 != 0) {
            halfLength++;
        }
        BetterBigInteger highA;
        BetterBigInteger lowA;
        Split(a, halfLength, out lowA, out highA);
        BetterBigInteger highB;
        BetterBigInteger lowB;
        Split(b, halfLength, out lowB, out highB);
        BetterBigInteger sumA = lowA + highA;
        BetterBigInteger sumB = lowB + highB;
        BetterBigInteger z0 = Multiply(lowA, lowB);
        BetterBigInteger z1 = Multiply(sumA, sumB);
        BetterBigInteger z2 = Multiply(highA, highB);
        BetterBigInteger middle = z1 - z2 - z0;
        BetterBigInteger result = z0 + (middle << (halfLength * 32)) + (z2 << (halfLength * 64));
        bool negative;
        if (a.IsNegative != b.IsNegative) {
            negative = true;
        } else {
            negative = false;
        }
        if (negative) {
            return -result;
        }
        return result;
    }
    
    private void Split(BetterBigInteger number, int length, out BetterBigInteger low, out BetterBigInteger high)
    {
        ReadOnlySpan<uint> digits = number.GetDigits();
        int lowLength;
        if (digits.Length < length) {
            lowLength = digits.Length;
        } else {
            lowLength = length;
        }
        ReadOnlySpan<uint> lowSpan = digits.Slice(0, lowLength);
        low = CreateInteger(lowSpan, false);
        if (digits.Length <= length) {
            high = new BetterBigInteger(new uint[] { 0 });
        } else {
            int highLength = digits.Length - length;
            ReadOnlySpan<uint> highSpan = digits.Slice(length, highLength);
            high = CreateInteger(highSpan, false);
        }
    }

    private static BetterBigInteger CreateInteger(ReadOnlySpan<uint> span, bool isNegative)
    {
        if (span.Length == 0) {
            return new BetterBigInteger(new uint[] { 0 });
        }
        if (span.Length == 1) {
            return new BetterBigInteger(new uint[] { span[0] }, isNegative);
        }
        uint[] array = span.ToArray();
        return new BetterBigInteger(array, isNegative);
    }
}