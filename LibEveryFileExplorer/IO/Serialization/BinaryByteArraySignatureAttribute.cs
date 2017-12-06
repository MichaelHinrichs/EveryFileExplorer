using System;

namespace LibEveryFileExplorer.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class BinaryByteArraySignatureAttribute : BinaryAttribute
	{
		readonly byte[] Signature;

		public BinaryByteArraySignatureAttribute(params byte[] Signature)
		{
			this.Signature = Signature;
		}

		public override object Value
		{
			get { return Signature; }
		}
	}
}
