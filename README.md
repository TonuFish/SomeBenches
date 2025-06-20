# SomeBenches

Consolidating my random benchmarks and tangential code in one place.

🏎️ `ShiftMask` - Fold `((X >> Sc) & Mc) != 0` into `(X & (Mc << Sc)) != 0` where possible.

🏎️ `SpanSliceBounds` - Elide bounds checking when slicing spans. [dotnet/runtime #115154](https://github.com/dotnet/runtime/issues/115154)
