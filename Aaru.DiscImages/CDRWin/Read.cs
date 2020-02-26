﻿// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads CDRWin cuesheets (cue/bin).
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Decoders.CD;
using Schemas;
using Session = DiscImageChef.CommonTypes.Structs.Session;

namespace DiscImageChef.DiscImages
{
    public partial class CdrWin
    {
        public bool Open(IFilter imageFilter)
        {
            if(imageFilter == null)
                return false;

            _cdrwinFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                _cueStream = new StreamReader(imageFilter.GetDataForkStream());
                int  lineNumber     = 0;
                bool inTrack        = false;
                byte currentSession = 1;

                // Initialize all RegExs
                var regexSession                = new Regex(REGEX_SESSION);
                var regexDiskType               = new Regex(REGEX_MEDIA_TYPE);
                var regexLeadOut                = new Regex(REGEX_LEAD_OUT);
                var regexLba                    = new Regex(REGEX_LBA);
                var regexDiskId                 = new Regex(REGEX_DISC_ID);
                var regexBarCode                = new Regex(REGEX_BARCODE);
                var regexComment                = new Regex(REGEX_COMMENT);
                var regexCdText                 = new Regex(REGEX_CDTEXT);
                var regexMcn                    = new Regex(REGEX_MCN);
                var regexTitle                  = new Regex(REGEX_TITLE);
                var regexGenre                  = new Regex(REGEX_GENRE);
                var regexArranger               = new Regex(REGEX_ARRANGER);
                var regexComposer               = new Regex(REGEX_COMPOSER);
                var regexPerformer              = new Regex(REGEX_PERFORMER);
                var regexSongWriter             = new Regex(REGEX_SONGWRITER);
                var regexFile                   = new Regex(REGEX_FILE);
                var regexTrack                  = new Regex(REGEX_TRACK);
                var regexIsrc                   = new Regex(REGEX_ISRC);
                var regexIndex                  = new Regex(REGEX_INDEX);
                var regexPregap                 = new Regex(REGEX_PREGAP);
                var regexPostgap                = new Regex(REGEX_POSTGAP);
                var regexFlags                  = new Regex(REGEX_FLAGS);
                var regexApplication            = new Regex(REGEX_APPLICATION);
                var regexTruripDisc             = new Regex(REGEX_TRURIP_DISC_HASHES);
                var regexTruripDiscCrc32        = new Regex(REGEX_TRURIP_DISC_CRC32);
                var regexTruripDiscMd5          = new Regex(REGEX_TRURIP_DISC_MD5);
                var regexTruripDiscSha1         = new Regex(REGEX_TRURIP_DISC_SHA1);
                var regexTruripTrack            = new Regex(REGEX_TRURIP_TRACK_METHOD);
                var regexTruripTrackCrc32       = new Regex(REGEX_TRURIP_TRACK_CRC32);
                var regexTruripTrackMd5         = new Regex(REGEX_TRURIP_TRACK_MD5);
                var regexTruripTrackSha1        = new Regex(REGEX_TRURIP_TRACK_SHA1);
                var regexTruripTrackUnknownHash = new Regex(REGEX_TRURIP_TRACK_UNKNOWN);
                var regexDicMediaType           = new Regex(REGEX_DIC_MEDIA_TYPE);
                var regexApplicationVersion     = new Regex(REGEX_APPLICATION_VERSION);
                var regexDumpExtent             = new Regex(REGEX_DUMP_EXTENT);

                // Initialize all RegEx matches
                Match matchTrack;

                // Initialize disc
                _discImage = new CdrWinDisc
                {
                    Sessions   = new List<Session>(), Tracks = new List<CdrWinTrack>(), Comment = "",
                    DiscHashes = new Dictionary<string, string>()
                };

                var currentTrack = new CdrWinTrack
                {
                    Indexes = new Dictionary<int, ulong>()
                };

                var   currentFile             = new CdrWinTrackFile();
                ulong currentFileOffsetSector = 0;

                int trackCount = 0;

                while(_cueStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = _cueStream.ReadLine();

                    matchTrack = regexTrack.Match(line);

                    if(!matchTrack.Success)
                        continue;

                    uint trackSeq = uint.Parse(matchTrack.Groups[1].Value);

                    if(trackCount + 1 != trackSeq)
                        throw new
                            FeatureUnsupportedImageException($"Found TRACK {trackSeq} out of order in line {lineNumber}");

                    trackCount++;
                }

                if(trackCount == 0)
                    throw new FeatureUnsupportedImageException("No tracks found");

                CdrWinTrack[] cueTracks = new CdrWinTrack[trackCount];

                lineNumber = 0;
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                _cueStream = new StreamReader(imageFilter.GetDataForkStream());

                var  filtersList       = new FiltersList();
                bool inTruripDiscHash  = false;
                bool inTruripTrackHash = false;

                while(_cueStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = _cueStream.ReadLine();

                    Match matchSession            = regexSession.Match(line);
                    Match matchDiskType           = regexDiskType.Match(line);
                    Match matchComment            = regexComment.Match(line);
                    Match matchLba                = regexLba.Match(line);
                    Match matchLeadOut            = regexLeadOut.Match(line);
                    Match matchApplication        = regexApplication.Match(line);
                    Match matchTruripDisc         = regexTruripDisc.Match(line);
                    Match matchTruripTrack        = regexTruripTrack.Match(line);
                    Match matchDicMediaType       = regexDicMediaType.Match(line);
                    Match matchApplicationVersion = regexApplicationVersion.Match(line);
                    Match matchDumpExtent         = regexDumpExtent.Match(line);

                    if(inTruripDiscHash)
                    {
                        Match matchTruripDiscCrc32 = regexTruripDiscCrc32.Match(line);
                        Match matchTruripDiscMd5   = regexTruripDiscMd5.Match(line);
                        Match matchTruripDiscSha1  = regexTruripDiscSha1.Match(line);

                        if(matchTruripDiscCrc32.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found REM CRC32 at line {0}", lineNumber);
                            _discImage.DiscHashes.Add("crc32", matchTruripDiscCrc32.Groups[1].Value.ToLowerInvariant());

                            continue;
                        }

                        if(matchTruripDiscMd5.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found REM MD5 at line {0}", lineNumber);
                            _discImage.DiscHashes.Add("md5", matchTruripDiscMd5.Groups[1].Value.ToLowerInvariant());

                            continue;
                        }

                        if(matchTruripDiscSha1.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found REM SHA1 at line {0}", lineNumber);
                            _discImage.DiscHashes.Add("sha1", matchTruripDiscSha1.Groups[1].Value.ToLowerInvariant());

                            continue;
                        }
                    }
                    else if(inTruripTrackHash)
                    {
                        Match matchTruripTrackCrc32       = regexTruripTrackCrc32.Match(line);
                        Match matchTruripTrackMd5         = regexTruripTrackMd5.Match(line);
                        Match matchTruripTrackSha1        = regexTruripTrackSha1.Match(line);
                        Match matchTruripTrackUnknownHash = regexTruripTrackUnknownHash.Match(line);

                        if(matchTruripTrackCrc32.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CRC32 for {1} {2} at line {0}",
                                                      lineNumber,
                                                      matchTruripTrackCrc32.Groups[1].Value == "Trk" ? "track" : "gap",
                                                      matchTruripTrackCrc32.Groups[2].Value);

                            continue;
                        }

                        if(matchTruripTrackMd5.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CRC32 for {1} {2} at line {0}",
                                                      lineNumber,
                                                      matchTruripTrackMd5.Groups[1].Value == "Trk" ? "track" : "gap",
                                                      matchTruripTrackMd5.Groups[2].Value);

                            continue;
                        }

                        if(matchTruripTrackSha1.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CRC32 for {1} {2} at line {0}",
                                                      lineNumber,
                                                      matchTruripTrackSha1.Groups[1].Value == "Trk" ? "track" : "gap",
                                                      matchTruripTrackSha1.Groups[2].Value);

                            continue;
                        }

