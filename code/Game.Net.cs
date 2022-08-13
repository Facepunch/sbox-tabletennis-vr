namespace TableTennis;

public partial class TableTennisGame
{
	//
	// Isolate these RPCs here, the idea will be to do lag compensation here too.
	//

	[ConCmd.Server]
	public static void IHitTheBallCunt( Vector3 position, Vector3 velocity, float time )
	{
		Current.RPCBallHit( To.Multiple( Client.All.Where( c => c != ConsoleSystem.Caller ) ), ConsoleSystem.Caller, position, velocity, time );
	}

	[ConCmd.Server]
	public static void IThrewTheBallCunt( Vector3 position, Vector3 velocity, float time )
	{
		Current.RPCBallThrow( To.Multiple( Client.All.Where( c => c != ConsoleSystem.Caller ) ), ConsoleSystem.Caller, position, velocity, time );
	}

	[ClientRpc]
	public void RPCBallHit( Client hittingClient, Vector3 position, Vector3 velocity, float timeThen )
	{
		Ball.Position = position;
		Ball.Velocity = velocity;
		// BallPhysics.Move( ClientBall, Time.Now - time ); // lag compensation, but it sucks - do it a different way.
	}

	[ClientRpc]
	public void RPCBallThrow( Client throwingClient, Vector3 position, Vector3 velocity, float timeThen )
	{
		Ball.Position = position;
		Ball.Velocity = velocity;
		// BallPhysics.Move( ClientBall, Time.Now - time ); // lag compensation, but it sucks - do it a different way.
	}

	/// <summary>
	/// Gives a client's pawn the ability to serve the active ball.
	/// </summary>
	[ClientRpc]
	public void ClientServingBall( Client client )
	{
		if ( Ball.IsValid() ) Ball.Delete();
		Ball = new Ball();

		var twat = client.Pawn as PlayerPawn;
		twat.ServeHand.SetBall( Ball );
	}
}
