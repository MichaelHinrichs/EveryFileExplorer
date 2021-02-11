using System;

namespace LibEveryFileExplorer.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class BinaryBOMAttribute : BinaryAttribute
	{
		readonly uint LittleEndianMark;

		public BinaryBOMAttribute(uint LittleEndianMark)
		{
			this.LittleEndianMark = LittleEndianMark;
		}

		public override object Value
		{
			get { return LittleEndianMark; }
		}
	}
}
