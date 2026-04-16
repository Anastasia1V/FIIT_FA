using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class FftMultiplier : IMultiplier
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
        int fftLength = 1;
        int resultLength = digitsA.Length + digitsB.Length;
        while (fftLength < resultLength * 2) {
            fftLength <<= 1;
        }
        double[] realA = new double[fftLength];
        double[] imagA = new double[fftLength];
        double[] realB = new double[fftLength];
        double[] imagB = new double[fftLength];
        for (int i = 0; i < digitsA.Length; i++) {
            uint value = digitsA[i];
            realA[2 * i] = value & 0xFFFF;
            realA[2 * i + 1] = value >> 16;
        }
        for (int i = 0; i < digitsB.Length; i++) {
            uint value = digitsB[i];
            realB[2 * i] = value & 0xFFFF;
            realB[2 * i + 1] = value >> 16;
        }
        Fft(realA, imagA, false);
        Fft(realB, imagB, false);
        for (int i = 0; i < fftLength; i++) {
            double real = realA[i] * realB[i] - imagA[i] * imagB[i];
            double imag = realA[i] * imagB[i] + imagA[i] * realB[i];
            realA[i] = real;
            imagA[i] = imag;
        }
        Fft(realA, imagA, true);
        long[] fftArray = new long[fftLength];
        for (int i = 0; i < fftLength; i++) {
            fftArray[i] = (long)Math.Round(realA[i]);
        }
        for (int i = 0; i < fftLength - 1; i++) {
            long carry = fftArray[i] >> 16;
            fftArray[i] &= 0xFFFF;
            fftArray[i + 1] += carry;
        }
        uint[] result = new uint[resultLength];
        for (int i = 0; i < resultLength; i++) {
            long low = fftArray[2 * i];
            long high = fftArray[2 * i + 1];
            result[i] = (uint)(low | (high << 16));
        }
        return new BetterBigInteger(result, a.IsNegative ^ b.IsNegative);
    }

    private static void Fft(double[] real, double[] imag, bool invert)
    {
        int length = real.Length;
        for (int i = 1, j = 0; i < length; i++) {
            int bit = length >> 1;
            while ((j & bit) != 0) {
                j ^= bit;
                bit >>= 1;
            }
            j |= bit;
            if (i < j) {
                double currentReal = real[i];
                real[i] = real[j];
                real[j] = currentReal;
                double currentImag = imag[i];
                imag[i] = imag[j];
                imag[j] = currentImag;
            }
        }
        for (int len = 2; len <= length; len <<= 1) {
            double angle = 2 * Math.PI / len;
            if (invert) {
                angle = -angle;
            }
            double realRotation = Math.Cos(angle);
            double imagRotation = Math.Sin(angle);
            for (int i = 0; i < length; i += len) {
                double currentReal = 1;
                double currentImag = 0;
                for (int j = 0; j < len / 2; j++) {
                    int leftIndex = i + j;
                    int rightIndex = i + j + len / 2;
                    double leftReal = real[leftIndex];
                    double leftImag = imag[leftIndex];
                    double rightReal = real[rightIndex] * currentReal - imag[rightIndex] * currentImag;
                    double rightImag = real[rightIndex] * currentImag + imag[rightIndex] * currentReal;
                    real[leftIndex] = leftReal + rightReal;
                    imag[leftIndex] = leftImag + rightImag;
                    real[rightIndex] = leftReal - rightReal;
                    imag[rightIndex] = leftImag - rightImag;
                    double nextReal = currentReal * realRotation - currentImag * imagRotation;
                    double nextImag = currentReal * imagRotation + currentImag * realRotation;
                    currentReal = nextReal;
                    currentImag = nextImag;
                }
            }
        }
        if (invert) {
            for (int i = 0; i < length; i++) {
                real[i] /= length;
                imag[i] /= length;
            }
        }
    }
}