﻿using System;
using System.Collections.Generic;
using System.Text;
using ChallengeHarnessInterfaces;
using Newtonsoft.Json;
using SpaceInvaders.Core;

namespace SpaceInvaders.Renderers
{
    public class SpaceInvadersRenderer : IRenderer
    {
        public MatchRender Render(IMatch renderMatch)
        {
            var match = (Match) renderMatch;

            var result = new MatchRender
            {
                Map = RenderMap(match),
                State = RenderState(match),
                RoundNumber = match.GetRoundNumber(),
                Round = String.Format("Round {0}", match.GetRoundNumber())
            };

            result.Moves[0] = String.Format("Player {0}: {1,-20} > {2}",
                1, match.GetPlayerLastMove(1), match.GetPlayerLastMoveResult(1));
            result.Moves[1] = String.Format("Player {0}: {1,-20} > {2}",
                2, match.GetPlayerLastMove(2), match.GetPlayerLastMoveResult(2));

            return result;
        }

        public MatchSummary RenderSummary(IMatch renderMatch)
        {
            var match = (Match) renderMatch;

            var result = new MatchSummary
            {
                Rounds = match.GetRoundNumber()
            };

            result.Players.Add(RenderPlayer(match, 1));
            result.Players.Add(RenderPlayer(match, 2));

            RenderWinDetails(result, match);

            return result;
        }

        protected static void RenderWinDetails(MatchSummary result, Match match)
        {
            if (match.GetResult() == MatchResult.Tie)
            {
                result.Winner = 1;
                result.WinReason = "Match was a tie, so player 1 wins by default.";
                return;
            }

            var winner = match.GetResult() == MatchResult.PlayerOneWins ? match.GetPlayer(1) : match.GetPlayer(2);
            var opponent = match.GetResult() == MatchResult.PlayerOneWins ? match.GetPlayer(2) : match.GetPlayer(1);

            result.Winner = winner.PlayerNumber;

            if (((opponent.Lives <= 0) && (opponent.Ship == null)) &&
                ((winner.Lives >= 0) || (winner.Ship != null)))
            {
                result.WinReason = String.Format("Player {0} ran out of lives.", opponent.PlayerNumber);
            }
            else
            {
                result.WinReason = String.Format("Player {0} had more kills than player {1}.", winner.PlayerNumber,
                    opponent.PlayerNumber);
            }
        }

        protected static Dictionary<string, object> RenderPlayer(Match match, int playerNumber)
        {
            var player = match.GetPlayer(playerNumber);
            var result = new Dictionary<string, object>
            {
                {"Number", playerNumber},
                {"Name", player.PlayerName},
                {"Kills", player.Kills}
            };
            return result;
        }

        protected string RenderMap(Match match)
        {
            var output = new StringBuilder();
            RenderPlayerDetails(match, 2, output);
            RenderMap(match.Map, output);
            RenderPlayerDetails(match, 1, output);
            return output.ToString();
        }

        protected string RenderState(Match match)
        {
            return JsonConvert.SerializeObject(
                match,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }
                );
        }

        protected void RenderPlayerDetails(Match match, int playerNumber, StringBuilder output)
        {
            var player = match.GetPlayer(playerNumber);
            var lines = new List<string>();
            var width = match.Map.Width;

            lines.Add(new String('#', width) + Environment.NewLine);

            var statusLine = "# " + player.PlayerName + " ";
            lines.Add(
                String.Format(statusLine + new String(' ', Math.Max(width - statusLine.Length - 1, 0)) + "#" +
                              Environment.NewLine));

            statusLine = String.Format("# Round: {0,3} ", match.RoundNumber);
            lines.Add(statusLine + new String(' ', width - statusLine.Length - 1) + "#" + Environment.NewLine);

            statusLine = String.Format("# Kills: {0} ", player.Kills);
            lines.Add(statusLine + new String(' ', width - statusLine.Length - 1) + "#" + Environment.NewLine);

            statusLine = String.Format("# Lives: {0,1} ", player.Lives);
            lines.Add(statusLine + new String(' ', width - statusLine.Length - 1) + "#" + Environment.NewLine);

            statusLine = String.Format("# Missiles: {0,1}/{1,1} ", player.Missiles.Count, player.MissileLimit);
            lines.Add(statusLine + new String(' ', width - statusLine.Length - 1) + "#" + Environment.NewLine);

            if (player.PlayerNumber == 1)
            {
                lines.Reverse();
            }

            foreach (var line in lines)
            {
                output.Append(line);
            }
        }

        protected void RenderMap(Map map, StringBuilder output)
        {
            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    output.Append(GetEntitySymbol(map.GetEntity(x, y)).ToString());
                }
                output.Append(Environment.NewLine);
            }
        }

        protected char GetEntitySymbol(Entity entity)
        {
            var typeChar = EntityTypeCharacter.EmptyTile;
            if (entity == null)
            {
                return (char) typeChar;
            }

            if (entity.Type == EntityType.Ship)
            {
                typeChar = entity.PlayerNumber == 1
                    ? EntityTypeCharacter.PlayerOneShip
                    : EntityTypeCharacter.PlayerTwoShip;
            }
            else if (entity.Type == EntityType.Missile)
            {
                typeChar = entity.PlayerNumber == 1
                    ? EntityTypeCharacter.PlayerOneBullet
                    : EntityTypeCharacter.PlayerTwoBullet;
            }
            else if (entity.Type == EntityType.Bullet)
            {
                typeChar = EntityTypeCharacter.Bullet;
            }
            else if (entity.Type == EntityType.Alien)
            {
                typeChar = EntityTypeCharacter.Alien;
            }
            else if (entity.Type == EntityType.Shield)
            {
                typeChar = EntityTypeCharacter.Shield;
            }
            else if (entity.Type == EntityType.Wall)
            {
                typeChar = EntityTypeCharacter.Wall;
            }
            else if (entity.Type == EntityType.AlienFactory)
            {
                typeChar = EntityTypeCharacter.BuildingAlienFactory;
            }
            else if (entity.Type == EntityType.MissileController)
            {
                typeChar = EntityTypeCharacter.BuildingMissile;
            }

            return (char) typeChar;
        }
    }
}