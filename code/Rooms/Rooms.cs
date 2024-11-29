using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneralGame
{
	public sealed class Rooms : Component
	{
		[Property] private bool[,] verticalWalls;
		[Property] private int GridSize = 10;

		public void GenerateWalls()
		{
			verticalWalls = new bool[GridSize, GridSize];

			for ( int x = 0; x < GridSize; x++ )
			{
				for ( int y = 0; y < GridSize; y++ )
				{
					if ( x == 0 || y == 0 || x == GridSize - 1 || y == GridSize - 1 )
					{
						CreateWall( new Vector3( x * 100, y * 100, 0 ), Vector3.Forward );
					}
					else
					{
						if ( verticalWalls[x, y] )
						{
							CreateWall( new Vector3( x * 100, y * 100, 0 ), Vector3.Forward );
						}
						else
						{
							CreateWall( new Vector3( x * 100, y * 100, 0 ), Vector3.Right );
						}
					}
				}
			}
		}

		private void CreateWall( Vector3 position, Vector3 direction )
		{
			var prefab = ResourceLibrary.Get<PrefabFile>( "dungeon/walls/wall.prefab" );
			if ( prefab != null )
			{
				var wall = this;
				wall.WorldPosition = position;
				wall.WorldRotation = Rotation.LookAt( direction );
			}
			else
			{
				Log.Warning( "Wall prefab not found." );
			}
		}

		[Button( "Generate Walls" )]
		public  void GenerateWallsCommand()
		{
			var rooms = Scene.Components.GetOrCreate<Rooms>();
			if ( rooms != null )
			{
				rooms.GenerateWalls();
				Log.Info( "Walls generated." );
			}
			else
			{
				Log.Warning( "No Rooms component found." );
			}
		}
		[Button( "Clear Rooms" )]
		public void ClearWalls()
		{
			foreach ( var wall in Scene.Components.GetAll<Rooms>() )
			{
				wall.Destroy();
			}
		}

		[Event( "server.map.spawn" )]
		public void OnMapSpawn()
		{
			Scene.Components.Create<Rooms>();
		}
	}
}