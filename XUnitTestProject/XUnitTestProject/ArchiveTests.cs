using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.IO;
using SharpCompress.Readers;
using Xunit;

namespace SharpCompress.Test
{
    public class ArchiveTests : ReaderTests
    {
        protected void ArchiveStreamReadExtractAll(string testArchive, CompressionType compression)
        {
            testArchive = Path.Combine(TEST_ARCHIVES_PATH, testArchive);
            ArchiveStreamReadExtractAll(new String[] { testArchive }, compression);
        }


        protected void ArchiveStreamReadExtractAll(IEnumerable<string> testArchives, CompressionType compression)
        {
            foreach (var path in testArchives)
            {
                using (var stream = new NonDisposingStream(File.OpenRead(path), true))
                using (var archive = ArchiveFactory.Open(stream))
                {
                    Assert.True(archive.IsSolid);
                    using (var reader = archive.ExtractAllEntries())
                    {
                        UseReader(reader, compression);
                    }
                    VerifyFiles();

                    if (archive.Entries.First().CompressionType == CompressionType.Rar)
                    {
                        stream.ThrowOnDispose = false;
                        return;
                    }
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(SCRATCH_FILES_PATH,
                                               new ExtractionOptions
                                               {
                                                   ExtractFullPath = true,
                                                   Overwrite = true
                                               });
                    }
                    stream.ThrowOnDispose = false;
                }
                VerifyFiles();
            }
        }

        public void ArchiveStreamRead(string testArchive, ReaderOptions readerOptions = null)
        {
            testArchive = Path.Combine(TEST_ARCHIVES_PATH, testArchive);
            ArchiveStreamReadOption(readerOptions, new String[] { testArchive } );
        }

        public void ArchiveStreamRead(ReaderOptions readerOptions = null, params string[] testArchives)
        {
            ArchiveStreamReadOption(readerOptions, testArchives.Select(x => Path.Combine(TEST_ARCHIVES_PATH, x)).ToList());
        }

        public void ArchiveStreamReadOption(ReaderOptions readerOptions, IEnumerable<string> testArchives)
        {
            foreach (var path in testArchives)
            {
                using (var stream = new NonDisposingStream(File.OpenRead(path), true))
                using (var archive = ArchiveFactory.Open(stream, readerOptions))
                {
                    try
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            entry.WriteToDirectory(SCRATCH_FILES_PATH,
                                                   new ExtractionOptions()
                                                   {
                                                       ExtractFullPath = true,
                                                       Overwrite = true
                                                   });
                        }
                    }
                    catch (InvalidFormatException)
                    {
                        //rar SOLID test needs this
                        stream.ThrowOnDispose = false;
                        throw;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        //SevenZipArchive_BZip2_Split test needs this
                        stream.ThrowOnDispose = false;
                        throw;
                    }
                    stream.ThrowOnDispose = false;
                }
                VerifyFiles();
            }
        }

        public void ArchiveFileRead(string testArchive, ReaderOptions readerOptions = null)
        {
            testArchive = Path.Combine(TEST_ARCHIVES_PATH, testArchive);
            using (var archive = ArchiveFactory.Open(testArchive, readerOptions))
            {
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(SCRATCH_FILES_PATH,
                        new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                }
            }
            VerifyFiles();
        }

        /// <summary>
        /// Demonstrate the ExtractionOptions.PreserveFileTime and ExtractionOptions.PreserveAttributes extract options
        /// </summary>
        protected void ArchiveFileReadEx(string testArchive)
        {
            testArchive = Path.Combine(TEST_ARCHIVES_PATH, testArchive);
            using (var archive = ArchiveFactory.Open(testArchive))
            {
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(SCRATCH_FILES_PATH,
                        new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true,
                            PreserveAttributes = true,
                            PreserveFileTime = true
                        });
                }
            }
            VerifyFilesEx();
        }
    }
}
