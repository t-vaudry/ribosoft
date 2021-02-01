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
    /*! \class GenbankRequest
     * \brief Object class for the GenBank requests made on the request page
     */
    public static class GenbankRequest
    {
        /*! \property client
         * \brief HTTP Client
         */
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        /*! \property eUtilsHostString
         * \brief URL to NCBI esearch
         */
        private static string eUtilsHostString = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi";

        /*! \property genbankHostString
         * \brief URL to NCBI GenBank viewer
         */
        private static string genbankHostString = "https://www.ncbi.nlm.nih.gov/sviewer/viewer.fcgi";

        /*! \fn RunSequenceRequest
         * \brief HTTP GET request to retrieve the sequence information for the accession number provided
         * \param accession Accession number
         * \return Genbank result with sequence information
         */
        public static async Task<GenbankResult> RunSequenceRequest(string accession)
        {
            var genbankResult = new GenbankResult();

            try
            {
                genbankResult.AccessionId = await getIDAsync(accession);
                genbankResult.Sequence = await getSequenceAsync(genbankResult.AccessionId);
                var orf = await getORFAsync(genbankResult.AccessionId);
                genbankResult.OpenReadingFrameStart = orf[0];
                genbankResult.OpenReadingFrameEnd = orf[1];
            }
            catch (GenbankRequestsException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new GenbankRequestsException("An error occurred with the GenBank request.", e);
            }

            return genbankResult;
        }

        /*! \fn getIDAsync
         * \brief Helper function to extract the ID from the esearch result
         * \param accession Accession number
         * \return ID
         */
        private static async Task<string> getIDAsync(String accession)
        {
            // example: https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=nuccore&term=M73077[accn]
            var databaseString = "db=nuccore";
            var searchtermString = "term=" + accession + "[accn]";
            var requestURL = string.Concat(eUtilsHostString, "?", databaseString, "&", searchtermString);

            var result = await client.GetAsync(requestURL);
            result.EnsureSuccessStatusCode();

            var responseString = await result.Content.ReadAsStringAsync();
            var xmlIndex = responseString.IndexOf("<eSearchResult>");
            var xmlString = responseString.Substring(xmlIndex);

            XmlDocument doc = new XmlDocument();
            string id;
            
            try
            {
                doc.LoadXml(xmlString);
                XmlNode node = doc.DocumentElement.SelectSingleNode("/eSearchResult/IdList/Id");
                id = node.InnerText;
            }
            catch (Exception exc)
            {
                throw new GenbankRequestsException("The accession ID does not exist.", exc);
            }

            return id;
        }

        /*! \fn getSequenceAsync
         * \brief Helper function to retrieve sequence
         * \param id ID
         * \return RNA sequence
         */
        private static async Task<string> getSequenceAsync(String id)
        {
            // example: https://www.ncbi.nlm.nih.gov/sviewer/viewer.fcgi?id=183617&db=nuccore&report=genbank&fmt_mask=0&maxdownloadsize=1000000&rettype=fasta&retmode=text
            string idString = "id=" + id;
            string databaseString = "db=nuccore";
            string reportString = "report=genbank";
            string fmtmaskString = "fmt_mask=0";
            string maxdownloadString = "maxdownloadsize=1000000";
            string rettypeFastaString = "rettype=fasta";
            string retmodeString = "retmode=text";
            var fastaRequestURL = string.Concat(genbankHostString, "?", idString, "&", databaseString, "&", reportString, "&", fmtmaskString, "&",
                maxdownloadString, "&", rettypeFastaString, "&", retmodeString);

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
                throw new GenbankRequestsException(String.Format("A non-base character {0} was found in the sequence retrieved.", match.Value));
            }

            return sequenceString;
        }

        /*! \fn getORFAsync
         * \brief Helper function to retrieve open reading frame values
         * \param id ID
         * \return Open reading frame information
         */
        private static async Task<int[]> getORFAsync(string id)
        {
            string idString = "id=" + id;
            string databaseString = "db=nuccore";
            string reportString = "report=genbank";
            string fmtmaskString = "fmt_mask=0";
            string maxdownloadString = "maxdownloadsize=1000000";
            string rettypeXMLString = "rettype=xml";
            string retmodeString = "retmode=text";
            var xmlRequestUrl = string.Concat(genbankHostString, "?", idString, "&", databaseString, "&", reportString, "&", fmtmaskString, "&",
                maxdownloadString, "&", rettypeXMLString, "&", retmodeString);

            var xmlResult = await client.GetAsync(xmlRequestUrl);
            xmlResult.EnsureSuccessStatusCode();
            var xmlResponseString = await xmlResult.Content.ReadAsStringAsync();

            var xmlIndex = xmlResponseString.IndexOf("<Seq-entry>", StringComparison.Ordinal);
            var xmlString = xmlResponseString.Substring(xmlIndex);

            XmlDocument doc = new XmlDocument();
            int startIndex;
            int endIndex;

            try
            {
                doc.LoadXml(xmlString);

                // start index
                XmlNode node = doc.DocumentElement.SelectSingleNode(
                    "/Seq-entry/Seq-entry_set/Bioseq-set/Bioseq-set_annot/Seq-annot/Seq-annot_data/Seq-annot_data_ftable/Seq-feat/Seq-feat_location/Seq-loc/Seq-loc_int/Seq-interval/Seq-interval_from");
                startIndex = int.Parse(node.InnerText);

                // end index
                node = doc.DocumentElement.SelectSingleNode(
                    "/Seq-entry/Seq-entry_set/Bioseq-set/Bioseq-set_annot/Seq-annot/Seq-annot_data/Seq-annot_data_ftable/Seq-feat/Seq-feat_location/Seq-loc/Seq-loc_int/Seq-interval/Seq-interval_to");
                endIndex = int.Parse(node.InnerText);
            }
            catch (Exception exc)
            {
                throw new GenbankRequestsException("An error occurred while parsing the GenBank response.", exc);
            }

            return new[] {startIndex, endIndex};
        }
    }
}