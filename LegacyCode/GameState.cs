namespace TableTennis;

public enum GameState
{
	WaitingForPlayers, // Waiting for players to join the game.
	Serving, // Players are serving.
	FailedServe, // Server failed their serve.
	Playing, // Players are playing.
	PointAwarded, // A point has been awarded.
	GameOver, // The game is over.
}
