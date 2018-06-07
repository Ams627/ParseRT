using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ParseRT
{
    class RcsFtot
    {
        string ccstFormat;
        string ticketType;
        bool valid = false;
        public RcsFtot(string ticketType, IEnumerable<string> channelList, IEnumerable<string> ccstFormat)
        {
            if (!Regex.Match(ticketType, "[A-Z][0-9]{3}").Success)
            {
                throw new ElementException("Bad ticket type");
            }
        }
        public (string ftot, string ccstFormat, bool valid) CcstMapping => (ticketType, ccstFormat, valid);
        public bool Valid => true;
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var files = Directory.GetFiles(@"s:\", "RCS_R_T*.xml").OrderBy(x => x.Substring(8, 6).Reverse().ToString()).ToList();
                var filename = files.First();
                var doc = XDocument.Load(filename);

                var result = doc.Descendants("FTOT").Select(x =>
                {
                    RcsFtot mapping = null;
                    try
                    {
                        mapping = new RcsFtot
                        (
                            ticketType: x.Attribute("t")?.Value,
                            channelList: x.Descendants("Channel")?.Select(y => y.Attribute("ch")?.Value),
                            ccstFormat: x.Descendants("CCSTFormat")?.Select(y => y.Attribute("f")?.Value)
                        );
                    }
                    catch (ElementException ex)
                    {
                        Console.WriteLine($"Error in element: {ex.Message}");
                    }
                    return mapping;
                }).Where(y => y.Valid).ToLookup(z => z.CcstMapping.ftot);
                var dups = result.Where(x => x.Count() > 1);
                if (dups.Any())
                {
                    Console.WriteLine($"Duplicate ticket type found: {}");
                }
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
