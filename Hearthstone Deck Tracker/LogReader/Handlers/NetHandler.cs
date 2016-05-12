﻿#region

using System;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class NetHandler
	{
		private DateTime _lastGameStart;
		public void Handle(LogLineItem logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.ConnectionRegex.Match(logLine.Line);
			if(match.Success)
			{
				if(_lastGameStart != logLine.Time)
				{
					game.MetaData.PropagateLegendRank();
					_lastGameStart = logLine.Time;
				}

				game.MetaData.ServerAddress = match.Groups["address"].Value.Trim();
				game.MetaData.ClientId = match.Groups["client"].Value.Trim();
				game.MetaData.GameId = match.Groups["game"].Value.Trim();
				game.MetaData.SpectateKey = match.Groups["spectateKey"].Value.Trim();

				var region = Helper.GetRegionByServerIp(game.MetaData.ServerAddress);
				if(game.CurrentRegion == Region.UNKNOWN || region == Region.CHINA)
				{
					game.CurrentRegion = region;
					Log.Info("Set current region to" + region);
				}

				//just to make sure this still works in case the typo gets fixed
				if(logLine.Line.ToLower().Contains("reconncting=true") || logLine.Line.ToLower().Contains("reconnecting=true"))
				{
					game.StoreGameState();
					game.MetaData.Reconnected = true;
				}
				gameState.Reset();
				gameState.GameHandler.HandleGameStart();
			}
		}
	}
}