using Microsoft.AspNetCore.Mvc;
using ServerApp.Data;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerApp.WebApiCoreManagement
{
    // DTO for incoming move and game requests
    public class MoveDto
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ConnectFourApiController : ControllerBase
    {
        private readonly ConnectFourContext _context;
        private const int Rows = 6;
        private const int Columns = 7;

        public ConnectFourApiController(ConnectFourContext context)
        {
            _context = context;
        }

        // GET api/ConnectFourApi/games
        [HttpGet("games")]
        public ActionResult<IEnumerable<Game>> GetAllGames()
        {
            var games = _context.Games.ToList();
            return Ok(games);
        }

        // GET api/ConnectFourApi/nextGame/{playerId}
        [HttpGet("nextGame/{playerId}")]
        public ActionResult<int> NextGame(int playerId)
        {
            if (!_context.Players.Any(p => p.Id == playerId))
                return NotFound(new { message = "Player not found." });

            var game = new Game
            {
                PlayerId = playerId,
                StartTime = DateTime.UtcNow,
                Moves = "0",
                Duration = TimeSpan.Zero,
                Winner = null
            };
            _context.Games.Add(game);
            _context.SaveChanges();

            return Ok(game.Id);
        }

        // GET api/ConnectFourApi/moves/{gameId}
        [HttpGet("moves/{gameId}")]
        public ActionResult<IEnumerable<Move>> GetMovesForGame(int gameId)
        {
            if (!_context.Games.Any(g => g.Id == gameId))
                return NotFound(new { message = "Game not found." });

            var moves = _context.Moves
                                .Where(m => m.GameId == gameId)
                                .OrderBy(m => m.MoveTime)
                                .ToList();

            return Ok(moves);
        }

        // POST api/ConnectFourApi/move
        [HttpPost("move")]
        public ActionResult<object> SubmitMove([FromBody] MoveDto dto)
        {
            // Validate player and game
            if (!_context.Players.Any(p => p.Id == dto.PlayerId))
                return NotFound(new { message = "Player not found." });
            if (!_context.Games.Any(g => g.Id == dto.GameId))
                return NotFound(new { message = "Game not found." });

            // Save the player's move
            var playerMove = new Move
            {
                GameId = dto.GameId,
                PlayerId = dto.PlayerId,
                Row = dto.Row,
                Column = dto.Column,
                MoveTime = DateTime.UtcNow
            };
            _context.Moves.Add(playerMove);
            _context.SaveChanges();

            // Choose random column for opponent
            var rand = new Random();
            int oppCol;
            do
            {
                oppCol = rand.Next(0, Columns);
            } while (_context.Moves.Count(m => m.GameId == dto.GameId && m.Column == oppCol) >= Rows);

            // Compute opponent row
            var usedRows = _context.Moves
                .Where(m => m.GameId == dto.GameId && m.Column == oppCol)
                .Select(m => m.Row)
                .ToHashSet();
            int oppRow = Enumerable.Range(0, Rows)                                  
                .Reverse()
                .First(r => !usedRows.Contains(r));

            // Persist opponent move
            var compMove = new Move
            {
                GameId = dto.GameId,
                PlayerId = dto.PlayerId == 1 ? 2 : 1,
                Row = oppRow,
                Column = oppCol,
                MoveTime = DateTime.UtcNow
            };
            _context.Moves.Add(compMove);
            _context.SaveChanges();

            // Return both moves
            return Ok(new
            {
                yourMove = new { row = dto.Row, column = dto.Column },
                opponentMove = new { row = oppRow, column = oppCol }
            });
        }

        private bool ColumnIsFull(int gameId, int column)
        {
            return _context.Moves.Count(m => m.GameId == gameId && m.Column == column) >= Rows;
        }
    }
}
