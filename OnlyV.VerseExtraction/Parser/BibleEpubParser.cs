﻿namespace OnlyV.VerseExtraction.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Serilog;

    internal sealed class BibleEpubParser : IDisposable
    {
        private const char Ellipsis = '…';
        
        private readonly EpubAsArchive _epub;
        private readonly Lazy<IReadOnlyList<BibleBook>> _bibleBooks;
        private readonly Lazy<List<BookChapter>> _bookChapters;

        public BibleEpubParser(string epubPath)
        {
            _epub = new EpubAsArchive(epubPath);
            
            _bibleBooks = new Lazy<IReadOnlyList<BibleBook>>(GenerateBibleBooksList);
            _bookChapters = new Lazy<List<BookChapter>>(GenerateChapterList);
        }

        public void Dispose()
        {
            _epub?.Dispose();
        }

        public IReadOnlyCollection<BibleBookData> ExtractBookData()
        {
            var result = new List<BibleBookData>();

            foreach (var book in _bibleBooks.Value)
            {
                var rec = new BibleBookData
                {
                    Name = book.BookName,
                    Number = book.BookNumber
                };

                var chapters = _bookChapters.Value.Where(x => x.Book.Equals(book));
                foreach (var chapter in chapters)
                {
                    rec.AddChapter(chapter.Chapter, chapter.VerseRange);
                }

                result.Add(rec);
            }

            return result;
        }

        public string ExtractVerseText(int bibleBook, int chapter, int verse)
        {
            throw new NotImplementedException();
        }

        private List<BookChapter> GenerateChapterList()
        {
            Log.Logger.Information("Generating chapter list");
            return _epub.GenerateBibleChaptersList(_bibleBooks.Value);
        }

        private IReadOnlyList<BibleBook> GenerateBibleBooksList()
        {
            Log.Logger.Information("Initialising books");
            return _epub.GenerateBibleBooksList();
        }
    }
}