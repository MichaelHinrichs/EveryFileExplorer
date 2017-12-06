using System;

namespace LibEveryFileExplorer.IO.Serialization
{
    public abstract class BinaryAttribute : Attribute
	{
		public abstract Object Value { get; }
	}
}
