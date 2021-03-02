﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MaxiDisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class MaxiDisk : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "3DF800.HDK.lz", "3DS.HDK.lz", "3ES.HDK.lz", "3HD6.HDK.lz", "3HF168.HDK.lz", "3HF16.HDK.lz",
            "3HF172.HDK.lz", "3HS.HDK.lz", "5DS18.HDK.lz", "5DS1.HDK.lz", "5DS28.HDK.lz", "5DS2.HDK.lz",
            "5HF144.HDK.lz", "5HS.HDK.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // 3DF800.HDK.lz
            1600,

            // 3DS.HDK.lz
            1440,

            // 3ES.HDK.lz
            5760,

            // 3HD6.HDK.lz
            3360,

            // 3HF168.HDK.lz
            3360,

            // 3HF16.HDK.lz
            3200,

            // 3HF172.HDK.lz
            3444,

            // 3HS.HDK.lz
            2880,

            // 5DS18.HDK.lz
            320,

            // 5DS1.HDK.lz
            360,

            // 5DS28.HDK.lz
            640,

            // 5DS2.HDK.lz
            720,

            // 5HF144.HDK.lz
            2880,

            // 5HS.HDK.lz
            2400
        };

        public override uint[] _sectorSize => new uint[]
        {
            // 3DF800.HDK.lz
            512,

            // 3DS.HDK.lz
            512,

            // 3ES.HDK.lz
            512,

            // 3HD6.HDK.lz
            512,

            // 3HF168.HDK.lz
            512,

            // 3HF16.HDK.lz
            512,

            // 3HF172.HDK.lz
            512,

            // 3HS.HDK.lz
            512,

            // 5DS18.HDK.lz
            512,

            // 5DS1.HDK.lz
            512,

            // 5DS28.HDK.lz
            512,

            // 5DS2.HDK.lz
            512,

            // 5HF144.HDK.lz
            512,

            // 5HS.HDK.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // 3DF800.HDK.lz
            MediaType.CBM_35_DD,

            // 3DS.HDK.lz
            MediaType.DOS_35_DS_DD_9,

            // 3ES.HDK.lz
            MediaType.ECMA_147,

            // 3HD6.HDK.lz
            MediaType.DMF,

            // 3HF168.HDK.lz
            MediaType.DMF,

            // 3HF16.HDK.lz
            MediaType.Unknown,

            // 3HF172.HDK.lz
            MediaType.FDFORMAT_35_HD,

            // 3HS.HDK.lz
            MediaType.DOS_35_HD,

            // 5DS18.HDK.lz
            MediaType.DOS_525_SS_DD_8,

            // 5DS1.HDK.lz
            MediaType.DOS_525_SS_DD_9,

            // 5DS28.HDK.lz
            MediaType.DOS_525_DS_DD_8,

            // 5DS2.HDK.lz
            MediaType.DOS_525_DS_DD_9,

            // 5HF144.HDK.lz
            MediaType.DOS_35_HD,

            // 5HS.HDK.lz
            MediaType.DOS_525_HD
        };

        public override string[] _md5S => new[]
        {
            // 3DF800.HDK.lz
            "26532a62985b51a2c3b877a57f6d257b",

            // 3DS.HDK.lz
            "9827ba1b3e9cac41263caabd862e78f9",

            // 3ES.HDK.lz
            "4aeafaf2a088d6a7406856dce8118567",

            // 3HD6.HDK.lz
            "2bfd2e0a81bad704f8fc7758358cfcca",

            // 3HF168.HDK.lz
            "7e3bf04f3660dd1052a335dc99441e44",

            // 3HF16.HDK.lz
            "8eb8cb310feaf03c69fffd4f6e729847",

            // 3HF172.HDK.lz
            "a58fd062f024b95714f1223a8bc2232f",

            // 3HS.HDK.lz
            "00e61c06bf29f0c04a7eabe2dbd7efb6",

            // 5DS18.HDK.lz
            "d81f5cb64fd0b99f138eab34110bbc3c",

            // 5DS1.HDK.lz
            "a89006a75d13bee9202d1d6e52721ccb",

            // 5DS28.HDK.lz
            "beef1cdb004dc69391d6b3d508988b95",

            // 5DS2.HDK.lz
            "6213897b7dbf263f12abf76901d43862",

            // 5HF144.HDK.lz
            "073a172879a71339ef4b00ebb47b67fc",

            // 5HS.HDK.lz
            "02259cd5fbcc20f8484aa6bece7a37c6"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "MaxiDisk");
        public override IMediaImage _plugin => new DiscImages.MaxiDisk();
    }
}