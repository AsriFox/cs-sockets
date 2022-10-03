using System;
using System.Xml.Serialization;
using System.IO;

namespace CsSockets
{
	[XmlRoot("SettingsTable", IsNullable=false)]
	public class SettingsTable
	{
		public string Host;
		public int Port;

		public static void Write(SettingsTable table, string filename)
		{
			XmlSerializer serializer = new(typeof(SettingsTable));
			StreamWriter writer = new(filename);
			serializer.Serialize(writer, table);
			writer.Close();
		}

		public void Write(string filename) => Write(this, filename);

		public static SettingsTable Read(string filename)
		{
			XmlSerializer serializer = new(typeof(SettingsTable));
			serializer.UnknownNode += 
				(_, e) => Console.WriteLine($"Unknown node: {e.Name}\t{e.Text}");
			serializer.UnknownAttribute += 
				(_, e) => Console.Write($"Unknown attribute: {e.Attr.Name}='{e.Attr.Value}'");
			try {
				FileStream fs = new(filename, FileMode.Open);
				var table = serializer.Deserialize(fs) as SettingsTable;
				fs.Close();
				return table;
			}
			catch (IOException) {
				return null;
			}
		}
	}
}
