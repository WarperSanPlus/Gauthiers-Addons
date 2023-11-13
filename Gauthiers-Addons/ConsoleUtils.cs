using ArrayUtils;
using StringUtils;
using System.Drawing;
using static ColorUtils.ColorConsole;
using static StringUtils.StringManipulation;

namespace ConsoleUtils
{
    public static class ConsoleUI
    {
        /// <summary>
        /// Window that can hold text
        /// </summary>
        public class WindowRect
        {
            /// <summary>
            /// Name by default of a window
            /// </summary>
            private const string DefaultName = "New Window";

            private struct Pixel
            {
                /// <summary>
                /// Color of the character displayed by this pixel
                /// </summary>
                private Color foregroundColor = Color.White;

                internal Color ForegroundColor
                {
                    readonly get => foregroundColor;
                    set
                    {
                        // If the color isnt the same and the background character isn't transparent
                        if (foregroundColor != value && Character != ' ')
                            NeedsUpdate = true;

                        foregroundColor = value;
                    }
                }

                /// <summary>
                /// Color of the background displayed by this pixel
                /// </summary>
                private Color backgroundColor = Color.Black;

                internal Color BackgroundColor
                {
                    readonly get => backgroundColor;
                    set
                    {
                        if (backgroundColor != value)
                            NeedsUpdate = true;

                        backgroundColor = value;
                    }
                }

                /// <summary>
                /// Character this pixel displays
                /// </summary>
                internal char Character { get; private set; } = default;

                /// <summary>
                /// Determines if the pixel was changed since the last time it was drawn
                /// </summary>
                internal bool NeedsUpdate { get; set; } = default;

                internal FontStyle Styles { get; private set; } = default;

                public Pixel() { }

                internal void SetCharacter(char value)
                {
                    if (Character != value)
                        NeedsUpdate = true;

                    Character = value;
                }
                internal void SetStyles(FontStyle newStyles)
                {
                    if (Styles != newStyles)
                        NeedsUpdate = true;

                    Styles = newStyles;
                }

                internal Pixel DrawPixel()
                {
                    //System.Threading.Thread.Sleep(10);

                    ShowConsoleMessage(Character.ToString().ApplyStyles(Styles), false, foregroundColor, backgroundColor);

                    NeedsUpdate = false;
                    return this;
                }
            }

            /// <summary>
            /// Informations of this window
            /// </summary>
            private Rectangle Window;

            /// <summary>
            /// Width of the window
            /// </summary>
            public int Width => Window.Width;

            /// <summary>
            /// Height of the window
            /// </summary>
            public int Height => Window.Height;

            /// <summary>
            /// A <see cref="Point"/> that represents the upper-left of this <see cref="WindowRect"/>.
            /// </summary>
            public Point Position => Window.Location;

            /// <summary>
            /// Determines if this window is active or not
            /// </summary>
            private bool isActive = true;

            private readonly Pixel[,] Pixels;

            #region Constructors

            /// <inheritdoc/>
            public WindowRect(int x, int y, int width, int height, string name = DefaultName)
                : this(new Rectangle(x, y, width + 4, height + 2), name, Color.Gray, default, default) { }

            private WindowRect(
                Rectangle window,
                string name,
                Color frameColor,
                Color backgroundColor,
                int depth)
            {
                this.Window = window; // Size + Position
                this.Pixels = new Pixel[Height, Width]; // Pixel range
                this.Name = name;

                this.FrameColor = frameColor;
                this.BackgroundColor = backgroundColor;
                this.TextColor = Color.White;

                this.Depth = depth;

                AddWindow(this);
            }

            #endregion Constructors

            #region Text

            /// <summary>
            /// Color of the text of this window
            /// </summary>
            /// <remarks>
            /// If a color rule gets apply, a discontinuity of this color can occur
            /// </remarks>
            private Color _textColor = Color.White;

            /// <inheritdoc cref="_textColor"/>
            public Color TextColor
            {
                get => _textColor;
                set
                {
                    var offset = Offset;

                    for (int y = offset.Y; y < Pixels.GetLength(0) - 2; y++)
                    {
                        for (int x = offset.X; x < Pixels.GetLength(1) - 1; x++)
                        {
                            var pixel = Pixels[y, x];
                            pixel.ForegroundColor = value;
                            Pixels[y, x] = pixel;
                        }
                    }

                    _textColor = value;
                }
            }

            private string[] DisplayedLines = Array.Empty<string>();
            private string? displayedText; // Only useful when adding text

            /// <summary>
            /// Shows <paramref name="text"/> onto the window.
            /// </summary>
            public void ShowText(string text, bool refresh) => ShowText_Base(text, null, refresh);

