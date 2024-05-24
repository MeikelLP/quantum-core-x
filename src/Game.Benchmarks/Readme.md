# Benchmarks

## PacketSerializer

### Serialize

| Method            | Iterations |         Mean |      Error |     StdDev |   Gen0 | Allocated |
|-------------------|------------|-------------:|-----------:|-----------:|-------:|----------:|
| Class             | 1          |     9.784 ns |  0.0355 ns |  0.0314 ns | 0.0068 |      32 B |
| ClassProperties   | 1          |     9.885 ns |  0.0152 ns |  0.0142 ns | 0.0068 |      32 B |
| Struct            | 1          |     3.662 ns |  0.0058 ns |  0.0054 ns |      - |         - |
| ReadonlyStruct    | 1          |     3.654 ns |  0.0068 ns |  0.0060 ns |      - |         - |
| ReadonlyRefStruct | 1          |     3.657 ns |  0.0061 ns |  0.0054 ns |      - |         - |
| Class             | 10         |    89.231 ns |  0.2314 ns |  0.2051 ns | 0.0679 |     320 B |
| ClassProperties   | 10         |    87.625 ns |  0.0804 ns |  0.0671 ns | 0.0679 |     320 B |
| Struct            | 10         |    35.339 ns |  0.0356 ns |  0.0333 ns |      - |         - |
| ReadonlyStruct    | 10         |    35.335 ns |  0.0371 ns |  0.0347 ns |      - |         - |
| ReadonlyRefStruct | 10         |    35.338 ns |  0.0483 ns |  0.0452 ns |      - |         - |
| Class             | 100        |   886.877 ns |  1.9000 ns |  1.7772 ns | 0.6800 |    3200 B |
| ClassProperties   | 100        |   881.239 ns |  3.0216 ns |  2.8264 ns | 0.6800 |    3200 B |
| Struct            | 100        |   358.802 ns |  0.2954 ns |  0.2618 ns |      - |         - |
| ReadonlyStruct    | 100        |   360.367 ns |  0.3464 ns |  0.2893 ns |      - |         - |
| ReadonlyRefStruct | 100        |   362.098 ns |  2.1271 ns |  1.8856 ns |      - |         - |
| Class             | 1000       | 8,874.051 ns | 14.1649 ns | 12.5568 ns | 6.7902 |   32000 B |
| ClassProperties   | 1000       | 8,769.099 ns | 12.6838 ns | 11.2439 ns | 6.7902 |   32000 B |
| Struct            | 1000       | 3,533.667 ns |  3.1104 ns |  2.7573 ns |      - |         - |
| ReadonlyStruct    | 1000       | 3,532.071 ns |  1.5524 ns |  1.3762 ns |      - |         - |
| ReadonlyRefStruct | 1000       | 3,534.124 ns |  3.0441 ns |  2.8474 ns |      - |         - |

### Deserialize

