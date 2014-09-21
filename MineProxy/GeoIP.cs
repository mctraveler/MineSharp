using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Reflection;

namespace MineProxy
{
    public static class GeoIP
    {
        static readonly Range[] ranges;

        public static string Lookup(IPAddress address)
        {
            if (address == null)
                return "null";
            byte[] b = address.GetAddressBytes();
            if (b.Length != 4)
                return "short";
            uint ip = ((uint)b [0] << 24) + ((uint)b [1] << 16) + ((uint)b [2] << 8) + (uint)b [3];

            //Binary search
            int max = ranges.Length - 1;
            int min = 0;
            while (min <= max)
            {
                int mid = (max + min) / 2;
                Range r = ranges [mid];
                if (ip < r.Start)
                {
                    max = mid - 1;
                    continue;
                }
                if (ip > r.End)
                {
                    min = mid + 1;
                    continue;
                }

                return r.Country;
            }

            //Chat.TellNuxas("Unknown geoip: ", address + " out of " + ranges.Length);
            return "Earth";
        }

        static GeoIP()
        {
            try
            {
                uint prev = 0;

                string path = "GeoIPCountryWhois.csv";
                if (File.Exists(path) == false)
                {
                    Log.WriteServer("Not found: " + path);
                    return;
                }

                List<Range> list = new List<Range>();
                foreach (string line in File.ReadAllLines(path))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length < 6)
                        continue;
                    Range r = new Range();
                    r.Start = uint.Parse(parts [2].Trim('"'));
                    r.End = uint.Parse(parts [3].Trim('"'));
                    r.Country = parts [5].Trim('"');
                    list.Add(r);
                    if (prev > r.Start)
                        throw new InvalidOperationException("Not sorted geoip, " + r.Start + " < " + prev);
                }

                ranges = list.ToArray();

            } catch (Exception e)
            {
                Log.WriteServer(e);
            } finally
            {
                if (ranges == null)
                    ranges = new Range[0];
            }
        }

        class Range
        {
            public uint Start { get; set; }

            public uint End { get; set; }

            public string Country { get; set; }
        }
    }
}

