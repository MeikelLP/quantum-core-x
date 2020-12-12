// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace QuantumCore.Core.Types
{
    public partial class MobProto : KaitaiStruct
    {
        public static MobProto FromFile(string fileName)
        {
            return new MobProto(new KaitaiStream(fileName));
        }

        public MobProto(KaitaiStream p__io, KaitaiStruct p__parent = null, MobProto p__root = null) : base(p__io)
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
        public partial class MonsterSkill : KaitaiStruct
        {
            public static MonsterSkill FromFile(string fileName)
            {
                return new MonsterSkill(new KaitaiStream(fileName));
            }

            public MonsterSkill(KaitaiStream p__io, MobProto.Monster p__parent = null, MobProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _id = m_io.ReadU4le();
                _level = m_io.ReadU1();
            }
            private uint _id;
            private byte _level;
            private MobProto m_root;
            private MobProto.Monster m_parent;
            public uint Id { get { return _id; } }
            public byte Level { get { return _level; } }
            public MobProto M_Root { get { return m_root; } }
            public MobProto.Monster M_Parent { get { return m_parent; } }
        }
        public partial class Monster : KaitaiStruct
        {
            public static Monster FromFile(string fileName)
            {
                return new Monster(new KaitaiStream(fileName));
            }

            public Monster(KaitaiStream p__io, MobProto.MobsContainer p__parent = null, MobProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _id = m_io.ReadU4le();
                _name = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytes(25));
                _translatedName = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytes(25));
                _type = m_io.ReadU1();
                _rank = m_io.ReadU1();
                _battleType = m_io.ReadU1();
                _level = m_io.ReadU1();
                _size = m_io.ReadU1();
                _minGold = m_io.ReadU4le();
                _maxGold = m_io.ReadU4le();
                _experience = m_io.ReadU4le();
                _hp = m_io.ReadU4le();
                _regenDelay = m_io.ReadU1();
                _regenPercentage = m_io.ReadU1();
                _defence = m_io.ReadU2le();
                _aiFlag = m_io.ReadU4le();
                _raceFlag = m_io.ReadU4le();
                _immuneFlag = m_io.ReadU4le();
                _st = m_io.ReadU1();
                _dx = m_io.ReadU1();
                _ht = m_io.ReadU1();
                _iq = m_io.ReadU1();
                _damageRange = new List<uint>((int) (2));
                for (var i = 0; i < 2; i++)
                {
                    _damageRange.Add(m_io.ReadU4le());
                }
                _attackSpeed = m_io.ReadS2le();
                _moveSpeed = m_io.ReadS2le();
                _aggressivePct = m_io.ReadU1();
                _aggressiveSight = m_io.ReadU2le();
                _attackRange = m_io.ReadU2le();
                _enchantments = new List<byte>((int) (6));
                for (var i = 0; i < 6; i++)
                {
                    _enchantments.Add(m_io.ReadU1());
                }
                _resists = new List<byte>((int) (11));
                for (var i = 0; i < 11; i++)
                {
                    _resists.Add(m_io.ReadU1());
                }
                _resurrectionId = m_io.ReadU4le();
                _dropItemId = m_io.ReadU4le();
                _mountCapacity = m_io.ReadU1();
                _onClickType = m_io.ReadU1();
                _empire = m_io.ReadU1();
                _folder = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytes(65));
                _damageMultiply = m_io.ReadF4le();
                _summonId = m_io.ReadU4le();
                _drainSp = m_io.ReadU4le();
                _monsterColor = m_io.ReadU4le();
                _polymorphItemId = m_io.ReadU4le();
                _skills = new List<MonsterSkill>((int) (5));
                for (var i = 0; i < 5; i++)
                {
                    _skills.Add(new MonsterSkill(m_io, this, m_root));
                }
                _berserkPoint = m_io.ReadU1();
                _stoneSkinPoint = m_io.ReadU1();
                _godSpeedPoint = m_io.ReadU1();
                _deathBlowPoint = m_io.ReadU1();
                _revivePoint = m_io.ReadU1();
            }
            private uint _id;
            private string _name;
            private string _translatedName;
            private byte _type;
            private byte _rank;
            private byte _battleType;
            private byte _level;
            private byte _size;
            private uint _minGold;
            private uint _maxGold;
            private uint _experience;
            private uint _hp;
            private byte _regenDelay;
            private byte _regenPercentage;
            private ushort _defence;
            private uint _aiFlag;
            private uint _raceFlag;
            private uint _immuneFlag;
            private byte _st;
            private byte _dx;
            private byte _ht;
            private byte _iq;
            private List<uint> _damageRange;
            private short _attackSpeed;
            private short _moveSpeed;
            private byte _aggressivePct;
            private ushort _aggressiveSight;
            private ushort _attackRange;
            private List<byte> _enchantments;
            private List<byte> _resists;
            private uint _resurrectionId;
            private uint _dropItemId;
            private byte _mountCapacity;
            private byte _onClickType;
            private byte _empire;
            private string _folder;
            private float _damageMultiply;
            private uint _summonId;
            private uint _drainSp;
            private uint _monsterColor;
            private uint _polymorphItemId;
            private List<MonsterSkill> _skills;
            private byte _berserkPoint;
            private byte _stoneSkinPoint;
            private byte _godSpeedPoint;
            private byte _deathBlowPoint;
            private byte _revivePoint;
            private MobProto m_root;
            private MobProto.MobsContainer m_parent;
            public uint Id { get { return _id; } }
            public string Name { get { return _name; } }
            public string TranslatedName { get { return _translatedName; } }
            public byte Type { get { return _type; } }
            public byte Rank { get { return _rank; } }
            public byte BattleType { get { return _battleType; } }
            public byte Level { get { return _level; } }
            public byte Size { get { return _size; } }
            public uint MinGold { get { return _minGold; } }
            public uint MaxGold { get { return _maxGold; } }
            public uint Experience { get { return _experience; } }
            public uint Hp { get { return _hp; } }
            public byte RegenDelay { get { return _regenDelay; } }
            public byte RegenPercentage { get { return _regenPercentage; } }
            public ushort Defence { get { return _defence; } }
            public uint AiFlag { get { return _aiFlag; } }
            public uint RaceFlag { get { return _raceFlag; } }
            public uint ImmuneFlag { get { return _immuneFlag; } }
            public byte St { get { return _st; } }
            public byte Dx { get { return _dx; } }
            public byte Ht { get { return _ht; } }
            public byte Iq { get { return _iq; } }
            public List<uint> DamageRange { get { return _damageRange; } }
            public short AttackSpeed { get { return _attackSpeed; } }
            public short MoveSpeed { get { return _moveSpeed; } }
            public byte AggressivePct { get { return _aggressivePct; } }
            public ushort AggressiveSight { get { return _aggressiveSight; } }
            public ushort AttackRange { get { return _attackRange; } }
            public List<byte> Enchantments { get { return _enchantments; } }
            public List<byte> Resists { get { return _resists; } }
            public uint ResurrectionId { get { return _resurrectionId; } }
            public uint DropItemId { get { return _dropItemId; } }
            public byte MountCapacity { get { return _mountCapacity; } }
            public byte OnClickType { get { return _onClickType; } }
            public byte Empire { get { return _empire; } }
            public string Folder { get { return _folder; } }
            public float DamageMultiply { get { return _damageMultiply; } }
            public uint SummonId { get { return _summonId; } }
            public uint DrainSp { get { return _drainSp; } }
            public uint MonsterColor { get { return _monsterColor; } }
            public uint PolymorphItemId { get { return _polymorphItemId; } }
            public List<MonsterSkill> Skills { get { return _skills; } }
            public byte BerserkPoint { get { return _berserkPoint; } }
            public byte StoneSkinPoint { get { return _stoneSkinPoint; } }
            public byte GodSpeedPoint { get { return _godSpeedPoint; } }
            public byte DeathBlowPoint { get { return _deathBlowPoint; } }
            public byte RevivePoint { get { return _revivePoint; } }
            public MobProto M_Root { get { return m_root; } }
            public MobProto.MobsContainer M_Parent { get { return m_parent; } }
        }
        public partial class CryptedData : KaitaiStruct
        {
            public static CryptedData FromFile(string fileName)
            {
                return new CryptedData(new KaitaiStream(fileName));
            }

            public CryptedData(KaitaiStream p__io, MobProto p__parent = null, MobProto p__root = null) : base(p__io)
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
                LzoXtea _process__raw__raw_data = new LzoXtea(RealSize, CryptedSize, 4813894, 18955, 552631, 6822045);
                __raw_data = _process__raw__raw_data.Decode(__raw__raw_data);
                var io___raw_data = new KaitaiStream(__raw_data);
                _data = new MobsContainer(io___raw_data, this, m_root);
            }
            private byte[] _magic;
            private uint _cryptedSize;
            private uint _decryptedSize;
            private uint _realSize;
            private MobsContainer _data;
            private MobProto m_root;
            private MobProto m_parent;
            private byte[] __raw_data;
            private byte[] __raw__raw_data;
            public byte[] Magic { get { return _magic; } }
            public uint CryptedSize { get { return _cryptedSize; } }
            public uint DecryptedSize { get { return _decryptedSize; } }
            public uint RealSize { get { return _realSize; } }
            public MobsContainer Data { get { return _data; } }
            public MobProto M_Root { get { return m_root; } }
            public MobProto M_Parent { get { return m_parent; } }
            public byte[] M_RawData { get { return __raw_data; } }
            public byte[] M_RawM_RawData { get { return __raw__raw_data; } }
        }
        public partial class MobsContainer : KaitaiStruct
        {
            public static MobsContainer FromFile(string fileName)
            {
                return new MobsContainer(new KaitaiStream(fileName));
            }

            public MobsContainer(KaitaiStream p__io, MobProto.CryptedData p__parent = null, MobProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _monsters = new List<Monster>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _monsters.Add(new Monster(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<Monster> _monsters;
            private MobProto m_root;
            private MobProto.CryptedData m_parent;
            public List<Monster> Monsters { get { return _monsters; } }
            public MobProto M_Root { get { return m_root; } }
            public MobProto.CryptedData M_Parent { get { return m_parent; } }
        }
        public partial class Header : KaitaiStruct
        {
            public static Header FromFile(string fileName)
            {
                return new Header(new KaitaiStream(fileName));
            }

            public Header(KaitaiStream p__io, MobProto p__parent = null, MobProto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 77, 77, 80, 84 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 77, 77, 80, 84 }, Magic, M_Io, "/types/header/seq/0");
                }
                _elements = m_io.ReadU4le();
                _size = m_io.ReadU4le();
            }
            private byte[] _magic;
            private uint _elements;
            private uint _size;
            private MobProto m_root;
            private MobProto m_parent;
            public byte[] Magic { get { return _magic; } }
            public uint Elements { get { return _elements; } }
            public uint Size { get { return _size; } }
            public MobProto M_Root { get { return m_root; } }
            public MobProto M_Parent { get { return m_parent; } }
        }
        private Header _fileHeader;
        private CryptedData _content;
        private MobProto m_root;
        private KaitaiStruct m_parent;
        public Header FileHeader { get { return _fileHeader; } }
        public CryptedData Content { get { return _content; } }
        public MobProto M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
