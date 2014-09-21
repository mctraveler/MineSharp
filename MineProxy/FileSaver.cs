using System;
using System.IO;

namespace MineProxy
{
	public static class FileSaver
	{
		/// <summary>
		/// Write all data into a file.
		/// Guarantee that the file will only exist with all data
		/// </summary>
		public static void WriteAllBytes (string path, byte[] data)
		{
			try {
				Random r = new Random ();
				string tmp = path + r.Next ();
				File.WriteAllBytes (tmp, data);
				if (File.Exists (path))
					File.Delete (path);
				File.Move (tmp, path);
			} catch (IOException e) {
				Console.WriteLine (e);
			}
		}
	}
}