            /// <summary>
            /// Shows <paramref name="text"/> onto the window in the color <paramref name="consoleColor"/>
            /// </summary>
            public void ShowText(string text, ConsoleColor consoleColor, bool refresh) => ShowText(text, consoleColor.ToColor(), refresh);

            /// <summary>
            /// Shows text onto the window in the <paramref name="color"/>
            /// </summary>
            public void ShowText(string text, Color color, bool refresh)
            {
                TextColor = color;
                ShowText_Base(text, null, refresh);
            }

            /// <summary>
            /// Shows <paramref name="text"/> onto the window. For each <see cref="char"/>, <paramref name="getColor"/> is called
            /// to determine the color of the <see cref="char"/>.
            /// </summary>
            public void ShowText(string text, Func<int, int, char, Color> getColor, bool refresh) => ShowText_Base(text, getColor, refresh);

            private void ShowText_Base(string text, Func<int, int, char, Color>? getColor, bool refresh)
            {
                // Optimisation:
                // Calculate the amount of chars needed to be displayed vs the amount needed to be erased
                var lines = text.ConvertToWarpText(Width - 1, Height - 1);

                var linesToSkip = new bool[lines.Length];

                for (int i = 0; i < lines.Length; i++)
                {
                    if (DisplayedLines.Length <= i)
                        break;

                    linesToSkip[i] = lines[i].Equals(DisplayedLines[i]);
                }

                bool hasCustomColor = getColor != null;

                // For each character in each line
                for (int y = 0; y < lines.Length; y++)
                {
                    if (y >= Height - 1)
                        break;

                    if (linesToSkip[y] && !hasCustomColor)
                        continue;

                    int x = !hasCustomColor &&
                        DisplayedLines.Length > y &&
                        lines[y].StartsWith(DisplayedLines[y]) ? DisplayedLines[y].Length : 0;

                    PrintLine(lines[y], y, x, getColor);

                    // If the length of the line is shorter than the previous one
                    if (DisplayedLines.Length > y && DisplayedLines[y].Length > lines[y].Length)
                    {
                        // Clear the extra
                        PrintLine(DisplayedLines[y], y, lines[y].Length, getColor, ' ');
                    }
                }

                // If the amount of lines is shorter than the previous one
                if (DisplayedLines.Length > lines.Length && lines.Length < Height)
                {
                    // Clear the extra
                    for (int y = lines.Length; y < DisplayedLines.Length; ++y)
                    {
                        PrintLine(DisplayedLines[y], y, 0, getColor, ' ');
                    }
                }

                displayedText = text;
                DisplayedLines = lines;

                if (refresh)
                    ShowPixels();
            }

            private void PrintLine(string text, int y, int x, Func<int, int, char, Color>? colorRule, char? overrideChar = null)
            {
                var offset = Offset;
                var realY = y + offset.Y;

                for (_ = x; x < text.Length; ++x)
                {
                    var realX = x + offset.X;

                    char c = overrideChar ?? text[x];

                    // Get color
                    var charColor = colorRule?.Invoke(x, y, c) ?? Pixels[realY, realX].ForegroundColor;

                    // Get the color of the character
                    SetPixel(realX, realY, c, charColor);
                }
            }

            // Distance between the position of the window and the position of the text
            private Point Offset = new(2, 1);

            /// <summary>
            /// Clears the text of this window
            /// </summary>
            /// <param name="refresh"></param>
            public void ClearText(bool refresh) => ShowText("", refresh);

            //public void AddText(object text, bool addToStart, bool refresh)
            //{
            //    string textString = text.ToString();
            //    ShowText(addToStart ? textString + displayedText : displayedText + textString, refresh);
            //}

            #endregion Text

            #region Pixels

            /// <returns>Does a pixel exist at (<paramref name="x"/>; <paramref name="y"/>)</returns>
            private bool DoesPixelExist(int x, int y) => Pixels.GetLength(0) > y && y >= 0 && Pixels.GetLength(1) > x && x >= 0;

            /// <summary>
            /// Sets the pixel at (<paramref name="x"/>; <paramref name="y"/>) the given data
            /// </summary>
            private void SetPixel(
                int x, 
                int y, 
                char c, 
                Color foreground, 
                Color? background = null, 
                FontStyle? styles = null)
            {
                // Check if pixel exists
                if (!DoesPixelExist(x, y))
                    return;

                Pixel pixelToModify = Pixels[y, x];
                pixelToModify.SetCharacter(c);
                pixelToModify.ForegroundColor = foreground;
                
                if (background != null)
                    pixelToModify.BackgroundColor = background.Value;

                if (styles != null)
                    pixelToModify.SetStyles(styles.Value);

                Pixels[y, x] = pixelToModify;

                NeedsUpdate = true;
            }

