using System;
using System. Collections. Generic;
using System. Linq;
using System. Security. Cryptography. X509Certificates;
using System. Text;
using System. Threading. Tasks;

namespace MovieCatalog. ConsoleApp
{
	/// <summary>
	/// Egy <see cref="MovieCatalog.Database"/> típusú adatbázison megfogalmazott lekérdezések futtatásáért felelős objektum.
	/// </summary>
	public class MovieQueries
	{
		private long? Profit;

		/// <summary>
		/// Az adatbázis, amin a lekérdezések futnak
		/// </summary>
		public Database Database { get; }

		/// <summary>
		/// Egy <see cref="MovieQueries"/> példány létrehozása a megadott <paramref name="database"/> adatbázissal.
		/// </summary>
		/// <param name="database">Az "adatbázis", amin a lekérdezések futni fognak.</param>
		public MovieQueries ( Database database ) => Database = database;

		/// <summary>
		/// A legnépszerűbb film lekérdezése (ahol legalább 1000 szavazatot adtak, a legmagasabb átlagos értékelés szerint).
		/// </summary>
		/// <returns>A megadottaknak megfelelő film példány.</returns>
		public Movie GetTheBestPopularMovie ( )
		{
			return Database. Movies. Where ( m => m. VoteCount > 1000 ). OrderByDescending ( m => m. VoteAverage ). First ();
		}

		/// <summary>
		/// A megadott <paramref name="year"/> évben megjelent filmek számának lekérdezése.
		/// </summary>
		/// <param name="year">A kérdéses év, amelyre a szűrés történik.</param>
		/// <returns>A megadott <paramref name="year"/> évben megjelent filmek száma.</returns>
		public int GetNumberOfMoviesInYear ( int year )
		{
			return Database. Movies. Where ( m => m. ReleaseDate. HasValue && m. ReleaseDate. Value. Year == year ). Select ( m => m. Title ). Count ();
		}

		/// <summary>
		/// A 2010 és 2015 (inkluzív) intervallumba eső 5 (filmek száma szerinti) legnépszerűbb műfaj lekérdezése.
		/// </summary>
		/// <returns>Egy megszámlálható példány (pl. lista), amelyben a műfaj nevét és a hozzá tartozó 2010 és 2015 (inkluzív) között megjelent filmek számát tartalmazó tuple példányok találhatók.</returns>
		public IEnumerable<(string Genre, int NumberOfMovies)> GetTop5MostPopularGenresFrom2010To2015 ( )
		{
			return Database. Genres
				. OrderByDescending ( x => x. Value )
				. Take ( 5 )
				. Select ( x => (x. Value, Database. Movies. Where ( m => m. Genres. ContainsKey ( x. Key ) ). Count ()) ). ToList ();
		}

		/// <summary>
		/// A 10 legnagyobb költségvetésű film lekérdezése.
		/// </summary>
		/// <returns>A 10 legnagyobb költségvetésű film.</returns>
		public IEnumerable<Movie> GetTop10BiggestBudgetMovies ( )
		{
			return Database. Movies. OrderByDescending ( m => m. Budget ). Take ( 10 );
		}

		/// <summary>
		/// Az 5 legtöbb műfajjal rendelkező film lekérdezése.
		/// </summary>
		/// <returns>Az 5 legtöbb műfajjal rendelkező film kollekciója.</returns>
		public IEnumerable<Movie> GetTop5MoviesWithTheMostNumberOfGenres ( )
		{
			return Database. Movies. OrderByDescending ( m => m. Genres. Count () ). Take ( 5 );
		}

		/// <summary>
		/// A megadott <paramref name="country"/> országban megjelent legnagyobb költségvetésű film lekérdezése.
		/// </summary>
		/// <param name="country">Az ország kódja (pl. "US" vagy "HU").</param>
		/// <returns>A legnagyobb költségvetésű film a megadott országban, vagy null, ha nem található az országhoz tartozó film.</returns>
		public Movie? GetHighestBudgetMovieMadeInCountry ( string country )
		{
			return Database.Movies.Where( x => x.ProductionCountries.ContainsKey ( country ) ).Where( x => x.Budget.HasValue).OrderByDescending( x => x.Budget).FirstOrDefault ();

		}

		/// <summary>
		/// A legnagyobb profitot realizáló film lekérdezése. A profit a film <see cref="Movie.Revenue"/> bevételének és <see cref="Movie.Budget"/> büdzséjének különbsége.
		/// </summary>
		/// <returns>Egy tuple (ennes), melyben a film és az általa realizált profit (USD) található.</returns>
		public (Movie? Movie, long Profit) GetHighestProfitMovieEverMade ( )
		{
			var sol = Database. Movies. OrderByDescending ( x => x. Revenue - x. Budget ). FirstOrDefault ();
			return (sol, (long) (sol.Revenue - sol.Budget));
		}

		/// <summary>
		/// A legnagyobb bukás (legkisebb profitot realizáló film profit értéke) mértékének lekérdezése.
		/// </summary>
		/// <returns>A legnagyobb bukás mértéke USD-ben.</returns>
		public long GetBiggestMovieFlopEver ( )
		{
			return Database. Movies. OrderBy( x => x. Revenue - x. Budget ). Select( x => (long) (x.Revenue - x.Budget)).First();
		}

		/// <summary>
		/// Filmek keresése cím szerint.
		/// </summary>
		/// <param name="titleContains">A megadott címnek kis-nagybetűtől függetlenül, és ékezetek nélkül (lásd: <see cref="String.Contains(string, StringComparison)"/> és <see cref="StringComparison.OrdinalIgnoreCase"/>) kell szerepelnie a film <see cref="Movie.Title"/> címében.</param>
		/// <param name="page">A megadott oldalméretű oldal száma, 0-tól indexelve.</param>
		/// <param name="pageSize">Az oldalméret szabályozza, hány elem található az eredményhalmazban, illetve azt, hogy hány elemet hagyunk ki a szűretlen eredményhalmaz elejéről.</param>
		/// <returns>A (legfeljebb) <paramref name="pageSize"/> méretű kollekció a(z) <paramref name="page"/>. oldalról, amely filmek címe tartalmazza a megadott <paramref name="titleContains"/> paraméteret.</returns>
		public IEnumerable<Movie> GetMoviesOrderByPopularityWithTitleMatchPaged ( string titleContains, int page = 0, int pageSize = 5 )
		{
			return Database. Movies
				. Where ( m => m. Title. Contains ( titleContains, StringComparison. OrdinalIgnoreCase ))
				. Skip ( page * pageSize )
				. Take ( pageSize );
		}
	}
}
