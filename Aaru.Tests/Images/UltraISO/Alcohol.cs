﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Alcohol.cs
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

namespace Aaru.Tests.Images.UltraISO
{
    [TestFixture]
    public class Alcohol : OpticalMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "cdiready_the_apprentice.mds", "report_audiocd.mds", "report_cdrom.mds", "report_cdrw.mds",
            "report_dvdram_v2.mds", "report_dvd-r+dl.mds", "report_dvdrom.mds", "report_enhancedcd.mds",
            "test_multi_karaoke_sampler.mds"
        };
        public override uint[] _sectorSize => null;

        public override ulong[] _sectors => new ulong[]
        {
            // cdiready_the_apprentice.mds
            279300,

            // report_audiocd.mds
            247073,

            // report_cdrom.mds
            254265,

            // report_cdrw.mds
            308224,

            // report_dvdram_v2.mds
            471090,

            // report_dvd-r+dl.mds
            3455920,

            // report_dvdrom.mds
            2146357,

            // report_enhancedcd.mds
            303316,

            // test_multi_karaoke_sampler.mds
            329158
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // cdiready_the_apprentice.mds
            MediaType.CDDA,

            // report_audiocd.mds
            MediaType.CDDA,

            // report_cdrom.mds
            MediaType.CDROM,

            // report_cdrw.mds
            MediaType.CDROM,

            // report_dvdram_v2.mds
            MediaType.DVDROM,

            // report_dvd-r+dl.mds
            MediaType.DVDROM,

            // report_dvdrom.mds
            MediaType.DVDROM,

            // report_enhancedcd.mds
            MediaType.CDPLUS,

            // test_multi_karaoke_sampler.mds
            MediaType.CDROMXA
        };

        public override string[] _md5S => new[]
        {
            // cdiready_the_apprentice.mds
            "f6bd226d3f249fa821460aeb1393cf3b",

            // report_audiocd.mds
            "c96a7bf12427078bab252d941716cc32",

            // report_cdrom.mds
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw.mds
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // report_dvdram_v2.mds
            "35cb08dd5fedfb8e9ad2918292e51791",

            // report_dvd-r+dl.mds
            "1cd9b9be5c5e337c5e6576156b84b726",

            // report_dvdrom.mds
            "5e1841b7cd6ac0a95b8ae6f110fd89f2",

            // report_enhancedcd.mds
            "588d8ff1fef693bbe5719ac6c2f96bc1",

            // test_multi_karaoke_sampler.mds
            "9a19aa0df066732a8ec34025e8160248"
        };

        public override string[] _longMd5S => new[]
        {
            // cdiready_the_apprentice.mds
            "f6bd226d3f249fa821460aeb1393cf3b",

            // report_audiocd.mds
            "c96a7bf12427078bab252d941716cc32",

            // report_cdrom.mds
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw.mds
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_dvdram_v2.mds
            "35cb08dd5fedfb8e9ad2918292e51791",

            // report_dvd-r+dl.mds
            "1cd9b9be5c5e337c5e6576156b84b726",

            // report_dvdrom.mds
            "5e1841b7cd6ac0a95b8ae6f110fd89f2",

            // report_enhancedcd.mds
            "d72e737f49482d1330e8fe03b9f40b79",

            // test_multi_karaoke_sampler.mds
            "e981f7dfdb522ba937fe75474e23a446"
        };

        public override string[] _subchannelMd5S => new string[]
        {
            // cdiready_the_apprentice.mds
            null,

            // report_audiocd.mds
            null,

            // report_cdrom.mds
            null,

            // report_cdrw.mds
            null,

            // report_dvdram_v2.mds
            null,

            // report_dvd-r+dl.mds
            null,

            // report_dvdrom.mds
            null,

            // report_enhancedcd.mds
            null,

            // test_multi_karaoke_sampler.mds
            null
        };

        public override int[] _tracks => new[]
        {
            // cdiready_the_apprentice.mds
            22,

            // report_audiocd.mds
            14,

            // report_cdrom.mds
            1,

            // report_cdrw.mds
            1,

            // report_dvdram_v2.mds
            1,

            // report_dvd-r+dl.mds
            1,

            // report_dvdrom.mds
            1,

            // report_enhancedcd.mds
            14,

            // test_multi_karaoke_sampler.mds
            16
        };

        public override int[][] _trackSessions => new[]
        {
            // cdiready_the_apprentice.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdrom.mds
            new[]
            {
                1
            },

            // report_cdrw.mds
            new[]
            {
                1
            },

            // report_dvdram_v2.mds
            new[]
            {
                1
            },

            // report_dvd-r+dl.mds
            new[]
            {
                1
            },

            // report_dvdrom.mds
            new[]
            {
                1
            },

            // report_enhancedcd.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multi_karaoke_sampler.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // cdiready_the_apprentice.mds
            new ulong[]
            {
                69150, 88800, 107625, 112200, 133650, 138225, 159825, 164775, 185400, 190125, 208875, 213000, 232200,
                236700, 241875, 256125, 256875, 265650, 267375, 270000, 271650, 274275
            },

            // report_audiocd.mds
            new ulong[]
            {
                0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449
            },

            // report_cdrom.mds
            new ulong[]
            {
                0
            },

            // report_cdrw.mds
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.mds
            new ulong[]
            {
                0
            },

            // report_dvd-r+dl.mds
            new ulong[]
            {
                0
            },

            // report_dvdrom.mds
            new ulong[]
            {
                0
            },

            // report_enhancedcd.mds
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234180
            },

            // test_multi_karaoke_sampler.mds
            new ulong[]
            {
                0, 1887, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // cdiready_the_apprentice.mds
            new ulong[]
            {
                88799, 107624, 112199, 133649, 138224, 159824, 164774, 185399, 190124, 208874, 212999, 232199, 236699,
                241874, 256124, 256874, 265649, 267374, 269999, 271649, 274274, 279299
            },

            // report_audiocd.mds
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247072
            },

            // report_cdrom.mds
            new ulong[]
            {
                254264
            },

            // report_cdrw.mds
            new ulong[]
            {
                308223
            },

            // report_dvdram_v2.mds
            new ulong[]
            {
                471089
            },

            // report_dvd-r+dl.mds
            new ulong[]
            {
                3455919
            },

            // report_dvdrom.mds
            new ulong[]
            {
                2146356
            },

            // report_enhancedcd.mds
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },

            // test_multi_karaoke_sampler.mds
            new ulong[]
            {
                1886, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // cdiready_the_apprentice.mds
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.mds
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.mds
            new ulong[]
            {
                150
            },

            // report_cdrw.mds
            new ulong[]
            {
                150
            },

            // report_dvdram_v2.mds
            new ulong[]
            {
                0
            },

            // report_dvd-r+dl.mds
            new ulong[]
            {
                0
            },

            // report_dvdrom.mds
            new ulong[]
            {
                0
            },

            // report_enhancedcd.mds
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_multi_karaoke_sampler.mds
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // cdiready_the_apprentice.mds
            new byte[]
            {
                4
            },

            // report_audiocd.mds
            new byte[]
            {
                4
            },

            // report_cdrom.mds
            new byte[]
            {
                4
            },

            // report_cdrw.mds
            new byte[]
            {
                4
            },

            // report_dvdram_v2.mds
            null,

            // report_dvd-r+dl.mds
            null,

            // report_dvdrom.mds
            null,

            // report_enhancedcd.mds
            new byte[]
            {
                4
            },

            // test_multi_karaoke_sampler.mds
            new byte[]
            {
                4
            }
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "UltraISO", "Alcohol");
        public override IMediaImage _plugin => new DiscImages.Alcohol120();
    }
}