| Method            | Iterations |         Mean |       Error |      StdDev |   Gen0 | Allocated |
|-------------------|------------|-------------:|------------:|------------:|-------:|----------:|
| Class             | 1          |     6.441 ns |   0.1604 ns |   0.3488 ns | 0.0068 |      32 B |
| ClassProperties   | 1          |     7.154 ns |   0.1720 ns |   0.1981 ns | 0.0068 |      32 B |
| Struct            | 1          |     1.410 ns |   0.0132 ns |   0.0124 ns |      - |         - |
| ReadonlyStruct    | 1          |     1.400 ns |   0.0141 ns |   0.0132 ns |      - |         - |
| ReadonlyRefStruct | 1          |     1.385 ns |   0.0039 ns |   0.0033 ns |      - |         - |
| Class             | 10         |    59.462 ns |   1.2094 ns |   2.0537 ns | 0.0680 |     320 B |
| ClassProperties   | 10         |    59.133 ns |   1.2184 ns |   2.5433 ns | 0.0679 |     320 B |
| Struct            | 10         |    14.821 ns |   0.0333 ns |   0.0295 ns |      - |         - |
| ReadonlyStruct    | 10         |    15.248 ns |   0.3099 ns |   0.2899 ns |      - |         - |
| ReadonlyRefStruct | 10         |    14.818 ns |   0.0445 ns |   0.0395 ns |      - |         - |
| Class             | 100        |   592.283 ns |  11.7783 ns |  25.3541 ns | 0.6800 |    3200 B |
| ClassProperties   | 100        |   578.785 ns |  11.3603 ns |  20.1930 ns | 0.6800 |    3200 B |
| Struct            | 100        |   159.765 ns |   0.8763 ns |   0.7318 ns |      - |         - |
| ReadonlyStruct    | 100        |   160.529 ns |   0.3912 ns |   0.3659 ns |      - |         - |
| ReadonlyRefStruct | 100        |   160.168 ns |   0.2957 ns |   0.2469 ns |      - |         - |
| Class             | 1000       | 5,895.346 ns | 117.1990 ns | 273.9489 ns | 6.7978 |   32000 B |
| ClassProperties   | 1000       | 5,989.201 ns | 118.2885 ns | 269.4028 ns | 6.7978 |   32000 B |
| Struct            | 1000       | 1,546.375 ns |  10.8023 ns |  10.1045 ns |      - |         - |
| ReadonlyStruct    | 1000       | 1,552.031 ns |   3.2832 ns |   2.9105 ns |      - |         - |
| ReadonlyRefStruct | 1000       | 1,552.222 ns |   7.8820 ns |   6.5819 ns |      - |         - |

### PacketSender

#### Queue

| Method         | Packets |         Mean |      Error |     StdDev |   Gen0 |   Gen1 | Allocated |
|----------------|---------|-------------:|-----------:|-----------:|-------:|-------:|----------:|
| ObjectQueue    | 1       |     76.76 ns |   1.348 ns |   1.126 ns | 0.0050 | 0.0025 |      32 B |
| ByteArrayQueue | 1       |     40.03 ns |   0.018 ns |   0.017 ns |      - |      - |         - |
| ObjectQueue    | 10      |    767.00 ns |  14.850 ns |  15.890 ns | 0.0505 | 0.0248 |     320 B |
| ByteArrayQueue | 10      |    714.93 ns |   1.235 ns |   0.964 ns |      - |      - |         - |
| ObjectQueue    | 100     |  7,571.52 ns | 132.514 ns | 130.147 ns | 0.5035 | 0.2441 |    3200 B |
| ByteArrayQueue | 100     | 10,753.05 ns |  59.950 ns |  50.061 ns |      - |      - |         - |

#### ConcurrentQueue

| Method         | Packets |       Mean |     Error |    StdDev |   Gen0 |   Gen1 | Allocated |
|----------------|---------|-----------:|----------:|----------:|-------:|-------:|----------:|
| ObjectQueue    | 1       |   112.5 ns |   4.64 ns |  13.38 ns | 0.0956 | 0.0002 |     800 B |
| ByteArrayQueue | 1       |   137.0 ns |   4.60 ns |  13.42 ns | 0.0918 | 0.0002 |     768 B |
| ObjectQueue    | 10      |   215.7 ns |   4.34 ns |   6.50 ns | 0.1299 | 0.0007 |    1088 B |
| ByteArrayQueue | 10      |   706.0 ns |  13.88 ns |  13.63 ns | 0.0916 |      - |     768 B |
| ObjectQueue    | 100     | 1,710.2 ns |  32.51 ns |  30.41 ns | 0.9022 | 0.0362 |    7552 B |
| ByteArrayQueue | 100     | 9,843.0 ns | 161.53 ns | 151.10 ns | 0.5188 |      - |    4352 B |

## PacketReader

| Method | Packets |        Mean |       Error |      StdDev |   Gen0 | Allocated |
|--------|---------|------------:|------------:|------------:|-------:|----------:|
| Async  | 1       |    445.3 ns |    14.87 ns |    41.22 ns | 0.0362 |     304 B |
| Sync   | 1       |    134.5 ns |     3.11 ns |     8.97 ns | 0.0219 |     184 B |
| Async  | 10      |  3,683.9 ns |    73.05 ns |   157.26 ns | 0.0687 |     592 B |
| Sync   | 10      |    695.3 ns |    15.31 ns |    45.13 ns | 0.0563 |     472 B |
| Async  | 100     | 33,947.1 ns | 1,147.96 ns | 3,366.78 ns | 0.3662 |    3472 B |
| Sync   | 100     |  7,064.0 ns |   151.63 ns |   447.09 ns | 0.3967 |    3352 B |

