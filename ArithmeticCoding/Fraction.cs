using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Xml;

namespace ArithmeticCoding
{
    public class Fraction : IComparable<Fraction>
    {
        public const int ContinuedFractionMaxLength = 500;

        public BigInteger WholePart { get; set; }
        public BigInteger Numerator { get; set; }
        public BigInteger Denominator { get; set; }

        public string ContinuedFraction
        {
            get
            {
                if (Denominator == 1) return $"[{WholePart + Numerator}]";
                var sb = new StringBuilder();

                var list = new List<KeyValuePair<BigInteger, BigInteger>>();
                var result = new List<BigInteger>();

                var whole = Numerator / Denominator;
                var mod = Numerator % Denominator;
                var denominator = Denominator;
                list.Add(new KeyValuePair<BigInteger, BigInteger>(Numerator, Denominator));
                sb.Append($"[{whole + WholePart}");

                for (int i = 0; i < ContinuedFractionMaxLength && mod > 0; i++)
                {
                    whole = denominator / mod;
                    var temp = denominator % mod;
                    denominator = mod;
                    mod = temp;
                    result.Add(whole);
                    if (!list.Any(x => x.Key.CompareTo(denominator) == 0 && x.Value.CompareTo(mod) == 0))
                        list.Add(new KeyValuePair<BigInteger, BigInteger>(denominator, mod));
                    else
                    {
                        var first = list.FindIndex(x => x.Key.CompareTo(denominator) == 0 && x.Value.CompareTo(mod) == 0);
                        result.Insert(first, -1);
                        break;
                    }
                }

                bool flag = false;
                for (int i = 0; i < result.Count; i++)
                {
                    if (result[i] == -1 && i + 1 < result.Count)
                    {
                        flag = true;
                        sb.Append($", ({result[i + 1]}");
                        i++;
                    }
                    else sb.Append(", " + result[i]);
                }
                sb.Append($"{(flag ? ")" : "")}]");
                return sb.ToString();
            }
        }

        public string Decimal
        {
            get
            {
                if (Denominator == 1) return $"{WholePart + Numerator}";

                var sb = new StringBuilder();
                var whole = Numerator / Denominator;
                var mod = Numerator % Denominator;
                sb.Append($"{whole + WholePart}.");

                for (int i = 0; i < Denominator.ToString().Length * 2 && mod > 0; i++)
                {
                    mod *= 10;
                    whole = mod / Denominator;
                    mod = mod % Denominator;
                    sb.Append(whole);
                }

                return sb.ToString();
            }
        }

        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0) throw new DivideByZeroException();
            WholePart = numerator / denominator;
            numerator = numerator % denominator;

            BigInteger divisor = GCD(numerator, denominator);
            Numerator = numerator / divisor;
            Denominator = denominator / divisor;
        }

        public Fraction(BigInteger wholePart)
        {
            WholePart = wholePart;
            Numerator = 0;
            Denominator = 1;
        }

        #region Operators overload

        public static Fraction operator +(Fraction a, Fraction b)
            => new Fraction((a.Numerator + a.WholePart * a.Denominator) * b.Denominator + (b.Numerator + b.WholePart * b.Denominator) * a.Denominator , a.Denominator * b.Denominator);

        public static Fraction operator *(Fraction a, Fraction b)
            => new Fraction((a.Numerator + a.WholePart * a.Denominator) * (b.Numerator + b.WholePart * b.Denominator), a.Denominator * b.Denominator);

        public static Fraction operator -(Fraction a, Fraction b)
            => new Fraction((a.Numerator + a.WholePart * a.Denominator) * b.Denominator - (b.Numerator + b.WholePart * b.Denominator) * a.Denominator, a.Denominator * b.Denominator);

        public static Fraction operator /(Fraction a, Fraction b)
            => new Fraction((a.Numerator + a.WholePart * a.Denominator) * b.Denominator, a.Denominator * (b.Numerator + b.WholePart * b.Denominator));

        #endregion

        public static Fraction ParseDecimal(string text)
        {
            var split = text.Split('.');
            if (split.Length == 1) return new Fraction(BigInteger.Parse(split[0]));
            BigInteger num = 0;
            BigInteger den = 1;
            foreach (var ch in split[1])
            {
                num = BigInteger.Parse(ch.ToString()) + num * 10;
                den *= 10;
            }

            return new Fraction(num, den);
        }

        public override string ToString()
        {
            if (Denominator == 1) return $"{WholePart + Numerator}";
            //return $"{(WholePart > 0? $"{WholePart} + ({Numerator}/{Denominator})":$"{Numerator}/{Denominator}")}";
            return Decimal.ToString();
        }

        private BigInteger GCD(BigInteger a, BigInteger b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }

        public int CompareTo(Fraction other)
        {
            return ((Numerator + WholePart * Denominator) * other.Denominator).CompareTo((other.Numerator + other.WholePart * other.Denominator) * Denominator);
        } 
    }
}