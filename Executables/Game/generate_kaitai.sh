#!/bin/sh
MY_PATH="$(dirname -- "${BASH_SOURCE[0]}")"
kaitai-struct-compiler $MY_PATH/Types/item_proto.ksy -t csharp --outdir $MY_PATH/Types/ --dotnet-namespace QuantumCore.Core.Types
kaitai-struct-compiler $MY_PATH/Types/mob_proto.ksy -t csharp --outdir $MY_PATH/Types/ --dotnet-namespace QuantumCore.Core.Types
