# Experience Table

A CSV file with information about how much experience is required per level.

* The file has only one column.
* All values must be only digits (no delimiter)
* No comments allowed
* Each line represents the exp needed to acquire the next level. The line number represents the current level
* This file implicitly defines the maximum level

# Example

```csv
300
800
1500
2500
4300
7200
11000
17000
24000
33000
```

* This would make a level 1 player require 300 experience to level up to level 2.
* This would set the max level to 12