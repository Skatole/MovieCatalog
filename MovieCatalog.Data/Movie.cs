using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MovieCatalog
{
    public record Movie
    {
        private long? budget;
        [Name("budget")]
        public long? Budget { get => budget; set => budget = value != 0 ? value : null; }

        public IReadOnlyDictionary<int, string> Genres { get; set; } = null!;

        [Name("homepage")]
        public string? Homepage { get; set; }

        [Name("id")]
        public int Id { get; set; }

        [Name("imdb_id")]
        public string? ImdbId { get; set; }

        [Name("original_language")]
        public string? OriginalLanguage { get; set; }

        [Name("original_title")]
        public string OriginalTitle { get; set; } = null!;

        [Name("overview")]
        public string? Overview { get; set; }

        [Name("popularity")]
        public float Popularity { get; set; }

        public IReadOnlyDictionary<string, string> ProductionCountries { get; set; } = null!;

        [Name("release_date")]
        public DateTime? ReleaseDate { get; set; }

        private long? revenue;
        [Name("revenue")]
        public long? Revenue { get => revenue; set => revenue = value != 0 ? value : null; }

        public float? Runtime { get; set; }

        [Name("status")]
        public string? Status { get; set; }

        [Name("tagline")]
        public string? Tagline { get; set; }

        [Name("title")]
        public string Title { get; set; } = null!;

        private float voteAverage;
        [Name("vote_average")]
        public float? VoteAverage { get => VoteCount > 0 ? voteAverage : null; set => voteAverage = value.GetValueOrDefault(); }

        [Name("vote_count")]
        public int VoteCount { get; set; }

        //[Name("adult")]
        //public bool Adult { get; set; }
        //[Name("belongs_to_collection")]
        //public string? BelongsToCollectionJson { get; set; }
        //[Name("poster_path")]
        //public string? PosterPath { get; set; }
        //[Name("production_companies")]
        //public string? ProductionCompaniesJson { get; set; } = null!;
        //[Name("spoken_languages")]
        //public string? SpokenLanguagesJson { get; set; } = null!;
        //[Name("video")]
        //public bool Video { get; set; }

        public class MovieMap : ClassMap<Movie>
        {
            public MovieMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.Genres).Convert(AsDictionary<int, string>("genres"));
                Map(m => m.ProductionCountries).Convert(AsDictionary<string, string>("production_countries", "iso_3166_1"));
                Map(m => m.Runtime).Convert(AsJsonOrDefault<float?>("runtime"));
            }

            private static CsvHelper.ConvertFromString<T> AsJson<T>(string fieldName) => i =>
            {
                var field = Regex.Replace(i.Row.GetField(fieldName), "\\\"(.*?)\\\'(.*?)\\\"", "\'$1`$2\'").Replace('\'', '\"').Replace('`', '\'');
                if (string.IsNullOrWhiteSpace(field))
                    throw new NoNullAllowedException();
                return JsonSerializer.Deserialize<T>(field) ?? throw new NoNullAllowedException();
            };
            private static CsvHelper.ConvertFromString<T?> AsJsonOrDefault<T>(string fieldName) => i =>
            {
                var field = Regex.Replace(i.Row.GetField(fieldName), "\\\"(.*?)\\\'(.*?)\\\"", "\'$1`$2\'").Replace('\'', '\"').Replace('`', '\'');
                if (string.IsNullOrWhiteSpace(field))
                    return default;
                return JsonSerializer.Deserialize<T>(field);
            };
            private static CsvHelper.ConvertFromString<IReadOnlyDictionary<TKey, TValue>> AsDictionary<TKey, TValue>(string fieldName, string idProperty = "id", string nameProperty = "name") where TKey : notnull => i => AsJson<JsonElement[]>(fieldName)(i).ToDictionary(i => GetValue<TKey>(i.GetProperty(idProperty)), i => GetValue<TValue>(i.GetProperty(nameProperty)));

            private static T GetValue<T>(JsonElement element)
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)element.GetInt32();
                if (typeof(T) == typeof(string))
                    return (T)(object)element.GetString()!;
                throw new InvalidOperationException();
            }
        }
    }
}
