﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.GameData;
using System.IO;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Collections;
using System.Windows.Forms;
using LibEveryFileExplorer;
using System.ComponentModel;
using LibEveryFileExplorer.ComponentModel;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace MarioKart.MKDS.NKM
{
	public class KTPS : GameDataSection<KTPS.KTPSEntry>
	{
		public KTPS() { Signature = "KTPS"; }
		public KTPS(EndianBinaryReaderEx er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "KTPS") throw new SignatureNotCorrectException(Signature, "KTPS", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new KTPSEntry(er));
		}

		public void Write(EndianBinaryWriter er)
		{
			er.Write(Signature, Encoding.ASCII, false);
			NrEntries = (uint)Entries.Count;
			er.Write(NrEntries);
			for (int i = 0; i < NrEntries; i++) Entries[i].Write(er);
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"?",
					"Index"
				};
		}

		public class KTPSEntry : GameDataSectionEntry
		{
			public KTPSEntry()
			{
				Position = new Vector3(0, 0, 0);
				Rotation = new Vector3(0, 0, 0);
				Unknown = 0xFFFF;
				Index = -1;
			}
			public KTPSEntry(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}

			public override void Write(EndianBinaryWriter er)
			{
				er.WriteVecFx32(Position);
				er.WriteVecFx32(Rotation);
				er.Write(Unknown);
				er.Write(Index);
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString("#####0.############"));
				m.SubItems.Add(Position.Y.ToString("#####0.############"));
				m.SubItems.Add(Position.Z.ToString("#####0.############"));

				m.SubItems.Add(Rotation.X.ToString("#####0.############"));
				m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
				m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown));
				m.SubItems.Add(Index.ToString());
				return m;
			}
			[Category("Transformation")]
			[BinaryFixedPoint(true, 19, 12)]
			public Vector3 Position { get; set; }
			[Category("Transformation")]
			[BinaryFixedPoint(true, 19, 12)]
			public Vector3 Rotation { get; set; }
			[TypeConverter(typeof(HexTypeConverter)), HexReversedAttribute]
			public UInt16 Unknown { get; set; }
			public Int16 Index { get; set; }
		}
	}
}
