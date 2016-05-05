using System;
using System.Collections.Generic;

	public class IntPair : IEquatable<IntPair>
	{
		public int firstDigit;
		public int secondDigit;

		public IntPair (int x, int y)
		{
			if (x < y) {

				firstDigit = x;
				secondDigit = y;

			} else {

				firstDigit = y;
				secondDigit = x;

			}

		}

		public bool compareDigits(IntPair other){
			if (firstDigit == other.firstDigit && secondDigit == other.secondDigit) {
				return true;
			}
			return false;
		}

		public override bool Equals (Object obj)
		{
			if (obj == null) {
				return false;
			}

			IntPair pair = obj as IntPair;
			if (pair == null) {
				return false;
			} else {
				return Equals (pair);
			}
		}



		public bool Equals(IntPair other)
		{
			if (other == null) {
				return false;
			}

			return compareDigits (other);
		}



		public override int GetHashCode()
		{
			return 17 * this.firstDigit.GetHashCode () + 13 * this.secondDigit.GetHashCode ();
		}

		public static bool operator == (IntPair a, IntPair b)
		{
			if (((object)a) == null || ((object)b) == null) {
				return Object.Equals (a, b);
			}
			return a.Equals (b);
		}

		public static bool operator != (IntPair a, IntPair b)
		{
			return !(a == b);
		}



}