            /// <summary>
            /// Draws the pixel at the given position
            /// </summary>
            private void DrawPixel(int relativeX, int relativeY)
            {
                // Check if it covered or if the point is outisde the viewrect
                var absPos = new Point(relativeX + this.Position.X, relativeY + this.Position.Y);

                if (IsPointCovered(absPos) || !this.Window.Contains(absPos))
                    return;

                // Check if a pixel exists at this given position
                if (!DoesPixelExist(relativeX, relativeY))
                    return;

                Pixel pixelToDraw = this.Pixels[relativeY, relativeX];

                // Check if the pixel changed since last draw
                if (!pixelToDraw.NeedsUpdate)
                    return;

                // Move cursor to position
                Console.SetCursorPosition(absPos.X, absPos.Y);

                // Draw pixel
                this.Pixels[relativeY, relativeX] = pixelToDraw.DrawPixel();
            }

            /// <summary>
            /// Updates this window onto the screen
            /// </summary>
            /// <remarks>
            /// This is automatically called on <see cref="UpdateRects"/>
            /// </remarks>
            public void ShowPixels()
            {
                // If the rect doesn't need to update or if the is inactive
                if (!NeedsUpdate || !isActive)
                    return;

                // Saved original position
                int prevLeft = Console.CursorLeft;
                int prevTop = Console.CursorTop;

#pragma warning disable CA1416 // Validate platform compatibility
                bool prevVisibility = Console.CursorVisible;
#pragma warning restore CA1416 // Validate platform compatibility

                Console.CursorVisible = false;

                // Draw each pixel
                for (int y = 0; y < Height; ++y)
                {
                    for (int x = 0; x < Width; ++x)
                    {
                        this.DrawPixel(x, y);
                    }
                }

                // Return to original position
                Console.SetCursorPosition(prevLeft, prevTop);

                Console.CursorVisible = prevVisibility;
            }

            /// <returns>Is <paramref name="point"/> covered by another window</returns>
            private bool IsPointCovered(Point point)
            {
                // Check if point is outside drawable zone
                if (Console.BufferWidth <= point.X || Console.BufferHeight <= point.Y)
                    return true;

                foreach (var item in Overlays)
                {
                    // If the current overlay is the caller
                    if (item == this)
                        break;

                    if (item.Window.Contains(point))
                        return true;
                }
                return false;
            }

            /// <summary>
            /// States if this window needs to be updated
            /// </summary>
            public bool NeedsUpdate { get; private set; } = true;

            #endregion Pixels

            #region Name

            /// <summary>
            /// Name of this window
            /// </summary>
            private string _name;

            public string Name
            {
                get => _name;
                set
                {
                    // If value isn't the same name
                    // If the value isn't empty
                    if (_name != value && !string.IsNullOrEmpty(value))
                    {
                        FormattedName ??= FormatName(value, Width);

                        var pos = Offset.X;

                        FormattedName.ForEach((c) =>
                        {
                            SetPixel(pos, 0, c, Color.Wheat, BackgroundColor);

                            ++pos;
                        });
                    }

                    _name = value;
                }
            }

            /// <summary>
            /// Name of this window formatted
            /// </summary>
            public string FormattedName { get; set; } = null;

            #endregion Name

            #region Frame
            // Characters used to display the frame of the window
            private const char SideBar = '█';
            private const char TopBar = '▀';
            private const char BottomBar = '▄';

            private const char TopLeftCorner = '█';
            private const char TopRightCorner = '█';
            private const char BottomLeftCorner = '█';
            private const char BottomRightCorner = '█';
            // ---

            /// <summary>
            /// Color of the frame of this window
            /// </summary>
            private Color _frameColor;

            /// <inheritdoc cref="_frameColor"/>
            public Color FrameColor
            {
                get => _frameColor;
                set
                {
                    if (Width > 0 && Height > 0)
                    {
                        Point nameEndPoint;

                        if (!string.IsNullOrEmpty(Name))
                        {
                            FormattedName ??= FormatName(Name, Width);

                            SetPixel(1, 0, TopBar, value);
                            nameEndPoint = new Point(FormattedName.Length + 2, 0);
                        }
                        else
                            nameEndPoint = new Point(1, 0);

                        // Borders
                        DrawLine(nameEndPoint, new Point(Width - 1, 0), this, TopBar, value); // Top
                        DrawLine(new Point(Width - 1, 1), new Point(Width - 1, Height - 1), this, SideBar, value); // Right
                        DrawLine(new Point(0, 1), new Point(0, Height - 1), this, SideBar, value); // Left
                        DrawLine(new Point(1, Height - 1), new Point(Width - 1, Height - 1), this, BottomBar, value); // Bottom

                        // Corners
                        SetPixel(0, 0, TopLeftCorner, value);
                        SetPixel(0, Height - 1, BottomLeftCorner, value);
                        SetPixel(Width - 1, 0, TopRightCorner, value);
                        SetPixel(Width - 1, Height - 1, BottomRightCorner, value);
                    }

                    _frameColor = value;
                }
            }

