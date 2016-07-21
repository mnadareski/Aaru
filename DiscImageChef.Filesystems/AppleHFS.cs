/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : AppleHFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies HFS filesystems and shows information.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
using DiscImageChef.Console;


namespace DiscImageChef.Filesystems
{
    class AppleHFS : Filesystem
    {
        /// <summary>
        /// "BD", HFS magic
        /// </summary>
        const UInt16 HFS_MAGIC = 0x4244;
        /// <summary>
        /// "H+", HFS+ magic
        /// </summary>
        const UInt16 HFSP_MAGIC = 0x482B;
        /// <summary>
        /// "LK", HFS bootblock magic
        /// </summary>
        const UInt16 HFSBB_MAGIC = 0x4C4B;

        public AppleHFS()
        {
            Name = "Apple Hierarchical File System";
            PluginUUID = new Guid("36405F8D-0D26-6ECC-0BBB-1D5225FF404F");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if((2 + partitionStart) >= imagePlugin.GetSectors())
                return false;

            byte[] mdb_sector;
            UInt16 drSigWord;

            if(imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448 || imagePlugin.GetSectorSize() == 2048)
            {
                mdb_sector = imagePlugin.ReadSector(2 + partitionStart);
                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

                if(drSigWord == HFS_MAGIC)
                {
                    drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x7C); // Seek to embedded HFS+ signature

                    return drSigWord != HFSP_MAGIC;
                }
                mdb_sector = Read2048SectorAs512(imagePlugin, 2 + partitionStart);
                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

                if(drSigWord == HFS_MAGIC)
                {
                    DicConsole.DebugWriteLine("HFS plugin", "HFS sector size is 512 bytes, but device's 2048");

                    drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x7C); // Seek to embedded HFS+ signature

                    return drSigWord != HFSP_MAGIC;
                }
            }
            else
            {
                mdb_sector = imagePlugin.ReadSector(2 + partitionStart);
                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

                if(drSigWord == HFS_MAGIC)
                {
                    drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x7C); // Seek to embedded HFS+ signature

                    return drSigWord != HFSP_MAGIC;
                }
            }
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();

            HFS_MasterDirectoryBlock MDB = new HFS_MasterDirectoryBlock();
            HFS_BootBlock BB = new HFS_BootBlock();

            byte[] pString;

            byte[] bb_sector;
            byte[] mdb_sector;
            UInt16 drSigWord;

            bool APMFromHDDOnCD = false;

            if(imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448 || imagePlugin.GetSectorSize() == 2048)
            {
                mdb_sector = imagePlugin.ReadSector(2 + partitionStart);
                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

                if(drSigWord == HFS_MAGIC)
                {
                    bb_sector = imagePlugin.ReadSector(partitionStart);
                }
                else
                {
                    mdb_sector = Read2048SectorAs512(imagePlugin, 2 + partitionStart);
                    drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

                    if(drSigWord == HFS_MAGIC)
                    {
                        bb_sector = Read2048SectorAs512(imagePlugin, partitionStart);
                        APMFromHDDOnCD = true;
                    }
                    else
                        return;
                }
            }
            else
            {
                mdb_sector = imagePlugin.ReadSector(2 + partitionStart);
                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

                if(drSigWord == HFS_MAGIC)
                    bb_sector = imagePlugin.ReadSector(partitionStart);
                else
                    return;
            }

            MDB.drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x000);
            if(MDB.drSigWord != HFS_MAGIC)
                return;

            MDB.drCrDate = BigEndianBitConverter.ToUInt32(mdb_sector, 0x002);
            MDB.drLsMod = BigEndianBitConverter.ToUInt32(mdb_sector, 0x006);
            MDB.drAtrb = BigEndianBitConverter.ToUInt16(mdb_sector, 0x00A);
            MDB.drNmFls = BigEndianBitConverter.ToUInt16(mdb_sector, 0x00C);
            MDB.drVBMSt = BigEndianBitConverter.ToUInt16(mdb_sector, 0X00E);
            MDB.drAllocPtr = BigEndianBitConverter.ToUInt16(mdb_sector, 0x010);
            MDB.drNmAlBlks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x012);
            MDB.drAlBlkSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x014);
            MDB.drClpSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x018);
            MDB.drAlBlSt = BigEndianBitConverter.ToUInt16(mdb_sector, 0x01C);
            MDB.drNxtCNID = BigEndianBitConverter.ToUInt32(mdb_sector, 0x01E);
            MDB.drFreeBks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x022);
            pString = new byte[28];
            Array.Copy(mdb_sector, 0x024, pString, 0, 28);
            MDB.drVN = StringHandlers.PascalToString(pString);

            MDB.drVolBkUp = BigEndianBitConverter.ToUInt32(mdb_sector, 0x040);
            MDB.drVSeqNum = BigEndianBitConverter.ToUInt16(mdb_sector, 0x044);
            MDB.drWrCnt = BigEndianBitConverter.ToUInt32(mdb_sector, 0x046);
            MDB.drXTClpSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x04A);
            MDB.drCTClpSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x04E);
            MDB.drNmRtDirs = BigEndianBitConverter.ToUInt16(mdb_sector, 0x052);
            MDB.drFilCnt = BigEndianBitConverter.ToUInt32(mdb_sector, 0x054);
            MDB.drDirCnt = BigEndianBitConverter.ToUInt32(mdb_sector, 0x058);

            MDB.drFndrInfo0 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x05C);
            MDB.drFndrInfo1 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x060);
            MDB.drFndrInfo2 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x064);
            MDB.drFndrInfo3 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x068);
            MDB.drFndrInfo4 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x06C);
            MDB.drFndrInfo5 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x070);
            MDB.drFndrInfo6 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x074);
            MDB.drFndrInfo7 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x078);

            MDB.drVCSize = BigEndianBitConverter.ToUInt16(mdb_sector, 0x07C);
            MDB.drVBMCSize = BigEndianBitConverter.ToUInt16(mdb_sector, 0x07E);
            MDB.drCtlCSize = BigEndianBitConverter.ToUInt16(mdb_sector, 0x080);

            // For HFS+ embedded volume
            MDB.drEmbedSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x07C);
            MDB.xdrStABNt = BigEndianBitConverter.ToUInt16(mdb_sector, 0x07E);
            MDB.xdrNumABlks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x080);

            MDB.drXTFlSize = BigEndianBitConverter.ToUInt32(mdb_sector, 0x082);
            MDB.drCTFlSize = BigEndianBitConverter.ToUInt32(mdb_sector, 0x092);

            BB.signature = BigEndianBitConverter.ToUInt16(bb_sector, 0x000);

            if(BB.signature == HFSBB_MAGIC)
            {
                BB.branch = BigEndianBitConverter.ToUInt32(bb_sector, 0x002);
                BB.boot_flags = bb_sector[0x006];
                BB.boot_version = bb_sector[0x007];

                BB.sec_sv_pages = BigEndianBitConverter.ToInt16(bb_sector, 0x008);

                pString = new byte[16];
                Array.Copy(bb_sector, 0x00A, pString, 0, 16);
                BB.system_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x01A, pString, 0, 16);
                BB.finder_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x02A, pString, 0, 16);
                BB.debug_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x03A, pString, 0, 16);
                BB.disasm_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x04A, pString, 0, 16);
                BB.stupscr_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x05A, pString, 0, 16);
                BB.bootup_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x06A, pString, 0, 16);
                BB.clipbrd_name = StringHandlers.PascalToString(pString);

                BB.max_files = BigEndianBitConverter.ToUInt16(bb_sector, 0x07A);
                BB.queue_size = BigEndianBitConverter.ToUInt16(bb_sector, 0x07C);
                BB.heap_128k = BigEndianBitConverter.ToUInt32(bb_sector, 0x07E);
                BB.heap_256k = BigEndianBitConverter.ToUInt32(bb_sector, 0x082);
                BB.heap_512k = BigEndianBitConverter.ToUInt32(bb_sector, 0x086);
            }
            else
                BB.signature = 0x0000;

            sb.AppendLine("Apple Hierarchical File System");
            sb.AppendLine();
            if(APMFromHDDOnCD)
                sb.AppendLine("HFS uses 512 bytes/sector while devices uses 2048 bytes/sector.").AppendLine();
            sb.AppendLine("Master Directory Block:");
            sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(MDB.drCrDate)).AppendLine();
            sb.AppendFormat("Last modification date: {0}", DateHandlers.MacToDateTime(MDB.drLsMod)).AppendLine();
            sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(MDB.drVolBkUp)).AppendLine();
            sb.AppendFormat("Backup sequence number: {0}", MDB.drVSeqNum).AppendLine();

            if((MDB.drAtrb & 0x80) == 0x80)
                sb.AppendLine("Volume is locked by hardware.");
            if((MDB.drAtrb & 0x100) == 0x100)
                sb.AppendLine("Volume was unmonted.");
            else
                sb.AppendLine("Volume is mounted.");
            if((MDB.drAtrb & 0x200) == 0x200)
                sb.AppendLine("Volume has spared bad blocks.");
            if((MDB.drAtrb & 0x400) == 0x400)
                sb.AppendLine("Volume does not need cache.");
            if((MDB.drAtrb & 0x800) == 0x800)
                sb.AppendLine("Boot volume is inconsistent.");
            if((MDB.drAtrb & 0x1000) == 0x1000)
                sb.AppendLine("There are reused CNIDs.");
            if((MDB.drAtrb & 0x2000) == 0x2000)
                sb.AppendLine("Volume is journaled.");
            if((MDB.drAtrb & 0x4000) == 0x4000)
                sb.AppendLine("Volume is seriously inconsistent.");
            if((MDB.drAtrb & 0x8000) == 0x8000)
                sb.AppendLine("Volume is locked by software.");

            sb.AppendFormat("{0} files on root directory", MDB.drNmFls).AppendLine();
            sb.AppendFormat("{0} directories on root directory", MDB.drNmRtDirs).AppendLine();
            sb.AppendFormat("{0} files on volume", MDB.drFilCnt).AppendLine();
            sb.AppendFormat("{0} directories on volume", MDB.drDirCnt).AppendLine();
            sb.AppendFormat("Volume write count: {0}", MDB.drWrCnt).AppendLine();

            sb.AppendFormat("Volume bitmap starting sector (in 512-bytes): {0}", MDB.drVBMSt).AppendLine();
            sb.AppendFormat("Next allocation block: {0}.", MDB.drAllocPtr).AppendLine();
            sb.AppendFormat("{0} volume allocation blocks.", MDB.drNmAlBlks).AppendLine();
            sb.AppendFormat("{0} bytes per allocation block.", MDB.drAlBlkSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a file.", MDB.drClpSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a Extents B-Tree.", MDB.drXTClpSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a Catalog B-Tree.", MDB.drCTClpSiz).AppendLine();
            sb.AppendFormat("Sector of first allocation block: {0}", MDB.drAlBlSt).AppendLine();
            sb.AppendFormat("Next unused CNID: {0}", MDB.drNxtCNID).AppendLine();
            sb.AppendFormat("{0} unused allocation blocks.", MDB.drFreeBks).AppendLine();

            sb.AppendFormat("{0} bytes in the Extents B-Tree", MDB.drXTFlSize).AppendLine();
            sb.AppendFormat("{0} bytes in the Catalog B-Tree", MDB.drCTFlSize).AppendLine();

            sb.AppendFormat("Volume name: {0}", MDB.drVN).AppendLine();

            sb.AppendLine("Finder info:");
            sb.AppendFormat("CNID of bootable system's directory: {0}", MDB.drFndrInfo0).AppendLine();
            sb.AppendFormat("CNID of first-run application's directory: {0}", MDB.drFndrInfo1).AppendLine();
            sb.AppendFormat("CNID of previously opened directory: {0}", MDB.drFndrInfo2).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS 8 or 9 directory: {0}", MDB.drFndrInfo3).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS X directory: {0}", MDB.drFndrInfo5).AppendLine();
            sb.AppendFormat("Mac OS X Volume ID: {0:X8}{1:X8}", MDB.drFndrInfo6, MDB.drFndrInfo7).AppendLine();

            if(MDB.drEmbedSigWord == HFSP_MAGIC)
            {
                sb.AppendLine("Volume wraps a HFS+ volume.");
                sb.AppendFormat("Starting block of the HFS+ volume: {0}", MDB.xdrStABNt).AppendLine();
                sb.AppendFormat("Allocations blocks of the HFS+ volume: {0}", MDB.xdrNumABlks).AppendLine();
            }
            else
            {
                sb.AppendFormat("{0} blocks in volume cache", MDB.drVCSize).AppendLine();
                sb.AppendFormat("{0} blocks in volume bitmap cache", MDB.drVBMCSize).AppendLine();
                sb.AppendFormat("{0} blocks in volume common cache", MDB.drCtlCSize).AppendLine();
            }

            if(BB.signature == HFSBB_MAGIC)
            {
                sb.AppendLine("Volume is bootable.");
                sb.AppendLine();
                sb.AppendLine("Boot Block:");
                if((BB.boot_flags & 0x40) == 0x40)
                    sb.AppendLine("Boot block should be executed.");
                if((BB.boot_flags & 0x80) == 0x80)
                {
                    sb.AppendLine("Boot block is in new unknown format.");
                }
                else
                {
                    if(BB.sec_sv_pages > 0)
                        sb.AppendLine("Allocate secondary sound buffer at boot.");
                    else if(BB.sec_sv_pages < 0)
                        sb.AppendLine("Allocate secondary sound and video buffers at boot.");

                    sb.AppendFormat("System filename: {0}", BB.system_name).AppendLine();
                    sb.AppendFormat("Finder filename: {0}", BB.finder_name).AppendLine();
                    sb.AppendFormat("Debugger filename: {0}", BB.debug_name).AppendLine();
                    sb.AppendFormat("Disassembler filename: {0}", BB.disasm_name).AppendLine();
                    sb.AppendFormat("Startup screen filename: {0}", BB.stupscr_name).AppendLine();
                    sb.AppendFormat("First program to execute at boot: {0}", BB.bootup_name).AppendLine();
                    sb.AppendFormat("Clipboard filename: {0}", BB.clipbrd_name).AppendLine();
                    sb.AppendFormat("Maximum opened files: {0}", BB.max_files * 4).AppendLine();
                    sb.AppendFormat("Event queue size: {0}", BB.queue_size).AppendLine();
                    sb.AppendFormat("Heap size with 128KiB of RAM: {0} bytes", BB.heap_128k).AppendLine();
                    sb.AppendFormat("Heap size with 256KiB of RAM: {0} bytes", BB.heap_256k).AppendLine();
                    sb.AppendFormat("Heap size with 512KiB of RAM or more: {0} bytes", BB.heap_512k).AppendLine();
                }
            }
            else
                sb.AppendLine("Volume is not bootable.");

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType();
            if(MDB.drVolBkUp > 0)
            {
                xmlFSType.BackupDate = DateHandlers.MacToDateTime(MDB.drVolBkUp);
                xmlFSType.BackupDateSpecified = true;
            }
            xmlFSType.Bootable = BB.signature == HFSBB_MAGIC;
            xmlFSType.Clusters = MDB.drNmAlBlks;
            xmlFSType.ClusterSize = (int)MDB.drAlBlkSiz;
            if(MDB.drCrDate > 0)
            {
                xmlFSType.CreationDate = DateHandlers.MacToDateTime(MDB.drCrDate);
                xmlFSType.CreationDateSpecified = true;
            }
            xmlFSType.Dirty = (MDB.drAtrb & 0x100) != 0x100;
            xmlFSType.Files = MDB.drFilCnt;
            xmlFSType.FilesSpecified = true;
            xmlFSType.FreeClusters = MDB.drFreeBks;
            xmlFSType.FreeClustersSpecified = true;
            if(MDB.drLsMod > 0)
            {
                xmlFSType.ModificationDate = DateHandlers.MacToDateTime(MDB.drLsMod);
                xmlFSType.ModificationDateSpecified = true;
            }
            xmlFSType.Type = "HFS";
            xmlFSType.VolumeName = MDB.drVN;
            if(MDB.drFndrInfo6 != 0 && MDB.drFndrInfo7 != 0)
                xmlFSType.VolumeSerial = String.Format("{0:X8}{1:x8}", MDB.drFndrInfo6, MDB.drFndrInfo7);

            return;
        }

        static byte[] Read2048SectorAs512(ImagePlugins.ImagePlugin imagePlugin, UInt64 LBA)
        {
            UInt64 LBA2k = LBA / 4;
            int Remainder = (int)(LBA % 4);

            byte[] buffer = imagePlugin.ReadSector(LBA2k);
            byte[] sector = new byte[512];

            Array.Copy(buffer, Remainder * 512, sector, 0, 512);

            return sector;
        }

        /// <summary>
        /// Master Directory Block, should be sector 2 in volume
        /// </summary>
        struct HFS_MasterDirectoryBlock // Should be sector 2 in volume
        {
            /// <summary>0x000, Signature, 0x4244</summary>
            public UInt16 drSigWord;
            /// <summary>0x002, Volume creation date</summary>
            public UInt32 drCrDate;
            /// <summary>0x006, Volume last modification date</summary>
            public UInt32 drLsMod;
            /// <summary>0x00A, Volume attributes</summary>
            public UInt16 drAtrb;
            /// <summary>0x00C, Files in root directory</summary>
            public UInt16 drNmFls;
            /// <summary>0x00E, Start 512-byte sector of volume bitmap</summary>
            public UInt16 drVBMSt;
            /// <summary>0x010, Allocation block to begin next allocation</summary>
            public UInt16 drAllocPtr;
            /// <summary>0x012, Allocation blocks</summary>
            public UInt16 drNmAlBlks;
            /// <summary>0x014, Bytes per allocation block</summary>
            public UInt32 drAlBlkSiz;
            /// <summary>0x018, Bytes to allocate when extending a file</summary>
            public UInt32 drClpSiz;
            /// <summary>0x01C, Start 512-byte sector of first allocation block</summary>
            public UInt16 drAlBlSt;
            /// <summary>0x01E, CNID for next file</summary>
            public UInt32 drNxtCNID;
            /// <summary>0x022, Free allocation blocks</summary>
            public UInt16 drFreeBks;
            /// <summary>0x024, Volume name (28 bytes)</summary>
            public string drVN;
            /// <summary>0x040, Volume last backup time</summary>
            public UInt32 drVolBkUp;
            /// <summary>0x044, Volume backup sequence number</summary>
            public UInt16 drVSeqNum;
            /// <summary>0x046, Filesystem write count</summary>
            public UInt32 drWrCnt;
            /// <summary>0x04A, Bytes to allocate when extending the extents B-Tree</summary>
            public UInt32 drXTClpSiz;
            /// <summary>0x04E, Bytes to allocate when extending the catalog B-Tree</summary>
            public UInt32 drCTClpSiz;
            /// <summary>0x052, Number of directories in root directory</summary>
            public UInt16 drNmRtDirs;
            /// <summary>0x054, Number of files in the volume</summary>
            public UInt32 drFilCnt;
            /// <summary>0x058, Number of directories in the volume</summary>
            public UInt32 drDirCnt;
            /// <summary>0x05C, finderInfo[0], CNID for bootable system's directory</summary>
            public UInt32 drFndrInfo0;
            /// <summary>0x060, finderInfo[1], CNID of the directory containing the boot application</summary>
            public UInt32 drFndrInfo1;
            /// <summary>0x064, finderInfo[2], CNID of the directory that should be opened on boot</summary>
            public UInt32 drFndrInfo2;
            /// <summary>0x068, finderInfo[3], CNID for Mac OS 8 or 9 directory</summary>
            public UInt32 drFndrInfo3;
            /// <summary>0x06C, finderInfo[4], Reserved</summary>
            public UInt32 drFndrInfo4;
            /// <summary>0x070, finderInfo[5], CNID for Mac OS X directory</summary>
            public UInt32 drFndrInfo5;
            /// <summary>0x074, finderInfo[6], first part of Mac OS X volume ID</summary>
            public UInt32 drFndrInfo6;
            /// <summary>0x078, finderInfo[7], second part of Mac OS X volume ID</summary>
            public UInt32 drFndrInfo7;
            // If wrapping HFS+
            /// <summary>0x07C, Embedded volume signature, "H+" if HFS+ is embedded ignore following two fields if not</summary>
            public UInt16 drEmbedSigWord;
            /// <summary>0x07E, Starting block number of embedded HFS+ volume</summary>
            public UInt16 xdrStABNt;
            /// <summary>0x080, Allocation blocks used by embedded volume</summary>
            public UInt16 xdrNumABlks;
            // If not
            /// <summary>0x07C, Size in blocks of volume cache</summary>
            public UInt16 drVCSize;
            /// <summary>0x07E, Size in blocks of volume bitmap cache</summary>
            public UInt16 drVBMCSize;
            /// <summary>0x080, Size in blocks of volume common cache</summary>
            public UInt16 drCtlCSize;
            // End of variable variables :D
            /// <summary>0x082, Bytes in the extents B-Tree
            /// 3 HFS extents following, 32 bits each</summary>
            public UInt32 drXTFlSize;
            /// <summary>0x092, Bytes in the catalog B-Tree
            /// 3 HFS extents following, 32 bits each</summary>
            public UInt32 drCTFlSize;
        }

        /// <summary>
        /// Should be sectors 0 and 1 in volume, followed by boot code
        /// </summary>
        struct HFS_BootBlock // Should be sectors 0 and 1 in volume
        {
            /// <summary>0x000, Signature, 0x4C4B if bootable</summary>
            public UInt16 signature;
            /// <summary>0x002, Branch</summary>
            public UInt32 branch;
            /// <summary>0x006, Boot block flags</summary>
            public byte boot_flags;
            /// <summary>0x007, Boot block version</summary>
            public byte boot_version;
            /// <summary>0x008, Allocate secondary buffers</summary>
            public Int16 sec_sv_pages;
            /// <summary>0x00A, System file name (16 bytes)</summary>
            public string system_name;
            /// <summary>0x01A, Finder file name (16 bytes)</summary>
            public string finder_name;
            /// <summary>0x02A, Debugger file name (16 bytes)</summary>
            public string debug_name;
            /// <summary>0x03A, Disassembler file name (16 bytes)</summary>
            public string disasm_name;
            /// <summary>0x04A, Startup screen file name (16 bytes)</summary>
            public string stupscr_name;
            /// <summary>0x05A, First program to execute on boot (16 bytes)</summary>
            public string bootup_name;
            /// <summary>0x06A, Clipboard file name (16 bytes)</summary>
            public string clipbrd_name;
            /// <summary>0x07A, 1/4 of maximum opened at a time files</summary>
            public UInt16 max_files;
            /// <summary>0x07C, Event queue size</summary>
            public UInt16 queue_size;
            /// <summary>0x07E, Heap size on a Mac with 128KiB of RAM</summary>
            public UInt32 heap_128k;
            /// <summary>0x082, Heap size on a Mac with 256KiB of RAM</summary>
            public UInt32 heap_256k;
            /// <summary>0x086, Heap size on a Mac with 512KiB of RAM or more</summary>
            public UInt32 heap_512k;
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}
