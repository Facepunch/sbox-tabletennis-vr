namespace TableTennis;

public enum Rank
{
	Bronze,
	Silver,
	Gold,
	Diamond,
	Platinum
}

public partial class RankComponent : EntityComponent
{
	[Net] public float Elo { get; set; }

	public Rank Rank
	{
		get
		{
			if ( Elo < 1200 ) return Rank.Bronze;
			if ( Elo < 1400 ) return Rank.Silver;
			if ( Elo < 1600 ) return Rank.Gold;
			if ( Elo < 1800 ) return Rank.Diamond;
			return Rank.Platinum;
		}
	}

	public string RankImage
	{
		get => Rank switch
		{
			Rank.Bronze => "rank_bronze",
			Rank.Silver => "rank_silver",
			Rank.Gold => "rank_gold",
			Rank.Diamond => "rank_diamond",
			Rank.Platinum => "rank_platinum",
			_ => "rank_bronze"
		};
	}

	public async void FetchStats()
	{
		try
		{
			var cl = Entity as Client;

			var results = await GameServices.Leaderboard.Query( Global.GameIdent, cl.PlayerId, "Unknown" );
			var entry = results.Entries.FirstOrDefault();

			if ( results.Entries.Count > 0 )
			{
				Elo = entry.Rating;
				Log.Info( $"{cl.Name}'s Elo ({Elo}) was fetched." );
			}
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}

	protected override void OnActivate()
	{
		if ( !Host.IsServer ) return;

		FetchStats();
	}
}