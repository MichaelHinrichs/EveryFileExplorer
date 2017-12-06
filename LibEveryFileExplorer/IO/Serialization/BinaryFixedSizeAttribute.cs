using System;

namespace LibEveryFileExplorer.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class BinaryFixedSizeAttribute : BinaryAttribute
	{
		readonly int Length;

		public BinaryFixedSizeAttribute(int Length)
		{
			if (Length <= 0) throw new ArgumentException("Length must be greater than 0!");
			this.Length = Length;
		}

		public override object Value
		{
			get { return Length; }
		}
	}
}
