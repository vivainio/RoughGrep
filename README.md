# RoughGrep

Fast and minimal UI on top of RipGrep

"You RipGrep, so why not RoughGrep?" -Anon, 2018

RoughGrep is a very thin UI over RipGrep that allows comfortably browsing over the results, and launching VSCode
at the line of the hit, or event at the nesting project (e.g. the directory that has the .csproj or .fsproj file).

([RipGrep](https://github.com/BurntSushi/ripgrep), as people know, is the fastest Grep on the planet!)

## Requirements

- RipGrep (rg.exe) on PATH
- Microsoft Windows (r)
- .NET Framework 4.7.2 (because Windows Forms font rendering is broken on the old ones)
- VSCode launcher ("code") on PATH

## Installation

Grab it from [Releases](https://github.com/vivainio/RoughGrep/releases) and unzip to your path. It's two files and less than 20kb total.

## Usage

- Go to the directory you want to search from in prompt of your choice
- Run rgg.exe. You may also add ripgrep command line arguments, e.g. "rgg -g *.fs" to restrict the search to glob pattern
- Enter the search string in the box and press enter
- Navigate the results. When you find interesting result you can:
  - Press "space" to show some context in preview pane
  - Press ENTER to open it in vscode
  - Press "p" to open the parent project DIRECTORY in vscode. RoughGrep takes the best guess on what that might be.


## License

MIT


