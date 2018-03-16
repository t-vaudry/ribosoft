using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml;
using System.Text.RegularExpressions;

namespace Ribosoft.GenbankRequests
{

    public class GenbankRequest
    {

        private static readonly HttpClient client = new HttpClient();

        private static String eUtilsHostString = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi";
        private static String genbankHostString = "https://www.ncbi.nlm.nih.gov/sviewer/viewer.fcgi";

        static public string RunSequenceRequest(string accession)
        {
            {
                try
                {
                    string id = getIDAsync(accession).Result;
                    var rawSequence = getSequenceAsync(id).Result;
                    return rawSequence;
                }
                catch (System.Exception exc)
                {
                    throw new GenbankRequestsException("An error occurred while running Genbank requests: Sequence", exc);
                }
            }
        }

        static public string RunStartIndexRequest(string accession)
        {
            {
                try
                {
                    string id = getIDAsync(accession).Result;
                    var result = getORFStartIndexAsync(id).Result;
                    return result;
                }
                catch (System.Exception exc)
                {
                    throw new GenbankRequestsException("An error occurred while running Genbank requests: StartIndex", exc);
                }
            }
        }

        static public string RunEndIndexRequest(string accession)
        {
            {
                try
                {
                    string id = getIDAsync(accession).Result;
                    var result = getORFEndIndexAsync(id).Result;
                    return result;
                }
                catch (System.Exception exc)
                {
                    throw new GenbankRequestsException("An error occurred while running Genbank requests: EndIndex", exc);
                }
            }
        }

        static public async Task<string> getIDAsync(String accession)
        {
            // example: https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=nuccore&term=M73077
            var databaseString = "db=nuccore";
            var searchtermString = "term=" + accession;
            var requestURL = string.Concat(eUtilsHostString, "?", databaseString, "&", searchtermString);

            var result = await client.GetAsync(requestURL);
            result.EnsureSuccessStatusCode();

            var responseString = await result.Content.ReadAsStringAsync();
            var xmlIndex = responseString.IndexOf("<eSearchResult>");
            var xmlString = responseString.Substring(xmlIndex);

            XmlDocument doc = new XmlDocument();
            string id = "";
            try
            {
                doc.LoadXml(xmlString);
                XmlNode node = doc.DocumentElement.SelectSingleNode("/eSearchResult/IdList/Id");
                id = node.InnerText;
            }
            catch (System.Exception exc)
            {
                throw new GenbankRequestsException("An error occurred while parsing EUtils XML", exc);
            }

            return id;

        }

        static public async Task<string> getSequenceAsync(String id)
        {
            // example: https://www.ncbi.nlm.nih.gov/sviewer/viewer.fcgi?id=183617&db=nuccore&report=genbank&fmt_mask=0&maxdownloadsize=1000000&rettype=fasta&retmode=text
            string idString = "id=" + id;
            string databaseString = "db=nuccore";
            string reportString = "report=genbank";
            string fmtmaskString = "fmt_mask=0";
            string maxdownloadString = "maxdownloadsize=1000000";
            string rettypeFastaString = "rettype=fasta";
            string retmodeString = "retmode=text";
            var fastaRequestURL = string.Concat(genbankHostString, "?", idString, "&", databaseString, "&", reportString, "&", fmtmaskString, "&", maxdownloadString, "&", rettypeFastaString, "&", retmodeString);

            var fastaResult = await client.GetAsync(fastaRequestURL);
            fastaResult.EnsureSuccessStatusCode();
            var fastaResponseString = await fastaResult.Content.ReadAsStringAsync();

            var sequenceIndex = fastaResponseString.IndexOf("\n") + 1;
            if (sequenceIndex == -1)
            {
                throw new GenbankRequestsException("Genbank response was empty");
            }
            var sequenceString = fastaResponseString.Substring(sequenceIndex);
            sequenceString = sequenceString.Replace("\n", "");
            sequenceString = sequenceString.Replace("T", "U");

            var match = Regex.Match(sequenceString, @"[^ACGU]");
            if (match.Success)
            {
                throw new GenbankRequestsException(String.Format("A non-base character {0} was found in the sequence retrieved", match.Value));
            }

            return sequenceString;
        }

        static public async Task<string> getORFStartIndexAsync(String id)
        {
            string idString = "id=" + id;
            string databaseString = "db=nuccore";
            string reportString = "report=genbank";
            string fmtmaskString = "fmt_mask=0";
            string maxdownloadString = "maxdownloadsize=1000000";
            string rettypeXMLString = "rettype=xml";
            string retmodeString = "retmode=text";
            var xmlRequestURL = string.Concat(genbankHostString, "?", idString, "&", databaseString, "&", reportString, "&", fmtmaskString, "&", maxdownloadString, "&", rettypeXMLString, "&", retmodeString);

            var xmlResult = await client.GetAsync(xmlRequestURL);
            xmlResult.EnsureSuccessStatusCode();
            var xmlResponseString = await xmlResult.Content.ReadAsStringAsync();

            var xmlIndex = xmlResponseString.IndexOf("<Seq-entry>");
            var xmlString = xmlResponseString.Substring(xmlIndex);

            XmlDocument doc = new XmlDocument();
            string startIndex;
            try
            {
                doc.LoadXml(xmlString);
                XmlNode node = doc.DocumentElement.SelectSingleNode("/Seq-entry/Seq-entry_set/Bioseq-set/Bioseq-set_annot/Seq-annot/Seq-annot_data/Seq-annot_data_ftable/Seq-feat/Seq-feat_location/Seq-loc/Seq-loc_int/Seq-interval/Seq-interval_from");
                startIndex = node.InnerText;
            }
            catch (System.Exception exc)
            {
                throw new GenbankRequestsException("An error occurred while parsing GenBank XML", exc);
            }

            return startIndex;
        }

        static public async Task<string> getORFEndIndexAsync(String id)
        {
            string idString = "id=" + id;
            string databaseString = "db=nuccore";
            string reportString = "report=genbank";
            string fmtmaskString = "fmt_mask=0";
            string maxdownloadString = "maxdownloadsize=1000000";
            string rettypeXMLString = "rettype=xml";
            string retmodeString = "retmode=text";
            var xmlRequestURL = string.Concat(genbankHostString, "?", idString, "&", databaseString, "&", reportString, "&", fmtmaskString, "&", maxdownloadString, "&", rettypeXMLString, "&", retmodeString);

            var xmlResult = await client.GetAsync(xmlRequestURL);
            xmlResult.EnsureSuccessStatusCode();
            var xmlResponseString = await xmlResult.Content.ReadAsStringAsync();

            var xmlIndex = xmlResponseString.IndexOf("<Seq-entry>");
            var xmlString = xmlResponseString.Substring(xmlIndex);

            XmlDocument doc = new XmlDocument();
            string endIndex;
            try
            {
                doc.LoadXml(xmlString);
                XmlNode node = doc.DocumentElement.SelectSingleNode("/Seq-entry/Seq-entry_set/Bioseq-set/Bioseq-set_annot/Seq-annot/Seq-annot_data/Seq-annot_data_ftable/Seq-feat/Seq-feat_location/Seq-loc/Seq-loc_int/Seq-interval/Seq-interval_to");
                endIndex = node.InnerText;
            }
            catch (System.Exception exc)
            {
                throw new GenbankRequestsException("An error occurred while parsing GenBank XML", exc);
            }

            return endIndex;
        }
    }
}
