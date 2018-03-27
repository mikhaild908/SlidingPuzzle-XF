using System;
using Android.Views;
using Android.Graphics;
using Android.Content;
using Android.Util;

namespace SlidingPuzzle.Droid
{
    public class PuzzleCanvasView : View
    {
        #region Private Members

        readonly Tile[] _tiles = new Tile[9];

        readonly int _tileWidth;
        readonly int _tileHeight;

        Canvas _drawCanvas;
        Bitmap _canvasBitmap;
        Bitmap _sourceBitmap;
        Paint _whitePaint;
        Paint _clearTilePaint;

        Tile _selectedTile;
        Tile _emptyTile;

        bool _puzzleSolved;

        //int _xOffset;
        //int _yOffset;
        #endregion

        #region Constructors
        public PuzzleCanvasView(Context context) : base(context, null, 0)
        {
            //float scale = context.Resources.DisplayMetrics.Density; // 1.5 for ZTE

            _sourceBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.smiley);

            _tileWidth = _sourceBitmap.Width / 3;
            _tileHeight = _sourceBitmap.Height / 3;

            _whitePaint = new Paint() { Color = Color.White, StrokeWidth = 1f, AntiAlias = true };
            _whitePaint.SetStyle(Paint.Style.Stroke);

            _clearTilePaint = new Paint() { Color = Color.White, StrokeWidth = 5f, AntiAlias = true };
            _clearTilePaint.SetStyle(Paint.Style.FillAndStroke);

            //var metrics = Resources.DisplayMetrics;
            //_xOffset = metrics.WidthPixels / 2 - _sourceBitmap.Width / 2;
            //_yOffset = metrics.HeightPixels / 2 - _sourceBitmap.Height / 2;
        }

        public PuzzleCanvasView(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public PuzzleCanvasView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) { }
        #endregion

        #region Overriden Methods
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            //canvasBitmap = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888); // full-screen bitmap
            //canvasBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon);
            //canvasBitmap = Bitmap.CreateBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon), 0, 0, 72, 72);
            //_canvasBitmap = Bitmap.CreateBitmap(_sourceBitmap, 120, 120, 120, 120);
            //Bitmap mutableBitmap = _canvasBitmap.Copy(Bitmap.Config.Argb8888, true);

            _canvasBitmap = Bitmap.CreateBitmap(_sourceBitmap.Width, _sourceBitmap.Height, Bitmap.Config.Argb8888);
            _drawCanvas = new Canvas(_canvasBitmap);

            InitializePuzzle();
            Shuffle();
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                    {
                        if (_puzzleSolved)
                        {
                            return true;
                        }

                        var startingCoordinates = new MotionEvent.PointerCoords();

                        int id = e.GetPointerId(0);
                        e.GetPointerCoords(id, startingCoordinates);

                        _selectedTile = GetTile((int)startingCoordinates.X, (int)startingCoordinates.Y);

                        return true;
                    }

                case MotionEventActions.PointerDown:
                    {
                        return true;
                    }

                case MotionEventActions.Move:
                    {
                        if (_puzzleSolved)
                        {
                            return true;
                        }

                        var moveableLeft = IsEmptyTileToTheLeft(_selectedTile);
                        var moveableRight = IsEmptyTileToTheRight(_selectedTile);
                        var moveableUp = IsEmptyTileAbove(_selectedTile);
                        var moveableDown = IsEmptyTileBelow(_selectedTile);

                        if (moveableLeft || moveableRight || moveableUp || moveableDown)
                        {
                            ClearTile(_selectedTile);
                            MoveTile(_selectedTile,
                                     (int)e.GetX(0) - _tileWidth / 2,
                                     (int)e.GetY(0) - _tileHeight / 2,
                                     moveableLeft,
                                     moveableRight,
                                     moveableUp,
                                     moveableDown);
                            
                            Invalidate();
                        }

                        return true;
                    }

                case MotionEventActions.PointerUp:
                    {
                        return true;
                    }

