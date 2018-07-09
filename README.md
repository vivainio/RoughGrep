# RoughGrep

Fast, brutalist UI on top of RipGrep

"You RipGrep, so why not RoughGrep?" -Anon, 2018


([RipGrep](https://github.com/BurntSushi/ripgrep), as people know, is the fastest Grep on the planet!)

![Screenshot](https://user-images.githubusercontent.com/557579/42448089-b4c3842a-8384-11e8-8a20-f1924045a522.png)

## Requirements

- RipGrep (rg.exe) on PATH
- Microsoft Windows (r)
- .NET Framework 4.7.2 (because Windows Forms font rendering is broken on the old ones)
- VSCode launcher ("code") on PATH

## Installation

Grab it from [Releases](https://github.com/vivainio/RoughGrep/releases) and unzip somewhere. Works best when you can
launch it from command line.

## Usage

- Go to the directory you want to search from in prompt of your choice.
- Run rgg.exe. You may also add ripgrep command line arguments, e.g. `rgg -g *.fs` to restrict the search to glob pattern
- Enter the search string in the box and press enter.
- Navigate the results. When you find interesting result you can:
  - Press `space` to open the file at line in preview window
  - Press `ENTER` to open it in VSCode
  - Press `p` to open the parent project DIRECTORY in vscode. RoughGrep takes the best guess on what that might be (e.g. finds .csproj).
- You can modify RipGrep command line arguments after launch by using the ComboBox from the Status Bar.
- If you want to *find file names* instead of finding contents, use the `--files` command line argument.
  Then, the "text to search for" inputs becomes the list of glob patterns instead.


## Acknowledgements

Credit for the blazing fast UI performance goes to [ScintillaNet](https://github.com/jacobslusser/ScintillaNET) editor component.

## License

MIT
