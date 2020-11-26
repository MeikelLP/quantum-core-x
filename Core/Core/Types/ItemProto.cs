// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace QuantumCore.Core.Types
{
    public partial class ItemProto : KaitaiStruct
    {
        public static ItemProto FromFile(string fileName)
        {
            return new ItemProto(new KaitaiStream(fileName));
        }

        public ItemProto(KaitaiStream p__io, KaitaiStruct p__parent = null, ItemProto p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _fileHeader = new Header(m_io, this, m_root);
            _content = new CryptedData(m_io, this, m_root);
        }
        public partial class ItemLimit : KaitaiStruct
        {
            public static ItemLimit FromFile(string fileName)
            {
                return new ItemLimit(new KaitaiStream(fileName));
            }

            public ItemLimit(KaitaiStream p__io, ItemProto.Item p__parent = null, ItemProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = m_io.ReadU1();
                _value = m_io.ReadU4le();
            }
            private byte _type;
            private uint _value;
            private ItemProto m_root;
            private ItemProto.Item m_parent;
            public byte Type { get { return _type; } }
            public uint Value { get { return _value; } }
            public ItemProto M_Root { get { return m_root; } }
            public ItemProto.Item M_Parent { get { return m_parent; } }
        }
        public partial class ItemsContainer : KaitaiStruct
        {
            public static ItemsContainer FromFile(string fileName)
            {
                return new ItemsContainer(new KaitaiStream(fileName));
            }

            public ItemsContainer(KaitaiStream p__io, ItemProto.CryptedData p__parent = null, ItemProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _items = new List<Item>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _items.Add(new Item(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<Item> _items;
            private ItemProto m_root;
            private ItemProto.CryptedData m_parent;
            public List<Item> Items { get { return _items; } }
            public ItemProto M_Root { get { return m_root; } }
            public ItemProto.CryptedData M_Parent { get { return m_parent; } }
        }
        public partial class CryptedData : KaitaiStruct
        {
            public static CryptedData FromFile(string fileName)
            {
                return new CryptedData(new KaitaiStream(fileName));
            }

            public CryptedData(KaitaiStream p__io, ItemProto p__parent = null, ItemProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 77, 67, 79, 90 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 77, 67, 79, 90 }, Magic, M_Io, "/types/crypted_data/seq/0");
                }
                _cryptedSize = m_io.ReadU4le();
                _decryptedSize = m_io.ReadU4le();
                _realSize = m_io.ReadU4le();
                __raw__raw_data = m_io.ReadBytes(CryptedSize);
                LzoXtea _process__raw__raw_data = new LzoXtea(RealSize, CryptedSize);
                __raw_data = _process__raw__raw_data.Decode(__raw__raw_data);
                var io___raw_data = new KaitaiStream(__raw_data);
                _data = new ItemsContainer(io___raw_data, this, m_root);
            }
            private byte[] _magic;
            private uint _cryptedSize;
            private uint _decryptedSize;
            private uint _realSize;
            private ItemsContainer _data;
            private ItemProto m_root;
            private ItemProto m_parent;
            private byte[] __raw_data;
            private byte[] __raw__raw_data;
            public byte[] Magic { get { return _magic; } }
            public uint CryptedSize { get { return _cryptedSize; } }
            public uint DecryptedSize { get { return _decryptedSize; } }
            public uint RealSize { get { return _realSize; } }
            public ItemsContainer Data { get { return _data; } }
            public ItemProto M_Root { get { return m_root; } }
            public ItemProto M_Parent { get { return m_parent; } }
            public byte[] M_RawData { get { return __raw_data; } }
            public byte[] M_RawM_RawData { get { return __raw__raw_data; } }
        }
        public partial class Header : KaitaiStruct
        {
            public static Header FromFile(string fileName)
            {
                return new Header(new KaitaiStream(fileName));
            }

            public Header(KaitaiStream p__io, ItemProto p__parent = null, ItemProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 77, 73, 80, 88 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 77, 73, 80, 88 }, Magic, M_Io, "/types/header/seq/0");
                }
                _version = m_io.ReadU4le();
                _stride = m_io.ReadU4le();
                _elements = m_io.ReadU4le();
                _size = m_io.ReadU4le();
            }
            private byte[] _magic;
            private uint _version;
            private uint _stride;
            private uint _elements;
            private uint _size;
            private ItemProto m_root;
            private ItemProto m_parent;
            public byte[] Magic { get { return _magic; } }
            public uint Version { get { return _version; } }
            public uint Stride { get { return _stride; } }
            public uint Elements { get { return _elements; } }
            public uint Size { get { return _size; } }
            public ItemProto M_Root { get { return m_root; } }
            public ItemProto M_Parent { get { return m_parent; } }
        }
        public partial class ItemApply : KaitaiStruct
        {
            public static ItemApply FromFile(string fileName)
            {
                return new ItemApply(new KaitaiStream(fileName));
            }

            public ItemApply(KaitaiStream p__io, ItemProto.Item p__parent = null, ItemProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = m_io.ReadU1();
                _value = m_io.ReadU4le();
            }
            private byte _type;
            private uint _value;
            private ItemProto m_root;
            private ItemProto.Item m_parent;
            public byte Type { get { return _type; } }
            public uint Value { get { return _value; } }
            public ItemProto M_Root { get { return m_root; } }
            public ItemProto.Item M_Parent { get { return m_parent; } }
        }
        public partial class Item : KaitaiStruct
        {
            public static Item FromFile(string fileName)
            {
                return new Item(new KaitaiStream(fileName));
            }

            public Item(KaitaiStream p__io, ItemProto.ItemsContainer p__parent = null, ItemProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _id = m_io.ReadU4le();
                _unknown = m_io.ReadU4le();
                _name = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytes(25));
                _translatedName = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytes(25));
                _type = m_io.ReadU1();
                _subtype = m_io.ReadU1();
                _unknown2 = m_io.ReadU1();
                _size = m_io.ReadU1();
                _antiFlags = m_io.ReadU4le();
                _flags = m_io.ReadU4le();
                _wearFlags = m_io.ReadU4le();
                _immuneFlags = m_io.ReadU4le();
                _buyPrice = m_io.ReadU4le();
                _sellPrice = m_io.ReadU4le();
                _limits = new List<ItemLimit>((int) (2));
                for (var i = 0; i < 2; i++)
                {
                    _limits.Add(new ItemLimit(m_io, this, m_root));
                }
                _applies = new List<ItemApply>((int) (3));
                for (var i = 0; i < 3; i++)
                {
                    _applies.Add(new ItemApply(m_io, this, m_root));
                }
                _values = new List<int>((int) (6));
                for (var i = 0; i < 6; i++)
                {
                    _values.Add(m_io.ReadS4le());
                }
                _sockets = new List<int>((int) (3));
                for (var i = 0; i < 3; i++)
                {
                    _sockets.Add(m_io.ReadS4le());
                }
                _upgradeId = m_io.ReadU4le();
                _upgradeSet = m_io.ReadU2le();
                _magicItemPercentage = m_io.ReadU1();
                _specular = m_io.ReadU1();
                _socketPercentage = m_io.ReadU1();
            }
            private uint _id;
            private uint _unknown;
            private string _name;
            private string _translatedName;
            private byte _type;
            private byte _subtype;
            private byte _unknown2;
            private byte _size;
            private uint _antiFlags;
            private uint _flags;
            private uint _wearFlags;
            private uint _immuneFlags;
            private uint _buyPrice;
            private uint _sellPrice;
            private List<ItemLimit> _limits;
            private List<ItemApply> _applies;
            private List<int> _values;
            private List<int> _sockets;
            private uint _upgradeId;
            private ushort _upgradeSet;
            private byte _magicItemPercentage;
            private byte _specular;
            private byte _socketPercentage;
            private ItemProto m_root;
            private ItemProto.ItemsContainer m_parent;
            public uint Id { get { return _id; } }
            public uint Unknown { get { return _unknown; } }
            public string Name { get { return _name; } }
            public string TranslatedName { get { return _translatedName; } }
            public byte Type { get { return _type; } }
            public byte Subtype { get { return _subtype; } }
            public byte Unknown2 { get { return _unknown2; } }
            public byte Size { get { return _size; } }
            public uint AntiFlags { get { return _antiFlags; } }
            public uint Flags { get { return _flags; } }
            public uint WearFlags { get { return _wearFlags; } }
            public uint ImmuneFlags { get { return _immuneFlags; } }
            public uint BuyPrice { get { return _buyPrice; } }
            public uint SellPrice { get { return _sellPrice; } }
            public List<ItemLimit> Limits { get { return _limits; } }
            public List<ItemApply> Applies { get { return _applies; } }
            public List<int> Values { get { return _values; } }
            public List<int> Sockets { get { return _sockets; } }
            public uint UpgradeId { get { return _upgradeId; } }
            public ushort UpgradeSet { get { return _upgradeSet; } }
            public byte MagicItemPercentage { get { return _magicItemPercentage; } }
            public byte Specular { get { return _specular; } }
            public byte SocketPercentage { get { return _socketPercentage; } }
            public ItemProto M_Root { get { return m_root; } }
            public ItemProto.ItemsContainer M_Parent { get { return m_parent; } }
        }
        private Header _fileHeader;
        private CryptedData _content;
        private ItemProto m_root;
        private KaitaiStruct m_parent;
        public Header FileHeader { get { return _fileHeader; } }
        public CryptedData Content { get { return _content; } }
        public ItemProto M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