                case MotionEventActions.Up:
                    {
                        if (_puzzleSolved)
                        {
                            return true;
                        }

                        if (IsTileOverlappingWithEmptyTile(_selectedTile))
                        {
                            ClearTile(_selectedTile);
                            SwapTiles(_selectedTile, _emptyTile);

                            if(IsPuzzleSolved())
                            {
                                // TODO: display puzzle solved
                                _drawCanvas.DrawText("Puzzle solved!!!", 0, 0, new Paint { Color = Color.Green, StrokeWidth = 2f, AntiAlias = true});
                            }
                        }

                        Invalidate();

                        _selectedTile = null;
                        return true;
                    }

                default:
                    return false;
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            // Copy the off-screen canvas data onto the View from it's associated Bitmap (which stores the actual drawn data)
            canvas.DrawBitmap(_canvasBitmap, 0, 0, null);
        }
        #endregion

        #region Helper Methods
        private void InitializePuzzle()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                int x = (i % 3) * _tileWidth;
                int y = (i / 3) * _tileHeight;

                _tiles[i] = new Tile { Id = i, X = x, Y = y, XBeforeMoving = x, YBeforeMoving = y };

                if (_tiles.Length - 1 > i)
                {
                    _tiles[i].Image = Bitmap.CreateBitmap(_sourceBitmap, x, y, _tileWidth, _tileHeight);

                    _drawCanvas.DrawBitmap(_tiles[i].Image, x, y, null);
                    _drawCanvas.DrawRect(new Rect(x,
                                                  y,
                                                  x + _tileWidth,
                                                  y + _tileHeight),
                                         _whitePaint);
                }
                else
                {
                    _emptyTile = _tiles[i];
                }
            }
        }

        private Tile GetTile(int x, int y)
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (IsWithinTileBoundary(_tiles[i], x, y))
                {
                    return _tiles[i];
                }
            }

            return null;
        }

        private bool IsWithinTileBoundary(Tile tile, int x, int y)
        {
            return tile != null
                    && tile.X <= x
                    && tile.X + _tileWidth >= x
                    && tile.Y <= y
                    && tile.Y + _tileHeight >= y;
        }

        private void MoveTile(Tile tile, int newX, int newY, bool moveableLeft, bool moveableRight, bool moveableUp, bool moveableDown)
        {
            if (tile == null ||
                newX >= tile.XBeforeMoving + _tileWidth ||
                newX <= tile.XBeforeMoving - _tileWidth ||
                newY >= tile.YBeforeMoving + _tileHeight ||
                newY <= tile.YBeforeMoving - _tileHeight)
            {
                return;
            }

            if ((moveableLeft && newX < tile.X) || (moveableRight && newX > tile.X))
            {
                tile.X = newX;
            }

            if ((moveableUp && newY < tile.Y) || (moveableDown && newY > tile.Y))
            {
                tile.Y = newY;
            }

            _drawCanvas.DrawBitmap(tile.Image, tile.X, tile.Y, null);
        }

        private void ClearTile(Tile tile)
        {
            if (tile == null ||
                tile.X >= tile.XBeforeMoving + _tileWidth ||
                tile.X <= tile.XBeforeMoving - _tileWidth ||
                tile.Y >= tile.YBeforeMoving + _tileHeight ||
                tile.Y <= tile.YBeforeMoving - _tileHeight)
            {
                return;
            }

            _drawCanvas.DrawRect(tile.X, tile.Y, tile.X + _tileWidth, tile.Y + _tileHeight, _clearTilePaint);
        }

        private void SwapTiles(Tile tile1, Tile tile2)
        {
            var tempXBeforeMoving = tile1.XBeforeMoving;
            var tempYBeforeMoving = tile1.YBeforeMoving;
            var temp = tile1;

            var indexOfTile1 = Array.IndexOf(_tiles, tile1);
            var indexOfTile2 = Array.IndexOf(_tiles, tile2);

            _tiles[indexOfTile1] = tile2;
            _tiles[indexOfTile2] = tile1;

            tile1.X = tile2.XBeforeMoving;
            tile1.Y = tile2.YBeforeMoving;
            tile1.XBeforeMoving = tile2.XBeforeMoving;
            tile1.YBeforeMoving = tile2.YBeforeMoving;

            tile2.X = tempXBeforeMoving;
            tile2.Y = tempYBeforeMoving;
            tile2.XBeforeMoving = tempXBeforeMoving;
            tile2.YBeforeMoving = tempYBeforeMoving;

            if (tile1 == _emptyTile)
            {
                DrawEmptyTile(tile1.X, tile1.Y);
            }
            else
            {
                _drawCanvas.DrawBitmap(tile1.Image, tile1.XBeforeMoving, tile1.YBeforeMoving, null);
                _drawCanvas.DrawRect(new Rect(tile1.XBeforeMoving, tile1.YBeforeMoving, tile1.XBeforeMoving + _tileWidth, tile1.YBeforeMoving + _tileHeight), _whitePaint);
            }

            if(tile2 == _emptyTile)
            {
                DrawEmptyTile(tile2.X, tile2.Y);
            }
            else
            {
                _drawCanvas.DrawBitmap(tile2.Image, tile2.XBeforeMoving, tile2.YBeforeMoving, null);
                _drawCanvas.DrawRect(new Rect(tile2.XBeforeMoving, tile2.YBeforeMoving, tile2.XBeforeMoving + _tileWidth, tile2.YBeforeMoving + _tileHeight), _whitePaint);
            }
        }

        private bool IsTileOverlappingWithEmptyTile(Tile tile)
        {
            if (tile == null)
            {
                return false;
            }

            var emptyTileAbove = IsEmptyTileAbove(tile);
            var emptyTileBelow = !emptyTileAbove && IsEmptyTileBelow(tile);
            var emptyTileToTheLeft = !emptyTileAbove && !emptyTileBelow && IsEmptyTileToTheLeft(tile);
            var emptyTileToTheRight = !emptyTileAbove && !emptyTileBelow && !emptyTileToTheLeft && IsEmptyTileToTheRight(tile);

            return (emptyTileToTheRight && tile.X + _tileWidth >= _emptyTile.X + _tileWidth / 4) ||
                   (emptyTileToTheLeft && tile.X <= _emptyTile.X + 3 * _tileWidth / 4) ||
                   (emptyTileAbove && tile.Y <= _emptyTile.Y + 3 * _tileHeight / 4) ||
                   (emptyTileBelow && tile.Y + _tileHeight >= _emptyTile.Y + _tileHeight / 4);
        }

        private bool IsEmptyTileToTheRight(Tile tile)
        {
            if (tile == null)
            {
                return false;
            }

            var index = Array.IndexOf(_tiles, tile);

            var modulus = (index + 1) % 3;

            if (modulus == 0)
            {
                return false;
            }

            return _tiles[index + 1] == _emptyTile;
        }

        private bool IsEmptyTileToTheLeft(Tile tile)
        {
            if (tile == null)
            {
                return false;
            }

            var index = Array.IndexOf(_tiles, tile);

            var modulus = (index + 1) % 3;

            if (modulus == 1)
            {
                return false;
            }

            return _tiles[index - 1] == _emptyTile;
        }

        private bool IsEmptyTileBelow(Tile tile)
        {
            if (tile == null)
            {
                return false;
            }

            var index = Array.IndexOf(_tiles, tile);

            if (index > 5)
            {
                return false;
            }

            return _tiles[index + 3] == _emptyTile;
        }

        private bool IsEmptyTileAbove(Tile tile)
        {
            if (tile == null)
            {
                return false;
            }

            var index = Array.IndexOf(_tiles, tile);

            if (index < 3)
            {
                return false;
            }

            return _tiles[index - 3] == _emptyTile;
        }

        private void DrawEmptyTile(int x, int y)
        {
            _drawCanvas.DrawRect(x, y, x + _tileWidth, y + _tileHeight, _clearTilePaint);
        }

        private void Shuffle()
        {
            SwapTiles(_tiles[8], _tiles[5]);
            SwapTiles(_tiles[5], _tiles[2]);
            SwapTiles(_tiles[2], _tiles[1]);
            SwapTiles(_tiles[1], _tiles[0]);
            SwapTiles(_tiles[0], _tiles[3]);
        }

        private bool IsPuzzleSolved()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].Id != i)
                {
                    _puzzleSolved = false;
            
                    return _puzzleSolved;
                }
            }

            _puzzleSolved = true;

            return _puzzleSolved;
        }
        #endregion

        #region TODO
        public void SetImageSource(string imageSource)
        {
            //paint.imageSource = imageSource;
            //canvasBitmap.
        }
        #endregion

        // TODO: check if puzzle solved
    }
}
