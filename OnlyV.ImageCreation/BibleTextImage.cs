﻿namespace OnlyV.ImageCreation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Imaging;
    using OnlyV.ImageCreation.Utils;
    using OnlyV.VerseExtraction.Models;
    using TextSplitting;
    using VerseExtraction;

    public class BibleTextImage
    {
        private const char Ellipsis = '…';
        private const double VerseFontSizeFactor = 0.6;
        private Color _continuationArrowColor;
        private double _continuationArrowOpacity;

        private Color _backgroundColor;
        private Brush _backgroundBrush;
        private Brush _continuationBrush;

        public BibleTextImage()
        {
            Width = 1920;
            Height = 1080;
            TopMargin = 100;
            BottomMargin = 100;
            LeftMargin = 160;
            RightMargin = 160;

            MainFont = new FontDefinition
            {
                FontSize = 96,
                FontStyle = FontStyles.Normal,
                FontFamily = new FontFamily("Georgia"),
                FontColor = Colors.White
            };

            BackgroundColor = FromHtmlString("#275DAD");

            ShowVerseNumbers = true;
            HorzAlignment = TextAlignment.Center;

            LineSpacing = LineSpacing.Normal;

            ShowTitle = true;

            TitleFont = new FontDefinition
            {
                FontSize = (MainFont.FontSize * 3) / 4,
                FontFamily = MainFont.FontFamily,
                FontStyle = FontStyles.Italic,
                FontColor = FromHtmlString("#FCBA04")
            };

            TitlePosition = TitlePosition.Bottom;
            TitleHorzAlignment = TextAlignment.Right;

            VerseFont = new FontDefinition
            {
                FontSize = MainFont.FontSize * VerseFontSizeFactor,
                FontFamily = MainFont.FontFamily,
                FontColor = FromHtmlString("#FCBA04"),
                FontStyle = FontStyles.Normal
            };

            CultureInfo = CultureInfo.InvariantCulture;
            FlowDirection = FlowDirection.LeftToRight;
            AllowAutoFit = true;
            ShowBreakInVerses = true;
            ShowContinuationArrow = true;

            ContinuationArrowOpacity = 1.0;
            ContinuationArrowColor = FromHtmlString("#FCBA04");

            TitleDropShadow = true;
            TitleDropShadowColor = Colors.Black;
            TitleDropShadowOpacity = 0.5;
            TitleDropShadowBlurRadius = 10;
            TitleDropShadowDepth = 10;

            BodyDropShadow = true;
            BodyDropShadowColor = Colors.Black;
            BodyDropShadowOpacity = 0.5;
            BodyDropShadowBlurRadius = 10;
            BodyDropShadowDepth = 10;

            UseTildeParaSeparator = true;
            TrimPunctuation = false;
            TrimQuotes = false;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public FontDefinition MainFont { get; }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    _backgroundBrush = null;
                }
            }
        }

        public bool ShowVerseNumbers { get; set; }

        public TextAlignment HorzAlignment { get; set; }

        public int TopMargin { get; set; }

        public int BottomMargin { get; set; }

        public int LeftMargin { get; set; }

        public int RightMargin { get; set; }

        public LineSpacing LineSpacing { get; set; }

        public bool ShowTitle { get; set; }

        public FontDefinition TitleFont { get; }

        public TitlePosition TitlePosition { get; set; }

        public TextAlignment TitleHorzAlignment { get; set; }

        public FlowDirection FlowDirection { get; set; }

        public FontDefinition VerseFont { get; }

        public CultureInfo CultureInfo { get; set; }

        public string BackgroundImageFile { get; set; }

        public bool AllowAutoFit { get; set; }

        public bool ShowBreakInVerses { get; set; }

        public bool ShowContinuationArrow { get; set; }

        public bool UseTildeParaSeparator { get; set; }

        public bool TrimPunctuation { get; set; }

        public bool TrimQuotes { get; set; }

        public Color ContinuationArrowColor
        {
            get => _continuationArrowColor;
            set
            {
                if (_continuationArrowColor != value)
                {
                    _continuationArrowColor = value;
                    _continuationBrush = null;
                }
            }
        }

        public double ContinuationArrowOpacity
        {
            get => _continuationArrowOpacity;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_continuationArrowOpacity != value)
                {
                    _continuationArrowOpacity = value;
                    _continuationBrush = null;
                }
            }
        }

        public bool TitleDropShadow { get; set; }

        public Color TitleDropShadowColor { get; set; }

        public double TitleDropShadowOpacity { get; set; }

        public double TitleDropShadowBlurRadius { get; set; }

        public double TitleDropShadowDepth { get; set; }

        public bool BodyDropShadow { get; set; }

        public Color BodyDropShadowColor { get; set; }

        public double BodyDropShadowOpacity { get; set; }

        public double BodyDropShadowBlurRadius { get; set; }

        public double BodyDropShadowDepth { get; set; }

        private Brush BackgroundBrush => _backgroundBrush ?? (_backgroundBrush = new SolidColorBrush(BackgroundColor));

        private Brush ContinuationBrush => _continuationBrush ?? (_continuationBrush = new SolidColorBrush(ContinuationArrowColor) { Opacity = ContinuationArrowOpacity });

        public IEnumerable<BitmapSource> Generate(string epubPath, int bookNumber, string chapterAndVerses)
        {
            NormaliseMargins();

            VerseFont.FontSize = MainFont.FontSize * VerseFontSizeFactor;

            using (var reader = new BibleTextReader(epubPath))
            {
                string title = reader.GenerateVerseTitle(bookNumber, chapterAndVerses);

                var formattingOptions = new FormattingOptions
                {
                    IncludeVerseNumbers = ShowVerseNumbers,
                    ShowBreakInVerses = ShowBreakInVerses,
                    TrimPunctuation = TrimPunctuation,
                    TrimQuotes = TrimQuotes,
                    UseTildeSeparator = UseTildeParaSeparator
                };

                var text = reader.ExtractVerseText(bookNumber, chapterAndVerses, formattingOptions);

                var splitter = new TextSplitter(text, MeasureAString);
                var lines = splitter.GetLines(Width - LeftMargin - RightMargin);

                var originalFontSize = MainFont.FontSize;

                if (AllowAutoFit)
                {
                    lines = TryAutoFit(lines, title, splitter);
                }

                var result = InternalGenerate(lines, title);
                MainFont.FontSize = originalFontSize;
                VerseFont.FontSize = MainFont.FontSize * VerseFontSizeFactor;

                return result;
            }
        }

        private Size MeasureAString(string s)
        {
            FormattedText text = GetFormattedBodyText(s);
            return new Size(text.Width, text.Height);
        }

        private IReadOnlyCollection<string> TryAutoFit(IReadOnlyCollection<string> lines, string title, TextSplitter splitter)
        {
            int linesPerImage = CalcLinesPerImage(title);
            int origImageCount = CalcNumberBitmaps(linesPerImage, lines);
            if (origImageCount > 1)
            {
                var originalFontSize = MainFont.FontSize;

                const int MaxAttempts = 3;
                int attempts = 0;

                int numImages = origImageCount;
                while (numImages == origImageCount && attempts < MaxAttempts)
                {
                    // reduce the font size a little...
                    MainFont.FontSize = 0.975 * MainFont.FontSize;
                    VerseFont.FontSize = MainFont.FontSize * VerseFontSizeFactor;

                    lines = splitter.GetLines(Width - LeftMargin - RightMargin);

                    linesPerImage = CalcLinesPerImage(title);
                    numImages = CalcNumberBitmaps(linesPerImage, lines);

                    ++attempts;
                }

                if (origImageCount == numImages)
                {
                    // no change...
                    MainFont.FontSize = originalFontSize;
                    VerseFont.FontSize = MainFont.FontSize * VerseFontSizeFactor;
                    lines = splitter.GetLines(Width - LeftMargin - RightMargin);
                }
            }

            return lines;
        }

        private FormattedText GetFormattedVerseNumber(int verse, bool startOfLine)
        {
            var s = startOfLine ? $"{verse} " : $"  {verse} ";
            
            var tf = VerseFont.GetTypeface();
            return new FormattedText(
                s, 
                CultureInfo, 
                FlowDirection, 
                tf, 
                VerseFont.FontSize, 
                VerseFont.GetBrush());
        }

        private FormattedText GetFormattedTilde()
        {
            var tf = MainFont.GetTypeface();
            var fontSize = MainFont.FontSize / 2;

            return new FormattedText(
                " ~ ", 
                CultureInfo, 
                FlowDirection, 
                tf, 
                fontSize, 
                MainFont.GetBrush());
        }

        private FormattedText GetFormattedBodyText(string text)
        {
            var tf = MainFont.GetTypeface();
            return new FormattedText(
                text, 
                CultureInfo, 
                FlowDirection, 
                tf, 
                MainFont.FontSize, 
                MainFont.GetBrush());
        }

        private FormattedText GetFormattedTitleText(string text)
        {
            var tf = TitleFont.GetTypeface();
            var result = new FormattedText(
                text, 
                CultureInfo, 
                FlowDirection, 
                tf, 
                TitleFont.FontSize, 
                TitleFont.GetBrush());

            result.Trimming = TextTrimming.CharacterEllipsis;
            return result;
        }
        
        private IReadOnlyCollection<string> GetBatchOfLines(IReadOnlyCollection<string> lines, int batch, int batchSize)
        {
            if (batch >= 0 && (batch * batchSize) < lines.Count)
            {
                return lines.Skip(batch * batchSize).Take(batchSize).ToList();
            }

            return null;
        }

        private double GetSpaceForTitle(string title)
        {
            var titleFormattedText = GetFormattedTitleText(title);
            return ShowTitle ? titleFormattedText.Height * 2 : 0;
        }

        private int CalcLinesPerImage(string title)
        {
            var bodyLineHeight = AdjustHeightForLineSpacing(GetFormattedBodyText("My").Height);
            var spaceForTitle = GetSpaceForTitle(title);
            var availableHeight = Height - TopMargin - BottomMargin - spaceForTitle;
            return (int)(availableHeight / bodyLineHeight);
        }

        private IReadOnlyCollection<BitmapSource> InternalGenerate(IReadOnlyCollection<string> lines, string title)
        {
            var result = new List<BitmapSource>();

            if (lines.Any())
            {
                var bodyLineHeight = AdjustHeightForLineSpacing(GetFormattedBodyText("My").Height);
                var titleFormattedText = GetFormattedTitleText(title);
                var spaceForTitle = GetSpaceForTitle(title);

                int linesPerImage = CalcLinesPerImage(title);
                int numBitmaps = CalcNumberBitmaps(linesPerImage, lines);

                for (int n = 0; n < numBitmaps; ++n)
                {
                    var bmp = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Default);

                    var backgroundVisual = DrawBackground();
                    var bodyVisual = DrawBody(n, numBitmaps, lines, linesPerImage, bodyLineHeight, spaceForTitle);
                    var titleVisual = DrawTitle(titleFormattedText);

                    bmp.Render(backgroundVisual);
                    bmp.Render(bodyVisual);

                    if (titleVisual != null)
                    {
                        bmp.Render(titleVisual);
                    }

                    result.Add(bmp);
                }
            }

            return result;
        }

        private DrawingVisual DrawTitle(FormattedText titleFormattedText)
        {
            if (ShowTitle)
            {
                var visual = new DrawingVisual();
                using (var c = visual.RenderOpen())
                {
                    if (TitleDropShadow)
                    {
                        visual.Effect = new DropShadowEffect
                        {
                            Color = TitleDropShadowColor,
                            Opacity = TitleDropShadowOpacity,
                            BlurRadius = TitleDropShadowBlurRadius,
                            ShadowDepth = TitleDropShadowDepth
                        };
                    }

                    c.DrawText(titleFormattedText, GetTitlePos(titleFormattedText));
                }

                return visual;
            }

            return null;
        }

        private Point GetTitlePos(FormattedText titleFormattedText)
        {
            double x;

            switch (TitleHorzAlignment)
            {
                case TextAlignment.Left:
                    x = LeftMargin;
                    break;

                case TextAlignment.Center:
                    x = LeftMargin + ((Width - LeftMargin - RightMargin - titleFormattedText.Width) / 2);
                    break;

                default: // right
                    x = Width - RightMargin - titleFormattedText.Width;
                    break;
            }

            var y = TitlePosition == TitlePosition.Top ? TopMargin : Height - BottomMargin - titleFormattedText.Height;
            return new Point(x, y);
        }

        private DrawingVisual DrawBody(
            int imageNumber, 
            int totalImageCount, 
            IReadOnlyCollection<string> lines,
            int linesPerImage, 
            double lineHeight, 
            double spaceForTitle)
        {
            DrawingVisual visual = new DrawingVisual();

            if (BodyDropShadow)
            {
                visual.Effect = new DropShadowEffect
                {
                    Color = BodyDropShadowColor,
                    Opacity = BodyDropShadowOpacity,
                    BlurRadius = BodyDropShadowBlurRadius,
                    ShadowDepth = BodyDropShadowDepth
                };
            }

            using (var c = visual.RenderOpen())
            {
                var linesForBmp = GetBatchOfLines(lines, imageNumber, linesPerImage).ToArray();
                var linesForPreviousBmp = GetBatchOfLines(lines, imageNumber - 1, linesPerImage);

                int lineCount = linesForBmp.Length;
                double adjustmentForShortLineCount = 0;
                if (lineCount < linesPerImage)
                {
                    adjustmentForShortLineCount = (lineHeight * (linesPerImage - lineCount)) / 2;
                }

                var moreImagesAfterThis = imageNumber < totalImageCount - 1;

                var adjustmentForTitleAtTop = ShowTitle && TitlePosition == TitlePosition.Top ? spaceForTitle : 0;
                for (var lineNum = 0; lineNum < lineCount; ++lineNum)
                {
                    var lineStr = linesForBmp[lineNum];

                    if (lineNum == 0 && lineStr.Length > 0 && !IsEndOfSentenceOrPhrase(linesForPreviousBmp?.LastOrDefault()))
                    {
                        // ellipises at start of this line...
                        lineStr = string.Concat(Ellipsis, lineStr);
                    }

                    if (lineNum == linesForBmp.Length - 1 && moreImagesAfterThis && !IsEndOfSentenceOrPhrase(lineStr))
                    {
                        // ellipsis at the end of the last line (if appropriate)...
                        lineStr = string.Concat(lineStr, Ellipsis);
                    }

                    var y = TopMargin + adjustmentForTitleAtTop + adjustmentForShortLineCount + (lineNum * lineHeight);
                    DrawTextLine(c, lineStr, y);

                    if (ShowContinuationArrow && lineNum == linesForBmp.Length - 1 && moreImagesAfterThis)
                    {
                        DrawContinuationArrow(c, lineHeight, spaceForTitle);
                    }
                }
            }

            return visual;
        }

        private void DrawContinuationArrow(DrawingContext c, double lineHeight, double spaceForTitle)
        {
            var height = lineHeight / 2;
            var width = height;

            var xStart = FlowDirection == FlowDirection.LeftToRight
               ? Width - RightMargin - width
               : LeftMargin + width;

            var yStart = Height - BottomMargin - (spaceForTitle / 3);

            if (ShowTitle && TitlePosition == TitlePosition.Bottom)
            {
                yStart -= lineHeight;
            }

            var start = new Point(xStart, yStart);

            var segments = new[]
            {
                new LineSegment(new Point(xStart, yStart + height), false),
                new LineSegment(new Point(xStart + width, yStart + (height / 2)), false)
            };

            var figure = new PathFigure(start, segments, true);
            var geo = new PathGeometry(new[] { figure });
            c.DrawGeometry(ContinuationBrush, null, geo);
        }

        private bool IsEndOfSentenceOrPhrase(string lastOrDefault)
        {
            if (lastOrDefault != null)
            {
                char ch = lastOrDefault.Trim().LastOrDefault();
                if (ch == '.' || ch == ',' || ch == ';' || ch == ':' || ch == '-' || ch == '—')
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        private DrawingVisual DrawBackground()
        {
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                var rect = new Rect(new Size(Width, Height));

                if (BackgroundImageFile != null && File.Exists(BackgroundImageFile))
                {
                    Uri uri = new Uri(BackgroundImageFile);
                    ImageSource src = new BitmapImage(uri);
                    c.DrawImage(src, rect);
                }
                else
                {
                    c.DrawRectangle(BackgroundBrush, null, rect);
                }
            }

            return visual;
        }

        private void DrawTextLine(DrawingContext c, string line, double y)
        {
            var startOfLine = true;
            var tokens = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            var totalLineWidth = 0.0;
            var segments = new List<FormattedText>();
            var verseNumberSegments = new List<FormattedText>();
            var tildeSegments = new List<FormattedText>();

            foreach (var token in tokens)
            {
                FormattedText t;

                if (int.TryParse(token, out var verseNum))
                {
                    t = GetFormattedVerseNumber(verseNum, startOfLine);
                    verseNumberSegments.Add(t);
                    segments.Add(t);
                    totalLineWidth += t.WidthIncludingTrailingWhitespace;
                }
                else
                {
                    var poeticTokens = token.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                    for (var n = 0; n < poeticTokens.Length; ++n)
                    {
                        t = GetFormattedBodyText(poeticTokens[n]);
                        segments.Add(t);
                        totalLineWidth += t.WidthIncludingTrailingWhitespace;

                        if (n < poeticTokens.Length - 1)
                        {
                            t = GetFormattedTilde();
                            tildeSegments.Add(t);
                            segments.Add(t);
                            totalLineWidth += t.WidthIncludingTrailingWhitespace;
                        }
                    }
                }

                startOfLine = false;
            }

            foreach (var seg in segments)
            {
                var x = GetSegmentStartX(seg, segments, totalLineWidth);
                var y2 = y;

                if (verseNumberSegments.Contains(seg))
                {
                    y2 -= seg.Height / 6;
                }
                else if (tildeSegments.Contains(seg))
                {
                    y2 += seg.Height / 2;
                }

                c.DrawText(seg, new Point(x, y2));
            }
        }

        private double GetSegmentStartX(
            FormattedText seg, 
            IReadOnlyCollection<FormattedText> segments, 
            double totalLineWidth)
        {
            double x = 0.0;

            foreach (var segment in segments)
            {
                if (seg == segment)
                {
                    break;
                }

                x += segment.WidthIncludingTrailingWhitespace;
            }

            switch (HorzAlignment)
            {
                case TextAlignment.Right:
                    return Width - RightMargin - totalLineWidth + x;

                case TextAlignment.Left:
                    return LeftMargin + x;

                default:
                case TextAlignment.Center:
                    return LeftMargin + x + ((Width - LeftMargin - RightMargin - totalLineWidth) / 2);
            }
        }
        
        private double AdjustHeightForLineSpacing(double height)
        {
            switch (LineSpacing)
            {
                case LineSpacing.VerySmall:
                    return height * 0.8;
                case LineSpacing.Small:
                    return height * 0.9;
                case LineSpacing.Large:
                    return height * 1.1;
                case LineSpacing.VeryLarge:
                    return height * 1.2;
            }

            return height;
        }

        private int CalcNumberBitmaps(int linesPerImage, IEnumerable<string> lines)
        {
            int numLines = lines.Count();

            int result = numLines / linesPerImage;
            if (numLines % linesPerImage != 0)
            {
                ++result;
            }

            return result;
        }

        private void NormaliseMargins()
        {
            int vertMax = Height - 100;
            if (TopMargin > vertMax)
            {
                TopMargin = vertMax;
            }

            if (BottomMargin > vertMax)
            {
                BottomMargin = vertMax;
            }

            int horzMax = Width - 100;
            if (LeftMargin > horzMax)
            {
                LeftMargin = horzMax;
            }

            if (RightMargin > horzMax)
            {
                RightMargin = horzMax;
            }
        }

        private Color FromHtmlString(string htmlColor)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (Color)ColorConverter.ConvertFromString(htmlColor);
        }
    }
}
