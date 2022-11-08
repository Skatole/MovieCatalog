using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MovieCatalog
{
    public class Database
    {
        public IReadOnlyList<Movie> Movies { get; }
        public IReadOnlyDictionary<int, string> Genres { get; }
        public IReadOnlyDictionary<string, string> Countries { get; }

        public Database(Stream zipCsvStream)
        {
            using var zip = new ZipArchive(zipCsvStream, ZipArchiveMode.Read);
            using var csvFile = zip.Entries.Single().Open();
            using var reader = new StreamReader(csvFile);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            csvReader.Context.RegisterClassMap<Movie.MovieMap>();
            Movies = csvReader.GetRecords<Movie>().ToList();
            Genres = Movies.SelectMany(m => m.Genres).Distinct().ToDictionary(g => g.Key, g => g.Value);
            Countries = Movies.SelectMany(m => m.ProductionCountries).Distinct().ToDictionary(c => c.Key, c => c.Value);
        }
    }
}
