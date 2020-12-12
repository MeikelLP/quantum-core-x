meta:
  id: mob_proto
  file-extension: mob_proto
  endian: le
seq:
  - id: file_header
    type: header
  - id: content
    type: crypted_data
types:
  header:
    seq:
      - id: magic
        contents: 'MMPT'
      - id: elements
        type: u4
      - id: size
        type: u4
  crypted_data:
    seq:
      - id: magic
        contents: 'MCOZ'
      - id: crypted_size
        type: u4
      - id: decrypted_size
        type: u4
      - id: real_size
        type: u4
      - id: data
        size: crypted_size
        process: lzo_xtea(real_size, crypted_size, 0x497446, 0x4A0B, 0x86EB7, 0x68189D)
        type: mobs_container
  mobs_container:
    seq:
      - id: monsters
        type: monster
        repeat: eos
  monster:
    seq:
      - id: id
        type: u4
      - id: name
        type: str
        encoding: ascii
        size: 25
      - id: translated_name
        type: str
        encoding: ascii
        size: 25
      - id: type
        type: u1
      - id: rank
        type: u1
      - id: battle_type
        type: u1
      - id: level
        type: u1
      - id: size
        type: u1
      - id: min_gold
        type: u4
      - id: max_gold
        type: u4
      - id: experience
        type: u4
      - id: hp
        type: u4
      - id: regen_delay
        type: u1
      - id: regen_percentage
        type: u1
      - id: defence
        type: u2
      - id: ai_flag
        type: u4
      - id: race_flag
        type: u4
      - id: immune_flag
        type: u4
      - id: st
        type: u1
      - id: dx
        type: u1
      - id: ht
        type: u1
      - id: iq
        type: u1
      - id: damage_range
        type: u4
        repeat: expr
        repeat-expr: 2
      - id: attack_speed
        type: s2
      - id: move_speed
        type: s2
      - id: aggressive_pct
        type: u1
      - id: aggressive_sight
        type: u2
      - id: attack_range
        type: u2
      - id: enchantments
        type: u1
        repeat: expr
        repeat-expr: 6
      - id: resists
        type: u1
        repeat: expr
        repeat-expr: 11
      - id: resurrection_id
        type: u4
      - id: drop_item_id
        type: u4
      - id: mount_capacity
        type: u1
      - id: on_click_type
        type: u1
      - id: empire
        type: u1
      - id: folder
        type: str
        encoding: ascii
        size: 65
      - id: damage_multiply
        type: f4
      - id: summon_id
        type: u4
      - id: drain_sp
        type: u4
      - id: monster_color
        type: u4
      - id: polymorph_item_id
        type: u4
      - id: skills
        type: monster_skill
        repeat: expr
        repeat-expr: 5
      - id: berserk_point
        type: u1
      - id: stone_skin_point
        type: u1
      - id: god_speed_point
        type: u1
      - id: death_blow_point
        type: u1
      - id: revive_point
        type: u1
  monster_skill:
    seq:
      - id: id
        type: u4
      - id: level
        type: u1