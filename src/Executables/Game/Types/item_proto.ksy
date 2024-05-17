meta:
  id: item_proto
  file-extension: item_proto
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
        contents: 'MIPX'
      - id: version
        type: u4
      - id: stride
        type: u4
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
        process: lzo_xtea(real_size, crypted_size, 0x2A4A1, 0x45415AA, 0x185A8BE7, 0x1AAD6AB)
        type: items_container
  items_container:
    seq:
      - id: items
        type: item
        repeat: eos
  item:
    seq:
      - id: id
        type: u4
      - id: unknown
        type: u4
      - id: name
        type: strz
        encoding: EUC-KR
        size: 25
      - id: translated_name
        type: strz
        encoding: EUC-KR
        size: 25
      - id: type
        type: u1
      - id: subtype
        type: u1
      - id: unknown2
        type: u1
      - id: size
        type: u1
      - id: anti_flags
        type: u4
      - id: flags
        type: u4
      - id: wear_flags
        type: u4
      - id: immune_flags
        type: u4
      - id: buy_price
        type: u4
      - id: sell_price
        type: u4
      - id: limits
        type: item_limit
        repeat: expr
        repeat-expr: 2
      - id: applies
        type: item_apply
        repeat: expr
        repeat-expr: 3
      - id: values
        type: s4
        repeat: expr
        repeat-expr: 6
      - id: sockets
        type: s4
        repeat: expr
        repeat-expr: 3
      - id: upgrade_id
        type: u4
      - id: upgrade_set
        type: u2
      - id: magic_item_percentage
        type: u1
      - id: specular
        type: u1
      - id: socket_percentage
        type: u1
  item_limit:
    seq:
      - id: type
        type: u1
      - id: value
        type: u4
  item_apply:
    seq:
      - id: type
        type: u1
      - id: value
        type: u4
