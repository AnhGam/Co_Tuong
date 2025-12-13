using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Chinese_Chess.Models
{
    public class BoardState
    {
        public List<Piece> Pieces { get; set; } = new List<Piece>();
        public List<Move> Moves { get; set; } = new List<Move>();
        public PieceColor CurrentTurn { get; set; } = PieceColor.Red;
        public bool IsFlipped => CurrentTurn == PieceColor.Black;
        public Piece GetPieceAt(int x, int y)
            => Pieces.FirstOrDefault(p => p.X == x && p.Y == y && p.IsAlive);

        public void MovePiece(Piece piece, int toX, int toY)
        {
            var target = GetPieceAt(toX, toY);
            if (target != null) target.IsAlive = false;

            var move = new Move(piece, piece.X, piece.Y, toX, toY, target, Moves.Count + 1);
            Moves.Add(move);

            piece.X = toX;
            piece.Y = toY;

            CurrentTurn = CurrentTurn == PieceColor.Red ? PieceColor.Black : PieceColor.Red;
        }

        public BoardState Clone()
        {
            var newBoard = new BoardState();
            newBoard.Pieces = Pieces.Select(p => p.Clone()).ToList();
            newBoard.Moves = new List<Move>(Moves);
            newBoard.CurrentTurn = CurrentTurn;
            return newBoard;
        }

        public void Reset()
        {
            Pieces.Clear();
            Moves.Clear();
            CurrentTurn = PieceColor.Red;
        }

        public Move UndoLastMove()
        {
            if (Moves.Count == 0) return null;
            var lastMove = Moves[Moves.Count - 1];
            Moves.RemoveAt(Moves.Count - 1);

            lastMove.MovedPiece.X = lastMove.FromX;
            lastMove.MovedPiece.Y = lastMove.FromY;
            if (lastMove.CapturedPiece != null)
            {
                lastMove.CapturedPiece.IsAlive = true;
            }
            CurrentTurn = CurrentTurn == PieceColor.Red ? PieceColor.Black : PieceColor.Red;

            return lastMove;
        }
    }
}