            #endregion Frame

            #region Background

            /// <summary>
            /// Color of the background of this window
            /// </summary>
            private Color _backgroundColor;

            /// <inheritdoc cref="_backgroundColor"/>
            public Color BackgroundColor
            {
                get => _backgroundColor;
                set
                {
                    for (int y = 0; y < Pixels.GetLength(0); y++)
                    {
                        for (int x = 0; x < Pixels.GetLength(1); x++)
                        {
                            var pixel = Pixels[y, x];
                            pixel.BackgroundColor = value;
                            pixel.SetCharacter(pixel.Character == default ? ' ' : pixel.Character);
                            Pixels[y, x] = pixel;
                        }
                    }

                    _backgroundColor = value;
                }
            }

            #endregion Background

            #region Depth

            /// <summary>
            /// Depth of this window
            /// </summary>
            private int Depth;

            #endregion Depth

            /// <inheritdoc/>
            ~WindowRect() => RemoveWindow(this);

            #region Static

            #region Draw

            /// <summary>
            /// Draws a line from <paramref name="origin"/> to <paramref name="destination"/>
            /// </summary>
            // https://stackoverflow.com/a/11683720
            internal static void DrawLine(Point origin, Point destination, WindowRect window, char dot = '#', Color foreground = default, Color background = default)
            {
                int x = origin.X;
                int y = origin.Y;

                int x2 = destination.X;
                int y2 = destination.Y;

                int w = x2 - x;
                int h = y2 - y;

                int dy2 = 0,
                    dx1 = w != 0 ? Math.Sign(w) : 0,
                    dy1 = h != 0 ? Math.Sign(h) : 0,
                    dx2 = w != 0 ? Math.Sign(w) : 0;

                int longest = Math.Abs(w);
                int shortest = Math.Abs(h);

                if (!(longest > shortest))
                {
                    (longest, shortest) = (shortest, longest);

                    dy2 = h != 0 ? Math.Sign(h) : dy2;
                    dx2 = 0;
                }

                int numerator = longest >> 1;
                for (int i = 0; i != longest; ++i)
                {
                    window.SetPixel(x, y, dot, foreground, background);
                    //putpixel(x, y, color);
                    numerator += shortest;
                    if (!(numerator < longest))
                    {
                        numerator -= longest;
                        x += dx1;
                        y += dy1;
                    }
                    else
                    {
                        x += dx2;
                        y += dy2;
                    }
                }

                //int deltaX = Math.Abs(destination.X - origin.X);
                //int deltaY = Math.Abs(destination.Y - origin.Y);

                ////if (origin.X > destination.X || origin.Y > destination.Y)
                ////    origin = destination;

                //var linePoint = new Point(origin.X, origin.Y);

                //for (int i = 0; i <= deltaX; i++)
                //{
                //    linePoint.X = origin.X + i;

                //    window.SetPixel(linePoint.X, linePoint.Y, dot, foreground, background);
                //}

                //for (int i = 0; i <= deltaY; i++)
                //{
                //    linePoint.Y = origin.Y + i;

                //    window.SetPixel(linePoint.X, linePoint.Y, dot, foreground, background);
                //}
            }

            #endregion Draw

            #region Rect

            private static readonly List<WindowRect> Overlays = new();

            /// <summary>
            /// Adds <paramref name="rect"/> to the window pile
            /// </summary>
            /// <param name="rect"></param>
            private static void AddWindow(WindowRect rect)
            {
                int index = -1;

                for (int i = 0; i < Overlays.Count; i++)
                {
                    if (Overlays[i].Depth <= rect.Depth)
                    {
                        index = i;
                        break;
                    }
                }

                Overlays.Insert(index == -1 ? Overlays.Count : index, rect);
            }

            private static void RemoveWindow(WindowRect rect) => Overlays.Remove(rect);

            public static void UpdateRects() => Overlays.ForEach(rect => rect.ShowPixels());

            #endregion Rect

            private static string FormatName(string Name, int Width)
            {
                int maxNameLength = Width - 6 > 10 ? 10 : Width - 6;
                return maxNameLength > 0 ?
                    $" {(Name.Length <= maxNameLength ? Name : Name.Substring(0, maxNameLength))} " :
                    "";
            }

            #endregion Static
        }

        /// <summary>
        /// Affiche la question donnée en écrivant la question dans la couleur assignée aux questions (visuellement plus agréable, mais nécessaire)
        /// </summary>
        public static void ShowConsoleMessage(object message, bool useWriteLine, Color foreground, Color background)
        {
            Console.Write(foreground.ForeColor() + background.BackColor() + message.ToString());

            if (useWriteLine)
                Console.WriteLine();

            Console.Write(ResetColor());
        }
    }
}