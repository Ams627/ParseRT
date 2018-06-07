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
        public RcsFtot(string ticketType, IEnumerable<string> channelList, IEnumerable<(string ccstFormat, string endDate, string startDate)> ccstFormats)
        {
            if (!Regex.Match(ticketType, "[A-Z0-9]{3}").Success)
            {
                throw new ElementException("Bad ticket type");
            }
            if (ccstFormat.Count() > 1)
            {
                throw new ElementException($"More than one CCST-x format for ticket type {ticketType}");
            }
            foreach (var channel in channelList)
            {
                if (channel == "00004")
                {
                    valid = true;
                }
            }
            if (valid)
            {
                this.ticketType = ticketType;
                this.ccstFormat = ccstFormat.First();
            }
        }
        public (string ftot, string ccstFormat, bool valid) CcstMapping => (ticketType, ccstFormat, valid);
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var files = Directory.GetFiles(@"s:\", "RCS_R_T*.xml").OrderBy(x => x.Substring(8, 6).Reverse().ToString()).ToList();
                var filename = files.Any() ?  files.First() : throw new Exception("No RCS_R_T* files found");
                var doc = XDocument.Load(filename);
                var rootNamespace = doc.Root.GetDefaultNamespace();


                var res = from ftotElement in doc.Descendants(rootNamespace + "FTOT")
                          let ftot = ftotElement?.Attribute("t")?.Value
                          let ccst = (format: ftotElement.Elements
                          let filterLtypes = from licenseeTypeElement in ftotElement.Elements("LicenseeType")
                                             let lt = licenseeTypeElement.Attribute("lt")?.Value
                                             where lt == "00001"
                                                 from channel in licenseeTypeElement.Descendants("Channel")
                                                  let ch = channel.Attribute("ch")?.Value
                                                  where ch == "00004"
                                             select new { ftot,  };






var result = doc.Descendants(rootNamespace + "FTOT").Select(x =>
                {
                    RcsFtot mapping = null;
                    try
                    {
                        mapping = new RcsFtot
                        (
                            ticketType: x.Attribute("t")?.Value,
                            channelList: x.Descendants(rootNamespace + "Channel")?.Select(y => y.Attribute("ch")?.Value),
                            ccstFormats: x.Descendants(rootNamespace + "Traditional")?.Select(y =>
                            (
                                ccstFormat: y.Elements("CCSTFormat").Select(z=>z.Attribute("f").Value).SingleOrDefault() ?? throw new ElementException("More than one CCST element defined"),
                                startDate: y.Attribute("f")?.Value,
                                endDate: y.Attribute("u")?.Value
                            ))
                        );
                    }
            
                    catch (ElementException ex)
                    {
                        Console.WriteLine($"Error in element: {ex.Message}");
                    }
                    return mapping;
                }).Where(y => y.CcstMapping.valid).ToLookup(z => z.CcstMapping.ftot);
                var dups = result.Where(x => x.Count() > 1).ToList();
                dups.ForEach(x => Console.WriteLine($"Duplicate ticket type found: {x.Key}"));

                foreach (var mapping in result)
                {
                    var ccstMapping = mapping.First().CcstMapping;
                    Console.WriteLine($"{ccstMapping.ftot} is {ccstMapping.ccstFormat}");
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
