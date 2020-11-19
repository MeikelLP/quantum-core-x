meta:
  id: item_proto
  file-extension: item_proto
  endian: le
seq:
  - id: file_header
    type: header
  - id: content
    type: data
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
  data:
    seq:
      - id: magic
        contents: 'MCOZ'
      - id: crypted_size
        type: u4
      - id: decrypted_size
        type: u4
      - id: real_size
        type: u4
      - id: compressed
        size: crypted_size
        process: lzo_xtea(real_size, crypted_size)