| Method | Packets |        Mean |     Error |    StdDev |   Gen0 | Allocated |
|--------|---------|------------:|----------:|----------:|-------:|----------:|
| Async  | 1       |    452.9 ns |   7.22 ns |   6.40 ns | 0.0410 |     344 B |
| Sync   | 1       |    133.5 ns |   2.65 ns |   2.48 ns | 0.0267 |     224 B |
| Async  | 10      |  3,148.3 ns |  53.65 ns |  50.19 ns | 0.1183 |     992 B |
| Sync   | 10      |    886.3 ns |  14.62 ns |  13.68 ns | 0.1040 |     872 B |
| Async  | 100     | 30,880.5 ns | 406.49 ns | 380.23 ns | 0.8545 |    7472 B |
| Sync   | 100     |  8,375.4 ns | 165.98 ns | 335.29 ns | 0.8698 |    7352 B |

### Baseline

| Method | Packets |       Mean |     Error |    StdDev |   Gen0 | Allocated |
|--------|---------|-----------:|----------:|----------:|-------:|----------:|
| Sync   | 1       |   129.2 ns |   3.48 ns |  10.14 ns | 0.0219 |     184 B |
| Sync   | 10      |   625.8 ns |  21.43 ns |  63.18 ns | 0.0563 |     472 B |
| Sync   | 100     | 5,920.2 ns | 196.94 ns | 580.69 ns | 0.3967 |    3352 B |

### IPacketInfo + PacketInfo<T> to avoid Activator.CreateInstance

| Method    | Packets |         Mean |      Error |     StdDev |   Gen0 | Allocated |
|-----------|---------|-------------:|-----------:|-----------:|-------:|----------:|
| Sync      | 1       |    88.071 ns |  1.7986 ns |  3.3339 ns | 0.0219 |     184 B |
| Activator | 1       |     6.557 ns |  0.1282 ns |  0.1199 ns | 0.0038 |      32 B |
| Sync      | 10      |   348.693 ns |  5.1125 ns |  4.7823 ns | 0.0563 |     472 B |
| Activator | 10      |    74.979 ns |  1.5366 ns |  4.1542 ns | 0.0381 |     320 B |
| Sync      | 100     | 3,120.573 ns | 59.6362 ns | 58.5708 ns | 0.4005 |    3352 B |
| Activator | 100     |   792.730 ns | 15.8231 ns | 37.6053 ns | 0.3824 |    3200 B |

| Method      | Packets |         Mean |      Error |      StdDev |       Median |   Gen0 | Allocated |
|-------------|---------|-------------:|-----------:|------------:|-------------:|-------:|----------:|
| Sync        | 1       |    86.581 ns |  1.7525 ns |   4.3318 ns |    85.165 ns | 0.0219 |     184 B |
| Deserialize | 1       |     5.063 ns |  0.1279 ns |   0.3278 ns |     4.998 ns | 0.0038 |      32 B |
| Activator   | 1       |     7.332 ns |  0.1717 ns |   0.3697 ns |     7.249 ns | 0.0038 |      32 B |
| Sync        | 10      |   371.097 ns |  4.5024 ns |   4.2116 ns |   370.448 ns | 0.0563 |     472 B |
| Deserialize | 10      |    48.035 ns |  0.9561 ns |   1.4886 ns |    48.208 ns | 0.0382 |     320 B |
| Activator   | 10      |    71.341 ns |  1.0491 ns |   0.9813 ns |    71.444 ns | 0.0381 |     320 B |
| Sync        | 100     | 2,965.978 ns | 57.9421 ns |  54.1990 ns | 2,965.308 ns | 0.4005 |    3352 B |
| Deserialize | 100     |   447.995 ns |  8.2947 ns |  17.8551 ns |   443.709 ns | 0.3824 |    3200 B |
| Activator   | 100     |   806.666 ns | 37.2870 ns | 102.6991 ns |   797.578 ns | 0.3824 |    3200 B |
