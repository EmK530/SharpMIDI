# SharpMIDI

SharpMIDI is a console based MIDI player optimized to load huge MIDI files.

## Quick info

This is a project I've mainly done to try my hands on C# and become better at it, therefore you might find some parts of code that aren't quite so good. But I'd love to learn! Let me know about any issues!

## Requirements

OmniMIDI: This program only supports KDMAPI as of now.

## Build Info

This was programmed with .NET 6.0.

If you find that building does not work with the included batch file after downloading the SDK then you can try downloading the same version that this application was programmed with, 6.0.400.

## Credits

After some hefty optimization efforts on V1, I realized my code was way too bad so the way I reached the levels of optimization that V2 now has was to take inspiration from Zenith-MIDI. I clearly don't know what I'm doing to the fullest extent so this was required.

#### Resources used from Zenith-MIDI:

[BufferByteReader.cs](https://github.com/arduano/Zenith-MIDI/blob/master/BMEngine/BufferByteReader.cs)

A bit of similar loading code from [MidiTrack.cs](https://github.com/arduano/Zenith-MIDI/blob/master/BMEngine/MidiTrack.cs)

## License
[GNU GPLv3](https://choosealicense.com/licenses/gpl-3.0/)
