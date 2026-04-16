using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic.BigInt;

public sealed class BetterBigInteger : IBigInteger
{
    private int _signBit;
    
    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;
    
    public bool IsNegative => _signBit == 1;
    
    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        if (digits == null) {
            throw new ArgumentNullException(nameof(digits), "Некорректный массив цифр");
        }
        int lastIndex = digits.Length - 1;
        while (lastIndex > 0 && digits[lastIndex] == 0) {
            lastIndex--;
        }
        if (lastIndex == 0) {
            _smallValue = digits[0];
            _data = null;
        } else {
            _data = new uint[lastIndex + 1];
            for (int i = 0; i <= lastIndex; i++) {
                _data[i] = digits[i];
            }
            _smallValue = 0;
        }
        if (_data == null && _smallValue == 0) {
            _signBit = 0;
        } else {
            if (isNegative) {
                _signBit = 1;
            } else {
                _signBit = 0;
            }
        }
    }
    
    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false)
    {
        if (digits == null) {
            throw new ArgumentNullException(nameof(digits), "Некорректный массив цифр");
        }
        uint[] array = digits.ToArray();
        if (array.Length == 0) {
            _smallValue = 0;
            _data = null;
            _signBit = 0;
            return;
        }
        int lastIndex = array.Length - 1;
        while (lastIndex > 0 && array[lastIndex] == 0) {
            lastIndex--;
        }
        if (lastIndex == 0) {
            _smallValue = array[0];
            _data = null;
        } else {
            _data = new uint[lastIndex + 1];
            for (int i = 0; i <= lastIndex; i++) {
                _data[i] = array[i];
            }
            _smallValue = 0;
        }
        if (_data == null && _smallValue == 0) {
            _signBit = 0;
        } else {
            if (isNegative) {
                _signBit = 1;
            } else {
                _signBit = 0;
            }
        }
    }
    
    public BetterBigInteger(string value, int radix)
    {
        if (value == null) {
            throw new ArgumentNullException(nameof(value), "Некорректная строка");
        }
        if (radix < 2 || radix > 36) {
            throw new ArgumentException(nameof(radix), "Некорректное основание");
        }
        if (value.Length == 0) {
            throw new ArgumentException(nameof(value), "Пустая строка");
        }
        int index = 0;
        bool negative = false;
        if (value[0] == '-') {
            negative = true;
            index++;
        } else if (value[0] == '+') {
            index++;
        }
        BetterBigInteger result = new BetterBigInteger(new uint[] { 0 });
        BetterBigInteger integerRadix = new BetterBigInteger(new uint[] { (uint)radix });
        for (int i = index; i < value.Length; i++) {
            char c = value[i];
            int digit;
            if (c >= '0' && c <= '9') {
                digit = c - '0';
            } else if (c >= 'A' && c <= 'Z') {
                digit = c - 'A' + 10;
            } else if (c >= 'a' && c <= 'z') {
                digit = c - 'a' + 10;
            } else {
                throw new FormatException("Неизвестный символ");
            }
            if (digit >= radix) {
                throw new FormatException("Некорректная цифра для основания");
            }
            result *= integerRadix;
            result += new BetterBigInteger(new uint[] { (uint)digit });
        }
        BetterBigInteger res = new BetterBigInteger(result.GetDigits().ToArray(), negative);
        _signBit = res._signBit;
        _smallValue = res._smallValue;
        _data = res._data?.ToArray();
    }
    
    public ReadOnlySpan<uint> GetDigits()
    {
        return _data ?? [_smallValue];
    }
    
    public int CompareTo(IBigInteger? other)
    {
        if (other == null) {
            return 1;
        }
        BetterBigInteger integerOther = (BetterBigInteger)other;
        if (_signBit != integerOther._signBit) {
            if (_signBit == 1) {
                return -1;
            } else {
                return 1;
            }
        }
        int compare = PositiveCompare(this, integerOther);
        if (_signBit == 1) {
            return -compare;
        } else {
            return compare;
        }
    }

    public bool Equals(IBigInteger? other)
    {
        if (other == null) {
            return false;
        }
        if (CompareTo(other) == 0) {
            return true;
        } else {
            return false;
        }
    }

    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);

    public override int GetHashCode()
    {
        int hash = _signBit;
        foreach (uint digit in GetDigits()) {
            hash = hash * 31 + (int)digit;
        }
        return hash;
    }

    private static int PositiveCompare(BetterBigInteger a, BetterBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();
        if (digitsA.Length != digitsB.Length) {
            if (digitsA.Length < digitsB.Length) {
                return -1;
            } else {
                return 1;
            }
        }
        for (int i = digitsA.Length - 1; i >= 0; i--) {
            if (digitsA[i] < digitsB[i]) {
                return -1;
            }
            if (digitsA[i] > digitsB[i]) {
                return 1;
            }
        }
        return 0;
    }
    
    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b)
    {
        if (a._signBit == b._signBit) {
            uint[] sum = Add(a, b);
            return new BetterBigInteger(sum, a.IsNegative);
        }
        int compare = PositiveCompare(a, b);
        if (compare == 0) {
            return new BetterBigInteger(new uint[] { 0 });
        }
        if (compare > 0) {
            uint[] difA = Subtract(a, b);
            return new BetterBigInteger(difA, a.IsNegative);
        }
        uint[] difB = Subtract(b, a);
        return new BetterBigInteger(difB, b.IsNegative);
    }

    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b)
    {
        return a + (-b);
    }

    public static BetterBigInteger operator -(BetterBigInteger a)
    {
        if (a._smallValue == 0 && a._data == null) {
            return a;
        }
        return new BetterBigInteger(a.GetDigits().ToArray(), !a.IsNegative);
    }

    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b)
    {
        if (b._smallValue == 0 && b._data == null) {
            throw new DivideByZeroException();
        }
        BetterBigInteger remain;
        return DivWithRemain(a, b, out remain);
    }

    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b)
    {
        if (b._smallValue == 0 && b._data == null) {
            throw new DivideByZeroException();
        }
        BetterBigInteger remain;
        DivWithRemain(a, b, out remain);
        return remain;
    }

    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
    {
        if (a is null) {
            throw new ArgumentNullException(nameof(a), "Некорректный аргумент а");
        }
        if (b is null) {
            throw new ArgumentNullException(nameof(b), "Некорректный аргумент b");
        }
        int lengthA = a.GetDigits().Length;
        int lengthB = b.GetDigits().Length;
        int maxLength = Math.Max(lengthA, lengthB);
        IMultiplier multiplier;
        if (maxLength < 32) {
            multiplier = new SimpleMultiplier();
        } else if (maxLength < 256) {
            multiplier = new KaratsubaMultiplier();
        } else {
            multiplier = new FftMultiplier();
        }
        return multiplier.Multiply(a, b);
    }
    
    public static BetterBigInteger operator ~(BetterBigInteger a)
    {
        return (-a) - new BetterBigInteger(new uint[] { 1 });
    }

    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b)
    {
        return Operations(a, b, (x, y) => x & y);
    }

    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b)
    {
        return Operations(a, b, (x, y) => x | y);
    }

    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b)
    {
        return Operations(a, b, (x, y) => x ^ y);
    }

    private static BetterBigInteger Operations(BetterBigInteger a, BetterBigInteger b, Func<uint, uint, uint> operation)
    {
        int length = Math.Max(a.GetDigits().Length, b.GetDigits().Length);
        uint[] bitsA = ToTwosComplement(a, length + 1);
        uint[] bitsB = ToTwosComplement(b, length + 1);
        int maxLength = Math.Max(bitsA.Length, bitsB.Length);
        uint[] result = new uint[maxLength];
        for (int i = 0; i < maxLength; i++) {
            uint digitA;
            if (i < bitsA.Length) {
                digitA = bitsA[i];
            } else {
                if (a.IsNegative) {
                    digitA = uint.MaxValue;
                } else {
                    digitA = 0;
                }
            }
            uint digitB;
            if (i < bitsB.Length) {
                digitB = bitsB[i];
            } else {
                if (b.IsNegative) {
                    digitB = uint.MaxValue;
                } else {
                    digitB = 0;
                }
            }
            result[i] = operation(digitA, digitB);
        }
        return FromTwosComplement(result);
    }

    public static BetterBigInteger operator <<(BetterBigInteger a, int shift)
    {
        if (a._data == null && a._smallValue == 0) {
            return a;
        }
        var digits = a.GetDigits();
        int numberShift = shift / 32;
        int bitShift = shift % 32;
        uint[] result = new uint[digits.Length + numberShift + 1];
        ulong carry = 0;
        for (int i = 0; i < digits.Length; i++) {
            ulong currentDigit = (ulong)digits[i] << bitShift | carry;
            result[i + numberShift] = (uint)currentDigit;
            carry = currentDigit >> 32;
        }
        if (carry != 0) {
            result[digits.Length + numberShift] = (uint)carry;
        }
        return new BetterBigInteger(result, a.IsNegative);
    }

    public static BetterBigInteger operator >> (BetterBigInteger a, int shift)
    {
        if (a._data == null && a._smallValue == 0) {
            return a;
        }
        var digits = a.GetDigits();
        int numberShift = shift / 32;
        int bitShift = shift % 32;
        if (!a.IsNegative) {
            if (numberShift >= digits.Length) {
                return new BetterBigInteger(new uint[] { 0 });
            }
            uint[] positiveResult = new uint[digits.Length - numberShift];
            ulong carry = 0;
            for (int i = digits.Length - 1; i >= numberShift; i--) {
                ulong current = digits[i];
                ulong part = current >> bitShift;
                if (bitShift != 0) {
                    part |= carry << (32 - bitShift);
                    carry = current & ((1uL << bitShift) - 1);
                }
                positiveResult[i - numberShift] = (uint)part;
            }
            return new BetterBigInteger(positiveResult, false);
        }
        int length = Math.Max(a.GetDigits().Length, 1) + 1;
        uint[] bitsA = ToTwosComplement(a, length);
        uint[] result = new uint[length];
        for (int i = 0; i < length; i++) {
            int index = i + numberShift;
            uint currentNumber;
            if (index < length) {
                currentNumber = bitsA[index];
            } else {
                if (a.IsNegative) {
                    currentNumber = uint.MaxValue;
                } else {
                    currentNumber = 0;
                }
            }
            ulong shiftedNumber = currentNumber;
            if (bitShift != 0) {
                int nextIndex = index + 1;
                uint nextNumber;
                if (nextIndex < length) {
                    nextNumber = bitsA[nextIndex];
                } else {
                    if (a.IsNegative) {
                        nextNumber = uint.MaxValue;
                    } else {
                        nextNumber = 0;
                    }
                }
                shiftedNumber = (currentNumber >> bitShift) | ((ulong)nextNumber << (32 - bitShift));
            }
            result[i] = (uint)shiftedNumber;
        }
        return FromTwosComplement(result);
    }

    private static uint[] Add(BetterBigInteger a, BetterBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();
        int maxLength;
        if (digitsA.Length > digitsB.Length) {
            maxLength = digitsA.Length;
        } else {
            maxLength = digitsB.Length;
        }
        uint[] result = new uint[maxLength + 1];
        ulong carry = 0;
        for (int i = 0; i < maxLength; i++) {
            ulong digitA;
            if (i < digitsA.Length) {
                digitA = digitsA[i];
            } else {
                digitA = 0;
            }
            ulong digitB;
            if (i < digitsB.Length) {
                digitB = digitsB[i];
            } else {
                digitB = 0;
            }
            ulong currentDigit = digitA + digitB + carry;
            result[i] = (uint)currentDigit;
            carry = currentDigit >> 32;
        }
        if (carry != 0) {
            result[maxLength] = (uint)carry;
        }
        return result;
    }

    private static uint[] Subtract(BetterBigInteger a, BetterBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();
        uint[] result = new uint[digitsA.Length];
        long borrow = 0;
        for (int i = 0; i < digitsA.Length; i++) {
            long digitA = digitsA[i];
            long digitB;
            if (i < digitsB.Length) {
                digitB = digitsB[i];
            } else {
                digitB = 0;
            }
            long currentDigit = digitA - digitB - borrow;
            if (currentDigit < 0) {
                currentDigit = currentDigit + (1 << 32);
                borrow = 1;
            } else {
                borrow = 0;
            }
            result[i] = (uint)currentDigit;
        }
        return result;
    }

    private static BetterBigInteger DivWithRemain(BetterBigInteger a, BetterBigInteger b, out BetterBigInteger remain)
    {
        if (b._data == null && b._smallValue == 0) {
            throw new DivideByZeroException();
        }
        BetterBigInteger dividend = Abs(a);
        BetterBigInteger divisor = Abs(b);
        if (PositiveCompare(dividend, divisor) < 0) {
            remain = dividend;
            if (a.IsNegative != b.IsNegative) {
                return new BetterBigInteger(new uint[] { 0 });
            }
            return new BetterBigInteger(new uint[] { 0 });
        }
        if (dividend._data == null && divisor._data == null) {
            uint smallQuotient = dividend._smallValue / divisor._smallValue;
            uint smallRemain = dividend._smallValue % divisor._smallValue;
            remain = new BetterBigInteger(new uint[] { smallRemain });
            var quotient = new BetterBigInteger(new uint[] { smallQuotient }, a.IsNegative != b.IsNegative);
            if (a.IsNegative) {
                remain = -remain;
            }
            return quotient;
        }
        BetterBigInteger result = new BetterBigInteger(new uint[] { 0 });
        BetterBigInteger currentRemain = new BetterBigInteger(new uint[] { 0 });
        var dividendDigits = dividend.GetDigits();
        for (int i = dividendDigits.Length - 1; i >= 0; i--) {
            uint number = dividendDigits[i];
            for (int bit = 31; bit >= 0; bit--) {
                currentRemain <<= 1;
                if ((number & (1u << bit)) != 0) {
                    currentRemain += new BetterBigInteger(new uint[] { 1 });
                }
                result <<= 1;
                if (PositiveCompare(currentRemain, divisor) >= 0) {
                    currentRemain -= divisor;
                    result += new BetterBigInteger(new uint[] { 1 });
                }
            }
        }
        remain = currentRemain;
        if (a.IsNegative != b.IsNegative) {
            result = -result;
        }
        if (a.IsNegative) {
            remain = -remain;
        }
        return result;
    }

    private static BetterBigInteger Abs(BetterBigInteger v)
    {
        if (v.IsNegative) {
            return -v;
        }
        return v;
    }

    private static BetterBigInteger FromTwosComplement(uint[] array)
    {
        bool negative = (array.Length > 0 && (array[^1] & 0x80000000) != 0);
        if (!negative) {
            return new BetterBigInteger(array, false);
        }
        uint[] inverted = new uint[array.Length];
        for (int i = 0; i < array.Length; i++) {
            inverted[i] = ~array[i];
        }
        ulong carry = 1;
        for (int i = 0; i < inverted.Length; i++) {
            ulong currentNumber = (ulong)inverted[i] + carry;
            inverted[i] = (uint)currentNumber;
            carry = currentNumber >> 32;
        }
        return new BetterBigInteger(inverted, true);
    }

    private static uint[] ToTwosComplement(BetterBigInteger value, int minLength)
    {
        if (!value.IsNegative) {
            var digits = value.GetDigits();
            int length;
            if (digits.Length > minLength) {
                length = digits.Length;
            } else {
                length = minLength;
            }
            uint[] result = new uint[length];
            for (int i = 0; i < digits.Length; i++) {
                result[i] = digits[i];
            }
            return result;
        } else {
            var digits = value.GetDigits();
            int length;
            if (digits.Length > minLength) {
                length = digits.Length;
            } else {
                length = minLength;
            }
            length++;
            uint[] result = new uint[length];
            for (int i = 0; i < digits.Length; i++) {
                result[i] = ~digits[i];
            }
            for (int i = digits.Length; i < length; i++) {
                result[i] = uint.MaxValue;
            }
            ulong carry = 1;
            for (int i = 0; i < length; i++) {
                ulong sum = result[i] + carry;
                result[i] = (uint)sum;
                carry = sum >> 32;
                if (carry == 0) {
                    break;
                }
            }
            return result;
        }
    }
    
    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;
    
    public override string ToString() => ToString(10);
    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36) {
            throw new ArgumentException(nameof(radix), "Некорректное основание");
        }
        if (_data == null && _smallValue == 0) {
            return "0";
        }
        BetterBigInteger currentNumber = Abs(this);
        BetterBigInteger integerRadix = new BetterBigInteger(new uint[] { (uint)radix });
        List<char> digits = new List<char>();
        while (!(currentNumber._data == null && currentNumber._smallValue == 0)){
            BetterBigInteger remain;
            currentNumber = DivWithRemain(currentNumber, integerRadix, out remain);
            uint digit = remain.GetDigits()[0];
            char c;
            if (digit < 10) {
                c = (char)('0' + digit);
            } else {
                c = (char)('A' + (digit - 10));
            }
            digits.Add(c);
        }
        if (IsNegative) {
            digits.Add('-');
        }
        digits.Reverse();
        return new string(digits.ToArray());
    }
    
}