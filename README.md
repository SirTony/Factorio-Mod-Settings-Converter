# Factorio Mod Settings Converter

As of version 0.16.32, Factorio's `mod-settings.json` was converted to a binary format due to technical reasons.

This utility converts between JSON and the binary format to allow for easy editing outside of Factorio.

Pre-compiled binaries can be found on the [releases page](https://github.com/SirTony/Factorio-Mod-Settings-Converter/releases).

Currently only runs on Windows as it does not target .NET Core.

# Example

### From binary to JSON:

```
$ fmsc unpack -i mod-settings.dat -o mod-settings.json
```

The JSON output can also be written to STDOUT for piping into other applications. Simply omit the `-o/--output` flag.

```
$ fmsc unpack -i mod-settings.dat | wc -l
```

### From JSON to binary:

```
$ fmsc pack -i mod-settings.json -o mod-settings.dat
```

JSON can also be read from STDIN. Simply omit `-i/--input`.

```
$ cat mod-settings.json | fmsc pack -o mod-settings.dat
```
For more usage information see `$ fmsc pack --help` and `$ fmsc unpack --help`.
