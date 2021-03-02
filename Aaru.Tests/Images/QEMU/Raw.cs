﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Raw.cs
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
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images.QEMU
{
    [TestFixture]
    public class Raw : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "raw.img.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // raw.img.lz
            251904
        };

        public override uint[] _sectorSize => new uint[]
        {
            // raw.img.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // raw.img.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // raw.img.lz
            "4bfc9e9e2dd86aa52ef709e77d2617ed"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "QEMU", "raw");
        public override IMediaImage _plugin => new ZZZRawImage();
    }
}