                        if(matchTruripTrackUnknownHash.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin",
                                                      "Found unknown hash for {1} {2} at line {0}. Please report this disc image.",
                                                      lineNumber,
                                                      matchTruripTrackUnknownHash.Groups[1].Value == "Trk" ? "track"
                                                          : "gap", matchTruripTrackUnknownHash.Groups[2].Value);

                            continue;
                        }
                    }

                    inTruripDiscHash  = false;
                    inTruripTrackHash = false;

                    if(matchDumpExtent.Success                                                      &&
                       !inTrack                                                                     &&
                       ulong.TryParse(matchDumpExtent.Groups["start"].Value, out ulong extentStart) &&
                       ulong.TryParse(matchDumpExtent.Groups["end"].Value, out ulong extentEnd))
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM METADATA DUMP EXTENT at line {0}",
                                                  lineNumber);

                        if(DumpHardware is null)
                            DumpHardware = new List<DumpHardwareType>();

                        DumpHardwareType existingDump =
                            DumpHardware.FirstOrDefault(d =>
                                                            d.Manufacturer == matchDumpExtent.
                                                                              Groups["manufacturer"].Value         &&
                                                            d.Model    == matchDumpExtent.Groups["model"].Value    &&
                                                            d.Firmware == matchDumpExtent.Groups["firmware"].Value &&
                                                            d.Serial   == matchDumpExtent.Groups["serial"].Value   &&
                                                            d.Software.Name ==
                                                            matchDumpExtent.Groups["application"].Value &&
                                                            d.Software.Version == matchDumpExtent.
                                                                                  Groups["version"].Value &&
                                                            d.Software.OperatingSystem == matchDumpExtent.
                                                                                          Groups["os"].Value);

                        if(existingDump is null)
                        {
                            DumpHardware.Add(new DumpHardwareType
                            {
                                Extents = new[]
                                {
                                    new ExtentType
                                    {
                                        Start = extentStart, End = extentEnd
                                    }
                                },
                                Firmware     = matchDumpExtent.Groups["firmware"].Value,
                                Manufacturer = matchDumpExtent.Groups["manufacturer"].Value,
                                Model        = matchDumpExtent.Groups["model"].Value,
                                Serial       = matchDumpExtent.Groups["serial"].Value, Software = new SoftwareType
                                {
                                    Name            = matchDumpExtent.Groups["application"].Value,
                                    Version         = matchDumpExtent.Groups["version"].Value,
                                    OperatingSystem = matchDumpExtent.Groups["os"].Value
                                }
                            });
                        }
                        else
                        {
                            existingDump.Extents = new List<ExtentType>(existingDump.Extents)
                            {
                                new ExtentType
                                {
                                    Start = extentStart, End = extentEnd
                                }
                            }.OrderBy(e => e.Start).ToArray();
                        }
                    }
                    else if(matchDicMediaType.Success &&
                            !inTrack)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM METADATA DIC MEDIA-TYPE at line {0}",
                                                  lineNumber);

                        _discImage.DicMediaType = matchDicMediaType.Groups[1].Value;
                    }
                    else if(matchDiskType.Success &&
                            !inTrack)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM ORIGINAL MEDIA TYPE at line {0}",
                                                  lineNumber);

                        _discImage.OriginalMediaType = matchDiskType.Groups[1].Value;
                    }
                    else if(matchSession.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM SESSION at line {0}", lineNumber);
                        currentSession = byte.Parse(matchSession.Groups[1].Value);

                        // What happens between sessions
                    }
                    else if(matchLba.Success)
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM MSF at line {0}", lineNumber);
                    else if(matchLeadOut.Success)
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM LEAD-OUT at line {0}", lineNumber);
                    else if(matchApplication.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM Ripping Tool at line {0}", lineNumber);
                        _imageInfo.Application = matchApplication.Groups[1].Value;
                    }
                    else if(matchApplicationVersion.Success &&
                            !inTrack)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM Ripping Tool Version at line {0}",
                                                  lineNumber);

                        _imageInfo.ApplicationVersion = matchApplicationVersion.Groups[1].Value;
                    }
                    else if(matchTruripDisc.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM DISC HASHES at line {0}", lineNumber);
                        inTruripDiscHash = true;
                    }
                    else if(matchTruripTrack.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin",
                                                  "Found REM Gap Append Method: {1} [{2}] HASHES at line {0}",
                                                  lineNumber, matchTruripTrack.Groups[1].Value,
                                                  matchTruripTrack.Groups[2].Value);

                        inTruripTrackHash   = true;
                        _discImage.IsTrurip = true;
                    }
                    else if(matchComment.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM at line {0}", lineNumber);

                        if(_discImage.Comment == "")
                            _discImage.Comment = matchComment.Groups[1].Value; // First comment
                        else
                            _discImage.Comment +=
                                Environment.NewLine + matchComment.Groups[1].Value; // Append new comments as new lines
                    }
                    else
                    {
                        matchTrack = regexTrack.Match(line);
                        Match matchTitle      = regexTitle.Match(line);
                        Match matchSongWriter = regexSongWriter.Match(line);
                        Match matchPregap     = regexPregap.Match(line);
                        Match matchPostgap    = regexPostgap.Match(line);
                        Match matchPerformer  = regexPerformer.Match(line);
                        Match matchMcn        = regexMcn.Match(line);
                        Match matchIsrc       = regexIsrc.Match(line);
                        Match matchIndex      = regexIndex.Match(line);
                        Match matchGenre      = regexGenre.Match(line);
                        Match matchFlags      = regexFlags.Match(line);
                        Match matchFile       = regexFile.Match(line);
                        Match matchDiskId     = regexDiskId.Match(line);
                        Match matchComposer   = regexComposer.Match(line);
                        Match matchCdText     = regexCdText.Match(line);
                        Match matchBarCode    = regexBarCode.Match(line);
                        Match matchArranger   = regexArranger.Match(line);

                        if(matchArranger.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found ARRANGER at line {0}", lineNumber);

                            if(inTrack)
                                currentTrack.Arranger = matchArranger.Groups[1].Value;
                            else
                                _discImage.Arranger = matchArranger.Groups[1].Value;
                        }
                        else if(matchBarCode.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found UPC_EAN at line {0}", lineNumber);

                            if(!inTrack)
                                _discImage.Barcode = matchBarCode.Groups[1].Value;
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found barcode field in incorrect place at line {lineNumber}");
                        }
                        else if(matchCdText.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CDTEXTFILE at line {0}", lineNumber);

                            if(!inTrack)
                                _discImage.CdTextFile = matchCdText.Groups[1].Value;
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found CD-Text file field in incorrect place at line {lineNumber}");
                        }
                        else if(matchComposer.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found COMPOSER at line {0}", lineNumber);

                            if(inTrack)
                                currentTrack.Composer = matchComposer.Groups[1].Value;
                            else
                                _discImage.Composer = matchComposer.Groups[1].Value;
                        }
                        else if(matchDiskId.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found DISC_ID at line {0}", lineNumber);

                            if(!inTrack)
                                _discImage.DiscId = matchDiskId.Groups[1].Value;
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found CDDB ID field in incorrect place at line {lineNumber}");
                        }
                        else if(matchFile.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found FILE at line {0}", lineNumber);

                            if(currentTrack.Sequence != 0)
                            {
                                currentFile.Sequence   = currentTrack.Sequence;
                                currentTrack.TrackFile = currentFile;

                                currentTrack.Sectors =
                                    ((ulong)currentFile.DataFilter.GetLength() - currentFile.Offset) /
                                    CdrWinTrackTypeToBytesPerSector(currentTrack.TrackType);

                                cueTracks[currentTrack.Sequence - 1] = currentTrack;
                                inTrack                              = false;
                                currentTrack                         = new CdrWinTrack();
                                currentFile                          = new CdrWinTrackFile();
                                filtersList                          = new FiltersList();
                            }

                            string datafile = matchFile.Groups[1].Value;
                            currentFile.FileType = matchFile.Groups[2].Value;

                            // Check if file path is quoted
                            if(datafile[0]                   == '"' &&
                               datafile[datafile.Length - 1] == '"')
                                datafile = datafile.Substring(1, datafile.Length - 2); // Unquote it

                            currentFile.DataFilter = filtersList.GetFilter(datafile);

                            // Check if file exists
                            if(currentFile.DataFilter == null)
                                if(datafile[0] == '/' ||
                                   (datafile[0] == '/' && datafile[1] == '.')) // UNIX absolute path
                                {
                                    var   unixPath      = new Regex("^(.+)/([^/]+)$");
                                    Match unixPathMatch = unixPath.Match(datafile);

                                    if(unixPathMatch.Success)
                                    {
                                        currentFile.DataFilter = filtersList.GetFilter(unixPathMatch.Groups[1].Value);

                                        if(currentFile.DataFilter == null)
                                        {
                                            string path = imageFilter.GetParentFolder() + Path.PathSeparator +
                                                          unixPathMatch.Groups[1].Value;

                                            currentFile.DataFilter = filtersList.GetFilter(path);

                                            if(currentFile.DataFilter == null)
                                                throw new
                                                    FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                        }
                                    }
                                    else
                                        throw new
                                            FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                }
                                else if((datafile[1] == ':'  && datafile[2] == '\\') ||
                                        (datafile[0] == '\\' && datafile[1] == '\\') ||
                                        (datafile[0] == '.'  && datafile[1] == '\\')) // Windows absolute path
                                {
                                    var winPath =
                                        new
                                            Regex("^(?:[a-zA-Z]\\:(\\\\|\\/)|file\\:\\/\\/|\\\\\\\\|\\.(\\/|\\\\))([^\\\\\\/\\:\\*\\?\\<\\>\\\"\\|]+(\\\\|\\/){0,1})+$");

                                    Match winPathMatch = winPath.Match(datafile);

                                    if(winPathMatch.Success)
                                    {
                                        currentFile.DataFilter = filtersList.GetFilter(winPathMatch.Groups[1].Value);

                                        if(currentFile.DataFilter == null)
                                        {
                                            string path = imageFilter.GetParentFolder() + Path.PathSeparator +
                                                          winPathMatch.Groups[1].Value;

                                            currentFile.DataFilter = filtersList.GetFilter(path);

                                            if(currentFile.DataFilter == null)
                                                throw new
                                                    FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                        }
                                    }
                                    else
                                        throw new
                                            FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                }
                                else
                                {
                                    string path = imageFilter.GetParentFolder() + Path.PathSeparator + datafile;
                                    currentFile.DataFilter = filtersList.GetFilter(path);

                                    if(currentFile.DataFilter == null)
                                        throw new
                                            FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                }

                            // File does exist, process it
                            DicConsole.DebugWriteLine("CDRWin plugin", "File \"{0}\" found",
                                                      currentFile.DataFilter.GetFilename());

                            switch(currentFile.FileType)
                            {
                                case CDRWIN_DISK_TYPE_LITTLE_ENDIAN: break;
                                case CDRWIN_DISK_TYPE_BIG_ENDIAN:
                                case CDRWIN_DISK_TYPE_AIFF:
                                case CDRWIN_DISK_TYPE_RIFF:
                                case CDRWIN_DISK_TYPE_MP3:
                                    throw new
                                        FeatureSupportedButNotImplementedImageException($"Unsupported file type {currentFile.FileType}");
                                default:
                                    throw new
                                        FeatureUnsupportedImageException($"Unknown file type {currentFile.FileType}");
                            }

                            currentFile.Offset   = 0;
                            currentFile.Sequence = 0;
                        }
                        else if(matchFlags.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found FLAGS at line {0}", lineNumber);

                            if(!inTrack)
                                throw new
                                    FeatureUnsupportedImageException($"Found FLAGS field in incorrect place at line {lineNumber}");

                            currentTrack.FlagDcp  |= matchFlags.Groups["dcp"].Value  == "DCP";
                            currentTrack.Flag4ch  |= matchFlags.Groups["quad"].Value == "4CH";
                            currentTrack.FlagPre  |= matchFlags.Groups["pre"].Value  == "PRE";
                            currentTrack.FlagScms |= matchFlags.Groups["scms"].Value == "SCMS";
                        }
                        else if(matchGenre.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found GENRE at line {0}", lineNumber);

                            if(inTrack)
                                currentTrack.Genre = matchGenre.Groups[1].Value;
                            else
                                _discImage.Genre = matchGenre.Groups[1].Value;
                        }
                        else if(matchIndex.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found INDEX at line {0}", lineNumber);

                            if(!inTrack)
                                throw new FeatureUnsupportedImageException($"Found INDEX before a track {lineNumber}");

                            int   index  = int.Parse(matchIndex.Groups[1].Value);
                            ulong offset = CdrWinMsfToLba(matchIndex.Groups[2].Value);

                            if(index                      != 0 &&
                               index                      != 1 &&
                               currentTrack.Indexes.Count == 0)
                                throw new
                                    FeatureUnsupportedImageException($"Found INDEX {index} before INDEX 00 or INDEX 01");

                            if(index == 0 ||
                               (index == 1 && !currentTrack.Indexes.ContainsKey(0)))
                                if((int)(currentTrack.Sequence - 2) >= 0 &&
                                   offset                           > 1)
                                {
                                    cueTracks[currentTrack.Sequence - 2].Sectors = offset - currentFileOffsetSector;

                                    currentFile.Offset +=
                                        cueTracks[currentTrack.Sequence - 2].Sectors *
                                        cueTracks[currentTrack.Sequence - 2].Bps;

                                    DicConsole.DebugWriteLine("CDRWin plugin", "Sets currentFile.offset to {0}",
                                                              currentFile.Offset);

                                    DicConsole.DebugWriteLine("CDRWin plugin",
                                                              "cueTracks[currentTrack.sequence-2].sectors = {0}",
                                                              cueTracks[currentTrack.Sequence - 2].Sectors);

                                    DicConsole.DebugWriteLine("CDRWin plugin",
                                                              "cueTracks[currentTrack.sequence-2].bps = {0}",
                                                              cueTracks[currentTrack.Sequence - 2].Bps);
                                }

                            if((index == 0 || (index == 1 && !currentTrack.Indexes.ContainsKey(0))) &&
                               currentTrack.Sequence == 1)
                            {
                                DicConsole.DebugWriteLine("CDRWin plugin", "Sets currentFile.offset to {0}",
                                                          offset * currentTrack.Bps);

                                currentFile.Offset = offset * currentTrack.Bps;
                            }

                            currentFileOffsetSector = offset;
                            currentTrack.Indexes.Add(index, offset);
                        }
                        else if(matchIsrc.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found ISRC at line {0}", lineNumber);

                            if(!inTrack)
                                throw new FeatureUnsupportedImageException($"Found ISRC before a track {lineNumber}");

                            currentTrack.Isrc = matchIsrc.Groups[1].Value;
                        }
                        else if(matchMcn.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CATALOG at line {0}", lineNumber);

                            if(!inTrack)
                                _discImage.Mcn = matchMcn.Groups[1].Value;
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found CATALOG field in incorrect place at line {lineNumber}");
                        }
                        else if(matchPerformer.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found PERFORMER at line {0}", lineNumber);

                            if(inTrack)
                                currentTrack.Performer = matchPerformer.Groups[1].Value;
                            else
                                _discImage.Performer = matchPerformer.Groups[1].Value;
                        }
                        else if(matchPostgap.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found POSTGAP at line {0}", lineNumber);

                            if(inTrack)
                                currentTrack.Postgap = CdrWinMsfToLba(matchPostgap.Groups[1].Value);
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found POSTGAP field before a track at line {lineNumber}");
                        }
                        else if(matchPregap.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found PREGAP at line {0}", lineNumber);

                            if(inTrack)
                                currentTrack.Pregap = CdrWinMsfToLba(matchPregap.Groups[1].Value);
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found PREGAP field before a track at line {lineNumber}");
                        }
                        else if(matchSongWriter.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found SONGWRITER at line {0}", lineNumber);

                            if(inTrack)
                                currentTrack.Songwriter = matchSongWriter.Groups[1].Value;
                            else
                                _discImage.Songwriter = matchSongWriter.Groups[1].Value;
                        }
                        else if(matchTitle.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found TITLE at line {0}", lineNumber);

                            if(inTrack)
                                currentTrack.Title = matchTitle.Groups[1].Value;
                            else
                                _discImage.Title = matchTitle.Groups[1].Value;
                        }
                        else if(matchTrack.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found TRACK at line {0}", lineNumber);

                            if(currentFile.DataFilter == null)
                                throw new
                                    FeatureUnsupportedImageException($"Found TRACK field before a file is defined at line {lineNumber}");

                            if(inTrack)
                            {
                                if(currentTrack.Indexes.ContainsKey(0) &&
                                   currentTrack.Pregap == 0)
                                    currentTrack.Indexes.TryGetValue(0, out currentTrack.Pregap);

                                currentFile.Sequence                 = currentTrack.Sequence;
                                currentTrack.TrackFile               = currentFile;
                                cueTracks[currentTrack.Sequence - 1] = currentTrack;
                            }

                            currentTrack = new CdrWinTrack
                            {
                                Indexes  = new Dictionary<int, ulong>(),
                                Sequence = uint.Parse(matchTrack.Groups[1].Value)
                            };

                            DicConsole.DebugWriteLine("CDRWin plugin", "Setting currentTrack.sequence to {0}",
                                                      currentTrack.Sequence);

                            currentFile.Sequence   = currentTrack.Sequence;
                            currentTrack.Bps       = CdrWinTrackTypeToBytesPerSector(matchTrack.Groups[2].Value);
                            currentTrack.TrackType = matchTrack.Groups[2].Value;
                            currentTrack.Session   = currentSession;
                            inTrack                = true;
                        }
                        else if(line == "") // Empty line, ignore it
                        { }
                        else // Non-empty unknown field
                            throw new
                                FeatureUnsupportedImageException($"Found unknown field defined at line {lineNumber}: \"{line}\"");
                    }
                }

                if(currentTrack.Sequence != 0)
                {
                    currentFile.Sequence   = currentTrack.Sequence;
                    currentTrack.TrackFile = currentFile;

                    currentTrack.Sectors = ((ulong)currentFile.DataFilter.GetLength() - currentFile.Offset) /
                                           CdrWinTrackTypeToBytesPerSector(currentTrack.TrackType);

                    cueTracks[currentTrack.Sequence - 1] = currentTrack;
                }

                Session[] sessions = new Session[currentSession];

                for(int s = 1; s <= sessions.Length; s++)
                {
                    sessions[s - 1].SessionSequence = 1;

                    if(s > 1)
                        sessions[s - 1].StartSector = sessions[s - 2].EndSector + 1;
                    else
                        sessions[s - 1].StartSector = 0;

                    ulong sessionSectors   = 0;
                    int   lastSessionTrack = 0;

                    for(int i = 0; i < cueTracks.Length; i++)
                        if(cueTracks[i].Session == s)
                        {
                            sessionSectors += cueTracks[i].Sectors;

                            if(i > lastSessionTrack)
                                lastSessionTrack = i;
                        }

                    sessions[s - 1].EndTrack  = cueTracks[lastSessionTrack].Sequence;
                    sessions[s - 1].EndSector = sessionSectors - 1;
                }

                for(int s = 1; s <= sessions.Length; s++)
                    _discImage.Sessions.Add(sessions[s - 1]);

                for(int t = 1; t <= cueTracks.Length; t++)
                {
                    if(cueTracks[t - 1].Indexes.TryGetValue(0, out ulong idx0) &&
                       cueTracks[t - 1].Indexes.TryGetValue(1, out ulong idx1))
                        cueTracks[t - 1].Pregap = idx1 - idx0;

                    _discImage.Tracks.Add(cueTracks[t - 1]);
                }

                if(!string.IsNullOrWhiteSpace(_discImage.DicMediaType) &&
                   Enum.TryParse(_discImage.DicMediaType, true, out MediaType mediaType))
                {
                    _discImage.MediaType = mediaType;
                }
                else
                    _discImage.MediaType = CdrWinIsoBusterDiscTypeToMediaType(_discImage.OriginalMediaType);

                if(_discImage.MediaType == MediaType.Unknown ||
                   _discImage.MediaType == MediaType.CD)
                {
                    bool data       = false;
                    bool cdg        = false;
                    bool cdi        = false;
                    bool mode2      = false;
                    bool firstAudio = false;
                    bool firstData  = false;
                    bool audio      = false;

                    for(int i = 0; i < _discImage.Tracks.Count; i++)
                    {
                        // First track is audio
                        firstAudio |= i == 0 && _discImage.Tracks[i].TrackType == CDRWIN_TRACK_TYPE_AUDIO;

                        // First track is data
                        firstData |= i == 0 && _discImage.Tracks[i].TrackType != CDRWIN_TRACK_TYPE_AUDIO;

                        // Any non first track is data
                        data |= i != 0 && _discImage.Tracks[i].TrackType != CDRWIN_TRACK_TYPE_AUDIO;

                        // Any non first track is audio
                        audio |= i != 0 && _discImage.Tracks[i].TrackType == CDRWIN_TRACK_TYPE_AUDIO;

                        switch(_discImage.Tracks[i].TrackType)
                        {
                            case CDRWIN_TRACK_TYPE_CDG:
                                cdg = true;

                                break;
                            case CDRWIN_TRACK_TYPE_CDI:
                            case CDRWIN_TRACK_TYPE_CDI_RAW:
                                cdi = true;

                                break;
                            case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                            case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                            case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                            case CDRWIN_TRACK_TYPE_MODE2_RAW:
                                mode2 = true;

                                break;
                        }
                    }

                    if(!data &&
                       !firstData)
                        _discImage.MediaType = MediaType.CDDA;
                    else if(cdg)
                        _discImage.MediaType = MediaType.CDG;
                    else if(cdi)
                        _discImage.MediaType = MediaType.CDI;
                    else if(firstAudio                    &&
                            data                          &&
                            _discImage.Sessions.Count > 1 &&
                            mode2)
                        _discImage.MediaType = MediaType.CDPLUS;
                    else if((firstData && audio) || mode2)
                        _discImage.MediaType = MediaType.CDROMXA;
                    else if(!audio)
                        _discImage.MediaType = MediaType.CDROM;
                    else
                        _discImage.MediaType = MediaType.CD;
                }

                // DEBUG information
                DicConsole.DebugWriteLine("CDRWin plugin", "Disc image parsing results");
                DicConsole.DebugWriteLine("CDRWin plugin", "Disc CD-TEXT:");

                if(_discImage.Arranger == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tArranger is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tArranger: {0}", _discImage.Arranger);

                if(_discImage.Composer == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComposer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComposer: {0}", _discImage.Composer);

                if(_discImage.Genre == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tGenre is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tGenre: {0}", _discImage.Genre);

                if(_discImage.Performer == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPerformer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPerformer: {0}", _discImage.Performer);

                if(_discImage.Songwriter == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tSongwriter is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tSongwriter: {0}", _discImage.Songwriter);

                if(_discImage.Title == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tTitle is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tTitle: {0}", _discImage.Title);

                if(_discImage.CdTextFile == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tCD-TEXT binary file not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tCD-TEXT binary file: {0}", _discImage.CdTextFile);

                DicConsole.DebugWriteLine("CDRWin plugin", "Disc information:");

                if(_discImage.OriginalMediaType == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tISOBuster disc type not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tISOBuster disc type: {0}",
                                              _discImage.OriginalMediaType);

                DicConsole.DebugWriteLine("CDRWin plugin", "\tGuessed disk type: {0}", _discImage.MediaType);

                if(_discImage.Barcode == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tBarcode not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tBarcode: {0}", _discImage.Barcode);

                if(_discImage.DiscId == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc ID not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc ID: {0}", _discImage.DiscId);

                if(_discImage.Mcn == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tMCN not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tMCN: {0}", _discImage.Mcn);

                if(_discImage.Comment == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComment not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComment: \"{0}\"", _discImage.Comment);

                DicConsole.DebugWriteLine("CDRWin plugin", "Session information:");
                DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc contains {0} sessions", _discImage.Sessions.Count);

                for(int i = 0; i < _discImage.Sessions.Count; i++)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tSession {0} information:", i + 1);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tStarting track: {0}",
                                              _discImage.Sessions[i].StartTrack);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tStarting sector: {0}",
                                              _discImage.Sessions[i].StartSector);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tEnding track: {0}",
                                              _discImage.Sessions[i].EndTrack);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tEnding sector: {0}",
                                              _discImage.Sessions[i].EndSector);
                }

                DicConsole.DebugWriteLine("CDRWin plugin", "Track information:");
                DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc contains {0} tracks", _discImage.Tracks.Count);

                for(int i = 0; i < _discImage.Tracks.Count; i++)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tTrack {0} information:",
                                              _discImage.Tracks[i].Sequence);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\t{0} bytes per sector", _discImage.Tracks[i].Bps);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPregap: {0} sectors", _discImage.Tracks[i].Pregap);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tData: {0} sectors", _discImage.Tracks[i].Sectors);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPostgap: {0} sectors",
                                              _discImage.Tracks[i].Postgap);

                    if(_discImage.Tracks[i].Flag4ch)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack is flagged as quadraphonic");

                    if(_discImage.Tracks[i].FlagDcp)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack allows digital copy");

                    if(_discImage.Tracks[i].FlagPre)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack has pre-emphasis applied");

                    if(_discImage.Tracks[i].FlagScms)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack has SCMS");

                    DicConsole.DebugWriteLine("CDRWin plugin",
                                              "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                                              _discImage.Tracks[i].TrackFile.DataFilter.GetFilename(),
                                              _discImage.Tracks[i].TrackFile.FileType,
                                              _discImage.Tracks[i].TrackFile.Offset);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tIndexes:");

                    foreach(KeyValuePair<int, ulong> kvp in _discImage.Tracks[i].Indexes)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\t\tIndex {0} starts at sector {1}", kvp.Key,
                                                  kvp.Value);

                    if(_discImage.Tracks[i].Isrc == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tISRC is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tISRC: {0}", _discImage.Tracks[i].Isrc);

                    if(_discImage.Tracks[i].Arranger == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tArranger is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tArranger: {0}", _discImage.Tracks[i].Arranger);

                    if(_discImage.Tracks[i].Composer == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tComposer is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tComposer: {0}", _discImage.Tracks[i].Composer);

                    if(_discImage.Tracks[i].Genre == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tGenre is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tGenre: {0}", _discImage.Tracks[i].Genre);

                    if(_discImage.Tracks[i].Performer == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPerformer is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPerformer: {0}",
                                                  _discImage.Tracks[i].Performer);

                    if(_discImage.Tracks[i].Songwriter == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tSongwriter is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tSongwriter: {0}",
                                                  _discImage.Tracks[i].Songwriter);

                    if(_discImage.Tracks[i].Title == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTitle is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTitle: {0}", _discImage.Tracks[i].Title);
                }

                DicConsole.DebugWriteLine("CDRWin plugin", "Building offset map");

                Partitions = new List<Partition>();

                ulong byteOffset        = 0;
                ulong sectorOffset      = 0;
                ulong partitionSequence = 0;

                _offsetMap = new Dictionary<uint, ulong>();

                for(int i = 0; i < _discImage.Tracks.Count; i++)
                {
                    if(_discImage.Tracks[i].Sequence == 1 &&
                       i                             != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    var partition = new Partition();

                    if(!_discImage.Tracks[i].Indexes.TryGetValue(1, out ulong _))
                        throw new ImageNotSupportedException($"Track {_discImage.Tracks[i].Sequence} lacks index 01");

                    // Index 01
                    partition.Description = $"Track {_discImage.Tracks[i].Sequence}.";
                    partition.Name        = _discImage.Tracks[i].Title;
                    partition.Start       = sectorOffset;
                    partition.Size        = _discImage.Tracks[i].Sectors * _discImage.Tracks[i].Bps;
                    partition.Length      = _discImage.Tracks[i].Sectors;
                    partition.Sequence    = partitionSequence;
                    partition.Offset      = byteOffset;
                    partition.Type        = _discImage.Tracks[i].TrackType;

                    sectorOffset += partition.Length;
                    byteOffset   += partition.Size;
                    partitionSequence++;

                    if(!_offsetMap.ContainsKey(_discImage.Tracks[i].Sequence))
                        _offsetMap.Add(_discImage.Tracks[i].Sequence, partition.Start);
                    else
                    {
                        _offsetMap.TryGetValue(_discImage.Tracks[i].Sequence, out ulong oldStart);

                        if(partition.Start < oldStart)
                        {
                            _offsetMap.Remove(_discImage.Tracks[i].Sequence);
                            _offsetMap.Add(_discImage.Tracks[i].Sequence, partition.Start);
                        }
                    }

                    Partitions.Add(partition);
                }

                // Print offset map
                DicConsole.DebugWriteLine("CDRWin plugin", "printing partition map");

                foreach(Partition partition in Partitions)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "Partition sequence: {0}", partition.Sequence);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition name: {0}", partition.Name);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition description: {0}", partition.Description);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition type: {0}", partition.Type);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition starting sector: {0}", partition.Start);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition sectors: {0}", partition.Length);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition starting offset: {0}", partition.Offset);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition size in bytes: {0}", partition.Size);
                }

                foreach(CdrWinTrack track in _discImage.Tracks)
                    _imageInfo.ImageSize += track.Bps * track.Sectors;

                foreach(CdrWinTrack track in _discImage.Tracks)
                    _imageInfo.Sectors += track.Sectors;

                if(_discImage.MediaType != MediaType.CDROMXA &&
                   _discImage.MediaType != MediaType.CDDA    &&
                   _discImage.MediaType != MediaType.CDI     &&
                   _discImage.MediaType != MediaType.CDPLUS  &&
                   _discImage.MediaType != MediaType.CDG     &&
                   _discImage.MediaType != MediaType.CDEG    &&
                   _discImage.MediaType != MediaType.CDMIDI)
                    _imageInfo.SectorSize = 2048; // Only data tracks
                else
                    _imageInfo.SectorSize = 2352; // All others

                if(_discImage.Mcn != null)
                    _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);

                if(_discImage.CdTextFile != null)
                    _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);

                if(_imageInfo.Application is null)
                {
                    if(_discImage.IsTrurip)
                        _imageInfo.Application = "trurip";

                    // Detect ISOBuster extensions
                    else if(_discImage.OriginalMediaType != null               ||
                            _discImage.Comment.ToLower().Contains("isobuster") ||
                            _discImage.Sessions.Count > 1)
                        _imageInfo.Application = "ISOBuster";
                    else
                        _imageInfo.Application = "CDRWin";
                }

                _imageInfo.CreationTime         = imageFilter.GetCreationTime();
                _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

                _imageInfo.Comments          = _discImage.Comment;
                _imageInfo.MediaSerialNumber = _discImage.Mcn;
                _imageInfo.MediaBarcode      = _discImage.Barcode;
                _imageInfo.MediaType         = _discImage.MediaType;

                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                foreach(CdrWinTrack track in _discImage.Tracks)
                    switch(track.TrackType)
                    {
                        case CDRWIN_TRACK_TYPE_AUDIO:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

                            break;
                        }
                        case CDRWIN_TRACK_TYPE_CDG:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                            break;
                        }
                        case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                        case CDRWIN_TRACK_TYPE_CDI:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                            break;
                        }
                        case CDRWIN_TRACK_TYPE_MODE2_RAW:
                        case CDRWIN_TRACK_TYPE_CDI_RAW:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                            break;
                        }
                        case CDRWIN_TRACK_TYPE_MODE1_RAW:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                            break;
                        }
                    }

                _imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                DicConsole.VerboseWriteLine("CDRWIN image describes a disc of type {0}", _imageInfo.MediaType);

                if(!string.IsNullOrEmpty(_imageInfo.Comments))
                    DicConsole.VerboseWriteLine("CDRWIN comments: {0}", _imageInfo.Comments);

                return true;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.GetFilename());
                DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);

                return false;
            }
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_MCN:
                {
                    if(_discImage.Mcn != null)
                        return Encoding.ASCII.GetBytes(_discImage.Mcn);

                    throw new FeatureNotPresentImageException("Image does not contain MCN information.");
                }
                case MediaTagType.CD_TEXT:
                {
                    if(_discImage.CdTextFile != null)

                        // TODO: Check binary text file exists, open it, read it, send it to caller.
                        throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");

                    throw new FeatureNotPresentImageException("Image does not contain CD-TEXT information.");
                }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        public byte[] ReadSector(ulong sectorAddress, uint track) => ReadSectors(sectorAddress, 1, track);

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag) =>
            ReadSectorsTag(sectorAddress, 1, track, tag);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress >= kvp.Value
                                                     from cdrwinTrack in _discImage.Tracks
                                                     where cdrwinTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrwinTrack.Sectors select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress >= kvp.Value
                                                     from cdrwinTrack in _discImage.Tracks
                                                     where cdrwinTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrwinTrack.Sectors select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            var dicTrack = new CdrWinTrack
            {
                Sequence = 0
            };

            foreach(CdrWinTrack cdrwinTrack in _discImage.Tracks.Where(cdrwinTrack => cdrwinTrack.Sequence == track))
            {
                dicTrack = cdrwinTrack;

                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;
            bool mode2 = false;

            switch(dicTrack.TrackType)
            {
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                {
                    sectorOffset = 0;
                    sectorSize   = 2324;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                case CDRWIN_TRACK_TYPE_CDI:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2336;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_AUDIO:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE1_RAW:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288;

                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_RAW:
                case CDRWIN_TRACK_TYPE_CDI_RAW:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_CDG:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 96;

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream = dicTrack.TrackFile.DataFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)dicTrack.TrackFile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                buffer = br.ReadBytes((int)(sectorSize * length));

                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    Array.Copy(buffer, sectorSize * i, sector, 0, sectorSize);
                    sector = Sector.GetUserDataFromMode2(sector);
                    mode2Ms.Write(sector, 0, sector.Length);
                }

                buffer = mode2Ms.ToArray();
            }
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            var dicTrack = new CdrWinTrack
            {
                Sequence = 0
            };

            foreach(CdrWinTrack cdrwinTrack in _discImage.Tracks.Where(cdrwinTrack => cdrwinTrack.Sequence == track))
            {
                dicTrack = cdrwinTrack;

                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(tag)
            {
                case SectorTagType.CdSectorEcc:
                case SectorTagType.CdSectorEccP:
                case SectorTagType.CdSectorEccQ:
                case SectorTagType.CdSectorEdc:
                case SectorTagType.CdSectorHeader:
                case SectorTagType.CdSectorSubchannel:
                case SectorTagType.CdSectorSubHeader:
                case SectorTagType.CdSectorSync: break;
                case SectorTagType.CdTrackFlags:
                {
                    CdFlags flags = 0;

                    if(dicTrack.TrackType != CDRWIN_TRACK_TYPE_AUDIO &&
                       dicTrack.TrackType != CDRWIN_TRACK_TYPE_CDG)
                        flags |= CdFlags.DataTrack;

                    if(dicTrack.FlagDcp)
                        flags |= CdFlags.CopyPermitted;

                    if(dicTrack.FlagPre)
                        flags |= CdFlags.PreEmphasis;

                    if(dicTrack.Flag4ch)
                        flags |= CdFlags.FourChannel;

                    return new[]
                    {
                        (byte)flags
                    };
                }
                case SectorTagType.CdTrackIsrc:
                    return dicTrack.Isrc == null ? null : Encoding.UTF8.GetBytes(dicTrack.Isrc);

                case SectorTagType.CdTrackText:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(dicTrack.TrackType)
            {
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                    throw new ArgumentException("No tags in image for requested track", nameof(tag));
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                case CDRWIN_TRACK_TYPE_CDI:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubchannel:
                        case SectorTagType.CdSectorEcc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 0;
                            sectorSize   = 8;
                            sectorSkip   = 2328;

                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2332;
                            sectorSize   = 4;
                            sectorSkip   = 0;

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                case CDRWIN_TRACK_TYPE_AUDIO:
                    throw new ArgumentException("There are no tags on audio tracks", nameof(tag));
                case CDRWIN_TRACK_TYPE_MODE1_RAW:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize   = 12;
                            sectorSkip   = 2340;

                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize   = 4;
                            sectorSkip   = 2336;

                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 276;
                            sectorSkip   = 0;

                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 172;
                            sectorSkip   = 104;

                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize   = 104;
                            sectorSkip   = 0;

                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize   = 4;
                            sectorSkip   = 284;

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_RAW: // Requires reading sector
                case CDRWIN_TRACK_TYPE_CDI_RAW:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                case CDRWIN_TRACK_TYPE_CDG:
                {
                    if(tag != SectorTagType.CdSectorSubchannel)
                        throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));

                    sectorOffset = 2352;
                    sectorSize   = 96;
                    sectorSkip   = 0;

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream = dicTrack.TrackFile.DataFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)dicTrack.TrackFile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        public byte[] ReadSectorLong(ulong sectorAddress, uint track) => ReadSectorsLong(sectorAddress, 1, track);

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress >= kvp.Value
                                                     from cdrwinTrack in _discImage.Tracks
                                                     where cdrwinTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrwinTrack.Sectors select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            var dicTrack = new CdrWinTrack
            {
                Sequence = 0
            };

            foreach(CdrWinTrack cdrwinTrack in _discImage.Tracks.Where(cdrwinTrack => cdrwinTrack.Sequence == track))
            {
                dicTrack = cdrwinTrack;

                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.TrackType)
            {
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                {
                    sectorOffset = 0;
                    sectorSize   = 2324;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                case CDRWIN_TRACK_TYPE_CDI:
                {
                    sectorOffset = 0;
                    sectorSize   = 2336;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE1_RAW:
                case CDRWIN_TRACK_TYPE_MODE2_RAW:
                case CDRWIN_TRACK_TYPE_CDI_RAW:
                case CDRWIN_TRACK_TYPE_AUDIO:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case CDRWIN_TRACK_TYPE_CDG:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 96;

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream = dicTrack.TrackFile.DataFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)dicTrack.TrackFile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public List<Track> GetSessionTracks(Session session)
        {
            if(_discImage.Sessions.Contains(session))
                return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new List<Track>();

            foreach(CdrWinTrack cdrTrack in _discImage.Tracks)
                if(cdrTrack.Session == session)
                {
                    var dicTrack = new Track
                    {
                        Indexes             = cdrTrack.Indexes, TrackDescription = cdrTrack.Title,
                        TrackPregap         = cdrTrack.Pregap,
                        TrackSession        = cdrTrack.Session, TrackSequence = cdrTrack.Sequence,
                        TrackType           = CdrWinTrackTypeToTrackType(cdrTrack.TrackType),
                        TrackFile           = cdrTrack.TrackFile.DataFilter.GetFilename(),
                        TrackFilter         = cdrTrack.TrackFile.DataFilter,
                        TrackFileOffset     = cdrTrack.TrackFile.Offset,
                        TrackFileType       = cdrTrack.TrackFile.FileType, TrackRawBytesPerSector = cdrTrack.Bps,
                        TrackBytesPerSector = CdrWinTrackTypeToCookedBytesPerSector(cdrTrack.TrackType)
                    };

                    if(!cdrTrack.Indexes.TryGetValue(0, out dicTrack.TrackStartSector))
                        cdrTrack.Indexes.TryGetValue(1, out dicTrack.TrackStartSector);

                    dicTrack.TrackEndSector = (dicTrack.TrackStartSector + cdrTrack.Sectors) - 1;

                    if(cdrTrack.TrackType == CDRWIN_TRACK_TYPE_CDG)
                    {
                        dicTrack.TrackSubchannelFilter = cdrTrack.TrackFile.DataFilter;
                        dicTrack.TrackSubchannelFile   = cdrTrack.TrackFile.DataFilter.GetFilename();
                        dicTrack.TrackSubchannelOffset = cdrTrack.TrackFile.Offset;
                        dicTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;
                    }
                    else
                        dicTrack.TrackSubchannelType = TrackSubchannelType.None;

                    tracks.Add(dicTrack);
                }

            return tracks;
        }
    }
}