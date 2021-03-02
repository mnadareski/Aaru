// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Nero.cs
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
    public class Nero : OpticalMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "cdiready_the_apprentice.nrg", "report_audiocd.nrg", "report_cdrom.nrg", "report_cdrw.nrg",
            "report_dvdram_v2.nrg", "report_dvd-r+dl.nrg", "report_dvdrom.nrg", "report_enhancedcd.nrg",
            "test_multi_karaoke_sampler.nrg"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // cdiready_the_apprentice.nrg
            210150,

            // report_audiocd.nrg
            247073,

            // report_cdrom.nrg
            254265,

            // report_cdrw.nrg
            308224,

            // report_dvdram_v2.nrg
            471090,

            // report_dvd-r+dl.nrg
            3455920,

            // report_dvdrom.nrg
            2146357,

            // report_enhancedcd.nrg
            292066,

            // test_multi_karaoke_sampler.nrg
            329008
        };
        public override uint[] _sectorSize => null;

        public override MediaType[] _mediaTypes => new[]
        {
            // cdiready_the_apprentice.nrg
            MediaType.CDDA,

            // report_audiocd.nrg
            MediaType.CDDA,

            // report_cdrom.nrg
            MediaType.CDROM,

            // report_cdrw.nrg
            MediaType.CDROM,

            // report_dvdram_v2.nrg
            MediaType.DVDROM,

            // report_dvd-r+dl.nrg
            MediaType.DVDROM,

            // report_dvdrom.nrg
            MediaType.DVDROM,

            // report_enhancedcd.nrg
            MediaType.CDPLUS,

            // test_multi_karaoke_sampler.nrg
            MediaType.CDROMXA
        };

        public override string[] _md5S => new[]
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // report_audiocd.nrg
            "UNKNOWN",

            // report_cdrom.nrg
            "UNKNOWN",

            // report_cdrw.nrg
            "UNKNOWN",

            // report_dvdram_v2.nrg
            "UNKNOWN",

            // report_dvd-r+dl.nrg
            "UNKNOWN",

            // report_dvdrom.nrg
            "UNKNOWN",

            // report_enhancedcd.nrg
            "UNKNOWN",

            // test_multi_karaoke_sampler.nrg
            "UNKNOWN"
        };

        public override string[] _longMd5S => new[]
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // report_audiocd.nrg
            "UNKNOWN",

            // report_cdrom.nrg
            "UNKNOWN",

            // report_cdrw.nrg
            "UNKNOWN",

            // report_dvdram_v2.nrg
            "UNKNOWN",

            // report_dvd-r+dl.nrg
            "UNKNOWN",

            // report_dvdrom.nrg
            "UNKNOWN",

            // report_enhancedcd.nrg
            "UNKNOWN",

            // test_multi_karaoke_sampler.nrg
            "UNKNOWN"
        };

        public override string[] _subchannelMd5S => new[]
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // report_audiocd.nrg
            "UNKNOWN",

            // report_cdrom.nrg
            "UNKNOWN",

            // report_cdrw.nrg
            "UNKNOWN",

            // report_dvdram_v2.nrg
            null,

            // report_dvd-r+dl.nrg
            null,

            // report_dvdrom.nrg
            null,

            // report_enhancedcd.nrg
            "UNKNOWN",

            // test_multi_karaoke_sampler.nrg
            "UNKNOWN"
        };

        public override int[] _tracks => new[]
        {
            // cdiready_the_apprentice.nrg
            22,

            // report_audiocd.nrg
            14,

            // report_cdrom.nrg
            1,

            // report_cdrw.nrg
            1,

            // report_dvdram_v2.nrg
            1,

            // report_dvd-r+dl.nrg
            1,

            // report_dvdrom.nrg
            1,

            // report_enhancedcd.nrg
            14,

            // test_multi_karaoke_sampler.nrg
            16
        };

        public override int[][] _trackSessions => new[]
        {
            // cdiready_the_apprentice.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdrom.nrg
            new[]
            {
                1
            },

            // report_cdrw.nrg
            new[]
            {
                1
            },

            // report_dvdram_v2.nrg
            new[]
            {
                1
            },

            // report_dvd-r+dl.nrg
            new[]
            {
                1
            },

            // report_dvdrom.nrg
            new[]
            {
                1
            },

            // report_enhancedcd.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_multi_karaoke_sampler.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                0, 88800, 107625, 112200, 133650, 138225, 159825, 164775, 185400, 121125, 139875, 144000, 163200,
                167700, 172875, 190125, 208875, 213000, 232200, 236700, 241875, 256125
            },

            // report_audiocd.nrg
            new ulong[]
            {
                0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154222, 170901, 186689, 201949, 224599
            },

            // report_cdrom.nrg
            new ulong[]
            {
                0
            },

            // report_cdrw.nrg
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.nrg
            new[]
            {
                18446744073709551466
            },

            // report_dvd-r+dl.nrg
            new[]
            {
                18446744073709551466
            },

            // report_dvdrom.nrg
            new[]
            {
                18446744073709551466
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153568, 167082, 187263, 201591, 256021
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                0, 1737, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175732, 206367, 226356, 244261, 273871,
                293658, 175826
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                19649, 38474, 43049, 64499, 69074, 90674, 95624, 116249, 120974, 139874, 143999, 163199, 167699, 172874,
                187124, 121724, 148499, 145574, 165674, 169199, 175349, 191999
            },

            // report_audiocd.nrg
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170900, 186688, 201948, 224598, 247222
            },

            // report_cdrom.nrg
            new ulong[]
            {
                254264
            },

            // report_cdrw.nrg
            new ulong[]
            {
                308223
            },

            // report_dvdram_v2.nrg
            new ulong[]
            {
                471089
            },

            // report_dvd-r+dl.nrg
            new ulong[]
            {
                3455919
            },

            // report_dvdrom.nrg
            new ulong[]
            {
                2146356
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 167081, 187262, 201590, 222929,
                325456
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                1736, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206366, 226355, 244260, 273870,
                293657, 310616, 194272
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                150, 18446744073709482466, 18446744073709482466, 18446744073709482466, 18446744073709482466,
                18446744073709482466, 18446744073709482466, 18446744073709482466, 18446744073709482466, 0, 0, 0, 0, 0,
                0, 18446744073709482466, 18446744073709482466, 18446744073709482466, 18446744073709482466,
                18446744073709482466, 18446744073709482466, 18446744073709482466
            },

            // report_audiocd.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.nrg
            new ulong[]
            {
                150
            },

            // report_cdrw.nrg
            new ulong[]
            {
                150
            },

            // report_dvdram_v2.nrg
            new ulong[]
            {
                0
            },

            // report_dvd-r+dl.nrg
            new ulong[]
            {
                0
            },

            // report_dvdrom.nrg
            new ulong[]
            {
                0
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // cdiready_the_apprentice.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.nrg
            new byte[]
            {
                0
            },

            // report_cdrw.nrg
            new byte[]
            {
                0
            },

            // report_dvdram_v2.nrg
            new byte[]
            {
                0
            },

            // report_dvd-r+dl.nrg
            new byte[]
            {
                0
            },

            // report_dvdrom.nrg
            new byte[]
            {
                0
            },

            // report_enhancedcd.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multi_karaoke_sampler.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "UltraISO", "Nero");
        public override IMediaImage _plugin => new DiscImages.Nero();
    }
}