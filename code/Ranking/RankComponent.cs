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
			if ( Elo < 1010 ) return Rank.Bronze;
			if ( Elo < 1020 ) return Rank.Silver;
			if ( Elo < 1030 ) return Rank.Gold;
			if ( Elo < 1040 ) return Rank.Diamond;
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

	protected override void OnActivate()
	{
		if ( !Game.IsServer ) return;

		// TODO - Implement me!
	}
}
