using System;
using System.Net.NetworkInformation;
using System.Numerics;

namespace ArithmeticCoding
{
    public class Fraction : IComparable<Fraction>
    {
        public BigInteger Numerator { get; set; }
        public BigInteger Denominator { get; set; }
        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0) throw new DivideByZeroException();
            BigInteger divisor = GCD(numerator, denominator);
            Numerator = numerator / divisor;
            Denominator = denominator / divisor;
        }

        public Fraction(BigInteger numerator)
        {
            Numerator = numerator;
            Denominator = 1;
        }

        #region Operators overload

        public static Fraction operator +(Fraction a, Fraction b)
            => new Fraction(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);

        public static Fraction operator *(Fraction a, Fraction b)
            => new Fraction(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

        public static Fraction operator -(Fraction a, Fraction b)
            => new Fraction(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);

        public static Fraction operator /(Fraction a, Fraction b)
            => new Fraction(a.Numerator * b.Denominator, a.Denominator * b.Numerator);

        #endregion

        public static Fraction Parse(string text)
        {
            var split = text.Split('/');
            if (split.Length == 1) return new Fraction(BigInteger.Parse(split[0]));
            return new Fraction(BigInteger.Parse(split[0]), BigInteger.Parse(split[1]));
        }

        public override string ToString()
        {
            if (Denominator == 1) return Numerator.ToString();
            return $"{Numerator}/{Denominator}";
        }

        private BigInteger GCD(BigInteger a, BigInteger b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }

        public int CompareTo(Fraction other)
        {
            return (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
        }
    }
}