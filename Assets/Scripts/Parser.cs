using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Class for parsing data
/// </summary>
public class Parser {

    /// <summary>
    /// Method for reading csv-file and parsing each line to a list of strings.
    /// csv format: id;from;to;anything after this
    /// </summary>
    /// <param name="path">Path to the parsed csv-file. Path can be relative or absolute.</param>
    /// <returns>Collection of lists. Single list contains info about single mail.</returns>
    public static IEnumerable<List<string>> ReadCsv(string path)
    {
        List<string> csvLine = new List<string>();
        string mailFrom = "";
        List<string> mailsTo = new List<string>();

        using (var fileStream = File.OpenRead(path))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 1024))
        {
            // One loop per line from csv
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] values = line.Split(';');
                mailFrom = values[1]; // Skip index 0 since that info (id) isn't needed
                mailsTo = values[2].Split(',').ToList();
                csvLine.Clear(); // Clear previous line from the list before adding current line
                csvLine.Add(mailFrom);
                csvLine = csvLine.Concat(mailsTo).ToList();
                yield return csvLine;
            }
        }
    }
}
