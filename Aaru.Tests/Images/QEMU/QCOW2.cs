﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : QCOW2.cs
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
    public class QCOW2 : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "qcow2.qc2.lz", "qcow2_compressed.qc2.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // qcow2.qc2.lz
            251904,

            // qcow2_compressed.qc2.lz
            251904
        };

        public override uint[] _sectorSize => new uint[]
        {
            // qcow2.qc2.lz
            512,

            // qcow2_compressed.qc2.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // qcow2.qc2.lz
            MediaType.GENERIC_HDD,

            // qcow2_compressed.qc2.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // qcow2.qc2.lz
            "4bfc9e9e2dd86aa52ef709e77d2617ed",

            // qcow2_compressed.qc2.lz
            "4bfc9e9e2dd86aa52ef709e77d2617ed"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "QEMU", "QEMU Copy On Write 2");
        public override IMediaImage _plugin => new Qcow2();
    }
}