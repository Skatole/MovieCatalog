using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MovieCatalog.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Adatforrás: https://www.kaggle.com/rounakbanik/the-movies-dataset?select=movies_metadata.csv
            var db = new Database(File.OpenRead("movies_metadata.csv.zip"));
            var queries = new MovieQueries(db);

            TryEvaluate("GetTheBestPopularMovie", q => q.GetTheBestPopularMovie(), result =>
            {
                Console.WriteLine($"The best popular movie so far is: {result.Title} ({result.ReleaseDate:yyyy-MM-dd}), rated {result.VoteAverage} ({result.VoteCount} votes)");
            });

            TryEvaluate("GetNumberOfMoviesInYear", q => q.GetNumberOfMoviesInYear(2010), result =>
            {
                Console.WriteLine($"The number of movies in year 2010 is: {result}");
            });

            TryEvaluate("GetTop5MostPopularGenresFrom2010To2015", q => q.GetTop5MostPopularGenresFrom2010To2015(), result =>
            {
                Console.WriteLine(string.Join('\n', result.Select(gm => $"{gm.Genre} ({gm.NumberOfMovies} movies)")));
            });

            TryEvaluate("GetTop10BiggestBudgetMovies", q => q.GetTop10BiggestBudgetMovies(), result =>
            {
                Console.WriteLine(string.Join('\n', result.Select(m => $"{m.Title} ({m.ReleaseDate?.Year.ToString() ?? "----"}) - {m.Budget!.Value:N}")));
            });

            TryEvaluate("GetTop5MoviesWithTheMostNumberOfGenres", q => q.GetTop5MoviesWithTheMostNumberOfGenres(), result =>
            {
                Console.WriteLine(string.Join('\n', result.Select(m => $"{m.Title} ({m.ReleaseDate?.Year.ToString() ?? "----"}) - {string.Join(", ", m.Genres.Select(g => g.Value))}")));
            });

            TryEvaluate("GetHighestBudgetMovieMadeInCountry(\"HU\")", q => q.GetHighestBudgetMovieMadeInCountry("HU"), result =>
            {
                Console.WriteLine($"{result.Title} ({result.ReleaseDate?.Year.ToString() ?? "----"}) - {result.Budget!.Value:N}");
            });

            TryEvaluate("GetHighestProfitMovieEverMade", q => q.GetHighestProfitMovieEverMade(), result =>
            {
                Console.WriteLine($"{result.Movie.Title} ({result.Movie.ReleaseDate?.Year.ToString() ?? "----"})\n Budget: {result.Movie.Budget!.Value:N}\n Revenue: {result.Movie.Revenue:N}\n Profit: {result.Profit:N}");
            });

            TryEvaluate("GetMoviesOrderByPopularityWithTitleMatchPaged (\"tales\", 1)", q => q.GetMoviesOrderByPopularityWithTitleMatchPaged("tales", 1), result =>
            {
                Console.WriteLine(string.Join('\n', result.Select((e, i) => (e, i)).Select(m => $"{m.i + 1}: {m.e.Title} ({m.e.ReleaseDate?.Year.ToString() ?? "----"}) - {m.e.Popularity}")));
            });

            TryEvaluate("GetMoviesOrderByPopularityWithTitleMatchPaged (\"tales\", 2)", q => q.GetMoviesOrderByPopularityWithTitleMatchPaged("tales", 2), result =>
            {
                Console.WriteLine(string.Join('\n', result.Select((e, i) => (e, i)).Select(m => $"{m.i + 1}: {m.e.Title} ({m.e.ReleaseDate?.Year.ToString() ?? "----"}) - {m.e.Popularity}")));
            });


            void TryEvaluate<T>(string title, Func<MovieQueries, T> function, Action<T> action)
            {
                try
                {
                    Console.WriteLine(title);
                    var stopwatch = Stopwatch.StartNew();
                    var result = function(queries);
                    stopwatch.Stop();
                    action(result);
                    WriteMessageAndBreakLine($"{title} finished in {stopwatch.ElapsedMilliseconds}ms.");
                }
                catch (NotImplementedException)
                {
                    WriteMessageAndBreakLine($"{title} is not yet implemented.", true);
                }
                static void WriteMessageAndBreakLine(string message, bool warn = false)
                {
                    var (back, fore) = (Console.BackgroundColor, Console.ForegroundColor);
                    (Console.BackgroundColor, Console.ForegroundColor) = warn ? (ConsoleColor.Yellow, ConsoleColor.DarkBlue) : (back, ConsoleColor.Green);
                    Console.WriteLine($"{message}\n{new string('-', message.Length)}");
                    (Console.BackgroundColor, Console.ForegroundColor) = (back, fore);
                }
            }
        }
    }
}
